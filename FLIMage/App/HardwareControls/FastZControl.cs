using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TagLensController;
using TCSPC_controls;
using Utilities;

namespace FLIMage.HardwareControls
{
    public partial class FastZControl : Form
    {
        FLIMageMain flimage;
        ScanParameters State;
        FLIM_Parameters parameters;
        WindowLocManager winManager;
        String WindowName = "FastZControl.loc";
        TagCommand tag_command;

        Dictionary<int, double[]> preset = new Dictionary<int, double[]>();

        double[] acqData = new double[6];
        bool lockStarted = false;

        String comPort = "";

        public FastZControl(FLIMageMain FLIMage_in)
        {
            InitializeComponent();
            GetParamFromFLIMage(FLIMage_in);
            tag_command = new TagCommand();
            PresetCalculator(); //it will calculate, since it will change the preset.
            FillGUI();
            //preset[id] = amp_um, freq_Hz, amp_%, phase_deg, objective.
            LockResonance.Enabled = false;

            if (!ConnectDevice(State.Init.TagLensPort))
                InitializeSerial();
            else
            {
                ComportPulldown.Visible = false;
                StatusLabel.Text = "Connected";
            }
        }

        private void FastZControl_Load(object sender, EventArgs e)
        {
            winManager = new WindowLocManager(this, WindowName, State.Files.windowsInfoPath);
            winManager.LoadWindowLocation(false);
        }

        /// <summary>
        /// Look for match of pulldown and then set the value.
        /// </summary>
        private void SetPreset_Pulldown()
        {
            double nearestAmp = 100;
            int nearestIdx = 0;
            double oldfreq = State.Acq.FastZ_Freq;
            for (int i = 0; i < preset.Count; i++)
            {
                if (preset[i][1] >= oldfreq * 0.9 && preset[i][1] < oldfreq * 1.1)
                {
                    double diff = Math.Abs(preset[i][2] - State.Acq.FastZ_Amp);
                    if (diff < nearestAmp)
                    {
                        nearestAmp = diff;
                        nearestIdx = i;
                    }
                }
            }

            Preset_Pulldown.SelectedIndex = nearestIdx;
        }

        /// <summary>
        /// For window closing, I think we should turn off power, turn off pulses and then disconnect COM port.
        /// This is called by FormClosing. Also, when software is shutting down, it is called.
        /// </summary>
        public void WindowClosing()
        {
            PowerOff();
            PulseOff();
            DisConnect();
            winManager.SaveWindowLocation();
        }

        /// <summary>
        /// Called when the window is closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FastZControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            WindowClosing();
            this.Hide();
            flimage.ToolWindowClosed();
        }

        /// <summary>
        /// Connect with flimage, realtime State, FLIM parameters etc.
        /// </summary>
        /// <param name="FLIMage_in"></param>
        public void GetParamFromFLIMage(FLIMageMain FLIMage_in)
        {
            flimage = FLIMage_in;
            State = flimage.State;
            parameters = flimage.flimage_io.parameters;
        }

        /// <summary>
        /// Get the port lists from the computer, and display it. 
        /// This is active only when the default port setting failed.
        /// </summary>
        public void InitializeSerial()
        {
            string[] ports = SerialPort.GetPortNames();
            ComportPulldown.Items.Clear();
            foreach (string port in ports)
                ComportPulldown.Items.Add(port);

            if (ports != null && ports.Length > 0)
            {
                ComportPulldown.SelectedIndex = 0;
                StatusLabel.Text = "Select COM port and connect";
            }
            else
            {
                ComportPulldown.Visible = false;
                StatusLabel.Text = "No COM port found";
            }
        }

