# -*- coding: utf-8 -*-
"""
Created on Wed Sep 27 14:25:16 2017
This is a python class that provides functions to read and average all time course files (.csv files) in a directory. 
You can simply run this program, but need to edit the lines after 'if __name__ == "__main__": for your specific conditions'
@author: Ryohei Yasuda
"""


import tkinter as tk
from tkinter import filedialog
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

root = tk.Tk()
root.lift()
root.withdraw()
filename = filedialog.askopenfilename()
df = pd.read_excel(filename, index_col=None,header=0, sheet_name=1)

#fig, ax = plt.subplots(1,1)
#df.plot(kind='scatter', ax=ax, x=0, y=1)
#df.plot(kind='scatter', ax=ax, x=0, y=2, color='red')

values_s133a = df.values[np.logical_and(~np.isnan(df.values[:,0]), ~np.isnan(df.values[:,1]))]
values_s133a_s = values_s133a[values_s133a[:,0].argsort()]
values_s133a_s = values_s133a_s[:,0:2]

nSamples = np.shape(values_s133a_s)[0]
div = 10
nbin = int(nSamples/div+0.5)

all_values = np.zeros((nbin-1,3))
#all_values = np.zeros((nbin,3))

for i in range(nbin-1):
    vals = values_s133a_s[i*div:(i+1)*div]
    photon_mean = np.mean(vals[:,0])
    value_mean = np.mean(vals[:,1])
    value_std = np.std(vals[:,1])
    all_values[i,0] = photon_mean
    all_values[i,1] = value_mean
    all_values[i,2] = value_std

#vals = values_s133a_s[(nbin-1)*div:-1]
#photon_mean = np.mean(vals[:,0])
#value_mean = np.mean(vals[:,1])
#value_std = np.std(vals[:,1])
#all_values[nbin-1,0] = photon_mean
#all_values[nbin-1,1] = value_mean
#all_values[nbin-1,2] = value_std

tauD = 2.6
tauAD = 1.1 
p = np.mean(all_values[:,1])               
tau_m = (p*tauAD**2 + (1-p)*tauD**2)/(p*tauAD + (1-p)*tauD)                
#frac = tauD-tauAD: Linear model.
frac = -tauD*tauAD*(tauAD-tauD)/(tauD+p*(-tauD+tauAD))**2
theory_photons = range(2000,1500000,1000)
theory_stdev = tau_m/np.sqrt(theory_photons) / frac 

fig, ax = plt.subplots(1,1)
ax.plot(all_values[:,0], all_values[:,2], 'ko')
ax.semilogx(theory_photons,theory_stdev)
ax.set_ylim((0,0.08))
ax.set_xlim((0,1500000))

