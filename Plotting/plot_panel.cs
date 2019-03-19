﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathLibrary;

namespace FLIMimage
{
    class plot_panel
    {
        int width;
        int height;
        public float yMergin = 0.13F; //Mergin from top.
        public float xMergin = 0.15F; //Mergin from left.
        public float xFrac = 0.78F;
        public float yFrac = 0.77F;

        public float xMergin_White = 0.02F;
        public float yMergin_White = 0;

        public bool autoScaleX = true;
        public bool autoScaleY = true;

        public int nTickRX = 5; //Rough number of ticks;
        public double XTickSpace = 0;
        public double XTickLength = 0.02; // Fraction;

        public int nTickRY = 5; //Rough number of ticks;
        public double YTickSpace = 0;
        public double YTickLength = 0.02; // Fraction;

        private double[] X, Y, Xmod, Ymod;
        public double Xmax, Xmin, Ymax, Ymin;

        public bool LabelX_10x = false;
        public bool LabelY_10x = false;

        //private List<double[]> XList = new List<double[]>();
        //private List<double[]> YList = new List<double[]>();

        //private List<double[]> XmodList = new List<double[]>();
        //private List<double[]> YmodList = new List<double[]>();

        public List<PlotData> dataList = new List<PlotData>();

        Point[] P;
        public Pen plotPen = new Pen(Brushes.Blue, 1);
        Pen linePen = new Pen(Brushes.Black, 1);
        Font tickFont = new Font("Times", 10);
        Font tickFontBold = new Font("Times", 10, FontStyle.Bold);
        Font tickFontLog = new Font("Times", 8);

        public int maxDigitTick = 2;
        double digitX;
        double digitY;

        public bool logScaleX = false;
        public bool logScaleY = false;
        public bool noAxis = false;
        public bool noTickLabelX = false;
        public bool noTickLabelY = false;
        public bool noTitleX = false;
        public bool noTitleY = false;
        public bool noPlot = false;
        public bool drawLegend = false;

        public double Log_ReplaceNegativewith_X = Double.NaN;
        public double Log_ReplaceNegativewith_Y = Double.NaN;

        public String XTitle = "XTitle";
        public String YTitle = "YTitle";

        public String plotType = "-"; //Matlab type
        public int plotSize = 6;

        List<Pen> penListColor = new List<Pen>();

        public PaintEventArgs e;

        //Drawing ROI function
        public bool drawingROI = false;
        public Point startPos, currentPos;
        public Rectangle boxRoi;


        public plot_panel(int Width, int Height)
        {
            width = Width;
            height = Height;
            //            width = (int)e.Graphics.VisibleClipBounds.Width;//e.ClipRectangle.Width;
            //            height = (int)e.Graphics.VisibleClipBounds.Height; //e.ClipRectangle.Height;
            createPenList();
            drawingROI = false;
        }

        public plot_panel(PaintEventArgs e1, int Width, int Height)
        {
            e = e1;
            width = Width;
            height = Height;
            //            width = (int)e.Graphics.VisibleClipBounds.Width;//e.ClipRectangle.Width;
            //            height = (int)e.Graphics.VisibleClipBounds.Height; //e.ClipRectangle.Height;
            dataList.Clear();
            createPenList();
            drawingROI = false;
        }

        public class PlotData
        {
            public int PlotSize = 6;
            public String PlotType = "-";
            public Pen PlotPen = new Pen(Brushes.Red, 1);
            public String Legend = "";
            public double[] x_data;
            public double[] y_data;
            public double[] x_dataMod;
            public double[] y_dataMod;
            public PlotData(double[] data_x, double[] data_y)
            {
                x_data = data_x;
                y_data = data_y;
            }

            PlotData(double[] data_y)
            {
                x_data = Enumerable.Range(0, data_y.Length).Select(x => (double)x).ToArray();
                y_data = data_y;
            }
        }

        private Rectangle getRectangle()
        {
            return new Rectangle(
                Math.Min(startPos.X, currentPos.X),
                Math.Min(startPos.Y, currentPos.Y),
                Math.Abs(startPos.X - currentPos.X),
                Math.Abs(startPos.Y - currentPos.Y));
        }

