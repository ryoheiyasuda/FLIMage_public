using BitMiracle.LibTiff.Classic;
using MathLibrary;
using Utilities;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace FLIMage
{
    public class FileIO
    {

        //public List<String> headerList;
        public List<String> headerList_nonDevice;
        public List<String> headerDevice;
        List<String> headerList_all;
        public ScanParameters State;

        public String image_description;

        int time_elapsed = 0;

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
            return obj;
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


        public void LoadSetupFile(String fileName)
        {
            string saveDeviceSettinFileName = State.Files.deviceFileName;
            string saveInitFilePath = State.Files.initFolderPath;
            string saveDefaultInitFile = State.Files.defaultInitFile;

            if (File.Exists(fileName))
            {
                using (StreamReader sr = File.OpenText(fileName))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        try
                        {
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

        public String FLIM_FilePath(int ch, bool ChannelsInSeparatedFile, int counter, ImageType image_type, string ProjectionTypeString, string dirPath)
        {
            if (ch >= State.Acq.nChannels || ch < 0)
                ch = 0;

            String fileNameWithoutPath;

            if (ChannelsInSeparatedFile)
            {
                if (State.Files.numberedFile)
                    fileNameWithoutPath = String.Format("{0}_Ch{1}_{2:000}{3}.tif", State.Files.baseName, ch + 1, counter, ProjectionTypeString);
                else
                    fileNameWithoutPath = String.Format("{0}_Ch{1}{2}.tif", State.Files.baseName, ch + 1, ProjectionTypeString);
            }
            else
            {
                if (State.Files.numberedFile)
                    fileNameWithoutPath = String.Format("{0}{1:000}{2}.tif", State.Files.baseName, counter, ProjectionTypeString);
                else
                    fileNameWithoutPath = String.Format("{0}{1}.tif", State.Files.baseName, ProjectionTypeString);
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

                if (!datatype.Equals(typeof(String)))
                {
                    valueStr = Regex.Replace(valueStr, @"\s+", "");
                    valueStr = valueStr.Replace("\"", "").Replace(";", "").Replace("{", "").Replace("}", "");
                    valueStr = valueStr.Replace("[", "").Replace("]", "");
                }
                //Debug.WriteLine(fieldName + "=" + valueStr + "----");

                if (datatype.Equals(typeof(Double[])))
                {
                    String[] valueArray = valueStr.Split(',');
                    Double[] valArray = (Double[])member.GetValue(obj);
                    if (valueArray.Length == 0)
                    {
                        valueArray = new string[1];
                        valueArray[0] = valueStr;
                    }
                    int arrayL = Math.Max(valArray.Length, valueArray.Length);
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
                    String[] valueArray = valueStr.Split(',');
                    Int32[] valArray = (Int32[])member.GetValue(obj);
                    if (valueArray.Length == 0)
                    {
                        valueArray = new string[1];
                        valueArray[0] = valueStr;
                    }
                    int arrayL = Math.Max(valArray.Length, valueArray.Length);
                    Int32[] values = new Int32[arrayL];
                    for (int i = 0; i < valueArray.Length; i++)
                    {
                        values[i] = Convert.ToInt32(valueArray[i]);
                    }
                    if (overwrite)
                        member.SetValue(obj, values);

                    valobj = (object)values;
                }
                else if (datatype.Equals(typeof(bool[])))
                {
                    String[] valueArray = valueStr.Split(',');
                    bool[] valArray = (bool[])member.GetValue(obj);
                    if (valueArray.Length == 0)
                    {
                        valueArray = new string[1];
                        valueArray[0] = valueStr;
                    }
                    int arrayL = Math.Max(valArray.Length, valueArray.Length);
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
                return "";

            String strVal = value.ToString();

            if (type.Equals(typeof(String)))
            {
                strVal = "\"" + strVal + "\"";
            }
            else if (type.Equals(typeof(Double[])))
            {
                strVal = arrayStart;
                Double[] valArray = (Double[])memberInfo.GetValue(obj);
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


                //type2 = obj2.GetType();
                ////members2 = type2.GetFields();

                //for (int j = 0; j < members.Length; j++)
                //{
                //    var value = members[j].GetValue(obj);
                //    var valuetype = members[j].FieldType;
                //    if (value != null)
                //    {
                //        if (valuetype == typeof(int[]))
                //        {
                //            var valA = (int[])value;
                //            value = valA.Clone();
                //        }
                //        else if (valuetype == typeof(bool[]))
                //        {
                //            var valA = (bool[])value;
                //            value = valA.Clone();
                //        }
                //        else if (valuetype == typeof(double[]))
                //        {
                //            var valA = (double[])value;
                //            value = valA.Clone();
                //        }

                //        type2.GetField(members[j].Name).SetValue(obj2, value);
                //        //members2[j].SetValue(obj2, value);
                //    }
                //}
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


        /// <summary>
        /// Export images in TIFF file format. --- If FLIM5D, it will save the current page.
        /// </summary>
        /// <param name="FLIM"></param>
        /// <param name="SaveChannelsInSeparatedFile"></param>
        /// <param name="export_ProjectionFile"></param>
        public void SaveIntensityImageInTiff(FLIMData FLIM, bool SaveChannelsInSeparatedFile, bool export_ProjectionFile)
        {

            FLIM.State.Files.fullName();
            double[] intensity_range = new double[FLIM.nChannels];
            double[] FLIM_range = new double[FLIM.nChannels];

            bool[] saveChannels = new bool[FLIM.nChannels];
            bool[] saveChannelsFLIM = new bool[FLIM.nChannels];

            if (!SaveChannelsInSeparatedFile)
                for (int ch = 0; ch < FLIM.nChannels; ch++)
                {
                    if (FLIM.Project[ch] != null)
                        saveChannels[ch] = true;

                    if (FLIM.LifetimeMapF != null && FLIM.LifetimeMapF[ch] != null)
                        saveChannelsFLIM[ch] = true;
                }

            if (export_ProjectionFile)
            {
                for (int ch = 0; ch < FLIM.nChannels; ch++)
                {
                    String filePathIntensityMax = FLIM_FilePath(ch, SaveChannelsInSeparatedFile, FLIM.State.Files.fileCounter, ImageType.Intensity, FLIM.z_projection_type.ToString(), FLIM.pathName);
                    String filePathFLIMMax = FLIM_FilePath(ch, SaveChannelsInSeparatedFile, FLIM.State.Files.fileCounter, ImageType.FLIM_color, FLIM.z_projection_type.ToString(), FLIM.pathName);

                    if (ch == 0)
                    {
                        intensity_range = FLIM.State.Display.FLIM_Intensity_Range1;
                        FLIM_range = FLIM.State.Display.FLIM_Range1;
                    }
                    else
                    {
                        intensity_range = FLIM.State.Display.FLIM_Intensity_Range2;
                        FLIM_range = FLIM.State.Display.FLIM_Range2;
                    }


                    Debug.WriteLine("Saving intensity in...:" + filePathIntensityMax);

                    bool overwrite = SaveChannelsInSeparatedFile || ch == 0;

                    if (FLIM.Project[ch] != null)
                    {
                        if (SaveChannelsInSeparatedFile)
                            saveChannels[ch] = true;

                        Save2DImageInTiff(filePathIntensityMax, FLIM.Project[ch], FLIM.acquiredTime, overwrite, saveChannels); //Save one page.
                    }


                    if (FLIM.FLIM_on.Any(x => x == true))
                    {
                        if (FLIM.ZProjection)
                        {
                            Debug.WriteLine("Saving FLIM in...:" + filePathFLIMMax);

                            overwrite = SaveChannelsInSeparatedFile || ch == 0;
                            FLIM.filterMAP(FLIM.State.Display.filterWindow_FLIM, ch); //Subract and filter.

                            if (FLIM.LifetimeMapF[ch] != null)
                            {
                                if (SaveChannelsInSeparatedFile)
                                    saveChannels[ch] = true;

                                Bitmap FLIMBitmap = ImageProcessing.FormatImageFLIM(FLIM.width, FLIM.height, intensity_range, FLIM_range, FLIM.LifetimeMapF[ch], FLIM.ProjectF[ch], false);
                                SaveColorImageInTiff(filePathFLIMMax, FLIMBitmap, FLIM.acquiredTime, overwrite, saveChannelsFLIM); //Save one page.
                            }
                        }
                    }

                    if (SaveChannelsInSeparatedFile)
                        saveChannels[ch] = false;

                }
            } //Projection file


            //int n_page5D = FLIM.nFastZ > 1 ? FLIM.n_pages5D : 1;

            int channelRepeat = 1;
            if (SaveChannelsInSeparatedFile)
            {
                channelRepeat = FLIM.nChannels;
            }


            if (FLIM.FLIM_on.Any(x => x == true))
                for (int chR = 0; chR < channelRepeat; chR++) //for different channel.
                {
                    String filePathFLIM = FLIM_FilePath(chR, SaveChannelsInSeparatedFile, FLIM.State.Files.fileCounter, ImageType.FLIM_color, "", FLIM.pathName);

                    if (SaveChannelsInSeparatedFile)
                        saveChannels[chR] = true;

                    //for (int page5D = 0; page5D < n_page5D; page5D++)
                    //{
                    //    if (FLIM.nFastZ > 1 && FLIM.FLIM_Pages5D[page5D] != null)
                    //        FLIM.gotoPage5D(page5D);

                    for (int page = 0; page < FLIM.n_pages; page++)
                    {
                        int startCh = 0;
                        int endCh = FLIM.nChannels;
                        if (SaveChannelsInSeparatedFile)
                        {
                            startCh = chR;
                            endCh = chR;
                        }

                        for (int ch = startCh; ch < endCh; ch++)
                        {
                            bool channel_overwrite = (SaveChannelsInSeparatedFile || ch == 0);

                            if (ch == 0)
                            {
                                intensity_range = FLIM.State.Display.FLIM_Intensity_Range1;
                                FLIM_range = FLIM.State.Display.FLIM_Range1;
                            }
                            else
                            {
                                intensity_range = FLIM.State.Display.FLIM_Intensity_Range2;
                                FLIM_range = FLIM.State.Display.FLIM_Range2;
                            }

                            //bool overwrite = (page5D == 0 && page == 0 && channel_overwrite);
                            bool overwrite = (page == 0 && channel_overwrite);

                            overwrite = (page == 0 && channel_overwrite);

                            if (overwrite)
                                Debug.WriteLine("Saving FLIM in...:" + filePathFLIM);
                            //FLIM.gotoPage(page, false);

                            if (FLIM.LifetimeMapF[ch] != null)
                            {
                                saveChannelsFLIM[ch] = true;
                                FLIM.filterMAP(FLIM.State.Display.filterWindow_FLIM, ch);
                                Bitmap FLIMBitmap = ImageProcessing.FormatImageFLIM(FLIM.width, FLIM.height, intensity_range, FLIM_range, FLIM.LifetimeMapF[ch], FLIM.ProjectF[ch], false);
                                SaveColorImageInTiff(filePathFLIM, FLIMBitmap, FLIM.acquiredTime, overwrite, saveChannelsFLIM); //Save one page.
                            }

                        }//ch
                    }//page
                     //} //5dpage

                    if (SaveChannelsInSeparatedFile)
                        saveChannels[chR] = false;
                }//ChannelRepeat.
            //Done FLIM.

            for (int chR = 0; chR < channelRepeat; chR++)
            {
                if (SaveChannelsInSeparatedFile)
                    saveChannels[chR] = true;

                String filePathIntensity = FLIM_FilePath(chR, SaveChannelsInSeparatedFile, FLIM.State.Files.fileCounter, ImageType.Intensity, "", FLIM.pathName);
                //for (int page5D = 0; page5D < n_page5D; page5D++)
                //{
                //    if (FLIM.nFastZ > 1 && FLIM.FLIM_Pages5D[page5D] != null)
                //        FLIM.gotoPage5D(page5D);

                for (int page = 0; page < FLIM.n_pages; page++)
                {
                    int startCh = 0;
                    int endCh = FLIM.nChannels;
                    if (SaveChannelsInSeparatedFile)
                    {
                        startCh = chR;
                        endCh = chR;
                    }

                    for (int ch = startCh; ch < endCh; ch++)
                    {
                        bool channel_overwrite = (SaveChannelsInSeparatedFile || ch == 0);


                        FLIM.gotoPage(page);

                        //bool overwrite = (page5D == 0 && page == 0 && channel_overwrite);
                        bool overwrite = (page == 0 && channel_overwrite);

                        if (FLIM.Project[ch] != null)
                        {
                            saveChannels[ch] = true;
                            Save2DImageInTiff(filePathIntensity, FLIM.Project[ch], FLIM.acquiredTime, overwrite, saveChannels); //Save one page.
                        }

                    } //page
                      //} //5Dpage
                }//

                if (SaveChannelsInSeparatedFile)
                    saveChannels[chR] = false;
            } //channel.
        }//function


        public static FileError OpenTifFileDialog(String defaultPath, out String fileName)
        {
            FileError file_error = FileError.Success;
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = defaultPath; // State.Files.pathName;
            openFileDialog1.FileName = "FLIM.flim";
            openFileDialog1.Filter = "flim file (*.flim, *.tif)|*.flim; *.tif|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            fileName = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
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
            return file_error;
        }



        public static FileError SetupFLIMOpeningDlog(String defaultPath, out int nPages, out String fileName)
        {
            nPages = 0;
            string Header = "";
            FileError file_error = OpenTifFileDialog(defaultPath, out fileName);
            if (file_error == FileError.Success)
            {
                nPages = SetupFLIMOpening(fileName, out Header);
            }
            return file_error;
        }

        public static int SetupFLIMOpening(String fileName, out String Header)
        {
            int nPages;
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
                return nPages;
            }
        }

        public static FileError OpenFLIMTiffFilePage(String fileName, short read_page, int into_page, FLIMData FLIM, bool newFile, bool SavePagesInMemory)
        {
            if (String.Compare(fileName, "") == 0)
            {
                return FileError.NotFound;
            }

            using (Tiff image = Tiff.Open(fileName, "r"))
            {
                if (image == null)
                {
                    //MessageBox.Show("Could not open this image");
                    return FileError.UnKnown;
                }

                int nPages = image.NumberOfDirectories();

                if (read_page < nPages)
                    image.SetDirectory(read_page);

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
                        if (read_page == 0 && newFile)
                        {
                            FLIM.decodeHeader(Description, fileName);
                            dt = FLIM.acquiredTime;
                        }
                        else
                            dt = FLIM.decodeAcquiredTimeOnly(Description);
                    }

                    int height = FLIM.height;
                    int nCh = FLIM.nChannels;
                    int width = FLIM.width;
                    int nfastZ = FLIM.nFastZ;

                    FileFormat fm = FLIM.format;

                    if (fm == FileFormat.None)
                    {
                        if (widthAll == FLIM.n_time.Sum() && image_length == width * height)
                            fm = FileFormat.ChTime_YX;
                        else if (image_length == FLIM.n_time[0] && widthAll == width * height * nCh) //This can happen only if all channels have the same t.
                            fm = FileFormat.ChYX_Time;
                        else if (image_length == width * height * nCh && widthAll == FLIM.n_time[0]) //This can happen only if all channels have the same t.
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

                    int zPerFile = 1;
                    if (fm == FileFormat.ZLinear)
                        zPerFile = image_length;

                    if (height < 1 || nCh < 0)
                        return FileError.FormatError;

                    FLIM.AssureFLIMRawSize();
                    //var img = FLIM.FLIMRaw;
                    //Making linear model.

                    var imgZ = new ushort[zPerFile][][];
                    var bimgZ = new byte[zPerFile][][];

                    for (int z = 0; z < zPerFile; z++)
                    {
                        imgZ[z] = new ushort[nCh][];
                        bimgZ[z] = new byte[nCh][];
                        if (depth == 2)
                            for (int i = 0; i < nCh; i++)
                                imgZ[z][i] = new ushort[height * width * n_time[i]];

                        else if (depth == 1)
                            for (int i = 0; i < nCh; i++)
                                bimgZ[z][i] = new byte[height * width * n_time[i]];
                    }

                    if (fm == FileFormat.Linear || fm == FileFormat.ZLinear)
                    {
                        for (int z = 0; z < zPerFile; z++)
                        {
                            var img = imgZ[z];
                            var bimg = bimgZ[z];

                            image.ReadScanline(scanline, z);

                            int offset = 0;
                            for (int chnnl = 0; chnnl < nCh; ++chnnl)
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
                                for (int chnnl = 0; chnnl < nCh; ++chnnl)
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
                        for (int chnnl = 0; chnnl < nCh; ++chnnl)
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

                            //byte[] buf = new byte[height * nCh * width * depth]; //Do line-by-line!

                            for (int y = 0; y < height; ++y)
                            {
                                for (int x = 0; x < width; ++x)
                                {
                                    //Ch1
                                    for (int chnnl = 0; chnnl < nCh; ++chnnl)
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
                            FLIM.n_pages = nPages; //put in page first.
                        else
                            FLIM.n_pages = zPerFile; //put all files in page.
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

                    for (int z = 0; z < bimgZ.Length; z++)
                        for (int ch = 0; ch < nCh; ch++)
                            if (n_time[ch] == 0)
                            {
                                imgZ[z][ch] = null;
                            }

                    FLIM.imagesPerFile = zPerFile;
                    if (zPerFile == 1)
                    {
                        FLIM.PutToPageOnly_Linear(imgZ[0], dt, into_page);
                    }
                    else
                    {
                        FLIM.Add_AllFLIM_PageFormat_To_FLIM_Pages5D(imgZ, dt, into_page);
                    }

                } //COMPRESSION
            }

            String filePath = Path.GetDirectoryName(fileName);
            String fName = Path.GetFileName(fileName);
            String fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            //FileParts(fileName, FLIM.State, out String filePath, out String fileBaseName, out int fileNum);
            FLIM.State.Files.pathName = filePath;

            if (newFile)
            {
                FLIM.pathName = filePath;


                if (FLIM.State.Files.numberedFile)
                {
                    FLIM.baseName = FLIM.State.Files.baseName;
                    FLIM.fileCounter = FLIM.State.Files.fileCounter;
                    FLIM.numberedFile = true;
                    FLIM.fileName = FLIM.State.Files.fileName;
                    FLIM.fileExtension = FLIM.State.Files.extension;
                    FLIM.fullFileName = Path.Combine(filePath, FLIM.fileName);
                }
                else
                {
                    FLIM.baseName = fileName;
                    FLIM.fileCounter = 0;
                    FLIM.fileName = fName;
                    FLIM.numberedFile = false;
                    FLIM.fullFileName = fileName;
                }
            }

            return FileIO.FileError.Success;
        }



        public static void FileParts(String fullname, ScanParameters Scan, out String filePath, out String fileBaseName, out int num)
        {
            Scan.Files.fromFullNameToFolderPathAndFileName(fullname);
            filePath = Scan.Files.pathName;
            fileBaseName = Scan.Files.baseName;
            if (Scan.Files.numberedFile)
                num = Scan.Files.fileCounter;
            else
                num = -1;
        }

        public int SaveColorImageInTiff(String fileName, Bitmap bmp, DateTime dt, bool overwrite, bool[] saveChannels)
        {
            int error = 0;
            if (bmp == null)
                return -1;

            int width = bmp.Width;
            int height = bmp.Height;

            string writeMode;
            string acquiredTime = dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");

            if (overwrite)
                writeMode = "w";
            else
                writeMode = "a";

            using (Tiff output = Tiff.Open(fileName, writeMode))
            {
                if (output == null)
                {
                    Debug.WriteLine("Problem: " + fileName);
                    return -1;
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
            int depth = 2; //Bytes; 16 bit image.
            int error = 0;
            int nCh = State.Acq.nChannels;

            int width = img.GetLength(0);
            int height = img.GetLength(1);

            string writeMode;
            string acquiredTime = dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");


            if (overwrite)
                writeMode = "w";
            else
                writeMode = "a";

            using (Tiff output = Tiff.Open(fileName, writeMode))
            {
                if (output == null)
                    return -1;

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

        //Saving data for Z-C-Y-X-T data.
        public int SaveFLIMInTiffZStack(String fileName, UInt16[][][,,] FLIM_Pages, DateTime dt, bool overwrite, bool[] saveChannels)
        {
            //Save two channel images. //Not Safe at all.
            int depth = 2; //Bytes; 16 bit image.
            int error = 0;
            int nZ = FLIM_Pages.Length;
            int nCh = FLIM_Pages[0].Length;

            int height = State.Acq.linesPerFrame;
            int width = State.Acq.pixelsPerLine;

            int[] n_time = new int[nCh];

            for (int c = 0; c < nCh; c++)
            {
                if (FLIM_Pages[0][c] != null)
                    n_time[c] = FLIM_Pages[0][c].Length / height / width; //should be 1 or n_time.
                else
                    n_time[c] = 0;
            }

            string writeMode;

            string acquiredTime = dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");


            if (overwrite)
                writeMode = "w";
            else
                writeMode = "a";

            Stopwatch sw = new Stopwatch();
            sw.Start();
            int maxValue = 0;
            for (int z = 0; z < nZ; z++)
            {
                for (int ch = 0; ch < nCh; ch++)
                {
                    var mv = MatrixCalc.calcMax(FLIM_Pages[z][ch]);
                    if (maxValue < mv)
                        maxValue = mv;
                }
            }

            var bimg = new byte[nZ][][];
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

            sw.Stop();
            time_elapsed = (int)sw.ElapsedMilliseconds;
            //Debug.WriteLine("Max value = " + maxValue + ": Time spend = " + sw.ElapsedMilliseconds + "ms");

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
                    str1 = CreateHeader(saveChannels);
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

                return (error);
            }
        }//saveTiff

        public String SaveFLIMFileDialog(String TempFileName)
        {
            return SaveFLIMFileDialog(".flim");
        }

        public String SaveFLIMFileDialog(String TempFileName, String extension)
        {
            String filename = "";
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.InitialDirectory = Path.GetDirectoryName(TempFileName);
            saveFileDialog1.FileName = TempFileName;
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
        }

        public enum FileError
        {
            Success = 0,
            NotFound = 1,
            Canceled = 2,
            UnKnown = 3,
            FormatError = 4,
        }

        public enum ImageType
        {
            FLIMRaw = 0,
            Intensity = 1,
            FLIM_color = 2,
        }
    }
}
