using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities
{
    public class FormControllers
    {
        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        private const UInt32 WM_CLOSE = 0x0010;

        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);


        /// <summary>
        /// Silly drawing... from the web
        /// </summary>
        /// <param name="ctrl"></param>
        public static void TestSaveGraphics(Control ctrl)
        {
            //Set up reference Graphic
            Graphics refG = ctrl.CreateGraphics(); //assumin this code is running on a control/form
            IntPtr refGrap = refG.GetHdc();
            var img = new Metafile(refGrap, EmfType.EmfPlusDual, "..");

            //Draw some silly drawing
            using (var g = Graphics.FromImage(img))
            {
                var r = new Rectangle(0, 0, 100, 100);
                var reye1 = new Rectangle(20, 20, 20, 30);
                var reye2 = new Rectangle(70, 20, 20, 30);

                var pBlack = new Pen(Color.Black, 3);
                var pRed = new Pen(Color.Red, 2.5f);

                g.FillEllipse(Brushes.Yellow, r);
                g.FillEllipse(Brushes.White, reye1);
                g.FillEllipse(Brushes.White, reye2);
                g.DrawEllipse(pBlack, reye1);
                g.DrawEllipse(pBlack, reye2);
                g.DrawBezier(pRed, new Point(10, 50), new Point(10, 100), new Point(90, 100), new Point(90, 50));
            }

            refG.ReleaseHdc(refGrap); //cleanup
            refG.Dispose();

            img.SaveAsEmf("test.emf");  //chose this line

            //img.Save("test2.emf", ImageFormat.Emf); //or this line
        }

        public static IDictionary<IntPtr, string> GetOpenWindows()
        {
            IntPtr shellWindow = GetShellWindow();
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();

            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                //if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);
                windows[hWnd] = builder.ToString();
                return true;

            }, 0);

            return windows;
        }
        public static void CloseWindow(IntPtr hwnd)
        {
            SendMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public static void CloseOpenExcelWindow(String partOfWindowName)
        {
            foreach (KeyValuePair<IntPtr, string> window in GetOpenWindows())
            {
                IntPtr handle = window.Key;
                string title = window.Value.ToLower();
                if (title.Contains(partOfWindowName.ToLower()))
                    CloseWindow(handle);
            }
        }


        public static void PulldownSelectByItemString(ComboBox pulldownobject, String selection)
        {
            for (int i = 0; i < pulldownobject.Items.Count; i++)
            {
                if (pulldownobject.Items[i].ToString().Equals(selection))
                {
                    pulldownobject.SelectedIndex = i;
                    break;
                }
            }
        } //PUlldown

    } //Class FormController


    public class RegistryController
    {

        public static string[] FindPythonInstallPath()
        {
            List<string> pathList = new List<string>();

            List<string> RegList = new List<string>();
            RegList.Add(@"software\Python");
            RegList.Add(@"software\Wow6432Node\Python");

            for (int i = 0; i < RegList.Count; i++)
            {
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey(RegList[i]);
                FindInstallPathRecursive(pathList, regKey);
                regKey = Registry.LocalMachine.OpenSubKey(RegList[i]);
                FindInstallPathRecursive(pathList, regKey);
            }

            if (pathList.Count > 0)
                return pathList.ToArray();
            else
                return null;

        }

        public static void FindInstallPathRecursive(List<String> pathList, RegistryKey regKey)
        {
            if (regKey == null)
                return;

            var names = regKey.GetSubKeyNames();
            foreach (var name in names)
            {
                var RegKeyI = regKey.OpenSubKey(name);
                if (name == "InstallPath")
                {
                    AddInList(pathList, RegKeyI);
                }
                else
                {
                    FindInstallPathRecursive(pathList, RegKeyI);
                }
            }
        }


        public static void AddInList(List<String> pathList, RegistryKey regKeyI)
        {
            var namesI = regKeyI.GetValueNames(); //regKeyI = "Install"
            foreach (var nameI in namesI)
            {
                var fileName = regKeyI.GetValue(nameI).ToString();
                var pathname = fileName;
                if (pathname.Contains(".exe"))
                    pathname = Path.GetDirectoryName(fileName);
                bool exist = false;
                foreach (var path1 in pathList)
                {
                    if (path1 == pathname)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                    pathList.Add(pathname);
            }
        }

        public static List<string> FindProgramByDisplayNameInRegistry(RegistryKey parentKey, string part_name)
        {
            string[] nameList = parentKey.GetSubKeyNames();

            List<String> strList = new List<string>();

            if (nameList == null)
                return strList;

            for (int i = 0; i < nameList.Length; i++)
            {
                RegistryKey regKey = parentKey.OpenSubKey(nameList[i]);
                try
                {
                    string str = regKey.GetValue("DisplayName").ToString().ToLower();
                    Debug.WriteLine(str);
                    if (str.Contains(part_name.ToLower()))
                    {
                        strList.Add(regKey.GetValue("DisplayName").ToString());
                    }
                }
                catch (Exception EX)
                {
                    Debug.WriteLine(regKey.Name.ToString());
                    //Debug.WriteLine("Error: " + EX.ToString());
                }
            }

            return strList;
        }

    }

    public static class SynchronizeInvokeExtensions
    {
        public static void InvokeIfRequired<T>(this T obj, Action<T> action)
            where T : ISynchronizeInvoke
        {
            if (obj != null)
                if (obj.InvokeRequired)
                {
                    obj.Invoke(action, new object[] { obj });
                }
                else
                {
                    action(obj);
                }
        }

        public static void InvokeAnyway<T>(this T obj, Action<T> action)
    where T : ISynchronizeInvoke
        {
            obj.Invoke(action, new object[] { obj });
        }

        public static void BeginInvokeAnyway<T>(this T obj, Action<T> action)
    where T : ISynchronizeInvoke
        {
            obj.BeginInvoke(action, new object[] { obj });
        }

        public static void BeginInvokeIfRequired<T>(this T obj, Action<T> action)
    where T : ISynchronizeInvoke
        {
            if (obj.InvokeRequired)
            {
                obj.BeginInvoke(action, new object[] { obj });
            }
            else
            {
                action(obj);
            }
        }

        public static TOut InvokeIfRequired_withReturn<TIn, TOut>(this TIn obj, Func<TIn, TOut> func)
            where TIn : ISynchronizeInvoke
        {
            return obj.InvokeRequired
                ? (TOut)obj.Invoke(func, new object[] { obj })
                : func(obj);
        }

        public static TOut BeginInvokeIfRequired_withReturn<TIn, TOut>(this TIn obj, Func<TIn, TOut> func)
            where TIn : ISynchronizeInvoke
        {
            return obj.InvokeRequired
                ? (TOut)obj.BeginInvoke(func, new object[] { obj })
                : func(obj);
        }
    }
} //Utilities



