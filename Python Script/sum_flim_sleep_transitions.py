"""
Build peri-transition FLIM files from sleep-score label changes.

For each observed transition type, such as Wake_to_NREM, this script writes
one multi-page .flim file. Page 0 is the average/sum at PRE_FRAMES before the
transition, the transition frame is at relative frame 0, and the final page is
POST_FRAMES after the transition.

Labels are treated as a one-column file sampled every 5 seconds, starting at
the first FLIM acquisition time. A label transition at labels[i - 1] -> labels[i]
is assigned to the nearest FLIM page at first_acq_time + i * 5 seconds.
"""

from __future__ import annotations

import bisect
import csv
import math
import sys
import tempfile
from collections import Counter
from datetime import datetime, timedelta
from pathlib import Path

import numpy as np
import tifffile

from FLIMageFileIO import FLIMTiff


DATA_DIR = Path(r"C:\path\to\your\sleep-study\data")
INPUT_FLIM = DATA_DIR / "test_concat_align_bin2.flim"
LABELS_CSV = DATA_DIR / "processed" / "labels.csv"
OUTPUT_DIR = DATA_DIR / "processed"

LABEL_INTERVAL_SECONDS = 5.0
PRE_FRAMES = 10
POST_FRAMES = 10

# True means transitions too close to the beginning/end of the FLIM file are
# skipped so every relative frame has the same number of events.
REQUIRE_FULL_WINDOW = True

# True means a transition is skipped if any other sleep-score transition falls
# inside the same PRE_FRAMES to POST_FRAMES FLIM-page window.
REQUIRE_NO_OTHER_TRANSITIONS_IN_WINDOW = False

# "sum" preserves photon counts in UInt16 FLIM files. "mean" divides by the
# number of events and rounds to UInt16, which may erase sparse FLIM bins.
OUTPUT_MODE = "sum"  # "sum" or "mean"
OUTPUT_NAME_SUFFIX = f"{OUTPUT_MODE}_isolated" if REQUIRE_NO_OTHER_TRANSITIONS_IN_WINDOW else OUTPUT_MODE

# Source files are LZW-compressed; keep derived transition files compressed.
TIFF_COMPRESSION = "lzw"

# Dense transitions are faster with one disk-backed accumulator and one source
# pass. Rare transitions are faster and lighter with one in-memory accumulator
# per relative frame.
DENSE_ACCUMULATION_PAGE_MULTIPLIER = 2.0

# None processes every observed non-self transition. Example:
# TRANSITIONS_TO_PROCESS = [(3, 2), (2, 3)]  # Wake->NREM and NREM->Wake only
TRANSITIONS_TO_PROCESS: list[tuple[int, int]] | None = None

STATE_NAMES = {
    1: "REM",
    2: "WAKE",
    3: "NREM",
}


def resolve_input_flim(path: Path) -> Path:
    if path.exists():
        return path

    fallback = path.parent / "FLIMAGE" / path.name
    if fallback.exists():
        return fallback

    raise FileNotFoundError(f"Could not find FLIM file: {path} or {fallback}")


def read_labels(path: Path) -> np.ndarray:
    labels: list[int] = []
    with path.open(newline="") as handle:
        reader = csv.reader(handle)
        for row in reader:
            if not row:
                continue
            value = row[0].strip()
            if not value:
                continue
            try:
                labels.append(int(float(value)))
            except ValueError:
                continue

    if not labels:
        raise ValueError(f"No numeric labels were found in {path}")

    unknown = sorted(set(labels) - set(STATE_NAMES))
    if unknown:
        raise ValueError(f"Unknown labels in {path}: {unknown}")

    return np.asarray(labels, dtype=np.int16)


def parse_acquired_time(header: str) -> datetime:
    for raw_line in header.split("\r\n"):
        line = raw_line.strip()
        if not line.startswith("Acquired_Time"):
            continue
        _, value = line.split("=", 1)
        return datetime.fromisoformat(value.strip().rstrip(";"))

    raise ValueError("ImageDescription does not contain Acquired_Time")


def replace_header_line(lines: list[str], prefix: str, replacement: str) -> bool:
    for index, line in enumerate(lines):
        if line.strip().startswith(prefix):
            lines[index] = replacement
            return True
    return False


def state_name(code: int) -> str:
    return STATE_NAMES.get(code, f"State{code}")


