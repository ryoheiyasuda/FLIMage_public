namespace PhysiologyCSharp
{
    partial class PhysAnalysis
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
            this.PlotPicBox = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileSeriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveThisProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileNumberBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BaseStartBox = new System.Windows.Forms.TextBox();
            this.BaseEndBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SignalEndBox = new System.Windows.Forms.TextBox();
            this.SignalStartBox = new System.Windows.Forms.TextBox();
            this.slopeRadio = new System.Windows.Forms.RadioButton();
            this.averageRadio = new System.Windows.Forms.RadioButton();
            this.ClculateButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.FileUpButton = new System.Windows.Forms.Button();
            this.FileDOwnButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.ValueLabel = new System.Windows.Forms.Label();
            this.ValueBox = new System.Windows.Forms.TextBox();
            this.DisplayPanel = new System.Windows.Forms.Panel();
            this.ChRadio2 = new System.Windows.Forms.RadioButton();
            this.ChRadio1 = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.FileNumberLabel = new System.Windows.Forms.Label();
            this.ExportAverage = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.PlotPicBox)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.DisplayPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // PlotPicBox
            // 
            this.PlotPicBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PlotPicBox.BackColor = System.Drawing.Color.White;
            this.PlotPicBox.Location = new System.Drawing.Point(-2, 116);
            this.PlotPicBox.Margin = new System.Windows.Forms.Padding(2);
            this.PlotPicBox.Name = "PlotPicBox";
            this.PlotPicBox.Size = new System.Drawing.Size(620, 303);
            this.PlotPicBox.TabIndex = 0;
            this.PlotPicBox.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(616, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileSeriesToolStripMenuItem,
            this.openProjectToolStripMenuItem,
            this.saveThisProjectToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openFileSeriesToolStripMenuItem
            // 
            this.openFileSeriesToolStripMenuItem.Name = "openFileSeriesToolStripMenuItem";
            this.openFileSeriesToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.openFileSeriesToolStripMenuItem.Text = "Open file seriese";
            this.openFileSeriesToolStripMenuItem.Click += new System.EventHandler(this.openFileSeriesToolStripMenuItem_Click);
            // 
            // openProjectToolStripMenuItem
            // 
            this.openProjectToolStripMenuItem.Name = "openProjectToolStripMenuItem";
            this.openProjectToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.openProjectToolStripMenuItem.Text = "Open project";
            this.openProjectToolStripMenuItem.Click += new System.EventHandler(this.openProjectToolStripMenuItem_Click);
            // 
            // saveThisProjectToolStripMenuItem
            // 
            this.saveThisProjectToolStripMenuItem.Name = "saveThisProjectToolStripMenuItem";
            this.saveThisProjectToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.saveThisProjectToolStripMenuItem.Text = "Save project";
            this.saveThisProjectToolStripMenuItem.Click += new System.EventHandler(this.saveThisProjectToolStripMenuItem_Click);
            // 
            // fileNumberBox
            // 
            this.fileNumberBox.Location = new System.Drawing.Point(38, 27);
            this.fileNumberBox.Margin = new System.Windows.Forms.Padding(2);
            this.fileNumberBox.Name = "fileNumberBox";
            this.fileNumberBox.Size = new System.Drawing.Size(319, 20);
            this.fileNumberBox.TabIndex = 2;
            this.fileNumberBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fileNumberBox_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 29);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "File#";
            // 
            // BaseStartBox
            // 
            this.BaseStartBox.Location = new System.Drawing.Point(83, 48);
            this.BaseStartBox.Margin = new System.Windows.Forms.Padding(2);
            this.BaseStartBox.Name = "BaseStartBox";
            this.BaseStartBox.Size = new System.Drawing.Size(32, 20);
            this.BaseStartBox.TabIndex = 4;
            this.BaseStartBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RangeBoxKeyDown);
            // 
            // BaseEndBox
            // 
            this.BaseEndBox.Location = new System.Drawing.Point(83, 64);
            this.BaseEndBox.Margin = new System.Windows.Forms.Padding(2);
            this.BaseEndBox.Name = "BaseEndBox";
            this.BaseEndBox.Size = new System.Drawing.Size(32, 20);
            this.BaseEndBox.TabIndex = 5;
            this.BaseEndBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RangeBoxKeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 49);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Baseline from";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(60, 66);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "to";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(181, 66);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(16, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "to";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(137, 49);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Signal from";
            // 
            // SignalEndBox
            // 
            this.SignalEndBox.Location = new System.Drawing.Point(199, 64);
            this.SignalEndBox.Margin = new System.Windows.Forms.Padding(2);
            this.SignalEndBox.Name = "SignalEndBox";
            this.SignalEndBox.Size = new System.Drawing.Size(32, 20);
            this.SignalEndBox.TabIndex = 9;
            this.SignalEndBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RangeBoxKeyDown);
            // 
            // SignalStartBox
            // 
            this.SignalStartBox.Location = new System.Drawing.Point(199, 48);
            this.SignalStartBox.Margin = new System.Windows.Forms.Padding(2);
            this.SignalStartBox.Name = "SignalStartBox";
            this.SignalStartBox.Size = new System.Drawing.Size(32, 20);
            this.SignalStartBox.TabIndex = 8;
            this.SignalStartBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RangeBoxKeyDown);
            // 
            // slopeRadio
            // 
            this.slopeRadio.AutoSize = true;
            this.slopeRadio.Location = new System.Drawing.Point(406, 60);
            this.slopeRadio.Margin = new System.Windows.Forms.Padding(2);
            this.slopeRadio.Name = "slopeRadio";
            this.slopeRadio.Size = new System.Drawing.Size(52, 17);
            this.slopeRadio.TabIndex = 12;
            this.slopeRadio.TabStop = true;
            this.slopeRadio.Text = "Slope";
            this.slopeRadio.UseVisualStyleBackColor = true;
            // 
            // averageRadio
            // 
            this.averageRadio.AutoSize = true;
            this.averageRadio.Location = new System.Drawing.Point(406, 78);
            this.averageRadio.Margin = new System.Windows.Forms.Padding(2);
            this.averageRadio.Name = "averageRadio";
            this.averageRadio.Size = new System.Drawing.Size(52, 17);
            this.averageRadio.TabIndex = 13;
            this.averageRadio.TabStop = true;
            this.averageRadio.Text = "Mean";
            this.averageRadio.UseVisualStyleBackColor = true;
            this.averageRadio.CheckedChanged += new System.EventHandler(this.averageRadio_CheckedChanged);
            // 
            // ClculateButton
            // 
            this.ClculateButton.Location = new System.Drawing.Point(533, 74);
            this.ClculateButton.Margin = new System.Windows.Forms.Padding(2);
            this.ClculateButton.Name = "ClculateButton";
            this.ClculateButton.Size = new System.Drawing.Size(70, 26);
            this.ClculateButton.TabIndex = 18;
            this.ClculateButton.Text = "Calc peak";
            this.ClculateButton.UseVisualStyleBackColor = true;
            this.ClculateButton.Click += new System.EventHandler(this.CalculateButton_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(114, 49);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(20, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "ms";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(114, 66);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(20, 13);
            this.label8.TabIndex = 20;
            this.label8.Text = "ms";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(230, 49);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(20, 13);
            this.label9.TabIndex = 21;
            this.label9.Text = "ms";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(230, 66);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(20, 13);
            this.label10.TabIndex = 22;
            this.label10.Text = "ms";
            // 
            // FileUpButton
            // 
            this.FileUpButton.Location = new System.Drawing.Point(548, 24);
            this.FileUpButton.Name = "FileUpButton";
            this.FileUpButton.Size = new System.Drawing.Size(16, 22);
            this.FileUpButton.TabIndex = 47;
            this.FileUpButton.Text = ">";
            this.FileUpButton.UseVisualStyleBackColor = true;
            this.FileUpButton.Click += new System.EventHandler(this.FileUpButton_Click);
            // 
            // FileDOwnButton
            // 
            this.FileDOwnButton.Location = new System.Drawing.Point(531, 24);
            this.FileDOwnButton.Name = "FileDOwnButton";
            this.FileDOwnButton.Size = new System.Drawing.Size(16, 22);
            this.FileDOwnButton.TabIndex = 46;
            this.FileDOwnButton.Text = "<";
            this.FileDOwnButton.UseVisualStyleBackColor = true;
            this.FileDOwnButton.Click += new System.EventHandler(this.FileDOwnButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Location = new System.Drawing.Point(531, 47);
            this.DeleteButton.Margin = new System.Windows.Forms.Padding(2);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(33, 22);
            this.DeleteButton.TabIndex = 48;
            this.DeleteButton.Text = "Del";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DelButton_Click);
            // 
            // ValueLabel
            // 
            this.ValueLabel.AutoSize = true;
            this.ValueLabel.Location = new System.Drawing.Point(258, 49);
            this.ValueLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ValueLabel.Name = "ValueLabel";
            this.ValueLabel.Size = new System.Drawing.Size(37, 13);
            this.ValueLabel.TabIndex = 49;
            this.ValueLabel.Text = "Value:";
            // 
            // ValueBox
            // 
            this.ValueBox.Location = new System.Drawing.Point(297, 48);
            this.ValueBox.Margin = new System.Windows.Forms.Padding(2);
            this.ValueBox.Name = "ValueBox";
            this.ValueBox.Size = new System.Drawing.Size(52, 20);
            this.ValueBox.TabIndex = 50;
            // 
            // DisplayPanel
            // 
            this.DisplayPanel.Controls.Add(this.ChRadio2);
            this.DisplayPanel.Controls.Add(this.ChRadio1);
            this.DisplayPanel.Location = new System.Drawing.Point(470, 49);
            this.DisplayPanel.Name = "DisplayPanel";
            this.DisplayPanel.Size = new System.Drawing.Size(55, 48);
            this.DisplayPanel.TabIndex = 51;
            // 
            // ChRadio2
            // 
            this.ChRadio2.AutoSize = true;
            this.ChRadio2.Location = new System.Drawing.Point(8, 28);
            this.ChRadio2.Name = "ChRadio2";
            this.ChRadio2.Size = new System.Drawing.Size(44, 17);
            this.ChRadio2.TabIndex = 5;
            this.ChRadio2.Text = "Ch2";
            this.ChRadio2.UseVisualStyleBackColor = true;
            // 
            // ChRadio1
            // 
            this.ChRadio1.AutoSize = true;
            this.ChRadio1.Checked = true;
            this.ChRadio1.Location = new System.Drawing.Point(8, 11);
            this.ChRadio1.Name = "ChRadio1";
            this.ChRadio1.Size = new System.Drawing.Size(44, 17);
            this.ChRadio1.TabIndex = 5;
            this.ChRadio1.TabStop = true;
            this.ChRadio1.Text = "Ch1";
            this.ChRadio1.UseVisualStyleBackColor = true;
            this.ChRadio1.CheckedChanged += new System.EventHandler(this.Ch1_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 87);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(157, 13);
            this.label6.TabIndex = 52;
            this.label6.Text = "Black = average, Red = Current";
            // 
            // FileNumberLabel
            // 
            this.FileNumberLabel.AutoSize = true;
            this.FileNumberLabel.Location = new System.Drawing.Point(577, 27);
            this.FileNumberLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.FileNumberLabel.Name = "FileNumberLabel";
            this.FileNumberLabel.Size = new System.Drawing.Size(13, 13);
            this.FileNumberLabel.TabIndex = 53;
            this.FileNumberLabel.Text = "1";
            // 
            // ExportAverage
            // 
            this.ExportAverage.Location = new System.Drawing.Point(261, 71);
            this.ExportAverage.Margin = new System.Windows.Forms.Padding(2);
            this.ExportAverage.Name = "ExportAverage";
            this.ExportAverage.Size = new System.Drawing.Size(102, 24);
            this.ExportAverage.TabIndex = 54;
            this.ExportAverage.Text = "Export average";
            this.ExportAverage.UseVisualStyleBackColor = true;
            this.ExportAverage.Click += new System.EventHandler(this.ExportAverage_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(413, 45);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(32, 13);
            this.label11.TabIndex = 55;
            this.label11.Text = "Peak";
            // 
            // PhysAnalysis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 417);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.ExportAverage);
            this.Controls.Add(this.FileNumberLabel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.DisplayPanel);
            this.Controls.Add(this.ValueBox);
            this.Controls.Add(this.ValueLabel);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.FileUpButton);
            this.Controls.Add(this.FileDOwnButton);
            this.Controls.Add(this.ClculateButton);
            this.Controls.Add(this.averageRadio);
            this.Controls.Add(this.slopeRadio);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.SignalEndBox);
            this.Controls.Add(this.SignalStartBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BaseEndBox);
            this.Controls.Add(this.BaseStartBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.fileNumberBox);
            this.Controls.Add(this.PlotPicBox);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "PhysAnalysis";
            this.Text = "FisAnalysis";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PhysAnalysis_FormClosing);
            this.Load += new System.EventHandler(this.PhysAnalysis_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PlotPicBox)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.DisplayPanel.ResumeLayout(false);
            this.DisplayPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox PlotPicBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileSeriesToolStripMenuItem;
        private System.Windows.Forms.TextBox fileNumberBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem saveThisProjectToolStripMenuItem;
        private System.Windows.Forms.TextBox BaseStartBox;
        private System.Windows.Forms.TextBox BaseEndBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox SignalEndBox;
        private System.Windows.Forms.TextBox SignalStartBox;
        private System.Windows.Forms.RadioButton slopeRadio;
        private System.Windows.Forms.RadioButton averageRadio;
        private System.Windows.Forms.ToolStripMenuItem openProjectToolStripMenuItem;
        private System.Windows.Forms.Button ClculateButton;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button FileUpButton;
        private System.Windows.Forms.Button FileDOwnButton;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Label ValueLabel;
        private System.Windows.Forms.TextBox ValueBox;
        private System.Windows.Forms.Panel DisplayPanel;
        private System.Windows.Forms.RadioButton ChRadio2;
        private System.Windows.Forms.RadioButton ChRadio1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label FileNumberLabel;
        private System.Windows.Forms.Button ExportAverage;
        private System.Windows.Forms.Label label11;
    }
}