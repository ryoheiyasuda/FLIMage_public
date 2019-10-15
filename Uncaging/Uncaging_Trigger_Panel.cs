
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

namespace FLIMage.Uncaging
{
    public partial class Uncaging_Trigger_Panel : Form
    {
        public ScanParameters State;
        FLIMageMain FLIMage;


        Timer UncagingTimer = new Timer();
        public double[] uncaging_Calib = new double[2];
        public int uncaging_count = 0;

        HardwareControls.IOControls.AnalogOutput UncageMirrorAO;
        HardwareControls.IOControls.DigitalOutputControl digitalOutput;

        PlotOnPictureBox plot1, plot2;

        public bool UncagingShutter = false;
        public bool uncaging_running = false;

        UncagingCalibration uc;

        WindowLocManager winManager;
        String WindowName = "UncagingPanel.loc";

        bool abort_uncaging = false;

        public Uncaging_Trigger_Panel(FLIMageMain fc)
        {
            FLIMage = fc;

            InitializeComponent();
        }


        public void Uncaging_Trigger_Panel_Load(object sender, EventArgs e)
        {
            State = FLIMage.State;
            if (!FLIMage.image_display.uncaging_on)
                FLIMage.image_display.ActivateUncaging();

            plot1 = new PlotOnPictureBox(panel1);
            plot2 = new PlotOnPictureBox(panel2);

            UpdateUncaging(this);

            winManager = new WindowLocManager(this, WindowName, State.Files.windowsInfoPath);
            winManager.LoadWindowLocation(false);
        }

        public void SaveWindowLocation()
        {
            winManager.SaveWindowLocation();
        }

        public void Uncaging_Trigger_Panel_FormClosing(object sender, FormClosingEventArgs e)
        {
            winManager.SaveWindowLocation();
            e.Cancel = true;
            this.Hide();

            FLIMage.ToolWindowClosed();
        }

        public void MoveMirrosDuringUncagingPosCheck_Click(object sender, EventArgs e)
        {
            SetupUncage(sender);
        }


        public void uncage_generic_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tb = (TextBox)sender;
                String SaveText = tb.Text;