def transition_name(from_state: int, to_state: int) -> str:
    return f"{state_name(from_state)}_to_{state_name(to_state)}"


def nearest_page_index(time_seconds: float, page_seconds: list[float]) -> int:
    insertion = bisect.bisect_left(page_seconds, time_seconds)
    if insertion <= 0:
        return 0
    if insertion >= len(page_seconds):
        return len(page_seconds) - 1

    before = insertion - 1
    after = insertion
    if abs(page_seconds[before] - time_seconds) <= abs(page_seconds[after] - time_seconds):
        return before
    return after


def read_page_times_and_template(flim_path: Path) -> tuple[list[datetime], str, tuple[int, ...], np.dtype]:
    page_times: list[datetime] = []
    template_header = ""

    with tifffile.TiffFile(str(flim_path)) as tif:
        if not tif.pages:
            raise ValueError(f"No pages found in {flim_path}")

        first_page = tif.pages[0]
        page_shape = first_page.shape
        page_dtype = first_page.dtype

        for page_index, page in enumerate(tif.pages):
            header_tag = page.tags.get("ImageDescription")
            if header_tag is None:
                raise ValueError(f"Page {page_index} has no ImageDescription")

            header = str(header_tag.value)
            if page_index == 0:
                template_header = header
            page_times.append(parse_acquired_time(header))

    return page_times, template_header, page_shape, np.dtype(page_dtype)


def find_transition_events(
    labels: np.ndarray,
    page_times: list[datetime],
) -> list[dict[str, int | float | bool | str]]:
    start_time = page_times[0]
    page_seconds = [(t - start_time).total_seconds() for t in page_times]
    events: list[dict[str, int | float | bool | str]] = []

    for label_index in range(1, len(labels)):
        from_state = int(labels[label_index - 1])
        to_state = int(labels[label_index])
        if from_state == to_state:
            continue

        transition_seconds = label_index * LABEL_INTERVAL_SECONDS
        page_index = nearest_page_index(transition_seconds, page_seconds)

        events.append(
            {
                "label_index": label_index,
                "from_state": from_state,
                "to_state": to_state,
                "transition_seconds": transition_seconds,
                "page_index": page_index,
                "included": True,
                "skip_reason": "",
                "overlap_transition_count": 0,
            }
        )

    transition_pages = [int(event["page_index"]) for event in events]

    for event_index, event in enumerate(events):
        page_index = int(event["page_index"])
        window_start = page_index - PRE_FRAMES
        window_end = page_index + POST_FRAMES
        reasons: list[str] = []

        full_window = window_start >= 0 and window_end < len(page_times)
        if REQUIRE_FULL_WINDOW and not full_window:
            reasons.append("edge_window")

        if REQUIRE_NO_OTHER_TRANSITIONS_IN_WINDOW:
            overlap_count = 0
            for other_index, other_page_index in enumerate(transition_pages):
                if other_index == event_index:
                    continue
                if window_start <= other_page_index <= window_end:
                    overlap_count += 1

            event["overlap_transition_count"] = overlap_count
            if overlap_count > 0:
                reasons.append("overlap_transition")

        event["included"] = len(reasons) == 0
        event["skip_reason"] = ";".join(reasons)

    return events


def accumulator_dtype(event_count: int) -> np.dtype:
    max_possible = event_count * int(np.iinfo(np.uint16).max)
    if max_possible <= int(np.iinfo(np.uint32).max):
        return np.dtype(np.uint32)
    return np.dtype(np.uint64)


def build_page_targets(
    events: list[dict[str, int | float | bool | str]],
    page_count: int,
    relative_frames: list[int],
) -> tuple[list[list[int]], np.ndarray]:
    page_targets: list[list[int]] = [[] for _ in range(page_count)]
    counts = np.zeros(len(relative_frames), dtype=np.int64)

    for event in events:
        center_page = int(event["page_index"])
        for relative_index, relative_frame in enumerate(relative_frames):
            page_index = center_page + relative_frame
            if 0 <= page_index < page_count:
                page_targets[page_index].append(relative_index)
                counts[relative_index] += 1

    return page_targets, counts


