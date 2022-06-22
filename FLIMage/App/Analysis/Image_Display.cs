
using FLIMage.FlowControls;
using FLIMage.Plotting;
using MathLibrary;
using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace FLIMage.Analysis
{
    public partial class Image_Display : Form
    {
        const int N_MAXCHANNEL = 4;
        const int N_RANGE = 2; //start, end;
        const int N_DISPLAYCHANNEL = 2;
        const int N_TOTALDISPLAY = 3; //Ch1, Ch2, Ch12.
        const int N_FITPARAM = 6;

        const int MIN_INTENSITY_ROUND = 3;

        const String TempFileName = "Temp";


        double[] INITIAL_LIFETIME_RANGE = new double[] { 2.5, 3.5 };

        public FLIMData FLIM_ImgData;
        /// Drawing
        Bitmap IntensityBitmap;
        Bitmap IntensityBitmap2; //Second channel;
        Bitmap IntensityBitmap_Save, IntensityBitmap_Save2;
        Bitmap Bitmap_FastZ_PhaseComplementary; //For phase-detection mode. Showing complementary phases at once. 
        Bitmap FLIMBitmap;

        public Bitmap BlankBMP = ImageProcessing.FormatImage(new double[] { -1.0, 1.0 }, MatrixCalc.MatrixCreate2D<ushort>(128, 128));

        ushort[][,] FocusProjectBuffer;
        double[][,] FocusFLIMimageBuffer;
        public byte[][] pixelsBuffer;

        //ROI manipulation
        //Rectangle boxRoi_FLIM;
        public ROI imageRoi = new ROI();
        ROI imageRoiSave = new ROI();
        ROI imageRoiOld = new ROI();
        ROI CurrentROIBeforeMove = new ROI();
        float polyLineRadius = 8;
        float CircleDefaulRadius = 8;
        float image_scale = 3;

        float handleSize = 3;

        bool ThreeDRoi = false;
        int[] RoiZ = new int[] { 0 };

        drawROI_State roi_State = drawROI_State.NoROI;
        //bool drawing_ROI = false;
        //bool drag_ROI = false;
        //bool drag_ROICorner = false;
        //bool move_currentROI = false;

        int drag_ROI_whichCorner = 0;

        PointF startPos_FLIM, currentPos_FLIM, diffPos_FLIM;
        //int currentRoi = 0;
        //List<Rectangle> ROIs = new List<Rectangle>();
        ROI.ROItype ROItype = ROI.ROItype.Rectangle;

        public bool ZStack;
        public bool FastZStack;

        //Drawing parameters
        public bool realtime = false;
        public bool focusing = false;
        public int currentChannel = 0;

        public bool update_image_busy = false;

        Pen linePen = new Pen(Color.Black, (float)1);
        Pen plotPen = new Pen(Color.Blue, (float)1.5);
        Pen plotPen2 = new Pen(Color.DarkCyan, (float)1);
        Pen fitPen = new Pen(Color.Red, (float)1);
        Pen fitPen2 = new Pen(Color.Orange, (float)1);

        Font drawFont = new Font("Times", 10);
        Font fontTop = new Font("Times", 8);

        // Dialog parameters
        int binning = 2;
        int[] averagePages = new int[] { 1, 2 };

        // fitting parameters
        int[][] fit_range;
        double[][] fit_param;
        bool fittingDone = false;

        //int nAveragedSave;
        double[][] intensity_range_perFrame;
        double[][] FLIM_intensity_range_perFrame;
        double[][] FLIM_lifetime_range_perFrame; //just to save.

        public double[][] State_intensity_range;
        double[][] State_FLIM_intensity_range;
        double[][] State_FLIM_lifetime_range;

        //Pages        
        int[] page_range = new int[N_RANGE]; //start end
        public bool displayZProjection = false;
        public bool entireStack = true;

        //Uncaging
        public UncagingCursor drawUncagingPos = UncagingCursor.Inactive;
        private Point uncagingLoc = new Point(-1, -1); //Temporary uncaging location --- it will move with cursor.
        private Point uLoc = new Point(-1, -1); //It is the position of uncaging on display. 
        public List<double[]> uncagingLocs = new List<double[]>();
        public Point referenceLoc = new Point(-1, -1);
        public double[] uncagingLocFrac = new double[] { -1, -1 };

        //Subpanels for split scanning
        public int[] subPanel = new int[] { 1, 1 };

        //public bool moveUncaging = false;
        Stopwatch moveMouseEvent = new Stopwatch();

        delegate void UncagePosStartFunc();
        public bool uncaging_on = false;
        public bool calib_on = false;

        private int currentUncaging = -1; //0 = current. 1- multiple uncaging.
        private double sizeOfMark = 0.05; // times screen.

        object sync_disp = new object();
        object sync_disp2 = new object();

        List<object> syncBmp = new List<object>();
        object syncBmpFLIM = new object();

        //double SldrZoom = 10.0;
        //double SldrZoomFLIM = 100.0;
        int SldrMax = 1000;
        int SldrMaxDefault = 1000;

        //ROI pen
        Pen roiPen = new Pen(Brushes.Red, 2.0F);
        Pen multiRoiPen = new Pen(Brushes.Cyan, 2.0F);
        Pen BgRoiPen = new Pen(Brushes.GreenYellow, 2.0F);
        //
        Pen roiPenOutOfFocus = new Pen(Brushes.Red, 0.5F);
        Pen multiRoiPenOutOfFocus = new Pen(Brushes.Cyan, 0.5F);
        Pen BgRoiPenOutOfFocus = new Pen(Brushes.GreenYellow, 2.0F);

        Pen CursorPen = new Pen(Brushes.Red, 1.0F);
        Pen UncageCPen = new Pen(Brushes.Yellow, 1.0F);
        Pen UncageCPenThick = new Pen(Brushes.Yellow, 2.0F);
        Pen multiUncagePen = new Pen(Brushes.Cyan, 2.0F);

        Pen RefCPenThick = new Pen(Brushes.Orange, 2.0F);

        SolidBrush roiBrush = new SolidBrush(Color.Cyan);
        Font roiFont = new Font("Arial", 12, FontStyle.Bold);

        //fastPlot realtimePlot;
        public List<double> realtimeData = new List<double>();
        List<double> realtimeTime = new List<double>();

        public plot_timeCourse plot_realtime;
        public plot_timeCourse plot_regular;
        //
        public TimeCourse TC = new TimeCourse();
        public TimeCourse_Files TCF = new TimeCourse_Files();
        //
        int panelMerginY, imageMerginY, st_MerginY;
        public FLIMageMain flimage;

        bool StopFileOpening = false;
        // Scan parameters

        TrackBar[][] MaxMinSliders;// = new TrackBar[3][];
        TextBox[][] MaxMinTextBox; //= new TextBox[3][];
        double[] SldrZoom = new double[] { 10.0, 10.0, 100.0 };

        //Tools 
        FastZ_Calibration fast_calib;

        ImageDisplay_State image_display_state = ImageDisplay_State.Idle;

        String PythonPath = "";
        String ScriptDirectoryName = "Python Scripts";
        String ScriptPath = "";
        public String lastFilePath = "";
        SettingManager settingManager;
        String settingName = "ImageDisplay";

        List<RadioButton> ChannelChecks = new List<RadioButton>();
        List<RadioButton> ChChecks = new List<RadioButton>();

        ImageProcessing.ColorScheme color_scheme = ImageProcessing.ColorScheme.YellowHighlight;

        String[] ZStack_Text = { "Swtich to ZStack", "Swtich to Timecourse" };

        //bool SavePageInMemory = false;

        public Image_Display(FLIMData FLIM, FLIMageMain mc, bool createSim)
        {
            InitializeComponent();

            flimage = mc;

            FLIM_ImgData = FLIM;
            this.Text = "FLIMage! Analysis Version " + flimage.versionText;

            ScriptPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FLIMage", ScriptDirectoryName);

            if (createSim)
            {
                FLIM.nAveragedFrame = new int[] { 5, 5 };
                FLIM.calculateAll();
                FLIM.addCurrentToPage5D(0, false);

                try
                {
                    ImageInfo info = new ImageInfo(FLIM_ImgData);
                    TC.AddFile(info, 0);
                    TCF.AddFile(TC);
                    TCF.calculate();
                    TCF.FileName = TempFileName;
                }
                catch (Exception E)
                {
                    Debug.WriteLine("Problem in creatingSim: " + E.Message);
                }

            }

            for (int i = 0; i < N_MAXCHANNEL; i++)
                syncBmp.Add(new object());

            page_range[0] = 0;

            if (FLIM_ImgData.nFastZ > 1)
                page_range[1] = FLIM.nFastZ;
            else
                page_range[1] = FLIM.n_pages;

            PageEnableControl();

            MapControls();

            int nChannels = FLIM_ImgData.nChannels;
            FLIM_ImgData.loadFittingParamFromState();


            fit_range = new int[N_MAXCHANNEL][];
            fit_param = new double[N_MAXCHANNEL][];

            for (int ch = 0; ch < FLIM_ImgData.fit_range.Length; ch++)
            {
                fit_range[ch] = (int[])FLIM_ImgData.fit_range[ch].Clone();
                fit_param[ch] = (double[])((double[])FLIM_ImgData.State.Spc.analysis.GetType().GetField("fit_param" + (ch + 1)).GetValue(FLIM_ImgData.State.Spc.analysis)).Clone();
            }

            colorBar.Image = ImageProcessing.CreateColorBar(colorBar.Width, colorBar.Height, ImageProcessing.ColorBarDirection.RightToLeft, color_scheme);

            Auto1.Checked = true;
            intensity_range_perFrame = MatrixCalc.MatrixCreate2D<double>(N_MAXCHANNEL, N_RANGE);
            FLIM_intensity_range_perFrame = MatrixCalc.MatrixCreate2D<double>(N_MAXCHANNEL, N_RANGE);
            FLIM_lifetime_range_perFrame = MatrixCalc.MatrixCreate2D<double>(N_MAXCHANNEL, N_RANGE);

            for (int i = 0; i < N_MAXCHANNEL; i++)
                SaveIntensity_Range(i, true);

            UpdateImageParamText();
            for (int ch = 0; ch < nChannels; ch++)
                UpdateFittingParam(ch, false);

            UncagingBox.Visible = false;
            //UpdateImages(true, false, false, true);

            panelMerginY = ctrlPanel.Location.Y - Image1.Bottom;
            imageMerginY = Image1.Top;
            st_MerginY = st_im1.Top;

            SelectRoi.Checked = true;
            UpdateFileName();
        }

        private void Image_Display_Load(object sender, EventArgs e)
        {
            colorSchemeCheck();
            intelMKLLibraryOnToolStripMenuItem.Checked = MatrixCalc.IntelMKL_on;
            SetFastZModeDisplay(false);
            plot_realtime = new plot_timeCourse(true, this);
            plot_regular = new plot_timeCourse(false, this);
            if (AutoApplyOffset.Checked)
                plot_regular.TurnOnCalcUponOpen(true);

            InitializeSetting();
        }

        void InitializeSetting()
        {
            settingManager = new SettingManager(settingName, flimage.State.Files.initFolderPath);
            settingManager.AddToDict(keepPagesInMemoryToolStripMenuItem);
            settingManager.AddToDict(MaxIntensity3);
            settingManager.AddToDict(MinIntensity3);
            settingManager.AddToDict(fit_start);
            settingManager.AddToDict(fit_end);
            settingManager.AddToDict(tau1);
            settingManager.AddToDict(tau2);
            settingManager.AddToDict(cb_tau1Fix);
            settingManager.AddToDict(cb_tau2Fix);
            settingManager.AddToDict(AutoApplyOffset);
            settingManager.AddToDict("lastFilePath", lastFilePath);
            settingManager.AddToDict("PythonPath", PythonPath);
            settingManager.AddToDict("ScriptPath", ScriptPath);
            settingManager.LoadToObject();

            lastFilePath = settingManager.settingData["lastFilePath"];
            if (Directory.Exists(settingManager.settingData["PythonPath"]))
                PythonPath = settingManager.settingData["PythonPath"];

            if (Directory.Exists(settingManager.settingData["ScriptPath"]))
                ScriptPath = settingManager.settingData["ScriptPath"];
            //FLIM_ImgData.fromFullNameToFolderPathAndFileName(lastFilePath);
        }

        public void SaveSetting()
        {
            if (settingManager != null)
            {
                settingManager.settingData["lastFilePath"] = lastFilePath;
                settingManager.settingData["PythonPath"] = PythonPath;
                settingManager.settingData["ScriptPath"] = ScriptPath;
                settingManager.SaveFromObject();
            }
        }

        private void MapControls()
        {
            for (int i = 0; i < N_MAXCHANNEL; i++)
            {
                var ctrl = this.Controls.Find("Channel" + (i + 1), true);
                if (ctrl != null && ctrl.Length > 0)
                    ChannelChecks.Add((RadioButton)ctrl[0]);

                var ctrl2 = this.Controls.Find("Ch" + (i + 1), true);
                if (ctrl2 != null && ctrl2.Length > 0)
                    ChChecks.Add((RadioButton)ctrl2[0]);
            }

            MaxMinSliders = new TrackBar[N_TOTALDISPLAY][];
            MaxMinTextBox = new TextBox[N_TOTALDISPLAY][];

            for (int i = 0; i < N_TOTALDISPLAY; i++)
            {
                MaxMinSliders[i] = new TrackBar[N_RANGE];
                MaxMinTextBox[i] = new TextBox[N_RANGE];
                Control[] Found = this.Controls.Find("MaxSldr" + (i + 1), true);
                if (Found != null)
                {
                    MaxMinSliders[i][1] = (TrackBar)Found[0];
                }
                Found = this.Controls.Find("MinSldr" + (i + 1), true);
                if (Found != null)
                {
                    MaxMinSliders[i][0] = (TrackBar)Found[0];
                }
                Found = this.Controls.Find("MaxIntensity" + (i + 1), true);
                if (Found != null)
                {
                    MaxMinTextBox[i][1] = (TextBox)Found[0];
                }
                Found = this.Controls.Find("MinIntensity" + (i + 1), true);
                if (Found != null)
                {
                    MaxMinTextBox[i][0] = (TextBox)Found[0];
                }
            }
        }

        private void Image_Display_Resize(object sender, EventArgs e)
        {
            int imageHeight = ctrlPanel.Top - panelMerginY - imageMerginY;
            int imageWidth = this.Width * 384 / 1070;
            int imageLength = Math.Min(imageHeight, imageWidth);
            imageHeight = imageLength;
            imageWidth = imageLength;

            Image1.Height = imageHeight;
            Image1.Width = imageWidth;
            Image2.Height = imageHeight;
            Image2.Width = imageWidth;
            Image1.Location = new Point(0, imageMerginY);
            Image2.Location = new Point(imageWidth + 1, imageMerginY);
            LifetimeCurvePlot.Location = new Point(imageWidth * 2 + 2, imageMerginY);
            LifetimeCurvePlot.Width = this.Width - 2 * imageWidth - 3;
            LifetimeCurvePlot.Height = imageHeight;
            st_im1.Location = new Point(0, st_MerginY);
            st_im2.Location = new Point(imageWidth + 1, st_MerginY);
            st_im3.Location = new Point(imageWidth * 2 + 2, st_MerginY - 5);
            image_scale = Image1.Width / Math.Max((float)FLIM_ImgData.width, (float)FLIM_ImgData.height);

            UpdateImages(false, realtime, focusing, false);
        }


        private void Image_Display_ResizeEnd(object sender, EventArgs e)
        {
            UpdateImages(false, realtime, focusing, true);
        }

        public void ChangeRealtimeStatus(bool realtime_1)
        {
            realtime = realtime_1;
            openFLIMImageToolStripMenuItem.Enabled = !realtime;
            saveFLIMImageToolStripMenuItem1.Enabled = !realtime;
            if (FLIM_ImgData.nFastZ == 1)
            {
                PageUp.Enabled = !realtime;
                PageDown.Enabled = !realtime;
                FastZStack = false;
            }
            else
            {
                PageUp.Enabled = true;
                PageDown.Enabled = true;
                FastZStack = true;
            }
            PageUpUp.Enabled = !realtime;
            PageDownDown.Enabled = !realtime;
        }

        ///////////////////////////////////////
        public void ResetFitting(bool resetAllPages)
        {
            FLIM_ImgData.ResetFitting(false);
            fittingDone = false;
        }


        private void UpdateFittingParam(int c, bool fit_done)
        {
            double[] betahat;
            double res = FLIM_ImgData.psPerUnit / 1000;
            int nChannels = FLIM_ImgData.nChannels;
            if (c >= nChannels)
                c = nChannels - 1;

            fit_range[c] = FLIM_ImgData.ApplyFitRange(fit_range[c], c);


            Object obj = FLIM_ImgData.State.Spc.analysis;
            obj.GetType().GetField("fit_range" + (c + 1)).SetValue(obj, FLIM_ImgData.fit_range[c].Clone());

            fit_start.Text = (FLIM_ImgData.fit_range[c][0] + 1).ToString(); //1 base.
            fit_end.Text = FLIM_ImgData.fit_range[c][1].ToString(); //If 1-64, this means 0 <= x < 64)

            FLIM_ImgData.psPerUnit = FLIM_ImgData.State.Spc.spcData.resolution[0];
            psPerUnit.Text = String.Format("{0:0.000}", FLIM_ImgData.State.Spc.spcData.resolution[0]);

            bool showFit = false;
            double[] param1 = new double[6];

            fittingDone = fit_done;

            var beta_src = FLIM_ImgData.RoiFit.flim_parameters.beta0[c];
            if (fit_done || beta_src == null)
                beta_src = FLIM_ImgData.RoiFit.flim_parameters.beta[c];

            if (beta_src != null)
                betahat = (double[])beta_src.Clone();
            else
                betahat = new double[] { fit_param[c][0], res / fit_param[c][1], fit_param[c][2], res / fit_param[c][3],
                    fit_param[c][4] / res, fit_param[c][5] / res};

            FLIM_ImgData.RoiFit.flim_parameters.beta0[c] = (double[])betahat.Clone();

            if (fittingDone && FLIM_ImgData.Fit_type.Equals(FLIMData.FitType.GlobalRois) && Values_selectedROI.Checked && FLIM_ImgData.ROIs.Count > FLIM_ImgData.currentRoi)
            {
                if (FLIM_ImgData.currentRoi >= 0 && FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters.beta.Length > c)
                    betahat = (double[])FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters.beta[c].Clone();
                else
                    return;
            }

            if (betahat.Length == 4)
            {
                param1[0] = betahat[0];
                param1[1] = res / betahat[1];
                param1[2] = fit_param[c][2];
                param1[3] = fit_param[c][3];
                param1[4] = betahat[2] * res;
                param1[5] = betahat[3] * res;

                showFit = true;

                fit_param[c] = (double[])param1.Clone();

                param1[2] = 0;
                param1[3] = 0;
            }
            else if (betahat.Length == 6)
            {
                param1[0] = betahat[0];
                param1[1] = res / betahat[1];
                param1[2] = betahat[2];
                param1[3] = res / betahat[3];
                param1[4] = betahat[4] * res;
                param1[5] = betahat[5] * res;

                showFit = true;

                fit_param[c] = (double[])param1.Clone();
            }
            else
            {
                param1 = (double[])fit_param[c].Clone();
            }

            pop1.Text = String.Format("{0:0.0}", param1[0]);
            tau1.Text = String.Format("{0:0.000}", param1[1]);
            pop2.Text = String.Format("{0:0.0}", param1[2]);
            tau2.Text = String.Format("{0:0.000}", param1[3]);
            tauG.Text = String.Format("{0:0.000}", param1[4]);
            t0.Text = String.Format("{0:0.000}", param1[5]);
            if (showFit)
            {
                frac1.Text = String.Format("{0:0.0%}", param1[0] / (param1[0] + param1[2]));
                frac2.Text = String.Format("{0:0.0%}", param1[2] / (param1[0] + param1[2]));
                tau_m.Text = String.Format("{0:0.000}", FLIM_ImgData.RoiFit.flim_parameters.tau_m[c]);
                xi_square.Text = String.Format("{0:0.000}", FLIM_ImgData.RoiFit.flim_parameters.xi_square[c]);
            }
            else
            {
                frac1.Text = String.Format("");
                frac2.Text = String.Format("");
                tau_m.Text = String.Format("");
                xi_square.Text = String.Format("");
            }

            t0_Img.Text = String.Format("{0:0.00}", FLIM_ImgData.RoiFit.flim_parameters.offset_fit[c]);
            //t0_Img_2.Text = String.Format("{0:0.00}", FLIM_ImgData.offset_fit[0]);
            imgOffset1.Text = String.Format("{0:0.00} (Current)", FLIM_ImgData.offset[c]);
            //imgOffset2.Text = String.Format("{0:0.00}", FLIM_ImgData.offset[0]);

            filterWindow.Text = FLIM_ImgData.State.Display.filterWindow_FLIM.ToString();

            if (FLIM_ImgData.currentRoi >= FLIM_ImgData.ROIs.Count)
                FLIM_ImgData.currentRoi = FLIM_ImgData.ROIs.Count - 1;

            double[][] param2 = (double[][])Copier.DeepCopyArray(fit_param);
            param2[currentChannel] = param1;
            FLIM_ImgData.saveFittingParameters(param2);
        }

        /// <summary>
        /// Return true if settings are changed.
        /// </summary>
        /// <returns></returns>
        private bool SetFitting_Param(bool resetFittingAnyway)
        {
            int c = 0;
            if (Ch2.Checked)
                c = 1;

            if (FLIM_ImgData.FLIM_on[c])
            {
                double valD;
                int valI;

                var betasave = (double[])FLIM_ImgData.RoiFit.flim_parameters.beta0[c].Clone();
                var rangesave = (int[])FLIM_ImgData.fit_range[c].Clone();

                int fit_c_0 = 0;
                int fit_c_1 = FLIM_ImgData.n_time[c];
                if (Int32.TryParse(fit_start.Text, out valI)) fit_c_0 = valI - 1; //If "1", then start from 0.
                if (Int32.TryParse(fit_end.Text, out valI)) fit_c_1 = valI; //For 0 <= x < 64, fit_c_1 is 64.
                if (Double.TryParse(pop1.Text, out valD)) fit_param[c][0] = valD;
                if (Double.TryParse(tau1.Text, out valD)) fit_param[c][1] = valD;
                if (Double.TryParse(pop2.Text, out valD)) fit_param[c][2] = valD;
                if (Double.TryParse(tau2.Text, out valD)) fit_param[c][3] = valD;
                if (Double.TryParse(tauG.Text, out valD)) fit_param[c][4] = valD;
                if (Double.TryParse(t0.Text, out valD)) fit_param[c][5] = valD;

                if (FLIM_ImgData.n_time[c] > 1)
                {
                    if (fit_c_0 >= 0 && fit_c_0 < FLIM_ImgData.n_time[c] - 1)
                        fit_range[c][0] = fit_c_0;

                    if (fit_c_1 > fit_range[c][0] && fit_c_1 <= FLIM_ImgData.n_time[c])
                        fit_range[c][1] = fit_c_1;
                    else
                        fit_range[c][1] = FLIM_ImgData.n_time[c];
                }

                FLIM_ImgData.saveFittingParameters(fit_param);

                this.InvokeIfRequired(o => o.UpdateFittingParam(c, false));

                var beta_now = FLIM_ImgData.RoiFit.flim_parameters.beta0[c];
                bool changed = !(beta_now[1] == betasave[1] && beta_now[3] == betasave[3] &&
                    beta_now[4] == betasave[4] && beta_now[5] == betasave[5]);
                bool changed_fitRange = !FLIM_ImgData.fit_range[c].SequenceEqual(rangesave);

                if (changed_fitRange)
                {
                    FLIM_ImgData.fitRangeChanged();
                    ResetFitting(true);
                }

                if (changed || resetFittingAnyway) //beta is changed.
                {
                    ResetFitting(true);
                }

                return changed || changed_fitRange;
            }

            return false;
        }

        /// <summary>
        /// For Z-projection, calculate all slices.
        /// </summary>
        /// <param name="ON">Turn on off</param>
        public void Set_EntireStack(bool ON)
        {
            entireStack = ON;
            EntireStack_Check.Checked = ON;
        }

        public void Set_DisplayProjection(bool ON)
        {
            displayZProjection = ON;
            cb_projectionYes.Checked = ON;
        }

        /// <summary>
        /// Called by FLIMage main window, before performing real-time image acquisition.
        /// </summary>
        /// <param name="Scan"></param>
        /// <param name="flimdata"></param>
        public void SetupRealtimeImaging(ScanParameters Scan, FLIMData flimdata)
        {
            ScanParameters State = Scan;
            FLIM_ImgData = flimdata;

            FLIM_ImgData.psPerUnit = FLIM_ImgData.State.Spc.spcData.resolution[currentChannel];
            psPerUnit.Text = String.Format("{0:0.000}", FLIM_ImgData.State.Spc.spcData.resolution[currentChannel]);

            UpdateImages(true, false, false, true); //30 ms.            
            Refresh();

            FastZStack = (State.Acq.fastZScan && State.Acq.FastZ_nSlices > 1);

            bool projectionCondition = State.Acq.ZStack && !FastZStack;
            Set_DisplayProjection(projectionCondition);
            Set_EntireStack(projectionCondition);

            realtimeData.Clear();
            ImageRoiToFLIMage();
        }


        ///////////////////////////////////////
        ///////////////////////////////////////
        ///////////////////////////////////////
        //ROI functions from here.

        private void RemoveAllRoisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveAllROIs(sender, e);
        }

        private void RemoveAllROIs(object sender, EventArgs e)
        {

            imageRoi = new ROI();
            FLIM_ImgData.Roi.polyLineROI_Radius = polyLineRadius;

            FLIM_ImgData.ResetRoi(true);
            SetFitting_Param(true);
            roi_State = drawROI_State.NoROI;

            DrawImages();
        }

        public void copyROIVectors(ROI sourceroi, ref ROI distroi)
        {
            if (distroi == null)
                distroi = new ROI(sourceroi);
            distroi.X = (float[])sourceroi.X.Clone();
            distroi.Y = (float[])sourceroi.Y.Clone();
            distroi.Z = (int[])sourceroi.Z.Clone();
            distroi.Points = (PointF[])sourceroi.Points.Clone();
            distroi.Rect = new RectangleF(sourceroi.Rect.Location, sourceroi.Rect.Size);
            distroi.Roi3d = sourceroi.Roi3d;
            distroi.ROI_type = sourceroi.ROI_type;
            distroi.polyLineROI_Radius = sourceroi.polyLineROI_Radius;
            //distroi.polyLineROIs = new List<ROI>(distroi.polyLineROIs); //ShallowCopy.
            if (sourceroi.ROI_type == ROI.ROItype.PolyLine)
            {
                distroi.GetSmoothCurve();
                distroi.GetEqualDistanceCenters(polyLineRadius * image_scale);
            }
        }

        public void convertROIfromDisplayToData(ROI roi_display, ref ROI roi_dist)
        {
            int image_width = FLIM_ImgData.width;
            int image_height = FLIM_ImgData.height;
            if (subPanel[0] > 1)
            {
                image_height = image_height / FLIM_ImgData.State.Acq.nSplitScanning;

            }
            else
            {
                int nPixels = Math.Max(FLIM_ImgData.width, FLIM_ImgData.height);
                image_scale = (float)Image1.Width / (float)nPixels;

                float[] offset = new float[2];
                offset[0] = -(float)((nPixels - FLIM_ImgData.width) / 2);
                offset[1] = -(float)((nPixels - FLIM_ImgData.height) / 2);

                PointF shiftP = new PointF(offset[0], offset[1]);

                ROI newroi = roi_display.ScaleRoi(1 / image_scale);
                newroi.shiftRoi(shiftP);

                if (newroi.ROI_type == ROI.ROItype.PolyLine)
                {
                    newroi.GetSmoothCurve();
                    newroi.GetEqualDistanceCenters(polyLineRadius);
                }

                if (roi_dist == null)
                    roi_dist = newroi;
                else
                    copyROIVectors(newroi, ref roi_dist);
            }
        }

        public PointF convertPointFromDisplayToData(PointF P)
        {
            int nPixels = Math.Max(FLIM_ImgData.width, FLIM_ImgData.height);
            image_scale = (float)Image1.Width / (float)nPixels;

            float[] offset = new float[2];
            offset[0] = -(float)((nPixels - FLIM_ImgData.width) / 2);
            offset[1] = -(float)((nPixels - FLIM_ImgData.height) / 2);

            var Pnew = new PointF(P.X, P.Y);
            Pnew.X = Pnew.X / image_scale - offset[0];
            Pnew.Y = Pnew.Y / image_scale - offset[1];

            return Pnew;
        }

        public void convertROIFromDataToDisplay(ROI roi, ref ROI roi_display)
        {
            int nPixels = Math.Max(FLIM_ImgData.width, FLIM_ImgData.height);
            image_scale = (float)Image1.Width / (float)nPixels;

            float[] offset = new float[2];
            offset[0] = (float)((nPixels - FLIM_ImgData.width) / 2);
            offset[1] = (float)((nPixels - FLIM_ImgData.height) / 2);

            var roi1 = new ROI(roi);
            roi1.shiftRoi(new PointF(offset[0], offset[1]));

            ROI newroi = roi1.ScaleRoi(image_scale);

            //Should be included in ScaleRoi.
            //if (newroi.ROI_type == ROI.ROItype.PolyLine)
            //{
            //    newroi.GetSmoothCurve();
            //    newroi.GetEqualDistanceCenters(polyLineRadius * image_scale);
            //}

            if (roi_display == null)
                roi_display = newroi;
            else
                copyROIVectors(newroi, ref roi_display);
            return;
        }

        public void ImageRoiToFLIMage()
        {
            convertROIfromDisplayToData(imageRoi, ref FLIM_ImgData.Roi);

            if (imageRoi.Rect.Width == 0 || !FLIM_ImgData.SetRoi(FLIM_ImgData.Roi))
            {
                imageRoi = new ROI();
                FLIM_ImgData.ResetRoi(false);
                roi_State = drawROI_State.NoROI;
            }
        }

        private void RemoveRoi_MenuItemClick(object sender, EventArgs e)
        {
            FLIM_ImgData.removeCurrentRoi();
            SetFitting_Param(true);
            DrawImages();
            FLIM_ImgData.ResetRoi(false);
            roi_State = drawROI_State.NoROI;
        }

        private void CreateRoi_MenuItemClick(object sender, EventArgs e)
        {
            //if (imageRoi.ROI_type == ROI.ROItype.PolyLine)
            //    RemoveAllROIs(sender, e);

            FLIM_ImgData.addCurrentRoi(polyLineRadius);
            imageRoi.GetEqualDistanceCenters(polyLineRadius * image_scale);
            SetFitting_Param(true);
            DrawImages();
            SaveAllRois();
            roi_State = drawROI_State.Idle;
        }


        //ROI till here.
        ///////////////////////////////////////
        ///////////////////////////////////////
        ///////////////////////////////////////
        //Uncaging from here.

        private void removeUncagingEach_Click(object sender, EventArgs e)
        {
            if (currentUncaging < uncagingLocs.Count && currentUncaging >= 0)
                uncagingLocs.RemoveAt(currentUncaging);
            flimage.UpdateUncagingFromDisplay();
        }

        private void CreateUncagingLocDirectly_Click(object sender, EventArgs e)
        {
            uLoc = uncagingLoc;
            uncagingLocFrac = ConvertPointFromDisplayToReal(uLoc);
            uncagingLocs.Add((double[])uncagingLocFrac.Clone());
            flimage.UpdateUncagingFromDisplay();
        }


        public void ActivateUncaging(bool do_everything)
        {
            UncagePosStartFunc USF = new UncagePosStartFunc(() => { Activate_uncaging(true, do_everything); });
            this.Invoke(USF);
        }

        public void Activate_uncaging(bool activate, bool everything)
        {
            //if (!flimage.flimage_io.use_nidaq)
            //    activate = false;

            if (activate)
            {
                if (everything)
                {
                    ToolPanelClicked(UncagingBox, new EventArgs());
                    ROItype = ROI.ROItype.Point;
                    drawUncagingPos = UncagingCursor.Idle;
                    roi_State = drawROI_State.Inactive;
                    Image1.Cursor = Cursors.Cross;
                }
            }
            else
            {
                if (ROItype == ROI.ROItype.Point)
                {
                    ToolPanelClicked(Square_Box, new EventArgs());
                    ROItype = ROI.ROItype.Rectangle;
                }
                drawUncagingPos = UncagingCursor.Inactive;
                roi_State = drawROI_State.NoROI;
                Image1.Cursor = Cursors.Default;
            }

            UncagingBox.Visible = activate;
            uncaging_on = activate;
            setUncagingPositionToolStripMenuItem.Checked = uncaging_on;

            Refresh();
        }


        private void CreateUncagingLoc_Click(object sender, EventArgs e)
        {
            uncagingLocs.Add((double[])uncagingLocFrac.Clone());
            flimage.UpdateUncagingFromDisplay();
            DrawImages();
        }

        private void RemoveAllUncaging_Click(object sender, EventArgs e)
        {
            uncagingLocs.Clear();
            flimage.UpdateUncagingFromDisplay();
            DrawImages();
        }

        //Uncaging till here.
        ///////////////////////////////////////
        ///////////////////////////////////////
        ///////////////////////////////////////
        //Roi movement handling.

        private RectangleF GetRectangle()
        {
            PointF P1 = currentPos_FLIM;
            int Width = Image1.Width;
            int Height = Image1.Height;

            if (P1.X < 0)
                P1.X = 0;
            if (P1.Y < 0)
                P1.Y = 0;
            if (P1.X > Width - 1)
                P1.X = Width - 1;
            if (P1.Y > Height - 1)
                P1.Y = Height - 1;


            float roiLeft, roiTop, roiWidth, roiHeight;
            if (roi_State == drawROI_State.Moving || roi_State == drawROI_State.Moving_ExistingROI)
            {
                roiWidth = imageRoi.Rect.Width;
                roiHeight = imageRoi.Rect.Height;
                roiLeft = P1.X + diffPos_FLIM.X;  //(P1.X - startPos_FLIM.X) + boxRoi_FLIM.Left;
                roiTop = P1.Y + diffPos_FLIM.Y;  //(P1.Y - startPos_FLIM.Y) + boxRoi_FLIM.Top;
            }
            else if (roi_State == drawROI_State.Resizing || roi_State == drawROI_State.Resizing_ExistingROI)
            {
                float[] X1 = (float[])imageRoi.X.Clone();
                float[] Y1 = (float[])imageRoi.Y.Clone();
                X1[drag_ROI_whichCorner] = P1.X;
                Y1[drag_ROI_whichCorner] = P1.Y;

                if (imageRoi.ROI_type == ROI.ROItype.Rectangle)
                {
                    if (drag_ROI_whichCorner == 0)
                    {
                        X1[3] = P1.X;
                        Y1[1] = P1.Y;
                    }
                    else if (drag_ROI_whichCorner == 1)
                    {
                        X1[2] = P1.X;
                        Y1[0] = P1.Y;
                    }
                    else if (drag_ROI_whichCorner == 2)
                    {
                        X1[1] = P1.X;
                        Y1[3] = P1.Y;
                    }
                    else if (drag_ROI_whichCorner == 3)
                    {
                        X1[0] = P1.X;
                        Y1[2] = P1.Y;
                    }
                }

                roiLeft = X1.Min();
                roiTop = Y1.Min();
                roiWidth = (X1.Max() - X1.Min());
                roiHeight = (Y1.Max() - Y1.Min());
            }
            else
            {
                roiLeft = Math.Min(startPos_FLIM.X, P1.X);
                roiTop = Math.Min(startPos_FLIM.Y, P1.Y);
                roiWidth = Math.Abs(startPos_FLIM.X - P1.X);
                roiHeight = Math.Abs(startPos_FLIM.Y - P1.Y);
            }

            if (roiLeft < 0)
                roiLeft = 0;
            if (roiTop < 0)
                roiTop = 0;
            if (roiLeft + roiWidth > Width)
                roiLeft = Width - roiWidth;
            if (roiTop + roiHeight > Height)
                roiTop = Height - roiHeight;

            return new RectangleF(roiLeft, roiTop, roiWidth, roiHeight);
        }

        private void Image1_DoubleClick(object sender, EventArgs e)
        {
            if (drawUncagingPos != UncagingCursor.Inactive)
                drawUncagingPos = UncagingCursor.Doubleclicked;

            if (roi_State == drawROI_State.Creating
                && (imageRoi.ROI_type == ROI.ROItype.Polygon || imageRoi.ROI_type == ROI.ROItype.PolyLine))
            {
                roi_State = drawROI_State.Idle;

                if (ROItype.Equals(ROI.ROItype.Polygon))
                {
                    imageRoiOld.X[imageRoiOld.X.Length - 1] = imageRoiOld.X[0];
                    imageRoiOld.Y[imageRoiOld.Y.Length - 1] = imageRoiOld.Y[0];
                }
                else
                {
                    Array.Resize(ref imageRoiOld.X, imageRoiOld.X.Length - 1);
                    Array.Resize(ref imageRoiOld.Y, imageRoiOld.Y.Length - 1);
                }


                imageRoi = new ROI(ROItype, imageRoiOld.X, imageRoiOld.Y, FLIM_ImgData.nChannels, polyLineRadius * image_scale, 0, imageRoiOld.Roi3d, imageRoiOld.Z);
                //StatusText.Text = String.Format("{0}, {1}", imageRoiOld.Points[0].X, imageRoiOld.Points[1].Y);
                convertROIfromDisplayToData(imageRoiOld, ref FLIM_ImgData.Roi);
                //FLIM_ImgData.Roi = new ROI(ROItype, roi1.X, roi1.Y, FLIM_ImgData.nChannels, polyLineRadius, 0, roi1.Roi3d, roi1.Z);

            }
            else if (roi_State != drawROI_State.Inactive)
            {
                //FLIM_ImgData.Fit_type = FLIMData.FitType.WholeImage;
                imageRoi = new ROI();
                roi_State = drawROI_State.NoROI;
            }

            if (FLIM_ImgData.Fit_type.Equals(FLIMData.FitType.GlobalRois))
            {
                UpdateImages(false, realtime, focusing, true);
            }
            else
            {
                SetFitting_Param(true);
                FLIM_ImgData.ResetLifetimeCalculation(true);
                //ResetFitting(true);
                UpdateImages(false, realtime, focusing, true);
            }

            FLIM_ImgData.currentRoi = -1;
        }

        private void Image1_MouseDown(object sender, MouseEventArgs e)
        {
            if (drawUncagingPos == UncagingCursor.Idle)
            {
                if (e.Button == MouseButtons.Left)
                {
                    bool found = false;
                    for (int i = 0; i < uncagingLocs.Count; i++)
                    {
                        if (WithInMark(e.Location, uncagingLocs[i]))
                        {
                            currentUncaging = i;
                            drawUncagingPos = UncagingCursor.Moving; //moveUncaging = true;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        drawUncagingPos = UncagingCursor.Creating;
                        currentUncaging = -1;
                    }
                }
                roi_State = drawROI_State.Inactive;
                return;
            }


            //ROI.
            drawROI_State roi_state_preClick = roi_State;
            int x = e.Location.X;
            int y = e.Location.Y;
            bool foundRoi = false;
            bool inside_current = false;

            if (roi_state_preClick == drawROI_State.Idle || roi_state_preClick == drawROI_State.NoROI)
            {
                PointF p1 = convertPointFromDisplayToData((PointF)e.Location);
                int nROI = FLIM_ImgData.ROIs.Count;

                //Background.
                float dist;
                bool inside = FLIM_ImgData.bgRoi.IsInsideRoi(p1);
                bool corner = false;
                if (FLIM_ImgData.currentRoi == -2)
                    corner = FLIM_ImgData.bgRoi.isNearCornerOfRoi(p1, handleSize, out drag_ROI_whichCorner, out dist);

                if (inside || corner)
                {
                    FLIM_ImgData.currentRoi = -2;
                    //FLIM_ImgData.Roi = FLIM_ImgData.bgRoi;
                    copyROIVectors(FLIM_ImgData.bgRoi, ref FLIM_ImgData.Roi);
                    CurrentROIBeforeMove = FLIM_ImgData.bgRoi.CopyROI(-2);
                    convertROIFromDataToDisplay(FLIM_ImgData.bgRoi, ref imageRoi);
                    foundRoi = true;
                }


                float min_dist = float.MaxValue;
                if (!foundRoi && nROI > 0)
                {
                    int current_roi = -1;
                    for (int i = 0; i < nROI; i++)
                    {
                        ROI roi1 = FLIM_ImgData.ROIs[i];
                        inside = roi1.IsInsideRoi(p1);
                        corner = roi1.isNearCornerOfRoi(p1, handleSize, out drag_ROI_whichCorner, out dist);

                        if (FLIM_ImgData.currentRoi != i)
                        {
                            corner = false;
                        }

                        if (inside || corner)
                        {
                            foundRoi = true;
                            if (min_dist > dist)
                            {
                                min_dist = dist;
                                current_roi = i;
                                inside_current = inside;
                            }
                        }
                    } //For nROI

                    if (foundRoi)
                    {
                        FLIM_ImgData.currentRoi = current_roi;
                        //FLIM_ImgData.Roi = FLIM_ImgData.ROIs[current_roi];
                        copyROIVectors(FLIM_ImgData.ROIs[current_roi], ref FLIM_ImgData.Roi); // not work??
                        CurrentROIBeforeMove = new ROI(FLIM_ImgData.ROIs[current_roi]);
                        convertROIFromDataToDisplay(FLIM_ImgData.ROIs[current_roi], ref imageRoi); //roi1
                    }
                }


                if (foundRoi)
                {
                    if (inside_current)
                        roi_State = drawROI_State.Moving_ExistingROI;
                    else
                        roi_State = drawROI_State.Resizing_ExistingROI;
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                if (roi_state_preClick == drawROI_State.Idle || roi_state_preClick == drawROI_State.NoROI)
                {
                    if (imageRoi.IsInsideRoi(e.Location))
                    {
                        if (!foundRoi)
                        {
                            setSizeOfThisROIToolStripMenuItem.Visible = ROItype == ROI.ROItype.Elipsoid;
                            rightClickMenuStrip.Show((PictureBox)sender, e.Location);//places the menu at the pointer position
                        }
                        else
                        {
                            setRadiusOfThisROIToolStripMenuItem.Visible = ROItype == ROI.ROItype.Elipsoid;
                            rightClickMenuStrip_inROI.Show((PictureBox)sender, e.Location);
                        }

                    }
                    else
                    {
                        rightClickMenu_removeAll.Show((PictureBox)sender, e.Location);
                        return;
                    }

                    if (foundRoi)
                        return; //??
                }
                else
                {
                    rightClickMenu_removeAll.Show((PictureBox)sender, e.Location);
                }
            }
            else //Left click
            {
                if (roi_state_preClick == drawROI_State.Idle || roi_state_preClick == drawROI_State.NoROI)
                {
                    if (ThreeDRoi && (ZStack || FastZStack) && !displayZProjection && FLIM_ImgData.currentPage < FLIM_ImgData.n_pages && FLIM_ImgData.currentPage >= 0)
                        RoiZ = new int[] { FLIM_ImgData.currentPage };

                    var handleSizeImage = handleSize * image_scale;
                    if (imageRoi.isEditable() && imageRoi.isNearCornerOfRoi(e.Location, handleSizeImage, out drag_ROI_whichCorner, out float dist))
                    {
                        diffPos_FLIM = new PointF();
                        diffPos_FLIM.X = imageRoi.Rect.Left - x;
                        diffPos_FLIM.Y = imageRoi.Rect.Top - y;

                        if (foundRoi)
                            roi_State = drawROI_State.Resizing_ExistingROI;
                        else
                        {
                            roi_State = drawROI_State.Resizing;
                            FLIM_ImgData.currentRoi = -1;
                        }
                    }
                    else if (imageRoi.IsInsideRoi(e.Location))
                    {
                        diffPos_FLIM = new Point();
                        diffPos_FLIM.X = imageRoi.Rect.Left - x;
                        diffPos_FLIM.Y = imageRoi.Rect.Top - y;

                        if (foundRoi)
                            roi_State = drawROI_State.Moving_ExistingROI;
                        else
                        {
                            roi_State = drawROI_State.Moving;
                            FLIM_ImgData.currentRoi = -1;
                        }
                    }
                    else
                    {
                        roi_State = drawROI_State.Creating;
                    }
                }



                if ((ROItype.Equals(ROI.ROItype.Polygon) || ROItype.Equals(ROI.ROItype.PolyLine)) && roi_State == drawROI_State.Creating)
                {
                    if (roi_state_preClick != drawROI_State.Creating)
                    {
                        imageRoi = new ROI(ROItype, new float[] { e.X }, new float[] { e.Y }, FLIM_ImgData.nChannels, polyLineRadius * image_scale, 0, ThreeDRoi, RoiZ);
                        imageRoiOld = new ROI(ROItype, new float[] { e.X }, new float[] { e.Y }, FLIM_ImgData.nChannels, polyLineRadius * image_scale, 0, ThreeDRoi, RoiZ);
                    }
                    else
                    {
                        imageRoi = imageRoiOld.addPoints(e.Location);
                        imageRoiOld = imageRoiOld.addPoints(e.Location);
                    }
                }

                if (roi_State != drawROI_State.Inactive)
                {
                    imageRoiSave = new ROI(imageRoi);
                    startPos_FLIM = e.Location;
                }
            }
        }

        private void Image1_MouseMove(object sender, MouseEventArgs e)
        {

            if (roi_State != drawROI_State.Idle && roi_State != drawROI_State.NoROI && roi_State != drawROI_State.Inactive)
            {
                currentPos_FLIM = e.Location;
                var rc = GetRectangle();
                //Debug.WriteLine("Left = " + rc.Left + " Right = " + rc.Right);
                if (roi_State == drawROI_State.Moving || roi_State == drawROI_State.Moving_ExistingROI)
                {
                    PointF shiftP = new PointF(rc.Left - imageRoi.Rect.Left, rc.Top - imageRoi.Rect.Top);
                    imageRoi.shiftRoi(shiftP);
                    imageRoi = new ROI(imageRoi.ROI_type, imageRoi.X, imageRoi.Y, FLIM_ImgData.nChannels, polyLineRadius * image_scale
                        , 0, imageRoi.Roi3d, (int[])imageRoi.Z.Clone());
                }
                else if (roi_State == drawROI_State.Resizing || roi_State == drawROI_State.Resizing_ExistingROI)
                {
                    if (imageRoi.ROI_type.Equals(ROI.ROItype.Rectangle))
                        imageRoi = new ROI(imageRoi.ROI_type, rc, FLIM_ImgData.nChannels, 0, imageRoi.Roi3d, imageRoi.Z);
                    else if (imageRoi.ROI_type.Equals(ROI.ROItype.Elipsoid))
                        imageRoi = imageRoi.changeSizeOfCircle(currentPos_FLIM, startPos_FLIM, imageRoiSave);
                    else
                    {
                        imageRoi = imageRoi.MovePoint(currentPos_FLIM, drag_ROI_whichCorner);

                    }
                }
                else //creating new ROI
                {
                    if (ROItype.Equals(ROI.ROItype.Rectangle) || ROItype.Equals(ROI.ROItype.Elipsoid))
                        imageRoi = new ROI(ROItype, rc, FLIM_ImgData.nChannels, 0, imageRoi.Roi3d, imageRoi.Z);
                    else
                        imageRoi = imageRoiOld.addPoints(e.Location);
                }

                DrawImages();
            }

            if (drawUncagingPos == UncagingCursor.Moving || drawUncagingPos == UncagingCursor.Creating)
            {
                if (drawUncagingPos == UncagingCursor.Moving && currentUncaging >= 0 && currentUncaging < uncagingLocs.Count)
                {
                    uncagingLocs[currentUncaging] = ConvertPointFromDisplayToReal(e.Location);
                }
                //moveMouseEvent.Restart();
                uncagingLoc = e.Location;
                if (!realtime)
                    DrawImages();
            }
        }

        private bool IfROIMoved(ROI roi_new, ROI roi_old)
        {
            if (roi_new.X.Length != roi_old.X.Length || roi_new.Y.Length != roi_old.Y.Length)
                return true;
            else
            {
                for (int i = 0; i < roi_new.X.Length; i++)
                {
                    if ((int)(10 * Math.Abs(roi_new.X[i] - roi_old.X[i])) > 1) //Up to 1/10 pixels
                        return true;
                }


                for (int i = 0; i < roi_new.X.Length; i++)
                {
                    if ((int)(10 * Math.Abs(roi_new.Y[i] - roi_old.Y[i])) > 1) //Up to 1/10 pixels
                        return true;
                }
            }
            return false;
        }

        private void Image1_MouseUp(object sender, MouseEventArgs e)
        {
            var preMouseUp_State = roi_State;

            if (roi_State != drawROI_State.Idle && roi_State != drawROI_State.NoROI && roi_State != drawROI_State.Inactive)
            {
                currentPos_FLIM = e.Location;
                bool moved = false;
                var rc = GetRectangle();

                //If polygon or polyline, it will continue.
                if ((imageRoi.ROI_type.Equals(ROI.ROItype.Polygon) || imageRoi.ROI_type.Equals(ROI.ROItype.PolyLine))
                    && preMouseUp_State == drawROI_State.Creating)
                    roi_State = drawROI_State.Creating; //Continue until double clik.
                else
                    roi_State = drawROI_State.Idle;

                if (ThreeDRoi && (ZStack || FastZStack))
                {
                    imageRoi.Roi3d = ThreeDRoi;
                    if (!displayZProjection)
                        imageRoi.Z = new int[] { FLIM_ImgData.currentPage };
                }

                if (rc.Width > 0 && rc.Height > 0)
                {
                    ROI roi0 = new ROI(imageRoi.ROI_type, rc, FLIM_ImgData.nChannels, 0, imageRoi.Roi3d, imageRoi.Z);

                    if (preMouseUp_State != drawROI_State.Creating)
                    {
                        if (preMouseUp_State == drawROI_State.Moving || preMouseUp_State == drawROI_State.Moving_ExistingROI)
                        {
                            imageRoi.shiftRoi(new PointF(roi0.Rect.Left - imageRoi.Rect.Left, roi0.Rect.Top - imageRoi.Rect.Top));
                            imageRoi = new ROI(imageRoi.ROI_type, imageRoi.X, imageRoi.Y, FLIM_ImgData.nChannels, polyLineRadius * image_scale
                                , FLIM_ImgData.currentRoi, imageRoi.Roi3d, (int[])imageRoi.Z.Clone());
                        }
                        else if (preMouseUp_State == drawROI_State.Resizing || preMouseUp_State == drawROI_State.Resizing_ExistingROI)
                        {
                            if (imageRoi.ROI_type.Equals(ROI.ROItype.Rectangle))
                                imageRoi = roi0;
                            else if (imageRoi.ROI_type.Equals(ROI.ROItype.Elipsoid))
                                imageRoi = imageRoi.changeSizeOfCircle(currentPos_FLIM, startPos_FLIM, imageRoiSave);
                            else
                                imageRoi = imageRoi.MovePoint(currentPos_FLIM, drag_ROI_whichCorner);
                        }

                        moved = IfROIMoved(imageRoi, imageRoiSave);


                        if (moved)
                        {
                            convertROIfromDisplayToData(imageRoi, ref FLIM_ImgData.Roi);

                            if (preMouseUp_State == drawROI_State.Resizing_ExistingROI || preMouseUp_State == drawROI_State.Moving_ExistingROI)
                            {
                                if (FLIM_ImgData.currentRoi == -2) //Background.
                                {
                                    convertROIfromDisplayToData(imageRoi, ref FLIM_ImgData.bgRoi);
                                    FLIM_ImgData.bgRoi.ID = -2;
                                }
                                if (FLIM_ImgData.currentRoi >= 0 && FLIM_ImgData.currentRoi < FLIM_ImgData.ROIs.Count)
                                {
                                    //convertROIfromDisplayToData(imageRoi, ref FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi]);
                                    var roi = FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi];
                                    convertROIfromDisplayToData(imageRoi, ref roi);
                                    FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi] = roi;
                                }
                            }

                            if (preMouseUp_State == drawROI_State.Moving_ExistingROI && FLIM_ImgData.currentRoi >= 0)
                            {
                                float shiftX = -CurrentROIBeforeMove.Rect.Location.X + FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].Rect.Location.X;
                                float shiftY = -CurrentROIBeforeMove.Rect.Location.Y + FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].Rect.Location.Y;
                                if (Control.ModifierKeys == Keys.Shift)
                                {
                                    PointF shiftP2 = new PointF(shiftX, shiftY);
                                    for (int i = 0; i < FLIM_ImgData.ROIs.Count; i++)
                                    {
                                        if (i != FLIM_ImgData.currentRoi)
                                            FLIM_ImgData.ROIs[i].shiftRoi(shiftP2);
                                    }
                                }
                            }
                            else if (preMouseUp_State == drawROI_State.Resizing_ExistingROI)
                            {
                                //
                            }
                            else
                            {
                                FLIM_ImgData.currentRoi = -1;
                            }

                            SaveAllRois();
                        }
                    }
                    else //not moving. Creating.
                    {
                        if (ROItype.Equals(ROI.ROItype.Rectangle) || ROItype.Equals(ROI.ROItype.Elipsoid))
                            imageRoi = new ROI(ROItype, rc, FLIM_ImgData.nChannels, 0, ThreeDRoi, RoiZ);

                        convertROIfromDisplayToData(imageRoi, ref FLIM_ImgData.Roi);
                        //imageRoi.ScaleRoi((double)FLIM_ImgData.width / (double)Image1.Width, calculateImageOffset());

                        FLIM_ImgData.ResetLifetimeCalculation(false);

                        FLIM_ImgData.currentRoi = -1;
                    }

                    if (FLIM_ImgData.Roi.Rect.Width == 0 || FLIM_ImgData.Roi.Rect.Height == 0)
                        FLIM_ImgData.ResetRoi(false);


                    //if not moved.... Just update the fitting parameters?
                    if (FLIM_ImgData.Fit_type.Equals(FLIMData.FitType.GlobalRois) && !moved)
                    {
                        if (FLIM_ImgData.Fit_type.Equals(FLIMData.FitType.GlobalRois) && Values_selectedROI.Checked)
                        {
                            UpdateFittingParam(currentChannel, fittingDone);
                            if (image_display_state != ImageDisplay_State.Opening)
                                UpdateImages(true, realtime, focusing, true);
                        }
                    }
                    else if (moved)
                    {
                        FLIM_ImgData.ResetLifetimeCalculation(false);
                        fittingDone = false;
                        UpdateFittingParam(currentChannel, fittingDone);
                    }

                    if (image_display_state != ImageDisplay_State.Opening)
                        UpdateImages(true, realtime, focusing, true);
                }
                //drawImages();
            } ///drawingROI


            if (drawUncagingPos != UncagingCursor.Inactive)
            {
                if (drawUncagingPos == UncagingCursor.Moving || drawUncagingPos == UncagingCursor.Creating)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        uncagingLoc = e.Location;
                        uLoc = uncagingLoc;
                        uncagingLocFrac = ConvertPointFromDisplayToReal(uLoc);
                        flimage.UpdateUncagingFromDisplay();
                        referenceLoc = new Point(-1, -1);
                        flimage.flimage_io.uncaging_Calib = new double[] { 0.0, 0.0 };

                        if (currentUncaging >= 0 && currentUncaging < uncagingLocs.Count)
                        {
                            uncagingLocs[currentUncaging] = (double[])uncagingLocFrac.Clone();
                        }
                        drawUncagingPos = UncagingCursor.Idle;
                    }
                }

                if (e.Button == MouseButtons.Right)
                {
                    if (calib_on) //Right click to calibrate.
                    {
                        drawUncagingPos = UncagingCursor.Idle;

                        referenceLoc = e.Location;
                        double[] referenceLocFrac = new double[] { referenceLoc.X / (double)Image1.Width, referenceLoc.Y / (double)Image1.Height };
                        double[] calibratedLocationV = HardwareControls.IOControls.PositionFracToVoltage(referenceLocFrac, flimage.State);
                        double[] originalLocationV = HardwareControls.IOControls.PositionFracToVoltage(uncagingLocFrac, flimage.State);

                        for (int i = 0; i < 2; i++)
                            flimage.flimage_io.uncaging_Calib[i] = calibratedLocationV[i] - originalLocationV[i];

                        flimage.UpdateUncagingFromDisplay();
                    }
                    else
                    {
                        if (WithInMark(e.Location, uncagingLocFrac)) //Cliked on the mark.
                        {
                            rightClick_CreateUncaging.Show((PictureBox)sender, e.Location);
                        }
                        else
                        {
                            bool found = false;
                            for (int i = 0; i < uncagingLocs.Count; i++)
                            {
                                if (WithInMark(e.Location, uncagingLocs[i]))
                                {
                                    currentUncaging = i;
                                    found = true;
                                    rightClick_remUncageEach.Show((PictureBox)sender, e.Location);
                                    break;
                                }
                            }

                            if (!found)
                                rightClick_removeUncaging.Show((PictureBox)sender, e.Location);

                        }

                    }
                }

                if (drawUncagingPos == UncagingCursor.Doubleclicked)
                {
                    drawUncagingPos = UncagingCursor.Idle;
                    uncagingLocFrac = new double[] { -1.0, -1.0 };
                    referenceLoc = new Point(-1, -1);
                    //                Debug.WriteLine("Double clicked!");
                    uncagingLoc = e.Location;
                }
            }


            if (!realtime && image_display_state != ImageDisplay_State.Opening)
            {
                DrawImages();
                plot_regular.plotNow_noRealtime(TCF, TC, this, currentChannel);
            }

        }

        private bool WithInMark(Point MouseLocation, double[] uloc)
        {
            PointF targetLocation = ConvertFractionFromRealToDisplay(uloc);
            int widthOfMark = (int)(Image1.Width * sizeOfMark);
            return MouseLocation.X <= targetLocation.X + widthOfMark &&
                MouseLocation.X > targetLocation.X - widthOfMark &&
                MouseLocation.Y <= targetLocation.Y + widthOfMark &&
                MouseLocation.Y > targetLocation.Y - widthOfMark;
        }

        //ROi movement handling Done.
        ///////////////////////////////
        ///////////////////////////////
        ///////////////////////////////
        ///
        private void Fix_Check_Changed(object sender, EventArgs e)
        {
            if (sender.Equals(SelectRoi) || sender.Equals(AllRois))
                FLIM_ImgData.ResetLifetimeCalculation(true);
            SetFitting_Param(true);
            UpdateImages(true, realtime, focusing, true);
        }

        private void TurnOnMultiROI(bool On)
        {
            //
            this.InvokeIfRequired(o =>
            {
                var saveAllRois = o.AllRois.Checked;
                o.AllRois.Checked = On;
                o.SelectRoi.Checked = !On;
                if (saveAllRois != o.AllRois.Checked)
                {
                    FLIM_ImgData.ResetLifetimeCalculation(true);
                    o.SetFitting_Param(true);
                }
            });
        }

        public void UpdateFileName()
        {
            FileN.Text = Convert.ToString(FLIM_ImgData.fileCounter);
            BaseName.Text = FLIM_ImgData.baseName;
            //FileName.Text = FLIM_ImgData.fullName(currentChannel);
            Text = "FLIM Analysis: " + FLIM_ImgData.fullFileName;
            //FileName.Select(FileName.TextLength + 1, 0);
        }

        public void ReadRois(bool update_selectROI)
        {
            String fileName = RoiFileName(false);
            int roiID = 0;
            if (File.Exists(fileName))
            {
                using (StreamReader sr = File.OpenText(fileName))
                {
                    int mode = 0;
                    string s = "";
                    ROI roi = new ROI();

                    while ((s = sr.ReadLine()) != null)
                    {

                        if (s.Contains("SelectROI"))
                        {
                            mode = 1;
                            //roi = FLIM_ImgData.Roi;
                        }
                        else if (s.Contains("MultiROI"))
                        {
                            mode = 2;
                            FLIM_ImgData.ROIs.Clear();
                        }
                        else if (s.Contains("bgROI"))
                        {
                            mode = 3;

                        }

                        if (s.Contains("ROI-type"))
                        {
                            roi = new ROI();
                            String[] sp = Enum.GetNames(typeof(ROI.ROItype));
                            String[] sp2 = s.Split(',');
                            for (int i = 0; i < sp.Length; i++)
                            {
                                for (int j = 0; j < sp2.Length; j++)
                                    if (sp[i] == sp2[j])
                                    {
                                        Enum.TryParse(sp[i], out roi.ROI_type);
                                        break;
                                    }
                            }
                        }

                        if (s.Contains("polyLineRadius"))
                        {
                            String[] sP = s.Split(',');
                            for (int i = 0; i < sP.Length; i++)
                            {
                                if (sP[i].Contains("polyLineRadius"))
                                {
                                    polyLineRadius = Convert.ToSingle(sP[i + 1]);
                                    break;
                                }
                            }
                        }

                        if (s.Contains("Roi-ID"))
                        {
                            String[] sP = s.Split(',');
                            for (int i = 0; i < sP.Length; i++)
                            {
                                if (sP[i].Contains("Roi-ID"))
                                {
                                    roiID = Convert.ToInt32(sP[i + 1]);
                                    break;
                                }
                            }
                        }

                        String[] pS = s.Split(',');
                        if (pS[0].Equals("Rect") || pS[0].Equals("X") || pS[0].Equals("Y") || pS[0].Equals("Z"))
                        {
                            float[] val = new float[pS.Length - 1];
                            if (pS.Length > 1)
                            {
                                bool final = false;
                                for (int i = 1; i < pS.Length; i++)
                                    val[i - 1] = Convert.ToSingle(pS[i]);

                                if (pS[0].Equals("X"))
                                    roi.X = val;

                                else if (pS[0].Equals("Y"))
                                {
                                    roi.Y = val;
                                    roi = new ROI(roi.ROI_type, roi.X, roi.Y, FLIM_ImgData.nChannels, polyLineRadius, 0, roi.Roi3d, roi.Z);
                                    final = true;
                                }

                                else if (pS[0].Equals("Rect"))
                                {
                                    roi.Rect = new RectangleF(val[0], val[1], val[2], val[3]);
                                    roi = new ROI(roi.ROI_type, roi.Rect, FLIM_ImgData.nChannels, 0, roi.Roi3d, roi.Z);
                                    final = true;
                                }

                                else if (pS[0].Equals("Z")) //must put before.
                                {
                                    roi.Roi3d = true;
                                    roi.Z = val.Select(x => (int)x).ToArray();
                                }

                                if (final)
                                {
                                    roi.ID = roiID;
                                    if (mode == 1)
                                    {
                                        if (update_selectROI)
                                        {
                                            if (roi.Rect.Left >= 0 && roi.Rect.Top >= 0 && roi.Rect.Right <= FLIM_ImgData.width && roi.Rect.Bottom <= FLIM_ImgData.height)
                                            {
                                                FLIM_ImgData.Roi = roi;
                                                convertROIFromDataToDisplay(FLIM_ImgData.Roi, ref imageRoi);
                                            }
                                        }
                                    }
                                    else if (mode == 2) //MultiROI
                                    {
                                        if (roi.Rect.Left >= 0 && roi.Rect.Top >= 0 && roi.Rect.Right <= FLIM_ImgData.width && roi.Rect.Bottom <= FLIM_ImgData.height)
                                        {
                                            if (roiID == 0)
                                            {
                                                FLIM_ImgData.addToMultiRoi(roi);
                                            }
                                            else
                                            {
                                                FLIM_ImgData.ROIs.Add(roi);
                                                FLIM_ImgData.currentRoi = FLIM_ImgData.ROIs.Count - 1;
                                            }
                                        }
                                    }
                                    else if (mode == 3)
                                    {
                                        if (roi.Rect.Left >= 0 && roi.Rect.Top >= 0 && roi.Rect.Right <= FLIM_ImgData.width && roi.Rect.Bottom <= FLIM_ImgData.height)
                                        {
                                            FLIM_ImgData.bgRoi = roi;
                                        }
                                    }
                                }
                            } // ps.Lnegth > 1
                        }

                    }
                    sr.Close();
                }
            } // file exist
            else
            {
                Debug.WriteLine("Could not find ROI file!");
                //MessageBox.Show("Could not find ROI file!");
            }
        }

        public void SaveLineAsExcel(double[] array, String Title, ref StringBuilder sb)
        {
            sb.Append(Title);
            sb.Append(",");
            if (array != null)
                for (int i = 0; i < array.Length; i++)
                {
                    sb.Append(array[i]);
                    if (i == array.Length)
                        sb.AppendLine();
                    else
                        sb.Append(",");
                }
            sb.AppendLine();
        }

        public void ReadTimeCourse()
        {
            String fileName = TimeCourseFileName();

            if (!File.Exists(fileName))
                return;

            String fn = Path.GetFileNameWithoutExtension(fileName);
            FormControllers.CloseOpenExcelWindow(fn);

            String[] lines = File.ReadAllLines(fileName);

            String TC_FileName = FLIM_ImgData.fileName.Split('.')[0];
            String TC_BaseName = FLIM_ImgData.baseName;
            double[] TC_time_milliseconds = new double[1];
            double[] TC_time_seconds;
            int TC_nROI = 0;

            List<TimeCourse> TC_List = new List<TimeCourse>();
            Dictionary<string, double[]> roi_ch_values = new Dictionary<string, double[]>();
            HashSet<int> unique_Roi = new HashSet<int>();
            HashSet<int> unique_Ch = new HashSet<int>();
            HashSet<string> unique_name = new HashSet<string>();
            int data_length = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                ReadLineFromExcel(lines[i], out String title, out double[] values);
                if (title == "Time (ms)")
                {
                    TC_time_milliseconds = values;
                    data_length = values.Length;
                }
                else if (title == "Time (s)")
                {
                    TC_time_seconds = values;
                }
                else if (title == "nROIs")
                    TC_nROI = (int)values[0];

                else if (title == "FileNumber")
                {
                    int curN = 0;
                    for (int j = 0; j < values.Length; j++)
                    {
                        if (j == 0 || values[j] != values[j - 1]) //no transition
                        {
                            curN = 0;
                            TC = new TimeCourse();
                            TC_List.Add(TC);
                            TC.nData = 1;

                            TC.BaseName = TC_BaseName;
                            TC.FileNumber = (int)values[j];

                            //TC.nROI = TC_nROI;
                            //TC.nChannels = FLIM_ImgData.nChannels;
                            //TC.initializeArray(TC.nChannels, TC.nROI);

                            var filename0 = String.Format("{0}{1:000}", FLIM_ImgData.baseName, TC.FileNumber);
                            filename0 = Path.Combine(FLIM_ImgData.pathName, filename0);

                            TC.FileName = filename0;
                        }
                        else
                        {
                            curN++;
                            TC.nData++;
                        }

                        if (TC.time_milliseconds == null)
                            TC.time_milliseconds = new double[curN + 1];
                        else if (TC.time_milliseconds.Length < curN + 1)
                            Array.Resize(ref TC.time_milliseconds, curN + 1);

                        TC.time_milliseconds[curN] = TC_time_milliseconds[j];
                        // TC.calculate();
                    }
                }

                var sP = title.Split('-');
                if (sP.Length == 2 && sP[1].Contains("ch"))
                {

                    int ch = Convert.ToInt32(sP[1].Substring(2)) - 1;

                    int offset = 0;

                    for (int j = 0; j < TC_List.Count; j++)
                    {
                        int len = TC_List[j].nData;
                        var dArray = new double[len];
                        Array.Copy(values, offset, dArray, 0, len);
                        offset += len;
                        string nameField = sP[0];
                        TC_List[j].SetArrayByName(nameField, ch, -1, dArray);
                    }
                }
                else if (sP.Length == 3 && sP[1].Contains("ROI") && sP[2].Contains("ch"))
                {

                    var nodata = values.All(x => x == 0);
                    if (!nodata)
                    {
                        int ch1 = Convert.ToInt32(sP[2].Substring(2)) - 1;
                        int roi1 = Convert.ToInt32(sP[1].Substring(3)) - 1;
                        string str = sP[0] + "-" + ch1 + "-" + roi1;
                        roi_ch_values.Add(str, values);
                        unique_Roi.Add(roi1);
                        unique_Ch.Add(ch1);
                        unique_name.Add(sP[0]);
                    }
                }
            } //line


            var nROI = unique_Roi.ToList().Count;
            var nCh = unique_Ch.ToList().Count;
            var nPara = unique_name.ToList().Count;

            for (int j = 0; j < TC_List.Count; j++)
            {
                TC_List[j].initializeArray(nCh, nROI);
                TC_List[j].UniqueIDs = unique_Roi;
                TC_List[j].nROI = nROI;
            }

            for (int s = 0; s < nPara; s++)
                for (int ch = 0; ch < nCh; ch++)
                    for (int roi = 0; roi < nROI; roi++)
                    {
                        int offset = 0;
                        var ch1 = unique_Ch.ToList()[ch];
                        var roi1 = unique_Roi.ToList()[roi];
                        var nameField = unique_name.ToList()[s];
                        string str = nameField + "-" + ch1 + "-" + roi1;
                        nameField += "_ROI";
                        double[] values = new double[data_length];

                        try
                        {
                            values = roi_ch_values[str];
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }

                        if (values != null)
                        {
                            offset = 0;
                            for (int j = 0; j < TC_List.Count; j++)
                            {
                                int len = TC_List[j].nData;
                                var dArray = new double[len];
                                Array.Copy(values, offset, dArray, 0, len);
                                TC_List[j].SetArrayByName(nameField, ch, roi, dArray);
                                offset += len;
                            }
                        }
                    }


            for (int j = 0; j < TC_List.Count; j++)
                TCF.AddFile(TC_List[j]);

            TCF.calculate();
            plot_regular.plotNow_noRealtime(TCF, TC, this, currentChannel);

        } //Read time course

        /// <summary>
        /// Interpret lines from CSV files (not Excel actually)
        /// </summary>
        /// <param name="line"></param>
        /// <param name="Title"></param>
        /// <param name="values"></param>
        public void ReadLineFromExcel(string line, out String Title, out double[] values)
        {
            var pS = line.Split(',');
            Title = pS[0];

            if (pS.Length > 1)
                values = new double[pS.Length - 1];
            else
            {
                values = null;
                return;
            }

            int k = 0;
            for (int i = 1; i < pS.Length; i++)
            {
                if (Double.TryParse(pS[i], out values[i - 1]))
                    k++;
            }
            if (k > 0)
                Array.Resize(ref values, k);
            else
                values = null;
        }

        /// <summary>
        /// Save time course in the CSV file.
        /// </summary>
        public void SaveTimeCourse()
        {
            var sb = new StringBuilder();

            if (TCF.meanIntensity == null)
                return;


            if (FLIM_ImgData.ROIs == null || FLIM_ImgData.ROIs.Count == 0)
            {
                sb.Append("Select-ROI");
                sb.AppendLine();
                SaveLineAsExcel(TCF.time_milliseconds, "Time (ms)", ref sb);
                SaveLineAsExcel(TCF.fileNumber, "FileNumber", ref sb);
                SaveLineAsExcel(TCF.time_seconds, "Time (s)", ref sb);
                for (int ch = 0; ch < FLIM_ImgData.nChannels; ++ch)
                {
                    foreach (var fieldName in ImageInfo.paramNames)
                    {
                        var arr1_ch = TCF.GetArrayByName(fieldName, ch, -1);
                        SaveLineAsExcel(arr1_ch, fieldName + "-ch" + (ch + 1), ref sb);
                    }
                }

                sb.AppendLine();
            }

            sb.Append("Multi-ROI");
            sb.AppendLine();
            SaveLineAsExcel(new double[] { TCF.nROI }, "nROIs", ref sb);
            SaveLineAsExcel(TCF.time_milliseconds, "Time (ms)", ref sb);
            SaveLineAsExcel(TCF.fileNumber, "FileNumber", ref sb);
            SaveLineAsExcel(TCF.time_seconds, "Time (s)", ref sb);

            foreach (var fieldName in ImageInfo.paramNames)
            {
                String fieldNameROI = fieldName + "_ROI";
                String fieldName2 = fieldName + "-ROI";

                for (int ch = 0; ch < TCF.nChannels; ++ch)
                {
                    for (int i = 0; i < TCF.nROI; i++)
                    {
                        var arr1_ch_roi = TCF.GetArrayByName(fieldNameROI, ch, i);
                        SaveLineAsExcel(arr1_ch_roi, fieldName2 + (TCF.UniqueIDs.ToList()[i] + 1) + "-ch" + (ch + 1), ref sb);
                    }
                    sb.AppendLine();
                }
            }

            string allStr = sb.ToString();

            CreateAanalysisFolder();
            String fileName = TimeCourseFileName();
            TrySave(fileName, allStr);
            SaveAllRois();
        }

        public bool RoiIDExist(int ID)
        {
            bool exist = false;

            foreach (var roi1 in FLIM_ImgData.ROIs)
            {
                if (ID == roi1.ID)
                    exist = true;

                if (roi1.ROI_type == ROI.ROItype.PolyLine)
                {
                    foreach (var roi2 in roi1.polyLineROIs)
                        if (ID == roi2.ID + 1000 * roi1.ID)
                            exist = true;
                }
            }
            return exist;
        }


        public void SaveAllRois()
        {
            var sb = new StringBuilder();

            sb.Append("SelectROI");
            sb.AppendLine();
            sb.Append(FormatROIPositionToExcel(FLIM_ImgData.Roi));

            sb.Append("bgROI");
            sb.AppendLine();
            sb.Append(FormatROIPositionToExcel(FLIM_ImgData.bgRoi));

            sb.Append("MultiROI");
            sb.AppendLine();
            for (int i = 0; i < FLIM_ImgData.ROIs.Count; i++)
                sb.Append(FormatROIPositionToExcel(FLIM_ImgData.ROIs[i]));

            string allStr = sb.ToString();
            CreateAanalysisFolder();
            String fileName = RoiFileName(false);
            TrySave(fileName, allStr);
        }

        private StringBuilder FormatROIPositionToExcel(ROI roi)
        {
            var sb = new StringBuilder();
            sb.Append("ROI-type,");

            sb.Append(roi.ROI_type.ToString());

            sb.Append(",");
            sb.Append("Roi-ID,");
            sb.Append(roi.ID);

            sb.Append(",");
            sb.Append("polyLineRadius,");
            sb.Append(polyLineRadius);

            sb.AppendLine();
            if (roi.Roi3d)
            {
                sb.Append("Z,");
                for (int z = 0; z < roi.Z.Length; z++)
                {
                    sb.Append(roi.Z[z]);
                    if (z != roi.Z.Length - 1)
                        sb.Append(",");
                }
                sb.AppendLine();
            }

            if (roi.ROI_type == ROI.ROItype.Elipsoid || roi.ROI_type == ROI.ROItype.Rectangle)
            {
                sb.Append("Rect,");
                sb.Append(roi.Rect.Left);
                sb.Append(",");
                sb.Append(roi.Rect.Top);
                sb.Append(",");
                sb.Append(roi.Rect.Width);
                sb.Append(",");
                sb.Append(roi.Rect.Height);
                sb.AppendLine();
            }
            else
            {
                int nPoints = roi.X.Length;
                //sb.AppendLine();
                sb.Append("X,");
                for (int i = 0; i < nPoints; i++)
                {
                    sb.Append(roi.X[i]);
                    if (i != nPoints - 1)
                        sb.Append(",");
                }
                sb.AppendLine();
                sb.Append("Y,");
                for (int i = 0; i < nPoints; i++)
                {
                    sb.Append(roi.Y[i]);
                    if (i != nPoints - 1)
                        sb.Append(",");
                }
                sb.AppendLine();
            }
            return sb;
        }

        private void SaveFittingData()
        {

            var sb = new StringBuilder();
            int p = FLIM_ImgData.RoiFit.flim_parameters.beta[0].Length;

            if (p == 4)
            {
                sb.Append("Sinlge exponential fitting");
                sb.AppendLine();
                sb.Append("Ch/ROI,Pop1,Tau1,TauG,Tau0,MeanTau,Frac1,Frac2,xi_square");
            }
            else
            {
                sb.Append("Double exponential fitting");
                sb.AppendLine();
                sb.Append("Ch/ROI,Pop1,Tau1,Pop2,Tau2,TauG,Tau0,Meantau,Frac1,Frac2,xi_square");
            }

            sb.AppendLine();
            double res = (double)FLIM_ImgData.psPerUnit / 1000.0;
            int nChs = FLIM_ImgData.nChannels;

            List<bool> FLIM_OK = new List<bool>();
            for (int ch = 0; ch < nChs; ch++)
                FLIM_OK.Add(FLIM_ImgData.FLIMRaw[ch] != null && FLIM_ImgData.State.Acq.acqFLIMA[ch]);

            for (int ch = 0; ch < nChs; ch++)
            {
                if (FLIM_OK[ch])
                {
                    int cName = ch + 1;
                    sb.Append("ch" + cName);

                    for (int x = 0; x < p; x++)
                    {
                        sb.Append(",");
                        double val = FLIM_ImgData.RoiFit.flim_parameters.beta[ch][x];
                        if (x == 1 || (p == 6 && x == 3))
                            val = res / val;
                        else if ((p == 4 && (x == 2 || x == 3)) || (p == 6 && (x == 4 || x == 5)))
                            val = val * res;

                        sb.Append(val);
                    }
                    sb.Append(",");
                    sb.Append(FLIM_ImgData.RoiFit.flim_parameters.tau_m[ch] * res);
                    sb.Append(",");
                    if (p == 6)
                    {
                        sb.Append(FLIM_ImgData.RoiFit.flim_parameters.beta[ch][0] / (FLIM_ImgData.RoiFit.flim_parameters.beta[ch][0] + FLIM_ImgData.RoiFit.flim_parameters.beta[ch][2]));
                        sb.Append(",");
                        sb.Append(FLIM_ImgData.RoiFit.flim_parameters.beta[ch][2] / (FLIM_ImgData.RoiFit.flim_parameters.beta[ch][0] + FLIM_ImgData.RoiFit.flim_parameters.beta[ch][2]));
                    }
                    else
                        sb.Append("1,0");
                    sb.Append(",");
                    sb.Append(FLIM_ImgData.RoiFit.flim_parameters.xi_square[ch]);
                    sb.AppendLine();
                } //if (FLIM_OK)
            } // for (int ch = 0; ch < nChs; ch++)

            if (FLIM_ImgData.Fit_type.Equals(FLIMData.FitType.SelectedRoi))
            {
                sb.Append("Fitting Selected ROI");
            }
            else if (FLIM_ImgData.Fit_type.Equals(FLIMData.FitType.GlobalRois))
            {
                sb.Append("Fitting Multi ROIs");
            }
            else
                sb.Append("Fitting whole Image");

            sb.AppendLine();
            if (FLIM_ImgData.Fit_type.Equals(FLIMData.FitType.SelectedRoi))
            {
                for (int ch = 0; ch < nChs; ch++)
                {
                    if (FLIM_OK[ch] && FLIM_ImgData.Roi.ROI_type == ROI.ROItype.PolyLine)
                        foreach (var roi2 in FLIM_ImgData.Roi.polyLineROIs)
                        {
                            sb = ROI_Data_String(sb, roi2, ch, "PolyLineROI-");
                        }
                }
            }
            else if (FLIM_ImgData.Fit_type.Equals(FLIMData.FitType.GlobalRois))
                for (int ch = 0; ch < nChs; ch++)
                {
                    if (FLIM_OK[ch])
                    {
                        foreach (var roi1 in FLIM_ImgData.ROIs)
                        {
                            sb = ROI_Data_String(sb, roi1, ch, "");
                            if (roi1.ROI_type == ROI.ROItype.PolyLine)
                                foreach (var roi2 in roi1.polyLineROIs)
                                {
                                    sb = ROI_Data_String(sb, roi2, ch, "PolyLineROI" + roi1 + "-");
                                }
                        }
                    } // if FLIM OK
                } //Loop channel

            sb.AppendLine();

            for (int ch = 0; ch < nChs; ch++)
            {
                if (FLIM_OK[ch] && ch < FLIM_ImgData.RoiFit.flim_parameters.LifetimeX.Length && FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[ch] != null)
                {
                    int chName = ch + 1;
                    sb.Append("Time-" + chName + ",");
                    for (int x = 0; x < FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[ch].Length; x++)
                    {
                        sb.Append(FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[ch][x] * res);
                        sb.Append(",");
                    }
                    sb.AppendLine();
                    sb.Append("Lifetime-" + chName + ",");
                    for (int x = 0; x < FLIM_ImgData.RoiFit.flim_parameters.LifetimeY[ch].Length; x++)
                    {
                        sb.Append(FLIM_ImgData.RoiFit.flim_parameters.LifetimeY[ch][x]);
                        sb.Append(",");
                    }
                    sb.AppendLine();
                    sb.Append("Residuals-" + chName + ",");

                    if (FLIM_ImgData.RoiFit.flim_parameters.residual[ch] != null)
                        for (int x = 0; x < FLIM_ImgData.RoiFit.flim_parameters.residual[ch].Length; x++)
                        {
                            sb.Append(FLIM_ImgData.RoiFit.flim_parameters.residual[ch][x]);
                            sb.Append(",");
                        }
                    sb.AppendLine();
                    sb.AppendLine();
                }
            }

            string allStr = sb.ToString();

            CreateAanalysisFolder();
            String fileName = FitFileName();
            TrySave(fileName, allStr);
        }

        private StringBuilder ROI_Data_String(StringBuilder sb, ROI roi1, int ch, string Note)
        {
            int roi = roi1.ID;
            int rName = roi + 1;
            int cName = ch + 1;
            double res = FLIM_ImgData.psPerUnit / 1000;
            sb.Append(Note + "ROI" + rName + "-ch" + cName);
            int p = roi1.flim_parameters.beta[ch].Length;
            for (int x = 0; x < p; x++)
            {
                sb.Append(",");
                double val = roi1.flim_parameters.beta[ch][x];
                if (x == 1 || (p == 6 && x == 3))
                    val = res / val;
                else if ((p == 4 && (x == 2 || x == 3)) || (p == 6 && (x == 4 || x == 5)))
                    val = val * res;

                sb.Append(val);
            }
            sb.Append(",");
            sb.Append(roi1.flim_parameters.tau_m[ch] * res);
            sb.Append(",");
            if (p == 6)
            {
                sb.Append(roi1.flim_parameters.beta[ch][0] / (roi1.flim_parameters.beta[ch][0]
                    + roi1.flim_parameters.beta[ch][2]));
                sb.Append(",");
                sb.Append(roi1.flim_parameters.beta[ch][2] / (roi1.flim_parameters.beta[ch][0]
                    + roi1.flim_parameters.beta[ch][2]));
            }
            else
                sb.Append("1,0");
            sb.Append(",");
            sb.Append(roi1.flim_parameters.xi_square[ch]);
            sb.AppendLine();
            return sb;
        }



        private void TrySave(string fileName, string allStr)
        {
            try
            {
                //Try to close window with the same name.
                String fn = Path.GetFileNameWithoutExtension(fileName);
                FormControllers.CloseOpenExcelWindow(fn);
                File.WriteAllText(fileName, allStr);
            }
            catch (Exception E)
            {
                //if this does not save, it will create file (1), (2)....
                //

                Debug.WriteLine("Error in saving ..." + E.Message);
                //MessageBox.Show("The excel file: " + fileName + " is locked. Close the file and press OK");
                for (int i = 1; i < 3; i++)
                {
                    try
                    {
                        //String fileName1 = Path.GetFileNameWithoutExtension(fileName);
                        fileName = fileName + "(" + i + ").csv";
                        File.WriteAllText(fileName, allStr);
                        break;
                    }
                    catch (Exception E3)
                    {
                        Debug.WriteLine("Error in saving ..." + E3.ToString());
                        //MessageBox.Show("The excel file: " + fileName + " is locked. Close the file and press OK");
                    }
                }
            }

        }

        private void CreateAanalysisFolder()
        {
            Directory.CreateDirectory(FLIM_ImgData.pathName + "\\Analysis");
            Directory.CreateDirectory(FLIM_ImgData.pathName + "\\Analysis\\ROI");
            Directory.CreateDirectory(FLIM_ImgData.pathName + "\\Analysis\\FIT");
        }

        public String TimeCourseFileName()
        {
            return Path.Combine(FLIM_ImgData.pathName, "Analysis", FLIM_ImgData.baseName + "_TimeCourse.csv");
        }

        private String RoiFileName(bool imageJ)
        {
            if (imageJ)
                return Path.Combine(FLIM_ImgData.pathName, "Analysis", "ROI", FLIM_ImgData.fileName.Split('.')[0] + "_ROI_ImageJ.zip");
            else
                return Path.Combine(FLIM_ImgData.pathName, "Analysis", "ROI", FLIM_ImgData.fileName.Split('.')[0] + "_ROI.txt");
        }

        private String FitFileName()
        {
            return Path.Combine(FLIM_ImgData.pathName, "Analysis", "Fit", FLIM_ImgData.fileName.Split('.')[0] + "_Fit.csv");
        }


        private void Fit_Click(object sender, EventArgs e)
        {
            fittingDone = false;
            LifetimeCurvePlot.Invalidate();
            if (FLIM_ImgData.State.Acq.acqFLIMA.Any(x => x == true))
            {
                if (AutoApplyOffset.Checked)
                    ApplyOffset(true, true);
                else
                    FitData(false, true, -1);
            }
        }


        public double[] FitData(bool fixAll, bool saveEvent, int page1)
        {
            if (realtime || !FLIM_ImgData.State.Acq.acqFLIMA.Any(x1 => x1 == true))
                return FLIM_ImgData.offset;

            int mode;
            if (SingleExp.Checked)
                mode = 1;
            else
                mode = 2;

            double[] x = FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[0];
            double[] y = FLIM_ImgData.RoiFit.flim_parameters.LifetimeY[0];

            bool page_direct = FLIM_ImgData.PageDirect(page1);
            if (page_direct)
            {
                x = FLIM_ImgData.RoiFit.flim_parameters_Pages[page1].LifetimeX[0];
                y = FLIM_ImgData.RoiFit.flim_parameters_Pages[page1].LifetimeY[0];
            }
            else
                page1 = -1;

            if (y == null || y.Length != x.Length)
                FLIM_ImgData.ResetLifetimeCalculation(false);

            FLIM_ImgData.calculateLifetime(page1);

            double[][] beta_g = new double[FLIM_ImgData.nChannels][];

            for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
            {
                //if (page_direct)
                //    beta_g[ch] = (double[])FLIM_ImgData.RoiFit.flim_parameters_Pages[page1].beta[ch].Clone();
                //else
                beta_g[ch] = (double[])FLIM_ImgData.RoiFit.flim_parameters.beta0[ch].Clone();

                if (FLIM_ImgData.State.Acq.acqFLIMA[ch])
                {
                    double[] beta0;
                    bool[] fix;
                    double res = FLIM_ImgData.psPerUnit / 1000;

                    double[] param = new double[N_FITPARAM];

                    if (fit_param[ch] != null)
                        param = (double[])fit_param[ch].Clone();

                    if (mode == 1)
                    {
                        beta0 = new double[4];
                        beta0[0] = param[0];
                        beta0[1] = res / param[1];
                        beta0[2] = param[4] / res;
                        beta0[3] = param[5] / res;

                        fix = new bool[4];
                        fix[0] = false;
                        fix[1] = cb_tau1Fix.Checked;
                        fix[2] = cb_tauGFix.Checked;
                        fix[3] = cb_T0Fix.Checked;

                        if (fixAll)
                        {
                            fix[1] = true;
                            fix[2] = true;
                            fix[3] = true;
                        }
                    }
                    else
                    {
                        beta0 = new double[6];
                        beta0[0] = param[0];
                        beta0[1] = res / param[1];
                        beta0[2] = param[2];
                        beta0[3] = res / param[3];
                        beta0[4] = param[4] / res;
                        beta0[5] = param[5] / res;

                        fix = new bool[6];

                        fix[0] = false;
                        fix[1] = cb_tau1Fix.Checked;
                        fix[2] = false;
                        fix[3] = cb_tau2Fix.Checked;
                        fix[4] = cb_tauGFix.Checked;
                        fix[5] = cb_T0Fix.Checked;

                        if (fixAll)
                        {
                            fix[1] = true;
                            fix[3] = true;
                            fix[4] = true;
                            fix[5] = true;
                        }

                    }
                    //betahat = Fitting.nlinfit_exGauss(beta0, fix, y, x, 1);
                    //FLIM_ImgData.fittingList[ch] = betahat;

                    var bet1 = FLIM_ImgData.fitData(ch, mode, beta0, fix, page1);
                    if (bet1 != null)
                        beta_g[ch] = bet1;

                } //FLIMRaw[ch] != null && acqFLIMA[ch]

                if (FLIM_ImgData.Fit_type.Equals(FLIMData.FitType.SelectedRoi) && FLIM_ImgData.Roi.ROI_type == ROI.ROItype.PolyLine)
                {
                    for (int j = 0; j < FLIM_ImgData.Roi.polyLineROIs.Count; j++)
                        FLIM_ImgData.fitData_ROI_Ch(FLIM_ImgData.Roi.polyLineROIs[j], ch, mode, beta_g[ch], page1);
                }
            } // for all channels.

            if (FLIM_ImgData.Fit_type.Equals(FLIMData.FitType.GlobalRois))
                FLIM_ImgData.fitDataAllROIs(mode, page1, beta_g);

            double[] offset_out;
            if (page_direct)
                offset_out = (double[])FLIM_ImgData.RoiFit.flim_parameters_Pages[page1].offset_fit.Clone();
            else
                offset_out = (double[])FLIM_ImgData.RoiFit.flim_parameters.offset_fit.Clone();

            if (saveEvent)
            {
                if (page_direct)
                    FLIM_ImgData.RoiFit.flim_parameters = FLIM_ImgData.RoiFit.flim_parameters_Pages[page1].Copy();
                fittingDone = true;
                if (AutoApplyOffset.Checked)
                {
                    FLIM_ImgData.offset = offset_out;
                }
                int channel = Ch1.Checked ? 0 : 1;
                this.InvokeIfRequired(o => o.UpdateFittingParam(channel, true));
                LifetimeCurvePlot.Invalidate();
                //UpdateImages(false, false, false, true);
                SaveAllRois();
                SaveFittingData();
            } //If (SaveEvent)

            return offset_out;
        } //FitData end.


        /// <summary>
        /// Called when "FIx All" checkbox is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FixAll_CheckedChanged(object sender, EventArgs e)
        {
            if (sender.Equals(fix_all))
            {
                bool b = fix_all.Checked;
                cb_tau1Fix.Checked = b;
                cb_tau2Fix.Checked = b;
                cb_tauGFix.Checked = b;
                cb_T0Fix.Checked = b;
            }
        }

        /// <summary>
        /// Referesh Image1 and Image2.
        /// </summary>
        void DrawImages()
        {
            Image1.Invalidate(); //Referesh Image1.
            Image2.Invalidate(); //Referesh Image2.
        }

        /// <summary>
        /// Called when Iamge1 or Image2 is refreshed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_Paint(object sender, PaintEventArgs e)
        {
            Bitmap drawBMP;
            object sync = sync_disp;

            PictureBox pb = (PictureBox)sender;

            if (sender.Equals(Image1))
            {
                sync = sync_disp;
                if (MergeCB.Checked)
                {
                    lock (syncBmp)
                        drawBMP = ImageProcessing.MergeBitmaps(IntensityBitmap, IntensityBitmap2);
                }
                else if (FastZStack && FLIM_ImgData.State.Acq.FastZ_phase_detection_mode)
                {
                    lock (syncBmp)
                        drawBMP = ImageProcessing.MergeBitmaps(IntensityBitmap, Bitmap_FastZ_PhaseComplementary);
                }
                else
                {
                    if (Channel1.Checked || Channel12.Checked)
                    {
                        lock (syncBmp[0])
                        {
                            if (HoldCurrentImageCheckBox.Checked && IntensityBitmap_Save != null)
                                drawBMP = ImageProcessing.MergeBitmaps(IntensityBitmap, IntensityBitmap_Save);
                            else if (IntensityBitmap != null)
                                drawBMP = IntensityBitmap;
                            else
                                drawBMP = BlankBMP;
                        }
                    }
                    else if (Channel2.Checked)
                    {
                        lock (syncBmp[1])
                        {
                            if (HoldCurrentImageCheckBox.Checked && IntensityBitmap_Save2 != null)
                                drawBMP = ImageProcessing.MergeBitmaps(IntensityBitmap2, IntensityBitmap_Save2);
                            else if (IntensityBitmap2 != null)
                                drawBMP = IntensityBitmap2;
                            else
                                drawBMP = BlankBMP;
                        }
                    }
                    else
                    {
                        lock (syncBmp[0])
                            drawBMP = IntensityBitmap;
                    }
                }
            }

            else if (sender.Equals(Image2))
            {
                sync = sync_disp2;
                if (Channel12.Checked)
                {
                    lock (syncBmp[1])
                    {
                        if (HoldCurrentImageCheckBox.Checked && IntensityBitmap_Save2 != null)
                            drawBMP = ImageProcessing.MergeBitmaps(IntensityBitmap2, IntensityBitmap_Save2);
                        else if (IntensityBitmap2 != null)
                            drawBMP = IntensityBitmap2;
                        else
                            drawBMP = BlankBMP;
                    }
                }
                else
                {
                    int c = 0;
                    if (Channel2.Checked)
                        c = 1;

                    lock (syncBmpFLIM)
                    {
                        if (ShowFLIM.Checked && FLIM_ImgData.State.Acq.acqFLIMA[c])
                            drawBMP = FLIMBitmap;
                        else
                            drawBMP = BlankBMP;
                    }
                }

            }
            else
                return;

            if (drawBMP == null)
                drawBMP = BlankBMP;

            lock (sync)
            {
                try
                {
                    //if (FLIM_ImgData.State.Acq.nSplitScanning <= 2
                    //    || drawBMP.Width * 2 > drawBMP.Height) //128x128, for example.
                    //{
                    subPanel = new int[] { 1, 1 };
                    int startX = 0;
                    int startY = 0;
                    int destWidth = pb.Width;
                    int destHeight = pb.Height;
                    if (drawBMP.Width < drawBMP.Height)
                    {
                        destWidth = drawBMP.Width * pb.Width / drawBMP.Height;
                        startX = (pb.Width - destWidth) / 2;
                    }
                    else if (drawBMP.Width > drawBMP.Height)
                    {
                        destHeight = drawBMP.Height * pb.Height / drawBMP.Width;
                        startY = (pb.Height - destHeight) / 2;
                    }
                    e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    e.Graphics.DrawImage(drawBMP,
                        new Rectangle(startX, startY, destWidth, destHeight), // destination rectangle 
                        0, 0,           // upper-left corner of source rectangle
                        drawBMP.Width,       // width of source rectangle
                        drawBMP.Height,      // height of source rectangle
                        GraphicsUnit.Pixel);
                    //}
                    //else //perhpas 4, and not square.
                    //{
                    //    int nSplit = FLIM_ImgData.State.Acq.nSplitScanning;
                    //    int nCols = (int)Math.Round(Math.Sqrt(nSplit));
                    //    int nRows = (int)Math.Ceiling((double)nSplit / nCols);
                    //    int imWidth = drawBMP.Width;
                    //    int imHeight = drawBMP.Height / nSplit;
                    //    int destWidth_Frame = pb.Width / nCols;
                    //    int destHeight_Frame = pb.Height / nRows;
                    //    int destWidth_Image = destWidth_Frame;
                    //    int destHeight_Image = destHeight_Frame;
                    //    int startX = 0;
                    //    int startY = 0;
                    //    if (imWidth < imHeight)
                    //    {
                    //        destWidth_Image = destWidth_Frame * imWidth / imHeight;
                    //        startX = (destWidth_Frame - destWidth_Image) / 2;
                    //    }
                    //    else if (imWidth > imHeight)
                    //    {
                    //        destHeight_Image = destHeight_Frame * imHeight / imWidth;
                    //        startY = (destHeight_Frame - destHeight_Image) / 2;
                    //    }

                    //    subPanel = new int[] { nCols, nRows };

                    //    for (int x = 0; x < nCols; x++)
                    //        for (int y = 0; y < nRows; y++)
                    //        {
                    //            int imPos = y * nCols + x;
                    //            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    //            e.Graphics.DrawImage(drawBMP,
                    //                new Rectangle(startX + x * destWidth_Frame, startY + y * destHeight_Frame, destWidth_Image, destHeight_Image), // destination rectangle 
                    //                0, imPos * imHeight,           // upper-left corner of source rectangle
                    //                imWidth,       // width of source rectangle
                    //                imHeight,      // height of source rectangle
                    //                GraphicsUnit.Pixel);
                    //        }
                    //}
                }
                catch (Exception E)
                {
                    Debug.WriteLine("Error in drawing (Image1_Paint:" + E.Message);
                    return;
                }

                uLoc = ConvertFractionFromRealToDisplay(uncagingLocFrac);

                if (uncaging_on && drawUncagingPos != UncagingCursor.Inactive)
                {
                    drawUncaging(uLoc, UncageCPenThick, e);
                    drawUncaging(referenceLoc, RefCPenThick, e);
                }

                if (uncagingLocs.Count > 0)
                {
                    for (int i = 0; i < uncagingLocs.Count; i++)
                    {
                        Point uloc1 = (Point)ConvertFractionFromRealToDisplay(uncagingLocs[i]);
                        drawUncaging(uloc1, multiRoiPen, e);
                        int WidthOfMark = (int)(Image1.Width * sizeOfMark);
                        Rectangle drawRect = new Rectangle(uloc1.X - WidthOfMark, uloc1.Y - WidthOfMark - 15, 20, 20);
                        e.Graphics.DrawString((i + 1).ToString(), roiFont, roiBrush, drawRect);
                    }
                }

                PointF shiftP = new Point(0, 0);
                if (roi_State == drawROI_State.Moving_ExistingROI)
                {
                    ROI currentRoi = null;
                    convertROIfromDisplayToData(imageRoi, ref currentRoi);
                    shiftP.X = currentRoi.Rect.Left - CurrentROIBeforeMove.Rect.Left;
                    shiftP.Y = currentRoi.Rect.Top - CurrentROIBeforeMove.Rect.Top;
                }

                if (FLIM_ImgData.ROIs.Count > 0)
                {
                    for (int i = 0; i < FLIM_ImgData.ROIs.Count; i++)
                    {
                        ROI roi1 = FLIM_ImgData.ROIs[i].CopyROI(FLIM_ImgData.ROIs[i].ID);
                        if (roi_State == drawROI_State.Moving_ExistingROI && (Control.ModifierKeys == Keys.Shift || i == FLIM_ImgData.currentRoi))
                            roi1.shiftRoi(shiftP);

                        drawROIsGeneric(e, multiRoiPen, multiRoiPenOutOfFocus, roi1, false, false, (roi1.ID + 1).ToString());
                    }
                }//ROIs

                if (FLIM_ImgData.bgRoi.Rect.Width > 0 && FLIM_ImgData.bgRoi.Rect.Height > 0)
                {
                    ROI roi1 = FLIM_ImgData.bgRoi.CopyROI(-2);
                    if (roi_State == drawROI_State.Moving_ExistingROI && FLIM_ImgData.currentRoi == -2)
                        roi1.shiftRoi(shiftP);

                    drawROIsGeneric(e, BgRoiPen, BgRoiPenOutOfFocus, roi1, false, false, "bg");
                }

                if (FLIM_ImgData.Roi != null && roi_State != drawROI_State.NoROI && roi_State != drawROI_State.Inactive)
                {
                    drawROIsGeneric(e, roiPen, roiPenOutOfFocus, imageRoi, true, true, "");
                }
            } //sync
        }

        private double[] ConvertPointFromDisplayToReal(Point p)
        {
            return new double[] { (double)p.X / Image1.Width, (double)p.Y / Image1.Height };

            //double[] xy_disp = new double[] { (double)p.X / Image1.Width, (double)p.Y / Image1.Height };
            //This is for subpanels, but it appears not working..
            //if (subPanel[0] == 1)
            //return xy_disp;
            //double[] xy_sub = new double[2];
            //int[] panelLoc = new int[2];
            //for (int j = 0; j < 2; j++)
            //{
            //    double panelLen = 1.0 / (double)subPanel[j];
            //    panelLoc[j] = (int)(xy_disp[j] / panelLen);
            //    xy_sub[j] = xy_disp[j] - panelLen * panelLoc[j]; //0 --> 0 etc.
            //}

            //xy_sub[1] = xy_sub[1] + panelLoc[0] * subPanel[0] + panelLoc[1];
            //return xy_sub;
        }

        private Point ConvertFractionFromRealToDisplay(double[] xy_real)
        {
            return new Point((int)(xy_real[0] * Image1.Width), (int)(xy_real[1] * Image1.Height));

            //This is for subpanels, but it appears not working..
            //double[] xy_disp = (double[])xy_real.Clone();
            //int[] panelLoc = new int[2];
            //double panelLen = 1.0 / (double)subPanel[1];
            //int panelLoc_through = (int)(xy_real[1] / panelLen); //Y location.
            //xy_disp[1] = xy_real[1] - panelLen * panelLoc_through;
            //panelLoc[0] = panelLoc_through % subPanel[0]; //for 2x2 and pos 2, 0
            //panelLoc[1] = panelLoc_through / subPanel[0]; //for 2x2 and pos 2, 1.

            //for (int j = 0; j < 2; j++)
            //{
            //    xy_disp[j] = xy_real[j] + panelLen * panelLoc[j];
            //}
            //var display_Loc = new Point((int)(xy_disp[0] * Image1.Width), (int)(xy_disp[1] * Image1.Height));
            //return display_Loc;
        }

        private void drawROIsGeneric(PaintEventArgs e, Pen p, Pen outOfFocusPen, ROI roi_original, bool drawHndle, bool displayRoi, string label)
        {
            Pen pen = p;
            if (ThreeDRoi && !displayZProjection && !FLIM_ImgData.IsRoiInFocus(FLIM_ImgData.Roi))
                pen = outOfFocusPen;

            ROI roi = roi_original;
            if (!displayRoi)
            {
                roi = new ROI(roi_original);
                convertROIFromDataToDisplay(roi_original, ref roi);
            }

            var handleSizeImage = handleSize * image_scale;
            if (roi.Points != null)
            {
                var drawRect = new RectangleF(roi.X[0] - 15, roi.Y[0] - 15, 100, 20);
                e.Graphics.DrawString(label.ToString(), roiFont, roiBrush, drawRect);

                if (roi.ROI_type.Equals(ROI.ROItype.Elipsoid))
                    e.Graphics.DrawEllipse(pen, roi.Rect);
                else if (roi.ROI_type.Equals(ROI.ROItype.Rectangle))
                {
                    e.Graphics.DrawRectangle(pen, new Rectangle((int)roi.Rect.X, (int)roi.Rect.Y,
                        (int)roi.Rect.Width, (int)roi.Rect.Height));


                    if (drawHndle)
                        e.Graphics.FillRectangle(pen.Brush, new Rectangle((int)(roi.Rect.Right - handleSizeImage),
                            (int)(roi.Rect.Bottom - handleSizeImage), (int)handleSizeImage, (int)handleSizeImage));
                }
                else if (roi.Points.Length > 1 && roi.ROI_type.Equals(ROI.ROItype.PolyLine))
                {
                    drawPolyLineROI(e, pen, roi);
                }
                else if (roi.Points.Length > 1)
                {
                    e.Graphics.DrawPolygon(pen, roi.Points);
                    foreach (var point in roi.Points)
                        e.Graphics.FillEllipse(pen.Brush, point.X - handleSizeImage / 2, point.Y - handleSizeImage / 2, handleSizeImage, handleSizeImage);
                }
            }
        }

        private void drawPolyLineROI(PaintEventArgs e, Pen p, ROI roi)
        {
            p.Width = 1f;
            PointF[] points1 = roi.SmoothCurvePoints;
            if (points1 == null || points1.Length < 2)
                return;
            e.Graphics.DrawLines(p, points1);

            var handleSizeImage = handleSize * image_scale;
            foreach (var point in roi.Points)
                e.Graphics.FillEllipse(p.Brush, point.X - handleSizeImage / 2, point.Y - handleSizeImage / 2, handleSizeImage, handleSizeImage);

            p.Width = 0.5f;
            for (int i = 0; i < roi.polyLineROIs.Count; i++)
            {
                if (i == 0)
                    p.Width = 3f;
                else
                    p.Width = 0.5f;
                e.Graphics.DrawEllipse(p, roi.polyLineROIs[i].Rect);
            }
        }

        private void drawUncaging(Point position, Pen p, PaintEventArgs e)
        {
            if (position.Y >= 0 && position.Y < Image1.Height && position.X >= 0 && position.X < Image1.Width)
            {
                int highY = (int)(position.Y + Image1.Height * sizeOfMark);
                if (highY > Image1.Height)
                    highY = Image1.Height;
                int LowY = (int)(position.Y - Image1.Height * sizeOfMark);
                if (LowY < 0)
                    LowY = 0;
                int highX = (int)(position.X + Image1.Width * sizeOfMark);
                if (highX > Image1.Width)
                    highX = Image1.Width;
                int LowX = (int)(position.X - Image1.Height * sizeOfMark);
                if (LowX < 0)
                    LowX = 0;

                Point P1 = new Point(position.X, highY);
                Point P2 = new Point(position.X, LowY);
                e.Graphics.DrawLine(p, P1, P2);
                Point P3 = new Point(highX, position.Y);
                Point P4 = new Point(LowX, position.Y);
                e.Graphics.DrawLine(p, P3, P4);
            }
        }

        private void Auto1_CheckedChanged(object sender, EventArgs e)
        {
            UpdateImages(true, realtime, focusing, false);
        }

        private void Auto_contrast() //Does not handle any graphics.
        {
            MapStateIntensityRange();

            int cl = currentChannel;

            //for (int cl = 0; cl < FLIM_ImgData.nChannels; cl++)
            //{
            if (FLIM_ImgData.Project[cl] != null)
            {
                var project1 = FLIM_ImgData.Project[cl];
                var project2 = new UInt16[project1.GetLength(0) - 2, project1.GetLength(1) - 2];
                var siz = sizeof(UInt16);
                var l_length1 = project1.GetLength(1);
                var l_length2 = project2.GetLength(1) * siz;
                for (int i = 2; i < project1.GetLength(0) - 2; i++)
                    Buffer.BlockCopy(project1, l_length1 * i + siz, project2, l_length2 * i, l_length2);
                State_intensity_range[cl][1] = MatrixCalc.calcMax(project2);
                State_intensity_range[cl][0] = 0;
            }
            else
            {
                State_intensity_range[cl][1] = 2;
                State_intensity_range[cl][0] = 0;
            }

            if (State_intensity_range[cl][1] < 2)
                State_intensity_range[cl][1] = 2;

            if (State_intensity_range[cl][0] >= State_intensity_range[cl][1])
                State_intensity_range[cl][0] = State_intensity_range[cl][1] - 1;

            State_FLIM_intensity_range[cl][1] = Math.Ceiling(State_intensity_range[cl][1] * 0.5); //Calculate from maximum intensity.

            double def_val = (double)FLIM_ImgData.nAveragedFrame[cl] / 2.0;
            if (def_val < State_FLIM_intensity_range[cl][1])
                State_FLIM_intensity_range[cl][0] = def_val;
            else
                State_FLIM_intensity_range[cl][0] = Math.Round(State_intensity_range[cl][1] * 0.15);

            if (State_FLIM_intensity_range[cl][1] < 2)
                State_FLIM_intensity_range[cl][1] = 2;
            if (State_FLIM_intensity_range[cl][0] < 2)
                State_FLIM_intensity_range[cl][0] = 0.2;
            //}

            for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                SaveIntensity_Range(ch, true);
            //nAveragedSave = FLIM_ImgData.nAveragedFrame;

            //UpdateImageParamText(); //SetSlider and text.
        }

        private void Ch1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ChChecks.Count; i++)
            {
                if (ChChecks[i].Checked)
                {
                    currentChannel = i;
                    break;
                }
            }

            Channel1_Clicked(sender, e);
        }

        private void ChannelStatusChanged()
        {
            if (FLIM_ImgData.FLIM_on[currentChannel])
                this.InvokeIfRequired(o => o.UpdateFittingParam(currentChannel, fittingDone));
            this.InvokeIfRequired(o => o.UpdateImageParamText());
            UpdateImages(true, realtime, focusing, true);
        }

        private void Channel1_Clicked(object sender, EventArgs e)
        {


            if (ChannelChecks.Any(x => x.Checked))
            {
                for (int i = 0; i < ChannelChecks.Count; i++)
                {
                    if (ChannelChecks[i].Checked)
                    {
                        currentChannel = i;
                        break;
                    }
                }

                st_im1.Text = "Intensity " + (currentChannel + 1);
                st_im2.Text = "FLIM " + (currentChannel + 1);

                LifetimeCh_panel.Enabled = false;

                for (int i = 0; i < N_RANGE; i++)
                {
                    MaxMinTextBox[2][i].Enabled = true;
                    MaxMinSliders[2][i].Enabled = true;
                }
            }
            else if (Channel12.Checked)
            {
                st_im1.Text = "Intensity 1";
                st_im2.Text = "Intensity 2";

                LifetimeCh_panel.Enabled = true;

                for (int i = 0; i < N_RANGE; i++)
                {
                    MaxMinTextBox[2][i].Enabled = false;
                    MaxMinSliders[2][i].Enabled = false;
                }
            }

            for (int i = 0; i < ChChecks.Count; i++)
                ChChecks[i].Checked = (i == currentChannel);

            st_panel.Text = "Lifetime " + (currentChannel + 1);

            ChannelStatusChanged();
        }

        private void Slider_ValueChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < MaxMinSliders.Length; i++)
                for (int j = 0; j < N_RANGE; j++) //large small.
                {
                    if (MaxMinSliders[i][j].Equals(sender))
                        MaxMinTextBox[i][j].Text = (MaxMinSliders[i][j].Value / SldrZoom[i]).ToString();
                }
            ApplyTextToRange(true);
            //SaveIntensity_Range(currentChannel, true);
        }

        private void UpdateFigure_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tb = (TextBox)sender;
                String SaveText = tb.Text;
                try
                {
                    if (!Int32.TryParse(filterWindow.Text, out int fw))
                        fw = 0;
                    FLIM_ImgData.State.Display.filterWindow_FLIM = fw;

                    if (sender.Equals(MinIntensity2))
                        FLIM_ImgData.ResetLifetimeCalculation(true);

                    if (sender.Equals(c_page))
                    {
                        int currentPage = FLIM_ImgData.currentPage; //(FLIM_ImgData.nFastZ > 1) ? FLIM_ImgData.currentFastZ : FLIM_ImgData.currentPage;
                        int nPage = (FLIM_ImgData.nFastZ > 1) ? FLIM_ImgData.nFastZ : FLIM_ImgData.n_pages;

                        int page = currentPage;
                        int outI;
                        if (Int32.TryParse(c_page.Text, out outI)) page = outI - 1;
                        if (page < nPage && page != currentPage)
                        {
                            FLIM_ImgData.currentPage = page;
                            setupImageUpdateForZStack();
                            UpdateImages(true, realtime, focusing, true);
                        }

                        if (plot_regular.calc_upon_open && !ZStack && !FastZStack)
                            CalculateCurrentPage(false);
                    }
                    else
                    {
                        ApplyTextToRange(true);
                        UpdateImages(true, realtime, focusing, true);
                    }
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

        /// <summary>
        /// For external command
        /// </summary>
        /// <param name="channel"></param>
        public void SetChannel(int channel)
        {
            var sdr = Channel12;
            for (int i = 0; i < ChannelChecks.Count; i++)
            {
                ChannelChecks[i].Checked = (channel == i);
                sdr = ChannelChecks[i];
            }

            if (channel == 12)
            {
                Channel12.Checked = true;
                sdr = Channel12;
            }

            //Channel1_CheckedChanged(sdr, null); This will be automatic?
        }

        /// <summary>
        /// For external command.
        /// Set Fit Range. [fit1, fit2]
        /// </summary>
        /// <param name="values"></param>
        public void SetFitRange(int[] values)
        {
            fit_start.Text = values[0].ToString();
            fit_end.Text = values[1].ToString();
            SetFitting_Param(false);
            FLIM_ImgData.fitRangeChanged();
            UpdateImages(true, realtime, focusing, true);
        }


        /// <summary>
        /// For external command.
        /// </summary>
        /// Fix tau to a certain value. tau1, tau2
        /// <param name="values"></param>
        public void FixTau(double[] values)
        {
            tau1.Text = values[0].ToString();
            tau2.Text = values[1].ToString();
            cb_tau1Fix.Checked = true;
            cb_tau2Fix.Checked = true;
            SetFitting_Param(true);
            UpdateImages(true, realtime, focusing, true);
        }

        public void FixTauAll(bool ON)
        {
            cb_tau1Fix.Checked = ON;
            cb_tau2Fix.Checked = ON;
            cb_tauGFix.Checked = ON;
            cb_T0Fix.Checked = ON;
        }

        /// <summary>
        /// For external command. Set Intensity Ranges.
        /// Input = {min, max}
        /// </summary>
        /// <param name="values"></param>
        public void SetFLIMIntensityOffset(double[] values)
        {
            MinIntensity1.Text = values[0].ToString();
            MaxIntensity1.Text = values[1].ToString();
            MinIntensity2.Text = values[0].ToString();
            MaxIntensity2.Text = values[1].ToString();

            ApplyTextToRange(true);
            UpdateImages(true, realtime, focusing, true);
        }

        private void Analysis_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tb = (TextBox)sender;
                String SaveText = tb.Text;
                try
                {
                    SetFitting_Param(false);
                    UpdateImages(true, realtime, focusing, true);
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

        private void Slider_mouseUp(object sender, MouseEventArgs e)
        {
            ApplyTextToRange(true);
            UpdateImages(true, realtime, focusing, true);
        }

        /// <summary>
        /// Usually called from outside. To associate FLIM data with image_display
        /// </summary>
        /// <param name="FLIM"></param>
        public void LoadFLIM(FLIMData FLIM)
        {
            FLIM_ImgData = FLIM;
        }

        private void Apply_Offset_Click(object sender, EventArgs e)
        {
            ApplyOffsetFromGUI();
        }

        public void ApplyOffsetFromGUI()
        {
            FitData(false, true, -1);

            int c = 0;
            if (Ch1.Checked)
                c = 0;
            else if (Ch2.Checked)
                c = 1;

            double offset1;
            if (Double.TryParse(t0_Img.Text, out offset1)) FLIM_ImgData.offset[c] = offset1;
            FLIM_ImgData.State.Spc.analysis.offset[c] = FLIM_ImgData.offset[c];
            UpdateFittingParam(c, fittingDone);
            UpdateImages(true, realtime, focusing, true);
        }

        public void ApplyOffset(bool fitting, bool update_gui)
        {
            if (fitting)
            {
                FitData(false, true, -1);
            }

            int c = 0;
            if (Ch1.Checked)
                c = 0;
            else if (Ch2.Checked)
                c = 1;

            //This is for GUI.
            //if (Double.TryParse(t0_Img.Text, out offset1)) FLIM_ImgData.offset[c] = offset1;
            //FLIM_ImgData.State.Spc.analysis.offset[c] = FLIM_ImgData.offset[c];

            FLIM_ImgData.offset[c] = FLIM_ImgData.RoiFit.flim_parameters.offset_fit[c];
            FLIM_ImgData.State.Spc.analysis.offset[c] = FLIM_ImgData.offset[c];

            if (update_gui)
            {
                this.InvokeIfRequired(o => o.UpdateFittingParam(c, fittingDone));
                UpdateImages(true, realtime, focusing, true);
            }
        }

        private void PageEnableControl()
        {
            c_page.Enabled = (!cb_projectionYes.Checked && FLIM_ImgData.n_pages > 0);
            PageStart.Enabled = !c_page.Enabled;
            PageEnd.Enabled = !c_page.Enabled;
        }

        public void calcZProjection()
        {
            if (displayZProjection && !realtime)
            {
                if (AveProjection.Checked || MaxProjection.Checked)
                {
                    if (!FLIM_ImgData.KeepPagesInMemory)
                    {
                        if (page_range[1] - page_range[0] > 100)
                        {
                            //DialogResult dr = MessageBox.Show("The file is big (>100 frames). Are you sure?", "Projection of big file", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                            //if (dr != DialogResult.Yes)
                            //{
                            //    page_range[0] = FLIM_ImgData.currentPage;
                            //    page_range[1] = FLIM_ImgData.currentPage;
                            //}
                        }
                    }

                    if (entireStack)
                    {
                        int npages = FLIM_ImgData.FLIM_Pages.Length;
                        //if (FLIM_ImgData.nFastZ > 1)
                        //    npages = FLIM_ImgData.nFastZ;

                        page_range[0] = 0;
                        page_range[1] = npages;
                    }

                    if (AveProjection.Checked)
                    {
                        FLIM_ImgData.calcZProject(FLIMData.projectionType.Sum, page_range);
                        FLIM_ImgData.ZProjection = true;
                    }
                    else if (MaxProjection.Checked)
                    {
                        FLIM_ImgData.calcZProject(FLIMData.projectionType.Max, page_range);
                        FLIM_ImgData.ZProjection = true;
                    }
                }
            }
            else
            {
                int page = -1;
                if (FLIM_ImgData.currentPage < FLIM_ImgData.n_pages)
                    page = FLIM_ImgData.currentPage;
                else if (FLIM_ImgData.n_pages > 0)
                    page = 0;

                if (!realtime && page >= 0)
                    GotoPage4D(page);

                FLIM_ImgData.ZProjection = false;
            }

        }

        private void UpdateImagesRecalc_Click(object sender, EventArgs e)
        {
            PageEnableControl();

            displayZProjection = cb_projectionYes.Checked;
            entireStack = EntireStack_Check.Checked;



            if (sender.Equals(cb_projectionYes) && displayZProjection)
            {
                if (page_range[1] - page_range[0] > 100 && !FLIM_ImgData.KeepPagesInMemory)
                {
                    int currentPage = FLIM_ImgData.currentPage;
                    if (FLIM_ImgData.nFastZ > 1)
                        currentPage = FLIM_ImgData.nFastZ;

                    page_range[1] = currentPage;
                    page_range[0] = currentPage;
                }
            }

            if (displayZProjection)
                calcZProjection();
            else
                GotoPage4D(FLIM_ImgData.currentPage);

            UpdateImages(true, realtime, focusing, true);

        }


        public void GotoPage5D(int page)
        {
            if (FLIM_ImgData.nFastZ > 1)
                FLIM_ImgData.gotoPage5D(page);
        }

        private void GotoPage4D(int page)
        {
            FLIM_ImgData.gotoPage(page);
        }

        public void setupImageUpdateForZStack()
        {
            if (FLIM_ImgData.currentPage < 0)
                FLIM_ImgData.currentPage = 0;

            if (displayZProjection && !realtime)
                calcZProjection();
            else
                GotoPage4D(FLIM_ImgData.currentPage);
        }

        private void Page_UpDownClick(object sender, EventArgs e)
        {
            //if (cb_projectionYes.Checked)
            //    return;
            if (FLIM_ImgData.n_pages <= 1)
                return;

            int dpage = 0;
            int page = FLIM_ImgData.currentPage; //FLIM_ImgData.nFastZ > 1 ? FLIM_ImgData.currentFastZ : FLIM_ImgData.currentPage;
            if (sender.Equals(PageUp))
                dpage = 1;
            else if (sender.Equals(PageDown))
                dpage = -1;
            else if (sender.Equals(PageUpUp))
                dpage = 10;
            else if (sender.Equals(PageDownDown))
                dpage = -10;

            if (!displayZProjection)
                GotoPage4D(page + dpage);
            else
            {
                page_range[0] = page_range[0] + dpage;
                page_range[1] = page_range[1] + dpage;
                AssurePageRange();
                calcZProjection();
            }

            UpdateImages(true, realtime, focusing, true);

            if ((plot_regular.calc_upon_open || AutoApplyOffset.Checked) && !ZStack && !FastZStack)
                CalculateCurrentPage(false);
        }

        private void AlignSlicesframesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AlignFrames();
        }

        public void AlignFrames()
        {
            int refChannel = currentChannel;
            ushort[,] saveProject = FLIM_ImgData.Project[refChannel];
            for (int i = 0; i < FLIM_ImgData.n_pages; i++)
            {
                GotoPage4D(i);

                //UpdateImages(false, false, false, false);
                //if (i == 0)
                //{
                //    saveProject = FLIM_ImgData.Project[refChannel];
                //}


                double[] xyDrift;
                //int[] range = new int[] { FLIM_ImgData.width / 4, FLIM_ImgData.height / 4 };
                //double xcorr = ImageProcessing.MatrixMeasureDrift2D(saveProject, FLIM_ImgData.Project[refChannel], range, out xyDrift);
                double xcorr = ImageProcessing.MatrixMeasureDrift2D_FFT(saveProject, FLIM_ImgData.Project[refChannel], out xyDrift);

                Debug.WriteLine("X, Y drift = {0}, {1}, Confidence = {2}", xyDrift[0], xyDrift[1], xcorr);

                var new_flim = new ushort[FLIM_ImgData.nChannels][,,];
                for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                {
                    new_flim[ch] = ImageProcessing.MatrixCorrectDriftFLIM(FLIM_ImgData.FLIMRaw[ch], xyDrift);

                }
                FLIM_ImgData.LoadFLIMRawFromData4D(new_flim, FLIM_ImgData.acquiredTime, false);
                FLIM_ImgData.calculateAll();
                FLIM_ImgData.addCurrentFLIMRawToPage4D(true, i, true);

                //GotoPage(i);
                UpdateImages(false, false, false, false);
                this.Refresh();
            }

        }

        private void openFLIMImageInNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScanParameters State = new ScanParameters();
            FLIMData flimdata = new FLIMData(State);
            Image_Display image_display2 = new Image_Display(flimdata, flimage, false);


            flimdata.KeepPagesInMemory = keepPagesInMemoryToolStripMenuItem.Checked;
            FileIO.FileError file_error = FileIO.SetupFLIMOpeningDlog(FLIM_ImgData.pathName, out int nPages, out String fileName);

            if (file_error != FileIO.FileError.Success)
            {
                image_display2.Close();

            }
            else
            {
                image_display2.Show();

                file_error = image_display2.OpenFLIM(fileName, true, plot_regular.calc_upon_open, true);

                image_display2.Text = image_display2.FLIM_ImgData.fileName;
                image_display2.plot_regular.Text = image_display2.FLIM_ImgData.fileName;
            }

        }

        private void OpenFLIMImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFLIM();
        }

        private void SetupOpenedFile()
        {
            realtime = false;
            //This is to import fit setting form the panel.
            SetFitting_Param(true);
            this.InvokeIfRequired(o => o.UpdateImageParamText());
            this.InvokeIfRequired(o => UpdateFileName());
            // UpdateImages(true, false, false, true);
        }

        private void Image_Display_Shown(object sender, EventArgs e)
        {
            this.Activate();
        }


        private int CheckFolder(FileIO fileIO, bool openFolderIntensity, bool openFolderFLIM)
        {
            String intensityPath = Path.Combine(FLIM_ImgData.State.Files.pathName, "Intensity");
            String FLIMPath = Path.Combine(FLIM_ImgData.State.Files.pathName, "FLIM");

            if (!Directory.Exists(FLIM_ImgData.pathName))
            {
                intensityPath = Path.Combine(FLIM_ImgData.pathName, "Intensity");
                FLIMPath = Path.Combine(FLIM_ImgData.pathName, "FLIM");
            }

            //update fileIO path.
            fileIO.State.Files.pathNameIntensity = intensityPath;
            fileIO.State.Files.pathNameFLIM = FLIMPath;

            if (!Directory.Exists(FLIM_ImgData.pathName))
            {
                MessageBox.Show("The folder " + FLIM_ImgData.pathName + "does not exist");

                return -1;
            }

            Directory.CreateDirectory(intensityPath);

            if (openFolderIntensity)
                Process.Start(intensityPath);

            if (FLIM_ImgData.FLIM_on.Any(x => x == true))
            {
                Directory.CreateDirectory(FLIMPath);
                if (openFolderFLIM)
                    Process.Start(FLIMPath);
            }

            return 0;
        }

        /// <summary>
        /// Save formated images (similar to bitmap images on the screen). Called from the menubar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFLIMImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var exp = new FLIMage.Dialogs.ExportForm(this);
            exp.Show();
        }

        private void exportColorBarJPGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap bmp;
            String extension = ".jpg";
            System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Jpeg;

            bmp = (Bitmap)colorBar.Image;
            String fileName = "color_bar.jpg";

            var fileIO = new FileIO(FLIM_ImgData.State);
            fileName = fileIO.SaveFLIMFileDialog(fileName, extension);

            if (fileName != "")
            {
                bmp.Save(fileName, format);
            }
        }

        private void exportCurrentImageJPGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportBitMap(".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        private void exportCurrentImagePNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportBitMap(".png", System.Drawing.Imaging.ImageFormat.Png);
        }

        private void exportCurrentImageBMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportBitMap(".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }

        private void exportBitMap(String extension, System.Drawing.Imaging.ImageFormat format)
        {
            Bitmap bmp;
            if (MergeCB.Checked)
                bmp = ImageProcessing.MergeBitmaps(IntensityBitmap, IntensityBitmap2);
            else
                bmp = IntensityBitmap;

            //bmp = ImageProcessing.ResizeBitmap(bmp, Image1.Width, Image1.Height);

            FileIO fileIO = new FileIO(FLIM_ImgData.State);
            FLIM_ImgData.fullName(currentChannel, false);
            String fileName = FLIM_ImgData.fileName.Split('.')[0] + extension;

            fileName = fileIO.SaveFLIMFileDialog(fileName, extension);
            var dir = Path.GetDirectoryName(fileName);
            var fn = Path.GetFileNameWithoutExtension(fileName);
            //var flieName_Intensity = Path.Combine(dir, fn + "_Intensity" + extension);
            var fileName_FLIM = Path.Combine(dir, fn + "_FLIM" + extension);
            bmp.Save(fileName, format);
            FLIMBitmap.Save(fileName_FLIM, format);
        }

        public void ChannelHandling(int channel)
        {
            for (int i = 0; i < ChannelChecks.Count; i++)
                ChannelChecks[i].Checked = (channel == i);
            Channel1_Clicked(ChannelChecks[channel], null);
        }

        public void SaveCurrentIntensityImage(bool[] saveFormat, bool[] saveChannels, int[] FastZFormat, bool correctT0Each, bool openprocCtrl)
        {
            bool saveZProjection = saveFormat[1];
            bool saveNormalFile = saveFormat[0];
            if (!saveZProjection && !saveNormalFile)
                return;
            if (saveChannels.All(x => x == false))
                return;

            displayZProjection = saveZProjection;
            cb_projectionYes.Checked = displayZProjection;

            FileIO fileIO = new FileIO(FLIM_ImgData.State);
            CheckFolder(fileIO, true, true);

            if (openprocCtrl)
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false));

            //int ch = channelToSave;
            int fNum = FLIM_ImgData.fileCounter;
            String procType = FLIM_ImgData.z_projection_type.ToString();

            string pathname = FLIM_ImgData.State.Files.pathName;
            if (!Directory.Exists(pathname))
                pathname = FLIM_ImgData.pathName;
            if (!Directory.Exists(pathname))
                pathname = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FLIMage\\Data";

            for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
            {
                if (saveChannels[ch])
                {
                    this.InvokeIfRequired(o => o.ChannelHandling(ch));

                    String filePathIntensityMax = fileIO.FLIM_FilePath(ch, true, fNum, FileIO.ImageType.Intensity, procType, pathname);
                    String filePathFLIMMax = fileIO.FLIM_FilePath(ch, true, fNum, FileIO.ImageType.FLIM_color, procType, pathname);

                    String filePathIntensity = fileIO.FLIM_FilePath(ch, true, fNum, FileIO.ImageType.Intensity, "", pathname);
                    String filePathFLIM = fileIO.FLIM_FilePath(ch, true, fNum, FileIO.ImageType.FLIM_color, "", pathname);

                    bool[] saveCh = new bool[FLIM_ImgData.nChannels];
                    saveCh[ch] = true;
                    ushort[,] imageStich;
                    ushort[,] imageStichMax;
                    Bitmap bitmapFLIM = null;
                    Bitmap bitmapFLIM_Max = null;

                    int npages = FastZStack ? FLIM_ImgData.n_pages5D : 1;

                    for (int p = 0; p < npages; p++)
                    {
                        if (FastZStack)
                            GotoPage5D(p);

                        if (saveNormalFile)
                        {
                            if (FastZStack) //if FastZ, we will stich everything together.
                            {
                                int nCol = FastZFormat[0];
                                int nRo = FastZFormat[1];
                                int startS = FastZFormat[2];
                                int endS = Math.Min(FastZFormat[3], FLIM_ImgData.n_pages);
                                int height = FLIM_ImgData.height;
                                int width = FLIM_ImgData.width;

                                int height_all = height * nRo;
                                int width_all = width * nCol;
                                imageStich = new ushort[height_all, width_all]; //[y, x]

                                if (FLIM_ImgData.FLIM_on[ch])
                                {
                                    bitmapFLIM = new Bitmap(width_all, height_all); //[x, y]
                                }

                                for (int i = startS; i < endS; i++)
                                {
                                    GotoPage4D(i);
                                    UpdateImages(true, false, false, false, true);
                                    int row = (i / nCol);
                                    int colm = (i % nCol);

                                    for (int y = 0; y < FLIM_ImgData.height; y++)
                                    {
                                        //Array.Copy(FLIM_ImgData.Project[ch][y], 0, imageStich[y + row * height], width * colm, width);
                                        Array.Copy(FLIM_ImgData.Project[ch], y * width, imageStich,
                                            (y + row * height) * width_all + width * colm, width);
                                    }

                                    if (FLIM_ImgData.FLIM_on[ch])
                                    {
                                        Bitmap bm = ImageProcessing.FormatImageFLIM(State_FLIM_intensity_range[ch], State_FLIM_lifetime_range[ch], FLIM_ImgData.LifetimeMapF[ch], FLIM_ImgData.ProjectF[ch], false, color_scheme);
                                        if (bm != null)
                                            using (Graphics g = Graphics.FromImage(bitmapFLIM))
                                            {
                                                g.DrawImage(bm, colm * width, row * height);
                                            }
                                    }
                                }

                                bool overwrite = p == 0;
                                fileIO.Save2DImageInTiff(filePathIntensity, imageStich, FLIM_ImgData.acquiredTime, overwrite, saveCh);

                                if (FLIM_ImgData.FLIM_on[ch])
                                {
                                    fileIO.SaveColorImageInTiff(filePathFLIM, bitmapFLIM, FLIM_ImgData.acquiredTime, overwrite, saveCh);
                                }
                            }
                            else //not FastZ
                            {

                                for (int i = 0; i < FLIM_ImgData.n_pages; i++)
                                {
                                    GotoPage4D(i);
                                    UpdateImages(true, false, false, false, true);

                                    if (correctT0Each && FLIM_ImgData.State.Acq.acqFLIMA.Any(x => x == true))
                                    {
                                        ApplyOffset(true, true);
                                    }

                                    bool overwrite = p == 0 && i == 0;
                                    fileIO.Save2DImageInTiff(filePathIntensity, FLIM_ImgData.Project[ch], FLIM_ImgData.acquiredTime, overwrite, saveCh);

                                    if (FLIM_ImgData.FLIM_on[ch])
                                    {
                                        Bitmap bm = ImageProcessing.FormatImageFLIM(State_FLIM_intensity_range[ch], State_FLIM_lifetime_range[ch], FLIM_ImgData.LifetimeMapF[ch], FLIM_ImgData.ProjectF[ch], false, color_scheme);
                                        fileIO.SaveColorImageInTiff(filePathFLIM, bm, FLIM_ImgData.acquiredTime, overwrite, saveCh);
                                    }

                                    if (StopFileOpening && !FastZStack)
                                        break;
                                }
                            }
                        } //Savenormal

                        if (saveZProjection)
                        {
                            calcZProjection();
                            UpdateImages(true, false, false, false, true);
                            imageStichMax = FLIM_ImgData.Project[ch];

                            if (FLIM_ImgData.FLIM_on[ch])
                                bitmapFLIM_Max = ImageProcessing.FormatImageFLIM(State_FLIM_intensity_range[ch], State_FLIM_lifetime_range[ch], FLIM_ImgData.LifetimeMapF[ch], FLIM_ImgData.ProjectF[ch], false, color_scheme);

                            bool overwrite = p == 0;
                            fileIO.Save2DImageInTiff(filePathIntensityMax, imageStichMax, FLIM_ImgData.acquiredTime, overwrite, saveCh);

                            if (FLIM_ImgData.FLIM_on[ch])
                                for (int i = 0; i < 3; i++)
                                {
                                    int s = fileIO.SaveColorImageInTiff(filePathFLIMMax, bitmapFLIM_Max, FLIM_ImgData.acquiredTime, overwrite, saveCh); //Save one page.
                                    System.Threading.Thread.Sleep(10);
                                    if (s != -1)
                                        break;
                                }
                        } //if saveProjection.

                        if (StopFileOpening)
                            break;
                    } //page p
                } //if saveChannel = true
            } //channel ch

            if (openprocCtrl)
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

        } //function

        private void saveFLIMImageToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew((Action)delegate
            {
                SaveFLIM(true);
            });
        }

        private void SaveFLIM(bool openprocCtrl)
        {
            String fileName = FLIM_ImgData.fileName.Split('.')[0] + ".flim";
            FileIO fileIO = new FileIO(FLIM_ImgData.State);
            FLIM_ImgData.fullName(currentChannel, false);

            Invoke((Action)delegate
            {
                fileName = fileIO.SaveFLIMFileDialog(fileName);
            });

            if (openprocCtrl)
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

            if (fileName != "")
            {
                if (FLIM_ImgData.nFastZ > 1)
                {
                    for (int i5d = 0; i5d < FLIM_ImgData.FLIM_Pages5D.Length; i5d++)
                    {
                        if (StopFileOpening)
                            break;

                        GotoPage5D(i5d);
                        setupImageUpdateForZStack();
                        UpdateImages(false, false, false, false, false);

                        var error = fileIO.SaveFLIMInTiffZStack(fileName, FLIM_ImgData.FLIM_Pages, FLIM_ImgData.acquiredTime_Pages5D[i5d], i5d == 0, FLIM_ImgData.saveChannels);
                    }
                }
                else
                {
                    for (int i = 0; i < FLIM_ImgData.FLIM_Pages.Length; i++)
                    {
                        if (StopFileOpening)
                            break;

                        GotoPage4D(i);
                        UpdateImages(true, false, false, true);

                        //FLIM_ImgData.CopyFromFLIM_PageToFLIMRaw(i);
                        // FLIM_ImgData.FLIM_Pages[i]
                        var error = fileIO.SaveFLIMInTiff(fileName, FLIM_ImgData.FLIMRaw, FLIM_ImgData.acquiredTime_Pages[i], i == 0, FLIM_ImgData.saveChannels);

                        if (error != 0)
                        {
                            MessageBox.Show("Problem in saving ! Page = " + i);
                            break;
                        }
                    }
                }

                if (openprocCtrl)
                    this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));
            }
        }



        /// <summary>
        /// Batch processing of analysis and open file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BatchProcessingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew((Action)delegate
            {
                BatchProcessing();
            });
        }

        public string[] FLIMfilesInDirectory(bool openFolders)
        {
            FileIO fileIO = new FileIO(FLIM_ImgData.State);
            CheckFolder(fileIO, openFolders, openFolders);

            //DirectoryInfo dinfo = new DirectoryInfo(FLIM_ImgData.pathName);
            //String[] fileNames_flim = Directory.GetFiles(FLIM_ImgData.pathName, FLIM_ImgData.baseName + "*.flim");
            //String[] fileNames_tiff = Directory.GetFiles(FLIM_ImgData.pathName, FLIM_ImgData.baseName + "*.tif");

            //var list1 = fileNames_flim.ToList();
            //list1.AddRange(fileNames_tiff);

            String[] fileNames = FileHandling.GetFiles(FLIM_ImgData.pathName, new string[] {
            FLIM_ImgData.baseName + "*.flim", FLIM_ImgData.baseName + "*.tif"}).ToArray();

            if (fileNames.Length == 0)
            {
                MessageBox.Show("No file matches to: \"" + FLIM_ImgData.pathName + "\\" + FLIM_ImgData.baseName + "*" + FLIM_ImgData.State.Files.extension + "\"");
                return null;
            }

            Array.Sort(fileNames);

            return fileNames;
        }

        public void BatchExporting(bool[] saveFormat, bool[] channelToSave, int[] FastZFormat, bool correctT0EachPage)
        {

            if (saveFormat.All(x => x == false))
                return;
            if (channelToSave.All(x => x == false))
                return;

            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false)); //Turn on stop opening button and turn off control buttons for safety.

            string[] fileNames = FLIMfilesInDirectory(true);


            for (int i = 0; i < fileNames.Length; i++)
            {
                if (!StopFileOpening)
                {
                    var fn1 = Path.GetFileNameWithoutExtension(fileNames[i]);
                    if (!fn1.EndsWith("_max"))
                    {
                        OpenFLIM(fileNames[i], false, false, false);
                        //if (!plot_regular.calc_upon_open) //Not yet calculated.
                        //    CalculateTimecourse(TuningOnStopOpeningButton);

                        SaveCurrentIntensityImage(saveFormat, channelToSave, FastZFormat, correctT0EachPage, false);
                        //SaveCurrentIntensityImage();

                        Application.DoEvents(); //Make the event stoppable with button.
                    }
                }
                else
                    break;
            }

            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));
        }

        public void BatchProcessing()
        {

            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false)); //Turn on stop opening button and turn off control buttons for safety.

            string[] fileNames = FLIMfilesInDirectory(false);

            for (int i = 0; i < fileNames.Length; i++)
            {
                if (!StopFileOpening)
                {
                    OpenFLIM(fileNames[i], false, plot_regular.calc_upon_open, i == 0);
                }
                else
                    break;
            }

            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));
        }

        private void makeMoviesWithTheSameBaseNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] fileNames = FLIMfilesInDirectory(false);

            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

            var task1 = Task.Factory.StartNew((Action)delegate
           {
               ConcatenateFiles(fileNames);
           });
        }

        private void ConcatenateFiles(string[] fileNames)
        {
            string filenameTail = "_ALL";

            FLIMData flim_data = new FLIMData(FLIM_ImgData.State);

            flim_data.baseName = FLIM_ImgData.baseName;
            flim_data.PutROIs(FLIM_ImgData.CopyROIs());

            FLIMData.FileType file_type = FLIMData.FileType.TimeCourse;

            //if ((FLIM_ImgData.nFastZ > 1 || FLIM_ImgData.ZStack) && !displayZProjection)
            //    file_type = FLIMData.FileType.TimeCourse_ZStack;

            FLIM_ImgData.KeepPagesInMemory = true;
            flim_data.KeepPagesInMemory = true;
            int savePage = FLIM_ImgData.currentPage;
            bool saveStack = displayZProjection;

            int FileN = 0;

            if (fileNames == null)
                return;

            if (!FLIM_ImgData.numberedFile)
            {
                MessageBox.Show("Only numbered file works for this");
                return;
            }

            foreach (String fileName in fileNames)
            {
                var fileNameWithoutPath = Path.GetFileNameWithoutExtension(fileName);
                var fileNum = fileNameWithoutPath.Substring(FLIM_ImgData.baseName.Length);

                if (Int32.TryParse(fileNum, out int n))
                {
                    if (!StopFileOpening)
                    {
                        //Either stack or current page.
                        OpenFLIM(fileName, false, false, false);
                        bool fast_zThis = FLIM_ImgData.nFastZ > 1;
                        bool z_stackThis = FLIM_ImgData.ZStack;

                        if (file_type == FLIMData.FileType.TimeCourse_ZStack)
                        {
                            if (fast_zThis)
                            {
                                for (int p = 0; p < FLIM_ImgData.n_pages5D; p++)
                                {
                                    flim_data.addToPageAndCalculate5D(flim_data.FLIMRaw5D, flim_data.acquiredTime_Pages5D[p], true, true, FileN, true);
                                    //flim_data.FLIMRaw5D = FLIM_ImgData.FLIM_Pages5D[p];
                                    //flim_data.addCurrentToPage5D(true, true, FileN);
                                    FileN++;
                                }
                            }
                            else if (z_stackThis)
                            {
                                flim_data.Add_AllFLIM_PageFormat_To_FLIM_Pages5D(FLIM_ImgData.FLIM_Pages, FLIM_ImgData.acquiredTime, FileN);
                                FileN++;
                            }

                        }
                        else if (file_type == FLIMData.FileType.TimeCourse && !fast_zThis && !z_stackThis)
                        {

                            var flim_pages = (ushort[][][,,])Copier.DeepCopyArray(FLIM_ImgData.FLIM_Pages);
                            var acquiredTime_page = (DateTime[])FLIM_ImgData.acquiredTime_Pages.Clone();

                            FLIM_ImgData.expandPage(flim_pages.Length);
                            for (int i = 0; i < FLIM_ImgData.FLIM_Pages.Length; i++)
                            {
                                flim_data.PutToPageAndCalculate(flim_pages[i], acquiredTime_page[i], true, true, FileN);
                                FileN++;
                            }
                        }
                        else if (file_type == FLIMData.FileType.TimeCourse && ZStack)
                        {
                            if (saveStack)
                            {
                                this.InvokeIfRequired(o => o.cb_projectionYes.Checked = true);
                                displayZProjection = true;
                            }
                            else

                            {
                                int nextPage = savePage;
                                if (ZStack && nextPage < FLIM_ImgData.n_pages)
                                {
                                    GotoPage4D(nextPage);
                                }
                            }
                            UpdateImages(true, false, false, true);
                            flim_data.LoadFLIMRawFromData4D(FLIM_ImgData.FLIMRaw, FLIM_ImgData.acquiredTime, false);
                            flim_data.addCurrentFLIMRawToPage4D(true, FileN, true);
                            flim_data.ZStack = false;
                            flim_data.State.Acq.ZStack = false;
                            flim_data.State.Acq.fastZScan = false;
                            ZStack = false;
                            FileN++;
                        }
                    }
                    else
                        break;
                }
                Application.DoEvents(); //Make the event stoppable with button.0
            }

            FLIM_ImgData = flim_data;
            displayZProjection = false;
            this.InvokeIfRequired(o => o.cb_projectionYes.Checked = false);

            FLIM_ImgData.fileCounter = 0;
            FLIM_ImgData.baseName = FLIM_ImgData.baseName + filenameTail;
            FLIM_ImgData.State.Files.baseName = FLIM_ImgData.baseName + filenameTail;
            FLIM_ImgData.numberedFile = false;

            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false));

            if (FLIM_ImgData.nFastZ <= 1)
                GotoPage4D(0);
            else
                GotoPage5D(0);

            UpdateImages(true, realtime, focusing, true, true);
            SaveFLIM(true);
        }

        private void timeBinningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var h = Microsoft.VisualBasic.Interaction.InputBox("Time binning factor", "Binning", binning.ToString());

            if (!Int32.TryParse(h, out binning))
            {
                return;
            }
            BinFrames(binning);
        }

        public void BinFrames(int binning)
        {

            if (binning < 1 || binning > FLIM_ImgData.n_pages)
            {
                MessageBox.Show("Binnig factor needs to be > 0 and < #pages (" + FLIM_ImgData.n_pages + ")");
                return;
            }

            if (FLIM_ImgData.nFastZ > 1)
            {
                int finalPageSize = FLIM_ImgData.n_pages5D / binning;
                for (int i = 0; i < finalPageSize; i++)
                {
                    ushort[][][,,] ave = FLIM_ImgData.FLIM_Pages5D[i * binning]; //This is linearized!

                    for (int j = 1; j < binning; j++)
                    {
                        for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                            for (int z = 0; z < FLIM_ImgData.nFastZ; z++)
                                MatrixCalc.ArrayCalc(ave[ch][z], FLIM_ImgData.FLIM_Pages5D[i * binning + j][ch][z], CalculationType.Add);
                    }

                    FLIM_ImgData.addToPageAndCalculate5D(ave, FLIM_ImgData.acquiredTime_Pages5D[i * binning], true, true, i, false);


                    GotoPage5D(i);
                    //FLIM_ImgData.calculateAll();

                    //GotoPage(i);
                    UpdateImages(false, false, false, false);
                }

                FLIM_ImgData.resizePage5D(finalPageSize);
                GotoPage5D(0);
            }
            else
            {
                int finalPageSize = FLIM_ImgData.n_pages / binning;

                for (int i = 0; i < finalPageSize; i++)
                {
                    ushort[][,,] ave = FLIM_ImgData.FLIM_Pages[i * binning];

                    for (int j = 1; j < binning; j++)
                    {
                        for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                            MatrixCalc.ArrayCalc(ave[ch], FLIM_ImgData.FLIM_Pages[i * binning + j][ch], CalculationType.Add);
                    }

                    FLIM_ImgData.LoadFLIMRawFromData4D(ave, FLIM_ImgData.acquiredTime_Pages[i], false);
                    FLIM_ImgData.addCurrentFLIMRawToPage4D(true, i, false);

                    GotoPage4D(i);
                    FLIM_ImgData.calculateAll();

                    //GotoPage(i);
                    UpdateImages(false, false, false, false);
                }

                FLIM_ImgData.resizePage(finalPageSize);
                GotoPage4D(0);
            }

            UpdateImages(true, false, false, true, true);
        }

        private void deleteCurrentPageToolStripMenuItem_Click(object sender, EventArgs e)
        {

            int page_to_delete = (FLIM_ImgData.nFastZ > 1) ? FLIM_ImgData.currentPage5D : FLIM_ImgData.currentPage;
            int n_pages = (FLIM_ImgData.nFastZ > 1) ? FLIM_ImgData.n_pages5D : FLIM_ImgData.n_pages;

            if (0 <= page_to_delete && page_to_delete < n_pages && n_pages > 2)
            {
                if (FLIM_ImgData.nFastZ > 1)
                {
                    FLIM_ImgData.Delete5DFLIM(page_to_delete);
                }
                else
                {
                    FLIM_ImgData.Delete4DFLIM(page_to_delete);
                }
            }

            UpdateImages(true, false, false, true, true);
        }


        private void UpdateImage_Simple(object sender, EventArgs e)
        {
            UpdateImages(false, realtime, focusing, true);
        }

        private void LifetimeDecayPlot_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                int ch = currentChannel;
                bool showFit = (fittingDone && !realtime);

                if (FLIM_ImgData.RoiFit.flim_parameters.LifetimeX == null ||
                    FLIM_ImgData.RoiFit.flim_parameters.LifetimeX.Length <= ch)
                    return;

                if (FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[ch].Length < 2)
                    return;

                PlotOnPanel pp = new PlotOnPanel(e, LifetimeCurvePlot.Width, LifetimeCurvePlot.Height);
                pp.plotType = "-";
                pp.addData(FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[ch], FLIM_ImgData.RoiFit.flim_parameters.LifetimeY[ch], plotPen, "-");

                bool roi_plot = roi_State != drawROI_State.NoROI &&
                    Values_selectedROI.Checked && FLIM_ImgData.Fit_type == FLIMData.FitType.GlobalRois
                    && FLIM_ImgData.currentRoi >= 0 && FLIM_ImgData.currentRoi < FLIM_ImgData.ROIs.Count;

                if (roi_plot && FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters.LifetimeY[ch] != null)
                    pp.addData(FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters.LifetimeX[ch],
                        FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters.LifetimeY[ch], plotPen2, "-");

                if (showFit && fittingDone)
                {
                    if (FLIM_ImgData.RoiFit.flim_parameters.fitCurve[ch] != null)
                        pp.addData(FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[ch], FLIM_ImgData.RoiFit.flim_parameters.fitCurve[ch], fitPen, "-");
                    if (roi_plot)
                    {
                        if (FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters.fitCurve[ch] != null)
                            pp.addData(FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters.LifetimeX[ch],
                                FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters.fitCurve[ch], fitPen2, "-");
                    }
                }

                //pp.logScaleX = true;
                pp.logScaleY = logScale.Checked;
                pp.XTitle = "Time Point";
                pp.YTitle = "Number of Photons";
                pp.yFrac = 0.55F;
                pp.yMergin = 0.35F;
                pp.SetAxisScale(0, FLIM_ImgData.State.Spc.spcData.n_dataPoint, 0);
                pp.autoScaleX = false;
                pp.XTickSpace = 20 * Math.Ceiling(pp.Xmax / 128);
                pp.noTickLabelX = true;
                pp.AutoScaleNowY(true);
                pp.Log_ReplaceNegativewith_Y = 0.1;
                //pp.plotSize = 8;
                pp.plot(e);

                PlotOnPanel pp2 = new PlotOnPanel(e, LifetimeCurvePlot.Width, LifetimeCurvePlot.Height);
                pp2.plotType = "-";
                pp2.addData(FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[ch], FLIM_ImgData.RoiFit.flim_parameters.residual[ch], plotPen, "-");
                pp2.YTitle = "Residuals";
                pp2.yFrac = 0.18F;
                pp2.yMergin = 0.12F;
                pp2.autoScaleX = false;
                pp2.SetAxisScale(0, FLIM_ImgData.State.Spc.spcData.n_dataPoint, 0);
                pp2.XTickSpace = pp.XTickSpace;
                pp2.nTickRY = 2;
                pp2.noTitleX = true;
                pp2.noPlot = !showFit;
                pp2.plot(e);
            }
            catch (Exception Ex)
            {
                //MessageBox.Show(Ex.Message);
                //This is necessary in case plotting failed.
            }
        }

        private void TurnOffDuringOpeningProc(bool fishinedOpening)
        {
            PageUp.Enabled = fishinedOpening;
            PageDown.Enabled = fishinedOpening;
            PageUpUp.Enabled = fishinedOpening;
            PageDownDown.Enabled = fishinedOpening;

            StopFileOpening = false; //This way, you can stop the process anywhere you want.
            stopOpening.Visible = !fishinedOpening;
            stopOpening.Enabled = !fishinedOpening;

            FastZUp.Enabled = fishinedOpening;
            FastZDown.Enabled = fishinedOpening;

            //TC_Reset.Enabled = fishinedOpening;
            //CalcTimeCourse.Enabled = fishinedOpening;
            //DeleteCurrent.Enabled = fishinedOpening;
        }

        private FileIO.FileError OpenFLIM()
        {
            FileIO.FileError file_error = FileIO.SetupFLIMOpeningDlog(lastFilePath, out int nPages, out String fileName);

            if (file_error == FileIO.FileError.Success)
            {
                FLIM_ImgData.fileExtension = Path.GetExtension(fileName);

                return OpenFLIM(fileName, true, plot_regular.calc_upon_open, true);
            }
            else
                return file_error;
        }

        public bool ROI_DoingSomeghing()
        {
            return roi_State == drawROI_State.Moving || roi_State == drawROI_State.Moving_ExistingROI ||
                roi_State == drawROI_State.Creating || roi_State == drawROI_State.Resizing || roi_State == drawROI_State.Resizing_ExistingROI;
        }

        public FileIO.FileError OpenFLIM(String fn, bool TuningOnStopOpeningButton, bool calcTimeCourse, bool read_data)
        {
            image_display_state = ImageDisplay_State.Opening;
            var fIO = new FileIO(FLIM_ImgData.State);
            var saveState = fIO.CopyState();

            string oldBaseName = FLIM_ImgData.baseName;

            int nPages = FileIO.SetupFLIMOpening(fn, out string Header); //NPages = number of directory.
                                                                         //FLIM_ImgData.decodeHeader(Header);

            bool saveProjection = displayZProjection;
            int saveCurrentPage = FLIM_ImgData.currentPage;

            lastFilePath = fn;

            if (nPages < 0)
                return FileIO.FileError.UnKnown;

            if (fn.Length == 0)
                return FileIO.FileError.NotFound;

            FLIM_ImgData.KeepPagesInMemory = keepPagesInMemoryToolStripMenuItem.Checked;

            FileIO.FileError ret = FileIO.FileError.UnKnown;

            if (TuningOnStopOpeningButton)
            {
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false));
            }

            this.InvokeIfRequired(o => o.plot_regular.Show());


            int[] save_nAverage = (int[])FLIM_ImgData.nAveragedFrame.Clone();
            int[] file_nAverage;

            if (TCF.FileName == null)
                TCF.FileName = "";

            Debug.WriteLine(TCF.FileName);
            if (TCF.FileName.Equals(TempFileName)) //Very first file.
                TCF.DeleteAll();


            Task openTask = Task.Factory.StartNew(() =>
            {
                //Used to open a file. Anyway I removed it.... (now after openTask.wait()).
            });
            openTask.Wait();

            bool OpenSecondFile = false;
            string fn2 = "";
            //This will open the file ---- and initialized. ROIs associated with FLIM_ImgData is preserved.
            ret = FileIO.OpenFLIMTiffFilePage(fn, 0, 0, FLIM_ImgData, true, true); //Save In Mmeory for the first page --- we don't know the format yet. 
            SetFitting_Param(true);

            if (FLIM_ImgData.nFastZ > 1)
                FLIM_ImgData.expandArray5D(nPages);
            else
                FLIM_ImgData.expandPage(nPages);

            FLIM_ImgData.State.Display = saveState.Display;

            if (FLIM_ImgData.format == FileIO.FileFormat.None) //TIF file.
            {
                int nChannelSaved = FLIM_ImgData.saveChannels.Select(x => x ? 0 : 1).ToArray().Sum();
                if (FLIM_ImgData.nChannels > 1)
                {
                    if (nChannelSaved == 1)
                    {
                        if (fn.Contains("_Ch1_"))
                        {
                            fn2 = fn.Replace("_Ch1_", "_Ch2_");
                        }
                        else if (fn.Contains("_Ch2_"))
                        {
                            fn2 = fn.Replace("_Ch2_", "_Ch1_");
                        }
                        if (File.Exists(fn2))
                        {
                            OpenSecondFile = true;
                            ret = FileIO.OpenFLIMTiffFilePage(fn2, 0, 0, FLIM_ImgData, false, true);
                        }
                    }
                    else if (nChannelSaved > 1)
                    {
                        nPages = nPages / nChannelSaved;
                    }
                }
            }

            if (ret != FileIO.FileError.Success)
                return ret;


            ZStack = FLIM_ImgData.State.Acq.ZStack && FLIM_ImgData.State.Acq.nSlices > 1 && FLIM_ImgData.State.Acq.sliceStep != 0;
            FastZStack = FLIM_ImgData.State.Acq.fastZScan && FLIM_ImgData.State.Acq.FastZ_nSlices > 0;

            this.InvokeIfRequired(o => o.SetFastZModeDisplay(FastZStack));


            //if (FLIM_ImgData.fileCounter == 1 && (ZStack || FastZStack))
            //{
            //    this.InvokeIfRequired(o => o.cb_projectionYes.Checked = true);
            //    displayZProjection = true;
            //}

            //Uncaging revcovery
            uncagingLocFrac = (double[])FLIM_ImgData.State.Uncaging.Position.Clone();

            uncagingLocs.Clear();

            if (FLIM_ImgData.State.Uncaging.multiUncagingPosition)
            {
                for (int j = 0; j < FLIM_ImgData.State.Uncaging.UncagingPositionsX.Length; j++)
                {
                    if (j < FLIM_ImgData.State.Uncaging.UncagingPositionsY.Length)
                        uncagingLocs.Add(new double[] { FLIM_ImgData.State.Uncaging.UncagingPositionsX[j], FLIM_ImgData.State.Uncaging.UncagingPositionsY[j] });
                }
            }

            //FLIM_ImgData.RoiFit.flim_parameters.ChangeChannelNumber(FLIM_ImgData.nChannels);
            FLIM_ImgData.ResetFitting(true);

            file_nAverage = (int[])FLIM_ImgData.nAveragedFrame.Clone();
            FLIM_ImgData.nAveragedFrame = save_nAverage;
            ApplyTextToRange(false);
            FLIM_ImgData.nAveragedFrame = file_nAverage;

            SetupOpenedFile();

            try
            {
                if (read_data)
                    ReadTimeCourse();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problem in Reading time course (...), " + ex.Message);
            }

            if (FLIM_ImgData.ROIs.Count == 0 || oldBaseName != FLIM_ImgData.baseName)
            {
                ReadRois(roi_State == drawROI_State.NoROI);
                if (FLIM_ImgData.ROIs.Count > 0 && FLIM_ImgData.currentRoi >= 0 && FLIM_ImgData.currentRoi < FLIM_ImgData.ROIs.Count)
                {
                    copyROIVectors(FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi], ref FLIM_ImgData.Roi);
                    //FLIM_ImgData.Roi = FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi];
                    roi_State = drawROI_State.Idle;
                }
                DrawImages();
            }

            for (short i = 1; i < nPages + 1; i++)
            {
                int pre = i - 1;
                if (openTask != null)
                    openTask.Wait();
                openTask.Dispose();


                if (ZStack && !FastZStack)
                {
                    if (i < nPages)
                    {
                        openTask = Task.Factory.StartNew((object obj) =>
                            {
                                var data = (dynamic)obj;
                                ret = FileIO.OpenFLIMTiffFilePage(fn, (short)data.i2, data.i2, FLIM_ImgData, false, true);
                                if (OpenSecondFile)
                                    ret = FileIO.OpenFLIMTiffFilePage(fn2, (short)data.i2, data.i2, FLIM_ImgData, false, true);
                            }, new { i2 = i });
                    }

                    if (ret != FileIO.FileError.Success)
                        return ret;

                    if (pre >= 0 && pre < nPages && nPages > 1)
                    {
                        //if (!displayZProjection)
                        {
                            GotoPage4D(pre);
                            UpdateImages(true, false, false, true);
                        }
                    }
                }
                else if (FastZStack && FLIM_ImgData.imagesPerFile == 1) //Obsolete. 
                {
                    if (i < nPages)
                    {
                        int page1 = i / FLIM_ImgData.State.Acq.FastZ_nSlices;
                        int pageZ = i % FLIM_ImgData.State.Acq.FastZ_nSlices;

                        openTask = Task.Factory.StartNew((object obj) =>
                        {
                            var data = (dynamic)obj;
                            ret = FileIO.OpenFLIMTiffFilePage(fn, (short)data.i2, pageZ, FLIM_ImgData, false, true);
                            if (OpenSecondFile)
                                ret = FileIO.OpenFLIMTiffFilePage(fn2, (short)data.i2, pageZ, FLIM_ImgData, false, true);

                            if (pageZ == FLIM_ImgData.nFastZ - 1)
                            {
                                //Last page. Copy to FLIM_Pages5D directly.
                                FLIM_ImgData.Add_AllFLIM_PageFormat_To_FLIM_Pages5D(FLIM_ImgData.FLIM_Pages, FLIM_ImgData.acquiredTime_Pages[0], page1);
                            }
                        }, new { i2 = i });
                    }

                    if (pre >= 0 && pre < nPages)
                    {
                        int page1 = pre / FLIM_ImgData.State.Acq.FastZ_nSlices;
                        int pageZ = pre % FLIM_ImgData.State.Acq.FastZ_nSlices;

                        if (pageZ == FLIM_ImgData.nFastZ - 1) //Last Frame.
                        {
                            GotoPage5D(page1); //This should be already made.

                            if (displayZProjection)
                                calcZProjection();
                            else
                                GotoPage4D(pageZ);

                            UpdateImages(true, false, false, true, true);

                            if (calcTimeCourse)
                            {
                                CalculateTimecourse(!TuningOnStopOpeningButton);
                                plot_regular.updatePlot();
                                SaveTimeCourse();
                            }

                            if (!FLIM_ImgData.KeepPagesInMemory)
                                break;
                        }
                        else
                        {
                            if (!displayZProjection)
                            {
                                GotoPage4D(pageZ);
                                UpdateImages(true, false, false, true);
                            }
                        }
                    }
                }
                else if (FastZStack)
                {
                    if (i < nPages)
                    {
                        openTask = Task.Factory.StartNew((object obj) =>
                        {
                            var data = (dynamic)obj;
                            ret = FileIO.OpenFLIMTiffFilePage(fn, (short)data.i2, data.i2, FLIM_ImgData, false, true);
                            if (OpenSecondFile)
                                ret = FileIO.OpenFLIMTiffFilePage(fn2, (short)data.i2, data.i2, FLIM_ImgData, false, true);
                        }, new { i2 = i });
                    }

                    if (pre >= 0 && pre < nPages && nPages > 1)
                    {
                        GotoPage5D(pre);
                        setupImageUpdateForZStack();
                        FLIM_ImgData.State.Display = saveState.Display;
                        UpdateImages(true, false, false, true, true);

                        if (calcTimeCourse)
                        {
                            CalculateCurrentPage(pre, true);
                            plot_regular.updatePlot();
                            SaveTimeCourse();
                        }

                        if (!FLIM_ImgData.KeepPagesInMemory)
                            break;

                    }
                }
                else //!ZStack.
                {
                    if (i < nPages)
                    {
                        openTask = Task.Factory.StartNew((object obj) =>
                        {
                            var data = (dynamic)obj;
                            ret = FileIO.OpenFLIMTiffFilePage(fn, (short)data.i2, data.i2, FLIM_ImgData, false, FLIM_ImgData.KeepPagesInMemory);
                            if (OpenSecondFile)
                                ret = FileIO.OpenFLIMTiffFilePage(fn2, (short)data.i2, data.i2, FLIM_ImgData, false, FLIM_ImgData.KeepPagesInMemory);
                        }, new { i2 = i });
                    }

                    if (ret != FileIO.FileError.Success)
                        return ret;

                    if (pre >= 0 && pre < nPages && nPages > 1)
                    {
                        GotoPage4D(pre);
                        FLIM_ImgData.State.Display = saveState.Display;
                        UpdateImages(true, false, false, true, true);

                        if (calcTimeCourse)
                        {
                            CalculateCurrentPage(pre, true);
                            plot_regular.updatePlot();
                            SaveTimeCourse();
                        }

                        if (!FLIM_ImgData.KeepPagesInMemory)
                            break;
                    }
                }

                if (i < nPages)
                {
                    Application.DoEvents();

                    if (StopFileOpening)
                    {
                        if (FLIM_ImgData.KeepPagesInMemory)
                        {
                            if (FastZStack)
                                FLIM_ImgData.resizePage5D(i);
                            else
                                FLIM_ImgData.resizePage(i);
                        }
                        break;
                    }
                }
            } //End page


            page_range[0] = 0;

            if (!FastZStack)
                page_range[1] = FLIM_ImgData.n_pages;
            else
                page_range[1] = FLIM_ImgData.nFastZ;

            if (!ZStack && !FastZStack) //Time course. Show the first image.
                GotoPage4D(0);

            this.InvokeIfRequired(o => o.PageEnableControl());

            if (ZStack || FastZStack)
            {
                if (displayZProjection)
                    calcZProjection();
                else
                {
                    if (saveCurrentPage < FLIM_ImgData.n_pages)
                        saveCurrentPage = 0;

                    GotoPage4D(saveCurrentPage);
                }
            }

            FLIM_ImgData.State.Display = saveState.Display;
            SetFitting_Param(true);
            UpdateImages(true, false, false, true);

            if (TuningOnStopOpeningButton)
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

            if (calcTimeCourse && (ZStack || nPages == 1))
                CalculateTimecourse(false); //Include the saving.

            this.InvokeIfRequired(o => o.displayFastZParam()); //only if FastZStack == true. Included in the function.
            this.InvokeIfRequired(o => o.TurnOnOffThreeD(ThreeDRoi));
            this.InvokeIfRequired(o =>
            {
                o.toggleZStackAndTimecourseToolStripMenuItem.Text = o.ZStack ? ZStack_Text[1] : ZStack_Text[0];
            });

            image_display_state = ImageDisplay_State.Idle;

            try
            {
                PostOpenUserFunction();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Problem in Post Open User Function:" + e.Message);
            }

            return ret;
        }

        /// <summary>
        /// Calculate phase by intensity.
        /// </summary>
        public void CalculatePhase()
        {
            int half_n = FLIM_ImgData.n_pages / 2;
            int n = FLIM_ImgData.n_pages;

            var IntenstiyIndexM = new double[n];
            FLIM_ImgData.CalculateAllPages_Direct(true);

            int shift = half_n / 4;

            for (int i = 0; i < n; i++)
                IntenstiyIndexM[i] = (double)MatrixCalc.MatrixSum(FLIM_ImgData.Project_Pages[i][currentChannel]);

            var maxInt = IntenstiyIndexM.Max();

            for (int i = 0; i < n; i++)
                IntenstiyIndexM[i] /= maxInt;

            var similarities = new double[2 * shift + 1];

            int maxShift = -shift;
            double maxOverlap = 0;

            for (int j = -shift; j <= shift; j++)
            {
                double overlap = 0;
                for (int i = 0; i < half_n; i++)
                {
                    int img1 = i + j + n;
                    int img2 = n - i - 1 + j;
                    img1 = img1 % n;
                    img2 = img2 % n;

                    overlap = overlap + IntenstiyIndexM[img1] * IntenstiyIndexM[img2];
                }
                overlap = overlap * 100.0 / half_n;

                if (overlap > maxOverlap)
                {
                    maxOverlap = overlap;
                    maxShift = j;
                }

                similarities[j + shift] = overlap;
            }

            double[] result = MatrixCalc.FindPeak_WithGaussianFit1D(similarities);
            double xp = result[1];
            double yp = result[0];

            double best_phase = FLIM_ImgData.State.Acq.FastZ_Phase[0] + 360 * (xp - shift) / n;

            if (Double.IsNaN(yp))
                best_phase = Double.NaN;

            PhaseDetectionMode_Status.Text = String.Format("Phase matching = {0:0.0}%", similarities[shift]);
            PhaseDetectionMode_Status2.Text = String.Format("Best phase = {1:0} deg ({0:0.0}%)", yp, best_phase);

        }


        private void CalcPhase_Click(object sender, EventArgs e)
        {
            if (FastZStack && FLIM_ImgData.State.Acq.FastZ_phase_detection_mode)
                CalculatePhase();
        }


        /// <summary>
        /// This function calculate the best phase for tag-lens based imaging based on similarity.
        /// This did not work so well.
        /// </summary>
        public void CalculatePhase_Similarity()
        {
            int half_n = FLIM_ImgData.n_pages / 2;
            int n = FLIM_ImgData.n_pages;

            var similarityIndexM = new double[half_n];
            FLIM_ImgData.CalculateAllPages_Direct(true);
            double similarity = 0;
            int shift = half_n / 4;

            var similarities = new double[2 * shift + 1];

            int maxShift = -shift;
            double maxShim = 0;

            for (int j = -shift; j <= shift; j++)
            {
                for (int i = 0; i < half_n; i++)
                {
                    int img1 = i + j + n;
                    int img2 = n - i - 1 + j;
                    img1 = img1 % n;
                    img2 = img2 % n;

                    similarityIndexM[i] = MatrixCalc.Matrix_similarity<ushort>(FLIM_ImgData.Project_Pages[img1][currentChannel], FLIM_ImgData.Project_Pages[img2][currentChannel]);
                }

                similarity = Math.Sqrt(similarityIndexM.Sum() / half_n) * 100.0;
                if (similarity > maxShim)
                {
                    maxShim = similarity;
                    maxShift = j;
                }

                similarities[j + shift] = similarity;
            }

            //Try 2nd order correction. y = a*x^2 + b*x + c
            double y0 = similarities[maxShift + shift];
            double yn = 0;
            double peakValue = y0;
            double xp = 0;
            if (maxShift + shift - 1 >= 0)
            {
                yn = similarities[maxShift + shift - 1];

                double yp = similarities[maxShift + shift + 1];
                double a = (yn + yp) / 2.0;
                double b = (yp - yn) / 2.0;
                double c = y0;
                xp = b / 2.0 / a;
                peakValue = a * xp * xp + b * xp + c;
            }
            double best_phase = FLIM_ImgData.State.Acq.FastZ_Phase[0] + 360 * (maxShift + xp) / n;

            if (Double.IsNaN(peakValue))
                best_phase = Double.NaN;

            PhaseDetectionMode_Status.Text = String.Format("Phase matching = {0:0.0}%", similarities[shift]);
            PhaseDetectionMode_Status2.Text = String.Format("Best phase = {1:0} deg ({0:0.0}%)", peakValue, best_phase);


        }

        public void displayFastZParam()
        {
            if (FastZStack)
            {
                FastZPhaseText.Text = String.Format("{0:0}", FLIM_ImgData.State.Acq.FastZ_Phase[0]);
                FastZAmpText.Text = String.Format("{0:0.0}%", FLIM_ImgData.State.Acq.FastZ_Amp);
                FastZFreqText.Text = String.Format("{0:0}kHz", FLIM_ImgData.State.Acq.FastZ_Freq / 1000.0);
                FastZPhasePanel.Visible = true;

                PhaseDetectionMode_Status.Text = "";
                PhaseDetectionMode_Status2.Text = "";

                if (FLIM_ImgData.State.Acq.FastZ_phase_detection_mode)
                {
                    PhaseDetectionMode_Status.Text = "Calculating phase matching...";
                    Application.DoEvents();
                    CalculatePhase();
                }

            }
            else
            {
                FastZPhasePanel.Visible = false;
            }
        }

        /// <summary>
        /// Default setting for CalculateCurrentPage functionl.
        /// </summary>
        public void CalculateCurrentPage(bool turning_onoff_stop)
        {
            int page = FLIM_ImgData.currentPage;

            if (page < 0)
                page = 0;
            else if (page >= FLIM_ImgData.n_pages)
                page = FLIM_ImgData.n_pages - 1;

            if (!ZStack && !FastZStack)
            {
                CalculateCurrentPage(page, true);
                SaveTimeCourse();
            }
            else
            {
                Task.Factory.StartNew((Action)delegate
                {
                    CalculateTimecourse(turning_onoff_stop);
                });
            }

        }

        /// <summary>
        /// This calculate the time course of current page.
        /// </summary>
        /// <param name="page">Page number --- this is to insert the values in the right place.</param>
        /// <param name="firstPage">Will do first page specific task. Currently not used.</param>
        /// <param name="updateTCF">Update time course. Perhaps it is always true </param>
        public void CalculateCurrentPage(int page, bool doEverything)
        {
            realtime = false;
            //FLIMData.FitType ft = FLIM_ImgData.Fit_type;

            //For now, first-page is not used.

            bool page_direct = FLIM_ImgData.PageDirect(page);
            bool save_event = doEverything;

            double[] offsetC = FLIM_ImgData.offset;

            //FLIM_ImgData.ResetLifetimeCalculation(false);

            if (plot_regular.calc_Fit)
            {
                double[] fit_offset = (double[])FLIM_ImgData.offset;

                if (doEverything)
                {
                    if (FLIM_ImgData.ROIs.Count > 0)
                    {
                        TurnOnMultiROI(true);
                        FLIM_ImgData.Fit_type = FLIMData.FitType.GlobalRois;
                    }
                    else
                    {
                        TurnOnMultiROI(false);
                        FLIM_ImgData.Fit_type = FLIMData.FitType.SelectedRoi;
                    }
                }

                if (FLIM_ImgData.State.Acq.acqFLIMA.Any(x => x == true))
                    fit_offset = FitData(false, save_event, page);

                if (AutoApplyOffset.Checked)
                    offsetC = fit_offset;
            }


            FLIM_ImgData.calculate_MeanLifetime(page, offsetC); //This calculates mean lifetime (without fitting).

            //FLIM_ImgData.Fit_type = ft;

            if (!page_direct)
            {
                ImageInfo info = new ImageInfo(FLIM_ImgData);
                if (!TC.AddFile(info, page)) //If new file, it will create a new Time course.
                {
                    TC = new TimeCourse();
                    TC.AddFile(info, page);
                }
            }
            else
            {
                TC = new TimeCourse();
                TC.AddFLIM(FLIM_ImgData, page);
            }

            if (doEverything)
            {
                if (page_direct)
                    FLIM_ImgData.RoiFit.flim_parameters = FLIM_ImgData.RoiFit.flim_parameters_Pages[page].Copy();

                if (AutoApplyOffset.Checked)
                {
                    FLIM_ImgData.offset = (double[])offsetC.Clone();
                    FLIM_ImgData.State.Spc.analysis.offset = (double[])FLIM_ImgData.offset.Clone();
                }
                fittingDone = true;
                TCF.AddFile(TC);
                TCF.calculate();
                plot_regular.plotNow_noRealtime(TCF, TC, this, currentChannel);
            }

        }

        /// <summary>
        /// Calculate the time course of the current file.
        /// </summary>
        public void CalculateTimecourse(bool turning_onoff_stop)
        {

            FastZStack = FLIM_ImgData.nFastZ > 1;

            int nPages = FLIM_ImgData.n_pages;
            int savePage = FLIM_ImgData.currentPage;

            if (FastZStack)
                nPages = FLIM_ImgData.n_pages5D;

            if (turning_onoff_stop)
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false));

            //FLIM_ImgData.ResetLifetimeCalculation(false);

            if (ZStack)
            {
                nPages = 1;
                //savePage = 0;
            }

            if (FastZStack)
            {
                setupImageUpdateForZStack();
                UpdateImages(true, false, false, true);
            }

            if (ZStack || FastZStack)
                for (int i = 0; i < nPages; i++)
                {
                    if (FastZStack)
                    {
                        GotoPage5D(i);
                        FLIM_ImgData.currentPage = savePage;
                        UpdateImages(true, false, false, true);
                        CalculateCurrentPage(-1, true);
                    }
                    else if (ZStack)
                    {
                        if (displayZProjection)
                        {
                            calcZProjection();
                            UpdateImages(true, false, false, true);
                        }
                        else
                        {
                            GotoPage4D(FLIM_ImgData.currentPage);
                            UpdateImages(true, false, false, true);
                        }
                        CalculateCurrentPage(0, true);
                    }

                    //plot_regular.Refresh();
                    //plot_regular.plotNow_noRealtime(TCF, TC, this, currentChannel); Should be included.

                    Application.DoEvents();

                    if (StopFileOpening)
                    {
                        if (FLIM_ImgData.KeepPagesInMemory)
                            FLIM_ImgData.n_pages = FLIM_ImgData.FLIM_Pages.Length;
                        break;
                    }
                }//
            else
            {
                TurnOnMultiROI(true);
                if (FLIM_ImgData.ROIs.Count > 0)
                    FLIM_ImgData.Fit_type = FLIMData.FitType.GlobalRois;
                else
                    FLIM_ImgData.Fit_type = FLIMData.FitType.SelectedRoi;

                FLIM_ImgData.InitializeAllROI_FlimParameters_Pages();
                int step = 4;
                int k = 0;
                for (k = 0; k <= nPages - step; k += step)
                {
                    Parallel.For(k, k + step,
                        (i) =>
                        {
                            CalculateCurrentPage(i, false);
                        });

                    TCF.AddFile(TC);
                    TCF.calculate();
                    plot_regular.plotNow_noRealtime(TCF, TC, this, currentChannel);
                    fittingDone = true;
                    FLIM_ImgData.RoiFit.transfer_FittingParameterFromPage(k);
                    GotoPage4D(k);
                    UpdateImages(true, false, false, true);
                    //LifetimeCurvePlot.Invalidate();
                    //this.InvokeIfRequired(o => o.c_page.Text = k.ToString());
                    Application.DoEvents();
                    if (StopFileOpening)
                        break;
                }

                for (; k < nPages; k++)
                {
                    if (StopFileOpening)
                        break;

                    CalculateCurrentPage(k, true);
                    fittingDone = true;
                    LifetimeCurvePlot.Invalidate();
                    this.InvokeIfRequired(o => o.c_page.Text = k.ToString());
                    Application.DoEvents();
                }
            }


            TCF.AddFile(TC);
            TCF.calculate();
            //realtime_plot.Invalidate();
            plot_regular.plotNow_noRealtime(TCF, TC, this, currentChannel);


            if (!ZStack && !FastZStack || !displayZProjection)
            {
                GotoPage4D(savePage);
                UpdateImages(true, false, false, true);
                FitData(false, true, -1);
            }

            if (turning_onoff_stop)
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

            //realtime_plot.Invalidate();

            //plot_regular.plotNow_noRealtime(TCF, TC, this, currentChannel);
            //FLIM_ImgData.calculateAll();

            try
            {
                SaveTimeCourse();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Problem in saving time course:" + e.Message);
            }
            finally { }

        }

        /// <summary>
        /// Display setting is different for FastZ imaging.
        /// </summary>
        /// <param name="on"></param>
        public void SetFastZModeDisplay(bool on)
        {
            if (on)
            {

                FrameSlicePanel.Location = new Point(185, 102);
                FrameSlicePanel.Text = "Fast Z Stack";
                FastZPanel.Visible = true;


                if (FLIM_ImgData.currentPage < 0 && FLIM_ImgData.nFastZ > 1)
                {
                    if (!FLIM_ImgData.State.Acq.FastZ_phase_detection_mode)
                        FLIM_ImgData.currentPage = FLIM_ImgData.nFastZ / 2;
                    else
                        FLIM_ImgData.currentPage = FLIM_ImgData.nFastZ / 4;
                }
            }
            else
            {
                FrameSlicePanel.Location = new Point(185, 60);
                if (ZStack)
                    FrameSlicePanel.Text = "Z Stack";
                else
                    FrameSlicePanel.Text = "Time series";
                FastZPanel.Visible = false;
            }
        }


        private void FastZUpDown(object sender, EventArgs e)
        {
            FastZUp.Enabled = false;
            FastZDown.Enabled = false;

            if (sender.Equals(FastZUp))
            {
                if (FLIM_ImgData.currentPage5D < FLIM_ImgData.n_pages5D - 1)
                    FLIM_ImgData.currentPage5D++;
            }
            else
            {
                if (FLIM_ImgData.currentPage5D > 0)
                    FLIM_ImgData.currentPage5D--;
            }

            CurrentFastZPageTB.Text = FLIM_ImgData.currentPage5D.ToString();

            GotoPage5D(FLIM_ImgData.currentPage5D);

            displayZProjection = cb_projectionYes.Checked;
            if (cb_projectionYes.Checked)
            {
                calcZProjection();
            }

            UpdateImages(true, false, false, true);

            if (FastZStack && FLIM_ImgData.State.Acq.FastZ_phase_detection_mode)
                CalculatePhase();

            if (plot_regular.calc_upon_open)
                CalculateCurrentPage(false);

            try
            {
                PostFastZUpDownFunction();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problem in Post Open User Function:" + ex.Message);
            }

            FastZUp.Enabled = true;
            FastZDown.Enabled = true;
        }


        private void FileUpDown_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            bool status1 = btn.Enabled;
            btn.Enabled = false;

            int saveCounter = FLIM_ImgData.fileCounter;
            if (sender.Equals(FileDown))
            {
                if (saveCounter == 1)
                {
                    btn.Enabled = status1;
                    return;
                }
                FLIM_ImgData.fileCounter--;
            }
            else if (sender.Equals(FileUp))
                FLIM_ImgData.fileCounter++;


            //FLIM_ImgData.fullName(currentChannel, FLIM_ImgData.State.Files.channelsInSeparatedFile);

            String fn = FLIM_ImgData.fullName(currentChannel, FLIM_ImgData.State.Files.channelsInSeparatedFile);


            bool success = false;
            try
            {
                success = OpenFLIM(fn, true, plot_regular.calc_upon_open, false) == FileIO.FileError.Success;
            }
            catch
            {
                MessageBox.Show("Problem in opening file");
            }

            if (!success || !File.Exists(fn))
            {
                FLIM_ImgData.fileCounter = saveCounter;
                FLIM_ImgData.fullName(currentChannel, FLIM_ImgData.State.Files.channelsInSeparatedFile);
            }

            btn.Enabled = status1;
        }

        private void RecoverRoisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReadRois(true);
            DrawImages();
        }



        private void saveRoisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAllRois();
        }


        public void AssurePageRange()
        {
            if (page_range[1] < page_range[0])
            {
                int tmp = page_range[0];
                page_range[0] = page_range[1];
                page_range[1] = tmp;
            }

            if (page_range[0] < 0)
                page_range[0] = 0;

            int n_pages = FLIM_ImgData.n_pages;
            if (FLIM_ImgData.nFastZ > 1)
                n_pages = FLIM_ImgData.nFastZ;

            if (page_range[1] > n_pages)
                page_range[1] = n_pages;
        }

        private void UpdateImageParamText()
        {
            ScanParameters State = FLIM_ImgData.State;
            MapStateIntensityRange();
            filterWindow.Text = FLIM_ImgData.State.Display.filterWindow_FLIM.ToString();

            int[] aveNFrame = (int[])FLIM_ImgData.nAveragedFrame.Clone();

            if (displayZProjection && AveProjection.Checked)
            {
                AssurePageRange();
                if (page_range[1] - page_range[0] > 0)
                {
                    for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                        aveNFrame[ch] = aveNFrame[ch] * (page_range[1] - page_range[0] + 1);
                }

            }

            for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                for (int j = 0; j < N_RANGE; j++)
                {
                    MaxMinSliders[ch][j].Maximum = SldrMax * aveNFrame[ch];
                    MaxMinSliders[ch][j].LargeChange = (int)(SldrZoom[ch] * aveNFrame[ch]);
                }

            int c = currentChannel;

            double[][] intensity_rangeA = new double[N_TOTALDISPLAY][];

            intensity_rangeA[0] = State_intensity_range[currentChannel];
            intensity_rangeA[1] = State_FLIM_intensity_range[currentChannel];
            intensity_rangeA[2] = State_FLIM_lifetime_range[currentChannel];

            if (Channel12.Checked)
            {
                intensity_rangeA[0] = State_intensity_range[0];
                intensity_rangeA[1] = State_intensity_range[1];
                intensity_rangeA[2] = State_FLIM_lifetime_range[currentChannel];

            }

            //Slider max setup. First search for maximum values.
            double[] maxVal1 = new double[N_TOTALDISPLAY];

            for (int cl = 0; cl < N_TOTALDISPLAY; cl++)
            {
                MaxMinTextBox[cl][1].Text = intensity_rangeA[cl][1].ToString(); //Get Text value.
                double val1 = intensity_rangeA[cl][1] * SldrZoom[cl]; //Get Slider value.
                if (maxVal1[cl] < val1) //Search for maximum slider values?
                    maxVal1[cl] = val1;

                int NewMaxInt = (int)(2 * maxVal1[cl]);

                bool ChangeSldrMax = false;

                if ((cl == 0 || cl == 1) && cl < FLIM_ImgData.nChannels) //only if intensity. 
                    if (SldrMaxDefault * aveNFrame[cl] > maxVal1[cl])
                    {
                        ChangeSldrMax = true;
                        NewMaxInt = SldrMaxDefault * aveNFrame[cl];
                        SldrMax = SldrMaxDefault;
                    }
                    else if (SldrMax * aveNFrame[cl] > maxVal1[cl])
                    {
                        ChangeSldrMax = false;
                    }
                    else
                    {
                        ChangeSldrMax = true;
                        NewMaxInt = (int)(2 * maxVal1[cl]);
                        SldrMax = NewMaxInt / aveNFrame[cl];
                    }

                for (int j = 0; j < N_RANGE; j++)
                {
                    if (ChangeSldrMax)
                        MaxMinSliders[cl][j].Maximum = NewMaxInt;
                    MaxMinSliders[cl][j].Minimum = 0;
                }

                toggleZStackAndTimecourseToolStripMenuItem.Text = ZStack ? ZStack_Text[1] : ZStack_Text[0];
            }

            //Set to Max of maximum value.


            //Adjust the slider values.


            for (int cl = 0; cl < N_TOTALDISPLAY; cl++)
            {
                for (int j = 0; j < N_RANGE; j++)
                {
                    MaxMinTextBox[cl][j].Text = intensity_rangeA[cl][j].ToString();
                    double val1 = intensity_rangeA[cl][j] * SldrZoom[cl];

                    if (MaxMinSliders[cl][j].Maximum < val1 || val1 < 0 || double.IsNaN(val1))
                    {
                        if (j == 1)
                        {
                            val1 = MaxMinSliders[cl][j].Maximum / SldrZoom[cl];
                            MaxMinTextBox[cl][j].Text = val1.ToString();
                        }
                        else
                        {
                            val1 = 0;
                            MaxMinTextBox[cl][j].Text = "0";
                        }
                    }
                    else
                    {
                        MaxMinSliders[cl][j].Value = (int)val1;
                    }
                }


            }
        }



        private void ToolPanelPaint(object sender, PaintEventArgs e)
        {
            int mergin = 5;
            Size siz = Square_Box.Size;
            Pen drawPen = new Pen(Brushes.Black);
            Rectangle ROIrect = new Rectangle(mergin, mergin, siz.Width - 2 * mergin, siz.Height - 2 * mergin);

            if (sender.Equals(Square_Box))
                e.Graphics.DrawRectangle(drawPen, ROIrect);
            else if (sender.Equals(ElipsoidBox))
                e.Graphics.DrawEllipse(drawPen, ROIrect);
            else if (sender.Equals(PolygonBox))
            {
                Point[] P1 = new Point[4];
                P1[0] = new Point(mergin, mergin);
                P1[1] = new Point(siz.Width - mergin * 2, (int)(mergin * 1.45));
                P1[2] = new Point(siz.Width - mergin, siz.Height - mergin * 2);
                P1[3] = new Point((int)(mergin * 1.2), siz.Height - (int)(1.7 * mergin));
                e.Graphics.DrawPolygon(drawPen, P1);
            }
            else if (sender.Equals(LineBox))
            {
                Point[] P1 = new Point[4];
                P1[0] = new Point(mergin, mergin);
                P1[1] = new Point(siz.Width - mergin * 2, (int)(mergin * 1.45));
                P1[2] = new Point(siz.Width - mergin, siz.Height - mergin * 2);
                P1[3] = new Point((int)(mergin * 1.2), siz.Height - (int)(1.7 * mergin));
                e.Graphics.DrawCurve(drawPen, P1);
                foreach (var p in P1)
                    e.Graphics.DrawEllipse(drawPen, new RectangleF(p.X - 3, p.Y - 3, 7, 7));
                //e.Graphics.FillEllipse(drawPen.Brush, new Rectangle(p.X - 2, p.Y - 2, 5, 5));
            }
            else if (sender.Equals(UncagingBox))
            {
                int midX = (int)(siz.Width / 2);
                int midY = (int)(siz.Height / 2);
                e.Graphics.DrawLine(drawPen, new Point(midX, 0), new Point(midX, siz.Height));
                e.Graphics.DrawLine(drawPen, new Point(0, midY), new Point(siz.Width, midY));
            }
            else if (sender.Equals(ThreeDROIPanel))
            {
                float px = siz.Width / 10;
                float py = siz.Height / 10;
                e.Graphics.DrawString("3D", new Font("Arial", 12), Brushes.Black, new PointF(px, py));
            }
        }

        private void TurnOnOffThreeD(bool ON)
        {
            if (ON && (ZStack || FastZStack))
            {
                ThreeDROIPanel.BorderStyle = BorderStyle.Fixed3D;
                ThreeDRoi = ON;
            }
            else
            {
                ThreeDROIPanel.BorderStyle = BorderStyle.None;
                ThreeDRoi = false;
            }
        }

        private void ToolPanelClicked(object sender, EventArgs e)
        {
            Square_Box.BorderStyle = BorderStyle.None;
            ElipsoidBox.BorderStyle = BorderStyle.None;
            PolygonBox.BorderStyle = BorderStyle.None;
            UncagingBox.BorderStyle = BorderStyle.None;
            LineBox.BorderStyle = BorderStyle.None;

            roi_State = drawROI_State.Inactive;
            drawUncagingPos = UncagingCursor.Inactive;

            if (sender.Equals(Square_Box))
            {
                roi_State = drawROI_State.NoROI;
                ROItype = ROI.ROItype.Rectangle;
                Square_Box.BorderStyle = BorderStyle.Fixed3D;
            }
            else if (sender.Equals(ElipsoidBox))
            {
                roi_State = drawROI_State.NoROI;
                ROItype = ROI.ROItype.Elipsoid;
                ElipsoidBox.BorderStyle = BorderStyle.Fixed3D;
            }
            else if (sender.Equals(PolygonBox))
            {
                roi_State = drawROI_State.NoROI;
                ROItype = ROI.ROItype.Polygon;
                PolygonBox.BorderStyle = BorderStyle.Fixed3D;
            }
            else if (sender.Equals(LineBox))
            {
                roi_State = drawROI_State.NoROI;
                ROItype = ROI.ROItype.PolyLine;
                LineBox.BorderStyle = BorderStyle.Fixed3D;
            }
            else if (sender.Equals(UncagingBox))
            {
                drawUncagingPos = UncagingCursor.Idle;
                UncagingBox.BorderStyle = BorderStyle.Fixed3D;
            }
            else if (sender.Equals(ThreeDROIPanel))
            {
                roi_State = drawROI_State.NoROI;
                ThreeDRoi = !ThreeDRoi;
                TurnOnOffThreeD(ThreeDRoi);
                UpdateImages(false, realtime, focusing, false);
            }
            Refresh();
        }

        private void Image_Display_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSetting();

            if (flimage.use_mainPanel)
            {
                flimage.ToolWindowClosed();
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                //flimage.SaveWindows();
                flimage.Close();
            }
        }


        private void SetUncagingPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uncaging_on = !uncaging_on;
            uncagingLocFrac = FLIM_ImgData.State.Uncaging.Position;
            Activate_uncaging(uncaging_on, true);
            Refresh();
        }

        private void TimeCoursePlotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            plot_regular.Show();
        }

        private void PlotRaid_Click(object sender, EventArgs e)
        {
            plot_regular.plotNow_noRealtime(TCF, TC, this, currentChannel);
            //realtime_plot.Invalidate();
        }

        private void FileN_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int valI = 0;
                int saveCounter = FLIM_ImgData.fileCounter;
                string saveName = FLIM_ImgData.fullFileName;

                if (Int32.TryParse(FileN.Text, out valI)) FLIM_ImgData.fileCounter = valI;
                if (valI > 0)
                {
                    String fn = FLIM_ImgData.fullName(currentChannel, FLIM_ImgData.State.Files.channelsInSeparatedFile);

                    if (File.Exists(fn))
                    {
                        OpenFLIM(fn, true, plot_regular.calc_upon_open, false);
                    }
                    else
                    {
                        FLIM_ImgData.fileCounter = saveCounter;
                        FLIM_ImgData.fullFileName = saveName;
                    }
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

        }


        public void InitializeStripeBuffer(int nChannels, int height, int width)
        {
            int bytePerPixel = 3;

            pixelsBuffer = MatrixCalc.MatrixCreate2D<byte>(nChannels, width * bytePerPixel * height); //new byte[width * bytePerPixel * height];

            MapStateIntensityRange();
            UInt16[][] ProjectBlack = MatrixCalc.MatrixCreate2D<UInt16>(height, width);

            lock (syncBmp[0])
            {
                if (flimage.State.Acq.acquisition[0])
                    IntensityBitmap = ImageProcessing.FormatImage(State_intensity_range[0], ProjectBlack);
                else
                    IntensityBitmap = null;
            }
            lock (syncBmp[1])
            {
                if (flimage.State.Acq.acquisition[1])
                    IntensityBitmap2 = ImageProcessing.FormatImage(State_intensity_range[1], ProjectBlack);
                else
                    IntensityBitmap2 = null;
            }
        }

        public void ImportState(ScanParameters State_in)
        {
            FileIO fo = new FileIO(State_in);
            ScanParameters State = fo.CopyState();
            FLIM_ImgData.State.Display = State.Display;
            FLIM_ImgData.State.Spc.analysis = State.Spc.analysis;
            currentChannel = 0;
            UpdateFittingParam(currentChannel, false);
            UpdateImageParamText();
            ApplyTextToRange(true);
            UpdateImages(true, realtime, focusing, true, true);
        }

        public void ExportStateDisplay(ScanParameters State_out)
        {
            FileIO fo = new FileIO(FLIM_ImgData.State);
            ScanParameters State = fo.CopyState();
            State_out.Display = State.Display;
            State_out.Spc.analysis = State.Spc.analysis;
        }

        public void MapStateIntensityRange()
        {
            //int nCh = FLIM_ImgData.State.Acq.nChannels;
            //if (nCh < N_MAXCHANNEL)

            int nCh = N_MAXCHANNEL;

            if (State_intensity_range == null || State_intensity_range.Length != nCh)
            {
                State_intensity_range = new double[nCh][];
                State_FLIM_intensity_range = new double[nCh][];
                State_FLIM_lifetime_range = new double[nCh][];
            }

            for (int ch = 0; ch < nCh; ch++)
            {
                State_intensity_range[ch] = new double[N_RANGE];
                State_FLIM_intensity_range[ch] = new double[N_RANGE];
                State_FLIM_lifetime_range[ch] = new double[N_RANGE];
            }

            for (int ch = 0; ch < nCh; ch++)
            {
                var field = FLIM_ImgData.State.Display.GetType().GetField("Intensity_Range" + (ch + 1));
                if (field != null)
                    State_intensity_range[ch] = (double[])field.GetValue(FLIM_ImgData.State.Display);
                field = FLIM_ImgData.State.Display.GetType().GetField("FLIM_Intensity_Range" + (ch + 1));
                if (field != null)
                    State_FLIM_intensity_range[ch] = (double[])field.GetValue(FLIM_ImgData.State.Display);
                field = FLIM_ImgData.State.Display.GetType().GetField("FLIM_Range" + (ch + 1));
                if (field != null)
                    State_FLIM_lifetime_range[ch] = (double[])field.GetValue(FLIM_ImgData.State.Display);
            }
        }


        public void UpdateStripe(UInt16[][][,,] img, int ch, int StartLine, int EndLine) //Happens only during scanning.
        {
            MapStateIntensityRange();
            int[] t_range = FLIM_ImgData.fit_range[ch];

            if (EndLine > StartLine)
            {
                if (ch == 0)
                {
                    lock (syncBmp[0])
                    {
                        ImageProcessing.GetProjectFromFLIMLines(img[0][ch], FocusProjectBuffer[ch], t_range, StartLine, EndLine);
                        ImageProcessing.FormatImageLines(IntensityBitmap, State_intensity_range[ch], FocusProjectBuffer[ch], StartLine, EndLine);
                    }
                }
                else if (ch == 1)
                {
                    lock (syncBmp[1])
                    {
                        ImageProcessing.GetProjectFromFLIMLines(img[0][ch], FocusProjectBuffer[ch], t_range, StartLine, EndLine);
                        ImageProcessing.FormatImageLines(IntensityBitmap2, State_intensity_range[ch], FocusProjectBuffer[ch], StartLine, EndLine);
                    }
                }

                if ((Channel12.Checked && ch == 0) || (!Channel12.Checked && ch == currentChannel))
                {
                    Image1.Invalidate();

                }
                else if (Channel12.Checked && ch == 1)
                {
                    Image2.Invalidate();
                }
            }


        }


        public int CalcAdjustSumProjection()
        {
            //MapStateIntensityRange();
            int adjustSumProjection = 1;
            if (FLIM_ImgData.ZProjection && FLIM_ImgData.z_projection_type == FLIMData.projectionType.Sum)
                adjustSumProjection = FLIM_ImgData.ZProjection_Range[1] - FLIM_ImgData.ZProjection_Range[0]; // FLIM_ImgData.n_pages;
            return adjustSumProjection;
        }

        public void LoadIntensity_Range()
        {

            double int1;

            if (!FrameAdjustment.Checked)
                return;

            MapStateIntensityRange();

            int adjustSumProjection = CalcAdjustSumProjection();

            for (int cl = 0; cl < intensity_range_perFrame.Length; cl++)
            {
                double nAveragedFrame = FLIM_ImgData.nAveragedFrame.Max();
                if (FLIM_ImgData.nChannels > cl)
                    nAveragedFrame = FLIM_ImgData.nAveragedFrame[cl];

                if (!FLIM_ImgData.ZStack && FLIM_ImgData.State.Acq.nAveSlice > 1)
                    nAveragedFrame = nAveragedFrame * FLIM_ImgData.State.Acq.nAveSlice;

                if (nAveragedFrame <= 0)
                    nAveragedFrame = 1;

                for (int j = 0; j < N_RANGE; j++)
                {
                    int1 = (intensity_range_perFrame[cl][j] * nAveragedFrame * adjustSumProjection);
                    if (int1 > MIN_INTENSITY_ROUND)
                        int1 = Math.Round(int1);
                    State_intensity_range[cl][j] = int1;

                    int1 = (FLIM_intensity_range_perFrame[cl][j] * nAveragedFrame * adjustSumProjection);
                    if (int1 > MIN_INTENSITY_ROUND)
                        int1 = Math.Round(int1);
                    State_FLIM_intensity_range[cl][j] = int1;

                    ////FLIM channels
                    State_FLIM_lifetime_range[cl][j] = FLIM_lifetime_range_perFrame[cl][j];
                }
            }

        }

        public void SaveIntensity_Range(int channel, bool saveFlim)
        {
            MapStateIntensityRange();

            int adjustSumProjection = CalcAdjustSumProjection();

            double nAverageFrame = (double)FLIM_ImgData.nAveragedFrame.Max(); //this is place holder.

            if (channel < FLIM_ImgData.nChannels)
                nAverageFrame = (double)FLIM_ImgData.nAveragedFrame[channel]; //Actual value.

            if (nAverageFrame <= 0)
                nAverageFrame = 1;

            for (int j = 0; j < N_RANGE; j++)
            {
                intensity_range_perFrame[channel][j] = State_intensity_range[channel][j] / nAverageFrame / (double)adjustSumProjection;

                if (saveFlim)
                {
                    FLIM_intensity_range_perFrame[channel][j] = State_FLIM_intensity_range[channel][j] / nAverageFrame / (double)adjustSumProjection;
                    FLIM_lifetime_range_perFrame[channel][j] = State_FLIM_lifetime_range[channel][j];
                }
            }
        }


        public void ApplyTextToRange(bool saveToOriginalRange)
        {
            int nChannels = 3;
            double[][] intensity_rangeA = new double[nChannels][];

            int c = currentChannel;


            for (int i = 0; i < nChannels; i++)
            {
                intensity_rangeA[i] = new double[2];
            }

            for (int cl = 0; cl < nChannels; cl++)
            {
                for (int j = 0; j < N_RANGE; j++)
                {
                    double valD;
                    if (Double.TryParse(MaxMinTextBox[cl][j].Text, out valD)) intensity_rangeA[cl][j] = valD;
                }
            }


            ////FLIM channels
            if (!Channel12.Checked)
            {
                for (int j = 0; j < N_RANGE; j++)
                {
                    double valD;
                    if (Double.TryParse(MaxMinTextBox[2][j].Text, out valD)) State_FLIM_lifetime_range[c][j] = valD;
                }
            }


            int valI;
            int nPage = FLIM_ImgData.n_pages;

            if (Int32.TryParse(PageStart.Text, out valI) && valI <= nPage && valI > 0)
                page_range[0] = valI - 1;

            if (Int32.TryParse(PageStart.Text, out valI) && valI <= nPage && valI > 0)
                page_range[0] = valI - 1;

            if (Int32.TryParse(PageEnd.Text, out valI) && valI <= nPage && valI > 0)
                page_range[1] = valI;

            if (page_range[0] > page_range[1])
            {
                valI = page_range[1];
                page_range[1] = page_range[0];
                page_range[0] = valI;
            }


            MapStateIntensityRange();

            ScanParameters State = FLIM_ImgData.State;
            if (!Channel12.Checked)
            {
                for (int j = 0; j < N_RANGE; j++)
                {
                    State_intensity_range[c][j] = intensity_rangeA[0][j];
                    State_FLIM_intensity_range[c][j] = intensity_rangeA[1][j];
                }

                if (saveToOriginalRange)
                    SaveIntensity_Range(c, true);
            }

            else //c = 12
            {
                for (int j = 0; j < N_RANGE; j++)
                {
                    for (int ch = 0; ch < N_DISPLAYCHANNEL; ch++)
                        State_intensity_range[ch][j] = intensity_rangeA[ch][j];
                    // State_intensity_range[1][j] = intensity_rangeA[1][j];
                }

                if (saveToOriginalRange)
                {
                    for (int ch = 0; ch < N_DISPLAYCHANNEL; ch++)
                        SaveIntensity_Range(ch, false);
                }
            }
            //if (saveToOriginalRange)
            //    State.Display.GetType().GetField("FLIM_Range" + (currentChannel + 1)).SetValue(State.Display, intensity_rangeA[2]);
        }

        private void stopOpening_Click(object sender, EventArgs e)
        {
            StopFileOpening = true;
        }

        private void saveRoiAsImageJToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String FileName = RoiFileName(true);
            if (File.Exists(FileName))
                File.Delete(FileName);

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    for (int j = 0; j < FLIM_ImgData.ROIs.Count; j++)
                    {
                        var roiFile = archive.CreateEntry("ROI-" + FLIM_ImgData.ROIs[j].ID + ".roi");

                        using (var entryStream = roiFile.Open())
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            byte[] byteArray = CreateRoiImageJ(FLIM_ImgData.ROIs[j], "ROI-" + FLIM_ImgData.ROIs[j].ID);
                            entryStream.Write(byteArray, 0, byteArray.Length);
                        }
                    }
                }

                using (var fileStream = new FileStream(FileName, FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }

        }

        public static int C_POSITION = 4;
        public static int Z_POSITION = 8;
        public static int T_POSITION = 12;
        public static int NAME_OFFSET = 16;
        public static int NAME_LENGTH = 20;
        public static int OVERLAY_LABEL_COLOR = 24;
        public static int OVERLAY_FONT_SIZE = 28; //short
        public static int AVAILABLE_BYTE1 = 30;  //byte
        public static int IMAGE_OPACITY = 31;  //byte
        public static int IMAGE_SIZE = 32;  //int
        public static int FLOAT_STROKE_WIDTH = 36;  //float
        public static int ROI_PROPS_OFFSET = 40;
        public static int ROI_PROPS_LENGTH = 44;
        public static int COUNTERS_OFFSET = 48;

        public object ImageImfo { get; private set; }

        private byte[] CreateRoiImageJ(ROI roi, String RoiName)
        {
            //   0 - 3     "Iout"
            //   4 - 5     version(>= 217)
            //   6 - 7     roi type(encoded as one byte
            //   8 - 9     top
            //   10 - 11   left
            //   12 - 13   bottom
            //   14 - 15   right
            //   16 - 17   NCoordinates
            //   18 - 33   x1,y1,x2,y2(straight line)
            //   34 - 35   stroke width(v1.43i or later)
            //   36 - 39   ShapeRoi size(type must be 1 if this value > 0)
            //   40 - 43   stroke color(v1.43i or later)
            //   44 - 47   fill color(v1.43i or later)
            //   48 - 49   subtype(v1.43k or later)
            //   50 - 51   options(v1.43k or later)
            //   52 - 52   arrow style or aspect ratio(v1.43p or later)
            //   53 - 53   arrow head size(v1.43p or later)
            //   54 - 55   rounded rect arc size(v1.43p or later)
            //   56 - 59   position
            //   60 - 63   header2 offset
            //   64 - x - coordinates(short), followed by y - coordinates
            //    header2 offsets

            int BeginCoordinates = 64;
            int header2Position = 64;
            int header2Length = 52;
            int nameOffsetValue = header2Position + header2Length;
            int nameLength = RoiName.Length;
            int totalLength = BeginCoordinates + header2Length + nameLength;
            byte[] ByteArray = new byte[totalLength];

            ByteArray[0] = (byte)('I');
            ByteArray[1] = (byte)('o');
            ByteArray[2] = (byte)('u');
            ByteArray[3] = (byte)('t');
            IntToByteArray(ByteArray, 220, 4, 2);  //New version
            IntToByteArray(ByteArray, (int)roi.ROI_type, 6, 1);
            IntToByteArray(ByteArray, (int)roi.Rect.Top, 8, 2);
            IntToByteArray(ByteArray, (int)roi.Rect.Left, 10, 2);
            IntToByteArray(ByteArray, (int)roi.Rect.Bottom, 12, 2);
            IntToByteArray(ByteArray, (int)roi.Rect.Right, 14, 2);
            IntToByteArray(ByteArray, roi.ID, 56, 2);

            switch (roi.ROI_type)
            {
                case ROI.ROItype.Rectangle:
                case ROI.ROItype.Elipsoid:
                    break;
                case ROI.ROItype.Line:
                    IntToByteArray(ByteArray, (int)roi.X[0], 18, 4);
                    IntToByteArray(ByteArray, (int)roi.X[1], 22, 4);
                    IntToByteArray(ByteArray, (int)roi.Y[0], 26, 4);
                    IntToByteArray(ByteArray, (int)roi.Y[1], 30, 4);
                    break;
                case ROI.ROItype.FreeHand:
                case ROI.ROItype.FreeLine:
                case ROI.ROItype.Point:
                case ROI.ROItype.Polygon:
                case ROI.ROItype.PolyLine:
                case ROI.ROItype.Traced:
                    int n = roi.X.Length;
                    IntToByteArray(ByteArray, n, 16, 2);
                    int newSize = totalLength + 4 * n;
                    Array.Resize(ref ByteArray, newSize);
                    header2Position = header2Position + 4 * n;

                    for (int i = 0; i < n; i++)
                    {
                        IntToByteArray(ByteArray, (int)(roi.X[i] - roi.Rect.Left), BeginCoordinates + 2 * i, 2);
                        IntToByteArray(ByteArray, (int)(roi.Y[i] - roi.Rect.Top), BeginCoordinates + 2 * n + 2 * i, 2);
                    }
                    break;
                default:
                    break;

            }

            IntToByteArray(ByteArray, header2Position, 60, 4);
            nameOffsetValue = header2Position + header2Length;
            IntToByteArray(ByteArray, nameOffsetValue, header2Position + NAME_OFFSET, 4);
            IntToByteArray(ByteArray, nameLength, header2Position + NAME_LENGTH, 4);

            for (int i = 0; i < nameLength; i++)
            {
                ByteArray[i + nameOffsetValue] = (byte)RoiName[i];
            }
            return ByteArray;

        }

        public void IntToByteArray(byte[] ReturnArray, int val, int position, int Length)
        {
            byte[] ByteArray = new byte[4];
            ByteArray = BitConverter.GetBytes(val);
            Array.Resize(ref ByteArray, Length);
            Array.Reverse(ByteArray);
            Array.Copy(ByteArray, 0, ReturnArray, position, Length);
        }


        private void readRoiFromImageJToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            String defaultDirectory = FLIM_ImgData.pathName + "\\Analysis\\ROI";
            if (!Directory.Exists(defaultDirectory))
                defaultDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            openFileDialog1.InitialDirectory = defaultDirectory;
            openFileDialog1.FileName = "roi.zip";
            openFileDialog1.Filter = "ROI files (*.zip)|*.zip|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = false;

            String filename = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        filename = openFileDialog1.FileName;
                        myStream.Close();
                    }
                    else
                        return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    return;
                }
            }

            if (filename == "")
                return;

            ReadImageJROI(filename);
        }

        public void ReadImageJROI(String filename)
        {
            if (!File.Exists(filename))
                return;

            using (ZipArchive archive = ZipFile.OpenRead(filename))
            {
                FLIM_ImgData.ROIs.Clear();
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!entry.Name.EndsWith(".roi"))
                        break;
                    var stream = entry.Open();
                    int nBytes = 512;
                    byte[] ByteArray = new byte[nBytes];
                    int nBytesRead = stream.Read(ByteArray, 0, nBytes);
                    ROI.ROItype roiType = (ROI.ROItype)BitToInt(ByteArray, 6, 1);
                    int top = BitToInt(ByteArray, 8, 2);
                    int left = BitToInt(ByteArray, 10, 2);
                    int bottom = BitToInt(ByteArray, 12, 2);
                    int right = BitToInt(ByteArray, 14, 2);
                    int nCoordinates = BitToInt(ByteArray, 16, 2);
                    int x1 = BitToInt(ByteArray, 18, 4);
                    int y1 = BitToInt(ByteArray, 22, 4);
                    int x2 = BitToInt(ByteArray, 26, 4);
                    int y2 = BitToInt(ByteArray, 30, 4);
                    int subtype = BitToInt(ByteArray, 48, 2);
                    int options = BitToInt(ByteArray, 50, 2);
                    int roundRectArcSize = BitToInt(ByteArray, 54, 2);
                    int position = BitToInt(ByteArray, 56, 2);
                    int header2_offset = BitToInt(ByteArray, 60, 4);
                    float[] X, Y;

                    Rectangle Rect = new Rectangle(left, top, right - left, bottom - top);
                    ROI roi;
                    if (roiType == ROI.ROItype.Elipsoid || roiType == ROI.ROItype.Rectangle)
                    {
                        roi = new ROI(roiType, Rect, FLIM_ImgData.nChannels, 0, false, new int[] { 0 }); //ImageJ does not have 3d ROI?
                    }
                    else if (roiType == ROI.ROItype.Line)
                    {
                        X = new float[2];
                        Y = new float[2];
                        X[0] = x1;
                        X[1] = x2;
                        Y[0] = y1;
                        Y[1] = y2;
                        roi = new ROI(roiType, X, Y, FLIM_ImgData.nChannels, polyLineRadius, 0, false, new int[] { 0 }); //ImageJ does not have 3d ROI?
                    }
                    else
                    {
                        X = new float[nCoordinates];
                        Y = new float[nCoordinates];
                        for (int i = 0; i < nCoordinates; i++)
                        {
                            X[i] = BitToInt(ByteArray, 64 + i * 2, 2) + left;
                        }
                        for (int i = 0; i < nCoordinates; i++)
                        {
                            Y[i] = BitToInt(ByteArray, 64 + i * 2 + nCoordinates * 2, 2) + top;
                        }
                        roi = new ROI(roiType, X, Y, FLIM_ImgData.nChannels, polyLineRadius, 0, false, new int[] { 0 });
                    }

                    if (roi.Rect.Left >= 0 && roi.Rect.Top >= 0 && roi.Rect.Right <= FLIM_ImgData.width && roi.Rect.Bottom <= FLIM_ImgData.height)
                    {
                        //roi.ID = FLIM_ImgData.ROIs.Count + 1;
                        if (entry.Name.Contains("ROI-"))
                        {
                            int id = 0;
                            string num_s = entry.Name.Split('-')[1].Split('.')[0];
                            if (Int32.TryParse(num_s, out id))
                            {
                                FLIM_ImgData.addToMultiRoi(roi, id);
                            }
                            else
                            {
                                FLIM_ImgData.addToMultiRoi(roi);
                            }
                        }
                        else
                            FLIM_ImgData.addToMultiRoi(roi);
                    }
                }
            }

        }

        public Int32 BitToInt(byte[] byteArray, int StartByte, int Length)
        {
            byte[] byteArray2 = new byte[Length];
            Array.Copy(byteArray, StartByte, byteArray2, 0, Length);
            Array.Reverse(byteArray2);
            if (Length == 4)
                return BitConverter.ToInt32(byteArray2, 0);
            else if (Length == 1)
                return Convert.ToInt32(byteArray2[0]);
            else
                return BitConverter.ToInt16(byteArray2, 0);
        }


        private void MergeCB_Click(object sender, EventArgs e)
        {
            if (MergeCB.Checked)
                HoldCurrentImageCheckBox.Checked = false;
            DrawImages();
        }

        private void HoldCurrentImageCheckBox_Click(object sender, EventArgs e)
        {
            CheckBox sendr = (CheckBox)sender;
            //sendr.Checked = !sendr.Checked;
            if (HoldCurrentImageCheckBox.Checked)
                MergeCB.Checked = false;

            if (sendr.Checked)
            {
                lock (syncBmp[0])
                {
                    IntensityBitmap_Save = IntensityBitmap;
                    IntensityBitmap = new Bitmap(IntensityBitmap.Width, IntensityBitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                }

                lock (syncBmp[1])
                {
                    IntensityBitmap_Save2 = IntensityBitmap2;
                    IntensityBitmap2 = new Bitmap(IntensityBitmap2.Width, IntensityBitmap2.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                }
                UpdateImages(false, realtime, focusing, false, false);
            }
            else
            {
                UpdateImages(false, realtime, focusing, false);
            }
        }

        private void rightClickMenuStrip_Opening(object sender, CancelEventArgs e)
        {

        }



        private void scanThisRoiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flimage.changeScanArea(FLIM_ImgData.Roi);
        }


        private void keepPagesInMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            keepPagesInMemoryToolStripMenuItem.Checked = !keepPagesInMemoryToolStripMenuItem.Checked;
        }

        private void EntireStack_Check_Click(object sender, EventArgs e)
        {
            entireStack = EntireStack_Check.Checked;
            UpdateImagesRecalc_Click(sender, e);
        }

        private void convertToRectangularROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            convertROIType(ROI.ROItype.Rectangle);
        }

        private void convertToElipsoidROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            convertROIType(ROI.ROItype.Elipsoid);
        }

        private void convertToPolygonROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            convertROIType(ROI.ROItype.Polygon);
        }

        private void convertToPolyLineROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            convertROIType(ROI.ROItype.PolyLine);
        }

        public void convertROIType(ROI.ROItype roitype)
        {
            imageRoi.ROI_type = roitype;
            convertROIfromDisplayToData(imageRoi, ref FLIM_ImgData.Roi);
            FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].ROI_type = roitype;
            DrawImages();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            //capture up arrow key
            if (keyData == Keys.Up)
            {
            }
            //capture down arrow key
            else if (keyData == Keys.Down)
            {
            }
            //capture left arrow key
            else if (keyData == Keys.Left)
            {
                Page_UpDownClick(PageDown, null);
            }
            //capture right arrow key
            else if (keyData == Keys.Right)
            {
                Page_UpDownClick(PageUp, null);
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Image_Display_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                roi_State = drawROI_State.Idle;

            }

            //Arrow keys and < > keys will move the page forward /backword
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.O)
            {
                OpenFLIMImagesToolStripMenuItem_Click(openFLIMImageToolStripMenuItem, null);
            }

            if (e.KeyCode == Keys.PageUp)
                FileUpDown_Click(FileUp, e);
            else if (e.KeyCode == Keys.PageDown)
                FileUpDown_Click(FileDown, e);
        }

        /// <summary>
        /// Add a measured value to realtime data.
        /// </summary>
        /// <param name="num"></param>
        public void AddDataToRealtime(double value)
        {
            realtimeData.Add(value);
        }

        /// <summary>
        /// Function call by Z-project range editobox receive return key.
        /// e.handled = true and supressKePress is required to suprress beep. (but still beeps though..)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZProcRange_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    EntireStack_Check.Checked = false;
                    ApplyTextToRange(false);
                    UpdateImagesRecalc_Click(sender, e);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ZProcRange problem: " + ex.Message);
                }
                finally
                { };
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void showImageDescriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var im_desc = new ImageDescription(FLIM_ImgData);
            im_desc.Show();
        }

        private void intelMKLLibraryOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!MatrixCalc.IntelMKL_on)
                MessageBox.Show("Intel MKL library significantly accelrates matrix calculation, but we cannot distribute it. To use it, you need to download MathNet.Numerics.MKL for Win64 (see https://numerics.mathdotnet.com/mkl.html) and put the libraries in the application folder (usually in C:\\Program Files\\FLIMage\\FLIMage X.X.X.X");

            intelMKLLibraryOnToolStripMenuItem.Checked = MatrixCalc.IntelMKL_on;
        }

        private void getFocusFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FLIM_ImgData.n_time[currentChannel] > 0)
            {
                FLIM_ImgData.CalculateAllPages_Direct(false);
                int[] range = new int[2];
                range[0] = 0;
                range[1] = FLIM_ImgData.State.Acq.FastZ_phase_detection_mode ? FLIM_ImgData.n_pages / 2 : FLIM_ImgData.n_pages;
                int z = ImageProcessing.GetFocusFrame(FLIM_ImgData.Project_Pages, currentChannel, range);
                GotoPage4D(z);
                UpdateImages(true, false, false, true);
            }
        }

        private void fastZCalibrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FastZStack && ZStack)
            {
                if (fast_calib == null || !fast_calib.Visible)
                {
                    fast_calib = new FastZ_Calibration(this);
                }
                else
                    fast_calib.calculateFocus(); //automatic when opening.

                fast_calib.Show();
            }
            else
            {
                MessageBox.Show("Need Z-Stack && Fast-ZStack data");
            }
        }

        private void CurrentFastZPageTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (Int32.TryParse(CurrentFastZPageTB.Text, out int fastPage))
                {
                    fastPage -= 1;
                    GotoPage5D(fastPage);
                    if (displayZProjection)
                        calcZProjection();
                    UpdateImages(true, false, false, true);
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        public void FormatProject()
        {

            if (!focusing || !flimage.flimage_io.parameters.StripeDuringFocus)
            {
                lock (syncBmp[0])
                {
                    if (FLIM_ImgData.State.Acq.acquisition[0])
                        IntensityBitmap = ImageProcessing.FormatImage(State_intensity_range[0], FLIM_ImgData.Project[0]);
                    else
                        IntensityBitmap = null;
                }


                if (FLIM_ImgData.nChannels > 1)
                {
                    lock (syncBmp[1])
                    {
                        if (FLIM_ImgData.State.Acq.acquisition[1])
                            IntensityBitmap2 = ImageProcessing.FormatImage(State_intensity_range[1], FLIM_ImgData.Project[1]);
                        else
                            IntensityBitmap2 = null;
                    }
                }

                if (FLIM_ImgData.State.Acq.fastZScan && FLIM_ImgData.State.Acq.FastZ_phase_detection_mode)
                {
                    int page2 = FLIM_ImgData.n_pages - FLIM_ImgData.currentPage - 1;
                    FLIM_ImgData.CalculatePage_Direct(page2, false);
                    Bitmap_FastZ_PhaseComplementary = ImageProcessing.FormatImage(State_intensity_range[currentChannel], FLIM_ImgData.Project_Pages[page2][currentChannel]);
                }

            }
        }

        public void PostFastZUpDownFunction()
        {

        }

        public void PostOpenUserFunction()
        {

        }

        private void averageTimeCoursePythonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunPythonScript("ReadFLIMageCSVGUI.py", false);
        }


        private void runPythonScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunPythonScript("", true);
        }

        private string findPythonPath()
        {
            String[] locations = RegistryController.FindPythonInstallPath();
            string pythonPath = "";
            if (locations != null)
            {
                for (int i = 0; i < locations.Length; i++)
                {
                    if (locations[i].Contains("3"))
                        pythonPath = locations[i];
                    if (locations[i].ToLower().Contains("anaconda3")) //software is built on anaconda3
                    {
                        pythonPath = locations[i];
                        break;
                    }
                }
            }

            return pythonPath;
        }

        private void RunPythonScript(String script, bool openCommand)
        {
            if (!File.Exists(Path.Combine(PythonPath, "python.exe")))
            {
                PythonPath = findPythonPath();
            }

            if (!File.Exists(Path.Combine(PythonPath, "python.exe")))
            {
                MessageBox.Show(Path.Combine(PythonPath, "python.exe") + " not found");
                return;
            }

            List<String> Paths = new List<string>();
            Paths.Add(PythonPath);
            Paths.Add(Path.Combine(PythonPath, "bin"));
            Paths.Add(Path.Combine(PythonPath, "Library", "bin"));
            Paths.Add(Path.Combine(PythonPath, "Scripts"));
            //Paths.Add(Path.Combine(PythonPath, "Library", "usr", "bin"));
            Paths.Add(Path.Combine(PythonPath, "Library", "mingw-w64", "bin"));

            String PathStr = System.Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
            for (int i = 0; i < Paths.Count; i++)
                PathStr = PathStr + Paths[i] + ";";

            ProcessStartInfo start = new ProcessStartInfo();
            start.EnvironmentVariables["PATH"] = PathStr;
            start.FileName = "\"" + Path.Combine(PythonPath, "python.exe") + "\"";
            String scriptName = Path.Combine(ScriptPath, script); //
            if (!File.Exists(scriptName))
            {
                scriptName = SetScriptPath();
            }
            start.Arguments = "\"" + scriptName + "\"";

            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.WorkingDirectory = ScriptPath;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = !openCommand; //Does not open command prompt window.
            Task.Factory.StartNew(() =>
            {
                Process process = Process.Start(start);
            });

            //using (Process process = Process.Start(start))
            //{
            //    using (StreamReader reader = process.StandardOutput)
            //    {
            //        string result = reader.ReadToEnd();
            //    }
            //}
        }

        private void setPythonPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            String defaultDirectory = FLIM_ImgData.pathName;
            if (!Directory.Exists(defaultDirectory))
                defaultDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            openFileDialog1.InitialDirectory = defaultDirectory;
            openFileDialog1.FileName = "python.exe";
            openFileDialog1.Filter = "Python exe file (*.exe)|*.exe|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = false;

            String filename = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        filename = openFileDialog1.FileName;
                        PythonPath = Path.GetDirectoryName(filename);
                        myStream.Close();
                    }
                    else
                        return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    return;
                }
            }
        }

        private void setScriptPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetScriptPath();
        }

        /// <summary>
        /// Return FileName.
        /// </summary>
        /// <returns></returns>
        private String SetScriptPath()
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            String defaultDirectory = ScriptPath;
            if (!Directory.Exists(defaultDirectory))
                defaultDirectory = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FLIMage", ScriptDirectoryName);

            openFileDialog1.InitialDirectory = defaultDirectory;
            openFileDialog1.FileName = "ReadFLIMageCSVGUI.py";
            openFileDialog1.Filter = "Python py file (*.py)|*.py|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = false;

            String filename = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        filename = openFileDialog1.FileName;
                        ScriptPath = Path.GetDirectoryName(filename);
                        myStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }

            return filename;
        }

        private void AutoApplyOffset_CheckedChanged(object sender, EventArgs e)
        {
            if (AutoApplyOffset.Checked)
                plot_regular.TurnOnCalcUponOpen(true);
        }

        private void psPerUnit_TextChanged(object sender, EventArgs e)
        {
            Double.TryParse(psPerUnit.Text, out FLIM_ImgData.psPerUnit);
            for (int i = 0; i < FLIM_ImgData.State.Spc.spcData.resolution.Length; i++)
                FLIM_ImgData.State.Spc.spcData.resolution[i] = FLIM_ImgData.psPerUnit;
        }


        private void setPolylineRadiusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double um_pixel = FLIM_ImgData.State.Acq.field_of_view[0] / FLIM_ImgData.State.Acq.zoom / (double)FLIM_ImgData.width;

            string query = String.Format("Radius (um or pixel), ({0:0.00} um/pixel) = ", um_pixel);
            string default1 = String.Format("{0:0.000} um", polyLineRadius * um_pixel);
            string input = Interaction.InputBox(query, "PolyLine ROI radius", default1);

            if (input.Contains("um"))
            {
                string result = input.Replace("um", "");
                if (double.TryParse(result, out double radius_um))
                    polyLineRadius = (float)(radius_um / um_pixel);
            }
            else
            {
                string result = input.Replace("pix", "").Replace("el", "").Replace("s", "");
                if (double.TryParse(result, out double radius_pix))
                    polyLineRadius = (float)radius_pix;
            }

            FLIM_ImgData.Roi.GetEqualDistanceCenters(polyLineRadius);
            imageRoi.GetEqualDistanceCenters(polyLineRadius * image_scale);

            foreach (var roi in FLIM_ImgData.ROIs)
            {
                roi.GetEqualDistanceCenters(polyLineRadius);
            }

            DrawImages();
        }


        private void rightClickMenu_removeAll_Opening(object sender, CancelEventArgs e)
        {

        }

        private void createMultiROIFromCurrentROIToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


        private void setSizeOfThisROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FLIM_ImgData.Roi.ROI_type == ROI.ROItype.Elipsoid || FLIM_ImgData.Roi.ROI_type == ROI.ROItype.Rectangle)
            {
                double um_pixel = FLIM_ImgData.State.Acq.field_of_view[0] / FLIM_ImgData.State.Acq.zoom / (double)FLIM_ImgData.width;
                string query = String.Format("Radius (um or pixel), ({0:0.00} um/pixel) = ", um_pixel);
                string default1 = String.Format("{0:0.000} pixel", CircleDefaulRadius);
                string input = Interaction.InputBox(query, "Circular ROI radius", default1);

                if (input.Contains("um"))
                {
                    string result = input.Replace("um", "");
                    if (double.TryParse(result, out double radius_um))
                        CircleDefaulRadius = (float)(radius_um / um_pixel);
                }
                else
                {
                    string result = input.Replace("pix", "").Replace("el", "").Replace("s", "");
                    if (double.TryParse(result, out double radius_pix))
                        CircleDefaulRadius = (float)radius_pix;
                }

                float size1 = CircleDefaulRadius * 2;
                float[] currentCenter = new float[] { FLIM_ImgData.Roi.Rect.Location.X + FLIM_ImgData.Roi.Rect.Size.Width / 2, FLIM_ImgData.Roi.Rect.Location.Y + FLIM_ImgData.Roi.Rect.Size.Height / 2 };
                float[] newLocation = new float[] { currentCenter[0] - size1 / 2, currentCenter[1] - size1 / 2 };
                RectangleF newRect = new RectangleF(new PointF(newLocation[0], newLocation[1]), new SizeF(size1, size1));
                FLIM_ImgData.Roi = new ROI(FLIM_ImgData.Roi.ROI_type, newRect, FLIM_ImgData.nChannels, -1, FLIM_ImgData.Roi.Roi3d, FLIM_ImgData.Roi.Z);
                convertROIFromDataToDisplay(FLIM_ImgData.Roi, ref imageRoi);
                if (FLIM_ImgData.currentRoi >= 0 && FLIM_ImgData.currentRoi < FLIM_ImgData.ROIs.Count)
                {
                    FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi] = FLIM_ImgData.Roi.CopyROI(FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].ID);
                }

                SetFitting_Param(true);
                UpdateImages(false, realtime, focusing, true);
            }
        }

        private void setAsBackgroundROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FLIM_ImgData.bgRoi = FLIM_ImgData.Roi.CopyROI(-2);
            DrawImages();
        }

        ////////////////////////////////////////////////////////////////////////////////////              
        /////////////////////
        ////////////////////////////////////////////////////////////////////////////////////  
        public void UpdateImages(bool calcLifetimeMap, bool realtime1, bool focus, bool calcLifetime)
        {
            UpdateImages(calcLifetimeMap, realtime1, focus, calcLifetime, true);
        }

        private void stimulationTriggeredIntegrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int startPage = 33;
            int interval = 8;
            int repeat = 30;
            string query = String.Format("Start page, interval, repeat (Separated by comma(,)");
            string default_val = String.Format("{0}, {1}, {2}", startPage, interval, repeat);
            string input = Interaction.InputBox(query, query, default_val);
            string[] sP = input.Split(',');
            if (sP.Length >= 3)
            {
                startPage = Convert.ToInt32(sP[0]);
                interval = Convert.ToInt32(sP[1]);
                repeat = Convert.ToInt32(sP[2]);
            }
            Stim_Triggered_Sum(startPage - 1, interval, repeat);
        }

        public void Stim_Triggered_Sum(int startPage, int interval, int repeat)
        {

            if (FLIM_ImgData.nFastZ > 1)
            {
                ;
                for (int i = 0; i < interval; i++)
                {
                    ushort[][][,,] ave = FLIM_ImgData.FLIM_Pages5D[startPage + i]; //This is linearized!

                    for (int j = 1; j < repeat; j++)
                    {
                        if (startPage + i + interval * j < FLIM_ImgData.n_pages5D)
                            for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                                for (int z = 0; z < FLIM_ImgData.nFastZ; z++)
                                    MatrixCalc.ArrayCalc(ave[ch][z], FLIM_ImgData.FLIM_Pages5D[startPage + i + interval * j][ch][z], CalculationType.Add);
                    }

                    FLIM_ImgData.addToPageAndCalculate5D(ave, FLIM_ImgData.acquiredTime_Pages5D[i], true, true, i, false);

                    GotoPage5D(i);
                    //FLIM_ImgData.calculateAll();

                    UpdateImages(false, false, false, false);
                }

                FLIM_ImgData.resizePage5D(interval);
                GotoPage5D(0);
            }
            else
            {

                for (int i = 0; i < interval; i++)
                {
                    ushort[][,,] ave = FLIM_ImgData.FLIM_Pages[startPage + i];

                    for (int j = 1; j < repeat; j++)
                    {
                        if (startPage + i + interval * j < FLIM_ImgData.n_pages)
                            for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                                MatrixCalc.ArrayCalc(ave[ch], FLIM_ImgData.FLIM_Pages[startPage + i + interval * j][ch], CalculationType.Add);
                    }

                    FLIM_ImgData.LoadFLIMRawFromData4D(ave, FLIM_ImgData.acquiredTime_Pages[i], false);
                    FLIM_ImgData.addCurrentFLIMRawToPage4D(true, i, false);

                    GotoPage4D(i);
                    FLIM_ImgData.calculateAll();

                    //GotoPage(i);
                    UpdateImages(false, false, false, false);
                }

                FLIM_ImgData.resizePage(interval);
                GotoPage4D(0);
            }

            UpdateImages(true, false, false, true, true);
        }

        private void colorSchemeCheck()
        {
            spectrumToolStripMenuItem.Checked = color_scheme == ImageProcessing.ColorScheme.Spectrum;
            fireToolStripMenuItem.Checked = color_scheme == ImageProcessing.ColorScheme.Fire;
            redBlueToolStripMenuItem.Checked = color_scheme == ImageProcessing.ColorScheme.RB;
            yellowHighlighterToolStripMenuItem.Checked = color_scheme == ImageProcessing.ColorScheme.YellowHighlight;
            highlighterYellowBluePrinterModToolStripMenuItem.Checked = color_scheme == ImageProcessing.ColorScheme.YellowHighlight_Mod;
            plasmaToolStripMenuItem.Checked = color_scheme == ImageProcessing.ColorScheme.Plasma;
            colorBar.Image = ImageProcessing.CreateColorBar(colorBar.Width, colorBar.Height, ImageProcessing.ColorBarDirection.RightToLeft, color_scheme);
            UpdateImages(true, realtime, focusing, true);
        }


        private void spectrumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            color_scheme = ImageProcessing.ColorScheme.Spectrum;
            colorSchemeCheck();
        }

        private void fireToolStripMenuItem_Click(object sender, EventArgs e)
        {
            color_scheme = ImageProcessing.ColorScheme.Fire;
            colorSchemeCheck();
        }

        private void redBlueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            color_scheme = ImageProcessing.ColorScheme.RB;
            colorSchemeCheck();
        }

        private void yellowHighlighterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            color_scheme = ImageProcessing.ColorScheme.YellowHighlight;
            colorSchemeCheck();
        }

        private void plasmaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            color_scheme = ImageProcessing.ColorScheme.Plasma;
            colorSchemeCheck();
        }

        private void highlighterYellowBluePrinterModToolStripMenuItem_Click(object sender, EventArgs e)
        {
            color_scheme = ImageProcessing.ColorScheme.YellowHighlight_Mod;
            colorSchemeCheck();
        }

        private void setFieldOfViewSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var h = Microsoft.VisualBasic.Interaction.InputBox("Field of view size at zoom = 1 (in um): ", "Set field of view size", FLIM_ImgData.State.Acq.field_of_view[0].ToString());

            if (!double.TryParse(h, out double fieldOfViewSize))
            {
                return;
            }
            FLIM_ImgData.State.Acq.field_of_view = new double[] { fieldOfViewSize, fieldOfViewSize };
        }

        private void Image1_Click(object sender, EventArgs e)
        {

        }

        private void DragOverForm(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Image_Display_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[]; // get all files droppeds  
            if (files != null && files.Any())
                OpenFLIM(files[0], true, plot_regular.calc_upon_open, true);

        }

        private void toggleZStackAndTimecourseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ZStack = !ZStack;
            FLIM_ImgData.ZStack = ZStack;
            UpdateImageParamText();
        }

        public void UpdateImages(bool calcLifetimeMap, bool realtime1, bool focus, bool calcLifetime, bool calcProject) //Can be slow. //Done with timer.
        {
            update_image_busy = true;
            realtime = realtime1;
            focusing = focus;

            FLIM_ImgData.ThreeDRoi = ThreeDRoi;

            if (!FLIM_ImgData.FLIM_on.Any(x => x == true))
            {
                if (plot_realtime != null && realtime)
                    plot_realtime.InvokeIfRequired(o => o.intensity_onlyData());
                if (plot_regular != null && !realtime)
                    plot_regular.InvokeIfRequired(o => o.intensity_onlyData());
            }

            int height = FLIM_ImgData.height;
            int width = FLIM_ImgData.width;
            int n_time = FLIM_ImgData.n_time.Max();

            //FLIM_ImgData.psPerUnit = FLIM_ImgData.State.Spc.spcData.resolution[currentChannel];
            //double psPerChannel = FLIM_ImgData.psPerUnit; // 250 ps;
            int nChannels = FLIM_ImgData.nChannels;

            int c = currentChannel;
            bool ch12_checked = Channel12.Checked; //When Ch12 is clicked.         

            if (Channel1.Checked)
                c = 0;
            else if (Channel2.Checked)
                c = 1;
            else if (Channel12.Checked)
            {
                if (Ch1.Checked)
                    c = 0;
                else if (Ch2.Checked)
                    c = 1;

                if (realtime)
                {
                    calcLifetime = false;
                    calcLifetimeMap = false;
                }
            }

            if (c < nChannels)
                currentChannel = c;


            object obj = FLIM_ImgData.State.Display;
            int nCh = N_MAXCHANNEL;
            for (int i = 0; i < N_MAXCHANNEL; i++)
            {
                var field = obj.GetType().GetField("FLIM_Intensity_Range" + (i + 1));
                if (field == null)
                {
                    nCh = i;
                    break;
                }
            }
            FLIM_ImgData.low_threshold = new double[nCh];

            for (int i = 0; i < nCh; i++)
            {
                var val = (double[])obj.GetType().GetField("FLIM_Intensity_Range" + (i + 1)).GetValue(obj);
                FLIM_ImgData.low_threshold[i] = val[0];
            }

            bool showFLIM = ShowFLIM.Checked;


            if (Auto1.Checked)
                Auto_contrast(); //Save Intensity_range too.

            LoadIntensity_Range(); //Read intensity range from memory.

            if (calcProject)
                FLIM_ImgData.calculateProject();

            FormatProject();

            if (showFLIM && FLIM_ImgData.State.Acq.acqFLIMA[c] && FLIM_ImgData.State.Acq.acquisition[c])
            {
                if (calcLifetimeMap)
                    FLIM_ImgData.calculateLifetimeMapCh(c, FLIM_ImgData.offset[c]);

                FLIM_ImgData.low_threshold[c] = State_FLIM_intensity_range[c][0];

                FLIM_ImgData.filterMAP(FLIM_ImgData.State.Display.filterWindow_FLIM, c); //Subract and filter.

                lock (syncBmpFLIM)
                {
                    if (FLIM_ImgData.LifetimeMapF[c] != null)
                        FLIMBitmap = ImageProcessing.FormatImageFLIM(State_FLIM_intensity_range[c], State_FLIM_lifetime_range[c], FLIM_ImgData.LifetimeMapF[c], FLIM_ImgData.ProjectF[c], false, color_scheme);
                }

                if (roi_State == drawROI_State.NoROI && SelectRoi.Checked)
                    FLIM_ImgData.Fit_type = FLIMData.FitType.WholeImage;
                else
                {
                    if (SelectRoi.Checked)
                        FLIM_ImgData.Fit_type = FLIMData.FitType.SelectedRoi;
                    else if (AllRois.Checked)
                        FLIM_ImgData.Fit_type = FLIMData.FitType.GlobalRois;
                }

                if (calcLifetime)
                {
                    if (FLIM_ImgData.FLIM_on.Any(x => x == true))
                        FLIM_ImgData.calculateLifetime(-1);

                    //Image1.Invalidate();
                    //6 ms.
                }
            }
            else//showFLIM
                FLIMBitmap = null;
            //Debug.WriteLine("3 Elapsed time = " + sw.ElapsedMilliseconds + " ms");
            //sw.Restart();
            //0 - 3 ms to here.

            //Showing plot.
            if (realtime && plot_realtime.Visible)
            {
                if (FLIM_ImgData.State.Acq.acqFLIMA[currentChannel])
                {
                    FLIM_ImgData.calculate_MeanLifetime_ch(currentChannel, -1, FLIM_ImgData.offset);

                    if (plot_realtime.plotType == plot_timeCourse.plotWhat.meanIntensity || plot_realtime.plotType == plot_timeCourse.plotWhat.meanIntensity_bg)
                        realtimeData.Add(FLIM_ImgData.Roi.flim_parameters.meanIntensity[currentChannel] / (double)FLIM_ImgData.nAveragedFrame[currentChannel]);
                    else if (plot_realtime.plotType == plot_timeCourse.plotWhat.sumIntensity || plot_realtime.plotType == plot_timeCourse.plotWhat.sumIntensity_bg)
                        realtimeData.Add(FLIM_ImgData.Roi.flim_parameters.sumIntensity[currentChannel]);
                    else
                        realtimeData.Add(FLIM_ImgData.Roi.flim_parameters.tau_m_fromMAP[currentChannel] - FLIM_ImgData.offset[c]);
                }
                else
                {
                    FLIM_ImgData.calculate_MeanLifetime_ch(currentChannel, -1, FLIM_ImgData.offset); //calculate intensity
                    realtimeData.Add(FLIM_ImgData.Roi.flim_parameters.meanIntensity[currentChannel]);
                }
                //plot_realtime.realtimeData = realtimeData;

                plot_realtime.plotNow_realtime(realtimeData);
            }
            else if (!realtime)
            {

                if (plot_regular != null)
                    plot_regular.plotNow_noRealtime(TCF, TC, this, currentChannel);
            }

            if (FLIM_ImgData.State.Acq.acqFLIMA[currentChannel])
            {
                LifetimeCurvePlot.Invalidate();
            }

            if (!Channel12.Checked)
            {
                if (!focusing || !flimage.flimage_io.parameters.StripeDuringFocus)
                {
                    Image1.Invalidate();
                }

                Image2.Invalidate();
            }
            else
            {
                if (!focusing || !flimage.flimage_io.parameters.StripeDuringFocus)
                    DrawImages();
            }

            this.BeginInvokeIfRequired(o =>
            {
                try
                {
                    o.UpdateImageDisplay(calcLifetimeMap, calcLifetime, calcProject, c, focus);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error in Invoking Update ImageDisplay" + e.Message);
                }
            });

            update_image_busy = false;
        } //UpdateImges

        /////////////////////        
        /////////////////////

        public void UpdateImageDisplay(bool calcLifetimeMap, bool calcLifetime, bool calcProject, int c, bool focus)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool Ch12_checked = Channel12.Checked;
            bool showFLIM = ShowFLIM.Checked; //For now.
            int nChannels = FLIM_ImgData.nChannels;

            if (!Ch12_checked)
            {
                st_2Ch.Text = "FLIM";
            }
            else
            {
                st_2Ch.Text = "Intensity";
            }

            CurrentFastZPageTB.Text = (FLIM_ImgData.currentPage5D + 1).ToString();
            TotalFastZFrame.Text = "/ " + (FLIM_ImgData.n_pages5D).ToString();
            umPerSliceLabel.Text = "";

            if (FLIM_ImgData.n_pages > 0)
            {
                st_pageN.Text = "/ " + FLIM_ImgData.n_pages;
                if (FastZStack)
                    umPerSliceLabel.Text = String.Format("{0:0.0} μm / slice", FLIM_ImgData.State.Acq.FastZ_umPerSlice);
                else if (ZStack)
                    umPerSliceLabel.Text = String.Format("{0:0.0} μm / slice", FLIM_ImgData.State.Acq.sliceStep);
                else if (FLIM_ImgData.n_pages > 1 && FLIM_ImgData.acquiredTime_Pages.Length > 2)//Time course.
                {
                    if (FLIM_ImgData.currentPage >= 0 && FLIM_ImgData.currentPage < FLIM_ImgData.n_pages)
                    {
                        double FrameTime = FLIM_ImgData.acquiredTime_Pages[FLIM_ImgData.currentPage].Subtract(FLIM_ImgData.acquiredTime_Pages[0]).TotalSeconds;
                        umPerSliceLabel.Text = String.Format("{0:0.00} s", FrameTime);
                    }
                }


                if (displayZProjection)
                {
                    c_page.Text = String.Format("{0} - {1}", page_range[0] + 1, page_range[1]);
                    PageStart.Text = (page_range[0] + 1).ToString();
                    PageEnd.Text = (page_range[1]).ToString();
                }
                else
                {
                    c_page.Text = String.Format("{0}", FLIM_ImgData.currentPage + 1);
                    PageStart.Text = (FLIM_ImgData.currentPage + 1).ToString();
                    PageEnd.Text = (FLIM_ImgData.currentPage + 1).ToString();
                }
            }


            if (FrameAdjustment.Checked)
            {
                double nAverageFrame = FLIM_ImgData.nAveragedFrame[c];
                if (!FLIM_ImgData.ZStack && FLIM_ImgData.State.Acq.nAveSlice > 1)
                    nAverageFrame = nAverageFrame * FLIM_ImgData.State.Acq.nAveSlice;

                FrameAdjustment.Text = "Adjust for #frames (Frame = " + nAverageFrame + ")";
                if (FLIM_ImgData.ZProjection && FLIM_ImgData.z_projection_type == FLIMData.projectionType.Sum)
                    FrameAdjustment.Text = "Adjust for #frames (Frame = " + FLIM_ImgData.nAveragedFrame[c] + ", Page =" + (page_range[1] - page_range[0] + 1) + ")";
            }
            else
                FrameAdjustment.Text = "Adjust for #frames";

            MapStateIntensityRange();


            if (Auto1.Checked)
                UpdateImageParamText();

            Auto1.Checked = false; //Always.

            //If this is turned on, the slider is updated all the time, and cause problems during focusing.
            //UpdateImageParamText(); //SetSlider and text update.

        }

        //////////////////////////////////////
        public enum ImageDisplay_State
        {
            Opening = 1,
            Grabbing = 2,
            Focusing = 3,
            Idle = 4,
            Roi_TaskWaiting = 10
        }


        public enum drawROI_State
        {
            Creating = 1,
            Moving = 2,
            Resizing = 3,
            Moving_ExistingROI = 5,
            Resizing_ExistingROI = 6,
            Idle = 10,
            NoROI = 11,
            Inactive = 100
        }


        public enum UncagingCursor
        {
            Creating = 1,
            Moving = 2,
            Doubleclicked = 3,
            Moving_ExistingROI = 4,
            Idle = 10,
            caibration = 11,
            Inactive = 100
        }

    } //image_display
} //namespace
