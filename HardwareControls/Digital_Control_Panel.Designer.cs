namespace FLIMage.HardwareControls
{
    partial class Digital_Trigger_Panel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Digital_Trigger_Panel));
            this.Uncage_Save = new System.Windows.Forms.Button();
            this.PulseName = new System.Windows.Forms.TextBox();
            this.PulseNumber = new System.Windows.Forms.NumericUpDown();
            this.label97 = new System.Windows.Forms.Label();
            this.OutputRate = new System.Windows.Forms.TextBox();
            this.label92 = new System.Windows.Forms.Label();
            this.Length = new System.Windows.Forms.TextBox();
            this.label90 = new System.Windows.Forms.Label();
            this.Delay = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.ISI = new System.Windows.Forms.TextBox();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.FrameNote = new System.Windows.Forms.Label();
            this.SliceNote = new System.Windows.Forms.Label();
            this.BaseLine_Slice_s = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.SliceInterval_s = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.RepeatFrame = new System.Windows.Forms.Label();
            this.FrameBeforePulse_ms = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.FrameInterval_ms = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.FramesBeforePulse = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.U_counter = new System.Windows.Forms.Label();
            this.SyncWithSlice_Check = new System.Windows.Forms.RadioButton();
            this.SyncWithFrame_Check = new System.Windows.Forms.RadioButton();
            this.SliceInterval = new System.Windows.Forms.TextBox();
            this.SlicesBeforePulse = new System.Windows.Forms.TextBox();
            this.label73 = new System.Windows.Forms.Label();
            this.label71 = new System.Windows.Forms.Label();
            this.FrameInterval = new System.Windows.Forms.TextBox();
            this.label63 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.DO_Repeat = new System.Windows.Forms.TextBox();
            this.StartDO_button = new System.Windows.Forms.Button();
            this.label75 = new System.Windows.Forms.Label();
            this.DO_interval = new System.Windows.Forms.TextBox();
            this.label62 = new System.Windows.Forms.Label();
            this.dwell = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.pulseN = new System.Windows.Forms.TextBox();
            this.label93 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.PictureBox();
            this.UncageOnlyPanel = new System.Windows.Forms.GroupBox();
            this.label15 = new System.Windows.Forms.Label();
            this.U_counter2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ChannelComboBox = new System.Windows.Forms.ComboBox();
            this.ChannelLabel = new System.Windows.Forms.Label();
            this.ShowRepeat = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ActiveHighCheck = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.PulseNumber)).BeginInit();
            this.groupBox10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panel1)).BeginInit();
            this.UncageOnlyPanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Uncage_Save
            // 
            this.Uncage_Save.Location = new System.Drawing.Point(268, 14);
            this.Uncage_Save.Name = "Uncage_Save";
            this.Uncage_Save.Size = new System.Drawing.Size(45, 21);
            this.Uncage_Save.TabIndex = 391;
            this.Uncage_Save.Text = "Save";
            this.Uncage_Save.UseVisualStyleBackColor = true;
            this.Uncage_Save.Click += new System.EventHandler(this.Uncage_Save_Click);
            // 
            // PulseName
            // 
            this.PulseName.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PulseName.Location = new System.Drawing.Point(167, 38);
            this.PulseName.Margin = new System.Windows.Forms.Padding(1);
            this.PulseName.Name = "PulseName";
            this.PulseName.Size = new System.Drawing.Size(92, 20);
            this.PulseName.TabIndex = 388;
            this.PulseName.Text = "Pulse set";
            this.PulseName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // PulseNumber
            // 
            this.PulseNumber.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PulseNumber.Location = new System.Drawing.Point(261, 38);
            this.PulseNumber.Margin = new System.Windows.Forms.Padding(1);
            this.PulseNumber.Name = "PulseNumber";
            this.PulseNumber.Size = new System.Drawing.Size(52, 20);
            this.PulseNumber.TabIndex = 390;
            this.PulseNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PulseNumber.ValueChanged += new System.EventHandler(this.PulseNumber_ValueChanged);
            // 
            // label97
            // 
            this.label97.AutoSize = true;
            this.label97.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label97.Location = new System.Drawing.Point(190, 18);
            this.label97.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label97.Name = "label97";
            this.label97.Size = new System.Drawing.Size(34, 14);
            this.label97.TabIndex = 389;
            this.label97.Text = "Name";
            // 
            // OutputRate
            // 
            this.OutputRate.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OutputRate.Location = new System.Drawing.Point(251, 113);
            this.OutputRate.Margin = new System.Windows.Forms.Padding(1);
            this.OutputRate.Name = "OutputRate";
            this.OutputRate.Size = new System.Drawing.Size(50, 20);
            this.OutputRate.TabIndex = 387;
            this.OutputRate.Text = "0";
            this.OutputRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.OutputRate.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // label92
            // 
            this.label92.AutoSize = true;
            this.label92.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label92.Location = new System.Drawing.Point(165, 140);
            this.label92.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(86, 14);
            this.label92.TabIndex = 379;
            this.label92.Text = "Total length (ms)";
            // 
            // Length
            // 
            this.Length.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Length.Location = new System.Drawing.Point(251, 137);
            this.Length.Margin = new System.Windows.Forms.Padding(1);
            this.Length.Name = "Length";
            this.Length.Size = new System.Drawing.Size(50, 20);
            this.Length.TabIndex = 378;
            this.Length.Text = "0";
            this.Length.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.Length.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // label90
            // 
            this.label90.AutoSize = true;
            this.label90.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label90.Location = new System.Drawing.Point(67, 128);
            this.label90.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label90.Name = "label90";
            this.label90.Size = new System.Drawing.Size(59, 14);
            this.label90.TabIndex = 377;
            this.label90.Text = "Delay (ms)";
            // 
            // Delay
            // 
            this.Delay.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Delay.Location = new System.Drawing.Point(66, 144);
            this.Delay.Margin = new System.Windows.Forms.Padding(1);
            this.Delay.Name = "Delay";
            this.Delay.Size = new System.Drawing.Size(50, 20);
            this.Delay.TabIndex = 376;
            this.Delay.Text = "0";
            this.Delay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.Delay.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(1, 128);
            this.label10.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(67, 14);
            this.label10.TabIndex = 375;
            this.label10.Text = "Interval (ms)";
            // 
            // ISI
            // 
            this.ISI.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ISI.Location = new System.Drawing.Point(8, 144);
            this.ISI.Margin = new System.Windows.Forms.Padding(1);
            this.ISI.Name = "ISI";
            this.ISI.Size = new System.Drawing.Size(50, 20);
            this.ISI.TabIndex = 374;
            this.ISI.Text = "0";
            this.ISI.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ISI.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.FrameNote);
            this.groupBox10.Controls.Add(this.SliceNote);
            this.groupBox10.Controls.Add(this.BaseLine_Slice_s);
            this.groupBox10.Controls.Add(this.label17);
            this.groupBox10.Controls.Add(this.label19);
            this.groupBox10.Controls.Add(this.label18);
            this.groupBox10.Controls.Add(this.SliceInterval_s);
            this.groupBox10.Controls.Add(this.label14);
            this.groupBox10.Controls.Add(this.RepeatFrame);
            this.groupBox10.Controls.Add(this.FrameBeforePulse_ms);
            this.groupBox10.Controls.Add(this.label11);
            this.groupBox10.Controls.Add(this.label9);
            this.groupBox10.Controls.Add(this.FrameInterval_ms);
            this.groupBox10.Controls.Add(this.label3);
            this.groupBox10.Controls.Add(this.label8);
            this.groupBox10.Controls.Add(this.FramesBeforePulse);
            this.groupBox10.Controls.Add(this.label7);
            this.groupBox10.Controls.Add(this.U_counter);
            this.groupBox10.Controls.Add(this.SyncWithSlice_Check);
            this.groupBox10.Controls.Add(this.SyncWithFrame_Check);
            this.groupBox10.Controls.Add(this.SliceInterval);
            this.groupBox10.Controls.Add(this.SlicesBeforePulse);
            this.groupBox10.Controls.Add(this.label73);
            this.groupBox10.Controls.Add(this.label71);
            this.groupBox10.Controls.Add(this.FrameInterval);
            this.groupBox10.Controls.Add(this.label63);
            this.groupBox10.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox10.Location = new System.Drawing.Point(333, 96);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(303, 192);
            this.groupBox10.TabIndex = 370;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "DO trigger during imaging";
            // 
            // FrameNote
            // 
            this.FrameNote.AutoSize = true;
            this.FrameNote.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FrameNote.Location = new System.Drawing.Point(7, 149);
            this.FrameNote.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.FrameNote.Name = "FrameNote";
            this.FrameNote.Size = new System.Drawing.Size(119, 14);
            this.FrameNote.TabIndex = 429;
            this.FrameNote.Text = "(Frame interval = 0.0 s)";
            // 
            // SliceNote
            // 
            this.SliceNote.AutoSize = true;
            this.SliceNote.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SliceNote.Location = new System.Drawing.Point(149, 149);
            this.SliceNote.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.SliceNote.Name = "SliceNote";
            this.SliceNote.Size = new System.Drawing.Size(112, 14);
            this.SliceNote.TabIndex = 428;
            this.SliceNote.Text = "(Slice interval = 0.0 s)";
            // 
            // BaseLine_Slice_s
            // 
            this.BaseLine_Slice_s.AutoSize = true;
            this.BaseLine_Slice_s.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BaseLine_Slice_s.Location = new System.Drawing.Point(200, 74);
            this.BaseLine_Slice_s.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.BaseLine_Slice_s.Name = "BaseLine_Slice_s";
            this.BaseLine_Slice_s.Size = new System.Drawing.Size(31, 14);
            this.BaseLine_Slice_s.TabIndex = 427;
            this.BaseLine_Slice_s.Text = "0.0 s";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(226, 57);
            this.label17.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(36, 14);
            this.label17.TabIndex = 425;
            this.label17.Text = "Slices";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(149, 86);
            this.label19.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(90, 14);
            this.label19.TabIndex = 424;
            this.label19.Text = "Pulse-set interval";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(149, 35);
            this.label18.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(122, 14);
            this.label18.TabIndex = 423;
            this.label18.Text = "Baseline before uncage";
            // 
            // SliceInterval_s
            // 
            this.SliceInterval_s.AutoSize = true;
            this.SliceInterval_s.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SliceInterval_s.Location = new System.Drawing.Point(198, 126);
            this.SliceInterval_s.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.SliceInterval_s.Name = "SliceInterval_s";
            this.SliceInterval_s.Size = new System.Drawing.Size(31, 14);
            this.SliceInterval_s.TabIndex = 422;
            this.SliceInterval_s.Text = "0.0 s";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(19, 167);
            this.label14.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(44, 14);
            this.label14.TabIndex = 420;
            this.label14.Text = "Repeat:";
            // 
            // RepeatFrame
            // 
            this.RepeatFrame.AutoSize = true;
            this.RepeatFrame.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RepeatFrame.Location = new System.Drawing.Point(65, 167);
            this.RepeatFrame.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.RepeatFrame.Name = "RepeatFrame";
            this.RepeatFrame.Size = new System.Drawing.Size(28, 14);
            this.RepeatFrame.TabIndex = 419;
            this.RepeatFrame.Text = "0 / 0";
            // 
            // FrameBeforePulse_ms
            // 
            this.FrameBeforePulse_ms.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FrameBeforePulse_ms.Location = new System.Drawing.Point(40, 70);
            this.FrameBeforePulse_ms.Margin = new System.Windows.Forms.Padding(1);
            this.FrameBeforePulse_ms.Name = "FrameBeforePulse_ms";
            this.FrameBeforePulse_ms.Size = new System.Drawing.Size(50, 20);
            this.FrameBeforePulse_ms.TabIndex = 417;
            this.FrameBeforePulse_ms.Text = "0";
            this.FrameBeforePulse_ms.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.FrameBeforePulse_ms.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(92, 73);
            this.label11.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(21, 14);
            this.label11.TabIndex = 418;
            this.label11.Text = "ms";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(92, 52);
            this.label9.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(43, 14);
            this.label9.TabIndex = 416;
            this.label9.Text = "Frames";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FrameInterval_ms
            // 
            this.FrameInterval_ms.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FrameInterval_ms.Location = new System.Drawing.Point(40, 128);
            this.FrameInterval_ms.Margin = new System.Windows.Forms.Padding(1);
            this.FrameInterval_ms.Name = "FrameInterval_ms";
            this.FrameInterval_ms.Size = new System.Drawing.Size(50, 20);
            this.FrameInterval_ms.TabIndex = 413;
            this.FrameInterval_ms.Text = "0";
            this.FrameInterval_ms.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.FrameInterval_ms.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(92, 131);
            this.label3.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(21, 14);
            this.label3.TabIndex = 415;
            this.label3.Text = "ms";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(92, 112);
            this.label8.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(43, 14);
            this.label8.TabIndex = 407;
            this.label8.Text = "Frames";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FramesBeforePulse
            // 
            this.FramesBeforePulse.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FramesBeforePulse.Location = new System.Drawing.Point(40, 49);
            this.FramesBeforePulse.Margin = new System.Windows.Forms.Padding(1);
            this.FramesBeforePulse.Name = "FramesBeforePulse";
            this.FramesBeforePulse.Size = new System.Drawing.Size(50, 20);
            this.FramesBeforePulse.TabIndex = 404;
            this.FramesBeforePulse.Text = "0";
            this.FramesBeforePulse.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.FramesBeforePulse.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(165, 167);
            this.label7.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 14);
            this.label7.TabIndex = 402;
            this.label7.Text = "Repeat:";
            // 
            // U_counter
            // 
            this.U_counter.AutoSize = true;
            this.U_counter.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.U_counter.Location = new System.Drawing.Point(210, 167);
            this.U_counter.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.U_counter.Name = "U_counter";
            this.U_counter.Size = new System.Drawing.Size(28, 14);
            this.U_counter.TabIndex = 323;
            this.U_counter.Text = "0 / 0";
            // 
            // SyncWithSlice_Check
            // 
            this.SyncWithSlice_Check.AutoSize = true;
            this.SyncWithSlice_Check.Checked = true;
            this.SyncWithSlice_Check.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SyncWithSlice_Check.Location = new System.Drawing.Point(149, 16);
            this.SyncWithSlice_Check.Name = "SyncWithSlice_Check";
            this.SyncWithSlice_Check.Size = new System.Drawing.Size(129, 18);
            this.SyncWithSlice_Check.TabIndex = 316;
            this.SyncWithSlice_Check.TabStop = true;
            this.SyncWithSlice_Check.Text = "Insert between slices";
            this.SyncWithSlice_Check.UseVisualStyleBackColor = true;
            this.SyncWithSlice_Check.Click += new System.EventHandler(this.Generic_RadioButton);
            // 
            // SyncWithFrame_Check
            // 
            this.SyncWithFrame_Check.AutoSize = true;
            this.SyncWithFrame_Check.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SyncWithFrame_Check.Location = new System.Drawing.Point(10, 16);
            this.SyncWithFrame_Check.Name = "SyncWithFrame_Check";
            this.SyncWithFrame_Check.Size = new System.Drawing.Size(113, 18);
            this.SyncWithFrame_Check.TabIndex = 315;
            this.SyncWithFrame_Check.Text = "Sync with Frames";
            this.SyncWithFrame_Check.UseVisualStyleBackColor = true;
            this.SyncWithFrame_Check.Click += new System.EventHandler(this.Generic_RadioButton);
            // 
            // SliceInterval
            // 
            this.SliceInterval.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SliceInterval.Location = new System.Drawing.Point(172, 103);
            this.SliceInterval.Margin = new System.Windows.Forms.Padding(1);
            this.SliceInterval.Name = "SliceInterval";
            this.SliceInterval.Size = new System.Drawing.Size(50, 20);
            this.SliceInterval.TabIndex = 312;
            this.SliceInterval.Text = "0";
            this.SliceInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.SliceInterval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // SlicesBeforePulse
            // 
            this.SlicesBeforePulse.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SlicesBeforePulse.Location = new System.Drawing.Point(172, 52);
            this.SlicesBeforePulse.Margin = new System.Windows.Forms.Padding(1);
            this.SlicesBeforePulse.Name = "SlicesBeforePulse";
            this.SlicesBeforePulse.Size = new System.Drawing.Size(50, 20);
            this.SlicesBeforePulse.TabIndex = 310;
            this.SlicesBeforePulse.Text = "0";
            this.SlicesBeforePulse.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.SlicesBeforePulse.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // label73
            // 
            this.label73.AutoSize = true;
            this.label73.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label73.Location = new System.Drawing.Point(7, 35);
            this.label73.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label73.Name = "label73";
            this.label73.Size = new System.Drawing.Size(122, 14);
            this.label73.TabIndex = 309;
            this.label73.Text = "Baseline before uncage";
            // 
            // label71
            // 
            this.label71.AutoSize = true;
            this.label71.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label71.Location = new System.Drawing.Point(224, 106);
            this.label71.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label71.Name = "label71";
            this.label71.Size = new System.Drawing.Size(36, 14);
            this.label71.TabIndex = 313;
            this.label71.Text = "Slices";
            // 
            // FrameInterval
            // 
            this.FrameInterval.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FrameInterval.Location = new System.Drawing.Point(40, 106);
            this.FrameInterval.Margin = new System.Windows.Forms.Padding(1);
            this.FrameInterval.Name = "FrameInterval";
            this.FrameInterval.Size = new System.Drawing.Size(50, 20);
            this.FrameInterval.TabIndex = 301;
            this.FrameInterval.Text = "0";
            this.FrameInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.FrameInterval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // label63
            // 
            this.label63.AutoSize = true;
            this.label63.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label63.Location = new System.Drawing.Point(7, 91);
            this.label63.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label63.Name = "label63";
            this.label63.Size = new System.Drawing.Size(90, 14);
            this.label63.TabIndex = 302;
            this.label63.Text = "Pulse-set interval";
            this.label63.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(220, 233);
            this.label5.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 14);
            this.label5.TabIndex = 325;
            this.label5.Text = "Repeat pulse-set";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DO_Repeat
            // 
            this.DO_Repeat.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DO_Repeat.Location = new System.Drawing.Point(233, 251);
            this.DO_Repeat.Margin = new System.Windows.Forms.Padding(1);
            this.DO_Repeat.Name = "DO_Repeat";
            this.DO_Repeat.Size = new System.Drawing.Size(50, 20);
            this.DO_Repeat.TabIndex = 324;
            this.DO_Repeat.Text = "0";
            this.DO_Repeat.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.DO_Repeat.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // StartDO_button
            // 
            this.StartDO_button.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StartDO_button.Location = new System.Drawing.Point(246, 18);
            this.StartDO_button.Name = "StartDO_button";
            this.StartDO_button.Size = new System.Drawing.Size(42, 22);
            this.StartDO_button.TabIndex = 322;
            this.StartDO_button.Text = "Start";
            this.StartDO_button.UseVisualStyleBackColor = true;
            this.StartDO_button.Click += new System.EventHandler(this.Start_button_Click);
            // 
            // label75
            // 
            this.label75.AutoSize = true;
            this.label75.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label75.Location = new System.Drawing.Point(12, 29);
            this.label75.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label75.Name = "label75";
            this.label75.Size = new System.Drawing.Size(67, 14);
            this.label75.TabIndex = 319;
            this.label75.Text = "Interval (ms)";
            // 
            // DO_interval
            // 
            this.DO_interval.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DO_interval.Location = new System.Drawing.Point(81, 19);
            this.DO_interval.Margin = new System.Windows.Forms.Padding(1);
            this.DO_interval.Name = "DO_interval";
            this.DO_interval.Size = new System.Drawing.Size(50, 20);
            this.DO_interval.TabIndex = 318;
            this.DO_interval.Text = "0";
            this.DO_interval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.DO_interval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // label62
            // 
            this.label62.AutoSize = true;
            this.label62.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label62.Location = new System.Drawing.Point(62, 84);
            this.label62.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(59, 14);
            this.label62.TabIndex = 366;
            this.label62.Text = "Width (ms)";
            // 
            // dwell
            // 
            this.dwell.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dwell.Location = new System.Drawing.Point(66, 101);
            this.dwell.Margin = new System.Windows.Forms.Padding(1);
            this.dwell.Name = "dwell";
            this.dwell.Size = new System.Drawing.Size(50, 20);
            this.dwell.TabIndex = 365;
            this.dwell.Text = "0";
            this.dwell.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.dwell.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(10, 84);
            this.label12.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(45, 14);
            this.label12.TabIndex = 364;
            this.label12.Text = "#Pulses";
            // 
            // pulseN
            // 
            this.pulseN.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pulseN.Location = new System.Drawing.Point(7, 101);
            this.pulseN.Margin = new System.Windows.Forms.Padding(1);
            this.pulseN.Name = "pulseN";
            this.pulseN.Size = new System.Drawing.Size(50, 20);
            this.pulseN.TabIndex = 363;
            this.pulseN.Text = "0";
            this.pulseN.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.pulseN.KeyDown += new System.Windows.Forms.KeyEventHandler(this.digital_generic_KeyDown);
            // 
            // label93
            // 
            this.label93.AutoSize = true;
            this.label93.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label93.Location = new System.Drawing.Point(165, 116);
            this.label93.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label93.Name = "label93";
            this.label93.Size = new System.Drawing.Size(85, 14);
            this.label93.TabIndex = 382;
            this.label93.Text = "Output rate (Hz)";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Location = new System.Drawing.Point(23, 311);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(612, 161);
            this.panel1.TabIndex = 395;
            this.panel1.TabStop = false;
            // 
            // UncageOnlyPanel
            // 
            this.UncageOnlyPanel.Controls.Add(this.label15);
            this.UncageOnlyPanel.Controls.Add(this.U_counter2);
            this.UncageOnlyPanel.Controls.Add(this.label6);
            this.UncageOnlyPanel.Controls.Add(this.StartDO_button);
            this.UncageOnlyPanel.Controls.Add(this.label75);
            this.UncageOnlyPanel.Controls.Add(this.DO_interval);
            this.UncageOnlyPanel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UncageOnlyPanel.Location = new System.Drawing.Point(333, 40);
            this.UncageOnlyPanel.Name = "UncageOnlyPanel";
            this.UncageOnlyPanel.Size = new System.Drawing.Size(303, 54);
            this.UncageOnlyPanel.TabIndex = 369;
            this.UncageOnlyPanel.TabStop = false;
            this.UncageOnlyPanel.Text = "Digital output now";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(18, 16);
            this.label15.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(52, 14);
            this.label15.TabIndex = 413;
            this.label15.Text = "Pulse-set";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // U_counter2
            // 
            this.U_counter2.AutoSize = true;
            this.U_counter2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.U_counter2.Location = new System.Drawing.Point(185, 23);
            this.U_counter2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.U_counter2.Name = "U_counter2";
            this.U_counter2.Size = new System.Drawing.Size(22, 14);
            this.U_counter2.TabIndex = 401;
            this.U_counter2.Text = "1/1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(144, 23);
            this.label6.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 14);
            this.label6.TabIndex = 400;
            this.label6.Text = "Repeat:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ActiveHighCheck);
            this.groupBox1.Controls.Add(this.ChannelComboBox);
            this.groupBox1.Controls.Add(this.ChannelLabel);
            this.groupBox1.Controls.Add(this.Uncage_Save);
            this.groupBox1.Controls.Add(this.PulseName);
            this.groupBox1.Controls.Add(this.PulseNumber);
            this.groupBox1.Controls.Add(this.label97);
            this.groupBox1.Controls.Add(this.OutputRate);
            this.groupBox1.Controls.Add(this.Length);
            this.groupBox1.Controls.Add(this.label90);
            this.groupBox1.Controls.Add(this.Delay);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.ISI);
            this.groupBox1.Controls.Add(this.label62);
            this.groupBox1.Controls.Add(this.dwell);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.pulseN);
            this.groupBox1.Controls.Add(this.label93);
            this.groupBox1.Controls.Add(this.label92);
            this.groupBox1.Location = new System.Drawing.Point(8, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(319, 179);
            this.groupBox1.TabIndex = 400;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Pulse set";
            // 
            // ChannelComboBox
            // 
            this.ChannelComboBox.FormattingEnabled = true;
            this.ChannelComboBox.Location = new System.Drawing.Point(64, 28);
            this.ChannelComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.ChannelComboBox.Name = "ChannelComboBox";
            this.ChannelComboBox.Size = new System.Drawing.Size(62, 21);
            this.ChannelComboBox.TabIndex = 394;
            this.ChannelComboBox.SelectedIndexChanged += new System.EventHandler(this.ChannelComboBox_SelectedIndexChanged);
            // 
            // ChannelLabel
            // 
            this.ChannelLabel.AutoSize = true;
            this.ChannelLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ChannelLabel.Location = new System.Drawing.Point(14, 30);
            this.ChannelLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ChannelLabel.Name = "ChannelLabel";
            this.ChannelLabel.Size = new System.Drawing.Size(46, 14);
            this.ChannelLabel.TabIndex = 393;
            this.ChannelLabel.Text = "Channel";
            // 
            // ShowRepeat
            // 
            this.ShowRepeat.AutoSize = true;
            this.ShowRepeat.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ShowRepeat.Location = new System.Drawing.Point(23, 287);
            this.ShowRepeat.Name = "ShowRepeat";
            this.ShowRepeat.Size = new System.Drawing.Size(261, 18);
            this.ShowRepeat.TabIndex = 401;
            this.ShowRepeat.Text = "Show whole trace for \"Sync with Frames\" mode";
            this.ShowRepeat.UseVisualStyleBackColor = true;
            this.ShowRepeat.CheckedChanged += new System.EventHandler(this.ShowRepeat_CheckedChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(646, 24);
            this.menuStrip1.TabIndex = 402;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // ActiveHighCheck
            // 
            this.ActiveHighCheck.AutoSize = true;
            this.ActiveHighCheck.Location = new System.Drawing.Point(13, 62);
            this.ActiveHighCheck.Name = "ActiveHighCheck";
            this.ActiveHighCheck.Size = new System.Drawing.Size(79, 17);
            this.ActiveHighCheck.TabIndex = 395;
            this.ActiveHighCheck.Text = "Active high";
            this.ActiveHighCheck.UseVisualStyleBackColor = true;
            this.ActiveHighCheck.Click += new System.EventHandler(this.ActiveHighCheck_Click);
            // 
            // Digital_Trigger_Panel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 487);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.DO_Repeat);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox10);
            this.Controls.Add(this.UncageOnlyPanel);
            this.Controls.Add(this.ShowRepeat);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Digital_Trigger_Panel";
            this.Text = "Digital Output Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Digital_Trigger_Panel_FormClosing);
            this.Load += new System.EventHandler(this.Digital_Trigger_Panel_Load);
            this.Shown += new System.EventHandler(this.Digital_Trigger_Panel_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.PulseNumber)).EndInit();
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panel1)).EndInit();
            this.UncageOnlyPanel.ResumeLayout(false);
            this.UncageOnlyPanel.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.Button Uncage_Save;
        public System.Windows.Forms.TextBox PulseName;
        public System.Windows.Forms.NumericUpDown PulseNumber;
        public System.Windows.Forms.Label label97;
        public System.Windows.Forms.TextBox OutputRate;
        public System.Windows.Forms.Label label92;
        public System.Windows.Forms.TextBox Length;
        public System.Windows.Forms.Label label90;
        public System.Windows.Forms.TextBox Delay;
        public System.Windows.Forms.Label label10;
        public System.Windows.Forms.TextBox ISI;
        public System.Windows.Forms.GroupBox groupBox10;
        public System.Windows.Forms.RadioButton SyncWithSlice_Check;
        public System.Windows.Forms.RadioButton SyncWithFrame_Check;
        public System.Windows.Forms.TextBox SliceInterval;
        public System.Windows.Forms.TextBox SlicesBeforePulse;
        public System.Windows.Forms.Label label73;
        public System.Windows.Forms.Label label71;
        public System.Windows.Forms.TextBox FrameInterval;
        public System.Windows.Forms.Label label63;
        public System.Windows.Forms.Label label5;
        public System.Windows.Forms.TextBox DO_Repeat;
        public System.Windows.Forms.Label U_counter;
        public System.Windows.Forms.Button StartDO_button;
        public System.Windows.Forms.Label label75;
        public System.Windows.Forms.TextBox DO_interval;
        public System.Windows.Forms.Label label62;
        public System.Windows.Forms.TextBox dwell;
        public System.Windows.Forms.Label label12;
        public System.Windows.Forms.TextBox pulseN;
        public System.Windows.Forms.Label label93;
        public System.Windows.Forms.PictureBox panel1;
        public System.Windows.Forms.GroupBox UncageOnlyPanel;
        public System.Windows.Forms.Label label7;
        public System.Windows.Forms.Label U_counter2;
        public System.Windows.Forms.Label label6;
        public System.Windows.Forms.TextBox FramesBeforePulse;
        public System.Windows.Forms.Label label8;
        public System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.TextBox FrameInterval_ms;
        public System.Windows.Forms.Label label15;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox FrameBeforePulse_ms;
        public System.Windows.Forms.Label label11;
        public System.Windows.Forms.Label label9;
        public System.Windows.Forms.CheckBox ShowRepeat;
        public System.Windows.Forms.Label label14;
        public System.Windows.Forms.Label RepeatFrame;
        public System.Windows.Forms.MenuStrip menuStrip1;
        public System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        public System.Windows.Forms.Label SliceInterval_s;
        public System.Windows.Forms.Label BaseLine_Slice_s;
        public System.Windows.Forms.Label label17;
        public System.Windows.Forms.Label label19;
        public System.Windows.Forms.Label label18;
        public System.Windows.Forms.Label SliceNote;
        public System.Windows.Forms.Label FrameNote;
        public System.Windows.Forms.Label ChannelLabel;
        private System.Windows.Forms.ComboBox ChannelComboBox;
        private System.Windows.Forms.CheckBox ActiveHighCheck;
    }
}