        public void StartDrawingROI(MouseEventArgs e)
        {
            startPos = e.Location;
            currentPos = e.Location;
            drawingROI = true;
        }

        public void Draw_DuringMoveMouse(MouseEventArgs e)
        {
            if (drawingROI)
            {
                currentPos = e.Location;
                boxRoi = getRectangle();
            }
        }

        public void Finish_DrawoingROI_MouseUp(MouseEventArgs e)
        {
            currentPos = e.Location;
            drawingROI = false;
            var rc = getRectangle();
            if (rc.Width > 0 && rc.Height > 0 && rc.X > 0 && rc.Y > 0)
            {
                boxRoi = rc;
                double[] XYmin = ToXYValues(rc.Location);
                double[] XYmax = ToXYValues(rc.Location + rc.Size);
                double xValueMax = XYmax[0];
                double yValueMax = XYmin[1];
                double xValueMin = XYmin[0];
                double yValueMin = XYmax[1];

                double digitX = Math.Pow(10, Math.Floor(Math.Log10(xValueMax - xValueMin)) - 1);
                xValueMax = Math.Ceiling(xValueMax / digitX) * digitX;
                xValueMin = Math.Floor(xValueMin / digitX) * digitX;


                double digitY = Math.Pow(10, Math.Floor(Math.Log10(yValueMax - yValueMin)) - 1);
                yValueMax = Math.Ceiling(yValueMax / digitY) * digitY;
                yValueMin = Math.Floor(yValueMin / digitY) * digitY;

                double digit = 1.0;

                if (xValueMax - xValueMin == 0)
                    xValueMax = xValueMin + 1.0 / digit;

                if (yValueMax - yValueMin == 0)
                    yValueMax = yValueMin + 1.0 / digit;

                SetXScale(xValueMax, xValueMin);
                SetYScale(yValueMax, yValueMin);

                AutoScaleNow(false);
            }
        }

        public void createPenList()
        {
            penListColor.Clear();
            penListColor.Add(new Pen(Brushes.Blue, 1));
            penListColor.Add(new Pen(Brushes.Green, 1));
            penListColor.Add(new Pen(Brushes.Cyan, 1));
            penListColor.Add(new Pen(Brushes.Magenta, 1));
            penListColor.Add(new Pen(Brushes.Red, 1));
            penListColor.Add(new Pen(Brushes.Orange, 1));
            penListColor.Add(new Pen(Brushes.DarkGreen, 1));
            penListColor.Add(new Pen(Brushes.Pink, 1));
            penListColor.Add(new Pen(Brushes.Lime, 1));
            penListColor.Add(new Pen(Brushes.Black, 1));
        }

        public void autoAxisPosition(PaintEventArgs e_args)
        {
            e = e_args;

            SizeF sizTitleX = new SizeF(0.0f, 0.0f);
            SizeF sizTitleY = new SizeF(0.0f, 0.0f);
            SizeF sizLabel = e.Graphics.MeasureString("0.00".ToString(), tickFont);
            SizeF sizeLabel10x = new SizeF(0.0f, 0.0f);

            if (!noTitleX)
                sizTitleX = e.Graphics.MeasureString(XTitle, tickFont);

            sizeLabel10x = e.Graphics.MeasureString("x10", tickFont);

            yMergin = 1.1f * (sizTitleX.Height + sizLabel.Height) / height + (float)XTickLength + yMergin_White;
            yFrac = 1.0f - yMergin - (sizeLabel10x.Height / height) * 2f;

            if (!noTitleY)
                sizTitleY = e.Graphics.MeasureString(YTitle, tickFont);

            xMergin = 1.1f * (sizTitleY.Height + sizLabel.Width) / width + (float)YTickLength + xMergin_White;
            xFrac = 1.0f - xMergin - (sizeLabel10x.Width / width) * 1.1f;
        }

