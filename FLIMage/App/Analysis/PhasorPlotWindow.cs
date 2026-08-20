using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathLibrary;
using Utilities;

namespace FLIMage.Analysis
{
    public sealed class PhasorPlotWindow : Form
    {
        private const int HistogramBins = 256;
        private const double PlotMinS = -0.20;
        private const double PlotMaxS = 0.65;
        private const int PlotPadding = 38;
        private const int PlotCanvasSize = 420;
        private const int DefaultLeadingTrimBins = 1;
        private const int DefaultTrailingTrimBins = 1;
        private const float CurveHoverTolerancePx = 6.0F;
        private const string WindowName = "phasor_plot.loc";

        private readonly FLIMageMain flimage;
        private Image_Display image_display;

        private readonly TableLayoutPanel layoutPanel;
        private readonly PictureBox phasorImageBox;
        private readonly PictureBox phasorPlotBox;
        private readonly PictureBox timeCoursePlotBox;
        private readonly Label statusLabel;
        private readonly CheckBox calculateUponOpenCheckBox;
        private readonly Button calculateCurrentPageButton;
        private readonly Button calculateAllPagesButton;
        private readonly Button resetTimeCourseButton;
        private readonly ToolTip roiToolTip;
        private readonly PlotOnPictureBox timeCoursePlot;
        private readonly List<PhasorTimeCourseEntry> timeCourseEntries = new List<PhasorTimeCourseEntry>();

        private WindowLocManager winManager;
        private SettingManager settingManager;
        private RoiMarkerHit[] plotRoiHits = Array.Empty<RoiMarkerHit>();
        private string currentToolTipText = string.Empty;
        private int currentSyncRateHz = 0;
        private string timeCourseBaseName = string.Empty;
        private string lastTimeCourseFileName = string.Empty;
        private bool timeCourseCalculationRunning = false;

        private int refreshRequested = 0;
        private int refreshRunning = 0;
        private bool refreshOnShow = true;
        private const string SettingName = "phasor_plot";

