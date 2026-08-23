using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FLIMage.HardwareControls
{
    public partial class NIDAQ_Config : Form
    {
        ScanParameters State;
        FLIMageMain flimage;
        String String_Pockel = " (Same as pockels cell board)";
        String String_Mirror = " (Same as mirror board)";


        public NIDAQ_Config(FLIMageMain fc)
        {
            State = fc.State;
            flimage = fc;
            InitializeComponent();
        }

        private void NIDAQ_Config_Load(object sender, EventArgs e)
        {
            setupGUI();
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            setValueToState();
            setupGUI();

            System.IO.File.WriteAllText(State.Files.deviceFileName, flimage.fileIO.AllSetupValues_device());

            DialogResult dr = MessageBox.Show("You need to restart the application. OK?", "Restart warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                Application.Restart();
                Environment.Exit(0);
            }
        }

        private void setValueToState()
        {
            State.Init.mirrorAOPortX = MirrorBoard.Text + "/" + MirrorX.Text;
            State.Init.mirrorAOPortY = MirrorBoard.Text + "/" + MirrorY.Text;
            State.Init.lineClockPort = MirrorBoard.Text + "/" + LineClock_Output.Text;
            State.Init.TriggerInput = TriggerInput.Text;
            State.Init.SampleClockPort = SampleClkPort.Text;

            State.Init.UseExternalMirrorOffset = UseAOMirrorOffset.Checked;

            State.Init.mirrorOffsetX = MirrorOffsetBoard.Text + "/" + MirrorOffsetX.Text;
            State.Init.mirrorOffsetY = MirrorOffsetBoard.Text + "/" + MirrorOffsetY.Text;

            State.Init.EOM_nChannels = Convert.ToInt32(Pockels_NChannels.Text);
            if (State.Init.EOM_nChannels > 4)
                State.Init.EOM_nChannels = 4;

            for (int i = 0; i < State.Init.EOM_nChannels; i++)
            {
                Control[] found = this.Controls.Find("PockelsChannel" + (i + 1), true);
                String AOText = PockelsBoard.Text + "/" + found[0].Text;
                State.Init.GetType().GetField("EOM_Port" + i).SetValue(State.Init, AOText);
            }


            for (int i = 0; i < State.Init.EOM_nChannels; i++)
            {
                Control[] found = this.Controls.Find("AI" + (i + 1), true);
                String AOText = AIBoard.Text + "/" + found[0].Text;
                State.Init.GetType().GetField("EOM_AI_Port" + i).SetValue(State.Init, AOText);
            }


            State.Init.AO_uncagingShutter = AO_uncagingShutterCheck.Checked;
            State.Init.DO_uncagingShutter = DO_uncagingShutterCheck.Checked;

            State.Init.UncagingShutterAnalogPort = PockelsBoard.Text + "/" + Shutter_AOChannel.Text;

            State.Init.DigitalShutterPort = Shutter_DOChannel.Text;

            State.Init.SampleClockPort = SampleClkPort.Text;
            State.Init.TriggerInput = TriggerInput.Text;

            State.Init.shutterPort = ShutterOutputBoard.Text + "/Port0/" + ShutterOutputChannel.Text;
            State.Init.triggerPort = TriggerOutputBoard.Text + "/Port0/" + TriggerOutputChannel.Text;

            State.Init.ResonantOn = ResonantBoard_TB.Text + "/Port0/" + Resonant_ON_TB.Text;
            State.Init.ResonantSwitchPort = ResonantBoard_TB.Text + "/Port0/" + ResonantSwitch_TB.Text;
            State.Init.ResonantAOBoard = ResonantBoard_TB.Text;
            State.Init.ResonantZoom = ResonantBoard_TB.Text + "/" + Resonant_Amp_TB.Text;
            State.Init.ResonantMirrorX = ResonantBoard_TB.Text + "/" + Resonant_X_TB.Text;
            State.Init.ResonantMirrorY = ResonantBoard_TB.Text + "/" + Resonant_Y_TB.Text;
            State.Init.ResonantClockInput_fromScanner = Resonant_LineClockInput_TB.Text;

            State.Init.MarkerInput = MarkerInput.Text;
            State.Init.ExternalTriggerInputPort = ExternalTriggerChannel.Text;

            State.Init.MotorComPort = MotorCOM.Text;
            State.Init.openShutterDuringCalibration = openDuringCalib.Checked;

            State.Init.ComputerID = Convert.ToInt32(ComputerID.Text);
            State.Init.FLIMserial = Convert.ToInt32(FlimID.Text);

            State.Init.FLIM_on = FLIM_onCheck.Checked;
            State.Init.motor_on = Motor_onCheck.Checked;

            if (BH_radio.Checked)
                State.Init.FLIM_mode = "BH";
            else if (MH_radio.Checked)
                State.Init.FLIM_mode = "MH";
            else if (PH_Radio.Checked)
                State.Init.FLIM_mode = "PH";
            else if (HH_Radio.Checked)
                State.Init.FLIM_mode = "HH";
            else
                State.Init.FLIM_mode = "PQ";

            if (Custom_Resonant_Radio.Checked || Custom_Radio.Checked || Polygon_Radio.Checked)
            {
                State.Init.MicroscopeSystem = "";
                State.Init.enableRegularGalvo = true;
                State.Init.enableResonantScanner = Custom_Resonant_Radio.Checked;
                if (Polygon_Radio.Checked)
                    State.Init.MicroscopeSystem = "Polygon";
            }
            else if (MiniscopeRadio.Checked)
            {
                State.Init.MicroscopeSystem = "MiniScope";
                State.Init.enableRegularGalvo = true;
                State.Init.enableResonantScanner = false;
            }
            else if (BScopeGG_Radio.Checked)
            {
                State.Init.MicroscopeSystem = "Thorlab BScope GG";
                State.Init.enableRegularGalvo = true;
                State.Init.enableResonantScanner = false;
            }
            else if (BScope_RG_Radio.Checked)
            {
                State.Init.MicroscopeSystem = "Thorlab BScope RG";
                State.Init.enableRegularGalvo = true;
                State.Init.enableResonantScanner = true;
            }

            State.Init.resonantScanner_COMPort = Thor_ECU_COM.Text;
            if (State.Init.resonantScanner_COMPort.Contains("COM"))
            {
                if (State.Init.MicroscopeSystem.ToLower().Contains("bscope"))
                    State.Init.resonantScannerSystem = "BScope";
                else
                    State.Init.resonantScannerSystem = "ThorECU";
            }

            State.Init.PMTModule_COMPort = Thor_PMT_COM.Text;
            if (State.Init.PMTModule_COMPort.Contains("COM"))
            {
                    State.Init.resonantScannerSystem = "ThorECU";
            }

        }

        private void setupGUI()
        {
            String[] sP;
            sP = State.Init.mirrorAOPortX.Split('/');
            MirrorBoard.Text = sP[0];
            MirrorX.Text = sP[1];

            sP = State.Init.mirrorAOPortY.Split('/');
            MirrorY.Text = sP[1];


            sP = State.Init.lineClockPort.Split('/');
            LineClock_Output.Text = sP[1];

            //
            sP = State.Init.mirrorOffsetX.Split('/');
            MirrorOffsetBoard.Text = sP[0];
            MirrorOffsetX.Text = sP[1];

            sP = State.Init.mirrorOffsetY.Split('/');
            MirrorOffsetY.Text = sP[1];
            //

            MirrorOffsetBoard.Enabled = State.Init.UseExternalMirrorOffset;
            MirrorOffsetX.Enabled = State.Init.UseExternalMirrorOffset;
            MirrorOffsetY.Enabled = State.Init.UseExternalMirrorOffset;

            Pockels_NChannels.Text = State.Init.EOM_nChannels.ToString();

            for (int i = 0; i < 4; i++)
            {
                Control[] found = this.Controls.Find("PockelsChannel" + (i + 1), true);
                String s = State.Init.GetType().GetField("EOM_Port" + i).GetValue(State.Init).ToString();
                sP = s.Split('/');
                if (i == 0)
                    PockelsBoard.Text = sP[0];

                found[0].Text = sP[1];
                found[0].Enabled = i < State.Init.EOM_nChannels;
            }

            for (int i = 0; i < 4; i++)
            {
                Control[] found = this.Controls.Find("AI" + (i + 1), true);
                String s = State.Init.GetType().GetField("EOM_AI_Port" + i).GetValue(State.Init).ToString();
                sP = s.Split('/');
                if (i == 0)
                    AIBoard.Text = sP[0];

                found[0].Text = sP[1];
                found[0].Enabled = i < State.Init.EOM_nChannels;
            }

            AO_uncagingShutterCheck.Checked = State.Init.AO_uncagingShutter || State.Init.DO_uncagingShutter;

            DO_uncagingShutterCheck.Checked = State.Init.DO_uncagingShutter;
            AO_uncagingShutterCheck.Checked = State.Init.AO_uncagingShutter;

            UseAOMirrorOffset.Checked = State.Init.UseExternalMirrorOffset;

            sP = State.Init.UncagingShutterAnalogPort.Split('/');
            UncagingAOBoard.Text = PockelsBoard.Text + String_Pockel;
            Shutter_AOChannel.Text = sP[1];
            Shutter_AOChannel.Enabled = State.Init.AO_uncagingShutter;

            Shutter_DOBoard.Text = State.Init.MirrorAOBoard;
            Shutter_DOBoard.ReadOnly = true;
            Shutter_DOChannel.Text = State.Init.DigitalShutterPort;
            Shutter_DOChannel.Enabled = State.Init.DO_uncagingShutter;

            SampleClkPort.Text = State.Init.SampleClockPort;
            TriggerInput.Text = State.Init.TriggerInput;

            sP = State.Init.shutterPort.Split('/');
            ShutterOutputBoard.Text = sP[0];
            ShutterOutputChannel.Text = sP[2];

            sP = State.Init.triggerPort.Split('/');
            TriggerOutputBoard.Text = sP[0];
            TriggerOutputChannel.Text = sP[2];

            ResonantBoard_TB.Text = State.Init.ResonantAOBoard;
            Resonant_X_TB.Text = State.Init.ResonantMirrorX.Split('/')[1];
            Resonant_Y_TB.Text = State.Init.ResonantMirrorY.Split('/')[1];
            Resonant_Amp_TB.Text = State.Init.ResonantZoom.Split('/')[1];
            Resonant_ON_TB.Text = State.Init.ResonantOn.Split('/')[2];
            ResonantSwitch_TB.Text = State.Init.ResonantSwitchPort;
            Resonant_LineClockInput_TB.Text = State.Init.ResonantClockInput_fromScanner;

            MarkerInput.Text = State.Init.MarkerInput;

            ExternalTriggerChannel.Text = State.Init.ExternalTriggerInputPort;

            MotorCOM.Text = State.Init.MotorComPort;

            openDuringCalib.Checked = State.Init.openShutterDuringCalibration;

            ComputerID.Text = State.Init.ComputerID.ToString();
            FlimID.Text = State.Init.FLIMserial.ToString();

            BH_radio.Checked = State.Init.FLIM_mode == "BH";
            PQ_radio.Checked = State.Init.FLIM_mode == "PQ";
            MH_radio.Checked = State.Init.FLIM_mode == "MH";
            PH_Radio.Checked = State.Init.FLIM_mode == "PH";
            HH_Radio.Checked = State.Init.FLIM_mode == "HH";

            ThorBScope_Radio.Checked = State.Init.MotorHWName == "ThorBScope";
            Thorlab_MCM301_Radio.Checked = State.Init.MotorHWName == "ThorMCM301";
            Thorlab_MCM5000_Radio.Checked = State.Init.MotorHWName == "ThorMCM5000";
            Thorlab_MCM3000_Radio.Checked = State.Init.MotorHWName == "ThorMCM3000";
            Sutter_MP285_Radio.Checked = State.Init.MotorHWName == "MP-285";
            Sutter_MP285A_Radio.Checked = State.Init.MotorHWName == "MP-285A";

            BScopeGG_Radio.Checked = State.Init.MicroscopeSystem.ToLower().Contains("bscope") && State.Init.MicroscopeSystem.ToLower().Contains("gg");
            BScope_RG_Radio.Checked = State.Init.MicroscopeSystem.ToLower().Contains("bscope") && State.Init.MicroscopeSystem.ToLower().Contains("rg");
            MiniscopeRadio.Checked = State.Init.MicroscopeSystem.ToLower().Contains("mini");
            Custom_Radio.Checked = State.Init.MicroscopeSystem == "" && !State.Init.enableResonantScanner;
            Custom_Resonant_Radio.Checked = State.Init.MicroscopeSystem == "" && State.Init.enableResonantScanner;
            Polygon_Radio.Checked = State.Init.MicroscopeSystem.ToLower().Contains("polygon");

            Thor_ECU_COM.Text = State.Init.resonantScanner_COMPort;
            Thor_PMT_COM.Text = State.Init.PMTModule_COMPort;

            Motor_onCheck.Checked = State.Init.motor_on;
            FLIM_onCheck.Checked = State.Init.FLIM_on;
        }

        private void NIDAQ_Config_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();

            flimage.ToolWindowClosed();
        }

        private void NIDAQ_Config_Shown(object sender, EventArgs e)
        {
            setupGUI();
        }

        private void Generic_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void UseAOMirrorOffset_Click(object sender, EventArgs e)
        {
            State.Init.UseExternalMirrorOffset = UseAOMirrorOffset.Checked;
            MirrorOffsetBoard.Enabled = State.Init.UseExternalMirrorOffset;
            MirrorOffsetX.Enabled = State.Init.UseExternalMirrorOffset;
            MirrorOffsetY.Enabled = State.Init.UseExternalMirrorOffset;
        }

        private void AO_uncagingShutter_Click(object sender, EventArgs e)
        {
            Shutter_AOChannel.Enabled = AO_uncagingShutterCheck.Checked;
            Shutter_DOChannel.Enabled = DO_uncagingShutterCheck.Checked;
            //setupGUI();
        }


        private void Pockels_NChannels_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tb = (TextBox)sender;
                String SaveText = tb.Text;
                try
                {
                    int ValI;
                    if (!Int32.TryParse(Pockels_NChannels.Text, out ValI)) ValI = State.Init.EOM_nChannels;
                    if (ValI > 4)
                        ValI = 4;

                    for (int i = 0; i < 4; i++)
                    {
                        Control[] found = this.Controls.Find("PockelsChannel" + (i + 1), true);
                        found[0].Enabled = i < ValI;
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        Control[] found = this.Controls.Find("AI" + (i + 1), true);
                        found[0].Enabled = i < ValI;
                    }

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

        private void Thorlab_MCM_CheckedChanged(object sender, EventArgs e)
        {
            if (Thorlab_MCM3000_Radio.Checked)
            {
                MotorCOM.Text = "COM32";
                MotorCOM.ReadOnly = true;
            }
            else if (Thorlab_MCM5000_Radio.Checked || ThorBScope_Radio.Checked)
            {
                MotorCOM.Text = "COM31";
                MotorCOM.ReadOnly = true;
            }
            else if (Thorlab_MCM301_Radio.Checked)
            {
                MotorCOM.Text = "COM32"; // Not used yet
                MotorCOM.ReadOnly = true;
            }
            else
            {
                MotorCOM.Text = State.Init.MotorComPort;
                MotorCOM.ReadOnly = false;

            }
        }
    }
}
