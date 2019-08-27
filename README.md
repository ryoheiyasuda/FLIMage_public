# FLIMage
This software allows for controlling TCSPC hardware and generate fluorescence lifetime images in real time. The main part of the source code is free for academic and educational use. It is used and tested in the Yasuda lab (Max Planck Florida Institute for Neuroscience) on Windows 10 (x64) and NationalInstruments DAQmx 18.6. The software is updated frequently. 

* Compiled and packaged installer can be found in https://github.com/ryoheiyasuda/FLIMage_Installer

* This is open as it is, but for support, please contact Florida Lifetime Imaging LLC (http://www.lifetime-imaging.com/).

* The solution file is made in Visual Studio 17 (free "Community" version should build this).

* To build this software, please get the following packages, either by direct download or NuGet: BitMiracle.LibTiff.NET 2.4.626 or higher, System.Numerics.Vector.4.5.0 or higher, and C# DotNet 4.6.1 or higher. It will compile only on x64 mode. If you like to seed-up some calculation, you could install Intel MKL library (the same library used in Numpy and Matlab). Some calculations (FFT etc) will use the library when available.

* Please copy DLL in "External" into output folder. Alternatively you can add pre-build event command -- copy "$(ProjectDir)\\External\\$(Configuration)\\*" "$(OutDir)"

* You perhaps need to edit prebuild events, depending on where you store your external libraries.

* In default setting, binary will be created in $(SolutionDir)..\\..\\bin folder. You may need to create the folder before building.

* NI-card: If you want to use National Instruments cards, you need to install National instrument driver with DotNet support (we use version 18.6). Then, put NationalInstruments.Common.dll and NationalInstruments.DAQmx.dll in $(ProjectDir)..\\Libraries. We tested with PCIe-6231/6233 and PCI-6371/6373. It should compile without these libraries for analysis. 

* TCSPC card: If you want to use TCSPC card from PicoQuant (TimeHarp 260 / MultiHarp) or Becker Hickl (SPC-150), you need to install their drivers. Contact Florida Lifetime Imaging LLC for details (http://www.lifetime-imaging.com/). 

* For additional DLL files and their source codes you may want, please contact Yasuda lab (Ryohei.yasuda@mpfi.org). 

* Sutter or Thorlab linear stage controls and other hardware controls.
You need DLL files and configure them. Contact Florida Lifetime Imaging LLC (http://www.lifetime-imaging.com/).

* Tag-lens: 
It is possible to scan in Z-axis extremely fast (~200-1000KHz) with taglens. This feature is still under development. You need to add DLL.

* Resonant scanning:
We are aiming to implement it in next version.


