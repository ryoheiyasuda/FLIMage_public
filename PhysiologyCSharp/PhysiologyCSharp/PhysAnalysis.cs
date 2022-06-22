using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using MathLibrary;
using System.Diagnostics;
using Utilities;

namespace PhysiologyCSharp
{
    public partial class PhysAnalysis : Form
    {
        public string FolderPath;
        public string BaseName;
        public string ProjectExtension = ".prj";

        public double outputRate;
        public int dispCh;
        public int nChannels;
        public int currentFileNumber;
        public int[] measMod = { 0, 0 };

        public string fileNumberStr;

        public double[] BaseRange = new double[] { 0, 10 };
        public double[] SignalRange = new double[] { 50, 52 };


        int[] BaseRangeInt = new int[2];
        int[] SignalRangeInt = new int[2];

        double[] time1;
        double[] BaseLineTime;
        double[] BaseLineValues;
        double[] SignalTime;
        double[] SignalValues;
        double[] LinearReg;
        double[] FitCurve;

        double[][] AverageTimeCourse;

        List<int> fileNumbers = new List<int>();
        List<string> fileNames = new List<string>();
        List<DateTime> triggerTimes = new List<DateTime>();
        Dictionary<int, double[][]> dataset;

        Utilities.PlotOnPictureBox plot;

        WindowLocManager winManager;
        String windowName = "PhysAnalysis";
        String windowsInfoPath;

        public PhysAnalysis(StimPanel stim_Panel)
        {
            InitializeComponent();
            plot = new Utilities.PlotOnPictureBox(PlotPicBox);
            windowsInfoPath = stim_Panel.windowsInfoPath;
        }

        public void SaveWindowLoc()
        {
            winManager.SaveWindowLocation();
        }

        private void openFileSeriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = FileIO.AskOpenFileName("");
            OpenAll(filename);
        }

        private void OpenAll(String filename)
        {
            if (FileIO.filename_parts(filename, out string folderPath, out string baseName, out string fileExtension, out int fileN) != 0)
                return;

            BaseName = baseName;
            FolderPath = folderPath;

            PhysParameters phys_parameters;
            IOControls.MC700_Parameters mc700_params;
            phys_parameters = new PhysParameters();
            dataset = new Dictionary<int, double[][]>();
            mc700_params = new IOControls.MC700_Parameters();
            fileNames.Clear();
            triggerTimes.Clear();
            dataset.Clear();
            fileNumbers.Clear();

            if (fileExtension == ".txt")
            {
                string[] files = Directory.GetFiles(folderPath, baseName + '*' + fileExtension);

                foreach (var file in files)
                {
                    if (File.Exists(file) && FileIO.TextFileToData(file, phys_parameters, mc700_params, out DateTime triggerTime, out double[][] data1) == 0)
                    {
                        FileIO.filename_parts(file, out folderPath, out baseName, out fileExtension, out fileN);
                        outputRate = phys_parameters.outputRate;
                        nChannels = phys_parameters.nChannelsPatch;
                        measMod = mc700_params.Mode;
                        dataset.Add(fileN, data1);
                        fileNumbers.Add(fileN);
                        fileNames.Add(String.Format("{0}{1:000}", BaseName, fileN));
                        triggerTimes.Add(triggerTime);
                        currentFileNumber = fileN;
                    }
                }
            }
            else if (fileExtension == PlotData.EpochExtension)
            {
                var lines = File.ReadAllLines(filename);
                foreach (var line in lines)
                {
                    var values = line.Split(',');
                    if (values[0] == "Time")
                        ;
                    else if (values[0].Contains("Average"))
                        ; //this is average! Do we need it?
                    else
                    {
                        var props = values[0].Split(' ');
                        fileN = -1;
                        int channel = -1;
                        foreach (var prop in props)
                        {
                            var sP = prop.Split('=');
                            if (sP[0] == "FileNum")
                                fileN = Convert.ToInt32(sP[1]);
                            else if (sP[0] == "Ch")
                                channel = Convert.ToInt32(sP[1]) - 1;
                        }

                        if (fileN > 0 && channel == 0)
                        {
                            var each_fileName = String.Format("{0}{1:000}", BaseName, fileN);
                            each_fileName = Path.Combine(folderPath, each_fileName) + PlotData.FileExtension;
                            if (File.Exists(each_fileName) && FileIO.TextFileToData(each_fileName, phys_parameters, mc700_params, out DateTime triggerTime, out double[][] data1) == 0)
                            {
                                outputRate = phys_parameters.outputRate;
                                nChannels = phys_parameters.nChannelsPatch;
                                measMod = mc700_params.Mode;
                                dataset.Add(fileN, data1);
                                fileNumbers.Add(fileN);
                                fileNames.Add(String.Format("{0}{1:000}", BaseName, fileN));
                                triggerTimes.Add(triggerTime);
                                currentFileNumber = fileN;
                            }
                        }
                    }

                }
            }

            updateGUI();
        }

