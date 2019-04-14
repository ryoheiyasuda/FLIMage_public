using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;

namespace Stage_Control
{
    public class MotorCtrl_MP285
    {
        public event MotorHandler MotH;
        public MotrEventArgs e = new MotrEventArgs("");
        public delegate void MotorHandler(MotorCtrl_MP285 mCtrls, MotrEventArgs e);

        public double maxDistanceXY = 500; //micrometers
        public double maxDistanceZ = 100;

        public int N_Coordinate = 3;

        public int XPos, YPos, ZPos;
        public int XNewPos, YNewPos, ZNewPos;
        public int XPosMov, YPosMov, ZPosMov;

        public SerialPort port;

        public int minMotorVal = (int)(-Math.Pow(2, 31) + 1);

        private bool start_moving, moving;
        private int moving_count;

        public double resolutionX; //=0.04     
        public double resolutionY; //=0.04
        public double resolutionZ; //=0.005

        public int velocity_fine;
        public int velocity_coarse;
        public int maxVelocity;
        public int minVelocity;

        public int velVal;

        private Stopwatch sw = new Stopwatch();
        int timeout = 100; //millisecond
        int total_timeout = 2000;

        //public int ZStackStart_rel, ZStack_End_rel;
        public MotorCtrl.DeviceMode device_mode;
        public bool waitingReturn;
        private bool positionReceived = false;

        public bool freezing = false;

        public bool continuous_read = true;
        public bool continuous_readCheck = true;

        object lockPort;

        public String tString;//

        public MotorCtrl.StackPosition stack_Position = MotorCtrl.StackPosition.Undefined;

        public MotorCtrl_MP285(String COMport, Double[] resolution, int velocity)
        {
            lockPort = new object();

            port = new SerialPort(COMport, 9600, Parity.None, 8, StopBits.One);
            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

            port.Open();
            System.Threading.Thread.Sleep(200);
            reset();

            resolutionX = resolution[0];
            resolutionY = resolution[1];
            resolutionZ = resolution[2];

            XPos = 0;
            YPos = 0;
            ZPos = 0;

            XNewPos = XPos;
            YNewPos = YPos;
            ZNewPos = ZPos;

            velocity_fine = 100;
            velocity_coarse = velocity;
            maxVelocity = 1500;
            minVelocity = 100;

            start_moving = false;
            moving = false;



            tString = "";
            SetVelocity(velocity_coarse);

            //lock (lockPort)
            //{
            //    port.Write("c\r\n");
            //}
            //waitingReturn = true;
            freezing = false;
            
        }


        public void unsubscribe()
        {
            port.DataReceived -= port_DataReceived;
        }


