using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace MicroscopeHardwareLibs
{

    public class ThorMCM301
    {
        string port;
        int nBound;
        private string sn;
        private int hdl;

        public double[] conversionFactor = new double[3];
        public double[] maxPos = new double[3];
        public double[] minPos = new double[3];

        public ThorMCM301(String portname, int boundRate)
        {
            port = portname;
            nBound = boundRate;
            sn = GetSerial(); // Currently use sn to connect but not com port
        }

        private string GetSerial()
        {
            byte[] value = new byte[2048];
            var count = MCM301CommandProvider.List(value, 2048);
            if (count > 0)
            {
                var serialArray = System.Text.Encoding.UTF8.GetString(value);
                return serialArray.Split(',')[0]; // Use the first sn for test
            }

            return "";
        }

        public int OpenPort()
        {
            var res = MCM301CommandProvider.Open(sn, nBound, 3);
            if (res < 0)
                return -1;

            hdl = res;
            GetStageParams();
            return 0;
        }

        public void ClosePort()
        {
            MCM301CommandProvider.Close(hdl);
        }

        public int GetPosition(double[] pos)
        {
            for (int i = 0; i < 3; i++)
            {
                //pos[i] = 5;

                byte slot = (byte)(i + 4);

                uint status = 0;
                var ret = MCM301CommandProvider.GetPnpStatus(hdl, slot, ref status);

                if (ret == 0 && status == 0)
                {
                    int current_encoder = 0;
                    if (MCM301CommandProvider.GetMotStatus(hdl, slot, ref current_encoder, ref status) == 0)
                    {
                        double nm = 0;
                        if (MCM301CommandProvider.ConvertEncoderTonm(hdl, slot, current_encoder, ref nm) == 0)
                            pos[i] = nm;
                        else
                            return 0;
                    }
                    else
                        return 0;
                }
                else
                    return 0;
            }

            return 1;
        }

        public int GoToPosition(int axis, double position)
        {
            if (position < minPos[axis] || position > maxPos[axis])
                return 0;

            byte slot = (byte)(axis + 4);
            int current_encoder = 0;
            uint status = 0;
            MCM301CommandProvider.GetMotStatus(hdl, slot, ref current_encoder, ref status);

            if (MCM301CommandProvider.ConvertnmToEncoder(hdl, slot, position, ref current_encoder) == 0)
            {
                if (MCM301CommandProvider.MoveAbsolute(hdl, slot, current_encoder) == 0)
                    return 1;
            }
            return 0;
        }

        public void ZeroPosition(int axis)
        {
            byte slot = (byte)(axis + 4);
            MCM301CommandProvider.Home(hdl, slot);
        }

        public void Stop()
        {
            for (byte i = 4; i < 7; i++)
                MCM301CommandProvider.Stop(hdl, i);
        }

        public void SetVelocity(int percentage)
        {
            for (byte i = 4; i < 7; i++)
            {
                MCM301CommandProvider.SetVelocity(hdl, i, 0, (byte)percentage);
                //MCM301CommandProvider.SetVelocity(hdl, i, 1, (byte)percentage);
            }
        }

        public bool IsMotorBusy(int axis)
        {
            byte slot = (byte)(axis + 4);

            uint status = 0;
            MCM301CommandProvider.GetPnpStatus(hdl, slot, ref status);
            if (status == 0)
            {
                int current_encoder = 0;
                if (MCM301CommandProvider.GetMotStatus(hdl, slot, ref current_encoder, ref status) == 0)
                {
                    if ((status & 0x10) == 0x10 || (status & 0x20) == 0x20 || (status & 0x40) == 0x40 || (status & 0x80) == 0x80 || (status & 0x200) == 0x200)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void GetStageParams()
        {
            for (int i = 0; i < 3; i++)
            {
                //minPos[i] = -1000;
                //maxPos[i] = 1000;

                byte slot = (byte)(i + 4);
                StageParamsInfoStruct stageParamsInfoStruct = new StageParamsInfoStruct();

                if (MCM301CommandProvider.GetStageParams(hdl, slot, ref stageParamsInfoStruct) == 0)
                {
                    conversionFactor[i] = stageParamsInfoStruct.nm_per_count;

                    double nm = 0;
                    if (MCM301CommandProvider.ConvertEncoderTonm(hdl, slot, (int)stageParamsInfoStruct.minimum_position, ref nm) == 0)
                        minPos[i] = nm;
                    if (MCM301CommandProvider.ConvertEncoderTonm(hdl, slot, (int)stageParamsInfoStruct.maximum_position, ref nm) == 0)
                        maxPos[i] = nm;
                }
            }
        }
    }
}
