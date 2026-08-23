# FLIMage

FLIMage is Windows software for microscope control, time-correlated single-photon counting (TCSPC), real-time fluorescence-lifetime imaging (FLIM), image analysis, and physiology workflows. It is developed and used by the Yasuda Lab at the Max Planck Florida Institute for Neuroscience.

## Public source release

| Item | Version |
| --- | --- |
| FLIMage application | 4.0.47 |
| Application release date | July 30, 2026 |
| Public source snapshot | August 20, 2026 |
| Main managed target | .NET Framework 4.8 |
| Supported platform | Windows x64 |

The application version is defined in `FLIMage/App/Properties/AssemblyInfo.cs`. Changes after the 4.0.47 release include the August 20 correction for undefined lifetime-map colors; the assembly version remains 4.0.47.

See [RELEASE_NOTES.md](RELEASE_NOTES.md) for release highlights and [BUILDING.md](BUILDING.md) for build requirements.

## Pre-compiled installer

This public repository provides the FLIMage source release but does not
distribute a pre-compiled, pre-packaged installer because an installer contains
third-party runtime DLLs. To request a pre-compiled, pre-packaged installer,
contact [Ryohei Yasuda](mailto:ryohei.yasuda@mpfi.org).

## What this public repository contains

- The FLIMage application and managed production libraries.
- Managed microscope, TCSPC-control, tag-lens, and physiology source.
- The FLIMage-built x64 `TCSPC_Decode.dll` runtime binary.
- MathLibrary source and [API documentation](FLIMage/MathLibrary/API.md).
- Remote-control examples, Python tools, icons required by the application, and user documentation.

## Documentation

- [Build instructions](BUILDING.md)
- [Release notes](RELEASE_NOTES.md)
- [FLIMage user manual](Manual/FLIMage%20User%20Manual.pdf)
- [Remote-control tutorial](Manual/RemoteControl_Tutorial.pdf)
- [Poisson maximum-likelihood fitting](Manual/Poisson_Maximum_Likelihood_Fitting.md)
- [MathLibrary API](FLIMage/MathLibrary/API.md)

## External software and hardware

A full acquisition build requires software supplied by the relevant hardware vendors. At minimum:

- National Instruments NI-DAQmx with .NET Framework support. FLIMage supports multiple NI driver lines: its startup resolver explicitly recognizes NI-DAQmx 18.1, 18.6, 20.1, and 25.0, then loads the matching assemblies from a version-specific folder beside the executable.
- The vendor SDK and runtime libraries for each enabled TCSPC device and microscope component.
- The bundled x64 `TCSPC_Decode.dll` for acquisition workflows.

NuGet-managed dependencies are restored from their package metadata. Important direct versions in this snapshot include:

- MathNet.Numerics 5.0.0
- BitMiracle.LibTiff.NET 2.4.560
- System.Numerics.Vectors 4.6.1
- SharpAvi 3.0.1
- SharpCompress 0.49.1
- ZstdSharp.Port 0.8.7
- System.Text.Json 10.0.5

See [FLIMage/Libraries/README.md](FLIMage/Libraries/README.md) for expected binary names and placement.

## Repository layout

- `FLIMage/App` — main application.
- `FLIMage/MathLibrary` — fitting, matrix, image-processing, and FLIM-analysis library.
- `FLIMage/Utilities` — shared managed utilities.
- `Hardware` — managed hardware-control source.
- `PhysiologyCSharp` — physiology acquisition and MC700B integration.
- `PIPE` — remote-control client/server examples.
- `Python Script` — Python analysis and automation examples.
- `Manual` — user and technical documentation.

## License

See the [license terms](LICENSE) governing use and redistribution of FLIMage.
