
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
        ScanParameters State;
        int nChannels = 8;

        IOControls.DigitalOut[] DigitalOutputs;

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
                DigitalOutputs[i].PutValue(cb.Checked);
            }
        }

        public void DigitalSignalPanel_Load(object sender, EventArgs e)
        {
            DigitalOutputs = new IOControls.DigitalOut[nChannels];

            for (int i = 0; i < nChannels; i++)
            {
                Control[] found = Controls.Find("checkBox" + i, true);

                if (found.Length > 0)
                {
                    CheckBox cb = (CheckBox)found[0];

                    if (State.Init.triggerPort.EndsWith(i.ToString()))
                    {
                        cb.Enabled = false;
                        cb.Text = "Trig out";
                    }

                    else if (State.Init.shutterPort.EndsWith(i.ToString()))
                    {
                        //cb.Enabled = false;
                        cb.Text = "Shutter";
                    }

                    else
                    {
                        cb.Text = "Port " + i.ToString();
                    }
                }

                String portname = State.Init.triggerPort.Substring(0, State.Init.triggerPort.Length - 1) + i;

                DigitalOutputs[i] = new IOControls.DigitalOut(portname);

            }
        }
    }
}
