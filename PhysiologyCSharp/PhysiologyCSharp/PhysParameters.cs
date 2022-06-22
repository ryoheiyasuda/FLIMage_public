using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PhysiologyCSharp
{
    public class PhysParameters
    {
        public double outputRate = 10000;
        public double pulseSetTotalLength_ms = 100;
        public int pulse_set_repeat = 30;
        public int pulse_set_interval = 20;
        public bool sync_with_image = false;
        public bool sync_with_uncage = false;
        public bool acquire_data = true;
        public string PulseName = "Pulse Name";
        public int currentPulseN = 1;
        public int[] cycle = null;
        public int epoch = 1;

        public int nChannelsPatch = 2;
        public int nChannelsStim = 2;

        public Dictionary<string, PulseParameters> PulseSet = new Dictionary<string, PulseParameters>();

        public string PhysFolderPath;
        public string initFolderPath;
        public string initFilePath;
        public string settingFileBaseName = "setting-";

        public PhysParameters()
        {
            PulseSet.Add("Stim1", new PulseParameters());
            PulseSet.Add("Stim2", new PulseParameters());
            PulseSet.Add("Patch1", new PulseParameters());
            PulseSet.Add("Patch2", new PulseParameters());

            PhysFolderPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FLIMage", "Phys");
            initFolderPath = Path.Combine(PhysFolderPath, "Init_Files");
            initFilePath = Path.Combine(initFolderPath, "phys_setting.txt");

            Directory.CreateDirectory(initFolderPath);

            ReadParameters(initFilePath);
        }

        public void mkPulse(string key, out double[] dataOut, out double[] time)
        {
            int nSamples = (int)(outputRate / 1000.0 * pulseSetTotalLength_ms);
            dataOut = new double[nSamples];
            time = new double[nSamples];
            var pulse = PulseSet[key];

            for (int i = 0; i < nSamples; i++)
                time[i] = i / outputRate * 1000;

            for (int i = 0; i < pulse.Num; i++)
            {
                int pulseS = (int)((pulse.Delay_ms + pulse.Interval_ms * i) / 1000 * outputRate);
                int pulseE = (int)((pulse.Delay_ms + pulse.Interval_ms * i + pulse.Width_ms) / 1000 * outputRate);
                for (int j = pulseS; j < pulseE; j++)
                {
                    if (j < nSamples)
                        dataOut[j] = pulse.Amp;
                }
            }
        }

        public string filePathByNumber(int n)
        {
            var filename = settingFileBaseName + n.ToString() + ".txt";
            var pathname = Path.Combine(initFolderPath, filename);
            return pathname;
        }

        public void SaveParametersByNumber(int n)
        {
            SaveParameters(filePathByNumber(n));
        }

        public void ReadParametersByNumber(int n)
        {
            string fname = filePathByNumber(n);
            if (File.Exists(fname))
                ReadParameters(fname);
            else
            {
                currentPulseN = n;
                SaveParameters(fname);
            }
        }

        public void ReadParameters(string filename)
        {
            if (!File.Exists(filename))
            {
                SaveParameters(filename);
                return;
            }

            string text = File.ReadAllText(filename);
            StringToParam(text);
        }

        public int StringToParam(string text)
        {
            string[] lines = text.Split('\n');
            string pulseSetKey = "";
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Replace("\r", "");
                string[] sP = line.Split('=');

                if (sP.Length >= 2)
                {
                    if (sP[0] == "PulseSet")
                    {
                        pulseSetKey = sP[1];
                        Debug.WriteLine("Pulse Set = " + pulseSetKey + ";");
                    }
                    else
                    {
                        FieldInfo field;
                        object obj;

                        if (pulseSetKey == "")
                        {
                            field = this.GetType().GetField(sP[0]);
                            obj = this;
                        }
                        else
                        {
                            field = PulseSet[pulseSetKey].GetType().GetField(sP[0]);
                            obj = PulseSet[pulseSetKey];
                        }

                        if (field != null)
                        {
                            if (field.FieldType == typeof(int[]))
                            {
                                if (sP[1].Contains("null"))
                                    field.SetValue(obj, null);
                                else
                                {
                                    int[] intVals = Array.ConvertAll(sP[1].Split(','), int.Parse);
                                    field.SetValue(obj, intVals);
                                }
                            }
                            else
                                field.SetValue(obj, Convert.ChangeType(sP[1], field.FieldType));
                        }
                    }
                }
            }
            return 0;
        }

        public void SaveInitParameters()
        {
            SaveParameters(initFilePath);
        }

        public int ParamToString(out string ParamString)
        {
            StringBuilder sb = new StringBuilder();
            var infos = this.GetType().GetFields();
            foreach (var member in infos)
            {
                if (member.Name != "PulseSet" && !member.Name.Contains("Path") && member.Name != "settingFileBaseName")
                {
                    if (member.FieldType == typeof(int[]))
                    {
                        sb.Append(member.Name);
                        sb.Append("=");
                        var values = (int[])member.GetValue(this);
                        if (values != null)
                        {
                            sb.Append(String.Join(",", values));
                        }
                        else
                            sb.Append("null");
                    }
                    else
                    {
                        sb.Append(member.Name);
                        sb.Append("=");
                        sb.Append(member.GetValue(this));

                    }
                    sb.AppendLine();
                }
            }

            foreach (var pulse in PulseSet)
            {
                sb.Append("PulseSet=" + pulse.Key);
                sb.AppendLine();
                infos = pulse.Value.GetType().GetFields();
                foreach (var member in infos)
                {
                    sb.Append(member.Name);
                    sb.Append("=");
                    sb.Append(member.GetValue(pulse.Value));
                    sb.AppendLine();
                }
            }

            ParamString = sb.ToString();
            return 0;
        }

        public void SaveParameters(string filename)
        {
            ParamToString(out string paramText);
            File.WriteAllText(filename, paramText);
        }

        public class PulseParameters
        {
            public int Num = 1;
            public double Width_ms = 6;
            public double Amp = 100;
            public double Interval_ms = 50;
            public double Delay_ms = 10;
        }
    } //class
} //name space