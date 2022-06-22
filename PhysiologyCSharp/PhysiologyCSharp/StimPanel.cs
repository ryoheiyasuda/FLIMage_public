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

namespace PhysiologyCSharp
{
    public partial class StimPanel : Form
    {
        public PhysParameters phys_parameters;
        public IOControls io_controls;
        public IOControls.MC700_Parameters mc700_params;
        public bool nidaq_on = false;
        string currentKey = "Patch1";
        public double[][] data_in;
        public double[] time;
        public bool image_trigger_waiting = false;
        public bool uncage_trigger_waiting = false;
        public bool acquire = true;

        public DateTime triggerTime;

        Timer StimTimer = new Timer();
        int stimCounter = 0;

        Stopwatch sw = new Stopwatch();
        Timer ElapsedTimer = new Timer();

        public PlotData plot_data;
        public PlotData scope_panel;
        private bool first_event_done = false;

        readonly bool FromFLIMage = false;

        PhysAnalysis phys_analysis;
        public bool stim_running= false;

        string FileExtension = ".txt";
        private WindowLocManager winManager;
        private String WindowName = "StimPanel";
        public String windowsInfoPath;

        public StimPanel(bool activatedByFLIMage)
        {
            InitializeComponent();

            FromFLIMage = activatedByFLIMage;
            LoadStimPanel();
        }

        private void LoadStimPanel()
        {
            Text = "Fisiology! + Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4);

            phys_parameters = new PhysParameters();

            try
            {
                io_controls = new IOControls(phys_parameters.initFolderPath, true);
                nidaq_on = true;
            }
            catch (Exception ex)
            {
                nidaq_on = false;
            }

            phys_parameters.nChannelsPatch = IOControls.nPatchChannels;
            phys_parameters.nChannelsStim = IOControls.nStimChannels;
            mc700_params = new IOControls.MC700_Parameters();

            plot_data = new PlotData(false, this);
            scope_panel = new PlotData(true, this);

            windowsInfoPath = Path.Combine(phys_parameters.initFolderPath, "WindowsInfo");

        }

        private void StimPanel_Load(object sender, EventArgs e)
        {
            winManager = new WindowLocManager(this, WindowName, windowsInfoPath);
            winManager.LoadWindowLocation(false);

            Patch1Radio.Checked = true;
            //phys_parameters.ReadParameters(phys_parameters.initFilePath);
            LoadValuesToPanel(true);

            ResizeFunc();
        }

        public void CloseCommand()
        {
            phys_parameters.SaveInitParameters();
            winManager.SaveWindowLocation();
            if (phys_analysis != null && phys_analysis.Visible)
                phys_analysis.SaveWindowLoc();

            if (plot_data != null && plot_data.Visible)
                plot_data.SaveWindowLoc();

            if (scope_panel != null && scope_panel.Visible)
                scope_panel.SaveWindowLoc();
        }

        private void StimPanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseCommand();
            scope_panel.Hide();
            plot_data.Hide();
            Hide();

            if (FromFLIMage)
                e.Cancel = true;
        }

        private void GenericParameterFieldKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetValueFromPanel(true);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        public void SetValueFromPanel(bool confirm)
        {
            var pulse = phys_parameters.PulseSet[currentKey];
            Int32.TryParse(PulseN.Text, out pulse.Num);
            double.TryParse(PulseAmp.Text, out pulse.Amp);
            double.TryParse(PulseWidth.Text, out pulse.Width_ms);
            double.TryParse(PulseDelay.Text, out pulse.Delay_ms);
            double.TryParse(PulseInterval.Text, out pulse.Interval_ms);
            double.TryParse(TotalLength.Text, out phys_parameters.pulseSetTotalLength_ms);
            double.TryParse(OutputRate.Text, out phys_parameters.outputRate);
            if (PulseSet_Repeat.Text != "NA")
                Int32.TryParse(PulseSet_Repeat.Text, out phys_parameters.pulse_set_repeat);
            if (PulseSet_Interval.Text != "NA")
                Int32.TryParse(PulseSet_Interval.Text, out phys_parameters.pulse_set_interval);
            phys_parameters.sync_with_image = SyncWithImage.Checked;
            phys_parameters.sync_with_uncage = SyncWithUncageCheck.Checked;
            phys_parameters.acquire_data = AcqDataCheck.Checked;
            phys_parameters.PulseName = PulseName.Text;

            if (Cycle.Text == "")
            {
                phys_parameters.cycle = null;
            }
            else
            {
                string[] sP = Cycle.Text.Split(',');
                phys_parameters.cycle = new int[sP.Length];
                int nCycle = 0;
                for (int i = 0; i < sP.Length; i++)
                {
                    int val;
                    if (Int32.TryParse(sP[i], out val))
                    {
                        phys_parameters.cycle[i] = val;
                        nCycle++;
                    }
                }
                if (nCycle == 0)
                    phys_parameters.cycle = null;
            }

            if (confirm)
                LoadValuesToPanel(true);
        }


