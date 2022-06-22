# -*- coding: utf-8 -*-
"""
Created on Mon May 22 09:43:44 2017
Class to connect to FLIMage software.

You can use like:

flim = FLIM_Com()
flim.start()

if flim.Connected:
    flim.messageReceived += FLIM_message_received #Add your function to  handle.

flim.sendCommand('Command') #You can send command to FLIMage
    
@author: Ryohei Yasuda

Event handler code is from
http://www.valuedlessons.com/2008/04/events-in-python.html
"""

import os
import win32file
import struct
import threading
import time
from win32com.shell import shell, shellcon

class Event:
    def __init__(self):
        self.handlers = set()

    def handle(self, handler):
        self.handlers.add(handler)
        return self

    def unhandle(self, handler):
        try:
            self.handlers.remove(handler)
        except:
            print("Handler is not handling this event, so cannot unhandle it.")
            #raise ValueError("Handler is not handling this event, so cannot unhandle it.")
        return self

    def fire(self, *args, **kargs):
        for handler in self.handlers:
            handler(*args, **kargs)

    def getHandlerCount(self):
        return len(self.handlers)

    __iadd__ = handle
    __isub__ = unhandle
    __call__ = fire
    __len__  = getHandlerCount
    
class FLIM_Com:        
    def __init__(self):
        self.debug = False #More message printed for Debug mode.
        self.__handShakeCode = 'FLIMage'
        self.writeServerName = 'FLIMageW'
        self.readServerName = 'FLIMageR'
        self.initFile = "COM_method.txt" #This file is to start the server. --- there is no way to connect otherwise...
        initFilePath = r"FLIMage\Init_Files\COM"

        self.messageReceived = Event()
        self.Connected = False
        self.Initializing = False
        self.initFilePath = os.path.join(shell.SHGetFolderPath(0, shellcon.CSIDL_PERSONAL, None, 0), initFilePath)
        if not os.path.isdir(self.initFilePath):
            os.mkdier(self.initFilePath)
        
    def start(self):
        self.Initializing = True
        for i in range(10): #Try 10 times.
            self.Connected = False
            self.startServer() #It does not do anything if server is already activated.
            time.sleep(0.3)
            self.startConnection()
            if self.Connected:
                break
        self.Initializing = False

    def startServer(self):
        file1 = open(os.path.join(self.initFilePath, self.initFile),'w')
        file1.write('PIPE')
        file1.close()
        
    def startConnection(self):
        s1 = False
        s2 = False        
        try:
            self.clientR = win32file.CreateFile(f"\\\\.\\pipe\\{self.readServerName}", 
                                      win32file.GENERIC_READ | win32file.GENERIC_WRITE, 
                                      0, None, win32file.OPEN_EXISTING, 0, None)
    
            self.clientW = win32file.CreateFile(f"\\\\.\\pipe\\{self.writeServerName}", 
                                      win32file.GENERIC_READ | win32file.GENERIC_WRITE, 
                                      0, None, win32file.OPEN_EXISTING, 0, None)
            
            #Handshake must be on this order.
            s1 = self.__handShake(self.clientR)
            s2 = self.__handShake(self.clientW)
            
        except:
            print ('Connection fialed')
        
        if s1 and s2:
#            self.startReceiving()
            self.Connected = True
            self.messageReceived('PIPE connected', 'PIPE')
            self.startReceiving()
        else:
            self.Connected = False
            self.messageReceived('PIPE connection failed', 'PIPE')
            #print ('Failed: Perhaps server is not active')              

    def sendCommand(self, str1):
        if self.Connected:
            try:
                self.__sendMessage(self.clientW, str1)
                self.__readMessage(self.clientW)
                self.messageReceived(self.Received, 'W')
            except:
                self.failureHandle()
                
            return self.Received
            
    def receiveOne(self):
        try:
            self.__readMessage(self.clientR)
            self.messageReceived(self.Received, 'R')
        except:
            if not self.Initializing: #During initialization, it may happen.
                self.failureHandle()
            
    def failureHandle(self):
        self.Received = 'Connection problem'
        print('Connection problem: server terminated?\n', end="")
        self.Connected = False
        self.messageReceived('PIPE server terminated', 'PIPE')
        try:            
            self.close()         
        except:
            print('')

    def __repeatReceiving(self):
        while self.Connected:
            if not self.Connected:
                break
            else:
                self.receiveOne()
            
    def startReceiving(self):
        self.thread = threading.Thread(target=self.__repeatReceiving)
        self.thread.daemon = True
        self.thread.start()        
        
    def disconnect(self):
        if self.Connected:
            self.Connected = False
            self.__sendMessage(self.clientW, 'Disconnect')
            self.messageReceived('PIPE disconnected', 'PIPE')

    def close(self):
        self.Connected = False
        self.clientR.Close()
        self.clientW.Close()     

    def __handShake(self, client):
        self.__readMessage(client)
        if self.Received == self.__handShakeCode:           
            self.__sendMessage(client, self.__handShakeCode)
        else:
            return False
        self.__readMessage(client)
        return True
            
    def __sendMessage(self, client, str1):
        s_code = bytes(str1, 'utf-8')
        len1 = len(s_code);
        if len1 > 65535:
            s_code = s_code[0:65534]
            len1 = 65535

        win32file.WriteFile(client, bytes([int(len1/256)]))
        win32file.WriteFile(client, bytes([len1 & 255]))
        win32file.WriteFile(client, s_code)
        
        if self.debug:
            print('Message sent: ' + s_code.decode("utf-8") + '\n', end="")
    
    def __readMessage(self, client):        
        data1 = struct.unpack('B', win32file.ReadFile(client, 1)[1])[0]
        data2 = struct.unpack('B', win32file.ReadFile(client, 1)[1])[0]
        l_data = data1* 256 + data2
        self.Received = win32file.ReadFile(client, l_data)[1].decode("utf-8")

"""
Example event triggered message. FLIMage sometimes send message like
AcquisitionDone, or MotorMoveDone etc. 
"""
def FLIM_message_received(data, source):
    if source == 'R':
        print (f'    Message Received: {data}\n', end="") 
        #For PIPE, end="" and include "\n" is more stable.
    else:
        print (f'    Reply: {data}\n', end="");


if __name__ == "__main__":
    flim = FLIM_Com()
    flim.start()
    
    if flim.Connected:
        flim.messageReceived += FLIM_message_received #Add your function to  handle.
        