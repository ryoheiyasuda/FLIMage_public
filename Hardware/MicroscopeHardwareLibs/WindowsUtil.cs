using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroscopeHardwareLibs
{
    public class WindowsUtil
    {
        public static void SetupNIAssemblyBinding()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string nidaq_version = GetNIDaqmxVersionFromRegistry();

            string dirname = "";
            if (nidaq_version.Contains("18.1"))
                dirname = "NI18.1";
            else if (nidaq_version.Contains("18.6"))
                dirname = "NI18.6";
            else if (nidaq_version.Contains("20.1"))
                dirname = "NI20.1";
            else if (nidaq_version.Contains("25.0"))
                dirname = "NI25.0";
            else
            {
                Debug.WriteLine("NI driver not found");
                return;
            }

            string niDllFolder = Path.Combine(baseDir, dirname);

            if (Directory.Exists(niDllFolder))
            {
                // Ensure native NI DLLs like nicaiu.dll are found
                SetDllDirectory(niDllFolder);

                // Dynamically resolve missing assemblies from the correct folder
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    string assemblyName = new AssemblyName(args.Name).Name + ".dll";
                    string assemblyPath = Path.Combine(niDllFolder, assemblyName);

                    if (File.Exists(assemblyPath))
                    {
                        return Assembly.LoadFrom(assemblyPath);
                    }

                    return null;
                };
            }
            else
            {
                Debug.WriteLine("NI driver not found");
                return;
            }
        }

        public static string GetNIDaqmxVersionFromRegistry()
        {
            string version = "";
            string nicaiuPath = Path.Combine(Environment.SystemDirectory, "nicaiu.dll");

            if (File.Exists(nicaiuPath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(nicaiuPath);
                version = versionInfo.FileVersion;
            }

            return version;
        }


        public static bool IsWindows11()
        {
            string productName = GetWindowsProductName();
            return productName.Contains("Windows 11");
        }

        private static string GetWindowsProductName()
        {
            string name = "";
            try
            {
                var searcher = new System.Management.ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
                foreach (var os in searcher.Get())
                {
                    name = os["Caption"].ToString();
                    break;
                }
            }
            catch { }

            return name;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

    }
}
