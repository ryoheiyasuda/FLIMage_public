#!/usr/bin/env python

import socket
import matplotlib.pyplot as plt
import numpy as np
import struct
import time
from datetime import datetime

# open the device on the localhost, port 9999
t = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
t.connect(('127.0.0.1', 9999))

# read the server response
data = t.recv(8192)
print(data.decode('utf8'))

# check if the calibration of the TDCs has completed
command = bytes("T,v,1\n", "utf8")
t.send(command)
data = t.recv(8192)

# if the TDC is not calibrated, execute the calibration command
if data.decode('utf8') == "TDC calibration is invalid":
    command = bytes("T,c,1\n", "utf8")
    t.send(command)
    data = t.recv(8192)
    print(data.decode('utf8'))

start = time.time()
# do a stream of the data
measurement_time_seconds = 5
#command = bytes("SB," + str(measurement_time) + '\n', "utf8")
command = bytes("SB,0" + '\n', "utf8") #Continuous monitoring.
t.send(command)

offset = 0
nChannels = 23
offset_markers = 23

dwell_marker = 0
counts = [0]*nChannels # create a list of int's to get the count rates
timestamps = [[] for _ in range(nChannels)] # create a list of lists for each pixels timestamps

time1 = datetime.now()
meas_done = False

# look for the response
while True:
    time2 = datetime.now()
    
    #This session takes long time --- (every 5 sec, it sends out burst of data)
    data = t.recv(49152*10) # try different buffer sizes with multiples of 6
    
    time_elapsed1 = (datetime.now() - time2).total_seconds()   
    print(f'Time1 = {time_elapsed1}')
    
    time_elapsed_all = (datetime.now() - time1).total_seconds()
    if time_elapsed_all > measurement_time_seconds:
        command = bytes("SB,1\n","utf8") #Just to stop the flow.
        t.send(command)
    
    print(f'Data length = {len(data)/6}')
    if len(data) >= 6:
        for i in range(0, len(data)-6, 6): 
            readdata_pixelnr = data[i+0]
            readdata_coarsecount = int.from_bytes([data[i+1],data[i+2]], 'big')
                        
            if readdata_pixelnr > offset-1 and readdata_pixelnr < offset+nChannels:
                #print(f"Photon {readdata_pixelnr}")
                # readdata_coarsecount = int.from_bytes([data[i+1],data[i+2]], 'big') # coarse counter timestamp with respect to the laser clock
                readdata_finecount = int.from_bytes([data[i+3],data[i+4],data[i+5]], 'big') # fine timestamp in picoseconds with respect to the laser clock
                
                # sum to get the countrate
                counts[readdata_pixelnr-offset] += 1
                
                # store the fine timestamps for pixel 0
                if readdata_pixelnr-offset == 0:
                    timestamps[readdata_pixelnr-offset].append(readdata_finecount)
            
            # elif readdata_pixelnr == offset_markers + 2: #9 --> 25
            #     print('Dwell', readdata_pixelnr)
            # elif readdata_pixelnr == offset_markers + 3: #10 --> 26
            #     print('Line', readdata_pixelnr, readdata_coarsecount)
            #     # this is a dwell marker event
            #     dwell_marker += 1
                
            # elif readdata_pixelnr == offset_markers + 5: #12 --> 28
            #     print('Frame', readdata_pixelnr, readdata_coarsecount)
            #     # this is a line marker event
                
            # # elif readdata_pixelnr == 12:
            #     # this is a frame marker event
            # elif readdata_pixelnr == offset_markers: #55 --> 23
            #     print('Reset -- 0')
                
            # elif readdata_pixelnr == offset_markers + 7: #17 --> 33??
            #     # elif readdata_pixelnr == 17:
            #     # this is a fifo overflow event, one should take care if this happens
            #     print("FIFO overflow")
            # else:
            #     readdata_coarsecount = int.from_bytes([data[i+1],data[i+2]], 'big')
            #     print(readdata_pixelnr, readdata_coarsecount)
    if data[-4:] == bytearray("DONE", 'utf8'):
        
        print("Process complete")
        break
        
    elif data[-5:] == bytearray("ERROR", 'utf8'):
        
        print(data[-160:])
        print("Completed the run with errors")
        break

# close communication channel
t.close()
read = time.time()
print("Read time: ", "{:.2f}".format(read - start), " s")

print("Dwell markers: " + str(dwell_marker))

# print the count rates of all pixels
pix = 0
total_counts = 0
for i in counts:
    print(pix,',',i)
    pix += 1
    total_counts += i

print("Total counts:",total_counts)

# make a histogram from timestamps for pixel 0
time_max = max(timestamps[0])
time_axis = np.arange(0, time_max+10, 10)
histogram = np.zeros(len(time_axis))

for i in timestamps[0]:
    histogram[round(i/10)] += 1

# merge first and last bins
histogram[0] += histogram[-1]

end = time.time()
print("Elapsed time: ", "{:.2f}".format(end - start), " s")

# create a plot to show the histogram
plt.plot(time_axis[:-1], histogram[:-1]) # ignore the last bin
plt.title('Histogram of pixel 0')
plt.xlabel('Time [ps]')
plt.ylabel('Counts [#]')
plt.xlim([0, time_max])
plt.grid(True)
plt.show()