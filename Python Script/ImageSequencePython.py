# -*- coding: utf-8 -*-
"""
Created on Thu Aug  8 14:00:26 2019

This exmaple shows how to use PIPE client to run a seuqnece of image protocols.
First image settings to be used should be be saved.

@author: yasudar
"""
import os
import tkinter as tk
import tkinter.ttk as ttk
import time
import threading

from tkinter.filedialog import askopenfilename

from FLIM_pipeClient import FLIM_Com
      
class ImageSeqGUI(tk.Frame):
    def __init__(self, parent):
        tk.Frame.__init__(self, parent)
        self.parent=parent
        self.treeFileName = 'ImageSeq.txt'
        
        dirpath = os.path.dirname(os.path.abspath(__file__))
        self.treeFullFilename = os.path.join(dirpath, self.treeFileName)
        self.item_counter = 0        
        self.acquisitionDone = True
        self.initialize_user_interface()
              
        
    def initialize_user_interface(self):
        self.parent.title("Image Sequence")
        self.parent.config(background="lavender")
        self.parent.protocol('WM_DELETE_WINDOW', self.on_closing)

        # Define the different GUI widgets
        self.fileText = tk.StringVar()
        self.filename_label = tk.Label(self.parent, text="Setting file:")
        self.filename_entry = tk.Entry(self.parent,textvariable=self.fileText)           
        self.directoryText = tk.StringVar()
        self.dirname_label = tk.Label(self.parent, text="Setting directory:")
        self.dirname_entry = tk.Entry(self.parent,textvariable=self.directoryText)         
        self.repeatText = tk.StringVar()
        self.repeatText.set('1')
        self.repeat_label = tk.Label(self.parent, text="Repeat:")
        self.repeat_entry = tk.Entry(self.parent,textvariable=self.repeatText,
                                     width=20)        
        self.zoomText = tk.StringVar()
        self.zoom_label = tk.Label(self.parent, text="Zoom:")
        self.zoomText.set('10')
        self.zoom_entry = tk.Entry(self.parent,textvariable=self.zoomText,
                                   width=20)        
        self.intervalText = tk.StringVar()
        self.interval_label = tk.Label(self.parent, text="Interval (s):")
        self.intervalText.set('10')
        self.interval_entry = tk.Entry(self.parent,textvariable=self.intervalText,
                                   width=20)
        self.timeText = tk.StringVar()
        self.time_label = tk.Label(self.parent, textvariable=self.timeText)
        self.timeText.set('0.0 s')
        self.find_button = tk.Button(self.parent, text="Find file",
                                          command=self.find_file)        
        
        self.runText = tk.StringVar()
        self.run_button = tk.Button(self.parent, textvariable=self.runText, command=self.run)
        self.runText.set('Run')
        
        self.tree = ttk.Treeview(self.parent,
                columns=('FolderPath','FileName', 'Interval', 'Repeat', 'zoom'))
        self.tree.heading('#0', text='ID')
        self.tree.heading('#1', text='Folder')
        self.tree.heading('#2', text='Setting file')
        self.tree.heading('#3', text='Interval (s)')                  
        self.tree.heading('#4', text='Repeat')
        self.tree.heading('#5', text='Zoom')
        self.tree.column('#0',minwidth=0,width=50,stretch=tk.NO)
        self.tree.column('#1',minwidth=0,width=360,stretch=tk.NO)
        self.tree.column('#2',minwidth=0,width=160,stretch=tk.NO)                 
        self.tree.column('#3',minwidth=0,width=80,stretch=tk.NO)
        self.tree.column('#4',minwidth=0,width=80,stretch=tk.NO)
        self.tree.column('#5',minwidth=0,width=80,stretch=tk.NO)
                          
        self.buttonBar = buttonBar(self.parent, self) #Need to be after tree?
        self.connectVar = tk.IntVar()
        self.connectText = tk.StringVar()
        self.connectCheck = tk.Checkbutton(self.parent, textvariable=self.connectText, 
            variable=self.connectVar, command=self.PIPEconnect)
        self.connectText.set('Connect')
        #Location.
        self.filename_label.grid(row=0, column=0, sticky="E")
        self.filename_entry.grid(row=0, column=1, columnspan=4,sticky="EW")
        self.find_button.grid(row=0, column=5, sticky="W")
        self.dirname_label.grid(row=1, column=0, sticky="E")
        self.dirname_entry.grid(row=1, column=1, columnspan=4, sticky="EW")
        self.interval_label.grid(row=2,column=0,sticky="E")
        self.interval_entry.grid(row=2,column=1,sticky="W")
        self.repeat_label.grid(row=3, column=0, sticky="E")
        self.repeat_entry.grid(row=3, column=1, sticky="W")
        self.zoom_label.grid(row=2, column=2, sticky="E")
        self.zoom_entry.grid(row=2, column=3, sticky="W")
        self.time_label.grid(row=2, column=5)
        self.run_button.grid(row=3, column=5,sticky="EW")
        self.buttonBar.grid(row=3,column=2, sticky="W")
        self.connectCheck.grid(row=3,column=3)
        self.tree.grid(row=6, columnspan=6, sticky='nsew')
    
        self.flim = FLIM_Com()
        self.flim.messageReceived += self.PIPEclientMessage_received
        self.flim.start()
        
        if os.path.isfile(self.treeFullFilename):
            self.readTree(self.treeFullFilename)
            
    """
    PIPE communication functions
    You can run event-driven function, wheen FLIMage send a message, which can 
    be, for example, "AcquisitionDone'.
    """
    def PIPEconnect(self):
        if self.flim.Connected:
            self.flim.disconnect() 
        else:
            self.flim.start()
    
    def PIPEunsubscribe(self):
        self.flim.messageReceived -= self.PIPEclientMessage_received
        if self.flim.Connected:
            self.flim.disconnect() 
    
    def PIPEclientMessage_received(self, message, source):
        self.connectText.set('Connect with FLIMage')
        if source == 'R':
            print(f'Message from FLIMage: {message}')
            #This can be used for event-driven programming.
            if message == 'AcquisitionDone':
                self.acquisitionDone = True
        elif source == 'W' and self.flim.debug:
            print(f'Message from FLIMage: {message}')
            #This is simple reply.
        self.connectVar.set(self.flim.Connected)
    
        
    """
    Other GUI functions
    """
    def on_closing(self):
        try:
            self.PIPEunsubscribe()
        except:
            print('Software close')
        self.parent.destroy()
        
    def find_file(self):
        if self.directoryText.get() == "":      
            filename = askopenfilename()
        else:
            filename = askopenfilename(initialdir=self.directoryText.get())
        fn=os.path.basename(filename)        
        self.fileText.set(fn)
        self.directoryText.set(os.path.dirname(filename))
        
    def run(self):
        if self.runText.get() == 'Run':
            self.cancel = False
            self.runText.set('Stop')
            """
            We will use a different thread for the execution, so that the same button can be
            used to stop the thread.
            """
            self.thread = threading.Thread(target=self.startSeq)
            self.thread.daemon = True
            self.thread.start()  
        else:
            self.cancel = True
        
    """
    Main function for running sequence
    """
    def startSeq(self):       
        ids = self.tree.get_children()
        for i in range(len(ids)):
            self.tree.focus(ids[i])
            self.tree.selection_set(ids[i])
            values = self.tree.item(ids[i])['values']
            folder = values[0]
            filename = values[1]
            filenameFull = os.path.join(folder, filename)
            interval = values[2]
            repeat = values[3]
            zoom = values[4]
            
            if self.flim.Connected:
                self.flim.sendCommand(f'LoadSetting,{filenameFull}')
            
            for j in range(repeat):
                startTime = time.time()
                self.acquisitionDone = False
                
                if self.flim.Connected:
                    self.flim.sendCommand(f'SetZoom,{zoom}')                    
                    self.flim.sendCommand('StartGrab')
                """
                "StartGrab" command in a different thread in FLIMage, so that we can stop at any time with
                "StopGrab". You can ask if the process is finished by "IsGrabbing" command.
                Alternatively, FLIMage send message 'AcquisitionDone event, which turns on self.acquisitionDone
                in PIPEclientMessage_received fundtion. You can use it to see if the acquisition is done or not.
                This is same for "StartUncaging" "StopUncaging" and "IsUncaging" command.
                """
                while(True):
                    if self.flim.Connected:
                        grabbing = bool(int(self.flim.sendCommand('IsGrabbing').split(',')[1]))
                        #grabbing = grabbing or (not self.acquisitionDone)
                    else:
                        grabbing = False
                    
                    currentT = time.time() - startTime
                    self.timeText.set(f'{j+1}/{repeat} (Time: {currentT:0.1f} s)')
                    self.parent.update()
                    
                    #print(f'currentT = {currentT:0.0f}, interval={interval}, grabbing = {grabbing}, cancel={self.cancel}')
                    if (currentT >= interval and (not grabbing)) or self.cancel:
                        break
                    else:
                        time.sleep(0.5)
            if self.cancel:
                print('Canceled!')
                break
            
        """
        If Uncaging or Grabbing running, we will stop them
        """
        if self.cancel and self.flim.Connected:            
            grabbing = bool(self.flim.sendCommand('IsGrabbing').split(',')[1])
            uncaging = bool(self.flim.sendCommand('IsUncaging').split(',')[1])
            #This software does not have capability of doing just uncaging, but anyway...
            if grabbing:
                self.flim.sendCommand('StopGrab')  
            if uncaging:
                self.flim.sendCommand('StopUncaging')
                
        self.runText.set('Run')
        self.cancel = False
        
    def saveTree(self):        
        file1 = open(self.treeFullFilename, 'w')
        ids = self.tree.get_children()        
        for id1 in ids:
            values = self.tree.item(id1)['values']
            file1.write(f'{values}\n')
        file1.close()
        
    def readTree(self, filename):
        file1 = open(filename, 'r')
        lines = file1.readlines()        
        self.item_counter = 0
        for ln in lines:
            values = eval(ln)
            self.tree.insert('', 'end', text='P'+str(self.item_counter+1),
                    values=values)
            self.item_counter = self.item_counter + 1
        ids = self.tree.get_children()
        if len(ids) > 0:
            self.tree.focus(ids[-1])
            self.tree.selection_set(ids[-1])
            self.treeToForm()
            
    def treeToForm(self):
        curItem = self.tree.selection()[0]
        values = self.tree.item(curItem)['values']
        folder = values[0]
        filename = values[1]
        interval = values[2]
        repeat = values[3]
        zoom = values[4]
        self.fileText.set(filename)
        self.directoryText.set(folder)
        self.repeatText.set(str(repeat))
        self.intervalText.set(str(interval))
        self.zoomText.set(str(zoom))

