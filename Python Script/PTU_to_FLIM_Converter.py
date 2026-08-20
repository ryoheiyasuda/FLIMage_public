# -*- coding: utf-8 -*-
"""
Created on Thu Nov 20 17:59:59 2025

@author: yasudar
"""
import numpy as np
from ptufile import PtuFile
from FLIMageFileIO import FLIMTiff


def ptu_to_flim(ptu_path, out_flim_path, frame=0):
    """
    Convert PTU histogram data to a FLIMage-style .flim file in 'Linear' format.
    Compatible with the original FLIMageFileIO.decode_FLIM.

    ptu_path      : input .ptu (PicoQuant / Becker-Hickl style)
    out_flim_path : output .flim
    frame         : which time frame (T axis) to use
    """

    # ------------------------------------------------------------------
    # 1. Read PTU and get histogram (T, Y, X, C, H)
    # ------------------------------------------------------------------
    ptu = PtuFile(ptu_path)
    hist = ptu[:]                      # decode entire histogram
    print("PTU hist shape:", hist.shape, "dims:", ptu.dims)

    T, Y, X, C, H = hist.shape
    if frame >= T:
        raise ValueError(f"Requested frame {frame} but T={T}")

    hist_frame = hist[frame]           # shape (Y, X, C, H)

    # Time-bin width (s) from PTU coords; convert to ps per bin
    t_axis = ptu.coords["H"]           # 1D array of bin centers (seconds)
    if len(t_axis) > 1:
        dt = float(t_axis[1] - t_axis[0])   # seconds per bin
    else:
        dt = 1e-9                       # fallback
    bin_width_ps = dt * 1.0e12          # ps per bin
    print(f'Resolution = {bin_width_ps} ps')

    sync_rate = ptu.tags["TTResult_SyncRate"]
    # bin_width_s = ptu.tags["MeasDesc_Resolution"]
    # bin_width_ps = bin_width_s * 1e12
    # print(f'Resolution = {bin_width_ps} ps')

    
    # ------------------------------------------------------------------
    # 2. Build FLIMTiff object and header state
    # ------------------------------------------------------------------
    flim = FLIMTiff()
    flim.ImageFormat = "Linear"        # IMPORTANT: we're doing Linear

    flim.nChannels = C
    flim.width = X
    flim.height = Y
    flim.n_time = [H] * C
    flim.resolution = [bin_width_ps] * C

    # Fill State.* so the header is self-consistent and decodes correctly
    flim.State.Acq.nChannels       = C
    flim.State.Acq.pixelsPerLine   = X
    flim.State.Acq.linesPerFrame   = Y
    flim.State.Acq.acqFLIMA        = [True] * C
    flim.State.Acq.acquisition     = [True] * C
    flim.State.Acq.FastZ_nSlices   = 1
    flim.State.Acq.fastZScan       = False
    flim.State.Acq.ZStack          = False

    flim.State.Spc.spcData.n_dataPoint = H
    flim.State.Spc.spcData.resolution  = flim.resolution
    flim.State.Spc.datainfo.xres       = X
    flim.State.Spc.datainfo.yres       = Y
    flim.State.Spc.datainfo.syncRate = [sync_rate] * C

    flim.State.Files.baseName    = "PTU_Converted"
    flim.State.Files.fileName    = "PTU_Converted001"
    flim.State.Files.pathName    = "."
    flim.acqTime = [None]          # let _build_header generate Acquired_Time

    # ------------------------------------------------------------------
    # 3. Pack data into Linear layout: (1, width * height * sum(n_time))
    # ------------------------------------------------------------------
    # We build [page][channel] -> [H, W, n_time] *conceptually*,
    # but then we explicitly pack to Linear row to be safe.
    page = []
    for ch in range(C):
        # hist_frame: (Y, X, C, H); take one channel -> (Y, X, H)
        ch_img = hist_frame[:, :, ch, :]       # (Y, X, H)
        # FLIMage expects (height, width, n_time)
        if ch_img.shape != (Y, X, H):
            raise ValueError("Unexpected channel shape", ch_img.shape)
        page.append(ch_img)

    # Manual Linear packing (equivalent to flim._pack_page_linear_or_zlinear)
    total_time_bins = sum(flim.n_time)
    n_pixels = flim.width * flim.height
    row_length = n_pixels * total_time_bins

    row = np.zeros(row_length, dtype=np.uint16)
    offset = 0
    for ch in range(C):
        nt = flim.n_time[ch]
        img = np.asarray(page[ch], dtype=np.uint16)  # (Y,X,H)
        flat = img.reshape(-1, order="C")            # length = Y*X*H
        size = flat.size
        assert size == flim.width * flim.height * nt
        row[offset:offset + size] = flat
        offset += size

    assert offset == row_length
    flim_array = row.reshape(1, -1)   # (1, width*height*sum(n_time))

    print("Linear row shape:", flim_array.shape,
          "expected:", (1, row_length))

    # ------------------------------------------------------------------
    # 4. Use FLIMTiff header builder + tifffile to write page
    # ------------------------------------------------------------------
    import tifffile

    header_str = flim._build_header(acq_time_str=None)

    with tifffile.TiffWriter(out_flim_path, bigtiff=False) as tif:
        tif.write(
            flim_array,
            photometric="minisblack",
            metadata=None,
            description=header_str,
        )

    print("Wrote FLIM file:", out_flim_path)


if __name__ == "__main__":
    ptu_path = r"Series002_t1.ptu"
    out_flim_path = r"Series002_t1.flim"
    ptu_to_flim(ptu_path, out_flim_path, frame=0)

    flim = FLIMTiff()
    flim.read(out_flim_path)          # uses tifffile, no libtiff
    
    lifetime_range = [2,130]
    # compute maps for page 0, fastZ 0, channel 0
    flim.calculatePage(page=0, fastZpage=0, channel=0,
                        lifetimeRange=lifetime_range,
                        intensityLimit=[0, 5],
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
   