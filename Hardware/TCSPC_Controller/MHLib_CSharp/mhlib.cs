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
using System.Runtime.InteropServices;  // for DllImport

public static class mhlib
{
#if x64
  const string MHLib = "MHlib64"; 
#else
  const string MHLib = "Mhlib";
#endif

//  public const string TargetLibVersion = "1.0";  //this is what this program was written for

  [DllImport(MHLib)]
  extern public static int MH_GetLibraryVersion(StringBuilder vers);
  [DllImport(MHLib)]
  extern public static int MH_GetErrorString(StringBuilder errstring, int errcode);

  [DllImport(MHLib)]
  extern public static int MH_OpenDevice(int devidx, StringBuilder serial);
  [DllImport(MHLib)]
  extern public static int MH_CloseDevice(int devidx);
  [DllImport(MHLib)]
  extern public static int MH_Initialize(int devidx, int mode, int refsource);

  // all functions below can only be used after MH_Initialize

  [DllImport(MHLib)]
  extern public static int MH_GetHardwareInfo(int devidx, StringBuilder model, StringBuilder partno, StringBuilder version);
  [DllImport(MHLib)]
  extern public static int MH_GetSerialNumber(int devidx, StringBuilder serial);
  [DllImport(MHLib)]
  extern public static int MH_GetFeatures(int devidx, ref int features);
  [DllImport(MHLib)]
  extern public static int MH_GetBaseResolution(int devidx, ref double resolution, ref int binsteps);
  [DllImport(MHLib)]
  extern public static int MH_GetNumOfInputChannels(int devidx, ref int nchannels);

  [DllImport(MHLib)]
  extern public static int MH_SetSyncDiv(int devidx, int div);
  [DllImport(MHLib)]
  extern public static int MH_SetSyncEdgeTrg(int devidx, int level, int edge);
  [DllImport(MHLib)]
  extern public static int MH_SetSyncChannelOffset(int devidx, int value);

  [DllImport(MHLib)]
  extern public static int MH_SetInputEdgeTrg(int devidx, int channel, int level, int edge);
  [DllImport(MHLib)]
  extern public static int MH_SetInputChannelOffset(int devidx, int channel, int value);
  [DllImport(MHLib)]
  extern public static int MH_SetInputChannelEnable(int devidx, int channel, int enable);

  [DllImport(MHLib)]
  extern public static int MH_SetStopOverflow(int devidx, int stop_ovfl, uint stopcount);
  [DllImport(MHLib)]
  extern public static int MH_SetBinning(int devidx, int binning);
  [DllImport(MHLib)]
  extern public static int MH_SetOffset(int devidx, int offset);
  [DllImport(MHLib)]
  extern public static int MH_SetHistoLen(int devidx, int lencode, ref int actuallen);
  [DllImport(MHLib)]
  extern public static int MH_SetMeasControl(int devidx, int control, int startedge, int stopedge);
  [DllImport(MHLib)]
  extern public static int MH_SetTriggerOutput(int devidx, int period);
  
  [DllImport(MHLib)]
  extern public static int MH_ClearHistMem(int devidx);
  [DllImport(MHLib)]
  extern public static int MH_StartMeas(int devidx, int tacq);
  [DllImport(MHLib)]
  extern public static int MH_StopMeas(int devidx);
  [DllImport(MHLib)]
  extern public static int MH_CTCStatus(int devidx, ref int ctcstatus);

  [DllImport(MHLib)]
  extern public static int MH_GetHistogram(int devidx, uint[] chcount, int channel);
  [DllImport(MHLib)]
  extern public static int MH_GetAllHistogram(int devidx, uint[] chcount);
  [DllImport(MHLib)]
  extern public static int MH_GetResolution(int devidx, ref double resolution);
  [DllImport(MHLib)]
  extern public static int MH_GetSyncPeriod(int devidx, ref double period);
  [DllImport(MHLib)]
  extern public static int MH_GetSyncRate(int devidx, ref int syncrate);
  [DllImport(MHLib)]
  extern public static int MH_GetCountRate(int devidx, int channel, ref int cntrate);
  [DllImport(MHLib)]
  extern public static int MH_GetAllCountRates(int devidx, ref int syncrate, int[] cntrates);
  [DllImport(MHLib)]
  extern public static int MH_GetFlags(int devidx, ref int flags);
  [DllImport(MHLib)]
  extern public static int MH_GetElapsedMeasTime(int devidx, ref double elapsed);
  [DllImport(MHLib)]
  extern public static int MH_GetStartTime(int devidx, ref uint timedw2, ref uint timedw1, ref uint timedw0);
 
  [DllImport(MHLib)]
  extern public static int MH_GetWarnings(int devidx, ref int warnings);
  [DllImport(MHLib)]
  extern public static int MH_GetWarningsText(int devidx, StringBuilder text, int warnings);

  // for TT modes
  [DllImport(MHLib)]
  extern public static int MH_SetMarkerHoldoffTime(int devidx, int holdofftime);
  [DllImport(MHLib)]
  extern public static int MH_SetMarkerEdges(int devidx, int me1, int me2, int me3, int me4);
  [DllImport(MHLib)]
  extern public static int MH_SetMarkerEnable(int devidx, int en1, int en2, int en3, int en4);
  [DllImport(MHLib)]
  extern public static int MH_ReadFiFo(int devidx, uint[] buffer, ref int nactual);

  [DllImport(MHLib)]
  extern public static int MH_GetDebugInfo(int devidx, StringBuilder debuginfo);
  [DllImport(MHLib)]
  extern public static int MH_GetNumOfModules(int devidx, ref int nummod);
  [DllImport(MHLib)]
  extern public static int MH_GetModuleInfo(int devidx, int modidx, ref int modelcode, ref int versioncode);


  //for White Rabbit only
  [DllImport(MHLib)]
  extern public static int MH_WRabbitGetMAC(int devidx, StringBuilder mac_addr);
  [DllImport(MHLib)]
  extern public static int MH_WRabbitSetMAC(int devidx, StringBuilder mac_addr);
  [DllImport(MHLib)]
  extern public static int MH_WRabbitGetInitScript(int devidx, StringBuilder initscript);
  [DllImport(MHLib)]
  extern public static int MH_WRabbitSetInitScript(int devidx, StringBuilder initscript);
  [DllImport(MHLib)]
  extern public static int MH_WRabbitGetSFPData(int devidx, StringBuilder sfpnames, ref int dTxs, ref int dRxs, ref int alphas);
  [DllImport(MHLib)]
  extern public static int MH_WRabbitSetSFPData(int devidx, StringBuilder sfpnames, ref int dTxs, ref int dRxs, ref int alphas);
  [DllImport(MHLib)]
  extern public static int MH_WRabbitInitLink(int devidx, int link_on);
  [DllImport(MHLib)]
  extern public static int MH_WRabbitSetMode(int devidx, int bootfromscript, int reinit_with_mode, int mode);
  [DllImport(MHLib)]
  extern public static int MH_WRabbitSetTime(int devidx, uint timehidw, uint timelodw);
  [DllImport(MHLib)]
  extern public static int MH_WRabbitGetTime(int devidx, ref uint timehidw, ref uint timelodw, ref int subsec16ns);
  [DllImport(MHLib)]
  extern public static int MH_WRabbitGetStatus(int devidx, ref uint wrstatus);
  [DllImport(MHLib)]
  extern public static int MH_WRabbitGetTermOutput(int devidx, StringBuilder buffer, ref int nchar);

}
