using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using FLIMage;
using TCSPC_controls;
using static System.Windows.Forms.AxHost;
using System.IO.Compression;

namespace FLIMage.FlowControls
{
    public class FLIM_Utilities
    {
        static public FLIM_Parameters ConvertStateToFLIMParameters(ScanParameters state1, bool read_photon_file, FLIM_Parameters parameters)
        {
            if (parameters == null)
                parameters = new FLIM_Parameters();

            bool isFiberPhotometry = (state1?.Init?.MicroscopeSystem ?? "").ToLower().Contains("fiber");

            parameters.nDtime = state1.Spc.spcData.n_dataPoint;

            parameters.averageFrame = (bool[])state1.Acq.aveFrameA.Clone();
            parameters.n_average = state1.Acq.nAveFrame;

            if (state1.Acq.resonantScanning)
                parameters.focusAverage = state1.Acq.nAveFrame_focus_resonant;
            else
                parameters.focusAverage = state1.Acq.nAveFrame_focus;

            parameters.nFrames = state1.Acq.nFrames;
            parameters.nChannels = state1.Acq.nChannels;
            parameters.fastZScan.nFastZSlices = state1.Acq.FastZ_nSlices;
            parameters.enableFastZscan = state1.Acq.fastZScan;

            double msPerLine1 = state1.Acq.msPerLineActual(); ;
            int pixelsPerLine = state1.Acq.pixelsPerLine;

            // Default imaging path
            parameters.msPerLine = msPerLine1;
            parameters.nPixels = pixelsPerLine;
            parameters.nLines = state1.Acq.linesPerFrame + state1.Acq.AddLinesForSlaveMode;

            parameters = SetupFLIMParameters_ImageDelay(parameters, state1);

            parameters.spcData = state1.Spc.spcData;
            ApplyFastScanFirstFrameSkip(state1, read_photon_file, isFiberPhotometry);

            if (state1.Acq.BiDirectionalScanY)
                parameters.spcData.SkipFirstLines = 0;
            else
                parameters.spcData.SkipFirstLines = state1.Acq.SkipFirstLines;

            //Perhaps not necessary..
            if (parameters.rateInfo.syncRate[0] != 0)
                parameters.spcData.time_per_unit = 1.0 / (double)parameters.rateInfo.syncRate[0]; //sync with laser pulses.
            else
                parameters.spcData.time_per_unit = 1.244e-8;

            if (state1.Spc.spcData.BoardType == "PQ" && state1.Spc.spcData.acq_modePQ == 2)
                parameters.spcData.time_per_unit = parameters.spcData.resolution[0] * 1.0e-12;
            // (calculated in pixel_count in TCSPC_control.dll)

            parameters.fastZScan.VoxelTimeUs = 1000.0 / parameters.fastZScan.FrequencyKHz / parameters.fastZScan.nFastZSlices;
            parameters.fastZScan.VoxelCount = (int)(parameters.fastZScan.VoxelTimeUs / parameters.spcData.time_per_unit / 1e6);


            parameters.eraseMemory_afterAcqisition = !(state1.Acq.aveSlice && state1.Acq.nSlices > 1);

            double fillFraction = state1.Acq.resonantScanning || state1.Acq.polygonScanning
                ? state1.Acq.fillFraction_resonant
                : state1.Acq.fillFraction;
            parameters.fillFraction = fillFraction;
            parameters.pixel_time = msPerLine1 * fillFraction / (double)pixelsPerLine / 1000.0;

            parameters.LinesPerStripe = state1.Acq.linesPerStripe;
            parameters.nStripes = state1.Acq.nStripes;

            if ((double)state1.Acq.linesPerFrame * msPerLine1 > 255.0) //more than 4 Hz
                parameters.StripeDuringFocus = state1.Acq.StripeDuringFocus;
            else
                parameters.StripeDuringFocus = false;

            parameters.acquireFLIM = state1.Acq.acqFLIMA;
            parameters.acquisition = state1.Acq.acquisition;


            parameters.spcData.savePhotonsInFile = state1.Acq.photon_file_format;

            parameters.read_from_file = read_photon_file;

            parameters.rateInfo.syncRate = state1.Spc.datainfo.syncRate;

            // ------------------------------------------------------------
            // Fiber photometry override: single pixel (1x1) time-binned frames
            // ------------------------------------------------------------
            if (isFiberPhotometry)
            {
                parameters.fiberPhotometryMode = true;
                // IMPORTANT: do NOT reuse msPerLineActual() here. It can be <1 ms (resonant/polygon defaults),
                // which would make the fiber sampling interval wrong. Use the dedicated fiberBin_ms instead.
                state1.Acq.fiberPhotometryMode = true;
                state1.Acq.fiberStartMode = state1.Acq.externalTrigger ? 1 : 0;

                parameters.fiberBin_ms = state1.Acq.fiberBin_ms;
                parameters.fiberStartMode = state1.Acq.fiberStartMode;

                if (state1.Acq.fiberBin_ms > 0)
                    state1.Acq.msPerLine = state1.Acq.fiberBin_ms;

                // Use the fiber bin as the sampling time per line.
                parameters.msPerLine = state1.Acq.fiberBin_ms;

                // Keep the entire recording in ONE .flim file by default.
                // (Default init files often have maxNFramePerFile=100, which would otherwise split and bump fileCounter.)
                if (state1.Acq.nFrames > 0)
                    state1.Acq.maxNFramePerFile = state1.Acq.nFrames;
                else
                    state1.Acq.maxNFramePerFile = 320000;

                state1.Acq.linesPerFrame = Math.Max(1, state1.Acq.linesPerFrame);

                parameters.nPixels = 1;
                parameters.nLines = Math.Max(1, state1.Acq.linesPerFrame);
                parameters.fillFraction = 1.0;
                parameters.pixel_time = state1.Acq.fiberBin_ms / 1000.0; // 1 pixel covers the entire bin

                parameters.LinesPerStripe = 1;
                parameters.nStripes = 1;
                parameters.StripeDuringFocus = false;
                parameters.BiDirectionalScanX = 0;
                parameters.BiDirectionalScanY = 0;
                parameters.SineWaveScan = false;

                // No line skipping in single-point mode
                parameters.spcData.SkipFirstLines = 0;
            }
            else
            {
                parameters.fiberPhotometryMode = false;
                state1.Acq.fiberPhotometryMode = false;
            }

            parameters.lineScanMode = state1.Acq.isLineScanAcquisition;

            return parameters;
        }

