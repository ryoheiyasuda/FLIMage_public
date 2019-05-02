namespace FLIMage.HardwareControls.StageControls
{
    partial class MotorCtrlPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MotorCtrlPanel));
            this.AddPosition = new System.Windows.Forms.Button();
            this.motor_flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SelectPosition = new System.Windows.Forms.ComboBox();
            this.st_rm = new System.Windows.Forms.Label();
            this.RemoveButton = new System.Windows.Forms.Button();
            this.Clear = new System.Windows.Forms.Button();
            this.MotorX = new System.Windows.Forms.TextBox();
            this.MotorY = new System.Windows.Forms.TextBox();
            this.MotorZ = new System.Windows.Forms.TextBox();
            this.Relative = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // AddPosition
            // 
            this.AddPosition.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AddPosition.Location = new System.Drawing.Point(12, 278);
            this.AddPosition.Name = "AddPosition";
            this.AddPosition.Size = new System.Drawing.Size(113, 23);
            this.AddPosition.TabIndex = 0;
            this.AddPosition.Text = "Add Position";
            this.AddPosition.UseVisualStyleBackColor = true;
            this.AddPosition.Click += new System.EventHandler(this.AddPosition_Click);
            // 
            // motor_flowLayoutPanel
            // 
            this.motor_flowLayoutPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.motor_flowLayoutPanel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.motor_flowLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.motor_flowLayoutPanel.Name = "motor_flowLayoutPanel";
            this.motor_flowLayoutPanel.Size = new System.Drawing.Size(460, 249);
            this.motor_flowLayoutPanel.TabIndex = 1;
            // 
            // SelectPosition
            // 
            this.SelectPosition.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectPosition.FormattingEnabled = true;
            this.SelectPosition.Location = new System.Drawing.Point(238, 280);
            this.SelectPosition.Name = "SelectPosition";
            this.SelectPosition.Size = new System.Drawing.Size(121, 22);
            this.SelectPosition.TabIndex = 3;
            this.SelectPosition.SelectedIndexChanged += new System.EventHandler(this.SelectPosition_SelectedIndexChanged);
            // 
            // st_rm
            // 
            this.st_rm.AutoSize = true;
            this.st_rm.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.st_rm.Location = new System.Drawing.Point(235, 264);
            this.st_rm.Name = "st_rm";
            this.st_rm.Size = new System.Drawing.Size(44, 14);
            this.st_rm.TabIndex = 4;
            this.st_rm.Text = "Position";
            // 
            // RemoveButton
            // 
            this.RemoveButton.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RemoveButton.Location = new System.Drawing.Point(365, 280);
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.Size = new System.Drawing.Size(98, 23);
            this.RemoveButton.TabIndex = 5;
            this.RemoveButton.Text = "Remove";
            this.RemoveButton.UseVisualStyleBackColor = true;
            this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
            // 
            // Clear
            // 
            this.Clear.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Clear.Location = new System.Drawing.Point(365, 345);
            this.Clear.Name = "Clear";
            this.Clear.Size = new System.Drawing.Size(98, 23);
            this.Clear.TabIndex = 6;
            this.Clear.Text = "Clear All";
            this.Clear.UseVisualStyleBackColor = true;
            this.Clear.Click += new System.EventHandler(this.Clear_Click);
            // 
            // MotorX
            // 
            this.MotorX.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MotorX.Location = new System.Drawing.Point(238, 305);
            this.MotorX.Name = "MotorX";
            this.MotorX.Size = new System.Drawing.Size(100, 20);
            this.MotorX.TabIndex = 7;
            this.MotorX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // MotorY
            // 
            this.MotorY.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MotorY.Location = new System.Drawing.Point(238, 326);
            this.MotorY.Name = "MotorY";
            this.MotorY.Size = new System.Drawing.Size(100, 20);
            this.MotorY.TabIndex = 8;
            this.MotorY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // MotorZ
            // 
            this.MotorZ.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MotorZ.Location = new System.Drawing.Point(238, 347);
            this.MotorZ.Name = "MotorZ";
            this.MotorZ.Size = new System.Drawing.Size(100, 20);
            this.MotorZ.TabIndex = 9;
            this.MotorZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // Relative
            // 
            this.Relative.AutoSize = true;
            this.Relative.Checked = true;
            this.Relative.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Relative.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Relative.Location = new System.Drawing.Point(115, 307);
            this.Relative.Name = "Relative";
            this.Relative.Size = new System.Drawing.Size(120, 18);
            this.Relative.TabIndex = 11;
            this.Relative.Text = "Display relative pos";
            this.Relative.UseVisualStyleBackColor = true;
            this.Relative.Click += new System.EventHandler(this.SelectPosition_SelectedIndexChanged);
            // 
            // MotorCtrlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 380);
            this.Controls.Add(this.Relative);
            this.Controls.Add(this.MotorZ);
            this.Controls.Add(this.MotorY);
            this.Controls.Add(this.MotorX);
            this.Controls.Add(this.Clear);
            this.Controls.Add(this.RemoveButton);
            this.Controls.Add(this.st_rm);
            this.Controls.Add(this.SelectPosition);
            this.Controls.Add(this.motor_flowLayoutPanel);
            this.Controls.Add(this.AddPosition);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MotorCtrlPanel";
            this.Text = "Stage Position Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MotorCtrlPanel_FormClosing);
            this.Load += new System.EventHandler(this.MotorCtrlPanel_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button AddPosition;
        private System.Windows.Forms.FlowLayoutPanel motor_flowLayoutPanel;
        private System.Windows.Forms.ComboBox SelectPosition;
        private System.Windows.Forms.Label st_rm;
        private System.Windows.Forms.Button RemoveButton;
        private System.Windows.Forms.Button Clear;
        private System.Windows.Forms.TextBox MotorX;
        private System.Windows.Forms.TextBox MotorY;
        private System.Windows.Forms.TextBox MotorZ;
        private System.Windows.Forms.CheckBox Relative;
    }
}