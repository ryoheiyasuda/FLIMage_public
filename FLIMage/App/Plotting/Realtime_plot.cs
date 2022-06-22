using FLIMage.Analysis;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace FLIMage.Plotting
{
    public partial class plot_timeCourse : Form
    {
        //fastPlot realtimePlot;
        public bool real_time;
        public int channel;
        public bool calc_Fit = true;
        public bool calc_upon_open = true;

        public bool showCurrentTF = true;

        public plotWhat plotType = plotWhat.sumIntensity_bg;

        public bool autoScale = true;

        PlotOnPictureBox plot;

        TimeCourse_Files TCF; // = new TimeCourse_Files(); 
        TimeCourse TC;

        Image_Display image_display;
        FLIMageMain flimage;

        SettingManager settingManager;
        String settingName = "plot_regular";

        WindowLocManager winManager;
        int min_windowSize = 650;
        String WindowName_realtime = "plot_realtime.loc";
        String WindowName_regular = "plot_regular.loc";
        String WindowName = "plot_realtime.loc";

        public plot_timeCourse(bool realtime, Image_Display imd)
        {
            real_time = realtime;
            InitializeComponent();

            image_display = imd;
            flimage = imd.flimage;

            plot = new PlotOnPictureBox(RealtimePlot);


            if (real_time)
            {
                calcFitCheck.Visible = false;
                Lifetime_fit_radio.Visible = false;
                Fraction2_fit_radio.Visible = false;
                Fraction2_radio.Visible = false;
                //DeleteCurrent.Visible = false;
                //TC_Reset.Visible = false;
                CalcTimeCourse.Visible = false;
                CalculateSinglePage.Visible = false;
                CalculateUponOpen.Visible = false;
                OpenExcel.Visible = false;
                this.Text = "Realtime plot";
            }
            else
            {
                this.Text = "Time course";
            }

        }

        public void plotNow_noRealtime(TimeCourse_Files TCF1, TimeCourse TC1, Image_Display imd, int channel1)
        {
            channel = channel1;
            TCF = TCF1;
            TC = TC1;
            image_display = imd;

            updatePlot();
        }

        public void updatePlot()
        {
            if (real_time)
                return;

            calcFitCheck.Checked = calc_Fit;
            plot.ClearData();
            plotTimeCourse();
            plot.UpdatePlot();
        }

        public void plotNow_realtime(List<double> realtime_data1)
        {
            plot.ClearData();
            plot.AddData(realtime_data1, "-r", 1f);
            plot_titles();
            plot.UpdatePlot();
        }

        public void WarningTextDisplay(String str)
        {
            Warning.Text = str;
        }

        private void plot_titles()
        {
            if (real_time)
                plot.XTitle = "Time Point";
            else
                plot.XTitle = "Time (s)";

            if (plotType == plotWhat.tau_m || plotType == plotWhat.tau_m_fit)
                plot.YTitle = "Fluorescence lifetime (ns)";
            else if (plotType == plotWhat.farction2_fit || plotType == plotWhat.fraction2)
                plot.YTitle = "Binding fraction";
            else
                plot.YTitle = "Intensity (# photon)";
        }

        private void updateWhatToMeasure()
        {
            if (meanIntensity_radio.Checked && SubtractCheck.Checked)
                plotType = plotWhat.meanIntensity_bg;
            else if (meanIntensity_radio.Checked && !SubtractCheck.Checked)
                plotType = plotWhat.meanIntensity;
            else if (sumIntensity_radio.Checked && SubtractCheck.Checked)
                plotType = plotWhat.sumIntensity_bg;
            else if (sumIntensity_radio.Checked && !SubtractCheck.Checked)
                plotType = plotWhat.sumIntensity;
            else if (Fraction2_radio.Checked)
                plotType = plotWhat.fraction2;
            else if (Lifetime_radio.Checked)
                plotType = plotWhat.tau_m;
            else if (Lifetime_fit_radio.Checked)
                plotType = plotWhat.tau_m_fit;
            else if (Fraction2_fit_radio.Checked)
                plotType = plotWhat.farction2_fit;
        }


        private void Intensity_radio_Click(object sender, EventArgs e)
        {
            updateWhatToMeasure();
            if (!real_time)
                updatePlot();
        }


        private void plotTimeCourse()
        {
            int startP = 0; // range.Min();
            int endP = 0;

            if (TCF == null)
                return;

            int c = channel;

            plot_titles();

            List<String> strList = new List<String>();
            string[] paramList = ImageInfo.paramNames;
            string paramForPlot = "";
            foreach (var param in paramList)
            {
                if (!param.EndsWith("_bg"))
                {
                    var param_mod = param;
                    if (param.EndsWith("Intensity") && SubtractCheck.Checked)
                        param_mod = param + "_bg";

                    Control[] checkboxes = Controls.Find(param + "_radio", true);
                    if (checkboxes.Length > 0 && ((RadioButton)checkboxes[0]).Checked)
                    {
                        paramForPlot = param_mod;
                    }
                }
            }

            if (TCF != null)
            {
                double[] time1 = TCF.time_seconds;

                if (TCF.nROI == 0)
                {
                    double[] curve = null;
                    var crv = (double[][])TCF.GetType().GetField(paramForPlot).GetValue(TCF);
                    if (crv != null && crv.Length > c && crv[c] != null)
                        curve = crv[c];
                    if (curve != null)
                    {
                        double[] curve1 = curve;
                        plot.AddData(time1, curve1, "-r", 1);
                        strList.Add("Selected ROI");
                    }
                }
                else
                {
                    int currentRoi = image_display.FLIM_ImgData.currentRoi;

                    if (currentRoi >= 0 && image_display.FLIM_ImgData.ROIs.Count > currentRoi)
                        currentRoi = image_display.FLIM_ImgData.ROIs[currentRoi].ID;
                    else
                        currentRoi = -1;

                    Pen currentRoiPen = new Pen(Brushes.Red, 1);
                    double[] currentCurve = null;
                    int hilightID = -1;
                    for (int roi = 0; roi < TCF.nROI; roi++)
                    {
                        int roiID = TCF.UniqueIDs.ToList()[roi];
                        double[] curveROI = null;

                        var crv = (double[,][])TCF.GetType().GetField(paramForPlot + "_ROI").GetValue(TCF);
                        if (crv != null && crv.GetLength(0) > c && crv.GetLength(1) > roi)
                            curveROI = crv[c, roi];

                        if (curveROI != null)
                        {
                            double[] curve1 = (double[])curveROI;
                            if (TCF.nData < 200)
                                plot.AddData(time1, curve1, "o-", 1f);
                            else
                                plot.AddData(time1, curve1, "-", 1f);

                            if (roiID >= 1000)
                            {
                                int roi1_ID = roiID / 1000;
                                int roi2_ID = roiID - (1000 * roi1_ID) + 1;
                                strList.Add("ROI-" + roi1_ID.ToString() + "-" + roi2_ID);

                            }
                            else
                                strList.Add("ROI-" + (roiID + 1));

                            if (roiID == currentRoi)
                            {
                                currentCurve = (double[])curve1.Clone();
                                hilightID = strList.Count - 1;
                            }
                        }
                    } //ROI

                    if (currentCurve != null)
                    {
                        plot.AddData(TCF.time_seconds, currentCurve, "or-", 2);
                        plot.AddLegendWithHighlight(strList, hilightID);
                    }
                    else
                        plot.AddLegend(strList);
                }
            }

            calcFitCheck.Checked = calc_Fit;
            plot.UpdatePlot();
        }

        public void intensity_onlyData()
        {
            meanIntensity_radio.Checked = true;
            if (plotType != plotWhat.meanIntensity && plotType != plotWhat.meanIntensity_bg
                && plotType != plotWhat.sumIntensity && plotType != plotWhat.sumIntensity_bg)
                plotType = plotWhat.meanIntensity;
        }

        private void plot_timeCourse_Resize(object sender, EventArgs e)
        {
        }

        private void TC_Reset_Click(object sender, EventArgs e)
        {
            if (!real_time)
            {
                if (TCF != null)
                {
                    TC = new TimeCourse();
                    var iminfo = new ImageInfo(image_display.FLIM_ImgData);

                    int page = image_display.FLIM_ImgData.currentPage;
                    if (image_display.FLIM_ImgData.ZProjection)
                        page = 0;

                    TC.AddFile(iminfo, page);
                    TC.calculate();

                    TCF = new TimeCourse_Files();
                    TCF.AddFile(TC);
                    TCF.calculate();

                    image_display.TCF = TCF;
                    image_display.TC = TC;
                    image_display.SaveTimeCourse();
                }

                plotNow_noRealtime(TCF, TC, image_display, image_display.currentChannel);
                calcFitCheck.Checked = calc_Fit;

            }
            else
            {
                image_display.realtimeData.Clear();
            }
        }


        private void CalcCurrent_Click(object sender, EventArgs e)
        {
            if (TC != null)
                TC.ImInfos.Clear();

            image_display.CalculateTimecourse(true);
            updatePlot();
        }


        private void CalcFitCheck_CheckedChanged(object sender, EventArgs e)
        {
            calc_Fit = calcFitCheck.Checked;
        }

        public void TurnOnCalcFit(bool ON)
        {
            calc_Fit = ON;
            calcFitCheck.Checked = calc_Fit;
        }

        public void TurnOnCalcUponOpen(bool ON)
        {
            //calc_upon_open = ON; Automatic.
            CalculateUponOpen.Checked = ON;
        }
        //////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////

        private void CalculateSinglePage_Click(object sender, EventArgs e)
        {
            image_display.CalculateCurrentPage(true);
            updatePlot();
        }

        private void plot_timeCourse_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();

            flimage.ToolWindowClosed();

            SaveWindowLocation();
        }

        private void plot_timeCourse_Load(object sender, EventArgs e)
        {
            WindowName = real_time ? WindowName_realtime : WindowName_regular;

            winManager = new WindowLocManager(this, WindowName, image_display.FLIM_ImgData.State.Files.windowsInfoPath);

            DefaultWindowLocation();
            winManager.LoadWindowLocation(true);

            if (!real_time)
                InitializeSetting();

            updateWhatToMeasure();
        }

        private void DefaultWindowLocation()
        {
            int YplotSize = Screen.PrimaryScreen.WorkingArea.Size.Height - image_display.Bottom;

            if (YplotSize > min_windowSize)
                YplotSize = min_windowSize;

            Size plotSiz = new Size(new Point(image_display.Width / 2, YplotSize));

            if (real_time)
            {
                Location = new Point(image_display.Location.X, image_display.Bottom); //692
                Size = plotSiz;
            }
            else
            {
                Location = new Point(image_display.Left + image_display.Width / 2, image_display.Bottom);
                Size = plotSiz;
            }
        }

        private void OpenExcel_Click(object sender, EventArgs e)
        {
            String fileName = image_display.TimeCourseFileName();
            if (File.Exists(fileName))
            {
                Process ExternalProcess = new Process();
                ExternalProcess.StartInfo.FileName = fileName;
                ExternalProcess.Start();
            }
            else
            {
                MessageBox.Show("Could not find file: " + fileName);
            }

        }

        private void CalculateUponOpen_CheckedChanged(object sender, EventArgs e)
        {
            calc_upon_open = CalculateUponOpen.Checked;
        }

        void InitializeSetting()
        {
            settingManager = new SettingManager(settingName, image_display.FLIM_ImgData.State.Files.initFolderPath);
            settingManager.AddToDict(calcFitCheck);
            settingManager.AddToDict(CalculateUponOpen);
            settingManager.AddToDict(SubtractCheck);
            settingManager.LoadToObject();
        }

        public void SaveWindowLocation()
        {
            if (!real_time)
                settingManager.SaveFromObject();
            winManager.SaveWindowLocation();
        }


        public enum plotWhat
        {
            meanIntensity = 1,
            sumIntensity = 2,
            fraction2 = 3,
            tau_m = 4,
            farction2_fit = 5,
            tau_m_fit = 6,
            meanIntensity_bg = 7,
            sumIntensity_bg = 8,
        }

    }
}
