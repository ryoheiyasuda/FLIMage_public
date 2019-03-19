using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using TCSPC_controls;
using MathLibrary;
using Stage_Control;
using ThorlabController;
using Utilities;

namespace FLIMimage
{
    public partial class FLIMageMain : Form
    {
        public const bool DEBUGMODE = false;

        //Timer UITimer = new Timer();
        Timer RateTimer = new Timer();
        Stopwatch UIstopWatch_Loop = new Stopwatch();
        Stopwatch UIstopWatch_Image = new Stopwatch();
        Stopwatch UIstopWatch_Frame = new Stopwatch();
        Stopwatch SW_PerformanceMonitor = new Stopwatch();

        //For synchronization.
        object syncStateObj = new object();
        object syncFLIMacq = new object();
        object saveBufferObj = new object();
        object syncFLIMdisplay = new object();
        object syncFLIMsave = new object();
        object toolTipObj = new object();
        object waitSliceTaskobj = new object();
        object waitLoopImageTaskobj = new object();

        //////Counters for frames etc.
        //  int n_average;
        int averageCounter;
        int averageSliceCounter;
        public int internalFrameCounter;
        // int internalAveFrameCounter;
        public int internalSliceCounter;
        public int internalImageCounter;

        public bool[] newFile = new bool[] { false, false };

        //// Serve as an acquisiton counter. Event activated by NI mirror card.
        public int AO_FrameCounter;
        // int internalStripeCounter;

        public int savePageCounterTotal = 0;
        public int savePageCounter = 0;
        public int displayPageCounterTotal = 0;
        public int displayPageCounter = 0;
        public int deletedPageCounter = 0;

        double measuredSliceInterval;

        Task waitSlice, waitImage;
        Task saveTask;

        public bool save_image_busy = false;

        //List<Task> SaveTaskList = new List<Task>();
        public bool imageSequencing = false;
        public bool focusing = false;
        public bool grabbing = false;
        public bool looping = false;
        public bool allowLoop = true;
        public bool stopGrabActivated = false;
        bool force_stop = false;
        bool runningImgAcq = false;

        bool waitingImageTask = false;
        bool waitingSliceTask = false;

        int badSyncRateMaxCount = 5;
        int badSyncRateCounter = 0;

        int SettingFileN = 1;

        public bool saveIntensityImage = false;

        public bool use_nidaq = true; //use NIDAQ card
        bool use_pq = true; // use PicoQuant card
        bool use_bh = true; // use Becker and Hickl card
        bool use_motor = true; // use motor control.
        public bool use_mainPanel = true; //For analysis only

        public bool snapShot = false;
        Bitmap snapShotBMP;

        public ScanParameters State;
        ScanParameters Save_State;

        //National instrument DAQ card 
        public IOControls.lineClock lineClock;
        public IOControls.MirrorAO mirrorAO;
        public IOControls.pockelAO pockelAO;

        public IOControls.MirrorAO mirrorAO_S; //parking mirror
        public IOControls.dioTrigger dioTrigger;
        public IOControls.ShutterCtrl ShutterCtrl;
        public IOControls.DigitalIn DI;
        //public IOControls.UncagingShutterDO UncagingShutterDO;
        public IOControls.AO_Write UncagingShutterAO; //This is static.
        public IOControls.DigitalUncagingShutterSignal UncagingShutter_DO; //for time control
        public IOControls.DigitalUncagingShutterSignal UncagingShutter_DO_S; //_S for static shutter control.

        ///////FLIM card
        public FiFio_multiBoards FiFo_acquire;
        public FLIM_Parameters parameters;

        //public const BindingFlags MemberAccess = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;

        ////// FLIM data stroage. All temporal. 
        List<UInt16[][][]> FLIMSaveBuffer = new List<ushort[][][]>(); //Used only when images are not saved in memory.
        List<DateTime> acquiredTimeList = new List<DateTime>(); //Store acquisition time info.
        DateTime acquiredTime;
        public FLIMData FLIM_ImgData;

        ////// Motor parameters.
        public MotorCtrl motorCtrl;
        delegate void motorHandleFunctoin(); //Event triggered motor handling.

        public bool motor_moving = false;
        public double[] motorQ = new double[3];
        public bool show_relativePosition = true;

        object motorQlock = new object();

        ////// Uncaging prameters.
        public UncagingCalibration uc;
        Timer UncagingTimer = new Timer();

        public double[] uncaging_Calib = new double[2];
        int uncaging_SliceCounter = 0;


        ////// Other windows.
        static splashScreen ss = new splashScreen();
        public Image_Display image_display;

        public NIDAQ_Config nidaq_config;
        public MotorCtrlPanel motor_ctrl_panel;

        public Uncaging_Trigger_Panel uncaging_panel;
        public Image_seqeunce image_seqeunce;
        public ShadingCorrection shading_correction;
        public DriftCorrection drift_correction;
        public DigitalSignalPanel DIO_panel;
        public PMTControl pmt_control;

        public Plot uncaging_plotXY;
        public Plot uncaging_plotPockels;
        public Plot uncaging_PlotXY_Scanning;
        public Plot uncaging_PlotPockels_Scanning;

        //
        FastZControl fastZcontrol;

        //
        List<ScanParameters> StateList = new List<ScanParameters>(); //Not made yet.
        public COMserver com_server;
        public TextServer text_server;
        public FLIMage_Event flim_event;
        public FileIO fileIO;
        public Script script;
        public String versionText;

        //////  Event handler for broadcasting.
        public event FLIMage_EventHandler EventNotify;
        public ProcessEventArgs envt = new ProcessEventArgs("", null);
        public delegate void FLIMage_EventHandler(FLIMageMain mc, ProcessEventArgs evnt);

        ///// Shaidng
        public IOControls.Shading shading;

        //// Setting manager
        String settingName = "FLIMageWindow";
        SettingManager settingManager;


        public FLIMageMain()
        {
            InitializeComponent();
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

        void FLIMControls_Load(object sender, EventArgs e)
        {
            this.Hide();
            ss.Show();
            Application.DoEvents();

            this.Text = "FLIMage! Version " + ss.versionText;
            versionText = ss.versionText;


            State = new ScanParameters();

            fileIO = new FileIO(State); //initialize fileIO


            try
            {
                //System.IO.Directory.CreateDirectory(State.Files.FLIMfolderPath);
                System.IO.Directory.CreateDirectory(State.Files.initFolderPath);
                if (!System.IO.File.Exists(State.Files.deviceFileName))
                {
                    System.IO.File.WriteAllText(State.Files.deviceFileName, fileIO.AllSetupValues_device());
                }
                else
                {
                    fileIO.LoadSetupFile(State.Files.deviceFileName);
                }

                if (!System.IO.File.Exists(State.Files.defaultInitFile))
                {
                    System.IO.File.WriteAllText(State.Files.defaultInitFile, fileIO.AllSetupValues_nonDevice());
                }
                else
                {
                    fileIO.LoadSetupFile(State.Files.defaultInitFile);
                }
            }
            catch (Exception E)
            {
                Debug.WriteLine(E.Message);
            }

            InitializeCounter(); //Counter reset.


            use_nidaq = State.Init.NIDAQ_on;
            use_pq = State.Init.FLIM_on && (String.Equals(State.Init.FLIM_mode, "PQ"));
            use_bh = State.Init.FLIM_on && (String.Equals(State.Init.FLIM_mode, "BH"));
            use_motor = State.Init.motor_on;

            //Just to start withsomething...
            int height = 128; // State.Acq.linesPerFrame;
            int width = 128; // State.Acq.pixelsPerLine;
            int[] n_time = new int[] { 64, 64 }; // State.Spc.spcData.n_dataPoint;
            int nChannels = 2; // State.Acq.nChannels;
            int nZScan = 2; // State.Acq.FastZ_nSlices;
            double res = State.Spc.spcData.resolution[0];

            FLIM_ImgData = new FLIMData(State);
            FLIM_ImgData.height = height;
            FLIM_ImgData.width = width;
            FLIM_ImgData.n_time = n_time;
            FLIM_ImgData.nChannels = nChannels;
            FLIM_ImgData.nSlices = nZScan;
            FLIM_ImgData.nFastZ = 1;
            FLIM_ImgData.create_SimulatedFLIM();

            int squareLength = (height > width) ? height : width;
            snapShotBMP = ImageProcessing.FormatImage(new double[] { 0.0, 1.0 }, MatrixCalc.MatrixCreate2D<ushort>(squareLength, squareLength));

            FLIM_ImgData.State.Uncaging.Position = (double[])State.Uncaging.Position.Clone();
            image_display = new Image_Display(FLIM_ImgData, this, true);
            image_display.UpdateImages(true, false, false, true);

            UpdateFileName();

            image_display.LoadFLIM(FLIM_ImgData);
            image_display.ImportState(State);

            FLIM_ImgData.ResetRoi();

            //nidaq_config = new NIDAQ_Config(this);

            if (use_nidaq)
            {
                try
                {
                    IOControls.exportBaseClockSignal(State);
                    lineClock = new IOControls.lineClock(State, false);
                    dioTrigger = new IOControls.dioTrigger(State);
                    ShutterCtrl = new IOControls.ShutterCtrl(State);
                    shading = new IOControls.Shading(State);
                    //calib = new IOControls.Calibration(State, shading);
                    mirrorAO = new IOControls.MirrorAO(State, shading);
                    mirrorAO_S = new IOControls.MirrorAO(State, shading);

                    if (State.Init.EOM_nChannels > 0)
                        pockelAO = new IOControls.pockelAO(State, shading, true);

                    if (State.Init.DO_uncagingShutter)
                    {
                        UncagingShutter_DO = new IOControls.DigitalUncagingShutterSignal(State);
                        UncagingShutter_DO_S = new IOControls.DigitalUncagingShutterSignal(State);
                    }

                    if (State.Init.AO_uncagingShutter)
                        UncagingShutterAO = new IOControls.AO_Write(State.Init.UncagingShutterAnalogPort, 0);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Problem in loading NIDAQmx: " + ex.Message);
                    use_nidaq = false;
                }

            }


            if (!use_nidaq)
            {
                acquisitionPanel.Enabled = false;
                LaserPanel.Enabled = false;
            }

            parameters = new FLIM_Parameters();

            if (use_bh || use_pq)
            {
                parameters.ComputerID = State.Init.ComputerID;
                parameters.FLIMserial = State.Init.FLIMserial;
                parameters.spcData = State.Spc.spcData; //linked together.
                parameters.BoardType = State.Init.FLIM_mode;
                FiFo_acquire = new FiFio_multiBoards(parameters);
                State.Init.ComputerID = parameters.ComputerID;

                FiFio_multiBoards.ErrorCode error = FiFo_acquire.Initialize();
                //SetupFLIMParameters(); included in FiFo_acquire

                if (error != FiFio_multiBoards.ErrorCode.NONE)
                {
                    use_bh = false;
                    use_pq = false;
                }

                SetupTimerRate();

                bool serialError = (error == FiFio_multiBoards.ErrorCode.COMPUTERID_INCORRECT);

                if (error == FiFio_multiBoards.ErrorCode.NONE)
                {
                    FiFo_acquire.FrameDone += new FiFio_multiBoards.FrameDoneHandler(BackGroundFLIMAcq);
                    FiFo_acquire.StripeDone += new FiFio_multiBoards.StripeDoneHandler(StripeDoneEvent);
                    FiFo_acquire.MeasDone += new FiFio_multiBoards.MeasDoneHandler(MeasDoneEvent);
                    //FiFo_acquire.SetupParameters(focusing, parameters);
                    UpdateSPC();

                    if (use_bh)
                    {
                        sync_offset.Enabled = false;
                        sync_offset2.Enabled = false;
                        Binning_setting.Visible = false;
                        NTimePoints.Visible = false;
                        st_binning.Visible = false;
                        st_tp.Visible = false;
                        PQMode_Pulldown.Visible = false;
                        st_mode.Visible = false;
                    }

                    if (use_pq)
                    {
                        sync2Group.Enabled = false;
                        Resolution_Pulldown.Visible = false;
                        st_nTimeP.Visible = false;
                    }

                }

                if (serialError)
                    MessageBox.Show("Wrong FLIM Serial number!! FLIMage DLL was not loaded");
            }

            if (!use_bh && !use_pq)
            {
                tb_Pparameters.Enabled = false;
                State.Init.FLIM_on = false;
            }

            if (use_motor)
            {
                try
                {
                    double[] resolution = new double[] { State.Motor.resolutionX, State.Motor.resolutionY, State.Motor.resolutionZ };
                    double[] steps = new double[] { State.Motor.stepXY, State.Motor.stepXY, State.Motor.stepZ };
                    motorCtrl = new MotorCtrl(State.Init.MotorHWName, State.Init.MotorComPort, resolution, State.Motor.velocity, steps);
                    FillGUIMotor();
                    motorCtrl.MotH += new MotorCtrl.MotorHandler(MotorListener);

                    motorCtrl.continuousRead(false);
                    ContRead.Checked = false;
                }
                catch (Exception EX)
                {
                    Debug.WriteLine(EX.ToString());
                    use_motor = false;
                }
            }

            if (!use_motor)
            {
                stagePanel.Enabled = false;
            }


            for (int i = 0; i < 4; i++)
            {
                if (State.Init.EOM_nChannels <= i)
                {
                    Control[] found = this.Controls.Find("tabPage" + (i + 1), true);
                    if (found != null)
                    {
                        found[0].Visible = false;
                        found[0].Dispose();
                    }
                }
            }

            if (use_nidaq)
            {
                CalibEOM(false);
            }

            if (use_nidaq)
            {
                if (State.Init.UseExternalMirrorOffset)
                {
                    IOControls.AO_Write aoX = new IOControls.AO_Write(State.Init.mirrorOffsetX, State.Acq.XOffset);
                    IOControls.AO_Write aoY = new IOControls.AO_Write(State.Init.mirrorOffsetY, State.Acq.YOffset);
                }
            }

            if (!use_bh && !use_pq && !use_nidaq)
            {
                use_mainPanel = false;
            }
            else
                Show();

            SetParametersFromState(false);

            if (use_motor)
                this.Zero_all_Click(Zero_all, null);

            LoadToolsDefault();

            ss.Hide(); //splash screen.

            FLIM_EventHandling_Init();

            LoadWindows();
            InitializeSetting();
            binningSettingChange();

            EventNotify(this, new ProcessEventArgs("FLIMageStarted", null));
            EventNotify(this, new ProcessEventArgs("ParametersChanged", null));

        }

        void binningSettingChange()
        {
            if (parameters.spcData.HW_Model == "TimeHarp 260 N")
            {
                Binning_setting.Items.Clear();
                Binning_setting.Items.Add("0 (0.25 ns)");
                Binning_setting.Items.Add("1 (0.50 ns)");
                Binning_setting.Items.Add("2 (1.00 ns)");
                Binning_setting.Items.Add("4 (2.00 ns)");
                Binning_setting.Items.Add("8 (4.00 ns)");
            }
        }

        void InitializeSetting()
        {
            settingManager = new SettingManager(settingName, State.Files.initFolderPath);

            settingManager.AddToDict(KeepPagesInMemoryCheck);
            settingManager.AddToDict(BackToCenterCheck);
            settingManager.AddToDict(BackToStartCheck);
            settingManager.AddToDict(analyzeEach);
            settingManager.LoadToObject();
        }

        void settingManagerSave()
        {
            if (settingManager != null)
            {
                settingManager.SaveFromObject();
            }
        }

        /// <summary>
        /// Called when this window is loaded.
        /// </summary>
        void LoadToolsDefault()
        {
            image_display.Show();
            image_display.Activate();

            WindowLocationCalc(); //Locate windows for this and image_display

            image_display.plot_realtime.Show();
            image_display.plot_regular.Show();
        }

        void LoadWindows()
        {

            if (File.Exists(WindowsInfoFileName()))
            {
                String readText = File.ReadAllText(WindowsInfoFileName());
                if (readText.Contains("uncaging_panel") || State.Uncaging.uncage_whileImage && use_nidaq)
                {
                    uncaging_panel = new Uncaging_Trigger_Panel(this);
                    uncaging_panel.Show();
                }

                if (readText.Contains("image_sequence"))
                {
                    image_seqeunce = new Image_seqeunce(this);
                    image_seqeunce.Show();
                }

                if (readText.Contains("fastZcontrol"))
                {
                    fastZcontrol = new FastZControl(this);
                    fastZcontrol.Show();
                }

                if (readText.Contains("shading_correction") && use_nidaq)
                {
                    shading_correction = new ShadingCorrection(this);
                    shading_correction.Show();
                }

                if (readText.Contains("drift_correction"))
                {
                    drift_correction = new DriftCorrection(this);
                    drift_correction.Show();
                }

                if (readText.Contains("script"))
                {
                    script = new Script(this);
                    script.Show();
                }

                if (readText.Contains("pmt_control"))
                {
                    pmt_control = new PMTControl(State.Files.initFolderPath);
                    pmt_control.Show();
                }

                if (!readText.Contains("plot_realtime"))
                    image_display.plot_realtime.Hide();
                if (!readText.Contains("plot_regular"))
                    image_display.plot_regular.Hide();

            }

            MenuItems_CheckControls();
        }

        private void resetWindowPositionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] fileArray = Directory.GetFiles(State.Files.windowsInfoPath, "*.txt");
            foreach (string file in fileArray)
            {
                if (File.Exists(file))
                    File.Delete(file);
            }

            if (uncaging_panel != null && !uncaging_panel.IsDisposed)
            {
                uncaging_panel.Close();
                uncaging_panel = new Uncaging_Trigger_Panel(this);
            }

            if (image_seqeunce != null && !image_seqeunce.IsDisposed)
            {
                image_seqeunce.Close();
                image_seqeunce = new Image_seqeunce(this);
            }

            if (fastZcontrol != null && !fastZcontrol.IsDisposed)
            {
                fastZcontrol.Close();
                fastZcontrol = new FastZControl(this);
            }

            if (shading_correction != null && !shading_correction.IsDisposed)
            {
                shading_correction.Close();
                shading_correction = new ShadingCorrection(this);
            }

            if (drift_correction != null && !drift_correction.IsDisposed)
            {
                drift_correction.Close();
                drift_correction = new DriftCorrection(this);
            }

            if (script != null && !script.IsDisposed)
            {
                script.Close();
                script = new Script(this);
            }

            if (pmt_control != null && !pmt_control.IsDisposed)
            {
                pmt_control.Close();
                pmt_control = new PMTControl();
            }

            if (image_display != null && !image_display.IsDisposed)
            {
                image_display.Close();
                image_display = new Image_Display(FLIM_ImgData, this, true);
            }

            if (image_display.plot_realtime != null && !image_display.plot_realtime.IsDisposed)
            {
                image_display.plot_realtime.Close();
                image_display.plot_realtime = new plot_timeCourse(true, image_display);
            }

            if (image_display.plot_regular != null && !image_display.plot_regular.IsDisposed)
            {
                image_display.plot_regular.Close();
                image_display.plot_regular = new plot_timeCourse(false, image_display);
            }

        }


        public void SaveWindows()
        {
            StringBuilder sb = new StringBuilder();
            if (uncaging_panel != null && uncaging_panel.Visible)
            {
                uncaging_panel.SaveWindowLocation();
                sb.Append("uncaging_panel");
                sb.Append(",");
            }
            if (image_seqeunce != null && image_seqeunce.Visible)
            {
                image_seqeunce.SaveWindowLocation();
                sb.Append("image_sequence");
                sb.Append(",");
            }
            if (fastZcontrol != null && fastZcontrol.Visible)
            {
                fastZcontrol.WindowClosing();
                sb.Append("fastZcontrol");
                sb.Append(",");
            }
            if (shading_correction != null && shading_correction.Visible)
            {
                shading_correction.SaveWindowLocation();
                sb.Append("shading_correction");
                sb.Append(",");
            }
            if (drift_correction != null && drift_correction.Visible)
            {
                drift_correction.SaveWindowLocation();
                sb.Append("drift_correction");
                sb.Append(",");
            }

            if (script != null && script.Visible)
            {
                script.SaveWindowLocation();
                sb.Append("script");
                sb.Append(",");
            }

            //Plot windows.
            if (image_display.plot_realtime != null && image_display.plot_realtime.Visible)
            {
                image_display.plot_realtime.SaveWindowLocation();
                sb.Append("plot_realtime");
                sb.Append(",");
            }

            if (image_display.plot_regular != null && image_display.plot_regular.Visible)
            {
                image_display.plot_regular.SaveWindowLocation();
                sb.Append("plot_regular");
                sb.Append(",");
            }

            if (pmt_control != null && pmt_control.Visible)
            {
                pmt_control.SaveWindowLocationAndSetting();
                sb.Append("pmt_control");
                sb.Append(",");
            }

            string allStr = sb.ToString();
            File.WriteAllText(WindowsInfoFileName(), allStr);
        }

