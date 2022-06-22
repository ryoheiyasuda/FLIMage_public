using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroscopeHardwareLibs.Stage_Contoller
{
    public class MotorCtrl_ThorMCM3001
    {
        public event MotorHandler MotH;
        public MotrEventArgs e = new MotrEventArgs("");
        public delegate void MotorHandler(MotorCtrl_ThorMCM3001 mCtrls, MotrEventArgs e);

        public double maxDistanceXY = 500; //micrometers
        public double maxDistanceZ = 100;

        public int N_Coordinate = 3;

        public double XPos, YPos, ZPos;
        public double XNewPos, YNewPos, ZNewPos;
        public double XPosMov, YPosMov, ZPosMov;

        public double tolerance = 0.05; //in um

        public int minMotorVal = (int)(-Math.Pow(2, 31) + 1);

        private bool start_moving = false;
        private bool moving = false;

        public bool forceStop = false;

        public double resolutionX; //=0.04     
        public double resolutionY; //=0.04
        public double resolutionZ; //=0.005

        public int velocity_fine;
        public int velocity_coarse;
        public int maxVelocity;
        public int minVelocity;

        int timeout = 20; //millisecond
        int total_timeout = 2000;

        public MotorCtrl.DeviceMode device_mode;

        public bool freezing = false;
        public String tString;

        ThorMCM3000 thorMCM3000;
        System.Timers.Timer ThoTimer;

        public bool reading = false;
        public bool connected = false;
        public bool continuous_readCheck = true;

        public int MotorDisplayUpdateTime_ms = 300;

        public MotorCtrl_ThorMCM3001(string port, Double[] resolution, String typeString, int MotorDisplayUpdateTime)
        {
            String COMport = port; //Standard is COM32.
            int bout = 115200;
            var motorType = ThorMCM3000.Thor3000Type.MCM3001;
            if (typeString.Contains("3002"))
                motorType = ThorMCM3000.Thor3000Type.MCM3002;
            else if (typeString.Contains("3003"))
                motorType = ThorMCM3000.Thor3000Type.MCM3003;
            else if (typeString.Contains("BScope"))
            {
                bout = 9600;
                motorType = ThorMCM3000.Thor3000Type.BScope;
            }
            else if (typeString.Contains("Bergamo"))
            {
                bout = 9600;
                motorType = ThorMCM3000.Thor3000Type.Bergamo;
            }

            MotorDisplayUpdateTime_ms = MotorDisplayUpdateTime;
            thorMCM3000 = new ThorMCM3000(COMport, bout, motorType);

            if (thorMCM3000.OpenPort() == 0)
                return;

            connected = true;
            XPos = 0;
            YPos = 0;
            ZPos = 0;

            XNewPos = XPos;
            YNewPos = YPos;
            ZNewPos = ZPos;

            velocity_fine = 100;
            velocity_coarse = 1000;
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
            ThoTimer.Elapsed -= TimerEvent;
        }

        public int WaitUntilMovementDone()
        {
            if (!moving)
                return 1;

            for (int i = 0; i < total_timeout / timeout; i++)
            {
                System.Threading.Thread.Sleep(timeout);
                moving = thorMCM3000.IsMotorBusy(0) || thorMCM3000.IsMotorBusy(1) || thorMCM3000.IsMotorBusy(2);

                e.Name = moving ? "Moving" : "MovementDone";
                GetPosition();

                if (forceStop)
                {
                    Stop();
                    break;
                }

                if (!moving)
                {
                    return 1;
                }
                else
                {
                    Application.DoEvents(); //This is necessary for port to receive the event!!                    
                }
            }

            return 0;
        }

        public void disconnect()
        {
            ThoTimer.Close();
            ThoTimer.Dispose();
            connected = false;
            thorMCM3000.ClosePort();
        }

        public void reopen()
        {
            if (connected)
                thorMCM3000.ClosePort();
            connected = false;

            if (thorMCM3000.OpenPort() == 1)
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

        }


        public void SetNewPosition(double[] XYZ)
        {
            XNewPos = XYZ[0];
            YNewPos = XYZ[1];
            ZNewPos = XYZ[2];
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
            if (Math.Abs(XPos - XNewPos) < tolerance / resolutionX && Math.Abs(YPos - YNewPos) < tolerance / resolutionY && Math.Abs(ZPos - ZNewPos) < tolerance / resolutionZ)
            {
                MovementDone();
                return 0;
            }

            moving = true;
            start_moving = false;

            XPosMov = XPos;
            YPosMov = YPos;
            ZPosMov = ZPos;

            int timesTrials = 1;
            for (int i = 0; i < timesTrials; i++)
            {
                if (Math.Abs(XPos - XNewPos) > tolerance / resolutionX)
                {
                    if (thorMCM3000.GoToPosition(0, XNewPos) == 0)
                    {
                        MessageBox.Show("out of range for X movement");
                        MovementDone();
                        return 0;
                    }
                }

                if (Math.Abs(YPos - YNewPos) > tolerance / resolutionY)
                {
                    if (thorMCM3000.GoToPosition(1, YNewPos) == 0)
                    {
                        MessageBox.Show("out of range for Y movement");
                        MovementDone();
                        return 0;
                    }
                }

                if (Math.Abs(ZPos - ZNewPos) > tolerance / resolutionZ)
                {
                    if (thorMCM3000.GoToPosition(2, ZNewPos) == 0)
                    {
                        MessageBox.Show("out of range for Z movement");
                        MovementDone();
                        return 0;
                    }
                }

                var success = WaitUntilMovementDone();

                if (success == 0)
                    thorMCM3000.Stop();
            }

            MovementDone();
            return 1;
        }

        public void HardZero()
        {
            thorMCM3000.ZeroPosition(0);
            thorMCM3000.ZeroPosition(1);
            thorMCM3000.ZeroPosition(2);
        }

        public void Stop()
        {
            forceStop = true;
            thorMCM3000.Stop();
            XNewPos = XPos;
            YNewPos = YPos;
            ZNewPos = ZPos;
        }


        public void GetPosition()
        {
            reading = true;
            double[] pos = new double[3];
            int success = thorMCM3000.GetPosition(pos);
            tString = thorMCM3000.StringCode;

            if (success == 1)
            {
                XPos = pos[0];
                YPos = pos[1];
                ZPos = pos[2];
                MotH?.Invoke(this, e);
            }
            else
            {
                Debug.WriteLine("Position reading failed!");
            }
            reading = false;
        }

        public void TimerEvent(object source, System.Timers.ElapsedEventArgs e1)
        {
            if (!start_moving && !moving && continuous_readCheck && !reading)
                GetPosition();
            else if (moving)
            {
                e = new MotrEventArgs("Moving");
                MotH?.Invoke(this, e);
            }
        }


    } //motorCtrl

}
