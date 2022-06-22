using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroscopeHardwareLibs
{
    public class ThorECU
    {
        SerialCommandObj serial;
        int boudRate = 115200;
        double PMT_GAIN_POWER = 6.64;
        double GAIN_L_BOUND = 0.00002;
        double GAIN_H_BOUND = 0.25;
        int NUM_TWOWAY_ZONES = 251;
        int PMT_GAIN_MIN = 0;
        int PMT_GAIN_MAX = 100;
        int PMT_GAIN_DEFAULT = 0;
        int SCANNER_ZOOM_MIN = 0;
        int SCANNER_ZOOM_MAX = 255;
        int SCANNER_ZOOM_DEFAULT = 120;
        string port_name;
        public bool old;


        public ThorECU(String portStr, string portName, bool old_)
        {
            serial = new SerialCommandObj(portStr, boudRate, "ThorECU");
            port_name = portName;
            old = old_;
        }

        public int OpenPort()
        {
            int ret = serial.OpenPort();

            //Test if they have "zoom" command!
            string s = serial.SendAndWaitCR("pmt1?\r");
            Debug.WriteLine(s);

            //if (s.Contains("Invalid"))
            //    old = true;

            return ret;
        }

        public void ClosePort()
        {
            serial.ClosePort();
        }

        public void SetZoomOfScanner(int zoom)
        {
            if (!old)
            {
                serial.SendAndWaitCR("zoom=" + zoom + "\r");
            }
            else
            {
                if (zoom > SCANNER_ZOOM_MAX)
                    zoom = SCANNER_ZOOM_MAX;
                else if (zoom < SCANNER_ZOOM_MIN)
                    zoom = SCANNER_ZOOM_MIN;

                SetZoomOfScanner_old(zoom, port_name);
            }
        }

        private void SetZoomOfScanner_old(int zoom, string port)
        {
            byte[] portVal = DigPortCommand(zoom);
            for (int i = 0; i < 36; i++)
            {
                System.Threading.Thread.Sleep(1);
                new NiDaq.DigitalOutputPort(port, new byte[] { portVal[i] });
            }
        }

        private byte[] DigPortCommand(int zoom)
        {
            byte[] digPortCmd = new byte[36];
            digPortCmd[0] = 9;
            digPortCmd[1] = 8;
            int i;
            UInt16 BitSelector = 1;
            for (i = 0; i < 8; i++)
            {
                digPortCmd[i * 2 + 2] = (byte)(((200 & BitSelector) > 0) ? 12 : 8);
                digPortCmd[i * 2 + 3] = (byte)(((200 & BitSelector) > 0) ? 14 : 10);
                BitSelector *= 2;
            }
            for (i = 8; i < 16; i++)
            {
                digPortCmd[i * 2 + 2] = (byte)(((zoom & BitSelector) > 0) ? 12 : 8);
                digPortCmd[i * 2 + 3] = (byte)(((zoom & BitSelector) > 0) ? 14 : 10);
                BitSelector /= 2;
            }
            digPortCmd[34] = 9;
            digPortCmd[35] = 9;
            return digPortCmd;
        }

        public void SetAlignmentOfScanner(int alignment)
        {
            if (!old)
                serial.SendAndWaitCR("scanalign=" + alignment + "\r");
        }

        public void EnableScanner(bool enable)
        {
            int onoff = enable ? 1 : 0;
            serial.SendAndWaitCR("scan=" + onoff + "\r");
        }

        public string TurnOnPMT(int pmtN, int on)
        {
            string s = serial.SendAndWaitCR("pmt" + pmtN + "=" + on + "\r");
            if (old)
                s = serial.SendAndWaitCR("pmt?\r");
            else
                s = serial.SendAndWaitCR("pmt" + pmtN + "?" + "\r");
            return s;
        }

        public string SetPMTGain(int pmtN, int value)
        {
            string sendStr = "pmt" + pmtN + "gain=" + value;
            string s;
            if (!old)
            {
                //s = serial.SendAndWait("pmt" + pmtN + "?" + "\r");
                s = serial.SendAndWaitCR(sendStr + "\r");
                s = serial.SendAndWaitCR("pmt" + pmtN + "?\r");
            }
            else
            {
                s = serial.SendAndWaitCR(sendStr + "\r");
                s = serial.SendAndWaitCR("pmt?\r");
            }
            return s;
        }

        public int GetPMTGain(int pmtN)
        {
            int value = 0;
            string s;
            if (!old)
                s = serial.SendAndWaitCR("pmt" + pmtN + "gain?");
            else
                s = serial.SendAndWaitCR("pmt?");
            Int32.TryParse(s, out value);
            return value;
        }
    }
}

