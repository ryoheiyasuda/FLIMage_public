using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroscopeHardwareLibs.Stage_Contoller
{
    public class ThorMCMX000
    {

        public event MotorHandler MotH;
        public MotrEventArgs e = new MotrEventArgs("");
        public delegate void MotorHandler(ThorMCMX000 mCtrls, MotrEventArgs e);

        public double maxDistanceXY = 500; //micrometers
        public double maxDistanceZ = 100;

        public int N_Coordinate = 3;

        public double XPos, YPos, ZPos;
        public double XNewPos, YNewPos, ZNewPos;
        //public double XPosMov, YPosMov, ZPosMov;

        public double XMax, YMax, ZMax;
        public double XMin, YMin, ZMin;

        public double minMovX = 0.2;
        public double minMovY = 0.2;
        public double minMovZ = 0.05;
        //public SerialPort port;

        public double[] maxPos = new double[3];
        public double[] minPos = new double[3];

        public int minMotorVal = (int)(-Math.Pow(2, 31) + 1);

        public bool start_moving, moving;

        public double resolutionX = 1000;   
        public double resolutionY = 1000; //=0.04
        public double resolutionZ = 1000; //=0.005

        public double[] velocity = new double[3];
        public double[] maxVelocity = new double[3];
        public double[] minVelocity = new double[3];

        public bool forceStop = false;

        public double velVal;

        public Stopwatch sw = new Stopwatch();

        //public int ZStackStart_rel, ZStack_End_rel;
        public MotorCtrl.DeviceMode device_mode;

        public bool freezing = false;

        public bool continuous_read = true;
        public bool continuous_readCheck = true;

        object lockPort;

        public String tString;//

        System.Timers.Timer ThoTimer;
        public double timeoutMilliseconds = 5000;

        //Buffer for getinformation.
        int paramType = 0;
        int paramAvailable = 0;
        int paramReadOnly = 0;
        double paramMin = 0.0;
        double paramMax = 0.0;
        double paramDefault = 0.0;
        double param = 0.0;

        bool GG_light_path_available = true;
        bool GR_light_path_available = true;
        bool Camera_path_available = true;

        public const int TRUE = 1, FALSE = 0;
        public bool connected = false;
        public int MotorDisplayUpdateTime_ms = 300;
        ThorDLL thordll;
        private MotorCtrl.MotorTypeEnum motorType;

        public ThorMCMX000(MotorCtrl.MotorTypeEnum motor_type, int MotorDisplayUpdateTime)
        {

            //resolutionX = resolution[0];
            //resolutionY = resolution[1];
            //resolutionZ = resolution[2];

            motorType = motor_type;
            lockPort = new object();

            if (motor_type == MotorCtrl.MotorTypeEnum.thorMCM3000)
            {
                thordll = ThorDLL.ThorDLL_Load(ThorDLL.DLLType.ThorMCM3000);
            }
            else
                thordll = ThorDLL.ThorDLL_Load(ThorDLL.DLLType.ThorBScope);

            MotorDisplayUpdateTime_ms = MotorDisplayUpdateTime;

            int ret;
            int deviceCount = 0;
            ret = thordll.FindDevices(ref deviceCount);

            if (ret != TRUE)
            {
                MessageBox.Show("ThorMCMX000.FindDevices(ref deviceCount) failed: Failed to connect with the device. Check XML file and port!");
                return;
            }

            ret = thordll.SelectDevice(0);
            if (ret != TRUE)
            {
                MessageBox.Show("ThorMCMX000.SelectDevice(0) failed: Failed to connect with the device. Check if device is connected!");
                return;
            }

            if (thordll.GetParamInfo(ThorParam.PARAM_DEVICE_TYPE, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) != TRUE)
            {
                MessageBox.Show("ThorMCMX000.GetParamInfo(PARAM_DEVICE_TYPE) failed: Failed to connect with the device. Check the device status!");
                return;
            }

            if (thordll.GetParamInfo(ThorParam.PARAM_X_POS, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) != TRUE)
            {
                MessageBox.Show("ThorMCMX000.GetParamInfo(PARAM_X_POS) failed: Failed to connect with the device. Check the device status!");
                return;
            }

            if (paramAvailable == TRUE)
            {
                XMax = paramMax * resolutionX;
                XMin = paramMin * resolutionX;
            }

            if (thordll.GetParamInfo(ThorParam.PARAM_Y_POS, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) != TRUE)
            {
                MessageBox.Show("ThorMCMX000.GetParamInfo(PARAM_Y_POS) failed: Failed to connect with the device. Check the device status!");
                return;
            }

            if (paramAvailable == TRUE)
            {
                YMax = paramMax * resolutionY;
                YMin = paramMin * resolutionY;
            }


            if (thordll.GetParamInfo(ThorParam.PARAM_Z_POS, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) != TRUE)
            {
                MessageBox.Show("ThorMCMX000.GetParamInfo(PARAM_Z_POS) failed: Failed to connect with the device. Check the device status!");
                return;
            }

            if (paramAvailable == TRUE)
            {
                ZMax = paramMax * resolutionZ;
                ZMin = paramMin * resolutionZ;
            }

            if (thordll.GetParamInfo(ThorParam.PARAM_LIGHTPATH_GG, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) != TRUE)
            {
                GG_light_path_available = false;
            }

            if (thordll.GetParamInfo(ThorParam.PARAM_LIGHTPATH_GR, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) != TRUE)
            {
                GR_light_path_available = false;
            }

            if (thordll.GetParamInfo(ThorParam.PARAM_LIGHTPATH_CAMERA, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) != TRUE)
            {
                Camera_path_available = false;
            }

            GetVelocityLimits();
            
            GetPosition();
            velocity = GetStatus();

            start_moving = false;
            moving = false;
            
            tString = "";
            freezing = false;

            ThoTimer = new System.Timers.Timer(MotorDisplayUpdateTime_ms);
            ThoTimer.Elapsed += TimerEvent;
            ThoTimer.AutoReset = true;
            ThoTimer.Enabled = true;

            connected = true;
        }


        public double[] GetResolution()
        {
            return new double[] { resolutionX, resolutionY, resolutionZ };
        }

        public void unsubscribe()
        {
            ThoTimer.Elapsed -= TimerEvent;
        }

        public void disconnect()
        {
            ThoTimer.Close();
            ThoTimer.Dispose();
            connected = false;
            ThorDLL.ThorlabDLL_Unload(ThorDLL.DLLType.ThorMCM3000);
        }

        public void reopen()
        {
            if (motorType == MotorCtrl.MotorTypeEnum.thorMCM3000)
            {
                thordll = ThorDLL.ThorDLL_Load(ThorDLL.DLLType.ThorMCM3000);
            }
            else
                thordll = ThorDLL.ThorDLL_Load(ThorDLL.DLLType.ThorBScope);

            if (thordll.SelectDevice(0) == TRUE)
                connected = true;

            if (connected)
            {
                ThoTimer = new System.Timers.Timer(MotorDisplayUpdateTime_ms);
                ThoTimer.Elapsed += TimerEvent;
                ThoTimer.AutoReset = true;
                ThoTimer.Enabled = true;
            }
        }

        public void WaitUntilAllTaskDone()
        {
            WaitUntilMovementDone();
        }

        public int WaitUntilMovementDone()
        {
            if (!connected)
                return 0;

            if (!moving)
                return 1;

            Stopwatch sw = new Stopwatch();
            double X1 = XPos;
            double Y1 = YPos;
            double Z1 = ZPos;

            sw.Start();
            long status = 1;
            do
            {
                if (0 == thordll.StatusPosition(ref status))
                {
                    Debug.WriteLine("Failed movement");
                    return 0;
                }

                double readPos = 0;
                thordll.GetParam(ThorParam.PARAM_X_POS_CURRENT, ref readPos);
                XPos = readPos;
                thordll.GetParam(ThorParam.PARAM_Y_POS_CURRENT, ref readPos);
                YPos = readPos;
                thordll.GetParam(ThorParam.PARAM_Z_POS_CURRENT, ref readPos);
                ZPos = readPos;


                if (forceStop)
                {
                    Stop();
                    return 0;
                }

                if (sw.ElapsedMilliseconds > timeoutMilliseconds)
                {
                    if (X1 == XPos && Y1 == YPos && Z1 == ZPos)
                        MessageBox.Show("Movement failed. Time out = " + timeoutMilliseconds + " ms");
                    return 0;
                }


                MotH?.Invoke(this, new MotrEventArgs(""));
            }
            while (status == 0);

            moving = false;

            if (thordll.PostflightPosition() == FALSE)
            {
                Debug.WriteLine("Failed mvoement");
                return 0;
            }
            else
                return 1;
        }

        public void Stop()
        {
            if (connected)
            {
                forceStop = true;
                thordll.SetParam(ThorParam.PARAM_X_STOP, 1);
                thordll.SetParam(ThorParam.PARAM_Y_STOP, 1);
                thordll.SetParam(ThorParam.PARAM_Z_STOP, 1);
                XNewPos = XPos;
                YNewPos = YPos;
                ZNewPos = ZPos;
            }
        }

        public void GetVelocityLimits()
        {
            if (connected)
            {
                thordll.GetParamInfo(ThorParam.PARAM_X_VELOCITY, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault);
                if (paramAvailable == TRUE)
                {
                    velocity[0] = paramDefault;
                    maxVelocity[0] = paramMax;
                    minVelocity[0] = paramMin;
                }
                thordll.GetParamInfo(ThorParam.PARAM_Y_VELOCITY, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault);
                if (paramAvailable == TRUE)
                {
                    velocity[1] = paramDefault;
                    maxVelocity[1] = paramMax;
                    minVelocity[1] = paramMin;
                }
                thordll.GetParamInfo(ThorParam.PARAM_Z_VELOCITY, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault);
                if (paramAvailable == TRUE)
                {
                    velocity[2] = paramDefault;
                    maxVelocity[2] = paramMax;
                    minVelocity[2] = paramMin;
                }
            }
        }

        public double[] GetNewPosition()
        {
            double[] XYZ = new double[3];
            XYZ[0] = XNewPos;
            XYZ[1] = YNewPos;
            XYZ[2] = ZNewPos;
            return XYZ;
        }

        public double[] GetCurrentUncalibratedPosition()
        {
            double[] XYZ = new double[3];
            XYZ[0] = XPos;
            XYZ[1] = YPos;
            XYZ[2] = ZPos;
            return XYZ;
        }


        public void SetVelocity(double[] val)
        {
            WaitUntilAllTaskDone();
            //if (val[0] < maxVelocity[0] && val[0] > minVelocity[0])
            //    SetParam(PARAM_X_VELOCITY, val[0]);
            //if (val[1] < maxVelocity[1] && val[1] > minVelocity[1])
            //    SetParam(PARAM_Y_VELOCITY, val[2]);
            //if (val[2] < maxVelocity[2] && val[2] > minVelocity[2])
            //    SetParam(PARAM_Z_VELOCITY, val[2]);
        }


        public void SetNewPosition(double[] XYZ)
        {
            if (XYZ[0] < XMax && XYZ[0] > XMin)
                XNewPos = XYZ[0];
            else
                MessageBox.Show("X position is over the limit");

            if (XYZ[1] < YMax && XYZ[1] > YMin)
                YNewPos = XYZ[1];
            else
                MessageBox.Show("Y position is over the limit");

            if (XYZ[2] < ZMax && XYZ[2] > ZMin)
                ZNewPos = XYZ[2];
            else
                MessageBox.Show("Z position is over the limit");

        }

        public void SetPosition()
        {
            sw.Reset();
            sw.Start();
            forceStop = false;
            start_moving = true;
            setPosition_internal();
        }


        public int setPosition_internal()
        {
            if (connected)
            {
                WaitUntilAllTaskDone();

                moving = true;
                int ret = 1;

                if (Math.Abs(XNewPos - XPos) > minMovX / resolutionX)
                {
                    double Xtmp = 0;
                    thordll.GetParam(ThorParam.PARAM_X_POS, ref Xtmp);
                    thordll.SetParam(ThorParam.PARAM_X_POS, XNewPos);
                    ret = Move();
                    ret = WaitUntilMovementDone(); //Note that ret == 0 is fail. 
                }

                if (Math.Abs(YNewPos - YPos) > minMovY / resolutionY)
                {
                    thordll.SetParam(ThorParam.PARAM_Y_POS, YNewPos);
                    ret = Move();
                    ret = WaitUntilMovementDone(); //Note that ret == 0 is fail. 
                }

                if (Math.Abs(ZNewPos - ZPos) > minMovZ / resolutionZ)
                {
                    thordll.SetParam(ThorParam.PARAM_Z_POS, ZNewPos);
                    ret = Move();
                    ret = WaitUntilMovementDone(); //Note that ret == 0 is fail. 
                }

                start_moving = false;
                MotH?.Invoke(this, new MotrEventArgs(""));

                return 0;
            }
            else
                return 0;
            //System.Threading.Thread.Sleep(50);
        }

        public double[] GetStatus() //pretty much just asking velocity.
        {
            if (connected)
            {
                thordll.GetParam(ThorParam.PARAM_X_VELOCITY_CURRENT, ref param);
                velocity[0] = param;
                thordll.GetParam(ThorParam.PARAM_Y_VELOCITY_CURRENT, ref param);
                velocity[1] = param;
                thordll.GetParam(ThorParam.PARAM_Z_VELOCITY_CURRENT, ref param);
                velocity[2] = param;
                return velocity;
            }
            else
                return null;
        }

        public void HardZero()
        {
            if (connected)
            {
                thordll.SetParam(ThorParam.PARAM_X_ZERO, 1);
                thordll.SetParam(ThorParam.PARAM_Y_ZERO, 1);
                thordll.SetParam(ThorParam.PARAM_Z_ZERO, 1);
            }
        }

        public void GetPosition()
        {
            if (connected)
            {
                param = 0;
                thordll.GetParam(ThorParam.PARAM_X_POS_CURRENT, ref param);
                XPos = param;

                param = 0;
                thordll.GetParam(ThorParam.PARAM_Y_POS_CURRENT, ref param);
                YPos = param;

                param = 0;
                thordll.GetParam(ThorParam.PARAM_Z_POS_CURRENT, ref param);
                ZPos = param;

                //getCalibratedAbsolutePosition(); //Actually not necessary... but anyway.
                //getCalibratedRelativePosition();

                e = new MotrEventArgs("");
                MotH?.Invoke(this, e);
            }
        }

        private int Move()
        {
            if (connected)
            {
                int ret;
                ret = thordll.PreflightPosition();
                if (ret == 0)
                    return ret;
                thordll.SetupPosition(); //Note that ret == 0 is fail. 
                if (ret == 0)
                    return ret;
                ret = thordll.StartPosition(); //Note that ret == 0 is fail. 
                if (ret == 0)
                    return ret;
                ret = thordll.PostflightPosition();
                return ret;
            }
            else
                return 0;
        }

        public int GetPMTStatus()
        {
            if (connected && Camera_path_available)
            {
                thordll.GetParam(ThorParam.PARAM_LIGHTPATH_CAMERA, ref param);
                if (param == 0)
                    return 1;
                else
                    return 0;
            }
            else
                return 0;
        }

        public int GetGGStatus()
        {
            if (connected && GG_light_path_available)
            {
                thordll.GetParam(ThorParam.PARAM_LIGHTPATH_GG, ref param);
                return (int)param;
            }
            else
                return 0;
        }

        public int GetGRStatus()
        {
            if (connected && GR_light_path_available)
            {
                thordll.GetParam(ThorParam.PARAM_LIGHTPATH_GR, ref param);
                return (int)param;
            }
            else
                return 0;
        }

        public void SwitchPMT(bool ON)
        {
            if (connected & Camera_path_available)
            {
                double pos = ON ? 0 : 1;
                thordll.SetParam(ThorParam.PARAM_LIGHTPATH_CAMERA, pos);
                Move();
            }
        }

        public void SwitchGG(bool ON)
        {
            if (connected)
            {
                double pos = ON ? 0 : 1;
                thordll.SetParam(ThorParam.PARAM_LIGHTPATH_GG, pos);
                Move();
            }
        }

        public void SwitchGR(bool ON)
        {
            if (connected)
            {
                double pos = ON ? 0 : 1;
                thordll.SetParam(ThorParam.PARAM_LIGHTPATH_GR, pos);
                Move();
            }
        }


        public void TimerEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            if (connected && !start_moving && !moving && continuous_readCheck)
                GetPosition();
        }
        

    } //ThorMCM3000

}