def build_transition_header(
    template_header: str,
    output_path: Path,
    acquired_time: datetime,
    from_state: int,
    to_state: int,
    relative_frame: int,
    relative_seconds: float,
    observed_events: int,
    included_events: int,
    source_events: int,
    clipped_bins: int,
) -> str:
    lines = template_header.split("\r\n")
    transition = transition_name(from_state, to_state)

    replace_header_line(
        lines,
        "Acquired_Time",
        f"Acquired_Time = {acquired_time.strftime('%Y-%m-%dT%H:%M:%S.%f')[:-3]};",
    )
    replace_header_line(
        lines,
        "State.Acq.nFrames",
        f"State.Acq.nFrames = {PRE_FRAMES + POST_FRAMES + 1};",
    )
    replace_header_line(
        lines,
        "State.Files.fileName",
        f'State.Files.fileName = "{output_path.stem}";',
    )

    lines.extend(
        [
            f"Transition_Name = {transition};",
            f"Transition_From_Label = {from_state};",
            f"Transition_From_Name = {state_name(from_state)};",
            f"Transition_To_Label = {to_state};",
            f"Transition_To_Name = {state_name(to_state)};",
            f"Transition_RelativeFrame = {relative_frame};",
            f"Transition_RelativeSeconds = {relative_seconds:.6f};",
            f"Transition_OutputMode = {OUTPUT_MODE};",
            f"Transition_RequireIsolatedWindow = {REQUIRE_NO_OTHER_TRANSITIONS_IN_WINDOW};",
            f"Transition_ObservedEvents = {observed_events};",
            f"Transition_IncludedEvents = {included_events};",
            f"Transition_SourceEventsForPage = {source_events};",
            f"Transition_ClippedBinsForPage = {clipped_bins};",
        ]
    )

    return "\r\n".join(lines)


def output_page(accumulated: np.ndarray, source_count: int) -> tuple[np.ndarray, int]:
    if source_count <= 0:
        return np.zeros(accumulated.shape, dtype=np.uint16), 0

    if OUTPUT_MODE == "mean":
        values = np.rint(accumulated / source_count)
    elif OUTPUT_MODE == "sum":
        values = accumulated
    else:
        raise ValueError(f"Unsupported OUTPUT_MODE: {OUTPUT_MODE}")

    clipped_bins = int(np.count_nonzero(values > np.iinfo(np.uint16).max))
    values = np.clip(values, 0, np.iinfo(np.uint16).max)
    return values.astype(np.uint16), clipped_bins


def write_transition_flim(
    output_path: Path,
    accumulator: np.memmap,
    counts: np.ndarray,
    template_header: str,
    page_times: list[datetime],
    first_event_page: int,
    from_state: int,
    to_state: int,
    relative_frames: list[int],
    observed_events: int,
    included_events: int,
    median_frame_seconds: float,
) -> tuple[int, int]:
    total_clipped_bins = 0
    written_pages = 0
    output_path.parent.mkdir(parents=True, exist_ok=True)

    with tifffile.TiffWriter(str(output_path), bigtiff=True) as tif:
        for relative_index, relative_frame in enumerate(relative_frames):
            source_count = int(counts[relative_index])
            image, clipped_bins = output_page(accumulator[relative_index], source_count)
            total_clipped_bins += clipped_bins

            acquired_time = page_times[first_event_page] + timedelta(
                seconds=relative_frame * median_frame_seconds
            )
            header = build_transition_header(
                template_header,
                output_path,
                acquired_time,
                from_state,
                to_state,
                relative_frame,
                relative_frame * median_frame_seconds,
                observed_events,
                included_events,
                source_count,
                clipped_bins,
            )

            tif.write(
                image,
                photometric="minisblack",
                metadata=None,
                description=header,
                compression=TIFF_COMPRESSION,
            )
            written_pages += 1

    return written_pages, total_clipped_bins


