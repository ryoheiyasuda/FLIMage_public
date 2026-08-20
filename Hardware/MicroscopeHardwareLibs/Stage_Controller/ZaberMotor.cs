using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroscopeHardwareLibs.Stage_Contoller
{
    public class ZaberMotor
    {
        public event MotorHandler MotH;
        public MotrEventArgs e = new MotrEventArgs("");
        public delegate void MotorHandler(ZaberMotor mCtrls, MotrEventArgs e);

        public double maxDistanceXY = 500; //micrometers
        public double maxDistanceZ = 100;

        public int N_Coordinate = 3;

        public double XPos, YPos, ZPos;
        public double XNewPos, YNewPos, ZNewPos;
        //public double XPosMov, YPosMov, ZPosMov;

        public double XMax, YMax, ZMax;
        public double XMin, YMin, ZMin;

        public double minMovX = 10;
        public double minMovY = 10;
        public double minMovZ = 10;
        //public SerialPort port;

        public double[] maxPos = new double[3];
        public double[] minPos = new double[3];

        public int minMotorVal = (int)(-Math.Pow(2, 31) + 1);

        public bool moving;

        public double resolutionX = 1;
        public double resolutionY = 1;
        public double resolutionZ = 1;

        public double[] velocity = new double[] { 2000, 2000, 2000 };
        public double[] maxVelocity = new double[] { 10000, 10000, 10000 };
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

        public double tolerance = 1; //0.1 um

        private string zaber_id = "/2";
        private string zaber_address = "0";
        SerialCommandObj zaber_serial;

        public ZaberMotor(String port, int MotorDisplayUpdateTime)
        {

            MotorDisplayUpdateTime_ms = MotorDisplayUpdateTime;


            zaber_serial = new SerialCommandObj(port, 115200, "Zaber");

            if (zaber_serial.OpenPort() == 0)
                return;

            connected = true;
            reset();

            XPos = 0;
            YPos = 0;
            ZPos = 0;

            XNewPos = XPos;
            YNewPos = YPos;
            ZNewPos = ZPos;

            moving = false;

            tString = "";

            GetPosition();


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
                var success = zaber_serial.SendAndWait(zaber_id + " \r", 0, out byte[] ret);
                var str1 = zaber_serial.returnString.Replace("\r", "");

                if (str1.Contains("OK IDLE"))
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
            zaber_serial.SendAndWait(zaber_id + " stop\r", 0, out byte[] ret);
        }

        public void disconnect()
        {
            ThoTimer.Close();
            ThoTimer.Dispose();
            connected = false;
            zaber_serial.ClosePort();
        }

        public void reopen()
        {
            if (connected)
            {
                connected = false;
                zaber_serial.ClosePort();
            }

            if (zaber_serial.OpenPort() == 1)
                connected = true;

            if (connected)
            {
                ThoTimer = new System.Timers.Timer(MotorDisplayUpdateTime_ms);
                ThoTimer.Elapsed += TimerEvent;
                ThoTimer.AutoReset = true;
                ThoTimer.Enabled = true;
            }
        }

        public string SendAndWait(string command)
        {
            string str1 = "";
            for (int i = 0; i < 3; i++)
            {
                zaber_serial.SendAndWait(command);
                str1 = zaber_serial.returnString;
                if (str1.Contains("OK") && str1.EndsWith("\r\n"))
                    break;
            }
            return str1;
        }

        public void reset()
        {
            string str1;
            for (int j = 0; j < 3; j++)
            {
                SendAndWait("/\r");
                str1 = zaber_serial.returnString;
                var lines = str1.Split('\r');
                if (str1.EndsWith("\r\n"))
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var command = String.Format("/{0} get pos\r", i + 1);
                        var str_pos = SendAndWait(command);
                        if (!str_pos.Contains("BAD") && str_pos.Contains("@") && str_pos.Contains("OK"))
                        {
                            zaber_id = str_pos.Replace("@", "/").Split(' ')[0];
                            break;
                        }
                    }
                    break;
                }
            }

            str1 = SendAndWait(zaber_id + " get limit.max\n");
            if (str1.Contains("OK") && str1.Contains("IDLE") && str1.Contains("\r"))
            {
                int index = str1.IndexOf("IDLE");
                var str2 = str1.Substring(index + 4).Trim().Substring(2).Trim();
                var coord_str = str2.Split(' ');
                var maxPos = new double[3];
                for (int i = 0; i < coord_str.Length; i++)
                {
                    if (coord_str[i] == "NA")
                        maxPos[i] = Double.NaN;
                    else
                        maxPos[i] = Convert.ToDouble(coord_str[i]);
                }
                XMax = maxPos[1];
                YMax = maxPos[2];
                ZMax = maxPos[0];
            }
        }


        public void HardZero()
        {
            SendAndWait(zaber_id + " \r");
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
            velocity = new double[] { val, val, val };

            GetStatus();
            GetPosition();
        }

        public void GetStatus()
        {
            MotH?.Invoke(this, new MotrEventArgs("Status"));
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

            //if (Math.Abs(XPos - XNewPos) > tolerance || Math.Abs(YPos - YNewPos) > tolerance || Math.Abs(ZPos - ZNewPos) > tolerance)
            //{

            if (!Double.IsNaN(ZPos) && Math.Abs(ZPos - ZNewPos) > tolerance)
            {
                send_str = String.Format(zaber_id + " 1 move abs {0} {1}\r", ZNewPos, velocity[2]);
                zaber_serial.SendAndWait(send_str);
            }

            if (!Double.IsNaN(XPos) && Math.Abs(XPos - XNewPos) > tolerance)
            {
                send_str = String.Format(zaber_id + " 2 move abs {0} {1}\r", XNewPos, velocity[0]);
                zaber_serial.SendAndWait(send_str);
            }

            if (!Double.IsNaN(YPos) && Math.Abs(YPos - YNewPos) > tolerance)
            {
                send_str = String.Format(zaber_id + " 3 move abs {0} {1}\r", YNewPos, velocity[1]);
                zaber_serial.SendAndWait(send_str);
            }

            int retcode = WaitUntilMovementDone();

            if (retcode == 1)
            {
                MovementDone();
                return 0;
            }
            //}
            return 0;
        }

        public void FreezeResponse()
        {

        }

        public void GetPosition()
        {
            reading = true;
            bool success = false;
            string str1 = SendAndWait(zaber_id + " get pos\r");

            var coordinate = new double[3];
            if (str1.Contains("OK") && str1.Contains("IDLE") && str1.Contains("\r"))
            {
                int index = str1.IndexOf("IDLE");
                var str2 = str1.Substring(index + 4).Trim().Substring(2).Trim();
                var coord_str = str2.Split(' ');
                if (coord_str.Length >= 3)
                {
                    for (int i = 0; i < coord_str.Length; i++)
                    {
                        if (i < coord_str.Length)
                        {
                            if (coord_str[i] != "NA")
                                coordinate[i] = Convert.ToDouble(coord_str[i]);
                            else
                                coordinate[i] = Double.NaN;
                        }
                    }

                    if (!coordinate.Any(x => Double.IsNaN(x)))
                    {
                        XPos = coordinate[1];
                        YPos = coordinate[2];
                        ZPos = coordinate[0];
                        success = true;
                    }
                }
            }

            MotH?.Invoke(this, new MotrEventArgs("GetPositionDone"));
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
