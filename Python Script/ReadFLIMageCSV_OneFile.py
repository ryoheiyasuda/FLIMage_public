# -*- coding: utf-8 -*-
"""
Created on Wed Sep 27 14:25:16 2017
This is a python class that provides functions to read and average all time course files (.csv files) in a directory. 
You can simply run this program, but need to edit the lines after 'if __name__ == "__main__": for your specific conditions'
@author: Ryohei Yasuda
"""

import csv
import os
import tkinter as tk
#from tkinter import ttk
from tkinter import filedialog
import numpy as np
import pandas as pd

import matplotlib.pyplot as plt
from matplotlib.backends.backend_pdf import PdfPages


class ReadFLIMageCSV_OneFile:
    def __init__(self):
        self.outputFilename = 'average.csv'        
        self.outputHeader = 'This file was created by ReadFLIMageCSV'
        self.channelToAnalyze = 1
        self.multiRoi = True
        self.timeKey = 'Time (s)'
        
        self.data = dict()
        self.data_ave = dict()
        self.data_stdev = dict()
        self.data_nPhotons = dict()
        
        self.nPhotons = np.zeros([0])
        self.tau_stdev = np.zeros([0])
        self.tau_average = np.zeros([0])
        
        self.analyzeLifetime = False #or fraction.
    
   
    #Function to read file and to create a dictionary {header : array}
    def loadFLIMageCSV(self, filename):
        with open(filename, newline='') as csvfile:
            CSVreader = csv.reader(csvfile, delimiter=',', quotechar='%')
            data = dict()
            for row in CSVreader:
                if len(row) > 0:
                    headerStr = row[0]
                    if self.outputHeader == row[0]:
                        return -1
                    
                    headerSpl = headerStr.split('-')
                    
                    if len(headerSpl) == 3:
                        self.multiRoi = True
                    else:
                        self.multiRoi = False
                                        
                    #If there is nothing, we will shave it.
                    endLine = len(row)
                    for j in range(5):  
                        if row[endLine - 1] == '':
                            endLine = endLine - 1
                        else:
                            break
                        
                    if headerStr.startswith('Intensity'):
                        headerStr = 'mean' + headerStr
                    
                    if endLine > 1 and (headerStr != 'Select-ROI' and headerStr != 'Multi-ROI' and '-' in headerStr or self.timeKey == headerStr):
                        #Change values to float array.
                        try:
                            valueStrs = row[1:endLine]
                            values = np.array([float(x) for x in valueStrs])
                            data.update({headerStr:values})
                        except:
                            print(headerStr + ': Error')
            self.data = data
            return data
        
    def GetRoiChNum(self, keyStr):
        spStr = keyStr.split('-')
        roiN = 0
        chN = 0
        for str1 in spStr:
            if 'ROI' in str1:
                roiN = int(str1[3:])
            elif 'ch' in str1:
                chN = int(str1[2:])
        return [roiN, chN]
                    
    def PhotonNumberCalc(self):
        self.data_ave = dict()
        self.data_stdev = dict()
        self.data_nPhotons = dict()
        self.data_nPhotonsA = dict()
        self.data_valuesA = dict()
        maxROI = 0
        
        for key in self.data:
            [roiN, chN] = self.GetRoiChNum(key)
            maxROI = max(roiN,maxROI)
            
            if chN == self.channelToAnalyze:
                d_ave = np.mean(self.data[key])
                d_stdev = np.std(self.data[key])

                if 'sumIntensity' in key:
                    self.data_nPhotons.update({'ROI'+str(roiN):d_ave})
                    self.data_nPhotonsA.update({'ROI'+str(roiN):self.data[key]})
                    nBin = len(self.data_nPhotonsA['ROI'+str(roiN)])
                 
                if self.analyzeLifetime:
                    str1 = 'Lifetime_fit'
                else:
                    str1 = 'Fraction2_fit'
                
                if str1 in key:
                    self.data_ave.update({'ROI'+str(roiN):d_ave})
                    self.data_stdev.update({'ROI'+str(roiN):d_stdev})
                    self.data_valuesA.update({'ROI'+str(roiN):self.data[key]})
        
        self.nPhotons = np.zeros([maxROI,1])
        self.tau_average = np.zeros([maxROI,1])
        self.tau_stdev = np.zeros([maxROI,1])
        self.nPhotonsA = np.zeros([maxROI, nBin])
        self.valuesA = np.zeros([maxROI, nBin])
        for key in self.data_nPhotons:
            [roiN, chN] = self.GetRoiChNum(key)
            #print(roiN, maxROI)
            self.nPhotons[roiN-1,0] = self.data_nPhotons[key]
            self.tau_average[roiN-1,0] = self.data_ave[key]
            self.tau_stdev[roiN-1,0] = self.data_stdev[key]
            self.nPhotonsA[roiN-1,:] = self.data_nPhotonsA[key]
            self.valuesA[roiN-1,:] = self.data_valuesA[key]