        /// <summary>
        /// State parameter --> GUIs
        /// </summary>
        public void FillGUI()
        {
            if (State.Acq.FastZ_nSlices < 1)
                State.Acq.FastZ_nSlices = 1;
           
                NFastZSlices.Text = State.Acq.FastZ_nSlices.ToString();
                PhaseRangeStart.Text = State.Acq.FastZ_PhaseRange[0].ToString();
                PhaseRangeEnd.Text = State.Acq.FastZ_PhaseRange[1].ToString();

                FormControllers.PulldownSelectByItemString(SetFrequency_Pulldown, State.Acq.FastZ_Freq.ToString());
                AmplitudeEditBox.Text = State.Acq.FastZ_Amp.ToString();
                PhaseTextBox1.Text = State.Acq.FastZ_Phase[0].ToString();
                PhaseTextBox2.Text = State.Acq.FastZ_Phase[1].ToString();
                PhaseTextBox3.Text = State.Acq.FastZ_Phase[2].ToString();

                //SetPreset_Pulldown();
        }

        /// <summary>
        /// This will call the measurement of tag lens frequency by FLIM card.
        /// </summary>
        public void MeasureTagLensParameters()
        {
            UpdateStateFromGUI(flimage); //Get Parameter from the window.
            flimage.SetupFLIMParameters();
            parameters.enableFastZscan = false;
            if (flimage.flimage_io.tcspc_on)
                return;
            flimage.flimage_io.FiFo_StartNew(true, true, true);
            System.Threading.Thread.Sleep(100);
            flimage.flimage_io.FiFo_StopMeas(true);
            System.Threading.Thread.Sleep(100);
        }

        /// <summary>
        /// Called by "Measure" button. Measures the tag lens parameters with FLIM, and then aadjust the parameters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AdjustParameters_Click(object sender, EventArgs e)
        {
            if (!flimage.flimage_io.tcspc_on)
                return;

            Button buttonObj = (Button)sender;
            buttonObj.Enabled = false;
            RGBPulseStart();
            System.Threading.Thread.Sleep(100);

            parameters = flimage.flimage_io.parameters;

            if (parameters.rateInfo.syncRate[0] > 1e6)
            {
                MeasureTagLensParameters();

                State.Acq.FastZ_Freq = 1000.0 * parameters.fastZScan.FrequencyKHz;
                CalculateFastZParameters();
            }

            PulseOff();
            buttonObj.Enabled = true;
        }

        /// <summary>
        /// When acquiring, the entire window will be diabled.
        /// </summary>
        /// <param name="running"></param>
        public void ControlsDuringScanning(bool running)
        {
            this.Enabled = !running;
        }

        /// <summary>
        /// Command called when Enable tag scan checkbox is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnableTagScan_Click(object sender, EventArgs e)
        {
            UpdateStateFromGUI(flimage);
            //flimage.PQ_SettingGUI(); Maybe necesary?
            parameters.fastZScan.phase_detection_mode = PhaseDetecCB.Checked;
            State.Acq.FastZ_phase_detection_mode = PhaseDetecCB.Checked;
            if (State.Acq.fastZScan)
                RGBPulseStart();
            else
                PulseOff();
        }

        /// <summary>
        /// Update State parameters from GUI. You can call from outside.
        /// </summary>
        /// <param name="FLIMage_in"></param>
        public void UpdateStateFromGUI(FLIMageMain FLIMage_main)
        {
            GetParamFromFLIMage(FLIMage_main); //this is to connect flimage, State etc.
            Int32 valI;
            double valD;
            State.Acq.fastZScan = EnableTagScan.Checked;
            parameters.enableFastZscan = State.Acq.fastZScan;
            parameters.fastZScan.phase_detection_mode = PhaseDetecCB.Checked;
            State.Acq.FastZ_phase_detection_mode = PhaseDetecCB.Checked;

            if (Int32.TryParse(NFastZSlices.Text, out valI)) State.Acq.FastZ_nSlices = valI;
            if (Double.TryParse(PhaseRangeStart.Text, out valD)) State.Acq.FastZ_PhaseRange[0] = valD;
            if (Double.TryParse(PhaseRangeEnd.Text, out valD)) State.Acq.FastZ_PhaseRange[1] = valD;
        }