        static void ApplyFastScanFirstFrameSkip(ScanParameters state1, bool read_photon_file, bool isFiberPhotometry)
        {
            if (state1?.Spc?.spcData == null || state1.Acq == null)
                return;

            bool isMiniScope = state1.Acq.enableMiniScopeClock ||
                ((state1.Init?.MicroscopeSystem ?? "").IndexOf("mini", StringComparison.OrdinalIgnoreCase) >= 0);
            bool skipSettlingFrame = !read_photon_file &&
                !isFiberPhotometry &&
                (state1.Acq.resonantScanning || isMiniScope);

            if (skipSettlingFrame)
            {
                if (state1.Spc.spcData.SkipFirstFrames < 1)
                    state1.Spc.spcData.SkipFirstFrames = 1;
            }
            else if (state1.Spc.spcData.SkipFirstFrames == 1)
            {
                state1.Spc.spcData.SkipFirstFrames = 0;
            }
        }


        static FLIM_Parameters SetupFLIMParameters_ImageDelay(FLIM_Parameters parameters, ScanParameters state1)
        {
            parameters.SineWaveScan = state1.Acq.SineWaveScan;
            if (state1.Acq.SineWaveScan)
            {
                parameters.BiDirectionalScanX = 1;
                parameters.BiDirectionalScanY = state1.Acq.BiDirectionalScanY ? 1 : 0;
            }


            if (state1.Acq.polygonScanning)
            {
                parameters.BiDirectionalScanX = 0;
                parameters.BiDirectionalScanY = 0;
            }
            else if (state1.Acq.resonantScanning)
            {
                parameters.BiDirectionalScanX = 2;
                parameters.BiDirectionalScanY = state1.Acq.BiDirectionalScanY ? 1 : 0;
            }
            else
            {
                parameters.BiDirectionalScanX = state1.Acq.BiDirectionalScan ? state1.Acq.BidirectionalTriggerPerLine : 0;
                parameters.BiDirectionalScanY = state1.Acq.BiDirectionalScanY ? 1 : 0;

            }


            double AcqusitionDelay = HardwareControls.IOControls.GetAcquisitionDelay_ms(state1);
            double BiDirectionalDelay = HardwareControls.IOControls.GetBidirectionalDelay_ms(state1);

            parameters.BiDirectionalDelay = BiDirectionalDelay;
            parameters.AcquisitionDelay = AcqusitionDelay;

            return parameters;
        }

        static public void OpenBinaryTest2()
        {
            var dirname = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "data");
            var zip_filename = Path.Combine(dirname, "test1.photon");
            var photon_file = "vsdst_stim_cell1nostim001.phtn";
            string header;
            using (ZipArchive archive = ZipFile.OpenRead(zip_filename))
            {
                var file_list = archive.Entries.Where(e => e.FullName == photon_file).ToList();
                var entry = file_list[0];
                using (StreamReader s = new StreamReader(entry.Open()))
                {
                    // stream with the file
                    header = s.ReadToEnd();

                }

            }
            Console.WriteLine(header);

            using (ZipArchive archive = ZipFile.OpenRead(zip_filename))
            {
                var file_list = archive.Entries.Where(e => e.FullName.Contains(".bin")).ToList();
                var entry = file_list[1];
                var full_filename = Path.Combine(dirname, entry.FullName);
                if (!File.Exists(full_filename))
                    entry.ExtractToFile(full_filename);
            }
        }

        /// <summary>
        /// Just to test how to use ZIP archive....
        /// </summary>
        static public void OpenBinaryTest()
        {
            var dirname = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "data");

            string prefix = "vsdst_stim_cell1nostim001";
            string photon_file = Path.Combine(dirname, "vsdst_stim_cell1nostim001.phtn");
            var searchPattern = prefix + "*.bin";
            var filepaths = Directory.GetFiles(dirname, searchPattern);
            string header = File.ReadAllText(photon_file);

            var save_filename = Path.Combine(dirname, "test1.photon");
            var zip = ZipFile.Open(save_filename, ZipArchiveMode.Create);
            zip.CreateEntryFromFile(photon_file, Path.GetFileName(photon_file), CompressionLevel.Fastest);
            zip.Dispose();

            var zip2 = ZipFile.Open(save_filename, ZipArchiveMode.Update);
            foreach (var file in filepaths)
            {
                // Add the entry for each file
                zip2.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Fastest);
                File.Delete(file);
            }
            // Dispose of the object when we are done
            zip2.Dispose();


            //File.WriteAllBytes(save_filename, Encoding.UTF8.GetBytes(header));
            //foreach (var filepath in filepaths)
            //{
            //    byte[] bytes = File.ReadAllBytes(filepaths[0]);
            //    using (var stream = new FileStream(save_filename, FileMode.Append))
            //    {
            //        stream.Write(bytes, 0, bytes.Length);
            //    }
            //}
        }

    }
}