def makeColorDict():
    colorDict = dict()
    colorDict.update({'10':'violet'})
    colorDict.update({'20':'blue'})
    colorDict.update({'40':'green'})
    colorDict.update({'80':'lime'})  
    colorDict.update({'160':'orange'})
    colorDict.update({'290':'red'})
    colorDict.update({'580':'black'})
    colorDict.update({'590':'black'})
    return colorDict

if __name__ == "__main__":
    root = tk.Tk()
    root.lift()
    root.withdraw()
    
    filename = filedialog.askopenfilename() # show an "Open" dialog box and return the path to the selected file
    #print(filename)
    mypath = os.path.split(filename)[0]
    fname = os.path.split(filename)[1]
    fcore = fname.find('TimeCourse')
    fcorename = fname[:fcore+10]
    
    outfilename = os.path.join(mypath, 'allfiles_plot.pdf')
    outfilename2 = os.path.join(mypath, 'allfiles_plot_pop.pdf')

    #files = [f for f in os.listdir(mypath) if (os.path.isfile(os.path.join(mypath, f)) & f.startswith(fcorename) & f.endswith('csv'))]
    files = [f for f in os.listdir(mypath) if (os.path.isfile(os.path.join(mypath, f)) & ('TimeCourse' in f) & f.endswith('csv'))]
    basenames = dict()
    for file in files:
        basename = file.split('_')[0]
        basenames.update({basename:1})
    basenames = list(basenames.keys())

    av = ReadFLIMageCSV_OneFile()
    av.channelToAnalyze = 1
    av.sampleLength = 1400 #Cut off for the sample length. Put very large value if you don't want to cut the data.
        
    maxPhotons = np.zeros((len(basenames),1))
    minPhotons = np.zeros((len(basenames),1))
    data_val = np.zeros((len(basenames),1))
    colorDict = makeColorDict()
    
    with PdfPages(outfilename) as export_pdf:        
#        fig = plt.figure(figsize = plotsize, dpi=windowResolution)
#        ax = fig.add_subplot(1,1,1)
        fig = plt.figure(figsize = (8,4), dpi=300)
        ax = fig.add_subplot(1,2,1)
        legendStr = dict()
        for j in range(len(basenames)):
            files = [f for f in os.listdir(mypath) if (os.path.isfile(os.path.join(mypath, f)) & f.startswith(basenames[j]) & f.endswith('csv'))]        
           
            for i in range(len(files)):
                fileName = os.path.join(mypath, files[i])
                av.loadFLIMageCSV(fileName)
                fileNameWithoutExt = files[i].split('.')[0]
                aveFrameStr = fileNameWithoutExt.split('TimeCourse')[1]
                if aveFrameStr != '590':
                    av.PhotonNumberCalc()
                    if i == 0:
                        nPhotons = np.zeros([len(av.nPhotons), len(files)])
                        data_average = np.zeros([len(av.tau_average), len(files)])
                        data_stdev = np.zeros([len(av.tau_stdev), len(files)])
                    nPhotons[:,i] = av.nPhotons[:,0]
                    data_average[:,i] = av.tau_average[:,0]
                    data_stdev[:,i] = av.tau_stdev[:,0]                
                    h,=ax.plot(nPhotons[:,i], data_stdev[:,i], '.', color=colorDict[aveFrameStr])
                    if j == 0:
                       legendStr.update({int(aveFrameStr):h})
                
            maxPhotons[j] = np.ceil(np.max(nPhotons))
            minPhotons[j] = np.floor(np.min(nPhotons))
                        
            if av.analyzeLifetime:
                data_val[j] = np.mean(data_average)
            else:
                data_val[j] = np.mean(data_average)                
                
        #Calclate theoretical noise.
        #theory_photons = range(int(min(minPhotons)),int(max(maxPhotons)),100)
        theory_photons = range(1000, 2000000, 1000)
        if av.analyzeLifetime:
            theory_stdev = np.mean(data_val)/np.sqrt(theory_photons)
        else:
            tauD = 2.6
            tauAD = 1.1 
            p = np.mean(data_val)               
            tau_m = (p*tauAD**2 + (1-p)*tauD**2)/(p*tauAD + (1-p)*tauD)                
            #frac = tauD-tauAD: Linear model.
            frac = -tauD*tauAD*(tauAD-tauD)/(tauD+p*(-tauD+tauAD))**2
            theory_stdev = tau_m/np.sqrt(theory_photons) / frac 
        
        #Plot theoreticla line
        ax.semilogx(theory_photons, theory_stdev, '-', color='black')          
        ax.set_xlabel('Number of photons')
        ax.set_ylabel('Standard deviation')
        
        #Legend 
