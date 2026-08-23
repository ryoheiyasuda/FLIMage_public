"""
Sum FLIM pages by sleep-score labels and write one .flim per state.

The labels file is treated as a one-column list sampled every 5 seconds,
starting at the first FLIM acquisition time. Each FLIM page is assigned to
the nearest label time point.
"""

from __future__ import annotations

import csv
import math
import sys
from datetime import datetime
from pathlib import Path

import numpy as np
import tifffile

from FLIMageFileIO import FLIMTiff


DATA_DIR = Path(r"C:\path\to\your\sleep-study\data")
INPUT_FLIM = DATA_DIR / "test_concat_align_bin2.flim"
LABELS_CSV = DATA_DIR / "processed" / "labels.csv"
OUTPUT_DIR = DATA_DIR / "processed"

LABEL_INTERVAL_SECONDS = 5.0

STATE_NAMES = {
    1: "REM",
    2: "WAKE",
    3: "NREM",
}

# FLIMage stores FLIM pages as UInt16. Clipping is safer than wrapping if a
# summed bin exceeds UInt16.
CLIP_TO_UINT16 = True


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
                # Allows a header row if one is later added.
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
        value = value.strip().rstrip(";")
        return datetime.fromisoformat(value)

    raise ValueError("ImageDescription does not contain Acquired_Time")


def nearest_label_index(acquired_time: datetime, start_time: datetime) -> int:
    elapsed = (acquired_time - start_time).total_seconds()
    return int(math.floor(elapsed / LABEL_INTERVAL_SECONDS + 0.5))


def replace_header_line(lines: list[str], prefix: str, replacement: str) -> bool:
    for index, line in enumerate(lines):
        if line.strip().startswith(prefix):
            lines[index] = replacement
            return True
    return False


def build_output_header(
    template_header: str,
    output_path: Path,
    acquired_time: datetime,
    state_code: int,
    state_name: str,
    source_pages: int,
    unclipped_max: int,
    clipped_bins: int,
) -> str:
    lines = template_header.split("\r\n")

    replace_header_line(
        lines,
        "Acquired_Time",
        f"Acquired_Time = {acquired_time.strftime('%Y-%m-%dT%H:%M:%S.%f')[:-3]};",
    )
    replace_header_line(lines, "State.Acq.nFrames", "State.Acq.nFrames = 1;")
    replace_header_line(
        lines,
        "State.Files.fileName",
        f'State.Files.fileName = "{output_path.stem}";',
    )

    lines.extend(
        [
            f"SleepScore_Label = {state_code};",
            f"SleepScore_Name = {state_name};",
            f"SleepScore_SourcePages = {source_pages};",
            f"SleepScore_MaxBeforeUInt16Clip = {unclipped_max};",
            f"SleepScore_ClippedBins = {clipped_bins};",
        ]
    )

    return "\r\n".join(lines)


def write_single_page_flim(output_path: Path, image: np.ndarray, header: str) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with tifffile.TiffWriter(str(output_path), bigtiff=True) as tif:
        tif.write(
            image,
            photometric="minisblack",
            metadata=None,
            description=header,
        )


def main() -> None:
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(line_buffering=True)

    flim_path = resolve_input_flim(INPUT_FLIM)
    labels = read_labels(LABELS_CSV)
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    # Use FLIMageFileIO for metadata validation without loading the image data.
    metadata = FLIMTiff().read(str(flim_path), read_image=False)
    print(f"Input FLIM: {flim_path}")
    print(f"Pages: {metadata.n_images}, channels: {metadata.nChannels}, n_time: {metadata.n_time}")
    print(f"Labels: {len(labels)} rows at {LABEL_INTERVAL_SECONDS:g} s intervals")

    accumulators: dict[int, np.ndarray] = {}
    source_counts = {state: 0 for state in STATE_NAMES}
    first_times: dict[int, datetime] = {}
    clamped_before = 0
    clamped_after = 0
    template_header = ""
    start_time: datetime | None = None

    with tifffile.TiffFile(str(flim_path)) as tif:
        for page_index, page in enumerate(tif.pages):
            header_tag = page.tags.get("ImageDescription")
            if header_tag is None:
                raise ValueError(f"Page {page_index} has no ImageDescription")

            header = str(header_tag.value)
            if page_index == 0:
                template_header = header

            acquired_time = parse_acquired_time(header)
            if start_time is None:
                start_time = acquired_time

            label_index = nearest_label_index(acquired_time, start_time)
            if label_index < 0:
                label_index = 0
                clamped_before += 1
            elif label_index >= len(labels):
                label_index = len(labels) - 1
                clamped_after += 1

            state = int(labels[label_index])
            if state not in accumulators:
                accumulators[state] = np.zeros(page.shape, dtype=np.uint64)
                first_times[state] = acquired_time

            image = page.asarray()
            np.add(accumulators[state], image, out=accumulators[state], casting="unsafe")
            source_counts[state] += 1

            if (page_index + 1) % 250 == 0 or page_index + 1 == len(tif.pages):
                print(f"Processed {page_index + 1}/{len(tif.pages)} pages")

    if start_time is None:
        raise ValueError(f"No pages found in {flim_path}")

    print("")
    print("Writing summed FLIM files:")
    for state, state_name in STATE_NAMES.items():
        count = source_counts[state]
        if count == 0:
            print(f"  {state_name}: no assigned pages, skipping")
            continue

        accumulator = accumulators[state]
        unclipped_max = int(accumulator.max())
        clipped_bins = int(np.count_nonzero(accumulator > np.iinfo(np.uint16).max))

        if CLIP_TO_UINT16:
            output_image = np.clip(accumulator, 0, np.iinfo(np.uint16).max).astype(np.uint16)
        else:
            output_image = accumulator.astype(np.uint16)

        output_path = OUTPUT_DIR / f"{flim_path.stem}_{state_name}_sum.flim"
        header = build_output_header(
            template_header,
            output_path,
            first_times[state],
            state,
            state_name,
            count,
            unclipped_max,
            clipped_bins,
        )
        write_single_page_flim(output_path, output_image, header)
        print(
            f"  {state_name}: {count} pages -> {output_path} "
            f"(max={unclipped_max}, clipped_bins={clipped_bins})"
        )

    if clamped_before or clamped_after:
        print("")
        print(
            "Note: clamped label assignment for "
            f"{clamped_before} page(s) before the label table and "
            f"{clamped_after} page(s) after the label table."
        )


if __name__ == "__main__":
    main()
