/* 
    MHLib programming library for MultiHarp 150
    PicoQuant GmbH 

    Ver. 1.0.0.0     Sept. 2018
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Errorcodes
{
  public const int MH_ERROR_NONE = 0;

  public const int MH_ERROR_DEVICE_OPEN_FAIL = -1;
  public const int MH_ERROR_DEVICE_BUSY = -2;
  public const int MH_ERROR_DEVICE_HEVENT_FAIL = -3;
  public const int MH_ERROR_DEVICE_CALLBSET_FAIL = -4;
  public const int MH_ERROR_DEVICE_BARMAP_FAIL = -5;
  public const int MH_ERROR_DEVICE_CLOSE_FAIL = -6;
  public const int MH_ERROR_DEVICE_RESET_FAIL = -7;
  public const int MH_ERROR_DEVICE_GETVERSION_FAIL = -8;
  public const int MH_ERROR_DEVICE_VERSION_MISMATCH = -9;
  public const int MH_ERROR_DEVICE_NOT_OPEN = -10;
  public const int HH_ERROR_DEVICE_LOCKED = -11;
  public const int HH_ERROR_DEVICE_DRIVERVER_MISMATCH = -12;

  public const int MH_ERROR_INSTANCE_RUNNING = -16;
  public const int MH_ERROR_INVALID_ARGUMENT = -17;
  public const int MH_ERROR_INVALID_MODE = -18;
  public const int MH_ERROR_INVALID_OPTION = -19;
  public const int MH_ERROR_INVALID_MEMORY = -20;
  public const int MH_ERROR_INVALID_RDATA = -21;
  public const int MH_ERROR_NOT_INITIALIZED = -22;
  public const int MH_ERROR_NOT_CALIBRATED = -23;
  public const int MH_ERROR_DMA_FAIL = -24;
  public const int MH_ERROR_XTDEVICE_FAIL = -25;
  public const int MH_ERROR_FPGACONF_FAIL = -26;
  public const int MH_ERROR_IFCONF_FAIL = -27;
  public const int MH_ERROR_FIFORESET_FAIL = -28;
  public const int MH_ERROR_THREADSTATE_FAIL = -29;
  public const int MH_ERROR_THREADLOCK_FAIL = -30;

  public const int MH_ERROR_USB_GETDRIVERVER_FAIL = -32;
  public const int MH_ERROR_USB_DRIVERVER_MISMATCH = -33;
  public const int MH_ERROR_USB_GETIFINFO_FAIL = -34;
  public const int MH_ERROR_USB_HISPEED_FAIL = -35;
  public const int MH_ERROR_USB_VCMD_FAIL = -36;
  public const int MH_ERROR_USB_BULKRD_FAIL = -37;
  public const int MH_ERROR_USB_RESET_FAIL = -38;

  public const int MH_ERROR_LANEUP_TIMEOUT = -40;
  public const int MH_ERROR_DONEALL_TIMEOUT = -41;
  public const int MH_ERROR_MODACK_TIMEOUT = -42;
  public const int MH_ERROR_MACTIVE_TIMEOUT = -43;
  public const int MH_ERROR_MEMCLEAR_FAIL = -44;
  public const int MH_ERROR_MEMTEST_FAIL = -45;
  public const int MH_ERROR_CALIB_FAIL = -46;
  public const int MH_ERROR_REFSEL_FAIL = -47;
  public const int MH_ERROR_STATUS_FAIL = -48;
  public const int MH_ERROR_MODNUM_FAIL = -49;
  public const int MH_ERROR_DIGMUX_FAIL = -50;
  public const int HH_ERROR_MB_ACK_FAIL = -51;
  public const int MH_ERROR_MODFWPCB_MISMATCH = -52;
  public const int MH_ERROR_MODFWVER_MISMATCH = -53;
  public const int MH_ERROR_MODPROPERTY_MISMATCH = -54;
  public const int MH_ERROR_INVALID_MAGIC = -55;
  public const int MH_ERROR_INVALID_LENGTH = -56;
  public const int MH_ERROR_RATE_FAIL = -57;
  public const int MH_ERROR_MODFWVER_TOO_LOW = -58;
  public const int MH_ERROR_MODFWVER_TOO_HIGH = -59;
  public const int MH_ERROR_MB_ACK_FAIL = -60;

  public const int MH_ERROR_EEPROM_F01 = -64;
  public const int MH_ERROR_EEPROM_F02 = -65;
  public const int MH_ERROR_EEPROM_F03 = -66;
  public const int MH_ERROR_EEPROM_F04 = -67;
  public const int MH_ERROR_EEPROM_F05 = -68;
  public const int MH_ERROR_EEPROM_F06 = -69;
  public const int MH_ERROR_EEPROM_F07 = -70;
  public const int MH_ERROR_EEPROM_F08 = -71;
  public const int MH_ERROR_EEPROM_F09 = -72;
  public const int MH_ERROR_EEPROM_F10 = -73;
  public const int MH_ERROR_EEPROM_F11 = -74;
  public const int MH_ERROR_EEPROM_F12 = -75;
  public const int MH_ERROR_EEPROM_F13 = -76;
  public const int MH_ERROR_EEPROM_F14 = -77;
  public const int MH_ERROR_EEPROM_F15 = -78;
}

