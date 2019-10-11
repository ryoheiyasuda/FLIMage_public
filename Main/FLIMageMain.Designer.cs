namespace FLIMage
{
    partial class FLIMageMain
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        public System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        public void InitializeComponent()
        {
            System.Windows.Forms.TabPage tabPage2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FLIMageMain));
            this.powerRead2 = new System.Windows.Forms.Label();
            this.needCalib2 = new System.Windows.Forms.Button();
            this.UncageLaser2 = new System.Windows.Forms.CheckBox();
            this.ImageLaser2 = new System.Windows.Forms.CheckBox();
            this.label19 = new System.Windows.Forms.Label();
            this.PowerSlider2 = new System.Windows.Forms.TrackBar();
            this.Power2 = new System.Windows.Forms.TextBox();
            this.CurrentSlice = new System.Windows.Forms.TextBox();
            this.NSlices = new System.Windows.Forms.TextBox();
            this.NImages = new System.Windows.Forms.TextBox();
            this.CurrentImage = new System.Windows.Forms.TextBox();
            this.st_slices = new System.Windows.Forms.Label();
            this.st_NAcq = new System.Windows.Forms.Label();
            this.st_NDone = new System.Windows.Forms.Label();
            this.ImageInterval = new System.Windows.Forms.TextBox();
            this.st_interval = new System.Windows.Forms.Label();
            this.SliceInterval = new System.Windows.Forms.TextBox();
            this.FrameInterval = new System.Windows.Forms.TextBox();
            this.ETime = new System.Windows.Forms.Label();
            this.AveFrame_Check = new System.Windows.Forms.CheckBox();
            this.NumAve = new System.Windows.Forms.TextBox();
            this.CurrentFrame = new System.Windows.Forms.TextBox();
            this.PowerBoxImageParameters = new System.Windows.Forms.GroupBox();
            this.N_AveragedFrames2 = new System.Windows.Forms.TextBox();
            this.N_AveragedFrames1 = new System.Windows.Forms.TextBox();
            this.N_AveragedSlices = new System.Windows.Forms.TextBox();
            this.NAveragedSliceLabel = new System.Windows.Forms.Label();
            this.TotalNFrames2Label = new System.Windows.Forms.Label();
            this.N_AveSlices = new System.Windows.Forms.TextBox();
            this.NAveSliceLabel = new System.Windows.Forms.Label();
            this.SliceIntervalLabel = new System.Windows.Forms.Label();
            this.label101 = new System.Windows.Forms.Label();
            this.label102 = new System.Windows.Forms.Label();
            this.AveFrame2_Check = new System.Windows.Forms.CheckBox();
            this.TotalNFramesLabel1 = new System.Windows.Forms.Label();
            this.aveSlice_Interval = new System.Windows.Forms.Label();
            this.aveFrame_Interval = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.NFrames = new System.Windows.Forms.TextBox();
            this.AOCounter = new System.Windows.Forms.Label();
            this.label91 = new System.Windows.Forms.Label();
            this.SavedFileN = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label78 = new System.Windows.Forms.Label();
            this.label66 = new System.Windows.Forms.Label();
            this.nAverageFrame = new System.Windows.Forms.Label();
            this.AveSlices_check = new System.Windows.Forms.CheckBox();
            this.Measured_slice_interval = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label60 = new System.Windows.Forms.Label();
            this.Misc_about_Slice = new System.Windows.Forms.Label();
            this.ZStack_radio = new System.Windows.Forms.RadioButton();
            this.Timelapse_radio = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.ETime2 = new System.Windows.Forms.Label();
            this.FocusButton = new System.Windows.Forms.Button();
            this.GrabButton = new System.Windows.Forms.Button();
            this.XY_panel = new System.Windows.Forms.GroupBox();
            this.XYMotorStep = new System.Windows.Forms.TextBox();
            this.label43 = new System.Windows.Forms.Label();
            this.XUp = new System.Windows.Forms.Button();
            this.XDown = new System.Windows.Forms.Button();
            this.YDown = new System.Windows.Forms.Button();
            this.YUp = new System.Windows.Forms.Button();
            this.label44 = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.label46 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.label48 = new System.Windows.Forms.Label();
            this.label49 = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.label51 = new System.Windows.Forms.Label();
            this.Zero_all = new System.Windows.Forms.Button();
            this.ZRead = new System.Windows.Forms.TextBox();
            this.YRead = new System.Windows.Forms.TextBox();
            this.Set_bottom = new System.Windows.Forms.Button();
            this.XRead = new System.Windows.Forms.TextBox();
            this.Set_Top = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ZMotorStep = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.Zdown = new System.Windows.Forms.Button();
            this.label53 = new System.Windows.Forms.Label();
            this.Zup = new System.Windows.Forms.Button();
            this.zero_Z = new System.Windows.Forms.Button();
            this.stagePanel = new System.Windows.Forms.GroupBox();
            this.ResetMotor = new System.Windows.Forms.Button();
            this.MotorReadButton = new System.Windows.Forms.Button();
            this.Motor_Status = new System.Windows.Forms.Label();
            this.Relative = new System.Windows.Forms.CheckBox();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.Velocity = new System.Windows.Forms.TextBox();
            this.Vel_Down = new System.Windows.Forms.Button();
            this.Vel_Up = new System.Windows.Forms.Button();
            this.ContRead = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.StayMotorRadio = new System.Windows.Forms.RadioButton();
            this.BackToCenterRadio = new System.Windows.Forms.RadioButton();
            this.BackToStartRadio = new System.Windows.Forms.RadioButton();
            this.ZCenter = new System.Windows.Forms.TextBox();
            this.GoCenter = new System.Windows.Forms.Button();
            this.CenterLabel = new System.Windows.Forms.Label();
            this.Set_Center = new System.Windows.Forms.Button();
            this.GoEnd = new System.Windows.Forms.Button();
            this.GoStart = new System.Windows.Forms.Button();
            this.label77 = new System.Windows.Forms.Label();
            this.label72 = new System.Windows.Forms.Label();
            this.NSlices2 = new System.Windows.Forms.TextBox();
            this.label42 = new System.Windows.Forms.Label();
            this.ZEnd = new System.Windows.Forms.TextBox();
            this.SliceStep = new System.Windows.Forms.TextBox();
            this.ZStart = new System.Windows.Forms.TextBox();
            this.st_step = new System.Windows.Forms.Label();
            this.st_um = new System.Windows.Forms.Label();
            this.st_display = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.loadSttingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSettingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quickSettingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSetting1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSetting2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSetting3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.loadSetting1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadSetting2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadSetting3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.loadScanParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.powerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calibrateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCalibrationCurveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showInputoutputCurveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.plotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.plotScanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.plotPockelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.plotScanGrabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.plotPockelsGrabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.realtimePlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetWindowPositionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nIDAQConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dIOPanelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pMTControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncagingControlToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.digitalOutputControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageSeqControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fastZControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shadingCorretionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.driftCorrectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteControlToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.stageControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.electrophysiologyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fLIMageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Uncage_while_image_check = new System.Windows.Forms.CheckBox();
            this.Ch_rate2 = new System.Windows.Forms.Label();
            this.Ch_rate1 = new System.Windows.Forms.Label();
            this.Sync_rate = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.LaserPanel = new System.Windows.Forms.GroupBox();
            this.zeroVoltage = new System.Windows.Forms.Button();
            this.needCalibLabel = new System.Windows.Forms.Label();
            this.Calibrate1 = new System.Windows.Forms.Button();
            this.laserPowerPanel = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.powerRead1 = new System.Windows.Forms.Label();
            this.needCalib1 = new System.Windows.Forms.Button();
            this.UncageLaser1 = new System.Windows.Forms.CheckBox();
            this.ImageLaser1 = new System.Windows.Forms.CheckBox();
            this.st_Power = new System.Windows.Forms.Label();
            this.PowerSlider1 = new System.Windows.Forms.TrackBar();
            this.Power1 = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.powerRead3 = new System.Windows.Forms.Label();
            this.needCalib3 = new System.Windows.Forms.Button();
            this.UncageLaser3 = new System.Windows.Forms.CheckBox();
            this.ImageLaser3 = new System.Windows.Forms.CheckBox();
            this.label20 = new System.Windows.Forms.Label();
            this.PowerSlider3 = new System.Windows.Forms.TrackBar();
            this.Power3 = new System.Windows.Forms.TextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.powerRead4 = new System.Windows.Forms.Label();
            this.needCalib4 = new System.Windows.Forms.Button();
            this.UncageLaser4 = new System.Windows.Forms.CheckBox();
            this.ImageLaser4 = new System.Windows.Forms.CheckBox();
            this.label17 = new System.Windows.Forms.Label();
            this.PowerSlider4 = new System.Windows.Forms.TrackBar();
            this.Power4 = new System.Windows.Forms.TextBox();
            this.laserWarningButton = new System.Windows.Forms.Button();
            this.acquisitionPanel = new System.Windows.Forms.GroupBox();
            this.DO_whileImaging_check = new System.Windows.Forms.CheckBox();
            this.analyzeEach = new System.Windows.Forms.CheckBox();
            this.label79 = new System.Windows.Forms.Label();
            this.ExtTriggerCB = new System.Windows.Forms.CheckBox();
            this.expectedRate = new System.Windows.Forms.Label();
            this.label67 = new System.Windows.Forms.Label();
            this.Sync_rate2 = new System.Windows.Forms.Label();
            this.st_BaseName = new System.Windows.Forms.Label();
            this.st_PathName = new System.Windows.Forms.Label();
            this.st_fileN = new System.Windows.Forms.Label();
            this.BaseName = new System.Windows.Forms.TextBox();
            this.FileN = new System.Windows.Forms.TextBox();
            this.DirectoryName = new System.Windows.Forms.TextBox();
            this.FindPath = new System.Windows.Forms.Button();
            this.label70 = new System.Windows.Forms.Label();
            this.FileName = new System.Windows.Forms.TextBox();
            this.Panel_Files = new System.Windows.Forms.GroupBox();
            this.FullFileName = new System.Windows.Forms.TextBox();
            this.ImageIteration = new System.Windows.Forms.GroupBox();
            this.LoopButton = new System.Windows.Forms.Button();
            this.label88 = new System.Windows.Forms.Label();
            this.tb_Pparameters = new System.Windows.Forms.TabPage();
            this.label97 = new System.Windows.Forms.Label();
            this.FreqDivBox = new System.Windows.Forms.TextBox();
            this.label96 = new System.Windows.Forms.Label();
            this.StartPointBox = new System.Windows.Forms.TextBox();
            this.st_mode = new System.Windows.Forms.Label();
            this.PQMode_Pulldown = new System.Windows.Forms.ComboBox();
            this.Binning_setting = new System.Windows.Forms.ComboBox();
            this.st_nTimeP = new System.Windows.Forms.Label();
            this.Resolution_Pulldown = new System.Windows.Forms.ComboBox();
            this.sync2Group = new System.Windows.Forms.GroupBox();
            this.label80 = new System.Windows.Forms.Label();
            this.label81 = new System.Windows.Forms.Label();
            this.label82 = new System.Windows.Forms.Label();
            this.label83 = new System.Windows.Forms.Label();
            this.sync_offset2 = new System.Windows.Forms.TextBox();
            this.label84 = new System.Windows.Forms.Label();
            this.sync_threshold2 = new System.Windows.Forms.TextBox();
            this.label85 = new System.Windows.Forms.Label();
            this.sync_zc_level2 = new System.Windows.Forms.TextBox();
            this.st_tp = new System.Windows.Forms.Label();
            this.NTimePoints = new System.Windows.Forms.TextBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label26 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.sync_offset = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.sync_threshold = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.sync_zc_level = new System.Windows.Forms.TextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label30 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.label33 = new System.Windows.Forms.Label();
            this.ch_offset1 = new System.Windows.Forms.TextBox();
            this.label34 = new System.Windows.Forms.Label();
            this.ch_threshold1 = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.ch_zc_level1 = new System.Windows.Forms.TextBox();
            this.resolution = new System.Windows.Forms.TextBox();
            this.label28 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.resolution2 = new System.Windows.Forms.TextBox();
            this.label87 = new System.Windows.Forms.Label();
            this.label86 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.label37 = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.label39 = new System.Windows.Forms.Label();
            this.ch_offset2 = new System.Windows.Forms.TextBox();
            this.label40 = new System.Windows.Forms.Label();
            this.ch_threshold2 = new System.Windows.Forms.TextBox();
            this.label41 = new System.Windows.Forms.Label();
            this.ch_zc_level2 = new System.Windows.Forms.TextBox();
            this.st_binning = new System.Windows.Forms.Label();
            this.tbScanParam = new System.Windows.Forms.TabPage();
            this.SineWaveScanning_CB = new System.Windows.Forms.CheckBox();
            this.SwitchXY_CB = new System.Windows.Forms.CheckBox();
            this.FlipY_CB = new System.Windows.Forms.CheckBox();
            this.FlipX_CB = new System.Windows.Forms.CheckBox();
            this.MeasuredLineCorrection = new System.Windows.Forms.Label();
            this.label95 = new System.Windows.Forms.Label();
            this.LineTimeCorrection = new System.Windows.Forms.TextBox();
            this.ScanDelay = new System.Windows.Forms.TextBox();
            this.ScanFraction = new System.Windows.Forms.TextBox();
            this.FillFraction = new System.Windows.Forms.TextBox();
            this.MaxRangeY = new System.Windows.Forms.TextBox();
            this.MaxRangeX = new System.Windows.Forms.TextBox();
            this.pixelTime = new System.Windows.Forms.TextBox();
            this.MsPerLine = new System.Windows.Forms.TextBox();
            this.pixelsPerLine = new System.Windows.Forms.TextBox();
            this.linesPerFrame = new System.Windows.Forms.TextBox();
            this.label94 = new System.Windows.Forms.Label();
            this.BiDirecCB = new System.Windows.Forms.CheckBox();
            this.label89 = new System.Windows.Forms.Label();
            this.pb32 = new System.Windows.Forms.Button();
            this.pb1024 = new System.Windows.Forms.Button();
            this.pb512 = new System.Windows.Forms.Button();
            this.pb64 = new System.Windows.Forms.Button();
            this.pb256 = new System.Windows.Forms.Button();
            this.pb128 = new System.Windows.Forms.Button();
            this.label61 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.st_us = new System.Windows.Forms.Label();
            this.st_LineTime = new System.Windows.Forms.Label();
            this.st_PixelsPerLine = new System.Windows.Forms.Label();
            this.st_LinesPerFrame = new System.Windows.Forms.Label();
            this.st_ms = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.st_ScanFraction = new System.Windows.Forms.Label();
            this.st_ScanDelay = new System.Windows.Forms.Label();
            this.st_FillFraction = new System.Windows.Forms.Label();
            this.AdvancedCheck = new System.Windows.Forms.CheckBox();
            this.tb_ScanParameters = new System.Windows.Forms.TabPage();
            this.CurrentFOVY = new System.Windows.Forms.TextBox();
            this.CurrentFOVX = new System.Windows.Forms.TextBox();
            this.label54 = new System.Windows.Forms.Label();
            this.label90 = new System.Windows.Forms.Label();
            this.label92 = new System.Windows.Forms.Label();
            this.label93 = new System.Windows.Forms.Label();
            this.FieldOfViewY = new System.Windows.Forms.TextBox();
            this.FieldOfViewX = new System.Windows.Forms.TextBox();
            this.label68 = new System.Windows.Forms.Label();
            this.label65 = new System.Windows.Forms.Label();
            this.label63 = new System.Windows.Forms.Label();
            this.label64 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.Objective_Pulldown = new System.Windows.Forms.ComboBox();
            this.NPixels_PulldownY = new System.Windows.Forms.ComboBox();
            this.NPixels_PulldownX = new System.Windows.Forms.ComboBox();
            this.MaxScanning = new System.Windows.Forms.Button();
            this.ZeroAngle = new System.Windows.Forms.Button();
            this.Rotation = new System.Windows.Forms.TextBox();
            this.YOffset = new System.Windows.Forms.TextBox();
            this.XOffset = new System.Windows.Forms.TextBox();
            this.MirrorStep = new System.Windows.Forms.TextBox();
            this.Zoom = new System.Windows.Forms.TextBox();
            this.label76 = new System.Windows.Forms.Label();
            this.label75 = new System.Windows.Forms.Label();
            this.label71 = new System.Windows.Forms.Label();
            this.label69 = new System.Windows.Forms.Label();
            this.label73 = new System.Windows.Forms.Label();
            this.label74 = new System.Windows.Forms.Label();
            this.ZeroMirror = new System.Windows.Forms.Button();
            this.label62 = new System.Windows.Forms.Label();
            this.SnapShotButton = new System.Windows.Forms.Button();
            this.Zoom10 = new System.Windows.Forms.NumericUpDown();
            this.ScanPosition = new System.Windows.Forms.PictureBox();
            this.label59 = new System.Windows.Forms.Label();
            this.label58 = new System.Windows.Forms.Label();
            this.MirrorYDown = new System.Windows.Forms.Button();
            this.label57 = new System.Windows.Forms.Label();
            this.label56 = new System.Windows.Forms.Label();
            this.label55 = new System.Windows.Forms.Label();
            this.label52 = new System.Windows.Forms.Label();
            this.MirrorXUp = new System.Windows.Forms.Button();
            this.MirrorXDown = new System.Windows.Forms.Button();
            this.MirrorYUp = new System.Windows.Forms.Button();
            this.Zoom100 = new System.Windows.Forms.NumericUpDown();
            this.st_zoom = new System.Windows.Forms.Label();
            this.ZoomP1 = new System.Windows.Forms.NumericUpDown();
            this.Zoom1 = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.FLIMSetting_tab = new System.Windows.Forms.TabControl();
            this.ChannelSettingTab = new System.Windows.Forms.TabPage();
            this.KeepPagesInMemoryCheck = new System.Windows.Forms.CheckBox();
            this.AveFrameSeparately = new System.Windows.Forms.CheckBox();
            this.SaveInSeparatedFileCheck = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.Intensity_Radio2 = new System.Windows.Forms.RadioButton();
            this.Acquisition2 = new System.Windows.Forms.CheckBox();
            this.FLIM_Radio2 = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Intensity_Radio1 = new System.Windows.Forms.RadioButton();
            this.FLIM_Radio1 = new System.Windows.Forms.RadioButton();
            this.Acquisition1 = new System.Windows.Forms.CheckBox();
            this.StatusText = new System.Windows.Forms.Label();
            this.MotorStatus = new System.Windows.Forms.Label();
            this.PiezoZ = new System.Windows.Forms.TextBox();
            this.PiezoZUnitLabel = new System.Windows.Forms.Label();
            this.PiezoZLabel = new System.Windows.Forms.Label();
            this.CenterPiezoButton = new System.Windows.Forms.Button();
            tabPage2 = new System.Windows.Forms.TabPage();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PowerSlider2)).BeginInit();
            this.PowerBoxImageParameters.SuspendLayout();
            this.XY_panel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.stagePanel.SuspendLayout();
            this.groupBox11.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.LaserPanel.SuspendLayout();
            this.laserPowerPanel.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PowerSlider1)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PowerSlider3)).BeginInit();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PowerSlider4)).BeginInit();
            this.acquisitionPanel.SuspendLayout();
            this.Panel_Files.SuspendLayout();
            this.ImageIteration.SuspendLayout();
            this.tb_Pparameters.SuspendLayout();
            this.sync2Group.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tbScanParam.SuspendLayout();
            this.tb_ScanParameters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Zoom10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScanPosition)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Zoom100)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ZoomP1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Zoom1)).BeginInit();
            this.FLIMSetting_tab.SuspendLayout();
            this.ChannelSettingTab.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage2
            // 
            tabPage2.BackColor = System.Drawing.SystemColors.Control;
            tabPage2.Controls.Add(this.powerRead2);
            tabPage2.Controls.Add(this.needCalib2);
            tabPage2.Controls.Add(this.UncageLaser2);
            tabPage2.Controls.Add(this.ImageLaser2);
            tabPage2.Controls.Add(this.label19);
            tabPage2.Controls.Add(this.PowerSlider2);
            tabPage2.Controls.Add(this.Power2);
            tabPage2.Location = new System.Drawing.Point(4, 23);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new System.Windows.Forms.Padding(3);
            tabPage2.Size = new System.Drawing.Size(258, 76);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Laser 2";
            // 
            // powerRead2
            // 
            this.powerRead2.AutoSize = true;
            this.powerRead2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.powerRead2.Location = new System.Drawing.Point(3, 61);
            this.powerRead2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.powerRead2.Name = "powerRead2";
            this.powerRead2.Size = new System.Drawing.Size(36, 13);
            this.powerRead2.TabIndex = 126;
            this.powerRead2.Text = "X mW";
            // 
            // needCalib2
            // 
            this.needCalib2.ForeColor = System.Drawing.Color.Red;
            this.needCalib2.Location = new System.Drawing.Point(110, 8);
            this.needCalib2.Name = "needCalib2";
            this.needCalib2.Size = new System.Drawing.Size(125, 30);
            this.needCalib2.TabIndex = 125;
            this.needCalib2.Text = "Need Calibration";
            this.needCalib2.UseVisualStyleBackColor = true;
            // 
            // UncageLaser2
            // 
            this.UncageLaser2.AutoSize = true;
            this.UncageLaser2.Location = new System.Drawing.Point(160, 54);
            this.UncageLaser2.Name = "UncageLaser2";
            this.UncageLaser2.Size = new System.Drawing.Size(71, 18);
            this.UncageLaser2.TabIndex = 117;
            this.UncageLaser2.Text = "Uncaging";
            this.UncageLaser2.UseVisualStyleBackColor = true;
            this.UncageLaser2.Click += new System.EventHandler(this.ImageLaser1_CheckedChanged);
            // 
            // ImageLaser2
            // 
            this.ImageLaser2.AutoSize = true;
            this.ImageLaser2.Location = new System.Drawing.Point(90, 54);
            this.ImageLaser2.Name = "ImageLaser2";
            this.ImageLaser2.Size = new System.Drawing.Size(62, 18);
            this.ImageLaser2.TabIndex = 116;
            this.ImageLaser2.Text = "Imaging";
            this.ImageLaser2.UseVisualStyleBackColor = true;
            this.ImageLaser2.Click += new System.EventHandler(this.ImageLaser1_CheckedChanged);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(14, 10);
            this.label19.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(68, 16);
            this.label19.TabIndex = 113;
            this.label19.Text = "Power (%)";
            // 
            // PowerSlider2
            // 
            this.PowerSlider2.AutoSize = false;
            this.PowerSlider2.BackColor = System.Drawing.SystemColors.Control;
            this.PowerSlider2.LargeChange = 10;
            this.PowerSlider2.Location = new System.Drawing.Point(5, 35);
            this.PowerSlider2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PowerSlider2.Maximum = 100;
            this.PowerSlider2.Name = "PowerSlider2";
            this.PowerSlider2.Size = new System.Drawing.Size(226, 27);
            this.PowerSlider2.TabIndex = 115;
            this.PowerSlider2.TickStyle = System.Windows.Forms.TickStyle.None;
            this.PowerSlider2.Value = 10;
            this.PowerSlider2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PowerSlider1_MouseUp);
            // 
            // Power2
            // 
            this.Power2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Power2.Location = new System.Drawing.Point(171, 8);
            this.Power2.Margin = new System.Windows.Forms.Padding(1);
            this.Power2.Name = "Power2";
            this.Power2.Size = new System.Drawing.Size(56, 26);
            this.Power2.TabIndex = 114;
            this.Power2.Text = "10";
            this.Power2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Power2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PowerText_change);
            // 
            // CurrentSlice
            // 
            this.CurrentSlice.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentSlice.Location = new System.Drawing.Point(59, 123);
            this.CurrentSlice.Margin = new System.Windows.Forms.Padding(1);
            this.CurrentSlice.Name = "CurrentSlice";
            this.CurrentSlice.ReadOnly = true;
            this.CurrentSlice.Size = new System.Drawing.Size(40, 20);
            this.CurrentSlice.TabIndex = 4;
            this.CurrentSlice.Text = "0";
            this.CurrentSlice.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // NSlices
            // 
            this.NSlices.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NSlices.Location = new System.Drawing.Point(105, 123);
            this.NSlices.Margin = new System.Windows.Forms.Padding(1);
            this.NSlices.Name = "NSlices";
            this.NSlices.Size = new System.Drawing.Size(40, 20);
            this.NSlices.TabIndex = 7;
            this.NSlices.Text = "1";
            this.NSlices.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NSlices.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NSlices_KeyDown);
            // 
            // NImages
            // 
            this.NImages.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NImages.Location = new System.Drawing.Point(108, 33);
            this.NImages.Margin = new System.Windows.Forms.Padding(1);
            this.NImages.Name = "NImages";
            this.NImages.Size = new System.Drawing.Size(40, 20);
            this.NImages.TabIndex = 9;
            this.NImages.Text = "1";
            this.NImages.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NImages.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // CurrentImage
            // 
            this.CurrentImage.Enabled = false;
            this.CurrentImage.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentImage.Location = new System.Drawing.Point(65, 33);
            this.CurrentImage.Margin = new System.Windows.Forms.Padding(1);
            this.CurrentImage.Name = "CurrentImage";
            this.CurrentImage.Size = new System.Drawing.Size(40, 20);
            this.CurrentImage.TabIndex = 8;
            this.CurrentImage.Text = "0";
            this.CurrentImage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // st_slices
            // 
            this.st_slices.AutoSize = true;
            this.st_slices.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_slices.Location = new System.Drawing.Point(7, 117);
            this.st_slices.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_slices.Name = "st_slices";
            this.st_slices.Size = new System.Drawing.Size(42, 15);
            this.st_slices.TabIndex = 12;
            this.st_slices.Text = "Slices";
            // 
            // st_NAcq
            // 
            this.st_NAcq.AutoSize = true;
            this.st_NAcq.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_NAcq.Location = new System.Drawing.Point(101, 14);
            this.st_NAcq.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_NAcq.Name = "st_NAcq";
            this.st_NAcq.Size = new System.Drawing.Size(49, 14);
            this.st_NAcq.TabIndex = 18;
            this.st_NAcq.Text = "#Frames";
            // 
            // st_NDone
            // 
            this.st_NDone.AutoSize = true;
            this.st_NDone.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_NDone.Location = new System.Drawing.Point(58, 14);
            this.st_NDone.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_NDone.Name = "st_NDone";
            this.st_NDone.Size = new System.Drawing.Size(43, 14);
            this.st_NDone.TabIndex = 19;
            this.st_NDone.Text = "Current";
            // 
            // ImageInterval
            // 
            this.ImageInterval.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ImageInterval.Location = new System.Drawing.Point(151, 33);
            this.ImageInterval.Margin = new System.Windows.Forms.Padding(1);
            this.ImageInterval.Name = "ImageInterval";
            this.ImageInterval.Size = new System.Drawing.Size(53, 20);
            this.ImageInterval.TabIndex = 20;
            this.ImageInterval.Text = "120.00";
            this.ImageInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ImageInterval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // st_interval
            // 
            this.st_interval.AutoSize = true;
            this.st_interval.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_interval.Location = new System.Drawing.Point(148, 14);
            this.st_interval.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_interval.Name = "st_interval";
            this.st_interval.Size = new System.Drawing.Size(59, 14);
            this.st_interval.TabIndex = 21;
            this.st_interval.Text = "Interval (s)";
            // 
            // SliceInterval
            // 
            this.SliceInterval.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SliceInterval.Location = new System.Drawing.Point(151, 123);
            this.SliceInterval.Margin = new System.Windows.Forms.Padding(1);
            this.SliceInterval.Name = "SliceInterval";
            this.SliceInterval.Size = new System.Drawing.Size(53, 20);
            this.SliceInterval.TabIndex = 22;
            this.SliceInterval.Text = "0";
            this.SliceInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.SliceInterval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // FrameInterval
            // 
            this.FrameInterval.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FrameInterval.Location = new System.Drawing.Point(151, 29);
            this.FrameInterval.Margin = new System.Windows.Forms.Padding(1);
            this.FrameInterval.Name = "FrameInterval";
            this.FrameInterval.ReadOnly = true;
            this.FrameInterval.Size = new System.Drawing.Size(53, 20);
            this.FrameInterval.TabIndex = 23;
            this.FrameInterval.Text = "0.256";
            this.FrameInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ETime
            // 
            this.ETime.AutoSize = true;
            this.ETime.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ETime.Location = new System.Drawing.Point(137, 72);
            this.ETime.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ETime.Name = "ETime";
            this.ETime.Size = new System.Drawing.Size(48, 18);
            this.ETime.TabIndex = 54;
            this.ETime.Text = "00.00";
            this.ETime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AveFrame_Check
            // 
            this.AveFrame_Check.AutoSize = true;
            this.AveFrame_Check.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AveFrame_Check.Location = new System.Drawing.Point(251, 54);
            this.AveFrame_Check.Name = "AveFrame_Check";
            this.AveFrame_Check.Size = new System.Drawing.Size(46, 18);
            this.AveFrame_Check.TabIndex = 101;
            this.AveFrame_Check.Text = "Ave";
            this.AveFrame_Check.UseVisualStyleBackColor = true;
            this.AveFrame_Check.Click += new System.EventHandler(this.AverageFrame_ValueChanged);
            // 
            // NumAve
            // 
            this.NumAve.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NumAve.Location = new System.Drawing.Point(207, 29);
            this.NumAve.Margin = new System.Windows.Forms.Padding(1);
            this.NumAve.Name = "NumAve";
            this.NumAve.Size = new System.Drawing.Size(40, 20);
            this.NumAve.TabIndex = 104;
            this.NumAve.Text = "4";
            this.NumAve.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumAve.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // CurrentFrame
            // 
            this.CurrentFrame.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentFrame.Location = new System.Drawing.Point(59, 29);
            this.CurrentFrame.Margin = new System.Windows.Forms.Padding(1);
            this.CurrentFrame.Name = "CurrentFrame";
            this.CurrentFrame.ReadOnly = true;
            this.CurrentFrame.Size = new System.Drawing.Size(40, 20);
            this.CurrentFrame.TabIndex = 105;
            this.CurrentFrame.Text = "0";
            this.CurrentFrame.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // PowerBoxImageParameters
            // 
            this.PowerBoxImageParameters.Controls.Add(this.N_AveragedFrames2);
            this.PowerBoxImageParameters.Controls.Add(this.N_AveragedFrames1);
            this.PowerBoxImageParameters.Controls.Add(this.N_AveragedSlices);
            this.PowerBoxImageParameters.Controls.Add(this.NAveragedSliceLabel);
            this.PowerBoxImageParameters.Controls.Add(this.TotalNFrames2Label);
            this.PowerBoxImageParameters.Controls.Add(this.NSlices);
            this.PowerBoxImageParameters.Controls.Add(this.N_AveSlices);
            this.PowerBoxImageParameters.Controls.Add(this.SliceInterval);
            this.PowerBoxImageParameters.Controls.Add(this.CurrentSlice);
            this.PowerBoxImageParameters.Controls.Add(this.NAveSliceLabel);
            this.PowerBoxImageParameters.Controls.Add(this.SliceIntervalLabel);
            this.PowerBoxImageParameters.Controls.Add(this.label101);
            this.PowerBoxImageParameters.Controls.Add(this.label102);
            this.PowerBoxImageParameters.Controls.Add(this.AveFrame2_Check);
            this.PowerBoxImageParameters.Controls.Add(this.TotalNFramesLabel1);
            this.PowerBoxImageParameters.Controls.Add(this.aveSlice_Interval);
            this.PowerBoxImageParameters.Controls.Add(this.aveFrame_Interval);
            this.PowerBoxImageParameters.Controls.Add(this.label5);
            this.PowerBoxImageParameters.Controls.Add(this.label4);
            this.PowerBoxImageParameters.Controls.Add(this.NFrames);
            this.PowerBoxImageParameters.Controls.Add(this.AOCounter);
            this.PowerBoxImageParameters.Controls.Add(this.label91);
            this.PowerBoxImageParameters.Controls.Add(this.SavedFileN);
            this.PowerBoxImageParameters.Controls.Add(this.label16);
            this.PowerBoxImageParameters.Controls.Add(this.label78);
            this.PowerBoxImageParameters.Controls.Add(this.label66);
            this.PowerBoxImageParameters.Controls.Add(this.nAverageFrame);
            this.PowerBoxImageParameters.Controls.Add(this.AveSlices_check);
            this.PowerBoxImageParameters.Controls.Add(this.Measured_slice_interval);
            this.PowerBoxImageParameters.Controls.Add(this.label3);
            this.PowerBoxImageParameters.Controls.Add(this.label60);
            this.PowerBoxImageParameters.Controls.Add(this.CurrentFrame);
            this.PowerBoxImageParameters.Controls.Add(this.NumAve);
            this.PowerBoxImageParameters.Controls.Add(this.AveFrame_Check);
            this.PowerBoxImageParameters.Controls.Add(this.FrameInterval);
            this.PowerBoxImageParameters.Controls.Add(this.st_interval);
            this.PowerBoxImageParameters.Controls.Add(this.st_NDone);
            this.PowerBoxImageParameters.Controls.Add(this.st_NAcq);
            this.PowerBoxImageParameters.Controls.Add(this.st_slices);
            this.PowerBoxImageParameters.Controls.Add(this.Misc_about_Slice);
            this.PowerBoxImageParameters.Controls.Add(this.ZStack_radio);
            this.PowerBoxImageParameters.Controls.Add(this.Timelapse_radio);
            this.PowerBoxImageParameters.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PowerBoxImageParameters.Location = new System.Drawing.Point(10, 27);
            this.PowerBoxImageParameters.Name = "PowerBoxImageParameters";
            this.PowerBoxImageParameters.Size = new System.Drawing.Size(300, 205);
            this.PowerBoxImageParameters.TabIndex = 116;
            this.PowerBoxImageParameters.TabStop = false;
            this.PowerBoxImageParameters.Text = "Image Sequence";
            // 
            // N_AveragedFrames2
            // 
            this.N_AveragedFrames2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.N_AveragedFrames2.Location = new System.Drawing.Point(208, 74);
            this.N_AveragedFrames2.Margin = new System.Windows.Forms.Padding(1);
            this.N_AveragedFrames2.Name = "N_AveragedFrames2";
            this.N_AveragedFrames2.ReadOnly = true;
            this.N_AveragedFrames2.Size = new System.Drawing.Size(40, 20);
            this.N_AveragedFrames2.TabIndex = 348;
            this.N_AveragedFrames2.Text = "0";
            this.N_AveragedFrames2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // N_AveragedFrames1
            // 
            this.N_AveragedFrames1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.N_AveragedFrames1.Location = new System.Drawing.Point(208, 53);
            this.N_AveragedFrames1.Margin = new System.Windows.Forms.Padding(1);
            this.N_AveragedFrames1.Name = "N_AveragedFrames1";
            this.N_AveragedFrames1.ReadOnly = true;
            this.N_AveragedFrames1.Size = new System.Drawing.Size(40, 20);
            this.N_AveragedFrames1.TabIndex = 340;
            this.N_AveragedFrames1.Text = "0";
            this.N_AveragedFrames1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.N_AveragedFrames1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // N_AveragedSlices
            // 
            this.N_AveragedSlices.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.N_AveragedSlices.Location = new System.Drawing.Point(207, 147);
            this.N_AveragedSlices.Margin = new System.Windows.Forms.Padding(1);
            this.N_AveragedSlices.Name = "N_AveragedSlices";
            this.N_AveragedSlices.ReadOnly = true;
            this.N_AveragedSlices.Size = new System.Drawing.Size(40, 20);
            this.N_AveragedSlices.TabIndex = 342;
            this.N_AveragedSlices.Text = "1";
            this.N_AveragedSlices.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.N_AveragedSlices.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // NAveragedSliceLabel
            // 
            this.NAveragedSliceLabel.AutoSize = true;
            this.NAveragedSliceLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NAveragedSliceLabel.Location = new System.Drawing.Point(137, 150);
            this.NAveragedSliceLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.NAveragedSliceLabel.Name = "NAveragedSliceLabel";
            this.NAveragedSliceLabel.Size = new System.Drawing.Size(70, 14);
            this.NAveragedSliceLabel.TabIndex = 357;
            this.NAveragedSliceLabel.Text = "Total N slices";
            // 
            // TotalNFrames2Label
            // 
            this.TotalNFrames2Label.AutoSize = true;
            this.TotalNFrames2Label.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TotalNFrames2Label.Location = new System.Drawing.Point(103, 77);
            this.TotalNFrames2Label.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.TotalNFrames2Label.Name = "TotalNFrames2Label";
            this.TotalNFrames2Label.Size = new System.Drawing.Size(106, 14);
            this.TotalNFrames2Label.TabIndex = 356;
            this.TotalNFrames2Label.Text = "Total N frames (Ch2)";
            // 
            // N_AveSlices
            // 
            this.N_AveSlices.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.N_AveSlices.Location = new System.Drawing.Point(207, 123);
            this.N_AveSlices.Margin = new System.Windows.Forms.Padding(1);
            this.N_AveSlices.Name = "N_AveSlices";
            this.N_AveSlices.Size = new System.Drawing.Size(40, 20);
            this.N_AveSlices.TabIndex = 324;
            this.N_AveSlices.Text = "4";
            this.N_AveSlices.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.N_AveSlices.Visible = false;
            // 
            // NAveSliceLabel
            // 
            this.NAveSliceLabel.AutoSize = true;
            this.NAveSliceLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NAveSliceLabel.Location = new System.Drawing.Point(205, 110);
            this.NAveSliceLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.NAveSliceLabel.Name = "NAveSliceLabel";
            this.NAveSliceLabel.Size = new System.Drawing.Size(55, 14);
            this.NAveSliceLabel.TabIndex = 355;
            this.NAveSliceLabel.Text = "#Average";
            // 
            // SliceIntervalLabel
            // 
            this.SliceIntervalLabel.AutoSize = true;
            this.SliceIntervalLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SliceIntervalLabel.Location = new System.Drawing.Point(148, 110);
            this.SliceIntervalLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.SliceIntervalLabel.Name = "SliceIntervalLabel";
            this.SliceIntervalLabel.Size = new System.Drawing.Size(59, 14);
            this.SliceIntervalLabel.TabIndex = 354;
            this.SliceIntervalLabel.Text = "Interval (s)";
            // 
            // label101
            // 
            this.label101.AutoSize = true;
            this.label101.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label101.Location = new System.Drawing.Point(58, 110);
            this.label101.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label101.Name = "label101";
            this.label101.Size = new System.Drawing.Size(43, 14);
            this.label101.TabIndex = 353;
            this.label101.Text = "Current";
            // 
            // label102
            // 
            this.label102.AutoSize = true;
            this.label102.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label102.Location = new System.Drawing.Point(104, 110);
            this.label102.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label102.Name = "label102";
            this.label102.Size = new System.Drawing.Size(42, 14);
            this.label102.TabIndex = 352;
            this.label102.Text = "#Slices";
            // 
            // AveFrame2_Check
            // 
            this.AveFrame2_Check.AutoSize = true;
            this.AveFrame2_Check.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AveFrame2_Check.Location = new System.Drawing.Point(251, 75);
            this.AveFrame2_Check.Name = "AveFrame2_Check";
            this.AveFrame2_Check.Size = new System.Drawing.Size(46, 18);
            this.AveFrame2_Check.TabIndex = 351;
            this.AveFrame2_Check.Text = "Ave";
            this.AveFrame2_Check.UseVisualStyleBackColor = true;
            this.AveFrame2_Check.Click += new System.EventHandler(this.AverageFrame_ValueChanged);
            // 
            // TotalNFramesLabel1
            // 
            this.TotalNFramesLabel1.AutoSize = true;
            this.TotalNFramesLabel1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TotalNFramesLabel1.Location = new System.Drawing.Point(103, 56);
            this.TotalNFramesLabel1.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.TotalNFramesLabel1.Name = "TotalNFramesLabel1";
            this.TotalNFramesLabel1.Size = new System.Drawing.Size(106, 14);
            this.TotalNFramesLabel1.TabIndex = 349;
            this.TotalNFramesLabel1.Text = "Total N frames (Ch1)";
            this.TotalNFramesLabel1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // aveSlice_Interval
            // 
            this.aveSlice_Interval.AutoSize = true;
            this.aveSlice_Interval.Font = new System.Drawing.Font("Arial", 8.25F);
            this.aveSlice_Interval.Location = new System.Drawing.Point(72, 147);
            this.aveSlice_Interval.Name = "aveSlice_Interval";
            this.aveSlice_Interval.Size = new System.Drawing.Size(13, 14);
            this.aveSlice_Interval.TabIndex = 346;
            this.aveSlice_Interval.Text = "0";
            // 
            // aveFrame_Interval
            // 
            this.aveFrame_Interval.AutoSize = true;
            this.aveFrame_Interval.Font = new System.Drawing.Font("Arial", 8.25F);
            this.aveFrame_Interval.Location = new System.Drawing.Point(77, 94);
            this.aveFrame_Interval.Name = "aveFrame_Interval";
            this.aveFrame_Interval.Size = new System.Drawing.Size(13, 14);
            this.aveFrame_Interval.TabIndex = 344;
            this.aveFrame_Interval.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(3, 147);
            this.label5.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 14);
            this.label5.TabIndex = 347;
            this.label5.Text = "Time per ave:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(7, 94);
            this.label4.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 14);
            this.label4.TabIndex = 345;
            this.label4.Text = "Time per ave:";
            // 
            // NFrames
            // 
            this.NFrames.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NFrames.Location = new System.Drawing.Point(105, 29);
            this.NFrames.Margin = new System.Windows.Forms.Padding(1);
            this.NFrames.Name = "NFrames";
            this.NFrames.Size = new System.Drawing.Size(40, 20);
            this.NFrames.TabIndex = 343;
            this.NFrames.Text = "1";
            this.NFrames.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NFrames.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // AOCounter
            // 
            this.AOCounter.AutoSize = true;
            this.AOCounter.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AOCounter.Location = new System.Drawing.Point(59, 49);
            this.AOCounter.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.AOCounter.Name = "AOCounter";
            this.AOCounter.Size = new System.Drawing.Size(13, 14);
            this.AOCounter.TabIndex = 338;
            this.AOCounter.Text = "0";
            // 
            // label91
            // 
            this.label91.AutoSize = true;
            this.label91.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label91.Location = new System.Drawing.Point(5, 49);
            this.label91.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label91.Name = "label91";
            this.label91.Size = new System.Drawing.Size(55, 14);
            this.label91.TabIndex = 337;
            this.label91.Text = "Scanning:";
            // 
            // SavedFileN
            // 
            this.SavedFileN.AutoSize = true;
            this.SavedFileN.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SavedFileN.Location = new System.Drawing.Point(59, 63);
            this.SavedFileN.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.SavedFileN.Name = "SavedFileN";
            this.SavedFileN.Size = new System.Drawing.Size(13, 14);
            this.SavedFileN.TabIndex = 335;
            this.SavedFileN.Text = "0";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(18, 63);
            this.label16.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(41, 14);
            this.label16.TabIndex = 334;
            this.label16.Text = "Saved:";
            // 
            // label78
            // 
            this.label78.AutoSize = true;
            this.label78.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label78.Location = new System.Drawing.Point(3, 23);
            this.label78.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label78.Name = "label78";
            this.label78.Size = new System.Drawing.Size(50, 15);
            this.label78.TabIndex = 333;
            this.label78.Text = "Frames";
            // 
            // label66
            // 
            this.label66.AutoSize = true;
            this.label66.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label66.Location = new System.Drawing.Point(32, 77);
            this.label66.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label66.Name = "label66";
            this.label66.Size = new System.Drawing.Size(30, 14);
            this.label66.TabIndex = 331;
            this.label66.Text = "Ave:";
            // 
            // nAverageFrame
            // 
            this.nAverageFrame.AutoSize = true;
            this.nAverageFrame.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nAverageFrame.Location = new System.Drawing.Point(59, 77);
            this.nAverageFrame.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.nAverageFrame.Name = "nAverageFrame";
            this.nAverageFrame.Size = new System.Drawing.Size(13, 14);
            this.nAverageFrame.TabIndex = 330;
            this.nAverageFrame.Text = "0";
            // 
            // AveSlices_check
            // 
            this.AveSlices_check.AutoSize = true;
            this.AveSlices_check.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AveSlices_check.Location = new System.Drawing.Point(251, 147);
            this.AveSlices_check.Name = "AveSlices_check";
            this.AveSlices_check.Size = new System.Drawing.Size(46, 18);
            this.AveSlices_check.TabIndex = 115;
            this.AveSlices_check.Text = "Ave";
            this.AveSlices_check.UseVisualStyleBackColor = true;
            this.AveSlices_check.Visible = false;
            this.AveSlices_check.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // Measured_slice_interval
            // 
            this.Measured_slice_interval.AutoSize = true;
            this.Measured_slice_interval.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Measured_slice_interval.Location = new System.Drawing.Point(211, 170);
            this.Measured_slice_interval.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Measured_slice_interval.Name = "Measured_slice_interval";
            this.Measured_slice_interval.Size = new System.Drawing.Size(37, 14);
            this.Measured_slice_interval.TabIndex = 117;
            this.Measured_slice_interval.Text = "0.00 s";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(131, 170);
            this.label3.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 14);
            this.label3.TabIndex = 116;
            this.label3.Text = "Actual interval:";
            // 
            // label60
            // 
            this.label60.AutoSize = true;
            this.label60.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label60.Location = new System.Drawing.Point(205, 14);
            this.label60.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(55, 14);
            this.label60.TabIndex = 111;
            this.label60.Text = "#Average";
            // 
            // Misc_about_Slice
            // 
            this.Misc_about_Slice.AutoSize = true;
            this.Misc_about_Slice.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Misc_about_Slice.Location = new System.Drawing.Point(131, 185);
            this.Misc_about_Slice.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Misc_about_Slice.Name = "Misc_about_Slice";
            this.Misc_about_Slice.Size = new System.Drawing.Size(29, 14);
            this.Misc_about_Slice.TabIndex = 325;
            this.Misc_about_Slice.Text = "Misc";
            // 
            // ZStack_radio
            // 
            this.ZStack_radio.AutoSize = true;
            this.ZStack_radio.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZStack_radio.Location = new System.Drawing.Point(25, 176);
            this.ZStack_radio.Name = "ZStack_radio";
            this.ZStack_radio.Size = new System.Drawing.Size(63, 18);
            this.ZStack_radio.TabIndex = 329;
            this.ZStack_radio.TabStop = true;
            this.ZStack_radio.Text = "Z-Stack";
            this.ZStack_radio.UseVisualStyleBackColor = true;
            this.ZStack_radio.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // Timelapse_radio
            // 
            this.Timelapse_radio.AutoSize = true;
            this.Timelapse_radio.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Timelapse_radio.Location = new System.Drawing.Point(25, 160);
            this.Timelapse_radio.Name = "Timelapse_radio";
            this.Timelapse_radio.Size = new System.Drawing.Size(76, 18);
            this.Timelapse_radio.TabIndex = 328;
            this.Timelapse_radio.TabStop = true;
            this.Timelapse_radio.Text = "Time lapse";
            this.Timelapse_radio.UseVisualStyleBackColor = true;
            this.Timelapse_radio.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(150, 19);
            this.label2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 14);
            this.label2.TabIndex = 332;
            this.label2.Text = "Interval (s)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(66, 19);
            this.label7.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(43, 14);
            this.label7.TabIndex = 331;
            this.label7.Text = "Current";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(105, 19);
            this.label9.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(44, 14);
            this.label9.TabIndex = 330;
            this.label9.Text = "Number";
            // 
            // ETime2
            // 
            this.ETime2.AutoSize = true;
            this.ETime2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ETime2.Location = new System.Drawing.Point(236, 16);
            this.ETime2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ETime2.Name = "ETime2";
            this.ETime2.Size = new System.Drawing.Size(40, 16);
            this.ETime2.TabIndex = 323;
            this.ETime2.Text = "00.00";
            this.ETime2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FocusButton
            // 
            this.FocusButton.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FocusButton.Location = new System.Drawing.Point(12, 102);
            this.FocusButton.Margin = new System.Windows.Forms.Padding(1);
            this.FocusButton.Name = "FocusButton";
            this.FocusButton.Size = new System.Drawing.Size(79, 26);
            this.FocusButton.TabIndex = 110;
            this.FocusButton.Text = "FOCUS";
            this.FocusButton.UseVisualStyleBackColor = true;
            this.FocusButton.Click += new System.EventHandler(this.FocusButton_Click);
            // 
            // GrabButton
            // 
            this.GrabButton.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GrabButton.Location = new System.Drawing.Point(108, 102);
            this.GrabButton.Margin = new System.Windows.Forms.Padding(1);
            this.GrabButton.Name = "GrabButton";
            this.GrabButton.Size = new System.Drawing.Size(80, 26);
            this.GrabButton.TabIndex = 109;
            this.GrabButton.Tag = "GrabButton";
            this.GrabButton.Text = "GRAB";
            this.GrabButton.UseVisualStyleBackColor = true;
            this.GrabButton.Click += new System.EventHandler(this.GrabButtonClick);
            // 
            // XY_panel
            // 
            this.XY_panel.Controls.Add(this.XYMotorStep);
            this.XY_panel.Controls.Add(this.label43);
            this.XY_panel.Controls.Add(this.XUp);
            this.XY_panel.Controls.Add(this.XDown);
            this.XY_panel.Controls.Add(this.YDown);
            this.XY_panel.Controls.Add(this.YUp);
            this.XY_panel.Controls.Add(this.label44);
            this.XY_panel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XY_panel.Location = new System.Drawing.Point(146, 9);
            this.XY_panel.Name = "XY_panel";
            this.XY_panel.Size = new System.Drawing.Size(82, 119);
            this.XY_panel.TabIndex = 266;
            this.XY_panel.TabStop = false;
            this.XY_panel.Text = "Step XY";
            // 
            // XYMotorStep
            // 
            this.XYMotorStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.XYMotorStep.Location = new System.Drawing.Point(15, 29);
            this.XYMotorStep.Margin = new System.Windows.Forms.Padding(1);
            this.XYMotorStep.Name = "XYMotorStep";
            this.XYMotorStep.Size = new System.Drawing.Size(30, 20);
            this.XYMotorStep.TabIndex = 135;
            this.XYMotorStep.Text = "1";
            this.XYMotorStep.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.XYMotorStep.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MotorTextChanged);
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label43.Location = new System.Drawing.Point(16, 15);
            this.label43.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(29, 13);
            this.label43.TabIndex = 141;
            this.label43.Text = "Step";
            // 
            // XUp
            // 
            this.XUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.XUp.Location = new System.Drawing.Point(54, 71);
            this.XUp.Name = "XUp";
            this.XUp.Size = new System.Drawing.Size(25, 23);
            this.XUp.TabIndex = 140;
            this.XUp.Text = "→";
            this.XUp.UseVisualStyleBackColor = true;
            this.XUp.Click += new System.EventHandler(this.MotroStepMovementXYZ);
            // 
            // XDown
            // 
            this.XDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.XDown.Location = new System.Drawing.Point(4, 71);
            this.XDown.Name = "XDown";
            this.XDown.Size = new System.Drawing.Size(25, 23);
            this.XDown.TabIndex = 139;
            this.XDown.Text = "←";
            this.XDown.UseVisualStyleBackColor = true;
            this.XDown.Click += new System.EventHandler(this.MotroStepMovementXYZ);
            // 
            // YDown
            // 
            this.YDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.YDown.Location = new System.Drawing.Point(29, 89);
            this.YDown.Name = "YDown";
            this.YDown.Size = new System.Drawing.Size(25, 23);
            this.YDown.TabIndex = 138;
            this.YDown.Text = "↓";
            this.YDown.UseVisualStyleBackColor = true;
            this.YDown.Click += new System.EventHandler(this.MotroStepMovementXYZ);
            // 
            // YUp
            // 
            this.YUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.YUp.Location = new System.Drawing.Point(29, 56);
            this.YUp.Name = "YUp";
            this.YUp.Size = new System.Drawing.Size(25, 23);
            this.YUp.TabIndex = 137;
            this.YUp.Text = "↑";
            this.YUp.UseVisualStyleBackColor = true;
            this.YUp.Click += new System.EventHandler(this.MotroStepMovementXYZ);
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label44.Location = new System.Drawing.Point(45, 35);
            this.label44.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(21, 13);
            this.label44.TabIndex = 136;
            this.label44.Text = "μm";
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label45.Location = new System.Drawing.Point(198, 9);
            this.label45.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(0, 13);
            this.label45.TabIndex = 265;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.BackColor = System.Drawing.Color.Transparent;
            this.label46.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label46.Location = new System.Drawing.Point(1, 43);
            this.label46.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(14, 14);
            this.label46.TabIndex = 264;
            this.label46.Text = "X";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.BackColor = System.Drawing.Color.Transparent;
            this.label47.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label47.Location = new System.Drawing.Point(1, 65);
            this.label47.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(15, 14);
            this.label47.TabIndex = 263;
            this.label47.Text = "Y";
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.BackColor = System.Drawing.Color.Transparent;
            this.label48.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label48.Location = new System.Drawing.Point(1, 86);
            this.label48.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(14, 14);
            this.label48.TabIndex = 262;
            this.label48.Text = "Z";
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label49.Location = new System.Drawing.Point(64, 43);
            this.label49.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(21, 14);
            this.label49.TabIndex = 261;
            this.label49.Text = "μm";
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label50.Location = new System.Drawing.Point(64, 65);
            this.label50.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(21, 14);
            this.label50.TabIndex = 260;
            this.label50.Text = "μm";
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label51.Location = new System.Drawing.Point(64, 86);
            this.label51.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(21, 14);
            this.label51.TabIndex = 254;
            this.label51.Text = "μm";
            // 
            // Zero_all
            // 
            this.Zero_all.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Zero_all.Location = new System.Drawing.Point(86, 58);
            this.Zero_all.Margin = new System.Windows.Forms.Padding(1);
            this.Zero_all.Name = "Zero_all";
            this.Zero_all.Size = new System.Drawing.Size(51, 20);
            this.Zero_all.TabIndex = 258;
            this.Zero_all.Text = "Zero all";
            this.Zero_all.UseVisualStyleBackColor = true;
            this.Zero_all.Click += new System.EventHandler(this.Zero_all_Click);
            // 
            // ZRead
            // 
            this.ZRead.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZRead.Location = new System.Drawing.Point(15, 82);
            this.ZRead.Margin = new System.Windows.Forms.Padding(1);
            this.ZRead.Name = "ZRead";
            this.ZRead.Size = new System.Drawing.Size(50, 20);
            this.ZRead.TabIndex = 255;
            this.ZRead.Text = "0";
            this.ZRead.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // YRead
            // 
            this.YRead.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.YRead.Location = new System.Drawing.Point(15, 60);
            this.YRead.Margin = new System.Windows.Forms.Padding(1);
            this.YRead.Name = "YRead";
            this.YRead.Size = new System.Drawing.Size(50, 20);
            this.YRead.TabIndex = 257;
            this.YRead.Text = "0";
            this.YRead.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // Set_bottom
            // 
            this.Set_bottom.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Set_bottom.Location = new System.Drawing.Point(95, 57);
            this.Set_bottom.Margin = new System.Windows.Forms.Padding(1);
            this.Set_bottom.Name = "Set_bottom";
            this.Set_bottom.Size = new System.Drawing.Size(38, 20);
            this.Set_bottom.TabIndex = 253;
            this.Set_bottom.Text = "Set";
            this.Set_bottom.UseVisualStyleBackColor = true;
            this.Set_bottom.Click += new System.EventHandler(this.Set_bottom_Click);
            // 
            // XRead
            // 
            this.XRead.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XRead.Location = new System.Drawing.Point(15, 38);
            this.XRead.Margin = new System.Windows.Forms.Padding(1);
            this.XRead.Name = "XRead";
            this.XRead.Size = new System.Drawing.Size(50, 20);
            this.XRead.TabIndex = 256;
            this.XRead.Text = "0";
            this.XRead.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // Set_Top
            // 
            this.Set_Top.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Set_Top.Location = new System.Drawing.Point(95, 15);
            this.Set_Top.Margin = new System.Windows.Forms.Padding(1);
            this.Set_Top.Name = "Set_Top";
            this.Set_Top.Size = new System.Drawing.Size(38, 20);
            this.Set_Top.TabIndex = 252;
            this.Set_Top.Text = "Set";
            this.Set_Top.UseVisualStyleBackColor = true;
            this.Set_Top.Click += new System.EventHandler(this.Set_Top_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ZMotorStep);
            this.groupBox1.Controls.Add(this.label18);
            this.groupBox1.Controls.Add(this.Zdown);
            this.groupBox1.Controls.Add(this.label53);
            this.groupBox1.Controls.Add(this.Zup);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(231, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(63, 119);
            this.groupBox1.TabIndex = 271;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Step Z";
            // 
            // ZMotorStep
            // 
            this.ZMotorStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.ZMotorStep.Location = new System.Drawing.Point(12, 29);
            this.ZMotorStep.Margin = new System.Windows.Forms.Padding(1);
            this.ZMotorStep.Name = "ZMotorStep";
            this.ZMotorStep.Size = new System.Drawing.Size(31, 20);
            this.ZMotorStep.TabIndex = 142;
            this.ZMotorStep.Text = "1";
            this.ZMotorStep.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ZMotorStep.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MotorTextChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(12, 15);
            this.label18.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(29, 13);
            this.label18.TabIndex = 148;
            this.label18.Text = "Step";
            // 
            // Zdown
            // 
            this.Zdown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.Zdown.Location = new System.Drawing.Point(15, 85);
            this.Zdown.Name = "Zdown";
            this.Zdown.Size = new System.Drawing.Size(31, 23);
            this.Zdown.TabIndex = 147;
            this.Zdown.Text = "↓";
            this.Zdown.UseVisualStyleBackColor = true;
            this.Zdown.Click += new System.EventHandler(this.MotroStepMovementXYZ);
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label53.Location = new System.Drawing.Point(42, 35);
            this.label53.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(21, 13);
            this.label53.TabIndex = 143;
            this.label53.Text = "μm";
            // 
            // Zup
            // 
            this.Zup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.Zup.Location = new System.Drawing.Point(15, 62);
            this.Zup.Name = "Zup";
            this.Zup.Size = new System.Drawing.Size(31, 23);
            this.Zup.TabIndex = 146;
            this.Zup.Text = "↑";
            this.Zup.UseVisualStyleBackColor = true;
            this.Zup.Click += new System.EventHandler(this.MotroStepMovementXYZ);
            // 
            // zero_Z
            // 
            this.zero_Z.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.zero_Z.Location = new System.Drawing.Point(86, 79);
            this.zero_Z.Margin = new System.Windows.Forms.Padding(1);
            this.zero_Z.Name = "zero_Z";
            this.zero_Z.Size = new System.Drawing.Size(51, 20);
            this.zero_Z.TabIndex = 260;
            this.zero_Z.Text = "Zero Z";
            this.zero_Z.UseVisualStyleBackColor = true;
            this.zero_Z.Click += new System.EventHandler(this.Zero_Z_Click);
            // 
            // stagePanel
            // 
            this.stagePanel.BackColor = System.Drawing.SystemColors.Control;
            this.stagePanel.Controls.Add(this.CenterPiezoButton);
            this.stagePanel.Controls.Add(this.ResetMotor);
            this.stagePanel.Controls.Add(this.PiezoZLabel);
            this.stagePanel.Controls.Add(this.PiezoZUnitLabel);
            this.stagePanel.Controls.Add(this.PiezoZ);
            this.stagePanel.Controls.Add(this.YRead);
            this.stagePanel.Controls.Add(this.MotorReadButton);
            this.stagePanel.Controls.Add(this.Motor_Status);
            this.stagePanel.Controls.Add(this.Relative);
            this.stagePanel.Controls.Add(this.groupBox1);
            this.stagePanel.Controls.Add(this.groupBox11);
            this.stagePanel.Controls.Add(this.zero_Z);
            this.stagePanel.Controls.Add(this.Zero_all);
            this.stagePanel.Controls.Add(this.ZRead);
            this.stagePanel.Controls.Add(this.XY_panel);
            this.stagePanel.Controls.Add(this.label45);
            this.stagePanel.Controls.Add(this.label46);
            this.stagePanel.Controls.Add(this.label47);
            this.stagePanel.Controls.Add(this.label48);
            this.stagePanel.Controls.Add(this.label51);
            this.stagePanel.Controls.Add(this.XRead);
            this.stagePanel.Controls.Add(this.label49);
            this.stagePanel.Controls.Add(this.label50);
            this.stagePanel.Controls.Add(this.ContRead);
            this.stagePanel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stagePanel.Location = new System.Drawing.Point(10, 403);
            this.stagePanel.Name = "stagePanel";
            this.stagePanel.Size = new System.Drawing.Size(300, 180);
            this.stagePanel.TabIndex = 272;
            this.stagePanel.TabStop = false;
            this.stagePanel.Text = "Stage";
            // 
            // ResetMotor
            // 
            this.ResetMotor.ForeColor = System.Drawing.Color.Red;
            this.ResetMotor.Location = new System.Drawing.Point(20, 46);
            this.ResetMotor.Name = "ResetMotor";
            this.ResetMotor.Size = new System.Drawing.Size(275, 42);
            this.ResetMotor.TabIndex = 126;
            this.ResetMotor.Text = "Reset motor and press this!";
            this.ResetMotor.UseVisualStyleBackColor = true;
            this.ResetMotor.Visible = false;
            this.ResetMotor.Click += new System.EventHandler(this.ResetMotor_Click);
            // 
            // MotorReadButton
            // 
            this.MotorReadButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MotorReadButton.Location = new System.Drawing.Point(15, 16);
            this.MotorReadButton.Name = "MotorReadButton";
            this.MotorReadButton.Size = new System.Drawing.Size(50, 20);
            this.MotorReadButton.TabIndex = 284;
            this.MotorReadButton.Text = "Read";
            this.MotorReadButton.UseVisualStyleBackColor = true;
            this.MotorReadButton.Click += new System.EventHandler(this.MotorReadButton_Click);
            // 
            // Motor_Status
            // 
            this.Motor_Status.AutoSize = true;
            this.Motor_Status.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Motor_Status.Location = new System.Drawing.Point(199, 158);
            this.Motor_Status.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Motor_Status.Name = "Motor_Status";
            this.Motor_Status.Size = new System.Drawing.Size(16, 13);
            this.Motor_Status.TabIndex = 279;
            this.Motor_Status.Text = "...";
            // 
            // Relative
            // 
            this.Relative.AutoSize = true;
            this.Relative.Checked = true;
            this.Relative.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Relative.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Relative.Location = new System.Drawing.Point(74, 9);
            this.Relative.Name = "Relative";
            this.Relative.Size = new System.Drawing.Size(64, 18);
            this.Relative.TabIndex = 285;
            this.Relative.Text = "Relative";
            this.Relative.UseVisualStyleBackColor = true;
            this.Relative.Click += new System.EventHandler(this.Relative_Click);
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.Velocity);
            this.groupBox11.Controls.Add(this.Vel_Down);
            this.groupBox11.Controls.Add(this.Vel_Up);
            this.groupBox11.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox11.Location = new System.Drawing.Point(71, 124);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(120, 51);
            this.groupBox11.TabIndex = 281;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Velocity (Motor)";
            // 
            // Velocity
            // 
            this.Velocity.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Velocity.Location = new System.Drawing.Point(8, 20);
            this.Velocity.Margin = new System.Windows.Forms.Padding(1);
            this.Velocity.Name = "Velocity";
            this.Velocity.Size = new System.Drawing.Size(50, 20);
            this.Velocity.TabIndex = 283;
            this.Velocity.Text = "0";
            this.Velocity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.Velocity.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Velocity_KeyDown);
            // 
            // Vel_Down
            // 
            this.Vel_Down.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.Vel_Down.Location = new System.Drawing.Point(89, 18);
            this.Vel_Down.Name = "Vel_Down";
            this.Vel_Down.Size = new System.Drawing.Size(25, 23);
            this.Vel_Down.TabIndex = 278;
            this.Vel_Down.Text = "↓";
            this.Vel_Down.UseVisualStyleBackColor = true;
            this.Vel_Down.Click += new System.EventHandler(this.Vel_Up_Click);
            // 
            // Vel_Up
            // 
            this.Vel_Up.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.Vel_Up.Location = new System.Drawing.Point(62, 18);
            this.Vel_Up.Name = "Vel_Up";
            this.Vel_Up.Size = new System.Drawing.Size(25, 23);
            this.Vel_Up.TabIndex = 277;
            this.Vel_Up.Text = "↑";
            this.Vel_Up.UseVisualStyleBackColor = true;
            this.Vel_Up.Click += new System.EventHandler(this.Vel_Up_Click);
            // 
            // ContRead
            // 
            this.ContRead.AutoSize = true;
            this.ContRead.Checked = true;
            this.ContRead.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ContRead.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ContRead.Location = new System.Drawing.Point(74, 27);
            this.ContRead.Name = "ContRead";
            this.ContRead.Size = new System.Drawing.Size(51, 18);
            this.ContRead.TabIndex = 284;
            this.ContRead.Text = "Cont.";
            this.ContRead.UseVisualStyleBackColor = true;
            this.ContRead.CheckedChanged += new System.EventHandler(this.ContRead_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.StayMotorRadio);
            this.groupBox3.Controls.Add(this.BackToCenterRadio);
            this.groupBox3.Controls.Add(this.BackToStartRadio);
            this.groupBox3.Controls.Add(this.ZCenter);
            this.groupBox3.Controls.Add(this.GoCenter);
            this.groupBox3.Controls.Add(this.CenterLabel);
            this.groupBox3.Controls.Add(this.Set_Center);
            this.groupBox3.Controls.Add(this.GoEnd);
            this.groupBox3.Controls.Add(this.GoStart);
            this.groupBox3.Controls.Add(this.label77);
            this.groupBox3.Controls.Add(this.label72);
            this.groupBox3.Controls.Add(this.NSlices2);
            this.groupBox3.Controls.Add(this.label42);
            this.groupBox3.Controls.Add(this.ZEnd);
            this.groupBox3.Controls.Add(this.SliceStep);
            this.groupBox3.Controls.Add(this.ZStart);
            this.groupBox3.Controls.Add(this.st_step);
            this.groupBox3.Controls.Add(this.st_um);
            this.groupBox3.Controls.Add(this.Set_bottom);
            this.groupBox3.Controls.Add(this.Set_Top);
            this.groupBox3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(10, 300);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(300, 103);
            this.groupBox3.TabIndex = 274;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Z stack";
            // 
            // StayMotorRadio
            // 
            this.StayMotorRadio.AutoSize = true;
            this.StayMotorRadio.Font = new System.Drawing.Font("Arial", 8.25F);
            this.StayMotorRadio.Location = new System.Drawing.Point(201, 80);
            this.StayMotorRadio.Name = "StayMotorRadio";
            this.StayMotorRadio.Size = new System.Drawing.Size(47, 18);
            this.StayMotorRadio.TabIndex = 341;
            this.StayMotorRadio.Text = "Stay";
            this.StayMotorRadio.UseVisualStyleBackColor = true;
            // 
            // BackToCenterRadio
            // 
            this.BackToCenterRadio.AutoSize = true;
            this.BackToCenterRadio.Checked = true;
            this.BackToCenterRadio.Font = new System.Drawing.Font("Arial", 8.25F);
            this.BackToCenterRadio.Location = new System.Drawing.Point(8, 80);
            this.BackToCenterRadio.Name = "BackToCenterRadio";
            this.BackToCenterRadio.Size = new System.Drawing.Size(96, 18);
            this.BackToCenterRadio.TabIndex = 340;
            this.BackToCenterRadio.TabStop = true;
            this.BackToCenterRadio.Text = "Back to Center";
            this.BackToCenterRadio.UseVisualStyleBackColor = true;
            // 
            // BackToStartRadio
            // 
            this.BackToStartRadio.AutoSize = true;
            this.BackToStartRadio.Font = new System.Drawing.Font("Arial", 8.25F);
            this.BackToStartRadio.Location = new System.Drawing.Point(106, 80);
            this.BackToStartRadio.Name = "BackToStartRadio";
            this.BackToStartRadio.Size = new System.Drawing.Size(89, 18);
            this.BackToStartRadio.TabIndex = 339;
            this.BackToStartRadio.Text = "Back To Start";
            this.BackToStartRadio.UseVisualStyleBackColor = true;
            // 
            // ZCenter
            // 
            this.ZCenter.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZCenter.Location = new System.Drawing.Point(41, 35);
            this.ZCenter.Margin = new System.Windows.Forms.Padding(1);
            this.ZCenter.Name = "ZCenter";
            this.ZCenter.Size = new System.Drawing.Size(50, 20);
            this.ZCenter.TabIndex = 336;
            this.ZCenter.Text = "0";
            this.ZCenter.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // GoCenter
            // 
            this.GoCenter.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GoCenter.Location = new System.Drawing.Point(134, 36);
            this.GoCenter.Margin = new System.Windows.Forms.Padding(1);
            this.GoCenter.Name = "GoCenter";
            this.GoCenter.Size = new System.Drawing.Size(38, 20);
            this.GoCenter.TabIndex = 338;
            this.GoCenter.Text = "Go";
            this.GoCenter.UseVisualStyleBackColor = true;
            this.GoCenter.Click += new System.EventHandler(this.GoCenterButton_Click);
            // 
            // CenterLabel
            // 
            this.CenterLabel.AutoSize = true;
            this.CenterLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CenterLabel.Location = new System.Drawing.Point(4, 39);
            this.CenterLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.CenterLabel.Name = "CenterLabel";
            this.CenterLabel.Size = new System.Drawing.Size(39, 14);
            this.CenterLabel.TabIndex = 337;
            this.CenterLabel.Text = "Center";
            // 
            // Set_Center
            // 
            this.Set_Center.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Set_Center.Location = new System.Drawing.Point(95, 36);
            this.Set_Center.Margin = new System.Windows.Forms.Padding(1);
            this.Set_Center.Name = "Set_Center";
            this.Set_Center.Size = new System.Drawing.Size(38, 20);
            this.Set_Center.TabIndex = 335;
            this.Set_Center.Text = "Set";
            this.Set_Center.UseVisualStyleBackColor = true;
            this.Set_Center.Click += new System.EventHandler(this.Set_Center_Click);
            // 
            // GoEnd
            // 
            this.GoEnd.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GoEnd.Location = new System.Drawing.Point(134, 57);
            this.GoEnd.Margin = new System.Windows.Forms.Padding(1);
            this.GoEnd.Name = "GoEnd";
            this.GoEnd.Size = new System.Drawing.Size(38, 20);
            this.GoEnd.TabIndex = 282;
            this.GoEnd.Text = "Go";
            this.GoEnd.UseVisualStyleBackColor = true;
            this.GoEnd.Click += new System.EventHandler(this.GoEnd_Click);
            // 
            // GoStart
            // 
            this.GoStart.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GoStart.Location = new System.Drawing.Point(134, 15);
            this.GoStart.Margin = new System.Windows.Forms.Padding(1);
            this.GoStart.Name = "GoStart";
            this.GoStart.Size = new System.Drawing.Size(38, 20);
            this.GoStart.TabIndex = 281;
            this.GoStart.Text = "Go";
            this.GoStart.UseVisualStyleBackColor = true;
            this.GoStart.Click += new System.EventHandler(this.GoStart_Click);
            // 
            // label77
            // 
            this.label77.AutoSize = true;
            this.label77.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label77.Location = new System.Drawing.Point(8, 59);
            this.label77.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label77.Name = "label77";
            this.label77.Size = new System.Drawing.Size(29, 14);
            this.label77.TabIndex = 280;
            this.label77.Text = "Stop";
            // 
            // label72
            // 
            this.label72.AutoSize = true;
            this.label72.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label72.Location = new System.Drawing.Point(8, 20);
            this.label72.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label72.Name = "label72";
            this.label72.Size = new System.Drawing.Size(30, 14);
            this.label72.TabIndex = 279;
            this.label72.Text = "Start";
            // 
            // NSlices2
            // 
            this.NSlices2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NSlices2.Location = new System.Drawing.Point(228, 45);
            this.NSlices2.Margin = new System.Windows.Forms.Padding(1);
            this.NSlices2.Name = "NSlices2";
            this.NSlices2.Size = new System.Drawing.Size(40, 20);
            this.NSlices2.TabIndex = 277;
            this.NSlices2.Text = "5";
            this.NSlices2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.NSlices2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ZStackParameterTextKeyDown);
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label42.Location = new System.Drawing.Point(183, 47);
            this.label42.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(42, 14);
            this.label42.TabIndex = 278;
            this.label42.Text = "#Slices";
            // 
            // ZEnd
            // 
            this.ZEnd.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZEnd.Location = new System.Drawing.Point(41, 57);
            this.ZEnd.Margin = new System.Windows.Forms.Padding(1);
            this.ZEnd.Name = "ZEnd";
            this.ZEnd.Size = new System.Drawing.Size(50, 20);
            this.ZEnd.TabIndex = 276;
            this.ZEnd.Text = "0";
            this.ZEnd.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // SliceStep
            // 
            this.SliceStep.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SliceStep.Location = new System.Drawing.Point(214, 22);
            this.SliceStep.Margin = new System.Windows.Forms.Padding(1);
            this.SliceStep.Name = "SliceStep";
            this.SliceStep.Size = new System.Drawing.Size(55, 20);
            this.SliceStep.TabIndex = 274;
            this.SliceStep.Text = "1.0";
            this.SliceStep.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.SliceStep.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ZStackParameterTextKeyDown);
            // 
            // ZStart
            // 
            this.ZStart.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZStart.Location = new System.Drawing.Point(41, 14);
            this.ZStart.Margin = new System.Windows.Forms.Padding(1);
            this.ZStart.Name = "ZStart";
            this.ZStart.Size = new System.Drawing.Size(50, 20);
            this.ZStart.TabIndex = 275;
            this.ZStart.Text = "0";
            this.ZStart.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // st_step
            // 
            this.st_step.AutoSize = true;
            this.st_step.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_step.Location = new System.Drawing.Point(187, 24);
            this.st_step.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_step.Name = "st_step";
            this.st_step.Size = new System.Drawing.Size(29, 14);
            this.st_step.TabIndex = 275;
            this.st_step.Text = "Step";
            // 
            // st_um
            // 
            this.st_um.AutoSize = true;
            this.st_um.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_um.Location = new System.Drawing.Point(273, 27);
            this.st_um.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_um.Name = "st_um";
            this.st_um.Size = new System.Drawing.Size(21, 14);
            this.st_um.TabIndex = 276;
            this.st_um.Text = "μm";
            // 
            // st_display
            // 
            this.st_display.AutoSize = true;
            this.st_display.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_display.Location = new System.Drawing.Point(552, 9);
            this.st_display.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_display.Name = "st_display";
            this.st_display.Size = new System.Drawing.Size(41, 13);
            this.st_display.TabIndex = 102;
            this.st_display.Text = "Display";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.powerToolStripMenuItem,
            this.plotToolStripMenuItem,
            this.windowsToolStripMenuItem,
            this.settingToolStripMenuItem,
            this.ToolsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(6, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(638, 24);
            this.menuStrip1.TabIndex = 283;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setPathToolStripMenuItem,
            this.toolStripSeparator1,
            this.loadSttingToolStripMenuItem,
            this.saveSettingToolStripMenuItem,
            this.quickSettingToolStripMenuItem,
            this.toolStripSeparator2,
            this.loadScanParametersToolStripMenuItem,
            this.toolStripSeparator4,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // setPathToolStripMenuItem
            // 
            this.setPathToolStripMenuItem.Name = "setPathToolStripMenuItem";
            this.setPathToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.setPathToolStripMenuItem.Text = "Set Path...";
            this.setPathToolStripMenuItem.Click += new System.EventHandler(this.FindPath_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(196, 6);
            // 
            // loadSttingToolStripMenuItem
            // 
            this.loadSttingToolStripMenuItem.Name = "loadSttingToolStripMenuItem";
            this.loadSttingToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.loadSttingToolStripMenuItem.Text = "Load Setting...";
            this.loadSttingToolStripMenuItem.Click += new System.EventHandler(this.LoadSttingToolStripMenuItem_Click);
            // 
            // saveSettingToolStripMenuItem
            // 
            this.saveSettingToolStripMenuItem.Name = "saveSettingToolStripMenuItem";
            this.saveSettingToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.saveSettingToolStripMenuItem.Text = "Save Setting As...";
            this.saveSettingToolStripMenuItem.Click += new System.EventHandler(this.saveSettingToolStripMenuItem_Click);
            // 
            // quickSettingToolStripMenuItem
            // 
            this.quickSettingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveSetting1ToolStripMenuItem,
            this.saveSetting2ToolStripMenuItem,
            this.saveSetting3ToolStripMenuItem,
            this.toolStripSeparator3,
            this.loadSetting1ToolStripMenuItem,
            this.loadSetting2ToolStripMenuItem,
            this.loadSetting3ToolStripMenuItem});
            this.quickSettingToolStripMenuItem.Name = "quickSettingToolStripMenuItem";
            this.quickSettingToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.quickSettingToolStripMenuItem.Text = "Quick setting";
            // 
            // saveSetting1ToolStripMenuItem
            // 
            this.saveSetting1ToolStripMenuItem.Name = "saveSetting1ToolStripMenuItem";
            this.saveSetting1ToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.saveSetting1ToolStripMenuItem.Text = "Save setting 1";
            this.saveSetting1ToolStripMenuItem.Click += new System.EventHandler(this.Numbered_Setting_Click);
            // 
            // saveSetting2ToolStripMenuItem
            // 
            this.saveSetting2ToolStripMenuItem.Name = "saveSetting2ToolStripMenuItem";
            this.saveSetting2ToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.saveSetting2ToolStripMenuItem.Text = "Save setting 2";
            this.saveSetting2ToolStripMenuItem.Click += new System.EventHandler(this.Numbered_Setting_Click);
            // 
            // saveSetting3ToolStripMenuItem
            // 
            this.saveSetting3ToolStripMenuItem.Name = "saveSetting3ToolStripMenuItem";
            this.saveSetting3ToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.saveSetting3ToolStripMenuItem.Text = "Save setting 3";
            this.saveSetting3ToolStripMenuItem.Click += new System.EventHandler(this.Numbered_Setting_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(164, 6);
            // 
            // loadSetting1ToolStripMenuItem
            // 
            this.loadSetting1ToolStripMenuItem.Name = "loadSetting1ToolStripMenuItem";
            this.loadSetting1ToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.loadSetting1ToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.loadSetting1ToolStripMenuItem.Text = "Load setting 1";
            this.loadSetting1ToolStripMenuItem.Click += new System.EventHandler(this.Numbered_Setting_Click);
            // 
            // loadSetting2ToolStripMenuItem
            // 
            this.loadSetting2ToolStripMenuItem.Name = "loadSetting2ToolStripMenuItem";
            this.loadSetting2ToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.loadSetting2ToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.loadSetting2ToolStripMenuItem.Text = "Load setting 2";
            this.loadSetting2ToolStripMenuItem.Click += new System.EventHandler(this.Numbered_Setting_Click);
            // 
            // loadSetting3ToolStripMenuItem
            // 
            this.loadSetting3ToolStripMenuItem.Name = "loadSetting3ToolStripMenuItem";
            this.loadSetting3ToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.loadSetting3ToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.loadSetting3ToolStripMenuItem.Text = "Load setting 3";
            this.loadSetting3ToolStripMenuItem.Click += new System.EventHandler(this.Numbered_Setting_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(196, 6);
            // 
            // loadScanParametersToolStripMenuItem
            // 
            this.loadScanParametersToolStripMenuItem.Name = "loadScanParametersToolStripMenuItem";
            this.loadScanParametersToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.loadScanParametersToolStripMenuItem.Text = "Load Scan Parameters...";
            this.loadScanParametersToolStripMenuItem.Click += new System.EventHandler(this.loadScanParametersToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(196, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // powerToolStripMenuItem
            // 
            this.powerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.calibrateToolStripMenuItem,
            this.showCalibrationCurveToolStripMenuItem,
            this.showInputoutputCurveToolStripMenuItem});
            this.powerToolStripMenuItem.Name = "powerToolStripMenuItem";
            this.powerToolStripMenuItem.Size = new System.Drawing.Size(52, 22);
            this.powerToolStripMenuItem.Text = "Power";
            // 
            // calibrateToolStripMenuItem
            // 
            this.calibrateToolStripMenuItem.Name = "calibrateToolStripMenuItem";
            this.calibrateToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.calibrateToolStripMenuItem.Text = "Calibrate";
            this.calibrateToolStripMenuItem.Click += new System.EventHandler(this.CalibrateToolStripMenuItem_Click_1);
            // 
            // showCalibrationCurveToolStripMenuItem
            // 
            this.showCalibrationCurveToolStripMenuItem.Name = "showCalibrationCurveToolStripMenuItem";
            this.showCalibrationCurveToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.showCalibrationCurveToolStripMenuItem.Text = "Show calibration curve";
            this.showCalibrationCurveToolStripMenuItem.Click += new System.EventHandler(this.showCalibrationCurveToolStripMenuItem_Click_1);
            // 
            // showInputoutputCurveToolStripMenuItem
            // 
            this.showInputoutputCurveToolStripMenuItem.Name = "showInputoutputCurveToolStripMenuItem";
            this.showInputoutputCurveToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.showInputoutputCurveToolStripMenuItem.Text = "Show input-output curve";
            this.showInputoutputCurveToolStripMenuItem.Click += new System.EventHandler(this.showInputoutputCurveToolStripMenuItem_Click);
            // 
            // plotToolStripMenuItem
            // 
            this.plotToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.plotScanToolStripMenuItem,
            this.plotPockelsToolStripMenuItem,
            this.plotScanGrabToolStripMenuItem,
            this.plotPockelsGrabToolStripMenuItem});
            this.plotToolStripMenuItem.Name = "plotToolStripMenuItem";
            this.plotToolStripMenuItem.Size = new System.Drawing.Size(40, 22);
            this.plotToolStripMenuItem.Text = "Plot";
            // 
            // plotScanToolStripMenuItem
            // 
            this.plotScanToolStripMenuItem.Name = "plotScanToolStripMenuItem";
            this.plotScanToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.plotScanToolStripMenuItem.Text = "Plot Scan";
            this.plotScanToolStripMenuItem.Click += new System.EventHandler(this.PlotScanToolStripMenuItem_Click);
            // 
            // plotPockelsToolStripMenuItem
            // 
            this.plotPockelsToolStripMenuItem.Name = "plotPockelsToolStripMenuItem";
            this.plotPockelsToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.plotPockelsToolStripMenuItem.Text = "Plot Pockels";
            this.plotPockelsToolStripMenuItem.Click += new System.EventHandler(this.PlotPockelsToolStripMenuItem_Click);
            // 
            // plotScanGrabToolStripMenuItem
            // 
            this.plotScanGrabToolStripMenuItem.Name = "plotScanGrabToolStripMenuItem";
            this.plotScanGrabToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.plotScanGrabToolStripMenuItem.Text = "Plot Scan (+Uncage/Trigger)";
            this.plotScanGrabToolStripMenuItem.Click += new System.EventHandler(this.PlotScanGrabToolStripMenuItem_Click);
            // 
            // plotPockelsGrabToolStripMenuItem
            // 
            this.plotPockelsGrabToolStripMenuItem.Name = "plotPockelsGrabToolStripMenuItem";
            this.plotPockelsGrabToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.plotPockelsGrabToolStripMenuItem.Text = "Plot Pockels (+Uncage/Trigger)";
            this.plotPockelsGrabToolStripMenuItem.Click += new System.EventHandler(this.PlotPockelsGrabToolStripMenuItem_Click);
            // 
            // windowsToolStripMenuItem
            // 
            this.windowsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.imageToolStripMenuItem,
            this.realtimePlotToolStripMenuItem,
            this.resetWindowPositionsToolStripMenuItem});
            this.windowsToolStripMenuItem.Name = "windowsToolStripMenuItem";
            this.windowsToolStripMenuItem.Size = new System.Drawing.Size(68, 22);
            this.windowsToolStripMenuItem.Text = "Windows";
            // 
            // imageToolStripMenuItem
            // 
            this.imageToolStripMenuItem.Name = "imageToolStripMenuItem";
            this.imageToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.imageToolStripMenuItem.Text = "FLIM_analysis";
            this.imageToolStripMenuItem.Click += new System.EventHandler(this.Image1ToolStripMenuItem_Click);
            // 
            // realtimePlotToolStripMenuItem
            // 
            this.realtimePlotToolStripMenuItem.Name = "realtimePlotToolStripMenuItem";
            this.realtimePlotToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.realtimePlotToolStripMenuItem.Text = "Realtime plot";
            this.realtimePlotToolStripMenuItem.Click += new System.EventHandler(this.realtimePlotToolStripMenuItem_Click);
            // 
            // resetWindowPositionsToolStripMenuItem
            // 
            this.resetWindowPositionsToolStripMenuItem.Name = "resetWindowPositionsToolStripMenuItem";
            this.resetWindowPositionsToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.resetWindowPositionsToolStripMenuItem.Text = "Reset window positions";
            this.resetWindowPositionsToolStripMenuItem.Click += new System.EventHandler(this.resetWindowPositionsToolStripMenuItem_Click);
            // 
            // settingToolStripMenuItem
            // 
            this.settingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nIDAQConfigToolStripMenuItem,
            this.dIOPanelToolStripMenuItem,
            this.pMTControlToolStripMenuItem});
            this.settingToolStripMenuItem.Name = "settingToolStripMenuItem";
            this.settingToolStripMenuItem.Size = new System.Drawing.Size(70, 22);
            this.settingToolStripMenuItem.Text = "Hardware";
            this.settingToolStripMenuItem.Click += new System.EventHandler(this.settingToolStripMenuItem_Click);
            // 
            // nIDAQConfigToolStripMenuItem
            // 
            this.nIDAQConfigToolStripMenuItem.Name = "nIDAQConfigToolStripMenuItem";
            this.nIDAQConfigToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.nIDAQConfigToolStripMenuItem.Text = "Hardware Config";
            this.nIDAQConfigToolStripMenuItem.Click += new System.EventHandler(this.NIDAQConfigToolStripMenuItem_Click);
            // 
            // dIOPanelToolStripMenuItem
            // 
            this.dIOPanelToolStripMenuItem.Name = "dIOPanelToolStripMenuItem";
            this.dIOPanelToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.dIOPanelToolStripMenuItem.Text = "DIO panel";
            this.dIOPanelToolStripMenuItem.Click += new System.EventHandler(this.dIOPanelToolStripMenuItem_Click);
            // 
            // pMTControlToolStripMenuItem
            // 
            this.pMTControlToolStripMenuItem.Name = "pMTControlToolStripMenuItem";
            this.pMTControlToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.pMTControlToolStripMenuItem.Text = "PMT Control";
            this.pMTControlToolStripMenuItem.Click += new System.EventHandler(this.pMTControlToolStripMenuItem_Click);
            // 
            // ToolsToolStripMenuItem
            // 
            this.ToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.uncagingControlToolStripMenuItem1,
            this.digitalOutputControlToolStripMenuItem,
            this.imageSeqControlToolStripMenuItem,
            this.fastZControlToolStripMenuItem,
            this.shadingCorretionToolStripMenuItem,
            this.driftCorrectionToolStripMenuItem,
            this.remoteControlToolStripMenuItem1,
            this.stageControlToolStripMenuItem,
            this.electrophysiologyToolStripMenuItem});
            this.ToolsToolStripMenuItem.Name = "ToolsToolStripMenuItem";
            this.ToolsToolStripMenuItem.Size = new System.Drawing.Size(47, 22);
            this.ToolsToolStripMenuItem.Text = "Tools";
            this.ToolsToolStripMenuItem.Click += new System.EventHandler(this.settingToolStripMenuItem_Click);
            // 
            // uncagingControlToolStripMenuItem1
            // 
            this.uncagingControlToolStripMenuItem1.Name = "uncagingControlToolStripMenuItem1";
            this.uncagingControlToolStripMenuItem1.Size = new System.Drawing.Size(192, 22);
            this.uncagingControlToolStripMenuItem1.Text = "Uncaging Control";
            this.uncagingControlToolStripMenuItem1.Click += new System.EventHandler(this.uncagingControlToolStripMenuItem_Click);
            // 
            // digitalOutputControlToolStripMenuItem
            // 
            this.digitalOutputControlToolStripMenuItem.Name = "digitalOutputControlToolStripMenuItem";
            this.digitalOutputControlToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.digitalOutputControlToolStripMenuItem.Text = "Digital Output Control";
            this.digitalOutputControlToolStripMenuItem.Click += new System.EventHandler(this.digitalOutputControlToolStripMenuItem_Click);
            // 
            // imageSeqControlToolStripMenuItem
            // 
            this.imageSeqControlToolStripMenuItem.Name = "imageSeqControlToolStripMenuItem";
            this.imageSeqControlToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.imageSeqControlToolStripMenuItem.Text = "Image Seq Control";
            this.imageSeqControlToolStripMenuItem.Click += new System.EventHandler(this.imageSeqControlToolStripMenuItem_Click);
            // 
            // fastZControlToolStripMenuItem
            // 
            this.fastZControlToolStripMenuItem.Name = "fastZControlToolStripMenuItem";
            this.fastZControlToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.fastZControlToolStripMenuItem.Text = "FastZ Control";
            this.fastZControlToolStripMenuItem.Click += new System.EventHandler(this.fastZControlToolStripMenuItem_Click);
            // 
            // shadingCorretionToolStripMenuItem
            // 
            this.shadingCorretionToolStripMenuItem.Name = "shadingCorretionToolStripMenuItem";
            this.shadingCorretionToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.shadingCorretionToolStripMenuItem.Text = "Shading Corretion";
            this.shadingCorretionToolStripMenuItem.Click += new System.EventHandler(this.shadingCorretionToolStripMenuItem_Click);
            // 
            // driftCorrectionToolStripMenuItem
            // 
            this.driftCorrectionToolStripMenuItem.Name = "driftCorrectionToolStripMenuItem";
            this.driftCorrectionToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.driftCorrectionToolStripMenuItem.Text = "Drift Correction";
            this.driftCorrectionToolStripMenuItem.Click += new System.EventHandler(this.driftCorrectionToolStripMenuItem_Click);
            // 
            // remoteControlToolStripMenuItem1
            // 
            this.remoteControlToolStripMenuItem1.Name = "remoteControlToolStripMenuItem1";
            this.remoteControlToolStripMenuItem1.Size = new System.Drawing.Size(192, 22);
            this.remoteControlToolStripMenuItem1.Text = "Remote Control";
            this.remoteControlToolStripMenuItem1.Click += new System.EventHandler(this.remoteControlToolStripMenuItem1_Click);
            // 
            // stageControlToolStripMenuItem
            // 
            this.stageControlToolStripMenuItem.Name = "stageControlToolStripMenuItem";
            this.stageControlToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.stageControlToolStripMenuItem.Text = "Stage Control";
            this.stageControlToolStripMenuItem.Click += new System.EventHandler(this.stageControlToolStripMenuItem_Click);
            // 
            // electrophysiologyToolStripMenuItem
            // 
            this.electrophysiologyToolStripMenuItem.Name = "electrophysiologyToolStripMenuItem";
            this.electrophysiologyToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.electrophysiologyToolStripMenuItem.Text = "Electrophysiology";
            this.electrophysiologyToolStripMenuItem.Click += new System.EventHandler(this.electrophysiologyToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fLIMageToolStripMenuItem});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 22);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // fLIMageToolStripMenuItem
            // 
            this.fLIMageToolStripMenuItem.Name = "fLIMageToolStripMenuItem";
            this.fLIMageToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.fLIMageToolStripMenuItem.Text = "FLIMage";
            this.fLIMageToolStripMenuItem.Click += new System.EventHandler(this.FLIMageToolStripMenuItem_Click);
            // 
            // Uncage_while_image_check
            // 
            this.Uncage_while_image_check.AutoSize = true;
            this.Uncage_while_image_check.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Uncage_while_image_check.Location = new System.Drawing.Point(17, 20);
            this.Uncage_while_image_check.Name = "Uncage_while_image_check";
            this.Uncage_while_image_check.Size = new System.Drawing.Size(131, 18);
            this.Uncage_while_image_check.TabIndex = 317;
            this.Uncage_while_image_check.Text = "Uncage while imaging";
            this.Uncage_while_image_check.UseVisualStyleBackColor = true;
            this.Uncage_while_image_check.Click += new System.EventHandler(this.Uncage_while_image_check_Click);
            // 
            // Ch_rate2
            // 
            this.Ch_rate2.AutoSize = true;
            this.Ch_rate2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Ch_rate2.Location = new System.Drawing.Point(559, 665);
            this.Ch_rate2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Ch_rate2.Name = "Ch_rate2";
            this.Ch_rate2.Size = new System.Drawing.Size(46, 14);
            this.Ch_rate2.TabIndex = 290;
            this.Ch_rate2.Text = "1,234 /s";
            this.Ch_rate2.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // Ch_rate1
            // 
            this.Ch_rate1.AutoSize = true;
            this.Ch_rate1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Ch_rate1.Location = new System.Drawing.Point(559, 650);
            this.Ch_rate1.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Ch_rate1.Name = "Ch_rate1";
            this.Ch_rate1.Size = new System.Drawing.Size(46, 14);
            this.Ch_rate1.TabIndex = 289;
            this.Ch_rate1.Text = "1,234 /s";
            this.Ch_rate1.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // Sync_rate
            // 
            this.Sync_rate.AutoSize = true;
            this.Sync_rate.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Sync_rate.Location = new System.Drawing.Point(556, 567);
            this.Sync_rate.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Sync_rate.Name = "Sync_rate";
            this.Sync_rate.Size = new System.Drawing.Size(64, 14);
            this.Sync_rate.TabIndex = 288;
            this.Sync_rate.Text = "00.000 MHz";
            this.Sync_rate.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(224, 105);
            this.label13.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(0, 13);
            this.label13.TabIndex = 287;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(540, 635);
            this.label14.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(71, 14);
            this.label14.TabIndex = 286;
            this.label14.Text = "Photon Count";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(540, 550);
            this.label15.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(64, 14);
            this.label15.TabIndex = 285;
            this.label15.Text = "Laser pulse";
            // 
            // LaserPanel
            // 
            this.LaserPanel.Controls.Add(this.zeroVoltage);
            this.LaserPanel.Controls.Add(this.needCalibLabel);
            this.LaserPanel.Controls.Add(this.Calibrate1);
            this.LaserPanel.Controls.Add(this.laserPowerPanel);
            this.LaserPanel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LaserPanel.Location = new System.Drawing.Point(318, 396);
            this.LaserPanel.Name = "LaserPanel";
            this.LaserPanel.Size = new System.Drawing.Size(313, 139);
            this.LaserPanel.TabIndex = 282;
            this.LaserPanel.TabStop = false;
            this.LaserPanel.Text = "Laser";
            // 
            // zeroVoltage
            // 
            this.zeroVoltage.Location = new System.Drawing.Point(261, 30);
            this.zeroVoltage.Name = "zeroVoltage";
            this.zeroVoltage.Size = new System.Drawing.Size(33, 19);
            this.zeroVoltage.TabIndex = 128;
            this.zeroVoltage.Text = "0V";
            this.zeroVoltage.UseVisualStyleBackColor = true;
            this.zeroVoltage.Click += new System.EventHandler(this.ZeroVoltage_Click);
            // 
            // needCalibLabel
            // 
            this.needCalibLabel.AutoSize = true;
            this.needCalibLabel.ForeColor = System.Drawing.Color.Red;
            this.needCalibLabel.Location = new System.Drawing.Point(18, 15);
            this.needCalibLabel.Name = "needCalibLabel";
            this.needCalibLabel.Size = new System.Drawing.Size(117, 14);
            this.needCalibLabel.TabIndex = 124;
            this.needCalibLabel.Text = "** Need calibration **";
            // 
            // Calibrate1
            // 
            this.Calibrate1.Location = new System.Drawing.Point(215, 30);
            this.Calibrate1.Name = "Calibrate1";
            this.Calibrate1.Size = new System.Drawing.Size(45, 19);
            this.Calibrate1.TabIndex = 123;
            this.Calibrate1.Text = "Calib.";
            this.Calibrate1.UseVisualStyleBackColor = true;
            this.Calibrate1.Click += new System.EventHandler(this.Calibrate1_Click);
            // 
            // laserPowerPanel
            // 
            this.laserPowerPanel.Controls.Add(this.tabPage1);
            this.laserPowerPanel.Controls.Add(tabPage2);
            this.laserPowerPanel.Controls.Add(this.tabPage3);
            this.laserPowerPanel.Controls.Add(this.tabPage4);
            this.laserPowerPanel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.laserPowerPanel.Location = new System.Drawing.Point(13, 32);
            this.laserPowerPanel.Name = "laserPowerPanel";
            this.laserPowerPanel.SelectedIndex = 0;
            this.laserPowerPanel.Size = new System.Drawing.Size(266, 103);
            this.laserPowerPanel.TabIndex = 119;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.powerRead1);
            this.tabPage1.Controls.Add(this.needCalib1);
            this.tabPage1.Controls.Add(this.UncageLaser1);
            this.tabPage1.Controls.Add(this.ImageLaser1);
            this.tabPage1.Controls.Add(this.st_Power);
            this.tabPage1.Controls.Add(this.PowerSlider1);
            this.tabPage1.Controls.Add(this.Power1);
            this.tabPage1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabPage1.Location = new System.Drawing.Point(4, 23);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(258, 76);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Laser 1";
            // 
            // powerRead1
            // 
            this.powerRead1.AutoSize = true;
            this.powerRead1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.powerRead1.Location = new System.Drawing.Point(3, 61);
            this.powerRead1.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.powerRead1.Name = "powerRead1";
            this.powerRead1.Size = new System.Drawing.Size(36, 13);
            this.powerRead1.TabIndex = 125;
            this.powerRead1.Text = "X mW";
            // 
            // needCalib1
            // 
            this.needCalib1.ForeColor = System.Drawing.Color.Red;
            this.needCalib1.Location = new System.Drawing.Point(130, 5);
            this.needCalib1.Name = "needCalib1";
            this.needCalib1.Size = new System.Drawing.Size(125, 30);
            this.needCalib1.TabIndex = 124;
            this.needCalib1.Text = "Need Calibration";
            this.needCalib1.UseVisualStyleBackColor = true;
            this.needCalib1.Click += new System.EventHandler(this.Calibrate1_Click);
            // 
            // UncageLaser1
            // 
            this.UncageLaser1.AutoSize = true;
            this.UncageLaser1.Location = new System.Drawing.Point(160, 54);
            this.UncageLaser1.Name = "UncageLaser1";
            this.UncageLaser1.Size = new System.Drawing.Size(71, 18);
            this.UncageLaser1.TabIndex = 114;
            this.UncageLaser1.Text = "Uncaging";
            this.UncageLaser1.UseVisualStyleBackColor = true;
            this.UncageLaser1.Click += new System.EventHandler(this.ImageLaser1_CheckedChanged);
            // 
            // ImageLaser1
            // 
            this.ImageLaser1.AutoSize = true;
            this.ImageLaser1.Location = new System.Drawing.Point(90, 54);
            this.ImageLaser1.Name = "ImageLaser1";
            this.ImageLaser1.Size = new System.Drawing.Size(62, 18);
            this.ImageLaser1.TabIndex = 113;
            this.ImageLaser1.Text = "Imaging";
            this.ImageLaser1.UseVisualStyleBackColor = true;
            this.ImageLaser1.Click += new System.EventHandler(this.ImageLaser1_CheckedChanged);
            // 
            // st_Power
            // 
            this.st_Power.AutoSize = true;
            this.st_Power.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_Power.Location = new System.Drawing.Point(14, 10);
            this.st_Power.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_Power.Name = "st_Power";
            this.st_Power.Size = new System.Drawing.Size(68, 16);
            this.st_Power.TabIndex = 42;
            this.st_Power.Text = "Power (%)";
            // 
            // PowerSlider1
            // 
            this.PowerSlider1.AutoSize = false;
            this.PowerSlider1.BackColor = System.Drawing.SystemColors.Control;
            this.PowerSlider1.LargeChange = 10;
            this.PowerSlider1.Location = new System.Drawing.Point(5, 35);
            this.PowerSlider1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PowerSlider1.Maximum = 100;
            this.PowerSlider1.Name = "PowerSlider1";
            this.PowerSlider1.Size = new System.Drawing.Size(226, 27);
            this.PowerSlider1.TabIndex = 112;
            this.PowerSlider1.TickStyle = System.Windows.Forms.TickStyle.None;
            this.PowerSlider1.Value = 10;
            this.PowerSlider1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PowerSlider1_MouseUp);
            // 
            // Power1
            // 
            this.Power1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Power1.Location = new System.Drawing.Point(171, 8);
            this.Power1.Margin = new System.Windows.Forms.Padding(1);
            this.Power1.Name = "Power1";
            this.Power1.Size = new System.Drawing.Size(56, 26);
            this.Power1.TabIndex = 111;
            this.Power1.Text = "10";
            this.Power1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Power1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PowerText_change);
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage3.Controls.Add(this.powerRead3);
            this.tabPage3.Controls.Add(this.needCalib3);
            this.tabPage3.Controls.Add(this.UncageLaser3);
            this.tabPage3.Controls.Add(this.ImageLaser3);
            this.tabPage3.Controls.Add(this.label20);
            this.tabPage3.Controls.Add(this.PowerSlider3);
            this.tabPage3.Controls.Add(this.Power3);
            this.tabPage3.Location = new System.Drawing.Point(4, 23);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(258, 76);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Laser 3";
            // 
            // powerRead3
            // 
            this.powerRead3.AutoSize = true;
            this.powerRead3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.powerRead3.Location = new System.Drawing.Point(3, 61);
            this.powerRead3.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.powerRead3.Name = "powerRead3";
            this.powerRead3.Size = new System.Drawing.Size(35, 14);
            this.powerRead3.TabIndex = 126;
            this.powerRead3.Text = "X mW";
            // 
            // needCalib3
            // 
            this.needCalib3.ForeColor = System.Drawing.Color.Red;
            this.needCalib3.Location = new System.Drawing.Point(110, 8);
            this.needCalib3.Name = "needCalib3";
            this.needCalib3.Size = new System.Drawing.Size(125, 30);
            this.needCalib3.TabIndex = 125;
            this.needCalib3.Text = "Need Calibration";
            this.needCalib3.UseVisualStyleBackColor = true;
            // 
            // UncageLaser3
            // 
            this.UncageLaser3.AutoSize = true;
            this.UncageLaser3.Location = new System.Drawing.Point(160, 54);
            this.UncageLaser3.Name = "UncageLaser3";
            this.UncageLaser3.Size = new System.Drawing.Size(71, 18);
            this.UncageLaser3.TabIndex = 120;
            this.UncageLaser3.Text = "Uncaging";
            this.UncageLaser3.UseVisualStyleBackColor = true;
            this.UncageLaser3.Click += new System.EventHandler(this.ImageLaser1_CheckedChanged);
            // 
            // ImageLaser3
            // 
            this.ImageLaser3.AutoSize = true;
            this.ImageLaser3.Location = new System.Drawing.Point(90, 54);
            this.ImageLaser3.Name = "ImageLaser3";
            this.ImageLaser3.Size = new System.Drawing.Size(62, 18);
            this.ImageLaser3.TabIndex = 119;
            this.ImageLaser3.Text = "Imaging";
            this.ImageLaser3.UseVisualStyleBackColor = true;
            this.ImageLaser3.Click += new System.EventHandler(this.ImageLaser1_CheckedChanged);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.Location = new System.Drawing.Point(14, 10);
            this.label20.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(68, 16);
            this.label20.TabIndex = 116;
            this.label20.Text = "Power (%)";
            // 
            // PowerSlider3
            // 
            this.PowerSlider3.AutoSize = false;
            this.PowerSlider3.BackColor = System.Drawing.SystemColors.Control;
            this.PowerSlider3.LargeChange = 10;
            this.PowerSlider3.Location = new System.Drawing.Point(5, 35);
            this.PowerSlider3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PowerSlider3.Maximum = 100;
            this.PowerSlider3.Name = "PowerSlider3";
            this.PowerSlider3.Size = new System.Drawing.Size(226, 27);
            this.PowerSlider3.TabIndex = 118;
            this.PowerSlider3.TickStyle = System.Windows.Forms.TickStyle.None;
            this.PowerSlider3.Value = 10;
            this.PowerSlider3.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PowerSlider1_MouseUp);
            // 
            // Power3
            // 
            this.Power3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Power3.Location = new System.Drawing.Point(171, 8);
            this.Power3.Margin = new System.Windows.Forms.Padding(1);
            this.Power3.Name = "Power3";
            this.Power3.Size = new System.Drawing.Size(56, 26);
            this.Power3.TabIndex = 117;
            this.Power3.Text = "10";
            this.Power3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Power3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PowerText_change);
            // 
            // tabPage4
            // 
            this.tabPage4.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage4.Controls.Add(this.powerRead4);
            this.tabPage4.Controls.Add(this.needCalib4);
            this.tabPage4.Controls.Add(this.UncageLaser4);
            this.tabPage4.Controls.Add(this.ImageLaser4);
            this.tabPage4.Controls.Add(this.label17);
            this.tabPage4.Controls.Add(this.PowerSlider4);
            this.tabPage4.Controls.Add(this.Power4);
            this.tabPage4.Location = new System.Drawing.Point(4, 23);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(258, 76);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Laser 4";
            // 
            // powerRead4
            // 
            this.powerRead4.AutoSize = true;
            this.powerRead4.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.powerRead4.Location = new System.Drawing.Point(3, 61);
            this.powerRead4.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.powerRead4.Name = "powerRead4";
            this.powerRead4.Size = new System.Drawing.Size(35, 14);
            this.powerRead4.TabIndex = 126;
            this.powerRead4.Text = "X mW";
            // 
            // needCalib4
            // 
            this.needCalib4.ForeColor = System.Drawing.Color.Red;
            this.needCalib4.Location = new System.Drawing.Point(110, 8);
            this.needCalib4.Name = "needCalib4";
            this.needCalib4.Size = new System.Drawing.Size(125, 30);
            this.needCalib4.TabIndex = 125;
            this.needCalib4.Text = "Need Calibration";
            this.needCalib4.UseVisualStyleBackColor = true;
            // 
            // UncageLaser4
            // 
            this.UncageLaser4.AutoSize = true;
            this.UncageLaser4.Location = new System.Drawing.Point(160, 54);
            this.UncageLaser4.Name = "UncageLaser4";
            this.UncageLaser4.Size = new System.Drawing.Size(71, 18);
            this.UncageLaser4.TabIndex = 123;
            this.UncageLaser4.Text = "Uncaging";
            this.UncageLaser4.UseVisualStyleBackColor = true;
            this.UncageLaser4.Click += new System.EventHandler(this.ImageLaser1_CheckedChanged);
            // 
            // ImageLaser4
            // 
            this.ImageLaser4.AutoSize = true;
            this.ImageLaser4.Location = new System.Drawing.Point(90, 54);
            this.ImageLaser4.Name = "ImageLaser4";
            this.ImageLaser4.Size = new System.Drawing.Size(62, 18);
            this.ImageLaser4.TabIndex = 122;
            this.ImageLaser4.Text = "Imaging";
            this.ImageLaser4.UseVisualStyleBackColor = true;
            this.ImageLaser4.Click += new System.EventHandler(this.Image1ToolStripMenuItem_Click);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(14, 10);
            this.label17.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(68, 16);
            this.label17.TabIndex = 119;
            this.label17.Text = "Power (%)";
            // 
            // PowerSlider4
            // 
            this.PowerSlider4.AutoSize = false;
            this.PowerSlider4.BackColor = System.Drawing.SystemColors.Control;
            this.PowerSlider4.LargeChange = 10;
            this.PowerSlider4.Location = new System.Drawing.Point(5, 35);
            this.PowerSlider4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PowerSlider4.Maximum = 100;
            this.PowerSlider4.Name = "PowerSlider4";
            this.PowerSlider4.Size = new System.Drawing.Size(226, 27);
            this.PowerSlider4.TabIndex = 121;
            this.PowerSlider4.TickStyle = System.Windows.Forms.TickStyle.None;
            this.PowerSlider4.Value = 10;
            this.PowerSlider4.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PowerSlider1_MouseUp);
            // 
            // Power4
            // 
            this.Power4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Power4.Location = new System.Drawing.Point(171, 8);
            this.Power4.Margin = new System.Windows.Forms.Padding(1);
            this.Power4.Name = "Power4";
            this.Power4.Size = new System.Drawing.Size(56, 26);
            this.Power4.TabIndex = 120;
            this.Power4.Text = "10";
            this.Power4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Power4.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PowerText_change);
            // 
            // laserWarningButton
            // 
            this.laserWarningButton.ForeColor = System.Drawing.Color.Red;
            this.laserWarningButton.Location = new System.Drawing.Point(21, 59);
            this.laserWarningButton.Name = "laserWarningButton";
            this.laserWarningButton.Size = new System.Drawing.Size(174, 45);
            this.laserWarningButton.TabIndex = 126;
            this.laserWarningButton.Text = "Check laser setting!\r\nLaser pulse rate is not good!";
            this.laserWarningButton.UseVisualStyleBackColor = true;
            // 
            // acquisitionPanel
            // 
            this.acquisitionPanel.Controls.Add(this.DO_whileImaging_check);
            this.acquisitionPanel.Controls.Add(this.laserWarningButton);
            this.acquisitionPanel.Controls.Add(this.analyzeEach);
            this.acquisitionPanel.Controls.Add(this.label79);
            this.acquisitionPanel.Controls.Add(this.Uncage_while_image_check);
            this.acquisitionPanel.Controls.Add(this.label13);
            this.acquisitionPanel.Controls.Add(this.FocusButton);
            this.acquisitionPanel.Controls.Add(this.GrabButton);
            this.acquisitionPanel.Controls.Add(this.ETime);
            this.acquisitionPanel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.acquisitionPanel.Location = new System.Drawing.Point(318, 537);
            this.acquisitionPanel.Name = "acquisitionPanel";
            this.acquisitionPanel.Size = new System.Drawing.Size(201, 192);
            this.acquisitionPanel.TabIndex = 319;
            this.acquisitionPanel.TabStop = false;
            this.acquisitionPanel.Text = "Acquisition";
            // 
            // DO_whileImaging_check
            // 
            this.DO_whileImaging_check.AutoSize = true;
            this.DO_whileImaging_check.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DO_whileImaging_check.Location = new System.Drawing.Point(17, 38);
            this.DO_whileImaging_check.Name = "DO_whileImaging_check";
            this.DO_whileImaging_check.Size = new System.Drawing.Size(109, 18);
            this.DO_whileImaging_check.TabIndex = 323;
            this.DO_whileImaging_check.Text = "DO while imaging";
            this.DO_whileImaging_check.UseVisualStyleBackColor = true;
            this.DO_whileImaging_check.Click += new System.EventHandler(this.Uncage_while_image_check_Click);
            // 
            // analyzeEach
            // 
            this.analyzeEach.AutoSize = true;
            this.analyzeEach.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.analyzeEach.Location = new System.Drawing.Point(12, 151);
            this.analyzeEach.Name = "analyzeEach";
            this.analyzeEach.Size = new System.Drawing.Size(173, 18);
            this.analyzeEach.TabIndex = 322;
            this.analyzeEach.Text = "Analyze after each acquisition";
            this.analyzeEach.UseVisualStyleBackColor = true;
            this.analyzeEach.CheckedChanged += new System.EventHandler(this.analyzeEach_CheckedChanged);
            // 
            // label79
            // 
            this.label79.AutoSize = true;
            this.label79.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label79.Location = new System.Drawing.Point(65, 75);
            this.label79.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label79.Name = "label79";
            this.label79.Size = new System.Drawing.Size(70, 14);
            this.label79.TabIndex = 321;
            this.label79.Text = "Elapsed time:";
            // 
            // ExtTriggerCB
            // 
            this.ExtTriggerCB.AutoSize = true;
            this.ExtTriggerCB.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExtTriggerCB.Location = new System.Drawing.Point(541, 682);
            this.ExtTriggerCB.Name = "ExtTriggerCB";
            this.ExtTriggerCB.Size = new System.Drawing.Size(75, 18);
            this.ExtTriggerCB.TabIndex = 339;
            this.ExtTriggerCB.Text = "Ext trigger";
            this.ExtTriggerCB.UseVisualStyleBackColor = true;
            this.ExtTriggerCB.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // expectedRate
            // 
            this.expectedRate.AutoSize = true;
            this.expectedRate.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.expectedRate.Location = new System.Drawing.Point(558, 616);
            this.expectedRate.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.expectedRate.Name = "expectedRate";
            this.expectedRate.Size = new System.Drawing.Size(52, 14);
            this.expectedRate.TabIndex = 324;
            this.expectedRate.Text = "80.0 MHz";
            // 
            // label67
            // 
            this.label67.AutoSize = true;
            this.label67.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label67.Location = new System.Drawing.Point(539, 600);
            this.label67.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label67.Name = "label67";
            this.label67.Size = new System.Drawing.Size(77, 14);
            this.label67.TabIndex = 323;
            this.label67.Text = "Expected rate:";
            // 
            // Sync_rate2
            // 
            this.Sync_rate2.AutoSize = true;
            this.Sync_rate2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Sync_rate2.Location = new System.Drawing.Point(556, 584);
            this.Sync_rate2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Sync_rate2.Name = "Sync_rate2";
            this.Sync_rate2.Size = new System.Drawing.Size(64, 14);
            this.Sync_rate2.TabIndex = 320;
            this.Sync_rate2.Text = "00.000 MHz";
            this.Sync_rate2.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // st_BaseName
            // 
            this.st_BaseName.AutoSize = true;
            this.st_BaseName.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_BaseName.Location = new System.Drawing.Point(4, 41);
            this.st_BaseName.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_BaseName.Name = "st_BaseName";
            this.st_BaseName.Size = new System.Drawing.Size(62, 14);
            this.st_BaseName.TabIndex = 10;
            this.st_BaseName.Text = "Base Name";
            // 
            // st_PathName
            // 
            this.st_PathName.AutoSize = true;
            this.st_PathName.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_PathName.Location = new System.Drawing.Point(5, 92);
            this.st_PathName.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_PathName.Name = "st_PathName";
            this.st_PathName.Size = new System.Drawing.Size(81, 14);
            this.st_PathName.TabIndex = 39;
            this.st_PathName.Text = "Directory Name";
            // 
            // st_fileN
            // 
            this.st_fileN.AutoSize = true;
            this.st_fileN.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_fileN.Location = new System.Drawing.Point(199, 44);
            this.st_fileN.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_fileN.Name = "st_fileN";
            this.st_fileN.Size = new System.Drawing.Size(29, 14);
            this.st_fileN.TabIndex = 17;
            this.st_fileN.Text = "File#";
            // 
            // BaseName
            // 
            this.BaseName.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BaseName.Location = new System.Drawing.Point(63, 38);
            this.BaseName.Margin = new System.Windows.Forms.Padding(1);
            this.BaseName.Name = "BaseName";
            this.BaseName.Size = new System.Drawing.Size(134, 20);
            this.BaseName.TabIndex = 5;
            this.BaseName.Text = "Test";
            this.BaseName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // FileN
            // 
            this.FileN.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileN.Location = new System.Drawing.Point(231, 39);
            this.FileN.Margin = new System.Windows.Forms.Padding(1);
            this.FileN.Name = "FileN";
            this.FileN.Size = new System.Drawing.Size(56, 20);
            this.FileN.TabIndex = 16;
            this.FileN.Text = "1";
            this.FileN.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.FileN.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // DirectoryName
            // 
            this.DirectoryName.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DirectoryName.Location = new System.Drawing.Point(8, 108);
            this.DirectoryName.Margin = new System.Windows.Forms.Padding(1);
            this.DirectoryName.Name = "DirectoryName";
            this.DirectoryName.ReadOnly = true;
            this.DirectoryName.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.DirectoryName.Size = new System.Drawing.Size(289, 20);
            this.DirectoryName.TabIndex = 40;
            this.DirectoryName.Text = "C:\\\\";
            // 
            // FindPath
            // 
            this.FindPath.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FindPath.Location = new System.Drawing.Point(221, 87);
            this.FindPath.Margin = new System.Windows.Forms.Padding(1);
            this.FindPath.Name = "FindPath";
            this.FindPath.Size = new System.Drawing.Size(73, 21);
            this.FindPath.TabIndex = 41;
            this.FindPath.Text = "Set path ...";
            this.FindPath.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.FindPath.UseVisualStyleBackColor = true;
            this.FindPath.Click += new System.EventHandler(this.FindPath_Click);
            // 
            // label70
            // 
            this.label70.AutoSize = true;
            this.label70.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label70.Location = new System.Drawing.Point(4, 62);
            this.label70.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label70.Name = "label70";
            this.label70.Size = new System.Drawing.Size(53, 14);
            this.label70.TabIndex = 151;
            this.label70.Text = "File Name";
            // 
            // FileName
            // 
            this.FileName.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileName.Location = new System.Drawing.Point(63, 60);
            this.FileName.Margin = new System.Windows.Forms.Padding(1);
            this.FileName.Name = "FileName";
            this.FileName.Size = new System.Drawing.Size(224, 20);
            this.FileName.TabIndex = 150;
            this.FileName.Text = "Test";
            // 
            // Panel_Files
            // 
            this.Panel_Files.Controls.Add(this.FullFileName);
            this.Panel_Files.Controls.Add(this.FileName);
            this.Panel_Files.Controls.Add(this.label70);
            this.Panel_Files.Controls.Add(this.FindPath);
            this.Panel_Files.Controls.Add(this.DirectoryName);
            this.Panel_Files.Controls.Add(this.FileN);
            this.Panel_Files.Controls.Add(this.BaseName);
            this.Panel_Files.Controls.Add(this.st_fileN);
            this.Panel_Files.Controls.Add(this.st_PathName);
            this.Panel_Files.Controls.Add(this.st_BaseName);
            this.Panel_Files.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Panel_Files.Location = new System.Drawing.Point(10, 588);
            this.Panel_Files.Name = "Panel_Files";
            this.Panel_Files.Size = new System.Drawing.Size(300, 138);
            this.Panel_Files.TabIndex = 115;
            this.Panel_Files.TabStop = false;
            this.Panel_Files.Text = "Next file";
            // 
            // FullFileName
            // 
            this.FullFileName.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FullFileName.Location = new System.Drawing.Point(7, 17);
            this.FullFileName.Margin = new System.Windows.Forms.Padding(1);
            this.FullFileName.Name = "FullFileName";
            this.FullFileName.ReadOnly = true;
            this.FullFileName.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.FullFileName.Size = new System.Drawing.Size(289, 20);
            this.FullFileName.TabIndex = 153;
            this.FullFileName.Text = "C:\\\\";
            // 
            // ImageIteration
            // 
            this.ImageIteration.Controls.Add(this.LoopButton);
            this.ImageIteration.Controls.Add(this.label88);
            this.ImageIteration.Controls.Add(this.label2);
            this.ImageIteration.Controls.Add(this.ImageInterval);
            this.ImageIteration.Controls.Add(this.NImages);
            this.ImageIteration.Controls.Add(this.label7);
            this.ImageIteration.Controls.Add(this.CurrentImage);
            this.ImageIteration.Controls.Add(this.ETime2);
            this.ImageIteration.Controls.Add(this.label9);
            this.ImageIteration.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ImageIteration.Location = new System.Drawing.Point(10, 233);
            this.ImageIteration.Name = "ImageIteration";
            this.ImageIteration.Size = new System.Drawing.Size(300, 62);
            this.ImageIteration.TabIndex = 333;
            this.ImageIteration.TabStop = false;
            this.ImageIteration.Text = "Loop image sequence";
            // 
            // LoopButton
            // 
            this.LoopButton.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoopButton.Location = new System.Drawing.Point(218, 32);
            this.LoopButton.Margin = new System.Windows.Forms.Padding(1);
            this.LoopButton.Name = "LoopButton";
            this.LoopButton.Size = new System.Drawing.Size(80, 26);
            this.LoopButton.TabIndex = 340;
            this.LoopButton.Tag = "LoopButton";
            this.LoopButton.Text = "LOOP";
            this.LoopButton.UseVisualStyleBackColor = true;
            this.LoopButton.Click += new System.EventHandler(this.LoopButton_Click);
            // 
            // label88
            // 
            this.label88.AutoSize = true;
            this.label88.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label88.Location = new System.Drawing.Point(8, 33);
            this.label88.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label88.Name = "label88";
            this.label88.Size = new System.Drawing.Size(49, 15);
            this.label88.TabIndex = 337;
            this.label88.Text = "Images";
            // 
            // tb_Pparameters
            // 
            this.tb_Pparameters.BackColor = System.Drawing.SystemColors.Control;
            this.tb_Pparameters.Controls.Add(this.label97);
            this.tb_Pparameters.Controls.Add(this.FreqDivBox);
            this.tb_Pparameters.Controls.Add(this.label96);
            this.tb_Pparameters.Controls.Add(this.StartPointBox);
            this.tb_Pparameters.Controls.Add(this.st_mode);
            this.tb_Pparameters.Controls.Add(this.PQMode_Pulldown);
            this.tb_Pparameters.Controls.Add(this.Binning_setting);
            this.tb_Pparameters.Controls.Add(this.st_nTimeP);
            this.tb_Pparameters.Controls.Add(this.Resolution_Pulldown);
            this.tb_Pparameters.Controls.Add(this.sync2Group);
            this.tb_Pparameters.Controls.Add(this.st_tp);
            this.tb_Pparameters.Controls.Add(this.NTimePoints);
            this.tb_Pparameters.Controls.Add(this.groupBox7);
            this.tb_Pparameters.Controls.Add(this.groupBox6);
            this.tb_Pparameters.Controls.Add(this.groupBox5);
            this.tb_Pparameters.Controls.Add(this.st_binning);
            this.tb_Pparameters.Location = new System.Drawing.Point(4, 23);
            this.tb_Pparameters.Name = "tb_Pparameters";
            this.tb_Pparameters.Padding = new System.Windows.Forms.Padding(3);
            this.tb_Pparameters.Size = new System.Drawing.Size(314, 337);
            this.tb_Pparameters.TabIndex = 2;
            this.tb_Pparameters.Text = "Photon counting";
            // 
            // label97
            // 
            this.label97.AutoSize = true;
            this.label97.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label97.Location = new System.Drawing.Point(161, 279);
            this.label97.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label97.Name = "label97";
            this.label97.Size = new System.Drawing.Size(46, 14);
            this.label97.TabIndex = 269;
            this.label97.Text = "Freq div";
            // 
            // FreqDivBox
            // 
            this.FreqDivBox.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FreqDivBox.Location = new System.Drawing.Point(163, 295);
            this.FreqDivBox.Margin = new System.Windows.Forms.Padding(1);
            this.FreqDivBox.Name = "FreqDivBox";
            this.FreqDivBox.Size = new System.Drawing.Size(40, 20);
            this.FreqDivBox.TabIndex = 270;
            this.FreqDivBox.Text = "1";
            this.FreqDivBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.FreqDivBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // label96
            // 
            this.label96.AutoSize = true;
            this.label96.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label96.Location = new System.Drawing.Point(236, 230);
            this.label96.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label96.Name = "label96";
            this.label96.Size = new System.Drawing.Size(56, 14);
            this.label96.TabIndex = 268;
            this.label96.Text = "Start point";
            // 
            // StartPointBox
            // 
            this.StartPointBox.Location = new System.Drawing.Point(246, 245);
            this.StartPointBox.Name = "StartPointBox";
            this.StartPointBox.Size = new System.Drawing.Size(40, 20);
            this.StartPointBox.TabIndex = 267;
            this.StartPointBox.Text = "0";
            this.StartPointBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.StartPointBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // st_mode
            // 
            this.st_mode.AutoSize = true;
            this.st_mode.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_mode.Location = new System.Drawing.Point(10, 279);
            this.st_mode.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_mode.Name = "st_mode";
            this.st_mode.Size = new System.Drawing.Size(33, 14);
            this.st_mode.TabIndex = 266;
            this.st_mode.Text = "Mode";
            // 
            // PQMode_Pulldown
            // 
            this.PQMode_Pulldown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PQMode_Pulldown.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PQMode_Pulldown.FormattingEnabled = true;
            this.PQMode_Pulldown.Items.AddRange(new object[] {
            "T3 (> 40 MHz)",
            "T2 (< 40 MHz)"});
            this.PQMode_Pulldown.Location = new System.Drawing.Point(12, 294);
            this.PQMode_Pulldown.Name = "PQMode_Pulldown";
            this.PQMode_Pulldown.Size = new System.Drawing.Size(95, 22);
            this.PQMode_Pulldown.TabIndex = 265;
            this.PQMode_Pulldown.SelectedIndexChanged += new System.EventHandler(this.Res_setting_SelectedIndexChanged);
            // 
            // Binning_setting
            // 
            this.Binning_setting.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Binning_setting.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Binning_setting.FormattingEnabled = true;
            this.Binning_setting.Items.AddRange(new object[] {
            "0 (25 ps)",
            "1 (50ps)",
            "2 (100 ps)",
            "3 (200 ps)",
            "4 (400 ps)",
            "5 (800 ps)"});
            this.Binning_setting.Location = new System.Drawing.Point(125, 245);
            this.Binning_setting.Name = "Binning_setting";
            this.Binning_setting.Size = new System.Drawing.Size(95, 22);
            this.Binning_setting.TabIndex = 264;
            this.Binning_setting.SelectedIndexChanged += new System.EventHandler(this.Res_setting_SelectedIndexChanged);
            // 
            // st_nTimeP
            // 
            this.st_nTimeP.AutoSize = true;
            this.st_nTimeP.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_nTimeP.Location = new System.Drawing.Point(10, 230);
            this.st_nTimeP.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_nTimeP.Name = "st_nTimeP";
            this.st_nTimeP.Size = new System.Drawing.Size(67, 14);
            this.st_nTimeP.TabIndex = 263;
            this.st_nTimeP.Text = "#Time points";
            // 
            // Resolution_Pulldown
            // 
            this.Resolution_Pulldown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Resolution_Pulldown.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Resolution_Pulldown.FormattingEnabled = true;
            this.Resolution_Pulldown.Items.AddRange(new object[] {
            "64",
            "128",
            "256",
            "512",
            "1024",
            "2048",
            "4096"});
            this.Resolution_Pulldown.Location = new System.Drawing.Point(12, 245);
            this.Resolution_Pulldown.Name = "Resolution_Pulldown";
            this.Resolution_Pulldown.Size = new System.Drawing.Size(95, 22);
            this.Resolution_Pulldown.TabIndex = 262;
            this.Resolution_Pulldown.SelectedIndexChanged += new System.EventHandler(this.Res_setting_SelectedIndexChanged);
            // 
            // sync2Group
            // 
            this.sync2Group.Controls.Add(this.label80);
            this.sync2Group.Controls.Add(this.label81);
            this.sync2Group.Controls.Add(this.label82);
            this.sync2Group.Controls.Add(this.label83);
            this.sync2Group.Controls.Add(this.sync_offset2);
            this.sync2Group.Controls.Add(this.label84);
            this.sync2Group.Controls.Add(this.sync_threshold2);
            this.sync2Group.Controls.Add(this.label85);
            this.sync2Group.Controls.Add(this.sync_zc_level2);
            this.sync2Group.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sync2Group.Location = new System.Drawing.Point(156, 12);
            this.sync2Group.Name = "sync2Group";
            this.sync2Group.Size = new System.Drawing.Size(125, 86);
            this.sync2Group.TabIndex = 261;
            this.sync2Group.TabStop = false;
            this.sync2Group.Text = "Sync 2";
            // 
            // label80
            // 
            this.label80.AutoSize = true;
            this.label80.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label80.Location = new System.Drawing.Point(92, 59);
            this.label80.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label80.Name = "label80";
            this.label80.Size = new System.Drawing.Size(18, 13);
            this.label80.TabIndex = 83;
            this.label80.Text = "ps";
            // 
            // label81
            // 
            this.label81.AutoSize = true;
            this.label81.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label81.Location = new System.Drawing.Point(92, 40);
            this.label81.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label81.Name = "label81";
            this.label81.Size = new System.Drawing.Size(22, 13);
            this.label81.TabIndex = 82;
            this.label81.Text = "mV";
            // 
            // label82
            // 
            this.label82.AutoSize = true;
            this.label82.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label82.Location = new System.Drawing.Point(92, 21);
            this.label82.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label82.Name = "label82";
            this.label82.Size = new System.Drawing.Size(22, 13);
            this.label82.TabIndex = 81;
            this.label82.Text = "mV";
            // 
            // label83
            // 
            this.label83.AutoSize = true;
            this.label83.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label83.Location = new System.Drawing.Point(8, 59);
            this.label83.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label83.Name = "label83";
            this.label83.Size = new System.Drawing.Size(35, 13);
            this.label83.TabIndex = 80;
            this.label83.Text = "Offset";
            // 
            // sync_offset2
            // 
            this.sync_offset2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sync_offset2.Location = new System.Drawing.Point(43, 56);
            this.sync_offset2.Margin = new System.Windows.Forms.Padding(1);
            this.sync_offset2.Name = "sync_offset2";
            this.sync_offset2.Size = new System.Drawing.Size(46, 20);
            this.sync_offset2.TabIndex = 79;
            this.sync_offset2.Text = "7000";
            this.sync_offset2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.sync_offset2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // label84
            // 
            this.label84.AutoSize = true;
            this.label84.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label84.Location = new System.Drawing.Point(13, 40);
            this.label84.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label84.Name = "label84";
            this.label84.Size = new System.Drawing.Size(21, 13);
            this.label84.TabIndex = 78;
            this.label84.Text = "ZC";
            // 
            // sync_threshold2
            // 
            this.sync_threshold2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sync_threshold2.Location = new System.Drawing.Point(43, 17);
            this.sync_threshold2.Margin = new System.Windows.Forms.Padding(1);
            this.sync_threshold2.Name = "sync_threshold2";
            this.sync_threshold2.Size = new System.Drawing.Size(46, 20);
            this.sync_threshold2.TabIndex = 76;
            this.sync_threshold2.Text = "50";
            this.sync_threshold2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.sync_threshold2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // label85
            // 
            this.label85.AutoSize = true;
            this.label85.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label85.Location = new System.Drawing.Point(4, 21);
            this.label85.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label85.Name = "label85";
            this.label85.Size = new System.Drawing.Size(40, 13);
            this.label85.TabIndex = 76;
            this.label85.Text = "Thresh";
            // 
            // sync_zc_level2
            // 
            this.sync_zc_level2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sync_zc_level2.Location = new System.Drawing.Point(43, 37);
            this.sync_zc_level2.Margin = new System.Windows.Forms.Padding(1);
            this.sync_zc_level2.Name = "sync_zc_level2";
            this.sync_zc_level2.Size = new System.Drawing.Size(46, 20);
            this.sync_zc_level2.TabIndex = 77;
            this.sync_zc_level2.Text = "0";
            this.sync_zc_level2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.sync_zc_level2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // st_tp
            // 
            this.st_tp.AutoSize = true;
            this.st_tp.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_tp.Location = new System.Drawing.Point(236, 278);
            this.st_tp.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_tp.Name = "st_tp";
            this.st_tp.Size = new System.Drawing.Size(67, 14);
            this.st_tp.TabIndex = 259;
            this.st_tp.Text = "#Time points";
            // 
            // NTimePoints
            // 
            this.NTimePoints.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NTimePoints.Location = new System.Drawing.Point(245, 295);
            this.NTimePoints.Margin = new System.Windows.Forms.Padding(1);
            this.NTimePoints.Name = "NTimePoints";
            this.NTimePoints.Size = new System.Drawing.Size(40, 20);
            this.NTimePoints.TabIndex = 260;
            this.NTimePoints.Text = "128";
            this.NTimePoints.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.NTimePoints.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label26);
            this.groupBox7.Controls.Add(this.label24);
            this.groupBox7.Controls.Add(this.label25);
            this.groupBox7.Controls.Add(this.label23);
            this.groupBox7.Controls.Add(this.sync_offset);
            this.groupBox7.Controls.Add(this.label22);
            this.groupBox7.Controls.Add(this.sync_threshold);
            this.groupBox7.Controls.Add(this.label21);
            this.groupBox7.Controls.Add(this.sync_zc_level);
            this.groupBox7.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox7.Location = new System.Drawing.Point(18, 12);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(125, 86);
            this.groupBox7.TabIndex = 258;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Sync";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label26.Location = new System.Drawing.Point(92, 59);
            this.label26.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(18, 13);
            this.label26.TabIndex = 83;
            this.label26.Text = "ps";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label24.Location = new System.Drawing.Point(92, 40);
            this.label24.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(22, 13);
            this.label24.TabIndex = 82;
            this.label24.Text = "mV";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label25.Location = new System.Drawing.Point(92, 21);
            this.label25.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(22, 13);
            this.label25.TabIndex = 81;
            this.label25.Text = "mV";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label23.Location = new System.Drawing.Point(8, 59);
            this.label23.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(35, 13);
            this.label23.TabIndex = 80;
            this.label23.Text = "Offset";
            // 
            // sync_offset
            // 
            this.sync_offset.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sync_offset.Location = new System.Drawing.Point(43, 56);
            this.sync_offset.Margin = new System.Windows.Forms.Padding(1);
            this.sync_offset.Name = "sync_offset";
            this.sync_offset.Size = new System.Drawing.Size(46, 20);
            this.sync_offset.TabIndex = 79;
            this.sync_offset.Text = "7000";
            this.sync_offset.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.sync_offset.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.Location = new System.Drawing.Point(13, 40);
            this.label22.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(21, 13);
            this.label22.TabIndex = 78;
            this.label22.Text = "ZC";
            // 
            // sync_threshold
            // 
            this.sync_threshold.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sync_threshold.Location = new System.Drawing.Point(43, 17);
            this.sync_threshold.Margin = new System.Windows.Forms.Padding(1);
            this.sync_threshold.Name = "sync_threshold";
            this.sync_threshold.Size = new System.Drawing.Size(46, 20);
            this.sync_threshold.TabIndex = 76;
            this.sync_threshold.Text = "50";
            this.sync_threshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.sync_threshold.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.Location = new System.Drawing.Point(4, 21);
            this.label21.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(40, 13);
            this.label21.TabIndex = 76;
            this.label21.Text = "Thresh";
            // 
            // sync_zc_level
            // 
            this.sync_zc_level.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sync_zc_level.Location = new System.Drawing.Point(43, 37);
            this.sync_zc_level.Margin = new System.Windows.Forms.Padding(1);
            this.sync_zc_level.Name = "sync_zc_level";
            this.sync_zc_level.Size = new System.Drawing.Size(46, 20);
            this.sync_zc_level.TabIndex = 77;
            this.sync_zc_level.Text = "0";
            this.sync_zc_level.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.sync_zc_level.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label30);
            this.groupBox6.Controls.Add(this.label31);
            this.groupBox6.Controls.Add(this.label32);
            this.groupBox6.Controls.Add(this.label33);
            this.groupBox6.Controls.Add(this.ch_offset1);
            this.groupBox6.Controls.Add(this.label34);
            this.groupBox6.Controls.Add(this.ch_threshold1);
            this.groupBox6.Controls.Add(this.label35);
            this.groupBox6.Controls.Add(this.label29);
            this.groupBox6.Controls.Add(this.ch_zc_level1);
            this.groupBox6.Controls.Add(this.resolution);
            this.groupBox6.Controls.Add(this.label28);
            this.groupBox6.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox6.Location = new System.Drawing.Point(19, 107);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(125, 114);
            this.groupBox6.TabIndex = 257;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Channel1";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label30.Location = new System.Drawing.Point(92, 59);
            this.label30.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(18, 13);
            this.label30.TabIndex = 92;
            this.label30.Text = "ps";
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label31.Location = new System.Drawing.Point(92, 40);
            this.label31.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(22, 13);
            this.label31.TabIndex = 91;
            this.label31.Text = "mV";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label32.Location = new System.Drawing.Point(92, 21);
            this.label32.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(22, 13);
            this.label32.TabIndex = 90;
            this.label32.Text = "mV";
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label33.Location = new System.Drawing.Point(8, 59);
            this.label33.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(35, 13);
            this.label33.TabIndex = 89;
            this.label33.Text = "Offset";
            // 
            // ch_offset1
            // 
            this.ch_offset1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ch_offset1.Location = new System.Drawing.Point(43, 56);
            this.ch_offset1.Margin = new System.Windows.Forms.Padding(1);
            this.ch_offset1.Name = "ch_offset1";
            this.ch_offset1.Size = new System.Drawing.Size(46, 20);
            this.ch_offset1.TabIndex = 88;
            this.ch_offset1.Text = "0";
            this.ch_offset1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ch_offset1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label34.Location = new System.Drawing.Point(13, 40);
            this.label34.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(21, 13);
            this.label34.TabIndex = 87;
            this.label34.Text = "ZC";
            // 
            // ch_threshold1
            // 
            this.ch_threshold1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ch_threshold1.Location = new System.Drawing.Point(43, 17);
            this.ch_threshold1.Margin = new System.Windows.Forms.Padding(1);
            this.ch_threshold1.Name = "ch_threshold1";
            this.ch_threshold1.Size = new System.Drawing.Size(46, 20);
            this.ch_threshold1.TabIndex = 84;
            this.ch_threshold1.Text = "50";
            this.ch_threshold1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ch_threshold1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label35.Location = new System.Drawing.Point(4, 21);
            this.label35.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(40, 13);
            this.label35.TabIndex = 85;
            this.label35.Text = "Thresh";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label29.Location = new System.Drawing.Point(98, 86);
            this.label29.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(18, 13);
            this.label29.TabIndex = 251;
            this.label29.Text = "ps";
            // 
            // ch_zc_level1
            // 
            this.ch_zc_level1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ch_zc_level1.Location = new System.Drawing.Point(43, 37);
            this.ch_zc_level1.Margin = new System.Windows.Forms.Padding(1);
            this.ch_zc_level1.Name = "ch_zc_level1";
            this.ch_zc_level1.Size = new System.Drawing.Size(46, 20);
            this.ch_zc_level1.TabIndex = 86;
            this.ch_zc_level1.Text = "0";
            this.ch_zc_level1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ch_zc_level1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // resolution
            // 
            this.resolution.Enabled = false;
            this.resolution.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resolution.Location = new System.Drawing.Point(57, 82);
            this.resolution.Margin = new System.Windows.Forms.Padding(1);
            this.resolution.Name = "resolution";
            this.resolution.Size = new System.Drawing.Size(40, 20);
            this.resolution.TabIndex = 255;
            this.resolution.Text = "250";
            this.resolution.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label28.Location = new System.Drawing.Point(3, 85);
            this.label28.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(57, 13);
            this.label28.TabIndex = 254;
            this.label28.Text = "Resolution";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.resolution2);
            this.groupBox5.Controls.Add(this.label87);
            this.groupBox5.Controls.Add(this.label86);
            this.groupBox5.Controls.Add(this.label36);
            this.groupBox5.Controls.Add(this.label37);
            this.groupBox5.Controls.Add(this.label38);
            this.groupBox5.Controls.Add(this.label39);
            this.groupBox5.Controls.Add(this.ch_offset2);
            this.groupBox5.Controls.Add(this.label40);
            this.groupBox5.Controls.Add(this.ch_threshold2);
            this.groupBox5.Controls.Add(this.label41);
            this.groupBox5.Controls.Add(this.ch_zc_level2);
            this.groupBox5.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox5.Location = new System.Drawing.Point(157, 107);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(125, 114);
            this.groupBox5.TabIndex = 256;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Channel 2";
            // 
            // resolution2
            // 
            this.resolution2.Enabled = false;
            this.resolution2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resolution2.Location = new System.Drawing.Point(57, 82);
            this.resolution2.Margin = new System.Windows.Forms.Padding(1);
            this.resolution2.Name = "resolution2";
            this.resolution2.Size = new System.Drawing.Size(40, 20);
            this.resolution2.TabIndex = 263;
            this.resolution2.Text = "250";
            this.resolution2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label87
            // 
            this.label87.AutoSize = true;
            this.label87.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label87.Location = new System.Drawing.Point(3, 85);
            this.label87.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label87.Name = "label87";
            this.label87.Size = new System.Drawing.Size(57, 13);
            this.label87.TabIndex = 256;
            this.label87.Text = "Resolution";
            // 
            // label86
            // 
            this.label86.AutoSize = true;
            this.label86.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label86.Location = new System.Drawing.Point(98, 86);
            this.label86.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label86.Name = "label86";
            this.label86.Size = new System.Drawing.Size(18, 13);
            this.label86.TabIndex = 262;
            this.label86.Text = "ps";
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label36.Location = new System.Drawing.Point(92, 59);
            this.label36.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(18, 13);
            this.label36.TabIndex = 92;
            this.label36.Text = "ps";
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label37.Location = new System.Drawing.Point(92, 40);
            this.label37.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(22, 13);
            this.label37.TabIndex = 91;
            this.label37.Text = "mV";
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label38.Location = new System.Drawing.Point(92, 21);
            this.label38.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(22, 13);
            this.label38.TabIndex = 90;
            this.label38.Text = "mV";
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label39.Location = new System.Drawing.Point(8, 59);
            this.label39.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(35, 13);
            this.label39.TabIndex = 89;
            this.label39.Text = "Offset";
            // 
            // ch_offset2
            // 
            this.ch_offset2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ch_offset2.Location = new System.Drawing.Point(43, 56);
            this.ch_offset2.Margin = new System.Windows.Forms.Padding(1);
            this.ch_offset2.Name = "ch_offset2";
            this.ch_offset2.Size = new System.Drawing.Size(46, 20);
            this.ch_offset2.TabIndex = 88;
            this.ch_offset2.Text = "0";
            this.ch_offset2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ch_offset2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label40.Location = new System.Drawing.Point(13, 40);
            this.label40.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(21, 13);
            this.label40.TabIndex = 87;
            this.label40.Text = "ZC";
            // 
            // ch_threshold2
            // 
            this.ch_threshold2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ch_threshold2.Location = new System.Drawing.Point(43, 17);
            this.ch_threshold2.Margin = new System.Windows.Forms.Padding(1);
            this.ch_threshold2.Name = "ch_threshold2";
            this.ch_threshold2.Size = new System.Drawing.Size(46, 20);
            this.ch_threshold2.TabIndex = 84;
            this.ch_threshold2.Text = "50";
            this.ch_threshold2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ch_threshold2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label41.Location = new System.Drawing.Point(4, 21);
            this.label41.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(40, 13);
            this.label41.TabIndex = 85;
            this.label41.Text = "Thresh";
            // 
            // ch_zc_level2
            // 
            this.ch_zc_level2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ch_zc_level2.Location = new System.Drawing.Point(43, 37);
            this.ch_zc_level2.Margin = new System.Windows.Forms.Padding(1);
            this.ch_zc_level2.Name = "ch_zc_level2";
            this.ch_zc_level2.Size = new System.Drawing.Size(46, 20);
            this.ch_zc_level2.TabIndex = 86;
            this.ch_zc_level2.Text = "0";
            this.ch_zc_level2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ch_zc_level2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PQ_KeyDown);
            // 
            // st_binning
            // 
            this.st_binning.AutoSize = true;
            this.st_binning.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_binning.Location = new System.Drawing.Point(124, 230);
            this.st_binning.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_binning.Name = "st_binning";
            this.st_binning.Size = new System.Drawing.Size(57, 14);
            this.st_binning.TabIndex = 252;
            this.st_binning.Text = "Resolution";
            // 
            // tbScanParam
            // 
            this.tbScanParam.BackColor = System.Drawing.SystemColors.Control;
            this.tbScanParam.Controls.Add(this.SineWaveScanning_CB);
            this.tbScanParam.Controls.Add(this.SwitchXY_CB);
            this.tbScanParam.Controls.Add(this.FlipY_CB);
            this.tbScanParam.Controls.Add(this.FlipX_CB);
            this.tbScanParam.Controls.Add(this.MeasuredLineCorrection);
            this.tbScanParam.Controls.Add(this.label95);
            this.tbScanParam.Controls.Add(this.LineTimeCorrection);
            this.tbScanParam.Controls.Add(this.ScanDelay);
            this.tbScanParam.Controls.Add(this.ScanFraction);
            this.tbScanParam.Controls.Add(this.FillFraction);
            this.tbScanParam.Controls.Add(this.MaxRangeY);
            this.tbScanParam.Controls.Add(this.MaxRangeX);
            this.tbScanParam.Controls.Add(this.pixelTime);
            this.tbScanParam.Controls.Add(this.MsPerLine);
            this.tbScanParam.Controls.Add(this.pixelsPerLine);
            this.tbScanParam.Controls.Add(this.linesPerFrame);
            this.tbScanParam.Controls.Add(this.label94);
            this.tbScanParam.Controls.Add(this.BiDirecCB);
            this.tbScanParam.Controls.Add(this.label89);
            this.tbScanParam.Controls.Add(this.pb32);
            this.tbScanParam.Controls.Add(this.pb1024);
            this.tbScanParam.Controls.Add(this.pb512);
            this.tbScanParam.Controls.Add(this.pb64);
            this.tbScanParam.Controls.Add(this.pb256);
            this.tbScanParam.Controls.Add(this.pb128);
            this.tbScanParam.Controls.Add(this.label61);
            this.tbScanParam.Controls.Add(this.label10);
            this.tbScanParam.Controls.Add(this.label11);
            this.tbScanParam.Controls.Add(this.label1);
            this.tbScanParam.Controls.Add(this.st_us);
            this.tbScanParam.Controls.Add(this.st_LineTime);
            this.tbScanParam.Controls.Add(this.st_PixelsPerLine);
            this.tbScanParam.Controls.Add(this.st_LinesPerFrame);
            this.tbScanParam.Controls.Add(this.st_ms);
            this.tbScanParam.Controls.Add(this.label8);
            this.tbScanParam.Controls.Add(this.st_ScanFraction);
            this.tbScanParam.Controls.Add(this.st_ScanDelay);
            this.tbScanParam.Controls.Add(this.st_FillFraction);
            this.tbScanParam.Controls.Add(this.AdvancedCheck);
            this.tbScanParam.Location = new System.Drawing.Point(4, 23);
            this.tbScanParam.Name = "tbScanParam";
            this.tbScanParam.Padding = new System.Windows.Forms.Padding(3);
            this.tbScanParam.Size = new System.Drawing.Size(314, 337);
            this.tbScanParam.TabIndex = 1;
            this.tbScanParam.Text = "Scan Param";
            // 
            // SineWaveScanning_CB
            // 
            this.SineWaveScanning_CB.AutoSize = true;
            this.SineWaveScanning_CB.Enabled = false;
            this.SineWaveScanning_CB.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SineWaveScanning_CB.Location = new System.Drawing.Point(150, 217);
            this.SineWaveScanning_CB.Name = "SineWaveScanning_CB";
            this.SineWaveScanning_CB.Size = new System.Drawing.Size(152, 18);
            this.SineWaveScanning_CB.TabIndex = 309;
            this.SineWaveScanning_CB.Text = "Sinewave scanning (fast)";
            this.SineWaveScanning_CB.UseVisualStyleBackColor = true;
            this.SineWaveScanning_CB.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // SwitchXY_CB
            // 
            this.SwitchXY_CB.AutoSize = true;
            this.SwitchXY_CB.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SwitchXY_CB.Location = new System.Drawing.Point(15, 199);
            this.SwitchXY_CB.Name = "SwitchXY_CB";
            this.SwitchXY_CB.Size = new System.Drawing.Size(102, 18);
            this.SwitchXY_CB.TabIndex = 308;
            this.SwitchXY_CB.Text = "Switch X and Y";
            this.SwitchXY_CB.UseVisualStyleBackColor = true;
            this.SwitchXY_CB.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // FlipY_CB
            // 
            this.FlipY_CB.AutoSize = true;
            this.FlipY_CB.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FlipY_CB.Location = new System.Drawing.Point(15, 235);
            this.FlipY_CB.Name = "FlipY_CB";
            this.FlipY_CB.Size = new System.Drawing.Size(76, 18);
            this.FlipY_CB.TabIndex = 307;
            this.FlipY_CB.Text = "Flip Y-axis";
            this.FlipY_CB.UseVisualStyleBackColor = true;
            this.FlipY_CB.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // FlipX_CB
            // 
            this.FlipX_CB.AutoSize = true;
            this.FlipX_CB.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FlipX_CB.Location = new System.Drawing.Point(15, 217);
            this.FlipX_CB.Name = "FlipX_CB";
            this.FlipX_CB.Size = new System.Drawing.Size(76, 18);
            this.FlipX_CB.TabIndex = 306;
            this.FlipX_CB.Text = "Flip X-axis";
            this.FlipX_CB.UseVisualStyleBackColor = true;
            this.FlipX_CB.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // MeasuredLineCorrection
            // 
            this.MeasuredLineCorrection.AutoSize = true;
            this.MeasuredLineCorrection.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MeasuredLineCorrection.Location = new System.Drawing.Point(163, 297);
            this.MeasuredLineCorrection.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.MeasuredLineCorrection.Name = "MeasuredLineCorrection";
            this.MeasuredLineCorrection.Size = new System.Drawing.Size(22, 14);
            this.MeasuredLineCorrection.TabIndex = 305;
            this.MeasuredLineCorrection.Text = "1.0";
            // 
            // label95
            // 
            this.label95.AutoSize = true;
            this.label95.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label95.Location = new System.Drawing.Point(96, 297);
            this.label95.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label95.Name = "label95";
            this.label95.Size = new System.Drawing.Size(55, 14);
            this.label95.TabIndex = 304;
            this.label95.Text = "Measured";
            // 
            // LineTimeCorrection
            // 
            this.LineTimeCorrection.Enabled = false;
            this.LineTimeCorrection.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LineTimeCorrection.Location = new System.Drawing.Point(159, 274);
            this.LineTimeCorrection.Margin = new System.Windows.Forms.Padding(1);
            this.LineTimeCorrection.Name = "LineTimeCorrection";
            this.LineTimeCorrection.Size = new System.Drawing.Size(40, 20);
            this.LineTimeCorrection.TabIndex = 301;
            this.LineTimeCorrection.Text = "1.0";
            this.LineTimeCorrection.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.LineTimeCorrection.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // ScanDelay
            // 
            this.ScanDelay.Enabled = false;
            this.ScanDelay.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ScanDelay.Location = new System.Drawing.Point(227, 175);
            this.ScanDelay.Margin = new System.Windows.Forms.Padding(1);
            this.ScanDelay.Name = "ScanDelay";
            this.ScanDelay.Size = new System.Drawing.Size(50, 20);
            this.ScanDelay.TabIndex = 116;
            this.ScanDelay.Text = "0.100";
            this.ScanDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ScanDelay.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // ScanFraction
            // 
            this.ScanFraction.Enabled = false;
            this.ScanFraction.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ScanFraction.Location = new System.Drawing.Point(227, 155);
            this.ScanFraction.Margin = new System.Windows.Forms.Padding(1);
            this.ScanFraction.Name = "ScanFraction";
            this.ScanFraction.Size = new System.Drawing.Size(50, 20);
            this.ScanFraction.TabIndex = 118;
            this.ScanFraction.Text = "0.900";
            this.ScanFraction.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ScanFraction.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // FillFraction
            // 
            this.FillFraction.Enabled = false;
            this.FillFraction.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FillFraction.Location = new System.Drawing.Point(227, 135);
            this.FillFraction.Margin = new System.Windows.Forms.Padding(1);
            this.FillFraction.Name = "FillFraction";
            this.FillFraction.Size = new System.Drawing.Size(50, 20);
            this.FillFraction.TabIndex = 114;
            this.FillFraction.Text = "0.800";
            this.FillFraction.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.FillFraction.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // MaxRangeY
            // 
            this.MaxRangeY.Enabled = false;
            this.MaxRangeY.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaxRangeY.Location = new System.Drawing.Point(227, 115);
            this.MaxRangeY.Margin = new System.Windows.Forms.Padding(1);
            this.MaxRangeY.Name = "MaxRangeY";
            this.MaxRangeY.Size = new System.Drawing.Size(50, 20);
            this.MaxRangeY.TabIndex = 286;
            this.MaxRangeY.Text = "5";
            this.MaxRangeY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MaxRangeY.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // MaxRangeX
            // 
            this.MaxRangeX.Enabled = false;
            this.MaxRangeX.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaxRangeX.Location = new System.Drawing.Point(227, 95);
            this.MaxRangeX.Margin = new System.Windows.Forms.Padding(1);
            this.MaxRangeX.Name = "MaxRangeX";
            this.MaxRangeX.Size = new System.Drawing.Size(50, 20);
            this.MaxRangeX.TabIndex = 285;
            this.MaxRangeX.Text = "5";
            this.MaxRangeX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MaxRangeX.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // pixelTime
            // 
            this.pixelTime.Enabled = false;
            this.pixelTime.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pixelTime.Location = new System.Drawing.Point(65, 161);
            this.pixelTime.Margin = new System.Windows.Forms.Padding(1);
            this.pixelTime.Name = "pixelTime";
            this.pixelTime.Size = new System.Drawing.Size(50, 20);
            this.pixelTime.TabIndex = 127;
            this.pixelTime.Text = "12.500";
            this.pixelTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MsPerLine
            // 
            this.MsPerLine.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MsPerLine.Location = new System.Drawing.Point(72, 141);
            this.MsPerLine.Margin = new System.Windows.Forms.Padding(1);
            this.MsPerLine.Name = "MsPerLine";
            this.MsPerLine.Size = new System.Drawing.Size(40, 20);
            this.MsPerLine.TabIndex = 124;
            this.MsPerLine.Text = "2";
            this.MsPerLine.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MsPerLine.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // pixelsPerLine
            // 
            this.pixelsPerLine.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pixelsPerLine.Location = new System.Drawing.Point(72, 96);
            this.pixelsPerLine.Margin = new System.Windows.Forms.Padding(1);
            this.pixelsPerLine.Name = "pixelsPerLine";
            this.pixelsPerLine.ReadOnly = true;
            this.pixelsPerLine.Size = new System.Drawing.Size(40, 20);
            this.pixelsPerLine.TabIndex = 121;
            this.pixelsPerLine.Text = "128";
            this.pixelsPerLine.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.pixelsPerLine.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // linesPerFrame
            // 
            this.linesPerFrame.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linesPerFrame.Location = new System.Drawing.Point(72, 116);
            this.linesPerFrame.Margin = new System.Windows.Forms.Padding(1);
            this.linesPerFrame.Name = "linesPerFrame";
            this.linesPerFrame.ReadOnly = true;
            this.linesPerFrame.Size = new System.Drawing.Size(40, 20);
            this.linesPerFrame.TabIndex = 120;
            this.linesPerFrame.Text = "128";
            this.linesPerFrame.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.linesPerFrame.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // label94
            // 
            this.label94.AutoSize = true;
            this.label94.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label94.Location = new System.Drawing.Point(56, 276);
            this.label94.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label94.Name = "label94";
            this.label94.Size = new System.Drawing.Size(101, 14);
            this.label94.TabIndex = 303;
            this.label94.Text = "Line time correction";
            // 
            // BiDirecCB
            // 
            this.BiDirecCB.AutoSize = true;
            this.BiDirecCB.Enabled = false;
            this.BiDirecCB.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BiDirecCB.Location = new System.Drawing.Point(150, 199);
            this.BiDirecCB.Name = "BiDirecCB";
            this.BiDirecCB.Size = new System.Drawing.Size(115, 18);
            this.BiDirecCB.TabIndex = 293;
            this.BiDirecCB.Text = "Bi-directional scan";
            this.BiDirecCB.UseVisualStyleBackColor = true;
            this.BiDirecCB.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // label89
            // 
            this.label89.AutoSize = true;
            this.label89.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label89.Location = new System.Drawing.Point(142, 118);
            this.label89.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label89.Name = "label89";
            this.label89.Size = new System.Drawing.Size(85, 14);
            this.label89.TabIndex = 292;
            this.label89.Text = "Voltage range Y";
            // 
            // pb32
            // 
            this.pb32.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pb32.Location = new System.Drawing.Point(7, 4);
            this.pb32.Margin = new System.Windows.Forms.Padding(1);
            this.pb32.Name = "pb32";
            this.pb32.Size = new System.Drawing.Size(80, 23);
            this.pb32.TabIndex = 264;
            this.pb32.Text = "32 x 32";
            this.pb32.UseVisualStyleBackColor = true;
            this.pb32.Click += new System.EventHandler(this.pbPreset_general_Click);
            // 
            // pb1024
            // 
            this.pb1024.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pb1024.Location = new System.Drawing.Point(109, 53);
            this.pb1024.Margin = new System.Windows.Forms.Padding(1);
            this.pb1024.Name = "pb1024";
            this.pb1024.Size = new System.Drawing.Size(80, 23);
            this.pb1024.TabIndex = 263;
            this.pb1024.Text = "1024 x 1024";
            this.pb1024.UseVisualStyleBackColor = true;
            this.pb1024.Click += new System.EventHandler(this.pbPreset_general_Click);
            // 
            // pb512
            // 
            this.pb512.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pb512.Location = new System.Drawing.Point(109, 28);
            this.pb512.Margin = new System.Windows.Forms.Padding(1);
            this.pb512.Name = "pb512";
            this.pb512.Size = new System.Drawing.Size(80, 23);
            this.pb512.TabIndex = 262;
            this.pb512.Text = "512 x 512";
            this.pb512.UseVisualStyleBackColor = true;
            this.pb512.Click += new System.EventHandler(this.pbPreset_general_Click);
            // 
            // pb64
            // 
            this.pb64.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pb64.Location = new System.Drawing.Point(7, 28);
            this.pb64.Margin = new System.Windows.Forms.Padding(1);
            this.pb64.Name = "pb64";
            this.pb64.Size = new System.Drawing.Size(80, 23);
            this.pb64.TabIndex = 261;
            this.pb64.Text = "64 x 64";
            this.pb64.UseVisualStyleBackColor = true;
            this.pb64.Click += new System.EventHandler(this.pbPreset_general_Click);
            // 
            // pb256
            // 
            this.pb256.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pb256.Location = new System.Drawing.Point(109, 3);
            this.pb256.Margin = new System.Windows.Forms.Padding(1);
            this.pb256.Name = "pb256";
            this.pb256.Size = new System.Drawing.Size(80, 23);
            this.pb256.TabIndex = 260;
            this.pb256.Text = "256 x 256";
            this.pb256.UseVisualStyleBackColor = true;
            this.pb256.Click += new System.EventHandler(this.pbPreset_general_Click);
            // 
            // pb128
            // 
            this.pb128.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pb128.Location = new System.Drawing.Point(7, 53);
            this.pb128.Margin = new System.Windows.Forms.Padding(1);
            this.pb128.Name = "pb128";
            this.pb128.Size = new System.Drawing.Size(80, 23);
            this.pb128.TabIndex = 259;
            this.pb128.Text = "128 x 128";
            this.pb128.UseVisualStyleBackColor = true;
            this.pb128.Click += new System.EventHandler(this.pbPreset_general_Click);
            // 
            // label61
            // 
            this.label61.AutoSize = true;
            this.label61.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label61.Location = new System.Drawing.Point(142, 98);
            this.label61.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(84, 14);
            this.label61.TabIndex = 291;
            this.label61.Text = "Voltage range X";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(279, 117);
            this.label10.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(15, 14);
            this.label10.TabIndex = 288;
            this.label10.Text = "V";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(279, 101);
            this.label11.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(15, 14);
            this.label11.TabIndex = 287;
            this.label11.Text = "V";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(14, 164);
            this.label1.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 14);
            this.label1.TabIndex = 128;
            this.label1.Text = "Pixel time";
            // 
            // st_us
            // 
            this.st_us.AutoSize = true;
            this.st_us.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_us.Location = new System.Drawing.Point(114, 164);
            this.st_us.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_us.Name = "st_us";
            this.st_us.Size = new System.Drawing.Size(19, 14);
            this.st_us.TabIndex = 129;
            this.st_us.Text = "μs";
            // 
            // st_LineTime
            // 
            this.st_LineTime.AutoSize = true;
            this.st_LineTime.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_LineTime.Location = new System.Drawing.Point(21, 144);
            this.st_LineTime.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_LineTime.Name = "st_LineTime";
            this.st_LineTime.Size = new System.Drawing.Size(49, 14);
            this.st_LineTime.TabIndex = 125;
            this.st_LineTime.Text = "Line time";
            // 
            // st_PixelsPerLine
            // 
            this.st_PixelsPerLine.AutoSize = true;
            this.st_PixelsPerLine.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_PixelsPerLine.Location = new System.Drawing.Point(12, 99);
            this.st_PixelsPerLine.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_PixelsPerLine.Name = "st_PixelsPerLine";
            this.st_PixelsPerLine.Size = new System.Drawing.Size(58, 14);
            this.st_PixelsPerLine.TabIndex = 123;
            this.st_PixelsPerLine.Text = "Pixels/Line";
            // 
            // st_LinesPerFrame
            // 
            this.st_LinesPerFrame.AutoSize = true;
            this.st_LinesPerFrame.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_LinesPerFrame.Location = new System.Drawing.Point(6, 119);
            this.st_LinesPerFrame.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_LinesPerFrame.Name = "st_LinesPerFrame";
            this.st_LinesPerFrame.Size = new System.Drawing.Size(66, 14);
            this.st_LinesPerFrame.TabIndex = 122;
            this.st_LinesPerFrame.Text = "Lines/Frame";
            // 
            // st_ms
            // 
            this.st_ms.AutoSize = true;
            this.st_ms.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_ms.Location = new System.Drawing.Point(114, 144);
            this.st_ms.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_ms.Name = "st_ms";
            this.st_ms.Size = new System.Drawing.Size(21, 14);
            this.st_ms.TabIndex = 126;
            this.st_ms.Text = "ms";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(276, 180);
            this.label8.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(21, 14);
            this.label8.TabIndex = 119;
            this.label8.Text = "ms";
            // 
            // st_ScanFraction
            // 
            this.st_ScanFraction.AutoSize = true;
            this.st_ScanFraction.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_ScanFraction.Location = new System.Drawing.Point(154, 158);
            this.st_ScanFraction.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_ScanFraction.Name = "st_ScanFraction";
            this.st_ScanFraction.Size = new System.Drawing.Size(72, 14);
            this.st_ScanFraction.TabIndex = 117;
            this.st_ScanFraction.Text = "Scan fraction";
            // 
            // st_ScanDelay
            // 
            this.st_ScanDelay.AutoSize = true;
            this.st_ScanDelay.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_ScanDelay.Location = new System.Drawing.Point(163, 178);
            this.st_ScanDelay.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_ScanDelay.Name = "st_ScanDelay";
            this.st_ScanDelay.Size = new System.Drawing.Size(62, 14);
            this.st_ScanDelay.TabIndex = 115;
            this.st_ScanDelay.Text = "Scan Delay";
            // 
            // st_FillFraction
            // 
            this.st_FillFraction.AutoSize = true;
            this.st_FillFraction.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_FillFraction.Location = new System.Drawing.Point(167, 138);
            this.st_FillFraction.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_FillFraction.Name = "st_FillFraction";
            this.st_FillFraction.Size = new System.Drawing.Size(59, 14);
            this.st_FillFraction.TabIndex = 113;
            this.st_FillFraction.Text = "Fill fraction";
            // 
            // AdvancedCheck
            // 
            this.AdvancedCheck.AutoSize = true;
            this.AdvancedCheck.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AdvancedCheck.Location = new System.Drawing.Point(168, 78);
            this.AdvancedCheck.Name = "AdvancedCheck";
            this.AdvancedCheck.Size = new System.Drawing.Size(111, 18);
            this.AdvancedCheck.TabIndex = 109;
            this.AdvancedCheck.Text = "Advanced setting";
            this.AdvancedCheck.UseVisualStyleBackColor = true;
            this.AdvancedCheck.CheckedChanged += new System.EventHandler(this.AdvancedCheck_CheckedChanged);
            // 
            // tb_ScanParameters
            // 
            this.tb_ScanParameters.BackColor = System.Drawing.SystemColors.Control;
            this.tb_ScanParameters.Controls.Add(this.CurrentFOVY);
            this.tb_ScanParameters.Controls.Add(this.CurrentFOVX);
            this.tb_ScanParameters.Controls.Add(this.label54);
            this.tb_ScanParameters.Controls.Add(this.label90);
            this.tb_ScanParameters.Controls.Add(this.label92);
            this.tb_ScanParameters.Controls.Add(this.label93);
            this.tb_ScanParameters.Controls.Add(this.FieldOfViewY);
            this.tb_ScanParameters.Controls.Add(this.FieldOfViewX);
            this.tb_ScanParameters.Controls.Add(this.label68);
            this.tb_ScanParameters.Controls.Add(this.label65);
            this.tb_ScanParameters.Controls.Add(this.label63);
            this.tb_ScanParameters.Controls.Add(this.label64);
            this.tb_ScanParameters.Controls.Add(this.label27);
            this.tb_ScanParameters.Controls.Add(this.label12);
            this.tb_ScanParameters.Controls.Add(this.Objective_Pulldown);
            this.tb_ScanParameters.Controls.Add(this.NPixels_PulldownY);
            this.tb_ScanParameters.Controls.Add(this.NPixels_PulldownX);
            this.tb_ScanParameters.Controls.Add(this.MaxScanning);
            this.tb_ScanParameters.Controls.Add(this.ZeroAngle);
            this.tb_ScanParameters.Controls.Add(this.Rotation);
            this.tb_ScanParameters.Controls.Add(this.YOffset);
            this.tb_ScanParameters.Controls.Add(this.XOffset);
            this.tb_ScanParameters.Controls.Add(this.MirrorStep);
            this.tb_ScanParameters.Controls.Add(this.Zoom);
            this.tb_ScanParameters.Controls.Add(this.label76);
            this.tb_ScanParameters.Controls.Add(this.label75);
            this.tb_ScanParameters.Controls.Add(this.label71);
            this.tb_ScanParameters.Controls.Add(this.label69);
            this.tb_ScanParameters.Controls.Add(this.label73);
            this.tb_ScanParameters.Controls.Add(this.label74);
            this.tb_ScanParameters.Controls.Add(this.ZeroMirror);
            this.tb_ScanParameters.Controls.Add(this.label62);
            this.tb_ScanParameters.Controls.Add(this.SnapShotButton);
            this.tb_ScanParameters.Controls.Add(this.Zoom10);
            this.tb_ScanParameters.Controls.Add(this.ScanPosition);
            this.tb_ScanParameters.Controls.Add(this.label59);
            this.tb_ScanParameters.Controls.Add(this.label58);
            this.tb_ScanParameters.Controls.Add(this.MirrorYDown);
            this.tb_ScanParameters.Controls.Add(this.label57);
            this.tb_ScanParameters.Controls.Add(this.label56);
            this.tb_ScanParameters.Controls.Add(this.label55);
            this.tb_ScanParameters.Controls.Add(this.label52);
            this.tb_ScanParameters.Controls.Add(this.MirrorXUp);
            this.tb_ScanParameters.Controls.Add(this.MirrorXDown);
            this.tb_ScanParameters.Controls.Add(this.MirrorYUp);
            this.tb_ScanParameters.Controls.Add(this.Zoom100);
            this.tb_ScanParameters.Controls.Add(this.st_zoom);
            this.tb_ScanParameters.Controls.Add(this.ZoomP1);
            this.tb_ScanParameters.Controls.Add(this.Zoom1);
            this.tb_ScanParameters.Controls.Add(this.label6);
            this.tb_ScanParameters.Location = new System.Drawing.Point(4, 23);
            this.tb_ScanParameters.Name = "tb_ScanParameters";
            this.tb_ScanParameters.Padding = new System.Windows.Forms.Padding(3);
            this.tb_ScanParameters.Size = new System.Drawing.Size(314, 337);
            this.tb_ScanParameters.TabIndex = 0;
            this.tb_ScanParameters.Text = "Zoom/Loc";
            // 
            // CurrentFOVY
            // 
            this.CurrentFOVY.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentFOVY.Location = new System.Drawing.Point(241, 255);
            this.CurrentFOVY.Margin = new System.Windows.Forms.Padding(1);
            this.CurrentFOVY.Name = "CurrentFOVY";
            this.CurrentFOVY.ReadOnly = true;
            this.CurrentFOVY.Size = new System.Drawing.Size(35, 20);
            this.CurrentFOVY.TabIndex = 321;
            this.CurrentFOVY.Text = "5";
            this.CurrentFOVY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // CurrentFOVX
            // 
            this.CurrentFOVX.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentFOVX.Location = new System.Drawing.Point(241, 234);
            this.CurrentFOVX.Margin = new System.Windows.Forms.Padding(1);
            this.CurrentFOVX.Name = "CurrentFOVX";
            this.CurrentFOVX.ReadOnly = true;
            this.CurrentFOVX.Size = new System.Drawing.Size(35, 20);
            this.CurrentFOVX.TabIndex = 320;
            this.CurrentFOVX.Text = "5";
            this.CurrentFOVX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label54.Location = new System.Drawing.Point(280, 259);
            this.label54.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(21, 13);
            this.label54.TabIndex = 325;
            this.label54.Text = "μm";
            // 
            // label90
            // 
            this.label90.AutoSize = true;
            this.label90.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label90.Location = new System.Drawing.Point(280, 239);
            this.label90.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label90.Name = "label90";
            this.label90.Size = new System.Drawing.Size(21, 13);
            this.label90.TabIndex = 324;
            this.label90.Text = "μm";
            // 
            // label92
            // 
            this.label92.AutoSize = true;
            this.label92.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label92.Location = new System.Drawing.Point(225, 258);
            this.label92.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(15, 14);
            this.label92.TabIndex = 323;
            this.label92.Text = "Y";
            // 
            // label93
            // 
            this.label93.AutoSize = true;
            this.label93.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label93.Location = new System.Drawing.Point(225, 237);
            this.label93.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label93.Name = "label93";
            this.label93.Size = new System.Drawing.Size(14, 14);
            this.label93.TabIndex = 322;
            this.label93.Text = "X";
            // 
            // FieldOfViewY
            // 
            this.FieldOfViewY.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FieldOfViewY.Location = new System.Drawing.Point(243, 311);
            this.FieldOfViewY.Margin = new System.Windows.Forms.Padding(1);
            this.FieldOfViewY.Name = "FieldOfViewY";
            this.FieldOfViewY.Size = new System.Drawing.Size(35, 20);
            this.FieldOfViewY.TabIndex = 293;
            this.FieldOfViewY.Text = "5";
            this.FieldOfViewY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.FieldOfViewY.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // FieldOfViewX
            // 
            this.FieldOfViewX.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FieldOfViewX.Location = new System.Drawing.Point(243, 290);
            this.FieldOfViewX.Margin = new System.Windows.Forms.Padding(1);
            this.FieldOfViewX.Name = "FieldOfViewX";
            this.FieldOfViewX.Size = new System.Drawing.Size(35, 20);
            this.FieldOfViewX.TabIndex = 292;
            this.FieldOfViewX.Text = "5";
            this.FieldOfViewX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.FieldOfViewX.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // label68
            // 
            this.label68.AutoSize = true;
            this.label68.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label68.Location = new System.Drawing.Point(282, 315);
            this.label68.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label68.Name = "label68";
            this.label68.Size = new System.Drawing.Size(21, 13);
            this.label68.TabIndex = 300;
            this.label68.Text = "μm";
            // 
            // label65
            // 
            this.label65.AutoSize = true;
            this.label65.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label65.Location = new System.Drawing.Point(282, 295);
            this.label65.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label65.Name = "label65";
            this.label65.Size = new System.Drawing.Size(21, 13);
            this.label65.TabIndex = 299;
            this.label65.Text = "μm";
            // 
            // label63
            // 
            this.label63.AutoSize = true;
            this.label63.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label63.Location = new System.Drawing.Point(227, 314);
            this.label63.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label63.Name = "label63";
            this.label63.Size = new System.Drawing.Size(15, 14);
            this.label63.TabIndex = 297;
            this.label63.Text = "Y";
            // 
            // label64
            // 
            this.label64.AutoSize = true;
            this.label64.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label64.Location = new System.Drawing.Point(227, 293);
            this.label64.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label64.Name = "label64";
            this.label64.Size = new System.Drawing.Size(14, 14);
            this.label64.TabIndex = 296;
            this.label64.Text = "X";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label27.Location = new System.Drawing.Point(215, 277);
            this.label27.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(62, 14);
            this.label27.TabIndex = 319;
            this.label27.Text = "@zoom = 1";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(105, 283);
            this.label12.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(52, 14);
            this.label12.TabIndex = 318;
            this.label12.Text = "Objective";
            // 
            // Objective_Pulldown
            // 
            this.Objective_Pulldown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Objective_Pulldown.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Objective_Pulldown.FormattingEnabled = true;
            this.Objective_Pulldown.Items.AddRange(new object[] {
            "x16",
            "x20",
            "x40",
            "x60"});
            this.Objective_Pulldown.Location = new System.Drawing.Point(113, 299);
            this.Objective_Pulldown.Name = "Objective_Pulldown";
            this.Objective_Pulldown.Size = new System.Drawing.Size(69, 24);
            this.Objective_Pulldown.TabIndex = 317;
            this.Objective_Pulldown.SelectedIndexChanged += new System.EventHandler(this.Objective_Pulldown_SelectedIndexChanged);
            // 
            // NPixels_PulldownY
            // 
            this.NPixels_PulldownY.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.NPixels_PulldownY.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NPixels_PulldownY.FormattingEnabled = true;
            this.NPixels_PulldownY.Items.AddRange(new object[] {
            "2048",
            "1024",
            "512",
            "256",
            "128",
            "64",
            "32",
            "16",
            "8",
            "4",
            "1"});
            this.NPixels_PulldownY.Location = new System.Drawing.Point(225, 133);
            this.NPixels_PulldownY.Name = "NPixels_PulldownY";
            this.NPixels_PulldownY.Size = new System.Drawing.Size(69, 22);
            this.NPixels_PulldownY.TabIndex = 316;
            this.NPixels_PulldownY.SelectedIndexChanged += new System.EventHandler(this.NPixel_PullDown_ValueChaned);
            // 
            // NPixels_PulldownX
            // 
            this.NPixels_PulldownX.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.NPixels_PulldownX.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NPixels_PulldownX.FormattingEnabled = true;
            this.NPixels_PulldownX.Items.AddRange(new object[] {
            "2048",
            "1024",
            "512",
            "256",
            "128",
            "64",
            "32",
            "16",
            "8",
            "4",
            "1"});
            this.NPixels_PulldownX.Location = new System.Drawing.Point(225, 107);
            this.NPixels_PulldownX.Name = "NPixels_PulldownX";
            this.NPixels_PulldownX.Size = new System.Drawing.Size(69, 22);
            this.NPixels_PulldownX.TabIndex = 315;
            this.NPixels_PulldownX.SelectedIndexChanged += new System.EventHandler(this.NPixel_PullDown_ValueChaned);
            // 
            // MaxScanning
            // 
            this.MaxScanning.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaxScanning.Location = new System.Drawing.Point(71, 46);
            this.MaxScanning.Name = "MaxScanning";
            this.MaxScanning.Size = new System.Drawing.Size(59, 23);
            this.MaxScanning.TabIndex = 314;
            this.MaxScanning.Text = "Max";
            this.MaxScanning.UseVisualStyleBackColor = true;
            this.MaxScanning.Click += new System.EventHandler(this.MaxScanning_Click);
            // 
            // ZeroAngle
            // 
            this.ZeroAngle.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZeroAngle.Location = new System.Drawing.Point(266, 178);
            this.ZeroAngle.Name = "ZeroAngle";
            this.ZeroAngle.Size = new System.Drawing.Size(25, 23);
            this.ZeroAngle.TabIndex = 311;
            this.ZeroAngle.Text = "0";
            this.ZeroAngle.UseVisualStyleBackColor = true;
            this.ZeroAngle.Click += new System.EventHandler(this.ZeroAngle_Click);
            // 
            // Rotation
            // 
            this.Rotation.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Rotation.Location = new System.Drawing.Point(223, 180);
            this.Rotation.Margin = new System.Windows.Forms.Padding(1);
            this.Rotation.Name = "Rotation";
            this.Rotation.Size = new System.Drawing.Size(35, 20);
            this.Rotation.TabIndex = 308;
            this.Rotation.Text = "0";
            this.Rotation.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Rotation.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // YOffset
            // 
            this.YOffset.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.YOffset.Location = new System.Drawing.Point(147, 49);
            this.YOffset.Margin = new System.Windows.Forms.Padding(1);
            this.YOffset.Name = "YOffset";
            this.YOffset.Size = new System.Drawing.Size(39, 20);
            this.YOffset.TabIndex = 151;
            this.YOffset.Text = "0.00";
            this.YOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.YOffset.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // XOffset
            // 
            this.XOffset.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XOffset.Location = new System.Drawing.Point(147, 24);
            this.XOffset.Margin = new System.Windows.Forms.Padding(1);
            this.XOffset.Name = "XOffset";
            this.XOffset.Size = new System.Drawing.Size(39, 20);
            this.XOffset.TabIndex = 150;
            this.XOffset.Text = "0";
            this.XOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.XOffset.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // MirrorStep
            // 
            this.MirrorStep.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MirrorStep.Location = new System.Drawing.Point(278, 62);
            this.MirrorStep.Margin = new System.Windows.Forms.Padding(1);
            this.MirrorStep.Name = "MirrorStep";
            this.MirrorStep.Size = new System.Drawing.Size(30, 20);
            this.MirrorStep.TabIndex = 142;
            this.MirrorStep.Text = "0.1";
            this.MirrorStep.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // Zoom
            // 
            this.Zoom.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Zoom.Location = new System.Drawing.Point(2, 42);
            this.Zoom.Margin = new System.Windows.Forms.Padding(1);
            this.Zoom.Name = "Zoom";
            this.Zoom.Size = new System.Drawing.Size(53, 20);
            this.Zoom.TabIndex = 39;
            this.Zoom.Text = "1.0";
            this.Zoom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Zoom.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // label76
            // 
            this.label76.AutoSize = true;
            this.label76.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label76.Location = new System.Drawing.Point(251, 199);
            this.label76.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label76.Name = "label76";
            this.label76.Size = new System.Drawing.Size(45, 13);
            this.label76.TabIndex = 313;
            this.label76.Text = "degrees";
            // 
            // label75
            // 
            this.label75.AutoSize = true;
            this.label75.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label75.Location = new System.Drawing.Point(260, 186);
            this.label75.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label75.Name = "label75";
            this.label75.Size = new System.Drawing.Size(0, 14);
            this.label75.TabIndex = 310;
            // 
            // label71
            // 
            this.label71.AutoSize = true;
            this.label71.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label71.Location = new System.Drawing.Point(204, 164);
            this.label71.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label71.Name = "label71";
            this.label71.Size = new System.Drawing.Size(79, 14);
            this.label71.TabIndex = 309;
            this.label71.Text = "Rotate -90..90\r\n";
            // 
            // label69
            // 
            this.label69.AutoSize = true;
            this.label69.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label69.Location = new System.Drawing.Point(204, 92);
            this.label69.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label69.Name = "label69";
            this.label69.Size = new System.Drawing.Size(46, 14);
            this.label69.TabIndex = 307;
            this.label69.Text = "#Pixels";
            // 
            // label73
            // 
            this.label73.AutoSize = true;
            this.label73.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label73.Location = new System.Drawing.Point(206, 137);
            this.label73.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label73.Name = "label73";
            this.label73.Size = new System.Drawing.Size(15, 14);
            this.label73.TabIndex = 306;
            this.label73.Text = "Y";
            // 
            // label74
            // 
            this.label74.AutoSize = true;
            this.label74.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label74.Location = new System.Drawing.Point(206, 115);
            this.label74.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label74.Name = "label74";
            this.label74.Size = new System.Drawing.Size(14, 14);
            this.label74.TabIndex = 305;
            this.label74.Text = "X";
            // 
            // ZeroMirror
            // 
            this.ZeroMirror.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZeroMirror.Location = new System.Drawing.Point(224, 25);
            this.ZeroMirror.Name = "ZeroMirror";
            this.ZeroMirror.Size = new System.Drawing.Size(25, 23);
            this.ZeroMirror.TabIndex = 301;
            this.ZeroMirror.Text = "0";
            this.ZeroMirror.UseVisualStyleBackColor = true;
            this.ZeroMirror.Click += new System.EventHandler(this.ZeroMirror_Click);
            // 
            // label62
            // 
            this.label62.AutoSize = true;
            this.label62.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label62.Location = new System.Drawing.Point(204, 219);
            this.label62.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(76, 14);
            this.label62.TabIndex = 298;
            this.label62.Text = "Field of view";
            // 
            // SnapShotButton
            // 
            this.SnapShotButton.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SnapShotButton.Location = new System.Drawing.Point(10, 283);
            this.SnapShotButton.Name = "SnapShotButton";
            this.SnapShotButton.Size = new System.Drawing.Size(66, 37);
            this.SnapShotButton.TabIndex = 284;
            this.SnapShotButton.Text = "Capture Snapshot";
            this.SnapShotButton.UseVisualStyleBackColor = true;
            this.SnapShotButton.Click += new System.EventHandler(this.SnapShot_Click);
            // 
            // Zoom10
            // 
            this.Zoom10.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Zoom10.Location = new System.Drawing.Point(31, 20);
            this.Zoom10.Margin = new System.Windows.Forms.Padding(1);
            this.Zoom10.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.Zoom10.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.Zoom10.Name = "Zoom10";
            this.Zoom10.Size = new System.Drawing.Size(31, 20);
            this.Zoom10.TabIndex = 34;
            this.Zoom10.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.Zoom10.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ZoomPanel_Click);
            // 
            // ScanPosition
            // 
            this.ScanPosition.BackColor = System.Drawing.Color.Black;
            this.ScanPosition.Location = new System.Drawing.Point(4, 80);
            this.ScanPosition.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ScanPosition.Name = "ScanPosition";
            this.ScanPosition.Size = new System.Drawing.Size(196, 196);
            this.ScanPosition.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.ScanPosition.TabIndex = 283;
            this.ScanPosition.TabStop = false;
            this.ScanPosition.Paint += new System.Windows.Forms.PaintEventHandler(this.ScanPosition_Paint);
            // 
            // label59
            // 
            this.label59.AutoSize = true;
            this.label59.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label59.Location = new System.Drawing.Point(184, 53);
            this.label59.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(15, 14);
            this.label59.TabIndex = 155;
            this.label59.Text = "V";
            // 
            // label58
            // 
            this.label58.AutoSize = true;
            this.label58.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label58.Location = new System.Drawing.Point(184, 28);
            this.label58.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label58.Name = "label58";
            this.label58.Size = new System.Drawing.Size(15, 14);
            this.label58.TabIndex = 154;
            this.label58.Text = "V";
            // 
            // MirrorYDown
            // 
            this.MirrorYDown.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MirrorYDown.Location = new System.Drawing.Point(224, 47);
            this.MirrorYDown.Name = "MirrorYDown";
            this.MirrorYDown.Size = new System.Drawing.Size(25, 23);
            this.MirrorYDown.TabIndex = 145;
            this.MirrorYDown.Text = "↓";
            this.MirrorYDown.UseVisualStyleBackColor = true;
            this.MirrorYDown.Click += new System.EventHandler(this.MirrorYUp_Click);
            // 
            // label57
            // 
            this.label57.AutoSize = true;
            this.label57.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label57.Location = new System.Drawing.Point(134, 53);
            this.label57.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(15, 14);
            this.label57.TabIndex = 153;
            this.label57.Text = "Y";
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label56.Location = new System.Drawing.Point(134, 29);
            this.label56.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(14, 14);
            this.label56.TabIndex = 152;
            this.label56.Text = "X";
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label55.Location = new System.Drawing.Point(135, 6);
            this.label55.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(52, 14);
            this.label55.TabIndex = 149;
            this.label55.Text = "Position";
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label52.Location = new System.Drawing.Point(250, 64);
            this.label52.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(29, 14);
            this.label52.TabIndex = 148;
            this.label52.Text = "Step";
            // 
            // MirrorXUp
            // 
            this.MirrorXUp.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MirrorXUp.Location = new System.Drawing.Point(248, 25);
            this.MirrorXUp.Name = "MirrorXUp";
            this.MirrorXUp.Size = new System.Drawing.Size(25, 23);
            this.MirrorXUp.TabIndex = 147;
            this.MirrorXUp.Text = "→";
            this.MirrorXUp.UseVisualStyleBackColor = true;
            this.MirrorXUp.Click += new System.EventHandler(this.MirrorYUp_Click);
            // 
            // MirrorXDown
            // 
            this.MirrorXDown.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MirrorXDown.Location = new System.Drawing.Point(200, 25);
            this.MirrorXDown.Name = "MirrorXDown";
            this.MirrorXDown.Size = new System.Drawing.Size(25, 23);
            this.MirrorXDown.TabIndex = 146;
            this.MirrorXDown.Text = "←";
            this.MirrorXDown.UseVisualStyleBackColor = true;
            this.MirrorXDown.Click += new System.EventHandler(this.MirrorYUp_Click);
            // 
            // MirrorYUp
            // 
            this.MirrorYUp.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MirrorYUp.Location = new System.Drawing.Point(224, 3);
            this.MirrorYUp.Name = "MirrorYUp";
            this.MirrorYUp.Size = new System.Drawing.Size(25, 23);
            this.MirrorYUp.TabIndex = 144;
            this.MirrorYUp.Text = "↑";
            this.MirrorYUp.UseVisualStyleBackColor = true;
            this.MirrorYUp.Click += new System.EventHandler(this.MirrorYUp_Click);
            // 
            // Zoom100
            // 
            this.Zoom100.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Zoom100.Location = new System.Drawing.Point(2, 20);
            this.Zoom100.Margin = new System.Windows.Forms.Padding(1);
            this.Zoom100.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.Zoom100.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.Zoom100.Name = "Zoom100";
            this.Zoom100.Size = new System.Drawing.Size(31, 20);
            this.Zoom100.TabIndex = 38;
            this.Zoom100.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.Zoom100.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ZoomPanel_Click);
            // 
            // st_zoom
            // 
            this.st_zoom.AutoSize = true;
            this.st_zoom.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_zoom.Location = new System.Drawing.Point(4, 6);
            this.st_zoom.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_zoom.Name = "st_zoom";
            this.st_zoom.Size = new System.Drawing.Size(39, 14);
            this.st_zoom.TabIndex = 37;
            this.st_zoom.Text = "Zoom";
            // 
            // ZoomP1
            // 
            this.ZoomP1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZoomP1.Location = new System.Drawing.Point(97, 20);
            this.ZoomP1.Margin = new System.Windows.Forms.Padding(1);
            this.ZoomP1.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.ZoomP1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.ZoomP1.Name = "ZoomP1";
            this.ZoomP1.Size = new System.Drawing.Size(31, 20);
            this.ZoomP1.TabIndex = 35;
            this.ZoomP1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ZoomP1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ZoomPanel_Click);
            // 
            // Zoom1
            // 
            this.Zoom1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Zoom1.Location = new System.Drawing.Point(61, 20);
            this.Zoom1.Margin = new System.Windows.Forms.Padding(1);
            this.Zoom1.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.Zoom1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.Zoom1.Name = "Zoom1";
            this.Zoom1.Size = new System.Drawing.Size(31, 20);
            this.Zoom1.TabIndex = 33;
            this.Zoom1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.Zoom1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.Zoom1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ZoomPanel_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(89, 25);
            this.label6.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(10, 14);
            this.label6.TabIndex = 36;
            this.label6.Text = ".";
            // 
            // FLIMSetting_tab
            // 
            this.FLIMSetting_tab.Controls.Add(this.tb_ScanParameters);
            this.FLIMSetting_tab.Controls.Add(this.tbScanParam);
            this.FLIMSetting_tab.Controls.Add(this.tb_Pparameters);
            this.FLIMSetting_tab.Controls.Add(this.ChannelSettingTab);
            this.FLIMSetting_tab.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FLIMSetting_tab.Location = new System.Drawing.Point(316, 30);
            this.FLIMSetting_tab.Name = "FLIMSetting_tab";
            this.FLIMSetting_tab.SelectedIndex = 0;
            this.FLIMSetting_tab.Size = new System.Drawing.Size(322, 364);
            this.FLIMSetting_tab.TabIndex = 281;
            // 
            // ChannelSettingTab
            // 
            this.ChannelSettingTab.BackColor = System.Drawing.SystemColors.Control;
            this.ChannelSettingTab.Controls.Add(this.KeepPagesInMemoryCheck);
            this.ChannelSettingTab.Controls.Add(this.AveFrameSeparately);
            this.ChannelSettingTab.Controls.Add(this.SaveInSeparatedFileCheck);
            this.ChannelSettingTab.Controls.Add(this.groupBox4);
            this.ChannelSettingTab.Controls.Add(this.groupBox2);
            this.ChannelSettingTab.Location = new System.Drawing.Point(4, 23);
            this.ChannelSettingTab.Name = "ChannelSettingTab";
            this.ChannelSettingTab.Padding = new System.Windows.Forms.Padding(3);
            this.ChannelSettingTab.Size = new System.Drawing.Size(314, 337);
            this.ChannelSettingTab.TabIndex = 3;
            this.ChannelSettingTab.Text = "Channels";
            // 
            // KeepPagesInMemoryCheck
            // 
            this.KeepPagesInMemoryCheck.AutoSize = true;
            this.KeepPagesInMemoryCheck.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeepPagesInMemoryCheck.Location = new System.Drawing.Point(21, 181);
            this.KeepPagesInMemoryCheck.Name = "KeepPagesInMemoryCheck";
            this.KeepPagesInMemoryCheck.Size = new System.Drawing.Size(140, 18);
            this.KeepPagesInMemoryCheck.TabIndex = 349;
            this.KeepPagesInMemoryCheck.Text = "Keep images in memory";
            this.KeepPagesInMemoryCheck.UseVisualStyleBackColor = true;
            this.KeepPagesInMemoryCheck.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // AveFrameSeparately
            // 
            this.AveFrameSeparately.AutoSize = true;
            this.AveFrameSeparately.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AveFrameSeparately.Location = new System.Drawing.Point(21, 156);
            this.AveFrameSeparately.Name = "AveFrameSeparately";
            this.AveFrameSeparately.Size = new System.Drawing.Size(155, 18);
            this.AveFrameSeparately.TabIndex = 348;
            this.AveFrameSeparately.Text = " Average frame separately";
            this.AveFrameSeparately.UseVisualStyleBackColor = true;
            this.AveFrameSeparately.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // SaveInSeparatedFileCheck
            // 
            this.SaveInSeparatedFileCheck.AutoSize = true;
            this.SaveInSeparatedFileCheck.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SaveInSeparatedFileCheck.Location = new System.Drawing.Point(21, 132);
            this.SaveInSeparatedFileCheck.Name = "SaveInSeparatedFileCheck";
            this.SaveInSeparatedFileCheck.Size = new System.Drawing.Size(129, 18);
            this.SaveInSeparatedFileCheck.TabIndex = 347;
            this.SaveInSeparatedFileCheck.Text = "Save in different files";
            this.SaveInSeparatedFileCheck.UseVisualStyleBackColor = true;
            this.SaveInSeparatedFileCheck.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.Intensity_Radio2);
            this.groupBox4.Controls.Add(this.Acquisition2);
            this.groupBox4.Controls.Add(this.FLIM_Radio2);
            this.groupBox4.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(165, 17);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(120, 100);
            this.groupBox4.TabIndex = 347;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Ch2";
            // 
            // Intensity_Radio2
            // 
            this.Intensity_Radio2.AutoSize = true;
            this.Intensity_Radio2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Intensity_Radio2.Location = new System.Drawing.Point(29, 60);
            this.Intensity_Radio2.Name = "Intensity_Radio2";
            this.Intensity_Radio2.Size = new System.Drawing.Size(65, 18);
            this.Intensity_Radio2.TabIndex = 348;
            this.Intensity_Radio2.TabStop = true;
            this.Intensity_Radio2.Text = "Intensity";
            this.Intensity_Radio2.UseVisualStyleBackColor = true;
            this.Intensity_Radio2.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // Acquisition2
            // 
            this.Acquisition2.AutoSize = true;
            this.Acquisition2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Acquisition2.Location = new System.Drawing.Point(13, 21);
            this.Acquisition2.Name = "Acquisition2";
            this.Acquisition2.Size = new System.Drawing.Size(79, 18);
            this.Acquisition2.TabIndex = 344;
            this.Acquisition2.Text = "Acquisition";
            this.Acquisition2.UseVisualStyleBackColor = true;
            this.Acquisition2.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // FLIM_Radio2
            // 
            this.FLIM_Radio2.AutoSize = true;
            this.FLIM_Radio2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FLIM_Radio2.Location = new System.Drawing.Point(29, 42);
            this.FLIM_Radio2.Name = "FLIM_Radio2";
            this.FLIM_Radio2.Size = new System.Drawing.Size(47, 18);
            this.FLIM_Radio2.TabIndex = 347;
            this.FLIM_Radio2.TabStop = true;
            this.FLIM_Radio2.Text = "FLIM";
            this.FLIM_Radio2.UseVisualStyleBackColor = true;
            this.FLIM_Radio2.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.Intensity_Radio1);
            this.groupBox2.Controls.Add(this.FLIM_Radio1);
            this.groupBox2.Controls.Add(this.Acquisition1);
            this.groupBox2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(21, 17);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(120, 100);
            this.groupBox2.TabIndex = 346;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Ch1";
            // 
            // Intensity_Radio1
            // 
            this.Intensity_Radio1.AutoSize = true;
            this.Intensity_Radio1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Intensity_Radio1.Location = new System.Drawing.Point(25, 60);
            this.Intensity_Radio1.Name = "Intensity_Radio1";
            this.Intensity_Radio1.Size = new System.Drawing.Size(65, 18);
            this.Intensity_Radio1.TabIndex = 346;
            this.Intensity_Radio1.TabStop = true;
            this.Intensity_Radio1.Text = "Intensity";
            this.Intensity_Radio1.UseVisualStyleBackColor = true;
            this.Intensity_Radio1.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // FLIM_Radio1
            // 
            this.FLIM_Radio1.AutoSize = true;
            this.FLIM_Radio1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FLIM_Radio1.Location = new System.Drawing.Point(25, 42);
            this.FLIM_Radio1.Name = "FLIM_Radio1";
            this.FLIM_Radio1.Size = new System.Drawing.Size(47, 18);
            this.FLIM_Radio1.TabIndex = 345;
            this.FLIM_Radio1.TabStop = true;
            this.FLIM_Radio1.Text = "FLIM";
            this.FLIM_Radio1.UseVisualStyleBackColor = true;
            this.FLIM_Radio1.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // Acquisition1
            // 
            this.Acquisition1.AutoSize = true;
            this.Acquisition1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Acquisition1.Location = new System.Drawing.Point(13, 21);
            this.Acquisition1.Name = "Acquisition1";
            this.Acquisition1.Size = new System.Drawing.Size(79, 18);
            this.Acquisition1.TabIndex = 344;
            this.Acquisition1.Text = "Acquisition";
            this.Acquisition1.UseVisualStyleBackColor = true;
            this.Acquisition1.Click += new System.EventHandler(this.Generic_ValueChanged);
            // 
            // StatusText
            // 
            this.StatusText.AutoSize = true;
            this.StatusText.Location = new System.Drawing.Point(11, 729);
            this.StatusText.Name = "StatusText";
            this.StatusText.Size = new System.Drawing.Size(46, 13);
            this.StatusText.TabIndex = 334;
            this.StatusText.Text = "Status...";
            // 
            // MotorStatus
            // 
            this.MotorStatus.AutoSize = true;
            this.MotorStatus.Location = new System.Drawing.Point(11, 744);
            this.MotorStatus.Name = "MotorStatus";
            this.MotorStatus.Size = new System.Drawing.Size(46, 13);
            this.MotorStatus.TabIndex = 335;
            this.MotorStatus.Text = "Status...";
            // 
            // PiezoZ
            // 
            this.PiezoZ.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PiezoZ.Location = new System.Drawing.Point(15, 104);
            this.PiezoZ.Margin = new System.Windows.Forms.Padding(1);
            this.PiezoZ.Name = "PiezoZ";
            this.PiezoZ.Size = new System.Drawing.Size(50, 20);
            this.PiezoZ.TabIndex = 286;
            this.PiezoZ.Text = "0";
            this.PiezoZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // PiezoZUnitLabel
            // 
            this.PiezoZUnitLabel.AutoSize = true;
            this.PiezoZUnitLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PiezoZUnitLabel.Location = new System.Drawing.Point(65, 108);
            this.PiezoZUnitLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.PiezoZUnitLabel.Name = "PiezoZUnitLabel";
            this.PiezoZUnitLabel.Size = new System.Drawing.Size(58, 14);
            this.PiezoZUnitLabel.TabIndex = 287;
            this.PiezoZUnitLabel.Text = "μm (Piezo)";
            // 
            // PiezoZLabel
            // 
            this.PiezoZLabel.AutoSize = true;
            this.PiezoZLabel.BackColor = System.Drawing.Color.Transparent;
            this.PiezoZLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PiezoZLabel.Location = new System.Drawing.Point(1, 107);
            this.PiezoZLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.PiezoZLabel.Name = "PiezoZLabel";
            this.PiezoZLabel.Size = new System.Drawing.Size(14, 14);
            this.PiezoZLabel.TabIndex = 288;
            this.PiezoZLabel.Text = "Z";
            // 
            // CenterPiezoButton
            // 
            this.CenterPiezoButton.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CenterPiezoButton.Location = new System.Drawing.Point(15, 133);
            this.CenterPiezoButton.Margin = new System.Windows.Forms.Padding(1);
            this.CenterPiezoButton.Name = "CenterPiezoButton";
            this.CenterPiezoButton.Size = new System.Drawing.Size(51, 40);
            this.CenterPiezoButton.TabIndex = 289;
            this.CenterPiezoButton.Text = "Center piezo";
            this.CenterPiezoButton.UseVisualStyleBackColor = true;
            this.CenterPiezoButton.Click += new System.EventHandler(this.CenterPiezoButton_Click);
            // 
            // FLIMageMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(638, 763);
            this.Controls.Add(this.MotorStatus);
            this.Controls.Add(this.StatusText);
            this.Controls.Add(this.ExtTriggerCB);
            this.Controls.Add(this.ImageIteration);
            this.Controls.Add(this.expectedRate);
            this.Controls.Add(this.acquisitionPanel);
            this.Controls.Add(this.label67);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.st_display);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.stagePanel);
            this.Controls.Add(this.Sync_rate2);
            this.Controls.Add(this.Panel_Files);
            this.Controls.Add(this.PowerBoxImageParameters);
            this.Controls.Add(this.LaserPanel);
            this.Controls.Add(this.Ch_rate2);
            this.Controls.Add(this.FLIMSetting_tab);
            this.Controls.Add(this.Ch_rate1);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.Sync_rate);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(5, 5);
            this.Margin = new System.Windows.Forms.Padding(1);
            this.Name = "FLIMageMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FLIMage!";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FLIMControls_FormClosing);
            this.Load += new System.EventHandler(this.FLIMageMain_Load);
            this.Shown += new System.EventHandler(this.FLIMcontrols_Shown);
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PowerSlider2)).EndInit();
            this.PowerBoxImageParameters.ResumeLayout(false);
            this.PowerBoxImageParameters.PerformLayout();
            this.XY_panel.ResumeLayout(false);
            this.XY_panel.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.stagePanel.ResumeLayout(false);
            this.stagePanel.PerformLayout();
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.LaserPanel.ResumeLayout(false);
            this.LaserPanel.PerformLayout();
            this.laserPowerPanel.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PowerSlider1)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PowerSlider3)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PowerSlider4)).EndInit();
            this.acquisitionPanel.ResumeLayout(false);
            this.acquisitionPanel.PerformLayout();
            this.Panel_Files.ResumeLayout(false);
            this.Panel_Files.PerformLayout();
            this.ImageIteration.ResumeLayout(false);
            this.ImageIteration.PerformLayout();
            this.tb_Pparameters.ResumeLayout(false);
            this.tb_Pparameters.PerformLayout();
            this.sync2Group.ResumeLayout(false);
            this.sync2Group.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.tbScanParam.ResumeLayout(false);
            this.tbScanParam.PerformLayout();
            this.tb_ScanParameters.ResumeLayout(false);
            this.tb_ScanParameters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Zoom10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScanPosition)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Zoom100)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ZoomP1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Zoom1)).EndInit();
            this.FLIMSetting_tab.ResumeLayout(false);
            this.ChannelSettingTab.ResumeLayout(false);
            this.ChannelSettingTab.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.TextBox CurrentSlice;
        public System.Windows.Forms.TextBox NSlices;
        public System.Windows.Forms.TextBox NImages;
        public System.Windows.Forms.TextBox CurrentImage;
        public System.Windows.Forms.Label st_slices;
        public System.Windows.Forms.Label st_NAcq;
        public System.Windows.Forms.Label st_NDone;
        public System.Windows.Forms.TextBox ImageInterval;
        public System.Windows.Forms.Label st_interval;
        public System.Windows.Forms.TextBox SliceInterval;
        public System.Windows.Forms.TextBox FrameInterval;
        public System.Windows.Forms.Label ETime;
        public System.Windows.Forms.CheckBox AveFrame_Check;
        public System.Windows.Forms.TextBox NumAve;
        public System.Windows.Forms.TextBox CurrentFrame;
        public System.Windows.Forms.GroupBox PowerBoxImageParameters;
        public System.Windows.Forms.GroupBox XY_panel;
        public System.Windows.Forms.Label label43;
        public System.Windows.Forms.Button XUp;
        public System.Windows.Forms.Button XDown;
        public System.Windows.Forms.Button YDown;
        public System.Windows.Forms.Button YUp;
        public System.Windows.Forms.Label label44;
        public System.Windows.Forms.TextBox XYMotorStep;
        public System.Windows.Forms.Label label45;
        public System.Windows.Forms.Label label46;
        public System.Windows.Forms.Label label47;
        public System.Windows.Forms.Label label48;
        public System.Windows.Forms.Label label49;
        public System.Windows.Forms.Label label50;
        public System.Windows.Forms.Label label51;
        public System.Windows.Forms.Button Zero_all;
        public System.Windows.Forms.TextBox ZRead;
        public System.Windows.Forms.TextBox YRead;
        public System.Windows.Forms.Button Set_bottom;
        public System.Windows.Forms.TextBox XRead;
        public System.Windows.Forms.Button Set_Top;
        public System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.TextBox ZMotorStep;
        public System.Windows.Forms.Button Zdown;
        public System.Windows.Forms.Label label53;
        public System.Windows.Forms.Button Zup;
        public System.Windows.Forms.Button zero_Z;
        public System.Windows.Forms.GroupBox stagePanel;
        public System.Windows.Forms.Button FocusButton;
        public System.Windows.Forms.Button GrabButton;
        public System.Windows.Forms.Label label18;
        public System.Windows.Forms.GroupBox groupBox3;
        public System.Windows.Forms.TextBox NSlices2;
        public System.Windows.Forms.Label label42;
        public System.Windows.Forms.TextBox ZEnd;
        public System.Windows.Forms.TextBox SliceStep;
        public System.Windows.Forms.TextBox ZStart;
        public System.Windows.Forms.Label st_step;
        public System.Windows.Forms.Label st_um;
        public System.Windows.Forms.Label label60;
        public System.Windows.Forms.Button Vel_Down;
        public System.Windows.Forms.Button Vel_Up;
        public System.Windows.Forms.Label st_display;
        public System.Windows.Forms.MenuStrip menuStrip1;
        public System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem setPathToolStripMenuItem;
        public System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        public System.Windows.Forms.ToolStripMenuItem loadSttingToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem saveSettingToolStripMenuItem;
        public System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        public System.Windows.Forms.ToolStripMenuItem powerToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem calibrateToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem showCalibrationCurveToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem plotToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem plotScanToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem plotPockelsToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem windowsToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem imageToolStripMenuItem;
        public System.Windows.Forms.Label Ch_rate2;
        public System.Windows.Forms.Label Ch_rate1;
        public System.Windows.Forms.Label Sync_rate;
        public System.Windows.Forms.Label label13;
        public System.Windows.Forms.Label label14;
        public System.Windows.Forms.Label label15;
        public System.Windows.Forms.GroupBox LaserPanel;
        public System.Windows.Forms.Button Calibrate1;
        public System.Windows.Forms.TabControl laserPowerPanel;
        public System.Windows.Forms.TabPage tabPage1;
        public System.Windows.Forms.CheckBox ImageLaser1;
        public System.Windows.Forms.Label st_Power;
        public System.Windows.Forms.TrackBar PowerSlider1;
        public System.Windows.Forms.TextBox Power1;
        public System.Windows.Forms.CheckBox ImageLaser2;
        public System.Windows.Forms.Label label19;
        public System.Windows.Forms.TrackBar PowerSlider2;
        public System.Windows.Forms.TextBox Power2;
        public System.Windows.Forms.TabPage tabPage3;
        public System.Windows.Forms.CheckBox ImageLaser3;
        public System.Windows.Forms.Label label20;
        public System.Windows.Forms.TrackBar PowerSlider3;
        public System.Windows.Forms.TextBox Power3;
        public System.Windows.Forms.TabPage tabPage4;
        public System.Windows.Forms.CheckBox ImageLaser4;
        public System.Windows.Forms.Label label17;
        public System.Windows.Forms.TrackBar PowerSlider4;
        public System.Windows.Forms.TextBox Power4;
        public System.Windows.Forms.CheckBox AveSlices_check;
        public System.Windows.Forms.Label Measured_slice_interval;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label Motor_Status;
        public System.Windows.Forms.CheckBox Uncage_while_image_check;
        public System.Windows.Forms.ToolStripMenuItem plotScanGrabToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem plotPockelsGrabToolStripMenuItem;
        public System.Windows.Forms.CheckBox UncageLaser1;
        public System.Windows.Forms.CheckBox UncageLaser2;
        public System.Windows.Forms.CheckBox UncageLaser3;
        public System.Windows.Forms.CheckBox UncageLaser4;
        public System.Windows.Forms.GroupBox groupBox11;
        public System.Windows.Forms.Label label77;
        public System.Windows.Forms.Label label72;
        public System.Windows.Forms.Button GoEnd;
        public System.Windows.Forms.Button GoStart;
        public System.Windows.Forms.GroupBox acquisitionPanel;
        public System.Windows.Forms.Label Sync_rate2;
        public System.Windows.Forms.Label label79;
        public System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem fLIMageToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem showInputoutputCurveToolStripMenuItem;
        public System.Windows.Forms.Label st_BaseName;
        public System.Windows.Forms.Label st_PathName;
        public System.Windows.Forms.Label st_fileN;
        public System.Windows.Forms.TextBox BaseName;
        public System.Windows.Forms.TextBox FileN;
        public System.Windows.Forms.TextBox DirectoryName;
        public System.Windows.Forms.Button FindPath;
        public System.Windows.Forms.Label label70;
        public System.Windows.Forms.TextBox FileName;
        public System.Windows.Forms.GroupBox Panel_Files;
        public System.Windows.Forms.Label ETime2;
        public System.Windows.Forms.Button needCalib1;
        public System.Windows.Forms.Button needCalib2;
        public System.Windows.Forms.Button needCalib3;
        public System.Windows.Forms.Button needCalib4;
        public System.Windows.Forms.TextBox N_AveSlices;
        public System.Windows.Forms.CheckBox analyzeEach;
        public System.Windows.Forms.Label Misc_about_Slice;
        public System.Windows.Forms.RadioButton ZStack_radio;
        public System.Windows.Forms.RadioButton Timelapse_radio;
        public System.Windows.Forms.TextBox FullFileName;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Label label7;
        public System.Windows.Forms.Label label9;
        public System.Windows.Forms.GroupBox ImageIteration;
        public System.Windows.Forms.Label nAverageFrame;
        public System.Windows.Forms.Label label66;
        public System.Windows.Forms.Label needCalibLabel;
        public System.Windows.Forms.Label powerRead1;
        public System.Windows.Forms.Label powerRead2;
        public System.Windows.Forms.Label powerRead3;
        public System.Windows.Forms.Label powerRead4;
        public System.Windows.Forms.Button zeroVoltage;
        public System.Windows.Forms.Button laserWarningButton;
        public System.Windows.Forms.Label label67;
        public System.Windows.Forms.Label expectedRate;
        public System.Windows.Forms.Label label78;
        public System.Windows.Forms.Label SavedFileN;
        public System.Windows.Forms.Label label16;
        public System.Windows.Forms.ToolStripMenuItem settingToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem nIDAQConfigToolStripMenuItem;
        public System.Windows.Forms.Label label88;
        public System.Windows.Forms.Label AOCounter;
        public System.Windows.Forms.Label label91;
        public System.Windows.Forms.CheckBox ExtTriggerCB;
        public System.Windows.Forms.TextBox N_AveragedFrames1;
        public System.Windows.Forms.ToolStripMenuItem ToolsToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem uncagingControlToolStripMenuItem1;
        public System.Windows.Forms.TextBox N_AveragedSlices;
        public System.Windows.Forms.TextBox NFrames;
        public System.Windows.Forms.Label aveSlice_Interval;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.Label aveFrame_Interval;
        public System.Windows.Forms.Label label5;
        public System.Windows.Forms.ToolStripMenuItem dIOPanelToolStripMenuItem;
        public System.Windows.Forms.Button LoopButton;
        public System.Windows.Forms.ToolStripMenuItem quickSettingToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem saveSetting1ToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem saveSetting2ToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem saveSetting3ToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem loadSetting1ToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem loadSetting2ToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem loadSetting3ToolStripMenuItem;
        public System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        public System.Windows.Forms.Button ResetMotor;
        public System.Windows.Forms.CheckBox ContRead;
        public System.Windows.Forms.TextBox Velocity;
        public System.Windows.Forms.CheckBox Relative;
        public System.Windows.Forms.ToolStripMenuItem loadScanParametersToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem shadingCorretionToolStripMenuItem;
        public System.Windows.Forms.Button MotorReadButton;
        public System.Windows.Forms.ToolStripMenuItem driftCorrectionToolStripMenuItem;
        public System.Windows.Forms.TabPage tb_Pparameters;
        public System.Windows.Forms.Label st_mode;
        public System.Windows.Forms.ComboBox PQMode_Pulldown;
        public System.Windows.Forms.ComboBox Binning_setting;
        public System.Windows.Forms.Label st_nTimeP;
        public System.Windows.Forms.ComboBox Resolution_Pulldown;
        public System.Windows.Forms.GroupBox sync2Group;
        public System.Windows.Forms.Label label80;
        public System.Windows.Forms.Label label81;
        public System.Windows.Forms.Label label82;
        public System.Windows.Forms.Label label83;
        public System.Windows.Forms.TextBox sync_offset2;
        public System.Windows.Forms.Label label84;
        public System.Windows.Forms.TextBox sync_threshold2;
        public System.Windows.Forms.Label label85;
        public System.Windows.Forms.TextBox sync_zc_level2;
        public System.Windows.Forms.Label st_tp;
        public System.Windows.Forms.TextBox NTimePoints;
        public System.Windows.Forms.GroupBox groupBox7;
        public System.Windows.Forms.Label label26;
        public System.Windows.Forms.Label label24;
        public System.Windows.Forms.Label label25;
        public System.Windows.Forms.Label label23;
        public System.Windows.Forms.TextBox sync_offset;
        public System.Windows.Forms.Label label22;
        public System.Windows.Forms.TextBox sync_threshold;
        public System.Windows.Forms.Label label21;
        public System.Windows.Forms.TextBox sync_zc_level;
        public System.Windows.Forms.GroupBox groupBox6;
        public System.Windows.Forms.Label label30;
        public System.Windows.Forms.Label label31;
        public System.Windows.Forms.Label label32;
        public System.Windows.Forms.Label label33;
        public System.Windows.Forms.TextBox ch_offset1;
        public System.Windows.Forms.Label label34;
        public System.Windows.Forms.TextBox ch_threshold1;
        public System.Windows.Forms.Label label35;
        public System.Windows.Forms.Label label29;
        public System.Windows.Forms.TextBox ch_zc_level1;
        public System.Windows.Forms.TextBox resolution;
        public System.Windows.Forms.Label label28;
        public System.Windows.Forms.GroupBox groupBox5;
        public System.Windows.Forms.TextBox resolution2;
        public System.Windows.Forms.Label label87;
        public System.Windows.Forms.Label label86;
        public System.Windows.Forms.Label label36;
        public System.Windows.Forms.Label label37;
        public System.Windows.Forms.Label label38;
        public System.Windows.Forms.Label label39;
        public System.Windows.Forms.TextBox ch_offset2;
        public System.Windows.Forms.Label label40;
        public System.Windows.Forms.TextBox ch_threshold2;
        public System.Windows.Forms.Label label41;
        public System.Windows.Forms.TextBox ch_zc_level2;
        public System.Windows.Forms.Label st_binning;
        public System.Windows.Forms.TabPage tbScanParam;
        public System.Windows.Forms.Label MeasuredLineCorrection;
        public System.Windows.Forms.Label label95;
        public System.Windows.Forms.TextBox LineTimeCorrection;
        public System.Windows.Forms.TextBox ScanDelay;
        public System.Windows.Forms.TextBox ScanFraction;
        public System.Windows.Forms.TextBox FillFraction;
        public System.Windows.Forms.TextBox MaxRangeY;
        public System.Windows.Forms.TextBox MaxRangeX;
        public System.Windows.Forms.TextBox pixelTime;
        public System.Windows.Forms.TextBox MsPerLine;
        public System.Windows.Forms.TextBox pixelsPerLine;
        public System.Windows.Forms.TextBox linesPerFrame;
        public System.Windows.Forms.Label label94;
        public System.Windows.Forms.CheckBox BiDirecCB;
        public System.Windows.Forms.Label label89;
        public System.Windows.Forms.Button pb32;
        public System.Windows.Forms.Button pb1024;
        public System.Windows.Forms.Button pb512;
        public System.Windows.Forms.Button pb64;
        public System.Windows.Forms.Button pb256;
        public System.Windows.Forms.Button pb128;
        public System.Windows.Forms.Label label61;
        public System.Windows.Forms.Label label10;
        public System.Windows.Forms.Label label11;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label st_us;
        public System.Windows.Forms.Label st_LineTime;
        public System.Windows.Forms.Label st_PixelsPerLine;
        public System.Windows.Forms.Label st_LinesPerFrame;
        public System.Windows.Forms.Label st_ms;
        public System.Windows.Forms.Label label8;
        public System.Windows.Forms.Label st_ScanFraction;
        public System.Windows.Forms.Label st_ScanDelay;
        public System.Windows.Forms.Label st_FillFraction;
        public System.Windows.Forms.CheckBox AdvancedCheck;
        public System.Windows.Forms.TabPage tb_ScanParameters;
        public System.Windows.Forms.Button MaxScanning;
        public System.Windows.Forms.Button ZeroAngle;
        public System.Windows.Forms.TextBox Rotation;
        public System.Windows.Forms.TextBox FieldOfViewY;
        public System.Windows.Forms.TextBox FieldOfViewX;
        public System.Windows.Forms.TextBox YOffset;
        public System.Windows.Forms.TextBox XOffset;
        public System.Windows.Forms.TextBox MirrorStep;
        public System.Windows.Forms.TextBox Zoom;
        public System.Windows.Forms.Label label76;
        public System.Windows.Forms.Label label75;
        public System.Windows.Forms.Label label71;
        public System.Windows.Forms.Label label69;
        public System.Windows.Forms.Label label73;
        public System.Windows.Forms.Label label74;
        public System.Windows.Forms.Button ZeroMirror;
        public System.Windows.Forms.Label label68;
        public System.Windows.Forms.Label label65;
        public System.Windows.Forms.Label label62;
        public System.Windows.Forms.Label label63;
        public System.Windows.Forms.Label label64;
        public System.Windows.Forms.Button SnapShotButton;
        public System.Windows.Forms.NumericUpDown Zoom10;
        public System.Windows.Forms.PictureBox ScanPosition;
        public System.Windows.Forms.Label label59;
        public System.Windows.Forms.Label label58;
        public System.Windows.Forms.Button MirrorYDown;
        public System.Windows.Forms.Label label57;
        public System.Windows.Forms.Label label56;
        public System.Windows.Forms.Label label55;
        public System.Windows.Forms.Label label52;
        public System.Windows.Forms.Button MirrorXUp;
        public System.Windows.Forms.Button MirrorXDown;
        public System.Windows.Forms.Button MirrorYUp;
        public System.Windows.Forms.NumericUpDown Zoom100;
        public System.Windows.Forms.Label st_zoom;
        public System.Windows.Forms.NumericUpDown ZoomP1;
        public System.Windows.Forms.NumericUpDown Zoom1;
        public System.Windows.Forms.Label label6;
        public System.Windows.Forms.TabControl FLIMSetting_tab;
        public System.Windows.Forms.ToolStripMenuItem fastZControlToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem imageSeqControlToolStripMenuItem;
        public System.Windows.Forms.Label StatusText;
        public System.Windows.Forms.Label MotorStatus;
        public System.Windows.Forms.TextBox ZCenter;
        public System.Windows.Forms.Button GoCenter;
        public System.Windows.Forms.Label CenterLabel;
        public System.Windows.Forms.Button Set_Center;
        public System.Windows.Forms.ToolStripMenuItem realtimePlotToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem remoteControlToolStripMenuItem1;
        public System.Windows.Forms.ToolStripMenuItem stageControlToolStripMenuItem;
        public System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        public System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        public System.Windows.Forms.Label NAveSliceLabel;
        public System.Windows.Forms.Label SliceIntervalLabel;
        public System.Windows.Forms.Label label101;
        public System.Windows.Forms.Label label102;
        public System.Windows.Forms.CheckBox AveFrame2_Check;
        public System.Windows.Forms.Label TotalNFramesLabel1;
        public System.Windows.Forms.TextBox N_AveragedFrames2;
        public System.Windows.Forms.Label NAveragedSliceLabel;
        public System.Windows.Forms.Label TotalNFrames2Label;
        public System.Windows.Forms.CheckBox Acquisition1;
        public System.Windows.Forms.TabPage ChannelSettingTab;
        public System.Windows.Forms.GroupBox groupBox4;
        public System.Windows.Forms.RadioButton Intensity_Radio2;
        public System.Windows.Forms.CheckBox Acquisition2;
        public System.Windows.Forms.RadioButton FLIM_Radio2;
        public System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.RadioButton Intensity_Radio1;
        public System.Windows.Forms.RadioButton FLIM_Radio1;
        public System.Windows.Forms.CheckBox SaveInSeparatedFileCheck;
        public System.Windows.Forms.CheckBox AveFrameSeparately;
        public System.Windows.Forms.CheckBox KeepPagesInMemoryCheck;
        public System.Windows.Forms.ComboBox NPixels_PulldownX;
        public System.Windows.Forms.ComboBox NPixels_PulldownY;
        public System.Windows.Forms.CheckBox FlipX_CB;
        public System.Windows.Forms.CheckBox FlipY_CB;
        public System.Windows.Forms.CheckBox SwitchXY_CB;
        public System.Windows.Forms.CheckBox SineWaveScanning_CB;
        public System.Windows.Forms.ComboBox Objective_Pulldown;
        public System.Windows.Forms.Label label12;
        public System.Windows.Forms.Label label27;
        public System.Windows.Forms.TextBox CurrentFOVY;
        public System.Windows.Forms.TextBox CurrentFOVX;
        public System.Windows.Forms.Label label54;
        public System.Windows.Forms.Label label90;
        public System.Windows.Forms.Label label92;
        public System.Windows.Forms.Label label93;
        public System.Windows.Forms.ToolStripMenuItem resetWindowPositionsToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem pMTControlToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem electrophysiologyToolStripMenuItem;
        public System.Windows.Forms.TextBox StartPointBox;
        public System.Windows.Forms.Label label96;
        public System.Windows.Forms.Label label97;
        public System.Windows.Forms.TextBox FreqDivBox;
        private System.Windows.Forms.RadioButton BackToStartRadio;
        private System.Windows.Forms.RadioButton BackToCenterRadio;
        private System.Windows.Forms.RadioButton StayMotorRadio;
        public System.Windows.Forms.CheckBox DO_whileImaging_check;
        private System.Windows.Forms.ToolStripMenuItem digitalOutputControlToolStripMenuItem;
        public System.Windows.Forms.Label PiezoZLabel;
        public System.Windows.Forms.Label PiezoZUnitLabel;
        public System.Windows.Forms.TextBox PiezoZ;
        public System.Windows.Forms.Button CenterPiezoButton;
    }
}