#        legendStr1 = dict()
#        for k, v in sorted(legendStr.items(), key=lambda x: x[0]):
#            legendStr1.update({k:v})
#        ax.legend(legendStr1.values(), legendStr1.keys())
        
        ax.set_title('Frame-to-frame variability')
        ax.set_ylim((0,0.085))
        ax.set_xlim((1000,2000000))
#        export_pdf.savefig()
#        plt.close()
    
    #with PdfPages(outfilename2) as export_pdf:
        ax2=fig.add_subplot(1,2,2)
        legendStr = dict()
        for j in range(len(basenames)):
            files = [f for f in os.listdir(mypath) if (os.path.isfile(os.path.join(mypath, f)) & f.startswith(basenames[j]) & f.endswith('csv'))]        
           
            for i in range(len(files)):    
                fileName = os.path.join(mypath, files[i])
                av.loadFLIMageCSV(fileName)
                fileNameWithoutExt = files[i].split('.')[0]
                aveFrameStr = fileNameWithoutExt.split('TimeCourse')[1]
                av.PhotonNumberCalc()
                if i == 0:
                    nPhotons = np.zeros([1, len(files)])
                    data_average = np.zeros([1, len(files)])
                    data_stdev = np.zeros([1, len(files)])
                    nPhotons_error = np.zeros([1,len(files)])
                    data_stdev_error = np.zeros([1,len(files)])
                nPhotons[:,i] = np.mean(np.mean(av.nPhotonsA,axis=0))
                nPhotons_error[:,i] = np.std(np.mean(av.nPhotonsA,axis=1))
                data_average[:,i] = np.mean(np.mean(av.valuesA,axis=0))
                data_stdev[:,i] = np.mean(np.std(av.valuesA,axis=0))
                data_stdev_error[:,i] = np.std(np.std(av.valuesA,axis=0))
                h2,=ax2.plot(nPhotons[:,i], data_stdev[:,i], 'o', color=colorDict[aveFrameStr])
                ax2.errorbar(nPhotons[:,i], data_stdev[:,i], xerr=nPhotons_error[:,i], color=colorDict[aveFrameStr], linewidth=0.1)
                if j == 0:
                   legendStr.update({int(aveFrameStr):h2})
                
                        
            if av.analyzeLifetime:
                data_val[j] = np.mean(data_average)
            else:
                data_val[j] = np.mean(data_average)      
                
                
        ax2.semilogx(theory_photons, theory_stdev, '-', color='black')          
        ax2.set_xlabel('Number of photons')
        #ax2.set_ylabel('Standard deviation')
        
        legendStr1 = dict()
        for k, v in sorted(legendStr.items(), key=lambda x: x[0]):
            legendStr1.update({k:v})
        ax.legend(legendStr1.values(), legendStr1.keys())
        
        ax2.set_title('Cell-to-cell variability')
        ax2.set_ylim((0,0.085))
        ax2.set_xlim((1000,2000000))
        
        export_pdf.savefig()
        plt.close()
        
    os.startfile(outfilename)
