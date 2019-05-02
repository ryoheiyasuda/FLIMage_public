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
            this.label1 = new System.Windows.Forms.Label();
            this.PMTPanel = new System.Windows.Forms.GroupBox();
            this.PMT2Status = new System.Windows.Forms.Label();
            this.PMT1Status = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.PMTEnableCB = new System.Windows.Forms.CheckBox();
            this.PMTGain2 = new System.Windows.Forms.TextBox();
            this.Camera2Pswitch = new System.Windows.Forms.GroupBox();
            this.TwoPhotonRadio = new System.Windows.Forms.RadioButton();
            this.CameraRadio = new System.Windows.Forms.RadioButton();
            this.PMTPanel.SuspendLayout();
            this.Camera2Pswitch.SuspendLayout();
            this.SuspendLayout();
            // 
            // PMTGain1
            // 
            this.PMTGain1.Location = new System.Drawing.Point(164, 24);
            this.PMTGain1.Name = "PMTGain1";
            this.PMTGain1.Size = new System.Drawing.Size(44, 20);
            this.PMTGain1.TabIndex = 0;
            this.PMTGain1.Text = "800";
            this.PMTGain1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "PMT Gain 1 (1 - 1000)";
            // 
            // PMTPanel
            // 
            this.PMTPanel.Controls.Add(this.PMT2Status);
            this.PMTPanel.Controls.Add(this.PMT1Status);
            this.PMTPanel.Controls.Add(this.label2);
            this.PMTPanel.Controls.Add(this.PMTEnableCB);
            this.PMTPanel.Controls.Add(this.label1);
            this.PMTPanel.Controls.Add(this.PMTGain1);
            this.PMTPanel.Controls.Add(this.PMTGain2);
            this.PMTPanel.Location = new System.Drawing.Point(12, 12);
            this.PMTPanel.Name = "PMTPanel";
            this.PMTPanel.Size = new System.Drawing.Size(252, 151);
            this.PMTPanel.TabIndex = 4;
            this.PMTPanel.TabStop = false;
            this.PMTPanel.Text = "PMT Control";
            // 
            // PMT2Status
            // 
            this.PMT2Status.AutoSize = true;
            this.PMT2Status.Location = new System.Drawing.Point(40, 95);
            this.PMT2Status.Name = "PMT2Status";
            this.PMT2Status.Size = new System.Drawing.Size(85, 13);
            this.PMT2Status.TabIndex = 7;
            this.PMT2Status.Text = "PMT1 Gain: 800";
            // 
            // PMT1Status
            // 
            this.PMT1Status.AutoSize = true;
            this.PMT1Status.Location = new System.Drawing.Point(40, 47);
            this.PMT1Status.Name = "PMT1Status";
            this.PMT1Status.Size = new System.Drawing.Size(85, 13);
            this.PMT1Status.TabIndex = 6;
            this.PMT1Status.Text = "PMT1 Gain: 800";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "PMT Gain 2 (1 - 1000)";
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
            this.PMTGain2.Text = "800";
            this.PMTGain2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Camera2Pswitch
            // 
            this.Camera2Pswitch.Controls.Add(this.TwoPhotonRadio);
            this.Camera2Pswitch.Controls.Add(this.CameraRadio);
            this.Camera2Pswitch.Enabled = false;
            this.Camera2Pswitch.Location = new System.Drawing.Point(12, 179);
            this.Camera2Pswitch.Name = "Camera2Pswitch";
            this.Camera2Pswitch.Size = new System.Drawing.Size(252, 77);
            this.Camera2Pswitch.TabIndex = 5;
            this.Camera2Pswitch.TabStop = false;
            this.Camera2Pswitch.Text = "Camera / 2-photon switch";
            // 
            // TwoPhotonRadio
            // 
            this.TwoPhotonRadio.AutoSize = true;
            this.TwoPhotonRadio.Location = new System.Drawing.Point(83, 36);
            this.TwoPhotonRadio.Name = "TwoPhotonRadio";
            this.TwoPhotonRadio.Size = new System.Drawing.Size(67, 17);
            this.TwoPhotonRadio.TabIndex = 1;
            this.TwoPhotonRadio.TabStop = true;
            this.TwoPhotonRadio.Text = "2-photon";
            this.TwoPhotonRadio.UseVisualStyleBackColor = true;
            this.TwoPhotonRadio.Click += new System.EventHandler(this.Camera2PSwitchRadioClick);
            // 
            // CameraRadio
            // 
            this.CameraRadio.AutoSize = true;
            this.CameraRadio.Location = new System.Drawing.Point(6, 36);
            this.CameraRadio.Name = "CameraRadio";
            this.CameraRadio.Size = new System.Drawing.Size(61, 17);
            this.CameraRadio.TabIndex = 0;
            this.CameraRadio.TabStop = true;
            this.CameraRadio.Text = "Camera";
            this.CameraRadio.UseVisualStyleBackColor = true;
            this.CameraRadio.Click += new System.EventHandler(this.Camera2PSwitchRadioClick);
            // 
            // PMTControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(276, 274);
            this.Controls.Add(this.Camera2Pswitch);
            this.Controls.Add(this.PMTPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PMTControl";
            this.Text = "PMT Contoller";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PMTControl_FormClosing);
            this.Load += new System.EventHandler(this.PMTControl_Load);
            this.PMTPanel.ResumeLayout(false);
            this.PMTPanel.PerformLayout();
            this.Camera2Pswitch.ResumeLayout(false);
            this.Camera2Pswitch.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TextBox PMTGain1;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.GroupBox PMTPanel;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.CheckBox PMTEnableCB;
        public System.Windows.Forms.GroupBox Camera2Pswitch;
        public System.Windows.Forms.RadioButton TwoPhotonRadio;
        public System.Windows.Forms.RadioButton CameraRadio;
        public System.Windows.Forms.TextBox PMTGain2;
        public System.Windows.Forms.Label PMT2Status;
        public System.Windows.Forms.Label PMT1Status;
    }
}