        public void DrawLegendWithHighlight(PaintEventArgs eventArg, List<string> StrList,  int highlightID)
        {
            e = eventArg;

            List<Pen> penList = new List<Pen>();
            for (int i = 0; i < dataList.Count; i++)
                penList.Add(dataList[i].PlotPen);

            float posY = 0;
            for (int i = 0; i < dataList.Count; i++)
            {
                if (i >= StrList.Count)
                    break;

                String label = StrList[i];
                Pen pen;
                if (i == highlightID)
                    pen = new Pen(Color.Red, 1);
                else
                    pen = penList[i];

                SizeF siz = e.Graphics.MeasureString(label, tickFont);

                PointF P = new PointF((float)(width - siz.Width - width * xMergin_White), posY);

                if (i == highlightID)
                    e.Graphics.DrawString(label, tickFontBold, pen.Brush, P);
                else
                    e.Graphics.DrawString(label, tickFont, pen.Brush, P);

                PointF p1 = new PointF(P.X - (float)width * 0.02f, posY + siz.Height / 2);
                PointF p2 = new PointF(P.X - (float)width * 0.12f, posY + siz.Height / 2);
                e.Graphics.DrawLine(pen, p1, p2);

                posY = posY + siz.Height;
            }
        }

        public void DrawLegend(PaintEventArgs e_args, List<String> StrList)
        {            
            DrawLegendWithHighlight(e_args, StrList , - 1);
        }

        public void DrawLegend(PaintEventArgs e_args)
        {
            List<String> LegendStr = new List<string>();
            for (int i = 0; i < dataList.Count; i++)
                LegendStr.Add(dataList[i].Legend);
            DrawLegend(e_args, LegendStr);
        }

        public void clearData()
        {
            dataList.Clear();
            //drawingROI = false;

            AutoScaleNowX(autoScaleX);
            AutoScaleNowY(autoScaleY);

        }

        public void Apply_Legend(String str)
        {
            dataList[dataList.Count - 1].Legend = str;
            drawLegend = true;
        }

        public void addData(List<double> Ydata)
        {
            addData(Ydata.ToArray());
        }
        public void addData(double[] Ydata)
        {
            double[] X1 = Enumerable.Range(0, Ydata.Length).Select(x => (double)x).ToArray();
            addData(X1, Ydata.ToArray());
        }

        public void addData(List<double> x_data, List<double> y_data, String Plot_type)
        {
            addData(x_data.ToArray(), y_data.ToArray(), Plot_type);
        }

        public void addData(double[] X1, double[] Y1, Pen P, String Plot_type)
        {
            addData(X1, Y1);
            applyPlotType(Plot_type);
            applyPlotPen(P);
        }

        public void addData(double[] X1, double[] Y1, String Plot_type)
        {
            addData(X1, Y1);
            applyPlotType(Plot_type);
        }

        public void addData(double[] X1, double[] Y1)
        {
            X = X1;
            Y = Y1;
            PlotData data = new PlotData(X, Y);
            data.PlotPen = penListColor[dataList.Count % penListColor.Count];
            dataList.Add(data);            
        }

        public void addData(double[,] MultipleY, double outputRate)
        {
            double[] X0 = new double[MultipleY.GetLength(1)];
            double[][] Y1 = new double[MultipleY.GetLength(0)][];
            for (int j = 0; j < X0.Length; j++)
                X0[j] = j / outputRate;

            for (int i = 0; i < Y1.Length; i++)
            {
                Y1[i] = new double[MultipleY.GetLength(1)];
                for (int j = 0; j < Y1[i].Length; j++)
                    Y1[i][j] = MultipleY[i, j];

                addData(X0, Y1[i]);
            }
        }

        public void applyPlotType(String str)
        {
            dataList[dataList.Count - 1].PlotType = str;
        }

        public void applyPlotPen(Pen P)
        {
            dataList[dataList.Count - 1].PlotPen = P;
        }

        public void plot(PaintEventArgs event_args)
        {
            e = event_args;
            plot();
        }

        private void plot()
        {
            try
            {
                plot_core();
            }
            catch (Exception Ex)
            {
                Debug.WriteLine("Problem in plotting" + Ex.Message);
            }
            finally { }
        }

        public void AutoScaleNow(bool ON)
        {
            AutoScaleNowX(ON);
            AutoScaleNowY(ON);
        }

        public void AutoScaleNowX(bool ON)
        {
            autoScaleX = ON;
            if (ON)
                XTickSpace = 0;
        }

