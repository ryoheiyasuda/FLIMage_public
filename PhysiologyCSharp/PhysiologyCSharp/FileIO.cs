using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace PhysiologyCSharp
{
    public class FileIO
    {
        public static int SaveData(string fileName, PhysParameters phys_param, IOControls.MC700_Parameters mc_param, DateTime triggerTime, double[][] data)
        {
            DataToText(phys_param, mc_param, triggerTime, data, out string formated_string);
            File.WriteAllText(fileName, formated_string);
            return 0;
        }

        public static int TextFileToData(string fileName, PhysParameters phys_param, IOControls.MC700_Parameters mc_param, out DateTime triggerTime, out double[][] data)
        {
            string all_lines = File.ReadAllText(fileName);

            int index_endTriggerTime = all_lines.IndexOf("StimParameters:");
            string timeString = all_lines.Substring(0, index_endTriggerTime);
            var lines = timeString.Split('\r');
            if (lines.Length > 2)
                triggerTime = DateTime.ParseExact(lines[1].Replace("\n", ""), "yyyy-MM-ddTHH:mm:ss.fff", null);
            else
                triggerTime = File.GetCreationTime(fileName);

            if (triggerTime.Year == 1)
                triggerTime = File.GetCreationTime(fileName);

            all_lines = all_lines.Substring(index_endTriggerTime);
            int index_endStim = all_lines.IndexOf("MC700Parameters:");
            if (index_endStim == -1)
            {
                data = null;
                return -1;
            }
            phys_param.StringToParam(all_lines.Substring(0, index_endStim));

            all_lines = all_lines.Substring(index_endStim);
            int index_endMC700 = all_lines.IndexOf("Data:");
            mc_param.FromStringToParam(all_lines.Substring(0, index_endMC700));

            all_lines = all_lines.Substring(index_endMC700);
            var sP = all_lines.Split('\n');

            if (sP[1].Contains("nSamples"))
            {
                //sP[0]: Data:
                //sP[1]: nSamples=100
                //sP[2]: nChannels=2
                //sP[3]: Time, Data1, Data2
                int startLine = 4;
                int nSamples = Convert.ToInt32(sP[1].Split('=')[1]);
                int nChannels = Convert.ToInt32(sP[2].Split('=')[1]);
                data = new double[nChannels][];
                for (int ch = 0; ch < nChannels; ch++)
                    data[ch] = new double[nSamples];

                var separator = ',';
                for (int i = startLine; i < sP.Length; i++)
                {
                    if (sP[i] != "")
                    {
                        var SepStr = sP[i].Split(separator); //Check if separator works.

                        if (i == startLine && SepStr.Length <= nChannels) //It should be nChannels + 1.
                        {
                            SepStr = sP[i].Split('\t');
                            if (SepStr.Length > nChannels)
                                separator = '\t'; //if the file is tab-separatable, separator is changed to tab.                    
                        }

                        if (SepStr.Length > nChannels)
                        {
                            double[] Vals = Array.ConvertAll(SepStr, double.Parse);

                            for (int ch = 0; ch < nChannels; ch++)
                                data[ch][i - startLine] = Vals[ch + 1];
                        }
                    }
                }
            }
            else
            {
                int nCh = 0;
                data = new double[1][];
                for (int i = 1; i < sP.Length; i++)
                {
                    var SepStr = sP[i].Split(',');
                    if (SepStr.Length > 1)
                    {
                        double[] Vals = Array.ConvertAll(SepStr, double.Parse);
                        if (Vals != null && Vals.Length >= 1)
                        {
                            nCh += 1;
                            Array.Resize(ref data, nCh);
                            data[nCh - 1] = Vals;
                        }
                    }
                }
            }
            return 0;
        }

        public static int TextFileToData_old(string fileName, PhysParameters phys_param, IOControls.MC700_Parameters mc_param, out DateTime triggerTime, out double[][] data)
        {
            string all_lines = File.ReadAllText(fileName);

            int index_endTriggerTime = all_lines.IndexOf("StimParameters:");
            string timeString = all_lines.Substring(0, index_endTriggerTime);
            var lines = timeString.Split('\r');
            if (lines.Length > 2)
                triggerTime = DateTime.ParseExact(lines[1].Replace("\n", ""), "yyyy-MM-ddTHH:mm:ss.fff", null);
            else
                triggerTime = File.GetCreationTime(fileName);

            if (triggerTime.Year == 1)
                triggerTime = File.GetCreationTime(fileName);

            all_lines = all_lines.Substring(index_endTriggerTime);
            int index_endStim = all_lines.IndexOf("MC700Parameters:");
            if (index_endStim == -1)
            {
                data = null;
                return -1;
            }
            phys_param.StringToParam(all_lines.Substring(0, index_endStim));

            all_lines = all_lines.Substring(index_endStim);
            int index_endMC700 = all_lines.IndexOf("Data:");
            mc_param.FromStringToParam(all_lines.Substring(0, index_endMC700));

            all_lines = all_lines.Substring(index_endMC700);
            var sP = all_lines.Split('\n');
            int nCh = 0;
            data = new double[1][];
            char separator = ',';

            for (int i = 1; i < sP.Length; i++)
            {
                var SepStr = sP[i].Split(separator); //Check if separator works.

                if (SepStr.Length > 1)
                {
                    double[] Vals = Array.ConvertAll(SepStr, double.Parse);
                    if (Vals != null && Vals.Length >= 1)
                    {
                        nCh += 1;
                        Array.Resize(ref data, nCh);
                        data[nCh - 1] = Vals;
                    }
                }
            }
            return 0;
        }

        public static int filename_parts(string filename, out string folderPath, out string basename, out string ext, out int fileN)
        {
            if (filename != "")
            {
                folderPath = Path.GetDirectoryName(filename);
                string fileName = Path.GetFileNameWithoutExtension(filename);
                ext = Path.GetExtension(filename);

                var sP = fileName.Split('_');
                if (sP.Length > 1 && sP[1][0] == 'e' && sP[1][2] == 'p')
                {
                    basename = sP[0];
                    fileN = Convert.ToInt32(sP[1][1]);
                }
                else
                {
                    string fileNStr = fileName.Substring(fileName.Length - 3);
                    fileN = -1;
                    if (Int32.TryParse(fileNStr, out fileN))
                    {
                        basename = fileName.Substring(0, fileName.Length - 3);
                    }
                    else
                        basename = fileName;
                }

                return 0;
            }
            else
            {
                folderPath = "";
                basename = "";
                ext = "";
                fileN = -1;

                return -1;
            }
        }

        public static int DataToText(PhysParameters phys_param, IOControls.MC700_Parameters mc_param, DateTime triggerTime, double[][] data, out string formated_string)
        {
            var sb = new StringBuilder();
            sb.AppendLine("TriggerTime:");
            sb.Append(triggerTime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
            sb.AppendLine();

            sb.AppendLine("StimParameters:");
            phys_param.ParamToString(out string param_string);
            sb.Append(param_string);
            sb.AppendLine();

            sb.AppendLine("MC700Parameters:");
            sb.AppendLine();
            sb.Append(mc_param.ToString());

            sb.AppendLine();
            sb.AppendLine("Data:");

            sb.AppendLine("nSamples=" + data[0].Length);
            sb.AppendLine("nChannels=" + data.Length);
            sb.Append("Time(ms)\t");
            for (int ch = 0; ch < data.Length; ch++)
            {
                sb.Append("Data" + (ch + 1).ToString());
                if (ch < data.Length - 1)
                    sb.Append("\t");
                else
                    sb.AppendLine();
            }

            for (int i = 0; i < data[0].Length; i++)
            {
                double outputRate = phys_param.outputRate;
                sb.Append((double)i / outputRate * 1000);
                sb.Append("\t");
                for (int ch = 0; ch < data.Length; ch++)
                {
                    sb.Append(data[ch][i]);
                    if (ch < data.Length - 1)
                        sb.Append("\t");
                    else
                        sb.AppendLine();
                }
            }

            formated_string = sb.ToString();
            return 0;
        }

        public static int DataToText_old(PhysParameters phys_param, IOControls.MC700_Parameters mc_param, DateTime triggerTime, double[][] data, out string formated_string)
        {
            var sb = new StringBuilder();
            sb.Append("TriggerTime:");
            sb.AppendLine();
            sb.Append(triggerTime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
            sb.AppendLine();

            sb.Append("StimParameters:");
            sb.AppendLine();
            phys_param.ParamToString(out string param_string);
            sb.Append(param_string);
            sb.AppendLine();

            sb.Append("MC700Parameters:");
            sb.AppendLine();
            sb.Append(mc_param.ToString());
            sb.AppendLine();

            sb.Append("Data:");
            sb.AppendLine();
            for (int i = 0; i < data.Length; i++)
            {
                double[] data_shaped = data[i].Select(x => Math.Round(x * 1000.0) / 1000.0).ToArray(); //Round to 0.01 mV/pA.
                sb.Append(String.Join(",", data_shaped));
                sb.AppendLine();
            }

            formated_string = sb.ToString();

            return 0;
        }

        static public string AskSaveFolderAndBaseName()
        {
            Stream myStream = null;
            SaveFileDialog openFileDialog1 = new SaveFileDialog();

            openFileDialog1.AddExtension = false;
            openFileDialog1.FileName = "BaseName";
            openFileDialog1.Filter = "BaseName (*)|*|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            string fileName = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog1.FileName;
            }
            else
            {
                fileName = "";
            }
            return fileName;
        }




        static public string AskSaveFileName(string defaulFilename, string extension)
        {
            Stream myStream = null;
            SaveFileDialog openFileDialog1 = new SaveFileDialog();

            openFileDialog1.FileName = defaulFilename + extension;
            openFileDialog1.Filter = "Data file (*" + extension + ")|*" + extension + "|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            string fileName = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        fileName = openFileDialog1.FileName;
                        myStream.Close();

                        if (!File.Exists(fileName))
                            fileName = "";
                    }
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                fileName = "";
            }
            return fileName;
        }


        static public string AskOpenFileName(string extension)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.FileName = "filename" + extension;
            openFileDialog1.Filter = "Data file (" + extension + ")|*" + extension + "|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            string fileName = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        fileName = openFileDialog1.FileName;
                        myStream.Close();

                        if (!File.Exists(fileName))
                            fileName = "";
                        //OpenFLIMTiff(fileName);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                fileName = "";
            }
            return fileName;
        }
    }
}
