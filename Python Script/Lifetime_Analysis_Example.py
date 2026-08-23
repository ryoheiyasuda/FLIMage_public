# -*- coding: utf-8 -*-
"""
Created on Fri Nov 21 13:50:02 2025

@author: yasudar
"""

import os
import numpy as np

from Reconvolution_Fitting2 import FLIMDecayFitter
from matplotlib import pyplot as plt
from FLIMageFileIO import FLIMTiff
from PTU_reader import PTU_Reader

dir1 = r'C:\path\to\your\data'
path1 = r'251018 fiber test\PH330\50MHz_5.6%_4F_002.ptu'
path2 = r'251018 1P test\SP8_fluorescein_export raw data.sptw\Series002_t1.ptu'
path3 = r'251018 2P test\fluorescein002.flim'
paths = [path1, path2, path3]
channels = [0, 0, 0]
titles = ['Fiber', '1p', '2p']

n_exp = 2

plt.figure(figsize = (12, 4))
for i, path in enumerate(paths):
    c = channels[i]
    if path.endswith('.ptu'):
        ptu_path = os.path.join(dir1, path)    
        flim = PTU_Reader()
        flim.read_ptu(ptu_path, c)
        lifetime_range = [2,flim.lifetime.shape[0]-10]
        sync_rate_hz = flim.sync_rate        
        dt_ps = flim.resolution
        dt_ns = dt_ps / 1000
        t_full = np.arange(flim.lifetime.shape[0]) * dt_ns
        y_full = flim.lifetime
        

    elif path.endswith('.flim'):
        flim_path = os.path.join(dir1, path)
        flim = FLIMTiff()
        flim.read(flim_path)          # uses tifffile, no libtiff
        flim.calculateLifetimeCurve(threshold=0)
        lifetime_range = [2,flim.lifetime.shape[0]-5]
        
        dt_ps = flim.State.Spc.spcData.resolution[c]
        sync_rate_hz = float(flim.State.Spc.datainfo.syncRate[c])   
        dt_ns = dt_ps / 1000
        
        t_full = np.arange(flim.FLIM3D.shape[2]) * dt_ns
        y_full = flim.lifetime
        
    # Extract only fitting range
    start, end = lifetime_range
    t = t_full[start:end]
    y = y_full[start:end]

    #%%
    fitter = FLIMDecayFitter(n_exp=n_exp, sync_rate_hz=sync_rate_hz, time_unit="ns")
    
    fixed = {} #{"bg": 0.0,}
    res = fitter.fit(t, y) #, fixed_params=fixed)
    
    params = res.x_full
    y_fit = fitter.reconvolution_model(t, params)
   
    print('----------------------')
    print(f'----{titles[i]}------')
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
    #print("chi2:", res.chi2)
    print("reduced chi2:", res.red_chi2)
    
    plt.subplot(1, len(paths), i+1)
    plt.semilogy(t, y)
    plt.semilogy(t, y_fit)
    if i == 0:
        plt.ylabel('N Photons')
    plt.xlabel('Time (ns)')
    plt.title(f'{titles[i]}')
    
    