        public void LoadValuesToPanel(bool change_all)
        {
            LoadValuesToPanel(change_all, true);
        }

        public void LoadValuesToPanel(bool change_all, bool acquisition)
        {
            var pulse = phys_parameters.PulseSet[currentKey];
            PulseN.Text = pulse.Num.ToString();
            PulseAmp.Text = pulse.Amp.ToString();
            PulseWidth.Text = pulse.Width_ms.ToString();
            PulseDelay.Text = pulse.Delay_ms.ToString();
            PulseInterval.Text = pulse.Interval_ms.ToString();

            PulseName.Text = phys_parameters.PulseName;
            TotalLength.Text = phys_parameters.pulseSetTotalLength_ms.ToString();

            if (change_all)
            {
                if (phys_parameters.outputRate == 0)
                    phys_parameters.outputRate = 12000;
                OutputRate.Text = phys_parameters.outputRate.ToString();
                PulseSet_Repeat.Text = phys_parameters.pulse_set_repeat.ToString();
                PulseSet_Interval.Text = phys_parameters.pulse_set_interval.ToString();
                SyncWithImage.Checked = phys_parameters.sync_with_image;
                SyncWithUncageCheck.Checked = phys_parameters.sync_with_uncage;
                AcqDataCheck.Checked = phys_parameters.acquire_data;
                if (phys_parameters.cycle == null)
                    Cycle.Text = "";
                else
                    Cycle.Text = String.Join(",", phys_parameters.cycle);
            }
            else
            {
                SetValueFromPanel(false);
            }

            data_in = new double[IOControls.nPatchChannels + IOControls.nStimChannels][];
            for (int i = 0; i < data_in.Length; i++)
            {
                if (i < IOControls.nPatchChannels)
                    phys_parameters.mkPulse("Patch" + (i + 1).ToString(), out data_in[i], out time);
                else
                    phys_parameters.mkPulse("Stim" + (i - IOControls.nPatchChannels + 1).ToString(), out data_in[i], out time);
            }

            if (acquisition)
            {
                phys_parameters.nChannelsPatch = IOControls.nPatchChannels;
                phys_parameters.nChannelsStim = IOControls.nStimChannels;
            }

            Patch2Radio.Visible = phys_parameters.nChannelsPatch >= 2;
            Stim2Radio.Visible = phys_parameters.nChannelsStim >= 2;
            Patch2Label.Visible = phys_parameters.nChannelsPatch >= 2;
            Stim2Label.Visible = phys_parameters.nChannelsStim >= 2;

            StimPlot.Invalidate();
            PatchPlot.Invalidate();

            Turn_Parameters_for_Sync_with_FLIMage();

            PulseNumber.Value = phys_parameters.currentPulseN;
        }

        private void PatchStimRadio_CheckedChanged(object sender, EventArgs e)
        {
            var radio = (RadioButton)sender;
            if (radio.Checked)
                currentKey = radio.Text;
            LoadValuesToPanel(true);
        }

        private void AcqDataCheck_CheckedChanged(object sender, EventArgs e)
        {
            SetValueFromPanel(true);
        }

        private void SyncWithImage_CheckedChanged(object sender, EventArgs e)
        {
            SetValueFromPanel(true);
        }

        private void Turn_Parameters_for_Sync_with_FLIMage()
        {
            bool sync1 = phys_parameters.sync_with_image || phys_parameters.sync_with_uncage;
            PulseSet_Interval.Enabled = !sync1;
            PulseSet_Repeat.Enabled = !sync1;
            elaspedTimeLabel.Visible = !sync1;
            if (sync1)
            {
                PulseSet_Interval.Text = "NA";
                PulseSet_Repeat.Text = "NA";
            }
        }

        private void PulseNumber_ValueChanged(object sender, EventArgs e)
        {
            int newN = (int)PulseNumber.Value;
            phys_parameters.ReadParametersByNumber(newN);
            phys_parameters.currentPulseN = newN;

            LoadValuesToPanel(false);
        }