        public void ToolWindowClosed()
        {
            this.BeginInvoke((Action)delegate
            {
                MenuItems_CheckControls();
            });
            SaveWindows(); //When forms are closed, this will be called.
        }

        void MenuItems_CheckControls()
        {
            uncagingControlToolStripMenuItem1.Checked = (uncaging_panel != null && uncaging_panel.Visible);
            shadingCorretionToolStripMenuItem.Checked = (shading_correction != null && shading_correction.Visible);
            driftCorrectionToolStripMenuItem.Checked = (drift_correction != null && drift_correction.Visible);
            fastZControlToolStripMenuItem.Checked = (fastZcontrol != null && fastZcontrol.Visible);
            imageSeqControlToolStripMenuItem.Checked = (image_seqeunce != null && image_seqeunce.Visible);
            imageToolStripMenuItem.Checked = image_display.Visible;
            realtimePlotToolStripMenuItem.Checked = image_display.plot_realtime.Visible;
            remoteControlToolStripMenuItem1.Checked = (script != null && script.Visible);
            stageControlToolStripMenuItem.Checked = (motor_ctrl_panel != null && motor_ctrl_panel.Visible);
            nIDAQConfigToolStripMenuItem.Checked = (nidaq_config != null && nidaq_config.Visible);
            dIOPanelToolStripMenuItem.Checked = (DIO_panel != null && DIO_panel.Visible);
            pMTControlToolStripMenuItem.Checked = pmt_control != null && pmt_control.Visible;
        }

        string WindowsInfoFileName()
        {
            Directory.CreateDirectory(State.Files.windowsInfoPath);
            return Path.Combine(State.Files.windowsInfoPath, "WindowInfo.fwi");
        }

        /// <summary>
        /// Called when windows loaded. Place FLIMage window (this window) and image_display window. 
        /// </summary>
        public void WindowLocationCalc()
        {
            if (use_mainPanel)
            {
                this.Location = new Point(5, 5);
                image_display.Location = new Point(this.Size.Width + this.Location.X, this.Location.Y);
            }
            else
            {
                this.Location = new Point(5, 5);
                image_display.Location = new Point(5, 5);
            }
        }

        /// <summary>
        /// Time Function to update Sync Rate and photon counting rate.
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="myEventArgs"></param>
        private void TimerEventProcessorRate(Object myObject, EventArgs myEventArgs)
        {
            if (use_bh || use_pq) //!runningImgAcq)
            {
                int ret = -1;

                double syncRate1 = 0;
                double syncRate2 = 0;
                double countRate1 = 0;
                double countRate2 = 0;

                ret = FiFo_acquire.GetRate();

                if (ret == 0)
                {
                    //State.Spc.datainfo = PQ_acquire.State.Spc.datainfo;
                    State.Spc.datainfo.syncRate = (int[])parameters.rateInfo.syncRate.Clone();
                    State.Spc.datainfo.countRate = (int[])parameters.rateInfo.countRate.Clone();

                    syncRate1 = (double)State.Spc.datainfo.syncRate[0] / 1e6;
                    syncRate2 = (double)State.Spc.datainfo.syncRate[1] / 1e6;
                    countRate1 = (double)State.Spc.datainfo.countRate[0];
                    countRate2 = (double)State.Spc.datainfo.countRate[1];

                    Sync_rate.Text = String.Format("{0: 0.000} MHz", syncRate1);
                    Sync_rate2.Text = String.Format("{0: 0.000} MHz", syncRate2);
                    Ch_rate1.Text = String.Format("{0:#,##0.##} /s", countRate1);
                    Ch_rate2.Text = String.Format("{0:#,##0.##} /s", countRate2);

                    expectedRate.Text = String.Format("{0: 0.0} MHz", State.Acq.ExpectedLaserPulseRate_MHz);

                    if (syncRate1 > State.Acq.ExpectedLaserPulseRate_MHz * 0.9
                        && syncRate1 < State.Acq.ExpectedLaserPulseRate_MHz * 1.1
                        && syncRate2 > State.Acq.ExpectedLaserPulseRate_MHz * 0.9
                        && syncRate2 < State.Acq.ExpectedLaserPulseRate_MHz * 1.1 || focusing)
                    {
                        badSyncRateCounter = 0;
                    }
                    else
                    {
                        badSyncRateCounter++; // laserWarningButton.Visible = true;
                    }

                    if (badSyncRateCounter > badSyncRateMaxCount)
                        laserWarningButton.Visible = true;
                    else
                    {
                        laserWarningButton.Visible = false;
                    }
                }
            }

            if (!runningImgAcq)
            {
                if (shading != null && shading.calibration != null)
                {
                    double[] result = shading.calibration.readIntensity();
                    if (result.Length > 0)
                        powerRead1.Text = String.Format("{0: 0.0} mV", result[0] * 1000);
                    if (result.Length > 1)
                        powerRead2.Text = String.Format("{0: 0.0} mV", result[1] * 1000);
                    if (result.Length > 2)
                        powerRead3.Text = String.Format("{0: 0.0} mV", result[2] * 1000);
                    if (result.Length > 3)
                        powerRead4.Text = String.Format("{0: 0.0} mV", result[3] * 1000);
                }
            } //Running

            ETime.Text = String.Format("{0:0} s", UIstopWatch_Image.ElapsedMilliseconds / 1000.0);
            ETime2.Text = String.Format("{0:0} s", UIstopWatch_Loop.ElapsedMilliseconds / 1000.0);

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

        /// <summary>
        /// Called by Zoom panel click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomPanel_Click(object sender, MouseEventArgs e)
        {
            GetParametersFromGUI(sender);
        }

        /// <summary>
        /// Called by checkbox or radiobutton, when they are clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Generic_ValueChanged(object sender, EventArgs e)
        {
            GetParametersFromGUI(sender);
        }

        /// <summary>
        /// Write status text.
        /// </summary>
        /// <param name="str"></param>
        public void WriteStatusText(String str)
        {
            if (StatusText.InvokeRequired)
            {
                StatusText.BeginInvoke((Action)delegate
                {
                    StatusText.Text = str;
                });
            }
            else
                StatusText.Text = str;
        }

        //////////////////////////////////////////Power//////////////////////////////////////////////////////////////////
        private void PowerText_change(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tb = (TextBox)sender;
                String SaveText = tb.Text;
                try
                {
                    int value = Convert.ToInt32(tb.Text);
                    if (value >= 0 && value <= 100)
                    {
                        if (sender.Equals(Power1))
                            PowerSlider1.Value = value;
                        else if (sender.Equals(Power2))
                            PowerSlider2.Value = value;
                        else if (sender.Equals(Power3))
                            PowerSlider3.Value = value;
                        else if (sender.Equals(Power4))
                            PowerSlider4.Value = value;
                    }
                    else
                    {
                        tb.Text = SaveText;
                    }
                }
                catch (System.FormatException)
                {
                    tb.Text = SaveText;
                }

                State.Acq.power[0] = PowerSlider1.Value;
                State.Acq.power[1] = PowerSlider2.Value;
                State.Acq.power[2] = PowerSlider3.Value;
                State.Acq.power[3] = PowerSlider4.Value;

                e.Handled = true;
                e.SuppressKeyPress = true;

                if (focusing)
                {
                    ResetFocus();
                }
                else if (grabbing)
                {
                    //Ignore.
                    //Space holder for some function during grabbing.
                }
                else
                    Power_putEOMValues(false);

            }
        }

        private void ZeroVoltage_Click(object sender, EventArgs e)
        {
            int addUncaging = 0;
            if (State.Init.AO_uncagingShutter)
                addUncaging = 1;
            double[] powerArray = new double[State.Init.EOM_nChannels + addUncaging];
            for (int i = 0; i < State.Init.EOM_nChannels; i++)
            {
                powerArray[i] = 0;
            }

            if (uncaging_panel != null && uncaging_panel.UncagingShutter && addUncaging == 1)
                powerArray[powerArray.Length - 1] = 5.0;
            else if (addUncaging == 1)
                powerArray[powerArray.Length - 1] = 0.0;

            uncagingShutterCtrl(uncaging_panel.UncagingShutter, true, true);
            shading.calibration.PutValue(powerArray);
        }

        private void PowerSlider1_MouseUp(object sender, MouseEventArgs e)
        {
            Power1.Text = PowerSlider1.Value.ToString();
            Power2.Text = PowerSlider2.Value.ToString();
            Power3.Text = PowerSlider3.Value.ToString();
            Power4.Text = PowerSlider3.Value.ToString();

            State.Acq.power[0] = PowerSlider1.Value;
            State.Acq.power[1] = PowerSlider2.Value;
            State.Acq.power[2] = PowerSlider3.Value;
            State.Acq.power[3] = PowerSlider4.Value;

            if (focusing)
            {
                ResetFocus();
            }
            else if (grabbing)
            {

            }
            else
                Power_putEOMValues(false);
        }

        public void Power_putEOMValues(bool zero)
        {
            int addUncaging = 0;
            if (State.Init.AO_uncagingShutter)
                addUncaging = 1;
            double[] powerArray = new double[State.Init.EOM_nChannels + addUncaging];
            for (int i = 0; i < State.Init.EOM_nChannels; i++)
            {
                //if (calib.success[i])
                //{
                if (zero)
                    powerArray[i] = shading.calibration.GetEOMVoltageByFitting(0, i);   //calibration.calibrationCurve[i][State.Acq.power[0]];
                else
                    powerArray[i] = shading.calibration.GetEOMVoltageByFitting(State.Acq.power[i], i);//calibrationCurve[i][State.Acq.power[i]];
                //}
            }

            if (uncaging_panel != null && uncaging_panel.UncagingShutter && addUncaging == 1)
                powerArray[powerArray.Length - 1] = 5.0;
            else if (addUncaging == 1)
                powerArray[powerArray.Length - 1] = 0.0;


            if (uncaging_panel != null && State.Init.DO_uncagingShutter)
                uncagingShutterCtrl(uncaging_panel.UncagingShutter, true, true);

            shading.calibration.PutValue(powerArray);
        }


        private void UpdatePowerGUI()
        {
            Power1.Text = Convert.ToString(State.Acq.power[0]);
            Power2.Text = Convert.ToString(State.Acq.power[1]);
            Power3.Text = Convert.ToString(State.Acq.power[2]);
            Power4.Text = Convert.ToString(State.Acq.power[3]);

            PowerSlider1.Value = State.Acq.power[0];
            PowerSlider2.Value = State.Acq.power[1];
            PowerSlider3.Value = State.Acq.power[2];
            PowerSlider4.Value = State.Acq.power[3];

            ImageLaser1.Checked = State.Init.imagingLasers[0];
            ImageLaser2.Checked = State.Init.imagingLasers[1];
            ImageLaser3.Checked = State.Init.imagingLasers[2];
            ImageLaser4.Checked = State.Init.imagingLasers[3];

            UncageLaser1.Checked = State.Init.uncagingLasers[0];
            UncageLaser2.Checked = State.Init.uncagingLasers[1];
            UncageLaser3.Checked = State.Init.uncagingLasers[2];
            UncageLaser4.Checked = State.Init.uncagingLasers[3];
        }


        private void CalibEOM(bool plot)
        {
            if (use_nidaq)
            {
                if (State.Init.openShutterDuringCalibration)
                    ShutterCtrl.open();

                bool[] success = shading.calibration.calibrateEOM(plot);
                shading.applyCalibration(State);

                if (fastZcontrol == null || !fastZcontrol.Visible)
                    ShutterCtrl.close();

                bool allTrue = true;

                for (int i = 0; i < success.Length; i++)
                {
                    Control[] found = this.Controls.Find("tabPage" + (i + 1), true);
                    Control[] button1 = this.Controls.Find("needCalib" + (i + 1), true);
                    button1[0].Visible = !success[i];
                    allTrue = allTrue && success[i];
                    if (!success[i])
                    {
                        found[0].Text = "Error " + (i + 1);
                    }
                    else
                    {
                        found[0].Text = "Laser " + (i + 1);
                    }
                } //for

                if (!allTrue)
                {
                    Calibrate1.ForeColor = Color.Red;
                    needCalibLabel.Visible = true;
                }
                else
                {
                    Power_putEOMValues(false);
                    Calibrate1.ForeColor = Color.Black;
                    needCalibLabel.Visible = false;
                }


            } //nidaq
        }

        //////////////////////////////////////////Power//////////////////////////////////////////////////////////////////
        //////////////////////////////////////////Power//////////////////////////////////////////////////////////////////

