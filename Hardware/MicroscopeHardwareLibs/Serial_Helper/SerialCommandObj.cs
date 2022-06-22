using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroscopeHardwareLibs
{
    public class SerialCommandObj
    {
        public SerialPort port;
        public String COMport = "COM32";
        public int boudRate = 9600;

        public string mode = "SutterMP285";

        int sub_wait = 10;
        int waitTimeMilli = 500;
        bool waitForData = false;
        public byte[] returnBytes = new byte[1];
        public string returnString = "";
        Char newLineChar = '\r';
        bool waitingCR = false;

        int expectedLength = 0;

        object waitForDataObj = new object();

        public SerialCommandObj(String portStr, int boud_rate, string _mode)
        {
            COMport = portStr;
            boudRate = boud_rate;
            mode = _mode;
        }

        public int OpenPort()
        {
            var portNames = SerialPort.GetPortNames();

            bool available = false;
            foreach (var port in portNames)
            {
                if (port == COMport)
                {
                    available = true;
                    break;
                }
            }

            if (!available)
                return 0;

            port = new SerialPort(COMport);
            port.BaudRate = boudRate;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.NewLine = newLineChar.ToString();
            port.Handshake = Handshake.None;
            port.RtsEnable = true;
            port.DtrEnable = true;
            port.ReadTimeout = waitTimeMilli;
            port.WriteTimeout = waitTimeMilli;
            //port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            try
            {
                port.Open();
                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public void ClosePort()
        {
            if (port != null && port.IsOpen)
                port.Close();
        }

        public void ClearBuffer()
        {
            try
            {
                if (port != null && !port.IsOpen)
                {
                    return;
                }

                if (port.BytesToRead > 0)
                {
                    System.Threading.Thread.Sleep(sub_wait * 2);
                    port.ReadExisting();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void SendCommand(string Command)
        {
            if (port != null && !port.IsOpen)
            {
                return;
            }

            expectedLength = 0;
            ClearBuffer();
            port.Write(Command);
        }

        public void SendCommand(byte[] Command)
        {
            if (port != null && !port.IsOpen)
            {
                return;
            }

            expectedLength = 0;
            ClearBuffer();
            port.Write(Command, 0, Command.Length);
        }

        //
        //Wiat for data needs to be turend on before calling this.
        private bool ReadBuffer_Simple(out byte[] ret, int expected_length, bool waitCR) //Done = true
        {
            if (port!= null && !port.IsOpen)
            {
                ret = null;
                return false;
            }

            expectedLength = expected_length;
            byte[] buf = new byte[1024];

            if (mode != "SutterMP285" || waitCR)
            {
                try
                {
                    for (int i = 0; i < waitTimeMilli / sub_wait; i++)
                    {
                        System.Threading.Thread.Sleep(sub_wait);
                        int bytesToRead = port.BytesToRead;
                        if (bytesToRead > 0)
                        {
                            System.Threading.Thread.Sleep(sub_wait);
                            returnString = port.ReadLine();
                            returnBytes = Encoding.ASCII.GetBytes(returnString);
                            waitForData = false;
                            waitingCR = false;
                            break;
                        }
                    }

                }
                catch (Exception EX)
                {
                    Debug.WriteLine(EX.Message);
                }
            }
            else
            {
                for (int i = 0; i < waitTimeMilli / sub_wait; i++) //Total waitTimeMilli/100 * 100 = 500 msec.
                {
                    System.Threading.Thread.Sleep(sub_wait);
                    if (port != null && !port.IsOpen)
                    {
                        ret = null;
                        return false;
                    }

                    int bytesToRead = port.BytesToRead;

                    //Debug.WriteLine("BytesToRead = " + bytesToRead + " expected = " + expectedLength + " i = " + i);

                    if (bytesToRead >= expected_length)
                    {
                        port.Read(buf, 0, bytesToRead);

                        if (buf[expected_length - 1] == newLineChar)
                        {
                            waitForData = false;
                            Array.Resize(ref buf, bytesToRead);
                            returnBytes = buf;
                            returnString = System.Text.Encoding.ASCII.GetString(buf);
                        }
                        break;
                    }
                }

                if (waitForData)
                {
                    if (port.BytesToRead > 0 && mode == "SutterMP285") //This may allow us to potentially detect crush.
                    {
                        System.Threading.Thread.Sleep(sub_wait);
                        int size = port.Read(buf, 0, port.BytesToRead);
                        Array.Resize(ref buf, size);
                        returnBytes = buf;
                        returnString = System.Text.Encoding.ASCII.GetString(buf);
                    }
                }
            }           

            ret = returnBytes;
            return !waitForData;
        }

        public bool SendSignalAndWaitForCR(string command, out string ret) //Need specifically for ZoZoLab.
        {
            if (port != null && !port.IsOpen)
            {
                ret = null;
                return false;
            }

            lock (waitForDataObj)
            {
                waitingCR = true;
                waitForData = true;

                if (command != "")
                    port.Write(command);

                bool success = ReadBuffer_Simple(out byte[] ret_byte, 1024, true);

                ret = returnString;

                waitForData = false;
                return success;
            }
        }

        public bool WaitForCR(out string ret)
        {
            if (port != null && !port.IsOpen)
            {
                ret = null;
                return false;
            }

            lock (waitForDataObj)
            {
                return SendSignalAndWaitForCR("", out ret);
            }
        }

        public bool WaitForResponse(out string ret)
        {
            if (port != null && !port.IsOpen)
            {
                ret = null;
                return false;
            }

            lock (waitForDataObj)
            {
                waitForData = true;

                for (int i = 0; i < waitTimeMilli / 25 * 2; i++) //Total waitTimeMilli/100 * 100 = 500 msec.
                {
                    System.Threading.Thread.Sleep(25);
                    if (!waitForData)
                        break;
                }

                ret = returnString;

                bool success = !waitForData;
                waitForData = false;

                return success;
            }
        }

        public bool SendAndWait(byte[] Command, int expected_length, out byte[] ret)
        {
            if (port != null && !port.IsOpen)
            {
                ret = null;
                return false;
            }

            lock (waitForDataObj)
            {
                waitForData = true;
                ClearBuffer();
                port.Write(Command, 0, Command.Length);
                var done = ReadBuffer_Simple(out ret, expected_length, false);

                expected_length = 0;
                return done;
            }
        }

        public bool SendAndWait(string Command, int expected_length, out byte[] ret)
        {
            if (port != null && !port.IsOpen)
            {
                ret = null;
                return false;
            }

            lock (waitForDataObj)
            {
                waitForData = true;
                ClearBuffer();
                port.Write(Command);
                var done = ReadBuffer_Simple(out ret, expected_length, false);
                return done;
            }
        }

        public string SendAndWaitCR(string Command)
        {
            if (port != null && !port.IsOpen)
            {
                return "";
            }

            lock (waitForDataObj)
            {
                waitForData = true;
                expectedLength = 1024;

                if (port == null)
                    return "";

                ClearBuffer();
                port.Write(Command);
                ReadBuffer_Simple(out byte[] ret, expectedLength, true);
                return returnString;
            }
        }

        public string GetStringCode()
        {
            string strA = "";
            if (returnBytes != null)
                strA = String.Join("-", returnBytes.Select(x => x.ToString()).ToList());
            return strA;
        }

        //public void port_DataReceived(object sender, SerialDataReceivedEventArgs e_serial)
        //{
        //    //int bytesToRead = port.BytesToRead;
        //    if (mode != "SutterMP285" || waitingCR)
        //    {
        //        try
        //        {
        //            returnString = port.ReadLine();
        //            returnBytes = Encoding.ASCII.GetBytes(returnString);
        //            waitForData = false;
        //            waitingCR = false;
        //        }
        //        catch (Exception EX)
        //        {
        //            Debug.WriteLine(EX.Message);
        //        }
        //    }
        //    else
        //    {
        //        byte[] buf = new byte[1024];
        //        int buf_read = 0;

        //        if (!waitForData)
        //            return;

        //        for (int i = 0; i < waitTimeMilli / 100; i++)
        //        {
        //            System.Threading.Thread.Sleep(100);

        //            int bytesToRead = port.BytesToRead;

        //            if (bytesToRead > 0)
        //                Debug.WriteLine("BytesToRead = " + bytesToRead + " expected = " + expectedLength + " i = " + i);

        //            port.Read(buf, buf_read, bytesToRead);
        //            buf_read += bytesToRead;

        //            if (expectedLength <= buf_read)
        //                break;

        //            if (mode != "SutterMP285" && buf[buf_read - 2] == newLineChar)
        //                break;

        //            if (mode == "SutterMP285" && buf_read >= 2)
        //            {
        //                if (buf[buf_read - 1] == newLineChar && buf[buf_read - 2] == newLineChar) //it is likely return. You may be extremely unlucky though.
        //                {
        //                    break;
        //                }
        //                if (buf[buf_read - 1] == 48 && buf[buf_read - 2] == 48) //Crash
        //                {
        //                    break;
        //                }
        //                if (buf[0] == 48 && buf_read == 1)
        //                    break;
        //            }
        //        }

        //        Array.Resize(ref buf, buf_read);
        //        returnBytes = buf;
        //        returnString = System.Text.Encoding.ASCII.GetString(buf);
        //        waitForData = false;
        //    }
        //}
    }
}
