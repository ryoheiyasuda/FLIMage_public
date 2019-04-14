
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities
{
    /// <summary>
    /// This is the manager of window locations. Save them in hardware and load it when application opens.
    /// </summary>
    public class WindowLocManager
    {
        Form winobj;
        String windowName;
        String saveDirectory;

        /// <summary>
        /// Constructor of WindowLocManager.
        /// </summary>
        /// <param name="winobj_in"></param>
        /// <param name="windowName_in"></param>
        /// <param name="State"></param>
        public WindowLocManager(Form Window_Form, String Window_Name, String saveFolder)
        {
            winobj = Window_Form;
            windowName = Window_Name;
            saveDirectory = saveFolder;
            Directory.CreateDirectory(saveDirectory);
            File.SetAttributes(saveDirectory, FileAttributes.Hidden);
        }

        public void SaveWindowLocation()
        {
            var sb = new StringBuilder();
            sb.Append(winobj.Location.X);
            sb.Append(",");
            sb.Append(winobj.Location.Y);
            sb.Append(",");
            sb.Append(winobj.Size.Width);
            sb.Append(",");
            sb.Append(winobj.Size.Height);
            string allStr = sb.ToString();
            File.WriteAllText(WindowLocFile(), allStr);
        }

        public void DeleteFile()
        {
            File.Delete(WindowLocFile());
        }

        public void LoadWindowLocation(bool changeSize)
        {
            if (File.Exists(WindowLocFile()))
            {
                String readText = File.ReadAllText(WindowLocFile());
                String[] sP = readText.Split(',');
                var X = Convert.ToInt32(sP[0]);
                var Y = Convert.ToInt32(sP[1]);

                if (X > 0 && Y > 0)
                {
                    winobj.Location = new Point(X, Y);
                }

                if (changeSize && sP.Length >= 4)
                {
                    var width = Convert.ToInt32(sP[2]);
                    var height = Convert.ToInt32(sP[3]);

                    winobj.Size = new Size(width, height);
                }
            }
        }

        String WindowLocFile()
        {
            return Path.Combine(saveDirectory, windowName);
        }
    }

    /// <summary>
    /// Setting manager class. It facilitate to save setting of each window controls.
    /// </summary>
    public class SettingManager
    {
        string SaveFolder = "";
        string FilePath = "";
        string SettingFolder = "Settings";
        string FileName = "setting.txt";
        public Dictionary<string, string> settingData = new Dictionary<string, string>();
        public List<Control> ControlList = new List<Control>();
        public List<Object> ObjectList = new List<object>();
        //public 

        public SettingManager(String SettingName, String initFolderPath)
        {
            if (initFolderPath != "" && Directory.Exists(initFolderPath))
            {
                SaveFolder = Path.Combine(initFolderPath, SettingFolder);
                Directory.CreateDirectory(SaveFolder);
                File.SetAttributes(SaveFolder, FileAttributes.Hidden);
                FileName = SettingName + ".txt";
                FilePath = Path.Combine(SaveFolder, FileName);
            }
            else
            {
                SaveFolder = "";
                FilePath = "";
            }
        }

        public void AddToDict(object obj)
        {
            ObjectList.Add(obj);
            Type t = obj.GetType();
            if (t == typeof(CheckBox))
                settingData.Add(((CheckBox)obj).Name, Convert.ToString(((CheckBox)obj).Checked));
            else if (t == typeof(ToolStripMenuItem))
                settingData.Add(((ToolStripMenuItem)obj).Name, Convert.ToString(((ToolStripMenuItem)obj).Checked));
            else if (t == typeof(TextBox))
                settingData.Add(((TextBox)obj).Name, ((TextBox)obj).Text);
        }

        public void AddToDict(String str1, String str2)
        {
            ObjectList.Add(str1);
            settingData.Add(str1, str2);
        }


        public void ApplyValuesFromObjToDict()
        {
            foreach (var obj in ObjectList)
            {
                Type t = obj.GetType();
                if (t == typeof(CheckBox))
                    settingData[((CheckBox)obj).Name] = Convert.ToString(((CheckBox)obj).Checked);
                else if (t == typeof(ToolStripMenuItem))
                    settingData[((ToolStripMenuItem)obj).Name] = Convert.ToString(((ToolStripMenuItem)obj).Checked);
                else if (t == typeof(TextBox))
                    settingData[((TextBox)obj).Name] = ((TextBox)obj).Text;
            }
        }

        public void ApplyValuesFromDicToObj()
        {
            foreach (var obj in ObjectList)
            {
                Type t = obj.GetType();
                if (t == typeof(CheckBox))
                {
                    if (settingData[((CheckBox)obj).Name] == "1" || settingData[((CheckBox)obj).Name] == "True")
                        ((CheckBox)obj).Checked = true;
                    else
                        ((CheckBox)obj).Checked = false;
                }
                else if (t == typeof(ToolStripMenuItem))
                {
                    if (settingData[((ToolStripMenuItem)obj).Name] == "1" || settingData[((ToolStripMenuItem)obj).Name] == "True")
                        ((ToolStripMenuItem)obj).Checked = true;
                    else
                        ((ToolStripMenuItem)obj).Checked = false;
                }
                else if (t == typeof(TextBox))
                    ((TextBox)obj).Text = settingData[((TextBox)obj).Name].ToString();
            }
        }

        public void SaveSetting()
        {
            StringBuilder sb = new StringBuilder();
            if (settingData == null)
                return;

            foreach (var items in settingData)
            {
                sb.Append(items.Key);
                sb.Append(": ");
                sb.Append(items.Value);
                sb.AppendLine();
            }

            if (Directory.Exists(SaveFolder))
                File.WriteAllText(FilePath, sb.ToString());
        }

        public void LoadToObject()
        {
            LoadSetting();
            ApplyValuesFromDicToObj();
        }

        public void SaveFromObject()
        {
            ApplyValuesFromObjToDict();
            SaveSetting();
        }

        public bool LoadSetting()
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            String[] sP = File.ReadAllLines(FilePath);

            foreach (string str in sP)
            {
                var index = str.IndexOf(':');
                String ValueStr = str.Substring(index + 2);
                String key = str.Substring(0, index);

                if (settingData.ContainsKey(key))
                {
                    settingData[key] = ValueStr;
                }
            }
            return true;

        }
    }
}
