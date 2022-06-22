using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;			//for File
using System.Linq;
using System.Runtime.InteropServices;   //for DllImport
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCSPC_controls
{
    /// <summary>
    /// This version use cpp DLL, and should work for all PicoQuant and Becker & Hicl
    /// AcqType = 1 (BH SPC series) AcqType = 2 (Th260) or 3 (MHLib). 
    /// </summary>
    public class TCSPC_Native
    {
        public static TCSPC_Native[] refPQ = new TCSPC_Native[3];
        public static int maxNChannels = 4;

#if DEBUG
        public static int DEBUGMODE = 1; //Debug 2: line-specific event, 3: all photons. Debug 4: all events. Debug 5: Line only.
#else
        public static int DEBUGMODE = 0;
#endif

        public bool Running = false;
        public bool DLLActive = false;
        public bool DLLSerialGoThrough = false;

        public FLIM_Parameters parameters = new FLIM_Parameters();

        public int nChannelsPerDevice = 2; //default.

        public int deviceID = 0;
        public int[] nDtime;
        public bool focusing;
        public bool saturated = false;
        public bool measureTagParameters = false;
        public int frameCounter = 0;
        public int frameAveCounter = 0;
        Task frame_event;

        public TCSPCType acq_type = TCSPCType.PQ_Th260;

        public DE_parameters pm = new DE_parameters();
        public PQ_Parameters pq_param = new PQ_Parameters();
        public SPCdata spc_data = new SPCdata();
        public RateInfo rate_info;
        public CompID compID = new CompID();

        public ushort[][][,,] FLIMData;
        public ushort[][][,,] StripeBuffer;

        public bool completed_acq = true;

        public event FrameDoneHandler FrameDone;
        public FrameEventArgs e_frame;
        public delegate void FrameDoneHandler(TCSPC_Native tcspc, FrameEventArgs e_frame);

        public event MeasDoneHandler MeasDone;
        public delegate void MeasDoneHandler(TCSPC_Native tcspc, EventArgs e);

        public event StripeDoneHandler StripeDone;
        public StripeEventArgs e_my;
        public delegate void StripeDoneHandler(TCSPC_Native tcspc, StripeEventArgs e_my);

        public delegate void GetMessageDelegate(int id, String str);
        public GetMessageDelegate callback;


        /////////////////////////////////////////////////////////
        public static bool CreateTCSPC_Native(FLIM_Parameters flim_parameters, int device)
        {
            TCSPC_Native tcspc;
            bool createNew = false;
            if (refPQ[device] == null)
            {
                tcspc = new TCSPC_Native(flim_parameters, device);
                refPQ[device] = tcspc;

                if (tcspc.DLLActive)
                    createNew = true;
            }

            return createNew;
        }


        public TCSPC_Native(FLIM_Parameters flim_parameters, int device)
        {
            parameters = flim_parameters;
            deviceID = device; // parameters.device


            if (parameters.spcData.BoardType == "BH")
            {
                acq_type = TCSPCType.BH_SPC150;
                nChannelsPerDevice = parameters.spcData.channelPerDeviceBH;

            }
            else if (parameters.spcData.BoardType == "MH")
            {
                acq_type = TCSPCType.PQ_MultiHarp;
                nChannelsPerDevice = parameters.spcData.channelPerDevicePQ;
            }
            else if (parameters.spcData.BoardType == "PQ")
            {
                acq_type = TCSPCType.PQ_Th260;
                nChannelsPerDevice = parameters.spcData.channelPerDevicePQ;
            }

            if (nChannelsPerDevice > maxNChannels)
                nChannelsPerDevice = maxNChannels;

            _setParameters(Enumerable.Repeat(true, maxNChannels).ToArray());

            callback = new GetMessageDelegate(CallbackReturn);

            StringBuilder dll_path = new StringBuilder(260); //260 is for maximum path length for C++

#if _x64
            if (acq_type == TCSPCType.BH_SPC150)
                dll_path.Append(Path.Combine(parameters.spcData.BH_DLLDir, "spcm64.dll"));
            else if (acq_type == TCSPCType.PQ_Th260)
                dll_path.Append("th260lib64");
            else
                dll_path.Append("mhlib64");
#else
            if (acq_type == TCSPCType.BH_SPC150)
                dll_path.Append(Path.Combine(parameters.spcData.BH_DLLDir, "spcm32.dll"));
            else if (acq_type == TCSPCType.PQ_Th260)
                dll_path.Append("th260lib");
            else
                dll_path.Append("mhlib");
#endif

            int retcode = -101;

            try
            {
                retcode = Start_TCSPC_Decode(deviceID, callback, ref pm, ref compID, dll_path);
            }
            catch (Exception EX)
            {
                Debug.WriteLine("Did not find DLL: " + EX.Message);
            }

            parameters.ComputerID = compID.compID;

            completed_acq = true;

            DLLActive = (retcode == 0);
            DLLSerialGoThrough = (retcode != -101);
        }

        public void CallbackReturn(int id, String str)
        {
#if DEBUG
            if (DEBUGMODE >= 1)
                Debug.WriteLine(id + ": " + str);
#endif
            bool busy = false;
            if (refPQ[id] != null)
            {
                if (str == "FrameDone")
                {
                    if (frameCounter > 1 && frame_event != null)
                    {
                        frame_event.Wait();
                        busy = false;
                    }

                    bool skip_process = focusing && busy;

                    frameCounter++;
                    frameAveCounter++;
                    if (parameters.focusAverage <= 1 || (parameters.focusAverage > 1 && frameAveCounter == parameters.focusAverage))
                    {
                        frameAveCounter = 0;
                    }
                    else
                    {
                        if (frameCounter != 1)
                            skip_process = true;
                    }

                    //To speed up, we should skip this during "n_average"
                    if (!skip_process)
                    {
                        GetData(); //Bringing data from DLL. (Copy from C++ memory bank)
                    }
#if DEBUG
                    if (DEBUGMODE >= 1)
                        Debug.WriteLine("Debug: C# Finished reading frames (GetData) = " + frameCounter);
#endif

                    frame_event = Task.Factory.StartNew(() =>
                    {
#if DEBUG
                        if (DEBUGMODE >= 1)
                            Debug.WriteLine("Debug: C# FrameDone event detected. frame = " + frameCounter);

                        if (DEBUGMODE >= 2)
                        {
                            ushort[,,] a = FLIMData[0][0];
                            ushort[] b = new ushort[a.Length];
                            Buffer.BlockCopy(a, 0, b, 0, a.Length);
                            var c = Array.ConvertAll<ushort, int>(b, new Converter<ushort, int>(x => (int)x));
                            Debug.WriteLine("NPhoton = " + c.Sum() + "; Frame = " + frameCounter);

                        }
#endif
                            FrameDone?.Invoke(this, new FrameEventArgs(frameCounter, id, FLIMData));

#if DEBUG
                        if (DEBUGMODE >= 1)
                            Debug.WriteLine("Debug: C# FrameDone Event came back...");
#endif
                        });

                }
                else if (str.StartsWith("StripeDone"))
                {
                    var sP = str.Split(',');
                    int startL = Convert.ToInt32(sP[1]);
                    int endL = Convert.ToInt32(sP[2]);
                    _CopyIntoStripe(startL, endL);
                    StripeDone(this, new StripeEventArgs(startL, endL, StripeBuffer));
                }
                else if (str == "Saturated")
                {
#if DEBUG
                    if (DEBUGMODE >= 1)
                        Console.WriteLine("Debug: Saturated!!");
#endif
                    saturated = true;
                    //FrameDone(this, new FrameEventArgs(frameCounter, id, FLIMData));
                    //Running = false;
                    //Do something.
                }
                else if (str == "MeasurementDone")
                {
#if DEBUG
                    if (DEBUGMODE >= 1)
                        Debug.WriteLine("Debug: MeasurementDone event detected. frame = " + frameCounter);
#endif

                    var t1 = Task.Factory.StartNew((Action)delegate
                    {
                        MeasDone?.Invoke(this, null);
                    });

                    t1.Wait(100); //Wait for 100 ms, just in case....
                    completed_acq = true;
                    Running = false;
                }
            }
            else
            {
                //Console.WriteLine("No instance for ID = " + id + " found!");
            }
        }

        public bool IsCompleted()
        {
            return completed_acq;
        }

        private void _CopyIntoStripe(int startL, int endL)
        {
            if (!_checkBuffer(StripeBuffer))
                _MakeStripe();

            _GetDataLinesFromMeasureBank(StripeBuffer, startL, endL);
        }

        private bool _checkBuffer(ushort[][][,,] buf)
        {
            if (pm.nChannels != buf.Length)
                return false;

            for (int c = 0; c < pm.nChannels; c++)
            {
                if (nDtime[c] != 0)
                {
                    if (pm.nZlocs != buf[c].Length)
                        return false;

                    for (int z = 0; z < pm.nZlocs; z++)
                    {
                        if (buf[c][z].GetLength(0) != pm.nLines || buf[c][z].GetLength(1) != pm.nPixels || buf[c][z].GetLength(2) != nDtime[c])
                            return false;
                    }
                }
            }

            return true;
        }

        private void _MakeStripe()
        {
            var data = new ushort[pm.nChannels][][,,];
            for (int c = 0; c < pm.nChannels; c++)
            {
                if (nDtime[c] != 0)
                {
                    data[c] = new ushort[pm.nZlocs][,,];
                    for (int z = 0; z < pm.nZlocs; z++)
                    {
                        data[c][z] = new ushort[pm.nLines, pm.nPixels, nDtime[c]]; //Should allocate first.
                    }
                }
            }
            StripeBuffer = data;
        }

        public void _GetDataLinesFromMeasureBank(ushort[][][,,] destination, int startLine, int endLine)
        {
            for (int c = 0; c < pm.nChannels; c++)
            {
                if (nDtime[c] != 0)
                {
                    for (int z = 0; z < pm.nZlocs; z++)
                    {
                        DE_GetDataLine(deviceID, destination[c][z], c, z, startLine, endLine);
                    }
                }
            }
        }


        public void GetData()
        {
            var data = new ushort[pm.nChannels][][,,];
            for (int c = 0; c < pm.nChannels; c++)
            {
                if (nDtime[c] != 0)
                {
                    data[c] = new ushort[pm.nZlocs][,,];
                    for (int z = 0; z < pm.nZlocs; z++)
                    {
                        data[c][z] = new ushort[pm.nLines, pm.nPixels, nDtime[c]]; //Should allocate first.
                        DE_GetData(deviceID, data[c][z], c, z); //copy data in DLL.
                    }
                }
            }
            FLIMData = data;
        }

        public void MeasureVoxelTiming()
        {
            double time_per_unitC = parameters.spcData.time_per_unit / parameters.spcData.line_time_correction;

            parameters.fastZScan.FrequencyKHz = _GetKHz();
            parameters.fastZScan.ZScanPerLine = (int)(parameters.msPerLine * parameters.fastZScan.FrequencyKHz * parameters.fillFraction); //Full scan.

            if (parameters.fastZScan.phase_detection_mode)
                parameters.fastZScan.ZScanPerPixel_Bidirecitonal = 2 * (uint)(parameters.fastZScan.ZScanPerLine / parameters.nPixels); //wil be even number.
            else
                parameters.fastZScan.ZScanPerPixel_Bidirecitonal = (uint)(2 * parameters.fastZScan.ZScanPerLine / parameters.nPixels);

            parameters.fastZScan.ZScanPerPixel = parameters.fastZScan.ZScanPerPixel_Bidirecitonal / 2.0f;
            parameters.fastZScan.CountPerFastZCycle = (uint)(1.0 / 1000.0 / parameters.fastZScan.FrequencyKHz / time_per_unitC); //Full Scan.
                                                                                                                                 //parameters.fastZScan.TimePerZScan = (tagTime - tagTimeStart) / (double)tagCounter;
        }

        private double _GetKHz()
        {
            double value = 0.0;
            DE_calcKHz(deviceID, ref value);
            return value;
        }


        public int TCSPC_StartMeas(bool[] EraseMemory, bool focus)
        {
            focusing = focus;
            saturated = false;
            completed_acq = false;

            _setParameters(EraseMemory);
            frameCounter = 0;
            frameAveCounter = 0;

            int retcode = Start_Measurement(deviceID, ref pm);
            if (retcode == 0)
            {
                Running = true;
            }
            else
                Running = false;

            return retcode;
        }


        ////////////////////////////
        public int CloseDevice()
        {
            return CloseDevice(deviceID);
        }

        //////////////////////////////////////////////////////////////////

        public int TCSPC_StopMeas(bool force)
        {
            return Stop_Measurement(deviceID, force ? 1 : 0);
        }


        /// <summary>
        /// Set and get all parameters
        /// </summary>
        /// <returns></returns>
        public int Set_allParameters(FLIM_Parameters flim_parameters)
        {
            parameters = flim_parameters;

            int retcode = 0;
            if (acq_type == TCSPCType.BH_SPC150)
            {

                spc_data.base_adr = 0;  // ushort; base I/O address on PCI bus
                spc_data.init = 0;      // short; set to initialisation result code
                spc_data.cfd_limit_low = (float)parameters.spcData.ch_threshold[nChannelsPerDevice * deviceID];   // for SPCx3x(140,150,131) -500 .. 0mV ,for SPCx0x 5 .. 80mV for DPC230 = CFD_TH1 threshold of CFD1 -510 ..0 mV */
                spc_data.cfd_limit_high = 80;  // float * 5 ..80 mV;default 80 mV ;not for SPC130,140,150,131,930 for DPC230 = CFD_TH2 threshold of CFD2 -510 ..0 mV */
                spc_data.cfd_zc_level = (float)parameters.spcData.ch_zc_level[nChannelsPerDevice * deviceID];  // float SPCx3x(140;150,131) -96 .. 96mV;SPCx0x -10 .. 10mV */
                spc_data.cfd_holdoff = 10.0f;     // float SPCx0x: 5 .. 20 ns, other modules: no influence                             
                spc_data.sync_zc_level = (float)parameters.spcData.sync_zc_level[nChannelsPerDevice * deviceID];   // SPCx3x(140,150,131): -96 .. 96mV, SPCx0x: -10..10mV   
                spc_data.sync_holdoff = 4.0f;   // 4 .. 16 ns ( SPC130,140,150,131,930: no influence)   
                spc_data.sync_threshold = (float)parameters.spcData.sync_threshold[nChannelsPerDevice * deviceID];  // SPCx3x(140,150,131): -500 .. -20mV, SPCx0x: no influence  
                spc_data.tac_range = (float)parameters.spcData.tac_range[nChannelsPerDevice * deviceID];      // 50 .. 5000 ns,
                spc_data.sync_freq_div = (short)parameters.spcData.sync_divider[nChannelsPerDevice * deviceID];   // 1,2,4,8,16 ( SPC130,140,150,131,930, DPC230 : 1,2,4) */
                spc_data.tac_gain = (short)parameters.spcData.tac_gain[nChannelsPerDevice * deviceID];        // 1 .. 15    not for DPC230 */
                spc_data.tac_offset = (float)parameters.spcData.tac_offset[nChannelsPerDevice * deviceID];      // 0 .. 100%, 
                spc_data.tac_limit_low = (float)parameters.spcData.tac_limit_low[nChannelsPerDevice * deviceID];  //0 .. 100%  not for DPC230 */
                spc_data.tac_limit_high = (float)parameters.spcData.tac_limit_high[nChannelsPerDevice * deviceID];  // 0 .. 100%  
                spc_data.adc_resolution = (short)parameters.spcData.adc_res;  // 6,8,10,12 bits, default 10 ,  
                spc_data.ext_latch_delay = (short)20; // 0 ..255 ns, (SPC130, DPC230 : no influence) */
                spc_data.collect_time = 100.0f;    // 1e-7 .. 100000s , default 0.01s */
                spc_data.display_time = 100.0f;    // 0.1 .. 100000s , default 1.0s, obsolete, not used in DLL */
                spc_data.repeat_time = 100.0f;      // 1e-7 .. 100000s , default 10.0s, not for DPC230 */
                spc_data.stop_on_time = 0;    // 1 (stop) or 0 (no stop) */
                spc_data.stop_on_ovfl = 0;  // 1 (stop) or 0 (no stop), not for DPC230  */
                spc_data.dither_range = 0;    // possible values - 0, 32,   64,   128,  256  have meaning:  0, 1/64, 1/32, 1/16, 1/8 
                spc_data.count_incr = 1;      // 1 .. 255, not for DPC230  */
                spc_data.mem_bank = 0;        // for SPC130,600,630, 150,131 :  0 , 1 , default 0
                spc_data.dead_time_comp = 0; // 0 (off) or 1 (on), default 1, not for DPC230   */
                spc_data.scan_control = 0; // SPC505(535,506,536) scanning(routing) control word,
                spc_data.routing_mode = 0;    //DPC230  bits 0-7 - control bits
                spc_data.tac_enable_hold = 0; // SPC230 10.0 .. 265.0 ns - duration of TAC enable pulse ,
                spc_data.pci_card_no = 0;      /* module no on PCI bus (0-7)  */
                spc_data.mode = 5;    //0 - normal operation (routing in), 1 - FIFO mode 32 bits, 2 -  Scan In, 3 - Scan Out  
                                      //5 - FIFO_mode 32 bits with markers ( FIFO_32M )

                spc_data.scan_size_x = (ulong)parameters.nPixels;  // for SPC7x0,830,140,150,930 modules in scanning modes 1 .. 65536, 
                spc_data.scan_size_y = (ulong)parameters.nLines;  // for SPC7x0,830,140,150,930 modules in scanning modes 1 .. 65536,
                spc_data.scan_rout_x = (ushort)(parameters.spcData.channelPerDeviceBH); // number of X routing channels in Scan In & Scan Out modes, not for DPC230
                spc_data.scan_rout_y = 1;  // number of Y routing channels in Scan In & Scan Out modes, not for DPC230
                spc_data.scan_flyback = 1;   // for SPC7x0,830,140,150,930 modules in Scan Out or Rout Out mode, 
                spc_data.scan_borders = 0;   // for SPC7x0,830,140,150,930 modules in Scan In mode, 
                spc_data.scan_polarity = 65535;   // for SPC7x0,830,140,150,930 modules in scanning modes, 
                                                  //for SPC140,150,830 in FIFO_32M mode
                                                  //  bit = 8 - HSYNC (Line) marker disabled (1) or enabled (0, default )
                                                  //              when disabled, line marker will not appear in FIFO photons stream
                spc_data.pixel_clock = 0;   // FIFO_32M mode it disables/enables pixel markers in photons stream */
                spc_data.line_compression = 1;   // line compression factor for SPC7x0,830,140,150,930 modules 
                spc_data.trigger = 00;    //external trigger condition - 
                                          //bits 1 & 0 mean :   00 - ( value 0 ) none(default), 
                                          //                    01 - ( value 1 ) active low, 
                                          //                    10 - ( value 2 ) active high 
                spc_data.pixel_time = (float)parameters.pixel_time;   // pixel time in sec for SPC7x0,830,140,150,930 modules in Scan In mode, 50e-9 .. 1.0 , default 200e-9 */
                spc_data.ext_pixclk_div = 1;  // divider of external pixel clock for SPC7x0,830,140,150 modules
                spc_data.rate_count_time = 0.05F;   // rate counting time in sec  default 1.0 sec
                spc_data.macro_time_clk = 0;   /*  macro time clock definition for SPC130,140,150,131,830,930 in FIFO mode     
                                  0 - 50ns (default), 25ns for SPC150,131 & 140 with FPGA v. > B0 , 
                                  1 - SYNC freq., 2 - 1/2 SYNC freq.,
                                  3 - 1/4 SYNC freq., 4 - 1/8 SYNC freq.
                                  0 - 50ns (default), 1 - SYNC freq., 2 - 1/2 SYNC freq.*/
                spc_data.add_select = 0;     // selects ADD signal source for all modules except SPC930 & DPC230 : 
                spc_data.test_eep = 0;       // test EEPROM checksum or not  */
                spc_data.adc_zoom = 0;     // selects ADC zoom level for module SPC830,140,150,131,930 default 0 
                spc_data.img_size_x = (ulong)parameters.nPixels;  // image X size ( SPC140,150,830 in FIFO_32M, SPC930 in Camera mode ),
                spc_data.img_size_y = (ulong)parameters.nLines;  // image Y size ( SPC140,150,830 in FIFO_32M, SPC930 in Camera mode ),
                spc_data.img_rout_x = (ulong)parameters.spcData.channelPerDeviceBH;  // no of X routing channels ( SPC140,150,830 in FIFO_32M, SPC930 in Camera mode ),
                spc_data.img_rout_y = 1;  // no of Y routing channels ( SPC140,150,830 in FIFO_32M, SPC930 in Camera mode ),
                spc_data.xy_gain = 1;    // selects gain for XY ADCs for module SPC930, 1,2,4, default 1 */
                spc_data.master_clock = 0;  //  use Master Clock( 1 ) or not ( 0 ), default 0,
                spc_data.adc_sample_delay = 0; //ADC's sample delay, only for module SPC930   
                spc_data.detector_type = 1;   // 
                spc_data.x_axis_type = 0;  // X axis representation, only for module SPC930
                spc_data.chan_enable = 0;   // for module DPC230/330 - enable(1)/disable(0) input channels
                spc_data.chan_slope = 0;  // for module DPC230 - active slope of input channels
                spc_data.chan_spec_no = 0;     // for module DPC230/330 - channel numbers of special inputs


                retcode = BH_AllParameters(deviceID, ref spc_data);


                parameters.spcData.ch_threshold[nChannelsPerDevice * deviceID] = spc_data.cfd_limit_low;
                parameters.spcData.ch_zc_level[nChannelsPerDevice * deviceID] = spc_data.cfd_zc_level;  // float SPCx3x(140;150,131) -96 .. 96mV;SPCx0x -10 .. 10mV */
                parameters.spcData.sync_zc_level[nChannelsPerDevice * deviceID] = spc_data.sync_zc_level;   // SPCx3x(140,150,131): -96 .. 96mV, SPCx0x: -10..10mV   
                parameters.spcData.sync_threshold[nChannelsPerDevice * deviceID] = spc_data.sync_threshold;  // SPCx3x(140,150,131): -500 .. -20mV, SPCx0x: no influence  
                parameters.spcData.tac_range[nChannelsPerDevice * deviceID] = spc_data.tac_range;      // 50 .. 5000 ns,
                parameters.spcData.sync_divider[nChannelsPerDevice * deviceID] = spc_data.sync_freq_div;   // 1,2,4,8,16 ( SPC130,140,150,131,930, DPC230 : 1,2,4) */
                parameters.spcData.tac_gain[nChannelsPerDevice * deviceID] = spc_data.tac_gain;        // 1 .. 15    not for DPC230 */
                parameters.spcData.tac_offset[nChannelsPerDevice * deviceID] = spc_data.tac_offset;      // 0 .. 100%, 
                parameters.spcData.tac_limit_low[nChannelsPerDevice * deviceID] = spc_data.tac_limit_low;  //0 .. 100%  not for DPC230 */
                parameters.spcData.tac_limit_high[nChannelsPerDevice * deviceID] = spc_data.tac_limit_high;  // 0 .. 100%  
                parameters.spcData.adc_res = spc_data.adc_resolution;  // 6,8,10,12 bits, default 10 ,  
                parameters.spcData.resolution[nChannelsPerDevice * deviceID] = (double)spc_data.tac_range / spc_data.tac_gain
                    / Math.Pow(2, spc_data.adc_resolution) * 1000.0;
            }
            else
            {
                int[] channels = DeviceChannelID();

                pq_param.Mode = parameters.spcData.acq_modePQ;
                pq_param.NumChannels = nChannelsPerDevice;
                pq_param.sync_divider = (int)parameters.spcData.sync_divider[channels[0]];
                pq_param.sync_threshold = (int)parameters.spcData.sync_threshold[channels[0]];
                pq_param.sync_zc_level = (int)parameters.spcData.sync_zc_level[channels[0]];

                pq_param.ch_threshold0 = (int)parameters.spcData.ch_threshold[channels[0]];
                pq_param.ch_threshold1 = (int)parameters.spcData.ch_threshold[channels[1]];
                pq_param.ch_threshold2 = (int)parameters.spcData.ch_threshold[channels[2]];
                pq_param.ch_threshold3 = (int)parameters.spcData.ch_threshold[channels[3]];

                pq_param.ch_zc_level0 = (int)parameters.spcData.ch_zc_level[channels[0]];
                pq_param.ch_zc_level1 = (int)parameters.spcData.ch_zc_level[channels[1]];
                pq_param.ch_zc_level2 = (int)parameters.spcData.ch_zc_level[channels[2]];
                pq_param.ch_zc_level3 = (int)parameters.spcData.ch_zc_level[channels[3]];

                pq_param.sync_offset = parameters.spcData.sync_offset;

                pq_param.ch_offset0 = parameters.spcData.ch_offset[channels[0]];
                pq_param.ch_offset1 = parameters.spcData.ch_offset[channels[1]];
                pq_param.ch_offset2 = parameters.spcData.ch_offset[channels[2]];
                pq_param.ch_offset3 = parameters.spcData.ch_offset[channels[3]];

                pq_param.resolution = parameters.spcData.resolution[channels[0]];
                pq_param.binning = parameters.spcData.binning;

                pq_param.hardware = 0;

                retcode = PQ_AllParameters(deviceID, ref pq_param);

                if (retcode == 0)
                {
                    for (int i = 0; i < nChannelsPerDevice; i++)
                        parameters.spcData.resolution[deviceID * nChannelsPerDevice + i] = pq_param.resolution;

                    if (pq_param.hardware == (int)PQHardware.TH260N)
                        parameters.spcData.HW_Model = "THarp 260 N";
                    else if (pq_param.hardware == (int)PQHardware.TH260P)
                        parameters.spcData.HW_Model = "THarp 260 P";
                }
            }

            return retcode;
        }

        public int[] DeviceChannelID()
        {
            int[] channels = new int[maxNChannels];
            for (int i = 0; i < maxNChannels; i++)
            {
                channels[i] = nChannelsPerDevice > i ? deviceID * nChannelsPerDevice + i : deviceID * nChannelsPerDevice;
            }
            return channels;
        }

        ///////////
        public int GetRate()
        {
            rate_info = new RateInfo
            {
                sync_rate = 0,
                ch_rate0 = 0,
                ch_rate1 = 0
            };

            int retcode = GetRate(deviceID, ref rate_info);
            parameters.rateInfo.syncRate[deviceID * nChannelsPerDevice] = rate_info.sync_rate;

            int[] channels = DeviceChannelID();
            parameters.rateInfo.countRate[channels[0]] = rate_info.ch_rate0;

            if (nChannelsPerDevice > 1)
                parameters.rateInfo.countRate[channels[1]] = rate_info.ch_rate1;

            if (nChannelsPerDevice > 2)
                parameters.rateInfo.countRate[channels[2]] = rate_info.ch_rate2;

            if (nChannelsPerDevice > 3)
                parameters.rateInfo.countRate[channels[3]] = rate_info.ch_rate3;

            return retcode;
        }

        private void _setParameters(bool[] eraseMemory)
        {
            bool use_bh = parameters.spcData.BoardType == "BH";
            int[] acqFLIM = new int[maxNChannels];
            int[] acq = new int[maxNChannels];
            int[] EraseM = new int[maxNChannels];
            int[] aveFrameA = new int[maxNChannels];
            nDtime = new int[maxNChannels];

            if (use_bh)
            {
                nChannelsPerDevice = parameters.spcData.channelPerDeviceBH;
            }
            else
                nChannelsPerDevice = parameters.spcData.channelPerDevicePQ;

            for (int i = 0; i < nChannelsPerDevice; i++)
            {
                acqFLIM[i] = parameters.acquireFLIM[nChannelsPerDevice * deviceID + i] ? 1 : 0;
                acq[i] = parameters.acquisition[nChannelsPerDevice * deviceID + i] ? 1 : 0;
                if (acq[i] == 0)
                    nDtime[i] = 0;
                else if (acqFLIM[i] == 0)
                    nDtime[i] = 1;
                else
                    nDtime[i] = parameters.nDtime;

                EraseM[i] = eraseMemory[i] ? 1 : 0;

                if (focusing)
                    aveFrameA[i] = (parameters.focusAverage) > 0 ? 1 : 0;
                else
                    aveFrameA[i] = parameters.averageFrame[nChannelsPerDevice * deviceID + i] ? 1 : 0;
            }

            parameters.nFastZSlices = parameters.enableFastZscan ? parameters.fastZScan.nFastZSlices : 1;

            if (parameters.rateInfo.syncRate[nChannelsPerDevice * deviceID] != 0)
                parameters.spcData.time_per_unit = 1.0 / (double)parameters.rateInfo.syncRate[nChannelsPerDevice * deviceID]; //sync with laser pulses.
            else
                parameters.spcData.time_per_unit = 1.244e-8;

            if (!use_bh && parameters.spcData.acq_modePQ == 2)
                parameters.spcData.time_per_unit = parameters.spcData.resolution[nChannelsPerDevice * deviceID] * 1.0e-12;
            else if (use_bh)
                parameters.spcData.time_per_unit = 2.5e-8;
            //parameters.spcData.time_per_unit = 1.0 / (double)parameters.rateInfo.syncRate[0];  //parameters.spcData.time_per_unit = 2.5e-8;

            if (parameters.spcData.BoardType == "BH")
                acq_type = TCSPCType.BH_SPC150;
            else if (parameters.spcData.BoardType == "PQ")
                acq_type = TCSPCType.PQ_Th260;
            else if (parameters.spcData.BoardType == "MH")
                acq_type = TCSPCType.PQ_MultiHarp;

            pm.acqType = (int)acq_type;
            pm.acquireFLIM0 = acqFLIM[0];
            pm.acquireFLIM1 = acqFLIM[1];
            pm.acquireFLIM2 = acqFLIM[2];
            pm.acquireFLIM3 = acqFLIM[3];

            pm.acquisition0 = acq[0];
            pm.acquisition1 = acq[1];
            pm.acquisition2 = acq[2];
            pm.acquisition3 = acq[3];

            pm.aveFrame0 = aveFrameA[0];
            pm.aveFrame1 = aveFrameA[1];
            pm.aveFrame2 = aveFrameA[2];
            pm.aveFrame3 = aveFrameA[3];

            pm.BiDirectionalScanX = parameters.BiDirectionalScanX; //1: 1 tick / line. 2: 1 tick / 2 lines.
            pm.BiDirectionalScanY = parameters.BiDirectionalScanY;

            pm.AcquisitionDelay = parameters.AcquisitionDelay;
            pm.BiDirectionalDelay = parameters.BiDirectionalDelay;

            pm.acq_modePQ = parameters.spcData.acq_modePQ;
            pm.binning = parameters.spcData.binning;
            pm.enableFastZscan = parameters.enableFastZscan ? 1 : 0;
            pm.line_time_correction = parameters.spcData.line_time_correction;

            pm.msPerLine = parameters.msPerLine;
            pm.time_per_unit = parameters.spcData.time_per_unit;

            pm.nChannels = nChannelsPerDevice;

            pm.nDtime0 = nDtime[0];
            pm.nDtime1 = nDtime[1];
            pm.nDtime2 = nDtime[2];
            pm.nDtime3 = nDtime[3];

            pm.nStartPoint = parameters.spcData.startPoint;
            pm.nEndPoint = parameters.spcData.startPoint + parameters.spcData.n_dataPoint;

            pm.nFrames = parameters.nFrames;
            pm.nLines = parameters.nLines;
            pm.nPixels = parameters.nPixels;
            pm.nZlocs = parameters.nFastZSlices;

            if (focusing)
                pm.n_average = parameters.focusAverage;
            else
                pm.n_average = parameters.n_average;


            pm.pixel_time = parameters.pixel_time;
            pm.resolution = parameters.spcData.resolution[0]; //resolution = (int)parameters.spcData.resolution[0];

            pm.StripeDuringFocus = parameters.StripeDuringFocus ? 1 : 0;
            pm.focus = focusing ? 1 : 0;
            pm.LinesPerStripe = parameters.LinesPerStripe;

            pm.pixel_binning = parameters.spcData.pixel_binning;
            pm.skipFirstLines = parameters.spcData.SkipFirstLines;

            pm.TagID = (use_bh) ? parameters.spcData.TagID : parameters.spcData.TagID - 1;
            pm.LineID = (use_bh) ? parameters.spcData.lineID_BH : parameters.spcData.lineID_PQ - 1;
            pm.FrameID = (use_bh) ? parameters.spcData.FrameID : parameters.spcData.FrameID - 1;

            pm.eraseMemory0 = EraseM[0];
            pm.eraseMemory1 = EraseM[1];
            pm.eraseMemory2 = EraseM[2];
            pm.eraseMemory3 = EraseM[3];

            pm.debug = DEBUGMODE; //(); no information, 1, some information, up to 3 most information)

            pm.fastZ_FrequencyKHz = parameters.fastZScan.FrequencyKHz;
            pm.fastZ_ZScanPerPixel = parameters.fastZScan.ZScanPerPixel;
            pm.fastZ_ZScanPerPixel_Bidirecitonal = parameters.fastZScan.ZScanPerPixel_Bidirecitonal;
            pm.fastZ_XYFillFraction = parameters.fastZScan.XYFillFraction;
            pm.fastZ_phaseRangeStart = parameters.fastZScan.phaseRange[0];
            pm.fastZ_phaseRangeEnd = parameters.fastZScan.phaseRange[1];
            pm.fastZ_phaseRangeCountStart = parameters.fastZScan.phaseRangeCount[0];
            pm.fastZ_phaseRangeCountEnd = parameters.fastZScan.phaseRangeCount[1];
            pm.fastZ_nFastZSlices = parameters.fastZScan.nFastZSlices;

            pm.fastZ_phase_detection_mode = parameters.fastZScan.phase_detection_mode ? 1 : 0;
            pm.fastZ_measureTagParameters = parameters.fastZScan.measureTagParameters ? 1 : 0;

            pm.fastZ_CountPerFastZCycle = parameters.fastZScan.CountPerFastZCycle;
            pm.fastZ_CountPerFastZCycleHalf = parameters.fastZScan.CountPerFastZCycleHalf;
            pm.fastZ_CountPerFastZSlice = parameters.fastZScan.CountPerFastZSlice;
            pm.fastZ_residual_for_PhaseDetection = parameters.fastZScan.residual_for_PhaseDetection;

            compID.compID = parameters.ComputerID;
            compID.FLIMID = parameters.FLIMserial;
        }


        /////////

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Get_ComputerID", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Get_ComputerID();

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Start_TCSPC_Decode", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Start_TCSPC_Decode(int id, GetMessageDelegate callback, ref DE_parameters param1, ref CompID comID, StringBuilder dll_path);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "PQ_AllParameters", CallingConvention = CallingConvention.Cdecl)]
        private static extern int PQ_AllParameters(int id, ref PQ_Parameters pap);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "BH_AllParameters", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BH_AllParameters(int id, ref SPCdata spc_data);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Start_Measurement", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Start_Measurement(int id, ref DE_parameters param1);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Stop_Measurement", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Stop_Measurement(int id, int force);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "GetRate", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetRate(int id, ref RateInfo pqr);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Close_Device", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CloseDevice(int id);

        ////////////////////////////////////////////////

        [DllImport("TCSPC_Decode.dll", EntryPoint = "calcKHz", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DE_calcKHz(int id, ref double kHz);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "DE_GetDataLine", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DE_GetDataLine(int id, ushort[,,] data, int channel, int zloc, int startLine, int endLine);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "DE_GetData", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DE_GetData(int id, ushort[,,] destination, int channel, int zloc);

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct PQ_Parameters
        {
            public int Mode;
            public int NumChannels;
            public int sync_divider;
            public int sync_threshold;
            public int sync_zc_level;

            public int ch_threshold0;
            public int ch_threshold1;
            public int ch_threshold2;
            public int ch_threshold3;

            public int ch_zc_level0;
            public int ch_zc_level1;
            public int ch_zc_level2;
            public int ch_zc_level3;

            public int sync_offset;

            public int ch_offset0;
            public int ch_offset1;
            public int ch_offset2;
            public int ch_offset3;

            public double resolution;
            public int binning;

            public int hardware;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct RateInfo
        {
            public int sync_rate;
            public int ch_rate0;
            public int ch_rate1;
            public int ch_rate2;
            public int ch_rate3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct DE_parameters
        {
            public int n_average;
            public double resolution;
            public int binning;
            public int enableFastZscan; //0 or 1
            public int nZlocs; //ZSCANNNING
            public int nLines;
            public int nPixels;
            public int nFrames;
            public int nChannels;

            public int BiDirectionalScanX;
            public int BiDirectionalScanY;
            public int TagID;
            public int LineID;
            public int FrameID; //if negative, it will be not used.
            public int skipFirstLines;
            public int pixel_binning;

            //TCSPC parameters
            public int acqType;
            public int acq_modePQ;
            public double time_per_unit;
            public double pixel_time;
            public double line_time_correction;
            public double msPerLine;

            public int nDtime0;
            public int nDtime1;
            public int nDtime2;
            public int nDtime3;

            public int nStartPoint;
            public int nEndPoint;

            public int acquisition0;
            public int acquisition1;
            public int acquisition2;
            public int acquisition3;

            public int acquireFLIM0;
            public int acquireFLIM1;
            public int acquireFLIM2;
            public int acquireFLIM3;

            public int aveFrame0;
            public int aveFrame1;
            public int aveFrame2;
            public int aveFrame3;

            public int focus;
            public int StripeDuringFocus;
            public int LinesPerStripe;

            public double AcquisitionDelay;
            public double BiDirectionalDelay;

            public int eraseMemory0;
            public int eraseMemory1;
            public int eraseMemory2;
            public int eraseMemory3;

            public int fastZ_measureTagParameters;
            public double fastZ_FrequencyKHz; //KHerz
            public float fastZ_ZScanPerPixel;
            public uint fastZ_ZScanPerPixel_Bidirecitonal;
            public double fastZ_XYFillFraction;
            public double fastZ_VoxelTimeUs; //Microsecond;
            public int fastZ_ZScanPerLine;
            public int fastZ_nFastZSlices;
            public int fastZ_VoxelCount;
            public double fastZ_phaseRangeStart;
            public double fastZ_phaseRangeEnd;
            public uint fastZ_phaseRangeCountStart;
            public uint fastZ_phaseRangeCountEnd;
            public int fastZ_phase_detection_mode;

            public uint fastZ_CountPerFastZCycle; //Count
            public uint fastZ_CountPerFastZCycleHalf;
            public uint fastZ_CountPerFastZSlice;
            public uint fastZ_residual_for_PhaseDetection;


            public int debug;
        }//struct

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct CompID
        {
            public int compID;
            public int FLIMID;
        }

    }//PQNative

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct SPCdata
    {    /* structure for library data  */
        public ushort base_adr;  /* base I/O address on PCI bus */
        public short init;      /* set to initialisation result code */
        public float cfd_limit_low;   /* for SPCx3x(140,150,131) -500 .. 0mV ,for SPCx0x 5 .. 80mV 
                            for DPC230 = CFD_TH1 threshold of CFD1 -510 ..0 mV */
        public float cfd_limit_high;  /* 5 ..80 mV, default 80 mV , not for SPC130,140,150,131,930 
                            for DPC230 = CFD_TH2 threshold of CFD2 -510 ..0 mV */
        public float cfd_zc_level;    /* SPCx3x(140,150,131) -96 .. 96mV, SPCx0x -10 .. 10mV   
                            for DPC230 = CFD_TH3 threshold of CFD3 -510 ..0 mV */
        public float cfd_holdoff;     /* SPCx0x: 5 .. 20 ns, other modules: no influence   
                            for DPC230 = CFD_TH4 threshold of CFD4 -510 ..0 mV */
        public float sync_zc_level;   /* SPCx3x(140,150,131): -96 .. 96mV, SPCx0x: -10..10mV   
                            for DPC230 = CFD_ZC1 Zero Cross level of CFD1 -96 ..96 mV */
        public float sync_holdoff;    /* 4 .. 16 ns ( SPC130,140,150,131,930: no influence)   
                            for DPC230 = CFD_ZC2 Zero Cross level of CFD2 -96 ..96 mV */
        public float sync_threshold;  /* SPCx3x(140,150,131): -500 .. -20mV, SPCx0x: no influence   
                            for DPC230 = CFD_ZC3 Zero Cross level of CFD3 -96 ..96 mV */
        public float tac_range;       /* 50 .. 5000 ns,
                            for DPC230 = DPC range in TCSPC and Multiscaler mode 
                                    0.16461 .. 1e7 ns */
        public short sync_freq_div;   /* 1,2,4,8,16 ( SPC130,140,150,131,930, DPC230 : 1,2,4) */
        public short tac_gain;        /* 1 .. 15    not for DPC230 */
        public float tac_offset;      /* 0 .. 100%, 
                     for DPC230 = TDC offset in TCSPC and Multiscaler mode -100 .. 100% */
        public float tac_limit_low;   /* 0 .. 100%  not for DPC230 */
                                      // for DPC590 = SYNC_FREQ  1 .. 100 MHz 
        public float tac_limit_high;  /* 0 .. 100%  
                            for DPC230 = CFD_ZC4 Zero Cross level of CFD4 -96 ..96 mV */
        public short adc_resolution;  /* 6,8,10,12 bits, default 10 ,  
                            (additionally 0,2,4 bits for SPC830,140,150,131,930 )
                     for DPC230 = no of points of decay curve in TCSPC and Multiscaler mode
                                          0,2,4,6,8,10,12,14,16  bits */
        public short ext_latch_delay; /* 0 ..255 ns, (SPC130, DPC230 : no influence) */
                                      /* SPC140,150,131,930: only values 0,10,20,30,40,50 ns are possible */
        public float collect_time;    /* 1e-7 .. 100000s , default 0.01s */
        public float display_time;    /* 0.1 .. 100000s , default 1.0s, obsolete, not used in DLL */
        public float repeat_time;     /* 1e-7 .. 100000s , default 10.0s, not for DPC230 */
        public short stop_on_time;    /* 1 (stop) or 0 (no stop) */
        public short stop_on_ovfl;    /* 1 (stop) or 0 (no stop), not for DPC230  */
        public short dither_range;    /* possible values - 0, 32,   64,   128,  256 
                               have meaning:  0, 1/64, 1/32, 1/16, 1/8 
                               not for DPC230 */
        public short count_incr;      /* 1 .. 255, not for DPC230  */
        public short mem_bank;        /* for SPC130,600,630, 150,131 :  0 , 1 , default 0
                            other SPC modules: always 0
                            DPC230 : bit 1 - DPC 1 active, bit 2 - DPC 2 active 
                          */
        public short dead_time_comp;  /* 0 (off) or 1 (on), default 1, not for DPC230   */
        public ushort scan_control; /* SPC505(535,506,536) scanning(routing) control word,
                                  other SPC modules always 0 */
        public ushort routing_mode;     /* DPC230  bits 0-7 - control bits
                             SPC150(830,140,131) 
                                - bits 7 - in FIFO_32M mode,  
                                           = 0 (default) Frame pulses on Marker 2,
                                           = 1 Frame pulses on Marker 3,
                                - bits 8 - 11 - enable(1)/disable(0), default 0 
                                              of recording Markers 0-3 entries in FIFO mode 
                                - bits 12 - 15 - active edge 0(falling), 1(rising), default 0 
                                               of Markers 0-3 in FIFO mode 
                             other SPC modules - not used  */
        public float tac_enable_hold;  /* SPC230 10.0 .. 265.0 ns - duration of TAC enable pulse ,
                             DPC230 - macro time clock in ps, default 82.305 ps,
                             other SPC modules always 0 */
        public short pci_card_no;      /* module no on PCI bus (0-7)  */
        public ushort mode;    /* for SPC7x0      , default 0       
                                0 - normal operation (routing in), 
                                1 - block address out, 2 -  Scan In, 3 - Scan Out 
                             for SPC6x0      , default 0       
                                0 - normal operation (routing in)   
                                2 - FIFO mode 48 bits, 3 - FIFO mode 32 bits  
                             for SPC130      , default 0       
                                0 - normal operation (routing in)   
                                2 - FIFO mode 32 bits 
                             for SPC140 , default 0       
                                0 - normal operation (routing in)   
                                1 - FIFO mode 32 bits, 2 -  Scan In, 3 - Scan Out  
                                5 - FIFO_mode 32 bits with markers ( FIFO_32M ), with FPGA v. > B0
                             for SPC150 , default 0       
                                0 - normal operation (routing in)   
                                1 - FIFO mode 32 bits, 2 -  Scan In, 3 - Scan Out  
                                5 - FIFO_mode 32 bits with markers ( FIFO_32M )
                             for SPC830,930 , default 0       
                                0 - normal operation (routing in)   
                                1 - FIFO mode 32 bits, 2 -  Scan In, 3 - Scan Out  
                                4 - Camera mode ( only SPC930 )   
                                5 - FIFO_mode 32 bits with markers ( FIFO_32M ), 
                                                SPC830 with FPGA v. > C0
                             for DPC230 , default 8       
                                6 - TCSPC FIFO    
                                7 - TCSPC FIFO Image mode    
                                8 - Absolute Time FIFO mode   
                                9 - Absolute Time FIFO Image mode 
                             for SPC131 , default 0       
                                0 - normal operation (routing in)   
                                1 - FIFO mode 32 bits
                              */
        public ulong scan_size_x;  /* for SPC7x0,830,140,150,930 modules in scanning modes 1 .. 65536, 
                                         default 1, not for DPC230  */
        public ulong scan_size_y;  /* for SPC7x0,830,140,150,930 modules in scanning modes 1 .. 65536,
                                         default 1, not for DPC230  */
        public ulong scan_rout_x;  /* number of X routing channels in Scan In & Scan Out modes, not for DPC230
                                  for SPC7x0,830,140,150,930 modules
                               1 .. 128, ( SPC7x0,830 ), 1 .. 16 (SPC140,150,930), default 1 */
        public ulong scan_rout_y;  /* number of Y routing channels in Scan In & Scan Out modes, not for DPC230
                                  for SPC7x0,830,140,150, 930 modules 
                               1 .. 128, ( SPC7x0,830 ), 1 .. 16 (SPC140,150,930), default 1 */
                                   /* INT(log2(scan_size_x)) + INT(log2(scan_size_y)) + 
                                      INT(log2(scan_rout_x)) + INT(log2(scan_rout_y)) <= max number of scanning bits
                                                      max number of scanning bits depends on current adc_resolution:
                                                              12 (10 for SPC7x0,140,150)   -              12
                                                              14 (12 for SPC7x0,140,150)   -              10
                                                              16 (14 for SPC7x0,140,150)   -               8
                                                              18 (16 for SPC7x0,140,150)   -               6
                                                              20 (18 for SPC140,150)       -               4
                                                              22 (20 for SPC140,150)       -               2
                                                              24 (22 for SPC140,150)       -               0
                                                              */
        public ulong scan_flyback;   /* for SPC7x0,830,140,150,930 modules in Scan Out or Rout Out mode, 
                                         default 0, not for DPC230  */
                                     /* bits 15-0  Flyback X in number of pixels
                                          bits 31-16 Flyback Y in number of lines */
        public ulong scan_borders;   /* for SPC7x0,830,140,150,930 modules in Scan In mode, 
                                         default 0, not for DPC230  */
                                     /* bits 15-0  Upper boarder, bits 31-16 Left boarder */
        public ushort scan_polarity;    /* for SPC7x0,830,140,150,930 modules in scanning modes, 
                                         default 0, not for DPC230  */
                                        /* bit 0 - polarity of HSYNC (Line), bit 1 - polarity of VSYNC (Frame),
                                           bit 2 - pixel clock polarity
                                           bit = 0 - falling edge(active low)
                                           bit = 1 - rising  edge(active high) 
                                         for SPC140,150,830 in FIFO_32M mode
                                           bit = 8 - HSYNC (Line) marker disabled (1) or enabled (0, default )
                                                       when disabled, line marker will not appear in FIFO photons stream */
        public ushort pixel_clock;   /* for SPC7x0,830,140,150,930 modules in Scan In mode, or DPC230 in Image modes
                             pixel clock source, 0 - internal,1 - external, default 0
                 for SPC140,150,830 in FIFO_32M mode it disables/enables pixel markers 
                                                 in photons stream */
        public ushort line_compression;   /* line compression factor for SPC7x0,830,140,150,930 modules 
                                   in Scan In mode,   1,2,4,8,16,32,64,128, default 1*/
        public ushort trigger;    /* external trigger condition - 
           bits 1 & 0 mean :   00 - ( value 0 ) none(default), 
                               01 - ( value 1 ) active low, 
                               10 - ( value 2 ) active high 
        when sequencer is enabled on SPC130,6x0,150,131 modules additionally
          bits 9 & 8 of the value mean:
           00 - trigger only at the start of the sequence,
           01 ( 100 hex, 256 decimal ) - trigger on each bank
           11 ( 300 hex, 768 decimal ) - trigger on each curve in the bank
        for SPC150, 131, 140 and SPC130 (FPGA v. > C0) multi-module configuration 
               bits 13 & 12 of the value mean:
           x0 - module does not use trigger bus ( trigger defined via bits 0-1),
           01 ( 1000 hex, 4096 decimal ) - module uses trigger bus as slave 
                                            ( waits for the trigger on master),
           11 ( 3000 hex, 12288 decimal ) - module uses trigger bus as master
                                  ( trigger defined via bits 0-1),
                                  ( only one module can be the master )
          */
        public float pixel_time;    /* pixel time in sec for SPC7x0,830,140,150,930 modules in Scan In mode,
                              50e-9 .. 1.0 , default 200e-9 */
        public ulong ext_pixclk_div;  /* divider of external pixel clock for SPC7x0,830,140,150 modules
                                in Scan In mode, 1 .. 0x3fe, default 1*/
        public float rate_count_time;    /* rate counting time in sec  default 1.0 sec
                              for SPC130,830,930,150,131 can be : 1.0, 250ms, 100ms, 50ms 
                              for SPC140 fixed to 50ms   
                              for DPC230 - 1.0sec, 
                                           0.0 - don't count rate outside the measurement, */
        public short macro_time_clk;     /*  macro time clock definition for SPC130,140,150,131,830,930 in FIFO mode     
                              for SPC130, SPC140,150,131:
                                  0 - 50ns (default), 25ns for SPC150,131 & 140 with FPGA v. > B0 , 
                                  1 - SYNC freq., 2 - 1/2 SYNC freq.,
                                  3 - 1/4 SYNC freq., 4 - 1/8 SYNC freq.
                              for SPC830:
                                  0 - 50ns (default), 1 - SYNC freq., 
                              for SPC930:
                                  0 - 50ns (default), 1 - SYNC freq., 2 - 1/2 SYNC freq.*/
        public short add_select;     /* selects ADD signal source for all modules except SPC930 & DPC230 : 
                            0 - internal (ADD only), 1 - external */
        public short test_eep;        /* test EEPROM checksum or not  */
        public short adc_zoom;     /* selects ADC zoom level for module SPC830,140,150,131,930 default 0 
                           bit 4 = 0(1) - zoom off(on ), 
                           bits 0 - 3 zoom level =  
                               0 - zoom of the 1st 1/16th of ADC range,  
                              15 - zoom of the 16th 1/16th of ADC range */
        public ulong img_size_x;  /* image X size ( SPC140,150,830 in FIFO_32M, SPC930 in Camera mode ),
                                      1 .. 1024, default 1 */
        public ulong img_size_y;  /* image Y size ( SPC140,150,830 in FIFO_32M, SPC930 in Camera mode ),
                                actually equal to img_size_x ( quadratic image ) */
        public ulong img_rout_x;  /* no of X routing channels ( SPC140,150,830 in FIFO_32M, SPC930 in Camera mode ),
                                      1 .. 16, default 1 */
        public ulong img_rout_y;  /* no of Y routing channels ( SPC140,150,830 in FIFO_32M, SPC930 in Camera mode ),
                                      1 .. 16, default 1 */
        public short xy_gain;      /* selects gain for XY ADCs for module SPC930, 1,2,4, default 1 */
        public short master_clock;  /*  use Master Clock( 1 ) or not ( 0 ), default 0,
                               only for SPC140,150,131 multi-module configuration 
                        - value 2 (when read) means Master Clock state was set by other application
                                  and cannot be changed */
        public short adc_sample_delay; /* ADC's sample delay, only for module SPC930   
                             0,10,20,30,40,50 ns (default 0 ) */
        public short detector_type;    /*  for module SPC930 :
                            detector type used in Camera mode, 1 .. 9899, default 1, 
                      normally recognised automatically from the corresponding .bit file
                               1 - Hamamatsu Resistive Anode 4 channels detector
                               2 - Wedge & Strip 3 channels detector   
                       for module DPC230 :
                          type of active inputs : bit 1 - TDC1, bit 2 - TDC2, 
                             bit value 0 , CFD inputs active,
                             bit value 1 , TTL inputs active */

        public short x_axis_type;      /* X axis representation, only for module SPC930
                               0 - time (default ), 1 - ADC1 Voltage, 
                               2 - ADC2 Voltage, 3 - ADC3 Voltage, 4 - ADC4 Voltage 
                           */
        public ulong chan_enable;   /* for module DPC230/330 - enable(1)/disable(0) input channels
                                bits 0-7   - en/disable TTL channel 0-7 in TDC1
                                bits 8-9   - en/disable CFD channel 0-1 in TDC1
                                bits 12-19 - en/disable TTL channel 0-7 in TDC2 
                                bits 20-21 - en/disable CFD channel 0-1 in TDC2 
                                */
        public ulong chan_slope;   /* for module DPC230 - active slope of input channels
                                   1 - rising, 0 - falling edge active
                                bits 0-7   - slope of TTL channel 0-7 in TDC1
                                bits 8-9   - slope of CFD channel 0-1 in TDC1
                                bits 12-19 - slope of TTL channel 0-7 in TDC2
                                bits 20-21 - slope of CFD channel 0-1 in TDC2
                                */
        public ulong chan_spec_no;     /* for module DPC230/330 - channel numbers of special inputs
                                                   default 0x8813 in imaging modes
              bits 0-4 - reference chan. no ( TCSPC and Multiscaler modes)
                        default = 19, value:
                       0-1 CFD chan. 0-1 of TDC1,   2-9 TTL chan. 0-7 of TDC1
                     10-11 CFD chan. 0-1 of TDC2, 12-19 TTL chan. 0-7 of TDC2
              bits  8-10 - frame clock TTL chan. no ( imaging modes ) 0-7, default 0
              bits 11-13 - line  clock TTL chan. no ( imaging modes ) 0-7, default 1
              bits 14-16 - pixel clock TTL chan. no ( imaging modes ) 0-7, default 2
              bit  17    - TDC no for pixel, line, frame clocks ( imaging modes )
                              0 = TDC1, 1 = TDC2, default 0
              bits 18-19 - not used 
              bits 20-23 - active channels of TDC1 for DPC-330 Hardware Histogram modes
              bits 24-27 - active channels of TDC2 for DPC-330 Hardware Histogram modes
              bits 28-31 - not used 
              */
    } //SPCdata


    enum PQHardware
    {
        TH260P = 1,
        TH260N = 2,
    }

}
