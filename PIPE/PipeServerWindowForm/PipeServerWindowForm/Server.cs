using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Windows.Forms;
using System.Threading;

namespace PipeServerWindowForm
{
    public partial class Server : Form
    {
        Boolean connected = false;
        NamedPipeServerStream pipeServer, pipeServerR;
        StreamString ss, ssR;
        String Received, ReceivedR;

        public Server()
        {
            InitializeComponent();
        }

        private void Server_Load(object sender, EventArgs e)
        {
            this.Hide();
            this.Show();

            Application.DoEvents();
            Thread s_pipes = new Thread(startServer);
            s_pipes.Start();
        }

        void startServer()
        {
            try
            {
                pipeServer = new NamedPipeServerStream("FLIMage", PipeDirection.InOut, 1);
                pipeServerR = new NamedPipeServerStream("FLIMageR", PipeDirection.InOut, 1);
                sendButton.BeginInvoke((Action)delegate ()
                {
                    sendButton.Text = "Searching client...";
                });

                pipeServer.WaitForConnection();
                pipeServerR.WaitForConnection();

                sendButton.BeginInvoke((Action)delegate ()
                {
                    sendButton.Text = "Send";
                });

                ss = new StreamString(pipeServer);
                ssR = new StreamString(pipeServerR);
                connected = handShake(ss) && handShake(ssR);

                if (connected)
                    receivingCommands();

            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine("Already Closed");
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
                Console.WriteLine("ERROR: {0}", e.Message);
            }
            return result;
        }


        void receivingCommands()
        {
            while (connected)
            {
                ReceivedR = ssR.ReadString();
                if (ReceivedR == "")
                {
                    connected = false;
                    pipeServer.Close();
                    pipeServerR.Close();
                    startServer();

                    return;
                }

                ReceiveText.BeginInvoke((Action)delegate ()
                {
                    ReceiveText.Text = ReceivedR;
                });

                ssR.WriteString(ReceivedR + ": Received");
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            sendCommand();
            //Thread sendC = new Thread(sendCommand);
            //sendC.Start();
        }

        private void sendCommand()
        {
            if (sendButton.Text == "Send")
            {
                if (Send.Text != "")
                    ss.WriteString(Send.Text);
            }
        }


        public class StreamString
        {
            private Stream ioStream;
            private UnicodeEncoding streamEncoding;

            public StreamString(Stream ioStream)
            {
                this.ioStream = ioStream;
                streamEncoding = new UnicodeEncoding();
            }

            public string ReadString()
            {
                int len, len1;
                len = ioStream.ReadByte() * 256;
                len += ioStream.ReadByte();
                len1 = len;
                if (len < 1)
                    len1 = 1;

                byte[] inBuffer = new byte[len1];
                String result;
                if (len > 0)
                {
                    ioStream.Read(inBuffer, 0, len);
                    result = streamEncoding.GetString(inBuffer);
                }
                else
                    result = "";

                return result;
            }

            public int WriteString(string outString)
            {
                if (outString == "")
                    return 0;

                byte[] outBuffer = streamEncoding.GetBytes(outString);
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

                    return outBuffer.Length + 2;
                }
                catch (IOException e)
                {
                    return 0;
                }
                catch (ObjectDisposedException e)
                {
                    return 0;
                }
            }
        }

        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            connected = false;
            if (pipeServer != null)
                pipeServer.Close();
            if (pipeServerR != null)
                pipeServerR.Close();

            Environment.Exit(0);
        }
    }
}

