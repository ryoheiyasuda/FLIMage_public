using MicroscopeHardwareLibs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FLIMage.HardwareControls.StageControls
{
    public class MotorCtrl_MP285A
    {
        public event MotorHandler MotH;
        public MotrEventArgs e = new MotrEventArgs("");
        public delegate void MotorHandler(MotorCtrl_MP285A mCtrls, MotrEventArgs e);

        public double maxDistanceXY = 500; //micrometers
        public double maxDistanceZ = 100;

        public int N_Coordinate = 3;

        public int XPos, YPos, ZPos;
        public int XNewPos, YNewPos, ZNewPos;
        public int XPosMov, YPosMov, ZPosMov;

        public int AllowError = 5;
        public SerialPort COMport;

        public int minMotorVal = (int)(-Math.Pow(2, 31) + 1);

        private bool start_moving = false;
        private bool moving = false;

        public double resolutionX; //=0.04     
        public double resolutionY; //=0.04
        public double resolutionZ; //=0.005

        public int velocity_fine;
        public int velocity_coarse;
        public int maxVelocity;
        public int minVelocity;

        public MotorCtrl.DeviceMode device_mode = MotorCtrl.DeviceMode.coarse;

        public bool freezing = false;
        public String tString;//

        SutterMP285 mp285;
        System.Timers.Timer ThoTimer;

        public bool reading = false;
        public bool connected = false;
        public bool continuous_readCheck = false;
        public int MotorDisplayUpdateTime_ms;


        public MotorCtrl_MP285A(String port, Double[] resolution, int velocity, bool crash_response, int MotorDisplayUpdateTime)
        {
            String COMport = port;

            MotorDisplayUpdateTime_ms = MotorDisplayUpdateTime;

            mp285 = new SutterMP285(COMport, 9600);

            if (mp285.OpenPort() == 0)
                return;

            mp285.crash_response = crash_response;
            connected = true;

            if (!mp285.crash_response)
                reset();


            XPos = 0;
            YPos = 0;
            ZPos = 0;

            XNewPos = XPos;
            YNewPos = YPos;
            ZNewPos = ZPos;

            velocity_fine = 100;
            velocity_coarse = velocity;
            maxVelocity = 10000;
            minVelocity = 100;

            resolutionX = resolution[0];
            resolutionY = resolution[1];
            resolutionZ = resolution[2];

            start_moving = false;
            moving = false;

            tString = "";

            GetPosition();

            if (velocity_coarse > minVelocity)
                SetVelocity(velocity_coarse);
            else
                SetVelocity(1500);


            freezing = false;
            //

            ThoTimer = new System.Timers.Timer(MotorDisplayUpdateTime_ms);
            ThoTimer.Elapsed += TimerEvent;
            ThoTimer.AutoReset = true;
            ThoTimer.Enabled = true;
        }

        public void unsubscribe()
        {

        }

        public void WaitUntilMovementDone()
        {
            if (!moving)
                return;

            for (int i = 0; i < 50; i++)
            {
                int[] position = new int[3];
                GetPosition();
                System.Threading.Thread.Sleep(100);
                if (Math.Abs(XPos - XNewPos) <= AllowError && Math.Abs(YPos - YNewPos) <= AllowError && Math.Abs(ZPos - ZNewPos) <= AllowError)
                {
                    break;
                }
                if (freezing)
                {
                    mp285.Stop();
                    break;
                }
            }
            //mp285.Stop();
            MovementDone();
        }


        public void reopen()
        {
            mp285.ClosePort();
            mp285.OpenPort();
            reset();
            System.Threading.Thread.Sleep(100);
            SetVelocity(velocity_coarse);
        }

        public void reset()
        {
            mp285.Reset();
        }

        public double[] GetResolution()
        {
            return new double[] { resolutionX, resolutionY, resolutionZ };
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


        public void SetVelocity(int val)
        {
            bool findMode = (device_mode == MotorCtrl.DeviceMode.fine);
            mp285.SetVelocity(val, findMode);
            System.Threading.Thread.Sleep(500);
            GetStatus();
            GetPosition();
        }

        public void GetStatus()
        {
            reading = true;
            bool fineMode = (device_mode == MotorCtrl.DeviceMode.fine);
            int vel;
            if (fineMode)
                vel = velocity_fine;
            else
                vel = velocity_coarse;
            int success = mp285.GetVelocity(ref vel, ref fineMode);
            tString = mp285.StringCode;
            if (success == -1)
            {
                FreezeResponse();
            }
            else
            {
                success = mp285.GetVelocity(ref vel, ref fineMode);
            }

            if (success == 1)
            {
                if (fineMode)
                {
                    device_mode = MotorCtrl.DeviceMode.fine;
                    velocity_fine = vel;
                }
                else
                {
                    device_mode = MotorCtrl.DeviceMode.coarse;
                    velocity_coarse = vel;
                }

                e = new MotrEventArgs("Status");
                MotH?.Invoke(this, e);
            }
            reading = false;
        }

        public void SetNewPosition(double[] XYZ)
        {
            XNewPos = (int)XYZ[0];
            YNewPos = (int)XYZ[1];
            ZNewPos = (int)XYZ[2];
        }


        public void SetPosition()
        {
            start_moving = true;
            setPosition_internal();
        }

        public void MovementDone()
        {
            e = new MotrEventArgs("MovementDone");
            start_moving = false;
            moving = false;
            GetPosition();
        }

        public int setPosition_internal()
        {

            if (Math.Abs(XPos - XNewPos) < AllowError && Math.Abs(YPos - YNewPos) < AllowError && Math.Abs(ZPos - ZNewPos) < AllowError)
            {
                MovementDone();
                return 0;
            }

            moving = true;
            start_moving = false;

            int retcode = mp285.GoToPosition(new int[] { XNewPos, YNewPos, ZNewPos });
            if (retcode == 1)
            {
                freezing = false;
                MovementDone();
                return 0;
            }


            XPosMov = XPos;
            YPosMov = YPos;
            ZPosMov = ZPos;

            WaitUntilMovementDone(); //Does not do anything.

            return 0;
        }

        public void FreezeResponse()
        {
            freezing = true;
            if (mp285.crash_response)
                e = new MotrEventArgs("FreezeA");
            else
                e = new MotrEventArgs("Freeze");
            MotH?.Invoke(this, e);
        }


        public void GetPosition()
        {
            reading = true;
            int[] pos = new int[3];
            int success = mp285.GetPosition(pos);
            tString = mp285.StringCode;

            if (success == 1 && !pos.All(x => x == 0))
            {
                freezing = false;
                XPos = pos[0];
                YPos = pos[1];
                ZPos = pos[2];

                if (e.Name == "Freeze" || e.Name == "FreezeA")
                    e.Name = "GetPositionDone";

                MotH?.Invoke(this, e);
            }
            else if (success == -1)
            {
                FreezeResponse();
            }
            else
            {
                //MessageBox.Show("Position reading failed!");
            }
            reading = false;
        }

        public void TimerEvent(object source, System.Timers.ElapsedEventArgs e1)
        {
            if (!start_moving && !moving && continuous_readCheck && !reading)
            {
                GetPosition();
            }
            else if (moving)
            {
                e = new MotrEventArgs("Moving");
                MotH?.Invoke(this, e);
            }
        }


    } //motorCtrl

}
