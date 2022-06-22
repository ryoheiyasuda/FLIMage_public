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
        

        //private const int PARAM_DEVICE_TYPE = 0;

        //private const int LIGHT_PATH = 0x2000000;
        //private const int PARAM_LIGHTPATH_GG = 1600;
        //private const int PARAM_LIGHTPATH_GR = 1601;
        //private const int PARAM_LIGHTPATH_CAMERA = 1602;

        //private const int TRUE = 1, FALSE = 0;

        //Buffer for getinformation.
        //int paramType = 0;
        //int paramAvailable = 0;
        //int paramReadOnly = 0;
        //double paramMin = 0.0;
        //double paramMax = 0.0;
        //double paramDefault = 0.0;

        ScanParameters State;

        bool thorECU_scan_on = false;
        bool thorECU_pmt_on = false;
        bool thorECU_scan_pmt_samePort = false;
        bool thor_controlFromMotor = false;

        bool zozo_pmt_on = false;

        ThorECU thorECU_d; //direct
        ThorBCM ThorBCM_g; //Galvo
        ThorBCM ThorBCM_r; //reso
        ThorBCM ThorBCM_c; //camera

        ThorDLL thorBCM;
        ZoZoLab_PMT zozoPMT;

        FLIMageMain flimage;

        public PMTControl(ScanParameters state, FLIMageMain flim)
        {
            InitializeComponent();
            PMTPanel.Enabled = false;
            GalvoPanel.Enabled = false;
            State = state;
            flimage = flim;
            thorECU_scan_on = flimage.flimage_io.thorECU_on;

            if (State.Init.PMTModule.Contains("ThorECU"))
            {
                if (thorECU_scan_on && State.Init.PMTModule_COMPort == State.Init.resonantScanner_COMPort)
                {
                    thorECU_d = flimage.flimage_io.thorECU;
                    thorECU_scan_pmt_samePort = true;
                    thorECU_pmt_on = true;
                }
                else
                {
                    string board = State.Init.ResonantAOBoard;
                    string port = board + "/port0/line0:3";

                    //Just for PMT.
                    bool old = false;
                    if (State.Init.resonantScannerSystem.ToLower().Contains("_old"))
                        old = true;

                    thorECU_d = new ThorECU(State.Init.PMTModule_COMPort, port, old);
                    thorECU_scan_pmt_samePort = false;
                    thorECU_pmt_on = true;
                }


            }
            else if (State.Init.PMTModule.ToLower().Contains("zozolab"))
            {
                zozoPMT = new ZoZoLab_PMT(State.Init.PMTModule_COMPort);
                zozo_pmt_on = true;
            }

            if (State.Init.MicroscopeFlipper == "ThorBCM")
            {
                ThorBCM_g = new ThorBCM("COM33");
                ThorBCM_r = new ThorBCM("COM34");
                ThorBCM_c = new ThorBCM("COM35");
            }
            else if (State.Init.MicroscopeFlipper.ToLower().Contains("bscope"))
            {
                if (State.Init.MotorHWName.ToLower().Contains("bscope"))
                    thor_controlFromMotor = true;
                else
                    thorBCM = ThorDLL.ThorDLL_Load(ThorDLL.DLLType.ThorBScope);
            }

            initFolder = State.Files.initFolderPath;
        }


        public void ClosePorts()
        {
            if (!thorECU_scan_pmt_samePort && PMTPanel.Enabled)
                thorECU_d.ClosePort();

            if (State.Init.MicroscopeFlipper == "ThorBCM")
            {
                if (GalvoPanel.Enabled)
                    ThorBCM_g.ClosePort();

                if (ResGalvoPanel.Enabled)
                    ThorBCM_r.ClosePort();

                if (CameraPanel.Enabled)
                    ThorBCM_c.ClosePort();
            }
        }

        public void connectToSerial()
        {
            if (zozo_pmt_on)
            {
                if (zozoPMT.OpenPort() == 1)
                {
                    PMTPanel.Enabled = true;
                    TurnOnPMT(false);
                }
            }
            if (thorECU_pmt_on)
            {
                if (thorECU_scan_pmt_samePort)
                {
                    PMTPanel.Enabled = true;
                    TurnOnPMT(false);
                }
                else
                {
                    if (thorECU_d.OpenPort() == 1)
                    {
                        PMTPanel.Enabled = true;
                        TurnOnPMT(false);
                    }
                }

                if (thorECU_d.old)
                {
                    PMT_GainLabel1.Text = "PMT Gain 1(1 - 100)";
                    PMT_GainLabel2.Text = "PMT Gain 2(1 - 100)";
                }
                else
                {
                    PMT_GainLabel1.Text = "PMT Gain 1(1 - 510)";
                    PMT_GainLabel2.Text = "PMT Gain 2(1 - 510)";
                }
            }
            

            if (State.Init.MicroscopeFlipper == "ThorBCM")
            {
                if (ThorBCM_g.OpenPort() == 1)
                {
                    GalvoPanel.Enabled = true;
                    ThorBCM_g.TurnOnOff(0);
                }
                else
                    GalvoPanel.Enabled = false;

                if (ThorBCM_r.OpenPort() == 1)
                {
                    ResGalvoOffRadio.Enabled = true;
                    ThorBCM_r.TurnOnOff(0);
                }
                else
                    ResGalvoOffRadio.Enabled = false;

                if (ThorBCM_c.OpenPort() == 1)
                {
                    CameraPanel.Enabled = true;
                    ThorBCM_c.TurnOnOff(1);
                }
                else
                    CameraPanel.Enabled = false;
            }

            if (thor_controlFromMotor)  //Flipper = "ThorBScope"
            {
                GalvoPanel.Enabled = State.Init.enableRegularGalvo;
                ResGalvoPanel.Enabled = State.Init.enableResonantScanner;                
                CameraPanel.Enabled = true;
                flimage.motorCtrl.SwitchPMT(false);
                int val = flimage.motorCtrl.GetGGStatus();
                GalvoOffRadio.Checked = (val == 0);
                GalvoOnRadio.Checked = (val != 0);
                val = flimage.motorCtrl.GetGRStatus();
                ResGalvoOnRadio.Checked = (val != 0);
                ResGalvoOffRadio.Checked = (val == 0);
            }
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

            if (thorECU_pmt_on)
            {
                string s1 = thorECU_d.TurnOnPMT(1, TurnOn);
                string s2 = thorECU_d.TurnOnPMT(2, TurnOn);

                s1 = thorECU_d.SetPMTGain(1, Gain1);
                s2 = thorECU_d.SetPMTGain(2, Gain2);
                PMT1Status.Text = s1;
                PMT2Status.Text = s2;
            }
            else if (zozo_pmt_on)
            {
                zozoPMT.TurnOnPMT(0, TurnOn);
                zozoPMT.SetPMTGain(1, Gain1);
                zozoPMT.SetPMTGain(2, Gain2);

                int val1 = zozoPMT.GetPMTGain(1);
                int val2 = zozoPMT.GetPMTGain(2);

                string s1 = "PMT 1: Gain = " + val1;
                string s2 = "PMT 2: Gain = " + val2;

                PMT1Status.Text = s1;
                PMT2Status.Text = s2;
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

            if (State.Init.MicroscopeFlipper == "ThorBCM")
                ThorBCM_g.TurnOnOff(val);
            else if (thor_controlFromMotor)
                flimage.motorCtrl.SwitchGG(GalvoOnRadio.Checked);
        }

        private void ReGaolvoClick(object sender, EventArgs e)
        {
            int val = ResGalvoOnRadio.Checked ? 1 : 0;
            if (State.Init.MicroscopeFlipper == "ThorBCM")
                ThorBCM_r.TurnOnOff(val);
            else if (thor_controlFromMotor)
                flimage.motorCtrl.SwitchGR(ResGalvoOnRadio.Checked);
        }

        private void CameraOnOffClick(object sender, EventArgs e)
        {
            int val = CameraOnRadio.Checked ? 1 : 0;
            if (State.Init.MicroscopeFlipper == "ThorBCM")
                ThorBCM_c.TurnOnOff(val);
            else
                flimage.motorCtrl.SwitchPMT(!CameraOnRadio.Checked);
        }

        private void PMTControl_Load(object sender, EventArgs e)
        {
            InitializeSetting();
            connectToSerial();
        }
    }//Class
}
