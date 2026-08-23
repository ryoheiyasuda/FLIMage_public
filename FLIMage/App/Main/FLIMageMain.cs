using FLIMage.Analysis;
using FLIMage.FileFormat;
using FLIMage.FlowControls;
using FLIMage.HardwareControls;
using FLIMage.HardwareControls.StageControls;
using FLIMage.Plotting;
using FLIMage.Uncaging;
using MathLibrary;
using MicroscopeHardwareLibs.Stage_Contoller;
using PhysiologyCSharp;
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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilites;
using Utilities;
using static FLIMage.FlowControls.FLIMage_Event;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;

namespace FLIMage
{
    public partial class FLIMageMain : Form
    {
        //FLIMage_IO --- main IO control.
        public static int PH330_Binning_Offset = 5;
        public static int HH500_Binning_Offset = 5;

        public FLIMage_IO flimage_io;
        int SettingFileN = 1;

        public bool saveIntensityImage = false;
        public bool use_motor = true; // use motor control.
        public bool use_piezo = false;
        public bool use_mainPanel = true; //For analysis only

        public Bitmap snapShotBMP;

        public bool analyzeAfterEachAcquisition = false;

        public ScanParameters State;

        public FileIO fileIO;

        ////// Motor parameters.
        public MotorCtrl motorCtrl;
        delegate void motorHandleFunctoin(); //Event triggered motor handling.

        public bool motor_moving = false;
        public double[] motorQ = new double[3];
        public bool show_relativePosition = true;

        object motorQlock = new object();

        ////// Other windows.
        static splashScreen ss = new splashScreen();
        public Image_Display image_display;

        public NIDAQ_Config nidaq_config;
        public MotorCtrlPanel motor_ctrl_panel;

        public Uncaging_Trigger_Panel uncaging_panel;
        public Digital_Trigger_Panel digital_panel;

        public ShadingCorrection shading_correction;
        public DigitalSignalPanel DIO_panel;
        public PMTControl pmt_control;
        public FastZControl fastZcontrol;
        public StimPanel physiology;
        public PhasorPlotWindow phasor_plot_window;
        public ZoZoMotorControl zozo_motor_ctrl;

        public Dialogs.ScanAreaWindow scanAreaWindow;

        public Plot uncaging_plotXY;
        public Plot uncaging_plotPockels;
        public Plot uncaging_PlotXY_Scanning;
        public Plot uncaging_PlotPockels_Scanning;

        //
        List<ScanParameters> StateList = new List<ScanParameters>(); //Not made yet.
        public COMserver com_server;
        public TextServer text_server;
        public FLIMage_Event flim_event;
        public RemoteControl script;
        public String versionText;

        //File watcher for method
        private FileSystemWatcher watcher;
        private String watchFile = "COM_method.txt";
        private String watchDirName = "COM";
        private String watchDir;
        private bool busyFile = false;

        public bool motor_back_to_start = false;
        public bool motor_back_to_center = true;
        public bool motor_stay = false;

        // Z stack synchronization object to prevent race conditions
        private object zStackSyncLock = new object();
        private bool zStackMovementInProgress = false;

        public int current_splitScanLocation = 0;

        public ROI Roi;
        public ROI[] ROIs;

        //// Setting manager
        String settingName = "FLIMageWindow";
        SettingManager settingManager;

        String[] plugin_names;
        ToolStripMenuItem[] plugin_tool_bar_hanels;
        object[] plugin_object;

        ZoomDataResonant resonant_setting;

        // Tools -> Scan calibration menu items
        private ToolStripMenuItem scanCalibrationToolStripMenuItem;
        private ToolStripMenuItem quickAutoScanDelayEvenOddToolStripMenuItem;

        private bool ShouldHoldFastWriterOpen()
        {
            if (State?.Files?.fastSaving ?? false)
                return true;
            if (State?.Acq?.fiberPhotometryMode ?? false)
                return true;

            var system = State?.Init?.MicroscopeSystem;
            return !string.IsNullOrEmpty(system)
                && system.IndexOf("fiber", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool CanUseOmeTiff(out string reason)
        {
            bool isFiber = State?.Acq?.fiberPhotometryMode == true
                || flimage_io?.microscope_system == MicroscopeSystem.FiberPhotometry;
            if (isFiber)
            {
                reason = "OME-TIFF saving is disabled for fiber photometry.";
                return false;
            }

            int width = State?.Acq?.pixelsPerLine ?? 0;
            int height = State?.Acq?.linesPerFrame ?? 0;
            if (width <= 0 || height <= 0)
            {
                reason = "OME-TIFF saving requires a non-zero image size.";
                return false;
            }

            reason = "";
            return true;
        }

        private void ApplyOmeTiffSetting(bool requestEnable, bool showMessage)
        {
            bool enable = requestEnable;
            if (requestEnable && !CanUseOmeTiff(out string reason))
            {
                enable = false;
                if (showMessage && !string.IsNullOrEmpty(reason))
                    MessageBox.Show(reason, "OME-TIFF", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            string newExtension = enable ? State.Files.extension_ome : ".flim";
            bool changed = State.Files.useOmeTiff != enable || State.Files.extension != newExtension;

            State.Files.useOmeTiff = enable;
            State.Files.extension = newExtension;

            if (UseOmeTiff_CB != null && UseOmeTiff_CB.Checked != enable)
                UseOmeTiff_CB.Checked = enable;

            if (image_display?.FLIM_ImgData?.State?.Files != null)
            {
                image_display.FLIM_ImgData.State.Files.useOmeTiff = State.Files.useOmeTiff;
                image_display.FLIM_ImgData.State.Files.extension = State.Files.extension;
            }
            if (flimage_io?.State?.Files != null)
            {
                flimage_io.State.Files.useOmeTiff = State.Files.useOmeTiff;
                flimage_io.State.Files.extension = State.Files.extension;
            }

            if (changed)
                UpdateFileName();
        }
        private ToolStripMenuItem autoResonantScanDelayEvenOddToolStripMenuItem;

        private Panel fiberScanPanel;
        private TextBox fiberSamplesPerFrame;
        private TextBox fiberFrameCount;
        private TextBox fiberSampleInterval;
        private Label fiberSamplesPerFrameLabel;
        private Label fiberFrameCountLabel;
        private Label fiberSampleIntervalLabel;
        private bool scanPanelVisibilityCached;
        private readonly Dictionary<Control, bool> scanPanelVisibility = new Dictionary<Control, bool>();
        private int scanParametersTabIndex = -1;
        private bool scanParametersTabRemoved;

        public string version;

        public FLIMageMain()
        {
            InitializeComponent();
            AddScanCalibrationMenuItems();
            EnsureFiberScanPanel();
            plugin_names = Assembly.GetExecutingAssembly().GetTypes()
                                  .Where(t => t.Namespace == "FLIMage.Plugins")
                                  .Where(t => t.BaseType.Name == "Form")
                                  .Select(t => t.FullName).ToArray();
            plugin_object = new object[plugin_names.Length];
            plugin_tool_bar_hanels = new ToolStripMenuItem[plugin_names.Length];
            for (int i = 0; i < plugin_names.Length; i++)
            {
                var plugin = plugin_names[i];
                var names = plugin.Split('.');
                var name1 = names[names.Length - 1];
                var name = name1.Replace("_", " ");
                var handle1 = new ToolStripMenuItem();
                this.ToolsToolStripMenuItem.DropDownItems.Add(handle1);
                handle1.Name = name1;
                handle1.Size = new Size(192, 22);
                handle1.Text = name;
                handle1.Click += new System.EventHandler(PluginHandle_Click);
                plugin_tool_bar_hanels[i] = handle1;
            }
        }

        private void AddScanCalibrationMenuItems()
        {
            try
            {
                if (ToolsToolStripMenuItem == null)
                    return;

                // Submenu to keep acquisition calibrations grouped (separate from "Power" calibration).
                scanCalibrationToolStripMenuItem = new ToolStripMenuItem
                {
                    Name = "scanCalibrationToolStripMenuItem",
                    Text = "Scan calibration",
                };

                autoResonantScanDelayEvenOddToolStripMenuItem = new ToolStripMenuItem
                {
                    Name = "autoResonantScanDelayEvenOddToolStripMenuItem",
                    Text = "Auto scan delay (even/odd)...",
                };
                autoResonantScanDelayEvenOddToolStripMenuItem.Click += AutoResonantScanDelayEvenOddToolStripMenuItem_Click;

                quickAutoScanDelayEvenOddToolStripMenuItem = new ToolStripMenuItem
                {
                    Name = "quickAutoScanDelayEvenOddToolStripMenuItem",
                    Text = "Quick auto scan delay (even/odd)",
                };
                quickAutoScanDelayEvenOddToolStripMenuItem.Click += QuickAutoScanDelayEvenOddToolStripMenuItem_Click;

                scanCalibrationToolStripMenuItem.DropDownItems.Add(quickAutoScanDelayEvenOddToolStripMenuItem);
                scanCalibrationToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
                scanCalibrationToolStripMenuItem.DropDownItems.Add(autoResonantScanDelayEvenOddToolStripMenuItem);

                // Insert before the separator that precedes plugins, if present.
                int insertIdx = -1;
                try { insertIdx = ToolsToolStripMenuItem.DropDownItems.IndexOf(toolStripMenuItem1); } catch { insertIdx = -1; }
                if (insertIdx < 0) insertIdx = ToolsToolStripMenuItem.DropDownItems.Count;

                ToolsToolStripMenuItem.DropDownItems.Insert(insertIdx, scanCalibrationToolStripMenuItem);
            }
            catch
            {
                // Menu convenience only; ignore failures.
            }
        }

        private async void AutoResonantScanDelayEvenOddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var mi = sender as ToolStripMenuItem;
            if (mi != null) mi.Enabled = false;

            try
            {
                if (image_display == null || image_display.IsDisposed)
                {
                    MessageBox.Show("Image display is not available yet.");
                    return;
                }

                // Preferred usage: run during focusing on beads/grid.
                await image_display.RunAutoResonantDelayEvenOddAsync();
            }
            finally
            {
                if (mi != null) mi.Enabled = true;
            }
        }

        private async void QuickAutoScanDelayEvenOddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var mi = sender as ToolStripMenuItem;
            if (mi != null) mi.Enabled = false;

            try
            {
                if (image_display == null || image_display.IsDisposed)
                {
                    MessageBox.Show("Image display is not available yet.");
                    return;
                }

                await image_display.RunQuickAutoScanDelayEvenOddAsync();
            }
            finally
            {
                if (mi != null) mi.Enabled = true;
            }
        }


        void FLIMageMain_Load(object sender, EventArgs e)
        {
            Hide();
            ss.Show();
            System.Windows.Forms.Application.DoEvents();

            this.Text = "FLIMage! Version " + ss.versionText;
            versionText = ss.versionText;


            State = new ScanParameters();
            fileIO = new FileIO(State);

            try
            {
                //After version 1.2.0.0, FLIMage uses "FLIM_deviceFile_N" file.
                //System.IO.Directory.CreateDirectory(State.Files.FLIMfolderPath);

                System.IO.Directory.CreateDirectory(State.Files.initFolderPath);
                if (!System.IO.File.Exists(State.Files.deviceFileName))
                {
                    //Old filename is FLIM_deviceFile. For backword compatibility, we will import it first.
                    List<string> old_fileNames = new List<string>();
                    old_fileNames.Add(Path.Combine(State.Files.initFolderPath, "FLIM_deviceFile.txt"));
                    old_fileNames.Add(Path.Combine(State.Files.initFolderPath, "FLIM_deviceFile_V1.txt"));

                    int i;
                    for (i = old_fileNames.Count - 1; i >= 0; i--)
                    {
                        if (File.Exists(old_fileNames[i]))
                        {
                            fileIO.LoadSetupFile(old_fileNames[i]);
                            break;
                        }
                    }

                    if (i == 0)
                    {
                        State.Init.MirrorAOBoard = State.Init.mirrorAOPortX.Split('/')[0];
                        State.Init.EOMBoard = State.Init.EOM_Port0.Split('/')[0];
                    }

                    fileIO.SaveDeviceFile();
                }
                else
                {
                    fileIO.LoadSetupFile(State.Files.deviceFileName);
                    fileIO.SaveDeviceFile();
                }

                if (!System.IO.File.Exists(State.Files.defaultInitFile))
                {
                    var setup_filename = fileIO.FindDefaultSetupFile();
                    if (System.IO.File.Exists(setup_filename))
                        fileIO.LoadSetupFile(setup_filename);

                    File.WriteAllText(setup_filename, fileIO.AllSetupValues_nonDevice());
                }
                else
                {
                    fileIO.LoadSetupFile(State.Files.defaultInitFile);
                }


                //Post-loading setup.
                if (State.Init.MicroscopeSystem.ToLower().Contains("poly"))
                {
                    State.Acq.resonantScanning = true;
                    State.Acq.polygonScanning = true;
                }
                else if (State.Init.enableResonantScanner)
                {
                    State.Acq.resonantScanning = true;
                }

            }
            catch (Exception E)
            {
                Debug.WriteLine(E.Message);
            }

            State.Acq.version = ss.versionText;
            version = ss.versionText;

            //End reading State

            use_motor = State.Init.motor_on;
            use_piezo = State.Init.usePiezo;

            // Just to start with something. Keep this placeholder state detached from
            // the real acquisition state so startup display settings do not leak into acquisition.
            var FLIM_ImgData = CreateStartupSimulatedFLIMData();
            double[] beta2 = CreateStartupSimulationBeta(FLIM_ImgData.State);

            FLIM_ImgData.create_SimulatedFLIM(beta2);

            int height = FLIM_ImgData.height;
            int width = FLIM_ImgData.width;
            int squareLength = (height > width) ? height : width;
            snapShotBMP = ImageProcessing.FormatImage(new double[] { 0.0, 1.0 }, new double[] { 0, -1 }, MatrixCalc.MatrixCreate2D<ushort>(squareLength, squareLength));

            FLIM_ImgData.State.Uncaging.Position = (double[])State.Uncaging.Position.Clone();
            image_display = new Image_Display(FLIM_ImgData, this, true);
            //image_display.UpdateImages(true, false, false, true);

            UpdateFileName();

            image_display.LoadFLIM(FLIM_ImgData);
            image_display.ImportState(State);

            FLIM_ImgData.ResetRoi(true);
            //nidaq_config = new NIDAQ_Config(this);

            flimage_io = new FLIMage_IO(this);
            image_display.FLIM_ImgData = FLIM_ImgData;

            //if (!flimage_io.use_nidaq)
            //{
            //    acquisitionPanel.Enabled = false;
            //    LaserPanel.Enabled = false;
            //}


            if (flimage_io.tcspc_on && flimage_io.use_bh)
            {
                sync_offset.Enabled = false;
                sync_offset2.Enabled = false;
                Binning_setting.Visible = false;
                NTimePoints.Visible = false;
                StartPointBox.Visible = false;
                st_binning.Visible = false;
                st_tp.Visible = false;
                PQMode_Pulldown.Visible = false;
                st_mode.Visible = false;
            }

            if (flimage_io.tcspc_on && flimage_io.use_pq)
            {
                sync2Group.Enabled = false;
                Resolution_Pulldown.Visible = false;
                st_nTimeP.Visible = false;
            }

            MiniScopePanel.Visible = flimage_io.microscope_system == MicroscopeSystem.MiniScope;
            EnableMiniScopeClockCheck.Checked = flimage_io.microscope_system == MicroscopeSystem.MiniScope;

            if (use_motor)
            {
                try
                {
                    if (State.Init.MotorConversionFactor.Any(x => x == 0)) //Just for compatibility with old format. Cannot be 0.
                    {
                        State.Init.MotorConversionFactor[0] = State.Motor.resolutionX;
                        State.Init.MotorConversionFactor[1] = State.Motor.resolutionY;
                        State.Init.MotorConversionFactor[2] = State.Motor.resolutionZ;
                        fileIO.SaveDeviceFile();
                    }
                    else
                    {
                        State.Motor.resolutionX = State.Init.MotorConversionFactor[0];
                        State.Motor.resolutionY = State.Init.MotorConversionFactor[1];
                        State.Motor.resolutionZ = State.Init.MotorConversionFactor[2];
                    }

                    double[] resolution = (double[])State.Init.MotorConversionFactor.Clone();
                    double[] steps = new double[] { State.Motor.stepXY, State.Motor.stepXY, State.Motor.stepZ };
                    motorCtrl = new MotorCtrl(State.Init.MotorHWName, State.Init.MotorComPort, resolution, State.Motor.velocity, steps, State.Init.MotorDisplayUpdateTime_ms);

                    if (motorCtrl.connected)
                    {
                        FillGUIMotor();
                        motorCtrl.MotH += new MotorCtrl.MotorHandler(MotorListener);

                        if (State.Init.MotorHWName.Equals("MP-285A") || State.Init.MotorHWName.Equals("MP285A"))
                        {
                            motorCtrl.continuousRead(false);
                            ContRead.Checked = false;
                        }
                        else
                        {
                            motorCtrl.continuousRead(true);
                            ContRead.Checked = true;
                        }
                    }
                    else
                    {
                        use_motor = false;
                        MessageBox.Show("Motor connection problem!!");
                    }
                }
                catch (Exception EX)
                {
                    Debug.WriteLine(EX.ToString());
                    use_motor = false;
                }
            }
            else
            {
                if (!State.Init.usePiezo)
                    stagePanel.Enabled = false;
            }

            if (!use_piezo)
            {
                CenterPiezoButton.Visible = false;
                PiezoZLabel.Visible = false;
                PiezoZUnitLabel.Visible = false;
                PiezoZ.Visible = false;
                usePiezoCheckBox.Visible = false;
                ZScanWithPiezo.Visible = false;
            }
            else
            {
                usePiezoCheckBox.Checked = true;
                ZScanWithPiezo.Checked = false;
            }


            for (int i = 0; i < laserPowerPanel.TabCount; i++)
            {
                if (State.Init.EOM_nChannels <= i)
                    laserPowerPanel.Controls[i].Dispose();
            }

            if (!State.Init.enableResonantScanner)
                State.Acq.resonantScanning = false;

            ResonantCheckBox.Visible = State.Init.enableResonantScanner;
            ResonantCheckBox.Checked = State.Acq.resonantScanning && State.Init.enableResonantScanner;
            resonant_setting = new ZoomDataResonant(State);

            if (!flimage_io.tcspc_on)
            {
                tb_Pparameters.Enabled = false;
            }

            if (!flimage_io.use_nidaq && !flimage_io.use_pq && !flimage_io.use_bh)
                use_mainPanel = false;
            else
                Show();


            SetParametersFromState(false);

            analyzeAfterEachAcquisition = analyzeEach.Checked;

            if (use_motor)
                this.Zero_all_Click(Zero_all, null);

            LoadToolsDefault();

            ss.Hide(); //splash screen.

            FLIM_EventHandling_Init();

            LoadWindows();
            InitializeSetting();
            binningSettingChange();

            flimage_io.PostFLIMageShowInitialization(this);

            StartNewFileWatcher();

            flimage_io.Notify(new ProcessEventArgs("FLIMageStarted", null));
            flimage_io.Notify(new ProcessEventArgs("ParametersChanged", null));
        }

        private FLIMData CreateStartupSimulatedFLIMData()
        {
            const int DefaultStartupPulseRateHz = 80000000;

            ScanParameters simulatedState = fileIO.CopyState();

            int nChannels = Math.Max(1, simulatedState.Acq.nChannels);
            simulatedState.Acq.nChannels = nChannels;

            simulatedState.Acq.acqFLIMA = EnsureBoolArray(simulatedState.Acq.acqFLIMA, nChannels, true);
            simulatedState.Acq.acquisition = EnsureBoolArray(simulatedState.Acq.acquisition, nChannels, true);
            simulatedState.Acq.aveFrameA = EnsureBoolArray(simulatedState.Acq.aveFrameA, nChannels, false);

            if (simulatedState.Spc.spcData.n_dataPoint < 2)
                simulatedState.Spc.spcData.n_dataPoint = 256;

            simulatedState.Spc.spcData.resolution = EnsureDoubleArray(simulatedState.Spc.spcData.resolution, nChannels, 100);
            for (int ch = 0; ch < nChannels; ch++)
            {
                if (simulatedState.Spc.spcData.resolution[ch] <= 0)
                    simulatedState.Spc.spcData.resolution[ch] = 100;
            }

            int fallbackPulseRateHz = DefaultStartupPulseRateHz;
            if (simulatedState.Acq.ExpectedLaserPulseRate_MHz > 0)
                fallbackPulseRateHz = (int)Math.Round(simulatedState.Acq.ExpectedLaserPulseRate_MHz * 1.0e6);

            simulatedState.Spc.datainfo.syncRate = EnsureIntArray(simulatedState.Spc.datainfo.syncRate, nChannels, fallbackPulseRateHz);
            for (int ch = 0; ch < nChannels; ch++)
            {
                if (simulatedState.Spc.datainfo.syncRate[ch] <= 0)
                    simulatedState.Spc.datainfo.syncRate[ch] = fallbackPulseRateHz;
            }

            simulatedState.Spc.datainfo.countRate = EnsureIntArray(simulatedState.Spc.datainfo.countRate, nChannels, 0);
            simulatedState.Spc.analysis.offset = EnsureDoubleArray(simulatedState.Spc.analysis.offset, nChannels, 0);

            ClampStartupTimeBinsToPulseInterval(simulatedState, nChannels);
            ClampStartupFitRangesToDataPoints(simulatedState, nChannels);

            return new FLIMData(simulatedState);
        }

        private void ClampStartupTimeBinsToPulseInterval(ScanParameters simulatedState, int nChannels)
        {
            int nDataPoint = simulatedState.Spc.spcData.n_dataPoint;
            for (int ch = 0; ch < nChannels; ch++)
            {
                if (simulatedState.Acq.acqFLIMA != null && ch < simulatedState.Acq.acqFLIMA.Length && !simulatedState.Acq.acqFLIMA[ch])
                    continue;
                if (simulatedState.Acq.acquisition != null && ch < simulatedState.Acq.acquisition.Length && !simulatedState.Acq.acquisition[ch])
                    continue;

                int pulseBins = GetPulseIntervalBins(simulatedState, ch);
                if (pulseBins > 1 && pulseBins < nDataPoint)
                    nDataPoint = pulseBins;
            }

            if (nDataPoint > 1)
                simulatedState.Spc.spcData.n_dataPoint = nDataPoint;
        }

        private int GetPulseIntervalBins(ScanParameters simulatedState, int channel)
        {
            double resolutionPs = GetArrayValue(simulatedState.Spc.spcData.resolution, channel, 0);
            double syncRateHz = GetArrayValue(simulatedState.Spc.datainfo.syncRate, channel, 0);

            if (resolutionPs <= 0 || syncRateHz <= 0)
                return Int32.MaxValue;

            double binsPerPulse = 1.0e12 / syncRateHz / resolutionPs;
            if (binsPerPulse <= 1)
                return Int32.MaxValue;

            return Math.Max(2, (int)Math.Floor(binsPerPulse + 1e-9));
        }

        private double GetArrayValue(double[] source, int index, double defaultValue)
        {
            if (source == null || source.Length == 0)
                return defaultValue;
            if (index >= 0 && index < source.Length)
                return source[index];
            return source[source.Length - 1];
        }

        private double GetArrayValue(int[] source, int index, double defaultValue)
        {
            if (source == null || source.Length == 0)
                return defaultValue;
            if (index >= 0 && index < source.Length)
                return source[index];
            return source[source.Length - 1];
        }

        private void ClampStartupFitRangesToDataPoints(ScanParameters simulatedState, int nChannels)
        {
            for (int ch = 0; ch < nChannels; ch++)
            {
                FieldInfo fitRangeField = simulatedState.Spc.analysis.GetType().GetField("fit_range" + (ch + 1));
                if (fitRangeField == null)
                    continue;

                int[] fitRange = fitRangeField.GetValue(simulatedState.Spc.analysis) as int[];
                fitRangeField.SetValue(simulatedState.Spc.analysis, ClampFitRangeToDataPoints(fitRange, simulatedState.Spc.spcData.n_dataPoint));
            }
        }

        private int[] ClampFitRangeToDataPoints(int[] fitRange, int nDataPoint)
        {
            if (nDataPoint <= 0)
                return new int[] { 0, 0 };

            int start = 0;
            int end = nDataPoint;
            if (fitRange != null && fitRange.Length > 0)
            {
                start = fitRange.Min();
                end = fitRange.Max();
            }

            start = Math.Max(0, Math.Min(start, nDataPoint - 1));
            end = Math.Max(start + 1, Math.Min(end, nDataPoint));
            return new int[] { start, end };
        }

        private double[] CreateStartupSimulationBeta(ScanParameters simulatedState)
        {
            double[] fitParam = simulatedState.Spc.analysis.fit_param1;
            double resolutionNs = simulatedState.Spc.spcData.resolution[0] / 1000.0;
            if (resolutionNs <= 0)
                resolutionNs = 0.1;

            double pulsePeriodNs = GetPulsePeriodNs(simulatedState, 0);

            // Fit amplitudes are ROI-scale initial guesses, not acquisition settings.
            // Use bounded per-pixel amplitudes for a stable startup placeholder while
            // taking only plausible timing guesses from the setting file.
            double pop1 = 4.0;
            double tau1 = GetStartupLifetimeParameter(fitParam, 1, 2.6, resolutionNs, pulsePeriodNs);
            double pop2 = 6.0;
            double tau2 = GetStartupLifetimeParameter(fitParam, 3, 1.1, resolutionNs, pulsePeriodNs);
            double tauG = GetStartupIrfWidthParameter(fitParam, 4, 0.15, resolutionNs, pulsePeriodNs);
            double t0 = GetStartupTimeOffsetParameter(fitParam, 5, 2.0, resolutionNs, pulsePeriodNs);

            return new double[] { pop1, resolutionNs / tau1, pop2, resolutionNs / tau2, tauG / resolutionNs, t0 / resolutionNs, 0 };
        }

        private double GetPulsePeriodNs(ScanParameters simulatedState, int channel)
        {
            double syncRateHz = GetArrayValue(simulatedState.Spc.datainfo.syncRate, channel, 0);
            if (syncRateHz <= 0)
                return 12.5;

            return 1.0e9 / syncRateHz;
        }

        private double GetStartupLifetimeParameter(double[] fitParam, int index, double defaultValue, double resolutionNs, double pulsePeriodNs)
        {
            double minLifetimeNs = Math.Max(2.0 * resolutionNs, 0.2);
            double maxLifetimeNs = Math.Max(defaultValue, 0.75 * pulsePeriodNs);

            if (TryGetFitParameterInRange(fitParam, index, minLifetimeNs, maxLifetimeNs, out double value))
                return value;

            return defaultValue;
        }

        private double GetStartupIrfWidthParameter(double[] fitParam, int index, double defaultValue, double resolutionNs, double pulsePeriodNs)
        {
            double minWidthNs = Math.Max(0.25 * resolutionNs, 0.025);
            double maxWidthNs = Math.Max(defaultValue, Math.Min(1.0, pulsePeriodNs / 5.0));

            if (TryGetFitParameterInRange(fitParam, index, minWidthNs, maxWidthNs, out double value))
                return value;

            return defaultValue;
        }

        private double GetStartupTimeOffsetParameter(double[] fitParam, int index, double defaultValue, double resolutionNs, double pulsePeriodNs)
        {
            double minOffsetNs = 0;
            double maxOffsetNs = Math.Max(defaultValue, pulsePeriodNs - resolutionNs);

            if (TryGetFitParameterInRange(fitParam, index, minOffsetNs, maxOffsetNs, out double value))
                return value;

            return defaultValue;
        }

        private bool TryGetFitParameterInRange(double[] fitParam, int index, double minimum, double maximum, out double value)
        {
            value = 0;
            if (fitParam == null || fitParam.Length <= index)
                return false;

            value = fitParam[index];
            return value >= minimum && value <= maximum;
        }

        private bool[] EnsureBoolArray(bool[] source, int length, bool defaultValue)
        {
            bool[] result = new bool[length];
            for (int i = 0; i < length; i++)
            {
                if (source != null && source.Length > i)
                    result[i] = source[i];
                else
                    result[i] = defaultValue;
            }

            return result;
        }

        private int[] EnsureIntArray(int[] source, int length, int defaultValue)
        {
            int[] result = new int[length];
            for (int i = 0; i < length; i++)
            {
                if (source != null && source.Length > i)
                    result[i] = source[i];
                else
                    result[i] = defaultValue;
            }

            return result;
        }

        private double[] EnsureDoubleArray(double[] source, int length, double defaultValue)
        {
            double[] result = new double[length];
            for (int i = 0; i < length; i++)
            {
                if (source != null && source.Length > i)
                    result[i] = source[i];
                else if (source != null && source.Length > 0)
                    result[i] = source[source.Length - 1];
                else
                    result[i] = defaultValue;
            }

            return result;
        }

        public void ResonantScan_TurnOnOff(bool ON)
        {
            SetEnableStatesOfControls();
            if (ON)
            {
                State.Acq.Rotation = 0;
                State.Acq.Rotation_Split = new double[State.Acq.nSplitScanning];

                if (flimage_io.resonant_switch != null)
                    flimage_io.resonant_switch.On();
            }
            else
            {
                if (flimage_io.resonant_switch != null)
                    flimage_io.resonant_switch.Off();
            }
        }

        public void StartNewFileWatcher()
        {
            watchDir = Path.Combine(State.Files.initFolderPath, watchDirName);
            Directory.CreateDirectory(watchDir);
            watcher = new FileSystemWatcher();
            watcher.Path = watchDir;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.Filter = watchFile; // "*.txt";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        public void OnChanged(object source, FileSystemEventArgs e)
        {
            if (!busyFile)
            {
                busyFile = true;
                try
                {
                    String commandMethod = File.ReadAllText(Path.Combine(watchDir, watchFile));
                    if (commandMethod.Contains("PIPE"))
                    {
                        Invoke((Action)delegate
                        {
                            remoteControlToolStripMenuItem1_Click(remoteControlToolStripMenuItem1, null);
                            script.TurnOnServer(true);
                        });
                    }
                }
                catch
                {

                }
                busyFile = false;
            }
        }

        void binningSettingChange()
        {
            if (flimage_io.parameters.spcData.BoardType == "PQ")
            {
                if (flimage_io.parameters.spcData.HW_Model.Contains("260 N"))
                {
                    Binning_setting.Items[0] = "0 (0.25 ns)";
                    Binning_setting.Items[1] = "1 (0.50 ns)";
                    Binning_setting.Items[2] = "2 (1.00 ns)";
                    Binning_setting.Items[3] = "3 (2.00 ns)";
                    Binning_setting.Items[4] = "4 (4.00 ns)";
                    Binning_setting.Items[5] = "5 (8.00 ns)";
                }
                else if (flimage_io.parameters.spcData.HW_Model.Contains("260 Q"))
                {
                    Binning_setting.Items[0] = "0 (25 ps)";
                    Binning_setting.Items[1] = "1 (50 ps)";
                    Binning_setting.Items[2] = "2 (100 ps)";
                    Binning_setting.Items[3] = "3 (200 ps)";
                    Binning_setting.Items[4] = "4 (400 ps)";
                    Binning_setting.Items[5] = "5 (800 ps)";
                }
            }
            else if (flimage_io.parameters.spcData.BoardType.ToLower() == "simpq")
            {
                Binning_setting.Items[0] = "0 (25 ps)";
                Binning_setting.Items[1] = "1 (50 ps)";
                Binning_setting.Items[2] = "2 (100 ps)";
                Binning_setting.Items[3] = "3 (200 ps)";
                Binning_setting.Items[4] = "4 (400 ps)";
                Binning_setting.Items[5] = "5 (800 ps)";
            }
            else if (flimage_io.parameters.spcData.BoardType == "MH")
            {
                Binning_setting.Items[0] = "0 (0.08 ns)";
                Binning_setting.Items[1] = "1 (0.16 ns)";
                Binning_setting.Items[2] = "2 (0.32 ns)";
                Binning_setting.Items[3] = "3 (0.64 ns)";
                Binning_setting.Items[4] = "4 (1.28 ns)";
                Binning_setting.Items[5] = "5 (2.56 ns)";
            }
            else if (flimage_io.parameters.spcData.BoardType == "PH")
            {
                Binning_setting.Items[0] = "5 (32 ps)";
                Binning_setting.Items[1] = "6 (64 ps)";
                Binning_setting.Items[2] = "7 (128 ps)";
                Binning_setting.Items[3] = "8 (256 ps)";
                Binning_setting.Items[4] = "9 (512 ps)";
                Binning_setting.Items[5] = "10 (1024 ps)";
            }
            else if (flimage_io.parameters.spcData.BoardType == "HH")
            {
                Binning_setting.Items[0] = "5 (32 ps)";
                Binning_setting.Items[1] = "6 (64 ps)";
                Binning_setting.Items[2] = "7 (128 ps)";
                Binning_setting.Items[3] = "8 (256 ps)";
                Binning_setting.Items[4] = "9 (512 ps)";
                Binning_setting.Items[5] = "10 (1024 ps)";
            }
        }

        void InitializeSetting()
        {
            settingManager = new SettingManager(settingName, State.Files.initFolderPath);

            settingManager.AddToDict(KeepPagesInMemoryCheck);
            settingManager.AddToDict(FastSaving_CB);
            settingManager.AddToDict(UseOmeTiff_CB);
            settingManager.AddToDict(BackToCenterRadio);
            settingManager.AddToDict(BackToStartRadio);
            settingManager.AddToDict(StayMotorRadio);
            settingManager.AddToDict(analyzeEach);
            settingManager.LoadToObject();

            State.Files.fastSaving = FastSaving_CB.Checked;
            ApplyOmeTiffSetting(UseOmeTiff_CB.Checked, false);
            if (image_display?.FLIM_ImgData?.State?.Files != null)
                image_display.FLIM_ImgData.State.Files.fastSaving = State.Files.fastSaving;
            if (flimage_io?.State?.Files != null)
                flimage_io.State.Files.fastSaving = State.Files.fastSaving;
            if (flimage_io?.fileIO != null)
                flimage_io.fileIO.HoldFastWriterOpen = ShouldHoldFastWriterOpen();

            //UiState.Load(this);
            MotorPositioningUpdate();
        }

        void settingManagerSave()
        {
            UiState.Save(this);
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

        public void EnsurePhasorPlotWindow(Image_Display sourceDisplay = null)
        {
            var display = sourceDisplay;
            if (display == null || display.IsDisposed)
                display = image_display;

            if (display == null || display.IsDisposed)
                return;

            if (phasor_plot_window == null || phasor_plot_window.IsDisposed)
                phasor_plot_window = new PhasorPlotWindow(this, display);
            else
                phasor_plot_window.AttachImageDisplay(display);
        }

        public void ShowPhasorPlotWindow(Image_Display sourceDisplay = null)
        {
            EnsurePhasorPlotWindow(sourceDisplay);

            if (phasor_plot_window == null)
                return;

            phasor_plot_window.Show();
            phasor_plot_window.Activate();
            phasor_plot_window.RequestRefresh();
        }

        void LoadWindows()
        {

            if (File.Exists(WindowsInfoFileName()))
            {
                String readText;
                try
                {
                    readText = File.ReadAllText(WindowsInfoFileName());
                }
                catch
                {
                    return;
                }
                String[] words = readText.Split(',');

                //Plugin windows.
                for (int i = 0; i < plugin_names.Length; i++)
                {
                    Type type = Type.GetType(plugin_names[i], true);
                    if (words.Any(x => x == type.Name.ToLower()))
                    {
                        object instance = Activator.CreateInstance(type, new object[] { this });
                        plugin_object[i] = instance;
                        instance.GetType().GetMethod("Show", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null).Invoke(instance, null);
                    }
                }


                if (words.Any(x => x == "uncaging_panel") || State.Uncaging.uncage_whileImage && flimage_io.use_nidaq)
                {
                    uncaging_panel = new Uncaging_Trigger_Panel(this);
                    uncaging_panel.Show();
                }

                if (words.Any(x => x == "digital_panel") || State.DO.DO_whileImage && flimage_io.use_nidaq)
                {
                    digital_panel = new Digital_Trigger_Panel(this);
                    digital_panel.Show();
                }

                if (words.Any(x => x == "DIO_panel") && flimage_io.use_nidaq)
                {
                    DIO_panel = new DigitalSignalPanel(this);
                    DIO_panel.Show();
                }

                if (words.Any(x => x == "fastZcontrol"))
                {
                    fastZcontrol = new FastZControl(this);
                    fastZcontrol.Show();
                }

                if (words.Any(x => x == "shading_correction") && flimage_io.use_nidaq)
                {
                    shading_correction = new ShadingCorrection(this);
                    shading_correction.Show();
                }

                if (words.Any(x => x == "script"))
                {
                    script = new RemoteControl(this);
                    script.Show();
                }

                if (words.Any(x => x == "pmt_control"))
                {
                    pmt_control = new PMTControl(State, this);
                    pmt_control.Show();
                }

                if (words.Any(x => x == "physiology"))
                {
                    if (flimage_io.use_nidaq)
                    {
                        physiology = new StimPanel(true);
                        physiology.Show();
                    }
                }

                if (!words.Any(x => x == "plot_realtime"))
                    image_display.plot_realtime.Hide();
                if (!words.Any(x => x == "plot_regular"))
                    image_display.plot_regular.Hide();
                if (words.Any(x => x == "phasor_plot_window"))
                {
                    EnsurePhasorPlotWindow();
                    phasor_plot_window.Show();
                    phasor_plot_window.RequestRefresh();
                }

            }

            MenuItems_CheckControls();
        }

        private void resetWindowPositionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var pattern in new[] { "*.loc", "*.txt" })
                {
                    foreach (string file in Directory.GetFiles(State.Files.windowsInfoPath, pattern))
                    {
                        try
                        {
                            if (File.Exists(file))
                                File.Delete(file);
                        }
                        catch
                        {
                            // Ignore.
                        }
                    }
                }
            }
            catch
            {
                // Ignore.
            }

            for (int i = 0; i < plugin_names.Length; i++)
            {
                if (plugin_object[i] != null)
                {
                    plugin_object[i].GetType().GetMethod("Close", BindingFlags.Instance | BindingFlags.Public,
                        null, new Type[] { }, null).Invoke(plugin_object[i], null);
                }
            }


            if (uncaging_panel != null && !uncaging_panel.IsDisposed)
            {
                uncaging_panel.Close();
                uncaging_panel = new Uncaging_Trigger_Panel(this);
                uncaging_panel.Show();
            }

            if (digital_panel != null && !digital_panel.IsDisposed)
            {
                digital_panel.Close();
                digital_panel = new Digital_Trigger_Panel(this);
                digital_panel.Show();
            }

            if (DIO_panel != null && !DIO_panel.IsDisposed)
            {
                DIO_panel.Close();
                DIO_panel = new DigitalSignalPanel(this);
                DIO_panel.Show();
            }

            if (fastZcontrol != null && !fastZcontrol.IsDisposed)
            {
                fastZcontrol.Close();
                fastZcontrol = new FastZControl(this);
                fastZcontrol.Show();
            }

            if (shading_correction != null && !shading_correction.IsDisposed)
            {
                shading_correction.Close();
                shading_correction = new ShadingCorrection(this);
                shading_correction.Show();
            }

            if (script != null && !script.IsDisposed)
            {
                script.Close();
                script = new RemoteControl(this);
                script.Show();
            }

            if (pmt_control != null && !pmt_control.IsDisposed)
            {
                pmt_control.Close();
                pmt_control = new PMTControl(State, this);
                pmt_control.Show();
            }

            if (physiology != null && !physiology.IsDisposed)
            {
                physiology.Close();
                physiology = new StimPanel(true);
                physiology.Show();
            }

            if (image_display != null && !image_display.IsDisposed)
            {
                image_display.Close();
            }
            image_display = new Image_Display(image_display.FLIM_ImgData, this, true);
            image_display.Show();

            if (image_display.plot_realtime != null && !image_display.plot_realtime.IsDisposed)
            {
                image_display.plot_realtime.Close();
            }
            image_display.plot_realtime = new plot_timeCourse(true, image_display);
            image_display.plot_realtime.Show();

            if (image_display.plot_regular != null && !image_display.plot_regular.IsDisposed)
            {
                image_display.plot_regular.Close();
                image_display.plot_regular = new plot_timeCourse(false, image_display);
                image_display.plot_regular.Show();
            }

            EnsurePhasorPlotWindow();
            if (phasor_plot_window != null)
            {
                phasor_plot_window.AttachImageDisplay(image_display);
                if (phasor_plot_window.Visible)
                {
                    phasor_plot_window.ResetWindowLocation();
                    phasor_plot_window.RequestRefresh();
                }
            }

        }

        public void LoadPhysiologyParametersFromState()
        {
            if (physiology != null && !physiology.IsDisposed)
            {
                fileIO.State = State;
                var phys_parameters = fileIO.CopyFromStateToPhysParameters();
                physiology.phys_parameters = phys_parameters;
                physiology.LoadValuesToPanel(true);
            }
        }

        bool IsWindowActive(object obj)
        {
            if (obj != null)
            {
                var field = obj.GetType().GetProperty("IsDisposed", BindingFlags.Instance | BindingFlags.Public);
                bool is_disposed;
                if (field == null)
                    is_disposed = true;
                else
                    is_disposed = (bool)field.GetValue(obj);
                return !is_disposed;
            }
            else
                return false;
        }

        bool IsWindowOpen(object obj)
        {
            if (obj != null)
            {
                if (IsWindowActive(obj))
                {
                    bool visible = (bool)(obj.GetType().GetProperty("Visible", BindingFlags.Instance | BindingFlags.Public).GetValue(obj));
                    return visible;
                }
                else
                    return false;
            }
            else
                return false;
        }


        public void SaveWindows()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < plugin_names.Length; i++)
            {
                if (IsWindowOpen(plugin_object[i]))
                {
                    plugin_object[i].GetType().GetMethod("SaveWindowLocation", BindingFlags.Instance | BindingFlags.Public,
                        null, new Type[] { }, null).Invoke(plugin_object[i], null);
                    sb.Append(plugin_object[i].GetType().Name.ToLower());
                    sb.Append(",");
                }
            }

