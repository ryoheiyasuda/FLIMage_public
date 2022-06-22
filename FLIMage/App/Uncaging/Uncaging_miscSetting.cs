using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FLIMage.Uncaging
{
    public partial class Uncaging_miscSetting : Form
    {
        FLIMageMain flimage;
        ScanParameters State;

        public Uncaging_miscSetting(FLIMageMain fc)
        {
            InitializeComponent();

            flimage = fc;
            State = fc.State;
            fillGUI();
        }

        public void Checkbox_Clicked(object sender, EventArgs e)
        {
            State.Uncaging.TurnOffImagingDuringUncaging = TurnOffImagingDuringUncagingCheck.Checked;
            State.Uncaging.MoveMirrorsToUncagingPosition = MoveMirrosDuringUncagingPosCheck.Checked;
        }

        public void fillGUI()
        {
            MoveMirrosDuringUncagingPosCheck.Checked = State.Uncaging.MoveMirrorsToUncagingPosition;
            TurnOffImagingDuringUncagingCheck.Checked = State.Uncaging.TurnOffImagingDuringUncaging;
            MirrorDelay.Text = State.Uncaging.Mirror_delay.ToString();
        }

        public void MirrorDelay_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tb = (TextBox)sender;
                String SaveText = tb.Text;

                try
                {
                    Double.TryParse(tb.Text, out State.Uncaging.Mirror_delay);
                    fillGUI();
                }
                catch (System.FormatException)
                {
                    tb.Text = SaveText;
                }
                finally
                { };
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }
}
