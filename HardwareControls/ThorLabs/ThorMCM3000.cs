using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

/// <summary>
/// This is obsolete. Now ThorDLL is used as integrated function.
/// </summary>

namespace FLIMage.HardwareControls.ThorLabs
{
    public class ThorMCM3000
    {
        const string DLLName = "ThorMCM3000.dll";

        [DllImport(DLLName, EntryPoint = "FindDevices", CallingConvention = CallingConvention.Cdecl)]
        public static extern int FindDevices(ref int deviceCount);

        [DllImport(DLLName, EntryPoint = "GetParam", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetParam(int paramID, ref double param);

        [DllImport(DLLName, EntryPoint = "GetParamInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetParamInfo(int paramID, ref int paramType, ref int paramAvailable, ref int paramReadOnly, ref double paramMin, ref double paramMax, ref double paramDefault);

        [DllImport(DLLName, EntryPoint = "PostflightPosition", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PostflightPosition();

        [DllImport(DLLName, EntryPoint = "PreflightPosition", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PreflightPosition();

        [DllImport(DLLName, EntryPoint = "SelectDevice", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SelectDevice(int device);

        [DllImport(DLLName, EntryPoint = "SetParam", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetParam(int paramID, double param);

        [DllImport(DLLName, EntryPoint = "SetupPosition", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetupPosition();

        [DllImport(DLLName, EntryPoint = "StartPosition", CallingConvention = CallingConvention.Cdecl)]
        public static extern int StartPosition();

        [DllImport(DLLName, EntryPoint = "StatusPosition", CallingConvention = CallingConvention.Cdecl)]
        public static extern int StatusPosition(ref long status);
    }
}
