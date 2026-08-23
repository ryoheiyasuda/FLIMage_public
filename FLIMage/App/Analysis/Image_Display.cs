
using FLIMage.FileFormat;
using FLIMage.FlowControls;
using FLIMage.Plotting;
using MathLibrary;
using Microsoft.VisualBasic;
using PhysiologyCSharp;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using TCSPC_controls;
using Utilites;
using Utilities;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.AxHost;
using File = System.IO.File;
//using System.Windows.Input;


namespace FLIMage.Analysis
{
    public partial class Image_Display : Form
    {
        const int N_MAXCHANNEL = 4;
        const int N_RANGE = 2; //start, end;
        const int N_DISPLAYCHANNEL = 2;
        const int N_TOTALDISPLAY = 3; //Ch1, Ch2, Ch12.
        const int N_FITPARAM = 6;
        const int CLASSIC_TIFF_MAX_FRAME_COUNT = short.MaxValue;

        const int MIN_INTENSITY_ROUND = 3;

        const String TempFileName = "Temp";


        double[] INITIAL_LIFETIME_RANGE = new double[] { 2.5, 3.5 };

        public FLIMData FLIM_ImgData;
        public string photon_filename = "";
        AutoResetEvent autoResetPhotonFileEvent = new AutoResetEvent(false);

        /// Drawing
        Bitmap IntensityBitmap;
        Bitmap IntensityBitmap2; //Second channel;
        Bitmap IntensityBitmap_Save, IntensityBitmap_Save2;
        Bitmap Bitmap_FastZ_PhaseComplementary; //For phase-detection mode. Showing complementary phases at once. 
        Bitmap FLIMBitmap;

        public Bitmap BlankBMP = ImageProcessing.FormatImage(new double[] { -1.0, 1.0 }, new double[] { 0, -1 }, MatrixCalc.MatrixCreate2D<ushort>(128, 128));

        ushort[][,] FocusProjectBuffer;
        double[][,] FocusFLIMimageBuffer;
        public byte[][] pixelsBuffer;

        //ROI manipulation
        //Rectangle boxRoi_FLIM;
        public ROI imageRoi = new ROI();
        ROI imageRoiSave = new ROI();
        ROI imageRoiOld = new ROI();
        ROI CurrentROIBeforeMove = new ROI();
        private const int LineScanTraceCurrentRoiId = -3;
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

        public FileIO.FileFormat fileformat = FileIO.FileFormat.None;

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
        private bool updatingPsPerUnitText = false;
        private bool updatingFitRangeText = false;
        private bool fitRangeTextEdited = false;

        //int nAveragedSave;
        double[][] intensity_range_perFrame;
        double[][] FLIM_intensity_range_perFrame;
        double[][] FLIM_lifetime_range_perFrame; //just to save.

        public double[][] State_intensity_range;
        double[][] State_FLIM_intensity_range;
        double[][] State_FLIM_lifetime_range;

        public double[][] thresholdFLIM_low_high = new double[N_MAXCHANNEL][];

        //Pages        
        //int[] page_range = new int[N_RANGE]; //start end
        public bool displayZProjection = false;
        public bool entireStack = true;

        //Uncaging
        public UncagingCursor drawUncagingPos = UncagingCursor.Inactive;
        private Point uncagingLoc = new Point(-1, -1); //Temporary uncaging location --- it will move with cursor.
        private Point uLoc = new Point(-1, -1); //It is the position of uncaging on display. 
        public List<double[]> uncagingLocs = new List<double[]>();
        public List<double[]> uncagingLocs_calib = new List<double[]>(); // For calibration (New). 2023.
        public Point referenceLoc = new Point(-1, -1); //For calibration (Orange)
        public double[] uncagingLocFrac = new double[] { -1, -1 };

        //Subpanels for split scanning
        public int[] subPanel = new int[] { 1, 1 };

        //public bool moveUncaging = false;
        Stopwatch moveMouseEvent = new Stopwatch();

        delegate void UncagePosStartFunc();
        public bool uncaging_on = false;
        public bool uncaging_calib_mode = false;
        public bool uncaging_calib_multi_mode = false;

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

        // Line-scan-trace ROI (reserved for scanning, not for analysis)
        Pen lineScanTracePen = new Pen(Brushes.Orange, 2.0F);
        Pen lineScanTracePenOutOfFocus = new Pen(Brushes.Orange, 0.5F);

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

        // Fiber photometry CSV (append-only during realtime acquisition)
        private string _fiberPhotometryCsvPath = null;
        private string _fiberPhotometryCsvStem = null;
        private int _fiberPhotometrySampleIndex = 0;
        private double _fiberPhotometryBin_s = 0.02;
        private bool _fiberPhotometryCsvLoaded = false;
        private double[] _fiberPhotometryLoadedMicrotime_ps = null;
        private List<DateTime> _fiberPhotometryLoadedMacroTime = null;
        private List<double> _fiberPhotometryLoadedElapsed_s = null;
        private List<double> _fiberPhotometryLoadedMeanLifetime_ns = null;
        private List<double> _fiberPhotometryLoadedSumIntensity = null;
        private List<ushort[]> _fiberPhotometryLoadedDecay = null;

        public plot_timeCourse plot_realtime;

        // Avoid re-entrant / duplicated phase calculations triggered by UI refreshes.
        private int _phaseCalcQueued = 0;
        public plot_timeCourse plot_regular;
        //
        public TimeCourse TC = new TimeCourse();
        public TimeCourse_Files TCF = new TimeCourse_Files();
        //
        int panelMerginY, imageMerginY, st_MerginY;
        public FLIMageMain flimage;

        private volatile bool StopFileOpening = false;
        public bool StopFileOpeningRequested => StopFileOpening;
        // Scan parameters

        TrackBar[][] MaxMinSliders;// = new TrackBar[3][];
        TextBox[][] MaxMinTextBox; //= new TextBox[3][];
        double[] SldrZoom = new double[] { 10.0, 10.0, 100.0 };

        public string AnalysisStatus = "None";

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

        public ImageProcessing.ColorScheme CurrentColorScheme => color_scheme;

        public double[] GetCurrentFlimLifetimeRange(int channel)
        {
            if (State_FLIM_lifetime_range == null || channel < 0 || channel >= State_FLIM_lifetime_range.Length || State_FLIM_lifetime_range[channel] == null)
                return null;

            return (double[])State_FLIM_lifetime_range[channel].Clone();
        }

        public double[] GetCurrentFlimIntensityRange(int channel)
        {
            if (State_FLIM_intensity_range == null || channel < 0 || channel >= State_FLIM_intensity_range.Length || State_FLIM_intensity_range[channel] == null)
                return null;

            return (double[])State_FLIM_intensity_range[channel].Clone();
        }

        public double[] GetCurrentFlimThresholdRange(int channel)
        {
            if (thresholdFLIM_low_high == null || channel < 0 || channel >= thresholdFLIM_low_high.Length || thresholdFLIM_low_high[channel] == null)
                return null;

            return (double[])thresholdFLIM_low_high[channel].Clone();
        }

        String[] ZStack_Text = { "Swtich to ZStack", "Swtich to Timecourse" };

        public PhotonFileHandle photon_file_handle;
        public AVIWriter[] movie_wirters;
        //bool SavePageInMemory = false;

        // Context menu item: mark a traceable ROI as the line-scan trace
        private ToolStripMenuItem _useThisTraceForLineScanningMenuItem_inROI;
        private ToolStripMenuItem _useThisTraceForLineScanningMenuItem_selectedROI;
        private ToolStripSeparator _useThisTraceForLineScanningSeparator_inROI;
        private ToolStripSeparator _useThisTraceForLineScanningSeparator_selectedROI;

        // Guard against re-entrancy for auto scan-delay calibration
        private volatile bool _autoResonantDelayRunning = false;
        private const double AutoScanDelayMaxMissingFraction = 0.60;



        public Image_Display(FLIMData FLIM, FLIMageMain mc, bool createSim)
        {
            InitializeComponent();

            flimage = mc;

            InitLineScanTraceContextMenus();

            FLIM_ImgData = FLIM;
            psPerUnit.ReadOnly = true;
            fit_start.TextChanged += FitRangeTextChanged;
            fit_end.TextChanged += FitRangeTextChanged;
            if (flimage != null && flimage.KeepPagesInMemoryCheck != null)
                FLIM_ImgData.KeepPagesInMemory = flimage.KeepPagesInMemoryCheck.Checked;
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

            FLIM.ZProjection_Range[0] = 0;

            if (FLIM.nFastZ > 1)
                FLIM.ZProjection_Range[1] = FLIM.nFastZ;
            else
                FLIM.ZProjection_Range[1] = FLIM.n_pages;

            PageEnableControl();

            MapControls();

            int nChannels = FLIM.nChannels;
            //FLIM.loadFittingParamFromState();


            fit_range = new int[N_MAXCHANNEL][];
            fit_param = new double[N_MAXCHANNEL][];

            for (int ch = 0; ch < FLIM.fit_range.Length; ch++)
            {
                fit_range[ch] = (int[])FLIM.fit_range[ch].Clone();
                fit_param[ch] = (double[])((double[])FLIM.State.Spc.analysis.GetType().GetField("fit_param" + (ch + 1)).GetValue(FLIM.State.Spc.analysis)).Clone();
            }

            colorBar.Image = ImageProcessing.CreateColorBar(colorBar.Width, colorBar.Height, ImageProcessing.ColorBarDirection.RightToLeft, color_scheme);

            Auto1.Checked = true;
            intensity_range_perFrame = MatrixCalc.MatrixCreate2D<double>(N_MAXCHANNEL, N_RANGE);
            FLIM_intensity_range_perFrame = MatrixCalc.MatrixCreate2D<double>(N_MAXCHANNEL, N_RANGE);
            FLIM_lifetime_range_perFrame = MatrixCalc.MatrixCreate2D<double>(N_MAXCHANNEL, N_RANGE);

            for (int i = 0; i < N_MAXCHANNEL; i++)
                SaveIntensity_Range(i, true);

            for (int i = 0; i < N_MAXCHANNEL; i++)
                thresholdFLIM_low_high[i] = new double[2];

            //UpdateImageParamText();
            //for (int ch = 0; ch < nChannels; ch++)
            //    UpdateFittingParam(ch, false);

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
            flimage.flimage_io.EventNotify += new FLIMage_IO.FLIMage_EventHandler(EventHandling);

            LoadSetting();

            colorSchemeCheck();
            intelMKLLibraryOnToolStripMenuItem.Checked = MatrixCalc.IntelMKL_on;
            SetFastZModeDisplay(false);
            plot_realtime = new plot_timeCourse(true, this);
            plot_regular = new plot_timeCourse(false, this);
            if (AutoApplyOffset.Checked)
                plot_regular.TurnOnCalcUponOpen(true);

            // KENGO BEGIN 1-8-2026
            // Attach MouseDown so right-clicks are handled for Page Up/Down
            this.PageUp.MouseDown -= Page_UpDown_MouseDown;
            this.PageUp.MouseDown += Page_UpDown_MouseDown;
            this.PageDown.MouseDown -= Page_UpDown_MouseDown;
            this.PageDown.MouseDown += Page_UpDown_MouseDown;
            this.PageUpUp.MouseDown -= Page_UpDown_MouseDown;
            this.PageUpUp.MouseDown += Page_UpDown_MouseDown;
            this.PageDownDown.MouseDown -= Page_UpDown_MouseDown;
            this.PageDownDown.MouseDown += Page_UpDown_MouseDown;
            // KENGO END

            // KENGO BEGIN 1-25-2026
            // Ensure the Lifetime channel panel enable state follows Channel12.Checked at startup.
            if (LifetimeCh_panel != null && Channel12 != null)
                LifetimeCh_panel.Enabled = Channel12.Checked;
            // KENGO END
        }

        void LoadSetting()
        {
            settingManager = new SettingManager(settingName, flimage.State.Files.initFolderPath);
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

            UiState.Load(this);
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

            SaveUISetting();
        }

        public void SaveUISetting()
        {
            UiState.Save(this);
        }

        public void EventHandling(FLIMage_IO fc, ProcessEventArgs e)
        {
            String eventStr = e.EventName;
            try
            {
                switch (eventStr)
                {
                    case "GrabStart":
                        {
                            image_display_state = ImageDisplay_State.Grabbing;
                            break;
                        }
                    case "AcquisitionDone":
                        {
                            image_display_state = ImageDisplay_State.Idle;
                            break;
                        }
                    case "FrameDone":
                        {
                            break;
                        }
                    case "SaveFrameDone":
                        {
                            break;
                        }
                    case "SaveSliceDone":
                        {
                            break;
                        }
                    case "SaveFileDone":
                        {
                            break;
                        }
                    case "SliceAcquisitionStart":
                        {
                            break;
                        }
                    case "SliceAcquisitionDone":
                        {
                            break;
                        }
                    case "FocusStart":
                        {
                            image_display_state = ImageDisplay_State.Focusing;
                            // Tetsuya BEGIN 06-29-2026
                            // Refresh intensity range for FOCUS frame count on start
                            // (previously: only image_display_state = Focusing; then break)
                            focusing = true;
                            RefreshIntensityRangeForDisplay();
                            // Tetsuya END
                            break;
                        }
                    case "FocusStop":
                        {
                            image_display_state = ImageDisplay_State.Idle;
                            break;
                        }
                    case "GrabAbort":
                        {
                            image_display_state = ImageDisplay_State.Idle;
                            break;
                        }
                    case "FLIMageStarted":
                        {
                            image_display_state = ImageDisplay_State.Idle;
                            break;
                        }
                    case "OpenFileStart":
                        {
                            image_display_state = ImageDisplay_State.Opening;
                            break;
                        }

                    case "ReadFileDone":
                        {
                            image_display_state = ImageDisplay_State.Idle;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            catch (Exception E)
            {
                Debug.WriteLine("Problem in user function (" + E.Source + "): " + E.Message);
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
            FLIM_ImgData.ResetFitting(resetAllPages);
            fittingDone = false;
        }

        private int GetSelectedFittingChannel()
        {
            return Ch2.Checked ? 1 : 0;
        }

        private bool HasFitRangeData(int channel)
        {
            if (FLIM_ImgData == null || channel < 0 || channel >= FLIM_ImgData.nChannels)
                return false;

            if (FLIM_ImgData.n_time == null || channel >= FLIM_ImgData.n_time.Length || FLIM_ImgData.n_time[channel] <= 1)
                return false;

            if (FLIM_ImgData.FLIM_on != null && channel < FLIM_ImgData.FLIM_on.Length && !FLIM_ImgData.FLIM_on[channel])
                return false;

            var acqFLIMA = FLIM_ImgData.State?.Acq?.acqFLIMA;
            if (acqFLIMA != null && channel < acqFLIMA.Length && !acqFLIMA[channel])
                return false;

            return true;
        }

        private void FitRangeTextChanged(object sender, EventArgs e)
        {
            if (!updatingFitRangeText)
                fitRangeTextEdited = true;
        }

        private void SetFitRangeText(int start, int end)
        {
            updatingFitRangeText = true;
            try
            {
                fit_start.Text = start.ToString();
                fit_end.Text = end.ToString();
            }
            finally
            {
                updatingFitRangeText = false;
            }
        }

        private int[][] CaptureFitRangeFromGui(bool includeCachedRanges = true)
        {
            int[][] range = new int[N_MAXCHANNEL][];
            if (includeCachedRanges && fit_range != null)
            {
                for (int ch = 0; ch < Math.Min(fit_range.Length, range.Length); ch++)
                    if (HasFitRangeData(ch) && fit_range[ch] != null)
                        range[ch] = (int[])fit_range[ch].Clone();
            }

            int c = GetSelectedFittingChannel();
            if (c >= 0 && c < range.Length && (includeCachedRanges || fitRangeTextEdited))
            {
                bool fitStartOk = Int32.TryParse(fit_start.Text, out int fitStartText);
                bool fitEndOk = Int32.TryParse(fit_end.Text, out int fitEndText);
                if (fitStartOk && fitEndOk && fitStartText >= 1 && fitEndText > fitStartText)
                {
                    range[c] = new int[] { fitStartText - 1, fitEndText };
                }
                else if (range[c] == null && HasFitRangeData(c))
                {
                    range[c] = new int[] { 0, FLIM_ImgData.n_time[c] };
                }
            }

            return range;
        }

        private int[] ClampFitRangeToImageData(int[] range, int channel)
        {
            if (range == null || range.Length < 2 || FLIM_ImgData == null || FLIM_ImgData.n_time == null || channel >= FLIM_ImgData.n_time.Length)
                return null;

            int nTime = FLIM_ImgData.n_time[channel];
            if (nTime <= 1)
                return null;

            int start = Math.Max(0, Math.Min(range[0], Math.Max(0, nTime - 1)));
            int end = Math.Max(start + 1, Math.Min(range[1], nTime));
            return new int[] { start, end };
        }

        private bool ApplyFitRangeFromGui(int[][] guiFitRange)
        {
            if (guiFitRange == null || FLIM_ImgData == null || FLIM_ImgData.fit_range == null || FLIM_ImgData.State == null)
                return false;

            if (fit_range == null || fit_range.Length != N_MAXCHANNEL)
                fit_range = new int[N_MAXCHANNEL][];

            int nChannels = Math.Min(FLIM_ImgData.nChannels, Math.Min(N_MAXCHANNEL, FLIM_ImgData.fit_range.Length));
            object analysis = FLIM_ImgData.State.Spc.analysis;
            bool changed = false;
            for (int ch = 0; ch < nChannels; ch++)
            {
                if (ch >= guiFitRange.Length || guiFitRange[ch] == null)
                    continue;
                if (!HasFitRangeData(ch))
                    continue;

                int[] range = ClampFitRangeToImageData(guiFitRange[ch], ch);
                if (range == null)
                    continue;

                if (fit_range[ch] == null || !fit_range[ch].SequenceEqual(range))
                    changed = true;
                if (FLIM_ImgData.fit_range[ch] == null || !FLIM_ImgData.fit_range[ch].SequenceEqual(range))
                    changed = true;

                fit_range[ch] = (int[])range.Clone();
                FLIM_ImgData.fit_range[ch] = (int[])range.Clone();

                var field = analysis.GetType().GetField("fit_range" + (ch + 1));
                if (field != null)
                    field.SetValue(analysis, range.Clone());
            }

            int c = GetSelectedFittingChannel();
            if (c >= 0 && c < fit_range.Length && HasFitRangeData(c) && fit_range[c] != null)
            {
                SetFitRangeText(fit_range[c][0] + 1, fit_range[c][1]);
            }

            if (changed)
                FLIM_ImgData.fitRangeChanged();

            return changed;
        }

        private int CaptureFilterWindowFromGui()
        {
            if (filterWindow != null && Int32.TryParse(filterWindow.Text, out int fw))
                return Math.Max(0, fw);

            return Math.Max(0, FLIM_ImgData?.State?.Display?.filterWindow_FLIM ?? 0);
        }

        private void ApplyFilterWindowToState(int fw)
        {
            fw = Math.Max(0, fw);
            if (FLIM_ImgData?.State?.Display != null)
                FLIM_ImgData.State.Display.filterWindow_FLIM = fw;

            if (filterWindow != null && filterWindow.Text != fw.ToString())
                filterWindow.Text = fw.ToString();
        }


        private void UpdateFittingParam(int c, bool fit_done)
        {
            double[] betahat;
            int nChannels = FLIM_ImgData.nChannels;
            if (c >= nChannels)
                c = nChannels - 1;
            if (c < 0)
                c = 0;

            SyncPsPerUnitFromState(c);
            if (!HasFitRangeData(c))
                return;

            double res = FLIM_ImgData.psPerUnit / 1000;

            int[] sourceRange = FLIM_ImgData.fit_range != null && c < FLIM_ImgData.fit_range.Length
                ? FLIM_ImgData.fit_range[c]
                : null;
            int[] appliedRange = FLIM_ImgData.ApplyFitRange(sourceRange, c);
            if (fit_range != null && c < fit_range.Length)
                fit_range[c] = (int[])appliedRange.Clone();


            Object obj = FLIM_ImgData.State.Spc.analysis;
            obj.GetType().GetField("fit_range" + (c + 1)).SetValue(obj, appliedRange.Clone());

            SetFitRangeText(appliedRange[0] + 1, appliedRange[1]); //1 base. If 1-64, this means 0 <= x < 64)

            bool showFit = false;
            bool singleExp_fit = SingleExp.Checked;

            int n_para = 8;

            double[] param1 = new double[n_para + 2];

            fittingDone = fit_done;

            var beta_src = FLIM_ImgData.RoiFit.flim_parameters.beta0[c];
            if (fit_done || beta_src == null)
                beta_src = FLIM_ImgData.RoiFit.flim_parameters.beta[c];

            if (beta_src != null)
                betahat = (double[])beta_src.Clone();
            else
                betahat = new double[] { fit_param[c][0], res / fit_param[c][1], fit_param[c][2], res / fit_param[c][3],
                    fit_param[c][4] / res, fit_param[c][5] / res, 0};

            //KENGO BEGIN 2-3-2026
            //Add the following part to prevent the fixed fit parameters from resetting when moved the ROI.
            //'betahat' values are restored from 'fit_param'.  Maybe this is not the best way.
            if (singleExp_fit)
            {
                if (cb_tau1Fix.Checked)
                    betahat[1] = res / fit_param[c][1];
                if (cb_tauGFix.Checked)
                    betahat[2] = fit_param[c][4] / res;
                if (cb_T0Fix.Checked)
                    betahat[3] = fit_param[c][5] / res;
            }
            else
            {
                if (cb_tau1Fix.Checked)
                    betahat[1] = res / fit_param[c][1];
                if (cb_tau2Fix.Checked)
                    betahat[3] = res / fit_param[c][3];
                if (cb_tauGFix.Checked)
                    betahat[4] = fit_param[c][4] / res;
                if (cb_T0Fix.Checked)
                    betahat[5] = fit_param[c][5] / res;
            }
            //KENGO END

            FLIM_ImgData.RoiFit.flim_parameters.beta0[c] = (double[])betahat.Clone();

            if (fittingDone && FLIM_ImgData.Fit_type.Equals(FLIMData.FitType.GlobalRois) && Values_selectedROI.Checked && FLIM_ImgData.ROIs.Count > FLIM_ImgData.currentRoi)
            {
                if (FLIM_ImgData.currentRoi >= 0 && FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters.beta.Length > c)
                    betahat = (double[])FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters.beta[c].Clone();
                //else
                //    return;
            }

            if (singleExp_fit)
            {
                param1[0] = betahat[0];
                param1[1] = res / betahat[1];
                param1[2] = fit_param[c][2];
                param1[3] = fit_param[c][3];
                param1[4] = betahat[2] * res;
                param1[5] = betahat[3] * res;
                param1[6] = betahat.Length > 4 ? betahat[4] : 0;

                showFit = true;

                fit_param[c] = (double[])param1.Clone();

                param1[2] = 0;
                param1[3] = 0;
            }
            else
            {
                param1[0] = betahat[0];
                param1[1] = res / betahat[1];
                param1[2] = betahat[2];
                param1[3] = res / betahat[3];
                param1[4] = betahat[4] * res;
                param1[5] = betahat[5] * res;
                param1[6] = betahat.Length > 6 ? betahat[6] : 0;

                showFit = true;

                fit_param[c] = (double[])param1.Clone();
            }
            //else
            //{
            //    param1 = (double[])fit_param[c].Clone();
            //}

            pop1.Text = String.Format("{0:0.0}", param1[0]);
            tau1.Text = String.Format("{0:0.000}", param1[1]);
            pop2.Text = String.Format("{0:0.0}", param1[2]);
            tau2.Text = String.Format("{0:0.000}", param1[3]);
            tauG.Text = String.Format("{0:0.000}", param1[4]);
            t0.Text = String.Format("{0:0.000}", param1[5]);
            BG.Text = String.Format("{0:0.0}", param1[6]);
            if (!cb_BGfix.Checked)
                UpdateBackgroundPerPixelsText(c, GetStoredBackgroundPerMillionPixel(c), false);

            if (showFit)
            {
                frac1.Text = String.Format("{0:0.0%}", param1[0] / (param1[0] + param1[2]));
                frac2.Text = String.Format("{0:0.0%}", param1[2] / (param1[0] + param1[2]));


                if (fittingDone && FLIM_ImgData.Fit_type.Equals(FLIMData.FitType.GlobalRois) && Values_selectedROI.Checked && FLIM_ImgData.ROIs.Count > FLIM_ImgData.currentRoi && FLIM_ImgData.currentRoi >= 0 && FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters.beta.Length > c)
                {
                    var tau_mean = FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters.tau_m[c];
                    var xi_square_roi = FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters.xi_square[c];
                    tau_m.Text = String.Format("{0:0.000}", tau_mean);
                    xi_square.Text = String.Format("{0:0.000}", xi_square_roi);
                }
                else
                {
                    tau_m.Text = String.Format("{0:0.000}", FLIM_ImgData.RoiFit.flim_parameters.tau_m[c]);
                    xi_square.Text = String.Format("{0:0.000}", FLIM_ImgData.RoiFit.flim_parameters.xi_square[c]);
                }
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

            // Save setting controls 
            SaveUISetting();

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

            SyncPsPerUnitFromState(c);

            if (HasFitRangeData(c))
            {
                double valD;

                var betasave = (double[])FLIM_ImgData.RoiFit.flim_parameters.beta0[c].Clone();
                var rangesave = (int[])FLIM_ImgData.fit_range[c].Clone();

                bool fitStartOk = Int32.TryParse(fit_start.Text, out int fitStartText);
                bool fitEndOk = Int32.TryParse(fit_end.Text, out int fitEndText);
                if (Double.TryParse(pop1.Text, out valD)) fit_param[c][0] = valD;
                if (Double.TryParse(tau1.Text, out valD)) fit_param[c][1] = valD;
                if (Double.TryParse(pop2.Text, out valD)) fit_param[c][2] = valD;
                if (Double.TryParse(tau2.Text, out valD)) fit_param[c][3] = valD;
                if (Double.TryParse(tauG.Text, out valD)) fit_param[c][4] = valD;
                if (Double.TryParse(t0.Text, out valD)) fit_param[c][5] = valD;

                if (FLIM_ImgData.n_time[c] > 1 && fitStartOk && fitEndOk && fitStartText >= 1 && fitEndText > fitStartText)
                {
                    int[] range = ClampFitRangeToImageData(new int[] { fitStartText - 1, fitEndText }, c);
                    if (range != null)
                    {
                        fit_range[c] = (int[])range.Clone();
                        FLIM_ImgData.fit_range[c] = (int[])range.Clone();
                    }
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
        public void SetupRealtimeImaging(ScanParameters Scan)
        {
            ScanParameters State = Scan;
            //FLIM_ImgData = flimdata;

            SyncPsPerUnitFromState(currentChannel);

            UpdateImages(true, false, false, true); //30 ms.            
            Refresh();

            FastZStack = (State.Acq.fastZScan && State.Acq.FastZ_nSlices > 1);

            bool projectionCondition = State.Acq.ZStack && !FastZStack;
            Set_DisplayProjection(projectionCondition);
            Set_EntireStack(projectionCondition);

            realtimeData.Clear();
            realtimeTime.Clear();
            ResetFiberPhotometryCsv();
            ImageRoiToFLIMage();
        }

        private bool IsFiberPhotometryMode()
        {
            try
            {
                if (_fiberPhotometryCsvLoaded)
                    return true;

                if (FLIM_ImgData?.State?.Acq?.fiberPhotometryMode == true)
                    return true;

                // Prefer the runtime enum (authoritative during acquisition), fallback to persisted string.
                if (flimage?.flimage_io != null && flimage.flimage_io.microscope_system == FLIMage.MicroscopeSystem.FiberPhotometry)
                    return true;

                if ((FLIM_ImgData?.State?.Init?.MicroscopeSystem ?? "").ToLower().Contains("fiber"))
                    return true;

                if (FLIM_ImgData != null && FLIM_ImgData.width == 1)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        private void ResetFiberPhotometryCsv()
        {
            // We lazily create the CSV on first append (so we have the correct baseName###).
            _fiberPhotometryCsvPath = null;
            _fiberPhotometryCsvStem = null;
            _fiberPhotometrySampleIndex = 0;
            _fiberPhotometryCsvLoaded = false;
            _fiberPhotometryLoadedMicrotime_ps = null;
            _fiberPhotometryLoadedMacroTime = null;
            _fiberPhotometryLoadedElapsed_s = null;
            _fiberPhotometryLoadedMeanLifetime_ns = null;
            _fiberPhotometryLoadedSumIntensity = null;
            _fiberPhotometryLoadedDecay = null;
        }


        private void EnsureFiberPhotometryRoi()
        {
            if (!IsFiberPhotometryMode() || FLIM_ImgData == null)
                return;

            int w = Math.Max(1, FLIM_ImgData.width);
            int h = Math.Max(1, FLIM_ImgData.height);
            int nCh = Math.Max(1, FLIM_ImgData.nChannels);

            // Fiber photometry is a single point; ROI selection from imaging would be out-of-bounds.
            // Force whole-image ROI so lifetime calculation never indexes outside [0,0].
            FLIM_ImgData.Fit_type = FLIMData.FitType.WholeImage;

            // Ensure ROI and ROI-fit match the current data size.
            if (FLIM_ImgData.Roi == null ||
                FLIM_ImgData.Roi.Rect.X != 0 || FLIM_ImgData.Roi.Rect.Y != 0 ||
                FLIM_ImgData.Roi.Rect.Width != w || FLIM_ImgData.Roi.Rect.Height != h)
            {
                FLIM_ImgData.Roi = new ROI(ROI.ROItype.Rectangle, new Rectangle(0, 0, w, h), nCh, -1, false, new int[] { 0, 0 });
            }

            if (FLIM_ImgData.RoiFit == null ||
                FLIM_ImgData.RoiFit.Rect.X != 0 || FLIM_ImgData.RoiFit.Rect.Y != 0 ||
                FLIM_ImgData.RoiFit.Rect.Width != w || FLIM_ImgData.RoiFit.Rect.Height != h)
            {
                FLIM_ImgData.RoiFit = new ROI(ROI.ROItype.Rectangle, new Rectangle(0, 0, w, h), nCh, -100, false, new int[] { 0, 0 });
            }
        }

        private void EnsureFiberPhotometryCsvInitialized(int channel, int nMicroBins)
        {
            try
            {
                if (!IsFiberPhotometryMode())
                    return;

                // Only for GRAB (not focus)
                if (flimage?.flimage_io != null && flimage.flimage_io.focusing)
                    return;

                string basePath = FLIM_ImgData?.State?.Files?.pathName;
                if (string.IsNullOrWhiteSpace(basePath))
                    basePath = FLIM_ImgData?.pathName;
                if (string.IsNullOrWhiteSpace(basePath))
                    return;

                // File stem follows FLIM convention: baseName### (State.Files.fileName)
                string stem = FLIM_ImgData?.State?.Files?.fileName;
                if (string.IsNullOrWhiteSpace(stem))
                {
                    string bn = FLIM_ImgData?.State?.Files?.baseName ?? "Fiber";
                    int fc = FLIM_ImgData?.State?.Files?.fileCounter ?? 1;
                    stem = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1:000}", bn, fc);
                }

                // If a new acquisition started (file stem changed), reset.
                if (_fiberPhotometryCsvStem != stem)
                {
                    _fiberPhotometryCsvStem = stem;
                    _fiberPhotometrySampleIndex = 0;
                }

                if (_fiberPhotometryCsvPath != null)
                    return;

                _fiberPhotometryBin_s = 0.02;
                try { _fiberPhotometryBin_s = FLIM_ImgData.State.Acq.msPerLineActual() / 1000.0; } catch { }
                if (_fiberPhotometryBin_s <= 0) _fiberPhotometryBin_s = 0.02;

                _fiberPhotometryCsvPath = Path.Combine(basePath, stem + ".csv");

                // Header row: fixed columns first, then one column per microtime bin (ps).
                double res_ps = 0.0;
                try { res_ps = FLIM_ImgData.State.Spc.spcData.resolution[channel]; } catch { res_ps = 250.0; }

                var sb = new StringBuilder();

                sb.Append("MacroTimeISO,Elapsed_s,Channel,MeanIntensity,SumIntensity,MeanLifetime_ns");
                for (int t = 0; t < nMicroBins; t++)
                {
                    double ps = t * res_ps; // stored bins are already startPoint-subtracted
                    sb.Append(",");
                    sb.Append("ps_");
                    sb.Append(ps.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
                }
                sb.AppendLine();

                File.WriteAllText(_fiberPhotometryCsvPath, sb.ToString());
            }
            catch
            {
                // ignore file I/O errors during realtime
                _fiberPhotometryCsvPath = null;
            }
        }

        private void AppendFiberPhotometryCsvRow(int channel, ushort[] decayCounts, double meanIntensity, double sumIntensity, double meanLifetime_ns)
        {
            if (_fiberPhotometryCsvPath == null || decayCounts == null)
                return;

            try
            {
                string ts = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
                double elapsed_s = _fiberPhotometrySampleIndex * _fiberPhotometryBin_s;

                var sb = new StringBuilder();
                sb.Append(ts);
                sb.Append(",");
                sb.Append(elapsed_s.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",");
                sb.Append((channel + 1).ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",");
                sb.Append(meanIntensity.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",");
                sb.Append(sumIntensity.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",");
                sb.Append(meanLifetime_ns.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture));

                for (int t = 0; t < decayCounts.Length; t++)
                {
                    sb.Append(",");
                    sb.Append(decayCounts[t].ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                sb.AppendLine();

                File.AppendAllText(_fiberPhotometryCsvPath, sb.ToString());
                _fiberPhotometrySampleIndex++;
            }
            catch
            {
                // ignore
            }
        }

        private FileIO.FileError OpenFiberPhotometryCsv(string fn)
        {
            try
            {
                var lines = File.ReadLines(fn);
                using (var it = lines.GetEnumerator())
                {
                    if (!it.MoveNext())
                        return FileIO.FileError.NotFound;

                    string header = it.Current ?? "";
                    var cols = header.Split(',');
                    if (cols.Length < 7 || cols[0] != "MacroTimeISO")
                        return FileIO.FileError.UnKnown;

                    int fixedCols = 6; // MacroTimeISO,Elapsed_s,Channel,MeanIntensity,SumIntensity,MeanLifetime_ns
                    int nBins = cols.Length - fixedCols;
                    if (nBins <= 1)
                        return FileIO.FileError.UnKnown;

                    var micro_ps = new double[nBins];
                    for (int i = 0; i < nBins; i++)
                    {
                        string name = cols[fixedCols + i];
                        if (name.StartsWith("ps_", StringComparison.Ordinal))
                            name = name.Substring(3);
                        double.TryParse(name, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out micro_ps[i]);
                    }

                    var macro = new List<DateTime>();
                    var elapsed = new List<double>();
                    var meanTau = new List<double>();
                    var sumI = new List<double>();
                    var decay = new List<ushort[]>();

                    while (it.MoveNext())
                    {
                        string line = it.Current;
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var p = line.Split(',');
                        if (p.Length < cols.Length)
                            continue;

                        if (!DateTime.TryParse(p[0], out DateTime ts))
                            continue;
                        if (!double.TryParse(p[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double t_s))
                            t_s = 0;
                        // channel (p[2]) currently ignored; we keep the curve as stored.
                        double.TryParse(p[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double sumIntensity);
                        double.TryParse(p[5], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double tau_ns);

                        var d = new ushort[nBins];
                        for (int i = 0; i < nBins; i++)
                        {
                            ushort.TryParse(p[fixedCols + i], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out d[i]);
                        }

                        macro.Add(ts);
                        elapsed.Add(t_s);
                        meanTau.Add(tau_ns);
                        sumI.Add(sumIntensity);
                        decay.Add(d);
                    }

                    _fiberPhotometryCsvLoaded = true;
                    _fiberPhotometryLoadedMicrotime_ps = micro_ps;
                    _fiberPhotometryLoadedMacroTime = macro;
                    _fiberPhotometryLoadedElapsed_s = elapsed;
                    _fiberPhotometryLoadedMeanLifetime_ns = meanTau;
                    _fiberPhotometryLoadedSumIntensity = sumI;
                    _fiberPhotometryLoadedDecay = decay;

                    // Populate lifetime curve plot from the first row
                    if (decay.Count > 0)
                    {
                        int ch = currentChannel;
                        if (ch < 0) ch = 0;
                        if (FLIM_ImgData?.RoiFit?.flim_parameters != null)
                        {
                            var fp = FLIM_ImgData.RoiFit.flim_parameters;
                            if (fp.LifetimeX == null || fp.LifetimeX.Length <= ch)
                            {
                                fp.LifetimeX = new double[Math.Max(ch + 1, 1)][];
                                fp.LifetimeY = new double[Math.Max(ch + 1, 1)][];
                            }
                            fp.LifetimeX[ch] = micro_ps;
                            fp.LifetimeY[ch] = decay[0].Select(v => (double)v).ToArray();
                        }
                        LifetimeCurvePlot.Invalidate();

                        // Show a quick timecourse in the realtime plot window (index-based)
                        if (plot_realtime != null)
                        {
                            plot_realtime.Show();
                            plot_realtime.plotNow_realtime(_fiberPhotometryLoadedMeanLifetime_ns);
                        }
                    }

                    return FileIO.FileError.Success;
                }
            }
            catch
            {
                return FileIO.FileError.UnKnown;
            }
        }

        private bool IsFiberPhotometryFile()
        {
            try
            {
                if (FLIM_ImgData?.State?.Init?.MicroscopeSystem != null &&
                    FLIM_ImgData.State.Init.MicroscopeSystem.ToLower().Contains("fiber"))
                    return true;

                // Fallback: 1x1 FLIM with multiple pages is very likely a fiber photometry recording.
                if (FLIM_ImgData != null && FLIM_ImgData.width == 1)
                    return true;
            }
            catch { }
            return false;
        }

        private void UpdateFiberPhotometryExportMenu()
        {
            try
            {
                if (exportFiberPhotometryCsvToolStripMenuItem == null)
                    return;

                bool on = IsFiberPhotometryFile();
                exportFiberPhotometryCsvToolStripMenuItem.Visible = on;
                exportFiberPhotometryCsvToolStripMenuItem.Enabled = on;
            }
            catch { }
        }

        private void ExportFiberPhotometryCsvFromFlim(string flimPath, string csvPath, int channel)
        {
            // Export current channel only (keeps format consistent even if channels have different n_time).
            int nPages = FileIO.SetupFLIMOpening(flimPath, out _);
            if (nPages <= 0)
                throw new IOException("No pages found in FLIM file.");

            var tmp = new FLIMData(FLIM_ImgData?.State ?? new ScanParameters());
            tmp.KeepPagesInMemory = false;
            tmp.fullFileName = flimPath;

            // Load first page to initialize State + dimensions.
            var err0 = FileIO.OpenFLIMTiffFilePage(flimPath, 0, 0, tmp, true, false);
            if (err0 != FileIO.FileError.Success)
                throw new IOException("Could not read FLIM page 1.");

            if (tmp.FLIMRaw == null || channel < 0 || channel >= tmp.FLIMRaw.Length || tmp.FLIMRaw[channel] == null)
                throw new IOException("Channel data not found.");

            int nT = tmp.FLIMRaw[channel].GetLength(2);
            if (nT <= 0)
                throw new IOException("Invalid microtime bins.");

            double res_ps = 250.0;
            try { res_ps = tmp.State.Spc.spcData.resolution[channel]; } catch { }
            if (res_ps <= 0) res_ps = 250.0;

            var micro_ps = new double[nT];
            for (int t = 0; t < nT; t++)
                micro_ps[t] = t * res_ps;

            DateTime t0 = tmp.acquiredTime;

            using (var sw = new StreamWriter(csvPath, false, Encoding.UTF8))
            {
                // Header
                sw.Write("MacroTimeISO,Elapsed_s,Channel,MeanIntensity,SumIntensity,MeanLifetime_ns");
                for (int t = 0; t < nT; t++)
                {
                    sw.Write(",");
                    sw.Write("ps_");
                    sw.Write(micro_ps[t].ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
                }
                sw.WriteLine();

                // Rows
                double line_ms = 0.0;
                try { line_ms = tmp.State.Acq.msPerLineActual(); } catch { }
                if (line_ms <= 0.0)
                    line_ms = 20.0;

                for (int page = 0; page < nPages; page++)
                {
                    var err = FileIO.OpenFLIMTiffFilePage(flimPath, page, 0, tmp, page == 0, false);
                    if (err != FileIO.FileError.Success)
                        break;

                    var raw = tmp.FLIMRaw[channel];
                    int ntThis = raw.GetLength(2);
                    int nt = Math.Min(nT, ntThis);
                    int h = raw.GetLength(0);
                    int w = raw.GetLength(1);

                    for (int y = 0; y < h; y++)
                    {
                        double sum = 0.0;
                        double wsum_ps = 0.0;
                        var decay = new ushort[nT];

                        for (int x = 0; x < w; x++)
                        {
                            for (int t = 0; t < nt; t++)
                            {
                                ushort v = raw[y, x, t];
                                decay[t] = (ushort)(decay[t] + v);
                                sum += v;
                                wsum_ps += v * micro_ps[t];
                            }
                        }

                        double sumIntensity = sum;
                        double meanIntensity = (w > 0) ? (sum / w) : sum;
                        double meanLifetime_ns = (sum > 0) ? (wsum_ps / sum / 1000.0) : 0.0;

                        DateTime ts = tmp.acquiredTime.AddMilliseconds(y * line_ms);
                        double elapsed_s = (ts - t0).TotalSeconds;

                        sw.Write(ts.ToString("yyyy-MM-ddTHH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture));
                        sw.Write(",");
                        sw.Write(elapsed_s.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture));
                        sw.Write(",");
                        sw.Write((channel + 1).ToString(System.Globalization.CultureInfo.InvariantCulture));
                        sw.Write(",");
                        sw.Write(meanIntensity.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture));
                        sw.Write(",");
                        sw.Write(sumIntensity.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture));
                        sw.Write(",");
                        sw.Write(meanLifetime_ns.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture));

                        for (int t = 0; t < nT; t++)
                        {
                            sw.Write(",");
                            ushort v = (t < ntThis) ? decay[t] : (ushort)0;
                            sw.Write(v.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        }

                        sw.WriteLine();
                    }
                }
            }
        }


        ///////////////////////////////////////
        ///////////////////////////////////////
        ///////////////////////////////////////
        //ROI functions from here.

        private void InitLineScanTraceContextMenus()
        {
            try
            {
                // Right-click on an ROI that is already in MultiROI list
                if (rightClickMenuStrip_inROI != null && _useThisTraceForLineScanningMenuItem_inROI == null)
                {
                    _useThisTraceForLineScanningMenuItem_inROI = new ToolStripMenuItem
                    {
                        Name = "useThisTraceForLineScanningToolStripMenuItem_inROI",
                        Text = "Use this trace for line scanning",
                    };
                    _useThisTraceForLineScanningMenuItem_inROI.Click += UseThisTraceForLineScanningMenuItem_Click;

                    // Insert near the top for visibility.
                    rightClickMenuStrip_inROI.Items.Insert(0, _useThisTraceForLineScanningMenuItem_inROI);
                    _useThisTraceForLineScanningSeparator_inROI = new ToolStripSeparator
                    {
                        Name = "useThisTraceForLineScanningSeparator_inROI",
                    };
                    rightClickMenuStrip_inROI.Items.Insert(1, _useThisTraceForLineScanningSeparator_inROI);
                }

                // Right-click on the currently selected ROI (not yet added to MultiROI list)
                if (rightClickMenuStrip != null && _useThisTraceForLineScanningMenuItem_selectedROI == null)
                {
                    _useThisTraceForLineScanningMenuItem_selectedROI = new ToolStripMenuItem
                    {
                        Name = "useThisTraceForLineScanningToolStripMenuItem_selectedROI",
                        Text = "Use this trace for line scanning",
                    };
                    _useThisTraceForLineScanningMenuItem_selectedROI.Click += UseThisTraceForLineScanningMenuItem_Click;

                    rightClickMenuStrip.Items.Insert(0, _useThisTraceForLineScanningMenuItem_selectedROI);
                    _useThisTraceForLineScanningSeparator_selectedROI = new ToolStripSeparator
                    {
                        Name = "useThisTraceForLineScanningSeparator_selectedROI",
                    };
                    rightClickMenuStrip.Items.Insert(1, _useThisTraceForLineScanningSeparator_selectedROI);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitLineScanTraceContextMenus failed: " + ex.Message);
            }
        }

        private static bool IsLineScanTraceRoiType(ROI roi)
        {
            if (roi == null)
                return false;
            return roi.ROI_type == ROI.ROItype.Polygon ||
                roi.ROI_type == ROI.ROItype.Rectangle ||
                roi.ROI_type == ROI.ROItype.Elipsoid;
        }

        private void UpdateLineScanTraceContextMenuItems()
        {
            // Called just before showing the context menu so the item is shown only for traceable ROIs and reflects current selection.
            ROI roiInList = null;
            if (FLIM_ImgData != null && FLIM_ImgData.ROIs != null &&
                FLIM_ImgData.currentRoi >= 0 && FLIM_ImgData.currentRoi < FLIM_ImgData.ROIs.Count)
            {
                roiInList = FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi];
            }

            bool clickedOnLineScanTrace = FLIM_ImgData != null && FLIM_ImgData.currentRoi == LineScanTraceCurrentRoiId && FLIM_ImgData.LineScanTraceRoi != null;
            bool inListIsTraceable = IsLineScanTraceRoiType(roiInList);
            bool traceIsTraceable = IsLineScanTraceRoiType(FLIM_ImgData?.LineScanTraceRoi);
            bool showInRoiMenu = inListIsTraceable || (clickedOnLineScanTrace && traceIsTraceable);
            if (_useThisTraceForLineScanningMenuItem_inROI != null)
            {
                _useThisTraceForLineScanningMenuItem_inROI.Visible = showInRoiMenu;
                _useThisTraceForLineScanningMenuItem_inROI.Checked = false;
                _useThisTraceForLineScanningMenuItem_inROI.Text = clickedOnLineScanTrace
                    ? "Remove this line scan trace"
                    : "Use this trace for line scanning";
            }
            if (_useThisTraceForLineScanningSeparator_inROI != null)
                _useThisTraceForLineScanningSeparator_inROI.Visible = showInRoiMenu && !clickedOnLineScanTrace;

            // For selected (not-yet-added) ROI, we use FLIM_ImgData.Roi as the source.
            ROI selected = FLIM_ImgData?.Roi;
            bool selectedIsTraceable = IsLineScanTraceRoiType(selected);
            if (_useThisTraceForLineScanningMenuItem_selectedROI != null)
            {
                _useThisTraceForLineScanningMenuItem_selectedROI.Visible = selectedIsTraceable;
                _useThisTraceForLineScanningMenuItem_selectedROI.Checked = false;
                _useThisTraceForLineScanningMenuItem_selectedROI.Text = "Use this trace for line scanning";
            }
            if (_useThisTraceForLineScanningSeparator_selectedROI != null)
                _useThisTraceForLineScanningSeparator_selectedROI.Visible = selectedIsTraceable;

            // If right-clicking the line-scan-trace ROI, hide unrelated ROI actions to avoid confusion.
            bool hideOtherInRoiMenuItems = clickedOnLineScanTrace;
            try
            {
                if (convertToRectangularROIToolStripMenuItem != null) convertToRectangularROIToolStripMenuItem.Visible = !hideOtherInRoiMenuItems;
                if (convertToElipsoidROIToolStripMenuItem != null) convertToElipsoidROIToolStripMenuItem.Visible = !hideOtherInRoiMenuItems;
                if (convertToPolygonROIToolStripMenuItem != null) convertToPolygonROIToolStripMenuItem.Visible = !hideOtherInRoiMenuItems;
                if (convertToPolyLineROIToolStripMenuItem != null) convertToPolyLineROIToolStripMenuItem.Visible = !hideOtherInRoiMenuItems;
                if (setRadiusOfThisROIToolStripMenuItem != null) setRadiusOfThisROIToolStripMenuItem.Visible = !hideOtherInRoiMenuItems;
                if (rmoeveRoiToolStripMenuItem != null) rmoeveRoiToolStripMenuItem.Visible = !hideOtherInRoiMenuItems;
                if (removeAllToolStripMenuItem1 != null) removeAllToolStripMenuItem1.Visible = !hideOtherInRoiMenuItems;
            }
            catch { }
        }

        private void UseThisTraceForLineScanningMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (FLIM_ImgData == null)
                    return;

                // Toggle OFF when right-clicking the existing line-scan trace ROI.
                if (FLIM_ImgData.currentRoi == LineScanTraceCurrentRoiId && FLIM_ImgData.LineScanTraceRoi != null)
                {
                    FLIM_ImgData.LineScanTraceRoi = null;
                    ClearLineScanTraceState();
                    if (FLIM_ImgData.currentRoi == LineScanTraceCurrentRoiId)
                        FLIM_ImgData.currentRoi = -1;

                    imageRoi = new ROI();
                    roi_State = drawROI_State.NoROI;
                    DrawImages();
                    return;
                }

                // Prefer the ROI under cursor in the MultiROI list when available; otherwise use the currently selected ROI.
                bool hasMultiRoiSelection = (FLIM_ImgData.currentRoi >= 0 && FLIM_ImgData.currentRoi < FLIM_ImgData.ROIs.Count);
                ROI roi = hasMultiRoiSelection ? FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi] : FLIM_ImgData.Roi;

                if (!IsLineScanTraceRoiType(roi))
                {
                    MessageBox.Show(Form.ActiveForm, "Line scan trace requires a polygon/rectangular/ellipsoid ROI.");
                    return;
                }

                // Create/update separate line-scan-trace ROI (NOT in ROIs list, so it won't be numbered / plotted).
                var trace = new ROI(roi);
                trace.IsLineScanTrace = true;
                trace.ID = LineScanTraceCurrentRoiId;
                FLIM_ImgData.LineScanTraceRoi = trace;

                // If it came from ROIs list, remove it from that list to avoid numbering/timecourse confusion.
                if (hasMultiRoiSelection)
                {
                    int idx = FLIM_ImgData.currentRoi;
                    if (idx >= 0 && idx < FLIM_ImgData.ROIs.Count)
                        FLIM_ImgData.ROIs.RemoveAt(idx);
                }

                if (!TrySetLineScanTraceStateFromROI(trace, out string reason))
                {
                    FLIM_ImgData.LineScanTraceRoi = null;
                    ClearLineScanTraceState();
                    MessageBox.Show(Form.ActiveForm, reason);
                    return;
                }

                // Select it in the display for editing.
                FLIM_ImgData.currentRoi = LineScanTraceCurrentRoiId;
                convertROIFromDataToDisplay(trace, ref imageRoi);
                imageRoi.IsLineScanTrace = true;
                roi_State = drawROI_State.Idle;

                SaveAllRois();
                DrawImages();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Form.ActiveForm, "Failed to set line scan trace.\n" + ex.Message);
            }
        }

        private void ClearLineScanTraceState()
        {
            try
            {
                if (FLIM_ImgData != null)
                {
                    FLIM_ImgData.LineScanTraceRoi = null;
                    if (FLIM_ImgData.currentRoi == LineScanTraceCurrentRoiId)
                        FLIM_ImgData.currentRoi = -1;
                }

                if (flimage?.State?.Acq != null)
                {
                    flimage.State.Acq.LineScanArrayX = null;
                    flimage.State.Acq.LineScanArrayY = null;
                }

                if (FLIM_ImgData?.State?.Acq != null)
                {
                    FLIM_ImgData.State.Acq.LineScanArrayX = null;
                    FLIM_ImgData.State.Acq.LineScanArrayY = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ClearLineScanTraceState failed: " + ex.Message);
            }
        }

        private bool TrySetLineScanTraceStateFromROI(ROI roi, out string reason)
        {
            reason = "";
            if (!IsLineScanTraceRoiType(roi))
            {
                reason = "Line scan trace requires a polygon/rectangular/ellipsoid ROI.";
                return false;
            }

            var xs = new List<double>();
            var ys = new List<double>();

            if (roi.ROI_type == ROI.ROItype.Elipsoid)
            {
                if (roi.Rect.Width <= 0 || roi.Rect.Height <= 0)
                {
                    reason = "Selected ellipsoid ROI has invalid size.";
                    return false;
                }

                int nPoints = 64;
                double cx = roi.Rect.Left + roi.Rect.Width / 2.0;
                double cy = roi.Rect.Top + roi.Rect.Height / 2.0;
                double a = roi.Rect.Width / 2.0;
                double b = roi.Rect.Height / 2.0;
                for (int i = 0; i < nPoints; i++)
                {
                    double theta = 2.0 * Math.PI * i / nPoints;
                    xs.Add(cx + a * Math.Cos(theta));
                    ys.Add(cy + b * Math.Sin(theta));
                }
            }
            else
            {
                if (roi.X == null || roi.Y == null || roi.X.Length < 3 || roi.Y.Length < 3)
                {
                    reason = "Selected ROI has too few vertices (need 3+).";
                    return false;
                }

                for (int i = 0; i < roi.X.Length && i < roi.Y.Length; i++)
                {
                    xs.Add(roi.X[i]);
                    ys.Add(roi.Y[i]);
                }
            }

            if (xs.Count > 1)
            {
                double dx0 = xs[0] - xs[xs.Count - 1];
                double dy0 = ys[0] - ys[ys.Count - 1];
                if (Math.Sqrt(dx0 * dx0 + dy0 * dy0) < 1e-3)
                {
                    xs.RemoveAt(xs.Count - 1);
                    ys.RemoveAt(ys.Count - 1);
                }
            }

            for (int i = xs.Count - 1; i >= 1; i--)
            {
                double dx = xs[i] - xs[i - 1];
                double dy = ys[i] - ys[i - 1];
                if (Math.Sqrt(dx * dx + dy * dy) < 1e-3)
                {
                    xs.RemoveAt(i);
                    ys.RemoveAt(i);
                }
            }

            if (xs.Count < 3)
            {
                reason = "Line scan trace has too few unique vertices (need 3+).";
                return false;
            }

            // Store in ScanParameters so it is written into the .flim header.
            if (flimage?.State?.Acq != null)
            {
                flimage.State.Acq.LineScanArrayX = xs.ToArray();
                flimage.State.Acq.LineScanArrayY = ys.ToArray();
            }
            if (FLIM_ImgData?.State?.Acq != null)
            {
                FLIM_ImgData.State.Acq.LineScanArrayX = xs.ToArray();
                FLIM_ImgData.State.Acq.LineScanArrayY = ys.ToArray();
            }

            return true;
        }

        private void RemoveAllRoisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveAllROIs(sender, e);
        }

        private void RemoveAllROIs(object sender, EventArgs e)
        {
            // If we are clearing all ROIs, also clear any stored line-scan-trace selection.
            ClearLineScanTraceState();

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
            distroi.IsLineScanTrace = sourceroi.IsLineScanTrace;
            //distroi.polyLineROIs = new List<ROI>(distroi.polyLineROIs); //ShallowCopy.
            if (sourceroi.ROI_type == ROI.ROItype.PolyLine)
            {
                distroi.GetSmoothCurve();
                distroi.GetEqualDistanceCenters(polyLineRadius * image_scale);
            }
            distroi.InvalidatePixelCache();
        }

        // KENGO BEGIN 1-12-2026
        // For copying PolyLine ROIs without scaling instead of copyROIVectors()
        public void copyROIVectorsNotScaling(ROI sourceroi, ref ROI distroi)
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
            distroi.IsLineScanTrace = sourceroi.IsLineScanTrace;
            //distroi.polyLineROIs = new List<ROI>(distroi.polyLineROIs); //ShallowCopy.
            if (sourceroi.ROI_type == ROI.ROItype.PolyLine)
            {
                distroi.GetSmoothCurve();
                distroi.GetEqualDistanceCenters(polyLineRadius);
            }
            distroi.InvalidatePixelCache();
        }
        // KENGO END

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
                // KENGO BEGIN 1-12-2026
                // For PolyLine ROIs, do not scale when copying.
                else if (newroi.ROI_type == ROI.ROItype.PolyLine)
                    copyROIVectorsNotScaling(newroi, ref roi_dist);
                // KENGO END
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
            Pnew.X = Pnew.X / image_scale + offset[0];
            Pnew.Y = Pnew.Y / image_scale + offset[1];

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
            // If the removed ROI was the line-scan trace, clear the stored selection in State.
            try
            {
                if (FLIM_ImgData != null && FLIM_ImgData.currentRoi == LineScanTraceCurrentRoiId)
                {
                    ClearLineScanTraceState();
                }
            }
            catch { }

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
                if (drag_ROI_whichCorner < X1.Length)
                {
                    X1[drag_ROI_whichCorner] = P1.X;
                    Y1[drag_ROI_whichCorner] = P1.Y;
                }

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

            if (IsLineScanTimeAxisMode())
            {
                bool editingLineScanTrace = imageRoi != null && imageRoi.IsLineScanTrace;
                if (!editingLineScanTrace && (FLIM_ImgData == null || FLIM_ImgData.currentRoi != LineScanTraceCurrentRoiId))
                {
                    roiTop = 0;
                    roiHeight = Height;
                }
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

        private bool IsLineScanTimeAxisMode()
        {
            if (focusing)
                return false;

            var acq = FLIM_ImgData?.State?.Acq;
            if (acq == null)
                return false;

            if (acq.isLineScanAcquisition)
                return true;

            bool isFiber = acq.fiberPhotometryMode
                || ((FLIM_ImgData?.State?.Init?.MicroscopeSystem ?? "")
                    .IndexOf("fiber", StringComparison.OrdinalIgnoreCase) >= 0);
            if (!isFiber && FLIM_ImgData != null && FLIM_ImgData.width == 1)
                isFiber = true;
            if (isFiber)
                return acq.linesPerFrame > 1;

            return acq.LineScanArrayX != null &&
                acq.LineScanArrayY != null &&
                acq.LineScanTimePoints > 0 &&
                acq.linesPerFrame > 1;
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

            FLIM_ImgData.currentRoi = -1; //Double click.
        }

        private void Image1_MouseDown(object sender, MouseEventArgs e)
        {
            if (drawUncagingPos == UncagingCursor.Idle)
            {
                if (e.Button == MouseButtons.Left)
                {
                    bool found = false;

                    if (uncaging_calib_multi_mode)
                    {
                        for (int i = 0; i < uncagingLocs_calib.Count; i++)
                        {
                            if (WithInMark(e.Location, uncagingLocs_calib[i]))
                            {
                                currentUncaging = i;
                                drawUncagingPos = UncagingCursor.Moving; //moveUncaging = true;
                                found = true;
                                break;
                            }
                        }
                    }
                    else
                    {
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
            bool lineScanTimeAxisMode = IsLineScanTimeAxisMode();
            if (lineScanTimeAxisMode && ROItype != ROI.ROItype.Rectangle)
                ROItype = ROI.ROItype.Rectangle;
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
                bool inside = false;
                bool corner = false;


                float min_dist = float.MaxValue;
                int current_roi = -1;

                // Line-scan trace ROI (separate from ROIs list)
                if (FLIM_ImgData.LineScanTraceRoi != null && IsLineScanTraceRoiType(FLIM_ImgData.LineScanTraceRoi))
                {
                    ROI roiTrace = FLIM_ImgData.LineScanTraceRoi;
                    inside = roiTrace.IsInsideRoi(p1);
                    corner = roiTrace.isNearCornerOfRoi(p1, handleSize, out drag_ROI_whichCorner, out dist);
                    if (FLIM_ImgData.currentRoi != LineScanTraceCurrentRoiId)
                        corner = false;

                    if (inside || corner)
                    {
                        foundRoi = true;
                        if (min_dist > dist)
                        {
                            min_dist = dist;
                            current_roi = LineScanTraceCurrentRoiId;
                            inside_current = inside;
                        }
                    }
                }

                if (nROI > 0)
                {
                    for (int i = 0; i < nROI; i++)
                    {
                        ROI roi1 = FLIM_ImgData.ROIs[i];
                        if (lineScanTimeAxisMode && roi1.ROI_type != ROI.ROItype.Rectangle)
                            continue;
                        inside = roi1.IsInsideRoi(p1);
                        corner = roi1.isNearCornerOfRoi(p1, handleSize, out drag_ROI_whichCorner, out dist);

                        if (FLIM_ImgData.currentRoi != i)
                            corner = false;

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
                }

                if (foundRoi)
                {
                    FLIM_ImgData.currentRoi = current_roi;

                    if (current_roi == LineScanTraceCurrentRoiId && FLIM_ImgData.LineScanTraceRoi != null)
                    {
                        CurrentROIBeforeMove = new ROI(FLIM_ImgData.LineScanTraceRoi);
                        convertROIFromDataToDisplay(FLIM_ImgData.LineScanTraceRoi, ref imageRoi);
                        imageRoi.IsLineScanTrace = true;
                    }
                    else if (current_roi >= 0 && current_roi < FLIM_ImgData.ROIs.Count)
                    {
                        //FLIM_ImgData.Roi = FLIM_ImgData.ROIs[current_roi];
                        copyROIVectors(FLIM_ImgData.ROIs[current_roi], ref FLIM_ImgData.Roi); // not work??
                        CurrentROIBeforeMove = new ROI(FLIM_ImgData.ROIs[current_roi]);
                        convertROIFromDataToDisplay(FLIM_ImgData.ROIs[current_roi], ref imageRoi); //roi1
                    }
                }


                if (!foundRoi)
                {
                    if (FLIM_ImgData.bgRoi != null)
                    {

                        inside = FLIM_ImgData.bgRoi.IsInsideRoi(p1);

                        if (FLIM_ImgData.bgRoi.Rect.Width < FLIM_ImgData.width && FLIM_ImgData.bgRoi.Rect.Height < FLIM_ImgData.height)
                        {
                            if (FLIM_ImgData.currentRoi == -2)
                                corner = FLIM_ImgData.bgRoi.isNearCornerOfRoi(p1, handleSize, out drag_ROI_whichCorner, out dist);
                        }

                        if (inside || corner)
                        {
                            FLIM_ImgData.currentRoi = -2;
                            //FLIM_ImgData.Roi = FLIM_ImgData.bgRoi;
                            copyROIVectors(FLIM_ImgData.bgRoi, ref FLIM_ImgData.Roi);
                            CurrentROIBeforeMove = FLIM_ImgData.bgRoi.CopyROI(-2);
                            convertROIFromDataToDisplay(FLIM_ImgData.bgRoi, ref imageRoi);
                            foundRoi = true;
                        }
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
                            UpdateLineScanTraceContextMenuItems();
                            rightClickMenuStrip.Show((PictureBox)sender, e.Location);//places the menu at the pointer position
                        }
                        else
                        {
                            setRadiusOfThisROIToolStripMenuItem.Visible = ROItype == ROI.ROItype.Elipsoid;
                            UpdateLineScanTraceContextMenuItems();
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

                    // KENGO BEGIN 1-8-2026
                    // set the size of handleSizeImage to a fixed value legardless of image scale 
                    //var handleSizeImage = handleSize * image_scale;
                    var handleSizeImage = handleSize * 2;
                    // KENGO END
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

                        // If LS ROI is currently selected, deselect it when starting to create a new ROI.
                        // Otherwise the new ROI inherits the "selected ROI" drawing path and LS disappears temporarily.
                        if (FLIM_ImgData.currentRoi == LineScanTraceCurrentRoiId)
                            FLIM_ImgData.currentRoi = -1;
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

                // Keep line-scan-trace ROI flagged while dragging so it stays orange.
                if (FLIM_ImgData.currentRoi == LineScanTraceCurrentRoiId && imageRoi != null)
                {
                    imageRoi.IsLineScanTrace = true;
                    imageRoi.ID = LineScanTraceCurrentRoiId;
                }

                DrawImages();
            }

            if (drawUncagingPos == UncagingCursor.Moving || drawUncagingPos == UncagingCursor.Creating)
            {
                if (drawUncagingPos == UncagingCursor.Moving && currentUncaging >= 0)
                {
                    if (uncaging_calib_multi_mode)
                    {
                        if (currentUncaging < uncagingLocs_calib.Count)
                            uncagingLocs_calib[currentUncaging] = ConvertPointFromDisplayToReal(e.Location);
                    }
                    else
                    {
                        if (currentUncaging < uncagingLocs.Count)
                            uncagingLocs[currentUncaging] = ConvertPointFromDisplayToReal(e.Location);
                    }
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
                            //Kengo BIGEN 11-30/2023
                            //Bug fix for roi moving by right click
                            //roi moves only by left mouse button
                            if (e.Button == MouseButtons.Left)
                            {
                                imageRoi.shiftRoi(new PointF(roi0.Rect.Left - imageRoi.Rect.Left, roi0.Rect.Top - imageRoi.Rect.Top));
                                imageRoi = new ROI(imageRoi.ROI_type, imageRoi.X, imageRoi.Y, FLIM_ImgData.nChannels, polyLineRadius * image_scale
                                    , FLIM_ImgData.currentRoi, imageRoi.Roi3d, (int[])imageRoi.Z.Clone());
                                    }
                            //Kengo END
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

                        // If we are editing the line-scan-trace ROI, keep it flagged so it stays orange after mouse-up.
                        if (FLIM_ImgData.currentRoi == LineScanTraceCurrentRoiId && imageRoi != null)
                        {
                            imageRoi.IsLineScanTrace = true;
                            imageRoi.ID = LineScanTraceCurrentRoiId;
                        }

                        moved = IfROIMoved(imageRoi, imageRoiSave);


                        if (moved)
                        {
                            // Commit ROI movement back to the correct storage location.
                            if (FLIM_ImgData.currentRoi == LineScanTraceCurrentRoiId)
                            {
                                // Line-scan trace ROI (separate from ROIs list; not used for analysis)
                                var trace = FLIM_ImgData.LineScanTraceRoi ?? new ROI();
                                convertROIfromDisplayToData(imageRoi, ref trace);
                                trace.IsLineScanTrace = true;
                                trace.ID = LineScanTraceCurrentRoiId;
                                FLIM_ImgData.LineScanTraceRoi = trace;

                                // Update header-stored vertices (minimal polygon coords)
                                if (!TrySetLineScanTraceStateFromROI(trace, out _))
                                    ClearLineScanTraceState();
                            }
                            else
                            {
                                // Selected ROI / normal analysis ROIs
                                convertROIfromDisplayToData(imageRoi, ref FLIM_ImgData.Roi);
                            }

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

                            if (preMouseUp_State == drawROI_State.Moving_ExistingROI)
                            {
                                // Shift+move applies only to MultiROIs (moves all other ROIs together).
                                if (FLIM_ImgData.currentRoi >= 0 && Control.ModifierKeys == Keys.Shift)
                                {
                                    float shiftX = -CurrentROIBeforeMove.Rect.Location.X + FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].Rect.Location.X;
                                    float shiftY = -CurrentROIBeforeMove.Rect.Location.Y + FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].Rect.Location.Y;
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

                        fittingDone = false;
                        FLIM_ImgData.ResetLifetimeCalculation(true); //This will reset the lifetime calculation in all pages.

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
                        fittingDone = false;
                        FLIM_ImgData.ResetLifetimeCalculation(true); //This will reset the lifetime calculation in all pages.
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

                        if (uncaging_calib_multi_mode)
                        {
                            if (currentUncaging >= 0 && currentUncaging < uncagingLocs_calib.Count)
                                uncagingLocs_calib[currentUncaging] = (double[])uncagingLocFrac.Clone();

                        }
                        else
                        {
                            if (currentUncaging >= 0 && currentUncaging < uncagingLocs.Count)
                                uncagingLocs[currentUncaging] = (double[])uncagingLocFrac.Clone();
                        }


                        drawUncagingPos = UncagingCursor.Idle;
                    }
                }

                if (e.Button == MouseButtons.Right)
                {
                    if (uncaging_calib_mode) //Right click to calibrate.
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
            if (!sender.Equals(Values_selectedROI))
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
                                    else if (mode == 3) //bgROI
                                    {
                                        if (roi.Rect.Left >= 0 && roi.Rect.Top >= 0 && roi.Rect.Right <= FLIM_ImgData.width && roi.Rect.Bottom <= FLIM_ImgData.height)
                                        {
                                            FLIM_ImgData.bgRoi = roi;
                                            FLIM_ImgData.bgRoi.ID = -2;
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

            if (IsFiberPhotometryFile())
                TCF.DeleteAll();

            String fn = Path.GetFileNameWithoutExtension(fileName);
            FormControllers.CloseOpenExcelWindow(fn);

            String[] lines = File.ReadAllLines(fileName);

            //Kengo BEGIN 12-5-2023
            //String TC_FileName = FLIM_ImgData.fileName.Split('.')[0];
            String TC_FileName = Path.GetFileNameWithoutExtension(FLIM_ImgData.fileName);
            //Kegno END
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


            if (IsFiberPhotometryFile() && FLIM_ImgData != null)
            {
                int fileNumber = FLIM_ImgData.fileCounter;
                if (fileNumber >= 0)
                    TC_List = TC_List.Where(tc => tc.FileNumber == fileNumber).ToList();
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

            bool saveSelectedRoi = (FLIM_ImgData.ROIs == null || FLIM_ImgData.ROIs.Count == 0) || TCF.nROI == 0;

            if (saveSelectedRoi)
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

            if (TCF.nROI > 0)
            {
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
                            //KENGO BEGIN 2-17-2026
                            //change the label format for PolyLineROI
                            //SaveLineAsExcel(arr1_ch_roi, fieldName2 + (TCF.UniqueIDs.ToList()[i] + 1) + "-ch" + (ch + 1), ref sb);
                            int roiID = TCF.UniqueIDs.ToList()[i];
                            String roiID_Str = (roiID + 1).ToString();
                            if (roiID > 1000)
                            {
                                int roi1_ID = roiID / 1000;
                                int roi2_ID = roiID - (1000 * roi1_ID);
                                roiID_Str = roi1_ID.ToString() + "_" + roi2_ID.ToString();
                            }
                            SaveLineAsExcel(arr1_ch_roi, fieldName2 + roiID_Str + "-ch" + (ch + 1), ref sb);
                            //KENGO END
                        }
                        sb.AppendLine();
                    }
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
            if (FLIM_ImgData.bgRoi != null)
                sb.Append(FormatROIPositionToExcel(FLIM_ImgData.bgRoi));

            sb.Append("MultiROI");
            sb.AppendLine();
            for (int i = 0; i < FLIM_ImgData.ROIs.Count; i++)
                sb.Append(FormatROIPositionToExcel(FLIM_ImgData.ROIs[i]));

            string allStr = sb.ToString();
            CreateAanalysisFolder();
            String fileName = RoiFileName(false);
            File.WriteAllText(fileName, allStr);
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
            int p = GetFittingParameterCountForSaving();
            SyncPsPerUnitFromState(currentChannel);

            if (p == 5)
            {
                sb.Append("Sinlge exponential fitting");
                sb.AppendLine();
                sb.Append("Ch/ROI,Pop1,Tau1,TauG,Tau0,BG,MeanTau,Frac1,Frac2,xi_square");
            }
            else
            {
                sb.Append("Double exponential fitting");
                sb.AppendLine();
                sb.Append("Ch/ROI,Pop1,Tau1,Pop2,Tau2,TauG,Tau0,BG,Meantau,Frac1,Frac2,xi_square");
            }

            sb.AppendLine();
            double res = (double)FLIM_ImgData.psPerUnit / 1000.0;
            int nChs = FLIM_ImgData.nChannels;

            List<bool> FLIM_OK = new List<bool>();
            for (int ch = 0; ch < nChs; ch++)
                FLIM_OK.Add(IsFittingChannelSaveable(ch, p));

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
                        if (x == 1 || (p == 7 && x == 3))
                            val = res / val;
                        else if ((p == 5 && (x == 2 || x == 3)) || (p == 7 && (x == 4 || x == 5)))
                            val = val * res;

                        sb.Append(val);
                    }
                    sb.Append(",");
                    sb.Append(FLIM_ImgData.RoiFit.flim_parameters.tau_m[ch]); // * res);
                    sb.Append(",");
                    if (p == 7)
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
                            sb = ROI_Data_String(sb, roi2, ch, "PolyLine");
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
                                    //KENGO BEGIN 2-17-2026
                                    //change the label format for PolyLineROI
                                    //sb = ROI_Data_String(sb, roi2, ch, "PolyLineROI" + roi1 + "-");
                                    sb = ROI_Data_String(sb, roi2, ch, "ROI" + (roi1.ID + 1) + "_PolyLine");
                                    //KENGO END
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
            //KENGO BEGIN 2-18-2026
            //Lifetime Curve for each ROI
            if (FLIM_ImgData.Fit_type.Equals(FLIMData.FitType.GlobalRois))
            {
                for (int ch = 0; ch < nChs; ch++)
                {
                    if (FLIM_OK[ch])
                    {
                        int index = 0;
                        foreach (var roi1 in FLIM_ImgData.ROIs)
                        {
                            if (index == 0 && HasRoiLifetimeData(roi1, ch))
                            {
                                int chName = ch + 1;
                                sb.Append("Time-" + chName + ",");
                                for (int x = 0; x < roi1.flim_parameters.LifetimeX[ch].Length; x++)
                                {
                                    sb.Append(roi1.flim_parameters.LifetimeX[ch][x] * res);
                                    sb.Append(",");
                                }
                                sb.AppendLine();
                            }
                            sb = ROI_LifeTimeData_String(sb, roi1, ch, "");
                            if (roi1.ROI_type == ROI.ROItype.PolyLine)
                                foreach (var roi2 in roi1.polyLineROIs)
                                {
                                    sb = ROI_LifeTimeData_String(sb, roi2, ch, "ROI" + (roi1.ID + 1) + "_PolyLine");
                                }
                            index++;
                        }
                        sb.AppendLine();
                    }
                }
            }
            //KENGO END

            string allStr = sb.ToString();

            CreateAanalysisFolder();
            String fileName = FitFileName();
            File.WriteAllText(fileName, allStr);
        }

        private double GetStateResolutionPs(int channel)
        {
            var resolution = FLIM_ImgData?.State?.Spc?.spcData?.resolution;
            if (resolution != null)
            {
                if (channel >= 0 && channel < resolution.Length && resolution[channel] > 0)
                    return resolution[channel];

                if (resolution.Length > 0 && resolution[0] > 0)
                    return resolution[0];
            }

            if (FLIM_ImgData != null && FLIM_ImgData.psPerUnit > 0)
                return FLIM_ImgData.psPerUnit;

            return 250.0;
        }

        private void SyncPsPerUnitFromState(int channel)
        {
            double resolutionPs = GetStateResolutionPs(channel);

            if (FLIM_ImgData != null)
                FLIM_ImgData.psPerUnit = resolutionPs;

            if (psPerUnit == null)
                return;

            string text = String.Format("{0:0.000}", resolutionPs);
            if (psPerUnit.Text == text)
                return;

            updatingPsPerUnitText = true;
            try
            {
                psPerUnit.Text = text;
            }
            finally
            {
                updatingPsPerUnitText = false;
            }
        }

        private int GetFittingParameterCountForSaving()
        {
            if (FLIM_ImgData?.RoiFit?.flim_parameters?.n_exponentials == 1)
                return 5;

            return 7;
        }

        private bool IsFittingChannelSaveable(int ch, int parameterCount)
        {
            if (FLIM_ImgData == null || ch < 0 || ch >= FLIM_ImgData.nChannels)
                return false;

            if (FLIM_ImgData.State?.Acq?.acqFLIMA == null || ch >= FLIM_ImgData.State.Acq.acqFLIMA.Length || !FLIM_ImgData.State.Acq.acqFLIMA[ch])
                return false;

            if (FLIM_ImgData.n_time == null || ch >= FLIM_ImgData.n_time.Length || FLIM_ImgData.n_time[ch] <= 1)
                return false;

            if (FLIM_ImgData.FLIMRaw == null || ch >= FLIM_ImgData.FLIMRaw.Length || FLIM_ImgData.FLIMRaw[ch] == null || FLIM_ImgData.FLIMRaw[ch].GetLength(2) <= 1)
                return false;

            var parameters = FLIM_ImgData.RoiFit?.flim_parameters;
            if (parameters == null ||
                parameters.beta == null || ch >= parameters.beta.Length || parameters.beta[ch] == null || parameters.beta[ch].Length < parameterCount ||
                parameters.tau_m == null || ch >= parameters.tau_m.Length ||
                parameters.xi_square == null || ch >= parameters.xi_square.Length)
                return false;

            return true;
        }

        private bool HasRoiFitData(ROI roi, int ch, int parameterCount)
        {
            var parameters = roi?.flim_parameters;
            return parameters != null &&
                parameters.beta != null && ch < parameters.beta.Length && parameters.beta[ch] != null && parameters.beta[ch].Length >= parameterCount &&
                parameters.tau_m != null && ch < parameters.tau_m.Length &&
                parameters.xi_square != null && ch < parameters.xi_square.Length;
        }

        private bool HasRoiLifetimeData(ROI roi, int ch)
        {
            var parameters = roi?.flim_parameters;
            return parameters != null &&
                parameters.LifetimeX != null && ch < parameters.LifetimeX.Length && parameters.LifetimeX[ch] != null &&
                parameters.LifetimeY != null && ch < parameters.LifetimeY.Length && parameters.LifetimeY[ch] != null;
        }

        //KENGO BEGIN 2-18-2026
        //Lifetime curve for each ROI
        private StringBuilder ROI_LifeTimeData_String(StringBuilder sb, ROI roi1, int ch, string Note)
        {
            if (!HasRoiLifetimeData(roi1, ch))
                return sb;

            int roi = roi1.ID;
            int chName = ch + 1;
            if (Note == "")
                Note = "ROI" + (roi + 1) + "-";
            else
                Note += "ROI" + roi + "-";
            sb.Append(Note + "Lifetime-" + chName + ",");
            for (int x = 0; x < roi1.flim_parameters.LifetimeY[ch].Length; x++)
            {
                sb.Append(roi1.flim_parameters.LifetimeY[ch][x]);
                sb.Append(",");
            }
            sb.AppendLine();
            return sb;
        }
        //KENGO END

        private StringBuilder ROI_Data_String(StringBuilder sb, ROI roi1, int ch, string Note)
        {
            int p = GetFittingParameterCountForSaving();
            if (!HasRoiFitData(roi1, ch, p))
                return sb;

            int roi = roi1.ID;
            int rName = Note == "" ? roi + 1 : roi;
            int cName = ch + 1;
            double res = FLIM_ImgData.psPerUnit / 1000;
            sb.Append(Note + "ROI" + rName + "-ch" + cName);
            for (int x = 0; x < p; x++)
            {
                sb.Append(",");
                double val = roi1.flim_parameters.beta[ch][x];
                if (x == 1 || (p == 7 && x == 3))
                    val = res / val;
                else if ((p == 5 && (x == 2 || x == 3)) || (p == 7 && (x == 4 || x == 5)))
                    val = val * res;

                sb.Append(val);
            }
            sb.Append(",");
            sb.Append(roi1.flim_parameters.tau_m[ch]); // * res);
            sb.Append(",");
            if (p == 7)
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
                //Kengo BEGIN 12-5-2023
                //return Path.Combine(FLIM_ImgData.pathName, "Analysis", "ROI", FLIM_ImgData.fileName.Split('.')[0] + "_ROI_ImageJ.zip");
                return Path.Combine(FLIM_ImgData.pathName, "Analysis", "ROI", Path.GetFileNameWithoutExtension(FLIM_ImgData.fileName) + "_ROI_ImageJ.zip");
            //Kengo END
            else
                //Kengo BEGIN 12-5-2023
                //return Path.Combine(FLIM_ImgData.pathName, "Analysis", "ROI", FLIM_ImgData.fileName.Split('.')[0] + "_ROI.txt");
                return Path.Combine(FLIM_ImgData.pathName, "Analysis", "ROI", Path.GetFileNameWithoutExtension(FLIM_ImgData.fileName) + "_ROI.txt");
            //Kengo END
        }

        private String FitFileName()
        {
            //Kengo BEGIN 12-5-2023
            //return Path.Combine(FLIM_ImgData.pathName, "Analysis", "Fit", FLIM_ImgData.fileName.Split('.')[0] + "_Fit.csv");
            return Path.Combine(FLIM_ImgData.pathName, "Analysis", "Fit", Path.GetFileNameWithoutExtension(FLIM_ImgData.fileName) + "_Fit.csv");
            //Kengo END
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

        private double GetStoredBackgroundPerMillionPixel(int ch)
        {
            if (FLIM_ImgData?.RoiFit?.flim_parameters?.background_per_million_pixel != null &&
                FLIM_ImgData.RoiFit.flim_parameters.background_per_million_pixel.Length > ch &&
                FLIM_ImgData.RoiFit.flim_parameters.background_per_million_pixel[ch] != 0)
            {
                return FLIM_ImgData.RoiFit.flim_parameters.background_per_million_pixel[ch];
            }

            if (FLIM_ImgData?.bgRoi?.flim_parameters?.background_per_million_pixel != null &&
                FLIM_ImgData.bgRoi.flim_parameters.background_per_million_pixel.Length > ch)
            {
                return FLIM_ImgData.bgRoi.flim_parameters.background_per_million_pixel[ch];
            }

            if (FLIM_ImgData?.RoiFit?.flim_parameters?.background_per_million_pixel != null &&
                FLIM_ImgData.RoiFit.flim_parameters.background_per_million_pixel.Length > ch)
            {
                return FLIM_ImgData.RoiFit.flim_parameters.background_per_million_pixel[ch];
            }

            return 0;
        }

        private void UpdateBackgroundPerPixelsText(int ch, double backgroundPerMillion, bool force)
        {
            if (BG_perPixels == null)
                return;

            if (!force && BG_perPixels.Focused)
                return;

            if (Double.IsNaN(backgroundPerMillion) || Double.IsInfinity(backgroundPerMillion))
                backgroundPerMillion = 0;

            BG_perPixels.Text = String.Format("{0:0.###}", backgroundPerMillion);
        }

        private bool TryReadBackgroundPerMillionPixelFromGui(out double backgroundPerMillion)
        {
            backgroundPerMillion = 0;
            return BG_perPixels != null && Double.TryParse(BG_perPixels.Text, out backgroundPerMillion);
        }

        private void StoreBackgroundPerMillionPixelFromFit(int ch, double[] beta, double nPixels, bool updateText)
        {
            if (beta == null || nPixels <= 0)
                return;

            int bgIndex;
            if (beta.Length >= 7)
                bgIndex = 6;
            else if (beta.Length >= 5)
                bgIndex = 4;
            else
                return;

            double backgroundPerMillion = beta[bgIndex] * 1000000.0 / nPixels;
            if (Double.IsNaN(backgroundPerMillion) || Double.IsInfinity(backgroundPerMillion))
                backgroundPerMillion = 0;

            FLIM_ImgData.RoiFit.flim_parameters.AssureSizeAllParameters();
            FLIM_ImgData.RoiFit.flim_parameters.background_per_million_pixel[ch] = backgroundPerMillion;

            if (updateText && ch == GetSelectedFittingChannel())
                UpdateBackgroundPerPixelsText(ch, backgroundPerMillion, false);
        }

        private void UpdateBackgroundPerPixelFromBackgroundRoi(int page1)
        {
            if (FLIM_ImgData == null || !FLIM_ImgData.HasBackgroundRoi())
                return;

            FLIM_ImgData.calculateBackgroundLifetime(page1);
            FLIM_ImgData.bgRoi.flim_parameters.AssureSizeAllParameters();

            for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
            {
                if (!FLIM_ImgData.State.Acq.acqFLIMA[ch])
                    continue;

                var bgParams = FLIM_ImgData.bgRoi.flim_parameters;
                double[] x = bgParams.LifetimeX != null && bgParams.LifetimeX.Length > ch ? bgParams.LifetimeX[ch] : null;
                double[] y = bgParams.LifetimeY != null && bgParams.LifetimeY.Length > ch ? bgParams.LifetimeY[ch] : null;
                if (x == null || y == null || x.Length < 2 || y.Length < 2)
                {
                    bgParams.background_per_million_pixel[ch] = 0;
                    continue;
                }

                double[] beta1 = new double[] { 0 };
                FLIM_ImgData.fitData_ROI_Ch(FLIM_ImgData.bgRoi, ch, 1, beta1, page1);

                double nPixels = bgParams.nPixels != null && bgParams.nPixels.Length > ch ? bgParams.nPixels[ch] : 0;
                double[] beta = bgParams.beta != null && bgParams.beta.Length > ch ? bgParams.beta[ch] : null;

                if (beta != null && beta.Length > 4 && nPixels > 0)
                    bgParams.background_per_million_pixel[ch] = beta[4] * 1000000.0 / nPixels;
                else
                    bgParams.background_per_million_pixel[ch] = 0;

                if (ch == GetSelectedFittingChannel() && !cb_BGfix.Checked)
                    UpdateBackgroundPerPixelsText(ch, bgParams.background_per_million_pixel[ch], false);
            }
        }

        private bool TryGetFixedBackgroundBaseline(int ch, double nPixels, out double baseline, out double backgroundPerMillion)
        {
            baseline = 0;
            backgroundPerMillion = 0;

            if (nPixels <= 0 || FLIM_ImgData == null)
                return false;

            if (ch == GetSelectedFittingChannel() && TryReadBackgroundPerMillionPixelFromGui(out backgroundPerMillion))
            {
                if (FLIM_ImgData.HasBackgroundRoi())
                {
                    var bgParamsGui = FLIM_ImgData.bgRoi.flim_parameters;
                    bgParamsGui.AssureSizeAllParameters();
                    bgParamsGui.background_per_million_pixel[ch] = backgroundPerMillion;
                }

                baseline = backgroundPerMillion * nPixels / 1000000.0;
                return !(Double.IsNaN(baseline) || Double.IsInfinity(baseline));
            }

            if (!FLIM_ImgData.HasBackgroundRoi())
                return false;

            var bgParams = FLIM_ImgData.bgRoi.flim_parameters;
            if (bgParams.background_per_million_pixel == null || bgParams.background_per_million_pixel.Length <= ch)
                return false;

            backgroundPerMillion = bgParams.background_per_million_pixel[ch];
            if (Double.IsNaN(backgroundPerMillion) || Double.IsInfinity(backgroundPerMillion))
                return false;

            baseline = backgroundPerMillion * nPixels / 1000000.0;
            return !(Double.IsNaN(baseline) || Double.IsInfinity(baseline));
        }

        private void ApplyBackgroundBaselineSetting(int ch, double nPixels, int baselineIndex, bool fitWithBaseline, bool fixBackgroundPerPixel, double[] beta0, bool[] fix)
        {
            if (beta0 == null || fix == null || baselineIndex >= beta0.Length || baselineIndex >= fix.Length)
                return;

            beta0[baselineIndex] = 0;
            fix[baselineIndex] = !fitWithBaseline;

            if (!fixBackgroundPerPixel)
                return;

            if (TryGetFixedBackgroundBaseline(ch, nPixels, out double baseline, out double backgroundPerMillion))
            {
                beta0[baselineIndex] = baseline;
                FLIM_ImgData.RoiFit.flim_parameters.AssureSizeAllParameters();
                FLIM_ImgData.RoiFit.flim_parameters.background_per_million_pixel[ch] = backgroundPerMillion;
            }

            fix[baselineIndex] = true;
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

            FLIM_ImgData.RoiFit.flim_parameters.n_exponentials = mode;
            double[] x = FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[0];
            double[] y = FLIM_ImgData.RoiFit.flim_parameters.LifetimeY[0];
            SyncPsPerUnitFromState(currentChannel);

            page1 = -1;

            if (x == null || y == null || y.Length != x.Length)
                FLIM_ImgData.ResetLifetimeCalculation(false);

            //Calculate lifetime for general fitting.
            FLIM_ImgData.calculateLifetime(page1);

            //Background fitting.
            UpdateBackgroundPerPixelFromBackgroundRoi(page1);

            double[][] beta_g = new double[FLIM_ImgData.nChannels][];

            bool fit_with_baseline = Fit_BG.Checked;
            bool fix_background_per_pixel = fit_with_baseline && (cb_BGfix.Checked || fixAll);

            for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
            {
                beta_g[ch] = (double[])FLIM_ImgData.RoiFit.flim_parameters.beta0[ch].Clone();


                if (FLIM_ImgData.State.Acq.acqFLIMA[ch] && FLIM_ImgData.n_time[ch] > 1)
                {
                    SyncPsPerUnitFromState(ch);
                    int nPixels = (int)FLIM_ImgData.RoiFit.flim_parameters.nPixels[ch];

                    double[] beta0;
                    bool[] fix;
                    double res = FLIM_ImgData.psPerUnit / 1000;

                    double[] param = new double[N_FITPARAM];

                    if (fit_param[ch] != null)
                        param = (double[])fit_param[ch].Clone();

                    if (mode == 1)
                    {
                        int p = 5;
                        beta0 = new double[p];
                        beta0[0] = param[0];
                        beta0[1] = res / param[1];
                        beta0[2] = param[4] / res;
                        beta0[3] = param[5] / res;

                        fix = new bool[p];
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

                        ApplyBackgroundBaselineSetting(ch, nPixels, 4, fit_with_baseline, fix_background_per_pixel, beta0, fix);
                    }
                    else
                    {
                        int p = 7;
                        beta0 = new double[7];
                        beta0[0] = param[0];
                        beta0[1] = res / param[1];
                        beta0[2] = param[2];
                        beta0[3] = res / param[3];
                        beta0[4] = param[4] / res;
                        beta0[5] = param[5] / res;
                        beta0[6] = 0;

                        fix = new bool[p];

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

                        ApplyBackgroundBaselineSetting(ch, nPixels, 6, fit_with_baseline, fix_background_per_pixel, beta0, fix);

                    }
                    //betahat = Fitting.nlinfit_exGauss(beta0, fix, y, x, 1);
                    //FLIM_ImgData.fittingList[ch] = betahat;

                    var bet1 = FLIM_ImgData.fitData(ch, mode, beta0, fix, page1);
                    if (bet1 != null)
                    {
                        beta_g[ch] = bet1;
                        if (fit_with_baseline)
                            StoreBackgroundPerMillionPixelFromFit(ch, bet1, nPixels, fit_with_baseline && !cb_BGfix.Checked);
                    }

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
            offset_out = (double[])FLIM_ImgData.RoiFit.flim_parameters.offset_fit.Clone();

            if (saveEvent)
            {
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
                cb_BGfix.Checked = b;
            }
        }

        /// <summary>
        /// Referesh Image1 and Image2.
        /// </summary>
        void DrawImages()
        {
            if (IsFiberPhotometryMode())
                return;

            Image1.Invalidate(); //Referesh Image1.
            Image2.Invalidate(); //Referesh Image2.
        }

        public void DrawImages_public()
        {
            DrawImages();
        }

        /// <summary>
        /// Called when Iamge1 or Image2 is refreshed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_Paint(object sender, PaintEventArgs e)
        {
            if (IsFiberPhotometryMode())
                return;

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

                if (uncaging_calib_multi_mode && uncagingLocs_calib.Count > 0)
                {
                    for (int i = 0; i < uncagingLocs_calib.Count; i++)
                    {
                        var uString = (i + 1).ToString();
                        var sizF = e.Graphics.MeasureString(uString, roiFont);

                        Point uloc1 = (Point)ConvertFractionFromRealToDisplay(uncagingLocs_calib[i]);
                        drawUncaging(uloc1, RefCPenThick, e);
                        int WidthOfMark = (int)(Image1.Width * sizeOfMark);
                        RectangleF drawRect = new RectangleF(uloc1.X - WidthOfMark, uloc1.Y - WidthOfMark - 15, sizF.Width, sizF.Height);
                        e.Graphics.DrawString(uString, roiFont, roiBrush, drawRect);
                    }
                }

                if (uncagingLocs.Count > 0)
                {
                    for (int i = 0; i < uncagingLocs.Count; i++)
                    {
                        var uString = (i + 1).ToString();
                        var sizF = e.Graphics.MeasureString(uString, roiFont);
                        Point uloc1 = (Point)ConvertFractionFromRealToDisplay(uncagingLocs[i]);
                        drawUncaging(uloc1, multiRoiPen, e);
                        int WidthOfMark = (int)(Image1.Width * sizeOfMark);
                        RectangleF drawRect = new RectangleF(uloc1.X - WidthOfMark, uloc1.Y - WidthOfMark - 15, sizF.Width, sizF.Height);
                        e.Graphics.DrawString(uString, roiFont, roiBrush, drawRect);
                    }
                }

                if (uncaging_on && drawUncagingPos != UncagingCursor.Inactive)
                {
                    drawUncaging(uLoc, UncageCPenThick, e);
                    drawUncaging(referenceLoc, RefCPenThick, e);
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

                if (FLIM_ImgData.bgRoi != null && FLIM_ImgData.bgRoi.Rect.Width > 0 && FLIM_ImgData.bgRoi.Rect.Height > 0)
                {
                    ROI roi1 = FLIM_ImgData.bgRoi.CopyROI(-2);
                    if (roi_State == drawROI_State.Moving_ExistingROI && FLIM_ImgData.currentRoi == -2)
                        roi1.shiftRoi(shiftP);

                    drawROIsGeneric(e, BgRoiPen, BgRoiPenOutOfFocus, roi1, false, false, "bg");
                }

                // Line-scan trace ROI (separate from ROIs list, drawn in orange and not numbered)
                if (FLIM_ImgData.LineScanTraceRoi != null &&
                    FLIM_ImgData.LineScanTraceRoi.Points != null &&
                    FLIM_ImgData.LineScanTraceRoi.Points.Length > 2 &&
                    FLIM_ImgData.currentRoi != LineScanTraceCurrentRoiId)
                {
                    ROI roi1 = FLIM_ImgData.LineScanTraceRoi.CopyROI(LineScanTraceCurrentRoiId);
                    drawROIsGeneric(e, lineScanTracePen, lineScanTracePenOutOfFocus, roi1, false, false, "LS");
                }

                if (FLIM_ImgData.Roi != null && roi_State != drawROI_State.NoROI && roi_State != drawROI_State.Inactive)
                {
                    // If LS ROI is selected, draw it in red (same as selected ROI) but keep "LS" label.
                    // When unselected, LS ROI is drawn separately in orange.
                    Pen p = roiPen;
                    Pen pOut = roiPenOutOfFocus;
                    drawROIsGeneric(e, p, pOut, imageRoi, true, true, (imageRoi != null && imageRoi.IsLineScanTrace) ? "LS" : "");
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

        private void drawROIsGeneric(PaintEventArgs e, Pen p, Pen outOfFocusPen, ROI roi_original, bool drawHndle, bool displayRoi, string label_string)
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

            // KENGO BEGIN 1-8-2026
            // set the size of handleSizeImage to a fixed value legardless of image scale 
            //var handleSizeImage = handleSize * image_scale;
            var handleSizeImage = handleSize * 2;
            // KENGO END
            if (roi.Points != null)
            {
                var roistring = label_string.ToString();
                var sizF = e.Graphics.MeasureString(roistring, roiFont);
                //KENGO BEGIN 2-20-2026
                //Adjust the label position for Elipsoid ROI
                //var drawRect = new RectangleF(roi.X[0] - 15, roi.Y[0] - 15, sizF.Width, sizF.Height);
                PointF labelPos;
                if (roi.ROI_type.Equals(ROI.ROItype.Elipsoid))
                    labelPos = new PointF(roi.Rect.Left - roi.Rect.Height / roi.Rect.Width - 6f, roi.Rect.Top - roi.Rect.Width / roi.Rect.Height - 6f);
                else
                    labelPos = new PointF(roi.X[0] - 15f, roi.Y[0] - 15f);
                var drawRect = new RectangleF(labelPos.X, labelPos.Y, sizF.Width, sizF.Height);
                //KENGO END
                e.Graphics.DrawString(roistring, roiFont, roiBrush, drawRect);

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
                    //Kengo BEGIN 10-25-2024
                    //ROItype.Traced does not show handles as well as FreeHand
                    //if (!roi.ROI_type.Equals(ROI.ROItype.FreeHand))
                    if (!roi.ROI_type.Equals(ROI.ROItype.FreeHand) && !roi.ROI_type.Equals(ROI.ROItype.Traced))
                    //END
                    {
                        foreach (var point in roi.Points)
                            e.Graphics.FillEllipse(pen.Brush, point.X - handleSizeImage / 2, point.Y - handleSizeImage / 2, handleSizeImage, handleSizeImage);
                    }
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

            // KENGO BEGIN 1-8-2026
            // set the size of handleSizeImage to a fixed value legardless of image scale 
            //var handleSizeImage = handleSize * image_scale;
            var handleSizeImage = handleSize * 2;
            // KENGO END
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
            if (FLIM_ImgData.Project != null && FLIM_ImgData.Project[cl] != null)
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

            // Tetsuya BEGIN 06-29-2026
            // Use FOCUS frame count when computing FLIM low threshold
            // double def_val = (double)FLIM_ImgData.nAveragedFrame[cl] / 2.0;
            double def_val = GetEffectiveFrameCountForIntensityRange(cl) / 2.0;
            // Tetsuya END
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
                            if (FLIM_ImgData.KeepPagesInMemory)
                            {
                                FLIM_ImgData.currentPage = page;
                            }
                            else
                            {
                                GotoPage4D(page);
                            }

                            setupImageUpdateForZStack();
                            UpdateImages(true, realtime, focusing, true);
                        }

                        if (plot_regular.calc_upon_open && !ZStack && !FastZStack)
                            CalculateCurrentPage(false);
                        if (!ZStack && !FastZStack)
                            CalculatePhasorCurrentPageUponOpen();
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
                { }
                ;
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
                ChannelChecks[i].Checked = (channel - 1 == i);
                sdr = ChannelChecks[i];
            }

            if (channel == 12)
            {
                Channel12.Checked = true;
                sdr = Channel12;
            }

            //Kengo BEGIN 12-2-2023
            //This will be required to update
            Channel1_Clicked(sdr, null);
            //Kengo END
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
            SetFitting_Param(true);
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
            cb_BGfix.Checked = ON;
            fix_all.Checked = ON;
        }

        /// <summary>
        /// For external command to set Fitting Params.
        /// Input = {tau1, tau2, tauG, t0}
        /// </summary>
        /// <param name="values"></param>
        public void SetFitParams(double[] values)
        {
            tau1.Text = values[0].ToString();
            tau2.Text = values[1].ToString();
            tauG.Text = values[2].ToString();
            t0.Text = values[3].ToString();
            SetFitting_Param(true);
            UpdateImages(true, realtime, focusing, true);
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

        public void SetMinFLIMIntensity(double[] values)
        {
            MinIntensity1.Text = values[0].ToString();
            MinIntensity2.Text = values[1].ToString();

            ApplyTextToRange(true);
            UpdateImages(true, realtime, focusing, true);
        }

        public void SetMaxFLIMIntensity(double[] values)
        {
            MaxIntensity1.Text = values[0].ToString();
            MaxIntensity2.Text = values[1].ToString();

            ApplyTextToRange(true);
            UpdateImages(true, realtime, focusing, true);
        }

        public void HoldThisImage(bool ON)
        {
            HoldCurrentImageCheckBox.Checked = ON;
            HoldCurrentImageCheckBox_Click(HoldCurrentImageCheckBox, null);
        }

        //Kengo BEGIN 05-19-2025
        //for remote commands (SaveImageJROI, RecoverROIs, RemoveROIs)
        public void SaveImageJROI()
        {
            saveRoiAsImageJToolStripMenuItem_Click(this, null);
        }

        public void RecoverROIs()
        {
            ReadRois(true);
            DrawImages();
        }

        public void RemoveROIs()
        {
            RemoveAllROIs(this, null);
        }

        public void ShiftAllROIs(float[] values)
        {
            if (FLIM_ImgData.ROIs.Count == 0)
                return;

            if (values.Length == 2)
            {
                PointF shiftP2 = new PointF(values[0], values[1]);
                for (int i = 0; i < FLIM_ImgData.ROIs.Count; i++)
                    FLIM_ImgData.ROIs[i].shiftRoi(shiftP2);

            }
            else if (values.Length == 2 * FLIM_ImgData.ROIs.Count)
            {
                for (int i = 0; i < FLIM_ImgData.ROIs.Count; i++)
                {
                    PointF shiftP2 = new PointF(values[2 * i], values[2 * i + 1]);
                    FLIM_ImgData.ROIs[i].shiftRoi(shiftP2);
                }
            }
            else
                return;
            if (FLIM_ImgData.currentRoi >= 0)
                convertROIFromDataToDisplay(FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi], ref imageRoi);

            SaveAllRois();

            fittingDone = false;
            FLIM_ImgData.ResetLifetimeCalculation(true);
            UpdateFittingParam(currentChannel, fittingDone);
            if (image_display_state != ImageDisplay_State.Opening)
            {
                UpdateImages(true, realtime, focusing, true);
                if (!realtime)
                {
                    DrawImages();
                    plot_regular.plotNow_noRealtime(TCF, TC, this, currentChannel);
                }
            }
        }
        //Kengo END

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
                { }
                ;
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
                    if (entireStack)
                    {
                        int npages = FLIM_ImgData.n_pages;
                        //if (FLIM_ImgData.nFastZ > 1)
                        //    npages = FLIM_ImgData.nFastZ;

                        FLIM_ImgData.ZProjection_Range[0] = 0;
                        FLIM_ImgData.ZProjection_Range[1] = npages;
                    }

                    if (!FLIM_ImgData.KeepPagesInMemory)
                    {
                        if (FLIM_ImgData.ZProjection_Range[1] - FLIM_ImgData.ZProjection_Range[0] > 100)
                        {
                            DialogResult dr = MessageBox.Show("The file is big (>100 frames). Do you want to calculate Projection?", "Projection of big file", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                            if (dr != DialogResult.Yes)
                            {
                                displayZProjection = false;
                                cb_projectionYes.Checked = false;
                                EntireStack_Check.Checked = false;
                                return;
                            }
                        }
                    }

                    if (AveProjection.Checked)
                    {
                        FLIM_ImgData.calcZProject(FLIMData.projectionType.Sum, this);
                        FLIM_ImgData.ZProjection = true;
                    }
                    else if (MaxProjection.Checked)
                    {
                        FLIM_ImgData.calcZProject(FLIMData.projectionType.Max, this);
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
                if (FLIM_ImgData.ZProjection_Range[1] - FLIM_ImgData.ZProjection_Range[0] > 100 && !FLIM_ImgData.KeepPagesInMemory)
                {
                    int currentPage = FLIM_ImgData.currentPage;
                    if (FLIM_ImgData.nFastZ > 1)
                        currentPage = FLIM_ImgData.nFastZ;

                    FLIM_ImgData.ZProjection_Range[1] = currentPage;
                    FLIM_ImgData.ZProjection_Range[0] = currentPage;
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
            if (FLIM_ImgData.nFastZ > 1 || FLIM_ImgData.n_pages5D > 0)
            {
                int[][] preLoadFitRange = CaptureFitRangeFromGui();
                int preLoadFilterWindow = CaptureFilterWindowFromGui();
                FLIM_ImgData.gotoPage5D(page);
                ApplyFilterWindowToState(preLoadFilterWindow);
                ApplyFitRangeFromGui(preLoadFitRange);
                CurrentFastZPageTB.Text = (FLIM_ImgData.currentPage5D + 1).ToString();
                if (HasZStack5DPages())
                {
                    int zSlices = GetCurrent5DZSliceCount();
                    FLIM_ImgData.ZProjection_Range[0] = 0;
                    FLIM_ImgData.ZProjection_Range[1] = Math.Max(1, zSlices);
                }
            }
        }

        private void GotoPage4D(int page)
        {
            int[][] preLoadFitRange = CaptureFitRangeFromGui();
            int preLoadFilterWindow = CaptureFilterWindowFromGui();
            FLIM_ImgData.gotoPage(page);
            ApplyFilterWindowToState(preLoadFilterWindow);
            ApplyFitRangeFromGui(preLoadFitRange);
        }

        private void CalculatePhasorCurrentPageUponOpen()
        {
            if (flimage?.phasor_plot_window == null || flimage.phasor_plot_window.IsDisposed)
                return;

            flimage.phasor_plot_window.CalculateCurrentPageUponOpen(this);
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

        // KENGO BEGIN 1-8-2026
        // implement Right click for Page Up/Down
        // Left click inc:  1    10, +Ctrl   20  200
        // Right click inc: 5    50, +Ctrl   100 1000
        private void Page_UpDown_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                return;

            Debug.WriteLine("MouseDown");
            try
            {
                if (FLIM_ImgData.n_pages <= 1)
                    return;

                int dpage = 0;
                int page = FLIM_ImgData.currentPage;

                if (sender.Equals(PageUp))
                    dpage = 5;
                else if (sender.Equals(PageDown))
                    dpage = -5;
                else if (sender.Equals(PageUpUp))
                    dpage = 50;
                else if (sender.Equals(PageDownDown))
                    dpage = -50;

                if (Control.ModifierKeys == Keys.Control)
                    dpage *= 20;

                if (!displayZProjection)
                    GotoPage4D(page + dpage);
                else
                {
                    FLIM_ImgData.ZProjection_Range[0] = FLIM_ImgData.ZProjection_Range[0] + dpage;
                    FLIM_ImgData.ZProjection_Range[1] = FLIM_ImgData.ZProjection_Range[1] + dpage;
                    AssurePageRange();
                    calcZProjection();
                }

                UpdateImages(true, realtime, focusing, true);

                if ((plot_regular.calc_upon_open || AutoApplyOffset.Checked) && !ZStack && !FastZStack)
                    CalculateCurrentPage(false);
                if (!ZStack && !FastZStack)
                    CalculatePhasorCurrentPageUponOpen();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Right-click navigation handler failed: " + ex.Message);
            }
        }
        // KENGO END

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
            // Kengo BEGIN 05-15-2025
            // Add 20x page-updown(+Ctrl key)
            if (Control.ModifierKeys == Keys.Control)
                dpage *= 20;
            // Kengo END
            if (!displayZProjection)
                GotoPage4D(page + dpage);
            else
            {
                FLIM_ImgData.ZProjection_Range[0] = FLIM_ImgData.ZProjection_Range[0] + dpage;
                FLIM_ImgData.ZProjection_Range[1] = FLIM_ImgData.ZProjection_Range[1] + dpage;
                AssurePageRange();
                calcZProjection();
            }

            UpdateImages(true, realtime, focusing, true);

            if ((plot_regular.calc_upon_open || AutoApplyOffset.Checked) && !ZStack && !FastZStack)
                CalculateCurrentPage(false);
            if (!ZStack && !FastZStack)
                CalculatePhasorCurrentPageUponOpen();
        }


        private void AlignSlicesframesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AlignFrames();
        }

        private void alignEvenOddLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AlignEvenOddLines();
            calcZProjection();
        }

        public Task RunAutoResonantDelayEvenOddAsync()
        {
            return AutoResonantDelayEvenOddCoreAsync();
        }

        public Task RunQuickAutoScanDelayEvenOddAsync()
        {
            return QuickAutoScanDelayEvenOddCoreAsync();
        }

        private async Task QuickAutoScanDelayEvenOddCoreAsync()
        {
            var state = flimage?.State ?? FLIM_ImgData?.State;
            if (state?.Acq == null || state?.Init == null)
                return;

            bool isResOrPoly = state.Acq.resonantScanning || state.Acq.polygonScanning;
            bool isBidirectionalX = state.Acq.BiDirectionalScan || state.Acq.SineWaveScan;

            if (!(isResOrPoly || isBidirectionalX))
            {
                MessageBox.Show("Quick auto scan delay requires resonant/polygon scanning or bidirectional X scanning.");
                return;
            }

            if (_autoResonantDelayRunning)
                return;

            _autoResonantDelayRunning = true;
            bool stopFocusWhenDone = !(flimage?.flimage_io != null && flimage.flimage_io.focusing);
            bool acceptedDelay = false;
            double acceptedDelay_us = 0;

            try
            {
                int refChannel = currentChannel;
                int timeoutMs = EstimateFocusAveragedFrameTimeoutMs(state);
                if (!(flimage?.flimage_io != null && flimage.flimage_io.focusing && flimage.flimage_io.tcspc_on))
                {
                    flimage?.WriteStatusText("Quick auto scan delay: starting FOCUS...");
                    if (!await ResetFocusAndWaitForFrame(timeoutMs))
                    {
                        MessageBox.Show("Quick auto scan delay could not start FOCUS or did not receive a frame.");
                        return;
                    }
                }

                if (FLIM_ImgData?.Project == null || refChannel < 0 || refChannel >= FLIM_ImgData.Project.Length || FLIM_ImgData.Project[refChannel] == null)
                {
                    MessageBox.Show("No intensity projection is available after starting FOCUS.");
                    return;
                }

                ushort[,] xyProject = FLIM_ImgData.Project[refChannel];
                if (!TryMeasureEvenOddShiftCoarseToFine(xyProject, out int xShiftPx, out double shiftScore, out int xStart, out int xEnd, out string error))
                {
                    MessageBox.Show(error ?? "Failed to measure even/odd shift.");
                    return;
                }

                double corr = double.NaN;
                int used = 0;
                TryComputeEvenOddZeroShiftCorrelationFromProjection(xyProject, out corr, out used, out _, out _, out _);

                double pixelTime_us = state.Acq.PixelTime() * 1e6;
                double oldDelay_us = GetScanDelayUs(state);
                double deltaDelay_us = -(xShiftPx * pixelTime_us) / 2.0;
                double maxAutoDelay_us = MaxAutoScanDelayUs(state, Math.Max(8, state.Acq.pixelsPerLine), pixelTime_us);
                int maxQuickPx = MaxEvenOddShiftPixelsForAutoScan(Math.Max(8, state.Acq.pixelsPerLine));

                flimage?.WriteStatusText(string.Format(
                    "Quick auto scan delay: old {0:0.###} us, measured shift {1} px, request {2:0.###} us -> {3:0.###} us",
                    oldDelay_us, xShiftPx, deltaDelay_us, ClampScanDelayUs(state, oldDelay_us + deltaDelay_us)));

                VerifiedDelayStepResult verified;
                if (Math.Abs(xShiftPx) <= 1)
                {
                    double localStep_us = Math.Max(1.0, (Math.Max(5.0, state.Acq.pixelsPerLine / 50.0) * pixelTime_us) / 2.0);
                    localStep_us = Math.Min(localStep_us, Math.Max(1.0, maxAutoDelay_us / 4.0));

                    var localCandidates = BuildSymmetricDelayCandidates(state, oldDelay_us, localStep_us, stepsEachSide: 2, maxAutoDelta_us: maxAutoDelay_us);
                    flimage?.WriteStatusText(string.Format(
                        "Quick auto scan delay: shift estimate is small; local search +/-{0:0.###} us around {1:0.###} us",
                        localStep_us * 2.0, oldDelay_us));

                    var localBest = await ScanDelayCandidates(localCandidates, state, refChannel, timeoutMs, stageLabel: "quick local");
                    verified = new VerifiedDelayStepResult
                    {
                        valid = localBest.valid,
                        delay_us = localBest.delay_us,
                        corr0 = localBest.corr0,
                        used = localBest.used,
                        xStart = localBest.xStart,
                        xEnd = localBest.xEnd,
                        xShiftPx = xShiftPx,
                        bestShiftCorr = corr
                    };

                    if (verified.valid)
                    {
                        ApplyScanDelayToStateAndGui(verified.delay_us);
                        await ResetFocusAndWaitForFrame(timeoutMs);
                        ApplyScanDelayToStateAndGui(verified.delay_us);

                        if (TryMeasureCurrentEvenOddAlignment(refChannel, out VerifiedDelayStepResult finalEval, out _))
                        {
                            finalEval.delay_us = verified.delay_us;
                            verified = finalEval;
                        }
                    }
                }
                else
                {
                    verified = await ApplyVerifiedScanDelayStepAsync(state, refChannel, oldDelay_us, deltaDelay_us, timeoutMs, "quick", maxAutoDelay_us, preferChangedDelay: true);
                }
                if (!verified.valid)
                {
                    ApplyScanDelayToStateAndGui(oldDelay_us);
                    MessageBox.Show("Quick auto scan delay could not verify a better direction. Delay was restored.");
                    return;
                }

                acceptedDelay = true;
                acceptedDelay_us = verified.delay_us;

                string mode = isResOrPoly ? "resonantScanDelay_us" : "ScanDelay";
                string note = "";
                if (Math.Abs(xShiftPx) >= Math.Max(1, maxQuickPx - 1))
                    note = " (shift near limit: consider full auto scan delay)";

                if (Math.Abs(verified.delay_us - oldDelay_us) < 1e-9)
                    note = " (kept current delay; +/- tests did not improve alignment)";

                var status = string.Format(
                    "Quick auto scan delay ({0}): shift {1} px (score {2:0.###}, corr0 {3:0.###}, used {4}, x {5}-{6}, range +/-{7}) -> verified shift {8} px, delta {9:0.###} us, new {10:0.###} us{11}",
                    mode, xShiftPx, shiftScore, corr, used, xStart, xEnd, maxQuickPx, verified.xShiftPx, (verified.delay_us - oldDelay_us), verified.delay_us, note);
                flimage?.WriteStatusText(status);
            }
            catch (Exception ex)
            {
                try { MessageBox.Show("Quick auto scan delay failed: " + ex.Message); } catch { }
            }
            finally
            {
                StopAutoStartedFocus(stopFocusWhenDone);
                if (acceptedDelay)
                    ApplyScanDelayToStateAndGui(acceptedDelay_us);
                _autoResonantDelayRunning = false;
            }
        }

        private async Task AutoResonantDelayEvenOddCoreAsync()
        {
            var state = flimage?.State ?? FLIM_ImgData?.State;
            if (state?.Acq == null || state?.Init == null)
                return;

            bool isResOrPoly = state.Acq.resonantScanning || state.Acq.polygonScanning;
            bool isBidirectionalX = state.Acq.BiDirectionalScan || state.Acq.SineWaveScan;

            if (!(isResOrPoly || isBidirectionalX))
            {
                MessageBox.Show("Auto scan delay requires resonant/polygon scanning or bidirectional X scanning.");
                return;
            }

            if (_autoResonantDelayRunning)
                return;

            _autoResonantDelayRunning = true;
            bool stopFocusWhenDone = !(flimage?.flimage_io != null && flimage.flimage_io.focusing);

            try
            {
                int refChannel = currentChannel;

                if (!(flimage?.flimage_io != null && flimage.flimage_io.focusing && flimage.flimage_io.tcspc_on))
                {
                    flimage?.WriteStatusText("Auto scan delay: starting FOCUS...");
                    int timeoutMs = EstimateFocusAveragedFrameTimeoutMs(state);
                    if (!await ResetFocusAndWaitForFrame(timeoutMs))
                    {
                        MessageBox.Show("Auto scan delay could not start FOCUS or did not receive a frame.");
                        return;
                    }
                }

                await AutoResonantDelay_SearchByAcquisition(state, refChannel);
                return;
            }
            catch (Exception ex)
            {
                try
                {
                    MessageBox.Show("Auto scan delay failed: " + ex.Message);
                }
                catch { }
            }
            finally
            {
                StopAutoStartedFocus(stopFocusWhenDone);
                _autoResonantDelayRunning = false;
            }
        }

        private void StopAutoStartedFocus(bool stopFocusWhenDone)
        {
            if (!stopFocusWhenDone || flimage?.flimage_io == null || !flimage.flimage_io.focusing)
                return;

            try
            {
                flimage.flimage_io.StopFocus();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Auto scan delay stop focus failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Fast shift estimate for small corrections. Returns the X shift (pixels) to apply to EVEN lines to match ODD lines.
        /// Searches only a small shift range around 0 using Pearson correlation.
        /// </summary>
        private bool TryMeasureEvenOddShiftQuick(
            ushort[,] xyProject,
            out int xShiftPx,
            out double corr,
            out int used,
            out int xStart,
            out int xEnd,
            out int maxQuickPx,
            out string error)
        {
            xShiftPx = 0;
            corr = double.NaN;
            used = 0;
            xStart = 0;
            xEnd = 0;
            maxQuickPx = 0;
            error = null;

            if (xyProject == null)
            {
                error = "No image data.";
                return false;
            }

            int height = xyProject.GetLength(0);
            int width = xyProject.GetLength(1);
            if (height < 4 || width < 8)
            {
                error = "Image is too small to measure even/odd shift.";
                return false;
            }

            ImageProcessing.EvenOddImage(xyProject, out ushort[,] evenImg, out ushort[,] oddImg);
            if (evenImg == null || oddImg == null)
            {
                error = "Failed to build even/odd images.";
                return false;
            }

            // Use same crop logic as the robust method, but compute once.
            if (!TryComputeIntensityCropRange(xyProject, 0.05, 0.95, out xStart, out xEnd))
            {
                int cropW = Math.Max(8, width / 2);
                xStart = Math.Max(0, (width - cropW) / 2);
                xEnd = Math.Min(width, xStart + cropW);
            }

            ushort thr = EstimateForegroundThreshold(evenImg, oddImg);

            // Small search window; designed for "touch up" after full calibration.
            // Do not evaluate shifts that would leave too much of the image non-overlapping.
            maxQuickPx = Math.Min(MaxEvenOddShiftPixelsForAutoScan(width), Math.Max(10, width / 20));

            bool any = false;
            double best = double.NegativeInfinity;
            int bestShift = 0;
            int bestUsed = 0;

            for (int s = -maxQuickPx; s <= maxQuickPx; s++)
            {
                if (!TryPearsonCorrelation(evenImg, oddImg, thr, s, xStart, xEnd, out double c1, out int used1))
                    continue;

                if (!any || c1 > best)
                {
                    any = true;
                    best = c1;
                    bestShift = s;
                    bestUsed = used1;
                }
            }

            if (!any)
            {
                error = "Not enough informative pixels to estimate shift. Try increasing averaging or use full auto scan delay.";
                return false;
            }

            xShiftPx = bestShift;
            corr = best;
            used = bestUsed;
            return true;
        }

        private async Task AutoResonantDelay_SearchByAcquisition(ScanParameters state, int refChannel)
        {
            // This routine assumes the user is imaging a stationary target (beads/grid) and wants a robust calibration.
            // Strategy:
            // - coarse scan of scan delay +/- the non-overlap limit with large steps (~width/10 in pixels)
            // - refine around the best delay with smaller steps
            // - apply best delay and (optional) one-step fine correction using even/odd shift

            if (flimage?.flimage_io == null)
                return;

            // Guard: focusing must be running (ResetFocus only works then).
            if (!flimage.flimage_io.focusing)
            {
                MessageBox.Show("Start FOCUS on a stationary target first, then run Auto scan delay.");
                return;
            }

            if (FLIM_ImgData?.Project == null || refChannel < 0 || refChannel >= FLIM_ImgData.Project.Length)
            {
                MessageBox.Show("No projection buffer available.");
                return;
            }

            double oldDelay_us = GetScanDelayUs(state);
            double pixelTime_us = state.Acq.PixelTime() * 1e6;
            int width = Math.Max(8, state.Acq.pixelsPerLine);
            double searchRadius_us = MaxAutoScanDelayUs(state, width, pixelTime_us);
            double hardwareMaxDelay_us = MaxScanDelayUs(state);

            // Coarse step: about width/10 pixels worth of delay because shift is about 2 * delay / pixelTime.
            double stepCoarse_us = (width / 10.0) * pixelTime_us / 2.0;
            stepCoarse_us = Math.Max(0.2, stepCoarse_us);

            // Limit candidate count for usability.
            int maxCandidates = 21;
            double coarseMin = Math.Max(0, oldDelay_us - searchRadius_us);
            double coarseMax = Math.Min(hardwareMaxDelay_us, oldDelay_us + searchRadius_us);
            double coarseSpan = coarseMax - coarseMin;
            if (stepCoarse_us > 0 && coarseSpan / stepCoarse_us > maxCandidates)
                stepCoarse_us = coarseSpan / maxCandidates;

            int timeoutMs = EstimateFocusAveragedFrameTimeoutMs(state);

            var coarse = BuildDelayCandidates(coarseMin, coarseMax, stepCoarse_us);
            var bestCoarse = await ScanDelayCandidates(coarse, state, refChannel, timeoutMs, stageLabel: "coarse");
            if (!bestCoarse.valid)
            {
                ApplyScanDelayToStateAndGui(oldDelay_us);
                MessageBox.Show("Auto scan delay: coarse scan failed (no valid frames). Try increasing FocusAverage or signal.");
                return;
            }

            // Fine scan around the best coarse delay.
            double stepFine_us = Math.Max(0.05, stepCoarse_us / 5.0);
            double fineMin = Math.Max(coarseMin, bestCoarse.delay_us - 2 * stepCoarse_us);
            double fineMax = Math.Min(coarseMax, bestCoarse.delay_us + 2 * stepCoarse_us);
            var fine = BuildDelayCandidates(fineMin, fineMax, stepFine_us);
            var bestFine = await ScanDelayCandidates(fine, state, refChannel, timeoutMs, stageLabel: "fine");
            if (!bestFine.valid)
                bestFine = bestCoarse;

            // Apply best delay and restart focus once more.
            ApplyScanDelayToStateAndGui(bestFine.delay_us);
            await ResetFocusAndWaitForFrame(timeoutMs);

            // Optional final fine correction: measure even/odd shift on the now-reasonable image.
            int xShiftPx = 0;
            double shiftScore = double.NaN;
            int xStart = 0, xEnd = 0;
            if (FLIM_ImgData.Project[refChannel] != null &&
                TryMeasureEvenOddShiftCoarseToFine(FLIM_ImgData.Project[refChannel], out xShiftPx, out shiftScore, out xStart, out xEnd, out _))
            {
                double deltaDelay_us = -(xShiftPx * pixelTime_us) / 2.0;
                var verified = await ApplyVerifiedScanDelayStepAsync(state, refChannel, bestFine.delay_us, deltaDelay_us, timeoutMs, "fine verify", searchRadius_us);
                double finalDelay_us = verified.valid ? verified.delay_us : bestFine.delay_us;
                if (!verified.valid)
                    ApplyScanDelayToStateAndGui(bestFine.delay_us);

                flimage.WriteStatusText(string.Format(
                    "Auto scan delay: coarse/fine corr {0:0.###} (x {1}-{2}), refine shift {3} px -> verified shift {4} px, final {5:0.###} us",
                    bestFine.corr0, bestFine.xStart, bestFine.xEnd, xShiftPx, verified.valid ? verified.xShiftPx : xShiftPx, finalDelay_us));
            }
            else
            {
                flimage.WriteStatusText(string.Format(
                    "Auto scan delay: coarse/fine corr {0:0.###} (x {1}-{2}) -> set {3:0.###} us",
                    bestFine.corr0, bestFine.xStart, bestFine.xEnd, bestFine.delay_us));
            }
        }

        private struct DelayCandidateResult
        {
            public bool valid;
            public double delay_us;
            public double corr0;
            public int used;
            public int xStart;
            public int xEnd;
        }

        private struct VerifiedDelayStepResult
        {
            public bool valid;
            public double delay_us;
            public int xShiftPx;
            public double corr0;
            public double bestShiftCorr;
            public int used;
            public int xStart;
            public int xEnd;
        }

        private async Task<DelayCandidateResult> ScanDelayCandidates(List<double> candidates_us, ScanParameters state, int refChannel, int timeoutMs, string stageLabel)
        {
            var best = new DelayCandidateResult
            {
                valid = false,
                corr0 = double.NegativeInfinity
            };

            for (int i = 0; i < candidates_us.Count; i++)
            {
                double d = ClampScanDelayUs(state, candidates_us[i]);
                ApplyScanDelayToStateAndGui(d);

                bool got = await ResetFocusAndWaitForFrame(timeoutMs);
                if (!got)
                {
                    flimage.WriteStatusText(string.Format("Auto scan delay ({0}): {1}/{2} delay {3:0.###} us -> timeout", stageLabel, i + 1, candidates_us.Count, d));
                    continue;
                }

                if (!TryComputeEvenOddZeroShiftCorrelationFromCurrentProjection(refChannel, out double corr0, out int used, out int xStart, out int xEnd, out _))
                {
                    flimage.WriteStatusText(string.Format("Auto scan delay ({0}): {1}/{2} delay {3:0.###} us -> no score", stageLabel, i + 1, candidates_us.Count, d));
                    continue;
                }

                flimage.WriteStatusText(string.Format("Auto scan delay ({0}): {1}/{2} delay {3:0.###} us -> corr {4:0.###} (used {5}, x {6}-{7})",
                    stageLabel, i + 1, candidates_us.Count, d, corr0, used, xStart, xEnd));

                if (!best.valid || corr0 > best.corr0)
                {
                    best.valid = true;
                    best.delay_us = d;
                    best.corr0 = corr0;
                    best.used = used;
                    best.xStart = xStart;
                    best.xEnd = xEnd;
                }
            }

            return best;
        }

        private async Task<VerifiedDelayStepResult> ApplyVerifiedScanDelayStepAsync(ScanParameters state, int refChannel, double baseDelay_us, double requestedDelta_us, int timeoutMs, string stageLabel, double maxAutoDelta_us, bool preferChangedDelay = false)
        {
            var best = new VerifiedDelayStepResult
            {
                valid = false,
                corr0 = double.NegativeInfinity,
                bestShiftCorr = double.NegativeInfinity
            };

            var candidates = BuildVerifiedDelayCandidates(state, baseDelay_us, requestedDelta_us, maxAutoDelta_us);
            double currentDelay_us = GetScanDelayUs(state);

            for (int i = 0; i < candidates.Count; i++)
            {
                double d = candidates[i];
                ApplyScanDelayToStateAndGui(d);
                currentDelay_us = d;

                bool got = await ResetFocusAndWaitForFrame(timeoutMs);
                if (!got)
                {
                    flimage?.WriteStatusText(string.Format("Auto scan delay ({0}): verify {1}/{2} delay {3:0.###} us -> timeout", stageLabel, i + 1, candidates.Count, d));
                    continue;
                }

                if (!TryMeasureCurrentEvenOddAlignment(refChannel, out VerifiedDelayStepResult eval, out _))
                {
                    flimage?.WriteStatusText(string.Format("Auto scan delay ({0}): verify {1}/{2} delay {3:0.###} us -> no score", stageLabel, i + 1, candidates.Count, d));
                    continue;
                }

                eval.delay_us = d;
                flimage?.WriteStatusText(string.Format(
                    "Auto scan delay ({0}): verify {1}/{2} delay {3:0.###} us -> shift {4} px, corr0 {5:0.###}",
                    stageLabel, i + 1, candidates.Count, d, eval.xShiftPx, eval.corr0));

                if (IsBetterVerifiedDelay(eval, best, baseDelay_us, preferChangedDelay))
                    best = eval;
            }

            if (!best.valid)
            {
                ApplyScanDelayToStateAndGui(baseDelay_us);
                if (Math.Abs(currentDelay_us - baseDelay_us) > 1e-9)
                    await ResetFocusAndWaitForFrame(timeoutMs);
                return best;
            }

            ApplyScanDelayToStateAndGui(best.delay_us);
            if (Math.Abs(currentDelay_us - best.delay_us) > 1e-9)
            {
                await ResetFocusAndWaitForFrame(timeoutMs);
                ApplyScanDelayToStateAndGui(best.delay_us);
            }

            return best;
        }

        private static List<double> BuildVerifiedDelayCandidates(ScanParameters state, double baseDelay_us, double requestedDelta_us, double maxAutoDelta_us)
        {
            var list = new List<double>();
            double[] candidates = new double[]
            {
                baseDelay_us,
                baseDelay_us + requestedDelta_us,
                baseDelay_us - requestedDelta_us
            };

            foreach (double candidate in candidates)
            {
                double d = ClampScanDelayUs(state, candidate);
                if (!double.IsNaN(maxAutoDelta_us) && !double.IsInfinity(maxAutoDelta_us) && maxAutoDelta_us >= 0)
                {
                    double minDelay_us = ClampScanDelayUs(state, baseDelay_us - maxAutoDelta_us);
                    double maxDelay_us = ClampScanDelayUs(state, baseDelay_us + maxAutoDelta_us);
                    if (d < minDelay_us) d = minDelay_us;
                    if (d > maxDelay_us) d = maxDelay_us;
                }

                bool duplicate = false;
                for (int i = 0; i < list.Count; i++)
                {
                    if (Math.Abs(list[i] - d) < 1e-6)
                    {
                        duplicate = true;
                        break;
                    }
                }

                if (!duplicate)
                    list.Add(d);
            }

            return list;
        }

        private static List<double> BuildSymmetricDelayCandidates(ScanParameters state, double centerDelay_us, double step_us, int stepsEachSide, double maxAutoDelta_us)
        {
            var list = new List<double>();
            if (step_us <= 0)
                step_us = 1.0;
            if (stepsEachSide < 1)
                stepsEachSide = 1;

            AddDelayCandidate(list, ClampScanDelayUs(state, centerDelay_us));
            for (int k = 1; k <= stepsEachSide; k++)
            {
                AddDelayCandidate(list, ClampScanDelayUs(state, centerDelay_us - k * step_us));
                AddDelayCandidate(list, ClampScanDelayUs(state, centerDelay_us + k * step_us));
            }

            if (!double.IsNaN(maxAutoDelta_us) && !double.IsInfinity(maxAutoDelta_us) && maxAutoDelta_us >= 0)
            {
                double minDelay_us = ClampScanDelayUs(state, centerDelay_us - maxAutoDelta_us);
                double maxDelay_us = ClampScanDelayUs(state, centerDelay_us + maxAutoDelta_us);
                list = list.Select(d =>
                {
                    if (d < minDelay_us) return minDelay_us;
                    if (d > maxDelay_us) return maxDelay_us;
                    return d;
                }).Distinct(new DelayCandidateComparer()).ToList();
            }

            return list;
        }

        private static void AddDelayCandidate(List<double> list, double delay_us)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (Math.Abs(list[i] - delay_us) < 1e-6)
                    return;
            }

            list.Add(delay_us);
        }

        private sealed class DelayCandidateComparer : IEqualityComparer<double>
        {
            public bool Equals(double x, double y)
            {
                return Math.Abs(x - y) < 1e-6;
            }

            public int GetHashCode(double obj)
            {
                return Math.Round(obj, 6).GetHashCode();
            }
        }

        private static bool IsBetterVerifiedDelay(VerifiedDelayStepResult candidate, VerifiedDelayStepResult best, double baseDelay_us, bool preferChangedDelay)
        {
            if (!candidate.valid)
                return false;

            if (!best.valid)
                return true;

            int cAbs = Math.Abs(candidate.xShiftPx);
            int bAbs = Math.Abs(best.xShiftPx);
            if (cAbs < bAbs)
                return true;
            if (bAbs < cAbs)
                return false;

            if (candidate.corr0 > best.corr0 + 0.002)
                return true;
            if (best.corr0 > candidate.corr0 + 0.002)
                return false;

            double cDist = Math.Abs(candidate.delay_us - baseDelay_us);
            double bDist = Math.Abs(best.delay_us - baseDelay_us);
            if (preferChangedDelay && Math.Abs(cDist - bDist) > 1e-9)
                return cDist > bDist;

            return cDist < bDist;
        }

        private bool TryMeasureCurrentEvenOddAlignment(int refChannel, out VerifiedDelayStepResult result, out string error)
        {
            result = new VerifiedDelayStepResult
            {
                valid = false,
                corr0 = double.NegativeInfinity,
                bestShiftCorr = double.NegativeInfinity
            };
            error = null;

            if (FLIM_ImgData?.Project == null || refChannel < 0 || refChannel >= FLIM_ImgData.Project.Length || FLIM_ImgData.Project[refChannel] == null)
            {
                error = "No projection.";
                return false;
            }

            ushort[,] proj = FLIM_ImgData.Project[refChannel];
            if (!TryMeasureEvenOddShiftCoarseToFine(proj, out int xShiftPx, out _, out int xStart, out int xEnd, out error))
                return false;

            if (!TryMeasureEvenOddShiftQuick(proj, out _, out double bestShiftCorr, out _, out _, out _, out _, out _))
                bestShiftCorr = double.NaN;

            if (!TryComputeEvenOddZeroShiftCorrelationFromProjection(proj, out double corr0, out int used, out int corrXStart, out int corrXEnd, out error))
                return false;

            result.valid = true;
            result.delay_us = 0;
            result.xShiftPx = xShiftPx;
            result.corr0 = corr0;
            result.bestShiftCorr = bestShiftCorr;
            result.used = used;
            result.xStart = corrXEnd > corrXStart ? corrXStart : xStart;
            result.xEnd = corrXEnd > corrXStart ? corrXEnd : xEnd;
            return true;
        }

        private async Task<bool> ResetFocusAndWaitForFrame(int timeoutMs)
        {
            if (flimage?.flimage_io == null)
                return false;

            // Trigger restart (async) while safely updating TCSPC parameters.
            // We need to update TCSPC decode offsets (AcquisitionDelay/BiDirectionalDelay) BEFORE restarting,
            // but doing so while acquisition is running is unreliable. So we pause first, then update params, then restart.
            if (flimage.flimage_io.focusing && !flimage.flimage_io.refocusing)
            {
                flimage.flimage_io.refocusing = true;
                flimage.flimage_io.displayPageCounter = 0;
                _ = Task.Run(() =>
                {
                    try
                    {
                        flimage.flimage_io.PauseFocus();
                        try { flimage.flimage_io.SetupFLIMParameters(flimage.flimage_io.State); } catch { }
                        flimage.flimage_io.DisposeDAQ();
                        flimage.flimage_io.ReStartFocus();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Auto scan delay focus restart failed: " + ex.Message);
                    }
                    finally
                    {
                        flimage.flimage_io.refocusing = false;
                    }
                });
            }
            else
            {
                flimage.flimage_io.displayPageCounter = 0;
                bool started = false;
                try
                {
                    if (flimage.InvokeRequired)
                        flimage.Invoke((Action)(() => started = flimage.flimage_io.TryStartFocus()));
                    else
                        started = flimage.flimage_io.TryStartFocus();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Auto scan delay focus start failed: " + ex.Message);
                    return false;
                }

                if (!started && !flimage.flimage_io.focusing && !flimage.flimage_io.refocusing)
                    return false;
            }

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                await Task.Delay(20);

                // Wait until refocusing is done and at least one displayed frame arrived.
                if (!flimage.flimage_io.refocusing &&
                    flimage.flimage_io.displayPageCounter >= 1 &&
                    !update_image_busy)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryComputeEvenOddZeroShiftCorrelationFromCurrentProjection(
            int refChannel,
            out double corr0,
            out int used,
            out int xStart,
            out int xEnd,
            out string error)
        {
            corr0 = double.NaN;
            used = 0;
            xStart = 0;
            xEnd = 0;
            error = null;

            if (FLIM_ImgData?.Project == null || refChannel < 0 || refChannel >= FLIM_ImgData.Project.Length)
            {
                error = "No projection.";
                return false;
            }

            var proj = FLIM_ImgData.Project[refChannel];
            if (proj == null)
            {
                error = "Projection is null.";
                return false;
            }

            return TryComputeEvenOddZeroShiftCorrelationFromProjection(proj, out corr0, out used, out xStart, out xEnd, out error);
        }

        private bool TryComputeEvenOddZeroShiftCorrelationFromProjection(
            ushort[,] proj,
            out double corr0,
            out int used,
            out int xStart,
            out int xEnd,
            out string error)
        {
            corr0 = double.NaN;
            used = 0;
            xStart = 0;
            xEnd = 0;
            error = null;

            if (proj == null)
            {
                error = "Projection is null.";
                return false;
            }

            ImageProcessing.EvenOddImage(proj, out ushort[,] evenImg, out ushort[,] oddImg);
            if (evenImg == null || oddImg == null)
            {
                error = "Failed to build even/odd images.";
                return false;
            }

            if (!TryComputeIntensityCropRange(proj, 0.05, 0.95, out xStart, out xEnd))
            {
                // fallback: central half
                int w = proj.GetLength(1);
                int cropW = Math.Max(8, w / 2);
                xStart = Math.Max(0, (w - cropW) / 2);
                xEnd = Math.Min(w, xStart + cropW);
            }

            ushort thr = EstimateForegroundThreshold(evenImg, oddImg);
            if (!TryPearsonCorrelation(evenImg, oddImg, thr, shift: 0, xStart, xEnd, out corr0, out used))
            {
                error = "Not enough informative pixels / variance to compute correlation.";
                return false;
            }

            return true;
        }

        private static bool TryPearsonCorrelation(
            ushort[,] evenImg,
            ushort[,] oddImg,
            ushort threshold,
            int shift,
            int xStart,
            int xEnd,
            out double corr,
            out int used)
        {
            corr = double.NaN;
            used = 0;

            int h = evenImg.GetLength(0);
            int w = evenImg.GetLength(1);
            if (h != oddImg.GetLength(0) || w != oddImg.GetLength(1))
                return false;

            // Clamp range.
            if (xStart < 0) xStart = 0;
            if (xEnd > w) xEnd = w;
            if (xEnd <= xStart)
                return false;

            int xMin = xStart;
            int xMax = xEnd;
            if (shift >= 0)
                xMin = Math.Max(xMin, shift);
            else
                xMax = Math.Min(xMax, w + shift);

            if (xMax <= xMin)
                return false;

            double sumA = 0, sumB = 0, sumAA = 0, sumBB = 0, sumAB = 0;
            int n = 0;

            for (int y = 0; y < h; y++)
            {
                for (int x = xMin; x < xMax; x++)
                {
                    int ex = x - shift;
                    ushort a = evenImg[y, ex];
                    ushort b = oddImg[y, x];

                    if (a < threshold && b < threshold)
                        continue;

                    double da = a;
                    double db = b;
                    sumA += da;
                    sumB += db;
                    sumAA += da * da;
                    sumBB += db * db;
                    sumAB += da * db;
                    n++;
                }
            }

            used = n;
            long minUsed = Math.Max(50, (long)h * Math.Max(1, (xEnd - xStart)) / 1000);
            if (n < minUsed)
                return false;

            double invN = 1.0 / n;
            double meanA = sumA * invN;
            double meanB = sumB * invN;
            double varA = sumAA * invN - meanA * meanA;
            double varB = sumBB * invN - meanB * meanB;
            double cov = sumAB * invN - meanA * meanB;

            if (varA <= 0 || varB <= 0)
                return false;

            corr = cov / Math.Sqrt(varA * varB);
            return !double.IsNaN(corr) && !double.IsInfinity(corr);
        }

        private static List<double> BuildDelayCandidates(double start_us, double end_us, double step_us)
        {
            var list = new List<double>();
            if (step_us <= 0)
                step_us = 0.1;

            if (end_us < start_us)
            {
                double tmp = start_us;
                start_us = end_us;
                end_us = tmp;
            }

            int maxN = 51; // hard cap to avoid pathological cases
            for (int k = 0; k < maxN; k++)
            {
                double v = start_us + k * step_us;
                if (v > end_us + 1e-9)
                    break;
                list.Add(v);
            }

            // Ensure end point included when reasonable.
            if (list.Count > 0 && Math.Abs(list[list.Count - 1] - end_us) > step_us * 0.25)
                list.Add(end_us);

            return list;
        }

        private static double MaxScanDelayUs(ScanParameters state)
        {
            // Match ScanDelayChange() bounds:
            // - resonant:   [0 .. 0.5 * (500/resFreq) ms]
            // - polygon:    [0 .. 0.5 * (1000/resFreq) ms]
            // - galvo:      [0 .. 0.5 * msPerLineActual()]
            if (state?.Acq == null || state?.Init == null)
                return 0;

            if (state.Acq.polygonScanning)
            {
                double line_ms = 1000.0 / Math.Max(1.0, state.Init.resonantFreq_Hz);
                return 0.5 * line_ms * 1000.0;
            }

            if (state.Acq.resonantScanning)
            {
                double line_ms = 500.0 / Math.Max(1.0, state.Init.resonantFreq_Hz);
                return 0.5 * line_ms * 1000.0;
            }

            double msPerLine = state.Acq.msPerLineActual();
            if (double.IsNaN(msPerLine) || double.IsInfinity(msPerLine) || msPerLine <= 0)
                msPerLine = state.Acq.msPerLine;

            return 0.5 * msPerLine * 1000.0;
        }

        private static int MaxEvenOddShiftPixelsForAutoScan(int width)
        {
            if (width <= 0)
                return 0;

            return Math.Max(0, (int)Math.Floor(width * AutoScanDelayMaxMissingFraction));
        }

        private static double MaxAutoScanDelayUs(ScanParameters state, int width, double pixelTime_us)
        {
            double hardwareMax_us = MaxScanDelayUs(state);
            if (double.IsNaN(pixelTime_us) || double.IsInfinity(pixelTime_us) || pixelTime_us <= 0)
                return hardwareMax_us;

            double coverageMax_us = MaxEvenOddShiftPixelsForAutoScan(width) * pixelTime_us / 2.0;
            return Math.Min(hardwareMax_us, coverageMax_us);
        }

        private static double GetScanDelayUs(ScanParameters state)
        {
            if (state?.Acq == null)
                return 0;

            if (state.Acq.resonantScanning || state.Acq.polygonScanning)
                return state.Acq.resonantScanDelay_us;

            return state.Acq.ScanDelay * 1000.0; // ms -> us
        }

        private static double ClampScanDelayUs(ScanParameters state, double delay_us)
        {
            double max = MaxScanDelayUs(state);
            if (double.IsNaN(delay_us) || double.IsInfinity(delay_us))
                return GetScanDelayUs(state);
            if (delay_us < 0) delay_us = 0;
            if (delay_us > max) delay_us = max;
            return delay_us;
        }

        private void ApplyScanDelayToStateAndGui(double delay_us)
        {
            var state = flimage?.State ?? FLIM_ImgData?.State;
            if (state?.Acq == null)
                return;

            delay_us = ClampScanDelayUs(state, delay_us);

            // Update State in correct unit.
            if (state.Acq.resonantScanning || state.Acq.polygonScanning)
                state.Acq.resonantScanDelay_us = delay_us;
            else
                state.Acq.ScanDelay = delay_us / 1000.0; // us -> ms

            // Update flimage_io.State (usually same reference, but keep safe).
            if (flimage?.flimage_io?.State?.Acq != null)
            {
                if (flimage.flimage_io.State.Acq.resonantScanning || flimage.flimage_io.State.Acq.polygonScanning)
                    flimage.flimage_io.State.Acq.resonantScanDelay_us = delay_us;
                else
                    flimage.flimage_io.State.Acq.ScanDelay = delay_us / 1000.0;
            }

            // Update GUI control if available (stored as ns in NumericUpDown/Text).
            if (flimage != null && flimage.ScanDelayUpDown_nano != null)
            {
                double newDelay_ns = delay_us * 1000.0;
                Action updateScanDelayControl = () =>
                {
                    try
                    {
                        decimal v = (decimal)newDelay_ns;
                        if (v < flimage.ScanDelayUpDown_nano.Minimum) v = flimage.ScanDelayUpDown_nano.Minimum;
                        if (v > flimage.ScanDelayUpDown_nano.Maximum) v = flimage.ScanDelayUpDown_nano.Maximum;
                        flimage.ScanDelayUpDown_nano.Value = v;
                        flimage.ScanDelayUpDown_nano.Text = string.Format("{0:0.###}", newDelay_ns);
                    }
                    catch
                    {
                        try { flimage.ScanDelayUpDown_nano.Text = string.Format("{0:0.###}", newDelay_ns); } catch { }
                    }
                };

                try
                {
                    if (flimage.InvokeRequired)
                        flimage.Invoke(updateScanDelayControl);
                    else
                        updateScanDelayControl();
                }
                catch { }
            }

            // IMPORTANT: Also update TCSPC decode parameters (AcquisitionDelay/BiDirectionalDelay) so the next
            // restart uses the correct offsets. This is critical when fillFraction is low and photons are dropped.
            try
            {
                // Only safe to push parameters to TCSPC when acquisition is NOT running.
                if (flimage?.flimage_io != null && !flimage.flimage_io.runningImgAcq)
                    flimage.flimage_io.SetupFLIMParameters(state);
            }
            catch { }
        }

        private static int EstimateFocusAveragedFrameTimeoutMs(ScanParameters state)
        {
            // Rough timeout = 3x expected averaged frame time, clamped.
            double msPerLine = state.Acq.msPerLineActual();
            if ((state.Acq.resonantScanning || state.Acq.polygonScanning) && (msPerLine <= 0 || msPerLine > 1000))
            {
                // fallback if resonant_msPerLine isn't initialized correctly
                msPerLine = state.Acq.polygonScanning
                    ? (1000.0 / Math.Max(1.0, state.Init.resonantFreq_Hz))
                    : (500.0 / Math.Max(1.0, state.Init.resonantFreq_Hz));
            }
            double frameMs = msPerLine * Math.Max(1, state.Acq.linesPerFrame);
            double ave = (state.Acq.resonantScanning || state.Acq.polygonScanning)
                ? Math.Max(1, state.Acq.nAveFrame_focus_resonant)
                : Math.Max(1, state.Acq.nAveFrame_focus);
            double expected = frameMs * ave + 500.0;
            int timeout = (int)Math.Round(expected * 3.0);
            if (timeout < 4000) timeout = 4000;
            if (timeout > 30000) timeout = 30000;
            return timeout;
        }

        /// <summary>
        /// Estimate the even/odd X shift robustly using a coarse-to-fine search on the current intensity projection.
        /// Returns the shift (pixels) to apply to EVEN lines to match ODD lines.
        /// </summary>
        private bool TryMeasureEvenOddShiftCoarseToFine(ushort[,] xyProject, out int xShiftPx, out double score, out int xStart, out int xEnd, out string error)
        {
            xShiftPx = 0;
            score = double.PositiveInfinity;
            xStart = 0;
            xEnd = 0;
            error = null;

            if (xyProject == null)
            {
                error = "No image data.";
                return false;
            }

            int height = xyProject.GetLength(0);
            int width = xyProject.GetLength(1);
            if (height < 4 || width < 8)
            {
                error = "Image is too small to measure even/odd shift.";
                return false;
            }

            // Compute an X-range that concentrates on where photons actually are (avoid blank edges).
            if (!TryComputeIntensityCropRange(xyProject, 0.05, 0.95, out xStart, out xEnd))
            {
                // Fallback: central region based on fill fraction (or central half if unknown).
                double frac = 0.5;
                try { frac = (flimage?.State?.Acq?.fillFraction ?? frac); } catch { }
                if (frac <= 0 || frac > 1) frac = 0.5;
                frac = Math.Min(0.9, Math.Max(0.3, frac * 0.9)); // give a little safety margin
                int cropW = Math.Max(8, (int)Math.Round(width * frac));
                xStart = Math.Max(0, (width - cropW) / 2);
                xEnd = Math.Min(width, xStart + cropW);
            }

            // Make sure the crop region is sane.
            int minCrop = Math.Max(8, width / 10);
            if (xEnd - xStart < minCrop)
            {
                xStart = Math.Max(0, width / 2 - minCrop / 2);
                xEnd = Math.Min(width, xStart + minCrop);
            }

            // Build even/odd images.
            ImageProcessing.EvenOddImage(xyProject, out ushort[,] evenImg, out ushort[,] oddImg);
            if (evenImg == null || oddImg == null)
            {
                error = "Failed to build even/odd images.";
                return false;
            }

            // Determine a threshold to ignore blank background (focus on beads/structure).
            ushort thr = EstimateForegroundThreshold(evenImg, oddImg);

            // Search range in pixels. Larger shifts leave too much of one side blank/non-overlapping.
            int maxShift = MaxEvenOddShiftPixelsForAutoScan(width);

            // Coarse-to-fine search (big steps first ~1/10 width).
            int step1 = Math.Max(1, width / 10);
            int step2 = Math.Max(1, width / 50);

            var best1 = FindTopShifts(evenImg, oddImg, thr, xStart, xEnd, -maxShift, maxShift, step1, topK: 5);
            if (best1.Count == 0)
            {
                error = "Could not evaluate shift candidates (too few informative pixels?). Try increasing averaging.";
                return false;
            }

            // Refine around the best few coarse candidates.
            int refineRange2 = step1 * 2;
            var best2 = new List<(int shift, double score)>();
            foreach (var seed in best1.Select(b => b.shift).Distinct())
            {
                int a = Math.Max(-maxShift, seed - refineRange2);
                int b = Math.Min(maxShift, seed + refineRange2);
                best2.AddRange(FindTopShifts(evenImg, oddImg, thr, xStart, xEnd, a, b, step2, topK: 3));
            }
            best2 = best2.OrderBy(t => t.score).Take(5).ToList();

            // Final brute-force (step=1) around the best refined candidate.
            int seedFinal = best2.Count > 0 ? best2[0].shift : best1[0].shift;
            int refineRange3 = Math.Max(10, step2 * 3);
            int a3 = Math.Max(-maxShift, seedFinal - refineRange3);
            int b3 = Math.Min(maxShift, seedFinal + refineRange3);
            var best3 = FindTopShifts(evenImg, oddImg, thr, xStart, xEnd, a3, b3, step: 1, topK: 1);
            if (best3.Count == 0)
            {
                error = "Failed to refine shift.";
                return false;
            }

            xShiftPx = best3[0].shift;
            score = best3[0].score;
            return true;
        }

        private static ushort EstimateForegroundThreshold(ushort[,] evenImg, ushort[,] oddImg)
        {
            // Use a small fraction of the max to ignore blank background.
            ushort maxV = 0;
            int h = evenImg.GetLength(0);
            int w = evenImg.GetLength(1);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    ushort a = evenImg[y, x];
                    if (a > maxV) maxV = a;
                    ushort b = oddImg[y, x];
                    if (b > maxV) maxV = b;
                }
            }

            // ~2.5% of max, but at least 1 count.
            int thr = Math.Max(1, maxV / 40);
            if (thr > ushort.MaxValue) thr = ushort.MaxValue;
            return (ushort)thr;
        }

        private static List<(int shift, double score)> FindTopShifts(
            ushort[,] evenImg,
            ushort[,] oddImg,
            ushort threshold,
            int xStart,
            int xEnd,
            int shiftMin,
            int shiftMax,
            int step,
            int topK)
        {
            var best = new List<(int shift, double score)>(topK);

            if (step <= 0) step = 1;
            if (shiftMin > shiftMax)
            {
                int tmp = shiftMin;
                shiftMin = shiftMax;
                shiftMax = tmp;
            }

            for (int s = shiftMin; s <= shiftMax; s += step)
            {
                double sc = ScoreShiftMeanAbsDiff(evenImg, oddImg, threshold, s, xStart, xEnd);
                if (double.IsInfinity(sc) || double.IsNaN(sc))
                    continue;

                // Insert into top-K list.
                if (best.Count < topK)
                {
                    best.Add((s, sc));
                    best.Sort((a, b) => a.score.CompareTo(b.score));
                }
                else if (sc < best[best.Count - 1].score)
                {
                    best[best.Count - 1] = (s, sc);
                    best.Sort((a, b) => a.score.CompareTo(b.score));
                }
            }

            return best;
        }

        /// <summary>
        /// Score a shift by comparing shifted EVEN vs ODD in a given X-range.
        /// Returns mean absolute difference over "informative" pixels (above threshold in either image).
        /// Penalizes shifts that leave too few informative pixels overlapping.
        /// </summary>
        private static double ScoreShiftMeanAbsDiff(ushort[,] evenImg, ushort[,] oddImg, ushort threshold, int shift, int xStart, int xEnd)
        {
            int h = evenImg.GetLength(0);
            int w = evenImg.GetLength(1);

            if (h != oddImg.GetLength(0) || w != oddImg.GetLength(1))
                return double.PositiveInfinity;

            // Clamp x-range.
            if (xStart < 0) xStart = 0;
            if (xEnd > w) xEnd = w;
            if (xEnd <= xStart)
                return double.PositiveInfinity;

            int xMin = xStart;
            int xMax = xEnd;
            if (shift >= 0)
            {
                // evenIdx = x - shift must be >= 0  -> x >= shift
                xMin = Math.Max(xMin, shift);
            }
            else
            {
                // evenIdx = x - shift must be < w -> x < w + shift (shift negative)
                xMax = Math.Min(xMax, w + shift);
            }

            if (xMax <= xMin)
                return double.PositiveInfinity;

            long sumAbs = 0;
            long used = 0;

            for (int y = 0; y < h; y++)
            {
                for (int x = xMin; x < xMax; x++)
                {
                    int ex = x - shift;
                    ushort a = evenImg[y, ex];
                    ushort b = oddImg[y, x];

                    // Ignore blank background so it can't dominate the score.
                    if (a < threshold && b < threshold)
                        continue;

                    sumAbs += Math.Abs((int)a - (int)b);
                    used++;
                }
            }

            // Require enough informative pixels; otherwise shifts aligning blank regions look "good".
            // Heuristic: at least ~0.1% of the crop region, with a small absolute floor.
            long minUsed = Math.Max(50, (long)h * Math.Max(1, (xEnd - xStart)) / 1000);
            if (used < minUsed)
                return double.PositiveInfinity;

            return (double)sumAbs / (double)used;
        }

        /// <summary>
        /// Find an X-range containing most intensity (to avoid blank/warped edges).
        /// </summary>
        private static bool TryComputeIntensityCropRange(ushort[,] img, double lowFrac, double highFrac, out int xStart, out int xEnd)
        {
            xStart = 0;
            xEnd = 0;

            int h = img.GetLength(0);
            int w = img.GetLength(1);
            if (h <= 0 || w <= 0)
                return false;

            if (lowFrac < 0) lowFrac = 0;
            if (highFrac > 1) highFrac = 1;
            if (highFrac <= lowFrac)
                return false;

            double[] col = new double[w];
            double total = 0;

            for (int x = 0; x < w; x++)
            {
                long s = 0;
                for (int y = 0; y < h; y++)
                    s += img[y, x];
                col[x] = s;
                total += s;
            }

            if (total <= 0)
                return false;

            double lo = total * lowFrac;
            double hi = total * highFrac;

            double c = 0;
            int xs = 0;
            for (int x = 0; x < w; x++)
            {
                c += col[x];
                if (c >= lo)
                {
                    xs = x;
                    break;
                }
            }

            c = 0;
            int xe = w - 1;
            for (int x = 0; x < w; x++)
            {
                c += col[x];
                if (c >= hi)
                {
                    xe = x;
                    break;
                }
            }

            // Add a small margin so we don't hug the edge too tightly.
            int margin = Math.Max(4, w / 50);
            xStart = Math.Max(0, xs - margin);
            xEnd = Math.Min(w, xe + margin + 1);

            return xEnd > xStart + 8;
        }

        public void FlipImages()
        {
            var new_flim = new ushort[FLIM_ImgData.nChannels][,,];
            for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                new_flim[ch] = ImageProcessing.flipImageX(FLIM_ImgData.FLIMRaw[ch]);

            FLIM_ImgData.LoadFLIMRawFromData4D(new_flim, FLIM_ImgData.acquiredTime, false);
            FLIM_ImgData.calculateAll();
            FLIM_ImgData.addCurrentFLIMRawToPage4D(true, FLIM_ImgData.currentPage, true);

            UpdateImages(true, realtime, focusing, true);
            this.Refresh();
        }

        public void measureOddLineDifference(out double new_resonant_delay_us, out double diff_us, ScanParameters State)
        {
            new_resonant_delay_us = double.NaN;
            diff_us = double.NaN;

            if (State?.Acq == null || FLIM_ImgData?.Project == null)
                return;

            int refChannel = currentChannel;
            if (refChannel < 0 || refChannel >= FLIM_ImgData.Project.Length || FLIM_ImgData.Project[refChannel] == null)
                return;

            ushort[,] xyProject = FLIM_ImgData.Project[refChannel];
            double pixelTime_us = State.Acq.PixelTime() * 1e6;

            if (!TryMeasureEvenOddShiftCoarseToFine(xyProject, out int xShiftPx, out _, out _, out _, out _))
                return;

            diff_us = -(xShiftPx * pixelTime_us) / 2.0;

            if (State.Acq.resonantScanning || State.Acq.polygonScanning)
            {
                new_resonant_delay_us = State.Acq.resonantScanDelay_us + diff_us;
            }
            else
            {
                // Report in microseconds even for galvo scan-delay (which is stored in ms).
                new_resonant_delay_us = State.Acq.ScanDelay * 1000.0 + diff_us;
            }
        }


        /// <summary>
        /// Align even odd lines based on the displayed image.
        /// </summary>
        public void AlignEvenOddLines()
        {
            var fileName = FLIM_ImgData.fullFileName;
            var fileName1 = Path.GetFileNameWithoutExtension(fileName) + "_alignEvenOdd.flim";
            var dirName = Path.GetDirectoryName(fileName);

            fileName = Path.Combine(dirName, fileName1);

            var fileIO = new FileIO(FLIM_ImgData.State);
            int refChannel = currentChannel;
            ushort[,] xyProject = FLIM_ImgData.Project[refChannel];
            ImageProcessing.EvenOddImage(xyProject, out ushort[,] arrModified_even, out ushort[,] arrModified_odd);
            ImageProcessing.MatrixMeasureDrift2D_FFT(arrModified_even, arrModified_odd, out double[] xyDrift);

            for (int i = 0; i < FLIM_ImgData.n_pages; i++)
            {
                GotoPage4D(i);

                var new_flim = new ushort[FLIM_ImgData.nChannels][,,];
                for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                {
                    new_flim[ch] = ImageProcessing.CorrectEvenOddDifference(FLIM_ImgData.FLIMRaw[ch], FLIM_ImgData.Project[ch], -xyDrift[0]);
                }

                FLIM_ImgData.LoadFLIMRawFromData4D(new_flim, FLIM_ImgData.acquiredTime, false);
                FLIM_ImgData.calculateAll();

                if (FLIM_ImgData.KeepPagesInMemory)
                {
                    FLIM_ImgData.addCurrentFLIMRawToPage4D(true, i, true);
                }
                else
                {
                    var error = fileIO.SaveFLIMInTiff(fileName, FLIM_ImgData.FLIMRaw, FLIM_ImgData.acquiredTime, i == 0, FLIM_ImgData.saveChannels);

                    if (error != 0)
                    {
                        MessageBox.Show("Problem in saving ! Page = " + i);
                        break;
                    }
                }

                //GotoPage(i);
                //UpdateImages(false, false, false, false);
                UpdateImages(true, realtime, focusing, true);
                Refresh();
            }
        }

        public void AlignFrames()
        {
            var fileName = FLIM_ImgData.fullFileName;
            bool saveAsOmeTiff = ShouldSaveDerivedAsOmeTiff();
            fileName = DerivedFLIMOutputFileName(fileName, "_align", null, saveAsOmeTiff);

            var fileIO = new FileIO(FLIM_ImgData.State);
            bool align5D = HasLoaded5DTimePages();
            int totalFrameCount = Math.Max(1, align5D
                ? Math.Min(FLIM_ImgData.n_pages5D, FLIM_ImgData.FLIM_Pages5D.Length)
                : FLIM_ImgData.n_pages);
            fileIO.ExpectedOmeTiffFrames = saveAsOmeTiff ? totalFrameCount : 0;

            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false));
            Application.DoEvents();

            bool stopped = false;
            bool failed = false;
            bool stoppedOutputHandled = false;
            int processedFrames = 0;
            try
            {
                int refChannel = currentChannel;
                if (align5D)
                {
                    int pageCount5D = Math.Min(FLIM_ImgData.n_pages5D, FLIM_ImgData.FLIM_Pages5D.Length);
                    int referencePage = FLIM_ImgData.currentPage5D >= 0 && FLIM_ImgData.currentPage5D < pageCount5D
                        ? FLIM_ImgData.currentPage5D
                        : 0;
                    int[] refFitRange = FLIM_ImgData.fit_range != null && refChannel >= 0 && refChannel < FLIM_ImgData.fit_range.Length
                        ? FLIM_ImgData.fit_range[refChannel]
                        : null;
                    ushort[,] referenceProject5D = Build5DAlignmentProject(FLIM_ImgData.FLIM_Pages5D[referencePage], refChannel, refFitRange);
                    if (referenceProject5D == null)
                    {
                        failed = true;
                        MessageBox.Show("Could not build 5D alignment reference projection.");
                        return;
                    }

                    var alignedPages = new List<ushort[][][,,]>();
                    var alignedTimes = new List<DateTime>();

                    for (int i = 0; i < pageCount5D; i++)
                    {
                        if (StopFileOpening)
                        {
                            stopped = true;
                            break;
                        }

                        var page5D = FLIM_ImgData.FLIM_Pages5D[i];
                        ushort[,] currentProject5D = Build5DAlignmentProject(page5D, refChannel, refFitRange);
                        if (currentProject5D == null || !SameProjectShape(referenceProject5D, currentProject5D))
                        {
                            failed = true;
                            MessageBox.Show("Could not build 5D alignment projection for page " + (i + 1));
                            break;
                        }

                        double xcorr = ImageProcessing.MatrixMeasureDrift2D_FFT(referenceProject5D, currentProject5D, out double[] xyDrift);
                        Debug.WriteLine("X, Y drift = {0}, {1}, Confidence = {2}", xyDrift[0], xyDrift[1], xcorr);

                        var correctedPage = CorrectDrift5DPage(page5D, xyDrift);
                        if (correctedPage == null)
                        {
                            failed = true;
                            MessageBox.Show("Could not correct 5D page " + (i + 1));
                            break;
                        }

                        DateTime acquiredTime = FLIM_ImgData.acquiredTime_Pages5D != null && i < FLIM_ImgData.acquiredTime_Pages5D.Length
                            ? FLIM_ImgData.acquiredTime_Pages5D[i]
                            : FLIM_ImgData.acquiredTime;
                        alignedPages.Add(correctedPage);
                        alignedTimes.Add(acquiredTime);
                        processedFrames++;
                        Application.DoEvents();
                    }

                    stopped = stopped || StopFileOpening;

                    if (!failed && alignedPages.Count > 0)
                    {
                        bool zStackPages = HasZStack5DPages();
                        int zSlices = zStackPages ? GetMaxZSlicesFrom5DPages(alignedPages) : 0;
                        ReplaceCurrentPages5DWithDerivedOutput(fileName, alignedPages, alignedTimes,
                            processedFrames, saveAsOmeTiff, zSlices, zStackPages);
                        SaveCurrentDerivedOutputIgnoringStop(fileName);

                        if (stopped)
                        {
                            ApplyStoppedDerivedOutput(fileName, processedFrames, saveAsOmeTiff, false, true,
                                String.Format("Alignment stopped after {0} frame(s).", processedFrames));
                            stoppedOutputHandled = true;
                        }
                    }

                    return;
                }

                ushort[,] saveProject = FLIM_ImgData.Project[refChannel];
                for (int i = 0; i < totalFrameCount; i++)
                {
                    if (StopFileOpening)
                    {
                        stopped = true;
                        break;
                    }

                    GotoPage4D(i);

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

                    if (FLIM_ImgData.KeepPagesInMemory)
                    {
                        FLIM_ImgData.addCurrentFLIMRawToPage4D(true, i, true);
                    }
                    else
                    {
                        int saveError = 0;
                        SaveWithDerivedHeader(fileIO, fileName, totalFrameCount, saveAsOmeTiff, () =>
                        {
                            saveError = fileIO.SaveFLIMInTiff(fileName, FLIM_ImgData.FLIMRaw, FLIM_ImgData.acquiredTime, i == 0, FLIM_ImgData.saveChannels);
                        });

                        if (saveError != 0)
                        {
                            failed = true;
                            MessageBox.Show("Problem in saving ! Page = " + i);
                            break;
                        }
                    }

                    processedFrames++;
                    UpdateImages(false, false, false, false);
                    Refresh();
                    Application.DoEvents();
                }

                stopped = stopped || StopFileOpening;

                if (stopped && !failed && processedFrames > 0)
                {
                    fileIO.CloseAllFlimWriters();
                    if (FLIM_ImgData.KeepPagesInMemory)
                    {
                        FLIM_ImgData.resizePage(processedFrames);
                        SaveCurrentDerivedOutputIgnoringStop(fileName);
                        ApplyStoppedDerivedOutput(fileName, processedFrames, saveAsOmeTiff, false, false,
                            String.Format("Alignment stopped after {0} frame(s).", processedFrames));
                    }
                    else
                    {
                        ApplyStoppedDerivedOutput(fileName, processedFrames, saveAsOmeTiff, true, false,
                            String.Format("Alignment stopped after {0} frame(s).", processedFrames));
                    }
                    stoppedOutputHandled = true;
                }
                else if (!stopped && !failed && !FLIM_ImgData.KeepPagesInMemory)
                {
                    fileIO.CloseAllFlimWriters();
                    OpenFLIM(fileName, true, plot_regular.calc_upon_open, true);
                }
                else if (!stopped && !failed)
                {
                    SaveFLIM(false, fileName, false);
                    ApplyOutputFileToCurrentData(fileName, totalFrameCount, saveAsOmeTiff);
                    GotoPage4D(0);
                    UpdateImages(true, false, false, true, true);
                }
            }
            finally
            {
                fileIO.CloseAllFlimWriters();
                if (stopped && !stoppedOutputHandled)
                    flimage?.WriteStatusText("Alignment canceled.");
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));
            }

        }

        // Kengo BEGIN 06-02-2025
        // add a function for a remote command that can translate images by shift values
        public void TranslateFrames(double[] values)
        {
            if (values.Length != 2 && values.Length != 2 * FLIM_ImgData.n_pages)
                return;

            for (int i = 0; i < FLIM_ImgData.n_pages; i++)
            {
                GotoPage4D(i);

                double[] xyShift = values.Length == 2 ? new double[] { -values[0], -values[1] } : new double[] { -values[2 * i], -values[2 * i + 1] };
                var new_flim = new ushort[FLIM_ImgData.nChannels][,,];
                for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                {
                    new_flim[ch] = ImageProcessing.MatrixCorrectDriftFLIM(FLIM_ImgData.FLIMRaw[ch], xyShift);

                }
                FLIM_ImgData.LoadFLIMRawFromData4D(new_flim, FLIM_ImgData.acquiredTime, false);
                FLIM_ImgData.calculateAll();
                FLIM_ImgData.addCurrentFLIMRawToPage4D(true, i, true);

                UpdateImages(false, false, false, false);
                this.Refresh();
            }

        }
        // Kengo END

        private void readPhotonFiles(string filename, bool TuningOnStopOpeningButton)
        {
            flimage.flimage_io.initializePhotonReading();

            var state1 = FLIM_ImgData.State;

            photon_file_handle = new PhotonFileHandle(filename, state1);
            bool success = photon_file_handle.PhotonFile_OpenFileHeader(out state1);
            if (!success)
                state1 = flimage.flimage_io.State;

            flimage.flimage_io.State = state1;
            flimage.flimage_io.State.Acq.photon_file_format = true;
            flimage.flimage_io.SetupFLIMParameters(state1);
            FLIM_ImgData.InitializeData(state1, true);
            if (flimage != null && flimage.KeepPagesInMemoryCheck != null)
                FLIM_ImgData.KeepPagesInMemory = flimage.KeepPagesInMemoryCheck.Checked;

            var filename1 = Path.GetFileNameWithoutExtension(filename);
            var fileN = filename1.Substring(filename1.Length - 3);
            if (int.TryParse(fileN, out int fileCount))
            {
                FLIM_ImgData.fileCounter = fileCount;
                FLIM_ImgData.baseName = filename1.Substring(0, filename1.Length - 3);
            }

            FLIM_ImgData.pathName = Path.GetDirectoryName(filename);

            FLIM_ImgData.fileExtension = FLIM_ImgData.State.Files.extension;
            FLIM_ImgData.fullFileName = FLIM_ImgData.fullName(currentChannel, false);
            FLIM_ImgData.photon_file_path = filename;
            photon_filename = filename;

            this.InvokeIfRequired(o => o.UpdateFileName());
            flimage.flimage_io.StartGrab(false);

            if (TuningOnStopOpeningButton)
            {
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false));
            }

            //if (TuningOnStopOpeningButton)
            //    this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));
        }

        private void openFLIMImageInNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScanParameters State = new ScanParameters();
            FLIMData flimdata = new FLIMData(State);
            Image_Display image_display2 = new Image_Display(flimdata, flimage, false);


            flimdata.KeepPagesInMemory = flimage.KeepPagesInMemoryCheck.Checked;
            FileIO.FileError file_error = FileIO.SetupFLIMOpeningDlog(FLIM_ImgData.pathName, out int nPages, out String fileName);

            fileformat = FileIO.FileFormat.None;
            if (file_error == FileIO.FileError.TextFile)
            {
                fileformat = FileIO.FileFormat.Photon_file;
                file_error = FileIO.FileError.Success;
            }


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
            Activate();
        }


        private int CheckFolder(FileIO fileIO, bool openFolderIntensity, bool openFolderFLIM)
        {
            String intensityPath = Path.Combine(FLIM_ImgData.pathName, "Intensity");
            String FLIMPath = Path.Combine(FLIM_ImgData.pathName, "FLIM");

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

            if (!Directory.Exists(intensityPath))
                Directory.CreateDirectory(intensityPath);

            if (openFolderIntensity)
                Process.Start(intensityPath);

            if (FLIM_ImgData.FLIM_on.Any(x => x == true))
            {
                if (!Directory.Exists(FLIMPath))
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
            var exp = new FLIMage.Dialogs.ExportForm(this, FileFormat.ImageFormat.TIFF);
            exp.Show();
        }

        private void exportMovieAVIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var exp = new FLIMage.Dialogs.ExportForm(this, FileFormat.ImageFormat.AVI);
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

        private void exportFiberPhotometryCsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsFiberPhotometryFile())
                {
                    MessageBox.Show("This export is only available for FiberPhotometry (1x1) data.");
                    return;
                }

                string flimPath = FLIM_ImgData?.fullFileName;
                if (string.IsNullOrWhiteSpace(flimPath) || !File.Exists(flimPath))
                {
                    MessageBox.Show("Could not find the source .flim file on disk.");
                    return;
                }

                string baseDir = Path.GetDirectoryName(flimPath);
                string baseName = Path.GetFileNameWithoutExtension(flimPath);
                string defaultCsv = Path.Combine(baseDir, baseName + "_FiberPhotometry.csv");

                using (var sfd = new SaveFileDialog())
                {
                    sfd.InitialDirectory = baseDir;
                    sfd.FileName = Path.GetFileName(defaultCsv);
                    sfd.Filter = "CSV file (*.csv)|*.csv|All files (*.*)|*.*";
                    sfd.FilterIndex = 1;
                    sfd.RestoreDirectory = false;
                    if (sfd.ShowDialog() != DialogResult.OK)
                        return;

                    ExportFiberPhotometryCsvFromFlim(flimPath, sfd.FileName, currentChannel);
                    MessageBox.Show("Exported: " + sfd.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to export CSV: " + ex.Message);
            }
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
            //Kengo BEGIN 12-5-2023
            //String fileName = FLIM_ImgData.fileName.Split('.')[0] + extension;
            String fileName = Path.GetFileNameWithoutExtension(FLIM_ImgData.fileName) + extension;
            //Kengo END

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

        public void SaveCurrentIntensityImage(bool[] saveFormat, bool[] saveChannels, int[] FastZFormat, bool correctT0Each, bool openprocCtrl, ImageFormat image_format)
        {

            int channel_saved = currentChannel;

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

            string dir_pathname = FLIM_ImgData.pathName;
            string basename = FLIM_ImgData.baseName;

            if (fNum == 0)
                FLIM_ImgData.numberedFile = false;

            if (!Directory.Exists(dir_pathname))
                dir_pathname = FLIM_ImgData.State.Files.pathName;

            if (!Directory.Exists(dir_pathname))
                dir_pathname = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FLIMage\\Data";

            for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
            {
                if (saveChannels[ch])
                {
                    this.InvokeIfRequired(o => o.ChannelHandling(ch));

                    string ext1 = ".tif";
                    if (image_format == ImageFormat.AVI)
                        ext1 = ".avi";

                    String filePathIntensityMax = fileIO.FLIM_FilePath(ch, true, fNum, FileIO.ImageType.Intensity, procType, dir_pathname, basename, ext1);
                    String filePathFLIMMax = fileIO.FLIM_FilePath(ch, true, fNum, FileIO.ImageType.FLIM_color, procType, dir_pathname, basename, ext1);

                    String filePathIntensity = fileIO.FLIM_FilePath(ch, true, fNum, FileIO.ImageType.Intensity, "", dir_pathname, basename, ext1);
                    String filePathFLIM = fileIO.FLIM_FilePath(ch, true, fNum, FileIO.ImageType.FLIM_color, "", dir_pathname, basename, ext1);

                    bool[] saveCh = new bool[FLIM_ImgData.nChannels];
                    saveCh[ch] = true;
                    ushort[,] imageStich;
                    ushort[,] imageStichMax;
                    Bitmap bitmapFLIM = null;
                    Bitmap bitmapFLIM_Max = null;


                    AVIWriter avi_writer_intensity = new AVIWriter(filePathIntensity);
                    AVIWriter avi_writer_FLIM = new AVIWriter(filePathFLIM);

                    int npages = FastZStack ? FLIM_ImgData.n_pages5D : 1;

                    Bitmap flim_bitmap_for_save = null;
                    Bitmap intensity_bitmap_for_save = null;

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
                                        flim_bitmap_for_save = ImageProcessing.FormatImageFLIM(State_FLIM_intensity_range[ch], State_FLIM_lifetime_range[ch], thresholdFLIM_low_high[ch], FLIM_ImgData.LifetimeMapF[ch], FLIM_ImgData.ProjectF[ch], false, color_scheme);
                                        if (flim_bitmap_for_save != null)
                                            using (Graphics g = Graphics.FromImage(bitmapFLIM))
                                            {
                                                g.DrawImage(flim_bitmap_for_save, colm * width, row * height);
                                            }
                                    }
                                }

                                bool overwrite = p == 0;
                                if (image_format == ImageFormat.TIFF)
                                {
                                    fileIO.Save2DImageInTiff(filePathIntensity, imageStich, FLIM_ImgData.acquiredTime, overwrite, saveCh);
                                }
                                else
                                {
                                    intensity_bitmap_for_save = ImageProcessing.FormatImage(State_FLIM_intensity_range[ch], thresholdFLIM_low_high[ch], imageStich);

                                    if (!avi_writer_intensity.setup_done && intensity_bitmap_for_save != null)
                                        avi_writer_intensity.StreamSetup(intensity_bitmap_for_save, 10);

                                    if (intensity_bitmap_for_save != null)
                                        avi_writer_intensity.WriteOneFrame(intensity_bitmap_for_save);
                                }

                                if (FLIM_ImgData.FLIM_on[ch])
                                {
                                    if (image_format == ImageFormat.TIFF)
                                    {
                                        fileIO.SaveColorImageInTiff(filePathFLIM, bitmapFLIM, FLIM_ImgData.acquiredTime, overwrite, saveCh);
                                    }
                                    else
                                    {
                                        if (!avi_writer_FLIM.setup_done && flim_bitmap_for_save != null)
                                            avi_writer_FLIM.StreamSetup(flim_bitmap_for_save, 10);

                                        if (flim_bitmap_for_save != null)
                                            avi_writer_FLIM.WriteOneFrame(flim_bitmap_for_save);
                                    }
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

                                    if (image_format == ImageFormat.TIFF)
                                    {
                                        fileIO.Save2DImageInTiff(filePathIntensity, FLIM_ImgData.Project[ch], FLIM_ImgData.acquiredTime, overwrite, saveCh);
                                    }
                                    else
                                    {
                                        intensity_bitmap_for_save = ImageProcessing.FormatImage(State_intensity_range[ch], thresholdFLIM_low_high[ch], FLIM_ImgData.Project[ch]);

                                        if (!avi_writer_intensity.setup_done && intensity_bitmap_for_save != null)
                                            avi_writer_intensity.StreamSetup(intensity_bitmap_for_save, 10);

                                        if (intensity_bitmap_for_save != null)
                                            avi_writer_intensity.WriteOneFrame(intensity_bitmap_for_save);
                                    }

                                    if (FLIM_ImgData.FLIM_on[ch])
                                    {
                                        flim_bitmap_for_save = ImageProcessing.FormatImageFLIM(State_FLIM_intensity_range[ch], State_FLIM_lifetime_range[ch], thresholdFLIM_low_high[ch], FLIM_ImgData.LifetimeMapF[ch], FLIM_ImgData.ProjectF[ch], false, color_scheme);

                                        if (image_format == ImageFormat.TIFF)
                                        {
                                            fileIO.SaveColorImageInTiff(filePathFLIM, flim_bitmap_for_save, FLIM_ImgData.acquiredTime, overwrite, saveCh);
                                        }
                                        else
                                        {
                                            if (!avi_writer_FLIM.setup_done && flim_bitmap_for_save != null)
                                                avi_writer_FLIM.StreamSetup(flim_bitmap_for_save, 10);

                                            if (flim_bitmap_for_save != null)
                                                avi_writer_FLIM.WriteOneFrame(flim_bitmap_for_save);
                                        }
                                    }

                                    if (StopFileOpening && !FastZStack)
                                        break;
                                }
                            }
                        } //Savenormal

                        if (saveZProjection && image_format != ImageFormat.AVI)
                        {
                            calcZProjection();
                            UpdateImages(true, false, false, false, true);
                            imageStichMax = FLIM_ImgData.ProjectF[ch];

                            if (FLIM_ImgData.FLIM_on[ch])
                            {
                                bitmapFLIM_Max = ImageProcessing.FormatImageFLIM(State_FLIM_intensity_range[ch], State_FLIM_lifetime_range[ch], thresholdFLIM_low_high[ch], FLIM_ImgData.LifetimeMapF[ch], FLIM_ImgData.ProjectF[ch], false, color_scheme);
                            }


                            bool overwrite = p == 0;
                            fileIO.Save2DImageInTiff(filePathIntensityMax, imageStichMax, FLIM_ImgData.acquiredTime, overwrite, saveCh);

                            if (FLIM_ImgData.FLIM_on[ch])
                            {
                                int s = fileIO.SaveColorImageInTiff(filePathFLIMMax, bitmapFLIM_Max, FLIM_ImgData.acquiredTime, overwrite, saveCh); //Save one page.

                                if (s != -1)
                                    break;
                            }
                        } //if saveProjection.

                        if (StopFileOpening)
                            break;
                    } //page p

                    avi_writer_intensity.FinishWriting();
                    avi_writer_FLIM.FinishWriting();

                } //if saveChannel = true
            } //channel ch

            this.InvokeIfRequired(o => o.SetChannel(channel_saved + 1));

            if (openprocCtrl)
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));


        } //function

        private void saveFLIMImageToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew((Action)delegate
            {
                SaveFLIM(true, null, false);
            });
        }

        private string OmeTiffExtension()
        {
            string extension = FLIM_ImgData?.State?.Files?.extension_ome;
            if (String.IsNullOrWhiteSpace(extension))
                extension = ".btf";
            if (!extension.StartsWith("."))
                extension = "." + extension;
            return extension;
        }

        private bool IsOmeTiffFileName(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName))
                return false;

            string extension = Path.GetExtension(fileName);
            return extension.Equals(".btf", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(OmeTiffExtension(), StringComparison.OrdinalIgnoreCase);
        }

        private bool CurrentFileIsOmeTiff()
        {
            return IsOmeTiffFileName(FLIM_ImgData?.fullFileName)
                || (FLIM_ImgData?.State?.Files?.useOmeTiff ?? false);
        }

        private bool ShouldSaveDerivedAsOmeTiff(string outputFileName = null)
        {
            return CurrentFileIsOmeTiff() || IsOmeTiffFileName(outputFileName);
        }

        private string NormalizeOmeTiffOutputFileName(string fileName, bool useOmeTiff)
        {
            if (!useOmeTiff || String.IsNullOrWhiteSpace(fileName))
                return fileName;

            if (IsOmeTiffFileName(fileName))
                return fileName;

            return Path.ChangeExtension(fileName, OmeTiffExtension());
        }

        private string DerivedFLIMOutputFileName(string sourceFileName, string suffix, string outputFileName, bool useOmeTiff)
        {
            string dirName = Path.GetDirectoryName(sourceFileName) ?? "";
            string extension = useOmeTiff ? OmeTiffExtension() : ".flim";

            if (String.IsNullOrWhiteSpace(outputFileName))
            {
                string fileName = Path.GetFileNameWithoutExtension(sourceFileName) + suffix + extension;
                return Path.Combine(dirName, fileName);
            }

            outputFileName = outputFileName.Trim();
            if (String.IsNullOrEmpty(Path.GetDirectoryName(outputFileName)))
                outputFileName = Path.Combine(dirName, outputFileName);

            return NormalizeOmeTiffOutputFileName(outputFileName, useOmeTiff);
        }

        private sealed class DerivedStateSnapshot
        {
            public ScanParameters State;
            public int NFrames;
            public bool ZStack;
            public int NSlices;
            public bool FastZScan;
            public int FastZNSlices;
            public string BaseName;
            public string PathName;
            public string FileName;
            public bool NumberedFile;
            public bool ChannelsInSeparatedFile;
            public bool UseOmeTiff;
            public int FileChannel;
            public int FileCounter;
            public string Extension;
            public string ExtensionOme;
        }

        private static DerivedStateSnapshot CaptureDerivedStateSnapshot(ScanParameters state)
        {
            if (state == null)
                return null;

            return new DerivedStateSnapshot
            {
                State = state,
                NFrames = state.Acq.nFrames,
                ZStack = state.Acq.ZStack,
                NSlices = state.Acq.nSlices,
                FastZScan = state.Acq.fastZScan,
                FastZNSlices = state.Acq.FastZ_nSlices,
                BaseName = state.Files.baseName,
                PathName = state.Files.pathName,
                FileName = state.Files.fileName,
                NumberedFile = state.Files.numberedFile,
                ChannelsInSeparatedFile = state.Files.channelsInSeparatedFile,
                UseOmeTiff = state.Files.useOmeTiff,
                FileChannel = state.Files.fileChannel,
                FileCounter = state.Files.fileCounter,
                Extension = state.Files.extension,
                ExtensionOme = state.Files.extension_ome,
            };
        }

        private static void RestoreDerivedStateSnapshot(DerivedStateSnapshot snapshot)
        {
            if (snapshot == null || snapshot.State == null)
                return;

            var state = snapshot.State;
            state.Acq.nFrames = snapshot.NFrames;
            state.Acq.ZStack = snapshot.ZStack;
            state.Acq.nSlices = snapshot.NSlices;
            state.Acq.fastZScan = snapshot.FastZScan;
            state.Acq.FastZ_nSlices = snapshot.FastZNSlices;
            state.Files.baseName = snapshot.BaseName;
            state.Files.pathName = snapshot.PathName;
            state.Files.fileName = snapshot.FileName;
            state.Files.numberedFile = snapshot.NumberedFile;
            state.Files.channelsInSeparatedFile = snapshot.ChannelsInSeparatedFile;
            state.Files.useOmeTiff = snapshot.UseOmeTiff;
            state.Files.fileChannel = snapshot.FileChannel;
            state.Files.fileCounter = snapshot.FileCounter;
            state.Files.extension = snapshot.Extension;
            state.Files.extension_ome = snapshot.ExtensionOme;
        }

        private static void ParseFileIdentity(string fullFileName, out string pathName, out string fileNameWithExtension,
            out string fileNameWithoutExtension, out string extension, out string baseName, out int fileCounter,
            out bool numberedFile, out bool channelsInSeparatedFile, out int fileChannel)
        {
            pathName = Path.GetDirectoryName(fullFileName) ?? "";
            fileNameWithExtension = Path.GetFileName(fullFileName);
            fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFileName);
            extension = Path.GetExtension(fullFileName);
            baseName = fileNameWithoutExtension;
            fileCounter = 0;
            numberedFile = false;
            channelsInSeparatedFile = false;
            fileChannel = 0;

            int parsedCounter;
            if (fileNameWithoutExtension.Length >= 3
                && Int32.TryParse(fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 3), out parsedCounter))
            {
                numberedFile = true;
                fileCounter = parsedCounter;

                int channel;
                if (fileNameWithoutExtension.Length >= 8
                    && fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 4, 1) == "_"
                    && fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 8, 3) == "_Ch"
                    && Int32.TryParse(fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 5, 1), out channel))
                {
                    fileChannel = channel;
                    channelsInSeparatedFile = true;
                    baseName = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - 8);
                }
                else
                {
                    baseName = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - 3);
                }
            }
            else if (fileNameWithoutExtension.Length >= 4)
            {
                int channel;
                if (fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 4, 3) == "_Ch"
                    && Int32.TryParse(fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 1, 1), out channel))
                {
                    fileChannel = channel;
                    channelsInSeparatedFile = true;
                    baseName = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - 4);
                }
            }
        }

        private void ApplyOutputFileToState(ScanParameters state, string outputFileName, bool useOmeTiff)
        {
            if (state == null || state.Files == null || String.IsNullOrWhiteSpace(outputFileName))
                return;

            ParseFileIdentity(outputFileName, out string pathName, out _, out string fileNameWithoutExtension,
                out string extension, out string baseName, out int fileCounter, out bool numberedFile,
                out bool channelsInSeparatedFile, out int fileChannel);

            state.Files.pathName = pathName;
            state.Files.fileName = fileNameWithoutExtension;
            state.Files.extension = extension;
            state.Files.baseName = baseName;
            state.Files.fileCounter = fileCounter;
            state.Files.numberedFile = numberedFile;
            state.Files.channelsInSeparatedFile = channelsInSeparatedFile;
            state.Files.fileChannel = fileChannel;
            state.Files.useOmeTiff = useOmeTiff;
            if (useOmeTiff)
                state.Files.extension_ome = extension;
        }

        private void ApplyOutputFileToCurrentData(string outputFileName, int frameCount, bool useOmeTiff, bool? zStack = null)
        {
            if (FLIM_ImgData == null || String.IsNullOrWhiteSpace(outputFileName))
                return;

            ParseFileIdentity(outputFileName, out string pathName, out string fileNameWithExtension, out _,
                out string extension, out string baseName, out int fileCounter, out bool numberedFile,
                out _, out _);

            FLIM_ImgData.pathName = pathName;
            FLIM_ImgData.baseName = baseName;
            FLIM_ImgData.fileCounter = fileCounter;
            FLIM_ImgData.numberedFile = numberedFile;
            FLIM_ImgData.fileName = fileNameWithExtension;
            FLIM_ImgData.fileExtension = extension;
            FLIM_ImgData.fullFileName = outputFileName;

            ApplyOutputFileToState(FLIM_ImgData.State, outputFileName, useOmeTiff);
            if (FLIM_ImgData.State != null && frameCount > 0)
                FLIM_ImgData.State.Acq.nFrames = frameCount;
            if (frameCount > 0)
                FLIM_ImgData.nFrames = frameCount;
            if (zStack.HasValue && FLIM_ImgData.State != null)
            {
                FLIM_ImgData.ZStack = zStack.Value;
                if (zStack.Value)
                    FLIM_ImgData.State.Acq.ZStack = true;
                else
                {
                    FLIM_ImgData.nFastZ = 1;
                    SetStateAsSinglePlaneTimeCourse(FLIM_ImgData.State);
                }
            }

            this.BeginInvokeIfRequired(o =>
            {
                o.UpdateFileName();
                o.PageEnableControl();
            });
        }

        private static void SetStateAsSinglePlaneTimeCourse(ScanParameters state)
        {
            if (state == null)
                return;

            state.Acq.ZStack = false;
            state.Acq.nSlices = 1;
            state.Acq.fastZScan = false;
            state.Acq.FastZ_nSlices = 1;
        }

        private static void SetStateAsZStackTimeCourse(ScanParameters state, int zSlices, int frameCount)
        {
            if (state == null)
                return;

            state.Acq.ZStack = true;
            state.Acq.nSlices = Math.Max(1, zSlices);
            if (state.Acq.sliceStep == 0)
                state.Acq.sliceStep = 1;
            state.Acq.fastZScan = false;
            state.Acq.FastZ_nSlices = 1;
            if (frameCount > 0)
                state.Acq.nFrames = frameCount;
        }

        private static int GetZSlicesFrom5DPage(ushort[][][,,] page)
        {
            if (page == null)
                return 1;

            for (int ch = 0; ch < page.Length; ch++)
            {
                if (page[ch] != null && page[ch].Length > 0)
                    return page[ch].Length;
            }

            return 1;
        }

        private int GetCurrent5DZSliceCount()
        {
            if (FLIM_ImgData == null)
                return 1;

            int zSlices = GetZSlicesFrom5DPage(FLIM_ImgData.FLIMRaw5D);
            if (zSlices <= 1 && FLIM_ImgData.imagesPerFile > 1)
                zSlices = FLIM_ImgData.imagesPerFile;
            if (zSlices <= 1 && FLIM_ImgData.n_pages > 1)
                zSlices = FLIM_ImgData.n_pages;

            return Math.Max(1, zSlices);
        }

        private bool HasLoaded5DTimePages()
        {
            if (FLIM_ImgData == null
                || !FLIM_ImgData.KeepPagesInMemory
                || FLIM_ImgData.FLIM_Pages5D == null
                || FLIM_ImgData.n_pages5D <= 0
                || !(FLIM_ImgData.nFastZ > 1 || HasZStack5DPages()))
            {
                return false;
            }

            int pageCount = Math.Min(FLIM_ImgData.n_pages5D, FLIM_ImgData.FLIM_Pages5D.Length);
            for (int page = 0; page < pageCount; page++)
            {
                if (FLIM_ImgData.FLIM_Pages5D[page] != null)
                    return true;
            }

            return false;
        }

        private static bool SamePlaneShape(ushort[,,] a, ushort[,,] b)
        {
            if (a == null || b == null)
                return false;

            return a.GetLength(0) == b.GetLength(0)
                && a.GetLength(1) == b.GetLength(1)
                && a.GetLength(2) == b.GetLength(2);
        }

        private static bool SameProjectShape(ushort[,] a, ushort[,] b)
        {
            if (a == null || b == null)
                return false;

            return a.GetLength(0) == b.GetLength(0)
                && a.GetLength(1) == b.GetLength(1);
        }

        private static int GetMaxZSlicesFrom5DPages(IList<ushort[][][,,]> pages)
        {
            if (pages == null || pages.Count == 0)
                return 1;

            int zSlices = 1;
            foreach (var page in pages)
                zSlices = Math.Max(zSlices, GetZSlicesFrom5DPage(page));
            return zSlices;
        }

        private bool TryBuildBinned5DPage(int startPage, int binning, out ushort[][][,,] binnedPage, out string errorMessage)
        {
            binnedPage = null;
            errorMessage = "";

            if (FLIM_ImgData?.FLIM_Pages5D == null)
            {
                errorMessage = "No 5D pages are loaded.";
                return false;
            }

            int pageCount = Math.Min(FLIM_ImgData.n_pages5D, FLIM_ImgData.FLIM_Pages5D.Length);
            if (startPage < 0 || startPage + binning > pageCount)
            {
                errorMessage = "The requested 5D bin range is outside the loaded pages.";
                return false;
            }

            int nChannels = Math.Max(FLIM_ImgData.nChannels, 1);
            for (int page = startPage; page < startPage + binning; page++)
            {
                var page5D = FLIM_ImgData.FLIM_Pages5D[page];
                if (page5D == null)
                {
                    errorMessage = "A 5D page is missing at page " + (page + 1);
                    return false;
                }
                nChannels = Math.Max(nChannels, page5D.Length);
            }

            int zSlices = 1;
            for (int page = startPage; page < startPage + binning; page++)
                zSlices = Math.Max(zSlices, GetZSlicesFrom5DPage(FLIM_ImgData.FLIM_Pages5D[page]));

            binnedPage = new ushort[nChannels][][,,];
            for (int ch = 0; ch < nChannels; ch++)
            {
                binnedPage[ch] = new ushort[zSlices][,,];
                for (int z = 0; z < zSlices; z++)
                {
                    ushort[,,] sum = null;
                    for (int page = startPage; page < startPage + binning; page++)
                    {
                        var page5D = FLIM_ImgData.FLIM_Pages5D[page];
                        if (page5D == null
                            || ch >= page5D.Length
                            || page5D[ch] == null
                            || z >= page5D[ch].Length
                            || page5D[ch][z] == null)
                        {
                            continue;
                        }

                        var plane = page5D[ch][z];
                        if (sum == null)
                        {
                            sum = (ushort[,,])plane.Clone();
                        }
                        else
                        {
                            if (!SamePlaneShape(sum, plane))
                            {
                                errorMessage = String.Format("5D page shape changed while binning page {0}, channel {1}, Z {2}.", page + 1, ch + 1, z + 1);
                                return false;
                            }

                            MatrixCalc.ArrayCalc(sum, plane, CalculationType.Add);
                        }
                    }

                    binnedPage[ch][z] = sum;
                }
            }

            return true;
        }

        private static ushort[,] Build5DAlignmentProject(ushort[][][,,] page5D, int refChannel, int[] fitRange)
        {
            if (page5D == null || refChannel < 0 || refChannel >= page5D.Length || page5D[refChannel] == null)
                return null;

            ushort[,] project = null;
            for (int z = 0; z < page5D[refChannel].Length; z++)
            {
                var plane = page5D[refChannel][z];
                if (plane == null)
                    continue;

                int[] range = fitRange != null ? (int[])fitRange.Clone() : null;
                var zProject = ImageProcessing.GetProjectFromFLIM(plane, range);
                if (zProject == null)
                    continue;

                if (project == null)
                {
                    project = (ushort[,])zProject.Clone();
                }
                else if (SameProjectShape(project, zProject))
                {
                    project = MatrixCalc.MatrixCalc2D(project, zProject, CalculationType.Add, true);
                }
            }

            return project;
        }

        private static ushort[][][,,] CorrectDrift5DPage(ushort[][][,,] page5D, double[] xyDrift)
        {
            if (page5D == null)
                return null;

            var corrected = new ushort[page5D.Length][][,,];
            for (int ch = 0; ch < page5D.Length; ch++)
            {
                if (page5D[ch] == null)
                    continue;

                corrected[ch] = new ushort[page5D[ch].Length][,,];
                for (int z = 0; z < page5D[ch].Length; z++)
                {
                    if (page5D[ch][z] != null)
                        corrected[ch][z] = ImageProcessing.MatrixCorrectDriftFLIM(page5D[ch][z], xyDrift);
                }
            }

            return corrected;
        }

        private void SaveWithDerivedHeader(FileIO fileIO, string outputFileName, int totalFrames, bool useOmeTiff, Action saveAction, int expectedOmeTiffFrames = 0)
        {
            if (saveAction == null)
                return;

            var states = new List<ScanParameters>();
            if (FLIM_ImgData?.State != null)
                states.Add(FLIM_ImgData.State);
            if (fileIO?.State != null && !states.Any(state => Object.ReferenceEquals(state, fileIO.State)))
                states.Add(fileIO.State);

            var snapshots = states.Select(CaptureDerivedStateSnapshot).ToList();
            int saveExpectedOmeFrames = fileIO?.ExpectedOmeTiffFrames ?? 0;

            try
            {
                foreach (var state in states)
                {
                    ApplyOutputFileToState(state, outputFileName, useOmeTiff);
                    if (totalFrames > 0)
                        state.Acq.nFrames = totalFrames;
                }
                int omeFrameCount = expectedOmeTiffFrames > 0 ? expectedOmeTiffFrames : totalFrames;
                if (fileIO != null && omeFrameCount > 0)
                    fileIO.ExpectedOmeTiffFrames = omeFrameCount;

                SaveWithTemporaryOmeTiff(fileIO, useOmeTiff, saveAction);
            }
            finally
            {
                foreach (var snapshot in snapshots)
                    RestoreDerivedStateSnapshot(snapshot);
                if (fileIO != null)
                    fileIO.ExpectedOmeTiffFrames = saveExpectedOmeFrames;
            }
        }

        private void ReplaceCurrentPagesWithDerivedOutput(string outputFileName, IList<ushort[][,,]> pages,
            IList<DateTime> acquiredTimes, int frameCount, bool useOmeTiff)
        {
            if (FLIM_ImgData == null || pages == null || pages.Count == 0)
                return;

            FLIM_ImgData.clearPages4D();
            FLIM_ImgData.KeepPagesInMemory = true;
            FLIM_ImgData.ZStack = false;
            FLIM_ImgData.nFastZ = 1;
            SetStateAsSinglePlaneTimeCourse(FLIM_ImgData.State);

            for (int i = 0; i < pages.Count; i++)
            {
                DateTime acquiredTime = acquiredTimes != null && i < acquiredTimes.Count ? acquiredTimes[i] : DateTime.Now;
                FLIM_ImgData.LoadFLIMRawFromData4D(pages[i], acquiredTime, false);
                FLIM_ImgData.addCurrentFLIMRawToPage4D(false, i, false);
            }

            FLIM_ImgData.resizePage(pages.Count);
            ApplyOutputFileToCurrentData(outputFileName, frameCount > 0 ? frameCount : pages.Count, useOmeTiff, false);
            GotoPage4D(0);
            UpdateImages(true, false, false, true, true);
        }

        private void ReplaceCurrentPages5DWithDerivedOutput(string outputFileName, IList<ushort[][][,,]> pages,
            IList<DateTime> acquiredTimes, int frameCount, bool useOmeTiff, int zSlices = 0, bool zStack = false)
        {
            if (FLIM_ImgData == null || pages == null || pages.Count == 0)
                return;

            if (zSlices <= 0)
                zSlices = GetZSlicesFrom5DPage(pages[0]);
            int currentPageZSlices = GetZSlicesFrom5DPage(pages[0]);

            FLIM_ImgData.clearPages5D();
            FLIM_ImgData.KeepPagesInMemory = true;
            if (zStack)
            {
                FLIM_ImgData.ZStack = true;
                FLIM_ImgData.nFastZ = 1;
                SetStateAsZStackTimeCourse(FLIM_ImgData.State, zSlices, frameCount > 0 ? frameCount : pages.Count);
            }

            for (int i = 0; i < pages.Count; i++)
            {
                DateTime acquiredTime = acquiredTimes != null && i < acquiredTimes.Count ? acquiredTimes[i] : DateTime.Now;
                FLIM_ImgData.Add5DFLIM(pages[i], acquiredTime, i, false);
            }

            FLIM_ImgData.resizePage5D(pages.Count);
            ApplyOutputFileToCurrentData(outputFileName, frameCount > 0 ? frameCount : pages.Count, useOmeTiff, zStack ? (bool?)true : null);
            if (zStack)
                SetStateAsZStackTimeCourse(FLIM_ImgData.State, zSlices, frameCount > 0 ? frameCount : pages.Count);
            GotoPage5D(0);
            if (zStack)
                FLIM_ImgData.ZProjection_Range[1] = Math.Max(1, currentPageZSlices);
            UpdateImages(true, false, false, true, true);
        }

        private void SaveCurrentDerivedOutputIgnoringStop(string outputFileName)
        {
            if (String.IsNullOrWhiteSpace(outputFileName))
                return;

            bool savedStopFileOpening = StopFileOpening;
            try
            {
                StopFileOpening = false;
                SaveFLIM(false, outputFileName, false);
            }
            finally
            {
                StopFileOpening = savedStopFileOpening;
            }
        }

        private void ApplyStoppedDerivedOutput(string outputFileName, int frameCount, bool useOmeTiff, bool openFromDisk, bool fastZ, string statusText)
        {
            if (String.IsNullOrWhiteSpace(outputFileName) || frameCount <= 0)
                return;

            if (openFromDisk)
            {
                OpenFLIM(outputFileName, true, plot_regular.calc_upon_open, true);
                ApplyOutputFileToCurrentData(outputFileName, frameCount, useOmeTiff, fastZ ? (bool?)null : false);
            }
            else
            {
                ApplyOutputFileToCurrentData(outputFileName, frameCount, useOmeTiff, fastZ ? (bool?)null : false);
                if (fastZ)
                    GotoPage5D(0);
                else
                    GotoPage4D(0);
                UpdateImages(true, false, false, true, true);
            }

            flimage?.WriteStatusText(statusText);
            AnalysisStatus = "None";
        }

        private int GetSaveFrameCount()
        {
            if (FLIM_ImgData == null)
                return 1;

            if (FLIM_ImgData.nFastZ > 1 || HasZStack5DPages())
            {
                int timePages = FLIM_ImgData.FLIM_Pages5D?.Length ?? Math.Max(1, FLIM_ImgData.n_pages5D);
                int zPages = FLIM_ImgData.FLIM_Pages?.Length ?? Math.Max(1, FLIM_ImgData.nFastZ);
                if (zPages <= 1 && FLIM_ImgData.FLIM_Pages5D != null && FLIM_ImgData.FLIM_Pages5D.Length > 0)
                    zPages = GetZSlicesFrom5DPage(FLIM_ImgData.FLIM_Pages5D[0]);
                return Math.Max(1, timePages * zPages);
            }

            return Math.Max(1, FLIM_ImgData.FLIM_Pages?.Length ?? FLIM_ImgData.n_pages);
        }

        private int GetSaveHeaderFrameCount()
        {
            if (FLIM_ImgData == null)
                return 1;

            if (FLIM_ImgData.nFastZ > 1 || HasZStack5DPages())
                return Math.Max(1, FLIM_ImgData.FLIM_Pages5D?.Length ?? FLIM_ImgData.n_pages5D);

            return Math.Max(1, FLIM_ImgData.FLIM_Pages?.Length ?? FLIM_ImgData.n_pages);
        }

        private bool HasZStack5DPages()
        {
            if (FLIM_ImgData == null
                || !FLIM_ImgData.ZStack
                || FLIM_ImgData.n_pages5D <= 0)
            {
                return false;
            }

            if (GetZSlicesFrom5DPage(FLIM_ImgData.FLIMRaw5D) > 1)
                return true;

            if (!FLIM_ImgData.KeepPagesInMemory && FLIM_ImgData.imagesPerFile > 1)
                return true;

            if (FLIM_ImgData.FLIM_Pages5D != null)
            {
                int pageCount = Math.Min(FLIM_ImgData.n_pages5D, FLIM_ImgData.FLIM_Pages5D.Length);
                for (int page = 0; page < pageCount; page++)
                {
                    if (GetZSlicesFrom5DPage(FLIM_ImgData.FLIM_Pages5D[page]) > 1)
                        return true;
                }
            }

            return false;
        }

        private void SaveWithTemporaryOmeTiff(FileIO fileIO, bool useOmeTiff, Action saveAction)
        {
            if (saveAction == null)
                return;

            if (!useOmeTiff)
            {
                saveAction();
                return;
            }

            var states = new List<ScanParameters>();
            if (FLIM_ImgData?.State != null)
                states.Add(FLIM_ImgData.State);
            if (fileIO?.State != null && !states.Any(state => Object.ReferenceEquals(state, fileIO.State)))
                states.Add(fileIO.State);

            bool[] savedUseOme = states.Select(state => state.Files.useOmeTiff).ToArray();
            string[] savedExtension = states.Select(state => state.Files.extension).ToArray();
            string[] savedOmeExtension = states.Select(state => state.Files.extension_ome).ToArray();
            string omeExtension = OmeTiffExtension();

            try
            {
                foreach (var state in states)
                {
                    state.Files.useOmeTiff = true;
                    state.Files.extension = omeExtension;
                    state.Files.extension_ome = omeExtension;
                }

                saveAction();
            }
            finally
            {
                for (int i = 0; i < states.Count; i++)
                {
                    states[i].Files.useOmeTiff = savedUseOme[i];
                    states[i].Files.extension = savedExtension[i];
                    states[i].Files.extension_ome = savedOmeExtension[i];
                }
            }
        }

        public void SaveFLIM(bool openprocCtrl, string filename, bool append)
        {
            var append_file = append;
            var filename_no_ext = Path.GetFileNameWithoutExtension(FLIM_ImgData.fileName);
            var dirname = Path.GetDirectoryName(FLIM_ImgData.fullFileName);
            var dirname_input = Path.GetDirectoryName(filename);
            bool defaultOmeTiff = ShouldSaveDerivedAsOmeTiff(filename);

            String fileName = Path.Combine(dirname, filename_no_ext);
            fileName += defaultOmeTiff ? OmeTiffExtension() : ".flim";

            FileIO fileIO = new FileIO(FLIM_ImgData.State);

            if (filename == null)
            {
                append_file = false;
                Invoke((Action)delegate
                {
                    filename = fileIO.SaveFLIMFileDialog(fileName);
                });
            }
            else if (dirname_input == "")
            {
                filename = Path.Combine(dirname, filename);
            }

            if (openprocCtrl)
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

            if (filename != null && filename != "")
            {
                bool saveAsOmeTiff = ShouldSaveDerivedAsOmeTiff(filename);
                filename = NormalizeOmeTiffOutputFileName(filename, saveAsOmeTiff);
                fileIO.ExpectedOmeTiffFrames = saveAsOmeTiff ? GetSaveFrameCount() : 0;

                try
                {
                    SaveWithDerivedHeader(fileIO, filename, GetSaveHeaderFrameCount(), saveAsOmeTiff, () =>
                    {
                        if (FLIM_ImgData.nFastZ > 1 || HasZStack5DPages())
                        {
                            for (int i5d = 0; i5d < FLIM_ImgData.FLIM_Pages5D.Length; i5d++)
                            {
                                if (StopFileOpening)
                                    break;

                                GotoPage5D(i5d);
                                setupImageUpdateForZStack();
                                UpdateImages(false, false, false, false, false);

                                var error = fileIO.SaveFLIMInTiffZStack(filename, FLIM_ImgData.FLIM_Pages, FLIM_ImgData.acquiredTime_Pages5D[i5d], i5d == 0 && !append, FLIM_ImgData.saveChannels);
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
                                var error = fileIO.SaveFLIMInTiff(filename, FLIM_ImgData.FLIMRaw, FLIM_ImgData.acquiredTime_Pages[i], i == 0 && !append, FLIM_ImgData.saveChannels);

                                if (error != 0)
                                {
                                    MessageBox.Show("Problem in saving ! Page = " + i);
                                    break;
                                }
                            }
                        }
                    }, GetSaveFrameCount());
                }
                finally
                {
                    fileIO.CloseAllFlimWriters();
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
            // KENGO BEGIN 1-8-2026
            // Always keep the Calclate_upon_open checkbox checked before start Batch proccessing
            plot_regular.TurnOnCalcUponOpen(true);
            // KENGO END
            Task.Factory.StartNew((Action)delegate
            {
                BatchProcessing();
            });
        }

        public string[] FLIMfilesInDirectory(bool openFolders, bool photon_files)
        {
            FileIO fileIO = new FileIO(FLIM_ImgData.State);
            CheckFolder(fileIO, openFolders, openFolders);

            //DirectoryInfo dinfo = new DirectoryInfo(FLIM_ImgData.pathName);
            //String[] fileNames_flim = Directory.GetFiles(FLIM_ImgData.pathName, FLIM_ImgData.baseName + "*.flim");
            //String[] fileNames_tiff = Directory.GetFiles(FLIM_ImgData.pathName, FLIM_ImgData.baseName + "*.tif");

            //var list1 = fileNames_flim.ToList();
            //list1.AddRange(fileNames_tiff);

            String[] fileNames;

            if (photon_files)
            {
                fileNames = FileHandling.GetFiles(FLIM_ImgData.pathName, new string[] { FLIM_ImgData.baseName + "???.phtn", FLIM_ImgData.baseName + "???.photon" }).ToArray();
            }
            else
            {
                fileNames = FileHandling.GetFiles(FLIM_ImgData.pathName, new string[] { FLIM_ImgData.baseName + "???.flim", FLIM_ImgData.baseName + "???.tif", FLIM_ImgData.baseName + "???.btf" }).ToArray();
            }

            if (fileNames.Length == 0)
            {
                MessageBox.Show("No file matches to: \"" + FLIM_ImgData.pathName + "\\" + FLIM_ImgData.baseName + "???" + FLIM_ImgData.State.Files.extension + "\"");
                return null;
            }

            Array.Sort(fileNames);

            return fileNames;
        }

        public void BatchExporting(bool[] saveFormat, bool[] channelToSave, int[] FastZFormat, bool correctT0EachPage, ImageFormat image_format)
        {
            AnalysisStatus = "BatchExporting";

            if (saveFormat.All(x => x == false))
                return;
            if (channelToSave.All(x => x == false))
                return;

            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false)); //Turn on stop opening button and turn off control buttons for safety.

            string[] fileNames = FLIMfilesInDirectory(true, false);


            for (int i = 0; i < fileNames.Length; i++)
            {
                if (!StopFileOpening)
                {
                    var fn1 = Path.GetFileNameWithoutExtension(fileNames[i]);
                    var ext1 = Path.GetExtension(fn1);
                    var poton_file = ext1 == ".phtn" || ext1 == ".txt" || ext1 == ".photon";
                    if (poton_file)
                        fileformat = FileIO.FileFormat.Photon_file;
                    else
                        fileformat = FileIO.FileFormat.None;

                    if (!fn1.EndsWith("_max"))
                    {
                        OpenFLIM(fileNames[i], false, false, false);
                        //if (!plot_regular.calc_upon_open) //Not yet calculated.
                        //    CalculateTimecourse(TuningOnStopOpeningButton);

                        SaveCurrentIntensityImage(saveFormat, channelToSave, FastZFormat, correctT0EachPage, false, image_format);
                        //SaveCurrentIntensityImage();
                    }
                }
                else
                    break;
            }

            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

            AnalysisStatus = "None";
        }


        public void finish_reading_photon_file()
        {
            //photon_file_handle.FinishReading();
            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));
            autoResetPhotonFileEvent.Set();

            FLIM_ImgData.CopyDataFrom5DPage_To_4DPage();
            photon_file_handle = null;
        }

        private void convertAllPhtonFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew((Action)delegate
            {
                ConvertAllPhotonFiles();
            });
        }


        public void ConvertAllPhotonFiles()
        {
            AnalysisStatus = "ConvertAllPhotonFiles";
            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false)); //Turn on stop opening button and turn off control buttons for safety.

            string[] fileNames = FLIMfilesInDirectory(false, true);

            for (int i = 0; i < fileNames.Length; i++)
            {
                if (!StopFileOpening)
                {
                    readPhotonFiles(fileNames[i], i == 0);
                    autoResetPhotonFileEvent.WaitOne();
                    while (true)
                    {
                        if (flimage.flimage_io.read_photon_file)
                        {
                            System.Threading.Thread.Sleep(250);
                        }
                        else
                            break;
                    }
                }
                else
                    break;
            }

            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

            AnalysisStatus = "None";
        }

        public void BatchProcessing()
        {
            AnalysisStatus = "BatchProcessing";

            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false)); //Turn on stop opening button and turn off control buttons for safety.

            string[] fileNames = FLIMfilesInDirectory(false, false);
            if (fileNames.Length == 0)
                fileNames = FLIMfilesInDirectory(false, true);

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

            AnalysisStatus = "None";
        }

        private void makeMoviesWithTheSameBaseNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            concatenateFilesInDirectory(null, false);
        }

        public Task concatenateFilesInDirectory(string output_filename, bool external_command)
        {
            AnalysisStatus = "ConcatenateFiles";

            string[] fileNames = FLIMfilesInDirectory(false, false);
            var dirname = FLIM_ImgData.pathName;

            if (String.IsNullOrWhiteSpace(output_filename) && fileNames != null && fileNames.Length > 0)
            {
                bool saveAsOmeTiff = ShouldConcatenateSaveAsOmeTiff(fileNames, displayZProjection, out _, out _);
                string extension = saveAsOmeTiff ? OmeTiffExtension() : ".flim";
                string temp_output_name = Path.GetFileNameWithoutExtension(fileNames[0]) + "_concat" + extension;
                string defaultOutputName = Path.Combine(dirname, temp_output_name);
                if (external_command)
                {
                    output_filename = defaultOutputName;
                }
                else
                {
                    Invoke((Action)delegate
                    {
                        var fileIO = new FileIO(FLIM_ImgData.State);

                        output_filename = fileIO.SaveFLIMFileDialog(defaultOutputName);
                    });
                }
            }

            if (fileNames != null && fileNames.Length > 0)
            {
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false));

                var task1 = Task.Factory.StartNew((Action)delegate
                {
                    ConcatenateFiles(fileNames, output_filename, external_command);
                });

                return task1;
            }
            else
                return null;
        }

        private void UpdateConcatenateProgress(int currentFile, int totalFiles, string fileName)
        {
            UpdateConcatenateProgress(currentFile, totalFiles, 0, 0, fileName);
        }

        private void UpdateConcatenateFrameProgress(int currentFile, int totalFiles, int currentFrame, int totalFrames, string fileName)
        {
            UpdateConcatenateProgress(currentFile, totalFiles, currentFrame, totalFrames, fileName);
        }

        private void UpdateConcatenateProgress(int currentFile, int totalFiles, int currentFrame, int totalFrames, string fileName)
        {
            string folderPath;
            string fileBaseName;
            int fileNumber;
            FileIO.FileParts(fileName, out folderPath, out fileBaseName, out fileNumber);

            bool showFrameProgress = currentFrame > 0 && totalFrames > 0;
            string progressText = showFrameProgress
                ? String.Format("Concatenating file {0}/{1}, frame {2}/{3}: {4}", currentFile, totalFiles, currentFrame, totalFrames, Path.GetFileName(fileName))
                : String.Format("Concatenating file {0}/{1}: {2}", currentFile, totalFiles, Path.GetFileName(fileName));
            string fileNumberText = fileNumber >= 0 ? fileNumber.ToString() : String.Format("{0}/{1}", currentFile, totalFiles);

            this.BeginInvokeIfRequired(o =>
            {
                o.FileN.Text = fileNumberText;
                if (showFrameProgress)
                {
                    o.c_page.Text = currentFrame.ToString();
                    o.st_pageN.Text = "/ " + totalFrames;
                }
                o.Text = "FLIM Analysis: " + progressText;
            });

            flimage?.WriteStatusText(progressText);
        }

        private sealed class ConcatenateFrameInfo
        {
            public int FrameCount = 1;
            public int DirectoryCount = 1;
            public int ZSlices = 1;
            public int Width;
            public int Height;
            public int TimeBins;
            public bool IsZStack;
            public bool IsZLinear;
            public bool VariableZSlices;
            public bool Use5DConcatenation;

            public string ShapeKey
            {
                get { return String.Format("{0}x{1}x{2}", Width, Height, TimeBins); }
            }
        }

        private static int[] CountConcatenateFrames(string[] fileNames, bool saveStack)
        {
            if (fileNames == null)
                return new int[0];

            int[] frameCounts = new int[fileNames.Length];
            for (int i = 0; i < fileNames.Length; i++)
            {
                if (saveStack)
                {
                    frameCounts[i] = 1;
                    continue;
                }

                try
                {
                    if (!IsNumberedConcatenateInput(fileNames[i]))
                        frameCounts[i] = 0;
                    else if (TryGetConcatenateFrameInfo(fileNames[i], out ConcatenateFrameInfo info))
                        frameCounts[i] = Math.Max(1, info.FrameCount);
                    else
                        frameCounts[i] = 1;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Could not count frames for concatenation progress: " + ex.Message);
                    frameCounts[i] = 1;
                }
            }

            return frameCounts;
        }

        private static bool IsNumberedConcatenateInput(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName))
                return false;

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            return Regex.IsMatch(fileNameWithoutExtension ?? "", @"\d{3}$");
        }

        private static bool ShouldConcatenateAs5DZStack(string[] fileNames, bool saveStack,
            out int zSlices, out int[] frameCounts, out int totalFrames, out bool variableZSlices, out string referenceShapeKey)
        {
            zSlices = 1;
            frameCounts = fileNames == null ? new int[0] : new int[fileNames.Length];
            totalFrames = 0;
            variableZSlices = false;
            referenceShapeKey = "";

            if (saveStack || fileNames == null || fileNames.Length == 0)
                return false;

            var stackInfos = new List<Tuple<int, ConcatenateFrameInfo>>();
            var shapeFrameCounts = new Dictionary<string, long>();
            var shapeFirstIndex = new Dictionary<string, int>();
            var shapeUses5D = new Dictionary<string, bool>();

            for (int i = 0; i < fileNames.Length; i++)
            {
                if (!IsNumberedConcatenateInput(fileNames[i]))
                    continue;

                if (!TryGetConcatenateFrameInfo(fileNames[i], out ConcatenateFrameInfo info) || info.ZSlices < 1)
                    return false;

                stackInfos.Add(Tuple.Create(i, info));

                string shapeKey = info.ShapeKey;
                if (!shapeFrameCounts.ContainsKey(shapeKey))
                {
                    shapeFrameCounts[shapeKey] = 0;
                    shapeFirstIndex[shapeKey] = i;
                }
                shapeFrameCounts[shapeKey] += Math.Max(0, info.FrameCount);
                if (!shapeUses5D.ContainsKey(shapeKey))
                    shapeUses5D[shapeKey] = false;
                shapeUses5D[shapeKey] = shapeUses5D[shapeKey] || info.Use5DConcatenation;
            }

            if (stackInfos.Count == 0)
                return false;

            foreach (var entry in shapeFrameCounts)
            {
                if (!shapeUses5D.TryGetValue(entry.Key, out bool use5D) || !use5D)
                    continue;

                if (String.IsNullOrEmpty(referenceShapeKey)
                    || entry.Value > shapeFrameCounts[referenceShapeKey]
                    || (entry.Value == shapeFrameCounts[referenceShapeKey]
                        && shapeFirstIndex[entry.Key] < shapeFirstIndex[referenceShapeKey]))
                {
                    referenceShapeKey = entry.Key;
                }
            }

            if (String.IsNullOrEmpty(referenceShapeKey))
                return false;

            bool found5DInput = false;
            long totalFramesLong = 0;
            int firstIncludedZSlices = -1;
            foreach (var item in stackInfos)
            {
                int fileIndex = item.Item1;
                ConcatenateFrameInfo info = item.Item2;
                if (!String.Equals(info.ShapeKey, referenceShapeKey, StringComparison.Ordinal))
                    continue;

                int fileZSlices = Math.Max(1, info.ZSlices);
                if (info.Use5DConcatenation)
                    found5DInput = true;
                if (info.VariableZSlices)
                    variableZSlices = true;

                if (firstIncludedZSlices < 0)
                    firstIncludedZSlices = fileZSlices;
                else if (firstIncludedZSlices != fileZSlices)
                    variableZSlices = true;

                if (zSlices != fileZSlices)
                {
                    if (totalFramesLong > 0)
                        variableZSlices = true;
                    zSlices = Math.Max(zSlices, fileZSlices);
                }

                totalFramesLong += Math.Max(0, info.FrameCount);
                frameCounts[fileIndex] = Math.Max(0, info.FrameCount);
            }

            if (found5DInput)
                totalFrames = totalFramesLong > int.MaxValue ? int.MaxValue : (int)totalFramesLong;

            return found5DInput;
        }

        private static bool TryGetConcatenateFrameInfo(string fileName, out ConcatenateFrameInfo info)
        {
            info = new ConcatenateFrameInfo();

            int nPages = FileIO.SetupFLIMOpening(fileName, out string header);
            if (nPages <= 0)
                return false;

            info.DirectoryCount = nPages;
            info.FrameCount = nPages;
            info.Width = ReadHeaderInt(header, "State.Acq.pixelsPerLine", 0);
            info.Height = ReadHeaderInt(header, "State.Acq.linesPerFrame", 0);
            info.TimeBins = ReadHeaderInt(header, "State.Spc.spcData.n_dataPoint", 0);

            int headerSlices = ReadHeaderInt(header, "State.Acq.nSlices", 1);
            if (headerSlices <= 1)
                headerSlices = ReadHeaderInt(header, "State.Acq.numberOfZSlices", 1);
            bool averageFrames = HeaderAveragesFrames(header);
            int effectiveHeaderFrames = GetEffectiveHeaderFrameCount(header);

            bool headerZStack = ReadHeaderBool(header, "State.Acq.ZStack", false) && headerSlices > 1;
            string format = ReadHeaderString(header, "Format", "");
            bool formatZLinear = String.Equals(format, FileIO.FileFormat.ZLinear.ToString(), StringComparison.OrdinalIgnoreCase);

            int zLinearSlices = 1;
            bool variableZLinearSlices = false;
            if (Path.GetExtension(fileName).Equals(".btf", StringComparison.OrdinalIgnoreCase))
            {
                zLinearSlices = 1;
            }
            else if (formatZLinear)
            {
                GetClassicTiffImageLengthRange(fileName, out int minImageLength, out int maxImageLength);
                zLinearSlices = maxImageLength;
                variableZLinearSlices = minImageLength != maxImageLength;
            }

            int zSlices = Math.Max(headerSlices, zLinearSlices);
            info.IsZStack = headerZStack || formatZLinear;
            info.IsZLinear = formatZLinear;
            info.VariableZSlices = variableZLinearSlices;
            info.Use5DConcatenation = info.IsZStack;
            info.ZSlices = info.IsZStack ? Math.Max(1, zSlices) : 1;

            if (formatZLinear)
            {
                info.FrameCount = Math.Max(1, nPages);
            }
            else if (headerZStack)
            {
                int plannedZSlices = Math.Max(1, headerSlices);
                bool oneFlatZStack = nPages <= plannedZSlices;
                bool repeatedFlatZStack = !averageFrames
                    && effectiveHeaderFrames > 1
                    && nPages == effectiveHeaderFrames * plannedZSlices;

                if (oneFlatZStack)
                {
                    info.IsZStack = true;
                    info.ZSlices = Math.Max(1, nPages);
                    info.FrameCount = 1;
                }
                else if (repeatedFlatZStack)
                {
                    info.IsZStack = true;
                    info.ZSlices = plannedZSlices;
                    info.FrameCount = Math.Max(1, effectiveHeaderFrames);
                }
                else
                {
                    info.IsZStack = false;
                    info.ZSlices = 1;
                    info.FrameCount = Math.Max(1, nPages);
                    info.Use5DConcatenation = true;
                }
            }

            return true;
        }

        private static int GetFirstClassicTiffImageLength(string fileName)
        {
            try
            {
                using (var image = BitMiracle.LibTiff.Classic.Tiff.Open(fileName, "r"))
                {
                    if (image == null)
                        return 1;

                    var value = image.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGELENGTH);
                    if (value == null || value.Length == 0)
                        return 1;

                    return Math.Max(1, value[0].ToInt());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not inspect TIFF stack layout: " + ex.Message);
                return 1;
            }
        }

        private static void GetClassicTiffImageLengthRange(string fileName, out int minImageLength, out int maxImageLength)
        {
            minImageLength = 1;
            maxImageLength = 1;

            try
            {
                using (var image = BitMiracle.LibTiff.Classic.Tiff.Open(fileName, "r"))
                {
                    if (image == null)
                        return;

                    bool foundDirectory = false;
                    do
                    {
                        var value = image.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGELENGTH);
                        if (value == null || value.Length == 0)
                            continue;

                        int imageLength = Math.Max(1, value[0].ToInt());
                        if (!foundDirectory)
                        {
                            minImageLength = imageLength;
                            maxImageLength = imageLength;
                            foundDirectory = true;
                        }
                        else
                        {
                            minImageLength = Math.Min(minImageLength, imageLength);
                            maxImageLength = Math.Max(maxImageLength, imageLength);
                        }
                    }
                    while (image.ReadDirectory());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not inspect TIFF stack range: " + ex.Message);
                minImageLength = 1;
                maxImageLength = 1;
            }
        }

        private static int ReadHeaderInt(string header, string key, int fallback)
        {
            string value = ReadHeaderString(header, key, "");
            return Int32.TryParse(value, out int result) ? result : fallback;
        }

        private static bool ReadHeaderBool(string header, string key, bool fallback)
        {
            string value = ReadHeaderString(header, key, "");
            return Boolean.TryParse(value, out bool result) ? result : fallback;
        }

        private static bool HeaderAveragesFrames(string header)
        {
            return ReadHeaderBool(header, "State.Acq.aveFrame", false)
                || ReadHeaderBoolArrayAny(header, "State.Acq.aveFrameA");
        }

        private static int GetEffectiveHeaderFrameCount(string header)
        {
            int headerFrames = Math.Max(1, ReadHeaderInt(header, "State.Acq.nFrames", 1));
            if (!HeaderAveragesFrames(header))
                return headerFrames;

            int averagedFrames = ReadHeaderInt(header, "State.Acq.nAveragedFrames", 0);
            if (averagedFrames > 0)
                return averagedFrames;

            int nAveFrame = Math.Max(1, ReadHeaderInt(header, "State.Acq.nAveFrame", 1));
            if (nAveFrame > 1)
                return Math.Max(1, (int)Math.Ceiling((double)headerFrames / nAveFrame));

            return headerFrames;
        }

        private static bool ReadHeaderBoolArrayAny(string header, string key)
        {
            string value = ReadHeaderString(header, key, "");
            if (String.IsNullOrWhiteSpace(value))
                return false;

            foreach (Match match in Regex.Matches(value, @"\b(true|false|1|0)\b", RegexOptions.IgnoreCase))
            {
                string token = match.Groups[1].Value;
                if (String.Equals(token, "true", StringComparison.OrdinalIgnoreCase) || token == "1")
                    return true;
            }

            return false;
        }

        private static string ReadHeaderString(string header, string key, string fallback)
        {
            if (String.IsNullOrEmpty(header) || String.IsNullOrEmpty(key))
                return fallback;

            string pattern = @"(^[ \t]*" + Regex.Escape(key) + @"[ \t]*=[ \t]*)([^;\r\n]*)([ \t]*;?[^\r\n]*)";
            Match match = Regex.Match(header, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (!match.Success)
                return fallback;

            return match.Groups[2].Value.Trim().Trim('"');
        }

        private static bool ShouldOpenZLinearAs5D(string fileName, string header, int nPages)
        {
            if (nPages <= 0)
                return false;

            string format = ReadHeaderString(header, "Format", "");
            if (!String.Equals(format, FileIO.FileFormat.ZLinear.ToString(), StringComparison.OrdinalIgnoreCase))
                return false;

            return nPages > 1 || GetFirstClassicTiffImageLength(fileName) > 1;
        }

        private static bool IsHeaderZStackStoredAsTimeSeries(string header, int nPages)
        {
            if (nPages <= 1)
                return false;

            string format = ReadHeaderString(header, "Format", "");
            if (String.Equals(format, FileIO.FileFormat.ZLinear.ToString(), StringComparison.OrdinalIgnoreCase))
                return false;

            int headerSlices = ReadHeaderInt(header, "State.Acq.nSlices", 1);
            if (headerSlices <= 1)
                headerSlices = ReadHeaderInt(header, "State.Acq.numberOfZSlices", 1);
            bool averageFrames = HeaderAveragesFrames(header);
            int effectiveHeaderFrames = GetEffectiveHeaderFrameCount(header);

            bool headerZStack = ReadHeaderBool(header, "State.Acq.ZStack", false) && headerSlices > 1;
            if (!headerZStack)
                return false;

            int plannedZSlices = Math.Max(1, headerSlices);
            if (nPages <= plannedZSlices)
                return false;

            if (averageFrames && effectiveHeaderFrames > 1)
                return true;

            return effectiveHeaderFrames <= 1 || nPages != effectiveHeaderFrames * plannedZSlices;
        }

        private bool TryReadConcatenate5DZStackPages(string fileName, int zSlices,
            out List<ushort[][][,,]> pages, out List<DateTime> acquiredTimes,
            out bool[] saveChannels, out ScanParameters state, out string errorMessage)
        {
            pages = new List<ushort[][][,,]>();
            acquiredTimes = new List<DateTime>();
            saveChannels = null;
            state = null;
            errorMessage = "";

            int nPages = FileIO.SetupFLIMOpening(fileName, out _);
            if (nPages <= 0)
            {
                errorMessage = "Input file has no pages: " + fileName;
                return false;
            }

            FLIMData inputData = new FLIMData(FLIM_ImgData.State);
            inputData.KeepPagesInMemory = true;

            for (int page = 0; page < nPages; page++)
            {
                FileIO.FileError error = FileIO.OpenFLIMTiffFilePage(fileName, page, page, inputData, page == 0, true);
                if (error != FileIO.FileError.Success)
                {
                    errorMessage = String.Format("Could not read page {0} from {1}", page + 1, fileName);
                    return false;
                }
            }

            saveChannels = inputData.saveChannels;
            state = inputData.State;
            Extract5DZStackPages(inputData, zSlices, pages, acquiredTimes);

            if (pages.Count == 0)
            {
                errorMessage = "Could not build 5D pages from Z-stack file: " + fileName;
                return false;
            }

            return true;
        }

        private bool TryReadConcatenateFlatZPlanes(string fileName,
            out List<ushort[][,,]> planes, out List<DateTime> acquiredTimes,
            out bool[] saveChannels, out ScanParameters state, out string errorMessage)
        {
            planes = new List<ushort[][,,]>();
            acquiredTimes = new List<DateTime>();
            saveChannels = null;
            state = null;
            errorMessage = "";

            int nPages = FileIO.SetupFLIMOpening(fileName, out _);
            if (nPages <= 0)
            {
                errorMessage = "Input file has no pages: " + fileName;
                return false;
            }

            FLIMData inputData = new FLIMData(FLIM_ImgData.State);
            inputData.KeepPagesInMemory = true;

            for (int page = 0; page < nPages; page++)
            {
                FileIO.FileError error = FileIO.OpenFLIMTiffFilePage(fileName, page, page, inputData, page == 0, true);
                if (error != FileIO.FileError.Success)
                {
                    errorMessage = String.Format("Could not read page {0} from {1}", page + 1, fileName);
                    return false;
                }
            }

            saveChannels = inputData.saveChannels;
            state = inputData.State;

            if (inputData.FLIM_Pages != null)
            {
                int planeCount = Math.Min(inputData.FLIM_Pages.Length, Math.Max(nPages, inputData.n_pages));
                for (int page = 0; page < planeCount; page++)
                {
                    ushort[][,,] plane = inputData.FLIM_Pages[page];
                    if (plane == null)
                        continue;

                    planes.Add(plane);
                    DateTime acquiredTime = inputData.acquiredTime_Pages != null && page < inputData.acquiredTime_Pages.Length
                        ? inputData.acquiredTime_Pages[page]
                        : inputData.acquiredTime;
                    acquiredTimes.Add(acquiredTime);
                }
            }

            if (planes.Count == 0)
            {
                errorMessage = "Could not read image pages from file: " + fileName;
                return false;
            }

            return true;
        }

        private static ushort[][][,,] Build5DZStackPageFromFlatPlanes(List<ushort[][,,]> planes, int zSlices)
        {
            if (planes == null || planes.Count < zSlices || zSlices <= 0)
                return null;

            ushort[][][,,] zPages = new ushort[zSlices][][,,];
            for (int z = 0; z < zSlices; z++)
                zPages[z] = planes[z];

            return ImageProcessing.FLIM_Pages2FLIMRaw5D(zPages);
        }

        private static ushort[][][,,] BuildSingleZ5DPage(ushort[][,,] plane)
        {
            if (plane == null)
                return null;

            return ImageProcessing.FLIM_Pages2FLIMRaw5D(new ushort[1][][,,] { plane });
        }

        private static ushort[][,,] BuildSinglePlane4DPage(ushort[][][,,] page5D)
        {
            if (page5D == null)
                return null;

            var pages4D = ImageProcessing.PermuteFLIM5D(page5D, false);
            if (pages4D == null || pages4D.Length == 0)
                return null;

            return pages4D[0];
        }

        private int SaveConcatenatedSinglePlanePage(FileIO fileIO, string output_filename,
            ushort[][,,] page4D, DateTime acquiredTime, bool[] saveChannels,
            int totalFrames, bool saveAsOmeTiff, int expectedOmePlanes, int decodedFrame)
        {
            int saveError = 0;
            SaveWithDerivedHeader(fileIO, output_filename, totalFrames, saveAsOmeTiff, () =>
            {
                SetStateAsSinglePlaneTimeCourse(fileIO.State);
                saveError = fileIO.SaveFLIMInTiff(output_filename, page4D, acquiredTime,
                    decodedFrame == 0, saveChannels);
            }, expectedOmePlanes);

            return saveError;
        }

        private int SaveConcatenated5DZStackPage(FileIO fileIO, string output_filename,
            ushort[][][,,] page5D, DateTime acquiredTime, bool[] saveChannels,
            int zSlices, int totalFrames, bool saveAsOmeTiff, int expectedOmePlanes, int decodedFrame)
        {
            int saveError = 0;
            ushort[][][,,] zStackPage = ImageProcessing.PermuteFLIM5D(page5D, false);
            SaveWithDerivedHeader(fileIO, output_filename, totalFrames, saveAsOmeTiff, () =>
            {
                SetStateAsZStackTimeCourse(fileIO.State, zSlices, totalFrames);
                saveError = fileIO.SaveFLIMInTiffZStack(output_filename, zStackPage, acquiredTime,
                    decodedFrame == 0, saveChannels);
            }, expectedOmePlanes);

            return saveError;
        }

        private static void Extract5DZStackPages(FLIMData inputData, int zSlices,
            List<ushort[][][,,]> pages, List<DateTime> acquiredTimes)
        {
            if (inputData == null || pages == null || acquiredTimes == null)
                return;

            if (inputData.FLIM_Pages5D != null && inputData.n_pages5D > 0)
            {
                int pageCount = Math.Min(inputData.n_pages5D, inputData.FLIM_Pages5D.Length);
                for (int page = 0; page < pageCount; page++)
                {
                    ushort[][][,,] page5D = inputData.FLIM_Pages5D[page];
                    if (page5D == null)
                        continue;

                    pages.Add(page5D);
                    DateTime acquiredTime = inputData.acquiredTime_Pages5D != null && page < inputData.acquiredTime_Pages5D.Length
                        ? inputData.acquiredTime_Pages5D[page]
                        : inputData.acquiredTime;
                    acquiredTimes.Add(acquiredTime);
                }

                if (pages.Count > 0)
                    return;
            }

            if (inputData.FLIM_Pages == null || inputData.FLIM_Pages.Length == 0)
                return;

            zSlices = Math.Max(1, zSlices);
            int totalPlanes = inputData.FLIM_Pages.Length;
            int timePages = Math.Max(1, totalPlanes / zSlices);
            for (int page = 0; page < timePages; page++)
            {
                int start = page * zSlices;
                if (start + zSlices > totalPlanes)
                    break;

                ushort[][][,,] zPages = new ushort[zSlices][][,,];
                for (int z = 0; z < zSlices; z++)
                    zPages[z] = inputData.FLIM_Pages[start + z];

                pages.Add(ImageProcessing.FLIM_Pages2FLIMRaw5D(zPages));
                DateTime acquiredTime = inputData.acquiredTime_Pages != null && start < inputData.acquiredTime_Pages.Length
                    ? inputData.acquiredTime_Pages[start]
                    : inputData.acquiredTime;
                acquiredTimes.Add(acquiredTime);
            }
        }

        private static long SumFrameCounts(int[] frameCounts)
        {
            if (frameCounts == null)
                return 0;

            long total = 0;
            for (int i = 0; i < frameCounts.Length; i++)
                total += Math.Max(0, frameCounts[i]);
            return total;
        }

        private bool ShouldConcatenateSaveAsOmeTiff(string[] fileNames, bool saveStack, out int[] frameCounts, out int totalFrames)
        {
            frameCounts = CountConcatenateFrames(fileNames, saveStack);
            long totalFramesLong = SumFrameCounts(frameCounts);
            if (totalFramesLong <= 0)
                totalFramesLong = fileNames?.Length ?? 0;

            totalFrames = totalFramesLong > int.MaxValue ? int.MaxValue : (int)totalFramesLong;

            bool hasOmeInput = fileNames != null && fileNames.Any(IsOmeTiffFileName);
            return hasOmeInput || CurrentFileIsOmeTiff() || totalFramesLong > CLASSIC_TIFF_MAX_FRAME_COUNT;
        }

        private void SaveWithConcatenateFrameCount(FileIO fileIO, int totalFrames, Action saveAction)
        {
            if (saveAction == null)
                return;

            if (FLIM_ImgData == null || FLIM_ImgData.State == null || totalFrames <= 0)
            {
                saveAction();
                return;
            }

            int saveImageNFrames = FLIM_ImgData.State.Acq.nFrames;
            bool restoreFileIONFrames = fileIO != null
                && fileIO.State != null
                && !Object.ReferenceEquals(fileIO.State, FLIM_ImgData.State);
            int saveFileIONFrames = restoreFileIONFrames ? fileIO.State.Acq.nFrames : 0;
            int saveExpectedOmeFrames = fileIO?.ExpectedOmeTiffFrames ?? 0;

            try
            {
                FLIM_ImgData.State.Acq.nFrames = totalFrames;
                if (restoreFileIONFrames)
                    fileIO.State.Acq.nFrames = totalFrames;
                if (fileIO != null)
                    fileIO.ExpectedOmeTiffFrames = totalFrames;

                saveAction();
            }
            finally
            {
                FLIM_ImgData.State.Acq.nFrames = saveImageNFrames;
                if (restoreFileIONFrames)
                    fileIO.State.Acq.nFrames = saveFileIONFrames;
                if (fileIO != null)
                    fileIO.ExpectedOmeTiffFrames = saveExpectedOmeFrames;
            }
        }

        private void CancelConcatenateOutput(string output_filename)
        {
            CancelDerivedOutput(output_filename, "Concatenation canceled.");
        }

        private void CancelDerivedOutput(string output_filename, string statusText)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(output_filename) && File.Exists(output_filename))
                    File.Delete(output_filename);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not delete canceled output: " + ex.Message);
            }

            flimage?.WriteStatusText(statusText);
            this.InvokeIfRequired(o =>
            {
                o.TurnOffDuringOpeningProc(true);
                o.UpdateFileName();
            });
            AnalysisStatus = "None";
        }

        public void ConcatenateFiles(string[] fileNames, string output_filename, bool external_command)
        {
            AnalysisStatus = "ConcatenateFiles";
            string filenameTail = "_concat";

            if (fileNames == null || fileNames.Length == 0)
                return;

            FLIMData flim_data = new FLIMData(FLIM_ImgData.State);

            flim_data.baseName = FLIM_ImgData.baseName;
            flim_data.PutROIs(FLIM_ImgData.CopyROIs());



            var fileNameWithoutPath = Path.GetFileNameWithoutExtension(fileNames[0]);
            bool saveStack = displayZProjection;
            bool saveAsOmeTiff = ShouldConcatenateSaveAsOmeTiff(fileNames, saveStack, out int[] decodedFrameCounts, out int decodedTotalFrames);
            bool concatenateAs5DZStack = ShouldConcatenateAs5DZStack(fileNames, saveStack,
                out int concatenateZSlices, out int[] stackFrameCounts, out int stackTotalFrames,
                out bool variableConcatenateZSlices, out string concatenateShapeKey);
            if (concatenateAs5DZStack)
            {
                decodedFrameCounts = stackFrameCounts;
                decodedTotalFrames = stackTotalFrames;
            }
            if (concatenateAs5DZStack && variableConcatenateZSlices)
                saveAsOmeTiff = false;

            if (String.IsNullOrWhiteSpace(output_filename))
                output_filename = Path.Combine(Path.GetDirectoryName(fileNames[0]), fileNameWithoutPath + filenameTail + (saveAsOmeTiff ? OmeTiffExtension() : ".flim"));
            else if (String.IsNullOrEmpty(Path.GetDirectoryName(output_filename)))
                output_filename = Path.Combine(Path.GetDirectoryName(fileNames[0]), output_filename);
            output_filename = NormalizeOmeTiffOutputFileName(output_filename, saveAsOmeTiff);
            if (concatenateAs5DZStack && variableConcatenateZSlices && IsOmeTiffFileName(output_filename))
                output_filename = Path.ChangeExtension(output_filename, ".flim");
            bool concatenateAsSinglePlaneTimeCourse = concatenateAs5DZStack && concatenateZSlices <= 1;

            FLIMData.FileType file_type = FLIMData.FileType.TimeCourse;

            //if ((FLIM_ImgData.nFastZ > 1 || FLIM_ImgData.ZStack) && !displayZProjection)
            //    file_type = FLIMData.FileType.TimeCourse_ZStack;

            //FLIM_ImgData.KeepPagesInMemory = true;
            //flim_data.KeepPagesInMemory = true;
            int savePage = FLIM_ImgData.currentPage;
            bool keepResultInMemory = flimage?.KeepPagesInMemoryCheck?.Checked ?? FLIM_ImgData.KeepPagesInMemory;
            List<ushort[][,,]> concatenatedPages = keepResultInMemory ? new List<ushort[][,,]>() : null;
            List<DateTime> concatenatedTimes = keepResultInMemory ? new List<DateTime>() : null;
            List<ushort[][][,,]> concatenatedPages5D = keepResultInMemory && concatenateAs5DZStack && !concatenateAsSinglePlaneTimeCourse ? new List<ushort[][][,,]>() : null;
            List<DateTime> concatenatedTimes5D = keepResultInMemory && concatenateAs5DZStack && !concatenateAsSinglePlaneTimeCourse ? new List<DateTime>() : null;

            int FileN = 0;

            var fileIO = new FileIO(FLIM_ImgData.State);

            if (!FLIM_ImgData.numberedFile)
            {
                MessageBox.Show("Only numbered file works for this");
                return;
            }

            try
            {
                if (!keepResultInMemory && !saveAsOmeTiff && !saveStack && !concatenateAs5DZStack && !StopFileOpening)
                {
                    try
                    {
                        if (fileIO.TryConcatenateFLIMTiffRaw(fileNames, output_filename, out string rawConcatError,
                            () => StopFileOpening, UpdateConcatenateProgress, UpdateConcatenateFrameProgress))
                        {
                            if (StopFileOpening)
                            {
                                CancelConcatenateOutput(output_filename);
                                return;
                            }

                            OpenFLIM(output_filename, true, plot_regular.calc_upon_open, true);

                            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

                            if (external_command)
                            {
                                flimage.flimage_io.Notify(new ProcessEventArgs("AnalysisDone", null));
                            }

                            AnalysisStatus = "None";
                            return;
                        }

                        if (StopFileOpening || String.Equals(rawConcatError, "Canceled.", StringComparison.OrdinalIgnoreCase))
                        {
                            CancelConcatenateOutput(output_filename);
                            return;
                        }

                        Debug.WriteLine("Raw FLIM concatenation skipped: " + rawConcatError);
                    }
                    catch (Exception ex)
                    {
                        if (StopFileOpening)
                        {
                            CancelConcatenateOutput(output_filename);
                            return;
                        }

                        Debug.WriteLine("Raw FLIM concatenation failed; using decoded path. " + ex.Message);
                    }
                }

            bool stopped_before_done = false;
            if (decodedTotalFrames <= 0)
                decodedTotalFrames = concatenateAs5DZStack ? 0 : fileNames.Length;
            int decodedFrame = 0;

            if (concatenateAs5DZStack)
            {
                long expectedOmePlanesLong = decodedTotalFrames > 0 && concatenateZSlices > 0
                    ? (long)decodedTotalFrames * concatenateZSlices
                    : 0;
                int expectedOmePlanes = expectedOmePlanesLong > int.MaxValue ? int.MaxValue : (int)expectedOmePlanesLong;
                bool failed = false;
                int skippedIncompatibleFiles = 0;

                for (int i = 0; i < fileNames.Length; i++)
                {
                    var fileName = fileNames[i];
                    var currentFileNameWithoutPath = Path.GetFileNameWithoutExtension(fileName);
                    if (currentFileNameWithoutPath.Length < FLIM_ImgData.baseName.Length)
                        continue;

                    var fileNum = currentFileNameWithoutPath.Substring(FLIM_ImgData.baseName.Length);

                    if (Int32.TryParse(fileNum, out int n))
                    {
                        UpdateConcatenateProgress(i + 1, fileNames.Length, Math.Min(decodedFrame + 1, decodedTotalFrames), decodedTotalFrames, fileName);

                        if (StopFileOpening)
                        {
                            stopped_before_done = true;
                            break;
                        }

                        if (!TryGetConcatenateFrameInfo(fileName, out ConcatenateFrameInfo currentInfo))
                        {
                            failed = true;
                            MessageBox.Show("Could not inspect Z-stack file: " + fileName);
                            break;
                        }

                        if (!String.IsNullOrEmpty(concatenateShapeKey)
                            && !String.Equals(currentInfo.ShapeKey, concatenateShapeKey, StringComparison.Ordinal))
                        {
                            Debug.WriteLine(String.Format("Skipping incompatible Z-stack file {0}: shape {1}, expected {2}",
                                fileName, currentInfo.ShapeKey, concatenateShapeKey));
                            skippedIncompatibleFiles++;
                            continue;
                        }

                        if (currentInfo.IsZLinear)
                        {
                            if (!TryReadConcatenate5DZStackPages(fileName, concatenateZSlices,
                                out List<ushort[][][,,]> pages5D, out List<DateTime> pageTimes,
                                out bool[] inputSaveChannels, out ScanParameters inputState, out string readError))
                            {
                                failed = true;
                                MessageBox.Show(readError);
                                break;
                            }

                            fileIO.State = inputState;
                            for (int stackPage = 0; stackPage < pages5D.Count; stackPage++)
                            {
                                if (StopFileOpening)
                                {
                                    stopped_before_done = true;
                                    break;
                                }

                                ushort[][][,,] page5D = pages5D[stackPage];
                                DateTime acquiredTime = pageTimes != null && stackPage < pageTimes.Count ? pageTimes[stackPage] : DateTime.Now;
                                int pageZSlices = GetZSlicesFrom5DPage(page5D);
                                int saveError;
                                if (concatenateAsSinglePlaneTimeCourse)
                                {
                                    ushort[][,,] page4D = BuildSinglePlane4DPage(page5D);
                                    if (page4D == null)
                                    {
                                        failed = true;
                                        MessageBox.Show("Could not build single-plane page from ZLinear file: " + fileName);
                                        break;
                                    }

                                    saveError = SaveConcatenatedSinglePlanePage(fileIO, output_filename, page4D, acquiredTime,
                                        inputSaveChannels, decodedTotalFrames, saveAsOmeTiff, expectedOmePlanes, decodedFrame);

                                    if (keepResultInMemory)
                                    {
                                        concatenatedPages.Add(page4D);
                                        concatenatedTimes.Add(acquiredTime);
                                    }
                                }
                                else
                                {
                                    saveError = SaveConcatenated5DZStackPage(fileIO, output_filename, page5D, acquiredTime,
                                        inputSaveChannels, pageZSlices, decodedTotalFrames, saveAsOmeTiff, expectedOmePlanes, decodedFrame);

                                    if (keepResultInMemory)
                                    {
                                        concatenatedPages5D.Add(page5D);
                                        concatenatedTimes5D.Add(acquiredTime);
                                    }
                                }

                                if (saveError != 0)
                                {
                                    failed = true;
                                    MessageBox.Show("Problem in saving ! Frame = " + (decodedFrame + 1));
                                    break;
                                }

                                decodedFrame++;
                                UpdateConcatenateProgress(i + 1, fileNames.Length, decodedFrame, decodedTotalFrames, fileName);
                            }
                        }
                        else
                        {
                            if (!TryReadConcatenateFlatZPlanes(fileName,
                                out List<ushort[][,,]> zPlanes, out List<DateTime> zPlaneTimes,
                                out bool[] inputSaveChannels, out ScanParameters inputState, out string readError))
                            {
                                failed = true;
                                MessageBox.Show(readError);
                                break;
                            }

                            fileIO.State = inputState;
                            if (StopFileOpening)
                            {
                                stopped_before_done = true;
                                break;
                            }

                            if (currentInfo.IsZStack)
                            {
                                int pageZSlices = Math.Max(1, currentInfo.ZSlices);
                                int savedStackPages = 0;
                                int stackPageCount = Math.Max(1, zPlanes.Count / pageZSlices);
                                for (int stackPage = 0; stackPage < stackPageCount; stackPage++)
                                {
                                    if (StopFileOpening)
                                    {
                                        stopped_before_done = true;
                                        break;
                                    }

                                    int start = stackPage * pageZSlices;
                                    if (start + pageZSlices > zPlanes.Count)
                                        break;

                                    var stackPlanes = new List<ushort[][,,]>();
                                    for (int z = 0; z < pageZSlices; z++)
                                        stackPlanes.Add(zPlanes[start + z]);

                                    ushort[][][,,] page5D = Build5DZStackPageFromFlatPlanes(stackPlanes, pageZSlices);
                                    if (page5D == null)
                                    {
                                        failed = true;
                                        MessageBox.Show("Could not build 5D page from Z-stack file: " + fileName);
                                        break;
                                    }

                                    DateTime acquiredTime = zPlaneTimes != null && start < zPlaneTimes.Count ? zPlaneTimes[start] : DateTime.Now;
                                    int saveError;
                                    if (concatenateAsSinglePlaneTimeCourse)
                                    {
                                        saveError = SaveConcatenatedSinglePlanePage(fileIO, output_filename, stackPlanes[0], acquiredTime,
                                            inputSaveChannels, decodedTotalFrames, saveAsOmeTiff, expectedOmePlanes, decodedFrame);

                                        if (keepResultInMemory)
                                        {
                                            concatenatedPages.Add(stackPlanes[0]);
                                            concatenatedTimes.Add(acquiredTime);
                                        }
                                    }
                                    else
                                    {
                                        saveError = SaveConcatenated5DZStackPage(fileIO, output_filename, page5D, acquiredTime,
                                            inputSaveChannels, pageZSlices, decodedTotalFrames, saveAsOmeTiff, expectedOmePlanes, decodedFrame);

                                        if (keepResultInMemory)
                                        {
                                            concatenatedPages5D.Add(page5D);
                                            concatenatedTimes5D.Add(acquiredTime);
                                        }
                                    }

                                    if (saveError != 0)
                                    {
                                        failed = true;
                                        MessageBox.Show("Problem in saving ! Frame = " + (decodedFrame + 1));
                                        break;
                                    }

                                    savedStackPages++;
                                    decodedFrame++;
                                    UpdateConcatenateProgress(i + 1, fileNames.Length, decodedFrame, decodedTotalFrames, fileName);
                                }

                                if (!stopped_before_done && !failed && savedStackPages == 0)
                                {
                                    failed = true;
                                    MessageBox.Show("Could not build 5D page from Z-stack file: " + fileName);
                                    break;
                                }
                            }
                            else
                            {
                                for (int page = 0; page < zPlanes.Count; page++)
                                {
                                    if (StopFileOpening)
                                    {
                                        stopped_before_done = true;
                                        break;
                                    }

                                    ushort[][][,,] page5D = BuildSingleZ5DPage(zPlanes[page]);
                                    if (page5D == null)
                                    {
                                        failed = true;
                                        MessageBox.Show("Could not build single-Z 5D page from time-stack file: " + fileName);
                                        break;
                                    }

                                    DateTime acquiredTime = zPlaneTimes != null && page < zPlaneTimes.Count ? zPlaneTimes[page] : DateTime.Now;
                                    int saveError;
                                    if (concatenateAsSinglePlaneTimeCourse)
                                    {
                                        saveError = SaveConcatenatedSinglePlanePage(fileIO, output_filename, zPlanes[page], acquiredTime,
                                            inputSaveChannels, decodedTotalFrames, saveAsOmeTiff, expectedOmePlanes, decodedFrame);

                                        if (keepResultInMemory)
                                        {
                                            concatenatedPages.Add(zPlanes[page]);
                                            concatenatedTimes.Add(acquiredTime);
                                        }
                                    }
                                    else
                                    {
                                        saveError = SaveConcatenated5DZStackPage(fileIO, output_filename, page5D, acquiredTime,
                                            inputSaveChannels, 1, decodedTotalFrames, saveAsOmeTiff, expectedOmePlanes, decodedFrame);

                                        if (keepResultInMemory)
                                        {
                                            concatenatedPages5D.Add(page5D);
                                            concatenatedTimes5D.Add(acquiredTime);
                                        }
                                    }

                                    if (saveError != 0)
                                    {
                                        failed = true;
                                        MessageBox.Show("Problem in saving ! Frame = " + (decodedFrame + 1));
                                        break;
                                    }

                                    decodedFrame++;
                                    UpdateConcatenateProgress(i + 1, fileNames.Length, decodedFrame, decodedTotalFrames, fileName);
                                }
                            }
                        }

                        if (stopped_before_done || failed)
                            break;
                    }
                    Application.DoEvents();
                    if (StopFileOpening)
                    {
                        stopped_before_done = true;
                        break;
                    }
                }

                if (!stopped_before_done && !StopFileOpening && !failed)
                {
                    if (decodedFrame == 0)
                    {
                        failed = true;
                        MessageBox.Show("No complete Z-stack frames were found in the selected files.");
                    }
                }

                if (stopped_before_done || StopFileOpening || failed)
                {
                    fileIO.CloseAllFlimWriters();
                    if (failed)
                        CancelConcatenateOutput(output_filename);
                    else
                        CancelConcatenateOutput(output_filename);
                    return;
                }

                fileIO.CloseAllFlimWriters();
                if (concatenateAsSinglePlaneTimeCourse && keepResultInMemory && concatenatedPages != null && concatenatedPages.Count > 0)
                    ReplaceCurrentPagesWithDerivedOutput(output_filename, concatenatedPages, concatenatedTimes,
                        decodedFrame, saveAsOmeTiff);
                else if (keepResultInMemory && concatenatedPages5D != null && concatenatedPages5D.Count > 0)
                    ReplaceCurrentPages5DWithDerivedOutput(output_filename, concatenatedPages5D, concatenatedTimes5D,
                        decodedFrame, saveAsOmeTiff, concatenateZSlices, true);
                else
                    OpenFLIM(output_filename, true, plot_regular.calc_upon_open, true);

                if (skippedIncompatibleFiles > 0)
                    flimage?.WriteStatusText(String.Format("Concatenation finished. Skipped {0} incompatible Z-stack file(s).", skippedIncompatibleFiles));

                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

                if (external_command)
                    flimage.flimage_io.Notify(new ProcessEventArgs("AnalysisDone", null));

                AnalysisStatus = "None";
                return;
            }

            //foreach (String fileName in fileNames)
            for (int i = 0; i < fileNames.Length; i++)
            {
                var fileName = fileNames[i];
                var currentFileNameWithoutPath = Path.GetFileNameWithoutExtension(fileName);
                if (currentFileNameWithoutPath.Length < FLIM_ImgData.baseName.Length)
                    continue;

                var fileNum = currentFileNameWithoutPath.Substring(FLIM_ImgData.baseName.Length);

                if (Int32.TryParse(fileNum, out int n))
                {
                    UpdateConcatenateProgress(i + 1, fileNames.Length, Math.Min(decodedFrame + 1, decodedTotalFrames), decodedTotalFrames, fileName);

                    if (!StopFileOpening)
                    {
                        OpenFLIM(fileName, false, plot_regular.calc_upon_open, true);

                        if (saveStack)
                        {
                            calcZProjection();
                            if (keepResultInMemory)
                            {
                                concatenatedPages.Add((ushort[][,,])Copier.DeepCopyArray(FLIM_ImgData.FLIMRaw));
                                concatenatedTimes.Add(FLIM_ImgData.acquiredTime);
                            }
                            SaveWithDerivedHeader(fileIO, output_filename, decodedTotalFrames, saveAsOmeTiff, () =>
                            {
                                SetStateAsSinglePlaneTimeCourse(fileIO.State);
                                var error = fileIO.SaveFLIMInTiff(output_filename, FLIM_ImgData.FLIMRaw, FLIM_ImgData.acquiredTime, i == 0, FLIM_ImgData.saveChannels);
                            });
                            decodedFrame += 1;
                            UpdateConcatenateProgress(i + 1, fileNames.Length, decodedFrame, decodedTotalFrames, fileName);
                        }
                        else
                        {
                            SetStateAsSinglePlaneTimeCourse(FLIM_ImgData.State);
                            for (int page = 0; page < FLIM_ImgData.n_pages; page++)
                            {
                                if (StopFileOpening)
                                {
                                    stopped_before_done = true;
                                    break;
                                }

                                UpdateConcatenateProgress(i + 1, fileNames.Length, Math.Min(decodedFrame + 1, decodedTotalFrames), decodedTotalFrames, fileName);
                                GotoPage4D(page);
                                if (keepResultInMemory)
                                {
                                    ushort[][,,] pageData = FLIM_ImgData.KeepPagesInMemory
                                        && FLIM_ImgData.FLIM_Pages != null
                                        && page >= 0
                                        && page < FLIM_ImgData.FLIM_Pages.Length
                                        ? FLIM_ImgData.FLIM_Pages[page]
                                        : FLIM_ImgData.FLIMRaw;
                                    concatenatedPages.Add(pageData);
                                    concatenatedTimes.Add(FLIM_ImgData.acquiredTime);
                                }
                                SaveWithDerivedHeader(fileIO, output_filename, decodedTotalFrames, saveAsOmeTiff, () =>
                                {
                                    SetStateAsSinglePlaneTimeCourse(fileIO.State);
                                    var error = fileIO.SaveFLIMInTiff(output_filename, FLIM_ImgData.FLIMRaw, FLIM_ImgData.acquiredTime, i == 0 && page == 0, FLIM_ImgData.saveChannels);
                                });
                                decodedFrame++;
                            }
                        }

                    }
                    else
                    {
                        stopped_before_done = true;
                        break;
                    }
                }
                Application.DoEvents(); //make the event stoppable with button.0
                if (StopFileOpening)
                {
                    stopped_before_done = true;
                    break;
                }
            }

            if (stopped_before_done || StopFileOpening)
            {
                fileIO.CloseAllFlimWriters();
                CancelConcatenateOutput(output_filename);
                return;
            }

            fileIO.CloseAllFlimWriters();
            if (keepResultInMemory && concatenatedPages != null && concatenatedPages.Count > 0)
            {
                ReplaceCurrentPagesWithDerivedOutput(output_filename, concatenatedPages, concatenatedTimes, decodedTotalFrames, saveAsOmeTiff);
            }
            else
            {
                OpenFLIM(output_filename, true, plot_regular.calc_upon_open, true);
            }

            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

            if (external_command)
            {
                flimage.flimage_io.Notify(new ProcessEventArgs("AnalysisDone", null));
            }

            AnalysisStatus = "None";
            }
            finally
            {
                fileIO.CloseAllFlimWriters();
            }
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

        private string BinFramesOutputFileName(int binning, string output_filename)
        {
            var sourceFileName = FLIM_ImgData.fullFileName;
            bool saveAsOmeTiff = ShouldSaveDerivedAsOmeTiff(output_filename);
            return DerivedFLIMOutputFileName(sourceFileName, String.Format("_bin{0}", binning), output_filename, saveAsOmeTiff);
        }

        public void BinFrames(int binning, string output_filename = null)
        {

            bool bin5D = HasLoaded5DTimePages();
            int totalPages = bin5D ? FLIM_ImgData.n_pages5D : FLIM_ImgData.n_pages;
            if (binning < 1 || binning > totalPages)
            {
                MessageBox.Show("Binning factor (" + binning + ") needs to be > 0 and < #pages (" + totalPages + ")");
                return;
            }

            bool stopped = false;
            bool failed = false;
            bool stoppedOutputHandled = false;
            string outputFileToCancel = null;
            this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false));

            try
            {
                if (bin5D)
                {
                    int pageCount5D = Math.Min(FLIM_ImgData.n_pages5D, FLIM_ImgData.FLIM_Pages5D.Length);
                    int finalPageSize = pageCount5D / binning;
                    var fileName = BinFramesOutputFileName(binning, output_filename);
                    bool saveAsOmeTiff = ShouldSaveDerivedAsOmeTiff(fileName);
                    var binnedPages = new List<ushort[][][,,]>();
                    var binnedTimes = new List<DateTime>();

                    for (int i = 0; i < finalPageSize; i++)
                    {
                        if (StopFileOpening)
                        {
                            stopped = true;
                            break;
                        }

                        if (!TryBuildBinned5DPage(i * binning, binning, out ushort[][][,,] ave, out string binError))
                        {
                            failed = true;
                            MessageBox.Show("Could not bin 5D pages: " + binError);
                            break;
                        }

                        if (stopped)
                            break;

                        binnedPages.Add(ave);
                        binnedTimes.Add(FLIM_ImgData.acquiredTime_Pages5D[i * binning]);
                        Application.DoEvents();
                    }

                    if (!failed && binnedPages.Count > 0)
                    {
                        int committedPageSize = binnedPages.Count;
                        bool zStackPages = HasZStack5DPages();
                        int zSlices = zStackPages ? GetMaxZSlicesFrom5DPages(binnedPages) : 0;
                        ReplaceCurrentPages5DWithDerivedOutput(fileName, binnedPages, binnedTimes, committedPageSize, saveAsOmeTiff, zSlices, zStackPages);
                        SaveCurrentDerivedOutputIgnoringStop(fileName);

                        if (stopped || StopFileOpening)
                        {
                            stopped = true;
                            ApplyStoppedDerivedOutput(fileName, committedPageSize, saveAsOmeTiff, false, true,
                                String.Format("Binning stopped after {0} binned frame(s).", committedPageSize));
                            stoppedOutputHandled = true;
                        }
                    }
                }
                else
                {
                    if (FLIM_ImgData.KeepPagesInMemory)
                    {
                        int finalPageSize = FLIM_ImgData.n_pages / binning;
                        var fileName = BinFramesOutputFileName(binning, output_filename);
                        bool saveAsOmeTiff = ShouldSaveDerivedAsOmeTiff(fileName);
                        var time_acquired = (DateTime[])FLIM_ImgData.acquiredTime_Pages.Clone();
                        var binnedPages = new List<ushort[][,,]>();
                        var binnedTimes = new List<DateTime>();

                        for (int i = 0; i < finalPageSize; i++)
                        {
                            if (StopFileOpening)
                            {
                                stopped = true;
                                break;
                            }

                            ushort[][,,] ave = (ushort[][,,])Copier.DeepCopyArray(FLIM_ImgData.FLIM_Pages[i * binning]);

                            for (int j = 1; j < binning; j++)
                            {
                                if (StopFileOpening)
                                {
                                    stopped = true;
                                    break;
                                }

                                for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                                {
                                    if (StopFileOpening)
                                    {
                                        stopped = true;
                                        break;
                                    }

                                    MatrixCalc.ArrayCalc(ave[ch], FLIM_ImgData.FLIM_Pages[i * binning + j][ch], CalculationType.Add);
                                }

                                Application.DoEvents();
                                if (stopped)
                                    break;
                            }

                            if (stopped)
                                break;

                            binnedPages.Add(ave);
                            binnedTimes.Add(time_acquired[i * binning]);
                            FLIM_ImgData.LoadFLIMRawFromData4D(ave, time_acquired[i * binning], false);
                            FLIM_ImgData.calculateAll();
                            UpdateImages(false, false, false, false);
                            Refresh();
                            Application.DoEvents();
                        }

                        if (!failed && binnedPages.Count > 0)
                        {
                            int committedPageSize = binnedPages.Count;
                            ReplaceCurrentPagesWithDerivedOutput(fileName, binnedPages, binnedTimes, committedPageSize, saveAsOmeTiff);
                            SaveCurrentDerivedOutputIgnoringStop(fileName);

                            if (stopped || StopFileOpening)
                            {
                                stopped = true;
                                ApplyStoppedDerivedOutput(fileName, committedPageSize, saveAsOmeTiff, false, false,
                                    String.Format("Binning stopped after {0} binned frame(s).", committedPageSize));
                                stoppedOutputHandled = true;
                            }
                        }
                    }
                    else
                    {
                        var fileName = BinFramesOutputFileName(binning, output_filename);
                        bool saveAsOmeTiff = ShouldSaveDerivedAsOmeTiff(fileName);
                        var fileIO = new FileIO(FLIM_ImgData.State);

                        int bin_counter = 0;
                        var ave = new ushort[FLIM_ImgData.nChannels][,,];
                        var bin_dt = FLIM_ImgData.acquiredTime;
                        var total_image_saved = 0;
                        int finalPageSize = FLIM_ImgData.n_pages / binning;
                        fileIO.ExpectedOmeTiffFrames = saveAsOmeTiff ? Math.Max(1, finalPageSize) : 0;

                        try
                        {
                            for (int i = 0; i < FLIM_ImgData.n_pages; i++)
                            {
                                if (StopFileOpening)
                                {
                                    stopped = true;
                                    break;
                                }

                                bin_counter++;

                                GotoPage4D(i);
                                UpdateImages(true, false, false, true, true);
                                Application.DoEvents();

                                var dt = FLIM_ImgData.acquiredTime;

                                if (bin_counter == 1)
                                {
                                    bin_dt = dt;
                                    for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                                        ave[ch] = (ushort[,,])FLIM_ImgData.FLIMRaw[ch].Clone();
                                }
                                else
                                {
                                    for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                                    {
                                        if (StopFileOpening)
                                        {
                                            stopped = true;
                                            break;
                                        }

                                        MatrixCalc.ArrayCalc(ave[ch], FLIM_ImgData.FLIMRaw[ch], CalculationType.Add);
                                    }
                                }

                                if (stopped)
                                    break;

                                if (bin_counter == binning)
                                {
                                    int error = 0;
                                    SaveWithDerivedHeader(fileIO, fileName, finalPageSize, saveAsOmeTiff, () =>
                                    {
                                        error = fileIO.SaveFLIMInTiff(fileName, ave, bin_dt, total_image_saved == 0, FLIM_ImgData.saveChannels);
                                    });

                                    if (error != 0)
                                    {
                                        failed = true;
                                        MessageBox.Show("Problem in saving ! Page = " + i);
                                        break;
                                    }

                                    outputFileToCancel = fileName;
                                    total_image_saved++;
                                    bin_counter = 0;
                                }

                                Application.DoEvents();
                            }
                        }
                        finally
                        {
                            fileIO.CloseAllFlimWriters();
                        }

                        stopped = stopped || StopFileOpening;
                        if (stopped && !failed && total_image_saved > 0)
                        {
                            ApplyStoppedDerivedOutput(fileName, total_image_saved, saveAsOmeTiff, true, false,
                                String.Format("Binning stopped after {0} binned frame(s).", total_image_saved));
                            stoppedOutputHandled = true;
                        }
                        else if (!stopped && !failed)
                        {
                            OpenFLIM(fileName, true, plot_regular.calc_upon_open, true);
                            ApplyOutputFileToCurrentData(fileName, Math.Max(1, finalPageSize), saveAsOmeTiff, false);
                        }
                    }//Memory

                } //FastZ

                if (!stopped && !failed)
                    UpdateImages(true, false, false, true, true);
            }
            finally
            {
                if (stopped && !stoppedOutputHandled)
                    CancelDerivedOutput(outputFileToCancel, "Binning canceled.");
                else
                    this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));
            }

        }





        private void deleteCurrentPageToolStripMenuItem_Click(object sender, EventArgs e)
        {

            int page_to_delete = (FLIM_ImgData.nFastZ > 1) ? FLIM_ImgData.currentPage5D : FLIM_ImgData.currentPage;
            int n_pages = (FLIM_ImgData.nFastZ > 1) ? FLIM_ImgData.n_pages5D : FLIM_ImgData.n_pages;

            if (n_pages < 2)
            {
                MessageBox.Show("This function is only for images with multiple pages");
            }

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

                if (FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[ch] == null || FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[ch].Length < 2)
                    return;

                PlotOnPanel pp = new PlotOnPanel(e, LifetimeCurvePlot.Width, LifetimeCurvePlot.Height);
                pp.plotType = "-";
                pp.addData(FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[ch], FLIM_ImgData.RoiFit.flim_parameters.LifetimeY[ch], plotPen, "-");

                bool roi_plot = roi_State != drawROI_State.NoROI &&
                    Values_selectedROI.Checked && FLIM_ImgData.Fit_type == FLIMData.FitType.GlobalRois;
                bool roi_standard = FLIM_ImgData.currentRoi >= 0 && FLIM_ImgData.currentRoi < FLIM_ImgData.ROIs.Count;

                if (roi_plot && roi_standard)
                {
                    var roiParams = FLIM_ImgData.ROIs[FLIM_ImgData.currentRoi].flim_parameters;
                    if (roiParams?.LifetimeX != null && roiParams.LifetimeX.Length > ch &&
                        roiParams.LifetimeY != null && roiParams.LifetimeY.Length > ch &&
                        roiParams.LifetimeX[ch] != null && roiParams.LifetimeX[ch].Length >= 2 &&
                        roiParams.LifetimeY[ch] != null && roiParams.LifetimeY[ch].Length >= 2)
                    {
                        pp.addData(roiParams.LifetimeX[ch], roiParams.LifetimeY[ch], plotPen2, "-");
                    }
                }
                else if (roi_plot && FLIM_ImgData.currentRoi == -2)
                {
                    var bgParams = FLIM_ImgData.bgRoi?.flim_parameters;
                    if (bgParams?.LifetimeX != null && bgParams.LifetimeX.Length > ch &&
                        bgParams.LifetimeY != null && bgParams.LifetimeY.Length > ch &&
                        bgParams.LifetimeX[ch] != null && bgParams.LifetimeX[ch].Length >= 2 &&
                        bgParams.LifetimeY[ch] != null && bgParams.LifetimeY[ch].Length >= 2)
                    {
                        pp.addData(bgParams.LifetimeX[ch], bgParams.LifetimeY[ch], plotPen2, "-");
                    }
                }

                if (showFit && fittingDone)
                {
                    if (FLIM_ImgData.RoiFit.flim_parameters.fitCurve[ch] != null)
                        pp.addData(FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[ch], FLIM_ImgData.RoiFit.flim_parameters.fitCurve[ch], fitPen, "-");
                    if (roi_plot && roi_standard)
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
            fileformat = FileIO.FileFormat.None;
            FileIO.FileError file_error = FileIO.SetupFLIMOpeningDlog(lastFilePath, out int nPages, out String fileName);

            if (file_error == FileIO.FileError.TextFile)
            {
                fileformat = FileIO.FileFormat.Photon_file;
                file_error = FileIO.FileError.Success;
            }

            if (file_error == FileIO.FileError.Success)
            {
                FLIM_ImgData.fileExtension = Path.GetExtension(fileName);
                if (fileformat == FileIO.FileFormat.Photon_file)
                    flimage.flimage_io.State.Acq.photon_file_format = true;

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
            //image_display_state = ImageDisplay_State.Opening;
            flimage.flimage_io.Notify(new ProcessEventArgs("OpenFileStart", null));
            int[][] preOpenFitRange = CaptureFitRangeFromGui(false);
            fittingDone = false;
            LifetimeCurvePlot.Invalidate();

            var fIO = new FileIO(FLIM_ImgData.State);
            var saveState = fIO.CopyState();

            string oldBaseName = FLIM_ImgData.baseName;

            if (Path.GetExtension(fn).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                // Fiber photometry CSV (no image)
                lastFilePath = fn;
                return OpenFiberPhotometryCsv(fn);
            }

            if (Path.GetExtension(fn) == ".phtn" || Path.GetExtension(fn) == ".photon")
                fileformat = FileIO.FileFormat.Photon_file;
            else if (Path.GetExtension(fn) == ".flim")
            {
                fileformat = FileIO.FileFormat.ZLinear; //Temporary put this. 
            }

            if (fileformat == FileIO.FileFormat.Photon_file)
            {
                readPhotonFiles(fn, true);
                return FileIO.FileError.Success;
            }

            int nPages = FileIO.SetupFLIMOpening(fn, out string Header);

            bool saveProjection = cb_projectionYes.Checked;
            int saveCurrentPage = FLIM_ImgData.currentPage;

            lastFilePath = fn;

            if (nPages < 0)
                return FileIO.FileError.UnKnown;

            if (fn.Length == 0)
                return FileIO.FileError.NotFound;

            bool KeepPagesInMemory = flimage.KeepPagesInMemoryCheck.Checked;
            bool openZLinearAs5D = ShouldOpenZLinearAs5D(fn, Header, nPages);
            bool headerZStackTimeSeries = IsHeaderZStackStoredAsTimeSeries(Header, nPages);
            FLIM_ImgData.KeepPagesInMemory = KeepPagesInMemory;

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

            bool OpenSecondFile = false;
            string fn2 = "";


            //This will open the file ---- and initialized. ROIs associated with FLIM_ImgData is preserved.
            ret = FileIO.OpenFLIMTiffFilePage(fn, 0, 0, FLIM_ImgData, true, KeepPagesInMemory); //Save In Mmeory for the first page --- we don't know the format yet. 


            ApplyFitRangeFromGui(preOpenFitRange);
            SetFitting_Param(false);

            if (FLIM_ImgData.nFastZ > 1 || openZLinearAs5D)
                FLIM_ImgData.resizePage5D(nPages);
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
                            ret = FileIO.OpenFLIMTiffFilePage(fn2, 0, 0, FLIM_ImgData, false, KeepPagesInMemory);
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


            bool zLinearStackPages = FLIM_ImgData.format == FileIO.FileFormat.ZLinear
                && (openZLinearAs5D || FLIM_ImgData.imagesPerFile > 1);
            if (headerZStackTimeSeries)
            {
                FLIM_ImgData.State.Acq.ZStack = false;
                FLIM_ImgData.State.Acq.nSlices = 1;
                FLIM_ImgData.ZStack = false;
            }
            if (zLinearStackPages)
            {
                FLIM_ImgData.State.Acq.ZStack = true;
                FLIM_ImgData.State.Acq.nSlices = Math.Max(FLIM_ImgData.State.Acq.nSlices, FLIM_ImgData.imagesPerFile);
                if (FLIM_ImgData.State.Acq.sliceStep == 0)
                    FLIM_ImgData.State.Acq.sliceStep = 1;
            }

            ZStack = !headerZStackTimeSeries
                && FLIM_ImgData.State.Acq.ZStack
                && (FLIM_ImgData.State.Acq.nSlices > 1 || zLinearStackPages)
                && (FLIM_ImgData.State.Acq.sliceStep != 0 || zLinearStackPages);
            FLIM_ImgData.ZStack = ZStack;
            FastZStack = FLIM_ImgData.State.Acq.fastZScan && FLIM_ImgData.State.Acq.FastZ_nSlices > 0;
            bool openingZStackTimePages = ZStack && !FastZStack && openZLinearAs5D && FLIM_ImgData.imagesPerFile > 1 && nPages > 1;

            if (!ZStack && !FastZStack)
            {
                displayZProjection = false;
                this.InvokeIfRequired(o => o.cb_projectionYes.Checked = false);
            }
            else
            {
                displayZProjection = saveProjection;
                this.InvokeIfRequired(o => o.cb_projectionYes.Checked = displayZProjection);
            }

            this.InvokeIfRequired(o => o.SetFastZModeDisplay(FastZStack || openingZStackTimePages));

            //if (FLIM_ImgData.fileCounter == 1 && (ZStack || FastZStack))
            //{
            //    this.InvokeIfRequired(o => o.cb_projectionYes.Checked = true);
            //    displayZProjection = true;
            //}

            //Uncaging revcovery
            uncagingLocFrac = (double[])FLIM_ImgData.State.Uncaging.Position.Clone();

            uncagingLocs.Clear();
            uncagingLocs_calib.Clear();

            if (FLIM_ImgData.State.Uncaging.multiUncagingPosition)
            {
                for (int j = 0; j < FLIM_ImgData.State.Uncaging.UncagingPositionsX.Length; j++)
                {
                    if (j < FLIM_ImgData.State.Uncaging.UncagingPositionsY.Length)
                        uncagingLocs.Add(new double[] { FLIM_ImgData.State.Uncaging.UncagingPositionsX[j], FLIM_ImgData.State.Uncaging.UncagingPositionsY[j] });
                }
            }

            FLIM_ImgData.ResetFitting(true);
            fittingDone = false;

            file_nAverage = (int[])FLIM_ImgData.nAveragedFrame.Clone();
            FLIM_ImgData.nAveragedFrame = save_nAverage;
            ApplyTextToRange(false);
            FLIM_ImgData.nAveragedFrame = file_nAverage;

            SetupOpenedFile();
            if (!FLIM_ImgData.KeepPagesInMemory)
            {
                int pageToLoad = FLIM_ImgData.currentPage >= 0 ? FLIM_ImgData.currentPage : 0;
                GotoPage4D(pageToLoad);
            }

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

            for (int i = 0; i < nPages; i++)
            {
                if (openingZStackTimePages)
                {
                    if (KeepPagesInMemory || i > 0)
                    {
                        ret = FileIO.OpenFLIMTiffFilePage(fn, i, i, FLIM_ImgData, false, KeepPagesInMemory);
                        if (OpenSecondFile)
                            ret = FileIO.OpenFLIMTiffFilePage(fn2, i, i, FLIM_ImgData, false, KeepPagesInMemory);
                    }

                    if (ret != FileIO.FileError.Success)
                        return ret;

                    if (i >= 0 && i < nPages && nPages > 1)
                    {
                        GotoPage5D(i);
                        setupImageUpdateForZStack();
                        FLIM_ImgData.State.Display = saveState.Display;
                        UpdateImages(true, false, false, true, true);

                        if (calcTimeCourse)
                        {
                            CalculateCurrentPage(i, false);
                            plot_regular.updatePlot();
                            SaveTimeCourse();
                        }

                        if (!KeepPagesInMemory)
                            break;
                    }
                }
                else if (ZStack && !FastZStack)
                {
                    if (i < nPages)
                    {
                        ret = FileIO.OpenFLIMTiffFilePage(fn, i, i, FLIM_ImgData, false, KeepPagesInMemory);
                        if (OpenSecondFile)
                            ret = FileIO.OpenFLIMTiffFilePage(fn2, i, i, FLIM_ImgData, false, KeepPagesInMemory);
                    }

                    if (ret != FileIO.FileError.Success)
                        return ret;

                    if (!KeepPagesInMemory)
                        break;
                }
                else if (FastZStack)
                {
                    if (i < nPages)
                    {
                        ret = FileIO.OpenFLIMTiffFilePage(fn, i, i, FLIM_ImgData, false, KeepPagesInMemory);
                        if (OpenSecondFile)
                            ret = FileIO.OpenFLIMTiffFilePage(fn2, i, i, FLIM_ImgData, false, KeepPagesInMemory);
                    }

                    if (i >= 0 && i < nPages && nPages > 1)
                    {
                        GotoPage5D(i);
                        setupImageUpdateForZStack();
                        FLIM_ImgData.State.Display = saveState.Display;
                        UpdateImages(true, false, false, true, true);

                        if (calcTimeCourse)
                        {
                            CalculateCurrentPage(i, false);
                            plot_regular.updatePlot();
                            SaveTimeCourse();
                        }

                        if (!KeepPagesInMemory)
                            break;

                    }
                }
                else //!ZStack.
                {

                    ret = FileIO.OpenFLIMTiffFilePage(fn, i, i, FLIM_ImgData, false, KeepPagesInMemory);
                    if (OpenSecondFile)
                        ret = FileIO.OpenFLIMTiffFilePage(fn2, i, i, FLIM_ImgData, false, KeepPagesInMemory);


                    if (ret != FileIO.FileError.Success)
                        return ret;

                    if (i >= 0 && i < nPages && nPages >= 1)
                    {
                        GotoPage4D(i);
                        UpdateImages(true, false, false, true);
                        FLIM_ImgData.State.Display = saveState.Display;

                        if (calcTimeCourse)
                        {
                            CalculateCurrentPage(i, false);
                            plot_regular.updatePlot();
                            SaveTimeCourse();
                        }

                        if (!KeepPagesInMemory)
                            break;
                    }
                }

                UpdateImages(true, false, false, true, true);
                Application.DoEvents();

                if (StopFileOpening)
                {
                    if (KeepPagesInMemory)
                    {
                        if (FastZStack || openingZStackTimePages)
                            FLIM_ImgData.resizePage5D(i);
                        else
                            FLIM_ImgData.resizePage(i);
                    }
                    break;
                }
            } //End page

            bool zStackTimePages = openingZStackTimePages
                && FLIM_ImgData.FLIM_Pages5D != null
                && FLIM_ImgData.n_pages5D > 0
                && FLIM_ImgData.FLIM_Pages5D.Length > 0
                && HasZStack5DPages();
            bool fiveDTimePages = FastZStack || zStackTimePages;
            if (fiveDTimePages)
                GotoPage5D(0);
            this.InvokeIfRequired(o => o.SetFastZModeDisplay(FastZStack || zStackTimePages));
            displayZProjection = saveProjection && (ZStack || FastZStack || zStackTimePages);
            this.InvokeIfRequired(o => o.cb_projectionYes.Checked = displayZProjection);

            FLIM_ImgData.ZProjection_Range[0] = 0;

            if (!FastZStack)
                FLIM_ImgData.ZProjection_Range[1] = zStackTimePages ? GetCurrent5DZSliceCount() : FLIM_ImgData.n_pages;
            else
                FLIM_ImgData.ZProjection_Range[1] = FLIM_ImgData.nFastZ;

            if (!ZStack && !FastZStack) //Time course. Show the first image.
                GotoPage4D(0);

            this.InvokeIfRequired(o => o.PageEnableControl());

            if (ZStack || FastZStack)
            {
                if (displayZProjection)
                    calcZProjection();
                else if (fiveDTimePages)
                {
                    GotoPage5D(0);
                    GotoPage4D(0);
                }
                else
                {
                    // Kengo 05-30-2025
                    // fix the condition for reset of saveCurrentPage
                    //if (saveCurrentPage < FLIM_ImgData.n_pages)
                    if (saveCurrentPage > FLIM_ImgData.n_pages - 1)
                        saveCurrentPage = 0;

                    GotoPage4D(saveCurrentPage);
                }
            }

            FLIM_ImgData.State.Display = saveState.Display;
            ApplyFitRangeFromGui(preOpenFitRange);
            SetFitting_Param(false);
            fitRangeTextEdited = false;
            UpdateImages(true, false, false, true);

            // Opening a multi-page time course fits every page in sequence.
            // Recalculate the displayed page once at the end so the shared offset
            // matches the page we show instead of the last page processed.
            if (calcTimeCourse && !ZStack && !FastZStack && nPages > 1)
                CalculateCurrentPage(false);

            if (TuningOnStopOpeningButton)
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

            if (calcTimeCourse && (ZStack || nPages == 1) && !(zStackTimePages && !FLIM_ImgData.KeepPagesInMemory))
                CalculateTimecourse(false); //Include the saving.

            this.InvokeIfRequired(o => o.displayFastZParam()); //only if FastZStack == true. Included in the function.
            this.InvokeIfRequired(o => o.TurnOnOffThreeD(ThreeDRoi));
            this.InvokeIfRequired(o =>
            {
                o.toggleZStackAndTimecourseToolStripMenuItem.Text = o.ZStack ? ZStack_Text[1] : ZStack_Text[0];
            });

            flimage.flimage_io.Notify(new ProcessEventArgs("ReadFileDone", null));
            //image_display_state = ImageDisplay_State.Idle;

            try
            {
                PostOpenUserFunction();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Problem in Post Open User Function:" + e.Message);
            }

            if (flimage?.phasor_plot_window != null &&
                !flimage.phasor_plot_window.IsDisposed)
                flimage.phasor_plot_window.CalculateAllPagesUponOpen(this);

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
                    // Let the UI paint before doing work, without using Application.DoEvents (avoids re-entrancy issues).
                    if (Interlocked.Exchange(ref _phaseCalcQueued, 1) == 0)
                    {
                        BeginInvoke((Action)(() =>
                        {
                            try
                            {
                                if (!IsDisposed)
                                    CalculatePhase();
                            }
                            finally
                            {
                                Interlocked.Exchange(ref _phaseCalcQueued, 0);
                            }
                        }));
                    }
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
            bool zStackTimePages = ZStack && !FastZStack && HasZStack5DPages();

            if (page < 0)
                page = 0;
            else if (page >= FLIM_ImgData.n_pages)
                page = FLIM_ImgData.n_pages - 1;

            if (!ZStack && !FastZStack)
            {
                if (IsLineScanTimeAxisMode())
                {
                    CalculateLineScanCurrentPage(page);
                }
                else
                {
                    CalculateCurrentPage(page, true);
                }
                SaveTimeCourse();
            }
            else if (zStackTimePages)
            {
                int page5D = FLIM_ImgData.currentPage5D;
                if (page5D < 0)
                    page5D = 0;
                else if (page5D >= FLIM_ImgData.n_pages5D)
                    page5D = FLIM_ImgData.n_pages5D - 1;

                GotoPage5D(page5D);
                if (displayZProjection)
                    calcZProjection();
                else
                    GotoPage4D(FLIM_ImgData.currentPage);
                UpdateImages(true, false, false, true);

                CalculateCurrentPage(page5D, true);
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
        public void CalculateCurrentPage(int page, bool update_image)
        {
            realtime = false;

            if (IsLineScanTimeAxisMode() && !ZStack && !FastZStack)
            {
                CalculateLineScanCurrentPage(page);
                return;
            }

            double[] offsetC = FLIM_ImgData.offset;

            //FLIM_ImgData.ResetLifetimeCalculation(false);

            FLIM_ImgData.use_mask_for_intensity = plot_regular.mask_on;

            if (plot_regular.calc_Fit)
            {
                double[] fit_offset = (double[])FLIM_ImgData.offset;

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

                if (FLIM_ImgData.State.Acq.acqFLIMA.Any(x => x == true))
                    fit_offset = FitData(false, true, page);

                if (AutoApplyOffset.Checked)
                    offsetC = fit_offset;
            }


            FLIM_ImgData.calculate_MeanLifetime(page, offsetC); //This calculates mean lifetime (without fitting).

            //FLIM_ImgData.Fit_type = ft;

            ImageInfo info = new ImageInfo(FLIM_ImgData);
            if (!TC.AddFile(info, page)) //If new file, it will create a new Time course.
            {
                TC = new TimeCourse();
                TC.AddFile(info, page);
            }

            if (AutoApplyOffset.Checked)
            {
                FLIM_ImgData.offset = (double[])offsetC.Clone();
                FLIM_ImgData.State.Spc.analysis.offset = (double[])FLIM_ImgData.offset.Clone();
            }
            fittingDone = true;
            TCF.AddFile(TC);
            TCF.calculate();
            plot_regular.plotNow_noRealtime(TCF, TC, this, currentChannel);

            if (update_image)
            {
            }
        }

        private void CalculateLineScanCurrentPage(int page)
        {
            if (page < 0)
                page = 0;
            if (page >= FLIM_ImgData.n_pages)
                page = FLIM_ImgData.n_pages - 1;

            double lineTimeMs = FLIM_ImgData.State.Acq.msPerLineActual();
            if (lineTimeMs <= 0 && FLIM_ImgData.State.Acq.fiberPhotometryMode && FLIM_ImgData.State.Acq.fiberBin_ms > 0)
                lineTimeMs = FLIM_ImgData.State.Acq.fiberBin_ms;
            int linesPerFrame = Math.Max(1, Math.Max(FLIM_ImgData.State.Acq.linesPerFrame, FLIM_ImgData.height));
            int totalTimePoints = FLIM_ImgData.n_pages * linesPerFrame;
            if (FLIM_ImgData.State.Acq.LineScanTimePoints > 0)
                totalTimePoints = Math.Max(FLIM_ImgData.State.Acq.LineScanTimePoints, totalTimePoints);

            bool sameFile = TC != null && string.Equals(TC.FileName, FLIM_ImgData.fileName, StringComparison.OrdinalIgnoreCase);
            if (sameFile && TC.nData > 0)
                totalTimePoints = Math.Max(totalTimePoints, TC.nData);

            bool newFile = TC == null || !sameFile;
            if (newFile)
            {
                TC = new TimeCourse();
                TC.InitializeLineScan(FLIM_ImgData, totalTimePoints);
            }

            GotoPage4D(page);
            var frameData = FLIM_ImgData.FLIMRaw;
            if (frameData == null || frameData.Length == 0)
                return;

            ushort[,,] firstArr = null;
            for (int ch = 0; ch < frameData.Length; ch++)
            {
                if (frameData[ch] != null)
                {
                    firstArr = frameData[ch];
                    break;
                }
            }
            if (firstArr == null)
                return;

            DateTime baseTime = FLIM_ImgData.acquiredTime;
            if (FLIM_ImgData.acquiredTime_Pages != null && page < FLIM_ImgData.acquiredTime_Pages.Length)
                baseTime = FLIM_ImgData.acquiredTime_Pages[page];

            double baseTimeMs = (baseTime - ImageInfo.startDate).TotalMilliseconds;
            int linesInFrame = firstArr.GetLength(0);
            int lineIndexStart = page * linesInFrame;
            if (lineIndexStart >= totalTimePoints)
                return;

            int maxLines = Math.Min(linesInFrame, totalTimePoints - lineIndexStart);
            if (!plot_regular.calc_Fit)
                TC.ClearLineScanFixedFit();
            if (plot_regular.calc_Fit && !TC.HasLineScanFixedFit())
            {
                TC.PrepareLineScanFit(FLIM_ImgData, cb_tau1Fix.Checked, cb_tau2Fix.Checked, cb_tauGFix.Checked, cb_T0Fix.Checked, Fit_BG.Checked);
                TC.AccumulateLineScanGlobalDecay(frameData, maxLines);
                TC.FinalizeLineScanFixedFit(FLIM_ImgData);
            }
            TC.AddLineScanFrame(FLIM_ImgData, frameData, lineIndexStart, maxLines, baseTimeMs, lineTimeMs);
            TC.FinalizeLineScan();

            TCF.AddFile(TC);
            TCF.calculate();
            plot_regular.InvokeIfRequired(o => o.plotNow_noRealtime(TCF, TC, this, currentChannel));
            if (plot_regular.calc_Fit)
            {
                FitData(false, false, -1);
                fittingDone = true;
                this.InvokeIfRequired(o => o.UpdateFittingParam(currentChannel, fittingDone));
                LifetimeCurvePlot.Invalidate();
            }
        }

        /// <summary>
        /// Calculate the time course of the current file.
        /// </summary>
        public void CalculateTimecourse(bool turning_onoff_stop)
        {

            FastZStack = FLIM_ImgData.nFastZ > 1;
            bool zStackTimePages = !FastZStack && HasZStack5DPages();

            int nPages = FLIM_ImgData.n_pages;
            int savePage = FLIM_ImgData.currentPage;

            if (FastZStack)
                nPages = FLIM_ImgData.n_pages5D;
            else if (zStackTimePages)
                nPages = FLIM_ImgData.n_pages5D;

            if (turning_onoff_stop)
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(false));

            if (IsLineScanTimeAxisMode() && !ZStack && !FastZStack)
            {
                CalculateLineScanTimecourse(turning_onoff_stop, nPages, savePage);
                return;
            }

            //FLIM_ImgData.ResetLifetimeCalculation(false);

            if (ZStack && !zStackTimePages)
            {
                nPages = 1;
                //savePage = 0;
            }

            if (FastZStack || zStackTimePages)
            {
                setupImageUpdateForZStack();
                UpdateImages(true, false, false, true);
            }

            if (ZStack || FastZStack)
                for (int i = 0; i < nPages; i++)
                {
                    if (FastZStack || zStackTimePages)
                    {
                        GotoPage5D(i);
                        FLIM_ImgData.currentPage = savePage;
                        if (displayZProjection)
                            calcZProjection();
                        else
                            GotoPage4D(FLIM_ImgData.currentPage);
                        UpdateImages(true, false, false, true);
                        CalculateCurrentPage(i, true);
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

                if (FLIM_ImgData.ROIs.Count > 0)
                {
                    TurnOnMultiROI(true);
                    FLIM_ImgData.Fit_type = FLIMData.FitType.GlobalRois;
                }
                else
                    FLIM_ImgData.Fit_type = FLIMData.FitType.SelectedRoi;

                FLIM_ImgData.InitializeAllROI_FlimParameters_Pages();
                for (int i = 0; i < nPages; i++)
                {
                    GotoPage4D(i);

                    if (!FLIM_ImgData.KeepPagesInMemory)
                        UpdateImages(true, realtime, focusing, true, true);

                    CalculateCurrentPage(i, true);

                    if (FLIM_ImgData.KeepPagesInMemory)
                    {
                        FLIM_ImgData.RoiFit.transfer_FittingParameterFromPage(i);
                        UpdateImages(true, false, false, true);
                    }

                    this.InvokeIfRequired(o => o.c_page.Text = i.ToString());
                    Application.DoEvents();
                    if (StopFileOpening)
                        break;
                }
            }


            TCF.AddFile(TC);
            TCF.calculate();
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

        private void CalculateLineScanTimecourse(bool turning_onoff_stop, int nPages, int savePage)
        {
            double lineTimeMs = FLIM_ImgData.State.Acq.msPerLineActual();
            if (lineTimeMs <= 0 && FLIM_ImgData.State.Acq.fiberPhotometryMode && FLIM_ImgData.State.Acq.fiberBin_ms > 0)
                lineTimeMs = FLIM_ImgData.State.Acq.fiberBin_ms;
            int linesPerFrame = Math.Max(1, Math.Max(FLIM_ImgData.State.Acq.linesPerFrame, FLIM_ImgData.height));
            int totalTimePoints = nPages * linesPerFrame;
            if (FLIM_ImgData.State.Acq.LineScanTimePoints > 0)
                totalTimePoints = Math.Max(FLIM_ImgData.State.Acq.LineScanTimePoints, totalTimePoints);

            TC = new TimeCourse();
            TC.InitializeLineScan(FLIM_ImgData, totalTimePoints);
            TCF.AddFileReference(TC);

            int lineIndex = 0;
            bool doFit = plot_regular.calc_Fit;
            bool fitDisplayed = false;
            if (doFit)
                fittingDone = false;

            if (!doFit)
                TC.ClearLineScanFixedFit();

            if (doFit)
            {
                bool preparedFit = false;
                for (int i = 0; i < nPages; i++)
                {
                    if (lineIndex >= totalTimePoints)
                        break;

                    GotoPage4D(i);

                    var frameData = FLIM_ImgData.FLIMRaw;
                    if (frameData == null || frameData.Length == 0)
                        continue;

                    ushort[,,] firstArr = null;
                    for (int ch = 0; ch < frameData.Length; ch++)
                    {
                        if (frameData[ch] != null)
                        {
                            firstArr = frameData[ch];
                            break;
                        }
                    }
                    if (firstArr == null)
                        continue;

                    if (!preparedFit)
                    {
                        TC.PrepareLineScanFit(FLIM_ImgData, cb_tau1Fix.Checked, cb_tau2Fix.Checked, cb_tauGFix.Checked, cb_T0Fix.Checked, Fit_BG.Checked);
                        preparedFit = true;
                    }

                    int linesInFrame = firstArr.GetLength(0);
                    int maxLines = Math.Min(linesInFrame, totalTimePoints - lineIndex);
                    TC.AccumulateLineScanGlobalDecay(frameData, maxLines);

                    lineIndex += maxLines;
                    this.InvokeIfRequired(o => o.c_page.Text = i.ToString());
                    Application.DoEvents();
                    if (StopFileOpening)
                        break;
                }

                if (preparedFit)
                    TC.FinalizeLineScanFixedFit(FLIM_ImgData);
            }

            lineIndex = 0;
            for (int i = 0; i < nPages; i++)
            {
                if (lineIndex >= totalTimePoints)
                    break;

                GotoPage4D(i);

                var frameData = FLIM_ImgData.FLIMRaw;
                if (frameData == null || frameData.Length == 0)
                    continue;

                ushort[,,] firstArr = null;
                for (int ch = 0; ch < frameData.Length; ch++)
                {
                    if (frameData[ch] != null)
                    {
                        firstArr = frameData[ch];
                        break;
                    }
                }
                if (firstArr == null)
                    continue;

                DateTime baseTime = FLIM_ImgData.acquiredTime;
                if (FLIM_ImgData.acquiredTime_Pages != null && i < FLIM_ImgData.acquiredTime_Pages.Length)
                    baseTime = FLIM_ImgData.acquiredTime_Pages[i];

                double baseTimeMs = (baseTime - ImageInfo.startDate).TotalMilliseconds;
                int linesInFrame = firstArr.GetLength(0);
                int maxLines = Math.Min(linesInFrame, totalTimePoints - lineIndex);

                for (int line = 0; line < maxLines; line++)
                {
                    TC.AddLineScanFrame(FLIM_ImgData, frameData, lineIndex, 1, baseTimeMs + line * lineTimeMs, lineTimeMs, line);
                    lineIndex++;
                }

                TCF.calculate();
                plot_regular.InvokeIfRequired(o => o.plotNow_noRealtime(TCF, TC, this, currentChannel));

                if (doFit)
                {
                    FitData(false, false, -1);
                    if (!fitDisplayed)
                    {
                        fittingDone = true;
                        fitDisplayed = true;
                        this.InvokeIfRequired(o => o.UpdateFittingParam(currentChannel, fittingDone));
                        LifetimeCurvePlot.Invalidate();
                    }
                }

                UpdateImages(true, false, false, true);

                this.InvokeIfRequired(o => o.c_page.Text = i.ToString());
                Application.DoEvents();
                if (StopFileOpening)
                    break;

                if (StopFileOpening)
                    break;
            }

            TC.TrimLineScan(lineIndex);
            TC.FinalizeLineScan();

            TCF.AddFile(TC);
            TCF.calculate();
            plot_regular.InvokeIfRequired(o => o.plotNow_noRealtime(TCF, TC, this, currentChannel));

            GotoPage4D(savePage);
            UpdateImages(true, false, false, true);

            if (doFit)
            {
                FitData(false, false, -1);
                fittingDone = true;
                this.InvokeIfRequired(o => o.UpdateFittingParam(currentChannel, fittingDone));
                LifetimeCurvePlot.Invalidate();
            }

            if (turning_onoff_stop)
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

            try
            {
                SaveTimeCourse();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Problem in saving time course:" + e.Message);
            }
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
                FrameSlicePanel.Text = FastZStack ? "Fast Z Stack" : "Z Stack Time Series";
                FastZPanel.Visible = true;


                if (FLIM_ImgData.currentPage < 0 && FLIM_ImgData.nFastZ > 1)
                {
                    if (!FLIM_ImgData.State.Acq.FastZ_phase_detection_mode)
                        FLIM_ImgData.currentPage = FLIM_ImgData.nFastZ / 2;
                    else
                        FLIM_ImgData.currentPage = FLIM_ImgData.nFastZ / 4;
                }
                else if (FLIM_ImgData.currentPage < 0 && HasZStack5DPages())
                {
                    FLIM_ImgData.currentPage = 0;
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

            int page = FLIM_ImgData.currentPage5D;
            if (page < 0)
                page = 0;

            if (sender.Equals(FastZUp))
            {
                if (page < FLIM_ImgData.n_pages5D - 1)
                    page++;
            }
            else
            {
                if (page > 0)
                    page--;
            }

            GotoPage5D(page);

            CurrentFastZPageTB.Text = (FLIM_ImgData.currentPage5D + 1).ToString();

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
            CalculatePhasorCurrentPageUponOpen();

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
                //Kengo BEGIN 1-9-2024
                //Change the lower limit of the FileDown button from 1 to 0.
                //if (saveCounter == 1)
                if (saveCounter == 0)
                //Kengo END
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

            bool file_exist = File.Exists(fn);
            if (!file_exist)
            {
                fn = FLIM_ImgData.State.Files.fromFullNameToPhotonFilePath(fn, true);
                file_exist = File.Exists(fn);

            }

            bool success = false;

            if (file_exist)
            {
                try
                {
                    success = OpenFLIM(fn, true, plot_regular.calc_upon_open, false) == FileIO.FileError.Success;
                }
                catch
                {
                    MessageBox.Show("Problem in opening file");
                }
            }

            if (!success || !file_exist)
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
            if (FLIM_ImgData.ZProjection_Range[1] <= FLIM_ImgData.ZProjection_Range[0]) // Kengo 05-30-2025 add "="
            {
                int tmp = FLIM_ImgData.ZProjection_Range[0] + 1; // Kengo 05-30-2025 add "+1"
                FLIM_ImgData.ZProjection_Range[0] = FLIM_ImgData.ZProjection_Range[1] - 1; // Kengo 05-30-2025 add "-1"
                FLIM_ImgData.ZProjection_Range[1] = tmp;
            }

            if (FLIM_ImgData.ZProjection_Range[0] < 0)
                FLIM_ImgData.ZProjection_Range[0] = 0;

            // Kengo BEGIN 05-30-2025
            // add a condition so that it is not less than 1
            if (FLIM_ImgData.ZProjection_Range[1] < 1)
                FLIM_ImgData.ZProjection_Range[1] = 1;
            // Kengo END

            int n_pages = FLIM_ImgData.n_pages;
            if (FLIM_ImgData.nFastZ > 1)
                n_pages = FLIM_ImgData.nFastZ;

            // Kengo BEGIN 05-30-2025
            // add a condition so that it is not more than n_pages
            if (FLIM_ImgData.ZProjection_Range[0] > n_pages - 1)
                FLIM_ImgData.ZProjection_Range[0] = n_pages - 1;
            // Kengo END

            if (FLIM_ImgData.ZProjection_Range[1] > n_pages)
                FLIM_ImgData.ZProjection_Range[1] = n_pages;
        }

        private void UpdateImageParamText()
        {
            ScanParameters State = FLIM_ImgData.State;
            MapStateIntensityRange();
            filterWindow.Text = FLIM_ImgData.State.Display.filterWindow_FLIM.ToString();

            int[] aveNFrame = (int[])FLIM_ImgData.nAveragedFrame.Clone();

            // Tetsuya BEGIN 06-29-2026
            // Slider max uses FOCUS frame count during FOCUS
            // (previously: if (displayZProjection && AveProjection.Checked) immediately after clone)
            if (IsFocusingForIntensityRange())
            {
                for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                    aveNFrame[ch] = (int)GetEffectiveFrameCountForIntensityRange(ch);
            }
            else if (displayZProjection && AveProjection.Checked)
            {
                AssurePageRange();
                if (FLIM_ImgData.ZProjection_Range[1] - FLIM_ImgData.ZProjection_Range[0] > 0)
                {
                    for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                        aveNFrame[ch] = aveNFrame[ch] * (FLIM_ImgData.ZProjection_Range[1] - FLIM_ImgData.ZProjection_Range[0] + 1);
                }

            }
            // Tetsuya END

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
                        if (aveNFrame[cl] == 0)
                            aveNFrame[cl] = 1;
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


            if (thresholdFLIM_low_high[currentChannel][1] > 0)
                highThreshFLIM.Text = thresholdFLIM_low_high[currentChannel][1].ToString();
            else
                highThreshFLIM.Text = "INF";

            if (ThreshCB.Checked)
            {
                lowThreshFLIM.Text = State_FLIM_intensity_range[currentChannel][0].ToString();
                lowThreshFLIM.Enabled = false;
            }
            else
                lowThreshFLIM.Enabled = true;

            // Save setting
            SaveUISetting();
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

        private void phasorAnalysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flimage?.ShowPhasorPlotWindow(this);
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
                //Kengo BEGIN 1-9-2024
                //Change the lower limit of the FileDown button from 1 to 0.
                //if (valI > 0)
                if (valI >= 0)
                //Kengo END
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
            if (IsFiberPhotometryMode())
                return;

            int bytePerPixel = 3;

            pixelsBuffer = MatrixCalc.MatrixCreate2D<byte>(nChannels, width * bytePerPixel * height); //new byte[width * bytePerPixel * height];

            // Tetsuya BEGIN 06-29-2026
            // MapStateIntensityRange();
            RefreshIntensityRangeForDisplay();
            // Tetsuya END
            UInt16[][] ProjectBlack = MatrixCalc.MatrixCreate2D<UInt16>(height, width);

            lock (syncBmp[0])
            {
                if (flimage.State.Acq.acquisition[0])
                    IntensityBitmap = ImageProcessing.FormatImage(State_intensity_range[0], thresholdFLIM_low_high[0], ProjectBlack);
                else
                    IntensityBitmap = null;
            }
            lock (syncBmp[1])
            {
                if (flimage.State.Acq.acquisition[1])
                    IntensityBitmap2 = ImageProcessing.FormatImage(State_intensity_range[1], thresholdFLIM_low_high[1], ProjectBlack);
                else
                    IntensityBitmap2 = null;
            }

            EnsureFocusProjectBuffer(nChannels, height, width);
        }

        private void EnsureFocusProjectBuffer(int nChannels, int height, int width)
        {
            if (nChannels <= 0 || height <= 0 || width <= 0)
                return;

            if (FocusProjectBuffer == null || FocusProjectBuffer.Length != nChannels)
                FocusProjectBuffer = new ushort[nChannels][,];

            for (int i = 0; i < nChannels; i++)
            {
                if (FocusProjectBuffer[i] == null ||
                    FocusProjectBuffer[i].GetLength(0) != height ||
                    FocusProjectBuffer[i].GetLength(1) != width)
                {
                    FocusProjectBuffer[i] = new ushort[height, width];
                }
            }
        }

        public void ImportState(ScanParameters State_in)
        {
            FileIO fo = new FileIO(State_in);
            ScanParameters State = fo.CopyState();
            FLIM_ImgData.State.Display = State.Display;
            FLIM_ImgData.State.Spc.analysis = State.Spc.analysis;
            currentChannel = 0;
            //UpdateFittingParam(currentChannel, false);
            //UpdateImageParamText();
            //ApplyTextToRange(true);
            //UpdateImages(true, realtime, focusing, true, true);
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
            if (IsFiberPhotometryMode())
                return;

            if (img == null || img.Length == 0 || img[0] == null || ch < 0 || ch >= img[0].Length || img[0][ch] == null)
                return;

            int imgHeight = img[0][ch].GetLength(0);
            int imgWidth = img[0][ch].GetLength(1);
            int bufferChannels = Math.Max(FLIM_ImgData.nChannels, ch + 1);
            EnsureFocusProjectBuffer(bufferChannels, imgHeight, imgWidth);

            // Tetsuya BEGIN 06-29-2026
            // MapStateIntensityRange();
            RefreshIntensityRangeForDisplay();
            // Tetsuya END
            int[] t_range = FLIM_ImgData.fit_range[ch];

            if (EndLine > StartLine)
            {
                if (ch == 0)
                {
                    lock (syncBmp[0])
                    {
                        if (IntensityBitmap == null || FocusProjectBuffer == null || FocusProjectBuffer.Length <= ch || FocusProjectBuffer[ch] == null)
                            return;
                        ImageProcessing.GetProjectFromFLIMLines(img[0][ch], FocusProjectBuffer[ch], t_range, StartLine, EndLine);
                        ImageProcessing.FormatImageLines(IntensityBitmap, thresholdFLIM_low_high[ch], State_intensity_range[ch], FocusProjectBuffer[ch], StartLine, EndLine);
                    }
                }
                else if (ch == 1)
                {
                    lock (syncBmp[1])
                    {
                        if (IntensityBitmap2 == null || FocusProjectBuffer == null || FocusProjectBuffer.Length <= ch || FocusProjectBuffer[ch] == null)
                            return;
                        ImageProcessing.GetProjectFromFLIMLines(img[0][ch], FocusProjectBuffer[ch], t_range, StartLine, EndLine);
                        ImageProcessing.FormatImageLines(IntensityBitmap2, State_intensity_range[ch], thresholdFLIM_low_high[ch], FocusProjectBuffer[ch], StartLine, EndLine);
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
            //if (!FLIM_ImgData.ZProjection && FLIM_ImgData.z_projection_type == FLIMData.projectionType.Sum)
            if (FLIM_ImgData.z_projection_type == FLIMData.projectionType.Sum)
                adjustSumProjection = FLIM_ImgData.ZProjection_Range[1] - FLIM_ImgData.ZProjection_Range[0]; // FLIM_ImgData.n_pages;
            return adjustSumProjection;
        }

        // Tetsuya BEGIN 06-29-2026
        // FOCUS intensity range: sync flimage_io.focusing and apply correct frame count
        // (frame-count logic was previously inline in LoadIntensity_Range / SaveIntensity_Range)
        private bool IsFocusingForIntensityRange()
        {
            if (flimage?.flimage_io != null)
                return flimage.flimage_io.focusing;
            return focusing;
        }

        private double GetEffectiveFrameCountForIntensityRange(int channel)
        {
            var state = FLIM_ImgData.State;
            double nAveragedFrame;

            if (IsFocusingForIntensityRange())
            {
                if (state.Acq.resonantScanning || state.Acq.polygonScanning)
                    nAveragedFrame = state.Acq.nAveFrame_focus_resonant;
                else
                    nAveragedFrame = state.Acq.nAveFrame_focus;
            }
            else
            {
                nAveragedFrame = FLIM_ImgData.nAveragedFrame.Max();
                if (channel >= 0 && channel < FLIM_ImgData.nChannels)
                    nAveragedFrame = FLIM_ImgData.nAveragedFrame[channel];

                if (!FLIM_ImgData.ZStack && state.Acq.nAveSlice > 1)
                    nAveragedFrame = nAveragedFrame * state.Acq.nAveSlice;
            }

            if (nAveragedFrame <= 0)
                nAveragedFrame = 1;
            return nAveragedFrame;
        }

        private void RefreshIntensityRangeForDisplay()
        {
            if (FrameAdjustment.Checked)
                LoadIntensity_Range();
            else
                MapStateIntensityRange();
        }
        // Tetsuya END

        public void LoadIntensity_Range()
        {
            double int1;

            if (!FrameAdjustment.Checked)
                return;

            MapStateIntensityRange();

            int adjustSumProjection = CalcAdjustSumProjection();

            for (int cl = 0; cl < intensity_range_perFrame.Length; cl++)
            {
                // Tetsuya BEGIN 06-29-2026
                // double nAveragedFrame = FLIM_ImgData.nAveragedFrame.Max();
                // if (FLIM_ImgData.nChannels > cl)
                //     nAveragedFrame = FLIM_ImgData.nAveragedFrame[cl];
                // if (!FLIM_ImgData.ZStack && FLIM_ImgData.State.Acq.nAveSlice > 1)
                //     nAveragedFrame = nAveragedFrame * FLIM_ImgData.State.Acq.nAveSlice;
                // if (focusing)
                //     nAveragedFrame = FLIM_ImgData.State.Acq.nAveFrame_focus;
                // if (nAveragedFrame <= 0)
                //     nAveragedFrame = 1;
                double nAveragedFrame = GetEffectiveFrameCountForIntensityRange(cl);
                // Tetsuya END

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

            // Tetsuya BEGIN 06-29-2026
            // double nAverageFrame = (double)FLIM_ImgData.nAveragedFrame.Max();
            // if (channel < FLIM_ImgData.nChannels)
            //     nAverageFrame = (double)FLIM_ImgData.nAveragedFrame[channel];
            // if (focusing)
            //     nAverageFrame = (double)FLIM_ImgData.State.Acq.nAveFrame_focus;
            // if (nAverageFrame <= 0)
            //     nAverageFrame = 1;
            double nAverageFrame = GetEffectiveFrameCountForIntensityRange(channel);
            // Tetsuya END

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
                double valD;

                for (int j = 0; j < N_RANGE; j++)
                {
                    if (Double.TryParse(MaxMinTextBox[2][j].Text, out valD))
                        State_FLIM_lifetime_range[c][j] = valD;
                }

                if (thresholdFLIM_low_high[currentChannel] == null)
                    thresholdFLIM_low_high[currentChannel] = new double[2];


                if (Double.TryParse(lowThreshFLIM.Text, out valD))
                    thresholdFLIM_low_high[c][0] = valD;


                if (Double.TryParse(highThreshFLIM.Text, out valD))
                    thresholdFLIM_low_high[c][1] = valD;
                else
                    thresholdFLIM_low_high[c][1] = -1;
            }


            int valI;
            int nPage = FLIM_ImgData.n_pages;

            if (Int32.TryParse(PageStart.Text, out valI) && valI <= nPage && valI > 0)
                FLIM_ImgData.ZProjection_Range[0] = valI - 1;

            if (Int32.TryParse(PageStart.Text, out valI) && valI <= nPage && valI > 0)
                FLIM_ImgData.ZProjection_Range[0] = valI - 1;

            if (Int32.TryParse(PageEnd.Text, out valI) && valI <= nPage && valI > 0)
                FLIM_ImgData.ZProjection_Range[1] = valI;

            if (FLIM_ImgData.ZProjection_Range[0] + 1 > FLIM_ImgData.ZProjection_Range[1]) // Kengo 05-30-2025 add "+ 1"
            {
                valI = FLIM_ImgData.ZProjection_Range[1];
                FLIM_ImgData.ZProjection_Range[0] = valI - 1;
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
            try
            {
                if (flimage?.flimage_io == null)
                    return;

                // Photon-file opening runs via FLIMage_IO (not the page loop), so we must explicitly abort it.
                if (flimage.flimage_io.read_photon_file)
                {
                    flimage.flimage_io.StopGrab(true);
                    flimage.flimage_io.read_photon_file = false;
                    finish_reading_photon_file();
                }
            }
            catch
            {
                // Best-effort: stop button should never crash the UI.
            }
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
                    int nBytes = 16384;
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
                //Kengo BEGIN 12-1-2023
                //refresh image display after reading ImageJ's ROI
                DrawImages();
                //Kengo END
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
            //else if (keyData == Keys.F)
            //{
            //    FlipImages();
            //}
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
        /// Line-scan trace mode: append realtime plot points from one acquired frame.
        /// Appends one point per line (time point), but refreshes the plot once per frame.
        /// </summary>
        public void UpdateRealtimePlotFromFrame(UInt16[][][,,] frameData)
        {
            try
            {
                if (frameData == null)
                    return;
                if (!realtime || plot_realtime == null || !plot_realtime.Visible)
                    return;

                // We use the currently selected display channel for the plot (consistent with UpdateImages()).
                int ch = currentChannel;
                if (Channel1.Checked) ch = 0;
                else if (Channel2.Checked) ch = 1;
                else if (Channel12.Checked)
                {
                    if (Ch1.Checked) ch = 0;
                    else if (Ch2.Checked) ch = 1;
                }

                var arr = frameData[0][ch]; // [y,x,t]
                if (arr == null)
                    return;

                int y0 = 0;
                int y1 = arr.GetLength(0);
                int width = arr.GetLength(1);
                int nT = arr.GetLength(2);

                int[] tRange = FLIM_ImgData?.fit_range != null && ch < FLIM_ImgData.fit_range.Length ? FLIM_ImgData.fit_range[ch] : null;
                int tStart = 0;
                int tEnd = nT;
                if (tRange != null && tRange.Length >= 2)
                {
                    tStart = Math.Max(0, Math.Min(nT, tRange[0]));
                    tEnd = Math.Max(tStart, Math.Min(nT, tRange[1]));
                }

                double res_ps = 250.0;
                try { res_ps = FLIM_ImgData.State.Spc.spcData.resolution[ch]; } catch { }

                var points = new List<double>(Math.Max(0, y1 - y0));
                for (int y = y0; y < y1; y++)
                {
                    double sum = 0.0;
                    double wsum_ps = 0.0;
                    for (int x = 0; x < width; x++)
                    {
                        for (int t = tStart; t < tEnd; t++)
                        {
                            ushort v = arr[y, x, t];
                            sum += v;
                            wsum_ps += v * (t * res_ps);
                        }
                    }

                    if (plot_realtime.plotType == plot_timeCourse.plotWhat.sumIntensity || plot_realtime.plotType == plot_timeCourse.plotWhat.sumIntensity_bg)
                    {
                        points.Add(sum);
                    }
                    else if (plot_realtime.plotType == plot_timeCourse.plotWhat.meanIntensity || plot_realtime.plotType == plot_timeCourse.plotWhat.meanIntensity_bg)
                    {
                        double mean = (width > 0) ? (sum / width) : 0.0;
                        points.Add(mean);
                    }
                    else
                    {
                        // mean lifetime
                        double meanLifetime_ns = (sum > 0) ? (wsum_ps / sum / 1000.0) : 0.0;
                        double off = 0.0;
                        try { off = FLIM_ImgData.offset[ch]; } catch { }
                        points.Add(meanLifetime_ns - off);
                    }
                }

                if (points.Count == 0)
                    return;

                // Append and refresh plot once per frame.
                realtimeData.AddRange(points);
                plot_realtime.plotNow_realtime(realtimeData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateRealtimePlotFromFrame failed: " + ex.Message);
            }
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
                { }
                ;
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
                        IntensityBitmap = ImageProcessing.FormatImage(State_intensity_range[0], thresholdFLIM_low_high[0], FLIM_ImgData.Project[0]);
                    else
                        IntensityBitmap = null;
                }


                if (FLIM_ImgData.nChannels > 1)
                {
                    lock (syncBmp[1])
                    {
                        if (FLIM_ImgData.State.Acq.acquisition[1])
                            IntensityBitmap2 = ImageProcessing.FormatImage(State_intensity_range[1], thresholdFLIM_low_high[1], FLIM_ImgData.Project[1]);
                        else
                            IntensityBitmap2 = null;
                    }
                }

                if (FLIM_ImgData.State.Acq.fastZScan && FLIM_ImgData.State.Acq.FastZ_phase_detection_mode)
                {
                    int page2 = FLIM_ImgData.n_pages - FLIM_ImgData.currentPage - 1;
                    FLIM_ImgData.CalculatePage_Direct(page2, false);
                    Bitmap_FastZ_PhaseComplementary = ImageProcessing.FormatImage(State_intensity_range[currentChannel], thresholdFLIM_low_high[currentChannel], FLIM_ImgData.Project_Pages[page2][currentChannel]);
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
            if (updatingPsPerUnitText)
                return;

            SyncPsPerUnitFromState(currentChannel);
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



        private void saveSplitscanInDifferentFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSplitScan(true);
        }

        public void SaveSplitScan(bool openprocCtrl)
        {
            FileIO fileIO = new FileIO(FLIM_ImgData.State);
            string base_fileName = FLIM_ImgData.fullName(currentChannel, false);


            if (openprocCtrl)
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

            var nSplit = FLIM_ImgData.State.Acq.nSplitScanning;

            FLIM_ImgData.State.Acq.linesPerFrame /= nSplit;
            int nLines = FLIM_ImgData.State.Acq.linesPerFrame;
            FLIM_ImgData.State.Acq.nSplitScanning = 1;
            //Storing original images

            var original_image = new ushort[FLIM_ImgData.FLIM_Pages.Length][][,,];



            for (int slice = 0; slice < nSplit; slice++)
            {
                string fileName = base_fileName.Split('.')[0] + "_Split" + (slice + 1) + ".flim";
                //GotoPage(i);
                UpdateImages(false, false, false, false);

                if (fileName != "")
                {
                    if (FLIM_ImgData.nFastZ > 1)
                    {
                        MessageBox.Show("Not yet implemented. Please let Ryohei know that you need this function");
                    }
                    else
                    {


                        for (int i = 0; i < FLIM_ImgData.FLIM_Pages.Length; i++)
                        {
                            if (StopFileOpening)
                                break;

                            GotoPage4D(i);
                            UpdateImages(true, false, false, true);

                            if (slice == 0)
                                original_image[i] = (ushort[][,,])Copier.DeepCopyArray(FLIM_ImgData.FLIMRaw);

                            var new_flim = new ushort[FLIM_ImgData.nChannels][,,];
                            for (int ch = 0; ch < FLIM_ImgData.nChannels; ch++)
                            {
                                new_flim[ch] = ImageProcessing.getLinesFLIM(original_image[i][ch], slice * nLines, (slice + 1) * nLines);
                            }


                            FLIM_ImgData.LoadFLIMRawFromData4D(new_flim, FLIM_ImgData.acquiredTime, false);
                            FLIM_ImgData.calculateAll();
                            FLIM_ImgData.addCurrentFLIMRawToPage4D(true, i, true);


                            var error = fileIO.SaveFLIMInTiff(fileName, FLIM_ImgData.FLIMRaw, FLIM_ImgData.acquiredTime_Pages[i], i == 0, FLIM_ImgData.saveChannels);

                            if (error != 0)
                            {
                                MessageBox.Show("Problem in saving ! Page = " + i);
                                break;
                            }
                        }
                    }
                }
            }

            if (openprocCtrl)
                this.InvokeIfRequired(o => o.TurnOffDuringOpeningProc(true));

            string filename = base_fileName.Split('.')[0] + "_Split" + 1 + ".flim";
            OpenFLIM(filename, true, plot_regular.calc_upon_open, true);
        }

        private void extractPagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string query = "Start Page - End Page";
            string default1 = String.Format("1 - {0}", FLIM_ImgData.n_pages);
            string input = Interaction.InputBox(query, "Set Pages", default1);

            // Kengo BEGIN 05-30-2025
            // Add a condition to return if cliced cancel
            if (input == "")
                return;
            // Kengo END

            if (input.Split('-').Length < 1)
            {
                MessageBox.Show("The format appears to be incorrect!");
                return;
            }

            string start_page_s = input.Split('-')[0];
            string end_page_e = input.Split('-')[1];

            if (!int.TryParse(start_page_s, out int start_page))
            {
                MessageBox.Show("The format appears to be incorrect!");
                return;
            }

            if (!int.TryParse(end_page_e, out int end_page))
            {
                MessageBox.Show("The format appears to be incorrect!");
                return;
            }


            if (FLIM_ImgData.nFastZ > 1)
            {
                FLIM_ImgData.Extract5DFLIMPages(start_page - 1, end_page);
            }
            else
            {
                FLIM_ImgData.Extract4DFLIMPages(start_page - 1, end_page);
            }

            UpdateImages(true, false, false, true, true);
        }

        private void blankCurrentPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int page_to_blank = (FLIM_ImgData.nFastZ > 1) ? FLIM_ImgData.currentPage5D : FLIM_ImgData.currentPage;
            int n_pages = (FLIM_ImgData.nFastZ > 1) ? FLIM_ImgData.n_pages5D : FLIM_ImgData.n_pages;

            if (n_pages < 2)
            {
                MessageBox.Show("This function is only for images with multiple pages");
            }

            if (0 <= page_to_blank && page_to_blank < n_pages && n_pages > 2)
            {
                if (FLIM_ImgData.nFastZ > 1)
                {
                    FLIM_ImgData.Brank5DFLIMPage(page_to_blank);
                }
                else
                {
                    FLIM_ImgData.Brank4DFLIMPage(page_to_blank);
                }
            }

            UpdateImages(true, false, false, true, true);
        }

        // Kengo BEGIN 06-01-2025
        // Add for remote commands
        public void deleteCurrentPage()
        {
            deleteCurrentPageToolStripMenuItem_Click(null, null);
        }

        public void blankCurrentPage()
        {
            blankCurrentPageToolStripMenuItem_Click(null, null);
        }

        public void extractPages(int[] values)
        {
            if (FLIM_ImgData.nFastZ > 1)
            {
                FLIM_ImgData.Extract5DFLIMPages(values[0] - 1, values[1]);
            }
            else
            {
                FLIM_ImgData.Extract4DFLIMPages(values[0] - 1, values[1]);
            }

            UpdateImages(true, false, false, true, true);
        }

        private void tau1_TextChanged(object sender, EventArgs e)
        {
            ; //To find when this text is changed. Just for debugger mark.
        }

        //public void appendPage(int value)
        //{
        //    if (FLIM_ImgData.nFastZ > 1)
        //    {
        //        FLIM_ImgData.Append5DFLIMPage(value - 1);
        //    }
        //    else
        //    {
        //        FLIM_ImgData.Append4DFLIMPage(value - 1);
        //    }

        //    UpdateImages(true, false, false, true, true);
        //}
        // Kengo END

        public void UpdateImages(bool calcLifetimeMap, bool realtime1, bool focus, bool calcLifetime, bool calcProject) //Can be slow. //Done with timer.
        {
            update_image_busy = true;
            try
            {
                realtime = realtime1;
                focusing = focus;

                FLIM_ImgData.ThreeDRoi = ThreeDRoi;
                UpdateFiberPhotometryExportMenu();

                // Fiber photometry: do NOT update/render images. Only update lifetime curve + realtime plot.
                if (IsFiberPhotometryMode())
                {
                    try
                    {
                    EnsureFiberPhotometryRoi();

                    int nChannels_fp = FLIM_ImgData.nChannels;
                    int c_fp = currentChannel;

                    // Keep channel selection logic consistent with UI
                    if (Channel1.Checked) c_fp = 0;
                    else if (Channel2.Checked) c_fp = 1;
                    else if (Channel12.Checked)
                    {
                        if (Ch1.Checked) c_fp = 0;
                        else if (Ch2.Checked) c_fp = 1;
                    }
                    if (c_fp < nChannels_fp) currentChannel = c_fp;

                    // Lifetime computation (single-point)
                    if (calcLifetime && FLIM_ImgData.FLIM_on.Any(x => x == true))
                        FLIM_ImgData.calculateLifetime(-1);

                    // Realtime plot + CSV (Fiber photometry: each line = one time bin)
                    if (realtime)
                    {
                        var raw = (FLIM_ImgData.FLIMRaw != null && currentChannel < FLIM_ImgData.FLIMRaw.Length) ? FLIM_ImgData.FLIMRaw[currentChannel] : null;
                        if (raw != null && raw.GetLength(0) > 0 && raw.GetLength(1) > 0)
                        {
                            int h = raw.GetLength(0);
                            int w = raw.GetLength(1);
                            int nT = raw.GetLength(2);
                            bool plotNow = plot_realtime != null && plot_realtime.Visible;

                            EnsureFiberPhotometryCsvInitialized(currentChannel, nT);

                            double res_ps = 250.0;
                            try { res_ps = FLIM_ImgData.State.Spc.spcData.resolution[currentChannel]; } catch { }

                            for (int y = 0; y < h; y++)
                            {
                                var decay = new ushort[nT];
                                double sum = 0.0;
                                double wsum_ps = 0.0;

                                for (int x = 0; x < w; x++)
                                {
                                    for (int t = 0; t < nT; t++)
                                    {
                                        ushort v = raw[y, x, t];
                                        decay[t] = (ushort)(decay[t] + v);
                                        sum += v;
                                        wsum_ps += v * (t * res_ps);
                                    }
                                }

                                double sumIntensity = sum;
                                double meanIntensity = (w > 0) ? (sum / w) : sum;
                                double meanLifetime_ns = (sum > 0) ? (wsum_ps / sum / 1000.0) : 0.0;

                                AppendFiberPhotometryCsvRow(currentChannel, decay, meanIntensity, sumIntensity, meanLifetime_ns);

                                if (plotNow)
                                {
                                    if (plot_realtime.plotType == plot_timeCourse.plotWhat.meanIntensity || plot_realtime.plotType == plot_timeCourse.plotWhat.meanIntensity_bg)
                                        realtimeData.Add(meanIntensity);
                                    else if (plot_realtime.plotType == plot_timeCourse.plotWhat.sumIntensity || plot_realtime.plotType == plot_timeCourse.plotWhat.sumIntensity_bg)
                                        realtimeData.Add(sumIntensity);
                                    else
                                        realtimeData.Add(meanLifetime_ns);
                                }
                            }

                            if (plotNow)
                            {
                                // Realtime plot: keep x-axis as Time Point (index) for stability.
                                plot_realtime.plotNow_realtime(realtimeData);
                            }
                        }
                    }
                    else if (!realtime && plot_regular != null)
                    {
                        plot_regular.plotNow_noRealtime(TCF, TC, this, currentChannel);
                    }

                    // Keep page labels up to date in fiber mode (no image render path).
                    this.BeginInvokeIfRequired(o =>
                    {
                        if (o.FLIM_ImgData != null)
                        {
                            int nPages = o.FLIM_ImgData.n_pages;
                            o.st_pageN.Text = "/ " + nPages;
                            if (nPages > 0)
                            {
                                if (o.displayZProjection)
                                {
                                    o.c_page.Text = String.Format("{0} - {1}", o.FLIM_ImgData.ZProjection_Range[0] + 1, o.FLIM_ImgData.ZProjection_Range[1]);
                                    o.PageStart.Text = (o.FLIM_ImgData.ZProjection_Range[0] + 1).ToString();
                                    o.PageEnd.Text = (o.FLIM_ImgData.ZProjection_Range[1]).ToString();
                                }
                                else
                                {
                                    o.c_page.Text = String.Format("{0}", o.FLIM_ImgData.currentPage + 1);
                                    o.PageStart.Text = (o.FLIM_ImgData.currentPage + 1).ToString();
                                    o.PageEnd.Text = (o.FLIM_ImgData.currentPage + 1).ToString();
                                }
                            }
                        }
                    });

                    // Lifetime curve
                    if (FLIM_ImgData.State.Acq.acqFLIMA[currentChannel])
                        LifetimeCurvePlot.Invalidate();
                    }
                    finally
                    {
                        update_image_busy = false;
                    }
                    return;
                }

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

            bool showFLIM = ShowFLIM.Checked;


            if (Auto1.Checked)
                Auto_contrast(); //Save Intensity_range too.

            LoadIntensity_Range(); //Read intensity range from memory and calculate intensity range per frame...

            if (calcProject)
                FLIM_ImgData.calculateProject();

            FormatProject();

            if (showFLIM && FLIM_ImgData.State.Acq.acqFLIMA[c] && FLIM_ImgData.State.Acq.acquisition[c])
            {
                double valD;

                if (calcLifetimeMap)
                    FLIM_ImgData.calculateLifetimeMapCh(c, FLIM_ImgData.offset[c]);

                if (!Double.TryParse(highThreshFLIM.Text, out valD))
                    valD = -1;

                FLIM_ImgData.high_threshold[c] = valD;

                if (ThreshCB.Checked)
                {
                    FLIM_ImgData.low_threshold[c] = State_FLIM_intensity_range[c][0];
                }
                else
                {
                    if (!Double.TryParse(lowThreshFLIM.Text, out valD))
                        valD = State_FLIM_intensity_range[c][0];

                    FLIM_ImgData.low_threshold[c] = valD;
                }

                FLIM_ImgData.filterMAP(FLIM_ImgData.State.Display.filterWindow_FLIM, c); //Subract and filter.

                lock (syncBmpFLIM)
                {
                    if (FLIM_ImgData.LifetimeMapF[c] != null)
                        FLIMBitmap = ImageProcessing.FormatImageFLIM(State_FLIM_intensity_range[c], State_FLIM_lifetime_range[c], thresholdFLIM_low_high[c], FLIM_ImgData.LifetimeMapF[c], FLIM_ImgData.ProjectF[c], false, color_scheme);
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
            bool isLineScanTimeAxisMode = IsLineScanTimeAxisMode();

            if (realtime && plot_realtime.Visible && !isLineScanTimeAxisMode)
            {
                double meanI_for_csv = 0.0;
                double sumI_for_csv = 0.0;
                double tau_for_csv = 0.0;

                if (FLIM_ImgData.State.Acq.acqFLIMA[currentChannel])
                {
                    FLIM_ImgData.calculate_MeanLifetime_ch(currentChannel, -1, FLIM_ImgData.offset);

                    if (plot_realtime.plotType == plot_timeCourse.plotWhat.meanIntensity || plot_realtime.plotType == plot_timeCourse.plotWhat.meanIntensity_bg)
                        realtimeData.Add(FLIM_ImgData.Roi.flim_parameters.meanIntensity[currentChannel] / (double)FLIM_ImgData.nAveragedFrame[currentChannel]);
                    else if (plot_realtime.plotType == plot_timeCourse.plotWhat.sumIntensity || plot_realtime.plotType == plot_timeCourse.plotWhat.sumIntensity_bg)
                        realtimeData.Add(FLIM_ImgData.Roi.flim_parameters.sumIntensity[currentChannel]);
                    else
                        realtimeData.Add(FLIM_ImgData.Roi.flim_parameters.tau_m_fromMAP[currentChannel] - FLIM_ImgData.offset[c]);

                    meanI_for_csv = FLIM_ImgData.Roi.flim_parameters.meanIntensity[currentChannel] / (double)FLIM_ImgData.nAveragedFrame[currentChannel];
                    sumI_for_csv = FLIM_ImgData.Roi.flim_parameters.sumIntensity[currentChannel];
                    tau_for_csv = FLIM_ImgData.Roi.flim_parameters.tau_m_fromMAP[currentChannel] - FLIM_ImgData.offset[c];
                }
                else
                {
                    FLIM_ImgData.calculate_MeanLifetime_ch(currentChannel, -1, FLIM_ImgData.offset); //calculate intensity
                    realtimeData.Add(FLIM_ImgData.Roi.flim_parameters.meanIntensity[currentChannel]);

                    meanI_for_csv = FLIM_ImgData.Roi.flim_parameters.meanIntensity[currentChannel];
                    sumI_for_csv = FLIM_ImgData.Roi.flim_parameters.sumIntensity[currentChannel];
                    tau_for_csv = FLIM_ImgData.Roi.flim_parameters.tau_m_fromMAP[currentChannel] - FLIM_ImgData.offset[c];
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
                if (FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[c] != null && FLIM_ImgData.RoiFit.flim_parameters.LifetimeX[c].Length > 1)
                {
                    LifetimeCurvePlot.Invalidate();
                }
                else
                {
                    ;
                }
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

            if (flimage.phasor_plot_window != null && flimage.phasor_plot_window.Visible)
                flimage.phasor_plot_window.RequestRefresh();

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateImages failed: " + ex);
            }
            finally
            {
                update_image_busy = false;
            }
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
                st_1stCh.Text = "Intensity";
                st_2Ch.Text = "FLIM";
            }
            else
            {
                // KENGO BEGIN 01-26-2026
                // Change the label when Ch12 is selected
                //st_2Ch.Text = "Intensity";
                st_1stCh.Text = "Intensity 1";
                st_2Ch.Text = "Intensity 2";
                // KENGO END
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
                    c_page.Text = String.Format("{0} - {1}", FLIM_ImgData.ZProjection_Range[0] + 1, FLIM_ImgData.ZProjection_Range[1]);
                    PageStart.Text = (FLIM_ImgData.ZProjection_Range[0] + 1).ToString();
                    PageEnd.Text = (FLIM_ImgData.ZProjection_Range[1]).ToString();
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
                // Tetsuya BEGIN 06-29-2026
                // FrameAdjustment label uses FOCUS frame count during FOCUS
                // double nAverageFrame = FLIM_ImgData.nAveragedFrame[c];
                // if (!FLIM_ImgData.ZStack && FLIM_ImgData.State.Acq.nAveSlice > 1)
                //     nAverageFrame = nAverageFrame * FLIM_ImgData.State.Acq.nAveSlice;
                // FrameAdjustment.Text = "Adjust for #frames (Frame = " + nAverageFrame + ")";
                // if (FLIM_ImgData.ZProjection && FLIM_ImgData.z_projection_type == FLIMData.projectionType.Sum)
                //     FrameAdjustment.Text = "Adjust for #frames (Frame = " + FLIM_ImgData.nAveragedFrame[c] + ", Page =" + (FLIM_ImgData.ZProjection_Range[1] - FLIM_ImgData.ZProjection_Range[0] + 1) + ")";
                double nAverageFrame = GetEffectiveFrameCountForIntensityRange(c);

                FrameAdjustment.Text = "Adjust for #frames (Frame = " + nAverageFrame + ")";
                if (FLIM_ImgData.ZProjection && FLIM_ImgData.z_projection_type == FLIMData.projectionType.Sum)
                    FrameAdjustment.Text = "Adjust for #frames (Frame = " + GetEffectiveFrameCountForIntensityRange(c) + ", Page =" + (FLIM_ImgData.ZProjection_Range[1] - FLIM_ImgData.ZProjection_Range[0] + 1) + ")";
                // Tetsuya END
            }
            else
                FrameAdjustment.Text = "Adjust for #frames";

            MapStateIntensityRange();


            //if (Auto1.Checked)
            UpdateImageParamText();

            Auto1.Checked = false; //Always.

            //If this is turned on, the slider is updated all the time, and cause problems during focusing.
            //UpdateImageParamText(); //SetSlider and text update.

            var state = FLIM_ImgData.State;
            //if (state.Acq.resonantScanning || state.Acq.BiDirectionalScan)
            //{
            //    flimage.image_display.measureOddLineDifference(out double new_delay, out double diff, state);

            //    var str1 = String.Format("Status: New Delay: {0} (diff {1})", new_delay, diff);
            //    flimage.InvokeIfRequired(o => o.WriteStatusText(str1));
            //}


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
