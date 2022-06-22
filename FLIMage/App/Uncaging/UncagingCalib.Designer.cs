namespace FLIMage.Uncaging
{
    partial class UncagingCalibration
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UncagingCalibration));
            this.ApplyButton = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.UncagingCalibY = new System.Windows.Forms.TextBox();
            this.UncagingCalibX = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ApplyButton
            // 
            this.ApplyButton.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ApplyButton.Location = new System.Drawing.Point(225, 131);
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.Size = new System.Drawing.Size(68, 22);
            this.ApplyButton.TabIndex = 323;
            this.ApplyButton.Text = "Apply";
            this.ApplyButton.UseVisualStyleBackColor = true;
            this.ApplyButton.Click += new System.EventHandler(this.ApplyCalib_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(41, 92);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(155, 28);
            this.label10.TabIndex = 292;
            this.label10.Text = "Right click on actual  uncaging \r\nlocation on the image";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(29, 37);
            this.label4.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 14);
            this.label4.TabIndex = 291;
            this.label4.Text = "X";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(29, 57);
            this.label5.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(15, 14);
            this.label5.TabIndex = 290;
            this.label5.Text = "Y";
            // 
            // UncagingCalibY
            // 
            this.UncagingCalibY.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UncagingCalibY.Location = new System.Drawing.Point(44, 55);
            this.UncagingCalibY.Margin = new System.Windows.Forms.Padding(1);
            this.UncagingCalibY.Name = "UncagingCalibY";
            this.UncagingCalibY.Size = new System.Drawing.Size(50, 20);
            this.UncagingCalibY.TabIndex = 287;
            this.UncagingCalibY.Text = "0";
            this.UncagingCalibY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // UncagingCalibX
            // 
            this.UncagingCalibX.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UncagingCalibX.Location = new System.Drawing.Point(44, 33);
            this.UncagingCalibX.Margin = new System.Windows.Forms.Padding(1);
            this.UncagingCalibX.Name = "UncagingCalibX";
            this.UncagingCalibX.Size = new System.Drawing.Size(50, 20);
            this.UncagingCalibX.TabIndex = 286;
            this.UncagingCalibX.Text = "0";
            this.UncagingCalibX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(41, 131);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 14);
            this.label1.TabIndex = 324;
            this.label1.Text = "(Orange cursor)";
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Cancel_Button.Location = new System.Drawing.Point(225, 103);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(68, 22);
            this.Cancel_Button.TabIndex = 325;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            this.Cancel_Button.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // UncagingCalibration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(305, 165);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ApplyButton);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.UncagingCalibX);
            this.Controls.Add(this.UncagingCalibY);
            this.Controls.Add(this.label5);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "UncagingCalibration";
            this.Text = "Uncaging calibration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UncagingCalibration_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Button ApplyButton;
        public System.Windows.Forms.Label label10;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.Label label5;
        public System.Windows.Forms.TextBox UncagingCalibY;
        public System.Windows.Forms.TextBox UncagingCalibX;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Button Cancel_Button;
    }
}