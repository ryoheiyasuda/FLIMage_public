"""Convert FLIMage photon archives (.photon) into FLIMage .flim files.

This script supports two conversion modes:

1. Default native mode: Directly binds to TCSPC_Decode.dll via ctypes.
   - Requires: TCSPC_Decode.dll (64-bit native DLL)
   - Requires Python packages: tifffile, FLIMageFileIO (install via pip)

2. Benchmark/C# mode (--benchmark / --csharp): Uses PhotonFileBenchmark.exe
   to perform the conversion through the FLIMage benchmark app.
   - Requires: PhotonFileBenchmark.exe (64-bit .NET Framework 4.8 executable)
   - The script can build this executable from source if the FLIMage repository is available.

Usage:
    python photon_to_flim.py --input <path/to/file.photon|folder> [--output <outdir>] [--benchmark]

If `--input` is a single `.photon` file, the script converts that file only.
If `--input` is a folder, the script recursively converts all `.photon` files
found under that folder.

When benchmark mode is selected, the script will search for
PhotonFileBenchmark.exe under the repository and, if necessary, attempt to
build it using `dotnet build`.

Note: This script assumes a Windows environment and requires the .NET build tools
or Visual Studio with .NET Framework support for building the executable.
"""

from __future__ import annotations

import argparse
import ctypes
import os
import shutil
import subprocess
import sys
import tarfile
import threading
import time
import zipfile
from pathlib import Path
from typing import Iterable, List, Optional, Tuple

import numpy as np

# FLIMageFileIO depends on tifffile; only import it when needed to avoid requiring it for the
# non-native (C#) converter path.
FLIMTiff = None


def find_photon_files(path: Path) -> List[Path]:
    if path.is_file():
        return [path]
    if path.is_dir():
        return sorted(path.rglob("*.photon"))
    raise FileNotFoundError(f"Input path not found: {path}")


def find_photon_benchmark_exe(repo_root: Path) -> Optional[Path]:
    # Common output locations for the PhotonFileBenchmark project
    # Prefer 64-bit builds since TCSPC_Decode.dll is 64-bit
    candidates = [
        repo_root / "FLIMage" / "PhotonFileBenchmark" / "bin" / "Release" / "net48" / "win-x64" / "PhotonFileBenchmark.exe",
        repo_root / "FLIMage" / "PhotonFileBenchmark" / "bin" / "Debug" / "net48" / "win-x64" / "PhotonFileBenchmark.exe",
        repo_root / "FLIMage" / "PhotonFileBenchmark" / "bin" / "Release" / "net48" / "PhotonFileBenchmark.exe",
        repo_root / "FLIMage" / "PhotonFileBenchmark" / "bin" / "Debug" / "net48" / "PhotonFileBenchmark.exe",
        repo_root / "FLIMage" / "PhotonFileBenchmark" / "bin" / "Debug" / "PhotonFileBenchmark.exe",
        repo_root / "FLIMage" / "PhotonFileBenchmark" / "bin" / "Release" / "PhotonFileBenchmark.exe",
    ]

    for candidate in candidates:
        if candidate.exists():
            return candidate
    return None


def build_photon_benchmark(repo_root: Path, configuration: str = "Release") -> Optional[Path]:
    csproj = repo_root / "FLIMage" / "PhotonFileBenchmark" / "PhotonFileBenchmark.csproj"
    if not csproj.exists():
        raise FileNotFoundError(f"Could not find PhotonFileBenchmark.csproj at {csproj}")

    print("Building PhotonFileBenchmark project...")

    # Prefer dotnet build (works when .NET SDK is installed).
    # Fallback to msbuild if dotnet is not available.
    build_cmd = None

    def run_cmd(cmd: List[str]) -> int:
        print("  ", " ".join(cmd))
        try:
            res = subprocess.run(cmd, check=False)
            return res.returncode
        except FileNotFoundError:
            return -1

    ret = run_cmd(["dotnet", "--version"])
    if ret == 0:
        # Build for 64-bit since TCSPC_Decode.dll is 64-bit
        build_cmd = ["dotnet", "build", str(csproj), "-c", configuration, "-r", "win-x64"]
    else:
        # Try msbuild
        ret = run_cmd(["msbuild", "/version"])
        if ret == 0:
            build_cmd = ["msbuild", str(csproj), "/p:Configuration=" + configuration, "/p:Platform=x64"]

    if build_cmd is None:
        print("Unable to find 'dotnet' or 'msbuild' on PATH; cannot build PhotonFileBenchmark.")
        return None

    ret = run_cmd(build_cmd)
    if ret != 0:
        print("Build failed.")
        return None

    # Re-check for output exe
    exe = find_photon_benchmark_exe(repo_root)
    if exe is None:
        print("Build completed but PhotonFileBenchmark.exe not found in expected locations.")
    return exe


