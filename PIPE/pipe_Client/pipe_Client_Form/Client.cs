using System;
using System.Windows.Forms;
using pipe_Client;
using System.Threading;

namespace pipe_Client_Form
{
    public partial class Client : Form
    {
        pipeClient pC;
        public Client()
        {
            InitializeComponent();
            pC = new pipeClient();
            pC.r_tick += new pipeClient.ReadHandler(messageReceived);
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            String str = Send.Text;
            Receive.Text = pC.sendCommand(str);
        }

        private void sendCommand(String str)
        {
            pC.sendCommand(str);
        }

        private void messageReceived(pipeClient p, EventArgs e)
        {
            Receive.BeginInvoke((Action)delegate ()
               {
                   Receive.Text = pC.Received;
               });
        }

        private void StartReceive_Click(object sender, EventArgs e)
        {
            pC.startReceiving();
        }
    }
}
