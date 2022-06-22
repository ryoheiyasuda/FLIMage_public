# -*- coding: utf-8 -*-
"""
Created on Sat Aug  3 23:29:02 2019
Created for Tal.
Folder structure needs to be:
Use this after BatchProcessing.py
ParentFolder - Experiment (containing .flim and .cvs.) - Analysis (containing csv files)

@author: yasudar
"""
import os
import tkinter as tk
#from tkinter import ttk
from tkinter import filedialog
import numpy as np
import pandas as pd

import matplotlib.pyplot as plt
from matplotlib.backends.backend_pdf import PdfPages

from ReadFLIMageCSV_OneFile import ReadFLIMageCSV_OneFile
from ReadFLIMageCSV_OneFile import makeColorDict #For legend.


mutant = False
if mutant:
    ParentFolder = r'C:\Users\yasudar\Dropbox\GRANT_AND_PAPER\CREB paper\New version 2019\Tal_NoiseData\raw files for day 1-2 data\S133A'
else:
    ParentFolder = r'C:\Users\yasudar\Dropbox\GRANT_AND_PAPER\CREB paper\New version 2019\Tal_NoiseData\raw files for day 1-2 data\wt'

exclude = ['M173L3'] #put weird data here.
#exclude = []
folders = [f for f in os.listdir(ParentFolder) if (f != 'Analysis') & (not '.' in f) & \
          (f.startswith('M')) & (f not in exclude)]
#nTimePoints = 2
#nAvePoints = 7 #10,20,40,80,160,290,580/590
#aveList = [10,20,40,80,160,290,580]
#nAve = {10:0,20:1,40:2,80:3,160:4,290:5,590:6,580:6}
nFolders = len(folders)

colorDict = makeColorDict()
av = ReadFLIMageCSV_OneFile()
av.channelToAnalyze = 1
av.sampleLength = 1400 #Cut off for the sample length. Put very large value if you don't want to cut the data.

nPhotonsAll = list()
ValuesAll = list()

for folder in folders:
    nPhotons1 = dict()
    Values1 = dict()

    path1 = os.path.join(ParentFolder, folder, 'Analysis')
    files = [f for f in os.listdir(path1) if (os.path.isfile(os.path.join(path1, f)) \
                                   & ('TimeCourse' in f) & f.endswith('csv'))]
    basenames = dict()
    for file in files:
        basename = file.split('_')[0]
        basenames.update({basename:1})
    basenames = list(basenames.keys())

    for i in range(len(files)):
        basename = files[i].split('_')[0]
        baseIndex = basenames.index(basename)
        fileNameWithoutExt = files[i].split('.')[0]
        aveFrame = int(fileNameWithoutExt.split('TimeCourse')[1])
        fileName = os.path.join(path1, files[i])
        av.loadFLIMageCSV(fileName)
        av.PhotonNumberCalc()
        
        if not aveFrame in nPhotons1.keys():
            nROI = np.shape(av.nPhotonsA)[0]
            nTimePoints = np.shape(av.nPhotonsA)[1]
            shape1 = [len(basenames), nROI, nTimePoints]
            nPhotons1.update({aveFrame:np.zeros(shape1)})          
            Values1.update({aveFrame:np.zeros(shape1)})
            nPhotons1[aveFrame][baseIndex, :, :] = av.nPhotonsA
            Values1[aveFrame][baseIndex, :, :] = av.valuesA
        else:
            nTimePoints = np.shape(av.nPhotonsA)[1]
            if nTimePoints == np.shape(nPhotons1[aveFrame])[2]:
                nPhotons1[aveFrame][baseIndex, :, :] = av.nPhotonsA
                Values1[aveFrame][baseIndex, :, :] = av.valuesA
            else:
                nTimePoints = min(nTimePoints, np.shape(nPhotons1[aveFrame])[2])
                shape1 = [len(basenames), nROI, nTimePoints]
                nPhotons1[aveFrame] = nPhotons1[aveFrame][:,:,0:nTimePoints]
                Values1[aveFrame] = Values1[aveFrame][:,:,0:nTimePoints]                
                nPhotons1[aveFrame][baseIndex, :, :] = av.nPhotonsA[:,0:nTimePoints]
                Values1[aveFrame][baseIndex, :, :] = av.valuesA[:,0:nTimePoints]

    nPhotons2 = list()
    Values2 = list()
    for key in nPhotons1:
        nPhotons2.append(nPhotons1[key])
        Values2.append(Values1[key])
        
    nPhotonsAll.append(nPhotons2)
    ValuesAll.append(Values2)        

avePoints = list(nPhotons1.keys())
#nPhotonsAll[folder][average][file, Roi, timepoint]
    
#Calclate theoretical noise.
#theory_photons = range(int(min(minPhotons)),int(max(maxPhotons)),100)
theory_photons = range(1000, 2000000, 1000)
tauD = 2.6
tauAD = 1.1 
n_valuesA = list()
for ave in range(len(ValuesAll[0])):
    for fol in range(len(ValuesAll)):            
        val = np.mean(np.mean(np.mean(ValuesAll[fol][ave],axis=0),axis=0),axis=0)
        n_valuesA.append(val)
