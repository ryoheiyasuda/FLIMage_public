using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Stage_Control;

namespace FLIMimage
{
    public partial class MotorCtrlPanel : Form
    {
        FLIMageMain flim;
        MotorCtrl motorCtrl;
        int N_Coordinate;
        List<double[]> motor_positions = new List<double[]>();
        String Hdr = "Position-";

        public MotorCtrlPanel(FLIMageMain fc, MotorCtrl mc)
        {
            InitializeComponent();
            motorCtrl = mc;
            flim = fc;
            N_Coordinate = motorCtrl.N_Coordinate;

        }

        private void MotorCtrlPanel_Load(object sender, EventArgs e)
        {
            motor_flowLayoutPanel.WrapContents = true;
            motor_flowLayoutPanel.AutoScroll = true;
        }

        private void AddPosition_Click(object sender, EventArgs e)
        {
            Button newButton = new Button();
            newButton.Name = Hdr + (motor_flowLayoutPanel.Controls.Count + 1);
            newButton.Text = "Go:" + Hdr + (motor_flowLayoutPanel.Controls.Count + 1);
            newButton.Width = 120;
            newButton.Click += new System.EventHandler(newButton_Click);

            motor_flowLayoutPanel.Controls.Add(newButton);

            motorCtrl.GetPosition(true);
            SelectPosition.Items.Add(newButton.Name);
            double[] motor_pos = new double[N_Coordinate];
            motor_pos[0] = motorCtrl.XPos;
            motor_pos[1] = motorCtrl.YPos;
            motor_pos[2] = motorCtrl.ZPos;
            motor_positions.Add(motor_pos);
            SelectPosition.SelectedIndex = motor_flowLayoutPanel.Controls.Count - 1;
        }


        private void newButton_Click(object sender, EventArgs e)
        {
            Button button1 = (Button)sender;
            String name1 = button1.Name;
            int index1 = Convert.ToInt32(name1.Substring(Hdr.Length));
            SelectPosition.SelectedItem = name1;
            double[] motor_pos = motor_positions[index1 - 1];
            motorCtrl.XNewPos = motor_pos[0];
            motorCtrl.YNewPos = motor_pos[1];
            motorCtrl.ZNewPos = motor_pos[2];
            flim.ExternalCommand("SetMotorPosition");
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (SelectPosition.Items.Count == 0)
                return;

            int index1 = SelectPosition.SelectedIndex;
            if (index1 < motor_flowLayoutPanel.Controls.Count && index1 >= 0)
            {
                motor_flowLayoutPanel.Controls.RemoveAt(index1);
                SelectPosition.Items.RemoveAt(index1);
                if (index1 > 1)
                    SelectPosition.SelectedIndex = index1 - 1;

                //motor_positions.RemoveAt(index1);
            }
        }

        private void DisplayPosition()
        {
            int currentIndex = Convert.ToInt32(SelectPosition.SelectedItem.ToString().Substring(Hdr.Length)) - 1;
            if (currentIndex < motor_positions.Count)
            {
                if (!Relative.Checked)
                {
                    MotorX.Text = String.Format("{0:0.00}", motor_positions[currentIndex][0] * motorCtrl.resolutionX);
                    MotorY.Text = String.Format("{0:0.00}", motor_positions[currentIndex][1] * motorCtrl.resolutionY);
                    MotorZ.Text = String.Format("{0:0.00}", motor_positions[currentIndex][2] * motorCtrl.resolutionZ);
                }
                else
                {
                    MotorX.Text = String.Format("{0:0.00}", (motor_positions[currentIndex][0] - motorCtrl.XRefPos) * motorCtrl.resolutionX);
                    MotorY.Text = String.Format("{0:0.00}", (motor_positions[currentIndex][1] - motorCtrl.YRefPos) * motorCtrl.resolutionY);
                    MotorZ.Text = String.Format("{0:0.00}", (motor_positions[currentIndex][2] - motorCtrl.ZRefPos) * motorCtrl.resolutionZ);
                }
            }
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            motor_positions.Clear();
            SelectPosition.Items.Clear();
            SelectPosition.Text = "";
            motor_flowLayoutPanel.Controls.Clear();
        }

        private void SelectPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayPosition();
        }

        private void MotorCtrlPanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
