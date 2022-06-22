using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroscopeHardwareLibs.Stage_Contoller
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

        public int tolerance = 5; //
        public SerialPort COMport;

        public int minMotorVal = (int)(-Math.Pow(2, 31) + 1);

        private bool start_moving = false;
        private bool moving = false;

        public double resolutionX; //=0.04     
        public double resolutionY; //=0.04
        public double resolutionZ; //=0.005

        public int velocity;
        public int maxVelocity;
        public int minVelocity;

        public MotorCtrl.DeviceMode device_mode = MotorCtrl.DeviceMode.fine;

        public bool forceStop = false;

        public bool freezing = false;
        public String tString;//

        public SutterMP285 mp285;
        System.Timers.Timer ThoTimer;

        public bool reading = false;
        public bool connected = false;
        public bool continuous_readCheck = false;
        public int MotorDisplayUpdateTime_ms;

        public MotorCtrl_MP285A(String port, Double[] resolution, int _velocity, bool crash_response,
            int MotorDisplayUpdateTime)
        {
            constructor(port, resolution, _velocity, crash_response,
                MotorDisplayUpdateTime, MotorCtrl.MotorTypeEnum.mp285a);
        }

        public MotorCtrl_MP285A(String port, Double[] resolution, int _velocity, bool crash_response,
            int MotorDisplayUpdateTime, MotorCtrl.MotorTypeEnum motor_type)
        {
            constructor(port, resolution, _velocity, crash_response, MotorDisplayUpdateTime, motor_type);
        }

        public void constructor(String port, Double[] resolution, int _velocity, bool crash_response,
            int MotorDisplayUpdateTime, MotorCtrl.MotorTypeEnum motor_type)
        {
            String COMport = port;

            MotorDisplayUpdateTime_ms = MotorDisplayUpdateTime;

            string name = Enum.GetName(typeof(MotorCtrl.MotorTypeEnum), motor_type);
            mp285 = new SutterMP285(COMport, 9600, name);

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

            velocity = _velocity;
            maxVelocity = 10000;
            minVelocity = 40;

            resolutionX = resolution[0];
            resolutionY = resolution[1];
            resolutionZ = resolution[2];

            start_moving = false;
            moving = false;

            tString = "";

            GetPosition();

            if (velocity >= minVelocity)
                SetVelocity(velocity);
            else
                SetVelocity(1500);

            freezing = false;
            //
            TypeSpecificCommand();

            ThoTimer = new System.Timers.Timer(MotorDisplayUpdateTime_ms);
            ThoTimer.Elapsed += TimerEvent;
            ThoTimer.AutoReset = true;
            ThoTimer.Enabled = true;
        }

        public virtual void TypeSpecificCommand()
        {

        }

        public void unsubscribe()
        {
            ThoTimer.Elapsed -= TimerEvent;
        }

        public void WaitUntilMovementDone()
        {
            if (!moving)
                return;

            int[] PreMotorPosition = new int[] { XPos, YPos, ZPos };

            for (int i = 0; i < 10000; i++)
            {
                System.Threading.Thread.Sleep(50);

                int[] position = new int[3];
                GetPosition();
                if (Math.Abs(XPos - XNewPos) <= tolerance && Math.Abs(YPos - YNewPos) <= tolerance && Math.Abs(ZPos - ZNewPos) <= tolerance)
                {
                    break;
                }

                if (XPos == PreMotorPosition[0] && YPos == PreMotorPosition[1] && ZPos == PreMotorPosition[2])
                {
                    freezing = true;
                }

                if (freezing || forceStop)
                {
                    Stop();
                    break;
                }
            }
            //mp285.Stop();
            MovementDone();
        }

        public void Stop()
        {
            forceStop = true;
            GetPosition();
            XNewPos = XPos;
            YNewPos = YPos;
            ZNewPos = ZPos;
            mp285.Stop();
        }

        public void disconnect()
        {
            ThoTimer.Close();
            ThoTimer.Dispose();
            connected = false;
            mp285.ClosePort();
        }

        public void reopen()
        {
            if (connected)
            {
                connected = false;
                mp285.ClosePort();
            }

            if (mp285.OpenPort() == 1)
                connected = true;

            if (connected)
            {
                ThoTimer = new System.Timers.Timer(MotorDisplayUpdateTime_ms);
                ThoTimer.Elapsed += TimerEvent;
                ThoTimer.AutoReset = true;
                ThoTimer.Enabled = true;
            }            
        }

        public void reset()
        {
            mp285.Reset();
        }


        public int HardZero()
        {
            return mp285.SetHardZero();
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
            bool fineMode = (device_mode == MotorCtrl.DeviceMode.fine);
            mp285.SetVelocity(val, fineMode);
            System.Threading.Thread.Sleep(100);
            GetStatus();
            GetPosition();
        }

        public void GetStatus()
        {
            reading = true;
            bool fineMode = (device_mode == MotorCtrl.DeviceMode.fine);
            int vel = velocity;

            int success = mp285.GetVelocity(ref vel, ref fineMode);
            tString = mp285.StringCode;

            if (success == -1)
            {
                FreezeResponse();
            }

            if (success == 1)
            {
                if (fineMode)
                {
                    device_mode = MotorCtrl.DeviceMode.fine;
                    velocity = vel;
                }
                else
                {
                    device_mode = MotorCtrl.DeviceMode.coarse;
                    velocity = vel * 5;
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
            forceStop = false;
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

            if (Math.Abs(XPos - XNewPos) < tolerance && Math.Abs(YPos - YNewPos) < tolerance && Math.Abs(ZPos - ZNewPos) < tolerance)
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
            if (connected)
            {
                if (!start_moving && !moving && continuous_readCheck && !reading)
                {
                    GetPosition();
                    MotH?.Invoke(this, e);

                    GetStatus();
                    MotH?.Invoke(this, e);
                }
                else if (moving)
                {
                    e = new MotrEventArgs("Moving");
                    MotH?.Invoke(this, e);
                }
            }
        }


    } //motorCtrl

}