        private void PatchPlot_Paint(object sender, PaintEventArgs e)
        {
            var sdr = (PictureBox)sender;
            var pp = new PlotOnPanel(sdr.Width, sdr.Height);
            string plotKey;
            if (sdr.Name.Contains("Patch"))
                plotKey = "Patch";
            else
                plotKey = "Stim";

            if (data_in != null)
                if (plotKey == "Patch")
                {
                    pp.addData(time, data_in[0], "-r");
                    if (IOControls.nPatchChannels > 1)
                        pp.addData(time, data_in[1], "-b");
                }
                else
                {
                    pp.addData(time, data_in[IOControls.nPatchChannels], "-r");
                    if (IOControls.nStimChannels > 1)
                        pp.addData(time, data_in[IOControls.nPatchChannels + 1], "-b");
                }


            pp.XTitle = "Time (ms)";
            pp.YTitle = "Amp (mV/pA)";
            pp.autoAxisPosition(e);
            pp.plot(e);
        }


        public void DataOutDoneHandlerFcn(object sender, EventArgs e)
        {
            StartButton.Enabled = true;
        }

        public void AcquiredDoneHandlerFcn(object sender, EventArgs e)
        {
            io_controls.GetGain();
            var nSamples = io_controls.dataOutput[0].Length;
            Double[] t = Enumerable.Range(0, nSamples).Select(x => (double)x * 1000.0 / phys_parameters.outputRate).ToArray();

            if (plot_data == null || plot_data.IsDisposed)
                plot_data = new PlotData(false, this);

            var fields = io_controls.mc700_params.GetType().GetFields();
            foreach (var field in fields)
            {
                var val = field.GetValue(io_controls.mc700_params);
                var name = field.Name;
                mc700_params.GetType().GetField(name).SetValue(mc700_params, val);
            }

            triggerTime = io_controls.triggerTime;

            plot_data.InvokeIfRequired(o => o.AcquiredDataPlotAndSave(t, io_controls.dataOutput, io_controls.mc700_params));
            plot_data.InvokeIfRequired(o => o.Show());

            StartButton.InvokeIfRequired(o => o.Enabled = true);
        }

        public void StartAcq()
        {
            int ret = io_controls.PutValues(data_in, phys_parameters.outputRate, phys_parameters.acquire_data);
            if (ret < 0)
                StopRepeat();

            if (first_event_done)
            {
                io_controls.AcqDone -= AcquiredDoneHandlerFcn;
                io_controls.DataOutDone -= DataOutDoneHandlerFcn;
            }

            first_event_done = true;
            io_controls.AcqDone += new IOControls.AcqDoneHandler(AcquiredDoneHandlerFcn);
            io_controls.DataOutDone += new IOControls.DataOutDoneHandler(DataOutDoneHandlerFcn);

            bool ext = phys_parameters.sync_with_image || phys_parameters.sync_with_uncage;
            io_controls.Start(ext, phys_parameters.acquire_data);
            stimCounter++;
            RepeatProgress.InvokeIfRequired(o => o.Text = stimCounter + "/" + phys_parameters.pulse_set_repeat);            
        }

        void StimTimerEvent(Object myObject, EventArgs myEventArgs)
        {
            StartAcq();
            bool sync1 = phys_parameters.sync_with_image || phys_parameters.sync_with_uncage;
            if (stimCounter == phys_parameters.pulse_set_repeat && !sync1)
            {
                StopRepeat();
            }
        }

        public void StopRepeat()
        {
            if (nidaq_on)
                io_controls.Stop();

            if (StimTimer != null)
            {
                StimTimer.Stop();
                StimTimer.Dispose();
            }
            if (ElapsedTimer != null)
            {
                ElapsedTimer.Stop();
                ElapsedTimer.Dispose();
            }
            StartButton.Text = "Start";
            StartButton.Enabled = true;
            image_trigger_waiting = false;
            stim_running = false;
            sw.Reset();
        }

        private void ElapsedTimerEvent(object sender, EventArgs e)
        {
            elaspedTimeLabel.BeginInvokeIfRequired(o => o.Text = (sw.ElapsedMilliseconds / 1000).ToString() + " s");
        }

