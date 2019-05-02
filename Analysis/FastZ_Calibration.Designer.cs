namespace FLIMage.Analysis
{
    partial class FastZ_Calibration
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FastZ_Calibration));
            this.ZStack_Start = new System.Windows.Forms.TextBox();
            this.label104 = new System.Windows.Forms.Label();
            this.ZStack_End = new System.Windows.Forms.TextBox();
            this.PlotPanel = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.CalcButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.FastZStack_End = new System.Windows.Forms.TextBox();
            this.FastZStack_Start = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.FitData = new System.Windows.Forms.Label();
            this.PlotPanel2 = new System.Windows.Forms.PictureBox();
            this.PlotPanel3 = new System.Windows.Forms.PictureBox();
            this.FitData2 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.IntensityThreshold = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.AllSlices = new System.Windows.Forms.CheckBox();
            this.PlotPanel4 = new System.Windows.Forms.PictureBox();
            this.PlotPanel5 = new System.Windows.Forms.PictureBox();
            this.PlotPanel6 = new System.Windows.Forms.PictureBox();
            this.FOVLabel = new System.Windows.Forms.Label();
            this.Objective_Pulldown = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.PlotPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlotPanel2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlotPanel3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlotPanel4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlotPanel5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlotPanel6)).BeginInit();
            this.SuspendLayout();
            // 
            // ZStack_Start
            // 
            this.ZStack_Start.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZStack_Start.Location = new System.Drawing.Point(1566, 96);
            this.ZStack_Start.Margin = new System.Windows.Forms.Padding(2);
            this.ZStack_Start.Name = "ZStack_Start";
            this.ZStack_Start.Size = new System.Drawing.Size(96, 33);
            this.ZStack_Start.TabIndex = 280;
            this.ZStack_Start.Text = "0";
            this.ZStack_Start.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ZStack_Start.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ZStack_Start_KeyDown);
            // 
            // label104
            // 
            this.label104.AutoSize = true;
            this.label104.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label104.Location = new System.Drawing.Point(1494, 100);
            this.label104.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label104.Name = "label104";
            this.label104.Size = new System.Drawing.Size(58, 25);
            this.label104.TabIndex = 279;
            this.label104.Text = "Start";
            // 
            // ZStack_End
            // 
            this.ZStack_End.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZStack_End.Location = new System.Drawing.Point(1566, 144);
            this.ZStack_End.Margin = new System.Windows.Forms.Padding(2);
            this.ZStack_End.Name = "ZStack_End";
            this.ZStack_End.Size = new System.Drawing.Size(96, 33);
            this.ZStack_End.TabIndex = 282;
            this.ZStack_End.Text = "0";
            this.ZStack_End.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ZStack_End.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ZStack_Start_KeyDown);
            // 
            // PlotPanel
            // 
            this.PlotPanel.BackColor = System.Drawing.Color.White;
            this.PlotPanel.Location = new System.Drawing.Point(0, -2);
            this.PlotPanel.Margin = new System.Windows.Forms.Padding(4);
            this.PlotPanel.Name = "PlotPanel";
            this.PlotPanel.Size = new System.Drawing.Size(721, 460);
            this.PlotPanel.TabIndex = 0;
            this.PlotPanel.TabStop = false;
            this.PlotPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.PlotPanel_Paint);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(1502, 144);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 25);
            this.label1.TabIndex = 283;
            this.label1.Text = "End";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(1494, 50);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 25);
            this.label2.TabIndex = 284;
            this.label2.Text = "Z-Stack";
            // 
            // CalcButton
            // 
            this.CalcButton.Location = new System.Drawing.Point(1521, 800);
            this.CalcButton.Margin = new System.Windows.Forms.Padding(4);
            this.CalcButton.Name = "CalcButton";
            this.CalcButton.Size = new System.Drawing.Size(146, 71);
            this.CalcButton.TabIndex = 285;
            this.CalcButton.Text = "Calculate";
            this.CalcButton.UseVisualStyleBackColor = true;
            this.CalcButton.Click += new System.EventHandler(this.CalcButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(1494, 246);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(189, 25);
            this.label3.TabIndex = 290;
            this.label3.Text = "Phase range (deg)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(1502, 330);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 25);
            this.label4.TabIndex = 289;
            this.label4.Text = "End";
            // 
            // FastZStack_End
            // 
            this.FastZStack_End.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FastZStack_End.Location = new System.Drawing.Point(1566, 330);
            this.FastZStack_End.Margin = new System.Windows.Forms.Padding(2);
            this.FastZStack_End.Name = "FastZStack_End";
            this.FastZStack_End.Size = new System.Drawing.Size(96, 33);
            this.FastZStack_End.TabIndex = 288;
            this.FastZStack_End.Text = "0";
            this.FastZStack_End.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.FastZStack_End.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ZStack_Start_KeyDown);
            // 
            // FastZStack_Start
            // 
            this.FastZStack_Start.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FastZStack_Start.Location = new System.Drawing.Point(1566, 282);
            this.FastZStack_Start.Margin = new System.Windows.Forms.Padding(2);
            this.FastZStack_Start.Name = "FastZStack_Start";
            this.FastZStack_Start.Size = new System.Drawing.Size(96, 33);
            this.FastZStack_Start.TabIndex = 287;
            this.FastZStack_Start.Text = "0";
            this.FastZStack_Start.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.FastZStack_Start.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ZStack_Start_KeyDown);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(1494, 286);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 25);
            this.label5.TabIndex = 286;
            this.label5.Text = "Start";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(1503, 682);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(146, 25);
            this.label6.TabIndex = 291;
            this.label6.Text = "Z_Calibration:";
            // 
            // FitData
            // 
            this.FitData.AutoSize = true;
            this.FitData.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FitData.Location = new System.Drawing.Point(1501, 719);
            this.FitData.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.FitData.Name = "FitData";
            this.FitData.Size = new System.Drawing.Size(167, 25);
            this.FitData.TabIndex = 292;
            this.FitData.Text = "10.0 um / 90deg";
            // 
            // PlotPanel2
            // 
            this.PlotPanel2.BackColor = System.Drawing.Color.White;
            this.PlotPanel2.Location = new System.Drawing.Point(0, 463);
            this.PlotPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.PlotPanel2.Name = "PlotPanel2";
            this.PlotPanel2.Size = new System.Drawing.Size(721, 290);
            this.PlotPanel2.TabIndex = 293;
            this.PlotPanel2.TabStop = false;
            this.PlotPanel2.Paint += new System.Windows.Forms.PaintEventHandler(this.PlotPanel2_Paint);
            // 
            // PlotPanel3
            // 
            this.PlotPanel3.BackColor = System.Drawing.Color.White;
            this.PlotPanel3.Location = new System.Drawing.Point(0, 758);
            this.PlotPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.PlotPanel3.Name = "PlotPanel3";
            this.PlotPanel3.Size = new System.Drawing.Size(721, 290);
            this.PlotPanel3.TabIndex = 294;
            this.PlotPanel3.TabStop = false;
            this.PlotPanel3.Paint += new System.Windows.Forms.PaintEventHandler(this.PlotPanel3_Paint);
            // 
            // FitData2
            // 
            this.FitData2.AutoSize = true;
            this.FitData2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FitData2.Location = new System.Drawing.Point(1515, 750);
            this.FitData2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.FitData2.Name = "FitData2";
            this.FitData2.Size = new System.Drawing.Size(131, 25);
            this.FitData2.TabIndex = 295;
            this.FitData2.Text = "1.0 um / deg";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(1494, 425);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(184, 25);
            this.label7.TabIndex = 296;
            this.label7.Text = "Intensity threshold";
            // 
            // IntensityThreshold
            // 
            this.IntensityThreshold.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IntensityThreshold.Location = new System.Drawing.Point(1566, 488);
            this.IntensityThreshold.Margin = new System.Windows.Forms.Padding(2);
            this.IntensityThreshold.Name = "IntensityThreshold";
            this.IntensityThreshold.Size = new System.Drawing.Size(96, 33);
            this.IntensityThreshold.TabIndex = 297;
            this.IntensityThreshold.Text = "0";
            this.IntensityThreshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.IntensityThreshold.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ZStack_Start_KeyDown);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(1500, 459);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 25);
            this.label8.TabIndex = 298;
            this.label8.Text = "%Peak";
            // 
            // AllSlices
            // 
            this.AllSlices.AutoSize = true;
            this.AllSlices.Checked = true;
            this.AllSlices.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AllSlices.Location = new System.Drawing.Point(1508, 184);
            this.AllSlices.Margin = new System.Windows.Forms.Padding(6);
            this.AllSlices.Name = "AllSlices";
            this.AllSlices.Size = new System.Drawing.Size(129, 29);
            this.AllSlices.TabIndex = 299;
            this.AllSlices.Text = "All slices";
            this.AllSlices.UseVisualStyleBackColor = true;
            this.AllSlices.CheckedChanged += new System.EventHandler(this.AllSlices_CheckedChanged);
            // 
            // PlotPanel4
            // 
            this.PlotPanel4.BackColor = System.Drawing.Color.White;
            this.PlotPanel4.Location = new System.Drawing.Point(729, -2);
            this.PlotPanel4.Margin = new System.Windows.Forms.Padding(4);
            this.PlotPanel4.Name = "PlotPanel4";
            this.PlotPanel4.Size = new System.Drawing.Size(721, 460);
            this.PlotPanel4.TabIndex = 300;
            this.PlotPanel4.TabStop = false;
            this.PlotPanel4.Paint += new System.Windows.Forms.PaintEventHandler(this.PlotPanel4_Paint);
            // 
            // PlotPanel5
            // 
            this.PlotPanel5.BackColor = System.Drawing.Color.White;
            this.PlotPanel5.Location = new System.Drawing.Point(729, 463);
            this.PlotPanel5.Margin = new System.Windows.Forms.Padding(4);
            this.PlotPanel5.Name = "PlotPanel5";
            this.PlotPanel5.Size = new System.Drawing.Size(721, 290);
            this.PlotPanel5.TabIndex = 301;
            this.PlotPanel5.TabStop = false;
            this.PlotPanel5.Paint += new System.Windows.Forms.PaintEventHandler(this.PlotPanel5_Paint);
            // 
            // PlotPanel6
            // 
            this.PlotPanel6.BackColor = System.Drawing.Color.White;
            this.PlotPanel6.Location = new System.Drawing.Point(729, 758);
            this.PlotPanel6.Margin = new System.Windows.Forms.Padding(4);
            this.PlotPanel6.Name = "PlotPanel6";
            this.PlotPanel6.Size = new System.Drawing.Size(721, 290);
            this.PlotPanel6.TabIndex = 302;
            this.PlotPanel6.TabStop = false;
            this.PlotPanel6.Paint += new System.Windows.Forms.PaintEventHandler(this.PlotPanel6_Paint);
            // 
            // FOVLabel
            // 
            this.FOVLabel.AutoSize = true;
            this.FOVLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FOVLabel.Location = new System.Drawing.Point(1503, 568);
            this.FOVLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.FOVLabel.Name = "FOVLabel";
            this.FOVLabel.Size = new System.Drawing.Size(146, 25);
            this.FOVLabel.TabIndex = 303;
            this.FOVLabel.Text = "Objective lens";
            // 
            // Objective_Pulldown
            // 
            this.Objective_Pulldown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Objective_Pulldown.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Objective_Pulldown.FormattingEnabled = true;
            this.Objective_Pulldown.Items.AddRange(new object[] {
            "x16",
            "x20",
            "x40",
            "x60"});
            this.Objective_Pulldown.Location = new System.Drawing.Point(1521, 596);
            this.Objective_Pulldown.Name = "Objective_Pulldown";
            this.Objective_Pulldown.Size = new System.Drawing.Size(117, 39);
            this.Objective_Pulldown.TabIndex = 318;
            // 
            // FastZ_Calibration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1726, 1058);
            this.Controls.Add(this.Objective_Pulldown);
            this.Controls.Add(this.FOVLabel);
            this.Controls.Add(this.PlotPanel6);
            this.Controls.Add(this.PlotPanel5);
            this.Controls.Add(this.PlotPanel4);
            this.Controls.Add(this.AllSlices);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.IntensityThreshold);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.FitData2);
            this.Controls.Add(this.PlotPanel3);
            this.Controls.Add(this.PlotPanel2);
            this.Controls.Add(this.FitData);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.FastZStack_End);
            this.Controls.Add(this.FastZStack_Start);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.CalcButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ZStack_End);
            this.Controls.Add(this.ZStack_Start);
            this.Controls.Add(this.label104);
            this.Controls.Add(this.PlotPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FastZ_Calibration";
            this.Text = "FastZ_Calibration";
            this.Shown += new System.EventHandler(this.FastZ_Calibration_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.PlotPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlotPanel2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlotPanel3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlotPanel4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlotPanel5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlotPanel6)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.PictureBox PlotPanel;
        public System.Windows.Forms.TextBox ZStack_Start;
        public System.Windows.Forms.Label label104;
        public System.Windows.Forms.TextBox ZStack_End;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Button CalcButton;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox FastZStack_End;
        public System.Windows.Forms.TextBox FastZStack_Start;
        public System.Windows.Forms.Label label5;
        public System.Windows.Forms.Label label6;
        public System.Windows.Forms.Label FitData;
        public System.Windows.Forms.PictureBox PlotPanel2;
        public System.Windows.Forms.PictureBox PlotPanel3;
        public System.Windows.Forms.Label FitData2;
        public System.Windows.Forms.Label label7;
        public System.Windows.Forms.TextBox IntensityThreshold;
        public System.Windows.Forms.Label label8;
        public System.Windows.Forms.CheckBox AllSlices;
        public System.Windows.Forms.PictureBox PlotPanel4;
        public System.Windows.Forms.PictureBox PlotPanel5;
        public System.Windows.Forms.PictureBox PlotPanel6;
        public System.Windows.Forms.Label FOVLabel;
        public System.Windows.Forms.ComboBox Objective_Pulldown;
    }
}