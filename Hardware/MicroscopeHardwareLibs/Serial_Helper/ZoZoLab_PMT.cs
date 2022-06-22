using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroscopeHardwareLibs
{

    public class ZoZoLab_PMT
    {
        SerialCommandObj serial;
        public string StringCode; //For debugging.
        private string[] PMT_ID = { "A", "B", "C", "D" };
        private int maxNPMT = 2;

        public ZoZoLab_PMT(String portname)
        {
            int boundRate = 115200;
            serial = new SerialCommandObj(portname, boundRate, "ZoZoLab");
        }

        public int OpenPort()
        {
            int success = serial.OpenPort();
            System.Threading.Thread.Sleep(100);

            success = serial.WaitForResponse(out string str) ? 1 : 0;

            return success;
        }

        public void ClosePort()
        {
            serial.ClosePort();
        }

        public void ClearBuffer()
        {
            serial.ClearBuffer();
        }

        public void TurnOnPMT(int pmtN, int on)
        {
            if (pmtN < 1)
                pmtN = 1;
            serial.SendCommand("pmt" + PMT_ID[pmtN - 1] + on + "\r");
        }


        public int SetPMTGain(int pmtN, double valueD)
        {
            if (pmtN < 1)
                pmtN = 1;
            if (pmtN >= maxNPMT)
                pmtN = maxNPMT;

            int value = (int)(valueD / 5.0 * 1024.0 / 1000.0);
            string command = "v_i" + PMT_ID[pmtN - 1] + value + "\r";
            serial.SendCommand(command);
            return 1;
        }

        public int GetPMTGain(int pmtN)
        {
            if (pmtN < 1)
                pmtN = 1;
            if (pmtN >= maxNPMT)
                pmtN = maxNPMT;

            string s = serial.SendAndWaitCR("v_m" + PMT_ID[pmtN - 1]);
            Int32.TryParse(s, out int value);
            double valueD = (double)(value / 1024 * 5.0 * 1000);
            return value;
        }

        public int Stop()
        {
            serial.SendCommand("s");
            return 1;
        }

    }
}
