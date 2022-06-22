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

class ReadFLIMageCSV:
    def __init__(self):
        self.directoryPath = ''
        self.allFiles = []
        self.outputFilename = 'average.csv'
        self.files = []
        self.nFiles = 0
        
        self.outputHeader = 'This file was created by ReadFLIMageCSV'
        self.multiRoi = True
        self.timeKey = 'Time (s)'

        self.data_all = tuple()
        self.data_all2 = tuple()
        self.lenArray = []
        
        self.data_ave = dict()
        self.data_all_pd = dict()
        self.nSamples = dict()
        self.dataLength = 0
        
        self.data_sem = dict()
        self.data_save = dict()
    
        self.NormalizeWithBaseLine = True #For FLIM, it is subtraction.
        self.baseLineStart = 0
        self.baseLineEnd = 31
        self.sampleLength = 1400 #Cut off for the sample length. Put very large value if you don't want to cut the data.
        self.if_divide_with_ROI = False #If you want to normalize intensity with dendrite, put True.
        self.divide_with_ROI = 'ROI2' #ROI2 can be dendritic intensity, for example.

    def loadDataAndCalculateAverage(self, directoryPath):
        self.createDataArray(directoryPath)
        self.AverageData()
        self.CalculateSEM()
        self.SaveFiles()
    
        
    def createDataArray(self, directoryPath):
        #First, we calculate the first data to get the header (key)
        self.directoryPath = directoryPath;        
        self.data_all = tuple()
        self.getFileNames()
        
        self.nFiles = len(self.files)
        self.lenArray = [int(0)] * self.nFiles #Initialize lenArray
        maxLength = -1
        
        for i in range(self.nFiles):
            f = self.files[i]
            filename = os.path.join(directoryPath, f)
            data1 = self.loadFLIMageCSV(filename)
            if data1 != -1:
                data_length = len(data1[self.timeKey])
                if  data_length> maxLength:
                    maxLength = data_length
            
                if maxLength > self.sampleLength:
                    maxLength = self.sampleLength
                
                if data_length > self.sampleLength:
                    data_length = self.sampleLength
                    
                self.data_all = self.data_all + (data1,)
                self.lenArray[i] = data_length
                
                self.dataLength = maxLength;
    
                print(f + ': data length = ' + str(data_length))
            
        
    def getFileNames(self):
        self.files = [f for f in os.listdir(self.directoryPath) if (os.path.isfile(os.path.join(self.directoryPath, f)) & f.endswith('csv') & (f != self.outputFilename))]
    
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
                            meanValues = np.mean(values[self.baseLineStart:self.baseLineEnd])
                                      
                            if self.NormalizeWithBaseLine:
                                if 'Intensity' in headerStr:
                                    values = values / meanValues
                                elif headerStr.startswith('Lifetime'):
                                    values = values - meanValues
                                elif headerStr.startswith('Fraction2'):
                                    values = values - meanValues
                                    
                            data.update({headerStr:values})
                        except:
                            print(headerStr + ': Error')
            return data
           
    def AverageData(self):
        #Number of averaged trace for each key.    
        self.data_ave = dict()
        self.data_all_pd = dict()
        self.nSamples = dict()
        
        for i in range(len(self.data_all)):
            for key in self.data_all[i]:
                self.data_all_pd.update({key:pd.DataFrame(np.nan * np.zeros((self.nFiles, self.dataLength)))})
        
        #This has to be separated loop!
        for i in range(len(self.data_all)):
            for key in self.data_all[i]:
                data_length = self.lenArray[i]
                self.data_all_pd[key].iloc[i, 0:data_length] = self.data_all[i][key][0:data_length]

        #Divide with dendrite, if necessary.        
        if self.multiRoi and self.if_divide_with_ROI:           
            for key in self.data_all_pd:
                if 'Intensity' in key:  #Both for sumIntensity and meanIntenstiy
                    headers = key.split('-')
                    if headers[1] != self.divide_with_ROI:
                        keyRef = headers[0] + '-' + self.divide_with_ROI + '-' +  headers[2]
                        #print(key + ', ' + keyRef)
                        self.data_all_pd[key] = self.data_all_pd[key] / self.data_all_pd[keyRef]
            
            #Then put one to dendrite.
            for key in self.data_all_pd:
                if 'Intensity' in key:  #Both for sumIntensity and meanIntenstiy
                    headers = key.split('-')
                    if headers[1] == self.divide_with_ROI:
                        self.data_all_pd[key] = np.ones(len(self.data_all_pd[key]))
                        
        #Divide with the number.         #Longest
        for key in self.data_all_pd:
            self.data_ave.update({key: self.data_all_pd[key].mean(axis=0)})
            self.nSamples.update({key:self.data_all_pd[key].count(axis=0)})
        
        
    def CalculateSEM(self):
        self.data_sem = dict()
          
        semText = ' sem'
        
        for key in self.data_ave:
            if key != self.timeKey:
                keysem = key + semText
                semArray = self.data_all_pd[key].sem(axis=0)
                self.data_sem.update({keysem : semArray})
        
    def SaveFiles(self):
        numText = '#Sample'
        self.data_save = dict()
        for key in self.data_ave:
            if key != self.timeKey:
                self.data_save.update({key + ' mean' : self.data_ave[key]})
            else:
                self.data_save.update({key : self.data_ave[key]})
        
        for keysem in self.data_sem:
            self.data_save.update({keysem : self.data_sem[keysem]})
            
        for key in self.nSamples:
            if key != self.timeKey:
                self.data_save.update({key + numText : self.nSamples[key]})
        
        for key in self.data_all_pd:
            if key != self.timeKey:
                for i in range(len(self.data_all_pd[key])):
                    filename = os.path.splitext(os.path.basename(self.files[i]))[0]
                    self.data_save.update({key + "-file " + filename : self.data_all_pd[key].iloc[i]})
                
        self.SaveAverage()
        print ('Saved in file: ' + self.outputFilename)

    #Function to save average data in csv.
    def SaveAverage(self):
        outputFilePath = os.path.join(self.directoryPath, self.outputFilename)
        with open(outputFilePath, 'w', newline='') as csvfile:
            CSVwriter = csv.writer(csvfile, delimiter=',', quotechar='%')
            keylist = self.data_save.keys()
            keylist1 = [self.timeKey,]
            keylist1 = keylist1 + sorted([x for x in keylist if 'Intensity-' in x])
            keylist1 = keylist1 + sorted([x for x in keylist if x.startswith('Lifetime-')])
            keylist1 = keylist1 + sorted([x for x in keylist if x.startswith('Fraction2-')])
            keylist1 = keylist1 + sorted([x for x in keylist if x.startswith('Lifetime_fit-')])
            keylist1 = keylist1 + sorted([x for x in keylist if x.startswith('Fraction2_fit-')])
            
            CSVwriter.writerow([self.outputHeader])
            for key in keylist1:
                datastr = [key]
                for value in self.data_save[key].values:
                    if ~np.isnan(value):
                        datastr.append(value)
                CSVwriter.writerow(datastr)
                

if __name__ == "__main__":
    root = tk.Tk()
    root.lift()
    root.withdraw()
    
    filename = filedialog.askopenfilename() # show an "Open" dialog box and return the path to the selected file
    print(filename)
    mypath = os.path.split(filename)[0]
    
    av = ReadFLIMageCSV()
    av.outputFilename = 'average.csv' #Will be saved in the same folder with this name.
    av.NormalizeWithBaseLine = True #For FLIM, it is subtraction.
    av.baseLineStart = 0
    av.baseLineEnd = 5
    av.sampleLength = 1400 #Cut off for the sample length. Put very large value if you don't want to cut the data.
    av.if_divide_with_ROI = False #If you want to normalize intensity with dendrite, put True.
    av.divide_with_ROI = 'ROI2' #ROI2 can be dendritic intensity, for example.   
    av.loadDataAndCalculateAverage(mypath)
    
   
