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

namespace FLIMimage
{
    public partial class plot_timeCourse : Form
    {
        //fastPlot realtimePlot;
        public bool real_time;
        public int channel;
        public bool calc_Fit = true;
        public bool calc_upon_open = true;

        public bool showCurrentTF = true;

        public List<double> realtimeData = new List<double>();
        public List<double> realtimeTime = new List<double>();
        public plotWhat plotType = plotWhat.meanIntensity;

        public bool autoScale = true;

        plot_panel pp;

        TimeCourse_Files TCF; // = new TimeCourse_Files(); 
        TimeCourse TC;

        Image_Display image_display;
        FLIMageMain FLIMage;

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
            FLIMage = imd.FLIMage;

            pp = new plot_panel(RealtimePlot.Width, RealtimePlot.Height);

            if (real_time)
            {
                calcFitCheck.Visible = false;
                tau_m_fit_radio.Visible = false;
                fraction2_fit_radio.Visible = false;
                fraction2_radio.Visible = false;
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

            RealtimePlot.Invalidate();
        }

        public void updatePlot()
        {
            RealtimePlot.Invalidate();
        }

        public void plotNow_realtime(List<double> realtimeData1)
        {
            realtimeData = realtimeData1;
            RealtimePlot.Invalidate();
        }

        public void WarningTextDisplay(String str)
        {
            Warning.Text = str;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                pp.clearData();

                //Debug.WriteLine("autoScale: " + pp.autoScaleX);


                if (real_time)
                    plot_realtime(e);
                else
                    plotTimeCourse(e);
            }
            catch (Exception Ex)
            {
                if (real_time)
                    Debug.WriteLine("Problem in Drawing realtime_plot: " + Ex.Message);
                else
                    Debug.WriteLine("Problem in Drawing plot data: " + Ex.Message);
            }
            finally { }

            if (pp.drawingROI)
            {
                Pen rectPen = new Pen(Color.Red, (float)0.5);
                e.Graphics.DrawRectangle(rectPen, pp.boxRoi);
            }
        }

        private void plot_realtime(PaintEventArgs e)
        {
            double[] time1 = new double[realtimeData.Count];
            for (int i = 0; i < realtimeData.Count; i++)
                time1[i] = (double)i;

            double[] realtimedata1 = realtimeData.ToArray();
            pp.addData(time1, realtimedata1, new Pen(Brushes.Red, 1), "-");
            //pp.addData(realtimeData);

            pp.plotType = "-";
            pp.XTitle = "Time Point";

            if (!meanIntensity_radio.Checked && !sumIntensity_radio.Checked)
                pp.YTitle = "Fluorescence lifetime (ns)";
            else
                pp.YTitle = "Intensity (#Photons)";
            pp.plot(e);
        }


        public void updateGraph()
        {
            RealtimePlot.Invalidate();
        }

        private void Intensity_radio_Click(object sender, EventArgs e)
        {
            if (meanIntensity_radio.Checked)
                plotType = plotWhat.meanIntensity;
            else if (sumIntensity_radio.Checked)
                plotType = plotWhat.sumIntensity;
            else if (fraction2_radio.Checked)
                plotType = plotWhat.fraction2;
            else if (tau_m_radio.Checked)
                plotType = plotWhat.tau_m;
            else if (tau_m_fit_radio.Checked)
                plotType = plotWhat.tau_m_fit;
            else if (fraction2_fit_radio.Checked)
                plotType = plotWhat.farction2_fit;

            RealtimePlot.Invalidate();
        }