        private void updateGUI()
        {
            fileNumberStr = Utilities.StringHandling.ConvertIntArrayToMatLabStyleText(fileNumbers.ToArray());
            fileNumberBox.Text = fileNumberStr;

            Name = "Fis-Analysis: " + BaseName;
            FileNumberLabel.Text = currentFileNumber.ToString();

            BaseStartBox.Text = BaseRange[0].ToString();
            BaseEndBox.Text = BaseRange[1].ToString();
            SignalStartBox.Text = SignalRange[0].ToString();
            SignalEndBox.Text = SignalRange[1].ToString();

            LinearReg = CalculateValue(currentFileNumber, dispCh);
            updatePlot();
        }

        private void updatePlot()
        {
            List<string> legenedStr = new List<string>();
            plot.ClearData();
            float linewidth = 1;
            foreach (var key in dataset.Keys)
            {
                plot.AddData(time1, dataset[key][dispCh], "-", linewidth);
                //legenedStr.Add(String.Format("{0}{1:000}", BaseName, Convert.ToInt32(key)));
            }

            plot.AddData(time1, dataset[currentFileNumber][dispCh], "-r", 2.5f);

            if (averageRadio.Checked)
                plot.AddData(BaseLineTime, BaseLineValues, "-k", 3);

            plot.AddData(SignalTime, FitCurve, "-k", 3);

            plot.AddData(time1, AverageTimeCourse[dispCh], "-k", 3.0f);

            //plot.AddLegend(legenedStr);
            plot.XTitle = "Time (ms)";
            if (measMod[dispCh] == 0)
                plot.YTitle = "Current (pA)";
            else
                plot.YTitle = "Voltage (mV)";
            plot.UpdatePlot();
        }

        private void FileUpButton_Click(object sender, EventArgs e)
        {
            if (fileNumbers.Count > 0)
            {
                int indx = fileNumbers.IndexOf(currentFileNumber);
                indx++;
                UpdateFromIndex(indx);
            }
        }

        private void FileDOwnButton_Click(object sender, EventArgs e)
        {
            if (fileNumbers.Count > 0)
            {
                int indx = fileNumbers.IndexOf(currentFileNumber);
                indx--;
                UpdateFromIndex(indx);
            }
        }

