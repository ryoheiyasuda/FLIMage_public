using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stage_Control
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
        public SerialPort port;

        public int minMotorVal = (int)(-Math.Pow(2, 31) + 1);

        private bool start_moving = false;
        private bool moving = false;
        private int moving_count;

        public double resolutionX; //=0.04     
        public double resolutionY; //=0.04
        public double resolutionZ; //=0.005

        public int velocity_fine;
        public int velocity_coarse;
        public int maxVelocity;
        public int minVelocity;
        private bool setting_velocity = false;

        public int velVal;

        private Stopwatch sw = new Stopwatch();
        int timeout = 50; //millisecond
        int total_timeout = 2000;

        //public int ZStackStart_rel, ZStack_End_rel;
        public MotorCtrl.DeviceMode device_mode;
        public bool waitingReturn = false;
        private bool positionReceived = false;

        public bool freezing = false;

        private bool continuous_read = true;

        public int MaxReturnSize = 50;
        public int MinimumDisplaySize = 1;
        public int sizePosition = 13; //maybe 14
        public int sizeStatus = 33; //maybe 34
        public int returnSize = 1;

        private object lockPort;

        public String tString;//

        public MotorCtrl.StackPosition stack_Position = MotorCtrl.StackPosition.Undefined;

        public bool continuous_readCheck = true;


        public MotorCtrl_MP285A(String COMport, Double[] resolution, int velocity)
        {
            lockPort = new object();
            port = new SerialPort(COMport);
            port.BaudRate = 9600;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.NewLine = "\r";
            port.Handshake = Handshake.None;
            port.RtsEnable = true;
            port.DtrEnable = true;
            port.ReadTimeout = timeout;
            port.WriteTimeout = timeout;


            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

            port.Open();

            System.Threading.Thread.Sleep(timeout);

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

            GetPosition(true, false);

            if (velocity_coarse > minVelocity)
                SetVelocity(velocity_coarse);
            else
                SetVelocity(1500);


            waitingReturn = true;
            freezing = false;
            //
        }

        public void unsubscribe()
        {
            port.DataReceived -= port_DataReceived;
        }

        public void WriteCommandtoSerial(String s, Boolean terminate)
        {
            lock (lockPort)
            {
                port.Write(s);
                if (terminate)
                {
                    port.Write("\r");
                }
            }
        }

        public void WriteBinaryToSerial(Byte[] byteCode)
        {
            lock (lockPort)
            {
                port.Write(byteCode, 0, byteCode.Length);
                port.Write("\r");
            }
        }

        public void WaitUntilMovementDone()
        {
            for (int i = 0; i < total_timeout / timeout; i++)
            {
                if (!moving && !start_moving)
                {
                    break;
                }
                else
                {
                    System.Windows.Forms.Application.DoEvents(); //This is necessary for port to receive the event!!
                    System.Threading.Thread.Sleep(timeout);
                }
            }

            if (moving || start_moving)
            {
                start_moving = false;
                moving = false;
                freezing = true;
                e.Name = "Moving";
                MotH?.Invoke(this, e);
            }
            else
            {
                for (int i = 0; i < total_timeout / timeout; i++)
                {
                    if (!waitingReturn)
                    {
                        break;
                    }
                    else
                    {
                        System.Windows.Forms.Application.DoEvents(); //This is necessary for port to receive the event!!
                        System.Threading.Thread.Sleep(timeout);
                    }
                }
            }
        }

        public void WaitUntilAllTaskDone()
        {
            continuous_read = false; //this is necessary to block continuous read.

            for (int i = 0; i < total_timeout / timeout; i++)
            {
                if (!waitingReturn)
                {
                    break;
                }
                else
                {
                    System.Windows.Forms.Application.DoEvents(); //This is necessary for port to receive the event!!
                    System.Threading.Thread.Sleep(timeout);
                }
            }

            if (waitingReturn)
            {
                waitingReturn = false;
                freezing = true;
                MotH?.Invoke(this, e);
            }
        }

        public void reopen()
        {
            WaitUntilAllTaskDone();

            port.Close();
            System.Threading.Thread.Sleep(timeout);
            port.Open();
            System.Threading.Thread.Sleep(timeout);
            reset();
            System.Threading.Thread.Sleep(timeout);
            waitingReturn = false;
            SetVelocity(velocity_coarse);
        }

        public void reset()
        {
            WriteCommandtoSerial("r", true);
        }

        public void ClearBuffer()
        {
            try
            {
                port.ReadExisting();
            }
            catch (TimeoutException EX)
            {
                Debug.WriteLine("Clearing --- timeout");
            }

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
            WaitUntilAllTaskDone();
            ClearBuffer();

            byte[] buffer = BitConverter.GetBytes((short)val);

            WriteCommandtoSerial("V", false);
            WriteBinaryToSerial(buffer);

            setting_velocity = true;
            returnSize = 1;
        }


        public void SetNewPosition(double[] XYZ)
        {
            XNewPos = (int)XYZ[0];
            YNewPos = (int)XYZ[1];
            ZNewPos = (int)XYZ[2];
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
            ClearBuffer();

            System.Threading.Thread.Sleep(40);
            ClearBuffer();

            if (Math.Abs(XPos - XNewPos) < AllowError && Math.Abs(YPos - YNewPos) < AllowError && Math.Abs(ZPos - ZNewPos) < AllowError)
            {
                System.Threading.Thread.Sleep(40);
                Debug.WriteLine("Finished movement. Z = new {0}, old {1}. Time for movement = {2} ms", ZNewPos, ZPos, sw.ElapsedMilliseconds);
                sw.Stop();
                e = new MotrEventArgs("MovementDone");
                MotH?.Invoke(this, e);
                start_moving = false;
                moving = false;
                GetPosition(false, false);
                return 0;
            }

            moving = true;
            //            this.Enabled = false;

            byte[] buffer = new byte[12];
            int[] intA = { XNewPos, YNewPos, ZNewPos };

            Buffer.BlockCopy(intA, 0, buffer, 0, 12);

            WriteCommandtoSerial("m", false);
            WriteBinaryToSerial(buffer);


            start_moving = false;

            XPosMov = XPos;
            YPosMov = YPos;
            ZPosMov = ZPos;

            moving_count = 0;

            returnSize = 13;

            return 0;
            //System.Threading.Thread.Sleep(50);
        }

        public void GetStatus(bool block)
        {
            if (freezing)
                return;

            bool saveContinuousRead = continuous_readCheck;
            if (block)
                continuous_readCheck = false;

            WaitUntilAllTaskDone();
            ClearBuffer();

            WriteCommandtoSerial("s", true);

            waitingReturn = true;

            returnSize = sizeStatus;

            if (block)
                WaitUntilAllTaskDone();

            continuous_readCheck = saveContinuousRead;
        }

        public void GetPosition(bool block, bool duringMovement)
        {
            if (freezing)
                return;

            bool saveContinuousRead = continuous_readCheck;

            if (block)
                continuous_readCheck = false;

            WaitUntilAllTaskDone();
            ClearBuffer();

            WriteCommandtoSerial("c", true);
            waitingReturn = true;

            returnSize = sizePosition;

            if (block)
            {
                WaitUntilAllTaskDone();
            }

            if (!duringMovement)
            {
                XNewPos = XPos;
                YNewPos = YPos;
                ZNewPos = ZPos;
            }

            continuous_readCheck = saveContinuousRead;
        }

        //public void GetPosition()
        //{
        //    WaitUntilAllTaskDone();
        //    ClearBuffer();

        //    WriteCommandtoSerial("c", true);

        //    waitingReturn = true;

        //    returnSize = sizePosition;
        //}

        public void tryRecovery()
        {
            lock (lockPort)
            {
                for (int j = 0; j < 10; j++)
                {
                    port.Write("\r");
                }
            }
            returnSize = 1;
        }


        public void port_DataReceived(object sender, SerialDataReceivedEventArgs e_serial)
        {


            //Debug.WriteLine("Byte read: {0}", port.BytesToRead);
            continuous_read = true;

            byte[] buffer = new byte[MaxReturnSize];
            int bytesRead = 0;

            for (int i = 0; i < MaxReturnSize; i++)
            {
                bool stopbit = false;
                System.Threading.Thread.Sleep(1);
                try
                {
                    port.Read(buffer, i, 1);
                    bytesRead++;
                    if (i == 0)
                    {
                        if (buffer[0] == 13)
                            stopbit = true;
                    }

                    if (buffer[i] == 13 && bytesRead >= returnSize)
                        stopbit = true; //Stops when 13 comes in.
                }
                catch (TimeoutException Ex)
                {
                    Debug.WriteLine("Timeout!!!!");
                    //port.Write("\r");
                    stopbit = true;
                }

                if (stopbit)
                    break;
            }

            Array.Resize(ref buffer, bytesRead);
            //Debug.WriteLine("Expected return Size / BytesRead = " + returnSize + ", " + bytesRead);

            bool commandSent = false;

            e.Name = "";

            if (buffer.Length == 0)
            {
                Debug.WriteLine("Zero return????");
                //continuous_read = false; //??
            }


            if (setting_velocity)
            {
                if (buffer.Length > 0)
                    Debug.WriteLine("Set velocity signal received {0}", buffer[0]);
                continuous_read = true;
                //waitingReturn = false; //Finished capturing data.
                setting_velocity = false;

                waitingReturn = false;
                Task.Factory.StartNew(() => { GetStatus(false); });
                return;
            }


            if (moving || start_moving)
            {
                Debug.Write(String.Format("Moving stage now: {0} words: {1} ms; returnSize {2}: Word = ", bytesRead, sw.ElapsedMilliseconds, returnSize));
                for (int i = 0; i < bytesRead; i++)
                    Debug.Write(String.Format("{0}, ", buffer[i]));
                Debug.Write("\n");

                moving = true;
                start_moving = false;

                if (bytesRead != sizePosition) //Strange return...
                {
                    if (buffer.Length > 0)
                        Debug.WriteLine("Motion signal redceived {0}", buffer[0]);

                    System.Threading.Thread.Sleep(40);
                    waitingReturn = false;
                    Task.Factory.StartNew(() => { GetPosition(false, moving); });
                    commandSent = true;
                }
            }


            if (!freezing) //Process position info.
            {
                if (bytesRead == sizePosition)
                {
                    bool success = true;

                    for (int i = 0; i < 3; i++)
                    {
                        int value = BitConverter.ToInt32(buffer, i * 4);

                        if (i == 0)
                        {
                            XPos = value;
                        }
                        else if (i == 1)
                        {
                            YPos = value;
                        }
                        else if (i == 2)
                        {
                            ZPos = value;
                        }
                        //if (i == 0)
                        //{
                        //    if (Math.Abs(XPos - value) < 1000 / resolutionX || !positionReceived)
                        //        XPos = value;
                        //    else
                        //        success = false;
                        //}
                        //else if (i == 1)
                        //{
                        //    if (Math.Abs(YPos - value) < 1000 / resolutionY || !positionReceived)
                        //        YPos = value;
                        //    else
                        //        success = false;
                        //}
                        //else if (i == 2)
                        //{
                        //    if (Math.Abs(ZPos - value) < 1000 / resolutionZ || !positionReceived)
                        //        ZPos = value;
                        //    else
                        //        success = false;
                        //}

                        if (!success)
                        {
                            waitingReturn = false;
                            GetPosition(true, moving);
                            return;
                        }
                    }

                    if (!positionReceived) //Only the first time.... Reset to zero.
                    {
                        positionReceived = true;
                    }

                    if (moving)
                    {
                        e.Name = "Moving";
                        if (Math.Abs(XPos - XNewPos) <= AllowError && Math.Abs(YPos - YNewPos) <= AllowError && Math.Abs(ZPos - ZNewPos) <= AllowError)
                        {
                            System.Threading.Thread.Sleep(30);
                            Debug.WriteLine("Finished movement. Z = new {0}, old {1}. Time for movement = {2} ms", ZNewPos, ZPos, sw.ElapsedMilliseconds);
                            sw.Stop();
                            start_moving = false;
                            moving = false;
                            waitingReturn = false;
                            //Task.Factory.StartNew(() => { GetPosition(false, moving); });
                            //commandSent = true;
                            e.Name = "MovementDone";
                        }
                        else if (XPos == XPosMov && YPos == YPosMov && ZPos == ZPosMov) //not moving...
                        {
                            moving_count++;

                            if (moving_count < 3)
                            {

                                Debug.WriteLine("Not moving!!!... OK, let's get position one more time!");
                                Task.Factory.StartNew(() =>
                                {
                                    waitingReturn = false;
                                    System.Threading.Thread.Sleep(50);
                                    setPosition_internal();
                                });

                            }
                            else
                            {
                                //System.Threading.Thread.Sleep(100);
                                Debug.WriteLine("Not moving yet?????????????????? {0} ms, count= {1}", sw.ElapsedMilliseconds, moving_count);
                                sw.Stop();
                                start_moving = false;
                                moving = false;
                                Task.Factory.StartNew(() =>
                                {
                                    waitingReturn = false;
                                    System.Threading.Thread.Sleep(50);
                                    GetPosition(false, moving);
                                });
                                commandSent = true;
                                //e.Name = "MovementDone";
                                //return;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("NOT yet finished movement. Z = new {0}, old {1}. Time for movement = {2} ms", ZNewPos, ZPos, sw.ElapsedMilliseconds);
                            Debug.WriteLine("X = new {0}, old {1}. Y = new {2}, old{3}", XNewPos, XPos, YNewPos, YPos);

                            waitingReturn = false;
                            //Task.Factory.StartNew(() => { setPosition_internal(); });
                            commandSent = true;
                        }
                    }
                    else
                    {

                    }
                }

                if (bytesRead == sizeStatus) //STATUS
                {
                    continuous_read = true;

                    velVal = (int)buffer[29] * 256 + (int)buffer[28]; //Litle endian?
                                                                      //Debug.WriteLine("Velocity = {0}", velVal);

                    if (velVal >= (int)Math.Pow(2, 15))
                    {
                        device_mode = MotorCtrl.DeviceMode.fine;
                        velVal = velVal - (int)Math.Pow(2, 15);
                        if (velVal != velocity_fine)
                        {
                            velocity_fine = velVal;
                        }
                    }
                    else
                    {
                        device_mode = MotorCtrl.DeviceMode.coarse;
                        if (velVal != velocity_coarse)
                        {
                            velocity_coarse = velVal;
                        }
                    }

                    e.Name = "Status";
                }
            }

            //Commeting STATUS.

            if (IsFreeze(buffer))
            {
                freezing = true;
                tryRecovery();
                e.Name = "Freeze";
                //e.Name = "";
                continuous_read = false;
            }
            else if (bytesRead >= MinimumDisplaySize)
            {
                freezing = false;
                tString = "A" + buffer.Length.ToString() + ":";
                for (int i = 0; i < buffer.Length; i++)
                {
                    int val1 = buffer[i];
                    tString = tString + "-" + val1.ToString();
                }
            }



            if (moving && sw.ElapsedMilliseconds > total_timeout)
            {
                moving = false;
            }


            MotH?.Invoke(this, e);


            if (continuous_read && continuous_readCheck && !commandSent)
            {
                for (int i = 0; i < 200 / timeout; i++)
                {
                    if (continuous_read)
                    {
                        System.Threading.Thread.Sleep(timeout);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    else
                        break;

                    if (moving)
                        break;
                }

                if (continuous_read && continuous_readCheck || moving)
                {
                    if (bytesRead == sizePosition)
                        Task.Factory.StartNew(() => { GetStatus(false); });
                    else
                        Task.Factory.StartNew(() => { GetPosition(false, moving); });
                }
            }

            waitingReturn = false;
            return;

            //clearBuffer();
        }

        public bool IsFreeze(Byte[] buffer1)
        {
            bool freez = false;
            int bytesRead1 = buffer1.Length;

            if (buffer1.Length > 0)
                if (buffer1[0] == 48 && bytesRead1 != sizePosition && bytesRead1 != sizeStatus)
                {
                    freez = true;
                    for (int i = 1; i < 5; i++)
                    {
                        if (i < bytesRead1)
                        {
                            freez = freez && (buffer1[i] == 48);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

            return freez;

        }

    } //motorCtrl

}
