namespace FLIMimage
{
    partial class Script
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Script));
            this.FLIMageMessageWindow = new System.Windows.Forms.TextBox();
            this.Execute = new System.Windows.Forms.Button();
            this.ClientMessageWindow = new System.Windows.Forms.TextBox();
            this.EventReceived = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ServerOn = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ServerStatus_FLIMage = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.ServerStatus_Client = new System.Windows.Forms.Label();
            this.Connect_With_Text = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.FLIMageFileName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.CommandFilePath = new System.Windows.Forms.TextBox();
            this.ClientFileName = new System.Windows.Forms.TextBox();
            this.SetPath = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SetFlimageOutputFileName = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.LoadNotifyLIstButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // FLIMageMessageWindow
            // 
            this.FLIMageMessageWindow.Location = new System.Drawing.Point(17, 22);
            this.FLIMageMessageWindow.Multiline = true;
            this.FLIMageMessageWindow.Name = "FLIMageMessageWindow";
            this.FLIMageMessageWindow.Size = new System.Drawing.Size(561, 67);
            this.FLIMageMessageWindow.TabIndex = 0;
            this.FLIMageMessageWindow.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.FLIMageMessageWindow.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // Execute
            // 
            this.Execute.Location = new System.Drawing.Point(517, 194);
            this.Execute.Name = "Execute";
            this.Execute.Size = new System.Drawing.Size(61, 23);
            this.Execute.TabIndex = 1;
            this.Execute.Text = "Execute";
            this.Execute.UseVisualStyleBackColor = true;
            this.Execute.Click += new System.EventHandler(this.Execute_Click);
            // 
            // ClientMessageWindow
            // 
            this.ClientMessageWindow.Location = new System.Drawing.Point(17, 112);
            this.ClientMessageWindow.Multiline = true;
            this.ClientMessageWindow.Name = "ClientMessageWindow";
            this.ClientMessageWindow.Size = new System.Drawing.Size(561, 67);
            this.ClientMessageWindow.TabIndex = 2;
            // 
            // EventReceived
            // 
            this.EventReceived.AutoSize = true;
            this.EventReceived.Location = new System.Drawing.Point(105, 200);
            this.EventReceived.Name = "EventReceived";
            this.EventReceived.Size = new System.Drawing.Size(35, 13);
            this.EventReceived.TabIndex = 3;
            this.EventReceived.Text = "label1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "FLIMage output";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 98);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Client output";
            // 
            // ServerOn
            // 
            this.ServerOn.AutoSize = true;
            this.ServerOn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ServerOn.Location = new System.Drawing.Point(359, 14);
            this.ServerOn.Name = "ServerOn";
            this.ServerOn.Size = new System.Drawing.Size(182, 17);
            this.ServerOn.TabIndex = 275;
            this.ServerOn.Text = "Connect with client through PIPE";
            this.ServerOn.UseVisualStyleBackColor = true;
            this.ServerOn.Click += new System.EventHandler(this.ServerOn_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(5, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 13);
            this.label3.TabIndex = 276;
            this.label3.Text = "PIPE server status:";
            // 
            // ServerStatus_FLIMage
            // 
            this.ServerStatus_FLIMage.AutoSize = true;
            this.ServerStatus_FLIMage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ServerStatus_FLIMage.Location = new System.Drawing.Point(99, 18);
            this.ServerStatus_FLIMage.Name = "ServerStatus_FLIMage";
            this.ServerStatus_FLIMage.Size = new System.Drawing.Size(71, 13);
            this.ServerStatus_FLIMage.TabIndex = 277;
            this.ServerStatus_FLIMage.Text = "Server Status";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 200);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 278;
            this.label5.Text = "FLIMage event:";
            // 
            // ServerStatus_Client
            // 
            this.ServerStatus_Client.AutoSize = true;
            this.ServerStatus_Client.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ServerStatus_Client.Location = new System.Drawing.Point(99, 32);
            this.ServerStatus_Client.Name = "ServerStatus_Client";
            this.ServerStatus_Client.Size = new System.Drawing.Size(71, 13);
            this.ServerStatus_Client.TabIndex = 279;
            this.ServerStatus_Client.Text = "Server Status";
            // 
            // Connect_With_Text
            // 
            this.Connect_With_Text.AutoSize = true;
            this.Connect_With_Text.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Connect_With_Text.Location = new System.Drawing.Point(359, 11);
            this.Connect_With_Text.Name = "Connect_With_Text";
            this.Connect_With_Text.Size = new System.Drawing.Size(174, 17);
            this.Connect_With_Text.TabIndex = 280;
            this.Connect_With_Text.Text = "Connect with client through File";
            this.Connect_With_Text.UseVisualStyleBackColor = true;
            this.Connect_With_Text.Click += new System.EventHandler(this.Connect_With_Text_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 281;
            this.label4.Text = "Directory:";
            // 
            // FLIMageFileName
            // 
            this.FLIMageFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FLIMageFileName.Location = new System.Drawing.Point(11, 75);
            this.FLIMageFileName.Multiline = true;
            this.FLIMageFileName.Name = "FLIMageFileName";
            this.FLIMageFileName.Size = new System.Drawing.Size(186, 21);
            this.FLIMageFileName.TabIndex = 283;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(12, 60);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(130, 13);
            this.label6.TabIndex = 285;
            this.label6.Text = "FLIMage output file name:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(310, 60);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(114, 13);
            this.label7.TabIndex = 286;
            this.label7.Text = "Client output file name:";
            // 
            // CommandFilePath
            // 
            this.CommandFilePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CommandFilePath.Location = new System.Drawing.Point(11, 36);
            this.CommandFilePath.Multiline = true;
            this.CommandFilePath.Name = "CommandFilePath";
            this.CommandFilePath.Size = new System.Drawing.Size(555, 21);
            this.CommandFilePath.TabIndex = 282;
            // 
            // ClientFileName
            // 
            this.ClientFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClientFileName.Location = new System.Drawing.Point(308, 75);
            this.ClientFileName.Multiline = true;
            this.ClientFileName.Name = "ClientFileName";
            this.ClientFileName.Size = new System.Drawing.Size(181, 21);
            this.ClientFileName.TabIndex = 284;
            // 
            // SetPath
            // 
            this.SetPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SetPath.Location = new System.Drawing.Point(493, 74);
            this.SetPath.Name = "SetPath";
            this.SetPath.Size = new System.Drawing.Size(75, 23);
            this.SetPath.TabIndex = 287;
            this.SetPath.Text = "Set path...";
            this.SetPath.UseVisualStyleBackColor = true;
            this.SetPath.Click += new System.EventHandler(this.SetPath_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.SetFlimageOutputFileName);
            this.groupBox1.Controls.Add(this.SetPath);
            this.groupBox1.Controls.Add(this.ClientFileName);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.FLIMageFileName);
            this.groupBox1.Controls.Add(this.CommandFilePath);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.Connect_With_Text);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 223);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(572, 104);
            this.groupBox1.TabIndex = 288;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "File-based connection";
            // 
            // SetFlimageOutputFileName
            // 
            this.SetFlimageOutputFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SetFlimageOutputFileName.Location = new System.Drawing.Point(200, 74);
            this.SetFlimageOutputFileName.Name = "SetFlimageOutputFileName";
            this.SetFlimageOutputFileName.Size = new System.Drawing.Size(80, 23);
            this.SetFlimageOutputFileName.TabIndex = 288;
            this.SetFlimageOutputFileName.Text = "Set path...";
            this.SetFlimageOutputFileName.UseVisualStyleBackColor = true;
            this.SetFlimageOutputFileName.Click += new System.EventHandler(this.SetFlimageOutputFileName_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ServerStatus_Client);
            this.groupBox2.Controls.Add(this.ServerStatus_FLIMage);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.ServerOn);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(12, 345);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(572, 51);
            this.groupBox2.TabIndex = 289;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "PIPE-based connection";
            // 
            // LoadNotifyLIstButton
            // 
            this.LoadNotifyLIstButton.Location = new System.Drawing.Point(491, 402);
            this.LoadNotifyLIstButton.Name = "LoadNotifyLIstButton";
            this.LoadNotifyLIstButton.Size = new System.Drawing.Size(93, 23);
            this.LoadNotifyLIstButton.TabIndex = 290;
            this.LoadNotifyLIstButton.Text = "Notify List";
            this.LoadNotifyLIstButton.UseVisualStyleBackColor = true;
            this.LoadNotifyLIstButton.Click += new System.EventHandler(this.LoadNotifyLIstButton_Click);
            // 
            // Script
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 434);
            this.Controls.Add(this.LoadNotifyLIstButton);
            this.Controls.Add(this.Execute);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.EventReceived);
            this.Controls.Add(this.ClientMessageWindow);
            this.Controls.Add(this.FLIMageMessageWindow);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Script";
            this.Text = "Remote control & script";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Script_FormClosing);
            this.Load += new System.EventHandler(this.Script_Load);
            this.Shown += new System.EventHandler(this.Script_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox FLIMageMessageWindow;
        private System.Windows.Forms.Button Execute;
        private System.Windows.Forms.TextBox ClientMessageWindow;
        private System.Windows.Forms.Label EventReceived;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox ServerOn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label ServerStatus_FLIMage;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label ServerStatus_Client;
        private System.Windows.Forms.CheckBox Connect_With_Text;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox FLIMageFileName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox CommandFilePath;
        private System.Windows.Forms.TextBox ClientFileName;
        private System.Windows.Forms.Button SetPath;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button SetFlimageOutputFileName;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button LoadNotifyLIstButton;
    }
}