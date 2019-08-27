using FLIMage.Analysis;
using FLIMage.FlowControls;
using FLIMage.HardwareControls;
using FLIMage.HardwareControls.StageControls;
using FLIMage.Plotting;
using FLIMage.Uncaging;
using MathLibrary;
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Utilities;

namespace FLIMage
{
    public partial class FLIMageMain : Form
    {
        public const bool DEBUGMODE = false;


        //FLIMage_IO --- main IO control.
        public FLIMage_IO flimage_io;
        object syncStateObj = new object();

        int SettingFileN = 1;

        public bool saveIntensityImage = false;
        bool use_motor = true; // use motor control.
        public bool use_mainPanel = true; //For analysis only

        public bool snapShot = false;
        Bitmap snapShotBMP;

        public bool analyzeAfterEachAcquisiiton = false;

        public ScanParameters State;
        ScanParameters Save_State;

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
        public Image_seqeunce image_seqeunce;
        public ShadingCorrection shading_correction;
        public DriftCorrection drift_correction;
        public DigitalSignalPanel DIO_panel;
        public PMTControl pmt_control;
        public FastZControl fastZcontrol;
        public StimPanel physiology;

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

        //// Setting manager
        String settingName = "FLIMageWindow";
        SettingManager settingManager;


        public FLIMageMain()
        {
            InitializeComponent();
        }


