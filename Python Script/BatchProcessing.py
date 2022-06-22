# -*- coding: utf-8 -*-
"""
Example of batch processing.
Use FLIM_PipeClient to process.

Parent folder includes subfolders that contains data for each condition.
Each folder includes ROI files (.zip) and FLIM files (.flim)
From the files in the folder of 590-599 frames, this program will bin 
frames with 10,20,40,80,160,290,580 frames, and calculte time course. 
It is useful for noise analysis.

Before using this program, you have to open FLIMage and then open "Remote 
controls" window (FLIMage->Tools->Remote Controls)
Turn on PIPE by clicking "Connect with client through PIPE"

Used for Tal Laviv et al., 2019 (in revision)

Written by Ryohei Yasuda
8/3/2019.
"""

import os
import tkinter as tk
from tkinter.filedialog import askopenfilename

from FLIM_pipeClient import FLIM_Com
from FLIM_pipeClient import FLIM_message_received

#Call PIPE program.
#When this script is called multiple times, previous thread needs to be killed.
if 'flim' in globals():
    if not flim.Connected:
        flim.start()
else:
    flim = FLIM_Com()
    flim.start()    
    if flim.Connected:
        flim.messageReceived += FLIM_message_received #Add your function to  handle.
        flim.startReceiving()

            
#End Calling.

mutant = True

if mutant:
    ParentFolder = r'C:\Users\yasudar\Dropbox\GRANT_AND_PAPER\CREB paper\New version 2019\Tal_NoiseData\raw files for day 1-2 data\S133A'
else:
    ParentFolder = r'C:\Users\yasudar\Dropbox\GRANT_AND_PAPER\CREB paper\New version 2019\Tal_NoiseData\raw files for day 1-2 data\wt'

folders = [f for f in os.listdir(ParentFolder) if (f != 'Analysis') and ('.' not in f)]

#analyze = ['M172L4']
#folders = [f for f in os.listdir(ParentFolder) if (f != 'Analysis') and ('.' not in f) \
#           and (f in analyze)]

fixTau = [2.6,1.1]
fitRange = [10,60]
lowThresh = 1.5 #For frame = 1. Calculation start with frame = 10.
ChannelToAnalyze = 1 #1 or 2

#Functions
def binFileAndCalcTimeCourse(aveF, j, folderPath, filename):
    tcfileName = filename.split('.')[0][0:-3] + '_TimeCourse.csv'
    tcfileName2 = filename.split('.')[0][0:-3] + '_TimeCourse' + str(aveF) + '.csv'
    tcFile = os.path.join(folderPath, 'Analysis', tcfileName)
    tcFile2 = os.path.join(folderPath, 'Analysis', tcfileName2)  
      
    if os.path.isfile(tcFile2):
        os.remove(tcFile2)

    flim.sendCommand(f'BinFrames,{j}') #First average...
    flim.sendCommand(f'SetFLIMIntensityOffset,{lowThresh*aveF},{lowThresh*2*aveF}')
    flim.sendCommand('FixTauAll, 0') #Unfix all tau values first.
    flim.sendCommand(f'FixTau,{fixTau[0]},{fixTau[1]}') #Fix tau1 and tau2.
    flim.sendCommand(f'SetFitRange,{fitRange[0]},{fitRange[1]}') #Set Fit range.
    flim.sendCommand('ApplyFitOffset') #Will fit and apply offset. Not necessary.
    flim.sendCommand('FitEachFrame, 1') #For time course, it will fit every frame.
    flim.sendCommand('CalcTimeCourse') 
    
    os.rename(tcFile, tcFile2)

def calculateFolder(folderPath):
    files = [f for f in os.listdir(folderPath) if (os.path.isfile(os.path.join(folderPath, f)) & \
                               f.endswith('.flim'))]
    flim.sendCommand(f'SetChannel,{ChannelToAnalyze}') # + str(ChannelToAnalyze))
    for f in files:
        fullFileName = os.path.join(folderPath, f)
        filesROI = f.split('.')[0] + '_ROI_ImageJ.zip'
        fullRoiFileName = os.path.join(folderPath, filesROI)
    
        #10->20->40->80->160, we will do it in sequence.
        flim.sendCommand(f'ReadImageJROI,{fullRoiFileName}')
        flim.sendCommand(f'SetFLIMIntensityOffset,{lowThresh},{lowThresh*2}') 
        flim.sendCommand(f'OpenFile,{fullFileName}')
        page = int(flim.sendCommand('GetNPages').split(',')[1])
        print(f'page number = {page}\n', end="")
        aveF = 1    
        for j in [10,2,2,2,2]: #10,20,40,80,160
            aveF = aveF*j #10,20,40,80,160. More efficient than open everytime.
            binFileAndCalcTimeCourse(aveF, j, folderPath, f)
    
        #For 290 and 580, we will do it again.
        flim.sendCommand(f'SetFLIMIntensityOffset,{lowThresh},{lowThresh*2}') 
        flim.sendCommand(f'OpenFile,{fullFileName}')
        aveF = 1
        for j in [290,2]: #290, 580
            aveF = aveF*j
            binFileAndCalcTimeCourse(aveF, j, folderPath, f)
    
#main function..
            
for folder in folders:
    calculateFolder(os.path.join(ParentFolder, folder))
