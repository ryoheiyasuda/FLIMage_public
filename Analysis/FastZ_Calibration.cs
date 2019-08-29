
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;
using MathLibrary;

namespace FLIMage.Analysis
{
    public partial class FastZ_Calibration : Form
    {
        Image_Display image_display;
        FLIMData FLIM_ImgData;
        ScanParameters State;

        double deg_per_slice = 5;

        double[] zz_um;
        double[] zz_focusPhase;
        double[] zz_peak;
        double[] zz_sigma;
        double[] zz_driftX;
        double[] zz_driftY;
        double[] zz_sigma_x;
        double[] zz_sigma_y;
        double[] zz_center_x;
        double[] zz_center_y;

        double default_phase_start = 35;
        double default_phase_end = 145;
        double default_intensity = 15;

        PlotOnPanel pp, pp2, pp3, pp4, pp5, pp6;

        public FastZ_Calibration(Image_Display image_display_in)
        {
            InitializeComponent();

            image_display = image_display_in;
            FLIM_ImgData = image_display.FLIM_ImgData;
            State = FLIM_ImgData.State;
            Utilities.FormControllers.PulldownSelectByItemString(Objective_Pulldown, "x" + State.Acq.object_magnification);
            Objective_Pulldown.SelectedIndexChanged += new System.EventHandler(this.Objective_Pulldown_SelectedIndexChanged);

        }

        /// <summary>
        /// Function to caclulate focus.
        /// </summary>
        public void calculateFocus()
        {

            FLIM_ImgData.CalculateAllPages_Direct(false);

            var z_len = FLIM_ImgData.Project_Pages.Length;

            var zz_len = FLIM_ImgData.FLIM_Pages5D.Length;

            if (z_len <= 3)
                return;

            zz_um = new double[zz_len]; //Distance by um.
            zz_focusPhase = new double[zz_len]; //Peak phases.
            zz_peak = new double[zz_len]; //Intensity list.
            zz_sigma = new double[zz_len];
            zz_driftX = new double[zz_len];
            zz_driftY = new double[zz_len];
            zz_sigma_x = new double[zz_len];
            zz_sigma_y = new double[zz_len];
            zz_center_x = new double[zz_len];
            zz_center_y = new double[zz_len];

            //// zz is the motor step.
            //// z is the phase by fastZ.
            for (int zz = 0; zz < zz_len; zz++)
            {
                FLIM_ImgData.gotoPage5D(zz);

                int[] fastZRange = new int[] { 0, z_len };

                if (State.Acq.FastZ_phase_detection_mode)
                    fastZRange[1] = z_len / 2;

                int channel = image_display.currentChannel;

                var beta = ImageProcessing.GetFocusFrameByIntensity(FLIM_ImgData.Project_Pages, channel, fastZRange, true, out double[] z_data);
                ///Equation: y = beta[0] * Math.Exp(- Math.Pow(x[i] - beta[1], 2) / 2.0 /beta[2]) + beta[3];

                int x0 = (int)beta[1];

                double um_per_pixel = State.Acq.FOV_default[0] * State.Acq.scanVoltageMultiplier[0] * State.Acq.object_magnification_default / State.Acq.object_magnification / (double)State.Acq.pixelsPerLine / State.Acq.zoom;
                int pixel_to_calculate = (int)(3 * um_per_pixel); // + / - 3 um.


                if (x0 > 0 && x0 < fastZRange[1])
                {
                    int height = FLIM_ImgData.Project_Pages[x0][channel].GetLength(0);
                    int width = FLIM_ImgData.Project_Pages[x0][channel].GetLength(1);

                    //[peak, x-center, x-variance, y-center, y-variance, background]
                    
                    var betaGauss = ImageProcessing.FitImageWithGaussian2D_NearPeak(FLIM_ImgData.Project_Pages[x0][channel], pixel_to_calculate);

                    zz_center_x[zz] = betaGauss[1];
                    zz_sigma_x[zz] = Math.Sqrt(betaGauss[2]);
                    zz_center_y[zz] = betaGauss[3];
                    zz_sigma_y[zz] = Math.Sqrt(betaGauss[4]);

                    if (State.Acq.FastZ_phase_detection_mode)
                    {
                        fastZRange[0] = z_len / 2;
                        fastZRange[1] = z_len;
                        var beta1 = ImageProcessing.GetFocusFrameByIntensity(FLIM_ImgData.Project_Pages, channel, fastZRange, true, out double[] z_data1);

                        int x0R = (int)beta1[1];
                        if (x0R < 0)
                            x0R = 0;
                        if (x0R >= z_len / 2)
                            x0R = z_len / 2 - 1;
                        betaGauss = ImageProcessing.FitImageWithGaussian2D_NearPeak(FLIM_ImgData.Project_Pages[x0R][channel], pixel_to_calculate);

                        zz_driftX[zz] = betaGauss[1] - zz_center_x[zz];
                        zz_driftY[zz] = betaGauss[3] - zz_center_y[zz];

                        fastZRange[0] = 0; //it is used later.
                        fastZRange[1] = z_len / 2;
                    }
                }
                //if (State.Acq.FastZ_phase_detection_mode && beta[1] > 0 && beta[1] < zz_len / 2)
                //{
                //    int x0 = (int)beta[1];
                //    int x0R = z_len - x0 - 1;
                //    ImageProcessing.MatrixMeasureDrift2D_FFT(FLIM_ImgData.Project_Pages[x0][channel], FLIM_ImgData.Project_Pages[x0R][channel], out double[] drift);
                //    zz_driftX[zz] = drift[0];
                //    zz_driftY[zz] = drift[1];
                //}

                var phase_data = new double[fastZRange[1]];
                for (int i = 0; i < phase_data.Length; i++)
                    phase_data[i] = i;

                var fit_curve = MatrixCalc.Gaussian(beta, phase_data);

                GaussianFit_Display(phase_data, z_data, fit_curve);
                Application.DoEvents();
                //System.Threading.Thread.Sleep(100);

                double range = State.Acq.FastZ_phase_detection_mode ? 360 : State.Acq.FastZ_PhaseRange[1] - State.Acq.FastZ_PhaseRange[0];

                deg_per_slice = range / (double)FLIM_ImgData.n_pages;
                double startphase = State.Acq.FastZ_phase_detection_mode ? 0 : State.Acq.FastZ_PhaseRange[0];

                zz_um[zz] = State.Acq.sliceStep * (zz - zz_len / 2);
                zz_focusPhase[zz] = startphase + ((double)beta[1] + 0.5) * deg_per_slice;
                zz_peak[zz] = (double)beta[0];
                zz_sigma[zz] = (double)Math.Sqrt(beta[2]) * deg_per_slice;

            } // for zz.

            PlotPanel_Display();
        }

