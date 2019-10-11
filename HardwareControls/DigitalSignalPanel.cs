
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FLIMage.HardwareControls
{
    public partial class DigitalSignalPanel : Form
    {
        const int nChannels = 8;

        ScanParameters State;
        String[] portNames = new string[nChannels];

        public DigitalSignalPanel(ScanParameters Scan)
        {
            State = Scan;
            InitializeComponent();
        }

        public void checkBox1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < nChannels; i++)
            {
                Control[] found = Controls.Find("checkBox" + i, true);
                CheckBox cb = (CheckBox)found[0];
                new IOControls.Digital_Out(portNames[i], cb.Checked);
            }
        }

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
                    new IOControls.Digital_Out(portNames[i], cb.Checked);
                }
            }
        }
    }
}
