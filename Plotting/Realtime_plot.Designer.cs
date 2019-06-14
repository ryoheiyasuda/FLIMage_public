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
            this.RealtimePlot.Location = new System.Drawing.Point(0, -1);
            this.RealtimePlot.Name = "RealtimePlot";
            this.RealtimePlot.Size = new System.Drawing.Size(441, 287);
            this.RealtimePlot.TabIndex = 1;
            this.RealtimePlot.TabStop = false;
            // 
            // tau_m_radio
            // 
            this.tau_m_radio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tau_m_radio.AutoSize = true;
            this.tau_m_radio.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.tau_m_radio.Location = new System.Drawing.Point(450, 48);
            this.tau_m_radio.Name = "tau_m_radio";
            this.tau_m_radio.Size = new System.Drawing.Size(74, 17);
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
            this.meanIntensity_radio.Location = new System.Drawing.Point(449, 8);
            this.meanIntensity_radio.Name = "meanIntensity_radio";
            this.meanIntensity_radio.Size = new System.Drawing.Size(100, 17);
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
            this.fraction2_radio.Location = new System.Drawing.Point(449, 67);
            this.fraction2_radio.Name = "fraction2_radio";
            this.fraction2_radio.Size = new System.Drawing.Size(69, 17);
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
            this.tau_m_fit_radio.Location = new System.Drawing.Point(465, 110);
            this.tau_m_fit_radio.Name = "tau_m_fit_radio";
            this.tau_m_fit_radio.Size = new System.Drawing.Size(87, 17);
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
            this.fraction2_fit_radio.Location = new System.Drawing.Point(465, 130);
            this.fraction2_fit_radio.Name = "fraction2_fit_radio";
            this.fraction2_fit_radio.Size = new System.Drawing.Size(86, 17);
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
            this.calcFitCheck.Location = new System.Drawing.Point(449, 90);
            this.calcFitCheck.Name = "calcFitCheck";
            this.calcFitCheck.Size = new System.Drawing.Size(72, 17);
            this.calcFitCheck.TabIndex = 480;
            this.calcFitCheck.Text = "Fit curves";
            this.calcFitCheck.UseVisualStyleBackColor = true;
            this.calcFitCheck.CheckedChanged += new System.EventHandler(this.CalcFitCheck_CheckedChanged);
            // 
            // TC_Reset
            // 
            this.TC_Reset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TC_Reset.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.TC_Reset.Location = new System.Drawing.Point(449, 175);
            this.TC_Reset.Name = "TC_Reset";
            this.TC_Reset.Size = new System.Drawing.Size(122, 27);
            this.TC_Reset.TabIndex = 482;
            this.TC_Reset.Text = "Reset plot";
            this.TC_Reset.UseVisualStyleBackColor = true;
            this.TC_Reset.Click += new System.EventHandler(this.TC_Reset_Click);
            // 
            // CalcTimeCourse
            // 
            this.CalcTimeCourse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CalcTimeCourse.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.CalcTimeCourse.Location = new System.Drawing.Point(449, 227);
            this.CalcTimeCourse.Name = "CalcTimeCourse";
            this.CalcTimeCourse.Size = new System.Drawing.Size(122, 27);
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
            this.Warning.Location = new System.Drawing.Point(65, 21);
            this.Warning.Name = "Warning";
            this.Warning.Size = new System.Drawing.Size(0, 17);
            this.Warning.TabIndex = 483;
            // 
            // CalculateSinglePage
            // 
            this.CalculateSinglePage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CalculateSinglePage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.CalculateSinglePage.Location = new System.Drawing.Point(449, 201);
            this.CalculateSinglePage.Name = "CalculateSinglePage";
            this.CalculateSinglePage.Size = new System.Drawing.Size(122, 27);
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
            this.sumIntensity_radio.Location = new System.Drawing.Point(449, 28);
            this.sumIntensity_radio.Name = "sumIntensity_radio";
            this.sumIntensity_radio.Size = new System.Drawing.Size(94, 17);
            this.sumIntensity_radio.TabIndex = 485;
            this.sumIntensity_radio.Text = "Intensity (Sum)";
            this.sumIntensity_radio.UseVisualStyleBackColor = true;
            this.sumIntensity_radio.Click += new System.EventHandler(this.Intensity_radio_Click);
            // 
            // CalculateUponOpen
            // 
            this.CalculateUponOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CalculateUponOpen.AutoSize = true;
            this.CalculateUponOpen.Checked = true;
            this.CalculateUponOpen.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CalculateUponOpen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.CalculateUponOpen.Location = new System.Drawing.Point(448, 153);
            this.CalculateUponOpen.Name = "CalculateUponOpen";
            this.CalculateUponOpen.Size = new System.Drawing.Size(124, 17);
            this.CalculateUponOpen.TabIndex = 486;
            this.CalculateUponOpen.Text = "Calculate upon open";
            this.CalculateUponOpen.UseVisualStyleBackColor = true;
            this.CalculateUponOpen.CheckedChanged += new System.EventHandler(this.CalculateUponOpen_CheckedChanged);
            // 
            // OpenExcel
            // 
            this.OpenExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenExcel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.OpenExcel.Location = new System.Drawing.Point(528, 253);
            this.OpenExcel.Name = "OpenExcel";
            this.OpenExcel.Size = new System.Drawing.Size(42, 27);
            this.OpenExcel.TabIndex = 487;
            this.OpenExcel.Text = "View";
            this.OpenExcel.UseVisualStyleBackColor = true;
            this.OpenExcel.Click += new System.EventHandler(this.OpenExcel_Click);
            // 
            // plot_timeCourse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(578, 285);
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