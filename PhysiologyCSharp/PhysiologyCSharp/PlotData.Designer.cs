namespace PhysiologyCSharp
{
    partial class PlotData
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
            this.PhysDataPlot = new System.Windows.Forms.PictureBox();
            this.GainBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.GainBox2 = new System.Windows.Forms.TextBox();
            this.Ch2Panel = new System.Windows.Forms.Panel();
            this.VClampLabel2 = new System.Windows.Forms.Label();
            this.Ch1Panel = new System.Windows.Forms.Panel();
            this.VClampLabel1 = new System.Windows.Forms.Label();
            this.ChRadio1 = new System.Windows.Forms.RadioButton();
            this.ChRadio2 = new System.Windows.Forms.RadioButton();
            this.DisplayPanel = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.StartButton = new System.Windows.Forms.Button();
            this.FileDown = new System.Windows.Forms.Button();
            this.FileUp = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.FilePanel = new System.Windows.Forms.Panel();
            this.Reset = new System.Windows.Forms.Button();
            this.FileCounterBox = new System.Windows.Forms.TextBox();
            this.WidthBox = new System.Windows.Forms.TextBox();
            this.AmpBox = new System.Windows.Forms.TextBox();
            this.ScopePanel = new System.Windows.Forms.Panel();
            this.IntervalBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.CmBox = new System.Windows.Forms.TextBox();
            this.RinBox = new System.Windows.Forms.TextBox();
            this.RsBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.AmpUnitLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.PulseWidthLabel = new System.Windows.Forms.Label();
            this.ShowEpochAveCheck = new System.Windows.Forms.CheckBox();
            this.EpochControlBox = new System.Windows.Forms.Panel();
            this.FilePlotColor = new System.Windows.Forms.Label();
            this.FileInThisGroupLabel = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.Patch2Label = new System.Windows.Forms.Label();
            this.EpochUpButton = new System.Windows.Forms.Button();
            this.EpochDownButton = new System.Windows.Forms.Button();
            this.RemoveButton = new System.Windows.Forms.Button();
            this.AddButton = new System.Windows.Forms.Button();
            this.EpochReset = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.PulseBox = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.EpochBox = new System.Windows.Forms.TextBox();
            this.ShowAllDataInEpochCheck = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.PhysDataPlot)).BeginInit();
            this.Ch2Panel.SuspendLayout();
            this.Ch1Panel.SuspendLayout();
            this.DisplayPanel.SuspendLayout();
            this.FilePanel.SuspendLayout();
            this.ScopePanel.SuspendLayout();
            this.EpochControlBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PhysDataPlot
            // 
            this.PhysDataPlot.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PhysDataPlot.BackColor = System.Drawing.Color.White;
            this.PhysDataPlot.Location = new System.Drawing.Point(0, 82);
            this.PhysDataPlot.Name = "PhysDataPlot";
            this.PhysDataPlot.Size = new System.Drawing.Size(675, 445);
            this.PhysDataPlot.TabIndex = 0;
            this.PhysDataPlot.TabStop = false;
            // 
            // GainBox1
            // 
            this.GainBox1.Location = new System.Drawing.Point(42, 4);
            this.GainBox1.Name = "GainBox1";
            this.GainBox1.ReadOnly = true;
            this.GainBox1.Size = new System.Drawing.Size(32, 20);
            this.GainBox1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Gain 1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Gain 2";
            // 
            // GainBox2
            // 
            this.GainBox2.Location = new System.Drawing.Point(42, 4);
            this.GainBox2.Name = "GainBox2";
            this.GainBox2.ReadOnly = true;
            this.GainBox2.Size = new System.Drawing.Size(32, 20);
            this.GainBox2.TabIndex = 5;
            // 
            // Ch2Panel
            // 
            this.Ch2Panel.Controls.Add(this.VClampLabel2);
            this.Ch2Panel.Controls.Add(this.label2);
            this.Ch2Panel.Controls.Add(this.GainBox2);
            this.Ch2Panel.Location = new System.Drawing.Point(75, 9);
            this.Ch2Panel.Name = "Ch2Panel";
            this.Ch2Panel.Size = new System.Drawing.Size(78, 67);
            this.Ch2Panel.TabIndex = 9;
            // 
            // VClampLabel2
            // 
            this.VClampLabel2.AutoSize = true;
            this.VClampLabel2.Location = new System.Drawing.Point(18, 35);
            this.VClampLabel2.Name = "VClampLabel2";
            this.VClampLabel2.Size = new System.Drawing.Size(46, 13);
            this.VClampLabel2.TabIndex = 37;
            this.VClampLabel2.Text = "V-Clamp";
            // 
            // Ch1Panel
            // 
            this.Ch1Panel.Controls.Add(this.VClampLabel1);
            this.Ch1Panel.Controls.Add(this.label1);
            this.Ch1Panel.Controls.Add(this.GainBox1);
            this.Ch1Panel.Location = new System.Drawing.Point(-2, 9);
            this.Ch1Panel.Name = "Ch1Panel";
            this.Ch1Panel.Size = new System.Drawing.Size(78, 67);
            this.Ch1Panel.TabIndex = 10;
            // 
            // VClampLabel1
            // 
            this.VClampLabel1.AutoSize = true;
            this.VClampLabel1.Location = new System.Drawing.Point(17, 35);
            this.VClampLabel1.Name = "VClampLabel1";
            this.VClampLabel1.Size = new System.Drawing.Size(46, 13);
            this.VClampLabel1.TabIndex = 36;
            this.VClampLabel1.Text = "V-Clamp";
            // 
            // ChRadio1
            // 
            this.ChRadio1.AutoSize = true;
            this.ChRadio1.Checked = true;
            this.ChRadio1.Location = new System.Drawing.Point(8, 22);
            this.ChRadio1.Name = "ChRadio1";
            this.ChRadio1.Size = new System.Drawing.Size(44, 17);
            this.ChRadio1.TabIndex = 5;
            this.ChRadio1.TabStop = true;
            this.ChRadio1.Text = "Ch1";
            this.ChRadio1.UseVisualStyleBackColor = true;
            this.ChRadio1.CheckedChanged += new System.EventHandler(this.Ch1_CheckedChanged);
            // 
            // ChRadio2
            // 
            this.ChRadio2.AutoSize = true;
            this.ChRadio2.Location = new System.Drawing.Point(8, 39);
            this.ChRadio2.Name = "ChRadio2";
            this.ChRadio2.Size = new System.Drawing.Size(44, 17);
            this.ChRadio2.TabIndex = 5;
            this.ChRadio2.Text = "Ch2";
            this.ChRadio2.UseVisualStyleBackColor = true;
            this.ChRadio2.CheckedChanged += new System.EventHandler(this.CH2_CheckedChanged);
            // 
            // DisplayPanel
            // 
            this.DisplayPanel.Controls.Add(this.label3);
            this.DisplayPanel.Controls.Add(this.ChRadio2);
            this.DisplayPanel.Controls.Add(this.ChRadio1);
            this.DisplayPanel.Location = new System.Drawing.Point(152, 9);
            this.DisplayPanel.Name = "DisplayPanel";
            this.DisplayPanel.Size = new System.Drawing.Size(55, 67);
            this.DisplayPanel.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Display";
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(568, 12);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(99, 45);
            this.StartButton.TabIndex = 12;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // FileDown
            // 
            this.FileDown.Location = new System.Drawing.Point(4, 42);
            this.FileDown.Name = "FileDown";
            this.FileDown.Size = new System.Drawing.Size(19, 22);
            this.FileDown.TabIndex = 13;
            this.FileDown.Text = "<";
            this.FileDown.UseVisualStyleBackColor = true;
            this.FileDown.Click += new System.EventHandler(this.FileDown_Click);
            // 
            // FileUp
            // 
            this.FileUp.Location = new System.Drawing.Point(24, 42);
            this.FileUp.Name = "FileUp";
            this.FileUp.Size = new System.Drawing.Size(19, 22);
            this.FileUp.TabIndex = 14;
            this.FileUp.Text = ">";
            this.FileUp.UseVisualStyleBackColor = true;
            this.FileUp.Click += new System.EventHandler(this.FileUp_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "File";
            // 
            // FilePanel
            // 
            this.FilePanel.Controls.Add(this.Reset);
            this.FilePanel.Controls.Add(this.FileCounterBox);
            this.FilePanel.Controls.Add(this.label4);
            this.FilePanel.Controls.Add(this.FileDown);
            this.FilePanel.Controls.Add(this.FileUp);
            this.FilePanel.Location = new System.Drawing.Point(206, 9);
            this.FilePanel.Name = "FilePanel";
            this.FilePanel.Size = new System.Drawing.Size(89, 67);
            this.FilePanel.TabIndex = 12;
            // 
            // Reset
            // 
            this.Reset.Location = new System.Drawing.Point(42, 19);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(46, 20);
            this.Reset.TabIndex = 35;
            this.Reset.Text = "Reset";
            this.Reset.UseVisualStyleBackColor = true;
            this.Reset.Click += new System.EventHandler(this.Reset_Click);
            // 
            // FileCounterBox
            // 
            this.FileCounterBox.Location = new System.Drawing.Point(2, 21);
            this.FileCounterBox.Name = "FileCounterBox";
            this.FileCounterBox.Size = new System.Drawing.Size(40, 20);
            this.FileCounterBox.TabIndex = 34;
            this.FileCounterBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.FileCounterBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FileCounterBox_KeyDown);
            // 
            // WidthBox
            // 
            this.WidthBox.Location = new System.Drawing.Point(43, 3);
            this.WidthBox.Name = "WidthBox";
            this.WidthBox.Size = new System.Drawing.Size(42, 20);
            this.WidthBox.TabIndex = 5;
            this.WidthBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.WidthBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScopeParamChangedKeyDown);
            // 
            // AmpBox
            // 
            this.AmpBox.Location = new System.Drawing.Point(43, 24);
            this.AmpBox.Name = "AmpBox";
            this.AmpBox.Size = new System.Drawing.Size(42, 20);
            this.AmpBox.TabIndex = 13;
            this.AmpBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.AmpBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScopeParamChangedKeyDown);
            // 
            // ScopePanel
            // 
            this.ScopePanel.Controls.Add(this.IntervalBox);
            this.ScopePanel.Controls.Add(this.label6);
            this.ScopePanel.Controls.Add(this.label14);
            this.ScopePanel.Controls.Add(this.label13);
            this.ScopePanel.Controls.Add(this.label11);
            this.ScopePanel.Controls.Add(this.label12);
            this.ScopePanel.Controls.Add(this.CmBox);
            this.ScopePanel.Controls.Add(this.RinBox);
            this.ScopePanel.Controls.Add(this.RsBox);
            this.ScopePanel.Controls.Add(this.label10);
            this.ScopePanel.Controls.Add(this.label9);
            this.ScopePanel.Controls.Add(this.label8);
            this.ScopePanel.Controls.Add(this.WidthBox);
            this.ScopePanel.Controls.Add(this.AmpBox);
            this.ScopePanel.Controls.Add(this.label7);
            this.ScopePanel.Controls.Add(this.AmpUnitLabel);
            this.ScopePanel.Controls.Add(this.label5);
            this.ScopePanel.Controls.Add(this.PulseWidthLabel);
            this.ScopePanel.Location = new System.Drawing.Point(296, 9);
            this.ScopePanel.Name = "ScopePanel";
            this.ScopePanel.Size = new System.Drawing.Size(249, 67);
            this.ScopePanel.TabIndex = 15;
            // 
            // IntervalBox
            // 
            this.IntervalBox.Location = new System.Drawing.Point(43, 45);
            this.IntervalBox.Name = "IntervalBox";
            this.IntervalBox.Size = new System.Drawing.Size(42, 20);
            this.IntervalBox.TabIndex = 31;
            this.IntervalBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.IntervalBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScopeParamChangedKeyDown);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 13);
            this.label6.TabIndex = 33;
            this.label6.Text = "Interval";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(84, 48);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(12, 13);
            this.label14.TabIndex = 32;
            this.label14.Text = "s";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(174, 49);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(19, 13);
            this.label13.TabIndex = 30;
            this.label13.Text = "pF";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(174, 29);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(38, 13);
            this.label11.TabIndex = 29;
            this.label11.Text = "MOhm";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(174, 7);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(38, 13);
            this.label12.TabIndex = 28;
            this.label12.Text = "MOhm";
            // 
            // CmBox
            // 
            this.CmBox.Location = new System.Drawing.Point(132, 45);
            this.CmBox.Name = "CmBox";
            this.CmBox.Size = new System.Drawing.Size(42, 20);
            this.CmBox.TabIndex = 20;
            this.CmBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // RinBox
            // 
            this.RinBox.Location = new System.Drawing.Point(132, 24);
            this.RinBox.Name = "RinBox";
            this.RinBox.Size = new System.Drawing.Size(42, 20);
            this.RinBox.TabIndex = 19;
            this.RinBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // RsBox
            // 
            this.RsBox.Location = new System.Drawing.Point(132, 3);
            this.RsBox.Name = "RsBox";
            this.RsBox.Size = new System.Drawing.Size(42, 20);
            this.RsBox.TabIndex = 18;
            this.RsBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(112, 46);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(22, 13);
            this.label10.TabIndex = 27;
            this.label10.Text = "Cm";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(111, 25);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(23, 13);
            this.label9.TabIndex = 26;
            this.label9.Text = "Rin";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(113, 6);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(20, 13);
            this.label8.TabIndex = 25;
            this.label8.Text = "Rs";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 27);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(28, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "Amp";
            // 
            // AmpUnitLabel
            // 
            this.AmpUnitLabel.AutoSize = true;
            this.AmpUnitLabel.Location = new System.Drawing.Point(84, 27);
            this.AmpUnitLabel.Name = "AmpUnitLabel";
            this.AmpUnitLabel.Size = new System.Drawing.Size(22, 13);
            this.AmpUnitLabel.TabIndex = 23;
            this.AmpUnitLabel.Text = "mV";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(84, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(20, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "ms";
            // 
            // PulseWidthLabel
            // 
            this.PulseWidthLabel.AutoSize = true;
            this.PulseWidthLabel.Location = new System.Drawing.Point(9, 7);
            this.PulseWidthLabel.Name = "PulseWidthLabel";
            this.PulseWidthLabel.Size = new System.Drawing.Size(35, 13);
            this.PulseWidthLabel.TabIndex = 21;
            this.PulseWidthLabel.Text = "Width";
            // 
            // ShowEpochAveCheck
            // 
            this.ShowEpochAveCheck.AutoSize = true;
            this.ShowEpochAveCheck.Checked = true;
            this.ShowEpochAveCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowEpochAveCheck.Location = new System.Drawing.Point(200, 41);
            this.ShowEpochAveCheck.Margin = new System.Windows.Forms.Padding(2);
            this.ShowEpochAveCheck.Name = "ShowEpochAveCheck";
            this.ShowEpochAveCheck.Size = new System.Drawing.Size(125, 17);
            this.ShowEpochAveCheck.TabIndex = 36;
            this.ShowEpochAveCheck.Text = "Show group average";
            this.ShowEpochAveCheck.UseVisualStyleBackColor = true;
            this.ShowEpochAveCheck.CheckedChanged += new System.EventHandler(this.ShowEpochAveCheck_CheckedChanged);
            // 
            // EpochControlBox
            // 
            this.EpochControlBox.Controls.Add(this.FilePlotColor);
            this.EpochControlBox.Controls.Add(this.FileInThisGroupLabel);
            this.EpochControlBox.Controls.Add(this.label17);
            this.EpochControlBox.Controls.Add(this.Patch2Label);
            this.EpochControlBox.Controls.Add(this.EpochUpButton);
            this.EpochControlBox.Controls.Add(this.EpochDownButton);
            this.EpochControlBox.Controls.Add(this.RemoveButton);
            this.EpochControlBox.Controls.Add(this.AddButton);
            this.EpochControlBox.Controls.Add(this.EpochReset);
            this.EpochControlBox.Controls.Add(this.label16);
            this.EpochControlBox.Controls.Add(this.PulseBox);
            this.EpochControlBox.Controls.Add(this.label15);
            this.EpochControlBox.Controls.Add(this.EpochBox);
            this.EpochControlBox.Controls.Add(this.ShowAllDataInEpochCheck);
            this.EpochControlBox.Controls.Add(this.ShowEpochAveCheck);
            this.EpochControlBox.Location = new System.Drawing.Point(296, 107);
            this.EpochControlBox.Margin = new System.Windows.Forms.Padding(2);
            this.EpochControlBox.Name = "EpochControlBox";
            this.EpochControlBox.Size = new System.Drawing.Size(373, 67);
            this.EpochControlBox.TabIndex = 37;
            // 
            // FilePlotColor
            // 
            this.FilePlotColor.AutoSize = true;
            this.FilePlotColor.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FilePlotColor.ForeColor = System.Drawing.Color.Aqua;
            this.FilePlotColor.Location = new System.Drawing.Point(92, 41);
            this.FilePlotColor.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.FilePlotColor.Name = "FilePlotColor";
            this.FilePlotColor.Size = new System.Drawing.Size(30, 25);
            this.FilePlotColor.TabIndex = 407;
            this.FilePlotColor.Text = "– ";
            // 
            // FileInThisGroupLabel
            // 
            this.FileInThisGroupLabel.AutoSize = true;
            this.FileInThisGroupLabel.Location = new System.Drawing.Point(3, 47);
            this.FileInThisGroupLabel.Name = "FileInThisGroupLabel";
            this.FileInThisGroupLabel.Size = new System.Drawing.Size(83, 13);
            this.FileInThisGroupLabel.TabIndex = 406;
            this.FileInThisGroupLabel.Text = "File in this group";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.Aqua;
            this.label17.Location = new System.Drawing.Point(328, 14);
            this.label17.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(30, 25);
            this.label17.TabIndex = 405;
            this.label17.Text = "– ";
            // 
            // Patch2Label
            // 
            this.Patch2Label.AutoSize = true;
            this.Patch2Label.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Patch2Label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.Patch2Label.Location = new System.Drawing.Point(324, 36);
            this.Patch2Label.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Patch2Label.Name = "Patch2Label";
            this.Patch2Label.Size = new System.Drawing.Size(30, 25);
            this.Patch2Label.TabIndex = 404;
            this.Patch2Label.Text = "– ";
            // 
            // EpochUpButton
            // 
            this.EpochUpButton.Location = new System.Drawing.Point(91, 5);
            this.EpochUpButton.Name = "EpochUpButton";
            this.EpochUpButton.Size = new System.Drawing.Size(16, 22);
            this.EpochUpButton.TabIndex = 45;
            this.EpochUpButton.Text = ">";
            this.EpochUpButton.UseVisualStyleBackColor = true;
            this.EpochUpButton.Click += new System.EventHandler(this.EpochUpButton_Click);
            // 
            // EpochDownButton
            // 
            this.EpochDownButton.Location = new System.Drawing.Point(74, 5);
            this.EpochDownButton.Name = "EpochDownButton";
            this.EpochDownButton.Size = new System.Drawing.Size(16, 22);
            this.EpochDownButton.TabIndex = 44;
            this.EpochDownButton.Text = "<";
            this.EpochDownButton.UseVisualStyleBackColor = true;
            this.EpochDownButton.Click += new System.EventHandler(this.EpochDownButton_Click);
            // 
            // RemoveButton
            // 
            this.RemoveButton.Location = new System.Drawing.Point(134, 42);
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.Size = new System.Drawing.Size(58, 20);
            this.RemoveButton.TabIndex = 43;
            this.RemoveButton.Text = "Remove";
            this.RemoveButton.UseVisualStyleBackColor = true;
            this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
            // 
            // AddButton
            // 
            this.AddButton.Location = new System.Drawing.Point(134, 23);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(58, 20);
            this.AddButton.TabIndex = 42;
            this.AddButton.Text = "Add";
            this.AddButton.UseVisualStyleBackColor = true;
            this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // EpochReset
            // 
            this.EpochReset.Location = new System.Drawing.Point(134, 5);
            this.EpochReset.Name = "EpochReset";
            this.EpochReset.Size = new System.Drawing.Size(58, 20);
            this.EpochReset.TabIndex = 41;
            this.EpochReset.Text = "Reset";
            this.EpochReset.UseVisualStyleBackColor = true;
            this.EpochReset.Click += new System.EventHandler(this.EpochReset_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(3, 29);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(33, 13);
            this.label16.TabIndex = 38;
            this.label16.Text = "Pulse";
            // 
            // PulseBox
            // 
            this.PulseBox.Location = new System.Drawing.Point(43, 26);
            this.PulseBox.Name = "PulseBox";
            this.PulseBox.ReadOnly = true;
            this.PulseBox.Size = new System.Drawing.Size(27, 20);
            this.PulseBox.TabIndex = 39;
            this.PulseBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(3, 9);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(38, 13);
            this.label15.TabIndex = 34;
            this.label15.Text = "Epoch";
            // 
            // EpochBox
            // 
            this.EpochBox.Location = new System.Drawing.Point(43, 6);
            this.EpochBox.Name = "EpochBox";
            this.EpochBox.Size = new System.Drawing.Size(27, 20);
            this.EpochBox.TabIndex = 34;
            this.EpochBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.EpochBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EpochBox_KeyDown);
            // 
            // ShowAllDataInEpochCheck
            // 
            this.ShowAllDataInEpochCheck.AutoSize = true;
            this.ShowAllDataInEpochCheck.Location = new System.Drawing.Point(200, 20);
            this.ShowAllDataInEpochCheck.Margin = new System.Windows.Forms.Padding(2);
            this.ShowAllDataInEpochCheck.Name = "ShowAllDataInEpochCheck";
            this.ShowAllDataInEpochCheck.Size = new System.Drawing.Size(131, 17);
            this.ShowAllDataInEpochCheck.TabIndex = 37;
            this.ShowAllDataInEpochCheck.Text = "Show all data in group";
            this.ShowAllDataInEpochCheck.UseVisualStyleBackColor = true;
            this.ShowAllDataInEpochCheck.CheckedChanged += new System.EventHandler(this.ShowAllDataInEpochCheck_CheckedChanged);
            // 
            // PlotData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(675, 527);
            this.Controls.Add(this.EpochControlBox);
            this.Controls.Add(this.ScopePanel);
            this.Controls.Add(this.FilePanel);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.DisplayPanel);
            this.Controls.Add(this.Ch1Panel);
            this.Controls.Add(this.Ch2Panel);
            this.Controls.Add(this.PhysDataPlot);
            this.Name = "PlotData";
            this.Text = "PlotData";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PlotData_FormClosing);
            this.Load += new System.EventHandler(this.PlotData_Load);
            this.Resize += new System.EventHandler(this.PlotData_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.PhysDataPlot)).EndInit();
            this.Ch2Panel.ResumeLayout(false);
            this.Ch2Panel.PerformLayout();
            this.Ch1Panel.ResumeLayout(false);
            this.Ch1Panel.PerformLayout();
            this.DisplayPanel.ResumeLayout(false);
            this.DisplayPanel.PerformLayout();
            this.FilePanel.ResumeLayout(false);
            this.FilePanel.PerformLayout();
            this.ScopePanel.ResumeLayout(false);
            this.ScopePanel.PerformLayout();
            this.EpochControlBox.ResumeLayout(false);
            this.EpochControlBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox PhysDataPlot;
        private System.Windows.Forms.TextBox GainBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox GainBox2;
        private System.Windows.Forms.Panel Ch2Panel;
        private System.Windows.Forms.Panel Ch1Panel;
        private System.Windows.Forms.RadioButton ChRadio1;
        private System.Windows.Forms.RadioButton ChRadio2;
        private System.Windows.Forms.Panel DisplayPanel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button FileDown;
        private System.Windows.Forms.Button FileUp;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel FilePanel;
        private System.Windows.Forms.TextBox WidthBox;
        private System.Windows.Forms.TextBox AmpBox;
        private System.Windows.Forms.Panel ScopePanel;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox CmBox;
        private System.Windows.Forms.TextBox RinBox;
        private System.Windows.Forms.TextBox RsBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label AmpUnitLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label PulseWidthLabel;
        private System.Windows.Forms.TextBox IntervalBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button Reset;
        private System.Windows.Forms.TextBox FileCounterBox;
        private System.Windows.Forms.CheckBox ShowEpochAveCheck;
        private System.Windows.Forms.Panel EpochControlBox;
        private System.Windows.Forms.CheckBox ShowAllDataInEpochCheck;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox PulseBox;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox EpochBox;
        private System.Windows.Forms.Button RemoveButton;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.Button EpochReset;
        private System.Windows.Forms.Button EpochUpButton;
        private System.Windows.Forms.Button EpochDownButton;
        private System.Windows.Forms.Label VClampLabel2;
        private System.Windows.Forms.Label VClampLabel1;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label Patch2Label;
        private System.Windows.Forms.Label FileInThisGroupLabel;
        private System.Windows.Forms.Label FilePlotColor;
    }
}