        public void StartButton_Click(object sender, EventArgs e)
        {
            if (!nidaq_on)
            {
                MessageBox.Show("NIDAQ and/or MC700B Controller is not working. Check init file.");
                StopRepeat();
                return;
            }

            if (!Directory.Exists(plot_data.FolderPathName))
            {
                MessageBox.Show("Set save folder and basename from file menu");
                StopRepeat();
                return;
            }

            StartButton.Enabled = false;
            if (StartButton.Text == "Start" && nidaq_on)
            {
                if (scope_panel != null && !scope_panel.IsDisposed)
                {
                    scope_panel.StopScope();
                }

                StartButton.Text = "Stop";
                stimCounter = 0;

                bool sync1 = phys_parameters.sync_with_image || phys_parameters.sync_with_uncage;
                if (!sync1) //otherwise it will be loaded from FLIMage.
                {
                    if (phys_parameters.pulse_set_repeat > 1)
                    {
                        StimTimer = new Timer();
                        StimTimer.Tick += new EventHandler(StimTimerEvent);
                        StimTimer.Interval = (int)(phys_parameters.pulse_set_interval * 1000);
                        StimTimer.Start();
                    }

                    StartAcq();

                    if (phys_parameters.pulse_set_repeat <= 1)
                    {
                        StartButton.Text = "Start";
                        phys_parameters.pulse_set_repeat = 1;
                    }

                    ElapsedTimer = new Timer();
                    ElapsedTimer.Tick += new EventHandler(ElapsedTimerEvent);
                    ElapsedTimer.Interval = 1;
                    ElapsedTimer.Start();
                    sw.Restart();

                    stim_running = true;
                }
                else //sync with image.
                {
                    image_trigger_waiting = phys_parameters.sync_with_image;
                    uncage_trigger_waiting = phys_parameters.sync_with_uncage;
                }
            }
            else
            {
                StopRepeat();
            }

            StartButton.Enabled = true;
        }

        private void mC700BTestPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (io_controls != null)
            {
                var mc = new MC700BCommander(io_controls.mc700_commander);
                mc.Show();
            }
            else
            {
                MessageBox.Show("MC700Commander is not active");
            }
        }

        private void scopeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scope_panel = new PlotData(true, this);
            scope_panel.Show();
        }


        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = FileIO.AskOpenFileName(FileExtension);
            if (filename != "")
            {
                if (plot_data == null || plot_data.IsDisposed)
                {
                    plot_data = new PlotData(false, this);
                }
                plot_data.OpenFile(filename);
                plot_data.Show();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (plot_data == null || plot_data.data == null || plot_data.data.Length < 1)
            {
                MessageBox.Show("No data to save");
                return;
            }

            string filename = FileIO.AskSaveFileName("PlotData", FileExtension);
            if (filename != "")
            {
                plot_data.SaveFile(filename);
            }
        }

        private void dataWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (plot_data == null || plot_data.IsDisposed)
                plot_data = new PlotData(false, this);

            plot_data.Show();
        }

        private void setSaveFolderAndBaseNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (plot_data == null || plot_data.IsDisposed)
                plot_data = new PlotData(false, this);

            string filename = plot_data.SetSaveFolderAndBaseName();
            if (filename != "")
            {
                plot_data.Show();

            }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadStimPanel();
        }

        private void EpochChanged()
        {
            if (phys_parameters.epoch < 1)
                phys_parameters.epoch = 1;

            LoadValuesToPanel(false);

            if (plot_data != null)
                plot_data.LoadEpochFile();
        }

        private void analysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            phys_analysis = new PhysAnalysis(this);
            phys_analysis.Show();
        }

        private void StimPanel_Resize(object sender, EventArgs e)
        {
            ResizeFunc();
        }

        private void ResizeFunc()
        {
            var top1 = Patch1Radio.Top;
            var bottom1 = Height;
            var panel_height = (bottom1 - top1) / 2 - (2 * Patch1Radio.Height);
            PatchPlot.Top = top1 + Stim1Radio.Height;
            PatchPlot.Height = panel_height;

            Stim1Radio.Top = panel_height + top1 + Stim1Radio.Height;
            Stim2Radio.Top = panel_height + top1 + Stim1Radio.Height;
            Stim1Label.Top = Stim1Radio.Top - 5;
            Stim2Label.Top = Stim1Radio.Top - 5;

            StimPlot.Top = top1 + panel_height + 2 * Stim1Radio.Height;
            StimPlot.Height = panel_height;
            StimPlot.Invalidate();
            PatchPlot.Invalidate();
        }

        private void StimPanel_Shown(object sender, EventArgs e)
        {
            winManager.LoadWindowLocation(false);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SetValueFromPanel(true);
            phys_parameters.SaveInitParameters();
            phys_parameters.SaveParametersByNumber(phys_parameters.currentPulseN); //Save in default file.
        }

        private void SyncWithUncageCheck_Click(object sender, EventArgs e)
        {
            if (sender.Equals(SyncWithUncageCheck))
            {
                if (SyncWithUncageCheck.Checked)
                    SyncWithImage.Checked = false;
            }

            if (sender.Equals(SyncWithImage))
            {
                if (SyncWithImage.Checked)
                    SyncWithUncageCheck.Checked = false;
            }
            SetValueFromPanel(true);
        }
    }
}
