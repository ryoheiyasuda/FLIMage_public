using MicroscopeHardwareLibs.Stage_Contoller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MotorCtrlTest
{
    public partial class MotorCtrlTest : Form
    {
        MotorCtrl motorCtrl;
        double currentVelocity = 0;
        double[] motorCurrentPosition;
        bool[] MotorPolarity = new bool[4];
        string ComPort = "";
        int ROE_Speed = 0;

        public MotorCtrlTest()
        {
            InitializeComponent();
        }

        public MotorCtrlTest(String comPort)
        {
            ComPort = comPort;
            InitializeComponent();
        }

        private void MotorCtrlTest_Load(object sender, EventArgs e)
        {
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();

            if (ComPort == "")
            {
                for (int i = 0; i < ports.Length; i++)
                    ComPortPullDown.Items.Add(ports[i]);
                if (ports.Length > 1)
                    ComPortPullDown.SelectedIndex = 0;
                EnableButtons(false);
            }
            else
            {
                ComPortPullDown.Items.Add(ComPort);
                if (ports.Length > 1)
                    ComPortPullDown.SelectedIndex = 0;
                ConnectButton_Click(ConnectButton, null);
            }            
        }

        private void EnableButtons(Boolean ON)
        {
            StopButton.Enabled = ON;
            Xup.Enabled = ON;
            Xdown.Enabled = ON;
            Yup.Enabled = ON;
            Ydown.Enabled = ON;
            Zup.Enabled = ON;
            Zdown.Enabled = ON;
            ZeroButton.Enabled = ON;
            ConnectButton.Enabled = !ON;
            disconnectButton.Enabled = ON;
            HardZeroButton.Enabled = ON;
        }


        private void ComPortPullDown_Click(object sender, EventArgs e)
        {
        }


        private void disconnectButton_Click(object sender, EventArgs e)
        {
            if (motorCtrl != null && motorCtrl.connected)
            {
                motorCtrl.MotH -= MotorListener;
                motorCtrl.disconnect();
            }

            EnableButtons(false);
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (motorCtrl != null && motorCtrl.connected)
                return;

            double[] resolution = new double[] { 0.04, 0.04, 0.005 };
            double[] velocity = new double[] { 1000, 1000, 1000 };
            double[] steps = new double[] { 1, 1, 1 };
            int MotorDisplayUpdateTime_ms = 200;
            string comP = Convert.ToString(ComPortPullDown.SelectedItem);
            motorCtrl = new MotorCtrl("ZoZoLab", comP, resolution, velocity, steps, MotorDisplayUpdateTime_ms);

            if (motorCtrl != null && motorCtrl.connected)
            {
                motorCtrl.continuousRead(true);
                if (motorCtrl.connected)
                    motorCtrl.MotH += new MotorCtrl.MotorHandler(MotorListener);

                double resolution1 = motorCtrl.GetXYResolution();
                XYResolutionTextBox.Text = resolution1.ToString();
                ROE_Speed = motorCtrl.GetROESpeed();
                MotorPolarity = motorCtrl.GetPolarity();
                checkMotorSwitchBoxControl();
                ROESpeedTextBox.Text = ROE_Speed.ToString();

                EnableButtons(true);
            }
        }

        void checkMotorSwitchBoxControl()
        {
            checkBox1.Checked = MotorPolarity[0];
            checkBox2.Checked = MotorPolarity[1];
            checkBox3.Checked = MotorPolarity[2];
            checkBox4.Checked = MotorPolarity[3];
        }

        private void checkBox4_Click(object sender, EventArgs e)
        {
            MotorPolarity[0] = checkBox1.Checked;
            MotorPolarity[1] = checkBox2.Checked;
            MotorPolarity[2] = checkBox3.Checked;
            MotorPolarity[3] = checkBox4.Checked;

            MotorPolarity = motorCtrl.SetPolarity(MotorPolarity);
            checkMotorSwitchBoxControl();

        }


        void MotorHandler(MotrEventArgs e)
        {
            var temp = new double[3];
            if (RelativePositionCheck.Checked)
                temp = motorCtrl.getCalibratedRelativePosition();
            else
                temp = motorCtrl.getCalibratedAbsolutePosition();

            if (motorCurrentPosition == null)
                motorCurrentPosition = new double[3];

            for (int j = 0; j < 3; j++)
                if (motorCurrentPosition[j] != temp[j])
                {
                    motorCurrentPosition[j] = temp[j];
                    if (j == 0)
                        MotorPositionX_TextBox.Text = motorCurrentPosition[j].ToString();
                    else if (j == 1)
                        MotorPositionY_TextBox.Text = motorCurrentPosition[j].ToString();
                    else
                        MotorPositionZ_TextBox.Text = motorCurrentPosition[j].ToString();
                }

            double tmpVel = motorCtrl.velocity[0];

            if (currentVelocity != tmpVel)
            {
                VelocityTextBox.Text = motorCtrl.velocity[0].ToString();
                currentVelocity = tmpVel;
            }

            if (motorCtrl.tString.Split('-').Length <= 14)
                PositionSignalLabel.Text = motorCtrl.tString;
            else
                StatusSignalTextLabel.Text = motorCtrl.tString;
        }

        void MotorListener(MotorCtrl m, MotrEventArgs e)
        {
            try
            {
                //Blocking action.
                this.Invoke((Action)delegate
                {
                    MotorHandler(e);
                });
            }
            catch (Exception ex)
            {

            }
        }

        private void MotorCtrlTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (motorCtrl != null && motorCtrl.connected)
            {
                motorCtrl.MotH -= MotorListener;
                motorCtrl.disconnect();
            }

        }


        private void MotorPositionKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                double[] motorUpdatedPosition = new double[]
                {Convert.ToDouble(MotorPositionX_TextBox.Text),
                Convert.ToDouble(MotorPositionY_TextBox.Text),
                Convert.ToDouble(MotorPositionZ_TextBox.Text)};

                if (RelativePositionCheck.Checked)
                    motorCurrentPosition = motorCtrl.getCalibratedRelativePosition();
                else
                    motorCurrentPosition = motorCtrl.getCalibratedAbsolutePosition();

                double[] steps = new double[3];

                for (int i = 0; i < 3; i++)
                {
                    steps[i] = motorUpdatedPosition[i] - motorCurrentPosition[i];
                }

                motorCtrl.SetNewPosition_StepSize_um(steps);
                motorCtrl.MoveMotor(false, true);
                e.SuppressKeyPress = true;
            }
        }

        private void VelocityTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var newVel = Convert.ToDouble(VelocityTextBox.Text);
                motorCtrl.SetVelocity(new double[] { newVel, newVel, newVel });
                e.SuppressKeyPress = true;
            }
        }

        private void ROESpeedTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var newVel = Convert.ToInt32(ROESpeedTextBox.Text);
                newVel = motorCtrl.SetROESpeed(newVel);
                ROESpeedTextBox.Text = newVel.ToString();
                e.SuppressKeyPress = true;
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            motorCtrl.Stop();
        }

        private void ZeroButton_Click(object sender, EventArgs e)
        {
            motorCtrl.Zero_All();
        }

        private void UpDownClick(object sender, EventArgs e)
        {
            motorCtrl.GetPosition();
            double[] StepSize = new double[3];
            double[] mv = new double[] { 10, 10, 1 };
            Double.TryParse(StepX.Text, out mv[0]);
            Double.TryParse(StepY.Text, out mv[1]);
            Double.TryParse(StepZ.Text, out mv[2]);

            if (sender.Equals(Xup))
                StepSize[0] = mv[0];
            else if (sender.Equals(Xdown))
                StepSize[0] = -mv[0];
            else if (sender.Equals(Yup))
                StepSize[1] = mv[1];
            else if (sender.Equals(Ydown))
                StepSize[1] = -mv[1];
            else if (sender.Equals(Zup))
                StepSize[2] = mv[2];
            else if (sender.Equals(Zdown))
                StepSize[2] = -mv[2];
            motorCtrl.SetNewPosition_StepSize_um(StepSize);
            motorCtrl.MoveMotor(true, true);
        }

        private void HardZeroButton_Click(object sender, EventArgs e)
        {
            motorCtrl.HardZero();
        }
    }
}
