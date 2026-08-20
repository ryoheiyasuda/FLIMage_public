using FLIMage.Analysis;
using FLIMage.FileFormat;
using FLIMage.FlowControls;
using FLIMage.HardwareControls.StageControls;
using FLIMage.Uncaging;
using MathLibrary;
using MicroscopeHardwareLibs;
using MicroscopeHardwareLibs.Stage_Contoller;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCSPC_controls;
using Utilities;

namespace FLIMage
{
    public class FLIMage_IO
    {
#if DEBUG
        int DEBUGMODE = 1;
#else
        int DEBUGMODE = 0;
#endif
        FLIMageMain flimage;
        public ScanParameters State;

        //Timer to acquire count rate.
        Timer RateTimer = new Timer();

        public bool use_nidaq = true; //use NIDAQ card
        public bool use_pq = true; // use PicoQuant card
        public bool use_bh = false; // use Becker and Hickl card

        public Stopwatch UIstopWatch_Loop = new Stopwatch();
        public Stopwatch UIstopWatch_Image = new Stopwatch();
        public Stopwatch SW_PerformanceMonitor = new Stopwatch();
        public Stopwatch SW_PerformanceMonitorFrame = new Stopwatch();

        // Throttle UI counter updates during acquisition to reduce cross-thread overhead.
        // (UpdateCounters triggers UI work; doing it every frame can become a bottleneck at high frame rates.)
        private int _lastCountersUpdateTick = Environment.TickCount;
        private const int CountersUpdateIntervalMs = 100;
        private const int PhotonStreamChunkBytes = 1024 * 1024;

        //For synchronization.
        readonly object syncFLIMacq = new object();
        readonly object saveBufferObj = new object();
        readonly object syncFLIMdisplay = new object();
        readonly object syncFLIMsave = new object();
        readonly object toolTipObj = new object();
        readonly object waitSliceTaskobj = new object();
        readonly object waitLoopImageTaskobj = new object();

        public ScanParameters SaveState;

        public bool imageSequencing = false;
        public bool snapShot { get; private set; }
        public bool focusing { get; private set; }
        public bool grabbing { get; private set; }
        public bool refocusing = false;
        public bool post_grabbing_process { get; private set; }
        public bool looping { get; private set; }
        public bool allowLoop { get; private set; }
        public bool stopGrabActivated { get; private set; }
        public bool acquisition_done_event_activated { get; private set; }
        public bool force_stop = false;
        public bool runningImgAcq = false;

        // ------------------------------------------------------------
        // Line scan trace mode (trace ROI -> 1-line frame)
        // ------------------------------------------------------------
        private bool _lineScanTraceActive = false;
        private LineScanTraceBackup _lineScanTraceBackup = null;

        private sealed class LineScanTraceBackup
        {
            public int nFrames;
            public int linesPerFrame;
            public int linesPerStripe;
            public int nStripes;
            public int skipFirstLines;
            public bool biDirectionalScan;
            public bool biDirectionalScanY;
            public bool sineWaveScan;
            public double fillFraction;
            public double scanFraction;
            public double scanDelay;
            public int lineScanTimePoints;
            public bool isLineScanAcquisition;
        }

        public bool physWaitforTrigger = false;
        public bool uncagingWaitForTrigger = false;
        //////Counters for frames etc.
        //  int n_average;

        public bool[] newFile = new bool[] { false, false }; //For saving.

        public int averageCounter;
        public int averageSliceCounter;
        public int internalFrameCounter;
        // int internalAveFrameCounter;
        public int internalSliceCounter;
        public int internalImageCounter;

        //// Serve as an acquisiton counter. Event activated by NI mirror card.
        public int AO_FrameCounter;
        public List<DateTime> real_acquired_datetime = new List<DateTime>();
        public int saved_realCounter = 0;
        // int internalStripeCounter;
        public int savePageCounterTotal = 0;
        public int savePageCounter = 0;
        public int savePageBufferCounter = 0;
        public int displayPageCounterTotal = 0;
        public int displayPageCounter = 0;
        public int deletedPageCounter = 0;
        private int sleep_counter = 0;
        public double measuredSliceInterval;
        Task waitSlice, waitImage;
        Task saveTask;
        Task photonStreamTask;
        public bool save_image_busy = false;
        private volatile bool _saveErrorNotified = false;
        // When true, suppresses the "Overwrite?" confirmation dialog for this FLIMage instance.
        // This is controlled via remote commands (e.g., SetOverwriteWarningOff/On).
        public volatile bool suppressOverwritePrompt = false;
        bool waitingImageTask = false;
        bool waitingSliceTask = false;
        private bool _pendingFocusStart = false;

        public volatile bool read_photon_file = false;

        ////// FLIM data stroage. All temporal. 
        List<UInt16[][][,,]> FLIMSaveBuffer = new List<ushort[][][,,]>(); //Used only when images are not saved in memory.
        List<DateTime> acquiredTimeList = new List<DateTime>(); //Store acquisition time info.
        DateTime acquiredTime;
        //public FLIMData FLIM_ImgData;

        //National instrument DAQ card 
        public HardwareControls.IOControls.LineClockByCounter lineClock;
        public HardwareControls.IOControls.MiniScopeClock miniScopeClock;
        public HardwareControls.IOControls.TriggeredLineClock lineTriggeredSampleClock;
        public HardwareControls.IOControls.AnalogOutput AO_Mirror_EOM;
        //public HardwareControls.IOControls.AnalogOutput AO_Mirror_EOM_withZScan;
        public HardwareControls.IOControls.AnalogOutput Resonant_Mirror;
        public HardwareControls.IOControls.AnalogOutput Resonant_EOM;
        public HardwareControls.IOControls.dioTrigger dioTrigger;
        public HardwareControls.IOControls.ShutterCtrl shutterCtrl;
        public HardwareControls.IOControls.ResonantOn resonant_on;
        public HardwareControls.IOControls.ResonantScannerSwitch resonant_switch;
        public HardwareControls.IOControls.DigitalOutputControl digitalOutput_WClock; //for time control
        public HardwareControls.IOControls.PiezoControl piezo;

        public string DigitalUncagingShutterPort = "";

        ////// Uncaging prameters.
        public UncagingCalibration uc;
        readonly Timer UncagingTimer = new Timer();
        public double[] uncaging_Calib = new double[2];
        int uncaging_DO_SliceCounter = 0;


        ///// Shaidng
        public HardwareControls.IOControls.Shading shading;

        ///// Microscope System
        public MicroscopeSystem microscope_system = MicroscopeSystem.ScanImageGG;

        ////Thor labs
        public ThorECU thorECU;
        public int maxValue_Resonant = 1023;
        public bool thorECU_on = false;

        ///////FLIM card
        public FiFio_multiBoards FiFo_acquire;
        public FLIM_Parameters parameters;
        public bool tcspc_on = true;
        /// <summary>
        /// App-level simulation flag.
        ///
        /// IMPORTANT: This is NOT "SimPQ".
        /// "SimPQ" is simulated inside TCSPC_Decode.dll and should use the normal DLL acquisition path.
        /// This flag is only used when explicitly calling <see cref="RunSimulation"/> (managed synthetic generator).
        /// </summary>
        public bool simulation_mode = false;

        //////Rate
        int badSyncRateCounter = 0;
        int badSyncRateMaxCount = 10;

        //////  Event handler for broadcasting.
        public event FLIMage_EventHandler EventNotify;
        public ProcessEventArgs envt = new ProcessEventArgs("", null);
        public delegate void FLIMage_EventHandler(FLIMage_IO flimage_io, ProcessEventArgs evnt);

        public FileIO fileIO;
        string filename_last = "";
        string filename_timestamp = "";

        public PhotonFileHandle photon_file_handle;

        public FLIMage_IO(FLIMageMain FLIMage)
        {
            flimage = FLIMage;
            State = flimage.State;
            fileIO = new FileIO(State); //initialize fileIO
            fileIO.HoldFastWriterOpen = ShouldHoldFastWriterOpen(State);

            use_nidaq = State.Init.NIDAQ_on;
            NormalizeFlimMode();
            use_pq = State.Init.FLIM_on && (String.Equals(State.Init.FLIM_mode, "PQ", StringComparison.OrdinalIgnoreCase)
                || String.Equals(State.Init.FLIM_mode, "MH", StringComparison.OrdinalIgnoreCase)
                || String.Equals(State.Init.FLIM_mode, "PH", StringComparison.OrdinalIgnoreCase)
                || String.Equals(State.Init.FLIM_mode, "HH", StringComparison.OrdinalIgnoreCase));
            use_bh = State.Init.FLIM_on && (String.Equals(State.Init.FLIM_mode, "BH"));
            use_pq = use_pq || String.Equals(State.Init.FLIM_mode, "SimPQ", StringComparison.OrdinalIgnoreCase);

            tcspc_on = use_pq || use_bh;

            if (State.Init.MicroscopeSystem.ToLower().Contains("bscope") && State.Init.MicroscopeSystem.ToLower().Contains("gg"))
                microscope_system = MicroscopeSystem.ThorLabBScopeGG;

            if (State.Init.MicroscopeSystem.ToLower().Contains("bscope") && State.Init.MicroscopeSystem.ToLower().Contains("rg"))
                microscope_system = MicroscopeSystem.ThorLabBScopeRG;

            if (State.Init.MicroscopeSystem.ToLower().Contains("sutter"))
                microscope_system = MicroscopeSystem.SutterGG;

            if (State.Init.MicroscopeSystem.ToLower().Contains("mini"))
                microscope_system = MicroscopeSystem.MiniScope;

            if (State.Init.MicroscopeSystem.ToLower().Contains("fiber"))
                microscope_system = MicroscopeSystem.FiberPhotometry;

            if (microscope_system == MicroscopeSystem.FiberPhotometry)
            {
                State.Init.enableResonantScanner = false;
                State.Acq.resonantScanning = false;
                State.Acq.fillFraction = 1.0;
            }

            SafetyFeature();

            if (State.Init.enableResonantScanner)
            {
                if (State.Init.resonantScannerSystem.ToLower().Contains("thorecu"))
                {
                    string board = State.Init.ResonantAOBoard;
                    string port = board + "/port0/line0:3";

                    bool old = false;
                    if (State.Init.resonantScannerSystem.ToLower().Contains("_old"))
                        old = true;

                    thorECU = new ThorECU(State.Init.resonantScanner_COMPort, port, old);

                    if (thorECU.old)
                        maxValue_Resonant = 255;

                    if (thorECU.OpenPort() == 1)
                    {
                        thorECU_on = true;
                        //thorECU.SetZoomOfScanner(255);
                        //thorECU.EnableScanner(true);
                    }
                }
                else
                {
                    //Setting up Resonant scanner for analog. 
                }
            }

            if (use_nidaq)
            {
                var Setting_Status = "Activating DLL";

                try
                {
                    var access = new MicroscopeHardwareLibs.NiDaq.AccessKey(State.Init.FLIMserial);

                    if (MicroscopeHardwareLibs.NiDaq.DLLactive)
                    {
                        Setting_Status = "Activating line clock by counter";
                        if (!State.Init.use_digitalLineClock)
                            lineClock = new HardwareControls.IOControls.LineClockByCounter();

                        Setting_Status = "Activating miniScope clock";
                        miniScopeClock = new HardwareControls.IOControls.MiniScopeClock();

                        Setting_Status = "Activating digital clock";
                        digitalOutput_WClock = new HardwareControls.IOControls.DigitalOutputControl(State);

                        Setting_Status = "Activating dio trigger";
                        dioTrigger = new HardwareControls.IOControls.dioTrigger(State);

                        Setting_Status = "Activating shutter control";
                        shutterCtrl = new HardwareControls.IOControls.ShutterCtrl(State);

                        Setting_Status = "Initializing shading";
                        shading = new HardwareControls.IOControls.Shading(State);

                        if (State.Init.enableRegularGalvo)
                        {
                            Setting_Status = "Initializing mirror AO and EOM AO";
                            AO_Mirror_EOM = new HardwareControls.IOControls.AnalogOutput(State, shading, true, true, false, false);
                            AO_Mirror_EOM.FrameDone += new HardwareControls.IOControls.AnalogOutput.FrameDoneHandler(mirrorAOFrameDoneEvent);

                        }

                        if (State.Init.enableResonantScanner)
                        {
                            Setting_Status = "Initializing resonant mirror AO and EOM AO";
                            //lineTriggeredSampleClock = new HardwareControls.IOControls.TriggeredLineClock(State);
                            Resonant_Mirror = new HardwareControls.IOControls.AnalogOutput(State, shading, true, false, true, false);
                            Resonant_Mirror.FrameDone += new HardwareControls.IOControls.AnalogOutput.FrameDoneHandler(mirrorAOFrameDoneEvent);
                            if (State.Init.EOM_nChannels > 0)
                                Resonant_EOM = new HardwareControls.IOControls.AnalogOutput(State, shading, false, true, true, false);

                            resonant_on = new HardwareControls.IOControls.ResonantOn(State);
                            resonant_switch = new HardwareControls.IOControls.ResonantScannerSwitch(State);
                        }

                        if (State.Init.DO_uncagingShutter)
                        {
                            Setting_Status = "Initializing DO uncaging shutter";
                            DigitalUncagingShutterPort = State.Init.MirrorAOBoard + "/port0/" + State.Init.DigitalShutterPort;
                            new HardwareControls.IOControls.Digital_Out(DigitalUncagingShutterPort, false);
                        }


                        if (State.Init.AO_uncagingShutter)
                        {
                            Setting_Status = "Initializing AO uncaging shutter";
                            new HardwareControls.IOControls.AO_Write(State.Init.UncagingShutterAnalogPort, 0);
                        }

                        if (State.Init.usePiezo)
                        {
                            Setting_Status = "Initializing piezo";
                            piezo = new HardwareControls.IOControls.PiezoControl(State);
                        }
                    }
                    else
                    {
                        Setting_Status = "NIDAQ DLL not loaded";
                        use_nidaq = false;
                    }
                }
                catch (Exception ex)
                {
                    DialogResult dr = MessageBox.Show(Form.ActiveForm, "Problem in loading NIDAQmx DLL: " + Setting_Status + ": " + ex.Message + "\n\nDo you want to turn off NIDAQ function?",
                  "NIDAQ error", MessageBoxButtons.YesNo);
                    switch (dr)
                    {
                        case DialogResult.Yes:
                            State.Init.NIDAQ_on = false;
                            break;
                        case DialogResult.No:
                            State.Init.NIDAQ_on = true;
                            break;
                    }

                    fileIO.SaveDeviceFile();
                    use_nidaq = false;
                }
            }

            parameters = new FLIM_Parameters();
            parameters = FLIM_Utilities.ConvertStateToFLIMParameters(State, read_photon_file, parameters);


            tcspc_on = true;
            TCSPC_Open();

            if (!(use_bh || use_pq))
            {
                State.Init.FLIM_on = false;
            }
        }