def _normalize_benchmark_output(out_file: Path, preexisting: set[Path]) -> None:
    if out_file.exists():
        return

    candidates = sorted(
        (p for p in out_file.parent.glob(f"{out_file.stem}*.{out_file.suffix.lstrip('.')}") if p not in preexisting),
        key=lambda p: p.stat().st_mtime,
        reverse=True,
    )
    if not candidates:
        return

    candidate = candidates[0]
    if candidate.resolve() != out_file.resolve():
        if out_file.exists():
            out_file.unlink()
        shutil.move(str(candidate), str(out_file))


def run_photon_benchmark(
    exe: Path,
    photon_file: Path,
    out_file: Path,
    extra_args: List[str],
    aggregate: bool = False,
) -> int:
    preexisting = set(out_file.parent.glob(f"{out_file.stem}*.{out_file.suffix.lstrip('.')}"))
    cmd = [str(exe), str(photon_file), "--decode-flim", "--flim-output", str(out_file)]
    if aggregate:
        cmd.append("--flim-aggregate")
    if extra_args:
        cmd += extra_args

    print(f"Converting: {photon_file} -> {out_file}")
    res = subprocess.run(cmd)
    if res.returncode == 0:
        _normalize_benchmark_output(out_file, preexisting)
    return res.returncode


# -----------------------------------------------------------------------------
# Native decoder via TCSPC_Decode.dll (no C# needed)
# -----------------------------------------------------------------------------

class CompID(ctypes.Structure):
    _pack_ = 1
    _fields_ = [
        ("compID", ctypes.c_int),
        ("FLIMID", ctypes.c_int),
    ]


