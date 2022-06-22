namespace MotorCtrlTest
{
    partial class MotorCtrlTest
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MotorCtrlTest));
            this.MotorPositionX_TextBox = new System.Windows.Forms.TextBox();
            this.MotorPositionY_TextBox = new System.Windows.Forms.TextBox();
            this.MotorPositionZ_TextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.PositionSignalLabel = new System.Windows.Forms.Label();
            this.RelativePositionCheck = new System.Windows.Forms.CheckBox();
            this.VelocityTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.StatusSignalTextLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.ROESpeedTextBox = new System.Windows.Forms.TextBox();
            this.XYResolutionTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.StopButton = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.ZeroButton = new System.Windows.Forms.Button();
            this.ComPortPullDown = new System.Windows.Forms.ComboBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.Xup = new System.Windows.Forms.Button();
            this.Xdown = new System.Windows.Forms.Button();
            this.Ydown = new System.Windows.Forms.Button();
            this.Yup = new System.Windows.Forms.Button();
            this.Zdown = new System.Windows.Forms.Button();
            this.Zup = new System.Windows.Forms.Button();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.StepZ = new System.Windows.Forms.TextBox();
            this.StepY = new System.Windows.Forms.TextBox();
            this.StepX = new System.Windows.Forms.TextBox();
            this.HardZeroButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // MotorPositionX_TextBox
            // 
            this.MotorPositionX_TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MotorPositionX_TextBox.Location = new System.Drawing.Point(51, 53);
            this.MotorPositionX_TextBox.Name = "MotorPositionX_TextBox";
            this.MotorPositionX_TextBox.Size = new System.Drawing.Size(308, 62);
            this.MotorPositionX_TextBox.TabIndex = 0;
            this.MotorPositionX_TextBox.Text = "0";
            this.MotorPositionX_TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MotorPositionX_TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MotorPositionKeyDown);
            // 
            // MotorPositionY_TextBox
            // 
            this.MotorPositionY_TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MotorPositionY_TextBox.Location = new System.Drawing.Point(51, 137);
            this.MotorPositionY_TextBox.Name = "MotorPositionY_TextBox";
            this.MotorPositionY_TextBox.Size = new System.Drawing.Size(308, 62);
            this.MotorPositionY_TextBox.TabIndex = 1;
            this.MotorPositionY_TextBox.Text = "0";
            this.MotorPositionY_TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MotorPositionY_TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MotorPositionKeyDown);
            // 
            // MotorPositionZ_TextBox
            // 
            this.MotorPositionZ_TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MotorPositionZ_TextBox.Location = new System.Drawing.Point(51, 222);
            this.MotorPositionZ_TextBox.Name = "MotorPositionZ_TextBox";
            this.MotorPositionZ_TextBox.Size = new System.Drawing.Size(308, 62);
            this.MotorPositionZ_TextBox.TabIndex = 2;
            this.MotorPositionZ_TextBox.Text = "0";
            this.MotorPositionZ_TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MotorPositionZ_TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MotorPositionKeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 31);
            this.label1.TabIndex = 3;
            this.label1.Text = "X";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 140);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 31);
            this.label2.TabIndex = 4;
            this.label2.Text = "Y";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(11, 222);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 31);
            this.label3.TabIndex = 5;
            this.label3.Text = "Z";
            // 
            // PositionSignalLabel
            // 
            this.PositionSignalLabel.AutoSize = true;
            this.PositionSignalLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PositionSignalLabel.Location = new System.Drawing.Point(47, 452);
            this.PositionSignalLabel.Name = "PositionSignalLabel";
            this.PositionSignalLabel.Size = new System.Drawing.Size(74, 20);
            this.PositionSignalLabel.TabIndex = 6;
            this.PositionSignalLabel.Text = "Message";
            // 
            // RelativePositionCheck
            // 
            this.RelativePositionCheck.AutoSize = true;
            this.RelativePositionCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RelativePositionCheck.Location = new System.Drawing.Point(40, 14);
            this.RelativePositionCheck.Name = "RelativePositionCheck";
            this.RelativePositionCheck.Size = new System.Drawing.Size(119, 33);
            this.RelativePositionCheck.TabIndex = 7;
            this.RelativePositionCheck.Text = "Relative";
            this.RelativePositionCheck.UseVisualStyleBackColor = true;
            // 
            // VelocityTextBox
            // 
            this.VelocityTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VelocityTextBox.Location = new System.Drawing.Point(759, 71);
            this.VelocityTextBox.Name = "VelocityTextBox";
            this.VelocityTextBox.Size = new System.Drawing.Size(100, 38);
            this.VelocityTextBox.TabIndex = 8;
            this.VelocityTextBox.Text = "0";
            this.VelocityTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.VelocityTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VelocityTextBox_KeyDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(759, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(209, 25);
            this.label4.TabIndex = 9;
            this.label4.Text = "Motor speed (mm /s)";
            // 
            // StatusSignalTextLabel
            // 
            this.StatusSignalTextLabel.AutoSize = true;
            this.StatusSignalTextLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusSignalTextLabel.Location = new System.Drawing.Point(47, 472);
            this.StatusSignalTextLabel.Name = "StatusSignalTextLabel";
            this.StatusSignalTextLabel.Size = new System.Drawing.Size(74, 20);
            this.StatusSignalTextLabel.TabIndex = 10;
            this.StatusSignalTextLabel.Text = "Message";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(759, 124);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(244, 25);
            this.label5.TabIndex = 12;
            this.label5.Text = "ROE Speed (step/pulse)";
            // 
            // ROESpeedTextBox
            // 
            this.ROESpeedTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ROESpeedTextBox.Location = new System.Drawing.Point(759, 162);
            this.ROESpeedTextBox.Name = "ROESpeedTextBox";
            this.ROESpeedTextBox.Size = new System.Drawing.Size(100, 38);
            this.ROESpeedTextBox.TabIndex = 11;
            this.ROESpeedTextBox.Text = "0";
            this.ROESpeedTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ROESpeedTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ROESpeedTextBox_KeyDown);
            // 
            // XYResolutionTextBox
            // 
            this.XYResolutionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XYResolutionTextBox.Location = new System.Drawing.Point(759, 255);
            this.XYResolutionTextBox.Name = "XYResolutionTextBox";
            this.XYResolutionTextBox.Size = new System.Drawing.Size(100, 38);
            this.XYResolutionTextBox.TabIndex = 13;
            this.XYResolutionTextBox.Text = "0";
            this.XYResolutionTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(759, 215);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(190, 25);
            this.label6.TabIndex = 14;
            this.label6.Text = "XY resolution (nm)";
            // 
            // StopButton
            // 
            this.StopButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StopButton.Location = new System.Drawing.Point(448, 303);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(158, 135);
            this.StopButton.TabIndex = 15;
            this.StopButton.Text = "STOP";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox1.Location = new System.Drawing.Point(51, 323);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(131, 29);
            this.checkBox1.TabIndex = 16;
            this.checkBox1.Text = "Reverse X";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Click += new System.EventHandler(this.checkBox4_Click);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox2.Location = new System.Drawing.Point(51, 352);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(132, 29);
            this.checkBox2.TabIndex = 17;
            this.checkBox2.Text = "Reverse Y";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.Click += new System.EventHandler(this.checkBox4_Click);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox3.Location = new System.Drawing.Point(51, 381);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(130, 29);
            this.checkBox3.TabIndex = 18;
            this.checkBox3.Text = "Reverse Z";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.Click += new System.EventHandler(this.checkBox4_Click);
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox4.Location = new System.Drawing.Point(51, 410);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(123, 29);
            this.checkBox4.TabIndex = 19;
            this.checkBox4.Text = "SwitchXY";
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.Click += new System.EventHandler(this.checkBox4_Click);
            // 
            // ZeroButton
            // 
            this.ZeroButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZeroButton.Location = new System.Drawing.Point(245, 303);
            this.ZeroButton.Name = "ZeroButton";
            this.ZeroButton.Size = new System.Drawing.Size(197, 63);
            this.ZeroButton.TabIndex = 20;
            this.ZeroButton.Text = "Rel Zero";
            this.ZeroButton.UseVisualStyleBackColor = true;
            this.ZeroButton.Click += new System.EventHandler(this.ZeroButton_Click);
            // 
            // ComPortPullDown
            // 
            this.ComPortPullDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComPortPullDown.FormattingEnabled = true;
            this.ComPortPullDown.Location = new System.Drawing.Point(744, 349);
            this.ComPortPullDown.Name = "ComPortPullDown";
            this.ComPortPullDown.Size = new System.Drawing.Size(172, 39);
            this.ComPortPullDown.TabIndex = 21;
            this.ComPortPullDown.Click += new System.EventHandler(this.ComPortPullDown_Click);
            // 
            // ConnectButton
            // 
            this.ConnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConnectButton.Location = new System.Drawing.Point(744, 394);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(172, 44);
            this.ConnectButton.TabIndex = 22;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // Xup
            // 
            this.Xup.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Xup.Location = new System.Drawing.Point(362, 52);
            this.Xup.Name = "Xup";
            this.Xup.Size = new System.Drawing.Size(100, 63);
            this.Xup.TabIndex = 23;
            this.Xup.Text = "Up";
            this.Xup.UseVisualStyleBackColor = true;
            this.Xup.Click += new System.EventHandler(this.UpDownClick);
            // 
            // Xdown
            // 
            this.Xdown.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Xdown.Location = new System.Drawing.Point(468, 51);
            this.Xdown.Name = "Xdown";
            this.Xdown.Size = new System.Drawing.Size(100, 63);
            this.Xdown.TabIndex = 24;
            this.Xdown.Text = "Down";
            this.Xdown.UseVisualStyleBackColor = true;
            this.Xdown.Click += new System.EventHandler(this.UpDownClick);
            // 
            // Ydown
            // 
            this.Ydown.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Ydown.Location = new System.Drawing.Point(468, 136);
            this.Ydown.Name = "Ydown";
            this.Ydown.Size = new System.Drawing.Size(100, 63);
            this.Ydown.TabIndex = 26;
            this.Ydown.Text = "Down";
            this.Ydown.UseVisualStyleBackColor = true;
            this.Ydown.Click += new System.EventHandler(this.UpDownClick);
            // 
            // Yup
            // 
            this.Yup.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Yup.Location = new System.Drawing.Point(362, 137);
            this.Yup.Name = "Yup";
            this.Yup.Size = new System.Drawing.Size(100, 63);
            this.Yup.TabIndex = 25;
            this.Yup.Text = "Up";
            this.Yup.UseVisualStyleBackColor = true;
            this.Yup.Click += new System.EventHandler(this.UpDownClick);
            // 
            // Zdown
            // 
            this.Zdown.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Zdown.Location = new System.Drawing.Point(468, 221);
            this.Zdown.Name = "Zdown";
            this.Zdown.Size = new System.Drawing.Size(100, 63);
            this.Zdown.TabIndex = 28;
            this.Zdown.Text = "Down";
            this.Zdown.UseVisualStyleBackColor = true;
            this.Zdown.Click += new System.EventHandler(this.UpDownClick);
            // 
            // Zup
            // 
            this.Zup.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Zup.Location = new System.Drawing.Point(362, 222);
            this.Zup.Name = "Zup";
            this.Zup.Size = new System.Drawing.Size(100, 63);
            this.Zup.TabIndex = 27;
            this.Zup.Text = "Up";
            this.Zup.UseVisualStyleBackColor = true;
            this.Zup.Click += new System.EventHandler(this.UpDownClick);
            // 
            // disconnectButton
            // 
            this.disconnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.disconnectButton.Location = new System.Drawing.Point(744, 443);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(172, 44);
            this.disconnectButton.TabIndex = 29;
            this.disconnectButton.Text = "Disconnect";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // StepZ
            // 
            this.StepZ.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StepZ.Location = new System.Drawing.Point(574, 221);
            this.StepZ.Name = "StepZ";
            this.StepZ.Size = new System.Drawing.Size(101, 62);
            this.StepZ.TabIndex = 32;
            this.StepZ.Text = "1";
            this.StepZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // StepY
            // 
            this.StepY.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StepY.Location = new System.Drawing.Point(574, 136);
            this.StepY.Name = "StepY";
            this.StepY.Size = new System.Drawing.Size(101, 62);
            this.StepY.TabIndex = 31;
            this.StepY.Text = "10";
            this.StepY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // StepX
            // 
            this.StepX.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StepX.Location = new System.Drawing.Point(574, 52);
            this.StepX.Name = "StepX";
            this.StepX.Size = new System.Drawing.Size(101, 62);
            this.StepX.TabIndex = 30;
            this.StepX.Text = "10";
            this.StepX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // HardZeroButton
            // 
            this.HardZeroButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HardZeroButton.Location = new System.Drawing.Point(245, 375);
            this.HardZeroButton.Name = "HardZeroButton";
            this.HardZeroButton.Size = new System.Drawing.Size(197, 63);
            this.HardZeroButton.TabIndex = 33;
            this.HardZeroButton.Text = "Hard Zero";
            this.HardZeroButton.UseVisualStyleBackColor = true;
            this.HardZeroButton.Click += new System.EventHandler(this.HardZeroButton_Click);
            // 
            // MotorCtrlTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1027, 505);
            this.Controls.Add(this.HardZeroButton);
            this.Controls.Add(this.StepZ);
            this.Controls.Add(this.StepY);
            this.Controls.Add(this.StepX);
            this.Controls.Add(this.disconnectButton);
            this.Controls.Add(this.Zdown);
            this.Controls.Add(this.Zup);
            this.Controls.Add(this.Ydown);
            this.Controls.Add(this.Yup);
            this.Controls.Add(this.Xdown);
            this.Controls.Add(this.Xup);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.ComPortPullDown);
            this.Controls.Add(this.ZeroButton);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.XYResolutionTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ROESpeedTextBox);
            this.Controls.Add(this.StatusSignalTextLabel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.VelocityTextBox);
            this.Controls.Add(this.RelativePositionCheck);
            this.Controls.Add(this.PositionSignalLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.MotorPositionZ_TextBox);
            this.Controls.Add(this.MotorPositionY_TextBox);
            this.Controls.Add(this.MotorPositionX_TextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MotorCtrlTest";
            this.Text = "ZoZoLab Motor Controller Panel";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MotorCtrlTest_FormClosing);
            this.Load += new System.EventHandler(this.MotorCtrlTest_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox MotorPositionX_TextBox;
        private System.Windows.Forms.TextBox MotorPositionY_TextBox;
        private System.Windows.Forms.TextBox MotorPositionZ_TextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label PositionSignalLabel;
        private System.Windows.Forms.CheckBox RelativePositionCheck;
        private System.Windows.Forms.TextBox VelocityTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label StatusSignalTextLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox ROESpeedTextBox;
        private System.Windows.Forms.TextBox XYResolutionTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.Button ZeroButton;
        private System.Windows.Forms.ComboBox ComPortPullDown;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.Button Xup;
        private System.Windows.Forms.Button Xdown;
        private System.Windows.Forms.Button Ydown;
        private System.Windows.Forms.Button Yup;
        private System.Windows.Forms.Button Zdown;
        private System.Windows.Forms.Button Zup;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.TextBox StepZ;
        private System.Windows.Forms.TextBox StepY;
        private System.Windows.Forms.TextBox StepX;
        private System.Windows.Forms.Button HardZeroButton;
    }
}