                try
                {
                    SetupUncage(sender);
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

        public void SetupUncage(object sender)
        {
            int valI;
            double valD;

            State = FLIMage.State;

            if (Double.TryParse(OutputRate.Text, out valD)) State.Uncaging.outputRate = valD;

            if (Double.TryParse(uncage_interval.Text, out valD)) State.Uncaging.trainInterval = valD;
            if (Int32.TryParse(uncage_pulseN.Text, out valI)) State.Uncaging.nPulses = valI;
            if (Double.TryParse(undage_dwell.Text, out valD)) State.Uncaging.pulseWidth = valD;
            if (Double.TryParse(uncage_power.Text, out valD)) State.Uncaging.Power = valD;
            if (Double.TryParse(Uncage_Length.Text, out valD)) State.Uncaging.sampleLength = valD;
            if (Double.TryParse(Uncage_Delay.Text, out valD)) State.Uncaging.pulseDelay = valD;
            if (Double.TryParse(Uncage_ISI.Text, out valD)) State.Uncaging.pulseISI = valD;
            if (Int32.TryParse(Uncage_Repeat.Text, out valI)) State.Uncaging.trainRepeat = valI;
            if (Double.TryParse(AnalogShutterDelay.Text, out valD)) State.Uncaging.AnalogShutter_delay = valD;
            if (Double.TryParse(DigitalShutterDelay.Text, out valD)) State.Uncaging.DigitalShutter_delay = valD;

            if (Int32.TryParse(SlicesBeforeUncage.Text, out valI)) State.Uncaging.SlicesBeforeUncage = valI;

            if (Double.TryParse(Uncage_SliceInterval.Text, out valD)) State.Uncaging.Uncage_SliceInterval = valD;


            if (Double.TryParse(FramesBeforeUncage.Text, out valD)) State.Uncaging.FramesBeforeUncage = valD;
            if (Double.TryParse(FrameBeforeUncage_ms.Text, out valD)) State.Uncaging.baselineBeforeTrain_forFrame = valD;
            if (Double.TryParse(Uncage_FrameInterval.Text, out valD)) State.Uncaging.Uncage_FrameInterval = valD;
            if (Double.TryParse(Uncage_FrameInterval_ms.Text, out valD)) State.Uncaging.pulseSetInterval_forFrame = valD;


            //if (Double.TryParse(Uncage_FramePInterval.Text, out valD)) FramePulseInterval = valD;
            //if (Double.TryParse(Uncage_FramePDelay.Text, out valD)) FramePulseDelay = valD;



            UncagingShutter = Shutter2.Checked;

            State.Uncaging.sync_withFrame = SyncWithFrame_Check.Checked;
            State.Uncaging.sync_withSlice = SyncWithSlice_Check.Checked;

            if (Double.TryParse(UncagingPosX.Text, out valD)) State.Uncaging.PositionV[0] = valD;
            if (Double.TryParse(UncagingPosY.Text, out valD)) State.Uncaging.PositionV[1] = valD;

            State.Uncaging.name = PulseName.Text;

            try
            {
                if (UncageMultiRoi.SelectedItem.ToString() == "Rotate")
                    State.Uncaging.rotatePosition = true;
                else
                {
                    State.Uncaging.currentPosition = UncageMultiRoi.SelectedIndex;
                    State.Uncaging.rotatePosition = false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Problem in UncagingSelection" + e.Message);
            }

            this.InvokeIfRequired(o => o.UpdateUncaging(sender));
        }

        public void UncageMultiRoi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (UncageMultiRoi.SelectedItem.ToString() == "Rotate")
                State.Uncaging.rotatePosition = true;
            else
            {
                State.Uncaging.currentPosition = UncageMultiRoi.SelectedIndex;
                State.Uncaging.rotatePosition = false;
            }

            UpdateUncaging(sender);
        }

        public void Uncage_Save_Click(object sender, EventArgs e)
        {
            List<String> ls = new List<String>();
            ls.Add("State.Uncaging");
            String oldFileName = State.Files.uncagePathName + Path.DirectorySeparatorChar + "Uncaging-" + State.Uncaging.pulse_number + ".unc";
            Directory.CreateDirectory(State.Files.uncagePathName);
            File.WriteAllText(oldFileName, FLIMage.fileIO.SelectedSetupValues(ls));
        }

        public void UpdateUncaging(object sender)
        {
            if (uc != null)
                uc.updateWindow();

            bool anyUncaging = State.Init.uncagingLasers.Any(item => item == true);
            String str1 = "";

            if (!anyUncaging)
            {
                str1 = "No uncaging laser assigned\r\n";

                if (State.Init.AO_uncagingShutter)
                    str1 = str1 + "AO: " + State.Init.UncagingShutterAnalogPort + "\r\n";

                if (State.Init.DO_uncagingShutter)
                    str1 = str1 + "DO: " + State.Init.MirrorAOBoard + "/port0/" + State.Init.DigitalShutterPort;

                if (!State.Init.AO_uncagingShutter && !State.Init.DO_uncagingShutter)
                    str1 = "No shutter";

            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (State.Init.uncagingLasers[i])
                    {
                        str1 = "Uncaging: Laser-" + (i + 1);
                        break;
                    }
                }

                if (State.Init.AO_uncagingShutter)
                {
                    str1 = str1 + "\r\nUncaging AO shutter: " + State.Init.UncagingShutterAnalogPort;
                }

                if (State.Init.DO_uncagingShutter)
                {
                    str1 = str1 + "\r\nUncaging DO shutter: " + State.Init.MirrorAOBoard + "/port0/" + State.Init.DigitalShutterPort;
                }
            }

            uncageNote.Text = str1;


            double minLength = (State.Uncaging.nPulses - 1) * State.Uncaging.pulseISI + State.Uncaging.pulseDelay + State.Uncaging.pulseWidth + 1;
            if (State.Uncaging.sampleLength < minLength)
                State.Uncaging.sampleLength = minLength;

            double maxDelay = Math.Max(State.Uncaging.AnalogShutter_delay, State.Uncaging.DigitalShutter_delay);
            if (State.Uncaging.pulseDelay < maxDelay)
                State.Uncaging.pulseDelay = maxDelay;

            double msPerLine = State.Acq.msPerLine;
            if (State.Acq.fastZScan)
                msPerLine = State.Acq.FastZ_msPerLine;

            double frameTime = msPerLine * State.Acq.linesPerFrame;

            if (sender != null)
            {
                if (sender.Equals(Uncage_FrameInterval) || sender.Equals(FramesBeforeUncage))
                {
                    State.Uncaging.pulseSetInterval_forFrame = State.Uncaging.Uncage_FrameInterval * frameTime;
                    State.Uncaging.baselineBeforeTrain_forFrame = State.Uncaging.FramesBeforeUncage * frameTime;
                    State.Uncaging.trainInterval = State.Uncaging.pulseSetInterval_forFrame;
                    //State.Uncaging.pulseISI = FramePulseInterval * frameTime;
                    //State.Uncaging.pulseDelay = FramePulseDelay * frameTime;
                }

                else if (sender.Equals(Uncage_FrameInterval_ms) || sender.Equals(FrameBeforeUncage_ms))
                {
                    State.Uncaging.Uncage_FrameInterval = State.Uncaging.pulseSetInterval_forFrame / frameTime;
                    State.Uncaging.FramesBeforeUncage = State.Uncaging.baselineBeforeTrain_forFrame / frameTime;
                    State.Uncaging.trainInterval = State.Uncaging.pulseSetInterval_forFrame;
                }

                else if (sender.Equals(uncage_interval))
                {
                    State.Uncaging.pulseSetInterval_forFrame = State.Uncaging.trainInterval;
                    State.Uncaging.Uncage_FrameInterval = State.Uncaging.pulseSetInterval_forFrame / frameTime;
                }
                else
                    State.Uncaging.trainInterval = State.Uncaging.pulseSetInterval_forFrame;
            }

            State.Uncaging.trainInterval = State.Uncaging.pulseSetInterval_forFrame;

            if (State.Uncaging.outputRate < 1000)
                State.Uncaging.outputRate = 1000;

            //if (State.Uncaging.pulseSetInterval_forFrame < State.Uncaging.sampleLength)
            //{
            //    State.Uncaging.pulseSetInterval_forFrame = State.Uncaging.sampleLength;
            //    State.Uncaging.trainInterval = State.Uncaging.sampleLength;
            //}

            OutputRate.Text = State.Uncaging.outputRate.ToString();
            PulseName.Text = State.Uncaging.name;


            //PulseNumber.Value = State.Uncaging.pulse_number;
            //tab_uncaging.Enabled = anyUncaging;
            //Uncage_while_image_check.Enabled = anyUncaging;
            //UncagePosPanel.Enabled = anyUncaging;
            //UncageOnlyPanel.Enabled = anyUncaging;
            //uncage_power.Enabled = anyUncaging;
            //Shutter2.Enabled = State.Init.AO_uncagingShutter;


            double xOffset = State.Acq.XOffset;
            double yOffset = State.Acq.YOffset;

            if (State.Init.UseExternalMirrorOffset)
            {
                xOffset = 0;
                yOffset = 0;
            }

            if (sender == null || !sender.Equals(UncageMultiRoi))
            {
                UncageMultiRoi.Items.Clear();
                UncageMultiRoi.Items.Add("Current");

                int nCount = FLIMage.image_display.uncagingLocs.Count;


                if (nCount > 0)
                {
                    State.Uncaging.multiUncagingPosition = true;
                    State.Uncaging.UncagingPositionsX = new double[nCount];
                    State.Uncaging.UncagingPositionsY = new double[nCount];
                    State.Uncaging.UncagingPositionsVX = new double[nCount];
                    State.Uncaging.UncagingPositionsVY = new double[nCount];



                    for (int i = 0; i < nCount; i++)
                    {
                        State.Uncaging.UncagingPositionsX[i] = FLIMage.image_display.uncagingLocs[i][0];
                        State.Uncaging.UncagingPositionsY[i] = FLIMage.image_display.uncagingLocs[i][1];

                        double[] xy_voltage = HardwareControls.IOControls.PositionFracToVoltage(FLIMage.image_display.uncagingLocs[i], State);
                        State.Uncaging.UncagingPositionsVX[i] = xy_voltage[0];
                        State.Uncaging.UncagingPositionsVY[i] = xy_voltage[1];
                        //State.Uncaging.UncagingPositionsVX[i] = (FLIMage.image_display.uncagingLocs[i][0] - 0.5) * State.Acq.XMaxVoltage / State.Acq.zoom + State.Uncaging.CalibV[0] + xOffset;
                        //State.Uncaging.UncagingPositionsVY[i] = (FLIMage.image_display.uncagingLocs[i][1] - 0.5) * State.Acq.YMaxVoltage / State.Acq.zoom + State.Uncaging.CalibV[1] + yOffset;
                    }


                    for (int i = 1; i < nCount + 1; i++) //Some reason need one more?
                    {
                        UncageMultiRoi.Items.Add(i.ToString());
                    }
                    UncageMultiRoi.Items.Add("Rotate");

                    if (State.Uncaging.rotatePosition)
                        UncageMultiRoi.SelectedItem = "Rotate";
                    else if (State.Uncaging.currentPosition < nCount + 1)
                        UncageMultiRoi.SelectedIndex = State.Uncaging.currentPosition;

                }
                else
                {
                    State.Uncaging.rotatePosition = false;
                    State.Uncaging.multiUncagingPosition = false;
                    UncageMultiRoi.SelectedIndex = 0;
                    State.Uncaging.currentPosition = 0;
                }
            }

            double[] pos = (double[])FLIMage.image_display.uncagingLocFrac.Clone();

            if (pos[0] > 0.0 && pos[0] < 1.0 && pos[1] > 0.0 && pos[1] < 1.0)
            {
                State.Uncaging.Position = pos;
            }
            else
            {
                FLIMage.image_display.uncagingLocFrac = (double[])State.Uncaging.Position.Clone();
            }


            //State.Uncaging.PositionV[0] = (State.Uncaging.Position[0] - 0.5) * State.Acq.XMaxVoltage / State.Acq.zoom + State.Uncaging.CalibV[0] + xOffset;
            //State.Uncaging.PositionV[1] = (State.Uncaging.Position[1] - 0.5) * State.Acq.YMaxVoltage / State.Acq.zoom + State.Uncaging.CalibV[1] + yOffset;
            State.Uncaging.PositionV = HardwareControls.IOControls.PositionFracToVoltage(State.Uncaging.Position, State);

            UncagingPosX.Text = String.Format("{0:0.000}", State.Uncaging.PositionV[0]);
            UncagingPosY.Text = String.Format("{0:0.000}", State.Uncaging.PositionV[1]);

            uncage_interval.Text = String.Format("{0}", State.Uncaging.trainInterval);
            uncage_pulseN.Text = String.Format("{0}", State.Uncaging.nPulses);
            undage_dwell.Text = String.Format("{0}", State.Uncaging.pulseWidth);
            uncage_power.Text = String.Format("{0}", State.Uncaging.Power);
            Uncage_ISI.Text = String.Format("{0}", State.Uncaging.pulseISI);
            Uncage_Delay.Text = String.Format("{0}", State.Uncaging.pulseDelay);
            Uncage_Length.Text = String.Format("{0}", State.Uncaging.sampleLength);
            Uncage_Repeat.Text = String.Format("{0}", State.Uncaging.trainRepeat);
            AnalogShutterDelay.Text = String.Format("{0}", State.Uncaging.AnalogShutter_delay);
            DigitalShutterDelay.Text = String.Format("{0}", State.Uncaging.DigitalShutter_delay);

            FramesBeforeUncage.Text = String.Format("{0:0.00}", State.Uncaging.FramesBeforeUncage);
            FrameBeforeUncage_ms.Text = String.Format("{0:0}", State.Uncaging.baselineBeforeTrain_forFrame);
            Uncage_FrameInterval.Text = String.Format("{0:0.00}", State.Uncaging.Uncage_FrameInterval);
            Uncage_FrameInterval_ms.Text = String.Format("{0:0}", State.Uncaging.pulseSetInterval_forFrame);

            if (State.Acq.ZStack)
            {
                FrameNote.Text = "Need time-lapse, not Z-stack";
            }
            else
            {
                FrameNote.Text = String.Format("Frame interval = {0} ms", Math.Round(frameTime));
            }

            //double frameTime = State.Acq.msPerLine * State.Acq.linesPerFrame;
            //Uncage_FramePInterval.Text = String.Format("{0:0.00}", State.Uncaging.pulseISI / frameTime);
            //Uncage_FramePDelay.Text = String.Format("{0:0.00}", State.Uncaging.pulseDelay / frameTime);

            SlicesBeforeUncage.Text = String.Format("{0}", State.Uncaging.SlicesBeforeUncage);
            Uncage_SliceInterval.Text = String.Format("{0}", State.Uncaging.Uncage_SliceInterval);

            if (State.Acq.ZStack)
            {
                BaseLine_Slice_s.Text = "NA";
                SliceInterval_s.Text = "NA";
                SliceNote.Text = "Need time-lapse, not Z-stack";
            }
            else
            {
                double baseline_s;
                baseline_s = State.Uncaging.SlicesBeforeUncage * State.Acq.sliceInterval;
                BaseLine_Slice_s.Text = String.Format("{0:0.00} s", baseline_s);
                double interval_s = State.Uncaging.Uncage_SliceInterval * State.Acq.sliceInterval;
                SliceInterval_s.Text = String.Format("{0:0.00} s", interval_s);
                SliceNote.Text = String.Format("Slice interval = {0:0.00} s", State.Acq.sliceInterval);
            }


            U_counter.Text = "0 /" + State.Uncaging.trainRepeat.ToString();
            U_counter2.Text = "0 /" + State.Uncaging.trainRepeat.ToString();
            RepeatFrame.Text = State.Uncaging.trainRepeat.ToString();

            Shutter2.Checked = UncagingShutter;

            SyncWithFrame_Check.Checked = State.Uncaging.sync_withFrame;
            SyncWithSlice_Check.Checked = State.Uncaging.sync_withSlice;

            if (FLIMage.flimage_io.uc != null && !FLIMage.flimage_io.uc.IsDisposed)
                FLIMage.flimage_io.uc.updateWindow();

            if (FLIMage.flimage_io.shading != null)
                FLIMage.flimage_io.shading.applyCalibration(State);




            updatePlot(plot1);
            updatePlot(plot2);
        }

