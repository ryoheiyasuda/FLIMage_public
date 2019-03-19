using MathLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace FLIMimage
{
    public partial class DriftCorrection : Form
    {
        FLIMageMain FLIMage;
        ScanParameters State;
        ushort[][] template_image;
        double[] template_z_profile;
        //ushort[][] template_image_xz;
        ushort[][] current_image;
        //ushort[][] current_image_xz;
        double[] current_z_profile;
        ushort[][][] FLIM_5D;
        ushort[][][] imageZYX;
        int[] dimYXT = new int[] { 128, 128, 64 };
        int nSlices = 0;
        Bitmap templateBMP, templateBMP_XZ;
        bool template_image_set = false;
        int correctionChannel = 0;
        WindowLocManager winManager;
        String WindowName = "DriftCorrection.loc";

        public DriftCorrection(FLIMageMain FLIMage_in)
        {
            InitializeComponent();

            FLIMage = FLIMage_in;
            State = FLIMage.State;
            Channel_Pulldown.SelectedIndex = 0; // FLIMage.image_display.currentChannel;

            FLIMage.EventNotify += new FLIMageMain.FLIMage_EventHandler(EventHandling);

            template_image = MatrixCalc.MatrixCreate2D<ushort>(128, 128);
            template_z_profile = new double[16];
            ApplyBMP();
        }

        private void SelectCurrentImage_Click(object sender, EventArgs e)
        {
            SelectImage();
        }

        private void ApplyBMP()
        {
            templateBMP = ImageProcessing.FormatImage(FLIMage.image_display.State_intensity_range[correctionChannel], template_image);
            TemplateImage_PB.Invalidate();
        }

        public void TurnOnOffCorrection(bool ON)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)delegate
                {
                    DriftCorrection_CB.Checked = ON;
                });
            }
            else
                DriftCorrection_CB.Checked = ON;
        }

        public void SelectImage()
        {
            TemplateFileName.Text = FLIMage.image_display.FLIM_ImgData.fullFileName;
            correctionChannel = Channel_Pulldown.SelectedIndex;
            reconstructImageFromPages();

            template_image = getProjection();
            template_z_profile = getProjection_z();

            template_image_set = true;
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)delegate
                {
                    ApplyBMP();
                });
            }
            else
                ApplyBMP();
        }

        private void reconstructImageFromPages()
        {
            FLIM_5D = MatrixCalc.MatrixCopy3D(FLIMage.image_display.FLIM_ImgData.FLIM_Pages);
            dimYXT = FLIMage.image_display.FLIM_ImgData.dimensionYXT(correctionChannel);
            nSlices = FLIM_5D.Length;


            imageZYX = new ushort[nSlices][][];
            for (int z = 0; z < nSlices; z++)
                imageZYX[z] = ImageProcessing.GetProjectFromFLIM_Linear(FLIM_5D[z][correctionChannel], dimYXT);

        }

        public double[] calculateDriftXYZ()
        {
            double[] xyz_drift = new double[3];

            if (template_image_set)
            {
                reconstructImageFromPages();

                current_image = getProjection();
                current_z_profile = getProjection_z();

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

                xyz_drift[2] = CalculateZdrift();
            }

            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)delegate
                {
                    DisplayDriftXYZ(xyz_drift);
                });
            }
            else
            {
                DisplayDriftXYZ(xyz_drift);
            }

            return xyz_drift;
        }

        private double CalculateZdrift()
        {
            double z_drift = 0;
            if (nSlices > 3)
            {
                var xcorr = MatrixCalc.xcorr(template_z_profile, current_z_profile);

                if (xcorr != null)
                    z_drift = -(xcorr.ToList().IndexOf(xcorr.Max()) - xcorr.Length / 2);
                else
                    z_drift = double.NaN;
            }

            return z_drift;
        }

        private void DisplayDriftXYZ(double[] xyz_drift)
        {
            Status_XY.Text = String.Format("X, Y drift = {0:0}, {1:0} pixels", xyz_drift[0], xyz_drift[1]);
            Status_Z.Text = String.Format("Z drift = {0:0.00}", xyz_drift[2]);
        }

        public void CalculateDrift()
        {
            if (!template_image_set)
                return;

            var xyz_drift = calculateDriftXYZ();
            double[] voltage_xy = new double[2];
            double width = (double)current_image[0].Length;
            double height = (double)current_image.Length;
            voltage_xy[0] = xyz_drift[0] / width / State.Acq.zoom * State.Acq.scanVoltageMultiplier[0] * State.Acq.XMaxVoltage;
            voltage_xy[1] = xyz_drift[1] / height / State.Acq.zoom * State.Acq.scanVoltageMultiplier[1] * State.Acq.YMaxVoltage;
            voltage_xy = MatrixCalc.Rotate(voltage_xy, State.Acq.Rotation);

            if (this.InvokeRequired)
                this.BeginInvoke((Action)delegate
                {
                    Status_V.Text = String.Format("Votlage = {0:0.000} V, {1:0.000} V", voltage_xy[0], voltage_xy[1]);
                });
            else
                Status_V.Text = String.Format("Votlage = {0:0.000} V, {1:0.000} V", voltage_xy[0], voltage_xy[1]);

            if (DriftCorrection_CB.Checked)
            {
                if (XYCorrect_CB.Checked)
                {
                    State.Acq.XOffset = State.Acq.XOffset + voltage_xy[0];
                    State.Acq.YOffset = State.Acq.YOffset + voltage_xy[1];
                    FLIMage.ReSetupValues(false);
                }

                if (ZCorrect_CB.Checked)
                {
                    if (MoveOposite_Z.Checked)
                        FLIMage.motorCtrl.SetNewPosition_StepSize_um(new double[] { 0, 0, -xyz_drift[2] });
                    else
                        FLIMage.motorCtrl.SetNewPosition_StepSize_um(new double[] { 0, 0, xyz_drift[2] });

                    FLIMage.SetMotorPosition(true, true);
                }
            }
        }

        private void EventHandling(FLIMageMain fc, FLIMageMain.ProcessEventArgs e)
        {
            String eventStr = e.EventName;
            String eventName;
            if (State.Acq.fastZScan)
                eventName = "SaveImageDone";
            else if (State.Acq.ZStack)
                eventName = "AcquisitionDone";
            else
                return;

            if (eventStr == eventName)
            {
                CalculateDrift();
            }
        }

        public void SaveWindowLocation()
        {
            winManager.SaveWindowLocation();
        }

        private void DriftCorrection_Load(object sender, EventArgs e)
        {
            winManager = new WindowLocManager(this, WindowName, State.Files.windowsInfoPath);
            winManager.LoadWindowLocation(false);
        }

        private void DriftCorrection_FormClosing(object sender, FormClosingEventArgs e)
        {
            FLIMage.EventNotify -= EventHandling;
            SaveWindowLocation();
            Hide();
            FLIMage.ToolWindowClosed();
        }

        private double[] calculate_drift()
        {
            double[] xy_drift;
            double corVal = ImageProcessing.MatrixMeasureDrift2D_FFT(template_image, current_image, out xy_drift);
            return xy_drift;
        }

        private double[] getProjection_z()
        {
            double[] data_projection_z = new double[imageZYX.Length];

            for (int z = 0; z < imageZYX.Length; z++)
            {
                data_projection_z[z] = (double)MatrixCalc.MatrixSum(imageZYX[z]);
            }

            return data_projection_z;
        }

        private UInt16[][] getProjection()
        {
            int z_length = FLIM_5D.Length;
            int c_length = FLIM_5D[0].Length;
            int y_length = dimYXT[0];
            int x_length = dimYXT[1];
            int t_length = dimYXT[2];

            UInt16[][] data_projection = MatrixCalc.MatrixCreate2D<UInt16>(y_length, x_length);

            for (int z = 0; z < nSlices; z++)
                MatrixCalc.MatrixCalc2D(data_projection, imageZYX[z], "Max", true);

            return data_projection;
        }

        private void TemplateImage_PB_Paint(object sender, PaintEventArgs e)
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
