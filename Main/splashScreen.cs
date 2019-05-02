using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using TCSPC_controls;
using System.Deployment.Application;

namespace FLIMage
{
    public partial class splashScreen : Form
    {
        public int computerID = 0;
        public String versionText = "";

        public splashScreen()
        {
            InitializeComponent();

            var parameters = new FLIM_Parameters();
            var FiFo_acquire = new FiFio_multiBoards(parameters);
            parameters.BoardType = ""; //Try not opening anything.

            computerID = FiFo_acquire.computer_id();

            MacID.Text = "Computer id: " + computerID;
            MacID.Visible = true;
            this.Refresh();

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                //Debug.WriteLine(string.Format("Your application name - v{0}", ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(4)));
                versionText = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(4);
            }
            else
            {
                versionText = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                //Debug.WriteLine("From reflection = " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4));
            }
            this.Text = "FLIMage! Version " + versionText;
        }

        private void splashScreen_Load(object sender, EventArgs e)
        {

        }


    }
}
