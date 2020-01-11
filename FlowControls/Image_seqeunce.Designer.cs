namespace FLIMage.FlowControls
{
    partial class Image_seqeunce
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Image_seqeunce));
            this.ImageSequenceGridView = new System.Windows.Forms.DataGridView();
            this.SettingID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SettingName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Interval = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Repetition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Zoom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AddCurrentSetting = new System.Windows.Forms.Button();
            this.RowUp = new System.Windows.Forms.Button();
            this.RowDown = new System.Windows.Forms.Button();
            this.DeleteRow = new System.Windows.Forms.Button();
            this.RunSeq = new System.Windows.Forms.Button();
            this.ClearSetting = new System.Windows.Forms.Button();
            this.RepNumber = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.RepTime = new System.Windows.Forms.Label();
            this.ReplaceWithCurrent = new System.Windows.Forms.Button();
            this.LoadSelected = new System.Windows.Forms.Button();
            this.AutoDriftCorrection = new System.Windows.Forms.CheckBox();
            this.LoopCheck = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.totalTimeLabel = new System.Windows.Forms.Label();
            this.PauseButton = new System.Windows.Forms.Button();
            this.ProgressEdit = new System.Windows.Forms.TextBox();
            this.StartFromCurrentCheck = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.ImageSequenceGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // ImageSequenceGridView
            // 
            this.ImageSequenceGridView.AllowUserToAddRows = false;
            this.ImageSequenceGridView.AllowUserToDeleteRows = false;
            this.ImageSequenceGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ImageSequenceGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SettingID,
            this.SettingName,
            this.Interval,
            this.Repetition,
            this.Zoom});
            this.ImageSequenceGridView.Location = new System.Drawing.Point(0, 2);
            this.ImageSequenceGridView.Name = "ImageSequenceGridView";
            this.ImageSequenceGridView.Size = new System.Drawing.Size(578, 266);
            this.ImageSequenceGridView.TabIndex = 0;
            this.ImageSequenceGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.ImageSequenceGridView_CellEndEdit);
            this.ImageSequenceGridView.CurrentCellChanged += new System.EventHandler(this.ImageSequenceGridView_CurrentCellChanged);
            // 
            // SettingID
            // 
            this.SettingID.FillWeight = 60F;
            this.SettingID.HeaderText = "Setting ID";
            this.SettingID.Name = "SettingID";
            this.SettingID.Width = 60;
            // 
            // SettingName
            // 
            this.SettingName.FillWeight = 150F;
            this.SettingName.HeaderText = "Setting Name";
            this.SettingName.Name = "SettingName";
            this.SettingName.Width = 150;
            // 
            // Interval
            // 
            this.Interval.FillWeight = 60F;
            this.Interval.HeaderText = "Interval(s)";
            this.Interval.Name = "Interval";
            this.Interval.Width = 60;
            // 
            // Repetition
            // 
            this.Repetition.FillWeight = 60F;
            this.Repetition.HeaderText = "Repetition";
            this.Repetition.Name = "Repetition";
            this.Repetition.Width = 60;
            // 
            // Zoom
            // 
            this.Zoom.FillWeight = 60F;
            this.Zoom.HeaderText = "Zoom";
            this.Zoom.Name = "Zoom";
            this.Zoom.Width = 60;
            // 
            // AddCurrentSetting
            // 
            this.AddCurrentSetting.Location = new System.Drawing.Point(592, 156);
            this.AddCurrentSetting.Name = "AddCurrentSetting";
            this.AddCurrentSetting.Size = new System.Drawing.Size(60, 23);
            this.AddCurrentSetting.TabIndex = 1;
            this.AddCurrentSetting.Text = "Add";
            this.AddCurrentSetting.UseVisualStyleBackColor = true;
            this.AddCurrentSetting.Click += new System.EventHandler(this.AddCurrentSetting_Click);
            // 
            // RowUp
            // 
            this.RowUp.Location = new System.Drawing.Point(592, 42);
            this.RowUp.Name = "RowUp";
            this.RowUp.Size = new System.Drawing.Size(60, 23);
            this.RowUp.TabIndex = 2;
            this.RowUp.Text = "Up";
            this.RowUp.UseVisualStyleBackColor = true;
            this.RowUp.Click += new System.EventHandler(this.SettingID_UpDown);
            // 
            // RowDown
            // 
            this.RowDown.Location = new System.Drawing.Point(592, 71);
            this.RowDown.Name = "RowDown";
            this.RowDown.Size = new System.Drawing.Size(60, 23);
            this.RowDown.TabIndex = 3;
            this.RowDown.Text = "Down";
            this.RowDown.UseVisualStyleBackColor = true;
            this.RowDown.Click += new System.EventHandler(this.SettingID_UpDown);
            // 
            // DeleteRow
            // 
            this.DeleteRow.Location = new System.Drawing.Point(592, 185);
            this.DeleteRow.Name = "DeleteRow";
            this.DeleteRow.Size = new System.Drawing.Size(60, 23);
            this.DeleteRow.TabIndex = 4;
            this.DeleteRow.Text = "Remove";
            this.DeleteRow.UseVisualStyleBackColor = true;
            this.DeleteRow.Click += new System.EventHandler(this.DeleteRow_Click);
            // 
            // RunSeq
            // 
            this.RunSeq.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RunSeq.Location = new System.Drawing.Point(408, 324);
            this.RunSeq.Name = "RunSeq";
            this.RunSeq.Size = new System.Drawing.Size(118, 33);
            this.RunSeq.TabIndex = 5;
            this.RunSeq.Text = "Run sequence";
            this.RunSeq.UseVisualStyleBackColor = true;
            this.RunSeq.Click += new System.EventHandler(this.RunSeq_Click);
            // 
            // ClearSetting
            // 
            this.ClearSetting.Location = new System.Drawing.Point(12, 290);
            this.ClearSetting.Name = "ClearSetting";
            this.ClearSetting.Size = new System.Drawing.Size(118, 23);
            this.ClearSetting.TabIndex = 6;
            this.ClearSetting.Text = "Clear All";
            this.ClearSetting.UseVisualStyleBackColor = true;
            this.ClearSetting.Click += new System.EventHandler(this.ClearSetting_Click);
            // 
            // RepNumber
            // 
            this.RepNumber.AutoSize = true;
            this.RepNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RepNumber.Location = new System.Drawing.Point(502, 276);
            this.RepNumber.Name = "RepNumber";
            this.RepNumber.Size = new System.Drawing.Size(18, 13);
            this.RepNumber.TabIndex = 7;
            this.RepNumber.Text = "/0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(405, 276);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Progress:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(405, 291);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Time:";
            // 
            // RepTime
            // 
            this.RepTime.AutoSize = true;
            this.RepTime.Location = new System.Drawing.Point(464, 291);
            this.RepTime.Name = "RepTime";
            this.RepTime.Size = new System.Drawing.Size(22, 13);
            this.RepTime.TabIndex = 10;
            this.RepTime.Text = "0.0";
            // 
            // ReplaceWithCurrent
            // 
            this.ReplaceWithCurrent.Location = new System.Drawing.Point(205, 322);
            this.ReplaceWithCurrent.Name = "ReplaceWithCurrent";
            this.ReplaceWithCurrent.Size = new System.Drawing.Size(139, 23);
            this.ReplaceWithCurrent.TabIndex = 11;
            this.ReplaceWithCurrent.Text = "Save selected setting";
            this.ReplaceWithCurrent.UseVisualStyleBackColor = true;
            this.ReplaceWithCurrent.Click += new System.EventHandler(this.ReplaceWithCurrent_Click);
            // 
            // LoadSelected
            // 
            this.LoadSelected.Location = new System.Drawing.Point(205, 289);
            this.LoadSelected.Name = "LoadSelected";
            this.LoadSelected.Size = new System.Drawing.Size(139, 23);
            this.LoadSelected.TabIndex = 12;
            this.LoadSelected.Text = "Load selected setting";
            this.LoadSelected.UseVisualStyleBackColor = true;
            this.LoadSelected.Click += new System.EventHandler(this.LoadSelected_Click);
            // 
            // AutoDriftCorrection
            // 
            this.AutoDriftCorrection.AutoSize = true;
            this.AutoDriftCorrection.Location = new System.Drawing.Point(35, 328);
            this.AutoDriftCorrection.Name = "AutoDriftCorrection";
            this.AutoDriftCorrection.Size = new System.Drawing.Size(95, 17);
            this.AutoDriftCorrection.TabIndex = 13;
            this.AutoDriftCorrection.Text = "Drift correction";
            this.AutoDriftCorrection.UseVisualStyleBackColor = true;
            this.AutoDriftCorrection.Click += new System.EventHandler(this.AutoDriftCorrection_Click);
            // 
            // LoopCheck
            // 
            this.LoopCheck.AutoSize = true;
            this.LoopCheck.Location = new System.Drawing.Point(411, 362);
            this.LoopCheck.Name = "LoopCheck";
            this.LoopCheck.Size = new System.Drawing.Size(50, 17);
            this.LoopCheck.TabIndex = 14;
            this.LoopCheck.Text = "Loop";
            this.LoopCheck.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(405, 308);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Total time:";
            // 
            // totalTimeLabel
            // 
            this.totalTimeLabel.AutoSize = true;
            this.totalTimeLabel.Location = new System.Drawing.Point(464, 308);
            this.totalTimeLabel.Name = "totalTimeLabel";
            this.totalTimeLabel.Size = new System.Drawing.Size(22, 13);
            this.totalTimeLabel.TabIndex = 16;
            this.totalTimeLabel.Text = "0.0";
            // 
            // PauseButton
            // 
            this.PauseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PauseButton.Location = new System.Drawing.Point(532, 324);
            this.PauseButton.Name = "PauseButton";
            this.PauseButton.Size = new System.Drawing.Size(118, 33);
            this.PauseButton.TabIndex = 17;
            this.PauseButton.Text = "Pause";
            this.PauseButton.UseVisualStyleBackColor = true;
            this.PauseButton.Click += new System.EventHandler(this.PauseButton_Click);
            // 
            // ProgressEdit
            // 
            this.ProgressEdit.Location = new System.Drawing.Point(461, 271);
            this.ProgressEdit.Name = "ProgressEdit";
            this.ProgressEdit.Size = new System.Drawing.Size(35, 20);
            this.ProgressEdit.TabIndex = 19;
            this.ProgressEdit.Text = "0";
            // 
            // StartFromCurrentCheck
            // 
            this.StartFromCurrentCheck.AutoSize = true;
            this.StartFromCurrentCheck.Location = new System.Drawing.Point(411, 382);
            this.StartFromCurrentCheck.Name = "StartFromCurrentCheck";
            this.StartFromCurrentCheck.Size = new System.Drawing.Size(146, 17);
            this.StartFromCurrentCheck.TabIndex = 20;
            this.StartFromCurrentCheck.Text = "Start from current position";
            this.StartFromCurrentCheck.UseVisualStyleBackColor = true;
            // 
            // Image_seqeunce
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(677, 411);
            this.Controls.Add(this.StartFromCurrentCheck);
            this.Controls.Add(this.ProgressEdit);
            this.Controls.Add(this.PauseButton);
            this.Controls.Add(this.totalTimeLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.LoopCheck);
            this.Controls.Add(this.AutoDriftCorrection);
            this.Controls.Add(this.LoadSelected);
            this.Controls.Add(this.ReplaceWithCurrent);
            this.Controls.Add(this.RepTime);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RepNumber);
            this.Controls.Add(this.ClearSetting);
            this.Controls.Add(this.RunSeq);
            this.Controls.Add(this.DeleteRow);
            this.Controls.Add(this.RowDown);
            this.Controls.Add(this.RowUp);
            this.Controls.Add(this.AddCurrentSetting);
            this.Controls.Add(this.ImageSequenceGridView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Image_seqeunce";
            this.Text = "Image_seqeunce";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Image_seqeunce_FormClosing);
            this.Load += new System.EventHandler(this.Image_seqeunce_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ImageSequenceGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.DataGridView ImageSequenceGridView;
        public System.Windows.Forms.Button AddCurrentSetting;
        public System.Windows.Forms.Button RowUp;
        public System.Windows.Forms.Button RowDown;
        public System.Windows.Forms.Button DeleteRow;
        public System.Windows.Forms.Button RunSeq;
        public System.Windows.Forms.Button ClearSetting;
        public System.Windows.Forms.Label RepNumber;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.DataGridViewTextBoxColumn SettingID;
        public System.Windows.Forms.DataGridViewTextBoxColumn SettingName;
        public System.Windows.Forms.DataGridViewTextBoxColumn Interval;
        public System.Windows.Forms.DataGridViewTextBoxColumn Repetition;
        public System.Windows.Forms.DataGridViewTextBoxColumn Zoom;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Label RepTime;
        public System.Windows.Forms.Button ReplaceWithCurrent;
        public System.Windows.Forms.Button LoadSelected;
        public System.Windows.Forms.CheckBox AutoDriftCorrection;
        public System.Windows.Forms.CheckBox LoopCheck;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label totalTimeLabel;
        public System.Windows.Forms.Button PauseButton;
        private System.Windows.Forms.TextBox ProgressEdit;
        public System.Windows.Forms.CheckBox StartFromCurrentCheck;
    }
}