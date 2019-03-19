namespace FLIMimage
{
    partial class Image_Display
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Image_Display));
            this.logScale = new System.Windows.Forms.CheckBox();
            this.ShowFLIM = new System.Windows.Forms.CheckBox();
            this.Channel1 = new System.Windows.Forms.RadioButton();
            this.Channel2 = new System.Windows.Forms.RadioButton();
            this.imgOffset1 = new System.Windows.Forms.Label();
            this.Apply_Offset = new System.Windows.Forms.Button();
            this.fix_all = new System.Windows.Forms.CheckBox();
            this.label75 = new System.Windows.Forms.Label();
            this.xi_square = new System.Windows.Forms.Label();
            this.label73 = new System.Windows.Forms.Label();
            this.label74 = new System.Windows.Forms.Label();
            this.fit_end = new System.Windows.Forms.TextBox();
            this.label67 = new System.Windows.Forms.Label();
            this.t0_Img = new System.Windows.Forms.TextBox();
            this.fit_start = new System.Windows.Forms.TextBox();
            this.cb_T0Fix = new System.Windows.Forms.CheckBox();
            this.cb_tauGFix = new System.Windows.Forms.CheckBox();
            this.cb_tau2Fix = new System.Windows.Forms.CheckBox();
            this.cb_tau1Fix = new System.Windows.Forms.CheckBox();
            this.frac2 = new System.Windows.Forms.Label();
            this.frac1 = new System.Windows.Forms.Label();
            this.label69 = new System.Windows.Forms.Label();
            this.tau_m = new System.Windows.Forms.Label();
            this.label66 = new System.Windows.Forms.Label();
            this.t0 = new System.Windows.Forms.TextBox();
            this.label65 = new System.Windows.Forms.Label();
            this.tauG = new System.Windows.Forms.TextBox();
            this.label63 = new System.Windows.Forms.Label();
            this.label64 = new System.Windows.Forms.Label();
            this.label62 = new System.Windows.Forms.Label();
            this.label61 = new System.Windows.Forms.Label();
            this.tau2 = new System.Windows.Forms.TextBox();
            this.pop2 = new System.Windows.Forms.TextBox();
            this.tau1 = new System.Windows.Forms.TextBox();
            this.pop1 = new System.Windows.Forms.TextBox();
            this.Values_selectedROI = new System.Windows.Forms.CheckBox();
            this.AllRois = new System.Windows.Forms.RadioButton();
            this.SelectRoi = new System.Windows.Forms.RadioButton();
            this.Fit = new System.Windows.Forms.Button();
            this.rightClickMenuStrip_inROI = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.convertToRectangularROIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.convertToElipsoidROIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.convertToPolygonROIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rmoeveRoiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.rightClickMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CreateRoi = new System.Windows.Forms.ToolStripMenuItem();
            this.scanThisRoiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Auto1 = new System.Windows.Forms.CheckBox();
            this.MinSldr1 = new System.Windows.Forms.TrackBar();
            this.MaxSldr1 = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.MinIntensity1 = new System.Windows.Forms.TextBox();
            this.MaxIntensity1 = new System.Windows.Forms.TextBox();
            this.MinSldr2 = new System.Windows.Forms.TrackBar();
            this.MaxSldr2 = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.MinIntensity2 = new System.Windows.Forms.TextBox();
            this.MaxIntensity2 = new System.Windows.Forms.TextBox();
            this.MinSldr3 = new System.Windows.Forms.TrackBar();
            this.MinIntensity3 = new System.Windows.Forms.TextBox();
            this.MaxSldr3 = new System.Windows.Forms.TrackBar();
            this.MaxIntensity3 = new System.Windows.Forms.TextBox();
            this.Fitting_Group = new System.Windows.Forms.GroupBox();
            this.Roi_SelectA = new System.Windows.Forms.ComboBox();
            this.psPerUnit = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.DoubleExp = new System.Windows.Forms.RadioButton();
            this.SingleExp = new System.Windows.Forms.RadioButton();
            this.FrameSlicePanel = new System.Windows.Forms.GroupBox();
            this.umPerSliceLabel = new System.Windows.Forms.Label();
            this.stopOpening = new System.Windows.Forms.Button();
            this.EntireStack_Check = new System.Windows.Forms.CheckBox();
            this.st_pageN = new System.Windows.Forms.Label();
            this.c_page = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.PageEnd = new System.Windows.Forms.TextBox();
            this.PageStart = new System.Windows.Forms.TextBox();
            this.PageDownDown = new System.Windows.Forms.Button();
            this.PageUpUp = new System.Windows.Forms.Button();
            this.PageUp = new System.Windows.Forms.Button();
            this.PageDown = new System.Windows.Forms.Button();
            this.AveProjection = new System.Windows.Forms.RadioButton();
            this.MaxProjection = new System.Windows.Forms.RadioButton();
            this.cb_projectionYes = new System.Windows.Forms.CheckBox();
            this.st_Filter = new System.Windows.Forms.Label();
            this.filterWindow = new System.Windows.Forms.TextBox();
            this.rightClickMenu_removeAll = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.Main_Menu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFLIMImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFLIMImageToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFLIMImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BatchProcessingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showImageDescriptionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.openFLIMImageInNewWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rOIsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveRoisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recoverRoisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllRoisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.readRoiFromImageJToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveRoiAsImageJToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncagingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setUncagingPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alignSlicesframesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeSinlgeFileMovieFromFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getFocusFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.fastZCalibrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteCurrentPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timeCoursePlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.driftMeasurementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setttingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keepPagesInMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.intelMKLLibraryOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FilePanel = new System.Windows.Forms.GroupBox();
            this.FileUp = new System.Windows.Forms.Button();
            this.FileDown = new System.Windows.Forms.Button();
            this.FileN = new System.Windows.Forms.TextBox();
            this.BaseName = new System.Windows.Forms.TextBox();
            this.st_fileN = new System.Windows.Forms.Label();
            this.st_BaseName = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.LifetimeCh_panel = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.Ch1 = new System.Windows.Forms.RadioButton();
            this.Ch2 = new System.Windows.Forms.RadioButton();
            this.Ch12 = new System.Windows.Forms.RadioButton();
            this.st_im1 = new System.Windows.Forms.Label();
            this.st_im2 = new System.Windows.Forms.Label();
            this.LifetimeCurvePlot = new System.Windows.Forms.PictureBox();
            this.st_panel = new System.Windows.Forms.Label();
            this.st_2Ch = new System.Windows.Forms.Label();
            this.st_1stCh = new System.Windows.Forms.Label();
            this.st_panel2 = new System.Windows.Forms.Label();
            this.Image2 = new System.Windows.Forms.PictureBox();
            this.Image1 = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.FrameAdjustment = new System.Windows.Forms.CheckBox();
            this.st_lifetime_ns = new System.Windows.Forms.Label();
            this.colorBar = new System.Windows.Forms.PictureBox();
            this.Square_Box = new System.Windows.Forms.PictureBox();
            this.ElipsoidBox = new System.Windows.Forms.PictureBox();
            this.UncagingBox = new System.Windows.Forms.PictureBox();
            this.PolygonBox = new System.Windows.Forms.PictureBox();
            this.ctrlPanel = new System.Windows.Forms.Panel();
            this.FastZPhasePanel = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.FastZFreqText = new System.Windows.Forms.Label();
            this.FastZAmpText = new System.Windows.Forms.Label();
            this.FastZPhaseText = new System.Windows.Forms.Label();
            this.PhaseDetectionMode_Status2 = new System.Windows.Forms.Label();
            this.PhaseDetectionMode_Status = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.FastZPanel = new System.Windows.Forms.GroupBox();
            this.TotalFastZFrame = new System.Windows.Forms.Label();
            this.FastZUp = new System.Windows.Forms.Button();
            this.FastZDown = new System.Windows.Forms.Button();
            this.CurrentFastZPageTB = new System.Windows.Forms.TextBox();
            this.st_im3 = new System.Windows.Forms.Panel();
            this.rightClick_CreateUncaging = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CreateUncagingLoc = new System.Windows.Forms.ToolStripMenuItem();
            this.rightClick_removeUncaging = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.RemoveAllUncaging = new System.Windows.Forms.ToolStripMenuItem();
            this.createRoiToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.rightClick_remUncageEach = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeUncagingEach = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HoldCurrentImageCheckBox = new System.Windows.Forms.CheckBox();
            this.MergeCB = new System.Windows.Forms.CheckBox();
            this.ThreeDROIPanel = new System.Windows.Forms.PictureBox();
            this.pythonScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.averageTimeCoursePythonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setPythonPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setScriptPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runPythonScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rightClickMenuStrip_inROI.SuspendLayout();
            this.rightClickMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinSldr1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxSldr1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinSldr2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxSldr2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinSldr3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxSldr3)).BeginInit();
            this.Fitting_Group.SuspendLayout();
            this.panel1.SuspendLayout();
            this.FrameSlicePanel.SuspendLayout();
            this.rightClickMenu_removeAll.SuspendLayout();
            this.Main_Menu.SuspendLayout();
            this.FilePanel.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.LifetimeCh_panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LifetimeCurvePlot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Image2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Image1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.colorBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Square_Box)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ElipsoidBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UncagingBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PolygonBox)).BeginInit();
            this.ctrlPanel.SuspendLayout();
            this.FastZPhasePanel.SuspendLayout();
            this.FastZPanel.SuspendLayout();
            this.st_im3.SuspendLayout();
            this.rightClick_CreateUncaging.SuspendLayout();
            this.rightClick_removeUncaging.SuspendLayout();
            this.rightClick_remUncageEach.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ThreeDROIPanel)).BeginInit();
            this.SuspendLayout();
            // 
            // logScale
            // 
            this.logScale.AutoSize = true;
            this.logScale.Checked = true;
            this.logScale.CheckState = System.Windows.Forms.CheckState.Checked;
            this.logScale.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logScale.Location = new System.Drawing.Point(212, 2);
            this.logScale.Name = "logScale";
            this.logScale.Size = new System.Drawing.Size(73, 18);
            this.logScale.TabIndex = 298;
            this.logScale.Text = "Log scale";
            this.logScale.UseVisualStyleBackColor = true;
            this.logScale.Click += new System.EventHandler(this.UpdateImage_Simple);
            // 
            // ShowFLIM
            // 
            this.ShowFLIM.AutoSize = true;
            this.ShowFLIM.Checked = true;
            this.ShowFLIM.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowFLIM.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ShowFLIM.Location = new System.Drawing.Point(437, 33);
            this.ShowFLIM.Name = "ShowFLIM";
            this.ShowFLIM.Size = new System.Drawing.Size(80, 18);
            this.ShowFLIM.TabIndex = 299;
            this.ShowFLIM.Text = "Show FLIM";
            this.ShowFLIM.UseVisualStyleBackColor = true;
            this.ShowFLIM.Click += new System.EventHandler(this.Channel1_CheckedChanged);
            // 
            // Channel1
            // 
            this.Channel1.AutoSize = true;
            this.Channel1.Checked = true;
            this.Channel1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Channel1.Location = new System.Drawing.Point(11, 21);
            this.Channel1.Name = "Channel1";
            this.Channel1.Size = new System.Drawing.Size(150, 20);
            this.Channel1.TabIndex = 300;
            this.Channel1.TabStop = true;
            this.Channel1.Text = "Ch1 (Intensity/FLIM)";
            this.Channel1.UseVisualStyleBackColor = true;
            this.Channel1.Click += new System.EventHandler(this.Channel1_CheckedChanged);
            // 
            // Channel2
            // 
            this.Channel2.AutoSize = true;
            this.Channel2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Channel2.Location = new System.Drawing.Point(11, 50);
            this.Channel2.Name = "Channel2";
            this.Channel2.Size = new System.Drawing.Size(150, 20);
            this.Channel2.TabIndex = 301;
            this.Channel2.Text = "Ch2 (Intensity/FLIM)";
            this.Channel2.UseVisualStyleBackColor = true;
            this.Channel2.Click += new System.EventHandler(this.Channel1_CheckedChanged);
            // 
            // imgOffset1
            // 
            this.imgOffset1.AutoSize = true;
            this.imgOffset1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.imgOffset1.Location = new System.Drawing.Point(175, 106);
            this.imgOffset1.Name = "imgOffset1";
            this.imgOffset1.Size = new System.Drawing.Size(37, 14);
            this.imgOffset1.TabIndex = 431;
            this.imgOffset1.Text = "2.0 ns";
            // 
            // Apply_Offset
            // 
            this.Apply_Offset.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Apply_Offset.Location = new System.Drawing.Point(195, 142);
            this.Apply_Offset.Name = "Apply_Offset";
            this.Apply_Offset.Size = new System.Drawing.Size(48, 23);
            this.Apply_Offset.TabIndex = 430;
            this.Apply_Offset.Text = "Apply";
            this.Apply_Offset.UseVisualStyleBackColor = true;
            this.Apply_Offset.Click += new System.EventHandler(this.Apply_Offset_Click);
            // 
            // fix_all
            // 
            this.fix_all.AutoSize = true;
            this.fix_all.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fix_all.Location = new System.Drawing.Point(98, 158);
            this.fix_all.Name = "fix_all";
            this.fix_all.Size = new System.Drawing.Size(81, 18);
            this.fix_all.TabIndex = 429;
            this.fix_all.Text = "Fix/Unfix all";
            this.fix_all.UseVisualStyleBackColor = true;
            this.fix_all.Click += new System.EventHandler(this.FixAll_CheckedChanged);
            // 
            // label75
            // 
            this.label75.AutoSize = true;
            this.label75.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label75.Location = new System.Drawing.Point(9, 174);
            this.label75.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label75.Name = "label75";
            this.label75.Size = new System.Drawing.Size(52, 14);
            this.label75.TabIndex = 428;
            this.label75.Text = "xi square";
            // 
            // xi_square
            // 
            this.xi_square.AutoSize = true;
            this.xi_square.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xi_square.Location = new System.Drawing.Point(61, 176);
            this.xi_square.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.xi_square.Name = "xi_square";
            this.xi_square.Size = new System.Drawing.Size(34, 14);
            this.xi_square.TabIndex = 427;
            this.xi_square.Text = "0.000";
            this.xi_square.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label73
            // 
            this.label73.AutoSize = true;
            this.label73.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label73.Location = new System.Drawing.Point(158, 50);
            this.label73.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label73.Name = "label73";
            this.label73.Size = new System.Drawing.Size(39, 14);
            this.label73.TabIndex = 426;
            this.label73.Text = "Fit end";
            // 
            // label74
            // 
            this.label74.AutoSize = true;
            this.label74.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label74.Location = new System.Drawing.Point(158, 16);
            this.label74.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label74.Name = "label74";
            this.label74.Size = new System.Drawing.Size(43, 14);
            this.label74.TabIndex = 425;
            this.label74.Text = "Fit start";
            // 
            // fit_end
            // 
            this.fit_end.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fit_end.Location = new System.Drawing.Point(178, 62);
            this.fit_end.Margin = new System.Windows.Forms.Padding(1);
            this.fit_end.Name = "fit_end";
            this.fit_end.Size = new System.Drawing.Size(50, 20);
            this.fit_end.TabIndex = 424;
            this.fit_end.Text = "0";
            this.fit_end.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.fit_end.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Analysis_KeyDown);
            // 
            // label67
            // 
            this.label67.AutoSize = true;
            this.label67.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label67.Location = new System.Drawing.Point(158, 90);
            this.label67.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label67.Name = "label67";
            this.label67.Size = new System.Drawing.Size(85, 14);
            this.label67.TabIndex = 402;
            this.label67.Text = "T0 (Image color)";
            // 
            // t0_Img
            // 
            this.t0_Img.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.t0_Img.Location = new System.Drawing.Point(178, 121);
            this.t0_Img.Margin = new System.Windows.Forms.Padding(1);
            this.t0_Img.Name = "t0_Img";
            this.t0_Img.Size = new System.Drawing.Size(50, 20);
            this.t0_Img.TabIndex = 401;
            this.t0_Img.Text = "0";
            this.t0_Img.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // fit_start
            // 
            this.fit_start.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fit_start.Location = new System.Drawing.Point(178, 30);
            this.fit_start.Margin = new System.Windows.Forms.Padding(1);
            this.fit_start.Name = "fit_start";
            this.fit_start.Size = new System.Drawing.Size(50, 20);
            this.fit_start.TabIndex = 423;
            this.fit_start.Text = "0";
            this.fit_start.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.fit_start.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Analysis_KeyDown);
            // 
            // cb_T0Fix
            // 
            this.cb_T0Fix.AutoSize = true;
            this.cb_T0Fix.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_T0Fix.Location = new System.Drawing.Point(98, 135);
            this.cb_T0Fix.Name = "cb_T0Fix";
            this.cb_T0Fix.Size = new System.Drawing.Size(40, 18);
            this.cb_T0Fix.TabIndex = 422;
            this.cb_T0Fix.Text = "Fix";
            this.cb_T0Fix.UseVisualStyleBackColor = true;
            this.cb_T0Fix.Click += new System.EventHandler(this.Fix_Check_Changed);
            // 
            // cb_tauGFix
            // 
            this.cb_tauGFix.AutoSize = true;
            this.cb_tauGFix.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_tauGFix.Location = new System.Drawing.Point(98, 113);
            this.cb_tauGFix.Name = "cb_tauGFix";
            this.cb_tauGFix.Size = new System.Drawing.Size(40, 18);
            this.cb_tauGFix.TabIndex = 421;
            this.cb_tauGFix.Text = "Fix";
            this.cb_tauGFix.UseVisualStyleBackColor = true;
            this.cb_tauGFix.Click += new System.EventHandler(this.Fix_Check_Changed);
            // 
            // cb_tau2Fix
            // 
            this.cb_tau2Fix.AutoSize = true;
            this.cb_tau2Fix.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_tau2Fix.Location = new System.Drawing.Point(98, 91);
            this.cb_tau2Fix.Name = "cb_tau2Fix";
            this.cb_tau2Fix.Size = new System.Drawing.Size(40, 18);
            this.cb_tau2Fix.TabIndex = 420;
            this.cb_tau2Fix.Text = "Fix";
            this.cb_tau2Fix.UseVisualStyleBackColor = true;
            this.cb_tau2Fix.Click += new System.EventHandler(this.Fix_Check_Changed);
            // 
            // cb_tau1Fix
            // 
            this.cb_tau1Fix.AutoSize = true;
            this.cb_tau1Fix.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_tau1Fix.Location = new System.Drawing.Point(98, 46);
            this.cb_tau1Fix.Name = "cb_tau1Fix";
            this.cb_tau1Fix.Size = new System.Drawing.Size(40, 18);
            this.cb_tau1Fix.TabIndex = 419;
            this.cb_tau1Fix.Text = "Fix";
            this.cb_tau1Fix.UseVisualStyleBackColor = true;
            this.cb_tau1Fix.Click += new System.EventHandler(this.Fix_Check_Changed);
            // 
            // frac2
            // 
            this.frac2.AutoSize = true;
            this.frac2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.frac2.Location = new System.Drawing.Point(95, 70);
            this.frac2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.frac2.Name = "frac2";
            this.frac2.Size = new System.Drawing.Size(34, 14);
            this.frac2.TabIndex = 418;
            this.frac2.Text = "0.000";
            this.frac2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // frac1
            // 
            this.frac1.AutoSize = true;
            this.frac1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.frac1.Location = new System.Drawing.Point(95, 26);
            this.frac1.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.frac1.Name = "frac1";
            this.frac1.Size = new System.Drawing.Size(34, 14);
            this.frac1.TabIndex = 417;
            this.frac1.Text = "0.000";
            this.frac1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label69
            // 
            this.label69.AutoSize = true;
            this.label69.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label69.Location = new System.Drawing.Point(8, 159);
            this.label69.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label69.Name = "label69";
            this.label69.Size = new System.Drawing.Size(53, 14);
            this.label69.TabIndex = 416;
            this.label69.Text = "Mean Tau";
            // 
            // tau_m
            // 
            this.tau_m.AutoSize = true;
            this.tau_m.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tau_m.Location = new System.Drawing.Point(61, 159);
            this.tau_m.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.tau_m.Name = "tau_m";
            this.tau_m.Size = new System.Drawing.Size(34, 14);
            this.tau_m.TabIndex = 415;
            this.tau_m.Text = "0.000";
            this.tau_m.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label66
            // 
            this.label66.AutoSize = true;
            this.label66.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label66.Location = new System.Drawing.Point(15, 139);
            this.label66.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label66.Name = "label66";
            this.label66.Size = new System.Drawing.Size(19, 14);
            this.label66.TabIndex = 414;
            this.label66.Text = "T0";
            // 
            // t0
            // 
            this.t0.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.t0.Location = new System.Drawing.Point(43, 133);
            this.t0.Margin = new System.Windows.Forms.Padding(1);
            this.t0.Name = "t0";
            this.t0.Size = new System.Drawing.Size(50, 20);
            this.t0.TabIndex = 413;
            this.t0.Text = "0";
            this.t0.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.t0.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Analysis_KeyDown);
            // 
            // label65
            // 
            this.label65.AutoSize = true;
            this.label65.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label65.Location = new System.Drawing.Point(8, 116);
            this.label65.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label65.Name = "label65";
            this.label65.Size = new System.Drawing.Size(32, 14);
            this.label65.TabIndex = 412;
            this.label65.Text = "TauG";
            // 
            // tauG
            // 
            this.tauG.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tauG.Location = new System.Drawing.Point(43, 111);
            this.tauG.Margin = new System.Windows.Forms.Padding(1);
            this.tauG.Name = "tauG";
            this.tauG.Size = new System.Drawing.Size(50, 20);
            this.tauG.TabIndex = 411;
            this.tauG.Text = "0";
            this.tauG.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tauG.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Analysis_KeyDown);
            // 
            // label63
            // 
            this.label63.AutoSize = true;
            this.label63.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label63.Location = new System.Drawing.Point(9, 69);
            this.label63.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label63.Name = "label63";
            this.label63.Size = new System.Drawing.Size(31, 14);
            this.label63.TabIndex = 410;
            this.label63.Text = "Pop2";
            // 
            // label64
            // 
            this.label64.AutoSize = true;
            this.label64.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label64.Location = new System.Drawing.Point(9, 26);
            this.label64.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label64.Name = "label64";
            this.label64.Size = new System.Drawing.Size(31, 14);
            this.label64.TabIndex = 409;
            this.label64.Text = "Pop1";
            // 
            // label62
            // 
            this.label62.AutoSize = true;
            this.label62.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label62.Location = new System.Drawing.Point(9, 92);
            this.label62.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(30, 14);
            this.label62.TabIndex = 408;
            this.label62.Text = "Tau2";
            // 
            // label61
            // 
            this.label61.AutoSize = true;
            this.label61.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label61.Location = new System.Drawing.Point(9, 49);
            this.label61.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(30, 14);
            this.label61.TabIndex = 407;
            this.label61.Text = "Tau1";
            // 
            // tau2
            // 
            this.tau2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tau2.Location = new System.Drawing.Point(43, 89);
            this.tau2.Margin = new System.Windows.Forms.Padding(1);
            this.tau2.Name = "tau2";
            this.tau2.Size = new System.Drawing.Size(50, 20);
            this.tau2.TabIndex = 406;
            this.tau2.Text = "0";
            this.tau2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tau2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Analysis_KeyDown);
            // 
            // pop2
            // 
            this.pop2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pop2.Location = new System.Drawing.Point(43, 67);
            this.pop2.Margin = new System.Windows.Forms.Padding(1);
            this.pop2.Name = "pop2";
            this.pop2.Size = new System.Drawing.Size(50, 20);
            this.pop2.TabIndex = 403;
            this.pop2.Text = "0";
            this.pop2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.pop2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Analysis_KeyDown);
            // 
            // tau1
            // 
            this.tau1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tau1.Location = new System.Drawing.Point(43, 45);
            this.tau1.Margin = new System.Windows.Forms.Padding(1);
            this.tau1.Name = "tau1";
            this.tau1.Size = new System.Drawing.Size(50, 20);
            this.tau1.TabIndex = 405;
            this.tau1.Text = "0";
            this.tau1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tau1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Analysis_KeyDown);
            // 
            // pop1
            // 
            this.pop1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pop1.Location = new System.Drawing.Point(43, 23);
            this.pop1.Margin = new System.Windows.Forms.Padding(1);
            this.pop1.Name = "pop1";
            this.pop1.Size = new System.Drawing.Size(50, 20);
            this.pop1.TabIndex = 404;
            this.pop1.Text = "0";
            this.pop1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.pop1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Analysis_KeyDown);
            // 
            // Values_selectedROI
            // 
            this.Values_selectedROI.AutoSize = true;
            this.Values_selectedROI.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Values_selectedROI.Location = new System.Drawing.Point(266, 53);
            this.Values_selectedROI.Name = "Values_selectedROI";
            this.Values_selectedROI.Size = new System.Drawing.Size(104, 32);
            this.Values_selectedROI.TabIndex = 399;
            this.Values_selectedROI.Text = "Show values \r\nfor selected ROI\r\n";
            this.Values_selectedROI.UseVisualStyleBackColor = true;
            this.Values_selectedROI.Click += new System.EventHandler(this.Fix_Check_Changed);
            // 
            // AllRois
            // 
            this.AllRois.AutoSize = true;
            this.AllRois.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AllRois.Location = new System.Drawing.Point(251, 39);
            this.AllRois.Name = "AllRois";
            this.AllRois.Size = new System.Drawing.Size(106, 18);
            this.AllRois.TabIndex = 398;
            this.AllRois.Text = "Multi ROIs (blue) ";
            this.AllRois.UseVisualStyleBackColor = true;
            this.AllRois.Click += new System.EventHandler(this.Fix_Check_Changed);
            // 
            // SelectRoi
            // 
            this.SelectRoi.AutoSize = true;
            this.SelectRoi.Checked = true;
            this.SelectRoi.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectRoi.Location = new System.Drawing.Point(251, 20);
            this.SelectRoi.Name = "SelectRoi";
            this.SelectRoi.Size = new System.Drawing.Size(114, 18);
            this.SelectRoi.TabIndex = 397;
            this.SelectRoi.TabStop = true;
            this.SelectRoi.Text = "Selected ROI (red)";
            this.SelectRoi.UseVisualStyleBackColor = true;
            this.SelectRoi.Click += new System.EventHandler(this.Fix_Check_Changed);
            // 
            // Fit
            // 
            this.Fit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Fit.Location = new System.Drawing.Point(11, 40);
            this.Fit.Name = "Fit";
            this.Fit.Size = new System.Drawing.Size(75, 23);
            this.Fit.TabIndex = 393;
            this.Fit.Text = "Fit";
            this.Fit.UseVisualStyleBackColor = true;
            this.Fit.Click += new System.EventHandler(this.Fit_Click);
            // 
            // rightClickMenuStrip_inROI
            // 
            this.rightClickMenuStrip_inROI.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.rightClickMenuStrip_inROI.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.convertToRectangularROIToolStripMenuItem,
            this.convertToElipsoidROIToolStripMenuItem,
            this.convertToPolygonROIToolStripMenuItem,
            this.rmoeveRoiToolStripMenuItem,
            this.removeAllToolStripMenuItem1});
            this.rightClickMenuStrip_inROI.Name = "rightClickMenuStrip2";
            this.rightClickMenuStrip_inROI.Size = new System.Drawing.Size(219, 114);
            // 
            // convertToRectangularROIToolStripMenuItem
            // 
            this.convertToRectangularROIToolStripMenuItem.Name = "convertToRectangularROIToolStripMenuItem";
            this.convertToRectangularROIToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.convertToRectangularROIToolStripMenuItem.Text = "Convert to Rectangular ROI";
            this.convertToRectangularROIToolStripMenuItem.Click += new System.EventHandler(this.convertToRectangularROIToolStripMenuItem_Click);
            // 
            // convertToElipsoidROIToolStripMenuItem
            // 
            this.convertToElipsoidROIToolStripMenuItem.Name = "convertToElipsoidROIToolStripMenuItem";
            this.convertToElipsoidROIToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.convertToElipsoidROIToolStripMenuItem.Text = "Convert to Elipsoid ROI";
            this.convertToElipsoidROIToolStripMenuItem.Click += new System.EventHandler(this.convertToElipsoidROIToolStripMenuItem_Click);
            // 
            // convertToPolygonROIToolStripMenuItem
            // 
            this.convertToPolygonROIToolStripMenuItem.Name = "convertToPolygonROIToolStripMenuItem";
            this.convertToPolygonROIToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.convertToPolygonROIToolStripMenuItem.Text = "Convert to Polygon ROI";
            this.convertToPolygonROIToolStripMenuItem.Click += new System.EventHandler(this.convertToPolygonROIToolStripMenuItem_Click);
            // 
            // rmoeveRoiToolStripMenuItem
            // 
            this.rmoeveRoiToolStripMenuItem.Name = "rmoeveRoiToolStripMenuItem";
            this.rmoeveRoiToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.rmoeveRoiToolStripMenuItem.Text = "Rmoeve this";
            this.rmoeveRoiToolStripMenuItem.Click += new System.EventHandler(this.RemoveRoi_MenuItemClick);
            // 
            // removeAllToolStripMenuItem1
            // 
            this.removeAllToolStripMenuItem1.Name = "removeAllToolStripMenuItem1";
            this.removeAllToolStripMenuItem1.Size = new System.Drawing.Size(218, 22);
            this.removeAllToolStripMenuItem1.Text = "Remove all";
            this.removeAllToolStripMenuItem1.Click += new System.EventHandler(this.RemoveAllROIs);
            // 
            // rightClickMenuStrip
            // 
            this.rightClickMenuStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.rightClickMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CreateRoi,
            this.scanThisRoiToolStripMenuItem});
            this.rightClickMenuStrip.Name = "rightClickMenuStrip";
            this.rightClickMenuStrip.Size = new System.Drawing.Size(144, 48);
            // 
            // CreateRoi
            // 
            this.CreateRoi.Name = "CreateRoi";
            this.CreateRoi.Size = new System.Drawing.Size(143, 22);
            this.CreateRoi.Text = "Create ROI";
            this.CreateRoi.Click += new System.EventHandler(this.CreateRoi_MenuItemClick);
            // 
            // scanThisRoiToolStripMenuItem
            // 
            this.scanThisRoiToolStripMenuItem.Name = "scanThisRoiToolStripMenuItem";
            this.scanThisRoiToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.scanThisRoiToolStripMenuItem.Text = "Scan this ROI";
            this.scanThisRoiToolStripMenuItem.Click += new System.EventHandler(this.scanThisRoiToolStripMenuItem_Click);
            // 
            // Auto1
            // 
            this.Auto1.AutoSize = true;
            this.Auto1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Auto1.Location = new System.Drawing.Point(14, 19);
            this.Auto1.Name = "Auto1";
            this.Auto1.Size = new System.Drawing.Size(92, 18);
            this.Auto1.TabIndex = 440;
            this.Auto1.Text = "Auto contrast";
            this.Auto1.UseVisualStyleBackColor = true;
            this.Auto1.Click += new System.EventHandler(this.Auto1_CheckedChanged);
            // 
            // MinSldr1
            // 
            this.MinSldr1.AutoSize = false;
            this.MinSldr1.LargeChange = 10;
            this.MinSldr1.Location = new System.Drawing.Point(58, 121);
            this.MinSldr1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinSldr1.Maximum = 100;
            this.MinSldr1.Name = "MinSldr1";
            this.MinSldr1.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.MinSldr1.Size = new System.Drawing.Size(15, 75);
            this.MinSldr1.TabIndex = 439;
            this.MinSldr1.TickStyle = System.Windows.Forms.TickStyle.None;
            this.MinSldr1.Scroll += new System.EventHandler(this.Slider_ValueChanged);
            this.MinSldr1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Slider_mouseUp);
            // 
            // MaxSldr1
            // 
            this.MaxSldr1.AutoSize = false;
            this.MaxSldr1.LargeChange = 10;
            this.MaxSldr1.Location = new System.Drawing.Point(19, 121);
            this.MaxSldr1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaxSldr1.Maximum = 100;
            this.MaxSldr1.Name = "MaxSldr1";
            this.MaxSldr1.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.MaxSldr1.Size = new System.Drawing.Size(15, 75);
            this.MaxSldr1.TabIndex = 438;
            this.MaxSldr1.TickStyle = System.Windows.Forms.TickStyle.None;
            this.MaxSldr1.Value = 100;
            this.MaxSldr1.Scroll += new System.EventHandler(this.Slider_ValueChanged);
            this.MaxSldr1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Slider_mouseUp);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(52, 81);
            this.label4.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 14);
            this.label4.TabIndex = 437;
            this.label4.Text = "Min";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(8, 81);
            this.label3.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 14);
            this.label3.TabIndex = 436;
            this.label3.Text = "Max";
            // 
            // MinIntensity1
            // 
            this.MinIntensity1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinIntensity1.Location = new System.Drawing.Point(43, 100);
            this.MinIntensity1.Margin = new System.Windows.Forms.Padding(1);
            this.MinIntensity1.Name = "MinIntensity1";
            this.MinIntensity1.Size = new System.Drawing.Size(40, 20);
            this.MinIntensity1.TabIndex = 435;
            this.MinIntensity1.Text = "0";
            this.MinIntensity1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MinIntensity1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UpdateFigure_KeyDown);
            // 
            // MaxIntensity1
            // 
            this.MaxIntensity1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaxIntensity1.Location = new System.Drawing.Point(4, 100);
            this.MaxIntensity1.Margin = new System.Windows.Forms.Padding(1);
            this.MaxIntensity1.Name = "MaxIntensity1";
            this.MaxIntensity1.Size = new System.Drawing.Size(40, 20);
            this.MaxIntensity1.TabIndex = 434;
            this.MaxIntensity1.Text = "100";
            this.MaxIntensity1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MaxIntensity1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UpdateFigure_KeyDown);
            // 
            // MinSldr2
            // 
            this.MinSldr2.AutoSize = false;
            this.MinSldr2.LargeChange = 10;
            this.MinSldr2.Location = new System.Drawing.Point(137, 121);
            this.MinSldr2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinSldr2.Maximum = 100;
            this.MinSldr2.Name = "MinSldr2";
            this.MinSldr2.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.MinSldr2.Size = new System.Drawing.Size(15, 75);
            this.MinSldr2.TabIndex = 446;
            this.MinSldr2.TickStyle = System.Windows.Forms.TickStyle.None;
            this.MinSldr2.Scroll += new System.EventHandler(this.Slider_ValueChanged);
            this.MinSldr2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Slider_mouseUp);
            // 
            // MaxSldr2
            // 
            this.MaxSldr2.AutoSize = false;
            this.MaxSldr2.LargeChange = 10;
            this.MaxSldr2.Location = new System.Drawing.Point(101, 121);
            this.MaxSldr2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaxSldr2.Maximum = 100;
            this.MaxSldr2.Name = "MaxSldr2";
            this.MaxSldr2.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.MaxSldr2.Size = new System.Drawing.Size(15, 75);
            this.MaxSldr2.TabIndex = 445;
            this.MaxSldr2.TickStyle = System.Windows.Forms.TickStyle.None;
            this.MaxSldr2.Value = 100;
            this.MaxSldr2.Scroll += new System.EventHandler(this.Slider_ValueChanged);
            this.MaxSldr2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Slider_mouseUp);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(131, 81);
            this.label5.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 14);
            this.label5.TabIndex = 444;
            this.label5.Text = "Min";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(90, 81);
            this.label7.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(27, 14);
            this.label7.TabIndex = 443;
            this.label7.Text = "Max";
            // 
            // MinIntensity2
            // 
            this.MinIntensity2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinIntensity2.Location = new System.Drawing.Point(123, 100);
            this.MinIntensity2.Margin = new System.Windows.Forms.Padding(1);
            this.MinIntensity2.Name = "MinIntensity2";
            this.MinIntensity2.Size = new System.Drawing.Size(40, 20);
            this.MinIntensity2.TabIndex = 442;
            this.MinIntensity2.Text = "0";
            this.MinIntensity2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MinIntensity2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UpdateFigure_KeyDown);
            // 
            // MaxIntensity2
            // 
            this.MaxIntensity2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaxIntensity2.Location = new System.Drawing.Point(84, 100);
            this.MaxIntensity2.Margin = new System.Windows.Forms.Padding(1);
            this.MaxIntensity2.Name = "MaxIntensity2";
            this.MaxIntensity2.Size = new System.Drawing.Size(40, 20);
            this.MaxIntensity2.TabIndex = 441;
            this.MaxIntensity2.Text = "100";
            this.MaxIntensity2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MaxIntensity2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UpdateFigure_KeyDown);
            // 
            // MinSldr3
            // 
            this.MinSldr3.AutoSize = false;
            this.MinSldr3.BackColor = System.Drawing.SystemColors.Highlight;
            this.MinSldr3.LargeChange = 10;
            this.MinSldr3.Location = new System.Drawing.Point(209, 122);
            this.MinSldr3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinSldr3.Maximum = 1000;
            this.MinSldr3.Name = "MinSldr3";
            this.MinSldr3.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.MinSldr3.Size = new System.Drawing.Size(15, 75);
            this.MinSldr3.TabIndex = 453;
            this.MinSldr3.TickStyle = System.Windows.Forms.TickStyle.None;
            this.MinSldr3.Value = 220;
            this.MinSldr3.Scroll += new System.EventHandler(this.Slider_ValueChanged);
            this.MinSldr3.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Slider_mouseUp);
            // 
            // MinIntensity3
            // 
            this.MinIntensity3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinIntensity3.Location = new System.Drawing.Point(201, 100);
            this.MinIntensity3.Margin = new System.Windows.Forms.Padding(1);
            this.MinIntensity3.Name = "MinIntensity3";
            this.MinIntensity3.Size = new System.Drawing.Size(30, 20);
            this.MinIntensity3.TabIndex = 450;
            this.MinIntensity3.Text = "2.00";
            this.MinIntensity3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MinIntensity3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UpdateFigure_KeyDown);
            // 
            // MaxSldr3
            // 
            this.MaxSldr3.AutoSize = false;
            this.MaxSldr3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.MaxSldr3.LargeChange = 10;
            this.MaxSldr3.Location = new System.Drawing.Point(169, 122);
            this.MaxSldr3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaxSldr3.Maximum = 1000;
            this.MaxSldr3.Name = "MaxSldr3";
            this.MaxSldr3.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.MaxSldr3.Size = new System.Drawing.Size(15, 75);
            this.MaxSldr3.TabIndex = 452;
            this.MaxSldr3.TickStyle = System.Windows.Forms.TickStyle.None;
            this.MaxSldr3.Value = 270;
            this.MaxSldr3.Scroll += new System.EventHandler(this.Slider_ValueChanged);
            this.MaxSldr3.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Slider_mouseUp);
            // 
            // MaxIntensity3
            // 
            this.MaxIntensity3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaxIntensity3.Location = new System.Drawing.Point(161, 100);
            this.MaxIntensity3.Margin = new System.Windows.Forms.Padding(1);
            this.MaxIntensity3.Name = "MaxIntensity3";
            this.MaxIntensity3.Size = new System.Drawing.Size(30, 20);
            this.MaxIntensity3.TabIndex = 449;
            this.MaxIntensity3.Text = "3.00";
            this.MaxIntensity3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MaxIntensity3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UpdateFigure_KeyDown);
            // 
            // Fitting_Group
            // 
            this.Fitting_Group.Controls.Add(this.Roi_SelectA);
            this.Fitting_Group.Controls.Add(this.psPerUnit);
            this.Fitting_Group.Controls.Add(this.label10);
            this.Fitting_Group.Controls.Add(this.panel1);
            this.Fitting_Group.Controls.Add(this.fit_end);
            this.Fitting_Group.Controls.Add(this.fit_start);
            this.Fitting_Group.Controls.Add(this.imgOffset1);
            this.Fitting_Group.Controls.Add(this.Apply_Offset);
            this.Fitting_Group.Controls.Add(this.fix_all);
            this.Fitting_Group.Controls.Add(this.label75);
            this.Fitting_Group.Controls.Add(this.xi_square);
            this.Fitting_Group.Controls.Add(this.label73);
            this.Fitting_Group.Controls.Add(this.label74);
            this.Fitting_Group.Controls.Add(this.label67);
            this.Fitting_Group.Controls.Add(this.t0_Img);
            this.Fitting_Group.Controls.Add(this.cb_T0Fix);
            this.Fitting_Group.Controls.Add(this.cb_tauGFix);
            this.Fitting_Group.Controls.Add(this.cb_tau2Fix);
            this.Fitting_Group.Controls.Add(this.cb_tau1Fix);
            this.Fitting_Group.Controls.Add(this.frac2);
            this.Fitting_Group.Controls.Add(this.frac1);
            this.Fitting_Group.Controls.Add(this.label69);
            this.Fitting_Group.Controls.Add(this.tau_m);
            this.Fitting_Group.Controls.Add(this.label66);
            this.Fitting_Group.Controls.Add(this.t0);
            this.Fitting_Group.Controls.Add(this.label65);
            this.Fitting_Group.Controls.Add(this.tauG);
            this.Fitting_Group.Controls.Add(this.label63);
            this.Fitting_Group.Controls.Add(this.label64);
            this.Fitting_Group.Controls.Add(this.label62);
            this.Fitting_Group.Controls.Add(this.label61);
            this.Fitting_Group.Controls.Add(this.tau2);
            this.Fitting_Group.Controls.Add(this.pop2);
            this.Fitting_Group.Controls.Add(this.tau1);
            this.Fitting_Group.Controls.Add(this.pop1);
            this.Fitting_Group.Controls.Add(this.Values_selectedROI);
            this.Fitting_Group.Controls.Add(this.AllRois);
            this.Fitting_Group.Controls.Add(this.SelectRoi);
            this.Fitting_Group.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Fitting_Group.Location = new System.Drawing.Point(670, 3);
            this.Fitting_Group.Name = "Fitting_Group";
            this.Fitting_Group.Size = new System.Drawing.Size(380, 210);
            this.Fitting_Group.TabIndex = 455;
            this.Fitting_Group.TabStop = false;
            this.Fitting_Group.Text = "Fitting";
            // 
            // Roi_SelectA
            // 
            this.Roi_SelectA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Roi_SelectA.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Roi_SelectA.FormattingEnabled = true;
            this.Roi_SelectA.Location = new System.Drawing.Point(275, 88);
            this.Roi_SelectA.Name = "Roi_SelectA";
            this.Roi_SelectA.Size = new System.Drawing.Size(95, 22);
            this.Roi_SelectA.TabIndex = 439;
            this.Roi_SelectA.SelectedIndexChanged += new System.EventHandler(this.Roi_SelectA_SelectedIndexChanged);
            // 
            // psPerUnit
            // 
            this.psPerUnit.AutoSize = true;
            this.psPerUnit.Font = new System.Drawing.Font("Arial", 8.25F);
            this.psPerUnit.Location = new System.Drawing.Point(195, 183);
            this.psPerUnit.Name = "psPerUnit";
            this.psPerUnit.Size = new System.Drawing.Size(19, 14);
            this.psPerUnit.TabIndex = 438;
            this.psPerUnit.Text = "ps";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Arial", 8.25F);
            this.label10.Location = new System.Drawing.Point(133, 183);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(67, 14);
            this.label10.TabIndex = 437;
            this.label10.Text = "Time point = ";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.DoubleExp);
            this.panel1.Controls.Add(this.SingleExp);
            this.panel1.Controls.Add(this.Fit);
            this.panel1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.Location = new System.Drawing.Point(271, 124);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(102, 73);
            this.panel1.TabIndex = 436;
            // 
            // DoubleExp
            // 
            this.DoubleExp.AutoSize = true;
            this.DoubleExp.Checked = true;
            this.DoubleExp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.DoubleExp.Location = new System.Drawing.Point(11, 20);
            this.DoubleExp.Name = "DoubleExp";
            this.DoubleExp.Size = new System.Drawing.Size(80, 17);
            this.DoubleExp.TabIndex = 435;
            this.DoubleExp.TabStop = true;
            this.DoubleExp.Text = "Double Exp";
            this.DoubleExp.UseVisualStyleBackColor = true;
            // 
            // SingleExp
            // 
            this.SingleExp.AutoSize = true;
            this.SingleExp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.SingleExp.Location = new System.Drawing.Point(11, 5);
            this.SingleExp.Name = "SingleExp";
            this.SingleExp.Size = new System.Drawing.Size(75, 17);
            this.SingleExp.TabIndex = 434;
            this.SingleExp.Text = "Single Exp";
            this.SingleExp.UseVisualStyleBackColor = true;
            // 
            // FrameSlicePanel
            // 
            this.FrameSlicePanel.Controls.Add(this.umPerSliceLabel);
            this.FrameSlicePanel.Controls.Add(this.stopOpening);
            this.FrameSlicePanel.Controls.Add(this.EntireStack_Check);
            this.FrameSlicePanel.Controls.Add(this.st_pageN);
            this.FrameSlicePanel.Controls.Add(this.c_page);
            this.FrameSlicePanel.Controls.Add(this.label9);
            this.FrameSlicePanel.Controls.Add(this.label6);
            this.FrameSlicePanel.Controls.Add(this.PageEnd);
            this.FrameSlicePanel.Controls.Add(this.PageStart);
            this.FrameSlicePanel.Controls.Add(this.PageDownDown);
            this.FrameSlicePanel.Controls.Add(this.PageUpUp);
            this.FrameSlicePanel.Controls.Add(this.PageUp);
            this.FrameSlicePanel.Controls.Add(this.PageDown);
            this.FrameSlicePanel.Controls.Add(this.AveProjection);
            this.FrameSlicePanel.Controls.Add(this.MaxProjection);
            this.FrameSlicePanel.Controls.Add(this.cb_projectionYes);
            this.FrameSlicePanel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FrameSlicePanel.Location = new System.Drawing.Point(185, 102);
            this.FrameSlicePanel.Name = "FrameSlicePanel";
            this.FrameSlicePanel.Size = new System.Drawing.Size(243, 112);
            this.FrameSlicePanel.TabIndex = 457;
            this.FrameSlicePanel.TabStop = false;
            this.FrameSlicePanel.Text = "Frames / Slices";
            // 
            // umPerSliceLabel
            // 
            this.umPerSliceLabel.AutoSize = true;
            this.umPerSliceLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.umPerSliceLabel.Location = new System.Drawing.Point(175, 48);
            this.umPerSliceLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.umPerSliceLabel.Name = "umPerSliceLabel";
            this.umPerSliceLabel.Size = new System.Drawing.Size(0, 14);
            this.umPerSliceLabel.TabIndex = 483;
            // 
            // stopOpening
            // 
            this.stopOpening.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stopOpening.ForeColor = System.Drawing.Color.Red;
            this.stopOpening.Location = new System.Drawing.Point(83, 15);
            this.stopOpening.Name = "stopOpening";
            this.stopOpening.Size = new System.Drawing.Size(123, 27);
            this.stopOpening.TabIndex = 468;
            this.stopOpening.Text = "Stop Opening";
            this.stopOpening.UseVisualStyleBackColor = true;
            this.stopOpening.Visible = false;
            this.stopOpening.Click += new System.EventHandler(this.stopOpening_Click);
            // 
            // EntireStack_Check
            // 
            this.EntireStack_Check.AutoSize = true;
            this.EntireStack_Check.Checked = true;
            this.EntireStack_Check.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EntireStack_Check.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EntireStack_Check.Location = new System.Drawing.Point(10, 76);
            this.EntireStack_Check.Name = "EntireStack_Check";
            this.EntireStack_Check.Size = new System.Drawing.Size(71, 18);
            this.EntireStack_Check.TabIndex = 475;
            this.EntireStack_Check.Text = "All pages";
            this.EntireStack_Check.UseVisualStyleBackColor = true;
            this.EntireStack_Check.Click += new System.EventHandler(this.EntireStack_Check_Click);
            // 
            // st_pageN
            // 
            this.st_pageN.AutoSize = true;
            this.st_pageN.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_pageN.Location = new System.Drawing.Point(142, 48);
            this.st_pageN.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_pageN.Name = "st_pageN";
            this.st_pageN.Size = new System.Drawing.Size(25, 14);
            this.st_pageN.TabIndex = 434;
            this.st_pageN.Text = "/ 10";
            // 
            // c_page
            // 
            this.c_page.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_page.Location = new System.Drawing.Point(89, 45);
            this.c_page.Margin = new System.Windows.Forms.Padding(1);
            this.c_page.Name = "c_page";
            this.c_page.Size = new System.Drawing.Size(51, 20);
            this.c_page.TabIndex = 474;
            this.c_page.Text = "1";
            this.c_page.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.c_page.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UpdateFigure_KeyDown);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(148, 70);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(18, 14);
            this.label9.TabIndex = 473;
            this.label9.Text = "To";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(87, 70);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 14);
            this.label6.TabIndex = 472;
            this.label6.Text = "From";
            // 
            // PageEnd
            // 
            this.PageEnd.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PageEnd.Location = new System.Drawing.Point(149, 85);
            this.PageEnd.Margin = new System.Windows.Forms.Padding(1);
            this.PageEnd.Name = "PageEnd";
            this.PageEnd.Size = new System.Drawing.Size(48, 20);
            this.PageEnd.TabIndex = 471;
            this.PageEnd.Text = "1";
            this.PageEnd.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.PageEnd.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ZProcRange_KeyDown);
            // 
            // PageStart
            // 
            this.PageStart.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PageStart.Location = new System.Drawing.Point(90, 85);
            this.PageStart.Margin = new System.Windows.Forms.Padding(1);
            this.PageStart.Name = "PageStart";
            this.PageStart.Size = new System.Drawing.Size(51, 20);
            this.PageStart.TabIndex = 470;
            this.PageStart.Text = "1";
            this.PageStart.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.PageStart.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ZProcRange_KeyDown);
            // 
            // PageDownDown
            // 
            this.PageDownDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.PageDownDown.Location = new System.Drawing.Point(93, 15);
            this.PageDownDown.Name = "PageDownDown";
            this.PageDownDown.Size = new System.Drawing.Size(32, 23);
            this.PageDownDown.TabIndex = 467;
            this.PageDownDown.Text = "<<";
            this.PageDownDown.UseVisualStyleBackColor = true;
            this.PageDownDown.Click += new System.EventHandler(this.Page_UpDownClick);
            // 
            // PageUpUp
            // 
            this.PageUpUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.PageUpUp.Location = new System.Drawing.Point(175, 15);
            this.PageUpUp.Name = "PageUpUp";
            this.PageUpUp.Size = new System.Drawing.Size(32, 23);
            this.PageUpUp.TabIndex = 466;
            this.PageUpUp.Text = ">>";
            this.PageUpUp.UseVisualStyleBackColor = true;
            this.PageUpUp.Click += new System.EventHandler(this.Page_UpDownClick);
            // 
            // PageUp
            // 
            this.PageUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.PageUp.Location = new System.Drawing.Point(149, 15);
            this.PageUp.Name = "PageUp";
            this.PageUp.Size = new System.Drawing.Size(25, 23);
            this.PageUp.TabIndex = 464;
            this.PageUp.Text = ">";
            this.PageUp.UseVisualStyleBackColor = true;
            this.PageUp.Click += new System.EventHandler(this.Page_UpDownClick);
            // 
            // PageDown
            // 
            this.PageDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.PageDown.Location = new System.Drawing.Point(125, 15);
            this.PageDown.Name = "PageDown";
            this.PageDown.Size = new System.Drawing.Size(25, 23);
            this.PageDown.TabIndex = 463;
            this.PageDown.Text = "<";
            this.PageDown.UseVisualStyleBackColor = true;
            this.PageDown.Click += new System.EventHandler(this.Page_UpDownClick);
            // 
            // AveProjection
            // 
            this.AveProjection.AutoSize = true;
            this.AveProjection.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AveProjection.Location = new System.Drawing.Point(10, 56);
            this.AveProjection.Name = "AveProjection";
            this.AveProjection.Size = new System.Drawing.Size(46, 18);
            this.AveProjection.TabIndex = 2;
            this.AveProjection.Text = "Sum";
            this.AveProjection.UseVisualStyleBackColor = true;
            this.AveProjection.Click += new System.EventHandler(this.UpdateImagesRecalc_Click);
            // 
            // MaxProjection
            // 
            this.MaxProjection.AutoSize = true;
            this.MaxProjection.Checked = true;
            this.MaxProjection.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaxProjection.Location = new System.Drawing.Point(10, 39);
            this.MaxProjection.Name = "MaxProjection";
            this.MaxProjection.Size = new System.Drawing.Size(45, 18);
            this.MaxProjection.TabIndex = 1;
            this.MaxProjection.TabStop = true;
            this.MaxProjection.Text = "Max";
            this.MaxProjection.UseVisualStyleBackColor = true;
            this.MaxProjection.Click += new System.EventHandler(this.UpdateImagesRecalc_Click);
            // 
            // cb_projectionYes
            // 
            this.cb_projectionYes.AutoSize = true;
            this.cb_projectionYes.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_projectionYes.Location = new System.Drawing.Point(9, 20);
            this.cb_projectionYes.Name = "cb_projectionYes";
            this.cb_projectionYes.Size = new System.Drawing.Size(59, 18);
            this.cb_projectionYes.TabIndex = 0;
            this.cb_projectionYes.Text = "Project";
            this.cb_projectionYes.UseVisualStyleBackColor = true;
            this.cb_projectionYes.Click += new System.EventHandler(this.UpdateImagesRecalc_Click);
            // 
            // st_Filter
            // 
            this.st_Filter.AutoSize = true;
            this.st_Filter.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_Filter.Location = new System.Drawing.Point(151, 14);
            this.st_Filter.Name = "st_Filter";
            this.st_Filter.Size = new System.Drawing.Size(30, 14);
            this.st_Filter.TabIndex = 458;
            this.st_Filter.Text = "Filter";
            // 
            // filterWindow
            // 
            this.filterWindow.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.filterWindow.Location = new System.Drawing.Point(182, 11);
            this.filterWindow.Margin = new System.Windows.Forms.Padding(1);
            this.filterWindow.Name = "filterWindow";
            this.filterWindow.Size = new System.Drawing.Size(41, 20);
            this.filterWindow.TabIndex = 459;
            this.filterWindow.Text = "0";
            this.filterWindow.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.filterWindow.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UpdateFigure_KeyDown);
            // 
            // rightClickMenu_removeAll
            // 
            this.rightClickMenu_removeAll.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.rightClickMenu_removeAll.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.rightClickMenu_removeAll.Name = "rightClickMenuStrip2";
            this.rightClickMenu_removeAll.Size = new System.Drawing.Size(158, 26);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(157, 22);
            this.toolStripMenuItem1.Text = "Remove all Rois";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.RemoveAllROIs);
            // 
            // Main_Menu
            // 
            this.Main_Menu.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Main_Menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.rOIsToolStripMenuItem,
            this.uncagingToolStripMenuItem,
            this.analysisToolStripMenuItem,
            this.windowsToolStripMenuItem,
            this.setttingToolStripMenuItem,
            this.pythonScriptToolStripMenuItem});
            this.Main_Menu.Location = new System.Drawing.Point(0, 0);
            this.Main_Menu.Name = "Main_Menu";
            this.Main_Menu.Padding = new System.Windows.Forms.Padding(6, 1, 0, 1);
            this.Main_Menu.Size = new System.Drawing.Size(1054, 24);
            this.Main_Menu.TabIndex = 461;
            this.Main_Menu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFLIMImageToolStripMenuItem,
            this.saveFLIMImageToolStripMenuItem1,
            this.saveFLIMImageToolStripMenuItem,
            this.BatchProcessingToolStripMenuItem,
            this.showImageDescriptionToolStripMenuItem,
            this.toolStripSeparator2,
            this.openFLIMImageInNewWindowToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openFLIMImageToolStripMenuItem
            // 
            this.openFLIMImageToolStripMenuItem.Name = "openFLIMImageToolStripMenuItem";
            this.openFLIMImageToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.openFLIMImageToolStripMenuItem.Text = "Open FLIM image ...       Ctrl + O";
            this.openFLIMImageToolStripMenuItem.Click += new System.EventHandler(this.OpenFLIMImagesToolStripMenuItem_Click);
            // 
            // saveFLIMImageToolStripMenuItem1
            // 
            this.saveFLIMImageToolStripMenuItem1.Name = "saveFLIMImageToolStripMenuItem1";
            this.saveFLIMImageToolStripMenuItem1.Size = new System.Drawing.Size(251, 22);
            this.saveFLIMImageToolStripMenuItem1.Text = "Save FLIM image";
            this.saveFLIMImageToolStripMenuItem1.Click += new System.EventHandler(this.saveFLIMImageToolStripMenuItem1_Click);
            // 
            // saveFLIMImageToolStripMenuItem
            // 
            this.saveFLIMImageToolStripMenuItem.Name = "saveFLIMImageToolStripMenuItem";
            this.saveFLIMImageToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.saveFLIMImageToolStripMenuItem.Text = "Export formatted image (TIF)...";
            this.saveFLIMImageToolStripMenuItem.Click += new System.EventHandler(this.SaveFLIMImageToolStripMenuItem_Click);
            // 
            // BatchProcessingToolStripMenuItem
            // 
            this.BatchProcessingToolStripMenuItem.Name = "BatchProcessingToolStripMenuItem";
            this.BatchProcessingToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.BatchProcessingToolStripMenuItem.Text = "Batch processing (analysis)";
            this.BatchProcessingToolStripMenuItem.Click += new System.EventHandler(this.BatchProcessingToolStripMenuItem_Click);
            // 
            // showImageDescriptionToolStripMenuItem
            // 
            this.showImageDescriptionToolStripMenuItem.Name = "showImageDescriptionToolStripMenuItem";
            this.showImageDescriptionToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.showImageDescriptionToolStripMenuItem.Text = "Show image description";
            this.showImageDescriptionToolStripMenuItem.Click += new System.EventHandler(this.showImageDescriptionToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(248, 6);
            // 
            // openFLIMImageInNewWindowToolStripMenuItem
            // 
            this.openFLIMImageInNewWindowToolStripMenuItem.Name = "openFLIMImageInNewWindowToolStripMenuItem";
            this.openFLIMImageInNewWindowToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.openFLIMImageInNewWindowToolStripMenuItem.Text = "Open FLIM image in new window";
            this.openFLIMImageInNewWindowToolStripMenuItem.Click += new System.EventHandler(this.openFLIMImageInNewWindowToolStripMenuItem_Click);
            // 
            // rOIsToolStripMenuItem
            // 
            this.rOIsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveRoisToolStripMenuItem,
            this.recoverRoisToolStripMenuItem,
            this.removeAllRoisToolStripMenuItem,
            this.toolStripSeparator1,
            this.readRoiFromImageJToolStripMenuItem,
            this.saveRoiAsImageJToolStripMenuItem});
            this.rOIsToolStripMenuItem.Name = "rOIsToolStripMenuItem";
            this.rOIsToolStripMenuItem.Size = new System.Drawing.Size(43, 22);
            this.rOIsToolStripMenuItem.Text = "ROIs";
            // 
            // saveRoisToolStripMenuItem
            // 
            this.saveRoisToolStripMenuItem.Name = "saveRoisToolStripMenuItem";
            this.saveRoisToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.saveRoisToolStripMenuItem.Text = "Save Rois";
            this.saveRoisToolStripMenuItem.Click += new System.EventHandler(this.saveRoisToolStripMenuItem_Click);
            // 
            // recoverRoisToolStripMenuItem
            // 
            this.recoverRoisToolStripMenuItem.Name = "recoverRoisToolStripMenuItem";
            this.recoverRoisToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.recoverRoisToolStripMenuItem.Text = "Recover Rois";
            this.recoverRoisToolStripMenuItem.Click += new System.EventHandler(this.RecoverRoisToolStripMenuItem_Click);
            // 
            // removeAllRoisToolStripMenuItem
            // 
            this.removeAllRoisToolStripMenuItem.Name = "removeAllRoisToolStripMenuItem";
            this.removeAllRoisToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.removeAllRoisToolStripMenuItem.Text = "Remove All Rois";
            this.removeAllRoisToolStripMenuItem.Click += new System.EventHandler(this.RemoveAllRoisToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(186, 6);
            // 
            // readRoiFromImageJToolStripMenuItem
            // 
            this.readRoiFromImageJToolStripMenuItem.Name = "readRoiFromImageJToolStripMenuItem";
            this.readRoiFromImageJToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.readRoiFromImageJToolStripMenuItem.Text = "Read Roi from ImageJ";
            this.readRoiFromImageJToolStripMenuItem.Click += new System.EventHandler(this.readRoiFromImageJToolStripMenuItem_Click);
            // 
            // saveRoiAsImageJToolStripMenuItem
            // 
            this.saveRoiAsImageJToolStripMenuItem.Name = "saveRoiAsImageJToolStripMenuItem";
            this.saveRoiAsImageJToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.saveRoiAsImageJToolStripMenuItem.Text = "Save Roi as ImageJ";
            this.saveRoiAsImageJToolStripMenuItem.Click += new System.EventHandler(this.saveRoiAsImageJToolStripMenuItem_Click);
            // 
            // uncagingToolStripMenuItem
            // 
            this.uncagingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setUncagingPositionToolStripMenuItem});
            this.uncagingToolStripMenuItem.Name = "uncagingToolStripMenuItem";
            this.uncagingToolStripMenuItem.Size = new System.Drawing.Size(70, 22);
            this.uncagingToolStripMenuItem.Text = "Uncaging";
            // 
            // setUncagingPositionToolStripMenuItem
            // 
            this.setUncagingPositionToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.setUncagingPositionToolStripMenuItem.Name = "setUncagingPositionToolStripMenuItem";
            this.setUncagingPositionToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.setUncagingPositionToolStripMenuItem.Text = "Show uncaging position";
            this.setUncagingPositionToolStripMenuItem.Click += new System.EventHandler(this.SetUncagingPositionToolStripMenuItem_Click);
            // 
            // analysisToolStripMenuItem
            // 
            this.analysisToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.alignSlicesframesToolStripMenuItem,
            this.makeSinlgeFileMovieFromFilesToolStripMenuItem,
            this.getFocusFrameToolStripMenuItem,
            this.toolStripSeparator4,
            this.fastZCalibrationToolStripMenuItem,
            this.toolStripSeparator3,
            this.deleteCurrentPageToolStripMenuItem});
            this.analysisToolStripMenuItem.Name = "analysisToolStripMenuItem";
            this.analysisToolStripMenuItem.Size = new System.Drawing.Size(62, 22);
            this.analysisToolStripMenuItem.Text = "Analysis";
            // 
            // alignSlicesframesToolStripMenuItem
            // 
            this.alignSlicesframesToolStripMenuItem.Name = "alignSlicesframesToolStripMenuItem";
            this.alignSlicesframesToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.alignSlicesframesToolStripMenuItem.Text = "Align pages";
            this.alignSlicesframesToolStripMenuItem.Click += new System.EventHandler(this.AlignSlicesframesToolStripMenuItem_Click);
            // 
            // makeSinlgeFileMovieFromFilesToolStripMenuItem
            // 
            this.makeSinlgeFileMovieFromFilesToolStripMenuItem.Name = "makeSinlgeFileMovieFromFilesToolStripMenuItem";
            this.makeSinlgeFileMovieFromFilesToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.makeSinlgeFileMovieFromFilesToolStripMenuItem.Text = "Make Movie From Series";
            this.makeSinlgeFileMovieFromFilesToolStripMenuItem.Click += new System.EventHandler(this.makeMoviesWithTheSameBaseNameToolStripMenuItem_Click);
            // 
            // getFocusFrameToolStripMenuItem
            // 
            this.getFocusFrameToolStripMenuItem.Name = "getFocusFrameToolStripMenuItem";
            this.getFocusFrameToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.getFocusFrameToolStripMenuItem.Text = "GetFocusFrame";
            this.getFocusFrameToolStripMenuItem.Click += new System.EventHandler(this.getFocusFrameToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(200, 6);
            // 
            // fastZCalibrationToolStripMenuItem
            // 
            this.fastZCalibrationToolStripMenuItem.Name = "fastZCalibrationToolStripMenuItem";
            this.fastZCalibrationToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.fastZCalibrationToolStripMenuItem.Text = "Fast Z Calibration";
            this.fastZCalibrationToolStripMenuItem.Click += new System.EventHandler(this.fastZCalibrationToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(200, 6);
            // 
            // deleteCurrentPageToolStripMenuItem
            // 
            this.deleteCurrentPageToolStripMenuItem.Name = "deleteCurrentPageToolStripMenuItem";
            this.deleteCurrentPageToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.deleteCurrentPageToolStripMenuItem.Text = "Delete current page";
            this.deleteCurrentPageToolStripMenuItem.Click += new System.EventHandler(this.deleteCurrentPageToolStripMenuItem_Click);
            // 
            // windowsToolStripMenuItem
            // 
            this.windowsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.timeCoursePlotToolStripMenuItem,
            this.driftMeasurementToolStripMenuItem});
            this.windowsToolStripMenuItem.Name = "windowsToolStripMenuItem";
            this.windowsToolStripMenuItem.Size = new System.Drawing.Size(68, 22);
            this.windowsToolStripMenuItem.Text = "Windows";
            // 
            // timeCoursePlotToolStripMenuItem
            // 
            this.timeCoursePlotToolStripMenuItem.Name = "timeCoursePlotToolStripMenuItem";
            this.timeCoursePlotToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.timeCoursePlotToolStripMenuItem.Text = "Time course plot";
            this.timeCoursePlotToolStripMenuItem.Click += new System.EventHandler(this.TimeCoursePlotToolStripMenuItem_Click);
            // 
            // driftMeasurementToolStripMenuItem
            // 
            this.driftMeasurementToolStripMenuItem.Name = "driftMeasurementToolStripMenuItem";
            this.driftMeasurementToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.driftMeasurementToolStripMenuItem.Text = "Drift measurement";
            this.driftMeasurementToolStripMenuItem.Click += new System.EventHandler(this.driftMeasurementToolStripMenuItem_Click);
            // 
            // setttingToolStripMenuItem
            // 
            this.setttingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.keepPagesInMemoryToolStripMenuItem,
            this.intelMKLLibraryOnToolStripMenuItem});
            this.setttingToolStripMenuItem.Name = "setttingToolStripMenuItem";
            this.setttingToolStripMenuItem.Size = new System.Drawing.Size(60, 22);
            this.setttingToolStripMenuItem.Text = "Settting";
            // 
            // keepPagesInMemoryToolStripMenuItem
            // 
            this.keepPagesInMemoryToolStripMenuItem.Checked = true;
            this.keepPagesInMemoryToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.keepPagesInMemoryToolStripMenuItem.Name = "keepPagesInMemoryToolStripMenuItem";
            this.keepPagesInMemoryToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.keepPagesInMemoryToolStripMenuItem.Text = "Keep images in memory";
            this.keepPagesInMemoryToolStripMenuItem.Click += new System.EventHandler(this.keepPagesInMemoryToolStripMenuItem_Click);
            // 
            // intelMKLLibraryOnToolStripMenuItem
            // 
            this.intelMKLLibraryOnToolStripMenuItem.Name = "intelMKLLibraryOnToolStripMenuItem";
            this.intelMKLLibraryOnToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.intelMKLLibraryOnToolStripMenuItem.Text = "Use Intel MKL library";
            this.intelMKLLibraryOnToolStripMenuItem.Click += new System.EventHandler(this.intelMKLLibraryOnToolStripMenuItem_Click);
            // 
            // FilePanel
            // 
            this.FilePanel.Controls.Add(this.FileUp);
            this.FilePanel.Controls.Add(this.FileDown);
            this.FilePanel.Controls.Add(this.FileN);
            this.FilePanel.Controls.Add(this.BaseName);
            this.FilePanel.Controls.Add(this.st_fileN);
            this.FilePanel.Controls.Add(this.st_BaseName);
            this.FilePanel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FilePanel.Location = new System.Drawing.Point(185, 3);
            this.FilePanel.Name = "FilePanel";
            this.FilePanel.Size = new System.Drawing.Size(243, 57);
            this.FilePanel.TabIndex = 462;
            this.FilePanel.TabStop = false;
            this.FilePanel.Text = "File";
            // 
            // FileUp
            // 
            this.FileUp.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileUp.Location = new System.Drawing.Point(190, 25);
            this.FileUp.Name = "FileUp";
            this.FileUp.Size = new System.Drawing.Size(25, 23);
            this.FileUp.TabIndex = 469;
            this.FileUp.Text = ">";
            this.FileUp.UseVisualStyleBackColor = true;
            this.FileUp.Click += new System.EventHandler(this.FileUpDown_Click);
            // 
            // FileDown
            // 
            this.FileDown.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileDown.Location = new System.Drawing.Point(166, 25);
            this.FileDown.Name = "FileDown";
            this.FileDown.Size = new System.Drawing.Size(25, 23);
            this.FileDown.TabIndex = 468;
            this.FileDown.Text = "<";
            this.FileDown.UseVisualStyleBackColor = true;
            this.FileDown.Click += new System.EventHandler(this.FileUpDown_Click);
            // 
            // FileN
            // 
            this.FileN.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileN.Location = new System.Drawing.Point(111, 28);
            this.FileN.Margin = new System.Windows.Forms.Padding(1);
            this.FileN.Name = "FileN";
            this.FileN.Size = new System.Drawing.Size(51, 20);
            this.FileN.TabIndex = 16;
            this.FileN.Text = "1";
            this.FileN.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.FileN.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FileN_KeyDown);
            // 
            // BaseName
            // 
            this.BaseName.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BaseName.Location = new System.Drawing.Point(6, 28);
            this.BaseName.Margin = new System.Windows.Forms.Padding(1);
            this.BaseName.Name = "BaseName";
            this.BaseName.ReadOnly = true;
            this.BaseName.Size = new System.Drawing.Size(88, 20);
            this.BaseName.TabIndex = 5;
            this.BaseName.Text = "Test";
            // 
            // st_fileN
            // 
            this.st_fileN.AutoSize = true;
            this.st_fileN.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_fileN.Location = new System.Drawing.Point(107, 14);
            this.st_fileN.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_fileN.Name = "st_fileN";
            this.st_fileN.Size = new System.Drawing.Size(29, 14);
            this.st_fileN.TabIndex = 17;
            this.st_fileN.Text = "File#";
            // 
            // st_BaseName
            // 
            this.st_BaseName.AutoSize = true;
            this.st_BaseName.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_BaseName.Location = new System.Drawing.Point(5, 13);
            this.st_BaseName.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.st_BaseName.Name = "st_BaseName";
            this.st_BaseName.Size = new System.Drawing.Size(62, 14);
            this.st_BaseName.TabIndex = 10;
            this.st_BaseName.Text = "Base Name";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.LifetimeCh_panel);
            this.groupBox3.Controls.Add(this.Ch12);
            this.groupBox3.Controls.Add(this.Channel2);
            this.groupBox3.Controls.Add(this.Channel1);
            this.groupBox3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(180, 147);
            this.groupBox3.TabIndex = 463;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Display";
            // 
            // LifetimeCh_panel
            // 
            this.LifetimeCh_panel.Controls.Add(this.label8);
            this.LifetimeCh_panel.Controls.Add(this.Ch1);
            this.LifetimeCh_panel.Controls.Add(this.Ch2);
            this.LifetimeCh_panel.Enabled = false;
            this.LifetimeCh_panel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LifetimeCh_panel.Location = new System.Drawing.Point(25, 105);
            this.LifetimeCh_panel.Name = "LifetimeCh_panel";
            this.LifetimeCh_panel.Size = new System.Drawing.Size(136, 38);
            this.LifetimeCh_panel.TabIndex = 481;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(5, 4);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(63, 14);
            this.label8.TabIndex = 478;
            this.label8.Text = "Lifetime Ch:";
            // 
            // Ch1
            // 
            this.Ch1.AutoSize = true;
            this.Ch1.Checked = true;
            this.Ch1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Ch1.Location = new System.Drawing.Point(83, 2);
            this.Ch1.Name = "Ch1";
            this.Ch1.Size = new System.Drawing.Size(44, 18);
            this.Ch1.TabIndex = 479;
            this.Ch1.TabStop = true;
            this.Ch1.Text = "Ch1";
            this.Ch1.UseVisualStyleBackColor = true;
            this.Ch1.Click += new System.EventHandler(this.Ch1_Click);
            // 
            // Ch2
            // 
            this.Ch2.AutoSize = true;
            this.Ch2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Ch2.Location = new System.Drawing.Point(83, 18);
            this.Ch2.Name = "Ch2";
            this.Ch2.Size = new System.Drawing.Size(44, 18);
            this.Ch2.TabIndex = 480;
            this.Ch2.Text = "Ch2";
            this.Ch2.UseVisualStyleBackColor = true;
            this.Ch2.Click += new System.EventHandler(this.Ch1_Click);
            // 
            // Ch12
            // 
            this.Ch12.AutoSize = true;
            this.Ch12.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Ch12.Location = new System.Drawing.Point(11, 78);
            this.Ch12.Name = "Ch12";
            this.Ch12.Size = new System.Drawing.Size(143, 20);
            this.Ch12.TabIndex = 302;
            this.Ch12.Text = "Ch1/Ch2 (Intensity)";
            this.Ch12.UseVisualStyleBackColor = true;
            this.Ch12.Click += new System.EventHandler(this.Channel1_CheckedChanged);
            // 
            // st_im1
            // 
            this.st_im1.AutoSize = true;
            this.st_im1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_im1.Location = new System.Drawing.Point(0, 35);
            this.st_im1.Name = "st_im1";
            this.st_im1.Size = new System.Drawing.Size(64, 14);
            this.st_im1.TabIndex = 464;
            this.st_im1.Text = "Intensity 1";
            // 
            // st_im2
            // 
            this.st_im2.AutoSize = true;
            this.st_im2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_im2.Location = new System.Drawing.Point(385, 35);
            this.st_im2.Name = "st_im2";
            this.st_im2.Size = new System.Drawing.Size(42, 14);
            this.st_im2.TabIndex = 465;
            this.st_im2.Text = "FLIM 1";
            // 
            // LifetimeCurvePlot
            // 
            this.LifetimeCurvePlot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LifetimeCurvePlot.BackColor = System.Drawing.Color.White;
            this.LifetimeCurvePlot.Location = new System.Drawing.Point(770, 50);
            this.LifetimeCurvePlot.Name = "LifetimeCurvePlot";
            this.LifetimeCurvePlot.Size = new System.Drawing.Size(283, 384);
            this.LifetimeCurvePlot.TabIndex = 467;
            this.LifetimeCurvePlot.TabStop = false;
            this.LifetimeCurvePlot.Paint += new System.Windows.Forms.PaintEventHandler(this.LifetimeDecayPlot_Paint);
            // 
            // st_panel
            // 
            this.st_panel.AutoSize = true;
            this.st_panel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_panel.Location = new System.Drawing.Point(1, 4);
            this.st_panel.Name = "st_panel";
            this.st_panel.Size = new System.Drawing.Size(62, 14);
            this.st_panel.TabIndex = 470;
            this.st_panel.Text = "Lifetime 1";
            // 
            // st_2Ch
            // 
            this.st_2Ch.AutoSize = true;
            this.st_2Ch.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_2Ch.Location = new System.Drawing.Point(90, 62);
            this.st_2Ch.Name = "st_2Ch";
            this.st_2Ch.Size = new System.Drawing.Size(33, 14);
            this.st_2Ch.TabIndex = 472;
            this.st_2Ch.Text = "FLIM";
            // 
            // st_1stCh
            // 
            this.st_1stCh.AutoSize = true;
            this.st_1stCh.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_1stCh.Location = new System.Drawing.Point(13, 62);
            this.st_1stCh.Name = "st_1stCh";
            this.st_1stCh.Size = new System.Drawing.Size(55, 14);
            this.st_1stCh.TabIndex = 473;
            this.st_1stCh.Text = "Intensity";
            // 
            // st_panel2
            // 
            this.st_panel2.AutoSize = true;
            this.st_panel2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_panel2.Location = new System.Drawing.Point(68, 4);
            this.st_panel2.Name = "st_panel2";
            this.st_panel2.Size = new System.Drawing.Size(137, 14);
            this.st_panel2.TabIndex = 474;
            this.st_panel2.Text = "(Pixels above FLIM min)";
            // 
            // Image2
            // 
            this.Image2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Image2.BackColor = System.Drawing.Color.Black;
            this.Image2.Cursor = System.Windows.Forms.Cursors.Default;
            this.Image2.Location = new System.Drawing.Point(385, 50);
            this.Image2.Margin = new System.Windows.Forms.Padding(0);
            this.Image2.Name = "Image2";
            this.Image2.Size = new System.Drawing.Size(384, 384);
            this.Image2.TabIndex = 79;
            this.Image2.TabStop = false;
            this.Image2.Paint += new System.Windows.Forms.PaintEventHandler(this.Image_Paint);
            this.Image2.DoubleClick += new System.EventHandler(this.Image1_DoubleClick);
            this.Image2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Image1_MouseDown);
            this.Image2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Image1_MouseMove);
            this.Image2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Image1_MouseUp);
            // 
            // Image1
            // 
            this.Image1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Image1.BackColor = System.Drawing.Color.Black;
            this.Image1.Cursor = System.Windows.Forms.Cursors.Default;
            this.Image1.Location = new System.Drawing.Point(0, 50);
            this.Image1.Name = "Image1";
            this.Image1.Size = new System.Drawing.Size(384, 384);
            this.Image1.TabIndex = 80;
            this.Image1.TabStop = false;
            this.Image1.Paint += new System.Windows.Forms.PaintEventHandler(this.Image_Paint);
            this.Image1.DoubleClick += new System.EventHandler(this.Image1_DoubleClick);
            this.Image1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Image1_MouseDown);
            this.Image1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Image1_MouseMove);
            this.Image1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Image1_MouseUp);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.FrameAdjustment);
            this.groupBox1.Controls.Add(this.st_lifetime_ns);
            this.groupBox1.Controls.Add(this.colorBar);
            this.groupBox1.Controls.Add(this.st_1stCh);
            this.groupBox1.Controls.Add(this.st_Filter);
            this.groupBox1.Controls.Add(this.filterWindow);
            this.groupBox1.Controls.Add(this.st_2Ch);
            this.groupBox1.Controls.Add(this.MinSldr3);
            this.groupBox1.Controls.Add(this.MinIntensity3);
            this.groupBox1.Controls.Add(this.MaxSldr3);
            this.groupBox1.Controls.Add(this.MaxIntensity3);
            this.groupBox1.Controls.Add(this.MinSldr2);
            this.groupBox1.Controls.Add(this.MaxSldr2);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.MinIntensity2);
            this.groupBox1.Controls.Add(this.MaxIntensity2);
            this.groupBox1.Controls.Add(this.Auto1);
            this.groupBox1.Controls.Add(this.MinSldr1);
            this.groupBox1.Controls.Add(this.MaxSldr1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.MinIntensity1);
            this.groupBox1.Controls.Add(this.MaxIntensity1);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(432, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(233, 210);
            this.groupBox1.TabIndex = 476;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Image control";
            // 
            // FrameAdjustment
            // 
            this.FrameAdjustment.AutoSize = true;
            this.FrameAdjustment.Checked = true;
            this.FrameAdjustment.CheckState = System.Windows.Forms.CheckState.Checked;
            this.FrameAdjustment.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FrameAdjustment.Location = new System.Drawing.Point(14, 35);
            this.FrameAdjustment.Name = "FrameAdjustment";
            this.FrameAdjustment.Size = new System.Drawing.Size(169, 18);
            this.FrameAdjustment.TabIndex = 476;
            this.FrameAdjustment.Text = "#Adjust for number of frames";
            this.FrameAdjustment.UseVisualStyleBackColor = true;
            this.FrameAdjustment.Click += new System.EventHandler(this.Auto1_CheckedChanged);
            // 
            // st_lifetime_ns
            // 
            this.st_lifetime_ns.AutoSize = true;
            this.st_lifetime_ns.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_lifetime_ns.Location = new System.Drawing.Point(157, 62);
            this.st_lifetime_ns.Name = "st_lifetime_ns";
            this.st_lifetime_ns.Size = new System.Drawing.Size(78, 14);
            this.st_lifetime_ns.TabIndex = 475;
            this.st_lifetime_ns.Text = "Lifetime (ns)";
            // 
            // colorBar
            // 
            this.colorBar.Location = new System.Drawing.Point(165, 77);
            this.colorBar.Name = "colorBar";
            this.colorBar.Size = new System.Drawing.Size(64, 20);
            this.colorBar.TabIndex = 474;
            this.colorBar.TabStop = false;
            // 
            // Square_Box
            // 
            this.Square_Box.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Square_Box.Location = new System.Drawing.Point(490, 0);
            this.Square_Box.Name = "Square_Box";
            this.Square_Box.Size = new System.Drawing.Size(30, 30);
            this.Square_Box.TabIndex = 478;
            this.Square_Box.TabStop = false;
            this.Square_Box.Click += new System.EventHandler(this.ToolPanelClicked);
            this.Square_Box.Paint += new System.Windows.Forms.PaintEventHandler(this.ToolPanelPaint);
            // 
            // ElipsoidBox
            // 
            this.ElipsoidBox.Location = new System.Drawing.Point(521, 0);
            this.ElipsoidBox.Name = "ElipsoidBox";
            this.ElipsoidBox.Size = new System.Drawing.Size(30, 30);
            this.ElipsoidBox.TabIndex = 479;
            this.ElipsoidBox.TabStop = false;
            this.ElipsoidBox.Click += new System.EventHandler(this.ToolPanelClicked);
            this.ElipsoidBox.Paint += new System.Windows.Forms.PaintEventHandler(this.ToolPanelPaint);
            // 
            // UncagingBox
            // 
            this.UncagingBox.Location = new System.Drawing.Point(638, 0);
            this.UncagingBox.Name = "UncagingBox";
            this.UncagingBox.Size = new System.Drawing.Size(30, 30);
            this.UncagingBox.TabIndex = 481;
            this.UncagingBox.TabStop = false;
            this.UncagingBox.Click += new System.EventHandler(this.ToolPanelClicked);
            this.UncagingBox.Paint += new System.Windows.Forms.PaintEventHandler(this.ToolPanelPaint);
            // 
            // PolygonBox
            // 
            this.PolygonBox.Location = new System.Drawing.Point(553, 0);
            this.PolygonBox.Name = "PolygonBox";
            this.PolygonBox.Size = new System.Drawing.Size(30, 30);
            this.PolygonBox.TabIndex = 480;
            this.PolygonBox.TabStop = false;
            this.PolygonBox.Click += new System.EventHandler(this.ToolPanelClicked);
            this.PolygonBox.Paint += new System.Windows.Forms.PaintEventHandler(this.ToolPanelPaint);
            // 
            // ctrlPanel
            // 
            this.ctrlPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ctrlPanel.Controls.Add(this.FastZPhasePanel);
            this.ctrlPanel.Controls.Add(this.FastZPanel);
            this.ctrlPanel.Controls.Add(this.groupBox3);
            this.ctrlPanel.Controls.Add(this.FilePanel);
            this.ctrlPanel.Controls.Add(this.Fitting_Group);
            this.ctrlPanel.Controls.Add(this.groupBox1);
            this.ctrlPanel.Controls.Add(this.FrameSlicePanel);
            this.ctrlPanel.Location = new System.Drawing.Point(0, 435);
            this.ctrlPanel.Name = "ctrlPanel";
            this.ctrlPanel.Size = new System.Drawing.Size(1053, 220);
            this.ctrlPanel.TabIndex = 482;
            // 
            // FastZPhasePanel
            // 
            this.FastZPhasePanel.Controls.Add(this.label11);
            this.FastZPhasePanel.Controls.Add(this.FastZFreqText);
            this.FastZPhasePanel.Controls.Add(this.FastZAmpText);
            this.FastZPhasePanel.Controls.Add(this.FastZPhaseText);
            this.FastZPhasePanel.Controls.Add(this.PhaseDetectionMode_Status2);
            this.FastZPhasePanel.Controls.Add(this.PhaseDetectionMode_Status);
            this.FastZPhasePanel.Controls.Add(this.label1);
            this.FastZPhasePanel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FastZPhasePanel.Location = new System.Drawing.Point(3, 150);
            this.FastZPhasePanel.Name = "FastZPhasePanel";
            this.FastZPhasePanel.Size = new System.Drawing.Size(180, 63);
            this.FastZPhasePanel.TabIndex = 471;
            this.FastZPhasePanel.TabStop = false;
            this.FastZPhasePanel.Text = "Fast Z parameters";
            this.FastZPhasePanel.Visible = false;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(51, 15);
            this.label11.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(0, 14);
            this.label11.TabIndex = 481;
            // 
            // FastZFreqText
            // 
            this.FastZFreqText.AutoSize = true;
            this.FastZFreqText.Font = new System.Drawing.Font("Arial", 8.25F);
            this.FastZFreqText.Location = new System.Drawing.Point(99, 15);
            this.FastZFreqText.Name = "FastZFreqText";
            this.FastZFreqText.Size = new System.Drawing.Size(45, 14);
            this.FastZFreqText.TabIndex = 482;
            this.FastZFreqText.Text = "188KHz";
            // 
            // FastZAmpText
            // 
            this.FastZAmpText.AutoSize = true;
            this.FastZAmpText.Font = new System.Drawing.Font("Arial", 8.25F);
            this.FastZAmpText.Location = new System.Drawing.Point(59, 15);
            this.FastZAmpText.Name = "FastZAmpText";
            this.FastZAmpText.Size = new System.Drawing.Size(38, 14);
            this.FastZAmpText.TabIndex = 480;
            this.FastZAmpText.Text = "29.8%";
            // 
            // FastZPhaseText
            // 
            this.FastZPhaseText.AutoSize = true;
            this.FastZPhaseText.Font = new System.Drawing.Font("Arial", 8.25F);
            this.FastZPhaseText.Location = new System.Drawing.Point(37, 15);
            this.FastZPhaseText.Name = "FastZPhaseText";
            this.FastZPhaseText.Size = new System.Drawing.Size(19, 14);
            this.FastZPhaseText.TabIndex = 476;
            this.FastZPhaseText.Text = "30";
            // 
            // PhaseDetectionMode_Status2
            // 
            this.PhaseDetectionMode_Status2.AutoSize = true;
            this.PhaseDetectionMode_Status2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PhaseDetectionMode_Status2.Location = new System.Drawing.Point(6, 45);
            this.PhaseDetectionMode_Status2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.PhaseDetectionMode_Status2.Name = "PhaseDetectionMode_Status2";
            this.PhaseDetectionMode_Status2.Size = new System.Drawing.Size(29, 14);
            this.PhaseDetectionMode_Status2.TabIndex = 479;
            this.PhaseDetectionMode_Status2.Text = "Best";
            // 
            // PhaseDetectionMode_Status
            // 
            this.PhaseDetectionMode_Status.AutoSize = true;
            this.PhaseDetectionMode_Status.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PhaseDetectionMode_Status.Location = new System.Drawing.Point(5, 30);
            this.PhaseDetectionMode_Status.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.PhaseDetectionMode_Status.Name = "PhaseDetectionMode_Status";
            this.PhaseDetectionMode_Status.Size = new System.Drawing.Size(120, 14);
            this.PhaseDetectionMode_Status.TabIndex = 478;
            this.PhaseDetectionMode_Status.Text = "Phase matching = 6.2%";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 15);
            this.label1.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 14);
            this.label1.TabIndex = 476;
            this.label1.Text = "Phase";
            // 
            // FastZPanel
            // 
            this.FastZPanel.Controls.Add(this.TotalFastZFrame);
            this.FastZPanel.Controls.Add(this.FastZUp);
            this.FastZPanel.Controls.Add(this.FastZDown);
            this.FastZPanel.Controls.Add(this.CurrentFastZPageTB);
            this.FastZPanel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FastZPanel.Location = new System.Drawing.Point(185, 60);
            this.FastZPanel.Name = "FastZPanel";
            this.FastZPanel.Size = new System.Drawing.Size(243, 41);
            this.FastZPanel.TabIndex = 470;
            this.FastZPanel.TabStop = false;
            this.FastZPanel.Text = "Fast Z pages";
            this.FastZPanel.Visible = false;
            // 
            // TotalFastZFrame
            // 
            this.TotalFastZFrame.AutoSize = true;
            this.TotalFastZFrame.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TotalFastZFrame.Location = new System.Drawing.Point(66, 18);
            this.TotalFastZFrame.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.TotalFastZFrame.Name = "TotalFastZFrame";
            this.TotalFastZFrame.Size = new System.Drawing.Size(25, 14);
            this.TotalFastZFrame.TabIndex = 470;
            this.TotalFastZFrame.Text = "/ 10";
            // 
            // FastZUp
            // 
            this.FastZUp.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FastZUp.Location = new System.Drawing.Point(190, 10);
            this.FastZUp.Name = "FastZUp";
            this.FastZUp.Size = new System.Drawing.Size(25, 23);
            this.FastZUp.TabIndex = 469;
            this.FastZUp.Text = ">";
            this.FastZUp.UseVisualStyleBackColor = true;
            this.FastZUp.Click += new System.EventHandler(this.FastZUpDown);
            // 
            // FastZDown
            // 
            this.FastZDown.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FastZDown.Location = new System.Drawing.Point(166, 10);
            this.FastZDown.Name = "FastZDown";
            this.FastZDown.Size = new System.Drawing.Size(25, 23);
            this.FastZDown.TabIndex = 468;
            this.FastZDown.Text = "<";
            this.FastZDown.UseVisualStyleBackColor = true;
            this.FastZDown.Click += new System.EventHandler(this.FastZUpDown);
            // 
            // CurrentFastZPageTB
            // 
            this.CurrentFastZPageTB.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentFastZPageTB.Location = new System.Drawing.Point(8, 15);
            this.CurrentFastZPageTB.Margin = new System.Windows.Forms.Padding(1);
            this.CurrentFastZPageTB.Name = "CurrentFastZPageTB";
            this.CurrentFastZPageTB.Size = new System.Drawing.Size(51, 20);
            this.CurrentFastZPageTB.TabIndex = 16;
            this.CurrentFastZPageTB.Text = "1";
            this.CurrentFastZPageTB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.CurrentFastZPageTB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CurrentFastZPageTB_KeyDown);
            // 
            // st_im3
            // 
            this.st_im3.Controls.Add(this.st_panel2);
            this.st_im3.Controls.Add(this.st_panel);
            this.st_im3.Controls.Add(this.logScale);
            this.st_im3.Location = new System.Drawing.Point(770, 30);
            this.st_im3.Name = "st_im3";
            this.st_im3.Size = new System.Drawing.Size(283, 20);
            this.st_im3.TabIndex = 483;
            // 
            // rightClick_CreateUncaging
            // 
            this.rightClick_CreateUncaging.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.rightClick_CreateUncaging.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CreateUncagingLoc});
            this.rightClick_CreateUncaging.Name = "rightClickMenuStrip";
            this.rightClick_CreateUncaging.Size = new System.Drawing.Size(212, 26);
            // 
            // CreateUncagingLoc
            // 
            this.CreateUncagingLoc.Name = "CreateUncagingLoc";
            this.CreateUncagingLoc.Size = new System.Drawing.Size(211, 22);
            this.CreateUncagingLoc.Text = "Create Uncaging Location";
            this.CreateUncagingLoc.Click += new System.EventHandler(this.CreateUncagingLoc_Click);
            // 
            // rightClick_removeUncaging
            // 
            this.rightClick_removeUncaging.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.rightClick_removeUncaging.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RemoveAllUncaging,
            this.createRoiToolStripMenuItem1});
            this.rightClick_removeUncaging.Name = "rightClickMenuStrip2";
            this.rightClick_removeUncaging.Size = new System.Drawing.Size(240, 48);
            // 
            // RemoveAllUncaging
            // 
            this.RemoveAllUncaging.Name = "RemoveAllUncaging";
            this.RemoveAllUncaging.Size = new System.Drawing.Size(239, 22);
            this.RemoveAllUncaging.Text = "Remove ALL";
            this.RemoveAllUncaging.Click += new System.EventHandler(this.RemoveAllUncaging_Click);
            // 
            // createRoiToolStripMenuItem1
            // 
            this.createRoiToolStripMenuItem1.Name = "createRoiToolStripMenuItem1";
            this.createRoiToolStripMenuItem1.Size = new System.Drawing.Size(239, 22);
            this.createRoiToolStripMenuItem1.Text = "Create Uncaging Location Here";
            this.createRoiToolStripMenuItem1.Click += new System.EventHandler(this.CreateUncagingLocDirectly_Click);
            // 
            // rightClick_remUncageEach
            // 
            this.rightClick_remUncageEach.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.rightClick_remUncageEach.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeUncagingEach,
            this.removeAllToolStripMenuItem});
            this.rightClick_remUncageEach.Name = "rightClickMenuStrip";
            this.rightClick_remUncageEach.Size = new System.Drawing.Size(140, 48);
            // 
            // removeUncagingEach
            // 
            this.removeUncagingEach.Name = "removeUncagingEach";
            this.removeUncagingEach.Size = new System.Drawing.Size(139, 22);
            this.removeUncagingEach.Text = "Remove this";
            this.removeUncagingEach.Click += new System.EventHandler(this.removeUncagingEach_Click);
            // 
            // removeAllToolStripMenuItem
            // 
            this.removeAllToolStripMenuItem.Name = "removeAllToolStripMenuItem";
            this.removeAllToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.removeAllToolStripMenuItem.Text = "Remove all";
            this.removeAllToolStripMenuItem.Click += new System.EventHandler(this.RemoveAllUncaging_Click);
            // 
            // HoldCurrentImageCheckBox
            // 
            this.HoldCurrentImageCheckBox.AutoSize = true;
            this.HoldCurrentImageCheckBox.Location = new System.Drawing.Point(87, 34);
            this.HoldCurrentImageCheckBox.Name = "HoldCurrentImageCheckBox";
            this.HoldCurrentImageCheckBox.Size = new System.Drawing.Size(148, 17);
            this.HoldCurrentImageCheckBox.TabIndex = 484;
            this.HoldCurrentImageCheckBox.Text = "Hold this image (magenta)";
            this.HoldCurrentImageCheckBox.UseVisualStyleBackColor = true;
            this.HoldCurrentImageCheckBox.Click += new System.EventHandler(this.HoldCurrentImageCheckBox_Click);
            // 
            // MergeCB
            // 
            this.MergeCB.AutoSize = true;
            this.MergeCB.Location = new System.Drawing.Point(263, 34);
            this.MergeCB.Name = "MergeCB";
            this.MergeCB.Size = new System.Drawing.Size(102, 17);
            this.MergeCB.TabIndex = 485;
            this.MergeCB.Text = "Merge channels";
            this.MergeCB.UseVisualStyleBackColor = true;
            this.MergeCB.Click += new System.EventHandler(this.MergeCB_Click);
            // 
            // ThreeDROIPanel
            // 
            this.ThreeDROIPanel.Location = new System.Drawing.Point(586, 0);
            this.ThreeDROIPanel.Name = "ThreeDROIPanel";
            this.ThreeDROIPanel.Size = new System.Drawing.Size(30, 30);
            this.ThreeDROIPanel.TabIndex = 486;
            this.ThreeDROIPanel.TabStop = false;
            this.ThreeDROIPanel.Click += new System.EventHandler(this.ToolPanelClicked);
            this.ThreeDROIPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.ToolPanelPaint);
            // 
            // pythonScriptToolStripMenuItem
            // 
            this.pythonScriptToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.averageTimeCoursePythonToolStripMenuItem,
            this.runPythonScriptToolStripMenuItem,
            this.setPythonPathToolStripMenuItem,
            this.setScriptPathToolStripMenuItem});
            this.pythonScriptToolStripMenuItem.Name = "pythonScriptToolStripMenuItem";
            this.pythonScriptToolStripMenuItem.Size = new System.Drawing.Size(89, 22);
            this.pythonScriptToolStripMenuItem.Text = "Python script";
            // 
            // averageTimeCoursePythonToolStripMenuItem
            // 
            this.averageTimeCoursePythonToolStripMenuItem.Name = "averageTimeCoursePythonToolStripMenuItem";
            this.averageTimeCoursePythonToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.averageTimeCoursePythonToolStripMenuItem.Text = "Average Time Courses";
            this.averageTimeCoursePythonToolStripMenuItem.Click += new System.EventHandler(this.averageTimeCoursePythonToolStripMenuItem_Click);
            // 
            // setPythonPathToolStripMenuItem
            // 
            this.setPythonPathToolStripMenuItem.Name = "setPythonPathToolStripMenuItem";
            this.setPythonPathToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.setPythonPathToolStripMenuItem.Text = "Set Python Path";
            this.setPythonPathToolStripMenuItem.Click += new System.EventHandler(this.setPythonPathToolStripMenuItem_Click);
            // 
            // setScriptPathToolStripMenuItem
            // 
            this.setScriptPathToolStripMenuItem.Name = "setScriptPathToolStripMenuItem";
            this.setScriptPathToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.setScriptPathToolStripMenuItem.Text = "Set Script Path";
            this.setScriptPathToolStripMenuItem.Click += new System.EventHandler(this.setScriptPathToolStripMenuItem_Click);
            // 
            // runPythonScriptToolStripMenuItem
            // 
            this.runPythonScriptToolStripMenuItem.Name = "runPythonScriptToolStripMenuItem";
            this.runPythonScriptToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.runPythonScriptToolStripMenuItem.Text = "Run Python Script";
            this.runPythonScriptToolStripMenuItem.Click += new System.EventHandler(this.runPythonScriptToolStripMenuItem_Click);
            // 
            // Image_Display
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1054, 661);
            this.Controls.Add(this.ThreeDROIPanel);
            this.Controls.Add(this.Image1);
            this.Controls.Add(this.MergeCB);
            this.Controls.Add(this.HoldCurrentImageCheckBox);
            this.Controls.Add(this.st_im3);
            this.Controls.Add(this.ctrlPanel);
            this.Controls.Add(this.UncagingBox);
            this.Controls.Add(this.PolygonBox);
            this.Controls.Add(this.ElipsoidBox);
            this.Controls.Add(this.Square_Box);
            this.Controls.Add(this.Image2);
            this.Controls.Add(this.LifetimeCurvePlot);
            this.Controls.Add(this.st_im2);
            this.Controls.Add(this.st_im1);
            this.Controls.Add(this.Main_Menu);
            this.Controls.Add(this.ShowFLIM);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "Image_Display";
            this.Text = "FLIM Analysis";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Image_Display_FormClosing);
            this.Load += new System.EventHandler(this.Image_Display_Load);
            this.Shown += new System.EventHandler(this.Image_Display_Shown);
            this.ResizeEnd += new System.EventHandler(this.Image_Display_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Image_Display_KeyDown);
            this.Resize += new System.EventHandler(this.Image_Display_Resize);
            this.rightClickMenuStrip_inROI.ResumeLayout(false);
            this.rightClickMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MinSldr1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxSldr1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinSldr2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxSldr2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinSldr3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxSldr3)).EndInit();
            this.Fitting_Group.ResumeLayout(false);
            this.Fitting_Group.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.FrameSlicePanel.ResumeLayout(false);
            this.FrameSlicePanel.PerformLayout();
            this.rightClickMenu_removeAll.ResumeLayout(false);
            this.Main_Menu.ResumeLayout(false);
            this.Main_Menu.PerformLayout();
            this.FilePanel.ResumeLayout(false);
            this.FilePanel.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.LifetimeCh_panel.ResumeLayout(false);
            this.LifetimeCh_panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LifetimeCurvePlot)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Image2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Image1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.colorBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Square_Box)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ElipsoidBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UncagingBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PolygonBox)).EndInit();
            this.ctrlPanel.ResumeLayout(false);
            this.FastZPhasePanel.ResumeLayout(false);
            this.FastZPhasePanel.PerformLayout();
            this.FastZPanel.ResumeLayout(false);
            this.FastZPanel.PerformLayout();
            this.st_im3.ResumeLayout(false);
            this.st_im3.PerformLayout();
            this.rightClick_CreateUncaging.ResumeLayout(false);
            this.rightClick_removeUncaging.ResumeLayout(false);
            this.rightClick_remUncageEach.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ThreeDROIPanel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox logScale;
        private System.Windows.Forms.CheckBox ShowFLIM;
        private System.Windows.Forms.RadioButton Channel1;
        private System.Windows.Forms.RadioButton Channel2;
        private System.Windows.Forms.Label imgOffset1;
        private System.Windows.Forms.Button Apply_Offset;
        private System.Windows.Forms.CheckBox fix_all;
        private System.Windows.Forms.Label label75;
        private System.Windows.Forms.Label xi_square;
        private System.Windows.Forms.Label label73;
        private System.Windows.Forms.Label label74;
        private System.Windows.Forms.TextBox fit_end;
        private System.Windows.Forms.Label label67;
        private System.Windows.Forms.TextBox t0_Img;
        private System.Windows.Forms.TextBox fit_start;
        private System.Windows.Forms.CheckBox cb_T0Fix;
        private System.Windows.Forms.CheckBox cb_tauGFix;
        private System.Windows.Forms.CheckBox cb_tau2Fix;
        private System.Windows.Forms.CheckBox cb_tau1Fix;
        private System.Windows.Forms.Label frac2;
        private System.Windows.Forms.Label frac1;
        private System.Windows.Forms.Label label69;
        private System.Windows.Forms.Label tau_m;
        private System.Windows.Forms.Label label66;
        private System.Windows.Forms.TextBox t0;
        private System.Windows.Forms.Label label65;
        private System.Windows.Forms.TextBox tauG;
        private System.Windows.Forms.Label label63;
        private System.Windows.Forms.Label label64;
        private System.Windows.Forms.Label label62;
        private System.Windows.Forms.Label label61;
        private System.Windows.Forms.TextBox tau2;
        private System.Windows.Forms.TextBox pop2;
        private System.Windows.Forms.TextBox tau1;
        private System.Windows.Forms.TextBox pop1;
        private System.Windows.Forms.CheckBox Values_selectedROI;
        private System.Windows.Forms.RadioButton AllRois;
        private System.Windows.Forms.RadioButton SelectRoi;
        private System.Windows.Forms.Button Fit;
        private System.Windows.Forms.ContextMenuStrip rightClickMenuStrip_inROI;
        private System.Windows.Forms.ToolStripMenuItem rmoeveRoiToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip rightClickMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem CreateRoi;
        private System.Windows.Forms.CheckBox Auto1;
        private System.Windows.Forms.TrackBar MinSldr1;
        private System.Windows.Forms.TrackBar MaxSldr1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox MinIntensity1;
        private System.Windows.Forms.TextBox MaxIntensity1;
        private System.Windows.Forms.TrackBar MinSldr2;
        private System.Windows.Forms.TrackBar MaxSldr2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox MinIntensity2;
        private System.Windows.Forms.TextBox MaxIntensity2;
        private System.Windows.Forms.TrackBar MinSldr3;
        private System.Windows.Forms.TextBox MinIntensity3;
        private System.Windows.Forms.TrackBar MaxSldr3;
        private System.Windows.Forms.TextBox MaxIntensity3;
        private System.Windows.Forms.GroupBox Fitting_Group;
        private System.Windows.Forms.GroupBox FrameSlicePanel;
        private System.Windows.Forms.RadioButton AveProjection;
        private System.Windows.Forms.RadioButton MaxProjection;
        private System.Windows.Forms.Label st_Filter;
        private System.Windows.Forms.TextBox filterWindow;
        private System.Windows.Forms.ContextMenuStrip rightClickMenu_removeAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.Button PageDownDown;
        private System.Windows.Forms.Button PageUpUp;
        private System.Windows.Forms.Button PageUp;
        private System.Windows.Forms.Button PageDown;
        private System.Windows.Forms.MenuStrip Main_Menu;
        private System.Windows.Forms.GroupBox FilePanel;
        private System.Windows.Forms.TextBox FileN;
        private System.Windows.Forms.TextBox BaseName;
        private System.Windows.Forms.Label st_fileN;
        private System.Windows.Forms.Label st_BaseName;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label st_im1;
        private System.Windows.Forms.Label st_im2;
        private System.Windows.Forms.PictureBox LifetimeCurvePlot;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFLIMImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveFLIMImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rOIsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recoverRoisToolStripMenuItem;
        private System.Windows.Forms.Button FileUp;
        private System.Windows.Forms.Button FileDown;
        private System.Windows.Forms.Label st_panel;
        private System.Windows.Forms.Label st_2Ch;
        private System.Windows.Forms.Label st_1stCh;
        private System.Windows.Forms.Label st_panel2;
        private System.Windows.Forms.PictureBox Image2;
        private System.Windows.Forms.PictureBox Image1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cb_projectionYes;
        private System.Windows.Forms.PictureBox Square_Box;
        private System.Windows.Forms.PictureBox ElipsoidBox;
        private System.Windows.Forms.PictureBox UncagingBox;
        private System.Windows.Forms.PictureBox PolygonBox;
        private System.Windows.Forms.Button stopOpening;
        private System.Windows.Forms.ToolStripMenuItem windowsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem timeCoursePlotToolStripMenuItem;
        private System.Windows.Forms.Panel ctrlPanel;
        private System.Windows.Forms.Panel st_im3;
        private System.Windows.Forms.RadioButton Ch12;
        private System.Windows.Forms.Panel LifetimeCh_panel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.RadioButton Ch1;
        private System.Windows.Forms.RadioButton Ch2;
        private System.Windows.Forms.ToolStripMenuItem uncagingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setUncagingPositionToolStripMenuItem;
        private System.Windows.Forms.PictureBox colorBar;
        private System.Windows.Forms.Label st_lifetime_ns;
        private System.Windows.Forms.CheckBox FrameAdjustment;
        private System.Windows.Forms.ToolStripMenuItem removeAllRoisToolStripMenuItem;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox PageEnd;
        private System.Windows.Forms.TextBox PageStart;
        private System.Windows.Forms.ToolStripMenuItem analysisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alignSlicesframesToolStripMenuItem;
        private System.Windows.Forms.Label st_pageN;
        private System.Windows.Forms.TextBox c_page;
        private System.Windows.Forms.RadioButton DoubleExp;
        private System.Windows.Forms.RadioButton SingleExp;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripMenuItem BatchProcessingToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip rightClick_CreateUncaging;
        private System.Windows.Forms.ToolStripMenuItem CreateUncagingLoc;
        private System.Windows.Forms.ContextMenuStrip rightClick_removeUncaging;
        private System.Windows.Forms.ToolStripMenuItem RemoveAllUncaging;
        private System.Windows.Forms.ContextMenuStrip rightClick_remUncageEach;
        private System.Windows.Forms.ToolStripMenuItem removeUncagingEach;
        private System.Windows.Forms.ToolStripMenuItem removeAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAllToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem createRoiToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem readRoiFromImageJToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveRoiAsImageJToolStripMenuItem;
        private System.Windows.Forms.Label psPerUnit;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox HoldCurrentImageCheckBox;
        private System.Windows.Forms.ComboBox Roi_SelectA;
        private System.Windows.Forms.ToolStripMenuItem scanThisRoiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveRoisToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.CheckBox EntireStack_Check;
        private System.Windows.Forms.ToolStripMenuItem makeSinlgeFileMovieFromFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveFLIMImageToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem deleteCurrentPageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFLIMImageInNewWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem convertToRectangularROIToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem convertToElipsoidROIToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem convertToPolygonROIToolStripMenuItem;
        private System.Windows.Forms.CheckBox MergeCB;
        private System.Windows.Forms.ToolStripMenuItem setttingToolStripMenuItem;
        private System.Windows.Forms.GroupBox FastZPanel;
        private System.Windows.Forms.Button FastZUp;
        private System.Windows.Forms.Button FastZDown;
        private System.Windows.Forms.TextBox CurrentFastZPageTB;
        private System.Windows.Forms.Label TotalFastZFrame;
        private System.Windows.Forms.ToolStripMenuItem keepPagesInMemoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem showImageDescriptionToolStripMenuItem;
        private System.Windows.Forms.GroupBox FastZPhasePanel;
        private System.Windows.Forms.Label PhaseDetectionMode_Status;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label PhaseDetectionMode_Status2;
        private System.Windows.Forms.Label FastZFreqText;
        private System.Windows.Forms.Label FastZAmpText;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label FastZPhaseText;
        private System.Windows.Forms.ToolStripMenuItem intelMKLLibraryOnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getFocusFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem driftMeasurementToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem fastZCalibrationToolStripMenuItem;
        private System.Windows.Forms.PictureBox ThreeDROIPanel;
        private System.Windows.Forms.Label umPerSliceLabel;
        private System.Windows.Forms.ToolStripMenuItem pythonScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem averageTimeCoursePythonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setPythonPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setScriptPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runPythonScriptToolStripMenuItem;
    }
}