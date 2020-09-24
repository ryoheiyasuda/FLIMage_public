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

namespace FLIMage.FlowControls
{
    public partial class DriftCorrection : Form
    {
        FLIMageMain FLIMage;
        ScanParameters State;
        ushort[,] template_image;
        double[] template_z_profile;
        //ushort[][] template_image_xz;
        ushort[,] current_image;
        //ushort[][] current_image_xz;
        double[] current_z_profile;
        ushort[][][,,] FLIM_5D;
        ushort[][,] imageZYX; //[z][x,y]
        int[] dimYXT = new int[] { 128, 128, 64 };
        int nSlices = 0;
        Bitmap templateBMP, templateBMP_XZ;
        bool template_image_set = false;
        int correctionChannel = 0;
        WindowLocManager winManager;
        String WindowName = "DriftCorrection.loc";
        SettingManager settingManager;
        String settingName = "DriftCorrection";

        public DriftCorrection(FLIMageMain FLIMage_in)
        {
            InitializeComponent();

            FLIMage = FLIMage_in;
            State = FLIMage.State;
            Channel_Pulldown.SelectedIndex = 0; // FLIMage.image_display.currentChannel;

            FLIMage.flimage_io.EventNotify += new FLIMage_IO.FLIMage_EventHandler(EventHandling);

            template_image = new ushort[128, 128];
            template_z_profile = new double[16];
            ApplyBMP();
        }

        void InitializeSetting()
        {
            settingManager = new SettingManager(settingName, FLIMage.State.Files.initFolderPath);
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
            templateBMP = ImageProcessing.FormatImage(FLIMage.image_display.State_intensity_range[correctionChannel], template_image);
            TemplateImage_PB.Invalidate();
        }

        public void TurnOnOffCorrection(bool ON)
        {
            DriftCorrection_CB.InvokeIfRequired(o => o.Checked = ON);
        }

        public void SelectImage()
        {
            TemplateFileName.Text = FLIMage.image_display.FLIM_ImgData.fullFileName;
            correctionChannel = Channel_Pulldown.SelectedIndex;
            reconstructImageFromPages();

            template_image = getProjection();
            template_z_profile = getProjection_z();

            template_image_set = true;

            this.BeginInvokeIfRequired(o => o.ApplyBMP());
        }

        public void reconstructImageFromPages()
        {
            FLIM_5D = (ushort[][][,,])Copier.DeepCopyArray(FLIMage.image_display.FLIM_ImgData.FLIM_Pages);
            dimYXT = FLIMage.image_display.FLIM_ImgData.dimensionYXT(correctionChannel);
            nSlices = FLIM_5D.Length;


            imageZYX = new ushort[nSlices][,];
            for (int z = 0; z < nSlices; z++)
                imageZYX[z] = ImageProcessing.GetProjectFromFLIM(FLIM_5D[z][correctionChannel]);

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

            return xyz_drift;
        }

        public double CalculateZdrift()
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

        public void CalculateDrift()
        {
            if (!template_image_set)
                return;

            var xyz_drift = calculateDriftXYZ();
            double[] voltage_xy = new double[2];
            double width = (double)current_image.GetLength(1);
            double height = (double)current_image.GetLength(0);
            voltage_xy[0] = xyz_drift[0] / width / State.Acq.zoom * State.Acq.scanVoltageMultiplier[0] * State.Acq.XMaxVoltage;
            voltage_xy[1] = xyz_drift[1] / height / State.Acq.zoom * State.Acq.scanVoltageMultiplier[1] * State.Acq.YMaxVoltage;
            voltage_xy = MatrixCalc.Rotate(voltage_xy, State.Acq.Rotation);

            double[] xy_um = new double[2];
            xy_um[0] = voltage_xy[0] / State.Acq.XMaxVoltage * State.Acq.field_of_view[0];
            xy_um[1] = voltage_xy[1] / State.Acq.YMaxVoltage * State.Acq.field_of_view[1];

            Status_XY.BeginInvokeIfRequired(o => o.Text = String.Format("X, Y drift = {0:0}, {1:0} pixels", xyz_drift[0], xyz_drift[1]));
            Status_Z.BeginInvokeIfRequired(o => o.Text = String.Format("Z drift = {0:0.00}", xyz_drift[2]));
            Status_V.BeginInvokeIfRequired(o => o.Text = String.Format("Voltage = {0:0.000} V, {1:0.000} V", voltage_xy[0], voltage_xy[1]));
            Status_um.BeginInvokeIfRequired(o => o.Text = String.Format("Micrometers = {0:0.000} um, {1:0.000} um", xy_um[0], xy_um[1]));

            if (DriftCorrection_CB.Checked)
            {
                if (XYCorrect_CB.Checked)
                {
                    State.Acq.XOffset = State.Acq.XOffset + voltage_xy[0];
                    State.Acq.YOffset = State.Acq.YOffset + voltage_xy[1];
                    FLIMage.ReSetupValues(false);
                }
                else
                {
                    FLIMage.motorCtrl.SetNewPosition_StepSize_um(new double[] { -xy_um[0], -xy_um[1], 0 });
                    FLIMage.SetMotorPosition(true, true);
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

        public void EventHandling(FLIMage_IO fc, ProcessEventArgs e)
        {
            String eventStr = e.EventName;
            String eventName;
            if (State.Acq.fastZScan)
                eventName = "SaveImageDone";
            else if (State.Acq.ZStack)
                eventName = "AcquisitionDone";
            else
                eventName = "AcquisitionDone";

            if (eventStr == eventName)
            {
                CalculateDrift();
            }
        }

        public void SaveWindowLocation()
        {
            winManager.SaveWindowLocation();
        }

        public void DriftCorrection_Load(object sender, EventArgs e)
        {
            winManager = new WindowLocManager(this, WindowName, State.Files.windowsInfoPath);
            winManager.LoadWindowLocation(false);
        }

        public void DriftCorrection_FormClosing(object sender, FormClosingEventArgs e)
        {
            FLIMage.flimage_io.EventNotify -= EventHandling;
            SaveSetting();
            SaveWindowLocation();
            Hide();
            FLIMage.ToolWindowClosed();
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

        public UInt16[,] getProjection()
        {
            int z_length = FLIM_5D.Length;
            int c_length = FLIM_5D[0].Length;
            int y_length = dimYXT[0];
            int x_length = dimYXT[1];
            int t_length = dimYXT[2];

            UInt16[,] data_projection = new ushort[y_length, x_length];

            for (int z = 0; z < nSlices; z++)
                MatrixCalc.MatrixCalc2D(data_projection, imageZYX[z], "Max", true);

            return data_projection;
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
