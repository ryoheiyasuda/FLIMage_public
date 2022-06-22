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

public static class Constants
{
  // the following constants are taken from mhdefin.h
  
  public const string LIB_VERSION = "1.0";  // library version

  public const int MAXDEVNUM = 8;  // max num of USB devices
  
  public const int MAXINPCHAN = 8;  // max num of physicl input channels
  
  public const int BINSTEPSMAX = 23;  // max num of binning steps, get actual number via MH_GetBaseResolution()
  
  public const int MAXHISTLEN = 65536;  // max number of histogram bins

  public const int TTREADMAX = 1048576;  // number of event records that can be read by MH_ReadFiFo
                                         // buffer must provide space for this number of dwords

  //symbolic constants for MH_Initialize
  public const int REFSRC_INTERNAL = 0;  // use internal clock
  public const int REFSRC_EXTERNAL_10MHZ = 1;  // use 10MHz external clock
  public const int REFSRC_WR_MASTER_GENERIC = 2;  // White Rabbit master with generic partner
  public const int REFSRC_WR_SLAVE_GENERIC = 3;  // White Rabbit slave with generic partner
  public const int REFSRC_WR_GRANDM_GENERIC = 4;  // White Rabbit grand master with generic partner
  public const int REFSRC_EXTN_GPS_PPS = 5;  // use 10 MHz + PPS from GPS
  public const int REFSRC_EXTN_GPS_PPS_UART = 6;  // use 10 MHz + PPS + time via UART from GPS
  public const int REFSRC_WR_MASTER_MHARP = 7;  // White Rabbit master with MultiHarp as partner
  public const int REFSRC_WR_SLAVE_MHARP = 8;  // White Rabbit slave with MultiHarp as partner
  public const int REFSRC_WR_GRANDM_MHARP = 9;  // White Rabbit grand master with MultiHarp as partner

  //symbolic constants for MH_Initialize
  public const int MODE_HIST = 0;
  public const int MODE_T2 = 2;
  public const int MODE_T3 = 3;

  //symbolic constants for MH_SetMeasControl
  public const int MEASCTRL_SINGLESHOT_CTC = 0;  //default
  public const int MEASCTRL_C1_GATED = 1;
  public const int MEASCTRL_C1_START_CTC_STOP = 2;
  public const int MEASCTRL_C1_START_C2_STOP = 3;
  public const int MEASCTRL_WR_M2S = 4;
  public const int MEASCTRL_WR_S2M = 5;

  //symb. const. for MH_SetMeasControl, MH_SetSyncEdgeTrg and MH_SetInputEdgeTrg
  public const int EDGE_RISING = 1;
  public const int EDGE_FALLING = 0;

  //bitmasks for results from MH_GetFeatures
  public const int FEATURE_DLL = 0x0001;
  public const int FEATURE_TTTR = 0x0002;
  public const int FEATURE_MARKERS = 0x0004;
  public const int FEATURE_LOWRES = 0x0008;
  public const int FEATURE_TRIGOUT = 0x0010;

  //bitmasks for results from MH_GetFlag
  public const int FLAG_OVERFLOW = 0x0001;  // histo mode only
  public const int FLAG_FIFOFULL = 0x0002;  // TTTR mode only
  public const int FLAG_SYNC_LOST = 0x0004;
  public const int FLAG_REF_LOST = 0x0008;
  public const int FLAG_SYSERROR = 0x0010;  // hardware error, must contact support
  public const int FLAG_ACTIVE = 0x0020;  // measurement is running
  public const int FLAG_CNTS_DROPPED = 0x0040;  // counts were dropped

  //limits for MH_SetHistoLen
  //note: length codes 0 and 1 will not work with MH_GetHistogram
  //if you need these short lengths then use MH_GetAllHistograms
  public const int MINLENCODE = 0;
  public const int MAXLENCODE = 6;  // default

  //limits for MH_SetSyncDiv
  public const int SYNCDIVMIN = 1;
  public const int SYNCDIVMAX = 16;

  //limits for MH_SetSyncEdgeTrg and MH_SetInputEdgeTrg
  public const int TRGLVLMIN = -1200;  // mV  MH150 Nano only
  public const int TRGLVLMAX = 1200;  // mV  MH150 Nano only 
  