        public void updatePlot(PlotOnPictureBox plot)
        {
            plot.ClearData();
            double[] t1;
            double[,] Data;
            bool plotEOM = true;

            if (plot.Equals(plot2))
            {
                plotEOM = false;
                Data = HardwareControls.IOControls.MakeUncagePulses_MirrorAO(State, State.Uncaging.outputRate);
            }
            else
            {
                Data = HardwareControls.IOControls.MakePockelsPulses_PockelsAO(State, State.Uncaging.outputRate, ShowShutter.Checked, ShowRepeat.Checked, FLIMage.flimage_io.shading);
            }

            if (Data.GetLength(1) > 0 && Data.GetLength(0) > 0)
            {
                t1 = new double[Data.GetLength(1)];
                double[] Data1 = new double[Data.GetLength(1)];
                for (int i = 0; i < t1.Length; i++)
                    t1[i] = i * 1000.0 / State.Uncaging.outputRate; //in msec.

                for (int j = 0; j < Data.GetLength(0); j++)
                {
                    for (int i = 0; i < Data1.Length; i++)
                        Data1[i] = Data[j, i];
                    plot.AddData(t1, Data1, "-", 1);
                }
            }

            if (State.Init.DO_uncagingShutter && plotEOM && ShowShutter.Checked)
            {
                double[][] dodata = HardwareControls.IOControls.GetDigitalOutputInDouble(State, true, false, ShowRepeat.Checked, out double outputRate);
                double[] Data_DO_Shutter = dodata[0];

                t1 = new double[Data_DO_Shutter.Length];
                for (int i = 0; i < t1.Length; i++)
                    t1[i] = i * 1000.0 / outputRate; //in msec.

                plot.AddData(t1, Data_DO_Shutter, "-", 1);
            }

            plot.XTitle = "Time (ms)";
            plot.YTitle = "Voltage";

            plot.plot.xMergin = 0.12F;
            plot.plot.xFrac = 0.85F;
            plot.plot.yMergin = 0.3F;
            plot.plot.yFrac = 0.45F;

            plot.UpdatePlot();
        }