        private void plotTimeCourse(PaintEventArgs e)
        {

            int c = channel;

            pp.XTitle = "Time (s)";
            pp.YTitle = "Fluorescence lifetime (ns)";
            pp.plotType = "-";


            List<String> strList = new List<String>();

            if (TCF != null && TCF.Lifetime != null)
            {
                if (TCF.nROI == 0)
                {
                    double[] curve = TCF.Lifetime[c];
                    double[] curve2 = TC.Lifetime[c];

                    if (fraction2_radio.Checked)
                    {
                        curve = TCF.Fraction2[c];
                        curve2 = TC.Fraction2[c];
                        pp.YTitle = "Binding fraction";
                    }
                    else if (tau_m_fit_radio.Checked)
                    {
                        curve = TCF.Lifetime_fit[c];
                        curve2 = TC.Lifetime_fit[c];
                    }
                    else if (fraction2_fit_radio.Checked)
                    {
                        curve = TCF.Fraction2_fit[c];
                        curve2 = TC.Fraction2_fit[c];
                        pp.YTitle = "Binding fraction";
                    }
                    else if (sumIntensity_radio.Checked)
                    {
                        curve = TCF.sumIntensity[c];
                        curve2 = TC.sumIntensity[c];
                        pp.YTitle = "Intensity";
                    }
                    else if (meanIntensity_radio.Checked)
                    {
                        curve = TCF.Intensity[c];
                        curve2 = TC.Intensity[c];
                        pp.YTitle = "Intensity";
                    }

                    pp.addData(TCF.time_seconds, curve, new Pen(Brushes.Red, 1), "-");

                    //pp.addData(x1, curve);
                    strList.Add("Selected ROI");
                }

                int currentRoi = image_display.FLIM_ImgData.currentRoi;
                if (currentRoi >= 0)
                    currentRoi = image_display.FLIM_ImgData.ROIs[currentRoi].ID - 1;

                Pen currentRoiPen = new Pen(Brushes.Red, 1);
                double[] currentCurve = null;
                int hilightID = -1;
                for (int j = 0; j < TCF.nROI; j++)
                {
                    //
                    //Note that j is ROI id. not hte number.
                    //
                    if (image_display.RoiIDExist(j + 1))
                    {
                        double[] curveROI = TCF.lifetime_ROI[c, j];
                        if (fraction2_radio.Checked)
                        {
                            curveROI = TCF.fraction2_ROI[c, j];
                        }
                        else if (tau_m_fit_radio.Checked)
                        {
                            curveROI = TCF.lifetime_fit_ROI[c, j];
                        }
                        else if (fraction2_fit_radio.Checked)
                        {
                            curveROI = TCF.fraction2_fit_ROI[c, j];
                        }
                        else if (meanIntensity_radio.Checked)
                        {
                            curveROI = TCF.intensity_ROI[c, j];
                        }
                        else if (sumIntensity_radio.Checked)
                        {
                            curveROI = TCF.sumIntensity_ROI[c, j];
                        }

                        pp.addData(TCF.time_seconds, curveROI);
                        if (TCF.nData < 200)
                            pp.applyPlotType("o-");
                        else
                            pp.applyPlotType("-");

                        strList.Add("ROI-" + (j + 1));

                        if (j == currentRoi)
                        {
                            currentCurve = (double[])curveROI.Clone();
                            hilightID = j;
                        }
                    }//If exist
                } //loop roi.

                if (currentCurve != null)
                {
                    pp.addData(TCF.time_seconds, currentCurve, new Pen(Brushes.Red, 2), "-");
                    pp.DrawLegendWithHighlight(e, strList, hilightID);
                }
                else
                    pp.DrawLegend(e, strList);
            }

            pp.autoAxisPosition(e);
            pp.plot(e);
            calcFitCheck.Checked = calc_Fit;
        }

        public void intensity_onlyData()
        {
            meanIntensity_radio.Checked = true;
            plotType = plotWhat.meanIntensity;
        }

        private void plot_timeCourse_Resize(object sender, EventArgs e)
        {
            pp = new plot_panel(RealtimePlot.Width, RealtimePlot.Height);
            RealtimePlot.Invalidate();
            calcFitCheck.Checked = calc_Fit;
        }


        private void TC_Reset_Click(object sender, EventArgs e)
        {
            if (!real_time)
            {
                if (TCF != null)
                {
                    TCF.DeleteAll();
                    TCF.AddFile(TC);
                    TCF.calculate();
                }

                plotNow_noRealtime(TCF, TC, image_display, image_display.currentChannel);
                //image_display.CalculateTimecourse();
                calcFitCheck.Checked = calc_Fit;

            }
            else
            {
                realtimeData.Clear();
            }
            //realtime_plot.Invalidate();
        }


        private void CalcCurrent_Click(object sender, EventArgs e)
        {
            if (TC != null)
                TC.ImInfos.Clear();

            image_display.CalculateTimecourse(true);
            calcFitCheck.Checked = calc_Fit;
            //showCurrentTF = true;
            //Refresh();
            RealtimePlot.Invalidate();
        }


        private void CalcFitCheck_CheckedChanged(object sender, EventArgs e)
        {
            calc_Fit = calcFitCheck.Checked;
        }

        //////////////////////////////////////////////////////////////
        /// <summary>
        /// This is to get a zoom-up view of the plot --- next 3 functions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void realtime_plot_MouseDown(object sender, MouseEventArgs e)
        {
            pp.StartDrawingROI(e);
        }

        private void realtime_plot_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            pp.AutoScaleNow(true);
            RealtimePlot.Invalidate();
        }

        private void realtime_plot_MouseMove(object sender, MouseEventArgs e)
        {
            pp.Draw_DuringMoveMouse(e);
            if (pp.drawingROI)
                RealtimePlot.Invalidate();
        }

        private void realtime_plot_MouseUp(object sender, MouseEventArgs e)
        {
            pp.Finish_DrawoingROI_MouseUp(e);
            RealtimePlot.Invalidate();
        }

        //////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////

        private void CalculateSinglePage_Click(object sender, EventArgs e)
        {
            image_display.CalculateCurrentPage(true);
            RealtimePlot.Invalidate();
        }

        private void plot_timeCourse_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();

            image_display.FLIMage.ToolWindowClosed();

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
        }

    }
}
