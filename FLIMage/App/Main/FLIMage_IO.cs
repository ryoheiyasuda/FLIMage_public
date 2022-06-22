using FLIMage.Analysis;
using FLIMage.HardwareControls.StageControls;
using FLIMage.Uncaging;
using MathLibrary;
using MicroscopeHardwareLibs;
using MicroscopeHardwareLibs.Stage_Contoller;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool snapShot = false;
        public bool focusing = false;
        public bool grabbing = false;
        public bool refocusing = false;
        public bool post_grabbing_process = false;
        public bool looping = false;
        public bool allowLoop = true;
        public bool stopGrabActivated = false;
        public bool force_stop = false;
        public bool runningImgAcq = false;

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
        // int internalStripeCounter;
        public int savePageCounterTotal = 0;
        public int savePageCounter = 0;
        public int savePageBufferCounter = 0;
        public int displayPageCounterTotal = 0;
        public int displayPageCounter = 0;
        public int deletedPageCounter = 0;
        public double measuredSliceInterval;
        Task waitSlice, waitImage;
        Task saveTask;
        public bool save_image_busy = false;
        bool waitingImageTask = false;
        bool waitingSliceTask = false;

        ////// FLIM data stroage. All temporal. 
        List<UInt16[][][,,]> FLIMSaveBuffer = new List<ushort[][][,,]>(); //Used only when images are not saved in memory.
        List<DateTime> acquiredTimeList = new List<DateTime>(); //Store acquisition time info.
        DateTime acquiredTime;
        public FLIMData FLIM_ImgData;

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
        public HardwareControls.IOControls.ShutterCtrl resonant_on;
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
        public bool simulation_mode = false;

        //////Rate
        int badSyncRateCounter = 0;
        int badSyncRateMaxCount = 10;

        //////  Event handler for broadcasting.
        public event FLIMage_EventHandler EventNotify;
        public ProcessEventArgs envt = new ProcessEventArgs("", null);
        public delegate void FLIMage_EventHandler(FLIMage_IO flimage_io, ProcessEventArgs evnt);

        public FileIO fileIO;

        public FLIMage_IO(FLIMageMain FLIMage)
        {
            flimage = FLIMage;
            State = flimage.State;
            fileIO = new FileIO(State); //initialize fileIO

            use_nidaq = State.Init.NIDAQ_on;
            use_pq = State.Init.FLIM_on && (String.Equals(State.Init.FLIM_mode, "PQ") || String.Equals(State.Init.FLIM_mode, "MH"));
            use_bh = State.Init.FLIM_on && (String.Equals(State.Init.FLIM_mode, "BH"));
            tcspc_on = use_pq || use_bh;

            if (State.Init.MicroscopeSystem.ToLower().Contains("bscope") && State.Init.MicroscopeSystem.ToLower().Contains("gg"))
                microscope_system = MicroscopeSystem.ThorLabBScopeGG;

            if (State.Init.MicroscopeSystem.ToLower().Contains("bscope") && State.Init.MicroscopeSystem.ToLower().Contains("rg"))
                microscope_system = MicroscopeSystem.ThorLabBScopeRG;

            if (State.Init.MicroscopeSystem.ToLower().Contains("sutter"))
                microscope_system = MicroscopeSystem.SutterGG;

            if (State.Init.MicroscopeSystem.ToLower().Contains("mini"))
                microscope_system = MicroscopeSystem.MiniScope;

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
                            lineTriggeredSampleClock = new HardwareControls.IOControls.TriggeredLineClock(State);
                            Resonant_Mirror = new HardwareControls.IOControls.AnalogOutput(State, shading, true, false, true, false);
                            Resonant_Mirror.FrameDone += new HardwareControls.IOControls.AnalogOutput.FrameDoneHandler(mirrorAOFrameDoneEvent);
                            if (State.Init.EOM_nChannels > 0)
                                Resonant_EOM = new HardwareControls.IOControls.AnalogOutput(State, shading, false, true, true, false);

                            resonant_on = new HardwareControls.IOControls.ShutterCtrl(State);
                            

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
                    DialogResult dr = MessageBox.Show("Problem in loading NIDAQmx DLL: " + Setting_Status + ": " + ex.Message + "\n\nDo you want to turn off NIDAQ function?",
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

            tcspc_on = true; // use_bh || use_pq;
            TCSPC_Open();

            if (!(use_bh || use_pq))
            {
                State.Init.FLIM_on = false;
            }
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

            newFile = boolAllChannels(true);

            if (!focusing)
            {
                FLIMSaveBuffer.Clear();
                acquiredTimeList.Clear();
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
            DateTime at = DateTime.Now;
            if (AO_FrameCounter == 0)
            {
                double msPerLine = State.Acq.fastZScan ? State.Acq.FastZ_msPerLine : State.Acq.msPerLine;
                if (State.Acq.resonantScanning)
                    msPerLine = 500.0 / State.Init.resonantFreq_Hz;

                acquiredTime = at.AddMilliseconds(-msPerLine * State.Acq.linesPerFrame);
                State.Acq.triggerTime = acquiredTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");
#if DEBUG
                Debug.WriteLine("Time now at " + at.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                Debug.WriteLine("Triggered triggered at " + State.Acq.triggerTime);
#endif
                FLIM_ImgData.State.Acq.triggerTime = State.Acq.triggerTime;
            }

            AO_FrameCounter++;
            //EventNotify?.Invoke(this, new ProcessEventArgs("FrameScanDone", (object)AO_FrameCounter));

            if (AO_FrameCounter <= State.Acq.nFrames || focusing)
            {
                flimage.BeginInvoke((Action)delegate
                {
                    flimage.AO_FrameUpdate(AO_FrameCounter);
                });
            }
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
            if (focusing)
            {
                refocusing = true;
                StopFocus();
            }

            piezo.move_Piezo_1step_um(stepSize);

            if (refocusing)
            {
                StartGrab(true);
                refocusing = false;
            }
        }

        public void ResetFocus()
        {
            if (focusing && !refocusing)
            {
                Task.Factory.StartNew(() =>
                {
                    refocusing = true;
                    PauseFocus();
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
                flimage.RateTimerEvent_GUI_Update(badRate);
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
            if (tcspc_on & FiFo_acquire != null)
                FiFo_acquire.SetupParameters(focusing, parameters);
        }

        public void TCSPC_Open()
        {
            FiFio_multiBoards.ErrorCode error;
            if (!State.Init.FLIM_on)
            {
                FiFo_acquire = new FiFio_multiBoards(parameters);
                FiFo_acquire.startSimulationMode();
                error = FiFio_multiBoards.ErrorCode.NONE;
                simulation_mode = true;
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

                simulation_mode = error == FiFio_multiBoards.ErrorCode.NONE;
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

            //SetupFLIMParameters(); included in FiFo_acquire

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


                DialogResult dr = MessageBox.Show(error_Message + "\n\nDo you want to turn off FLIM function?",
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

        //
        public void StartDAQ(bool[] eraseSPCmemory, bool recordTriggerTime)
        {
            runningImgAcq = true;
            waitForAcquisitionTaskCompleted();

            if (stopGrabActivated)
                return;

            if (use_nidaq)
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
                        System.Threading.Thread.Sleep(100);
                    }
                    else
                    {
                        resonant_on.open();
                        Resonant_Mirror.setResonantZoom(State.Acq.zoom);
                    }

                    if (Resonant_EOM != null)
                    {
                        Resonant_EOM.putValue_S_ToStartPos(true, false);
                        Resonant_EOM.PutValueResonantEOM_LineClockTriggered(shading, State.Acq.resonantEOMDelay_us); //Testing fast EOM control.
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
                }
            }

            FiFo_StartNew(eraseSPCmemory, focusing, false);
            System.Threading.Thread.Sleep(1);

            if (use_nidaq)
            {
                shutterCtrl.open();
                System.Threading.Thread.Sleep(State.Init.mainShutterDelay); //shutter open.

                DateTime at = new DateTime();

                if (!State.Acq.externalTrigger || focusing) //Not allowing ext trigger yet.
                {
                    if (State.Acq.resonantScanning)
                    {

                        Resonant_EOM.Start();
                        Resonant_Mirror.Start();
                        lineTriggeredSampleClock.Start(); //Testing triggered clock.
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
                        Debug.WriteLine("Triggered..." + acquiredTime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                    }

                    if (recordTriggerTime)
                    {
                        acquiredTime = at;
                        State.Acq.triggerTime = at.ToString("yyyy-MM-ddTHH:mm:ss.fff");
                        FLIM_ImgData.State.Acq.triggerTime = State.Acq.triggerTime;
                    }
                }
                else
                {
                    acquiredTime = DateTime.Now;
                    State.Acq.triggerTime = acquiredTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");
                    FLIM_ImgData.State.Acq.triggerTime = State.Acq.triggerTime;
                }

            } //nidaq
            else
            {
                //Used for simulation mode
                acquiredTime = DateTime.Now;
                State.Acq.triggerTime = acquiredTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");
                FLIM_ImgData.State.Acq.triggerTime = State.Acq.triggerTime;
            }
        }

        /// <summary>
        /// Actual program for start grab.
        /// </summary>
        /// <param name="focus"></param>
        public void StartGrab(bool focus)
        {
            flimage.ImageDisplayOpen();
            flimage.GetParametersFromGUI(this); //Setup all parameters.

            if (State.Uncaging.uncage_whileImage && flimage.uncaging_panel != null && !focus)
                flimage.uncaging_panel.SetupUncage(this); //Invoke not necessary

            if (State.Acq.XOffset > State.Acq.XMaxVoltage || State.Acq.YOffset > State.Acq.YMaxVoltage)
            {
                MessageBox.Show("Offset exceeds maximum voltage!!");
                return;
            }

            flimage.SetupFLIMParameters(); //this will setup parameters for FLIM card..

            if (flimage.fastZcontrol != null)
            {
                flimage.fastZcontrol.InvokeIfRequired(o =>
                    {
                        o.ControlsDuringScanning(true); //Just enable the control.... Should be blocking.
                    });
            }

            FLIM_ImgData.InitializeData(State, true);
            fileIO = new FileIO(State);

            //Apply physiology data.
            if (flimage.physiology == null || flimage.physiology.IsDisposed)
                State.Ephys.Ephys_on = false;
            else
                State = fileIO.CopyPhysiologyParamToState(flimage.physiology.phys_parameters);

            flimage.image_display.InvokeIfRequired(o =>
            {
                o.SetupRealtimeImaging(o.flimage.flimage_io.State, o.flimage.flimage_io.FLIM_ImgData);
                o.SetFastZModeDisplay(o.flimage.flimage_io.State.Acq.fastZScan);
            });

            ParkMirrors(true);

            if (flimage.image_display.plot_realtime.Visible)
                flimage.image_display.plot_realtime.InvokeIfRequired(o => o.WarningTextDisplay("")); //will be invoked if necessary.

            if (focus)
            {
                focusing = true;
                grabbing = false;
                post_grabbing_process = false;

                EventNotify?.Invoke(this, new ProcessEventArgs("FocusStart", null));
            }
            else
            {
                if (!looping)
                {
                    if (CheckSavingParameters() < 0)
                    {
                        runningImgAcq = false;
                        grabbing = false;
                        focusing = false;
                        post_grabbing_process = false;
                        if (flimage != null)
                        {
                            flimage.InvokeIfRequired(o => o.StopGrab_GUI_Update());
                        }
                        return;
                    }
                }

                grabbing = true;
                post_grabbing_process = false;
                focusing = false;
                EventNotify?.Invoke(this, new ProcessEventArgs("GrabStart", null));
            }

            runningImgAcq = true;
            flimage.InvokeIfRequired(o => o.StarGrab_GUI_Update(focus)); //need blocking.
            force_stop = false;

            InitializeCounter(); //Counters Reset. SaveBuffer Clear. Acquired time buffer clear.

            flimage.image_display.InitializeStripeBuffer(State.Acq.nChannels, State.Acq.linesPerFrame, State.Acq.pixelsPerLine);

            if (!focusing) //Setup FLIM_ImgData mode. ZStack? FastZ?
            {
                FLIM_ImgData.clearMemory();
                FLIM_ImgData.ZStack = (State.Acq.ZStack && State.Acq.nSlices > 1 && State.Acq.sliceStep > 0.0);
                FLIM_ImgData.nFastZ = State.Acq.fastZScan ? State.Acq.FastZ_nSlices : 1;
            }

            System.Threading.Thread.Sleep(10);

            bool[] eraseMemoryA = boolAllChannels(true);

            if (!focus)
            {
                //UIstopWatch.Reset();
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

                StartDAQ(eraseMemoryA, true); //Including trigger.

                if (internalImageCounter == 0)
                {
                    UIstopWatch_Loop.Start();
                }
                UIstopWatch_Image.Start();

            }
            else
            {
                StartDAQ(eraseMemoryA, false); //Including trigger.
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
            StartDAQ(eraseMemoryA, false); //Including trigger.
        }

        public void StopNIDAQIOControls()
        {
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
        }

        /// <summary>
        /// Temporarily pausing DAQ.
        /// </summary>
        //public void PauseDAQ()
        //{
        //    if (use_nidaq)
        //    {
        //        //shutterCtrl.Close();

        //        if (State.Acq.resonantScanning)
        //        {
        //            if (thorECU != null)
        //                thorECU.EnableScanner(false);

        //            if (lineTriggeredSampleClock != null)
        //                lineTriggeredSampleClock.Stop();

        //            if (Resonant_EOM != null)
        //                Resonant_EOM.Stop();

        //            if (resonant_on != null)
        //                resonant_on.Close();

        //            if (Resonant_Mirror != null)
        //            {                        
        //                Resonant_Mirror.Stop();
        //                Resonant_Mirror.putValue_Single(new double[] { 0, 0 }, false, false);
        //            }

        //            if (Resonant_EOM != null)
        //            {
        //                Resonant_EOM.Stop();
        //                Resonant_EOM.putValue_S_ToStartPos(true, false);
        //            }

        //            if (AO_Mirror_EOM != null)
        //            {
        //                AO_Mirror_EOM.Stop(); //Resonant EOM.
        //                AO_Mirror_EOM.putValue_S_ToStartPos(true, false);
        //            }
        //        }
        //        else
        //        {
        //            if (!State.Init.use_digitalLineClock && lineClock != null)
        //                lineClock.Stop();

        //            if (miniScopeClock != null)
        //                miniScopeClock.Stop();

        //            if (digitalOutput_WClock != null)
        //                digitalOutput_WClock.Stop();

        //            if (AO_Mirror_EOM != null)
        //                AO_Mirror_EOM.Stop();
        //        }
        //    }
        //    runningImgAcq = false;
        //}

        /// <summary>
        /// Stop NIDAQ boards used for grabbing.
        /// Dupe with Pause????
        /// </summary>
        public void StopDAQ(bool temporal)
        {
            if (use_nidaq)
            {
                shutterCtrl.Close();

                if (State.Acq.resonantScanning)
                {
                    if (thorECU != null)
                        thorECU.EnableScanner(false);

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
            StopDAQ(true);
        }

        public void StopFocus()
        {
            StopGrab(true, true);
        }

        public void StopGrab(bool force)
        {
            StopGrab(force, false);
        }

        public void StopGrab(bool force, bool focusStop)
        {
            force_stop = force;

            if (focusStop)
                stopGrabActivated = false;

            FiFo_StopMeas(force);

            StopDAQ(false);
            DisposeDAQ();

            ParkMirrors(true);
            runningImgAcq = false;

            if (!looping || stopGrabActivated)
            {
                UIstopWatch_Loop.Stop();
                UIstopWatch_Image.Stop();
            }

            SW_PerformanceMonitor.Stop();

            if (focusStop)
            {
                flimage.InvokeIfRequired(o => o.StopFocus_GUI_Update()); //Should be done before (focusing = false).
                focusing = false;
            }
            else
            {
                flimage.InvokeIfRequired(o => o.StopGrab_GUI_Update()); //Should be done before (focusing = false).
                grabbing = false;

                if (looping)
                {
                    flimage.GrabButton.InvokeIfRequired(o => o.Text = "STOP");
                    flimage.GrabButton.InvokeIfRequired(o => o.Enabled = false);
                }
            }
        }

        /// <summary>
        /// When stopping, mirrors need to be parked.
        /// </summary>
        public void ParkMirrors(bool zeroEOM)
        {
            bool zeroEOM1 = State.Acq.flyBackBlancking && zeroEOM;
            if (use_nidaq)
            {
                bool shutterAO = flimage.uncaging_panel != null && State.Init.AO_uncagingShutter && flimage.uncaging_panel.UncagingShutter;
                if (State.Init.enableRegularGalvo)
                    AO_Mirror_EOM.putValue_S_ToStartPos(zeroEOM1, shutterAO);

                if (State.Init.enableResonantScanner)
                {
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

            int standard_waitTime = 40;  //With this cycle, we can see if stopGrab is activated. 

            bool uncaging_slice = State.Uncaging.sync_withSlice && State.Uncaging.uncage_whileImage;
            bool DO_slice = State.DO.sync_withSlice && State.DO.DO_whileImage;

            double overHead_ms = 40; //each uncaging or DO output requires some overhead time.

            if (uncaging_slice || DO_slice) //Uncaging protocol!!
            {
                while (waitingSliceTask && !State.Acq.ZStack) //Cycle roughly every standard_waitTime.
                {
                    double eTime2 = UIstopWatch_Image.ElapsedMilliseconds;

                    double sampleLength_ms = State.Uncaging.sampleLength + overHead_ms;

                    if (DO_slice)
                        sampleLength_ms = State.DO.sampleLength + overHead_ms;

                    if (uncaging_slice && DO_slice)
                        sampleLength_ms = State.DO.sampleLength + State.Uncaging.sampleLength + 2 * overHead_ms;


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

                bool fire_uncaging_cond = uncaging_DO_SliceCounter >= State.Uncaging.SlicesBeforeUncage && uncaging_slice;
                bool fire_DO_cond = uncaging_DO_SliceCounter >= State.DO.SlicesBeforeDO && DO_slice;
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

                    if (FLIM_ImgData.ZStack) //for ZStack, we willl 
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

            eTime = UIstopWatch_Image.ElapsedMilliseconds;
            measuredSliceInterval = eTime / 1000.0 / internalSliceCounter;

            bool[] eraseMemoryA = boolAllChannels(!State.Acq.aveSlice || averageSliceCounter == 0);

            AO_FrameCounter = 0;
            waitingSliceTask = false; //Now it finished its work.

#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Before Start DAQ in wait slice task"); //Too let us know where we are.
#endif

            if (!stopGrabActivated)
            {
                ParkMirrors(true);
                StartDAQ(eraseMemoryA, false);
                EventNotify?.Invoke(this, new ProcessEventArgs("SliceAcquisitionStart", null));
            }

#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Finished wait slice task");
#endif

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

        ///////////////////////////////////Measurement Done Handle//////////////////////////////////////
        public void MeasDoneEvent(FiFio_multiBoards fifo, EventArgs e)
        {
            //Debug.WriteLine("Measurement Done!");
            if (fifo.saturated)
            {
                if (focusing)
                    StopFocus();
                if (grabbing)
                    StopGrab(true);

                MessageBox.Show("FiFo saturated!!");
            }

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

            if (fifo.Running)
            {
                UInt16[][][,,] FLIMTemp = e.data; //order = c, z, [y,x,t]

                FLIM_ImgData.LoadFLIMdata5D_Realtime(FLIMTemp, acquiredTime, false); //Save in FLIM_ImgData class. ShallowCopy       
                FLIM_ImgData.MakeFLIM_Pages4DFromFLIMRaw5D(false);

                if (State.Acq.fastZScan)
                {
                    flimage.image_display.calcZProjection(); //For realtime, it just displays one image.
                }
                else
                {
                    FLIM_ImgData.LoadFLIMRawFromData4D(FLIM_ImgData.FLIM_Pages[0], acquiredTime, false); //ShallowCopy
                }
            }

            //if (tcspc_on)
            //{
            if (runningImgAcq && fifo.Running)
            {
                bool[] savebool = new bool[State.Acq.nChannels];  //boolAllChannels(false);
                bool protecting_save_task = false;

                bool updateImage = true;

                if (focusing)
                {
                    updateImage = false;
                    FLIM_ImgData.nAveragedFrame = Enumerable.Repeat(1, State.Acq.nChannels).ToArray();

                    if (State.Acq.nAveFrame_focus > 1)
                    {
                        averageCounter++;
                        FLIM_ImgData.nAveragedFrame = GetAverageFrame(averageCounter);

                        if (State.Acq.nAveFrame_focus == averageCounter) //Finished averaging 1 slice
                        {
                            averageCounter = 0;
                            updateImage = true;
                        }
                    }
                    else
                        updateImage = true;
                }
                else if (grabbing)
                {
                    for (int i = 0; i < State.Acq.nChannels; i++)
                        savebool[i] = (!(State.Acq.aveFrameA[i] && State.Acq.nAveFrame > 1)) && (State.Acq.acquisition[i]);

                    updateImage = false;
                    if (State.Acq.aveFrameA.Any(x => x == true) && State.Acq.nAveFrame > 1)
                    {
                        averageCounter++;

                        if (averageCounter % State.Acq.nAveFrame_focus == 0)
                            updateImage = true;

                        if (!State.Acq.aveSlice)
                        {
                            FLIM_ImgData.nAveragedFrame = GetAverageFrame(averageCounter);
                        }
                        else
                            FLIM_ImgData.nAveragedFrame = Enumerable.Repeat(averageCounter + State.Acq.nAveFrame * averageSliceCounter, State.Acq.nChannels).ToArray();

                        if (State.Acq.nAveFrame == averageCounter) //Finished averaging 1 slice
                        {
                            averageCounter = 0;
                            averageSliceCounter++;

                            protecting_save_task = true;
                            updateImage = true;
                            //When you finish 1 slice, you should wait for saving task.

                            //Debug.WriteLine("AverageSliceCounter :" + averageSliceCounter + "   n_Slice: " + State.Acq.nSlices);

                            if (!State.Acq.aveSlice)
                            {
                                for (int i = 0; i < State.Acq.nChannels; i++)
                                {
                                    if (State.Acq.aveFrameA[i] && State.Acq.acquisition[i])
                                        savebool[i] = true;
                                }

                            }
                            else // aveSlice
                            {
                                if (averageSliceCounter == State.Acq.nAveSlice)
                                {
                                    averageSliceCounter = 0;
                                    for (int i = 0; i < State.Acq.nChannels; i++)
                                    {
                                        if (State.Acq.aveFrameA[i] && State.Acq.acquisition[i])
                                            savebool[i] = true;
                                    }
                                }
                            } // aveSlice
                        } //Frame
                    } //Average
                    else
                    {
                        updateImage = true;
                    }
                } //if focus.

#if DEBUG
                if (force_stop || stopGrabActivated)
                    Debug.WriteLine("Force stop activated");
                Debug.WriteLine("Debug: C# Frame Done process 1");
#endif
                if (grabbing || focusing)
                {
                    FLIM_ImgData.saveChannels = (bool[])savebool.Clone();
                    SaveUpdateAcquiredImage(savebool, protecting_save_task, updateImage);
                }

                if (grabbing || focusing)
                    FLIMProgressChanged();
                else
                    Debug.WriteLine("No progress");

            } //Running
            //} //if (tcspc_on)

#if DEBUG
            if (DEBUGMODE != 0)
            {
                SW_PerformanceMonitorFrame.Stop();
                Debug.WriteLine("Debug: C# Time to acquire frame: " + SW_PerformanceMonitorFrame.ElapsedMilliseconds + " ms");
            }
#endif

        } //FrameDoneEvent_Core.

        public int[] GetAverageFrame(int nAverage)
        {
            int[] aveN = new int[State.Acq.nChannels];
            for (int c = 0; c < State.Acq.nChannels; c++)
                if (State.Acq.aveFrameA[c])
                    aveN[c] = nAverage;
                else
                    aveN[c] = 1;

            return aveN;
        }

        public int[] Get_n_time()
        {
            int[] n_time = Enumerable.Repeat<int>(State.Spc.spcData.n_dataPoint, State.Acq.nChannels).ToArray();
            for (int i = 0; i < State.Acq.nChannels; i++)
            {
                if (!State.Acq.acqFLIMA[i])
                    n_time[i] = 1;
            }

            return n_time;
        }


        /// <summary>
        /// SaveUpdateAcquiredImage: Called by FrameDoneEvent_Core. Control saving and displaying acquired data.
        /// </summary>
        /// <param name="saveFileBools"></param>
        /// <param name="protecting_save_task"></param>
        public void SaveUpdateAcquiredImage(bool[] saveFileBools, bool protecting_save_task, bool updateImage)
        {
            DateTime acTime;
            double msPerLine = State.Acq.fastZScan ? State.Acq.FastZ_msPerLine : State.Acq.msPerLine;
            if (State.Acq.resonantScanning)
                msPerLine = 500.0 / State.Init.resonantFreq_Hz;

#if DEBUG
            Debug.WriteLine("Debug: C# SaveUpdateAcquiredImage process 1");
#endif

            //Deepcopy of FLIMRaw5D.
            if (saveFileBools.Any(x => x == true)) //If any savefile exists.
            {
                acTime = acquiredTime.AddMilliseconds(internalFrameCounter * msPerLine * State.Acq.linesPerFrame);

                var FLIMForSave = (ushort[][][,,])FLIM_ImgData.FLIMRaw5D.Clone();  //Shallow copy.
                //Copier.DeepCopyArray(FLIM_ImgData.FLIMRaw5D);

                FLIM_ImgData.saveChannels = saveFileBools;

                for (int i = 0; i < saveFileBools.Length; i++)
                {
                    if (!saveFileBools[i])
                    {
                        FLIMForSave[i] = null;
                    }
                }

                lock (saveBufferObj)
                    if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
                    {
                        FLIM_ImgData.Add5DFLIM(FLIMForSave, acTime, savePageBufferCounter, false);
                        savePageBufferCounter++;
                    }
                    else
                    {
                        FLIMSaveBuffer.Add(FLIMForSave);
                        acquiredTimeList.Add(acTime);
                    }
            }

            bool busy = AO_FrameCounter - internalFrameCounter > 3;

#if DEBUG
            Debug.WriteLine("Debug: C# SaveUpdateAcquiredImage process 2");
#endif

            if ((focusing || grabbing) && updateImage)
                UpdateImages();

#if DEBUG
            Debug.WriteLine("Debug: C# SaveUpdateAcquiredImage process 3");
#endif


            if (grabbing && saveFileBools.Any(x => x == true))
            {
                SaveFile(protecting_save_task);
            }
        }

        /// <summary>
        /// If not keeping the data in memory, the stack is removed after saving. For ZStack, everything is saved in memory.
        /// </summary>
        public void CheckDeletePageBuffer()
        {
            lock (saveBufferObj)
            {
                if (!(FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack))
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
        }

        public void RemoveFrameAt(int frameToWork) //Clear memory very well.
        {
            if (FLIMSaveBuffer != null && frameToWork < FLIMSaveBuffer.Count)
                FLIMSaveBuffer.RemoveAt(frameToWork);
        }

        public int totalPagesSaved()
        {
            int aveNFrame = 1;
            if (State.Acq.aveFrameA.Any(x => x == true))
                aveNFrame = State.Acq.nAveFrame;

            int TotalNPages = State.Acq.nFrames / aveNFrame;
            int aveNslices = 1;
            if (State.Acq.aveSlice)
                aveNslices = State.Acq.nAveSlice;

            TotalNPages = TotalNPages * State.Acq.nSlices / aveNslices;

            //Debug.WriteLine("Total pages saved = " + TotalNPages);

            return TotalNPages;
        }

        public void SaveFile(bool protecting_save_task)
        {
#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Start Save File");
#endif
            bool updated = false;
            int savePage = savePageCounter - deletedPageCounter;
            //int safeMergin = 0;

            //If previous saveTask is still running, it will wait until it is done.
            if (saveTask != null && !saveTask.IsCompleted)
                saveTask.Wait();

            if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
            {
                //Save task occurs only when savePage Counter is less than 5D page.
                updated = (FLIM_ImgData.FLIM_Pages5D.Length > savePageCounter);
                if (!updated)
                {
                    Debug.WriteLine("**** FLIM_Page5D = {0}, savePageCounter = {1} ****", FLIM_ImgData.FLIM_Pages5D.Length, savePageCounter);
                }
            }
            else
            {
                updated = (FLIMSaveBuffer.Count > savePage && savePage >= 0);
            }


            if (updated && (saveTask == null || saveTask.IsCompleted))
            {
                if (protecting_save_task)
                    SaveTask(savePage);
                else
                    saveTask = Task.Factory.StartNew((object obj) =>
                    {
                        var data = (dynamic)obj;
                        SaveTask(data.page);
                    }, new { page = savePage });

            }
            else
            {
                Debug.WriteLine("Saving BUSY***************************Could not save:" + (savePageCounterTotal + 1) + " (" + (savePageCounter + 1) + "/" + FLIM_ImgData.FLIM_Pages5D.Count() + ")");
            }

            flimage.InvokeAnyway(o => o.UpdateSavedNumberOfFile());
        }

        /// <summary>
        /// Save acquired image. Called in different thread (Task.Factory.StartNew). 
        /// </summary>
        /// <param name="savePage"></param>
        public void SaveTask(int savePage)
        {
#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Start Save Task");
#endif

            lock (syncFLIMsave)
            {
#if DEBUG
                if (DEBUGMODE != 0)
                    Debug.WriteLine("Start FLIMsave sync");
#endif

                UInt16[][][,,] FLIMImage; //Before permutation.
                DateTime acqTimeTemp;

                ushort[][][,,] FLIM_5D; //After permutation.

                ////Overwrite and savefile setting
                bool[] overwrite = (bool[])newFile.Clone();

                int error = 0;
                String fullFileName = State.Files.fullName();

                bool[] saveCh = boolAllChannels(true);

                //Everything about FLIM_Page5D or FLIMSaveBufer. 
#if DEBUG
                if (DEBUGMODE != 0)
                    Debug.WriteLine("Start FLIMmovie sync");
#endif

                if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
                {
                    FLIMImage = FLIM_ImgData.FLIM_Pages5D[savePageCounter];
                    acqTimeTemp = FLIM_ImgData.acquiredTime_Pages5D[savePageCounter];
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
                    Debug.WriteLine("Start Saving File.");
#endif

                for (int ch = 0; ch < State.Acq.nChannels; ch++)
                {
                    if (FLIMImage[ch] == null)
                        saveCh[ch] = false;
                }

                State.Acq.acqFLIM = false;
                for (int ch = 0; ch < State.Acq.nChannels; ch++)
                    State.Acq.acqFLIM = (State.Acq.acqFLIM || (State.Acq.acqFLIMA[ch] && saveCh[ch]));

                if (!State.Acq.acqFLIM)
                {
                    flimage.saveIntensityImage = true;
                }
                else
                {
                    if (!State.Files.channelsInSeparatedFile)
                    {
                        bool overwrite1 = overwrite[0];
                        newFile = boolAllChannels(false);

                        if (FLIM_5D.Length == 1)
                            error = fileIO.SaveFLIMInTiff(fullFileName, FLIM_5D[0], acqTimeTemp, overwrite1, saveCh);
                        else
                            error = fileIO.SaveFLIMInTiffZStack(fullFileName, FLIM_5D, acqTimeTemp, overwrite1, saveCh);
                    }
                    else //Separate channel.
                    {
                        for (int ch = 0; ch < State.Acq.nChannels; ch++)
                        {
                            if (FLIMImage[ch] != null)
                            {
                                saveCh = new bool[State.Acq.nChannels];
                                saveCh[ch] = true;
                                newFile[ch] = false;

                                bool overwrite1 = overwrite[ch];
                                String fileName = State.Files.fullName(ch);
                                if (FLIM_5D.Length == 1)
                                {
                                    ushort[][,,] FLIM_separated = new ushort[State.Acq.nChannels][,,];
                                    FLIM_separated[ch] = FLIM_5D[0][ch];
                                    error = fileIO.SaveFLIMInTiff(fileName, FLIM_separated, acqTimeTemp, overwrite1, saveCh);

#if DEBUG
                                    if (DEBUGMODE != 0)
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
                                    error = fileIO.SaveFLIMInTiffZStack(fileName, FLIM_separated, acqTimeTemp, overwrite1, saveCh);

#if DEBUG
                                    if (DEBUGMODE != 0)
                                        Debug.WriteLine("Saving file.. {0}, channel = {1}, overwrite = {2}, n_zslice = ", fileName, ch, overwrite1, FLIM_5D.Length);
#endif
                                }
                            }
                        }
                    }
                }

                if (flimage.saveIntensityImage)
                {
                    for (int c = 0; c < State.Acq.nChannels; c++)
                    {
                        if (FLIMImage[c] != null)
                        {
                            if (State.Files.channelsInSeparatedFile)
                            {
                                saveCh = new bool[State.Acq.nChannels];
                                saveCh[c] = true;
                            }

                            var file_type = FileIO.ImageType.FLIMRaw;
                            if (State.Acq.acqFLIM)
                                file_type = FileIO.ImageType.Intensity;

                            String fileName = fileIO.FLIM_FilePath(c, State.Files.channelsInSeparatedFile, State.Files.fileCounter, file_type, "", State.Files.pathName);

                            for (int z = 0; z < FLIM_5D.Length; z++)
                            {
                                bool overwrite1 = (overwrite[c] && (z == 0)) && (State.Files.channelsInSeparatedFile || c == 0);
                                fileIO.Save2DImageInTiff(fileName, ImageProcessing.GetProjectFromFLIM(FLIM_5D[z][c], FLIM_ImgData.fit_range[c]), acqTimeTemp, overwrite1, saveCh);
                            }
                        }
                    }
                }

                if (overwrite.Any(x => x == true))
                    FLIM_ImgData.image_description = fileIO.image_description;


                savePageCounter++;
                savePageCounterTotal++;

                int TotalNPages = totalPagesSaved();

                if (TotalNPages == savePageCounter)
                {
                    EventNotify?.Invoke(this, new ProcessEventArgs("SaveImageDone", FLIMImage));
                }

                //if (parameters.enableFastZscan)  //always in different files. to avoid confusion.
                //{
                //    newFile = true;
                //    State.Files.fileCounter++;
                //    UpdateFileName();
                //}

                if (savePageCounter == State.Acq.maxNFramePerFile && savePageCounterTotal < TotalNPages)
                {
                    //Finished saving. Reaching maximum page number. 

                    newFile = boolAllChannels(true);
                    State.Files.fileCounter++;
                    flimage.UpdateFileName();

                    if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
                    {
                        lock (saveBufferObj)
                        {
                            FLIM_ImgData.RemovePageRange5D(0, State.Acq.maxNFramePerFile);
                            displayPageCounter = displayPageCounter - State.Acq.maxNFramePerFile;

                            savePageCounter = 0;

                            if (displayPageCounter < 0)
                                displayPageCounter = 0;
                        }

                    }
                    else
                    {
                        savePageCounter = 0;
                        deletedPageCounter = deletedPageCounter - State.Acq.maxNFramePerFile;
                        displayPageCounter = displayPageCounter - State.Acq.maxNFramePerFile;

                        if (displayPageCounter < 0)
                            displayPageCounter = 0;

                        if (deletedPageCounter < -1)
                        {
                            // Should be -1. It will be added at CheckDeletePageBuffer();
                            Debug.WriteLine("Deleted Counter < 0 problem!!" + deletedPageCounter + ", Disp = " + displayPageCounter);
                        }

                    }
                }
                // }


                //Debug.WriteLine("PageCount = " + FLIM_ImgData.FLIM_Pages5D.Count + ", save Count" + savePageCounter);

                CheckDeletePageBuffer();

                //EventNotify?.Invoke(this, new ProcessEventArgs("FrameAcquisitionDone", null));

            } //Sync

#if DEBUG
            if (DEBUGMODE != 0)
                Debug.WriteLine("Ended Save Task");
#endif
        }

        /// <summary>
        /// CheckSavingParameters: creating directory for saving etc before starting grab.
        /// </summary>
        /// <returns></returns>
        public int CheckSavingParameters()
        {
            bool[] saveChannels = Enumerable.Repeat<bool>(true, FLIM_ImgData.nChannels).ToArray();
            fileIO.CreateHeader(saveChannels);
            System.IO.Directory.CreateDirectory(State.Files.pathName);
            System.IO.Directory.CreateDirectory(State.Files.pathNameIntensity);
            System.IO.Directory.CreateDirectory(State.Files.pathNameFLIM);
            if (System.IO.File.Exists(State.Files.fullName()))
            {
                DialogResult dr = MessageBox.Show("File already exist! Do you want to overwrite?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                    return (0);
                else
                    return (-1);

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
        public void FLIMProgressChanged()
        {
            int[] nAve = GetAverageFrame(State.Acq.nAveFrame);
            for (int ch = 0; ch < State.Acq.nChannels; ch++)
            {
                if (State.Acq.aveSlice)
                    nAve[ch] = nAve[ch] * State.Acq.nAveSlice;
            }

            internalFrameCounter++;

#if DEBUG
            if (DEBUGMODE >= 2)
                Debug.WriteLine("Debug: C# Frame Done Event FLIMProgressChanged 1. grabbing = " + grabbing + ", focusing = " + focusing + ", stopGrabActivated = " + stopGrabActivated);
#endif

            //Update GUI. We are in different thread from main window. So, we will evoke it.
            flimage.InvokeIfRequired(o => o.UpdateCounters());

            if (internalFrameCounter == State.Acq.nFrames && !focusing && grabbing)
            {
                //Debug.WriteLine("Slice done!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                internalSliceCounter++;
                internalFrameCounter = 0;


                StopDAQ(false); //close shutter included.
                DisposeDAQ();
                ParkMirrors(true);
                if (microscope_system == MicroscopeSystem.ThorLabBScopeGG && State.Acq.zoom < 3) //This is required for Thorlab mirrors to settle....
                    System.Threading.Thread.Sleep(2500);

                FiFo_StopMeas(true);
                //Save current status in FLIM_ImgData.
                if (internalSliceCounter == 1)
                    FLIM_ImgData.copyState(State);

                if (saveTask != null && !saveTask.IsCompleted)
                    saveTask.Wait();

                int NPages = 0;

                if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
                {
                    if (FLIM_ImgData.FLIM_Pages5D[0] != null)
                        NPages = FLIM_ImgData.FLIM_Pages5D.Length;
                    deletedPageCounter = 0;
                }
                else
                {
                    NPages = FLIMSaveBuffer.Count;
                }


                while (NPages > savePageCounter - deletedPageCounter)
                {
                    if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
                    {
                        NPages = FLIM_ImgData.FLIM_Pages5D.Length;
                        deletedPageCounter = 0;
                    }
                    else
                        NPages = FLIMSaveBuffer.Count;

                    SaveFile(true);

#if DEBUG
                    if (DEBUGMODE != 0)
                        Debug.WriteLine("Saving extra file.... " + (savePageCounterTotal), "/" + (savePageCounter));
#endif
                }

                FLIMSaveBuffer.Clear();

                flimage.InvokeIfRequired(o => o.UpdateMeasuredSliceInterval());

                if (internalSliceCounter == State.Acq.nSlices) //All slices done.
                {
                    if (State.Acq.ZStack && State.Acq.nSlices > 1)
                        flimage.MoveBackToHome();

                    State.Files.fileCounter++; //This needs to be after MoveMotorBack, since movemotorbacktohome sends notification with the current fileCounter.
                    internalImageCounter++;

                    post_grabbing_process = true;

                    if (internalImageCounter >= State.Acq.nImages || !allowLoop)
                    {
                        StopGrab(true); //This will stop grabbing. We can actually activate this in looping....
                    }

                    if (State.Acq.fastZScan && !State.Acq.ZStack)
                    {
                        //Setup display.
                        var FLIM5D = FLIM_ImgData.FLIMRaw5D;
                        FLIM_ImgData.clearPages4D(); //Clean the the 4D stack. it is separated from acquisition.
                        FLIM_ImgData.addToPageAndCalculate5D(FLIM5D, acquiredTime, true, true, 0, true);
                    }
                    else
                    {
                        for (int i = 0; i < FLIM_ImgData.FLIM_Pages5D.Length; i++)
                        {
                            var FLIMImage = FLIM_ImgData.FLIM_Pages5D[i];
                            var acqTimeTemp = FLIM_ImgData.acquiredTime_Pages5D[i];
                            FLIM_ImgData.LoadFLIMData4D_Page_fromFLIMData5D(FLIMImage, i, acqTimeTemp, false);
                            FLIM_ImgData.gotoPage(0);
                        }
                    }


                    try
                    {
                        var image_display = flimage.image_display;
                        flimage.UpdateFileName();
                        image_display.ZStack = FLIM_ImgData.ZStack || State.Acq.fastZScan;
                        image_display.displayZProjection = FLIM_ImgData.ZStack || State.Acq.fastZScan;
                        if (FLIM_ImgData.ZStack)
                            flimage.image_display.calcZProjection();

                        //FLIM_ImgData.n_pages = savePageCounter;
                        image_display.UpdateImages(true, false, false, true);

                        if (snapShot)
                            SnapShotProcess();

                        //if (!State.Acq.acqFLIMA)
                        //    image_display.calculateTimecourse();

                        FLIM_ImgData.fileUpdateRealtime(State, false); //FileName update.
                        image_display.InvokeIfRequired(o => o.UpdateFileName());

                        if (flimage.analyzeAfterEachAcquisition && image_display != null)
                        {
                            image_display.InvokeIfRequired(o =>
                            {
                                o.plot_regular.Show();
                                o.plot_regular.Activate();
                                o.OpenFLIM(FLIM_ImgData.State.Files.fullName(), true, o.plot_regular.calc_upon_open, false);
                            });
                        }

                        try
                        {
                            PostImageUserFunction();
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Problem in Post Open User Function:" + e.Message);
                        }
                    }
                    catch (Exception E)
                    {
                        Debug.WriteLine("Problem in displaying and /or analyzing saved image: " + E.Message);
                        //WriteStatusText("Problem in displaying and /or analyzing saved image");
                    }

                    EventNotify?.Invoke(this, new ProcessEventArgs("AcquisitionDone", null));

                    grabbing = false; //grab is finished. (not necessary but just in case).
                    post_grabbing_process = false; //analysis is also finished.

                    //////////////
                    //We will wait for next imaging process, but from a different thread.
                    if (internalImageCounter < State.Acq.nImages && allowLoop)
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
                else if (internalSliceCounter < State.Acq.nSlices) // Slices still not done.
                {
                    if (State.Acq.ZStack)
                    {
                        flimage.MoveMotorStep(true);
                        if (internalSliceCounter == State.Acq.nSlices - 1)
                        {
                            if (flimage.motorCtrl != null)
                                flimage.motorCtrl.stack_Position = MotorCtrl.StackPosition.End;
                        }
                    }

                    EventNotify?.Invoke(this, new ProcessEventArgs("SliceAcquisitionDone", null));

                    waitSlice = Task.Factory.StartNew(() =>
                    {
                        lock (waitSliceTaskobj) //This task will never overlap. Don't use in any other lock.
                            WaitForNextSlice();
                    });
                }
                else
                {
#if DEBUG
                    if (DEBUGMODE != 0)
                        Debug.WriteLine("Warning.... Slice counter: {0} > n Slices: {1}", internalSliceCounter, State.Acq.nSlices);
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

        public void PostImageUserFunction()
        {
            //if (flimage.drift_correction != null && flimage.drift_correction.Visible)
            //{
            //    flimage.drift_correction.calculateDriftXYZ();
            //}
        }


        public void UpdateImages() //Can be slow. //Done with timer. //Thread safe.
        {
            bool updated = false;

            FLIM_ImgData.State.Uncaging.Position = (double[])State.Uncaging.Position.Clone();
            displayPageCounter++;
            updated = !flimage.image_display.update_image_busy;
            if (updated)
            {
                //It is always from different thread, since it comes from fifo.
                if (State.Acq.fastZScan)
                {
                    flimage.image_display.BeginInvoke((Action)delegate
                    {
                        if (displayPageCounter == 1) //First page = 1. After ++.
                            flimage.image_display.SetFastZModeDisplay(State.Acq.fastZScan); //To set the page at the center.
                        flimage.image_display.setupImageUpdateForZStack();
                    });
                }

                flimage.image_display.displayZProjection = false;
                FLIM_ImgData.currentPage = displayPageCounter - 1;
                flimage.image_display.UpdateImages(true, true, focusing, true);

            }
            else
            {
#if DEBUG
                Debug.WriteLine("**** Busy ************" + displayPageCounter);
#endif

                displayPageCounter++;
                displayPageCounterTotal++;
            }

            if (displayPageCounter == State.Acq.maxNFramePerFile && displayPageCounterTotal < totalPagesSaved())
            {
#if DEBUG
                Debug.WriteLine("Total page saved:" + totalPagesSaved() + ", displayCounter = " + displayPageCounterTotal);
#endif
                flimage.image_display.realtimeData.Clear();
            }

            SW_PerformanceMonitor.Restart();
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

