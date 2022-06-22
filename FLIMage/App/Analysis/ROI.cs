using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace FLIMage.Analysis
{
    public class ROI_FLIM_Parameters
    {
        public int nChannels = 2;
        public int[] n_timePoints = { 64, 50 };
        public int n_beta = 6;
        public double[][] LifetimeX;
        public double[][] LifetimeY;
        public double[][] beta;
        public double[][] beta0;
        public double[][] fitCurve;
        public double[][] residual;
        public double[] offset_fit;
        //public double[] intensity;
        public double[] nPixels;
        public double[] meanIntensity;
        public double[] sumIntensity;
        public double[] tau_m;
        public double[] tau_m_fromMAP;
        public double[] xi_square;
        public int roiID = -1;

        public ROI_FLIM_Parameters()
        {
            initializeParams(nChannels, n_timePoints);
        }

        public void AssureSizeAllParameters()
        {
            var fields = this.GetType().GetFields();
            foreach (var f1 in fields)
            {
                Type f = f1.FieldType;
                if (f1.FieldType == typeof(double[][]))
                {
                    var value = (double[][])f1.GetValue(this);
                    if (value == null)
                    {
                        value = new double[nChannels][];
                        f1.SetValue(this, value);
                    }
                    else if (value.Length != nChannels)
                    {
                        Array.Resize(ref value, nChannels);
                        f1.SetValue(this, value);
                    }
                }
                else if (f1.FieldType == typeof(double[]))
                {
                    var value = (double[])f1.GetValue(this);
                    if (value == null)
                    {
                        value = new double[nChannels];
                        f1.SetValue(this, value);
                    }
                    else if (value.Length != nChannels)
                    {
                        Array.Resize(ref value, nChannels);
                        f1.SetValue(this, value);
                    }
                }
            }

            for (int ch = 0; ch < nChannels; ch++)
            {
                if (beta0[ch] == null)
                {
                    beta0[ch] = new double[] { 100, 0.2 / 2.6, 100, 0.2 / 1.1, 10 / 0.2, 0.3 / 0.2 };
                }
                if (beta[ch] == null)
                {
                    beta[ch] = new double[] { 100, 0.2 / 2.6, 100, 0.2 / 1.1, 10 / 0.2, 0.3 / 0.2 };
                }
            }
        }

        public ROI_FLIM_Parameters Copy()
        {
            AssureSizeAllParameters();

            var param = new ROI_FLIM_Parameters();
            var fields = this.GetType().GetFields();
            foreach (var f1 in fields)
            {
                if (f1.FieldType == typeof(double[][]))
                {
                    var value = (double[][])f1.GetValue(this);
                    if (value != null)
                    {
                        var new_value = new double[value.Length][];
                        for (int ch = 0; ch < value.Length; ch++)
                        {
                            if (value[ch] != null)
                                new_value[ch] = (double[])value[ch].Clone();
                        }
                        f1.SetValue(param, new_value);
                    }
                    else
                        f1.SetValue(param, null);
                }
                else if (f1.FieldType == typeof(double[]))
                {
                    var value = (double[])f1.GetValue(this);
                    f1.SetValue(param, value.Clone());
                }
                else //Scholar
                {
                    var value = f1.GetValue(this);
                    f1.SetValue(param, value);
                }
            }
            return param;
        }


        public void initializeParams(int numChannels, int[] n_points)
        {
            nChannels = numChannels;
            n_timePoints = (int[])n_points.Clone();
            AssureSizeAllParameters();
        }

        public void ChangeChannelNumber(int finalChannels)
        {

            if (finalChannels != nChannels)
            {
                Array.Resize(ref tau_m, finalChannels);
                Array.Resize(ref tau_m_fromMAP, finalChannels);
                Array.Resize(ref meanIntensity, finalChannels);
                Array.Resize(ref sumIntensity, finalChannels);
                Array.Resize(ref nPixels, finalChannels);
                Array.Resize(ref xi_square, finalChannels);
                Array.Resize(ref LifetimeX, finalChannels);
                Array.Resize(ref LifetimeY, finalChannels);
                Array.Resize(ref beta, finalChannels);
                nChannels = finalChannels;
                for (int ch = 0; ch < nChannels; ch++)
                {
                    if (LifetimeX[ch] == null)
                    {
                        LifetimeX[ch] = new double[n_timePoints[ch]];
                        LifetimeY[ch] = new double[n_timePoints[ch]];
                        beta[ch] = new double[6];
                    }
                }


            }
        }
    }

    public class ROI
    {
        public ROItype ROI_type = ROItype.Rectangle;
        public float[] X = new float[1];
        public float[] Y = new float[1];
        public PointF[] Points;

        public PointF[] SmoothCurvePoints;
        public float polyLineROI_Radius = 8;
        public PointF[] EquiDistanceCircleCoord;

        public RectangleF Rect = new RectangleF(0, 0, 0, 0);

        //Curve and Fitting stored in ROI.
        public ROI_FLIM_Parameters flim_parameters = new ROI_FLIM_Parameters();
        public List<ROI> polyLineROIs = new List<ROI>();

        public ROI_FLIM_Parameters[] flim_parameters_Pages;

        public int ID = 1;


        //For 3DROI. Not implemented yer.
        public int[] Z = new int[1]; //Range from Z[0] to Z[1].
        public bool Roi3d = false;
        //public double[] offset_fit;

        public ROI()
        {
        }

        public ROI(ROI roi)
        {
            ROI_type = roi.ROI_type;
            X = (float[])roi.X.Clone();
            Y = (float[])roi.Y.Clone();
            Rect = new RectangleF(X.Min(), Y.Min(), (X.Max() - X.Min() + 1), (Y.Max() - Y.Min() + 1));
            producePointArray();

            flim_parameters = roi.flim_parameters.Copy();


            polyLineROI_Radius = roi.polyLineROI_Radius;
            ID = roi.ID;

            Z = (int[])roi.Z.Clone();
            Roi3d = roi.Roi3d;

            if (ROI_type == ROItype.PolyLine)
            {
                GetSmoothCurve();
                GetEqualDistanceCenters(roi.polyLineROI_Radius);
            }
        }

        public ROI(ROItype R, float[] x, float[] y, int n_channels, float polyLineRadius, int id, bool roi3d, int[] z) //Constructor
        {
            polyLineROI_Radius = polyLineRadius;
            ROI_type = R;

            X = (float[])x.Clone();
            Y = (float[])y.Clone();

            Rect = new RectangleF(x.Min(), y.Min(), (x.Max() - x.Min()), (y.Max() - y.Min()));
            producePointArray();

            flim_parameters.initializeParams(n_channels, flim_parameters.n_timePoints);
            ID = id;
            flim_parameters.roiID = id;

            Roi3d = roi3d;

            if (z != null)
                Z = (int[])z.Clone();
            else
                Z = null;

            if (ROI_type == ROItype.PolyLine)
            {
                GetSmoothCurve();
                GetEqualDistanceCenters(polyLineRadius);
            }
        }

        public ROI(ROItype R, RectangleF Rect1, int n_channels, int id, bool roi3d, int[] z) //Constructor 2
        {
            ROI_type = R;
            X = new float[] { Rect1.Left, Rect1.Right - 1, Rect1.Right - 1, Rect1.Left };
            Y = new float[] { Rect1.Top, Rect1.Top, Rect1.Bottom - 1, Rect1.Bottom - 1 };
            producePointArray();
            Rect = new RectangleF(Rect1.Location, Rect1.Size);
            ID = id;
            flim_parameters.roiID = id;
            flim_parameters.initializeParams(n_channels, flim_parameters.n_timePoints);

            Roi3d = roi3d;
            Z = (int[])z.Clone();
        }

        public ROI CopyROI(int new_id)
        {
            var R1 = new ROI(this);
            R1.ID = new_id;
            return R1;
        }

        /// <summary>
        /// Count only Zstart <= Z < Zend. Zend is exclusive.
        /// </summary>
        /// <param name="Zstart"></param>
        /// <param name="Zend"></param>
        public void Set_Roi3d_Z(int Zstart, int Zend)
        {
            Roi3d = true;
            Z[0] = Zstart;
            Z[1] = Zend;
        }


        public void importROIfromIJ(byte[] IJByteArray)
        {
            MemoryStream stream = new MemoryStream(IJByteArray);
        }



        public void shiftRoi(PointF P)
        {
            if (Rect != null)
            {
                Rect.Location = new PointF(Rect.Location.X + P.X, Rect.Location.Y + P.Y);
                for (int i = 0; i < X.Length; i++)
                {
                    X[i] = X[i] + P.X;
                    Y[i] = Y[i] + P.Y;
                    Points[i] = new PointF(X[i], Y[i]);
                }
            }
        }

        public void initialize_flimParameter_Pages(int nChannels, int[] ndTimes, int nPages)
        {
            if (flim_parameters_Pages == null)
                flim_parameters_Pages = new ROI_FLIM_Parameters[nPages];
            else
                Array.Resize(ref flim_parameters_Pages, nPages);

            for (int i = 0; i < flim_parameters_Pages.Length; i++)
            {
                if (flim_parameters_Pages[i] == null)
                {
                    flim_parameters_Pages[i] = new ROI_FLIM_Parameters();
                    flim_parameters_Pages[i].initializeParams(nChannels, ndTimes);
                }
                else
                {
                    flim_parameters_Pages[i].AssureSizeAllParameters();
                }
            }
        }

        public void transfer_FittingParameterFromPage(int page)
        {
            if (flim_parameters_Pages != null && page < flim_parameters_Pages.Length && page >= 0)
                flim_parameters = flim_parameters_Pages[page].Copy();
        }

        public void shiftRoiCorner(Point P, int whichConer)
        {
            if (Rect != null)
            {
                Rect.Location = new PointF(Rect.Location.X + P.X, Rect.Location.Y + P.Y);
                for (int i = 0; i < X.Length; i++)
                {
                    X[i] = X[i] + P.X;
                    Y[i] = Y[i] + P.Y;
                    Points[i] = new PointF(X[i], Y[i]);
                }
            }
        }

        public ROI changeSizeOfCircle(PointF afterP, PointF beforeP, ROI roiBefore)
        {
            var centerX = (roiBefore.Rect.Left + roiBefore.Rect.Right) / 2;
            var centerY = (roiBefore.Rect.Top + roiBefore.Rect.Bottom) / 2;

            double distFromCenterB2 = Math.Pow(centerX - beforeP.X, 2) + Math.Pow(centerY - beforeP.Y, 2);
            double distFromCenterA2 = Math.Pow(centerX - afterP.X, 2) + Math.Pow(centerY - afterP.Y, 2);
            double scale = Math.Sqrt(distFromCenterA2 / distFromCenterB2);
            var newWidth = (float)(roiBefore.Rect.Width * scale);
            var newHeight = (float)(roiBefore.Rect.Height * scale);
            var newLeft = centerX - newWidth / 2;
            var newTop = centerY - newHeight / 2;

            int n_channels = flim_parameters.nChannels;
            var newRoi = new ROI(ROI_type, new RectangleF(newLeft, newTop, newWidth, newHeight), n_channels, ID, Roi3d, (int[])Z.Clone());
            return newRoi;
        }

        public ROI MovePoint(PointF P, int loc)
        {
            float[] X1, Y1;
            X1 = (float[])X.Clone();
            Y1 = (float[])Y.Clone();
            X1[loc] = P.X;
            Y1[loc] = P.Y;

            if (ROI_type == ROItype.Polygon)
            {
                if (loc == 0)
                {
                    X1[X1.Length - 1] = P.X;
                    Y1[Y1.Length - 1] = P.Y;
                }
                else if (loc == X1.Length - 1)
                {
                    X1[0] = P.X;
                    Y1[0] = P.Y;
                }
            }
            int n_channels = flim_parameters.nChannels;
            var newRoi = new ROI(ROI_type, X1, Y1, n_channels, polyLineROI_Radius, ID, Roi3d, (int[])Z.Clone());
            return newRoi;
        }


        public ROI addPoints(PointF P)
        {
            float[] X1, Y1;
            if (X == null)
            {
                X1 = new float[1];
                Y1 = new float[1];
                //Points = new Point[1];
                X1[0] = P.X;
                Y1[0] = P.Y;
            }
            else
            {
                int n = X.Length;
                X1 = new float[n + 1];
                Y1 = new float[n + 1];
                //Points = new Point[n + 1];
                for (int i = 0; i < n; i++)
                {
                    X1[i] = X[i];
                    Y1[i] = Y[i];
                }
                X1[n] = P.X;
                Y1[n] = P.Y;
            }
            int n_channels = flim_parameters.nChannels;

            var newRoi = new ROI(ROI_type, X1, Y1, n_channels, polyLineROI_Radius, ID, Roi3d, (int[])Z.Clone());

            return newRoi;
        }

        /// <summary>
        /// Apply offset and then scale the ROI. Offset is offset before scale.
        /// It will create a new roi.
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public ROI ScaleRoi(double scale)
        {
            int n_channels = flim_parameters.nChannels;

            float[] X1 = X.Select(x => (float)(x * scale)).ToArray();
            float[] Y1 = Y.Select(x => (float)(x * scale)).ToArray();
            //PointF[] P1 = Points.Select(p => new PointF((float)(p.X * scale), (float)(p.Y * scale))).ToArray();

            float polyline_radius_afterScale = polyLineROI_Radius * (float)scale;

            ROI roi = new ROI(ROI_type, X1, Y1, n_channels, polyline_radius_afterScale, ID, Roi3d, (int[])Z.Clone());
            roi.flim_parameters = flim_parameters.Copy();

            return roi;
        }

        public bool IsInsideRoi(PointF P)
        {
            if (Points == null)
                return false;

            bool c = false;
            if (ROI_type.Equals(ROItype.Polygon))
                c = IsInsideROI_Polygon(P);
            else if (ROI_type.Equals(ROItype.Rectangle))
                c = isInsideROI_Rect(P);
            else if (ROI_type.Equals(ROItype.Elipsoid))
                c = isInsideROI_Elipse(P);
            else if (ROI_type.Equals(ROItype.PolyLine))
                c = isInsideROI_PolyLine(P, out int circleID);
            return c;
        }


        public bool isEditable()
        {
            return ROI_type.Equals(ROItype.Elipsoid) || ROI_type.Equals(ROItype.Rectangle) || ROI_type.Equals(ROItype.Polygon)
                || ROI_type.Equals(ROItype.PolyLine);
        }

        public bool isNearCornerOfRoi(PointF P, float threshhold_distance, out int loc, out float actual_distance)
        {
            bool c = false;
            loc = 0;

            if (Points == null)
            {
                actual_distance = float.MaxValue;
                return false;
            }

            if (ROI_type.Equals(ROItype.Elipsoid))
            {
                double a = (double)Rect.Width / 2.0;
                double b = (double)Rect.Height / 2.0;
                double CenterX = Rect.Left + a;
                double CenterY = Rect.Top + b;
                double Dist_sq = Math.Pow((P.X - CenterX) / a, 2) + Math.Pow((P.Y - CenterY) / b, 2);

                actual_distance = (float)(Math.Max(a, b) * Math.Abs(Dist_sq - 1));

                if (actual_distance < threshhold_distance)
                    c = true;

                return c;
            }
            else if (ROI_type.Equals(ROItype.Rectangle))
            {
                if ((P.X <= Rect.Right) && (P.X >= Rect.Right - threshhold_distance) &&
                    (P.Y <= Rect.Bottom) && (P.Y >= Rect.Bottom - threshhold_distance))
                    c = true;
                loc = 2;
                actual_distance = (P.X - Rect.Right) * (P.X - Rect.Right) + (P.Y - Rect.Bottom) * (P.Y - Rect.Bottom);
                actual_distance = (float)Math.Sqrt(actual_distance);
                return c;
            }
            else
            {
                var th_dist = threshhold_distance / 2.0f;
                actual_distance = float.MaxValue; //min distance.
                for (int i = 0; i < X.Length; i++)
                {
                    float x = X[i];
                    float y = Y[i];

                    //minimum distance.
                    float dist1 = (P.X - x) * (P.X - x) + (P.Y - y) * (P.Y - y);
                    dist1 = (float)Math.Sqrt(dist1);
                    if (dist1 < actual_distance)
                    {
                        actual_distance = dist1;
                        if (actual_distance < th_dist)
                            c = true;

                        loc = i;
                    }
                }


                return c;
            }
        }

        public bool isNearSmoothCurve(Point P, float distance)
        {
            bool c = false;
            int loc = 0;
            if (ROI_type.Equals(ROItype.PolyLine))
            {
                if (SmoothCurvePoints == null)
                    return false;

                for (int i = 0; i < SmoothCurvePoints.Length; i++)
                {
                    float x = SmoothCurvePoints[i].X;
                    float y = SmoothCurvePoints[i].Y;
                    if (P.X <= x + distance && P.X >= x - distance && P.Y <= y + distance && P.Y >= y - distance)
                    {
                        c = true;
                        loc = i;
                        return c;
                    }
                }
            }
            return c;
        }

        public PointF[] GetEqualDistanceCenters(float distance)
        {
            polyLineROI_Radius = distance;

            if (ROI_type != ROItype.PolyLine)
                return null;

            if (SmoothCurvePoints == null)
                GetSmoothCurve();

            if (SmoothCurvePoints == null)
                return null;

            PointF[] curvePoints = SmoothCurvePoints;
            List<PointF> equiDistP = new List<PointF>();


            double cumD = 0;
            double preCumD = 0;
            equiDistP.Add(curvePoints[0]);

            for (int k = 0; k < curvePoints.Length - 1; k++)
            {
                double dx = curvePoints[k].X - curvePoints[k + 1].X;
                double dy = curvePoints[k].Y - curvePoints[k + 1].Y;
                double d = Math.Sqrt(dx * dx + dy * dy);
                cumD += d;
                if (cumD >= distance)
                {
                    if (cumD == distance)
                        equiDistP.Add(curvePoints[k]);
                    else
                    {
                        double ratio = (distance - preCumD) / (cumD - preCumD);
                        double x1 = curvePoints[k + 1].X * ratio + curvePoints[k].X * (1 - ratio);
                        double y1 = curvePoints[k + 1].Y * ratio + curvePoints[k].Y * (1 - ratio);
                        equiDistP.Add(new PointF((float)x1, (float)y1));
                    }
                    cumD = cumD - distance;
                }
                preCumD = cumD;
            }

            EquiDistanceCircleCoord = equiDistP.ToArray();
            float[] Xcoor = EquiDistanceCircleCoord.Select(p => p.X).ToArray();
            float[] Ycoor = EquiDistanceCircleCoord.Select(p => p.Y).ToArray();
            float xmin = Math.Max(0, Xcoor.Min() - polyLineROI_Radius);
            float xmax = Xcoor.Max() + polyLineROI_Radius;
            float ymin = Math.Max(0, Ycoor.Min() - polyLineROI_Radius);
            float ymax = Ycoor.Max() + polyLineROI_Radius;
            Rect = new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);

            polyLineROIs.Clear();
            for (int i = 0; i < EquiDistanceCircleCoord.Length; i++)
            {
                var Rect1 = new RectangleF((float)(Xcoor[i] - polyLineROI_Radius), (float)(Ycoor[i] - polyLineROI_Radius),
                    polyLineROI_Radius * 2, polyLineROI_Radius * 2);
                var roi1 = new ROI(ROItype.Elipsoid, Rect1, flim_parameters.nChannels, i + 1, Roi3d, Z);
                polyLineROIs.Add(roi1);
            }
            return EquiDistanceCircleCoord;
        }

        public PointF[] GetSmoothCurve()
        {
            if (ROI_type != ROItype.PolyLine)
                return null;

            if (Points.Length < 1)
                return null;

            PointF[] points1;
            if (Points.Length == 1)
            {
                points1 = (PointF[])Points.Clone();
            }
            else if (Points.Length == 2)
            {
                double dx = Points[1].X - Points[0].X;
                double dy = Points[1].Y - Points[0].Y;
                int dist = (int)Math.Ceiling(Math.Sqrt(dx * dx + dy * dy));

                if (dist == 0)
                    points1 = null;
                else
                {
                    points1 = new PointF[dist];
                    for (int i = 0; i < points1.Length; i++)
                    {
                        points1[i].X = Points[0].X + (float)dx * i / (float)dist;
                        points1[i].Y = Points[0].Y + (float)dy * i / (float)dist;
                    }
                }
            }
            else
            {
                using (var path = new GraphicsPath())
                {
                    path.AddCurve(Points, 1.0F);
                    /* use a unit matrix to get points per pixel */
                    using (var mx = new Matrix(1, 0, 0, 1, 0, 0))
                    {
                        path.Flatten(mx, 0.001f);
                    }
                    /* store points in a list */
                    var list_of_points = new List<PointF>(path.PathPoints);
                    points1 = list_of_points.ToArray();
                }
            }
            SmoothCurvePoints = points1;
            return points1;
        }

        private void producePointArray()
        {
            Points = new PointF[X.Length];
            for (int i = 0; i < X.Length; i++)
            {
                Points[i] = new PointF(X[i], Y[i]);
            }
        }

        private bool isInsideROI_Elipse(PointF P)
        {
            bool c = false;
            double a = (double)Rect.Width / 2.0;
            double b = (double)Rect.Height / 2.0;
            double CenterX = Rect.Left + a;
            double CenterY = Rect.Top + b;

            c = isInsideROI_Elipse(P, CenterX, CenterY, a, b);

            return c;
        }

        private bool isInsideROI_Elipse(PointF P, double CenterX, double CenterY, double rad_X, double rad_Y)
        {
            bool c = false;

            float px = P.X + 0.5f; //? 
            float py = P.Y + 0.5f; //?
            double dx = (px - CenterX) / rad_X;
            double dy = (py - CenterY) / rad_Y;

            double Dist_sq = dx * dx + dy * dy;

            if (Dist_sq <= 1)
                c = true;

            return c;
        }

        private bool isInsideROI_Rect(PointF P)
        {
            bool c = false;
            if (Rect.Left <= P.X && Rect.Right > P.X && Rect.Top <= P.Y && Rect.Bottom > P.Y)
                c = true;
            return c;
        }

        private bool IsInsideROI_Polygon(PointF P)
        {

            bool c = MathLibrary.GraphicCalc.isInsidePolygon(Points, Points.Length, P);

            //bool c = false;
            //int n = X.Length;
            //int i, j;
            //float px = P.X; // + 0.5f; //? 
            //float py = P.Y; // + 0.5f; //?

            //for (i = 0, j = n - 1; i < n; j = i++)
            //{
            //    if (((Y[i] > py) != (Y[j] > py)) && (px < (X[j] - X[i]) * (px - Y[i]) / (Y[j] - Y[i]) + X[i]))
            //        c = !c;
            //}

            return c;
        }

        private bool isInsideROI_PolyLine(PointF P, out int circleID)
        {
            bool c = false;
            circleID = -1;
            if (SmoothCurvePoints == null)
            {
                if (Points.Length > 1)
                    GetSmoothCurve();
                else
                    return false;
            }

            if (EquiDistanceCircleCoord == null)
            {
                GetEqualDistanceCenters(polyLineROI_Radius);
            }

            if (EquiDistanceCircleCoord == null)
                return false;

            for (int i = 0; i < EquiDistanceCircleCoord.Length; i++)
            {
                double x = EquiDistanceCircleCoord[i].X;
                double y = EquiDistanceCircleCoord[i].Y;
                if (isInsideROI_Elipse(P, x, y, polyLineROI_Radius, polyLineROI_Radius))
                {
                    c = true;
                    circleID = i;
                    break;
                }
            }
            return c;
        }

        public enum ROItype
        {
            Polygon = 0,
            Rectangle = 1,
            Elipsoid = 2,
            Line = 3,
            FreeLine = 4,
            PolyLine = 5,
            NoRoi = 6,
            FreeHand = 7,
            Traced = 8,
            Angle = 9,
            Point = 10,
        }

    }

}
