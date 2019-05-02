using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FLIMage.Analysis
{
    public class ROI
    {
        public ROItype ROI_type = ROItype.Rectangle;
        public int[] X = new int[1];
        public int[] Y = new int[1];
        public Point[] Points;
        public Rectangle Rect = new Rectangle(0, 0, 0, 0);

        //Curve and Fitting stored in ROI.
        public int nChannels = 2;
        public List<double[]> LifetimeX = new List<double[]>();
        public List<double[]> LifetimeY = new List<double[]>();
        public List<double[]> beta = new List<double[]>();
        public double[] intensity;
        public double[] nPixels;
        public double[] sumIntensity;
        public double[] tau_m;
        public double[] tau_m_fromMAP;
        public double[] xi_square;
        public int ID = 1;

        //For 3DROI. Not implemented yer.
        public int[] Z = new int[1]; //Range from Z[0] to Z[1].
        public bool Roi3d = false;
        //public double[] offset_fit;

        public ROI()
        { }

        public ROI(ROI roi)
        {
            ROI_type = roi.ROI_type;
            X = (int[])roi.X.Clone();
            Y = (int[])roi.Y.Clone();
            Rect = new Rectangle(X.Min(), Y.Min(), (X.Max() - X.Min() + 1), (Y.Max() - Y.Min() + 1));
            producePointArray();
            nChannels = roi.nChannels;
            initializeFittingParam();
            ID = roi.ID;

            Z = (int[])roi.Z.Clone();
            Roi3d = roi.Roi3d;
        }

        public ROI(ROItype R, int[] x, int[] y, int n_channels, int id, bool roi3d, int[] z) //Constructor
        {
            ROI_type = R;
            //X = new int[x.Length];
            //Y = new int[y.Length];
            X = (int[])x.Clone();
            Y = (int[])y.Clone();
            Rect = new Rectangle(x.Min(), y.Min(), (x.Max() - x.Min()), (y.Max() - y.Min()));
            producePointArray();
            nChannels = n_channels;
            initializeFittingParam();
            ID = id;

            Roi3d = roi3d;
            Z = (int[])z.Clone();
        }

        public ROI(ROItype R, Rectangle Rect1, int n_channels, int id, bool roi3d, int[] z) //Constructor 2
        {
            ROI_type = R;
            X = new int[] { Rect1.Left, Rect1.Right - 1, Rect1.Right - 1, Rect1.Left };
            Y = new int[] { Rect1.Top, Rect1.Top, Rect1.Bottom - 1, Rect1.Bottom - 1 };
            producePointArray();
            Rect = new Rectangle(Rect1.Location, Rect1.Size);
            nChannels = n_channels;
            ID = id;
            initializeFittingParam();

            Roi3d = roi3d;
            Z = (int[])z.Clone();
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

        public void initializeFittingParam()
        {
            tau_m = new double[nChannels];
            tau_m_fromMAP = new double[nChannels];
            intensity = new double[nChannels];
            sumIntensity = new double[nChannels];
            nPixels = new double[nChannels];
            xi_square = new double[nChannels];

            double[] someVal = new double[] { 0.0, 0.0 };
            for (int ch = 0; ch < nChannels; ch++)
            {
                beta.Add(someVal);
                LifetimeX.Add(someVal);
                LifetimeY.Add(someVal);
            }
        }

        public void ChangeChannelNumber(int finalChannels)
        {

            if (finalChannels != nChannels)
            {
                Array.Resize(ref tau_m, finalChannels);
                Array.Resize(ref tau_m_fromMAP, finalChannels);
                Array.Resize(ref intensity, finalChannels);
                Array.Resize(ref sumIntensity, finalChannels);
                Array.Resize(ref nPixels, finalChannels);
                Array.Resize(ref xi_square, finalChannels);

                if (finalChannels < nChannels)
                {
                    int subChannels = -finalChannels + nChannels;
                    beta.RemoveRange(beta.Count, subChannels);
                    LifetimeX.RemoveRange(beta.Count, subChannels);
                    LifetimeY.RemoveRange(beta.Count, subChannels);
                }
                else if (finalChannels > nChannels)
                {
                    double[] someVal = new double[] { 0.0, 0.0 };
                    for (int i = 0; i < finalChannels - nChannels; i++)
                    {
                        beta.Add(someVal);
                        LifetimeX.Add(someVal);
                        LifetimeY.Add(someVal);
                    }
                }

                nChannels = finalChannels;
            }

        }


        public void shiftRoi(Point P)
        {
            if (Rect != null)
            {
                Rect.Location = new Point(Rect.Location.X + P.X, Rect.Location.Y + P.Y);
                for (int i = 0; i < X.Length; i++)
                {
                    X[i] = X[i] + P.X;
                    Y[i] = Y[i] + P.Y;
                    Points[i] = new Point(X[i], Y[i]);
                }
            }
        }

        public void shiftRoiCorner(Point P, int whichConer)
        {
            if (Rect != null)
            {
                Rect.Location = new Point(Rect.Location.X + P.X, Rect.Location.Y + P.Y);
                for (int i = 0; i < X.Length; i++)
                {
                    X[i] = X[i] + P.X;
                    Y[i] = Y[i] + P.Y;
                    Points[i] = new Point(X[i], Y[i]);
                }
            }
        }

        public ROI MovePoint(Point P, int loc)
        {
            int[] X1, Y1;
            X1 = (int[])X.Clone();
            Y1 = (int[])Y.Clone();
            X1[loc] = P.X;
            Y1[loc] = P.Y;
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

            int n_channels = nChannels;
            var newRoi = new ROI(ROI_type, X1, Y1, n_channels, ID, Roi3d, (int[])Z.Clone());
            return newRoi;
        }


        public ROI addPoints(Point P)
        {
            int[] X1, Y1;
            if (X == null)
            {
                X1 = new int[1];
                Y1 = new int[1];
                //Points = new Point[1];
                X1[0] = P.X;
                Y1[0] = P.Y;
            }
            else
            {
                int n = X.Length;
                X1 = new int[n + 1];
                Y1 = new int[n + 1];
                //Points = new Point[n + 1];
                for (int i = 0; i < n; i++)
                {
                    X1[i] = X[i];
                    Y1[i] = Y[i];
                }
                X1[n] = P.X;
                Y1[n] = P.Y;
            }
            int n_channels = nChannels;

            var newRoi = new ROI(ROI_type, X1, Y1, n_channels, ID, Roi3d, (int[])Z.Clone());

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
            int n_channels = nChannels;
            int[] X1 = new int[X.Length];
            int[] Y1 = new int[Y.Length];
            for (int i = 0; i < X.Length; i++)
            {
                X1[i] = (int)Math.Round(X[i] * scale);
                Y1[i] = (int)Math.Round(Y[i] * scale);
                //Points[i] = new Point(X[i], Y[i]);
            }

            ROI roi = new ROI(ROI_type, X1, Y1, n_channels, ID, Roi3d, (int[])Z.Clone());
            return roi;
        }

        public bool IsInsideRoi(Point P)
        {
            bool c = false;
            if (ROI_type.Equals(ROItype.Polygon))
                c = IsInsideROI_Polygon(P);
            else if (ROI_type.Equals(ROItype.Rectangle))
                c = isInsideROI_Rect(P);
            else
                c = isInsideROI_Elipse(P);

            return c;

        }

        public bool isNearCornerOfRoi(Point P, out int loc)
        {
            bool c = false;
            c = isNearConerOfRoi(P, out loc);
            return c;
        }

        public bool isEditable()
        {
            return ROI_type.Equals(ROItype.Elipsoid) || ROI_type.Equals(ROItype.Rectangle) || ROI_type.Equals(ROItype.Polygon);
        }

        private bool isNearConerOfRoi(Point P, out int loc)
        {
            bool c = false;
            int winSize = 2;
            loc = 0;
            for (int i = 0; i < X.Length; i++)
            {
                int x = X[i];
                int y = Y[i];
                if (P.X <= x + winSize && P.X >= x - winSize && P.Y <= y + winSize && P.Y >= y - winSize)
                {
                    c = true;
                    loc = i;
                    return c;
                }
            }

            return c;
        }

        private void producePointArray()
        {
            Points = new Point[X.Length];
            for (int i = 0; i < X.Length; i++)
            {
                Points[i] = new Point(X[i], Y[i]);
            }
        }

        private bool isInsideROI_Elipse(Point P)
        {
            bool c = false;
            double a = (double)Rect.Width / 2.0;
            double b = (double)Rect.Height / 2.0;
            double CenterX = Rect.Left + a;
            double CenterY = Rect.Top + b;
            double Dist_sq = Math.Pow((P.X - CenterX) / a, 2) + Math.Pow((P.Y - CenterY) / b, 2);

            if (Dist_sq <= 1)
                c = true;

            return c;
        }


        private bool isInsideROI_Rect(Point P)
        {
            bool c = false;
            if (Rect.Left <= P.X && Rect.Right > P.X && Rect.Top <= P.Y && Rect.Bottom > P.Y)
                c = true;
            return c;
        }

        private bool IsInsideROI_Polygon(Point P)
        {
            bool c = false;
            int n = X.Length;
            int i, j;

            for (i = 0, j = n - 1; i < n; j = i++)
            {
                if (((Y[i] > P.Y) != (Y[j] > P.Y)) && (P.X < (X[j] - X[i]) * (P.Y - Y[i]) / (Y[j] - Y[i]) + X[i]))
                    c = !c;
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