        void UncagingTimerEvent(Object myObject, EventArgs myEventArgs)
        {
            UncageOnce(true, this);
            if (uncaging_count == State.Uncaging.trainRepeat)
            {
                UncagingTimer.Stop();
                UncagingTimer.Dispose();
                StartUncaging_button.InvokeIfRequired(o => o.Text = "Start");
                FLIMage.ExternalCommand("UncagingDone");
                uncaging_running = false;
            }
        }

        public void StartPrep()
        {
            uncaging_count = 0;
            UpdateUncagingCounter();
        }

        public void StartUncaging_button_Click(object sender, EventArgs e)
        {
            uncaging_running = true;
            abort_uncaging = false;

            SetupUncage(sender);

            if (StartUncaging_button.Text.Equals("Start"))
            {
                StartPrep();
                StartUncaging_button.InvokeIfRequired(o => o.Text = "Stop");

                if (State.Uncaging.trainRepeat > 1)
                {
                    UncagingTimer = new Timer();
                    UncagingTimer.Tick += new EventHandler(UncagingTimerEvent);
                    UncagingTimer.Interval = (int)State.Uncaging.trainInterval;
                    UncagingTimer.Start();
                }

                UncageOnce(true, this);

                if (State.Uncaging.trainRepeat <= 1)
                {
                    StartUncaging_button.InvokeIfRequired(o => o.Text = "Start");
                    State.Uncaging.trainRepeat = 1;

                }
            }
            else
            {
                StopUncaging();
            }
        }

