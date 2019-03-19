using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace FLIMimage
{
    public class COMserver
    {
        FLIMageMain mc;
        public Boolean connected = false;
        public Boolean connectedR = false;

        bool connecting = false;
        bool connectingR = false;

        NamedPipeServerStream pipeServer, pipeServerR;
        public StreamString ss, ssR;
        public String Received, ReceivedR;

        public event ReadHandler r_tick;
        public EventArgs e = EventArgs.Empty;
        public delegate void ReadHandler(COMserver cs, EventArgs e);

        String SNameR = "FLIMageW"; //Name must be opposite from Client for writing and reading
        String SName = "FLIMageR";

        public COMserver(FLIMageMain f)
        {
            mc = f;
        }

        public void start()
        {
            start(FLIMage_Event.CommandReceivedFrom.Client);
            start(FLIMage_Event.CommandReceivedFrom.FLIMage);
        }

        public void start(FLIMage_Event.CommandReceivedFrom rw)
        {
            mc.script.status_ComServer(false, rw);
            Thread t1;

            if (rw == FLIMage_Event.CommandReceivedFrom.Client)
            {
                pipeServer = new NamedPipeServerStream(SName, PipeDirection.InOut, 254, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                connecting = true;
                t1 = new Thread(() => startEach(SName, pipeServer));
            }
            else
            {
                pipeServerR = new NamedPipeServerStream(SNameR, PipeDirection.InOut, 254, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                connectingR = true;
                t1 = new Thread(() => startEach(SNameR, pipeServerR));
            }

            t1.Start();
        }

        public void startEach(string str, NamedPipeServerStream pp)
        {
            try
            {
                pp.WaitForConnection();

                if (str == SName)
                    connecting = false;
                else
                    connectingR = false;

                StreamString ss1 = new StreamString(pp);
                Boolean success = handShake(ss1);

                if (str == SName)
                {
                    connected = success;
                    mc.script.status_ComServer(success, FLIMage_Event.CommandReceivedFrom.Client);
                    ss = ss1;
                }
                else
                {
                    connectedR = success;
                    mc.script.status_ComServer(success, FLIMage_Event.CommandReceivedFrom.FLIMage);
                    ssR = ss1;
                    StartReceiveThread();
                }

                if (success)
                    Debug.WriteLine("Connected: " + str);
                else
                    Debug.WriteLine("Failed: " + str);
            }
            catch (IOException e)
            {
                Debug.WriteLine("Pipe busy: " + e.Message);
            }
            catch (ObjectDisposedException e)
            {
                Debug.WriteLine("Already Closed: " + e.Message);
            }

        }

        Boolean handShake(StreamString ss0)
        {
            Boolean result = false;
            try
            {
                ss0.WriteString("FLIMage");
                Received = ss0.ReadString();
                if (Received == "FLIMage")
                {
                    result = true;
                    ss0.WriteString("Connected");
                }
            }
            catch (IOException e)
            {
                Debug.WriteLine("ERROR: {0}", e.Message);
            }
            return result;
        }

        void StartReceiveThread()
        {
            Thread th = new Thread(receivingCommands);
            th.Start();
        }

        void receivingCommands()
        {
            while (connectedR)
            {
                try
                {
                    ReceivedR = ssR.ReadString();
                    r_tick?.Invoke(this, e);
                    //if (r_tick != null) 
                    //    r_tick(this, e);
                }
                catch (IOException e)
                {
                    Debug.WriteLine("Failed receiving commands: " + e.Message);
                    connectedR = false;
                    //Restart();
                }
            }
        }

        public void sendCommand(string str)
        {
            if (str != "" && connected)
            {
                connected = false;
                Thread th = new Thread(() => sendCommandThread(str));
                th.Start();
                //                ss.WriteString(str);
            }
        }

        private void sendCommandThread(string str)
        {
            int ret = ss.WriteString(str);
            if (ret != 0)
                connected = true;
        }

        public void Restart()
        {
            Close();
            start();
        }

        public void Close()
        {
            if (connecting)
            {
                NamedPipeClientStream clt = new NamedPipeClientStream(".", SName);
                clt.Connect();
                clt.Close();
                connecting = false;
            }

            if (connectingR)
            {
                NamedPipeClientStream clt = new NamedPipeClientStream(".", SNameR);
                clt.Connect();
                clt.Close();
                connectingR = false;
            }

            if (pipeServer != null)
            {
                pipeServer.Close();
                pipeServer.Dispose();
            }

            if (pipeServerR != null)
            {
                pipeServerR.Close();
                pipeServerR.Dispose();
            }

            connected = false;
            connectedR = false;
        }

        public class StreamString
        {
            private Stream ioStream;
            private Encoding streamEncoding;

            public StreamString(Stream ioStream)
            {
                this.ioStream = ioStream;
                streamEncoding = Encoding.UTF8;

                //ioStream.WriteTimeout = 100;
                //ioStream.ReadTimeout = 100;
            }

            public string ReadString()
            {
                String result = "";
                try
                {
                    int len, len1;
                    len = ioStream.ReadByte() * 256;
                    len += ioStream.ReadByte();
                    len1 = len;
                    if (len < 1)
                        len1 = 1;

                    byte[] inBuffer = new byte[len1];

                    if (len > 0)
                    {
                        ioStream.Read(inBuffer, 0, len);
                        result = streamEncoding.GetString(inBuffer);
                    }
                }
                catch (TimeoutException e)
                {
                    Debug.WriteLine("Time out" + e);
                }
                catch (ObjectDisposedException e)
                {
                    Debug.WriteLine("Read String error: " + e);
                }

                return result;
            }

            public int WriteString(object obj)
            {
                byte[] outBuffer = null;

                if (obj == null)
                    return 0;

                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    outBuffer = ms.ToArray();
                }

                return WriteString(outBuffer);
            }

            public int WriteString(String outString)
            {
                byte[] outBuffer = streamEncoding.GetBytes(outString);
                return WriteString(outBuffer);
            }

            public int WriteString(byte[] outBuffer)
            {
                int len = outBuffer.Length;
                if (len < 0)
                    return 0;

                if (len > UInt16.MaxValue)
                {
                    len = (int)UInt16.MaxValue;
                }

                try
                {
                    ioStream.WriteByte((byte)(len / 256));
                    ioStream.WriteByte((byte)(len & 255));
                    ioStream.Write(outBuffer, 0, len);

                    ioStream.Flush();

                    return len + 2;
                }
                catch (OperationCanceledException e)
                {
                    Debug.WriteLine("Canceled" + e);
                    return 0;
                }
                catch (TimeoutException e)
                {
                    Debug.WriteLine("Time out" + e);
                    return 0;
                }
                catch (IOException e)
                {
                    Debug.WriteLine("Write String error IOException: " + e);
                    return 0;
                }
                catch (ObjectDisposedException e)
                {
                    Debug.WriteLine("Write String error: " + e);
                    return 0;
                }
            }//WriteString
        } //Class streaming

    }

} //Name space