class DEParameters(ctypes.Structure):
    _pack_ = 1
    # Matches the C# DE_parameters struct layout in TCSPC_Native_Dynamic.cs
    _fields_ = [
        ("n_average", ctypes.c_int),
        ("resolution", ctypes.c_double),
        ("binning", ctypes.c_int),
        ("enableFastZscan", ctypes.c_int),
        ("nZlocs", ctypes.c_int),
        ("nLines", ctypes.c_int),
        ("nPixels", ctypes.c_int),
        ("nFrames", ctypes.c_int),
        ("nChannels", ctypes.c_int),
        ("BiDirectionalScanX", ctypes.c_int),
        ("BiDirectionalScanY", ctypes.c_int),
        ("TagID", ctypes.c_int),
        ("LineID", ctypes.c_int),
        ("FrameID", ctypes.c_int),
        ("skipFirstLines", ctypes.c_int),
        ("skipFirstFrames", ctypes.c_int),
        ("pixel_binning", ctypes.c_int),
        ("acqType", ctypes.c_int),
        ("acq_modePQ", ctypes.c_int),
        ("time_per_unit", ctypes.c_double),
        ("pixel_time", ctypes.c_double),
        ("line_time_correction", ctypes.c_double),
        ("msPerLine", ctypes.c_double),
        ("nDtime0", ctypes.c_int),
        ("nDtime1", ctypes.c_int),
        ("nDtime2", ctypes.c_int),
        ("nDtime3", ctypes.c_int),
        ("nStartPoint", ctypes.c_int),
        ("nEndPoint", ctypes.c_int),
        ("acquisition0", ctypes.c_int),
        ("acquisition1", ctypes.c_int),
        ("acquisition2", ctypes.c_int),
        ("acquisition3", ctypes.c_int),
        ("acquireFLIM0", ctypes.c_int),
        ("acquireFLIM1", ctypes.c_int),
        ("acquireFLIM2", ctypes.c_int),
        ("acquireFLIM3", ctypes.c_int),
        ("aveFrame0", ctypes.c_int),
        ("aveFrame1", ctypes.c_int),
        ("aveFrame2", ctypes.c_int),
        ("aveFrame3", ctypes.c_int),
        ("focus", ctypes.c_int),
        ("StripeDuringFocus", ctypes.c_int),
        ("LinesPerStripe", ctypes.c_int),
        ("AcquisitionDelay", ctypes.c_double),
        ("BiDirectionalDelay", ctypes.c_double),
        ("eraseMemory0", ctypes.c_int),
        ("eraseMemory1", ctypes.c_int),
        ("eraseMemory2", ctypes.c_int),
        ("eraseMemory3", ctypes.c_int),
        ("savePhotonsInFile", ctypes.c_int),
        ("readFromPhotonFile", ctypes.c_int),
        ("lineClockDivision", ctypes.c_int),
        ("fastZ_measureTagParameters", ctypes.c_int),
        ("fastZ_FrequencyKHz", ctypes.c_double),
        ("fastZ_ZScanPerPixel", ctypes.c_float),
        ("fastZ_ZScanPerPixel_Bidirecitonal", ctypes.c_uint),
        ("fastZ_XYFillFraction", ctypes.c_double),
        ("fastZ_VoxelTimeUs", ctypes.c_double),
        ("fastZ_ZScanPerLine", ctypes.c_int),
        ("fastZ_nFastZSlices", ctypes.c_int),
        ("fastZ_VoxelCount", ctypes.c_int),
        ("fastZ_phaseRangeStart", ctypes.c_double),
        ("fastZ_phaseRangeEnd", ctypes.c_double),
        ("fastZ_phaseRangeCountStart", ctypes.c_uint),
        ("fastZ_phaseRangeCountEnd", ctypes.c_uint),
        ("fastZ_phase_detection_mode", ctypes.c_int),
        ("fastZ_CountPerFastZCycle", ctypes.c_uint),  # Count
        ("fastZ_CountPerFastZCycleHalf", ctypes.c_uint),
        ("fastZ_CountPerFastZSlice", ctypes.c_uint),
        ("fastZ_residual_for_PhaseDetection", ctypes.c_uint),
        ("bundle_all_channels", ctypes.c_int),
        ("debug", ctypes.c_int),
        # Fiber photometry (single point) mode
        ("fiberPhotometryMode", ctypes.c_int),  # 0/1
        ("fiberBin_ms", ctypes.c_double),  # bin width in ms (default 20)
        ("fiberStartMode", ctypes.c_int),  # 0 = soft (first photon), 1 = external marker (FrameID)
        ("lineScanMode", ctypes.c_int),  # 0/1
    ]


