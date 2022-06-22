using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCSPC_controls
{
    public class FiFio_multiBoards
    {
        public int nDevices = 2;
        public int channelsPerDevice = 1;
        public int nChannels;

        public TCSPCType board_type = TCSPCType.PQ_Th260;

        public bool Running = false;
        public bool force_stop = false;
        public bool averageFrame = false;
        public bool focusing = false;
        public bool saturated = false;
        public bool enableFastZscan = false;
        public bool simulation_mode = false;

        public TCSPC_Native FLIM_N; //Testing now, but not used yet.
        public List<TCSPC_Native> FLIM_FiFoList = new List<TCSPC_Native>();

        public UInt16[][][,,] FLIM_Stripe;
        public UInt16[][][,,] FLIM_data;
        public List<UInt16[][][,,]> FLIM_data_buffer;
        public List<UInt16[][][,,]> FLIM_data_completed;

        //public bool[] frameDoneBool;
        public int frameCounter = 0;
        public int deletedFrame = 0;

        List<bool[]> finishedArray = new List<bool[]>();


        object sync_acq = new object();
        object syncBuffer = new object();
        public FLIM_Parameters parameters;

        public event FrameDoneHandler FrameDone;
        public FrameEventArgs e_frame = null;
        public delegate void FrameDoneHandler(FiFio_multiBoards fifo_m, FrameEventArgs e_frame);

        public event MeasDoneHandler MeasDone;
        public delegate void MeasDoneHandler(FiFio_multiBoards fifo_m, EventArgs e);

        public event StripeDoneHandler StripeDone;
        public delegate void StripeDoneHandler(FiFio_multiBoards fifo_m, StripeEventArgs e2);

        public FiFio_multiBoards(FLIM_Parameters flim_parameters)
        {
            parameters = flim_parameters;
            String boardType = parameters.spcData.BoardType;

            enableFastZscan = flim_parameters.enableFastZscan;

            switch (boardType)
            {
                case "BH":
                    board_type = TCSPCType.BH_SPC150;
                    nDevices = parameters.spcData.n_devicesBH;
                    channelsPerDevice = parameters.spcData.channelPerDeviceBH;
                    if (parameters.spcData.n_devicesBH * parameters.spcData.channelPerDeviceBH != parameters.nChannels)
                        channelsPerDevice = parameters.nChannels / parameters.spcData.n_devicesBH;
                    break;
                case "MH":
                    board_type = TCSPCType.PQ_MultiHarp;
                    nDevices = parameters.spcData.n_devicesPQ;
                    channelsPerDevice = parameters.spcData.channelPerDevicePQ;
                    if (parameters.spcData.n_devicesPQ * parameters.spcData.channelPerDevicePQ != parameters.nChannels)
                        channelsPerDevice = parameters.nChannels / parameters.spcData.n_devicesPQ;
                    break;
                case "PQ":
                    board_type = TCSPCType.PQ_Th260;
                    nDevices = parameters.spcData.n_devicesPQ;
                    channelsPerDevice = parameters.spcData.channelPerDevicePQ;
                    if (parameters.spcData.n_devicesPQ * parameters.spcData.channelPerDevicePQ != parameters.nChannels)
                        channelsPerDevice = parameters.nChannels / parameters.spcData.n_devicesPQ;
                    break;
                default:
                    board_type = TCSPCType.PQ_Th260;
                    nDevices = parameters.spcData.n_devicesPQ;
                    channelsPerDevice = parameters.spcData.channelPerDevicePQ;
                    if (parameters.spcData.n_devicesPQ * parameters.spcData.channelPerDevicePQ != parameters.nChannels)
                        channelsPerDevice = parameters.nChannels / parameters.spcData.n_devicesPQ;
                    break;
            }

            parameters.spcData.nDevices = nDevices;
            parameters.spcData.channelPerDevice = channelsPerDevice;

        }

        public int computer_id()
        {
            try
            {
                int id = TCSPC_Native.Get_ComputerID();
                return id;
            }
            catch
            {
                return 0;
            }
        }

        public void startSimulationMode()
        {
            simulation_mode = true;
        }


        public enum ErrorCode
        {
            DLL_NOTFOUND = 1,
            COMPUTERID_INCORRECT = 2,
            BOARD_INITIALIZE_FAILURE = 3,
            BOARD_OPEN_FAILURE = 4,
            PARAMETER_ERROR = 5,
            NONE = 6,
        }

        public ErrorCode Initialize()
        {
            short retcode = 0;
            simulation_mode = true;

            if (board_type == TCSPCType.BH_SPC150) //for BH, it is necessary to find a library.
            {
                String dllFolder = parameters.spcData.BH_DLLDir;

                bool use_bh = true;
                if (Directory.Exists(dllFolder))
                {
                    parameters.spcData.BH_init_file = dllFolder + "\\" + "spcm.ini";

                    if (!File.Exists(dllFolder + "\\" + "spcm64.dll"))
                        use_bh = false;
                    else
                        Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + dllFolder);
                }
                else
                    use_bh = false;

                if (!use_bh)
                {
                    return ErrorCode.DLL_NOTFOUND;
                }
            }

            parameters.ComputerID = computer_id();

            if (parameters.ComputerID == 0)
                return ErrorCode.DLL_NOTFOUND;

            for (short i = 0; i < nDevices; i++)
            {
                bool createNew = TCSPC_Native.CreateTCSPC_Native(parameters, i);
                FLIM_N = TCSPC_Native.refPQ[i];

                if (FLIM_N == null)
                {
                    return ErrorCode.DLL_NOTFOUND;
                }

                if (FLIM_N.DLLActive)
                {
                    FLIM_N.Set_allParameters(parameters);
                }
                else
                {
                    if (!FLIM_N.DLLSerialGoThrough)
                        return ErrorCode.COMPUTERID_INCORRECT;

                    if (!FLIM_N.DLLActive)
                        return ErrorCode.DLL_NOTFOUND;
                }

                if (retcode < 0)
                {
                    return ErrorCode.PARAMETER_ERROR;
                }

                if (createNew) //should do only once.
                {
                    FLIM_N.deviceID = i;
                    FLIM_FiFoList.Add(FLIM_N);
                    FLIM_FiFoList[i].FrameDone += new TCSPC_Native.FrameDoneHandler(AcquireOne);
                    FLIM_FiFoList[i].StripeDone += new TCSPC_Native.StripeDoneHandler(AcquireStripe);
                    FLIM_FiFoList[i].MeasDone += new TCSPC_Native.MeasDoneHandler(MeasDoneHandle);
                }
            }

            simulation_mode = false;
            return ErrorCode.NONE;
        }

        public void SetupParameters(bool focus, FLIM_Parameters parametersInput)
        {
            focusing = focus;
            parameters = parametersInput;

            if (simulation_mode)
                return;

            for (short i = 0; i < nDevices; i++)
            {
                FLIM_FiFoList[i].parameters = parameters;

                if (!simulation_mode)
                    FLIM_FiFoList[i].Set_allParameters(parameters);
            }

        }

        public void RunSimulationData()
        {
            int id = 0;
            Running = true;
            simulation_mode = true;

            Task.Factory.StartNew((Action)delegate
            {
                var FLIMData = new ushort[parameters.nChannels][][,,];
                for (int ch = 0; ch < FLIMData.Length; ch++)
                {
                    FLIMData[ch] = new ushort[1][,,];
                    int nDtime = parameters.acquireFLIM[ch] ? parameters.nDtime : 1;
                    FLIMData[ch][0] = new ushort[parameters.nLines, parameters.nPixels, nDtime];
                }


                int n_average = 1;
                int[] average_counter = new int[nChannels];
                int nFrame = focusing ? 32767 : parameters.nFrames;

                if (!focusing && parameters.n_average > 1)
                    n_average = parameters.n_average;
                else if (focusing && parameters.focusAverage > 1)
                    n_average = parameters.focusAverage;

                double resN = 0.2; //Resolution in nanoseconds.
                double[] beta2 = new double[] { 0.5, 1 / (2.6 / resN), 0.2, 1 / (0.5 / resN), 0.15 / resN, 2.0 / resN };
                double pulseI = 12.5 / resN;


                for (int frameCounter = 0; frameCounter < nFrame; frameCounter++)
                {
                    int stripe1 = frameCounter % 4 + 1;
                    //Create frame here!
                    for (int ch = 0; ch < FLIMData.Length; ch++)
                    {
                        var data1 = MathLibrary.ImageProcessing.CreateFLIM_Sim(parameters.nDtime, parameters.nLines,
                            parameters.nPixels, beta2, pulseI, stripe1);

                        if (average_counter[ch] == 0)
                            FLIMData[ch][0] = data1;
                        else
                            FLIMData[ch][0] = MathLibrary.MatrixCalc.MatrixCalc3D(FLIMData[ch][0], data1, MathLibrary.CalculationType.Add);

                        average_counter[ch]++;
                        if (average_counter[ch] == n_average || !parameters.averageFrame[ch])
                            average_counter[ch] = 0;
                    }

                    FrameDone?.Invoke(this, new FrameEventArgs(frameCounter, id, FLIMData));
                    if (!Running)
                        break;
                }

                MeasDone?.Invoke(this, null);
                Running = false;
            });


        }

        public void StopSimulation()
        {
            Running = false;
        }



        public void StartMeas(bool[] EraseMemory, bool focus, bool measureTagParameters)
        {
            force_stop = false;
            focusing = focus;
            saturated = false;
            nChannels = nDevices * channelsPerDevice;

            parameters.fastZScan.measureTagParameters = measureTagParameters;

            if (parameters.enableFastZscan)
            {
                if (parameters.fastZScan.phase_detection_mode)
                    parameters.nFastZSlices = parameters.fastZScan.nFastZSlices * 2;
                else
                    parameters.nFastZSlices = parameters.fastZScan.nFastZSlices;
            }
            else
                parameters.nFastZSlices = 1;

            int nZScan = parameters.nFastZSlices;


            FLIM_data = new UInt16[nChannels][][,,];
            FLIM_Stripe = new ushort[nChannels][][,,];
            FLIM_data_buffer = new List<UInt16[][][,,]>();
            FLIM_data_completed = new List<UInt16[][][,,]>();
            frameCounter = 0;
            deletedFrame = 0;

            if (!simulation_mode)
                simulation_mode = FLIM_FiFoList == null;

            if (!simulation_mode)
            {
                for (short i = 0; i < nDevices; i++)
                {
                    bool[] eraseMemoryEach = new bool[channelsPerDevice];
                    for (int ch = 0; ch < channelsPerDevice; ch++)
                        eraseMemoryEach[ch] = EraseMemory[i * channelsPerDevice + ch];

                    FLIM_FiFoList[i].TCSPC_StartMeas(eraseMemoryEach, focus); //This includes BackGround.
                    Running = Running || FLIM_FiFoList[i].Running;
                }
                System.Threading.Thread.Sleep(1);
            }
            else
                RunSimulationData();
            //if (WaitForStart(1000) != 0)
            //    Debug.WriteLine("Start may be failed!!");
        }

        private int WaitForStart(int timeOutMilliseconds)
        {
            int retcode = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!IfStarted())
            {
                System.Threading.Thread.Sleep(1);
                if (sw.ElapsedMilliseconds > timeOutMilliseconds)
                {
                    retcode = -1;
                    break;
                }
            }

            Debug.WriteLine("Started at " + sw.ElapsedMilliseconds + " ms");

            return retcode;
        }

        private bool IfStarted()
        {
            bool allActive = false;
            bool[] acquring = new bool[nDevices];
            for (short j = 0; j < nDevices; j++)
            {
                acquring[j] = FLIM_FiFoList[j].Running;
                allActive = acquring.All(x => x == true);
            }

            return allActive;
        }

        public void StopMeas(bool force)
        {
            if (simulation_mode)
            {
                Running = false;
                return;
            }

            Task.Factory.StartNew((Action)delegate
            {
                for (short i = 0; i < nDevices; i++)
                {
                    FLIM_FiFoList[i].TCSPC_StopMeas(force);
                }
            });

            force_stop = force;
            Running = false;
            ClearMemory();
        }

        public void AcquireStripe(TCSPC_Native tcspc_fifo, StripeEventArgs e)
        {
            if (focusing)
            {
                int nChEach;
                int id;
                int nZscan = parameters.nFastZSlices;
                nChEach = tcspc_fifo.nChannelsPerDevice;
                id = tcspc_fifo.deviceID;

                for (int ch = 0; ch < nChEach; ch++)
                    FLIM_Stripe[id * nChEach + ch] = e.data[ch];

                e.data = FLIM_Stripe;

                StripeDone(this, e);
            }
        }

        public bool IsCompleted()
        {
            if (simulation_mode)
                return true;

            bool done = true;
            for (short i = 0; i < nDevices; i++)
            {
                done = done && FLIM_FiFoList[i].IsCompleted();
            }
            return done;
        }

        public void MeasDoneHandle(TCSPC_Native tcspc_fifo, EventArgs e)
        {
            if (tcspc_fifo.saturated)
                saturated = true;
            MeasDone?.Invoke(this, e);
        }

        public void handleMultiBoard(TCSPC_Native tcspc_fifo, FrameEventArgs e)
        {
            UInt16[][][,,] data;
            int nChEach;
            int frameN;
            int id;

            if (e != null)
                data = e.data;
            else
                data = null;

            nChEach = tcspc_fifo.nChannelsPerDevice;

            if (e != null)
                frameN = e.frameNumber;
            else
                frameN = 0;

            id = tcspc_fifo.deviceID;
            saturated = tcspc_fifo.saturated;


            if (data == null)
                return;

            if (saturated)
                return;

            if (frameN >= frameCounter) //make new frame if not existing.
            {
                int nAdd = frameN - frameCounter;
                for (int i = 0; i <= nAdd; i++)
                {
                    FLIM_data = new ushort[nChannels][][,,]; //New file.
                    FLIM_data_buffer.Add(FLIM_data);
                    finishedArray.Add(new bool[nChannels]);
                    frameCounter++;
                }
            }


            int frameToWork = frameN - 1 - deletedFrame;

            if (FLIM_data_buffer.Count > frameToWork && frameToWork >= 0) //Prevent crash during the abort.
            {
                for (int ch = 0; ch < nChEach; ch++)
                {
                    FLIM_data_buffer[frameToWork][id * nChEach + ch] = data[ch];
                    finishedArray[frameToWork][id * nChEach + ch] = true;
                }

                //Debug.WriteLine("Image = " + frameN + "," + id + ", deleted " + deletedFrame + "{ " + finishedArray[frameToWork][0] + ", " + finishedArray[frameToWork][1] + "}");


                if (finishedArray[frameToWork].All(x => x == true))
                {
                    FLIM_data = (ushort[][][,,])FLIM_data_buffer[frameToWork];
                    if (FrameDone != null)
                    {
                        e_frame = new FrameEventArgs(frameN, 0, FLIM_data);
                        FrameDone(this, e_frame); //It filled two chanels.
                        RemoveFrameAt(frameToWork); //this includes counting of deleteCounter.
                    }
                }
            }
            else
            {
                Debug.WriteLine("******************Perhaps saturated: Frame Number = " + frameN);
            }
        }



        public void AcquireOne(TCSPC_Native tcspc_fifo, FrameEventArgs e)
        {
            lock (sync_acq)
            {
                if (nDevices == 1)
                    FrameDone?.Invoke(this, e);
                else if (nDevices > 1)
                    handleMultiBoard(tcspc_fifo, e);
            } //loc
        }


        private void ClearMemory()
        {
            if (FLIM_data_buffer != null)
                FLIM_data_buffer.Clear();

            finishedArray.Clear();
        }

        private void RemoveFrameAt(int frameToWork)
        {
            if (frameToWork < FLIM_data_buffer.Count)
            {
                FLIM_data_buffer.RemoveAt(frameToWork);
                finishedArray.RemoveAt(frameToWork);
                deletedFrame++;
            }
            else
            {
                Debug.WriteLine("Could not delete File: MultiFLIMBOards.RemoveFrameAt(" + frameToWork + ")");
            }
        }

        /// <summary>
        /// 0 for no error.
        /// </summary>
        /// <returns></returns>
        public int GetRate()
        {
            if (simulation_mode)
            {
                parameters.rateInfo.syncRate = new int[] { 80 * 1000 * 1000, 80 * 1000 * 1000 };
                parameters.rateInfo.countRate = new int[] { 1234, 1234 };
                return 0;
            }

            int retcode = 0;
            try
            {
                for (short i = 0; i < nDevices; i++)
                    retcode = FLIM_FiFoList[i].GetRate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in GetRate, MultiFLIMBoards" + ex.Message);
            }

            return retcode;
        }

        public void closeDevice()
        {
            if (simulation_mode)
                return;

            for (short i = 0; i < nDevices; i++)
            {
                if (FLIM_FiFoList.Count > i)
                    FLIM_FiFoList[i].CloseDevice();
            }
        }


    } //multiboards

    public enum TCSPCType
    {
        BH_SPC150 = 1,
        PQ_Th260 = 2,
        PQ_MultiHarp = 3,
        SI_TimeTagger = 4,
    }

    public class FrameEventArgs : EventArgs
    {
        public int frameNumber;
        public int device = 0;
        public UInt16[][][,,] data;
        public List<int> channelList = new List<int>();
        public FrameEventArgs(int _frameNumber, int _device, UInt16[][][,,] _data)
        {
            frameNumber = _frameNumber;
            device = _device;
            data = _data;
        } // eo ctor
    } // eo class StripeEventArgs

    public class StripeEventArgs : EventArgs
    {
        public int StartLine;
        public int EndLine;
        public int device = 0;
        public List<int> channelList = new List<int>();
        public UInt16[][][,,] data;
        public StripeEventArgs(int _startL, int _endL, UInt16[][][,,] _data)
        {
            StartLine = _startL;
            EndLine = _endL;
            data = _data;
        } // eo ctor
    } // eo class StripeEventArgs

}
