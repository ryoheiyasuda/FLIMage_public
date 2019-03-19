namespace FLIMimage
{
    partial class FastZControl
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FastZControl));
            this.label96 = new System.Windows.Forms.Label();
            this.US_Per_ZScan = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.AdjustFillFraction = new System.Windows.Forms.Button();
            this.NFastZSlices = new System.Windows.Forms.TextBox();
            this.label104 = new System.Windows.Forms.Label();
            this.EnableTagScan = new System.Windows.Forms.CheckBox();
            this.VoxelCount = new System.Windows.Forms.TextBox();
            this.label103 = new System.Windows.Forms.Label();
            this.VoxelTimeUS = new System.Windows.Forms.TextBox();
            this.label102 = new System.Windows.Forms.Label();
            this.ZPixelsPerLine = new System.Windows.Forms.TextBox();
            this.label100 = new System.Windows.Forms.Label();
            this.ZScanPerPixel = new System.Windows.Forms.TextBox();
            this.label99 = new System.Windows.Forms.Label();
            this.FastScanFreq = new System.Windows.Forms.Label();
            this.label98 = new System.Windows.Forms.Label();
            this.ZScanPerLine = new System.Windows.Forms.TextBox();
            this.PhaseDetecCB = new System.Windows.Forms.CheckBox();
            this.TagScanPanel = new System.Windows.Forms.GroupBox();
            this.FastZSliceStep = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.PhaseRangeEnd = new System.Windows.Forms.TextBox();
            this.PhaseRangeStart = new System.Windows.Forms.TextBox();
            this.FreqKHz = new System.Windows.Forms.TextBox();
            this.FastZScanMsPerLine = new System.Windows.Forms.TextBox();
            this.SetFrequency_Pulldown = new System.Windows.Forms.ComboBox();
            this.Connect_button = new System.Windows.Forms.Button();
            this.LockResonance = new System.Windows.Forms.Button();
            this.ComportPulldown = new System.Windows.Forms.ComboBox();
            this.PowerButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Freq_Label = new System.Windows.Forms.Label();
            this.PhaseTextBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.dataAcqTimer = new System.Windows.Forms.Timer(this.components);
            this.AmplitudeLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label_25 = new System.Windows.Forms.Label();
            this.label_26 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.RMSC_Label = new System.Windows.Forms.Label();
            this.RMSV_Label = new System.Windows.Forms.Label();
            this.RealPowerMW = new System.Windows.Forms.Label();
            this.ImgPowermVA = new System.Windows.Forms.Label();
            this.LensPhase = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.AmplitudeEditBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.RGBPhase1 = new System.Windows.Forms.Label();
            this.RGBPhase2 = new System.Windows.Forms.Label();
            this.PhaseTextBox2 = new System.Windows.Forms.TextBox();
            this.RGBPhase3 = new System.Windows.Forms.Label();
            this.PhaseTextBox3 = new System.Windows.Forms.TextBox();
            this.Preset_Pulldown = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.CountPerZScanEB = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ResidualEB = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.Residual_deg = new System.Windows.Forms.Label();
            this.TagScanPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label96
            // 
            this.label96.AutoSize = true;
            this.label96.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label96.Location = new System.Drawing.Point(17, 77);
            this.label96.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label96.Name = "label96";
            this.label96.Size = new System.Drawing.Size(74, 14);
            this.label96.TabIndex = 334;
            this.label96.Text = "Line time (ms)";
            // 
            // US_Per_ZScan
            // 
            this.US_Per_ZScan.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.US_Per_ZScan.Location = new System.Drawing.Point(241, 74);
            this.US_Per_ZScan.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.US_Per_ZScan.Name = "US_Per_ZScan";
            this.US_Per_ZScan.ReadOnly = true;
            this.US_Per_ZScan.Size = new System.Drawing.Size(50, 20);
            this.US_Per_ZScan.TabIndex = 281;
            this.US_Per_ZScan.Text = "3.00";
            this.US_Per_ZScan.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(152, 77);
            this.label12.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(87, 14);
            this.label12.TabIndex = 280;
            this.label12.Text = "Z-scan time (μs)";
            // 
            // AdjustFillFraction
            // 
            this.AdjustFillFraction.Location = new System.Drawing.Point(235, 133);
            this.AdjustFillFraction.Name = "AdjustFillFraction";
            this.AdjustFillFraction.Size = new System.Drawing.Size(68, 23);
            this.AdjustFillFraction.TabIndex = 279;
            this.AdjustFillFraction.Text = "Measure";
            this.AdjustFillFraction.UseVisualStyleBackColor = true;
            this.AdjustFillFraction.Click += new System.EventHandler(this.AdjustParameters_Click);
            // 
            // NFastZSlices
            // 
            this.NFastZSlices.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NFastZSlices.Location = new System.Drawing.Point(92, 124);
            this.NFastZSlices.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.NFastZSlices.Name = "NFastZSlices";
            this.NFastZSlices.Size = new System.Drawing.Size(50, 20);
            this.NFastZSlices.TabIndex = 278;
            this.NFastZSlices.Text = "0";
            this.NFastZSlices.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.NFastZSlices.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // label104
            // 
            this.label104.AutoSize = true;
            this.label104.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label104.Location = new System.Drawing.Point(14, 127);
            this.label104.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label104.Name = "label104";
            this.label104.Size = new System.Drawing.Size(76, 14);
            this.label104.TabIndex = 277;
            this.label104.Text = "#Fast Z Slices";
            // 
            // EnableTagScan
            // 
            this.EnableTagScan.AutoSize = true;
            this.EnableTagScan.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EnableTagScan.Location = new System.Drawing.Point(196, 168);
            this.EnableTagScan.Name = "EnableTagScan";
            this.EnableTagScan.Size = new System.Drawing.Size(107, 18);
            this.EnableTagScan.TabIndex = 273;
            this.EnableTagScan.Text = "Enable Tag-Scan";
            this.EnableTagScan.UseVisualStyleBackColor = true;
            this.EnableTagScan.Click += new System.EventHandler(this.EnableTagScan_Click);
            // 
            // VoxelCount
            // 
            this.VoxelCount.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VoxelCount.Location = new System.Drawing.Point(241, 55);
            this.VoxelCount.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.VoxelCount.Name = "VoxelCount";
            this.VoxelCount.ReadOnly = true;
            this.VoxelCount.Size = new System.Drawing.Size(50, 20);
            this.VoxelCount.TabIndex = 276;
            this.VoxelCount.Text = "4";
            this.VoxelCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label103
            // 
            this.label103.AutoSize = true;
            this.label103.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label103.Location = new System.Drawing.Point(168, 57);
            this.label103.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label103.Name = "label103";
            this.label103.Size = new System.Drawing.Size(71, 14);
            this.label103.TabIndex = 275;
            this.label103.Text = "Count / Voxel";
            // 
            // VoxelTimeUS
            // 
            this.VoxelTimeUS.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VoxelTimeUS.Location = new System.Drawing.Point(92, 36);
            this.VoxelTimeUS.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.VoxelTimeUS.Name = "VoxelTimeUS";
            this.VoxelTimeUS.ReadOnly = true;
            this.VoxelTimeUS.Size = new System.Drawing.Size(50, 20);
            this.VoxelTimeUS.TabIndex = 274;
            this.VoxelTimeUS.Text = "0";
            this.VoxelTimeUS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label102
            // 
            this.label102.AutoSize = true;
            this.label102.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label102.Location = new System.Drawing.Point(12, 38);
            this.label102.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label102.Name = "label102";
            this.label102.Size = new System.Drawing.Size(79, 14);
            this.label102.TabIndex = 273;
            this.label102.Text = "Voxel time (μs)";
            // 
            // ZPixelsPerLine
            // 
            this.ZPixelsPerLine.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZPixelsPerLine.Location = new System.Drawing.Point(241, 36);
            this.ZPixelsPerLine.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.ZPixelsPerLine.Name = "ZPixelsPerLine";
            this.ZPixelsPerLine.ReadOnly = true;
            this.ZPixelsPerLine.Size = new System.Drawing.Size(50, 20);
            this.ZPixelsPerLine.TabIndex = 271;
            this.ZPixelsPerLine.Text = "4";
            this.ZPixelsPerLine.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label100
            // 
            this.label100.AutoSize = true;
            this.label100.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label100.Location = new System.Drawing.Point(181, 39);
            this.label100.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label100.Name = "label100";
            this.label100.Size = new System.Drawing.Size(58, 14);
            this.label100.TabIndex = 270;
            this.label100.Text = "Pixels/Line";
            // 
            // ZScanPerPixel
            // 
            this.ZScanPerPixel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZScanPerPixel.Location = new System.Drawing.Point(241, 17);
            this.ZScanPerPixel.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.ZScanPerPixel.Name = "ZScanPerPixel";
            this.ZScanPerPixel.ReadOnly = true;
            this.ZScanPerPixel.Size = new System.Drawing.Size(50, 20);
            this.ZScanPerPixel.TabIndex = 269;
            this.ZScanPerPixel.Text = "4";
            this.ZScanPerPixel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label99
            // 
            this.label99.AutoSize = true;
            this.label99.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label99.Location = new System.Drawing.Point(165, 20);
            this.label99.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label99.Name = "label99";
            this.label99.Size = new System.Drawing.Size(74, 14);
            this.label99.TabIndex = 268;
            this.label99.Text = "Z-Scan / pixel";
            // 
            // FastScanFreq
            // 
            this.FastScanFreq.AutoSize = true;
            this.FastScanFreq.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FastScanFreq.Location = new System.Drawing.Point(31, 58);
            this.FastScanFreq.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.FastScanFreq.Name = "FastScanFreq";
            this.FastScanFreq.Size = new System.Drawing.Size(60, 14);
            this.FastScanFreq.TabIndex = 267;
            this.FastScanFreq.Text = "Freq (KHz)";
            // 
            // label98
            // 
            this.label98.AutoSize = true;
            this.label98.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label98.Location = new System.Drawing.Point(23, 20);
            this.label98.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label98.Name = "label98";
            this.label98.Size = new System.Drawing.Size(68, 14);
            this.label98.TabIndex = 266;
            this.label98.Text = "Z-Scan / line";
            // 
            // ZScanPerLine
            // 
            this.ZScanPerLine.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZScanPerLine.Location = new System.Drawing.Point(92, 17);
            this.ZScanPerLine.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.ZScanPerLine.Name = "ZScanPerLine";
            this.ZScanPerLine.ReadOnly = true;
            this.ZScanPerLine.Size = new System.Drawing.Size(50, 20);
            this.ZScanPerLine.TabIndex = 265;
            this.ZScanPerLine.Text = "0";
            this.ZScanPerLine.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // PhaseDetecCB
            // 
            this.PhaseDetecCB.AutoSize = true;
            this.PhaseDetecCB.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PhaseDetecCB.Location = new System.Drawing.Point(16, 200);
            this.PhaseDetecCB.Name = "PhaseDetecCB";
            this.PhaseDetecCB.Size = new System.Drawing.Size(195, 18);
            this.PhaseDetecCB.TabIndex = 286;
            this.PhaseDetecCB.Text = "Phase detection mode (0 - 360deg)";
            this.PhaseDetecCB.UseVisualStyleBackColor = true;
            this.PhaseDetecCB.Click += new System.EventHandler(this.PhaseDetecCB_Click);
            // 
            // TagScanPanel
            // 
            this.TagScanPanel.Controls.Add(this.Residual_deg);
            this.TagScanPanel.Controls.Add(this.label11);
            this.TagScanPanel.Controls.Add(this.ResidualEB);
            this.TagScanPanel.Controls.Add(this.label10);
            this.TagScanPanel.Controls.Add(this.CountPerZScanEB);
            this.TagScanPanel.Controls.Add(this.label3);
            this.TagScanPanel.Controls.Add(this.FastZSliceStep);
            this.TagScanPanel.Controls.Add(this.label9);
            this.TagScanPanel.Controls.Add(this.PhaseRangeEnd);
            this.TagScanPanel.Controls.Add(this.PhaseRangeStart);
            this.TagScanPanel.Controls.Add(this.FreqKHz);
            this.TagScanPanel.Controls.Add(this.FastZScanMsPerLine);
            this.TagScanPanel.Controls.Add(this.label96);
            this.TagScanPanel.Controls.Add(this.US_Per_ZScan);
            this.TagScanPanel.Controls.Add(this.label12);
            this.TagScanPanel.Controls.Add(this.AdjustFillFraction);
            this.TagScanPanel.Controls.Add(this.NFastZSlices);
            this.TagScanPanel.Controls.Add(this.label104);
            this.TagScanPanel.Controls.Add(this.EnableTagScan);
            this.TagScanPanel.Controls.Add(this.VoxelCount);
            this.TagScanPanel.Controls.Add(this.label103);
            this.TagScanPanel.Controls.Add(this.VoxelTimeUS);
            this.TagScanPanel.Controls.Add(this.label102);
            this.TagScanPanel.Controls.Add(this.ZPixelsPerLine);
            this.TagScanPanel.Controls.Add(this.label100);
            this.TagScanPanel.Controls.Add(this.ZScanPerPixel);
            this.TagScanPanel.Controls.Add(this.label99);
            this.TagScanPanel.Controls.Add(this.FastScanFreq);
            this.TagScanPanel.Controls.Add(this.label98);
            this.TagScanPanel.Controls.Add(this.ZScanPerLine);
            this.TagScanPanel.Location = new System.Drawing.Point(11, 7);
            this.TagScanPanel.Name = "TagScanPanel";
            this.TagScanPanel.Size = new System.Drawing.Size(310, 189);
            this.TagScanPanel.TabIndex = 285;
            this.TagScanPanel.TabStop = false;
            this.TagScanPanel.Text = "Tag-lens parameters";
            // 
            // FastZSliceStep
            // 
            this.FastZSliceStep.AutoSize = true;
            this.FastZSliceStep.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FastZSliceStep.Location = new System.Drawing.Point(16, 169);
            this.FastZSliceStep.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.FastZSliceStep.Name = "FastZSliceStep";
            this.FastZSliceStep.Size = new System.Drawing.Size(103, 14);
            this.FastZSliceStep.TabIndex = 341;
            this.FastZSliceStep.Text = "Slice = 5 um (5 deg)";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(25, 146);
            this.label9.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(62, 14);
            this.label9.TabIndex = 340;
            this.label9.Text = "Phase from";
            // 
            // PhaseRangeEnd
            // 
            this.PhaseRangeEnd.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PhaseRangeEnd.Location = new System.Drawing.Point(165, 143);
            this.PhaseRangeEnd.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PhaseRangeEnd.Name = "PhaseRangeEnd";
            this.PhaseRangeEnd.Size = new System.Drawing.Size(50, 20);
            this.PhaseRangeEnd.TabIndex = 339;
            this.PhaseRangeEnd.Text = "150";
            this.PhaseRangeEnd.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.PhaseRangeEnd.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // PhaseRangeStart
            // 
            this.PhaseRangeStart.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PhaseRangeStart.Location = new System.Drawing.Point(92, 143);
            this.PhaseRangeStart.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PhaseRangeStart.Name = "PhaseRangeStart";
            this.PhaseRangeStart.Size = new System.Drawing.Size(50, 20);
            this.PhaseRangeStart.TabIndex = 338;
            this.PhaseRangeStart.Text = "35";
            this.PhaseRangeStart.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.PhaseRangeStart.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // FreqKHz
            // 
            this.FreqKHz.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FreqKHz.Location = new System.Drawing.Point(92, 55);
            this.FreqKHz.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.FreqKHz.Name = "FreqKHz";
            this.FreqKHz.ReadOnly = true;
            this.FreqKHz.Size = new System.Drawing.Size(50, 20);
            this.FreqKHz.TabIndex = 337;
            this.FreqKHz.Text = "0";
            this.FreqKHz.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // FastZScanMsPerLine
            // 
            this.FastZScanMsPerLine.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FastZScanMsPerLine.Location = new System.Drawing.Point(92, 74);
            this.FastZScanMsPerLine.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.FastZScanMsPerLine.Name = "FastZScanMsPerLine";
            this.FastZScanMsPerLine.ReadOnly = true;
            this.FastZScanMsPerLine.Size = new System.Drawing.Size(50, 20);
            this.FastZScanMsPerLine.TabIndex = 336;
            this.FastZScanMsPerLine.Text = "0";
            this.FastZScanMsPerLine.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // SetFrequency_Pulldown
            // 
            this.SetFrequency_Pulldown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SetFrequency_Pulldown.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SetFrequency_Pulldown.FormattingEnabled = true;
            this.SetFrequency_Pulldown.Location = new System.Drawing.Point(329, 47);
            this.SetFrequency_Pulldown.Name = "SetFrequency_Pulldown";
            this.SetFrequency_Pulldown.Size = new System.Drawing.Size(105, 22);
            this.SetFrequency_Pulldown.TabIndex = 287;
            this.SetFrequency_Pulldown.SelectedIndexChanged += new System.EventHandler(this.SetFrequency_Pulldown_SelectedIndexChanged);
            // 
            // Connect_button
            // 
            this.Connect_button.Location = new System.Drawing.Point(436, 5);
            this.Connect_button.Name = "Connect_button";
            this.Connect_button.Size = new System.Drawing.Size(105, 23);
            this.Connect_button.TabIndex = 336;
            this.Connect_button.Text = "Connect";
            this.Connect_button.UseVisualStyleBackColor = true;
            this.Connect_button.Click += new System.EventHandler(this.Connect_button_Click);
            // 
            // LockResonance
            // 
            this.LockResonance.ForeColor = System.Drawing.Color.Red;
            this.LockResonance.Location = new System.Drawing.Point(442, 47);
            this.LockResonance.Name = "LockResonance";
            this.LockResonance.Size = new System.Drawing.Size(155, 23);
            this.LockResonance.TabIndex = 337;
            this.LockResonance.Text = "Not Locked!";
            this.LockResonance.UseVisualStyleBackColor = true;
            this.LockResonance.Click += new System.EventHandler(this.LockResonance_Click);
            // 
            // ComportPulldown
            // 
            this.ComportPulldown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComportPulldown.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComportPulldown.FormattingEnabled = true;
            this.ComportPulldown.Location = new System.Drawing.Point(547, 5);
            this.ComportPulldown.Name = "ComportPulldown";
            this.ComportPulldown.Size = new System.Drawing.Size(90, 22);
            this.ComportPulldown.TabIndex = 338;
            // 
            // PowerButton
            // 
            this.PowerButton.Location = new System.Drawing.Point(329, 5);
            this.PowerButton.Name = "PowerButton";
            this.PowerButton.Size = new System.Drawing.Size(105, 23);
            this.PowerButton.TabIndex = 340;
            this.PowerButton.Text = "Start";
            this.PowerButton.UseVisualStyleBackColor = true;
            this.PowerButton.Click += new System.EventHandler(this.PowerButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(332, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 341;
            this.label1.Text = "Frequency (Hz): ";
            // 
            // Freq_Label
            // 
            this.Freq_Label.AutoSize = true;
            this.Freq_Label.Location = new System.Drawing.Point(416, 32);
            this.Freq_Label.Name = "Freq_Label";
            this.Freq_Label.Size = new System.Drawing.Size(13, 13);
            this.Freq_Label.TabIndex = 342;
            this.Freq_Label.Text = "0";
            // 
            // PhaseTextBox1
            // 
            this.PhaseTextBox1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PhaseTextBox1.Location = new System.Drawing.Point(432, 136);
            this.PhaseTextBox1.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PhaseTextBox1.Name = "PhaseTextBox1";
            this.PhaseTextBox1.Size = new System.Drawing.Size(40, 20);
            this.PhaseTextBox1.TabIndex = 336;
            this.PhaseTextBox1.Text = "30";
            this.PhaseTextBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.PhaseTextBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericEditBox_KeyDow);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(425, 120);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 343;
            this.label2.Text = "Pulse phase (deg)";
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(342, 195);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(40, 13);
            this.StatusLabel.TabIndex = 344;
            this.StatusLabel.Text = "Status:";
            // 
            // dataAcqTimer
            // 
            this.dataAcqTimer.Interval = 1000;
            this.dataAcqTimer.Tick += new System.EventHandler(this.dataAcqTimer_Tick);
            // 
            // AmplitudeLabel
            // 
            this.AmplitudeLabel.AutoSize = true;
            this.AmplitudeLabel.Location = new System.Drawing.Point(361, 159);
            this.AmplitudeLabel.Name = "AmplitudeLabel";
            this.AmplitudeLabel.Size = new System.Drawing.Size(21, 13);
            this.AmplitudeLabel.TabIndex = 346;
            this.AmplitudeLabel.Text = "0%";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(541, 135);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 347;
            this.label4.Text = "RMS (mA):";
            // 
            // label_25
            // 
            this.label_25.AutoSize = true;
            this.label_25.Location = new System.Drawing.Point(549, 148);
            this.label_25.Name = "label_25";
            this.label_25.Size = new System.Drawing.Size(50, 13);
            this.label_25.TabIndex = 348;
            this.label_25.Text = "RMS (V):";
            // 
            // label_26
            // 
            this.label_26.AutoSize = true;
            this.label_26.Location = new System.Drawing.Point(531, 161);
            this.label_26.Name = "label_26";
            this.label_26.Size = new System.Drawing.Size(68, 13);
            this.label_26.TabIndex = 349;
            this.label_26.Text = "Power (mW):";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(528, 174);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(71, 13);
            this.label7.TabIndex = 350;
            this.label7.Text = "Power (mVA):";
            // 
            // RMSC_Label
            // 
            this.RMSC_Label.AutoSize = true;
            this.RMSC_Label.Location = new System.Drawing.Point(606, 135);
            this.RMSC_Label.Name = "RMSC_Label";
            this.RMSC_Label.Size = new System.Drawing.Size(13, 13);
            this.RMSC_Label.TabIndex = 351;
            this.RMSC_Label.Text = "0";
            // 
            // RMSV_Label
            // 
            this.RMSV_Label.AutoSize = true;
            this.RMSV_Label.Location = new System.Drawing.Point(606, 148);
            this.RMSV_Label.Name = "RMSV_Label";
            this.RMSV_Label.Size = new System.Drawing.Size(13, 13);
            this.RMSV_Label.TabIndex = 352;
            this.RMSV_Label.Text = "0";
            // 
            // RealPowerMW
            // 
            this.RealPowerMW.AutoSize = true;
            this.RealPowerMW.Location = new System.Drawing.Point(606, 161);
            this.RealPowerMW.Name = "RealPowerMW";
            this.RealPowerMW.Size = new System.Drawing.Size(13, 13);
            this.RealPowerMW.TabIndex = 353;
            this.RealPowerMW.Text = "0";
            // 
            // ImgPowermVA
            // 
            this.ImgPowermVA.AutoSize = true;
            this.ImgPowermVA.Location = new System.Drawing.Point(606, 174);
            this.ImgPowermVA.Name = "ImgPowermVA";
            this.ImgPowermVA.Size = new System.Drawing.Size(13, 13);
            this.ImgPowermVA.TabIndex = 354;
            this.ImgPowermVA.Text = "0";
            // 
            // LensPhase
            // 
            this.LensPhase.AutoSize = true;
            this.LensPhase.Location = new System.Drawing.Point(606, 187);
            this.LensPhase.Name = "LensPhase";
            this.LensPhase.Size = new System.Drawing.Size(13, 13);
            this.LensPhase.TabIndex = 356;
            this.LensPhase.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(534, 187);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 13);
            this.label6.TabIndex = 355;
            this.label6.Text = "Lens phase:";
            // 
            // AmplitudeEditBox
            // 
            this.AmplitudeEditBox.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AmplitudeEditBox.Location = new System.Drawing.Point(347, 136);
            this.AmplitudeEditBox.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.AmplitudeEditBox.Name = "AmplitudeEditBox";
            this.AmplitudeEditBox.Size = new System.Drawing.Size(40, 20);
            this.AmplitudeEditBox.TabIndex = 357;
            this.AmplitudeEditBox.Text = "5";
            this.AmplitudeEditBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.AmplitudeEditBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericEditBox_KeyDow);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(333, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 358;
            this.label5.Text = "Amplitude (%)";
            // 
            // RGBPhase1
            // 
            this.RGBPhase1.AutoSize = true;
            this.RGBPhase1.Location = new System.Drawing.Point(476, 140);
            this.RGBPhase1.Name = "RGBPhase1";
            this.RGBPhase1.Size = new System.Drawing.Size(13, 13);
            this.RGBPhase1.TabIndex = 360;
            this.RGBPhase1.Text = "0";
            // 
            // RGBPhase2
            // 
            this.RGBPhase2.AutoSize = true;
            this.RGBPhase2.Location = new System.Drawing.Point(476, 160);
            this.RGBPhase2.Name = "RGBPhase2";
            this.RGBPhase2.Size = new System.Drawing.Size(13, 13);
            this.RGBPhase2.TabIndex = 362;
            this.RGBPhase2.Text = "0";
            // 
            // PhaseTextBox2
            // 
            this.PhaseTextBox2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PhaseTextBox2.Location = new System.Drawing.Point(432, 156);
            this.PhaseTextBox2.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PhaseTextBox2.Name = "PhaseTextBox2";
            this.PhaseTextBox2.Size = new System.Drawing.Size(40, 20);
            this.PhaseTextBox2.TabIndex = 361;
            this.PhaseTextBox2.Text = "30";
            this.PhaseTextBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.PhaseTextBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericEditBox_KeyDow);
            // 
            // RGBPhase3
            // 
            this.RGBPhase3.AutoSize = true;
            this.RGBPhase3.Location = new System.Drawing.Point(476, 180);
            this.RGBPhase3.Name = "RGBPhase3";
            this.RGBPhase3.Size = new System.Drawing.Size(13, 13);
            this.RGBPhase3.TabIndex = 364;
            this.RGBPhase3.Text = "0";
            // 
            // PhaseTextBox3
            // 
            this.PhaseTextBox3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PhaseTextBox3.Location = new System.Drawing.Point(432, 176);
            this.PhaseTextBox3.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.PhaseTextBox3.Name = "PhaseTextBox3";
            this.PhaseTextBox3.Size = new System.Drawing.Size(40, 20);
            this.PhaseTextBox3.TabIndex = 363;
            this.PhaseTextBox3.Text = "30";
            this.PhaseTextBox3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.PhaseTextBox3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenericEditBox_KeyDow);
            // 
            // Preset_Pulldown
            // 
            this.Preset_Pulldown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Preset_Pulldown.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Preset_Pulldown.FormattingEnabled = true;
            this.Preset_Pulldown.Location = new System.Drawing.Point(379, 77);
            this.Preset_Pulldown.Name = "Preset_Pulldown";
            this.Preset_Pulldown.Size = new System.Drawing.Size(215, 22);
            this.Preset_Pulldown.TabIndex = 365;
            this.Preset_Pulldown.SelectedIndexChanged += new System.EventHandler(this.Preset_Pulldown_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(333, 81);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(40, 13);
            this.label8.TabIndex = 366;
            this.label8.Text = "Pre-set";
            // 
            // CountPerZScanEB
            // 
            this.CountPerZScanEB.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CountPerZScanEB.Location = new System.Drawing.Point(92, 93);
            this.CountPerZScanEB.Margin = new System.Windows.Forms.Padding(1);
            this.CountPerZScanEB.Name = "CountPerZScanEB";
            this.CountPerZScanEB.ReadOnly = true;
            this.CountPerZScanEB.Size = new System.Drawing.Size(50, 20);
            this.CountPerZScanEB.TabIndex = 344;
            this.CountPerZScanEB.Text = "0";
            this.CountPerZScanEB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(15, 96);
            this.label3.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 14);
            this.label3.TabIndex = 343;
            this.label3.Text = "Count / ZScan";
            // 
            // ResidualEB
            // 
            this.ResidualEB.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResidualEB.Location = new System.Drawing.Point(241, 93);
            this.ResidualEB.Margin = new System.Windows.Forms.Padding(1);
            this.ResidualEB.Name = "ResidualEB";
            this.ResidualEB.ReadOnly = true;
            this.ResidualEB.Size = new System.Drawing.Size(50, 20);
            this.ResidualEB.TabIndex = 346;
            this.ResidualEB.Text = "0";
            this.ResidualEB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(191, 96);
            this.label10.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(48, 14);
            this.label10.TabIndex = 345;
            this.label10.Text = "Residual";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(145, 146);
            this.label11.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(18, 14);
            this.label11.TabIndex = 347;
            this.label11.Text = "To";
            // 
            // Residual_deg
            // 
            this.Residual_deg.AutoSize = true;
            this.Residual_deg.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Residual_deg.Location = new System.Drawing.Point(242, 115);
            this.Residual_deg.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Residual_deg.Name = "Residual_deg";
            this.Residual_deg.Size = new System.Drawing.Size(48, 14);
            this.Residual_deg.TabIndex = 348;
            this.Residual_deg.Text = "Residual";
            // 
            // FastZControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(666, 220);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.Preset_Pulldown);
            this.Controls.Add(this.RGBPhase3);
            this.Controls.Add(this.PhaseTextBox3);
            this.Controls.Add(this.RGBPhase2);
            this.Controls.Add(this.PhaseTextBox2);
            this.Controls.Add(this.RGBPhase1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.AmplitudeEditBox);
            this.Controls.Add(this.LensPhase);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ImgPowermVA);
            this.Controls.Add(this.RealPowerMW);
            this.Controls.Add(this.RMSV_Label);
            this.Controls.Add(this.RMSC_Label);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label_26);
            this.Controls.Add(this.label_25);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.AmplitudeLabel);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.PhaseTextBox1);
            this.Controls.Add(this.Freq_Label);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PowerButton);
            this.Controls.Add(this.ComportPulldown);
            this.Controls.Add(this.LockResonance);
            this.Controls.Add(this.SetFrequency_Pulldown);
            this.Controls.Add(this.Connect_button);
            this.Controls.Add(this.PhaseDetecCB);
            this.Controls.Add(this.TagScanPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FastZControl";
            this.Text = "FastZControl";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FastZControl_FormClosing);
            this.Load += new System.EventHandler(this.FastZControl_Load);
            this.Shown += new System.EventHandler(this.FastZControl_Shown);
            this.TagScanPanel.ResumeLayout(false);
            this.TagScanPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label96;
        private System.Windows.Forms.TextBox US_Per_ZScan;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button AdjustFillFraction;
        private System.Windows.Forms.TextBox NFastZSlices;
        private System.Windows.Forms.Label label104;
        private System.Windows.Forms.CheckBox EnableTagScan;
        private System.Windows.Forms.TextBox VoxelCount;
        private System.Windows.Forms.Label label103;
        private System.Windows.Forms.TextBox VoxelTimeUS;
        private System.Windows.Forms.Label label102;
        private System.Windows.Forms.TextBox ZPixelsPerLine;
        private System.Windows.Forms.Label label100;
        private System.Windows.Forms.TextBox ZScanPerPixel;
        private System.Windows.Forms.Label label99;
        private System.Windows.Forms.Label FastScanFreq;
        private System.Windows.Forms.Label label98;
        private System.Windows.Forms.TextBox ZScanPerLine;
        private System.Windows.Forms.CheckBox PhaseDetecCB;
        private System.Windows.Forms.GroupBox TagScanPanel;
        private System.Windows.Forms.ComboBox SetFrequency_Pulldown;
        private System.Windows.Forms.Button Connect_button;
        private System.Windows.Forms.Button LockResonance;
        private System.Windows.Forms.ComboBox ComportPulldown;
        private System.Windows.Forms.Button PowerButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label Freq_Label;
        private System.Windows.Forms.TextBox PhaseTextBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.Timer dataAcqTimer;
        private System.Windows.Forms.Label AmplitudeLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label_25;
        private System.Windows.Forms.Label label_26;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label RMSC_Label;
        private System.Windows.Forms.Label RMSV_Label;
        private System.Windows.Forms.Label RealPowerMW;
        private System.Windows.Forms.Label ImgPowermVA;
        private System.Windows.Forms.Label LensPhase;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox AmplitudeEditBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label RGBPhase1;
        private System.Windows.Forms.Label RGBPhase2;
        private System.Windows.Forms.TextBox PhaseTextBox2;
        private System.Windows.Forms.Label RGBPhase3;
        private System.Windows.Forms.TextBox PhaseTextBox3;
        private System.Windows.Forms.TextBox FreqKHz;
        private System.Windows.Forms.TextBox FastZScanMsPerLine;
        private System.Windows.Forms.ComboBox Preset_Pulldown;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox PhaseRangeEnd;
        private System.Windows.Forms.TextBox PhaseRangeStart;
        private System.Windows.Forms.Label FastZSliceStep;
        private System.Windows.Forms.TextBox CountPerZScanEB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ResidualEB;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label Residual_deg;
    }
}