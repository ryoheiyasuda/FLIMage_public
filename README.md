# FLIMage
This software allows for controlling TCSPC software and produce image.

* To build this software, please get the following package:
- BitMiracle.LibTiff.NET 2.4.626 or higher
- MathNet.Numerics.4.7.0 or higher
- System.Numerics.Vector.4.5.0 or higher

* National Instruments
If you want to use National Instruments, please install National instrument driver with DotNet support. We tested with PCIe-6231/6233 and PCI-6371/6373.

* TCSPC card.
Additionally, if you want to use TCSPC card from PicoQuant (Time Harp 260) or Becker Hickl (SPC-150), you need to install driver DLL. The software will look for the driver automatically.

* Thorlab linear motor stage.
If you want to use ThorLab MCM3000 or MCM5000 motor control, you need to put ThorMCM3000.dll and ThorMCM3000Settings.xml in ./Packages folder or ../../bin folder. Default appears to be port 32.

* Sutter linear stage.
This uses COM port. You can just configure at Documents\FLIMage\Init_Files\FLIM_deviceFile.txt.