class buttonBar(tk.Frame):
    def __init__(self, parentFrame, img_seq):
      tk.Frame.__init__(self, parentFrame)
      self.parentFrame=parentFrame
      self.img_seq=img_seq
      self.tree = self.img_seq.tree
      self.delete_button = tk.Button(self, text="Delete",
                                  command=self.delete_data)
      self.insert_button = tk.Button(self, text="Insert",
                                            command=self.insert_data)
      self.replace_button = tk.Button(self, text="Replace",
                                            command=self.replace_data)
      self.delete_button.pack(side='left', anchor='w', expand=True)
      self.insert_button.pack(side='left', anchor='w', expand=True)
      self.replace_button.pack(side='left', anchor='w', expand=True)

        
    def replace_data(self):
        curItem = self.tree.selection()[0]
        ids = self.tree.get_children()
        for i in range(len(ids)):
            if ids[i] == curItem:
                break
        text1 = self.tree.item(ids[i])['text']
        self.tree.delete(curItem)        
        if self.img_seq.directoryText.get() != "" and self.img_seq.filename_entry.get() != "":
            self.tree.insert('', i, text=str(text1),
                    values=(self.img_seq.directoryText.get(), self.img_seq.fileText.get(), 
                            self.img_seq.intervalText.get(), self.img_seq.repeatText.get(), 
                            self.img_seq.zoomText.get()))
        ids = self.tree.get_children()
        self.tree.focus(ids[i])
        self.tree.selection_set(ids[i])
        self.img_seq.saveTree()
        
    def delete_data(self):        
        curItem = self.tree.selection()[0]
        ids = self.tree.get_children()
        for i in range(len(ids)):
            if ids[i] == curItem:
                break
        self.tree.delete(curItem)
        #Select row eitehr above or below of the deleted culumn.
        if i == len(ids)-1:
            i = i-1
        else:
            i = i+1
        if i >= 0 and i < len(ids)-1:
            self.tree.focus(ids[i])
            self.tree.selection_set(ids[i])   
        self.img_seq.saveTree()
         
    def insert_data(self):
        if self.img_seq.directoryText.get() != "" and self.img_seq.filename_entry.get() != "":
            self.tree.insert('', 'end', text='P'+str(self.img_seq.item_counter+1),
                    values=(self.img_seq.directoryText.get(), self.img_seq.fileText.get(), 
                            self.img_seq.intervalText.get(), self.img_seq.repeatText.get(), 
                            self.img_seq.zoomText.get()))
        self.img_seq.item_counter = self.img_seq.item_counter + 1
        child_id = self.tree.get_children()[-1]
        self.tree.focus(child_id)
        self.tree.selection_set(child_id)
        self.img_seq.saveTree()
        
def main():
    root=tk.Tk()
    d=ImageSeqGUI(root)
    root.mainloop()
    d.PIPEunsubscribe()

if __name__=="__main__":
    root=tk.Tk()
    d=ImageSeqGUI(root)
    root.mainloop()
    d.PIPEunsubscribe()
