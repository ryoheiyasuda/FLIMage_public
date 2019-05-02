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
using FLIMage.HardwareControls.ThorLabs;

namespace FLIMage.HardwareControls.StageControls
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
        public double minMovZ = 0.2;
        //public SerialPort port;

        public double[] maxPos = new double[3];
        public double[] minPos = new double[3];

        public int minMotorVal = (int)(-Math.Pow(2, 31) + 1);

        public bool start_moving, moving;

        public double resolutionX; //=0.04     
        public double resolutionY; //=0.04
        public double resolutionZ; //=0.005

        public double[] velocity = new double[3];
        public double[] maxVelocity = new double[3];
        public double[] minVelocity = new double[3];

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

        public const int TRUE = 1, FALSE = 0;
        ThorLabs.ThorDLL thordll;

        public ThorMCMX000(double[] resolution, MotorCtrl.MotorTypeEnum motor_type)
        {
            lockPort = new object();

            if (motor_type == MotorCtrl.MotorTypeEnum.thorMCM3000)
                thordll = ThorDLL.ThorDLL_Load(ThorLabs.ThorDLL.DLLType.ThorMCM3000);
            else
                thordll = ThorDLL.ThorDLL_Load(ThorLabs.ThorDLL.DLLType.ThorBScope);

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

            if (paramAvailable == 1)
            {
                XMax = paramMax;
                XMin = paramMin;
            }

            if (thordll.GetParamInfo(ThorParam.PARAM_Y_POS, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) != TRUE)
            {
                MessageBox.Show("ThorMCMX000.GetParamInfo(PARAM_Y_POS) failed: Failed to connect with the device. Check the device status!");
                return;
            }

            if (paramAvailable == 1)
            {
                YMax = paramMax;
                YMin = paramMin;
            }


            if (thordll.GetParamInfo(ThorParam.PARAM_Z_POS, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) != TRUE)
            {
                MessageBox.Show("ThorMCMX000.GetParamInfo(PARAM_Z_POS) failed: Failed to connect with the device. Check the device status!");
                return;
            }

            if (paramAvailable == 1)
            {
                ZMax = paramMax;
                ZMin = paramMin;
            }


            GetVelocityLimits();

            resolutionX = resolution[0];
            resolutionY = resolution[1];
            resolutionZ = resolution[2];

            GetPosition(true, false);
            velocity = GetStatus(true);

            start_moving = false;
            moving = false;



            tString = "";
            freezing = false;

            ThoTimer = new System.Timers.Timer(300);
            ThoTimer.Elapsed += TimerEvent;
            ThoTimer.AutoReset = true;

            ThoTimer.Enabled = true;
        }


        public double[] GetResolution()
        {
            return new double[] { resolutionX, resolutionY, resolutionZ };
        }

        public void unsubscribe()
        {
            //ThoTimer.Elapsed -= TimerEvent;
        }

        public void reopen()
        {

        }

        public void WaitUntilAllTaskDone()
        {
            WaitUntilMovementDone();
        }

        public int WaitUntilMovementDone()
        {
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

        public void GetVelocityLimits()
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
            start_moving = true;
            setPosition_internal();
        }


        public int setPosition_internal()
        {
            WaitUntilAllTaskDone();

            moving = true;
            int ret = 1;

            if (Math.Abs(XNewPos - XPos) > minMovX / resolutionX)
            {
                thordll.SetParam(ThorParam.PARAM_X_POS, XNewPos);
                ret = thordll.PreflightPosition();
                ret = thordll.SetupPosition(); //Note that ret == 0 is fail. 
                ret = thordll.StartPosition(); //Note that ret == 0 is fail. 
                ret = WaitUntilMovementDone(); //Note that ret == 0 is fail. 
            }

            if (Math.Abs(YNewPos - YPos) > minMovY / resolutionY)
            {
                thordll.SetParam(ThorParam.PARAM_Y_POS, YNewPos);
                ret = thordll.PreflightPosition();
                ret = thordll.SetupPosition(); //Note that ret == 0 is fail. 
                ret = thordll.StartPosition(); //Note that ret == 0 is fail. 
                ret = WaitUntilMovementDone(); //Note that ret == 0 is fail. 
            }

            if (Math.Abs(ZNewPos - ZPos) > minMovZ / resolutionZ)
            {
                thordll.SetParam(ThorParam.PARAM_Z_POS, ZNewPos);
                ret = thordll.PreflightPosition();
                ret = thordll.SetupPosition(); //Note that ret == 0 is fail. 
                ret = thordll.StartPosition(); //Note that ret == 0 is fail. 
                ret = WaitUntilMovementDone(); //Note that ret == 0 is fail. 
            }

            start_moving = false;
            MotH?.Invoke(this, new MotrEventArgs(""));

            return 0;
            //System.Threading.Thread.Sleep(50);
        }

        public double[] GetStatus(bool block) //pretty much just asking velocity.
        {
            thordll.GetParam(ThorParam.PARAM_X_VELOCITY_CURRENT, ref param);
            velocity[0] = param;
            thordll.GetParam(ThorParam.PARAM_Y_VELOCITY_CURRENT, ref param);
            velocity[1] = param;
            thordll.GetParam(ThorParam.PARAM_Z_VELOCITY_CURRENT, ref param);
            velocity[2] = param;
            return velocity;
        }

        public void GetPosition(bool block, bool duringMovement)
        {
            thordll.GetParam(ThorParam.PARAM_X_POS_CURRENT, ref param);
            XPos = param;
            thordll.GetParam(ThorParam.PARAM_Y_POS_CURRENT, ref param);
            YPos = param;
            thordll.GetParam(ThorParam.PARAM_Z_POS_CURRENT, ref param);
            ZPos = param;

            //getCalibratedAbsolutePosition(); //Actually not necessary... but anyway.
            //getCalibratedRelativePosition();

            e = new MotrEventArgs("");
            MotH?.Invoke(this, e);
        }

        public void TimerEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            if (!start_moving && !moving && continuous_read)
                GetPosition(true, false);
        }
        

    } //ThorMCM3000

}
