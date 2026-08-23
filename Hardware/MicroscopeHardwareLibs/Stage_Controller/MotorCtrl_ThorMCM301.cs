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
    public class MotorCtrl_ThorMCM301
    {
        public event MotorHandler MotH;
        public MotrEventArgs e = new MotrEventArgs("");
        public delegate void MotorHandler(MotorCtrl_ThorMCM301 mCtrls, MotrEventArgs e);

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

        ThorMCM301 thorMCM301;
        System.Timers.Timer ThoTimer;

        public bool reading = false;
        public bool connected = false;
        public bool continuous_readCheck = true;

        public int MotorDisplayUpdateTime_ms = 300;

        public MotorCtrl_ThorMCM301(string port, String typeString, int MotorDisplayUpdateTime)
        {
            String COMport = port; //Standard is COM32.
            int bout = 115200;

            MotorDisplayUpdateTime_ms = MotorDisplayUpdateTime;
            thorMCM301 = new ThorMCM301(COMport, bout);

            if (thorMCM301.OpenPort() < 0)
                return;

            connected = true;
            XPos = 0;
            YPos = 0;
            ZPos = 0;

            XNewPos = XPos;
            YNewPos = YPos;
            ZNewPos = ZPos;

            velocity_fine = 100;
            velocity_coarse = 100;
            maxVelocity = 100;
            minVelocity = 0;

            resolutionX = thorMCM301.conversionFactor[0] / 1000; // um per encoder // resolution[0];
            resolutionY = thorMCM301.conversionFactor[1] / 1000;  // resolution[1];
            resolutionZ = thorMCM301.conversionFactor[2] / 1000;  // resolution[2];

            start_moving = false;
            moving = false;

            tString = "";

            GetPosition();

            if (velocity_coarse > minVelocity)
                SetVelocity(velocity_coarse);
            else
                SetVelocity(100);

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
                moving = thorMCM301.IsMotorBusy(0) || thorMCM301.IsMotorBusy(1) || thorMCM301.IsMotorBusy(2);

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
            thorMCM301.ClosePort();
        }

        public void reopen()
        {
            if (connected)
                thorMCM301.ClosePort();
            connected = false;

            if (thorMCM301.OpenPort() == 1)
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
            thorMCM301.SetVelocity(val);
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
            MotH?.Invoke(this, new MotrEventArgs("MovementDone"));
            start_moving = false;
            moving = false;
            GetPosition();
        }

        public int setPosition_internal()
        {
            if (Math.Abs(XPos - XNewPos) < resolutionX * 1000 && Math.Abs(YPos - YNewPos) < resolutionY * 1000 && Math.Abs(ZPos - ZNewPos) < resolutionZ * 1000)
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
                if (Math.Abs(XPos - XNewPos) > resolutionX * 1000)
                {
                    if (thorMCM301.GoToPosition(0, XNewPos) == 0)
                    {
                        MessageBox.Show("out of range for X movement");
                        MovementDone();
                        return 0;
                    }
                }

                if (Math.Abs(YPos - YNewPos) > resolutionX * 1000)
                {
                    if (thorMCM301.GoToPosition(1, YNewPos) == 0)
                    {
                        MessageBox.Show("out of range for Y movement");
                        MovementDone();
                        return 0;
                    }
                }

                if (Math.Abs(ZPos - ZNewPos) > resolutionX * 1000)
                {
                    if (thorMCM301.GoToPosition(2, ZNewPos) == 0)
                    {
                        MessageBox.Show("out of range for Z movement");
                        MovementDone();
                        return 0;
                    }
                }

                var success = WaitUntilMovementDone();

                if (success == 0)
                    thorMCM301.Stop();
            }

            MovementDone();
            return 1;
        }

        public void HardZero()
        {
            thorMCM301.ZeroPosition(0);
            thorMCM301.ZeroPosition(1);
            thorMCM301.ZeroPosition(2);
        }

        public void Stop()
        {
            forceStop = true;
            thorMCM301.Stop();
            XNewPos = XPos;
            YNewPos = YPos;
            ZNewPos = ZPos;
        }


        public void GetPosition()
        {
            reading = true;
            double[] pos = new double[3];
            int success = thorMCM301.GetPosition(pos);

            if (success == 1)
            {
                XPos = pos[0];
                YPos = pos[1];
                ZPos = pos[2];
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
            {
                GetPosition();
                MotH?.Invoke(this, new MotrEventArgs(""));
            }
            else if (moving)
            {
                MotH?.Invoke(this, new MotrEventArgs("Moving"));
            }
        }


    } //motorCtrl

}
