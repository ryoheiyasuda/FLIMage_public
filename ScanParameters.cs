using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FLIMage
{
    public class ScanParameters
    {
        public Initialize Init = new Initialize();
        public Acquisition Acq = new Acquisition();
        public DisplaySetting Display = new DisplaySetting();
        public FLIM Spc = new FLIM();
        public Motor_Parameters Motor = new Motor_Parameters();
        public Files_Setting Files = new Files_Setting();
        public Uncaging_Setting Uncaging = new Uncaging_Setting();
        public Digital_Output_Setting DO  = new Digital_Output_Setting();

        public class Initialize
        {
            public Boolean motor_on = true;
            public Boolean FLIM_on = true;
            public String FLIM_mode = "PQ"; //or "BH"
            public Boolean NIDAQ_on = true;

            public String lineClockPort = "Dev4/ctr0";
            public String frameClockPort = "Dev4/ctr1";

            public String triggerPort = "Dev4/Port0/line0";
            public String shutterPort = "Dev4/Port0/line1";

            public String MirrorAOBoard = "Dev4";
            public String mirrorAOPortX = "Dev4/AO0";
            public String mirrorAOPortY = "Dev4/AO1";
            public String mirrorOffsetX = "Dev4/AO5";
            public String mirrorOffsetY = "Dev4/AO6";

            public String masterClock = ""; //"/Dev4/20MHzTimebase";
            public String masterClockPort = "/Dev4/RTSI7";

            public String EOMBoard = "Dev2";
            public String EOM_Port0 = "Dev2/AO0";
            public String EOM_Port1 = "Dev2/AO1";
            public String EOM_Port2 = "Dev2/AO2";
            public String EOM_Port3 = "Dev2/AO4";

            public String UncagingShutterAnalogPort = "Dev2/AO3";

            public String DigitalLinePort = "line2";
            public String DigitalShutterPort = "line3";
            public String DigitalOutput1 = "line4";
            public String DigitalOutput2 = "line5";
            public String DigitalOutput3 = "line6";
            public String TriggerInput = "PFI6";
            public String SampleClockPort = "PFI4";

            public String EOM_slaveClockPort = ""; //"/Dev1/RTSI7";

            public String EOM_AI_Port0 = "Dev4/AI0";
            public String EOM_AI_Port1 = "Dev4/AI1";
            public String EOM_AI_Port2 = "Dev4/AI2";
            public String EOM_AI_Port3 = "Dev4/AI3";

            public String Piezo_Z_Signal = "Dev2/AO3";
            public String Piezo_Z_Monitor = "Dev2/AI3";

            //public String EOM_AI_Trigger = "/Dev4/PFI6";

            public String MarkerInput = "";

            public String ExternalTriggerInputPort = "PFI2";

            public bool UseExternalMirrorOffset = false;

            public int EOM_nChannels = 2;
            public bool AO_uncagingShutter = true;
            public bool DO_uncagingShutter = false;
            public bool use_digitalLineClock = false;
            public bool lineClockAcitveHigh = true;
            public double[] mirrorParkPosition = { -1, -1 };

            public String MotorComPort = "COM1";
            public String MotorHWName = "MP-285A"; //or "MP-285" or "ThorlabMCM3000"
            public String TagLensPort = "COM6";
            public int MotorDisplayUpdateTime_ms = 1000;
            public double[] MotorConversionFactor = { 0, 0, 0 };

            public String MicroscopeSystem = ""; //"ThorBScopeGG"; //"THorBScopeRG" or None
            public String ThorPMTModule = "ThorECU"; //or ThorPMT21000
            public String ThorFlipper = "ThorBCA"; // or ThorBScope
            public bool Use_EPhys = true;

            public bool usePiezo = false;
            public double Piezo_um_per_V = 100; // Total 10 V. If 1000 um, it is 100.

            public int FLIMserial = 0;
            public int ComputerID = 0;

            public double AbsoluteMaxVoltageScan = 10;

            public bool[] imagingLasers = { true, false, false, false, false }; //Up to 5 channels
            public bool[] uncagingLasers = { false, true, false, false, false };
            public bool[] uncagingShutter = { false, false, true, false, false };
            public int[] minimumPower = { 0, 0, 0, 0 };

            public bool openShutterDuringCalibration = false;
        }

        public class Acquisition
        {
            public int pixelsPerLine = 128;
            public int linesPerFrame = 128;
            public int maxNFramePerFile = 4000;
            public bool aveFrame = false; //obsolete for backward compatibility.
            public bool aveFrameSeparately = false;
            public bool[] aveFrameA = new bool[] { false, false };
            public bool aveSlice = false;
            public bool ZStack = true;
            public bool acqFLIM = true; //obsolete for backward compatibility.
            public bool[] acqFLIMA = new bool[] { true, true };
            public bool[] acquisition = new bool[] { true, true };
            public int nAveFrame = 4;
            public int nAveragedFrames = 16;
            public int nFrames = 64;
            public int nSlices = 24;
            public int nAveSlice = 4; //number of slices to be averaged.
            public int nAveragedSlices = 6; //number of "aveSlice"
            public int nImages = 1;
            public int linesPerStripe = 32;
            public bool StripeDuringFocus = false;
            public bool BiDirectionalScan = false;
            public bool SineWaveScan = false;
            public bool[] flipXYScan = { false, false };
            public bool switchXYScan = false;
            public double LineClockDelay = 0.1;
            public int nStripes = 4;
            public bool fastZScan = false;
            public double fillFraction = 0.75; //Minimum is 0.7.
            public double scanFraction = 0.85;
            public double ScanDelay = 0.074;
            public double ScanDelay2 = 0; //not used now.
            public double ScanDelay4 = 0; //not used now.
            public double msPerLine = 2.0;
            public double SliceMergin = 200;
            public double[] FOV_default = { 260.0, 260.0 };
            public double object_magnification_default = 60;
            public double[] field_of_view = new double[] { 260.0, 260.0 };
            // = FOV_default * object_magnification_default  / object_magnification
            public double object_magnification = 60;
            public double[] scanVoltageMultiplier = new double[] { 1.0, 1.0 };
            public double zoom = 3;
            public double XMaxVoltage = 4.5;
            public double YMaxVoltage = 4.5;
            public double Rotation = 0.0;
            public double XOffset = 0.0;
            public double YOffset = 0.0;
            public double imageInterval = 120;
            public double sliceInterval = 0;
            public double sliceStep = 1.0;
            public int nChannels = 2;
            public int outputRate = 250000;
            public int inputRate = 250000;
            public String triggerTime = "";
            public int[] power = new int[] { 10, 10, 10, 10 };
            public bool externalTrigger = false;
            public double ExpectedLaserPulseRate_MHz = 80.0;

            public int FastZ_nSlices = 1;
            public double FastZ_msPerLine = 0;
            public double FastZ_Freq = 310000;
            public double FastZ_Amp = 8;
            public bool FastZ_phase_detection_mode = false;
            public double[] FastZ_Phase = new double[] { 30.0, 30.0, 30.0 };
            public double[] FastZ_PhaseRange = new double[] { 35, 145 };
            public double FastZ_umPerSlice = 1.0;
            public double FastZ_degreePerSlice = 4.0;

            public double[] FOV_calculation(double objMag)
            {
                double[] fov = new double[2];
                object_magnification = objMag;
                for (int i = 0; i < 2; i++)
                {
                    field_of_view[i] = FOV_default[i] * object_magnification_default / object_magnification;
                    fov[i] = field_of_view[i] / zoom * scanVoltageMultiplier[i];
                }

                return fov;
            }

            public void FOV_to_default()
            {
                for (int i = 0; i < 2; i++)
                    FOV_default[i] = field_of_view[i] / object_magnification_default * object_magnification;
            }

            public double PixelTime()
            {
                return ((msPerLine * fillFraction) / (double)pixelsPerLine / 1000.0);
            }
            public double frameInterval()
            {
                return ((double)linesPerFrame * msPerLine / 1000.0);
            }

            public double[] frameAveInterval()
            {
                double[] frameAveInterval = new double[aveFrameA.Length];
                for (int i = 0; i < aveFrameA.Length; i++)
                {
                    if (aveFrameA[i])
                        frameAveInterval[i] = ((double)linesPerFrame * msPerLine * (double)nAveFrame / 1000.0);
                    else
                        frameAveInterval[i] = ((double)linesPerFrame * msPerLine / 1000.0);
                }

                return frameAveInterval;
            }
        }

        public class DisplaySetting
        {
            public double[] Intensity_Range1 = { 0, 100 };
            public double[] Intensity_Range2 = { 0, 100 };
            public double[] FLIM_Intensity_Range1 = { 0, 100 };
            public double[] FLIM_Intensity_Range2 = { 0, 100 };
            public double[] FLIM_Range1 = { 1.6, 2.7 };
            public double[] FLIM_Range2 = { 1.6, 2.7 };
            public int filterWindow_FLIM = 0;
            //public bool SavePagesInMemory = false;
        }

        public class Digital_Output_Setting
        {
            public int NChannels = 3;
            public string name = "pulse set";
            public bool DO_whileImage = false;
            public bool sync_withFrame = false;
            public bool sync_withSlice = true;
            public double FramesBeforeDO = 32;
            public int SlicesBeforeDO = 32;
            public double FrameInterval = 4;
            public double SliceInterval = 1;

            public int pulse_number = 1;
            public int[] nPulses = { 1, 0, 0 };
            public double[] pulseWidth = { 6, 6, 6 }; //ms
            public double[] pulseISI = { 2048, 2048, 2048 };
            public double[] pulseDelay = { 100, 100, 100 };
            public bool[] active_high = { true, true, true };

            public double sampleLength = 300; //milisecond.
            public double outputRate = 4000;

            public double baselineBeforeTrain_forFrame = 2048;
            public double pulseSetInterval_forFrame = 2048;

            public int trainRepeat = 1;
            public double trainInterval = 2048; //ms
        }

        public class Uncaging_Setting
        {
            public string name = "pulse set";
            public int pulse_number = 1;

            public bool uncage_whileImage = false;
            public bool sync_withFrame = false;
            public bool sync_withSlice = true;
            public double FramesBeforeUncage = 32;
            public int SlicesBeforeUncage = 32;
            public double Uncage_FrameInterval = 4;
            public double Uncage_SliceInterval = 1;
            public double AnalogShutter_delay = 4; //ms
            public double DigitalShutter_delay = 8; //ms
            public double Mirror_delay = 4; //ms

            public bool shutter_activeHigh = true;

            //public double delay = 8192; //ms
            public int nPulses = 30;
            public double pulseWidth = 6; //ms
            public double pulseISI = 2048;
            public double pulseDelay = 4096;
            public double sampleLength = 6000;

            public double outputRate = 4000;

            public double baselineBeforeTrain_forFrame = 2048;
            public double pulseSetInterval_forFrame = 2048;

            public int trainRepeat = 1;
            public double trainInterval = 2048; //ms

            public double Power = 25; //percent.
            public double[] Position = { -1, -1 }; //Frac in image.
            public double[] PositionV = { 0, 0 }; //Voltage
            public double[] CalibV = { 0, 0 }; //Voltage
            //Multipositions::
            public bool multiUncagingPosition = false;
            public bool rotatePosition = false;
            public int currentPosition = 0; //0 = current position. 1, 2, 3... are the numbered positions.
            public double[] UncagingPositionsX = { 0 };
            public double[] UncagingPositionsY = { 0 };
            public double[] UncagingPositionsVX = { 0 }; //voltage
            public double[] UncagingPositionsVY = { 0 }; //voltage

            public bool MoveMirrorsToUncagingPosition = true;
            public bool TurnOffImagingDuringUncaging = true;
        }

        public class FLIM
        {
            public analysis1 analysis = new analysis1();
            public TCSPC_controls.FLIM_Parameters.RateInfo datainfo = new TCSPC_controls.FLIM_Parameters.RateInfo();
            public TCSPC_controls.FLIM_Parameters.SPCData spcData = new TCSPC_controls.FLIM_Parameters.SPCData();

            public class analysis1
            {
                public double[] offset = { 2.0, 1.8 };
                public int[] fit_range1 = { 3, 120 };
                public int[] fit_range2 = { 3, 120 };
                public double[] fit_param1 = { 1000, 2.6, 100, 1.1, 0.1, 1.8 };
                public double[] fit_param2 = { 1000, 2.6, 100, 1.1, 0.1, 1.8 };
            }
        }

        public class Motor_Parameters
        {
            public double stepXY = 5;
            public double stepZ = 1;
            public double[] velocity = { 1500, 1500, 1500 };
            public double resolutionX = 0.04;
            public double resolutionY = 0.04;
            public double resolutionZ = 0.005;
            public double[] motorPosition = { 0, 0, 0 };
        }

        public class Files_Setting
        {
            public String baseName = "test";
            public String FLIMfolderPath = "\\FLIMage";
            public String pathName;
            public String pathNameIntensity;
            public String pathNameFLIM;
            public String initFolderPath;
            public String initFileName;
            public String deviceFileName;
            public String defaultInitFile;
            public String commandPathName;
            public String eventOutputListFileName = "eventOutputList.txt";
            public String uncagePathName;
            public String windowsInfoPath;
            public String FLIMageOutputFileName = "instructions_fromFLIMage.txt";
            public String ClientOutputFileName = "instructions_fromClient.txt";
            public String parameterFile = "scan_parameters.txt";
            public String fileName = "test001";
            public bool numberedFile = true;
            public bool channelsInSeparatedFile = false;
            public int fileChannel = 0;
            public int fileCounter = 1;
            public String extension = ".flim";
            public String BH_initFile = "spcm.ini";
            public bool useCommandFile = false;

            public Files_Setting()
            {
                pathName = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + FLIMfolderPath + Path.DirectorySeparatorChar + "test";
                pathNameIntensity = pathName + Path.DirectorySeparatorChar + "Intensity";
                pathNameFLIM = pathName + Path.DirectorySeparatorChar + "FLIM";
                initFolderPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + FLIMfolderPath + Path.DirectorySeparatorChar + "Init_Files";
                initFileName = initFolderPath + Path.DirectorySeparatorChar + "FLIM_init.txt";
                deviceFileName = initFolderPath + Path.DirectorySeparatorChar + "FLIM_deviceFile_V1.txt";
                BH_initFile = initFolderPath + Path.DirectorySeparatorChar + "spcm.ini";
                defaultInitFile = initFolderPath + Path.DirectorySeparatorChar + "Default.txt";
                commandPathName = initFolderPath + Path.DirectorySeparatorChar + "Command";
                uncagePathName = initFolderPath + Path.DirectorySeparatorChar + "Uncaging";
                windowsInfoPath = initFolderPath + Path.DirectorySeparatorChar + "WindowsInfo";
            }

            public void fromFullNameToFolderPathAndFileName(String fullFileName)
            {
                fileName = Path.GetFileNameWithoutExtension(fullFileName);
                pathName = Path.GetDirectoryName(fullFileName);
                extension = Path.GetExtension(fullFileName);

                if (!fileName.Contains("_Ch"))
                    channelsInSeparatedFile = false;


                if (fileName.Length > 3 && !Int32.TryParse(fileName.Substring(fileName.Length - 3), out int result))
                    numberedFile = false;
                else
                    numberedFile = true;
            }

            public void fullNameTofileNames(String fullFileName)
            {
                fileName = Path.GetFileNameWithoutExtension(fullFileName);
                pathName = Path.GetDirectoryName(fullFileName);
                extension = Path.GetExtension(fullFileName);

                int len = fileName.Length;
                if (Int32.TryParse(fileName.Substring(len - 3), out int result))
                {
                    numberedFile = true;
                    fileCounter = result;

                    if (fileName.Substring(len - 4, 1) == "_" && fileName.Substring(len - 8, 3) == "_Ch" && Int32.TryParse(fileName.Substring(len - 5, 1), out result))
                    {
                        fileChannel = result;
                        channelsInSeparatedFile = true;
                        baseName = fileName.Substring(0, (len - 8));
                    }
                    else
                    {
                        fileChannel = 0;
                        channelsInSeparatedFile = false;
                        baseName = fileName.Substring(0, (len - 3));
                    }
                }
                else
                {
                    numberedFile = false;
                    fileCounter = 0;
                    if (fileName.Substring(len - 4, 3) == "_Ch" && Int32.TryParse(fileName.Substring(len - 1, 1), out result))
                    {
                        fileChannel = result;
                        channelsInSeparatedFile = true;
                        baseName = fileName.Substring(0, (len - 4));
                    }
                    else
                    {
                        fileChannel = 0;
                        channelsInSeparatedFile = false;
                        baseName = fileName;
                    }
                }
            }

            public String fullName()
            {
                fileChannel = 0;
                return fullName(fileChannel);
            }

            public String fullName(int channel)
            {
                fileChannel = channel;
                if (numberedFile)
                {
                    if (!channelsInSeparatedFile)
                        fileName = String.Format("{0}{1:000}", baseName, fileCounter);
                    else
                        fileName = String.Format("{0}_Ch{1}_{2:000}", baseName, channel + 1, fileCounter);
                    return String.Format("{0}{1}{2}{3}", pathName, Path.DirectorySeparatorChar, fileName, extension);
                }
                else
                {
                    if (!channelsInSeparatedFile)
                        fileName = String.Format("{0}", baseName);
                    else
                        fileName = String.Format("{0}_Ch{1}", baseName, channel + 1);

                    return (String.Format("{0}{2}fileName{1}", fileName, extension, Path.DirectorySeparatorChar));
                }
            }

            public String AutoPathName()
            {
                String str = String.Format("{0:yyyyMMdd}", DateTime.Now);
                pathName = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + FLIMfolderPath + "\\Data-" + str;
                pathNameIntensity = pathName + Path.DirectorySeparatorChar + "Intensity";
                pathNameFLIM = pathName + Path.DirectorySeparatorChar + "FLIM";
                return pathName;
            }
        }


    }
}