        private double[] CalculateValue(int fileNumber, int channel)
        {
            AverageTimeCourse = new double[nChannels][];
            for (int i = 0; i < nChannels; i++)
            {
                double[] ave = new double[1];
                bool first = true;
                int num = 0;
                foreach (var key in dataset.Keys)
                {
                    if (first)
                    {
                        ave = (double[])dataset[key][i].Clone();
                        int nSamples = ave.Length;
                        time1 = Enumerable.Range(0, nSamples).Select(x => (double)x * 1000.0 / outputRate).ToArray();
                        first = false;
                    }
                    else
                        MatrixCalc.ArrayCalc(ave, dataset[key][i], CalculationType.Add);
                    num++;
                }
                AverageTimeCourse[i] = MatrixCalc.DivideConstantFromVector(ave, num);
            }

            if (dataset.TryGetValue(fileNumber, out double[][] data1))
            {
                if (channel >= nChannels)
                    return null;

                double[][] dataChs = data1;
                double[] data = dataChs[channel];
                for (int i = 0; i < 2; i++)
                {
                    BaseRangeInt[i] = (int)(BaseRange[i] / 1000 * outputRate);
                    SignalRangeInt[i] = (int)(SignalRange[i] / 1000 * outputRate);
                }

                if (BaseRangeInt[1] < time1.Length && BaseRangeInt[0] < BaseRangeInt[1] && BaseRangeInt[0] >= 0)
                {
                    BaseLineTime = new double[BaseRangeInt[1] - BaseRangeInt[0]];
                    BaseLineValues = new double[BaseRangeInt[1] - BaseRangeInt[0]];
                    Array.Copy(time1, BaseRangeInt[0], BaseLineTime, 0, BaseLineTime.Length);
                    Array.Copy(data, BaseRangeInt[0], BaseLineValues, 0, BaseLineValues.Length);
                }

                int signal_length = SignalRangeInt[1] - SignalRangeInt[0];
                if (SignalRangeInt[1] < time1.Length && SignalRangeInt[0] < SignalRangeInt[1] && SignalRangeInt[0] >= 0)
                {
                    SignalTime = new double[signal_length];
                    SignalValues = new double[signal_length];
                    FitCurve = new double[signal_length];
                    Array.Copy(time1, SignalRangeInt[0], SignalTime, 0, signal_length);
                    Array.Copy(data, SignalRangeInt[0], SignalValues, 0, signal_length);
                }
                else
                    return null;

                double[] beta;
                if (averageRadio.Checked)
                {
                    double value = SignalValues.Average() - BaseLineValues.Average();
                    //value = Math.Round(value * 1000) / 1000;
                    //ValueBox.Text = value.ToString();
                    beta = new double[] { SignalValues.Average(), BaseLineValues.Average() };
                    for (int i = 0; i < signal_length; i++)
                        FitCurve[i] = beta[0];
                }
                else
                {
                    beta = MatrixCalc.linearRegression(SignalTime, SignalValues); //a + bx
                    //ValueBox.Text = beta[1].ToString();
                    for (int i = 0; i < signal_length; i++)
                        FitCurve[i] = beta[0] + beta[1] * SignalTime[i];
                }
                return beta;
            }
            else
                return null;
        }

        private void Ch1_CheckedChanged(object sender, EventArgs e)
        {
            dispCh = 0;
            if (ChRadio1.Checked)
                dispCh = 0;
            else if (ChRadio2.Checked && dataset[currentFileNumber].Length >= 2)
                dispCh = 1;
            else
            {
                //Default.
                dispCh = 0;
                ChRadio2.Checked = false;
                ChRadio1.Checked = true;
            }
            updateGUI();
        }

        private void UpdateFromIndex(int indx)
        {
            if (fileNumbers.Count > 0)
            {
                if (indx < 0)
                    indx = 0;
                if (indx > fileNumbers.Count - 1)
                    indx = fileNumbers.Count - 1;
                currentFileNumber = fileNumbers[indx];
                updateGUI();
            }

        }

        private void DelButton_Click(object sender, EventArgs e)
        {
            int indx = fileNumbers.IndexOf(currentFileNumber);
            fileNumbers.RemoveAt(indx);
            dataset.Remove(currentFileNumber);
            UpdateFromIndex(indx);
        }

        private void PhysAnalysis_Load(object sender, EventArgs e)
        {
            averageRadio.Checked = true;
            winManager = new WindowLocManager(this, windowName, windowsInfoPath);
            winManager.LoadWindowLocation(false);
        }

