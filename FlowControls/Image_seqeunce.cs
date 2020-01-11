using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace FLIMage.FlowControls
{
    public partial class Image_seqeunce : Form
    {
        FLIMageMain FLIMage;
        public ScanParameters State;
        int settingIDLargest = 0;

        WindowLocManager winManager;
        String WindowName = "ImageSequence.loc";
        String SettingFolderName = "ImageSequence";
        String SettingFolderPath;
        String SettingNameHeader = "ImageSeqSetting-";
        String ImageSeqFileName = "ImageSeq.seq";
        String ImageSeqFilePath;
        Char Separator = '\t';


        Stopwatch timer_rep = new Stopwatch();
        Stopwatch total_time = new Stopwatch();
        int current_repeat = 0;
        int current_row = 0;

        bool running_current = false;
        bool image_seq_running = false;
        bool abortGrabActivated = false;
        bool wait_current_image_finish = false;
        bool pause = false;
        int CheckingInterval_ms = 100;
        bool autoDriftCorrection = false;

        int selectedRowIndex = 0;
        int startRow = 0;
        int startPos = 0;

        public Image_seqeunce(FLIMageMain FLIMage_in)
        {
            InitializeComponent();
            DataGridViewComboBoxColumn procColumn = new DataGridViewComboBoxColumn();
            procColumn.Name = "Procedure";
            procColumn.HeaderText = "Procedure";
            procColumn.Items.Add("Imaging");
            procColumn.Items.Add("Imaging + Uncaging");
            procColumn.Items.Add("Imaging + DO");
            procColumn.Items.Add("Uncaging");
            procColumn.Items.Add("DO");
            procColumn.Width = 130;
            ImageSequenceGridView.Columns.Add(procColumn);
            ImageSequenceGridView.CurrentCellDirtyStateChanged += new EventHandler(dataGridView1_CellValueChanged);

            FLIMage = FLIMage_in;
            State = FLIMage.State;
            //ImageSequenceGridView.AllowUserToAddRows = false;
            ImageSequenceGridView.Columns["SettingID"].ReadOnly = true;
            SettingFolderPath = Path.Combine(State.Files.initFolderPath, SettingFolderName);
            Directory.CreateDirectory(SettingFolderPath);

            ImageSeqFilePath = Path.Combine(SettingFolderPath, ImageSeqFileName);

            string[] fileArray = Directory.GetFiles(@SettingFolderPath, SettingNameHeader + "*.txt");
            settingIDLargest = 0;
            LoadImageSeqFile();
        }

        public void Image_seqeunce_Load(object sender, EventArgs e)
        {
            winManager = new WindowLocManager(this, WindowName, State.Files.windowsInfoPath);
            winManager.LoadWindowLocation(false);
            PauseButton.Visible = false;
        }

        public void SaveWindowLocation()
        {
            winManager.SaveWindowLocation();
        }

        public void Image_seqeunce_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveTable();
            SaveWindowLocation();

            e.Cancel = true;
            this.Hide();

            FLIMage.ToolWindowClosed();
        }

        public void AddCurrentSetting_Click(object sender, EventArgs e)
        {
            State = FLIMage.State;
            SetLargestID();
            settingIDLargest += 1;
            String fn = Path.GetFileNameWithoutExtension(State.Files.initFileName);
            ImageSequenceGridView.Rows.Add(settingIDLargest, fn, State.Acq.imageInterval, State.Acq.nImages, State.Acq.zoom, "Imaging");
            FileIO fo = new FileIO(State);
            String str1 = fo.AllSetupValues_nonDevice();
            str1 = fn + ";" + "\r\n" + str1;
            File.WriteAllText(StateFileName(settingIDLargest), str1);

            SaveTable();

            int nRows = ImageSequenceGridView.Rows.Count;
            ImageSequenceGridView.CurrentCell = ImageSequenceGridView[0, nRows - 1];
            ImageSequenceGridView.Rows[nRows - 1].Selected = true;
        }

        public void ReplaceWithCurrent_Click(object sender, EventArgs e)
        {
            int num = Convert.ToInt32(ImageSequenceGridView.Rows[selectedRowIndex].Cells["SettingID"].Value);

            State = FLIMage.State;
            String fn = Path.GetFileNameWithoutExtension(State.Files.initFileName);
            FileIO fo = new FileIO(State);
            String str1 = fo.AllSetupValues_nonDevice();
            str1 = fn + ";" + "\r\n" + str1;
            File.WriteAllText(StateFileName(num), str1);
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
                    if (File.Exists(fileName))
                    {
                        Array.Resize(ref sP, ImageSequenceGridView.ColumnCount);
                        if (sP[sP.Length - 1] == null || (sP[sP.Length - 1].Contains("Uncaging") && sP[sP.Length - 1].Contains("DO")))
                            sP[sP.Length - 1] = "Imaging";

                        ImageSequenceGridView.Rows.Add(sP);
                        if (fileNum > settingIDLargest)
                            settingIDLargest = fileNum;
                    }
                }
            }
        }

        public void SettingID_UpDown(object sender, EventArgs e)
        {
            int selected = ImageSequenceGridView.CurrentCell.RowIndex;
            var row = ImageSequenceGridView.Rows[selected];

            if (sender.Equals(RowUp) && selected > 0)
            {
                ImageSequenceGridView.Rows.RemoveAt(selected);
                selected--;
                ImageSequenceGridView.Rows.Insert(selected, row);
            }

            else if (sender.Equals(RowDown) && selected < ImageSequenceGridView.Rows.Count - 1)
            {
                ImageSequenceGridView.Rows.RemoveAt(selected);
                selected += 1;
                ImageSequenceGridView.Rows.Insert(selected, row);
            }

            foreach (DataGridViewRow row1 in ImageSequenceGridView.Rows)
                row1.Selected = false;

            ImageSequenceGridView.CurrentCell = ImageSequenceGridView[0, selected];
            ImageSequenceGridView.Rows[selected].Selected = true;
        }

        public void DeleteRow_Click(object sender, EventArgs e)
        {
            int selected = ImageSequenceGridView.CurrentCell.RowIndex;
            int num = Convert.ToInt32(ImageSequenceGridView.Rows[selected].Cells["SettingID"].Value);
            ImageSequenceGridView.Rows.RemoveAt(selected);
            String fn = StateFileName(num);
            if (File.Exists(fn))
                File.Delete(fn);
            SetLargestID();
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
        }

        public void RunSeq_Click(object sender, EventArgs e)
        {
            RunSeq.Enabled = false;
            State = FLIMage.State;
            running_current = false;
            pause = false;
            wait_current_image_finish = false;

            startRow = 0;
            startPos = 0;
            if (StartFromCurrentCheck.Checked)
            {
                startRow = selectedRowIndex;

                int startPos_tmp = 0;
                Int32.TryParse(ProgressEdit.Text, out startPos_tmp);
                startPos_tmp = startPos_tmp - 1;
                int repetition = Convert.ToInt32(ImageSequenceGridView.Rows[selectedRowIndex].Cells["Repetition"].Value);
                
                if (startPos_tmp < 0)
                    startPos_tmp = 0;
                if (startPos_tmp >= repetition)
                    startPos_tmp = repetition - 1;

                startPos = startPos_tmp;
            }

                if (RunSeq.Text == "Abort")
            {
                RunSeq.Text = "Run sequence";
                abortGrabActivated = true;
                while (image_seq_running)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(CheckingInterval_ms);
                }
                PauseButton.Visible = false;
            }
            else
            {
                abortGrabActivated = false;
                RunSeq.Text = "Abort";
                PauseButton.Visible = true;
                PauseButton.Text = "Pause";
                Task.Factory.StartNew((Action)delegate
                {
                    RunSequence();
                    Invoke((Action)delegate
                    {
                        RunSeq.Text = "Run sequence";
                    });
                });
            }
            RunSeq.Enabled = true;
        }


        public void reportProgress(bool timeonly)
        {
            this.Invoke((Action)delegate
            {
                if (!timeonly)
                {
                    ImageSequenceGridView.CurrentCell = ImageSequenceGridView[0, current_row];
                    ImageSequenceGridView.Rows[current_row].Selected = true;
                }

                int repetition = Convert.ToInt32(ImageSequenceGridView.Rows[current_row].Cells["Repetition"].Value);
                RepNumber.Text = "/" + repetition.ToString();
                ProgressEdit.Text = (current_repeat + 1).ToString();
                RepTime.Text = String.Format("{0:0.0}", timer_rep.ElapsedMilliseconds / 1000.0);
                totalTimeLabel.Text = String.Format("{0:0.0}", total_time.ElapsedMilliseconds / 1000.0);
                
                if (wait_current_image_finish && !running_current)
                {
                    PauseButton.Text = "Restart";
                    PauseButton.Font = new Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    wait_current_image_finish = false;
                }
            });
        }

        public void LoadSetting(String fileName)
        {
            FLIMage.BeginInvoke((Action)delegate
            {
                FLIMage.LoadSettingFile(fileName, false);
            });
        }

        public void AnyUncagingOrDO(out bool anyDO, out bool anyUncaging)
        {
            anyUncaging = false;
            anyDO = false;
            for (int i = 0; i < ImageSequenceGridView.Rows.Count; i++)
            {
                bool imaging = ((String)(ImageSequenceGridView.Rows[i].Cells["Procedure"].Value)).Contains("Imaging");
                bool uncaging = ((String)(ImageSequenceGridView.Rows[i].Cells["Procedure"].Value)).Contains("Uncaging");
                bool DO = ((String)(ImageSequenceGridView.Rows[i].Cells["Procedure"].Value)).Contains("DO");
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
            }
        }

        public void RunSequence()
        {
            //Problem check.
            AnyUncagingOrDO(out bool anyDO, out bool anyUncaging);
            if (anyUncaging && FLIMage.uncaging_panel == null)
            {
                MessageBox.Show("Please open uncaging panel!!");
                return;
            }

            if (anyDO && FLIMage.digital_panel == null)
            {
                MessageBox.Show("Please open DO panel!!");
                return;
            }

            //Start Running.
            image_seq_running = true;
            FLIMage.flimage_io.imageSequencing = true;
            bool firstImage = false;
            if (autoDriftCorrection)
                FLIMage.drift_correction.TurnOnOffCorrection(false);

            total_time.Restart();
            current_row = 0;


            for (int loop = 0; loop < (LoopCheck.Checked ? 1000 : 1); loop++)
            {
                if (loop > 0)
                {
                    startPos = 0;
                    startRow = 0;
                }

                for (int i = startRow; i < ImageSequenceGridView.Rows.Count; i++)
                {
                    current_row = i;
                    current_repeat = startRow;

                    //int num = Convert.ToInt32(ImageSequenceGridView.Rows[i].Cells["SettingID"].Value);
                    //LoadSetting_Number(num);                

                    int interval_ms = (int)(1000.0 * Convert.ToDouble(ImageSequenceGridView.Rows[i].Cells["Interval"].Value));
                    int repetition = Convert.ToInt32(ImageSequenceGridView.Rows[i].Cells["Repetition"].Value);

                    reportProgress(false); //Select current row.
                    LoadSelectedSetting();

                    bool imaging = ((String)(ImageSequenceGridView.Rows[i].Cells["Procedure"].Value)).Contains("Imaging");
                    bool uncaging = (String)(ImageSequenceGridView.Rows[i].Cells["Procedure"].Value) == "Uncaging";
                    bool DO = (String)(ImageSequenceGridView.Rows[i].Cells["Procedure"].Value) == "DO";

                    if (imaging)
                    {
                        bool image_uncage = ((String)(ImageSequenceGridView.Rows[i].Cells["Procedure"].Value)).Contains("Uncaging");
                        bool image_DO = ((String)(ImageSequenceGridView.Rows[i].Cells["Procedure"].Value)).Contains("DO");
                        if (image_uncage)
                            State.Uncaging.uncage_whileImage = true;
                        else
                            State.Uncaging.uncage_whileImage = false;

                        if (image_DO)
                            State.DO.DO_whileImage = true;
                        else
                            State.DO.DO_whileImage = false;


                        FLIMage.ExternalCommand("UpdateGUI");
                    }

                    for (int rep = startPos; rep < repetition; rep++)
                    {
                        firstImage = rep == 0 && i == 0;
                        current_repeat = rep;

                        timer_rep.Restart();

                        if (imaging)
                            ImageOnce();
                        else if (uncaging)
                            UncageOnce();
                        else
                            DO_Once();

                        System.Threading.Thread.Sleep(CheckingInterval_ms);

                        running_current = true;

                        while (running_current)
                        {
                            if (imaging)
                                running_current = (FLIMage.flimage_io.grabbing || FLIMage.flimage_io.post_grabbing_process);
                            else if (uncaging)
                                running_current = FLIMage.uncaging_panel.uncaging_running;
                            else if (DO)
                                running_current = FLIMage.digital_panel.digital_running;

                            if (abortGrabActivated || !running_current)
                                break;

                            System.Threading.Thread.Sleep(CheckingInterval_ms);
                            reportProgress(true);
                        }

                        if (autoDriftCorrection && firstImage && !abortGrabActivated)
                        {
                            FLIMage.drift_correction.SelectImage();
                            FLIMage.drift_correction.TurnOnOffCorrection(true);
                        }

                        while (interval_ms - (int)timer_rep.ElapsedMilliseconds > CheckingInterval_ms)
                        {
                            if (abortGrabActivated)
                                break;
                            System.Threading.Thread.Sleep(CheckingInterval_ms);
                            reportProgress(true);
                        }

                        if ((int)timer_rep.ElapsedMilliseconds < interval_ms)
                        {
                            if (abortGrabActivated)
                                break;
                            System.Threading.Thread.Sleep(interval_ms - (int)timer_rep.ElapsedMilliseconds);
                            reportProgress(true);
                        }

                        if (abortGrabActivated)
                            break;
                    }

                    if (abortGrabActivated)
                        break;
                }

                if (abortGrabActivated)
                    break;
            }

            if (FLIMage.flimage_io.grabbing || FLIMage.flimage_io.post_grabbing_process)
            {
                FLIMage.ExternalCommand("AbortGrab");
                while (FLIMage.flimage_io.grabbing || FLIMage.flimage_io.post_grabbing_process)
                    System.Threading.Thread.Sleep(CheckingInterval_ms);
            }

            if (FLIMage.uncaging_panel != null && FLIMage.uncaging_panel.uncaging_running)
            {
                FLIMage.ExternalCommand("StopUncaging");
                while (FLIMage.uncaging_panel.uncaging_running)
                    System.Threading.Thread.Sleep(CheckingInterval_ms);
            }

            if (FLIMage.digital_panel != null && FLIMage.digital_panel.digital_running)
            {
                FLIMage.ExternalCommand("StopDO");
                while (FLIMage.digital_panel.digital_running)
                    System.Threading.Thread.Sleep(CheckingInterval_ms);
            }

            FLIMage.flimage_io.imageSequencing = false;
            FLIMage.Invoke((Action)delegate
            {
                FLIMage.ChangeItemsStatus(true, false);
            });

            image_seq_running = false;
            running_current = false;
        }

        public void ImageOnce()
        {
            FLIMage.ExternalCommand("StartGrab");
            Debug.WriteLine("Start Grab started : " + FLIMage.flimage_io.grabbing);
        }

        public void UncageOnce()
        {
            FLIMage.ExternalCommand("StartUncaging");
            if (FLIMage.uncaging_panel != null)
                Debug.WriteLine("Start Uncaging started : " + FLIMage.uncaging_panel.uncaging_running);
            else
                MessageBox.Show("Uncaging failed : please open uncaging panel");
        }

        public void DO_Once()
        {
            FLIMage.ExternalCommand("StartDO");
            if (FLIMage.digital_panel != null)
                Debug.WriteLine("Start DO started: " + FLIMage.digital_panel.digital_running);
            else
                MessageBox.Show("DO failed : please open digital output control panel");
        }

        public void ImageSequenceGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            if (ImageSequenceGridView.CurrentCell != null)
            {
                selectedRowIndex = ImageSequenceGridView.CurrentCell.RowIndex;

                ImageSequenceGridView.Rows[selectedRowIndex].Selected = true;
                int repetition = Convert.ToInt32(ImageSequenceGridView.Rows[selectedRowIndex].Cells["Repetition"].Value);
                RepNumber.Text = "/" + repetition.ToString();
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
            int num = Convert.ToInt32(ImageSequenceGridView.Rows[selectedRowIndex].Cells["SettingID"].Value);
            LoadSetting_Number(num);
            double zoom = Convert.ToDouble(ImageSequenceGridView.Rows[selectedRowIndex].Cells["Zoom"].Value);
            State.Acq.zoom = zoom;
            FLIMage.ExternalCommand("UpdateGUI");
        }

        public void EditProc()
        {
            SaveTable();
            SaveWindowLocation();
        }

        public void ImageSequenceGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            EditProc();
        }
        public void dataGridView1_CellValueChanged(object sender, EventArgs e)
        {
            if (ImageSequenceGridView.IsCurrentCellDirty)
            {
                EditProc();
            }
        }

        public void AutoDriftCorrection_Click(object sender, EventArgs e)
        {
            if (AutoDriftCorrection.Checked)
            {
                if (FLIMage.drift_correction == null)
                    FLIMage.drift_correction = new DriftCorrection(FLIMage);
                FLIMage.drift_correction.Show();
            }
            autoDriftCorrection = AutoDriftCorrection.Checked;
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (!pause)
            {
                timer_rep.Stop();
                if (!running_current)
                {
                    PauseButton.Text = "Restart";
                    PauseButton.Font = new Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    wait_current_image_finish = false;
                }
                else
                {
                    PauseButton.Text = "Pause after current";
                    PauseButton.Font = new Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    wait_current_image_finish = true;
                }
                pause = true;
            }
            else
            {
                timer_rep.Start();
                PauseButton.Text = "Pause";
                PauseButton.Font = new Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                pause = false;
                wait_current_image_finish = false;
            }
        }
    }
}