        void FLIMageMain_Load(object sender, EventArgs e)
        {
            Hide();
            ss.Show();
            Application.DoEvents();

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
                    string old_fileName = Path.Combine(State.Files.initFolderPath, "FLIM_deviceFile.txt");
                    if (File.Exists(old_fileName))
                        fileIO.LoadSetupFile(old_fileName);
                    string board = "";
                    string trigger = "";
                    string ex_trigger = "";
                    string sclock = "";
                    IOControls.GetTriggerPortName(State.Init.mirrorAOPortX, State, ref board, ref trigger, ref ex_trigger, ref sclock);
                    State.Init.MirrorAOBoard = board;
                    IOControls.GetTriggerPortName(State.Init.EOM_AI_Port0, State, ref board, ref trigger, ref ex_trigger, ref sclock);
                    State.Init.EOMBoard = board;

                    //create new file.
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


            use_motor = State.Init.motor_on;

            //Just to start withsomething...
            int height = 128; // State.Acq.linesPerFrame;
            int width = 128; // State.Acq.pixelsPerLine;
            int[] n_time = new int[] { 64, 64 }; // State.Spc.spcData.n_dataPoint;
            int nChannels = 2; // State.Acq.nChannels;
            int nZScan = 2; // State.Acq.FastZ_nSlices;
            double res = State.Spc.spcData.resolution[0];

            var FLIM_ImgData = new FLIMData(State);
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

            flimage_io = new FLIMage_IO(this);
            flimage_io.FLIM_ImgData = FLIM_ImgData;

            if (!flimage_io.use_nidaq)
            {
                acquisitionPanel.Enabled = false;
                LaserPanel.Enabled = false;
            }


            if (flimage_io.use_bh)
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

            if (flimage_io.use_pq)
            {
                sync2Group.Enabled = false;
                Resolution_Pulldown.Visible = false;
                st_nTimeP.Visible = false;
            }


            if (use_motor)
            {
                try
                {
                    if (State.Init.MotorConversionFactor.Any(x => x == 0)) //Just for compatibility with old format. Cannot be 0.
                    {
                        State.Init.MotorConversionFactor[0] = State.Motor.resolutionX;
                        State.Init.MotorConversionFactor[1] = State.Motor.resolutionY;
                        State.Init.MotorConversionFactor[2] = State.Motor.resolutionZ;
                        File.WriteAllText(State.Files.deviceFileName, fileIO.AllSetupValues_device());
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

            if (!flimage_io.use_bh && !flimage_io.use_pq)
            {
                tb_Pparameters.Enabled = false;
            }

            if (!(flimage_io.use_bh || flimage_io.use_pq) || !flimage_io.use_nidaq)
            {
                use_mainPanel = false;
            }
            else
                Show();

            SetParametersFromState(false);

            analyzeAfterEachAcquisiiton = analyzeEach.Checked;

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
            if (flimage_io.parameters.spcData.HW_Model == "TimeHarp 260 N")
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
            settingManager.AddToDict(BackToCenterRadio);
            settingManager.AddToDict(BackToStartRadio);
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
                if (readText.Contains("uncaging_panel") || State.Uncaging.uncage_whileImage && flimage_io.use_nidaq)
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

                if (readText.Contains("shading_correction") && flimage_io.use_nidaq)
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
                    script = new RemoteControl(this);
                    script.Show();
                }

                if (readText.Contains("pmt_control") && State.Init.MicroscopeSystem.Contains("Thor"))
                {
                    pmt_control = new PMTControl(State);
                    pmt_control.Show();
                }

                if (readText.Contains("physiology"))
                {
                    physiology = new StimPanel(true);
                    physiology.Show();
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
                script = new RemoteControl(this);
            }

            if (pmt_control != null && !pmt_control.IsDisposed)
            {
                pmt_control.Close();
                pmt_control = new PMTControl(State);
            }

            if (physiology != null && !physiology.IsDisposed)
            {
                physiology.Close();
                physiology = new StimPanel(true);
            }

            if (image_display != null && !image_display.IsDisposed)
            {
                image_display.Close();
                image_display = new Image_Display(flimage_io.FLIM_ImgData, this, true);
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

            if (pmt_control != null && pmt_control.Visible)
            {
                pmt_control.WindowClosing();
                sb.Append("pmt_control");
                sb.Append(",");
            }

            string allStr = sb.ToString();
            File.WriteAllText(WindowsInfoFileName(), allStr);
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

            pMTControlToolStripMenuItem.Visible = State.Init.MicroscopeSystem.Contains("Thor");
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
                StatusText.Invoke((Action)delegate
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

                flimage_io.ResetFocus();
                if (!flimage_io.grabbing && !flimage_io.focusing)
                {
                    flimage_io.Power_putEOMValues(false);
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
            if (!flimage_io.grabbing && !flimage_io.focusing)
            {
                flimage_io.Power_putEOMValues(false);
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
            double countRate1 = (double)State.Spc.datainfo.countRate[0];
            double countRate2 = (double)State.Spc.datainfo.countRate[1];

            Sync_rate.Text = String.Format("{0: 0.000} MHz", syncRate1);
            Sync_rate2.Text = String.Format("{0: 0.000} MHz", syncRate2);
            Ch_rate1.Text = String.Format("{0:#,##0.##} /s", countRate1);
            Ch_rate2.Text = String.Format("{0:#,##0.##} /s", countRate2);
            expectedRate.Text = String.Format("{0: 0.0} MHz", State.Acq.ExpectedLaserPulseRate_MHz);

            laserWarningButton.Visible = badrate;

            //if (!flimage_io.runningImgAcq)
            {
                if (flimage_io.shading != null && flimage_io.shading.calibration != null)
                {
                    double[] result = flimage_io.shading.calibration.readIntensity();
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
            snapShotBMP = ImageProcessing.FormatImage(image_display.State_intensity_range[image_display.currentChannel], flimage_io.FLIM_ImgData.Project[image_display.currentChannel]);
            ScanPosition.Invalidate();

            snapShot = false;
            State = Save_State;
            flimage_io.updateState(State);
        }

        public void CalibEOM_GUI_Update(bool[] success)
        {
            bool allTrue = true;

            for (int i = 0; i < success.Length; i++)
            {
                Control[] found = Controls.Find("tabPage" + (i + 1), true);
                Control[] button1 = Controls.Find("needCalib" + (i + 1), true);
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
                flimage_io.Power_putEOMValues(false);
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

            if (use_motor)
            {
                motorCtrl.continuousRead(ContRead.Checked);
                motorCtrl.GetPosition();
            }

            if (fastZcontrol != null)
                fastZcontrol.ControlsDuringScanning(false);

            if (BackToCenterRadio.Checked)
                MoveMotorBackToCenter();

            if (BackToStartRadio.Checked)
                MoveMotorBackToStart();
            //SaveSetting();
            //
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
            flimage_io.State = State;
            flimage_io.StartGrab(focus);
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

                if (!flimage_io.looping)
                {
                    int error = flimage_io.CheckSavingParameters();
                    if (error < 0)
                    {
                        ChangeItemsStatus(true, false);
                        return;
                    }
                }

                if (use_motor)
                {
                    motorCtrl.GetPosition();
                    motorCtrl.continuousRead(false);
                }


                if (State.Acq.ZStack && State.Acq.nSlices > 1 && use_motor)
                {
                    if (motorCtrl.stack_Position != MotorCtrl.StackPosition.Undefined)
                    {
                        if (BackToCenterRadio.Checked)
                        {
                            if (motorCtrl.stack_Position == MotorCtrl.StackPosition.Center)
                            {
                                Set_Center_Click(Set_Center, null);
                                MoveMotorFromCenterToStart();
                            }
                            else
                                MoveMotorBackToStart();
                        }

                        if (BackToStartRadio.Checked)
                        {
                            if (motorCtrl.stack_Position == MotorCtrl.StackPosition.Start)
                                Set_Top_Click(Set_Top, null); //Set the start position.
                            else
                                MoveMotorBackToStart();
                        }
                    }
                }

                State.Uncaging.uncage_whileImage = Uncage_while_image_check.Checked;

                if (State.Uncaging.uncage_whileImage && uncaging_panel != null)
                    uncaging_panel.SetupUncage(uncaging_panel);

                GrabButton.Text = "ABORT";

                if (analyzeEach.Checked)
                    image_display.plot_regular.Show();

            }
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
            flimage_io.grabbing = true;
            flimage_io.looping = true;
            flimage_io.allowLoop = true; //This is the difference between loop and grab.
            flimage_io.stopGrabActivated = false;

            if (this.InvokeRequired)
                this.Invoke((Action)delegate { LoopButton.Text = "STOP"; });
            else
                LoopButton.Text = "STOP";

            GrabButton.Enabled = false;
            flimage_io.internalImageCounter = 0;

            StartGrab(false);
        }

        public void StopLoop()
        {
            flimage_io.stopGrabActivated = true;
            StopGrab(true);


            if (this.InvokeRequired)
                this.Invoke((Action)delegate
                {
                    LoopButton.Text = "LOOP";
                    if (!flimage_io.imageSequencing)
                        GrabButton.Enabled = true;
                });
            else
            {
                LoopButton.Text = "LOOP";
                if (!flimage_io.imageSequencing)
                    GrabButton.Enabled = true;
            }
            flimage_io.looping = false;
        }

        private void GrabButtonClick(object sender, EventArgs e)
        {
            if (flimage_io.focusing)
                StopFocus();

            if (!flimage_io.grabbing)
            {
                flimage_io.grabbing = true;
                flimage_io.looping = false;
                flimage_io.allowLoop = false;
                flimage_io.stopGrabActivated = false;
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
                flimage_io.stopGrabActivated = false;
                StartGrab(true);
            }
            else
            {
                StopFocus();
            }
        }


        /// <summary>
        /// GUI --> State called when GUI is changed.
        /// </summary>
        /// <param name="sdr"></param>
        public void GetParametersFromGUI(object sdr)
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

                flimage_io.ResetFocus();

                if (!flimage_io.grabbing && !flimage_io.focusing)
                {
                    flimage_io.Power_putEOMValues(false);
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
                    flimage_io.FLIM_ImgData.KeepPagesInMemory = KeepPagesInMemoryCheck.Checked;
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
                            //State.Acq.msPerLine = Math.Round(10 * State.Acq.msPerLine) / 10.0;
                        }
                    }

                    else if (sdr.Equals(ScanFraction))
                    {
                        if (!Double.TryParse(ScanFraction.Text, out scanFraction1)) scanFraction1 = State.Acq.scanFraction;

                        if (scanFraction1 > State.Acq.fillFraction && scanFraction1 > 0.7 && scanFraction1 <= 0.99)
                        {
                            if (State.Init.MicroscopeSystem == "ThorBScopeGG" && scanFraction1 > 0.86)
                                scanFraction1 = 0.86;

                            State.Acq.scanFraction = scanFraction1;
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
                        if (fillFraction1 <= 0.99 && fillFraction1 > 0.65 && fillFraction1 < State.Acq.scanFraction)
                        {
                            if (State.Init.MicroscopeSystem == "ThorBScopeGG" && fillFraction1 < 0.7)
                                fillFraction1 = 0.7;

                            State.Acq.fillFraction = fillFraction1;
                            //State.Acq.ScanDelay = State.Acq.msPerLine * (State.Acq.scanFraction - State.Acq.fillFraction);
                        }
                    }

                    if (Int32.TryParse(linesPerFrame.Text, out valI)) State.Acq.linesPerFrame = valI;

                    double maxX = State.Acq.XMaxVoltage;
                    double maxY = State.Acq.YMaxVoltage;

                    Double.TryParse(MaxRangeX.Text, out maxX);
                    Double.TryParse(MaxRangeY.Text, out maxY);

                    if (State.Init.MicroscopeSystem == "ThorBScopeGG" && maxX > 5)
                        maxX = 5;

                    if (State.Init.MicroscopeSystem == "ThorBScopeGG" && maxY > 5)
                        maxY = 5;

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

                    if (xOffset < State.Acq.XMaxVoltage && xOffset > -State.Acq.XMaxVoltage)
                        State.Acq.XOffset = xOffset;

                    if (yOffset < State.Acq.YMaxVoltage && yOffset > -State.Acq.YMaxVoltage)
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
                    if (InvokeRequired)
                    {
                        Invoke((Action)delegate { FillGUI(); });
                    }
                    else
                        FillGUI();
                } //sync
            }

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
                MeasuredLineCorrection.Text = String.Format("{0:0.000}", flimage_io.parameters.spcData.measured_line_time_correction);
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
            CurrentSlice.Text = (flimage_io.internalSliceCounter).ToString();
        }

        public void UpdateAverageFrameCounter()
        {
            try
            {
                Invoke((Action)delegate
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
                });
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

            if (Int32.TryParse(FreqDivBox.Text, out valI)) State.Spc.spcData.sync_divider[0] = valI;

            if (flimage_io.use_pq)
            {
                if (Binning_setting.SelectedIndex >= 0)
                    State.Spc.spcData.binning = Binning_setting.SelectedIndex;

                //if (Int32.TryParse(binning.Text, out valI)) State.Spc.spcData.binning = valI;
                if (Int32.TryParse(NTimePoints.Text, out valI)) State.Spc.spcData.n_dataPoint = valI;
                if (Int32.TryParse(StartPointBox.Text, out valI)) State.Spc.spcData.startPoint = valI;

                if (PQMode_Pulldown.SelectedIndex == 0)
                    State.Spc.spcData.acq_modePQ = 3;
                else if (PQMode_Pulldown.SelectedIndex == 1)
                    State.Spc.spcData.acq_modePQ = 2;
            }
            else if (flimage_io.use_bh)
            {
                if (Int32.TryParse(Resolution_Pulldown.SelectedItem.ToString(), out valI)) State.Spc.spcData.n_dataPoint = valI;
            }

            SetupFLIMParameters();
        }

        public void changeScanArea(ROI roi)
        {
            var FLIM_ImgData = flimage_io.FLIM_ImgData;

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
            var parameters = flimage_io.parameters;

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
            var parameters = flimage_io.parameters;

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

            if (flimage_io.use_pq && parameters.spcData.acq_modePQ == 2)
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

            if (flimage_io.FiFo_acquire != null)
                flimage_io.FiFo_acquire.SetupParameters(flimage_io.focusing, parameters);

            if (this.InvokeRequired)
            {
                this.Invoke((Action)delegate
               {
                   UpdateSPC_GUI();
               });
            }
            else
                UpdateSPC_GUI();

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

            Uncage_while_image_check.Checked = State.Uncaging.uncage_whileImage;

            ScanPosition.Invalidate();
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


            resolution.Text = String.Format("{0:0.0}", State.Spc.spcData.resolution[0]);
            resolution2.Text = String.Format("{0:0.0}", State.Spc.spcData.resolution[0]);

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
                resolution2.Text = String.Format("{0:0.0}", State.Spc.spcData.resolution[1]);

                int n_bit = (int)Math.Ceiling(Math.Log(State.Spc.spcData.n_dataPoint, 2));
                if (Binning_setting.Items.Count > n_bit - 6)
                    Resolution_Pulldown.SelectedIndex = n_bit - 6;

            }
            else if (flimage_io.use_pq)
            {
                NTimePoints.Text = Convert.ToString(State.Spc.spcData.n_dataPoint);
                StartPointBox.Text = Convert.ToString(State.Spc.spcData.startPoint);

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

            if (flimage_io != null)
                CurrentImage.Text = flimage_io.internalImageCounter.ToString();
        }

        /// <summary>
        /// When called from different thread, the function is invoekd from this thread.
        /// </summary>
        public void UpdateFileName()
        {
            if (this.InvokeRequired)
                this.Invoke((Action)delegate
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

            if (focus && !status || flimage_io.imageSequencing)
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


        public void SetParametersFromState(bool notify)
        {
            FillGUI();
            GetParametersFromGUI(this);

            if (State.Uncaging.uncage_whileImage && uncaging_panel != null)
                uncaging_panel.Show();

            flimage_io.updateState(State);
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
            State.Spc.analysis = flimage_io.FLIM_ImgData.State.Spc.analysis;
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


            if (use_motor)
            {
                motorCtrl.unsubscribe();
                motorCtrl.MotH -= MotorListener;
            }

            flimage_io.StopNIDAQIOControls();
            flimage_io.TCSPC_Close();

            com_server.Close();
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
                if (!flimage_io.grabbing && !flimage_io.focusing)
                {
                    flimage_io.stopGrabActivated = false;
                    flimage_io.grabbing = true;
                    flimage_io.allowLoop = false;
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
                if (uncaging_panel == null)
                    uncaging_panel = new Uncaging_Trigger_Panel(this);

                uncaging_panel.Show();
                uncaging_panel.Activate();

                this.Invoke((Action)delegate
                {
                    image_display.uncaging_on = true;
                    image_display.Activate_uncaging(true);

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
            else if (command == "UncagingStart")
            {
                flimage_io.NotifyEventExternal("UncagingStart");
            }
            else if (command == "UncagingDone")
            {
                flimage_io.NotifyEventExternal("UncagingDone");
            }
            else if (command == "LoadSettingWithNumber")
            {
                int defaultVal = 1;
                int argnumber = 1;
                if (!Int32.TryParse(argument, out argnumber)) argnumber = defaultVal;
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
            else if (command == "Focus")
            {
                FocusButton_Click(this, null);
            }
            else if (command == "SetMotorPosition")
            {
                SetMotorPosition(false, true); //Block the thread until movement done.
            }
            else if (command == "UpdateGUI")
            {
                ReSetupValues(true);
            }
            else if (command == "OpenFile")
            {
                image_display.Invoke((Action)delegate
                {
                    image_display.OpenFLIM(argument, true);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "BinFrames")
            {
                int defaultVal = 1;
                int argnumber = 1;
                if (!Int32.TryParse(argument, out argnumber)) argnumber = defaultVal;

                image_display.Invoke((Action)delegate
                {
                    image_display.BinFrames(argnumber);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "AlignFrames")
            {
                image_display.Invoke((Action)delegate
                {
                    image_display.AlignFrames();
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "FitData")
            {
                image_display.Invoke((Action)delegate
                {
                    image_display.FitData(false, true);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "FitEachFrame")
            {
                int defaultVal = 1;
                int argnumber = 1;
                if (!Int32.TryParse(argument, out argnumber)) argnumber = defaultVal;

                image_display.Invoke((Action)delegate
                {
                    image_display.plot_regular.TurnOnCalcFit(argnumber != 0);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "ReadImageJROI")
            {
                image_display.Invoke((Action)delegate
                {
                    image_display.ReadImageJROI(argument);
                });
                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "CalcTimeCourse")
            {
                image_display.Invoke((Action)delegate
                {
                    if (image_display.TC != null)
                        image_display.TC.ImInfos.Clear();
                    image_display.CalculateTimecourse(true);
                    image_display.plot_regular.updatePlot();
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "SetFLIMIntensityOffset")
            {
                double[] defaultVals = new double[] { 100, 1000 };
                String[] sP = argument.Split(',');
                if (sP.Length != 2)
                    return false;

                double[] argnumbers = new double[sP.Length];
                for (int i = 0; i < sP.Length; i++)
                    if (!double.TryParse(sP[i], out argnumbers[i])) argnumbers[i] = defaultVals[i];

                image_display.Invoke((Action)delegate
                {
                    image_display.SetFLIMIntensityOffset(argnumbers);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "ApplyFitOffset")
            {
                image_display.Invoke((Action)delegate
                {
                    image_display.ApplyOffset();
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));

            }
            else if (command == "FixTauAll")
            {
                int defaultVal = 1;
                int argnumber = 1;
                if (!Int32.TryParse(argument, out argnumber)) argnumber = defaultVal;

                image_display.Invoke((Action)delegate
                {
                    image_display.FixTauAll(argnumber != 0);
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
                    if (!double.TryParse(sP[i], out argnumbers[i])) argnumbers[i] = defaultVals[i];

                image_display.Invoke((Action)delegate
                {
                    image_display.FixTau(argnumbers);
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
                    if (!int.TryParse(sP[i], out argnumbers[i])) argnumbers[i] = defaultVals[i];

                image_display.Invoke((Action)delegate
                {
                    image_display.SetFitRange(argnumbers);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "SetChannel")
            {
                int defaultVal = 1;
                int argnumber = 1;
                if (!Int32.TryParse(argument, out argnumber)) argnumber = defaultVal;

                image_display.Invoke((Action)delegate
                {
                    image_display.SetChannel(argnumber);
                });

                flimage_io.Notify(new ProcessEventArgs("ExtCommandExecuted", null));
            }
            else if (command == "GetParametersOfImageFile")
            {
                State = image_display.FLIM_ImgData.State;
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
            double[,] DataEOM = IOControls.MakeEOMOutput(State, flimage_io.shading, false, false);
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
            if (snapShotBMP == null)
                return;

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
            motorCtrl.GetPosition();
            if (ContRead.Checked)
                motorCtrl.GetStatus();
            //motorCtrl.GetPosition();
        }

        void Motor_TextSet()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)delegate
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
            //this.Enabled = !motorCtrl.start_moving;

        }


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

        public void MoveBackToHome()
        {
            if (State.Acq.ZStack && State.Acq.sliceStep != 0 && State.Acq.nSlices > 1)
            {
                if (BackToStartRadio.Checked)
                    MoveMotorBackToStart();
                else if (BackToCenterRadio.Checked)
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
                motorCtrl.ZStackStart = current[2] - ZStackHalfStroke();
                motorCtrl.ZStackCenter = current[2];
                motorCtrl.ZStackEnd = current[2] + ZStackHalfStroke();
            }
        }

        double ZStackHalfStroke()
        {
            return (double)(State.Acq.nSlices - 1) * State.Acq.sliceStep / motorCtrl.resolutionZ / 2.0;
        }

        /// <summary>
        /// Set motor position to new position.
        /// </summary>
        public void SetMotorPosition(bool warningOn, bool waitUntilFinish)
        {
            if (waitUntilFinish)
                WaitForMotorMove(); //make sure that there is no task remaining.

            flimage_io.Notify(new ProcessEventArgs("StageMoveStart", null));

            //this.Enabled = false;
            motorCtrl.IfStepTooBig(warningOn);

            //if (StepSizeX != 0 || StepSizeY != 0 || StepSizeZ != 0)
            motorCtrl.SetPosition();

            if (waitUntilFinish) //For external command it will be always true.
            {
                WaitForMotorMove();
                motorCtrl.GetPosition();
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

            motorCtrl.GetPosition();

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
            motorCtrl.GetPosition();
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
            motorCtrl.GetPosition();
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
            motorCtrl.GetPosition();
            double[] position = motorCtrl.CurrentUncalibratedPosition();
            motorCtrl.ZStackStart = position[2];
            motorCtrl.ZStackCenter = motorCtrl.ZStackStart + ZStackHalfStroke();
            motorCtrl.ZStackEnd = motorCtrl.ZStackStart + 2 * ZStackHalfStroke();
            motorCtrl.stack_Position = MotorCtrl.StackPosition.Start;
            CalcStackSize();
            MotorHandler(new MotrEventArgs(""));
        }

        private void Set_Center_Click(object sender, EventArgs e)
        {
            AutoCalculateStackStartEnd();
            motorCtrl.stack_Position = MotorCtrl.StackPosition.Center;
            MotorHandler(new MotrEventArgs(""));
        }

        void Set_bottom_Click(object sender, EventArgs e)
        {
            if (motorCtrl.ZStackStart == motorCtrl.minMotorVal)
                MessageBox.Show("Please choose Start position first");
            else
            {
                motorCtrl.GetPosition();
                double[] position = motorCtrl.CurrentUncalibratedPosition();
                motorCtrl.ZStackEnd = position[2];
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
            if (!this.InvokeRequired)
            {
                ImageDisplaySetup();
            }
            else
            {
                this.Invoke((Action)delegate
                {
                    ImageDisplaySetup();
                });
            }
        }

        private void ImageDisplaySetup()
        {
            if (image_display == null || image_display.IsDisposed)
                image_display = new Image_Display(flimage_io.FLIM_ImgData, this, false);
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
            if (flimage_io.focusing)
                StopFocus();

            snapShot = true;
            Save_State = fileIO.CopyState();
            flimage_io.updateState(State);
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


            flimage_io.stopGrabActivated = false;
            StartGrab(true);
        }

        //////////////////////////////////////////UNCAGING//////////////////////////////////////////////////////////////////        

        public void UpdateUncagingFromDisplay()
        {
            if (uncaging_panel != null && uncaging_panel.InvokeRequired && uncaging_panel.Visible)
            {
                uncaging_panel.Invoke((Action)delegate
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
            motorCtrl.GetStatus();
            motorCtrl.GetPosition();
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
            motorCtrl.GetPosition();
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
                script = new RemoteControl(this);
            script.Show();
            MenuItems_CheckControls();
        }


        private void pMTControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pmt_control == null || pmt_control.IsDisposed)
                pmt_control = new PMTControl(State);
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
            double[,] DataXY = IOControls.makeEOMOutput_Imaging_Uncaging(State, flimage_io.shading);

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

        private void electrophysiologyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (physiology == null || physiology.IsDisposed)
                physiology = new StimPanel(true);
            physiology.Show();
        }

        private void analyzeEach_CheckedChanged(object sender, EventArgs e)
        {
            analyzeAfterEachAcquisiiton = analyzeEach.Checked;
        }

    } //Form


}
