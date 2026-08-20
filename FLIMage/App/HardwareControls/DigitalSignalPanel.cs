
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace FLIMage.HardwareControls
{
    public partial class DigitalSignalPanel : Form
    {
        const int nChannels = 8;

        FLIMageMain flimage;
        ScanParameters State;
        String[] portNames = new string[nChannels];

        WindowLocManager winManager;
        String WindowName = "DIOPanel.loc";

        public DigitalSignalPanel(FLIMageMain fc)
        {
            flimage = fc;
            State = flimage.State;
            InitializeComponent();
        }

        public void checkBox1_Click(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            new IOControls.Digital_Out(portNames[Convert.ToInt32(cb.Text.Split(':')[0])], cb.Checked);
        }

        //Kengo BEGIN 12-12-2023
        //This is for a remote command to control DIO_panel
        public void SetDIOPanel(int channel, bool ON)
        {
            Control[] found = Controls.Find("checkBox" + channel, true);

            if (found.Length > 0)
            {
                CheckBox cb = (CheckBox)found[0];
                cb.Checked = ON;
                new IOControls.Digital_Out(portNames[Convert.ToInt32(cb.Text.Split(':')[0])], ON);
            }
        }
        //Kengo END

        public void DigitalSignalPanel_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < nChannels; i++)
            {
                Control[] found = Controls.Find("checkBox" + i, true);

                if (found.Length > 0)
                {
                    CheckBox cb = (CheckBox)found[0];

                    if (State.Init.triggerPort.EndsWith(i.ToString()))
                    {
                        //cb.Enabled = false;
                        cb.Text = "Trigger out";
                    }

                    else if (State.Init.shutterPort.EndsWith(i.ToString()))
                    {
                        cb.Text = "Shutter";
                    }
                    else if (State.Init.use_digitalLineClock && State.Init.DigitalLinePort.EndsWith(i.ToString()))
                    {
                        cb.Text = "Line Clock";
                    }
                    else if (State.Init.DO_uncagingShutter && State.Init.DigitalShutterPort.EndsWith(i.ToString()))
                    {
                        cb.Text = "Uncaging Shutter";
                    }
                    else if (State.Init.DigitalOutput1.EndsWith(i.ToString()))
                    {
                        cb.Text = "DO1";
                    }
                    else if (State.Init.DigitalOutput2.EndsWith(i.ToString()))
                    {
                        cb.Text = "DO2";
                    }
                    else if (State.Init.DigitalOutput3.EndsWith(i.ToString()))
                    {
                        cb.Text = "DO3";
                    }
                    else
                    {
                        cb.Text = "Reserved ";
                    }

                    cb.Text = i + ": " + cb.Text;

                    portNames[i] = State.Init.triggerPort.Substring(0, State.Init.triggerPort.Length - 1) + i;
                    //new IOControls.Digital_Out(portNames[i], cb.Checked);
                }
            }

            winManager = new WindowLocManager(this, WindowName, State.Files.windowsInfoPath);
            winManager.LoadWindowLocation(false);
        }

        public void SaveWindowLocation()
        {
            winManager.SaveWindowLocation();
        }

        public void DigitalSignalPanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            winManager.SaveWindowLocation();
            e.Cancel = true;
            this.Hide();

            flimage.ToolWindowClosed();
        }
    }
}
