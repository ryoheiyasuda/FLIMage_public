import os
from datetime import datetime
from typing import List, Any

import numpy as np
import tifffile


class SimpleNS:
    """Simple dynamic namespace (like a very small SimpleNamespace)."""
    def __init__(self, **kwargs):
        self.__dict__.update(kwargs)


class MicroscopeParameters:
    """
    Minimal stand-in for the original microscope_parameters.
    decode_header will fill these via exec() of header lines.
    """
    def __init__(self):
        # Core state sub-objects
        self.Acq = SimpleNS()
        self.Spc = SimpleNS(
            spcData=SimpleNS(),
            datainfo=SimpleNS()
        )
        self.Uncaging = SimpleNS()
        self.Motor = SimpleNS()
        self.DO = SimpleNS()
        self.Ephys = SimpleNS()
        self.Files = SimpleNS()

        # Reasonable defaults (will be overwritten by header if present)
        self.Acq.nChannels = 2
        self.Acq.pixelsPerLine = 128
        self.Acq.linesPerFrame = 128
        self.Acq.acqFLIMA = [True, False]
        self.Acq.acquisition = [True, True]
        self.Acq.FastZ_nSlices = 1
        self.Acq.fastZScan = False
        self.Acq.ZStack = False

        self.Spc.spcData.n_dataPoint = 64
        self.Spc.spcData.resolution = [250, 250]  # ps per bin, per channel
        self.Spc.datainfo.xres = 128
        self.Spc.datainfo.yres = 128