p = np.mean(n_valuesA)               
tau_m = (p*tauAD**2 + (1-p)*tauD**2)/(p*tauAD + (1-p)*tauD)                
frac = -tauD*tauAD*(tauAD-tauD)/(tauD+p*(-tauD+tauAD))**2
theory_stdev = tau_m/np.sqrt(theory_photons) / frac 

yrange = [0,0.085]
xrange = [1000,2000000]

outfilename = os.path.join(ParentFolder, 'allfiles_plot.pdf');
with PdfPages(outfilename) as export_pdf:        
    fig = plt.figure(figsize = (10,4), dpi=300)
    ax = fig.add_subplot(1,3,1)
    legendStr = dict()
    #First frame to frame variability.
    for ave in range(len(nPhotonsAll[0])):
        for fol in range(len(nPhotonsAll)):            
            nAverage = avePoints[ave]
            n_photons = np.mean(np.mean(nPhotonsAll[fol][ave],axis=2),axis=0) #timepoints --> file
            std_values = np.sqrt(np.mean(np.var(ValuesAll[fol][ave],axis=2,ddof=1),axis=0)) #timepoints --> file
            mean_values = np.mean(np.mean(ValuesAll[fol][ave],axis=2),axis=0) #timepoints --> file
            h,=ax.plot(n_photons, std_values, '.', markersize=1,color=colorDict[str(nAverage)])
            if fol == 0:
                legendStr.update({nAverage:h}) 
    
    #Legend 
    legendStr1 = dict()
    for k, v in sorted(legendStr.items(), key=lambda x: x[0]):
        legendStr1.update({k:v})

    ax.semilogx(theory_photons, theory_stdev, '-', color='black')          
    leg = ax.legend(legendStr1.values(), legendStr1.keys())
    for i in range(len(leg.legendHandles)):
        leg.legendHandles[i]._legmarker.set_markersize(6)
    ax.set_xlabel('Number of photons')
    ax.set_ylabel('Standard deviation')
    ax.set_title('Frame-to-frame variability')
    ax.set_ylim(yrange)
    ax.set_xlim(xrange)
    
    data_cell = list()
    ax2 = fig.add_subplot(1,3,2)
    #Second cell-to-cell variability
    for ave in range(len(nPhotonsAll[0])):
        for fol in range(len(nPhotonsAll)):
            nAverage = avePoints[ave]
            n_photons = np.mean(np.mean(nPhotonsAll[fol][ave],axis=1),axis=1) #roiN --> timepoints --> file
            std_values = np.sqrt(np.mean(np.var(ValuesAll[fol][ave],axis=1,ddof=1),axis=1)) #roiN --> timepoints --> file
            mean_values = np.mean(np.mean(ValuesAll[fol][ave],axis=1),axis=1) #roiN --> timepoints --> file
            if nAverage > 550:
                for val in std_values:
                    data_cell.append(val)

            ax2.plot(n_photons, std_values, '.',color=colorDict[str(nAverage)])

    ax2.semilogx(theory_photons, theory_stdev, '-', color='black')          
    #ax2.legend(legendStr1.values(), legendStr1.keys())
    ax2.set_xlabel('Number of photons')
    #ax.set_ylabel('Standard deviation')
    ax2.set_title('Cell-to-cell variability')
    ax2.set_ylim(yrange)
    ax2.set_xlim(xrange)
    
    data_day = list()
    largeVarList = list()
    ax3 = fig.add_subplot(1,3,3)
    #Second cell-to-cell variability
    for ave in range(len(nPhotonsAll[0])):
        for fol in range(len(nPhotonsAll)):
            nAverage = avePoints[ave]
            n_photons = np.mean(np.mean(nPhotonsAll[fol][ave],axis=0),axis=1) #file -->timepoints
            std_values = np.sqrt(np.mean(np.var(ValuesAll[fol][ave],axis=0,ddof=1),axis=1)) #file --> timepoints
            #std_values = np.std(ValuesAll[fol][ave],axis=0,ddof=1)[:,0]
            np.set_printoptions(precision=3)
            if nAverage>550:
                for val in std_values:
                    data_day.append(val)
                roi = np.array(np.where(std_values > 0.02))[0]
                largeVarList.append(f'{folders[fol]}, ROI{roi+1}, Value{std_values[roi]}')
            np.set_printoptions(precision=8)
            mean_values = np.mean(np.mean(ValuesAll[fol][ave],axis=0),axis=1) #file --> timepoints
            ax3.plot(n_photons, std_values,'.',markersize=1,color=colorDict[str(nAverage)])

    ax3.semilogx(theory_photons, theory_stdev, '-', color='black')          
    #ax3.legend(legendStr1.values(), legendStr1.keys())
    ax3.set_xlabel('Number of photons')
    #ax.set_ylabel('Standard deviation')
    ax3.set_title('Day-to-day variability')
    ax3.set_ylim(yrange)
    ax3.set_xlim(xrange)

    export_pdf.savefig()
    plt.close()

print('Data with large day-to-day variability:')
for data1 in largeVarList:
    print(f'{data1}')
print(f'Total N of Cells = {len(data_day)}')

os.startfile(outfilename)
