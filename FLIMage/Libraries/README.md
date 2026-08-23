# Runtime libraries

This directory contains one FLIMage-built binary:

```text
TCSPC_Decode.dll
```

It is the unsigned x64 decoder build dated July 31, 2026. SHA-256:

```text
3D4875AA24B0640EA8D1FE21CCCCDEC9B6CD097AB021480F151D8068F17FA1BF
```

Obtain additional binary dependencies from their respective owners and verify that your license permits local use.

## Required to compile the main application

Place compatible assemblies here when they are not resolved from an installed SDK or the GAC:

```text
NationalInstruments.Common.dll
NationalInstruments.DAQmx.dll
```

BitMiracle.LibTiff.NET is restored from NuGet and does not belong in this directory. The current source targets .NET Framework 4.8. Its NI references are not restricted to one exact assembly version.

For runtime selection, FLIMage reads the installed `nicaiu.dll` version and recognizes these directory names:

```text
NI18.1
NI18.6
NI20.1
NI25.0
```

Create the applicable subdirectory here and populate it with the matching NI-provided .NET assemblies. The application build preserves the subdirectory when copying `FLIMage\Libraries` to its output. The same version directory must therefore appear beside `FLIMage.exe` in a deployed installation.

## Required at runtime for selected hardware

Depending on the configured devices, FLIMage may also require separately supplied binaries such as:

```text
MC700BCommanderDLL.dll
ThorBCM.dll
ThorBCMPA.dll
ThorBScope.dll
ThorECU.dll
ThorMCM3000.dll
ThorMCM301.dll
ThorPMT2100.dll
ThorZStepper.dll
TimeTaggerControllerDLL.dll
ome_tiff_library.dll
```

This list describes expected integration points; it does not authorize redistribution. Install vendor device drivers and SDKs using their official installers.

The settings XML files in this directory are FLIMage configuration templates.
