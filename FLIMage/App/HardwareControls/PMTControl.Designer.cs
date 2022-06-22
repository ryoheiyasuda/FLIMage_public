namespace FLIMage.HardwareControls
{
    partial class PMTControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        public System.ComponentModel.IContainer components = null;

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
        public void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PMTControl));
            this.PMTGain1 = new System.Windows.Forms.TextBox();
            this.PMT_GainLabel1 = new System.Windows.Forms.Label();
            this.PMTPanel = new System.Windows.Forms.GroupBox();
            this.PMT2Status = new System.Windows.Forms.Label();
            this.PMT1Status = new System.Windows.Forms.Label();
            this.PMT_GainLabel2 = new System.Windows.Forms.Label();
            this.PMTEnableCB = new System.Windows.Forms.CheckBox();
            this.PMTGain2 = new System.Windows.Forms.TextBox();
            this.GalvoPanel = new System.Windows.Forms.GroupBox();
            this.GalvoOnRadio = new System.Windows.Forms.RadioButton();
            this.GalvoOffRadio = new System.Windows.Forms.RadioButton();
            this.ResGalvoPanel = new System.Windows.Forms.GroupBox();
            this.ResGalvoOnRadio = new System.Windows.Forms.RadioButton();
            this.ResGalvoOffRadio = new System.Windows.Forms.RadioButton();
            this.CameraPanel = new System.Windows.Forms.GroupBox();
            this.CameraOnRadio = new System.Windows.Forms.RadioButton();
            this.CameraOffRadio = new System.Windows.Forms.RadioButton();
            this.PMTPanel.SuspendLayout();
            this.GalvoPanel.SuspendLayout();
            this.ResGalvoPanel.SuspendLayout();
            this.CameraPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // PMTGain1
            // 
            this.PMTGain1.Location = new System.Drawing.Point(164, 24);
            this.PMTGain1.Name = "PMTGain1";
            this.PMTGain1.Size = new System.Drawing.Size(44, 20);
            this.PMTGain1.TabIndex = 0;
            this.PMTGain1.Text = "480";
            this.PMTGain1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // PMT_GainLabel1
            // 
            this.PMT_GainLabel1.AutoSize = true;
            this.PMT_GainLabel1.Location = new System.Drawing.Point(21, 27);
            this.PMT_GainLabel1.Name = "PMT_GainLabel1";
            this.PMT_GainLabel1.Size = new System.Drawing.Size(106, 13);
            this.PMT_GainLabel1.TabIndex = 2;
            this.PMT_GainLabel1.Text = "PMT Gain 1 (1 - 510)";
            // 
            // PMTPanel
            // 
            this.PMTPanel.Controls.Add(this.PMT2Status);
            this.PMTPanel.Controls.Add(this.PMT1Status);
            this.PMTPanel.Controls.Add(this.PMT_GainLabel2);
            this.PMTPanel.Controls.Add(this.PMTEnableCB);
            this.PMTPanel.Controls.Add(this.PMT_GainLabel1);
            this.PMTPanel.Controls.Add(this.PMTGain1);
            this.PMTPanel.Controls.Add(this.PMTGain2);
            this.PMTPanel.Location = new System.Drawing.Point(12, 12);
            this.PMTPanel.Name = "PMTPanel";
            this.PMTPanel.Size = new System.Drawing.Size(348, 151);
            this.PMTPanel.TabIndex = 4;
            this.PMTPanel.TabStop = false;
            this.PMTPanel.Text = "PMT Control";
            // 
            // PMT2Status
            // 
            this.PMT2Status.AutoSize = true;
            this.PMT2Status.Location = new System.Drawing.Point(27, 95);
            this.PMT2Status.Name = "PMT2Status";
            this.PMT2Status.Size = new System.Drawing.Size(0, 13);
            this.PMT2Status.TabIndex = 7;
            // 
            // PMT1Status
            // 
            this.PMT1Status.AutoSize = true;
            this.PMT1Status.Location = new System.Drawing.Point(27, 47);
            this.PMT1Status.Name = "PMT1Status";
            this.PMT1Status.Size = new System.Drawing.Size(0, 13);
            this.PMT1Status.TabIndex = 6;
            // 
            // PMT_GainLabel2
            // 
            this.PMT_GainLabel2.AutoSize = true;
            this.PMT_GainLabel2.Location = new System.Drawing.Point(21, 75);
            this.PMT_GainLabel2.Name = "PMT_GainLabel2";
            this.PMT_GainLabel2.Size = new System.Drawing.Size(106, 13);
            this.PMT_GainLabel2.TabIndex = 5;
            this.PMT_GainLabel2.Text = "PMT Gain 2 (1 - 510)";
            // 
            // PMTEnableCB
            // 
            this.PMTEnableCB.AutoSize = true;
            this.PMTEnableCB.Location = new System.Drawing.Point(24, 121);
            this.PMTEnableCB.Name = "PMTEnableCB";
            this.PMTEnableCB.Size = new System.Drawing.Size(85, 17);
            this.PMTEnableCB.TabIndex = 4;
            this.PMTEnableCB.Text = "Enable PMT";
            this.PMTEnableCB.UseVisualStyleBackColor = true;
            this.PMTEnableCB.CheckedChanged += new System.EventHandler(this.PMTEnableCB_CheckedChanged);
            // 
            // PMTGain2
            // 
            this.PMTGain2.Location = new System.Drawing.Point(164, 72);
            this.PMTGain2.Name = "PMTGain2";
            this.PMTGain2.Size = new System.Drawing.Size(44, 20);
            this.PMTGain2.TabIndex = 1;
            this.PMTGain2.Text = "480";
            this.PMTGain2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // GalvoPanel
            // 
            this.GalvoPanel.Controls.Add(this.GalvoOnRadio);
            this.GalvoPanel.Controls.Add(this.GalvoOffRadio);
            this.GalvoPanel.Enabled = false;
            this.GalvoPanel.Location = new System.Drawing.Point(12, 179);
            this.GalvoPanel.Name = "GalvoPanel";
            this.GalvoPanel.Size = new System.Drawing.Size(86, 77);
            this.GalvoPanel.TabIndex = 5;
            this.GalvoPanel.TabStop = false;
            this.GalvoPanel.Text = "Galvo";
            // 
            // GalvoOnRadio
            // 
            this.GalvoOnRadio.AutoSize = true;
            this.GalvoOnRadio.Location = new System.Drawing.Point(11, 28);
            this.GalvoOnRadio.Name = "GalvoOnRadio";
            this.GalvoOnRadio.Size = new System.Drawing.Size(39, 17);
            this.GalvoOnRadio.TabIndex = 1;
            this.GalvoOnRadio.Text = "On";
            this.GalvoOnRadio.UseVisualStyleBackColor = true;
            this.GalvoOnRadio.Click += new System.EventHandler(this.GalvoOnOffClick);
            // 
            // GalvoOffRadio
            // 
            this.GalvoOffRadio.AutoSize = true;
            this.GalvoOffRadio.Checked = true;
            this.GalvoOffRadio.Location = new System.Drawing.Point(11, 49);
            this.GalvoOffRadio.Name = "GalvoOffRadio";
            this.GalvoOffRadio.Size = new System.Drawing.Size(39, 17);
            this.GalvoOffRadio.TabIndex = 0;
            this.GalvoOffRadio.TabStop = true;
            this.GalvoOffRadio.Text = "Off";
            this.GalvoOffRadio.UseVisualStyleBackColor = true;
            this.GalvoOffRadio.Click += new System.EventHandler(this.GalvoOnOffClick);
            // 
            // ResGalvoPanel
            // 
            this.ResGalvoPanel.Controls.Add(this.ResGalvoOnRadio);
            this.ResGalvoPanel.Controls.Add(this.ResGalvoOffRadio);
            this.ResGalvoPanel.Enabled = false;
            this.ResGalvoPanel.Location = new System.Drawing.Point(114, 179);
            this.ResGalvoPanel.Name = "ResGalvoPanel";
            this.ResGalvoPanel.Size = new System.Drawing.Size(86, 77);
            this.ResGalvoPanel.TabIndex = 6;
            this.ResGalvoPanel.TabStop = false;
            this.ResGalvoPanel.Text = "Res Galvo";
            // 
            // ResGalvoOnRadio
            // 
            this.ResGalvoOnRadio.AutoSize = true;
            this.ResGalvoOnRadio.Location = new System.Drawing.Point(11, 28);
            this.ResGalvoOnRadio.Name = "ResGalvoOnRadio";
            this.ResGalvoOnRadio.Size = new System.Drawing.Size(39, 17);
            this.ResGalvoOnRadio.TabIndex = 1;
            this.ResGalvoOnRadio.Text = "On";
            this.ResGalvoOnRadio.UseVisualStyleBackColor = true;
            this.ResGalvoOnRadio.Click += new System.EventHandler(this.ReGaolvoClick);
            // 
            // ResGalvoOffRadio
            // 
            this.ResGalvoOffRadio.AutoSize = true;
            this.ResGalvoOffRadio.Checked = true;
            this.ResGalvoOffRadio.Location = new System.Drawing.Point(11, 49);
            this.ResGalvoOffRadio.Name = "ResGalvoOffRadio";
            this.ResGalvoOffRadio.Size = new System.Drawing.Size(39, 17);
            this.ResGalvoOffRadio.TabIndex = 0;
            this.ResGalvoOffRadio.TabStop = true;
            this.ResGalvoOffRadio.Text = "Off";
            this.ResGalvoOffRadio.UseVisualStyleBackColor = true;
            this.ResGalvoOffRadio.Click += new System.EventHandler(this.ReGaolvoClick);
            // 
            // CameraPanel
            // 
            this.CameraPanel.Controls.Add(this.CameraOnRadio);
            this.CameraPanel.Controls.Add(this.CameraOffRadio);
            this.CameraPanel.Enabled = false;
            this.CameraPanel.Location = new System.Drawing.Point(213, 179);
            this.CameraPanel.Name = "CameraPanel";
            this.CameraPanel.Size = new System.Drawing.Size(86, 77);
            this.CameraPanel.TabIndex = 6;
            this.CameraPanel.TabStop = false;
            this.CameraPanel.Text = "Camera";
            // 
            // CameraOnRadio
            // 
            this.CameraOnRadio.AutoSize = true;
            this.CameraOnRadio.Checked = true;
            this.CameraOnRadio.Location = new System.Drawing.Point(11, 28);
            this.CameraOnRadio.Name = "CameraOnRadio";
            this.CameraOnRadio.Size = new System.Drawing.Size(39, 17);
            this.CameraOnRadio.TabIndex = 1;
            this.CameraOnRadio.TabStop = true;
            this.CameraOnRadio.Text = "On";
            this.CameraOnRadio.UseVisualStyleBackColor = true;
            this.CameraOnRadio.Click += new System.EventHandler(this.CameraOnOffClick);
            // 
            // CameraOffRadio
            // 
            this.CameraOffRadio.AutoSize = true;
            this.CameraOffRadio.Location = new System.Drawing.Point(11, 49);
            this.CameraOffRadio.Name = "CameraOffRadio";
            this.CameraOffRadio.Size = new System.Drawing.Size(39, 17);
            this.CameraOffRadio.TabIndex = 0;
            this.CameraOffRadio.Text = "Off";
            this.CameraOffRadio.UseVisualStyleBackColor = true;
            this.CameraOffRadio.Click += new System.EventHandler(this.CameraOnOffClick);
            // 
            // PMTControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 274);
            this.Controls.Add(this.CameraPanel);
            this.Controls.Add(this.ResGalvoPanel);
            this.Controls.Add(this.GalvoPanel);
            this.Controls.Add(this.PMTPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PMTControl";
            this.Text = "PMT Contoller";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PMTControl_FormClosing);
            this.Load += new System.EventHandler(this.PMTControl_Load);
            this.PMTPanel.ResumeLayout(false);
            this.PMTPanel.PerformLayout();
            this.GalvoPanel.ResumeLayout(false);
            this.GalvoPanel.PerformLayout();
            this.ResGalvoPanel.ResumeLayout(false);
            this.ResGalvoPanel.PerformLayout();
            this.CameraPanel.ResumeLayout(false);
            this.CameraPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TextBox PMTGain1;
        public System.Windows.Forms.Label PMT_GainLabel1;
        public System.Windows.Forms.GroupBox PMTPanel;
        public System.Windows.Forms.Label PMT_GainLabel2;
        public System.Windows.Forms.CheckBox PMTEnableCB;
        public System.Windows.Forms.GroupBox GalvoPanel;
        public System.Windows.Forms.RadioButton GalvoOnRadio;
        public System.Windows.Forms.RadioButton GalvoOffRadio;
        public System.Windows.Forms.TextBox PMTGain2;
        public System.Windows.Forms.Label PMT2Status;
        public System.Windows.Forms.Label PMT1Status;
        public System.Windows.Forms.GroupBox ResGalvoPanel;
        public System.Windows.Forms.RadioButton ResGalvoOnRadio;
        public System.Windows.Forms.RadioButton ResGalvoOffRadio;
        public System.Windows.Forms.GroupBox CameraPanel;
        public System.Windows.Forms.RadioButton CameraOnRadio;
        public System.Windows.Forms.RadioButton CameraOffRadio;
    }
}

