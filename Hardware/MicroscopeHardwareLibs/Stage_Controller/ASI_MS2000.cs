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
    public class ASI_MS2000
    {

        public event MotorHandler MotH;
        public MotrEventArgs e = new MotrEventArgs("");
        public delegate void MotorHandler(ASI_MS2000 mCtrls, MotrEventArgs e);

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

        public bool moving;

        public double resolutionX = 0.1;
        public double resolutionY = 0.1;
        public double resolutionZ = 0.1;

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

        public const int TRUE = 1, FALSE = 0;
        public bool connected = false;
        public int MotorDisplayUpdateTime_ms = 300;

        public bool reading = false;

        public double tolerance = 2; //0.1 um

        SerialCommandObj asi_serial;

        public ASI_MS2000(String port, int MotorDisplayUpdateTime)
        {

            MotorDisplayUpdateTime_ms = MotorDisplayUpdateTime;


            asi_serial = new SerialCommandObj(port, 9600, "ASI_MS2000");
            asi_serial.newLineChar = '\n';

            if (asi_serial.OpenPort() == 0)
                return;

            connected = true;

            XPos = 0;
            YPos = 0;
            ZPos = 0;

            XNewPos = XPos;
            YNewPos = YPos;
            ZNewPos = ZPos;

            moving = false;

            tString = "";

            GetPosition();
            // Sync commanded positions so that the first step move does not target (0,0,0).
            XNewPos = XPos;
            YNewPos = YPos;
            ZNewPos = ZPos;

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

        /// <summary>
        /// Continue to read position until it goes to target.
        /// </summary>
        /// <returns>Return 0 when done.</returns>
        public int WaitUntilMovementDone()
        {
            if (!moving)
            {
                return 1;
            }

            for (int i = 0; i < 10000; i++)
            {
                System.Threading.Thread.Sleep(25);

                // This is a straight forward way for MS-2000
                var success = asi_serial.SendAndWait("STATUS \r", 0, out byte[] ret);
                var str1 = asi_serial.returnString.Replace("\r", "");
                
                if (str1=="N")
                {
                    break;
                }

                // until here

                if (forceStop)
                {
                    Stop();
                    break;
                }
            }
            MovementDone();

            return 0;
        }

        public void Stop()
        {
            forceStop = true;
            GetPosition();
            XNewPos = XPos;
            YNewPos = YPos;
            ZNewPos = ZPos;
            asi_serial.SendAndWait("\\\r", 0, out byte[] ret);
        }

        public void disconnect()
        {
            ThoTimer.Close();
            ThoTimer.Dispose();
            connected = false;
            asi_serial.ClosePort();
        }

        public void reopen()
        {
            if (connected)
            {
                connected = false;
                asi_serial.ClosePort();
            }

            if (asi_serial.OpenPort() == 1)
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


        public void HardZero()
        {
            asi_serial.SendAndWait("\\\r", 0, out byte[] ret);
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


        public void SetVelocity(double val)
        {
            val = val / 1000;
            string send_str = String.Format("S X={0} Y={0} Z={1}\r", val, val/5);
            asi_serial.SendAndWait(send_str, 0, out byte[] ret);

            GetStatus();
            GetPosition();
        }

        public void GetStatus()
        {
            reading = true;
            var success = asi_serial.SendAndWait("s x? y? z?\r", 0, out byte[] ret);
            if (!success)
            {
                reading = false;
                return;
            }

            //Return should be :A X=5.745920 Y = 5.745920
            var str1 = asi_serial.returnString;
            if (str1.StartsWith(":A "))
            {
                var str2 = str1.Replace(":A ", "").Replace("\r", "");
                var coord_str = str2.Split(' ');
                for (int i = 0; i < coord_str.Length; i++)
                {
                    if (i < velocity.Length - 1)
                        velocity[i] = Convert.ToDouble(coord_str[i].Split('=')[1]) * 1000.0;
                }
                //tString = str1;
                MotH?.Invoke(this, new MotrEventArgs("Status"));
            }
            reading = false;
        }

        public void SetNewPosition(double[] XYZ)
        {
            XNewPos = (int)Math.Round(XYZ[0]);
            YNewPos = (int)Math.Round(XYZ[1]);
            ZNewPos = (int)Math.Round(XYZ[2]);
        }


        public void SetPosition()
        {
            forceStop = false;
            setPosition_internal();
        }

        public void MovementDone()
        {
            //e = new MotrEventArgs("MovementDone"); // This is not enough to send StageMoveDone signal via PIPE
            MotH?.Invoke(this, new MotrEventArgs("MovementDone"));  // This can send StageMoveDone signal 
            moving = false;
            GetPosition();

        }

        public int setPosition_internal()
        {

            moving = true;
            string send_str;
            //Send Z position only when the X and Y positions are same.
            //Otherwize, X and Y position gradually moves when you get Z-stack .
            //The reason is that MS-2000 internal position has tens nm resolution but return only hundreds nm resolution.
            if (Math.Abs(XPos - XNewPos) < tolerance && Math.Abs(YPos - YNewPos) < tolerance)
            {
                send_str = String.Format("M Z={0}\r", (int)ZNewPos);
            }
            else
            {
                send_str = String.Format("M X={0} Y={1} Z={2}\r", (int)XNewPos, (int)YNewPos, (int)ZNewPos);
            }
            asi_serial.SendAndWait(send_str, 0, out byte[] ret);

            int retcode = WaitUntilMovementDone();

            if (retcode == 1)
            {
                MovementDone();
                return 0;
            }

            return 0;
        }

        public void FreezeResponse()
        {

        }


        public void GetPosition()
        {
            reading = true;
            var success = asi_serial.SendAndWait("W X Y Z\r", 0, out byte[] ret);
            if (!success)
            {
                reading = false;
                return;
            }

            //Return should be :A X=5.745920 Y = 5.745920
            var str1 = asi_serial.returnString;
            var coordinate = new double[3];
            if (str1.StartsWith(":A "))
            {
                var str2 = str1.Replace(":A ", "").Replace("\r", "");
                var coord_str = str2.Split(' ');
                for (int i = 0; i < coord_str.Length; i++)
                {
                    if (i < coord_str.Length - 1)
                        coordinate[i] = Convert.ToDouble(coord_str[i]);
                }

                XPos = coordinate[0];
                YPos = coordinate[1];
                ZPos = coordinate[2];

                //tString = str1;
                MotH?.Invoke(this, new MotrEventArgs("GetPositionDone"));
            }
            reading = false;
        }

        public void TimerEvent(object source, System.Timers.ElapsedEventArgs e1)
        {
            if (connected)
            {
                if (!moving && continuous_readCheck && !reading)
                {
                    GetStatus();

                    GetPosition();
                }
                else if (moving)
                {
                    MotH?.Invoke(this, new MotrEventArgs("Moving"));
                }
            }
        }

    }
}