class TCSPCDecoder:
    """ctypes wrapper that follows the same file-decoding path as FLIMage."""

    CALLBACK_TYPE = ctypes.CFUNCTYPE(None, ctypes.c_int, ctypes.c_char_p, ctypes.c_int)

    def __init__(self, dll_path: Path):
        self.dll_path = dll_path
        self.dll = ctypes.CDLL(str(dll_path))
        self._setup_function_signatures()
        self.callback = None
        self.frame_messages: List[str] = []
        self.frames: List[list] = []
        self.params: Optional[DEParameters] = None
        self.measurement_done = threading.Event()
        self.last_captured_frame = 0
        self.last_reported_frame = 0
        self.callback_error: Optional[Exception] = None
        self._photon_binary: Optional[np.ndarray] = None

    def _setup_function_signatures(self):
        self.dll.Start_TCSPC_Decode.argtypes = [
            ctypes.c_int,
            self.CALLBACK_TYPE,
            ctypes.POINTER(DEParameters),
            ctypes.POINTER(CompID),
            ctypes.c_char_p,
        ]
        self.dll.Start_TCSPC_Decode.restype = ctypes.c_int
        self.dll.Setup_Photon_Data_Binary.argtypes = [ctypes.c_int, ctypes.c_void_p, ctypes.c_int]
        self.dll.Setup_Photon_Data_Binary.restype = ctypes.c_int
        self.dll.Start_Measurement.argtypes = [ctypes.c_int, ctypes.POINTER(DEParameters)]
        self.dll.Start_Measurement.restype = ctypes.c_int
        self.dll.Stop_Measurement.argtypes = [ctypes.c_int, ctypes.c_int]
        self.dll.Stop_Measurement.restype = ctypes.c_int
        self.dll.Close_Device.argtypes = [ctypes.c_int]
        self.dll.Close_Device.restype = ctypes.c_int
        self.dll.DE_GetFrameCount.argtypes = [ctypes.c_int]
        self.dll.DE_GetFrameCount.restype = ctypes.c_int
        self.dll.DE_GetLineCount.argtypes = [ctypes.c_int]
        self.dll.DE_GetLineCount.restype = ctypes.c_int
        self.dll.DE_GetPhotonCount.argtypes = [ctypes.c_int]
        self.dll.DE_GetPhotonCount.restype = ctypes.c_long

        # Get decoded histogram data (per channel/z)
        self.dll.DE_GetData.argtypes = [ctypes.c_int, ctypes.POINTER(ctypes.c_ushort), ctypes.c_int, ctypes.c_int]
        self.dll.DE_GetData.restype = ctypes.c_int

    def _make_callback(self):
        @self.CALLBACK_TYPE
        def _cb(id, msg, frame):
            try:
                msg_str = msg.decode("ascii", errors="ignore") if msg else ""
            except Exception:
                msg_str = ""
            self.frame_messages.append(f"Frame {frame}: {msg_str}")
            try:
                self._handle_callback_message(msg_str, frame)
            except Exception as exc:
                self.callback_error = exc
                self.measurement_done.set()

        self.callback = _cb
        return self.callback

    def _handle_callback_message(self, msg_str: str, frame: int) -> None:
        if msg_str.startswith("FrameDone"):
            self.last_reported_frame = max(self.last_reported_frame, frame)
            self._capture_frame(frame)
        elif msg_str.startswith("MeasurementDone"):
            self.last_reported_frame = max(self.last_reported_frame, frame)
            if frame > self.last_captured_frame:
                self._capture_frame(frame)
            self.measurement_done.set()
        elif msg_str.startswith("Saturated"):
            raise RuntimeError("Decoder reported saturation.")

    def _capture_frame(self, frame_number: int):
        if self.params is None:
            return
        if frame_number <= self.last_captured_frame:
            return

        n_z = max(1, self.params.nZlocs)
        n_ch = max(1, self.params.nChannels)

        channel_data: List[list] = [list() for _ in range(n_ch)]
        for ch in range(n_ch):
            nt = int(getattr(self.params, f"nDtime{ch}"))
            for z in range(n_z):
                if nt <= 0:
                    channel_data[ch].append(None)
                    continue
                arr = np.zeros((self.params.nLines, self.params.nPixels, nt), dtype=np.uint16)
                self.get_data(ch, z, arr)
                channel_data[ch].append(arr)

        self.frames.append(self._format_frame(channel_data))
        self.last_captured_frame = frame_number

    def _format_frame(self, channel_data: List[list]) -> list:
        if self.params is None:
            return []

        n_z = max(1, self.params.nZlocs)
        n_ch = max(1, self.params.nChannels)
        if self.params.enableFastZscan and n_z > 1:
            frame = []
            for z in range(n_z):
                frame.append([channel_data[ch][z] for ch in range(n_ch)])
            return frame
        return [channel_data[ch][0] for ch in range(n_ch)]

    def start(self, params: DEParameters, comp_id: CompID, hardware_dll: str = ""):
        cb = self._make_callback()
        self.params = params
        dll_arg = hardware_dll.encode("ascii", errors="ignore")
        rc = self.dll.Start_TCSPC_Decode(0, cb, ctypes.byref(params), ctypes.byref(comp_id), dll_arg)
        if rc != 0:
            raise RuntimeError(f"Start_TCSPC_Decode failed with code {rc}")

    def setup_photon_binary(self, data: np.ndarray) -> None:
        assert data.dtype == np.uint32
        self._photon_binary = np.ascontiguousarray(data, dtype=np.uint32)
        rc = self.dll.Setup_Photon_Data_Binary(0, self._photon_binary.ctypes.data_as(ctypes.c_void_p), self._photon_binary.size)
        if rc != 0:
            raise RuntimeError(f"Setup_Photon_Data_Binary failed with code {rc}")

    def start_measurement(self, timeout_s: float) -> None:
        if self.params is None:
            raise RuntimeError("Decoder parameters were not initialized.")
        self.measurement_done.clear()
        rc = self.dll.Start_Measurement(0, ctypes.byref(self.params))
        if rc != 0:
            raise RuntimeError(f"Start_Measurement failed with code {rc}")
        if not self.measurement_done.wait(timeout=timeout_s):
            self.dll.Stop_Measurement(0, 1)
            raise TimeoutError(f"Timed out waiting for decode to finish after {timeout_s:.1f}s")
        if self.callback_error is not None:
            raise self.callback_error

    def get_frame_count(self) -> int:
        return self.dll.DE_GetFrameCount(0)

    def get_data(self, channel: int, zloc: int, buffer: np.ndarray) -> None:
        # buffer must be uint16 contiguous
        ptr = buffer.ctypes.data_as(ctypes.POINTER(ctypes.c_ushort))
        self.dll.DE_GetData(0, ptr, channel, zloc)

    def close(self) -> None:
        try:
            self.dll.Close_Device(0)
        except Exception:
            pass