def write_sparse_transition_flim(
    flim_path: Path,
    output_path: Path,
    included_events: list[dict[str, int | float | bool | str]],
    counts: np.ndarray,
    template_header: str,
    page_times: list[datetime],
    page_shape: tuple[int, ...],
    from_state: int,
    to_state: int,
    relative_frames: list[int],
    observed_events: int,
    median_frame_seconds: float,
) -> tuple[int, int]:
    total_clipped_bins = 0
    written_pages = 0
    acc_dtype = accumulator_dtype(len(included_events))
    first_event_page = int(included_events[0]["page_index"])
    output_path.parent.mkdir(parents=True, exist_ok=True)

    with tifffile.TiffFile(str(flim_path)) as tif, tifffile.TiffWriter(str(output_path), bigtiff=True) as writer:
        for relative_index, relative_frame in enumerate(relative_frames):
            accumulator = np.zeros(page_shape, dtype=acc_dtype)
            source_count = 0

            for event in included_events:
                page_index = int(event["page_index"]) + relative_frame
                if not 0 <= page_index < len(tif.pages):
                    continue

                image = tif.pages[page_index].asarray()
                np.add(accumulator, image, out=accumulator, casting="unsafe")
                source_count += 1

            expected_count = int(counts[relative_index])
            if source_count != expected_count:
                raise RuntimeError(
                    f"Internal count mismatch for {transition_name(from_state, to_state)} "
                    f"frame {relative_frame}: accumulated {source_count}, expected {expected_count}"
                )

            image, clipped_bins = output_page(accumulator, source_count)
            total_clipped_bins += clipped_bins
            acquired_time = page_times[first_event_page] + timedelta(
                seconds=relative_frame * median_frame_seconds
            )
            header = build_transition_header(
                template_header,
                output_path,
                acquired_time,
                from_state,
                to_state,
                relative_frame,
                relative_frame * median_frame_seconds,
                observed_events,
                len(included_events),
                source_count,
                clipped_bins,
            )

            writer.write(
                image,
                photometric="minisblack",
                metadata=None,
                description=header,
                compression=TIFF_COMPRESSION,
            )
            written_pages += 1

    return written_pages, total_clipped_bins


