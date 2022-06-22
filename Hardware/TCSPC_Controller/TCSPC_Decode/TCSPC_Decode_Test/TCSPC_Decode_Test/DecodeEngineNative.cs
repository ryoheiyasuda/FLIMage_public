using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TCSPC_controls
{
    class DecodeEngineNative
    {
        public static DecodeEngineNative[] refDE = new DecodeEngineNative[3];
        public static int maxNChannels = 4;
        public static int DEBUGMODE = 1;

        public DE_parameters pm;
        int deviceID;
        int[] nDtime;
        public delegate void SendMessageDelegate(int id, String str);
        public SendMessageDelegate callback = new SendMessageDelegate(CallbackReturn);
        public ushort[][][,,] FLIMData;
        public ushort[][][,,] StripeBuffer;
        public int channelPerDevice;
        public FLIM_Parameters parameters;

        public bool focusing;
        public bool[] eraseMemory;
        public bool measureTagParameters = false;
        public bool saturated = false;

        public int frameCounter = 0;

        Task frame_event;

        public DecodeEngineNative(int device)
        {
            deviceID = device;
            refDE[device] = this;
            DE_Start(device, callback);
        }

        public void Initialize(bool[] EraseMemory, bool focus, FLIM_Parameters parameter_in)
        {
            parameters = parameter_in;
            focusing = focus;
            eraseMemory = (bool[])EraseMemory.Clone();
            saturated = false;
            frameCounter = 0;

            _setParameters(); // use focusing, eraseMemory and parameters.
            _InitializeDecodeEngine();
            _MakeStripe();
        }

        private void _setParameters()
        {
            bool use_bh = parameters.BoardType == "BH";
            int[] acqFLIM = new int[maxNChannels];
            int[] acq = new int[maxNChannels];
            nDtime = new int[maxNChannels];

            if (use_bh)
            {
                channelPerDevice = parameters.spcData.channelPerDeviceBH;
            }
            else
                channelPerDevice = parameters.spcData.channelPerDevicePQ;

            for (int i = 0; i < channelPerDevice; i++)
            {
                acqFLIM[i] = parameters.acquireFLIM[channelPerDevice * deviceID + i] ? 1 : 0;
                acq[i] = parameters.acquisition[channelPerDevice * deviceID + i] ? 1 : 0;
                if (acq[i] == 0)
                    nDtime[i] = 0;
                else if (acqFLIM[i] == 0)
                    nDtime[i] = 1;
                else
                    nDtime[i] = parameters.nDtime;
            }

            parameters.nFastZSlices = parameters.enableFastZscan ? parameters.fastZScan.nFastZSlices : 1;

            if (parameters.rateInfo.syncRate[0] != 0)
                parameters.spcData.time_per_unit = 1.0 / (double)parameters.rateInfo.syncRate[0]; //sync with laser pulses.
            else
                parameters.spcData.time_per_unit = 1.244e-8;

            if (!use_bh && parameters.spcData.acq_modePQ == 2)
                parameters.spcData.time_per_unit = parameters.spcData.resolution[0] * 1.0e-12;
            else if (use_bh)
                parameters.spcData.time_per_unit = 2.5e-8;

            pm = new DE_parameters
            {
                acqType = use_bh ? 1 : 0,
                acquireFLIM0 = acqFLIM[0],
                acquireFLIM1 = acqFLIM[1],
                acquisition0 = acq[0],
                acquisition1 = acq[1],

                BiDirectionalScan = parameters.BiDirectionalScan ? 1 : 0,
                AcquisitionDelay = parameters.AcquisitionDelay,
                BiDirectionalDelay = parameters.BiDirectionalDelay,

                acq_modePQ = parameters.spcData.acq_modePQ,
                binning = parameters.spcData.binning,
                CountPerFastZCycle = parameters.fastZScan.TimePerZScan,
                enableFastZscan = parameters.enableFastZscan ? 1 : 0,
                line_time_correction = parameters.spcData.line_time_correction,

                msPerLine = parameters.msPerLine,
                time_per_unit = 12.8e-9,

                nChannels = channelPerDevice,

                nDtime0 = nDtime[0],
                nDtime1 = nDtime[1],

                nFrames = parameters.nFrames,
                nLines = parameters.nLines,
                nPixels = parameters.nPixels,
                nZlocs = parameters.nFastZSlices,
                n_average = parameters.n_average,

                phaseRangeCount_End = parameters.fastZScan.phaseRangeCount[1],
                phaseRangeCount_Start = parameters.fastZScan.phaseRangeCount[0],
                phase_detection_mode = parameters.fastZScan.phase_detection_mode ? 1 : 0,
                measureTagParameters = measureTagParameters ? 1 : 0,

                pixel_time = parameters.pixel_time,
                resolution = parameters.spcData.resolution[0], //resolution = (int)parameters.spcData.resolution[0];

                StripeDuringFocus = parameters.StripeDuringFocus ? 1 : 0,
                focus = focusing ? 1 : 0,
                LinesPerStripe = parameters.LinesPerStripe,

                TagID = parameters.spcData.TagID,
                LineID = (use_bh) ? parameters.spcData.lineID_BH : parameters.spcData.lineID_PQ,

                eraseMemory0 = eraseMemory[0] ? 1 : 0,
                eraseMemory1 = eraseMemory[1] ? 1 : 0,

                debug = DEBUGMODE //(), no information, 1, some information, 2 most information).
            };
        }

        static public void CallbackReturn(int id, String str)
        {
            if (DEBUGMODE >= 1)
                Console.WriteLine(id + ": " + str);

            if (refDE[id] != null)
            {
                if (str == "FrameDone")
                {
                    if (refDE[id].frame_event != null && !refDE[id].frame_event.IsCompleted)
                        refDE[id].frame_event.Wait();

                    refDE[id].frame_event = Task.Factory.StartNew(() =>
                    {
                        refDE[id].frameCounter++;
                        refDE[id].GetData(); //Bringing data from DLL.
                        if (DEBUGMODE >= 1)
                        {
                            Console.WriteLine("Debug: FrameDone event detected");
                            ushort[,,] a = refDE[id].FLIMData[0][0];
                            ushort[] b = new ushort[a.Length];
                            Buffer.BlockCopy(a, 0, b, 0, a.Length);
                            var c = Array.ConvertAll<ushort, int>(b, new Converter<ushort, int>(x => (int)x));
                            Console.WriteLine("NPhoton = " + c.Sum() + "Frame = " + refDE[id].frameCounter);

                        }
                    });

                }
                else if (str.StartsWith("StripeDone"))
                {
                    var sP = str.Split(',');
                    int startL = Convert.ToInt32(sP[1]);
                    int endL = Convert.ToInt32(sP[2]);
                    refDE[id]._CopyIntoStripe(startL, endL);
                }
                else if (str == "Saturated")
                {
                    if (DEBUGMODE >= 1)
                        Console.WriteLine("Debug: Saturated!!");

                    //Do something.
                }

                //Make Notify event here!
            }
            else
            {
                Console.WriteLine("No instance for ID = " + id + " found!");
            }
        }

        public double GetKHz()
        {
            double value = 0.0;
            DE_calcKHz(deviceID, ref value);
            return value;
        }

        private void _InitializeDecodeEngine()
        {
            DE_Initialize(deviceID, ref pm);
        }


        public void Decode(uint[] buffer, int nRecords)
        {
            DE_Decode(deviceID, buffer, nRecords);
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
            int measBank = 0;
            for (int c = 0; c < pm.nChannels; c++)
            {
                if (nDtime[c] != 0)
                {
                    for (int z = 0; z < pm.nZlocs; z++)
                    {
                        DE_GetDataLine(deviceID, destination[c][z], c, z, startLine, endLine, measBank);
                    }
                }
            }
        }

        public void GetData()
        {
            int bank = 1;
            var data = new ushort[pm.nChannels][][,,];
            for (int c = 0; c < pm.nChannels; c++)
            {
                if (nDtime[c] != 0)
                {
                    data[c] = new ushort[pm.nZlocs][,,];
                    for (int z = 0; z < pm.nZlocs; z++)
                    {
                        data[c][z] = new ushort[pm.nLines, pm.nPixels, nDtime[c]]; //Should allocate first.
                        DE_GetData(deviceID, data[c][z], c, z, bank); //copy data in DLL.
                    }
                }
            }
            FLIMData = data;
        }

        public void MeasureVoxelTiming()
        {
            double time_per_unitC = parameters.spcData.time_per_unit / parameters.spcData.line_time_correction;

            parameters.fastZScan.FrequencyKHz = GetKHz();
            parameters.fastZScan.ZScanPerLine = (int)(parameters.msPerLine * parameters.fastZScan.FrequencyKHz * parameters.fillFraction); //Full scan.

            if (parameters.fastZScan.phase_detection_mode)
                parameters.fastZScan.ZScanPerPixel_Bidirecitonal = 2 * (uint)(parameters.fastZScan.ZScanPerLine / parameters.nPixels); //wil be even number.
            else
                parameters.fastZScan.ZScanPerPixel_Bidirecitonal = (uint)(2 * parameters.fastZScan.ZScanPerLine / parameters.nPixels);

            parameters.fastZScan.ZScanPerPixel = parameters.fastZScan.ZScanPerPixel_Bidirecitonal / 2.0f;
            parameters.fastZScan.TimePerZScan = (uint)(1.0 / 1000.0 / parameters.fastZScan.FrequencyKHz / time_per_unitC); //Full Scan.
                                                                                                                           //parameters.fastZScan.TimePerZScan = (tagTime - tagTimeStart) / (double)tagCounter;
        }


        [DllImport("TCSPC_Decode.dll", EntryPoint = "StartDecodeEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DE_Start(int id, SendMessageDelegate message);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "InitializeDecodeEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DE_Initialize(int id, ref DE_parameters pm);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "DecodeBuffer", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DE_Decode(int id, uint[] buffer, int nRecords);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "calcKHz", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DE_calcKHz(int id, ref double kHz);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "DE_GetDataLine", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DE_GetDataLine(int id, ushort[,,] data, int channel, int zloc, int startLine, int endLine, int bank);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "DE_GetData", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DE_GetData(int id, ushort[,,] destination, int channel, int zloc, int bank);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
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

            public int BiDirectionalScan;
            public int TagID;
            public int LineID;

            //TCSPC parameters
            public int acqType; //PQ = 0, BH = 1;
            public int acq_modePQ;
            public double time_per_unit;
            public double pixel_time;
            public double line_time_correction;
            public double msPerLine;

            public int nDtime0;
            public int nDtime1;
            public int acquisition0;
            public int acquisition1;
            public int acquireFLIM0;
            public int acquireFLIM1;

            public int focus;
            public int StripeDuringFocus;
            public int LinesPerStripe;

            public uint CountPerFastZCycle;
            public int phase_detection_mode;
            public int measureTagParameters;

            public uint phaseRangeCount_Start;
            public uint phaseRangeCount_End;
            public double AcquisitionDelay;
            public double BiDirectionalDelay;

            public int eraseMemory0;
            public int eraseMemory1;

            public int debug;
        }
    }
}
