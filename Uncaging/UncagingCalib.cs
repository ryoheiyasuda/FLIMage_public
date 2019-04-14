using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FLIMimage
{
    public partial class UncagingCalibration : Form
    {
        ScanParameters State;
        FLIMageMain FLIMage;

        public UncagingCalibration(FLIMageMain flim_controls)
        {
            InitializeComponent();
            State = flim_controls.State;
            FLIMage = flim_controls;

            UncagingCalibX.Text = State.Uncaging.CalibV[0].ToString();
            UncagingCalibY.Text = State.Uncaging.CalibV[1].ToString();

            FLIMage.image_display.ActivateUncaging();
            FLIMage.image_display.Activate();
            FLIMage.image_display.calib_on = true;
            this.Activate();
            this.TopMost = true;
        }

        public void updateWindow()
        {
            UncagingCalibX.Text = String.Format("{0:0.000}", State.Uncaging.CalibV[0] - FLIMage.flimage_io.uncaging_Calib[0]);
            UncagingCalibY.Text = String.Format("{0:0.000}", State.Uncaging.CalibV[1] - FLIMage.flimage_io.uncaging_Calib[1]);
        }

        private void ApplyCalib_Click(object sender, EventArgs e)
        {
            double valD;
            if (Double.TryParse(UncagingCalibX.Text, out valD)) State.Uncaging.CalibV[0] = valD;
            if (Double.TryParse(UncagingCalibY.Text, out valD)) State.Uncaging.CalibV[1] = valD;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UncagingCalibration_FormClosing(object sender, FormClosingEventArgs e)
        {
            FLIMage.flimage_io.uncaging_Calib = new double[] { 0.0, 0.0 };
            FLIMage.image_display.referenceLoc = new Point(-1, -1);
            FLIMage.image_display.calib_on = false;
            FLIMage.image_display.Refresh();
        }
    }
}
