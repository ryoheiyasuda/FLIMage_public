# -*- coding: utf-8 -*-
"""
Created on Sat Mar  2 11:34:36 2019
use: 
    python.exe runPythohnFile.py [full FileName].

@author: yasudar
"""

import os, sys

def setEnvironmentalPath(pathName):
    exist = False
    for path in  os.environ['PATH'].split(','):
        if path == pathName:
            exist = True
            break
    if (not exist) and os.path.exists(pythonPath):
         os.environ['PATH'] + ";" + pathName


if __name__ == "__main__":
    pythonPath = os.path.dirname(sys.executable)
    
    setEnvironmentalPath(os.path.join(pythonPath))
    setEnvironmentalPath(os.path.join(pythonPath, 'bin'))
    setEnvironmentalPath(os.path.join(pythonPath, 'Library', 'bin'))
    setEnvironmentalPath(os.path.join(pythonPath, 'Scripts'))
    setEnvironmentalPath(os.path.join(pythonPath, 'Library', 'usr', 'bin'))
    setEnvironmentalPath(os.path.join(pythonPath, 'Library', 'mingw-w64', 'bin'))
    
    sys.path.insert(0, '')
    dir_path = os.path.dirname(os.path.realpath(__file__))
    
    if len(sys.argv) == 1:
        filename = os.path.join(dir_path, "ReadFLIMageCSVGUI.py")
    else:
        filename = str(sys.argv[1])
    name = filename.encode('ascii')
    source = open(filename, 'r').read()
    code = compile(source, name, "exec")
    exec(code)
