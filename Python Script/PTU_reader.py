# -*- coding: utf-8 -*-
"""
Created on Thu Nov 20 17:59:59 2025

@author: yasudar
"""
import numpy as np
from ptufile import PtuFile


class PTU_Reader():
    def __init__(self):
        self.sync_rate = 80e9
        self.resolution = 25
        self.channel = 0
    def read_ptu(self, ptu_path, channel):
    
        # ------------------------------------------------------------------
        # 1. Read PTU and get histogram (T, Y, X, C, H)
        # ------------------------------------------------------------------
        ptu = PtuFile(ptu_path)
        self.channel = channel
        hist = ptu[:]                      # decode entire histogram
        self.hist = hist
        #print("PTU hist shape:", hist.shape, "dims:", ptu.dims)
        
        if ptu.dims == ('T', 'C', 'H'):
            T, C, H = hist.shape
            lt = hist.sum(axis=0)[self.channel,:]
        elif ptu.dims == ('T', 'Y', 'X', 'C', 'H'):
            T, Y, X, C, H = hist.shape
            lt = hist.sum(axis=0).sum(axis=0).sum(axis=0)[self.channel,:]
        # Time-bin width (s) from PTU coords; convert to ps per bin
        t_axis = ptu.coords["H"]           # 1D array of bin centers (seconds)
        if len(t_axis) > 1:
            dt = float(t_axis[1] - t_axis[0])   # seconds per bin
        else:
            dt = 1e-9                       # fallback
        bin_width_ps = dt * 1.0e12          # ps per bin
        #print(f'Resolution = {bin_width_ps} ps')
        self.sync_rate = ptu.tags["TTResult_SyncRate"]
        self.resolution = bin_width_ps
        self.lifetime = lt

if __name__ == "__main__":
    #ptu_path = r"Series002_t1.ptu"
    dir1 = r'C:\path\to\your\data'
    ptu_path1 = r'251018 fiber test\PH330\20MHz_5%_4F_001.ptu'
    
    import os
    
    ptu_path = os.path.join(dir1, ptu_path1)    
    flim = PTU_Reader()
    flim.read_ptu(ptu_path, 0)
    lifetime_range = [2,flim.lifetime.shape[0]-10]
    sync_rate_hz = flim.sync_rate
    
    dt_ps = flim.resolution
    dt_ns = dt_ps / 1000
    t_full = np.arange(flim.lifetime.shape[0]) * dt_ns
    y_full = flim.lifetime
    
    # Extract only fitting range
    start, end = lifetime_range
    t = t_full[start:end]
    y = y_full[start:end]

    
    from Reconvolution_Fitting2 import FLIMDecayFitter
    from matplotlib import pyplot as plt
    
    #%%
    # compute maps for page 0, fastZ 0, channel 0
    n_exp = 1
    fitter = FLIMDecayFitter(n_exp=n_exp, sync_rate_hz=sync_rate_hz, time_unit="ns")
    
    fixed = {} #{"bg": 0.0,}
    res = fitter.fit(t, y) #, fixed_params=fixed)
    
    params = res.x_full
    y_fit = fitter.reconvolution_model(t, params)
   
    if n_exp == 1:
        A1, tau1, t0, sigma, bg = params
        p1 = 1
        print(f'tau1 = {tau1:0.3f} ns ({p1:0.2f})%')
    
    if n_exp == 2:
        A1, tau1, A2, tau2, t0, sigma, bg = params
        p1 = A1 / (A1 + A2) * 100
        p2 = A2 / (A1 + A2) * 100
        
        print(f'tau1 = {tau1:0.3f} ns ({p1:0.2f}%)')
        print(f'tau2 = {tau2:0.3f} ns ({p2:0.2f}%)')

    print(f'Sigma = {sigma:0.3f} ns')
    print(f't0 = {t0:0.3f} ns')
    print(f'bg = {bg}')
    print("chi2:", res.chi2)
    print("reduced chi2:", res.red_chi2)

    plt.semilogy(t, y)
    plt.semilogy(t, y_fit)
    plt.show()