def _read_photon_archive(photon_path: Path) -> Tuple[str, bytes]:
    """Read header (text) and first binary stream from a .photon archive."""
    if zipfile.is_zipfile(photon_path):
        with zipfile.ZipFile(photon_path, "r") as z:
            names = z.namelist()
            header_name = next((n for n in names if n.lower().endswith(".phtn")), None)
            if header_name is None:
                header_name = next((n for n in names if n.lower().endswith(".photon")), None)
            if header_name is None:
                raise FileNotFoundError("No header (.phtn/.photon) found in archive")

            header = z.read(header_name).decode("ascii", errors="ignore")
            bin_name = next((n for n in names if n.lower().endswith(".bin")), None)
            if bin_name is None:
                raise FileNotFoundError("No binary (.bin) entry found in .photon archive")

            data = z.read(bin_name)
            return header, data

    # Fallback: tar archive
    with tarfile.open(photon_path, "r:*") as tar:
        members = [m for m in tar.getmembers() if m.isreg()]
        header_member = next((m for m in members if m.name.lower().endswith(".phtn")), None)
        if header_member is None:
            header_member = next((m for m in members if m.name.lower().endswith(".photon")), None)
        if header_member is None:
            raise FileNotFoundError("No header (.phtn/.photon) found in tar archive")

        header = tar.extractfile(header_member).read().decode("ascii", errors="ignore")
        bin_member = next((m for m in members if m.name.lower().endswith(".bin")), None)
        if bin_member is None:
            raise FileNotFoundError("No binary (.bin) entry found in tar archive")
        data = tar.extractfile(bin_member).read()
        return header, data


