using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MathLibrary
{
    public class ImageProcessing
    {
        static public ushort[,] ImportImage(string filename)
        {
            var bitmap = new Bitmap(filename);
            var arr = new ushort[bitmap.Height, bitmap.Width];

            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                {
                    ushort pixelColor = bitmap.GetPixel(x, y).R;
                    arr[y, x] = pixelColor;
                }

            return arr;
        }

        static public void SaveImage(ushort[,] image, string filename)
        {
            var iHeight = image.GetLength(0);
            var iWidth = image.GetLength(1);

            Bitmap bitmap = new Bitmap(iWidth, iHeight);
            for (int y = 0; y < iHeight; y++)
                for (int x = 0; x < iWidth; x++)
                {
                    Color nc = Color.FromArgb(image[y, x], image[y, x], image[y, x]);
                    bitmap.SetPixel(x, y, nc);
                }

            bitmap.Save(filename);
        }

        static public void EvenOddImage(ushort[,] image, out ushort[,] evenImage, out ushort[,] oddImage)
        {
            var iHeight = image.GetLength(0);
            var iWidth = image.GetLength(1);

            evenImage = new ushort[iHeight / 2, iWidth];
            oddImage = new ushort[iHeight / 2, iWidth];

            for (int y = 0; y < iHeight; y++)
            {
                bool even1 = y % 2 == 0;
                for (int x = 0; x < iWidth; x++)
                {
                    if (even1)
                        evenImage[y / 2, x] = image[y, x];
                    else
                        oddImage[y / 2, x] = image[y, x];
                }
            }

        }

        static public ushort[][][,,] PermuteFLIM5D(ushort[][][,,] FLIM_in, bool deepCopy)
        {
            int n_c = FLIM_in.Length;
            int n_z = 1; // FLIM_in[0].Length;
            for (int c = 0; c < n_c; c++)
            {
                if (FLIM_in[c] != null)
                {
                    n_z = FLIM_in[c].Length;
                    break;
                }
            }

            var FLIM5D = new ushort[n_z][][,,];
            for (int z = 0; z < n_z; z++)
            {
                FLIM5D[z] = new ushort[n_c][,,];
                for (int c = 0; c < n_c; c++)
                {
                    if (FLIM_in[c] != null)
                    {
                        if (deepCopy)
                            if (FLIM5D[z][c] != null && FLIM5D[z][c].Length == FLIM_in[c][z].Length)
                            {
                                Array.Copy(FLIM_in[c][z], FLIM5D[z][c], FLIM_in[c][z].Length);
                            }
                            else
                            {
                                FLIM5D[z][c] = (ushort[,,])FLIM_in[c][z].Clone();
                            }
                        else
                            FLIM5D[z][c] = FLIM_in[c][z];
                    }
                    else
                        FLIM5D[z][c] = null;
                }
            }
            return FLIM5D;
        }

        static public ushort[][][,,] FLIM_Pages2FLIMRaw5D(ushort[][][] FLIM_Pages, int[] xy, int[] n_time)
        {
            var nChannels = FLIM_Pages[0].Length;
            ushort[][][,,] image5D = new ushort[nChannels][][,,];
            for (int ch = 0; ch < nChannels; ch++)
            {
                image5D[ch] = new ushort[FLIM_Pages.Length][,,];
                for (int page = 0; page < FLIM_Pages.Length; page++)
                {
                    if (FLIM_Pages[page][ch] == null)
                    {
                        image5D[ch] = null;
                        break;
                    }
                    else
                    {
                        image5D[ch][page] = new ushort[xy[0], xy[1], n_time[ch]];
                        Buffer.BlockCopy(FLIM_Pages[page][ch], 0, image5D[ch][page], 0, FLIM_Pages[page][ch].Length * sizeof(ushort));
                    }
                }
            }

            return image5D;
        }

        static public ushort[][][,,] FLIM_Pages2FLIMRaw5D(ushort[][][,,] FLIM_Pages)
        {
            var nChannels = FLIM_Pages[0].Length;
            ushort[][][,,] image5D = new ushort[nChannels][][,,];
            for (int ch = 0; ch < nChannels; ch++)
            {
                image5D[ch] = new ushort[FLIM_Pages.Length][,,];
                for (int page = 0; page < FLIM_Pages.Length; page++)
                {
                    if (FLIM_Pages[page] == null)
                    {
                        image5D = new ushort[nChannels][][,,];
                    }

                    if (FLIM_Pages[page][ch] == null)
                    {
                        image5D[ch] = null;
                        break;
                    }
                    else
                    {
                        image5D[ch][page] = (ushort[,,])FLIM_Pages[page][ch].Clone();
                    }
                }
            }

            return image5D;
        }


        static public T[,] ShiftImage<T>(T[,] Image, int shift_x, int shift_y)
        {
            int ysize = Image.GetLength(0);
            int xsize = Image.GetLength(1);
            T[,] result = new T[ysize, xsize]; //faster by copying first and calculate on itself. 

            for (int y = -0; y < ysize; y++)
                for (int x = 0; x < xsize; x++)
                {
                    if (y - shift_y >= 0 && y - shift_y < ysize && x - shift_x >= 0 && x - shift_x < xsize)
                        result[y, x] = Image[y - shift_y, x - shift_x];
                }

            return result;
        }


        static public float[,] ImageSqrt(float[,] Image)
        {
            int ysize = Image.GetLength(0);
            int xsize = Image.GetLength(1);
            float[,] result = (float[,])Image.Clone(); //faster by copying first and calculate on itself. 
            for (int y = 0; y < ysize; y++)
                for (int x = 0; x < xsize; x++)
                {
                    result[y, x] = (float)Math.Sqrt(result[y, x]);
                }
            return result;
        }

        static public double[] FitImageWithGaussian2D_NearPeak(ushort[,] image, int fitRange)
        {
            var nx = image.GetLength(1);
            var ny = image.GetLength(0);
            int k = 0;
            int peak = 0;
            int maxX = 0;
            int maxY = 0;
            for (int y = 0; y < ny; y++)
                for (int x = 0; x < nx; x++)
                {
                    var z = image[y, x];
                    if (peak < z)
                    {
                        peak = z;
                        maxX = x;
                        maxY = y;
                    }
                    k++;
                }

            int fitX0 = maxX - fitRange;
            int fitX1 = maxX + fitRange;
            int fitY0 = maxY - fitRange;
            int fitY1 = maxY + fitRange;
            if (fitX0 < 0)
                fitX0 = 0;
            if (fitX1 > nx)
                fitX1 = nx;
            if (fitY0 < 0)
                fitY0 = 0;
            if (fitY1 > ny)
                fitY1 = ny;

            var beta1 = FitImageWithGaussian2D(image, new int[] { fitX0, fitX1 }, new int[] { fitY0, fitY1 });
            beta1[1] = beta1[1] + fitX0;
            beta1[3] = beta1[3] + fitY0;

            return beta1;
        }

        static public double[] FitImageWithGaussian2D(ushort[,] image)
        {
            return FitImageWithGaussian2D(image, new int[] { 0, image.GetLength(1) }, new int[] { 0, image.GetLength(0) });
        }

        /// <summary>
        /// Return Gaussian parameters:
        /// [peak, x-center, x-variance, y-center, y-variance, background]
        /// </summary>
        /// <param name="image"></param>
        /// <param name="xrange"></param>
        /// <param name="yrange"></param>
        /// <returns></returns>
        static public double[] FitImageWithGaussian2D(ushort[,] image, int[] xrange, int[] yrange)
        {
            var nx = xrange[1] - xrange[0];
            var ny = yrange[1] - yrange[0];
            double[,] xy = new double[2, nx * ny];
            double[] z = new double[nx * ny];
            int k = 0;
            double peak = 0;
            double min = short.MaxValue;
            double maxX = 0;
            double maxY = 0;
            for (int y = yrange[0]; y < yrange[1]; y++)
                for (int x = xrange[0]; x < xrange[1]; x++)
                {
                    xy[0, k] = x;
                    xy[1, k] = y;
                    z[k] = (double)image[y, x];
                    if (peak < z[k])
                    {
                        peak = z[k];
                        maxX = x;
                        maxY = y;
                    }

                    if (min > z[k])
                        min = z[k];

                    k++;
                }

            var beta0 = new double[6];
            beta0[0] = peak - min;
            beta0[1] = maxX;
            beta0[2] = nx / 8;
            beta0[3] = maxY;
            beta0[4] = ny / 8;
            beta0[5] = min;

            //var betaMax = new double[6];
            //betaMax[0] = peak * 10;
            //betaMax[1] = nx;
            //betaMax[2] = nx;
            //betaMax[3] = ny;
            //betaMax[4] = ny;
            //betaMax[5] = peak;

            //var betaMin = new double[6];
            //betaMin[0] = 0;
            //betaMin[1] = 0;
            //betaMin[2] = 0;
            //betaMin[3] = 0;
            //betaMin[4] = 0;
            //betaMin[5] = -peak;

            var nlin2d = new Fitting.Nlinfit(beta0, xy, z);
            //nlin2d.betaMax = betaMax;
            //nlin2d.betaMin = betaMin;
            nlin2d.modelFunc2 = ((betaA, xA) => Gaussian2D(betaA, xA));
            nlin2d.Perform();

            return nlin2d.beta;
        }

        /// <summary>
        /// 2D Gaussian.
        /// </summary>
        /// <param name="beta">[peak, x-center, x-variance, y-center, y-variance, background]</param>
        /// <param name="xy">rows = x & y values, columns: x = 0, y = 1 </param>
        /// <returns></returns>
        static public double[] Gaussian2D(double[] beta, double[,] xy)
        {
            int len = xy.GetLength(1);
            double[] z = new double[len];
            for (int i = 0; i < len; i++)
            {
                var x = xy[0, i];
                var y = xy[1, i];
                z[i] = beta[0] * Math.Exp(-Math.Pow(x - beta[1], 2) / 2 / beta[2] - Math.Pow(y - beta[3], 2) / 2 / beta[4]) + beta[5];
            }

            return z;
        }

        static public float[,] ImageSmooth<T>(T[,] Image, int factor)
        {
            factor = factor / 2;
            int xsize = Image.GetLength(1);
            int ysize = Image.GetLength(0);
            float[,] result = new float[ysize, xsize];
            for (int y = 0; y < ysize; y++)
                for (int x = 0; x < xsize; x++)
                {
                    float value = 0;
                    int count = 0;
                    for (int y1 = y - factor; y1 < y + factor; y1++)
                        for (int x1 = x - factor; x1 < x + factor; x1++)
                        {
                            if (y1 >= 0 && y1 < ysize && x1 >= 0 && x1 < xsize)
                            {
                                value += Convert.ToSingle(Image[y1, x1]);
                                count++;
                            }
                        }
                    result[y, x] = value / (float)count;
                }

            return result;
        }


        /// <summary>
        /// Get focus frame from imageZCXY 4D format --- this is simply based on intensity.
        /// for fit, it returns beta0 for y = beta0[0] * Math.Exp(- Math.Pow(x[i] - beta0[1], 2) / 2.0 /beta0[2]) + beta0[3];
        /// Otherwise, it returns [peak_value, focus_index]
        /// </summary>
        /// <param name="imgZCYX"></param>
        /// <param name="channel"></param>
        /// <param name="range"></param>
        /// <param name="fit"></param>
        /// <returns></returns>
        public static double[] GetFocusFrameByIntensity(ushort[][][,] imgZCYX, int channel, int[] range, bool fit, out double[] z_data)
        {
            int n_Z = imgZCYX.Length;
            int length = range[1] - range[0];

            double[] focusList = new double[length];

            int focusIndex = 0;
            double focusVal = 0;
            for (int z = 0; z < length; z++)
            {
                focusList[z] = MatrixCalc.MatrixSum(imgZCYX[range[0] + z][channel]); //or MeasureFocusFFT
                if (focusVal < focusList[z])
                {
                    focusVal = focusList[z];
                    focusIndex = z;
                }
            }

            z_data = focusList;
            double[] peak;

            if (fit)
                peak = MatrixCalc.FindPeak_WithGaussianFit1D(focusList);
            else
                peak = new double[] { focusVal, focusIndex };

            return peak;
        }


        public static int GetFocusFrame(ushort[][][,] imgZCYX, int channel, int[] range)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int n_Z = imgZCYX.Length;
            double[] focusList = new double[n_Z];


            int focusIndex = 0;
            double focusVal = 0;
            for (int z = range[0]; z < range[1]; z++)
            {
                focusList[z] = MeasureFocus_FFT(imgZCYX[z][channel]); //or MeasureFocusFFT
                if (focusVal < focusList[z])
                {
                    focusVal = focusList[z];
                    focusIndex = z;
                }
            }

            sw.Stop();
            Debug.WriteLine("Calculation time = " + sw.ElapsedMilliseconds);

            return focusIndex;
        }

        /// <summary>
        /// Program to compute Z focus. Misha's paper. Geusebroek et al, 2000.
        /// Use FFT for convolution. FFT is much faster with Intel MKL.
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static double MeasureFocus_FFT(ushort[][] img)
        {
            int width = img[0].Length;
            int height = img.Length;
            int w_size = 15;
            int nn = w_size / 2; // from -7 to 7
            double sig = (double)nn / 2.5;
            Matrix<double> gx = Matrix<double>.Build.Dense(width, height);
            Matrix<double> gy = Matrix<double>.Build.Dense(width, height);

            for (int x = -nn + width / 2; x < nn + 1 + width / 2; x++)
                for (int y = -nn + height / 2; y < nn + 1 + height / 2; y++)
                {
                    int x1 = x - width / 2;
                    int y1 = y - height / 2;
                    double gg = Math.Exp(-(x1 * x1 + y1 * y1) / (2 * sig * sig)) / (2 * Math.PI * sig);
                    gx[x, y] = -x1 * gg / (sig * sig);
                    gy[x, y] = -y1 * gg / (sig * sig);
                }

            gx = gx / MatrixCalc.MatrixSum_Matrix(gx);
            gy = gy / MatrixCalc.MatrixSum_Matrix(gy);

            var rx = MatrixCalc.MatrixConvolution__FFT(img, gx);
            var ry = MatrixCalc.MatrixConvolution__FFT(img, gy);
            var f = rx.PointwisePower(2) + ry.PointwisePower(2);

            double f_m = MatrixCalc.MatrixSum_Matrix(f) / (width * height);
            return f_m;
        }

        public static double MeasureFocus_FFT(ushort[,] img)
        {
            int width = img.GetLength(1);
            int height = img.GetLength(0);
            int w_size = 15;
            int nn = w_size / 2; // from -7 to 7
            double sig = (double)nn / 2.5;
            Matrix<double> gx = Matrix<double>.Build.Dense(width, height);
            Matrix<double> gy = Matrix<double>.Build.Dense(width, height);

            for (int x = -nn + width / 2; x < nn + 1 + width / 2; x++)
                for (int y = -nn + height / 2; y < nn + 1 + height / 2; y++)
                {
                    int x1 = x - width / 2;
                    int y1 = y - height / 2;
                    double gg = Math.Exp(-(x1 * x1 + y1 * y1) / (2 * sig * sig)) / (2 * Math.PI * sig);
                    gx[x, y] = -x1 * gg / (sig * sig);
                    gy[x, y] = -y1 * gg / (sig * sig);
                }

            gx = gx / MatrixCalc.MatrixSum_Matrix(gx);
            gy = gy / MatrixCalc.MatrixSum_Matrix(gy);

            var rx = MatrixCalc.MatrixConvolution__FFT(img, gx);
            var ry = MatrixCalc.MatrixConvolution__FFT(img, gy);
            var f = rx.PointwisePower(2) + ry.PointwisePower(2);

            double f_m = MatrixCalc.MatrixSum_Matrix(f) / (width * height);
            return f_m;
        }

        static public double MatrixMeasureDrift2D_FFT(ushort[,] matrixA, ushort[,] matrixB, out double[] result)
        {
            int height = matrixA.GetLength(0);
            int width = matrixA.GetLength(1);

            int height2 = matrixB.GetLength(0);
            int width2 = matrixB.GetLength(1);

            if (height != height2 || width != width2)
            {
                result = null;
                return 0.0;
            }

            var matrixC = MatrixCalc.ConvertToComplexMatrix(matrixA);
            var matrixC2 = MatrixCalc.ConvertToComplexMatrix(matrixB);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    matrixC2[x, y] = new Complex(matrixB[y, x], 0);


            MatrixCalc.FFT2DForward(ref matrixC);
            MatrixCalc.FFT2DForward(ref matrixC2);

            matrixC[0, 0] = 0; //ignore DC component
            matrixC2[0, 0] = 0; //ignore DC component

            var AutoCorr = matrixC.PointwiseMultiply(matrixC2.Conjugate());

            MatrixCalc.FFT2DInverse(ref AutoCorr);

            var AutoCorrR = AutoCorr.Real();

            var AutoCorrRL = MatrixCalc.Matrix2Vector(AutoCorrR);
            var peak = AutoCorrRL.MaximumIndex();
            var peakV = AutoCorrRL.Maximum();

            int peakY = peak / width;
            int peakX = peak % width;

            if (peakX > width / 2)
                peakX = peakX - width;
            if (peakY > height / 2)
                peakY = peakY - height;

            result = new double[] { -peakX, -peakY };
            return peakV;
        }
        /// <summary>
        /// This is slow.
        /// </summary>
        /// <param name="matrixA"></param>
        /// <param name="matrixB"></param>
        /// <param name="calculationRange"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        static public double MatrixMeasureDrift2D(ushort[][] matrixA, ushort[][] matrixB, int[] calculationRange, out double[] result)
        {
            int y_length = matrixA.Length;
            int x_length = matrixA[0].Length;
            int y_lengthB = matrixB.Length;
            int x_lengthB = matrixB[0].Length;

            int calculationRangeX = calculationRange[0];
            int calculationRangeY = calculationRange[1];

            if (calculationRangeY > (y_length - 1) / 2)
                calculationRangeY = (y_length - 1) / 2; // 128 --> -64 to +63, 127 --> -63 to + 63, 126 --> 63 etc.

            if (calculationRangeX > (x_length - 1) / 2)
                calculationRangeX = (x_length - 1) / 2; // 128 --> -64 to +63, 127 --> -63 to + 63, 126 --> 63 etc.

            double[][] xcross = MatrixCalc.MatrixCreate2D<double>(2 * calculationRangeY, 2 * calculationRangeX);

            double meanA = MatrixCalc.Mean2D<ushort>(matrixA); //, new int[] { calculationRange, y_length - calculationRange }, new int[] { calculationRange, x_length - calculationRange });
            double meanB = MatrixCalc.Mean2D<ushort>(matrixB); //, new int[] { calculationRange, y_length - calculationRange }, new int[] { calculationRange, x_length - calculationRange });

            for (int y = -calculationRangeY; y < calculationRangeY; y++)
            {
                for (int x = -calculationRangeX; x < calculationRangeX; x++)
                {

                    int x1 = x + calculationRangeX;
                    int y1 = y + calculationRangeY;

                    double sum = 0;
                    double count = 0;

                    for (int y2 = calculationRangeY; y2 < y_length - calculationRangeY; y2++)
                        for (int x2 = calculationRangeX; x2 < x_length - calculationRangeX; x2++)
                        {
                            sum += (matrixA[y2][x2] - meanA) * (matrixB[y2 + y][x2 + x] - meanB);
                            //sum += (matrixA[y2][x2]) * (matrixB[y2 + y][x2 + x]);
                            count++;
                        }

                    xcross[y1][x1] = sum / count;
                }
            }
            //}
            //);

            result = new double[2];
            double maxValue = 0;

            for (int y1 = 0; y1 < xcross.Length; y1++)
                for (int x1 = 0; x1 < xcross[0].Length; x1++)
                {
                    if (xcross[y1][x1] > maxValue)
                    {
                        maxValue = xcross[y1][x1];
                        result[1] = y1 - calculationRangeY;
                        result[0] = x1 - calculationRangeX;
                    }
                }

            return maxValue / MatrixCalc.Std2D(matrixA) / MatrixCalc.Std2D(matrixB);
        }



        static public ushort[,,] MatrixCorrectDriftFLIM(ushort[,,] matrixA, double[] xyDrift) ///Let's do correctly. Y - X.
        {
            int height = matrixA.GetLength(0);
            int width = matrixA.GetLength(1);
            int n_time = matrixA.GetLength(2);

            ushort[,,] resultX = new ushort[height, width, n_time];
            ushort[,,] result = new ushort[height, width, n_time];
            int xDrift = (int)xyDrift[0];
            int yDrift = (int)xyDrift[1];



            if (xDrift < 0)
            {
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width + xDrift; x++)
                        Array.Copy(matrixA, (y * width + x) * n_time, resultX, (y * width + x - xDrift) * n_time, n_time); //width + xDrift);                                                                                                                             //});
            }
            else
            {
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width - xDrift; x++)
                        Array.Copy(matrixA, (y * width + x + xDrift) * n_time, resultX, (y * width + x) * n_time, n_time); //width + xDrift);
            }

            if (yDrift < 0)
            {
                for (int y = 0; y < height + yDrift; y++)
                    for (int x = 0; x < width; x++)
                        Array.Copy(resultX, (y * width + x) * n_time, result, ((y - yDrift) * width + x) * n_time, n_time);
            }
            else
            {
                for (int y = 0; y < height - yDrift; y++)
                    for (int x = 0; x < width; x++)
                        Array.Copy(resultX, ((y + yDrift) * width + x) * n_time, result, (y * width + x) * n_time, n_time);
            }
            return result;
        }



        public static float[,] GetLifetimeMapFromFLIM(UInt16[,,] acqFImg, int[] trange, float psPerChannel, float offset)
        {
            if (acqFImg == null)
                return null;

            int height = acqFImg.GetLength(0);
            int width = acqFImg.GetLength(1);
            int n_dtime = acqFImg.GetLength(2);

            if (n_dtime < 2)
                return null;

            var LifetimeMap = new float[height, width];

            if (trange[0] < 0)
                trange[0] = 0;
            if (trange[1] > n_dtime)
                trange[1] = n_dtime;

            var timeSeries = new float[n_dtime];
            for (int i = 0; i < n_dtime; i++)
                timeSeries[i] = i;

            var Vec = new ushort[n_dtime];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; ++x)
                {
                    MatrixCalc.extract3rdAxis(acqFImg, ref Vec, y, x);
                    double sumT = MatrixCalc.Dot_withRange(Vec, timeSeries, trange); //Return double.
                    double sum = MatrixCalc.calcSumUshort(Vec, trange); //return uint16. should make it float?

                    if (sum != 0)
                        LifetimeMap[y, x] = (float)(sumT / sum * psPerChannel / 1000.0) - offset;
                    else
                        LifetimeMap[y, x] = 0.0f;
                }

            return LifetimeMap;
        }

        public static void GetProjectFromFLIMLines(UInt16[,,] acqFImg, UInt16[,] Destination, int[] time_range, int StartLine, int EndLine)
        {
            if (acqFImg == null)
            {
                Destination = null;
                return;
            }

            int height = acqFImg.GetLength(0);
            int width = acqFImg.GetLength(1);
            int n_dtime = acqFImg.GetLength(2);

            if (time_range == null)
                time_range = new int[] { 0, n_dtime };

            if (time_range[0] < 0)
                time_range[0] = 0;

            if (time_range[1] > n_dtime)
                time_range[1] = n_dtime;

            if (StartLine < 0)
                StartLine = 0;
            if (EndLine > height)
                EndLine = height;
            if (StartLine > height)
                StartLine = 0;

            int simd_length = System.Numerics.Vector<ushort>.Count;
            bool hard_accel = System.Numerics.Vector.IsHardwareAccelerated;

            ushort[] Vec = new ushort[n_dtime];

            if (n_dtime == 1)
            {
                for (int y = StartLine; y < EndLine; y++)
                {
                    int siz = sizeof(ushort);
                    Buffer.BlockCopy(acqFImg, y * width * siz, Destination, y * width * siz, width * siz);
                }
            }
            else
            {
                for (int y = StartLine; y < EndLine; y++)
                    for (int x = 0; x < width; ++x)
                    {
                        if (hard_accel && n_dtime >= simd_length)
                        {
                            MatrixCalc.extract3rdAxis(acqFImg, ref Vec, y, x);
                            Destination[y, x] = MatrixCalc.calcSumFast(Vec, time_range);
                        }
                        else
                        {
                            int sum = 0;
                            for (int t = time_range[0]; t < time_range[1]; ++t)
                                sum += acqFImg[y, x, t];

                            Destination[y, x] = (ushort)sum;
                        }
                    }
            }
        }


        public static UInt16[,] GetProjectFromFLIM(UInt16[,,] acqFImg, int[] t_range)
        {
            if (acqFImg == null)
                return null;

            int height = acqFImg.GetLength(0);
            int width = acqFImg.GetLength(1);
            int n_dtime = acqFImg.GetLength(2);

            UInt16[,] Project1 = new ushort[height, width];
            GetProjectFromFLIMLines(acqFImg, Project1, t_range, 0, height);
            return Project1;
        }

        public enum ColorScheme
        {
            Spectrum,
            Fire,
            RB,
            YellowHighlight,
            YellowHighlight_Mod,
            Plasma,
        }

        //public static byte[] ValueToRGB(double val, double gray, ColorScheme color_scheme)
        //{

        //}


        public static byte[] ValueToRGB_RB(float[] val, float[] gray)
        {
            int stribe = 3;
            byte[] rgb = new byte[stribe * val.Length];
            float red, green, blue;
            float EightBits = 255;

            for (int x = 0; x < val.Length; x++)
            {
                if (val[x] <= 0)
                {
                    red = 0;
                    green = 0;
                    blue = EightBits;
                }
                else if (val[x] <= 1.0 / 2.0)
                {
                    red = (EightBits * 2.0f * val[x]);
                    green = (EightBits * 2.0f * val[x]);
                    blue = EightBits;
                }
                else if (val[x] < 1.0)
                {
                    red = EightBits;
                    green = EightBits * (-2.0f * val[x] + 2.0f);
                    blue = EightBits * (-2.0f * val[x] + 2.0f);
                }
                else
                {
                    red = EightBits;
                    green = 0;
                    blue = 0;
                }

                rgb[stribe * x + 0] = (byte)(blue * gray[x]);
                rgb[stribe * x + 1] = (byte)(green * gray[x]);
                rgb[stribe * x + 2] = (byte)(red * gray[x]);
            }
            return rgb;
        }


        public static byte[] ValueToRGB(float[] val, float[] gray, ColorScheme color_scheme)
        {

            //ColorScheme color_scheme = ColorScheme.Fire;
            if (color_scheme == ColorScheme.Fire)
                return ValueToRGB_Fire(val, gray);
            else if (color_scheme == ColorScheme.Spectrum)
                return ValueToRGB_Jet(val, gray);
            else if (color_scheme == ColorScheme.RB)
                return ValueToRGB_RB(val, gray);

            //General Scheme.
            int stribe = 3;
            byte[] rgb = new byte[stribe * val.Length];
            float[][] c = new float[5][]; //Five is optimum.
            float[] steps = new float[] { 0, 0.25f, 0.5f, 0.75f, 1 };

            if (color_scheme == ColorScheme.YellowHighlight_Mod)
            {
                c[4] = new float[] { 255, 255, 204 }; //Yellow whitish.~1.0
                c[3] = new float[] { 200, 200, 20 }; //Strong Yellow, ~0.9
                c[2] = new float[] { 65, 182, 196 }; //~0.6; 65, 182, 196
                c[1] = new float[] { 44, 127, 184 }; //~0.4
                c[0] = new float[] { 37, 52, 148 }; //~0.2
            }
            else if (color_scheme == ColorScheme.YellowHighlight)
            {
                c[4] = new float[] { 255, 255, 204 }; //Yellow whitish.~1.0
                c[3] = new float[] { 255, 255, 25 }; //Yellow, ~0.9
                c[2] = new float[] { 65, 182, 196 }; //~0.6; 65, 182, 196
                c[1] = new float[] { 44, 127, 184 }; //~0.4
                c[0] = new float[] { 37, 52, 148 }; //~0.2
            }
            else if (color_scheme == ColorScheme.Plasma)
            {
                c[4] = new float[] { 255, 255, 204 }; //Yellow whitish.
                c[3] = new float[] { 254, 217, 142 }; //Yellow
                c[2] = new float[] { 253, 141, 60 }; //red 
                c[1] = new float[] { 240, 59, 32 }; //Brown
                c[0] = new float[] { 189, 0, 38 };
            }

            var value_steps = MatrixCalc.MultiplyConstantToVector(val, (float)(c.Length - 1));
            var seps = value_steps.Select(x => (int)Math.Floor(x)).ToArray();

            for (int x = 0; x < val.Length; x++)
            {
                float val0 = val[x];
                int sep0 = seps[x]; //val = 0 - 1/4 --> 0, val <=1/4 --> 1, .... val > 1... 

                if (sep0 < 0)
                {
                    for (int i = 0; i < stribe; i++)
                        rgb[x * stribe + 2 - i] = (byte)(c[0][i] * gray[x]);
                }
                else if (sep0 < c.Length - 1)
                {
                    float s0 = steps[sep0];
                    float s1 = steps[sep0 + 1];
                    float[] c0 = c[sep0];
                    float[] c1 = c[sep0 + 1];
                    for (int i = 0; i < 3; i++)
                    {
                        var val1 = (c1[i] - c0[i]) * (val0 - s0) / (s1 - s0) + c0[i];
                        rgb[x * stribe + 2 - i] = (byte)(val1 * gray[x]);
                    }
                }
                else //Saturated.
                {
                    for (int i = 0; i < 3; i++)
                        rgb[x * stribe + 2 - i] = (byte)(c[c.Length - 1][i] * gray[x]);
                }
            }

            return rgb;
        }

        public static byte[] ValueToRGB_Fire(float[] val, float[] gray)
        {
            int stribe = 3;
            byte[] rgb = new byte[stribe * val.Length];
            float red, green, blue;
            float EightBits = 255;

            for (int x = 0; x < val.Length; x++)
            {
                if (val[x] <= 0)
                {
                    red = EightBits * 0.67f;
                    green = 0;
                    blue = EightBits;
                }
                else if (val[x] <= 0.33)
                {
                    red = EightBits * (0.7f + val[x] * 0.3f / 0.33f);
                    green = EightBits * 0.244f;
                    blue = EightBits * (1 - val[x] / 0.33f);
                }
                else if (val[x] <= 0.75)
                {
                    red = EightBits;
                    green = EightBits * (1.8f * (val[x] - 0.75f) + 1);
                    blue = 0;
                }
                else if (val[x] <= 1.0)
                {
                    red = EightBits;
                    green = EightBits;
                    blue = EightBits * (val[x] * 4.0f - 3.0f);
                }
                else
                {
                    red = EightBits;
                    green = EightBits;
                    blue = EightBits;
                }

                rgb[stribe * x + 0] = (byte)(blue * gray[x]);
                rgb[stribe * x + 1] = (byte)(green * gray[x]);
                rgb[stribe * x + 2] = (byte)(red * gray[x]);
            }
            return rgb;
        }

        public static byte[] ValueToRGB_Jet(float[] val, float[] gray)
        {
            int stribe = 3;
            byte[] rgb = new byte[stribe * val.Length];
            float red, green, blue;
            float EightBits = 255;

            for (int x = 0; x < val.Length; x++)
            {
                if (val[x] <= 0)
                {
                    red = 0;
                    green = 0;
                    blue = EightBits;
                }
                else if (val[x] <= 1.0 / 3.0)
                {
                    red = 0;
                    green = (EightBits * 3.0f * val[x]);
                    blue = EightBits;
                }
                else if (val[x] <= 2.0 / 3.0)
                {
                    red = (EightBits * (val[x] * 3.0f - 1));
                    green = EightBits;
                    blue = (EightBits * (-3.0f * val[x] + 2.0f));
                }
                else if (val[x] <= 1.0)
                {
                    red = EightBits;
                    green = (EightBits * (-3.0f * val[x] + 3.0f));
                    blue = 0;
                }
                else
                {
                    red = EightBits;
                    green = 0;
                    blue = 0;
                }

                rgb[stribe * x + 0] = (byte)(blue * gray[x]);
                rgb[stribe * x + 1] = (byte)(green * gray[x]);
                rgb[stribe * x + 2] = (byte)(red * gray[x]);
            }

            return rgb;
        }

        public static Bitmap FormatImageFLIM(double[] intensity_range, double[] FLIM_range, float[,] FLIMImg, UInt16[,] AcqImg, bool forceSquare, ColorScheme color_scheme)
        {
            if (FLIMImg == null || AcqImg == null)
                return null;

            int height = AcqImg.GetLength(0);
            int width = AcqImg.GetLength(1);

            int height1 = FLIMImg.GetLength(0);
            int width1 = FLIMImg.GetLength(1);

            int nPixels = Math.Max(height, width); //Fits to maximum.
            int startX = 0;
            int startY = 0;
            int bytePerPixel = 3;

            int stride = width * bytePerPixel;
            byte[] pixels = new byte[stride * height];

            if (forceSquare)
            {
                startX = (nPixels - width) / 2;
                startY = (nPixels - height) / 2;
                stride = nPixels * bytePerPixel;
                pixels = new byte[stride * nPixels];
            }

            float MaxInt = (float)intensity_range.Max();
            float MinInt = (float)intensity_range.Min();
            float Int_dif = MaxInt - MinInt;

            float MaxFLIM = (float)FLIM_range[1];
            float MinFLIM = (float)FLIM_range[0];
            float flim_dif = MaxFLIM - MinFLIM;


            System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            Bitmap bmp;
            if (forceSquare)
            {
                bmp = new Bitmap(nPixels, nPixels, format);
            }
            else
            {
                bmp = new Bitmap(width, height, format);
            }

            if (height != height1)
                return bmp;

            if (Int_dif > 0 && flim_dif != 0)
            {
                ushort[] intensity_ushort = MatrixCalc.LinearizeArray<ushort>(AcqImg);
                float[] intensity_float = MatrixCalc.convertToFloat(intensity_ushort);
                intensity_float = MatrixCalc.SubtractConstantFromVector(intensity_float, MinInt);
                intensity_float = MatrixCalc.DivideConstantFromVector(intensity_float, Int_dif);
                intensity_float = intensity_float.Select(x => x < 0 ? 0f : x > 1 ? 1f : x).ToArray();
                float[] flim_floatLinear = MatrixCalc.LinearizeArray<float>(FLIMImg);
                flim_floatLinear = MatrixCalc.SubtractConstantFromVector(flim_floatLinear, MinFLIM);
                flim_floatLinear = MatrixCalc.DivideConstantFromVector(flim_floatLinear, flim_dif);
                pixels = ValueToRGB(flim_floatLinear, intensity_float, color_scheme);
            }

            if (!forceSquare)
                bmp = new Bitmap(width, height, stride, format, GCHandle.Alloc(pixels, GCHandleType.Pinned).AddrOfPinnedObject());
            else
                bmp = new Bitmap(nPixels, nPixels, stride, format, GCHandle.Alloc(pixels, GCHandleType.Pinned).AddrOfPinnedObject());

            //Bitmap bmp1 = new Bitmap(bmp, new Size(targetWidth, targetHeight));
            //Bitmap bmp1 = ResizeBitmap(bmp, targetWidth, targetHeight);
            return bmp;

        }

        public static int GetMinInt(UInt16[][] Img)
        {
            int height = Img.Length;
            int MinInt = 2 ^ 15;
            for (int i = 0; i < height; i++)
            {
                int minInt1 = Img[i].Min();
                if (MinInt > minInt1)
                    MinInt = minInt1;
            }

            return MinInt;
        }


        public enum ColorBarDirection
        {
            LeftToRight = 1,
            RightToLeft = 2,
            TopToBottom = 3,
            BottomToTop = 4,
        }

        public static Bitmap CreateColorBar(int width, int height, ColorBarDirection direct, ColorScheme color_scheme)
        {

            int bytePerPixel = 3;
            int stride = width * bytePerPixel;

            //byte[] pixels = new byte[stride * height];
            byte[] pixels = new byte[width * bytePerPixel * height];
            //byte[][] pixel2D = MatrixCalc.MatrixCreate2D<byte>(height, width*bytePerPixel);

            System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            Bitmap bmp = new Bitmap(width, height, format);

            var LifetimeValues = new float[height * width];
            var gray = new float[height * width];
            gray = gray.Select(x => 1.0f).ToArray();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float lifetimeVal;

                    if (direct == ColorBarDirection.LeftToRight)
                        lifetimeVal = (float)x / (float)width;
                    else if (direct == ColorBarDirection.RightToLeft)
                        lifetimeVal = 1 - (float)x / (float)width;
                    else if (direct == ColorBarDirection.TopToBottom)
                        lifetimeVal = (float)y / (float)height;
                    else
                        lifetimeVal = 1 - (float)y / (float)height;

                    LifetimeValues[y * width + x] = lifetimeVal;
                }

            pixels = ValueToRGB(LifetimeValues, gray, color_scheme);

            bmp = new Bitmap(width, height, stride, format, GCHandle.Alloc(pixels, GCHandleType.Pinned).AddrOfPinnedObject());

            return bmp;
        }


        public static Bitmap FormatImageLines(Bitmap OriginalBitmap, double[] range, UInt16[][] AcqImg, int StartLine, int EndLine)
        {
            if (AcqImg == null)
                return null;

            int height = AcqImg.Length;
            int width = AcqImg[0].Length;

            int bytePerPixel = 3;

            float MaxInt = (float)range[1];
            float MinInt = (float)range[0];

            int nPixels = Math.Max(height, width);
            int startX = (nPixels - width) / 2;
            int startY = (nPixels - height) / 2;

            int stride = nPixels * bytePerPixel;
            byte[] pixels = new byte[nPixels * nPixels * bytePerPixel];

            //var rangePartitioner = Partitioner.Create(StartLine, EndLine);
            //Parallel.ForEach(rangePartitioner, range1 =>
            //{
            //    for (int y = range1.Item1; y < range1.Item2; y++)
            //    {

            for (int y = StartLine; y < EndLine; y++)
            {
                int val;
                for (int x = 0; x < width; x++)

                    for (int b = 0; b < bytePerPixel; b++)
                    {

                        if (b < 3)
                        {
                            if (MaxInt > MinInt)
                            {
                                val = (int)((AcqImg[y][x] - MinInt) * 255 / (MaxInt - MinInt));
                                if (val < 0)
                                    val = 0;
                                else if (val > 255)
                                    val = 255;
                            }
                            else
                            {
                                val = 0;
                            }

                            //pixel2D[y][x * bytePerPixel + b] = (byte)val;
                            pixels[(startY + y) * stride + (startX + x) * bytePerPixel + b] = (byte)val;
                        }
                        else
                        {
                            //pixel2D[y][x * bytePerPixel + b] = 255;
                            pixels[(startY + y) * stride + (startX + x) * bytePerPixel + b] = 255; //Handle 32 bit.
                        }

                    }
            }
            //}
            //); //parallel for

            PixelFormat format = PixelFormat.Format24bppRgb;

            Bitmap bmp = new Bitmap(nPixels, nPixels, stride, format, GCHandle.Alloc(pixels, GCHandleType.Pinned).AddrOfPinnedObject());

            if (OriginalBitmap != null)
            {
                try
                {
                    Graphics g = Graphics.FromImage(OriginalBitmap);
                    g.DrawImageUnscaled(bmp, 0, (int)(StartLine)); //
                    g.Dispose();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Image Processing.FormatImageLines. Error in Bitmap calling" + e.Message);
                }
            }

            return bmp;

        }

        /// <summary>
        /// Convert 16bit image to bitmap only for a given line.
        /// </summary>
        /// <param name="OriginalBitmap"> Bitmap image </param>
        /// <param name="range"> range = {min, max} </param>
        /// <param name="AcqImg"> Acquired images </param>
        /// <param name="StartLine"></param>
        /// <param name="EndLine"></param>
        /// <returns></returns>
        public static Bitmap FormatImageLines(Bitmap OriginalBitmap, double[] range, UInt16[,] AcqImg, int StartLine, int EndLine)
        {
            if (AcqImg == null)
                return null;

            int height = AcqImg.GetLength(0);
            int width = AcqImg.GetLength(1);

            int bytePerPixel = 3;

            float MaxInt = (float)range[1];
            float MinInt = (float)range[0];

            int stride = width * bytePerPixel;
            byte[] pixels = new byte[width * height * bytePerPixel];

            for (int y = StartLine; y < EndLine; y++)
            {
                int val;
                for (int x = 0; x < width; x++)

                    for (int b = 0; b < bytePerPixel; b++)
                    {

                        if (b < 3)
                        {
                            if (MaxInt > MinInt)
                            {
                                val = (int)((AcqImg[y, x] - MinInt) * 255 / (MaxInt - MinInt));
                                if (val < 0)
                                    val = 0;
                                else if (val > 255)
                                    val = 255;
                            }
                            else
                            {
                                val = 0;
                            }
                            pixels[y * stride + x * bytePerPixel + b] = (byte)val;
                        }
                        else
                        {
                            pixels[y * stride + x * bytePerPixel + b] = 255; //Handle 32 bit.
                        }

                    }
            }
            //}
            //); //parallel for

            PixelFormat format = PixelFormat.Format24bppRgb;

            Bitmap bmp = new Bitmap(width, height, stride, format, GCHandle.Alloc(pixels, GCHandleType.Pinned).AddrOfPinnedObject());

            if (OriginalBitmap != null)
            {
                try
                {
                    Graphics g = Graphics.FromImage(OriginalBitmap);
                    g.DrawImageUnscaled(bmp, 0, (int)(StartLine)); //
                    g.Dispose();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Image Processing.FormatImageLines. Error in Bitmap calling" + e.Message);
                }
            }

            return bmp;

        }
        public static Bitmap MergeBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1 == null || bmp2 == null)
            {
                return null;
            }

            byte[] byteArray1 = BitmapToByteArray(bmp1);
            byte[] ByteArray2 = BitmapToByteArray(bmp2);

            int[] colorA = { 0, 2 }; //Magenta.
                                     //int[] colorA = { 0 }  //red;

            if (byteArray1.Length != ByteArray2.Length)
                return new Bitmap(bmp1);

            int width = bmp1.Width;
            int height = bmp1.Height;

            //var rangePartitioner = Partitioner.Create(0, height);
            for (int y = 0; y < height; y++)

            //Parallel.ForEach(rangePartitioner, range1 =>
            //{
            //for (int y = range1.Item1; y < range1.Item2; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    foreach (int k in colorA)
                    {
                        int pos = y * width * 3 + x * 3 + k;
                        byteArray1[pos] = ByteArray2[pos];
                    }
                }
            }
            //});

            Bitmap bmpNew = PixelsToBitmap(byteArray1, bmp1.Width, bmp1.Height);
            return bmpNew;
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = null;

            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }

        }

        public static T[][] MapImageInSquare<T>(T[][] AcqImg)
        {
            int width = AcqImg[0].Length;
            int height = AcqImg.Length;

            int nPixels = Math.Max(width, height);
            T[][] result = MatrixCalc.MatrixCreate2D<T>(nPixels, nPixels);
            int startX = (nPixels - width) / 2;
            int startY = (nPixels - height) / 2;

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    result[startY + y][startX + x] = AcqImg[y][x];
                }

            return result;
        }

        public static Bitmap FormatImage(double[] range, UInt16[][] AcqImg)
        {
            if (AcqImg == null)
                return null;

            int width = AcqImg[0].Length;
            int height = AcqImg.Length;

            Bitmap bmp1;

            if (AcqImg == null)
                return null;
            else
                bmp1 = FormatImageLines(null, range, AcqImg, 0, height);

            return bmp1;
        }


        /// <summary>
        /// Convert 16bit image to bitmap. It calculates the range automatically.
        /// </summary>
        /// <param name="AcqImg"></param>
        /// <returns></returns>
        public static Bitmap FormatImage(UInt16[,] AcqImg)
        {
            var maxV = MatrixCalc.calcMax<ushort>(AcqImg);
            var minV = MatrixCalc.calcMin<ushort>(AcqImg);
            return ImageProcessing.FormatImage(new double[] { minV, maxV }, AcqImg);
        }

        /// <summary>
        /// Convert 16bit image to bitmap.
        /// </summary>
        /// <param name="range">range = { min, max} </param>
        /// <param name="AcqImg">16 bit image </param>
        /// <returns></returns>
        public static Bitmap FormatImage(double[] range, UInt16[,] AcqImg)
        {
            if (AcqImg == null)
                return null;

            int height = AcqImg.GetLength(0);
            int width = AcqImg.GetLength(1);

            Bitmap bmp1;

            if (AcqImg == null)
                return null;
            else
                bmp1 = FormatImageLines(null, range, AcqImg, 0, height);

            return bmp1;
        }

        public static Bitmap PixelsToBitmap(byte[] pixels, int width, int height)
        {
            System.Drawing.Imaging.PixelFormat format;
            format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;

            int stride = width * 3;
            //Bitmap bmp = new Bitmap(width, height, format);

            Bitmap bmp = new Bitmap(width, height, format);

            bmp = new Bitmap(width, height, stride, format, GCHandle.Alloc(pixels, GCHandleType.Pinned).AddrOfPinnedObject());

            //Bitmap bmp1 = ResizeBitmap(bmp, targetWidth, targetHeight);
            return bmp;
        }

        public static Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            if (sourceBMP == null)
                return null;

            if (width > 0 && height > 0)
            {
                Bitmap result = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.DrawImage(sourceBMP, 0, 0, width, height);
                }
                return result;
            }
            else
                return null;
        }

        public static void ImShow(ushort[,] image_ushort)
        {
            var imshow = new Image_Showing_Window(image_ushort);
            imshow.Show();
            Application.DoEvents();
        }

        public static void ImShow(ushort[,] image_ushort1, ushort[,] image_ushort2)
        {
            var bitmap1 = FormatImage(image_ushort1);
            var bitmap2 = FormatImage(image_ushort2);
            var bitmap_merge = MergeBitmaps(bitmap1, bitmap2);

            var imshow = new Image_Showing_Window(bitmap_merge);
            imshow.Show();
            Application.DoEvents();
        }

        public static Image_Showing_Window Plot(double[] array_y)
        {
            double[] array_x = new double[array_y.Length];
            for (int i = 0; i < array_y.Length; i++)
                array_x[i] = i;

            var plot = new Image_Showing_Window(array_x, array_y);
            plot.Show();
            Application.DoEvents();

            return plot;
        }

        public static Image_Showing_Window Plot(double[] array_x, double[] array_y)
        {
            var plot = new Image_Showing_Window(array_x, array_y);
            plot.Show();
            Application.DoEvents();

            return plot;
        }

        //////////////////////////////////////////////////////////////////////////
        public static UInt16[,,] CreateFLIM_Sim(int n_dtime, int height, int width, double[] beta2,
            double pulseI, int n_stripe)
        {
            UInt16[,,] acqFImg = new ushort[height, width, n_dtime];
            
            var beta3 = (double[])beta2.Clone();
            beta3[0] = beta2[0] / 5;
            beta3[2] = beta2[2] / 5;
            //double[] beta0 = { 10, 1 / t_decay, tau_g / res, offset / res };
            //double[] beta2 = { 4, 1 / (2.6 / res), 6, 1 / (0.5 / res), tau_g / res, offset / res };
            Random rnd = new Random();
            var width_each = width / n_stripe;
            double[] t1 = Enumerable.Range(0, n_dtime).Select(x => (double)x).ToArray();
            double[] F0 = Exp2GaussArray(beta2, t1, pulseI);
            double[] F1 = Exp2GaussArray(beta3, t1, pulseI);
            int F_int;
            for (int y = 0; y < height; ++y)
                for (int n = 0; n < n_stripe; n++)
                {
                    for (int x1 = 0; x1 < width_each; ++x1)
                    {
                        int x = x1 + n * width_each;
                        for (int t = 0; t < n_dtime; ++t)
                        {
                            double F;
                            if (x1 < width_each / 2)
                                F = F0[t];
                            else
                                F = F1[t];
                            //Algorithm due to Donald Knuth, 1969.
                            double p = 1.0, L = Math.Exp(-F);
                            int k = 0;
                            do
                            {
                                k++;
                                p *= rnd.NextDouble();
                            }
                            while (p > L);
                            F_int = k - 1;

                            acqFImg[y, x, t] = (UInt16)F_int; //(F - 0.5);
                        }
                    }
                }
            return acqFImg;
        }


        public static double Exp2Gauss(double[] beta0, double x, double pulseI)
        {
            double y;
            double pop1 = beta0[0];
            double k1 = beta0[1];
            double pop2 = beta0[2];
            double k2 = beta0[3];
            double tauG = beta0[4];
            double t0 = beta0[5];

            double[] beta1 = { pop1, k1, tauG, t0 };
            double[] beta2 = { pop2, k2, tauG, t0 };

            y = ExpGauss(beta1, x, pulseI) + ExpGauss(beta2, x, pulseI);
            return y;
        }

        public static double[] Exp2GaussArray(double[] beta0, double[] x, double pulseI)
        {
            double[] y = new double[x.Length];

            for (int i = 0; i < x.Length; i++)
            {
                y[i] = Exp2Gauss(beta0, x[i], pulseI);
            }
            return y;
        }

        public static double ExpGauss(double[] beta0, double x, double pulseI)
        {
            double y;
            //double res = State.Spc.spcData.resolution[0]; //picoseconds
            //double pulseI = 1.0e12 / State.Spc.datainfo.syncRate[0] / res;

            double pop1 = beta0[0];
            double k1 = beta0[1];
            double tauG = beta0[2];
            double t0 = beta0[3];

            double[] beta1 = { pop1, k1, tauG, t0 };
            double[] beta2 = { pop1, k1, tauG, t0 - pulseI };

            y = MatrixCalc.ExpGauss(beta1, x) + MatrixCalc.ExpGauss(beta2, x);

            return y;
        }


        public static double[] ExpGaussArray(double[] beta0, double[] x, double pulseI)
        {
            double[] y = new double[x.Length];

            for (int i = 0; i < x.Length; i++)
            {
                y[i] = ExpGauss(beta0, x[i], pulseI);
            }
            return y;
        }

    }
}