        public void WaitUntilAllTaskDone()
        {
            while (waitingReturn)
            {
                System.Threading.Thread.Sleep(50);
                Application.DoEvents();
                return;
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

        public double[] GetResolution()
        {
            return new double[] { resolutionX, resolutionY, resolutionZ };
        }

        public void reset()
        {
            lock (lockPort)
            {
                port.Write("r\r\n");
            }
        }

        public int clearBuffer(bool NeedReceipt)
        {
            for (int i = 0; i < 10; i++)
            {
                System.Threading.Thread.Sleep(30);

                if (port.BytesToRead > 0)
                {
                    byte[] bufferR = new byte[port.ReadBufferSize];
                    int bytesRead = port.Read(bufferR, 0, bufferR.Length);
                    if (NeedReceipt)
                        break;
                }
                else
                {
                    if (!NeedReceipt)
                        break;
                }
            }
            return port.BytesToRead;
        }


        public double[] GetCurrentUncalibratedPosition()
        {
            double[] XYZ = new double[3];
            XYZ[0] = XPos;
            XYZ[1] = YPos;
            XYZ[2] = ZPos;
            return XYZ;
        }

        public double[] GetNewPosition()
        {
            double[] XYZ = new double[3];
            XYZ[0] = XNewPos;
            XYZ[1] = YNewPos;
            XYZ[2] = ZNewPos;
            return XYZ;
        }        


        public void SetVelocity(int val)
        {
            //if (waitingReturn)
            //    return;

            WaitUntilAllTaskDone();

            byte[] buffer = BitConverter.GetBytes((short)val);

            clearBuffer(false);

            lock (lockPort)
            {
                port.Write("v");
            }

            clearBuffer(true);

            lock (lockPort)
            {
                port.Write(buffer, 0, buffer.Length);
                port.Write("\r\n");
            }
            //System.Threading.Thread.Sleep(50);
            //port.Write();

            waitingReturn = true;
        }


        public void reopen()
        {
           WaitUntilAllTaskDone();
            port.Close();
            System.Threading.Thread.Sleep(100);
            port.Open();
            System.Threading.Thread.Sleep(100);

            waitingReturn = false;
            SetVelocity(velocity_coarse);
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
            //if (waitingReturn)
            //    return -1;

            WaitUntilAllTaskDone();

            moving = true;
            //            this.Enabled = false;

            byte[] buffer = new byte[12];
            int[] intA = { XNewPos, YNewPos, ZNewPos };

            Buffer.BlockCopy(intA, 0, buffer, 0, 12);

            lock (lockPort)
            {
                port.Write("m");
                port.Write(buffer, 0, buffer.Length);
                port.Write("\r\n");
            }

            start_moving = false;
            waitingReturn = true;

            XPosMov = XPos;
            YPosMov = YPos;
            ZPosMov = ZPos;

            moving_count = 0;
            return 0;
            //System.Threading.Thread.Sleep(50);
        }

        public void GetStatus(bool block)
        {
            //if (waitingReturn)
            //    return;
            WaitUntilAllTaskDone();
            lock (lockPort)
            {
                port.Write("s\r\n");
            }
            waitingReturn = true;
            if (block)
                WaitUntilAllTaskDone();
        }

        public void GetPosition(bool block, bool duringMovement)
        {
            if (freezing)
                return;

            bool saveContinuousRead = continuous_readCheck;

            if (block)
                continuous_readCheck = false;

            WaitUntilAllTaskDone();

            lock (lockPort)
            {
                port.Write("c\r\n");
            }
            waitingReturn = true;

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

        public void tryRecovery()
        {
            for (int j = 0; j < 10; j++)
            {
                lock (lockPort)
                {
                    //byte[] bytes1 = new byte[] { 0 };
                    //port.Write(bytes1, 0, bytes1.Length);
                    port.Write("\r\n");
                }
            }

        }

        public void port_DataReceived(object sender, SerialDataReceivedEventArgs e_serial)
        {
            if (!freezing)
            {
                int bR = port.BytesToRead;
                for (int i = 0; i < 10; i++)
                {
                    bR = port.BytesToRead;
                    System.Threading.Thread.Sleep(30);
                    if (bR == port.BytesToRead)
                        break;
                }
            }

            waitingReturn = false;

            if (moving && sw.ElapsedMilliseconds > timeout)
            {
                moving = false;
                sw.Stop();
            }


            byte[] buffer = new byte[port.ReadBufferSize];
            int bytesRead = port.Read(buffer, 0, buffer.Length);
            int count = 0;
            int bytesRead1;
            byte[] buffer1;

            if (!freezing)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == 13)
                    {
                        count++;
                    }
                    else
                        break;
                }

                bytesRead1 = bytesRead - count;
                if (bytesRead1 == 0)
                    bytesRead1 = 1;
                buffer1 = new Byte[bytesRead1];
                Buffer.BlockCopy(buffer, count, buffer1, 0, bytesRead1);
            }
            else
            {
                count = 0;
                buffer1 = buffer;
                bytesRead1 = bytesRead;
            }



            if (moving || start_moving)
            {
                Debug.Write(String.Format("Moving stage now: {0} words: {1} ms: Word = ", bytesRead1, sw.ElapsedMilliseconds));
                for (int i = 0; i < bytesRead1; i++)
                    Debug.Write(String.Format("{0}, ", buffer1[i]));
                Debug.Write("\n");

                if (bytesRead1 == 1)
                {
                    GetPosition(false, true);
                    MotH?.Invoke(this, e);
                    return;
                }
            }
            else
            {
                GetStatus(false);
                MotH?.Invoke(this, e);
                return;
            }



            if (!freezing) //Process position info.
            {
                if (bytesRead1 == 14)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int value = BitConverter.ToInt32(buffer1, i * 4);

                        if (i == 0)
                            XPos = value;
                        else if (i == 1)
                            YPos = value;
                        if (i == 2)
                            ZPos = value;
                    }

                    if (!positionReceived) //Only the first time.... Reset to zero.
                    {
                        positionReceived = true;
                    }


                    if (moving)
                    {
                        if (XPos == XNewPos && YPos == YNewPos && ZPos == ZNewPos)
                        {
                            moving = false;
                            e.Name = "MovementDone";
                            Debug.WriteLine("Finished movement. Time for movement = {0} ms", sw.ElapsedMilliseconds);
                            sw.Stop();
                            GetPosition(false, true);
                            MotH?.Invoke(this, e);
                            return;
                        }
                        else if (XPos == XPosMov && YPos == YPosMov && ZPos == ZPosMov)
                        {

                            moving_count++;

                            if (moving_count < 3)
                            {
                                System.Threading.Thread.Sleep(50);
                                GetPosition(false, true);
                                MotH?.Invoke(this, e);
                                return;
                            }
                            else
                            {
                                Debug.WriteLine("Not moving?????????????????? {0} ms, count= {1}", sw.ElapsedMilliseconds, moving_count);
                                sw.Stop();
                                moving = false;
                            }

                        }
                        else
                        {
                            Debug.WriteLine("NOT yet finished movement. Z = new {0}, old {1}. Time for movement = {2} ms", ZNewPos, ZPos, sw.ElapsedMilliseconds);
                            System.Threading.Thread.Sleep(50);
                            setPosition_internal();
                            MotH?.Invoke(this, e);
                            return;
                        }
                    }
                    else
                    {
                        XNewPos = XPos;
                        YNewPos = YPos;
                        ZNewPos = ZPos;
                    }

                }

