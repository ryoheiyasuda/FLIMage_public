using MathLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace FLIMage.FlowControls
{
    public partial class Drift_correction : Form
    {
        FLIMageMain flimage;
        ushort[,] template_image;
        ushort[,] current_image;
        ushort[][][,,] FLIM_5D;
        ushort[][,] template_imageZYX;
        ushort[][,] imageZYX; //[z][x,y]
        int[] dimYXT = new int[] { 128, 128, 64 };
        int nSlices = 0;
        Bitmap templateBMP, templateBMP_XZ;
        bool template_image_set = false;
        int correctionChannel = 0;

        double[] motor_pos_template;
        double[] motor_pos_current;

        WindowLocManager winManager;
        String WindowName = "DriftCorrection.loc";
        SettingManager settingManager;
        String settingName = "DriftCorrection";

        public Drift_correction(FLIMageMain FLIMage_in)
        {
            InitializeComponent();

            flimage = FLIMage_in;
            Channel_Pulldown.SelectedIndex = 0; // flimage.image_display.currentChannel;

            flimage.flimage_io.EventNotify += new FLIMage_IO.FLIMage_EventHandler(EventHandling);

            template_image = new ushort[128, 128];
            //template_z_profile = new double[16];
            ApplyBMP();
        }

        void InitializeSetting()
        {
            settingManager = new SettingManager(settingName, flimage.State.Files.initFolderPath);
            settingManager.AddToDict(XYCorrect_CB);
            settingManager.AddToDict(ZCorrect_CB);
            settingManager.AddToDict(MoveOposite_Z);
            settingManager.AddToDict(UseMirror_CB);
            settingManager.LoadToObject();
        }

        public void SaveSetting()
        {
            if (settingManager != null)
            {
                settingManager.SaveFromObject();
            }
        }

        public void SelectCurrentImage_Click(object sender, EventArgs e)
        {
            SelectImage();
        }

        public void ApplyBMP()
        {
            //            templateBMP = ImageProcessing.FormatImage(flimage.image_display.State_intensity_range[correctionChannel], template_image);
            templateBMP = ImageProcessing.FormatImage(template_image);
            TemplateImage_PB.Invalidate();
        }

        public void TurnOnOffCorrection(bool ON)
        {
            DriftCorrection_CB.InvokeIfRequired(o => o.Checked = ON);
        }

        public void SelectImage()
        {
            var fname = Path.GetFileName(flimage.image_display.FLIM_ImgData.fullFileName);
            TemplateFileName.Text = "Filename: " + fname;
            correctionChannel = Channel_Pulldown.SelectedIndex;
            reconstructImageFromPages();

            template_image = getProjection(imageZYX);
            //template_z_profile = getProjection_z();
            template_imageZYX = new ushort[imageZYX.Length][,];
            for (int z = 0; z < imageZYX.Length; z++)
                template_imageZYX[z] = (ushort[,])imageZYX[z].Clone();
            template_image_set = true;

            motor_pos_template = (double[])flimage.image_display.FLIM_ImgData.State.Motor.motorPosition.Clone();
            this.BeginInvokeIfRequired(o => o.ApplyBMP());
        }

        public void reconstructImageFromPages()
        {
            FLIM_5D = (ushort[][][,,])Copier.DeepCopyArray(flimage.image_display.FLIM_ImgData.FLIM_Pages);
            dimYXT = flimage.image_display.FLIM_ImgData.dimensionYXT(correctionChannel);
            nSlices = FLIM_5D.Length;


            imageZYX = new ushort[nSlices][,];
            for (int z = 0; z < nSlices; z++)
                imageZYX[z] = ImageProcessing.GetProjectFromFLIM(FLIM_5D[z][correctionChannel], null);

        }

        public double[] calculateDriftXYZ()
        {
            double[] xyz_drift = new double[3];

            if (template_image_set)
            {
                reconstructImageFromPages();

                current_image = getProjection(imageZYX);
                //current_z_profile = getProjection_z();

                var xy_drift = calculate_drift();

                if (xy_drift != null)
                {
                    xyz_drift[0] = xy_drift[0];
                    xyz_drift[1] = xy_drift[1];
                }
                else
                {
                    xyz_drift[0] = double.NaN;
                    xyz_drift[1] = double.NaN;
                }

                xyz_drift[2] = CalculateZdrift(xyz_drift);
            }

            return xyz_drift;
        }

        public double CalculateZdrift(double[] xyz_drift)
        {
            double z_drift = 0;
            int z_len = imageZYX.Length;
            if (nSlices >= 3 && imageZYX.Length == template_imageZYX.Length)
            {
                var shiftImage = new ushort[z_len][,];
                for (int z = 0; z < z_len; z++)
                {
                    shiftImage[z] = ImageProcessing.ShiftImage(imageZYX[z], -(int)xyz_drift[0], -(int)xyz_drift[1]);
                }

#if DEBUG
                var shiftImageProject = getProjection(shiftImage);
                ImageProcessing.ImShow(shiftImageProject, template_image);
#endif

                //calculate mean.
                float mean_shift = 0;
                float mean_temp = 0;
                for (int z = 0; z < z_len; z++)
                {
                    mean_shift += (float)MatrixCalc.Mean2D(shiftImage[z]);
                    mean_temp += (float)MatrixCalc.Mean2D(template_imageZYX[z]);
                }
                mean_shift = mean_shift / (float)z_len;
                mean_temp = mean_temp / (float)z_len;

                //Subtract mean
                var shiftImage_n = new float[z_len][,];
                var templateImage_n = new float[z_len][,];

                for (int z = 0; z < z_len; z++)
                {
                    var image_temp = MatrixCalc.ConvertToFloatMatrix(shiftImage[z]);
                    shiftImage_n[z] = MatrixCalc.SubtractConstantFromMatrix(image_temp, mean_shift);
                    image_temp = MatrixCalc.ConvertToFloatMatrix(template_imageZYX[z]);
                    templateImage_n[z] = MatrixCalc.SubtractConstantFromMatrix(image_temp, mean_temp);
                }

                //

                int max_shift = (z_len - 1) / 2;
                double[] xCorr = new double[max_shift * 2 + 1];
                for (int z_shift = -max_shift; z_shift <= max_shift; z_shift++)
                {
                    double xcorr = 0;

                    for (int z = 0; z < z_len; z++)
                        if (z - z_shift >= 0 && z - z_shift < z_len)
                            xcorr += MatrixCalc.SIMD_Dot(shiftImage_n[z - z_shift], templateImage_n[z]);

                    xCorr[z_shift + max_shift] = xcorr;

                    //For normalization.
                    //double sigma1 = 0;
                    //double sigma2 = 0;
                    //for (int z = 0; z < z_len; z++)
                    //    if (z - z_shift >= 0 && z - z_shift < z_len)
                    //    {
                    //        sigma1 += MatrixCalc.SIMD_Dot(shiftImage_n[z - z_shift], shiftImage_n[z - z_shift]);
                    //        sigma2 += MatrixCalc.SIMD_Dot(templateImage_n[z - z_shift], templateImage_n[z - z_shift]);
                    //    }
                    //xCorr[z_shift + max_shift] = xcorr / Math.Sqrt(sigma1) / Math.Sqrt(sigma2); 
                }

                double[] fit_data = MatrixCalc.FindPeak_WithGaussianFit1D_NoOffset(xCorr);


#if DEBUG
                double[] x1 = Enumerable.Range(0, xCorr.Length).Select(i => (double)i - max_shift).ToArray();
                double[] x2 = Enumerable.Range(0, xCorr.Length * 10).Select(i => (double)i / 10.0).ToArray();
                double[] y2 = MatrixCalc.Gaussian_NoOffset(fit_data, x2);

                x2 = x2.Select(x => x - max_shift).ToArray(); //after fitting asignment.
                var plot = ImageProcessing.Plot(x1, xCorr);
                plot.AddPlot(x2, y2);
#endif

                z_drift = fit_data[1] - max_shift;
                if (z_drift > max_shift * 2)
                    z_drift = max_shift * 2;
                if (z_drift < -max_shift * 2)
                    z_drift = -max_shift * 2;
            }

            xyz_drift[2] = z_drift;
            return z_drift;
        }

        public void CalculateDrift(bool move_stage)
        {
            if (!template_image_set)
                return;

            var xyz_drift = calculateDriftXYZ();

            var State = flimage.image_display.FLIM_ImgData.State;

            double[] voltage_xy = new double[2];
            double width = (double)current_image.GetLength(1);
            double height = (double)current_image.GetLength(0);
            voltage_xy[0] = -xyz_drift[0] / width / State.Acq.zoom * State.Acq.scanVoltageMultiplier[0] * State.Acq.XMaxVoltage;
            voltage_xy[1] = -xyz_drift[1] / height / State.Acq.zoom * State.Acq.scanVoltageMultiplier[1] * State.Acq.YMaxVoltage;
            voltage_xy = MatrixCalc.Rotate(voltage_xy, State.Acq.Rotation);

            double[] xyz_um = new double[3];
            xyz_um[0] = voltage_xy[0] / State.Acq.XMaxVoltage * State.Acq.field_of_view[0];
            xyz_um[1] = voltage_xy[1] / State.Acq.YMaxVoltage * State.Acq.field_of_view[1];
            xyz_um[2] = xyz_drift[2] * flimage.image_display.FLIM_ImgData.State.Acq.sliceStep;

            Status_XY.BeginInvokeIfRequired(o => o.Text = String.Format("Image shift: {0:0}, {1:0} pixels", xyz_drift[0], xyz_drift[1]));
            Status_V.BeginInvokeIfRequired(o => o.Text = String.Format("Voltage: {0:0.000} V, {1:0.000} V", voltage_xy[0], voltage_xy[1]));
            Status_XYZ_um.BeginInvokeIfRequired(o => o.Text = String.Format("Estimated drift: {0:0.00}, {1:0.00}, {2:0.00} um", xyz_um[0], xyz_um[1], xyz_drift[2]));

            motor_pos_current = (double[])flimage.image_display.FLIM_ImgData.State.Motor.motorPosition.Clone();

            double x_shift = motor_pos_current[0] - motor_pos_template[0];
            double y_shift = motor_pos_current[1] - motor_pos_template[1];
            double z_shift = motor_pos_current[2] - motor_pos_template[2];
            Status_Motor.BeginInvokeIfRequired(o => o.Text = String.Format("Motor position: {0:0.00}, {1:0.00}, {2:0.00} um", 
               x_shift, y_shift, z_shift));

            if (DriftCorrection_CB.Checked && move_stage)
            {
                if (XYCorrect_CB.Checked)
                {
                    if (UseMirror_CB.Checked)
                    {
                        State.Acq.XOffset = State.Acq.XOffset - voltage_xy[0];
                        State.Acq.YOffset = State.Acq.YOffset - voltage_xy[1];
                        flimage.ReSetupValues(false);
                    }
                    else
                    {
                        flimage.motorCtrl.SetNewPosition_StepSize_um(new double[] { -xyz_um[0], -xyz_um[1], 0 });
                        flimage.SetMotorPosition(true, true, false);
                    }
                }

                if (ZCorrect_CB.Checked)
                {
                    if (MoveOposite_Z.Checked)
                        flimage.motorCtrl.SetNewPosition_StepSize_um(new double[] { 0, 0, xyz_um[2] });
                    else
                        flimage.motorCtrl.SetNewPosition_StepSize_um(new double[] { 0, 0, -xyz_um[2] });

                    flimage.SetMotorPosition(true, true, false);
                }
            }
        }

        public void EventHandling(FLIMage_IO fc, ProcessEventArgs e)
        {
            var State = flimage.State;

            String eventStr = e.EventName;
            String eventName = "";
            if (State.Acq.fastZScan)
                eventName = "SaveImageDone";
            else if (State.Acq.ZStack)
                eventName = "AcquisitionDone";
            else
                eventName = "AcquisitionDone";

            if (eventStr == eventName)
            {
                CalculateDrift(true);
            }
        }

        public void SaveWindowLocation()
        {
            SaveSetting();
            if (winManager != null)
            winManager.SaveWindowLocation();
        }

        public void DriftCorrection_Load(object sender, EventArgs e)
        {
            var State = flimage.State;
            winManager = new WindowLocManager(this, WindowName, State.Files.windowsInfoPath);
            winManager.LoadWindowLocation(false);
            InitializeSetting();
        }

        public void DriftCorrection_FormClosing(object sender, FormClosingEventArgs e)
        {
            flimage.flimage_io.EventNotify -= EventHandling;
            SaveSetting();
            SaveWindowLocation();
            Hide();
            flimage.ToolWindowClosed();
        }

        public double[] calculate_drift()
        {
            double[] xy_drift;
            double corVal = ImageProcessing.MatrixMeasureDrift2D_FFT(template_image, current_image, out xy_drift);
            return xy_drift;
        }

        public double[] getProjection_z()
        {
            double[] data_projection_z = new double[imageZYX.Length];

            for (int z = 0; z < imageZYX.Length; z++)
            {
                data_projection_z[z] = (double)MatrixCalc.MatrixSum(imageZYX[z]);
            }

            return data_projection_z;
        }

        public UInt16[,] getProjection(ushort[][,] image_3d)
        {
            int y_length = image_3d[0].GetLength(0);
            int x_length = image_3d[0].GetLength(1);

            UInt16[,] data_projection = new ushort[y_length, x_length];

            for (int z = 0; z < image_3d.Length; z++)
                MatrixCalc.MatrixCalc2D(data_projection, image_3d[z], CalculationType.Max, true);

            return data_projection;
        }

        private void calculateDriftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CalculateDrift(false);
        }

        private void setCurrentImageForTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectImage();
        }

        public void TemplateImage_PB_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(templateBMP,
                new Rectangle(0, 0, TemplateImage_PB.Width, TemplateImage_PB.Height), // destination rectangle 
                0, 0,           // upper-left corner of source rectangle
                templateBMP.Width,       // width of source rectangle
                templateBMP.Height,      // height of source rectangle
                GraphicsUnit.Pixel);

        }
    }
}
