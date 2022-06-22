namespace FLIMage.Dialogs
{
    partial class ScanAreaWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScanAreaWindow));
            this.ScanPosition = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.ScanPosition)).BeginInit();
            this.SuspendLayout();
            // 
            // ScanPosition
            // 
            this.ScanPosition.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(10)))));
            this.ScanPosition.Location = new System.Drawing.Point(0, 0);
            this.ScanPosition.Name = "ScanPosition";
            this.ScanPosition.Size = new System.Drawing.Size(512, 512);
            this.ScanPosition.TabIndex = 0;
            this.ScanPosition.TabStop = false;
            this.ScanPosition.Paint += new System.Windows.Forms.PaintEventHandler(this.ScanPosition_Paint);
            this.ScanPosition.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ScanPosition_MouseDown);
            this.ScanPosition.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ScanPosition_MouseMove);
            this.ScanPosition.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ScanPosition_MouseUp);
            // 
            // ScanAreaWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 513);
            this.Controls.Add(this.ScanPosition);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ScanAreaWindow";
            this.Text = "ScanAreaWindow";
            this.Resize += new System.EventHandler(this.ScanAreaWindow_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.ScanPosition)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox ScanPosition;
    }
}