# FLIMage 4.0.47 public release notes

Application release: July 30, 2026

Public source publication: August 20, 2026

Previous public source release: FLIMage 3.0.5, June 22, 2022

These notes summarize the major user-visible changes since the previous public source release.

## Acquisition and hardware

- Expanded TCSPC acquisition, photon-data saving, and offline photon-file analysis, including operation without connected acquisition hardware.
- Added HydraHarp 500 and PicoHarp 330 support and improved compatibility with PicoQuant and Becker & Hickl devices.
- Expanded resonant-scanning options and improved scan timing and automatic calibration.
- Added advanced controls for Lab Maker's mini-2p, including bidirectional-Y scanning and saved delay settings. FLIMage mini-2p support is compatible only with Lab Maker's mini-2p.
- Added compatibility with NI-DAQmx 18.1, 18.6, 20.1, and 25.0.
- Added and refined stage control for ASI, Zaber, Thorlabs MCM301, and MPC-200 controllers.
- Allowed intensity-only and FLIM acquisition on different channels and supported acquisitions with no practical frame-count limit.
- Added line-scan acquisition and analysis improvements, including uncaging during line scanning and editable trace ROIs.
- Improved physiology acquisition and stimulation behavior, including bipolar stimulation.

## Files and multidimensional data

- Added photon-data archiving and offline reading with improved cancellation, recovery, and large-file support.
- Added OME-TIFF reading and writing, large page indices, and 64-bit TIFF support. OME-TIFF reading and writing require a compatible native `ome_tiff_library.dll`, supplied separately by Florida Lifetime Imaging and not included in this public repository.
- Added direct import of PicoQuant `.ptu` files in the FLIMage file-opening workflow.
- Added opening, saving, navigation, alignment, binning, and concatenation of multidimensional Z/T image data.
- Added AVI export and image-page extraction.
- Improved batch processing, file conversion, concatenation, and handling of large image data sets.

## FLIM analysis and visualization

- Added a phasor-analysis window with interactive display, ROI analysis, and time-course export.
- Added Poisson maximum-likelihood fitting and improved fitting accuracy, stability, speed, initialization, fit ranges, and background handling.
- Added high/low intensity thresholds for masking lifetime images and intensity-weighted ROI mean lifetime.
- Added freehand and traced ROIs, improved polygon and line-scan ROI behavior, and expanded ImageJ ROI import/export.
- Improved multi-page and multi-ROI analysis, time-course handling, and fitting-data export.
- Improved FLIM projection and real-time lifetime-image display performance.
- Fixed undefined-lifetime rendering so pixels without a valid lifetime are displayed as black.

## Remote control and automation

- Expanded remote commands for acquisition, file operations, motor and laser control, ROIs, analysis, digital outputs, page selection, and status queries.
- Added support for multiple simultaneous remote-control connections.
- Improved remote connection, disconnection, and shutdown reliability.
- Updated the Python remote-control client and added examples for batch analysis, reconvolution fitting, PTU processing, and sleep-score-based FLIM analysis.

## Reliability, performance, and build support

- Improved reliability and performance for TCSPC acquisition, photon-data saving, focus mode, Z stacks, resonant scanning, large acquisitions, and file opening.
- Reduced memory use during long imaging and analysis sessions.
- Updated Windows x64 build support and the application framework and dependencies.
- Added Markdown and printable PDF documentation for Poisson maximum-likelihood fitting.

## Changes after the 4.0.47 application release

The August 20 public source publication includes several changes made after the July 30 application release. The application assembly version remains 4.0.47:

- Direct `.ptu` import and the Poisson-fitting documentation described above.
- Updates to the Python batch-processing, plotting, PTU-reading, and FLIM-file-reading tools.
- The undefined-lifetime rendering correction described above.
