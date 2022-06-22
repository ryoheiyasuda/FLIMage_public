using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhysiologyCSharp
{
    public partial class MC700BCommander : Form
    {
        public MC700CommanderCore commnder;
        public MC700BCommander(MC700CommanderCore com)
        {
            InitializeComponent();
            commnder = com;
        }

        private void MC700BCommander_Load(object sender, EventArgs e)
        {
            //commnder = new MC700CommanderCore();
        }

        private void ReadButton_Click(object sender, EventArgs e)
        {
            ReadGain();
        }

        private void ReadGain()
        {
            commnder.GetMC700BGain();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < commnder.MC700_Params.Length; i++)
            {
                sb.Append("-----------------------");
                sb.AppendLine();
                var pm = commnder.MC700_Params[i];
                sb.Append("Channel = " + i);
                sb.AppendLine();
                sb.Append("ID = " + pm.ID);
                sb.AppendLine();
                sb.Append("Mode = " + pm.mode);
                sb.AppendLine();
                sb.Append("Primary Gain = " + pm.primary_gain);
                sb.AppendLine();
                sb.Append("Scale Factor = " + pm.scaleFactor);
                sb.AppendLine();
                sb.Append("Ext Command Sensitivity = " + pm.external_cmd_sensitivity);
                sb.AppendLine();
            }

            valueTextBox.Text = sb.ToString();

        }
    }
}