        public void AutoScaleNowY(bool ON)
        {
            autoScaleY = ON;
            if (ON)
                YTickSpace = 0;
        }


        public void SetYScale(double yValueMax, double yValueMin)
        {
            autoScaleX = false;
            autoScaleY = false;
            Ymax = yValueMax;
            Ymin = yValueMin;
            YTickSpace = 0;
        }

        public void SetXScale(double xValueMax, double xValueMin)
        {
            autoScaleX = false;
            autoScaleY = false;
            Xmax = xValueMax;
            Xmin = xValueMin;
            XTickSpace = 0;
        }

        public void plot_core()
        {
            double Xmax1, Xmin1, Ymax1, Ymin1;

            Xmax1 = double.NegativeInfinity;
            Xmin1 = double.PositiveInfinity;
            Ymax1 = double.NegativeInfinity;
            Ymin1 = double.PositiveInfinity;

            for (int i = 0; i < dataList.Count; i++)
            {
                X = dataList[i].x_data;
                Y = dataList[i].y_data;

                getMaxMin(); //Does not change Xmax or Xmin if not autoscale. Produces Xmod and Ymod

                dataList[i].x_dataMod = Xmod;
                dataList[i].y_dataMod = Ymod;

                if (Xmod.Length > 0)
                {
                    Xmax1 = Math.Max(Xmax, Xmax1);
                    Xmin1 = Math.Min(Xmin, Xmin1);
                    Ymax1 = Math.Max(Ymax, Ymax1);
                    Ymin1 = Math.Min(Ymin, Ymin1);
                }
            }

            if (Double.IsInfinity(Xmax1) || Double.IsInfinity(Xmin1))
            {
                Xmax1 = 1; //To avoid problem.
                Xmin1 = -1;
            }
            if (Double.IsInfinity(Ymax1) || Double.IsInfinity(Ymin1))
            {
                Ymax1 = 1;
                Ymin1 = -1;
            }


            if (autoScaleX)
            {
                Xmax = Xmax1;
                Xmin = Xmin1;
            }

            if (autoScaleY)
            {
                Ymax = Ymax1;
                Ymin = Ymin1;
            }

            CalcMaxMinTickX();
            CalcMaxMinTickY();

            if (!noAxis)
            {
                DrawAxes(); //requires Xmax and Xmin.
                DrawTickX();
                DrawTickY();
            }

            if (!noPlot) 
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    Xmod = dataList[i].x_dataMod;
                    Ymod = dataList[i].y_dataMod;

                    //removeNAN();

                    if (Xmod.Length > 0)
                    {
                        calcCoordinate();
                        plotType = dataList[i].PlotType;
                        plotSize = dataList[i].PlotSize;
                        plotPen = dataList[i].PlotPen;
                        drawPlot();
                    }
                }
            }

            if (drawLegend)
                DrawLegend(e);

