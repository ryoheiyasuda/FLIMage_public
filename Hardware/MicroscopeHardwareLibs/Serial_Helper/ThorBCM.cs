using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroscopeHardwareLibs
{
    public class ThorBCM
    {
        SerialCommandObj serial;
        int boudRate = 460800;

        const int GALVO_GALVO = 0;
        const int GALVO_RESONANT = 1;
        const int CAMERA = 2;
        const int DEVICE_NUM = 3;


        public ThorBCM(String portStr)
        {
            serial = new SerialCommandObj(portStr, boudRate, "ThorBCM");
        }

        public int OpenPort()
        {
            return serial.OpenPort();
        }

        public void ClosePort()
        {
            serial.ClosePort();
        }

        public void TurnOnOff(int on)
        {
            byte[] command;
            if (on == 0)
                command = new byte[] { 0x2f, 0x4d, 0x30, 0x52, 0x0d };
            else
                command = new byte[] { 0x2f, 0x4d, 0x31, 0x52, 0x0d };
            serial.SendCommand(command);
        }
    }
}