  //limits for MH_SetSyncChannelOffset and MH_SetInputChannelOffset
  public const int CHANOFFSMIN = -99999;  // ps
  public const int CHANOFFSMAX = 99999;  // ps
  
  //limits for MH_SetOffset
  public const int OFFSETMIN = 0;  // ns, for MH_SetOffset
  public const int OFFSETMAX = 100000000;  // ns, for MH_SetOffset 

  //limits for MH_StartMeas
  public const int ACQTMIN = 1;  // ms
  public const int ACQTMAX = 360000000;  // ms  (100*60*60*1000ms = 100h)

  //limits for MH_SetStopOverflow
  public const int STOPCNTMIN = 1;
  public const uint STOPCNTMAX = 4294967295;  // 32 bit is mem max

  //limits for MH_SetTriggerOutput
  public const int TRIGOUTMIN = 0;  // 0=off
  public const int TRIGOUTMAX = 16777215;  // in units of 100ns
  
  //limits for MH_SetMarkerHoldoffTime
  public const int HOLDOFFMIN = 0;  // ns
  public const int HOLDOFFMAX = 25500;  // ns
  
  
  // The following are bitmasks for return values from GetWarnings()

  public const int WARNING_SYNC_RATE_ZERO = 0x0001;
  public const int WARNING_SYNC_RATE_TOO_LOW = 0x0002;
  public const int WARNING_SYNC_RATE_TOO_HIGH = 0x0004;
  public const int WARNING_INPT_RATE_ZERO = 0x0010;
  public const int WARNING_INPT_RATE_TOO_HIGH = 0x0040;
  public const int WARNING_INPT_RATE_RATIO = 0x0100;
  public const int WARNING_DIVIDER_GREATER_ONE = 0x0200;
  public const int WARNING_TIME_SPAN_TOO_SMALL = 0x0400;
  public const int WARNING_OFFSET_UNNECESSARY = 0x0800;
  public const int WARNING_DIVIDER_TOO_SMALL = 0x1000;
  public const int WARNING_COUNTS_DROPPED = 0x2000;


  //The following is only for use with White Rabbit

  public const int WR_STATUS_LINK_ON = 0x00000001;  // mac address is set
  public const int WR_STATUS_LINK_UP = 0x00000002;  // WR link is established

  public const int WR_STATUS_MODE_BITMASK = 0x0000000C;  // mask for the mode bits
  public const int WR_STATUS_MODE_OFF = 0x00000000;  // mode is "off"
  public const int WR_STATUS_MODE_SLAVE = 0x00000004;  // mode is "slave"
  public const int WR_STATUS_MODE_MASTER = 0x00000008;  // mode is "master" 
  public const int WR_STATUS_MODE_GMASTER = 0x0000000C;  // mode is "grandmaster"

  public const int WR_STATUS_LOCKED_CALIBD = 0x00000010;  // locked and calibrated

  public const int WR_STATUS_PTP_BITMASK = 0x000000E0;  // mask for the PTP bits
  public const int WR_STATUS_PTP_LISTENING = 0x00000020;
  public const int WR_STATUS_PTP_UNCLWRSLCK = 0x00000040;
  public const int WR_STATUS_PTP_SLAVE = 0x00000060;
  public const int WR_STATUS_PTP_MSTRWRMLCK = 0x00000080;
  public const int WR_STATUS_PTP_MASTER = 0x000000A0;

  public const int WR_STATUS_SERVO_BITMASK = 0x00000700;  // mask for the servo bits
  public const int WR_STATUS_SERVO_UNINITLZD = 0x00000100;  //
  public const int WR_STATUS_SERVO_SYNC_SEC = 0x00000200;  //
  public const int WR_STATUS_SERVO_SYNC_NSEC = 0x00000300;  //
  public const int WR_STATUS_SERVO_SYNC_PHASE = 0x00000400;  //
  public const int WR_STATUS_SERVO_WAIT_OFFST = 0x00000500;  //
  public const int WR_STATUS_SERVO_TRCK_PHASE = 0x00000600;  //

  public const int WR_STATUS_MAC_SET = 0x00000002;  // mac address is set
  public const uint WR_STATUS_IS_NEW = 0x80000000;  // status updated since last check
}
