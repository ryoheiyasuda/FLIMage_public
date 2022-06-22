using NationalInstruments;
using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MicroscopeHardwareLibs
{
    public class NiDaq
    {
        static public bool DLLactive = false;

        public class AccessKey
        {
            public AccessKey(int serialKey)
            {
                try
                {
                    var compID = Get_ComputerID();
                    if (Check_SerialKey(serialKey) == 1)
                        DLLactive = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            [DllImport("TCSPC_Decode.dll", EntryPoint = "Get_ComputerID", CallingConvention = CallingConvention.Cdecl)]
            public static extern int Get_ComputerID();
            [DllImport("TCSPC_Decode.dll", EntryPoint = "Check_SerialKey", CallingConvention = CallingConvention.Cdecl)]
            public static extern int Check_SerialKey(int serialKey);
        }


        public static void ConnectTerminals(String Port1, String Port2)
        {
            DaqSystem.Local.ConnectTerminals(Port1, Port2);
        }


        public class AO_Write
        {
            public Task AO;
            public AnalogSingleChannelWriter writer;
            public double maxV = 10;
            public double minV = -10;

            public AO_Write(String port, double value, double[] range)
            {
                if (DLLactive)
                {
                    maxV = range.Max();
                    minV = range.Min();
                    AO = new Task();
                    AO.AOChannels.CreateVoltageChannel(port, "Port1", minV, maxV, AOVoltageUnits.Volts);
                    AO.Control(TaskAction.Verify);
                    writer = new AnalogSingleChannelWriter(AO.Stream);
                    writer.WriteSingleSample(true, value);
                    AO.Dispose();
                }
            }
        }

        public class AI_Read_SingleValue
        {
            public double maxV = 10;
            public double minV = -10;

            public AI_Read_SingleValue(String port, double[] range, out double result)
            {
                if (DLLactive)
                {
                    maxV = range.Max();
                    minV = range.Min();
                    var AI = new Task();
                    AI.AIChannels.CreateVoltageChannel(port, "Port1", (AITerminalConfiguration)(-1), minV, maxV, AIVoltageUnits.Volts);
                    AI.Control(TaskAction.Verify);
                    var reader = new AnalogSingleChannelReader(AI.Stream);
                    var value = reader.ReadSingleSample();
                    AI.Dispose();
                    result = value;
                }
                else
                    result = double.NaN;
            }
        }


        public class CounterInput
        {
            Task counter;
            String board;
            bool port_active = false;
            CounterSingleChannelReader counterReader;

            public event EveryNSamplesEventHandler EveryNSamplesEvent;
            public delegate void EveryNSamplesEventHandler(CounterInput ao_task, EventArgs e);

            public CounterInput(String counterPort)
            {
                if (DLLactive)
                {
                    port_active = false;
                    counter = new Task();
                    board = counterPort.Split('/')[0];
                    counter.CIChannels.CreateCountEdgesChannel(counterPort, "Pulse counter", CICountEdgesActiveEdge.Rising, 0, CICountEdgesCountDirection.Up);
                    counter.Timing.ConfigureImplicit(SampleQuantityMode.ContinuousSamples);
                    counter.EveryNSamplesReadEventInterval = 512;
                    counter.EveryNSamplesRead += new EveryNSamplesReadEventHandler(TaskDoneEverySample);
                }
            }

            public void TaskDoneEverySample(object sender, EveryNSamplesReadEventArgs e)
            {
                var reading = counterReader.ReadSingleSampleUInt32();
                Debug.WriteLine("Counter Number = " + reading);
            }

            void EveryNSampleEventFcn(object sender, EveryNSamplesWrittenEventArgs e)
            {
                EveryNSamplesEvent?.Invoke(this, null);
            }

            public void Start()
            {
                if (counter != null)
                {
                    counterReader = new CounterSingleChannelReader(counter.Stream);
                    counter.Start();
                    port_active = true;
                }
            }

            public void Stop()
            {
                if (counter != null && port_active)
                {
                    var reading = counterReader.ReadSingleSampleUInt32();
                    counter.Stop();
                    counter.WaitUntilDone();
                    port_active = false;
                }
            }

            public void Dispose()
            {
                if (counter != null && !port_active)
                    counter.Dispose();
            }
        }


        public class CounterOutput
        {
            Task counter;
            String board;
            bool port_active = false;

            public CounterOutput(String counterPort, String trigPort, double delay_microseconds, double frequency, double dutyCycle)
            {
                if (DLLactive)
                {
                    port_active = false;
                    counter = new Task();
                    board = counterPort.Split('/')[0];
                    //counter.COChannels.CreatePulseChannelFrequency(counterPort, "PulseTrain", COPulseFrequencyUnits.Hertz, COPulseIdleState.Low, delay, frequency, dutyCycle);
                    //OK, PCIe-6323 is 100 MHz time base.
                    int cnt = (int)(100e6 / frequency / 2);
                    int delay_cnt = (int)(delay_microseconds * 100); // 1 cnt = 1/100 microseconds. 
                    counter.COChannels.CreatePulseChannelTicks(counterPort, "", "/" + board + "/" + "100MHzTimebase", COPulseIdleState.Low, delay_cnt, cnt, cnt);
                    counter.Triggers.StartTrigger.DigitalEdge.Source = "/" + board + "/" + trigPort;
                    counter.Triggers.StartTrigger.Type = StartTriggerType.DigitalEdge;
                    counter.Triggers.StartTrigger.DigitalEdge.Edge = DigitalEdgeStartTriggerEdge.Rising;
                    counter.Timing.ConfigureImplicit(SampleQuantityMode.ContinuousSamples);
                }
            }

            public void Start()
            {
                if (counter != null)
                {
                    try
                    {
                        counter.Start();
                        port_active = true;
                    }
                    catch (Exception EX)
                    {
                        Debug.WriteLine("Problem in Starting counter: " + EX.Message);
                    }
                }
            }

            public void Stop()
            {
                if (counter != null && port_active)
                {
                    counter.Stop();
                    counter.WaitUntilDone();
                    port_active = false;
                }
            }

            public void Dispose()
            {
                if (counter != null && !port_active)
                    counter.Dispose();
            }
        }


        public class DigitalOutputPort
        {
            private String DO_Port;
            private String board;

            private Task myTask;
            //private Task OnOfTaskAll;
            private DigitalWaveform[] waveform_output;
            private DigitalMultiChannelWriter writer;
            private bool portActive = false;

            public DigitalOutputPort(String DigitalPort, byte[] data_out)
            {
                if (DLLactive)
                {
                    DO_Port = DigitalPort;
                    board = DO_Port.Split('/')[0];
                    PutSingleValue(data_out);
                }
            }

            private int PutSingleValue(byte[] data_out)
            {
                Dispose();
                myTask = new Task();
                myTask.DOChannels.CreateChannel(DO_Port, "", ChannelLineGrouping.OneChannelForAllLines);

                writer = new DigitalMultiChannelWriter(myTask.Stream);

                writer.WriteSingleSamplePort(true, data_out);

                Dispose();
                return 0;
            }


            private void WaitUntilDone(int timeout)
            {
                if (myTask != null)
                    myTask.WaitUntilDone(timeout);
            }

            private void Dispose()
            {
                if (myTask != null)
                    myTask.Dispose();
            }

        }


        public class DigitalOutputSignal
        {
            private String[] DO_Port;
            private String board;

            private Task myTask;
            //private Task OnOfTaskAll;
            private DigitalWaveform[] waveform_output;
            private DigitalMultiChannelWriter writer;
            private bool portActive = false;

            public DigitalOutputSignal(String[] DigitalPortName)
            {
                if (DLLactive)
                {
                    DO_Port = (String[])DigitalPortName.Clone();

                    board = DO_Port[0].Split('/')[0];
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data_out">output data. need to be the same number with DO_Port</param>
            /// <returns>int error_code. 0 = sucess, 1 = failure</returns>
            public int PutSingleValue(bool[] data_out)
            {
                if (data_out.Length != DO_Port.Length)
                {
                    return -1;
                }

                Dispose();

                myTask = new Task();
                for (int j = 0; j < DO_Port.Length; j++)
                {
                    myTask.DOChannels.CreateChannel(DO_Port[j], "", ChannelLineGrouping.OneChannelForEachLine);
                }

                writer = new DigitalMultiChannelWriter(myTask.Stream);

                bool[,] data1 = new bool[DO_Port.Length, 1];

                for (int i = 0; i < data_out.GetLength(0); i++)
                    data1[i, 0] = data_out[i];

                writer.WriteSingleSampleMultiLine(true, data1);

                myTask.WaitUntilDone();
                myTask.Dispose();

                return 0;
            }

            public void MakeLineTriggeredClock(int nSamples_perCycle, double inputRate, int nCycles, String lineClockInput)
            {
                myTask = new Task();
                if (nSamples_perCycle < 8)
                    nSamples_perCycle = 8;

                //nSample1 : nSample * nCycle.
                //nSample = nSample per cycle.
                int nSamples1 = nSamples_perCycle * nCycles;
                double outputRate = inputRate * nSamples1;
                int nSamples2 = nSamples1 - 2;

                String triggerP = "/" + board + "/" + lineClockInput;
                for (int i = 0; i < DO_Port.Length; i++)
                {
                    myTask.DOChannels.CreateChannel(DO_Port[i], "", ChannelLineGrouping.OneChannelForEachLine);
                }

                myTask.Timing.ConfigureSampleClock("", outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, nSamples2);
                myTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerP, DigitalEdgeStartTriggerEdge.Rising);
                myTask.Triggers.StartTrigger.Retriggerable = true;

                myTask.Control(TaskAction.Verify);
                var waveform_output = new DigitalWaveform[2];
                for (int i = 0; i < DO_Port.Length; i++)
                    waveform_output[i] = new DigitalWaveform(nSamples2, 1);

                //output usual clock 
                for (int i = 0; i < nSamples1 / 2; i++)
                {
                    waveform_output[0].Signals[0].States[i] = DigitalState.ForceUp;
                }

                //output divided clock
                for (int j = 0; j < nCycles; j++)
                    for (int i = 0; i < nSamples_perCycle / 2; i++)
                    {
                        int c = j * nSamples_perCycle + i;
                        if (c < nSamples2)
                            waveform_output[1].Signals[0].States[c] = DigitalState.ForceUp;
                    }

                var writer1 = new DigitalMultiChannelWriter(myTask.Stream);
                writer1.WriteWaveform(false, waveform_output);
            }

            public int PutValue(bool[][] data, double outputRate, String sampleClockPort, String triggerPort, bool ContinuousSamples)
            {
                int nSamples = data[0].Length;
                int nPort = data.Length;

                if (nPort != DO_Port.Length)
                {
                    myTask = null;
                    return -1;
                }

                waveform_output = boolToWaveform(data);

                String sampleClockP = "";
                String triggerP = "";

                if (sampleClockPort != "")
                    sampleClockP = "/" + board + "/" + sampleClockPort;

                if (triggerPort != "")
                    triggerP = "/" + board + "/" + triggerPort;

                myTask = new Task();

                for (int j = 0; j < DO_Port.Length; j++)
                {
                    myTask.DOChannels.CreateChannel(DO_Port[j], "", ChannelLineGrouping.OneChannelForEachLine);
                }

                if (!ContinuousSamples)
                    myTask.Timing.ConfigureSampleClock(sampleClockP, outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, nSamples);
                else
                    myTask.Timing.ConfigureSampleClock(sampleClockP, outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, nSamples);

                if (triggerP != "")
                    myTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerP, DigitalEdgeStartTriggerEdge.Rising);

                myTask.Control(TaskAction.Verify);

                writer = new DigitalMultiChannelWriter(myTask.Stream);
                writer.WriteWaveform(false, waveform_output);

                return 0;
            }

            public void Start()
            {
                myTask.Start();
                portActive = true;
            }


            public void Stop()
            {
                if (myTask != null && portActive)
                {
                    try
                    {
                        myTask.Stop();
                        myTask.WaitUntilDone();
                    }
                    catch (DaqException dex)
                    {
                        Debug.WriteLine("Error in Stopping DigitalOutput! {0}", dex.Message);
                    }
                    catch (Exception ex)
                    {

                    }
                    portActive = false;
                }
            }

            public void WaitUntilDone(int timeout)
            {
                if (myTask != null)
                    myTask.WaitUntilDone(timeout);

                portActive = false;
            }

            public void Dispose()
            {
                if (myTask != null)
                    myTask.Dispose();
            }
        }

        public class DigitalIn_SingleValue
        {
            public Task DI;
            public DigitalSingleChannelReader reader;
            public String port;

            public DigitalIn_SingleValue(String port_in)
            {
                port = port_in;
            }

            public bool readDI()
            {
                DI = new Task();
                DI.DIChannels.CreateChannel(port, "DI", ChannelLineGrouping.OneChannelForEachLine);
                reader = new DigitalSingleChannelReader(DI.Stream);
                var value = reader.ReadSingleSampleSingleLine();
                value = reader.ReadSingleSampleSingleLine();
                DI.Dispose();
                return value;
            }

        }

        public class DigitalOut_SingleValue
        {
            Task DO;
            public DigitalSingleChannelWriter writer;
            String port;

            public DigitalOut_SingleValue(String port_in, bool signal)
            {
                if (DLLactive)
                {
                    port = port_in;
                    DO = new Task();
                    Write(signal);
                    DO.Dispose();
                }
            }

            private void Write(bool signal)
            {
                DO.DOChannels.CreateChannel(port, "DIO", ChannelLineGrouping.OneChannelForEachLine);
                writer = new DigitalSingleChannelWriter(DO.Stream);
                try
                {
                    writer.WriteSingleSampleSingleLine(true, signal);
                }
                catch (DaqException dex)
                {
                    Debug.WriteLine("Problem in writing DigitalOutput_SingleValue" + dex.Message);
                }
            }

            private void Dispose()
            {
                try
                {
                    DO.Dispose();
                }
                catch (DaqException dex)
                {
                    Debug.WriteLine("Problem in disposing DigitalOutput_SingleValue" + dex.Message);
                }
            }

        }

        public static DigitalWaveform[] boolToWaveform(bool[][] data)
        {
            int nSamples = data[0].Length;
            int nPort = data.Length;
            DigitalWaveform[] waveform_output = new DigitalWaveform[nPort];
            for (int i = 0; i < nPort; i++)
            {
                waveform_output[i] = new DigitalWaveform(nSamples, 1);
                for (int j = 0; j < nSamples; j++)
                    waveform_output[i].Signals[0].States[j] = data[i][j] ? DigitalState.ForceUp : DigitalState.ForceDown;
            }

            return waveform_output;
        }


        public class AnalogOutput
        {
            public int minimumSamples = 8000;
            public Task[] aoTasks;
            public AOChannel[][] ao_channels;
            public AnalogMultiChannelWriter[] writerAOs;
            public String[] AO_Ports;
            public double[] maxV;
            public double[] minV;
            public bool running;
            public bool[] portActive;

            public bool SlaveMode = false;
            public bool ExportClock = false;

            bool Continuous = false;
            int[] factors;

            bool anyOldCard = false;
            double maxOutputRate = double.MaxValue;

            private int nSamples_Acq;

            public List<String> boardList;
            public String[] VirtualNames;
            public int[] boardIDs;
            public List<int> NChannelsPerBoard;
            public int nBoards = 1;
            public int nPorts = 1;
            public event EveryNSamplesEventHandler EveryNSamplesEvent;
            public delegate void EveryNSamplesEventHandler(AnalogOutput ao_task, EventArgs e);

            public AnalogOutput(String[] AnalogOutputPorts, double[] max_voltage, double[] min_voltage)
            {
                if (DLLactive)
                {
                    AO_Ports = (String[])AnalogOutputPorts.Clone();
                    maxV = (double[])max_voltage.Clone();
                    minV = (double[])min_voltage.Clone();
                    SortBoard(AO_Ports, out boardIDs, out boardList, out NChannelsPerBoard);
                    nBoards = NChannelsPerBoard.Count;
                    nPorts = AO_Ports.Length;
                    portActive = new bool[nBoards];
                    factors = Enumerable.Repeat(1, nBoards).ToArray();
                    ao_channels = new AOChannel[nBoards][];
                    for (int i = 0; i < nBoards; i++)
                        ao_channels[i] = new AOChannel[NChannelsPerBoard[i]];

                    aoTasks = MakeTask();
                    CheckBoardInfo();
                    Dispose();
                }
            }

            private void CheckBoardInfo()
            {
                for (int i = 0; i < aoTasks.Length; i++)
                {
                    var devName = aoTasks[i].Devices[0];
                    var dev = DaqSystem.Local.LoadDevice(aoTasks[i].Devices[0]);
                    var rate = dev.AOMaximumRate;
                    if (maxOutputRate < rate)
                        maxOutputRate = rate;
#if DEBUG
                    Debug.WriteLine("Device name = " + dev.ProductType);
#endif
                    if (dev.BusType == DeviceBusType.Pci)
                    {
                        anyOldCard = true;
#if DEBUG
                        Debug.WriteLine("PCI card is detected. It requires 8000 > Samples to correctly trigger output");
#endif
                        break;
                    }
                }
            }

            private Task[] MakeTask()
            {
                var ao_tasks = new Task[NChannelsPerBoard.Count];
                for (int i = 0; i < NChannelsPerBoard.Count; i++)
                {
                    Task ao_Task = null;
                    ao_Task = new Task();

                    int num = 0;
                    for (int j = 0; j < AO_Ports.Length; j++)
                    {
                        if (i == boardIDs[j])
                        {
                            ao_channels[i][num] = ao_Task.AOChannels.CreateVoltageChannel(AO_Ports[j], "", minV[j], maxV[j], AOVoltageUnits.Volts);
                            num++;
                        }
                    }

                    ao_Task.Control(TaskAction.Verify);

                    ao_tasks[i] = ao_Task;
                }

                return ao_tasks;
            }

            private double[][,] SortData(double[,] data)
            {
                var nSamples = data.GetLength(1);
                var sortedData = new double[nBoards][,];

                for (int i = 0; i < nBoards; i++)
                {
                    var data1 = new double[NChannelsPerBoard[i], nSamples];
                    int num = 0;
                    for (int j = 0; j < nPorts; j++)
                    {
                        if (i == boardIDs[j])
                        {
                            Buffer.BlockCopy(data, nSamples * j * sizeof(double), data1, nSamples * num * sizeof(double),
                                nSamples * sizeof(double));
                            num++;
                        }
                    }
                    sortedData[i] = data1;
                }
                return sortedData;
            }

            private double[][] SortData(double[] data)
            {
                var nSamples = data.Length;

                var sortedData = new double[nBoards][];
                for (int i = 0; i < nBoards; i++)
                {
                    var data1 = new double[NChannelsPerBoard[i]];
                    int num = 0;
                    for (int j = 0; j < nPorts; j++)
                    {
                        if (i == boardIDs[j])
                        {
                            data1[num] = data[j];
                            num++;
                        }
                    }
                    sortedData[i] = data1;
                }
                return sortedData;
            }

            public double[,] MultiplySize(double[,] data, int factor)
            {
                double[,] new_data = new double[data.GetLength(0), data.GetLength(1) * factor];
                for (int i = 0; i < data.GetLength(0); i++)
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        var tmp = data[i, j];
                        for (int k = 0; k < factor; k++)
                            new_data[i, factor * j + k] = tmp;
                    }
                return new_data;
            }

            public double[,] ShapeData(double[,] data, double outputRate, out double new_outputRate, out int factor)
            {
                double[,] new_data;
                new_outputRate = outputRate;
                factor = 1;
                var nSamples = data.GetLength(1);
                if (nSamples < minimumSamples)
                {
                    factor = (minimumSamples + nSamples - 1) / nSamples;
                    new_data = MultiplySize(data, factor);
                    new_outputRate = factor * outputRate;
                }
                else
                    new_data = (double[,])data.Clone();
                return new_data;
            }

            /// <summary>
            /// Return error -1 if outputRate is too high or #sample is too small. -2 if there is no device.
            /// </summary>
            /// <param name="data"></param>
            /// <param name="outputRate">Board output rate. It will be changed if #sample is too small. For PCI card, #sample needs to be > 8000</param>
            /// <param name="sampleClockPort">Port name without device name. Only if you want to synchronise. Otherwise put "".</param>
            /// <param name="triggerPort">Port name without device name. For example, "PF6"</param>
            /// <param name="reTriggerPort">Port name without device name. For example, "PF6"</param>
            /// <param name="continuousSamples">If you want to loop, put true</param>
            /// <returns></returns>
            public int PutValue_RetriggerByLineClock(double[,] data, double outputRate, double delay_us, string sampleClockPort, String reTriggerPort, bool continuousSamples, bool slaveMode)
            {
                Dispose();
                running = true;
                aoTasks = MakeTask();
                Continuous = continuousSamples;
                SlaveMode = slaveMode;

                bool syncNecessary = sampleClockPort != "" && aoTasks.Length > 1;
                ExportClock = (ExportClock || syncNecessary) && !slaveMode;

                var sortedData = SortData(data);
                writerAOs = new AnalogMultiChannelWriter[nBoards];

                if (aoTasks != null)
                {
                    for (int i = 0; i < aoTasks.Length; i++)
                    {
                        var aoTask = aoTasks[i];

                        var data1 = sortedData[i];
                        double[,] data_ch;
                        var outputRate_ch = outputRate;

                        if (anyOldCard)
                            data_ch = ShapeData(data1, outputRate, out outputRate_ch, out factors[i]);
                        else
                            data_ch = (double[,])data1.Clone();

                        if (outputRate > maxOutputRate)
                            return -1;

                        var nSamples_ch = data_ch.GetLength(1);

                        String sampleClockP = "";
                        if (sampleClockPort != "")
                            sampleClockP = "/" + boardList[i] + "/" + sampleClockPort;

                        bool runOnOwnClock = ((i == 0) || !syncNecessary) && !SlaveMode;

                        if (i == 0 && ExportClock)
                        {
                            aoTask.ExportSignals.ExportHardwareSignal(ExportSignal.SampleClock, sampleClockP);
                            Debug.WriteLine("Sample clock was exported: " + sampleClockP);
                        }

                        if (runOnOwnClock)
                            sampleClockP = "";

                        //if (Continuous)
                        //    aoTask.Timing.ConfigureSampleClock(sampleClockP, outputRate_ch, SampleClockActiveEdge.Rising,
                        //        SampleQuantityMode.ContinuousSamples, nSamples_ch);
                        //else
                        aoTask.Timing.ConfigureSampleClock(sampleClockP, outputRate_ch, SampleClockActiveEdge.Rising,
                        SampleQuantityMode.FiniteSamples, nSamples_ch);

                        if (delay_us > 0)
                        {
                            aoTask.Triggers.StartTrigger.DelayUnits = StartTriggerDelayUnits.Ticks;
                            aoTask.Triggers.StartTrigger.Delay = (int)(delay_us * 100); //1 us = 100 ticks.
                        }
                        aoTask.Triggers.StartTrigger.Retriggerable = true;

                        if (runOnOwnClock)
                        {
                            String triggerP = "/" + boardList[i] + "/" + reTriggerPort;
                            aoTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerP, DigitalEdgeStartTriggerEdge.Rising);

                        }
                        else
                        {
                            aoTask.Triggers.StartTrigger.ConfigureNone();
                        }

                        aoTask.Control(TaskAction.Verify);

                        nSamples_Acq = nSamples_ch;
                        writerAOs[i] = new AnalogMultiChannelWriter(aoTask.Stream);
                        portActive[i] = false;
                        writerAOs[i].WriteMultiSample(false, data_ch);
                    }

                    return 0;
                }

                return -2;
            }


            /// <summary>
            /// Return error -1 if outputRate is too high or #sample is too small. -2 if there is no device.
            /// </summary>
            /// <param name="data"></param>
            /// <param name="outputRate">Board output rate. It will be changed if #sample is too small. For PCI card, #sample needs to be > 8000</param>
            /// <param name="sampleClockPort">Port name without device name. Only if you want to synchronise. Otherwise put "".</param>
            /// <param name="triggerPort">Port name without device name. For example, "PF6"</param>
            /// <param name="continuousSamples">If you want to loop, put true</param>
            /// <returns></returns>
            public int PutValue(double[,] data, double outputRate, string sampleClockPort, String triggerPort, bool continuousSamples, bool slaveMode)
            {
                Dispose();
                running = true;
                aoTasks = MakeTask();
                Continuous = continuousSamples;
                SlaveMode = slaveMode;

                bool syncNecessary = sampleClockPort != "" && aoTasks.Length > 1;
                ExportClock = (ExportClock || syncNecessary) && !slaveMode;

                var sortedData = SortData(data);
                writerAOs = new AnalogMultiChannelWriter[nBoards];


                if (aoTasks != null)
                {
                    for (int i = 0; i < aoTasks.Length; i++)
                    {
                        var aoTask = aoTasks[i];

                        var data1 = sortedData[i];
                        double[,] data_ch;
                        var outputRate_ch = outputRate;

                        if (anyOldCard)
                            data_ch = ShapeData(data1, outputRate, out outputRate_ch, out factors[i]);
                        else
                            data_ch = (double[,])data1.Clone();

                        if (outputRate > maxOutputRate)
                            return -1;

                        var nSamples_ch = data_ch.GetLength(1);

                        String sampleClockP = "";
                        if (sampleClockPort != "")
                            sampleClockP = "/" + boardList[i] + "/" + sampleClockPort;

                        bool runOnOwnClock = ((i == 0) || !syncNecessary) && !SlaveMode;

                        if (i == 0 && ExportClock)
                        {
                            aoTask.ExportSignals.ExportHardwareSignal(ExportSignal.SampleClock, sampleClockP);
                            Debug.WriteLine("Sample clock was exported: " + sampleClockP);
                        }

                        if (runOnOwnClock)
                            sampleClockP = "";

                        if (Continuous)
                            aoTask.Timing.ConfigureSampleClock(sampleClockP, outputRate_ch, SampleClockActiveEdge.Rising,
                                SampleQuantityMode.ContinuousSamples, nSamples_ch);
                        else
                            aoTask.Timing.ConfigureSampleClock(sampleClockP, outputRate_ch, SampleClockActiveEdge.Rising,
                                SampleQuantityMode.FiniteSamples, nSamples_ch);

                        if (runOnOwnClock && triggerPort != "")
                        {
                            String triggerP = "/" + boardList[i] + "/" + triggerPort;
                            aoTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerP, DigitalEdgeStartTriggerEdge.Rising);
#if DEBUG
                            if (!Continuous)
                            {
                                aoTask.Done += new TaskDoneEventHandler(TaskDoneEvent);
                            }
#endif
                        }
                        else
                        {
                            aoTask.Triggers.StartTrigger.ConfigureNone();
#if DEBUG
                            if (!Continuous)
                            {
                                aoTask.Done += new TaskDoneEventHandler(TaskDoneEvent);
                                //aoTask.EveryNSamplesWrittenEventInterval = nSamples_ch;
                                //aoTask.EveryNSamplesWritten += new EveryNSamplesWrittenEventHandler(TaskDoneEvent_EveryN);
                            }
#endif
                        }

                        aoTask.Control(TaskAction.Verify);

                        nSamples_Acq = nSamples_ch;
                        writerAOs[i] = new AnalogMultiChannelWriter(aoTask.Stream);
                        portActive[i] = false;
                        writerAOs[i].WriteMultiSample(false, data_ch);
                    }

                    return 0;
                }

                return -2;
            }

            /// <summary>
            /// Need to be called after putvalue.
            /// </summary>
            public void SetReturnFunction(int EveryNSamples)
            {
                if (aoTasks != null)
                {
                    aoTasks[0].EveryNSamplesWrittenEventInterval = EveryNSamples * factors[0];
                    aoTasks[0].EveryNSamplesWritten += new EveryNSamplesWrittenEventHandler(EveryNSampleEventFcn);
                }
            }

            void TaskDoneEvent(object sender, TaskDoneEventArgs e)
            {
#if DEBUG
                var aoTask = (Task)sender;
                if (aoTask != null)
                    Debug.WriteLine("Sample writing done: " + aoTask.Devices[0] +
                        ", " + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff"));
#endif
            }

            void TaskDoneEvent_EveryN(object sender, EveryNSamplesWrittenEventArgs e)
            {
#if DEBUG

                Debug.WriteLine("Time: " + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff"));
                for (int i = 0; i < aoTasks.Length; i++)
                {
                    var aoTask = aoTasks[i];
                    Debug.WriteLine("Sample written (Every N Events): " + aoTask.Devices[0]);
                    Debug.WriteLine("Total sample generated = " + aoTask.Stream.TotalSamplesGeneratedPerChannel);
                }
#endif
            }

            void EveryNSampleEventFcn(object sender, EveryNSamplesWrittenEventArgs e)
            {
                EveryNSamplesEvent?.Invoke(this, null);
            }

            public void PutValue_SingleValue(double[] values)
            {
                if (running)
                    return;

                Stop();
                Dispose();

                Task[] aoTask_S_array = MakeTask();
                double[][] sortedData = SortData(values);
                for (int i = 0; i < aoTask_S_array.Length; i++)
                {
                    Task aoTask_S = aoTask_S_array[i];
                    aoTask_S.Control(TaskAction.Verify);
                    var writerAO_S = new AnalogMultiChannelWriter(aoTask_S.Stream);

#if DEBUG
                    writerAO_S.WriteSingleSample(true, sortedData[i]);
                    aoTask_S.WaitUntilDone();
#else

                    try
                    {
                        writerAO_S.WriteSingleSample(true, sortedData[i]);
                        aoTask_S.WaitUntilDone();
                    }
                    catch (DaqException ex)
                    {
                        Debug.WriteLine("Problem in AO timeout" + ex.Message);
                    }
#endif
                    aoTask_S.Dispose();
                }
            }

            public bool WaitUntilDone(int timeout)
            {

                if (aoTasks != null)
                {
                    var sw = new Stopwatch();
                    sw.Start();

                    bool[] success = new bool[aoTasks.Length];
                    bool firstOne = true;
                    for (int i = 0; i < aoTasks.Length; i++)
                    {
                        var aoTask = aoTasks[i];
                        long totalSamplesGenerated = -1;

                        try
                        {
                            totalSamplesGenerated = aoTask.Stream.TotalSamplesGeneratedPerChannel;
                            Debug.WriteLine("Total sample written = " + totalSamplesGenerated);
                        }
                        catch (DaqException ex)
                        {
                            Debug.WriteLine("Problem in Stream.TotlSampleGeratePerChannel" + ex.Message);
                            break; //Something wrong. Just exit.
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Problem in Stream.TotlSampleGeratePerChannel" + ex.Message);
                            break; //Something wrong. Just exit.s
                        }

                        if (aoTask != null && firstOne)
                        {
                            if (totalSamplesGenerated >= 0)
                            {
                                try
                                {
                                    aoTask.WaitUntilDone(timeout);
                                    success[i] = true;
                                }
                                catch (DaqException ex)
                                {
                                    Debug.WriteLine("DAQ error: AO timeout" + ex.Message);
                                    aoTask.Stop();
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("DAQ error: Problem in AO waitUntilDone: " + ex.Message);
                                }

                                firstOne = false;
                            }
                        }
                        else if (aoTask != null)
                        {
                            while (true)
                            {
                                if (totalSamplesGenerated < 0)
                                    break;

                                try
                                {
                                    if (totalSamplesGenerated >= nSamples_Acq - 1) //For some PCI board, only nSamples_Acq-1 is acquired.
                                    {
                                        aoTask.Stop();
                                        success[i] = true;
                                        break;
                                    }
                                }
                                catch (DaqException ex)
                                {
                                    Debug.WriteLine("Problem in Stopping DAQ" + ex.Message);
                                    break;
                                }

                                System.Threading.Thread.Sleep(5);
                                if (sw.ElapsedMilliseconds > timeout)
                                {
                                    success[i] = false;
                                    break;
                                }
                            } //Loop forever.
                        }

                        portActive[i] = false;
                    }
                    sw.Stop();
                    return success.All(x => x == true);
                }
                running = false;
                return false;
            }

            public void Start()
            {
                if (aoTasks != null)
                {
                    //Start all boards.
                    for (int i = 0; i < aoTasks.Length; i++)
                    {
                        var aoTask = aoTasks[i];
                        if (!portActive[i])
                        {
                            aoTask.Start();
                            portActive[i] = true;
                        }
                    }
                }
            }

            public void CheckCurrentStatus(int boardID)
            {
#if DEBUG
                try
                {
                    if (boardID < nBoards)
                        Debug.WriteLine("Checking status: Samples generated = " + aoTasks[boardID].Stream.TotalSamplesGeneratedPerChannel);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error, perhaps board is not started: " + aoTasks[boardID].Devices[0]);
                }
#endif
            }


            public void Stop()
            {
                if (aoTasks != null && running)
                {
                    for (int i = 0; i < aoTasks.Length; i++)
                    {
                        var aoTask = aoTasks[i];
                        if (aoTask != null)
                        {
                            try
                            {
                                aoTask.Stop();
                            }
                            catch (DaqException dex)
                            {
                                Debug.WriteLine("DAQ error in stopping AOTask, device {0}: {1}", i, dex.Message);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Non-DAQ error in stopping AOTask, device {0}: {1}", i, ex.Message);
                            }
                        }
                    }
                    WaitUntilDone(1000);
                }

                running = false;
            }

            public void Dispose()
            {
                if (aoTasks != null)
                    foreach (var aoTask in aoTasks)
                    {
                        if (aoTask != null)
                        {
                            aoTask.Dispose();
                        }
                    }
                running = false;
            }

        }

        public class AnalogInput
        {
            public AsyncCallback analogCallback;
            public Task[] aiTasks;
            public AnalogMultiChannelReader[] readers;
            public AIChannel[][] ai_channels;

            public int nSamples;
            public String[] AI_Ports;
            public int[] boardIDs;
            public List<String> boardList;
            public List<int> NChannelsPerBoard;
            public double[] maxV;
            public double[] minV;
            public bool portActive = false;
            public int nBoards;
            public int nPorts;
            public bool ExportClock = false;

            public event EveryNSamplesEventHandler EveryNSamplesEvent;
            public delegate void EveryNSamplesEventHandler(AnalogInput ai_task, EventArgs e);

            public AnalogInput(String[] AnalogInputPorts, double[] max_voltage, double[] min_voltage)
            {
                if (DLLactive)
                {
                    AI_Ports = (String[])AnalogInputPorts.Clone();
                    maxV = (double[])max_voltage.Clone();
                    minV = (double[])min_voltage.Clone();
                    SortBoard(AI_Ports, out boardIDs, out boardList, out NChannelsPerBoard);
                    nBoards = NChannelsPerBoard.Count;
                    nPorts = AI_Ports.Length;
                    readers = new AnalogMultiChannelReader[nBoards];
                    ai_channels = new AIChannel[nBoards][];
                    for (int i = 0; i < nBoards; i++)
                        ai_channels[i] = new AIChannel[NChannelsPerBoard[i]];
                }
            }

            private Task[] MakeTask()
            {
                var ai_tasks = new Task[NChannelsPerBoard.Count];
                for (int i = 0; i < NChannelsPerBoard.Count; i++)
                {
                    Task ai_Task = new Task();

                    int num = 0;
                    for (int j = 0; j < AI_Ports.Length; j++)
                    {
                        if (i == boardIDs[j])
                        {
                            ai_channels[i][num] = ai_Task.AIChannels.CreateVoltageChannel(AI_Ports[j], "", (AITerminalConfiguration)(-1),
                                minV[j], maxV[j], AIVoltageUnits.Volts);
                            num++;
                        }
                    }

                    ai_Task.Control(TaskAction.Verify);

                    ai_tasks[i] = ai_Task;
                }

                return ai_tasks;
            }

            void EveryNSampleEventFcn(object sender, EveryNSamplesReadEventArgs e)
            {
                EveryNSamplesEvent?.Invoke(this, null);
            }

            public void SetupAI(int SamplesPerTrigger, double inputRate, String sampleClockPort, String triggerPort, bool ContinuousSamples)
            {
                Dispose();
                aiTasks = MakeTask();
                nSamples = SamplesPerTrigger;

                if (aiTasks != null)
                {
                    for (int i = 0; i < aiTasks.Length; i++)
                    {
                        var aiTask = aiTasks[i];
                        String sampleClockP = "/" + boardList[i] + "/" + sampleClockPort;
                        String triggerP = "/" + boardList[i] + "/" + triggerPort;

                        if (i == 0 && ExportClock)
                        {
                            aiTask.ExportSignals.ExportHardwareSignal(ExportSignal.SampleClock, sampleClockP);
                            Debug.WriteLine("Sample clock was exported: " + sampleClockP);
                        }

                        if (i == 0 || sampleClockPort == "")
                            sampleClockP = "";

                        if (ContinuousSamples)
                            aiTask.Timing.ConfigureSampleClock(sampleClockP, inputRate, SampleClockActiveEdge.Rising,
                                SampleQuantityMode.ContinuousSamples, nSamples);
                        else
                            aiTask.Timing.ConfigureSampleClock(sampleClockP, inputRate, SampleClockActiveEdge.Rising,
                                SampleQuantityMode.FiniteSamples, nSamples);

                        aiTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger("/" + boardList[i] + "/" + triggerPort, DigitalEdgeStartTriggerEdge.Rising);

                        if (i == 0 && !ContinuousSamples)
                        {
#if DEBUG
                            aiTask.Done += new TaskDoneEventHandler(TaskDoneEvent);
                            //aiTask.EveryNSamplesReadEventInterval = nSamples;
                            //aiTask.EveryNSamplesRead += new EveryNSamplesReadEventHandler(TaskDoneEverySample);
#endif
                        }

                        aiTask.Control(TaskAction.Verify);

                        readers[i] = new AnalogMultiChannelReader(aiTask.Stream);
                    }
                }
            }

            void TaskDoneEverySample(object sender, EveryNSamplesReadEventArgs e)
            {
                var aiTask = (Task)sender;
#if DEBUG
                Debug.WriteLine("Sample acquisition (Every " + aiTask.EveryNSamplesReadEventInterval
                    + " Event) " + aiTask.Devices[0] + ", " + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff"));
                Debug.WriteLine("Acquired sample = " + aiTask.Stream.TotalSamplesAcquiredPerChannel);
#endif
            }

            void TaskDoneEvent(object sender, TaskDoneEventArgs e)
            {
                var aiTask = (Task)sender;
#if DEBUG
                Debug.WriteLine("Sample acquisition done: " + aiTask.Devices[0] +
                    ", " + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff"));
#endif
            }

            /// <summary>
            /// Need to be called after SetupAI.
            /// </summary>
            public void SetReturnFunction(int EveryNSamples)
            {
                if (aiTasks != null)
                {
                    aiTasks[0].EveryNSamplesReadEventInterval = EveryNSamples;
                    aiTasks[0].EveryNSamplesRead += new EveryNSamplesReadEventHandler(EveryNSampleEventFcn);
                }
            }

            public void Start(String triggerPort) //does not work.
            {
                portActive = true;
                for (int i = 0; i < aiTasks.Length; i++)
                {
                    aiTasks[i].Start();
                }
            }

            public double[] GetSingleValue()
            {
                if (portActive)
                    return null;

                Dispose();
                aiTasks = MakeTask();
                double[] data = new double[AI_Ports.Length];
                int num = 0;
                if (aiTasks != null)
                {
                    for (int i = 0; i < aiTasks.Length; i++)
                    {
                        var aiTask = aiTasks[i];
                        AnalogMultiChannelReader reader = new AnalogMultiChannelReader(aiTask.Stream);
                        double[] data1 = new double[NChannelsPerBoard[i]];
                        aiTask.Control(TaskAction.Verify);
#if DEBUG
                        data1 = reader.ReadSingleSample();
#else
                        try
                        {
                            data1 = reader.ReadSingleSample();
                        }
                        catch (DaqException ex)
                        {
                            Debug.WriteLine("Failed:" + ex.Message);
                        }
#endif
                        Array.Copy(data1, 0, data, num, data1.Length);
                        num += data1.Length;
                    }
                }
                return data;
            }

            public void Stop()
            {
                if (aiTasks != null && portActive)
                    for (int i = 0; i < aiTasks.Length; i++)
                    {
                        if (aiTasks[i] != null)
                            aiTasks[i].Stop();
                    }
                WaitUntilDone(100);
                portActive = false;
            }

            public bool WaitUntilDone()
            {
                return WaitUntilDone(int.MaxValue);
            }

            public bool WaitUntilDone(int timeoutEachChannel)
            {
                bool[] success = new bool[aiTasks.Length];
                if (aiTasks != null)
                    for (int i = 0; i < aiTasks.Length; i++)
                    {
                        var aiTask = aiTasks[i];
                        try
                        {
                            if (aiTask != null)
                                aiTask.WaitUntilDone(timeoutEachChannel);
                            success[i] = true;
                        }
                        catch (DaqException ex)
                        {
                            Debug.WriteLine("Problem in AI timeout" + ex.Message);
                            Stop();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Problem in AI waitUntilDone: " + ex.Message);
                        }
                    }

                portActive = false;
                return success.All(x => x == true);
            }

            public void Dispose()
            {
                if (aiTasks != null)
                    for (int i = 0; i < aiTasks.Length; i++)
                    {
                        if (aiTasks[i] != null)
                            aiTasks[i].Dispose();
                    }
                portActive = false;
            }

            public double[,] ReadSample()
            {
                return ReadSample((int)aiTasks[0].Stream.AvailableSamplesPerChannel);
            }

            public double[,] ReadSample(int n_samples)
            {
                int n_board = aiTasks.Length;
                int n_channels = AI_Ports.Length;
                double[,] data = new double[n_channels, n_samples];
                int num = 0;
                if (aiTasks != null)
                {
                    int min_samples = int.MaxValue;
                    for (int i = 0; i < n_board; i++)
                    {
                        var n_samp = (int)aiTasks[i].Stream.AvailableSamplesPerChannel;
                        if (min_samples < n_samp)
                            min_samples = n_samp;
                    }

                    if (n_samples > min_samples)
                        n_samples = min_samples;

                    for (int i = 0; i < n_board; i++)
                    {
                        var data1 = new double[NChannelsPerBoard[i], n_samples];
                        var aiTask = aiTasks[i];
                        data1 = readers[i].ReadMultiSample((int)n_samples);
                        Buffer.BlockCopy(data1, 0, data, num * n_samples * sizeof(double), data1.Length * sizeof(double));
                        num += NChannelsPerBoard[i];
                    }
                }

                return data;
            }
        }

        static private void SortBoard(String[] Ports, out int[] boardIDs, out List<string> boardList, out List<int> NChannelsPerBoard)
        {
            boardIDs = new int[Ports.Length];
            boardList = new List<string>();
            NChannelsPerBoard = new List<int>();
            for (int i = 0; i < Ports.Length; i++)
            {
                var sP = Ports[i].Split('/');
                var board = sP[0];
                if (NChannelsPerBoard.Count == 0)
                {
                    boardList.Add(board);
                    NChannelsPerBoard.Add(1);
                    boardIDs[i] = 0;
                }
                else
                {
                    bool found = false;
                    for (int j = 0; j < NChannelsPerBoard.Count; j++)
                    {
                        if (board == boardList[j])
                        {
                            found = true;
                            boardIDs[i] = j;
                            NChannelsPerBoard[j]++;
                            break;
                        }
                    }
                    if (!found)
                    {
                        boardList.Add(board);
                        NChannelsPerBoard.Add(1);
                        boardIDs[i] = boardList.Count - 1;
                    }
                }
            }
        }
    }
}