        /// <summary>
        /// From State parameters to GUI.
        /// This can be called from other thread. 
        /// </summary>
        /// <param name="FLIMage_main"></param>
        public void DisplayTagLensParameters(FLIMageMain FLIMage_main)
        {
            GetParamFromFLIMage(FLIMage_main);

            if (this.InvokeRequired)
                this.BeginInvoke((Action)delegate
                {
                    DisplayTagLensParametersCore();
                });
            else
                DisplayTagLensParametersCore();
        }

        public void DisplayTagLensParametersCore()
        {

            FastZSliceStep.Text = String.Format("Slice step: {0:0.00} μm, {1:0.0} deg", State.Acq.FastZ_umPerSlice, State.Acq.FastZ_degreePerSlice);
            CountPerZScanEB.Text = parameters.fastZScan.CountPerFastZCycle.ToString();

            var res = parameters.fastZScan.CountPerFastZCycle % parameters.fastZScan.VoxelCount;
            if (!State.Acq.FastZ_phase_detection_mode)
                res = (parameters.fastZScan.phaseRangeCount[1] - parameters.fastZScan.phaseRangeCount[0]) % parameters.fastZScan.VoxelCount;

            ResidualEB.Text = res.ToString();
            Residual_deg.Text = String.Format("({0:0.0} deg)", (double)res / parameters.fastZScan.CountPerFastZCycle * 360.0);

            //UpdateState(flimage);
            int zScanPerLine = parameters.fastZScan.ZScanPerLine;
            ZScanPerLine.Text = zScanPerLine.ToString();

            NFastZSlices.Text = State.Acq.FastZ_nSlices.ToString();

            if (parameters.fastZScan.ZScanPerPixel > 0)
            {
                FreqKHz.Text = String.Format("{0:0.00}", parameters.fastZScan.FrequencyKHz);
                uint zScanPerPixel_Bd = parameters.fastZScan.ZScanPerPixel_Bidirecitonal;

                ZScanPerPixel.Text = zScanPerPixel_Bd.ToString();
                ZPixelsPerLine.Text = (zScanPerLine * 2 / zScanPerPixel_Bd).ToString();

                VoxelCount.Text = parameters.fastZScan.VoxelCount.ToString();
                VoxelTimeUS.Text = String.Format("{0:0.00}", parameters.fastZScan.VoxelTimeUs); //parameters.fastZScan.VoxelTimeUs.ToString();

                US_Per_ZScan.Text = String.Format("{0:0.00}", parameters.fastZScan.CountPerFastZCycle * parameters.spcData.time_per_unit * 1e6);
                FastZScanMsPerLine.Text = String.Format("{0:0.0}", State.Acq.FastZ_msPerLine);
            }
            else
                FreqKHz.Text = String.Format("{0:0.000}", 0);

            if (zScanPerLine > 100)
            {
                EnableTagScan.Enabled = true;
            }
            else
            {
                parameters.enableFastZscan = false;
                EnableTagScan.Enabled = false;
                EnableTagScan.Checked = false;
            }
        }

