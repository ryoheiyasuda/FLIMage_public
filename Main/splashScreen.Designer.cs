namespace FLIMimage
{
    partial class splashScreen
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
            this.MacID = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // MacID
            // 
            this.MacID.AutoSize = true;
            this.MacID.BackColor = System.Drawing.Color.Transparent;
            this.MacID.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.MacID.ForeColor = System.Drawing.Color.Lime;
            this.MacID.Location = new System.Drawing.Point(12, 429);
            this.MacID.Name = "MacID";
            this.MacID.Size = new System.Drawing.Size(56, 17);
            this.MacID.TabIndex = 0;
            this.MacID.Text = "ID: XXX";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::FLIMimage.Properties.Resources.FLIMage_title;
            this.pictureBox1.Location = new System.Drawing.Point(12, 120);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(697, 202);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // splashScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(710, 451);
            this.ControlBox = false;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.MacID);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MinimizeBox = false;
            this.Name = "splashScreen";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FLIMage version 1.0";
            this.Load += new System.EventHandler(this.splashScreen_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label MacID;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}