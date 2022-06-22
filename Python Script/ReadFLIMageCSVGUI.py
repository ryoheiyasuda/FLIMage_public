# -*- coding: utf-8 -*-
"""
This program is a GUI backend for ReadFLIMageCSV program.
It will read all CSV files made by FLIMage in a folder, and average them.
@author: Ryohei Yasuda
"""

import os, win32gui, win32con

import ctypes.wintypes
from ReadFLIMageCSV import ReadFLIMageCSV
import tkinter as tk
from tkinter.filedialog import askopenfilename

import matplotlib as mpl
mpversion = mpl.__version__
mplversion = [int(x) for x in mpversion.split('.')]

if mplversion[0] >= 3:  
    from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg, NavigationToolbar2Tk
else:
    from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg, NavigationToolbar2TkAgg
from matplotlib.figure import Figure #To use Figure command


excelWinTitles = []
excelWindowHwnd = []
def enumHandler(hwnd, lParam):
    fn = win32gui.GetWindowText(hwnd)
    if 'Excel' in win32gui.GetWindowText(hwnd):
        print(fn)
        excelWinTitles.append(fn)
        excelWindowHwnd.append(hwnd)

def ifExcelFileOpenClose(fileName):
    excelWinTitles.clear()
    excelWindowHwnd.clear()
    win32gui.EnumWindows(enumHandler, None)
    for i, title in enumerate(excelWinTitles):
        if fileName in title:
            print(fileName)
            win32gui.PostMessage(excelWindowHwnd[i],win32con.WM_CLOSE,0,0)
    