namespace System.Drawing.Imaging
{
    public static class ExtensionMethods
    {
        public static void SaveAsEmf(this Metafile me, string fileName)
        {
            /* http://social.msdn.microsoft.com/Forums/en-US/csharpgeneral/thread/12a1c749-b320-4ce9-aff7-9de0d7fd30ea
                How to save or serialize a Metafile: Solution found
                by : SWAT Team member _1
                Date : Friday, February 01, 2008 1:38 PM
             */
            int enfMetafileHandle = me.GetHenhmetafile().ToInt32();
            int bufferSize = GetEnhMetaFileBits(enfMetafileHandle, 0, null); // Get required buffer size.
            byte[] buffer = new byte[bufferSize]; // Allocate sufficient buffer
            if (GetEnhMetaFileBits(enfMetafileHandle, bufferSize, buffer) <= 0) // Get raw metafile data.
                throw new SystemException("Fail");

            FileStream ms = File.Open(fileName, FileMode.Create);
            ms.Write(buffer, 0, bufferSize);
            ms.Close();
            ms.Dispose();
            if (!DeleteEnhMetaFile(enfMetafileHandle)) //free handle
                throw new SystemException("Fail Free");
        }

        [DllImport("gdi32")]
        public static extern int GetEnhMetaFileBits(int hemf, int cbBuffer, byte[] lpbBuffer);

        [DllImport("gdi32")]
        public static extern bool DeleteEnhMetaFile(int hemfbitHandle);
    }
}
