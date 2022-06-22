using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace PhysiologyCSharp
{
    public partial class PlotData : Form
    {
        public double[] t = { 0, 150 };
        public double[][] data = new double[1][];

        public double[] MCGain = { 1, 1 };
        public int[] MCMode = { 1, 1 };
        public int dispCh = 1;

        public string FileName = "yphys001.txt";
        public string FolderPathName = "";
        public string BaseName = "yphys";
        public int FileCounter = 0;

        static public string FileExtension = ".txt";
        static public string EpochExtension = ".csv";

        object scopeLock = new object();

        //Scope data.
        bool scope = false;
        double[][] scopeOutData;
        Timer ScopeTimer = new Timer();
        ScopeParam scope_param;
        StimPanel stim_panel;
        IOControls scope_io_controls;
        bool scope_ok = false; //in case NIDAQ etc fails.

        double sampleLength_ms = 0;

        PlotOnPictureBox plot;

        public string currentEpochKey;
        public Dictionary<string, DataSetEpoch> data_setDict = new Dictionary<string, DataSetEpoch>();

        WindowLocManager winManager;
        String windowName;
        String windowName_data = "PlotData";
        String windowName_Scope = "PlotScope";

        public class DataSetEpoch
        {
            public int epoch;
            public int pulse;
            public string keyname;
            public Dictionary<string, double[][]> dataset = new Dictionary<string, double[][]>();
            public double[][] average_data;
            public string basename;
            public int nChannels = 1;
            public int nSamples = 1;
            public DataSetEpoch(int _epoch, int _pulse, string _basename)
            {
                epoch = _epoch;
                pulse = _pulse;
                basename = _basename;
                keyname = GetKeyName(_epoch, _pulse, _basename);
            }
            static public string GetKeyName(int epoch, int pulse, string basename)
            {
                return basename + "_e" + epoch + "p" + pulse;
            }
            public void AddData(double[][] data_in, int fileNum)
            {
                var datacopy = new double[data_in.Length][];
                for (int ch = 0; ch < data_in.Length; ch++)
                    datacopy[ch] = (double[])data_in[ch].Clone();

                if (nChannels < data_in.Length)
                    nChannels = data_in.Length;
                if (nSamples < data_in[0].Length)
                    nSamples = data_in[0].Length;

                if (dataset.TryGetValue(fileNum.ToString(), out double[][] values))
                    dataset[fileNum.ToString()] = datacopy;
                else
                    dataset.Add(fileNum.ToString(), datacopy);
                AverageData();
            }
            public void AverageData()
            {
                average_data = new double[nChannels][];
                for (int i = 0; i < nChannels; i++)
                {
                    average_data[i] = new double[nSamples];
                    int[] data_N = new int[nSamples];

                    foreach (var fileNum in dataset.Keys)
                    {
                        var data1 = dataset[fileNum];
                        if (data1.Length > i)
                        {
                            for (int j = 0; j < nSamples; j++)
                            {
                                if (data1[i] != null && data1[i].Length > j)
                                {
                                    average_data[i][j] += data1[i][j];
                                    data_N[j]++;
                                }
                            }
                        }
                    }

                    for (int j = 0; j < nSamples; j++)
                        average_data[i][j] /= data_N[j];

                }
            }
        }


        public class ScopeParam
        {
            public double ScopeWidthV = 50;
            public double ScopeAmpV = -5;
            public double ScopeWidthC = 100;
            public double ScopeAmpC = 800;
            public double intervalV = 0.3;
            public double intervalC = 1;
            public double outputRate = 10000;
        }

        public PlotData(bool use_scope, StimPanel _stim_panel)
        {
            InitializeComponent();
            scope = use_scope;
            stim_panel = _stim_panel;

            plot = new PlotOnPictureBox(PhysDataPlot);

            //Put temporal data...
            t = Enumerable.Range(0, 100).Select(x => (double)x * 1000.0 / stim_panel.phys_parameters.outputRate).ToArray();
            int nChannels = stim_panel.phys_parameters.nChannelsPatch; //IOControls.nPatchChannels;
            data = new double[nChannels][];
            for (int i = 0; i < nChannels; i++)
                data[i] = new double[t.Length]; //Just to fill some data.

            if (!scope)
            {
                StartButton.Visible = false;
                ScopePanel.Visible = false;
                EpochControlBox.Location = new Point(ScopePanel.Location.X, ScopePanel.Location.Y);
                windowName = windowName_data;
            }
            else
            {
                FilePanel.Visible = false;
                EpochControlBox.Visible = false;

                windowName = windowName_Scope;

                var ps = new PhysParameters();
                scope_param = new ScopeParam();
                try
                {
                    scope_io_controls = new IOControls(ps.initFolderPath, false);
                    scope_io_controls.AcqDone += new IOControls.AcqDoneHandler(AcquiredDoneHandlerFcn);
                    scope_ok = true;
                }
                catch
                {
                    scope_ok = false;
                }
                UpdateGUI();
            }

        }

        private void PlotData_Load(object sender, EventArgs e)
        {
            winManager = new WindowLocManager(this, windowName, stim_panel.windowsInfoPath);
            winManager.LoadWindowLocation(false);
        }

        private void ScopeParamToPanel()
        {
            if (scope)
            {
                double width, amp, interval;
                if (MCMode.Length <= dispCh)
                {
                    dispCh = 0;
                }

                if (MCMode.Length <= dispCh || MCMode[dispCh] == 0)
                {
                    width = scope_param.ScopeWidthV;
                    amp = scope_param.ScopeAmpV;
                    interval = scope_param.intervalV;
                }
                else
                {
                    width = scope_param.ScopeWidthC;
                    amp = scope_param.ScopeAmpC;
                    interval = scope_param.intervalC;
                }

                sampleLength_ms = width * 3;

                WidthBox.Text = width.ToString();
                AmpBox.Text = amp.ToString();
                IntervalBox.Text = interval.ToString();

                int nSamples = (int)(3 * width * scope_param.outputRate / 1000);
                int nSampleThird = (int)(width * scope_param.outputRate / 1000);
                scopeOutData = new Double[IOControls.nPatchChannels + IOControls.nStimChannels][];
                for (int i = 0; i < scopeOutData.Length; i++)
                {
                    scopeOutData[i] = new double[nSamples];
                    if (i == dispCh)
                    {
                        for (int j = nSampleThird; j < nSampleThird * 2; j++)
                            scopeOutData[i][j] = amp;
                    }
                }
            }
        }

        public void UpdateGUI()
        {
            if (scope && scope_ok)
            {
                scope_io_controls.RetrieveMC700Info();
                MCMode = scope_io_controls.mc700_params.Mode;
                MCGain = scope_io_controls.mc700_params.PrimaryGain;
                ScopeParamToPanel();
            }
            else
            {
                if (stim_panel != null && stim_panel.phys_parameters != null)
                {
                    EpochBox.Text = stim_panel.phys_parameters.epoch.ToString();
                    PulseBox.Text = stim_panel.phys_parameters.currentPulseN.ToString();
                }
            }

            if (MCMode != null && MCMode.Length != 0)
            {
                GainBox1.Text = MCGain[0].ToString();
                if (MCMode[0] == 0)
                    VClampLabel1.Text = "V-Clamp";
                else if (MCMode[0] == 1)
                    VClampLabel1.Text = "C-Clamp";
                else
                    VClampLabel1.Text = "I = 0";


                Ch2Panel.Visible = stim_panel.phys_parameters.nChannelsPatch >= 2;
                DisplayPanel.Visible = stim_panel.phys_parameters.nChannelsPatch >= 2;
                if (IOControls.nPatchChannels <= 1)
                {
                    dispCh = 0;
                }
                else if (MCGain.Length >= 2)
                {
                    GainBox2.Text = MCGain[1].ToString();

                    if (MCMode[1] == 0)
                        VClampLabel2.Text = "V-Clamp";
                    else if (MCMode[1] == 1)
                        VClampLabel2.Text = "C-Clamp";
                    else
                        VClampLabel2.Text = "I = 0";

                    dispCh = dispCh < 1 ? 0 : 1;
                }
            }
            ChRadio1.Checked = dispCh == 0;
            ChRadio2.Checked = dispCh == 1;
            UpdatePlot();
            PhysDataPlot.Invalidate();
        }

        public string SetSaveFolderAndBaseName()
        {
            string filename = FileIO.AskSaveFolderAndBaseName();
            if (filename != "")
            {
                BaseName = Path.GetFileNameWithoutExtension(filename);
                FolderPathName = Path.GetDirectoryName(filename);

                string tempfilename = String.Format("{0}{1:000}{2}", BaseName, 0, FileExtension);
                Text = Path.Combine(FolderPathName, tempfilename);

                data_setDict.Clear();
            }
            return filename;
        }

        public void AcquiredDataPlotAndSave(double[] x, double[][] y, IOControls.MC700_Parameters mc700_param)
        {
            FileCounter++;
            FileCounterBox.Text = FileCounter.ToString();

            string filename = String.Format("{0}{1:000}{2}", BaseName, FileCounter, FileExtension);
            string temp_path = Path.Combine(FolderPathName, filename);

            AddDataToDict(y);
            LoadDataAndPlot(x, y, mc700_param.PrimaryGain, mc700_param.Mode);

            SaveFile(temp_path);
            SaveEpochFile();
        }

        public string GetCurrentKey()
        {
            currentEpochKey = DataSetEpoch.GetKeyName(stim_panel.phys_parameters.epoch, stim_panel.phys_parameters.currentPulseN, BaseName);
            return currentEpochKey;
        }

        public void CreateEpoch()
        {
            GetCurrentKey();
            if (data_setDict.TryGetValue(currentEpochKey, out DataSetEpoch values))
                values.dataset.Clear();
            else
                data_setDict.Add(currentEpochKey, new DataSetEpoch(stim_panel.phys_parameters.epoch, stim_panel.phys_parameters.currentPulseN, BaseName));

        }

        public void ResetEpoch()
        {
            CreateEpoch();
            data_setDict[currentEpochKey].AddData(data, FileCounter);
            UpdateGUI();
        }

        public void AddToEpoch()
        {
            if (data_setDict.TryGetValue(GetCurrentKey(), out DataSetEpoch values))
                values.AddData(data, FileCounter);
            else
            {
                data_setDict.Add(GetCurrentKey(), new DataSetEpoch(stim_panel.phys_parameters.epoch, stim_panel.phys_parameters.currentPulseN, BaseName));
                data_setDict[currentEpochKey].AddData(data, FileCounter);
            }
            UpdateGUI();
            SaveEpochFile();
        }

        public void RemoveFromEpoch()
        {
            if (data_setDict.TryGetValue(GetCurrentKey(), out DataSetEpoch values))
            {
                if (values.dataset.TryGetValue(FileCounter.ToString(), out double[][] val))
                {
                    values.dataset.Remove(FileCounter.ToString());
                }
            }
            UpdateGUI();
            SaveEpochFile();
        }

        public void LoadEpochFile()
        {
            var filename = Path.Combine(FolderPathName, GetCurrentKey() + PlotData.EpochExtension);

            String fn = Path.GetFileNameWithoutExtension(filename);
            FormControllers.CloseOpenExcelWindow(fn);

            int nSamples = 1; //max nSamples.
            int nChannels = 1;

            if (File.Exists(filename))
            {
                bool ZeroBase = false; //just in case it starts from ch=0, 1, 2... it should be not necessary...
                CreateEpoch(); //Create and initialize file.
                var lines = File.ReadAllLines(filename);
                foreach (var line in lines)
                {
                    if (line.StartsWith("FileNum"))
                    {
                        var words = line.Split(',');
                        var indx = words[0].IndexOf("Ch");

                        int fileN = Convert.ToInt32(words[0].Substring(0, indx).Split('=')[1]);
                        int ch = Convert.ToInt32(words[0].Substring(indx).Split('=')[1]) - 1;

                        if (ch < 0)
                        {
                            ZeroBase = true;
                        }

                        if (ZeroBase) //if it founds ch == 0, add 1. 
                            ch += 1;

                        if (ch + 1 > nChannels)
                            nChannels = ch + 1;

                        double[] values = new double[words.Length - 1];
                        int nSamplesThis = words.Length - 1;
                        for (int i = 0; i < values.Length; i++)
                        {
                            if (!double.TryParse(words[i + 1], out values[i]))
                            {
                                nSamplesThis = i;
                                break;
                            }
                        }

                        if (nSamplesThis > nSamples)
                            nSamples = nSamplesThis;

                        if (nSamplesThis == 0)
                            values = null;

                        double[][] valuesCh;

                        if (data_setDict[currentEpochKey].dataset.TryGetValue(fileN.ToString(), out valuesCh))
                        {
                            if (valuesCh == null)
                                valuesCh = new double[ch + 1][];

                            if (valuesCh.Length < ch + 1)
                                Array.Resize(ref valuesCh, ch + 1);

                            valuesCh[ch] = values;
                            data_setDict[currentEpochKey].dataset[fileN.ToString()] = valuesCh;
                        }
                        else
                        {
                            valuesCh = new double[ch + 1][];
                            valuesCh[ch] = values;
                            data_setDict[currentEpochKey].dataset.Add(fileN.ToString(), valuesCh);
                        }
                    }//right line.
                } //go through lines

                data_setDict[currentEpochKey].nChannels = nChannels;
                data_setDict[currentEpochKey].nSamples = nSamples;
                data_setDict[currentEpochKey].AverageData();

            } //If file exists.
        }

        public void SaveEpochFile()
        {
            var filename = Path.Combine(FolderPathName, GetCurrentKey() + PlotData.EpochExtension);
            if (data_setDict.TryGetValue(GetCurrentKey(), out DataSetEpoch values))
            {
                var sb = new StringBuilder();

                for (int ch = 0; ch < values.nChannels; ch++)
                {
                    sb.Append("Time,");
                    sb.Append(String.Join(",", t));
                    sb.AppendLine();

                    foreach (string fileNstr in values.dataset.Keys)
                    {
                        sb.Append("FileNum=" + fileNstr + " Ch=" + (ch + 1) + ",");
                        if (values.dataset[fileNstr].Length > ch && values.dataset[fileNstr][ch].Length >= 1)
                        {
                            var valuestr1 = String.Join(",", values.dataset[fileNstr][ch].Select(x => Math.Round(x * 1000.0) / 1000.0).ToArray());
                            sb.Append(valuestr1);
                        }
                        sb.AppendLine();
                    }

                    sb.Append("Average ch=" + (ch + 1) + ",");
                    var valuestr = String.Join(",", values.average_data[ch].Select(x => Math.Round(x * 1009.0) / 1000.0).ToArray());
                    sb.Append(valuestr);
                    sb.AppendLine();
                    sb.AppendLine();
                }

                File.WriteAllText(filename, sb.ToString());
            }
        }

        public double[] RangeData(int range_start, int range_end)
        {
            int len = range_end - range_start;
            double[] rangeArray = new double[len];
            Array.Copy(data[dispCh], range_start, rangeArray, 0, len);
            return rangeArray;
        }

        public void CalculateResistance()
        {
            if (scope && scope_ok && scope_io_controls.mc700_params.Mode[dispCh] == 0)
            {
                double width1 = scope_param.ScopeWidthV;
                double amp1 = scope_param.ScopeAmpV;

                int milli = (int)(scope_param.outputRate / 1000);
                int start_pulse = (int)(scope_param.outputRate * 1 * width1 / 1000.0);
                int end_pulse = (int)(scope_param.outputRate * 2 * width1 / 1000.0);
                int width_int = (int)(scope_param.outputRate * width1 / 1000.0);
                int rangeRin_start = end_pulse - width_int / 3;
                int rangeRinE = end_pulse - 1;
                double chargeRin = RangeData(rangeRin_start, rangeRinE).Average();

                //Now we do charge_in - baseline.
                int baseS = width_int / 3;
                int baseE = width_int - 1; //may not require to -1. 
                var chargeRin_sub = chargeRin - RangeData(baseS, baseE).Average();

                int rangeRs_start = start_pulse - milli;
                int rangeRs_end = start_pulse + 2 * milli; // 2? 0.5 ms average.
                double peak1;
                if (amp1 > 0)
                    peak1 = RangeData(rangeRs_start, rangeRs_end).Max();
                else
                    peak1 = RangeData(rangeRs_start, rangeRs_end).Min();
                double chargeRs = peak1 - chargeRin;

                int rangeCmS = start_pulse;
                int rangeCmE = start_pulse * 2;
                double area1 = RangeData(rangeCmS, rangeCmE).Sum();
                double tau = (area1 - chargeRin * (double)(rangeCmE - rangeCmS)) / chargeRs / scope_param.outputRate; //s

                double Rs = amp1 / chargeRs * 1000;
                double Rin = amp1 / chargeRin_sub * 1000;

                double Cm = (Rs + Rin) * tau * 1000 * 1000 / (Rs * Rin); //picoFarad.

                RsBox.Text = Rs.ToString("0.00");
                RinBox.Text = Rin.ToString("0.00");
                CmBox.Text = Cm.ToString("0.0");
            }
        }

        public void AddDataToDict(double[][] y)
        {
            currentEpochKey = GetCurrentKey();
            if (!data_setDict.TryGetValue(currentEpochKey, out DataSetEpoch val))
            {
                var data_setE = new DataSetEpoch(stim_panel.phys_parameters.epoch, stim_panel.phys_parameters.currentPulseN, BaseName);
                data_setDict.Add(currentEpochKey, data_setE);
            }
            data_setDict[currentEpochKey].AddData(y, FileCounter);
        }

        public void LoadDataAndPlot(double[] x, double[][] y, double[] gain, int[] mode)
        {
            t = (double[])x.Clone();
            data = new double[y.Length][];
            for (int i = 0; i < y.Length; i++)
                data[i] = (double[])y[i].Clone();
            MCGain = gain;
            MCMode = mode;
            UpdateGUI();
        }

        private bool FileInAverage()
        {
            if (data_setDict.TryGetValue(GetCurrentKey(), out DataSetEpoch values))
            {
                if (values.dataset.TryGetValue(FileCounter.ToString(), out double[][] val))
                {
                    FileInThisGroupLabel.Text = "File in this group";
                    FilePlotColor.ForeColor = Color.Red;
                    return true;
                }
            }
            FileInThisGroupLabel.Text = "NOT in this group";
            FilePlotColor.ForeColor = Color.Green;
            return false;
        }

        public void UpdatePlot()
        {
            int _dispCh = 0;
            if (dispCh < data.Length)
                _dispCh = dispCh;

            plot.ClearData();

            if (data_setDict.TryGetValue(GetCurrentKey(), out DataSetEpoch data_set))
            {
                if (ShowAllDataInEpochCheck.Checked)
                {
                    foreach (var filekey in data_set.dataset.Keys)
                    {
                        plot.AddData(t, data_set.dataset[filekey][_dispCh], "-c", 1);
                    }
                }

                if (ShowEpochAveCheck.Checked)
                {
                    plot.AddData(t, data_set.average_data[_dispCh], "-b", 1);
                }
            }

            if (FileInAverage())
                plot.AddData(t, data[_dispCh], "-r", 1);
            else
                plot.AddData(t, data[_dispCh], "-g", 1);

            plot.XTitle = "Time (ms)";
            if (MCMode.Length == 0 || MCMode[dispCh] == 0)
                plot.YTitle = "Current (pA)";
            else
                plot.YTitle = "Voltage (mV)";

            plot.UpdatePlot();
        }


        public void AcquiredDoneHandlerFcn(object sender, EventArgs e)
        {
            if (scope && scope_ok)
            {
                scope_io_controls.GetGain();
                var nSamples = scope_io_controls.dataOutput[0].Length;
                Double[] t = Enumerable.Range(0, nSamples).Select(x => (double)x * 1000.0 / scope_param.outputRate).ToArray();
                LoadDataAndPlot(t, scope_io_controls.dataOutput, scope_io_controls.mc700_params.PrimaryGain, scope_io_controls.mc700_params.Mode);
                CalculateResistance();
            }
        }

        public void StartAcq()
        {
            if (scope && scope_ok)
            {
                //scope_io_controls.PutZero();

                int ret = scope_io_controls.PutValues(scopeOutData, scope_param.outputRate, true);

                if (ret < 0)
                    StopScope();

                scope_io_controls.Start(false, true);
                System.Threading.Thread.Sleep((int)sampleLength_ms + 100);
                scope_io_controls.WaitUntilDone(500);
            }
        }

        private void ScopeTimerEvent(object sender, EventArgs e)
        {
            lock (scopeLock)
                StartAcq();
        }

        public void StopScope()
        {
            if (scope)
            {
                scope_io_controls.Stop();
                ScopeTimer.Stop();
                ScopeTimer.Dispose();
                StartButton.Text = "Start";
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (scope && scope_ok)
            {
                StartButton.InvokeIfRequired(o => o.Enabled = false);
                stim_panel.StopRepeat();
                UpdateGUI();

                double interval = scope_param.intervalV;
                if (scope_io_controls.mc700_params.Mode[dispCh] != 0)
                    interval = scope_param.intervalC;

                if (interval < sampleLength_ms / 1000 + 0.2)
                    interval = sampleLength_ms / 1000 + 0.2;

                if (StartButton.Text == "Start")
                {
                    StartButton.InvokeIfRequired(o => o.Text = "Stop");

                    ScopeTimer = new Timer();
                    ScopeTimer.Tick += new EventHandler(ScopeTimerEvent);
                    ScopeTimer.Interval = (int)(interval * 1000);
                    ScopeTimer.Start();
                    StartAcq();
                }
                else
                {
                    StopScope();
                }

                StartButton.InvokeIfRequired(o => o.Enabled = true);
            }
        }

        private void SetValueFromPanel()
        {
            if (scope && scope_ok)
            {
                scope_io_controls.RetrieveMC700Info();
                MCMode = scope_io_controls.mc700_params.Mode;

                double width = MCMode[dispCh] == 0 ? scope_param.ScopeWidthV : scope_param.ScopeWidthC;
                double amp = MCMode[dispCh] == 0 ? scope_param.ScopeAmpV : scope_param.ScopeAmpC;
                double interval = MCMode[dispCh] == 0 ? scope_param.intervalV : scope_param.intervalC;

                sampleLength_ms = width * 3;

                Double.TryParse(WidthBox.Text, out width);
                Double.TryParse(AmpBox.Text, out amp);
                Double.TryParse(IntervalBox.Text, out interval);

                if (MCMode[dispCh] == 0)
                {
                    scope_param.ScopeWidthV = width;
                    scope_param.ScopeAmpV = amp;
                    scope_param.intervalV = interval;
                }
                else
                {
                    scope_param.ScopeWidthC = width;
                    scope_param.ScopeAmpC = amp;
                    scope_param.intervalC = interval;
                }

                UpdateGUI();
            }//scope
        }

        private void ScopeParamChangedKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetValueFromPanel();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

        }

        public void OpenFile(string filename)
        {
            String fn = Path.GetFileNameWithoutExtension(filename);
            FormControllers.CloseOpenExcelWindow(fn);

            int saveEpoch = stim_panel.phys_parameters.epoch;
            if (FileIO.TextFileToData(filename, stim_panel.phys_parameters, stim_panel.mc700_params, out DateTime triggerTime, out double[][] data1) != 0)
            {
                stim_panel.triggerTime = triggerTime;
                return;
            }

            stim_panel.triggerTime = triggerTime;
            stim_panel.phys_parameters.epoch = saveEpoch; //Necessary? --- perhaps.

            SetFileName(filename);
            if (data1 != null)
            {
                var nSamples = data1[0].Length;
                Double[] t1 = Enumerable.Range(0, nSamples).Select(x => (double)x * 1000.0 / stim_panel.phys_parameters.outputRate).ToArray();
                LoadEpochFile();
                LoadDataAndPlot(t1, data1, stim_panel.mc700_params.PrimaryGain, stim_panel.mc700_params.Mode);
            }

            stim_panel.phys_parameters.nChannelsPatch = data1.Length;
            stim_panel.LoadValuesToPanel(false, false);
        }

        public void SaveFile(string filename)
        {
            String fn = Path.GetFileNameWithoutExtension(filename);
            FormControllers.CloseOpenExcelWindow(fn);

            if (stim_panel.phys_parameters == null)
            {
                //MessageBox.Show("Stim_panel.phys_parameter is null...");
                stim_panel.phys_parameters = new PhysParameters();
            }

            if (stim_panel.mc700_params == null)
            {
                //MessageBox.Show("Stim_panel.mc700_params is null...");
                stim_panel.mc700_params = new IOControls.MC700_Parameters();
            }

            if (stim_panel.triggerTime == null)
            {
                //MessageBox.Show("Stim_panel.triggerTime is null...");
                stim_panel.triggerTime = DateTime.Now;
            }

            try
            {
                FileIO.SaveData(filename, stim_panel.phys_parameters, stim_panel.mc700_params, stim_panel.triggerTime, data);
            }
            catch (Exception EX)
            {
                MessageBox.Show("Problem in saving: " + EX.Message);   
            }

        }

        public void SetFileName(string filename)
        {
            if (FileIO.filename_parts(filename, out string folderPath, out string basename, out string ext, out int fileN) == 0)
            {
                FileName = filename;
                FolderPathName = folderPath;
                BaseName = basename;
                FileCounter = fileN;
                FileExtension = ext;
                FileCounterBox.Text = fileN.ToString();
                Text = filename;
            }
        }

        private void FileDown_Click(object sender, EventArgs e)
        {
            string filename = String.Format("{0}{1:000}{2}", BaseName, FileCounter - 1, FileExtension);
            string temp_path = Path.Combine(FolderPathName, filename);
            if (File.Exists(temp_path))
            {
                OpenFile(temp_path);
            }
        }

        private void FileUp_Click(object sender, EventArgs e)
        {
            string filename = String.Format("{0}{1:000}{2}", BaseName, FileCounter + 1, FileExtension);
            string temp_path = Path.Combine(FolderPathName, filename);
            if (File.Exists(temp_path))
            {
                OpenFile(temp_path);
            }
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            FileCounter = 0;
            FileCounterBox.Text = FileCounter.ToString();
        }

        private void FileCounterBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;


            int FileN = -1;
            Int32.TryParse(FileCounterBox.Text, out FileN);
            if (FileN > 0)
            {
                string filename = String.Format("{0}{1:000}{2}", BaseName, FileN, FileExtension);
                string temp_path = Path.Combine(FolderPathName, filename);
                if (File.Exists(temp_path))
                {
                    OpenFile(temp_path);
                }
            }

            FileCounterBox.Text = FileCounter.ToString();


            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        public void SaveWindowLoc()
        {
            winManager.SaveWindowLocation();
        }

        private void PlotData_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowLoc();
            Hide();
            e.Cancel = true;
        }

        private void CH2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Ch1_CheckedChanged(object sender, EventArgs e)
        {
            dispCh = 0;
            if (ChRadio1.Checked)
                dispCh = 0;
            else if (ChRadio2.Checked && data.Length >= 2)
                dispCh = 1;
            else
            {
                //Default.
                dispCh = 0;
                ChRadio2.Checked = false;
                ChRadio1.Checked = true;
            }
            UpdateGUI();
        }

        private void ShowEpochAveCheck_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        private void ShowAllDataInEpochCheck_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        private void EpochReset_Click(object sender, EventArgs e)
        {
            ResetEpoch();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            AddToEpoch();
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            RemoveFromEpoch();
        }

        private void EpochChanged()
        {
            if (stim_panel.phys_parameters.epoch < 1)
                stim_panel.phys_parameters.epoch = 1;
            EpochBox.Text = stim_panel.phys_parameters.epoch.ToString();
            LoadEpochFile();
            UpdateGUI();
        }

        private void EpochDownButton_Click(object sender, EventArgs e)
        {
            if (stim_panel != null && stim_panel.phys_parameters != null)
            {
                stim_panel.phys_parameters.epoch--;
                EpochChanged();
            }
        }

        private void EpochUpButton_Click(object sender, EventArgs e)
        {
            if (stim_panel != null && stim_panel.phys_parameters != null)
            {
                stim_panel.phys_parameters.epoch++;
                EpochChanged();
            }
        }

        private void EpochBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (stim_panel != null && stim_panel.phys_parameters != null)
                {
                    if (Int32.TryParse(EpochBox.Text, out int result))
                    {
                        stim_panel.phys_parameters.epoch = result;
                        EpochChanged();
                    }
                    else
                    {
                        EpochBox.Text = stim_panel.phys_parameters.epoch.ToString();
                    }
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void PlotData_Resize(object sender, EventArgs e)
        {
            //UpdateGUI();
        }
    }
}