            drawTitles();
        }

        public void drawTitles()
        {
            SizeF sz, siz;
            PointF P;
            sz = e.Graphics.VisibleClipBounds.Size;

            if (!noTitleX)
            {
                siz = e.Graphics.MeasureString(XTitle, tickFont);
                //P = new PointF((float)ToCoordinateX((Xmax - Xmin) / 2) - siz.Width / 2, height - (float)(siz.Height * 1.2));
                P = new PointF((float)width * (xFrac / 2 + xMergin) - siz.Width / 2, height - (float)(siz.Height * 1.2) - yMergin_White);
                e.Graphics.DrawString(XTitle, tickFont, Brushes.Black, P);
            }
            if (!noTitleY)
            {
                e.Graphics.TranslateTransform(0, sz.Height);
                e.Graphics.RotateTransform(270);
                siz = e.Graphics.MeasureString(YTitle, tickFont);
                P = new PointF((float)(height * (yFrac / 2 + yMergin)) - siz.Width / 2.0F, xMergin_White);
                e.Graphics.DrawString(YTitle, tickFont, Brushes.Black, P);
                e.Graphics.ResetTransform();
            }
        }

        public void getMaxMin()
        {
            Xmod = new double[X.Length];
            if (logScaleX)
                for (int i = 0; i < X.Length; i++)
                {
                    if (!Double.IsNaN(Log_ReplaceNegativewith_X) && X[i] <= 0 && Log_ReplaceNegativewith_X > 0)
                        Xmod[i] = Math.Log10(Log_ReplaceNegativewith_X);
                    else
                        Xmod[i] = Math.Log10(X[i]);
                }
            else
                Xmod = (double[])X.Clone();

            Ymod = new double[Y.Length];
            if (logScaleY)
                for (int i = 0; i < Y.Length; i++)
                {
                    if (!Double.IsNaN(Log_ReplaceNegativewith_Y) && Y[i] <= 0 && Log_ReplaceNegativewith_Y > 0)
                        Ymod[i] = Math.Log10(Log_ReplaceNegativewith_Y);
                    else
                        Ymod[i] = Math.Log10(Y[i]);
                }
            else
                Ymod = (double[])Y.Clone();

            removeNAN();

            if (Xmod.Length == 0)
                return;

            if (autoScaleX)
            {
                Xmax = Xmod.Max();
                Xmin = Xmod.Min();
                //calcMaxMinTickX();
            }
            if (autoScaleY)
            {
                Ymax = Ymod.Max();
                Ymin = Ymod.Min();
                //calcMaxMinTickY();
            }
        }

        void DrawTickX()
        {
            double XTick = Xmin;
            bool dividedLabel = maxDigitTick < Math.Abs(Math.Log10(digitX)) && !logScaleX;
            LabelX_10x = dividedLabel;

            while (XTick <= Xmax)
            {
                Point P1 = new Point(ToCoordinateX(XTick), ToCoordinateY(Ymin));
                Point P2 = new Point(ToCoordinateX(XTick), ToCoordinateY(Ymin) + (int)(height * XTickLength));
                e.Graphics.DrawLine(linePen, P1, P2);

                if (!noTickLabelX)
                    if (!logScaleX)
                    {
                        String label;
                        if (!dividedLabel)
                        {
                            label = String.Format("{0}", XTick);
                        }
                        else
                            label = (XTick / digitX).ToString();

                        SizeF siz = e.Graphics.MeasureString(label, tickFont);
                        PointF P = new PointF((float)(P1.X - siz.Width / 2), (float)(P2.Y + siz.Height / 6));
                        e.Graphics.DrawString(label, tickFont, Brushes.Black, P);
                    }
                    else
                    {
                        String label = "10";
                        String labelLog = XTick.ToString();
                        SizeF siz = e.Graphics.MeasureString(label, tickFont);
                        SizeF sizLog = e.Graphics.MeasureString(labelLog, tickFontLog);

                        PointF P = new PointF((float)(P1.X - siz.Width / 2 - sizLog.Height / 4), (float)(P2.Y + siz.Height / 4));
                        e.Graphics.DrawString(label, tickFont, Brushes.Black, P);

                        PointF PLog = new PointF(P.X + siz.Width - sizLog.Width / 4, P.Y - sizLog.Height / 2);
                        e.Graphics.DrawString(labelLog, tickFontLog, Brushes.Black, PLog);
                    }
                XTick += XTickSpace;
            }
            if (dividedLabel && !noTickLabelX)
            {
                Point P2 = new Point(ToCoordinateX(Xmax), ToCoordinateY(Ymin) + (int)(height * XTickLength));
                String label = "×10";
                String labelLog = Math.Log10(digitX).ToString();
                SizeF siz = e.Graphics.MeasureString(label, tickFont);
                SizeF sizLog = e.Graphics.MeasureString(labelLog, tickFontLog);

                PointF P = new PointF((float)(P2.X - siz.Width / 2 - sizLog.Width), (float)(P2.Y + siz.Height * 1.5));
                e.Graphics.DrawString(label, tickFont, Brushes.Black, P);
                PointF PLog = new PointF(P.X + siz.Width - sizLog.Width / 4, P.Y - sizLog.Height / 2);
                e.Graphics.DrawString(labelLog, tickFontLog, Brushes.Black, PLog);
            }
        }

        void DrawTickY()
        {
            double YTick = Ymin;
            bool dividedLabel = maxDigitTick < Math.Abs(Math.Log10(digitY)) && !logScaleY;
            LabelY_10x = dividedLabel;

            while (YTick <= Ymax)
            {
                Point P1 = new Point(ToCoordinateX(Xmin), ToCoordinateY(YTick));
                Point P2 = new Point(ToCoordinateX(Xmin) - (int)(width * YTickLength), ToCoordinateY(YTick));
                e.Graphics.DrawLine(linePen, P1, P2);

                if (!noTickLabelY)
                    if (!logScaleY)
                    {
                        String label;
                        if (!dividedLabel)
                            label = YTick.ToString();
                        else
                            label = (YTick / digitY).ToString();

                        SizeF siz = e.Graphics.MeasureString(label, tickFont);
                        PointF P = new PointF((float)(P2.X - siz.Width), (float)(P2.Y - siz.Height / 2));
                        e.Graphics.DrawString(label, tickFont, Brushes.Black, P);
                    }
                    else
                    {
                        if (YTick == Math.Log10(Log_ReplaceNegativewith_Y))
                        {
                            String label = "0";
                            String labelLog = String.Format("{0}", YTick);
                            SizeF siz = e.Graphics.MeasureString(label, tickFont);
                            PointF P = new PointF((float)(P2.X - siz.Width), (float)(P2.Y - siz.Height / 2));
                            e.Graphics.DrawString(label, tickFont, Brushes.Black, P);
                        }
                        else
                        {
                            String label = "10";
                            String labelLog = YTick.ToString();
                            SizeF siz = e.Graphics.MeasureString(label, tickFont);
                            SizeF sizLog = e.Graphics.MeasureString(labelLog, tickFontLog);

                            PointF P = new PointF((float)(P2.X - siz.Width - sizLog.Width), (float)(P2.Y - siz.Height / 2));
                            e.Graphics.DrawString(label, tickFont, Brushes.Black, P);
                            PointF PLog = new PointF(P.X + siz.Width - sizLog.Width / 4, P.Y - sizLog.Height / 2);
                            e.Graphics.DrawString(labelLog, tickFontLog, Brushes.Black, PLog);
                        }

                    }
                YTick += YTickSpace;
            }
            if (dividedLabel && !noTickLabelY)
            {
                Point P2 = new Point(ToCoordinateX(Xmin) - (int)(width * YTickLength), ToCoordinateY(Ymax));
                String label = "× 10";
                String labelLog = Math.Log10(digitY).ToString();
                SizeF siz = e.Graphics.MeasureString(label, tickFont);
                SizeF sizLog = e.Graphics.MeasureString(labelLog, tickFontLog);

                PointF P = new PointF((float)(P2.X - siz.Width / 2 - sizLog.Width), (float)(P2.Y - siz.Height * 1.5));
                e.Graphics.DrawString(label, tickFont, Brushes.Black, P);
                PointF PLog = new PointF(P.X + siz.Width - sizLog.Width / 4, P.Y - sizLog.Height / 2);
                e.Graphics.DrawString(labelLog, tickFontLog, Brushes.Black, PLog);
            }
        }

        void CalcMaxMinTickY()
        {
            calcMaxMinSpace(ref Ymax, ref Ymin, ref YTickSpace, ref digitY, ref nTickRY, logScaleY, autoScaleY);
        }

        void CalcMaxMinTickX() //Very stupid copy&Paste!!
        {
            calcMaxMinSpace(ref Xmax, ref Xmin, ref XTickSpace, ref digitX, ref nTickRX, logScaleX, autoScaleX);
        }

        void calcMaxMinSpace(ref double ValMax, ref double ValMin, ref double ValTickSpace, ref double ValDigit, ref int nTickR, bool LogScale, bool AutoScale)
        {
            if (ValMax == ValMin)
            {
                ValMax = ValMax + 1;
                //Xmin = Xmin - 1;
                ValTickSpace = 1;
                ValDigit = 1;
            }
            else
            {
                if (ValTickSpace == 0)
                {
                    double digit = (ValMax - ValMin) / nTickR;
                    double digit2 = Math.Pow(10, Math.Round(Math.Log10(digit)));
                    ValTickSpace = digit2 * Math.Ceiling(digit / digit2);
                    if (LogScale && ValTickSpace < 1)
                        ValTickSpace = 1;
                    ValDigit = digit2;
                }
                else
                {
                    ValDigit = ValTickSpace;
                    //digitX = Math.Ceiling(Math.Pow(10, Math.Round(Math.Log10(XTickSpace))));
                }
            }

            if (AutoScale)
            {
                if (!LogScale)
                {
                    ValMax = ValTickSpace * Math.Ceiling(ValMax / ValTickSpace);  //Math.Ceiling(Xmax);
                    ValMin = ValTickSpace * Math.Floor(ValMin / ValTickSpace);
                }
                else
                {
                    ValMin = Math.Floor(ValMin);
                    if (ValMax - ValMin < 1)
                        ValMax = ValMin + 1;
                }
            }

        }

        void removeNAN()
        {
            int k = 0;
            int len = Math.Min(Xmod.Length, Ymod.Length);
            double[] X1 = new double[len];
            double[] Y1 = new double[len];
            for (int j = 0; j < len; j++)
            {
                if (!Double.IsNaN(Xmod[j]) && !Double.IsNaN(Ymod[j]) && !Double.IsInfinity(Xmod[j]) && !Double.IsInfinity(Ymod[j]))
                {
                    X1[k] = Xmod[j];
                    Y1[k] = Ymod[j];
                    k++;
                }
            }
            Xmod = new double[k];
            Ymod = new double[k];
            for (int i = 0; i < Xmod.Length; i++)
            {
                Xmod[i] = X1[i];
                Ymod[i] = Y1[i];
            }

        }

        void drawPlot()
        {
            if (plotType.Contains("o") || P.Length == 1)
                for (int i = 0; i < P.Length; i++)
                {
                    Point P1 = P[i];
                    P1 = new Point((int)(P1.X - plotSize / 2), (int)(P1.Y - plotSize / 2));
                    Size S1 = new Size(plotSize, plotSize);
                    e.Graphics.FillEllipse(plotPen.Brush, new Rectangle(P1, S1));
                }

            if (plotType.Contains("-") && P.Length > 1)
                e.Graphics.DrawLines(plotPen, P);
        }

        void DrawAxes()
        {
            Point P1 = new Point(ToCoordinateX(Xmin), ToCoordinateY(Ymin));
            Point P2 = new Point(ToCoordinateX(Xmax), ToCoordinateY(Ymin));
            Point P3 = new Point(ToCoordinateX(Xmin), ToCoordinateY(Ymin));
            Point P4 = new Point(ToCoordinateX(Xmin), ToCoordinateY(Ymax));
            e.Graphics.DrawLine(linePen, P1, P2);
            e.Graphics.DrawLine(linePen, P3, P4);
        }

        void calcCoordinate()
        {
            int len = Math.Min(Xmod.Length, Ymod.Length);
            P = new Point[len];
            for (int j = 0; j < len; j++)
            {
                int xa = ToCoordinateX(Xmod[j]);
                int ya = ToCoordinateY(Ymod[j]);

                if (xa > width * (xMergin + xFrac))
                    xa = (int)(width * (xMergin + xFrac));
                if (xa < width * xMergin)
                    xa = (int)(width * xMergin);
                if (ya < height * (1 - yMergin - yFrac))
                    ya = (int)(height * (1 - yMergin - yFrac));
                if (ya > height * (1 - yMergin))
                    ya = (int)(height * (1 - yMergin));

                P[j] = new Point(xa, ya);
            }
        }

        public double[] ToXYValues(Point P)
        {
            double[] result = new double[2];
            double xa = (double)P.X;
            double ya = (double)P.Y;
            result[0] = (xa / (double)width - xMergin) / (double)xFrac * (Xmax - Xmin) + Xmin;

            double y1 = -ya / (double)height + 1 - yMergin;
            result[1] = y1 / yFrac * (Ymax - Ymin) + Ymin;
            return result;
        }

        int ToCoordinateX(double x)
        {
            int xa = (int)(width * (xMergin + xFrac * (x - Xmin) / (Xmax - Xmin)));
            return xa;
        }

        int ToCoordinateY(double y)
        {
            double y1 = yFrac * (y - Ymin) / (Ymax - Ymin);
            int ya = (int)(height * (1 - yMergin - y1));
            return ya;
        }
    }
}