def _build_decoder_params(flim: FLIMTiff) -> DEParameters:
    """Build DE_parameters to match the file-decoding path in FLIMage."""
    params = DEParameters()

    acq = flim.State.Acq
    spc = flim.State.Spc.spcData

    max_channels = 4
    lines_per_frame = int(getattr(acq, "linesPerFrame", flim.height))
    add_lines = int(getattr(acq, "AddLinesForSlaveMode", 0))
    pixels_per_line = int(getattr(acq, "pixelsPerLine", flim.width))
    n_channels = int(getattr(acq, "nChannels", flim.nChannels or 1))
    board_type = str(getattr(spc, "BoardType", "PQ") or "PQ")
    board_upper = board_type.upper()
    fill_fraction = float(getattr(acq, "fillFraction", 1.0))
    ms_per_line = float(getattr(acq, "msPerLine", 0.0))

    params.nChannels = int(getattr(acq, "nChannels", flim.nChannels))
    params.nLines = lines_per_frame + add_lines
    params.nPixels = pixels_per_line
    params.nFrames = int(getattr(acq, "nFrames", 1))
    params.n_average = max(1, int(getattr(acq, "nAveFrame", 1)))
    params.nZlocs = int(getattr(acq, "FastZ_nSlices", 1))
    params.enableFastZscan = 1 if getattr(acq, "fastZScan", False) else 0
    if not params.enableFastZscan:
        params.nZlocs = 1
    params.msPerLine = ms_per_line
    params.pixel_time = (ms_per_line * fill_fraction / max(1, pixels_per_line)) / 1000.0

    res0 = float(flim.resolution[0] if flim.resolution else 250.0)
    params.resolution = res0
    params.acq_modePQ = int(getattr(spc, "acq_modePQ", 0))
    params.time_per_unit = float(getattr(spc, "time_per_unit", 1.244e-8) or 1.244e-8)
    if board_upper == "PQ" and params.acq_modePQ == 2:
        params.time_per_unit = res0 * 1e-12

    for i in range(max_channels):
        val = 0
        if i < len(flim.n_time):
            val = int(flim.n_time[i])
        setattr(params, f"nDtime{i}", val)

    acq_list = list(getattr(acq, "acquisition", [True] * n_channels))
    acqflim_list = list(getattr(acq, "acqFLIMA", [True] * n_channels))
    aveframe_list = list(getattr(acq, "aveFrameA", [False] * n_channels))

    for i in range(max_channels):
        setattr(params, f"acquisition{i}", 1 if (i < len(acq_list) and acq_list[i]) else 0)
        setattr(params, f"acquireFLIM{i}", 1 if (i < len(acqflim_list) and acqflim_list[i]) else 0)
        setattr(params, f"aveFrame{i}", 1 if (i < len(aveframe_list) and aveframe_list[i]) else 0)

    if board_upper == "BH":
        params.acqType = 1
        params.TagID = int(getattr(spc, "TagID", 2))
        params.LineID = int(getattr(spc, "lineID_BH", getattr(spc, "LineID", 1)))
        params.FrameID = int(getattr(spc, "FrameID", -1))
    elif board_upper in ("PQ",):
        params.acqType = 2
        params.TagID = int(getattr(spc, "TagID", 2)) - 1
        params.LineID = int(getattr(spc, "lineID_PQ", getattr(spc, "LineID", 3))) - 1
        params.FrameID = int(getattr(spc, "FrameID", -1)) - 1
    elif board_upper == "MH":
        params.acqType = 3
        params.TagID = int(getattr(spc, "TagID", 2)) - 1
        params.LineID = int(getattr(spc, "lineID_PQ", getattr(spc, "LineID", 3))) - 1
        params.FrameID = int(getattr(spc, "FrameID", -1)) - 1
    elif board_upper == "PH":
        params.acqType = 4
        params.TagID = int(getattr(spc, "TagID", 2)) - 1
        params.LineID = int(getattr(spc, "lineID_PQ", getattr(spc, "LineID", 3))) - 1
        params.FrameID = int(getattr(spc, "FrameID", -1)) - 1
    elif board_upper == "SPAD":
        params.acqType = 11
        params.TagID = int(getattr(spc, "TagID", 2))
        params.LineID = int(getattr(spc, "LineID", 1))
        params.FrameID = int(getattr(spc, "FrameID", -1))
    elif board_upper in ("SIMPQ", "SYMPQ"):
        params.acqType = -1
        params.TagID = int(getattr(spc, "TagID", 2)) - 1
        params.LineID = int(getattr(spc, "lineID_PQ", getattr(spc, "LineID", 3))) - 1
        params.FrameID = int(getattr(spc, "FrameID", -1)) - 1
    else:
        params.acqType = 2
        params.TagID = int(getattr(spc, "TagID", 2)) - 1
        params.LineID = int(getattr(spc, "lineID_PQ", getattr(spc, "LineID", 3))) - 1
        params.FrameID = int(getattr(spc, "FrameID", -1)) - 1

    if getattr(acq, "polygonScanning", False):
        params.BiDirectionalScanX = 0
        params.BiDirectionalScanY = 0
    elif getattr(acq, "resonantScanning", False):
        params.BiDirectionalScanX = 2
        params.BiDirectionalScanY = 1 if getattr(acq, "BiDirectionalScanY", False) else 0
    else:
        params.BiDirectionalScanX = 1 if getattr(acq, "BiDirectionalScan", False) else 0
        params.BiDirectionalScanY = 1 if getattr(acq, "BiDirectionalScanY", False) else 0

    params.binning = int(getattr(spc, "binning", 0))
    params.skipFirstLines = int(getattr(spc, "SkipFirstLines", 0))
    params.skipFirstFrames = int(getattr(spc, "SkipFirstFrames", 0))
    params.pixel_binning = int(getattr(spc, "pixel_binning", 0))
    params.line_time_correction = float(getattr(spc, "line_time_correction", 1.0) or 1.0)
    params.nStartPoint = int(getattr(spc, "startPoint", 0))
    params.nEndPoint = params.nStartPoint + int(getattr(spc, "n_dataPoint", res0))
    params.focus = 0
    params.StripeDuringFocus = 0
    params.LinesPerStripe = int(getattr(acq, "linesPerStripe", 1) or 1)
    params.AcquisitionDelay = float(getattr(acq, "AcquisitionDelay", 0.0) or 0.0)
    params.BiDirectionalDelay = float(getattr(acq, "BiDirectionalDelay", 0.0) or 0.0)
    params.savePhotonsInFile = 0
    params.readFromPhotonFile = 1
    params.lineClockDivision = int(getattr(spc, "line_clock_division", 1) or 1)
    params.fastZ_measureTagParameters = 0
    params.fastZ_FrequencyKHz = 0.0
    params.fastZ_ZScanPerPixel = 0.0
    params.fastZ_ZScanPerPixel_Bidirecitonal = 0
    params.fastZ_XYFillFraction = 1.0
    params.fastZ_VoxelTimeUs = 0.0
    params.fastZ_ZScanPerLine = 1
    params.fastZ_nFastZSlices = 1
    params.fastZ_VoxelCount = 1
    params.fastZ_phaseRangeStart = 0.0
    params.fastZ_phaseRangeEnd = 0.0
    params.fastZ_phaseRangeCountStart = 0
    params.fastZ_phaseRangeCountEnd = 0
    params.fastZ_phase_detection_mode = 0
    params.fastZ_CountPerFastZCycle = 0
    params.fastZ_CountPerFastZCycleHalf = 0
    params.fastZ_CountPerFastZSlice = 0
    params.fastZ_residual_for_PhaseDetection = 0
    params.bundle_all_channels = int(getattr(spc, "bundle_all_channels", 0))
    params.fiberPhotometryMode = 0
    params.fiberBin_ms = 20.0
    params.fiberStartMode = 0
    params.lineScanMode = 1 if getattr(acq, "isLineScanAcquisition", False) else 0
    params.debug = 0
    params.eraseMemory0 = 1
    params.eraseMemory1 = 1
    params.eraseMemory2 = 1
    params.eraseMemory3 = 1

    return params


