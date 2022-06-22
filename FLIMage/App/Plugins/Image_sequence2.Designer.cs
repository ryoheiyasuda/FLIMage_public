namespace FLIMage.Plugins
{
    partial class Image_sequence2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Image_sequence2));
            this.PlotBox = new System.Windows.Forms.PictureBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.ReloadTemplate = new System.Windows.Forms.Button();
            this.PositionDataGridView = new System.Windows.Forms.DataGridView();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.X = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PositionY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Z = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AcquireTemplateImageButton = new System.Windows.Forms.Button();
            this.Pos_Delete = new System.Windows.Forms.Button();
            this.Pos_ClearAll = new System.Windows.Forms.Button();
            this.Pos_Goto = new System.Windows.Forms.Button();
            this.RecalculateDrift = new System.Windows.Forms.Button();
            this.Status_Motor = new System.Windows.Forms.Label();
            this.Status_V = new System.Windows.Forms.Label();
            this.Status_XYZ_um = new System.Windows.Forms.Label();
            this.Status_XY = new System.Windows.Forms.Label();
            this.TemplateImage_PB = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Channel_Pulldown = new System.Windows.Forms.ComboBox();
            this.TemplateFileName = new System.Windows.Forms.Label();
            this.SelectCurrentImage = new System.Windows.Forms.Button();
            this.UseMirror_CB = new System.Windows.Forms.CheckBox();
            this.ZCorrect_CB = new System.Windows.Forms.CheckBox();
            this.XYCorrect_CB = new System.Windows.Forms.CheckBox();
            this.DriftCorrection_CB = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.TimeShift_textBox = new System.Windows.Forms.TextBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.setting_groupBox = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.nAveFrame_textBox = new System.Windows.Forms.TextBox();
            this.nSlices_textBox = new System.Windows.Forms.TextBox();
            this.aveFrame_checkBox = new System.Windows.Forms.CheckBox();
            this.sliceStep_textBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.nFrames_textBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.ZStack_checkBox = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ZoomTextBox = new System.Windows.Forms.TextBox();
            this.AddCurrentSetting = new System.Windows.Forms.Button();
            this.ClearSetting = new System.Windows.Forms.Button();
            this.LoadSelected = new System.Windows.Forms.Button();
            this.RowUp = new System.Windows.Forms.Button();
            this.ReplaceWithCurrent = new System.Windows.Forms.Button();
            this.RowDown = new System.Windows.Forms.Button();
            this.ImageSequenceGridView = new System.Windows.Forms.DataGridView();
            this.SettingID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SettingName1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Interval = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Repetition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Exclusive = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.DeleteRow = new System.Windows.Forms.Button();
            this.ForwardButton = new System.Windows.Forms.Button();
            this.BackButton = new System.Windows.Forms.Button();
            this.NPosition_Label = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.StartFromCurrentCheck = new System.Windows.Forms.CheckBox();
            this.PauseButton = new System.Windows.Forms.Button();
            this.totalTimeLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.RunSeq = new System.Windows.Forms.Button();
            this.LoopCheck = new System.Windows.Forms.CheckBox();
            this.RepNumberLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.RepTime = new System.Windows.Forms.Label();
            this.HomeTab = new System.Windows.Forms.TabControl();
            this.SortFileNames_checkBox = new System.Windows.Forms.CheckBox();
            this.NewFileName = new System.Windows.Forms.Label();
            this.Analyze_checkBox = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.PlotBox)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PositionDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TemplateImage_PB)).BeginInit();
            this.tabPage1.SuspendLayout();
            this.setting_groupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageSequenceGridView)).BeginInit();
            this.HomeTab.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PlotBox
            // 
            this.PlotBox.BackColor = System.Drawing.Color.White;
            this.PlotBox.Location = new System.Drawing.Point(610, 34);
            this.PlotBox.Name = "PlotBox";
            this.PlotBox.Size = new System.Drawing.Size(589, 350);
            this.PlotBox.TabIndex = 22;
            this.PlotBox.TabStop = false;
            this.PlotBox.Paint += new System.Windows.Forms.PaintEventHandler(this.PlotBox_Paint);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.ReloadTemplate);
            this.tabPage2.Controls.Add(this.PositionDataGridView);
            this.tabPage2.Controls.Add(this.AcquireTemplateImageButton);
            this.tabPage2.Controls.Add(this.Pos_Delete);
            this.tabPage2.Controls.Add(this.Pos_ClearAll);
            this.tabPage2.Controls.Add(this.Pos_Goto);
            this.tabPage2.Controls.Add(this.RecalculateDrift);
            this.tabPage2.Controls.Add(this.Status_Motor);
            this.tabPage2.Controls.Add(this.Status_V);
            this.tabPage2.Controls.Add(this.Status_XYZ_um);
            this.tabPage2.Controls.Add(this.Status_XY);
            this.tabPage2.Controls.Add(this.TemplateImage_PB);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.Channel_Pulldown);
            this.tabPage2.Controls.Add(this.TemplateFileName);
            this.tabPage2.Controls.Add(this.SelectCurrentImage);
            this.tabPage2.Controls.Add(this.UseMirror_CB);
            this.tabPage2.Controls.Add(this.ZCorrect_CB);
            this.tabPage2.Controls.Add(this.XYCorrect_CB);
            this.tabPage2.Controls.Add(this.DriftCorrection_CB);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(595, 431);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Position / Drift corr";
            // 
            // ReloadTemplate
            // 
            this.ReloadTemplate.Location = new System.Drawing.Point(372, 387);
            this.ReloadTemplate.Name = "ReloadTemplate";
            this.ReloadTemplate.Size = new System.Drawing.Size(69, 23);
            this.ReloadTemplate.TabIndex = 302;
            this.ReloadTemplate.Text = "Reload";
            this.ReloadTemplate.UseVisualStyleBackColor = true;
            this.ReloadTemplate.Click += new System.EventHandler(this.ReloadTemplate_Click);
            // 
            // PositionDataGridView
            // 
            this.PositionDataGridView.AllowUserToAddRows = false;
            this.PositionDataGridView.AllowUserToDeleteRows = false;
            this.PositionDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.PositionDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PositionDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.X,
            this.PositionY,
            this.Z});
            this.PositionDataGridView.Location = new System.Drawing.Point(5, 136);
            this.PositionDataGridView.Name = "PositionDataGridView";
            this.PositionDataGridView.ReadOnly = true;
            this.PositionDataGridView.Size = new System.Drawing.Size(317, 150);
            this.PositionDataGridView.TabIndex = 301;
            this.PositionDataGridView.CurrentCellChanged += new System.EventHandler(this.PositionDataGridView_CurrentCellChanged);
            // 
            // ID
            // 
            this.ID.HeaderText = "ID";
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            // 
            // X
            // 
            this.X.HeaderText = "X";
            this.X.Name = "X";
            this.X.ReadOnly = true;
            // 
            // PositionY
            // 
            this.PositionY.HeaderText = "Y";
            this.PositionY.Name = "PositionY";
            this.PositionY.ReadOnly = true;
            // 
            // Z
            // 
            this.Z.HeaderText = "Z";
            this.Z.Name = "Z";
            this.Z.ReadOnly = true;
            // 
            // AcquireTemplateImageButton
            // 
            this.AcquireTemplateImageButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AcquireTemplateImageButton.Location = new System.Drawing.Point(330, 298);
            this.AcquireTemplateImageButton.Name = "AcquireTemplateImageButton";
            this.AcquireTemplateImageButton.Size = new System.Drawing.Size(116, 58);
            this.AcquireTemplateImageButton.TabIndex = 300;
            this.AcquireTemplateImageButton.Text = "Acquire image for template";
            this.AcquireTemplateImageButton.UseVisualStyleBackColor = true;
            this.AcquireTemplateImageButton.Click += new System.EventHandler(this.AcquireTemplateImageButton_Click);
            // 
            // Pos_Delete
            // 
            this.Pos_Delete.Location = new System.Drawing.Point(491, 350);
            this.Pos_Delete.Name = "Pos_Delete";
            this.Pos_Delete.Size = new System.Drawing.Size(67, 23);
            this.Pos_Delete.TabIndex = 292;
            this.Pos_Delete.Text = "Delete";
            this.Pos_Delete.UseVisualStyleBackColor = true;
            this.Pos_Delete.Click += new System.EventHandler(this.Pos_Delete_Click);
            // 
            // Pos_ClearAll
            // 
            this.Pos_ClearAll.Location = new System.Drawing.Point(491, 373);
            this.Pos_ClearAll.Name = "Pos_ClearAll";
            this.Pos_ClearAll.Size = new System.Drawing.Size(67, 23);
            this.Pos_ClearAll.TabIndex = 291;
            this.Pos_ClearAll.Text = "Clear all";
            this.Pos_ClearAll.UseVisualStyleBackColor = true;
            this.Pos_ClearAll.Click += new System.EventHandler(this.Pos_ClearAll_Click);
            // 
            // Pos_Goto
            // 
            this.Pos_Goto.Location = new System.Drawing.Point(491, 327);
            this.Pos_Goto.Name = "Pos_Goto";
            this.Pos_Goto.Size = new System.Drawing.Size(67, 23);
            this.Pos_Goto.TabIndex = 290;
            this.Pos_Goto.Text = "GoTo";
            this.Pos_Goto.UseVisualStyleBackColor = true;
            this.Pos_Goto.Click += new System.EventHandler(this.Pos_Goto_Click);
            // 
            // RecalculateDrift
            // 
            this.RecalculateDrift.Location = new System.Drawing.Point(491, 298);
            this.RecalculateDrift.Name = "RecalculateDrift";
            this.RecalculateDrift.Size = new System.Drawing.Size(95, 23);
            this.RecalculateDrift.TabIndex = 289;
            this.RecalculateDrift.Text = "Recalc drift";
            this.RecalculateDrift.UseVisualStyleBackColor = true;
            this.RecalculateDrift.Click += new System.EventHandler(this.RecalculateDrift_Click);
            // 
            // Status_Motor
            // 
            this.Status_Motor.AutoSize = true;
            this.Status_Motor.Location = new System.Drawing.Point(7, 373);
            this.Status_Motor.Name = "Status_Motor";
            this.Status_Motor.Size = new System.Drawing.Size(193, 13);
            this.Status_Motor.TabIndex = 286;
            this.Status_Motor.Text = "Motor position shift: 0.00, 0.00, 0.00 um";
            // 
            // Status_V
            // 
            this.Status_V.AutoSize = true;
            this.Status_V.Location = new System.Drawing.Point(19, 337);
            this.Status_V.Name = "Status_V";
            this.Status_V.Size = new System.Drawing.Size(107, 13);
            this.Status_V.TabIndex = 284;
            this.Status_V.Text = "Voltage: 0.00, 0.00 V";
            // 
            // Status_XYZ_um
            // 
            this.Status_XYZ_um.AutoSize = true;
            this.Status_XYZ_um.Location = new System.Drawing.Point(7, 356);
            this.Status_XYZ_um.Name = "Status_XYZ_um";
            this.Status_XYZ_um.Size = new System.Drawing.Size(171, 13);
            this.Status_XYZ_um.TabIndex = 283;
            this.Status_XYZ_um.Text = "Estimated drift: 0.00, 0.00, 0.00 um";
            // 
            // Status_XY
            // 
            this.Status_XY.AutoSize = true;
            this.Status_XY.Location = new System.Drawing.Point(6, 323);
            this.Status_XY.Name = "Status_XY";
            this.Status_XY.Size = new System.Drawing.Size(111, 13);
            this.Status_XY.TabIndex = 282;
            this.Status_XY.Text = "Image shift: 0, 0 pixels";
            // 
            // TemplateImage_PB
            // 
            this.TemplateImage_PB.BackColor = System.Drawing.Color.Black;
            this.TemplateImage_PB.Location = new System.Drawing.Point(330, 28);
            this.TemplateImage_PB.Name = "TemplateImage_PB";
            this.TemplateImage_PB.Size = new System.Drawing.Size(256, 256);
            this.TemplateImage_PB.TabIndex = 281;
            this.TemplateImage_PB.TabStop = false;
            this.TemplateImage_PB.Paint += new System.Windows.Forms.PaintEventHandler(this.TemplateImage_PB_Paint);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(294, 366);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 13);
            this.label4.TabIndex = 280;
            this.label4.Text = "Channel";
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
            this.Channel_Pulldown.Location = new System.Drawing.Point(345, 362);
            this.Channel_Pulldown.Name = "Channel_Pulldown";
            this.Channel_Pulldown.Size = new System.Drawing.Size(95, 22);
            this.Channel_Pulldown.TabIndex = 279;
            this.Channel_Pulldown.SelectedIndexChanged += new System.EventHandler(this.Channel_Pulldown_SelectedIndexChanged);
            // 
            // TemplateFileName
            // 
            this.TemplateFileName.AutoSize = true;
            this.TemplateFileName.Location = new System.Drawing.Point(6, 301);
            this.TemplateFileName.Name = "TemplateFileName";
            this.TemplateFileName.Size = new System.Drawing.Size(52, 13);
            this.TemplateFileName.TabIndex = 278;
            this.TemplateFileName.Text = "Filename:";
            // 
            // SelectCurrentImage
            // 
            this.SelectCurrentImage.Location = new System.Drawing.Point(146, 389);
            this.SelectCurrentImage.Name = "SelectCurrentImage";
            this.SelectCurrentImage.Size = new System.Drawing.Size(176, 23);
            this.SelectCurrentImage.TabIndex = 277;
            this.SelectCurrentImage.Text = "Add current image for template";
            this.SelectCurrentImage.UseVisualStyleBackColor = true;
            this.SelectCurrentImage.Click += new System.EventHandler(this.setCurrentImageForTemplateToolStripMenuItem_Click);
            // 
            // UseMirror_CB
            // 
            this.UseMirror_CB.AutoSize = true;
            this.UseMirror_CB.Checked = true;
            this.UseMirror_CB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.UseMirror_CB.Location = new System.Drawing.Point(60, 65);
            this.UseMirror_CB.Name = "UseMirror_CB";
            this.UseMirror_CB.Size = new System.Drawing.Size(73, 17);
            this.UseMirror_CB.TabIndex = 276;
            this.UseMirror_CB.Text = "Use mirror";
            this.UseMirror_CB.UseVisualStyleBackColor = true;
            // 
            // ZCorrect_CB
            // 
            this.ZCorrect_CB.AutoSize = true;
            this.ZCorrect_CB.Checked = true;
            this.ZCorrect_CB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ZCorrect_CB.Location = new System.Drawing.Point(48, 88);
            this.ZCorrect_CB.Name = "ZCorrect_CB";
            this.ZCorrect_CB.Size = new System.Drawing.Size(33, 17);
            this.ZCorrect_CB.TabIndex = 275;
            this.ZCorrect_CB.Text = "Z";
            this.ZCorrect_CB.UseVisualStyleBackColor = true;
            // 
            // XYCorrect_CB
            // 
            this.XYCorrect_CB.AutoSize = true;
            this.XYCorrect_CB.Location = new System.Drawing.Point(48, 44);
            this.XYCorrect_CB.Name = "XYCorrect_CB";
            this.XYCorrect_CB.Size = new System.Drawing.Size(40, 17);
            this.XYCorrect_CB.TabIndex = 274;
            this.XYCorrect_CB.Text = "XY";
            this.XYCorrect_CB.UseVisualStyleBackColor = true;
            // 
            // DriftCorrection_CB
            // 
            this.DriftCorrection_CB.AutoSize = true;
            this.DriftCorrection_CB.Location = new System.Drawing.Point(26, 23);
            this.DriftCorrection_CB.Name = "DriftCorrection_CB";
            this.DriftCorrection_CB.Size = new System.Drawing.Size(114, 17);
            this.DriftCorrection_CB.TabIndex = 273;
            this.DriftCorrection_CB.Text = "Drift correction ON";
            this.DriftCorrection_CB.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(620, 443);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(94, 13);
            this.label6.TabIndex = 295;
            this.label6.Text = "Time shifting (min):";
            // 
            // TimeShift_textBox
            // 
            this.TimeShift_textBox.BackColor = System.Drawing.Color.White;
            this.TimeShift_textBox.Location = new System.Drawing.Point(715, 440);
            this.TimeShift_textBox.Name = "TimeShift_textBox";
            this.TimeShift_textBox.Size = new System.Drawing.Size(70, 20);
            this.TimeShift_textBox.TabIndex = 294;
            this.TimeShift_textBox.Text = "2";
            this.TimeShift_textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TimeShift_textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Starggering_textBox_KeyDown);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.setting_groupBox);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.ZoomTextBox);
            this.tabPage1.Controls.Add(this.AddCurrentSetting);
            this.tabPage1.Controls.Add(this.ClearSetting);
            this.tabPage1.Controls.Add(this.LoadSelected);
            this.tabPage1.Controls.Add(this.RowUp);
            this.tabPage1.Controls.Add(this.ReplaceWithCurrent);
            this.tabPage1.Controls.Add(this.RowDown);
            this.tabPage1.Controls.Add(this.ImageSequenceGridView);
            this.tabPage1.Controls.Add(this.DeleteRow);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(595, 431);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Home";
            // 
            // setting_groupBox
            // 
            this.setting_groupBox.Controls.Add(this.label11);
            this.setting_groupBox.Controls.Add(this.label10);
            this.setting_groupBox.Controls.Add(this.nAveFrame_textBox);
            this.setting_groupBox.Controls.Add(this.nSlices_textBox);
            this.setting_groupBox.Controls.Add(this.aveFrame_checkBox);
            this.setting_groupBox.Controls.Add(this.sliceStep_textBox);
            this.setting_groupBox.Controls.Add(this.label8);
            this.setting_groupBox.Controls.Add(this.nFrames_textBox);
            this.setting_groupBox.Controls.Add(this.label9);
            this.setting_groupBox.Controls.Add(this.ZStack_checkBox);
            this.setting_groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.setting_groupBox.Location = new System.Drawing.Point(114, 341);
            this.setting_groupBox.Name = "setting_groupBox";
            this.setting_groupBox.Size = new System.Drawing.Size(288, 85);
            this.setting_groupBox.TabIndex = 308;
            this.setting_groupBox.TabStop = false;
            this.setting_groupBox.Text = "Setting parameters";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(154, 24);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(53, 13);
            this.label11.TabIndex = 307;
            this.label11.Text = "nAverage";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(23, 25);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(47, 13);
            this.label10.TabIndex = 304;
            this.label10.Text = "nFrames";
            // 
            // nAveFrame_textBox
            // 
            this.nAveFrame_textBox.BackColor = System.Drawing.Color.White;
            this.nAveFrame_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nAveFrame_textBox.Location = new System.Drawing.Point(209, 20);
            this.nAveFrame_textBox.Name = "nAveFrame_textBox";
            this.nAveFrame_textBox.Size = new System.Drawing.Size(70, 20);
            this.nAveFrame_textBox.TabIndex = 306;
            this.nAveFrame_textBox.Text = "8";
            this.nAveFrame_textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nAveFrame_textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // nSlices_textBox
            // 
            this.nSlices_textBox.BackColor = System.Drawing.Color.White;
            this.nSlices_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nSlices_textBox.Location = new System.Drawing.Point(72, 40);
            this.nSlices_textBox.Name = "nSlices_textBox";
            this.nSlices_textBox.Size = new System.Drawing.Size(70, 20);
            this.nSlices_textBox.TabIndex = 298;
            this.nSlices_textBox.Text = "5";
            this.nSlices_textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nSlices_textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // aveFrame_checkBox
            // 
            this.aveFrame_checkBox.AutoSize = true;
            this.aveFrame_checkBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.aveFrame_checkBox.Location = new System.Drawing.Point(101, 67);
            this.aveFrame_checkBox.Name = "aveFrame_checkBox";
            this.aveFrame_checkBox.Size = new System.Drawing.Size(100, 17);
            this.aveFrame_checkBox.TabIndex = 305;
            this.aveFrame_checkBox.Text = "Average frames";
            this.aveFrame_checkBox.UseVisualStyleBackColor = true;
            this.aveFrame_checkBox.Click += new System.EventHandler(this.Generic_checkBoxClick);
            // 
            // sliceStep_textBox
            // 
            this.sliceStep_textBox.BackColor = System.Drawing.Color.White;
            this.sliceStep_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sliceStep_textBox.Location = new System.Drawing.Point(209, 39);
            this.sliceStep_textBox.Name = "sliceStep_textBox";
            this.sliceStep_textBox.Size = new System.Drawing.Size(70, 20);
            this.sliceStep_textBox.TabIndex = 299;
            this.sliceStep_textBox.Text = "1";
            this.sliceStep_textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.sliceStep_textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(26, 43);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 13);
            this.label8.TabIndex = 300;
            this.label8.Text = "nSlices";
            // 
            // nFrames_textBox
            // 
            this.nFrames_textBox.BackColor = System.Drawing.Color.White;
            this.nFrames_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nFrames_textBox.Location = new System.Drawing.Point(72, 21);
            this.nFrames_textBox.Name = "nFrames_textBox";
            this.nFrames_textBox.Size = new System.Drawing.Size(70, 20);
            this.nFrames_textBox.TabIndex = 303;
            this.nFrames_textBox.Text = "8";
            this.nFrames_textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nFrames_textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Generic_KeyDown);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(150, 43);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(58, 13);
            this.label9.TabIndex = 301;
            this.label9.Text = "Z step size";
            // 
            // ZStack_checkBox
            // 
            this.ZStack_checkBox.AutoSize = true;
            this.ZStack_checkBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZStack_checkBox.Location = new System.Drawing.Point(34, 67);
            this.ZStack_checkBox.Name = "ZStack_checkBox";
            this.ZStack_checkBox.Size = new System.Drawing.Size(61, 17);
            this.ZStack_checkBox.TabIndex = 302;
            this.ZStack_checkBox.Text = "ZStack";
            this.ZStack_checkBox.UseVisualStyleBackColor = true;
            this.ZStack_checkBox.Click += new System.EventHandler(this.Generic_checkBoxClick);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(466, 346);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 297;
            this.label5.Text = "Zoom";
            // 
            // ZoomTextBox
            // 
            this.ZoomTextBox.BackColor = System.Drawing.Color.White;
            this.ZoomTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZoomTextBox.Location = new System.Drawing.Point(469, 362);
            this.ZoomTextBox.Name = "ZoomTextBox";
            this.ZoomTextBox.Size = new System.Drawing.Size(70, 20);
            this.ZoomTextBox.TabIndex = 296;
            this.ZoomTextBox.Text = "25";
            this.ZoomTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // AddCurrentSetting
            // 
            this.AddCurrentSetting.Location = new System.Drawing.Point(7, 281);
            this.AddCurrentSetting.Name = "AddCurrentSetting";
            this.AddCurrentSetting.Size = new System.Drawing.Size(101, 23);
            this.AddCurrentSetting.TabIndex = 1;
            this.AddCurrentSetting.Text = "Add";
            this.AddCurrentSetting.UseVisualStyleBackColor = true;
            this.AddCurrentSetting.Click += new System.EventHandler(this.AddCurrentSetting_Click);
            // 
            // ClearSetting
            // 
            this.ClearSetting.Location = new System.Drawing.Point(7, 335);
            this.ClearSetting.Name = "ClearSetting";
            this.ClearSetting.Size = new System.Drawing.Size(101, 23);
            this.ClearSetting.TabIndex = 6;
            this.ClearSetting.Text = "Clear";
            this.ClearSetting.UseVisualStyleBackColor = true;
            this.ClearSetting.Click += new System.EventHandler(this.ClearSetting_Click);
            // 
            // LoadSelected
            // 
            this.LoadSelected.Location = new System.Drawing.Point(117, 280);
            this.LoadSelected.Name = "LoadSelected";
            this.LoadSelected.Size = new System.Drawing.Size(157, 23);
            this.LoadSelected.TabIndex = 12;
            this.LoadSelected.Text = "Selected row → FLIMage";
            this.LoadSelected.UseVisualStyleBackColor = true;
            this.LoadSelected.Click += new System.EventHandler(this.LoadSelected_Click);
            // 
            // RowUp
            // 
            this.RowUp.Location = new System.Drawing.Point(447, 287);
            this.RowUp.Name = "RowUp";
            this.RowUp.Size = new System.Drawing.Size(111, 23);
            this.RowUp.TabIndex = 2;
            this.RowUp.Text = "Up";
            this.RowUp.UseVisualStyleBackColor = true;
            this.RowUp.Click += new System.EventHandler(this.SettingID_UpDown);
            // 
            // ReplaceWithCurrent
            // 
            this.ReplaceWithCurrent.Location = new System.Drawing.Point(117, 307);
            this.ReplaceWithCurrent.Name = "ReplaceWithCurrent";
            this.ReplaceWithCurrent.Size = new System.Drawing.Size(157, 23);
            this.ReplaceWithCurrent.TabIndex = 11;
            this.ReplaceWithCurrent.Text = "FLIMage → selected row";
            this.ReplaceWithCurrent.UseVisualStyleBackColor = true;
            this.ReplaceWithCurrent.Click += new System.EventHandler(this.ReplaceWithCurrent_Click);
            // 
            // RowDown
            // 
            this.RowDown.Location = new System.Drawing.Point(447, 314);
            this.RowDown.Name = "RowDown";
            this.RowDown.Size = new System.Drawing.Size(111, 23);
            this.RowDown.TabIndex = 3;
            this.RowDown.Text = "Down";
            this.RowDown.UseVisualStyleBackColor = true;
            this.RowDown.Click += new System.EventHandler(this.SettingID_UpDown);
            // 
            // ImageSequenceGridView
            // 
            this.ImageSequenceGridView.AllowUserToAddRows = false;
            this.ImageSequenceGridView.AllowUserToDeleteRows = false;
            this.ImageSequenceGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.ImageSequenceGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ImageSequenceGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SettingID,
            this.SettingName1,
            this.Interval,
            this.Repetition,
            this.Exclusive});
            this.ImageSequenceGridView.Location = new System.Drawing.Point(0, 0);
            this.ImageSequenceGridView.Name = "ImageSequenceGridView";
            this.ImageSequenceGridView.Size = new System.Drawing.Size(595, 266);
            this.ImageSequenceGridView.TabIndex = 0;
            this.ImageSequenceGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.ImageSequenceGridView_CellEndEdit);
            this.ImageSequenceGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ImageSequenceGridView_CellMouseClick);
            this.ImageSequenceGridView.CurrentCellChanged += new System.EventHandler(this.ImageSequenceGridView_CurrentCellChanged);
            // 
            // SettingID
            // 
            this.SettingID.FillWeight = 47.68946F;
            this.SettingID.HeaderText = "Setting ID";
            this.SettingID.Name = "SettingID";
            // 
            // SettingName1
            // 
            this.SettingName1.FillWeight = 95.85716F;
            this.SettingName1.HeaderText = "Setting Name";
            this.SettingName1.Name = "SettingName1";
            // 
            // Interval
            // 
            this.Interval.FillWeight = 84.66787F;
            this.Interval.HeaderText = "Interval(s)";
            this.Interval.Name = "Interval";
            // 
            // Repetition
            // 
            this.Repetition.FillWeight = 90.39159F;
            this.Repetition.HeaderText = "Repetition";
            this.Repetition.Name = "Repetition";
            // 
            // Exclusive
            // 
            this.Exclusive.FillWeight = 111.3939F;
            this.Exclusive.HeaderText = "Exclusive";
            this.Exclusive.Name = "Exclusive";
            // 
            // DeleteRow
            // 
            this.DeleteRow.Location = new System.Drawing.Point(7, 308);
            this.DeleteRow.Name = "DeleteRow";
            this.DeleteRow.Size = new System.Drawing.Size(101, 23);
            this.DeleteRow.TabIndex = 4;
            this.DeleteRow.Text = "Delete";
            this.DeleteRow.UseVisualStyleBackColor = true;
            this.DeleteRow.Click += new System.EventHandler(this.DeleteRow_Click);
            // 
            // ForwardButton
            // 
            this.ForwardButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForwardButton.Location = new System.Drawing.Point(900, 421);
            this.ForwardButton.Name = "ForwardButton";
            this.ForwardButton.Size = new System.Drawing.Size(32, 33);
            this.ForwardButton.TabIndex = 26;
            this.ForwardButton.Text = ">";
            this.ForwardButton.UseVisualStyleBackColor = true;
            this.ForwardButton.Click += new System.EventHandler(this.ForwardButton_Click);
            // 
            // BackButton
            // 
            this.BackButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BackButton.Location = new System.Drawing.Point(862, 421);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(32, 33);
            this.BackButton.TabIndex = 25;
            this.BackButton.Text = "<";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // NPosition_Label
            // 
            this.NPosition_Label.AutoSize = true;
            this.NPosition_Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NPosition_Label.Location = new System.Drawing.Point(1091, 402);
            this.NPosition_Label.Name = "NPosition_Label";
            this.NPosition_Label.Size = new System.Drawing.Size(24, 13);
            this.NPosition_Label.TabIndex = 22;
            this.NPosition_Label.Text = "0/0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(1038, 402);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 13);
            this.label7.TabIndex = 23;
            this.label7.Text = "Position:";
            // 
            // StartFromCurrentCheck
            // 
            this.StartFromCurrentCheck.AutoSize = true;
            this.StartFromCurrentCheck.Location = new System.Drawing.Point(623, 421);
            this.StartFromCurrentCheck.Name = "StartFromCurrentCheck";
            this.StartFromCurrentCheck.Size = new System.Drawing.Size(146, 17);
            this.StartFromCurrentCheck.TabIndex = 20;
            this.StartFromCurrentCheck.Text = "Start from current position";
            this.StartFromCurrentCheck.UseVisualStyleBackColor = true;
            // 
            // PauseButton
            // 
            this.PauseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PauseButton.Location = new System.Drawing.Point(1075, 423);
            this.PauseButton.Name = "PauseButton";
            this.PauseButton.Size = new System.Drawing.Size(118, 33);
            this.PauseButton.TabIndex = 17;
            this.PauseButton.Text = "Pause";
            this.PauseButton.UseVisualStyleBackColor = true;
            this.PauseButton.Click += new System.EventHandler(this.PauseButton_Click);
            // 
            // totalTimeLabel
            // 
            this.totalTimeLabel.AutoSize = true;
            this.totalTimeLabel.Location = new System.Drawing.Point(901, 402);
            this.totalTimeLabel.Name = "totalTimeLabel";
            this.totalTimeLabel.Size = new System.Drawing.Size(22, 13);
            this.totalTimeLabel.TabIndex = 16;
            this.totalTimeLabel.Text = "0.0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(842, 402);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Total time:";
            // 
            // RunSeq
            // 
            this.RunSeq.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RunSeq.Location = new System.Drawing.Point(951, 423);
            this.RunSeq.Name = "RunSeq";
            this.RunSeq.Size = new System.Drawing.Size(118, 33);
            this.RunSeq.TabIndex = 5;
            this.RunSeq.Text = "Run sequence";
            this.RunSeq.UseVisualStyleBackColor = true;
            this.RunSeq.Click += new System.EventHandler(this.RunSeq_Click);
            // 
            // LoopCheck
            // 
            this.LoopCheck.AutoSize = true;
            this.LoopCheck.Location = new System.Drawing.Point(623, 398);
            this.LoopCheck.Name = "LoopCheck";
            this.LoopCheck.Size = new System.Drawing.Size(50, 17);
            this.LoopCheck.TabIndex = 14;
            this.LoopCheck.Text = "Loop";
            this.LoopCheck.UseVisualStyleBackColor = true;
            // 
            // RepNumberLabel
            // 
            this.RepNumberLabel.AutoSize = true;
            this.RepNumberLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RepNumberLabel.Location = new System.Drawing.Point(997, 402);
            this.RepNumberLabel.Name = "RepNumberLabel";
            this.RepNumberLabel.Size = new System.Drawing.Size(24, 13);
            this.RepNumberLabel.TabIndex = 7;
            this.RepNumberLabel.Text = "0/0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(937, 402);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Repetition:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(749, 402);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Time:";
            // 
            // RepTime
            // 
            this.RepTime.AutoSize = true;
            this.RepTime.Location = new System.Drawing.Point(793, 402);
            this.RepTime.Name = "RepTime";
            this.RepTime.Size = new System.Drawing.Size(22, 13);
            this.RepTime.TabIndex = 10;
            this.RepTime.Text = "0.0";
            // 
            // HomeTab
            // 
            this.HomeTab.Controls.Add(this.tabPage1);
            this.HomeTab.Controls.Add(this.tabPage2);
            this.HomeTab.Location = new System.Drawing.Point(1, 34);
            this.HomeTab.Name = "HomeTab";
            this.HomeTab.SelectedIndex = 0;
            this.HomeTab.Size = new System.Drawing.Size(603, 457);
            this.HomeTab.TabIndex = 21;
            // 
            // SortFileNames_checkBox
            // 
            this.SortFileNames_checkBox.AutoSize = true;
            this.SortFileNames_checkBox.Checked = true;
            this.SortFileNames_checkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SortFileNames_checkBox.Location = new System.Drawing.Point(623, 463);
            this.SortFileNames_checkBox.Name = "SortFileNames_checkBox";
            this.SortFileNames_checkBox.Size = new System.Drawing.Size(154, 17);
            this.SortFileNames_checkBox.TabIndex = 296;
            this.SortFileNames_checkBox.Text = "Sort File Names by Position";
            this.SortFileNames_checkBox.UseVisualStyleBackColor = true;
            // 
            // NewFileName
            // 
            this.NewFileName.AutoSize = true;
            this.NewFileName.Location = new System.Drawing.Point(923, 465);
            this.NewFileName.Name = "NewFileName";
            this.NewFileName.Size = new System.Drawing.Size(55, 13);
            this.NewFileName.TabIndex = 297;
            this.NewFileName.Text = "Filename: ";
            // 
            // Analyze_checkBox
            // 
            this.Analyze_checkBox.AutoSize = true;
            this.Analyze_checkBox.Checked = true;
            this.Analyze_checkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Analyze_checkBox.Location = new System.Drawing.Point(780, 463);
            this.Analyze_checkBox.Name = "Analyze_checkBox";
            this.Analyze_checkBox.Size = new System.Drawing.Size(140, 17);
            this.Analyze_checkBox.TabIndex = 298;
            this.Analyze_checkBox.Text = "Analyze after acquisition";
            this.Analyze_checkBox.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1205, 24);
            this.menuStrip1.TabIndex = 299;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // Image_sequence2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1205, 490);
            this.Controls.Add(this.Analyze_checkBox);
            this.Controls.Add(this.NewFileName);
            this.Controls.Add(this.SortFileNames_checkBox);
            this.Controls.Add(this.NPosition_Label);
            this.Controls.Add(this.ForwardButton);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.PlotBox);
            this.Controls.Add(this.HomeTab);
            this.Controls.Add(this.totalTimeLabel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.LoopCheck);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TimeShift_textBox);
            this.Controls.Add(this.RepNumberLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RunSeq);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.RepTime);
            this.Controls.Add(this.StartFromCurrentCheck);
            this.Controls.Add(this.PauseButton);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Image_sequence2";
            this.Text = "Image_sequence";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Image_seqeunce_FormClosing);
            this.Load += new System.EventHandler(this.Image_seqeunce_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PlotBox)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PositionDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TemplateImage_PB)).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.setting_groupBox.ResumeLayout(false);
            this.setting_groupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageSequenceGridView)).EndInit();
            this.HomeTab.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox PlotBox;
        private System.Windows.Forms.TabPage tabPage2;
        public System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox TimeShift_textBox;
        public System.Windows.Forms.Button Pos_Delete;
        public System.Windows.Forms.Button Pos_ClearAll;
        public System.Windows.Forms.Button Pos_Goto;
        public System.Windows.Forms.Button RecalculateDrift;
        public System.Windows.Forms.Label Status_Motor;
        public System.Windows.Forms.Label Status_V;
        public System.Windows.Forms.Label Status_XYZ_um;
        public System.Windows.Forms.Label Status_XY;
        public System.Windows.Forms.PictureBox TemplateImage_PB;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.ComboBox Channel_Pulldown;
        public System.Windows.Forms.Label TemplateFileName;
        public System.Windows.Forms.Button SelectCurrentImage;
        public System.Windows.Forms.CheckBox UseMirror_CB;
        public System.Windows.Forms.CheckBox ZCorrect_CB;
        public System.Windows.Forms.CheckBox XYCorrect_CB;
        public System.Windows.Forms.CheckBox DriftCorrection_CB;
        private System.Windows.Forms.TabPage tabPage1;
        public System.Windows.Forms.Button ForwardButton;
        public System.Windows.Forms.Button BackButton;
        public System.Windows.Forms.Label NPosition_Label;
        public System.Windows.Forms.Label label7;
        public System.Windows.Forms.Button AddCurrentSetting;
        public System.Windows.Forms.Button RowUp;
        public System.Windows.Forms.Button RowDown;
        public System.Windows.Forms.Button DeleteRow;
        public System.Windows.Forms.Button ClearSetting;
        public System.Windows.Forms.Button LoadSelected;
        public System.Windows.Forms.Button ReplaceWithCurrent;
        public System.Windows.Forms.DataGridView ImageSequenceGridView;
        public System.Windows.Forms.CheckBox StartFromCurrentCheck;
        public System.Windows.Forms.Button PauseButton;
        public System.Windows.Forms.Label totalTimeLabel;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Button RunSeq;
        public System.Windows.Forms.CheckBox LoopCheck;
        public System.Windows.Forms.Label RepNumberLabel;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Label RepTime;
        private System.Windows.Forms.TabControl HomeTab;
        private System.Windows.Forms.DataGridView PositionDataGridView;
        public System.Windows.Forms.Button AcquireTemplateImageButton;
        public System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox ZoomTextBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn SettingID;
        private System.Windows.Forms.DataGridViewTextBoxColumn SettingName1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Interval;
        private System.Windows.Forms.DataGridViewTextBoxColumn Repetition;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Exclusive;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn X;
        private System.Windows.Forms.DataGridViewTextBoxColumn PositionY;
        private System.Windows.Forms.DataGridViewTextBoxColumn Z;
        private System.Windows.Forms.CheckBox aveFrame_checkBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox nFrames_textBox;
        private System.Windows.Forms.CheckBox ZStack_checkBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox sliceStep_textBox;
        private System.Windows.Forms.TextBox nSlices_textBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox nAveFrame_textBox;
        private System.Windows.Forms.GroupBox setting_groupBox;
        public System.Windows.Forms.Button ReloadTemplate;
        private System.Windows.Forms.CheckBox SortFileNames_checkBox;
        private System.Windows.Forms.Label NewFileName;
        private System.Windows.Forms.CheckBox Analyze_checkBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
    }
}