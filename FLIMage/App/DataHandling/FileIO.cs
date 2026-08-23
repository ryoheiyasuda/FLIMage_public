using BitMiracle.LibTiff.Classic;
using FLIMage.FileFormat;
using MathLibrary;
using Microsoft.CSharp;
using PhysiologyCSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Utilities;
using System.Runtime.InteropServices;
using static FLIMage.OmeTiffLibraryWrapper;
using static FLIMage.ScanParameters;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FLIMage
{
    public class ImageParameterCalculation
    {
        static public double[] MirrorOffsetToMicrometers(ScanParameters State)
        {

            return voltage2micrometers_XY(new double[] { State.Acq.XOffset, State.Acq.YOffset }, State);
        }

        static public void MirrorOffsetVoltageFromMicrometers(ScanParameters State, double[] offset_um)
        {
            double[] XY = micrometers2voltage_XY(offset_um, State);
            State.Acq.XOffset = XY[0];
            State.Acq.YOffset = XY[1];
        }

        /// <summary>
        /// If voltave_xy has 3 values, third value will remain.
        /// </summary>
        /// <param name="voltage_xy"></param>
        /// <param name="state1"></param>
        /// <returns></returns>
        public static double[] voltage2micrometers_XY(double[] voltage_xy, ScanParameters state1)
        {
            int switchXY = state1.Acq.switchXYScanToMotor ? -1 : 1;
            int flipX = state1.Acq.flipDirectionOfScanToMotor[0] ? -1 : 1;
            int flipY = state1.Acq.flipDirectionOfScanToMotor[1] ? -1 : 1;

            //mirror setting.
            int switchXY2 = state1.Acq.switchXYScan ? -1 : 1;
            int flipX2 = state1.Acq.flipXYScan[0] ? -1 : 1;
            int flipY2 = state1.Acq.flipXYScan[1] ? -1 : 1;

            flipX = flipX * flipX2;
            flipY = flipY * flipY2;
            switchXY = switchXY * switchXY2;

            //mirror: Switch --> Flip
            double[] xy_um = (double[])voltage_xy.Clone();
            if (switchXY == 1)
            {
                xy_um[0] = flipX * voltage_xy[0] / state1.Acq.XMaxVoltage * state1.Acq.field_of_view[0];
                xy_um[1] = flipY * voltage_xy[1] / state1.Acq.YMaxVoltage * state1.Acq.field_of_view[1];
            }
            else
            {
                xy_um[1] = flipX * voltage_xy[0] / state1.Acq.XMaxVoltage * state1.Acq.field_of_view[0];
                xy_um[0] = flipY * voltage_xy[1] / state1.Acq.YMaxVoltage * state1.Acq.field_of_view[1];
            }

            return xy_um;
        }

        public static double[] micrometers2voltage_XY(double[] xy_um, ScanParameters state1)
        {
            int switchXY = state1.Acq.switchXYScanToMotor ? -1 : 1;
            int flipX = state1.Acq.flipDirectionOfScanToMotor[0] ? -1 : 1;
            int flipY = state1.Acq.flipDirectionOfScanToMotor[1] ? -1 : 1;

            //mirror setting.
            int switchXY2 = state1.Acq.switchXYScan ? -1 : 1;
            int flipX2 = state1.Acq.flipXYScan[0] ? -1 : 1;
            int flipY2 = state1.Acq.flipXYScan[1] ? -1 : 1;

            flipX = flipX * flipX2;
            flipY = flipY * flipY2;
            switchXY = switchXY * switchXY2;

            double[] voltage_xy = (double[])xy_um.Clone();
            if (switchXY == 1)
            {
                voltage_xy[0] = flipX * xy_um[0] * state1.Acq.XMaxVoltage / state1.Acq.field_of_view[0];
                voltage_xy[1] = flipY * xy_um[1] * state1.Acq.YMaxVoltage / state1.Acq.field_of_view[1];
            }
            else
            {
                voltage_xy[1] = flipX * xy_um[0] * state1.Acq.XMaxVoltage / state1.Acq.field_of_view[0];
                voltage_xy[0] = flipY * xy_um[1] * state1.Acq.YMaxVoltage / state1.Acq.field_of_view[1];
            }

            return voltage_xy;
        }

    }

    public class FileIO
    {

        //public List<String> headerList;
        public List<String> headerList_nonDevice;
        public List<String> headerDevice;
        List<String> headerList_all;
        public ScanParameters State;

        public String image_description;
        public int omeHandle = -1;
        private int totalZCount = 0;
        private int currentZ = 0;
        private string omeFileName = "";
        private byte[][] omeChannelBuffers;
        private GCHandle[] omeChannelHandles;
        private IntPtr[] omeChannelPointers;
        private int[] omeChannelBufferBytes;
        private bool omeFrameInfoInitialized = false;
        private static ushort tag_header = 0xea00;
        private static FrameInfo frameInfo;

        int time_elapsed = 0;
        private readonly object _flimWriterLock = new object();
        private readonly Dictionary<string, FlimTiffWriter> _flimWriters = new Dictionary<string, FlimTiffWriter>(StringComparer.OrdinalIgnoreCase);

        public bool HoldFastWriterOpen { get; set; }
        public int ExpectedOmeTiffFrames { get; set; }

        public FileIO(ScanParameters Scan)
        {
            State = Scan;

            headerList_nonDevice = new List<String>();
            //headerList_nonDevice.Add("State.Init");
            headerList_nonDevice.Add("State.Acq");
            headerList_nonDevice.Add("State.Files");
            headerList_nonDevice.Add("State.Display");
            headerList_nonDevice.Add("State.Motor");
            headerList_nonDevice.Add("State.Spc.analysis");
            headerList_nonDevice.Add("State.Spc.datainfo");
            headerList_nonDevice.Add("State.Spc.spcData");
            headerList_nonDevice.Add("State.Uncaging");
            headerList_nonDevice.Add("State.DO");
            headerList_nonDevice.Add("State.Ephys");

            headerDevice = new List<String>();
            headerDevice.Add("State.Init");

            headerList_all = headerDevice.Concat(headerList_nonDevice).ToList();
        }


        object Str2obj(string strA, ScanParameters Scan)
        {
            object obj = Scan.Init;
            if (strA.Contains("Init"))
                obj = Scan.Init;
            else if (strA.Contains("Acq"))
                obj = Scan.Acq;
            else if (strA.Contains("analysis"))
                obj = Scan.Spc.analysis;
            else if (strA.Contains("datainfo"))
                obj = Scan.Spc.datainfo;
            else if (strA.Contains("spcData"))
                obj = Scan.Spc.spcData;
            else if (strA.Contains("Files"))
                obj = Scan.Files;
            else if (strA.Contains("Display"))
                obj = Scan.Display;
            else if (strA.Contains("Motor"))
                obj = Scan.Motor;
            else if (strA.Contains("Uncaging"))
                obj = Scan.Uncaging;
            else if (strA.Contains("DO"))
                obj = Scan.DO;
            else if (strA.Contains("Ephys"))
                obj = Scan.Ephys;
            return obj;
        }

        public PhysParameters CopyFromStateToPhysParameters()
        {
            var phys_param = new PhysParameters();
            //var StringSet = new String[] { "Stim1", "Stim2", "Patch1", "Patch2" };
            String[] StringSet = phys_param.PulseSet.Keys.ToArray();
            FieldInfo[] fields_pulseSet = null;
            String[] pulseSetNames = null;
            if (StringSet != null && StringSet.Length > 0)
            {
                fields_pulseSet = phys_param.PulseSet[StringSet[0]].GetType().GetFields();
                pulseSetNames = fields_pulseSet.Select(x => x.Name).ToArray();
            }

            //Should be "Amp, Delay_ms, Interval_ms, Width_ms.

            var fields = State.Ephys.GetType().GetFields();
            foreach (var field in fields)
            {
                bool[] pulseset_bool = StringSet.Select(x => field.Name.Contains(x)).ToArray();

                if (pulseset_bool.Any(x => x == true) && pulseSetNames != null)
                {
                    int index = Array.IndexOf(pulseset_bool, true);
                    double value = (double)field.GetValue(State.Ephys);
                    var obj = phys_param.PulseSet[StringSet[index]];

                    foreach (var pulseSetName in pulseSetNames)
                    {
                        if (field.Name.EndsWith(pulseSetName))
                        {
                            obj.GetType().GetField(pulseSetName).SetValue(obj, value);
                            break;
                        }
                    }
                }
                else
                {
                    var field_in_phys = phys_param.GetType().GetField(field.Name);

                    if (field_in_phys != null)
                    {
                        var value = field.GetValue(State.Ephys);
                        field_in_phys.SetValue(phys_param, value);
                    }
                }
            }
            return phys_param;

        }

        public ScanParameters CopyPhysiologyParamToState(PhysParameters phys_param)
        {
            if (phys_param == null)
            {
                State.Ephys.Ephys_on = false;
                return State;
            }

            String[] StringSet = phys_param.PulseSet.Keys.ToArray();
            FieldInfo[] fields_pulseSet = null;
            String[] pulseSetNames = null;

            if (StringSet != null && StringSet.Length > 0)
            {
                fields_pulseSet = phys_param.PulseSet[StringSet[0]].GetType().GetFields();
                pulseSetNames = fields_pulseSet.Select(x => x.Name).ToArray();
            }

            var fields = State.Ephys.GetType().GetFields();
            foreach (var field in fields)
            {
                bool[] pulseset_bool = StringSet.Select(x => field.Name.Contains(x)).ToArray();

                if (pulseset_bool.Any(x => x == true) && pulseSetNames != null)
                {
                    int index = Array.IndexOf(pulseset_bool, true);
                    double value = (double)field.GetValue(State.Ephys);
                    var obj = phys_param.PulseSet[StringSet[index]];

                    foreach (var pulseSetName in pulseSetNames)
                    {
                        if (field.Name.EndsWith(pulseSetName))
                        {
                            value = (double)obj.GetType().GetField(pulseSetName).GetValue(obj);
                            field.SetValue(State.Ephys, value);
                            break;
                        }
                    }
                }
                else
                {
                    var field_in_phys = phys_param.GetType().GetField(field.Name);
                    if (field_in_phys != null)
                    {
                        var value = field_in_phys.GetValue(phys_param);
                        field.SetValue(State.Ephys, value);
                    }
                }
            }

            return State;
        }

        public void SaveSetupFile()
        {
            //Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.InitialDirectory = State.Files.initFolderPath;
            saveFileDialog1.FileName = "FLIM_init.txt";
            saveFileDialog1.Filter = "ini files (*.txt, *.ini)|*.txt; *.ini|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = false;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    String fileName = saveFileDialog1.FileName;
                    State.Files.initFileName = fileName;
                    File.WriteAllText(fileName, AllSetupValues_nonDevice());
                    Debug.WriteLine("Writing ini file in...." + fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file to disk. Original error: " + ex.Message);
                }
            }
        }

        public string FindDefaultSetupFile()
        {
            var versionText = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3).Replace(".", "_");

            string prefix = "Default";
            var searchPattern = prefix + "*.txt";
            var filepaths = Directory.GetFiles(State.Files.initFolderPath, searchPattern);

            if (filepaths.Length == 0)
                return State.Files.initFolderPath + Path.DirectorySeparatorChar + prefix + "*.txt";

            var version_numbers = new List<int>();
            foreach (var filepath in filepaths)
            {
                var fname = Path.GetFileNameWithoutExtension(filepath);
                if (fname != prefix)
                {
                    var split1 = fname.Split('-');
                    var version_text = new string[] { "" };

                    if (split1.Length > 1)
                        version_text = split1[1].Split('_');

                    var version_number = new int[] { 0, };
                    try
                    {

                        if (version_text.Length == 3)
                        {
                            version_number = Array.ConvertAll(version_text, int.Parse);
                            int total = version_number[0] * 1000 + version_number[1] * 100 + version_number[2] * 100;
                            version_numbers.Add(total);
                        }
                        else
                        {
                            version_numbers.Add(0);
                        }
                    }
                    catch
                    {
                    }

                }
                else
                {
                    version_numbers.Add(0);
                }
            }

            var current_version_number = Array.ConvertAll(versionText.Split('_'), int.Parse);
            var current_n = current_version_number[0] * 1000 + current_version_number[1] * 100 + current_version_number[2] * 100;

            var sortedIndices = Enumerable.Range(0, version_numbers.Count()).OrderByDescending(i => version_numbers[i]).ToList();

            var finaleFileName = "";
            for (int i = 0; i < version_numbers.Count(); i++)
            {
                int j = sortedIndices[i];
                finaleFileName = filepaths[j];
                if (version_numbers[j] < current_n)
                    break;
            }

            return finaleFileName;
        }

        public void SaveDeviceFile()
        {
            System.IO.File.WriteAllText(State.Files.deviceFileName, AllSetupValues_device());
        }

        public void LoadArray(String FileName, out float[][] Image)
        {
            Image = null;
            using (FileStream stream = new FileStream(Path.Combine(FileName), FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int width = reader.ReadInt32();
                    int height = reader.ReadInt32();
                    Image = MatrixCalc.MatrixCreate2D<float>(height, width);
                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                            Image[y][x] = reader.ReadSingle();
                    reader.Close();
                }
            }
        }

        public void SaveArray(String FileName, Double[][] Image)
        {
            int width = Image[0].Length;
            int height = Image.Length;
            using (FileStream stream = new FileStream(Path.Combine(State.Files.initFolderPath, FileName), FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(width);
                    writer.Write(height);
                    for (int y = 0; y < width; y++)
                        for (int x = 0; x < width; x++)
                            writer.Write(Image[y][x]);
                    writer.Close();
                }
            }
        }

        public String OpenGetSetupFileName()
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = State.Files.initFolderPath;
            openFileDialog1.FileName = "FLIM_init.txt";
            openFileDialog1.Filter = "ini files (*.txt, *.ini)|*.txt; *.ini|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = false;

            String filename = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        State.Files.initFileName = openFileDialog1.FileName;
                        //Debug.WriteLine("FILENAME: ", State.Files.initFileName);
                        filename = State.Files.initFileName;
                        myStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }

            return filename;
        }


        //public void openSetupFile()
        //{
        //    Stream myStream = null;
        //    OpenFileDialog openFileDialog1 = new OpenFileDialog();

        //    openFileDialog1.InitialDirectory = State.Files.initFolderPath;
        //    openFileDialog1.FileName = "FLIM_init.txt";
        //    openFileDialog1.Filter = "ini files (*.txt)|*.txt|All files (*.*)|*.*";
        //    openFileDialog1.FilterIndex = 1;
        //    openFileDialog1.RestoreDirectory = false;


        //    if (openFileDialog1.ShowDialog() == DialogResult.OK)
        //    {
        //        try
        //        {
        //            if ((myStream = openFileDialog1.OpenFile()) != null)
        //            {
        //                State.Files.initFileName = openFileDialog1.FileName;
        //                //Debug.WriteLine("FILENAME: ", State.Files.initFileName);

        //                using (StreamReader sr = new StreamReader(myStream))
        //                {
        //                    string s = "";
        //                    while ((s = sr.ReadLine()) != null)
        //                    {
        //                        //Debug.WriteLine(s);
        //                        executeLine(s);
        //                    }
        //                }
        //                myStream.Close();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
        //        }
        //    }
        //}





        static public bool IsBinary(string filePath, int requiredConsecutiveNul = 1)
        {
            const int charsToCheck = 8000;
            const char nulChar = '\0';

            int nulCount = 0;

            using (var streamReader = new StreamReader(filePath))
            {
                for (var i = 0; i < charsToCheck; i++)
                {
                    if (streamReader.EndOfStream)
                        return false;

                    if ((char)streamReader.Read() == nulChar)
                    {
                        nulCount++;

                        if (nulCount >= requiredConsecutiveNul)
                            return true;
                    }
                    else
                    {
                        nulCount = 0;
                    }
                }
            }

            return false;
        }

        public void LoadSetupFile(String fileName)
        {
            string saveDeviceSettinFileName = State.Files.deviceFileName;
            string saveInitFilePath = State.Files.initFolderPath;
            string saveDefaultInitFile = State.Files.defaultInitFile;
            bool hasResonantMaxVoltage = false;

            File.SetAttributes(State.Files.initFolderPath, FileAttributes.Normal);

            if (File.Exists(fileName))
            {
                using (StreamReader sr = File.OpenText(fileName))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        try
                        {
                            if (!hasResonantMaxVoltage)
                            {
                                var trimmed = s.TrimStart();
                                if (trimmed.StartsWith("State.Acq.XMaxVoltage_Resonant", StringComparison.OrdinalIgnoreCase))
                                    hasResonantMaxVoltage = true;
                            }
                            ExecuteLine(s);
                        }
                        catch
                        {
                            Debug.WriteLine("Problem in " + s);
                        }
                    }
                    sr.Close();
                }
            }

            State.Files.deviceFileName = saveDeviceSettinFileName;
            State.Files.initFolderPath = saveInitFilePath;
            State.Files.defaultInitFile = saveDefaultInitFile;

            if (!hasResonantMaxVoltage)
                State.Acq.XMaxVoltage_Resonant = State.Acq.XMaxVoltage;

            for (int i = State.Init.EOM_nChannels; i < State.Init.imagingLasers.Length; i++)
            {
                State.Init.imagingLasers[i] = false;
                State.Init.uncagingLasers[i] = false;
            }

        }

        public void ExecuteLine(String s)
        {
            //Type datatype = typeof(double);
            ExecuteLine(s, true);
        }

        public String ExecuteLine(String s, bool overwrite)
        {
            String valStr = "";

            //s = Regex.Replace(s, @"\s+", "");
            s = s.Replace("\n", "");

            foreach (String strA in headerList_all)
            {
                if (s.StartsWith(strA, true, null))
                {
                    string fieldName;

                    object obj = Str2obj(strA, State);
                    Type type = obj.GetType();

                    if (overwrite)
                    {
                        string[] sP = s.Split('=');
                        string valueStr = sP[1];
                        fieldName = sP[0].Substring(strA.Length + 1);
                        fieldName = Regex.Replace(fieldName, @"\s+", "");

                        SetGlobalValues(type, fieldName, valueStr, obj);
                    }
                    else
                    {
                        if (s.Contains("="))
                        {
                            string[] sP = s.Split('=');
                            s = sP[0];
                        }

                        fieldName = s.Substring(strA.Length + 1);
                        fieldName = Regex.Replace(fieldName, @"\s+", "");
                    }

                    FieldInfo member = type.GetField(fieldName);

                    if (member != null)
                        valStr = ConvertFieldToStringEach(member, strA, "[", "]", obj);
                }
            }
            return valStr;
        }

        public String FLIM_FilePath(int ch, bool ChannelsInSeparatedFile, int counter, ImageType image_type, string ProjectionTypeString, string dirPath, string basename, string extension)
        {
            if (ch >= State.Acq.nChannels || ch < 0)
                ch = 0;

            String fileNameWithoutPath;

            if (ChannelsInSeparatedFile)
            {
                if (State.Files.numberedFile && counter != 0)
                    fileNameWithoutPath = String.Format("{0}_Ch{1}_{2:000}{3}{4}", basename, ch + 1, counter, ProjectionTypeString, extension);
                else
                    fileNameWithoutPath = String.Format("{0}_Ch{1}{2}{3}", basename, ch + 1, ProjectionTypeString, extension);
            }
            else
            {
                if (State.Files.numberedFile && counter != 0)
                    fileNameWithoutPath = String.Format("{0}{1:000}{2}{3}", basename, counter, ProjectionTypeString, extension);
                else
                    fileNameWithoutPath = String.Format("{0}{1}{2}", basename, ProjectionTypeString, extension);
            }
            String folderPath = "";
            if (image_type == ImageType.FLIM_color)
                folderPath = dirPath + Path.DirectorySeparatorChar + "FLIM";
            else if (image_type == ImageType.Intensity)
                folderPath = dirPath + Path.DirectorySeparatorChar + "Intensity";
            else if (image_type == ImageType.FLIMRaw)
                folderPath = dirPath;


            String fileName = folderPath + Path.DirectorySeparatorChar + fileNameWithoutPath;
            return fileName;
        }

        public void SetGlobalValues(Type type, String fieldName, String valueStr, object obj)
        {
            //Type datatype = typeof(double);
            SetGetGlobalValues(type, fieldName, valueStr, obj, true);
        }

        public object SetGetGlobalValues(Type type, String fieldName, String valueStr, object obj, bool overwrite)
        {
            object valobj = null;
            Type datatype = null;
            //Type datatype;
            FieldInfo member;
            member = type.GetField(fieldName);
            //Debug.WriteLine(fieldName + "=" + valueStr + "----");
            if (member != null)
            {
                datatype = member.FieldType;
                string valueStrRaw = valueStr;

                if (!datatype.Equals(typeof(String)))
                {
                    valueStr = Regex.Replace(valueStr, @"\s+", "");
                    valueStr = valueStr.Replace("\"", "").Replace(";", "").Replace("{", "").Replace("}", "");
                    valueStr = valueStr.Replace("[", "").Replace("]", "");
                }
                //Debug.WriteLine(fieldName + "=" + valueStr + "----");

                if (datatype.Equals(typeof(Double[])))
                {
                    // Important: some legacy headers/settings write empty values for arrays (e.g. "field=").
                    // In that case we should KEEP the existing default array, not overwrite with Array.Empty<>,
                    // otherwise downstream code may crash when indexing per-channel arrays.
                    string rawCompact = Regex.Replace(valueStrRaw ?? "", @"\s+", "");
                    rawCompact = rawCompact.Replace("\"", "").Replace(";", "").Replace("{", "").Replace("}", "");
                    bool explicitEmptyArray = rawCompact == "[]";
                    if (string.IsNullOrWhiteSpace(valueStr))
                    {
                        if (!explicitEmptyArray)
                        {
                            valobj = member.GetValue(obj);
                            return valobj;
                        }

                        double[] emptyValues = Array.Empty<double>();
                        if (overwrite)
                            member.SetValue(obj, emptyValues);
                        return emptyValues;
                    }

                    String[] valueArray = valueStr.Split(',');
                    Double[] valArray = (Double[])member.GetValue(obj);
                    if (valueArray.Length == 0)
                    {
                        valueArray = new string[1];
                        valueArray[0] = valueStr;
                    }

                    int arrayL = valueArray.Length;
                    if (valArray != null)
                        arrayL = Math.Max(valArray.Length, valueArray.Length);

                    double[] values = new double[arrayL];
                    for (int i = 0; i < valueArray.Length; i++)
                    {
                        values[i] = Convert.ToDouble(valueArray[i]);
                    }
                    if (overwrite)
                        member.SetValue(obj, values);

                    valobj = (object)values;
                }
                else if (datatype.Equals(typeof(Int32[])))
                {
                    string rawCompact = Regex.Replace(valueStrRaw ?? "", @"\s+", "");
                    rawCompact = rawCompact.Replace("\"", "").Replace(";", "").Replace("{", "").Replace("}", "");
                    bool explicitEmptyArray = rawCompact == "[]";
                    if (string.IsNullOrWhiteSpace(valueStr))
                    {
                        if (!explicitEmptyArray)
                        {
                            valobj = member.GetValue(obj);
                            return valobj;
                        }

                        Int32[] emptyValues = Array.Empty<Int32>();
                        if (overwrite)
                            member.SetValue(obj, emptyValues);
                        return emptyValues;
                    }

                    String[] valueArray = valueStr.Split(',');
                    Int32[] valArray = (Int32[])member.GetValue(obj);
                    if (valueArray.Length == 0)
                    {
                        valueArray = new string[1];
                        valueArray[0] = valueStr;
                    }

                    Int32[] values;
                    int arrayL = (valArray != null) ? Math.Max(valArray.Length, valueArray.Length) : valueArray.Length;
                        values = new Int32[arrayL];
                        for (int i = 0; i < valueArray.Length; i++)
                            values[i] = Convert.ToInt32(valueArray[i]);
                    if (overwrite)
                        member.SetValue(obj, values);

                    valobj = (object)values;
                }
                else if (datatype.Equals(typeof(UInt32[])))
                {
                    string rawCompact = Regex.Replace(valueStrRaw ?? "", @"\s+", "");
                    rawCompact = rawCompact.Replace("\"", "").Replace(";", "").Replace("{", "").Replace("}", "");
                    bool explicitEmptyArray = rawCompact == "[]";
                    if (string.IsNullOrWhiteSpace(valueStr))
                    {
                        if (!explicitEmptyArray)
                        {
                            valobj = member.GetValue(obj);
                            return valobj;
                        }

                        UInt32[] emptyValues = Array.Empty<UInt32>();
                        if (overwrite)
                            member.SetValue(obj, emptyValues);
                        return emptyValues;
                    }

                    String[] valueArray = valueStr.Split(',');
                    UInt32[] valArray = (UInt32[])member.GetValue(obj);
                    if (valueArray.Length == 0)
                    {
                        valueArray = new string[1];
                        valueArray[0] = valueStr;
                    }

                    int arrayL = (valArray != null) ? Math.Max(valArray.Length, valueArray.Length) : valueArray.Length;
                    UInt32[] values = new UInt32[arrayL];
                    for (int i = 0; i < valueArray.Length; i++)
                    {
                        values[i] = Convert.ToUInt32(valueArray[i]);
                    }
                    if (overwrite)
                        member.SetValue(obj, values);

                    valobj = (object)values;
                }
                else if (datatype.Equals(typeof(bool[])))
                {
                    string rawCompact = Regex.Replace(valueStrRaw ?? "", @"\s+", "");
                    rawCompact = rawCompact.Replace("\"", "").Replace(";", "").Replace("{", "").Replace("}", "");
                    bool explicitEmptyArray = rawCompact == "[]";
                    if (string.IsNullOrWhiteSpace(valueStr))
                    {
                        if (!explicitEmptyArray)
                        {
                            valobj = member.GetValue(obj);
                            return valobj;
                        }

                        bool[] emptyValues = Array.Empty<bool>();
                        if (overwrite)
                            member.SetValue(obj, emptyValues);
                        return emptyValues;
                    }

                    String[] valueArray = valueStr.Split(',');
                    bool[] valArray = (bool[])member.GetValue(obj);
                    if (valueArray.Length == 0)
                    {
                        valueArray = new string[1];
                        valueArray[0] = valueStr;
                    }
                    int arrayL = (valArray != null) ? Math.Max(valArray.Length, valueArray.Length) : valueArray.Length;
                    bool[] values = new bool[arrayL];
                    for (int i = 0; i < valueArray.Length; i++)
                    {
                        values[i] = Convert.ToBoolean(valueArray[i]);
                    }
                    if (overwrite)
                        member.SetValue(obj, values);

                    valobj = (object)values;
                }
                else if (datatype.Equals(typeof(String)))
                {
                    string[] valStrs = valueStr.Split('\"');
                    valueStr = valStrs[1];

                    if (overwrite)
                        member.SetValue(obj, valueStr);

                    valobj = (object)valueStr;

                    //Remove ""
                    //Debug.WriteLine(fieldName + "=" + member.GetValue(null).ToString());
                }
                else
                {
                    var value = Convert.ChangeType(valueStr, datatype);
                    if (overwrite)
                        member.SetValue(obj, value);

                    valobj = (object)value;
                }
            }

            return valobj;

        }


        public void ExecuteString(String formula)
        {
            double result;
            try
            {
                CompilerParameters compilerParameters = new CompilerParameters
                {
                    GenerateInMemory = true,
                    TreatWarningsAsErrors = false,
                    GenerateExecutable = false,
                };

                string[] referencedAssemblies = { "System.dll" };
                compilerParameters.ReferencedAssemblies.AddRange(referencedAssemblies);

                const string codeTemplate = "using System;namespace FLIMimage{{public class Dynamic {{static public void Calculate(){{  {0}   }} }} }}";
                string code = string.Format(codeTemplate, formula);

                Debug.WriteLine(code);

                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerResults compilerResults = provider.CompileAssemblyFromSource(compilerParameters, new string[] { code });
                if (compilerResults.Errors.HasErrors)
                    throw new Exception();

                Module module = compilerResults.CompiledAssembly.GetModules()[0];
                Type type = module.GetType("Dynamic");
                MethodInfo method = type.GetMethod("Calculate");

                result = (double)(method.Invoke(null, null));
            }
            catch (Exception)
            { }
        }

        public String ConvertFieldToStringEach(FieldInfo memberInfo, String parentStr, String arrayStart, String arrayEnd, object obj)
        {
            var value = memberInfo.GetValue(obj);
            Type type = memberInfo.FieldType;
            String typeStr = type.ToString();

            if (value == null)
            {
                // For arrays, emit an explicit empty array so the header stays parseable.
                if (type.Equals(typeof(Double[])) || type.Equals(typeof(Int32[])) || type.Equals(typeof(bool[])))
                    return arrayStart + arrayEnd;

                return "";
            }

            String strVal = value.ToString();

            if (type.Equals(typeof(String)))
            {
                strVal = "\"" + strVal + "\"";
            }
            else if (type.Equals(typeof(Double[])))
            {
                strVal = arrayStart;
                Double[] valArray = (Double[])memberInfo.GetValue(obj);
                if (valArray.Length == 0)
                    return arrayStart + arrayEnd;
                for (int i = 0; i < valArray.Length; i++)
                {
                    strVal = strVal + valArray[i].ToString();
                    if (i < valArray.Length - 1)
                        strVal = strVal + ", ";
                    else
                        strVal = strVal + arrayEnd;
                }
            }
            else if (type.Equals(typeof(Int32[])))
            {
                strVal = arrayStart;
                Int32[] valArray = (Int32[])memberInfo.GetValue(obj);
                if (valArray.Length == 0)
                    return arrayStart + arrayEnd;
                for (int i = 0; i < valArray.Length; i++)
                {
                    strVal = strVal + valArray[i].ToString();
                    if (i < valArray.Length - 1)
                        strVal = strVal + ", ";
                    else
                        strVal = strVal + arrayEnd;
                }
            }
            else if (type.Equals(typeof(bool[])))
            {
                strVal = arrayStart;
                bool[] valArray = (bool[])memberInfo.GetValue(obj);
                if (valArray.Length == 0)
                    return arrayStart + arrayEnd;
                for (int i = 0; i < valArray.Length; i++)
                {
                    strVal = strVal + valArray[i].ToString();
                    if (i < valArray.Length - 1)
                        strVal = strVal + ", ";
                    else
                        strVal = strVal + arrayEnd;
                }
            }

            return strVal;

        }

        public String ConvertFieldToString(FieldInfo[] members, String parentStr, String arrayStart, String arrayEnd, object obj)
        {
            String str1 = "";
            String strVal;
            //obj = null;
            foreach (FieldInfo memberInfo in members)
            {
                strVal = ConvertFieldToStringEach(memberInfo, parentStr, arrayStart, arrayEnd, obj);
                str1 = str1 + String.Format("{0}.{1} = {2};\r\n", parentStr, memberInfo.Name, strVal); // Name: MyField

                //Debug.WriteLine("Member Type: {0}", memberInfo.MemberType); // Member Type: Property}
            }
            return str1;
        }

        public String AllSetupFile()
        {
            return SelectedSetupValues(headerList_all);
        }

        public String AllSetupValues_device()
        {
            return SelectedSetupValues(headerDevice);
        }

        public String AllSetupValues_nonDevice()
        {
            return SelectedSetupValues(headerList_nonDevice);

            //FieldInfo[] members;
            //Type type = typeof(ScanParameters.Initialize);

            //String str1 = "FLIMimage parameters\r\n";
            //for (int i = 0; i < headerList_nonDevice.Count; i++)
            //{
            //    String strA = headerList_nonDevice[i];
            //    //    type = typeList2[i];
            //    object obj = Str2obj(strA, State);

            //    type = obj.GetType();
            //    members = type.GetFields();
            //    str1 = str1 + ConvertFieldToString(members, strA, "[", "]", obj);
            //}
            //return str1;
        }

        public String SelectedSetupValues(List<String> stringList)
        {
            FieldInfo[] members;
            Type type = typeof(ScanParameters.Initialize);

            String str1 = "FLIMimage parameters\r\n";
            for (int i = 0; i < stringList.Count; i++)
            {
                String strA = stringList[i];
                object obj = Str2obj(strA, State);

                type = obj.GetType();
                members = type.GetFields();
                str1 = str1 + ConvertFieldToString(members, strA, "[", "]", obj);
            }
            return str1;
        }

        public ScanParameters CopyState()
        {
            List<String> copyList = new List<String>();
            copyList.Add("State.Init");
            copyList.Add("State.Acq");
            copyList.Add("State.Files");
            copyList.Add("State.Display");
            copyList.Add("State.Motor");
            copyList.Add("State.Spc.analysis");
            copyList.Add("State.Spc.datainfo");
            copyList.Add("State.Spc.spcData");
            copyList.Add("State.Uncaging");
            return CopyState(copyList);
        }

        public ScanParameters CopyState(String copyStr)
        {
            List<string> stringList = new List<string>();
            stringList.Add(copyStr);
            return CopyState(stringList);
        }


        public ScanParameters CopyState(List<String> stringList)
        {
            //FieldInfo[] members; members2;
            Type type = typeof(ScanParameters.Initialize);
            Type type2 = typeof(ScanParameters.Initialize);
            ScanParameters StateNew = new ScanParameters();

            for (int i = 0; i < stringList.Count; i++)
            {
                String strA = stringList[i];
                object obj = Str2obj(strA, State);

                type = obj.GetType();
                FieldInfo[] members = type.GetFields();

                object obj2 = Str2obj(strA, StateNew);

                Copier.DeepCopyClass(obj, obj2);
            }

            return StateNew;
        }

        public ScanParameters CopyState_Old(List<String> stringList)
        {
            String s = SelectedSetupValues(stringList);

            ScanParameters State = new ScanParameters();
            String[] headerstr = s.Split('\r');
            foreach (String s1 in headerstr)
                ExecuteLine(s1);

            return State;
        }

        public String CreateHeader(bool[] saveChannels)
        {
            String str1 = AllSetupValues_nonDevice();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.Append("SaveChannels = [");
            for (int i = 0; i < saveChannels.Length; i++)
            {
                sb.Append(saveChannels[i].ToString());
                if (i != saveChannels.Length - 1)
                    sb.Append(", ");
            }
            sb.Append("];");
            sb.AppendLine();

            str1 = str1 + sb.ToString();
            return str1;
        }


        public static FileError OpenImageFileDialog(String defaultPath, out String fileName)
        {
            FileError file_error = FileError.Success;
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = defaultPath; // State.Files.pathName;
            openFileDialog1.FileName = "FLIM.flim";
            openFileDialog1.Filter = "FLIM files (*.flim, *.tif, *.ptu, *.phtn, *.photon, *.flim2, *.btf)|*.flim; *.tif; *.ptu; *.phtn; *.photon; *.flim2; *.btf|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            fileName = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                try
                {
                    if (openFileDialog1.FileName.EndsWith(".photon"))
                    {
                        fileName = openFileDialog1.FileName;
                        if (File.Exists(fileName))
                            file_error = FileError.TextFile;
                        else
                            file_error = FileError.NotFound;
                    }
                    else if (openFileDialog1.FileName.EndsWith(".btf", StringComparison.OrdinalIgnoreCase))
                    {
                        fileName = openFileDialog1.FileName;
                        if (File.Exists(fileName))
                            file_error = FileError.Success;
                        else
                            file_error = FileError.NotFound;
                    }
                    else if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        fileName = openFileDialog1.FileName;
                        myStream.Close();

                        if (File.Exists(fileName))
                            file_error = FileError.Success;
                        else
                            file_error = FileError.NotFound;
                        //OpenFLIMTiff(fileName);
                    }
                }
                catch (Exception ex)
                {
                    if (!File.Exists(fileName))
                    {
                        MessageBox.Show("Error: File Does not exist: " + fileName + ":" + ex.Message);
                        file_error = FileError.NotFound;
                    }
                    else
                    {
                        MessageBox.Show("Error: Could not read file from disk. Original error: " + fileName + ":" + ex.Message);
                        file_error = FileError.UnKnown;
                    }
                }
            }
            else
            {
                file_error = FileError.Canceled;
            }

            if (file_error == FileError.Success)
            {
                if (!FileIO.IsBinary(fileName))
                {
                    file_error = FileError.TextFile;
                }
            }

            return file_error;
        }



        public static FileError SetupFLIMOpeningDlog(String defaultPath, out int nPages, out String fileName)
        {
            nPages = 0;
            string Header = "";
            FileError file_error = OpenImageFileDialog(defaultPath, out fileName);

            if (file_error == FileError.Success)
            {
                nPages = SetupFLIMOpening(fileName, out Header);
            }
            return file_error;
        }

        public static int SetupFLIMOpening(String fileName, out String Header)
        {
            int nPages = 0;
            string extension = Path.GetExtension(fileName);
            if (extension.Equals(".ptu", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return PtuFileReader.SetupOpening(fileName, out Header);
                }
                catch
                {
                    Header = "";
                    return -1;
                }
            }
            else if (extension.Equals(".btf", StringComparison.OrdinalIgnoreCase))
            {
                GetOMEHeaderTag(fileName, tag_header, out Header);
                nPages = GetOmePageCount(fileName);
            }
            else
            {
                using (Tiff image = Tiff.Open(fileName, "r"))
                {
                    if (image == null)
                    {
                        MessageBox.Show("Could not open this image");
                        Header = "";
                        return -1;
                    }

                    int stride = image.ScanlineSize();
                    byte[] scanline = new byte[stride];
                    FieldValue[] value;


                    value = image.GetField(TiffTag.IMAGEDESCRIPTION);
                    if (value != null)
                        Header = value[0].ToString();
                    else
                        Header = "";

                    //Debug.WriteLine(Description); //For Debug.

                    nPages = image.NumberOfDirectories();
                }
            }


            return nPages;

        }

        private static bool TryGetOmeRegion(int handle, out PlateInfo plate, out WellInfo well, out ScanInfo scan, out ScanRegionInfo region)
        {
            plate = default;
            well = default;
            scan = default;
            region = default;

            if (handle < 0)
                return false;

            var plateCount = ome_get_plates_num(handle);
            if (plateCount <= 0)
                return false;

            PlateInfo[] plates = new PlateInfo[plateCount];
            var ret = ome_get_plates(handle, plates);
            if (ret < 0)
                return false;
            plate = plates[0];

            var wellCount = ome_get_wells_num(handle, plate.Id);
            if (wellCount <= 0)
                return false;
            WellInfo[] wells = new WellInfo[wellCount];
            ret = ome_get_wells(handle, plate.Id, wells);
            if (ret < 0)
                return false;
            well = wells[0];

            var scanCount = ome_get_scans_num(handle, plate.Id);
            if (scanCount <= 0)
                return false;
            ScanInfo[] scans = new ScanInfo[scanCount];
            ret = ome_get_scans(handle, plate.Id, scans);
            if (ret < 0)
                return false;
            scan = scans[0];

            var regionCount = ome_get_scan_regions_num(handle, plate.Id, scan.Id, well.Id);
            if (regionCount <= 0)
                return false;
            ScanRegionInfo[] regions = new ScanRegionInfo[regionCount];
            ret = ome_get_scan_regions(handle, plate.Id, scan.Id, well.Id, regions);
            if (ret < 0)
                return false;
            region = regions[0];

            return true;
        }

        private static int GetOmePageCount(string fileName)
        {
            int handle = ome_open_file(fileName, OpenMode.READ_ONLY_MODE);
            if (handle < 0)
                return 0;

            try
            {
                if (!TryGetOmeRegion(handle, out _, out _, out _, out ScanRegionInfo region))
                    return 0;

                int zCount = (int)region.PixelSizeZ;
                int tCount = (int)region.SizeT;
                if (zCount < 1)
                    zCount = 1;
                if (tCount < 1)
                    tCount = 1;

                long metadataPages = (long)zCount * tCount;
                if (metadataPages > int.MaxValue)
                    metadataPages = int.MaxValue;

                int existingPages = GetOmeExistingPageCount(handle, zCount, tCount);
                if (existingPages > 0 && existingPages <= metadataPages)
                    return existingPages;

                return (int)metadataPages;
            }
            finally
            {
                ome_close_file(handle);
            }
        }

        private static int GetOmeExistingPageCount(int handle, int zCount, int tCount)
        {
            if (handle < 0 || zCount < 1 || tCount < 1)
                return 0;

            if (!TryGetOmeRegion(handle, out PlateInfo plate, out _, out ScanInfo scan, out _))
                return 0;

            uint channelCount = ome_get_channels_num(handle, plate.Id, scan.Id);
            if (channelCount == 0)
                return 0;

            ChannelInfo[] channels = new ChannelInfo[channelCount];
            int ret = ome_get_channels(handle, plate.Id, scan.Id, channels);
            if (ret < 0 || channels.Length == 0)
                return 0;

            uint channelId = channels[0].Id;

            if (InitFrameInfo(handle) < 0)
                return 0;

            long maxPages = (long)zCount * tCount;
            if (maxPages > int.MaxValue)
                maxPages = int.MaxValue;

            int maxPageIndex = (int)maxPages - 1;
            if (maxPageIndex < 0)
                return 0;

            int low = 0;
            int high = maxPageIndex;
            int lastValid = -1;

            while (low <= high)
            {
                int mid = low + ((high - low) / 2);
                if (TryOmePageExists(handle, channelId, zCount, mid))
                {
                    lastValid = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return lastValid + 1;
        }

        private static bool TryOmePageExists(int handle, uint channelId, int zCount, int pageIndex)
        {
            if (pageIndex < 0)
                return false;

            int zIndex = 0;
            int tIndex = 0;
            if (zCount > 1)
            {
                tIndex = pageIndex / zCount;
                zIndex = pageIndex % zCount;
            }
            else
            {
                tIndex = pageIndex;
            }

            FrameInfo curFrame = frameInfo;
            curFrame.c_id = channelId;
            curFrame.z_id = (uint)zIndex;
            curFrame.t_id = (uint)tIndex;

            TiffTagDataType tagType = 0;
            uint tagCount = 0;
            int ret = ome_get_tag(handle, curFrame, (ushort)TiffTag.IMAGEWIDTH, ref tagType, ref tagCount, IntPtr.Zero);
            return ret >= 0;
        }

        public static FileError OpenOMETiffFilePage(String fileName, long read_page, int into_page, FLIMData FLIM, bool newFile, bool SavePagesInMemory)
        {
            double[] save_currentOffset = (double[])FLIM.offset.Clone();

            if (String.Compare(fileName, "") == 0)
                return FileError.NotFound;

            DateTime dt = FLIM.acquiredTime;
            if (read_page == 0 && newFile)
            {
                if (GetOMEHeaderTag(fileName, tag_header, out string headerText) == 0
                    && !string.IsNullOrWhiteSpace(headerText))
                {
                    FLIM.decodeHeader(headerText, fileName);
                    dt = FLIM.acquiredTime;
                }

                FLIM.InitializeData(FLIM.State, true);
            }

            int height = FLIM.height;
            int nChannels = FLIM.nChannels;
            int width = FLIM.width;
            int[] n_time = FLIM.n_time;

            if (height < 1 || nChannels < 0)
                return FileError.FormatError;

            var imgZ = new ushort[nChannels][];
            int depth = 2;

            var handle = ome_open_file(fileName, OpenMode.READ_ONLY_MODE);
            if (handle < 0)
                return FileError.FormatError;

            int zCount = 1;
            int tCount = 1;
            if (TryGetOmeRegion(handle, out _, out _, out _, out ScanRegionInfo regionInfo))
            {
                zCount = (int)regionInfo.PixelSizeZ;
                tCount = (int)regionInfo.SizeT;
            }
            if (zCount < 1)
                zCount = 1;
            if (tCount < 1)
                tCount = 1;

            int totalPages = GetOmeExistingPageCount(handle, zCount, tCount);
            if (totalPages <= 0)
                totalPages = zCount * tCount;
            if (read_page < 0 || read_page >= totalPages)
            {
                ome_close_file(handle);
                return FileError.FormatError;
            }

            if (InitFrameInfo(handle) < 0)
            {
                ome_close_file(handle);
                return FileError.FormatError;
            }

            long zIndexLong = 0;
            long tIndexLong = 0;
            if (zCount > 1)
            {
                tIndexLong = read_page / zCount;
                zIndexLong = read_page % zCount;
            }
            else
            {
                tIndexLong = read_page;
            }
            if (tIndexLong > int.MaxValue || zIndexLong > int.MaxValue)
            {
                ome_close_file(handle);
                return FileError.FormatError;
            }
            int zIndex = (int)zIndexLong;
            int tIndex = (int)tIndexLong;

            if (!(read_page == 0 || newFile))
            {
                var ret = GetOMETag(handle, zIndex, 0, (ushort)TiffTag.EXIF_DATETIMEDIGITIZED, TiffTagDataType.TIFF_ASCII, out string dateString);
                if (ret == 0)
                    dt = DateTime.ParseExact(dateString, "yyyy-MM-ddTHH:mm:ss.fff", null);
            }

            OmeRect omeRect = new OmeRect()
            {
                x = 0,
                y = 0,
                width = (uint)width,
                height = (uint)height,
            };

            for (int i = 0; i < nChannels; i++)
            {
                if (n_time[i] > 0)
                    imgZ[i] = new ushort[height * width * n_time[i]];
                else
                    imgZ[i] = null;
            }

            for (uint i = 0; i < nChannels; i++)
            {
                if (n_time[i] <= 0)
                    continue;

                FrameInfo curFrame = frameInfo;
                curFrame.c_id = i;
                curFrame.z_id = (uint)zIndex;
                curFrame.t_id = (uint)tIndex;

                int bufferBytes = height * width * n_time[i] * depth;
                byte[] imageBuffer = new byte[bufferBytes];
                int status = ome_get_raw_data(handle, curFrame, omeRect, imageBuffer, (uint)(width * n_time[i] * depth));
                if (status != 0)
                {
                    ome_close_file(handle);
                    return FileError.UnKnown;
                }

                Buffer.BlockCopy(imageBuffer, 0, imgZ[i], 0, bufferBytes);
            }

            ome_close_file(handle);

            if (newFile)
            {
                FLIM.clearMemory();
                FLIM.n_pages = totalPages;
            }

            FLIM.KeepPagesInMemory = SavePagesInMemory;

            for (int ch = 0; ch < nChannels; ch++)
                if (n_time[ch] == 0)
                {
                    imgZ[ch] = null;
                }

            FLIM.imagesPerFile = 1;
            if (SavePagesInMemory)
            {
                FLIM.PutToPage_Linear(imgZ, dt, into_page);
            }
            else
            {
                var flim4d = new ushort[imgZ.Length][,,];
                for (int ch = 0; ch < imgZ.Length; ch++)
                {
                    if (imgZ[ch] != null)
                        flim4d[ch] = (ushort[,,])MatrixCalc.Reshape(imgZ[ch], new int[] { height, width, n_time[ch] });
                }

                FLIM.LoadFLIMRawFromData4D(flim4d, dt, false);
            }

            String fName = Path.GetFileName(fileName);
            FileParts(fileName, out String filePath, out String fileBaseName, out int fileNum);
            FLIM.State.Files.pathName = filePath;

            for (int i = 0; i < FLIM.offset.Length; i++)
                if (i < save_currentOffset.Length)
                    FLIM.offset[i] = save_currentOffset[i];
            FLIM.State.Spc.analysis.offset = (double[])FLIM.offset.Clone();

            if (newFile)
            {
                FLIM.pathName = filePath;

                if (fileNum >= 0)
                {
                    FLIM.baseName = fileBaseName;
                    FLIM.fileCounter = fileNum;
                    FLIM.numberedFile = true;
                    FLIM.fileName = fName;
                    FLIM.fileExtension = Path.GetExtension(fName);
                    FLIM.State.Files.extension = FLIM.fileExtension;
                    FLIM.State.Files.useOmeTiff = true;
                    FLIM.State.Files.extension_ome = FLIM.fileExtension;
                    FLIM.fullFileName = Path.Combine(filePath, FLIM.fileName);
                }
                else
                {
                    FLIM.baseName = fileName;
                    FLIM.fileCounter = 0;
                    FLIM.fileName = fName;
                    FLIM.numberedFile = false;
                    FLIM.fullFileName = fileName;
                    FLIM.fileExtension = Path.GetExtension(fName);
                    FLIM.State.Files.extension = FLIM.fileExtension;
                    FLIM.State.Files.useOmeTiff = true;
                    FLIM.State.Files.extension_ome = FLIM.fileExtension;
                }
            }

            return FileIO.FileError.Success;
        }

        /// <summary>
        /// Open FLIM file (it is a 1D-tiff format, but usually '.flim').
        /// ROI information will be preserved. If you want to delete, FLIM.ROIs.Clear() is required.
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="read_page">Page number of TIF file.</param>
        /// <param name="into_page">Open file into this page number </param>
        /// <param name="FLIM">FLIMData file format.</param>
        /// <param name="newFile">If new file, FLIM is initialized.</param>
        /// <param name="SavePagesInMemory">usually true.... but if file is extremely large, 'false' may make sense</param>
        /// <returns></returns>
        public static FileError OpenFLIMTiffFilePage(String fileName, long read_page, int into_page, FLIMData FLIM, bool newFile, bool SavePagesInMemory)
        {
            double[] save_currentOffset = (double[])FLIM.offset.Clone();

            if (String.Compare(fileName, "") == 0)
            {
                return FileError.NotFound;
            }

            string extension = Path.GetExtension(fileName);
            if (extension.Equals(".ptu", StringComparison.OrdinalIgnoreCase))
                return PtuFileReader.OpenPage(fileName, read_page, into_page, FLIM, newFile, SavePagesInMemory);
            if (extension.Equals(".btf", StringComparison.OrdinalIgnoreCase))
                return OpenOMETiffFilePage(fileName, read_page, into_page, FLIM, newFile, SavePagesInMemory);

            using (Tiff image = Tiff.Open(fileName, "r"))
            {
                if (image == null)
                {
                    //MessageBox.Show("Could not open this image");
                    return FileError.UnKnown;
                }

                int nPages = image.NumberOfDirectories();

                if (read_page < nPages)
                {
                    if (read_page > short.MaxValue)
                        return FileError.FormatError;
                    image.SetDirectory((short)read_page);
                }

                int stride = image.ScanlineSize();
                byte[] scanline = new byte[stride];
                FieldValue[] value;

                Compression compression = (Compression)image.GetField(TiffTag.COMPRESSION)[0].ToInt();
                value = image.GetField(TiffTag.IMAGEDESCRIPTION);
                String Description = "";
                if (value != null)
                    Description = value[0].ToString();
                //Debug.WriteLine(Description); //For Debug.

                value = image.GetField(TiffTag.IMAGEWIDTH);
                int widthAll = value[0].ToInt(); //Everything else;

                value = image.GetField(TiffTag.IMAGELENGTH);
                int image_length = value[0].ToInt();

                value = image.GetField(TiffTag.BITSPERSAMPLE);
                int depth = value[0].ToInt() / 8;

                //FLIM.clearPages();
                //FLIM.acquiredTime = new DateTime();

                if (compression == Compression.LZW || compression == Compression.PACKBITS || compression == Compression.NONE)
                {
                    // LZW and PackBits compression schemes do not allow 
                    // scanlines to be read in a random fashion. 
                    // So, we need to read all scanlines from start of the image. 

                    value = image.GetField(TiffTag.IMAGEDESCRIPTION);

                    DateTime dt = new DateTime();
                    if (value != null)
                    {
                        Description = value[0].ToString();
                        if (read_page == 0 || newFile)
                        {
                            FLIM.State = new ScanParameters();
                            FLIM.decodeHeader(Description, fileName);
                            dt = FLIM.acquiredTime;

                            //FLIM.ResetLifetimeCalculation(true);
                        }
                        else
                            dt = FLIM.decodeAcquiredTimeOnly(Description);
                    }

                    int height = FLIM.height;
                    int nChannels = FLIM.nChannels;
                    int width = FLIM.width;
                    int nfastZ = FLIM.nFastZ;

                    FileFormat fm = FLIM.format;

                    if (fm == FileFormat.None)
                    {
                        if (widthAll == width && image_length == height)
                            fm = FileFormat.None;
                        else if (widthAll == FLIM.n_time.Sum() && image_length == width * height)
                            fm = FileFormat.ChTime_YX;
                        else if (image_length == FLIM.n_time[0] && widthAll == width * height * nChannels) //This can happen only if all channels have the same t.
                            fm = FileFormat.ChYX_Time;
                        else if (image_length == width * height * nChannels && widthAll == FLIM.n_time[0]) //This can happen only if all channels have the same t.
                            fm = FileFormat.Time_ChYX;
                        else if (image_length == 1)
                            fm = FileFormat.Linear;
                        else if (image_length == nfastZ)
                            fm = FileFormat.ZLinear;
                        else
                        {
                            MessageBox.Show("This file is not FLIMage file.");
                            return FileError.FormatError;
                        }
                    }

                    int[] n_time = FLIM.n_time;

                    if (fm == FileFormat.None)
                    {
                        for (int i = 0; i < nChannels; i++)
                            n_time[i] = FLIM.saveChannels[i] ? 1 : 0;

                        FLIM.State.Spc.spcData.n_dataPoint = 1;
                    }

                    if (read_page == 0 && newFile)
                        FLIM.InitializeData(FLIM.State, true);


                    int zPerFile = 1;
                    if (fm == FileFormat.ZLinear)
                        zPerFile = image_length;

                    if (height < 1 || nChannels < 0)
                        return FileError.FormatError;

                    FLIM.AssureFLIMRawSize();
                    //var img = FLIM.FLIMRaw;
                    //Making linear model.

                    var imgZ = new ushort[zPerFile][][];
                    var bimgZ = new byte[zPerFile][][];

                    for (int z = 0; z < zPerFile; z++)
                    {
                        imgZ[z] = new ushort[nChannels][];
                        bimgZ[z] = new byte[nChannels][];
                        if (depth == 2)
                            for (int i = 0; i < nChannels; i++)
                                imgZ[z][i] = new ushort[height * width * n_time[i]];

                        else if (depth == 1)
                            for (int i = 0; i < nChannels; i++)
                                bimgZ[z][i] = new byte[height * width * n_time[i]];
                    }

                    if (fm == FileFormat.None)
                    {
                        var img = imgZ[0];
                        var bimg = bimgZ[0];
                        int nCh = 0;
                        for (short chnnl = 0; chnnl < nChannels; ++chnnl)
                        {
                            if (FLIM.saveChannels[chnnl])
                                nCh++;
                        }

                        for (short chnnl = 0; chnnl < nChannels; ++chnnl)
                        {
                            if (FLIM.saveChannels[chnnl])
                            {
                                long dirIndex = read_page * nCh + chnnl;
                                if (dirIndex > short.MaxValue)
                                    return FileError.FormatError;
                                image.SetDirectory((short)dirIndex);
                                for (int y = 0; y < height; ++y)
                                {
                                    image.ReadScanline(scanline, y);
                                    if (depth == 2)
                                        Buffer.BlockCopy(scanline, 0, img[chnnl], y * width * depth, width * depth);
                                    else
                                        Buffer.BlockCopy(scanline, 0, bimg[chnnl], y * width * depth, width * depth);
                                }
                            }
                        }
                    }
                    else if (fm == FileFormat.Linear || fm == FileFormat.ZLinear)
                    {
                        for (int z = 0; z < zPerFile; z++)
                        {
                            var img = imgZ[z];
                            var bimg = bimgZ[z];

                            image.ReadScanline(scanline, z);

                            int offset = 0;
                            for (int chnnl = 0; chnnl < nChannels; ++chnnl)
                            {
                                if (n_time[chnnl] != 0)
                                {
                                    if (depth == 2)
                                        Buffer.BlockCopy(scanline, offset, img[chnnl], 0, img[chnnl].Length * depth);
                                    else
                                        Buffer.BlockCopy(scanline, offset, bimg[chnnl], 0, bimg[chnnl].Length * depth);
                                }
                                offset += n_time[chnnl] * width * height * depth;
                            }
                        }
                    }
                    else if (fm == FileFormat.ChTime_YX) //Standard 
                    {
                        var img = imgZ[0];
                        var bimg = bimgZ[0];
                        for (int y = 0; y < height; ++y)
                            for (int x = 0; x < width; ++x)
                            {
                                image.ReadScanline(scanline, y * width + x);
                                int offset = 0;
                                for (int chnnl = 0; chnnl < nChannels; ++chnnl)
                                {
                                    if (n_time[chnnl] != 0)
                                    {
                                        if (depth == 2)
                                            Buffer.BlockCopy(scanline, offset, img[chnnl], (y * width + x) * n_time[chnnl] * depth, n_time[chnnl] * depth);
                                        else
                                            Buffer.BlockCopy(scanline, offset, bimg[chnnl], (y * width + x) * n_time[chnnl] * depth, n_time[chnnl] * depth);
                                    }
                                    offset += n_time[chnnl] * depth;
                                }
                            }
                    }
                    else if (fm == FileFormat.Time_ChYX)
                    {
                        var img = imgZ[0];
                        var bimg = bimgZ[0];

                        int nT = n_time.Max();
                        for (int chnnl = 0; chnnl < nChannels; ++chnnl)
                            for (int y = 0; y < height; ++y)
                                for (int x = 0; x < width; ++x)
                                {
                                    image.ReadScanline(scanline, chnnl * width * height + y * width + x);
                                    if (depth == 2)
                                        Buffer.BlockCopy(scanline, 0, img[chnnl], (y * width + x) * nT * depth, nT * depth);
                                    else
                                        Buffer.BlockCopy(scanline, 0, bimg[chnnl], (y * width + x) * nT * depth, nT * depth);
                                }
                    }
                    else if (fm == FileFormat.ChYX_Time)
                    {
                        var img = imgZ[0];
                        var bimg = bimgZ[0];

                        int nT = n_time.Max();

                        for (int i = 0; i < n_time[0]; i++)
                        {
                            image.ReadScanline(scanline, i);

                            //byte[] buf = new byte[height * nChannels * width * depth]; //Do line-by-line!

                            for (int y = 0; y < height; ++y)
                            {
                                for (int x = 0; x < width; ++x)
                                {
                                    //Ch1
                                    for (int chnnl = 0; chnnl < nChannels; ++chnnl)
                                    {

                                        byte[] byteArray = new byte[depth];

                                        for (int k = 0; k < depth; ++k)
                                        {
                                            int index = chnnl * width * height * depth + y * width * depth + x * depth + k;
                                            byteArray[k] = scanline[index];
                                        }
                                        if (depth == 1)
                                            bimg[chnnl][(y * width + x) * nT + i] = byteArray[0];
                                        else
                                            img[chnnl][(y * width + x) * nT + i] = BitConverter.ToUInt16(byteArray, 0);
                                    }
                                }
                            }
                        }//i
                         //image.ReadDirectory();
                         //}//page
                    }


                    if (newFile)
                    {
                        FLIM.clearMemory();
                        if (zPerFile == 1)
                        {
                            FLIM.n_pages = nPages; //put in page first.                            
                        }
                        else
                        {
                            FLIM.n_pages = zPerFile; //put all files in page.
                        }
                    }

                    FLIM.KeepPagesInMemory = SavePagesInMemory;

                    if (depth == 1)
                    {
                        for (int z = 0; z < bimgZ.Length; z++)
                            for (int i = 0; i < bimgZ[z].Length; i++)
                            {
                                if (bimgZ[z][i] != null)
                                    imgZ[z][i] = MatrixCalc.changeDepthFrom8To16(bimgZ[z][i]);
                                else
                                    imgZ[z][i] = null;
                            }
                    }

                    for (int z = 0; z < imgZ.Length; z++)
                        for (int ch = 0; ch < nChannels; ch++)
                            if (n_time[ch] == 0)
                            {
                                imgZ[z][ch] = null;
                            }

                    FLIM.imagesPerFile = zPerFile;

                    // Route by zPerFile (IMAGELENGTH for this IFD), not by Format alone: multi-IFD Z stacks
                    // use Format=ZLinear in the header while each IFD still has one Z row (zPerFile==1).
                    // Those must use PutToPage_Linear / 4D load per page (pre-7080967 behavior).
                    if (SavePagesInMemory)
                    {
                        if (zPerFile == 1)
                        {
                            FLIM.PutToPage_Linear(imgZ[0], dt, into_page);
                        }
                        else
                        {
                            FLIM.Add_AllFLIM_PageFormat_To_FLIM_Pages5D(imgZ, dt, into_page);
                        }
                    }
                    else
                    {
                        if (zPerFile == 1)
                        {
                            var flim4d = new ushort[imgZ[0].Length][,,];
                            for (int ch = 0; ch < flim4d.Length; ch++)
                            {
                                if (imgZ[0][ch] == null)
                                {
                                    flim4d[ch] = null;
                                    continue;
                                }
                                flim4d[ch] = (ushort[,,])MatrixCalc.Reshape(imgZ[0][ch], new int[] { height, width, n_time[ch] });
                            }
                            FLIM.LoadFLIMRawFromData4D(flim4d, dt, false);
                            FLIM.currentPage = read_page > int.MaxValue ? int.MaxValue : (int)read_page;
                        }
                        else
                        {
                            var flim_data5d = ImageProcessing.FLIM_Pages2FLIMRaw5D(imgZ, new int[] { height, width }, n_time);
                            FLIM.addToPageAndCalculate5D(flim_data5d, dt, true, true, 0, true);
                            FLIM.currentPage5D = read_page > int.MaxValue ? int.MaxValue : (int)read_page;
                            FLIM.SetAcquiredTimePage5D(FLIM.currentPage5D, dt);
                        }
                    }

                    if (!SavePagesInMemory)
                    {
                        // Preserve per-page acquisition time even when pages are not cached.
                        FLIM.expandPage(into_page + 1);
                        if (FLIM.acquiredTime_Pages != null && into_page >= 0 && into_page < FLIM.acquiredTime_Pages.Length)
                            FLIM.acquiredTime_Pages[into_page] = dt;
                    }
                } //COMPRESSION
            }

            //String filePath = Path.GetDirectoryName(fileName);
            //String fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            String fName = Path.GetFileName(fileName);
            FileParts(fileName, out String filePath, out String fileBaseName, out int fileNum);
            FLIM.State.Files.pathName = filePath;

            //Very stupid way but it works..... TODO 
            for (int i = 0; i < FLIM.offset.Length; i++)
                if (i < save_currentOffset.Length)
                    FLIM.offset[i] = save_currentOffset[i];
            FLIM.State.Spc.analysis.offset = (double[])FLIM.offset.Clone();

            if (newFile)
            {
                FLIM.pathName = filePath;

                if (fileNum >= 0)
                {
                    FLIM.baseName = fileBaseName;
                    FLIM.fileCounter = fileNum;
                    FLIM.numberedFile = true;
                    FLIM.fileName = fName;
                    FLIM.fileExtension = Path.GetExtension(fName);
                    FLIM.State.Files.extension = FLIM.fileExtension;
                    FLIM.fullFileName = Path.Combine(filePath, FLIM.fileName);
                }
                else
                {
                    FLIM.baseName = fileName;
                    FLIM.fileCounter = 0;
                    FLIM.fileName = fName;
                    FLIM.numberedFile = false;
                    FLIM.fullFileName = fileName;
                    FLIM.fileExtension = Path.GetExtension(fName);
                    FLIM.State.Files.extension = FLIM.fileExtension;
                }
            }

            return FileIO.FileError.Success;
        }



        public static void FileParts(String fullname, out String folderPath, out String fileBaseName, out int num)
        {
            string fileName = Path.GetFileNameWithoutExtension(fullname);
            folderPath = Path.GetDirectoryName(fullname);
            string extension = Path.GetExtension(fullname);

            num = -1;
            fileBaseName = fileName;

            bool numberedFile = true;
            if (fileName.Length > 3 && !Int32.TryParse(fileName.Substring(fileName.Length - 3), out num))
                numberedFile = false;
            else
            {
                numberedFile = true;
                fileBaseName = fileName.Substring(0, fileName.Length - 3);
            }

        }

        private static void PrepareForOverwrite(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            if (!File.Exists(fileName))
                return;

            try
            {
                // In case the previous run marked the file read-only.
                try { File.SetAttributes(fileName, FileAttributes.Normal); } catch { }

                // LibTiff "w"/"w8" should overwrite, but on Windows an existing file can still block open/overwrite
                // depending on how another process has it opened. Deleting first makes the intent explicit and gives
                // a clearer exception message when the file is in use.
                File.Delete(fileName);
            }
            catch (Exception ex)
            {
                throw new IOException("Could not delete existing file for overwrite: " + fileName, ex);
            }
        }

        private static bool IsBigTiff(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            if (!File.Exists(fileName))
                return false;

            try
            {
                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (stream.Length < 4)
                        return false;

                    int b0 = stream.ReadByte();
                    int b1 = stream.ReadByte();
                    int b2 = stream.ReadByte();
                    int b3 = stream.ReadByte();

                    return (b0 == 'I' && b1 == 'I' && b2 == 0x2B && b3 == 0x00) ||
                           (b0 == 'M' && b1 == 'M' && b2 == 0x00 && b3 == 0x2B);
                }
            }
            catch
            {
                return false;
            }
        }

        public int SaveColorImageInTiff(String fileName, Bitmap bmp, DateTime dt, bool overwrite, bool[] saveChannels)
        {
            int error = 0;
            for (int i = 0; i < 100; i++)
            {
                error = SaveColorImageInTiff_core(fileName, bmp, dt, overwrite, saveChannels);
                if (error != -100)
                    break;
                System.Threading.Thread.Sleep(10);
            }

            return error;
        }

        public int SaveColorImageInTiff_core(String fileName, Bitmap bmp, DateTime dt, bool overwrite, bool[] saveChannels)
        {
            int error = 0;
            if (bmp == null)
                return -1;

            int width = bmp.Width;
            int height = bmp.Height;

            string writeMode;
            string acquiredTime = dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");

            if (overwrite)
                writeMode = "w8";
            else
                writeMode = IsBigTiff(fileName) ? "a8" : "a";

            if (overwrite)
                PrepareForOverwrite(fileName);

            using (Tiff output = Tiff.Open(fileName, writeMode))
            {
                if (output == null)
                {
                    Debug.WriteLine("Problem: " + fileName);
                    return -100;
                }

                PixelFormat format = PixelFormat.Format32bppRgb; //Format for saving.

                byte[] raster = GetImageRasterBytes(bmp, PixelFormat.Format24bppRgb);

                output.SetField(TiffTag.IMAGEWIDTH, width);
                output.SetField(TiffTag.IMAGELENGTH, height);

                if (format == PixelFormat.Format32bppRgb)
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 4); //FOR COLOR
                else
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 3); //FOR COLOR

                output.SetField(TiffTag.BITSPERSAMPLE, 8); //FOR COLOR
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                output.SetField(TiffTag.ROWSPERSTRIP, width);
                output.SetField(TiffTag.XRESOLUTION, 100.0);
                output.SetField(TiffTag.YRESOLUTION, 100.0);
                output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.NONE);
                output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

                string str1;
                if (overwrite)
                {
                    str1 = CreateHeader(saveChannels);
                    str1 = str1 + String.Format("Acquired_Time = {0}", acquiredTime);
                }
                else
                    str1 = String.Format("Acquired_Time = {0}", acquiredTime);

                output.SetField(TiffTag.IMAGEDESCRIPTION, str1);


                raster = ConvertSamples(raster, bmp.Width, bmp.Height, format);
                int stride = raster.Length / bmp.Height;

                for (int i = 0, offset = 0; i < bmp.Height; i++)
                {
                    output.WriteScanline(raster, offset, i, 0);
                    offset += stride;
                }

                output.WriteDirectory();
            } //Tiff

            return (error);
        } //SaveImageInTiff

        public static byte[] GetImageRasterBytes(Bitmap bmp, PixelFormat format)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            byte[] bits = null;

            //Bitmap NewBitmap = bmp.Clone(rect, format);
            Bitmap NewBitmap = bmp;
            try
            {
                // Lock the managed memory
                BitmapData bmpdata = NewBitmap.LockBits(rect, ImageLockMode.ReadWrite, format);

                // Declare an array to hold the bytes of the bitmap.
                bits = new byte[bmpdata.Stride * bmpdata.Height];

                // Copy the values into the array.
                System.Runtime.InteropServices.Marshal.Copy(bmpdata.Scan0, bits, 0, bits.Length);

                // Release managed memory
                NewBitmap.UnlockBits(bmpdata);

            }
            catch
            {
                return null;
            }

            return bits;
        }

        public static byte[] ConvertSamples(byte[] data, int width, int height, PixelFormat format)
        {
            //data is always in 24bit color.
            int stride = data.Length / height;
            int samplesPerPixel = 3;
            int samplesPerPixelNew = 3;
            if (format == PixelFormat.Format32bppRgb)
            {
                samplesPerPixelNew = 4;
            }

            byte[] dataNew = new byte[height * width * samplesPerPixelNew];
            int strideNew = dataNew.Length / height;

            for (int y = 0; y < height; y++)
            {
                int offset = stride * y;
                int offsetNew = strideNew * y;
                //int strideEnd = offset + width * samplesPerPixel;
                //int strideEndNew = offsetNew + width * samplesPerPixelNew;
                //for (int i = offsetNew; i < strideEnd; i += samplesPerPixel)
                for (int i = 0; i < width; i++)
                {
                    int newPlace = offsetNew + i * samplesPerPixelNew;
                    int oldPlace = offset + i * samplesPerPixel;
                    dataNew[newPlace] = data[oldPlace + 2];
                    dataNew[newPlace + 1] = data[oldPlace + 1];
                    dataNew[newPlace + 2] = data[oldPlace];

                    if (samplesPerPixelNew == 4)
                        dataNew[newPlace + 3] = 255;
                }
            }

            return dataNew;
        }

        public FileError LoadFloatArrayFromTiff(string filename, int read_page, out float[,] img)
        {
            img = null;

            using (Tiff image = Tiff.Open(filename, "r"))
            {
                if (image == null)
                {
                    //MessageBox.Show("Could not open this image");
                    return FileError.UnKnown;
                }

                int nPages = image.NumberOfDirectories();

                if (read_page < nPages)
                    image.SetDirectory((short)read_page);

                int stride = image.ScanlineSize();
                byte[] scanline = new byte[stride];

                FieldValue[] value;
                Compression compression = (Compression)image.GetField(TiffTag.COMPRESSION)[0].ToInt();
                value = image.GetField(TiffTag.IMAGEDESCRIPTION);
                String Description = value[0].ToString();

                value = image.GetField(TiffTag.IMAGEWIDTH);
                int width = value[0].ToInt(); //Everything else;
                value = image.GetField(TiffTag.IMAGELENGTH);
                int height = value[0].ToInt();
                value = image.GetField(TiffTag.BITSPERSAMPLE);
                int depth = value[0].ToInt() / 8;


                if (compression == Compression.LZW || compression == Compression.PACKBITS || compression == Compression.NONE)
                {
                    // LZW and PackBits compression schemes do not allow 
                    // scanlines to be read in a random fashion. 
                    // So, we need to read all scanlines from start of the image.                     

                    img = new float[height, width];


                    for (int y = 0; y < height; y++)
                    {
                        var buf = new ushort[width];
                        image.ReadScanline(scanline, y);
                        Buffer.BlockCopy(scanline, 0, buf, 0, scanline.Length);
                        for (int x = 0; x < width; x++)
                            img[y, x] = (float)buf[x] / 1000.0f;
                    }
                } //COMPRESSION
            } //Tiff

            return FileError.Success;
        }

        public void SaveFloatImageInTiff(float[][] image, string filename)
        {
            UInt16[,] image16 = new ushort[image.Length, image[0].Length];
            var saveChannels = new bool[] { true };
            for (int y = 0; y < image.Length; y++)
                for (int x = 0; x < image[0].Length; x++)
                {
                    image16[y, x] = (UInt16)(1000.0 * image[y][x]);
                }
            Save2DImageInTiff(filename, image16, DateTime.Now, true, saveChannels);
        }

        public void SaveFloatImageInTiff(float[,] image, string filename)
        {
            int h = image.GetLength(0);
            int w = image.GetLength(1);
            ushort[,] image16 = new ushort[h, w];
            var saveChannels = new bool[] { true };
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    image16[y, x] = (UInt16)(1000.0 * image[y, x]);
                }
            Save2DImageInTiff(filename, image16, DateTime.Now, true, saveChannels);
        }

        public int Save2DImageInTiff(String fileName, UInt16[,] img, DateTime dt, bool overwrite, bool[] saveChannels)
        {
            int error = 0;
            for (int i = 0; i < 100; i++)
            {
                error = Save2DImageInTiff_core(fileName, img, dt, overwrite, saveChannels);
                if (error != -100)
                    break;
                System.Threading.Thread.Sleep(10);
            }

            return error;
        }


        public int Save2DImageInTiff_core(String fileName, UInt16[,] img, DateTime dt, bool overwrite, bool[] saveChannels)
        {
            int depth = 2; //Bytes; 16 bit image.
            int error = 0;
            int nCh = State.Acq.nChannels;

            int width = img.GetLength(0);
            int height = img.GetLength(1);

            string writeMode;
            string acquiredTime = dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");


            if (overwrite)
                writeMode = "w8";
            else
                writeMode = IsBigTiff(fileName) ? "a8" : "a";

            if (overwrite)
                PrepareForOverwrite(fileName);

            using (Tiff output = Tiff.Open(fileName, writeMode))
            {
                if (output == null)
                    return -100;

                output.SetField(TiffTag.IMAGEWIDTH, height);
                output.SetField(TiffTag.IMAGELENGTH, width);
                output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                output.SetField(TiffTag.BITSPERSAMPLE, depth * 8);
                output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                output.SetField(TiffTag.ROWSPERSTRIP, height);
                output.SetField(TiffTag.XRESOLUTION, 100.0);
                output.SetField(TiffTag.YRESOLUTION, 100.0);
                output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.NONE);
                output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                //output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.SEPARATE);
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                output.SetField(TiffTag.COMPRESSION, Compression.LZW);
                output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

                string str1;
                if (overwrite)
                {
                    str1 = CreateHeader(saveChannels);
                    str1 = str1 + String.Format("Acquired_Time = {0}", acquiredTime);
                }
                else
                    str1 = String.Format("Acquired_Time = {0}", acquiredTime);

                output.SetField(TiffTag.IMAGEDESCRIPTION, str1);

                for (int x = 0; x < width; ++x)
                {

                    byte[] buf = new byte[height * depth]; //Do line-by-line!
                                                           //Buffer.BlockCopy(img[x], 0, buf, 0, buf.Length);
                    Buffer.BlockCopy(img, x * width * depth, buf, 0, buf.Length);

                    output.WriteScanline(buf, x);
                }

                output.WriteDirectory();
            } //Tiff

            return (error);
        } //SaveImageInTiff


        //Saving data for Y-X-T data.
        public int SaveFLIMInTiff(String fileName, UInt16[,,] img, DateTime dt, bool overwrite, bool[] saveChannels)
        {
            var image5D = new UInt16[1][][,,];
            image5D[0] = new ushort[1][,,];
            image5D[0][0] = img;
            return SaveFLIMInTiffZStack(fileName, image5D, dt, overwrite, saveChannels);
        }

        //save data for C-Y-X-T data
        public int SaveFLIMInTiff(String fileName, UInt16[][,,] img, DateTime dt, bool overwrite, bool[] saveChannels)
        {
            var image5D = new UInt16[1][][,,];
            image5D[0] = img;
            return SaveFLIMInTiffZStack(fileName, image5D, dt, overwrite, saveChannels);
        }

        public bool TryConcatenateFLIMTiffRaw(string[] inputFiles, string outputFile, out string errorMessage,
            Func<bool> shouldCancel = null, Action<int, int, string> progress = null,
            Action<int, int, int, int, string> frameProgress = null)
        {
            errorMessage = "";

            if (inputFiles == null || inputFiles.Length == 0)
            {
                errorMessage = "No input files were provided.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(outputFile))
            {
                errorMessage = "No output file was provided.";
                return false;
            }

            if (Path.GetExtension(outputFile).Equals(".btf", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "OME-TIFF .btf output is not supported by raw FLIM concatenation.";
                return false;
            }

            string outputFullPath = Path.GetFullPath(outputFile);
            foreach (string inputFile in inputFiles)
            {
                if (string.IsNullOrWhiteSpace(inputFile))
                {
                    errorMessage = "One of the input file names is empty.";
                    return false;
                }

                if (!File.Exists(inputFile))
                {
                    errorMessage = "Input file not found: " + inputFile;
                    return false;
                }

                if (Path.GetExtension(inputFile).Equals(".btf", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = "OME-TIFF .btf input is not supported by raw FLIM concatenation: " + inputFile;
                    return false;
                }

                if (String.Equals(Path.GetFullPath(inputFile), outputFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = "The output file cannot also be an input file.";
                    return false;
                }
            }

            if (RawConcatCanceled(shouldCancel, out errorMessage))
                return false;

            int[] directoryCounts = new int[inputFiles.Length];
            long totalDirectories = 0;
            for (int inputIndex = 0; inputIndex < inputFiles.Length; inputIndex++)
            {
                if (RawConcatCanceled(shouldCancel, out errorMessage))
                    return false;

                string inputFile = inputFiles[inputIndex];
                using (Tiff input = Tiff.Open(inputFile, "r"))
                {
                    if (input == null)
                    {
                        errorMessage = "Could not open input file: " + inputFile;
                        return false;
                    }

                    int nDirectories = input.NumberOfDirectories();
                    if (nDirectories <= 0)
                    {
                        errorMessage = "Input file has no TIFF directories: " + inputFile;
                        return false;
                    }

                    directoryCounts[inputIndex] = nDirectories;
                    totalDirectories += nDirectories;
                    if (totalDirectories > int.MaxValue)
                    {
                        errorMessage = "Too many TIFF directories to concatenate.";
                        return false;
                    }
                }
            }

            PrepareForOverwrite(outputFile);

            RawFlimTiffLayout expectedLayout = null;
            int outputDirectory = 0;
            int totalFrames = (int)totalDirectories;

            using (Tiff output = Tiff.Open(outputFile, "w8"))
            {
                if (output == null)
                {
                    errorMessage = "Could not open output file for writing: " + outputFile;
                    return false;
                }

                for (int inputIndex = 0; inputIndex < inputFiles.Length; inputIndex++)
                {
                    if (RawConcatCanceled(shouldCancel, out errorMessage))
                        return false;

                    string inputFile = inputFiles[inputIndex];
                    try
                    {
                        progress?.Invoke(inputIndex + 1, inputFiles.Length, inputFile);
                    }
                    catch
                    {
                    }

                    using (Tiff input = Tiff.Open(inputFile, "r"))
                    {
                        if (input == null)
                        {
                            errorMessage = "Could not open input file: " + inputFile;
                            return false;
                        }

                        int nDirectories = directoryCounts[inputIndex];

                        for (int directory = 0; directory < nDirectories; directory++)
                        {
                            if (RawConcatCanceled(shouldCancel, out errorMessage))
                                return false;

                            if (directory > short.MaxValue)
                            {
                                errorMessage = "Input file has too many TIFF directories for LibTiff.NET: " + inputFile;
                                return false;
                            }

                            if (!input.SetDirectory((short)directory))
                            {
                                errorMessage = "Could not select TIFF directory " + directory + " in " + inputFile;
                                return false;
                            }

                            RawFlimTiffLayout layout = RawFlimTiffLayout.FromTiff(input);
                            if (!layout.IsValid)
                            {
                                errorMessage = "Input file is missing required FLIM TIFF tags: " + inputFile;
                                return false;
                            }

                            if (expectedLayout == null)
                                expectedLayout = layout;
                            else if (!expectedLayout.IsCompatibleWith(layout))
                            {
                                errorMessage = "Input file has TIFF layout that differs from the first file: " + inputFile;
                                return false;
                            }

                            try
                            {
                                frameProgress?.Invoke(inputIndex + 1, inputFiles.Length, outputDirectory + 1, totalFrames, inputFile);
                            }
                            catch
                            {
                            }

                            if (!CopyCurrentTiffDirectoryRaw(input, output, outputFile, outputDirectory == 0, totalFrames, shouldCancel, out errorMessage))
                            {
                                if (!String.Equals(errorMessage, "Canceled.", StringComparison.OrdinalIgnoreCase))
                                    errorMessage = errorMessage + " File: " + inputFile;
                                return false;
                            }

                            outputDirectory++;
                        }
                    }
                }
            }

            if (outputDirectory == 0)
            {
                errorMessage = "No TIFF directories were copied.";
                return false;
            }

            return true;
        }

        private static bool CopyCurrentTiffDirectoryRaw(Tiff input, Tiff output, string outputFile, bool includeFullHeader, int headerFrameCount,
            Func<bool> shouldCancel, out string errorMessage, bool forceSinglePlaneTimeCourse = true, bool useOmeTiff = false)
        {
            errorMessage = "";

            if (RawConcatCanceled(shouldCancel, out errorMessage))
                return false;

            RawFlimTiffLayout layout = RawFlimTiffLayout.FromTiff(input);
            if (!layout.IsValid)
            {
                errorMessage = "Missing required TIFF tags.";
                return false;
            }

            output.SetField(TiffTag.IMAGEWIDTH, layout.Width);
            output.SetField(TiffTag.IMAGELENGTH, layout.Length);
            output.SetField(TiffTag.ROWSPERSTRIP, layout.RowsPerStrip);
            output.SetField(TiffTag.SAMPLESPERPIXEL, layout.SamplesPerPixel);
            output.SetField(TiffTag.BITSPERSAMPLE, layout.BitsPerSample);
            output.SetField(TiffTag.ORIENTATION, (BitMiracle.LibTiff.Classic.Orientation)layout.Orientation);
            output.SetField(TiffTag.XRESOLUTION, layout.XResolution);
            output.SetField(TiffTag.YRESOLUTION, layout.YResolution);
            output.SetField(TiffTag.RESOLUTIONUNIT, (ResUnit)layout.ResolutionUnit);
            output.SetField(TiffTag.PLANARCONFIG, (PlanarConfig)layout.PlanarConfig);
            output.SetField(TiffTag.PHOTOMETRIC, (Photometric)layout.Photometric);
            output.SetField(TiffTag.COMPRESSION, (Compression)layout.Compression);
            output.SetField(TiffTag.FILLORDER, (FillOrder)layout.FillOrder);
            CopyOptionalIntField(input, output, TiffTag.PREDICTOR);

            string description = GetTiffString(input, TiffTag.IMAGEDESCRIPTION, "");
            if (includeFullHeader)
            {
                description = UpdateHeaderIntValue(description, "State.Acq.nFrames", headerFrameCount);
                if (forceSinglePlaneTimeCourse)
                {
                    description = UpdateHeaderBoolValue(description, "State.Acq.ZStack", false);
                    description = UpdateHeaderIntValue(description, "State.Acq.nSlices", 1);
                    description = UpdateHeaderBoolValue(description, "State.Acq.fastZScan", false);
                    description = UpdateHeaderIntValue(description, "State.Acq.FastZ_nSlices", 1);
                }
                description = UpdateHeaderFileValues(description, outputFile, useOmeTiff);
            }
            else
                description = CreateConcatenatedPageDescription(description);
            output.SetField(TiffTag.IMAGEDESCRIPTION, description);

            int nStrips = input.NumberOfStrips();
            if (nStrips <= 0)
            {
                errorMessage = "No strips found in TIFF directory.";
                return false;
            }

            for (int strip = 0; strip < nStrips; strip++)
            {
                if (RawConcatCanceled(shouldCancel, out errorMessage))
                    return false;

                long rawSize = input.RawStripSize(strip);
                if (rawSize < 0 || rawSize > int.MaxValue)
                {
                    errorMessage = "Unsupported raw strip size: " + rawSize;
                    return false;
                }

                byte[] buffer = new byte[Math.Max(1, (int)rawSize)];
                int bytesRead = input.ReadRawStrip(strip, buffer, 0, (int)rawSize);
                if (bytesRead < 0)
                {
                    errorMessage = "Could not read raw strip " + strip;
                    return false;
                }

                int bytesWritten = output.WriteRawStrip(strip, buffer, 0, bytesRead);
                if (bytesWritten < 0)
                {
                    errorMessage = "Could not write raw strip " + strip;
                    return false;
                }

                if (RawConcatCanceled(shouldCancel, out errorMessage))
                    return false;
            }

            if (!output.WriteDirectory())
            {
                errorMessage = "Could not write TIFF directory.";
                return false;
            }

            return true;
        }

        private static bool RawConcatCanceled(Func<bool> shouldCancel, out string errorMessage)
        {
            errorMessage = "";

            if (shouldCancel != null && shouldCancel())
            {
                errorMessage = "Canceled.";
                return true;
            }

            return false;
        }

        private sealed class RawFlimTiffLayout
        {
            public int Width;
            public int Length;
            public int RowsPerStrip;
            public int SamplesPerPixel;
            public int BitsPerSample;
            public int Orientation;
            public double XResolution;
            public double YResolution;
            public int ResolutionUnit;
            public int PlanarConfig;
            public int Photometric;
            public int Compression;
            public int FillOrder;
            public int StripCount;
            public string Format;

            public bool IsValid
            {
                get
                {
                    return Width > 0
                        && Length > 0
                        && RowsPerStrip > 0
                        && SamplesPerPixel > 0
                        && BitsPerSample > 0
                        && StripCount > 0;
                }
            }

            public bool IsCompatibleWith(RawFlimTiffLayout other)
            {
                if (other == null)
                    return false;

                return Width == other.Width
                    && Length == other.Length
                    && RowsPerStrip == other.RowsPerStrip
                    && SamplesPerPixel == other.SamplesPerPixel
                    && BitsPerSample == other.BitsPerSample
                    && PlanarConfig == other.PlanarConfig
                    && Photometric == other.Photometric
                    && Compression == other.Compression
                    && FillOrder == other.FillOrder
                    && StripCount == other.StripCount
                    && String.Equals(Format, other.Format, StringComparison.OrdinalIgnoreCase);
            }

            public static RawFlimTiffLayout FromTiff(Tiff tiff)
            {
                string description = GetTiffString(tiff, TiffTag.IMAGEDESCRIPTION, "");
                return new RawFlimTiffLayout
                {
                    Width = GetTiffInt(tiff, TiffTag.IMAGEWIDTH, -1, false),
                    Length = GetTiffInt(tiff, TiffTag.IMAGELENGTH, -1, false),
                    RowsPerStrip = GetTiffInt(tiff, TiffTag.ROWSPERSTRIP, -1, true),
                    SamplesPerPixel = GetTiffInt(tiff, TiffTag.SAMPLESPERPIXEL, 1, true),
                    BitsPerSample = GetTiffInt(tiff, TiffTag.BITSPERSAMPLE, 1, true),
                    Orientation = GetTiffInt(tiff, TiffTag.ORIENTATION, (int)BitMiracle.LibTiff.Classic.Orientation.TOPLEFT, true),
                    XResolution = GetTiffDouble(tiff, TiffTag.XRESOLUTION, 100.0, true),
                    YResolution = GetTiffDouble(tiff, TiffTag.YRESOLUTION, 100.0, true),
                    ResolutionUnit = GetTiffInt(tiff, TiffTag.RESOLUTIONUNIT, (int)ResUnit.NONE, true),
                    PlanarConfig = GetTiffInt(tiff, TiffTag.PLANARCONFIG, (int)BitMiracle.LibTiff.Classic.PlanarConfig.CONTIG, true),
                    Photometric = GetTiffInt(tiff, TiffTag.PHOTOMETRIC, (int)BitMiracle.LibTiff.Classic.Photometric.MINISBLACK, true),
                    Compression = GetTiffInt(tiff, TiffTag.COMPRESSION, (int)BitMiracle.LibTiff.Classic.Compression.NONE, true),
                    FillOrder = GetTiffInt(tiff, TiffTag.FILLORDER, (int)BitMiracle.LibTiff.Classic.FillOrder.MSB2LSB, true),
                    StripCount = tiff.NumberOfStrips(),
                    Format = ExtractHeaderValue(description, "Format")
                };
            }
        }

        private static int GetTiffInt(Tiff tiff, TiffTag tag, int fallback, bool useDefaulted)
        {
            FieldValue[] values = useDefaulted ? tiff.GetFieldDefaulted(tag) : tiff.GetField(tag);
            if (values == null || values.Length == 0)
                return fallback;
            return values[0].ToInt();
        }

        private static double GetTiffDouble(Tiff tiff, TiffTag tag, double fallback, bool useDefaulted)
        {
            FieldValue[] values = useDefaulted ? tiff.GetFieldDefaulted(tag) : tiff.GetField(tag);
            if (values == null || values.Length == 0)
                return fallback;
            return values[0].ToDouble();
        }

        private static string GetTiffString(Tiff tiff, TiffTag tag, string fallback)
        {
            FieldValue[] values = tiff.GetField(tag);
            if (values == null || values.Length == 0)
                return fallback;
            return values[0].ToString();
        }

        private static void CopyOptionalIntField(Tiff input, Tiff output, TiffTag tag)
        {
            FieldValue[] values = input.GetField(tag);
            if (values == null || values.Length == 0)
                return;
            output.SetField(tag, values[0].ToInt());
        }

        private static string CreateConcatenatedPageDescription(string description)
        {
            string acquiredTimeLine = ExtractHeaderLine(description, "Acquired_Time");
            string formatLine = ExtractHeaderLine(description, "Format");

            if (String.IsNullOrEmpty(acquiredTimeLine) && String.IsNullOrEmpty(formatLine))
                return description ?? "";

            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrEmpty(acquiredTimeLine))
                sb.Append(acquiredTimeLine);
            if (!String.IsNullOrEmpty(formatLine))
                sb.Append(formatLine);
            return sb.ToString();
        }

        private static string UpdateHeaderIntValue(string description, string key, int value)
        {
            return UpdateHeaderLiteralValue(description, key, value.ToString());
        }

        private static string UpdateHeaderBoolValue(string description, string key, bool value)
        {
            return UpdateHeaderLiteralValue(description, key, value ? "True" : "False");
        }

        private static string UpdateHeaderStringValue(string description, string key, string value)
        {
            value = value ?? "";
            return UpdateHeaderLiteralValue(description, key, "\"" + value.Replace("\"", "") + "\"");
        }

        private static string UpdateHeaderLiteralValue(string description, string key, string valueLiteral)
        {
            if (String.IsNullOrEmpty(key))
                return description ?? "";

            description = description ?? "";

            string pattern = @"(^[ \t]*" + Regex.Escape(key) + @"[ \t]*=[ \t]*)([^;\r\n]*)([ \t]*;[^\r\n]*)";
            bool updated = false;
            string result = Regex.Replace(description, pattern, match =>
            {
                updated = true;
                return match.Groups[1].Value + valueLiteral + match.Groups[3].Value;
            }, RegexOptions.Multiline);

            if (updated)
                return result;

            if (description.Length > 0 && !description.EndsWith("\n"))
                description += "\r\n";

            return description + key + " = " + valueLiteral + ";\r\n";
        }

        private static string UpdateHeaderFileValues(string description, string outputFile, bool useOmeTiff)
        {
            if (String.IsNullOrWhiteSpace(outputFile))
                return description ?? "";

            ParseHeaderFileIdentity(outputFile, out string pathName, out string fileNameWithoutExtension,
                out string extension, out string baseName, out int fileCounter, out bool numberedFile,
                out bool channelsInSeparatedFile, out int fileChannel);

            description = UpdateHeaderStringValue(description, "State.Files.pathName", pathName);
            description = UpdateHeaderStringValue(description, "State.Files.fileName", fileNameWithoutExtension);
            description = UpdateHeaderStringValue(description, "State.Files.extension", extension);
            description = UpdateHeaderStringValue(description, "State.Files.baseName", baseName);
            description = UpdateHeaderIntValue(description, "State.Files.fileCounter", fileCounter);
            description = UpdateHeaderBoolValue(description, "State.Files.numberedFile", numberedFile);
            description = UpdateHeaderBoolValue(description, "State.Files.channelsInSeparatedFile", channelsInSeparatedFile);
            description = UpdateHeaderIntValue(description, "State.Files.fileChannel", fileChannel);
            description = UpdateHeaderBoolValue(description, "State.Files.useOmeTiff", useOmeTiff);
            if (useOmeTiff)
                description = UpdateHeaderStringValue(description, "State.Files.extension_ome", extension);

            return description;
        }

        private static void ParseHeaderFileIdentity(string fullFileName, out string pathName, out string fileNameWithoutExtension,
            out string extension, out string baseName, out int fileCounter, out bool numberedFile,
            out bool channelsInSeparatedFile, out int fileChannel)
        {
            pathName = Path.GetDirectoryName(fullFileName) ?? "";
            fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFileName);
            extension = Path.GetExtension(fullFileName);
            baseName = fileNameWithoutExtension;
            fileCounter = 0;
            numberedFile = false;
            channelsInSeparatedFile = false;
            fileChannel = 0;

            int parsedCounter;
            if (fileNameWithoutExtension.Length >= 3
                && Int32.TryParse(fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 3), out parsedCounter))
            {
                numberedFile = true;
                fileCounter = parsedCounter;

                int channel;
                if (fileNameWithoutExtension.Length >= 8
                    && fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 4, 1) == "_"
                    && fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 8, 3) == "_Ch"
                    && Int32.TryParse(fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 5, 1), out channel))
                {
                    fileChannel = channel;
                    channelsInSeparatedFile = true;
                    baseName = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - 8);
                }
                else
                {
                    baseName = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - 3);
                }
            }
            else if (fileNameWithoutExtension.Length >= 4)
            {
                int channel;
                if (fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 4, 3) == "_Ch"
                    && Int32.TryParse(fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 1, 1), out channel))
                {
                    fileChannel = channel;
                    channelsInSeparatedFile = true;
                    baseName = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - 4);
                }
            }
        }

        private static string ExtractHeaderLine(string description, string key)
        {
            if (String.IsNullOrEmpty(description) || String.IsNullOrEmpty(key))
                return "";

            using (StringReader reader = new StringReader(description))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string trimmed = line.TrimStart();
                    if (trimmed.StartsWith(key + " ", StringComparison.OrdinalIgnoreCase)
                        || trimmed.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                    {
                        return line.TrimEnd('\r', '\n') + "\r\n";
                    }
                }
            }

            return "";
        }

        private static string ExtractHeaderValue(string description, string key)
        {
            string line = ExtractHeaderLine(description, key);
            if (String.IsNullOrEmpty(line))
                return "";

            int equals = line.IndexOf('=');
            if (equals < 0)
                return "";

            string value = line.Substring(equals + 1).Trim();
            if (value.EndsWith(";"))
                value = value.Substring(0, value.Length - 1).Trim();
            return value;
        }

        //Saving data for Z-C-Y-X-T data.
        public int SaveFLIMInTiffZStack(String fileName, UInt16[][][,,] FLIM_Pages, DateTime dt, bool overwrite, bool[] saveChannels)
        {
            int error = 0;
            if (State?.Files?.useOmeTiff ?? false)
            {
                error = SaveFLIMInOmeTiffZStack(fileName, FLIM_Pages, dt, overwrite, saveChannels);
                if (error < 0)
                    Debug.WriteLine("Error in OME page writing");
                return error;
            }

            if (FastFlimEnabled)
                error = SaveFLIMInTiffZStack_fast(fileName, FLIM_Pages, dt, overwrite, saveChannels);
            else
                error = SaveFLIMInTiffZStack_core(fileName, FLIM_Pages, dt, overwrite, saveChannels);

            if (error < 0)
                Debug.WriteLine("Error in page writing");

            return error;
        }

        public int SaveFLIMInTiffZStack(String fileName, UInt16[][][,,] FLIM_Pages, DateTime dt, bool overwrite, bool[] saveChannels, FlimCompression compression)
        {
            int error = 0;
            if (State?.Files?.useOmeTiff ?? false)
            {
                error = SaveFLIMInOmeTiffZStack(fileName, FLIM_Pages, dt, overwrite, saveChannels);
                if (error < 0)
                    Debug.WriteLine("Error in OME page writing");
                return error;
            }

            if (FastFlimEnabled)
                error = SaveFLIMInTiffZStack_fast(fileName, FLIM_Pages, dt, overwrite, saveChannels);
            else
                error = SaveFLIMInTiffZStack_core(fileName, FLIM_Pages, dt, overwrite, saveChannels, compression);

            if (error < 0)
                Debug.WriteLine("Error in page writing");

            return error;
        }

        private int SaveFLIMInTiffZStack_fast(String fileName, UInt16[][][,,] FLIM_Pages, DateTime dt, bool overwrite, bool[] saveChannels)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return -1;

            if (!File.Exists(fileName))
                overwrite = true;

            if (!HoldFastWriterOpen)
            {
                using (var writer = BeginFlimTiffWriter(fileName, FLIM_Pages, saveChannels, true, true, true, FlimCompression.None, overwrite))
                {
                    return writer.AppendFrame(FLIM_Pages, dt, overwrite);
                }
            }

            lock (_flimWriterLock)
            {
                if (overwrite)
                    CloseFlimWriterNoLock(fileName);

                if (!_flimWriters.TryGetValue(fileName, out var writer) || !writer.CanAppendFrame(FLIM_Pages))
                {
                    if (writer != null)
                        CloseFlimWriterNoLock(fileName);

                    writer = BeginFlimTiffWriter(fileName, FLIM_Pages, saveChannels, true, true, true, FlimCompression.None, overwrite);
                    _flimWriters[fileName] = writer;
                }

                return writer.AppendFrame(FLIM_Pages, dt, overwrite);
            }
        }

        private int SaveFLIMInOmeTiffZStack(String fileName, UInt16[][][,,] FLIM_Pages, DateTime dt, bool overwrite, bool[] saveChannels)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return -1;
            if (FLIM_Pages == null || FLIM_Pages.Length == 0)
                return -1;

            string omeName = Path.ChangeExtension(fileName, State.Files.extension_ome);
            if (string.IsNullOrWhiteSpace(omeName))
                return -1;

            int nZ = FLIM_Pages.Length;
            int nCh = FLIM_Pages[0]?.Length ?? 0;
            if (nCh <= 0)
                return -1;

            int height = State.Acq.linesPerFrame;
            int width = State.Acq.pixelsPerLine;
            ushort[,,] first = null;
            for (int c = 0; c < nCh && first == null; c++)
            {
                if (FLIM_Pages[0][c] != null)
                    first = FLIM_Pages[0][c];
            }
            if (first != null)
            {
                int h0 = first.GetLength(0);
                int w0 = first.GetLength(1);
                if (h0 > 0 && w0 > 0)
                {
                    height = h0;
                    width = w0;
                }
            }

            int totalPlanes = ExpectedOmeTiffFrames > 0 ? ExpectedOmeTiffFrames : GetAcquisitionOmeTiffFrameCount();

            int zCount = Math.Max(1, nZ);
            int tCount = Math.Max(1, (int)Math.Ceiling((double)Math.Max(1, totalPlanes) / zCount));

            if (overwrite || omeHandle < 0 || !string.Equals(omeFileName, omeName, StringComparison.OrdinalIgnoreCase))
            {
                if (overwrite)
                    PrepareForOverwrite(omeName);

                CloseOMETiff();
                omeFileName = omeName;

                string header = CreateHeader(saveChannels ?? new bool[0]);
                int ret = CreateAndConfigureOMETiff(omeName, saveChannels ?? new bool[0], header, zCount, tCount, width, height);
                if (ret < 0)
                    return ret;
            }

            if (omeHandle < 0)
                return -1;

            int[] n_time = new int[nCh];
            for (int c = 0; c < nCh; c++)
            {
                if (FLIM_Pages[0][c] != null)
                    n_time[c] = FLIM_Pages[0][c].GetLength(2);
                else
                    n_time[c] = 0;
            }

            int depth = 2;
            string acquiredTime = dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");

            if (currentZ + nZ > totalZCount)
                return -1;

            EnsureOmeChannelBuffers(nCh, height, width, n_time, depth);
            int baseIndex = currentZ;
            for (int z = 0; z < nZ; z++)
            {
                int planeIndex = baseIndex + z;
                uint zIndex = (uint)(planeIndex % zCount);
                uint tIndex = (uint)(planeIndex / zCount);
                for (int c = 0; c < nCh; c++)
                {
                    if (saveChannels != null && c < saveChannels.Length && !saveChannels[c])
                        continue;
                    if (FLIM_Pages[z][c] == null || n_time[c] <= 0)
                        continue;

                    byte[] buffer = omeChannelBuffers?[c];
                    IntPtr bufferPtr = omeChannelPointers != null ? omeChannelPointers[c] : IntPtr.Zero;
                    if (buffer == null || bufferPtr == IntPtr.Zero)
                        continue;

                    int length = Buffer.ByteLength(FLIM_Pages[z][c]);
                    if (length > buffer.Length)
                        continue;

                    Buffer.BlockCopy(FLIM_Pages[z][c], 0, buffer, 0, length);
                    SaveOMEData(omeHandle, c, zIndex, tIndex, bufferPtr);
                }
            }

            if (InitFrameInfo(omeHandle) == 0)
            {
                FrameInfo curFrame = frameInfo;
                if (overwrite && baseIndex == 0)
                {
                    image_description += String.Format("Acquired_Time = {0};\r\n", acquiredTime);
                    IntPtr headerPtr = Marshal.StringToHGlobalAnsi(image_description);

                    curFrame.plate_id = uint.MaxValue;
                    SetOMETag(omeHandle, curFrame, tag_header, TiffTagDataType.TIFF_ASCII, image_description.Length, headerPtr);
                    Marshal.FreeHGlobal(headerPtr);
                }
                if (nZ > 1)
                {
                    IntPtr timePtr = Marshal.StringToHGlobalAnsi(acquiredTime);
                    for (int z = 0; z < nZ; z++)
                    {
                        int planeIndex = baseIndex + z;
                        curFrame.z_id = (uint)(planeIndex % zCount);
                        curFrame.t_id = (uint)(planeIndex / zCount);
                        SetOMETag(omeHandle, curFrame, (ushort)TiffTag.EXIF_DATETIMEDIGITIZED, TiffTagDataType.TIFF_ASCII, acquiredTime.Length, timePtr);
                    }
                    Marshal.FreeHGlobal(timePtr);
                }
                else if (baseIndex == 0)
                {
                    IntPtr timePtr = Marshal.StringToHGlobalAnsi(acquiredTime);
                    curFrame.z_id = (uint)(baseIndex % zCount);
                    curFrame.t_id = (uint)(baseIndex / zCount);
                    SetOMETag(omeHandle, curFrame, (ushort)TiffTag.EXIF_DATETIMEDIGITIZED, TiffTagDataType.TIFF_ASCII, acquiredTime.Length, timePtr);
                    Marshal.FreeHGlobal(timePtr);
                }
            }

            currentZ += nZ;

            return 0;
        }

        public FlimTiffWriter BeginFlimTiffWriter(
            string fileName,
            ushort[][][,,] firstFrame,
            bool[] saveChannels,
            bool useStrips,
            bool reuseBuffers,
            bool skipValidation,
            FlimCompression compression,
            bool overwrite)
        {
            return new FlimTiffWriter(this, fileName, firstFrame, saveChannels, useStrips, reuseBuffers, skipValidation, compression, overwrite);
        }

        public sealed class FlimTiffWriter : IDisposable
        {
            private readonly FileIO _owner;
            private readonly Tiff _output;
            private readonly bool[] _saveChannels;
            private readonly bool _useStrips;
            private readonly bool _reuseBuffers;
            private readonly bool _skipValidation;
            private readonly FlimCompression _compression;
            private readonly int _depth;
            private readonly int _nZ;
            private readonly int _nCh;
            private readonly int _height;
            private readonly int _width;
            private readonly int[] _nTime;
            private readonly int _totalTime;
            private readonly int _linearRowBytes;
            private readonly int _timeChyxRowBytes;
            private readonly int _chTimeRowBytes;
            private readonly int[] _channelByteCounts;
            private readonly FileFormat _format;
            private readonly string _formatLine;
            private string _headerBase;
            private byte[] _linearBuffer;
            private byte[] _scanlineBuffer;
            private bool _disposed;

            public FlimTiffWriter(
                FileIO owner,
                string fileName,
                ushort[][][,,] firstFrame,
                bool[] saveChannels,
                bool useStrips,
                bool reuseBuffers,
                bool skipValidation,
                FlimCompression compression,
                bool overwrite)
            {
                if (owner == null)
                    throw new ArgumentNullException(nameof(owner));
                if (firstFrame == null)
                    throw new ArgumentNullException(nameof(firstFrame));
                if (string.IsNullOrWhiteSpace(fileName))
                    throw new ArgumentException("File name is required.", nameof(fileName));

                _owner = owner;
                _saveChannels = saveChannels ?? new bool[0];
                _useStrips = useStrips;
                _reuseBuffers = reuseBuffers;
                _skipValidation = skipValidation;
                _compression = compression;
                _depth = 2;

                if (overwrite)
                    PrepareForOverwrite(fileName);

                _nZ = firstFrame.Length;
                _nCh = _nZ > 0 && firstFrame[0] != null ? firstFrame[0].Length : 0;

                int height = owner.State.Acq.linesPerFrame;
                int width = owner.State.Acq.pixelsPerLine;

                ushort[,,] first = null;
                if (_nZ > 0 && _nCh > 0)
                {
                    for (int c = 0; c < _nCh && first == null; c++)
                    {
                        if (firstFrame[0][c] != null)
                            first = firstFrame[0][c];
                    }
                }

                if (first != null)
                {
                    int h0 = first.GetLength(0);
                    int w0 = first.GetLength(1);
                    if (h0 > 0 && w0 > 0)
                    {
                        if (height != h0 || width != w0)
                        {
                            height = h0;
                            width = w0;
                        }
                    }
                }

                _height = height;
                _width = width;

                _nTime = new int[_nCh];
                for (int c = 0; c < _nCh; c++)
                {
                    if (firstFrame[0][c] != null)
                        _nTime[c] = firstFrame[0][c].GetLength(2);
                    else
                        _nTime[c] = 0;
                }

                _totalTime = _nTime.Sum();
                // Use nZ only: ZStack UI can be true for a single-slice preview; tagging ZLinear then
                // breaks readers that key off Format while each IFD still has IMAGELENGTH==1.
                _format = _nZ == 1 ? FileFormat.Linear : FileFormat.ZLinear;
                _formatLine = $"Format = {_format};\r\n";

                _channelByteCounts = new int[_nCh];
                int offset = 0;
                for (int c = 0; c < _nCh; c++)
                {
                    int byteCount = _nTime[c] * _height * _width * _depth;
                    _channelByteCounts[c] = byteCount;
                    offset += byteCount;
                }

                _linearRowBytes = _totalTime * _height * _width * _depth;
                _timeChyxRowBytes = _nTime.Length > 0 ? _nTime[0] * _depth : 0;
                _chTimeRowBytes = _totalTime * _depth;

                if (_reuseBuffers)
                {
                    if (_format == FileFormat.Linear || _format == FileFormat.ZLinear)
                        _linearBuffer = new byte[Math.Max(1, _linearRowBytes)];
                    else if (_format == FileFormat.Time_ChYX)
                        _scanlineBuffer = new byte[Math.Max(1, _timeChyxRowBytes)];
                    else if (_format == FileFormat.ChTime_YX)
                        _scanlineBuffer = new byte[Math.Max(1, _chTimeRowBytes)];
                }

                string writeMode = overwrite ? "w8" : (IsBigTiff(fileName) ? "a8" : "a");
                _output = Tiff.Open(fileName, writeMode);
                if (_output == null)
                    throw new IOException("Could not open TIFF file for writing: " + fileName);
            }

            public int AppendFrame(ushort[][][,,] pages, DateTime dt, bool includeHeader)
            {
                if (_disposed)
                    return -1;

                if (!_skipValidation && !ValidateFrame(pages))
                    return -2;

                SetCommonTags();

                string acquiredTime = dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");
                string description;
                if (includeHeader)
                {
                    string header = GetHeaderBase();
                    description = header + $"Acquired_Time = {acquiredTime};\r\n" + _formatLine;
                }
                else
                {
                    description = $"Acquired_Time = {acquiredTime};\r\n" + _formatLine;
                }

                _output.SetField(TiffTag.IMAGEDESCRIPTION, description);

                WriteFrameData(pages);
                _output.WriteDirectory();
                return 0;
            }

            private void SetCommonTags()
            {
                if (_format == FileFormat.Linear || _format == FileFormat.ZLinear)
                {
                    _output.SetField(TiffTag.IMAGEWIDTH, _totalTime * _height * _width);
                    _output.SetField(TiffTag.IMAGELENGTH, _nZ);
                    _output.SetField(TiffTag.ROWSPERSTRIP, _useStrips ? 1 : _nZ);
                }
                else if (_format == FileFormat.Time_ChYX)
                {
                    _output.SetField(TiffTag.IMAGEWIDTH, _nTime[0]);
                    _output.SetField(TiffTag.IMAGELENGTH, _nCh * _height * _width);
                    _output.SetField(TiffTag.ROWSPERSTRIP, _useStrips ? 1 : _nCh * _height * _width);
                }
                else if (_format == FileFormat.ChTime_YX)
                {
                    _output.SetField(TiffTag.IMAGEWIDTH, _totalTime);
                    _output.SetField(TiffTag.IMAGELENGTH, _height * _width);
                    _output.SetField(TiffTag.ROWSPERSTRIP, _useStrips ? 1 : _height * _width);
                }

                _output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                _output.SetField(TiffTag.BITSPERSAMPLE, _depth * 8);
                _output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                _output.SetField(TiffTag.XRESOLUTION, 100.0);
                _output.SetField(TiffTag.YRESOLUTION, 100.0);
                _output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.NONE);
                _output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                _output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                _output.SetField(TiffTag.COMPRESSION, ResolveCompression(_compression));
                _output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
            }

            private string GetHeaderBase()
            {
                if (_headerBase != null)
                    return _headerBase;

                int saveLinesPerFrame = _owner.State.Acq.linesPerFrame;
                int savePixelsPerLine = _owner.State.Acq.pixelsPerLine;
                try
                {
                    if (_owner.State.Acq.linesPerFrame != _height || _owner.State.Acq.pixelsPerLine != _width)
                    {
                        _owner.State.Acq.linesPerFrame = _height;
                        _owner.State.Acq.pixelsPerLine = _width;
                    }

                    _headerBase = _owner.CreateHeader(_saveChannels);
                }
                finally
                {
                    _owner.State.Acq.linesPerFrame = saveLinesPerFrame;
                    _owner.State.Acq.pixelsPerLine = savePixelsPerLine;
                }

                _owner.image_description = _headerBase;
                return _headerBase;
            }

            private void WriteFrameData(ushort[][][,,] pages)
            {
                if (_format == FileFormat.Linear || _format == FileFormat.ZLinear)
                    WriteLinear(pages);
                else if (_format == FileFormat.Time_ChYX)
                    WriteTimeChYX(pages);
                else if (_format == FileFormat.ChTime_YX)
                    WriteChTimeYX(pages);
            }

            private void WriteLinear(ushort[][][,,] pages)
            {
                byte[] buffer = _reuseBuffers ? _linearBuffer : new byte[Math.Max(1, _linearRowBytes)];

                for (int z = 0; z < _nZ; z++)
                {
                    int offset = 0;
                    for (int ch = 0; ch < _nCh; ch++)
                    {
                        int byteCount = _channelByteCounts[ch];
                        if (byteCount <= 0)
                            continue;

                        var data = pages[z][ch];
                        if (data != null)
                            Buffer.BlockCopy(data, 0, buffer, offset, byteCount);
                        else
                            Array.Clear(buffer, offset, byteCount);

                        offset += byteCount;
                    }

                    if (_useStrips)
                        _output.WriteEncodedStrip(z, buffer, buffer.Length);
                    else
                        _output.WriteScanline(buffer, z);
                }
            }

            private void WriteTimeChYX(ushort[][][,,] pages)
            {
                var img = pages[0];
                byte[] buffer = _reuseBuffers ? _scanlineBuffer : new byte[Math.Max(1, _timeChyxRowBytes)];

                for (int ch = 0; ch < _nCh; ch++)
                    for (int y = 0; y < _height; y++)
                        for (int x = 0; x < _width; x++)
                        {
                            var data = img[ch];
                            if (data != null && _timeChyxRowBytes > 0)
                            {
                                Buffer.BlockCopy(data, (y * _width * _nTime[ch] + x * _nTime[ch]) * _depth, buffer, 0, _timeChyxRowBytes);
                            }
                            else
                            {
                                Array.Clear(buffer, 0, buffer.Length);
                            }

                            int row = ch * _width * _height + y * _width + x;
                            if (_useStrips)
                                _output.WriteEncodedStrip(row, buffer, buffer.Length);
                            else
                                _output.WriteScanline(buffer, row);
                        }
            }

            private void WriteChTimeYX(ushort[][][,,] pages)
            {
                var img = pages[0];
                byte[] buffer = _reuseBuffers ? _scanlineBuffer : new byte[Math.Max(1, _chTimeRowBytes)];

                for (int y = 0; y < _height; y++)
                    for (int x = 0; x < _width; x++)
                    {
                        int offset = 0;
                        for (int ch = 0; ch < _nCh; ch++)
                        {
                            int bytes = _nTime[ch] * _depth;
                            if (bytes > 0)
                            {
                                var data = img[ch];
                                if (data != null)
                                    Buffer.BlockCopy(data, (y * _width * _nTime[ch] + x * _nTime[ch]) * _depth, buffer, offset, bytes);
                                else
                                    Array.Clear(buffer, offset, bytes);

                                offset += bytes;
                            }
                        }

                        int row = y * _width + x;
                        if (_useStrips)
                            _output.WriteEncodedStrip(row, buffer, buffer.Length);
                        else
                            _output.WriteScanline(buffer, row);
                    }
            }

            private bool ValidateFrame(ushort[][][,,] pages)
            {
                if (pages == null || pages.Length != _nZ)
                    return false;

                for (int z = 0; z < _nZ; z++)
                {
                    if (pages[z] == null || pages[z].Length != _nCh)
                        return false;
                }

                ushort[,,] first = null;
                for (int c = 0; c < _nCh && first == null; c++)
                {
                    if (pages[0][c] != null)
                        first = pages[0][c];
                }

                if (first != null)
                {
                    if (first.GetLength(0) != _height || first.GetLength(1) != _width)
                        return false;

                    for (int c = 0; c < _nCh; c++)
                    {
                        if (pages[0][c] != null && pages[0][c].GetLength(2) != _nTime[c])
                            return false;
                    }
                }

                return true;
            }

            public bool CanAppendFrame(ushort[][][,,] pages)
            {
                return !_disposed && ValidateFrame(pages);
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _output?.Dispose();
                _disposed = true;
            }
        }

        private static Compression ResolveCompression(FlimCompression compression)
        {
            return compression == FlimCompression.None ? Compression.NONE : Compression.LZW;
        }

        private bool FastFlimEnabled => IsFiberPhotometryMode || (State?.Files?.fastSaving ?? false);

        private bool IsFiberPhotometryMode
        {
            get
            {
                if (State?.Acq?.fiberPhotometryMode == true)
                    return true;

                var system = State?.Init?.MicroscopeSystem;
                return !string.IsNullOrEmpty(system)
                    && system.IndexOf("fiber", StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        public void CloseFlimWriter(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            lock (_flimWriterLock)
            {
                CloseFlimWriterNoLock(fileName);
            }
        }

        public void CloseAllFlimWriters()
        {
            lock (_flimWriterLock)
            {
                foreach (var writer in _flimWriters.Values)
                {
                    writer.Dispose();
                }
                _flimWriters.Clear();
            }
            CloseOMETiff();
        }

        private void CloseFlimWriterNoLock(string fileName)
        {
            if (_flimWriters.TryGetValue(fileName, out var writer))
            {
                writer.Dispose();
                _flimWriters.Remove(fileName);
            }
        }

        public int SaveFLIMInTiffZStack_core(String fileName, UInt16[][][,,] FLIM_Pages, DateTime dt, bool overwrite, bool[] saveChannels)
        //Save two channel images. //Not Safe at all.
        {
            return SaveFLIMInTiffZStack_core(fileName, FLIM_Pages, dt, overwrite, saveChannels, FlimCompression.Lzw);
        }

        public int SaveFLIMInTiffZStack_core(String fileName, UInt16[][][,,] FLIM_Pages, DateTime dt, bool overwrite, bool[] saveChannels, FlimCompression compression)
        //Save two channel images. //Not Safe at all.
        {
            int depth = 2; //Bytes; 16 bit image.
            int error = 0;
            int nZ = FLIM_Pages.Length;
            int nCh = FLIM_Pages[0].Length;

            int height = State.Acq.linesPerFrame;
            int width = State.Acq.pixelsPerLine;

            int[] n_time = new int[nCh];

            // Infer dimensions from actual data when State.Acq doesn't match the buffer (e.g. FiberPhotometry 1x1).
            // This prevents writing an invalid TIFF when height/width in State are larger than the actual array.
            ushort[,,] first = null;
            for (int c = 0; c < nCh && first == null; c++)
            {
                if (FLIM_Pages[0][c] != null)
                    first = FLIM_Pages[0][c];
            }

            if (first != null)
            {
                int h0 = first.GetLength(0);
                int w0 = first.GetLength(1);
                if (h0 > 0 && w0 > 0)
                {
                    // Trust the data array when dimensions don't match the State values.
                    if (height != h0 || width != w0)
                    {
                        height = h0;
                        width = w0;
                    }
                }
            }

            for (int c = 0; c < nCh; c++)
            {
                if (FLIM_Pages[0][c] != null)
                {
                    // Safer than Length / height / width (which can silently become 0 when height/width mismatch).
                    n_time[c] = FLIM_Pages[0][c].GetLength(2);
                }
                else
                {
                    n_time[c] = 0;
                }
            }

            string writeMode;

            string acquiredTime = dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");


            if (overwrite)
                writeMode = "w8";
            else
                writeMode = IsBigTiff(fileName) ? "a8" : "a";

#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            var bimg = new byte[nZ][][];
            bool convert_to_8_bit = false;

            if (convert_to_8_bit)
            {
                int maxValue = 0;
                for (int z = 0; z < nZ; z++)
                {
                    for (int ch = 0; ch < nCh; ch++)
                    {
                        var mv = MatrixCalc.calcMax(FLIM_Pages[z][ch]);
                        if (maxValue < mv)
                            maxValue = (int)mv;
                    }
                }

                if (maxValue < 255)
                {
                    depth = 1;
                    for (int z = 0; z < nZ; z++)
                    {
                        bimg[z] = new byte[nCh][];
                        for (int ch = 0; ch < nCh; ch++)
                        {
                            bimg[z][ch] = MatrixCalc.changeDepthFrom16To8(FLIM_Pages[z][ch]);
                        }
                    }
                }
            }

#if DEBUG
            sw.Stop();
            time_elapsed = (int)sw.ElapsedMilliseconds;
            //Debug.WriteLine("Max value = " + maxValue + ": Time spend = " + sw.ElapsedMilliseconds + "ms");
#endif

            if (overwrite)
                PrepareForOverwrite(fileName);

            using (Tiff output = Tiff.Open(fileName, writeMode))
            {
                if (output == null)
                    return -100;

                bool all_n_time_same = n_time.All(x => x == n_time[0]);

                FileFormat fm = FileFormat.Time_ChYX;

                //fm = FileFormat.ChTime_YX;
                if (nZ == 1)
                    fm = FileFormat.Linear;
                else
                    fm = FileFormat.ZLinear;

                if (fm == FileFormat.Linear || fm == FileFormat.ZLinear)
                {
                    output.SetField(TiffTag.IMAGEWIDTH, n_time.Sum() * height * width);
                    output.SetField(TiffTag.IMAGELENGTH, nZ);
                    output.SetField(TiffTag.ROWSPERSTRIP, nZ);
                }
                else if (fm == FileFormat.Time_ChYX)
                {
                    output.SetField(TiffTag.IMAGEWIDTH, n_time[0]);
                    output.SetField(TiffTag.IMAGELENGTH, nCh * height * width);
                    output.SetField(TiffTag.ROWSPERSTRIP, nCh * height * width);
                }
                else if (fm == FileFormat.ChTime_YX)
                {
                    output.SetField(TiffTag.IMAGEWIDTH, n_time.Sum());
                    output.SetField(TiffTag.IMAGELENGTH, height * width);
                    output.SetField(TiffTag.ROWSPERSTRIP, height * width);
                }
                else if (fm == FileFormat.ChYX_Time)
                {
                    output.SetField(TiffTag.IMAGEWIDTH, nCh * height * width);
                    output.SetField(TiffTag.IMAGELENGTH, n_time[0]);
                    output.SetField(TiffTag.ROWSPERSTRIP, n_time[0]);
                }

                output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                output.SetField(TiffTag.BITSPERSAMPLE, depth * 8);
                output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);

                output.SetField(TiffTag.XRESOLUTION, 100.0);
                output.SetField(TiffTag.YRESOLUTION, 100.0);
                output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.NONE);
                output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                //output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.SEPARATE);
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                output.SetField(TiffTag.COMPRESSION, Compression.LZW);
                output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

                string str1;
                if (overwrite)
                {
                    // IMPORTANT (FiberPhotometry):
                    // The reader trusts the header (IMAGEDESCRIPTION) for width/height. If the header says
                    // 256x256 but the actual TIFF payload is 1x1, OpenFLIM will crash at Buffer.BlockCopy.
                    // So when the actual data dimensions don't match State.Acq, temporarily override State
                    // while generating the header string.
                    int saveLinesPerFrame = State.Acq.linesPerFrame;
                    int savePixelsPerLine = State.Acq.pixelsPerLine;
                    try
                    {
                        if (State.Acq.linesPerFrame != height || State.Acq.pixelsPerLine != width)
                        {
                            State.Acq.linesPerFrame = height;
                            State.Acq.pixelsPerLine = width;
                        }

                        str1 = CreateHeader(saveChannels);
                    }
                    finally
                    {
                        State.Acq.linesPerFrame = saveLinesPerFrame;
                        State.Acq.pixelsPerLine = savePixelsPerLine;
                    }
                    image_description = str1;
                    str1 = str1 + String.Format("Acquired_Time = {0};\r\n", acquiredTime);
                }
                else
                {
                    str1 = String.Format("Acquired_Time = {0};\r\n", acquiredTime);
                }

                str1 = str1 + String.Format("Format = {0};\r\n", fm);

                output.SetField(TiffTag.IMAGEDESCRIPTION, str1);

                if (fm == FileFormat.Linear || fm == FileFormat.ZLinear)
                {
                    for (int z = 0; z < nZ; z++)
                    {
                        var img = FLIM_Pages;

                        byte[] buf = new byte[n_time.Sum() * width * height * depth];
                        int offset = 0;
                        for (int chnnl = 0; chnnl < nCh; ++chnnl)
                        {
                            if (n_time[chnnl] > 0)
                            {
                                if (depth == 2)
                                    Buffer.BlockCopy(img[z][chnnl], 0, buf, offset, img[z][chnnl].Length * depth);
                                else
                                    Buffer.BlockCopy(bimg[z][chnnl], 0, buf, offset, bimg[z][chnnl].Length);
                            }
                            offset = offset + n_time[chnnl] * width * height * depth;
                        }

                        output.WriteScanline(buf, z);
                    }
                }
                else if (fm == FileFormat.Time_ChYX)
                {
                    var img = FLIM_Pages[0];

                    for (int chnnl = 0; chnnl < nCh; ++chnnl)
                        for (int y = 0; y < height; ++y)
                            for (int x = 0; x < width; ++x)
                            {
                                byte[] buf = new byte[n_time[0] * depth];
                                if (depth == 2)
                                    Buffer.BlockCopy(img[chnnl], (y * width * n_time[chnnl] + x * n_time[chnnl]) * depth, buf, 0, buf.Length);
                                else
                                    Buffer.BlockCopy(bimg[chnnl], (y * width * n_time[chnnl] + x * n_time[chnnl]) * depth, buf, 0, buf.Length);

                                output.WriteScanline(buf, chnnl * width * height + y * width + x);
                            }
                }
                else if (fm == FileFormat.ChTime_YX)
                {
                    var img = FLIM_Pages[0];

                    for (int y = 0; y < height; ++y)
                        for (int x = 0; x < width; ++x)
                        {
                            byte[] buf = new byte[n_time.Sum() * depth];
                            int offset = 0;
                            for (int c = 0; c < nCh; ++c)
                            {
                                if (n_time[c] > 0)
                                {
                                    if (depth == 2)
                                        Buffer.BlockCopy(img[c], (y * width * n_time[c] + x * n_time[c]) * depth, buf, offset, n_time[c] * depth);
                                    else
                                        Buffer.BlockCopy(bimg[c], (y * width * n_time[c] + x * n_time[c]) * depth, buf, offset, n_time[c] * depth);

                                    offset = offset + n_time[c] * depth;
                                }
                            }

                            output.WriteScanline(buf, y * width + x);

                        }
                }

                output.WriteDirectory();
            } //TIF

            if (error != 0)
                Debug.WriteLine("Error " + error);

            return error;

        }//saveTiff

        #region Save OME Tiff functions
        public int CreateOMEFile(string filename)
        {
            var compression = FastFlimEnabled
                ? OmeTiffLibraryWrapper.CompressionMode.COMPRESSIONMODE_NONE
                : OmeTiffLibraryWrapper.CompressionMode.COMPRESSIONMODE_LZW;
            return ome_open_file(filename, OpenMode.CREATE_MODE, compression);
        }

        public int ConfigureOMEHeader(int handle, float width, float height, float topLeftCenterOffsetX, float topLeftCenterOffsetY,
            int regionPixelX, int regionPixelY, int zCount, int tCount, int bitsPerPixel, float regionPixelSizeXUM, float regionPixelSizeYUM,
            float zStepSizeUM, float intervalSec, bool[] saveChannels, uint binCount)
        {
            PlateInfo plate = new PlateInfo
            {
                Id = 0,
                Name = "Plate_0",
                Width = width,
                Height = height,
                PhysicalSizeUnitX = DistanceUnit.DISTANCE_MILLIMETER,
                PhysicalSizeUnitY = DistanceUnit.DISTANCE_MILLIMETER,
                RowSize = 1,
                ColumnSize = 1,
            };

            var ret = ome_add_plate(handle, plate);
            if (ret < 0)
            {
                ome_close_file(handle);
                return ret;
            }

            WellInfo well = new WellInfo
            {
                Id = 0,
                WellShape = Shape.SHAPE_RECTANGLE,
                Width = width,
                Height = height,
                RowIndex = 0,
                ColumnIndex = 0,
                PositionX = topLeftCenterOffsetX,
                PositionY = topLeftCenterOffsetY,
            };
            ret = ome_add_well(handle, plate.Id, well);
            if (ret < 0)
            {
                ome_close_file(handle);
                return ret;
            }

            ScanInfo scan = new ScanInfo
            {
                Id = 0,
                PixelPhysicalSizeX = regionPixelSizeXUM,
                PixelPhysicalSizeY = regionPixelSizeYUM,
                PixelPhysicalSizeZ = zStepSizeUM,
                PixelPhysicalUnitX = DistanceUnit.DISTANCE_MICROMETER,
                PixelPhysicalUnitY = DistanceUnit.DISTANCE_MICROMETER,
                PixelPhysicalUnitZ = DistanceUnit.DISTANCE_MICROMETER,
                TimeIncrement = intervalSec,
                TimeIncrementUnit = TimeUnit.TIME_SECOND,
                DimensionOrder = "XYCZT",
                TilePixelSizeWidth = (uint)regionPixelX,
                TilePixelSizeHeight = (uint)regionPixelY,
                PixelType = PixelType.PIXEL_UINT16,
                SignificatBits = (uint)bitsPerPixel,
            };

            ret = ome_add_scan(handle, plate.Id, scan);
            if (ret < 0)
            {
                ome_close_file(handle);
                return ret;
            }

            if (saveChannels != null)
            {
                for (int i = 0; i < saveChannels.Length; i++)
                {
                    if (!saveChannels[i])
                        continue;

                    ChannelInfo channel = new ChannelInfo
                    {
                        Id = (uint)i,
                        Name = "Channel_" + i,
                        SamplesPerPixel = 1,
                        BinSize = binCount,
                    };
                    ret = ome_add_channel(handle, plate.Id, scan.Id, channel);
                    if (ret < 0)
                    {
                        ome_close_file(handle);
                        return ret;
                    }
                }
            }

            ScanRegionInfo region = new ScanRegionInfo
            {
                Id = 0,
                PixelSizeX = (uint)regionPixelX,
                PixelSizeY = (uint)regionPixelY,
                PixelSizeZ = (uint)zCount,
                SizeT = (uint)tCount,
                StartPhysicalX = well.PositionX * 1000,
                StartPhysicalY = well.PositionY * 1000,
                StartPhysicalZ = 0,
                StartUnitX = DistanceUnit.DISTANCE_MICROMETER,
                StartUnitY = DistanceUnit.DISTANCE_MICROMETER,
                StartUnitZ = DistanceUnit.DISTANCE_MICROMETER,
            };

            ret = ome_add_scan_region(handle, plate.Id, scan.Id, well.Id, region);
            if (ret < 0)
            {
                ome_close_file(handle);
                return ret;
            }

            return 0;
        }

        public int SaveOMEData(int handle, int channelID, uint zIndex, uint tIndex, IntPtr data)
        {
            int ret;
            if (!omeFrameInfoInitialized)
            {
                ret = InitFrameInfo(handle);
                if (ret < 0)
                    return ret;
                omeFrameInfoInitialized = true;
            }

            FrameInfo curFrame = frameInfo;
            curFrame.c_id = (uint)channelID;
            curFrame.z_id = zIndex;
            curFrame.t_id = tIndex;

            ret = ome_save_tile_data(handle, data, curFrame, 0, 0);
            if (ret < 0)
                return ret;

            return 0;
        }

        public int SetOMETag(int handle, FrameInfo frame, ushort tag_id, TiffTagDataType tag_type, int tag_size, IntPtr tag_value)
        {
            if (handle < 0)
                return -1;

            return ome_set_tag(handle, frame, tag_id, tag_type, (uint)tag_size, tag_value);
        }

        public static int GetOMETag(int handle, int z, int c, ushort tag_id, TiffTagDataType tag_type, out String tag_value)
        {
            tag_value = "";
            var ret = InitFrameInfo(handle);
            if (ret < 0)
                return ret;

            FrameInfo curFrame = frameInfo;
            curFrame.c_id = (uint)c;
            curFrame.z_id = (uint)z;

            uint tag_count = 0;
            ret = ome_get_tag(handle, curFrame, tag_id, ref tag_type, ref tag_count, IntPtr.Zero);
            if (ret < 0)
                return ret;

            int tag_type_size = GetTagTypeSize(tag_type);
            IntPtr ptr = Marshal.AllocHGlobal((int)tag_count * tag_type_size * sizeof(byte));
            ret = ome_get_tag(handle, curFrame, tag_id, ref tag_type, ref tag_count, ptr);
            if (ret < 0)
            {
                Marshal.FreeHGlobal(ptr);
                return ret;
            }

            tag_value = Marshal.PtrToStringAnsi(ptr, (int)tag_count * tag_type_size);
            Marshal.FreeHGlobal(ptr);

            return 0;
        }

        public static int GetOMEHeaderTag(string filename, ushort tag_id, out String tag_value)
        {
            tag_value = "";
            var handle = ome_open_file(filename, OpenMode.READ_ONLY_MODE);
            if (handle < 0)
                return -1;
            try
            {
                var ret = InitFrameInfo(handle);
                if (ret < 0)
                    return ret;

                FrameInfo curframe = frameInfo;
                curframe.plate_id = uint.MaxValue;

                uint tag_count = 0;
                TiffTagDataType tag_type = 0;
                ret = ome_get_tag(handle, curframe, tag_id, ref tag_type, ref tag_count, IntPtr.Zero);
                if (ret < 0)
                    return ret;

                int tag_type_size = GetTagTypeSize(tag_type);
                IntPtr ptr = Marshal.AllocHGlobal((int)tag_count * tag_type_size * sizeof(byte));
                ret = ome_get_tag(handle, curframe, tag_id, ref tag_type, ref tag_count, ptr);
                if (ret < 0)
                {
                    Marshal.FreeHGlobal(ptr);
                    return ret;
                }

                tag_value = Marshal.PtrToStringAnsi(ptr, (int)tag_count * tag_type_size);
                Marshal.FreeHGlobal(ptr);

                return ret;
            }
            finally
            {
                ome_close_file(handle);
            }
        }

        private static int GetTagTypeSize(TiffTagDataType tag_type)
        {
            switch (tag_type)
            {
                case TiffTagDataType.TIFF_ASCII:
                case TiffTagDataType.TIFF_BYTE:
                case TiffTagDataType.TIFF_SBYTE:
                case TiffTagDataType.TIFF_UNDEFINED:
                    return 1;
                case TiffTagDataType.TIFF_SHORT:
                case TiffTagDataType.TIFF_SSHORT:
                    return 2;
                case TiffTagDataType.TIFF_LONG:
                case TiffTagDataType.TIFF_SLONG:
                case TiffTagDataType.TIFF_FLOAT:
                case TiffTagDataType.TIFF_IFD:
                    return 4;
                case TiffTagDataType.TIFF_RATIONAL:
                case TiffTagDataType.TIFF_SRATIONAL:
                case TiffTagDataType.TIFF_DOUBLE:
                case TiffTagDataType.TIFF_LONG8:
                case TiffTagDataType.TIFF_SLONG8:
                case TiffTagDataType.TIFF_IFD8:
                    return 8;
                default:
                    return 0;
            }
        }

        public static int InitFrameInfo(int handle)
        {
            if (!TryGetOmeRegion(handle, out PlateInfo plate, out WellInfo well, out ScanInfo scan, out ScanRegionInfo region))
                return -1;

            frameInfo.plate_id = plate.Id;
            frameInfo.scan_id = scan.Id;
            frameInfo.region_id = region.Id;
            frameInfo.c_id = 0;
            frameInfo.z_id = 0;
            frameInfo.t_id = 0;

            return 0;
        }

        public int CreateAndConfigureOMETiff(string fileName, bool[] saveChannels, string header, int zCount, int tCount, int width, int height)
        {
            currentZ = 0;
            totalZCount = Math.Max(1, zCount) * Math.Max(1, tCount);
            omeFrameInfoInitialized = false;
            ReleaseOmeChannelBuffers();
            omeHandle = CreateOMEFile(fileName);

            if (omeHandle < 0)
                return -1;

            int depth = 2;
            float widthMM = (float)State.Acq.field_of_view[0] / 1000;
            float heightMM = (float)State.Acq.field_of_view[1] / 1000;
            float topLeftCenterOffsetX = (float)State.Acq.XOffset / 1000;
            float topLeftCenterOffsetY = (float)State.Acq.YOffset / 1000;
            int regionPixelX = width;
            int regionPixelY = height;
            int bitsPerPixel = depth * 8;
            int timeCount = Math.Max(1, tCount);
            float regionPixelSizeXUM = (float)State.Acq.field_of_view[0] / regionPixelX;
            float regionPixelSizeYUM = (float)State.Acq.field_of_view[1] / regionPixelY;
            float zStepSizeUM = State.Acq.fastZScan ? (float)State.Acq.FastZ_umPerSlice : (float)State.Acq.sliceStep;
            float intervalSec = (float)State.Acq.sliceInterval;
            uint binCount = (uint)State.Spc.spcData.n_dataPoint;

            var ret = ConfigureOMEHeader(omeHandle, widthMM, heightMM, topLeftCenterOffsetX, topLeftCenterOffsetY, regionPixelX, regionPixelY,
                zCount, timeCount, bitsPerPixel, regionPixelSizeXUM, regionPixelSizeYUM, zStepSizeUM, intervalSec, saveChannels, binCount);

            if (ret < 0)
            {
                CloseOMETiff();
                return ret;
            }

            image_description = header;
            omeFrameInfoInitialized = InitFrameInfo(omeHandle) == 0;

            return 0;
        }

        private int GetAcquisitionOmeTiffFrameCount()
        {
            bool averageFrames = State.Acq.aveFrame || (State.Acq.aveFrameA != null && State.Acq.aveFrameA.Any(v => v));
            int effectiveFrames = State.Acq.nFrames > 0 ? State.Acq.nFrames : 1;
            if (averageFrames)
            {
                if (State.Acq.nAveragedFrames > 0)
                {
                    effectiveFrames = State.Acq.nAveragedFrames;
                }
                else if (State.Acq.nAveFrame > 1 && State.Acq.nFrames > 0)
                {
                    effectiveFrames = (int)Math.Ceiling((double)State.Acq.nFrames / State.Acq.nAveFrame);
                }
            }

            bool averageSlices = averageFrames && State.Acq.nAveSlice > 1;
            int effectiveSlices = State.Acq.nSlices > 0 ? State.Acq.nSlices : 1;
            if (averageSlices && State.Acq.nAveragedSlices > 0)
                effectiveSlices = State.Acq.nAveragedSlices;

            long totalPagesLong = (long)Math.Max(1, effectiveFrames) * Math.Max(1, effectiveSlices);
            return totalPagesLong > int.MaxValue ? int.MaxValue : (int)totalPagesLong;
        }

        public int CloseOMETiff()
        {
            int ret = 0;
            if (omeHandle >= 0)
                ret = ome_close_file(omeHandle);

            omeHandle = -1;
            currentZ = 0;
            totalZCount = 0;
            omeFileName = "";
            omeFrameInfoInitialized = false;
            ReleaseOmeChannelBuffers();

            return ret;
        }

        private void EnsureOmeChannelBuffers(int channelCount, int height, int width, int[] n_time, int depth)
        {
            if (channelCount <= 0 || n_time == null || n_time.Length == 0)
                return;

            if (omeChannelBuffers == null || omeChannelBuffers.Length != channelCount)
            {
                ReleaseOmeChannelBuffers();
                omeChannelBuffers = new byte[channelCount][];
                omeChannelHandles = new GCHandle[channelCount];
                omeChannelPointers = new IntPtr[channelCount];
                omeChannelBufferBytes = new int[channelCount];
            }

            for (int c = 0; c < channelCount; c++)
            {
                int nTime = c < n_time.Length ? n_time[c] : 0;
                if (nTime <= 0 || height <= 0 || width <= 0)
                {
                    ReleaseOmeChannelBuffer(c);
                    continue;
                }

                long bytesLong = (long)height * width * nTime * depth;
                if (bytesLong > int.MaxValue)
                {
                    ReleaseOmeChannelBuffer(c);
                    continue;
                }

                int bytes = (int)bytesLong;
                if (omeChannelBuffers[c] == null || omeChannelBufferBytes[c] != bytes)
                {
                    ReleaseOmeChannelBuffer(c);
                    omeChannelBuffers[c] = new byte[bytes];
                    omeChannelBufferBytes[c] = bytes;
                    omeChannelHandles[c] = GCHandle.Alloc(omeChannelBuffers[c], GCHandleType.Pinned);
                    omeChannelPointers[c] = omeChannelHandles[c].AddrOfPinnedObject();
                }
            }
        }

        private void ReleaseOmeChannelBuffers()
        {
            if (omeChannelHandles != null)
            {
                for (int i = 0; i < omeChannelHandles.Length; i++)
                {
                    if (omeChannelHandles[i].IsAllocated)
                        omeChannelHandles[i].Free();
                }
            }

            omeChannelBuffers = null;
            omeChannelHandles = null;
            omeChannelPointers = null;
            omeChannelBufferBytes = null;
        }

        private void ReleaseOmeChannelBuffer(int index)
        {
            if (omeChannelHandles != null && index >= 0 && index < omeChannelHandles.Length)
            {
                if (omeChannelHandles[index].IsAllocated)
                    omeChannelHandles[index].Free();
            }

            if (omeChannelBuffers != null && index >= 0 && index < omeChannelBuffers.Length)
                omeChannelBuffers[index] = null;
            if (omeChannelPointers != null && index >= 0 && index < omeChannelPointers.Length)
                omeChannelPointers[index] = IntPtr.Zero;
            if (omeChannelBufferBytes != null && index >= 0 && index < omeChannelBufferBytes.Length)
                omeChannelBufferBytes[index] = 0;
        }
        #endregion

        public String SaveFLIMFileDialog(String TempFileName)
        {
            return SaveFLIMFileDialog(TempFileName, Path.GetExtension(TempFileName));
        }

        public String SaveFLIMFileDialog(String TempFileName, String extension)
        {
            String filename = "";
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.InitialDirectory = Path.GetDirectoryName(TempFileName);
            saveFileDialog1.FileName = Path.GetFileName(TempFileName);
            saveFileDialog1.Filter = String.Format("Image files (*{0})|*{0}|All files (*.*)|*.*", extension);
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = false;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    filename = saveFileDialog1.FileName;
                    //State.Files.initFileName = filename;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file to disk. Original error: " + ex.Message);
                }
            }

            return filename;
        }


        public void SaveCVSFile(string text, string FileName)
        {
            //Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.InitialDirectory = State.Files.pathName;
            saveFileDialog1.FileName = FileName;
            saveFileDialog1.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = false;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    String fileName = saveFileDialog1.FileName;
                    State.Files.initFileName = fileName;
                    File.WriteAllText(fileName, text);
                    Debug.WriteLine("Writing ini file in...." + fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file to disk. Original error: " + ex.Message);
                }
            }
        }


        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="fileName"></param>
        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //Log exception here
            }
        }


        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                string attributeXml = string.Empty;

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return objectOut;
        }

        public enum FileFormat
        {
            Time_ChYX = 0,
            ChYX_Time = 1,
            ChTime_YX = 2,
            Linear = 3,
            ZLinear = 4,
            None = 5,
            Photon_file = 10,
        }

        public enum FlimCompression
        {
            Lzw = 0,
            None = 1
        }

        public enum FileError
        {
            Success = 0,
            NotFound = 1,
            Canceled = 2,
            UnKnown = 3,
            FormatError = 4,
            TextFile = 5,
        }

        public enum ImageType
        {
            FLIMRaw = 0,
            Intensity = 1,
            FLIM_color = 2,
        }
    }
}
