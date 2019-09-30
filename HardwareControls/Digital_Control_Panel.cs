using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace FLIMage.HardwareControls
{
    public partial class Digital_Trigger_Panel : Form
    {
        public ScanParameters State;
        FLIMageMain FLIMage;


        Timer DigitalTimer = new Timer();
        public int DO_count = 0;

        IOControls.DigitalOutputSignal digitalOutput;

        PlotOnPictureBox plot;

        public bool digital_running = false;

        WindowLocManager winManager;
        String WindowName = "DigitalPanel.loc";

        bool abort_digital = false;

        public Digital_Trigger_Panel(FLIMageMain fc)
        {
            FLIMage = fc;

            InitializeComponent();
        }


        public void Digital_Trigger_Panel_Load(object sender, EventArgs e)
        {
            State = FLIMage.State;

            plot = new PlotOnPictureBox(panel1);
            UpdateDO(this);

            winManager = new WindowLocManager(this, WindowName, State.Files.windowsInfoPath);
            winManager.LoadWindowLocation(false);
        }

        public void SaveWindowLocation()
        {
            winManager.SaveWindowLocation();
        }

        public void Digital_Trigger_Panel_FormClosing(object sender, FormClosingEventArgs e)
        {
            winManager.SaveWindowLocation();
            e.Cancel = true;
            this.Hide();

            FLIMage.ToolWindowClosed();
        }


        public void digital_generic_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tb = (TextBox)sender;
                String SaveText = tb.Text;

                try
                {
                    SetupDO(sender);
                }
                catch (System.FormatException)
                {
                    tb.Text = SaveText;
                }
                finally
                { };
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

        }

        public void SetupDO(object sender)
        {
            int valI;
            double valD;
            int ch = ChannelComboBox.SelectedIndex;
            if (ch < 0)
                ch = 0;

            State = FLIMage.State;

            if (Double.TryParse(OutputRate.Text, out valD)) State.DO.outputRate = valD;

            if (Double.TryParse(DO_interval.Text, out valD)) State.DO.trainInterval = valD;
            if (Int32.TryParse(pulseN.Text, out valI)) State.DO.nPulses[ch] = valI;
            if (Double.TryParse(dwell.Text, out valD)) State.DO.pulseWidth[ch] = valD;
            if (Double.TryParse(Length.Text, out valD)) State.DO.sampleLength = valD;
            if (Double.TryParse(Delay.Text, out valD)) State.DO.pulseDelay[ch] = valD;
            if (Double.TryParse(ISI.Text, out valD)) State.DO.pulseISI[ch] = valD;
            if (Int32.TryParse(DO_Repeat.Text, out valI)) State.DO.trainRepeat = valI;

            if (Int32.TryParse(SlicesBeforePulse.Text, out valI)) State.DO.SlicesBeforeDO = valI;

            if (Double.TryParse(SliceInterval.Text, out valD)) State.DO.SliceInterval = valD;


            if (Double.TryParse(FramesBeforePulse.Text, out valD)) State.DO.FramesBeforeDO = valD;
            if (Double.TryParse(FrameBeforePulse_ms.Text, out valD)) State.DO.baselineBeforeTrain_forFrame = valD;
            if (Double.TryParse(FrameInterval.Text, out valD)) State.DO.FrameInterval = valD;
            if (Double.TryParse(FrameInterval_ms.Text, out valD)) State.DO.pulseSetInterval_forFrame = valD;

            State.DO.active_high[ch] = ActiveHighCheck.Checked;
            //if (Double.TryParse(Uncage_FramePInterval.Text, out valD)) FramePulseInterval = valD;
            //if (Double.TryParse(Uncage_FramePDelay.Text, out valD)) FramePulseDelay = valD;



            State.DO.sync_withFrame = SyncWithFrame_Check.Checked;
            State.DO.sync_withSlice = SyncWithSlice_Check.Checked;

            State.DO.name = PulseName.Text;

            if (this.InvokeRequired)
                this.Invoke((Action)delegate
                {
                    UpdateDO(sender);
                });
            else
                UpdateDO(sender);
        }


        public void Uncage_Save_Click(object sender, EventArgs e)
        {
            List<String> ls = new List<String>();
            ls.Add("State.DO");
            String oldFileName = State.Files.uncagePathName + Path.DirectorySeparatorChar + "Digital-" + State.DO.pulse_number + ".dig";
            Directory.CreateDirectory(State.Files.uncagePathName);
            File.WriteAllText(oldFileName, FLIMage.fileIO.SelectedSetupValues(ls));
        }

        public void UpdateDO(object sender)
        {
            int ch = ChannelComboBox.SelectedIndex;
            if (ch < 0)
                ch = 0;
            double maxLength = 0;
            for (int i = 0; i < State.DO.nPulses.Length; i++)
            {
                double len1 = (State.DO.nPulses[i] - 1) * State.DO.pulseISI[i] + State.DO.pulseDelay[i] + State.DO.pulseWidth[i] + 1;
                if (len1 > maxLength)
                    maxLength = len1;
            }

            if (State.DO.sampleLength < maxLength)
                State.DO.sampleLength = maxLength;

            double msPerLine = State.Acq.msPerLine;
            if (State.Acq.fastZScan)
                msPerLine = State.Acq.FastZ_msPerLine;

            double frameTime = msPerLine * State.Acq.linesPerFrame;

            if (sender != null)
            {
                if (sender.Equals(FrameInterval) || sender.Equals(FramesBeforePulse))
                {
                    State.DO.pulseSetInterval_forFrame = State.DO.FrameInterval * frameTime;
                    State.DO.baselineBeforeTrain_forFrame = State.DO.FramesBeforeDO * frameTime;
                    State.DO.trainInterval = State.DO.pulseSetInterval_forFrame;
                    //State.DO.pulseISI = FramePulseInterval * frameTime;
                    //State.DO.pulseDelay = FramePulseDelay * frameTime;
                }

                else if (sender.Equals(FrameInterval_ms) || sender.Equals(FrameBeforePulse_ms))
                {
                    State.DO.FrameInterval = State.DO.pulseSetInterval_forFrame / frameTime;
                    State.DO.FramesBeforeDO = State.DO.baselineBeforeTrain_forFrame / frameTime;
                    State.DO.trainInterval = State.DO.pulseSetInterval_forFrame;
                }

                else if (sender.Equals(DO_interval))
                {
                    State.DO.pulseSetInterval_forFrame = State.DO.trainInterval;
                    State.DO.FrameInterval = State.DO.pulseSetInterval_forFrame / frameTime;
                }
                else
                    State.DO.trainInterval = State.DO.pulseSetInterval_forFrame;
            }

            State.DO.trainInterval = State.DO.pulseSetInterval_forFrame;

            if (State.DO.outputRate < 1000)
                State.DO.outputRate = 1000;

            if (State.DO.pulseSetInterval_forFrame < State.DO.sampleLength)
            {
                State.DO.pulseSetInterval_forFrame = State.DO.sampleLength;
                State.DO.trainInterval = State.DO.sampleLength;
            }

            OutputRate.Text = State.DO.outputRate.ToString();
            PulseName.Text = State.DO.name;

            ActiveHighCheck.Checked = State.DO.active_high[ch];

            DO_interval.Text = String.Format("{0}", State.DO.trainInterval);
            pulseN.Text = String.Format("{0}", State.DO.nPulses[ch]);
            dwell.Text = String.Format("{0}", State.DO.pulseWidth[ch]);
            ISI.Text = String.Format("{0}", State.DO.pulseISI[ch]);
            Delay.Text = String.Format("{0}", State.DO.pulseDelay[ch]);

            Length.Text = String.Format("{0}", State.DO.sampleLength);
            DO_Repeat.Text = String.Format("{0}", State.DO.trainRepeat);

            FramesBeforePulse.Text = String.Format("{0:0.00}", State.DO.FramesBeforeDO);
            FrameBeforePulse_ms.Text = String.Format("{0:0}", State.DO.baselineBeforeTrain_forFrame);
            FrameInterval.Text = String.Format("{0:0.00}", State.DO.FrameInterval);
            FrameInterval_ms.Text = String.Format("{0:0}", State.DO.pulseSetInterval_forFrame);

            if (State.Acq.ZStack)
            {
                FrameNote.Text = "Need time-lapse, not Z-stack";
            }
            else
            {
                FrameNote.Text = String.Format("Frame interval = {0} ms", Math.Round(frameTime));
            }

            //double frameTime = State.Acq.msPerLine * State.Acq.linesPerFrame;
            //Uncage_FramePInterval.Text = String.Format("{0:0.00}", State.DO.pulseISI / frameTime);
            //Uncage_FramePDelay.Text = String.Format("{0:0.00}", State.DO.pulseDelay / frameTime);

            SlicesBeforePulse.Text = String.Format("{0}", State.DO.SlicesBeforeDO);
            SliceInterval.Text = String.Format("{0}", State.DO.SliceInterval);

            if (State.Acq.ZStack)
            {
                BaseLine_Slice_s.Text = "NA";
                SliceInterval_s.Text = "NA";
                SliceNote.Text = "Need time-lapse, not Z-stack";
            }
            else
            {
                double baseline_s;
                baseline_s = State.DO.SlicesBeforeDO * State.Acq.sliceInterval;
                BaseLine_Slice_s.Text = String.Format("{0:0.00} s", baseline_s);
                double interval_s = State.DO.SliceInterval * State.Acq.sliceInterval;
                SliceInterval_s.Text = String.Format("{0:0.00} s", interval_s);
                SliceNote.Text = String.Format("Slice interval = {0:0.00} s", State.Acq.sliceInterval);
            }


            U_counter.Text = "0 /" + State.DO.trainRepeat.ToString();
            U_counter2.Text = "0 /" + State.DO.trainRepeat.ToString();
            RepeatFrame.Text = State.DO.trainRepeat.ToString();

            SyncWithFrame_Check.Checked = State.DO.sync_withFrame;
            SyncWithSlice_Check.Checked = State.DO.sync_withSlice;

            if (FLIMage.flimage_io.uc != null && !FLIMage.flimage_io.uc.IsDisposed)
                FLIMage.flimage_io.uc.updateWindow();

            if (FLIMage.flimage_io.shading != null)
                FLIMage.flimage_io.shading.applyCalibration(State);

            UpdatePlot();

            if (FLIMage.flimage_io.use_nidaq)
            {
                var DO1 = new IOControls.DigitalOutputSignal(State);
                DO1.PutSingleValue(false);
                DO1.dispose();
            }
        }

        void DigitalTimerEvent(Object myObject, EventArgs myEventArgs)
        {
            DigitalOutOnce(true, this);
            if (DO_count == State.DO.trainRepeat)
            {
                DigitalTimer.Stop();
                DigitalTimer.Dispose();
                StartDO_button.Text = "Start";
                digital_running = false;
            }
        }

        public void StartPrep()
        {
            DO_count = 0;
            abort_digital = false;
            SetupDO(this);
            UpdateDOCounter();
        }

        public void Start_button_Click(object sender, EventArgs e)
        {
            digital_running = true;

            if (StartDO_button.Text.Equals("Start"))
            {
                StartPrep();
                StartDO_button.Text = "Stop";

                if (State.DO.trainRepeat > 1)
                {
                    DigitalTimer = new Timer();
                    DigitalTimer.Tick += new EventHandler(DigitalTimerEvent);
                    DigitalTimer.Interval = (int)State.DO.trainInterval;
                    DigitalTimer.Start();
                }

                DigitalOutOnce(true, this);

                if (State.DO.trainRepeat <= 1)
                {
                    StartDO_button.Text = "Start";
                    State.DO.trainRepeat = 1;

                }
            }
            else
            {
                Stop_DO();
            }
        }

        public void Stop_DO()
        {
            StartDO_button.Text = "Start";
            DigitalTimer.Stop();
            DigitalTimer.Dispose();
            abort_digital = true;
            digital_running = false;
        }

        public void UpdateDOCounter()
        {
            U_counter.Text = String.Format("{0} / {1}", DO_count, State.DO.trainRepeat);
            U_counter2.Text = String.Format("{0} / {1}", DO_count, State.DO.trainRepeat);
        }

        public void CleanBufferForDO()
        {
            if (digitalOutput != null)
                digitalOutput.dispose();
        }

        /// <summary>
        /// Perform one uncaging event. 
        /// </summary>
        /// <param name="mainShutterCtrl">if you want to open/close shutter within this function, it is true</param>
        /// <param name="sender">is it from uncaging_panel or FLIMage window?</param>
        public void DigitalOutOnce(bool mainShutterCtrl, object sender)
        {
            CleanBufferForDO();

            digitalOutput = new IOControls.DigitalOutputSignal(State);
            digitalOutput.PutValue_and_Start(false, false, true, false);

            if (mainShutterCtrl)
            {
                FLIMage.flimage_io.ShutterCtrl.open();
            }

            System.Threading.Thread.Sleep(1); //Wait for misc

            FLIMage.flimage_io.dioTrigger.Evoke();

            int timeout = (int)(State.DO.sampleLength + 10.0);

            bool forcestop = false;

            //For a long call, you may want to terminate in the middle. 
            Stopwatch sw1 = new Stopwatch();
            sw1.Restart();
            while (sw1.ElapsedMilliseconds < timeout)
            {
                System.Threading.Thread.Sleep(10);
                Application.DoEvents();
                //abort_digital becomes true when stop button is pressed.
                if (abort_digital)
                {
                    forcestop = true;
                    break;
                }
            }


            if (digitalOutput != null)
            {
                digitalOutput.Stop();
                digitalOutput.dispose();
            }

            if (mainShutterCtrl)
                FLIMage.flimage_io.ShutterCtrl.close();


            DO_count++;
            Debug.WriteLine("DO counter = " + DO_count);

            System.Threading.Thread.Sleep(1);

            if (U_counter.Created)
                U_counter.BeginInvoke(new Action(() => UpdateDOCounter()));
        }

        public void PulseNumber_ValueChanged(object sender, EventArgs e)
        {
            int ch = ChannelComboBox.SelectedIndex;
            if (PulseNumber.Value > 0)
            {
                String newFileName = State.Files.uncagePathName + Path.DirectorySeparatorChar + "Digital-" + PulseNumber.Value + ".dig";

                if (File.Exists(newFileName))
                {
                    ScanParameters StateOld = FLIMage.fileIO.CopyState();
                    ScanParameters StateNew = FLIMage.fileIO.CopyState(); //Detach from current State file.
                    FileIO fileIONew = new FileIO(StateNew); //new fileIO synthesized.
                    fileIONew.LoadSetupFile(newFileName); //put new State on the fileIO.
                    State.DO = fileIONew.State.DO;
                }
                else
                {
                    State.DO.pulse_number = (int)PulseNumber.Value;
                    //State.DO.multiUncagingPosition = false;
                }


                UpdateDO(sender);
                FLIMage.image_display.Refresh();
            }
        }

        public void UpdatePlot()
        {
            plot.ClearData();
            double[][] data_all = IOControls.GetDigitalOutputInDouble(State, false, true, ShowRepeat.Checked, out double outputRate);
            double[] t1;

            if (data_all[0].Length > 0)
            {
                t1 = new double[data_all[0].Length];
                for (int i = 0; i < t1.Length; i++)
                    t1[i] = i * 1000.0 / outputRate; //in msec.

                for (int i = 0; i < data_all.Length; i++)
                    plot.AddData(t1, data_all[i], "-", 1);
            }

            plot.XTitle = "Time (ms)";
            plot.YTitle = "Signal";

            plot.plot.xMergin = 0.12F;
            plot.plot.xFrac = 0.85F;
            plot.plot.yMergin = 0.3F;
            plot.plot.yFrac = 0.45F;

            plot.UpdatePlot();

        }


        public void Generic_RadioButton(object sender, EventArgs e)
        {
            SetupDO(sender);
        }

        private void Digital_Trigger_Panel_Shown(object sender, EventArgs e)
        {
            ChannelComboBox.Items.Add(State.Init.DigitalOutput1);
            ChannelComboBox.Items.Add(State.Init.DigitalOutput2);
            ChannelComboBox.Items.Add(State.Init.DigitalOutput3);
            ChannelComboBox.SelectedIndex = 0;
        }

        private void ShowRepeat_CheckedChanged(object sender, EventArgs e)
        {
            SetupDO(sender);
        }

        private void ChannelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDO(sender);
        }

        private void ActiveHighCheck_Click(object sender, EventArgs e)
        {
            SetupDO(sender);
        }
    }
}
