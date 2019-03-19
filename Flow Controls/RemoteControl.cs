using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace FLIMimage
{
    public partial class Script : Form
    {
        public FLIMageMain FLIMage;
        public FLIMage_Event FLIM_event;
        public COMserver COM_server;
        public TextServer text_server;
        public ScanParameters State;

        NotificationTable nf;

        WindowLocManager winManager;
        String WindowName = "RemoteControl.loc";

        public Script(FLIMageMain fc)
        {
            InitializeComponent();
            FLIMage = fc;
            FLIM_event = fc.flim_event;
            COM_server = fc.com_server;
            text_server = fc.text_server;
            State = fc.State;
            Connect_With_Text.Checked = State.Files.useCommandFile;
            SetCommandFilePath();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Execute_Click(object sender, EventArgs e)
        {
            String code = ClientMessageWindow.Text;

            String[] lines = code.Split('\n');
            //String[] ReceivedLines = new String[lines.Length];
            String str = "";
            String writeStr = "";

            for (int i = 0; i < lines.Length; i++)
            {
                String str1 = FLIM_event.ExecuteReceivedCommand(lines[i], true, out FLIMage_Event.CommandMode cm);
                str = str + str1;
                if (i < lines.Length - 1)
                    str = str + "\r\n";

                if (cm == FLIMage_Event.CommandMode.Get_Parameter || cm == FLIMage_Event.CommandMode.Set_Parameter)
                {
                    writeStr = writeStr + str1;
                    if (i < lines.Length - 1)
                        writeStr = writeStr + "\r\n";
                }
            }

            if (writeStr != "")
            {
                FLIMage.text_server.WriteEventsInCommandFile(writeStr);
            }

            if (str != "")
            {
                displaySendText(str, FLIMage_Event.CommandReceivedFrom.FLIMage);
            }
        }

        public void EventHandling(String EventReceived)
        {
            DisplayNotification(EventReceived);
        }

        private void DisplayNotification(String str)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)delegate
                {
                    EventReceived.Text = str;
                });
            }
            else
                EventReceived.Text = str;
        }

        public void SaveWindowLocation()
        {
            winManager.SaveWindowLocation();
        }

        private void Script_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowLocation();

            if (ServerOn.Checked && COM_server.connected)
            {
                COM_server.Close();
                //FLIM_event.unSubscribe();
                ServerOn.Checked = false;
            }
            this.Hide();
            e.Cancel = true;

            FLIMage.ToolWindowClosed();
        }

        public void displaySendTextCore(String str, FLIMage_Event.CommandReceivedFrom wr)
        {
            if (wr == FLIMage_Event.CommandReceivedFrom.Client)
                ClientMessageWindow.Text = str;
            else
                FLIMageMessageWindow.Text = str;
        }

        public void displayStatusTextCore(String str, FLIMage_Event.CommandReceivedFrom wr)
        {
            if (wr == FLIMage_Event.CommandReceivedFrom.Client)
                ServerStatus_Client.Text = str;
            else
                ServerStatus_FLIMage.Text = str;
        }

        public void displaySendText(String str, FLIMage_Event.CommandReceivedFrom wr)
        {
            if (this.InvokeRequired)
            {
                ServerOn.BeginInvoke((Action)delegate ()
                {
                    displaySendTextCore(str, wr);
                });
            }
            else
            {
                displaySendTextCore(str, wr);
            }
        }

        public void messageReceived(COMserver c, EventArgs e)
        {
            displaySendText(c.ReceivedR, FLIMage_Event.CommandReceivedFrom.FLIMage);
            SetCommandFilePath();
        }

        public void displayStatusText(String str, FLIMage_Event.CommandReceivedFrom wr)
        {
            if (this.InvokeRequired)
            {
                ServerOn.BeginInvoke((Action)delegate ()
                {
                    displayStatusTextCore(str, wr);
                });
            }
            else
            {
                displayStatusTextCore(str, wr);
            }
        }

        public void status_ComServer(bool active, FLIMage_Event.CommandReceivedFrom wr)
        {
            String str;
            if (!active)
                str = "Searching client...";
            else
                str = "Connected";
            displayStatusText(str, wr);
        }

        private void ServerOn_Click(object sender, EventArgs e)
        {
            COM_server.Close();
            //FLIM_event.unSubscribe();
            if (ServerOn.Checked && !COM_server.connected)
            {
                //FLIMage.FLIM_EventHandling_Init();
                COM_server.start();
                status_ComServer(COM_server.connected, FLIMage_Event.CommandReceivedFrom.Client);
                status_ComServer(COM_server.connectedR, FLIMage_Event.CommandReceivedFrom.FLIMage);
            }
            else
            {
                displayStatusText("", FLIMage_Event.CommandReceivedFrom.Client);
                displayStatusText("", FLIMage_Event.CommandReceivedFrom.FLIMage);
            }
        }

        private void Script_Shown(object sender, EventArgs e)
        {
            if (ServerOn.Checked)
            {
                status_ComServer(COM_server.connected, FLIMage_Event.CommandReceivedFrom.Client);
                status_ComServer(COM_server.connectedR, FLIMage_Event.CommandReceivedFrom.FLIMage);
            }
            else
            {
                displayStatusText("", FLIMage_Event.CommandReceivedFrom.Client);
                displayStatusText("", FLIMage_Event.CommandReceivedFrom.FLIMage);
            }
        }

        private void Connect_With_Text_Click(object sender, EventArgs e)
        {
            State.Files.useCommandFile = Connect_With_Text.Checked;
        }

        private void Script_Load(object sender, EventArgs e)
        {
            winManager = new WindowLocManager(this, WindowName, State.Files.windowsInfoPath);
            winManager.LoadWindowLocation(false);
        }

        private void SetCommandFilePath()
        {
            FLIMageFileName.Text = State.Files.FLIMageOutputFileName;
            ClientFileName.Text = State.Files.ClientOutputFileName;
            CommandFilePath.Text = State.Files.commandPathName;
        }

        private void OpenCommandFileDialog(FLIMage_Event.CommandReceivedFrom fileFrom)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = State.Files.commandPathName;

            if (fileFrom == FLIMage_Event.CommandReceivedFrom.Client)
            {
                openFileDialog1.FileName = State.Files.ClientOutputFileName;
                openFileDialog1.Filter = "Choose client output file (*.txt, *.ini)|*.txt; *.ini|All files (*.*)|*.*";
            }
            else
            {
                openFileDialog1.FileName = State.Files.FLIMageOutputFileName;
                openFileDialog1.Filter = "Choose FLIMage output file (*.txt, *.ini)|*.txt; *.ini|All files (*.*)|*.*";
            }

            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    String fileName = openFileDialog1.FileName;
                    if (fileFrom == FLIMage_Event.CommandReceivedFrom.Client)
                        State.Files.ClientOutputFileName = Path.GetFileName(fileName);
                    else
                        State.Files.FLIMageOutputFileName = Path.GetFileName(fileName);

                    State.Files.commandPathName = Path.GetDirectoryName(fileName);
                    SetCommandFilePath();
                    text_server.StartNewFileWatcher();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file to disk. Original error: " + ex.Message);
                }
            }
        }

        private void SetPath_Click(object sender, EventArgs e)
        {
            OpenCommandFileDialog(FLIMage_Event.CommandReceivedFrom.Client);
        }

        private void SetFlimageOutputFileName_Click(object sender, EventArgs e)
        {
            OpenCommandFileDialog(FLIMage_Event.CommandReceivedFrom.FLIMage);
        }

        private void LoadNotifyLIstButton_Click(object sender, EventArgs e)
        {
            //FLIMage.flim_event.ReadEventNotifyList();
            if (nf == null || nf.IsDisposed)
                nf = new NotificationTable(FLIMage.flim_event);
            nf.Show();
        }
    }


}