        private void PQ_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                TextBox tb = (TextBox)sender;
                String SaveText = tb.Text;
                try
                {
                    PQ_SettingGUI();
                }
                catch (System.FormatException)
                {
                    tb.Text = SaveText;
                }
                finally
                { };
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }


        private void Generic_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                TextBox tb = (TextBox)sender;
                String SaveText = tb.Text;
                try
                {
                    GetParametersFromGUI(sender);
                }
                catch (System.FormatException)
                {
                    tb.Text = SaveText;
                }
                finally
                { };
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }



        private void UpdateImages() //Can be slow. //Done with timer. //Thread safe.
        {
            bool updated = false;

            FLIM_ImgData.State.Uncaging.Position = (double[])State.Uncaging.Position.Clone();
            displayPageCounter++;
            updated = !image_display.update_image_busy;
            if (updated)
            {
                if (State.Acq.fastZScan)
                {
                    if (displayPageCounter == 1) //First page = 1. After ++.
                        image_display.SetFastZModeDisplay(State.Acq.fastZScan); //To set the page at the center.
                    image_display.setupImageUpdateForZStack();
                }

                image_display.UpdateImages(true, true, focusing, true);
            }
            else
            {
                //if (!focusing)
                Debug.WriteLine("**** Busy ************" + displayPageCounter);

                displayPageCounter++;
                displayPageCounterTotal++;
            }

            if (displayPageCounter == State.Acq.maxNFramePerFile && displayPageCounterTotal < totalPagesSaved())
            {
                Debug.WriteLine("Total page saved:" + totalPagesSaved() + ", displayCounter = " + displayPageCounterTotal);
                image_display.realtimeData.Clear();
            }


            //Refresh();

            //Debug.WriteLine("Time from previous frame: " + SW_PerformanceMonitor.ElapsedMilliseconds);
            //if (SW_PerformanceMonitor.ElapsedMilliseconds > 1.3 * State.Acq.frameInterval() * 1000.0)
            //    Debug.WriteLine("Performance is not great!!" + SW_PerformanceMonitor.ElapsedMilliseconds + " ms per frame");


            SW_PerformanceMonitor.Restart();


        }


        private void InitializeCounter()
        {
            averageCounter = 0;
            averageSliceCounter = 0;

            internalSliceCounter = 0;
            internalFrameCounter = 0;

            //internalStripeCounter = 0;
            //internalAveFrameCounter = 0;

            if (uncaging_panel != null)
                uncaging_panel.uncaging_count = 0;
            uncaging_SliceCounter = 0;

            savePageCounterTotal = 0;
            savePageCounter = 0;
            displayPageCounter = 0;
            displayPageCounterTotal = 0;

            newFile = boolAllChannels(true);

            CurrentFrame.Text = "1";
            CurrentSlice.Text = "1";
            nAverageFrame.Text = "1";

            if (!looping)
                CurrentImage.Text = "1";
        }


        /// <summary>
        /// Called when Background acquisition is done. However, it should be noted that save task and display Task are in spearated threads.
        /// </summary>
        private void FLIMProgressChanged()
        {
            int[] nAve = GetAverageFrame(State.Acq.nAveFrame);
            for (int ch = 0; ch < State.Acq.nChannels; ch++)
            {
                if (State.Acq.aveSlice)
                    nAve[ch] = nAve[ch] * State.Acq.nAveSlice;
            }

            internalFrameCounter++;

            //Update GUI. We are in different thread from main window. So, we will evoke it.
            this.BeginInvoke((Action)delegate
            {
                if (!focusing)
                {
                    CurrentFrame.Text = internalFrameCounter.ToString();
                    if (internalSliceCounter + 1 <= State.Acq.nSlices)
                        CurrentSlice.Text = (internalSliceCounter + 1).ToString();
                    if (internalImageCounter + 1 <= State.Acq.nSlices)
                        CurrentImage.Text = (internalImageCounter + 1).ToString();

                }
                else
                {
                    MeasuredLineCorrection.Text = String.Format("{0:0.000}", parameters.spcData.measured_line_time_correction);
                }
            });
            //Debug.WriteLine("Acquired frame {0} / {1}, Slice {2} / {3}. Time = {4} ms", internalFrameCounter, Acq_nFrames, internalSliceCounter, State.Acq.nSlices, UIstopWatch_Frame.ElapsedMilliseconds);

            UIstopWatch_Frame.Restart();

            if (internalFrameCounter == State.Acq.nFrames && !focusing)
            {
                //Debug.WriteLine("Slice done!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                internalSliceCounter++;
                internalFrameCounter = 0;


                StopDAQ(); //close shutter included.
                DisposeDAQ();
                FiFo_acquire.StopMeas(true);

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
                    Debug.WriteLine("Saving extra file.... " + (savePageCounterTotal), "/" + (savePageCounter));
                }

                FLIMSaveBuffer.Clear();

                this.BeginInvoke((Action)delegate
                {
                    if (measuredSliceInterval != 0)
                        Measured_slice_interval.Text = String.Format("{0:0.00} s", measuredSliceInterval);
                    CurrentSlice.Text = (internalSliceCounter).ToString();
                });

                if (internalSliceCounter == State.Acq.nSlices) //All slices done.
                {

                    MoveBackToHome();

                    State.Files.fileCounter++; //This needs to be after MoveMotorBack, since movemotorbacktohome sends notification with the current fileCounter.

                    internalImageCounter++;

                    if (internalImageCounter >= State.Acq.nImages || !allowLoop)
                    {
                        Task.Factory.StartNew(() => { StopGrab(true); });
                    }

                    if (State.Acq.fastZScan && !State.Acq.ZStack)
                    {
                        //Setup display.
                        var FLIM5D = (ushort[][][])FLIM_ImgData.FLIMRaw5D;
                        FLIM_ImgData.clearPages4D(); //Clean the the 4D stack. it is separated from acquisition.
                        FLIM_ImgData.addToPageAndCalculate5D(FLIM5D, acquiredTime, true, true, 0, true);
                    }

                    try
                    {
                        UpdateFileName();
                        image_display.ZStack = FLIM_ImgData.ZStack || State.Acq.fastZScan;
                        image_display.displayZProjection = FLIM_ImgData.ZStack || State.Acq.fastZScan;

                        //FLIM_ImgData.n_pages = savePageCounter;
                        image_display.UpdateImages(true, false, false, true);

                        //if (!State.Acq.acqFLIMA)
                        //    image_display.calculateTimecourse();

                        FLIM_ImgData.fileUpdateRealtime(State, false); //FileName update.
                        image_display.UpdateFileName();

                        if (analyzeEach.Checked)
                        {
                            image_display.BeginInvoke((Action)delegate
                            {
                                image_display.plot_regular.Show();
                                image_display.plot_regular.Activate();
                                image_display.OpenFLIM(FLIM_ImgData.State.Files.fullName(), true);
                            });
                        }
                    }
                    catch (Exception E)
                    {
                        Debug.WriteLine("Problem in displaying and /or analyzing saved image: " + E.Message);
                        WriteStatusText("Problem in displaying and /or analyzing saved image");
                    }

                    EventNotify(this, new ProcessEventArgs("AcquisitionDone", null));

                    grabbing = false; //grab is finished.

                    //////////////
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
                        StopLoop();
                    }

                }
                else if (internalSliceCounter < State.Acq.nSlices) // Slices still not done.
                {
                    if (ZStack_radio.Checked)
                    {
                        MoveMotorStep(true);
                        if (internalSliceCounter == State.Acq.nSlices - 1)
                            motorCtrl.stack_Position = MotorCtrl.StackPosition.End;
                    }

                    EventNotify(this, new ProcessEventArgs("SliceAcquisitionDone", null));

                    waitSlice = Task.Factory.StartNew(() =>
                    {
                        lock (waitSliceTaskobj) //This task will never overlap. Don't use in any other lock.
                            WaitForNextSlice();
                    });
                }
                else
                {
                    Debug.WriteLine("Warning.... Slice counter: {0} > n Slices: {1}", internalSliceCounter, State.Acq.nSlices);
                }

            }//internalFrame

            try
            {
                this.Invoke((Action)delegate
                {
                    if (!focusing)
                    {
                        if (State.Acq.aveSlice)
                        {
                            if (internalFrameCounter == 0 && averageSliceCounter == 0)
                                nAverageFrame.Text = (State.Acq.nAveSlice * State.Acq.nAveFrame).ToString();
                            else
                                nAverageFrame.Text = (internalFrameCounter + averageSliceCounter * State.Acq.nAveFrame).ToString();
                        }
                        else if (State.Acq.aveFrameA[0])
                            nAverageFrame.Text = (averageCounter + 1).ToString();
                        else
                            nAverageFrame.Text = "1";
                    }
                    else
                    {
                        nAverageFrame.Text = "1";
                    }
                });
            }
            catch
            {
                Debug.WriteLine("****FLIM control may not exist anymore****");
            }
        }

        void waitForAcquisitionTaskCompleted()
        {
            //There is no "wait" function in FiFO_acquire.
            if (FiFo_acquire != null)
            {
                System.Threading.Thread.Sleep(5);
                while (!FiFo_acquire.IsCompleted())
                    System.Threading.Thread.Sleep(5);
            }
        }


        /// <summary>
        /// Wait for next slice and then execute next slice. This occurs in different thread, so that this window is released.
        /// </summary>
        void WaitForNextSlice()
        {
            double eTime = UIstopWatch_Image.ElapsedMilliseconds; //Start measuring the time
            waitingSliceTask = true; //you can turn off this to stop this task. Not called by any function for now.

            if (DEBUGMODE)
                Debug.WriteLine("Now WaitForNextSlice task started");

            DisposeDAQ(); //Just to make sure in this thread...
            waitForAcquisitionTaskCompleted();
            uncaging_SliceCounter++;

            int standard_waitTime = 40;  //With this cycle, we can see if stopGrab is activated. 

            if (State.Uncaging.sync_withSlice && State.Uncaging.uncage_whileImage) //Uncaging protocol!!
            {
                while (waitingSliceTask && !State.Acq.ZStack) //Cycle roughly every standard_waitTime.
                {
                    double eTime2 = UIstopWatch_Image.ElapsedMilliseconds;
                    double waitTime = State.Acq.sliceInterval * internalSliceCounter * 1000.0 - State.Uncaging.uncagingTime() - eTime2;

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

                if (stopGrabActivated || !waitingSliceTask) //stoped in the middle.
                {
                    MoveBackToHome();
                    return;
                }

                if (uncaging_SliceCounter >= State.Uncaging.SlicesBeforeUncage)
                {
                    bool fire = (uncaging_SliceCounter - State.Uncaging.SlicesBeforeUncage) % State.Uncaging.Uncage_SliceInterval == 0;
                    fire = fire && uncaging_panel.uncaging_count < State.Uncaging.trainRepeat;
                    //Stopwatch sw1 = new Stopwatch();
                    //sw1.Start();
                    if (fire)
                    {
                        if (uncaging_panel != null)
                        {
                            Power_putEOMValues(true); //Put zero value first before opening the shutter.
                            ShutterCtrl.open();
                            uncaging_panel.UncageOnce(false, this); //Takes 40 + uncaging ms.
                                                                    //Debug.WriteLine("Uncaging time: {0}", sw1.ElapsedMilliseconds);
                        }
                    }
                    else
                        System.Threading.Thread.Sleep((int)State.Uncaging.uncagingTime());
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


            if (stopGrabActivated || !waitingSliceTask) //When stopped, the above loop breaks;
            {
                MoveBackToHome();
                return;
            }

            eTime = UIstopWatch_Image.ElapsedMilliseconds;
            measuredSliceInterval = eTime / 1000.0 / internalSliceCounter;

            bool[] eraseMemoryA = boolAllChannels(!State.Acq.aveSlice || averageSliceCounter == 0);

            AO_FrameCounter = 0;
            waitingSliceTask = false; //Now it finished its work.

            if (DEBUGMODE)
                Debug.WriteLine("Before Start DAQ in wait slice task"); //Too let us know where we are.

            if (!stopGrabActivated)
            {
                Power_putEOMValues(false);
                StartDAQ(eraseMemoryA, true, false);
                EventNotify(this, new ProcessEventArgs("SliceAcquisitionStart", null));
            }

            if (DEBUGMODE)
                Debug.WriteLine("Finished wait slice task");

        }

        bool[] boolAllChannels(bool bool1)
        {
            return Enumerable.Repeat<bool>(bool1, State.Acq.nChannels).ToArray();
        }

        void LoopingImageAcq()
        {
            waitForAcquisitionTaskCompleted(); //// You have to do it from different thread!!;

            waitingImageTask = true;

            FocusButton.BeginInvoke((Action)delegate ()
            {
                FocusButton.Enabled = true;
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
                MessageBox.Show("FiFo saturated!!");

            try
            {
                if (fastZcontrol != null)
                {
                    fastZcontrol.CalculateFastZParameters();
                }
                else
                    State.Acq.fastZScan = false;
            }
            catch
            {
                Debug.WriteLine("***Main window is closed!!*****");
            }
        }


        ///////////////////////////////////STRIPE EVENT HANDLING////////////////////////////////////////
        private void StripeDoneEventAll(UInt16[][][,,] StripeImage, StripeEventArgs e)
        {
            if (DEBUGMODE)
                Debug.WriteLine("Stripe event called: device = " + e.device + "/ channel" + e.channelList[0] + " / " + e.channelList.Count + " FirstLine = " + e.StartLine + "EndLine = " + e.EndLine);
            try
            {
                for (int ch = 0; ch < e.channelList.Count; ch++)
                {
                    image_display.UpdateStripe(StripeImage, e.channelList[ch], e.StartLine, e.EndLine);
                }
            }
            catch (Exception E1)
            {
                Debug.WriteLine("Error during Stripe: " + E1.Message);
            }
        }

        private void StripeDoneEvent(FiFio_multiBoards fifo, StripeEventArgs e)
        {
            if (focusing)
            {
                StripeDoneEventAll(fifo.FLIM_Stripe, e);
            }
        }


        ///////////////////////////////////STRIPE EVENT HANDLING////////////////////////////////////////

        ///////////////////////////////////FRAME EVENT HANDLING////////////////////////////////////////

        private void BackGroundFLIMAcq(FiFio_multiBoards fifo, EventArgs e) //called when a Frame is done.
        {
            lock (syncFLIMacq) //acquisition lock.
            {
                BackGroundFLIMAll(fifo);
            }
        }

        private int[] GetAverageFrame(int nAverage)
        {
            int[] aveN = new int[State.Acq.nChannels];
            for (int c = 0; c < State.Acq.nChannels; c++)
                if (State.Acq.aveFrameA[c])
                    aveN[c] = nAverage;
                else
                    aveN[c] = 1;

            return aveN;
        }

        private int[] Get_n_time()
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
        /// This handles acquired images. Called by FiFo when a frame is acquired.
        /// </summary>
        /// <param name="fifo"></param>
        private void BackGroundFLIMAll(FiFio_multiBoards fifo)
        {
            if (DEBUGMODE)
                Debug.WriteLine("Frame done event received. Time = " + DateTime.Now.ToString("HH:mm:ss.fff"));

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            if (snapShot & (use_pq || use_bh))
            {
                UInt16[][][,,] FLIMTemp = (UInt16[][][,,])fifo.FLIM_data;

                FLIM_ImgData.LoadFLIMdata5D_Realtime(FLIMTemp); //Save in FLIM_ImgData class. Linearize first.
                var flimZdata = ImageProcessing.PermuteFLIM5D(FLIM_ImgData.FLIMRaw5D); //Copy to FLIMPage.

                if (!State.Acq.fastZScan)
                    FLIM_ImgData.LoadFLIMRawFromLinearAllChannels(flimZdata[0]);
                else
                {
                    FLIM_ImgData.SetupPagesFromZProject(flimZdata, acquiredTime);
                    image_display.calcZProjection();
                }


                FLIM_ImgData.nAveragedFrame = Enumerable.Repeat<int>(1, State.Acq.nChannels).ToArray();
                FLIM_ImgData.calculateProject();

                snapShotBMP = ImageProcessing.FormatImage(image_display.State_intensity_range[image_display.currentChannel], FLIM_ImgData.Project[image_display.currentChannel]);

                ScanPosition.Invalidate();
                StopFocus();

                snapShot = false;
                State = Save_State;

                this.BeginInvoke((Action)delegate
                {
                    SetParametersFromState(true);
                });

                return;
            } //Snapshot.

            if (use_pq || use_bh)
            {
                Boolean running = false;

                if (runningImgAcq)
                {
                    running = fifo.Running;
                }

                if (running)
                {
                    UInt16[][][,,] FLIMTemp = fifo.FLIM_data;

                    FLIM_ImgData.LoadFLIMdata5D_Realtime(FLIMTemp); //Save in FLIM_ImgData class. This linearize the vector.

                    var flimZdata = ImageProcessing.PermuteFLIM5D(FLIM_ImgData.FLIMRaw5D);

                    if (!State.Acq.fastZScan)
                        FLIM_ImgData.LoadFLIMRawFromLinearAllChannels(flimZdata[0]);
                    else
                    {
                        FLIM_ImgData.SetupPagesFromZProject(flimZdata, acquiredTime);
                        //image_display.calcZProjection();
                    }

                    bool[] savebool = boolAllChannels(false);
                    bool protecting_save_task = false;

                    if (focusing)
                    {
                        FLIM_ImgData.nAveragedFrame = Enumerable.Repeat(1, State.Acq.nChannels).ToArray();
                    }
                    else
                    {
                        for (int i = 0; i < State.Acq.nChannels; i++)
                            savebool[i] = (!(State.Acq.aveFrameA[i] && State.Acq.nAveFrame > 1)) && (State.Acq.acquisition[i]);

                        if (State.Acq.aveFrameA.Any(x => x == true) && State.Acq.nAveFrame > 1)
                        {
                            averageCounter++;

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
                            }
                        }
                    } //if focus.

                    FLIM_ImgData.saveChannels = (bool[])savebool.Clone();

                    SaveUpdateAcquiredImage(savebool, protecting_save_task);

                    if (runningImgAcq)
                        FLIMProgressChanged();


                } //Running
            } //if (use_bh || use_pq)
            //Debug.WriteLine("Time to acquire file: " + sw.ElapsedMilliseconds + " ms");
        } //BackgroundFLIMAll.

        /// <summary>
        /// SaveUpdateAcquiredImage: Called by BackgroundFLIMAll. Control saving and displaying acquired data.
        /// </summary>
        /// <param name="saveFileBools"></param>
        /// <param name="protecting_save_task"></param>
        private void SaveUpdateAcquiredImage(bool[] saveFileBools, bool protecting_save_task)
        {
            DateTime acTime;
            double msPerLine = State.Acq.fastZScan ? State.Acq.FastZ_msPerLine : State.Acq.msPerLine;

            //Deepcopy of FLIMRaw5D.
            if (saveFileBools.Any(x => x == true)) //If any savefile exists.
            {
                acTime = acquiredTime.AddMilliseconds(internalFrameCounter * msPerLine * State.Acq.linesPerFrame);

                var FLIMForSave = MatrixCalc.MatrixCopy3D(FLIM_ImgData.FLIMRaw5D);

                FLIM_ImgData.saveChannels = saveFileBools;

                for (int i = 0; i < saveFileBools.Length; i++)
                {
                    if (!saveFileBools[i])
                    {
                        FLIMForSave[i] = null;
                    }
                }


                lock (saveBufferObj)
                {
                    if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
                    {
                        //Adding FLIMforSave data to FLIM_Pages5D. We don't need deep copy because it is already copied from FLIMRaw5D. (the last argument.
                        FLIM_ImgData.Add5DFLIM(FLIMForSave, acTime, savePageCounter, false);

                        //Debug.WriteLine("Loaded to page: " + displayPageCounter + "/" + FLIM_ImgData.FLIM_Pages5D.Count);
                    }
                    else
                    {
                        FLIMSaveBuffer.Add(FLIMForSave); //This is not a copy.
                        acquiredTimeList.Add(acTime);
                    }
                }
            }


            UpdateImages();


            if (saveFileBools.Any(x => x == true))
            {
                SaveFile(protecting_save_task);
            }
        }

        /// <summary>
        /// If not keeping the data in memory, the stack is removed after saving. For ZStack, everything is saved in memory.
        /// </summary>
        private void CheckDeletePageBuffer()
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

        private void RemoveFrameAt(int frameToWork) //Clear memory very well.
        {
            if (FLIMSaveBuffer != null && frameToWork < FLIMSaveBuffer.Count)
                FLIMSaveBuffer.RemoveAt(frameToWork);
        }

        private int totalPagesSaved()
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

        private void SaveFile(bool protecting_save_task)
        {
            if (DEBUGMODE)
                Debug.WriteLine("Start Save File");

            bool updated = false;
            int savePage = savePageCounter - deletedPageCounter;
            //int safeMergin = 0;

            if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
            {
                //Save task occurs only when savePage Counter is less than 5D page.
                updated = (FLIM_ImgData.FLIM_Pages5D.Length > savePageCounter);
            }
            else
            {
                updated = (FLIMSaveBuffer.Count > savePage && savePage >= 0);
            }

            //If previous saveTask is still running, it will wait until it is done.
            if (protecting_save_task && saveTask != null && !saveTask.IsCompleted) 
                saveTask.Wait();

            if (updated && (saveTask == null || saveTask.IsCompleted))
            {
                int savePage1 = savePage; //Protect the count.

                if (protecting_save_task)
                    SaveTask(savePage1);
                else
                    saveTask = Task.Factory.StartNew(() => { SaveTask(savePage1); });

            }
            else
            {
                Debug.WriteLine("Saving BUSY***************************Could not save:" + (savePageCounterTotal + 1) + " (" + (savePageCounter + 1) + "/" + FLIM_ImgData.FLIM_Pages5D.Count() + ")");
            }

            if (this.InvokeRequired)
            {
                this.Invoke((Action)delegate
                {
                    if (SavedFileN != null)
                        SavedFileN.Text = (savePageCounterTotal + 1).ToString();
                });
            }
            else
            {
                if (SavedFileN != null)
                    SavedFileN.Text = (savePageCounterTotal + 1).ToString();
            }
            //busySaving = false;
        }

        /// <summary>
        /// Save acquired image. Called in different thread (Task.Factory.StartNew). 
        /// </summary>
        /// <param name="savePage"></param>
        private void SaveTask(int savePage)
        {
            if (DEBUGMODE)
                Debug.WriteLine("Start Save Task");

            lock (syncFLIMsave)
            {
                if (DEBUGMODE)
                    Debug.WriteLine("Start FLIMsave sync");

                UInt16[][][] FLIMImage; //Before permutation.
                DateTime acqTimeTemp;

                ushort[][][] FLIM_5D; //After permutation.

                ////Overwrite and savefile setting
                bool[] overwrite = (bool[])newFile.Clone();

                int error = 0;
                String fullFileName = State.Files.fullName();

                bool[] saveCh = boolAllChannels(true);

                //Everything about FLIM_Page5D or FLIMSaveBufer. 
                lock (saveBufferObj)
                {
                    if (DEBUGMODE)
                        Debug.WriteLine("Start FLIMmovie sync");

                    if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
                    {
                        FLIMImage = FLIM_ImgData.FLIM_Pages5D[savePageCounter];
                        acqTimeTemp = FLIM_ImgData.acquiredTime_Pages5D[savePageCounter];
                    }
                    else
                    {
                        FLIMImage = FLIMSaveBuffer[savePage];
                        acqTimeTemp = acquiredTimeList[savePage];
                    }

                    FLIM_5D = ImageProcessing.PermuteFLIM5D(FLIMImage); //Finished copy.

                    for (int ch = 0; ch < State.Acq.nChannels; ch++)
                    {
                        if (FLIMImage[ch] == null)
                            saveCh[ch] = false;
                    }
                }

                if (DEBUGMODE)
                    Debug.WriteLine("Start Saving File.");

                if (!State.Files.channelsInSeparatedFile)
                {
                    bool overwrite1 = overwrite[0];
                    newFile = boolAllChannels(false);

                    if (FLIM_5D.Length == 1)
                        error = fileIO.SaveFLIMInTiff(fullFileName, FLIM_5D[0], acqTimeTemp, overwrite1, saveCh);
                    else
                        error = fileIO.SaveFLIMInTiffZStack(fullFileName, FLIM_5D, acqTimeTemp, overwrite1, saveCh);
                }
                else
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
                                ushort[][] FLIM_separated = new ushort[State.Acq.nChannels][];
                                FLIM_separated[ch] = FLIM_5D[0][ch];
                                error = fileIO.SaveFLIMInTiff(fileName, FLIM_separated, acqTimeTemp, overwrite1, saveCh);

                                if (DEBUGMODE)
                                    Debug.WriteLine("Saving file.. {0}, channel = {1}, overwrite = {2}", fileName, ch, overwrite1);
                            }
                            else
                            {
                                ushort[][][] FLIM_separated = new ushort[FLIM_5D.Length][][];
                                for (int z = 0; z < FLIM_5D.Length; z++)
                                {
                                    FLIM_separated[z][ch] = FLIM_5D[z][ch];
                                }
                                error = fileIO.SaveFLIMInTiffZStack(fileName, FLIM_separated, acqTimeTemp, overwrite1, saveCh);

                                if (DEBUGMODE)
                                    Debug.WriteLine("Saving file.. {0}, channel = {1}, overwrite = {2}, n_zslice = ", fileName, ch, overwrite1, FLIM_5D.Length);

                            }
                        }
                    }


                if (saveIntensityImage)
                {
                    //bool ChannelsInSeparatedFiles = false; // image_display.ExportSeparatedChannelFiles;
                    for (int c = 0; c < State.Acq.nChannels; c++)
                    {
                        if (FLIMImage[c] != null)
                        {
                            if (State.Files.channelsInSeparatedFile)
                            {
                                saveCh = new bool[State.Acq.nChannels];
                                saveCh[c] = true;
                            }

                            for (int z = 0; z < FLIM_5D.Length; z++)
                            {
                                String fileName = fileIO.FLIM_FilePath(c, State.Files.channelsInSeparatedFile, State.Files.fileCounter, FileIO.ImageType.Intensity, "", State.Files.pathName);
                                bool overwrite1 = (overwrite[c] && (z == 0)) && (State.Files.channelsInSeparatedFile || c == 0);
                                int[] dimYXT = FLIM_ImgData.dimensionYXT(c);
                                fileIO.Save2DImageInTiff(fileName, ImageProcessing.GetProjectFromFLIM_Linear(FLIM_5D[z][c], dimYXT), acqTimeTemp, overwrite1, saveCh);
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
                    EventNotify(this, new ProcessEventArgs("SaveImageDone", FLIMImage));
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
                    UpdateFileName();

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

                //EventNotify(this, new ProcessEventArgs("FrameAcquisitionDone", null));

            } //Sync

            if (DEBUGMODE)
                Debug.WriteLine("Ended Save Task");
        }

        ///////////////////////////////////FRAME EVENT HANDLING////////////////////////////////////////

        /// <summary>
        /// Abort focusing by button click.
        /// </summary>
        private void StopFocusCore()
        {
            force_stop = true;

            if (FiFo_acquire != null)
                FiFo_acquire.StopMeas(force_stop);

            StopDAQ();
            DisposeDAQ();
            //
            ParkMirrors();

            runningImgAcq = false;

            ChangeItemsStatus(true, true);
            FocusButton.Text = "FOCUS";
            focusing = false;

            SaveSetting();

            //
            focusing = false;

            if (fastZcontrol != null)
                fastZcontrol.ControlsDuringScanning(false);

            EventNotify(this, new ProcessEventArgs("FocusStop", null));
        }

        /// <summary>
        /// Directly called after stop button click.
        /// </summary>
        private void StopFocus()
        {
            if (FocusButton.InvokeRequired)
            {
                FocusButton.BeginInvoke((Action)delegate
                {
                    StopFocusCore();
                });
            }
            else
                StopFocusCore();
        }

        /// <summary>
        /// Called by abort grab button click.
        /// </summary>
        /// <param name="force"></param>
        private void StopGrab(bool force)
        {
            if (GrabButton.InvokeRequired)
            {
                GrabButton.BeginInvoke((Action)delegate
                {
                    StopGrabCore(force);
                });
            }
            else
                StopGrabCore(force);
        }

        /// <summary>
        /// Actual program for stop grabbing.
        /// </summary>
        /// <param name="force"></param>
        private void StopGrabCore(bool force)
        {
            force_stop = force;
            stopGrabActivated = force;

            //KillThread(); //Kill threadtask.

            if (FiFo_acquire != null)
                FiFo_acquire.StopMeas(force);

            StopDAQ();
            DisposeDAQ();

            ParkMirrors();

            runningImgAcq = false;

            if (!looping || stopGrabActivated)
            {
                UIstopWatch_Loop.Stop();
                UIstopWatch_Image.Stop();
            }

            SW_PerformanceMonitor.Stop();
            ChangeItemsStatus(true, false);
            GrabButton.Text = "GRAB";

            if (use_motor)
            {
                motorCtrl.continuousRead(ContRead.Checked);
                motorCtrl.GetPosition(true);
            }

            grabbing = false;
            looping = false;

            if (fastZcontrol != null)
                fastZcontrol.ControlsDuringScanning(false);
            //SaveSetting();
            //
        }

        /// <summary>
        /// When stopping, mirrors need to be parked.
        /// </summary>
        void ParkMirrors()
        {
            double[] values = State.Init.mirrorParkPosition;
            mirrorAO_S.putValue_S(values);
            Power_putEOMValues(true);
        }

        /// <summary>
        /// CheckSavingParameters: creating directory for saving etc before starting grab.
        /// </summary>
        /// <returns></returns>
        int CheckSavingParameters()
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

        /// <summary>
        /// NI-DAQ boards for grabbing are disposed. When stopping grab/focus.
        /// </summary>
        void DisposeDAQ()
        {
            if (use_nidaq)
            {
                lineClock.dispose();
                mirrorAO.dispose();

                if (State.Init.EOM_nChannels > 0)
                    pockelAO.dispose();

                if (State.Init.DO_uncagingShutter)
                    UncagingShutter_DO.dispose();
            }
        }

        /// <summary>
        /// Stop NIDAQ boards used for grabbing.
        /// </summary>
        private void StopDAQ()
        {
            if (use_nidaq)
            {
                if (fastZcontrol == null || !fastZcontrol.Visible)
                    ShutterCtrl.close();
                lineClock.stop();
                mirrorAO.stop();
                if (State.Init.EOM_nChannels > 0)
                    pockelAO.stop();

                if (State.Init.DO_uncagingShutter)
                {
                    UncagingShutter_DO.Stop();
                }
            }
            runningImgAcq = false;
        }

        /// <summary>
        /// Called by NIDAQ mirror AO using the FrameDone event listener, when a frame is done. 
        /// See IOcontrolls.cs for the listener setup.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void mirrorAOFrameDoneEvent(object o, EventArgs e)
        {
            AO_FrameCounter++;
            //EventNotify(this, new ProcessEventArgs("FrameScanDone", (object)AO_FrameCounter));

            if (AO_FrameCounter <= State.Acq.nFrames)
            {
                this.BeginInvoke((Action)delegate
                {
                    AOCounter.Text = AO_FrameCounter.ToString();
                });
            }
        }

        private void StartDAQ(bool[] eraseSPCmemory, bool putValue, bool recordTriggerTime)
        {
            runningImgAcq = true;

            waitForAcquisitionTaskCompleted();

            if (FiFo_acquire == null)
                return;

            if (use_nidaq)
            {
                if (putValue)
                {
                    //Is this better?
                    lineClock.dispose();
                    lineClock = new IOControls.lineClock(State, focusing);

                    mirrorAO.dispose();
                    mirrorAO = new IOControls.MirrorAO(State, shading);
                    mirrorAO.FrameDone += new IOControls.MirrorAO.FrameDoneHandler(mirrorAOFrameDoneEvent);


                    if (State.Init.EOM_nChannels > 0 && !mirrorAO.SameBoard)
                    {
                        pockelAO.dispose();
                        pockelAO = new IOControls.pockelAO(State, shading, true);
                    }

                    if (!focusing && State.Uncaging.sync_withFrame && State.Uncaging.uncage_whileImage)
                    {
                        mirrorAO.putValueScanAndUncaging();
                        if (State.Init.EOM_nChannels > 0 && !mirrorAO.SameBoard)
                            pockelAO.putValueScanAndUncaging();

                        if (State.Init.DO_uncagingShutter)
                        {
                            UncagingShutter_DO.dispose();
                            UncagingShutter_DO = new IOControls.DigitalUncagingShutterSignal(State);
                            UncagingShutter_DO.PutValue_and_Start(true);
                        }
                    }
                    else
                    {
                        bool uncaging_shutter = false;
                        if (uncaging_panel != null)
                        {
                            uncaging_shutter = uncaging_panel.UncagingShutter;
                        }

                        try
                        {
                            mirrorAO.putValueScan(focusing, uncaging_shutter, true);
                        }
                        catch (Exception EX)
                        {
                            Debug.WriteLine("mirrorAO error !" + EX.Message);
                        }

                        if (State.Init.EOM_nChannels > 0 && !mirrorAO.SameBoard)
                            pockelAO.putValueScan(focusing, uncaging_shutter, true);

                        if (State.Init.DO_uncagingShutter)
                        {
                            uncagingShutterCtrl(uncaging_shutter, false, true);
                        }

                    }


                    FLIM_ImgData.copyState(State);

                    if (State.Init.EOM_nChannels > 0 && !mirrorAO.SameBoard)
                        pockelAO.start(State.Acq.externalTrigger);

                    mirrorAO.start(State.Acq.externalTrigger);
                    lineClock.start(State.Acq.externalTrigger);
                }
            }

            FiFo_acquire.StartMeas(eraseSPCmemory, focusing, false);

            if (use_nidaq)
            {
                if (putValue)
                {
                    ShutterCtrl.open();
                    System.Threading.Thread.Sleep(2); //shutter open.

                    if (!State.Acq.externalTrigger)
                        dioTrigger.Evoke();

                    DateTime at = new DateTime();
                    at = DateTime.Now;

                    if (eraseSPCmemory.Any(x => x == true))
                    {
                        acquiredTime = at;
                        Debug.WriteLine("Triggered..." + acquiredTime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                    }

                    //if (eraseSPCmemory || internalSliceCounter == 0)
                    //{
                    //    acquiredTime = at;
                    //    Debug.WriteLine("Triggered..." + acquiredTime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                    //}
                    if (recordTriggerTime)
                    {
                        State.Acq.triggerTime = acquiredTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");
                        FLIM_ImgData.State.Acq.triggerTime = State.Acq.triggerTime;
                    }

                } //put value
            } //nidaq
        }

        /// <summary>
        /// Start new measurment --- designed for TagLens parameter measurement. 
        /// </summary>
        /// <param name="eraseAllMemory"></param>
        /// <param name="focusing"></param>
        /// <param name="measure_tagParameters"></param>
        public void FiFo_StartNew(bool eraseAllMemory, bool focusing, bool measure_tagParameters)
        {
            bool[] erase = boolAllChannels(eraseAllMemory);
            FiFo_acquire.StartMeas(erase, focusing, measure_tagParameters);
        }

        /// <summary>
        /// Starting Grab. 
        /// </summary>
        /// <param name="focus"></param>
        private void StartGrab(bool focus)
        {
            //In case this function is activated from different thread. 
            //To make the process thread safe.
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)delegate
                {
                    StartGrabCore(focus);
                });
            }
            else
                StartGrabCore(focus);
        }

        /// <summary>
        /// Actual program for start grab.
        /// </summary>
        /// <param name="focus"></param>
        private void StartGrabCore(bool focus)
        {
            ImageDisplayOpen();

            //if (!snapShot)
            //{
            //    image_display.plot_realtime.Show();
            //    image_display.plot_realtime.Activate();

            //}
            //image_display.plot_regular.Hide();

            //focusing = false;
            GetParametersFromGUI(this); //Setup all parameters.
            if (State.Uncaging.uncage_whileImage && uncaging_panel != null)
                uncaging_panel.SetupUncage(this);

            if (State.Acq.XOffset > State.Acq.XMaxVoltage || State.Acq.YOffset > State.Acq.YMaxVoltage)
            {
                MessageBox.Show("Offset exceeds maximum voltage!!");
                return;
            }

            SetupFLIMParameters(); //this will setup parameters for FLIM card..

            //Initialize FLIM_ImgData and fileIO, that reflects State.
            if (fastZcontrol != null)
                fastZcontrol.ControlsDuringScanning(true);

            List<ROI> ROIs = FLIM_ImgData.ROIs;
            FLIM_ImgData.InitializeData(State);
            FLIM_ImgData.ROIs = ROIs;
            fileIO = new FileIO(State);


            image_display.SetupRealtimeImaging(State, FLIM_ImgData);
            image_display.SetFastZModeDisplay(State.Acq.fastZScan);

            Power_putEOMValues(false);
            runningImgAcq = true;

            savePageCounter = 0;
            displayPageCounter = 0;
            deletedPageCounter = 0;
            AO_FrameCounter = 0;

            if (image_display.plot_realtime.Visible)
                image_display.plot_realtime.WarningTextDisplay("");


            if (focus)
            {
                focusing = true;
                EventNotify(this, new ProcessEventArgs("FocusStart", null));

                ChangeItemsStatus(false, true);
                FocusButton.Text = "ABORT";
            }
            else
            {
                ChangeItemsStatus(false, false);

                if (!looping)
                {
                    int error = CheckSavingParameters();
                    if (error < 0)
                    {
                        ChangeItemsStatus(true, false);
                        return;
                    }
                }

                if (use_motor)
                {
                    motorCtrl.GetPosition(true);
                    motorCtrl.continuousRead(false);
                }

                grabbing = true;
                focusing = false;
                EventNotify(this, new ProcessEventArgs("GrabStart", null));
                //EventNotify(this, new ProcessEventArgs("SliceAcquisitionStart", null));

                if (State.Acq.ZStack)
                {
                    if (BackToCenterCheck.Checked)
                    {
                        Set_Center_Click(Set_Center, null);
                        MoveMotorFromCenterToStart();
                    }

                    if (BackToStartCheck.Checked)
                    {
                        Set_Top_Click(Set_Top, null); //Set the start position.
                    }
                }

                State.Uncaging.uncage_whileImage = Uncage_while_image_check.Checked;

                if (State.Uncaging.uncage_whileImage && uncaging_panel != null)
                    uncaging_panel.SetupUncage(uncaging_panel);

                GrabButton.Text = "ABORT";

                if (analyzeEach.Checked)
                    image_display.plot_regular.Show();
                //RateTimer.Stop();

            }


            force_stop = false;


            FLIMSaveBuffer.Clear();
            acquiredTimeList.Clear();

            image_display.InitializeStripeBuffer(State.Acq.nChannels, State.Acq.linesPerFrame, State.Acq.pixelsPerLine);


            //FLIM_ImgData.InitializeData(State);
            //System.GC.SuppressFinalize(FLIM_ImgData);
            //FLIM_ImgData = new FLIMData(State);

            if (!focusing) //Setup FLIM_ImgData mode. ZStack? FastZ?
            {
                FLIM_ImgData.clearMemory();
                FLIM_ImgData.ZStack = (State.Acq.ZStack && State.Acq.nSlices > 1 && State.Acq.sliceStep > 0.0);
                FLIM_ImgData.nFastZ = State.Acq.fastZScan ? State.Acq.FastZ_nSlices : 1;
            }

            InitializeCounter(); //Counters Reset.

            UIstopWatch_Frame.Start();

            System.Threading.Thread.Sleep(10);

            bool[] eraseMemoryA = boolAllChannels(true);

            if (!focus)
            {
                //UIstopWatch.Reset();
                if (internalImageCounter == 0)
                    UIstopWatch_Loop.Reset();

                UIstopWatch_Image.Reset();


                StartDAQ(eraseMemoryA, true, true); //Including trigger.

                if (internalImageCounter == 0)
                {
                    UIstopWatch_Loop.Start();
                }
                UIstopWatch_Image.Start();

            }
            else
            {
                StartDAQ(eraseMemoryA, true, false); //Including trigger.
            }

            SW_PerformanceMonitor.Start();
        } //Start grab core.


        /// <summary>
        /// Called when Loop button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoopButton_Click(object sender, EventArgs e)
        {
            if (focusing)
                StopFocus();

            if (!looping && !grabbing)
            {
                StartLoop();
            }
            else
            {
                StopLoop();
            }
        }

        /// <summary>
        /// Start loop button function. Can be called from outside.
        /// </summary>
        private void StartLoop()
        {
            grabbing = true;
            looping = true;
            allowLoop = true; //This is the difference between loop and grab.
            stopGrabActivated = false;

            if (this.InvokeRequired)
                this.BeginInvoke((Action)delegate { LoopButton.Text = "STOP"; });
            else
                LoopButton.Text = "STOP";

            GrabButton.Enabled = false;
            internalImageCounter = 0;
            StartGrab(false);
        }

        private void StopLoop()
        {
            stopGrabActivated = true;
            StopGrab(true);


            if (this.InvokeRequired)
                this.BeginInvoke((Action)delegate
                {
                    LoopButton.Text = "LOOP";
                    if (!imageSequencing)
                        GrabButton.Enabled = true;
                });
            else
            {
                LoopButton.Text = "LOOP";
                if (!imageSequencing)
                    GrabButton.Enabled = true;
            }
            looping = false;
        }

        private void GrabButtonClick(object sender, EventArgs e)
        {
            if (focusing)
                StopFocus();

            if (!grabbing)
            {
                grabbing = true;
                looping = false;
                allowLoop = false;
                stopGrabActivated = false;
                StartGrab(false);
            }
            else
            {
                if (grabbing)
                {
                    EventNotify(this, new ProcessEventArgs("GrabAbort", null));
                }
                StopGrab(true);
            }
        }

        private void FocusButton_Click(object sender, EventArgs e)
        {
            if (!focusing)
            {
                stopGrabActivated = false;
                StartGrab(true);
            }
            else
            {
                StopFocus();
            }
        }

        public void uncagingShutterCtrl(bool ON, bool controlAO, bool controlDO)
        {
            if (State.Init.DO_uncagingShutter && controlDO)
            {
                UncagingShutter_DO_S.TurnOnOff(ON);
            }

            if (State.Init.AO_uncagingShutter && controlAO)
            {
                if (ON)
                    UncagingShutterAO.AO_putValue(5);
                else
                    UncagingShutterAO.AO_putValue(0);
            }
        }

        private void ResetFocus()
        {
            if (focusing)
            {
                StopFocus();
                System.Threading.Thread.Sleep(100);
                StartGrab(true);
            }
        }

        /// <summary>
        /// GUI --> State called when GUI is changed.
        /// </summary>
        /// <param name="sdr"></param>
        private void GetParametersFromGUI(object sdr)
        {
            double zoom;
            //
            if (sdr.Equals(Zoom100) || sdr.Equals(Zoom10) || sdr.Equals(Zoom1)
                || sdr.Equals(ZoomP1))
            {
                NumericUpDown sender = (NumericUpDown)sdr;

                if (ZoomP1.Value > 9)
                {
                    ZoomP1.Value = 0;
                    Zoom1.Value += 1;
                }

                if (Zoom1.Value > 9)
                {
                    Zoom1.Value = 0;
                    Zoom10.Value += 1;
                }

                if (Zoom10.Value > 9)
                {
                    Zoom10.Value = 0;
                    Zoom100.Value += 1;
                }

                if (Zoom100.Value > 9)
                    Zoom100.Value = 0;

                if (ZoomP1.Value < 0)
                {
                    if (Zoom1.Value > 0 || Zoom10.Value > 0 || Zoom100.Value > 0)
                    {
                        ZoomP1.Value = 9;
                        Zoom1.Value -= 1;
                    }
                    else
                        ZoomP1.Value = 0;
                }


                if (Zoom1.Value < 0)
                {
                    if (Zoom10.Value > 0 || Zoom100.Value > 0)
                    {
                        Zoom1.Value = 9;
                        Zoom10.Value -= 1;
                    }
                    else
                    {
                        Zoom1.Value = 0;
                    }
                }

                if (Zoom10.Value < 0)
                {
                    if (Zoom100.Value > 0)
                    {
                        Zoom10.Value = 9;
                        Zoom100.Value -= 1;
                    }
                    else
                    {
                        Zoom10.Value = 0;
                    }
                }

                if (Zoom100.Value < 0)
                {
                    Zoom100.Value = 0;
                }

                if (sender.Equals(Zoom10))
                {
                    if (Zoom10.Value == 0 && Zoom100.Value == 0)
                    {
                        // Zoom1.Value = 9;
                    }
                }
                else if (sender.Equals(Zoom100))
                {
                    if (Zoom100.Value == 0)
                    {
                        //Zoom10.Value = 9;
                    }
                }


                lock (syncStateObj)
                {
                    zoom = (double)Zoom100.Value * 100 + (double)Zoom10.Value * 10
                        + (double)Zoom1.Value + (double)ZoomP1.Value * 0.1;

                    if (zoom > 1)
                        State.Acq.zoom = zoom;
                    else
                    {
                        State.Acq.zoom = 1.0;
                        ZoomP1.Value = 0;
                        Zoom1.Value = 1;
                        Zoom10.Value = 0;
                        Zoom100.Value = 0;
                    }

                    Zoom.Text = string.Format("{0:0.0}", State.Acq.zoom);
                }

                ScanPosition.Invalidate();

                if (focusing)
                {
                    ResetFocus();
                }
            }
            else
            {
                //Make sure all text is reflected.
                lock (syncStateObj)
                {
                    //State.Acq.nChannels = 2;
                    double maxVoltage = 5;
                    double valD;
                    int valI;

                    if (!Double.TryParse(Zoom.Text, out zoom)) zoom = State.Acq.zoom;
                    if (zoom < 1000 && zoom >= 1)
                        State.Acq.zoom = zoom;
                    ScanPosition.Invalidate();

                    State.Acq.aveFrame = AveFrame_Check.Checked;
                    State.Acq.aveFrameA = new bool[] { AveFrame_Check.Checked, AveFrame2_Check.Checked };
                    State.Acq.aveSlice = AveSlices_check.Checked;
                    State.Acq.ZStack = ZStack_radio.Checked;
                    State.Acq.acqFLIMA[0] = FLIM_Radio1.Checked;
                    State.Acq.acqFLIMA[1] = FLIM_Radio2.Checked;
                    State.Acq.acquisition[0] = Acquisition1.Checked;
                    State.Acq.acquisition[1] = Acquisition2.Checked;
                    State.Files.channelsInSeparatedFile = SaveInSeparatedFileCheck.Checked;
                    FLIM_ImgData.KeepPagesInMemory = KeepPagesInMemoryCheck.Checked;
                    State.Acq.aveFrameSeparately = AveFrameSeparately.Checked;
                    State.Acq.externalTrigger = ExtTriggerCB.Checked;

                    Double msPerLine1, fillFraction1, ScanDelay1, scanFraction1;

                    State.Acq.BiDirectionalScan = BiDirecCB.Checked;
                    State.Acq.SineWaveScan = SineWaveScanning_CB.Checked;
                    State.Acq.flipXYScan[0] = FlipX_CB.Checked;
                    State.Acq.flipXYScan[1] = FlipY_CB.Checked;
                    State.Acq.switchXYScan = SwitchXY_CB.Checked;

                    if (sdr.Equals(MsPerLine))
                    {
                        if (!Double.TryParse(MsPerLine.Text, out msPerLine1)) msPerLine1 = State.Acq.msPerLine;
                        if (msPerLine1 >= 0.5)
                        {
                            State.Acq.msPerLine = msPerLine1;
                            State.Acq.msPerLine = Math.Round(10 * State.Acq.msPerLine) / 10.0;
                            //State.Acq.fillFraction = State.Acq.scanFraction - State.Acq.ScanDelay / State.Acq.msPerLine;
                        }
                    }

                    else if (sdr.Equals(ScanFraction))
                    {
                        if (!Double.TryParse(ScanFraction.Text, out scanFraction1)) scanFraction1 = State.Acq.scanFraction;

                        if (scanFraction1 > State.Acq.fillFraction && scanFraction1 > 0.5 && scanFraction1 < 0.99)
                        {
                            State.Acq.scanFraction = scanFraction1;
                            //State.Acq.ScanDelay = State.Acq.msPerLine * (State.Acq.scanFraction - State.Acq.fillFraction);
                        }
                    }

                    else if (sdr.Equals(ScanDelay))
                    {
                        if (!Double.TryParse(ScanDelay.Text, out ScanDelay1)) ScanDelay1 = State.Acq.ScanDelay;

                        if (ScanDelay1 >= 0 && ScanDelay1 < 0.5 * State.Acq.msPerLine)
                        {
                            State.Acq.ScanDelay = ScanDelay1;
                            //State.Acq.fillFraction = State.Acq.scanFraction - State.Acq.ScanDelay / State.Acq.msPerLine;
                        }
                    }

                    else if (sdr.Equals(FillFraction))
                    {
                        if (!Double.TryParse(FillFraction.Text, out fillFraction1)) fillFraction1 = State.Acq.fillFraction;
                        if (fillFraction1 < 0.99 && fillFraction1 > 0.5 && fillFraction1 < State.Acq.scanFraction)
                        {
                            State.Acq.fillFraction = fillFraction1;
                            //State.Acq.ScanDelay = State.Acq.msPerLine * (State.Acq.scanFraction - State.Acq.fillFraction);
                        }
                    }

                    if (Int32.TryParse(linesPerFrame.Text, out valI)) State.Acq.linesPerFrame = valI;

                    double maxX = State.Acq.XMaxVoltage;
                    double maxY = State.Acq.YMaxVoltage;

                    if (!Double.TryParse(MaxRangeX.Text, out maxX)) maxX = State.Acq.XMaxVoltage;
                    if (!Double.TryParse(MaxRangeY.Text, out maxY)) maxY = State.Acq.YMaxVoltage;

                    if (maxX <= State.Init.AbsoluteMaxVoltageScan && maxX >= 0)
                        State.Acq.XMaxVoltage = maxX;
                    if (maxY <= State.Init.AbsoluteMaxVoltageScan && maxY >= 0)
                        State.Acq.YMaxVoltage = maxY;


                    maxX = State.Acq.field_of_view[0];
                    maxY = State.Acq.field_of_view[1];
                    if (!Double.TryParse(FieldOfViewX.Text, out maxX)) maxX = State.Acq.field_of_view[0];
                    if (!Double.TryParse(FieldOfViewY.Text, out maxY)) maxY = State.Acq.field_of_view[1];
                    if (maxX <= 100000 && maxX >= 0)
                        State.Acq.field_of_view[0] = maxX;
                    if (maxY <= 100000 && maxY >= 0)
                        State.Acq.field_of_view[1] = maxY;

                    if (sdr.Equals(FieldOfViewX) || sdr.Equals(FieldOfViewY))
                        State.Acq.FOV_to_default();

                    double xOffset = State.Acq.XOffset;
                    double yOffset = State.Acq.YOffset;

                    if (!Double.TryParse(XOffset.Text, out xOffset)) xOffset = State.Acq.XOffset;
                    if (!Double.TryParse(YOffset.Text, out yOffset)) yOffset = State.Acq.YOffset;

                    if (xOffset < maxVoltage && xOffset > -maxVoltage)
                        State.Acq.XOffset = xOffset;

                    if (yOffset < maxVoltage && yOffset > -maxVoltage)
                        State.Acq.YOffset = yOffset;

                    if (Int32.TryParse(pixelsPerLine.Text, out valI)) State.Acq.pixelsPerLine = valI;
                    if (Double.TryParse(SliceInterval.Text, out valD)) State.Acq.sliceInterval = valD;
                    if (Double.TryParse(SliceStep.Text, out valD)) State.Acq.sliceStep = valD;
                    if (Double.TryParse(ImageInterval.Text, out valD)) State.Acq.imageInterval = valD;

                    if (Int32.TryParse(NFrames.Text, out valI)) State.Acq.nFrames = valI;


                    //if (sdr.Equals(NSlices2))
                    //{
                    //    if (Int32.TryParse(NSlices2.Text, out valI)) State.Acq.nSlices = valI;
                    //}
                    //else
                    //{
                    //    if (Int32.TryParse(NSlices.Text, out valI)) State.Acq.nSlices = valI;
                    //}

                    if (Int32.TryParse(NSlices.Text, out valI)) State.Acq.nSlices = valI;

                    if (State.Acq.nSlices < 1)
                        State.Acq.nSlices = 1;

                    if (Int32.TryParse(NImages.Text, out valI)) State.Acq.nImages = valI;

                    if (Int32.TryParse(N_AveragedFrames1.Text, out valI)) State.Acq.nAveragedFrames = valI;
                    if (Int32.TryParse(N_AveragedSlices.Text, out valI)) State.Acq.nAveragedSlices = valI;

                    if (Int32.TryParse(FileN.Text, out valI)) State.Files.fileCounter = valI;

                    State.Files.baseName = BaseName.Text;
                    State.Files.pathName = DirectoryName.Text;

                    if (Int32.TryParse(NumAve.Text, out valI)) State.Acq.nAveFrame = valI;
                    if (State.Acq.nAveFrame < 1)
                        State.Acq.nAveFrame = 1;
                    if (Int32.TryParse(N_AveSlices.Text, out valI)) State.Acq.nAveSlice = valI;
                    if (State.Acq.nAveSlice < 1)
                        State.Acq.nAveSlice = 1;

                    if (State.Acq.nAveragedSlices < 1)
                        State.Acq.nAveragedSlices = 1;

                    double xMultiplier = State.Acq.scanVoltageMultiplier[0];
                    double yMultiplier = State.Acq.scanVoltageMultiplier[1];

                    if (xMultiplier <= 1.0 && xMultiplier >= 0.0)
                        State.Acq.scanVoltageMultiplier[0] = xMultiplier;

                    if (yMultiplier <= 1.0 && yMultiplier >= 0.0)
                        State.Acq.scanVoltageMultiplier[1] = yMultiplier;

                    double angle = State.Acq.Rotation;
                    if (!Double.TryParse(Rotation.Text, out angle)) angle = State.Acq.Rotation;
                    if (angle <= 90 && angle >= -90)
                    {
                        State.Acq.Rotation = angle;
                    }

                    State.Acq.BiDirectionalScan = BiDirecCB.Checked;
                    State.Acq.SineWaveScan = SineWaveScanning_CB.Checked;
                    State.Acq.flipXYScan[0] = FlipX_CB.Checked;
                    State.Acq.flipXYScan[1] = FlipY_CB.Checked;
                    State.Acq.switchXYScan = SwitchXY_CB.Checked;

                    Double.TryParse(LineTimeCorrection.Text, out State.Spc.spcData.line_time_correction);

                    if (sdr.Equals(AveFrame_Check) && AveFrame_Check.Checked == false)
                    {
                        AveSlices_check.Checked = false;
                        State.Acq.aveSlice = false;
                    }

                    if (sdr.Equals(ZStack_radio))
                    {
                        //if (State.Acq.ZStack)
                        //{
                        //    State.Acq.aveSlice = false;
                        //    State.Acq.aveFrame = true;
                        //}
                    }
                    else if (sdr.Equals(AveSlices_check))
                    {
                        if (State.Acq.aveSlice)
                            State.Acq.ZStack = false;
                    }
                    else if (sdr.Equals(AveFrame_Check))
                    {
                        if (!State.Acq.aveFrame)
                        {
                            State.Acq.nAveragedFrames = State.Acq.nFrames;
                            //State.Acq.nAveFrame = 1;
                            State.Acq.ZStack = false;
                            State.Acq.aveSlice = false;
                        }
                    }
                    //else if (sdr.Equals(AveFrame2_Check))
                    //{
                    //    if (!State.Acq.aveFrameA[1])
                    //    {
                    //        State.Acq.ZStack = false;
                    //        State.Acq.aveSlice = false;
                    //    }
                    //}

                    //if (sdr.Equals(NumAve))
                    //{
                    //    if (State.Acq.nAveFrame > 1)
                    //        State.Acq.aveFrame = true;
                    //    else
                    //        State.Acq.aveFrame = false;
                    //}


                    if (State.Acq.ZStack)
                    {
                        State.Acq.sliceInterval = 0;
                        State.Acq.nAveSlice = 1;
                        State.Acq.aveSlice = false;
                    }

                    if (State.Acq.aveSlice)
                    {
                        if (sdr.Equals(NumAve))
                            State.Acq.nFrames = State.Acq.nAveFrame;
                        else
                            State.Acq.nAveFrame = State.Acq.nFrames;

                        State.Acq.aveFrame = true;
                        State.Acq.aveFrameA = Enumerable.Repeat(true, State.Acq.nChannels).ToArray();
                        State.Acq.ZStack = false;
                        State.Acq.nAveFrame = State.Acq.nFrames;
                        State.Acq.nAveragedFrames = 1;
                    }


                    if (sdr.Equals(N_AveragedSlices))
                    {
                        State.Acq.nSlices = State.Acq.nAveragedSlices * State.Acq.nAveSlice;
                    }

                    if (State.Acq.ZStack)
                        State.Acq.nAveSlice = 1;

                    if (State.Acq.nAveSlice > State.Acq.nSlices)
                        State.Acq.nAveSlice = State.Acq.nSlices;
                    else
                        State.Acq.nSlices = State.Acq.nAveSlice * (int)Math.Ceiling((double)State.Acq.nSlices / (double)State.Acq.nAveSlice);

                    State.Acq.nAveragedSlices = (int)Math.Ceiling((double)State.Acq.nSlices / (double)State.Acq.nAveSlice);


                    //State.Acq.linesPerStripe = 32;

                    //State.Acq.nStripes = (int)Math.Ceiling((double)State.Acq.linesPerFrame / (double)State.Acq.linesPerStripe);

                    //if (State.Acq.aveFrame)
                    //{
                    if (State.Acq.nAveFrame > State.Acq.nFrames)
                    {
                        if (sdr.Equals(NumAve))
                            State.Acq.nFrames = State.Acq.nAveFrame;
                        else
                            State.Acq.nAveFrame = State.Acq.nFrames;
                    }
                    else
                    {
                        if (sdr.Equals(NumAve))
                        {
                            State.Acq.nAveragedFrames = (int)Math.Ceiling((double)State.Acq.nFrames / (double)State.Acq.nAveFrame);
                            State.Acq.nFrames = State.Acq.nAveFrame * State.Acq.nAveragedFrames;
                        }
                        else if (sdr.Equals(N_AveragedFrames1))
                        {
                            State.Acq.nFrames = State.Acq.nAveFrame * State.Acq.nAveragedFrames;
                        }
                        else
                        {
                            double nAve = (double)State.Acq.nFrames / (double)State.Acq.nAveFrame;
                            if (nAve != Math.Round(nAve))
                            {
                                for (int i = State.Acq.nAveFrame; i > 0; i--)
                                {
                                    nAve = (double)State.Acq.nFrames / (double)i;
                                    if (nAve == Math.Round(nAve))
                                    {
                                        State.Acq.nAveFrame = i;
                                        break;
                                    }
                                }

                            }

                        }
                    } // else
                      //} //aveFrame                    

                    if (State.Acq.aveFrame)
                        State.Acq.nAveragedFrames = (int)((double)State.Acq.nFrames / (double)State.Acq.nAveFrame);
                    else
                    {
                        if (!sdr.Equals(NFrames))
                            State.Acq.nFrames = State.Acq.nAveragedFrames;
                        else
                            State.Acq.nAveragedFrames = State.Acq.nFrames;

                    }

                    State.Acq.SliceMergin = 0;

                    if (!State.Acq.ZStack)
                    {
                        if (State.Acq.sliceInterval < State.Acq.nFrames * State.Acq.linesPerFrame * State.Acq.msPerLine / 1000 + State.Acq.SliceMergin / 1000)
                            State.Acq.sliceInterval = State.Acq.nFrames * State.Acq.linesPerFrame * State.Acq.msPerLine / 1000 + State.Acq.SliceMergin / 1000;
                    }


                    State.Acq.nStripes = (int)Math.Ceiling((double)State.Acq.linesPerFrame / (double)State.Acq.nStripes);


                    //SPC
                    FillGUI();
                } //sync
            }

            SaveSetting();
        }

        public void PQ_SettingGUI()
        {
            int valI;
            double valD;

            if (Double.TryParse(sync_threshold.Text, out valD)) State.Spc.spcData.sync_threshold[0] = valD;
            if (Double.TryParse(sync_zc_level.Text, out valD)) State.Spc.spcData.sync_zc_level[0] = valD;
            if (Double.TryParse(sync_threshold2.Text, out valD)) State.Spc.spcData.sync_threshold[1] = valD;
            if (Double.TryParse(sync_zc_level2.Text, out valD)) State.Spc.spcData.sync_zc_level[1] = valD;
            if (Int32.TryParse(sync_offset.Text, out valI)) State.Spc.spcData.sync_offset = valI;

            if (Int32.TryParse(ch_offset1.Text, out valI)) State.Spc.spcData.ch_offset[0] = valI;
            if (Int32.TryParse(ch_offset2.Text, out valI)) State.Spc.spcData.ch_offset[1] = valI;
            if (Double.TryParse(ch_threshold1.Text, out valD)) State.Spc.spcData.ch_threshold[0] = valD;
            if (Double.TryParse(ch_threshold2.Text, out valD)) State.Spc.spcData.ch_threshold[1] = valD;
            if (Double.TryParse(ch_zc_level1.Text, out valD)) State.Spc.spcData.ch_zc_level[0] = valD;
            if (Double.TryParse(ch_zc_level2.Text, out valD)) State.Spc.spcData.ch_zc_level[1] = valD;

            for (int i = 0; i < State.Spc.spcData.sync_threshold.Length; i++)
            {
                if (State.Spc.spcData.sync_threshold[i] > 0)
                    State.Spc.spcData.sync_threshold[i] = -State.Spc.spcData.sync_threshold[i];
            }

            for (int i = 0; i < State.Spc.spcData.sync_zc_level.Length; i++)
            {
                if (State.Spc.spcData.sync_zc_level[i] > 0)
                    State.Spc.spcData.sync_zc_level[i] = -State.Spc.spcData.sync_zc_level[i];
            }

            for (int i = 0; i < State.Spc.spcData.ch_threshold.Length; i++)
            {
                if (State.Spc.spcData.ch_threshold[i] > 0)
                    State.Spc.spcData.ch_threshold[i] = -State.Spc.spcData.ch_threshold[i];
            }

            for (int i = 0; i < State.Spc.spcData.ch_zc_level.Length; i++)
            {
                if (State.Spc.spcData.ch_zc_level[i] > 0)
                    State.Spc.spcData.ch_zc_level[i] = -State.Spc.spcData.ch_zc_level[i];
            }

            if (fastZcontrol != null)
            {
                fastZcontrol.UpdateStateFromGUI(this);
            }
            else
                State.Acq.fastZScan = false;

            if (use_pq)
            {
                if (Binning_setting.SelectedIndex >= 0)
                    State.Spc.spcData.binning = Binning_setting.SelectedIndex;

                //if (Int32.TryParse(binning.Text, out valI)) State.Spc.spcData.binning = valI;
                if (Int32.TryParse(NTimePoints.Text, out valI)) State.Spc.spcData.n_dataPoint = valI;
                if (PQMode_Pulldown.SelectedIndex == 0)
                    State.Spc.spcData.acq_modePQ = 3;
                else if (PQMode_Pulldown.SelectedIndex == 1)
                    State.Spc.spcData.acq_modePQ = 2;
            }
            else if (use_bh)
            {
                if (Int32.TryParse(Resolution_Pulldown.SelectedItem.ToString(), out valI)) State.Spc.spcData.n_dataPoint = valI;
            }

            SetupFLIMParameters();
        }

        public void changeScanArea(ROI roi)
        {
            bool fixAspectRatio = true;
            double imageLength = Math.Max(roi.Rect.Height, roi.Rect.Width);

            double imageAspect = imageLength / (double)Math.Min(roi.Rect.Height, roi.Rect.Width);
            imageAspect = Math.Pow(2, (int)Math.Round(Math.Log(imageAspect, 2)));
            if (imageAspect > imageLength)
                imageAspect = imageLength;

            double imageLengthBefore = Math.Max(FLIM_ImgData.height, FLIM_ImgData.width);
            double xLoc = ((double)roi.Rect.Left + (double)roi.Rect.Width / 2) / FLIM_ImgData.width - 0.5; //in percentage.
            double yLoc = ((double)roi.Rect.Top + (double)roi.Rect.Height / 2) / FLIM_ImgData.height - 0.5; //in percentage.

            double ratio = imageLengthBefore / imageLength;


            int nPixelsMax = Math.Max(FLIM_ImgData.State.Acq.pixelsPerLine, FLIM_ImgData.State.Acq.linesPerFrame);

            if (fixAspectRatio)
            {
                State.Acq.pixelsPerLine = nPixelsMax;
                State.Acq.linesPerFrame = nPixelsMax;
            }
            else
            {
                if (roi.Rect.Height > roi.Rect.Width)
                {
                    State.Acq.pixelsPerLine = (int)(nPixelsMax / imageAspect); //width
                    State.Acq.linesPerFrame = nPixelsMax;
                }
                else
                {
                    State.Acq.pixelsPerLine = nPixelsMax;
                    State.Acq.linesPerFrame = (int)(nPixelsMax / imageAspect); //width
                }
            }

            xLoc = FLIM_ImgData.State.Acq.XMaxVoltage / FLIM_ImgData.State.Acq.zoom * xLoc;
            yLoc = FLIM_ImgData.State.Acq.YMaxVoltage / FLIM_ImgData.State.Acq.zoom * yLoc;

            State.Acq.XOffset = FLIM_ImgData.State.Acq.XOffset + xLoc;
            State.Acq.YOffset = FLIM_ImgData.State.Acq.YOffset + yLoc;

            double maxP = Math.Max(State.Acq.pixelsPerLine, State.Acq.linesPerFrame);

            if (!fixAspectRatio)
            {
                State.Acq.scanVoltageMultiplier[0] = (double)State.Acq.pixelsPerLine / maxP;
                State.Acq.scanVoltageMultiplier[1] = (double)State.Acq.linesPerFrame / maxP;
            }
            else
            {
                State.Acq.scanVoltageMultiplier[0] = 1;
                State.Acq.scanVoltageMultiplier[1] = 1;
            }

            State.Acq.zoom = (int)(FLIM_ImgData.State.Acq.zoom * ratio * 10) / 10;

            SetParametersFromState(true);
        }

        private void SetupFLIMParameters_ImageDelay()
        {
            parameters.SineWaveScan = State.Acq.SineWaveScan;
            parameters.BiDirectionalScan = State.Acq.BiDirectionalScan;
            double AcqusitionDelay = IOControls.GetAcquisitionDelay_ms(State);
            double BiDirectionalDelay = IOControls.GetBidirectionalDelay_ms(State);

            State.Acq.LineClockDelay = 0.05;

            parameters.BiDirectionalDelay = BiDirectionalDelay - State.Acq.LineClockDelay;
            parameters.AcquisitionDelay = AcqusitionDelay - State.Acq.LineClockDelay;
        }

        public void SetupFLIMParameters()
        {
            parameters.nDtime = State.Spc.spcData.n_dataPoint;
            parameters.nPixels = State.Acq.pixelsPerLine;
            parameters.nLines = State.Acq.linesPerFrame;
            parameters.msPerLine = State.Acq.msPerLine;
            parameters.averageFrame = (bool[])State.Acq.aveFrameA.Clone();
            parameters.n_average = State.Acq.nAveFrame;
            parameters.nFrames = State.Acq.nFrames;
            parameters.nChannels = State.Acq.nChannels;
            parameters.fastZScan.nFastZSlices = State.Acq.FastZ_nSlices;
            parameters.enableFastZscan = State.Acq.fastZScan;

            if (State.Acq.fastZScan)
                parameters.msPerLine = State.Acq.FastZ_msPerLine;


            SetupFLIMParameters_ImageDelay();

            //Perhaps not necessary..
            if (parameters.rateInfo.syncRate[0] != 0)
                parameters.spcData.time_per_unit = 1.0 / (double)parameters.rateInfo.syncRate[0]; //sync with laser pulses.
            else
                parameters.spcData.time_per_unit = 1.244e-8;

            if (use_pq && parameters.spcData.acq_modePQ == 2)
                parameters.spcData.time_per_unit = parameters.spcData.resolution[0] * 1.0e-12;
            // (calculated in pixel_count in TCSPC_control.dll)

            parameters.fastZScan.VoxelTimeUs = 1000.0 / parameters.fastZScan.FrequencyKHz / parameters.fastZScan.nFastZSlices;
            parameters.fastZScan.VoxelCount = (int)(parameters.fastZScan.VoxelTimeUs / parameters.spcData.time_per_unit / 1e6);


            parameters.eraseMemory_afterAcqisition = !(State.Acq.aveSlice && State.Acq.nSlices > 1);

            parameters.pixel_time = State.Acq.PixelTime();
            parameters.fillFraction = State.Acq.fillFraction;
            parameters.LinesPerStripe = State.Acq.linesPerStripe;
            parameters.nStripes = State.Acq.nStripes;

            if ((double)State.Acq.linesPerFrame * State.Acq.msPerLine > 255.0) //more than 4 Hz
                parameters.StripeDuringFocus = State.Acq.StripeDuringFocus;
            else
                parameters.StripeDuringFocus = false;

            parameters.acquireFLIM = State.Acq.acqFLIMA;
            parameters.acquisition = State.Acq.acquisition;

            if (FiFo_acquire != null)
                FiFo_acquire.SetupParameters(focusing, parameters);

            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)delegate
               {
                   UpdateSPC();
               });
            }
            else
                UpdateSPC();

        }

        /// <summary>
        /// State --> GUI.
        /// </summary>
        private void FillGUI()
        {
            AveFrame_Check.Checked = State.Acq.aveFrameA[0];
            AveFrame2_Check.Checked = State.Acq.aveFrameA[1];
            AveSlices_check.Checked = State.Acq.aveSlice;
            ZStack_radio.Checked = State.Acq.ZStack;
            Timelapse_radio.Checked = !ZStack_radio.Checked;
            FLIM_Radio1.Checked = State.Acq.acqFLIMA[0];
            FLIM_Radio2.Checked = State.Acq.acqFLIMA[1];
            Intensity_Radio1.Checked = !FLIM_Radio1.Checked;
            Intensity_Radio2.Checked = !FLIM_Radio2.Checked;
            Acquisition1.Checked = State.Acq.acquisition[0];
            Acquisition2.Checked = State.Acq.acquisition[1];
            ExtTriggerCB.Checked = State.Acq.externalTrigger;

            int nAve;
            if (State.Acq.aveFrame)
                nAve = State.Acq.nAveFrame;
            else
                nAve = 1;

            SliceInterval.Text = string.Format("{0:0.00}", State.Acq.sliceInterval);
            ImageInterval.Text = string.Format("{0:0.00}", State.Acq.imageInterval);
            FrameInterval.Text = string.Format("{0:0.000}", State.Acq.frameInterval());
            aveFrame_Interval.Text = string.Format("{0:0.000}", State.Acq.frameInterval() * nAve);

            if (State.Acq.aveSlice)
                nAve = State.Acq.nAveSlice;
            else
                nAve = 1;

            aveSlice_Interval.Text = string.Format("{0:0.000}", State.Acq.sliceInterval * nAve);
            SliceStep.Text = string.Format("{0:0.0}", State.Acq.sliceStep);

            Zoom.Text = string.Format("{0:0.0}", State.Acq.zoom);

            XOffset.Text = string.Format("{0:0.00}", State.Acq.XOffset);
            YOffset.Text = string.Format("{0:0.00}", State.Acq.YOffset);

            Rotation.Text = string.Format("{0:0.0}", State.Acq.Rotation);

            MaxRangeX.Text = String.Format("{0:0.0}", State.Acq.XMaxVoltage);
            MaxRangeY.Text = String.Format("{0:0.0}", State.Acq.YMaxVoltage);

            FormControllers.PulldownSelectByItemString(Objective_Pulldown, "x" + State.Acq.object_magnification);

            double[] fov = State.Acq.FOV_calculation(State.Acq.object_magnification);
            FieldOfViewX.Text = string.Format("{0:0.0}", State.Acq.field_of_view[0]);
            FieldOfViewY.Text = string.Format("{0:0.0}", State.Acq.field_of_view[1]);

            CurrentFOVX.Text = string.Format("{0:0.0}", fov[0]);
            CurrentFOVY.Text = string.Format("{0:0.0}", fov[1]);

            linesPerFrame.Text = Convert.ToString(State.Acq.linesPerFrame);

            if (State.Acq.msPerLine < 0)
                State.Acq.msPerLine = 1.0;

            MsPerLine.Text = Convert.ToString(State.Acq.msPerLine);
            pixelsPerLine.Text = Convert.ToString(State.Acq.pixelsPerLine);

            int index = NPixels_PulldownX.FindStringExact(State.Acq.pixelsPerLine.ToString());
            if (index < 0)
                index = NPixels_PulldownX.FindStringExact("128");
            NPixels_PulldownX.SelectedIndex = index;

            index = NPixels_PulldownY.FindStringExact(State.Acq.linesPerFrame.ToString());
            if (index < 0)
                index = NPixels_PulldownY.FindStringExact("128");
            NPixels_PulldownY.SelectedIndex = index;

            FillFraction.Text = string.Format("{0:0.000}", State.Acq.fillFraction);
            pixelTime.Text = string.Format("{0:0.000}", State.Acq.PixelTime() * 1e6);
            ScanDelay.Text = string.Format("{0:0.000}", State.Acq.ScanDelay);
            ScanFraction.Text = string.Format("{0:0.000}", State.Acq.scanFraction);

            NFrames.Text = State.Acq.nFrames.ToString();

            N_AveragedFrames1.Text = State.Acq.nAveragedFrames.ToString();

            if (State.Acq.aveFrameA[1])
                N_AveragedFrames2.Text = (State.Acq.nFrames / State.Acq.nAveFrame).ToString();
            else
                N_AveragedFrames2.Text = State.Acq.nFrames.ToString();

            N_AveragedSlices.Text = State.Acq.nAveragedSlices.ToString();

            NSlices.Text = State.Acq.nSlices.ToString();
            NSlices2.Text = State.Acq.nSlices.ToString();
            NImages.Text = State.Acq.nImages.ToString();
            NumAve.Text = State.Acq.nAveFrame.ToString();

            N_AveSlices.Text = State.Acq.nAveSlice.ToString();

            BiDirecCB.Checked = State.Acq.BiDirectionalScan;
            SineWaveScanning_CB.Checked = State.Acq.SineWaveScan;
            FlipX_CB.Checked = State.Acq.flipXYScan[0];
            FlipY_CB.Checked = State.Acq.flipXYScan[1];

            LineTimeCorrection.Text = String.Format("{0:0.0000}", State.Spc.spcData.line_time_correction);

            if (State.Acq.aveSlice)
            {
                Misc_about_Slice.Text = String.Format("Total Number of frames per page = {0}", State.Acq.nAveSlice * State.Acq.nFrames);
            }
            else
            {
                int nFrame = 1;
                if (State.Acq.aveFrameA[0])
                    nFrame = State.Acq.nAveFrame;
                Misc_about_Slice.Text = ""; // String.Format("Total Number of frames per page = {0}", nFrame);
                if (State.Acq.ZStack)
                    Misc_about_Slice.Text = "";
            }

            if (State.Acq.ZStack)
            {
                SliceInterval.Text = "NA";
            }

            SliceInterval.Visible = !State.Acq.ZStack;
            N_AveSlices.Visible = !State.Acq.ZStack;
            AveSlices_check.Visible = !State.Acq.ZStack;
            N_AveragedSlices.Visible = !State.Acq.ZStack;
            NAveragedSliceLabel.Visible = !State.Acq.ZStack;
            NAveSliceLabel.Visible = !State.Acq.ZStack;
            SliceIntervalLabel.Visible = !State.Acq.ZStack;

            TotalNFramesLabel1.Text = AveFrameSeparately.Checked ? "Total N frames (Ch1)" : "        Total N frames";
            TotalNFrames2Label.Visible = AveFrameSeparately.Checked;
            N_AveragedFrames2.Visible = AveFrameSeparately.Checked;
            AveFrame2_Check.Visible = AveFrameSeparately.Checked;

            if (fastZcontrol != null && fastZcontrol.Visible)
                fastZcontrol.CalculateFastZParameters();
            else
                State.Acq.fastZScan = false;

            //PQ Update.
            UpdateSPC();

            UpdateFileName();

            //This activates value change process.
            Zoom100.Value = Math.Floor((Decimal)State.Acq.zoom / 100);
            Zoom10.Value = Math.Floor(((Decimal)State.Acq.zoom - Zoom100.Value * 100) / 10);
            Zoom1.Value = Math.Floor(((Decimal)State.Acq.zoom - Zoom100.Value * 100 - Zoom10.Value * 10));
            ZoomP1.Value = Math.Floor(((Decimal)State.Acq.zoom - Math.Floor((Decimal)State.Acq.zoom)) * 10);

            UpdatePowerGUI();

            if (uncaging_panel != null)
                uncaging_panel.UpdateUncaging(this);

            Uncage_while_image_check.Checked = State.Uncaging.uncage_whileImage;

            ScanPosition.Invalidate();
        }

        private void UpdateSPC()
        {
            sync_threshold.Text = String.Format("{0:0.0}", State.Spc.spcData.sync_threshold[0]);
            sync_zc_level.Text = String.Format("{0:0.00}", State.Spc.spcData.sync_zc_level[0]);

            sync_threshold2.Text = String.Format("{0:0.0}", State.Spc.spcData.sync_threshold[1]);
            sync_zc_level2.Text = String.Format("{0:0.00}", State.Spc.spcData.sync_zc_level[1]);

            sync_offset.Text = String.Format("{0:0}", State.Spc.spcData.sync_offset);

            ch_offset1.Text = Convert.ToString(State.Spc.spcData.ch_offset[0]);
            ch_offset2.Text = Convert.ToString(State.Spc.spcData.ch_offset[1]);
            ch_threshold1.Text = String.Format("{0:0.00}", State.Spc.spcData.ch_threshold[0]);
            ch_threshold2.Text = String.Format("{0:0.00}", State.Spc.spcData.ch_threshold[1]);
            ch_zc_level1.Text = String.Format("{0:0.00}", State.Spc.spcData.ch_zc_level[0]);
            ch_zc_level2.Text = String.Format("{0:0.00}", State.Spc.spcData.ch_zc_level[1]);


            resolution.Text = String.Format("{0:0.0}", State.Spc.spcData.resolution[0]);
            resolution2.Text = String.Format("{0:0.0}", State.Spc.spcData.resolution[0]);

            if (parameters != null && fastZcontrol != null)
            {
                fastZcontrol.GetParamFromFLIMage(this);
                fastZcontrol.CalculateFastZParameters();
            }
            else
                State.Acq.fastZScan = false;

            if (use_bh)
            {
                resolution2.Text = String.Format("{0:0.0}", State.Spc.spcData.resolution[1]);

                int n_bit = (int)Math.Ceiling(Math.Log(State.Spc.spcData.n_dataPoint, 2));
                if (Binning_setting.Items.Count > n_bit - 6)
                    Resolution_Pulldown.SelectedIndex = n_bit - 6;

            }
            else if (use_pq)
            {
                NTimePoints.Text = Convert.ToString(State.Spc.spcData.n_dataPoint);

                if (Binning_setting != null && PQMode_Pulldown != null)
                {
                    if (Binning_setting.Items.Count > State.Spc.spcData.binning)
                        Binning_setting.SelectedIndex = State.Spc.spcData.binning;


                    if (State.Spc.spcData.acq_modePQ == 3)
                        PQMode_Pulldown.SelectedIndex = 0;
                    else
                        PQMode_Pulldown.SelectedIndex = 1;

                }
            }
        }

        /// <summary>
        /// Called when "Find path" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindPath_Click(object sender, EventArgs e)
        {
            //if (sender.Equals(FindPath))
            //{
            //    State.Files.AutoPathName();
            //    System.IO.Directory.CreateDirectory(State.Files.pathName);
            //    System.IO.Directory.CreateDirectory(State.Files.pathNameIntensity);
            //}

            FolderBrowserDialog folderBrowserDialog1;
            folderBrowserDialog1 = new FolderBrowserDialog();
            //folderBrowserDialog1.ShowNewFolderButton = true;
            //folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Personal;
            folderBrowserDialog1.SelectedPath = State.Files.pathName;
            folderBrowserDialog1.ShowNewFolderButton = true;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                State.Files.pathName = folderBrowserDialog1.SelectedPath;
                State.Files.pathNameIntensity = folderBrowserDialog1.SelectedPath + Path.DirectorySeparatorChar + "Intensity";
                State.Files.pathNameFLIM = folderBrowserDialog1.SelectedPath + Path.DirectorySeparatorChar + "FLIM";
            }
            UpdateFileName();
        }

        /// <summary>
        /// Actual function for Udate File Name. GUI is also updated.
        /// </summary>
        void UpdateFileNameCore()
        {
            bool must_separatedFile = (!State.Acq.aveFrameA.All(x => x == State.Acq.aveFrameA[0]) && State.Acq.nAveFrame > 1);
            if (must_separatedFile)
                State.Files.channelsInSeparatedFile = must_separatedFile;

            //SaveInSeparatedFileCheck.Enabled = !must_separatedFile;
            SaveInSeparatedFileCheck.Checked = State.Files.channelsInSeparatedFile;
            AveFrameSeparately.Checked = State.Acq.aveFrameSeparately;

            FileN.Text = Convert.ToString(State.Files.fileCounter);
            BaseName.Text = State.Files.baseName;
            DirectoryName.Text = State.Files.pathName;
            DirectoryName.Select(DirectoryName.TextLength + 1, 0);
            FullFileName.Text = State.Files.fullName();
            FullFileName.Select(FullFileName.TextLength + 1, 0);
            FileName.Text = State.Files.fileName;

            CurrentImage.Text = internalImageCounter.ToString();
        }

        /// <summary>
        /// When called from different thread, the function is invoekd from this thread.
        /// </summary>
        private void UpdateFileName()
        {
            if (this.InvokeRequired)
                this.BeginInvoke((Action)delegate
                {
                    UpdateFileNameCore();
                });
            else
                UpdateFileNameCore();
        }

        /// <summary>
        /// Enable/disable controls when focusing/grabbing.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="focus"></param>
        public void ChangeItemsStatus(bool status, bool focus)
        {
            //NFrames.Enabled = status;
            N_AveragedSlices.Enabled = status;
            N_AveragedFrames1.Enabled = status;

            NumAve.Enabled = status;
            N_AveSlices.Enabled = status;

            SliceInterval.Enabled = status;
            NFrames.Enabled = status;
            NSlices.Enabled = status;
            NImages.Enabled = status;
            Zoom.Enabled = status;
            NPixels_PulldownX.Enabled = status;
            NPixels_PulldownY.Enabled = status;
            linesPerFrame.Enabled = status;
            pixelsPerLine.Enabled = status;
            MsPerLine.Enabled = status;
            FillFraction.Enabled = status;
            ScanFraction.Enabled = status;
            ScanDelay.Enabled = status;
            BaseName.Enabled = status;
            DirectoryName.Enabled = status;
            FileN.Enabled = status;
            AdvancedCheck.Enabled = status;
            SnapShotButton.Enabled = status;

            AveFrame_Check.Enabled = status;
            AveFrame2_Check.Enabled = status;

            AveSlices_check.Enabled = status;
            ZStack_radio.Enabled = status;
            Timelapse_radio.Enabled = status;
            tb_Pparameters.Enabled = status;
            Calibrate1.Enabled = status;

            image_display.ChangeRealtimeStatus(!status);

            ChannelSettingTab.Enabled = status;
            tb_Pparameters.Enabled = status;
            tbScanParam.Enabled = status;

            //image_display.Enabled = status;

            if (status)
            {
                GrabButton.Enabled = true;
                FocusButton.Enabled = true;
                AdvancedCheck_CheckedChanged(AdvancedCheck, null);
            }

            if (focus && !status || imageSequencing)
                GrabButton.Enabled = false;
            else if (!focus && !status)
                FocusButton.Enabled = false;


        }

        private void AdvancedCheck_CheckedChanged(object sender, EventArgs e)
        {
            FillFraction.Enabled = AdvancedCheck.Checked;
            ScanFraction.Enabled = AdvancedCheck.Checked;
            ScanDelay.Enabled = AdvancedCheck.Checked;
            MaxRangeX.Enabled = AdvancedCheck.Checked;
            MaxRangeY.Enabled = AdvancedCheck.Checked;
            BiDirecCB.Enabled = AdvancedCheck.Checked;
            FlipX_CB.Enabled = AdvancedCheck.Checked;
            FlipY_CB.Enabled = AdvancedCheck.Checked;
            SwitchXY_CB.Enabled = AdvancedCheck.Checked;
            //PhaseDetecCB.Enabled = AdvancedCheck.Checked;
            LineTimeCorrection.Enabled = AdvancedCheck.Checked;
        }


        private void SetParametersFromState(bool notify)
        {
            FillGUI();
            GetParametersFromGUI(this);

            if (State.Uncaging.uncage_whileImage && uncaging_panel != null)
                uncaging_panel.Show();

            if (notify)
                EventNotify(this, new ProcessEventArgs("ParametersChanged", null));
        }

        public void LoadSettingFile(String fileName, bool loadScanSetting)
        {
            ScanParameters copyState = fileIO.CopyState();
            fileIO.LoadSetupFile(fileName);
            State = fileIO.State;

            //These values are unchanged by opening setup file from the menu.
            State.Init = copyState.Init;
            State.Spc = copyState.Spc;
            State.Motor = copyState.Motor;
            State.Files = copyState.Files;
            State.Display = copyState.Display;

            State.Uncaging.Position = copyState.Uncaging.Position;
            State.Uncaging.PositionV = copyState.Uncaging.PositionV;
            State.Uncaging.CalibV = copyState.Uncaging.CalibV;
            //State.Uncaging.UncagingPositionsVX = copyState.Uncaging.UncagingPositionsVX;
            //State.Uncaging.UncagingPositionsVY = copyState.Uncaging.UncagingPositionsVY;
            //State.Uncaging.UncagingPositionsX = copyState.Uncaging.UncagingPositionsX;
            //State.Uncaging.UncagingPositionsY = copyState.Uncaging.UncagingPositionsY;

            State.Acq.power = copyState.Acq.power;

            if (!loadScanSetting)
            {
                State.Acq.XOffset = copyState.Acq.XOffset;
                State.Acq.YOffset = copyState.Acq.YOffset;
                State.Acq.zoom = copyState.Acq.zoom;
                State.Acq.field_of_view = copyState.Acq.field_of_view;
                State.Acq.ScanDelay = copyState.Acq.ScanDelay;
                State.Acq.ScanDelay2 = copyState.Acq.ScanDelay2;
                State.Acq.ScanDelay4 = copyState.Acq.ScanDelay4;
                State.Acq.fillFraction = copyState.Acq.fillFraction;
                State.Acq.scanFraction = copyState.Acq.scanFraction;
                State.Acq.XMaxVoltage = copyState.Acq.XMaxVoltage;
                State.Acq.YMaxVoltage = copyState.Acq.YMaxVoltage;
                State.Acq.BiDirectionalScan = copyState.Acq.BiDirectionalScan;
                State.Acq.SineWaveScan = copyState.Acq.SineWaveScan;
                State.Acq.flipXYScan = copyState.Acq.flipXYScan;
                State.Acq.switchXYScan = copyState.Acq.switchXYScan;

                State.Acq.aveFrameA = copyState.Acq.aveFrameA;
                State.Acq.acquisition = copyState.Acq.acquisition;
                State.Acq.acqFLIMA = copyState.Acq.acqFLIMA;
                State.Acq.aveFrameSeparately = copyState.Acq.aveFrameSeparately;
            }
            //
            SetParametersFromState(true);
        }

        private void Calibrate1_Click(object sender, EventArgs e)
        {
            CalibEOM(true);
        }


        private void ImageLaser1_CheckedChanged(object sender, EventArgs e)
        {
            State.Init.imagingLasers[0] = ImageLaser1.Checked;
            State.Init.imagingLasers[1] = ImageLaser2.Checked;
            State.Init.imagingLasers[2] = ImageLaser3.Checked;
            State.Init.imagingLasers[3] = ImageLaser4.Checked;

            if (!State.Init.imagingLasers.Any(item => item == true))
                State.Init.imagingLasers[0] = true;

            State.Init.uncagingLasers[0] = UncageLaser1.Checked;
            State.Init.uncagingLasers[1] = UncageLaser2.Checked;
            State.Init.uncagingLasers[2] = UncageLaser3.Checked;
            State.Init.uncagingLasers[3] = UncageLaser4.Checked;

            UpdatePowerGUI();

            Uncage_while_image_check.Checked = State.Uncaging.uncage_whileImage;

            if (uncaging_panel != null && uncaging_panel.Visible)
                uncaging_panel.UpdateUncaging(this);
        }


        void Generic_MouseUp(object sender, MouseEventArgs e)
        {
            GetParametersFromGUI(sender);
        }



        /////////////////////////////// PRESET
        void pbPreset_general_Click(object sender, EventArgs e)
        {
            int preset = 128;
            if (sender.Equals(pb32))
                preset = 32;
            else if (sender.Equals(pb64))
                preset = 64;
            else if (sender.Equals(pb128))
                preset = 128;
            else if (sender.Equals(pb256))
                preset = 256;
            else if (sender.Equals(pb512))
                preset = 512;
            else if (sender.Equals(pb1024))
                preset = 1024;

            //double msPerLineValue = 1.0;

            State.Acq.linesPerFrame = preset;
            State.Acq.pixelsPerLine = preset;
            State.Acq.scanVoltageMultiplier = new double[] { 1, 1 };
            //State.Acq.msPerLine = msPerLineValue;
            SetParametersFromState(true);
        }

        /////////////////////////////// PRESET END

        void AveFrame_CheckedChanged(object sender, EventArgs e)
        {

        }

        void SaveSetting()
        {
            State.Spc.analysis = FLIM_ImgData.State.Spc.analysis;
            image_display.ExportStateDisplay(State);
            Directory.CreateDirectory(State.Files.initFolderPath);
            File.WriteAllText(State.Files.defaultInitFile, fileIO.AllSetupValues_nonDevice());
        }

        void SaveSettingAsNumber(int SettingNumber)
        {
            SettingFileN = SettingNumber;
            String fileName = String.Format("Setting-{0}.ini", SettingNumber);
            File.WriteAllText(Path.Combine(State.Files.initFolderPath, fileName), fileIO.AllSetupValues_nonDevice());
        }


        void LoadSettingAsNumber(int SettingNumber)
        {
            SettingFileN = SettingNumber;
            String fileName = String.Format("Setting-{0}.ini", SettingNumber);
            fileName = Path.Combine(State.Files.initFolderPath, fileName);
            if (File.Exists(fileName))
            {
                LoadSettingFile(fileName, false);
            }
        }


        void AddCurrentState_Image_Click(object sender, EventArgs e)
        {
            ScanParameters State1 = fileIO.CopyState();
            StateList.Add(State1);
            File.WriteAllText(StateFileName(), fileIO.AllSetupValues_nonDevice());
        }

        String StateFileName()
        {
            int nFiles = StateList.Count;
            return State.Files.pathName + "\\SetupFile" + nFiles + ".ini";
        }


        void FLIMControls_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindows();
            settingManagerSave();

            image_display.SaveSetting();

            Application.DoEvents();
            SaveSetting();

            if (lineClock != null)
                lineClock.dispose();

            if (mirrorAO_S != null)
                mirrorAO_S.dispose(); //parking mirror

            if (dioTrigger != null)
                dioTrigger.dispose();

            if (ShutterCtrl != null)
                ShutterCtrl.dispose();

            if (DI != null)
                DI.dispose();

            if (UncagingShutterAO != null)
                UncagingShutterAO.dispose();

            if (UncagingShutter_DO != null)
                UncagingShutter_DO.dispose();

            if (UncagingShutter_DO_S != null)
                UncagingShutter_DO_S.dispose();


            if (use_motor)
            {
                motorCtrl.unsubscribe();
                motorCtrl.MotH -= MotorListener;
            }

            if (use_pq || use_bh && runningImgAcq)
            {
                runningImgAcq = false;
                if (grabbing || focusing)
                    StopGrab(true);
                System.Threading.Thread.Sleep(50);
            }

            if (use_pq || use_bh)
            {
                RateTimer.Stop();
                RateTimer.Dispose();
                System.Threading.Thread.Sleep(50);

                if (use_pq)
                {
                    FiFo_acquire.VeryShortRun();
                    System.Threading.Thread.Sleep(50);
                    //FiFo_acquire.Initialize();
                }

                FiFo_acquire.closeDevice();
                Debug.WriteLine("****Closing devices...****");
                System.Threading.Thread.Sleep(50);
            }

            com_server.Close();

            //try
            //{
            //    Environment.Exit(0);
            //}
            //catch (System.ComponentModel.Win32Exception E)
            //{
            //    Debug.WriteLine(E.Message);
            //}
        }

        ////////////////////////////////
        ///COM server///
        ////////////////////////////////
        public void FLIM_EventHandling_Init()
        {
            com_server = new COMserver(this);
            text_server = new TextServer(this);
            flim_event = new FLIMage_Event(this);
        }


        public void ReSetupValues(bool notify)
        {
            this.Invoke((Action)delegate ()
            {
                SetParametersFromState(notify);
                Refresh();
            });
        }


        public bool ExternalCommand(String command)
        {
            return ExternalCommand(command, "");
        }

        public bool ExternalCommand(String command, String argument)
        {
            bool success = true;
            if (command == "StartGrab")
            {
                if (!grabbing && !focusing)
                {
                    stopGrabActivated = false;
                    grabbing = true;
                    allowLoop = false;
                    StartGrab(false);
                }
            }
            else if (command == "StartLoop")
            {
                StartLoop();
            }
            else if (command == "StopLoop")
            {
                StopLoop();
            }
            else if (command == "AbortGrab")
            {
                StopGrab(true);
            }
            else if (command == "StartUncaging")
            {
                this.BeginInvoke((Action)delegate
                {
                    image_display.uncaging_on = true;
                    image_display.Activate_uncaging(true);

                    if (!uncaging_panel.uncaging_running && !grabbing && !focusing)
                        uncaging_panel.StartUncaging_button_Click(uncaging_panel, null);
                });
            }
            else if (command == "UncagingStart")
            {
                EventNotify(this, new ProcessEventArgs("UncagingStart", null));
            }
            else if (command == "UncagingDone")
            {
                EventNotify(this, new ProcessEventArgs("UncagingDone", null));
            }
            else if (command == "LoadSettingWithNumber")
            {
                int defaultVal = 1;
                int argnumber = 1;
                if (!Int32.TryParse(argument, out argnumber)) argnumber = defaultVal;
                LoadSettingAsNumber(argnumber);
            }
            else if (command == "Focus")
            {
                FocusButton_Click(this, null);
            }
            else if (command == "SetMotorPosition")
            {
                SetMotorPosition(false, true);
            }
            else if (command == "UpdateGUI")
            {
                ReSetupValues(true);
            }
            else
            {
                success = false;
            }

            return success;
        }

        ////////////////////////////////
        ///Plotting///
        ////////////////////////////////


        void PlotScanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[,] DataXY = IOControls.MakeMirrorOutputXY(State);
            Plot plot2 = new Plot(DataXY, "Time (ms)", "Voltage (V)", State.Acq.outputRate, "Laser");
            plot2.Show();
        }


        void PlotPockelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[,] DataXY = IOControls.MakeMirrorOutputXY(State);
            double[,] DataEOM = IOControls.MakeEOMOutput(State, shading, false, false);
            Plot plot2 = new Plot(DataEOM, "Time (ms)", "Voltage (V)", State.Acq.outputRate, "Laser");
            plot2.Show();
        }


        /// <summary>
        /// Mirror position.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void ScanPosition_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(snapShotBMP,
                new Rectangle(0, 0, ScanPosition.Width, ScanPosition.Height), // destination rectangle 
                0, 0,           // upper-left corner of source rectangle
                snapShotBMP.Width,       // width of source rectangle
                snapShotBMP.Height,      // height of source rectangle
                GraphicsUnit.Pixel);

            float width = ScanPosition.Width;
            float height = ScanPosition.Height;
            double zoom = State.Acq.zoom;
            float XOffset = width * (float)(State.Acq.XOffset / State.Acq.XMaxVoltage);
            float YOffset = height * (float)(State.Acq.YOffset / State.Acq.YMaxVoltage);
            float rect_width = (float)(width / zoom * State.Acq.scanVoltageMultiplier[0]);
            float rect_height = (float)(height / zoom * State.Acq.scanVoltageMultiplier[1]);

            PointF[] polygon = new PointF[4];
            float[] x = new float[polygon.Length];
            float[] y = new float[polygon.Length];
            x[0] = -rect_width / 2F;
            y[0] = -rect_height / 2F;
            x[1] = x[0];
            y[1] = y[0] + rect_height;
            x[2] = x[0] + rect_width;
            y[2] = y[1];
            x[3] = x[2];
            y[3] = y[0];

            double theta = State.Acq.Rotation;
            double sinA = Math.Sin(theta / 180.0 * Math.PI);
            double cosA = Math.Cos(theta / 180.0 * Math.PI);
            for (int i = 0; i < polygon.Length; i++)
            {
                float x1 = (float)(x[i] * cosA - y[i] * sinA) + width / 2F + XOffset;
                float y1 = (float)(x[i] * sinA + y[i] * cosA) + height / 2F + YOffset;
                polygon[i] = new PointF(x1, y1);
            }

            Pen rectPen = new Pen(Color.White, (float)0.5);
            e.Graphics.DrawPolygon(rectPen, polygon);
        }



        ////////MIROR CONTRL

        void MirrorYUp_Click(object sender, EventArgs e)
        {
            double XYStep;
            if (!Double.TryParse(MirrorStep.Text, out XYStep)) XYStep = 0.1;
            double XStep = State.Acq.XMaxVoltage / State.Acq.zoom * XYStep;
            double YStep = State.Acq.YMaxVoltage / State.Acq.zoom * XYStep;
            if (sender.Equals(MirrorYUp))
                State.Acq.YOffset = State.Acq.YOffset - YStep;
            else if (sender.Equals(MirrorYDown))
                State.Acq.YOffset = State.Acq.YOffset + YStep;
            else if (sender.Equals(MirrorXUp))
                State.Acq.XOffset = State.Acq.XOffset + XStep;
            else if (sender.Equals(MirrorXDown))
                State.Acq.XOffset = State.Acq.XOffset - XStep;

            XOffset.Text = string.Format("{0:0.00}", State.Acq.XOffset);
            YOffset.Text = string.Format("{0:0.00}", State.Acq.YOffset);

            ScanPosition.Invalidate();
            SetParametersFromState(true);

            if (State.Init.UseExternalMirrorOffset)
            {
                IOControls.AO_Write aoX = new IOControls.AO_Write(State.Init.mirrorOffsetX, State.Acq.XOffset);
                IOControls.AO_Write aoY = new IOControls.AO_Write(State.Init.mirrorOffsetY, State.Acq.YOffset);
            }
        }



        /////////MOTOR CONTROL/////////////////////////////////////////////////


        void ContRead_CheckedChanged(object sender, EventArgs e)
        {
            motorCtrl.continuousRead(ContRead.Checked);
            motorCtrl.GetPosition(true);
            if (ContRead.Checked)
                motorCtrl.GetStatus(false);
            //motorCtrl.GetPosition();
        }

        void Motor_TextSet()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)delegate
                {
                    Motor_TextSetCore();
                });
            }
            else
                Motor_TextSetCore();
        }

        void Motor_TextSetCore()
        {
            double ZStep;
            if (!Double.TryParse(ZMotorStep.Text, out ZStep)) ZStep = motorCtrl.ZStep;
            double XYStep;
            if (!Double.TryParse(XYMotorStep.Text, out XYStep)) ZStep = motorCtrl.XYStep;

            double VValue;
            if (!Double.TryParse(Velocity.Text, out VValue)) VValue = motorCtrl.velocity[0];

            motorCtrl.ZStep = ZStep;
            motorCtrl.XYStep = XYStep;

            State.Motor.stepZ = ZStep;
            State.Motor.stepXY = XYStep;

            FillGUIMotor();
        }

        void FillGUIMotor()
        {
            XYMotorStep.Text = State.Motor.stepXY.ToString();
            ZMotorStep.Text = State.Motor.stepZ.ToString();
            Velocity.Text = State.Motor.velocity[0].ToString();
        }

        void MotorTextChanged(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tb = (TextBox)sender;
                String SaveText = tb.Text;
                try
                {
                    Motor_TextSet();
                }
                catch (System.FormatException)
                {
                    tb.Text = SaveText;
                }
                finally
                { };
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }


        void MotorReset_ButtonClick(object sender, EventArgs e)
        {
            turn_motorButtons(false);

            motorCtrl.reopen();

            turn_motorButtons(true);
        }


        void MotorHandler(MotrEventArgs e)
        {
            double minVal = motorCtrl.minMotorVal;
            //motorCtrl.XRefPos = motorCtrl.XPos;

            double[] motorRelativePosition = motorCtrl.getCalibratedRelativePosition();
            double[] motorAbsolutePosition = motorCtrl.getCalibratedAbsolutePosition();

            State.Motor.motorPosition = motorAbsolutePosition;

            if (show_relativePosition)
            {
                XRead.Text = String.Format("{0:0.00}", motorRelativePosition[0]);
                YRead.Text = String.Format("{0:0.00}", motorRelativePosition[1]);
                ZRead.Text = String.Format("{0:0.00}", motorRelativePosition[2]);
            }
            else
            {
                XRead.Text = String.Format("{0:0.00}", motorAbsolutePosition[0]);
                YRead.Text = String.Format("{0:0.00}", motorAbsolutePosition[1]);
                ZRead.Text = String.Format("{0:0.00}", motorAbsolutePosition[2]);
            }
            Velocity.Text = String.Format("{0}", motorCtrl.velocity[0]);

            if (motorCtrl.ZStackStart == int.MaxValue)
                AutoCalculateStackStartEnd();

            if (motorCtrl.ZStackStart != int.MaxValue)
                ZStart.Text = String.Format("{0:0.00}", (motorCtrl.ZStackStart - motorCtrl.ZRefPos) * motorCtrl.resolutionZ);
            else
                ZEnd.Text = "NA";

            if (motorCtrl.ZStackEnd != int.MaxValue)
                ZEnd.Text = String.Format("{0:0.00}", (motorCtrl.ZStackEnd - motorCtrl.ZRefPos) * motorCtrl.resolutionZ);
            else
                ZEnd.Text = "NA";

            if (motorCtrl.ZStackStart != int.MaxValue)
                ZCenter.Text = String.Format("{0:0.00}", (motorCtrl.ZStackCenter - motorCtrl.ZRefPos) * motorCtrl.resolutionZ);
            else
                ZCenter.Text = "NA";

            //ZStart.Text = String.Format("{0:0.00}", motorCtrl.ZStart_Rel() * motorCtrl.resolutionZ);
            //ZEnd.Text = String.Format("{0:0.00}", motorCtrl.ZEnd_Rel() * motorCtrl.resolutionZ);

            MotorStatus.Text = motorCtrl.tString;

            if (e.Name == "Moving")
            {
                Motor_Status.Text = "Moving...";
            }
            else
            {
                Motor_Status.Text = "...";
                turn_motorButtons(true);
            }

            if (e.Name == "Freeze")
            {
                XRead.Text = "NA";
                YRead.Text = "NA";
                ZRead.Text = "NA";
                motorCtrl.continuousRead(false);
                ContRead.Checked = false;
                ResetMotor.Visible = true;
            }

            if (e.Name == "MovementDone")
                EventNotify(this, new ProcessEventArgs("StageMoveDone", null));
            //this.Enabled = !motorCtrl.start_moving;

        }


        void MotorListener(MotorCtrl m, MotrEventArgs e)
        {
            try
            {
                this.Invoke((Action)delegate
                   {
                       MotorHandler(e);
                   });
            }
            catch (Exception ex)
            {

            }
        }

        void WaitForMotorMove()
        {
            if (use_motor)
            {
                motorCtrl.WaitUntilMovementDone();
            }
        }

        void MoveMotorStep(bool waitUntilFinish)
        {
            if (use_motor)
            {
                if (State.Acq.sliceStep != 0)
                {
                    motorCtrl.SetNewPosition_StepSize_um(new double[] { 0, 0, State.Acq.sliceStep });
                    motorCtrl.stack_Position = MotorCtrl.StackPosition.InStack;
                    SetMotorPosition(true, waitUntilFinish);
                }
            }
        }

        void MoveBackToHome()
        {
            if (State.Acq.ZStack && State.Acq.sliceStep != 0 && State.Acq.nSlices > 1)
            {
                if (BackToStartCheck.Checked)
                    MoveMotorBackToStart();
                else if (BackToCenterCheck.Checked)
                    MoveMotorBackToCenter();
            }
        }

        void MoveMotorBackToStart() //Called after acquisition.
        {
            if (use_motor)
            {
                if (State.Acq.nSlices > 1 && State.Acq.sliceStep != 0)// && motorCtrl.stack_Position != MotorCtrl.stackPosition.Start)
                {
                    if (motorCtrl.minMotorVal != motorCtrl.ZStackStart)
                    {
                        double[] current = motorCtrl.CurrentUncalibratedPosition();
                        current[2] = motorCtrl.ZStackStart;
                        motorCtrl.SetNewPosition(current);
                        motorCtrl.stack_Position = MotorCtrl.StackPosition.Start;
                        SetMotorPosition(false, true);
                    }
                }
            }
        }

        void MoveMotorBackToCenter()  //Called after acquisition.
        {
            if (use_motor)
            {
                if (State.Acq.nSlices > 1 && State.Acq.sliceStep != 0 && motorCtrl.stack_Position != MotorCtrl.StackPosition.Center)
                {
                    if (motorCtrl.minMotorVal != motorCtrl.ZStackStart)
                    {
                        double[] current = motorCtrl.CurrentUncalibratedPosition();
                        current[2] = motorCtrl.ZStackStart + (int)((double)(State.Acq.nSlices - 1) * (double)State.Acq.sliceStep / motorCtrl.resolutionZ / 2.0);
                        motorCtrl.SetNewPosition(current);
                        motorCtrl.stack_Position = MotorCtrl.StackPosition.Center;
                        SetMotorPosition(false, true);
                    }
                }
            }
        }


        void MoveMotorFromCenterToStart()
        {
            if (use_motor)
            {
                if (State.Acq.nSlices > 1 && State.Acq.sliceStep != 0)
                {
                    double[] current = motorCtrl.CurrentUncalibratedPosition();
                    motorCtrl.ZStackCenter = (int)current[2];
                    current[2] = current[2] - ZStackHalfStroke();
                    motorCtrl.ZStackStart = (int)current[2]; //Actually we should use calibrated value.. but anyway...
                    motorCtrl.ZStackEnd = motorCtrl.ZStackStart + ZStackHalfStroke() * 2;

                    motorCtrl.SetNewPosition(current);
                    motorCtrl.stack_Position = MotorCtrl.StackPosition.Start;

                    SetMotorPosition(false, true);
                }
            }
        }

        void AutoCalculateStackStartEnd()
        {
            double[] current = motorCtrl.CurrentUncalibratedPosition();
            if (current[2] != 0)
            {
                motorCtrl.ZStackStart = (int)(current[2] - ZStackHalfStroke());
                motorCtrl.ZStackCenter = (int)current[2];
                motorCtrl.ZStackEnd = (int)(current[2] + ZStackHalfStroke());
            }
        }

        int ZStackHalfStroke()
        {
            return (int)((double)(State.Acq.nSlices - 1) * (double)State.Acq.sliceStep / motorCtrl.resolutionZ / 2.0);
        }

        /// <summary>
        /// Set motor position to new position.
        /// </summary>
        public void SetMotorPosition(bool warningOn, bool waitUntilFinish)
        {
            if (waitUntilFinish)
                WaitForMotorMove(); //make sure that there is no task remaining.

            EventNotify(this, new ProcessEventArgs("StageMoveStart", null));

            //this.Enabled = false;
            motorCtrl.IfStepTooBig(warningOn);

            //if (StepSizeX != 0 || StepSizeY != 0 || StepSizeZ != 0)
            motorCtrl.SetPosition();

            if (waitUntilFinish) //For external command it will be always true.
            {
                WaitForMotorMove();
                motorCtrl.GetPosition(true);
                //
            }
        }

        public void MotroStepMovementXYZ(object sender, EventArgs e)
        {

            Motor_TextSet();

            if (motorQ.Any(item => item != 0))
            {
                System.Threading.Thread.Sleep(1);
            }

            lock (motorQlock)
            {
                if (sender.Equals(Zup))
                    motorQ[2] += motorCtrl.ZStep;
                else if (sender.Equals(Zdown))
                    motorQ[2] -= motorCtrl.ZStep;
                else if (sender.Equals(YUp))
                    motorQ[1] += motorCtrl.XYStep;
                else if (sender.Equals(YDown))
                    motorQ[1] -= motorCtrl.XYStep;
                else if (sender.Equals(XUp))
                    motorQ[0] += motorCtrl.XYStep;
                else if (sender.Equals(XDown))
                    motorQ[0] -= motorCtrl.XYStep;

                Debug.WriteLine("Set MotorZ position {0}", motorQ[2]);
            }

            motorCtrl.GetPosition(true);

            if (!motor_moving)
            {
                motor_moving = true;

                Task.Factory.StartNew(() =>
                {
                    while (motorQ.Any(item => item != 0))
                    {
                        lock (motorQlock)
                        {
                            Debug.WriteLine("Executing MotorZ position {0}", motorQ[2]);
                            motorCtrl.SetNewPosition_StepSize_um(motorQ);
                            motorQ = new double[3];
                        }
                        SetMotorPosition(true, true);
                    }


                    motor_moving = false;
                });
            }

        }

        private void MotorReadButton_Click(object sender, EventArgs e)
        {
            motorCtrl.GetPosition(true);
        }

        void Zero_Z_Click(object sender, EventArgs e)
        {
            motorCtrl.Zero_Z();
        }

        void Zero_all_Click(object sender, EventArgs e)
        {
            motorCtrl.Zero_All();
        }

        void CalcStackSize()
        {
            double stepSize; // = Convert.ToDouble(SliceStep.Text);
            if (!Double.TryParse(SliceStep.Text, out stepSize)) stepSize = 1.0;
            int nSlices = (int)Math.Ceiling(Math.Abs((double)(motorCtrl.ZStackStart - motorCtrl.ZStackEnd) * motorCtrl.resolutionZ / (double)stepSize)) + 1;
            if (motorCtrl.ZStackStart - motorCtrl.ZStackEnd > 0)
                stepSize = -Math.Abs(stepSize);
            else
                stepSize = Math.Abs(stepSize);

            NSlices.Text = nSlices.ToString();
            N_AveragedSlices.Text = nSlices.ToString();

            NSlices2.Text = nSlices.ToString();
            SliceStep.Text = stepSize.ToString();
            State.Acq.sliceStep = stepSize;
            State.Acq.nSlices = nSlices;
            State.Acq.ZStack = true;
        }

        void Go_Start()
        {
            double[] position = motorCtrl.CurrentUncalibratedPosition();
            position[2] = motorCtrl.ZStackStart;
            motorCtrl.SetNewPosition(position);
            SetMotorPosition(true, true);
            motorCtrl.stack_Position = MotorCtrl.StackPosition.Start;
            motorCtrl.GetPosition(true);
        }

        void GoStart_Click(object sender, EventArgs e)
        {
            if (use_motor)
                if (motorCtrl.ZStackStart != int.MaxValue && motorCtrl.ZStackEnd != int.MaxValue)
                {
                    Go_Start();
                }
        }

        void GoEnd_Click(object sender, EventArgs e)
        {
            if (use_motor)
                if (motorCtrl.ZStackStart > 0 && motorCtrl.ZStackEnd > 0)
                {
                    double[] position = motorCtrl.CurrentUncalibratedPosition();
                    position[2] = motorCtrl.ZStackEnd;
                    motorCtrl.SetNewPosition(position);
                    SetMotorPosition(true, true);
                    motorCtrl.stack_Position = MotorCtrl.StackPosition.End;
                }
        }

        private void GoCenterButton_Click(object sender, EventArgs e)
        {
            MoveMotorBackToCenter();
        }

        void Set_Top_Click(object sender, EventArgs e)
        {
            motorCtrl.GetPosition(true);
            double[] position = motorCtrl.CurrentUncalibratedPosition();
            motorCtrl.ZStackStart = (int)position[2];
            motorCtrl.ZStackCenter = motorCtrl.ZStackStart + ZStackHalfStroke();
            motorCtrl.ZStackEnd = motorCtrl.ZStackStart + 2 * ZStackHalfStroke();
            motorCtrl.stack_Position = MotorCtrl.StackPosition.Start;
            CalcStackSize();
            MotorHandler(new MotrEventArgs(""));
        }

        private void Set_Center_Click(object sender, EventArgs e)
        {
            AutoCalculateStackStartEnd();
            MotorHandler(new MotrEventArgs(""));
        }

        void Set_bottom_Click(object sender, EventArgs e)
        {
            if (motorCtrl.ZStackStart == motorCtrl.minMotorVal)
                MessageBox.Show("Please choose Start position first");
            else
            {
                motorCtrl.GetPosition(true);
                double[] position = motorCtrl.CurrentUncalibratedPosition();
                motorCtrl.ZStackEnd = (int)position[2];
                motorCtrl.stack_Position = MotorCtrl.StackPosition.End;
                CalcStackSize();

                motorCtrl.ZStackCenter = motorCtrl.ZStackStart + ZStackHalfStroke();
                MotorHandler(new MotrEventArgs(""));
            }
        }


        private void NSlices_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int valI = State.Acq.nSlices;
                if (Int32.TryParse(NSlices.Text, out valI)) State.Acq.nSlices = valI;
                NSlices.Text = valI.ToString();
                NSlices2.Text = valI.ToString();
                SetParametersFromState(true);
                MotorHandler(new MotrEventArgs(""));

                AutoCalculateStackStartEnd();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }


        private void ZStackParameterTextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tb = (TextBox)sender;
                String SaveText = tb.Text;
                try
                {
                    int valI = State.Acq.nSlices;
                    if (Int32.TryParse(NSlices2.Text, out valI)) State.Acq.nSlices = valI;
                    double valD = State.Acq.sliceStep;
                    if (Double.TryParse(SliceStep.Text, out valD)) State.Acq.sliceStep = valD;

                    State.Acq.ZStack = true;

                    if (sender.Equals(NSlices2) || sender.Equals(SliceStep))
                        AutoCalculateStackStartEnd();
                    else
                        CalcStackSize();

                    SetParametersFromState(true);
                    MotorHandler(new MotrEventArgs(""));
                }
                catch (System.FormatException)
                {
                    tb.Text = SaveText;
                }
                finally
                { };
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        public void turn_motorButtons(bool On)
        {
            Vel_Up.Enabled = On;
            Vel_Down.Enabled = On;
            XUp.Enabled = On;
            XDown.Enabled = On;
            YUp.Enabled = On;
            YDown.Enabled = On;
            Zup.Enabled = On;
            Zdown.Enabled = On;
            Zero_all.Enabled = On;
            zero_Z.Enabled = On;

        }

        public void Vel_Up_Click(object sender, EventArgs e)
        {
            motorCtrl.GetStatus(true);
            Motor_TextSet();

            turn_motorButtons(false);

            double vel_step = 100;
            if (sender.Equals(Vel_Down))
                vel_step = -100;

            double vel = motorCtrl.velocity[0];
            if (motorCtrl.velocity[0] + vel_step <= motorCtrl.maxVelocity[0])
                vel = vel + vel_step;

            //motorCtrl.GetStatus();
            motorCtrl.SetVelocity(new double[] { vel, vel, vel });
        }


        void CalibrateToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            CalibEOM(true);
        }

        void showCalibrationCurveToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Plot plot2 = new Plot(shading.calibration.calibrationCurve, "Power (%)", "Applied voltage (V)", 1000, "Laser");
            plot2.Show();
        }

        void showInputoutputCurveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Plot plot2 = new Plot(shading.calibration.calibrationCurve, shading.calibration.calibrationOutput, "Applied voltage (V)", "Photodiode voltage (V)", 1000, "Laser");
            plot2.Show();
        }



        void FLIMageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ss.ControlBox = true;
            ss.Show();
        }

        void Res_setting_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if (cb.SelectedIndex >= 0)
            {
                PQ_SettingGUI();
            }
        }

        void ImageDisplayOpen()
        {
            if (image_display == null || image_display.IsDisposed)
                image_display = new Image_Display(FLIM_ImgData, this, false);
            image_display.Show();

        }

        private void uncagingControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uncaging_panel == null || uncaging_panel.IsDisposed)
                uncaging_panel = new Uncaging_Trigger_Panel(this);
            uncaging_panel.Show();
            MenuItems_CheckControls();
        }

        void Image1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageDisplayOpen();
            MenuItems_CheckControls();
        }


        void NIDAQConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (nidaq_config == null || nidaq_config.IsDisposed)
                nidaq_config = new NIDAQ_Config(this);
            nidaq_config.Show();
            MenuItems_CheckControls();
        }


        private void dIOPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DIO_panel == null || DIO_panel.IsDisposed)
            DIO_panel = new DigitalSignalPanel(State);
            DIO_panel.Show();
            MenuItems_CheckControls();
        }

        void SnapShot_Click(object sender, EventArgs e)
        {
            snapShot = true;
            Save_State = fileIO.CopyState();
            State.Acq.zoom = 1;
            State.Acq.nSlices = 1;
            State.Acq.nFrames = 1;
            State.Acq.nImages = 1;
            State.Acq.XOffset = 0;
            State.Acq.YOffset = 0;
            int NPixels = Math.Max(State.Acq.linesPerFrame, State.Acq.pixelsPerLine);
            State.Acq.linesPerFrame = NPixels;
            State.Acq.pixelsPerLine = NPixels;
            State.Acq.scanVoltageMultiplier[0] = 1;
            State.Acq.scanVoltageMultiplier[1] = 1;
            SetParametersFromState(true);
            FocusButton_Click(this, null);
        }

        //////////////////////////////////////////UNCAGING//////////////////////////////////////////////////////////////////        

        public void UpdateUncagingFromDisplay()
        {
            if (uncaging_panel.InvokeRequired && uncaging_panel.Visible)
            {
                uncaging_panel.BeginInvoke((Action)delegate
                {
                    if (uncaging_panel != null)
                        uncaging_panel.UpdateUncaging(uncaging_panel);
                });
            }
            else
            {
                if (uncaging_panel != null && uncaging_panel.Visible)
                    uncaging_panel.UpdateUncaging(uncaging_panel);
            }
        }



        void PlotScanGrabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[,] DataXY = IOControls.makeMirrorOutput_Imaging_Uncaging(State);
            if (uncaging_PlotXY_Scanning == null || uncaging_PlotXY_Scanning.IsDisposed)
            {
                uncaging_PlotXY_Scanning = new Plot(DataXY, "Time (ms)", "Voltage (V)", State.Acq.outputRate, "Output Ch");
                uncaging_PlotXY_Scanning.setPlotTitle("Uncaging and Scanning Mirror Channels");
                //uncaging_plotPockels.setPlotTitle("Uncaging and scanning mirror channels");
                uncaging_PlotXY_Scanning.Show();
            }
            else
            {
                uncaging_PlotXY_Scanning.Replot(DataXY, "Time (ms)", "Voltage (V)", State.Acq.outputRate, "Output Ch");
            }
        }

        private void Uncage_while_image_check_Click(object sender, EventArgs e)
        {
            State.Uncaging.uncage_whileImage = Uncage_while_image_check.Checked;

            if (State.Uncaging.uncage_whileImage)
            {
                if (uncaging_panel == null)
                {
                    uncaging_panel = new Uncaging_Trigger_Panel(this);
                }
                uncaging_panel.Show();
            }

            if (uncaging_panel != null)
            {
                uncaging_panel.SetupUncage(uncaging_panel);
            }
        }


        private void Numbered_Setting_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sp = (ToolStripMenuItem)sender;

            for (int i = 0; i < 3; i++)
            {
                if (sp.Name.Contains("loadSetting" + (i + 1)))
                {
                    LoadSettingAsNumber(i + 1);
                }
                else if (sp.Name.Contains("saveSetting" + (i + 1)))
                {
                    SaveSettingAsNumber(i + 1);
                }
            }
        }

        private void ResetMotor_Click(object sender, EventArgs e)
        {
            ResetMotor.Visible = false;
            motorCtrl.GetStatus(false);
        }

        private void ZeroMirror_Click(object sender, EventArgs e)
        {
            State.Acq.XOffset = 0;
            State.Acq.YOffset = 0;
            SetParametersFromState(true);
        }

        private void MaxScanning_Click(object sender, EventArgs e)
        {
            State.Acq.XOffset = 0;
            State.Acq.YOffset = 0;
            State.Acq.zoom = 1;
            State.Acq.scanVoltageMultiplier[0] = 1;
            State.Acq.scanVoltageMultiplier[1] = 1;
            int NPixels = Math.Max(State.Acq.linesPerFrame, State.Acq.pixelsPerLine);
            State.Acq.linesPerFrame = NPixels;
            State.Acq.pixelsPerLine = NPixels;
            SetParametersFromState(true);
            SetParametersFromState(true);
        }


        private void NPixel_PullDown_ValueChaned(object sender, EventArgs e)
        {
            if (sender.Equals(NPixels_PulldownX))
                State.Acq.pixelsPerLine = Convert.ToInt32(NPixels_PulldownX.SelectedItem);
            else if (sender.Equals(NPixels_PulldownY))
                State.Acq.linesPerFrame = Convert.ToInt32(NPixels_PulldownY.SelectedItem);
            else
                return;

            int nPixels = Math.Max(State.Acq.pixelsPerLine, State.Acq.linesPerFrame);

            State.Acq.scanVoltageMultiplier[0] = (double)State.Acq.pixelsPerLine / (double)nPixels;
            State.Acq.scanVoltageMultiplier[1] = (double)State.Acq.linesPerFrame / (double)nPixels;

            SetParametersFromState(false);
        }

        private void ZeroAngle_Click(object sender, EventArgs e)
        {
            State.Acq.Rotation = 0;
            SetParametersFromState(true);
        }

        private void Relative_Click(object sender, EventArgs e)
        {
            show_relativePosition = Relative.Checked;
            motorCtrl.GetPosition(true);
        }

        private void Velocity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tb = (TextBox)sender;
                String SaveText = tb.Text;
                try
                {
                    int vel = 1500;
                    Int32.TryParse(Velocity.Text, out vel);
                    if (vel > 100 && vel < 10000)
                    {
                        State.Motor.velocity[0] = vel;
                    }

                    motorCtrl.SetVelocity(State.Motor.velocity);
                }
                catch (System.FormatException)
                {
                    tb.Text = SaveText;
                }
                finally
                { };
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }




        private void saveSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileIO.SaveSetupFile();
        }

        private void loadScanParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String filename = fileIO.OpenGetSetupFileName();
            LoadSettingFile(filename, true);
            SetParametersFromState(true);
        }

        private void LoadSttingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String filename = fileIO.OpenGetSetupFileName();
            LoadSettingFile(filename, false);
            SetParametersFromState(true);
        }

        private void shadingCorretionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (shading_correction == null || shading_correction.IsDisposed)
                shading_correction = new ShadingCorrection(this);
            shading_correction.Show();
        }

        private void driftCorrectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (drift_correction == null || drift_correction.IsDisposed)
                drift_correction = new DriftCorrection(this);
            drift_correction.Show();
        }


        private void fastZControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fastZcontrol == null || fastZcontrol.IsDisposed)
                fastZcontrol = new FastZControl(this);

            fastZcontrol.Show();
            MenuItems_CheckControls();
        }

        private void imageSeqControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (image_seqeunce == null || image_seqeunce.IsDisposed)
                image_seqeunce = new Image_seqeunce(this);
            image_seqeunce.Show();
            MenuItems_CheckControls();
        }

        private void realtimePlotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            image_display.plot_realtime.Show();
            MenuItems_CheckControls();
        }

        private void remoteControlToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (script == null || script.IsDisposed)
                script = new Script(this);
            script.Show();
            MenuItems_CheckControls();
        }


        private void pMTControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pmt_control == null || pmt_control.IsDisposed)
                pmt_control = new PMTControl(State.Files.initFolderPath);
            pmt_control.Show();
            pmt_control.Activate();
            MenuItems_CheckControls();
        }


        private void stageControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (motor_ctrl_panel == null || motor_ctrl_panel.IsDisposed)
                motor_ctrl_panel = new MotorCtrlPanel(this, motorCtrl);
            motor_ctrl_panel.Show();
            motor_ctrl_panel.Activate();
            MenuItems_CheckControls();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AverageFrame_ValueChanged(object sender, EventArgs e)
        {
            if (!AveFrameSeparately.Checked)
            {
                if (sender.Equals(AveFrame_Check))
                    AveFrame2_Check.Checked = AveFrame_Check.Checked;
                else if (sender.Equals(AveFrame2_Check))
                    AveFrame_Check.Checked = AveFrame2_Check.Checked;
            }

            Generic_ValueChanged(sender, e);
        }

        private void FLIMcontrols_Shown(object sender, EventArgs e)
        {
            if (!use_mainPanel)
                this.WindowState = FormWindowState.Minimized;

        }

        private void Objective_Pulldown_SelectedIndexChanged(object sender, EventArgs e)
        {
            var str = Objective_Pulldown.SelectedItem.ToString();
            str = str.Replace("x", "");
            State.Acq.FOV_calculation(Convert.ToInt32(str));
            SetParametersFromState(false);
            if (fastZcontrol != null && fastZcontrol.Visible)
                fastZcontrol.PresetCalculator(); //This will induce all calculation.
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuItems_CheckControls();
        }

        void PlotPockelsGrabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[,] DataXY = IOControls.makeEOMOutput_Imaging_Uncaging(State, shading);

            if (uncaging_PlotPockels_Scanning == null || uncaging_PlotPockels_Scanning.IsDisposed)
            {
                uncaging_PlotPockels_Scanning = new Plot(DataXY, "Time (ms)", "Voltage (V)", State.Acq.outputRate, "Output Ch");
                if (State.Init.AO_uncagingShutter)
                {
                    uncaging_PlotPockels_Scanning.setPlotTitle("Uncaging and scanning Pockels cells: last channel = shutter");
                }
                else
                    uncaging_plotPockels.setPlotTitle("Uncaging Pockels cells");
                uncaging_PlotPockels_Scanning.Show();
            }
            else
            {
                uncaging_PlotPockels_Scanning.Replot(DataXY, "Time (ms)", "Voltage (V)", State.Acq.outputRate, "Output Ch");
            }
        }


    }


}