        private void RangeBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Double.TryParse(BaseStartBox.Text, out BaseRange[0]);
                Double.TryParse(BaseEndBox.Text, out BaseRange[1]);

                Double.TryParse(SignalStartBox.Text, out SignalRange[0]);
                Double.TryParse(SignalEndBox.Text, out SignalRange[1]);

                updateGUI();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = FileIO.AskOpenFileName(ProjectExtension);
            if (filename == "")
                return;
            var lines = File.ReadAllLines(filename);
            foreach (string line in lines)
            {
                var sP = line.Split('=');
                var name = sP[0];
                var field = this.GetType().GetField(name);
                if (field.FieldType == typeof(double[]))
                {
                    Array.ConvertAll(sP[1].Split(','), double.Parse);
                }
                else
                {
                    field.SetValue(this, Convert.ChangeType(sP[1], field.FieldType));
                }
            }

            Utilities.StringHandling.ConvertMatlabStyleTextToIntArray(fileNumberStr, out int[] intVals);
            fileNumbers = intVals.ToList();

            PhysParameters phys_parameters;
            IOControls.MC700_Parameters mc700_params;
            phys_parameters = new PhysParameters();
            dataset = new Dictionary<int, double[][]>();
            mc700_params = new IOControls.MC700_Parameters();
            fileNames.Clear();
            triggerTimes.Clear();