        public void StopUncaging()
        {
            StartUncaging_button.Text = "Start";
            UncagingTimer.Stop();
            UncagingTimer.Dispose();
            abort_uncaging = true;
            uncaging_running = false;
        }

        public void UpdateUncagingCounter()
        {
            U_counter.Text = String.Format("{0} / {1}", uncaging_count, State.Uncaging.trainRepeat);
            U_counter2.Text = String.Format("{0} / {1}", uncaging_count, State.Uncaging.trainRepeat);
        }

        public void CleanBufferForUncaging()
        {
            if (UncageMirrorAO != null)
                UncageMirrorAO.Dispose();
            if (digitalOutput != null)
                digitalOutput.Dispose();
        }

        /// <summary>
        /// Perform one uncaging event. 
        /// </summary>
        /// <param name="mainShutterCtrl">if you want to open/close shutter within this function, it is true</param>
        /// <param name="sender">is it from uncaging_panel or FLIMage window?</param>
        public void UncageOnce(bool mainShutterCtrl, object sender)
        {
            CleanBufferForUncaging();

            int pos = State.Uncaging.currentPosition;

            double[,] uncagingPos = HardwareControls.IOControls.DefinePulsePosition(State);

            //Put static values first.
            FLIMage.flimage_io.AO_Mirror_EOM.putValue_Single(new double[] { uncagingPos[0, 0], uncagingPos[1, 0] }, true, false);
            FLIMage.flimage_io.uncagingShutterCtrl(false, true, true); //Close uncaging shutter.

            if (State.Init.DO_uncagingShutter)
            {
                digitalOutput = new HardwareControls.IOControls.DigitalOutputControl(State);
                digitalOutput.PutValue(false, true, false, false, false);
                digitalOutput.Start(false);
            }

            if (mainShutterCtrl)
            {
                FLIMage.flimage_io.shutterCtrl.open();
                System.Threading.Thread.Sleep(1); //Wait for shutter open.
            }

            UncageMirrorAO = new HardwareControls.IOControls.AnalogOutput(State, FLIMage.flimage_io.shading, true);
            FLIMage.flimage_io.shading.applyCalibration(State);
            double[,] dataXY = UncageMirrorAO.putvalueUncageOnce();
            UncageMirrorAO.Start(false);

            FLIMage.flimage_io.dioTrigger.Evoke();

            int timeout = (int)(State.Uncaging.sampleLength + 10.0);

            bool forcestop = false;

            //For a long call, you may want to terminate in the middle. 
            Stopwatch sw1 = new Stopwatch();
            sw1.Restart();
            while (sw1.ElapsedMilliseconds < timeout)
            {
                System.Threading.Thread.Sleep(10);
                //abort_uncaging becomes true when stop button is pressed.
                if (abort_uncaging)
                {
                    forcestop = true;
                    break;
                }
            }

            if (!forcestop)
            {
                UncageMirrorAO.WaitUntilDone(timeout); //Should be immediate but anyway....
            }
            else
            {
                UncageMirrorAO.Stop();
            }

            if (State.Init.DO_uncagingShutter)
            {
                digitalOutput.Stop();
                digitalOutput.Dispose();
            }

            if (mainShutterCtrl)
                FLIMage.flimage_io.shutterCtrl.Close();

            UncageMirrorAO.Dispose();

            uncaging_count++;
            Debug.WriteLine("Uncaging counter = " + uncaging_count);

            System.Threading.Thread.Sleep(1);

            this.InvokeIfRequired(o => o.UpdateUncagingCounter());
        }

