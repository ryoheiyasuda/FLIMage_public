using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FLIMage.HardwareControls.ThorLabs
{
    /// <summary>
    /// Generic Thorlab DLL loader.
    /// </summary>
    public class ThorDLL
    {
        String DLLName;
        IntPtr hModule;

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport("Kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        static Dictionary<DLLType, ThorDLL> ThorlabDLLs = new Dictionary<DLLType, ThorDLL>();

        public enum DLLType
        {
            ThorMCM3000 = 1,
            ThorBScope = 2,
            ThorECU = 3,
            ThorBCM = 4,
        }

        public static ThorDLL ThorDLL_Load(DLLType dll_type)
        {
            if (ThorlabDLLs.TryGetValue(dll_type, out ThorDLL dll))
            {
                return dll;
            }
            else
            {
                dll = new ThorDLL(dll_type);
                ThorlabDLLs.Add(dll_type, dll);
                return dll;
            }
        }

        public static void ThorlabDLL_Unload(DLLType dll_type)
        {
            if (ThorlabDLLs.TryGetValue(dll_type, out ThorDLL dll))
            {
                FreeLibrary(dll.hModule);
                ThorlabDLLs.Remove(dll_type);
            }
        }

        public ThorDLL(DLLType dll_type)
        {
            DLLName = dll_type.ToString("F") + ".dll";
            //if (dll_type == DLLType.ThorECU)
            //    DLLName = "ThorECU.dll";
            //else if (dll_type == DLLType.ThorMCM3000)
            //    DLLName = "ThorMCM3000.dll";
            //else if (dll_type == DLLType.ThorBScope)
            //    DLLName = "ThorBScope.dll";
            //else if (dll_type == DLLType.ThorBCM)
            //    DLLName = "ThorBCM.dll";

            hModule = LoadLibrary(DLLName);
        }

        public int FindDevices(ref int deviceCount)
        {
            var findDevice = (ThorFindDevices)loadFunction<ThorFindDevices>(DLLName, "FindDevices");
            return findDevice(ref deviceCount);
        }

        public int GetParam(int paramID, ref double param1)
        {
            var func = (ThorGetParam)loadFunction<ThorGetParam>(DLLName, "GetParam");
            return func(paramID, ref param1);
        }

        public int GetParamInfo(int paramID, ref int paramType, ref int paramAvailable, ref int paramReadOnly, ref double paramMin, ref double paramMax, ref double paramDefault)
        {
            var func = (ThorGetParamInfo)loadFunction<ThorGetParamInfo>(DLLName, "GetParamInfo");
            return func(paramID, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault);
        }

        public int PostflightPosition()
        {
            var func = (ThorPostflightPosition)loadFunction<ThorPostflightPosition>(DLLName, "PostflightPosition");
            return func();
        }

        public int PreflightPosition()
        {
            var func = (ThorPreflightPosition)loadFunction<ThorPreflightPosition>(DLLName, "PreflightPosition");
            return func();
        }

        public int SelectDevice(int device)
        {
            var func = (ThorSelectDevice)loadFunction<ThorSelectDevice>(DLLName, "SelectDevice");
            return func(device);
        }

        public int SetParam(int paramID, double param1)
        {
            var func = (ThorSetParam)loadFunction<ThorSetParam>(DLLName, "SetParam");
            return func(paramID, param1);
        }

        public int SetupPosition()
        {
            var func = (ThorSetupPosition)loadFunction<ThorSetupPosition>(DLLName, "SetupPosition");
            return func();
        }

        public int StartPosition()
        {
            var func = (ThorStartPosition)loadFunction<ThorStartPosition>(DLLName, "StartPosition");
            return func();
        }

        public int StatusPosition(ref long status)
        {
            var func = (ThorStatusPosition)loadFunction<ThorStatusPosition>(DLLName, "StatusPosition");
            return func(ref status);
        }

        private Delegate loadFunction<T>(string dllPath, string functionName)
        {
            var functionAddress = GetProcAddress(hModule, functionName);
            return Marshal.GetDelegateForFunctionPointer(functionAddress, typeof(T));
        }

        private delegate int ThorFindDevices(ref int deviceCount);
        private delegate int ThorGetParam(int paramID, ref double param1);
        private delegate int ThorGetParamInfo(int paramID, ref int paramType, ref int paramAvailable, ref int paramReadOnly, ref double paramMin, ref double paramMax, ref double paramDefault);
        private delegate int ThorPostflightPosition();
        private delegate int ThorPreflightPosition();
        private delegate int ThorSelectDevice(int device);
        private delegate int ThorSetParam(int paramID, double param1);
        private delegate int ThorSetupPosition();
        private delegate int ThorStartPosition();
        private delegate int ThorStatusPosition(ref long status);

    }
}
