using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MicroscopeHardwareLibs
{

    public struct EFSHWInfoStruct
    {
        public byte available;
        public byte version;
        public ushort page_size;
        public ushort page_supported;
        public ushort maximum_files;
        public ushort files_remain;
        public ushort pages_remains;
    }

    public struct EFSFileInfoStruct
    {
        public byte file_name;
        public byte exist;
        public byte owned;
        public byte attributes;
        public ushort file_size;
    }

    public struct BorderStatusInfoStruct
    {
        public double border_temperature;
        public double cpu_temperature;
        public double high_voltage;
        public byte error_code;
    };

    public struct JoystickControlInfoStruct
    {
        public ushort hid_usage_page;
        public ushort hid_usage_id;
    };

    public struct JoystickInfoStruct
    {
        public ushort vendor_id;
        public ushort product_id;
        public byte is_hub;
        public byte hub_port_count;
        public byte input_control_count;
        public byte output_control_count;
    };

    public struct StageParamsInfoStruct
    {
        public uint counts_per_unit;
        public float nm_per_count;
        public uint minimum_position;
        public uint maximum_position;
        public double maximum_speed;
        public double maximum_acc;
    };

    public struct JoystickMapInStruct
    {
        public byte port_number;
        public byte control_number;
        public ushort vendor_id;
        public ushort product_id;
        public byte target_port_number;
        public ushort target_control_number;
        public byte destination_slots;
        public byte destination_bit;
        public byte destination_port;
        public byte destination_virtual;
        public byte speed;
        public byte reverse;
        public byte dead_band;
        public uint mode;
        public byte enable;
    }

    public struct JoystickMapOutStruct
    {
        public byte port_number;
        public byte control_number;
        public byte usage_type;
        public ushort vendor_id;
        public ushort product_id;
        public byte led_mode;
        public byte color1;
        public byte color2;
        public byte color3;
        public byte source_slot;
        public byte source_bit;
        public byte source_port;
        public byte source_virtual;
    }

    public class MCM301CommandProvider
    {
        // Below are APIs to control device and exchange data with external dll 
        // You can replace them by yours
        private const string LibraryPath = "ThorMCM301.dll";

        [DllImport(LibraryPath, EntryPoint = "List", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int List([Out] byte[] b, int bufferLength);
        [DllImport(LibraryPath, EntryPoint = "Open", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int Open(string b, int nBaud, int timeout);
        [DllImport(LibraryPath, EntryPoint = "IsOpen", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int IsOpen(string serialNo);
        [DllImport(LibraryPath, EntryPoint = "Close", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int Close(int hdl);


        [DllImport(LibraryPath, EntryPoint = "ChanIdentify", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int ChanIdentify(int hdl, byte slot);
        [DllImport(LibraryPath, EntryPoint = "Home", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int Home(int hdl, byte slot);
        [DllImport(LibraryPath, EntryPoint = "MoveStop", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int Stop(int hdl, byte slot);
        [DllImport(LibraryPath, EntryPoint = "MoveJog", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int MoveJog(int hdl, byte slot, byte direction);
        [DllImport(LibraryPath, EntryPoint = "MoveAbsolute", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int MoveAbsolute(int hdl, byte slot, int targetEncoder);
        [DllImport(LibraryPath, EntryPoint = "SetVelocity", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetVelocity(int hdl, byte slot, byte direction, byte velocity);


        [DllImport(LibraryPath, EntryPoint = "SetChanEnableState", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetChanEnableStatus(int hdl, byte slot, byte enableState);
        [DllImport(LibraryPath, EntryPoint = "SetJogParams", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetJogParams(int hdl, byte slot, uint stepSize);
        [DllImport(LibraryPath, EntryPoint = "SetMOTEncCounter", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetMOTEncCounter(int hdl, byte slot, int encoderCount);
        [DllImport(LibraryPath, EntryPoint = "SetSlotTitle", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetSlotTitle(int hdl, byte slot, [In] byte[] title, int titleLength);
        [DllImport(LibraryPath, EntryPoint = "SetSystemDim", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetSystemDim(int hdl, byte dim);
        [DllImport(LibraryPath, EntryPoint = "SetSoftLimit", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetSoftLimit(int hdl, byte slot, byte mode);
        [DllImport(LibraryPath, EntryPoint = "SetSoftLimitValue", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetSoftLimitValue(int hdl, byte slot, int cwValue, int ccwValue);
        [DllImport(LibraryPath, EntryPoint = "SetEEPROMPARAMSSoftLimit", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetEEPROMPARAMSSoftLimit(int hdl, byte slot);
        [DllImport(LibraryPath, EntryPoint = "SetEEPROMPARAMSJoystickIn", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetEEPROMPARAMSSoftJoystickIn(int hdl, byte portNumber);
        [DllImport(LibraryPath, EntryPoint = "SetEEPROMPARAMSJoystickOut", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetEEPROMPARAMSSoftJoystickOut(int hdl, byte portNumber);
        [DllImport(LibraryPath, EntryPoint = "SetEEPROMPARAMSJogParams", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetEEPROMPARAMSJogParams(int hdl, byte slot);
        [DllImport(LibraryPath, EntryPoint = "SetEEPROMPARAMSHome", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetEEPROMPARAMSHome(int hdl, byte slot);
        [DllImport(LibraryPath, EntryPoint = "SetJoystickIn", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetJoystickMapInControlInfo(int hdl, JoystickMapInStruct info);
        [DllImport(LibraryPath, EntryPoint = "SetJoystickOut", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetJoystickMapOutControlInfo(int hdl, JoystickMapOutStruct info);
        [DllImport(LibraryPath, EntryPoint = "SetEFSFileData", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetEFSFileData(int hdl, byte file_name, int file_address, [In] byte[] data, ushort data_length);
        [DllImport(LibraryPath, EntryPoint = "SetEFSFileInfo", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetEFSFileInfo(int hdl, byte file_name, byte file_attribute, ushort file_length);
        [DllImport(LibraryPath, EntryPoint = "SetHomeInfo", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int SetHomeInfo(int hdl, byte slot, byte home_direction);


        [DllImport(LibraryPath, EntryPoint = "GetChanEnableState", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetChanEnableState(int hdl, byte slot, ref byte enableState);
        [DllImport(LibraryPath, EntryPoint = "GetSystemDim", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetSystemDim(int hdl, ref byte dim);
        [DllImport(LibraryPath, EntryPoint = "GetSlotTitle", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetSlotTitle(int hdl, byte slot, [Out] byte[] title, int bufferLength);
        [DllImport(LibraryPath, EntryPoint = "GetJogParams", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetJogParams(int hdl, byte slot, ref uint jog_step_size);
        [DllImport(LibraryPath, EntryPoint = "GetMotStatus", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetMotStatus(int hdl, byte slot, ref int currentEncoder, ref uint statusBit);
        [DllImport(LibraryPath, EntryPoint = "GetErrorState", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetErrorState(int hdl);
        [DllImport(LibraryPath, EntryPoint = "GetPNPStatus", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetPnpStatus(int hdl, byte slot, ref uint status);
        [DllImport(LibraryPath, EntryPoint = "GetBoardStatus", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetBoardStatus(int hdl, ref BorderStatusInfoStruct borderStatusInfoStruct);
        [DllImport(LibraryPath, EntryPoint = "GetStageParams", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetStageParams(int hdl, byte slot, ref StageParamsInfoStruct stageParamsInfoStruct);
        [DllImport(LibraryPath, EntryPoint = "GetSlotDeviceType", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetSlotDeviceType(int hdl, byte slot, [Out] byte[] deviceType, int deviceTypeLength);
        [DllImport(LibraryPath, EntryPoint = "GetSoftwareLimit", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetSoftwareLimit(int hdl, byte slot, ref int set_software_limit_cw, ref int soft_limit_cw, ref int set_software_limit_ccw, ref int soft_limit_ccw);
        [DllImport(LibraryPath, EntryPoint = "GetHardwareInfo", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetHardwareInfo(int hdl, [Out] byte[] firmware_version, int firmware_version_bufferLen, [Out] byte[] CPID_version, int CPID_version_bufferLen);
        [DllImport(LibraryPath, EntryPoint = "GetJoystickInfo", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetJoystickInfo(int hdl, byte portNumber, ref JoystickInfoStruct joystickInfoStruct);
        [DllImport(LibraryPath, EntryPoint = "GetJoystickControlInfo", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetJoystickControlInfo(int hdl, byte portNumber, byte control_type, byte control_number, ref JoystickControlInfoStruct joystickControlInfoStruct);
        [DllImport(LibraryPath, EntryPoint = "GetJoystickMapInControlInfo", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetJoystickMapInControlInfo(int hdl, byte portNumber, byte control_number, ref JoystickMapInStruct info);
        [DllImport(LibraryPath, EntryPoint = "GetJoystickMapOutControlInfo", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetJoystickMapOutControlInfo(int hdl, byte portNumber, byte control_number, ref JoystickMapOutStruct info);
        [DllImport(LibraryPath, EntryPoint = "GetEFSHWInfo", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetEFSHWInfo(int hdl, ref EFSHWInfoStruct @struct);
        [DllImport(LibraryPath, EntryPoint = "GetEFSFileInfo", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetEFSFileInfo(int hdl, byte file_name, ref EFSFileInfoStruct @struct);
        [DllImport(LibraryPath, EntryPoint = "GetEFSFileData", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetEFSFileData(int hdl, byte file_name, int file_address, ushort read_length, [Out] byte[] data);
        [DllImport(LibraryPath, EntryPoint = "GetHomeInfo", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetHomeInfo(int hdl, byte slot, ref byte home_direction);

        [DllImport(LibraryPath, EntryPoint = "ConvertEncoderTonm", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int ConvertEncoderTonm(int hdl, byte slot, int encoder_count, ref double nm);
        [DllImport(LibraryPath, EntryPoint = "ConvertnmToEncoder", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int ConvertnmToEncoder(int hdl, byte slot, double nm, ref int encoder_count);
    }
}
