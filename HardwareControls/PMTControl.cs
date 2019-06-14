using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;
using MicroscopeHardwareLibs;

namespace FLIMage.HardwareControls
{
    public partial class PMTControl : Form
    {
        int defaultGain1 = 800;
        int defaultGain2 = 800;
        int nChannels = 2;

        SettingManager settingManager;
        String settingName = "PMTControl";
        String initFolder = "";
        WindowLocManager winManager;
        String windowName = "PMTControl";



        private const int PARAM_DEVICE_TYPE = 0;

        private const int LIGHT_PATH = 0x2000000;
        private const int PARAM_LIGHTPATH_GG = 1600;
        private const int PARAM_LIGHTPATH_GR = 1601;
        private const int PARAM_LIGHTPATH_CAMERA = 1602;

        private const int TRUE = 1, FALSE = 0;

        //Buffer for getinformation.
        int paramType = 0;
        int paramAvailable = 0;
        int paramReadOnly = 0;
        double paramMin = 0.0;
        double paramMax = 0.0;
        double paramDefault = 0.0;

        ScanParameters State;

        ThorECU thorECU_d; //direct
        ThorBCM ThorBCM_g; //Galvo
        ThorBCM ThorBCM_r; //reso
        ThorBCM ThorBCM_c; //camera

        ThorDLL thorBCM;

        public PMTControl(ScanParameters state)
        {
            InitializeComponent();
            PMTPanel.Enabled = false;
            GalvoPanel.Enabled = false;
            State = state;

            if (State.Init.ThorPMTModule == "ThorECU")
            {
                thorECU_d = new ThorECU("COM29");
            }
            else if (State.Init.ThorPMTModule == "ThorBScope") //perhaps just different com port.
            {
                thorECU_d = new ThorECU("COM31");
            }
            else
            {
                MessageBox.Show("There is no PMT module DLL for " + state);
                return;
            }

            if (State.Init.ThorFlipper == "ThorBCM")
            {
                ThorBCM_g = new ThorBCM("COM33");
                ThorBCM_r = new ThorBCM("COM34");
                ThorBCM_c = new ThorBCM("COM35");
            }
            else if (State.Init.ThorFlipper == "ThorBScope")
                thorBCM = ThorDLL.ThorDLL_Load(ThorDLL.DLLType.ThorBScope);

            initFolder = State.Files.initFolderPath;
        }


        public void ClosePorts()
        {
            if (PMTPanel.Enabled)
                thorECU_d.ClosePort();

            if (GalvoPanel.Enabled)
                ThorBCM_g.ClosePort();

            if (ResGalvoPanel.Enabled)
                ThorBCM_r.ClosePort();

            if (CameraPanel.Enabled)
                ThorBCM_c.ClosePort();
        }

        public void connectToSerial()
        {

            if (thorECU_d.OpenPort() == 1)
            {
                PMTPanel.Enabled = true;
                TurnOnPMT(false);
            }

            if (ThorBCM_g.OpenPort() == 1)
            {
                GalvoPanel.Enabled = true;
                ThorBCM_g.TurnOnOff(0);
            }
            else
                GalvoPanel.Enabled = false;

            if (ThorBCM_r.OpenPort() == 1)
            {
                ResGalvoOffPanel.Enabled = true;
                ThorBCM_r.TurnOnOff(0);
            }
            else
                ResGalvoOffPanel.Enabled = false;

            if (ThorBCM_c.OpenPort() == 1)
            {
                CameraPanel.Enabled = true;
                ThorBCM_c.TurnOnOff(1);
            }
            else
                CameraPanel.Enabled = false;
        }


        private void PMTEnableCB_CheckedChanged(object sender, EventArgs e)
        {
            TurnOnPMT(PMTEnableCB.Checked);
        }



        void TurnOnPMT(bool on)
        {
            int TurnOn = (on) ? 1 : 0;

            if (!Int32.TryParse(PMTGain1.Text, out int Gain1) || Gain1 >= 1000 || Gain1 < 0)
            {
                Gain1 = defaultGain1;
            }

            if (!Int32.TryParse(PMTGain2.Text, out int Gain2) || Gain2 >= 1000 || Gain2 < 0)
            {
                Gain2 = defaultGain2;
            }

            string s1 = thorECU_d.TurnOnPMT(1, TurnOn);
            string s2 = thorECU_d.TurnOnPMT(2, TurnOn);

            s1 = thorECU_d.SetPMTGain(1, Gain1);
            s2 = thorECU_d.SetPMTGain(2, Gain2);

            PMT1Status.Text = s1;
            PMT2Status.Text = s2;
        }

        void InitializeSetting()
        {
            if (initFolder != "")
            {
                settingManager = new SettingManager(settingName, initFolder);
                settingManager.AddToDict(PMTGain1);
                settingManager.AddToDict(PMTGain2);
                settingManager.LoadToObject();
                winManager = new WindowLocManager(this, windowName, initFolder);
                winManager.LoadWindowLocation(true);
            }
        }

        void PMTControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            WindowClosing();
            ClosePorts();
            //Hide();
            //e.Cancel = true;
        }

        public void WindowClosing()
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


        private void GalvoOnOffClick(object sender, EventArgs e)
        {
            int val = GalvoOnRadio.Checked ? 1 : 0;
            ThorBCM_g.TurnOnOff(val);
        }

        private void ReGaolvoClick(object sender, EventArgs e)
        {
            int val = ResGalvoOnRadio.Checked ? 1 : 0;
            ThorBCM_r.TurnOnOff(val);
        }

        private void CameraOnOffClick(object sender, EventArgs e)
        {
            int val = CameraOnRadio.Checked ? 1 : 0;
            ThorBCM_c.TurnOnOff(val);
        }

        private void PMTControl_Load(object sender, EventArgs e)
        {
            InitializeSetting();
            connectToSerial();
        }

    }//Class
}
