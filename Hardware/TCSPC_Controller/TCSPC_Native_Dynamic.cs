using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TCSPC_controls
{
    public class TCSPC_Native_Dynamic
    {
        String DLLName = "TCSPC_Decode.dll";
        IntPtr hModule;

        public delegate void GetMessageDelegate(int id, String str);
        public GetMessageDelegate callback;

        public event MessageHandler MessageCallback;
        public delegate void MessageHandler(int id, String message);

        public TCSPC_Native_Dynamic()
        {
            if (hModule == IntPtr.Zero)
            {
                hModule = LoadLibrary(DLLName);
                callback = new GetMessageDelegate(CallBackFunc);
            }
        }

        public void Unload()
        {
            if (hModule != IntPtr.Zero)
            {
                var success = FreeLibrary(hModule);
                if (success)
                    hModule = IntPtr.Zero;
            }
        }

        public void CallBackFunc(int id, String str1)
        {
            MessageCallback?.Invoke(id, str1);
        }

        public int Get_ComputerID()
        {
            var func = (get_ComputerID)loadFunction<get_ComputerID>(DLLName, "Get_ComputerID");
            return func();
        }

        public int Start_TCSPC_Decode(int id, ref DE_parameters param1, ref CompID comID, StringBuilder dll_path)
        {
            var func = (start_TCSPC_Decode)loadFunction<start_TCSPC_Decode>(DLLName, "Start_TCSPC_Decode");
            
            return func(id, callback, ref param1, ref comID, dll_path);
        }

        public int Get_TCSPC_Decode_ABI_Version()
        {
            var func = (get_TCSPC_Decode_ABI_Version)loadFunction<get_TCSPC_Decode_ABI_Version>(DLLName, "Get_TCSPC_Decode_ABI_Version");
            return func();
        }

        public int Setup_Photon_Data_File(int id, StringBuilder photon_data_file)
        {
            var func = (setup_Photon_Data_File)loadFunction<setup_Photon_Data_File>(DLLName, "Setup_Photon_Data_File");
            return func(id, photon_data_file);
        }

        public int Setup_Photon_Data_Stream(int id)
        {
            var func = (setup_Photon_Data_Stream)loadFunction<setup_Photon_Data_Stream>(DLLName, "Setup_Photon_Data_Stream");
            return func(id);
        }

        public int Append_Photon_Data_Stream(int id, IntPtr data, int length, int isLast, IntPtr token)
        {
            var func = (append_Photon_Data_Stream)loadFunction<append_Photon_Data_Stream>(DLLName, "Append_Photon_Data_Stream");
            return func(id, data, length, isLast, token);
        }

        public int Drain_Released_Photon_Stream_Handles(int id, IntPtr[] tokens, int maxCount)
        {
            var func = (drain_Released_Photon_Stream_Handles)loadFunction<drain_Released_Photon_Stream_Handles>(DLLName, "Drain_Released_Photon_Stream_Handles");
            return func(id, tokens, maxCount);
        }

        public int PQ_AllParameters(int id, ref PQ_Parameters pq_param)
        {
            var func = (pq_AllParameters)loadFunction<pq_AllParameters>(DLLName, "PQ_AllParameters");
            return func(id, ref pq_param);
        }

        public int BH_AllParameters(int id, ref SPCdata spc_data)
        {
            var func = (bh_AllParamete)loadFunction<bh_AllParamete>(DLLName, "BH_AllParameters");
            return func(id, ref spc_data);
        }

        public int Start_Measurement(int id, ref DE_parameters pm)
        {
            var func = (start_Measurement)loadFunction<start_Measurement>(DLLName, "Start_Measurement");
            return func(id, ref pm);
        }

        public int Stop_Measurement(int id, int force)
        {
            var func = (stop_Measurement)loadFunction<stop_Measurement>(DLLName, "Stop_Measurement");
            return func(id, force);
        }

        public int GetRate(int id, ref RateInfo pqr)
        {
            var func = (getRate)loadFunction<getRate>(DLLName, "GetRate");
            return func(id, ref pqr);
        }

        public int Close_Device(int id)
        {
            var func = (closeDevice)loadFunction<closeDevice>(DLLName, "Close_Device");
            return func(id);
        }

        public int calcKHz(int id, ref double kHz)
        {
            var func = (_calcKHz)loadFunction<_calcKHz>(DLLName, "calcKHz");
            return func(id, ref kHz);
        }

        public int DE_GetDataLine(int id, ushort[,,] data, int channel, int zloc, int startLine, int endLine)
        {
            var func = (de_GetDataLine)loadFunction<de_GetDataLine>(DLLName, "DE_GetDataLine");
            return func(id, data, channel, zloc, startLine, endLine);
        }

        public int DE_GetData(int id, ushort[,,] data, int channel, int zloc)
        {
            var func = (de_GetData)loadFunction<de_GetData>(DLLName, "DE_GetData");
            return func(id, data, channel, zloc);
        }

        public int Stop_DLL(int id)
        {
            var func = (stop_DLL)loadFunction<stop_DLL>(DLLName, "Stop_DLL");
            return func(id);
        }

        public int RestartEngine(int id)
        {
            var func = (restartEngine)loadFunction<restartEngine>(DLLName, "ReStartEngine");
            return func(id);
        }

        private Delegate loadFunction<T>(string dllPath, string functionName)
        {
            var functionAddress = GetProcAddress(hModule, functionName);
            return Marshal.GetDelegateForFunctionPointer(functionAddress, typeof(T));
        }

        private delegate int get_ComputerID();
        private delegate int get_TCSPC_Decode_ABI_Version();
        private delegate int start_TCSPC_Decode(int id, GetMessageDelegate callback, ref DE_parameters param1, ref CompID comID, StringBuilder dll_path);
        private delegate int setup_Photon_Data_File(int id, StringBuilder photon_data_file);
        private delegate int setup_Photon_Data_Stream(int id);
        private delegate int append_Photon_Data_Stream(int id, IntPtr data, int length, int isLast, IntPtr token);
        private delegate int drain_Released_Photon_Stream_Handles(int id, [Out] IntPtr[] tokens, int maxCount);
        private delegate int pq_AllParameters(int id, ref PQ_Parameters pq_param);
        private delegate int bh_AllParamete(int id, ref SPCdata spc_data);
        private delegate int start_Measurement(int id, ref DE_parameters param1);
        private delegate int stop_Measurement(int id, int force);
        private delegate int getRate(int id, ref RateInfo pqr);
        private delegate int closeDevice(int id);
        private delegate int _calcKHz(int id, ref double kHz);
        private delegate int de_GetDataLine(int id, ushort[,,] data, int channel, int zloc, int startLine, int endLine);
        private delegate int de_GetData(int id, ushort[,,] destination, int channel, int zloc);
        private delegate int stop_DLL(int id);
        private delegate int restartEngine(int id);


        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport("Kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    }


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

        public int savePhotonsInFile;
        public int readFromPhotonFile;

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
}




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
