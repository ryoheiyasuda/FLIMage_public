namespace FLIMage.Plotting
{
    partial class plot_timeCourse
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(plot_timeCourse));
            this.RealtimePlot = new System.Windows.Forms.PictureBox();
            this.tau_m_radio = new System.Windows.Forms.RadioButton();
            this.meanIntensity_radio = new System.Windows.Forms.RadioButton();
            this.fraction2_radio = new System.Windows.Forms.RadioButton();
            this.tau_m_fit_radio = new System.Windows.Forms.RadioButton();
            this.fraction2_fit_radio = new System.Windows.Forms.RadioButton();
            this.calcFitCheck = new System.Windows.Forms.CheckBox();
            this.TC_Reset = new System.Windows.Forms.Button();
            this.CalcTimeCourse = new System.Windows.Forms.Button();
            this.Warning = new System.Windows.Forms.Label();
            this.CalculateSinglePage = new System.Windows.Forms.Button();
            this.sumIntensity_radio = new System.Windows.Forms.RadioButton();
            this.CalculateUponOpen = new System.Windows.Forms.CheckBox();
            this.OpenExcel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.RealtimePlot)).BeginInit();
            this.SuspendLayout();
            // 
            // RealtimePlot
            // 
            this.RealtimePlot.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RealtimePlot.BackColor = System.Drawing.Color.White;
            this.RealtimePlot.Location = new System.Drawing.Point(0, -2);
            this.RealtimePlot.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.RealtimePlot.Name = "RealtimePlot";
            this.RealtimePlot.Size = new System.Drawing.Size(882, 552);
            this.RealtimePlot.TabIndex = 1;
            this.RealtimePlot.TabStop = false;
            // 
            // tau_m_radio
            // 
            this.tau_m_radio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tau_m_radio.AutoSize = true;
            this.tau_m_radio.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.tau_m_radio.Location = new System.Drawing.Point(907, 92);
            this.tau_m_radio.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.tau_m_radio.Name = "tau_m_radio";
            this.tau_m_radio.Size = new System.Drawing.Size(139, 30);
            this.tau_m_radio.TabIndex = 1;
            this.tau_m_radio.Text = "Mean Tau";
            this.tau_m_radio.UseVisualStyleBackColor = true;
            this.tau_m_radio.Click += new System.EventHandler(this.Intensity_radio_Click);
            // 
            // meanIntensity_radio
            // 
            this.meanIntensity_radio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.meanIntensity_radio.AutoSize = true;
            this.meanIntensity_radio.Checked = true;
            this.meanIntensity_radio.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.meanIntensity_radio.Location = new System.Drawing.Point(900, 15);
            this.meanIntensity_radio.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.meanIntensity_radio.Name = "meanIntensity_radio";
            this.meanIntensity_radio.Size = new System.Drawing.Size(198, 30);
            this.meanIntensity_radio.TabIndex = 5;
            this.meanIntensity_radio.TabStop = true;
            this.meanIntensity_radio.Text = "Intensity (Mean)";
            this.meanIntensity_radio.UseVisualStyleBackColor = true;
            this.meanIntensity_radio.Click += new System.EventHandler(this.Intensity_radio_Click);
            // 
            // fraction2_radio
            // 
            this.fraction2_radio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fraction2_radio.AutoSize = true;
            this.fraction2_radio.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.fraction2_radio.Location = new System.Drawing.Point(903, 129);
            this.fraction2_radio.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.fraction2_radio.Name = "fraction2_radio";
            this.fraction2_radio.Size = new System.Drawing.Size(133, 30);
            this.fraction2_radio.TabIndex = 477;
            this.fraction2_radio.Text = "Fraction2";
            this.fraction2_radio.UseVisualStyleBackColor = true;
            this.fraction2_radio.Click += new System.EventHandler(this.Intensity_radio_Click);
            // 
            // tau_m_fit_radio
            // 
            this.tau_m_fit_radio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tau_m_fit_radio.AutoSize = true;
            this.tau_m_fit_radio.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.tau_m_fit_radio.Location = new System.Drawing.Point(934, 212);
            this.tau_m_fit_radio.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.tau_m_fit_radio.Name = "tau_m_fit_radio";
            this.tau_m_fit_radio.Size = new System.Drawing.Size(170, 30);
            this.tau_m_fit_radio.TabIndex = 478;
            this.tau_m_fit_radio.Text = "Mean tau (fit)";
            this.tau_m_fit_radio.UseVisualStyleBackColor = true;
            this.tau_m_fit_radio.Click += new System.EventHandler(this.Intensity_radio_Click);
            // 
            // fraction2_fit_radio
            // 
            this.fraction2_fit_radio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fraction2_fit_radio.AutoSize = true;
            this.fraction2_fit_radio.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.fraction2_fit_radio.Location = new System.Drawing.Point(932, 250);
            this.fraction2_fit_radio.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.fraction2_fit_radio.Name = "fraction2_fit_radio";
            this.fraction2_fit_radio.Size = new System.Drawing.Size(170, 30);
            this.fraction2_fit_radio.TabIndex = 479;
            this.fraction2_fit_radio.Text = "Fraction2 (fit)";
            this.fraction2_fit_radio.UseVisualStyleBackColor = true;
            this.fraction2_fit_radio.Click += new System.EventHandler(this.Intensity_radio_Click);
            // 
            // calcFitCheck
            // 
            this.calcFitCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.calcFitCheck.AutoSize = true;
            this.calcFitCheck.Checked = true;
            this.calcFitCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.calcFitCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.calcFitCheck.Location = new System.Drawing.Point(904, 173);
            this.calcFitCheck.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.calcFitCheck.Name = "calcFitCheck";
            this.calcFitCheck.Size = new System.Drawing.Size(138, 30);
            this.calcFitCheck.TabIndex = 480;
            this.calcFitCheck.Text = "Fit curves";
            this.calcFitCheck.UseVisualStyleBackColor = true;
            this.calcFitCheck.CheckedChanged += new System.EventHandler(this.CalcFitCheck_CheckedChanged);
            // 
            // TC_Reset
            // 
            this.TC_Reset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TC_Reset.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.TC_Reset.Location = new System.Drawing.Point(898, 337);
            this.TC_Reset.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.TC_Reset.Name = "TC_Reset";
            this.TC_Reset.Size = new System.Drawing.Size(244, 52);
            this.TC_Reset.TabIndex = 482;
            this.TC_Reset.Text = "Reset plot";
            this.TC_Reset.UseVisualStyleBackColor = true;
            this.TC_Reset.Click += new System.EventHandler(this.TC_Reset_Click);
            // 
            // CalcTimeCourse
            // 
            this.CalcTimeCourse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CalcTimeCourse.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.CalcTimeCourse.Location = new System.Drawing.Point(898, 437);
            this.CalcTimeCourse.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.CalcTimeCourse.Name = "CalcTimeCourse";
            this.CalcTimeCourse.Size = new System.Drawing.Size(244, 52);
            this.CalcTimeCourse.TabIndex = 481;
            this.CalcTimeCourse.Text = "Calculate all pages";
            this.CalcTimeCourse.UseVisualStyleBackColor = true;
            this.CalcTimeCourse.Click += new System.EventHandler(this.CalcCurrent_Click);
            // 
            // Warning
            // 
            this.Warning.AutoSize = true;
            this.Warning.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.Warning.ForeColor = System.Drawing.Color.Red;
            this.Warning.Location = new System.Drawing.Point(130, 40);
            this.Warning.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Warning.Name = "Warning";
            this.Warning.Size = new System.Drawing.Size(0, 31);
            this.Warning.TabIndex = 483;
            // 
            // CalculateSinglePage
            // 
            this.CalculateSinglePage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CalculateSinglePage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.CalculateSinglePage.Location = new System.Drawing.Point(898, 387);
            this.CalculateSinglePage.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.CalculateSinglePage.Name = "CalculateSinglePage";
            this.CalculateSinglePage.Size = new System.Drawing.Size(244, 52);
            this.CalculateSinglePage.TabIndex = 484;
            this.CalculateSinglePage.Text = "Calculate current page";
            this.CalculateSinglePage.UseVisualStyleBackColor = true;
            this.CalculateSinglePage.Click += new System.EventHandler(this.CalculateSinglePage_Click);
            // 
            // sumIntensity_radio
            // 
            this.sumIntensity_radio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sumIntensity_radio.AutoSize = true;
            this.sumIntensity_radio.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.sumIntensity_radio.Location = new System.Drawing.Point(896, 54);
            this.sumIntensity_radio.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.sumIntensity_radio.Name = "sumIntensity_radio";
            this.sumIntensity_radio.Size = new System.Drawing.Size(190, 30);
            this.sumIntensity_radio.TabIndex = 485;
            this.sumIntensity_radio.Text = "Intensity (Sum)";
            this.sumIntensity_radio.UseVisualStyleBackColor = true;
            // 
            // CalculateUponOpen
            // 
            this.CalculateUponOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CalculateUponOpen.AutoSize = true;
            this.CalculateUponOpen.Checked = true;
            this.CalculateUponOpen.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CalculateUponOpen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.CalculateUponOpen.Location = new System.Drawing.Point(899, 294);
            this.CalculateUponOpen.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.CalculateUponOpen.Name = "CalculateUponOpen";
            this.CalculateUponOpen.Size = new System.Drawing.Size(243, 30);
            this.CalculateUponOpen.TabIndex = 486;
            this.CalculateUponOpen.Text = "Calculate upon open";
            this.CalculateUponOpen.UseVisualStyleBackColor = true;
            this.CalculateUponOpen.CheckedChanged += new System.EventHandler(this.CalculateUponOpen_CheckedChanged);
            // 
            // OpenExcel
            // 
            this.OpenExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenExcel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.OpenExcel.Location = new System.Drawing.Point(1056, 487);
            this.OpenExcel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.OpenExcel.Name = "OpenExcel";
            this.OpenExcel.Size = new System.Drawing.Size(84, 52);
            this.OpenExcel.TabIndex = 487;
            this.OpenExcel.Text = "View";
            this.OpenExcel.UseVisualStyleBackColor = true;
            this.OpenExcel.Click += new System.EventHandler(this.OpenExcel_Click);
            // 
            // plot_timeCourse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1156, 548);
            this.Controls.Add(this.OpenExcel);
            this.Controls.Add(this.CalculateUponOpen);
            this.Controls.Add(this.sumIntensity_radio);
            this.Controls.Add(this.CalculateSinglePage);
            this.Controls.Add(this.Warning);
            this.Controls.Add(this.TC_Reset);
            this.Controls.Add(this.CalcTimeCourse);
            this.Controls.Add(this.calcFitCheck);
            this.Controls.Add(this.tau_m_fit_radio);
            this.Controls.Add(this.fraction2_fit_radio);
            this.Controls.Add(this.fraction2_radio);
            this.Controls.Add(this.meanIntensity_radio);
            this.Controls.Add(this.tau_m_radio);
            this.Controls.Add(this.RealtimePlot);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "plot_timeCourse";
            this.Text = "Plot time course";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.plot_timeCourse_FormClosing);
            this.Load += new System.EventHandler(this.plot_timeCourse_Load);
            this.Resize += new System.EventHandler(this.plot_timeCourse_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.RealtimePlot)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox RealtimePlot;
        private System.Windows.Forms.RadioButton tau_m_radio;
        private System.Windows.Forms.RadioButton meanIntensity_radio;
        private System.Windows.Forms.RadioButton fraction2_radio;
        private System.Windows.Forms.RadioButton tau_m_fit_radio;
        private System.Windows.Forms.RadioButton fraction2_fit_radio;
        private System.Windows.Forms.CheckBox calcFitCheck;
        private System.Windows.Forms.Button TC_Reset;
        private System.Windows.Forms.Button CalcTimeCourse;
        private System.Windows.Forms.Label Warning;
        private System.Windows.Forms.Button CalculateSinglePage;
        private System.Windows.Forms.RadioButton sumIntensity_radio;
        private System.Windows.Forms.CheckBox CalculateUponOpen;
        private System.Windows.Forms.Button OpenExcel;
    }
}