class ReadFLIMageCSVGUI(tk.Frame):
    def __init__(self, parent):
        tk.Frame.__init__(self, parent) #parent = root frame.
        
        #parameters
        self.displayROI = "ROI1"
        self.displayCh = "ch1"
        self.dirPath = self.winDefFolder()
        
        #GUI building
        #Normalize checkbox
        self.normalizeBool = tk.IntVar()
        self.normalizeCheckBox = tk.Checkbutton(self, variable=self.normalizeBool, onvalue=1, offvalue=0, text="Normalize with baseline")
        self.normalizeCheckBox.select()
        
        # Note that frame is counted as 1,2,3... in this prompt (subtracted by 1 later)
        tab = " " * 20
        self.prompt_baselineFrom = tk.Label(self, text= tab+"Baseline frame from (> 1): ", anchor="w")
        self.prompt_baselineTo = tk.Label(self, text= tab+"Baseline frame to (> 1):", anchor="w")
        self.baselineStartEntry = tk.Entry(self)
        self.baselineStartEntry.insert(10, '1')
        self.baselineEndEntry = tk.Entry(self)
        self.baselineEndEntry.insert(10, '32')
        
        self.promptMaxNSamples = tk.Label(self, text="Max number of samples / file: ", anchor="w")
        self.maxNSamplesEntry = tk.Entry(self)
        self.maxNSamplesEntry.insert(10, '2000');
        
        #Display
        self.promptDisplay = tk.Label(self, text = "Plot display setting:")        
        self.promptDisplayROI = tk.Label(self, text=tab+"Display ROI name (eg ROI1):")
        self.displayROIEntry = tk.Entry(self)
        self.displayROIEntry.insert(10, self.displayROI)
        self.promptDisplayCh = tk.Label(self, text=tab+"Display channel name (eg ch1):")
        self.displayChEntry = tk.Entry(self)
        self.displayChEntry.insert(10, self.displayCh)
        
        self.displayCheckLabel = tk.Label(self, text=tab+"Display plot of...")
        self.displayCheckbar = Checkbar(self, ['sumIntensity',  'sumIntensity_bg', 'meanIntensity', 'meanIntensity_bg', 'Lifetime', 'Fraction2', 'Lifetime_fit', 'Fraction2_fit'])
        self.displayCheckbar.SetValues([0,1,0,1,1,0,0])
        
        
        #Divide with dendrite checkbox
        self.divideWithDendriteBool = tk.IntVar()
        self.divideWithDendriteCheckBox = tk.Checkbutton(self, variable=self.divideWithDendriteBool, onvalue=1, offvalue=0, text="Divide with dendrite (Intensity only)")
        
        self.promptDend = tk.Label(self, text=tab+"Dendrite ROI name (eg ROI2):")
        self.dendriteEntry = tk.Entry(self)
        self.dendriteEntry.insert(10, 'ROI2')
        
        self.setDirButton = tk.Button(self, text="Set file directory", command = self.setDirectory, width=20)
        self.dirNameLabel = tk.Label(self, text=self.dirPath)
        
        self.calcButton = tk.Button(self,text="Calculate average", command = self.calculateAndDisplay, width=20)

        self.averageFileNameEntry = tk.Entry(self)
        self.averageFileNameEntry.insert(10, "average.csv")

        self.openExcelBool = tk.IntVar()
        self.OpenExcelCheckBox = tk.Checkbutton(self, variable=self.openExcelBool, onvalue=1, offvalue=0,text="Open in excel after calculation")
        self.OpenExcelCheckBox.select()
        
        # lay the widgets out on the screen. 
        EmptyEntryHeight = 25
        i=0 
        self.normalizeCheckBox.grid(column=0,row=i,columnspan=2,sticky="W")
        self.prompt_baselineFrom.grid(column=0,row=i+1,sticky="W")
        self.baselineStartEntry.grid(column=1,row=i+1,sticky="W")
        self.prompt_baselineTo.grid(column=0,row=i+2,sticky="W")
        self.baselineEndEntry.grid(column=1,row=i+2,sticky="W")
        self.grid_rowconfigure(i+3,minsize=EmptyEntryHeight) #empty space.
        i=i+4 #Used 3 lines above.

        self.promptDisplay.grid(column=0,row=i,sticky="W")
        self.promptDisplayROI.grid(column=0,row=i+1,sticky="W")
        self.displayROIEntry.grid(column=1,row=i+1,sticky="W")
        self.promptDisplayCh.grid(column=0,row=i+2,sticky="W")
        self.displayChEntry.grid(column=1,row=i+2,sticky="W")
        i=i+3
        
        #self.displayCheckLabel.grid(column=0,row=i, sticky="W")
        self.displayCheckbar.grid(column=0,row=i+1,columnspan=2)
        self.grid_rowconfigure(i+2,minsize=EmptyEntryHeight) #empty space.
        i=i+3
        
        self.divideWithDendriteCheckBox.grid(column=0,row=i,columnspan=2,sticky="W")
        self.promptDend.grid(column=0,row=i+1,sticky="W")
        self.dendriteEntry.grid(column=1,row=i+1,sticky="W")
        self.grid_rowconfigure(i+2,minsize=EmptyEntryHeight) #empty space.
        i=i+3 #2 lines above.
                
        self.promptMaxNSamples.grid(column=0,row=i,sticky="W")
        self.maxNSamplesEntry.grid(column=1,row=i,sticky="W")  
        self.grid_rowconfigure(i+1,minsize=EmptyEntryHeight) #empty space.
        i=i+2 #
        
        self.setDirButton.grid(column=0,row=i)
        self.dirNameLabel.grid(column=1,row=i,sticky="W")
        self.grid_rowconfigure(i+1,minsize=EmptyEntryHeight) #empty space.
        i=i+2 #
        
        self.calcButton.grid(column=0,row=i)
        self.grid_rowconfigure(i+1,minsize=EmptyEntryHeight) #empty space.
        self.averageFileNameEntry.grid(column=1,row=i,sticky="W")
        i=i+2        
        
        self.OpenExcelCheckBox.grid(column=0,row=i)
        i=i+1;
        
        self.root = parent
        self.root.protocol("WM_DELETE_WINDOW", self.on_closing)
        
        self.Average = ReadFLIMageCSV()
        
    def winDefFolder(self):
        CSIDL_PERSONAL = 5       # My Documents
        SHGFP_TYPE_CURRENT = 0   # Get current, not default value
        buf= ctypes.create_unicode_buffer(ctypes.wintypes.MAX_PATH)
        ctypes.windll.shell32.SHGetFolderPathW(None, CSIDL_PERSONAL, None, SHGFP_TYPE_CURRENT, buf)
        return buf.value
        
    def on_closing(self):
        self.root.destroy()
    
    def appendValues(self):
        #First get all variables.
        self.Average.NormalizeWithBaseLine = bool(self.normalizeBool.get()) #For FLIM, it is subtraction.
        self.Average.baseLineStart = int(self.baselineStartEntry.get()) - 1 #
        self.Average.baseLineEnd = int(self.baselineEndEntry.get()) - 1 #
        self.Average.sampleLength = int(self.maxNSamplesEntry.get()) #Cut off for the sample length. Put very large value if you don't want to cut the data.
        self.Average.if_divide_with_ROI = bool(self.divideWithDendriteBool.get()) #If you want to normalize intensity with dendrite, put True.
        self.Average.divide_with_ROI = self.dendriteEntry.get().upper() #ROI2 can be dendritic intensity, for example.   
        self.Average.outputFilename = self.averageFileNameEntry.get()
        
        self.displayROI = self.displayROIEntry.get().upper()
        self.displayCh = self.displayChEntry.get().lower()
        
    def setDirectory(self):
        self.appendValues()
        if os.path.isdir(self.dirPath):
            os.chdir(self.dirPath)
        filename = askopenfilename()
        self.dirPath = os.path.split(filename)[0]
        self.dirNameLabel.config(text=self.dirPath)

    def calculateAndDisplay(self):
        self.appendValues()
        print('normalize ' + str(self.Average.NormalizeWithBaseLine) + ' baseLine = ' + str(self.Average.baseLineStart) + ':' + str(self.Average.baseLineEnd))
        ifExcelFileOpenClose(self.Average.outputFilename)
        self.Average.loadDataAndCalculateAverage(self.dirPath)
        self.display()


    def display(self):

        displayTypes = [] #('sumIntensity', 'Lifetime', 'Fraction2') #Intensity, Lifetime, Lifetime_fit, Fraction2 or Fraction2_fit

        state = self.displayCheckbar.state()
        for i in range(len(state)):
            if bool(state[i]):                
                displayTypes.append(self.displayCheckbar.names[i])
            
                #Figure size.
        plotsize = (5 * len(displayTypes),4) #x, y. in inch
        windowResolution = 100 #dot per inch 
        
        self.plotWindow = tk.Tk()
        self.plotWindow.wm_title('Plot window')                
        f = Figure(figsize = plotsize, dpi=windowResolution) #define the size of the figure.
        
        if self.Average.multiRoi:
            title = self.displayROI + '-' + self.displayCh
        else:
            title = self.displayCh
        f.suptitle(title, fontsize=14, fontweight='bold')      
        
        NFig = len(displayTypes)
        for figN in range(NFig):
            dispType = displayTypes[figN]
            
            a = f.add_subplot(1,NFig,figN+1) #subplot. 
            
            a.set_xlabel('Time (s)')
            
            if self.Average.NormalizeWithBaseLine:
                displayType2 = '$\Delta$ ' + dispType
                if 'Intensity' in dispType:
                    displayType2 = displayType2 + ' (%)'
            else:
                displayType2 = dispType
            
            if (dispType == 'Lifetime') | (dispType == 'Lifetime_fit'):
                displayType2 = displayType2 + ' (ns)'
        
            a.set_title(displayType2)
            a.set_ylabel(displayType2)
            hPlot = tuple()
            for key in self.Average.data_ave:
                headStr = key.split('-')
                if key != self.Average.timeKey:
                    if len(headStr) >= 3 and self.Average.multiRoi:
                        cond1 = key.startswith(dispType + '-') & (headStr[1] == self.displayROI) & (headStr[2] == self.displayCh)
                    elif len(headStr) >= 2:
                        cond1 = key.startswith(dispType + '-') & (headStr[1] == self.displayCh)
                    if cond1:
                        for i in range(self.Average.nFiles):
                            h1, = a.plot(self.Average.data_ave['Time (s)'].values, self.Average.data_all_pd[key].values.transpose()[:, i], '-', label = self.Average.files[i])
                            hPlot = hPlot + (h1,)
                        
                        h1, = a.plot(self.Average.data_ave['Time (s)'], self.Average.data_ave[key].values, 'k-', lw = 4, label = 'Average')
                        hPlot = hPlot + (h1,)
                        print(key)
            
            
            box = a.get_position()
                
            a.set_position([box.x0 * 0.9, box.y0, box.width * 0.8, box.height])
            if figN == NFig - 1:
                a.legend(handles = hPlot, loc='center left', bbox_to_anchor = (1, 0.5))            
        
        #This is the trick to show figure in Tk window.
        canvas = FigureCanvasTkAgg(f, self.plotWindow)
        canvas.get_tk_widget().pack(side = tk.TOP, fill = tk.BOTH, expand = True)
        
        #This will be very useful for magnifying etc.
        if mplversion[0] >= 3:  
            NavigationToolbar2Tk(canvas, self.plotWindow).update()
        else:
            NavigationToolbar2TkAgg(canvas, self.plotWindow).update()
        canvas._tkcanvas.pack(side = tk.TOP, fill = tk.BOTH, expand = True)
        
        if self.openExcelBool:
            os.system("start EXCEL.EXE " + os.path.join(self.Average.directoryPath, self.Average.outputFilename))
        #self.plotWindow.protocol("WM_DELETE_WINDOW", self.plotWindow.quit)
        self.plotWindow.lift()
        self.plotWindow.mainloop()


                
class Checkbar(tk.Frame):
    def __init__(self, parent=None, picks=[]):
      tk.Frame.__init__(self, parent)
      self.vars = []
      self.names = picks
      for pick in picks:
         var = tk.IntVar()
         chk = tk.Checkbutton(self, text=pick, variable=var)
         chk.pack(side='left', anchor='w', expand=True)
         self.vars.append(var)
         
    def SetValues(self, values=[]):
        for i in range(len(values)):
            self.vars[i].set(values[i])
                

    def state(self):
        val = []
        for i in range(len(self.vars)):
            val.append(self.vars[i].get())
        return val

#Special function: if the program run as main.        
if __name__ == "__main__": #__name__ is the spacial command for this purpose.
    root = tk.Tk()
    root.wm_title('Average time courses')
    root.lift()
    ReadFLIMageCSVGUI(root).pack(fill="both", expand=True)
    root.mainloop()
