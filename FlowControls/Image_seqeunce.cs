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
        int current_repeat = 0;
        int current_row = 0;

        bool abortGrabActivated = false;
        int CheckingInterval_ms = 100;
        bool autoDriftCorrection = false;

        public Image_seqeunce(FLIMageMain FLIMage_in)
        {
            InitializeComponent();

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
            ImageSequenceGridView.Rows.Add(settingIDLargest, fn, State.Acq.imageInterval, State.Acq.nImages, State.Acq.zoom);
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
            int selected = ImageSequenceGridView.CurrentCell.RowIndex;
            int num = Convert.ToInt32(ImageSequenceGridView.Rows[selected].Cells["SettingID"].Value);

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
            State = FLIMage.State;


            if (RunSeq.Text == "Abort")
            {
                RunSeq.Text = "Run sequence";
                abortGrabActivated = true;
                FLIMage.ExternalCommand("AbortGrab");
            }
            else
            {
                abortGrabActivated = false;
                RunSeq.Text = "Abort";
                Task.Factory.StartNew((Action)delegate
                {
                    RunSequence();
                });
            }
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
                Progress.Text = String.Format("{0} / {1}", current_repeat + 1, repetition);

                RepTime.Text = String.Format("{0:0.0}", timer_rep.ElapsedMilliseconds / 1000.0);
            });
        }

        public void LoadSetting(String fileName)
        {
            FLIMage.BeginInvoke((Action)delegate
            {
                FLIMage.LoadSettingFile(fileName, false);
            });
        }

        public void RunSequence()
        {
            FLIMage.flimage_io.imageSequencing = true;
            bool firstImage = false;
            if (autoDriftCorrection)
                FLIMage.drift_correction.TurnOnOffCorrection(false);

            current_row = 0;
            for (int i = 0; i < ImageSequenceGridView.Rows.Count; i++)
            {
                current_row = i;
                current_repeat = 0;

                //int num = Convert.ToInt32(ImageSequenceGridView.Rows[i].Cells["SettingID"].Value);
                //LoadSetting_Number(num);                

                int interval_ms = (int)(1000.0 * Convert.ToDouble(ImageSequenceGridView.Rows[i].Cells["Interval"].Value));
                int repetition = Convert.ToInt32(ImageSequenceGridView.Rows[i].Cells["Repetition"].Value);

                reportProgress(false); //Select current row.
                LoadSelectedSetting();


                for (int rep = 0; rep < repetition; rep++)
                {
                    firstImage = rep == 0 && i == 0;
                    current_repeat = rep;

                    timer_rep.Restart();

                    RunOnce();

                    System.Threading.Thread.Sleep(CheckingInterval_ms);

                    while (FLIMage.flimage_io.grabbing && !abortGrabActivated)
                    {
                        System.Threading.Thread.Sleep(CheckingInterval_ms);
                        reportProgress(true);
                    }

                    if (autoDriftCorrection && firstImage)
                    {
                        FLIMage.drift_correction.SelectImage();
                        FLIMage.drift_correction.TurnOnOffCorrection(true);
                    }

                    while (interval_ms - (int)timer_rep.ElapsedMilliseconds > CheckingInterval_ms)
                    {
                        if (abortGrabActivated)
                            return;
                        System.Threading.Thread.Sleep(CheckingInterval_ms);
                        reportProgress(true);
                    }

                    if ((int)timer_rep.ElapsedMilliseconds < interval_ms)
                    {
                        if (abortGrabActivated)
                            return;
                        System.Threading.Thread.Sleep(interval_ms - (int)timer_rep.ElapsedMilliseconds);
                        reportProgress(true);
                    }
                }

            }

            FLIMage.flimage_io.imageSequencing = false;
            FLIMage.Invoke((Action)delegate
            {
                FLIMage.ChangeItemsStatus(true, false);
            });
        }

        public void RunOnce()
        {
            FLIMage.ExternalCommand("StartGrab");
            Debug.WriteLine("Start Grab started : " + FLIMage.flimage_io.grabbing);
        }

        public void ImageSequenceGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            if (ImageSequenceGridView.CurrentCell != null)
            {
                int index = ImageSequenceGridView.CurrentCell.RowIndex;
                ImageSequenceGridView.Rows[index].Selected = true;
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
            int index = ImageSequenceGridView.CurrentCell.RowIndex;
            int num = Convert.ToInt32(ImageSequenceGridView.Rows[index].Cells["SettingID"].Value);
            LoadSetting_Number(num);
            double zoom = Convert.ToDouble(ImageSequenceGridView.Rows[index].Cells["Zoom"].Value);
            State.Acq.zoom = zoom;
            FLIMage.ExternalCommand("UpdateGUI");
        }


        public void ImageSequenceGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            SaveTable();
            SaveWindowLocation();
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
    }
}