                if (bytesRead1 == 34 && !moving) //STATUS
                {
                    velVal = (int)buffer1[29] * 256 + (int)buffer1[28]; //Litle endian?
                    //Debug.WriteLine("Velocity = {0}", velVal);
                    if (velVal >= (int)Math.Pow(2, 15))
                    {
                        device_mode = MotorCtrl.DeviceMode.fine;
                        velVal = velVal - (int)Math.Pow(2, 15);
                    }
                    else
                    {
                        device_mode = MotorCtrl.DeviceMode.coarse;
                    }

                    System.Threading.Thread.Sleep(35);
                    MotH?.Invoke(this, e);
                    GetPosition(false, false);
                    return;
                }
            }



            //Commeting STATUS.
            if (true)
            {
                if (IsFreeze(buffer1))
                {
                    freezing = true;
                    tryRecovery();
                    //GetPosition();
                    MotH?.Invoke(this, e);
                    return;
                }
                else
                {
                    freezing = false;
                    tString = "A" + buffer1.Length.ToString() + ":";
                    for (int i = 0; i < buffer1.Length; i++)
                    {
                        int val1 = buffer1[i];
                        tString = tString + "-" + val1.ToString();
                    }
                }
            }

            MotH?.Invoke(this, e);

            if (continuous_read && continuous_readCheck)
            {
                if (bytesRead1 == 14) //POSITION
                {
                    System.Threading.Thread.Sleep(35);
                    GetStatus(false);
                }
                else if (bytesRead1 == 34)
                {
                    Task.Factory.StartNew(() =>
                    {
                        System.Threading.Thread.Sleep(500);
                        GetPosition(false, false);
                    });
                }
                else
                {
                    System.Threading.Thread.Sleep(35);
                    GetStatus(false);
                }

            }

            return;

            //clearBuffer();
        }

        public bool IsFreeze(Byte[] buffer1)
        {
            bool freez = false;
            int bytesRead1 = buffer1.Length;
            if (buffer1[0] == 48 && bytesRead1 != 14 && bytesRead1 != 34)
            {
                freez = true;
                for (int i = 1; i < 5; i++)
                {
                    if (bytesRead1 > i)
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
