# -*- coding: utf-8 -*-
"""
Created on Sat Apr 12 17:18:32 2025

@author: yasudalab
"""

#!/usr/bin/env python

import socket

# open the device on the localhost, port 9999


# check if the calibration of the TDCs has completed
for i in range(100):
    t = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    t.connect(('127.0.0.1', 9999))

    # read the server response
    data = t.recv(8192)
    
    #print(data.decode('utf8'))
    command = bytes("I,10\n", "utf8")
    t.send(command)
    data = t.recv(8192*10)
    photons = [int(x.split(',')[1]) for x in data.decode().split('\n')]
    
    print(sum(photons))
    
    t.close()
