# -*- coding: utf-8 -*-
"""
Created on Fri Feb 15 10:46:04 2019
This is an example script that uses FLIMageFileReader to open '*.flim' images created by FLIMage, calculate lifetime and plot.
@author: yasudar
"""

#Plotting example
from FLIMageFileReader import FileReader
import matplotlib as mpl
mpversion = mpl.__version__
mplversion = [int(x) for x in mpversion.split('.')]

if mplversion[0] >= 3:  
    from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg, NavigationToolbar2Tk
else:
    from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg, NavigationToolbar2TkAgg
    
from matplotlib.figure import Figure #To use Figure command
import tkinter as tk
from tkinter import filedialog

plotsize = (10,4) #x, y. in inch
windowResolution = 100 #dot per inch 

plotWindow = tk.Tk()
plotWindow.wm_title('Fluorescence lifetime')                

file_path = filedialog.askopenfilename()
iminfo = FileReader()
iminfo.read_imageFile(file_path, True)
iminfo.calculatePage(0, 0, 0, [0, iminfo.n_time[0]], [0, 50], [1.6, 3], 1.5)

f = Figure(figsize = plotsize, dpi=windowResolution) #define the size of the figure.  
a = f.add_subplot(1,3,1) #subplot. 
a.set_title('Intensity')
a.set_xticks([])
a.set_yticks([])
a.imshow(iminfo.intensity)
pos = a.get_position()
a.set_position([pos.width * 0.05, pos.height * 0.4, pos.width, pos.height])

if iminfo.ifFLIMimage():
    a = f.add_subplot(1,3,2)
    a.set_title('FLIM')
    a.set_xticks([])
    a.set_yticks([])
    a.imshow(iminfo.rgbLifetime)
    a.set_position([pos.width * 1.1, pos.height * 0.4, pos.width, pos.height])
    
    a = f.add_subplot(1,3,3) #subplot 2
    a.set_title('Lifetime')
    a.set_xlabel('Time (ns)')
    a.set_ylabel('N Photons')
    a.plot(iminfo.time, iminfo.lifetime)
    a.set_position([pos.width * 2.6, pos.height * 0.4, pos.width * 1.5, pos.height])

canvas = FigureCanvasTkAgg(f, plotWindow)
canvas.get_tk_widget().pack(side = tk.TOP, fill = tk.BOTH, expand = True)

if mplversion[0] >= 3:  
    NavigationToolbar2Tk(canvas, plotWindow).update()
else:
    NavigationToolbar2TkAgg(canvas, plotWindow).update()
    
canvas._tkcanvas.pack(side = tk.TOP, fill = tk.BOTH, expand = True)

plotWindow.protocol("WM_DELETE_WINDOW", plotWindow.quit)
plotWindow.lift()
plotWindow.mainloop()
plotWindow.destroy()