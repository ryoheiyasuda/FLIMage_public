# -*- coding: utf-8 -*-
"""
Created on Fri Jan  5 15:17:54 2024

Note:
Use FLIMage version 4.0.42 or later
Recommend Turning OFF "Keep images in memory" in "Channel" tab

@author: yasudalab
"""

'''
Here you can put multiple folders anb basenames. 
All of them will be analyzed sequentially.
'''

folders = [r'C:\path\to\your\FLIMage\data', ]
file_basenames = ['test',] * len(folders)
number_of_binning = 32



from FLIM_pipeClient import FLIM_Com
import time

flim = FLIM_Com()
flim.start()
    
if not flim.Connected:
    flim.close()
    flim.start()
    
if not flim.Connected:
    print('Connection with FLIMage failed. Reestablish the connection:')
    print('Click off and on the "Enable PIPE server (listening)" check box in the "Remote control & Script window"')
    print('The "Remote control & Script window" can be opened from FLMage main window -> Tools -> Remote control')
    
for i, folder in enumerate(folders):
    
    print(f'*************Working on folder {folder}')
    print( '*************')    
    version = flim.sendCommand('GetVersion').split(', ')[1] #You can send command to FLIMage 
    version_a = version.split('.')
    version_n = int(version_a[0])*1000 + int(version_a[1])*100 + int(version_a[2])
    if version_n < 4042:
        'This script works only with a FLIMage version higher than 4.0.42'
    
    
    basename_fullpath = folder + '/' + file_basenames[i]
    full_first_filename = basename_fullpath + '001.flim'
    concat_file_name = basename_fullpath + '_concat.flim'
    
    alignfilename = basename_fullpath + '_align.flim'
    bin_file = basename_fullpath + f'_concat_align_bin{number_of_binning}.flim'
    
    flim.sendCommand(f'OpenFile, {full_first_filename}')
    flim.sendCommand(f'ConcatenateImages, {concat_file_name}')
    print('Concatenating: ')
    while True:
        reply = flim.sendCommand('GetAnalysisStatus')
        time.sleep(5)
        if 'None' in reply:
            print('Concat done')
            break
        else:
            print('*', end='')
    
    print(f'Allining frames.. {alignfilename}')
    flim.sendCommand(f'OpenFile, {concat_file_name}')
    flim.sendCommand('AlignFrames, {alignfilename}')  
    
    print(f'Binning frames.. {bin_file}')
    flim.sendCommand(f'OpenFile, {alignfilename}')
    flim.sendCommand(f'BinFrames,{number_of_binning}, {bin_file}')
