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
    public partial class UncagingCalibration : Form
    {
        ScanParameters State;
        FLIMageMain flimage;

        public UncagingCalibration(FLIMageMain flim_controls)
        {
            InitializeComponent();
            State = flim_controls.State;
            flimage = flim_controls;

            UncagingCalibX.Text = State.Uncaging.CalibV[0].ToString();
            UncagingCalibY.Text = State.Uncaging.CalibV[1].ToString();

            flimage.image_display.ActivateUncaging(true);
            flimage.image_display.Activate();
            flimage.image_display.calib_on = true;
            this.Activate();
            this.TopMost = true;
        }

        public void updateWindow()
        {
            UncagingCalibX.Text = String.Format("{0:0.000}", State.Uncaging.CalibV[0] - flimage.flimage_io.uncaging_Calib[0]);
            UncagingCalibY.Text = String.Format("{0:0.000}", State.Uncaging.CalibV[1] - flimage.flimage_io.uncaging_Calib[1]);
        }

        public void ApplyCalib_Click(object sender, EventArgs e)
        {
            double valD;
            if (Double.TryParse(UncagingCalibX.Text, out valD)) State.Uncaging.CalibV[0] = valD;
            if (Double.TryParse(UncagingCalibY.Text, out valD)) State.Uncaging.CalibV[1] = valD;
            this.Close();
        }

        public void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void UncagingCalibration_FormClosing(object sender, FormClosingEventArgs e)
        {
            flimage.flimage_io.uncaging_Calib = new double[] { 0.0, 0.0 };
            flimage.image_display.referenceLoc = new Point(-1, -1);
            flimage.image_display.calib_on = false;
            flimage.image_display.Refresh();
        }
    }
}