        public void PulseNumber_ValueChanged(object sender, EventArgs e)
        {

            if (PulseNumber.Value > 0)
            {
                String newFileName = State.Files.uncagePathName + Path.DirectorySeparatorChar + "Uncaging-" + PulseNumber.Value + ".unc";

                if (File.Exists(newFileName))
                {
                    ScanParameters StateOld = FLIMage.fileIO.CopyState();
                    ScanParameters StateNew = FLIMage.fileIO.CopyState(); //Detach from current State file.
                    FileIO fileIONew = new FileIO(StateNew); //new fileIO synthesized.
                    fileIONew.LoadSetupFile(newFileName); //put new State on the fileIO.
                    State.Uncaging = fileIONew.State.Uncaging;
                    State.Uncaging.CalibV = StateOld.Uncaging.CalibV;
                    State.Uncaging.Position = StateOld.Uncaging.Position;
                    State.Uncaging.PositionV = StateOld.Uncaging.PositionV;
                }
                else
                {
                    State.Uncaging.pulse_number = (int)PulseNumber.Value;
                    //State.Uncaging.multiUncagingPosition = false;
                }


                FLIMage.image_display.uncagingLocFrac = (double[])State.Uncaging.Position.Clone();
                FLIMage.image_display.uncagingLocs.Clear();

                if (State.Uncaging.multiUncagingPosition)
                {
                    for (int i = 0; i < State.Uncaging.UncagingPositionsX.Length; i++)
                    {
                        FLIMage.image_display.uncagingLocs.Add(new double[] { State.Uncaging.UncagingPositionsX[i], State.Uncaging.UncagingPositionsY[i] });
                    }
                }


                UpdateUncaging(sender);
                FLIMage.image_display.Refresh();
            }
        }


        public void Shutter2_Click(object sender, EventArgs e)
        {
            FLIMage.flimage_io.uncagingShutterCtrl(Shutter2.Checked, !FLIMage.flimage_io.grabbing && !FLIMage.flimage_io.focusing, true);
        }

        public void ShowShutter_Click(object sender, EventArgs e)
        {
            UpdateUncaging(sender);
        }

        public void Generic_RadioButton(object sender, EventArgs e)
        {
            SetupUncage(sender);
        }

        public void calibrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uc = new UncagingCalibration(FLIMage);
            uc.Show();
        }

        private void ShowShutter_CheckedChanged(object sender, EventArgs e)
        {

        }

        public void miscSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Uncaging_miscSetting um = new Uncaging_miscSetting(FLIMage);
            um.Show();
        }


    }
}
