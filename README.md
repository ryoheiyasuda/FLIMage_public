# FLIMage
This software allows for controlling TCSPC software and produce image.

* To build this software, please get the following package (you can do NuGet): BitMiracle.LibTiff.NET 2.4.626 or higher, MathNet.Numerics.4.7.0 or higher, System.Numerics.Vector.4.5.0 or higher

* When we build FLIMage, we put external libraries in 
..\External and ..\Library folders. In the default setting, you need to create these folders. Alternatively you can change pre-compile option (in FLIMage properties).

* NI-card: If you want to use National Instruments, please install National instrument driver with DotNet support. We tested with PCIe-6231/6233 and PCI-6371/6373.

* TCSPC card: Additionally, if you want to use TCSPC card from PicoQuant (TimeHarp 260 / MultiHarp) or Becker Hickl (SPC-150), you need to install drivers. Contrlling DLL writen in C++ and C# wrapper is included in \External. In the default setting, you need to copy them to ..\External. In addition, to use some of DLL you need to get license from Florida Lifetime Imaging LCC.

* Thorlab linear motor stage.
If you want to use ThorLab MCM3000 or MCM5000 motor control, you need to put ThorMCM3000.dll and ThorMCM3000Settings.xml in ../External folder. Default appears to be port 32.

* Sutter linear stage.
This uses COM port. You can just configure at Documents\FLIMage\Init_Files\FLIM_deviceFile.txt.

