using FLIMage.HardwareControls.ThorLabs;
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

        ThorDLL thorECU;
        ThorDLL thorBCM;

        public PMTControl(ScanParameters state)
        {
            InitializeComponent();
            PMTPanel.Enabled = false;

            State = state;
            if (State.Init.ThorPMTModule == "ThorECU")
                thorECU = ThorDLL.ThorDLL_Load(ThorDLL.DLLType.ThorECU);
            else if (State.Init.ThorPMTModule == "ThorBScope")
                thorECU = ThorDLL.ThorDLL_Load(ThorDLL.DLLType.ThorBScope);
            else
            {
                MessageBox.Show("There is no PMT module DLL for " + state);
                return;
            }

            if (State.Init.ThorFlipper == "ThorBCM")
                thorBCM = ThorDLL.ThorDLL_Load(ThorDLL.DLLType.ThorBCM);
            else if (State.Init.ThorFlipper == "ThorBScope")
                thorBCM = ThorDLL.ThorDLL_Load(ThorDLL.DLLType.ThorBScope);

            initFolder = State.Files.initFolderPath;
        }


        public void DLLUnload()
        {
            //We should not unload until very end.
            //ThorDLL.ThorlabDLL_Unload(ThorDLL.DLLType.ThorECU);
            //ThorDLL.ThorlabDLL_Unload(ThorDLL.DLLType.ThorBCM);
        }
        
        public void connectToSerial()
        {
            int ret;
            int deviceCount = 0;

            ret = thorECU.FindDevices(ref deviceCount);
            if (ret != 1)
            {
                MessageBox.Show("ThorECU.FindDevices(ref deviceCount) failed: Failed to connect with the device. Check XML file and port!");
                return;
            }

            ret = thorECU.SelectDevice(0);
            if (ret != 1)
            {
                MessageBox.Show("ThorECU.SelectDevice(0) failed: Failed to connect with the device. Check if device is connected!");
                return;
            }

            if (thorECU.GetParamInfo(ThorParam.PARAM_PMT2_SAFETY, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) == 1)
            {
                thorECU.GetParam(ThorParam.PARAM_PMT1_SAFETY, ref paramDefault);
                thorECU.GetParam(ThorParam.PARAM_PMT2_SAFETY, ref paramDefault);
            }


            ret = thorBCM.FindDevices(ref deviceCount);
            if (ret != 1)
            {
                MessageBox.Show("ThorBCM.FindDevices(ref deviceCount) failed: Failed to connect with the device. Check XML file and port!");
                return;
            }

            ret = thorBCM.SelectDevice(0);
            if (ret != 1)
            {
                MessageBox.Show("ThorBCM.SelectDevice(0) failed: Failed to connect with the device. Check if device is connected!");
                return;
            }

        }


        private void PMTEnableCB_CheckedChanged(object sender, EventArgs e)
        {
            TurnOnPMT(PMTEnableCB.Checked);
        }

        void SwitchToCamera(bool camera_on)
        {
            int CameraBool = camera_on ? 1 : 0;
            int PMTBool = camera_on ? 0 : 1;

            //First put switch to Camera port anyway first.
            if (thorBCM.GetParamInfo(PARAM_LIGHTPATH_CAMERA, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) == TRUE)
            {
                thorBCM.SetParam(PARAM_LIGHTPATH_CAMERA, 1);
                thorBCM.StartPosition();

            }

            //Not sure necessary?
            if (thorBCM.GetParamInfo(PARAM_LIGHTPATH_GR, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) == TRUE)
            {
                thorBCM.SetParam(PARAM_LIGHTPATH_GR, PMTBool);
                thorBCM.StartPosition();
            }

            if (thorBCM.GetParamInfo(PARAM_LIGHTPATH_GG, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) == TRUE)
            {
                thorBCM.SetParam(PARAM_LIGHTPATH_GG, PMTBool);
                thorBCM.StartPosition();
            }

            if (!camera_on) //PMT on.
                if (thorBCM.GetParamInfo(PARAM_LIGHTPATH_CAMERA, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) == TRUE)
                {
                    thorBCM.SetParam(PARAM_LIGHTPATH_CAMERA, CameraBool);
                    thorBCM.StartPosition();
                }
        }

        void TurnOnPMT(bool on)
        {
            int TurnOn = (on) ? 1 : 0;

            if (!Double.TryParse(PMTGain1.Text, out double Gain1) || Gain1 >= 1000.0 || Gain1 < 0)
            {
                Gain1 = defaultGain1;
            }

            if (!Double.TryParse(PMTGain1.Text, out double Gain2) || Gain2 >= 1000.0 || Gain2 < 0)
            {
                Gain2 = defaultGain2;
            }

            if (thorECU.GetParamInfo(ThorParam.PARAM_PMT1_GAIN_POS, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) == 1)
            {
                thorECU.PreflightPosition();
                thorECU.SetParam(ThorParam.PARAM_PMT1_ENABLE, TurnOn);
                if (PMTEnableCB.Checked)
                    thorECU.SetParam(ThorParam.PARAM_PMT1_GAIN_POS, Gain1 / 1000.0 * paramMax);
                else
                    thorECU.SetParam(ThorParam.PARAM_PMT1_GAIN_POS, paramMin);
                thorECU.SetupPosition();
                thorECU.StartPosition();
                thorECU.PostflightPosition();
                thorECU.GetParam(ThorParam.PARAM_PMT1_GAIN_POS, ref Gain1);
                PMT1Status.Text = "PMT1 Gain: " + (Gain1 / paramMax * 1000.0);
            }
            else
            {
                MessageBox.Show("PMT1 not available");
                return;
            }

            if (thorECU.GetParamInfo(ThorParam.PARAM_PMT2_GAIN_POS, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) == 1)
            {
                thorECU.PreflightPosition();
                thorECU.SetParam(ThorParam.PARAM_PMT2_ENABLE, TurnOn);
                if (PMTEnableCB.Checked)
                    thorECU.SetParam(ThorParam.PARAM_PMT2_GAIN_POS, Gain2 / 1000.0 * paramMax);
                else
                    thorECU.SetParam(ThorParam.PARAM_PMT2_GAIN_POS, paramMin);
                thorECU.SetupPosition();
                thorECU.StartPosition();
                thorECU.PostflightPosition();
                thorECU.GetParam(ThorParam.PARAM_PMT2_GAIN_POS, ref Gain2);
                PMT2Status.Text = "PMT2 Gain: " + (Gain2 / paramMax * 1000.0);
            }
            else
            {
                MessageBox.Show("PMT2 not available");
                return;
            }

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
            Hide();
            e.Cancel = true;
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

        private void Camera2PSwitchRadioClick(object sender, EventArgs e)
        {
            SwitchToCamera(CameraRadio.Checked);
        }

        private void PMTControl_Load(object sender, EventArgs e)
        {
            InitializeSetting();
            connectToSerial();
        }

    }//Class
}
