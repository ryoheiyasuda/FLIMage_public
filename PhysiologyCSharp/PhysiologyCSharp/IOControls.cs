using NationalInstruments;
using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace PhysiologyCSharp
{
    public class IOControls
    {
        static public string board = "Dev3";
        static public string TriggerEvokePort = "line0";
        static public string TriggerReceviePort = "PFI6";
        static public string ExtTriggerReceviePort = "PFI2";
        static public string outputPortPatch1 = "AO0";
        static public string outputPortPatch2 = "AO2";
        static public string outputPortStim1 = "AO1";
        static public string outputPortStim2 = "AO3";
        static public string inputPortPatch1 = "AI0";
        static public string inputPortPatch2 = "AI1";
        static public int[] outputRange = { -5, 5 };
        static public int[] inputRange = { -5, 5 };
        static public int nPatchChannels = 2;
        static public int nStimChannels = 2;

        public DateTime triggerTime;
        public string initFileName = "device_setting.txt";
        public string commanderPath;
        DioTrigger dioTrigger;
        PhysAI phys_AI;
        PhysAO phys_AO;
        public MC700CommanderCore mc700_commander;

        public double[,] returnData;
        public double[][] dataOutput;

        public bool scope;
        public event AcqDoneHandler AcqDone;
        public EventArgs e = null;
        public delegate void AcqDoneHandler(IOControls io_control, EventArgs e);

        public event DataOutDoneHandler DataOutDone;
        public delegate void DataOutDoneHandler(IOControls io_control, EventArgs e);

        public MC700_Parameters mc700_params = new MC700_Parameters();

        public class MC700_Parameters
        {
            public double[] PrimaryGain = { 1, 1 };
            public double[] CommandSensitivity = { 0.02, 0.02 };
            public int[] Mode = { 0, 0 };

            public MC700_Parameters()
            {
                PrimaryGain = Enumerable.Repeat<double>(1, nPatchChannels).ToArray();
                CommandSensitivity = Enumerable.Repeat<double>(0.02, nPatchChannels).ToArray();
                Mode = Enumerable.Repeat<int>(0, nPatchChannels).ToArray();
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                var fields = this.GetType().GetFields();
                for (int i = 0; i < fields.Length; i++)
                {
                    sb.Append(fields[i].Name + "=");
                    if (fields[i].FieldType == typeof(double[]))
                    {
                        var data_shaped = (double[])fields[i].GetValue(this);
                        sb.Append(String.Join(",", data_shaped));
                    }
                    else if (fields[i].FieldType == typeof(int[]))
                    {
                        var data_shaped = (int[])fields[i].GetValue(this);
                        sb.Append(String.Join(",", data_shaped));
                    }
                    else if (fields[i].FieldType == typeof(bool[]))
                    {
                        var data_shaped = (bool[])fields[i].GetValue(this);
                        sb.Append(String.Join(",", data_shaped));
                    }
                    sb.AppendLine();
                }
                return sb.ToString();
            }

            public void FromStringToParam(string text)
            {
                string[] lines = text.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Replace("\r", "");
                    string[] sP = line.Split('=');
                    if (sP.Length >= 2 && sP[1].Contains(","))
                    {
                        var field = this.GetType().GetField(sP[0]);
                        if (field.FieldType == typeof(double[]))
                        {
                            double[] Vals = Array.ConvertAll(sP[1].Split(','), double.Parse);
                            field.SetValue(this, Vals);
                        }
                        else if (field.FieldType == typeof(int[]))
                        {
                            int[] Vals = Array.ConvertAll(sP[1].Split(','), int.Parse);
                            field.SetValue(this, Vals);
                        }
                    }
                }
            }
        }


        public IOControls(String initFoldeName, bool warning_on)
        {
            initFileName = Path.Combine(initFoldeName, initFileName);
            if (File.Exists(initFileName))
                ReadParameters(initFileName);
            else
                SaveParameters(initFileName);

            dioTrigger = new DioTrigger();
            mc700_commander = new MC700CommanderCore();
            int deviceN = RetrieveMC700Info();

            commanderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Molecular Devices", "MultiClamp 700B Commander", "MC700B.exe");
            //System.Diagnostics.Process.Start();
            if (deviceN == 0)
            {
                if (File.Exists(commanderPath))
                {
                    System.Diagnostics.Process.Start(commanderPath);
                    for (int i = 0; i < 50; i++)
                    {
                        System.Threading.Thread.Sleep(500);
                        deviceN = RetrieveMC700Info();
                        if (deviceN > 0)
                            break;
                    }
                }

                if (deviceN == 0 && warning_on)
                    MessageBox.Show("Perhaps MultiClamp commander is not open");
            }

        }

        public void Close()
        {
            mc700_commander.Close();
            if (phys_AI != null)
                phys_AI.dispose();
            if (phys_AO != null)
                phys_AO.dispose();
            if (dioTrigger != null)
                dioTrigger.dispose();
        }

        public int RetrieveMC700Info()
        {
            mc700_commander.GetMC700BGain();
            int deviceN = mc700_commander.MC700_Params.Length;
            mc700_params.PrimaryGain = new double[deviceN];
            mc700_params.Mode = new int[deviceN];
            mc700_params.CommandSensitivity = new double[deviceN];
            for (int i = 0; i < deviceN; i++)
            {
                var pm = mc700_commander.MC700_Params[i];
                mc700_params.PrimaryGain[i] = pm.primary_gain;
                mc700_params.Mode[i] = pm.mode;
            }
            return deviceN;
        }

        public double[] GetExternalCommandSensitivity()
        {
            mc700_commander.GetMC700BGain();
            int deviceN = mc700_commander.MC700_Params.Length;

            double[] results = new double[deviceN];
            for (int i = 0; i < deviceN; i++)
            {
                var pm = mc700_commander.MC700_Params[i];
                if (pm.mode == 0)
                    results[i] = pm.external_cmd_sensitivity * 1000;
                else
                    results[i] = pm.external_cmd_sensitivity * 1e12;
            }

            return results;
        }

        /// <summary>
        /// Get Gain from MC700 Commander.
        /// </summary>
        /// <returns></returns>
        public double[] GetGain()
        {
            int deviceN = RetrieveMC700Info();
            double[] results = new double[deviceN];
            for (int i = 0; i < deviceN; i++)
            {
                var pm = mc700_commander.MC700_Params[i];
                results[i] = pm.primary_gain * pm.scaleFactor / 1000; //make it mV or pA
            }
            return results;
        }


        public int ScaleDataInput(double[][] data_in, out double[,] data_out)
        {
            if (data_in == null || data_in.Length < 1)
            {
                //This should never happens, but just in case.
                MessageBox.Show("Cannot put values. data = null.");
                data_out = null;
                return -1;
            }

            var sensitivity = GetExternalCommandSensitivity();
            int nCh = data_in.Length;
            int nSamples = data_in[0].Length;

            data_out = new double[nCh, nSamples];
            for (int i = 0; i < nCh; i++)
                Buffer.BlockCopy(data_in[i], 0, data_out, nSamples * i * sizeof(double), nSamples * sizeof(double));

            if (nCh != nPatchChannels + nStimChannels || sensitivity.Length < nPatchChannels)
            {
                MessageBox.Show("Number of channels are not correctly set");
                return -2;
            }

            for (int i = 0; i < nCh; i++)
            {
                if (i < nPatchChannels)
                {
                    var sens1 = sensitivity[0];
                    if (sensitivity.Length > i)
                        sens1 = sensitivity[i];

                    for (int j = 0; j < nSamples; j++)
                    {
                        Double val = data_in[i][j] / sens1;
                        if (val > inputRange[1])
                            val = inputRange[1];
                        else if (val < inputRange[0])
                            val = inputRange[0];
                        data_out[i, j] = val;
                    }
                }
                else
                {
                    for (int j = 0; j < nSamples; j++)
                    {
                        data_out[i, j] = data_in[i][j] / 1000.0;
                    }
                }
            }

            return 0;
        }

        public double[][] ScaleDataOutput(double[,] data_in)
        {
            if (data_in == null)
                return null;

            var gain = GetGain();
            int nCh = data_in.GetLength(0);
            int nSamples = data_in.GetLength(1);

            var data_out = new double[nCh][];
            for (int i = 0; i < nCh; i++)
            {
                data_out[i] = new double[nSamples];
                Buffer.BlockCopy(data_in, nSamples * i * sizeof(double), data_out[i], 0, nSamples * sizeof(double));
            }

            if (nCh != nPatchChannels || gain.Length < nPatchChannels)
            {
                //MessageBox.Show("Data output error: Number of channels are not correctly set");
                Debug.WriteLine("Data output error: Number of channels are not correctly set");
            }

            for (int i = 0; i < nCh; i++)
            {
                var gain1 = gain[0];
                if (gain.Length > i)
                    gain1 = gain[i];

                for (int j = 0; j < nSamples; j++)
                    data_out[i][j] = data_in[i, j] / gain1;
            }

            return data_out;
        }

        /// <summary>
        /// Called when acquisition is done.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AcquiredDoneHandlerFcn(object sender, EventArgs e)
        {
            returnData = phys_AI.result;
            dataOutput = ScaleDataOutput(returnData);
            AcqDone?.Invoke(this, e);
        }

        public void DataOut_DoneHandlerFcn(object sender, EventArgs e)
        {
            DataOutDone?.Invoke(this, e);
        }

        public void PutZero()
        {
            Stop();
            var phys_AO1 = new PhysAO();
            var values = new double[phys_AO1.nTotalChannels];
            phys_AO1.PutSingleValue(values);
            phys_AO1.dispose();
        }


        public void PutSingleValue(double[] values)
        {
            var phys_AO1 = new PhysAO();
            phys_AO1.PutSingleValue(values);
            phys_AO1.dispose();
        }

        public int PutValues(double[][] data_in, double outputRate_in, bool AI_on)
        {
            if (data_in == null)
            {
                MessageBox.Show("Failed to put value. data = null. Check the number of channels etc");
                return -1;
            }

            int ret = ScaleDataInput(data_in, out double[,] data);

            if (ret < 0)
                return -2;

            if (phys_AO != null)
                phys_AO.dispose();
            if (phys_AI != null)
                phys_AI.dispose();

            phys_AO = new PhysAO();
            phys_AO.putValue(data, outputRate_in);
            phys_AO.DataOutDone += new IOControls.PhysAO.DataOutDoneHandler(DataOut_DoneHandlerFcn);

            if (AI_on)
            {
                phys_AI = new PhysAI();
                phys_AI.AcqDone += new IOControls.PhysAI.AcqDoneHandler(AcquiredDoneHandlerFcn);
                int data_length = data.GetLength(1);
                phys_AI.setupAI(data_length, outputRate_in);
            }

            return 0;
        }

        public void WaitUntilDone(int timeout)
        {
            phys_AO.WaitUntilDone(timeout);
        }

        public void Stop()
        {
            if (phys_AO != null)
            {
                phys_AO.stop();
                phys_AO.dispose();
            }

            if (phys_AI != null)
            {
                phys_AI.stop();
                phys_AI.dispose();
            }
        }

        public void Start(bool ext_trigger, bool AI_on)
        {
            phys_AO.start(ext_trigger);

            if (AI_on)
                phys_AI.start(ext_trigger);

            if (!ext_trigger)
            {
                dioTrigger.Evoke();
                triggerTime = DateTime.Now;
            }
        }

        public void SaveParameters(string filename)
        {
            StringBuilder sb = new StringBuilder();
            var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var member in fields)
            {
                sb.Append(member.Name);
                sb.Append("=");
                if (member.FieldType == typeof(int[]))
                {
                    var values = (int[])member.GetValue(this);
                    sb.Append(String.Join(",", values));
                }
                else
                {
                    var value = member.GetValue(this).ToString();
                    sb.Append(value);
                }
                sb.AppendLine();
            }
            File.WriteAllText(filename, sb.ToString());
        }

        void ReadParameters(string filename)
        {
            var linesAll = File.ReadAllText(filename);
            var lines = linesAll.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Replace("\r", "").Replace(" ", "");
                var eq = line.Split('=');
                if (eq.Length >= 2)
                {
                    var value = eq[1];
                    var name = eq[0];
                    var member = GetType().GetField(name);

                    if (member.FieldType == typeof(int[]))
                    {
                        int[] arrayI = Array.ConvertAll(value.Split(','), int.Parse);
                        member.SetValue(this, arrayI);
                    }
                    else
                        member.SetValue(this, Convert.ChangeType(value, member.FieldType));
                }
            }
        }

        public class DioTrigger
        {
            public Task dioTaskS;
            public String port;
            private DigitalSingleChannelWriter writerS;

            public DioTrigger()
            {
                port = board + "/" + "Port0/" + TriggerEvokePort;
                dioTaskS = new Task();
                dioTaskS.DOChannels.CreateChannel(port, "DIO", ChannelLineGrouping.OneChannelForEachLine);
                writerS = new DigitalSingleChannelWriter(dioTaskS.Stream);

                writerS.WriteSingleSampleSingleLine(true, false);
            }


            public void Evoke()
            {
                writerS.WriteSingleSampleSingleLine(true, false);
                writerS.WriteSingleSampleSingleLine(true, true);
                writerS.WriteSingleSampleSingleLine(true, false);
            }

            public void dispose()
            {
                dioTaskS.Dispose();
            }
        }

        public class PhysAI
        {
            public Task hPhys_AI;
            public AnalogMultiChannelReader readerPhys;
            public Task runningTask;
            public AsyncCallback analogCallback;

            public String port0;
            public String port1;

            public int samplesPerTrigger;
            public String triggerPort;
            public string ExternalTriggerPort;
            public DigitalEdgeStartTriggerEdge triggerEdge;

            public bool measurement_done = false;

            public double[,] result;
            public event AcqDoneHandler AcqDone;
            public EventArgs e = null;
            public delegate void AcqDoneHandler(PhysAI phys_AI, EventArgs e);

            public PhysAI()
            {
                //triggerPort = State.Init.EOM_AI_Trigger;
                port0 = board + "/" + inputPortPatch1;
                port1 = board + "/" + inputPortPatch2;

                double maxV = inputRange[1];
                double minV = inputRange[0];

                triggerEdge = DigitalEdgeStartTriggerEdge.Rising;
                triggerPort = "/" + board + "/" + TriggerReceviePort;
                ExternalTriggerPort = "/" + board + "/" + ExtTriggerReceviePort;


                hPhys_AI = new Task();
                hPhys_AI.AIChannels.CreateVoltageChannel(port0, "PhysAI_Ch0", (AITerminalConfiguration)(-1), minV, maxV, AIVoltageUnits.Volts);

                if (nPatchChannels > 1)
                    hPhys_AI.AIChannels.CreateVoltageChannel(port1, "PhysAI_Ch1", (AITerminalConfiguration)(-1), minV, maxV, AIVoltageUnits.Volts);

                hPhys_AI.Control(TaskAction.Verify);

                //hEOM_AI.EveryNSamplesReadEventInterval = 1;
                //hEOM_AI.EveryNSamplesRead += new EveryNSamplesReadEventHandler(SampleCompleted);
            }

            public void setupAI(int samplesPerChannel, double inputRate)
            {
                samplesPerTrigger = samplesPerChannel;
                hPhys_AI.Timing.ConfigureSampleClock("", inputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, samplesPerTrigger);
                hPhys_AI.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, triggerEdge);
                hPhys_AI.Control(TaskAction.Verify);
                hPhys_AI.EveryNSamplesReadEventInterval = samplesPerChannel;
                hPhys_AI.EveryNSamplesRead += new EveryNSamplesReadEventHandler(EveryNSampleEvent);
                result = new double[nPatchChannels, samplesPerChannel];
            }

            public void EveryNSampleEvent(object sender, EveryNSamplesReadEventArgs e)
            {
                readSample();
                AcqDone?.Invoke(this, null);
            }

            public void start(bool externalTrigger)
            {
                if (externalTrigger)
                    hPhys_AI.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(ExternalTriggerPort, triggerEdge);

                hPhys_AI.Control(TaskAction.Verify);

                readerPhys = new AnalogMultiChannelReader(hPhys_AI.Stream);
                measurement_done = false;
                hPhys_AI.Start();
            }

            public void WaitUntilDone(int timeout)
            {
                try
                {
                    hPhys_AI.WaitUntilDone(timeout);
                }
                catch (DaqException ex)
                {
                    Debug.WriteLine("TIMEOUT: hPhys output" + ex.Message);
                    stop();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TIMEOUT: hPhys mirror data" + ex.Message);
                }
            }

            public void stop()
            {
                try
                {
                    hPhys_AI.Stop();
                }
                catch
                {

                }
            }

            public void dispose()
            {
                hPhys_AI.Dispose();
            }

            public void readSample()
            {
                if (hPhys_AI.Stream.AvailableSamplesPerChannel == samplesPerTrigger)
                {
                    result = readerPhys.ReadMultiSample(samplesPerTrigger);
                }
                else
                {
                    result = readerPhys.ReadMultiSample((int)hPhys_AI.Stream.AvailableSamplesPerChannel);
                }
                measurement_done = true;
            }

            //public void AnalogInCallback(IAsyncResult ar)
            //{
            //    try
            //    {
            //        result = readerPhys.EndReadMultiSample(ar);
            //    }
            //    catch (DaqException ex)
            //    {
            //        Debug.WriteLine("Problem in AnalogInCallback by pockelAI: " + ex.Message);
            //        runningTask = null;
            //    }
            //    finally
            //    {
            //        measurement_done = true;
            //    }
            //}

        }



        public class PhysAO
        {
            public Task hPhys;
            public AnalogMultiChannelWriter writerPhys;
            public int nTotalChannels;

            public double outputRate;
            public String portPatch1, portPatch2, portStim1, portStim2;
            public String triggerPort = "";
            public String ExternalTriggerInputPort = "";

            public event DataOutDoneHandler DataOutDone;
            public EventArgs e = null;
            public delegate void DataOutDoneHandler(PhysAO phys_AO, EventArgs e);


            public PhysAO()
            {
                hPhys = new Task();

                double maxV = outputRange[1];
                double minV = outputRange[0];

                portPatch1 = board + "/" + outputPortPatch1;
                portPatch2 = board + "/" + outputPortPatch2;
                portStim1 = board + "/" + outputPortStim1;
                portStim2 = board + "/" + outputPortStim2;

                nTotalChannels = nPatchChannels + nStimChannels;

                hPhys.AOChannels.CreateVoltageChannel(portPatch1, "portPatch1", minV, maxV, AOVoltageUnits.Volts);
                if (nPatchChannels > 1)
                    hPhys.AOChannels.CreateVoltageChannel(portPatch2, "portPatch2", minV, maxV, AOVoltageUnits.Volts);

                hPhys.AOChannels.CreateVoltageChannel(portStim1, "portStim1", minV, maxV, AOVoltageUnits.Volts);
                if (nStimChannels > 1)
                    hPhys.AOChannels.CreateVoltageChannel(portStim2, "portStim2", minV, maxV, AOVoltageUnits.Volts);

                triggerPort = "/" + board + "/" + TriggerReceviePort;
                ExternalTriggerInputPort = "/" + board + "/" + ExtTriggerReceviePort;

                hPhys.Control(TaskAction.Verify);

            }

            public void EveryNSampleEvent(object sender, EveryNSamplesWrittenEventArgs e)
            {
                DataOutDone?.Invoke(this, null);
            }

            public void PutSingleValue(double[] value)
            {
                var writer = new AnalogMultiChannelWriter(hPhys.Stream);
                writer.WriteSingleSample(true, value);
            }

            //Put value for simple scanning.
            public void putValue(double[,] data, double outputRate_in)
            {
                outputRate = outputRate_in;
                int samplesPerChannel = data.GetLength(1);

                hPhys.Timing.ConfigureSampleClock("", outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, samplesPerChannel);
                hPhys.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, DigitalEdgeStartTriggerEdge.Rising);

                hPhys.EveryNSamplesWrittenEventInterval = samplesPerChannel;
                hPhys.EveryNSamplesWritten += new EveryNSamplesWrittenEventHandler(EveryNSampleEvent);

                writerPhys = new AnalogMultiChannelWriter(hPhys.Stream);
                writerPhys.WriteMultiSample(false, data);
            }

            public bool start(bool externalTrigger)
            {
                if (externalTrigger)
                    hPhys.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(ExternalTriggerInputPort, DigitalEdgeStartTriggerEdge.Rising);
                else
                    hPhys.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, DigitalEdgeStartTriggerEdge.Rising);
                try
                {
                    hPhys.Start();
                    return true;
                }
                catch (DaqException ex)
                {
                    //hPhys.Start();
                    return false;
                }
            }

            public void WaitUntilDone(int timeout)
            {
                try
                {
                    hPhys.WaitUntilDone(timeout);
                }
                catch (DaqException ex)
                {
                    Debug.WriteLine("TIMEOUT: hPhys output" + ex.Message);
                    stop();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TIMEOUT: hPhys mirror data" + ex.Message);
                }
            }

            public void stop()
            {
                try
                {
                    hPhys.Stop();
                    hPhys.WaitUntilDone();
                }
                catch { }
                finally { }
            }

            public void dispose()
            {
                if (hPhys != null)
                    hPhys.Dispose();

            }
        } //Data Output



    }
}