def convert_photon_to_flim_native(photon_path: Path, out_path: Path, dll_path: Optional[Path] = None):
    repo_root = Path(__file__).resolve().parents[1]
    if dll_path is None:
        # Use the Release build of TCSPC_Decode.dll
        dll_path = repo_root / "bin" / "Release" / "TCSPC_Decode.dll"
        if not dll_path.exists():
            # Fallback to the Libraries version
            dll_path = repo_root / "FLIMage" / "Libraries" / "TCSPC_Decode.dll"

    if not dll_path.exists():
        raise FileNotFoundError(f"TCSPC_Decode.dll not found at {dll_path}")

    # Lazy import to avoid requiring tifffile for non-native conversion mode.
    global FLIMTiff
    if FLIMTiff is None:
        try:
            from FLIMageFileIO import FLIMTiff as _FLIMTiff
        except ImportError as e:
            raise RuntimeError(
                "Unable to import FLIMageFileIO (requires tifffile). "
                "Install tifffile via 'pip install tifffile' to use native conversion."
            ) from e
        FLIMTiff = _FLIMTiff

    header, bin_data = _read_photon_archive(photon_path)

    flim = FLIMTiff()
    flim.decode_header(header, new=True)

    # Build decoder parameters
    params = _build_decoder_params(flim)

    comp = CompID()
    comp.compID = 0
    comp.FLIMID = 1
    decoder = TCSPCDecoder(dll_path)
    try:
        decoder.start(params, comp, hardware_dll="")
        uint_data = np.frombuffer(bin_data, dtype=np.uint32)
        decoder.setup_photon_binary(uint_data)
        timeout_s = max(5.0, params.nFrames * max(1, params.nLines) * max(params.msPerLine, 0.001) / 1000.0 * 5.0 + 5.0)
        decoder.start_measurement(timeout_s=timeout_s)

        if not decoder.frames:
            raise RuntimeError("No frames decoded from photon data.")

        first_frame = decoder.frames[0]
        if params.enableFastZscan and params.nZlocs > 1:
            first_channels = first_frame[0] if first_frame else []
            flim.FastZStack = True
            flim.nFastZSlices = params.nZlocs
        else:
            first_channels = first_frame
            flim.FastZStack = False
            flim.nFastZSlices = 1

        flim.nChannels = len(first_channels)
        flim.n_time = [arr.shape[2] if arr is not None else 0 for arr in first_channels]
        flim.width = params.nPixels
        flim.height = params.nLines
        flim.write_flim(str(out_path), decoder.frames)
        print(f"Wrote FLIM file with {len(decoder.frames)} frame(s): {out_path}")
    finally:
        decoder.close()


