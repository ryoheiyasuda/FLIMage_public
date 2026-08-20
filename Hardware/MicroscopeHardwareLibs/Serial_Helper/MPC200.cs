using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroscopeHardwareLibs
{

    public class MPC200
    {
        SerialCommandObj serial;
        public bool crash_response = false;
        public string StringCode; //For debugging.
        public int ROESPeed = 0;
        public double velocity = 1500;

        public MPC200(String portname, int boundRate)
        {
            serial = new SerialCommandObj(portname, boundRate, "MPC200");
        }

        public int OpenPort()
        {
            int ret1 = serial.OpenPort();
            string ret_string = "";
            serial.WaitForCR(out ret_string);
            return ret1;
        }

        public void ClosePort()
        {
            serial.ClosePort();
        }

        public void ClearBuffer()
        {
            serial.ClearBuffer();
        }

        public int GetDeviceInfo()
        {
            byte[] command = new byte[2];
            command[0] = Convert.ToByte('K');
            command[1] = Convert.ToByte('\r');
            int len_com = 5;
            bool done = serial.SendAndWait(command, len_com, out byte[] ret);
            if (done && ret.Length >= len_com && ret[len_com - 1] == '\r')
                return ret[1];
            else
                return -1;
        }

        public int GetVelocity(ref int vel)
        {
            vel = (int)velocity;
            return 0;
        }

        public int SetVelocity(int velocity1)
        {
            velocity = velocity1;
            return 0;
            //byte[] command = new byte[4];

            //command[0] = Convert.ToByte('V');
            //command[3] = Convert.ToByte('\r');

            //if (velocity >= (int)Math.Pow(2, 15))
            //    velocity -= (int)Math.Pow(2, 15);

            //if (!FineMode)
            //    velocity += (int)Math.Pow(2, 15);

            //Buffer.BlockCopy(new short[] { (short)velocity }, 0, command, 1, 2);

            //bool done = serial.SendAndWait(command, 1, out byte[] ret);

            //return done ? 1 : 0;
            //bool done = serial.SendAndWait(command, 1, out byte[] ret);

            //if (done && ret.Length > 1 && ret[0] == 13)
            //    return 1;
            //else
            //    return 0;
        }

        public void CrashResponse()
        {
            byte[] command = new byte[2];
            command[0] = 13;
            command[1] = 13;
            for (int i = 0; i < 200; i++)
                serial.SendCommand(command);
        }

        public bool IsCrashed(byte[] ret)
        {
            return false;
        }

        public int SetHardZero()
        {

            bool done = serial.SendSignalAndWaitForCR("o\r", out string ret_string);
            return done ? 1 : 0;

        }


        public int SetROESpeed(int ROESpeed)
        {
            return 0;
        }

        public int GetROESpeed(out int ROESpeed)
        {
            ROESpeed = 0;

            return 0;
        }


        public int SetXYResolution(int resolution)
        {
            return 0;
        }

        public int GetXYResolution(out int resolution)
        {
            resolution = 0;
            return 0;
        }

        public int GetPosition(int[] pos)
        {
            int success = 1;
            byte[] command = new byte[2];
            command[0] = Convert.ToByte('C');
            command[1] = Convert.ToByte('\r');
            bool done = serial.SendAndWait(command, 13, out byte[] ret);
            StringCode = serial.GetStringCode();

            if (IsCrashed(ret))
            {
                if (crash_response)
                    CrashResponse();

                return -1;
            }

            if (done && ret.Length == 13)
            {
                success = 1;
                Buffer.BlockCopy(ret, 0, pos, 0, 12);
            }
            else if (done && ret.Length > 13 && ret[12] == 13 && ret[13] == 13)
            {
                success = 1;
                Buffer.BlockCopy(ret, 0, pos, 0, 12);
            }
            else
            {
                done = serial.SendAndWait(command, 13, out ret);
                if (done && ret != null && (ret.Length == 13 || ret.Length == 14))
                {
                    success = 1;
                    Buffer.BlockCopy(ret, 0, pos, 0, 12);
                }
                else
                    success = 0;
            }

            return success;
        }

        //public int waitForMovement()
        //{
        //    int success;
        //    bool done = serial.WaitForCR(out string rest_string);
        //    success = done ? 1 : 0;
        //    return success;
        //}

        public int GoToPosition(int[] position)
        {
            byte[] command0 = new byte[2];
            command0[0] = Convert.ToByte('S');
            command0[1] = Convert.ToByte('\r');
            serial.SendCommand(command0);

            System.Threading.Thread.Sleep(30);

            byte[] command = new byte[14];
            command[0] = Convert.ToByte('S');
            command[13] = Convert.ToByte('\r');
            Buffer.BlockCopy(position, 0, command, 1, 12);
            serial.SendCommand(command);
            return 0;
        }

        public int ZeroPosition()
        {
            byte[] command = new byte[2];
            command[0] = Convert.ToByte('o');
            command[1] = Convert.ToByte('\r');
            bool done = serial.SendAndWait(command, 1, out byte[] ret);
            if (done && ret[0] == Convert.ToByte('\r'))
                return 1;
            else
                return 0;
        }

        public int Stop()
        {
            byte[] command = new byte[1];
            command[0] = 0x03;
            bool done = serial.SendAndWait(command, 1, out byte[] ret);
            if (done && ret[0] == Convert.ToByte('\r'))
                return 1;
            else
                return 0;
        }


        public void Reset()
        {
            byte[] command = new byte[2];
            command[0] = Convert.ToByte('r');
            command[1] = Convert.ToByte('\r');
            serial.SendCommand(command); //No return.
        }

    }
}
