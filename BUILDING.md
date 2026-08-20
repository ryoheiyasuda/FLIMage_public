# Building the public FLIMage source

FLIMage is a Windows x64 application. Hardware acquisition requires the appropriate vendor SDKs, drivers, and runtime libraries. The FLIMage-built x64 `TCSPC_Decode.dll` runtime binary is included.

## Toolchain

Install:

- Windows 10 or later.
- Visual Studio 2022 or Visual Studio 18 with .NET desktop development.
- The .NET Framework 4.8 developer/targeting pack.
- Visual Studio package restore support.

The production projects target .NET Framework 4.8.

Only `x64` is supported. Hardware drivers and native dependencies make `x86` unsupported.

## Restore NuGet packages

From the repository root:

```powershell
$msbuild = "C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe"
& $msbuild ".\FLIMage\FLIMageWin10.sln" /t:Restore `
  /p:RestorePackagesConfig=true /p:RestorePackagesPath=".\FLIMage\packages" `
  /p:Configuration=Release /p:Platform=x64
```

Adjust the Visual Studio edition/version in the path as needed. Visual Studio can also restore the `packages.config` dependencies when the solution is opened.

## Supply external binaries

Follow [FLIMage/Libraries/README.md](FLIMage/Libraries/README.md). The only committed DLL is the FLIMage-built x64 `TCSPC_Decode.dll`.

For a full acquisition build, the project must be able to resolve at least:

```text
FLIMage\Libraries\NationalInstruments.Common.dll
FLIMage\Libraries\NationalInstruments.DAQmx.dll
```

The application requires additional vendor libraries at runtime for the hardware features being used. MultiClamp workflows require a separately supplied `MC700BCommanderDLL.dll`.

### NI-DAQmx version selection

FLIMage is not limited to NI-DAQmx 18.6. At startup, `WindowsUtil.SetupNIAssemblyBinding()` reads the file version of the installed `%WINDIR%\System32\nicaiu.dll` and selects one of these subdirectories beside `FLIMage.exe`:

| Installed NI-DAQmx line | Runtime assembly directory |
| --- | --- |
| 18.1 | `NI18.1` |
| 18.6 | `NI18.6` |
| 20.1 | `NI20.1` |
| 25.0 | `NI25.0` |

Place the matching NI-provided .NET assemblies in the corresponding directory. During a repository build, the same layout can be created under `FLIMage\Libraries`; the application project copies it recursively to the output directory. The NI project references use compatible assembly resolution rather than restricting the application to one exact DAQmx release.

## Build the production solution

With dependencies installed, build from a Visual Studio developer PowerShell:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" `
  ".\FLIMage\FLIMageWin10.sln" /t:Build /p:Configuration=Release /p:Platform=x64
```

Replace `Professional` with `Community`, `Enterprise`, or `BuildTools` as appropriate. The helper script performs the same operation and searches common Visual Studio locations:

```powershell
.\build_flimage.ps1 -Configuration Release
```

To use Visual Studio 18 explicitly:

```powershell
.\build_flimage.ps1 -Configuration Release -VisualStudioVersion 18
```

## Build without vendor-dependent projects

MathLibrary can be built independently after package restore:

```powershell
& $env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe `
  ".\FLIMage\MathLibrary\MathLibrary.csproj" /t:Build /p:Configuration=Release /p:Platform=x64
```

The main application, `MicroscopeHardwareLibs`, and `PhysiologyCSharp` reference National Instruments assemblies and will not compile until those references resolve.

## TCSPC decoder runtime

`TCSPC_Controls` provides the managed integration layer. At runtime, TCSPC acquisition code loads the bundled x64 decoder DLL from the application output directory; the application project copies files from `FLIMage/Libraries` during the build.

## Expected limitations

- A clean public checkout is not a turnkey hardware installation.
- Hardware features only work when the matching vendor driver, SDK, and runtime DLL versions are installed.
- Intel MKL is optional; MathNet uses its managed provider when MKL is unavailable.
- The installer is maintained in the separate FLIMage_Installer repository.