        /// <summary>
        /// Called by FastZ Slice editbox and Phase range start and end editbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Generic_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                UpdateStateFromGUI(flimage);
                CalculateFastZParameters();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }


        /// <summary>
        /// Calculate FastZ parameters from FastZ frequency. 
        /// </summary>
        public void CalculateFastZParameters()
        {
            GetParamFromFLIMage(flimage);
            FillGUI(); //not changing phase detection etc. 

            State.Acq.FastZ_phase_detection_mode = PhaseDetecCB.Checked;
            parameters.fastZScan.FrequencyKHz = State.Acq.FastZ_Freq / 1000.0;

            var ZScanPerLine = (uint)(State.Acq.msPerLine * parameters.fastZScan.FrequencyKHz * State.Acq.fillFraction + 0.5); //Full Scan.

            var ZScanPerPixel_BD = (uint)((double)ZScanPerLine * 2.0 / (double)State.Acq.pixelsPerLine + 0.5); //+0.5 == for Rounding.

            if (State.Acq.FastZ_phase_detection_mode)
                ZScanPerPixel_BD = 2 * (uint)((double)ZScanPerLine / (double)State.Acq.pixelsPerLine + 0.5);

            double ZScanTime_ms_BD = 1.0 / parameters.fastZScan.FrequencyKHz / 2.0; //milliseconds

            double msPerLine = (double)ZScanPerPixel_BD * ZScanTime_ms_BD * ((double)State.Acq.pixelsPerLine + 0.5) / State.Acq.fillFraction;
            msPerLine = Math.Round(msPerLine * 10.0) / 10.0;
            State.Acq.FastZ_msPerLine = msPerLine;

            for (int i = 0; i < 2; i++)
            {
                if (State.Acq.FastZ_PhaseRange[i] < 0)
                    State.Acq.FastZ_PhaseRange[i] = 0;

                if (State.Acq.FastZ_PhaseRange[i] > 180)
                    State.Acq.FastZ_PhaseRange[i] = 180;
            }

            if (State.Acq.FastZ_PhaseRange[0] > 90)
                State.Acq.FastZ_PhaseRange[0] = 90;

            if (State.Acq.FastZ_PhaseRange[1] < 91)
                State.Acq.FastZ_PhaseRange[1] = 91;

            parameters.fastZScan.phaseRange = (double[])State.Acq.FastZ_PhaseRange.Clone();


            if (msPerLine > State.Acq.msPerLine * 0.5 && msPerLine < State.Acq.msPerLine * 1.5)
            {
                parameters.fastZScan.nFastZSlices = State.Acq.FastZ_nSlices;
                parameters.fastZScan.phase_detection_mode = PhaseDetecCB.Checked;

                parameters.fastZScan.ZScanPerPixel_Bidirecitonal = ZScanPerPixel_BD;
                parameters.fastZScan.ZScanPerPixel = ZScanPerPixel_BD / 2.0f;


                double CountPerZScanD = 1.0 / State.Acq.FastZ_Freq / parameters.spcData.time_per_unit; //For 188K & 80MHz laser, it is 425 counts.

                for (int i = 0; i < 2; i++)
                    parameters.fastZScan.phaseRangeCount[i] = (uint)(CountPerZScanD * parameters.fastZScan.phaseRange[i] / 360.0);


                parameters.fastZScan.CountPerFastZCycle = (uint)CountPerZScanD;
                parameters.fastZScan.CountPerFastZCycleHalf = (uint)(CountPerZScanD / 2);

                parameters.fastZScan.ZScanPerLine = (int)(msPerLine * parameters.fastZScan.FrequencyKHz * State.Acq.fillFraction);

                int nSlices = State.Acq.FastZ_nSlices;


                //We need to make the integer Count works!! Especially for phase detection mode.
                if (State.Acq.FastZ_phase_detection_mode)
                {
                    parameters.fastZScan.VoxelTimeUs = 1000.0 / parameters.fastZScan.FrequencyKHz / nSlices;
                    parameters.fastZScan.VoxelCount = (int)(parameters.fastZScan.VoxelTimeUs / parameters.spcData.time_per_unit / 1e6);

                    nSlices = (int)parameters.fastZScan.CountPerFastZCycle / parameters.fastZScan.VoxelCount;

                    State.Acq.FastZ_nSlices = nSlices;
                    parameters.fastZScan.nFastZSlices = State.Acq.FastZ_nSlices;
                }
                else
                {
                    parameters.fastZScan.VoxelTimeUs = (1000.0 / parameters.fastZScan.FrequencyKHz) * (double)(parameters.fastZScan.phaseRangeCount[1] - parameters.fastZScan.phaseRangeCount[0]) / 360.0 / (double)nSlices;
                    parameters.fastZScan.VoxelCount = (int)(parameters.fastZScan.VoxelTimeUs / parameters.spcData.time_per_unit / 1e6);

                    //nSlices = (int)(parameters.fastZScan.phaseRangeCount[1] - parameters.fastZScan.phaseRangeCount[0]) / parameters.fastZScan.VoxelCount;

                    State.Acq.FastZ_nSlices = nSlices;
                    parameters.fastZScan.nFastZSlices = State.Acq.FastZ_nSlices;
                }

                double degPerSlice = (State.Acq.FastZ_PhaseRange[1] - State.Acq.FastZ_PhaseRange[0]) / State.Acq.FastZ_nSlices;
                if (State.Acq.FastZ_phase_detection_mode)
                    degPerSlice = 360.0 / (double)State.Acq.FastZ_nSlices;

                State.Acq.FastZ_degreePerSlice = degPerSlice;

                int indx = Preset_Pulldown.SelectedIndex;
                State.Acq.FastZ_umPerSlice = (indx >= 0) ? preset[indx][0] / 90 * degPerSlice : 1;


                uint nZlocs = (uint)State.Acq.FastZ_nSlices;
                if (parameters.fastZScan.phase_detection_mode)
                {
                    parameters.fastZScan.CountPerFastZSlice = parameters.fastZScan.CountPerFastZCycle / nZlocs;
                    parameters.fastZScan.residual_for_PhaseDetection = parameters.fastZScan.CountPerFastZCycle % nZlocs;
                }
                else
                {
                    parameters.fastZScan.CountPerFastZSlice = (parameters.fastZScan.phaseRangeCount[1] - parameters.fastZScan.phaseRangeCount[0]) / nZlocs;
                    parameters.fastZScan.residual_for_PhaseDetection = 0;
                }

                DisplayTagLensParameters(flimage);
            }
        }

        private bool ConnectDevice(String comport)
        {
            bool success = false;
            success = tag_command.Connect(comport);

            if (success)
            {
                comPort = comport;
                Text = "Tag Lens Control: Version " + tag_command.RGBVersion[0] + "." + tag_command.RGBVersion[1] + "." + tag_command.RGBVersion[2];
                SetFrequency_Pulldown.Items.Clear();
                for (int i = 0; i < tag_command.calib.defResAll.Count; i++)
                {
                    SetFrequency_Pulldown.Items.Add(tag_command.calib.defResAll[i]["Res.Freq(Hz)"]);
                }
                PulseOff();
                dataAcqTimer.Start();
                Connect_button.Text = "Disconnect";
            }
            else
            {
                SetFrequency_Pulldown.Items.Clear();
                SetFrequency_Pulldown.Items.Add("69000");
                SetFrequency_Pulldown.Items.Add("188000");
                SetFrequency_Pulldown.Items.Add("310000");
            }


            UpdateStateFromGUI(flimage);
            parameters.fastZScan.FrequencyKHz = State.Acq.FastZ_Freq / 1000.0;
            CalculateFastZParameters();

            return success;
        }

        private void DisConnect()
        {
            tag_command.CloseAll();
            Connect_button.Text = "Connect";

            dataAcqTimer.Stop();
        }

        private void Connect_button_Click(object sender, EventArgs e)
        {
            bool success = false;
            String com_port = comPort;
            if (com_port == "" && ComportPulldown.Items.Count > 0)
                com_port = ComportPulldown.SelectedItem.ToString();

            if (Connect_button.Text == "Connect")
            {
                success = ConnectDevice(com_port);
                if (success)
                {
                    comPort = com_port;
                    ComportPulldown.Visible = false;
                }
            }

            if (!success)
            {
                DisConnect();
            }

        }

        /// <summary>
        /// Communicate with tag_command to set oscillation amplitude (%)
        /// </summary>
        /// <param name="amp"></param>
        /// <returns></returns>
        private bool SetAmplitude(double amp)
        {
            double amp1 = tag_command.SetAmplitude_0to100(amp);
            if (amp1 < 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// RGB pulses are to synchronize TagLens.
        /// </summary>
        /// <returns></returns>
        public bool RGBPulseStart()
        {
            var phase = State.Acq.FastZ_Phase;

            for (int i = 0; i < phase.Length; i++)
            {
                if (phase[i] > 180)
                    phase[i] = 180;
                else if (phase[i] < -180)
                    phase[i] = -180;
            }
            double[] duration = new double[] { 100, 100, 100 };

            bool success = tag_command.SetRGBPhaseDuration(phase, duration, out double[] phaseArrayOut, out double[] DurationArrayOut);

            if (success)
            {
                //PhaseTextBox.Text = phaseArrayOut[0].ToString();
                int mode = tag_command.GetPulseMode();
                tag_command.GetRGBPhaseDuration(out double[] phaseOut, out double[] durationOut);
                StatusLabel.Text = "Mode = " + mode + ", Phase = " + phaseOut[0] + "," + phaseOut[1] + "," + phaseOut[2];
            }
            else
            {
                StatusLabel.Text = "RBG pulsing failed";
            }

            return success;
        }

        /// <summary>
        /// Power On. No return.
        /// </summary>
        public void PowerOn() => PowerOn(true);

        /// <summary>
        /// Power off. No return.
        /// </summary>
        public void PowerOff() => PowerOn(false);

        /// <summary>
        /// Power of Taglens on / off
        /// </summary>
        /// <param name="turn_on"></param>
        /// <returns></returns>
        public bool PowerOn(bool turn_on)
        {
            bool on = false;

            if (tag_command.IsOpen())
            {
                if (turn_on)
                    on = tag_command.SetPiezoOn();
                else
                    on = tag_command.SetPiezoOff();

                if (on)
                {
                    PowerButton.Text = "Abort";
                    SetAmplitude(State.Acq.FastZ_Amp);
                    tag_command.SetFrequency(State.Acq.FastZ_Freq);
                    LockResonance.Enabled = true;
                }
                else
                {
                    UnlockRes();
                    bool success = tag_command.SetPiezoOff();
                    PowerButton.Text = "Start";
                    LockResonance.Enabled = false;
                }
            }

            return on;
        }

        /// <summary>
        /// Turning off RGB pulses.
        /// </summary>
        private void PulseOff() => tag_command.PulseOff();

        private void PowerButton_Click(object sender, EventArgs e)
        {
            if (PowerButton.Text == "Start")
            {
                if (PowerOn(true))
                {
                    LockRes();
                }
            }
            else
                PowerOff();
        }

        /// <summary>
        /// Use timer to acquire Taglens parameters constantly. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataAcqTimer_Tick(object sender, EventArgs e)
        {
            if (tag_command.IsOpen())
            {
                acqData = tag_command.GetLensAFIVPPQ();
                AmplitudeLabel.Text = String.Format("{0:0.00}%", acqData[0]);
                Freq_Label.Text = acqData[1].ToString();
                RMSC_Label.Text = acqData[2].ToString();
                RMSV_Label.Text = acqData[3].ToString();
                LensPhase.Text = acqData[4].ToString();
                RealPowerMW.Text = acqData[5].ToString();
                ImgPowermVA.Text = acqData[6].ToString();

                tag_command.GetRGBPhaseDuration(out double[] phaseOut, out double[] durationOut);
                RGBPhase1.Text = String.Format("{0:0.00}", phaseOut[0]);
                RGBPhase2.Text = String.Format("{0:0.00}", phaseOut[1]);
                RGBPhase3.Text = String.Format("{0:0.00}", phaseOut[2]);

                if (lockStarted)
                {
                    int ret = tag_command.GetLockPhase();
                    if (ret == 1)
                    {
                        StatusLabel.Text = "Scanning";
                        LockResonance.Text = "Scanning: Press to cancel";
                        LockResonance.ForeColor = Color.Red;
                    }
                    else if (ret == 2)
                    {
                        StatusLabel.Text = "Scanning";
                        LockResonance.Text = "Scanning: Press to cancel";
                        LockResonance.ForeColor = Color.Red;
                    }
                    else if (ret == 3 || ret == 4)
                    {
                        StatusLabel.Text = "Locked - " + ret;
                        LockResonance.Text = "Locked: Press to unlock";
                        LockResonance.ForeColor = Color.Green;
                    }
                }
            }
        }

        /// <summary>
        /// Called by Keydown to Amp and Phase editobox in this window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenericEditBox_KeyDow(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ChangeParameters(sender);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Called by GenericEditBox_KeyDown. It handles text changes.
        /// </summary>
        /// <param name="sender"></param>
        private void ChangeParameters(object sender)
        {
            TextBox tb = (TextBox)sender;
            String SaveText = tb.Text;
            double value;

            if (Double.TryParse(SaveText, out value))
            {
                if (sender.Equals(AmplitudeEditBox))
                {
                    State.Acq.FastZ_Amp = value;
                    if (tag_command.IsOpen())
                        SetAmplitude(value);
                }
                else if (tb.Name.Contains("PhaseTextBox"))
                {
                    string tbText = tb.Name.ToString();
                    int n = Convert.ToInt32(tbText.Substring(tbText.Length - 1));
                    State.Acq.FastZ_Phase[n - 1] = value;
                    ChangePhaseAndRestart();
                }
            }//TryParse

            CalculateFastZParameters();
        }

        private void ChangePhaseAndRestart()
        {
            if (tag_command.IsOpen() && EnableTagScan.Checked)
            {
                tag_command.PulseOff();
                System.Threading.Thread.Sleep(100);

                if (!RGBPulseStart())
                    StatusLabel.Text = "RGB pulse failed!!";
            }
        }


        /// <summary>
        /// Called when value for SetFrequency pulldown menu is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetFrequency_Pulldown_SelectedIndexChanged(object sender, EventArgs e)
        {
            State.Acq.FastZ_Freq = Convert.ToDouble(SetFrequency_Pulldown.SelectedItem);

            CalculateFastZParameters();

            if (tag_command.IsOpen())
                tag_command.SetFrequency(State.Acq.FastZ_Freq);
        }

        /// <summary>
        /// Turn off/on buttons accordingly to the lock states.
        /// </summary>
        /// <param name="LockOn"></param>
        private void LockStatus(bool LockOn)
        {
            lockStarted = LockOn;
            AmplitudeEditBox.Enabled = !LockOn;
            SetFrequency_Pulldown.Enabled = !LockOn;
            Preset_Pulldown.Enabled = !LockOn;
            //PhaseTextBox1.Enabled = !LockOn;
            //PhaseTextBox2.Enabled = !LockOn;
            //PhaseTextBox3.Enabled = !LockOn;
        }

        /// <summary>
        /// Following two commands are to turn on/off lock resonance. Alias for LockResonanceNow() function.
        /// </summary>
        public void LockRes() => LockResonanceNow(true);
        public void UnlockRes() => LockResonanceNow(false);

        /// <summary>
        /// Comunicate with TagCommand class to lock/unlock resonance.
        /// </summary>
        /// <param name="turn_on">if true, it turns on. Otherwise, it turns off. </param>
        public void LockResonanceNow(bool turn_on)
        {
            if (tag_command.IsOpen())
            {
                int lockState;
                double max_freq, min_freq, max_amp, min_amp, amp_step, freq_step;
                double res_amp = State.Acq.FastZ_Amp;
                if (turn_on)
                {
                    for (int i = 0; i < tag_command.calib.defResAll.Count; i++)
                    {
                        double freq = tag_command.calib.defResAll[i]["Res.Freq(Hz)"];
                        if (freq == State.Acq.FastZ_Freq)
                        {
                            max_freq = tag_command.calib.defResAll[i]["Max.Freq(Hz)"];
                            min_freq = tag_command.calib.defResAll[i]["Min.Freq(Hz)"];
                            freq_step = tag_command.calib.defResAll[i]["Stp.Freq(Hz)"];
                            max_amp = tag_command.calib.defResAll[i]["Max.Amp(%)"];
                            min_amp = tag_command.calib.defResAll[i]["Min.Amp(%)"];
                            amp_step = tag_command.calib.defResAll[i]["Stp.Amp(%)"];
                            lockState = tag_command.EnableLockLens(res_amp, min_amp, max_amp, amp_step, min_freq, max_freq, freq_step);

                            //LockResonance.Text = "Unlock Resonance";
                            LockStatus(true);

                            break;
                        }
                    }

                }
                else
                {
                    LockStatus(false);
                    LockResonance.Text = "Press to Lock!";
                    LockResonance.ForeColor = Color.Red;
                    lockState = tag_command.DisableLock();
                }
            }
        }

        private void LockResonance_Click(object sender, EventArgs e)
        {
            if (LockResonance.Text == "Press to Lock!")
                LockRes();
            else
                UnlockRes();
        }

        public void PresetCalculator()
        {
            preset.Clear();
            //This is for 20x. [oscillation amp, freq, amp, phase, objective]
            preset.Add(0, new double[] { 10, 188000, 8, 15, 20 });
            preset.Add(1, new double[] { 20, 188000, 14.5, 15, 20 });
            preset.Add(2, new double[] { 30, 188000, 22, 17, 20 });
            preset.Add(3, new double[] { 40, 188000, 28, 20, 20 });
            preset.Add(4, new double[] { 60, 188000, 42, 20, 20 });
            preset.Add(5, new double[] { 80, 188000, 56, 24, 20 });
            preset.Add(6, new double[] { 100, 188000, 70, 29, 20 });

            int s = Preset_Pulldown.SelectedIndex;

            Preset_Pulldown.Items.Clear();
            for (int i = 0; i < preset.Count; i++)
            {
                preset[i][0] = preset[i][0] * Math.Pow(preset[i][4] / State.Acq.object_magnification, 2);
                double[] prst = preset[i];
                string str = String.Format("{0:0.0} μm: {1} Hz, {2:0.0}%, phase {3:0}", prst[0], prst[1], prst[2], prst[3]);
                Preset_Pulldown.Items.Add(str);
            }

            Preset_Pulldown.SelectedIndex = s;
        }

        private void Preset_Pulldown_SelectedIndexChanged(object sender, EventArgs e)
        {
            int indx = Preset_Pulldown.SelectedIndex;

            double freq = preset[indx][1];
            double amp = preset[indx][2];
            double phase = preset[indx][3];

            double oldfreq = State.Acq.FastZ_Freq;

            //To avoid wiping out meausred frequency, when it is similar frequency we will not change it.
            if (freq < oldfreq * 1.1 && freq >= oldfreq * 0.9)
                State.Acq.FastZ_Freq = freq;
            State.Acq.FastZ_Amp = amp;
            State.Acq.FastZ_Phase = new double[] { phase, phase, phase };

            ChangePhaseAndRestart();

            CalculateFastZParameters();
        }

        private void FastZControl_Shown(object sender, EventArgs e)
        {
            SetPreset_Pulldown();
            CalculateFastZParameters();
        }

        private void PhaseDetecCB_Click(object sender, EventArgs e)
        {
            State.Acq.FastZ_phase_detection_mode = PhaseDetecCB.Checked;
            CalculateFastZParameters();
        }
    }
}
