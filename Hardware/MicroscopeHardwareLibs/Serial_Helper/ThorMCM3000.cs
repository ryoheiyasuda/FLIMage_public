using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace MicroscopeHardwareLibs
{

    public class ThorMCM3000
    {
        SerialCommandObj serial;

        public double[] conversionFactor = { 0.2116667 / 1000.0, 0.2116667 / 1000.0, 0.2116667 / 1000.0 };
        public double[] maxPos = { 10, 10, 10 };
        public string StringCode;

        double[] con3001 = { 0.2116667 / 1000.0, 0.2116667 / 1000.0, 0.2116667 / 1000.0 };
        double[] con3002 = { 0.5 / 1000.0, 0.5 / 1000.0, 0.5 / 1000.0 };
        double[] con3003 = { .0390625 / 1000.0, .0390625 / 1000.0, .0390625 / 1000.0 };
        double[] conbscope = { 0.2116667 / 1000.0, 0.2116667 / 1000.0, .1 / 1000.0 };
        double[] conbergamo = { .5 / 1000.0, .5 / 1000.0, .1 / 1000.0 };

        public double[] maxPos3001 = { 10, 10, 10 };
        public double[] maxPos3002 = { 20, 20, 20 };
        public double[] maxPos3003 = { 20, 20, 20 };
        public double[] maxBscope = { 10, 10, 3 };
        public double[] maxBergamo = { 20, 20, 3 };

        public Thor3000Type motorType = Thor3000Type.MCM3001;

        public ThorMCM3000(String portname, int boundRate, Thor3000Type motor_type)
        {
            serial = new SerialCommandObj(portname, boundRate, "ThorMCM3000");
            motorType = motor_type;
            if (motorType == Thor3000Type.MCM3001)
            {
                conversionFactor = con3001;
                maxPos = maxPos3001;
            }
            else if (motorType == Thor3000Type.MCM3002)
            {
                conversionFactor = con3002;
                maxPos = maxPos3002;
            }
            else if (motorType == Thor3000Type.MCM3003)
            {
                conversionFactor = con3003;
                maxPos = maxPos3003;
            }
            else if (motorType == Thor3000Type.BScope)
            {
                conversionFactor = conbscope;
                maxPos = maxBscope;
            }
            else if (motorType == Thor3000Type.Bergamo)
            {
                conversionFactor = conbergamo;
                maxPos = maxBergamo;
            }
        }

        public int OpenPort()
        {
            return serial.OpenPort();
        }

        public void ClosePort()
        {
            serial.ClosePort();
        }

        public void ClearBuffer()
        {
            serial.ClearBuffer();
        }

        public int GetPosition(double[] pos)
        {
            int success = 1;

            for (int i = 0; i < 3; i++)
            {
                if (GetPositionCount(i, out int posEach) == 1)
                {
                    pos[i] = (double)posEach * conversionFactor[i];
                }
                else
                {
                    if (GetPositionCount(i, out posEach) == 1)
                        pos[i] = (double)posEach * conversionFactor[i];
                    else
                    {
                        success = 0;
                        break;
                    }
                }
            }
            return success;
        }

        public int GoToPosition(int axis, double position)
        {
            if (position < maxPos[axis] && position > -maxPos[axis])
            {
                int PositionCount = (int)Math.Round(position / conversionFactor[axis]);
                GoToPositionCount(axis, PositionCount);
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public void ZeroPosition(int axis)
        {
            SetEncoderCount(axis, 0);
        }

        public void Stop()
        {
            for (int i = 0; i < 3; i++)
                StopAxis(i);
        }

        public bool IsMotorBusy(int axis)
        {
            byte[] command = new byte[] { 0x80, 0x04, (byte)axis, 0x00, 0x00, 0x00 };
            bool done = serial.SendAndWait(command, 20, out byte[] response);

            if (done && response.Length >= 16)
            {
                int bitand = response[16] & 0x30;
                bool busy = (bitand > 0);
                return busy;
            }
            else
                return false;
        }

        private void SetEncoderCount(int axis, int positionCount)
        {
            byte[] command = new byte[12];
            byte[] header = new byte[] { 0x09, 0x04, 0x06, 0x00, 0x00, 0x00, (byte)axis, 0x00 };
            Buffer.BlockCopy(header, 0, command, 0, header.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(positionCount), 0, command, 8, 4);
            serial.SendCommand(command);
        }

        public void GoToPositionCount(int axis, int positionCount)
        {
            byte[] command = new byte[12];
            byte[] header = new byte[] { 0x53, 0x04, 0x06, 0x00, 0x00, 0x00, (byte)axis, 0x00 };
            Buffer.BlockCopy(header, 0, command, 0, header.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(positionCount), 0, command, 8, 4);
            serial.SendCommand(command);
        }


        public int GetPositionCount(int axis, out int pos) //0, 1, 2
        {
            byte[] command = new byte[] { 0x0A, 0x04, (byte)axis, 0x00, 0x00, 0x00 };
            bool done = serial.SendAndWait(command, 6 + 6, out byte[] response);
            StringCode = serial.GetStringCode();

            if (done && response.Length >= 6 + 6)
            {
                int ch = (int)BitConverter.ToInt16(response, 6);
                bool success = response[0] == 0x0b && response[1] == 0x04 && response[2] == 0x06;
                success = success && ch == axis;
                if (success)
                {
                    pos = BitConverter.ToInt32(response, 8);
                    return 1;
                }
                else
                {
                    pos = 0;
                    return 0;
                }
            }
            else
            {
                pos = 0;
                return 0;
            }
        }


        private void StopAxis(int axis)
        {
            byte[] command = new byte[] { 0x65, 0x04, (byte)axis, 0x01, 0x00, 0x00 };
            serial.SendCommand(command);
        }

        public enum Thor3000Type
        {
            MCM3001 = 1,
            MCM3002 = 2,
            MCM3003 = 3,
            BScope = 4,
            Bergamo = 5,
        }
    }
}
