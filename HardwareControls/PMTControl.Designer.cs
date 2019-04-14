namespace ThorlabController
{
    partial class PMTControl
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
            this.PortName = new System.Windows.Forms.TextBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.ComportPulldown = new System.Windows.Forms.ComboBox();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.PMTPanel.SuspendLayout();
            this.Camera2Pswitch.SuspendLayout();
            this.SuspendLayout();
            // 
            // PMTGain1
            // 
            this.PMTGain1.Location = new System.Drawing.Point(106, 24);
            this.PMTGain1.Name = "PMTGain1";
            this.PMTGain1.Size = new System.Drawing.Size(44, 20);
            this.PMTGain1.TabIndex = 0;
            this.PMTGain1.Text = "80";
            this.PMTGain1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "PMT Gain 1";
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
            this.PMTPanel.Size = new System.Drawing.Size(183, 151);
            this.PMTPanel.TabIndex = 4;
            this.PMTPanel.TabStop = false;
            this.PMTPanel.Text = "PMT Control";
            // 
            // PMT2Status
            // 
            this.PMT2Status.AutoSize = true;
            this.PMT2Status.Location = new System.Drawing.Point(40, 95);
            this.PMT2Status.Name = "PMT2Status";
            this.PMT2Status.Size = new System.Drawing.Size(79, 13);
            this.PMT2Status.TabIndex = 7;
            this.PMT2Status.Text = "PMT1 Gain: 80";
            // 
            // PMT1Status
            // 
            this.PMT1Status.AutoSize = true;
            this.PMT1Status.Location = new System.Drawing.Point(40, 47);
            this.PMT1Status.Name = "PMT1Status";
            this.PMT1Status.Size = new System.Drawing.Size(79, 13);
            this.PMT1Status.TabIndex = 6;
            this.PMT1Status.Text = "PMT1 Gain: 80";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "PMT Gain 2";
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
            this.PMTGain2.Location = new System.Drawing.Point(106, 72);
            this.PMTGain2.Name = "PMTGain2";
            this.PMTGain2.Size = new System.Drawing.Size(44, 20);
            this.PMTGain2.TabIndex = 1;
            this.PMTGain2.Text = "80";
            this.PMTGain2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Camera2Pswitch
            // 
            this.Camera2Pswitch.Controls.Add(this.TwoPhotonRadio);
            this.Camera2Pswitch.Controls.Add(this.CameraRadio);
            this.Camera2Pswitch.Enabled = false;
            this.Camera2Pswitch.Location = new System.Drawing.Point(12, 179);
            this.Camera2Pswitch.Name = "Camera2Pswitch";
            this.Camera2Pswitch.Size = new System.Drawing.Size(183, 77);
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
            // 
            // PortName
            // 
            this.PortName.Location = new System.Drawing.Point(201, 32);
            this.PortName.Name = "PortName";
            this.PortName.Size = new System.Drawing.Size(71, 20);
            this.PortName.TabIndex = 8;
            this.PortName.Text = "COM1";
            this.PortName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(201, 59);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(71, 23);
            this.ConnectButton.TabIndex = 9;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // ComportPulldown
            // 
            this.ComportPulldown.FormattingEnabled = true;
            this.ComportPulldown.Location = new System.Drawing.Point(201, 88);
            this.ComportPulldown.Name = "ComportPulldown";
            this.ComportPulldown.Size = new System.Drawing.Size(71, 21);
            this.ComportPulldown.TabIndex = 10;
            this.ComportPulldown.SelectedIndexChanged += new System.EventHandler(this.ComportPulldown_SelectedIndexChanged);
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(203, 133);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(79, 13);
            this.StatusLabel.TabIndex = 8;
            this.StatusLabel.Text = "Not Connected";
            // 
            // PMTControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(296, 274);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.ComportPulldown);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.PortName);
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
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox PMTGain1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox PMTPanel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox PMTEnableCB;
        private System.Windows.Forms.GroupBox Camera2Pswitch;
        private System.Windows.Forms.RadioButton TwoPhotonRadio;
        private System.Windows.Forms.RadioButton CameraRadio;
        private System.Windows.Forms.TextBox PMTGain2;
        private System.Windows.Forms.Label PMT2Status;
        private System.Windows.Forms.Label PMT1Status;
        private System.Windows.Forms.TextBox PortName;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.ComboBox ComportPulldown;
        private System.Windows.Forms.Label StatusLabel;
    }
}