def write_summary_csv(summary_rows: list[dict[str, object]], output_path: Path) -> None:
    if not summary_rows:
        return

    fieldnames = list(summary_rows[0].keys())
    with output_path.open("w", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(summary_rows)


def main() -> None:
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(line_buffering=True)

    flim_path = resolve_input_flim(INPUT_FLIM)
    labels = read_labels(LABELS_CSV)
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    metadata = FLIMTiff().read(str(flim_path), read_image=False)
    page_times, template_header, page_shape, page_dtype = read_page_times_and_template(flim_path)
    if page_dtype != np.dtype(np.uint16):
        raise ValueError(f"Expected uint16 FLIM pages, got {page_dtype}")

    page_seconds = np.array([(t - page_times[0]).total_seconds() for t in page_times], dtype=float)
    median_frame_seconds = float(np.median(np.diff(page_seconds))) if len(page_seconds) > 1 else 0.0

    all_events = find_transition_events(labels, page_times)
    observed_counts = Counter((int(e["from_state"]), int(e["to_state"])) for e in all_events)

    if TRANSITIONS_TO_PROCESS is None:
        transition_keys = sorted(observed_counts, key=lambda item: (state_name(item[0]), state_name(item[1])))
    else:
        transition_keys = TRANSITIONS_TO_PROCESS

    relative_frames = list(range(-PRE_FRAMES, POST_FRAMES + 1))
    summary_rows: list[dict[str, object]] = []

    print(f"Input FLIM: {flim_path}")
    print(f"Pages: {metadata.n_images}, channels: {metadata.nChannels}, n_time: {metadata.n_time}")
    print(f"Labels: {len(labels)} rows at {LABEL_INTERVAL_SECONDS:g} s intervals")
    print(f"Transitions observed: {len(all_events)}")
    print(
        f"Window: {relative_frames[0]} to {relative_frames[-1]} FLIM frames "
        f"({len(relative_frames)} output pages per transition)"
    )
    print(f"Output mode: {OUTPUT_MODE}")
    print(f"Require isolated transition window: {REQUIRE_NO_OTHER_TRANSITIONS_IN_WINDOW}")
    print(f"Output suffix: {OUTPUT_NAME_SUFFIX}")
    print("")

    for from_state, to_state in transition_keys:
        transition = transition_name(from_state, to_state)
        observed_events = [
            event for event in all_events
            if int(event["from_state"]) == from_state and int(event["to_state"]) == to_state
        ]
        included_events = [event for event in observed_events if bool(event["included"])]

        if not observed_events:
            print(f"{transition}: no observed events, skipping")
            continue
        skipped_reasons = Counter(
            str(event["skip_reason"])
            for event in observed_events
            if not bool(event["included"])
        )
        if not included_events:
            print(f"{transition}: no events with usable windows, skipping")
            if skipped_reasons:
                print(f"  skipped reasons: {dict(skipped_reasons)}")
            continue

        print(
            f"{transition}: {len(included_events)}/{len(observed_events)} events included "
            f"({len(observed_events) - len(included_events)} skipped)"
        )
        if skipped_reasons:
            print(f"  skipped reasons: {dict(skipped_reasons)}")

        page_targets, counts = build_page_targets(included_events, len(page_times), relative_frames)
        sample_pages = len(included_events) * len(relative_frames)
        dense_threshold = len(page_times) * DENSE_ACCUMULATION_PAGE_MULTIPLIER
        use_dense = sample_pages >= dense_threshold
        strategy = "dense_memmap" if use_dense else "sparse_per_offset"
        output_path = OUTPUT_DIR / f"{flim_path.stem}_transition_{transition}_{OUTPUT_NAME_SUFFIX}.flim"

        print(f"  strategy: {strategy}")

        if use_dense:
            acc_dtype = accumulator_dtype(int(counts.max(initial=0)))
            temp_path = Path(tempfile.gettempdir()) / (
                f"{flim_path.stem}_{transition}_transition_accumulator.dat"
            )
            if temp_path.exists():
                temp_path.unlink()

            accumulator = np.memmap(
                temp_path,
                dtype=acc_dtype,
                mode="w+",
                shape=(len(relative_frames),) + tuple(page_shape),
            )
            accumulator[:] = 0
            accumulator.flush()

            try:
                used_pages = 0
                with tifffile.TiffFile(str(flim_path)) as tif:
                    for page_index, page in enumerate(tif.pages):
                        targets = page_targets[page_index]
                        if not targets:
                            continue

                        used_pages += 1
                        image = page.asarray()
                        for relative_index in targets:
                            np.add(
                                accumulator[relative_index],
                                image,
                                out=accumulator[relative_index],
                                casting="unsafe",
                            )

                        if used_pages % 250 == 0:
                            print(f"  {transition}: accumulated {used_pages} source pages")

                accumulator.flush()

                first_event_page = int(included_events[0]["page_index"])
                written_pages, total_clipped_bins = write_transition_flim(
                    output_path,
                    accumulator,
                    counts,
                    template_header,
                    page_times,
                    first_event_page,
                    from_state,
                    to_state,
                    relative_frames,
                    len(observed_events),
                    len(included_events),
                    median_frame_seconds,
                )
            finally:
                del accumulator
                if temp_path.exists():
                    temp_path.unlink()
        else:
            written_pages, total_clipped_bins = write_sparse_transition_flim(
                flim_path,
                output_path,
                included_events,
                counts,
                template_header,
                page_times,
                tuple(page_shape),
                from_state,
                to_state,
                relative_frames,
                len(observed_events),
                median_frame_seconds,
            )

        print(
            f"  wrote {written_pages} pages -> {output_path} "
            f"(clipped_bins={total_clipped_bins})"
        )

        for relative_index, relative_frame in enumerate(relative_frames):
            summary_rows.append(
                {
                    "transition": transition,
                    "from_label": from_state,
                    "from_name": state_name(from_state),
                    "to_label": to_state,
                    "to_name": state_name(to_state),
                    "observed_events": len(observed_events),
                    "included_events": len(included_events),
                    "skipped_events": len(observed_events) - len(included_events),
                    "relative_frame": relative_frame,
                    "relative_seconds": f"{relative_frame * median_frame_seconds:.6f}",
                    "source_events_for_page": int(counts[relative_index]),
                    "output_mode": OUTPUT_MODE,
                    "output_name_suffix": OUTPUT_NAME_SUFFIX,
                    "accumulation_strategy": strategy,
                    "output_file": str(output_path),
                }
            )

    if not summary_rows:
        print("")
        print("No transition files were written because no events passed the filters.")
        return

    summary_path = OUTPUT_DIR / f"{flim_path.stem}_transition_{OUTPUT_NAME_SUFFIX}_summary.csv"
    write_summary_csv(summary_rows, summary_path)
    print("")
    print(f"Summary CSV: {summary_path}")


if __name__ == "__main__":
    main()
