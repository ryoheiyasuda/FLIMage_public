using MathLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace FLIMage.Plugins
{
    public partial class Image_sequence2 : Form
    {
        FLIMageMain flimage;
        int settingIDLargest = 0;

        WindowLocManager winManager;
        String WindowName = "ImageSequence2.loc";
        String SettingFolderName = "ImageSequence2";
        String SettingFolderPath;
        String SettingNameHeader = "ImageSeq2Setting-";
        String ImageSeqFileName = "ImageSeq2.seq";
        String ImageSeqTemplateFileName = "ImageSeq2Template.seq";
        String ImageSeqFilePath;
        Char Separator = '\t';

        int[] fileCounters_eachPos;
        Stopwatch timer_rep = new Stopwatch();
        Stopwatch total_time = new Stopwatch();
        bool using_previousSetting = false;
        int current_sequence_pos = 0;
        List<ImSeqUnit> im_seq_unit_list = new List<ImSeqUnit>();

        bool running_current = false;
        public bool image_seq_running = false;
        bool abortGrabActivated = false;
        bool pause = false;
        int CheckingInterval_ms = 200;

        int procedure_selectedRawIndex = 0;
        int startPos = 0;

        double zoom = 30;

        List<ushort[,]> template_imageYX = new List<ushort[,]>();
        ushort[,] current_imageXY;
        ushort[][][,,] FLIM_5D;
        List<ushort[][,]> template_imageZYX = new List<ushort[][,]>(); //[pos][z][x,y]
        ushort[][,] imageZYX; //[z][x,y]
        int nSlices = 0;
        Bitmap templateBMP, templateBMP_XZ;
        int correctionChannel = 0;

        List<ImageSeqProcedure> procedureList = new List<ImageSeqProcedure>();
        PlotOnPictureBox plot;

        List<String> template_file_names = new List<string>();
        List<double[][]> PositionList = new List<double[][]>();
        int currentMotor_Pos = -1;
        int numMotorPos = 0;
        double[] centerPositionXY;

        int exclusive_indx = 5;

        SettingManager settingManager;
        String settingName = "ImageSequence2";


        public Image_sequence2(FLIMageMain FLIMage_in)
        {
            InitializeComponent();
            DataGridViewComboBoxColumn procColumn = new DataGridViewComboBoxColumn();
            procColumn.Name = "Procedure";
            procColumn.HeaderText = "Procedure";
            procColumn.Items.Add("Imaging");
            procColumn.Items.Add("Imaging + Uncaging");
            procColumn.Items.Add("Imaging + DO");
            procColumn.Items.Add("Imaging + Ephys");
            procColumn.Items.Add("Uncaging");
            procColumn.Items.Add("Uncaging + Ephys");
            procColumn.Items.Add("DO");
            procColumn.Items.Add("Ephys");
            procColumn.Width = 130;
            ImageSequenceGridView.Columns.Add(procColumn);
            ImageSequenceGridView.CurrentCellDirtyStateChanged += new EventHandler(dataGridView1_CellValueChanged);
            PositionDataGridView.CurrentCellDirtyStateChanged += new EventHandler(dataGridView1_CellValueChanged);

            flimage = FLIMage_in;
            //ImageSequenceGridView.AllowUserToAddRows = false;
            ImageSequenceGridView.Columns["SettingID"].ReadOnly = true;
            exclusive_indx = ImageSequenceGridView.Columns["Exclusive"].Index;
            SettingFolderPath = Path.Combine(flimage.State.Files.initFolderPath, SettingFolderName);
            Directory.CreateDirectory(SettingFolderPath);

            ImageSeqFilePath = Path.Combine(SettingFolderPath, ImageSeqFileName);

            string[] fileArray = Directory.GetFiles(@SettingFolderPath, SettingNameHeader + "*.txt");
            settingIDLargest = 0;

            Channel_Pulldown.SelectedIndex = 0; // flimage.image_display.currentChannel;           
            flimage.flimage_io.EventNotify += new FLIMage_IO.FLIMage_EventHandler(EventHandling);

            plot = new PlotOnPictureBox(PlotBox);

            //template_imageYX.Add(new ushort[128, 128]);
            ZoomTextBox.Text = zoom.ToString();

            ApplyBMP();
            LoadImageSeqFile();
            LoadTemplateInfo();
        }

        /// <summary>
        /// This runs when image sequence panel is loaded. It loads WindowLocManager which remembers windows position in the previous use. 
        /// It also starts settingManager. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Image_seqeunce_Load(object sender, EventArgs e)
        {
            winManager = new WindowLocManager(this, WindowName, flimage.State.Files.windowsInfoPath);
            winManager.LoadWindowLocation(false);
            InitializeSetting();
            PauseButton.Visible = false; //Pause button is visible only when the imaging sequence tool is running.
        }

        /// <summary>
        /// settingManager allows you to automatically save the setting. It works for checkbox, ToolStripMenuItem and textbox for now.
        /// Just need to add the object to the dictionary. You need both "Save SetSetting" and "InitizliaeSetting".
        /// </summary>
        void InitializeSetting()
        {
            settingManager = new SettingManager(settingName, flimage.State.Files.initFolderPath);
            settingManager.AddToDict(XYCorrect_CB);
            settingManager.AddToDict(ZCorrect_CB);
            settingManager.AddToDict(UseMirror_CB);
            settingManager.AddToDict(TimeShift_textBox);
            settingManager.AddToDict(Analyze_checkBox);
            settingManager.LoadToObject();
        }

        /// <summary>
        /// settingManager allows you to automatically save the setting. It works for checkbox, ToolStripMenuItem and textbox for now.
        /// Just need to add the object to the dictionary. You need both "Save SetSetting" and "InitizliaeSetting".
        /// </summary>
        public void SaveSetting()
        {
            if (settingManager != null)
            {
                settingManager.SaveFromObject();
            }
        }

        /// <summary>
        /// SaveWindowLocation is the function called before the image is closed by FLIMage.
        /// It should be also add to XXX_FormClosing.
        /// </summary>
        public void SaveWindowLocation()
        {
            SaveTable();
            SaveSetting();
            if (winManager != null)
                winManager.SaveWindowLocation();
        }

        /// <summary>
        /// Called when this form is closed. flimage. ToolWindowClosed should be called to tel FLIMage the status of this window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Image_seqeunce_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowLocation();

            e.Cancel = true;
            this.Hide();

            flimage.ToolWindowClosed();
        }

        /// <summary>
        /// Save State in setting file.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="state"></param>
        void SaveSettingFile(int ID, ScanParameters state)
        {
            String fn = Path.GetFileNameWithoutExtension(flimage.State.Files.initFileName);
            FileIO fo = new FileIO(state);
            String str1 = fo.AllSetupValues_nonDevice();
            str1 = fn + ";" + "\r\n" + str1;
            string fileName = StateFileName(ID);
            File.WriteAllText(fileName, str1);
        }

        /// <summary>
        /// Called when "Add" button is clicked. This will add FLIMage setting to the table.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddCurrentSetting_Click(object sender, EventArgs e)
        {
            var State = flimage.State;
            SetLargestID();
            settingIDLargest += 1;
            String fn = Path.GetFileNameWithoutExtension(State.Files.initFileName);
            ImageSequenceGridView.Rows.Add(settingIDLargest, fn, State.Acq.imageInterval, State.Acq.nImages, false, "Imaging");

            int nRows = ImageSequenceGridView.Rows.Count;
            ImageSequenceGridView.CurrentCell = ImageSequenceGridView[0, nRows - 1];

            SaveSettingFile(settingIDLargest, State);

            var state1 = importFromPhysiology();

            EditProc(state1, nRows - 1);
            SaveTable();
            SaveWindowLocation();
            PlotProcedures(true, false);
        }

        /// <summary>
        /// This function import physiology parameters to State.
        /// </summary>
        /// <returns></returns>
        public ScanParameters importFromPhysiology()
        {
            if (flimage.physiology != null && !flimage.physiology.IsDisposed)
                flimage.State = flimage.fileIO.CopyPhysiologyParamToState(flimage.physiology.phys_parameters);
            else
                flimage.State.Ephys.Ephys_on = false;

            return flimage.State;
        }

        public void ReplaceWithCurrent_Click(object sender, EventArgs e)
        {
            int num = Convert.ToInt32(ImageSequenceGridView.Rows[procedure_selectedRawIndex].Cells["SettingID"].Value);

            if (flimage.physiology != null && !flimage.physiology.IsDisposed)
                flimage.State = flimage.fileIO.CopyPhysiologyParamToState(flimage.physiology.phys_parameters);
            else
                flimage.State.Ephys.Ephys_on = false;

            flimage.State = importFromPhysiology();
            EditProc(flimage.State, procedure_selectedRawIndex);
            //SaveSettingFile(num, flimage.State);

            PlotProcedures(true, false);
        }

        public String StateFileName(int settingNumber)
        {
            return Path.Combine(SettingFolderPath, SettingNameHeader + settingNumber) + ".txt";
        }

        public void SaveTable()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < ImageSequenceGridView.Rows.Count; i++)
            {
                for (int j = 0; j < ImageSequenceGridView.ColumnCount; j++)
                {
                    builder.Append(ImageSequenceGridView[j, i].Value);
                    if (j != ImageSequenceGridView.ColumnCount - 1)
                        builder.Append(Separator);
                }
                builder.AppendLine();
            }
            File.WriteAllText(ImageSeqFilePath, builder.ToString());
        }

        public void LoadImageSeqFile()
        {
            if (File.Exists(ImageSeqFilePath))
            {
                ImageSequenceGridView.Rows.Clear();
                String[] lines = File.ReadAllLines(ImageSeqFilePath);
                for (int j = 0; j < lines.Length; j++)
                {
                    String[] sP = lines[j].Split(Separator);
                    int fileNum = Convert.ToInt32(sP[0]);
                    String fileName = StateFileName(fileNum);

                    var state1 = new ScanParameters();
                    var fileIO = new FileIO(state1);

                    if (File.Exists(fileName))
                    {
                        Array.Resize(ref sP, ImageSequenceGridView.ColumnCount);
                        if (sP[sP.Length - 1] == null)
                            sP[sP.Length - 1] = "Imaging";

                        ImageSequenceGridView.Rows.Add(sP);
                        if (fileNum > settingIDLargest)
                            settingIDLargest = fileNum;

                        fileIO.LoadSetupFile(fileName);
                        EditProc(fileIO.State, j);
                    }
                }

                PlotProcedures(true, true);
            }
        }

        public void SettingID_UpDown(object sender, EventArgs e)
        {
            int selected = ImageSequenceGridView.CurrentCell.RowIndex;
            var row = ImageSequenceGridView.Rows[selected];
            var data = procedureList[selected];

            if (sender.Equals(RowUp) && selected > 0)
            {
                ImageSequenceGridView.Rows.RemoveAt(selected);
                procedureList.RemoveAt(selected);

                selected--;

                ImageSequenceGridView.Rows.Insert(selected, row);
                procedureList.Insert(selected, data);
            }

            else if (sender.Equals(RowDown) && selected < ImageSequenceGridView.Rows.Count - 1)
            {
                ImageSequenceGridView.Rows.RemoveAt(selected);
                procedureList.RemoveAt(selected);

                selected += 1;

                ImageSequenceGridView.Rows.Insert(selected, row);
                procedureList.Insert(selected, data);
            }

            ImageSequenceGridView.CurrentCell = ImageSequenceGridView[0, selected];

            SaveTable();
            SaveWindowLocation();
            PlotProcedures(true, false);
        }

        public void DeleteRow_Click(object sender, EventArgs e)
        {
            int selected = ImageSequenceGridView.CurrentCell.RowIndex;
            int num = Convert.ToInt32(ImageSequenceGridView.Rows[selected].Cells["SettingID"].Value);
            ImageSequenceGridView.Rows.RemoveAt(selected);
            String fn = StateFileName(num);
            if (File.Exists(fn))
                File.Delete(fn);

            procedureList.RemoveAt(selected);
            SetLargestID();

            SaveTable();
            SaveWindowLocation();
            PlotProcedures(true, true);
        }

        public void SetLargestID()
        {
            int max = 0;
            for (int i = 0; i < ImageSequenceGridView.Rows.Count; i++)
            {
                int num = Convert.ToInt32(ImageSequenceGridView.Rows[i].Cells["SettingID"].Value);
                if (max < num)
                    max = num;
            }
            settingIDLargest = max;
        }

        public void ClearSetting_Click(object sender, EventArgs e)
        {
            settingIDLargest = 0;
            ImageSequenceGridView.Rows.Clear();
            string[] fileArray = Directory.GetFiles(SettingFolderPath, SettingNameHeader + "*.txt");
            foreach (string file in fileArray)
            {
                if (File.Exists(file))
                    File.Delete(file);
            }

            procedureList.Clear();
        }

        public void RunSeq_Click(object sender, EventArgs e)
        {
            StartSequence(false);
        }

        void StartSequence(bool restart)
        {
            RunSeq.Enabled = false;
            var State = flimage.State;
            running_current = false;

            if (using_previousSetting)
            {
                DialogResult dialogResult = MessageBox.Show("You are using previously stored templates. Are you sure?", "Warning...", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                    using_previousSetting = false;
                else
                {
                    using_previousSetting = false;
                    return;
                }
            }

            if (restart)
                ProduceSequence();

            //Problem check.
            AnyUncagingOrDO(out bool anyDO, out bool anyUncaging, out bool anyEphys);
            if (anyUncaging && flimage.uncaging_panel == null)
            {
                MessageBox.Show("Please open uncaging panel!!");
                return;
            }

            if (anyDO && flimage.digital_panel == null)
            {
                MessageBox.Show("Please open DO panel!!");
                return;
            }

            if (anyEphys && flimage.physiology == null)
            {
                MessageBox.Show("Please open E-phys panel!!");
                return;
            }

            if (RunSeq.Text == "Abort")
            {
                abortGrabActivated = true;
                ForceStopAll();
                if (pause && !image_seq_running)
                {
                    this.InvokeIfRequired(o => o.total_time.Stop());
                    this.InvokeIfRequired(o => o.GUIStartStop(false));
                }
                pause = false;
            }
            else
            {
                pause = false;
                abortGrabActivated = false;

                if (StartFromCurrentCheck.Checked || restart)
                {
                    startPos = current_sequence_pos;
                }

                this.InvokeIfRequired(o => o.GUIStartStop(true));
                Task.Factory.StartNew((Action)delegate
                {
                    RunSequence(restart);
                    if (!pause)
                        this.InvokeAnyway(o => o.GUIStartStop(false));
                });
            }

            RunSeq.Enabled = true;
        }

        public void reportProgress()
        {
            this.InvokeIfRequired(o =>
            {
                o.RepTime.Text = String.Format("{0:0.0}", o.timer_rep.ElapsedMilliseconds / 1000.0);
                o.totalTimeLabel.Text = String.Format("{0:0.0}", o.total_time.ElapsedMilliseconds / 1000.0);

                if (!o.image_seq_running && o.pause)
                {
                    o.PauseButton.Font = new Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    o.PauseButton.Text = "Restart";
                    o.RunSeq.Enabled = true;
                }

                o.PlotProcedures(false, true);
                if (o.numMotorPos > 0)
                {
                    o.PositionDataGridView.CurrentCell = PositionDataGridView[0, currentMotor_Pos];
                    o.PositionDataGridView.Rows[currentMotor_Pos].Selected = true;
                }
                o.ApplyBMP();
            });
        }

        public void LoadSetting(String fileName)
        {
            flimage.InvokeIfRequired(o => o.LoadSettingFile(fileName, false));
        }

        public void AnyUncagingOrDO(out bool anyDO, out bool anyUncaging, out bool anyEphys)
        {
            anyUncaging = false;
            anyDO = false;
            anyEphys = false;
            for (int i = 0; i < ImageSequenceGridView.Rows.Count; i++)
            {
                bool imaging = ((String)(ImageSequenceGridView.Rows[i].Cells["Procedure"].Value)).Contains("Imaging");
                bool uncaging = ((String)(ImageSequenceGridView.Rows[i].Cells["Procedure"].Value)).Contains("Uncaging");
                bool DO = ((String)(ImageSequenceGridView.Rows[i].Cells["Procedure"].Value)).Contains("DO");
                bool ephys = ((String)(ImageSequenceGridView.Rows[i].Cells["Procedure"].Value)).Contains("Ephys");
                if (uncaging)
                {
                    anyUncaging = true;
                    break;
                }
                if (DO)
                {
                    anyDO = true;
                    break;
                }
                if (ephys)
                {
                    anyEphys = true;
                    break;
                }
            }
        }

        public void RunSequence(bool restart)
        {
            var State = flimage.State;

            //Start Running.
            image_seq_running = true;
            flimage.flimage_io.imageSequencing = true;
            if (!restart)
            {
                this.InvokeIfRequired(o => o.total_time.Restart());
                fileCounters_eachPos = new int[numMotorPos];
            }
            //

            if (numMotorPos > 1)
            {
                var next_motor_num = im_seq_unit_list[startPos].position;
                GoToPosition(next_motor_num, true);
            }

            if (SortFileNames_checkBox.Checked)
                flimage.InvokeIfRequired(o => o.TurnOn_AnalyzeAfterAcquisition(false));

            for (int loop = 0; loop < (LoopCheck.Checked ? 1000 : 1); loop++)
            {
                if (loop > 0)
                {
                    startPos = 0;
                }

                for (int i = startPos; i < im_seq_unit_list.Count; i++)
                {
                    this.InvokeIfRequired(o => o.timer_rep.Restart());

                    current_sequence_pos = i;
                    reportProgress();

                    var currentSeq = im_seq_unit_list[current_sequence_pos];
                    var currentUnit = procedureList[currentSeq.proc_no];
                    var currentMeth = currentUnit.procedure_type;
                    var currentPos = currentSeq.position;
                    double interval_ms = currentUnit.Interval_ms;

                    LoadSelectedSetting();

                    State.Motor.positionID = im_seq_unit_list[startPos].position;
                    State.Uncaging.uncage_whileImage = false;
                    State.Ephys.sync_with_image = false;
                    State.Ephys.sync_with_uncage = false;
                    State.DO.DO_whileImage = false;
                    bool start_imaging = false;
                    bool start_ephys = false;
                    bool start_uncaging = false;
                    bool start_do = false;


                    if (currentMeth == ProcedureType.Imaging_Uncaging)
                    {
                        State.Uncaging.uncage_whileImage = true;
                        start_imaging = true;
                    }
                    else if (currentMeth == ProcedureType.Imaging_Uncaging_Ephys)
                    {
                        State.Uncaging.uncage_whileImage = true;
                        State.Ephys.sync_with_image = true;
                        start_imaging = true;
                        start_ephys = true;
                    }
                    else if (currentMeth == ProcedureType.Imaging)
                    {
                        start_imaging = true;
                    }
                    else if (currentMeth == ProcedureType.Imaging_DO)
                    {
                        start_imaging = true;
                        State.DO.DO_whileImage = true;
                    }
                    else if (currentMeth == ProcedureType.Ephys)
                    {
                        start_ephys = true;
                    }
                    else if (currentMeth == ProcedureType.Uncaging)
                    {
                        start_uncaging = true;
                    }
                    else if (currentMeth == ProcedureType.Uncaging_Ephys)
                    {
                        start_ephys = true;
                        start_uncaging = true;
                        State.Ephys.sync_with_uncage = true;
                    }
                    else if (currentMeth == ProcedureType.DO)
                    {
                        start_do = true;
                    }


                    flimage.ExternalCommand("UpdateGUI");

                    if (start_ephys)
                    {
                        flimage.fileIO.State = flimage.State;
                        flimage.physiology.phys_parameters = flimage.fileIO.CopyFromStateToPhysParameters();
                        //Perhaps not necessary.
                        flimage.physiology.phys_parameters.sync_with_image = start_imaging;
                        flimage.physiology.phys_parameters.sync_with_uncage = start_uncaging;
                        //
                        flimage.physiology.phys_parameters.acquire_data = true;
                        State.Ephys.Ephys_on = true;
                        flimage.physiology.InvokeAnyway(o => o.LoadValuesToPanel(true));

                        Ephys_Once();
                    }

                    if (start_uncaging)
                    {
                        State.Uncaging.trainRepeat = 1;
                        flimage.uncaging_panel.InvokeAnyway(o => o.UpdateUncaging(flimage.uncaging_panel));
                        UncageOnce();
                    }

                    if (start_do)
                    {
                        State.DO.trainRepeat = 1;
                        flimage.digital_panel.InvokeAnyway(o => o.UpdateDO(flimage.digital_panel));
                    }

                    if (start_imaging)
                        ImageOnce();

                    System.Threading.Thread.Sleep(CheckingInterval_ms);

                    running_current = true;

                    while (running_current)
                    {
                        bool running_current1 = false;

                        if (start_imaging)
                            running_current1 = running_current1 || (flimage.flimage_io.grabbing || flimage.flimage_io.post_grabbing_process);

                        if (start_uncaging)
                            running_current1 = running_current1 || flimage.uncaging_panel.uncaging_running;

                        if (start_do)
                            running_current1 = running_current1 || flimage.digital_panel.digital_running;

                        if (start_ephys)
                            running_current1 = running_current1 || flimage.physiology.stim_running;

                        running_current = running_current1;

                        if (abortGrabActivated || !running_current)
                            break;

                        System.Threading.Thread.Sleep(CheckingInterval_ms);

                        reportProgress();
                    }


                    //Sort file names if required.
                    if (SortFileNames_checkBox.Checked && numMotorPos > 0)
                    {
                        string org_fname = flimage.image_display.FLIM_ImgData.State.Files.fullName();
                        if (File.Exists(org_fname))
                        {
                            string fullFilename = "";
                            fileCounters_eachPos[currentPos]++;
                            string dirName = Path.GetDirectoryName(org_fname);
                            string basename = flimage.image_display.FLIM_ImgData.State.Files.baseName + "_POS" + (currentPos + 1) + "_";

                            for (int j = 0; j < 1000; j++)
                            {
                                string filename = basename + fileCounters_eachPos[currentPos].ToString("000") + ".flim";
                                fullFilename = Path.Combine(dirName, filename);
                                if (File.Exists(fullFilename))
                                    fileCounters_eachPos[currentPos]++;
                                else
                                {
                                    File.Move(org_fname, fullFilename);
                                    this.InvokeIfRequired(o => o.NewFileName.Text = "Renamed: " + filename);
                                    break;
                                }
                            }

                            //Analyze the file.
                            if (Analyze_checkBox.Checked)
                            {
                                var image_display = flimage.image_display;
                                if (image_display != null)
                                {
                                    image_display.InvokeIfRequired(o =>
                                    {
                                        o.plot_regular.Show();
                                        o.plot_regular.Activate();
                                        o.OpenFLIM(fullFilename, true, o.plot_regular.calc_upon_open, false);
                                    });
                                }
                            }
                        }

                    }

                    //Update current position to next!
                    if (i + 1 >= im_seq_unit_list.Count && LoopCheck.Checked)
                        current_sequence_pos = 0;
                    else if (i + 1 < im_seq_unit_list.Count)
                        current_sequence_pos = i + 1;


                    //Should go for drift corretion caclculation.
                    //If looping, going to zero.
                    if (numMotorPos > 1)
                    {
                        var next_motor_num = im_seq_unit_list[current_sequence_pos].position;
                        var warning = (currentSeq.proc_no == 0 && currentSeq.repeat == 0);
                        GoToPosition(next_motor_num, warning);
                    }

                    while (interval_ms - (int)timer_rep.ElapsedMilliseconds > CheckingInterval_ms)
                    {
                        if (abortGrabActivated || pause)
                            break;
                        System.Threading.Thread.Sleep(CheckingInterval_ms);
                        reportProgress();
                    }

                    if ((int)timer_rep.ElapsedMilliseconds < interval_ms)
                    {
                        if (abortGrabActivated || pause)
                            break;
                        System.Threading.Thread.Sleep((int)interval_ms - (int)timer_rep.ElapsedMilliseconds);
                        reportProgress();
                    }

                    if (abortGrabActivated || pause)
                        break;
                } //rep

                if (abortGrabActivated || pause)
                    break;
            } //loop


            ForceStopAll();

            flimage.flimage_io.imageSequencing = false;
            flimage.InvokeIfRequired(o => o.ChangeItemsStatus(true, false));

            if (!pause)
                this.InvokeIfRequired(o => o.total_time.Stop());

            image_seq_running = false;
            running_current = false;

            this.InvokeIfRequired(o => o.GUIStartStop(false));
            reportProgress();

            if (pause && !image_seq_running)
            {
                Task.Factory.StartNew((Action)delegate
                {
                    while (pause)
                    {
                        System.Threading.Thread.Sleep(CheckingInterval_ms);
                        reportProgress();
                    }
                });
            }
        }

        private void GUIStartStop(bool start)
        {
            if (start)
                RunSeq.Text = "Abort";
            else
                RunSeq.Text = "Run sequence";
            RunSeq.Enabled = !start;
            PauseButton.Visible = start;
        }

        private void ForceStopAll()
        {
            if (flimage.flimage_io.grabbing || flimage.flimage_io.post_grabbing_process)
            {
                flimage.ExternalCommand("StopGrab");
                while (flimage.flimage_io.grabbing || flimage.flimage_io.post_grabbing_process)
                    System.Threading.Thread.Sleep(CheckingInterval_ms);
            }

            if (flimage.uncaging_panel != null && flimage.uncaging_panel.uncaging_running)
            {
                flimage.ExternalCommand("StopUncaging");
                while (flimage.uncaging_panel.uncaging_running)
                    System.Threading.Thread.Sleep(CheckingInterval_ms);
            }

            if (flimage.digital_panel != null && flimage.digital_panel.digital_running)
            {
                flimage.ExternalCommand("StopDO");
                while (flimage.digital_panel.digital_running)
                    System.Threading.Thread.Sleep(CheckingInterval_ms);
            }

            if (flimage.physiology != null && flimage.physiology.stim_running)
            {
                flimage.ExternalCommand("StopEphys");
                while (flimage.physiology.stim_running)
                    System.Threading.Thread.Sleep(CheckingInterval_ms);
            }
        }

        public void ImageOnce()
        {
            flimage.ExternalCommand("StartGrab");
            Debug.WriteLine("Start Grab started : " + flimage.flimage_io.grabbing);
        }

        public void UncageOnce()
        {
            flimage.ExternalCommand("StartUncaging");
            if (flimage.uncaging_panel != null)
                Debug.WriteLine("Start Uncaging started : " + flimage.uncaging_panel.uncaging_running);
            else
                MessageBox.Show("Uncaging failed : please open uncaging panel");
        }

        public void DO_Once()
        {
            flimage.ExternalCommand("StartDO");
            if (flimage.digital_panel != null)
                Debug.WriteLine("Start DO started: " + flimage.digital_panel.digital_running);
            else
                MessageBox.Show("DO failed : please open digital output control panel");
        }

        public void Ephys_Once()
        {
            flimage.ExternalCommand("StartEphys");
            if (flimage.physiology != null)
                Debug.WriteLine("Start Ephys started: " + flimage.physiology.stim_running);
            else
                MessageBox.Show("DO failed : please open digital output control panel");
        }

        public void ImageSequenceGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            if (ImageSequenceGridView.CurrentCell != null)
            {
                procedure_selectedRawIndex = ImageSequenceGridView.CurrentCell.RowIndex;
                ImageSequenceGridView.Rows[procedure_selectedRawIndex].Selected = true;
                FillGUI_State(procedure_selectedRawIndex);
            }
        }

        public void LoadSetting_Number(int num)
        {
            String fileName = StateFileName(num);
            LoadSetting(fileName);
        }

        public void LoadSelected_Click(object sender, EventArgs e)
        {
            LoadSelectedSetting();
        }

        public void LoadSelectedSetting()
        {
            int num = procedureList[procedure_selectedRawIndex].ID;
            LoadSetting_Number(num);
            double.TryParse(ZoomTextBox.Text, out zoom);
            flimage.State.Acq.zoom = zoom;
            flimage.LoadPhysiologyParametersFromState();
            flimage.ReSetupValues(true);
        }

        public void EditProc(ScanParameters state, int selected)
        {
            int repetition = Convert.ToInt32(ImageSequenceGridView.Rows[selected].Cells["Repetition"].Value);
            int ID = Convert.ToInt32(ImageSequenceGridView.Rows[selected].Cells["SettingID"].Value);
            double interval_ms = 1000.0 * Convert.ToDouble(ImageSequenceGridView.Rows[selected].Cells["Interval"].Value);
            bool exclusive = Convert.ToBoolean(ImageSequenceGridView.Rows[selected].Cells["Exclusive"].Value);
            var proc_string = (String)(ImageSequenceGridView.Rows[selected].Cells["Procedure"].Value);

            ImageSeqProcedure im_seq_proc;
            var fileIO = new FileIO(state);

            if (selected >= procedureList.Count)
            {
                im_seq_proc = new ImageSeqProcedure(proc_string, repetition, interval_ms, exclusive, ID, state);
                for (int i = 0; i < selected + 1; i++)
                {
                    procedureList.Add(im_seq_proc);
                    if (selected == procedureList.Count - 1)
                        break;
                }
            }
            else
            {
                im_seq_proc = procedureList[selected];
                im_seq_proc = new ImageSeqProcedure(proc_string, repetition, interval_ms, exclusive, ID, state);
            }

            procedureList[selected] = im_seq_proc;

            FillGUI_State(selected);

            //PlotProcedures();
            SaveSettingFile(procedureList[selected].ID, procedureList[selected].State);
            SaveTable();
            SaveWindowLocation();
        }

        public void ImageSequenceGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

            ImageSequenceGridView.Rows[e.RowIndex].Selected = true;
            procedure_selectedRawIndex = e.RowIndex;
            EditProc(procedureList[procedure_selectedRawIndex].State, e.RowIndex);
            PlotProcedures(true, false);
        }

        public void dataGridView1_CellValueChanged(object sender, EventArgs e)
        {
            EditProc(procedureList[procedure_selectedRawIndex].State, procedure_selectedRawIndex);
            PlotProcedures(true, false);
        }

        private void ImageSequenceGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == exclusive_indx && e.RowIndex >= 0 && e.RowIndex < ImageSequenceGridView.RowCount)
            {
                ImageSequenceGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = !Convert.ToBoolean(ImageSequenceGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                ImageSequenceGridView.EndEdit();

                //EditProc(procedureList[e.RowIndex].State, e.RowIndex);
                //PlotProcedures();
            }

        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (pause)
            {
                if (!image_seq_running)
                {
                    PauseButton.Font = new Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    PauseButton.Text = "Pause";
                    pause = false;
                    StartSequence(true);
                }
                else
                {
                    PauseButton.Enabled = false;
                    PauseButton.Font = new Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    PauseButton.Text = "Pause after current";
                }
            }
            else //not pausing.
            {
                PauseButton.Enabled = false;
                PauseButton.Font = new Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                PauseButton.Text = "Pause after current";
                pause = true;
            }
        }



        public void SelectCurrentImage_Click(object sender, EventArgs e)
        {
            SelectImage(flimage.image_display.FLIM_ImgData);
        }


        public void ApplyBMP()
        {
            //            templateBMP = ImageProcessing.FormatImage(flimage.image_display.State_intensity_range[correctionChannel], template_image);
            if (currentMotor_Pos >= 0 && currentMotor_Pos < template_imageYX.Count &&
                template_imageYX != null && template_imageYX[currentMotor_Pos] != null)
                templateBMP = ImageProcessing.FormatImage(template_imageYX[currentMotor_Pos]);
            UpdateGUI();
            TemplateImage_PB.Invalidate();
        }

        public void TurnOnOffCorrection(bool ON)
        {
            DriftCorrection_CB.InvokeIfRequired(o => o.Checked = ON);
        }


        public void SelectImage(FLIMData flim)
        {
            correctionChannel = Channel_Pulldown.SelectedIndex;
            reconstructImageFromPages(flim);
            
            AddPosition();
            currentMotor_Pos = numMotorPos - 1;

            template_file_names[currentMotor_Pos] = flim.State.Files.fullName();
            template_imageYX[currentMotor_Pos] = getProjection(imageZYX);
            template_imageZYX[currentMotor_Pos] = new ushort[imageZYX.Length][,];
            for (int z = 0; z < imageZYX.Length; z++)
                template_imageZYX[currentMotor_Pos][z] = (ushort[,])imageZYX[z].Clone();

            PositionList[currentMotor_Pos] = GetPosition_Mirror_Motor_Combined(flim.State);

            ReformatPositionGridData();
            SaveTemplateInfo();
            ApplyBMP();
        }

        public double[][] GetPosition_Mirror_Motor_Combined(ScanParameters state)
        {
            double[][] position2;
            var position1 = (double[])state.Motor.motorPosition.Clone();

            double[] voltage_xy_shift = new double[] { state.Acq.XOffset, state.Acq.YOffset };
            double[] xy_shift_um = ImageParameterCalculation.voltage2micrometers_XY(voltage_xy_shift, state);

            if (state.Acq.nSplitScanning > 1)
            {
                int nSplit = state.Acq.nSplitScanning;
                position2 = new double[nSplit][];
                for (int j = 0; j < nSplit; j++)
                {
                    position2[j] = (double[])position1.Clone();
                    voltage_xy_shift = new double[] { state.Acq.XOffset_Split[j], state.Acq.YOffset_Split[j] };
                    xy_shift_um = ImageParameterCalculation.voltage2micrometers_XY(voltage_xy_shift, state);
                    for (int i = 0; i < 2; i++)
                        position2[j][i] += xy_shift_um[i];
                }

            }
            else
            {
                position2 = new double[1][];
                position2[0] = position1;
                for (int i = 0; i < 2; i++)
                    position2[0][i] += xy_shift_um[i];
            }

            return position2;
        }

        public void UpdateGUI()
        {
            if (currentMotor_Pos >= 0 && currentMotor_Pos < template_file_names.Count)
            {
                var filename1 = template_file_names[currentMotor_Pos];
                var fname1 = Path.GetFileName(filename1);
                TemplateFileName.Text = "Filename: " + fname1;
            }
        }

        public void SaveTemplateInfo()
        {
            correctionChannel = Channel_Pulldown.SelectedIndex;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Channel");
            sb.AppendLine(correctionChannel.ToString());
            sb.AppendLine("Images");
            foreach (var fn in template_file_names)
                sb.AppendLine(fn);
            sb.AppendLine("Positions");

            foreach (var pos in PositionList)
            {
                var lin_pos = MatrixCalc.Linearize2DJaggedArray(pos);
                var string1 = String.Join(",", lin_pos.Select(x => x.ToString()));
                sb.AppendLine(string1);
            }

            var fname = Path.Combine(SettingFolderPath, ImageSeqTemplateFileName);
            File.WriteAllText(fname, sb.ToString());
        }

        public void LoadTemplateInfo()
        {
            var fname = Path.Combine(SettingFolderPath, ImageSeqTemplateFileName);
            if (!File.Exists(fname))
                return;

            string[] lines = File.ReadAllLines(fname);
            string reading = "image";
            template_file_names.Clear();
            PositionList.Clear();
            PositionDataGridView.Rows.Clear();

            int channel_to_analyze = 0;
            foreach (var line in lines)
            {
                if (line == "Channel")
                    reading = "channel";
                else if (line == "Images")
                    reading = "image";
                else if (line == "Positions")
                    reading = "position";
                else if (reading == "channel")
                {
                    if (line != "")
                        channel_to_analyze = Convert.ToInt32(line);
                }
                else if (reading == "image")
                {
                    if (line.Length > 3)
                        template_file_names.Add(line);
                }
                else if (reading == "position")
                {
                    var sP = line.Split(',');
                    double[] values = sP.Select(x => Convert.ToDouble(x)).ToArray();
                    int nCol = values.Length / 3; //XYZ
                    var values_g = MatrixCalc.CreateJaggedArrayFromLinearArray(values, new int[] { nCol, 3 });
                    PositionList.Add(values_g);
                }
            }

            template_imageYX.Clear();
            template_imageZYX.Clear();

            for (int j = 0; j < template_file_names.Count; j++)
            {
                var file = template_file_names[j];
                if (File.Exists(file))
                {
                    var flim = new FLIMData(flimage.State);
                    var nPages = FileIO.SetupFLIMOpening(file, out string header);
                    for (short page = 0; page < nPages; page++)
                    {
                        FileIO.OpenFLIMTiffFilePage(file, page, page, flim, page == 0, true);
                    }

                    Channel_Pulldown.SelectedIndex = channel_to_analyze;
                    reconstructImageFromPages(flim);
                    template_imageYX.Add(getProjection(imageZYX));

                    var template_image = new ushort[imageZYX.Length][,];
                    for (int z = 0; z < imageZYX.Length; z++)
                        template_image[z] = (ushort[,])imageZYX[z].Clone();

                    template_imageZYX.Add(template_image);
                }
            }

            if (template_imageZYX.Count == template_file_names.Count)
            {
                using_previousSetting = true;
                currentMotor_Pos = 0;
                ReformatPositionGridData();
                ApplyBMP();
                PlotProcedures(true, false);
            }
            else
            {
                Pos_ClearAll_Click(Pos_ClearAll, null);
            }
        }

        public void reconstructImageFromPages(FLIMData flim)
        {
            FLIM_5D = (ushort[][][,,])Copier.DeepCopyArray(flim.FLIM_Pages);
            nSlices = FLIM_5D.Length;

            if (FLIM_5D == null || FLIM_5D[0] == null)
                return;

            imageZYX = new ushort[nSlices][,];
            for (int z = 0; z < nSlices; z++)
                imageZYX[z] = ImageProcessing.GetProjectFromFLIM(FLIM_5D[z][correctionChannel], null);

        }

        public double[][] calculateDriftXYZ()
        {

            if (template_imageZYX == null || template_imageZYX.Count <= currentMotor_Pos)
                return null;

            reconstructImageFromPages(flimage.image_display.FLIM_ImgData);

            current_imageXY = getProjection(imageZYX);
            //current_z_profile = getProjection_z();

            var xy_drift = calculate_XYdrift();
            double[][] xyz_drift = new double[xy_drift.Length][];

            for (int j = 0; j < xy_drift.Length; j++)
            {
                xyz_drift[j] = new double[3];

                if (xy_drift[j] != null)
                {
                    xyz_drift[j][0] = xy_drift[j][0];
                    xyz_drift[j][1] = xy_drift[j][1];
                }
                else
                {
                    xyz_drift[j][0] = double.NaN;
                    xyz_drift[j][1] = double.NaN;
                }

            }

            for (int j = 0; j < xy_drift.Length; j++)
            {
                xyz_drift[j][2] = CalculateZdrift_each(xyz_drift, xy_drift.Length, j);
            }

            return xyz_drift;
        }

        public double CalculateZdrift_each(double[][] xyz_drift, int nSplit, int number)
        {
            double z_drift = 0;
            int z_len = imageZYX.Length;
            if (template_imageZYX == null || template_imageZYX.Count <= currentMotor_Pos)
                return 0;

            if (nSlices >= 3 && imageZYX.Length == template_imageZYX[currentMotor_Pos].Length)
            {

                var shiftImage = new ushort[z_len][,];
                for (int z = 0; z < z_len; z++)
                {
                    var im = MatrixCalc.GetSplitImage(imageZYX[z], nSplit, number);
                    shiftImage[z] = ImageProcessing.ShiftImage(im, -(int)xyz_drift[number][0], -(int)xyz_drift[number][1]);
                }



#if DEBUG
                //var shiftImageProject = getProjection(shiftImage);
                //ImageProcessing.ImShow(shiftImageProject, template_imageYX[currentMotor_Pos]);
#endif

                //calculate mean.
                float mean_shift = 0;
                float mean_temp = 0;
                var tempImage = new ushort[z_len][,];

                for (int z = 0; z < z_len; z++)
                {
                    tempImage[z] = MatrixCalc.GetSplitImage(template_imageZYX[currentMotor_Pos][z], nSplit, number);
                    mean_shift += (float)MatrixCalc.Mean2D(shiftImage[z]);
                    mean_temp += (float)MatrixCalc.Mean2D(tempImage[z]);
                }
                mean_shift = mean_shift / (float)z_len;
                mean_temp = mean_temp / (float)z_len;

                //Subtract mean
                var shiftImage_n = new float[z_len][,];
                var templateImage_n = new float[z_len][,];

                for (int z = 0; z < z_len; z++)
                {
                    var image_temp = MatrixCalc.ConvertToFloatMatrix(shiftImage[z]);
                    shiftImage_n[z] = MatrixCalc.SubtractConstantFromMatrix(image_temp, mean_shift);
                    image_temp = MatrixCalc.ConvertToFloatMatrix(tempImage[z]);
                    templateImage_n[z] = MatrixCalc.SubtractConstantFromMatrix(image_temp, mean_temp);
                }

                //

                int max_shift = (z_len - 1) / 2;
                double[] xCorr = new double[max_shift * 2 + 1];
                for (int z_shift = -max_shift; z_shift <= max_shift; z_shift++)
                {
                    double xcorr = 0;

                    for (int z = 0; z < z_len; z++)
                        if (z - z_shift >= 0 && z - z_shift < z_len)
                            xcorr += MatrixCalc.SIMD_Dot(shiftImage_n[z - z_shift], templateImage_n[z]);

                    xCorr[z_shift + max_shift] = xcorr;
                }

                double[] fit_data = MatrixCalc.FindPeak_WithGaussianFit1D_NoOffset(xCorr);


#if DEBUG
                //double[] x1 = Enumerable.Range(0, xCorr.Length).Select(i => (double)i - max_shift).ToArray();
                //double[] x2 = Enumerable.Range(0, xCorr.Length * 10).Select(i => (double)i / 10.0).ToArray();
                //double[] y2 = MatrixCalc.Gaussian_NoOffset(fit_data, x2);

                //x2 = x2.Select(x => x - max_shift).ToArray(); //after fitting asignment.
                //var plot = ImageProcessing.Plot(x1, xCorr);
                //plot.AddPlot(x2, y2);
#endif

                z_drift = fit_data[1] - max_shift;
                if (z_drift > max_shift * 2)
                    z_drift = max_shift * 2;
                if (z_drift < -max_shift * 2)
                    z_drift = -max_shift * 2;
            }

            xyz_drift[number][2] = z_drift;
            return z_drift;
        }

        public double[] meanMultiplePos(double[][] data_pos)
        {
            double[] result = new double[data_pos[0].Length];
            for (int i = 0; i < result.Length; i++)
               result[i] = data_pos.Select(x => x[i]).ToArray().Average();

            return result;
        }


        public void CalculateDrift(bool move_stage)
        {
            if (template_imageZYX == null || template_imageZYX.Count <= currentMotor_Pos)
                return;

            var State = flimage.image_display.FLIM_ImgData.State;

            var xyz_drift = calculateDriftXYZ();

            int nSplit = xyz_drift.Length;
            double[][] voltage_xyz = new double[nSplit][];
            double[][] xyz_um = new double[nSplit][];

            for (int j = 0; j < nSplit; j++)
            {
                voltage_xyz[j] = new double[3]; //z can be used for piezo, for example.

                double width = (double)current_imageXY.GetLength(1);
                double height = (double)current_imageXY.GetLength(0);
                voltage_xyz[j][0] = -xyz_drift[j][0] / width / State.Acq.zoom * State.Acq.scanVoltageMultiplier[0] * State.Acq.XMaxVoltage;
                voltage_xyz[j][1] = -xyz_drift[j][1] / height / State.Acq.zoom * State.Acq.scanVoltageMultiplier[1] * State.Acq.YMaxVoltage;
                voltage_xyz[j] = MatrixCalc.Rotate(voltage_xyz[j], State.Acq.Rotation);

                double[] xy_um = ImageParameterCalculation.voltage2micrometers_XY(voltage_xyz[j], State);

                xyz_um[j] = new double[3];
                xyz_um[j][0] = xy_um[0];
                xyz_um[j][1] = xy_um[1];
                xyz_um[j][2] = xyz_drift[j][2] * State.Acq.sliceStep;
            }

            double[] shiftMean_um = meanMultiplePos(xyz_um);
            double[] shiftMean_pixel = meanMultiplePos(xyz_drift);
            double[] shiftMean_volt = meanMultiplePos(voltage_xyz);

            Status_XY.BeginInvokeIfRequired(o => o.Text = String.Format("Image shift: {0:0}, {1:0} pixels", shiftMean_pixel[0], shiftMean_pixel[1]));
            Status_V.BeginInvokeIfRequired(o => o.Text = String.Format("Voltage shift: {0:0.000} V, {1:0.000} V, (mean of {2})",
                shiftMean_volt[0], shiftMean_volt[1], nSplit));
            Status_XYZ_um.BeginInvokeIfRequired(o => o.Text = String.Format("Estimated drift: {0:0.00}, {1:0.00}, {2:0.00} um",
                shiftMean_um[0], shiftMean_um[1], shiftMean_um[2]));

            var position_current = GetPosition_Mirror_Motor_Combined(State);

            var position_template = PositionList[currentMotor_Pos];

            double x_shift = position_current[0][0] - position_template[0][0];
            double y_shift = position_current[0][1] - position_template[0][1];
            double z_shift = position_current[0][2] - position_template[0][2];
            Status_Motor.BeginInvokeIfRequired(o => o.Text = String.Format("Motor position shift: {0:0.00}, {1:0.00}, {2:0.00} um",
               x_shift, y_shift, z_shift));

            if (DriftCorrection_CB.Checked && move_stage)
            {
                if (XYCorrect_CB.Checked)
                {
                    for (int j = 0; j < PositionList[currentMotor_Pos].Length; j++) //Shift in the same way.
                    {
                        PositionList[currentMotor_Pos][j][0] = position_template[j][0] - xyz_um[j][0];
                        PositionList[currentMotor_Pos][j][1] = position_template[j][1] - xyz_um[j][1];
                    }

                    if (UseMirror_CB.Checked)
                    {
                        if (numMotorPos <= 1) //if there is noly one position, we move immediately.
                        {
                            if (flimage.State.Acq.nSplitScanning > 1)
                            {
                                for (int j = 0; j < nSplit; j++)
                                {
                                    flimage.State.Acq.XOffset_Split[j] -= voltage_xyz[j][0];
                                    flimage.State.Acq.YOffset_Split[j] -= voltage_xyz[j][1];
                                }
                            }
                            else
                            {
                                flimage.State.Acq.XOffset -= voltage_xyz[0][0];
                                flimage.State.Acq.YOffset -= voltage_xyz[0][1];
                            }
                            flimage.ReSetupValues(false);
                        }
                    }
                    else
                    {

                        if (numMotorPos <= 1) //if there is noly one position, we move immediately.
                        {
                            flimage.motorCtrl.SetNewPosition_StepSize_um(new double[] { -shiftMean_um[0], -shiftMean_um[1], 0 });
                            flimage.SetMotorPosition(true, true, false);
                        }
                    }
                }

                if (ZCorrect_CB.Checked)
                {
                    for (int j = 0; j < nSplit; j++)
                        PositionList[currentMotor_Pos][j][2] = position_template[j][2] - shiftMean_um[2];

                    if (numMotorPos <= 1) //if there is noly one position, we move immediately.
                    {
                        flimage.motorCtrl.SetNewPosition_StepSize_um(new double[] { 0, 0, -shiftMean_um[2] });
                        flimage.SetMotorPosition(true, true, false);
                        if (flimage.motor_back_to_center)
                            flimage.InvokeIfRequired(o => o.SetCenter());
                        else if (flimage.motor_back_to_start)
                            flimage.InvokeIfRequired(o => o.SetTop());
                    }

                }

                SaveTemplateInfo();
                ReformatPositionGridData();
            }

        }

        public void EventHandling(FLIMage_IO fc, ProcessEventArgs e)
        {
            var State = flimage.State;

            String eventStr = e.EventName;
            String eventName = "";
            if (State.Acq.fastZScan)
                eventName = "SaveImageDone";
            else if (State.Acq.ZStack)
                eventName = "AcquisitionDone";
            else
                eventName = "AcquisitionDone";

            if (eventStr == eventName)
            {
                this.InvokeAnyway(o => o.CalculateDrift(true));
            }
        }



        public double[][] calculate_XYdrift()
        {
            var nSplit = PositionList[currentMotor_Pos].Length;
            double[][] xy_drift = new double[nSplit][];

            var temp = MatrixCalc.SplitImage(template_imageYX[currentMotor_Pos], nSplit);
            var cur = MatrixCalc.SplitImage(current_imageXY, nSplit);

            for (int j = 0; j < nSplit; j++)
            {
                double corVal = ImageProcessing.MatrixMeasureDrift2D_FFT(temp[j], cur[j], out xy_drift[j]);
            }

            return xy_drift;
        }

        public UInt16[,] getProjection(ushort[][,] image_3d)
        {
            int y_length = image_3d[0].GetLength(0);
            int x_length = image_3d[0].GetLength(1);

            UInt16[,] data_projection = new ushort[y_length, x_length];

            for (int z = 0; z < image_3d.Length; z++)
                MatrixCalc.MatrixCalc2D(data_projection, image_3d[z], CalculationType.Max, true);

            return data_projection;
        }

        private void setCurrentImageForTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file_name = flimage.image_display.FLIM_ImgData.fullFileName;
            if (!File.Exists(file_name))
            {
                file_name = flimage.image_display.lastFilePath;
            }

            if (File.Exists(file_name))
            {
                using_previousSetting = false;
                flimage.image_display.OpenFLIM(file_name, true, flimage.image_display.plot_regular.calc_upon_open, false);
                SelectImage(flimage.image_display.FLIM_ImgData);
                PlotProcedures(true, true);
            }
            else
            {
                MessageBox.Show("File not found");
            }
        }

        private void RecalculateDrift_Click(object sender, EventArgs e)
        {
            CalculateDrift(false);
        }

        private void Pos_ClearAll_Click(object sender, EventArgs e)
        {
            PositionDataGridView.Rows.Clear();
            template_imageYX.Clear();
            template_imageZYX.Clear();
            PositionList.Clear();
            numMotorPos = 0;
            PlotProcedures(true, true);
        }

        private void AddPosition()
        {
            PositionDataGridView.Rows.Add();
            template_imageYX.Add(null);
            template_imageZYX.Add(null);
            PositionList.Add(null);
            template_file_names.Add("");
            numMotorPos = PositionList.Count;
            //This is a part of "Select image".
        }

        private void ReformatPositionGridData()
        {
            int curPos = currentMotor_Pos; //when reconstructing, cell selection changes.
            if (PositionDataGridView.RowCount < PositionList.Count)
            {
                for (int i = PositionDataGridView.RowCount; i < PositionList.Count; i++)
                    PositionDataGridView.Rows.Add();
            }
            else if (PositionList.Count < PositionDataGridView.RowCount)
            {
                int diff = PositionDataGridView.RowCount - PositionList.Count;
                for (int i = 0; i < diff; i++)
                    PositionDataGridView.Rows.RemoveAt(PositionDataGridView.RowCount - 1);
            }

            for (int i = 0; i < PositionList.Count; i++)
            {
                object[] values = new object[] { (i + 1), PositionList[i][0][0].ToString("0.0"),
                    PositionList[i][0][1].ToString("0.0"), PositionList[i][0][2].ToString("0.0") };
                PositionDataGridView.Rows[i].SetValues(values);
            }

            currentMotor_Pos = curPos;
            if (currentMotor_Pos < 0)
                currentMotor_Pos = 0;
            else if (currentMotor_Pos >= PositionList.Count)
                currentMotor_Pos = PositionList.Count - 1;

            if (PositionDataGridView.Rows.Count > 0)
                PositionDataGridView.CurrentCell = PositionDataGridView[0, currentMotor_Pos];
        }

        private void RemovePosition(int remPos)
        {
            template_imageYX.RemoveAt(remPos);
            template_imageZYX.RemoveAt(remPos);
            PositionList.RemoveAt(remPos);
            currentMotor_Pos = remPos - 1;

            ReformatPositionGridData();
            PlotProcedures(true, true);
        }

        private void Pos_Delete_Click(object sender, EventArgs e)
        {
            RemovePosition(currentMotor_Pos);
        }

        private void GoToPosition(int position_num, bool warning_on)
        {
            if (position_num < numMotorPos)
            {
                var State = flimage.State;

                var pos = new double[PositionList[position_num].Length][];
                for (int j = 0; j < pos.Length; j++)
                    pos[j] = (double[])PositionList[position_num][j].Clone();

                flimage.MotorPositioningUpdate(); //back to center etc.
                //We need to set the position to the center, if the motor is assumed to be back to the center.
                if (flimage.motor_back_to_center)
                    for (int j = 0; j < pos.Length; j++)
                        pos[j][2] += State.Acq.sliceStep * (State.Acq.nSlices + 1.0) / 2.0;

                double[] newMotorPos = new double[3];

                if (UseMirror_CB.Checked)
                {
                    double[] curPosition = flimage.motorCtrl.getCalibratedAbsolutePosition();
                    double[] shiftXY_um;
                    double[] voltage;
                    if (State.Acq.nSplitScanning > 1)
                    {
                        State.Acq.XOffset_Split = new double[pos.Length];
                        State.Acq.YOffset_Split = new double[pos.Length];
                        for (int j = 0; j < pos.Length; j++)
                        {
                            shiftXY_um = new double[] { pos[j][0] - curPosition[0], pos[j][1] - curPosition[1] };
                            voltage = ImageParameterCalculation.micrometers2voltage_XY(shiftXY_um, State);
                            State.Acq.XOffset_Split[j] = voltage[0];
                            State.Acq.YOffset_Split[j] = voltage[1];
                        }
                    }
                    else
                    {
                        shiftXY_um = new double[] { pos[0][0] - curPosition[0], pos[0][1] - curPosition[1] };
                        voltage = ImageParameterCalculation.micrometers2voltage_XY(shiftXY_um, State);
                        State.Acq.XOffset = voltage[0];
                        State.Acq.YOffset = voltage[1];
                    }

                    newMotorPos[0] = curPosition[0];
                    newMotorPos[1] = curPosition[1];
                    newMotorPos[2] = pos[0][2];
                }
                else
                {
                    //Calculate motor position with position 0. 
                    newMotorPos = (double[])pos[0].Clone();
                    double[] voltage;
                    if (State.Acq.nSplitScanning > 1)
                        voltage = new double[] { State.Acq.XOffset_Split[0], State.Acq.YOffset_Split[0] };
                    else
                        voltage = new double[] { State.Acq.XOffset, State.Acq.YOffset };

                    double[] xy_um = ImageParameterCalculation.voltage2micrometers_XY(voltage, State);
                    newMotorPos[0] -= xy_um[0];
                    newMotorPos[1] -= xy_um[1];
                }

                flimage.ReSetupValues(false);
                flimage.MoveMotorPosition(newMotorPos, warning_on);

                if (flimage.motor_back_to_center)
                    flimage.InvokeIfRequired(o => o.SetCenter());
                else if (flimage.motor_back_to_start)
                    flimage.InvokeIfRequired(o => o.SetTop());
            }
        }

        private void Pos_Goto_Click(object sender, EventArgs e)
        {
            GoToPosition(currentMotor_Pos, true);
        }

        public class ImSeqUnit
        {
            public double time_start = 0;
            public double time_end = 0;
            public int proc_no = 0;
            public int position = 0;
            public bool exclusive = false;
            public int repeat = 0;
        }

        public double FurtherDelay(List<ImSeqUnit> unit_list, int currentPos, double currentTime)
        {
            double curTime = currentTime;
            bool in_sequence = false;
            foreach (var seqU in unit_list)
            {
                if (seqU.exclusive && seqU.position != currentPos)
                {
                    var exclusiveArea = Math.Max(procedureList[seqU.proc_no].Interval_ms, procedureList[seqU.proc_no].estimated_time_ms);
                    var seqU_endTime = seqU.time_start + exclusiveArea;
                    bool cur_sequence = (curTime >= seqU.time_start && curTime < seqU_endTime);
                    if (cur_sequence)
                        curTime = seqU_endTime;

                    if (in_sequence && !cur_sequence)
                        break;

                    in_sequence = cur_sequence;
                }
            }

            return curTime - currentTime;
        }

        public List<ImSeqUnit> ProduceSequence()
        {
            im_seq_unit_list = new List<ImSeqUnit>();
            double delay_minutes = 2;

            Double.TryParse(TimeShift_textBox.Text, out delay_minutes);

            double delay_ms = delay_minutes * 60.0 * 1000.0;
            double timeStart = 0;
            double[] pos_delay_byExclusive = new double[numMotorPos];

            if (procedureList.Count == 0)
                return im_seq_unit_list;

            double estimatedTime_max = procedureList.Select(x => x.Exclusive ? 0.0 : x.estimated_time_ms).ToArray().Max();

            int totalRepeat = procedureList.Select(x => x.Repetition).ToArray().Sum();
            int maxPos = 1;
            if (numMotorPos > 1)
                maxPos = PositionList.Count;
            bool[,] doneTable = new bool[totalRepeat, maxPos];
            bool[] firstExclusive = new bool[maxPos];

            int totalRep = 0;
            for (int i = 0; i < procedureList.Count; i++)
            {
                bool exclusive = procedureList[i].Exclusive;

                if (firstExclusive.All(x => x == true) && !exclusive)
                {
                    firstExclusive = new bool[firstExclusive.Length];
                }

                for (int rep = 0; rep < procedureList[i].Repetition; rep++)
                {

                    for (int pos = 0; pos < maxPos; pos++)
                    {
                        if (!doneTable[totalRep, pos])
                        {

                            double pos_delay = pos * (delay_ms + estimatedTime_max);
                            //
                            double t_start = timeStart + pos_delay;
                            if (numMotorPos > 1)
                                t_start += pos_delay_byExclusive[pos];
                            //
                            //Calculate delay by exclusive procedure in other position.


                            //
                            if (!firstExclusive[pos] && numMotorPos > 1)
                            {
                                firstExclusive[pos] = true;
                                //if current one is excluxive, we will find all consecutive ones.
                                int rep2 = totalRep;
                                double timeStart2 = timeStart;
                                for (int j = i; j < procedureList.Count; j++)
                                {
                                    for (int r = 0; r < procedureList[j].Repetition; r++)
                                    {
                                        var seqUnit = new ImSeqUnit();

                                        t_start = timeStart2 + pos_delay + pos_delay_byExclusive[pos];
                                        pos_delay_byExclusive[pos] += FurtherDelay(im_seq_unit_list, pos, t_start);
                                        t_start = timeStart2 + pos_delay + pos_delay_byExclusive[pos];

                                        seqUnit.time_start = t_start;
                                        seqUnit.time_end = t_start + procedureList[j].estimated_time_ms;
                                        seqUnit.proc_no = j;
                                        seqUnit.position = pos;
                                        seqUnit.exclusive = procedureList[j].Exclusive;
                                        seqUnit.repeat = r;

                                        im_seq_unit_list.Add(seqUnit);
                                        doneTable[rep2, pos] = true;
                                        var length = procedureList[j].Interval_ms;
                                        timeStart2 += length;

                                        rep2++;
                                    }

                                    if (j == procedureList.Count - 1 || (procedureList[j].Exclusive && !procedureList[j + 1].Exclusive))
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (numMotorPos > 1)
                                    pos_delay_byExclusive[pos] += FurtherDelay(im_seq_unit_list, pos, t_start);

                                //You have recalculate after pos_delay_byExclusive is calclated.
                                t_start = timeStart + pos_delay;
                                if (numMotorPos > 1)
                                    t_start += pos_delay_byExclusive[pos];

                                var seqUnit = new ImSeqUnit();
                                seqUnit.time_start = t_start;
                                seqUnit.time_end = t_start + procedureList[i].estimated_time_ms;
                                seqUnit.proc_no = i;
                                seqUnit.position = pos;
                                seqUnit.exclusive = procedureList[i].Exclusive;
                                seqUnit.repeat = rep;

                                im_seq_unit_list.Add(seqUnit);
                                doneTable[totalRep, pos] = true;
                            }

                        } //if table.
                    } //pos

                    timeStart += procedureList[i].Interval_ms;
                    totalRep++;
                } //rep
            } //proc

            im_seq_unit_list = im_seq_unit_list.OrderBy(x => x.time_start).ToList();

            return im_seq_unit_list;
        }

        private void PlotProcedures(bool recalc, bool updateSelectedRow)
        {
            if (recalc)
                ProduceSequence();
            plot.ClearData();
            int maxPos = (numMotorPos == 0) ? 1 : numMotorPos;

            if (im_seq_unit_list.Count == 0)
                return;

            string[] plot_types = new string[] { "-g", "-b", "-m", "-c", "-k", "-n", "-p" };

            for (int i = 0; i < im_seq_unit_list.Count; i++)
            {
                var unit = im_seq_unit_list[i];

                float lineWidth = 1f;
                string plottype = plot_types[unit.proc_no % plot_types.Length];


                double pos1 = maxPos - unit.position;
                double end_time = unit.time_start + procedureList[unit.proc_no].Interval_ms;
                var boxHeight = 0.8f;
                var xdata = new double[] { unit.time_start, end_time, end_time, unit.time_start, unit.time_start };
                xdata = xdata.Select(x => x / 1000.0 / 60.0).ToArray();
                var ydata = new double[] { pos1 - boxHeight / 2, pos1 - boxHeight / 2, pos1 + boxHeight / 2, pos1 + boxHeight / 2, pos1 - boxHeight / 2 };
                plot.AddData(xdata, ydata, plottype, lineWidth);

                if (i == current_sequence_pos)
                {
                    plot.AddData(xdata, ydata, "-r", 5);
                }
                //plot.YTitle = "Position";
            }

            plot.XTitle = "Time (min)";
            plot.SetAxisRange(-1, maxPos + 2, 1);
            plot.axis.noTickLabelY = true;
            plot.axis.noTickY = true;
            plot.autoAxisPosition = false;
            plot.axis.yMergin = 0.15f;
            plot.axis.xMergin = 0.025f;
            plot.axis.fill = true;
            plot.UpdatePlot();

            if (current_sequence_pos >= im_seq_unit_list.Count && current_sequence_pos < 0)
                current_sequence_pos = 0;

            NPosition_Label.Text = String.Format("{0}/{1}", im_seq_unit_list[current_sequence_pos].position, maxPos);
            currentMotor_Pos = im_seq_unit_list[current_sequence_pos].position;
            var proc_num = im_seq_unit_list[current_sequence_pos].proc_no;
            RepNumberLabel.Text = String.Format("{0}/{1}", im_seq_unit_list[current_sequence_pos].repeat, procedureList[proc_num].Repetition);

            if (updateSelectedRow)
            {
                procedure_selectedRawIndex = proc_num;
                ImageSequenceGridView.CurrentCell = ImageSequenceGridView[0, proc_num];
            }
        }

        private void PlotBox_Paint(object sender, PaintEventArgs e)
        {
        }

        public void TemplateImage_PB_Paint(object sender, PaintEventArgs e)
        {
            if (templateBMP == null)
                return;

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(templateBMP,
                new Rectangle(0, 0, TemplateImage_PB.Width, TemplateImage_PB.Height), // destination rectangle 
                0, 0,           // upper-left corner of source rectangle
                templateBMP.Width,       // width of source rectangle
                templateBMP.Height,      // height of source rectangle
                GraphicsUnit.Pixel);
        }

        class ImageSeqProcedure
        {
            public ProcedureType procedure_type;
            public ScanParameters State;
            public double Interval_ms;
            public double estimated_time_ms;
            public bool Exclusive;
            public int Repetition;
            public int ID;

            public ImageSeqProcedure(string proc_string, int repetition, double interval_milliseconds, bool exclusive,
                int id, ScanParameters state)
            {
                Interval_ms = interval_milliseconds;
                var fileIO = new FileIO(state);
                State = fileIO.CopyState();
                Exclusive = exclusive;
                Repetition = repetition;
                ID = id;

                bool imaging = proc_string.Contains("Imaging");
                bool imaging_only = proc_string == "Imaging";
                bool uncaging_only = proc_string == "Uncaging";
                bool uncaging = proc_string.Contains("Uncaging");
                bool DO_only = proc_string == "DO";
                bool DO = proc_string.Contains("DO");
                bool ephys_only = proc_string == "Ephys";
                bool ephys = proc_string.Contains("Ephys");

                double im_length = estimated_time_ms = State.Acq.msPerLine * State.Acq.linesPerFrame * State.Acq.nSlices;
                double uncage_length = State.Uncaging.sampleLength;
                double DO_Length = State.DO.sampleLength;
                double ephys_length = State.Ephys.pulseSetTotalLength_ms;

                if (imaging_only)
                {
                    procedure_type = ProcedureType.Imaging;
                    estimated_time_ms = im_length;
                }
                else if (imaging && uncaging)
                {
                    procedure_type = ProcedureType.Imaging_Uncaging;
                    estimated_time_ms = im_length;
                }
                else if (imaging && ephys)
                {
                    procedure_type = ProcedureType.Imaging_Ephys;
                    estimated_time_ms = Math.Max(im_length, ephys_length);
                }
                else if (imaging && DO)
                {
                    procedure_type = ProcedureType.Imaging_DO;
                    estimated_time_ms = im_length;
                }
                else if (uncaging_only)
                {
                    procedure_type = ProcedureType.Uncaging;
                    estimated_time_ms = uncage_length;
                }
                else if (uncaging && ephys)
                {
                    procedure_type = ProcedureType.Uncaging_Ephys;
                    estimated_time_ms = Math.Max(ephys_length, uncage_length);
                }
                else if (uncaging && ephys && imaging)
                {
                    procedure_type = ProcedureType.Imaging_Uncaging_Ephys;
                    estimated_time_ms = Math.Max(im_length, ephys_length);
                }
                else
                {
                    procedure_type = ProcedureType.Imaging;
                    estimated_time_ms = im_length;
                }
            }
        }

        private void ForwardButton_Click(object sender, EventArgs e)
        {
            current_sequence_pos++;
            if (current_sequence_pos > im_seq_unit_list.Count - 1)
                current_sequence_pos = 0;

            reportProgress();
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            current_sequence_pos--;
            if (current_sequence_pos < 0)
                current_sequence_pos = im_seq_unit_list.Count - 1;

            reportProgress();
        }

        private void Starggering_textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                PlotProcedures(true, true);

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void AcquireTemplateImageButton_Click(object sender, EventArgs e)
        {
            using_previousSetting = false;

            Task.Factory.StartNew((Action)delegate
            {
                if (double.TryParse(ZoomTextBox.Text, out double zoom))
                    flimage.State.Acq.zoom = zoom;
                flimage.ReSetupValues(false);
                this.InvokeAnyway(o => o.DriftCorrection_CB.Checked = false);

                ImageOnce();

                System.Threading.Thread.Sleep(CheckingInterval_ms);
                while (flimage.flimage_io.grabbing || flimage.flimage_io.post_grabbing_process)
                {
                    System.Threading.Thread.Sleep(CheckingInterval_ms);
                }

                this.InvokeAnyway(o =>
                {
                    o.SelectImage(flimage.image_display.FLIM_ImgData);
                    o.PlotProcedures(true, true);
                });
            });
        }


        private void PositionDataGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            if (PositionDataGridView.CurrentCell != null)
            {
                currentMotor_Pos = PositionDataGridView.CurrentCell.RowIndex;
                PositionDataGridView.Rows[currentMotor_Pos].Selected = true;

                ApplyBMP();
            }

        }

        private void FillGUI_State(int selectedIndex)
        {
            if (procedureList.Count > selectedIndex && selectedIndex >= 0)
            {
                var state = procedureList[selectedIndex].State;
                nFrames_textBox.Text = state.Acq.nFrames.ToString();
                nSlices_textBox.Text = state.Acq.nSlices.ToString();
                nAveFrame_textBox.Text = state.Acq.nAveFrame.ToString();
                ZStack_checkBox.Checked = state.Acq.ZStack;
                aveFrame_checkBox.Checked = state.Acq.aveFrame;
                setting_groupBox.Text = "Setting parameters ID " + procedureList[selectedIndex].ID;
            }
        }

        private void generic_change()
        {
            if (procedureList.Count > procedure_selectedRawIndex && procedure_selectedRawIndex >= 0)
            {
                var state = procedureList[procedure_selectedRawIndex].State;
                Int32.TryParse(nFrames_textBox.Text, out state.Acq.nFrames);
                Int32.TryParse(nSlices_textBox.Text, out state.Acq.nSlices);
                Double.TryParse(sliceStep_textBox.Text, out state.Acq.sliceStep);
                Int32.TryParse(nAveFrame_textBox.Text, out state.Acq.nAveFrame);
                state.Acq.ZStack = ZStack_checkBox.Checked;
                state.Acq.aveFrame = aveFrame_checkBox.Checked;
                EditProc(state, procedure_selectedRawIndex);
            }

        }

        private void Generic_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                generic_change();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void Generic_checkBoxClick(object sender, EventArgs e)
        {
            generic_change();
        }

        private void Channel_Pulldown_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void ReloadTemplate_Click(object sender, EventArgs e)
        {
            LoadTemplateInfo();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String filename = "";
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.InitialDirectory = Path.GetDirectoryName(SettingFolderPath);
            saveFileDialog1.FileName = "ImageSeqFile.zip";
            saveFileDialog1.Filter = String.Format("Image Sequence files (*{0})|*{0}|All files (*.*)|*.*", ".zip");
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = false;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    filename = saveFileDialog1.FileName;

                    if (File.Exists(filename))
                        File.Delete(filename);

                    using (ZipArchive archive = ZipFile.Open(filename, ZipArchiveMode.Create))
                    {
                        var files = FileHandling.GetFiles(SettingFolderPath, new string[] { "*.txt", "*.seq" });
                        foreach (var file in files)
                        {
                            var entryName = Path.GetFileName(file);                            
                            archive.CreateEntryFromFile(file, entryName);
                        }
                    }

                    //ZipFile.CreateFromDirectory(SettingFolderPath, filename);

                    //State.Files.initFileName = filename;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file to disk. Original error: " + ex.Message);
                }
            }

                     

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = Path.GetDirectoryName(SettingFolderPath);
            openFileDialog1.FileName = "ImageSeqFile.zip";
            openFileDialog1.Filter = "ini files (*.zip)|*.zip|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = false;

            String filename = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (Directory.Exists(SettingFolderPath))
                    {
                        var files = FileHandling.GetFiles(SettingFolderPath, new string[] {"*.txt", "*.seq" });
                        foreach (var file in files)
                            File.Delete(file);
                    }
                    ZipFile.ExtractToDirectory(openFileDialog1.FileName, SettingFolderPath);
                    LoadImageSeqFile();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        enum ProcedureType
        {
            Imaging,
            Uncaging,
            Ephys,
            DO,
            Imaging_Uncaging,
            Imaging_Uncaging_Ephys,
            Imaging_Ephys,
            Imaging_DO,
            Uncaging_Ephys,
        }
    }
}
