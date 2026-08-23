#!/usr/bin/env python

import sys
import socket
import matplotlib.pyplot as plt
import numpy as np
import time

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
    
# align the channels to the laser
#command = bytes("T,a,1000\n", "utf8")
#t.send(command)
#data = t.recv(2097152)
#print(data.decode('utf8'))

start = time.time()
# do a stream of the data
measurement_time = 1000 # time in ms
command = bytes("S," + str(measurement_time) + '\n', "utf8")
t.send(command)

datastr = ""
# look for the response
while 1:
    data = t.recv(2097152) #32768 #65536 # try different buffer sizes
    # process the data, its format is 
    # pixelnr, coarse timestamp, fine timestamp (if TDC system) || marker+224, coarse timestamp
    datastr += data.decode('utf8') # concatenate the strings from the measurement data
    
    # cancel the measurement at any time, by sending the previous command again
    #t.send(command)
        
    if data[-4:] == bytearray("DONE", 'utf8'):
        
        print("Process complete")
        break
        
    elif data[-5:] == bytearray("ERROR", 'utf8'):
        
        print(data[-160:])
        print("Completed the run with errors")
        quit()
    

# close communication channel, get read time
t.close()
read = time.time()
print("Read time: ", "{:.2f}".format(read - start), " s")

# remove DONE from the end of the data
data = data[:-4]

# write the data to a file for reference
# f = open('Stream_text.txt', 'w')
# f.write(datastr)
# f.close()

# split the data by newlines

offset = 0

datasplit = datastr.split("\n")
datasplit.pop(-1) # remove last item "DONE"
counts = [0]*23 # create a list of int's to get the count rates
timestamps = [[] for _ in range(23)] # create a list of lists for each pixels timestamps
for x in datasplit:
    # split each string into two or three parts (depends on if it is a TDC system or not)
    readdata = x.split(",")
    readdata_pixelnr = int(readdata[0])
    if readdata_pixelnr >= offset and readdata_pixelnr < 23 + offset:
        # this is data for the normal pixels
        # readdata_coarsecount = int(readdata[1]) # coarse counter timestamp with respect to the laser clock
        readdata_finecount = int(readdata[2]) # fine timestamp in picoseconds with respect to the laser clock
        
        # sum to get the countrate
        counts[readdata_pixelnr-offset] += 1
        
        # store the fine timestamps for pixel 0
        if readdata_pixelnr-offset == 0:
            timestamps[readdata_pixelnr-offset].append(readdata_finecount)
        
    # elif readdata_pixelnr == 55:
        # this is showing the overrun of the internal coarse counter
    
    # elif readdata_pixelnr == 9:
        # this is a dwell marker event
        
    # elif readdata_pixelnr == 10:
        # this is a line marker event
        
    # elif readdata_pixelnr == 12:
        # this is a frame marker event
        
    # elif readdata_pixelnr == 17:
        # this is a fifo overflow event, one should take care if this happens
        # print("FIFO overflow")

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