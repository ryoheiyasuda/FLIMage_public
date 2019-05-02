# FLIMage
This software allows for controlling TCSPC software and produce image. The main part of the source code is free for academic and educational uses. It is used and tested in the Yasuda lab on Windows X and NationalInstruments DAQmx 18.6. The software is updated frequently. If you need binary file, please contact Florida Lifetime Imaging LLC.

* The project file is for Visual Studio 17 (free "Community" version can build this).

* To build this software, please get the following packages, either by direct download or NuGet: BitMiracle.LibTiff.NET 2.4.626 or higher, System.Numerics.Vector.4.5.0 or higher, and C# DotNet 4.6.1 or higher.

* To build this software, you need to put necessary libraries in 
..\Libraries (BitMiracles.Lib.Tiff.NET.dll). Also, create folder named ..\bin for output (or change the output directory)

* NI-card: If you want to use National Instruments, please install National instrument driver with DotNet support. Put NationalInstruments.Common.dll, NationalInstruments.DAQmx.dll in ../Libraries folder. We tested with PCIe-6231/6233 and PCI-6371/6373.

* TCSPC card: Additionally, if you want to use TCSPC card from PicoQuant (TimeHarp 260 / MultiHarp) or Becker Hickl (SPC-150), you need to install their drivers as well as DLL from Florida Lifetime Imaging LLC.

* Thorlab linear motor stage.
You need DLL and XML files and configure them. Contact Florida Lifetime Imaging LLC.

* Sutter linear stage.
This uses COM port. You can just configure at Documents\FLIMage\Init_Files\FLIM_deviceFile_V1.txt.

