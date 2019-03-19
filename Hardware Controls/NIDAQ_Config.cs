﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FLIMimage
{
    public partial class NIDAQ_Config : Form
    {
        ScanParameters State;
        FLIMageMain FLIMage;
        String String_Pockel = " (Same as pockels cell board)";
        String String_Mirror = " (Same as mirror board)";


        public NIDAQ_Config(FLIMageMain fc)
        {
            State = fc.State;
            FLIMage = fc;
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

            System.IO.File.WriteAllText(State.Files.deviceFileName, FLIMage.fileIO.AllSetupValues_device());

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
            State.Init.mirrorAOTrigger = "/" + MirrorBoard.Text + "/" + Mirror_TriggerInput.Text;
            State.Init.lineClockPort = MirrorBoard.Text + "/" + LineClock_Output.Text;
            State.Init.lineClockTrigger = "/" + MirrorBoard.Text + "/" + Mirror_TriggerInput.Text;
            State.Init.mirrorAOSampleClockOutput = "/" + MirrorBoard.Text + "/" + Mirror_SampleClkOutput.Text;

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

            State.Init.EOM_AI_Trigger = "/" + AIBoard.Text + "/" + Pockels_TriggerIn.Text;

            State.Init.AO_uncagingShutter = AO_uncagingShutterCheck.Checked;
            State.Init.DO_uncagingShutter = DO_uncagingShutterCheck.Checked;
            State.Init.DO_uncagingShutter_useForPMTsignal = ActiveLow.Checked;

            State.Init.UncagingShutterAnalogPort = PockelsBoard.Text + "/" + Shutter_AOChannel.Text;
            
            State.Init.UncagingShutterDOPort = MirrorBoard.Text + "/Port0/" + Shutter_DOChannel.Text;
            State.Init.UncagingShutterTrigger = "/" + MirrorBoard.Text + "/" + Mirror_TriggerInput.Text; //This is for Digital signal!

            State.Init.EOM_SampleClockInput = "/" + PockelsBoard.Text + "/" + Pockels_SampleClkInput.Text;
            State.Init.EOM_Trigger = "/" + PockelsBoard.Text + "/" + Pockels_TriggerIn.Text;

            State.Init.shutterPort = ShutterOutputBoard.Text + "/Port0/" + ShutterOutputChannel.Text;
            State.Init.triggerPort = TriggerOutputBoard.Text + "/Port0/" + TriggerOutputChannel.Text;

            State.Init.MarkerInput = MarkerInput.Text;
            State.Init.ExternalTriggerInputPort = ExternalTriggerChannel.Text;

            State.Init.MotorComPort = MotorCOM.Text;
            State.Init.openShutterDuringCalibration = openDuringCalib.Checked;

            State.Init.ComputerID = Convert.ToInt32(ComputerID.Text);
            State.Init.FLIMserial = Convert.ToInt32(FlimID.Text);

            State.Init.FLIM_on = FLIM_onCheck.Checked;
            State.Init.motor_on = Motor_onCheck.Checked;

            State.Init.FLIM_mode = (PQ_radio.Checked) ? "PQ" : "BH";
        }

        private void setupGUI()
        {
            String[] sP;
            sP = State.Init.mirrorAOPortX.Split('/');
            MirrorBoard.Text = sP[0];
            MirrorX.Text = sP[1];

            sP = State.Init.mirrorAOPortY.Split('/');
            MirrorY.Text = sP[1];

            sP = State.Init.mirrorAOTrigger.Split('/');
            Mirror_TriggerInput.Text = sP[2];

            sP = State.Init.mirrorAOSampleClockOutput.Split('/');
            Mirror_SampleClkOutput.Text = sP[2];

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
            ActiveLow.Checked = State.Init.DO_uncagingShutter_useForPMTsignal;

            UseAOMirrorOffset.Checked = State.Init.UseExternalMirrorOffset;

            sP = State.Init.UncagingShutterAnalogPort.Split('/');
            UncagingAOBoard.Text = PockelsBoard.Text + String_Pockel;
            Shutter_AOChannel.Text = sP[1];
            Shutter_AOChannel.Enabled = State.Init.AO_uncagingShutter;


            sP = State.Init.UncagingShutterDOPort.Split('/');
            UncagingDOBoard.Text = MirrorBoard.Text + String_Mirror;
            Shutter_DOChannel.Text = sP[2];
            Shutter_DOChannel.Enabled = State.Init.DO_uncagingShutter;

            sP = State.Init.EOM_SampleClockInput.Split('/');
            Pockels_SampleClkInput.Text = sP[2];

            sP = State.Init.EOM_Trigger.Split('/');
            Pockels_TriggerIn.Text = sP[2];

            sP = State.Init.shutterPort.Split('/');
            ShutterOutputBoard.Text = sP[0];
            ShutterOutputChannel.Text = sP[2];

            sP = State.Init.triggerPort.Split('/');
            TriggerOutputBoard.Text = sP[0];
            TriggerOutputChannel.Text = sP[2];

            MarkerInput.Text = State.Init.MarkerInput;

            ExternalTriggerChannel.Text = State.Init.ExternalTriggerInputPort;

            MotorCOM.Text = State.Init.MotorComPort;

            openDuringCalib.Checked = State.Init.openShutterDuringCalibration;

            ComputerID.Text = State.Init.ComputerID.ToString();
            FlimID.Text = State.Init.FLIMserial.ToString();

            BH_radio.Checked = State.Init.FLIM_mode == "BH";
            PQ_radio.Checked = State.Init.FLIM_mode == "PQ";

            Motor_onCheck.Checked = State.Init.motor_on;
            FLIM_onCheck.Checked = State.Init.FLIM_on;
        }

        private void NIDAQ_Config_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();

            FLIMage.ToolWindowClosed();
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


    }
}
