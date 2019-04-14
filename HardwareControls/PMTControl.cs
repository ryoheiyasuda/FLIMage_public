using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace ThorlabController
{
    public partial class PMTControl : Form
    {
        SerialPort port;
        String COMport;
        int baudRate = 115200;

        int defaultGain1 = 80;
        int defaultGain2 = 80;
        int nChannels = 2;

        SettingManager settingManager;
        String settingName = "PMTControl";
        String initFolder = "";
        WindowLocManager winManager;
        String windowName = "PMTControl";

        public PMTControl()
        {
            InitializeComponent();
            PMTPanel.Enabled = false;
        }

        public PMTControl(String SettingSaveFolder)
        {
            InitializeComponent();
            PMTPanel.Enabled = false;
            initFolder = SettingSaveFolder;
        }

        public void connectToSerial()
        {

            COMport = PortName.Text;
            port = new SerialPort(COMport, baudRate, Parity.None, 8, StopBits.One);
            try
            {
                if (ConnectButton.Text == "Connect")
                {
                    port.Open();
                    PMTPanel.Enabled = true;
                    ConnectButton.Text = "Disconnect";
                    StatusLabel.Text = "Connected!";
                }
                else
                {
                    port.Close();
                    ConnectButton.Text = "Connect";
                    StatusLabel.Text = "Disconnected!";
                }
            }
            catch (Exception EX)
            {
                Debug.WriteLine(EX.ToString());
                StatusLabel.Text = "Failed connecting!!";
            }
            System.Threading.Thread.Sleep(200);
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            connectToSerial();
        }

        public void ClearBuffer()
        {
            try
            {
                port.ReadExisting();
            }
            catch (TimeoutException EX)
            {
                Debug.WriteLine("Clearing --- timeout");
            }
        }

        private void PMTEnableCB_CheckedChanged(object sender, EventArgs e)
        {
            PMTPanel.Enabled = false;
            ClearBuffer();
            int enable = PMTEnableCB.Checked ? 1 : 0;
            int gain1 = Int32.TryParse(PMTGain1.Text, out int gain1T) ? gain1T : defaultGain1;
            int gain2 = Int32.TryParse(PMTGain2.Text, out int gain2T) ? gain2T : defaultGain2;
            port.Write(String.Format("pmt1={0}\r", enable));
            port.Write(String.Format("pmt2={0}\r", enable));
            port.Write(String.Format("pmt1gain={0}\r", gain1));
            port.Write(String.Format("pmt1gain={0}\r", gain2));

            System.Threading.Thread.Sleep(250);

            ClearBuffer();
            for (int i = 0; i < nChannels; i++)
            {
                port.Write(String.Format("pmt{0}?\r", i));
                System.Threading.Thread.Sleep(200);
                String str = ReadPort();
                if (i == 0)
                    PMT1Status.Text = str;
                else
                    PMT2Status.Text = str;
            }

            PMTPanel.Enabled = true;
        }

        String ReadPort()
        {
            return port.ReadLine();
        }

        public void InitializeSerial()
        {
            string[] ports = SerialPort.GetPortNames();
            ComportPulldown.Items.Clear();
            foreach (string port in ports)
                ComportPulldown.Items.Add(port);

            FormControllers.PulldownSelectByItemString(ComportPulldown, PortName.Text);
        }

        private void ComportPulldown_SelectedIndexChanged(object sender, EventArgs e)
        {
            PortName.Text = ComportPulldown.SelectedItem.ToString();
        }

        void InitializeSetting()
        {
            if (initFolder != "")
            {
                settingManager = new SettingManager(settingName, initFolder);
                settingManager.AddToDict(PortName);
                settingManager.AddToDict(PMTGain1);
                settingManager.AddToDict(PMTGain2);
                settingManager.LoadToObject();
                winManager = new WindowLocManager(this, windowName, initFolder);
                winManager.LoadWindowLocation(true);
            }
        }

        void PMTControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowLocationAndSetting();
        }

        public void SaveWindowLocationAndSetting()
        {
            if (initFolder != "" && settingManager != null)
                settingManager.SaveFromObject();

            if (initFolder != "" && winManager != null)
                winManager.SaveWindowLocation();
        }

        private void PMTControl_Load(object sender, EventArgs e)
        {
            InitializeSetting();
            InitializeSerial();
            Application.DoEvents();
            System.Threading.Thread.Sleep(100);
            connectToSerial();
        }
    }//Class
}
