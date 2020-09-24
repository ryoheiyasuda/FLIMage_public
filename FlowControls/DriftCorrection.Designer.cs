namespace FLIMage.FlowControls
{
    partial class DriftCorrection
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DriftCorrection));
            this.DriftCorrection_CB = new System.Windows.Forms.CheckBox();
            this.XYCorrect_CB = new System.Windows.Forms.CheckBox();
            this.ZCorrect_CB = new System.Windows.Forms.CheckBox();
            this.UseMirror_CB = new System.Windows.Forms.CheckBox();
            this.SelectCurrentImage = new System.Windows.Forms.Button();
            this.TemplateFileName = new System.Windows.Forms.Label();
            this.Channel_Pulldown = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TemplateImage_PB = new System.Windows.Forms.PictureBox();
            this.Status_XY = new System.Windows.Forms.Label();
            this.Status_Z = new System.Windows.Forms.Label();
            this.Status_V = new System.Windows.Forms.Label();
            this.MoveOposite_Z = new System.Windows.Forms.CheckBox();
            this.Status_um = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.TemplateImage_PB)).BeginInit();
            this.SuspendLayout();
            // 
            // DriftCorrection_CB
            // 
            this.DriftCorrection_CB.AutoSize = true;
            this.DriftCorrection_CB.Location = new System.Drawing.Point(24, 12);
            this.DriftCorrection_CB.Name = "DriftCorrection_CB";
            this.DriftCorrection_CB.Size = new System.Drawing.Size(114, 17);
            this.DriftCorrection_CB.TabIndex = 0;
            this.DriftCorrection_CB.Text = "Drift correction ON";
            this.DriftCorrection_CB.UseVisualStyleBackColor = true;
            // 
            // XYCorrect_CB
            // 
            this.XYCorrect_CB.AutoSize = true;
            this.XYCorrect_CB.Location = new System.Drawing.Point(49, 40);
            this.XYCorrect_CB.Name = "XYCorrect_CB";
            this.XYCorrect_CB.Size = new System.Drawing.Size(40, 17);
            this.XYCorrect_CB.TabIndex = 1;
            this.XYCorrect_CB.Text = "XY";
            this.XYCorrect_CB.UseVisualStyleBackColor = true;
            // 
            // ZCorrect_CB
            // 
            this.ZCorrect_CB.AutoSize = true;
            this.ZCorrect_CB.Checked = true;
            this.ZCorrect_CB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ZCorrect_CB.Location = new System.Drawing.Point(49, 84);
            this.ZCorrect_CB.Name = "ZCorrect_CB";
            this.ZCorrect_CB.Size = new System.Drawing.Size(33, 17);
            this.ZCorrect_CB.TabIndex = 2;
            this.ZCorrect_CB.Text = "Z";
            this.ZCorrect_CB.UseVisualStyleBackColor = true;
            // 
            // UseMirror_CB
            // 
            this.UseMirror_CB.AutoSize = true;
            this.UseMirror_CB.Checked = true;
            this.UseMirror_CB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.UseMirror_CB.Location = new System.Drawing.Point(77, 61);
            this.UseMirror_CB.Name = "UseMirror_CB";
            this.UseMirror_CB.Size = new System.Drawing.Size(73, 17);
            this.UseMirror_CB.TabIndex = 3;
            this.UseMirror_CB.Text = "Use mirror";
            this.UseMirror_CB.UseVisualStyleBackColor = true;
            // 
            // SelectCurrentImage
            // 
            this.SelectCurrentImage.Location = new System.Drawing.Point(24, 127);
            this.SelectCurrentImage.Name = "SelectCurrentImage";
            this.SelectCurrentImage.Size = new System.Drawing.Size(221, 23);
            this.SelectCurrentImage.TabIndex = 4;
            this.SelectCurrentImage.Text = "Select current image for template";
            this.SelectCurrentImage.UseVisualStyleBackColor = true;
            this.SelectCurrentImage.Click += new System.EventHandler(this.SelectCurrentImage_Click);
            // 
            // TemplateFileName
            // 
            this.TemplateFileName.AutoSize = true;
            this.TemplateFileName.Location = new System.Drawing.Point(10, 194);
            this.TemplateFileName.Name = "TemplateFileName";
            this.TemplateFileName.Size = new System.Drawing.Size(54, 13);
            this.TemplateFileName.TabIndex = 5;
            this.TemplateFileName.Text = "FileName:";
            // 
            // Channel_Pulldown
            // 
            this.Channel_Pulldown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Channel_Pulldown.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Channel_Pulldown.FormattingEnabled = true;
            this.Channel_Pulldown.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4"});
            this.Channel_Pulldown.Location = new System.Drawing.Point(84, 156);
            this.Channel_Pulldown.Name = "Channel_Pulldown";
            this.Channel_Pulldown.Size = new System.Drawing.Size(95, 22);
            this.Channel_Pulldown.TabIndex = 263;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 153);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 264;
            this.label1.Text = "Channel";
            // 
            // TemplateImage_PB
            // 
            this.TemplateImage_PB.Location = new System.Drawing.Point(266, 12);
            this.TemplateImage_PB.Name = "TemplateImage_PB";
            this.TemplateImage_PB.Size = new System.Drawing.Size(256, 256);
            this.TemplateImage_PB.TabIndex = 265;
            this.TemplateImage_PB.TabStop = false;
            this.TemplateImage_PB.Paint += new System.Windows.Forms.PaintEventHandler(this.TemplateImage_PB_Paint);
            // 
            // Status_XY
            // 
            this.Status_XY.AutoSize = true;
            this.Status_XY.Location = new System.Drawing.Point(10, 215);
            this.Status_XY.Name = "Status_XY";
            this.Status_XY.Size = new System.Drawing.Size(44, 13);
            this.Status_XY.TabIndex = 266;
            this.Status_XY.Text = "XY drift:";
            // 
            // Status_Z
            // 
            this.Status_Z.AutoSize = true;
            this.Status_Z.Location = new System.Drawing.Point(15, 258);
            this.Status_Z.Name = "Status_Z";
            this.Status_Z.Size = new System.Drawing.Size(37, 13);
            this.Status_Z.TabIndex = 267;
            this.Status_Z.Text = "Z drift:";
            // 
            // Status_V
            // 
            this.Status_V.AutoSize = true;
            this.Status_V.Location = new System.Drawing.Point(10, 229);
            this.Status_V.Name = "Status_V";
            this.Status_V.Size = new System.Drawing.Size(49, 13);
            this.Status_V.TabIndex = 268;
            this.Status_V.Text = "Voltage: ";
            // 
            // MoveOposite_Z
            // 
            this.MoveOposite_Z.AutoSize = true;
            this.MoveOposite_Z.Location = new System.Drawing.Point(77, 103);
            this.MoveOposite_Z.Name = "MoveOposite_Z";
            this.MoveOposite_Z.Size = new System.Drawing.Size(161, 17);
            this.MoveOposite_Z.TabIndex = 269;
            this.MoveOposite_Z.Text = "Move to oposite direction (Z)";
            this.MoveOposite_Z.UseVisualStyleBackColor = true;
            // 
            // Status_um
            // 
            this.Status_um.AutoSize = true;
            this.Status_um.Location = new System.Drawing.Point(9, 242);
            this.Status_um.Name = "Status_um";
            this.Status_um.Size = new System.Drawing.Size(70, 13);
            this.Status_um.TabIndex = 270;
            this.Status_um.Text = "Micrometers: ";
            // 
            // DriftCorrection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(537, 282);
            this.Controls.Add(this.Status_um);
            this.Controls.Add(this.MoveOposite_Z);
            this.Controls.Add(this.Status_V);
            this.Controls.Add(this.Status_Z);
            this.Controls.Add(this.Status_XY);
            this.Controls.Add(this.TemplateImage_PB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Channel_Pulldown);
            this.Controls.Add(this.TemplateFileName);
            this.Controls.Add(this.SelectCurrentImage);
            this.Controls.Add(this.UseMirror_CB);
            this.Controls.Add(this.ZCorrect_CB);
            this.Controls.Add(this.XYCorrect_CB);
            this.Controls.Add(this.DriftCorrection_CB);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DriftCorrection";
            this.Text = "DriftCorrection";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DriftCorrection_FormClosing);
            this.Load += new System.EventHandler(this.DriftCorrection_Load);
            ((System.ComponentModel.ISupportInitialize)(this.TemplateImage_PB)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.CheckBox DriftCorrection_CB;
        public System.Windows.Forms.CheckBox XYCorrect_CB;
        public System.Windows.Forms.CheckBox ZCorrect_CB;
        public System.Windows.Forms.CheckBox UseMirror_CB;
        public System.Windows.Forms.Button SelectCurrentImage;
        public System.Windows.Forms.Label TemplateFileName;
        public System.Windows.Forms.ComboBox Channel_Pulldown;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.PictureBox TemplateImage_PB;
        public System.Windows.Forms.Label Status_XY;
        public System.Windows.Forms.Label Status_Z;
        public System.Windows.Forms.Label Status_V;
        public System.Windows.Forms.CheckBox MoveOposite_Z;
        public System.Windows.Forms.Label Status_um;
    }
}