namespace FLIMage.Dialogs
{
    partial class ExportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportForm));
            this.MaxProc = new System.Windows.Forms.RadioButton();
            this.SumProc = new System.Windows.Forms.RadioButton();
            this.SaveButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SaveChannel4 = new System.Windows.Forms.CheckBox();
            this.SaveChannel3 = new System.Windows.Forms.CheckBox();
            this.SaveChannel2 = new System.Windows.Forms.CheckBox();
            this.SaveChannel1 = new System.Windows.Forms.CheckBox();
            this.ProjectionBox = new System.Windows.Forms.GroupBox();
            this.AllFiles = new System.Windows.Forms.CheckBox();
            this.FastZGroup = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.LastSlice = new System.Windows.Forms.TextBox();
            this.StartSlice = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Row = new System.Windows.Forms.TextBox();
            this.Column = new System.Windows.Forms.TextBox();
            this.CorrectT0EachPageCheck = new System.Windows.Forms.CheckBox();
            this.ProjectionTypePanel = new System.Windows.Forms.Panel();
            this.NoProjection = new System.Windows.Forms.RadioButton();
            this.ZProjectionCheckBox = new System.Windows.Forms.RadioButton();
            this.Message = new System.Windows.Forms.Label();
            this.Message_Projection = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.ProjectionBox.SuspendLayout();
            this.FastZGroup.SuspendLayout();
            this.ProjectionTypePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MaxProc
            // 
            this.MaxProc.AutoSize = true;
            this.MaxProc.Checked = true;
            this.MaxProc.Location = new System.Drawing.Point(19, 9);
            this.MaxProc.Name = "MaxProc";
            this.MaxProc.Size = new System.Drawing.Size(45, 17);
            this.MaxProc.TabIndex = 6;
            this.MaxProc.TabStop = true;
            this.MaxProc.Text = "Max";
            this.MaxProc.UseVisualStyleBackColor = true;
            // 
            // SumProc
            // 
            this.SumProc.AutoSize = true;
            this.SumProc.Location = new System.Drawing.Point(19, 31);
            this.SumProc.Name = "SumProc";
            this.SumProc.Size = new System.Drawing.Size(46, 17);
            this.SumProc.TabIndex = 7;
            this.SumProc.Text = "Sum";
            this.SumProc.UseVisualStyleBackColor = true;
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(205, 295);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(94, 27);
            this.SaveButton.TabIndex = 9;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Message);
            this.groupBox1.Controls.Add(this.SaveChannel4);
            this.groupBox1.Controls.Add(this.SaveChannel3);
            this.groupBox1.Controls.Add(this.SaveChannel2);
            this.groupBox1.Controls.Add(this.SaveChannel1);
            this.groupBox1.Location = new System.Drawing.Point(9, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(226, 75);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Save Channel";
            // 
            // SaveChannel4
            // 
            this.SaveChannel4.AutoSize = true;
            this.SaveChannel4.Location = new System.Drawing.Point(133, 37);
            this.SaveChannel4.Name = "SaveChannel4";
            this.SaveChannel4.Size = new System.Drawing.Size(74, 17);
            this.SaveChannel4.TabIndex = 17;
            this.SaveChannel4.Text = "Channel 4";
            this.SaveChannel4.UseVisualStyleBackColor = true;
            // 
            // SaveChannel3
            // 
            this.SaveChannel3.AutoSize = true;
            this.SaveChannel3.Location = new System.Drawing.Point(133, 19);
            this.SaveChannel3.Name = "SaveChannel3";
            this.SaveChannel3.Size = new System.Drawing.Size(74, 17);
            this.SaveChannel3.TabIndex = 16;
            this.SaveChannel3.Text = "Channel 3";
            this.SaveChannel3.UseVisualStyleBackColor = true;
            // 
            // SaveChannel2
            // 
            this.SaveChannel2.AutoSize = true;
            this.SaveChannel2.Location = new System.Drawing.Point(25, 37);
            this.SaveChannel2.Name = "SaveChannel2";
            this.SaveChannel2.Size = new System.Drawing.Size(74, 17);
            this.SaveChannel2.TabIndex = 15;
            this.SaveChannel2.Text = "Channel 2";
            this.SaveChannel2.UseVisualStyleBackColor = true;
            // 
            // SaveChannel1
            // 
            this.SaveChannel1.AutoSize = true;
            this.SaveChannel1.Location = new System.Drawing.Point(25, 19);
            this.SaveChannel1.Name = "SaveChannel1";
            this.SaveChannel1.Size = new System.Drawing.Size(74, 17);
            this.SaveChannel1.TabIndex = 14;
            this.SaveChannel1.Text = "Channel 1";
            this.SaveChannel1.UseVisualStyleBackColor = true;
            // 
            // ProjectionBox
            // 
            this.ProjectionBox.Controls.Add(this.Message_Projection);
            this.ProjectionBox.Controls.Add(this.ZProjectionCheckBox);
            this.ProjectionBox.Controls.Add(this.NoProjection);
            this.ProjectionBox.Controls.Add(this.ProjectionTypePanel);
            this.ProjectionBox.Location = new System.Drawing.Point(9, 93);
            this.ProjectionBox.Name = "ProjectionBox";
            this.ProjectionBox.Size = new System.Drawing.Size(226, 81);
            this.ProjectionBox.TabIndex = 13;
            this.ProjectionBox.TabStop = false;
            this.ProjectionBox.Text = "Projection";
            // 
            // AllFiles
            // 
            this.AllFiles.AutoSize = true;
            this.AllFiles.Location = new System.Drawing.Point(12, 269);
            this.AllFiles.Name = "AllFiles";
            this.AllFiles.Size = new System.Drawing.Size(178, 17);
            this.AllFiles.TabIndex = 8;
            this.AllFiles.Text = "All files with the same basename";
            this.AllFiles.UseVisualStyleBackColor = true;
            // 
            // FastZGroup
            // 
            this.FastZGroup.Controls.Add(this.label3);
            this.FastZGroup.Controls.Add(this.label4);
            this.FastZGroup.Controls.Add(this.LastSlice);
            this.FastZGroup.Controls.Add(this.StartSlice);
            this.FastZGroup.Controls.Add(this.label2);
            this.FastZGroup.Controls.Add(this.label1);
            this.FastZGroup.Controls.Add(this.Row);
            this.FastZGroup.Controls.Add(this.Column);
            this.FastZGroup.Location = new System.Drawing.Point(9, 182);
            this.FastZGroup.Name = "FastZGroup";
            this.FastZGroup.Size = new System.Drawing.Size(260, 81);
            this.FastZGroup.TabIndex = 14;
            this.FastZGroup.TabStop = false;
            this.FastZGroup.Text = "Fast Z Slices";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(132, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Last slice";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(132, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Start slice";
            // 
            // LastSlice
            // 
            this.LastSlice.Location = new System.Drawing.Point(189, 45);
            this.LastSlice.Name = "LastSlice";
            this.LastSlice.Size = new System.Drawing.Size(50, 20);
            this.LastSlice.TabIndex = 5;
            // 
            // StartSlice
            // 
            this.StartSlice.Location = new System.Drawing.Point(189, 19);
            this.StartSlice.Name = "StartSlice";
            this.StartSlice.Size = new System.Drawing.Size(50, 20);
            this.StartSlice.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Row";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Column";
            // 
            // Row
            // 
            this.Row.Location = new System.Drawing.Point(58, 45);
            this.Row.Name = "Row";
            this.Row.Size = new System.Drawing.Size(50, 20);
            this.Row.TabIndex = 1;
            // 
            // Column
            // 
            this.Column.Location = new System.Drawing.Point(58, 19);
            this.Column.Name = "Column";
            this.Column.Size = new System.Drawing.Size(50, 20);
            this.Column.TabIndex = 0;
            // 
            // CorrectT0EachPageCheck
            // 
            this.CorrectT0EachPageCheck.AutoSize = true;
            this.CorrectT0EachPageCheck.Checked = true;
            this.CorrectT0EachPageCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CorrectT0EachPageCheck.Location = new System.Drawing.Point(12, 290);
            this.CorrectT0EachPageCheck.Name = "CorrectT0EachPageCheck";
            this.CorrectT0EachPageCheck.Size = new System.Drawing.Size(145, 17);
            this.CorrectT0EachPageCheck.TabIndex = 15;
            this.CorrectT0EachPageCheck.Text = "Correct T0 for each page";
            this.CorrectT0EachPageCheck.UseVisualStyleBackColor = true;
            // 
            // ProjectionTypePanel
            // 
            this.ProjectionTypePanel.Controls.Add(this.MaxProc);
            this.ProjectionTypePanel.Controls.Add(this.SumProc);
            this.ProjectionTypePanel.Location = new System.Drawing.Point(135, 20);
            this.ProjectionTypePanel.Name = "ProjectionTypePanel";
            this.ProjectionTypePanel.Size = new System.Drawing.Size(85, 56);
            this.ProjectionTypePanel.TabIndex = 16;
            // 
            // NoProjection
            // 
            this.NoProjection.AutoSize = true;
            this.NoProjection.Checked = true;
            this.NoProjection.Location = new System.Drawing.Point(14, 21);
            this.NoProjection.Name = "NoProjection";
            this.NoProjection.Size = new System.Drawing.Size(105, 17);
            this.NoProjection.TabIndex = 17;
            this.NoProjection.TabStop = true;
            this.NoProjection.Text = "All pages / slices";
            this.NoProjection.UseVisualStyleBackColor = true;
            // 
            // ZProjectionCheckBox
            // 
            this.ZProjectionCheckBox.AutoSize = true;
            this.ZProjectionCheckBox.Location = new System.Drawing.Point(14, 41);
            this.ZProjectionCheckBox.Name = "ZProjectionCheckBox";
            this.ZProjectionCheckBox.Size = new System.Drawing.Size(81, 17);
            this.ZProjectionCheckBox.TabIndex = 18;
            this.ZProjectionCheckBox.Text = "Z projection";
            this.ZProjectionCheckBox.UseVisualStyleBackColor = true;
            this.ZProjectionCheckBox.CheckedChanged += new System.EventHandler(this.ZProjectionCheckBox_CheckedChanged);
            // 
            // Message
            // 
            this.Message.AutoSize = true;
            this.Message.ForeColor = System.Drawing.Color.Red;
            this.Message.Location = new System.Drawing.Point(6, 59);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(0, 13);
            this.Message.TabIndex = 16;
            // 
            // Message_Projection
            // 
            this.Message_Projection.AutoSize = true;
            this.Message_Projection.ForeColor = System.Drawing.Color.Red;
            this.Message_Projection.Location = new System.Drawing.Point(11, 63);
            this.Message_Projection.Name = "Message_Projection";
            this.Message_Projection.Size = new System.Drawing.Size(0, 13);
            this.Message_Projection.TabIndex = 18;
            // 
            // ExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(313, 337);
            this.Controls.Add(this.CorrectT0EachPageCheck);
            this.Controls.Add(this.FastZGroup);
            this.Controls.Add(this.AllFiles);
            this.Controls.Add(this.ProjectionBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.SaveButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExportForm";
            this.Text = "Export file in TIFF ...";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExportForm_FormClosing);
            this.Load += new System.EventHandler(this.ExportForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ProjectionBox.ResumeLayout(false);
            this.ProjectionBox.PerformLayout();
            this.FastZGroup.ResumeLayout(false);
            this.FastZGroup.PerformLayout();
            this.ProjectionTypePanel.ResumeLayout(false);
            this.ProjectionTypePanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RadioButton MaxProc;
        private System.Windows.Forms.RadioButton SumProc;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox ProjectionBox;
        private System.Windows.Forms.CheckBox AllFiles;
        private System.Windows.Forms.CheckBox SaveChannel2;
        private System.Windows.Forms.CheckBox SaveChannel1;
        private System.Windows.Forms.CheckBox SaveChannel4;
        private System.Windows.Forms.CheckBox SaveChannel3;
        private System.Windows.Forms.GroupBox FastZGroup;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Row;
        private System.Windows.Forms.TextBox Column;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox LastSlice;
        private System.Windows.Forms.TextBox StartSlice;
        private System.Windows.Forms.CheckBox CorrectT0EachPageCheck;
        private System.Windows.Forms.RadioButton ZProjectionCheckBox;
        private System.Windows.Forms.RadioButton NoProjection;
        private System.Windows.Forms.Panel ProjectionTypePanel;
        private System.Windows.Forms.Label Message;
        private System.Windows.Forms.Label Message_Projection;
    }
}