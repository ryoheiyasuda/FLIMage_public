using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FLIMage.Plotting
{
    public partial class Plot : Form
    {

        Rectangle boxRoi;
        bool drawing = false;
        Point startPos, currentPos;

        double[][] X;
        double[][] Y;

        double[][] Xpart;
        double[][] Ypart;
        int MaxNChannels = 4;
        int maxSample = 10000;
        //double[] yrange;
        double[] xrange;
        int nChannels;
        int t_nSamples;
        int nSamples;
        double outputRate;

        String mode = "XY";

        String Xtitle, Ytitle;

        double digit = 1000;

        public Plot(double[][] X1, double[][] Y1, String XAxis, String YAxis, double sampleFrequency, String[] legendstrs)
        {
            InitializeComponent();
            outputRate = sampleFrequency;

            String[] dataStr = new String[MaxNChannels];
            for (int dataCh = 0; dataCh < MaxNChannels; dataCh++)
            {
                dataStr[dataCh] = "Data" + (dataCh + 1).ToString();
                chart1.Series[dataStr[dataCh]].Points.Clear();
                if (legendstrs.Length > dataCh)
                    chart1.Series[dataStr[dataCh]].LegendText = legendstrs[dataCh];
            }

            nChannels = Y1.Length;
            nSamples = Y1[0].Length;
            t_nSamples = Y1[0].Length;

            X = X1;
            Y = Y1; // new double[nChannels][];

            xrange = new double[] { 0, X[0].Length - 1 };

            Xtitle = XAxis;
            Ytitle = YAxis;

            mode = "XY";

            plot_chart();
        }

        public Plot(double[][] X1, double[][] Y1, String XAxis, String YAxis, double sampleFrequency, String legendstr)
        {
            InitializeComponent();
            outputRate = sampleFrequency;

            String[] dataStr = new String[MaxNChannels];
            for (int dataCh = 0; dataCh < MaxNChannels; dataCh++)
            {
                dataStr[dataCh] = "Data" + (dataCh + 1).ToString();
                chart1.Series[dataStr[dataCh]].Points.Clear();
                chart1.Series[dataStr[dataCh]].LegendText = legendstr + (dataCh + 1).ToString();
            }

            nChannels = Y1.Length;
            nSamples = Y1[0].Length;
            t_nSamples = Y1[0].Length;

            X = X1;
            Y = Y1; // new double[nChannels][];

            xrange = new double[] { 0, X[0].Length - 1 };

            Xtitle = XAxis;
            Ytitle = YAxis;

            mode = "XY";

            plot_chart();
        }

        public Plot(double[][] XY, String XAxis, String YAxis, double sampleFrequency, String legendstr)
        {
            InitializeComponent();
            Replot(XY, XAxis, YAxis, sampleFrequency, legendstr);
        }

        public Plot(double[,] XY, String XAxis, String YAxis, double sampleFrequency, String legendstr)
        {
            InitializeComponent();
            Replot(XY, XAxis, YAxis, sampleFrequency, legendstr);
        }

        public void Replot(double[][] XY, String XAxis, String YAxis, double sampleFrequency, String legendstr)
        {
            outputRate = sampleFrequency;
            String[] dataStr = new String[MaxNChannels];
            for (int dataCh = 0; dataCh < MaxNChannels; dataCh++)
            {
                dataStr[dataCh] = "Data" + (dataCh + 1).ToString();
                chart1.Series[dataStr[dataCh]].Points.Clear();
                chart1.Series[dataStr[dataCh]].LegendText = legendstr + (dataCh + 1).ToString();
            }

            if (XY == null)
                return;

            nChannels = XY.Length;
            nSamples = XY[0].Length;
            t_nSamples = nSamples;

            X = new double[nChannels][];
            for (int i = 0; i < nChannels; i++)
                X[i] = new double[nSamples];

            Y = XY; // new double[nChannels][];

            for (int ch = 0; ch < nChannels; ch++)
                for (int i = 0; i < nSamples; i++)
                    X[ch][i] = (double)i / outputRate; // * 1000;

            xrange = new double[] { 0, X[0].Length - 1 };
            Xtitle = XAxis;
            Ytitle = YAxis;

            mode = "Y";

            plot_chart();
        }

        public void Replot(double[,] XY, String XAxis, String YAxis, double sampleFrequency, String legendstr)
        {
            outputRate = sampleFrequency;
            String[] dataStr = new String[MaxNChannels];
            for (int dataCh = 0; dataCh < MaxNChannels; dataCh++)
            {
                dataStr[dataCh] = "Data" + (dataCh + 1).ToString();
                chart1.Series[dataStr[dataCh]].Points.Clear();
                chart1.Series[dataStr[dataCh]].LegendText = legendstr + (dataCh + 1).ToString();
            }

            nChannels = XY.GetLength(0);
            nSamples = XY.GetLength(1);
            t_nSamples = nSamples;

            X = new double[nChannels][];
            for (int i = 0; i < nChannels; i++)
                X[i] = new double[nSamples];

            Y = new double[nChannels][];

            for (int i = 0; i < nChannels; i++)
                Y[i] = new double[nSamples];

            for (int ch = 0; ch < nChannels; ch++)
                for (int i = 0; i < nSamples; i++)
                    X[ch][i] = (double)i / outputRate; // * 1000;

            for (int ch = 0; ch < nChannels; ch++)
                for (int i = 0; i < nSamples; i++)
                    Y[ch][i] = XY[ch, i];

            Xtitle = XAxis;
            Ytitle = YAxis;

            mode = "Y";
            plot_chart();
        }

        public void plot_chart()
        {
            double[] maxXarray = new double[nChannels];
            double[] minXarray = new double[nChannels];
            for (int ch = 0; ch < nChannels; ch++)
                maxXarray[ch] = X[ch].Max();

            for (int ch = 0; ch < nChannels; ch++)
                minXarray[ch] = X[ch].Min();

            xrange = new double[] { 0, X[0].Length - 1 };
            recalculateDataAndPlot();

            chart1.Titles["XTitle"].Text = Xtitle;
            chart1.Titles["YTitle"].Text = Ytitle;

            maximizePlot();

            //chart1.ChartAreas[0].AxisX.Maximum = Math.Ceiling(digit * xrange[1] / outputRate) / digit;
            //chart1.ChartAreas[0].AxisX.Minimum = Math.Floor(digit * xrange[0] / outputRate) / digit;
        }

        public void recalculateDataAndPlot()
        {
            double skip = 1;

            if (t_nSamples > maxSample && String.Compare(mode, "Y") == 0)
            {
                nSamples = (int)((xrange[1] - xrange[0]));
                if (nSamples > maxSample)
                    skip = nSamples / maxSample;

                nSamples = (int)(nSamples / skip);
                if (nSamples * skip + xrange[0] >= X[0].Length)
                {
                    nSamples = (int)Math.Floor((X[0].Length - xrange[0]) / skip);
                }



                Xpart = new double[nChannels][];
                Ypart = new double[nChannels][];

                for (int i = 0; i < nChannels; i++)
                    Xpart[i] = new double[nSamples];

                for (int i = 0; i < nChannels; i++)
                    Ypart[i] = new double[nSamples];


                for (int ch = 0; ch < nChannels; ch++)
                    for (int i = 0; i < nSamples; i++)
                    {
                        int pos = (int)(xrange[0] + skip * i);
                        if (pos < 0)
                            pos = 0;

                        if (pos < X[0].Length)
                        {
                            Xpart[ch][i] = X[ch][pos];
                            Ypart[ch][i] = Y[ch][pos];
                        }
                    }
            }
            else
            {
                Xpart = X;
                Ypart = Y;
            }

            for (int ch = 0; ch < MaxNChannels; ch++)
            {
                if (ch < nChannels)
                {
                    chart1.Series[ch].Points.DataBindXY(Xpart[ch], Ypart[ch]);
                    chart1.Series[ch].IsVisibleInLegend = true;
                }
                else
                {
                    chart1.Series[ch].IsVisibleInLegend = false;
                }
            }
        }

        public Rectangle getRectangle()
        {
            return new Rectangle(
                Math.Min(startPos.X, currentPos.X),
                Math.Min(startPos.Y, currentPos.Y),
                Math.Abs(startPos.X - currentPos.X),
                Math.Abs(startPos.Y - currentPos.Y));
        }

        public void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            startPos = e.Location;
            drawing = true;
        }

        public void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawing)
            {
                currentPos = e.Location;
                var rc = getRectangle();
                boxRoi = rc;
                //drawRectangle();
                chart1.Invalidate();
            }
        }


        public void chart1_MouseUp(object sender, MouseEventArgs e)
        {
            if (drawing)
            {
                currentPos = e.Location;
                drawing = false;
                var rc = getRectangle();
                if (rc.Width > 0 && rc.Height > 0 && rc.X > 0 && rc.Y > 0)
                {
                    boxRoi = rc;
                    try
                    {
                        var xValueMin = chart1.ChartAreas[0].AxisX.PixelPositionToValue(rc.X);
                        var xValueMax = chart1.ChartAreas[0].AxisX.PixelPositionToValue(rc.X + rc.Width);

                        var yValueMax = chart1.ChartAreas[0].AxisY.PixelPositionToValue(rc.Y);
                        var yValueMin = chart1.ChartAreas[0].AxisY.PixelPositionToValue(rc.Y + rc.Height);


                        double digitX = Math.Pow(10, Math.Floor(Math.Log10(xValueMax - xValueMin)) - 1);
                        xValueMax = Math.Ceiling(xValueMax / digitX) * digitX;
                        xValueMin = Math.Floor(xValueMin / digitX) * digitX;


                        double digitY = Math.Pow(10, Math.Floor(Math.Log10(yValueMax - yValueMin)) - 1);
                        yValueMax = Math.Ceiling(yValueMax / digitY) * digitY;
                        yValueMin = Math.Floor(yValueMin / digitY) * digitY;


                        if (xValueMax - xValueMin == 0)
                            xValueMax = xValueMin + 1.0 / digit;

                        if (yValueMax - yValueMin == 0)
                            yValueMax = yValueMin + 1.0 / digit;

                        chart1.ChartAreas[0].AxisX.Maximum = xValueMax;
                        chart1.ChartAreas[0].AxisX.Minimum = xValueMin;
                        chart1.ChartAreas[0].AxisY.Maximum = yValueMax;
                        chart1.ChartAreas[0].AxisY.Minimum = yValueMin;

                        xrange = new double[] { xValueMin * outputRate, xValueMax * outputRate };
                        //xrange = new double[] { xValueMin, xValueMax };

                        if (mode == "Y")
                            recalculateDataAndPlot();
                        //// yrange = new double[] { yValueMin, yValueMax };
                    }
                    catch (Exception Ex)
                    {
                        //Debug.WriteLine("Problem in drawing RC" + Ex.Message);
                    }

                }
                chart1.Invalidate();

            }
        }

        public void setPlotTitle(String str)
        {
            this.Text = str;
        }

        public void chart1_Paint(object sender, PaintEventArgs e)
        {
            if (drawing)
            {
                Pen rectPen = new Pen(Color.Red, (float)0.5);
                e.Graphics.DrawRectangle(rectPen, boxRoi);

            }
        }

        public void maximizePlot()
        {
            double[] maxXarray = new double[nChannels];
            double[] minXarray = new double[nChannels];
            double[] maxYarray = new double[nChannels];
            double[] minYarray = new double[nChannels];

            for (int ch = 0; ch < nChannels; ch++)
                maxXarray[ch] = Xpart[ch].Max();

            for (int ch = 0; ch < nChannels; ch++)
                minXarray[ch] = Xpart[ch].Min();

            for (int ch = 0; ch < nChannels; ch++)
                maxYarray[ch] = Ypart[ch].Max();

            for (int ch = 0; ch < nChannels; ch++)
                minYarray[ch] = Ypart[ch].Min();

            double xValueMax = maxXarray.Max();
            double xValueMin = minXarray.Min();
            double yValueMax = maxYarray.Max();
            double yValueMin = minYarray.Min();

            double digitX = Math.Pow(10, Math.Floor(Math.Log10(xValueMax - xValueMin)) - 1);
            xValueMax = Math.Ceiling(xValueMax / digitX) * digitX;
            xValueMin = Math.Floor(xValueMin / digitX) * digitX;


            double digitY = Math.Pow(10, Math.Floor(Math.Log10(yValueMax - yValueMin)) - 1);
            yValueMax = Math.Ceiling(yValueMax / digitY) * digitY;
            yValueMin = Math.Floor(yValueMin / digitY) * digitY;


            if (xValueMax - xValueMin == 0)
                xValueMax = xValueMin + 1.0 / digit;

            if (yValueMax - yValueMin == 0)
                yValueMax = yValueMin + 1.0 / digit;

            chart1.ChartAreas[0].AxisX.Maximum = xValueMax;
            chart1.ChartAreas[0].AxisX.Minimum = xValueMin;
            chart1.ChartAreas[0].AxisY.Maximum = yValueMax;
            chart1.ChartAreas[0].AxisY.Minimum = yValueMin;
        }

        public void chart1_DoubleClick(object sender, EventArgs e)
        {

            xrange = new double[] { X[0].Min() * outputRate, X[0].Max() * outputRate };
            recalculateDataAndPlot();
            maximizePlot();

            chart1.Invalidate();
        }
    }
}
