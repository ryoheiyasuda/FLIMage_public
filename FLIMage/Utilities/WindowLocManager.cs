
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
            saveDirectory = ResolveWritableDirectory(saveFolder, fallbackLeafFolder: "WindowsInfo");
        }

        public void SaveWindowLocation()
        {
            if (string.IsNullOrWhiteSpace(saveDirectory))
                return;

            var sb = new StringBuilder();
            sb.Append(winobj.Location.X);
            sb.Append(",");
            sb.Append(winobj.Location.Y);
            sb.Append(",");
            sb.Append(winobj.Size.Width);
            sb.Append(",");
            sb.Append(winobj.Size.Height);
            string allStr = sb.ToString();
            try
            {
                File.WriteAllText(WindowLocFile(), allStr);
            }
            catch
            {
                // Ignore IO issues (access denied, etc.) so UI never crashes.
            }
        }

        public void DeleteFile()
        {
            if (string.IsNullOrWhiteSpace(saveDirectory))
                return;

            try
            {
                File.Delete(WindowLocFile());
            }
            catch
            {
                // Ignore.
            }
        }

        public void LoadWindowLocation(bool changeSize)
        {
            if (string.IsNullOrWhiteSpace(saveDirectory))
                return;

            if (File.Exists(WindowLocFile()))
            {
                String readText;
                try
                {
                    readText = File.ReadAllText(WindowLocFile());
                }
                catch
                {
                    return;
                }

                String[] sP = readText.Split(',');
                if (sP.Length < 2)
                    return;

                int X;
                int Y;
                if (!int.TryParse(sP[0], out X) || !int.TryParse(sP[1], out Y))
                    return;

                // Kengo BEGIN 06-01-2025
                // bug fix for multimonotor
                //if (X > 0 && Y > 0)
                if (IsOnScreen(new Point(X, Y)))
                {
                    winobj.Location = new Point(X, Y);
                }
                // Kengo END

                if (changeSize && sP.Length >= 4)
                {
                    int width;
                    int height;
                    if (int.TryParse(sP[2], out width) && int.TryParse(sP[3], out height))
                    {
                        winobj.Size = new Size(width, height);
                    }
                }
            }
        }

        // Kengo BEGIN 06-01-2025
        // whether the location is on any of screens or not
        public static bool IsOnScreen(Point loc)
        {
            foreach (Screen s in Screen.AllScreens)
            {
                if (s.WorkingArea.Contains(loc))
                    return true;
            }
            return false;
        }
        // Kengo END

        String WindowLocFile()
        {
            return Path.Combine(saveDirectory, windowName);
        }

        private static string ResolveWritableDirectory(string preferredDirectory, string fallbackLeafFolder)
        {
            // Try preferred first.
            if (!string.IsNullOrWhiteSpace(preferredDirectory))
            {
                try
                {
                    Directory.CreateDirectory(preferredDirectory);
                    return preferredDirectory;
                }
                catch
                {
                    // fall through to AppData fallback
                }
            }

            // Fallback to per-user local app data (not protected like Documents on some systems).
            string baseDir;
            try
            {
                baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
            catch
            {
                return "";
            }

            var appName = Application.ProductName;
            if (string.IsNullOrWhiteSpace(appName))
                appName = "FLIMage";

            var fallbackDirectory = Path.Combine(baseDir, appName, fallbackLeafFolder);
            try
            {
                Directory.CreateDirectory(fallbackDirectory);
                return fallbackDirectory;
            }
            catch
            {
                return "";
            }
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
            SaveFolder = "";
            FilePath = "";

            // Prefer Init_Files\Settings (backward compatible), but fall back to LocalAppData when unwritable.
            if (!string.IsNullOrWhiteSpace(initFolderPath) && Directory.Exists(initFolderPath))
            {
                var preferred = Path.Combine(initFolderPath, SettingFolder);
                if (TryEnsureDirectory(preferred))
                {
                    SaveFolder = preferred;
                }
            }

            if (string.IsNullOrWhiteSpace(SaveFolder))
            {
                var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appName = Application.ProductName;
                if (string.IsNullOrWhiteSpace(appName))
                    appName = "FLIMage";
                var fallback = Path.Combine(baseDir, appName, SettingFolder);
                if (TryEnsureDirectory(fallback))
                {
                    SaveFolder = fallback;
                }
            }

            if (!string.IsNullOrWhiteSpace(SaveFolder))
            {
                FileName = SettingName + ".txt";
                FilePath = Path.Combine(SaveFolder, FileName);
            }
        }

        private static bool TryEnsureDirectory(string dir)
        {
            try
            {
                Directory.CreateDirectory(dir);
                return true;
            }
            catch
            {
                return false;
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
            {
                try
                {
                    File.WriteAllText(FilePath, sb.ToString());
                }
                catch
                {
                    // Ignore IO issues (access denied, etc.) so UI never crashes.
                }
            }
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

            String[] sP;
            try
            {
                sP = File.ReadAllLines(FilePath);
            }
            catch
            {
                return false;
            }

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
