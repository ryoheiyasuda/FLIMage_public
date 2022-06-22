namespace PhysiologyCSharp
{
    partial class StimPanel
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
            this.Stim1Radio = new System.Windows.Forms.RadioButton();
            this.StimPlot = new System.Windows.Forms.PictureBox();
            this.Stim2Radio = new System.Windows.Forms.RadioButton();
            this.Patch2Radio = new System.Windows.Forms.RadioButton();
            this.Patch1Radio = new System.Windows.Forms.RadioButton();
            this.PatchPlot = new System.Windows.Forms.PictureBox();
            this.PulseSetGroupBox = new System.Windows.Forms.GroupBox();
            this.TotalLength = new System.Windows.Forms.TextBox();
            this.SaveButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.PulseN = new System.Windows.Forms.TextBox();
            this.PulseWidth = new System.Windows.Forms.TextBox();
            this.label62 = new System.Windows.Forms.Label();
            this.PulseAmp = new System.Windows.Forms.TextBox();
            this.label92 = new System.Windows.Forms.Label();
            this.label68 = new System.Windows.Forms.Label();
            this.PulseInterval = new System.Windows.Forms.TextBox();
            this.label90 = new System.Windows.Forms.Label();
            this.PulseDelay = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label97 = new System.Windows.Forms.Label();
            this.PulseName = new System.Windows.Forms.TextBox();
            this.PulseNumber = new System.Windows.Forms.NumericUpDown();
            this.Cycle = new System.Windows.Forms.TextBox();
            this.elaspedTimeLabel = new System.Windows.Forms.Label();
            this.AcqDataCheck = new System.Windows.Forms.CheckBox();
            this.RepeatProgress = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.StartButton = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.OutputRate = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.PulseSet_Interval = new System.Windows.Forms.TextBox();
            this.SyncWithImage = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.PulseSet_Repeat = new System.Windows.Forms.TextBox();
            this.label93 = new System.Windows.Forms.Label();
            this.Patch1Label = new System.Windows.Forms.Label();
            this.Stim1Label = new System.Windows.Forms.Label();
            this.Patch2Label = new System.Windows.Forms.Label();
            this.Stim2Label = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setSaveFolderAndBaseNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scopeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mC700BTestPanelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SyncWithUncageCheck = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.StimPlot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PatchPlot)).BeginInit();
            this.PulseSetGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PulseNumber)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Stim1Radio
            // 
            this.Stim1Radio.AutoSize = true;
            this.Stim1Radio.Location = new System.Drawing.Point(16, 371);
            this.Stim1Radio.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Stim1Radio.Name = "Stim1Radio";
            this.Stim1Radio.Size = new System.Drawing.Size(51, 17);
            this.Stim1Radio.TabIndex = 7;
            this.Stim1Radio.Text = "Stim1";
            this.Stim1Radio.UseVisualStyleBackColor = true;
            this.Stim1Radio.CheckedChanged += new System.EventHandler(this.PatchStimRadio_CheckedChanged);
            // 
            // StimPlot
            // 
            this.StimPlot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.StimPlot.BackColor = System.Drawing.Color.White;
            this.StimPlot.Location = new System.Drawing.Point(0, 390);
            this.StimPlot.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.StimPlot.Name = "StimPlot";
            this.StimPlot.Size = new System.Drawing.Size(492, 156);
            this.StimPlot.TabIndex = 6;
            this.StimPlot.TabStop = false;
            this.StimPlot.Paint += new System.Windows.Forms.PaintEventHandler(this.PatchPlot_Paint);
            // 
            // Stim2Radio
            // 
            this.Stim2Radio.AutoSize = true;
            this.Stim2Radio.Location = new System.Drawing.Point(112, 371);
            this.Stim2Radio.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Stim2Radio.Name = "Stim2Radio";
            this.Stim2Radio.Size = new System.Drawing.Size(51, 17);
            this.Stim2Radio.TabIndex = 8;
            this.Stim2Radio.Text = "Stim2";
            this.Stim2Radio.UseVisualStyleBackColor = true;
            this.Stim2Radio.CheckedChanged += new System.EventHandler(this.PatchStimRadio_CheckedChanged);
            // 
            // Patch2Radio
            // 
            this.Patch2Radio.AutoSize = true;
            this.Patch2Radio.Location = new System.Drawing.Point(111, 195);
            this.Patch2Radio.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Patch2Radio.Name = "Patch2Radio";
            this.Patch2Radio.Size = new System.Drawing.Size(59, 17);
            this.Patch2Radio.TabIndex = 11;
            this.Patch2Radio.Text = "Patch2";
            this.Patch2Radio.UseVisualStyleBackColor = true;
            this.Patch2Radio.CheckedChanged += new System.EventHandler(this.PatchStimRadio_CheckedChanged);
            // 
            // Patch1Radio
            // 
            this.Patch1Radio.AutoSize = true;
            this.Patch1Radio.Checked = true;
            this.Patch1Radio.Location = new System.Drawing.Point(14, 195);
            this.Patch1Radio.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Patch1Radio.Name = "Patch1Radio";
            this.Patch1Radio.Size = new System.Drawing.Size(59, 17);
            this.Patch1Radio.TabIndex = 10;
            this.Patch1Radio.TabStop = true;
            this.Patch1Radio.Text = "Patch1";
            this.Patch1Radio.UseVisualStyleBackColor = true;
            this.Patch1Radio.CheckedChanged += new System.EventHandler(this.PatchStimRadio_CheckedChanged);
            // 
            // PatchPlot
            // 
            this.PatchPlot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PatchPlot.BackColor = System.Drawing.Color.White;
            this.PatchPlot.Location = new System.Drawing.Point(0, 213);
            this.PatchPlot.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.PatchPlot.Name = "PatchPlot";
            this.PatchPlot.Size = new System.Drawing.Size(492, 156);
            this.PatchPlot.TabIndex = 9;
            this.PatchPlot.TabStop = false;
            this.PatchPlot.Paint += new System.Windows.Forms.PaintEventHandler(this.PatchPlot_Paint);
            // 
            // PulseSetGroupBox
            // 
            this.PulseSetGroupBox.Controls.Add(this.TotalLength);
            this.PulseSetGroupBox.Controls.Add(this.SaveButton);
            this.PulseSetGroupBox.Controls.Add(this.label7);
            this.PulseSetGroupBox.Controls.Add(this.label12);
            this.PulseSetGroupBox.Controls.Add(this.PulseN);
            this.PulseSetGroupBox.Controls.Add(this.PulseWidth);
            this.PulseSetGroupBox.Controls.Add(this.label62);
            this.PulseSetGroupBox.Controls.Add(this.PulseAmp);
            this.PulseSetGroupBox.Controls.Add(this.label92);
            this.PulseSetGroupBox.Controls.Add(this.label68);
            this.PulseSetGroupBox.Controls.Add(this.PulseInterval);
            this.PulseSetGroupBox.Controls.Add(this.label90);
            this.PulseSetGroupBox.Controls.Add(this.PulseDelay);
            this.PulseSetGroupBox.Controls.Add(this.label10);
            this.PulseSetGroupBox.Controls.Add(this.label97);
            this.PulseSetGroupBox.Controls.Add(this.PulseName);
            this.PulseSetGroupBox.Controls.Add(this.PulseNumber);
            this.PulseSetGroupBox.Location = new System.Drawing.Point(6, 27);
            this.PulseSetGroupBox.Name = "PulseSetGroupBox";
            this.PulseSetGroupBox.Size = new System.Drawing.Size(316, 108);
            this.PulseSetGroupBox.TabIndex = 401;
            this.PulseSetGroupBox.TabStop = false;
            this.PulseSetGroupBox.Text = "Pulse Set";
            // 
            // TotalLength
            // 
            this.TotalLength.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TotalLength.Location = new System.Drawing.Point(127, 80);
            this.TotalLength.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.TotalLength.Name = "TotalLength";
            this.TotalLength.Size = new System.Drawing.Size(50, 20);
            this.TotalLength.TabIndex = 378;
            this.TotalLength.Text = "0";
            this.TotalLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TotalLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericParameterFieldKeyDown);
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(267, 60);
            this.SaveButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(44, 21);
            this.SaveButton.TabIndex = 406;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(176, 82);
            this.label7.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(21, 14);
            this.label7.TabIndex = 397;
            this.label7.Text = "ms";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(8, 19);
            this.label12.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(45, 14);
            this.label12.TabIndex = 364;
            this.label12.Text = "#Pulses";
            // 
            // PulseN
            // 
            this.PulseN.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PulseN.Location = new System.Drawing.Point(8, 36);
            this.PulseN.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PulseN.Name = "PulseN";
            this.PulseN.Size = new System.Drawing.Size(50, 20);
            this.PulseN.TabIndex = 363;
            this.PulseN.Text = "0";
            this.PulseN.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PulseN.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericParameterFieldKeyDown);
            // 
            // PulseWidth
            // 
            this.PulseWidth.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PulseWidth.Location = new System.Drawing.Point(68, 36);
            this.PulseWidth.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PulseWidth.Name = "PulseWidth";
            this.PulseWidth.Size = new System.Drawing.Size(50, 20);
            this.PulseWidth.TabIndex = 365;
            this.PulseWidth.Text = "0";
            this.PulseWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PulseWidth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericParameterFieldKeyDown);
            // 
            // label62
            // 
            this.label62.AutoSize = true;
            this.label62.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label62.Location = new System.Drawing.Point(68, 19);
            this.label62.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(56, 14);
            this.label62.TabIndex = 366;
            this.label62.Text = "Width(ms)";
            // 
            // PulseAmp
            // 
            this.PulseAmp.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PulseAmp.Location = new System.Drawing.Point(127, 36);
            this.PulseAmp.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PulseAmp.Name = "PulseAmp";
            this.PulseAmp.Size = new System.Drawing.Size(50, 20);
            this.PulseAmp.TabIndex = 367;
            this.PulseAmp.Text = "0";
            this.PulseAmp.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PulseAmp.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericParameterFieldKeyDown);
            // 
            // label92
            // 
            this.label92.AutoSize = true;
            this.label92.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label92.Location = new System.Drawing.Point(124, 64);
            this.label92.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(61, 14);
            this.label92.TabIndex = 379;
            this.label92.Text = "Total length";
            // 
            // label68
            // 
            this.label68.AutoSize = true;
            this.label68.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label68.Location = new System.Drawing.Point(126, 19);
            this.label68.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label68.Name = "label68";
            this.label68.Size = new System.Drawing.Size(72, 14);
            this.label68.TabIndex = 368;
            this.label68.Text = "Amp(mV/mA)";
            // 
            // PulseInterval
            // 
            this.PulseInterval.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PulseInterval.Location = new System.Drawing.Point(8, 81);
            this.PulseInterval.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PulseInterval.Name = "PulseInterval";
            this.PulseInterval.Size = new System.Drawing.Size(50, 20);
            this.PulseInterval.TabIndex = 374;
            this.PulseInterval.Text = "0";
            this.PulseInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PulseInterval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericParameterFieldKeyDown);
            // 
            // label90
            // 
            this.label90.AutoSize = true;
            this.label90.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label90.Location = new System.Drawing.Point(68, 64);
            this.label90.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label90.Name = "label90";
            this.label90.Size = new System.Drawing.Size(56, 14);
            this.label90.TabIndex = 377;
            this.label90.Text = "Delay(ms)";
            // 
            // PulseDelay
            // 
            this.PulseDelay.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PulseDelay.Location = new System.Drawing.Point(68, 81);
            this.PulseDelay.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PulseDelay.Name = "PulseDelay";
            this.PulseDelay.Size = new System.Drawing.Size(50, 20);
            this.PulseDelay.TabIndex = 376;
            this.PulseDelay.Text = "0";
            this.PulseDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PulseDelay.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericParameterFieldKeyDown);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(8, 64);
            this.label10.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(64, 14);
            this.label10.TabIndex = 375;
            this.label10.Text = "Interval(ms)";
            // 
            // label97
            // 
            this.label97.AutoSize = true;
            this.label97.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label97.Location = new System.Drawing.Point(209, 19);
            this.label97.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label97.Name = "label97";
            this.label97.Size = new System.Drawing.Size(34, 14);
            this.label97.TabIndex = 389;
            this.label97.Text = "Name";
            // 
            // PulseName
            // 
            this.PulseName.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PulseName.Location = new System.Drawing.Point(209, 36);
            this.PulseName.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PulseName.Name = "PulseName";
            this.PulseName.Size = new System.Drawing.Size(92, 20);
            this.PulseName.TabIndex = 388;
            this.PulseName.Text = "Pulse set";
            this.PulseName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericParameterFieldKeyDown);
            // 
            // PulseNumber
            // 
            this.PulseNumber.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PulseNumber.Location = new System.Drawing.Point(209, 60);
            this.PulseNumber.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PulseNumber.Name = "PulseNumber";
            this.PulseNumber.Size = new System.Drawing.Size(52, 20);
            this.PulseNumber.TabIndex = 390;
            this.PulseNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PulseNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.PulseNumber.ValueChanged += new System.EventHandler(this.PulseNumber_ValueChanged);
            // 
            // Cycle
            // 
            this.Cycle.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Cycle.Location = new System.Drawing.Point(332, 58);
            this.Cycle.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.Cycle.Name = "Cycle";
            this.Cycle.Size = new System.Drawing.Size(146, 20);
            this.Cycle.TabIndex = 401;
            this.Cycle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericParameterFieldKeyDown);
            // 
            // elaspedTimeLabel
            // 
            this.elaspedTimeLabel.AutoSize = true;
            this.elaspedTimeLabel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.elaspedTimeLabel.Location = new System.Drawing.Point(403, 135);
            this.elaspedTimeLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.elaspedTimeLabel.Name = "elaspedTimeLabel";
            this.elaspedTimeLabel.Size = new System.Drawing.Size(33, 16);
            this.elaspedTimeLabel.TabIndex = 405;
            this.elaspedTimeLabel.Text = "0.00";
            // 
            // AcqDataCheck
            // 
            this.AcqDataCheck.AutoSize = true;
            this.AcqDataCheck.Location = new System.Drawing.Point(14, 172);
            this.AcqDataCheck.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.AcqDataCheck.Name = "AcqDataCheck";
            this.AcqDataCheck.Size = new System.Drawing.Size(86, 17);
            this.AcqDataCheck.TabIndex = 404;
            this.AcqDataCheck.Text = "Acquire data";
            this.AcqDataCheck.UseVisualStyleBackColor = true;
            this.AcqDataCheck.CheckedChanged += new System.EventHandler(this.AcqDataCheck_CheckedChanged);
            // 
            // RepeatProgress
            // 
            this.RepeatProgress.AutoSize = true;
            this.RepeatProgress.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RepeatProgress.Location = new System.Drawing.Point(312, 181);
            this.RepeatProgress.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.RepeatProgress.Name = "RepeatProgress";
            this.RepeatProgress.Size = new System.Drawing.Size(26, 16);
            this.RepeatProgress.TabIndex = 403;
            this.RepeatProgress.Text = "0/0";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(332, 42);
            this.label11.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(81, 14);
            this.label11.TabIndex = 402;
            this.label11.Text = "Cycle pulse set";
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(396, 154);
            this.StartButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(76, 34);
            this.StartButton.TabIndex = 400;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(239, 164);
            this.label9.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(13, 14);
            this.label9.TabIndex = 399;
            this.label9.Text = "s";
            // 
            // OutputRate
            // 
            this.OutputRate.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OutputRate.Location = new System.Drawing.Point(332, 98);
            this.OutputRate.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.OutputRate.Name = "OutputRate";
            this.OutputRate.Size = new System.Drawing.Size(50, 20);
            this.OutputRate.TabIndex = 387;
            this.OutputRate.Text = "0";
            this.OutputRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.OutputRate.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericParameterFieldKeyDown);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(332, 102);
            this.label8.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 14);
            this.label8.TabIndex = 398;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(185, 147);
            this.label6.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(87, 14);
            this.label6.TabIndex = 396;
            this.label6.Text = "PulseSet interval";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PulseSet_Interval
            // 
            this.PulseSet_Interval.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PulseSet_Interval.Location = new System.Drawing.Point(187, 161);
            this.PulseSet_Interval.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PulseSet_Interval.Name = "PulseSet_Interval";
            this.PulseSet_Interval.Size = new System.Drawing.Size(50, 20);
            this.PulseSet_Interval.TabIndex = 395;
            this.PulseSet_Interval.Text = "0";
            this.PulseSet_Interval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PulseSet_Interval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericParameterFieldKeyDown);
            // 
            // SyncWithImage
            // 
            this.SyncWithImage.AutoSize = true;
            this.SyncWithImage.Location = new System.Drawing.Point(14, 137);
            this.SyncWithImage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.SyncWithImage.Name = "SyncWithImage";
            this.SyncWithImage.Size = new System.Drawing.Size(117, 17);
            this.SyncWithImage.TabIndex = 394;
            this.SyncWithImage.Text = "Sync with FLIMage";
            this.SyncWithImage.UseVisualStyleBackColor = true;
            this.SyncWithImage.CheckedChanged += new System.EventHandler(this.SyncWithImage_CheckedChanged);
            this.SyncWithImage.Click += new System.EventHandler(this.SyncWithUncageCheck_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(286, 147);
            this.label5.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 14);
            this.label5.TabIndex = 393;
            this.label5.Text = "PulseSet Rep";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PulseSet_Repeat
            // 
            this.PulseSet_Repeat.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PulseSet_Repeat.Location = new System.Drawing.Point(288, 161);
            this.PulseSet_Repeat.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PulseSet_Repeat.Name = "PulseSet_Repeat";
            this.PulseSet_Repeat.Size = new System.Drawing.Size(50, 20);
            this.PulseSet_Repeat.TabIndex = 392;
            this.PulseSet_Repeat.Text = "0";
            this.PulseSet_Repeat.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PulseSet_Repeat.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericParameterFieldKeyDown);
            // 
            // label93
            // 
            this.label93.AutoSize = true;
            this.label93.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label93.Location = new System.Drawing.Point(332, 83);
            this.label93.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label93.Name = "label93";
            this.label93.Size = new System.Drawing.Size(85, 14);
            this.label93.TabIndex = 382;
            this.label93.Text = "Output rate (Hz)";
            // 
            // Patch1Label
            // 
            this.Patch1Label.AutoSize = true;
            this.Patch1Label.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Patch1Label.ForeColor = System.Drawing.Color.Red;
            this.Patch1Label.Location = new System.Drawing.Point(69, 190);
            this.Patch1Label.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Patch1Label.Name = "Patch1Label";
            this.Patch1Label.Size = new System.Drawing.Size(30, 25);
            this.Patch1Label.TabIndex = 395;
            this.Patch1Label.Text = "– ";
            // 
            // Stim1Label
            // 
            this.Stim1Label.AutoSize = true;
            this.Stim1Label.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Stim1Label.ForeColor = System.Drawing.Color.Red;
            this.Stim1Label.Location = new System.Drawing.Point(69, 365);
            this.Stim1Label.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Stim1Label.Name = "Stim1Label";
            this.Stim1Label.Size = new System.Drawing.Size(30, 25);
            this.Stim1Label.TabIndex = 402;
            this.Stim1Label.Text = "– ";
            // 
            // Patch2Label
            // 
            this.Patch2Label.AutoSize = true;
            this.Patch2Label.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Patch2Label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.Patch2Label.Location = new System.Drawing.Point(165, 190);
            this.Patch2Label.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Patch2Label.Name = "Patch2Label";
            this.Patch2Label.Size = new System.Drawing.Size(30, 25);
            this.Patch2Label.TabIndex = 403;
            this.Patch2Label.Text = "– ";
            // 
            // Stim2Label
            // 
            this.Stim2Label.AutoSize = true;
            this.Stim2Label.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Stim2Label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.Stim2Label.Location = new System.Drawing.Point(165, 365);
            this.Stim2Label.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Stim2Label.Name = "Stim2Label";
            this.Stim2Label.Size = new System.Drawing.Size(30, 25);
            this.Stim2Label.TabIndex = 404;
            this.Stim2Label.Text = "– ";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.windowsToolStripMenuItem,
            this.analysisToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(492, 24);
            this.menuStrip1.TabIndex = 405;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setSaveFolderAndBaseNameToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // setSaveFolderAndBaseNameToolStripMenuItem
            // 
            this.setSaveFolderAndBaseNameToolStripMenuItem.Name = "setSaveFolderAndBaseNameToolStripMenuItem";
            this.setSaveFolderAndBaseNameToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
            this.setSaveFolderAndBaseNameToolStripMenuItem.Text = "Set Save Folder and BaseName";
            this.setSaveFolderAndBaseNameToolStripMenuItem.Click += new System.EventHandler(this.setSaveFolderAndBaseNameToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
            this.openToolStripMenuItem.Text = "Open data";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
            this.saveToolStripMenuItem.Text = "Save data";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scopeToolStripMenuItem,
            this.mC700BTestPanelToolStripMenuItem,
            this.resetToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(47, 22);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // scopeToolStripMenuItem
            // 
            this.scopeToolStripMenuItem.Name = "scopeToolStripMenuItem";
            this.scopeToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.scopeToolStripMenuItem.Text = "Scope";
            this.scopeToolStripMenuItem.Click += new System.EventHandler(this.scopeToolStripMenuItem_Click);
            // 
            // mC700BTestPanelToolStripMenuItem
            // 
            this.mC700BTestPanelToolStripMenuItem.Name = "mC700BTestPanelToolStripMenuItem";
            this.mC700BTestPanelToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.mC700BTestPanelToolStripMenuItem.Text = "MC700B Test Panel";
            this.mC700BTestPanelToolStripMenuItem.Click += new System.EventHandler(this.mC700BTestPanelToolStripMenuItem_Click);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.resetToolStripMenuItem.Text = "Restart";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);
            // 
            // windowsToolStripMenuItem
            // 
            this.windowsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dataWindowToolStripMenuItem});
            this.windowsToolStripMenuItem.Name = "windowsToolStripMenuItem";
            this.windowsToolStripMenuItem.Size = new System.Drawing.Size(68, 22);
            this.windowsToolStripMenuItem.Text = "Windows";
            // 
            // dataWindowToolStripMenuItem
            // 
            this.dataWindowToolStripMenuItem.Name = "dataWindowToolStripMenuItem";
            this.dataWindowToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.dataWindowToolStripMenuItem.Text = "Data window";
            this.dataWindowToolStripMenuItem.Click += new System.EventHandler(this.dataWindowToolStripMenuItem_Click);
            // 
            // analysisToolStripMenuItem
            // 
            this.analysisToolStripMenuItem.Name = "analysisToolStripMenuItem";
            this.analysisToolStripMenuItem.Size = new System.Drawing.Size(62, 22);
            this.analysisToolStripMenuItem.Text = "Analysis";
            this.analysisToolStripMenuItem.Click += new System.EventHandler(this.analysisToolStripMenuItem_Click);
            // 
            // SyncWithUncageCheck
            // 
            this.SyncWithUncageCheck.AutoSize = true;
            this.SyncWithUncageCheck.Location = new System.Drawing.Point(14, 155);
            this.SyncWithUncageCheck.Margin = new System.Windows.Forms.Padding(2);
            this.SyncWithUncageCheck.Name = "SyncWithUncageCheck";
            this.SyncWithUncageCheck.Size = new System.Drawing.Size(119, 17);
            this.SyncWithUncageCheck.TabIndex = 406;
            this.SyncWithUncageCheck.Text = "Sync with uncaging";
            this.SyncWithUncageCheck.UseVisualStyleBackColor = true;
            this.SyncWithUncageCheck.Click += new System.EventHandler(this.SyncWithUncageCheck_Click);
            // 
            // StimPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 552);
            this.Controls.Add(this.SyncWithUncageCheck);
            this.Controls.Add(this.AcqDataCheck);
            this.Controls.Add(this.OutputRate);
            this.Controls.Add(this.PulseSetGroupBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.Cycle);
            this.Controls.Add(this.SyncWithImage);
            this.Controls.Add(this.Patch2Radio);
            this.Controls.Add(this.elaspedTimeLabel);
            this.Controls.Add(this.Patch1Radio);
            this.Controls.Add(this.PatchPlot);
            this.Controls.Add(this.RepeatProgress);
            this.Controls.Add(this.Stim2Radio);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.Stim1Radio);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label93);
            this.Controls.Add(this.StimPlot);
            this.Controls.Add(this.Stim2Label);
            this.Controls.Add(this.Patch2Label);
            this.Controls.Add(this.Stim1Label);
            this.Controls.Add(this.Patch1Label);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.PulseSet_Interval);
            this.Controls.Add(this.PulseSet_Repeat);
            this.Controls.Add(this.label5);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "StimPanel";
            this.Text = "StimPanel";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StimPanel_FormClosing);
            this.Load += new System.EventHandler(this.StimPanel_Load);
            this.Shown += new System.EventHandler(this.StimPanel_Shown);
            this.Resize += new System.EventHandler(this.StimPanel_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.StimPlot)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PatchPlot)).EndInit();
            this.PulseSetGroupBox.ResumeLayout(false);
            this.PulseSetGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PulseNumber)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RadioButton Stim1Radio;
        private System.Windows.Forms.PictureBox StimPlot;
        private System.Windows.Forms.RadioButton Stim2Radio;
        private System.Windows.Forms.RadioButton Patch2Radio;
        private System.Windows.Forms.RadioButton Patch1Radio;
        private System.Windows.Forms.PictureBox PatchPlot;
        private System.Windows.Forms.GroupBox PulseSetGroupBox;
        private System.Windows.Forms.TextBox PulseName;
        private System.Windows.Forms.NumericUpDown PulseNumber;
        private System.Windows.Forms.Label label97;
        private System.Windows.Forms.TextBox OutputRate;
        private System.Windows.Forms.Label label92;
        private System.Windows.Forms.TextBox TotalLength;
        private System.Windows.Forms.Label label93;
        private System.Windows.Forms.CheckBox SyncWithImage;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox PulseSet_Repeat;
        private System.Windows.Forms.Label Patch1Label;
        private System.Windows.Forms.Label Stim1Label;
        private System.Windows.Forms.Label Patch2Label;
        private System.Windows.Forms.Label Stim2Label;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox PulseN;
        private System.Windows.Forms.TextBox PulseWidth;
        private System.Windows.Forms.Label label62;
        private System.Windows.Forms.TextBox PulseAmp;
        private System.Windows.Forms.Label label68;
        private System.Windows.Forms.TextBox PulseInterval;
        private System.Windows.Forms.Label label90;
        private System.Windows.Forms.TextBox PulseDelay;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox PulseSet_Interval;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox Cycle;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mC700BTestPanelToolStripMenuItem;
        private System.Windows.Forms.Label RepeatProgress;
        private System.Windows.Forms.CheckBox AcqDataCheck;
        private System.Windows.Forms.Label elaspedTimeLabel;
        private System.Windows.Forms.ToolStripMenuItem scopeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dataWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setSaveFolderAndBaseNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem analysisToolStripMenuItem;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.CheckBox SyncWithUncageCheck;
    }
}