            foreach (var fileN in fileNumbers)
            {
                string tmpName = String.Format("{0}{1:000}{2}", BaseName, fileN, PlotData.FileExtension);
                string file = Path.Combine(FolderPath, tmpName);
                if (FileIO.TextFileToData(file, phys_parameters, mc700_params, out DateTime triggerTime, out double[][] data1) == 0) //new data1 is created.
                {
                    outputRate = phys_parameters.outputRate;
                    nChannels = phys_parameters.nChannelsPatch;

                    dataset.Add(fileN, data1);
                    fileNames.Add(String.Format("{0}{1:000}", BaseName, fileN));
                    triggerTimes.Add(triggerTime);
                }
            }
            updateGUI();
        }

        private void saveThisProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = FileIO.AskSaveFileName(BaseName, ProjectExtension);
            StringBuilder sb = new StringBuilder();
            var fields = this.GetType().GetFields();
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(double[]))
                {
                    sb.Append(field.Name + "=");
                    var values = (double[])field.GetValue(this);
                    sb.Append(String.Join(",", values));
                    sb.AppendLine();
                }
                else if (field.FieldType == typeof(int) || field.FieldType == typeof(double) || field.FieldType == typeof(string))
                {
                    sb.Append(field.Name + "=");
                    sb.Append(field.GetValue(this));
                    sb.AppendLine();
                }
            }

            File.WriteAllText(filename, sb.ToString());
        }

        private void averageRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (dataset != null)
            {
                LinearReg = CalculateValue(currentFileNumber, dispCh);
                updatePlot();
            }
        }


        private void ExportAverage_Click(object sender, EventArgs e)
        {
            var filename = FileIO.AskSaveFileName(BaseName + "_average", ".csv");

            if (filename == "")
                return;

            if (AverageTimeCourse != null && AverageTimeCourse.Length > 0)
            {
                var sb = new StringBuilder();
                sb.Append("Time (ms),");
                for (int i = 0; i < AverageTimeCourse.Length; i++)
                {
                    sb.Append("Average Ch" + (i + 1) + ",");
                }

                sb.AppendLine();

                for (int j = 0; j < AverageTimeCourse[0].Length; j++)
                {
                    sb.Append(j / outputRate * 1000);
                    sb.Append(",");
                    for (int i = 0; i < AverageTimeCourse.Length; i++)
                    {
                        sb.Append(AverageTimeCourse[i][j].ToString("0.000"));
                        sb.Append(",");
                    }
                    sb.AppendLine();
                }

                filename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + "_average.csv");

                FormControllers.CloseOpenExcelWindow(Path.GetFileNameWithoutExtension(filename));
                File.WriteAllText(filename, sb.ToString());

                if (File.Exists(filename))
                {
                    Process ExternalProcess = new Process();
                    ExternalProcess.StartInfo.FileName = filename;
                    ExternalProcess.Start();
                }
            }
        }

        private void CalculateButton_Click(object sender, EventArgs e)
        {
            var filename = FileIO.AskSaveFileName(BaseName + "_data", ".csv");

            //var filename = Path.Combine(FolderPath, BaseName + "_timeCoursePeak.csv");
            if (filename != "")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("FileNumber,");
                sb.Append(String.Join(",", fileNumbers));
                sb.AppendLine();

                sb.Append("Trigger time,");
                foreach (DateTime dt in triggerTimes)
                {
                    sb.Append(dt.ToString("yyyy-MM-ddTHH:mm:ss.fff,"));
                }
                sb.AppendLine();

                sb.Append("Time (s), ");
                foreach (DateTime dt in triggerTimes)
                {
                    var value = dt - triggerTimes[0];
                    double val = value.TotalMilliseconds / 1000.0;
                    sb.Append(val.ToString("0.000") + ",");
                }
                sb.AppendLine();

                for (int i = 0; i < nChannels; i++)
                {
                    sb.Append("Channel" + (i + 1) + ",");
                    foreach (int fileN in fileNumbers)
                    {
                        var beta = CalculateValue(fileN, i);
                        double val;
                        if (beta != null)
                        {
                            if (averageRadio.Checked)
                                val = beta[0] - beta[1];
                            else
                                val = beta[1]; //Slope.

                            sb.Append(val + ",");
                        }
                        else
                            sb.Append("NaN");

                    }
                    sb.AppendLine();
                }

                FormControllers.CloseOpenExcelWindow(Path.GetFileNameWithoutExtension(filename));
                File.WriteAllText(filename, sb.ToString());

                if (File.Exists(filename))
                {
                    Process ExternalProcess = new Process();
                    ExternalProcess.StartInfo.FileName = filename;
                    ExternalProcess.Start();
                }

 
            }
        }




        private void PhysAnalysis_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowLoc();
        }

        private void fileNumberBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            fileNumberStr = fileNumberBox.Text;
            StringHandling.ConvertMatlabStyleTextToIntArray(fileNumberStr, out int[] fileNumArray);
            if (fileNumArray != null)
            {
                PhysParameters phys_parameters;
                IOControls.MC700_Parameters mc700_params;
                phys_parameters = new PhysParameters();
                dataset = new Dictionary<int, double[][]>();
                mc700_params = new IOControls.MC700_Parameters();
                fileNames.Clear();
                triggerTimes.Clear();
                dataset.Clear();
                fileNumbers.Clear();

                string[] files = Directory.GetFiles(FolderPath, BaseName + '*' + PlotData.FileExtension);

                foreach (var file in files)
                    if (FileIO.TextFileToData(file, phys_parameters, mc700_params, out DateTime triggerTime, out double[][] data1) == 0)
                    {
                        FileIO.filename_parts(file, out string folderPath, out string baseName, out string fileExtension, out int fileN);
                        outputRate = phys_parameters.outputRate;
                        nChannels = phys_parameters.nChannelsPatch;
                        measMod = mc700_params.Mode;

                        if (Array.Exists(fileNumArray, x => x == fileN))
                        {
                            dataset.Add(fileN, data1);
                            fileNumbers.Add(fileN);
                            fileNames.Add(String.Format("{0}{1:000}", baseName, fileN));
                            triggerTimes.Add(triggerTime);
                            currentFileNumber = fileN;
                        }
                    }
            }
            updateGUI();

            e.Handled = true;
            e.SuppressKeyPress = true;
        } //KeyDown

    }
}