            if (uncaging_panel != null && uncaging_panel.Visible)
            {
                uncaging_panel.SaveWindowLocation();
                sb.Append("uncaging_panel");
                sb.Append(",");
            }

            if (digital_panel != null && digital_panel.Visible)
            {
                digital_panel.SaveWindowLocation();
                sb.Append("digital_panel");
                sb.Append(",");
            }

            if (DIO_panel != null && DIO_panel.Visible)
            {
                DIO_panel.SaveWindowLocation();
                sb.Append("DIO_panel");
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

            if (script != null && script.Visible)
            {
                script.SaveWindowLocation();
                sb.Append("script");
                sb.Append(",");
            }

            if (physiology != null && physiology.Visible)
            {
                physiology.CloseCommand();
                sb.Append("physiology");
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

            if (phasor_plot_window != null && phasor_plot_window.Visible)
            {
                phasor_plot_window.SaveWindowLocation();
                sb.Append("phasor_plot_window");
                sb.Append(",");
            }

            if (pmt_control != null && pmt_control.Visible)
            {
                pmt_control.WindowClosing();
                sb.Append("pmt_control");
                sb.Append(",");
            }

            string allStr = sb.ToString();
            try
            {
                File.WriteAllText(WindowsInfoFileName(), allStr);
            }
            catch
            {
                // Ignore IO issues (access denied, etc.) so shutdown/close never crashes.
            }
        } //SaveWindows

        public void ToolWindowClosed()
        {
            this.Invoke((Action)delegate
            {
                MenuItems_CheckControls();
            });
            SaveWindows(); //When forms are closed, this will be called.
        }

        void MenuItems_CheckControls()
        {

            uncagingControlToolStripMenuItem1.Checked = (uncaging_panel != null && uncaging_panel.Visible);
            digitalOutputControlToolStripMenuItem.Checked = digital_panel != null && digital_panel.Visible;
            shadingCorretionToolStripMenuItem.Checked = (shading_correction != null && shading_correction.Visible);
            fastZControlToolStripMenuItem.Checked = (fastZcontrol != null && fastZcontrol.Visible);
            imageToolStripMenuItem.Checked = image_display.Visible;
            realtimePlotToolStripMenuItem.Checked = image_display.plot_realtime.Visible;
            remoteControlToolStripMenuItem1.Checked = (script != null && script.Visible);
            stageControlToolStripMenuItem.Checked = (motor_ctrl_panel != null && motor_ctrl_panel.Visible);
            nIDAQConfigToolStripMenuItem.Checked = (nidaq_config != null && nidaq_config.Visible);
            dIOPanelToolStripMenuItem.Checked = (DIO_panel != null && DIO_panel.Visible);
            pMTControlToolStripMenuItem.Checked = pmt_control != null && pmt_control.Visible;
            zoZoLabMotorControlToolStripMenuItem.Checked = zozo_motor_ctrl != null && zozo_motor_ctrl.Visible;
            pMTControlToolStripMenuItem.Visible = State.Init.MicroscopeSystem.ToLower().Contains("thor");

            zoZoLabMotorControlToolStripMenuItem.Visible = State.Init.MotorHWName.ToLower().Contains("zozo");

            for (int i = 0; i < plugin_object.Length; i++)
            {
                plugin_tool_bar_hanels[i].Checked = IsWindowOpen(plugin_object[i]);
            }
        }

        string WindowsInfoFileName()
        {
            var dir = State.Files.windowsInfoPath;
            if (string.IsNullOrWhiteSpace(dir) || !TryEnsureDirectory(dir))
            {
                // Fallback: some PCs block writes to Documents (or Init_Files may have restrictive ACLs).
                var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appName = System.Windows.Forms.Application.ProductName;
                if (string.IsNullOrWhiteSpace(appName))
                    appName = "FLIMage";

                dir = Path.Combine(baseDir, appName, "WindowsInfo");
                TryEnsureDirectory(dir);
                State.Files.windowsInfoPath = dir;
            }

            return Path.Combine(dir, "WindowInfo.fwi");
        }

        private static bool TryEnsureDirectory(string dir)
        {
            try
            {
                Directory.CreateDirectory(dir);
                return true;
            }
            catch
            {
                return false;
            }
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

        private void EOMDelayUpDown_us_Clicked(object sender, EventArgs e)
        {
            if (EOMDelayChange())
            {
                GetParametersFromGUI(sender);
                flimage_io.ResetFocus();
            }
        }

        private void EOMDelayUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (EOMDelayChange())
                {
                    GetParametersFromGUI(sender);
                    flimage_io.ResetFocus();
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }


        /// <summary>
        /// Return if value changed.
        /// </summary>
        /// <returns></returns>
        private bool EOMDelayChange()
        {
            if (State.Acq.resonantScanning || State.Acq.polygonScanning)
            {
                double saveEOM = State.Acq.resonantEOMDelay_us;

                double EOMDelay1_us;
                if (!Double.TryParse(EOMDelayUpDown_us.Text, out EOMDelay1_us))
                    EOMDelay1_us = State.Acq.resonantEOMDelay_us;

                double msPerLine = State.Acq.msPerLineActual();

                double maxDelay_us = 0.5 * msPerLine * 1000;

                if (EOMDelay1_us >= -maxDelay_us && EOMDelay1_us <= maxDelay_us)
                {
                    State.Acq.resonantEOMDelay_us = EOMDelay1_us;
                }

                return State.Acq.resonantEOMDelay_us != saveEOM;
            }
            else
            {
                double saveEOM = State.Acq.EOMDelay;

                double EOMDelay1_us;
                if (!Double.TryParse(EOMDelayUpDown_us.Text, out EOMDelay1_us))
                    EOMDelay1_us = State.Acq.EOMDelay * 1000.0; //microseconds.

                double msPerLine = State.Acq.msPerLineActual();
                double maxDelay = 0.5 * msPerLine * 1000.0;

                if (EOMDelay1_us >= -maxDelay && EOMDelay1_us <= maxDelay)
                {
                    State.Acq.EOMDelay = EOMDelay1_us / 1000.0;
                }

                return saveEOM != State.Acq.EOMDelay;
            }
        }

        private void ScanDelayUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (ScanDelayChange())
                {
                    GetParametersFromGUI(sender);
                    flimage_io.ResetFocus();
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void ScanDelayUpDown_nano_Click(object sender, EventArgs e)
        {
            if (ScanDelayChange())
            {
                GetParametersFromGUI(sender);
                flimage_io.ResetFocus();
            }
        }

        private bool ScanDelayChange()
        {

            double ScanDelay_nano;

            if (State.Acq.resonantScanning || State.Acq.polygonScanning)
            {
                double save_delay = State.Acq.resonantScanDelay_us;

                if (!Double.TryParse(ScanDelayUpDown_nano.Text, out ScanDelay_nano))
                    ScanDelay_nano = State.Acq.resonantScanDelay_us * 1000.0; //nanosecond

                double nsPerLine = 500.0 / State.Init.resonantFreq_Hz * 1000.0 * 1000.0;
                if (State.Acq.polygonScanning)
                    nsPerLine = 1000.0 / State.Init.resonantFreq_Hz * 1000.0 * 1000.0;

                if (ScanDelay_nano >= 0 && ScanDelay_nano <= 0.5 * nsPerLine)
                {
                    State.Acq.resonantScanDelay_us = ScanDelay_nano / 1000.0;
                }

                return save_delay != State.Acq.resonantScanDelay_us;
            }
            else
            {
                double save_delay = State.Acq.ScanDelay;

                if (!Double.TryParse(ScanDelayUpDown_nano.Text, out ScanDelay_nano))
                    ScanDelay_nano = State.Acq.ScanDelay * 1000.0 * 1000.0; //nanosecond

                double scanDelay_ms = ScanDelay_nano / 1000.0 / 1000.0;

                double msPerLine1 = State.Acq.msPerLineActual();

                if (scanDelay_ms >= 0 && scanDelay_ms <= 0.5 * msPerLine1)
                {
                    State.Acq.ScanDelay = scanDelay_ms;
                }

                return save_delay != State.Acq.ScanDelay;
            }
        }

        private void LineClockDelay_us_Click(object sender, EventArgs e)
        {
            if (LineDelayChanged())
            {
                GetParametersFromGUI(sender);
                flimage_io.ResetFocus();
            }
        }

        private void LineClockDelay_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (LineDelayChanged())
                    flimage_io.ResetFocus();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private bool LineDelayChanged()
        {
            double saveDelay = State.Acq.LineClockDelay_us;
            double delay_us = State.Acq.LineClockDelay_us;
            Double.TryParse(LineClockDelay_us.Text, out delay_us);
            State.Acq.LineClockDelay_us = delay_us;

            return saveDelay != State.Acq.LineClockDelay_us;
        }

        // KENGO BIGEN 11-13-2025
        // Add for the following purpuses:
        // Update the NumericUpDown located under the current mouse cursor.
        // Top half of the control => increment, bottom half => decrement.
        private void UpdateNumericUnderMouse()
        {
            // controls to consider (adjust list if you have more)
            var nudList = new NumericUpDown[] { ZoomP1, Zoom1, Zoom10, Zoom100 };
            var mouse = Cursor.Position; // screen coordinates

            foreach (var nud in nudList)
            {
                if (nud == null) continue;

                // numeric control bounds in screen coordinates
                var nudScreenTopLeft = nud.PointToScreen(Point.Empty);
                var nudRect = new Rectangle(nudScreenTopLeft, nud.Size);

                if (!nudRect.Contains(mouse)) continue;

                try
                {
                    int clickY = mouse.Y - nudRect.Top;
                    bool topHalf = clickY < (nud.Height / 2);

                    decimal target = topHalf ? nud.Value + nud.Increment : nud.Value - nud.Increment;

                    if (target > nud.Maximum) target = nud.Maximum;
                    if (target < nud.Minimum) target = nud.Minimum;

                    // assign value (this raises ValueChanged and updates the GUI)
                    nud.Value = target;
                }
                catch
                {
                    // swallow to avoid breaking UI in edge cases
                }

                break; // only update the top-most matching control
            }
        }
        // KENGO END

        /// <summary>
        /// Called by Zoom panel click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomPanel_Click(object sender, MouseEventArgs e)
        {
            // KENGO BEGIN 11-13-2025
            // Add for the following purpuses:
            // Ensure a right/middle click on a NumericUpDown updates that control immediately
            if (e != null && (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle))
            {
                UpdateNumericUnderMouse();
            }
            // KENGO END

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
                Zoom100.Value = 9;

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

            //if (sender.Equals(Zoom10))
            //{
            //    if (Zoom10.Value == 0 && Zoom100.Value == 0)
            //    {
            //        Zoom1.Value = 9;
            //    }
            //}
            //else if (sender.Equals(Zoom100))
            //{
            //    if (Zoom100.Value == 0)
            //    {
            //        Zoom10.Value = 9;
            //    }
            //}


            double zoom = (double)Zoom100.Value * 100 + (double)Zoom10.Value * 10
                + (double)Zoom1.Value + (double)ZoomP1.Value * 0.1;

            // Kengo BEGIN 05-16-2025
            // 5-point increment by pressing Ctrl key
            // twice increment by pressing Shift key
            // KENGO 11-13-2025, added the middle and right mouse button functionality
            double temp = zoom;

            // treat middle mouse button as Ctrl, right mouse button as Shift
            bool middleMouse = (e != null && e.Button == MouseButtons.Middle);
            bool rightMouse = (e != null && e.Button == MouseButtons.Right);

            bool ctrlEffective = (Control.ModifierKeys & Keys.Control) != 0 || middleMouse;
            bool shiftEffective = (Control.ModifierKeys & Keys.Shift) != 0 || rightMouse;

            if (ctrlEffective)
            {
                if (State.Acq.zoom < zoom)
                    zoom = State.Acq.zoom + 5.0 - (State.Acq.zoom + 5.0) % 5.0;
                else if (State.Acq.zoom > zoom)
                    zoom = State.Acq.zoom - 5.0 - (State.Acq.zoom - 5.0) % 5.0;
            }

            if (shiftEffective)
            {
                if (State.Acq.zoom < zoom)
                    zoom = State.Acq.zoom * 2.0;
                else if (State.Acq.zoom > zoom)
                    zoom = State.Acq.zoom * 0.5;
                zoom = Math.Floor(zoom * 10.0 + 0.5) / 10.0;
            }

            if (zoom != temp)
            {
                Zoom100.Value = Math.Floor((Decimal)zoom / 100);
                Zoom10.Value = Math.Floor(((Decimal)zoom - Zoom100.Value * 100) / 10);
                Zoom1.Value = Math.Floor(((Decimal)zoom - Zoom100.Value * 100 - Zoom10.Value * 10));
                ZoomP1.Value = Math.Floor(((Decimal)zoom - Math.Floor((Decimal)zoom)) * 10);
            }
            // Kengo END


            if (zoom < 0.5)
            {
                zoom = 0.5;
                ZoomP1.Value = 5;
                Zoom1.Value = 0;
                Zoom10.Value = 0;
                Zoom100.Value = 0;
            }

            if (zoom != State.Acq.zoom)
            {
                State.Acq.zoom = zoom;

                Zoom.InvokeIfRequired(o => o.Text = string.Format("{0:0.0}", State.Acq.zoom));

                ZoomChangedFunc();

                //UpdateScanPositionWindow();

                if (!flimage_io.grabbing && !flimage_io.focusing && !flimage_io.refocusing)
                    this.InvokeIfRequired(o => o.FillGUI());

                flimage_io.ResetFocus();

                if (!flimage_io.grabbing && !flimage_io.focusing && !flimage_io.refocusing)
                {
                    flimage_io.ParkMirrors(false);
                    SaveSetting();
                }
            }
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
            StatusText.BeginInvokeIfRequired(o => o.Text = str);
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

                flimage_io.ResetFocus();

                if (!flimage_io.grabbing && !flimage_io.focusing)
                {
                    flimage_io.ParkMirrors(false);
                }

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

            if (uncaging_panel != null && uncaging_panel.UncagingShutter)
                flimage_io.uncagingShutterCtrl(uncaging_panel.UncagingShutter, true, true);

            flimage_io.shading.calibration.PutValue(powerArray);
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

            //make function!
            flimage_io.ResetFocus();

            if (!flimage_io.grabbing && !flimage_io.focusing && !flimage_io.refocusing)
            {
                flimage_io.ParkMirrors(false);
            }
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

        public void RateTimerEvent_GUI_Update(bool badrate)
        {
            double syncRate1 = (double)State.Spc.datainfo.syncRate[0] / 1e6;
            double syncRate2 = (double)State.Spc.datainfo.syncRate[1] / 1e6;

            if (State.Spc.spcData.channelPerDevice > 1)
                syncRate2 = syncRate1;

            double countRate1 = (double)State.Spc.datainfo.countRate[0];
            double countRate2 = (double)State.Spc.datainfo.countRate[1];

            Sync_rate.Text = String.Format("{0: 0.000} MHz", syncRate1);
            Sync_rate2.Text = String.Format("{0: 0.000} MHz", syncRate2);
            Ch_rate1.Text = String.Format("{0:#,##0.##} /s", countRate1);
            Ch_rate2.Text = String.Format("{0:#,##0.##} /s", countRate2);
            expectedRate.Text = String.Format("{0: 0.0} MHz", State.Acq.ExpectedLaserPulseRate_MHz);

            laserWarningButton.Visible = badrate;

            if (!flimage_io.grabbing && !flimage_io.focusing)
            {
                if (flimage_io.shading != null && flimage_io.shading.calibration != null)
                {
                    double[] result = flimage_io.shading.calibration.readIntensity();
                    if (result != null)
                    {
                        if (result.Length > 0)
                            powerRead1.Text = String.Format("{0: 0.0} mV", result[0] * 1000);
                        if (result.Length > 1)
                            powerRead2.Text = String.Format("{0: 0.0} mV", result[1] * 1000);
                        if (result.Length > 2)
                            powerRead3.Text = String.Format("{0: 0.0} mV", result[2] * 1000);
                        if (result.Length > 3)
                            powerRead4.Text = String.Format("{0: 0.0} mV", result[3] * 1000);
                    }
                }
            } //Running

            ETime.Text = String.Format("{0:0} s", flimage_io.UIstopWatch_Image.ElapsedMilliseconds / 1000.0);
            ETime2.Text = String.Format("{0:0} s", flimage_io.UIstopWatch_Loop.ElapsedMilliseconds / 1000.0);


        }

        public void FLIM_On_GUIUpdate(bool ON)
        {

        }


        public void InitializeCounter_GUI_Update()
        {
            CurrentFrame.Text = "1";
            CurrentSlice.Text = "1";
            nAverageFrame.Text = "1";

            if (!flimage_io.looping)
                CurrentImage.Text = "1";

        }

        public void DisplaySnapShot()
        {
            var state1 = image_display.FLIM_ImgData.State;
            Bitmap smallImage = ImageProcessing.FormatImage(image_display.State_intensity_range[image_display.currentChannel], image_display.thresholdFLIM_low_high[image_display.currentChannel],
                image_display.FLIM_ImgData.Project[image_display.currentChannel]);

            //All images are zoomed by *zoom.
            int nPix = (int)(Math.Max(state1.Acq.pixelsPerLine, state1.Acq.linesPerFrame) * state1.Acq.zoom);
            if (state1.Acq.nSplitScanning < 2)
            {
                double[,] offset0 = new double[2, 1];
                offset0[0, 0] = state1.Acq.XOffset / State.Acq.XMaxVoltage * state1.Acq.pixelsPerLine * State.Acq.zoom;
                offset0[1, 0] = state1.Acq.YOffset / State.Acq.YMaxVoltage * state1.Acq.linesPerFrame * State.Acq.zoom;
                MathLibrary.MatrixCalc.RotateOffsetTimeSeries(offset0, -State.Acq.Rotation, new double[2], new double[2], State.Acq.flipXYScan, State.Acq.switchXYScan);
                int offsetX = (nPix - state1.Acq.pixelsPerLine) / 2;
                int offsetY = (nPix - state1.Acq.linesPerFrame) / 2;
                var snapShotBMP2 = new Bitmap(nPix, nPix);
                Graphics g = Graphics.FromImage(snapShotBMP2);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.TranslateTransform(nPix / 2, nPix / 2);
                g.RotateTransform((float)state1.Acq.Rotation);
                g.TranslateTransform(-nPix / 2, -nPix / 2);
                g.TranslateTransform((float)offset0[0, 0], (float)offset0[1, 0]);
                g.DrawImage(smallImage, new Point(offsetX, offsetY));
                if (SuperImposeCheckBox.Checked && snapShotBMP.Width > 1 && snapShotBMP.Height > 1)
                {
                    Graphics g2 = Graphics.FromImage(snapShotBMP);
                    g2.InterpolationMode = InterpolationMode.NearestNeighbor;
                    float sx = (float)snapShotBMP2.Width / snapShotBMP.Width;
                    float sy = (float)snapShotBMP2.Height / snapShotBMP.Height;
                    g2.ScaleTransform(sx, sy);
                    g2.DrawImage(snapShotBMP2, new Point(0, 0));
                }
                else
                    snapShotBMP = snapShotBMP2;

                UpdateScanPositionWindow();
            }
            else
            {

                int nSplit = state1.Acq.nSplitScanning;

                int frameHieght = smallImage.Height / nSplit;
                Graphics g2 = Graphics.FromImage(snapShotBMP);

                for (int i = 0; i < nSplit; i++)
                {
                    double[,] offset0 = new double[2, 1];
                    offset0[0, 0] = state1.Acq.XOffset_Split[i] / State.Acq.XMaxVoltage * state1.Acq.pixelsPerLine * State.Acq.zoom;
                    offset0[1, 0] = state1.Acq.YOffset_Split[i] / State.Acq.YMaxVoltage * state1.Acq.linesPerFrame * State.Acq.zoom;
                    //Bring the rotation back, and then apply flip and switch.
                    MathLibrary.MatrixCalc.RotateOffsetTimeSeries(offset0, -State.Acq.Rotation_Split[i], new double[2], new double[2], State.Acq.flipXYScan, State.Acq.switchXYScan);
                    int offsetX = (nPix - state1.Acq.pixelsPerLine) / 2;
                    int offsetY = (nPix - state1.Acq.linesPerFrame / nSplit) / 2;

                    var snapShotBMP2 = new Bitmap(nPix, nPix);
                    Graphics g = Graphics.FromImage(snapShotBMP2);
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.TranslateTransform(nPix / 2, nPix / 2);
                    g.RotateTransform((float)state1.Acq.Rotation_Split[i]);
                    g.TranslateTransform(-nPix / 2, -nPix / 2);
                    g.TranslateTransform((float)offset0[0, 0], (float)offset0[1, 0]);
                    g.DrawImage(smallImage, new Rectangle(offsetX, offsetY, smallImage.Width, frameHieght),
                                    0, frameHieght * i,           // upper-left corner of source rectangle
                                    smallImage.Width,       // width of source rectangle
                                    frameHieght,      // height of source rectangle
                                    GraphicsUnit.Pixel);

                    if (i == 0)
                    {
                        if (!SuperImposeCheckBox.Checked || snapShotBMP.Width <= 1 || snapShotBMP.Height <= 1)
                        {
                            g2.Dispose();
                            snapShotBMP = new Bitmap(nPix, nPix);
                            g2 = Graphics.FromImage(snapShotBMP);
                        }

                        float sx = (float)snapShotBMP2.Width / snapShotBMP.Width;
                        float sy = (float)snapShotBMP2.Height / snapShotBMP.Height;
                        g2.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g2.ScaleTransform(sx, sy);
                    }

                    g2.DrawImage(snapShotBMP2, new Point(0, 0));
                }

                UpdateScanPositionWindow();
            }
        }

        public void CalibEOM_GUI_Update(bool[] success)
        {
            bool allTrue = true;

            for (int i = 0; i < success.Length; i++)
            {
                Control[] found = Controls.Find("tabPage" + (i + 1), true);
                Control[] button1 = Controls.Find("needCalib" + (i + 1), true);
                if (button1.Length > 0)
                    button1[0].Visible = !success[i];
                if (found.Length > 0)
                {
                    allTrue = allTrue && success[i];
                    if (!success[i])
                    {
                        found[0].Text = "Error " + (i + 1);
                    }
                    else
                    {
                        found[0].Text = "Laser " + (i + 1);
                    }
                }
            } //for

            if (!allTrue)
            {
                Calibrate1.ForeColor = Color.Red;
                needCalibLabel.Visible = true;
            }
            else
            {
                flimage_io.ParkMirrors(false);
                Calibrate1.ForeColor = Color.Black;
                needCalibLabel.Visible = false;
            }
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
                { }
                ;
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
                { }
                ;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }


        public void FocusButtonEnable(bool enable)
        {
            FocusButton.Enabled = Enabled;
        }


        /// <summary>
        /// Directly called after stop button click.
        /// </summary>
        private void StopFocus()
        {
            flimage_io.StopFocus();
        }

        /// <summary>
        /// Called by abort grab button click.
        /// </summary>
        /// <param name="force"></param>
        private void StopGrab(bool force)
        {
            flimage_io.StopGrab(force);
        }

        /// <summary>
        /// Actual program for stop grabbing.
        /// </summary>
        /// <param name="force"></param>
        public void StopGrab_GUI_Update()
        {

            ChangeItemsStatus(true, false);
            GrabButton.Text = "GRAB";
            GrabButton.Enabled = true;

            if (use_motor)
            {
                motorCtrl.continuousRead(ContRead.Checked);
                motorCtrl.GetPosition();
            }

            if (fastZcontrol != null)
                fastZcontrol.ControlsDuringScanning(false);

            if (State.Acq.ZStack && State.Acq.nSlices > 1)
            {
                // Reset Z stack synchronization flag
                lock (zStackSyncLock)
                {
                    zStackMovementInProgress = false;
                }

                if (motor_back_to_center)
                    MoveMotorBackToCenter();

                if (motor_back_to_start)
                    MoveMotorBackToStart();
            }
            //SaveSetting();

            if (flimage_io.grabbing)  // Kengo 05-25-2025 Add condition to notify only when grabbing = true
                flimage_io.Notify(new ProcessEventArgs("GrabAbort", null));
        }

        /// <summary>
        /// Abort focusing by button click.
        /// </summary>
        public void StopFocus_GUI_Update()
        {
            ChangeItemsStatus(true, true);
            FocusButton.Text = "FOCUS";
            SaveSetting();
            if (fastZcontrol != null)
                fastZcontrol.ControlsDuringScanning(false);

            flimage_io.Notify(new ProcessEventArgs("FocusStop", null));
        }



        public void AO_FrameUpdate(int AO_FrameCounter)
        {
            AOCounter.Text = AO_FrameCounter.ToString();
        }



        /// <summary>
        /// Starting Grab. 
        /// </summary>
        /// <param name="focus"></param>
        private void StartGrab(bool focus)
        {
            SyncAnalysisStateForAcquisition();

            Task.Factory.StartNew(() =>
            {
                var moving_back = false;
                //Not going. Just to set the center at the current position.
                if (State.Acq.ZStack && State.Acq.nSlices > 1 && State.Acq.sliceStep != 0)
                {
                    if (motor_back_to_center)
                    {
                        if (use_piezo && usePiezoCheckBox.Checked)
                            moving_back = flimage_io.piezo.current_position == HardwareControls.IOControls.CurrentPosition.Center;
                        else if (use_motor)
                            moving_back = motorCtrl.stack_Position == MotorCtrl.StackPosition.Center;

                        if (moving_back)
                        {
                            SetCenter();
                            if (!focus)
                                GoStart_Click(GoStart, null);
                        }
                    }
                    else if (motor_back_to_start)
                    {
                        if (use_piezo && usePiezoCheckBox.Checked)
                            moving_back = flimage_io.piezo.current_position == HardwareControls.IOControls.CurrentPosition.Start;
                        else if (use_motor)
                            moving_back = motorCtrl.stack_Position == MotorCtrl.StackPosition.Start;

                        if (moving_back)
                        {
                            SetTop();
                            if (!focus)
                                GoStart_Click(GoStart, null);
                        }
                    }
                }

                flimage_io.State = State;
                flimage_io.StartGrab(focus);
            });
        }

        private void SyncAnalysisStateForAcquisition()
        {
            if (image_display == null || image_display.IsDisposed || image_display.FLIM_ImgData == null)
                return;

            image_display.ExportStateDisplay(State);

            if (image_display.FLIM_ImgData.offset != null)
                State.Spc.analysis.offset = (double[])image_display.FLIM_ImgData.offset.Clone();
        }

        public void StarGrab_GUI_Update(bool focus)
        {
            if (focus)
            {
                ChangeItemsStatus(false, true);
                FocusButton.Text = "ABORT";
            }
            else
            {
                ChangeItemsStatus(false, false);
                MotorPositioningUpdate();

                if (use_motor)
                {
                    motorCtrl.GetPosition();
                    motorCtrl.continuousRead(false);
                }


                if (State.Acq.ZStack && State.Acq.nSlices > 1 && use_motor)
                {
                    if (motorCtrl.stack_Position != MotorCtrl.StackPosition.Undefined)
                    {
                        if (motor_back_to_center)
                        {
                            if (motorCtrl.stack_Position == MotorCtrl.StackPosition.Center)
                            {
                                SetCenter();
                                MoveMotorFromCenterToStart();
                            }
                            else
                                MoveMotorBackToStart();
                        }

                        else if (motor_back_to_start)
                        {
                            if (motorCtrl.stack_Position == MotorCtrl.StackPosition.Start)
                                SetTop();
                            else
                                MoveMotorBackToStart();
                        }
                    }
                }

                State.Uncaging.uncage_whileImage = Uncage_while_image_check.Checked;
                State.DO.DO_whileImage = DO_whileImaging_check.Checked;

                if (State.Uncaging.uncage_whileImage && uncaging_panel != null)
                    uncaging_panel.SetupUncage(uncaging_panel);

                if (State.DO.DO_whileImage && digital_panel != null)
                    digital_panel.SetupDO(digital_panel);

                GrabButton.Text = "ABORT";

                if (analyzeAfterEachAcquisition)
                    image_display.plot_regular.Show();
            }
        }

        /// <summary>
        /// Change check for analyze after each acquisition. If it is not run on FLIMage_main, you have to invoke.
        /// </summary>
        /// <param name="on"></param>
        public void TurnOn_AnalyzeAfterAcquisition(bool on)
        {
            analyzeEach.Checked = on;
            analyzeAfterEachAcquisition = on;
        }


        /// <summary>
        /// Called when Loop button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoopButton_Click(object sender, EventArgs e)
        {
            if (flimage_io.focusing)
                StopFocus();

            if (!flimage_io.looping && !flimage_io.grabbing)
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
        public void StartLoop()
        {
            flimage_io.InitializeLoop();
            LoopButton.InvokeIfRequired(o => o.Text = "Stop"); //Should block. (Invoke instead of BeginInvoke)
            GrabButton.InvokeIfRequired(o => o.Enabled = false);
            StartGrab(false);
        }

        public void StopLoop()
        {
            if (flimage_io.looping)
            {
                flimage_io.stopLoopingStatus();
                StopGrab(true);
                LoopButton.InvokeIfRequired(o => o.Text = "LOOP");
                LoopButton.InvokeIfRequired(o => o.Enabled = true);
                flimage_io.UIstopWatch_Image.Stop();
                flimage_io.UIstopWatch_Loop.Stop();
            }
        }

        private void GrabButtonClick(object sender, EventArgs e)
        {
            if (flimage_io.focusing)
            {
                StopFocus();
            }

            if (!flimage_io.grabbing)
            {
                flimage_io.initializeGrabbing();
                StartGrab(false);
            }
            else
            {
                StopGrab(true);
            }
        }

        private void FocusButton_Click(object sender, EventArgs e)
        {
            if (!flimage_io.focusing)
            {
                flimage_io.TryStartFocus();
            }
            else
            {
                StopFocus();
            }
        }

        private void EnsureFiberScanPanel()
        {
            if (fiberScanPanel != null || tbScanParam == null)
                return;

            fiberScanPanel = new Panel();
            fiberScanPanel.Name = "fiberScanPanel";
            fiberScanPanel.Size = new Size(292, 116);
            fiberScanPanel.Location = new Point(10, 10);
            fiberScanPanel.BorderStyle = BorderStyle.FixedSingle;
            fiberScanPanel.Visible = false;

            fiberSamplesPerFrameLabel = new Label();
            fiberSamplesPerFrameLabel.AutoSize = true;
            fiberSamplesPerFrameLabel.Location = new Point(10, 14);
            fiberSamplesPerFrameLabel.Text = "Samples per frame";

            fiberSamplesPerFrame = new TextBox();
            fiberSamplesPerFrame.Name = "FiberSamplesPerFrame";
            fiberSamplesPerFrame.Size = new Size(60, 20);
            fiberSamplesPerFrame.Location = new Point(180, 10);
            fiberSamplesPerFrame.TextAlign = HorizontalAlignment.Right;
            fiberSamplesPerFrame.KeyDown += new KeyEventHandler(this.Generic_KeyDown);

            fiberFrameCountLabel = new Label();
            fiberFrameCountLabel.AutoSize = true;
            fiberFrameCountLabel.Location = new Point(10, 44);
            fiberFrameCountLabel.Text = "Frames";

            fiberFrameCount = new TextBox();
            fiberFrameCount.Name = "FiberFrameCount";
            fiberFrameCount.Size = new Size(60, 20);
            fiberFrameCount.Location = new Point(180, 40);
            fiberFrameCount.TextAlign = HorizontalAlignment.Right;
            fiberFrameCount.KeyDown += new KeyEventHandler(this.Generic_KeyDown);

            fiberSampleIntervalLabel = new Label();
            fiberSampleIntervalLabel.AutoSize = true;
            fiberSampleIntervalLabel.Location = new Point(10, 74);
            fiberSampleIntervalLabel.Text = "Sample interval (ms)";

            fiberSampleInterval = new TextBox();
            fiberSampleInterval.Name = "FiberSampleInterval";
            fiberSampleInterval.Size = new Size(60, 20);
            fiberSampleInterval.Location = new Point(180, 70);
            fiberSampleInterval.TextAlign = HorizontalAlignment.Right;
            fiberSampleInterval.KeyDown += new KeyEventHandler(this.Generic_KeyDown);

            fiberScanPanel.Controls.Add(fiberSamplesPerFrameLabel);
            fiberScanPanel.Controls.Add(fiberSamplesPerFrame);
            fiberScanPanel.Controls.Add(fiberFrameCountLabel);
            fiberScanPanel.Controls.Add(fiberFrameCount);
            fiberScanPanel.Controls.Add(fiberSampleIntervalLabel);
            fiberScanPanel.Controls.Add(fiberSampleInterval);

            tbScanParam.Controls.Add(fiberScanPanel);
            fiberScanPanel.BringToFront();
        }

        private void UpdateFiberScanPanelVisibility()
        {
            EnsureFiberScanPanel();

            bool isFiberPhotometry = (flimage_io != null && flimage_io.microscope_system == MicroscopeSystem.FiberPhotometry)
                || (State?.Acq?.fiberPhotometryMode ?? false);

            if (isFiberPhotometry)
            {
                tbScanParam.Text = "Sampling";
                if (FLIMSetting_tab != null && tb_ScanParameters != null && FLIMSetting_tab.TabPages.Contains(tb_ScanParameters))
                {
                    scanParametersTabIndex = FLIMSetting_tab.TabPages.IndexOf(tb_ScanParameters);
                    if (FLIMSetting_tab.SelectedTab == tb_ScanParameters)
                        FLIMSetting_tab.SelectedTab = tbScanParam ?? FLIMSetting_tab.TabPages[0];
                    FLIMSetting_tab.TabPages.Remove(tb_ScanParameters);
                    scanParametersTabRemoved = true;
                }
                if (!scanPanelVisibilityCached)
                {
                    scanPanelVisibility.Clear();
                    foreach (Control control in tbScanParam.Controls)
                    {
                        if (control == fiberScanPanel)
                            continue;
                        scanPanelVisibility[control] = control.Visible;
                    }
                    scanPanelVisibilityCached = true;
                }

                foreach (Control control in tbScanParam.Controls)
                {
                    if (control == fiberScanPanel)
                        continue;
                    control.Visible = false;
                }

                fiberSamplesPerFrame.Text = State.Acq.linesPerFrame.ToString();
                fiberFrameCount.Text = State.Acq.nFrames.ToString();
                fiberSampleInterval.Text = State.Acq.fiberBin_ms.ToString();
                fiberScanPanel.Visible = true;
            }
            else
            {
                tbScanParam.Text = "Scan";
                if (FLIMSetting_tab != null && tb_ScanParameters != null
                    && scanParametersTabRemoved
                    && !FLIMSetting_tab.TabPages.Contains(tb_ScanParameters))
                {
                    int insertIndex = scanParametersTabIndex;
                    if (insertIndex < 0 || insertIndex > FLIMSetting_tab.TabPages.Count)
                        insertIndex = 0;
                    FLIMSetting_tab.TabPages.Insert(insertIndex, tb_ScanParameters);
                    scanParametersTabRemoved = false;
                }
                fiberScanPanel.Visible = false;
                if (scanPanelVisibilityCached)
                {
                    foreach (var kvp in scanPanelVisibility)
                    {
                        if (kvp.Key != null)
                            kvp.Key.Visible = kvp.Value;
                    }
                }
            }
        }

        private void LineScan_CB_Click(object sender, EventArgs e)
        {
            if (!LineScan_CB.Checked)
                return;

            // Regular galvo-galvo only (no resonant / polygon scanning).
            if (State.Acq.resonantScanning || State.Acq.polygonScanning)
            {
                LineScan_CB.Checked = false;
                MessageBox.Show(Form.ActiveForm, "Line scan trace works only with regular galvo-galvo scanning.\nDisable resonant/polygon scanning first.");
                return;
            }

            if (!TryValidateLineScanTraceSelection(out string reason))
            {
                LineScan_CB.Checked = false;
                MessageBox.Show(Form.ActiveForm, reason);
                return;
            }
        }

        private bool TryValidateLineScanTraceSelection(out string reason)
        {
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

            reason = "";
            return true;
        }


        public void CorrectFillFraction()
        {
            if (flimage_io.microscope_system == MicroscopeSystem.FiberPhotometry)
            {
                State.Acq.fillFraction = 1;
                return;
            }

            bool useResonantFillFraction = State.Acq.resonantScanning || State.Acq.polygonScanning;
            double fillFraction = useResonantFillFraction ? State.Acq.fillFraction_resonant : State.Acq.fillFraction;

            if (fillFraction < 0.25)
                fillFraction = 0.25;
            if (fillFraction > 1)
                fillFraction = 1.0;

            double msPerLine1 = State.Acq.msPerLineActual();

            var pixel_time = msPerLine1 * fillFraction / (double)State.Acq.pixelsPerLine / 1000.0;

            double syncRate1 = State.Spc.datainfo.syncRate[0];
            UInt32 pixel_count;

            if (syncRate1 > State.Acq.ExpectedLaserPulseRate_MHz * 1e6 * 0.9
                && syncRate1 < State.Acq.ExpectedLaserPulseRate_MHz * 1e6 * 1.1)
                syncRate1 = State.Spc.datainfo.syncRate[0];
            else
                syncRate1 = State.Acq.ExpectedLaserPulseRate_MHz * 1e6;

            pixel_count = (UInt32)(pixel_time * syncRate1 + 0.1);
            var new_pixel_time = pixel_count / syncRate1;
            fillFraction = new_pixel_time / msPerLine1 * (double)State.Acq.pixelsPerLine * 1000.1;

            if (useResonantFillFraction)
                State.Acq.fillFraction_resonant = fillFraction;
            else
                State.Acq.fillFraction = fillFraction;
        }


        public void UpdateScanPositionWindow()
        {
            ScanPosition.Invalidate();

            if (scanAreaWindow != null && !scanAreaWindow.IsDisposed)
                scanAreaWindow.UpdateScanPositionWindow();
        }


        /// <summary>
        /// GUI --> State called when GUI is changed.
        /// </summary>
        /// <param name="sdr"></param>
        public void GetParametersFromGUI(object sdr)
        {
            double zoom;
            //

            //Make sure all text is reflected.
            double valD;
            int valI;
            bool isFiberPhotometry = (flimage_io != null && flimage_io.microscope_system == MicroscopeSystem.FiberPhotometry)
                || (State?.Acq?.fiberPhotometryMode ?? false);

            if (sdr != null)
            {
                if (sdr.Equals(fiberSamplesPerFrame))
                    linesPerFrame.Text = fiberSamplesPerFrame.Text;
                if (sdr.Equals(fiberFrameCount))
                    NFrames.Text = fiberFrameCount.Text;
                if (sdr.Equals(fiberSampleInterval))
                    MsPerLine.Text = fiberSampleInterval.Text;
            }

            if (!Double.TryParse(Zoom.Text, out zoom)) zoom = State.Acq.zoom;
            if (zoom != State.Acq.zoom & zoom < 1000 && zoom >= 1)
            {
                State.Acq.zoom = zoom;
            }
            UpdateScanPositionWindow();

            State.Acq.aveFrame = AveFrame_Check.Checked;
            State.Acq.aveFrameA = new bool[] { AveFrame_Check.Checked, AveFrame2_Check.Checked };
            State.Acq.aveSlice = AveSlices_check.Checked;
            State.Acq.ZStack = ZStack_radio.Checked;
            State.Acq.acqFLIMA[0] = FLIM_Radio1.Checked;
            State.Acq.acqFLIMA[1] = FLIM_Radio2.Checked;
            State.Acq.acquisition[0] = Acquisition1.Checked;
            State.Acq.acquisition[1] = Acquisition2.Checked;
            State.Files.channelsInSeparatedFile = SaveInSeparatedFileCheck.Checked;
            image_display.FLIM_ImgData.KeepPagesInMemory = KeepPagesInMemoryCheck.Checked;
            State.Files.fastSaving = FastSaving_CB.Checked;
            if (image_display?.FLIM_ImgData?.State?.Files != null)
                image_display.FLIM_ImgData.State.Files.fastSaving = State.Files.fastSaving;
            if (flimage_io?.State?.Files != null)
                flimage_io.State.Files.fastSaving = State.Files.fastSaving;
            if (flimage_io?.fileIO != null)
                flimage_io.fileIO.HoldFastWriterOpen = ShouldHoldFastWriterOpen();
            State.Acq.aveFrameSeparately = AveFrameSeparately.Checked;
            State.Acq.externalTrigger = ExtTriggerCB.Checked;

            Double msPerLine1, fillFraction1, scanFraction1;

            State.Acq.BiDirectionalScan = BiDirecCB.Checked;
            State.Acq.BidirectionalTriggerPerLine = Trig_Every2.Checked ? 2 : 1;
            State.Acq.BiDirectionalScanY = BiDrecYCheck.Checked;
            State.Acq.SineWaveScan = SineWaveScanning_CB.Checked;
            State.Acq.flipXYScan[0] = FlipX_CB.Checked;
            State.Acq.flipXYScan[1] = FlipY_CB.Checked;
            State.Acq.switchXYScan = SwitchXY_CB.Checked;
            State.Acq.scanZWithPiezo = ZScanWithPiezo.Checked;
            State.Acq.switchXYScanToMotor = SwitchXY_MotorMirror.Checked;
            State.Acq.flipDirectionOfScanToMotor[0] = FlipMotorMirrorX.Checked;
            State.Acq.flipDirectionOfScanToMotor[1] = FlipMotorMirrorY.Checked;

            State.Acq.photon_file_format = photon_file_CB.Checked;
            if (photon_safe_mode_CB != null)
            {
                photon_safe_mode_CB.Enabled = photon_file_CB.Checked;
                PhotonFileHandle.SetPhotonSafeMode(photon_safe_mode_CB.Checked);
            }

            //State.Acq.nSplitScanning = 0;

            if (sdr.Equals(MsPerLine) || sdr.Equals(fiberSampleInterval))
            {
                if (!Double.TryParse(MsPerLine.Text, out msPerLine1))
                    msPerLine1 = State.Acq.msPerLine;

                if (isFiberPhotometry)
                {
                    if (msPerLine1 > 0)
                    {
                        State.Acq.fiberBin_ms = msPerLine1;
                        State.Acq.msPerLine = msPerLine1;
                    }
                }
                else if (!State.Acq.resonantScanning && !State.Acq.polygonScanning)
                {
                    if (msPerLine1 >= State.Init.msPerLine_min)
                    {
                        State.Acq.msPerLine = msPerLine1;
                    }
                }
            }

            //Safety feature. 
            if (!isFiberPhotometry && State.Acq.msPerLine < State.Init.msPerLine_min)
                State.Acq.msPerLine = State.Init.msPerLine_min;

            if (sdr.Equals(ScanFraction))
            {
                if (!Double.TryParse(ScanFraction.Text, out scanFraction1)) scanFraction1 = State.Acq.scanFraction;

                if (scanFraction1 > State.Acq.fillFraction && scanFraction1 >= 0.5 && scanFraction1 <= 0.99)
                {
                    State.Acq.scanFraction = scanFraction1;
                }
            }

            if (sdr.Equals(FillFraction))
            {
                if (!Double.TryParse(FillFraction.Text, out fillFraction1))
                    fillFraction1 = State.Acq.fillFraction;

                if (State.Acq.resonantScanning || State.Acq.polygonScanning)
                {
                    if (fillFraction1 <= 0.99 && fillFraction1 >= 0.2 && fillFraction1 < State.Acq.scanFraction)
                    {
                        State.Acq.fillFraction_resonant = fillFraction1;
                    }
                }
                else
                {
                    State.Acq.fillFraction = fillFraction1;
                }
            }

            if (Double.TryParse(LineClockDelay_us.Text, out double delay1))
                State.Acq.LineClockDelay_us = delay1;

            if (Double.TryParse(ScanDelayUpDown_nano.Text, out double delay2))
            {
                if (State.Acq.resonantScanning || State.Acq.polygonScanning)
                    State.Acq.resonantScanDelay_us = delay2 / 1000.0;
                else
                    State.Acq.ScanDelay = delay2 / 1000.0 / 1000.0; //milliseconds.
            }

            if (Double.TryParse(EOMDelayUpDown_us.Text, out double delay3)) //microsecond.
            {
                if (State.Acq.resonantScanning || State.Acq.polygonScanning)
                    State.Acq.EOMDelay = delay3 / 1000.0;
                else
                    State.Acq.resonantEOMDelay_us = delay3;
            }


            State.Acq.enableMiniScopeClock = EnableMiniScopeClockCheck.Checked;

            ////MiniScope Setting Begin
            double fclk_freq_KHz = 240;
            double sck_freq_KHz = 60;
            Double.TryParse(FClkTextBox.Text, out fclk_freq_KHz);
            Double.TryParse(SClkTextBox.Text, out sck_freq_KHz);
            if (fclk_freq_KHz > 1 && fclk_freq_KHz < 1000)
                State.Acq.FClkFreq = fclk_freq_KHz * 1000;

            if (sck_freq_KHz > 1 && sck_freq_KHz < 1000)
                State.Acq.SClkFreq = sck_freq_KHz * 1000;
            ////MiniScope Setting End

            if (Int32.TryParse(linesPerFrame.Text, out valI))
                State.Acq.linesPerFrame = valI;

            double maxX = State.Acq.XMaxVoltage;
            double maxY = State.Acq.YMaxVoltage;
            double maxXR = State.Acq.XMaxVoltage_Resonant;

            Double.TryParse(MaxRangeX.Text, out maxX);
            Double.TryParse(MaxRangeY.Text, out maxY);
            Double.TryParse(resonantMaxVoltage.Text, out maxXR);

            bool syncResonantMax = (State.Acq.resonantScanning || State.Acq.polygonScanning)
                && resonantMaxVoltage != null
                && !resonantMaxVoltage.Enabled;
            if (syncResonantMax)
                maxXR = maxX;

            State.Acq.XMaxVoltage_Resonant = maxXR;

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

            if (Int32.TryParse(pixelsPerLine.Text, out valI)) State.Acq.pixelsPerLine = valI;
            ApplyOmeTiffSetting(UseOmeTiff_CB.Checked, sdr == UseOmeTiff_CB);
            if (Double.TryParse(SliceInterval.Text, out valD)) State.Acq.sliceInterval = valD;
            if (Double.TryParse(SliceStep.Text, out valD)) State.Acq.sliceStep = valD;
            if (Double.TryParse(ImageInterval.Text, out valD)) State.Acq.imageInterval = valD;

            if (Int32.TryParse(NFrames.Text, out valI)) State.Acq.nFrames = valI;


            if (Int32.TryParse(NSlices.Text, out valI)) State.Acq.nSlices = valI;

            if (State.Acq.nSlices < 1)
                State.Acq.nSlices = 1;

            if (Int32.TryParse(NImages.Text, out valI)) State.Acq.nImages = valI;

            if (Int32.TryParse(N_AveragedFrames1.Text, out valI)) State.Acq.nAveragedFrames = valI;
            if (Int32.TryParse(N_AveragedSlices.Text, out valI)) State.Acq.nAveragedSlices = valI;

            if (Int32.TryParse(FileN.Text, out valI)) State.Files.fileCounter = valI;

            State.Files.baseName = BaseName.Text;
            State.Files.pathName = DirectoryName.Text;

            int focusAverage = 0;
            if (Int32.TryParse(FocusAverage.Text, out valI)) focusAverage = valI;
            if (State.Acq.resonantScanning || State.Acq.polygonScanning)
                State.Acq.nAveFrame_focus_resonant = valI;
            else
                State.Acq.nAveFrame_focus = valI;

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

            //State.Acq.BiDirectionalScan = BiDirecCB.Checked;
            //State.Acq.SineWaveScan = SineWaveScanning_CB.Checked;
            //State.Acq.flipXYScan[0] = FlipX_CB.Checked;
            //State.Acq.flipXYScan[1] = FlipY_CB.Checked;
            //State.Acq.switchXYScanToMotor = SwitchXY_MotorMirror.Checked;
            //State.Acq.flipDirectionOfScanToMotor[0] = FlipMotorMirrorX.Checked;
            //State.Acq.flipDirectionOfScanToMotor[1] = FlipMotorMirrorY.Checked;
            //State.Acq.switchXYScan = SwitchXY_CB.Checked;

            //Double.TryParse(LineTimeCorrection.Text, out State.Spc.spcData.line_time_correction);

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

            if (!isFiberPhotometry && State.Acq.aveSlice && State.Acq.nAveFrame > 1)
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
            if (!isFiberPhotometry)
            {
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
            }
            else
            {
                State.Acq.nAveFrame = 1;
                State.Acq.nAveragedFrames = State.Acq.nFrames;
                State.Acq.aveFrame = false;
                State.Acq.aveFrameA = new bool[] { false, false };
                State.Acq.aveSlice = false;
                State.Acq.nAveSlice = 1;
                State.Acq.nAveragedSlices = 1;
                State.Acq.nSlices = 1;
                State.Acq.ZStack = false;
            }

            State.Acq.SliceMergin = 0;

            //Calculation of slice intervals.
            if (!State.Acq.ZStack)
            {
                var msPerLine = State.Acq.msPerLineActual(); ;
                if (State.Acq.sliceInterval < State.Acq.frameInterval() + State.Acq.SliceMergin / 1000)
                    State.Acq.sliceInterval = State.Acq.frameInterval() + State.Acq.SliceMergin / 1000;
            }


            State.Acq.nStripes = (int)Math.Ceiling((double)State.Acq.linesPerFrame / (double)State.Acq.nStripes);

            analyzeAfterEachAcquisition = analyzeEach.Checked;

            State.Acq.resonantScanning = ResonantCheckBox.Checked;

            flimage_io.SafetyFeature();
            this.BeginInvokeIfRequired(o => o.FillGUI());
            SaveSetting();
        }

        public void UpdateCounters()
        {
            if (!flimage_io.focusing)
            {
                CurrentFrame.Text = flimage_io.internalFrameCounter.ToString();
                if (flimage_io.internalSliceCounter + 1 <= State.Acq.nSlices)
                    CurrentSlice.Text = (flimage_io.internalSliceCounter + 1).ToString();
                if (flimage_io.internalImageCounter + 1 <= State.Acq.nSlices)
                    CurrentImage.Text = (flimage_io.internalImageCounter + 1).ToString();

            }
            else
            {
                CurrentFrame.Text = flimage_io.internalFrameCounter.ToString();
            }
        }

        public void UpdateSavedNumberOfFile()
        {
            if (SavedFileN != null)
                SavedFileN.Text = (flimage_io.savePageCounterTotal + 1).ToString();
        }

        public void UpdateMeasuredSliceInterval()
        {
            if (flimage_io.measuredSliceInterval != 0)
                Measured_slice_interval.Text = String.Format("{0:0.00} s", flimage_io.measuredSliceInterval);
            //CurrentSlice.Text = (flimage_io.internalSliceCounter).ToString();
        }

        public void UpdateAverageFrameCounter()
        {
            try
            {
                if (!flimage_io.focusing)
                {
                    if (State.Acq.aveSlice)
                    {
                        if (flimage_io.internalFrameCounter == 0 && flimage_io.averageSliceCounter == 0)
                            nAverageFrame.Text = (State.Acq.nAveSlice * State.Acq.nAveFrame).ToString();
                        else
                            nAverageFrame.Text = (flimage_io.internalFrameCounter + flimage_io.averageSliceCounter * State.Acq.nAveFrame).ToString();
                    }
                    else if (State.Acq.aveFrameA[0])
                        nAverageFrame.Text = (flimage_io.averageCounter + 1).ToString();
                    else
                        nAverageFrame.Text = "1";
                }
                else
                {
                    nAverageFrame.Text = "1";
                }
            }
            catch
            {
                Debug.WriteLine("****FLIM control may not exist anymore****");
            }
        }

        public void PQ_SettingGUI()
        {
            int valI;
            double valD;

            State.Spc.spcData.BoardType = State.Init.FLIM_mode;

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

            State.Spc.spcData.CFD_on = CFD_on_CB.Checked ? 1 : 0;
            State.Spc.spcData.sync_trigger_edge = rising_sync_CB.Checked ? 1 : 0;
            State.Spc.spcData.input_trigger_edge = Rising_Input_CB.Checked ? 1 : 0;


            //for (int i = 0; i < State.Spc.spcData.sync_threshold.Length; i++)
            //{
            //    if (State.Spc.spcData.sync_threshold[i] > 0)
            //        State.Spc.spcData.sync_threshold[i] = -State.Spc.spcData.sync_threshold[i];
            //}

            //for (int i = 0; i < State.Spc.spcData.sync_zc_level.Length; i++)
            //{
            //    if (State.Spc.spcData.sync_zc_level[i] > 0)
            //        State.Spc.spcData.sync_zc_level[i] = -State.Spc.spcData.sync_zc_level[i];
            //}

            //for (int i = 0; i < State.Spc.spcData.ch_threshold.Length; i++)
            //{
            //    if (State.Spc.spcData.ch_threshold[i] > 0)
            //        State.Spc.spcData.ch_threshold[i] = -State.Spc.spcData.ch_threshold[i];
            //}

            //for (int i = 0; i < State.Spc.spcData.ch_zc_level.Length; i++)
            //{
            //    if (State.Spc.spcData.ch_zc_level[i] > 0)
            //        State.Spc.spcData.ch_zc_level[i] = -State.Spc.spcData.ch_zc_level[i];
            //}

            if (fastZcontrol != null)
            {
                fastZcontrol.UpdateStateFromGUI(this);
            }
            else
                State.Acq.fastZScan = false;

            if (Int32.TryParse(FreqDivBox.Text, out valI))
            {
                for (int i = 0; i < State.Spc.spcData.sync_divider.Length; i++)
                    State.Spc.spcData.sync_divider[i] = valI;
            }

            if (flimage_io.use_pq)
            {
                if (Binning_setting.SelectedIndex >= 0)
                    State.Spc.spcData.binning = Binning_setting.SelectedIndex;
                else if (State.Spc.spcData.BoardType == "PH" || State.Spc.spcData.BoardType == "HH")
                    State.Spc.spcData.binning = 0;

                if (State.Spc.spcData.BoardType == "PH")
                    State.Spc.spcData.binning += PH330_Binning_Offset;
                else if (State.Spc.spcData.BoardType == "HH")
                    State.Spc.spcData.binning += HH500_Binning_Offset;

                //if (Int32.TryParse(binning.Text, out valI)) State.Spc.spcData.binning = valI;
                if (Int32.TryParse(NTimePoints.Text, out valI)) State.Spc.spcData.n_dataPoint = valI;
                if (Int32.TryParse(StartPointBox.Text, out valI)) State.Spc.spcData.startPoint = valI;

                if (PQMode_Pulldown.SelectedIndex == 0)
                    State.Spc.spcData.acq_modePQ = 3;
                else if (PQMode_Pulldown.SelectedIndex == 1)
                    State.Spc.spcData.acq_modePQ = 2;

                if (State.Spc.spcData.BoardType == "PH")
                    State.Spc.spcData.resolution = new double[] { 1, 1 };
                else if (State.Spc.spcData.BoardType == "HH")
                    State.Spc.spcData.resolution = new double[] { 1, 1 };
                else if (State.Spc.spcData.BoardType == "PQ" && State.Spc.spcData.HW_Model.Contains("N"))
                    State.Spc.spcData.resolution = new double[] { 250, 250 };
                else
                    State.Spc.spcData.resolution = new double[] { 25, 25 }; //Default

                for (int i = 0; i < State.Spc.spcData.resolution.Length; i++)
                {
                    State.Spc.spcData.resolution[i] *= Math.Pow(2, State.Spc.spcData.binning);
                }

            }
            else if (flimage_io.use_bh)
            {
                if (Int32.TryParse(Resolution_Pulldown.SelectedItem.ToString(), out valI)) State.Spc.spcData.n_dataPoint = valI;
                State.Spc.spcData.adc_res = (short)(Math.Log(State.Spc.spcData.n_dataPoint, 2));
            }

            SetupFLIMParameters_GUI(State);
        }

        public void changeScanArea(ROI roi)
        {
            if (State.Acq.resonantScanning || State.Acq.polygonScanning)
            {
                MessageBox.Show("This function does not work for resonant/polygon scanning");
                return;
            }

            var FLIM_ImgData = image_display.FLIM_ImgData;

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

            State.Acq.nSplitScanning = 0;

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

            double zoom = (double)(int)(FLIM_ImgData.State.Acq.zoom * ratio * 10) / 10;

            if (zoom != State.Acq.zoom & zoom < 1000 && zoom >= 1)
            {
                ZoomChangedFunc();
            }

            SetParametersFromState(true);
        }

        public void SetupFLIMParameters_GUI(ScanParameters state1)
        {
            flimage_io.SetupFLIMParameters(state1);
            this.BeginInvokeIfRequired(o => o.UpdateSPC_GUI());
        }

        public void Fill_OffsetGUI()
        {

            if (State.Acq.XOffset_Split.All(x => x == 0) && State.Acq.YOffset_Split.All(x => x == 0))
            {
                SetYOffsetDefault();
            }

            double[] offset_um;
            double[] offset_volt;
            double rot;

            if (State.Acq.nSplitScanning > 1)
            {
                offset_volt = new double[] { State.Acq.XOffset_Split[current_splitScanLocation],
                        State.Acq.YOffset_Split[current_splitScanLocation] };
                rot = State.Acq.Rotation_Split[current_splitScanLocation];
            }
            else
            {
                offset_volt = new double[] { State.Acq.XOffset, State.Acq.YOffset };
                rot = State.Acq.Rotation;
            }

            offset_um = ImageParameterCalculation.voltage2micrometers_XY(offset_volt, State);

            XOffset.Text = string.Format("X {0:0.00} ({1:0.0}μm)", offset_volt[0], offset_um[0]);
            YOffset.Text = string.Format("Y {0:0.00} ({1:0.0}μm)", offset_volt[1], offset_um[1]);
            Rotation_EditBox.Text = string.Format("{0:0.0}", rot);

            UpdateScanPositionWindow();

        }

        private IEnumerable<Control> GetAlllControl(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control.HasChildren)
                {
                    foreach (var child in GetAlllControl(control))
                    {
                        if (child.Tag != null)
                            yield return child;
                    }
                }

                if (control.Tag != null)
                {
                    yield return control;
                }
            }
        }

        // KENGO BEGIN 2025-10-29
        // add "NAGrab" tag so that keeping "z stack panel" enabled during focusing
        public void SetEnableStatesOfControls()
        {
            foreach (Control control in GetAlllControl(this))
            {
                bool grab_disable = control.Tag.ToString().Contains("NGrab");
                bool grab_disable2 = control.Tag.ToString().Contains("NAGrab"); // special case: keep enabled during focus
                bool advanced_enable = control.Tag.ToString().Contains("Adv");
                bool resonant_disable = control.Tag.ToString().Contains("NRes");
                bool galvo_disable = control.Tag.ToString().Contains("NGG");
                bool miniscope_enable = control.Tag.ToString().Contains("miniScope");

                // When Tag contains "NAGrab" and 'focus' is true, grab_disable_effective = false
                bool grab_disable_effective = grab_disable2 ? !flimage_io.focusing : grab_disable;

                bool busy = flimage_io.grabbing || flimage_io.focusing;

                if (grab_disable_effective || advanced_enable || resonant_disable)
                {
                    control.Enabled = (!(grab_disable_effective && busy && !flimage_io.read_photon_file) || !grab_disable_effective)
                        && (advanced_enable && AdvancedCheck.Checked || !advanced_enable)
                        && (!(resonant_disable && State.Acq.resonantScanning) || !resonant_disable);
                }

                if (galvo_disable || miniscope_enable)
                {
                    control.Visible = (galvo_disable && State.Acq.resonantScanning || !galvo_disable)
                        && (miniscope_enable && flimage_io.microscope_system == MicroscopeSystem.MiniScope || !miniscope_enable);
                }
            }
        }
        // KENGO END 2025-10-29


        /// <summary>
        /// State --> GUI.
        /// </summary>
        public void FillGUI()
        {
            SetEnableStatesOfControls();

            State.Acq.resonantScanning = State.Acq.resonantScanning && State.Init.enableResonantScanner;
            bool isFiberPhotometry = (flimage_io != null && flimage_io.microscope_system == MicroscopeSystem.FiberPhotometry)
                || (State?.Acq?.fiberPhotometryMode ?? false);

            AveFrame_Check.Checked = State.Acq.aveFrame;    //State.Acq.aveFrameA[0];
            AveFrame2_Check.Checked = State.Acq.aveFrame;   //State.Acq.aveFrameA[1];
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
            BiDirecCB.Checked = State.Acq.BiDirectionalScan;
            Trig_Every2.Checked = State.Acq.BidirectionalTriggerPerLine == 2;
            BiDrecYCheck.Checked = State.Acq.BiDirectionalScanY;
            SineWaveScanning_CB.Checked = State.Acq.SineWaveScan;
            SwitchXY_CB.Checked = State.Acq.switchXYScan;
            ZScanWithPiezo.Checked = State.Acq.scanZWithPiezo;
            if (State.Acq.nSplitScanning == 2)
                SplitDrop.SelectedIndex = 1;
            else if (State.Acq.nSplitScanning == 4)
                SplitDrop.SelectedIndex = 2;
            else
                SplitDrop.SelectedIndex = 0;
            FlipX_CB.Checked = State.Acq.flipXYScan[0];
            FlipY_CB.Checked = State.Acq.flipXYScan[1];
            SwitchXY_MotorMirror.Checked = State.Acq.switchXYScanToMotor;
            FlipMotorMirrorX.Checked = State.Acq.flipDirectionOfScanToMotor[0];
            FlipMotorMirrorY.Checked = State.Acq.flipDirectionOfScanToMotor[1];

            ResonantCheckBox.Checked = State.Acq.resonantScanning;
            photon_file_CB.Checked = State.Acq.photon_file_format;
            photon_safe_mode_CB.Checked = PhotonFileHandle.GetPhotonSafeModeEnabled();
            photon_safe_mode_CB.Enabled = photon_file_CB.Checked;
            UseOmeTiff_CB.Checked = State.Files.useOmeTiff;


            ResonantScan_TurnOnOff(State.Acq.resonantScanning);

            Fill_OffsetGUI();

            int nAve;
            if (State.Acq.aveFrame)
                nAve = State.Acq.nAveFrame;
            else
                nAve = 1;

            SliceInterval.Text = string.Format("{0:0.00}", State.Acq.sliceInterval);
            ImageInterval.Text = string.Format("{0:0.00}", State.Acq.imageInterval);
            FrameInterval.ReadOnly = true;
            FrameInterval.BackColor = System.Drawing.SystemColors.Control;
            double frame_s = State.Acq.frameInterval();
            FrameInterval.Text = string.Format("{0:0.000}", frame_s);
            aveFrame_Interval.Text = string.Format("{0:0.000}", frame_s * nAve);

            if (State.Acq.aveSlice)
                nAve = State.Acq.nAveSlice;
            else
                nAve = 1;

            aveSlice_Interval.Text = string.Format("{0:0.000}", State.Acq.sliceInterval * nAve);
            SliceStep.Text = State.Acq.sliceStep.ToString("F1"); //string.Format("{0:0.0}", State.Acq.sliceStep);

            Zoom.Text = string.Format("{0:0.0}", State.Acq.zoom);


            SClkTextBox.Text = (State.Acq.SClkFreq / 1000).ToString();
            FClkTextBox.Text = (State.Acq.FClkFreq / 1000).ToString();

            //if (State.Acq.resonantScanning)
            //    Rotation.Text = "0";
            //else
            //    Rotation.Text = string.Format("{0:0.0}", State.Acq.Rotation);

            MaxRangeX.Text = String.Format("{0:0.0}", State.Acq.XMaxVoltage);
            MaxRangeY.Text = String.Format("{0:0.0}", State.Acq.YMaxVoltage);
            resonantMaxVoltage.Text = String.Format("{0:0.0}", State.Acq.XMaxVoltage_Resonant);

            FormControllers.PulldownSelectByItemString(Objective_Pulldown, "x" + State.Acq.object_magnification);

            double[] fov = State.Acq.FOV_calculation(State.Acq.object_magnification);
            FieldOfViewX.Text = string.Format("{0:0.0}", State.Acq.field_of_view[0]);
            FieldOfViewY.Text = string.Format("{0:0.0}", State.Acq.field_of_view[1]);

            CurrentFOVX.Text = string.Format("{0:0.0}", fov[0]);
            CurrentFOVY.Text = string.Format("{0:0.0}", fov[1]);

            linesPerFrame.Text = Convert.ToString(State.Acq.linesPerFrame);

            if (!isFiberPhotometry && State.Acq.msPerLine < State.Init.msPerLine_min)
                State.Acq.msPerLine = State.Init.msPerLine_min;

            pixelsPerLine.Text = Convert.ToString(State.Acq.pixelsPerLine);

            int index = NPixels_PulldownX.FindStringExact(State.Acq.pixelsPerLine.ToString());
            if (index < 0)
                index = NPixels_PulldownX.FindStringExact("128");
            NPixels_PulldownX.SelectedIndex = index;

            index = NPixels_PulldownY.FindStringExact(State.Acq.linesPerFrame.ToString());
            if (index < 0)
                index = NPixels_PulldownY.FindStringExact("128");
            NPixels_PulldownY.SelectedIndex = index;

            if (!flimage_io.read_photon_file)
                CorrectFillFraction();

            MsPerLine.Text = Convert.ToString(State.Acq.msPerLineActual());

            if (State.Acq.resonantScanning || State.Acq.polygonScanning)
            {
                ScanFraction.Text = "1";
                FillFraction.Text = string.Format("{0:0.000}", State.Acq.fillFraction_resonant);
                pixelTime.Text = string.Format("{0:0.000}", 500 * State.Acq.fillFraction_resonant / State.Init.resonantFreq_Hz / State.Acq.pixelsPerLine * 1e3);
                ScanDelayUpDown_nano.Text = string.Format("{0}", State.Acq.resonantScanDelay_us * 1000.0);
                EOMDelayUpDown_us.Text = string.Format("{0}", State.Acq.resonantEOMDelay_us);
            }
            else
            {
                ScanFraction.Text = string.Format("{0:0.000}", State.Acq.scanFraction);
                FillFraction.Text = string.Format("{0:0.000}", State.Acq.fillFraction);
                pixelTime.Text = string.Format("{0:0.000}", State.Acq.PixelTime() * 1e6);
                ScanDelayUpDown_nano.Text = string.Format("{0}", State.Acq.ScanDelay * 1.0e6);
                EOMDelayUpDown_us.Text = string.Format("{0}", State.Acq.EOMDelay * 1000.0);
            }

            LineClockDelay_us.Text = string.Format("{0}", State.Acq.LineClockDelay_us);

            NFrames.Text = State.Acq.nFrames.ToString();

            UpdateFiberScanPanelVisibility();

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

            if (State.Acq.resonantScanning || State.Acq.polygonScanning)
                FocusAverage.Text = State.Acq.nAveFrame_focus_resonant.ToString();
            else
                FocusAverage.Text = State.Acq.nAveFrame_focus.ToString();

            N_AveSlices.Text = State.Acq.nAveSlice.ToString();


            //LineTimeCorrection.Text = String.Format("{0:0.0000}", State.Spc.spcData.line_time_correction);

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

            // Kengo BEGIN 05-22-2025
            // To update State.Motor.StepXY,stepZ,velocity with the remote commands
            XYMotorStep.Text = State.Motor.stepXY.ToString();
            ZMotorStep.Text = State.Motor.stepZ.ToString();
            if (use_motor && motorCtrl != null)
            {
                if (State.Motor.velocity[0] <= motorCtrl.maxVelocity[0])
                    motorCtrl.SetVelocity(State.Motor.velocity);
            }
            // Kengo END

            //PQ Update.
            UpdateSPC_GUI();

            UpdateFileName();

            //This activates value change process.
            Zoom100.Value = Math.Floor((Decimal)State.Acq.zoom / 100);
            Zoom10.Value = Math.Floor(((Decimal)State.Acq.zoom - Zoom100.Value * 100) / 10);
            Zoom1.Value = Math.Floor(((Decimal)State.Acq.zoom - Zoom100.Value * 100 - Zoom10.Value * 10));
            ZoomP1.Value = Math.Floor(((Decimal)State.Acq.zoom - Math.Floor((Decimal)State.Acq.zoom)) * 10);

            UpdatePowerGUI();

            if (uncaging_panel != null)
                uncaging_panel.UpdateUncaging(this);

            if (digital_panel != null)
                digital_panel.UpdateDO(this);

            Uncage_while_image_check.Checked = State.Uncaging.uncage_whileImage;
            DO_whileImaging_check.Checked = State.DO.DO_whileImage;

            UpdateScanPositionWindow();
        }

        private void UpdateSPC_GUI()
        {
            UpdateSPC_GUI(flimage_io);
        }


        public void UpdateSPC_GUI(FLIMage_IO flimage_io_in)
        {
            flimage_io = flimage_io_in;

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

            CFD_on_CB.Checked = State.Spc.spcData.CFD_on == 1;
            rising_sync_CB.Checked = State.Spc.spcData.sync_trigger_edge == 1;
            Rising_Input_CB.Checked = State.Spc.spcData.input_trigger_edge == 1;

            resolution.Text = String.Format("{0:0.000}", State.Spc.spcData.resolution[0]);
            resolution2.Text = String.Format("{0:0.000}", State.Spc.spcData.resolution[0]);

            var parameters = flimage_io.parameters;

            if (parameters != null && fastZcontrol != null)
            {
                fastZcontrol.GetParamFromFLIMage(this);
                fastZcontrol.CalculateFastZParameters();
            }
            else
                State.Acq.fastZScan = false;

            FreqDivBox.Text = State.Spc.spcData.sync_divider[0].ToString();

            if (flimage_io.use_bh)
            {
                resolution2.Text = String.Format("{0:0.000}", State.Spc.spcData.resolution[1]);

                int n_bit = (int)Math.Ceiling(Math.Log(State.Spc.spcData.n_dataPoint, 2));
                if (Binning_setting.Items.Count > n_bit - 6)
                    Resolution_Pulldown.SelectedIndex = n_bit - 6;
            }
            else if (flimage_io.use_pq)
            {

                State.Spc.spcData.resolution[1] = State.Spc.spcData.resolution[0];

                NTimePoints.Text = Convert.ToString(State.Spc.spcData.n_dataPoint);
                StartPointBox.Text = Convert.ToString(State.Spc.spcData.startPoint);

                if (Binning_setting != null && PQMode_Pulldown != null)
                {
                    if (State.Spc.spcData.BoardType == "PH")
                    {
                        int bin1 = State.Spc.spcData.binning - PH330_Binning_Offset;
                        if (Binning_setting.Items.Count > bin1 && bin1 >= 0)
                            Binning_setting.SelectedIndex = bin1;
                        else
                            Binning_setting.SelectedIndex = 0;
                    }
                    else if (State.Spc.spcData.BoardType == "HH")
                    {
                        int bin1 = State.Spc.spcData.binning - HH500_Binning_Offset;
                        if (Binning_setting.Items.Count > bin1 && bin1 >= 0)
                            Binning_setting.SelectedIndex = bin1;
                        else
                        {
                            Binning_setting.SelectedIndex = 0;
                            State.Spc.spcData.binning = HH500_Binning_Offset;
                        }
                    }
                    else
                    {
                        if (Binning_setting.Items.Count > State.Spc.spcData.binning)
                            Binning_setting.SelectedIndex = State.Spc.spcData.binning;
                    }

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

            SaveInSeparatedFileCheck.Checked = State.Files.channelsInSeparatedFile;
            AveFrameSeparately.Checked = State.Acq.aveFrameSeparately;

            FileN.Text = Convert.ToString(State.Files.fileCounter);
            BaseName.Text = State.Files.baseName;
            DirectoryName.Text = State.Files.pathName;
            DirectoryName.Select(DirectoryName.TextLength + 1, 0);
            FullFileName.Text = State.Files.fullName();
            FullFileName.Select(FullFileName.TextLength + 1, 0);
            FileName.Text = State.Files.fileName;

            if (flimage_io != null)
                CurrentImage.Text = flimage_io.internalImageCounter.ToString();
        }

        /// <summary>
        /// When called from different thread, the function is invoekd from this thread.
        /// </summary>
        public void UpdateFileName()
        {
            this.BeginInvokeIfRequired(o => o.UpdateFileNameCore());
        }

        /// <summary>
        /// Enable/disable controls when focusing/grabbing.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="focus"></param>
        public void ChangeItemsStatus(bool status, bool focus)
        {
            SetEnableStatesOfControls();

            image_display.ChangeRealtimeStatus(!status);

            if (status)
            {
                GrabButton.Enabled = true;
                FocusButton.Enabled = true;
            }

            if (focus && !status || flimage_io.imageSequencing)
                GrabButton.Enabled = false;
            else if (!focus && !status)
                FocusButton.Enabled = false;


        }

        private void AdvancedCheck_CheckedChanged(object sender, EventArgs e)
        {
            FillGUI();
        }


        public void SetParametersFromState(bool notify)
        {
            FillGUI();
            GetParametersFromGUI(this);

            if (State.Uncaging.uncage_whileImage && uncaging_panel != null)
                uncaging_panel.Show();

            if (State.DO.DO_whileImage && digital_panel != null)
                digital_panel.Show();

            if (use_motor)
            {
                motorCtrl.ZStack_Stepsize = State.Acq.sliceStep;
                motorCtrl.ZStack_nSlices = State.Acq.nSlices;
            }

            flimage_io.updateState(State);
        }

        public void LoadSettingFile(String fileName, bool loadScanSetting)
        {
            ScanParameters copyState = fileIO.CopyState();
            fileIO.LoadSetupFile(fileName);
            State = fileIO.State;

            State.Files = copyState.Files;
            State.Display = copyState.Display;
            State.Uncaging.Position = copyState.Uncaging.Position;
            State.Uncaging.PositionV = copyState.Uncaging.PositionV;
            State.Uncaging.CalibV = copyState.Uncaging.CalibV;
            State.Uncaging.Calib_beta = copyState.Uncaging.Calib_beta;

            State.Acq.power = copyState.Acq.power;
            if (!flimage_io.read_photon_file)
            {
                //These values are unchanged by opening setup file from the menu.
                State.Init = copyState.Init;
                State.Spc = copyState.Spc;
                State.Motor = copyState.Motor;
                //State.Acq.photon_file_format = copyState.Acq.photon_file_format;

                if (!loadScanSetting)
                {
                    State.Acq.XOffset = copyState.Acq.XOffset;
                    State.Acq.YOffset = copyState.Acq.YOffset;

                    State.Acq.nSplitScanning = copyState.Acq.nSplitScanning;
                    State.Acq.XOffset_Split = (double[])copyState.Acq.XOffset_Split.Clone();
                    State.Acq.YOffset_Split = (double[])copyState.Acq.YOffset_Split.Clone();

                    State.Acq.YOffset_Resonant = copyState.Acq.YOffset_Resonant;

                    if (!State.Acq.resonantScanning)
                        State.Acq.zoom = copyState.Acq.zoom;

                    State.Acq.Rotation = copyState.Acq.Rotation;
                    State.Acq.Rotation_Split = (double[])copyState.Acq.Rotation_Split.Clone();

                    State.Acq.field_of_view = copyState.Acq.field_of_view;
                    State.Acq.ScanDelay = copyState.Acq.ScanDelay;
                    //State.Acq.resonantScanDelay_us = copyState.Acq.resonantScanDelay_us;
                    State.Acq.resonantEOMDelay_us = copyState.Acq.resonantEOMDelay_us;
                    State.Acq.EOMDelay = copyState.Acq.EOMDelay;
                    State.Acq.LineClockDelay_us = copyState.Acq.LineClockDelay_us;

                    State.Acq.fillFraction = copyState.Acq.fillFraction;
                    State.Acq.scanFraction = copyState.Acq.scanFraction;
                    State.Acq.XMaxVoltage = copyState.Acq.XMaxVoltage;
                    State.Acq.YMaxVoltage = copyState.Acq.YMaxVoltage;
                    State.Acq.XMaxVoltage_Resonant = copyState.Acq.XMaxVoltage_Resonant;
                    State.Acq.BiDirectionalScan = copyState.Acq.BiDirectionalScan;
                    State.Acq.BidirectionalTriggerPerLine = copyState.Acq.BidirectionalTriggerPerLine;
                    State.Acq.SineWaveScan = copyState.Acq.SineWaveScan;
                    State.Acq.flipXYScan = copyState.Acq.flipXYScan;
                    State.Acq.switchXYScan = copyState.Acq.switchXYScan;

                    //Motor per mirror
                    State.Acq.flipDirectionOfScanToMotor = copyState.Acq.flipDirectionOfScanToMotor;
                    State.Acq.switchXYScanToMotor = copyState.Acq.switchXYScanToMotor;

                    //State.Acq.aveFrameA = copyState.Acq.aveFrameA;
                    State.Acq.acquisition = copyState.Acq.acquisition;
                    //State.Acq.acqFLIMA = copyState.Acq.acqFLIMA;
                    State.Acq.aveFrameSeparately = copyState.Acq.aveFrameSeparately;
                }
            }
            //
            SetParametersFromState(true);
            ZoomChangedFunc();
        }

        private void Calibrate1_Click(object sender, EventArgs e)
        {
            flimage_io.CalibEOM(true);
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
            DO_whileImaging_check.Checked = State.DO.DO_whileImage;

            if (uncaging_panel != null && uncaging_panel.Visible)
                uncaging_panel.UpdateUncaging(this);

            fileIO = new FileIO(State);
            fileIO.SaveDeviceFile();
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
            SetParametersFromState(true);
        }

        /////////////////////////////// PRESET END

        void AveFrame_CheckedChanged(object sender, EventArgs e)
        {

        }

        void SaveSetting()
        {
            State.Spc.analysis = image_display.FLIM_ImgData.State.Spc.analysis;
            image_display.ExportStateDisplay(State);
            Directory.CreateDirectory(State.Files.initFolderPath);
            File.SetAttributes(State.Files.initFolderPath, FileAttributes.Normal);
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

            System.Windows.Forms.Application.DoEvents();
            try
            {
                SaveSetting();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            try
            {
                if (use_motor)
                {
                    motorCtrl.unsubscribe();
                    motorCtrl.MotH -= MotorListener;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            try
            {
                flimage_io.StopNIDAQIOControls();
                flimage_io.TCSPC_Close();
                flimage_io.ECU_Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            try
            {
                // Close the pipe server directly so it is shut down regardless of
                // whether script (RemoteControl) has already been disposed during
                // the closing cascade. COMserver.Close() no longer hangs when a
                // client is connected, and its worker threads are background
                // threads, so FLIMage can terminate cleanly and the named pipes are
                // released before the next launch.
                // by Kengo(Claude) 06-09-2026
                com_server?.Close();

                // Stop the command-serialization worker and drain its queue so
                // the background worker thread exits cleanly on shutdown.
                // by Kengo(Claude) 06-10-2026
                flim_event?.StopCommandWorker();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
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
                if (flimage_io.focusing)
                    StopFocus();

                if (flimage_io.grabbing)
                {
                    StopGrab(true);
                }

                flimage_io.initializeGrabbing();
                StartGrab(false);
            }
            else if (command == "StartLoop")
            {
                StartLoop();
            }
            else if (command == "StopLoop")
            {
                StopLoop();
            }
            else if (command == "AbortGrab" || command == "StopGrab")
            {
                if (flimage_io.focusing)
                    StopFocus();

                StopGrab(true);
            }
            else if (command == "StartDO")
            {
                this.Invoke((Action)delegate
                {
                    if (digital_panel == null)
                        digital_panel = new Digital_Trigger_Panel(this);

                    digital_panel.Show();
                    digital_panel.Activate();

                    if (!digital_panel.digital_running && !flimage_io.grabbing && !flimage_io.focusing)
                        digital_panel.Start_button_Click(digital_panel, null);
                });
            }
            else if (command == "StopDO")
            {
                if (digital_panel != null && !digital_panel.IsDisposed)
                {
                    this.Invoke((Action)delegate
                    {
                        if (digital_panel.digital_running)
                            digital_panel.Stop_DO();
                    });
                }

            }
            else if (command == "StartUncaging")
            {
                this.Invoke((Action)delegate
                {
                    if (uncaging_panel == null)
                        uncaging_panel = new Uncaging_Trigger_Panel(this);

                    uncaging_panel.Show();
                    uncaging_panel.Activate();
                    image_display.uncaging_on = true;
                    image_display.Activate_uncaging(true, true);

                    if (!uncaging_panel.uncaging_running && !flimage_io.grabbing && !flimage_io.focusing)
                        uncaging_panel.StartUncaging_button_Click(uncaging_panel, null);
                });
            }
            else if (command == "StopUncaging")
            {
                if (uncaging_panel != null && !uncaging_panel.IsDisposed)
                {
                    this.Invoke((Action)delegate
                    {
                        if (uncaging_panel.uncaging_running)
                            uncaging_panel.StopUncaging();
                    });
                }
            }
            else if (command == "StartEphys")
            {
                this.Invoke((Action)delegate
                {
                    if (physiology == null)
                        physiology = new StimPanel(true);

                    physiology.Show();
                    physiology.Activate();

                    if (!physiology.stim_running && !flimage_io.grabbing && !flimage_io.focusing)
                        physiology.StartButton_Click(physiology, null);
                });
            }
            else if (command == "StopEphys")
            {
                if (physiology != null && !physiology.IsDisposed)
                {
                    this.Invoke((Action)delegate
                    {
                        if (physiology.stim_running)
                            physiology.StopRepeat();
                    });
                }
            }
            else if (command == "DODone")
            {
                flimage_io.NotifyEventExternal("DODone");
            }
            else if (command == "UncagingDone")
            {
                flimage_io.NotifyEventExternal("UncagingDone");
            }
            else if (command == "EphysDone")
            {
                flimage_io.NotifyEventExternal("EphysDone");
            }
            else if (command == "LoadSettingWithNumber")
            {
                int defaultVal = 1;
                int argnumber = 1;
                if (!Int32.TryParse(argument, out argnumber)) return false; //Kengo 05-23-2025 argnumber = defaultVal;
                LoadSettingAsNumber(argnumber);
            }
            else if (command == "LoadSettingFile")
            {
                if (File.Exists(argument))
                {
                    LoadSettingFile(argument, false);
                }
                else
                {
                    var fileName = Path.Combine(State.Files.initFolderPath, argument);
                    LoadSettingFile(fileName, false);
                }
            }
            // Kengo BEGIN 06-01-2025
            // add SaveSetting command
            else if (command == "SaveSetting")
            {
                if (Int32.TryParse(argument, out int argnumber))
                    SaveSettingAsNumber(argnumber);
                else
                {
                    string filename = argument.Trim();
                    if (filename == "")
                        fileIO.SaveSetupFile();
                    else
                    {
                        try
                        {
                            State.Files.initFileName = filename;
                            File.WriteAllText(filename, fileIO.AllSetupValues_nonDevice());
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: Could not write file. " + ex.Message);
                        }
                    }
                }
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            // Kengo END
            else if (command == "Focus")
            {
                FocusButton_Click(this, null);
            }
            else if (command == "SetMotorPosition")
            {
                SetMotorPosition(false, true, true); //Block the thread until movement done.
            }
            else if (command == "MovePiezoStep")
            {
                double step_um = 0;
                double return_um = 0;
                if (double.TryParse(argument, out step_um))
                {
                    return_um = flimage_io.piezo.move_Piezo_1step_um(step_um);
                }
            }
            // Kengo BEGIN 05-22-2025 Add
            else if (command == "StopMotor")
            {
                if (motorCtrl != null)
                    motorCtrl.Stop();
            }
            else if (command == "SetZeroAll")
            {
                motorCtrl.Zero_All();
            }
            // Kengo END
            else if (command == "UpdateGUI")
            {
                ReSetupValues(true);
            }
            else if (command == "SetOverwriteWarningOff")
            {
                if (flimage_io != null)
                    flimage_io.suppressOverwritePrompt = true;
                flimage_io?.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "SetOverwriteWarningOn")
            {
                if (flimage_io != null)
                    flimage_io.suppressOverwritePrompt = false;
                flimage_io?.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "OpenFile")
            {
                image_display.InvokeIfRequired(o =>
                {
                    o.OpenFLIM(argument, true, image_display.plot_regular.calc_upon_open, true);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "ConcatenateImages")
            {
                string filename = String.IsNullOrWhiteSpace(argument) ? null : argument.Trim();
                image_display.BeginInvokeIfRequired(o =>
                {
                    o.concatenateFilesInDirectory(filename, true);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "ExportCurrentIntensityImageInTIFF")
            {
                bool[] saveFormat = new bool[] { true, false };
                bool[] saveChannel = new bool[image_display.FLIM_ImgData.nChannels];
                for (int i = 0; i < saveChannel.Length; i++)
                {
                    if (argument == "")
                        saveChannel[i] = true;
                    else
                        saveChannel[i] = argument == (i + 1).ToString();
                }

                image_display.InvokeIfRequired(o =>
                {
                    o.SaveCurrentIntensityImage(saveFormat, saveChannel, null, false, true, FileFormat.ImageFormat.TIFF);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "SaveCurrentImage")
            {
                string filename = null;
                if (argument != "")
                    filename = argument;
                image_display.InvokeIfRequired(o =>
                {
                    o.SaveFLIM(true, filename, false);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "BinFrames")
            {
                String[] args = (argument ?? "").Split(new char[] { ',' }, 2);
                int argnumber = 1;
                if (args.Length == 0 || !Int32.TryParse(args[0].Trim(), out argnumber)) return false; //Kengo 05-23-2025 argnumber = defaultVal;

                string output_filename = null;
                if (args.Length > 1 && !String.IsNullOrWhiteSpace(args[1]))
                    output_filename = args[1].Trim();

                image_display.InvokeIfRequired(o =>
                {
                    o.BinFrames(argnumber, output_filename);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "AlignFrames")
            {
                image_display.InvokeIfRequired(o =>
                {
                    o.AlignFrames();
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            // Kengo BEGIN 06-02-2025
            // translate images by the shift values  
            else if (command == "TranslateFrames")
            {
                String[] sP = argument.Split(',');
                if (sP.Length % 2 != 0)
                    return false;

                double[] argnumbers = new double[sP.Length];
                for (int i = 0; i < sP.Length; i++)
                    if (!double.TryParse(sP[i], out argnumbers[i]))
                        return false;
                image_display.InvokeIfRequired(o =>
                {
                    o.TranslateFrames(argnumbers);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            // Kengo END
            // Kengo BEGIN 06-01-2025
            // Add
            else if (command == "DeleteCurrentPage")
            {
                image_display.InvokeIfRequired(o =>
                {
                    o.deleteCurrentPage();
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "BlankCurrentPage")
            {
                image_display.InvokeIfRequired(o =>
                {
                    o.blankCurrentPage();
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "ExtractPages")
            {
                String[] sP = argument.Split('-');
                if (sP.Length != 1 && sP.Length != 2)
                    return false;

                int[] argnumbers = new int[] { 1, 1 };
                for (int i = 0; i < sP.Length; i++)
                    if (!int.TryParse(sP[i], out argnumbers[i])) return false;
                if (sP.Length == 1)
                    argnumbers[1] = argnumbers[0];
                if (argnumbers[0] < 1 || image_display.FLIM_ImgData.n_pages < argnumbers[1]) return false;

                image_display.InvokeIfRequired(o =>
                {
                    o.extractPages(argnumbers);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            // Kengo END
            // Kengo BEGIN 12-30-2025 add
            else if (command == "SetPages")
            {
                if (image_display.FLIM_ImgData.n_pages <= 1) return false;

                String[] sP = argument.Split(',');
                if (sP.Length != 1 && sP.Length != 2) return false;

                int[] argnumbers = new int[] { 1, 1 };
                for (int i = 0; i < sP.Length; i++)
                    if (!int.TryParse(sP[i], out argnumbers[i])) return false;
                if (sP.Length == 1)
                    argnumbers[1] = argnumbers[0];
                if (argnumbers[0] < 1 || image_display.FLIM_ImgData.n_pages < argnumbers[1]) return false;

                image_display.InvokeIfRequired(o =>
                {
                    if (!o.displayZProjection)
                        o.FLIM_ImgData.gotoPage(argnumbers[0] - 1);
                    else
                    {
                        o.FLIM_ImgData.ZProjection_Range[0] = argnumbers[0] - 1;
                        o.FLIM_ImgData.ZProjection_Range[1] = argnumbers[1];
                        o.AssurePageRange();
                        o.calcZProjection();
                    }
                    o.UpdateImages(true, o.realtime, o.focusing, true);
                    if ((o.plot_regular.calc_upon_open || o.AutoApplyOffset.Checked) && !o.ZStack && !o.FastZStack)
                        o.CalculateCurrentPage(false);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            // KENGO END
            // Kengo BEGIN 1-2-2026 add
            else if (command == "SetFileCounter")
            {
                int argnumber = 0;
                if (!Int32.TryParse(argument, out argnumber))
                    return false;

                int saveCounter = image_display.FLIM_ImgData.fileCounter;
                if (argnumber > 0)
                {
                    image_display.InvokeIfRequired(o =>
                    {
                        o.FLIM_ImgData.fileCounter = argnumber;
                        String fn = o.FLIM_ImgData.fullName(o.currentChannel, o.FLIM_ImgData.State.Files.channelsInSeparatedFile);

                        if (File.Exists(fn))
                            o.OpenFLIM(fn, true, o.plot_regular.calc_upon_open, false);
                        else
                        {
                            o.FLIM_ImgData.fileCounter = saveCounter;
                            argnumber = 0;
                        }
                    });
                }
                if (argnumber <= 0)
                    return false;
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            // Kengo END
            else if (command == "FitData")
            {
                image_display.InvokeIfRequired(o =>
                {
                    o.FitData(false, true, -1);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "FitEachFrame")
            {
                int defaultVal = 1;
                int argnumber = 1;
                //Kengo BEGIN 05-23-2025
                //if (!Int32.TryParse(argument, out argnumber)) return false; //Kengo 05-23-2025 argnumber = defaultVal;
                if (argument == "")
                    argnumber = defaultVal;
                else if (!Int32.TryParse(argument, out argnumber))
                    return false;
                //Kengo END

                image_display.InvokeIfRequired(o =>
                {
                    o.plot_regular.TurnOnCalcFit(argnumber != 0);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "ReadImageJROI")
            {
                image_display.InvokeIfRequired(o =>
                {
                    o.ReadImageJROI(argument);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            //Kengo BEGIN 05-19-2025
            //Add remote commands (SaveImageJROI, SaveROIs, RecoverROIs, RemoveAllROIs, BatchProcessing)
            else if (command == "SaveImageJROI")
            {
                image_display.InvokeIfRequired(o =>
                {
                    o.SaveImageJROI();
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "SaveROIs")
            {
                image_display.InvokeIfRequired(o =>
                {
                    o.SaveAllRois();
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "RecoverROIs")
            {
                image_display.InvokeIfRequired(o =>
                {
                    o.RecoverROIs();
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "RemoveAllROIs")
            {
                image_display.InvokeIfRequired(o =>
                {
                    o.RemoveROIs();
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "ShiftAllROIs")
            {
                String[] sP = argument.Split(',');
                if (sP.Length % 2 != 0 || sP.Length == 0)
                    return false;

                float[] argnumbers = new float[sP.Length];
                for (int i = 0; i < sP.Length; i++)
                    if (!float.TryParse(sP[i], out argnumbers[i]))
                        return false;

                image_display.InvokeIfRequired(o =>
                {
                    o.ShiftAllROIs(argnumbers);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "BatchProcessing")
            {
                image_display.InvokeIfRequired(o =>
                {
                    Task.Factory.StartNew((Action)delegate
                    {
                        o.BatchProcessing();
                    });
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "SetAnalyzeAfterAcq")
            {
                int argnumber = 1;
                if (!Int32.TryParse(argument, out argnumber)) return false;
                this.Invoke((Action)delegate
                {
                    analyzeEach.Checked = (argnumber != 0);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            //Kengo END 05-19-2025
            else if (command == "CalcTimeCourse")
            {
                image_display.InvokeIfRequired(o =>
                {
                    if (o.TC != null)
                        o.TC.ImInfos.Clear();
                    o.plot_regular.Show();  // Kengo ADD 05-15-2025
                    o.CalculateTimecourse(true);
                    o.plot_regular.updatePlot();
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "CalcCurrentPage")
            {
                image_display.InvokeIfRequired(o =>
                {
                    o.CalculateCurrentPage(true);
                    o.plot_regular.updatePlot();
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            //KENGO BEGIN 1-4-2025
            else if (command == "ResetTimeCourse")
            {
                image_display.InvokeIfRequired(o =>
                {
                    if (!o.plot_regular.real_time)
                    {
                        if (o.TCF != null)
                        {
                            o.TC = new TimeCourse();
                            var iminfo = new ImageInfo(image_display.FLIM_ImgData);
                            int page = image_display.FLIM_ImgData.currentPage;
                            if (page >= 0)
                            {
                                o.TC.AddFile(iminfo, page);
                                o.TC.calculate();
                                o.TCF = new TimeCourse_Files();
                                o.TCF.AddFile(o.TC);
                                o.TCF.calculate();
                                image_display.TCF = o.TCF;
                                image_display.TC = o.TC;
                                image_display.SaveTimeCourse();
                            }
                        }
                        o.plot_regular.plotNow_noRealtime(o.TCF, o.TC, image_display, image_display.currentChannel);
                    }
                    else
                    {
                        o.realtimeData.Clear();
                    }

                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            //KENGO END
            else if (command == "SetFLIMIntensityOffset")
            {
                double[] defaultVals = new double[] { 100, 1000 };
                String[] sP = argument.Split(',');
                if (sP.Length != 2)
                    return false;

                double[] argnumbers = new double[sP.Length];
                for (int i = 0; i < sP.Length; i++)
                    if (!double.TryParse(sP[i], out argnumbers[i])) return false; //Kengo 05-23-2025 argnumbers[i] = defaultVals[i];

                image_display.InvokeIfRequired(o =>
                {
                    o.SetFLIMIntensityOffset(argnumbers);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "SetMinFLIMIntensity")
            {
                double[] defaultVals = new double[] { 20, 10 };
                String[] sP = argument.Split(',');
                if (sP.Length != 2)
                    return false;

                double[] argnumbers = new double[sP.Length];
                for (int i = 0; i < sP.Length; i++)
                    if (!double.TryParse(sP[i], out argnumbers[i])) return false;

                image_display.InvokeIfRequired(o =>
                {
                    o.SetMinFLIMIntensity(argnumbers);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "SetMaxFLIMIntensity")
            {
                double[] defaultVals = new double[] { 200, 100 };
                String[] sP = argument.Split(',');
                if (sP.Length != 2)
                    return false;

                double[] argnumbers = new double[sP.Length];
                for (int i = 0; i < sP.Length; i++)
                    if (!double.TryParse(sP[i], out argnumbers[i])) return false;

                image_display.InvokeIfRequired(o =>
                {
                    o.SetMaxFLIMIntensity(argnumbers);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "ApplyFitOffset")
            {
                image_display.InvokeIfRequired(o =>
                {
                    o.ApplyOffset(true, true);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));

            }
            else if (command == "FixTauAll")
            {
                int defaultVal = 1;
                int argnumber = 1;
                //Kengo BEGIN 05-23-2025
                //if (!Int32.TryParse(argument, out argnumber)) argnumber = defaultVal;
                if (argument == "")
                    argnumber = defaultVal;
                else if (!Int32.TryParse(argument, out argnumber))
                    return false;
                //Kengo END

                image_display.InvokeIfRequired(o =>
                {
                    o.FixTauAll(argnumber != 0);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "FixTau")
            {
                double[] defaultVals = new double[] { 2.6, 1.1 };
                String[] sP = argument.Split(',');
                if (sP.Length != 2)
                    return false;

                double[] argnumbers = new double[sP.Length];
                for (int i = 0; i < sP.Length; i++)
                    if (!double.TryParse(sP[i], out argnumbers[i])) return false; //Kengo 05-23-2025 argnumbers[i] = defaultVals[i];

                image_display.InvokeIfRequired(o =>
                {
                    o.FixTau(argnumbers);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "SetFitParams")
            {
                double[] defaultVals = new double[] { 2.6, 1.1, 0.12, 1.0 };
                String[] sP = argument.Split(',');
                if (sP.Length != 4)
                    return false;

                double[] argnumbers = new double[sP.Length];
                for (int i = 0; i < sP.Length; i++)
                    if (!double.TryParse(sP[i], out argnumbers[i])) return false;

                image_display.InvokeIfRequired(o =>
                {
                    o.SetFitParams(argnumbers);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "SetFitRange")
            {
                int[] defaultVals = new int[] { 10, 50 };
                String[] sP = argument.Split(',');
                if (sP.Length != 2)
                    return false;

                int[] argnumbers = new int[sP.Length];
                for (int i = 0; i < sP.Length; i++)
                    if (!int.TryParse(sP[i], out argnumbers[i])) return false; //Kengo 05-23-2025 argnumbers[i] = defaultVals[i];

                image_display.InvokeIfRequired(o =>
                {
                    o.SetFitRange(argnumbers);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "SetChannel")
            {
                int defaultVal = 1;
                int argnumber = 1;
                //Kengo BEGIN 05-23-2025
                //if (!Int32.TryParse(argument, out argnumber)) argnumber = defaultVal;
                if (argument == "")
                    argnumber = defaultVal;
                else if (!Int32.TryParse(argument, out argnumber))
                    return false;
                //Kengo END

                image_display.InvokeIfRequired(o =>
                {
                    o.SetChannel(argnumber);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "HoldThisImage")
            {
                int argnumber = 1;
                if (!Int32.TryParse(argument, out argnumber)) return false;

                image_display.InvokeIfRequired(o =>
                {
                    o.HoldThisImage(argnumber != 0);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            //append a remote command to control DIO_panel
            else if (command == "SetDIOPanel")
            {
                String[] sP = argument.Split(',');
                if (sP.Length % 2 != 0) return false;

                int[] argnumbers = new int[sP.Length];
                for (int i = 0; i < sP.Length; i++)
                    if (!int.TryParse(sP[i], out argnumbers[i])) return false;

                this.InvokeIfRequired(o =>
                {
                    if (DIO_panel == null)
                        DIO_panel = new DigitalSignalPanel(this);
                    DIO_panel.Show();

                    for (int i = 0; i < argnumbers.Length; i += 2)
                        DIO_panel.SetDIOPanel(argnumbers[i], argnumbers[i + 1] != 0);
                });
            }
            else if (command == "GetParametersOfImageFile")
            {
                State = image_display.FLIM_ImgData.State;
            }
            else
            {
                success = false;
            }

            Thread.Sleep(1); //To stabilize???? Not sure...
            return success;
        }

        ////////////////////////////////
        ///Plotting///
        ////////////////////////////////
        private void plotScanXYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[,] DataXY = HardwareControls.IOControls.MakeMirrorOutputXY(State);
            Plot plot2 = new Plot(DataXY, "-Y");
            plot2.Show();
        }

        private void plotScanXYZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[,] DataXY = HardwareControls.IOControls.MakeMirrorOutputXY(State);
            var DataAll = HardwareControls.IOControls.ReplicateByFrameNumber(DataXY, State.Acq.nSlices * 2);
            int nSamples = DataXY.GetLength(1);
            var DataZ = HardwareControls.IOControls.calculateZMotion(State, nSamples, State.Acq.outputRate);
            DataAll = HardwareControls.IOControls.ConcatChannels(DataAll, DataZ);
            Plot plot2 = new Plot(DataAll, "Time (ms)", "Voltage (V)", State.Acq.outputRate, "Ch");
            plot2.Show();
        }

        private void plotLineScanTraceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (flimage_io == null)
                return;

            if (!flimage_io.TryGetLineScanTraceWaveform(1, out var traceXY, out string reason))
            {
                if (!string.IsNullOrWhiteSpace(reason))
                    MessageBox.Show(Form.ActiveForm, reason);
                return;
            }

            if (traceXY == null || traceXY.GetLength(0) < 2 || traceXY.GetLength(1) < 2)
            {
                MessageBox.Show(Form.ActiveForm, "Line scan trace waveform is empty.");
                return;
            }

            var plotXY = new Plot(traceXY, "-Y");
            plotXY.Text = "LineScan Trace (XY)";
            plotXY.Show();

            var plotT = new Plot(traceXY, "Time (ms)", "Voltage (V)", State.Acq.outputRate, "LineScan");
            plotT.Text = "LineScan Trace (X/Y vs Time)";
            plotT.Show();
        }

        void PlotScanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[,] DataXY = HardwareControls.IOControls.MakeMirrorOutputXY(State);
            Plot plot2 = new Plot(DataXY, "Time (ms)", "Voltage (V)", State.Acq.outputRate, "Laser");
            plot2.Show();
        }


        void PlotPockelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[,] DataXY = HardwareControls.IOControls.MakeMirrorOutputXY(State);
            double[,] DataEOM = HardwareControls.IOControls.MakeEOMOutput(State, flimage_io.shading, false, false);
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
            ScanPosition_PaintProcol(State, e, ScanPosition, ref ROIs, ref Roi);
        }


        public void ScanPosition_PaintProcol(ScanParameters State, PaintEventArgs e, PictureBox scan_p,
            ref ROI[] rois, ref ROI roi)
        {
            //var State = flimage.State;

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(snapShotBMP,
                new Rectangle(0, 0, scan_p.Width, scan_p.Height), // destination rectangle 
                0, 0,           // upper-left corner of source rectangle
                snapShotBMP.Width,       // width of source rectangle
                snapShotBMP.Height,      // height of source rectangle
                GraphicsUnit.Pixel);

            float width = scan_p.Width;
            float height = scan_p.Height;
            double zoom = State.Acq.zoom;

            var roi_size_x = State.Acq.XMaxVoltage / State.Acq.zoom * State.Acq.scanVoltageMultiplier[0];
            var roi_size_y = State.Acq.YMaxVoltage / State.Acq.zoom * State.Acq.scanVoltageMultiplier[1];

            if (State.Acq.nSplitScanning > 1)
            {
                float rect_width = (float)(width / zoom * State.Acq.scanVoltageMultiplier[0]);
                float rect_height = (float)(height / zoom * State.Acq.scanVoltageMultiplier[1]) / State.Acq.nSplitScanning;

                rois = new ROI[State.Acq.nSplitScanning];
                roi = null;

                double nSplit = State.Acq.nSplitScanning;
                var roi_size_y_s = roi_size_y / nSplit;

                for (int loc = 0; loc < nSplit; loc++)
                {
                    var roiRect_voltage_XY = new double[,] { { -roi_size_x / 2.0, -roi_size_x / 2.0, +roi_size_x / 2.0, +roi_size_x / 2.0 },
                                                { roi_size_y_s, 0, 0, roi_size_y_s} };

                    for (int i = 0; i < roiRect_voltage_XY.GetLength(1); i++)
                        roiRect_voltage_XY[1, i] = roiRect_voltage_XY[1, i] + roi_size_y_s * loc - roi_size_y / 2.0;

                    FLIMage.HardwareControls.IOControls.RotateAndOffset(roiRect_voltage_XY, State, loc);

                    var x1 = new float[roiRect_voltage_XY.GetLength(1)];
                    var y1 = new float[roiRect_voltage_XY.GetLength(1)];
                    var polygon = new PointF[roiRect_voltage_XY.GetLength(1)];

                    for (int i = 0; i < x1.Length; i++)
                    {
                        x1[i] = (float)(width * roiRect_voltage_XY[0, i] / State.Acq.XMaxVoltage) + width / 2.0f;
                        y1[i] = (float)(height * roiRect_voltage_XY[1, i] / State.Acq.YMaxVoltage) + height / 2.0f;
                        polygon[i] = new PointF(x1[i], y1[i]);
                    }

                    rois[loc] = new ROI(ROI.ROItype.Polygon, x1, y1, 1, 1, 0, false, null);

                    Pen rectPen = new Pen(Color.White, (float)0.5);
                    if (loc == current_splitScanLocation)
                        rectPen = new Pen(Color.White, (float)2);
                    e.Graphics.DrawPolygon(rectPen, polygon);

                    var drawRect = new RectangleF(x1[0] - 10, y1[0] - 10, 100, 20);
                    SolidBrush roiBrush = new SolidBrush(Color.Cyan);
                    Font roiFont = new Font("Arial", 8, FontStyle.Regular);
                    e.Graphics.DrawString((loc + 1).ToString(), roiFont, roiBrush, drawRect);
                }
            }
            else
            {
                var roiRect_voltage_XY = new double[,] { { -roi_size_x / 2.0, -roi_size_x / 2.0, +roi_size_x / 2.0, +roi_size_x / 2.0 },
                                                { -roi_size_y / 2.0, +roi_size_y / 2.0, +roi_size_y / 2.0, -roi_size_y / 2.0 } };

                FLIMage.HardwareControls.IOControls.RotateAndOffset(roiRect_voltage_XY, State, 0);
                var x1 = new float[roiRect_voltage_XY.GetLength(1)];
                var y1 = new float[roiRect_voltage_XY.GetLength(1)];
                var polygon = new PointF[roiRect_voltage_XY.GetLength(1)];

                for (int i = 0; i < x1.Length; i++)
                {
                    x1[i] = (float)(width * roiRect_voltage_XY[0, i] / State.Acq.XMaxVoltage) + width / 2.0f;
                    y1[i] = (float)(height * roiRect_voltage_XY[1, i] / State.Acq.YMaxVoltage) + height / 2.0f;
                    polygon[i] = new PointF(x1[i], y1[i]);
                }

                roi = new ROI(ROI.ROItype.Polygon, x1, y1, 1, 1, 0, false, null);
                rois = null;

                Pen rectPen = new Pen(Color.White, (float)0.5);
                e.Graphics.DrawPolygon(rectPen, polygon);
            }
        }



        //////////MIROR CONTRL
        //void MirrorYUp_Click(object sender, EventArgs e)
        //{
        //    double XYStep;
        //    if (!Double.TryParse(MirrorStep.Text, out XYStep)) XYStep = 0.1;
        //    double XStep = State.Acq.XMaxVoltage / State.Acq.zoom * XYStep;
        //    double YStep = State.Acq.YMaxVoltage / State.Acq.zoom * XYStep;

        //    if (State.Acq.resonantScanning)
        //    {
        //        XStep = 0;
        //        YStep = State.Acq.YMaxVoltage_Resonant / State.Acq.zoom * XYStep;
        //    }

        //    if (sender.Equals(MirrorYUp))
        //        State.Acq.YOffset = State.Acq.YOffset - YStep;
        //    else if (sender.Equals(MirrorYDown))
        //        State.Acq.YOffset = State.Acq.YOffset + YStep;
        //    else if (sender.Equals(MirrorXUp))
        //        State.Acq.XOffset = State.Acq.XOffset + XStep;
        //    else if (sender.Equals(MirrorXDown))
        //        State.Acq.XOffset = State.Acq.XOffset - XStep;

        //    XOffset.Text = string.Format("{0:0.00}", State.Acq.XOffset);
        //    YOffset.Text = string.Format("{0:0.00}", State.Acq.YOffset);

        //    UpdateScanPositionWindow()
        //    SetParametersFromState(true);

        //    if (State.Init.UseExternalMirrorOffset)
        //    {
        //        var aoX = new HardwareControls.IOControls.AO_Write(State.Init.mirrorOffsetX, State.Acq.XOffset);
        //        var aoY = new HardwareControls.IOControls.AO_Write(State.Init.mirrorOffsetY, State.Acq.YOffset);
        //    }

        //    flimage_io.ResetFocus();

        //    if (!flimage_io.grabbing && !flimage_io.focusing)
        //    {
        //        flimage_io.ParkMirrors(false);
        //    }

        //}



        /////////MOTOR CONTROL/////////////////////////////////////////////////

        public void MotorPositioningUpdate()
        {
            motor_back_to_center = BackToCenterRadio.Checked;
            motor_back_to_start = BackToStartRadio.Checked;
            motor_stay = StayMotorRadio.Checked;
        }

        void ContRead_CheckedChanged(object sender, EventArgs e)
        {
            if (use_motor)
            {
                motorCtrl.continuousRead(ContRead.Checked);
                motorCtrl.GetPosition();
            }

            if (use_piezo)
                UpdatePiezoPositionGUI();
        }

        void Motor_TextSet()
        {
            double ZStep;
            if (!Double.TryParse(ZMotorStep.Text, out ZStep)) ZStep = motorCtrl.ZStep;
            double XYStep;
            if (!Double.TryParse(XYMotorStep.Text, out XYStep)) ZStep = motorCtrl.XYStep;

            double VValue;
            if (!Double.TryParse(Velocity.Text, out VValue)) VValue = motorCtrl.velocity[0];

            if (motorCtrl != null)
            {
                motorCtrl.ZStep = ZStep;
                motorCtrl.XYStep = XYStep;
            }

            State.Motor.stepZ = ZStep;
            State.Motor.stepXY = XYStep;

            this.BeginInvokeIfRequired(o => o.FillGUIMotor());
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
                { }
                ;
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

        public void UpdatePiezoPositionGUI()
        {
            if (use_piezo)
            {
                if (flimage_io.piezo != null)
                {
                    bool success = false;
                    var piezo_z = 0.0;
                    var z_start = 0.0;
                    var z_end = 0.0;
                    try
                    {
                        State.Acq.currentPositionPiezo_V = flimage_io.piezo.getPosition_V();
                        flimage_io.piezo.CorrectOutOfRange();
                        piezo_z = flimage_io.piezo.getPosition_um();
                        z_start = flimage_io.piezo.stack_start_um;
                        z_end = flimage_io.piezo.stack_end_um;
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error in getting Piezo position!!");
                    }

                    if (success)
                    {

                        this.InvokeIfRequired(o =>
                        {
                            o.PiezoZ.Text = piezo_z.ToString("F2");
                            o.ZStart.Text = z_start.ToString("F2");
                            o.ZEnd.Text = z_end.ToString("F2");
                            double center1 = (flimage_io.piezo.stack_start_um + flimage_io.piezo.stack_end_um) / 2;
                            o.ZCenter.Text = center1.ToString("F2");
                            State.Acq.scanZWithPiezoRange_um = new double[] { flimage_io.piezo.stack_start_um, flimage_io.piezo.stack_end_um };
                        });
                    }
                }
                else
                    this.InvokeIfRequired(o => { o.PiezoZ.Text = "NA"; });

            }
        }

        void MotorHandler(MotrEventArgs e)
        {
            if (motorCtrl == null)
                return;

            double minVal = motorCtrl.minMotorVal; ;

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
            State.Motor.velocity[0] = motorCtrl.velocity[0];

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

            // KENGO BIGEIN 2025-11-05
            // add these three lines so that stack position is indicated by underlining the corresponding label
            SetStartLabelStyle(motorCtrl.stack_Position == MotorCtrl.StackPosition.Start);
            SetCenterLabelStyle(motorCtrl.stack_Position == MotorCtrl.StackPosition.Center);
            SetStopLabelStyle(motorCtrl.stack_Position == MotorCtrl.StackPosition.End);
            // KENGO END

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
            else if (e.Name == "FreezeA")
            {
                XRead.Text = "---";
                YRead.Text = "---";
                ZRead.Text = "---";
                Motor_Status.Text = "Recovering from crash...";
            }

            if (e.Name == "MovementDone")
                flimage_io.Notify(new ProcessEventArgs("StageMoveDone", null));
        }

        // KENGO BEGIN 2025-11-05
        // Add for toggle Underline on any Label
        private void SetLabelStyle(System.Windows.Forms.Label lbl, bool underline)
        {
            if (lbl == null) return;

            if (lbl.InvokeRequired)
            {
                lbl.Invoke((Action)(() => SetLabelStyle(lbl, underline)));
                return;
            }

            var f = lbl.Font;
            var newStyle = underline ? (f.Style | System.Drawing.FontStyle.Underline) : (f.Style & ~System.Drawing.FontStyle.Underline);
            lbl.Font = new System.Drawing.Font(f.FontFamily, f.Size, newStyle, f.Unit);
        }

        // Wrappers for StartLabel, CenterLabel, StopLabel
        private void SetStartLabelStyle(bool underline) => SetLabelStyle(this.StartLabel, underline);
        private void SetCenterLabelStyle(bool underline) => SetLabelStyle(this.CenterLabel, underline);
        private void SetStopLabelStyle(bool underline) => SetLabelStyle(this.StopLabel, underline);
        // KENGO END

        void MotorListener(MotorCtrl m, MotrEventArgs e)
        {
            try
            {
                //Blocking action.
                this.Invoke((Action)delegate
                   {
                       MotorHandler(e);
                   });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        void WaitForMotorMove()
        {
            if (use_motor)
            {
                motorCtrl.WaitUntilMovementDone();
            }
        }

        public void MoveMotorStep(bool waitUntilFinish)
        {
            if (use_piezo)
            {
                double um = flimage_io.piezo.move_Piezo_1step_um(State.Acq.sliceStep);
                flimage_io.piezo.current_position = HardwareControls.IOControls.CurrentPosition.InStack;
                UpdatePiezoPositionGUI();
            }
            else if (use_motor)
            {
                if (State.Acq.sliceStep != 0)
                {
                    // 20250630 Tetsuya from here  
                    // During Z stack acquisition, do not start acquisition before reaching to the correct Z position.
                    // Synchronize Z stack movements to prevent race conditions
                    if (State.Acq.ZStack && State.Acq.nSlices > 1)
                    {
                        lock (zStackSyncLock)
                        {
                            // Wait if another Z stack movement is in progress
                            while (zStackMovementInProgress)
                            {
                                System.Threading.Thread.Sleep(10);
                            }

                            zStackMovementInProgress = true;

                            try
                            {
                                // Ensure previous movement is completely finished before starting new movement
                                motorCtrl.WaitUntilMovementDone();

                                // Use relative positioning with proper synchronization
                                motorCtrl.SetNewPosition_StepSize_um(new double[] { 0, 0, State.Acq.sliceStep });
                                motorCtrl.stack_Position = MotorCtrl.StackPosition.InStack;

                                // Use MoveMotor instead of MoveMotor_Certified to avoid race conditions
                                // MoveMotor_Certified can cause position jumps due to repeated movement attempts
                                SetMotorPosition(true, waitUntilFinish, false);
                            }
                            finally
                            {
                                zStackMovementInProgress = false;
                            }
                        }
                    }
                    else
                    {
                        // For non-Z stack movements, use original method
                        motorCtrl.SetNewPosition_StepSize_um(new double[] { 0, 0, State.Acq.sliceStep });
                        motorCtrl.stack_Position = MotorCtrl.StackPosition.InStack;
                        SetMotorPosition(true, waitUntilFinish, true);
                    }
                    // Till here  20250630 Tetsuya
                }
            }
        }

        public void MoveBackToHome()
        {
            if (State.Acq.nSlices > 1 && State.Acq.ZStack)
            {
                if (motor_back_to_start)
                    MoveMotorBackToStart();
                else if (motor_back_to_center)
                    MoveMotorBackToCenter();
            }
        }

        void MoveMotorBackToStart() //Called after acquisition.
        {
            if (State.Acq.nSlices > 1 && State.Acq.ZStack)
            {
                if (use_piezo && usePiezoCheckBox.Checked)
                {
                    flimage_io.piezo.move_Piezo_um(flimage_io.piezo.stack_start_um);
                    flimage_io.piezo.current_position = HardwareControls.IOControls.CurrentPosition.Start;
                    UpdatePiezoPositionGUI();
                }
                else if (use_motor)
                    motorCtrl.MoveMotorBackToStart();

            }
        }

        void MoveMotorBackToCenter()  //Called after acquisition.
        {
            if (State.Acq.nSlices > 1 && State.Acq.ZStack)
            {
                if (use_piezo && usePiezoCheckBox.Checked)
                {
                    var center1 = (flimage_io.piezo.stack_start_um + flimage_io.piezo.stack_end_um) / 2;
                    flimage_io.piezo.move_Piezo_um(center1);
                    flimage_io.piezo.current_position = HardwareControls.IOControls.CurrentPosition.Center;
                    UpdatePiezoPositionGUI();
                }
                else if (use_motor)
                    motorCtrl.MoveMotorBackToCenter_RelativeToZStart();


            }
        }


        void MoveMotorFromCenterToStart()
        {
            if (State.Acq.nSlices > 1 && State.Acq.ZStack)
            {
                if (use_piezo && usePiezoCheckBox.Checked)
                {
                    flimage_io.piezo.move_Piezo_um(flimage_io.piezo.stack_start_um);
                    flimage_io.piezo.current_position = HardwareControls.IOControls.CurrentPosition.Start;
                    UpdatePiezoPositionGUI();
                }
                else if (use_motor)
                    motorCtrl.MoveMotorFromCenterToStart();

            }
        }


        void AutoCalculateStackStartEnd()
        {
            if (use_motor)
                motorCtrl.AutoCalculateStackStartEnd();
        }

        public void MoveMotorPosition(double[] position_um, bool warningOn)
        {
            if (use_motor)
            {
                motorCtrl.SetNewPosition_um(position_um);
                SetMotorPosition(warningOn, true, true);
            }
        }


        /// <summary>
        /// Set motor position to new position.
        /// </summary>
        public void SetMotorPosition(bool warningOn, bool waitUntilFinish, bool confirm)
        {
            var task1 = Task.Factory.StartNew((Action)delegate
            {
                if (use_motor)
                {
                    flimage_io.Notify(new ProcessEventArgs("StageMoveStart", null));
                    if (confirm)
                        motorCtrl.MoveMotor_Certified(warningOn, 5);
                    else
                        motorCtrl.MoveMotor(warningOn, waitUntilFinish);
                }
            });

            //motorCtrl.WaitUntilMovementDone();
            //System.Threading.Thread.Sleep(500);
            task1.Wait(500);
            //motorCtrl.Stop();

        }

        public void MotroStepMovementXYZ(object sender, EventArgs e)
        {
            bool use_piezo1 = use_piezo && usePiezoCheckBox.Checked && flimage_io.piezo != null;
            if (use_piezo1) //Very fast. We don't que.
            {
                turn_motorButtons(false);
                double stepSize = Convert.ToDouble(ZMotorStep.Text);
                double stepSize1 = stepSize;
                if (sender.Equals(Zup))
                {
                    stepSize1 = stepSize;
                }
                else if (sender.Equals(Zdown))
                {
                    stepSize1 = -stepSize;
                }
                flimage_io.PiezoMoveDuringFocus(stepSize1);
                UpdatePiezoPositionGUI();
                turn_motorButtons(true);
            }

            if (use_motor)
            {
                Motor_TextSet();

                if (motorQ.Any(item => item != 0))
                {
                    System.Threading.Thread.Sleep(1);
                }

                lock (motorQlock) //If it is clicked many times at once, it will juist add the distance in motorQ.
                {
                    if (!use_piezo1)
                    {
                        if (sender.Equals(Zup))
                            motorQ[2] += motorCtrl.ZStep;
                        else if (sender.Equals(Zdown))
                            motorQ[2] -= motorCtrl.ZStep;
                    }

                    if (sender.Equals(YUp))
                        motorQ[1] += motorCtrl.XYStep;
                    else if (sender.Equals(YDown))
                        motorQ[1] -= motorCtrl.XYStep;
                    else if (sender.Equals(XUp))
                        motorQ[0] += motorCtrl.XYStep;
                    else if (sender.Equals(XDown))
                        motorQ[0] -= motorCtrl.XYStep;

                    Debug.WriteLine("Set MotorZ position {0}", motorQ[2]);
                }

                motorCtrl.GetPosition();

                if (!motor_moving) //If motor is still moving, motorQ will not be excecuted. 
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

                            // confirm=false: step moves use a single MoveMotor (no retry loop).
                            // Cumulative drift is prevented upstream by SetNewPosition_StepSize_um
                            // always stepping from the previously commanded target (prevNewXYZ),
                            // so retries are no longer needed to correct drift.
                            SetMotorPosition(true, true, false);
                        }

                        motor_moving = false;
                    });
                }
            }
        }

        private void MotorReadButton_Click(object sender, EventArgs e)
        {
            if (use_piezo)
                UpdatePiezoPositionGUI();

            if (use_motor)
                motorCtrl.GetPosition();
        }

        void Zero_Z_Click(object sender, EventArgs e)
        {
            if (use_motor)
                motorCtrl.Zero_Z();

        }

        void Zero_all_Click(object sender, EventArgs e)
        {
            motorCtrl.Zero_All();
        }

        void CalcStackSize()
        {
            double stepSize;
            if (!Double.TryParse(SliceStep.Text, out stepSize)) stepSize = 1.0;

            if (use_motor)
            {
                motorCtrl.CalcNSlices(stepSize);
                State.Acq.sliceStep = motorCtrl.ZStack_Stepsize;
                State.Acq.nSlices = motorCtrl.ZStack_nSlices;
                State.Acq.ZStack = true;
            }

            if (use_piezo)
            {
                var piezo = flimage_io.piezo;
                int nSlices = (int)((piezo.stack_end_um - piezo.stack_start_um) / State.Acq.sliceStep + 1);
                piezo.stack_end_um = piezo.stack_start_um + nSlices * State.Acq.sliceStep;
                State.Acq.nSlices = nSlices;
                State.Acq.ZStack = true;
                UpdatePiezoPositionGUI();
            }

            UpdateStackSizeGUI();
        }

        void UpdateStackSizeGUI()
        {
            this.BeginInvokeIfRequired(o =>
            {
                o.NSlices.Text = o.State.Acq.nSlices.ToString();
                o.N_AveragedSlices.Text = o.State.Acq.nSlices.ToString();
                o.NSlices2.Text = o.State.Acq.nSlices.ToString();
                o.SliceStep.Text = o.State.Acq.sliceStep.ToString();
                if (!o.ZStack_radio.Checked && o.State.Acq.ZStack)
                    o.Generic_ValueChanged(o.ZStack_radio, null);
            });
        }

        //void UpdateStackSizeGUI_Core()
        //{
        //    NSlices.Text = State.Acq.nSlices.ToString();
        //    N_AveragedSlices.Text = State.Acq.nSlices.ToString();
        //    NSlices2.Text = State.Acq.nSlices.ToString();
        //    SliceStep.Text = State.Acq.sliceStep.ToString();
        //    if (!ZStack_radio.Checked && State.Acq.ZStack)
        //        Generic_ValueChanged(ZStack_radio, null);
        //}

        void GoStart_Click(object sender, EventArgs e)
        {
            if (use_piezo)
            {
                flimage_io.piezo.move_Piezo_um(flimage_io.piezo.stack_start_um);
                UpdatePiezoPositionGUI();
            }
            else if (use_motor && motorCtrl != null)
                motorCtrl.GotoStartPosition(true, true);
        }

        void GoEnd_Click(object sender, EventArgs e)
        {
            if (use_piezo)
            {
                flimage_io.piezo.move_Piezo_um(flimage_io.piezo.stack_end_um);
                UpdatePiezoPositionGUI();
            }
            else if (use_motor && motorCtrl != null)
                motorCtrl.GotoEndPosition(true, true);


        }

        private void GoCenterButton_Click(object sender, EventArgs e)
        {
            MoveMotorBackToCenter();
        }

        // Kengo BEGIN 2025-11-06
        // Add MouseDown event to know the mouse buttons
        public void Set_Top_Click(object sender, MouseEventArgs e)
        {
            SetTop(e);
        }

        public void SetTop(MouseEventArgs e)
        {
            if (use_piezo)
            {
                flimage_io.piezo.stack_start_um = flimage_io.piezo.getPosition_um();
                flimage_io.piezo.stack_end_um = flimage_io.piezo.stack_start_um + State.Acq.nSlices * State.Acq.sliceStep;
                flimage_io.piezo.current_position = HardwareControls.IOControls.CurrentPosition.Start;
                UpdatePiezoPositionGUI();
            }
            else if (use_motor && motorCtrl != null)
            {
                motorCtrl.SetTopPosition(e.Button);
                State.Acq.sliceStep = motorCtrl.ZStack_Stepsize;
                State.Acq.nSlices = motorCtrl.ZStack_nSlices;
                UpdateStackSizeGUI();
                motorCtrl.GetPosition();
            }
        }
        // KENGO END

        public void SetTop()
        {
            if (use_piezo)
            {
                flimage_io.piezo.stack_start_um = flimage_io.piezo.getPosition_um();
                flimage_io.piezo.stack_end_um = flimage_io.piezo.stack_start_um + State.Acq.nSlices * State.Acq.sliceStep;
                flimage_io.piezo.current_position = HardwareControls.IOControls.CurrentPosition.Start;
                UpdatePiezoPositionGUI();
            }
            else if (use_motor && motorCtrl != null)
            {
                //motorCtrl.GetPosition();  // KENGO this line is no need
                // KENGO BEGIN 2025-11-06
                // add MouseButtons parameter to SetTopPosition and updating State.Acq parameters
                motorCtrl.SetTopPosition(MouseButtons.Left);
                State.Acq.sliceStep = motorCtrl.ZStack_Stepsize;
                State.Acq.nSlices = motorCtrl.ZStack_nSlices;
                // KENGO END
                UpdateStackSizeGUI();
                motorCtrl.GetPosition();
            }
        }

        public void Set_Center_Click(object sender, EventArgs e)
        {
            SetCenter();
        }

        public void SetCenter()
        {
            if (use_piezo)
            {
                double center1 = flimage_io.piezo.getPosition_um();
                double half_stroke = (State.Acq.nSlices - 1) * State.Acq.sliceStep / 2;
                flimage_io.piezo.stack_start_um = center1 - half_stroke;
                flimage_io.piezo.stack_end_um = center1 + half_stroke;
                flimage_io.piezo.current_position = HardwareControls.IOControls.CurrentPosition.Center;
                UpdatePiezoPositionGUI();
            }
            else if (motorCtrl != null)
            {
                motorCtrl.GetPosition();
                motorCtrl.SetCenterPosition();
                UpdateStackSizeGUI();
                motorCtrl.GetPosition();
            }
        }

        void Set_bottom_Click(object sender, MouseEventArgs e)
        {
            if (use_piezo)
            {
                flimage_io.piezo.stack_end_um = flimage_io.piezo.getPosition_um();
                flimage_io.piezo.current_position = HardwareControls.IOControls.CurrentPosition.End;
                UpdatePiezoPositionGUI();
            }
            else if (motorCtrl != null)
            {
                motorCtrl.SetBottomPosition(e.Button);
                // KENGO BEGIN 2025-11-05
                // Update State.Acq parameters
                State.Acq.sliceStep = motorCtrl.ZStack_Stepsize;
                State.Acq.nSlices = motorCtrl.ZStack_nSlices;
                // KENGO END
                UpdateStackSizeGUI();
                motorCtrl.GetPosition();
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

                    SetParametersFromState(true);

                    if (sender.Equals(NSlices2) || sender.Equals(SliceStep))
                        AutoCalculateStackStartEnd();
                    else
                        CalcStackSize();

                    if (use_motor)
                        MotorHandler(new MotrEventArgs(""));
                }
                catch (System.FormatException)
                {
                    tb.Text = SaveText;
                }
                finally
                { }
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
            turn_motorButtons(false);

            //motorCtrl.GetStatus();
            //Motor_TextSet();

            double vel_step = 100;
            if (sender.Equals(Vel_Down))
                vel_step = -100;

            double vel = motorCtrl.velocity[0];
            if (motorCtrl.velocity[0] + vel_step <= motorCtrl.maxVelocity[0])
                vel = vel + vel_step;

            motorCtrl.SetVelocity(new double[] { vel, vel, vel });
        }

        private void CenterPiezoButton_Click(object sender, EventArgs e)
        {
            flimage_io.movePiezoToCenter();
            PiezoZ.Text = flimage_io.piezo.getPosition_um().ToString();
        }

        void CalibrateToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            flimage_io.CalibEOM(true);
        }

        void showCalibrationCurveToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Plot plot2 = new Plot(flimage_io.shading.calibration.calibrationCurve, "Power (%)", "Applied voltage (V)", 1000, "Laser");
            plot2.Show();
        }

        void showInputoutputCurveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Plot plot2 = new Plot(flimage_io.shading.calibration.calibrationCurve, flimage_io.shading.calibration.calibrationOutput, "Applied voltage (V)", "Photodiode voltage (V)", 1000, "Laser");
            plot2.Show();
        }


        void FLIMageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ss.SetComputerID(State.Init.ComputerID);
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

        public void ImageDisplayOpen()
        {
            this.BeginInvokeIfRequired(o => o.ImageDisplaySetup());
        }

        private void ImageDisplaySetup()
        {
            if (image_display == null || image_display.IsDisposed)
                image_display = new Image_Display(image_display.FLIM_ImgData, this, false);
            EnsurePhasorPlotWindow();
            image_display.Show();
            image_display.Activate();
            phasor_plot_window?.AttachImageDisplay(image_display);
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
                DIO_panel = new DigitalSignalPanel(this);
            DIO_panel.Show();
            MenuItems_CheckControls();
        }

        void SnapShot_Click(object sender, EventArgs e)
        {
            if (flimage_io.focusing)
                StopFocus();

            flimage_io.SaveState = fileIO.CopyState();
            flimage_io.updateState(State);
            SetMax();
            SetParametersFromState(true);

            flimage_io.initializeSnapShot();
            StartGrab(false);
        }

        //////////////////////////////////////////UNCAGING//////////////////////////////////////////////////////////////////        

        public void UpdateUncagingFromDisplay()
        {
            if (uncaging_panel != null && uncaging_panel.Visible)
                uncaging_panel.BeginInvokeIfRequired(o =>
            {
                o.UpdateUncaging(o);
            });
        }



        void PlotScanGrabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[,] DataXY = HardwareControls.IOControls.makeMirrorOutput_Imaging_Uncaging(State);
            if (uncaging_PlotXY_Scanning == null || uncaging_PlotXY_Scanning.IsDisposed)
            {
                uncaging_PlotXY_Scanning = new Plot(DataXY, "Time (ms)", "Voltage (V)", State.Acq.outputRate / 1000, "Output Ch");
                uncaging_PlotXY_Scanning.setPlotTitle("Uncaging and Scanning Mirror Channels");
                //uncaging_plotPockels.setPlotTitle("Uncaging and scanning mirror channels");
                uncaging_PlotXY_Scanning.Show();
            }
            else
            {
                uncaging_PlotXY_Scanning.Replot(DataXY, "Time (ms)", "Voltage (V)", State.Acq.outputRate / 1000, "Output Ch");
            }
        }

        private void Uncage_while_image_check_Click(object sender, EventArgs e)
        {
            State.Uncaging.uncage_whileImage = Uncage_while_image_check.Checked;
            State.DO.DO_whileImage = DO_whileImaging_check.Checked;

            if (State.Uncaging.uncage_whileImage)
            {
                if (uncaging_panel == null)
                {
                    uncaging_panel = new Uncaging_Trigger_Panel(this);
                }
                uncaging_panel.Show();
            }

            if (uncaging_panel != null)
                uncaging_panel.SetupUncage(uncaging_panel);


            if (State.DO.DO_whileImage)
            {
                if (digital_panel == null)
                {
                    digital_panel = new Digital_Trigger_Panel(this);
                }
                digital_panel.Show();
            }

            if (digital_panel != null)
                digital_panel.SetupDO(digital_panel);

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
            if (motorCtrl.MotorType == MotorCtrl.MotorTypeEnum.zozolab)
            {
                if (zozo_motor_ctrl == null || zozo_motor_ctrl.IsDisposed)
                    motorCtrl.reopen();
                else
                {
                    MessageBox.Show("Zozo Lab Motor Control panel needs to be closed first.");
                }
            }

            if (motorCtrl.connected)
            {
                ResetMotor.Visible = false;
                motorCtrl.GetStatus();
                motorCtrl.GetPosition();
            }
        }

        /// <summary>
        /// Calculating the offset for split scanning so that it makes the original image together.
        /// </summary>
        private void SetYOffsetDefault()
        {
            if (State.Acq.nSplitScanning > 1)
            {
                State.Acq.XOffset_Split = new double[State.Acq.XOffset_Split.Length];
                for (int j = 0; j < State.Acq.YOffset_Split.Length; j++)
                    State.Acq.YOffset_Split[j] = HardwareControls.IOControls.GetZeroOffset(State, j);
            }
        }

        private void ZeroMirror_Click(object sender, EventArgs e)
        {
            State.Acq.XOffset = 0;
            State.Acq.YOffset = 0;
            State.Acq.XOffset_Split = new double[State.Acq.XOffset_Split.Length];
            SetYOffsetDefault();
            Fill_OffsetGUI();
            flimage_io.ResetFocus();
        }

        private void MaxScanning_Click(object sender, EventArgs e)
        {
            SetMax();
            SetParametersFromState(true);
            flimage_io.ResetFocus();
        }

        private void SetMax()
        {
            State.Acq.XOffset = 0;
            State.Acq.YOffset = 0;
            var zoom = State.Acq.zoom;
            State.Acq.zoom = 1;
            State.Acq.scanVoltageMultiplier[0] = 1;
            State.Acq.scanVoltageMultiplier[1] = 1;

            State.Acq.nSplitScanning = 0;
            int NPixels = Math.Max(State.Acq.linesPerFrame, State.Acq.pixelsPerLine);
            State.Acq.linesPerFrame = NPixels;
            State.Acq.pixelsPerLine = NPixels;
            State.Acq.scanVoltageMultiplier[0] = 1;
            State.Acq.scanVoltageMultiplier[1] = 1;

            if (zoom != State.Acq.zoom)
            {
                ZoomChangedFunc();
            }
        }


        private void NPixel_PullDown_ValueChaned(object sender, EventArgs e)
        {
            bool isFiberPhotometry = (flimage_io != null && flimage_io.microscope_system == MicroscopeSystem.FiberPhotometry)
                || (State?.Acq?.fiberPhotometryMode ?? false);
            if (isFiberPhotometry)
                return;

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
            State.Acq.Rotation_Split = new double[State.Acq.Rotation_Split.Length];
            SetParametersFromState(true);
        }

        private void Relative_Click(object sender, EventArgs e)
        {
            show_relativePosition = Relative.Checked;
            if (use_motor)
                motorCtrl.GetPosition();

            if (use_piezo)
                PiezoZ.Text = flimage_io.piezo.getPosition_um().ToString();
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
                { }
                ;
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
            flimage_io.shading = new HardwareControls.IOControls.Shading(State);
            if (shading_correction == null || shading_correction.IsDisposed)
                shading_correction = new ShadingCorrection(this);
            shading_correction.Show();
        }

        private void fastZControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fastZcontrol == null || fastZcontrol.IsDisposed)
                fastZcontrol = new FastZControl(this);

            fastZcontrol.Show();
            MenuItems_CheckControls();
        }

        private void PluginHandle_Click(object sender, EventArgs e)
        {
            //var sender1 = (ToolStripMenuItem)sender;

            for (int i = 0; i < plugin_object.Length; i++)
            {
                if (sender.Equals(plugin_tool_bar_hanels[i]))
                {
                    object instance = plugin_object[i];
                    if (!IsWindowActive(plugin_object[i]))
                    {
                        Type type = Type.GetType(plugin_names[i], true);
                        instance = Activator.CreateInstance(type, new object[] { this });
                        plugin_object[i] = instance;
                    }
                    instance.GetType().GetMethod("Show", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null).Invoke(instance, null);
                }
            }
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
                script = new RemoteControl(this);
            script.Show();
            MenuItems_CheckControls();
        }


        private void zoZoLabMotorControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (motorCtrl.MotorType != MotorCtrl.MotorTypeEnum.zozolab)
                return;

            if (zozo_motor_ctrl == null || zozo_motor_ctrl.IsDisposed)
            {
                motorCtrl.disconnect();
                ResetMotor.Text = "Restart motor";
                ResetMotor.Visible = true;

                zozo_motor_ctrl = new ZoZoMotorControl(State.Init.MotorComPort);
            }
            zozo_motor_ctrl.Show();
            MenuItems_CheckControls();
        }

        private void pMTControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pmt_control == null || pmt_control.IsDisposed)
                pmt_control = new PMTControl(State, this);
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
            double[,] DataXY = HardwareControls.IOControls.makeEOMOutput_Imaging_Uncaging(State, flimage_io.shading);

            if (uncaging_PlotPockels_Scanning == null || uncaging_PlotPockels_Scanning.IsDisposed)
            {
                uncaging_PlotPockels_Scanning = new Plot(DataXY, "Time (ms)", "Voltage (V)", State.Acq.outputRate / 1000, "Output Ch");
                if (State.Init.AO_uncagingShutter)
                {
                    uncaging_PlotPockels_Scanning.setPlotTitle("Uncaging and scanning Pockels cells: last channel = shutter");
                }
                else
                    uncaging_PlotPockels_Scanning.setPlotTitle("Uncaging Pockels cells");
                uncaging_PlotPockels_Scanning.Show();
            }
            else
            {
                uncaging_PlotPockels_Scanning.Replot(DataXY, "Time (ms)", "Voltage (V)", State.Acq.outputRate / 1000, "Output Ch");
            }
        }

        private void electrophysiologyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (physiology == null || physiology.IsDisposed)
                physiology = new StimPanel(true);
            physiology.Show();
        }

        private void analyzeEach_CheckedChanged(object sender, EventArgs e)
        {
            analyzeAfterEachAcquisition = analyzeEach.Checked;
        }

        private void digitalOutputControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (digital_panel == null || digital_panel.IsDisposed)
                digital_panel = new Digital_Trigger_Panel(this);

            digital_panel.Show();
        }

        private void ResonantCheckBox_Click(object sender, EventArgs e)
        {
            bool ON = State.Init.enableResonantScanner && ResonantCheckBox.Checked;
            State.Acq.resonantScanning = ON;
            FillGUI();
        }

        private void SPC_Off_Click(object sender, EventArgs e)
        {
            if (flimage_io.tcspc_on)
            {
                flimage_io.TCSPC_Close();
            }
            else
            {
                flimage_io.TCSPC_Open();
            }

            if (flimage_io.tcspc_on)
                SPC_Off.Text = "TCSPC off";
            else
                SPC_Off.Text = "TCSPC on";
        }

        private void StopMotorButton_Click(object sender, EventArgs e)
        {
            if (motorCtrl != null)
                motorCtrl.Stop();
        }

        private void laserWarningButton_Click(object sender, EventArgs e)
        {

        }

        private void BackToCenterRadio_Click(object sender, EventArgs e)
        {
            MotorPositioningUpdate();
        }

        private void MirrorUp_Click(object sender, EventArgs e)
        {
            double stepSize = 0.1;

            if (State.Acq.nSplitScanning > 1)
            {
                stepSize = stepSize / State.Acq.nSplitScanning;
                if (sender.Equals(MirrorDown))
                {
                    State.Acq.YOffset_Split[current_splitScanLocation] += State.Acq.YMaxVoltage / State.Acq.zoom * stepSize;
                }
                else if (sender.Equals(MirrorUp))
                {
                    State.Acq.YOffset_Split[current_splitScanLocation] -= State.Acq.YMaxVoltage / State.Acq.zoom * stepSize;
                }
                else if (sender.Equals(MirrorLeft))
                {
                    State.Acq.XOffset_Split[current_splitScanLocation] -= State.Acq.XMaxVoltage / State.Acq.zoom * stepSize;
                }
                else if (sender.Equals(MirrorRight))
                {
                    State.Acq.XOffset_Split[current_splitScanLocation] += State.Acq.XMaxVoltage / State.Acq.zoom * stepSize;
                }
            }
            else
            {
                if (sender.Equals(MirrorDown))
                {
                    State.Acq.YOffset += State.Acq.YMaxVoltage / State.Acq.zoom * stepSize;
                }
                else if (sender.Equals(MirrorUp))
                {
                    State.Acq.YOffset -= State.Acq.YMaxVoltage / State.Acq.zoom * stepSize;
                }
                else if (sender.Equals(MirrorLeft))
                {
                    State.Acq.XOffset -= State.Acq.XMaxVoltage / State.Acq.zoom * stepSize;
                }
                else if (sender.Equals(MirrorRight))
                {
                    State.Acq.XOffset += State.Acq.XMaxVoltage / State.Acq.zoom * stepSize;
                }
            }

            Fill_OffsetGUI();
            flimage_io.ResetFocus();
        }


        private void SplitDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nSplit = 1;
            nSplit = (int)Math.Pow(2, SplitDrop.SelectedIndex);

            State.Acq.nSplitScanning = nSplit;
            Fill_OffsetGUI();
        }


        private bool drag_mouse = false;
        private PointF originalPoint;

        private void ScanPosition_MouseDown(object sender, MouseEventArgs e)
        {
            drag_mouse = false;
            if (ROIs != null)
            {
                for (int loc = 0; loc < ROIs.Length; loc++)
                {
                    PointF pnt = new PointF(e.X, e.Y);
                    if (ROIs[loc].IsInsideRoi(pnt))
                    {
                        current_splitScanLocation = loc;
                        originalPoint = pnt;
                        drag_mouse = true;
                        return;
                    }
                }
            }

            if (Roi != null)
            {
                PointF pnt = new PointF(e.X, e.Y);
                if (Roi.IsInsideRoi(pnt))
                {
                    originalPoint = pnt;
                    drag_mouse = true;
                }
            }
        }

        private void calcVoltageMove(PointF pnt)
        {
            if (pnt.X < 0 || pnt.X >= ScanPosition.Width)
                return;
            if (pnt.Y < 0 || pnt.Y >= ScanPosition.Height)
                return;

            PointF movement = new PointF(pnt.X - originalPoint.X, pnt.Y - originalPoint.Y);
            double xm = State.Acq.XMaxVoltage / ScanPosition.Width * movement.X;
            double ym = State.Acq.YMaxVoltage / ScanPosition.Height * movement.Y;
            originalPoint = pnt;

            if (ROIs != null)
            {
                State.Acq.XOffset_Split[current_splitScanLocation] += xm;
                State.Acq.YOffset_Split[current_splitScanLocation] += ym;
            }

            if (Roi != null)
            {
                State.Acq.XOffset += xm;
                State.Acq.YOffset += ym;
            }
        }

        private void ScanPosition_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag_mouse)
            {
                PointF pnt = new PointF(e.X, e.Y);
                calcVoltageMove(pnt);
                UpdateScanPositionWindow();
            }
        }

        private void ScanPosition_MouseUp(object sender, MouseEventArgs e)
        {
            if (drag_mouse)
            {
                PointF pnt = new PointF(e.X, e.Y);
                calcVoltageMove(pnt);

                Fill_OffsetGUI();
                flimage_io.ResetFocus();
            }

            drag_mouse = false;

        }

        private void Enlarge_Button_Click(object sender, EventArgs e)
        {
            if (scanAreaWindow != null && !scanAreaWindow.IsDisposed)
                scanAreaWindow.Close();

            scanAreaWindow = new FLIMage.Dialogs.ScanAreaWindow(this);
            scanAreaWindow.Show();
            scanAreaWindow.Location = new Point(this.Size.Width + this.Location.X, this.Location.Y);
        }

        private void useCurrent_Button_Click(object sender, EventArgs e)
        {
            DisplaySnapShot();
        }

        private void startSimulationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (flimage_io.runningImgAcq)
            {
                flimage_io.StopSimulation();
            }
            else
            {
                flimage_io.RunSimulation();
            }

            var sdr = (ToolStripMenuItem)sender;
            sdr.Checked = flimage_io.runningImgAcq;
        }

        private void SuperImposeCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RotationUpDownButton(object sender, EventArgs e)
        {
            if (State.Acq.nSplitScanning > 1)
            {
                if (sender.Equals(RotationUpButton))
                    State.Acq.Rotation_Split[current_splitScanLocation] += 5;
                else if (sender.Equals(RotationDownButton))
                    State.Acq.Rotation_Split[current_splitScanLocation] -= 5;
                if (State.Acq.Rotation_Split[current_splitScanLocation] > 90)
                    State.Acq.Rotation_Split[current_splitScanLocation] = 90;
                else if (State.Acq.Rotation_Split[current_splitScanLocation] < -90)
                    State.Acq.Rotation_Split[current_splitScanLocation] = -90;
            }
            else
            {
                if (sender.Equals(RotationUpButton))
                    State.Acq.Rotation += 5;
                else if (sender.Equals(RotationDownButton))
                    State.Acq.Rotation -= 5;
                if (State.Acq.Rotation < -90)
                    State.Acq.Rotation = -90;
                else if (State.Acq.Rotation > 90)
                    State.Acq.Rotation = 90;


            }

            Fill_OffsetGUI();
            flimage_io.ResetFocus();
        }

        private void CFD_on_CB_Click(object sender, EventArgs e)
        {
            var saveCheck = CFD_on_CB.Checked;
            try
            {
                PQ_SettingGUI();
            }
            catch (System.FormatException)
            {
                CFD_on_CB.Checked = saveCheck;
            }
        }

        private void trigger_edge_clicked(object sender, EventArgs e)
        {
            var saveCheck = ((CheckBox)sender).Checked;
            try
            {
                PQ_SettingGUI();
            }
            catch (System.FormatException)
            {
                ((CheckBox)sender).Checked = saveCheck;
            }
        }


        public void ZoomChangedFunc()
        {
            if (State.Acq.resonantScanning || flimage_io.microscope_system == MicroscopeSystem.MiniScope)
                resonant_setting.GetDelayData();
        }


        private void SaveDelayResonant_Click(object sender, EventArgs e)
        {
            if (State.Acq.resonantScanning || flimage_io.microscope_system == MicroscopeSystem.MiniScope)
                resonant_setting.SaveJsonForZoomDelayPair();
        }
    } //Form


}