        public void GaussianFit_Display(double[] phase_data, double[] z_um_data, double[] fit_data)
        {
            pp.clearData();
            pp.addData(phase_data, z_um_data, "o");
            pp.addData(phase_data, fit_data, new Pen(Brushes.Red), "-"); //fitting in red.
            pp.XTitle = "Phase (deg)";
            pp.YTitle = "Intensity";
            PlotPanel.Invalidate();
        }

        public void PlotPanel_Display()
        {
            if (zz_um == null)
                return;

            int x0, x1;
            if (!AllSlices.Checked)
            {
                if (!Int32.TryParse(ZStack_Start.Text, out x0)) x0 = 1;
                if (!Int32.TryParse(ZStack_End.Text, out x1)) x1 = zz_um.Length;
            }
            else
            {
                x0 = 1;
                x1 = zz_um.Length;
            }

            if (!Double.TryParse(FastZStack_Start.Text, out double y0)) y0 = default_phase_start;
            if (!Double.TryParse(FastZStack_End.Text, out double y1)) y1 = default_phase_end;
            if (!Double.TryParse(IntensityThreshold.Text, out double i0)) i0 = default_intensity;
            var str1 = Objective_Pulldown.SelectedItem.ToString().Replace("x", "");
            var objective_mag = Convert.ToDouble(str1);

            var um_per_pixel = new double[2];
            for (int i = 0; i < 2; i++)
                um_per_pixel[i] = State.Acq.FOV_default[i] * State.Acq.scanVoltageMultiplier[i] * State.Acq.object_magnification_default / objective_mag / (double)State.Acq.pixelsPerLine / State.Acq.zoom;

            ZStack_Start.Text = x0.ToString();
            ZStack_End.Text = x1.ToString();
            FastZStack_Start.Text = y0.ToString();
            FastZStack_End.Text = y1.ToString();
            IntensityThreshold.Text = i0.ToString();

            List<double> z_um = new List<double>();
            List<double> z_phase = new List<double>();
            List<double> z_peak = new List<double>();
            List<double> z_width = new List<double>();
            List<double> shift_x = new List<double>();
            List<double> shift_y = new List<double>();
            List<double> center_x = new List<double>();
            List<double> center_y = new List<double>();
            List<double> sigma_x = new List<double>();
            List<double> sigma_y = new List<double>();

            var peakVal = zz_peak.Max();

            for (int i = 0; i < zz_um.Length; i++)
            {
                if (i >= x0 && i <= x1 && zz_focusPhase[i] >= y0 && zz_focusPhase[i] <= y1 && zz_peak[i] / peakVal * 100 >= i0)
                {
                    z_um.Add(zz_um[i]);
                    z_phase.Add(zz_focusPhase[i]);
                    z_peak.Add(zz_peak[i]);
                    z_width.Add(zz_sigma[i]);

                    shift_x.Add(zz_driftX[i] * um_per_pixel[0]);
                    shift_y.Add(zz_driftY[i] * um_per_pixel[1]);

                    center_x.Add(zz_center_x[i] * um_per_pixel[0]);
                    center_y.Add(zz_center_y[i] * um_per_pixel[1]);
                    sigma_x.Add(zz_sigma_x[i] * um_per_pixel[0]);
                    sigma_y.Add(zz_sigma_y[i] * um_per_pixel[1]);
                }
            }

            double[] phase_data = z_phase.ToArray();
            double[] z_um_data = z_um.ToArray();
            double[] peak_int_data = z_peak.ToArray();
            double[] width_data = z_width.ToArray();
            double[] shiftX_data = shift_x.ToArray();
            double[] shiftY_data = shift_y.ToArray();
            double[] center_x_data = center_x.ToArray();
            double[] center_y_data = center_y.ToArray();
            double[] sigma_x_data = sigma_x.ToArray();
            double[] sigma_y_data = sigma_y.ToArray();

            var beta = phase_data.Length > 1 ? MatrixCalc.linearRegression(phase_data, z_um_data) : new double[] { 0, 0 };
            double[] fit_data = phase_data.Select(x => beta[0] + beta[1] * x).ToArray();

            for (int i = 0; i < width_data.Length; i++)
                width_data[i] = Math.Abs(width_data[i] * beta[1]); //beta1 = um/degree.

            pp.clearData();
            pp.addData(phase_data, z_um_data, "o");
            pp.addData(phase_data, fit_data, new Pen(Brushes.Red), "-"); //fitting in red.
            pp.noTitleX = false;
            pp.XTitle = "Phase (deg)";
            pp.YTitle = "Distance (um)";

            pp2.clearData();
            pp2.addData(phase_data, peak_int_data, "-o");
            pp2.noTitleX = true;
            pp2.XTitle = "Phase (deg)";
            pp2.YTitle = "Intensity";

            pp3.clearData();
            pp3.addData(phase_data, width_data, "-o");
            pp3.noTitleX = true;
            pp3.XTitle = "Phase (deg)";
            pp3.YTitle = "Resolution (μm)";

            pp4.clearData();
            pp4.addData(phase_data, center_x_data, "-o");
            pp4.Apply_Legend("X");
            pp4.addData(phase_data, center_y_data, "-o");
            pp4.Apply_Legend("Y");
            pp4.noTitleX = false;
            pp4.XTitle = "Phase (deg)";
            pp4.YTitle = "Center position XY (μm)";

            pp5.clearData();
            pp5.addData(phase_data, sigma_x_data, "-o");
            pp5.addData(phase_data, sigma_y_data, "-o");
            pp5.noTitleX = true;
            pp5.XTitle = "Phase (deg)";
            pp5.YTitle = "Resolution XY (μm)";

            pp6.clearData();
            pp6.addData(phase_data, shiftX_data, "-o");
            pp6.addData(phase_data, shiftY_data, "-o");
            pp6.noTitleX = true;
            pp6.XTitle = "Phase (deg)";
            pp6.YTitle = "Shift by phase (μm)";

            PlotPanel.Invalidate();
            PlotPanel2.Invalidate();
            PlotPanel3.Invalidate();
            PlotPanel4.Invalidate();
            PlotPanel5.Invalidate();
            PlotPanel6.Invalidate();

            DisplayFitData(beta);
            var all_data = new double[10][];
            all_data[0] = phase_data;
            all_data[1] = z_um_data;
            all_data[2] = peak_int_data;
            all_data[3] = width_data;
            all_data[4] = center_x_data;
            all_data[5] = sigma_x_data;
            all_data[6] = center_y_data;
            all_data[7] = sigma_y_data;
            all_data[8] = shiftX_data;
            all_data[9] = shiftY_data;

            var titles = new List<String>();
            titles.Add("Phase");
            titles.Add("Z (um)");
            titles.Add("Intensity");
            titles.Add("Z res (um)");
            titles.Add("CenterX (um)");
            titles.Add("X res (um)");
            titles.Add("CenterY (um)");
            titles.Add("Y res (um)");
            titles.Add("Shift X (um)");
            titles.Add("Shift Y (um)");

            SaveData(all_data, titles, beta);
        }

