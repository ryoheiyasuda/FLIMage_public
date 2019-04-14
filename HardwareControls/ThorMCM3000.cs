using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace Stage_Control
{
    public class ThorMCM3000
    {
        private const int PARAM_DEVICE_TYPE = 0;
        private const int PARAM_X_ACCEL = 205;
        private const int PARAM_X_DECEL = 206;
        private const int PARAM_X_HOME = 201;
        private const int PARAM_X_JOYSTICK_VELOCITY = 208;
        private const int PARAM_X_POS = 200;
        private const int PARAM_X_POS_CURRENT = 207;
        private const int PARAM_X_STEPS_PER_MM = 204;
        private const int PARAM_X_STOP = 209;
        private const int PARAM_X_VELOCITY = 203;
        private const int PARAM_X_VELOCITY_CURRENT = 210;
        private const int PARAM_X_ZERO = 202;
        private const int PARAM_Y_ACCEL = 305;
        private const int PARAM_Y_DECEL = 306;
        private const int PARAM_Y_HOME = 301;
        private const int PARAM_Y_JOYSTICK_VELOCITY = 308;
        private const int PARAM_Y_POS = 300;
        private const int PARAM_Y_POS_CURRENT = 307;
        private const int PARAM_Y_STEPS_PER_MM = 304;
        private const int PARAM_Y_STOP = 309;
        private const int PARAM_Y_VELOCITY = 303;
        private const int PARAM_Y_VELOCITY_CURRENT = 310;
        private const int PARAM_Y_ZERO = 302;
        private const int PARAM_Z_ACCEL = 405;
        private const int PARAM_Z_DECEL = 406;
        private const int PARAM_Z_HOME = 401;
        private const int PARAM_Z_JOYSTICK_VELOCITY = 408;
        private const int PARAM_Z_POS = 400;
        private const int PARAM_Z_POS_CURRENT = 407;
        private const int PARAM_Z_STEPS_PER_MM = 404;
        private const int PARAM_Z_STOP = 409;
        private const int PARAM_Z_VELOCITY = 403;
        private const int PARAM_Z_VELOCITY_CURRENT = 410;
        private const int PARAM_Z_ZERO = 402;
        private const int STAGE_X = 0x00000004;
        private const int STAGE_Y = 0x00000008;
        private const int STAGE_Z = 0x00000010;
        private const int TRUE = 1, FALSE = 0;


        public event MotorHandler MotH;
        public MotrEventArgs e = new MotrEventArgs("");
        public delegate void MotorHandler(ThorMCM3000 mCtrls, MotrEventArgs e);

        public double maxDistanceXY = 500; //micrometers
        public double maxDistanceZ = 100;

        public int N_Coordinate = 3;

        public double XPos, YPos, ZPos;
        public double XNewPos, YNewPos, ZNewPos;
        public double XPosMov, YPosMov, ZPosMov;
        //public SerialPort port;

        private double[] maxPos = new double[3];
        private double[] minPos = new double[3];

        public int minMotorVal = (int)(-Math.Pow(2, 31) + 1);

        private bool start_moving, moving;

        public double resolutionX; //=0.04     
        public double resolutionY; //=0.04
        public double resolutionZ; //=0.005

        public double[] velocity = new double[3];
        public double[] maxVelocity = new double[3];
        public double[] minVelocity = new double[3];

        public double velVal;

        private Stopwatch sw = new Stopwatch();

        //public int ZStackStart_rel, ZStack_End_rel;
        public MotorCtrl.DeviceMode device_mode;

        public bool freezing = false;

        public bool continuous_read = true;
        public bool continuous_readCheck = true;

        object lockPort;

        public String tString;//

        System.Timers.Timer ThoTimer;

        //Buffer for getinformation.
        int paramType = 0;
        int paramAvailable = 0;
        int paramReadOnly = 0;
        double paramMin = 0.0;
        double paramMax = 0.0;
        double paramDefault = 0.0;
        double param = 0.0;

        public ThorMCM3000()
        {
            lockPort = new object();

            SelectDevice(0);
            if (GetParamInfo(PARAM_DEVICE_TYPE, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault) != TRUE)
                return;

            int targetDevType = (int)(STAGE_X | STAGE_Y | STAGE_Z);
            if (((int)(paramDefault) & targetDevType) != targetDevType)
                return;

            GetVelocityLimits();

            resolutionX = 1;
            resolutionY = 1;
            resolutionZ = 1;

            GetPosition(true, false);

            start_moving = false;
            moving = false;



            tString = "";
            freezing = false;

            ThoTimer = new System.Timers.Timer(500);
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
            ThoTimer.Elapsed -= TimerEvent;
        }

        public void reopen()
        {

        }

        public void WaitUntilAllTaskDone()
        {
            WaitUntilMovementDone();
        }

        public void WaitUntilMovementDone()
        {
            int status = 0;
            do
            {
                StatusPosition(ref status);
            }
            while (status == 0);

            moving = false;

            if (PostflightPosition() == FALSE)
            {
                Debug.WriteLine("Failed mvoement");
            }
        }

        public void GetVelocityLimits()
        {

            GetParamInfo(PARAM_X_VELOCITY, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault);
            if (paramAvailable == TRUE)
            {
                velocity[0] = paramDefault;
                maxVelocity[0] = paramMax;
                minVelocity[0] = paramMin;
            }
            GetParamInfo(PARAM_Y_VELOCITY, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault);
            if (paramAvailable == TRUE)
            {
                velocity[1] = paramDefault;
                maxVelocity[1] = paramMax;
                minVelocity[1] = paramMin;
            }
            GetParamInfo(PARAM_Z_VELOCITY, ref paramType, ref paramAvailable, ref paramReadOnly, ref paramMin, ref paramMax, ref paramDefault);
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
            XNewPos = XYZ[0];
            YNewPos = XYZ[1];
            ZNewPos = XYZ[2];
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
            //            this.Enabled = false;

            if (XNewPos != XPos)
                SetParam(PARAM_X_POS, XNewPos);

            if (YNewPos != YPos)
                SetParam(PARAM_Y_POS, YNewPos);

            if (ZNewPos != ZPos)
                SetParam(PARAM_Z_POS, ZNewPos);

            start_moving = false;

            XPosMov = XPos;
            YPosMov = YPos;
            ZPosMov = ZPos;


            WaitUntilMovementDone();


            e = new MotrEventArgs("");
            MotH?.Invoke(this, e);

            return 0;
            //System.Threading.Thread.Sleep(50);
        }

        public double[] GetStatus(bool block) //pretty much just asking velocity.
        {
            GetParam(PARAM_X_VELOCITY_CURRENT, ref param);
            if (paramAvailable == TRUE)
            {
                velocity[0] = param;
            }
            GetParam(PARAM_Y_VELOCITY_CURRENT, ref param);
            if (paramAvailable == TRUE)
            {
                velocity[1] = param;
            }
            GetParam(PARAM_Z_VELOCITY_CURRENT, ref param);
            if (paramAvailable == TRUE)
            {
                velocity[2] = param;
            }

            return velocity;
        }

        public void GetPosition(bool block, bool duringMovement)
        {
            GetParam(PARAM_X_POS_CURRENT, ref param);
            if (paramAvailable == TRUE)
            {
                XPos = param;
            }
            GetParam(PARAM_Y_POS_CURRENT, ref param);
            if (paramAvailable == TRUE)
            {
                YPos = param;
            }
            GetParam(PARAM_Z_POS_CURRENT, ref param);
            if (paramAvailable == TRUE)
            {
                ZPos = param;
            }

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
        


        [DllImport("ThorMCM3000.dll", EntryPoint = "GetParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetParam(int paramID, ref double param);

        [DllImport("ThorMCM3000.dll", EntryPoint = "GetParamInfo", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetParamInfo(int paramID, ref int paramType, ref int paramAvailable, ref int paramReadOnly, ref double paramMin, ref double paramMax, ref double paramDefault);

        [DllImport("ThorMCM3000.dll", EntryPoint = "PostflightPosition", CallingConvention = CallingConvention.Cdecl)]
        private static extern int PostflightPosition();

        [DllImport("ThorMCM3000.dll", EntryPoint = "PreflightPosition", CallingConvention = CallingConvention.Cdecl)]
        private static extern int PreflightPosition();

        [DllImport("ThorMCM3000.dll", EntryPoint = "SelectDevice", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SelectDevice(int device);

        [DllImport("ThorMCM3000.dll", EntryPoint = "SetParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetParam(int paramID, double param);

        [DllImport("ThorMCM3000.dll", EntryPoint = "SetupPosition", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetupPosition();

        [DllImport("ThorMCM3000.dll", EntryPoint = "StartPosition", CallingConvention = CallingConvention.Cdecl)]
        private static extern int StartPosition();

        [DllImport("ThorMCM3000.dll", EntryPoint = "StatusPosition", CallingConvention = CallingConvention.Cdecl)]
        private static extern int StatusPosition(ref int status);

    } //ThorMCM3000

}