def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="Convert .photon archives into .flim files using FLIMage decoder.")
    parser.add_argument("--input", "-i", required=True, help="Input .photon file or directory containing .photon files.")
    parser.add_argument("--output", "-o", help="Output directory (default: same folder as input file).")
    parser.set_defaults(use_native=True)
    parser.add_argument("--native", dest="use_native", action="store_true", help="Use native TCSPC_Decode.dll binding (default).")
    parser.add_argument("--benchmark", "--csharp", dest="use_native", action="store_false", help="Use PhotonFileBenchmark.exe instead of the native decoder.")
    parser.add_argument("--dll", help="Path to TCSPC_Decode.dll (optional, only used with --native).")
    parser.add_argument("--exe", help="Path to PhotonFileBenchmark.exe (optional).")
    parser.add_argument("--aggregate", action="store_true", help="Save a single averaged frame instead of all frames.")
    parser.add_argument("--no-build", dest="build", action="store_false", help="Do not attempt to build PhotonFileBenchmark if executable is missing.")
    parser.add_argument("--configuration", default="Debug", help="Build configuration (Debug/Release).")
    parser.add_argument("--extra-args", nargs=argparse.REMAINDER, help="Extra arguments to pass to PhotonFileBenchmark.exe.")

    args = parser.parse_args(argv)

    repo_root = Path(__file__).resolve().parents[1]
    input_path = Path(args.input)
    out_dir = Path(args.output) if args.output else None

    photon_files = find_photon_files(input_path)
    if not photon_files:
        print("No .photon files found.")
        return 1

    failures = []

    if args.use_native:
        dll_path = None
        if args.dll:
            dll_path = Path(args.dll)
            if not dll_path.exists():
                print(f"Specified DLL does not exist: {dll_path}")
                return 1

        for photon_file in photon_files:
            out_root = out_dir if out_dir else photon_file.parent
            out_root.mkdir(parents=True, exist_ok=True)
            out_file = out_root / (photon_file.stem + ".flim")

            try:
                convert_photon_to_flim_native(photon_file, out_file, dll_path=dll_path)
            except Exception as ex:
                print(f"Conversion failed for {photon_file}: {ex}")
                failures.append((photon_file, ex))
    else:
        exe_path = None
        if args.exe:
            exe_path = Path(args.exe)
            if not exe_path.exists():
                print(f"Specified executable does not exist: {exe_path}")
                return 1
        else:
            exe_path = find_photon_benchmark_exe(repo_root)

        if exe_path is None and args.build:
            exe_path = build_photon_benchmark(repo_root, configuration=args.configuration)

        if exe_path is None:
            print("Could not locate PhotonFileBenchmark.exe. Provide --exe or build the project.")
            return 1

        extra_args = args.extra_args or []

        for photon_file in photon_files:
            out_root = out_dir if out_dir else photon_file.parent
            out_root.mkdir(parents=True, exist_ok=True)
            out_file = out_root / (photon_file.stem + ".flim")

            rc = run_photon_benchmark(exe_path, photon_file, out_file, extra_args, aggregate=args.aggregate)
            if rc != 0:
                failures.append((photon_file, rc))

    if failures:
        print("\nSome conversions failed:")
        for f, rc in failures:
            print(f"  {f} (exit {rc})")
        return 1

    print("\nAll conversions completed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
