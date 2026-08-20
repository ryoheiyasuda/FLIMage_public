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
        public static bool DLL_Busy = false;
        private const int ExpectedTcspcDecodeAbiVersion = 2;
        private const int TcspcDecodeAbiMismatchRetCode = -102;

#if DEBUG
        public static int DEBUGMODE = 1; //Debug 2: line-specific event, 3: all photons. Debug 4: all events. Debug 5: Line only.
#else
        public static int DEBUGMODE = 0;
#endif

        public bool Running = false;
        public bool DLLActive = false;
        public bool DLLSerialGoThrough = false;

        public bool force_stop_event_received = false;

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

        public bool photon_file_saving_mode = false;

        public event FrameDoneHandler FrameDone;
        public FrameEventArgs e_frame;
        public delegate void FrameDoneHandler(TCSPC_Native tcspc, FrameEventArgs e_frame);

        public event MeasDoneHandler MeasDone;
        public delegate void MeasDoneHandler(TCSPC_Native tcspc, EventArgs e);

        public event StripeDoneHandler StripeDone;
        public StripeEventArgs e_my;
        public delegate void StripeDoneHandler(TCSPC_Native tcspc, StripeEventArgs e_my);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void GetMessageDelegate(int id, [MarshalAs(UnmanagedType.LPStr)] string str, int frame);
        public GetMessageDelegate callback;

        public GCHandle photon_binary_handle;
        private GCHandle photon_write_callback_handle;
        private Stream photon_write_stream;
        private byte[] photon_write_buffer;
        private readonly object photon_write_lock = new object();
        private readonly object photon_stream_lock = new object();
        private readonly HashSet<IntPtr> photon_stream_handles = new HashSet<IntPtr>();
        private readonly IntPtr[] photon_stream_release_buffer = new IntPtr[32];


        /////////////////////////////////////////////////////////
        public static bool CreateTCSPC_Native(FLIM_Parameters flim_parameters, int device)
        {
            TCSPC_Native tcspc;
            bool createNew = false;
            if (refPQ[device] == null)
            {
                tcspc = new TCSPC_Native(flim_parameters, device);
                refPQ[device] = tcspc;

                //if (tcspc.DLLActive)
                createNew = true;
            }

            return createNew;
        }


        public TCSPC_Native(FLIM_Parameters flim_parameters, int device)
        {
            parameters = flim_parameters;
            deviceID = device; // parameters.device


            // Optimized: Use string comparison with StringComparison.Ordinal for better performance
            string boardType = parameters.spcData.BoardType;
            if (string.Equals(boardType, "BH", StringComparison.OrdinalIgnoreCase))
            {
                acq_type = TCSPCType.BH_SPC150;
                nChannelsPerDevice = parameters.spcData.channelPerDeviceBH;
            }
            else if (string.Equals(boardType, "MH", StringComparison.OrdinalIgnoreCase))
            {
                acq_type = TCSPCType.PQ_MultiHarp;
                nChannelsPerDevice = parameters.spcData.channelPerDevicePQ;
            }
            else if (string.Equals(boardType, "PQ", StringComparison.OrdinalIgnoreCase))
            {
                acq_type = TCSPCType.PQ_Th260;
                nChannelsPerDevice = parameters.spcData.channelPerDevicePQ;
            }
            else if (string.Equals(boardType, "PH", StringComparison.OrdinalIgnoreCase))
            {
                acq_type = TCSPCType.PQ_PH330;
                nChannelsPerDevice = parameters.spcData.channelPerDevicePQ;
            }
            else if (string.Equals(boardType, "HH", StringComparison.OrdinalIgnoreCase))
            {
                acq_type = TCSPCType.PQ_HH500;
                nChannelsPerDevice = parameters.spcData.channelPerDevicePQ;
            }
            else if (string.Equals(boardType, "SimPQ", StringComparison.OrdinalIgnoreCase)
                || string.Equals(boardType, "SymPQ", StringComparison.OrdinalIgnoreCase))
            {
                acq_type = TCSPCType.simpq;
                nChannelsPerDevice = parameters.spcData.channelPerDevicePQ;
            }

            if (nChannelsPerDevice > maxNChannels)
                nChannelsPerDevice = maxNChannels;

            // Optimized: Pre-allocate array instead of LINQ
            bool[] eraseMemory = new bool[maxNChannels];
            for (int i = 0; i < maxNChannels; i++)
                eraseMemory[i] = true;
            _setParameters(eraseMemory);

            callback = new GetMessageDelegate(CallbackReturn);

            // Optimized: Pre-calculate DLL path to avoid StringBuilder overhead
            string dll_path_str;
#if _x64
            if (acq_type == TCSPCType.BH_SPC150)
                dll_path_str = Path.Combine(parameters.spcData.BH_DLLDir, "spcm64.dll");
            else if (acq_type == TCSPCType.PQ_Th260)
                dll_path_str = "th260lib64";
            else if (acq_type == TCSPCType.PQ_MultiHarp)
                dll_path_str = "mhlib64";
            else if (acq_type == TCSPCType.PQ_PH330)
                dll_path_str = "PH330Lib";
            else if (acq_type == TCSPCType.PQ_HH500)
                dll_path_str = "hh500lib";
            else if (acq_type == TCSPCType.SPAD23)
                dll_path_str = string.Empty;
            else
                dll_path_str = string.Empty;
#else
            if (acq_type == TCSPCType.BH_SPC150)
                dll_path_str = Path.Combine(parameters.spcData.BH_DLLDir, "spcm32.dll");
            else if (acq_type == TCSPCType.PQ_Th260)
                dll_path_str = "th260lib";
            else
                dll_path_str = "mhlib";
#endif
            StringBuilder dll_path = new StringBuilder(260); //260 is for maximum path length for C++
            dll_path.Append(dll_path_str);

            int retcode = -101;
            bool startAttempted = false;

            try
            {
                int abiVersion = Get_TCSPC_Decode_ABI_Version();
                if (abiVersion != ExpectedTcspcDecodeAbiVersion)
                {
                    retcode = TcspcDecodeAbiMismatchRetCode;
                    Debug.WriteLine($"TCSPC_Decode ABI mismatch: expected {ExpectedTcspcDecodeAbiVersion}, got {abiVersion}.");
                }
                else
                {
                    startAttempted = true;
                    retcode = Start_TCSPC_Decode(deviceID, callback, ref pm, ref compID, dll_path);
                }
            }
            catch (EntryPointNotFoundException ex)
            {
                retcode = TcspcDecodeAbiMismatchRetCode;
                Debug.WriteLine("TCSPC_Decode ABI check failed: missing required entry point. " + ex.Message);
            }
            catch (DllNotFoundException ex)
            {
                Debug.WriteLine("Did not find DLL: " + ex.Message);
            }
            catch (Exception EX)
            {
                Debug.WriteLine("Did not find DLL: " + EX.Message);
            }

            parameters.ComputerID = compID.compID;

            completed_acq = true;

            DLLActive = (retcode == 0);
            DLLSerialGoThrough = startAttempted && retcode != -101;
        }

        // Optimized: Cache common string comparisons
        private static readonly string FrameDoneStr = "FrameDone";
        private static readonly string StripeDoneStr = "StripeDone";
        private static readonly string SaturatedStr = "Saturated";
        private static readonly string MeasurementDoneStr = "MeasurementDone";
        private static readonly string GetSyncRateStr = "GetSyncRate";

        public void CallbackReturn(int id, String str, int frame)
        {
            // Optimized: Early return for null reference
            if (refPQ[id] == null)
                return;

#if DEBUG
            if (DEBUGMODE >= 1)
            {
                // Optimized: Use IndexOf instead of Contains for better performance
                if (str.IndexOf(GetSyncRateStr, StringComparison.Ordinal) < 0)
                    Debug.WriteLine("Debug: C# CallbackReturn" + id + ": " + str);
            }
#endif

            // Allow trailing payloads/garbage in the callback string.
            if (!string.IsNullOrEmpty(str) && str.StartsWith(FrameDoneStr, StringComparison.Ordinal))
            {
                if (force_stop_event_received)
                    return;

                frameCounter = frame;

                // Optimized: Pre-calculate condition to avoid repeated evaluation
                bool process_event = parameters.read_from_file || (!DLL_Busy || !photon_file_saving_mode) || frameCounter == parameters.nFrames;
                if (process_event)
                {
                    GetData(); //Bringing data from DLL. (Copy from C++ memory bank)
#if DEBUG
                    if (DEBUGMODE >= 1)
                        Debug.WriteLine("Debug: C# Finished reading frames (GetData) = " + frameCounter);
#endif
                    HandleFrameEvent(id);
                }
            }
            else if (str.Length >= 10 && str.StartsWith(StripeDoneStr, StringComparison.Ordinal))
            {
                // Optimized: Manual parsing instead of Split to avoid allocation
                int comma1 = str.IndexOf(',', 10);
                int comma2 = str.IndexOf(',', comma1 + 1);
                if (comma1 > 0 && comma2 > comma1)
                {
                    int startL = int.Parse(str.Substring(comma1 + 1, comma2 - comma1 - 1));
                    int endL = int.Parse(str.Substring(comma2 + 1));
                    _CopyIntoStripe(startL, endL);
                    StripeDone?.Invoke(this, new StripeEventArgs(startL, endL, StripeBuffer));
                }
            }
            else if (ReferenceEquals(str, SaturatedStr) || str == SaturatedStr)
            {
#if DEBUG
                if (DEBUGMODE >= 1)
                    Console.WriteLine("Debug: Saturated!!");
#endif
                saturated = true;
                force_stop_event_received = true;
            }
            else if (str.StartsWith(MeasurementDoneStr, StringComparison.Ordinal))
            {
                //Note that frameCounter is read only for "FrameDone" event.
                if (frameCounter < frame)
                    FrameDone?.Invoke(this, new FrameEventArgs(frame, id, FLIMData, false));
#if DEBUG
                if (DEBUGMODE >= 1)
                    Debug.WriteLine("Debug: MeasurementDone event detected. frame = " + frameCounter);
#endif
                completed_acq = true;
                Running = false;
                DrainReleasedPhotonStreamHandles();

                MeasDone?.Invoke(this, null);
            }
        }

        public void HandleFrameEvent(int id)
        {
#if DEBUG
            Stopwatch sw = null;
            if (DEBUGMODE >= 1)
            {
                sw = Stopwatch.StartNew();
                Debug.WriteLine("Debug: C# FrameDone event detected. frame = " + frameCounter);
            }

            if (DEBUGMODE >= 2 && FLIMData != null && FLIMData.Length > 0 && FLIMData[0] != null && FLIMData[0].Length > 0)
            {
                // Optimized: Use direct array access instead of BlockCopy + ConvertAll + Sum
                ushort[,,] a = FLIMData[0][0];
                long sum = 0;
                int len0 = a.GetLength(0);
                int len1 = a.GetLength(1);
                int len2 = a.GetLength(2);
                for (int i = 0; i < len0; i++)
                    for (int j = 0; j < len1; j++)
                        for (int k = 0; k < len2; k++)
                            sum += a[i, j, k];
                Debug.WriteLine("NPhoton = " + sum + "; Frame = " + frameCounter);
            }

            if (DEBUGMODE >= 1 && sw != null)
                Debug.WriteLine("Debug:Time# 3 = " + sw.ElapsedMilliseconds);
#endif

            //if (focusing || photon_file_saving_mode)
            FrameDone?.Invoke(this, new FrameEventArgs(frameCounter, id, FLIMData, false));

#if DEBUG
            if (DEBUGMODE >= 1 && sw != null)
            {
                Debug.WriteLine("Debug:Time# 4 = " + sw.ElapsedMilliseconds);
                Debug.WriteLine("Debug: C# FrameDone Event came back...");
            }
#endif
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
            // Optimized: Early return for null or mismatched channel count
            if (buf == null || pm.nChannels != buf.Length)
                return false;

            // Optimized: Cache frequently accessed values
            int nChannels = pm.nChannels;
            int nZlocs = pm.nZlocs;
            int nLines = pm.nLines;
            int nPixels = pm.nPixels;

            for (int c = 0; c < nChannels; c++)
            {
                if (nDtime[c] != 0)
                {
                    // Optimized: Early return for null or mismatched z-loc count
                    if (buf[c] == null || nZlocs != buf[c].Length)
                        return false;

                    int nDtime_c = nDtime[c];
                    for (int z = 0; z < nZlocs; z++)
                    {
                        // Optimized: Early return for null buffer
                        if (buf[c][z] == null)
                            return false;

                        // Optimized: Check all dimensions in one pass
                        if (buf[c][z].GetLength(0) != nLines || 
                            buf[c][z].GetLength(1) != nPixels || 
                            buf[c][z].GetLength(2) != nDtime_c)
                            return false;
                    }
                }
            }

            return true;
        }

        private void _MakeStripe()
        {
            // Optimized: Cache frequently accessed values
            int nChannels = pm.nChannels;
            int nZlocs = pm.nZlocs;
            int nLines = pm.nLines;
            int nPixels = pm.nPixels;

            var data = new ushort[nChannels][][,,];
            for (int c = 0; c < nChannels; c++)
            {
                int nDtime_c = nDtime[c];
                if (nDtime_c != 0)
                {
                    data[c] = new ushort[nZlocs][,,];
                    for (int z = 0; z < nZlocs; z++)
                    {
                        data[c][z] = new ushort[nLines, nPixels, nDtime_c]; //Should allocate first.
                    }
                }
            }
            StripeBuffer = data;
        }

        public void _GetDataLinesFromMeasureBank(ushort[][][,,] destination, int startLine, int endLine)
        {
            // Optimized: Cache frequently accessed values and validate early
            if (destination == null)
                return;

            int nChannels = pm.nChannels;
            int nZlocs = pm.nZlocs;

            for (int c = 0; c < nChannels; c++)
            {
                if (nDtime[c] != 0 && destination[c] != null)
                {
                    for (int z = 0; z < nZlocs; z++)
                    {
                        if (destination[c][z] != null)
                            DE_GetDataLine(deviceID, destination[c][z], c, z, startLine, endLine);
                    }
                }
            }
        }


        public void GetData()
        {
            // IMPORTANT: FLIMage retains references to the frame buffers (shallow copy) for saving/analysis.
            // Therefore, each frame must have its own managed buffers so later frames don't overwrite prior frames.
            int nChannels = pm.nChannels;
            int nZlocs = pm.nZlocs;
            int nLines = pm.nLines;
            int nPixels = pm.nPixels;

            var data = new ushort[nChannels][][,,];
            for (int c = 0; c < nChannels; c++)
            {
                int nDtime_c = nDtime[c];
                if (nDtime_c != 0)
                {
                    data[c] = new ushort[nZlocs][,,];
                    for (int z = 0; z < nZlocs; z++)
                    {
                        data[c][z] = new ushort[nLines, nPixels, nDtime_c]; // allocate per frame
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
            force_stop_event_received = false;

            _setParameters(EraseMemory);
            frameCounter = 0;
            frameAveCounter = 0;

            // Optimized: Reduce retry attempts and use const for max retries
            const int maxRetries = 5;
            int retcode = -1;
            for (int i = 0; i < maxRetries; i++)
            {
                retcode = Start_Measurement(deviceID, ref pm);
                if (retcode == 0)
                {
                    Running = true;
                    return retcode; // Early return on success
                }
                Running = false;
#if DEBUG
                Debug.WriteLine("Debug: Start Measurement Failed. Restarting...");
#endif
                // Only sleep if not the last attempt
                if (i < maxRetries - 1)
                    System.Threading.Thread.Sleep(10);
            }

            return retcode;
        }

        public int restartEngine()
        {
            return RestartEngine(deviceID);
        }

        ////////////////////////////
        public int CloseDevice()
        {
            ReleaseAllPhotonStreamHandles();
            return CloseDevice(deviceID);
        }

        //////////////////////////////////////////////////////////////////

        public int TCSPC_StopMeas(bool force)
        {
            force_stop_event_received = force;
            int ret = Stop_Measurement(deviceID, force ? 1 : 0);
            ReleaseAllPhotonStreamHandles();
            return ret;
        }


        public int SetupPhotonDataFile(string fname)
        {
            ReleaseAllPhotonStreamHandles();

            // Optimized: Reuse StringBuilder with capacity to avoid reallocation
            var filename = new StringBuilder(fname, 260);
            return Setup_Photon_Data_File(deviceID, filename);
        }

        public int SetupPhotonDataWriteStream(Stream stream)
        {
            ClearPhotonDataWriteStream();

            if (stream == null)
                return -1;

            photon_write_stream = stream;

            if (!photon_write_callback_handle.IsAllocated)
                photon_write_callback_handle = GCHandle.Alloc(this);

            return Set_Photon_Write_Callback(deviceID, PhotonWriteCallbackHandler,
                GCHandle.ToIntPtr(photon_write_callback_handle));
        }

        public void ClearPhotonDataWriteStream()
        {
            photon_write_stream = null;

            if (photon_write_callback_handle.IsAllocated)
            {
                Clear_Photon_Write_Callback(deviceID);
                photon_write_callback_handle.Free();
            }
        }

        private void DrainReleasedPhotonStreamHandles()
        {
            lock (photon_stream_lock)
            {
                while (true)
                {
                    int released = Drain_Released_Photon_Stream_Handles(deviceID, photon_stream_release_buffer, photon_stream_release_buffer.Length);
                    if (released <= 0)
                        break;

                    for (int i = 0; i < released; i++)
                    {
                        IntPtr token = photon_stream_release_buffer[i];
                        photon_stream_release_buffer[i] = IntPtr.Zero;

                        if (token != IntPtr.Zero && photon_stream_handles.Remove(token))
                            GCHandle.FromIntPtr(token).Free();
                    }
                }
            }
        }

        private void ReleaseAllPhotonStreamHandles()
        {
            lock (photon_stream_lock)
            {
                while (true)
                {
                    int released = Drain_Released_Photon_Stream_Handles(deviceID, photon_stream_release_buffer, photon_stream_release_buffer.Length);
                    if (released <= 0)
                        break;

                    for (int i = 0; i < released; i++)
                    {
                        IntPtr token = photon_stream_release_buffer[i];
                        photon_stream_release_buffer[i] = IntPtr.Zero;

                        if (token != IntPtr.Zero && photon_stream_handles.Remove(token))
                            GCHandle.FromIntPtr(token).Free();
                    }
                }

                if (photon_stream_handles.Count == 0)
                    return;

                foreach (IntPtr token in photon_stream_handles.ToArray())
                {
                    if (token != IntPtr.Zero)
                        GCHandle.FromIntPtr(token).Free();
                }

                photon_stream_handles.Clear();
            }
        }

        public int SetupPhotonDataBinary(uint[] data)
        {
            ReleaseAllPhotonStreamHandles();

            // Avoid leaking pinned handles if this is called repeatedly.
            if (photon_binary_handle.IsAllocated)
                photon_binary_handle.Free();

            photon_binary_handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr ptr = photon_binary_handle.AddrOfPinnedObject();
            var success = Setup_Photon_Data_Binary(deviceID, ptr, data.Length);
            return success;
        }

        public int SetupPhotonDataStream()
        {
            ReleaseAllPhotonStreamHandles();
            return Setup_Photon_Data_Stream(deviceID);
        }

        public int AppendPhotonDataStream(uint[] data, int length, bool isLast)
        {
            if (data == null)
                data = Array.Empty<uint>();

            GCHandle handle = default;
            IntPtr token = IntPtr.Zero;
            try
            {
                IntPtr ptr = IntPtr.Zero;
                if (length > 0)
                {
                    handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    ptr = handle.AddrOfPinnedObject();
                    token = GCHandle.ToIntPtr(handle);
                }

                int ret = Append_Photon_Data_Stream(deviceID, ptr, length, isLast ? 1 : 0, token);
                if (ret == 0 && token != IntPtr.Zero)
                {
                    lock (photon_stream_lock)
                    {
                        photon_stream_handles.Add(token);
                    }
                }
                else if (handle.IsAllocated)
                {
                    handle.Free();
                    token = IntPtr.Zero;
                }

                DrainReleasedPhotonStreamHandles();
                return ret;
            }
            catch
            {
                if (handle.IsAllocated)
                    handle.Free();

                throw;
            }
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

                pq_param.trigger_mode = parameters.spcData.CFD_on;
                pq_param.sync_trigger_edge = parameters.spcData.sync_trigger_edge;
                pq_param.input_trigger_edge = parameters.spcData.input_trigger_edge;

                pq_param.hardware = 0;

                pq_param.input_deadtime = parameters.spcData.input_deadtime;

                retcode = PQ_ChannelOffsets(deviceID, parameters.spcData.ch_offset, parameters.spcData.ch_offset.Length);
                // Avoid LINQ/ToArray allocations on the acquisition path.
                var threshSrc = parameters.spcData.ch_threshold;
                int[] threshAll = new int[threshSrc.Length];
                for (int i = 0; i < threshSrc.Length; i++)
                    threshAll[i] = (int)threshSrc[i];
                retcode = PQ_ChannelThresholds(deviceID, threshAll, threshAll.Length);
                retcode = PQ_AllParameters(deviceID, ref pq_param);

                if (retcode == 0)
                {
                    for (int i = 0; i < nChannelsPerDevice; i++)
                        parameters.spcData.resolution[deviceID * nChannelsPerDevice + i] = pq_param.resolution;

                    if (pq_param.hardware == (int)PQHardware.TH260N)
                        parameters.spcData.HW_Model = "THarp 260 N";
                    else if (pq_param.hardware == (int)PQHardware.TH260P)
                        parameters.spcData.HW_Model = "THarp 260 P";
                    else if (pq_param.hardware == (int)TCSPCType.PQ_HH500)
                        parameters.spcData.HW_Model = "HydraHarp 500";
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
            if (parameters.spcData.BoardType.ToLower() == "simpq")
            {
                int syncRate = 80000000;
                int chRate0 = 1000000;
                int chRate1 = 1000000;
                if (parameters.rateInfo?.syncRate != null && parameters.rateInfo.syncRate.Length > 0 && parameters.rateInfo.syncRate[0] > 0)
                    syncRate = parameters.rateInfo.syncRate[0];
                if (parameters.rateInfo?.countRate != null && parameters.rateInfo.countRate.Length > 0 && parameters.rateInfo.countRate[0] > 0)
                    chRate0 = parameters.rateInfo.countRate[0];
                if (parameters.rateInfo?.countRate != null && parameters.rateInfo.countRate.Length > 1 && parameters.rateInfo.countRate[1] > 0)
                    chRate1 = parameters.rateInfo.countRate[1];
                rate_info = new RateInfo
                {
                    sync_rate = syncRate,
                    ch_rate0 = chRate0,
                    ch_rate1 = chRate1
                };
            }
            else
            {
                rate_info = new RateInfo
                {
                    sync_rate = 0,
                    ch_rate0 = 0,
                    ch_rate1 = 0
                };
            }

            int retcode = GetRate(deviceID, ref rate_info);

            if (retcode == 0)
            {
                parameters.rateInfo.syncRate[deviceID * nChannelsPerDevice] = rate_info.sync_rate;

                int[] channels = DeviceChannelID();
                parameters.rateInfo.countRate[channels[0]] = rate_info.ch_rate0;

                if (nChannelsPerDevice > 1)
                    parameters.rateInfo.countRate[channels[1]] = rate_info.ch_rate1;

                if (nChannelsPerDevice > 2)
                    parameters.rateInfo.countRate[channels[2]] = rate_info.ch_rate2;

                if (nChannelsPerDevice > 3)
                    parameters.rateInfo.countRate[channels[3]] = rate_info.ch_rate3;
            }
            return retcode;
        }

        public bool TryGetFileWriterStats(out FileWriterStats stats)
        {
            stats = new FileWriterStats();
            if (!DLLActive)
                return false;

            try
            {
                int retcode = Get_FileWriter_Stats(deviceID, ref stats);
                return retcode >= 0;
            }
            catch (EntryPointNotFoundException)
            {
                return false;
            }
            catch (DllNotFoundException)
            {
                return false;
            }
        }

        private void _setParameters(bool[] eraseMemory)
        {
            bool use_bh = parameters.spcData.BoardType == "BH";
            int[] acqFLIM = new int[maxNChannels];
            int[] acq = new int[maxNChannels];
            int[] EraseM = new int[maxNChannels];
            int[] aveFrameA = new int[maxNChannels];
            nDtime = new int[maxNChannels];

            photon_file_saving_mode = parameters.spcData.savePhotonsInFile && !parameters.read_from_file;

            if (use_bh)
                nChannelsPerDevice = parameters.spcData.channelPerDeviceBH;
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

                if (photon_file_saving_mode)
                {
                    if (focusing)
                        aveFrameA[i] = parameters.focusAverage > 0 ? 1 : 0;
                    else
                    {
                        aveFrameA[i] = parameters.averageFrame[nChannelsPerDevice * deviceID + i] ? 1 : 0;
                        if (aveFrameA[i] == 0)
                        {
                            aveFrameA[i] = parameters.focusAverage > 0 ? 1 : 0;
                        }
                    }
                }
                else
                {
                    if (focusing)
                        aveFrameA[i] = (parameters.focusAverage) > 0 ? 1 : 0;
                    else
                        aveFrameA[i] = parameters.averageFrame[nChannelsPerDevice * deviceID + i] ? 1 : 0;
                }
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
            else if (parameters.spcData.BoardType == "PH")
                acq_type = TCSPCType.PQ_PH330;
            else if (parameters.spcData.BoardType == "HH")
                acq_type = TCSPCType.PQ_HH500;
            else if (parameters.spcData.BoardType == "SPAD")
                acq_type = TCSPCType.SPAD23;
            else if (parameters.spcData.BoardType.ToUpper() == "SIMPQ")
                acq_type = TCSPCType.simpq;

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

            pm.lineClockDivision = parameters.spcData.line_clock_division < 1 ? 1 : parameters.spcData.line_clock_division; //Usually 1. 

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

            if (photon_file_saving_mode)
            {
                if (focusing)
                {
                    pm.n_average = parameters.focusAverage;
                }
                else
                {
                    pm.n_average = parameters.n_average;
                    if (pm.n_average <= 1)
                        pm.n_average = parameters.focusAverage;
                }
            }
            else
            {
                if (focusing)
                    pm.n_average = parameters.focusAverage;
                else
                    pm.n_average = parameters.n_average;
            }

            pm.pixel_time = parameters.pixel_time;
            pm.resolution = parameters.spcData.resolution[0]; //resolution = (int)parameters.spcData.resolution[0];

            pm.StripeDuringFocus = parameters.StripeDuringFocus ? 1 : 0;
            pm.focus = focusing ? 1 : 0;
            pm.LinesPerStripe = parameters.LinesPerStripe;

            pm.pixel_binning = parameters.spcData.pixel_binning;
            pm.skipFirstLines = parameters.spcData.SkipFirstLines;
            pm.skipFirstFrames = parameters.spcData.SkipFirstFrames;

            pm.TagID = (use_bh) ? parameters.spcData.TagID : parameters.spcData.TagID - 1;
            pm.LineID = (use_bh) ? parameters.spcData.lineID_BH : parameters.spcData.lineID_PQ - 1;
            pm.FrameID = (use_bh) ? parameters.spcData.FrameID : parameters.spcData.FrameID - 1;

            pm.eraseMemory0 = EraseM[0];
            pm.eraseMemory1 = EraseM[1];
            pm.eraseMemory2 = EraseM[2];
            pm.eraseMemory3 = EraseM[3];

            pm.savePhotonsInFile = parameters.spcData.savePhotonsInFile ? 1 : 0;
            pm.readFromPhotonFile = parameters.read_from_file ? 1 : 0;

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

            pm.bundle_all_channels = parameters.spcData.bundle_all_channels;

            compID.compID = parameters.ComputerID;
            compID.FLIMID = parameters.FLIMserial;

            // Fiber photometry mode (native time-binning into 1x1 frames)
            pm.fiberPhotometryMode = parameters.fiberPhotometryMode ? 1 : 0;
            pm.fiberBin_ms = parameters.fiberBin_ms;
            pm.fiberStartMode = parameters.fiberStartMode;
            pm.lineScanMode = parameters.lineScanMode ? 1 : 0;
        }


        /////////

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Get_ComputerID", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Get_ComputerID();

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Get_TCSPC_Decode_ABI_Version", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Get_TCSPC_Decode_ABI_Version();

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Start_TCSPC_Decode", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Start_TCSPC_Decode(int id, GetMessageDelegate callback, ref DE_parameters param1, ref CompID comID, StringBuilder dll_path);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Setup_Photon_Data_File", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Setup_Photon_Data_File(int id, StringBuilder photon_data_file);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Setup_Photon_Data_Binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Setup_Photon_Data_Binary(int id, IntPtr data, int length);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Setup_Photon_Data_Stream", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Setup_Photon_Data_Stream(int id);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Append_Photon_Data_Stream", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Append_Photon_Data_Stream(int id, IntPtr data, int length, int isLast, IntPtr token);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Drain_Released_Photon_Stream_Handles", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Drain_Released_Photon_Stream_Handles(int id, [Out] IntPtr[] tokens, int maxCount);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PhotonWriteCallback(IntPtr data, int length, IntPtr user);

        private static readonly PhotonWriteCallback PhotonWriteCallbackHandler = OnPhotonWriteCallback;

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Set_Photon_Write_Callback", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Set_Photon_Write_Callback(int id, PhotonWriteCallback cb, IntPtr user);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "Clear_Photon_Write_Callback", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Clear_Photon_Write_Callback(int id);

        private static int OnPhotonWriteCallback(IntPtr data, int length, IntPtr user)
        {
            if (user == IntPtr.Zero)
                return -1;

            var handle = GCHandle.FromIntPtr(user);
            if (!(handle.Target is TCSPC_Native native))
                return -1;

            return native.WritePhotonStream(data, length);
        }

        private int WritePhotonStream(IntPtr data, int length)
        {
            if (length <= 0)
                return 0;

            var stream = photon_write_stream;
            if (stream == null)
                return -1;

            int byteCount = length * sizeof(uint);
            lock (photon_write_lock)
            {
                if (photon_write_buffer == null || photon_write_buffer.Length < byteCount)
                    photon_write_buffer = new byte[byteCount];
                System.Runtime.InteropServices.Marshal.Copy(data, photon_write_buffer, 0, byteCount);
                stream.Write(photon_write_buffer, 0, byteCount);
            }

            return 0;
        }
        [DllImport("TCSPC_Decode.dll", EntryPoint = "Get_FileWriter_Stats", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Get_FileWriter_Stats(int id, ref FileWriterStats stats);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "PQ_AllParameters", CallingConvention = CallingConvention.Cdecl)]
        private static extern int PQ_AllParameters(int id, ref PQ_Parameters pap);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "PQ_ChannelOffsets", CallingConvention = CallingConvention.Cdecl)]
        private static extern int PQ_ChannelOffsets(int id, int[] offsets, int length);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "PQ_ChannelThresholds", CallingConvention = CallingConvention.Cdecl)]
        private static extern int PQ_ChannelThresholds(int id, int[] offsets, int length);

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

        [DllImport("TCSPC_Decode.dll", EntryPoint = "RestartEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern int RestartEngine(int id);


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

            public int trigger_mode; //0 for threshold, 1 for CFD. Only for PH330. Others are automatic.
            public int input_trigger_edge; //0 for falling, 1 for rising
            public int sync_trigger_edge;

            public int hardware;

            public int input_deadtime;
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
        public struct FileWriterStats
        {
            public long bytes_written;
            public int queue_max_depth;
            public int enqueue_block_count;
            public int queue_overflow_count;
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
            public int skipFirstFrames;
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

            public int lineClockDivision;

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

            public int bundle_all_channels;
            public int debug;

            // Fiber photometry (single point) mode
            public int fiberPhotometryMode; // 0/1
            public double fiberBin_ms;      // bin width in ms (default 20)
            public int fiberStartMode;      // 0 = soft (first photon), 1 = external marker (FrameID)
            public int lineScanMode;        // 0/1
        }//struct

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct CompID
        {
            public int compID;
            public int FLIMID;
        }

    }//PQNative


}