        public void DisplayFitData(double[] beta)
        {

            FitData.Text = String.Format("{0:0.0} um / 90 deg", beta[1] * 90.0);
            FitData2.Text = String.Format("{0:0.0} um / fastZ", beta[1] * deg_per_slice);
        }

        public void SaveData(double[][] all_data, List<string> titles, double[] beta)
        {
            var directoryPath = Path.Combine(FLIM_ImgData.pathName, "Analysis");
            var fileName = String.Format("{0}{1:000}_PhaseData.csv", FLIM_ImgData.baseName, FLIM_ImgData.fileCounter);
            var filePath = Path.Combine(directoryPath, fileName);

            var sb = new StringBuilder();
            for (int j = 0; j < titles.Count; j++)
            {
                sb.Append(titles[j]);
                sb.Append(", ");
            }
            sb.AppendLine();
            for (int i = 0; i < all_data[0].Length; i++)
            {
                for (int j = 0; j < all_data.Length; j++)
                {
                    sb.Append(all_data[j][i]);
                    sb.Append(", ");
                }
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.Append("Fitting parameters: y = a * x + b");
            sb.AppendLine();
            sb.Append("a, b");
            sb.AppendLine();
            sb.Append(beta[1]);
            sb.Append(", ");
            sb.Append(beta[0]);
            sb.AppendLine();

            try
            {
                File.WriteAllText(filePath, sb.ToString());
            }
            catch
            {
                Debug.WriteLine("The file is used!");
                for (int i = 1; i < 10; i++)
                {
                    fileName = String.Format("{0}{1:000}_PhaseData({2}).csv", FLIM_ImgData.baseName, FLIM_ImgData.fileCounter, i);
                    filePath = Path.Combine(directoryPath, fileName);
                    try
                    {
                        File.WriteAllText(filePath, sb.ToString());
                        break;
                    }
                    catch
                    {

                    }
                }
            }
        }

        public void PlotPanel_Paint(object sender, PaintEventArgs e)
        {
            if (pp != null)
            {
                pp.autoAxisPosition(e);
                pp.plot(e);
            }
        }

        public void PlotPanel2_Paint(object sender, PaintEventArgs e)
        {
            if (pp2 != null)
            {
                pp2.autoAxisPosition(e);
                pp2.plot(e);
            }
        }

        public void PlotPanel3_Paint(object sender, PaintEventArgs e)
        {
            if (pp3 != null)
            {
                pp3.autoAxisPosition(e);
                pp3.plot(e);
            }
        }

        public void PlotPanel4_Paint(object sender, PaintEventArgs e)
        {
            if (pp4 != null)
            {
                pp4.autoAxisPosition(e);
                pp4.plot(e);
            }
        }

        public void PlotPanel5_Paint(object sender, PaintEventArgs e)
        {
            if (pp5 != null)
            {
                pp5.autoAxisPosition(e);
                pp5.plot(e);
            }
        }


        public void PlotPanel6_Paint(object sender, PaintEventArgs e)
        {
            if (pp6 != null)
            {
                pp6.autoAxisPosition(e);
                pp6.plot(e);
            }
        }

        public void FastZ_Calibration_Shown(object sender, EventArgs e)
        {
            //int y1 = State.Acq.FastZ_phase_detection_mode ? FLIM_ImgData.n_pages / 2 : FLIM_ImgData.n_pages;

            ZStack_Start.Text = 1.ToString();
            ZStack_End.Text = FLIM_ImgData.n_pages5D.ToString();
            FastZStack_Start.Text = default_phase_start.ToString();
            FastZStack_End.Text = default_phase_end.ToString();
            IntensityThreshold.Text = default_intensity.ToString();

            pp = new PlotOnPanel(PlotPanel.Width, PlotPanel.Height);
            pp2 = new PlotOnPanel(PlotPanel2.Width, PlotPanel2.Height);
            pp3 = new PlotOnPanel(PlotPanel3.Width, PlotPanel3.Height);
            pp4 = new PlotOnPanel(PlotPanel4.Width, PlotPanel4.Height);
            pp5 = new PlotOnPanel(PlotPanel5.Width, PlotPanel5.Height);
            pp6 = new PlotOnPanel(PlotPanel6.Width, PlotPanel6.Height);

            calculateFocus();
        }

        public void Objective_Pulldown_SelectedIndexChanged(object sender, EventArgs e)
        {
            PlotPanel_Display();
        }

        public void AllSlices_CheckedChanged(object sender, EventArgs e)
        {
            PlotPanel_Display();
        }

        public void CalcButton_Click(object sender, EventArgs e)
        {
            CalcButton.Enabled = false;
            FLIM_ImgData = image_display.FLIM_ImgData;
            State = FLIM_ImgData.State;
            calculateFocus();
            CalcButton.Enabled = true;
        }

        public void ZStack_Start_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                PlotPanel_Display();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }

        }

    }
}