        private static bool ShouldHoldFastWriterOpen(ScanParameters state)
        {
            if (state?.Files?.fastSaving ?? false)
                return true;
            if (state?.Acq?.fiberPhotometryMode ?? false)
                return true;

            var system = state?.Init?.MicroscopeSystem;
            return !string.IsNullOrEmpty(system)
                && system.IndexOf("fiber", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public void SafetyFeature()
        {
            if (microscope_system == MicroscopeSystem.ThorLabBScopeGG)
            {
                if (State.Acq.scanFraction > 0.86)
                    State.Acq.scanFraction = 0.86;
                if (State.Acq.fillFraction < 0.7)
                    State.Acq.scanFraction = 0.7;
                if (State.Acq.XMaxVoltage > 5)
                    State.Acq.XMaxVoltage = 5;
                if (State.Acq.YMaxVoltage > 5)
                    State.Acq.YMaxVoltage = 5;

                double minFlyBackTimeY = 0.4; // ms.
                if (State.Acq.msPerLine * (1 - State.Acq.scanFraction) < minFlyBackTimeY)
                    State.Acq.scanFraction = 1.0 - (minFlyBackTimeY / State.Acq.msPerLine);
            }

            if (State.Init.AbsoluteMaxVoltageScan > 10)
                State.Init.AbsoluteMaxVoltageScan = 10;
        }

        private void NormalizeFlimMode()
        {
            if (State?.Init == null || string.IsNullOrWhiteSpace(State.Init.FLIM_mode))
                return;

            var trimmed = State.Init.FLIM_mode.Trim();
            if (string.Equals(trimmed, "SymPQ", StringComparison.OrdinalIgnoreCase))
                trimmed = "SimPQ";

            State.Init.FLIM_mode = trimmed;
        }

        public void PostFLIMageShowInitialization(FLIMageMain flim_in)
        {
            flimage = flim_in;

            if (use_nidaq && State.Init.EOM_nChannels > 0)
            {
                CalibEOM(false); //should be in flim_io.
            }

            if (use_nidaq)
            {
                if (State.Init.UseExternalMirrorOffset)
                {
                    var aoX = new HardwareControls.IOControls.AO_Write(State.Init.mirrorOffsetX, State.Acq.XOffset);
                    var aoY = new HardwareControls.IOControls.AO_Write(State.Init.mirrorOffsetY, State.Acq.YOffset);
                }
            }

            InitializeCounter(); //Counter reset.
        }

        public void stopLoopingStatus()
        {
            stopGrabActivated = true;
            looping = false;
            snapShot = false;
        }


        public void initializeFocusing()
        {
            snapShot = false;
            stopGrabActivated = false;
            focusing = true;
            grabbing = false;
            read_photon_file = false;
        }

        public void initializeSnapShot()
        {
            snapShot = true;
            stopGrabActivated = false;
            read_photon_file = false;
        }

        public void initializePhotonReading()
        {
            read_photon_file = true;
            grabbing = true;
            focusing = false;
            snapShot = false;
        }

        public void initializeGrabbing()
        {
            snapShot = false;
            grabbing = true;
            looping = false;
            allowLoop = false;
            stopGrabActivated = false;
            read_photon_file = false;
        }

        public void InitializeLoop()
        {
            grabbing = true;
            looping = true;
            allowLoop = true; //This is the difference between loop and grab.
            stopGrabActivated = false;
            snapShot = false;
            read_photon_file = false;

            internalImageCounter = 0;
        }

        public void InitializeCounter()
        {
            averageCounter = 0;
            averageSliceCounter = 0;

            internalSliceCounter = 0;
            internalFrameCounter = 0;

            uncaging_DO_SliceCounter = 0;
            savePageCounterTotal = 0;
            savePageCounter = 0;
            savePageBufferCounter = 0;
            displayPageCounter = 0;
            displayPageCounterTotal = 0;

            deletedPageCounter = 0;

            AO_FrameCounter = 0;
            saved_realCounter = 0;

            TCSPC_Native.DLL_Busy = false;
            acquisition_done_event_activated = false;

            newFile = boolAllChannels(true);

            if (!focusing)
            {
                FLIMSaveBuffer.Clear();
                acquiredTimeList.Clear();
                real_acquired_datetime.Clear();
            }

            if (flimage != null)
            {
                flimage.InvokeIfRequired(o => o.flimage_io.StartPrepAllPanels());
            }
        }

        void StartPrepAllPanels()
        {
            if (flimage.uncaging_panel != null && !focusing)
                flimage.uncaging_panel.StartPrep(use_nidaq);

            if (flimage.digital_panel != null && !focusing)
                flimage.digital_panel.StartPrep();

            flimage.InitializeCounter_GUI_Update();
        }

        /// <summary>
        /// Making the event handler for RateTimer. 
        /// </summary>
        void SetupTimerRate()
        {
            RateTimer.Tick += new EventHandler(TimerEventProcessorRate); //Attach function
            RateTimer.Interval = 1000;
            RateTimer.Start();
        }

        public void updateState(ScanParameters State_in)
        {
            State = State_in;

            if (use_nidaq && AO_Mirror_EOM != null)
                AO_Mirror_EOM.putValue_S_ToStartPos(true, false);

            EventNotify?.Invoke(this, new ProcessEventArgs("ParametersChanged", null));
        }

        public void Notify(ProcessEventArgs e)
        {
            EventNotify?.Invoke(this, e);
        }

        public void NotifyEventExternal(String eventName)
        {
            EventNotify?.Invoke(this, new ProcessEventArgs(eventName, null));
        }

        /// <summary>
        /// Called by NIDAQ mirror AO using the FrameDone event listener, when a frame is done. 
        /// See IOcontrolls.cs for the listener setup.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void mirrorAOFrameDoneEvent(object o, EventArgs e)
        {
            double msPerLine = State.Acq.fastZScan ? State.Acq.FastZ_msPerLine : State.Acq.msPerLine;
            if (State.Acq.resonantScanning)
                msPerLine = 500.0 / State.Init.resonantFreq_Hz;

            DateTime at = DateTime.Now;
            if (AO_FrameCounter == 0)
            {
                if (State.Acq.BiDirectionalScanY)
                    acquiredTime = at.AddMilliseconds(-msPerLine * State.Acq.linesPerFrame * 2);
                else
                    acquiredTime = at.AddMilliseconds(-msPerLine * State.Acq.linesPerFrame);

                State.Acq.triggerTime = acquiredTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");
#if DEBUG
                Debug.WriteLine("Time now at " + at.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                Debug.WriteLine("Triggered triggered at " + State.Acq.triggerTime);
#endif
                flimage.image_display.FLIM_ImgData.State.Acq.triggerTime = State.Acq.triggerTime;

                real_acquired_datetime.Add(acquiredTime);
                if (State.Acq.BiDirectionalScanY)
                {
                    var acquiredTime1 = at.AddMilliseconds(-msPerLine * State.Acq.linesPerFrame);
                    real_acquired_datetime.Add(acquiredTime);
                }
            }


            if (State.Acq.BiDirectionalScanY)
            {
                var at2 = at.AddMilliseconds(-msPerLine * State.Acq.linesPerFrame);
                real_acquired_datetime.Add(at2);
                real_acquired_datetime.Add(at);
                AO_FrameCounter += 2;
            }
            else
            {
                real_acquired_datetime.Add(at);
                AO_FrameCounter++;
            }

            TCSPC_Native.DLL_Busy = AO_FrameCounter - internalFrameCounter > 100;


            if (AO_FrameCounter <= State.Acq.nFrames + State.Spc.spcData.SkipFirstFrames || focusing)
            {
                flimage.BeginInvoke((Action)delegate
                {
                    flimage.AO_FrameUpdate(AO_FrameCounter);
                });
            }

            var every = State.Init.DigitalMarkEveryFrame;
            if (AO_FrameCounter % every <= 2 || AO_FrameCounter % every >= every - 2)
                if (State.Init.DigitalMark_On)
                {
                    var task = Task.Factory.StartNew((Action)delegate
                        {
                            if (AO_FrameCounter % (int)(every) == 0)
                                new HardwareControls.IOControls.Digital_Out(State.Init.DigitalMarkOutputPort, true);
                            else
                                new HardwareControls.IOControls.Digital_Out(State.Init.DigitalMarkOutputPort, false);

                        });
                }

            bool save_count = AO_FrameCounter % (int)(every) == 0 || AO_FrameCounter == State.Acq.nFrames + State.Spc.spcData.SkipFirstFrames;

            //if (State.Acq.photon_file_format && save_count && !read_photon_file && !focusing)
            //{
            //    string all_text = "";
            //    for (int i = saved_realCounter; i < real_acquired_datetime.Count; i++)
            //    {
            //        var text1 = String.Format("{0}, {1}\n", i, real_acquired_datetime[i].ToString("yyyy-MM-ddTHH:mm:ss.fff"));
            //        all_text += text1;
            //    }
            //    File.AppendAllText(filename_timestamp, all_text);
            //    saved_realCounter = real_acquired_datetime.Count;
            //}
        }

        /// <summary>
        /// Start new measurment --- designed for TagLens parameter measurement. 
        /// </summary>
        /// <param name="eraseAllMemory"></param>
        /// <param name="focusing"></param>
        /// <param name="measure_tagParameters"></param>
        public void FiFo_StartNew(bool eraseAllMemory, bool focusing, bool measure_tagParameters)
        {
            if (tcspc_on && FiFo_acquire != null)
            {
                bool[] erase = boolAllChannels(eraseAllMemory);
                FiFo_acquire.StartMeas(erase, focusing, measure_tagParameters);
            }
        }

        public void FiFo_StartNew(bool[] erase, bool focusing, bool measure_tagParameters)
        {
            if (tcspc_on && FiFo_acquire != null)
            {
                FiFo_acquire.StartMeas(erase, focusing, measure_tagParameters);
            }
        }

        public void FiFo_StopMeas(bool force)
        {
            if (tcspc_on && FiFo_acquire != null)
                FiFo_acquire.StopMeas(force);
        }

        public void PiezoMoveDuringFocus(double stepSize)
        {
            Task.Factory.StartNew(() =>
            {
                if (focusing)
                {
                    refocusing = true;
                    StopFocus();
                }

                System.Threading.Thread.Sleep((int)State.Init.PiezoSettlingTime_ms);

                piezo.move_Piezo_1step_um(stepSize);

                System.Threading.Thread.Sleep((int)State.Init.PiezoSettlingTime_ms);

                if (refocusing)
                {
                    StartGrab(true);
                    refocusing = false;
                }
            });
        }

        public void ResetFocus()
        {
            if (focusing && !refocusing)
            {
                Task.Factory.StartNew(() =>
                {
                    refocusing = true;
                    PauseFocus();
                    DisposeDAQ();
                    ReStartFocus();
                    refocusing = false;
                });
            }
        }

        /// <summary>
        /// Time Function to update Sync Rate and photon counting rate.
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="myEventArgs"></param>
        public void TimerEventProcessorRate(Object myObject, EventArgs myEventArgs)
        {
            bool badRate = (badSyncRateCounter > badSyncRateMaxCount);

            if (flimage.use_piezo)
            {
                flimage.InvokeIfRequired(o => o.UpdatePiezoPositionGUI());
            }

            sleep_counter++;

            if ((grabbing && !read_photon_file) || focusing)
                sleep_counter = 0;

            if (sleep_counter >= State.Acq.ResonantScanTurnOffAfterXSeconds)
            {
                if (State.Acq.resonantScanning)
                {
                    if (thorECU_on)
                    {
                        thorECU.EnableScanner(false);
                    }

                    if (Resonant_Mirror != null)
                        Resonant_Mirror.setResonantZoom(1000);
                }

                sleep_counter = 0;
            }

            if (tcspc_on) //!runningImgAcq)
            {
                int ret = -1;

                ret = FiFo_acquire.GetRate();

                if (ret == 0)
                {
                    //State.Spc.datainfo = PQ_acquire.State.Spc.datainfo;
                    State.Spc.datainfo.syncRate = (int[])parameters.rateInfo.syncRate.Clone();
                    State.Spc.datainfo.countRate = (int[])parameters.rateInfo.countRate.Clone();

                    bool badRate1 = false;
                    for (int i = 0; i < State.Spc.spcData.nDevices; i++)
                    {
                        double syncRate1 = State.Spc.datainfo.syncRate[i] / 1e6;

                        if (syncRate1 > State.Acq.ExpectedLaserPulseRate_MHz * 0.9
                            && syncRate1 < State.Acq.ExpectedLaserPulseRate_MHz * 1.1)
                        {
                            badRate1 = false;
                        }
                        else
                        {
                            badRate1 = true;
                            break;
                        }
                    }

                    if (badRate1)
                        badSyncRateCounter++;
                    else
                        badSyncRateCounter = 0;
                }
            }

            if (flimage != null)
            {
                flimage.InvokeIfRequired(o => o.RateTimerEvent_GUI_Update(badRate));
            }
        }



        public void CalibEOM(bool plot)
        {
            if (use_nidaq && State.Init.EOM_nChannels > 0)
            {
                if (State.Init.openShutterDuringCalibration)
                    shutterCtrl.open();

                bool[] success = shading.calibration.calibrateEOM(plot);
                shading.applyCalibration(State);

                shutterCtrl.Close();

                flimage.CalibEOM_GUI_Update(success);

            } //nidaq
        }

        public void StopRateTimer()
        {
            if (tcspc_on)
            {
                RateTimer.Stop();
                RateTimer.Dispose();
                System.Threading.Thread.Sleep(50);
            }
        }

        public void ECU_Close()
        {
            if (thorECU != null)
            {
                thorECU.EnableScanner(false);
                thorECU.ClosePort();
            }
            thorECU_on = false;
        }

        public void TCSPC_SetupParameters()
        {
            if ((tcspc_on || read_photon_file) & FiFo_acquire != null)
                FiFo_acquire.SetupParameters(focusing, parameters);
        }

        public void TCSPC_Open()
        {
            FiFio_multiBoards.ErrorCode error;
            if (!State.Init.FLIM_on)
            {
                FiFo_acquire = new FiFio_multiBoards(parameters);
                //FiFo_acquire.startSimulationMode();
                error = FiFo_acquire.Initialize();
                //error = FiFio_multiBoards.ErrorCode.NONE;
                //simulation_mode = true;
            }
            else
            {
                parameters.ComputerID = State.Init.ComputerID;
                parameters.FLIMserial = State.Init.FLIMserial;
                parameters.spcData = State.Spc.spcData; //linked together.
                parameters.spcData.BoardType = State.Init.FLIM_mode;

                FiFo_acquire = new FiFio_multiBoards(parameters);
                State.Init.ComputerID = parameters.ComputerID;


                error = FiFo_acquire.Initialize();
                //bool serialError = (error == FiFio_multiBoards.ErrorCode.COMPUTERID_INCORRECT);

                State.Init.ComputerID = parameters.ComputerID;

                simulation_mode = FiFo_acquire != null && FiFo_acquire.simulation_mode;

#if DEBUG
                try
                {
                    bool dllActive0 = FiFo_acquire?.FLIM_FiFoList != null && FiFo_acquire.FLIM_FiFoList.Count > 0 && FiFo_acquire.FLIM_FiFoList[0] != null
                        ? FiFo_acquire.FLIM_FiFoList[0].DLLActive
                        : false;
                    Debug.WriteLine($"TCSPC_Open: BoardType={parameters?.spcData?.BoardType}, Initialize={error}, MultiBoards.simulation_mode={FiFo_acquire?.simulation_mode}, DLLActive0={dllActive0}");
                }
                catch { }
#endif
            }

            if (error == FiFio_multiBoards.ErrorCode.COMPUTERID_INCORRECT)
            {
                string query = String.Format("FLIM DLL serial number (Computer ID = {0}) : ", parameters.ComputerID);
                string default1 = "0";
                string input = Interaction.InputBox(query, "FLIM serial input", default1);

                if (int.TryParse(input, out int id))
                {
                    fileIO.State.Init.FLIMserial = id;
                    fileIO.SaveDeviceFile();
                    FiFo_acquire.closeDevice();

                    Application.Restart();
                    Environment.Exit(0);
                }
            }

            SetupFLIMParameters(State);

            if (error == FiFio_multiBoards.ErrorCode.NONE && FiFo_acquire != null)
            {
                SetupTimerRate();
                FiFo_acquire.FrameDone += new FiFio_multiBoards.FrameDoneHandler(FrameDoneEvent);
                FiFo_acquire.StripeDone += new FiFio_multiBoards.StripeDoneHandler(StripeDoneEvent);
                FiFo_acquire.MeasDone += new FiFio_multiBoards.MeasDoneHandler(MeasDoneEvent);
                //FiFo_acquire.SetupParameters(focusing, parameters);

                flimage.InvokeIfRequired(o => o.UpdateSPC_GUI(this));
            }

            if (error != FiFio_multiBoards.ErrorCode.NONE)
            {
                var error_Message = "FLIM serial number is incorrect.";
                if (error == FiFio_multiBoards.ErrorCode.BOARD_INITIALIZE_FAILURE)
                    error_Message = "FLIM board initialization error.";
                else if (error == FiFio_multiBoards.ErrorCode.BOARD_OPEN_FAILURE)
                    error_Message = "FLIM board open error.";
                else if (error == FiFio_multiBoards.ErrorCode.DLL_NOTFOUND)
                    error_Message = "FLIM DLL not found";
                else if (error == FiFio_multiBoards.ErrorCode.PARAMETER_ERROR)
                    error_Message = "Wrong FLIM parameters";


                DialogResult dr = MessageBox.Show(Form.ActiveForm, error_Message + "\n\nDo you want to turn off FLIM function?",
                 "FLIM error", MessageBoxButtons.YesNo);
                switch (dr)
                {
                    case DialogResult.Yes:
                        State.Init.FLIM_on = false;
                        break;
                    case DialogResult.No:
                        State.Init.FLIM_on = true;
                        break;
                }
                fileIO.SaveDeviceFile();
            }
        }

        public void TCSPC_Close()
        {
            if (tcspc_on)
            {
                StopRateTimer();

                if (FiFo_acquire != null && FiFo_acquire.nDevices > 1)
                    FiFo_acquire.closeDevice();

                tcspc_on = false;
            }
        }

        void waitForAcquisitionTaskCompleted()
        {
            //There is no "wait" function in FiFO_acquire.
            if (tcspc_on && FiFo_acquire != null)
            {
                System.Threading.Thread.Sleep(5);
                for (int i = 0; i < 100; i++) // (!FiFo_acquire.IsCompleted())
                {
                    System.Threading.Thread.Sleep(5);
                    if (FiFo_acquire.IsCompleted())
                        break;
                }
            }
        }

        /// <summary>
        /// return success
        /// </summary>
        /// <param name="eraseSPCmemory"></param>
        /// <param name="recordTriggerTime"></param>
        public bool StartDAQ(bool[] eraseSPCmemory, bool recordTriggerTime)
        {
            runningImgAcq = true;
            waitForAcquisitionTaskCompleted();

            if (stopGrabActivated)
                return false;

            if (grabbing && State.Acq.photon_file_format)
            {
                var setup_success = SetupPhotonFileNameForSlices(internalSliceCounter);

                if (read_photon_file)
                {
                    acquiredTime = flimage.image_display.photon_file_handle.binary_trigger_time;
                }

                if (!setup_success)
                    return false;
            }

            if (use_nidaq && !read_photon_file)
            {
                bool use_clock = State.Init.use_digitalLineClock;
                bool use_uncaging = State.Uncaging.uncage_whileImage && grabbing && State.Uncaging.sync_withFrame;
                bool use_digital = State.DO.DO_whileImage && grabbing && State.DO.sync_withFrame;


                if (State.Acq.resonantScanning)
                {
                    if (thorECU_on)
                    {
                        thorECU.SetZoomOfScanner((int)(maxValue_Resonant / State.Acq.zoom));
                        thorECU.EnableScanner(true);
                        Resonant_Mirror.setResonantZoom(State.Acq.zoom);

                        shutterCtrl.open(); //It is actually shutter??
                        System.Threading.Thread.Sleep(100);
                    }
                    else
                    {
                        shutterCtrl.open();
                        Resonant_Mirror.setResonantZoom(State.Acq.zoom);
                    }

                    if (Resonant_EOM != null)
                    {
                        Resonant_EOM.putValue_S_ToStartPos(false, false);

                        if (State.Acq.resonantEOM_blank_edge) //Currently not working well...
                        {
                            Resonant_EOM.putValue_S_ToStartPos(true, false);
                            Resonant_EOM.PutValueResonantEOM_LineClockTriggered(shading, State.Acq.resonantEOMDelay_us);
                        }
                    }

                    Resonant_Mirror.RestateScanParameters(State);
                    Resonant_Mirror.putValueResonantScan();
                    //Resonant_Mirror.Start();

                    //ParkMirrors(false); Already done.
                    //Resonant_Mirror.putValue_Single(new double[] { 0, 0 }, false, false);                    

                    if (!focusing && State.Uncaging.sync_withFrame && State.Uncaging.uncage_whileImage)
                    {

                    }
                }
                else //not resonant.
                {

                    if (!State.Init.use_digitalLineClock)
                    {
                        lineClock.Setup(State);
                    }

                    if (State.Acq.enableMiniScopeClock)
                        miniScopeClock.Setup(State);

                    if (use_clock || use_uncaging || use_digital)
                    {
                        digitalOutput_WClock.RestateScanParameters(State);
                        digitalOutput_WClock.PutValue(use_clock, use_uncaging,
                            use_digital, grabbing, focusing);
                    }

                    if (microscope_system != MicroscopeSystem.FiberPhotometry)
                    {
                        AO_Mirror_EOM.RestateScanParameters(State);

                        if (!focusing && State.Uncaging.sync_withFrame && State.Uncaging.uncage_whileImage && !State.Acq.resonantScanning)
                        {
                            var success = AO_Mirror_EOM.putValueScanAndUncaging();
                        }
                        else
                        {
                            bool uncaging_shutter = false;
                            if (flimage.uncaging_panel != null)
                            {
                                uncaging_shutter = flimage.uncaging_panel.UncagingShutter;
                            }

                            var data1 = AO_Mirror_EOM.putValueScan(focusing, uncaging_shutter);

                            if (State.Init.DO_uncagingShutter)
                            {
                                uncagingShutterCtrl(uncaging_shutter, false, true);
                            }

                        }

                        AO_Mirror_EOM.Start();

                        if (!State.Acq.resonantScanning)
                        {
                            if (use_clock || use_uncaging || use_digital)
                                digitalOutput_WClock.Start();

                            if (!State.Init.use_digitalLineClock)
                                lineClock.Start();

                            if (State.Acq.enableMiniScopeClock)
                                miniScopeClock.Start();
                        }
                    } //Not fiber
                }
            }

            FiFo_StartNew(eraseSPCmemory, focusing, false);
            //System.Threading.Thread.Sleep(100);

            if (use_nidaq && !read_photon_file)
            {
                shutterCtrl.open();
                System.Threading.Thread.Sleep(State.Init.mainShutterDelay); //shutter open.

                DateTime at = new DateTime();

                if (!State.Acq.externalTrigger || focusing) //Not allowing ext trigger yet.
                {
                    if (State.Acq.resonantScanning)
                    {
                        if (State.Acq.resonantEOM_blank_edge)
                            Resonant_EOM.Start();
                        Resonant_Mirror.Start();
                        System.Threading.Thread.Sleep(5);
                        resonant_on.open();
                        //lineTriggeredSampleClock.Start(); //Testing triggered clock.
                    }
                    else
                        dioTrigger.Evoke();

                    at = DateTime.Now;

                    if (physWaitforTrigger)
                    {
                        flimage.physiology.io_controls.triggerTime = at;
                        physWaitforTrigger = false;
                    }

                    if (uncagingWaitForTrigger)
                    {
                        flimage.uncaging_panel.waitTriggerUncagig = false;
                        uncagingWaitForTrigger = false;
                    }

                    if (eraseSPCmemory.Any(x => x == true))
                    {
                        acquiredTime = at;
                        Debug.WriteLine("FLIMage_IO Triggered..." + acquiredTime.ToString(State.Acq.datetime_formatter));
                    }

                    if (recordTriggerTime)
                    {
                        acquiredTime = at;
                        State.Acq.triggerTime = at.ToString(State.Acq.datetime_formatter);
                        flimage.image_display.FLIM_ImgData.State.Acq.triggerTime = State.Acq.triggerTime;
                    }
                }
                else
                {
                    acquiredTime = DateTime.Now;
                    State.Acq.triggerTime = acquiredTime.ToString(State.Acq.datetime_formatter);
                    flimage.image_display.FLIM_ImgData.State.Acq.triggerTime = State.Acq.triggerTime;
                }

            } //nidaq
            else
            {
                if (!read_photon_file)
                {
                    //Used for simulation mode
                    acquiredTime = DateTime.Now;
                    State.Acq.triggerTime = acquiredTime.ToString(State.Acq.datetime_formatter);
                    flimage.image_display.FLIM_ImgData.State.Acq.triggerTime = State.Acq.triggerTime;
                }
                else
                {
                    acquiredTime = flimage.image_display.photon_file_handle.binary_trigger_time;
                }
            }
            return true;
        }

        private void RestoreLineScanTraceStateIfNeeded()
        {
            // Always clear any custom mirror waveform unless we explicitly re-enable it for the next grab.
            if (AO_Mirror_EOM != null)
            {
                AO_Mirror_EOM.UseCustomMirrorOutputXY = false;
                AO_Mirror_EOM.CustomMirrorOutputXY = null;
            }

            if (_lineScanTraceBackup == null)
            {
                State.Acq.isLineScanAcquisition = false;
                _lineScanTraceActive = false;
                return;
            }

            try
            {
                State.Acq.nFrames = _lineScanTraceBackup.nFrames;
                State.Acq.linesPerFrame = _lineScanTraceBackup.linesPerFrame;
                State.Acq.linesPerStripe = _lineScanTraceBackup.linesPerStripe;
                State.Acq.nStripes = _lineScanTraceBackup.nStripes;
                State.Acq.SkipFirstLines = _lineScanTraceBackup.skipFirstLines;
                State.Acq.BiDirectionalScan = _lineScanTraceBackup.biDirectionalScan;
                State.Acq.BiDirectionalScanY = _lineScanTraceBackup.biDirectionalScanY;
                State.Acq.SineWaveScan = _lineScanTraceBackup.sineWaveScan;
                State.Acq.fillFraction = _lineScanTraceBackup.fillFraction;
                State.Acq.scanFraction = _lineScanTraceBackup.scanFraction;
                State.Acq.ScanDelay = _lineScanTraceBackup.scanDelay;
                State.Acq.LineScanTimePoints = _lineScanTraceBackup.lineScanTimePoints;
                State.Acq.isLineScanAcquisition = _lineScanTraceBackup.isLineScanAcquisition;
            }
            finally
            {
                _lineScanTraceBackup = null;
                _lineScanTraceActive = false;
            }
        }

        private bool TryEnableLineScanTraceFromPolygonROI(out string reason)
        {
            reason = "";
            bool isSimPq = string.Equals(State.Init.FLIM_mode, "SimPQ", StringComparison.OrdinalIgnoreCase);

            if (microscope_system == MicroscopeSystem.FiberPhotometry)
            {
                reason = "Line scan trace is not available in fiber photometry mode.";
                return false;
            }

            // Regular galvo-galvo only
            if (State.Acq.resonantScanning || State.Acq.polygonScanning)
            {
                reason = "Line scan trace works only with regular galvo-galvo scanning.\nDisable resonant/polygon scanning first.";
                return false;
            }

            bool canUseAo = State.Init.NIDAQ_on && State.Init.enableRegularGalvo && AO_Mirror_EOM != null;
            if (!isSimPq && !canUseAo)
            {
                reason = "Regular galvo scanning is not enabled (AO mirror output is not available).";
                return false;
            }

            if (!TryGetLineScanTraceVerticesPx(out var verticesPx, out reason))
                return false;

            // In line-scan mode we treat one trace traversal as one "line" (Y=time).
            // We keep the normal acquisition structure:
            // - nFrames = GUI nFrames
            // - linesPerFrame = GUI linesPerFrame (typically ~128)
            // Total time points = nFrames * linesPerFrame.
            int nFrames = Math.Max(1, State.Acq.nFrames);
            int linesPerFrame = Math.Max(1, State.Acq.linesPerFrame);
            int totalTimePoints = nFrames * linesPerFrame;

            double[,] traceXY = null;
            if (canUseAo)
            {
                if (!TryBuildPolygonTraceWaveform(verticesPx, State, linesPerFrame, out traceXY, out reason))
                    return false;
            }

            // Backup scan parameters we override for the grab.
            _lineScanTraceBackup = new LineScanTraceBackup
            {
                nFrames = State.Acq.nFrames,
                linesPerFrame = State.Acq.linesPerFrame,
                linesPerStripe = State.Acq.linesPerStripe,
                nStripes = State.Acq.nStripes,
                skipFirstLines = State.Acq.SkipFirstLines,
                biDirectionalScan = State.Acq.BiDirectionalScan,
                biDirectionalScanY = State.Acq.BiDirectionalScanY,
                sineWaveScan = State.Acq.SineWaveScan,
                fillFraction = State.Acq.fillFraction,
                scanFraction = State.Acq.scanFraction,
                scanDelay = State.Acq.ScanDelay,
                lineScanTimePoints = State.Acq.LineScanTimePoints,
                isLineScanAcquisition = State.Acq.isLineScanAcquisition,
            };

            // Record total timepoints for analysis/display logic.
            State.Acq.LineScanTimePoints = totalTimePoints;
            State.Acq.isLineScanAcquisition = true;

            // Keep standard saving/display behavior (no special chunking):
            // reduce stripe events by using one stripe per frame.
            State.Acq.linesPerStripe = linesPerFrame;
            State.Acq.nStripes = 1;
            State.Acq.SkipFirstLines = 0;

            // Disable raster-only options for this mode.
            State.Acq.BiDirectionalScan = false;
            State.Acq.BiDirectionalScanY = false;
            State.Acq.SineWaveScan = false;

            // Make TCSPC timing consistent with "full-line acquisition".
            State.Acq.fillFraction = 1.0;
            State.Acq.scanFraction = 1.0;
            State.Acq.ScanDelay = 0.0;

            // Supply the mirror waveform (already in volt domain).
            if (canUseAo)
            {
                AO_Mirror_EOM.CustomMirrorOutputXY = traceXY;
                AO_Mirror_EOM.UseCustomMirrorOutputXY = true;
            }

            _lineScanTraceActive = true;
            return true;
        }

        public bool TryGetLineScanTraceWaveform(int nLines, out double[,] traceXY, out string reason)
        {
            traceXY = null;
            reason = "";

            if (!TryGetLineScanTraceVerticesPx(out var verticesPx, out reason))
                return false;

            int lineCount = nLines > 0 ? nLines : 1;
            return TryBuildPolygonTraceWaveform(verticesPx, State, lineCount, out traceXY, out reason);
        }

        private bool TryGetLineScanTraceVerticesPx(out List<System.Drawing.PointF> verticesPx, out string reason)
        {
            verticesPx = null;
            reason = "No line scan trace is selected.\nRight-click a polygon/rectangular/ellipsoid ROI and choose \"Use this trace for line scanning\".";

            var xs = State?.Acq?.LineScanArrayX;
            var ys = State?.Acq?.LineScanArrayY;
            if (xs == null || ys == null)
                return false;

            if (xs.Length != ys.Length || xs.Length < 3)
            {
                reason = "Line scan trace vertices are invalid.\nRight-click a polygon/rectangular/ellipsoid ROI and choose \"Use this trace for line scanning\" again.";
                return false;
            }

            var list = new List<System.Drawing.PointF>(xs.Length);
            for (int i = 0; i < xs.Length; i++)
                list.Add(new System.Drawing.PointF((float)xs[i], (float)ys[i]));

            verticesPx = list;
            reason = "";
            return true;
        }

        private static bool TryBuildPolygonTraceWaveform(IReadOnlyList<System.Drawing.PointF> polygonVerticesPx, ScanParameters stateForMapping, int nLines, out double[,] traceXY, out string reason)
        {
            traceXY = null;
            reason = "";

            if (polygonVerticesPx == null || polygonVerticesPx.Count < 3)
            {
                reason = "Invalid line scan trace (need 3+ vertices).";
                return false;
            }

            // Convert ROI vertices (image pixels) -> mirror voltages.
            var verticesPx = new List<System.Drawing.PointF>(polygonVerticesPx.Count);
            verticesPx.AddRange(polygonVerticesPx);

            // Remove redundant closing point if present.
            if (verticesPx.Count > 1)
            {
                var p0 = verticesPx[0];
                var pLast = verticesPx[verticesPx.Count - 1];
                double dx0 = p0.X - pLast.X;
                double dy0 = p0.Y - pLast.Y;
                if (Math.Sqrt(dx0 * dx0 + dy0 * dy0) < 1e-3)
                    verticesPx.RemoveAt(verticesPx.Count - 1);
            }

            // Remove consecutive duplicates.
            for (int i = verticesPx.Count - 1; i >= 1; i--)
            {
                var a = verticesPx[i - 1];
                var b = verticesPx[i];
                double dx = a.X - b.X;
                double dy = a.Y - b.Y;
                if (Math.Sqrt(dx * dx + dy * dy) < 1e-3)
                    verticesPx.RemoveAt(i);
            }

            if (verticesPx.Count < 3)
            {
                reason = "Line scan trace has too few unique vertices.";
                return false;
            }

            int nSeg = verticesPx.Count; // closed polygon: segments = vertices

            double msPerLine = stateForMapping.Acq.fastZScan ? stateForMapping.Acq.FastZ_msPerLine : stateForMapping.Acq.msPerLine;
            int nSamplesLine = (int)(msPerLine * stateForMapping.Acq.outputRate / 1000.0);
            if (nSamplesLine < 2)
            {
                reason = "Line scan trace: msPerLine/outputRate results in too few samples.";
                return false;
            }

            if (nLines < 1)
                nLines = 1;

            // We generate nSamplesLine points, which correspond to (nSamplesLine - 1) steps.
            int totalSteps = nSamplesLine - 1;
            if (totalSteps < 3)
            {
                reason = "Line scan trace needs more samples per line.\nIncrease msPerLine or outputRate (need at least 4 samples).";
                return false;
            }

            if (totalSteps < nSeg)
            {
                int targetSeg = Math.Max(3, totalSteps);
                var reduced = new List<System.Drawing.PointF>(targetSeg);
                double step = (double)nSeg / targetSeg;
                for (int i = 0; i < targetSeg; i++)
                {
                    int segIndex = (int)Math.Floor(i * step);
                    if (segIndex < 0) segIndex = 0;
                    if (segIndex >= nSeg) segIndex = nSeg - 1;
                    reduced.Add(verticesPx[segIndex]);
                }
                verticesPx = reduced;
                nSeg = verticesPx.Count;
            }

            var vx = new double[nSeg];
            var vy = new double[nSeg];

            for (int i = 0; i < nSeg; i++)
            {
                var p = verticesPx[i];
                double[] frac = HardwareControls.IOControls.PixelsToFracOnScreen(new double[] { p.X, p.Y }, stateForMapping);

                double baseX = (frac[0] - 0.5) * stateForMapping.Acq.XMaxVoltage * stateForMapping.Acq.scanVoltageMultiplier[0] / stateForMapping.Acq.zoom;
                double baseY = (frac[1] - 0.5) * stateForMapping.Acq.YMaxVoltage * stateForMapping.Acq.scanVoltageMultiplier[1] / stateForMapping.Acq.zoom;

                double[,] vpos = new double[2, 1];
                vpos[0, 0] = baseX;
                vpos[1, 0] = baseY;

                int splitPos = 0;
                if (stateForMapping.Acq.nSplitScanning > 1)
                {
                    double splitHeight = 1.0 / stateForMapping.Acq.nSplitScanning;
                    splitPos = (int)Math.Floor(frac[1] * stateForMapping.Acq.scanVoltageMultiplier[1] / splitHeight);
                    if (splitPos < 0) splitPos = 0;
                    if (splitPos >= stateForMapping.Acq.nSplitScanning) splitPos = stateForMapping.Acq.nSplitScanning - 1;
                }

                HardwareControls.IOControls.RotateAndOffset(vpos, stateForMapping, splitPos);

                vx[i] = vpos[0, 0];
                vy[i] = vpos[1, 0];
            }

            // Segment lengths (in voltage space) to allocate time evenly by distance.
            var lenSeg = new double[nSeg];
            double totalLen = 0.0;
            for (int i = 0; i < nSeg; i++)
            {
                int j = (i + 1) % nSeg;
                double dx = vx[j] - vx[i];
                double dy = vy[j] - vy[i];
                double len = Math.Sqrt(dx * dx + dy * dy);
                lenSeg[i] = len;
                totalLen += len;
            }

            if (!(totalLen > 0))
            {
                reason = "Polygon ROI perimeter length is zero (all vertices are identical).";
                return false;
            }

            // Allocate steps per segment: at least 1 step per segment, remaining by length.
            var steps = Enumerable.Repeat(1, nSeg).ToArray();
            int remaining = totalSteps - nSeg;
            if (remaining > 0)
            {
                var add = new int[nSeg];
                var fracPart = new double[nSeg];
                int allocated = 0;
                for (int i = 0; i < nSeg; i++)
                {
                    double raw = (lenSeg[i] / totalLen) * remaining;
                    int a = (int)Math.Floor(raw);
                    add[i] = a;
                    fracPart[i] = raw - a;
                    allocated += a;
                }

                int left = remaining - allocated;
                while (left > 0)
                {
                    int best = 0;
                    double bestFrac = fracPart[0];
                    for (int i = 1; i < nSeg; i++)
                    {
                        if (fracPart[i] > bestFrac)
                        {
                            best = i;
                            bestFrac = fracPart[i];
                        }
                    }
                    add[best] += 1;
                    fracPart[best] = 0.0;
                    left--;
                }

                for (int i = 0; i < nSeg; i++)
                    steps[i] += add[i];
            }

            // Build one closed-trace line (periodic: ends at the starting vertex).
            var oneLineXY = new double[2, nSamplesLine];
            oneLineXY[0, 0] = vx[0];
            oneLineXY[1, 0] = vy[0];

            int idx = 1;
            for (int seg = 0; seg < nSeg; seg++)
            {
                int next = (seg + 1) % nSeg;
                int nStep = steps[seg];
                double x0 = vx[seg];
                double y0 = vy[seg];
                double x1 = vx[next];
                double y1 = vy[next];

                for (int s = 1; s <= nStep; s++)
                {
                    double t = (double)s / nStep;
                    oneLineXY[0, idx] = x0 + (x1 - x0) * t;
                    oneLineXY[1, idx] = y0 + (y1 - y0) * t;
                    idx++;
                }
            }

            if (idx != nSamplesLine)
            {
                // Should never happen; keep it safe.
                reason = "Internal error while building line scan trace waveform (sample count mismatch).";
                traceXY = null;
                return false;
            }

            // Stack lines into one frame (Y=time): repeat the same closed trace back-to-back.
            if (nLines == 1)
            {
                traceXY = oneLineXY;
                return true;
            }

            traceXY = new double[2, nSamplesLine * nLines];
            int totalSamples = nSamplesLine * nLines;
            int lineBytes = nSamplesLine * sizeof(double);
            int rowBytes = totalSamples * sizeof(double);
            int srcRow1Offset = nSamplesLine * sizeof(double);

            // Copy X and Y rows separately (rectangular arrays are row-major).
            for (int line = 0; line < nLines; line++)
            {
                int dstLineOffset = line * lineBytes;
                Buffer.BlockCopy(oneLineXY, 0, traceXY, dstLineOffset, lineBytes);
                Buffer.BlockCopy(oneLineXY, srcRow1Offset, traceXY, rowBytes + dstLineOffset, lineBytes);
            }

            return true;
        }


        /// <summary>
        /// Actual program for start grab.
        /// </summary>
        /// <param name="focus"></param>
        public void StartGrab(bool focus)
        {
            flimage.ImageDisplayOpen();

            if (focus && !read_photon_file && flimage?.State != null)
            {
                // Live focus should use the current UI state, not a previously loaded photon-file state.
                State = flimage.State;
            }

            if (!read_photon_file)
            {
                // Ensure any previous line-scan-trace override is cleared/restored before reading GUI values.
                RestoreLineScanTraceStateIfNeeded();

                flimage.GetParametersFromGUI(this); //Setup all parameters.

                if (!use_nidaq && State.Acq.externalTrigger)
                {
                    State.Acq.externalTrigger = false;
                    flimage?.BeginInvokeIfRequired(o =>
                    {
                        o.ExtTriggerCB.Checked = false;
                        o.WriteStatusText("External trigger disabled (no NI-DAQ available).");
                    });
                }

                // Line scan trace mode (grab only): trace ROI -> mirror voltage trace -> 1-line frame.
                // Focus always stays as normal 2D imaging so the user can see/edit the ROI.
                if (!focus && flimage?.LineScan_CB != null && flimage.LineScan_CB.Checked)
                {
                    if (!TryEnableLineScanTraceFromPolygonROI(out string reason))
                    {
                        // Per spec: auto-disable and warn if not selectable/compatible.
                        flimage.BeginInvokeIfRequired(o => o.LineScan_CB.Checked = false);
                        if (!string.IsNullOrWhiteSpace(reason))
                            flimage.BeginInvokeIfRequired(o => MessageBox.Show(o, reason));
                    }
                }

                if (State.Uncaging.uncage_whileImage && flimage.uncaging_panel != null && !focus)
                    flimage.uncaging_panel.SetupUncage(this); //Invoke not necessary

                if (State.Acq.XOffset > State.Acq.XMaxVoltage || State.Acq.YOffset > State.Acq.YMaxVoltage)
                {
                    if (microscope_system != MicroscopeSystem.FiberPhotometry)
                    {
                        flimage.BeginInvokeIfRequired(o => MessageBox.Show(o, "Offset exceeds maximum voltage!!"));
                        return;
                    }
                }

                FiFo_acquire.GetRate();

                flimage.flimage_io.SetupFLIMParameters(State);

                if (microscope_system != MicroscopeSystem.FiberPhotometry)
                {
                    if (flimage.fastZcontrol != null)
                    {
                        flimage.fastZcontrol.InvokeIfRequired(o =>
                            {
                                o.ControlsDuringScanning(true); //Just enable the control.... Should be blocking.
                            });
                    }
                }


                if (!focus)
                {
                    // Fiber photometry also saves using the normal .flim structure (1x1 pixels),
                    // so keep the standard saving parameter checks/prompt.
                    if (CheckSavingParameters() < 0)
                    {
                        runningImgAcq = false;
                        grabbing = false;
                        focusing = false;
                        looping = false;
                        post_grabbing_process = false;
                        if (flimage != null)
                        {
                            flimage.InvokeIfRequired(o => o.StopGrab_GUI_Update());
                            flimage.InvokeIfRequired(o => o.LoopButton.Enabled = true);
                        }
                        return;
                    }
                }
            }



            if (!read_photon_file)
            {
                flimage.image_display.FLIM_ImgData.InitializeData(State, true);
                fileIO = new FileIO(State);
                fileIO.HoldFastWriterOpen = ShouldHoldFastWriterOpen(State);

                //Apply physiology data.
                if (flimage.physiology == null || flimage.physiology.IsDisposed)
                    State.Ephys.Ephys_on = false;
                else
                    State = fileIO.CopyPhysiologyParamToState(flimage.physiology.phys_parameters);

                ParkMirrors(true);
            }

            flimage.image_display.InvokeIfRequired(o =>
            {
                if (!read_photon_file)
                {
                    o.SetupRealtimeImaging(o.flimage.flimage_io.State);
                    o.SetFastZModeDisplay(o.flimage.flimage_io.State.Acq.fastZScan);
                }
                else
                {
                    o.SetupRealtimeImaging(o.FLIM_ImgData.State);
                    o.SetFastZModeDisplay(o.FLIM_ImgData.State.Acq.fastZScan);
                }
            });

            if (flimage.image_display.plot_realtime.Visible)
                flimage.image_display.plot_realtime.InvokeIfRequired(o => o.WarningTextDisplay("")); //will be invoked if necessary.

            stopGrabActivated = false;
            if (focus)
            {
                focusing = true;
                grabbing = false;
                post_grabbing_process = false;

                EventNotify?.Invoke(this, new ProcessEventArgs("FocusStart", null));
            }
            else if (read_photon_file)
            {
                grabbing = true;
                post_grabbing_process = false;
                focusing = false;

            }
            else
            {
                grabbing = true;
                post_grabbing_process = false;
                focusing = false;
                EventNotify?.Invoke(this, new ProcessEventArgs("GrabStart", null));
            }

            runningImgAcq = true;

            if (!read_photon_file)
                flimage.InvokeIfRequired(o => o.StarGrab_GUI_Update(focus)); //need blocking.

            force_stop = false;

            InitializeCounter(); //Counters Reset. SaveBuffer Clear. Acquired time buffer clear.

            // Fiber photometry does not render images (single-point only)
            if (microscope_system != MicroscopeSystem.FiberPhotometry)
                flimage.image_display.InitializeStripeBuffer(State.Acq.nChannels, State.Acq.linesPerFrame, State.Acq.pixelsPerLine);

            if (!focusing) //Setup FLIM_ImgData mode. ZStack? FastZ?
            {
                flimage.image_display.FLIM_ImgData.clearMemory();
                flimage.image_display.FLIM_ImgData.ZStack = (State.Acq.ZStack && State.Acq.nSlices > 1 && State.Acq.sliceStep > 0.0);
                flimage.image_display.FLIM_ImgData.nFastZ = State.Acq.fastZScan ? State.Acq.FastZ_nSlices : 1;
            }

            System.Threading.Thread.Sleep(10);

            bool[] eraseMemoryA = boolAllChannels(true);

            if (!focus)
            {
                //UIstopWatch.Reset();
                if (!read_photon_file)
                {
                    if (internalImageCounter == 0)
                        UIstopWatch_Loop.Reset();

                    UIstopWatch_Image.Reset();

                    if (flimage.physiology != null && flimage.physiology.image_trigger_waiting)
                    {
                        flimage.physiology.StartAcq();
                        physWaitforTrigger = true;
                    }

                    if (flimage.uncaging_panel != null && flimage.uncaging_panel.waitTriggerUncagig)
                    {
                        flimage.uncaging_panel.StartUncagingDAQ();
                        flimage.uncaging_panel.waitTriggerUncagig = true;
                    }
                }

                var success = StartDAQ(eraseMemoryA, true); //Including trigger.
                if (!success && read_photon_file && State.Acq.photon_file_format)
                {
                    read_file_done_signal();
                    return;
                }

                if (internalImageCounter == 0)
                {
                    UIstopWatch_Loop.Start();
                }
                UIstopWatch_Image.Start();
            }
            else
            {
                var success = StartDAQ(eraseMemoryA, false); //Including trigger.
            }

            SW_PerformanceMonitor.Start();
        } //Start grab core.

        public void ReStartFocus()
        {
            runningImgAcq = true;
            focusing = true;
            force_stop = false;

            InitializeCounter(); //Counters Reset.
            bool[] eraseMemoryA = boolAllChannels(true);
            var success = StartDAQ(eraseMemoryA, false); //Including trigger.
        }

        public void StopNIDAQIOControls()
        {
            if (State.Acq.resonantScanning)
            {
                if (resonant_switch != null)
                    resonant_switch.On();

                if (Resonant_Mirror != null)
                    Resonant_Mirror.setResonantZoom(1000);
            }

            if (tcspc_on && runningImgAcq)
            {
                runningImgAcq = false;
                if (grabbing || focusing)
                    StopGrab(true, true);
                System.Threading.Thread.Sleep(50);
            }

            if (digitalOutput_WClock != null)
                digitalOutput_WClock.Dispose();

            if (lineClock != null)
                lineClock.Stop();

            if (miniScopeClock != null)
                miniScopeClock.Stop();

            if (AO_Mirror_EOM != null)
                AO_Mirror_EOM.Dispose(); //parking mirror

            if (piezo != null)
            {
                piezo.move_to_zero();
            }
        }

        /// <summary>
        /// Stop NIDAQ boards used for grabbing.
        /// Dupe with Pause????
        /// </summary>
        public void StopDAQ(bool temporal, bool shutter_close)
        {
            if (use_nidaq)
            {
                if (shutter_close)
                    shutterCtrl.Close();

                if (State.Acq.resonantScanning)
                {
                    //if (thorECU != null)
                    //    thorECU.EnableScanner(false);

                    if (lineTriggeredSampleClock != null)
                        lineTriggeredSampleClock.Stop();

                    if (Resonant_EOM != null)
                        Resonant_EOM.Stop();

                    if (resonant_on != null)
                        resonant_on.Close();

                    if (Resonant_Mirror != null)
                    {
                        Resonant_Mirror.Stop();
                        Resonant_Mirror.putValue_Single(new double[] { 0, 0 }, false, false);
                    }

                    if (Resonant_EOM != null)
                    {
                        Resonant_EOM.Stop();
                        Resonant_EOM.putValue_S_ToStartPos(true, false);
                    }

                    if (AO_Mirror_EOM != null)
                    {
                        AO_Mirror_EOM.Stop(); //Resonant EOM.
                        AO_Mirror_EOM.putValue_S_ToStartPos(true, false);
                    }

                }
                else
                {
                    if (!State.Init.use_digitalLineClock && lineClock != null)
                        lineClock.Stop();

                    if (miniScopeClock != null)
                        miniScopeClock.Stop();

                    if (digitalOutput_WClock != null)
                        digitalOutput_WClock.Stop();

                    if (AO_Mirror_EOM != null)
                        AO_Mirror_EOM.Stop();
                }
            }
            runningImgAcq = false;
        }

        public void PauseFocus()
        {
            FiFo_StopMeas(true);
            StopDAQ(true, false);
        }

        public void StopFocus()
        {
            StopGrab(false, true);
        }

        public void StopGrab(bool force)
        {
            StopGrab(force, false);
        }

        public void StopGrab(bool force, bool focusStop)
        {
            force_stop = force;

            stopGrabActivated = force;

            FiFo_StopMeas(force);

            StopDAQ(false, true);
            DisposeDAQ();

            // Restore line-scan-trace overrides (if any) before parking mirrors,
            // so the park position matches the user's normal 2D scan settings.
            RestoreLineScanTraceStateIfNeeded();

            if (!read_photon_file)
                ParkMirrors(true);

            runningImgAcq = false;

            if (!looping || stopGrabActivated)
            {
                UIstopWatch_Loop.Stop();
                UIstopWatch_Image.Stop();
            }

            SW_PerformanceMonitor.Stop();
            fileIO?.CloseAllFlimWriters();
            if (!read_photon_file && (State?.Acq?.photon_file_format ?? false))
            {
                try
                {
                    bool canClosePhotonStream = FiFo_acquire == null || FiFo_acquire.IsCompleted();
                    if (photon_file_handle != null && canClosePhotonStream)
                    {
                        if (photon_file_handle.IsStreamingZipWrite)
                        {
                            photon_file_handle.EndPhotonBinaryStreamWrite();
                            FiFo_acquire?.ClearPhotonWriteStream();
                        }
                        photon_file_handle.FinishWriting();
                    }
                    else if (photon_file_handle != null)
                    {
                        Debug.WriteLine("Photon file close deferred: acquisition still running.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Photon file close failed: " + ex.Message);
                }
            }

            if (focusStop)
            {
                flimage.InvokeIfRequired(o => o.StopFocus_GUI_Update()); //Should be done before (focusing = false).
                focusing = false;
            }
            else
            {
                grabbing = false;

                if (looping)
                {
                    flimage.GrabButton.InvokeIfRequired(o => o.Text = "STOP");
                    flimage.GrabButton.InvokeIfRequired(o => o.Enabled = false);
                }

                // If acquisition is aborted while a save task is still running, do NOT re-enable Grab immediately.
                // Otherwise the user can start a new grab with the same filename while the previous save still
                // holds the file, making "overwrite" fail.
                var pendingSaveTask = (saveTask != null && !saveTask.IsCompleted);
                if (pendingSaveTask)
                {
                    // Keep UI responsive and avoid deadlock: SaveTask uses InvokeAnyway (blocking Invoke),
                    // so we must not Wait() on the UI thread.
                    flimage.InvokeIfRequired(o =>
                    {
                        // Re-enable other controls, but keep Grab disabled until saving finishes.
                        o.ChangeItemsStatus(true, false);
                        o.GrabButton.Text = "SAVING";
                        o.GrabButton.Enabled = false;
                    });

                    Task.Factory.StartNew(() =>
                    {
                        try { saveTask.Wait(); } catch { /* ignore */ }

                        // Now the file handle should be released; restore normal UI state.
                        flimage.BeginInvokeIfRequired(o =>
                        {
                            o.StopGrab_GUI_Update();
                            o.SetEnableStatesOfControls();
                        });
                    });

                    return;
                }

                flimage.InvokeIfRequired(o => o.StopGrab_GUI_Update()); //Should be done before (focusing = false).
            }
            flimage.InvokeIfRequired(o => o.SetEnableStatesOfControls());
        }

        /// <summary>
        /// When stopping, mirrors need to be parked.
        /// </summary>
        public void ParkMirrors(bool zeroEOM)
        {
            bool zeroEOM1 = State.Acq.flyBackBlancking && zeroEOM;

            if (use_nidaq && !read_photon_file)
            {
                bool shutterAO = flimage.uncaging_panel != null && State.Init.AO_uncagingShutter && flimage.uncaging_panel.UncagingShutter;
                if (State.Init.enableRegularGalvo)
                    AO_Mirror_EOM.putValue_S_ToStartPos(zeroEOM1, shutterAO);

                if (State.Init.enableResonantScanner)
                {
                    zeroEOM1 = State.Acq.resonantEOM_blank_edge && zeroEOM;
                    if (Resonant_EOM != null)
                        Resonant_EOM.putValue_S_ToStartPos(zeroEOM1, false);

                    if (Resonant_Mirror != null)
                    {
                        Resonant_Mirror.putValue_S_ToStartPos(zeroEOM1, shutterAO);
                    }
                }

                if (flimage.uncaging_panel != null && State.Init.DO_uncagingShutter)
                    uncagingShutterCtrl(flimage.uncaging_panel.UncagingShutter, true, true);
                System.Threading.Thread.Sleep(5);
            }
        }

        /// <summary>
        /// NI-DAQ boards for grabbing are disposed. When stopping grab/focus.
        /// </summary>
        public void DisposeDAQ()
        {
            if (use_nidaq)
            {
                if (lineClock != null)
                    lineClock.Dispose();

                if (miniScopeClock != null)
                    miniScopeClock.Dispose();

                digitalOutput_WClock.Dispose();

                if (AO_Mirror_EOM != null)
                    AO_Mirror_EOM.Dispose();
            }
        }

        public void uncagingShutterCtrl(bool ON, bool controlAO, bool controlDO)
        {
            if (State.Init.DO_uncagingShutter && controlDO)
            {
                new HardwareControls.IOControls.Digital_Out(DigitalUncagingShutterPort, ON);
            }

            if (State.Init.AO_uncagingShutter && controlAO)
            {
                if (ON)
                    new HardwareControls.IOControls.AO_Write(State.Init.UncagingShutterAnalogPort, 5);
                else
                    new HardwareControls.IOControls.AO_Write(State.Init.UncagingShutterAnalogPort, 0);
            }
        }

        void Uncaging_DIO_Wait_Function()
        {

            int standard_waitTime = 40;  //With this cycle, we can see if stopGrab is activated. 

            bool uncaging_slice = State.Uncaging.sync_withSlice && State.Uncaging.uncage_whileImage;
            bool DO_slice = State.DO.sync_withSlice && State.DO.DO_whileImage;

            double overHead_ms = 40; //each uncaging or DO output requires some overhead time.

            if (uncaging_slice || DO_slice) //Uncaging protocol!!
            {
                bool fire_uncaging_cond = uncaging_DO_SliceCounter >= State.Uncaging.SlicesBeforeUncage && uncaging_slice;
                bool fire_DO_cond = uncaging_DO_SliceCounter >= State.DO.SlicesBeforeDO && DO_slice;

                while (waitingSliceTask && !State.Acq.ZStack) //Cycle roughly every standard_waitTime.
                {
                    double eTime2 = UIstopWatch_Image.ElapsedMilliseconds;

                    double sampleLength_ms = 0.0;

                    if (fire_DO_cond)
                        sampleLength_ms += State.DO.sampleLength + overHead_ms;

                    if (fire_uncaging_cond)
                        sampleLength_ms += State.Uncaging.sampleLength + overHead_ms;


                    double waitTime = State.Acq.sliceInterval * internalSliceCounter * 1000.0 - sampleLength_ms - eTime2;

                    if (waitTime > standard_waitTime)
                    {
                        System.Threading.Thread.Sleep(standard_waitTime);
                        if (stopGrabActivated)
                            break;
                    }
                    else
                    {
                        if (waitTime > 0 && !stopGrabActivated)
                            System.Threading.Thread.Sleep((int)waitTime);
                        break;
                    }
                } //While

                if ((stopGrabActivated || !waitingSliceTask) && State.Acq.ZStack) //stoped in the middle of loop.
                {
                    flimage.MoveBackToHome();
                    return;
                }

                if (fire_uncaging_cond)
                {
                    bool fire = (uncaging_DO_SliceCounter - State.Uncaging.SlicesBeforeUncage) % State.Uncaging.Uncage_SliceInterval == 0;
                    fire = fire && flimage.uncaging_panel.uncaging_count < State.Uncaging.trainRepeat;
                    if (fire)
                    {
                        if (flimage.uncaging_panel != null)
                        {
                            flimage.uncaging_panel.UncageOnce(true, true);
                        }
                    }
                    else
                        System.Threading.Thread.Sleep((int)(State.Uncaging.sampleLength + overHead_ms));
                }

                if (fire_DO_cond)
                {
                    bool fire = (uncaging_DO_SliceCounter - State.DO.SlicesBeforeDO) % State.DO.SliceInterval == 0;
                    fire = fire && flimage.digital_panel.DO_count < State.DO.trainRepeat;
                    if (fire)
                    {
                        if (flimage.digital_panel != null)
                            flimage.digital_panel.DigitalOutOnce(true); //Takes 40 + uncaging ms.
                    }
                    else
                        System.Threading.Thread.Sleep((int)(State.DO.sampleLength + overHead_ms));
                }
            }
            else
            {
                while (waitingSliceTask && !State.Acq.ZStack)
                {
                    double eTime2 = UIstopWatch_Image.ElapsedMilliseconds;
                    double waitTime = State.Acq.sliceInterval * internalSliceCounter * 1000.0 - eTime2;

                    if (flimage.image_display.FLIM_ImgData.ZStack) //for ZStack, we willl 
                        waitTime = 0;

                    if (waitTime > standard_waitTime)
                    {
                        System.Threading.Thread.Sleep(standard_waitTime);
                        if (stopGrabActivated)
                            break;
                    }
                    else
                    {
                        if (waitTime > 0)
                            System.Threading.Thread.Sleep((int)waitTime);
                        break;
                    }
                }
                //FocusButton.Enabled = false;
            } //While


            if ((stopGrabActivated || !waitingSliceTask) && State.Acq.ZStack) //When stopped, the above loop breaks;
            {
                flimage.MoveBackToHome();
                return;
            }
        }


        /// <summary>
        /// Wait for next slice and then execute next slice. This occurs in different thread, so that this window is released.
        /// </summary>
        void WaitForNextSlice()
        {
            double eTime = UIstopWatch_Image.ElapsedMilliseconds; //Start measuring the time
            waitingSliceTask = true; //you can turn off this to stop this task. Not called by any function for now.

#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Now WaitForNextSlice task started");
#endif

            DisposeDAQ(); //Just to make sure in this thread...
            waitForAcquisitionTaskCompleted();

            uncaging_DO_SliceCounter++;

            if (!read_photon_file)
                Uncaging_DIO_Wait_Function();

            eTime = UIstopWatch_Image.ElapsedMilliseconds;
            measuredSliceInterval = eTime / 1000.0 / internalSliceCounter;

            bool[] eraseMemoryA = boolAllChannels(!State.Acq.aveSlice || averageSliceCounter == 0);

            AO_FrameCounter = 0;
            TCSPC_Native.DLL_Busy = false;

            waitingSliceTask = false; //Now it finished its work.
#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Before Start DAQ in wait slice task"); //Too let us know where we are.
#endif

            if (!stopGrabActivated)
            {
                if (!read_photon_file)
                    ParkMirrors(true);
                var success = StartDAQ(eraseMemoryA, false);
                if (success)
                    EventNotify?.Invoke(this, new ProcessEventArgs("SliceAcquisitionStart", null));
                else
                {
                    read_file_done_signal();
                }
            }

#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Finished wait slice task");
#endif

        }

        public void SetupFLIMParameters(ScanParameters state1)
        {
            parameters = FLIM_Utilities.ConvertStateToFLIMParameters(state1, read_photon_file, parameters);
            TCSPC_SetupParameters();
        }

        /// <summary>
        /// Return success.
        /// </summary>
        /// <param name="slice"></param>
        /// <returns></returns>
        bool SetupPhotonFileNameForSlices(int slice)
        {
            string filename;
            var success = false;

            // Use direct-memory read by default for photon files.
            // Falls back to streaming or file extraction when buffers are too large.
            bool direct_memory = read_photon_file
                && State.Spc.spcData.nDevices <= 1;
            bool stream_memory = false;
            string stream_entry_name = null;

            if (read_photon_file)
            {
                filename = flimage.image_display.photon_filename;
                //flimage.image_display.FLIM_ImgData.State.Acq.maxNFramePerFile = 320000;

                if (flimage.image_display.photon_file_handle != null)
                {
                    flimage.image_display.photon_file_handle.MakePhotonBinaryNameForDLL(slice);
                    var photonBinaryName = flimage.image_display.photon_file_handle.slice_name_for_DLL;
                    parameters.spcData.PhotonsFileName = photonBinaryName;

                    //This is for Binary direct read --- may not work for big file.
                    if (direct_memory)
                    {
                        var entryName = flimage.image_display.photon_file_handle.slice_name_without_dir + "_0.bin";
                        long dataLength = -1;
                        if (flimage.image_display.photon_file_handle.extension == State.Files.extension_photon)
                        {
                            var binPath = Path.Combine(flimage.image_display.photon_file_handle.dirname, entryName);
                            var info = new FileInfo(binPath);
                            dataLength = info.Exists ? info.Length : -1;
                            if (dataLength > 0 && dataLength <= int.MaxValue && dataLength % sizeof(uint) == 0)
                                parameters.spcData.photonBinary = flimage.image_display.photon_file_handle.ReadUInt32ArrayFromFile(binPath);
                        }
                        else
                        {
                            dataLength = flimage.image_display.photon_file_handle.GetPhotonBinaryLength(entryName);
                            if (dataLength > int.MaxValue)
                            {
                                stream_memory = true;
                                stream_entry_name = entryName;
                            }
                            else if (dataLength > 0 && dataLength <= int.MaxValue && dataLength % sizeof(uint) == 0)
                            {
                                parameters.spcData.photonBinary = flimage.image_display.photon_file_handle.ReadUInt32ArrayFromArchive(entryName);
                            }
                        }

                        success = parameters.spcData.photonBinary != null && parameters.spcData.photonBinary.Length > 0;
                        if (!success)
                        {
                            parameters.spcData.photonBinary = null;
                            direct_memory = false;
                        }
                    }
                    if (!direct_memory && !stream_memory)
                        success = flimage.image_display.photon_file_handle.ExtractFile(slice, out string ext);

                    if (stream_memory)
                        success = true;

                    if (!success)
                        return false;
                }
                else
                    return false;
            }
            else //write mode.
            {
                if (slice == 0 || photon_file_handle == null)
                    photon_file_handle = new PhotonFileHandle(State);

                photon_file_handle.MakePhotonBinaryNameForDLL(slice);

                var photonBinaryName = photon_file_handle.slice_name_for_DLL;
                parameters.spcData.PhotonsFileName = photonBinaryName;
                filename_timestamp = parameters.spcData.PhotonsFileName + ".txt";

                if (photon_file_handle.IsStreamingZipWrite)
                {
                    string entryName = photon_file_handle.slice_name_without_dir + "_0.bin";
                    var stream = photon_file_handle.BeginPhotonBinaryStreamWrite(entryName);
                    if (stream != null && FiFo_acquire.SetupPhotonWriteStream(stream) == 0)
                    {
                        return true;
                    }
                    photon_file_handle.EndPhotonBinaryStreamWrite();
                }
            }

            if (stream_memory)
            {
                FiFo_acquire.SetupPhotonBinaryStream(parameters);
                StartPhotonBinaryStream(flimage.image_display.photon_file_handle, stream_entry_name);
            }
            else if (direct_memory)
            {
                FiFo_acquire.SetupPhotonBinary(parameters);
            }
            else
            {
                FiFo_acquire.SetupPhotonFileName(parameters);
            }


            if (read_photon_file)
            {
                if (slice == 0)
                    real_acquired_datetime.Clear();

                DateTime? baseTime = null;
                if (File.Exists(filename_timestamp))
                {
                    var lines = File.ReadAllLines(filename_timestamp);
                    foreach (var line in lines)
                    {
                        var datetime = "";
                        if (line.Contains(","))
                            datetime = line.Split(',')[1];
                        else if (line.Contains("="))
                            datetime = line.Split('=')[1].Replace(" ", "");
                        var time1 = DateTime.ParseExact(datetime, "yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);
                        if (!baseTime.HasValue)
                            baseTime = time1;
                        real_acquired_datetime.Add(time1);
                    }
                }
                else
                {
                    flimage.image_display.photon_file_handle.GetAcquisitionTime(flimage.image_display.photon_file_handle.slice_name_without_dir + ".txt", out DateTime time1);
                    baseTime = time1;
                    real_acquired_datetime.Add(time1);
                }

                if (baseTime.HasValue && flimage.image_display.photon_file_handle != null)
                    flimage.image_display.photon_file_handle.binary_trigger_time = baseTime.Value;
            }

            return true;
        }

        void StartPhotonBinaryStream(PhotonFileHandle photonHandle, string entryName)
        {
            if (photonHandle == null || string.IsNullOrEmpty(entryName) || FiFo_acquire == null)
                return;

            photonStreamTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    photonHandle.StreamUInt32ChunksFromArchive(entryName, PhotonStreamChunkBytes, AppendPhotonStreamChunk);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Photon stream error: " + ex.Message);
                    AppendPhotonStreamChunk(Array.Empty<uint>(), true);
                }
            }, TaskCreationOptions.LongRunning);
        }

        void AppendPhotonStreamChunk(uint[] chunk, bool isLast)
        {
            if (FiFo_acquire == null)
                return;

            while (true)
            {
                int ret = FiFo_acquire.AppendPhotonBinaryStream(chunk, isLast);
                if (ret == 0)
                    break;

                if (ret == 1)
                {
                    System.Threading.Thread.Sleep(1);
                    continue;
                }

                break;
            }
        }

        bool[] boolAllChannels(bool bool1)
        {
            return Enumerable.Repeat<bool>(bool1, State.Acq.nChannels).ToArray();
        }

        void LoopingImageAcq()
        {

            waitForAcquisitionTaskCompleted(); //// You have to do it from different thread!!;

            waitingImageTask = true;

            flimage.Invoke((Action)delegate ()
            {
                flimage.FocusButtonEnable(true);
            });

            //looping = true;

            while (waitingImageTask)
            {
                double eTime = UIstopWatch_Image.ElapsedMilliseconds;
                int waitTime = (int)(State.Acq.imageInterval * 1000 - eTime);
                if (waitTime > 10)
                {
                    System.Threading.Thread.Sleep(10);
                }
                else if (waitTime <= 0)
                {
                    System.Threading.Thread.Sleep(1);
                    break;
                }
                else
                {
                    if (waitTime > 0)
                        System.Threading.Thread.Sleep((int)(State.Acq.imageInterval * 1000 - eTime));
                    break;
                }

                if (stopGrabActivated)
                {
                    return;
                }
            }

            //FocusButton.Enabled = false;
            if (focusing)
                StopFocus();

            if (!waitingImageTask || stopGrabActivated)
                return;

            waitingImageTask = false;

            if (!stopGrabActivated)
                StartGrab(false);
            else
                StopGrab(true);
        }

        private void read_file_done_signal()
        {
            flimage.image_display.finish_reading_photon_file();
            read_photon_file = false;
            post_grabbing_process = false;
            runningImgAcq = false;
            grabbing = false;
            focusing = false;
            UIstopWatch_Loop.Stop();
            UIstopWatch_Image.Stop();
            SW_PerformanceMonitor.Stop();
            EventNotify?.Invoke(this, new ProcessEventArgs("ReadFileDone", null));
            acquisition_done_event_activated = true;

            var pendingSaveTask = (saveTask != null && !saveTask.IsCompleted);
            if (pendingSaveTask)
            {
                flimage.InvokeIfRequired(o =>
                {
                    o.ChangeItemsStatus(false, false);
                    o.GrabButton.Text = "SAVING";
                    o.GrabButton.Enabled = false;
                    o.FocusButton.Enabled = false;
                });

                Task.Factory.StartNew(() =>
                {
                    try { saveTask.Wait(); } catch { /* ignore */ }

                    flimage.BeginInvokeIfRequired(o =>
                    {
                        o.StopGrab_GUI_Update();
                        o.SetEnableStatesOfControls();
                    });

                    TryStartPendingFocus();
                });

                return;
            }

            flimage.InvokeIfRequired(o => o.StopGrab_GUI_Update());
            flimage.InvokeIfRequired(o => o.SetEnableStatesOfControls());
            TryStartPendingFocus();
        }

        public bool TryStartFocus()
        {
            if (read_photon_file && !runningImgAcq && (saveTask == null || saveTask.IsCompleted))
            {
                if (FiFo_acquire == null || !FiFo_acquire.Running)
                    read_file_done_signal();
            }

            if (focusing)
                return true;

            if (IsReadOrSaveBusy())
            {
                _pendingFocusStart = true;
                flimage.BeginInvokeIfRequired(o => o.WriteStatusText("Finishing photon read..."));
                return false;
            }

            initializeFocusing();
            StartGrab(true);
            return true;
        }

        private void TryStartPendingFocus()
        {
            if (!_pendingFocusStart)
                return;

            if (IsReadOrSaveBusy())
                return;

            _pendingFocusStart = false;
            initializeFocusing();
            StartGrab(true);
        }

        private bool IsReadOrSaveBusy()
        {
            if (read_photon_file || post_grabbing_process)
                return true;
            return saveTask != null && !saveTask.IsCompleted;
        }

        ///////////////////////////////////Measurement Done Handle//////////////////////////////////////
        public void MeasDoneEvent(FiFio_multiBoards fifo, EventArgs e)
        {
            if (fifo.saturated)
            {
                if (focusing)
                    StopFocus();
                if (grabbing)
                    StopGrab(true);

                MessageBox.Show(Form.ActiveForm, "FiFo saturated!!");
            }
            else
            {
                if (State.Acq.photon_file_format) // && grabbing)
                {
                    if (!read_photon_file)
                    {
                        if (photon_file_handle != null)
                        {
                            if (photon_file_handle.IsStreamingZipWrite)
                            {
                                photon_file_handle.EndPhotonBinaryStreamWrite();
                                photon_file_handle.AddPhotonTimeEntry(photon_file_handle.slice_name_without_dir + ".txt", acquiredTime);
                                FiFo_acquire.ClearPhotonWriteStream();
                            }
                            else
                            {
                                photon_file_handle.AddPhotonFileEntry(parameters.spcData.PhotonsFileName, acquiredTime);
                            }
                        }

                    }
                    else
                    {
                        if (flimage.image_display.photon_file_handle != null)
                            flimage.image_display.photon_file_handle.FinishReading();
                    }

                    if (acquisition_done_event_activated || read_photon_file)
                    {
                        grabbing = false; //grab is finished. (not necessary but just in case).

                        if (!read_photon_file)
                        {
                            photon_file_handle?.FinishWriting();
                            post_grabbing_process = true;
                            EventNotify?.Invoke(this, new ProcessEventArgs("AcquisitionDone", null));
                            post_acquisition_analysis();
                            post_grabbing_process = false; //analysis is also finished.
                        }
                        else
                        {
                            read_file_done_signal();
                        }

                    }
                else
                {
                    if ((grabbing || read_photon_file) && !focusing)
                        waitSlice = Task.Factory.StartNew(() =>
                        {
                            lock (waitSliceTaskobj) //This task will never overlap. Don't use in any other lock.
                                    WaitForNextSlice();
                        });
                }
            }

            fileIO?.CloseAllFlimWriters();
        }


            if (!read_photon_file)
            {

#if !DEBUG
                try
                {
#endif

                if (flimage.fastZcontrol != null)
                {
                    //BeginInvoke is correct.
                    flimage.fastZcontrol.Invoke((Action)delegate
                    {
                        flimage.fastZcontrol.Enabled = true;
                        if (parameters.fastZScan.measureTagParameters)
                            flimage.fastZcontrol.CalculateFastZParameters();
                    });
                }
                else
                    State.Acq.fastZScan = false;
#if !DEBUG
                }
                catch (Exception EX)
                {
                    Debug.WriteLine("***Main window is closed!!*****" + EX.ToString());
                }
#endif
            }
        }


        ///////////////////////////////////STRIPE EVENT HANDLING////////////////////////////////////////

        ///////////////////////////////////FRAME EVENT HANDLING////////////////////////////////////////

        private void SnapShotProcess()
        {
            if (snapShot & tcspc_on)
            {
                //Display whatever the item on the image_display.
                flimage.DisplaySnapShot();

                snapShot = false;
                State = SaveState;
                flimage.State = SaveState;
                updateState(State);

                //Back to original parameters.
                flimage.Invoke((Action)delegate
                {
                    flimage.SetParametersFromState(true);
                });

                return;
            }
        }


        public void FrameDoneEvent(FiFio_multiBoards fifo, FrameEventArgs e) //called when a Frame is done.
        {
            lock (syncFLIMacq) //acquisition lock.
            {
                FrameDoneEvent_Core(fifo, e);
            }
        }

        /// <summary>
        /// This handles acquired images. Called by FiFo when a frame is acquired.
        /// </summary>
        /// <param name="fifo"></param>
        public void FrameDoneEvent_Core(FiFio_multiBoards fifo, FrameEventArgs e)
        {
#if DEBUG
            if (DEBUGMODE != 0)
            {
                Debug.WriteLine("Debug: C# Frame done event received. Time = " + DateTime.Now.ToString("HH:mm:ss.fff") + " Frame = " + e.frameNumber);
                SW_PerformanceMonitorFrame.Restart();
            }
#endif

            var state1 = State;

            if (read_photon_file)
                state1 = flimage.image_display.FLIM_ImgData.State;

            bool acceptFrame = fifo.Running || (parameters != null && parameters.fiberPhotometryMode && grabbing);

            if (acceptFrame)
            {
                UInt16[][][,,] FLIMTemp = e.data; //order = c, z, [y,x,t]

                flimage.image_display.FLIM_ImgData.LoadFLIMdata5D_Realtime(FLIMTemp, acquiredTime, false); //Save in FLIM_ImgData class. ShallowCopy       
                flimage.image_display.FLIM_ImgData.MakeFLIM_Pages4DFromFLIMRaw5D(false);

                if (state1.Acq.fastZScan)
                {
                    flimage.image_display.calcZProjection(); //For realtime, it just displays one image.
                }
                else
                {
                    flimage.image_display.FLIM_ImgData.LoadFLIMRawFromData4D(flimage.image_display.FLIM_ImgData.FLIM_Pages[0], acquiredTime, false); //ShallowCopy
                }
            }

            // Line-scan trace mode: update realtime plot once per frame, but append one point per line.
            if (runningImgAcq && acceptFrame && grabbing && _lineScanTraceActive && !focusing)
            {
                try
                {
                    flimage.image_display.UpdateRealtimePlotFromFrame(e.data);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("UpdateRealtimePlotFromFrame failed: " + ex.Message);
                }
            }

            if (runningImgAcq && acceptFrame)
            {
                bool[] savebool = new bool[parameters.nChannels];  //boolAllChannels(false);
                bool protecting_save_task = false;

                bool updateImage = true;


                if (focusing || (grabbing && parameters.spcData.savePhotonsInFile && !read_photon_file))
                {
                    updateImage = true;
                }
                else if (grabbing) //include photon file.
                {
                    for (int i = 0; i < parameters.nChannels; i++)
                    {
                        bool avg = parameters.averageFrame != null && i < parameters.averageFrame.Length && parameters.averageFrame[i];
                        bool acq = parameters.acquisition != null && i < parameters.acquisition.Length && parameters.acquisition[i];
                        savebool[i] = (!(avg && parameters.n_average > 1)) && acq;
                    }

                    updateImage = false;
                    bool average_done = true;

                    if (parameters.averageFrame.Any(x => x == true) && parameters.n_average > 1)
                    {
                        if (!read_photon_file)
                        {
                            averageCounter++;

                            if (averageCounter % parameters.focusAverage == 0 || averageCounter % parameters.n_average == 0)
                                updateImage = true;

                            if (!state1.Acq.aveSlice)
                            {
                                flimage.image_display.FLIM_ImgData.nAveragedFrame = GetAverageFrame(averageCounter, parameters);
                            }
                            else
                                flimage.image_display.FLIM_ImgData.nAveragedFrame = Enumerable.Repeat(averageCounter + parameters.n_average * averageSliceCounter, parameters.nChannels).ToArray();

                            average_done = parameters.n_average == averageCounter;
                        }
                        else //read photon file.
                        {
                            if (!state1.Acq.aveSlice)
                            {
                                flimage.image_display.FLIM_ImgData.nAveragedFrame = GetAverageFrame(parameters.n_average, parameters);
                            }
                            else
                                flimage.image_display.FLIM_ImgData.nAveragedFrame = Enumerable.Repeat(parameters.n_average * averageSliceCounter, parameters.nChannels).ToArray();

                            average_done = e.frameNumber % parameters.n_average == 0;
                        } //

                        if (average_done) //Finished averaging 1 slice
                        {
                            averageCounter = 0;
                            averageSliceCounter++;

                            protecting_save_task = true;
                            updateImage = true;

                            if (!state1.Acq.aveSlice)
                            {
                                savebool = (bool[])parameters.acquisition.Clone();
                            }
                            else // aveSlice
                            {
                                if (averageSliceCounter == state1.Acq.nAveSlice)
                                {
                                    averageSliceCounter = 0;
                                    savebool = (bool[])parameters.acquisition.Clone();
                                }
                            } // aveSlice
                        } //Frame
                    } //Average
                    else //not average
                    {
                        protecting_save_task = false;
                        updateImage = true;
                    }
                }

                if (read_photon_file)
                {
                    protecting_save_task = true;
                    updateImage = true;
                }

#if DEBUG
                if (force_stop || stopGrabActivated)
                    Debug.WriteLine("Force stop activated");
                Debug.WriteLine("Debug: C# Frame Done process 1");
#endif
                if (grabbing || focusing)
                {
                    //protecting_save_task = true; 
                    flimage.image_display.FLIM_ImgData.saveChannels = (bool[])savebool.Clone();
                    SaveUpdateAcquiredImage(savebool, protecting_save_task, updateImage, state1);
                }

                if (grabbing || focusing)
                    FLIMProgressChanged(e.frameNumber, state1);
                else
                    Debug.WriteLine("No progress");

            } //Running

#if DEBUG
            if (DEBUGMODE != 0)
            {
                SW_PerformanceMonitorFrame.Stop();
                Debug.WriteLine("Debug: C# Time to acquire frame: " + SW_PerformanceMonitorFrame.ElapsedMilliseconds + " ms");
            }
#endif

        } //FrameDoneEvent_Core.

        public int[] GetAverageFrame(int nAverage, FLIM_Parameters parameters)
        {
            int[] aveN = new int[parameters.nChannels];
            for (int c = 0; c < parameters.nChannels; c++)
            {
                bool avg = parameters.averageFrame != null && c < parameters.averageFrame.Length && parameters.averageFrame[c];
                if (avg)
                    aveN[c] = nAverage;
                else
                    aveN[c] = 1;
            }

            return aveN;
        }


        /// <summary>
        /// SaveUpdateAcquiredImage: Called by FrameDoneEvent_Core. Control saving and displaying acquired data.
        /// </summary>
        /// <param name="saveFileBools"></param>
        /// <param name="protecting_save_task"></param>
        public void SaveUpdateAcquiredImage(bool[] saveFileBools, bool protecting_save_task, bool updateImage, ScanParameters state1)
        {
            DateTime acTime;
            double msPerLine = state1.Acq.fastZScan ? state1.Acq.FastZ_msPerLine : state1.Acq.msPerLine;
            if (state1.Acq.resonantScanning)
                msPerLine = 500.0 / state1.Init.resonantFreq_Hz;

#if DEBUG
            Debug.WriteLine("Debug: C# SaveUpdateAcquiredImage process 1");
#endif
            // Fiber photometry: sampling time is stored in msPerLine (bin size).
            if (microscope_system == MicroscopeSystem.FiberPhotometry && parameters != null && parameters.fiberPhotometryMode)
                msPerLine = (state1?.Acq != null && state1.Acq.fiberBin_ms > 0) ? state1.Acq.fiberBin_ms : parameters.fiberBin_ms;

#if DEBUG
            Debug.WriteLine("Debug: C# SaveUpdateAcquiredImage process 1");
#endif

            //Deepcopy of FLIMRaw5D.
            if (saveFileBools.Any(x => x == true) || read_photon_file) //If any savefile exists.
            {
                // Timestamp for each saved page.
                // Normal imaging: msPerLine * linesPerFrame.
                // Fiber photometry: msPerLine is the bin size, so page interval is msPerLine * linesPerFrame.
                double msPerPage = msPerLine * state1.Acq.linesPerFrame;

                acTime = acquiredTime.AddMilliseconds(internalFrameCounter * msPerPage);

                //if (real_acquired_datetime.Count > internalFrameCounter)
                //    acTime = real_acquired_datetime[internalFrameCounter];

                var FLIMForSave = (ushort[][][,,])flimage.image_display.FLIM_ImgData.FLIMRaw5D.Clone();  //Shallow copy.
                                                                                                         //Copier.DeepCopyArray(flimage.image_display.FLIM_ImgData.FLIMRaw5D);

                flimage.image_display.FLIM_ImgData.saveChannels = saveFileBools;

                for (int i = 0; i < saveFileBools.Length; i++)
                {
                    if (!saveFileBools[i])
                    {
                        FLIMForSave[i] = null;
                    }
                }

                if (flimage.image_display.FLIM_ImgData.KeepPagesInMemory || flimage.image_display.FLIM_ImgData.ZStack)
                {
                    flimage.image_display.FLIM_ImgData.Add5DFLIM(FLIMForSave, acTime, savePageBufferCounter, false);
                    savePageBufferCounter++;
                }
                else
                {
                    FLIMSaveBuffer.Add(FLIMForSave);
                    acquiredTimeList.Add(acTime);
                }
            }


#if DEBUG
            Debug.WriteLine("Debug: C# SaveUpdateAcquiredImage process 2");
#endif

            if ((focusing || grabbing) && updateImage)
                UpdateImages(state1);

#if DEBUG
            Debug.WriteLine("Debug: C# SaveUpdateAcquiredImage process 3");
#endif


            if (grabbing && saveFileBools.Any(x => x == true) && (!state1.Acq.photon_file_format || read_photon_file))
            {
                SaveFile(protecting_save_task, state1);
            }
        }

        /// <summary>
        /// If not keeping the data in memory, the stack is removed after saving. For ZStack, everything is saved in memory.
        /// </summary>
        public void CheckDeletePageBuffer()
        {
            if (!(flimage.image_display.FLIM_ImgData.KeepPagesInMemory || flimage.image_display.FLIM_ImgData.ZStack))
            {
                if (displayPageCounter > deletedPageCounter && savePageCounter > deletedPageCounter)
                {
                    if (FLIMSaveBuffer != null && FLIMSaveBuffer.Count > 0)
                    {
                        RemoveFrameAt(0);
                        if (acquiredTimeList != null && acquiredTimeList.Count > 0)
                            acquiredTimeList.RemoveAt(0);
                        deletedPageCounter++;
                    }
                }
            }
        }

        public void RemoveFrameAt(int frameToWork) //Clear memory very well.
        {
            if (FLIMSaveBuffer != null && frameToWork < FLIMSaveBuffer.Count)
                FLIMSaveBuffer.RemoveAt(frameToWork);
        }

        public int totalPagesSaved(ScanParameters state1)
        {
            int aveNFrame = 1;
            if (state1.Acq.aveFrameA.Any(x => x == true))
                aveNFrame = state1.Acq.nAveFrame;

            int TotalNPages = state1.Acq.nFrames / aveNFrame;
            int aveNslices = 1;
            if (state1.Acq.aveSlice)
                aveNslices = state1.Acq.nAveSlice;

            TotalNPages = TotalNPages * state1.Acq.nSlices / aveNslices;

            return TotalNPages;
        }

        public void SaveFile(bool protecting_save_task, ScanParameters state1)
        {

#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Start Save File");
#endif

            bool updated = false;

            int savePage = savePageCounter - deletedPageCounter;

            // IMPORTANT: this can be called on the TCSPC callback thread (FrameDoneEvent path).
            // Avoid blocking per-frame on file I/O; instead, only wait when the caller explicitly requests
            // protection (e.g., end-of-average/end-of-slice where ordering matters).
            if (saveTask != null && !saveTask.IsCompleted)
            {
                if (protecting_save_task)
                    saveTask.Wait();
                else
                    return; // try again on the next frame; savePageCounter hasn't advanced yet.
            }

            if (flimage.image_display.FLIM_ImgData.KeepPagesInMemory || flimage.image_display.FLIM_ImgData.ZStack)
            {
                //Save task occurs only when savePage Counter is less than 5D page.
                updated = (flimage.image_display.FLIM_ImgData.FLIM_Pages5D.Length > savePageCounter);
                if (!updated)
                {
                    Debug.WriteLine("**** FLIM_Page5D = {0}, savePageCounter = {1} ****", flimage.image_display.FLIM_ImgData.FLIM_Pages5D.Length, savePageCounter);
                }
            }
            else
            {
                updated = (FLIMSaveBuffer.Count > savePage && savePage >= 0);
            }


            if (updated && (saveTask == null || saveTask.IsCompleted))
            {
                if (protecting_save_task)
                {
                    SaveTask(savePage, state1);
                }
                else
                {
                    saveTask = Task.Factory.StartNew((object obj) =>
                    {
                        var data = (dynamic)obj;
                        SaveTask(data.page, state1);
                    }, new { page = savePage });
                }

            }
            else
            {
                Debug.WriteLine("Saving BUSY***************************Could not save:" + (savePageCounterTotal + 1) + " (" + (savePageCounter + 1) + "/" + flimage.image_display.FLIM_ImgData.FLIM_Pages5D.Count() + ")");
            }

            flimage.InvokeAnyway(o => o.UpdateSavedNumberOfFile());

        }


        /// <summary>
        /// Save acquired image. Called in different thread (Task.Factory.StartNew). 
        /// </summary>
        /// <param name="savePage"></param>
        public void SaveTask(int savePage, ScanParameters state1)
        {
#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Start Save Task");
#endif

            lock (syncFLIMsave)
            {
#if DEBUG
                if (DEBUGMODE != 0)
                    Debug.WriteLine("Debug: C# Start FLIMsave sync");
#endif

                UInt16[][][,,] FLIMImage; //Before permutation.
                DateTime acqTimeTemp;

                ushort[][][,,] FLIM_5D; //After permutation.

                ////Overwrite and savefile setting
                bool[] overwrite = (bool[])newFile.Clone();

                int error = 0;
                String fullFileName = state1.Files.fullName();

                if (read_photon_file)
                    fullFileName = flimage.image_display.FLIM_ImgData.fullFileName;

                bool[] saveCh = boolAllChannels(true);

                state1.Acq.version = flimage.version; //Just to make sure the version.


                if (flimage.image_display.FLIM_ImgData.KeepPagesInMemory || flimage.image_display.FLIM_ImgData.ZStack)
                {
                    FLIMImage = flimage.image_display.FLIM_ImgData.FLIM_Pages5D[savePageCounter];
                    acqTimeTemp = flimage.image_display.FLIM_ImgData.acquiredTime_Pages5D[savePageCounter];
                    FLIM_5D = ImageProcessing.PermuteFLIM5D(FLIMImage, false); //ShallowCopy
                }
                else
                {
                    FLIMImage = FLIMSaveBuffer[savePage];
                    acqTimeTemp = acquiredTimeList[savePage];
                    FLIM_5D = ImageProcessing.PermuteFLIM5D(FLIMImage, false); //ShallowCopy
                }

#if DEBUG
                if (DEBUGMODE != 0)
                    Debug.WriteLine("Debug: C# Start Saving File.");
#endif

                for (int ch = 0; ch < state1.Acq.nChannels; ch++)
                {
                    if (FLIMImage == null || FLIMImage[ch] == null)
                        saveCh[ch] = false;
                }



                //if (read_photon_file && State.Acq.nFrames > 100000)
                //{
                //    if (flimage.image_display.photon_file_handle != null)
                //    {
                //        flimage.image_display.photon_file_handle.SaveFLIMinArchive(savePageCounter, FLIM_5D, acqTimeTemp);
                //    }
                //}
                //else
                {
                    state1.Acq.acqFLIM = false;
                    for (int ch = 0; ch < state1.Acq.nChannels; ch++)
                        state1.Acq.acqFLIM = (state1.Acq.acqFLIM || (state1.Acq.acqFLIMA[ch] && saveCh[ch]));

#if DEBUG
                    if (DEBUGMODE != 0)
                        Debug.WriteLine("Debug: State.Acq.acqFLIM {0}, nChannels = {1}", state1.Acq.acqFLIM, state1.Acq.nChannels);
#endif

                    if (!state1.Acq.acqFLIM)
                    {
                        flimage.saveIntensityImage = true;
                    }
                    else
                    {
                        if (!state1.Files.channelsInSeparatedFile)
                        {
                            bool overwrite1 = overwrite[0]; //|| savePageCounter == 0;

                            if (FLIM_5D.Length == 1)
                            {
                                try
                                {
                                    if (fileIO == null || !ReferenceEquals(fileIO.State, state1))
                                    {
                                        fileIO = new FileIO(state1);
                                    }
                                    fileIO.HoldFastWriterOpen = !read_photon_file && ShouldHoldFastWriterOpen(state1);
                                    error = fileIO.SaveFLIMInTiff(fullFileName, FLIM_5D[0], acqTimeTemp, overwrite1, saveCh);
                                }
                            catch (Exception ex)
                            {
                                NotifySaveFailureAndStop(fullFileName, overwrite1, ex);
                                return;
                            }

#if DEBUG
                                Debug.WriteLine("Saving file.. {0}, overwrite = {1}, FileCounter = {2}/{3}", fullFileName, overwrite1, state1.Files.fileCounter, savePageCounter);
#endif
                            }
                            else
                        {
                            try
                            {
                                if (fileIO == null || !ReferenceEquals(fileIO.State, state1))
                                {
                                    fileIO = new FileIO(state1);
                                }
                                fileIO.HoldFastWriterOpen = !read_photon_file && ShouldHoldFastWriterOpen(state1);
                                error = fileIO.SaveFLIMInTiffZStack(fullFileName, FLIM_5D, acqTimeTemp, overwrite1, saveCh);
                            }
                            catch (Exception ex)
                            {
                                NotifySaveFailureAndStop(fullFileName, overwrite1, ex);
                                return;
                            }
                        }

                            filename_last = fullFileName;
                        }
                        else //Separate channel.
                        {
                            for (int ch = 0; ch < state1.Acq.nChannels; ch++)
                            {
                                if (FLIMImage[ch] != null)
                                {
                                    saveCh = new bool[state1.Acq.nChannels];
                                    saveCh[ch] = true;

                                    bool overwrite1 = overwrite[ch];
                                    String fileName = state1.Files.fullName(ch);
                                    if (FLIM_5D.Length == 1)
                                    {
                                        ushort[][,,] FLIM_separated = new ushort[state1.Acq.nChannels][,,];
                                        FLIM_separated[ch] = FLIM_5D[0][ch];
                                    try
                                    {
                                        if (fileIO == null || !ReferenceEquals(fileIO.State, state1))
                                        {
                                            fileIO = new FileIO(state1);
                                        }
                                        fileIO.HoldFastWriterOpen = !read_photon_file && ShouldHoldFastWriterOpen(state1);
                                        error = fileIO.SaveFLIMInTiff(fileName, FLIM_separated, acqTimeTemp, overwrite1, saveCh);
                                    }
                                    catch (Exception ex)
                                    {
                                        NotifySaveFailureAndStop(fileName, overwrite1, ex);
                                        return;
                                    }

#if DEBUG
                                        Debug.WriteLine("Saving file.. {0}, channel = {1}, overwrite = {2}", fileName, ch, overwrite1);
#endif
                                    }
                                    else
                                    {
                                        ushort[][][,,] FLIM_separated = new ushort[FLIM_5D.Length][][,,];
                                        for (int z = 0; z < FLIM_5D.Length; z++)
                                        {
                                            FLIM_separated[z][ch] = FLIM_5D[z][ch];
                                        }

                                    try
                                    {
                                        if (fileIO == null || !ReferenceEquals(fileIO.State, state1))
                                        {
                                            fileIO = new FileIO(state1);
                                        }
                                        fileIO.HoldFastWriterOpen = !read_photon_file && ShouldHoldFastWriterOpen(state1);
                                        error = fileIO.SaveFLIMInTiffZStack(fileName, FLIM_separated, acqTimeTemp, overwrite1, saveCh);
                                    }
                                    catch (Exception ex)
                                    {
                                        NotifySaveFailureAndStop(fileName, overwrite1, ex);
                                        return;
                                    }

#if DEBUG
                                        Debug.WriteLine("Saving file.. {0}, channel = {1}, overwrite = {2}, n_zslice = ", fileName, ch, overwrite1, FLIM_5D.Length);
#endif
                                    }

                                    filename_last = fileName;
                                }
                            }
                        }
                    }

                if (error < 0)
                {
                    NotifySaveFailureAndStop(filename_last ?? fullFileName, overwrite.Any(x => x), null, error);
                    return;
                }

                    if (flimage.saveIntensityImage && !state1.Acq.photon_file_format)
                    {
                        for (int c = 0; c < state1.Acq.nChannels; c++)
                        {
                            if (FLIMImage != null && FLIMImage[c] != null)
                            {
                                if (state1.Files.channelsInSeparatedFile)
                                {
                                    saveCh = new bool[state1.Acq.nChannels];
                                    saveCh[c] = true;
                                }

                                var file_type = FileIO.ImageType.FLIMRaw;
                                if (state1.Acq.acqFLIM)
                                    file_type = FileIO.ImageType.Intensity;

                                string baseName = state1.Files.baseName;
                                string dirName = state1.Files.pathName;

                                if (read_photon_file)
                                {
                                    baseName = flimage.image_display.FLIM_ImgData.baseName;
                                    dirName = flimage.image_display.FLIM_ImgData.pathName;
                                }

                                String fileName = fileIO.FLIM_FilePath(c, state1.Files.channelsInSeparatedFile, state1.Files.fileCounter, file_type, "", dirName, baseName, ".tif");

                                for (int z = 0; z < FLIM_5D.Length; z++)
                                {
                                    bool overwrite1 = (overwrite[c] && (z == 0)) && (state1.Files.channelsInSeparatedFile || c == 0);
                                try
                                {
                                    var err2 = fileIO.Save2DImageInTiff(fileName, ImageProcessing.GetProjectFromFLIM(FLIM_5D[z][c], flimage.image_display.FLIM_ImgData.fit_range[c]), acqTimeTemp, overwrite1, saveCh);
                                    if (err2 < 0)
                                    {
                                        NotifySaveFailureAndStop(fileName, overwrite1, null, err2);
                                        return;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    NotifySaveFailureAndStop(fileName, overwrite1, ex);
                                    return;
                                }
                                }

                                filename_last = fileName;
                            }
                        }
                    }
                }

                if (overwrite.Any(x => x == true))
                    flimage.image_display.FLIM_ImgData.image_description = fileIO.image_description;


                savePageCounter++;
                savePageCounterTotal++;

                int TotalNPages = totalPagesSaved(state1);

                if (TotalNPages == savePageCounter)
                {
                    if (read_photon_file)
                        fileIO?.CloseAllFlimWriters();
                    EventNotify?.Invoke(this, new ProcessEventArgs("SaveImageDone", FLIMImage));
                }

                //Save file in exiting file from Frame = 2.
                newFile = boolAllChannels(false);

                if (parameters.enableFastZscan && !(state1?.Acq?.fiberPhotometryMode ?? false))  //always in different files. to avoid confusion.
                {
                    newFile = boolAllChannels(true);
                    fileIO.CloseAllFlimWriters();
                    state1.Files.fileCounter++;
                    flimage.UpdateFileName();
                }

                if (!read_photon_file &&
                    savePageCounter == state1.Acq.maxNFramePerFile &&
                    savePageCounterTotal < TotalNPages &&
                    !(state1?.Acq?.fiberPhotometryMode ?? false))
                {
                    //Finished saving. Reaching maximum page number. 

                    if (flimage.image_display.FLIM_ImgData.KeepPagesInMemory || flimage.image_display.FLIM_ImgData.ZStack)
                    {
                        newFile = boolAllChannels(true);
                        fileIO.CloseAllFlimWriters();
                        state1.Files.fileCounter++;
                        flimage.UpdateFileName();

                        //lock (saveBufferObj)
                        {
                            flimage.image_display.FLIM_ImgData.RemovePageRange5D(0, state1.Acq.maxNFramePerFile);
                            displayPageCounter = displayPageCounter - state1.Acq.maxNFramePerFile;

                            savePageCounter = 0;

                            savePageBufferCounter -= state1.Acq.maxNFramePerFile;

                            if (displayPageCounter < 0)
                                displayPageCounter = 0;
                        }

                    }
                    //else
                    //{
                    //    savePageCounter = 0;
                    //    deletedPageCounter = deletedPageCounter - state1.Acq.maxNFramePerFile;
                    //    displayPageCounter = displayPageCounter - state1.Acq.maxNFramePerFile;

                    //    if (displayPageCounter < 0)
                    //        displayPageCounter = 0;

                    //    if (deletedPageCounter < -1)
                    //    {
                    //        // Should be -1. It will be added at CheckDeletePageBuffer();
                    //        Debug.WriteLine("Deleted Counter < 0 problem!!" + deletedPageCounter + ", Disp = " + displayPageCounter);
                    //    }

                    //}
                }
                // }


                //Debug.WriteLine("PageCount = " + flimage.image_display.FLIM_ImgData.FLIM_Pages5D.Count + ", save Count" + savePageCounter);

                CheckDeletePageBuffer();

                //EventNotify?.Invoke(this, new ProcessEventArgs("FrameAcquisitionDone", null));

            } //Sync

#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Ended Save Task");
#endif
        }

        private void NotifySaveFailureAndStop(string fileName, bool overwrite, Exception ex = null, int errorCode = int.MinValue)
        {
            // Avoid spamming multiple dialogs for the same root cause.
            if (_saveErrorNotified)
                return;
            _saveErrorNotified = true;

            var mode = overwrite ? "overwrite" : "append";
            var details = "";
            if (errorCode != int.MinValue)
                details += "ErrorCode = " + errorCode + "\r\n";
            if (ex != null)
                details += ex.GetType().Name + ": " + ex.Message + "\r\n";

            var msg =
                "Saving failed (" + mode + ").\r\n\r\n" +
                "File:\r\n" + fileName + "\r\n\r\n" +
                (string.IsNullOrEmpty(details) ? "" : ("Details:\r\n" + details + "\r\n")) +
                "Common causes:\r\n" +
                "- The file is open in another program (including Windows Explorer preview pane)\r\n" +
                "- Cloud sync / antivirus is temporarily locking the file\r\n" +
                "- The file/folder is read-only or you lack permission\r\n\r\n" +
                "Close anything that might be using the file and try again.";

            try
            {
                flimage.BeginInvokeIfRequired(o =>
                {
                    o.WriteStatusText("Save failed: " + Path.GetFileName(fileName));
                    MessageBox.Show(Form.ActiveForm, msg, "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
            catch
            {
                // ignore UI errors (e.g., closing)
            }

            // Stop acquisition ASAP to avoid further unsaved data.
            stopGrabActivated = true;
            force_stop = true;
            fileIO?.CloseAllFlimWriters();
            try { StopGrab(true); } catch { }
        }

        /// <summary>
        /// CheckSavingParameters: creating directory for saving etc before starting grab.
        /// </summary>
        /// <returns></returns>
        public int CheckSavingParameters()
        {
            bool[] saveChannels = Enumerable.Repeat<bool>(true, flimage.image_display.FLIM_ImgData.nChannels).ToArray();
            fileIO.CreateHeader(saveChannels);
            System.IO.Directory.CreateDirectory(State.Files.pathName);
            System.IO.Directory.CreateDirectory(State.Files.pathNameIntensity);
            System.IO.Directory.CreateDirectory(State.Files.pathNameFLIM);
            string fname = State.Files.fullName();
            if (State.Acq.photon_file_format)
                fname = State.Files.GetPhotonFilePath(true);
            //fname = fname.Split('.')[0] + State.Files.extension_photon;

            if (System.IO.File.Exists(fname))
            {
                // Show overwrite prompt on the UI thread (worker-thread MessageBox can appear hidden and look like a stall).
                DialogResult dr = DialogResult.No;
                if (suppressOverwritePrompt)
                {
                    dr = DialogResult.Yes; // Skip the confirmation dialog.
                }
                else
                {
                    try
                    {
                        if (flimage != null)
                        {
                            dr = flimage.InvokeIfRequired_withReturn(o =>
                                MessageBox.Show(
                                    o,
                                    "File already exists! Do you want to overwrite?\r\n\r\n" + fname,
                                    "Overwrite?",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning));
                        }
                        else
                        {
                            dr = MessageBox.Show(
                                "File already exists! Do you want to overwrite?\r\n\r\n" + fname,
                                "Overwrite?",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning);
                        }
                    }
                    catch
                    {
                        // If invoke fails for any reason, fall back.
                        dr = MessageBox.Show(
                            "File already exists! Do you want to overwrite?\r\n\r\n" + fname,
                            "Overwrite?",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);
                    }
                }

                if (dr != DialogResult.Yes)
                    return -1;

                // Delete upfront so overwrite failure is immediate and visible (not a silent stall later).
                try
                {
                    try { File.SetAttributes(fname, FileAttributes.Normal); } catch { }

                    const int retries = 40; // ~2 seconds total
                    for (int i = 0; i < retries; i++)
                    {
                        try
                        {
                            File.Delete(fname);
                            break;
                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(50);
                        }
                    }

                    if (File.Exists(fname))
                        throw new IOException("File still exists after delete attempts.");

                    flimage?.BeginInvokeIfRequired(o => o.WriteStatusText("Overwriting: deleted existing file"));
                }
                catch (Exception ex)
                {
                    try
                    {
                        flimage?.BeginInvokeIfRequired(o => o.WriteStatusText("Overwrite failed (delete): " + Path.GetFileName(fname)));
                    }
                    catch { }

                    MessageBox.Show(
                        flimage as IWin32Window,
                        "Could not delete the existing file for overwrite:\r\n\r\n" +
                        fname + "\r\n\r\n" +
                        ex.Message + "\r\n\r\n" +
                        "Common causes:\r\n" +
                        "- Windows Explorer preview pane / thumbnail generation\r\n" +
                        "- Another program has the file open\r\n" +
                        "- Cloud sync / antivirus is locking it\r\n" +
                        "- Read-only / permissions\r\n",
                        "Overwrite failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return -1;
                }

                return 0;

            }
            else
                return (0);
        }


        ///////////////////////////////////FRAME EVENT HANDLING////////////////////////////////////////

        ///////////////////////////////////STRIPE EVENT HANDLING////////////////////////////////////////
        public void StripeDoneEventAll(UInt16[][][,,] StripeImage, StripeEventArgs e)
        {
#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Stripe event called: device = " + e.device + "/ channel" + e.channelList[0] + " / " + e.channelList.Count + " FirstLine = " + e.StartLine + "EndLine = " + e.EndLine);
#endif
            try
            {
                for (int ch = 0; ch < e.channelList.Count; ch++)
                {
                    flimage.image_display.UpdateStripe(StripeImage, e.channelList[ch], e.StartLine, e.EndLine);
                }
            }
            catch (Exception E1)
            {
                Debug.WriteLine("Error during Stripe: " + E1.Message);
            }
        }

        public void StripeDoneEvent(FiFio_multiBoards fifo, StripeEventArgs e)
        {
            if (focusing)
            {
                StripeDoneEventAll(fifo.FLIM_Stripe, e);
            }
        }


        /// <summary>
        /// Called when Background acquisition is done. However, it should be noted that save task and display Task are in spearated threads.
        /// </summary>
        public void FLIMProgressChanged(int frame_counter, ScanParameters state1)
        {

            int[] nAve = GetAverageFrame(parameters.n_average, parameters);

            for (int ch = 0; ch < parameters.nChannels; ch++)
            {
                if (state1.Acq.aveSlice)
                    nAve[ch] = nAve[ch] * state1.Acq.nAveSlice;
            }

            //internalFrameCounter++;

            //if (state1.Acq.photon_file_format && !read_photon_file)
            internalFrameCounter = frame_counter;

#if DEBUG
            if (DEBUGMODE >= 2)
                Debug.WriteLine("Debug: C# Frame Done Event FLIMProgressChanged 1. grabbing = " + grabbing + ", focusing = " + focusing + ", stopGrabActivated = " + stopGrabActivated);
#endif

            // Update GUI (throttled). We are in different thread from main window.
            // Always update at slice end to keep UI consistent.
            int nowTick = Environment.TickCount;
            bool shouldUpdate =
                unchecked(nowTick - _lastCountersUpdateTick) >= CountersUpdateIntervalMs ||
                internalFrameCounter == state1.Acq.nFrames;

            if (shouldUpdate)
            {
                _lastCountersUpdateTick = nowTick;
                flimage.InvokeIfRequired(o => o.UpdateCounters());
            }

            if (internalFrameCounter == state1.Acq.nFrames && !focusing && grabbing)
            {
                //Debug.WriteLine("Slice done!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                internalSliceCounter++;
                internalFrameCounter = 0;

                // Acquisition is finished for this frame set. If this is the final image (loop ends),
                // we will spend some time saving/flushing remaining pages before StopGrab() can run.
                // Indicate that we are busy saving (ABORT is misleading here).
                bool finishingAllImages =
                    internalSliceCounter == state1.Acq.nSlices &&
                    ((internalImageCounter + 1) >= state1.Acq.nImages || !allowLoop);
                if (finishingAllImages)
                {
                    flimage.GrabButton.InvokeIfRequired(o =>
                    {
                        o.Text = "SAVING";
                        o.Enabled = false;
                    });
                }

                if (!read_photon_file)
                {
                    StopDAQ(false, internalSliceCounter >= state1.Acq.nSlices); //close shutter only when slices are done.
                    DisposeDAQ();
                    ParkMirrors(true);
                    if (microscope_system == MicroscopeSystem.ThorLabBScopeGG && state1.Acq.zoom < 3) //This is required for Thorlab mirrors to settle....
                        System.Threading.Thread.Sleep(2500);
                }

                FiFo_StopMeas(true);
                //Save current status in FLIM_ImgData.
                if (internalSliceCounter == 1)
                    flimage.image_display.FLIM_ImgData.copyState(state1);

                if (saveTask != null && !saveTask.IsCompleted)
                    saveTask.Wait();

                int NPages = 0;

                if (flimage.image_display.FLIM_ImgData.KeepPagesInMemory || flimage.image_display.FLIM_ImgData.ZStack)
                {
                    if (flimage.image_display.FLIM_ImgData.FLIM_Pages5D[0] != null)
                        NPages = flimage.image_display.FLIM_ImgData.FLIM_Pages5D.Length;
                    deletedPageCounter = 0;
                }
                else
                {
                    NPages = FLIMSaveBuffer.Count;
                }

                while (NPages > savePageCounter - deletedPageCounter)
                {
                    if (flimage.image_display.FLIM_ImgData.KeepPagesInMemory || flimage.image_display.FLIM_ImgData.ZStack)
                    {
                        NPages = flimage.image_display.FLIM_ImgData.FLIM_Pages5D.Length;
                        deletedPageCounter = 0;
                    }
                    else
                        NPages = FLIMSaveBuffer.Count;

                    SaveFile(true, state1);

#if DEBUG
                    if (DEBUGMODE != 0)
                        Debug.WriteLine("Saving extra file.... " + (savePageCounterTotal), "/" + (savePageCounter));
#endif
                }

                FLIMSaveBuffer.Clear();

                if (!read_photon_file)
                {
                    flimage.InvokeIfRequired(o => o.UpdateMeasuredSliceInterval());
                }


                if (internalSliceCounter == state1.Acq.nSlices) //All slices done.
                {
                    if (state1.Acq.ZStack && state1.Acq.nSlices > 1 && !read_photon_file)
                        flimage.MoveBackToHome();

                    var current_filename = state1.Files.fullName();
                    state1.Files.fileCounter++; //This needs to be after MoveMotorBack, since movemotorbacktohome sends notification with the current fileCounter.
                    internalImageCounter++;

                    post_grabbing_process = true;

                    if (internalImageCounter >= state1.Acq.nImages || !allowLoop)
                    {
                        if (!read_photon_file)
                            StopGrab(true); //This will stop grabbing. We can actually activate this in looping....
                    }

                    if (!state1.Acq.photon_file_format || read_photon_file)
                    {
                        if (state1.Acq.fastZScan && !state1.Acq.ZStack)
                        {
                            //Setup display.
                            var FLIM5D = flimage.image_display.FLIM_ImgData.FLIMRaw5D;
                            flimage.image_display.FLIM_ImgData.clearPages4D(); //Clean the the 4D stack. it is separated from acquisition.
                            flimage.image_display.FLIM_ImgData.addToPageAndCalculate5D(FLIM5D, acquiredTime, true, true, 0, true);
                        }
                        else
                        {
                            if (flimage.image_display.FLIM_ImgData.KeepPagesInMemory)
                            {
                                for (int i = 0; i < flimage.image_display.FLIM_ImgData.FLIM_Pages5D.Length; i++)
                                {
                                    var FLIMImage = flimage.image_display.FLIM_ImgData.FLIM_Pages5D[i];
                                    var acqTimeTemp = flimage.image_display.FLIM_ImgData.acquiredTime_Pages5D[i];
                                    flimage.image_display.FLIM_ImgData.LoadFLIMData4D_Page_fromFLIMData5D(FLIMImage, i, acqTimeTemp, false);
                                    flimage.image_display.FLIM_ImgData.gotoPage(0);
                                }
                            }
                            else
                            {
                                flimage.image_display.OpenFLIM(current_filename, false, false, false);
                            }
                        }
                    }

                    if (!read_photon_file)
                    {
                        flimage.UpdateFileName();
                        flimage.image_display.FLIM_ImgData.fileUpdateRealtime(state1, false); //FileName update.
                        flimage.image_display.InvokeIfRequired(o => o.UpdateFileName());
                    }

                    try
                    {
                        //if (!state1.Acq.photon_file_format || read_photon_file)
                        {
                            flimage.image_display.ZStack = flimage.image_display.FLIM_ImgData.ZStack || state1.Acq.fastZScan;
                            flimage.image_display.displayZProjection = flimage.image_display.FLIM_ImgData.ZStack || state1.Acq.fastZScan;
                            if (flimage.image_display.FLIM_ImgData.ZStack)
                                flimage.image_display.calcZProjection();

                            flimage.image_display.UpdateImages(true, false, false, true);

                            if (snapShot)
                                SnapShotProcess();

                            try
                            {
                                PostImageUserFunction();
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("Problem in Post Open User Function:" + e.Message);
                            }
                        }
                    }
                    catch (Exception E)
                    {
                        Debug.WriteLine("Problem in displaying and /or analyzing saved image: " + E.Message);
                        //WriteStatusText("Problem in displaying and /or analyzing saved image");
                    }

                    grabbing = false; //grab is finished. (not necessary but just in case).

                    if (!State.Acq.photon_file_format)
                    {
                        post_grabbing_process = true;
                        EventNotify?.Invoke(this, new ProcessEventArgs("AcquisitionDone", null));
                        post_acquisition_analysis();
                        post_grabbing_process = false; //analysis is also finished.

                    }
                    else
                    {
                        EventNotify?.Invoke(this, new ProcessEventArgs("AcquisitionDone", null));
                        acquisition_done_event_activated = true;
                    }


                    //////////////
                    //We will wait for next imaging process, but from a different thread.
                    if (internalImageCounter < state1.Acq.nImages && allowLoop)
                    {
                        waitImage = Task.Factory.StartNew(() =>
                        {
                            lock (waitLoopImageTaskobj) //This task will never overlap. Don't use in any other lock.
                                LoopingImageAcq();
                        });
                    }
                    else
                    {
                        flimage.StopLoop();
                    }

                }
                else if (internalSliceCounter < state1.Acq.nSlices) // Slices still not done.
                {
                    if (state1.Acq.ZStack)
                    {
                        flimage.MoveMotorStep(true);
                        if (internalSliceCounter == state1.Acq.nSlices - 1)
                        {
                            if (flimage.motorCtrl != null)
                                flimage.motorCtrl.stack_Position = MotorCtrl.StackPosition.End;
                        }
                    }

                    EventNotify?.Invoke(this, new ProcessEventArgs("SliceAcquisitionDone", null));

                    if (!State.Acq.photon_file_format)
                    {
                        waitSlice = Task.Factory.StartNew(() =>
                        {
                            lock (waitSliceTaskobj) //This task will never overlap. Don't use in any other lock.
                                WaitForNextSlice();
                        });
                    }
                }
                else
                {
#if DEBUG
                    if (DEBUGMODE != 0)
                        Debug.WriteLine("Warning.... Slice counter: {0} > n Slices: {1}", internalSliceCounter, state1.Acq.nSlices);
#endif
                }

            }//internalFrame && !focusing

#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Debug: C# Frame Done Event FLIMProgressChanged 3. grabbing = " + grabbing + ", focusing = " + focusing + ", stopGrabActivated = " + stopGrabActivated);
#endif
            if (focusing || grabbing)
                flimage.BeginInvoke((Action)delegate
                {
                    flimage.UpdateAverageFrameCounter();
                });

#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Debug: C# Frame Done Event FLIMProgressChanged Done");
#endif
        }

        public void post_acquisition_analysis()
        {
            if (post_grabbing_process && flimage.analyzeAfterEachAcquisition && !read_photon_file)
                Task.Factory.StartNew(() =>
                {
                    flimage.image_display.InvokeIfRequired(o =>
                    {
                        o.plot_regular.Show();
                        o.plot_regular.Activate();

                        var filename1 = filename_last;

                        var error = o.OpenFLIM(filename1, true, o.plot_regular.calc_upon_open, false);
                    });
                });
        }


        public void PostImageUserFunction()
        {
            //if (flimage.drift_correction != null && flimage.drift_correction.Visible)
            //{
            //    flimage.drift_correction.calculateDriftXYZ();
            //}
        }


        public void UpdateImages(ScanParameters state1) //Can be slow. //Done with timer. //Thread safe.
        {
            bool updated = false;

            flimage.image_display.FLIM_ImgData.State.Uncaging.Position = (double[])state1.Uncaging.Position.Clone();
            displayPageCounter++;
            updated = !flimage.image_display.update_image_busy;
            if (updated)
            {
                flimage.image_display.update_image_busy = true;

                //It is always from different thread, since it comes from fifo.
                if (state1.Acq.fastZScan)
                {
                    flimage.image_display.BeginInvoke((Action)delegate
                    {
                        if (displayPageCounter == 1) //First page = 1. After ++.
                            flimage.image_display.SetFastZModeDisplay(state1.Acq.fastZScan); //To set the page at the center.
                        flimage.image_display.setupImageUpdateForZStack();
                    });
                }

                flimage.image_display.displayZProjection = false;
                flimage.image_display.FLIM_ImgData.currentPage = displayPageCounter - 1;

                try
                {
                    flimage.image_display.BeginInvoke((Action)delegate
                    {
                        flimage.image_display.UpdateImages(true, true, focusing, true);
                    });
                }
                catch
                {
                    flimage.image_display.update_image_busy = false;
                    throw;
                }

            }
            else
            {
#if DEBUG
                Debug.WriteLine("**** Busy ************" + displayPageCounter);
#endif

                displayPageCounter++;
                displayPageCounterTotal++;
            }

            // On file rollover, the imaging realtime plot historically resets.
            // For FiberPhotometry we want a continuous trace across the entire acquisition.
            if (displayPageCounter == state1.Acq.maxNFramePerFile &&
                displayPageCounterTotal < totalPagesSaved(state1) &&
                microscope_system != MicroscopeSystem.FiberPhotometry &&
                !(state1?.Acq?.fiberPhotometryMode ?? false))
            {
#if DEBUG
                Debug.WriteLine("Total page saved:" + totalPagesSaved(state1) + ", displayCounter = " + displayPageCounterTotal);
#endif
                flimage.image_display.realtimeData.Clear();
            }

#if DEBUG
            SW_PerformanceMonitor.Restart();
#endif
        }

        public void movePiezoToCenter()
        {
            if (State.Init.usePiezo && State.Init.motor_on)
            {
                var um = piezo.goto_center_um();
                flimage.motorCtrl.SetNewPosition_StepSize_um(new double[] { 0, 0, -um });
                flimage.SetMotorPosition(true, true, true);
            }
            else if (State.Init.usePiezo)
            {
                piezo.goto_center_um();
            }

        }

        public void RunSimulation()
        {
            if (FiFo_acquire == null)
            {
                FiFo_acquire = new FiFio_multiBoards(parameters);
            }

            if (!tcspc_on)
            {
                FiFo_acquire.FrameDone += new FiFio_multiBoards.FrameDoneHandler(FrameDoneEvent);
                FiFo_acquire.StripeDone += new FiFio_multiBoards.StripeDoneHandler(StripeDoneEvent);
                FiFo_acquire.MeasDone += new FiFio_multiBoards.MeasDoneHandler(MeasDoneEvent);
                tcspc_on = true;
            }

            grabbing = true;
            runningImgAcq = true;
            FiFo_acquire.RunSimulationData();
        }

        public void StopSimulation()
        {
            grabbing = false;
            runningImgAcq = false;
            FiFo_acquire.StopSimulation();
        }

    } //Class

    public enum MicroscopeSystem
    {
        ThorLabBScopeGG = 1,
        ThorLabBScopeRG = 2,
        SutterGG = 3,
        ScanImageGG = 4,
        MiniScope = 5,
        FiberPhotometry = 10,
    }

    public class ProcessEventArgs : EventArgs
    {
        public string EventName { get; internal set; }
        public object EventData { get; internal set; }
        public ProcessEventArgs(string name, object data)
        {
            this.EventName = name;
            this.EventData = data;
        }
    }

}