class FLIMTiff:
    """
    Standalone FLIM reader/writer + FLIM calculations using tifffile only.

    Layout for FLIM image data in memory:
        self.image[page][fastZ][channel] -> array [height, width, n_time[channel]]
        If FastZStack is False:
            use fastZ index 0: self.image[page][0][channel]
    """

    def __init__(self):
        # File/meta
        self.filename: str = ""
        self.flim: bool = False

        # Per-file image collection
        self.n_images: int = 0
        self.image: List[Any] = []    # image[page][fastZ][channel]
        self.acqTime: List[str] = []  # one string per page

        # FLIM per-channel configuration
        self.n_time: List[int] = [1, 1]
        self.nChannels: int = 0
        self.width: int = 128
        self.height: int = 128
        self.resolution: List[float] = [250, 250]  # ps

        # FastZ / Z stack
        self.FastZStack: bool = False
        self.ZStack: bool = False
        self.nFastZSlices: int = 1

        # Format: "Linear", "ZLinear", "ChTime_YX"
        self.ImageFormat: str = "Linear"

        # State (header parameters)
        self.State = MicroscopeParameters()

        # Current selection
        self.currentPage: int = 0
        self.currentChannel: int = 0
        self.currentZPage: int = 0

        # FLIM data for current selection
        # FLIM3D[y, x, t]
        self.FLIM3D: np.ndarray = np.zeros((128, 128, 2), dtype=np.double)

        # Calculated outputs
        self.time: np.ndarray = np.zeros(2, dtype=float)         # time axis (ns)
        self.lifetime: np.ndarray = np.zeros(2, dtype=float)     # summed decay
        self.intensity: np.ndarray = np.zeros((128, 128))        # 2D intensity
        self.lifetimeMap: np.ndarray = np.zeros((128, 128))      # 2D lifetime (ns)
        self.rgbLifetime: np.ndarray = np.zeros((128, 128, 3))   # RGB FLIM map

    # ------------------------------------------------------------------
    # Header decoding
    # ------------------------------------------------------------------
    def executeLine(self, info: str):
        """
        Execute a header assignment line like:
            "State.Acq.pixelsPerLine = 128"
        with protection for Windows backslashes inside strings.
        """
        eq = info.split(" = ", 1)
        if len(eq) != 2:
            return

        lhs = eq[0].strip()
        rhs = eq[1].strip()

        # If rhs is a quoted string, escape backslashes, etc.
        if (rhs.startswith('"') and rhs.endswith('"')) or \
           (rhs.startswith("'") and rhs.endswith("'")):
            inner = rhs[1:-1]
            inner = inner.encode("unicode_escape").decode("ascii")
            rhs = f'"{inner}"'

        try:
            if "acqTime" not in lhs:
                # Prepend "self." so we set self.State.Acq..., etc.
                exec(f"self.{lhs} = {rhs}")
        except Exception:
            # Ignore parsing failures to match original behavior
            pass

    def decode_header(self, header, new: bool = True):
        """
        Parse ImageDescription text, populate self.State and acquisition meta.
        """
        if isinstance(header, bytes):
            htxt = header.decode("ASCII", errors="ignore")
        else:
            htxt = str(header)

        infos = htxt.split("\r\n")

        for info in infos:
            info = info.replace(";", "").strip()
            if not info:
                continue

            # Execute any State.* assignment
            if info.startswith("State."):
                self.executeLine(info)

            # Format line: "Format = Linear"
            if "Format" in info and not info.startswith("State."):
                parts = info.split(" = ", 1)
                if len(parts) == 2:
                    self.ImageFormat = parts[1].strip()

            # Acquisition time: "Acquired_Time = 2020-11-19T09:35:44.900"
            if "Acquired_Time" in info:
                parts = info.split(" = ", 1)
                if len(parts) == 2:
                    self.acqTime.append(parts[1].strip())

        if self.currentPage == 0 and infos:
            if "FLIMimage" not in infos[0]:
                # Not a strict error, but original code warns
                print("Warning: This file may not be generated by FLIMage")

        # --- Derive n_time per channel, nChannels, etc. from State ---
        # Channels
        acq = self.State.Acq
        spcData = self.State.Spc.spcData

        n_ch = getattr(acq, "nChannels", None)
        if n_ch is None:
            # fallback: length of acquisition or acqFLIMA, if present
            if hasattr(acq, "acquisition"):
                n_ch = len(acq.acquisition)
            elif hasattr(acq, "acqFLIMA"):
                n_ch = len(acq.acqFLIMA)
            else:
                n_ch = 1
        self.nChannels = int(n_ch)

        n_dataPoint = int(getattr(spcData, "n_dataPoint", 1))

        acqFLIMA = list(getattr(acq, "acqFLIMA", [True] * self.nChannels))
        acquisition = list(getattr(acq, "acquisition", [True] * self.nChannels))

        self.n_time = []
        for i in range(self.nChannels):
            acq_flim = acqFLIMA[i] if i < len(acqFLIMA) else True
            acq_img = acquisition[i] if i < len(acquisition) else True
            if acq_flim and acq_img:
                self.n_time.append(n_dataPoint)
            elif not acq_img:
                self.n_time.append(0)
            else:
                # intensity-only channel
                self.n_time.append(1)

        if self.currentPage == 0:
            # Image size
            self.width = int(getattr(acq, "pixelsPerLine",
                                     getattr(self.State.Spc.datainfo, "xres", 128)))
            self.height = int(getattr(acq, "linesPerFrame",
                                      getattr(self.State.Spc.datainfo, "yres", 128)))

            # Time resolution per channel (ps)
            res = getattr(spcData, "resolution", None)
            if res is None:
                self.resolution = [250] * self.nChannels
            else:
                if isinstance(res, (list, tuple)):
                    self.resolution = list(res)
                else:
                    self.resolution = [float(res)] * self.nChannels

            # FastZ and ZStack
            fastZ_nSlices = int(getattr(acq, "FastZ_nSlices", 1))
            fastZScan = bool(getattr(acq, "fastZScan", False))
            self.FastZStack = fastZ_nSlices > 1 and fastZScan
            self.ZStack = bool(getattr(acq, "ZStack", False))
            if self.FastZStack:
                self.nFastZSlices = fastZ_nSlices
            else:
                self.nFastZSlices = 1

    # ------------------------------------------------------------------
    # FLIM decoding
    # ------------------------------------------------------------------
    def decode_FLIM(self, flim: np.ndarray):
        """
        Decode raw FLIM array to [fastZ][channel] images.

        For Linear format:
            flim shape: (1, pixels * sum(n_time))
        For ZLinear:
            flim shape: (nFastZSlices, pixels * sum(n_time))
        For ChTime_YX:
            flim shape: (height, width, sum(n_time)).
        """
        image = []

        if self.ImageFormat in ("ZLinear", "Linear", "ChTime_YX"):
            flim_list = []
            if self.ImageFormat == "ZLinear":
                flim_list = np.split(flim, self.nFastZSlices, axis=0)
            else:
                flim_list = [flim]

            for flim_each in flim_list:
                imageC = []
                if self.ImageFormat == "ChTime_YX":
                    # flim_each already has combined time in last axis
                    img3d = np.reshape(
                        flim_each,
                        (self.height, self.width, sum(self.n_time)),
                        order="C",
                    )
                    offset = 0
                    for i in range(self.nChannels):
                        nt = self.n_time[i]
                        if nt > 0:
                            offset2 = offset + nt
                            imageC.append(img3d[:, :, offset:offset2])
                            offset = offset2
                        else:
                            imageC.append(np.zeros(1))
                else:
                    # Linear / ZLinear: flatten then split per channel
                    flat = np.ravel(flim_each)
                    offset = 0
                    pix = self.height * self.width
                    for i in range(self.nChannels):
                        nt = self.n_time[i]
                        if nt > 0:
                            size = pix * nt
                            offset2 = offset + size
                            chunk = flat[offset:offset2]
                            img = chunk.reshape(
                                (self.height, self.width, nt),
                                order="C",
                            )
                            imageC.append(img)
                            offset = offset2
                        else:
                            imageC.append(np.zeros(1))
                image.append(imageC)
        else:
            # fallback generic reshape (should rarely be needed)
            pix = self.height * self.width
            total_t = sum(self.n_time)
            flim_reshaped = np.reshape(flim, (self.nFastZSlices, pix * total_t))
            flim_list = np.split(flim_reshaped, self.nFastZSlices, axis=0)
            for flim_each in flim_list:
                flat = np.ravel(flim_each)
                offset = 0
                imageC = []
                for i in range(self.nChannels):
                    nt = self.n_time[i]
                    if nt > 0:
                        size = pix * nt
                        offset2 = offset + size
                        chunk = flat[offset:offset2]
                        img = chunk.reshape(
                            (self.height, self.width, nt),
                            order="C",
                        )
                        imageC.append(img)
                        offset = offset2
                    else:
                        imageC.append(np.zeros(1))
                image.append(imageC)

        return image

    # ------------------------------------------------------------------
    # TIFF reading (tifffile)
    # ------------------------------------------------------------------
    def read(self, file_path: str, read_image: bool = True):
        """
        Read a FLIMage-style .flim (or .tif) file using tifffile.
        """
        self.filename = file_path
        self.n_images = 0
        self.acqTime = []
        self.image = []
        self.currentPage = 0
        self.flim = (os.path.splitext(file_path)[-1].lower() == ".flim")

        with tifffile.TiffFile(file_path) as tf:
            for page_index, page in enumerate(tf.pages):
                desc_tag = page.tags.get("ImageDescription", None)
                if desc_tag is not None:
                    header = desc_tag.value
                    self.decode_header(header, new=(page_index == 0))

                if read_image:
                    arr = page.asarray()
                    if self.flim:
                        flim = np.array(arr).astype(np.ushort)
                        self.image.append(self.decode_FLIM(flim))
                    else:
                        self.image.append(np.array(arr))

                self.currentPage += 1
                self.n_images += 1

        # Reset to first page and load first FLIM cube if available
        self.currentPage = 0
        self.currentZPage = 0
        self.currentChannel = 0
        if self.flim and self.image:
            self.LoadFLIMFromMemory(0, 0, 0)

        return self

    # ------------------------------------------------------------------
    # FLIM3D handling and validity checks
    # ------------------------------------------------------------------
    def pageValid(self, page: int = 0, fastZpage: int = 0, channel: int = 0) -> bool:
        fastZValid = (not self.FastZStack) or (0 <= fastZpage < self.nFastZSlices)
        all_valid = (
            self.flim
            and fastZValid
            and 0 <= channel < self.nChannels
            and 0 <= page < len(self.image)
            and len(self.image) > 0
        )
        return all_valid

    def LoadFLIMFromMemory(self, page: int, fastZpage: int, channel: int):
        if not self.pageValid(page, fastZpage, channel):
            return

        if not self.FastZStack:
            fastZpage = 0

        ch_img = self.image[page][fastZpage][channel]
        self.currentPage = page
        self.currentZPage = fastZpage
        self.currentChannel = channel
        self.FLIM3D = np.array(ch_img, dtype=np.double)

        # Update intensity/time arrays sizes
        h, w, t = self.FLIM3D.shape
        self.intensity = np.zeros((h, w), dtype=float)
        self.lifetimeMap = np.zeros((h, w), dtype=float)
        self.rgbLifetime = np.zeros((h, w, 3), dtype=float)
        self.lifetime = np.zeros(t, dtype=float)
        # Time axis (ns) for current channel
        res_ps = self.resolution[self.currentChannel] if self.currentChannel < len(self.resolution) else 250.0
        self.time = np.arange(t) * res_ps / 1000.0

    def ifFLIMimage(self) -> bool:
        return (
            self.flim
            and self.FLIM3D is not None
            and self.FLIM3D.ndim == 3
            and self.FLIM3D.shape[2] > 1
        )

    # ------------------------------------------------------------------
    # FLIM calculations / "display" maps
    # ------------------------------------------------------------------
    def calculateIntensity(self):
        """
        Sum over time to get intensity image (2D).
        """
        if not self.ifFLIMimage():
            return
        self.intensity = np.sum(self.FLIM3D, axis=2)

    def calculateLifetimeCurve(self, page: int = None, channel: int = None,
                               threshold: float = 0):
        """
        Compute global decay curve by summing over x,y (optionally thresholded).
        Uses current FLIM3D; page/channel arguments are optional.
        """
        if page is not None and channel is not None:
            # load requested page/channel if provided
            self.LoadFLIMFromMemory(page, 0, channel)

        if not self.ifFLIMimage():
            return

        img = self.FLIM3D.copy()
        siz = img.shape

        if threshold > 0:
            if self.intensity is None or self.intensity.shape[:2] != siz[:2]:
                self.calculateIntensity()
            imgMask = np.reshape(
                np.repeat(self.intensity >= threshold, siz[2]),
                siz,
            )
            img = img * imgMask

        # sum over x,y into 1D decay curve
        self.lifetime = np.sum(np.sum(img, axis=0), axis=0)

        # update time axis for current channel
        ch = self.currentChannel
        res_ps = self.resolution[ch] if ch < len(self.resolution) else 250.0
        self.time = np.arange(self.FLIM3D.shape[2]) * res_ps / 1000.0

    def calculateLifetimeMap(self, lifetimeRange=None, lifetimeOffset: float = 0.5):
        """
        Compute mean lifetime map (ns) using first moment over selected bins.
        """
        if lifetimeRange is None:
            lifetimeRange = [0, self.FLIM3D.shape[2]]

        if not self.ifFLIMimage():
            return
        ch = self.currentChannel
        nt = self.n_time[ch] if ch < len(self.n_time) else self.FLIM3D.shape[2]
        if nt <= 1:
            return

        # clamp range
        lifetimeRange = list(lifetimeRange)
        if lifetimeRange[0] < 0:
            lifetimeRange[0] = 0
        if lifetimeRange[1] > nt:
            lifetimeRange[1] = nt

        img = self.FLIM3D
        siz = img.shape
        timeArray = np.array([np.arange(nt)])
        timeMatrix = np.repeat(timeArray, siz[0] * siz[1], axis=0)
        timeMatrix = np.reshape(timeMatrix, siz)

        lt_range = range(lifetimeRange[0], lifetimeRange[1])

        sumImg = np.sum(img[:, :, lt_range], axis=2)
        if self.intensity is None or self.intensity.shape[:2] != siz[:2]:
            self.calculateIntensity()
        sumImgZero = self.intensity == 0
        sumImg_safe = sumImg.copy()
        sumImg_safe[sumImgZero] = 1.0

        waitedSum = np.sum(
            img[:, :, lt_range] * timeMatrix[:, :, lt_range], axis=2
        ) / sumImg_safe
        waitedSum[sumImgZero] = 0.0

        res_ps = self.resolution[ch] if ch < len(self.resolution) else 250.0
        self.lifetimeMap = waitedSum * res_ps / 1000.0 - lifetimeOffset

    def calculateRGBLifetimeMap(self, lifetimeLimit=None, intensityLimit=None):
        """
        Convert lifetimeMap and intensity into an RGB lifetime image
        using a FLIM-style pseudocolor.
        """
        if lifetimeLimit is None:
            lifetimeLimit = [1.6, 2.0]
        if intensityLimit is None:
            intensityLimit = [3, 25]

        if not self.ifFLIMimage():
            return
        ch = self.currentChannel
        nt = self.n_time[ch] if ch < len(self.n_time) else self.FLIM3D.shape[2]
        if nt <= 1:
            return

        # Normalized reversed lifetime gray map
        gray = (self.lifetimeMap - lifetimeLimit[0]) / (lifetimeLimit[1] - lifetimeLimit[0])
        gray = 1.0 - gray
        gray = np.clip(gray, 0.0, 1.0)

        part1 = np.logical_and(0 <= gray, gray < 1 / 3)
        part2 = np.logical_and(1 / 3 <= gray, gray < 2 / 3)
        part3 = np.logical_and(2 / 3 <= gray, gray <= 1)

        blue = part1 + part2 * (-3 * gray + 2)
        green = part1 * (3 * gray) + part2 + part3 * (-3 * gray + 3)
        red = part2 * (3 * gray - 1) + part3

        if self.intensity is None or self.intensity.shape[:2] != gray.shape:
            self.calculateIntensity()
        alpha = (self.intensity - intensityLimit[0]) / (intensityLimit[1] - intensityLimit[0])
        alpha = np.clip(alpha, 0.0, 1.0)

        rgbImage = np.array([red * alpha, green * alpha, blue * alpha])
        self.rgbLifetime = np.transpose(rgbImage, (1, 2, 0))

    def calculateAll(self, lifetimeRange=None,
                     intensityLimit=None,
                     lifetimeLimit=None,
                     lifetimeOffset: float = 0.5):
        """
        Convenience wrapper to compute intensity, lifetime curve, lifetime map,
        and RGB FLIM map for the current page/Z/channel.
        """
        if lifetimeRange is None:
            lifetimeRange = [0, self.FLIM3D.shape[2]]
        if intensityLimit is None:
            intensityLimit = [0, 20]
        if lifetimeLimit is None:
            lifetimeLimit = [1.6, 2.0]

        self.calculateIntensity()
        self.calculateLifetimeCurve(threshold=intensityLimit[0])
        self.calculateLifetimeMap(lifetimeRange, lifetimeOffset)
        self.calculateRGBLifetimeMap(lifetimeLimit, intensityLimit)

    def calculatePage(self, page: int = 0, fastZpage: int = 0,
                      channel: int = 0,
                      lifetimeRange=None,
                      intensityLimit=None,
                      lifetimeLimit=None,
                      lifetimeOffset: float = 0.5):
        """
        Load selected page/fastZ/channel and compute all derived maps.
        """
        if not self.pageValid(page, fastZpage, channel):
            return
        self.LoadFLIMFromMemory(page, fastZpage, channel)
        self.calculateAll(lifetimeRange, intensityLimit, lifetimeLimit, lifetimeOffset)

    # ------------------------------------------------------------------
    # Writer helpers
    # ------------------------------------------------------------------
    def _object_to_lines(self, obj, prefix: str):
        """
        Turn a parameter object (like State.Acq) into 'key = value;' lines
        that decode_header will recognize and execute.
        """
        lines = []
        if obj is None:
            return lines
        for attr, value in obj.__dict__.items():
            if attr.startswith("_"):
                continue
            if callable(value):
                continue
            try:
                value_repr = repr(value)
            except Exception:
                continue
            lines.append(f"{prefix}{attr} = {value_repr};")
        return lines

    def _build_header(self, acq_time_str: str = None) -> str:
        """
        Build a FLIMage-style ImageDescription header that closely matches
        original FLIMage output.
        """
        # Default: current time with millisecond resolution, FLIMage style
        if acq_time_str is None:
            acq_time_str = datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%f")[:-3]

        lines = []

        # First line: FLIMage marker (as seen in real files)
        lines.append("FLIMimage parameters")

        # Dump all relevant State.* sections
        lines.extend(self._object_to_lines(self.State.Acq,          "State.Acq."))
        lines.extend(self._object_to_lines(self.State.Spc.spcData,  "State.Spc.spcData."))
        lines.extend(self._object_to_lines(self.State.Spc.datainfo, "State.Spc.datainfo."))
        lines.extend(self._object_to_lines(self.State.Uncaging,     "State.Uncaging."))
        lines.extend(self._object_to_lines(self.State.Motor,        "State.Motor."))
        lines.extend(self._object_to_lines(self.State.DO,           "State.DO."))
        lines.extend(self._object_to_lines(self.State.Ephys,        "State.Ephys."))
        # Include Files block so paths, baseName, etc. round-trip
        lines.extend(self._object_to_lines(self.State.Files,        "State.Files."))

        # SaveChannels (non-State variable present in original header)
        acquisition = getattr(self.State.Acq, "acquisition", None)
        if isinstance(acquisition, (list, tuple)):
            save_channels = list(acquisition)
            lines.append(f"SaveChannels = {repr(save_channels)};")

        # Acquisition time and format, same style as original
        lines.append(f"Acquired_Time = {acq_time_str};")
        lines.append(f"Format = {self.ImageFormat};")

        # Use CRLF because the original reader splits on '\r\n'
        return "\r\n".join(lines)

    def _pack_channels_into_row(self, channel_list):
        """
        Flatten and concatenate all channels in one fastZ slice into a 1D row
        in the order expected by decode_FLIM for 'Linear'/'ZLinear' format.
        """
        total_time_bins = sum(self.n_time)
        n_pixels = self.width * self.height
        row_length = n_pixels * total_time_bins
        row = np.zeros(row_length, dtype=np.uint16)

        offset = 0
        for ch in range(self.nChannels):
            nt = self.n_time[ch] if ch < len(self.n_time) else 0
            if nt <= 0:
                continue
            img = channel_list[ch]
            if img is None or np.size(img) == 1:
                continue
            img = np.asarray(img)
            if img.shape != (self.height, self.width, nt):
                raise ValueError(
                    f"Channel {ch} image has shape {img.shape}, "
                    f"expected ({self.height}, {self.width}, {nt})"
                )
            flat = img.reshape(-1, order="C")
            size = flat.size
            row[offset:offset + size] = flat
            offset += size
        return row

    def _pack_page_linear_or_zlinear(self, page):
        """
        Pack a single logical page into the 'Linear' or 'ZLinear' format.
        """
        total_time_bins = sum(self.n_time)
        n_pixels = self.width * self.height
        row_length = n_pixels * total_time_bins

        if self.FastZStack and self.nFastZSlices > 1:
            flim = np.zeros((self.nFastZSlices, row_length), dtype=np.uint16)
            for z in range(self.nFastZSlices):
                z_slice = page[z]
                flim[z, :] = self._pack_channels_into_row(z_slice)
        else:
            row = self._pack_channels_into_row(page)
            flim = row[np.newaxis, :]
        return flim

    def _pack_page_chtime_yx(self, page):
        """
        Pack a single logical page into 'ChTime_YX' format (H, W, sum(n_time)).
        """
        total_time_bins = sum(self.n_time)
        combined = np.zeros(
            (self.height, self.width, total_time_bins),
            dtype=np.uint16,
        )
        offset = 0
        for ch in range(self.nChannels):
            nt = self.n_time[ch] if ch < len(self.n_time) else 0
            if nt <= 0:
                continue
            img = page[ch]
            if img is None or np.size(img) == 1:
                continue
            img = np.asarray(img)
            if img.shape != (self.height, self.width, nt):
                raise ValueError(
                    f"Channel {ch} image has shape {img.shape}, "
                    f"expected ({self.height}, {self.width}, {nt})"
                )
            combined[:, :, offset:offset + nt] = img
            offset += nt
        return combined

    # ------------------------------------------------------------------
    # Writer: save FLIM using tifffile
    # ------------------------------------------------------------------
    def write_flim(self, file_path: str, image_pages):
        """
        Write FLIM data to a .flim (TIFF) file using tifffile.

        image_pages layout:
            If FastZStack == False:
                image_pages[page][channel] -> [H, W, n_time[ch]]
            If FastZStack == True:
                image_pages[page][fastZ][channel] -> [H, W, n_time[ch]]
        """
        with tifffile.TiffWriter(file_path, bigtiff=True) as tif:
            for page_index, page in enumerate(image_pages):
                # Use stored acqTime if available; otherwise generate now
                if page_index < len(self.acqTime):
                    acq_time_str = self.acqTime[page_index]
                else:
                    acq_time_str = None

                header_str = self._build_header(acq_time_str)

                if self.ImageFormat in ("Linear", "ZLinear"):
                    flim_array = self._pack_page_linear_or_zlinear(page)
                elif self.ImageFormat == "ChTime_YX":
                    flim_array = self._pack_page_chtime_yx(page)
                else:
                    raise ValueError(f"Unsupported ImageFormat {self.ImageFormat}")

                tif.write(
                    flim_array.astype(np.uint16),
                    photometric="minisblack",
                    metadata=None,
                    description=header_str,
                )
                
    def save_all(self, file_path: str):
        """
        Convenience wrapper to save the current multi-page FLIM dataset.
    
        This writes:
          - all pages in self.image
          - each with its own header
          - using self.acqTime[page_index] for Acquired_Time (if present)
        """
        self.write_flim(file_path, self.image)

if __name__ == "__main__":
    file_path = '..\SampleImages\yn294_201119HeLa\yn294a001.flim'

    flim = FLIMTiff()
    flim.read(file_path)          # uses tifffile, no libtiff
    
    lifetime_range = [2,60]
    # compute maps for page 0, fastZ 0, channel 0
    flim.calculatePage(page=0, fastZpage=0, channel=0,
                       lifetimeRange=lifetime_range,
                       intensityLimit=[3, 25],
                       lifetimeLimit=[1.6, 4.0],
                       lifetimeOffset=0.5)
    
    from matplotlib import pyplot as plt
    plt.figure();
    plt.subplot(1,2,1)
    plt.imshow(flim.rgbLifetime); 
    plt.axis("off"); 
    
    plt.subplot(1,2,2)
    plt.semilogy(flim.lifetime[lifetime_range[0]:lifetime_range[1]])
    plt.show()


    
    