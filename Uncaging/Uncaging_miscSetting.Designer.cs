namespace FLIMimage
{
    partial class Uncaging_miscSetting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Uncaging_miscSetting));
            this.TurnOffImagingDuringUncagingCheck = new System.Windows.Forms.CheckBox();
            this.MoveMirrosDuringUncagingPosCheck = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.MirrorDelay = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // TurnOffImagingDuringUncagingCheck
            // 
            this.TurnOffImagingDuringUncagingCheck.AutoSize = true;
            this.TurnOffImagingDuringUncagingCheck.Checked = true;
            this.TurnOffImagingDuringUncagingCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TurnOffImagingDuringUncagingCheck.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TurnOffImagingDuringUncagingCheck.Location = new System.Drawing.Point(38, 52);
            this.TurnOffImagingDuringUncagingCheck.Name = "TurnOffImagingDuringUncagingCheck";
            this.TurnOffImagingDuringUncagingCheck.Size = new System.Drawing.Size(211, 18);
            this.TurnOffImagingDuringUncagingCheck.TabIndex = 403;
            this.TurnOffImagingDuringUncagingCheck.Text = "Turn off imaging laser during uncaging";
            this.TurnOffImagingDuringUncagingCheck.UseVisualStyleBackColor = true;
            this.TurnOffImagingDuringUncagingCheck.Click += new System.EventHandler(this.Checkbox_Clicked);
            // 
            // MoveMirrosDuringUncagingPosCheck
            // 
            this.MoveMirrosDuringUncagingPosCheck.AutoSize = true;
            this.MoveMirrosDuringUncagingPosCheck.Checked = true;
            this.MoveMirrosDuringUncagingPosCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MoveMirrosDuringUncagingPosCheck.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MoveMirrosDuringUncagingPosCheck.Location = new System.Drawing.Point(38, 21);
            this.MoveMirrosDuringUncagingPosCheck.Name = "MoveMirrosDuringUncagingPosCheck";
            this.MoveMirrosDuringUncagingPosCheck.Size = new System.Drawing.Size(306, 18);
            this.MoveMirrosDuringUncagingPosCheck.TabIndex = 402;
            this.MoveMirrosDuringUncagingPosCheck.Text = " Uncage position used for \"uncaging during imaging\" mode";
            this.MoveMirrosDuringUncagingPosCheck.UseVisualStyleBackColor = true;
            this.MoveMirrosDuringUncagingPosCheck.Click += new System.EventHandler(this.Checkbox_Clicked);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(188, 119);
            this.label13.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(21, 14);
            this.label13.TabIndex = 407;
            this.label13.Text = "ms";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(119, 101);
            this.label16.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(216, 14);
            this.label16.TabIndex = 406;
            this.label16.Text = "Scan Mirror (for \"Uncaging during imaging\")";
            // 
            // MirrorDelay
            // 
            this.MirrorDelay.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MirrorDelay.Location = new System.Drawing.Point(135, 116);
            this.MirrorDelay.Margin = new System.Windows.Forms.Padding(1);
            this.MirrorDelay.Name = "MirrorDelay";
            this.MirrorDelay.Size = new System.Drawing.Size(50, 20);
            this.MirrorDelay.TabIndex = 405;
            this.MirrorDelay.Text = "0";
            this.MirrorDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.MirrorDelay.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MirrorDelay_KeyDown);
            // 
            // Uncaging_miscSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 156);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.MirrorDelay);
            this.Controls.Add(this.TurnOffImagingDuringUncagingCheck);
            this.Controls.Add(this.MoveMirrosDuringUncagingPosCheck);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Uncaging_miscSetting";
            this.Text = "Uncaging_miscSetting";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox TurnOffImagingDuringUncagingCheck;
        private System.Windows.Forms.CheckBox MoveMirrosDuringUncagingPosCheck;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox MirrorDelay;
    }
}