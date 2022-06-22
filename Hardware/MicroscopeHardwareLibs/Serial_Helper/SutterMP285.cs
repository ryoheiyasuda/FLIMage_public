using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroscopeHardwareLibs
{

    public class SutterMP285
    {
        SerialCommandObj serial;
        public bool crash_response = false;
        public string StringCode; //For debugging.
        public bool zozolab = false;
        public int ROESPeed = 0;

        public SutterMP285(String portname, int boundRate, string motorType)
        {
            serial = new SerialCommandObj(portname, boundRate, "SutterMP285");
            zozolab = motorType.ToLower().Contains("zozo");
        }

        public int OpenPort()
        {
            int ret1 = serial.OpenPort();
            string ret_string = "";
            serial.WaitForCR(out ret_string);
            if (zozolab)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (ret_string != "Initialized")
                        serial.WaitForCR(out ret_string);
                    else
                        break;
                }
            }
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

        public int GetVelocity(ref int vel, ref bool fineMode)
        {
            System.Threading.Thread.Sleep(100);
            int success = 1;
            byte[] command = new byte[2];
            command[0] = Convert.ToByte('s');
            command[1] = Convert.ToByte('\r');
            bool done = serial.SendAndWait(command, 33, out byte[] ret);
            StringCode = serial.GetStringCode();

            if (IsCrashed(ret))
            {
                return -1;
            }

            int velVal = 0;
            int fineCoarseFactor = 10;

            if (done && ret.Length >= 33 && ret[32] == '\r')
            {
                success = 1;
                ROESPeed = (int)ret[5] * 256 + (int)ret[4]; //microstep per ROI click.
                int nMicroStepPerPulse = (int)ret[11] * 256 + (int)ret[10];
                int adjustedPulseSpeed_uSteps_s = (int)ret[13] * 256 + (int)ret[12];
                fineCoarseFactor = (int)ret[11] * 256 + (int)ret[10];
                int step_div = (int)ret[25] * 256 + (int)ret[24];
                int step_mult = (int)ret[27] * 256 + (int)ret[26];
                velVal = (int)ret[29] * 256 + (int)ret[28];
                // 0,1,2,3,4,5, 6,7, 8,9,10,11, 12,13,14, 15, 16,17, 18,19,20,21,22,23,24,25,26,27, 28,29,30,31,32
                //98,3,1,1,4,0,20,0,99,0, 5, 0,136,19, 1,120,196, 9,136,19,80, 0, 0, 0,25, 0, 4, 0,220, 5,46, 1,13
                //For ROE setting = 4.

            }
            else
                success = 0;

            ROESPeed = ROESPeed * fineCoarseFactor;

            if (success == 1)
            {
                bool coarseMode = velVal >= (int)Math.Pow(2, 15);

                if (coarseMode)
                    velVal = velVal - (int)Math.Pow(2, 15);

                fineMode = !coarseMode;
                vel = velVal;
            }

            return success;
        }

        public int SetVelocity(int velocity, bool FineMode)
        {
            byte[] command = new byte[4];

            command[0] = Convert.ToByte('V');
            command[3] = Convert.ToByte('\r');

            if (velocity >= (int)Math.Pow(2, 15))
                velocity -= (int)Math.Pow(2, 15);

            if (!FineMode)
                velocity += (int)Math.Pow(2, 15);

            Buffer.BlockCopy(new short[] { (short)velocity }, 0, command, 1, 2);

            bool done = serial.SendAndWait(command, 1, out byte[] ret);

            return done ? 1 : 0;
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
            if (!zozolab)
            {
                if ((ret.Length > 6 && ret.All(x => x == 0)) ||
                   (ret.Length >= 1 && ret.All(x => x == 48)))
                {
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public int SetHardZero()
        {

            bool done = serial.SendSignalAndWaitForCR("o\r", out string ret_string);
            return done ? 1 : 0;

        }

        //zozo_Lab only.
        public int SetPolarity(bool[] PolarityCode)
        {
            int polCodeInt = 0;
            int pow2 = 1;
            for (int i = 0; i < PolarityCode.Length; i++)
            {
                polCodeInt += (PolarityCode[i] ? 1 : 0) * pow2;
                pow2 = pow2 * 2;

            }

            if (zozolab)
            {
                bool done = serial.SendSignalAndWaitForCR("xa" + polCodeInt + "\r", out string ret_string);
                return done ? 1 : 0;
            }
            else
                return 0;
        }

        public int GetPolarity(out bool[] PolarityCode)
        {
            PolarityCode = new bool[4];
            if (zozolab)
            {
                bool done = serial.SendSignalAndWaitForCR("xa\r", out string ret_string);
                if (done && ret_string.Length > 0)
                {
                    if (!Int32.TryParse(ret_string, out int PolarityInt))
                    {
                        return 0;
                    }
                    else
                    {
                        for (int i = 0; i < PolarityCode.Length; i++)
                            PolarityCode[i] = ((PolarityInt >> i) & 1) == 1;
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public int SetROESpeed(int ROESpeed)
        {
            if (zozolab)
            {
                bool done = serial.SendSignalAndWaitForCR("xe" + ROESpeed + "\r", out string ret_string);
                return done ? 1 : 0;
            }
            else
                return 0;
        }

        public int GetROESpeed(out int ROESpeed)
        {
            ROESpeed = 0;
            if (zozolab)
            {
                bool done = serial.SendSignalAndWaitForCR("xe\r", out string ret_string);
                if (done && ret_string.Length > 0)
                {
                    if (!Int32.TryParse(ret_string, out ROESpeed))
                    {
                        ROESpeed = 0;
                        return 0;
                    }
                    else
                        return 1;
                }
                else
                {
                    ROESpeed = 0;
                    return 0;
                }
            }
            else
                return 0;
        }


        public int SetXYResolution(int resolution)
        {
            if (zozolab)
            {
                bool done = serial.SendSignalAndWaitForCR("xr" + resolution + "\r", out string ret_string);
                return done ? 1 : 0;
            }
            else
                return 0;
        }

        public int GetXYResolution(out int resolution)
        {
            resolution = 0;
            if (zozolab)
            {
                bool done = serial.SendSignalAndWaitForCR("xr\r", out string ret_string);
                if (done && ret_string.Length > 0)
                {
                    if (!Int32.TryParse(ret_string, out resolution))
                    {
                        resolution = 0;
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    resolution = 0;
                    return 0;
                }
            }
            else
                return 0;
        }

        public int GetPosition(int[] pos)
        {
            int success = 1;
            byte[] command = new byte[2];
            command[0] = Convert.ToByte('c');
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
            byte[] command = new byte[14];

            command[0] = Convert.ToByte('m');
            command[13] = Convert.ToByte('\r');
            Buffer.BlockCopy(position, 0, command, 1, 12);
            //bool done = serial.SendAndWait(command, 2, out byte[] ret);
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
