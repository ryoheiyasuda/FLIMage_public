using System;
using System.Text;
using System.Security.Principal;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace pipe_Client
{
    public class pipeClient
    {
        NamedPipeClientStream pipeClientW, pipeClientR;
        public bool Client_On = false;
        public String Received;

        public event ReadHandler r_tick;
        public EventArgs e = EventArgs.Empty;
        public delegate void ReadHandler(pipeClient pC, EventArgs e);

        public String HandShakeCode = "FLIMage";

        public pipeClient()
        {
            pipeClientR = new NamedPipeClientStream(".", "FLIMageR", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            pipeClientW = new NamedPipeClientStream(".", "FLIMageW", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

            Console.WriteLine("Connecting to server...\n");

            try
            {
                pipeClientR.Connect(100);
                pipeClientW.Connect(100);
            }
            catch (TimeoutException e)
            {
                Received = "Time out error";
                Client_On = false;
            }
            catch (Exception e2)
            {
                Client_On = false;
            }
            
            bool b1 = connect(pipeClientR);
            bool b2 = connect(pipeClientW);

            Client_On = b1 && b2;
        }

        public bool connect(NamedPipeClientStream pipeClient1)
        {
            bool connected = false;
            try
            {
                String readText = ReadMessage(pipeClient1);
                if (readText == HandShakeCode)
                {
                    connected = true;
                    SendMessage(pipeClient1, HandShakeCode);
                    readText = ReadMessage(pipeClient1);
                }
                else
                    Console.WriteLine("Server could not be verified.");

            }
            catch (ObjectDisposedException e)
            {
            }
            return connected;
        }

        public void startReceiving()
        {
            Thread receivTh = new Thread(startReceivingCore);
            receivTh.Start();
        }

        public void startReceivingCore()
        {
            while (Client_On)
            {
                Received = ReadMessage(pipeClientR);

                if (Received == "")
                {
                    Client_On = false;
                    Received = "Crushed?";
                    return;
                }
                else
                    r_tick(this, e);
                //Make event!!
            }
        }

        public String ReadMessage(NamedPipeClientStream pipeClient1)
        {
            String reply = "";
            var data1 = pipeClient1.ReadByte();
            var data2 = pipeClient1.ReadByte();
            var l_data = data1 * 256 + data2;
            var readBytes = new byte[l_data];
            var readCount = pipeClient1.Read(readBytes, 0, l_data);

            if (readCount > 0)
                reply = Encoding.Default.GetString(readBytes, 0, readCount);

            return reply;
        }

        public void SendMessage(NamedPipeClientStream pipeClient1, String str1)
        {
            byte[] sendByte = Encoding.ASCII.GetBytes(str1);
            int data1 = sendByte.Length / 256;
            int data2 = sendByte.Length & 255;
            pipeClient1.WriteByte((byte)data1);
            pipeClient1.WriteByte((byte)data2);
            pipeClient1.Write(sendByte, 0, sendByte.Length);
        }

        public String sendCommand(String str)
        {
            String strR = "";
            if (str != "")
            {
                SendMessage(pipeClientW, str);
            }
            return strR;
        }

        public void Close()
        {
            if (pipeClientW != null)
                pipeClientW.Close();
            if (pipeClientR != null)
                pipeClientR.Close();

        }
    }
    
}