        public PhasorPlotWindow(FLIMageMain flim, Image_Display imageDisplay)
        {
            flimage = flim;
            image_display = imageDisplay;

            Text = "Phasor analysis";
            MinimumSize = new Size(780, 560);
            BackColor = Color.FromArgb(24, 24, 24);
            ForeColor = Color.WhiteSmoke;

            layoutPanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 3,
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                Padding = new Padding(8, 8, 8, 4),
            };
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 62F));
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            phasorImageBox = CreatePictureBox("Phasor image");
            phasorPlotBox = CreatePictureBox("Phasor plot");
            timeCoursePlotBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(4),
            };
            timeCoursePlot = new PlotOnPictureBox(timeCoursePlotBox)
            {
                XTitle = "Time (s)",
                YTitle = "Phasor tau (ns)",
            };

            statusLabel = new Label
            {
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                Height = 26,
                Padding = new Padding(6, 6, 6, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Gainsboro,
            };

            calculateUponOpenCheckBox = new CheckBox
            {
                AutoSize = true,
                BackColor = BackColor,
                ForeColor = Color.Gainsboro,
                Margin = new Padding(3, 6, 8, 2),
                Text = "Calculate upon open",
                UseVisualStyleBackColor = false,
            };

            calculateCurrentPageButton = CreateCommandButton("Calculate current page");
            calculateCurrentPageButton.Click += CalculateCurrentPageButton_Click;

            calculateAllPagesButton = CreateCommandButton("Calculate all pages");
            calculateAllPagesButton.Click += CalculateAllPagesButton_Click;

            resetTimeCourseButton = CreateCommandButton("Reset");
            resetTimeCourseButton.Click += ResetTimeCourseButton_Click;

            var commandPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                Padding = new Padding(0),
                WrapContents = false,
            };
            commandPanel.Controls.Add(calculateUponOpenCheckBox);
            commandPanel.Controls.Add(calculateCurrentPageButton);
            commandPanel.Controls.Add(calculateAllPagesButton);
            commandPanel.Controls.Add(resetTimeCourseButton);

            var bottomPanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Fill,
                AutoSize = true,
                BackColor = BackColor,
                Margin = new Padding(0),
                Padding = new Padding(0),
            };
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bottomPanel.Controls.Add(statusLabel, 0, 0);
            bottomPanel.Controls.Add(commandPanel, 1, 0);

            roiToolTip = new ToolTip
            {
                InitialDelay = 120,
                ReshowDelay = 60,
                AutoPopDelay = 8000,
                ShowAlways = true,
            };

            layoutPanel.Controls.Add(phasorImageBox, 0, 0);
            layoutPanel.Controls.Add(phasorPlotBox, 1, 0);
            layoutPanel.Controls.Add(timeCoursePlotBox, 0, 1);
            layoutPanel.SetColumnSpan(timeCoursePlotBox, 2);
            layoutPanel.Controls.Add(bottomPanel, 0, 2);
            layoutPanel.SetColumnSpan(bottomPanel, 2);
            Controls.Add(layoutPanel);

            phasorPlotBox.MouseMove += PhasorPlotBox_MouseMove;
            phasorPlotBox.MouseLeave += PhasorPlotBox_MouseLeave;
            Load += PhasorPlotWindow_Load;
            Shown += PhasorPlotWindow_Shown;
            FormClosing += PhasorPlotWindow_FormClosing;
            VisibleChanged += PhasorPlotWindow_VisibleChanged;
        }

        private static Button CreateCommandButton(string text)
        {
            return new Button
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ForeColor = Color.Black,
                Margin = new Padding(3, 2, 3, 2),
                Padding = new Padding(6, 2, 6, 2),
                Text = text,
                UseVisualStyleBackColor = true,
            };
        }

        public bool CalculateUponOpen => calculateUponOpenCheckBox.Checked;

        public void AttachImageDisplay(Image_Display imageDisplay)
        {
            AttachImageDisplay(imageDisplay, true);
        }

        private void AttachImageDisplay(Image_Display imageDisplay, bool requestRefresh)
        {
            image_display = imageDisplay;
            refreshOnShow = true;

            if (requestRefresh && Visible)
                RequestRefresh();
        }

        public void RequestRefresh()
        {
            if (IsDisposed)
                return;

            refreshOnShow = true;
            Interlocked.Exchange(ref refreshRequested, 1);

            if (Visible && IsHandleCreated)
                StartRefreshLoop();
        }

        public void SaveWindowLocation()
        {
            settingManager?.SaveFromObject();
            winManager?.SaveWindowLocation();
        }

        public void ResetWindowLocation()
        {
            DefaultWindowLocation();
            SaveWindowLocation();
        }

        public void CalculateAllPagesUponOpen(Image_Display imageDisplay = null)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => CalculateAllPagesUponOpen(imageDisplay)));
                return;
            }

            if (imageDisplay != null)
                AttachImageDisplay(imageDisplay, false);

            if (!CalculateUponOpen)
                return;

            var data = image_display?.FLIM_ImgData;
            if (data == null || data.n_pages <= 0)
                return;

            if (IsStopOpeningRequested())
                return;

            _ = CalculateUponOpenAfterUiSettlesAsync();
        }

        public void CalculateCurrentPageUponOpen(Image_Display imageDisplay = null)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => CalculateCurrentPageUponOpen(imageDisplay)));
                return;
            }

            if (imageDisplay != null)
                AttachImageDisplay(imageDisplay, false);

            if (!CalculateUponOpen || IsStopOpeningRequested())
                return;

            var data = image_display?.FLIM_ImgData;
            if (data == null || data.n_pages <= 0)
                return;

            int page = Math.Max(0, Math.Min(data.currentPage, data.n_pages - 1));
            _ = CalculatePhasorTimeCoursePagesAsync(new[] { page }, true);
        }

        private async Task CalculateUponOpenAfterUiSettlesAsync()
        {
            try
            {
                await Task.Delay(250);

                if (IsDisposed || !CalculateUponOpen || IsStopOpeningRequested())
                    return;

                var data = image_display?.FLIM_ImgData;
                if (data == null || data.n_pages <= 0)
                    return;

                int[] pages = GetCalculateUponOpenPages(data);
                if (pages.Length == 0)
                    return;

                await CalculatePhasorTimeCoursePagesAsync(pages, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problem scheduling phasor calculate upon open: " + ex);
            }
        }

        private int[] GetCalculateUponOpenPages(FLIMData data)
        {
            if (data == null || data.n_pages <= 0 || IsStopOpeningRequested())
                return Array.Empty<int>();

            if (!data.KeepPagesInMemory)
            {
                int page = Math.Max(0, Math.Min(data.currentPage, data.n_pages - 1));
                return new[] { page };
            }

            if (data.FLIM_Pages == null)
                return Array.Empty<int>();

            int nPages = Math.Min(data.n_pages, data.FLIM_Pages.Length);
            return Enumerable.Range(0, nPages)
                .Where(page => data.FLIM_Pages[page] != null)
                .ToArray();
        }

        private bool IsStopOpeningRequested()
        {
            return image_display != null && !image_display.IsDisposed && image_display.StopFileOpeningRequested;
        }

        private static PictureBox CreatePictureBox(string title)
        {
            var box = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(12, 12, 12),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(4),
            };

            box.Paint += (sender, e) =>
            {
                if (((PictureBox)sender).Image != null)
                    return;

                using (var brush = new SolidBrush(Color.FromArgb(210, 210, 210)))
                using (var font = new Font("Segoe UI", 10F, FontStyle.Regular))
                {
                    var rect = ((PictureBox)sender).ClientRectangle;
                    e.Graphics.DrawString(title, font, brush, rect,
                        new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center,
                        });
                }
            };

            return box;
        }

        private void PhasorPlotWindow_Load(object sender, EventArgs e)
        {
            string path = image_display?.FLIM_ImgData?.State?.Files?.windowsInfoPath;
            winManager = new WindowLocManager(this, WindowName, path);
            DefaultWindowLocation();
            winManager.LoadWindowLocation(true);
            InitializeSetting();
        }

        private void InitializeSetting()
        {
            string initPath = image_display?.FLIM_ImgData?.State?.Files?.initFolderPath;
            if (string.IsNullOrEmpty(initPath))
                return;

            settingManager = new SettingManager(SettingName, initPath);
            settingManager.AddToDict(calculateUponOpenCheckBox);
            settingManager.LoadToObject();
        }

        private void PhasorPlotWindow_Shown(object sender, EventArgs e)
        {
            RequestRefresh();
        }

        private void PhasorPlotWindow_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible && refreshOnShow)
                StartRefreshLoop();
        }

        private void PhasorPlotWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing ||
                e.CloseReason == CloseReason.None)
            {
                e.Cancel = true;
                Hide();
                SaveWindowLocation();
                flimage.ToolWindowClosed();
                return;
            }

            SaveWindowLocation();
            ReplaceBitmap(phasorImageBox, null);
            ReplaceBitmap(phasorPlotBox, null);
        }

        private void DefaultWindowLocation()
        {
            if (image_display == null || image_display.IsDisposed)
            {
                Size = new Size(900, 520);
                return;
            }

            Size = new Size(Math.Max(760, image_display.Width), Math.Max(420, image_display.Height));

            int screenRight = Screen.FromControl(image_display).WorkingArea.Right;
            int x = image_display.Right + 8;
            int y = image_display.Top;

            if (x + Width > screenRight)
            {
                x = image_display.Left;
                y = image_display.Bottom + 8;
            }

            Location = new Point(Math.Max(0, x), Math.Max(0, y));
        }

        private async void StartRefreshLoop()
        {
            if (Interlocked.CompareExchange(ref refreshRunning, 1, 0) != 0)
                return;

            try
            {
                while (Visible && Interlocked.Exchange(ref refreshRequested, 0) == 1)
                {
                    refreshOnShow = false;

                    PhasorSnapshot snapshot = CaptureSnapshot();
                    PhasorResult result = snapshot == null
                        ? PhasorResult.Empty("No FLIM data available.")
                        : await Task.Run(() => Calculate(snapshot));

                    if (IsDisposed)
                        return;

                    if (InvokeRequired)
                        BeginInvoke((Action)(() => ApplyResult(result)));
                    else
                        ApplyResult(result);
                }
            }
            finally
            {
                Interlocked.Exchange(ref refreshRunning, 0);

                if (Visible && Volatile.Read(ref refreshRequested) == 1)
                    StartRefreshLoop();
            }
        }

        private PhasorSnapshot CaptureSnapshot()
        {
            return CaptureSnapshot(true);
        }

        private PhasorSnapshot CaptureSnapshot(bool useFitTimingOrigin)
        {
            if (image_display == null || image_display.IsDisposed)
                return null;

            var data = image_display.FLIM_ImgData;
            if (data == null || data.FLIMRaw == null || data.nChannels <= 0)
                return null;

            int channel = Math.Max(0, Math.Min(image_display.currentChannel, data.nChannels - 1));
            if (channel >= data.FLIMRaw.Length)
                return null;

            ushort[,,] raw = data.FLIMRaw[channel];
            if (raw == null || raw.GetLength(2) < 2)
                return null;

            return CaptureSnapshot(data, channel, data.currentPage, raw, GetCurrentProjectImage(data, channel, data.currentPage), useFitTimingOrigin);
        }

        private PhasorSnapshot CaptureSnapshotForPage(int page, bool useFitTimingOrigin)
        {
            if (image_display == null || image_display.IsDisposed)
                return null;

            var data = image_display.FLIM_ImgData;
            if (data == null || data.FLIM_Pages == null || data.nChannels <= 0)
                return null;

            if (page < 0 || page >= data.FLIM_Pages.Length || data.FLIM_Pages[page] == null)
                return null;

            int channel = Math.Max(0, Math.Min(image_display.currentChannel, data.nChannels - 1));
            if (channel >= data.FLIM_Pages[page].Length)
                return null;

            ushort[,,] raw = data.FLIM_Pages[page][channel];
            if (raw == null || raw.GetLength(2) < 2)
                return null;

            return CaptureSnapshot(data, channel, page, raw, GetPageProjectImage(data, channel, page), useFitTimingOrigin);
        }

        private PhasorSnapshot CaptureSnapshot(FLIMData data, int channel, int page, ushort[,,] raw, ushort[,] projectImage, bool useFitTimingOrigin)
        {
            int nTime = raw.GetLength(2);
            int leadingTrimBins = Math.Min(DefaultLeadingTrimBins, Math.Max(0, nTime / 2 - 1));
            int trailingTrimBins = Math.Min(DefaultTrailingTrimBins, Math.Max(0, nTime - leadingTrimBins - 2));
            int start = leadingTrimBins;
            int end = nTime - trailingTrimBins;

            if (end - start < 2)
            {
                start = 0;
                end = nTime;
                leadingTrimBins = 0;
                trailingTrimBins = 0;
            }

            double psPerBin = data.psPerUnit;
            if (data.State?.Spc?.spcData?.resolution != null && channel < data.State.Spc.spcData.resolution.Length)
                psPerBin = data.State.Spc.spcData.resolution[channel];

            int syncRate = 0;
            string syncRateSource = "default";
            if (data.State?.Spc?.datainfo?.syncRate != null && data.State.Spc.datainfo.syncRate.Length > 0)
            {
                if (channel < data.State.Spc.datainfo.syncRate.Length)
                    syncRate = Math.Max(1, data.State.Spc.datainfo.syncRate[channel]);
                else
                    syncRate = Math.Max(1, data.State.Spc.datainfo.syncRate[0]);
                syncRateSource = "measured";
            }
            else if (data.syncRate != null && data.syncRate.Length > 0)
            {
                if (channel < data.syncRate.Length)
                    syncRate = Math.Max(1, data.syncRate[channel]);
                else
                    syncRate = Math.Max(1, data.syncRate[0]);
                syncRateSource = "measured";
            }

            double expectedSyncRateHz = 0.0;
            if (data.State?.Acq != null && data.State.Acq.ExpectedLaserPulseRate_MHz > 0.0)
                expectedSyncRateHz = data.State.Acq.ExpectedLaserPulseRate_MHz * 1e6;

            if (expectedSyncRateHz > 0.0)
            {
                if (syncRate <= 0 ||
                    syncRate < expectedSyncRateHz * 0.9 ||
                    syncRate > expectedSyncRateHz * 1.1)
                {
                    syncRate = (int)Math.Round(expectedSyncRateHz);
                    syncRateSource = "expected";
                }
            }

            if (syncRate <= 0)
            {
                syncRate = 80000000;
                syncRateSource = "default";
            }

            double imageOffsetNs = (data.offset != null && channel < data.offset.Length) ? data.offset[channel] : 0.0;
            double phasorOriginNs = imageOffsetNs;
            double irfSigmaNs = double.NaN;
            string originSource = "image";

            if (useFitTimingOrigin && TryGetFitTimingOriginNs(data, channel, psPerBin, out double fitT0Ns, out double fitSigmaNs))
            {
                phasorOriginNs = fitT0Ns;
                irfSigmaNs = fitSigmaNs;
                originSource = "fit";
            }

            RoiSnapshot[] rois = CaptureRois(data, page);

            return new PhasorSnapshot
            {
                Channel = channel,
                Page = page,
                FileNumber = data.fileCounter,
                FileName = data.fileName,
                BaseName = data.baseName,
                TimeMilliseconds = GetTimeMilliseconds(data, page),
                Raw = (ushort[,,])raw.Clone(),
                TimeStart = start,
                TimeEnd = end,
                PsPerBin = psPerBin,
                SyncRateHz = syncRate,
                SyncRateSource = syncRateSource,
                PhasorOriginNs = phasorOriginNs,
                OriginSource = originSource,
                ImageOffsetNs = imageOffsetNs,
                IrfSigmaNs = irfSigmaNs,
                LeadingTrimBins = leadingTrimBins,
                TrailingTrimBins = trailingTrimBins,
                ColorScheme = image_display.CurrentColorScheme,
                LifetimeRange = image_display.GetCurrentFlimLifetimeRange(channel),
                IntensityRange = image_display.GetCurrentFlimIntensityRange(channel),
                ThresholdRange = image_display.GetCurrentFlimThresholdRange(channel),
                ProjectImage = projectImage != null ? (ushort[,])projectImage.Clone() : null,
                Rois = rois,
            };
        }

        private static ushort[,] GetCurrentProjectImage(FLIMData data, int channel, int page)
        {
            if (data?.ProjectF != null && channel >= 0 && channel < data.ProjectF.Length && data.ProjectF[channel] != null)
                return data.ProjectF[channel];

            return GetPageProjectImage(data, channel, page);
        }

        private static ushort[,] GetPageProjectImage(FLIMData data, int channel, int page)
        {
            if (data?.Project_Pages == null || page < 0 || page >= data.Project_Pages.Length)
                return null;

            if (data.Project_Pages[page] == null || channel < 0 || channel >= data.Project_Pages[page].Length)
                return null;

            return data.Project_Pages[page][channel];
        }

        private static double GetTimeMilliseconds(FLIMData data)
        {
            return GetTimeMilliseconds(data, data?.currentPage ?? 0);
        }

        private static double GetTimeMilliseconds(FLIMData data, int page)
        {
            if (data == null)
                return 0.0;

            int safePage = Math.Max(0, page);
            bool synthesizeTiming = false;
            DateTime pageTime = data.acquiredTime;

            if (data.acquiredTime_Pages != null &&
                data.acquiredTime_Pages.Length > 1 &&
                safePage < data.acquiredTime_Pages.Length)
            {
                DateTime firstTime = data.acquiredTime_Pages[0];
                bool allSame = true;
                for (int i = 1; i < data.acquiredTime_Pages.Length; i++)
                {
                    if (data.acquiredTime_Pages[i] != firstTime)
                    {
                        allSame = false;
                        break;
                    }
                }

                if (allSame)
                {
                    pageTime = firstTime;
                    synthesizeTiming = true;
                }
                else
                {
                    pageTime = data.acquiredTime_Pages[safePage];
                }
            }

            double milliseconds = (pageTime - ImageInfo.startDate).TotalMilliseconds;
            if (milliseconds > 0.0 && !synthesizeTiming)
                return milliseconds;

            double baseMilliseconds = (pageTime - new DateTime(2000, 1, 1)).TotalMilliseconds;
            if (data.n_pages <= 1)
                return baseMilliseconds;

            double frameInterval = data.State?.Acq?.frameInterval() ?? 0.0;
            if (data.State?.Acq != null && data.State.Acq.aveFrame)
                frameInterval *= data.State.Acq.nAveFrame;

            int nFrames = 1;
            if (data.State?.Acq != null)
            {
                nFrames = data.State.Acq.aveFrame
                    ? Math.Max(1, data.State.Acq.nFrames / Math.Max(1, data.State.Acq.nAveFrame))
                    : Math.Max(1, data.State.Acq.nFrames);
            }

            int frame = safePage % nFrames;
            int slice = safePage / nFrames;
            double sliceInterval = data.State?.Acq?.sliceInterval ?? 0.0;
            return baseMilliseconds + 1000.0 * (frame * frameInterval + slice * sliceInterval);
        }

        private static bool TryGetFitTimingOriginNs(FLIMData data, int channel, double psPerBin, out double t0Ns, out double sigmaNs)
        {
            t0Ns = 0.0;
            sigmaNs = double.NaN;

            var parameters = data?.RoiFit?.flim_parameters;
            if (parameters?.fitCurve == null || channel >= parameters.fitCurve.Length || parameters.fitCurve[channel] == null || parameters.fitCurve[channel].Length == 0)
                return false;

            double[] beta = null;
            if (parameters.beta != null && channel < parameters.beta.Length)
                beta = parameters.beta[channel];
            if ((beta == null || beta.Length == 0) && parameters.beta0 != null && channel < parameters.beta0.Length)
                beta = parameters.beta0[channel];
            if (beta == null)
                return false;

            double nsPerBin = psPerBin * 1e-3;

            if (beta.Length >= 6)
            {
                sigmaNs = beta[4] * nsPerBin;
                t0Ns = beta[5] * nsPerBin;
                return true;
            }

            if (beta.Length >= 4)
            {
                sigmaNs = beta[2] * nsPerBin;
                t0Ns = beta[3] * nsPerBin;
                return true;
            }

            return false;
        }

        private static RoiSnapshot[] CaptureRois(FLIMData data, int page)
        {
            if (data?.ROIs == null || data.ROIs.Count == 0)
                return CaptureSelectedRoi(data, page);

            var imageSize = new Size(data.width, data.height);
            var rois = new List<RoiSnapshot>();

            foreach (var roi in data.ROIs)
            {
                if (roi == null || roi.ID < 0 || roi.IsLineScanTrace)
                    continue;

                AddRoiSnapshot(data, page, imageSize, roi, roi.ID, FormatRoiLabel(roi.ID), rois);

                if (roi.ROI_type == ROI.ROItype.PolyLine && roi.polyLineROIs != null)
                {
                    foreach (var childRoi in roi.polyLineROIs)
                    {
                        if (childRoi == null)
                            continue;

                        int childId = (roi.ID + 1) * 1000 + childRoi.ID;
                        AddRoiSnapshot(data, page, imageSize, childRoi, childId, FormatRoiLabel(childId), rois);
                    }
                }
            }

            return rois.ToArray();
        }

        private static RoiSnapshot[] CaptureSelectedRoi(FLIMData data, int page)
        {
            if (data?.Roi == null)
                return Array.Empty<RoiSnapshot>();

            var imageSize = new Size(data.width, data.height);
            var rois = new List<RoiSnapshot>();
            AddRoiSnapshot(data, page, imageSize, data.Roi, -1, "Selected ROI", rois);
            return rois.ToArray();
        }

        private static void AddRoiSnapshot(
            FLIMData data,
            int page,
            Size imageSize,
            ROI roi,
            int roiId,
            string label,
            List<RoiSnapshot> rois)
        {
            if (roi == null || rois == null)
                return;
            if (roiId >= 0 && !IsRoiInFocus(data, roi, page))
                return;

            var pixels = roi.GetPixelsInside2D(imageSize);
            if (pixels == null || pixels.Count == 0)
                return;

            var snapshot = new RoiSnapshot
            {
                RoiId = roiId,
                Label = label,
                Pixels = new Point[pixels.Count],
            };

            for (int i = 0; i < pixels.Count; i++)
                snapshot.Pixels[i] = pixels[i];

            rois.Add(snapshot);
        }

        private static bool IsRoiInFocus(FLIMData data, ROI roi, int page)
        {
            if (data == null || roi == null)
                return false;
            if (!data.ZStack && data.nFastZ <= 1)
                return true;

            return roi.Roi3d && roi.Z != null && roi.Z.Any(x => x == page);
        }

        private static string FormatRoiLabel(int roiId)
        {
            if (roiId >= 1000)
            {
                int parentId = roiId / 1000;
                int childId = roiId - parentId * 1000;
                return $"ROI-{parentId}_{childId}";
            }

            return $"ROI-{roiId + 1}";
        }

        private static PhasorResult Calculate(PhasorSnapshot snapshot)
        {
            int height = snapshot.Raw.GetLength(0);
            int width = snapshot.Raw.GetLength(1);
            int nTime = snapshot.Raw.GetLength(2);
            int start = Math.Max(0, Math.Min(snapshot.TimeStart, nTime - 1));
            int end = Math.Max(start + 1, Math.Min(snapshot.TimeEnd, nTime));
            int length = end - start;

            if (length < 2)
                return PhasorResult.Empty("Selected TCSPC period is too small for phasor analysis.");

            double dtSeconds = snapshot.PsPerBin * 1e-12;
            double omega = 2.0 * Math.PI * snapshot.SyncRateHz;
            double offsetPhase = omega * snapshot.PhasorOriginNs * 1e-9;

            var cosValues = new double[length];
            var sinValues = new double[length];
            for (int i = 0; i < length; i++)
            {
                double phase = omega * (start + i) * dtSeconds;
                cosValues[i] = Math.Cos(phase);
                sinValues[i] = Math.Sin(phase);
            }

            var gMap = new float[height, width];
            var sMap = new float[height, width];
            var intensityMap = new double[height, width];
            var histogram = new int[HistogramBins, HistogramBins];

            double maxIntensity = 0.0;
            double weightedG = 0.0;
            double weightedS = 0.0;
            double totalWeight = 0.0;
            int validPixels = 0;
            int maxBin = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ComputePixelPhasor(snapshot.Raw, y, x, start, length, cosValues, sinValues, offsetPhase,
                        out double sum, out double g, out double s);
                    intensityMap[y, x] = sum;
                    if (sum <= 0.0)
                        continue;

                    g = Clamp(g, 0.0, 1.0);
                    s = Clamp(s, PlotMinS, PlotMaxS);

                    gMap[y, x] = (float)g;
                    sMap[y, x] = (float)s;

                    maxIntensity = Math.Max(maxIntensity, sum);
                    weightedG += g * sum;
                    weightedS += s * sum;
                    totalWeight += sum;
                    validPixels++;

                    int binX = (int)Math.Round(g * (HistogramBins - 1));
                    int binY = (int)Math.Round((s - PlotMinS) / (PlotMaxS - PlotMinS) * (HistogramBins - 1));
                    binX = Math.Max(0, Math.Min(HistogramBins - 1, binX));
                    binY = Math.Max(0, Math.Min(HistogramBins - 1, binY));
                    histogram[binX, binY]++;
                    maxBin = Math.Max(maxBin, histogram[binX, binY]);
                }
            }

            if (validPixels == 0)
                return PhasorResult.Empty("Phasor analysis found no photons in the selected TCSPC period.");

            double avgG = totalWeight > 0 ? weightedG / totalWeight : 0.0;
            double avgS = totalWeight > 0 ? weightedS / totalWeight : 0.0;

            RoiPhasorPoint[] roiPoints = CalculateRoiPoints(snapshot, cosValues, sinValues, start, length, width, height, offsetPhase);

            Bitmap imageBitmap = BuildPhasorImage(snapshot, gMap, sMap, intensityMap, maxIntensity);
            PlotRenderResult plot = BuildPhasorPlot(histogram, maxBin, avgG, avgS, roiPoints);

            string status =
                $"{snapshot.FileName} | Ch {snapshot.Channel + 1} | bins {start + 1}-{end} | trim={snapshot.LeadingTrimBins}/{snapshot.TrailingTrimBins} | " +
                $"rep={snapshot.SyncRateHz / 1e6:0.000} MHz ({snapshot.SyncRateSource}) | t0={snapshot.PhasorOriginNs:0.000} ns ({snapshot.OriginSource}) | " +
                $"{FormatSigma(snapshot.IrfSigmaNs)}img offset={snapshot.ImageOffsetNs:0.000} ns | bg=0 | " +
                $"avg g = {avgG:0.000}, avg s = {avgS:0.000} | valid pixels = {validPixels} | ROI avg = {roiPoints.Length}";

            return new PhasorResult(imageBitmap, plot.Bitmap, status, plot.RoiMarkers, snapshot.SyncRateHz, roiPoints);
        }

        private static RoiPhasorPoint[] CalculateRoiPointsOnly(PhasorSnapshot snapshot)
        {
            if (snapshot?.Raw == null)
                return Array.Empty<RoiPhasorPoint>();

            int height = snapshot.Raw.GetLength(0);
            int width = snapshot.Raw.GetLength(1);
            int nTime = snapshot.Raw.GetLength(2);
            int start = Math.Max(0, Math.Min(snapshot.TimeStart, nTime - 1));
            int end = Math.Max(start + 1, Math.Min(snapshot.TimeEnd, nTime));
            int length = end - start;

            if (length < 2)
                return Array.Empty<RoiPhasorPoint>();

            double dtSeconds = snapshot.PsPerBin * 1e-12;
            double omega = 2.0 * Math.PI * snapshot.SyncRateHz;
            double offsetPhase = omega * snapshot.PhasorOriginNs * 1e-9;

            var cosValues = new double[length];
            var sinValues = new double[length];
            for (int i = 0; i < length; i++)
            {
                double phase = omega * (start + i) * dtSeconds;
                cosValues[i] = Math.Cos(phase);
                sinValues[i] = Math.Sin(phase);
            }

            return CalculateRoiPoints(snapshot, cosValues, sinValues, start, length, width, height, offsetPhase);
        }

        private static RoiPhasorPoint[] CalculateRoiPoints(
            PhasorSnapshot snapshot,
            double[] cosValues,
            double[] sinValues,
            int start,
            int length,
            int width,
            int height,
            double offsetPhase)
        {
            if (snapshot.Rois == null || snapshot.Rois.Length == 0)
                return Array.Empty<RoiPhasorPoint>();

            double omega = 2.0 * Math.PI * snapshot.SyncRateHz;
            var points = new System.Collections.Generic.List<RoiPhasorPoint>();

            foreach (var roi in snapshot.Rois)
            {
                if (roi?.Pixels == null || roi.Pixels.Length == 0)
                    continue;

                var decay = new double[length];

                foreach (Point pixel in roi.Pixels)
                {
                    if (pixel.X < 0 || pixel.Y < 0 || pixel.X >= width || pixel.Y >= height)
                        continue;

                    for (int t = 0; t < length; t++)
                        decay[t] += snapshot.Raw[pixel.Y, pixel.X, start + t];
                }

                ComputeDecayPhasor(decay, cosValues, sinValues, offsetPhase, out double sum, out double g, out double s);
                if (sum <= 0.0)
                    continue;

                g = Clamp(g, 0.0, 1.0);
                s = Clamp(s, PlotMinS, PlotMaxS);
                double tauNs = double.NaN;

                if (g > 1e-9 && omega > 0.0)
                    tauNs = (s / (g * omega)) * 1e9;

                points.Add(new RoiPhasorPoint
                {
                    RoiId = roi.RoiId,
                    Label = roi.Label,
                    G = g,
                    S = s,
                    TauNs = tauNs,
                    PhotonCount = sum,
                });
            }

            return points.ToArray();
        }

        private static void ComputePixelPhasor(
            ushort[,,] raw,
            int y,
            int x,
            int start,
            int length,
            double[] cosValues,
            double[] sinValues,
            double offsetPhase,
            out double sum,
            out double g,
            out double s)
        {
            var decay = new double[length];
            for (int t = 0; t < length; t++)
                decay[t] = raw[y, x, start + t];

            ComputeDecayPhasor(decay, cosValues, sinValues, offsetPhase, out sum, out g, out s);
        }

        private static void ComputeDecayPhasor(
            double[] decay,
            double[] cosValues,
            double[] sinValues,
            double offsetPhase,
            out double sum,
            out double g,
            out double s)
        {
            sum = 0.0;
            double sumG = 0.0;
            double sumS = 0.0;

            if (decay == null || decay.Length == 0)
            {
                g = 0.0;
                s = 0.0;
                return;
            }

            for (int t = 0; t < decay.Length; t++)
            {
                double value = decay[t];
                if (value < 0.0)
                    value = 0.0;

                sum += value;
                sumG += value * cosValues[t];
                sumS += value * sinValues[t];
            }

            if (sum <= 0.0)
            {
                g = 0.0;
                s = 0.0;
                return;
            }

            double gRaw = sumG / sum;
            double sRaw = sumS / sum;

            double cosOffset = Math.Cos(offsetPhase);
            double sinOffset = Math.Sin(offsetPhase);

            g = gRaw * cosOffset + sRaw * sinOffset;
            s = sRaw * cosOffset - gRaw * sinOffset;
        }

        private void ApplyResult(PhasorResult result)
        {
            if (IsDisposed)
            {
                result.Dispose();
                return;
            }

            ReplaceBitmap(phasorImageBox, result.ImageBitmap);
            ReplaceBitmap(phasorPlotBox, result.PlotBitmap);
            plotRoiHits = result.RoiMarkers ?? Array.Empty<RoiMarkerHit>();
            currentSyncRateHz = result.SyncRateHz;
            ClearPlotToolTip();
            statusLabel.Text = result.StatusText;

            if (!timeCourseCalculationRunning && timeCourseEntries.Count > 0)
                UpdateTimeCoursePlot();
        }

        private async void CalculateCurrentPageButton_Click(object sender, EventArgs e)
        {
            var data = image_display?.FLIM_ImgData;
            if (data == null || data.n_pages <= 0)
            {
                statusLabel.Text = "No FLIM pages available for phasor time course.";
                return;
            }

            int page = Math.Max(0, Math.Min(data.currentPage, data.n_pages - 1));
            await CalculatePhasorTimeCoursePagesAsync(new[] { page });
        }

        private async void CalculateAllPagesButton_Click(object sender, EventArgs e)
        {
            var data = image_display?.FLIM_ImgData;
            if (data == null || data.n_pages <= 0)
            {
                statusLabel.Text = "No FLIM pages available for phasor time course.";
                return;
            }

            int[] pages = Enumerable.Range(0, data.n_pages).ToArray();
            await CalculatePhasorTimeCoursePagesAsync(pages);
        }

        private void ResetTimeCourseButton_Click(object sender, EventArgs e)
        {
            timeCourseEntries.Clear();
            timeCourseBaseName = string.Empty;
            UpdateTimeCoursePlot();
            statusLabel.Text = "Phasor time course reset. The next calculation will start a new autosave.";
        }

        private async Task CalculatePhasorTimeCoursePagesAsync(int[] pages, bool stopOnImageDisplayStop = false)
        {
            if (timeCourseCalculationRunning)
                return;

            var data = image_display?.FLIM_ImgData;
            if (data == null || pages == null || pages.Length == 0)
                return;

            timeCourseCalculationRunning = true;
            SetTimeCourseButtonsEnabled(false);

            int savePage = Math.Max(0, Math.Min(data.currentPage, Math.Max(0, data.n_pages - 1)));
            bool restoreProjection = image_display != null && image_display.displayZProjection;
            bool restorePageNeeded = false;

            try
            {
                for (int i = 0; i < pages.Length; i++)
                {
                    if (stopOnImageDisplayStop && IsStopOpeningRequested())
                    {
                        statusLabel.Text = "Phasor time course calculation stopped.";
                        break;
                    }

                    if (data.n_pages <= 0)
                        break;

                    int page = Math.Max(0, Math.Min(pages[i], data.n_pages - 1));
                    PhasorSnapshot snapshot = page == data.currentPage
                        ? CaptureSnapshot(true)
                        : CaptureSnapshotForPage(page, true);
                    if (snapshot == null)
                    {
                        data.gotoPage(page);
                        restorePageNeeded = true;
                        snapshot = CaptureSnapshot(true);
                    }

                    if (snapshot == null)
                    {
                        statusLabel.Text = $"Phasor time course: page {page + 1} has no usable FLIM data.";
                        continue;
                    }

                    RoiPhasorPoint[] roiPoints = await Task.Run(() => CalculateRoiPointsOnly(snapshot));
                    AddTimeCourseEntry(snapshot, roiPoints);
                    UpdateTimeCoursePlot();
                    SavePhasorTimeCourse();

                    string saveText = string.IsNullOrEmpty(lastTimeCourseFileName)
                        ? string.Empty
                        : $" Autosaved: {Path.GetFileName(lastTimeCourseFileName)}";
                    statusLabel.Text = $"Phasor time course: calculated page {page + 1} ({i + 1}/{pages.Length}), ROI avg = {roiPoints.Length}.{saveText}";
                    await Task.Yield();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problem calculating phasor time course: " + ex);
                statusLabel.Text = "Problem calculating phasor time course: " + ex.Message;
            }
            finally
            {
                if (restorePageNeeded)
                    RestorePage(savePage, restoreProjection);
                SetTimeCourseButtonsEnabled(true);
                timeCourseCalculationRunning = false;
            }
        }

        private void RestorePage(int page, bool restoreProjection)
        {
            var data = image_display?.FLIM_ImgData;
            if (data == null || data.n_pages <= 0)
                return;

            try
            {
                if (restoreProjection && image_display != null && !image_display.IsDisposed)
                    image_display.calcZProjection();
                else
                    data.gotoPage(Math.Max(0, Math.Min(page, data.n_pages - 1)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problem restoring phasor page: " + ex.Message);
            }
        }

        private void SetTimeCourseButtonsEnabled(bool enabled)
        {
            calculateCurrentPageButton.Enabled = enabled;
            calculateAllPagesButton.Enabled = enabled;
            resetTimeCourseButton.Enabled = enabled;
        }

        private void AddTimeCourseEntry(PhasorSnapshot snapshot, RoiPhasorPoint[] roiPoints)
        {
            if (snapshot == null)
                return;

            string baseName = snapshot.BaseName ?? string.Empty;
            if (timeCourseEntries.Count > 0 &&
                !string.Equals(timeCourseBaseName, baseName, StringComparison.OrdinalIgnoreCase))
            {
                timeCourseEntries.Clear();
            }

            timeCourseBaseName = baseName;

            var entry = new PhasorTimeCourseEntry
            {
                Channel = snapshot.Channel,
                Page = snapshot.Page,
                FileNumber = snapshot.FileNumber,
                FileName = snapshot.FileName,
                BaseName = snapshot.BaseName,
                TimeMilliseconds = snapshot.TimeMilliseconds,
                RoiPoints = CloneRoiPoints(roiPoints),
            };

            int existing = timeCourseEntries.FindIndex(x =>
                x.Channel == entry.Channel &&
                x.Page == entry.Page &&
                x.FileNumber == entry.FileNumber);

            if (existing >= 0)
                timeCourseEntries[existing] = entry;
            else
                timeCourseEntries.Add(entry);
        }

        private void UpdateTimeCoursePlot()
        {
            if (timeCoursePlot == null)
                return;

            timeCoursePlot.ClearData();
            timeCoursePlot.XTitle = "Time (s)";
            timeCoursePlot.YTitle = "Phasor tau (ns)";

            int channel = 0;
            var data = image_display?.FLIM_ImgData;
            if (data != null && data.nChannels > 0)
                channel = Math.Max(0, Math.Min(image_display.currentChannel, data.nChannels - 1));

            List<PhasorTimeCourseEntry> entries = GetSortedEntries()
                .Where(x => x.Channel == channel)
                .ToList();

            if (entries.Count == 0)
            {
                timeCoursePlot.UpdatePlot();
                return;
            }

            double baseMilliseconds = entries[0].TimeMilliseconds;
            List<RoiTimeCourseKey> roiKeys = GetRoiKeys(entries);
            List<string> legends = new List<string>();
            int highlightedLegend = -1;
            int currentRoiId = GetCurrentRoiId();

            foreach (RoiTimeCourseKey roiKey in roiKeys)
            {
                List<double> xValues = new List<double>();
                List<double> yValues = new List<double>();

                foreach (PhasorTimeCourseEntry entry in entries)
                {
                    RoiPhasorPoint point = FindPoint(entry.RoiPoints, roiKey);
                    if (point == null || double.IsNaN(point.TauNs) || double.IsInfinity(point.TauNs))
                        continue;

                    xValues.Add(GetRelativeTimeSeconds(entry, baseMilliseconds));
                    yValues.Add(point.TauNs);
                }

                if (yValues.Count == 0)
                    continue;

                string plotStyle = entries.Count < 200 ? "o-" : "-";
                timeCoursePlot.AddData(xValues.ToArray(), yValues.ToArray(), plotStyle, 1F);
                legends.Add(roiKey.Label);

                if (roiKey.RoiId == currentRoiId)
                    highlightedLegend = legends.Count - 1;
            }

            if (legends.Count > 0)
            {
                if (highlightedLegend >= 0)
                    timeCoursePlot.AddLegendWithHighlight(legends, highlightedLegend);
                else
                    timeCoursePlot.AddLegend(legends);
            }

            timeCoursePlot.UpdatePlot();
        }

        private void SavePhasorTimeCourse()
        {
            if (timeCourseEntries.Count == 0)
                return;

            var data = image_display?.FLIM_ImgData;
            if (data == null)
                return;

            try
            {
                Directory.CreateDirectory(Path.Combine(data.pathName, "Analysis"));
                string fileName = PhasorTimeCourseFileName(data);
                string csv = BuildPhasorTimeCourseCsv();
                lastTimeCourseFileName = TrySaveText(fileName, csv);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problem saving phasor time course: " + ex);
                statusLabel.Text = "Problem saving phasor time course: " + ex.Message;
            }
        }

        private string BuildPhasorTimeCourseCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Phasor-TimeCourse");

            List<PhasorTimeCourseEntry> allEntries = GetSortedEntries();
            int[] channels = allEntries.Select(x => x.Channel).Distinct().OrderBy(x => x).ToArray();

            foreach (int channel in channels)
            {
                List<PhasorTimeCourseEntry> entries = allEntries.Where(x => x.Channel == channel).ToList();
                if (entries.Count == 0)
                    continue;

                double baseMilliseconds = entries[0].TimeMilliseconds;
                sb.AppendLine("Channel " + (channel + 1).ToString(CultureInfo.InvariantCulture));
                SaveCsvLine(sb, "Time (ms)", entries.Select(x => FormatCsvValue(x.TimeMilliseconds)));
                SaveCsvLine(sb, "FileNumber", entries.Select(x => FormatCsvValue(x.FileNumber)));
                SaveCsvLine(sb, "Page", entries.Select(x => FormatCsvValue(x.Page + 1)));
                SaveCsvLine(sb, "Time (s)", entries.Select(x => FormatCsvValue(GetRelativeTimeSeconds(x, baseMilliseconds))));

                foreach (RoiTimeCourseKey roiKey in GetRoiKeys(entries))
                {
                    SaveCsvLine(sb, "Tau-" + roiKey.Label + "-ch" + (channel + 1), entries.Select(x => FormatCsvValue(GetPointValue(x, roiKey, p => p.TauNs))));
                    SaveCsvLine(sb, "G-" + roiKey.Label + "-ch" + (channel + 1), entries.Select(x => FormatCsvValue(GetPointValue(x, roiKey, p => p.G))));
                    SaveCsvLine(sb, "S-" + roiKey.Label + "-ch" + (channel + 1), entries.Select(x => FormatCsvValue(GetPointValue(x, roiKey, p => p.S))));
                    SaveCsvLine(sb, "Photons-" + roiKey.Label + "-ch" + (channel + 1), entries.Select(x => FormatCsvValue(GetPointValue(x, roiKey, p => p.PhotonCount))));
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string PhasorTimeCourseFileName(FLIMData data)
        {
            string baseName = string.IsNullOrEmpty(data.baseName)
                ? Path.GetFileNameWithoutExtension(data.fileName)
                : data.baseName;
            return Path.Combine(data.pathName, "Analysis", baseName + "_PhasorTimeCourse.csv");
        }

        private static string TrySaveText(string fileName, string text)
        {
            try
            {
                File.WriteAllText(fileName, text);
                return fileName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving " + fileName + ": " + ex.Message);
            }

            string directory = Path.GetDirectoryName(fileName);
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            for (int i = 1; i < 100; i++)
            {
                string fallback = Path.Combine(directory, baseName + "(" + i + ")" + extension);
                try
                {
                    File.WriteAllText(fallback, text);
                    return fallback;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error saving " + fallback + ": " + ex.Message);
                }
            }

            return fileName;
        }

        private List<PhasorTimeCourseEntry> GetSortedEntries()
        {
            return timeCourseEntries
                .OrderBy(x => x.TimeMilliseconds)
                .ThenBy(x => x.FileNumber)
                .ThenBy(x => x.Page)
                .ThenBy(x => x.Channel)
                .ToList();
        }

        private static List<RoiTimeCourseKey> GetRoiKeys(IEnumerable<PhasorTimeCourseEntry> entries)
        {
            var keys = new List<RoiTimeCourseKey>();

            foreach (PhasorTimeCourseEntry entry in entries)
            {
                if (entry.RoiPoints == null)
                    continue;

                foreach (RoiPhasorPoint point in entry.RoiPoints)
                {
                    if (point == null)
                        continue;

                    if (!keys.Any(x => x.RoiId == point.RoiId && x.Label == point.Label))
                        keys.Add(new RoiTimeCourseKey { RoiId = point.RoiId, Label = point.Label });
                }
            }

            return keys
                .OrderBy(x => x.RoiId < 0 ? int.MaxValue : x.RoiId)
                .ThenBy(x => x.Label)
                .ToList();
        }

        private int GetCurrentRoiId()
        {
            var data = image_display?.FLIM_ImgData;
            if (data?.ROIs != null && data.currentRoi >= 0 && data.currentRoi < data.ROIs.Count)
                return data.ROIs[data.currentRoi].ID;

            return -1;
        }

        private static double GetRelativeTimeSeconds(PhasorTimeCourseEntry entry, double baseMilliseconds)
        {
            if (entry.TimeMilliseconds > 0.0 && baseMilliseconds > 0.0)
                return entry.TimeMilliseconds / 1000.0 - baseMilliseconds / 1000.0;

            return entry.Page;
        }

        private static RoiPhasorPoint FindPoint(RoiPhasorPoint[] points, RoiTimeCourseKey key)
        {
            if (points == null || key == null)
                return null;

            return points.FirstOrDefault(x => x.RoiId == key.RoiId && x.Label == key.Label);
        }

        private static double GetPointValue(PhasorTimeCourseEntry entry, RoiTimeCourseKey key, Func<RoiPhasorPoint, double> selector)
        {
            RoiPhasorPoint point = FindPoint(entry.RoiPoints, key);
            return point == null ? double.NaN : selector(point);
        }

        private static RoiPhasorPoint[] CloneRoiPoints(RoiPhasorPoint[] points)
        {
            if (points == null || points.Length == 0)
                return Array.Empty<RoiPhasorPoint>();

            var cloned = new RoiPhasorPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                RoiPhasorPoint point = points[i];
                cloned[i] = new RoiPhasorPoint
                {
                    RoiId = point.RoiId,
                    Label = point.Label,
                    G = point.G,
                    S = point.S,
                    TauNs = point.TauNs,
                    PhotonCount = point.PhotonCount,
                };
            }

            return cloned;
        }

        private static void SaveCsvLine(StringBuilder sb, string title, IEnumerable<string> values)
        {
            sb.Append(title);
            foreach (string value in values)
            {
                sb.Append(',');
                sb.Append(value);
            }
            sb.AppendLine();
        }

        private static string FormatCsvValue(double value)
        {
            if (double.IsNaN(value))
                return "NaN";
            if (double.IsPositiveInfinity(value))
                return "Infinity";
            if (double.IsNegativeInfinity(value))
                return "-Infinity";

            return value.ToString("G17", CultureInfo.InvariantCulture);
        }

        private void PhasorPlotBox_MouseMove(object sender, MouseEventArgs e)
        {
            PointF? imagePoint = TranslateToImagePoint(phasorPlotBox, e.Location);
            if (!imagePoint.HasValue || plotRoiHits == null || plotRoiHits.Length == 0)
            {
                ClearPlotToolTip();
                return;
            }

            string tooltipText = null;
            for (int i = 0; i < plotRoiHits.Length; i++)
            {
                if (plotRoiHits[i].Bounds.Contains(imagePoint.Value))
                {
                    tooltipText = plotRoiHits[i].ToolTipText;
                    break;
                }
            }

            if (tooltipText == null)
                tooltipText = GetCurveToolTipText(imagePoint.Value, currentSyncRateHz);

            if (tooltipText == currentToolTipText)
                return;

            currentToolTipText = tooltipText ?? string.Empty;
            if (currentToolTipText.Length == 0)
            {
                roiToolTip.Hide(phasorPlotBox);
                roiToolTip.SetToolTip(phasorPlotBox, string.Empty);
            }
            else
            {
                roiToolTip.SetToolTip(phasorPlotBox, currentToolTipText);
            }
        }

        private void PhasorPlotBox_MouseLeave(object sender, EventArgs e)
        {
            ClearPlotToolTip();
        }

        private void ClearPlotToolTip()
        {
            if (currentToolTipText.Length == 0)
                return;

            currentToolTipText = string.Empty;
            roiToolTip.Hide(phasorPlotBox);
            roiToolTip.SetToolTip(phasorPlotBox, string.Empty);
        }

        private static PointF? TranslateToImagePoint(PictureBox pictureBox, Point clientPoint)
        {
            if (pictureBox?.Image == null)
                return null;

            Rectangle client = pictureBox.ClientRectangle;
            if (client.Width <= 0 || client.Height <= 0)
                return null;

            float imageWidth = pictureBox.Image.Width;
            float imageHeight = pictureBox.Image.Height;
            float imageAspect = imageWidth / imageHeight;
            float clientAspect = client.Width / (float)client.Height;

            RectangleF displayRect;
            if (clientAspect > imageAspect)
            {
                float drawWidth = client.Height * imageAspect;
                float left = client.Left + (client.Width - drawWidth) * 0.5F;
                displayRect = new RectangleF(left, client.Top, drawWidth, client.Height);
            }
            else
            {
                float drawHeight = client.Width / imageAspect;
                float top = client.Top + (client.Height - drawHeight) * 0.5F;
                displayRect = new RectangleF(client.Left, top, client.Width, drawHeight);
            }

            if (!displayRect.Contains(clientPoint))
                return null;

            float x = (clientPoint.X - displayRect.Left) / displayRect.Width * imageWidth;
            float y = (clientPoint.Y - displayRect.Top) / displayRect.Height * imageHeight;
            return new PointF(x, y);
        }

        private static string GetCurveToolTipText(PointF imagePoint, int syncRateHz)
        {
            RectangleF plotRect = GetPlotRectangle();
            if (imagePoint.X < plotRect.Left || imagePoint.X > plotRect.Right)
                return null;

            double g = (imagePoint.X - plotRect.Left) / plotRect.Width;
            g = Clamp(g, 0.0, 1.0);

            double circleS = Math.Sqrt(Math.Max(0.0, g - g * g));
            if (circleS < 0.0 || circleS > 0.5)
                return null;

            float circleY = plotRect.Bottom - (float)((circleS - PlotMinS) / (PlotMaxS - PlotMinS) * plotRect.Height);
            if (Math.Abs(imagePoint.Y - circleY) > CurveHoverTolerancePx)
                return null;

            double tauNs;
            if (g <= 1e-9 || syncRateHz <= 0)
                tauNs = double.PositiveInfinity;
            else
                tauNs = (circleS / (g * 2.0 * Math.PI * syncRateHz)) * 1e9;

            string tauText = double.IsInfinity(tauNs)
                ? "tau=infinity"
                : $"tau={tauNs:0.00} ns";

            return $"Curve\ng={g:0.000}\ns={circleS:0.000}\n{tauText}";
        }

        private static RectangleF GetPlotRectangle()
        {
            return new RectangleF(PlotPadding, 12, PlotCanvasSize - PlotPadding - 12, PlotCanvasSize - PlotPadding - 12);
        }

        private static void ReplaceBitmap(PictureBox pictureBox, Bitmap bitmap)
        {
            var old = pictureBox.Image;
            pictureBox.Image = bitmap;
            old?.Dispose();
        }

        private static Bitmap BuildPhasorImage(PhasorSnapshot snapshot, float[,] gMap, float[,] sMap, double[,] intensityMap, double maxIntensity)
        {
            int height = gMap.GetLength(0);
            int width = gMap.GetLength(1);
            var tauMap = new float[height, width];
            double omega = 2.0 * Math.PI * Math.Max(1, snapshot.SyncRateHz);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double intensity = intensityMap[y, x];
                    if (intensity <= 0.0)
                        continue;

                    double g = gMap[y, x];
                    double s = sMap[y, x];
                    double tauNs = (g > 1e-9 && omega > 0.0) ? (s / (g * omega)) * 1e9 : 0.0;
                    if (double.IsNaN(tauNs) || double.IsInfinity(tauNs) || tauNs < 0.0)
                        tauNs = 0.0;

                    tauMap[y, x] = (float)tauNs;
                }
            }

            ushort[,] intensityImage = snapshot.ProjectImage ?? BuildIntensityImage(intensityMap, maxIntensity);
            double[] lifetimeRange = GetLifetimeRange(snapshot, tauMap);
            double[] intensityRange = GetIntensityRange(snapshot, intensityMap);
            return ImageProcessing.FormatImageFLIM(
                intensityRange,
                lifetimeRange,
                snapshot.ThresholdRange ?? new double[] { 0.0, -1.0 },
                tauMap,
                intensityImage,
                false,
                snapshot.ColorScheme);
        }

        private static ushort[,] BuildIntensityImage(double[,] intensityMap, double maxIntensity)
        {
            int height = intensityMap.GetLength(0);
            int width = intensityMap.GetLength(1);
            var intensityImage = new ushort[height, width];
            double maxIntensityClamped = Math.Max(maxIntensity, 1.0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double intensity = intensityMap[y, x];
                    if (intensity <= 0.0)
                        continue;

                    intensityImage[y, x] = (ushort)Math.Round(Clamp(intensity / maxIntensityClamped, 0.0, 1.0) * ushort.MaxValue);
                }
            }

            return intensityImage;
        }

        private static double[] GetLifetimeRange(PhasorSnapshot snapshot, float[,] tauMap)
        {
            if (snapshot.LifetimeRange != null && snapshot.LifetimeRange.Length >= 2 && snapshot.LifetimeRange[1] > snapshot.LifetimeRange[0])
                return (double[])snapshot.LifetimeRange.Clone();

            var valid = tauMap.Cast<float>().Where(v => v > 0.0F && !float.IsNaN(v) && !float.IsInfinity(v)).Select(v => (double)v).ToArray();
            if (valid.Length == 0)
                return new double[] { 0.0, 1.0 };

            double min = valid.Min();
            double max = valid.Max();
            if (max <= min)
                max = min + 1.0;

            return new double[] { min, max };
        }

        private static double[] GetIntensityRange(PhasorSnapshot snapshot, double[,] intensityMap)
        {
            if (snapshot.IntensityRange != null && snapshot.IntensityRange.Length >= 2 && snapshot.IntensityRange[1] > snapshot.IntensityRange[0])
                return (double[])snapshot.IntensityRange.Clone();

            var valid = intensityMap.Cast<double>().Where(v => v > 0.0).ToArray();
            if (valid.Length == 0)
                return new double[] { 0.0, 1.0 };

            double min = valid.Min();
            double max = valid.Max();
            if (max <= min)
                max = min + 1.0;

            return new double[] { min, max };
        }

        private static PlotRenderResult BuildPhasorPlot(int[,] histogram, int maxBin, double avgG, double avgS, RoiPhasorPoint[] roiPoints)
        {
            const int canvasSize = PlotCanvasSize;
            RectangleF plotRectF = GetPlotRectangle();
            Rectangle plotRect = Rectangle.Round(plotRectF);

            Bitmap plotBitmap = new Bitmap(canvasSize, canvasSize, PixelFormat.Format24bppRgb);
            var roiMarkers = new System.Collections.Generic.List<RoiMarkerHit>();

            using (Graphics graphics = Graphics.FromImage(plotBitmap))
            {
                graphics.Clear(Color.FromArgb(10, 10, 10));
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                using (Bitmap histBitmap = BuildHistogramBitmap(histogram, maxBin))
                {
                    graphics.DrawImage(histBitmap, plotRect);
                }

                using (Pen axisPen = new Pen(Color.FromArgb(170, 170, 170), 1.2F))
                using (Pen circlePen = new Pen(Color.FromArgb(100, 200, 255), 1.4F))
                using (Brush textBrush = new SolidBrush(Color.Gainsboro))
                using (Brush pointBrush = new SolidBrush(Color.White))
                using (Font font = new Font("Segoe UI", 9F))
                {
                    graphics.DrawRectangle(axisPen, plotRect);

                    PointF[] semicircle = new PointF[181];
                    for (int i = 0; i <= 180; i++)
                    {
                        double theta = Math.PI * i / 180.0;
                        double g = 0.5 + 0.5 * Math.Cos(theta);
                        double s = 0.5 * Math.Sin(theta);
                        semicircle[i] = new PointF(
                            plotRect.Left + (float)(g * plotRect.Width),
                            plotRect.Bottom - (float)((s - PlotMinS) / (PlotMaxS - PlotMinS) * plotRect.Height));
                    }
                    graphics.DrawLines(circlePen, semicircle);

                    float avgX = plotRect.Left + (float)(avgG * plotRect.Width);
                    float avgY = plotRect.Bottom - (float)((avgS - PlotMinS) / (PlotMaxS - PlotMinS) * plotRect.Height);
                    graphics.FillEllipse(pointBrush, avgX - 4F, avgY - 4F, 8F, 8F);

                    if (roiPoints != null)
                    {
                        for (int i = 0; i < roiPoints.Length; i++)
                        {
                            RoiPhasorPoint roiPoint = roiPoints[i];
                            Color roiColor = ColorFromHsv((i * 73.0) % 360.0, 0.85, 1.0);
                            using (Brush roiBrush = new SolidBrush(roiColor))
                            using (Pen roiPen = new Pen(Color.Black, 1F))
                            {
                                float x = plotRect.Left + (float)(roiPoint.G * plotRect.Width);
                                float y = plotRect.Bottom - (float)((roiPoint.S - PlotMinS) / (PlotMaxS - PlotMinS) * plotRect.Height);
                                graphics.FillEllipse(roiBrush, x - 5F, y - 5F, 10F, 10F);
                                graphics.DrawEllipse(roiPen, x - 5F, y - 5F, 10F, 10F);

                                string tauText = double.IsNaN(roiPoint.TauNs)
                                    ? $"{roiPoint.Label}"
                                    : $"{roiPoint.Label}  tau={roiPoint.TauNs:0.00} ns";
                                string hoverText = double.IsNaN(roiPoint.TauNs)
                                    ? $"{roiPoint.Label}\ng={roiPoint.G:0.000}\ns={roiPoint.S:0.000}"
                                    : $"{roiPoint.Label}\ntau={roiPoint.TauNs:0.00} ns\ng={roiPoint.G:0.000}\ns={roiPoint.S:0.000}";

                                float labelX = Math.Min(plotRect.Right - 6F, x + 8F);
                                float labelY = Math.Max(plotRect.Top + 2F, y - 14F);
                                graphics.DrawString(tauText, font, roiBrush, labelX, labelY);

                                roiMarkers.Add(new RoiMarkerHit
                                {
                                    Bounds = new RectangleF(x - 8F, y - 8F, 16F, 16F),
                                    ToolTipText = hoverText,
                                });
                            }
                        }
                    }

                    graphics.DrawString("g", font, textBrush, plotRect.Right - 8, plotRect.Bottom + 8,
                        new StringFormat { Alignment = StringAlignment.Far });
                    graphics.DrawString("s", font, textBrush, 8, plotRect.Top,
                        new StringFormat { Alignment = StringAlignment.Near });
                    graphics.DrawString("0", font, textBrush, plotRect.Left - 4, plotRect.Bottom + 8);
                    graphics.DrawString("1", font, textBrush, plotRect.Right - 12, plotRect.Bottom + 8);
                    graphics.DrawString($"{PlotMaxS:0.00}", font, textBrush, 4, plotRect.Top - 4);
                    graphics.DrawString($"{PlotMinS:0.00}", font, textBrush, 4, plotRect.Bottom - 18);
                }
            }

            return new PlotRenderResult(plotBitmap, roiMarkers.ToArray());
        }

        private static Bitmap BuildHistogramBitmap(int[,] histogram, int maxBin)
        {
            byte[] bytes = new byte[HistogramBins * HistogramBins * 3];
            double logMax = Math.Log10(maxBin + 1.0);

            for (int x = 0; x < HistogramBins; x++)
            {
                for (int y = 0; y < HistogramBins; y++)
                {
                    int count = histogram[x, y];
                    if (count <= 0 || logMax <= 0.0)
                        continue;

                    double norm = Math.Log10(count + 1.0) / logMax;
                    Color color = HeatMap(norm);
                    int drawY = HistogramBins - 1 - y;
                    int idx = (drawY * HistogramBins + x) * 3;
                    bytes[idx + 0] = color.B;
                    bytes[idx + 1] = color.G;
                    bytes[idx + 2] = color.R;
                }
            }

            return CreateBitmap(HistogramBins, HistogramBins, bytes);
        }

        private static Bitmap CreateBitmap(int width, int height, byte[] rgbBytes)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            try
            {
                int srcStride = width * 3;
                if (bmpData.Stride == srcStride)
                {
                    Marshal.Copy(rgbBytes, 0, bmpData.Scan0, rgbBytes.Length);
                }
                else
                {
                    for (int y = 0; y < height; y++)
                    {
                        IntPtr rowPtr = IntPtr.Add(bmpData.Scan0, y * bmpData.Stride);
                        Marshal.Copy(rgbBytes, y * srcStride, rowPtr, srcStride);
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }

            return bitmap;
        }

        private static Color HeatMap(double value)
        {
            value = Clamp(value, 0.0, 1.0);

            if (value < 0.33)
                return Lerp(Color.FromArgb(0, 0, 0), Color.FromArgb(0, 90, 190), value / 0.33);
            if (value < 0.66)
                return Lerp(Color.FromArgb(0, 90, 190), Color.FromArgb(255, 150, 0), (value - 0.33) / 0.33);

            return Lerp(Color.FromArgb(255, 150, 0), Color.FromArgb(255, 255, 255), (value - 0.66) / 0.34);
        }

        private static Color Lerp(Color start, Color end, double amount)
        {
            amount = Clamp(amount, 0.0, 1.0);
            int r = (int)Math.Round(start.R + (end.R - start.R) * amount);
            int g = (int)Math.Round(start.G + (end.G - start.G) * amount);
            int b = (int)Math.Round(start.B + (end.B - start.B) * amount);
            return Color.FromArgb(r, g, b);
        }

        private static Color ColorFromHsv(double hue, double saturation, double value)
        {
            hue = hue % 360.0;
            if (hue < 0.0)
                hue += 360.0;

            saturation = Clamp(saturation, 0.0, 1.0);
            value = Clamp(value, 0.0, 1.0);

            double c = value * saturation;
            double x = c * (1.0 - Math.Abs((hue / 60.0) % 2.0 - 1.0));
            double m = value - c;

            double r = 0.0;
            double g = 0.0;
            double b = 0.0;

            if (hue < 60.0)
            {
                r = c;
                g = x;
            }
            else if (hue < 120.0)
            {
                r = x;
                g = c;
            }
            else if (hue < 180.0)
            {
                g = c;
                b = x;
            }
            else if (hue < 240.0)
            {
                g = x;
                b = c;
            }
            else if (hue < 300.0)
            {
                r = x;
                b = c;
            }
            else
            {
                r = c;
                b = x;
            }

            return Color.FromArgb(
                (int)Math.Round((r + m) * 255.0),
                (int)Math.Round((g + m) * 255.0),
                (int)Math.Round((b + m) * 255.0));
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private static string FormatSigma(double sigmaNs)
        {
            if (double.IsNaN(sigmaNs) || double.IsInfinity(sigmaNs) || sigmaNs <= 0.0)
                return string.Empty;

            return $"tG={sigmaNs:0.000} ns | ";
        }

        private sealed class PhasorSnapshot
        {
            public int Channel;
            public int Page;
            public int FileNumber;
            public string FileName;
            public string BaseName;
            public double TimeMilliseconds;
            public ushort[,,] Raw;
            public int TimeStart;
            public int TimeEnd;
            public double PsPerBin;
            public int SyncRateHz;
            public string SyncRateSource;
            public double PhasorOriginNs;
            public string OriginSource;
            public double ImageOffsetNs;
            public double IrfSigmaNs;
            public int LeadingTrimBins;
            public int TrailingTrimBins;
            public ImageProcessing.ColorScheme ColorScheme;
            public double[] LifetimeRange;
            public double[] IntensityRange;
            public double[] ThresholdRange;
            public ushort[,] ProjectImage;
            public RoiSnapshot[] Rois;
        }

        private sealed class RoiSnapshot
        {
            public int RoiId;
            public string Label;
            public Point[] Pixels;
        }

        private sealed class RoiPhasorPoint
        {
            public int RoiId;
            public string Label;
            public double G;
            public double S;
            public double TauNs;
            public double PhotonCount;
        }

        private sealed class RoiMarkerHit
        {
            public RectangleF Bounds;
            public string ToolTipText;
        }

        private sealed class PlotRenderResult
        {
            public readonly Bitmap Bitmap;
            public readonly RoiMarkerHit[] RoiMarkers;

            public PlotRenderResult(Bitmap bitmap, RoiMarkerHit[] roiMarkers)
            {
                Bitmap = bitmap;
                RoiMarkers = roiMarkers;
            }
        }

        private sealed class PhasorTimeCourseEntry
        {
            public int Channel;
            public int Page;
            public int FileNumber;
            public string FileName;
            public string BaseName;
            public double TimeMilliseconds;
            public RoiPhasorPoint[] RoiPoints;
        }

        private sealed class RoiTimeCourseKey
        {
            public int RoiId;
            public string Label;
        }

        private sealed class PhasorResult : IDisposable
        {
            public readonly Bitmap ImageBitmap;
            public readonly Bitmap PlotBitmap;
            public readonly string StatusText;
            public readonly RoiMarkerHit[] RoiMarkers;
            public readonly int SyncRateHz;
            public readonly RoiPhasorPoint[] RoiPoints;

            public PhasorResult(
                Bitmap imageBitmap,
                Bitmap plotBitmap,
                string statusText,
                RoiMarkerHit[] roiMarkers,
                int syncRateHz,
                RoiPhasorPoint[] roiPoints)
            {
                ImageBitmap = imageBitmap;
                PlotBitmap = plotBitmap;
                StatusText = statusText;
                RoiMarkers = roiMarkers;
                SyncRateHz = syncRateHz;
                RoiPoints = roiPoints ?? Array.Empty<RoiPhasorPoint>();
            }

            public static PhasorResult Empty(string statusText)
            {
                return new PhasorResult(null, null, statusText, Array.Empty<RoiMarkerHit>(), 0, Array.Empty<RoiPhasorPoint>());
            }

            public void Dispose()
            {
                ImageBitmap?.Dispose();
                PlotBitmap?.Dispose();
            }
        }
    }
}
