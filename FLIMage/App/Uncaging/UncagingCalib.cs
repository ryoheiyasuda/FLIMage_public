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
            flimage.image_display.uncagingLocs_calib = new List<double[]>();

            if (flimage.image_display.uncagingLocs.Count > 0)
            {
                for (int i = 0; i < flimage.image_display.uncagingLocs.Count; i++)
                {
                    flimage.image_display.uncagingLocs_calib.Add((double[])flimage.image_display.uncagingLocs[i].Clone());
                    for (int j = 0; j < 2; j++)
                        flimage.image_display.uncagingLocs_calib[i][j] += 0.01;
                }

                flimage.image_display.uncaging_calib_multi_mode = true;

                Note_Label.Text = "Move the orange marks to \nthe actual uncaging spots";
                label1.Text = "(Two position calibration)";
            }
            else
            {
                flimage.image_display.uncaging_calib_mode = true;
            }

            flimage.image_display.Activate();
            flimage.image_display.DrawImages_public();
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
            if (flimage.image_display.uncaging_calib_mode)
            {
                double valD;
                if (Double.TryParse(UncagingCalibX.Text, out valD)) State.Uncaging.CalibV[0] = valD;
                if (Double.TryParse(UncagingCalibY.Text, out valD)) State.Uncaging.CalibV[1] = valD;
            }
            else if (flimage.image_display.uncaging_calib_multi_mode)
            {
                var uncage_positions = flimage.image_display.uncagingLocs;
                var actual_positions = flimage.image_display.uncagingLocs_calib;

                //State.Uncaging.Calib_beta = new double[] { 0, 0 }; //multiplying s
                var offset = new double[] { 0, 0 };
                var slope = new double[] { 1, 1 };

                double base_position = 0.5;

                for (int j = 0; j < 2; j++)
                {
                    var uncage_positions_x = uncage_positions.Select(x => x[j] - base_position).ToList();

                    var actual_positions_x = actual_positions.Select(x => x[j] - base_position).ToList();

                    var max_index = uncage_positions_x.IndexOf(uncage_positions_x.Max());
                    var min_index = uncage_positions_x.IndexOf(uncage_positions_x.Min());
                    slope[j] = (actual_positions_x[max_index] - actual_positions_x[min_index]) / (uncage_positions_x[max_index] - uncage_positions_x[min_index]);
                    offset[j] = actual_positions_x[min_index] - slope[j] * uncage_positions_x[min_index] + base_position;
                }

                var calibV_before = HardwareControls.IOControls.PositionFracToVoltage(new double[] { base_position, base_position }, flimage.State, false);
                var calibV_after = HardwareControls.IOControls.PositionFracToVoltage(offset, flimage.State, false);

                for (int j = 0; j < 2; j++)
                {
                    var calibV1 = (calibV_after[j] - calibV_before[j]);
                    if (j == 0)
                        calibV1 *= State.Acq.zoom; 
                    State.Uncaging.CalibV[j] = calibV1 + State.Uncaging.CalibV[j] * slope[j];
                    State.Uncaging.Calib_beta[j] *= slope[j];
                }
            }

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
            flimage.image_display.uncaging_calib_mode = false;
            flimage.image_display.uncaging_calib_multi_mode = false;
            flimage.image_display.Refresh();
        }
    }
}
