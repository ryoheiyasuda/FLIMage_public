using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra;

namespace MathLibrary
{
    public class MatrixCalc
    {
        static int simd_byte = System.Numerics.Vector<byte>.Count;
        static int simd_ushort = System.Numerics.Vector<ushort>.Count;
        static int simd_float = System.Numerics.Vector<float>.Count;
        static int simd_double = System.Numerics.Vector<double>.Count;
        static bool simd_on = System.Numerics.Vector.IsHardwareAccelerated;
        static public bool IntelMKL_on = MathNet.Numerics.Control.TryUseNativeMKL();

        public static byte[] changeDepthFrom16To8(ushort[,,] array3d)
        {
            var linearArray = LinearizeArray<ushort>(array3d);
            return changeDepthFrom16To8(linearArray);
        }

        public static byte[] changeDepthFrom16To8(ushort[] linearArray)
        {
            int n = linearArray.Length;
            byte[] bArray = new byte[n];
            for (int i = 0; i < n; i++)
                bArray[i] = (byte)linearArray[i];
            return bArray;
        }

        public static ushort[] changeDepthFrom8To16(byte[] byteArray)
        {
            int n = byteArray.Length;
            ushort[] ushortArray = new ushort[n];
            for (int i = 0; i < n; i++)
                ushortArray[i] = (ushort)byteArray[i];
            return ushortArray;
        }

        /// <summary>
        /// Calculate max of 3D array.
        /// </summary>
        /// <param name="matrix3d"></param>
        /// <returns></returns>
        public static double calcMax<T>(T[,,] matrix3d)
            where T : struct
        {
            var linArray = LinearizeArray<T>(matrix3d);
            return calcMax(linArray);
        }

        /// <summary>
        /// Calculate max of 2D array.
        /// </summary>
        /// <param name="matrix2d"></param>
        /// where T:struct
        /// <returns></returns>
        public static double calcMax<T>(T[,] matrix2d)
             where T : struct
        {
            var linArray = LinearizeArray<T>(matrix2d);
            return calcMax(linArray);
        }

        /// <summary>
        /// Calculate max of linear array.
        /// </summary>
        /// <param name="linearArray"></param>
        /// <returns></returns>
        public static double calcMax<T>(T[] linearArray)
            where T : struct
        {
            if (!simd_on)
                return calcMax_Normal(linearArray);
            else
                return calcMax_SIMD(linearArray);

        }

        /// <summary>
        /// Calculate min of 2D array.
        /// </summary>
        /// <param name="matrix2d"></param>
        /// where T:struct
        /// <returns></returns>
        public static double calcMin<T>(T[,] matrix2d)
             where T : struct
        {
            var linArray = LinearizeArray<T>(matrix2d);
            return calcMin(linArray);
        }

        /// <summary>
        /// Calculate min of linear array.
        /// </summary>
        /// <param name="linearArray"></param>
        /// <returns></returns>
        public static double calcMin<T>(T[] linearArray)
            where T : struct
        {
            if (!simd_on)
                return calcMin_Normal(linearArray);
            else
                return calcMin_SIMD(linearArray);
        }


        public static double calcMax_Normal<T>(T[] linearArray)
            where T : struct
        {
            return Convert.ToDouble(linearArray.Max());
        }

        public static double calcMin_Normal<T>(T[] linearArray)
            where T : struct
        {
            return Convert.ToDouble(linearArray.Min());
        }

        private static double calcMin_SIMD<T>(T[] linearArray)
            where T : struct
        {
            int siz = Marshal.SizeOf(typeof(T));
            var n_simd = simd_byte / siz;

            int i = 0;
            var minVec = new System.Numerics.Vector<T>(linearArray, 0);
            for (i = n_simd; i <= linearArray.Length - n_simd; i += n_simd)
            {
                var vec = new System.Numerics.Vector<T>(linearArray, i);
                minVec = System.Numerics.Vector.Min(minVec, vec);
            }

            var minArray = new T[n_simd];
            minVec.CopyTo(minArray);
            dynamic minValue = minArray.Min();

            int remainingLength = linearArray.Length - i - 1;
            T[] restArray;
            if (remainingLength > 0)
            {
                restArray = new T[remainingLength];
                Array.Copy(linearArray, i, restArray, 0, restArray.Length);
                var min2 = restArray.Min();
                if (minValue > min2)
                    minValue = min2;
            }
            return minValue;
        }

        private static double calcMax_SIMD<T>(T[] linearArray)
            where T : struct
        {
            int siz = Marshal.SizeOf(typeof(T));
            var n_simd = simd_byte / siz;
            var maxArray = new T[n_simd];

            int i = 0;
            var maxVec = new System.Numerics.Vector<T>(linearArray, 0);
            for (i = n_simd; i <= linearArray.Length - n_simd; i += n_simd)
            {
                var vec = new System.Numerics.Vector<T>(linearArray, i);
                maxVec = System.Numerics.Vector.Max(maxVec, vec);

                ////DEBUG only.
                //maxVec.CopyTo(maxArray);
                //if (Convert.ToInt32(maxArray.Max()) > 30)
                //{
                //    Debug.WriteLine("Here!");
                //}
            }

            maxVec.CopyTo(maxArray);
            double maxValue = calcMax_Normal(maxArray);

            int remainingLength = linearArray.Length - i - 1;
            T[] restArray;
            if (remainingLength > 0)
            {
                restArray = new T[remainingLength];
                Array.Copy(linearArray, i, restArray, 0, restArray.Length);
                var max2 = calcMax_Normal(restArray);
                if (maxValue < max2)
                    maxValue = max2;
            }
            return maxValue;
        }

        public static double calcSum_Normal<T>(T[] linearArray)
            where T : struct
        {
            double sum = 0;
            for (int i = 0; i < linearArray.Length; i++)
                sum += Convert.ToDouble(linearArray[i]);
            return sum;
        }

        public static double calcSum<T>(T[] linearArray)
            where T : struct
        {
            if (!simd_on)
                return calcSum_Normal(linearArray);
            else
                return calcSum_SIMD(linearArray);
        }

        public static float[] convertToFloat<T>(T[] linearArray)
            where T : struct
        {
            int len = linearArray.Length;
            var linearArray_F = new float[len];
            for (int k = 0; k < len; k++)
                linearArray_F[k] = Convert.ToSingle(linearArray[k]);

            return linearArray_F;

            //if (simd_on) //NOT Faster...
            //    return convertToFloatSIMD(linearArray); 
            //else
            //return convertToFloat_Normal(linearArray);
        }

        public static float[] convertToFloat_Normal<T>(T[] linearArray)
      where T : struct
        {
            int len = linearArray.Length;
            var linearArray_F = new float[len];
            for (int k = 0; k < len; k++)
                linearArray_F[k] = Convert.ToSingle(linearArray[k]);

            return linearArray_F;
        }

        /// <summary>
        /// Not Faster. Obsolete...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="linearArray"></param>
        /// <returns></returns>
        public static float[] convertToFloatSIMD<T>(T[] linearArray)
        where T : struct
        {
            int len = linearArray.Length;
            int siz = Marshal.SizeOf(typeof(T));
            var linearArray_F = new float[len];
            var n_simd = simd_byte / siz;

            object obArray = (object)linearArray;

            if (typeof(T) == typeof(ushort))
            {
                ushort[] linearArray_U = (ushort[])obArray;
                int k = 0;
                for (k = 0; k <= len - n_simd; k += n_simd)
                {
                    var vec = new System.Numerics.Vector<ushort>(linearArray_U, k);
                    System.Numerics.Vector.Widen(vec, out var vecA, out var vecB);
                    System.Numerics.Vector.ConvertToSingle(vecA).CopyTo(linearArray_F, k);
                    System.Numerics.Vector.ConvertToSingle(vecB).CopyTo(linearArray_F, k + simd_float);
                }
                for (; k < len; k++)
                    linearArray_F[k] = (float)linearArray_U[k];
            }
            else if (typeof(T) == typeof(short))
            {
                var linearArray_U = (short[])obArray;
                int k = 0;
                for (k = 0; k <= len - n_simd; k += n_simd)
                {
                    var vec = new System.Numerics.Vector<short>(linearArray_U, k);
                    System.Numerics.Vector.Widen(vec, out var vecA, out var vecB);
                    System.Numerics.Vector.ConvertToSingle(vecA).CopyTo(linearArray_F, k);
                    System.Numerics.Vector.ConvertToSingle(vecB).CopyTo(linearArray_F, k + simd_float);
                }
                for (; k < len; k++)
                    linearArray_F[k] = (float)linearArray_U[k];
            }
            else if (typeof(T) == typeof(int))
            {
                var linearArray_I = (int[])obArray;
                int k = 0;
                for (k = 0; k <= len - n_simd; k += n_simd)
                {
                    var vec = new System.Numerics.Vector<int>(linearArray_I, k);
                    System.Numerics.Vector.ConvertToSingle(vec).CopyTo(linearArray_F, k);
                }
                for (; k < len; k++)
                    linearArray_F[k] = (float)linearArray_I[k];
            }
            else if (typeof(T) == typeof(uint))
            {
                var linearArray_I = (uint[])obArray;
                int k = 0;
                for (k = 0; k <= len - n_simd; k += n_simd)
                {
                    var vec = new System.Numerics.Vector<uint>(linearArray_I, k);
                    System.Numerics.Vector.ConvertToSingle(vec).CopyTo(linearArray_F, k);
                }
                for (; k < len; k++)
                    linearArray_F[k] = (float)linearArray_I[k];
            }
            else if (typeof(T) == typeof(float))
            {
                linearArray_F = (float[])obArray;
            }
            else if (typeof(T) == typeof(double))
            {
                var linearArray_D = (double[])obArray;
                int k = 0;
                for (k = 0; k <= len - simd_float; k += simd_float)
                {
                    var vec1 = new System.Numerics.Vector<double>(linearArray_D, k);
                    var vec2 = new System.Numerics.Vector<double>(linearArray_D, k + simd_double);
                    System.Numerics.Vector.Narrow(vec1, vec2).CopyTo(linearArray_F, k);
                }
                for (; k < len; k++)
                    linearArray_F[k] = (float)linearArray_D[k];
            }
            else
            {
                for (int k = 0; k < len; k++)
                    linearArray_F[k] = Convert.ToSingle(linearArray[k]);
            }

            return linearArray_F;
        }

        public static double[] convertToDouble_Normal<T>(T[] linearArray)
            where T : struct
        {
            int len = linearArray.Length;
            var linearArray_D = new double[len];
            for (int k = 0; k < len; k++)
                linearArray_D[k] = Convert.ToDouble(linearArray[k]);

            return linearArray_D;
        }


        public static double[] convertToDouble_SIMD<T>(T[] linearArray)
            where T : struct
        {
            int len = linearArray.Length;
            int siz = Marshal.SizeOf(typeof(T));
            var linearArray_D = new double[len];
            var n_simd = simd_byte / siz;

            object obArray = (object)linearArray;

            if (typeof(T) == typeof(ushort))
            {
                ushort[] linearArray_U = (ushort[])obArray;
                int k = 0;
                for (k = 0; k <= len - n_simd; k += n_simd)
                {
                    var vec = new System.Numerics.Vector<ushort>(linearArray_U, k);
                    System.Numerics.Vector.Widen(vec, out var vecA, out var vecB);
                    System.Numerics.Vector.Widen(vecA, out var vecA1, out var vecA2);
                    System.Numerics.Vector.Widen(vecB, out var vecB1, out var vecB2);
                    System.Numerics.Vector.ConvertToDouble(vecA1).CopyTo(linearArray_D, k);
                    System.Numerics.Vector.ConvertToDouble(vecA2).CopyTo(linearArray_D, k + simd_double);
                    System.Numerics.Vector.ConvertToDouble(vecB1).CopyTo(linearArray_D, k + simd_double * 2);
                    System.Numerics.Vector.ConvertToDouble(vecB2).CopyTo(linearArray_D, k + simd_double * 3);
                }
                for (; k < len; k++)
                    linearArray_D[k] = (double)linearArray_U[k];
            }
            else if (typeof(T) == typeof(short))
            {
                short[] linearArray_U = (short[])obArray;
                int k = 0;
                for (k = 0; k <= len - n_simd; k += n_simd)
                {
                    var vec = new System.Numerics.Vector<short>(linearArray_U, k);
                    System.Numerics.Vector.Widen(vec, out var vecA, out var vecB);
                    System.Numerics.Vector.Widen(vecA, out var vecA1, out var vecA2);
                    System.Numerics.Vector.Widen(vecB, out var vecB1, out var vecB2);
                    System.Numerics.Vector.ConvertToDouble(vecA1).CopyTo(linearArray_D, k);
                    System.Numerics.Vector.ConvertToDouble(vecA2).CopyTo(linearArray_D, k + simd_double);
                    System.Numerics.Vector.ConvertToDouble(vecB1).CopyTo(linearArray_D, k + simd_double * 2);
                    System.Numerics.Vector.ConvertToDouble(vecB2).CopyTo(linearArray_D, k + simd_double * 3);
                }
                for (; k < len; k++)
                    linearArray_D[k] = (double)linearArray_U[k];
            }
            else if (typeof(T) == typeof(int))
            {
                var linearArray_F = (int[])obArray;
                int k = 0;
                for (k = 0; k <= len - n_simd; k += n_simd)
                {
                    var vec = new System.Numerics.Vector<int>(linearArray_F, k);
                    System.Numerics.Vector.Widen(vec, out var vecA, out var vecB);
                    var vecAD = System.Numerics.Vector.ConvertToDouble(vecA);
                    var vecBD = System.Numerics.Vector.ConvertToDouble(vecB);
                    vecAD.CopyTo(linearArray_D, k);
                    vecBD.CopyTo(linearArray_D, k + simd_double);
                }
                for (; k < len; k++)
                    linearArray_D[k] = (double)linearArray_F[k];
            }
            else if (typeof(T) == typeof(uint))
            {
                var linearArray_F = (uint[])obArray;
                int k = 0;
                for (k = 0; k <= len - n_simd; k += n_simd)
                {
                    var vec = new System.Numerics.Vector<uint>(linearArray_F, k);
                    System.Numerics.Vector.Widen(vec, out var vecA, out var vecB);
                    var vecAD = System.Numerics.Vector.ConvertToDouble(vecA);
                    var vecBD = System.Numerics.Vector.ConvertToDouble(vecB);
                    vecAD.CopyTo(linearArray_D, k);
                    vecBD.CopyTo(linearArray_D, k + simd_double);
                }
                for (; k < len; k++)
                    linearArray_D[k] = (double)linearArray_F[k];
            }
            else if (typeof(T) == typeof(float))
            {
                var linearArray_F = (float[])obArray;
                int k = 0;
                for (k = 0; k <= len - n_simd; k += n_simd)
                {
                    var vec = new System.Numerics.Vector<float>(linearArray_F, k);
                    System.Numerics.Vector.Widen(vec, out var vecA, out var vecB);
                    vecA.CopyTo(linearArray_D, k);
                    vecB.CopyTo(linearArray_D, k + simd_double);
                }
                for (; k < len; k++)
                    linearArray_D[k] = (double)linearArray_F[k];
            }
            else if (typeof(T) == typeof(double))
            {
                linearArray_D = (double[])obArray;
            }
            else
            {
                for (int k = 0; k < len; k++)
                    linearArray_D[k] = Convert.ToDouble(linearArray[k]);
            }

            return linearArray_D;
        }

        /// <summary>
        /// Calculate sum of linear array of ushort using SIMD acceleration.
        /// </summary>
        /// <param name="linearArray"></param>
        /// <returns></returns>
        private static double calcSum_SIMD<T>(T[] linearArray)
           where T : struct
        {
            int siz = Marshal.SizeOf(typeof(T));
            int len = linearArray.Length;
            System.Numerics.Vector<double> sumVec = System.Numerics.Vector<double>.Zero;

            double sumValue = 0;

            if (typeof(T) == typeof(ushort))
            {
                int i = 0;
                ushort[] linearArrayU = (ushort[])(object)linearArray;
                for (i = 0; i <= len - simd_ushort; i += simd_ushort)
                {
                    var vecU = new System.Numerics.Vector<ushort>(linearArrayU, i);
                    System.Numerics.Vector<float> ua_f, ub_f;
                    ConvertFromUshortVectorToSingleVector(vecU, out ua_f, out ub_f);
                    System.Numerics.Vector.Widen(ua_f, out System.Numerics.Vector<double> ua_f1, out System.Numerics.Vector<double> ua_f2);
                    System.Numerics.Vector.Widen(ub_f, out System.Numerics.Vector<double> ub_f1, out System.Numerics.Vector<double> ub_f2);
                    sumVec = System.Numerics.Vector.Add(sumVec, ua_f1);
                    sumVec = System.Numerics.Vector.Add(sumVec, ua_f2);
                    sumVec = System.Numerics.Vector.Add(sumVec, ub_f1);
                    sumVec = System.Numerics.Vector.Add(sumVec, ub_f2);
                }
                for (; i < linearArray.Length; i++)
                    sumValue += (double)linearArrayU[i];
            }
            else if (typeof(T) == typeof(uint))
            {
                int i = 0;
                uint[] linearArrayI = (uint[])(object)linearArray;
                for (i = 0; i <= len - simd_float; i += simd_float)
                {
                    var vecI = new System.Numerics.Vector<uint>(linearArrayI, i);
                    System.Numerics.Vector<float> vecF = System.Numerics.Vector.ConvertToSingle(vecI);
                    System.Numerics.Vector.Widen(vecF, out System.Numerics.Vector<double> ua, out System.Numerics.Vector<double> ub);
                    sumVec = System.Numerics.Vector.Add(sumVec, ua);
                    sumVec = System.Numerics.Vector.Add(sumVec, ub);
                }
                for (; i < linearArray.Length; i++)
                    sumValue += (double)linearArrayI[i];
            }
            else if (typeof(T) == typeof(int))
            {
                int i = 0;
                int[] linearArrayI = (int[])(object)linearArray;
                for (i = 0; i <= len - simd_float; i += simd_float)
                {
                    var vecI = new System.Numerics.Vector<int>(linearArrayI, i);
                    System.Numerics.Vector<float> vecF = System.Numerics.Vector.ConvertToSingle(vecI);
                    System.Numerics.Vector.Widen(vecF, out System.Numerics.Vector<double> ua, out System.Numerics.Vector<double> ub);
                    sumVec = System.Numerics.Vector.Add(sumVec, ua);
                    sumVec = System.Numerics.Vector.Add(sumVec, ub);
                }
                for (; i < linearArray.Length; i++)
                    sumValue += (double)linearArrayI[i];
            }
            else if (typeof(T) == typeof(float))
            {
                int i = 0;
                float[] linearArrayF = (float[])(object)linearArray;
                for (i = 0; i <= len - simd_float; i += simd_float)
                {
                    var vecF = new System.Numerics.Vector<float>(linearArrayF, i);
                    System.Numerics.Vector.Widen(vecF, out System.Numerics.Vector<double> ua, out System.Numerics.Vector<double> ub);
                    sumVec = System.Numerics.Vector.Add(sumVec, ua);
                    sumVec = System.Numerics.Vector.Add(sumVec, ub);
                }

                for (; i < linearArray.Length; i++)
                    sumValue += (double)linearArrayF[i];
            }
            else if (typeof(T) == typeof(double))
            {
                int i = 0;
                double[] linearArrayD = (double[])(object)linearArray;
                for (i = 0; i <= len - simd_double; i += simd_double)
                {
                    var vecF = new System.Numerics.Vector<double>(linearArrayD, i);
                    sumVec = System.Numerics.Vector.Add(sumVec, vecF);
                }

                for (; i < linearArray.Length; i++)
                    sumValue += linearArrayD[i];
            }
            else
            {
                var linearArray_D = convertToDouble_Normal(linearArray);

                int i = 0;
                sumVec = new System.Numerics.Vector<double>(linearArray_D, 0);
                for (i = simd_double; i <= len - simd_double; i += simd_double)
                {
                    var vec = new System.Numerics.Vector<double>(linearArray_D, i);
                    sumVec = System.Numerics.Vector.Add(sumVec, vec);
                }

                for (; i < linearArray.Length; i++)
                    sumValue += linearArray_D[i];
            }

            var sumArray = new double[simd_double];
            sumVec.CopyTo(sumArray);

            for (int j = 0; j < sumArray.Length; j++)
                sumValue += sumArray[j];

            return sumValue;
        }

        /// <summary>
        /// Calculate sum of linear array of ushort using SIMD acceleration.
        /// </summary>
        /// <param name="linearArray"></param>
        /// <returns></returns>
        private static double calcSum_SIMD(double[] linearArray)
        {
            int i = 0;
            var sumVec = System.Numerics.Vector<double>.Zero;
            for (i = 0; i <= linearArray.Length - simd_double; i += simd_double)
            {
                var vec = new System.Numerics.Vector<double>(linearArray, i);
                sumVec = System.Numerics.Vector.Add(sumVec, vec);
            }

            var sumArray = new double[simd_double];
            sumVec.CopyTo(sumArray);
            double sumValue = 0.0;
            for (int j = 0; j < sumArray.Length; j++)
                sumValue += sumArray[j];

            for (; i < linearArray.Length; i++)
                sumValue += linearArray[i];

            return sumValue;
        }

        static public double ArrayDotProduct_Normal<T>(T[] VectorA, T[] VectorB)
            where T : struct
        {
            int n0 = VectorA.Length;
            int n1 = VectorB.Length;
            if (n0 != n1)
                return (double)0;

            double sum = 0;
            for (int i = 0; i < n0; i++)
                sum += Convert.ToDouble(VectorA[i]) * Convert.ToDouble(VectorB[i]);

            return sum;
        }

        static public double ArrayDotProduct<T>(T[] VectorA, T[] VectorB)
            where T : struct
        {
            if (simd_on)
                return SIMD_Dot(VectorA, VectorB);
            else
                return ArrayDotProduct_Normal(VectorA, VectorB);
        }

        static public double ArrayDotProduct_SIMD_double(double[] VectorA, double[] VectorB)
        {
            int n0 = VectorA.Length;
            int n1 = VectorB.Length;
            if (n0 != n1)
                return double.NaN;

            double sum = 0;
            int i = 0;
            for (i = 0; i <= n0 - simd_double; i += simd_double)
            {
                var VecA = new System.Numerics.Vector<double>(VectorA, i);
                var VecB = new System.Numerics.Vector<double>(VectorB, i);
                sum += Vector.Dot(VecA, VecB);
            }

            for (; i < n0; i++)
                sum += VectorA[i] * VectorB[i];

            return sum;

        }

        /// <summary>
        /// The function is for pockels cell calibration.
        /// It calculates from voltage to percentage.
        /// </summary>
        /// <param name="beta0"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public double InverseSinusoidal(double[] beta0, double y)
        {
            double contrast = beta0[0] / beta0[2];
            double y1 = (y / 100.0 - contrast) / (1 - contrast);
            if (y1 <= 0)
                return beta0[3];
            if (y >= 100)
                return beta0[3] + Math.PI / beta0[1];
            double x1 = Math.Acos(1 - y1 * 2.0);
            return x1 / beta0[1] + beta0[3];
        }

        /// <summary>
        /// The function is for pockels cell calibration. 
        /// This is for fitting voltage - light relationship.
        /// </summary>
        /// <param name="beta0"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        static public double[] Sinusoidal(double[] beta0, double[] x)
        {
            double[] y = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                y[i] = beta0[2] * (1.0 - Math.Cos(beta0[1] * (x[i] - beta0[3]))) / 2.0 + beta0[0];
            }
            return y;
        }

        /// <summary>
        /// This is a gaussian function --- for fitting. 
        /// </summary>
        /// <param name="beta0"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        static public double[] Gaussian(double[] beta0, double[] x)
        {
            double[] y = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                y[i] = beta0[0] * Math.Exp(-Math.Pow(x[i] - beta0[1], 2) / 2.0 / beta0[2]) + beta0[3];
            }
            return y;
        }

        /// <summary>
        /// This is a gaussian function --- for fitting. 
        /// </summary>
        /// <param name="beta0"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        static public double[] Gaussian_NoOffset(double[] beta0, double[] x)
        {
            double[] y = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                y[i] = beta0[0] * Math.Exp(-Math.Pow(x[i] - beta0[1], 2) / 2.0 / beta0[2]);
            }
            return y;
        }

        /// <summary>
        /// Linear regresion by y = a * x + b. Returns [a, b].
        /// Using MathNet protocol, rather than my own protocol.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double[] linearRegression(double[] xdata, double[] ydata)
        {
            Tuple<double, double> p = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(xdata, ydata);
            double a = p.Item1; // == 10; intercept
            double b = p.Item2; // == 0.5; slope

            return new double[] { a, b };
        }

        /// <summary>
        /// Return peak index, peak value, sigma, baseline of Gaussian fitting. 
        /// return y = beta0[0] * Math.Exp(- Math.Pow(x[i] - beta0[1], 2) / 2.0 /beta0[2]);
        /// beta0
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double[] FindPeak_WithGaussianFit1D_NoOffset(double[] y)
        {
            if (y.Length < 3) //For gaussian fitting, at least 3 param is required.
                return null;

            double maxValue = y.Max();
            int maxInput = y.ToList().IndexOf(maxValue);
            double minValue = 0;
            //int minInput = y.ToList().IndexOf(minValue);

            double halfWidth = 0;
            //Go down first to find a half point.
            for (int i = maxInput; i >= 0; i--)
                if (y[i] < (maxValue - minValue) / 2.0 + minValue)
                {
                    halfWidth = (double)(-i + maxInput);
                    break;
                }

            //If it could not find, further looking at half point (maybe edge).
            if (halfWidth == 0)
            {
                for (int i = maxInput + 1; i < y.Length; i++)
                    if (y[i] < (maxValue - minValue) / 2.0 + minValue)
                    {
                        halfWidth = (double)(i - maxInput);
                        break;
                    }
            }

            if (halfWidth == 0)
                halfWidth = (double)y.Length / 2.0;

            double[] beta0 = new double[3];
            beta0[0] = maxValue - minValue;
            beta0[1] = maxInput;
            beta0[2] = Math.Pow(halfWidth, 2);

            var x = new double[y.Length];
            for (int i = 0; i < y.Length; i++)
                x[i] = i;

            Fitting.Nlinfit fit = new Fitting.Nlinfit(beta0, x, y);
            fit.modelFunc = ((betaA, xA) => MatrixCalc.Gaussian_NoOffset(betaA, xA));
            // y = beta0[0] * Math.Exp(- Math.Pow(x[i] - beta0[1], 2) / 2.0 /beta0[2]);

            fit.Perform();
            return fit.beta;
        }


        /// <summary>
        /// Return peak index, peak value, sigma, baseline of Gaussian fitting. 
        /// return y = beta0[0] * Math.Exp(- Math.Pow(x[i] - beta0[1], 2) / 2.0 /beta0[2]) + beta0[3];
        /// beta0
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double[] FindPeak_WithGaussianFit1D(double[] y)
        {
            if (y.Length < 3) //For gaussian fitting, at least 3 param is required.
                return null;

            double maxValue = y.Max();
            int maxInput = y.ToList().IndexOf(maxValue);
            double minValue = y.Min();
            int minInput = y.ToList().IndexOf(minValue);

            double halfWidth = 0;
            //Go down first to find a half point.
            for (int i = maxInput; i >= 0; i--)
                if (y[i] < (maxValue - minValue) / 2.0 + minValue)
                {
                    halfWidth = (double)(-i + maxInput);
                    break;
                }

            //If it could not find, further looking at half point (maybe edge).
            if (halfWidth == 0)
            {
                for (int i = maxInput + 1; i < y.Length; i++)
                    if (y[i] < (maxValue - minValue) / 2.0 + minValue)
                    {
                        halfWidth = (double)(i - maxInput);
                        break;
                    }
            }

            double[] beta0 = new double[4];
            beta0[0] = maxValue - minValue;
            beta0[1] = maxInput;
            beta0[2] = Math.Pow(halfWidth, 2);
            beta0[3] = minValue;

            var x = new double[y.Length];
            for (int i = 0; i < y.Length; i++)
                x[i] = i;

            Fitting.Nlinfit fit = new Fitting.Nlinfit(beta0, x, y);
            fit.modelFunc = ((betaA, xA) => MatrixCalc.Gaussian(betaA, xA));
            // y = beta0[0] * Math.Exp(- Math.Pow(x[i] - beta0[1], 2) / 2.0 /beta0[2]) + beta0[3];

            fit.Perform();
            return fit.beta;
        }



        /// <summary>
        /// Convolution of matrixA and matrixB with brute force.
        /// </summary>
        /// <param name="matrixA"></param>
        /// <param name="matrixB"></param>
        /// <returns></returns>
        public static Matrix<double> MatrixConvolution(ushort[][] matrixA, Matrix<double> matB)
        {
            int heightA = matrixA.Length;
            int widthA = matrixA[0].Length;
            int heightB = matB.ColumnCount;
            int widthB = matB.RowCount;

            var matA = ConvertToDoubleMatrix(matrixA);
            var result = Matrix<double>.Build.Dense(widthA, heightA);

            for (int yA = 0; yA < widthA; yA++)
                for (int xA = 0; xA < widthA; xA++)
                {
                    int startX = Math.Max(xA - widthB / 2, 0);
                    int endX = Math.Min(xA + widthB / 2, widthA); //exclusive.
                    int countX = endX - startX;

                    int startY = Math.Max(yA - heightB / 2, 0);
                    int endY = Math.Min(yA + heightB / 2, heightA); //exclusive.
                    int countY = endY - startY;

                    var matA_s = matA.SubMatrix(startX, countX, startY, countY);

                    int startXB = startX - (xA - widthB / 2);
                    //int endXB = widthB + endX - (xA + widthB / 2);
                    //int countXB = endXB - startXB;

                    int startYB = startY - (yA - heightB / 2);
                    //int endYB = widthB + endY - (yA + heightB / 2);
                    //int countYB = endYB - startYB;

                    var matB_s = matB.SubMatrix(startXB, countX, startYB, countY);

                    var matC = matA_s.PointwiseMultiply(matB_s);

                    result[xA, yA] = MatrixSum_Matrix(matC);
                }

            return result;
        }


        /// <summary>
        /// Convolution of matrixA and matrixB by FFT.
        /// </summary>
        /// <param name="matrixA"></param>
        /// <param name="matrixB"></param>
        /// <returns></returns>
        public static Matrix<double> MatrixConvolution__FFT(ushort[][] matrixA, Matrix<double> matrixB)
        {
            var matA = ConvertToComplexMatrix(matrixA);
            var matB = ConvertToComplexMatrix(matrixB);
            FFT2DForward(ref matA);
            FFT2DForward(ref matB);
            var matC = matA.PointwiseMultiply(matB);
            FFT2DInverse(ref matC);
            return matC.Real();
        }
        public static Matrix<double> MatrixConvolution__FFT(ushort[,] matrixA, Matrix<double> matrixB)
        {
            var matA = ConvertToComplexMatrix(matrixA);
            var matB = ConvertToComplexMatrix(matrixB);
            FFT2DForward(ref matA);
            FFT2DForward(ref matB);
            var matC = matA.PointwiseMultiply(matB);
            FFT2DInverse(ref matC);
            return matC.Real();
        }

        /// <summary>
        /// Making new 5D stack in multi-dimmensional matrix.
        /// </summary>
        /// <param name="n_c"></param>
        /// <param name="n_z"></param>
        /// <param name="n_y"></param>
        /// <param name="n_x"></param>
        /// <param name="n_t"></param>
        /// <returns></returns>
        public static ushort[][][,,] makeNew5DSlice(int n_c, int n_z, int n_y, int n_x, int[] n_t)
        {
            ushort[][][,,] outMatrix = new UInt16[n_c][][,,];

            for (int c = 0; c < n_c; c++)
            {
                if (n_t[c] == 0)
                    outMatrix[c] = null;
                else
                {
                    outMatrix[c] = new UInt16[n_z][,,];
                    for (int z = 0; z < n_z; z++)
                        outMatrix[c][z] = new UInt16[n_y, n_x, n_t[c]];
                }
            }

            return outMatrix;
        }


        /// <summary>
        /// Making new 5D stack.
        /// </summary>
        /// <param name="n_c"></param>
        /// <param name="n_z"></param>
        /// <param name="n_y"></param>
        /// <param name="n_x"></param>
        /// <param name="n_t"></param>
        /// <returns></returns>
        public static ushort[][][] makeNew5DSlice_linear(int n_c, int n_z, int n_y, int n_x, int[] n_t)
        {
            ushort[][][] outMatrix = new UInt16[n_c][][];

            for (int c = 0; c < n_c; c++)
            {
                if (n_t[c] == 0)
                    outMatrix[c] = null;
                else
                {
                    outMatrix[c] = new UInt16[n_z][]; // MatrixCreate4D<ushort>(n_z, n_y, n_x, n_t[c]);
                    for (int z = 0; z < n_z; z++)
                        outMatrix[c][z] = new UInt16[n_y * n_x * n_t[c]];
                }
            }

            return outMatrix;
        }

        /// <summary>
        /// Intel's version of Inverse 2DFFT combined with
        /// Ryohei's version of Forward 2DFFT. Doing row 1d FFT followed by column 1d FFT.
        /// </summary>
        /// <param name="data2D"></param>
        /// <returns></returns>
        public static void FFT2DForward(ref Matrix<Complex> data2D)
        {
            if (IntelMKL_on)
            {
                Fourier.Forward2D(data2D);
            }
            else
            {
                var dataC = data2D.ToRowArrays();

                for (int i = 0; i < dataC.Length; i++)
                    Fourier.Forward(dataC[i]);

                var dataC1 = Matrix<Complex>.Build.DenseOfRowArrays(dataC).ToColumnArrays();

                for (int i = 0; i < dataC1.Length; i++)
                    Fourier.Forward(dataC1[i]);

                data2D = Matrix<Complex>.Build.DenseOfColumnArrays(dataC1);
            }
        }

        /// <summary>
        /// Intel's version of Inverse 2DFFT combined with
        /// Ryohei's version of Inverse 2DFFT. Doing row iFFT followed by column iFFT. 
        /// </summary>
        /// <param name="data2D"></param>
        /// <returns></returns>
        public static void FFT2DInverse(ref Matrix<Complex> data2D)
        {
            if (IntelMKL_on)
            {
                Fourier.Inverse2D(data2D);
            }
            else
            {
                var dataC = data2D.ToRowArrays();

                for (int i = 0; i < dataC.Length; i++)
                    Fourier.Inverse(dataC[i]);

                var dataC1 = Matrix<Complex>.Build.DenseOfRowArrays(dataC).ToColumnArrays();

                for (int i = 0; i < dataC1.Length; i++)
                    Fourier.Inverse(dataC1[i]);

                data2D = Matrix<Complex>.Build.DenseOfColumnArrays(dataC1);
            }
        }

        /// <summary>
        /// Similarity based on (Iij * Jij)^2 / (Iij^2 Jij^2)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrixA"></param>
        /// <param name="matrixB"></param>
        /// <returns></returns>
        public static double Matrix_similarity<T>(T[][] matrixA, T[][] matrixB)
            where T : struct
        {
            int height = matrixA.Length;
            int width = matrixA[0].Length;

            int height2 = matrixB.Length;
            int width2 = matrixB[0].Length;

            if (height != height2 || width != width2)
                return 0.0;

            var matA = MatrixCalc.ConvertToDoubleMatrix(matrixA);
            var matA2 = matA.PointwisePower(2);
            var matB = MatrixCalc.ConvertToDoubleMatrix(matrixB);
            var matB2 = matB.PointwisePower(2);

            var matAB = matA.PointwiseMultiply(matB);
            var matAB2 = matAB.PointwisePower(2);

            double similarity = MatrixSum_Matrix(matAB2) / MatrixSum_Matrix(matA2) / MatrixSum_Matrix(matB2);

            return similarity;
        }

        public static double Matrix_similarity<T>(T[,] matrixA, T[,] matrixB)
          where T : struct
        {
            int height = matrixA.GetLength(0);
            int width = matrixA.GetLength(1);

            int height2 = matrixB.GetLength(0);
            int width2 = matrixB.GetLength(1);

            if (height != height2 || width != width2)
                return 0.0;

            var matA = MatrixCalc.ConvertToDoubleMatrix(matrixA);
            var matA2 = matA.PointwisePower(2);
            var matB = MatrixCalc.ConvertToDoubleMatrix(matrixB);
            var matB2 = matB.PointwisePower(2);

            var matAB = matA.PointwiseMultiply(matB);
            var matAB2 = matAB.PointwisePower(2);

            double similarity = MatrixSum_Matrix(matAB2) / MatrixSum_Matrix(matA2) / MatrixSum_Matrix(matB2);

            return similarity;
        }
        /// <summary>
        /// Convert Matrix to linear Vector.
        /// </summary>
        /// <param name="matrixA"></param>
        /// <returns></returns>
        public static MathNet.Numerics.LinearAlgebra.Vector<double> Matrix2Vector(Matrix<double> matrixA)
        {
            return MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(matrixA.Enumerate().ToArray());
        }

        public static T[] Matrix2Vector<T>(T[,] matrixA)
        {
            return matrixA.Cast<T>().ToArray();
        }

        /// <summary>
        /// This returns cross correlation of two vectors. Output length is twice longer, and from -length to +length.
        /// 0 shift correlation can be obtained as ans = xcorr(x1, x2); ans[ans.Length / 2];
        /// </summary>
        /// <param name="x1">first array </param>
        /// <param name="x2">second array</param>
        /// <returns></returns>
        public static double[] xcorr(double[] x1, double[] x2)
        {
            if (x1.Length != x2.Length)
            {
                return null;
            }

            var len = x1.Length;
            var len2 = 2 * len;
            var len3 = 3 * len;
            var s1 = new double[len3];
            var s2 = new double[len3];
            var cor = new double[len2];
            var lag = new double[len2];

            Array.Copy(x1, 0, s1, len, len);
            Array.Copy(x2, 0, s2, 0, len);

            for (int i = 0; i < len2; i++)
            {
                cor[i] = MathNet.Numerics.Statistics.Correlation.Pearson(s1, s2);
                lag[i] = i - len;
                Array.Copy(s2, 0, s2, 1, s2.Length - 1);
                s2[0] = 0;
            }

            return cor;
        }

        public static T[][,] SplitImage<T>(T[,] image1, int nSplit)
        {
            var result = new T[nSplit][,];
            var len = image1.Length;
            var block_siz = len / nSplit * Marshal.SizeOf(default(T));
            for (int j = 0; j < nSplit; j++)
            {
                result[j] = GetSplitImage(image1, nSplit, j);
            }
            return result;
        }

        public static T[,] GetSplitImage<T>(T[,] image1, int nSplit, int number)
        {
            var len = image1.Length;
            var block_siz = len / nSplit * Marshal.SizeOf(default(T));
            var result = new T[image1.GetLength(0) / nSplit, image1.GetLength(1)];
            Buffer.BlockCopy(image1, number * block_siz, result, 0, block_siz);
            return result;
        }

        public static double[] Linearize2DJaggedArray(double[][] jaggedA)
        {
            var len0 = jaggedA.Length;
            var len1 = jaggedA[0].Length;

            var len_total = jaggedA.Select(x => x.Length).Sum();
            var result = new double[len_total];

            int offset = 0;
            for (int i = 0; i < len0; i++)
            {
                Array.Copy(jaggedA[i], 0, result, offset, jaggedA[i].Length);
                offset += jaggedA[i].Length;
            }
            return result;
        }

        public static double[][] CreateJaggedArrayFromLinearArray(double[] linear, int[] dim)
        {
            var result = new double[dim[0]][];
            for (int i = 0; i < dim[0]; i++)
            {
                result[i] = new double[dim[1]];
                Array.Copy(linear, i * dim[1], result[i], 0, result[i].Length);
            }

            return result;
        }

        public static double Prod(Array sourceArray)
        {
            double val = 1;
            for (int i = 0; i < sourceArray.Length; i++)
                val *= Convert.ToDouble(sourceArray.GetValue(i));

            return val;
        }

        /// <summary>
        /// Same as matlab style shape function.
        /// </summary>
        /// <param name="sourceArray"></param>
        /// <param name="arrayShape"></param>
        /// <returns></returns>
        public static Array Reshape(Array sourceArray, int[] arrayShape)
        {
            var len0 = sourceArray.Length;
            var T = sourceArray.GetType();
            var T1 = T.GetElementType();
            var bytelen = Buffer.ByteLength(sourceArray);

            var lenMult = Prod(arrayShape);

            if (len0 != lenMult)
                return null;

            Array arr = Array.CreateInstance(T1, arrayShape);
            Buffer.BlockCopy(sourceArray, 0, arr, 0, bytelen);
            return arr;
        }

        public static T[] LinearizeArray<T>(Array array)
            where T : struct
        {
            var len = array.Length;
            var bytelen = Buffer.ByteLength(array);
            T[] arr = new T[len];
            Buffer.BlockCopy(array, 0, arr, 0, bytelen);
            return arr;
        }

        /// <summary>
        /// Sum of all matrix elements. It is very fast but only for ushort.
        /// </summary>
        /// <param name="matrixA"></param>
        /// <returns></returns>
        public static double MatrixSum(ushort[][] matrixA)
        {
            double sum = 0;
            for (int i = 0; i < matrixA.Length; i++)
                sum += calcSum(matrixA[i]);
            return sum;
        }

        public static double MatrixSum<T>(T[,] matrixA)
            where T : struct
        {
            T[] linear = LinearizeArray<T>(matrixA);
            return calcSum(linear);
        }

        /// <summary>
        /// Sum of all matrix elements. For MathNet.Numerics.LienarAlgebra.Matrix.
        /// </summary>
        /// <param name="matA"></param>
        /// <returns></returns>
        public static double MatrixSum_Matrix(Matrix<double> matA)
        {
            return matA.RowSums().Sum();
        }

        /// <summary>
        /// Convert any Vector to MathNet style Vector.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vectorA"></param>
        /// <returns></returns>
        public static MathNet.Numerics.LinearAlgebra.Vector<double> ConvertToDoubleVector<T>(T[] vectorA)
            where T : struct
        {
            int len = vectorA.Length;
            var vecA = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(len);

            for (int x = 0; x < len; x++)
                vecA[x] = Convert.ToDouble(vectorA[x]);
            return vecA;
        }

        /// <summary>
        /// Convert any Matrix to Double MathNet-style Matrix.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrixA"></param>
        /// <returns></returns>
        public static Matrix<double> ConvertToDoubleMatrix<T>(T[][] matrixA)
             where T : struct
        {
            int height = matrixA.Length;
            int width = matrixA[0].Length;
            var matA = Matrix<double>.Build.Dense(width, height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    matA[x, y] = Convert.ToDouble(matrixA[y][x]);

            return matA;
        }

        public static Matrix<double> ConvertToDoubleMatrix<T>(T[,] matrixA)
            where T : struct
        {
            int height = matrixA.GetLength(0);
            int width = matrixA.GetLength(1);
            var matA = Matrix<double>.Build.Dense(width, height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    matA[x, y] = Convert.ToDouble(matrixA[y, x]);

            return matA;
        }

        /// <summary>
        /// Convert double MathNet-style Matrix to Double MathNet-style Complex Matrix.
        /// </summary>
        /// <param name="matrixA"></param>
        /// <returns></returns>
        public static Matrix<Complex> ConvertToComplexMatrix(Matrix<double> matrixA)
        {
            int height = matrixA.ColumnCount;
            int width = matrixA.RowCount;
            var matA = Matrix<Complex>.Build.Dense(width, height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    matA[x, y] = new Complex(matrixA[x, y], 0);

            return matA;
        }

        /// <summary>
        /// Conver any Matrix to MathNet-style Complex Matrix.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrixA"></param>
        /// <returns></returns>
        public static Matrix<Complex> ConvertToComplexMatrix<T>(T[][] matrixA)
             where T : struct
        {
            int height = matrixA.Length;
            int width = matrixA[0].Length;
            var matA = Matrix<Complex>.Build.Dense(width, height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    matA[x, y] = new Complex(Convert.ToDouble(matrixA[y][x]), 0);

            return matA;
        }

        public static Matrix<Complex> ConvertToComplexMatrix<T>(T[,] matrixA)
        where T : struct
        {
            int height = matrixA.GetLength(0);
            int width = matrixA.GetLength(1);
            var matA = Matrix<Complex>.Build.Dense(width, height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    matA[x, y] = new Complex(Convert.ToDouble(matrixA[y, x]), 0);

            return matA;
        }

        /// <summary>
        /// Change the size of Array. 2D-versio of Array.Resize(ref vector, size)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="newCoNum"></param>
        /// <param name="newRoNum"></param>
        public static void ResizeArray2D<T>(ref T[,] original, int newCoNum, int newRoNum)
        {
            var newArray = new T[newCoNum, newRoNum];
            int ro1 = original.GetLength(1);
            int ro2 = newRoNum;
            int rows = Math.Min(original.GetLength(1), newRoNum);
            int columns = Math.Min(original.GetLength(0), newCoNum);
            for (int co = 0; co < columns; co++)
                Array.Copy(original, co * ro1, newArray, co * ro2, rows);
            original = newArray;
        }

        /// <summary>
        /// Function to calculate rotaiton and offset. Calculation is SwitchXY --> Flip --> Rotation.
        /// </summary>
        /// <param name="xy"></param>
        /// <param name="theta_deg"></param>
        /// <param name="offset">XY offset.</param>
        /// <param name="limit">Array of two elements for X limit and Y limit. {0,0} for no limit.</param>
        /// <param name="flipXY">Bool array of two elements for X and Y</param>
        /// <param name="switchXY"></param>
        public static void RotateOffsetTimeSeries(double[,] xy, double theta_deg, double[] offset, double[] limit, bool[] flipXY, bool switchXY)
        {
            int nChannels = xy.GetLength(0);
            int nSamples = xy.GetLength(1);
            double angle = theta_deg / 180.0 * Math.PI;
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);

            for (int i = 0; i < 2; i++)
            {
                if (switchXY)
                {
                    double temp = offset[1];
                    offset[1] = offset[0];
                    offset[0] = offset[1];
                }

                for (int j = 0; j < 2; j++)
                {
                    if (flipXY[j])
                        offset[j] = -offset[j];
                }
            }

            for (int i = 0; i < nSamples; i++)
            {
                //SwitchXY FIRST.
                if (switchXY)
                {
                    double temp = xy[1, i];
                    xy[1, i] = xy[0, i];
                    xy[0, i] = temp;
                }

                for (int j = 0; j < 2; j++)
                {
                    if (flipXY[j])
                        xy[j, i] = -xy[j, i];
                }

                //Rotation AFTER flip.
                double[] x = new double[2];
                x[0] = cosA * xy[0, i] - sinA * xy[1, i];  //X
                x[1] = sinA * xy[0, i] + cosA * xy[1, i];  //Y

                for (int j = 0; j < 2; j++)
                {
                    xy[j, i] = x[j] + offset[j];

                    if (xy[j, i] > limit[j] && limit[j] > 0)
                        xy[j, i] = limit[j];
                    else if (xy[j, i] < -limit[j] && limit[j] > 0)
                        xy[j, i] = -limit[j];
                }
            }

        }

        public static double[] Rotate(double[] xy, double theta_deg)
        {
            double[] xy1 = (double[])xy.Clone();
            double angle = theta_deg / 180.0 * Math.PI;
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);

            double x = cosA * xy[0] - sinA * xy[1];
            double y = sinA * xy[0] + cosA * xy[1];

            xy1[0] = x;
            xy1[1] = y;
            return xy1;
        }

        public static void CopyFrom3DToLinear<T>(T[][][] Matrix3D, out T[] linear)
        {

            if (Matrix3D != null)
            {
                int size0 = Matrix3D.Length;
                int size1 = Matrix3D[0].Length;
                int size2 = Matrix3D[0][0].Length;
                linear = new T[size0 * size1 * size2];

                for (int i = 0; i < size0; i++)
                {
                    for (int j = 0; j < size1; j++)
                    {
                        Array.Copy(Matrix3D[i][j], 0, linear, (i * size1 + j) * size2, size2);
                    }
                }
            }
            else
                linear = null;
        }

        public static void extract3rdAxis<T>(T[,,] Array3D, ref T[] linear, int y, int x)
        {
            int height = Array3D.GetLength(0);
            int width = Array3D.GetLength(1);
            int n_t = Array3D.GetLength(2);
            if (linear == null || linear.Length != n_t)
                linear = new T[n_t];

            int siz = Marshal.SizeOf(typeof(T));
            Buffer.BlockCopy(Array3D, siz * (width * y + x) * n_t, linear, 0, siz * n_t);
        }


        static public T[][] MatrixCreate2D<T>(int nPixelY, int nPixelX) //x and y
        {
            T[][] result = new T[nPixelY][];
            for (int i = 0; i < nPixelY; i++)
                result[i] = new T[nPixelX];

            return result;
        }

        static public T[][][] MatrixCreate3D<T>(int nPixelY, int nPixelX, int nPixelT)
        {
            T[][][] result = new T[nPixelY][][];

            for (int i = 0; i < nPixelY; i++)
            {
                result[i] = MatrixCreate2D<T>(nPixelX, nPixelT);
            }
            return result;
        }

        static public T[][][][] MatrixCreate4D<T>(int ch, int rows, int cols, int third)
        {
            T[][][][] result = new T[ch][][][];
            for (int k = 0; k < ch; k++)
                result[k] = MatrixCreate3D<T>(rows, cols, third);

            return result;
        }

        static public T[][][][][] MatrixCreate5D<T>(int ch, int rows, int cols, int third, int fourth)
        {
            T[][][][][] result = new T[ch][][][][];
            for (int k = 0; k < ch; k++)
                result[k] = MatrixCreate4D<T>(rows, cols, third, fourth);

            return result;
        }


        static public T[][] MatrixCopy2D<T>(T[][] MatrixA)
        {
            int n_Ch = MatrixA.Length;

            T[][] result = new T[n_Ch][];
            for (int ch = 0; ch < n_Ch; ch++)
            {
                int rows = MatrixA[ch].Length;
                if (MatrixA[ch] != null)
                    result[ch] = (T[])MatrixA[ch].Clone();
                else
                    result[ch] = null;
            }
            return result;

        }

        static public T[][][] MatrixCopy3D<T>(T[][][] MatrixA)
        {
            int NCols = MatrixA.Length;
            T[][][] result = new T[NCols][][];
            for (int i = 0; i < NCols; i++)
            {
                if (MatrixA[i] != null)
                    result[i] = MatrixCopy2D<T>(MatrixA[i]);
                else
                    result[i] = null;
            }
            return result;
        }

        static public T[][][][] MatrixCopy4D<T>(T[][][][] MatrixA)
        {
            int nCh = MatrixA.Length;
            T[][][][] result = new T[nCh][][][];

            for (int i = 0; i < nCh; i++)
            {
                if (MatrixA[i] != null)
                    result[i] = MatrixCopy3D<T>(MatrixA[i]);
                else
                    result[i] = null;
            }

            return result;
        }


        static public T[][][][][] MatrixCopy5D<T>(T[][][][][] MatrixA)
        {
            int nCh = MatrixA.Length;
            T[][][][][] result = new T[nCh][][][][];

            for (int i = 0; i < nCh; i++)
            {
                if (MatrixA[i] != null)
                    result[i] = MatrixCopy4D<T>(MatrixA[i]);
                else
                    result[i] = null;
            }
            return result;
        }


        static public T[][][][] TYX2XYT<T>(T[][][][] MatrixA)
        {
            int nCh = MatrixA.Length;
            int t = MatrixA[0].Length;
            int y = MatrixA[0][0].Length;
            int x = MatrixA[0][0][0].Length;
            T[][][][] result = MatrixCreate4D<T>(nCh, x, y, t);
            for (int c = 0; c < nCh; c++)
                for (int k = 0; k < t; k++)
                    for (int j = 0; j < y; j++)
                        for (int i = 0; i < x; i++)
                            result[c][i][j][k] = MatrixA[c][k][j][i];
            return result;
        }


        static public T[][][][] XYT2TYX<T>(T[][][][] MatrixA)
        {
            int nCh = MatrixA.Length;
            int x = MatrixA[0].Length;
            int y = MatrixA[0][0].Length;
            int t = MatrixA[0][0][0].Length;
            T[][][][] result = MatrixCreate4D<T>(nCh, t, y, x);
            for (int c = 0; c < nCh; c++)
                for (int i = 0; i < x; i++)
                    for (int j = 0; j < y; j++)
                        for (int k = 0; k < t; k++)
                            result[c][k][j][i] = MatrixA[c][i][j][k];
            return result;
        }

        static public double Mean<T>(T[] linearArray)
            where T : struct
        {
            double sum = calcSum<T>(linearArray);
            return sum / linearArray.Length;
        }

        static public double Mean2D<T>(T[,] matrixA)
            where T : struct
        {
            var linA = LinearizeArray<T>(matrixA);
            return Mean(linA);
        }

        static public double Mean2D<T>(T[][] matrixA, int[] Yrange, int[] Xrange)
            where T : struct
        {
            double result = 0.0;
            double sum = 0.0;
            double n = 0.0;
            int xlen = Xrange[1] - Xrange[0];

            for (int y = Yrange[0]; y < Yrange[1]; y++)
            {
                var linA = new T[xlen];
                Array.Copy(matrixA[y], Xrange[0], linA, 0, linA.Length);
                sum += calcSum(linA);
                n += xlen;
            }
            return result = sum / n;
        }

        static public double Std2D<t>(t[][] matrixA)
            where t : struct
        {
            double result = 0.0;
            double meanA = Mean2D<t>(matrixA);
            int y_length = matrixA.Length;
            int x_length = matrixA[0].Length;

            double sum = 0;

            for (int y = 0; y < y_length; y++)
            {
                SubtractConstantFromVector<t>(matrixA[y], (t)(object)meanA);
                sum += ArrayDotProduct(matrixA[y], matrixA[y]);
            }

            return result = Math.Sqrt(sum / (double)y_length / (double)x_length);
        }

        static public double Mean2D<T>(T[][] matrixA)
            where T : struct
        {
            double result = 0.0;
            double sum = 0.0;
            double n = 0.0;

            for (int y = 0; y < matrixA.Length; y++)
            {
                sum += calcSum(matrixA[y]);
                n += matrixA[y].Length;
            }

            return result = sum / n;
        }

        static void ConvertFromUshortVectorToSingleVector(System.Numerics.Vector<ushort> vectU,
            out System.Numerics.Vector<float> vectF_low,
            out System.Numerics.Vector<float> vectF_high)
        {
            System.Numerics.Vector.Widen(vectU, out System.Numerics.Vector<uint> vFL, out System.Numerics.Vector<uint> vFH);
            vectF_low = System.Numerics.Vector.ConvertToSingle(vFL);
            vectF_high = System.Numerics.Vector.ConvertToSingle(vFH);
        }

        static double SIMD_Dot<T>(T[] vect1, T[] vect2)
            where T : struct
        {
            int i = 0;
            double sumVT = 0;

            int siz = Marshal.SizeOf(typeof(T));
            var n_simd = simd_byte / siz;

            if (typeof(T) == typeof(ushort))
            {
                var vect1F = (ushort[])(object)vect1;
                var vect2F = (ushort[])(object)vect2;

                for (i = 0; i <= vect1.Length - simd_ushort; i += simd_ushort)
                {
                    System.Numerics.Vector<float> vF1L, vF1H;
                    var vF1 = new System.Numerics.Vector<ushort>(vect1F, i);
                    ConvertFromUshortVectorToSingleVector(vF1, out vF1L, out vF1H);

                    System.Numerics.Vector<float> vF2L, vF2H;
                    var vF2 = new System.Numerics.Vector<ushort>(vect2F, i);
                    ConvertFromUshortVectorToSingleVector(vF2, out vF2L, out vF2H);

                    sumVT = sumVT + (double)(System.Numerics.Vector.Dot(vF1L, vF2L));
                    sumVT = sumVT + (double)(System.Numerics.Vector.Dot(vF1H, vF2H));
                }
                for (; i < vect1.Length; i++)
                {
                    sumVT += (double)vect1F[i] * (double)vect2F[i];
                }//
            }
            else if (typeof(T) == typeof(float))
            {
                var vect1F = (float[])(object)vect1;
                var vect2F = (float[])(object)vect2;

                for (i = 0; i <= vect1.Length - simd_float; i += simd_float)
                {
                    var vF1 = new System.Numerics.Vector<float>(vect1F, i);
                    var vF2 = new System.Numerics.Vector<float>(vect2F, i);
                    //sumVT = sumVT + (double)(System.Numerics.Vector.Dot(vF1, vF2));
                    System.Numerics.Vector<double> vF1L, vF1H, vF2L, vF2H;
                    System.Numerics.Vector.Widen(vF1, out vF1L, out vF1H);
                    System.Numerics.Vector.Widen(vF2, out vF2L, out vF2H);
                    sumVT = sumVT + (double)(System.Numerics.Vector.Dot(vF1L, vF2L));
                    sumVT = sumVT + (double)(System.Numerics.Vector.Dot(vF1H, vF2H));
                }
                for (; i < vect1.Length; i++)
                {
                    sumVT += (double)vect1F[i] * (double)vect2F[i];
                }//
            }
            else
            {
                for (i = 0; i <= vect1.Length - n_simd; i += n_simd)
                {
                    var vF = new System.Numerics.Vector<T>(vect1, i);
                    var vF2 = new System.Numerics.Vector<T>(vect2, i);
                    sumVT = sumVT + Convert.ToDouble(System.Numerics.Vector.Dot(vF, vF2));
                }
                for (; i < vect1.Length; i++)
                {
                    sumVT += Convert.ToDouble(vect1[i]) * Convert.ToDouble(vect2[i]);
                }//
            }



            return sumVT;
        }

        public static double SIMD_Dot<T>(T[,] Mat1, T[,] Mat2)
             where T : struct
        {
            var vect1 = LinearizeArray<T>(Mat1);
            var vect2 = LinearizeArray<T>(Mat2);

            if (vect1.Length == vect2.Length)
                return SIMD_Dot(vect1, vect2);
            else
                return 0;
        }


        /// <summary>
        /// More generic, but slower than Dot_withRange.
        /// </summary>
        /// <typeparam name="T">float, uint, ushort etc</typeparam>
        /// <param name="vect">array</param>
        /// <param name="vect2_float">float array</param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static double Dot_withRange2<T>(T[] vect, float[] vect2_float, int[] range)
            where T : struct
        {
            int len = range[1] - range[0];
            var vect_r = new T[len];
            var vect2_r = new float[len];
            Array.Copy(vect, range[0], vect_r, 0, len);
            Array.Copy(vect2_float, range[0], vect2_r, 0, len);
            var vect1_r = convertToFloat(vect_r);
            return ArrayDotProduct(vect1_r, vect2_r);
        }


        public static double Dot_withRange(ushort[] vect, float[] vect2_float, int[] range)
        {
            if (simd_on)
                return Dot_withRange_SIMD(vect, vect2_float, range);
            else
                return Dot_withRange_Normal(vect, vect2_float, range);

        }

        public static double Dot_withRange_Normal(ushort[] vect, float[] vect2_float, int[] range)
        {
            double sumVT = 0;

            for (int i = range[0]; i < range[1]; i++)
            {
                sumVT += (float)vect[i] * vect2_float[i];
            }//

            return sumVT;
        }

        public static double Dot_withRange_SIMD(ushort[] vect, float[] vect2_float, int[] range)
        {
            int i = range[0];
            double sumVT = 0;

            for (i = range[0]; i <= range[1] - simd_ushort; i += simd_ushort)
            {
                System.Numerics.Vector<float> vF1, vF2;
                System.Numerics.Vector<ushort> vU = new System.Numerics.Vector<ushort>(vect, i);

                ConvertFromUshortVectorToSingleVector(vU, out vF1, out vF2);

                var vT1f = new System.Numerics.Vector<float>(vect2_float, i);
                var vT2f = new System.Numerics.Vector<float>(vect2_float, i + simd_float);

                sumVT = sumVT + System.Numerics.Vector.Dot(vF1, vT1f) + System.Numerics.Vector.Dot(vF2, vT2f);
            }

            for (; i < range[1]; i++)
            {
                sumVT += vect[i] * vect2_float[i];
            }//

            return sumVT;
        }

        public static uint calcSumUshort(ushort[] vect, int[] range)
        {
            if (simd_on)
                return SIMD_VectorSumUshort(vect, range);
            else
            {
                return calcSumUshort_Normal(vect, range);
            }
        }

        public static uint calcSumUshort_Normal(ushort[] vect, int[] range)
        {
            uint sum = 0;
            for (int i = range[0]; i < range[1]; i++)
                sum += (uint)vect[i];
            return sum;
        }

        public static T calcSumFast<T>(T[] vect, int[] range)
            where T : struct
        {
            if (simd_on)
                return SIMD_VectorSum<T>(vect, range);
            else
                return calcSumFast_Normal<T>(vect, range);
        }

        public static T calcSumFast_Normal<T>(T[] vect, int[] range)
           where T : struct
        {
            dynamic sum1 = 0;
            for (int i = range[0]; i < range[1]; i++)
                sum1 += (dynamic)vect[i];
            return (T)sum1;
        }

        public static uint SIMD_VectorSumUshort(ushort[] vect, int[] range)
        {
            var sumV = System.Numerics.Vector<uint>.Zero;

            var i = range[0];
            for (i = range[0]; i <= range[1] - simd_ushort; i += simd_ushort)
            {
                var vb = new System.Numerics.Vector<ushort>(vect, i);
                System.Numerics.Vector.Widen(vb, out System.Numerics.Vector<uint> vb_l, out System.Numerics.Vector<uint> vb_h);
                sumV = System.Numerics.Vector.Add(sumV, vb_l);
                sumV = System.Numerics.Vector.Add(sumV, vb_h);
            }

            uint sum1 = 0;
            for (int j = 0; j < simd_ushort / 2; j++)
                sum1 += sumV[j];

            for (; i < range[1]; i++)
                sum1 += (uint)vect[i];

            return sum1;
        }

        public static T SIMD_VectorSum<T>(T[] vect, int[] range)
            where T : struct
        {
            int siz = Marshal.SizeOf(typeof(T));
            var n_simd = simd_byte / siz;

            var sumV = System.Numerics.Vector<T>.Zero;

            var i = range[0];
            for (i = range[0]; i <= range[1] - n_simd; i += n_simd)
            {
                var vb = new System.Numerics.Vector<T>(vect, i);
                sumV = System.Numerics.Vector.Add(sumV, vb);
            }

            dynamic sum1 = 0;
            for (int j = 0; j < n_simd; j++)
                sum1 += (dynamic)sumV[j];

            //ushort sum1 = System.Numerics.Vector.Dot(sumV, System.Numerics.Vector<ushort>.One);

            for (; i < range[1]; i++)
            {
                sum1 += (dynamic)vect[i];
            }

            return (T)sum1;
        }

        static public void ArrayCalc<T>(T[,,] arrayA, T[,,] arrayB, CalculationType Op)
            where T : struct
        {
            var linear1 = new T[arrayA.Length];
            var linear2 = new T[arrayB.Length];
            int siz = Marshal.SizeOf(typeof(T));

            Buffer.BlockCopy(arrayA, 0, linear1, 0, arrayA.Length * siz);
            Buffer.BlockCopy(arrayB, 0, linear2, 0, arrayB.Length * siz);

            ArrayCalc(linear1, linear2, Op);

            Buffer.BlockCopy(linear1, 0, arrayA, 0, arrayA.Length * siz);
        }

        static public void ArrayCalc<T>(T[] arrayA, T[] arrayB, CalculationType Op)
            where T : struct
        {
            int siz = Marshal.SizeOf(typeof(T));
            var n_simd = simd_byte / siz;

            int n = arrayA.Length;
            bool add = Op == CalculationType.Add;
            bool sub = Op == CalculationType.Subtract;
            bool mul = Op == CalculationType.Multiply;
            bool max = Op == CalculationType.Max;


            if (simd_on)
            {
                int k = 0;
                for (k = 0; k <= n - n_simd; k += n_simd)
                {
                    var va = new System.Numerics.Vector<T>(arrayA, k);
                    var vb = new System.Numerics.Vector<T>(arrayB, k);
                    if (add)
                        (va + vb).CopyTo(arrayA, k);
                    else if (sub)
                        (va - vb).CopyTo(arrayA, k);
                    else if (mul)
                        (va * vb).CopyTo(arrayA, k);
                    else if (max)
                        System.Numerics.Vector.Max(va, vb).CopyTo(arrayA, k);
                }

                ArrayCalc_Normal<T>(arrayA, arrayB, new int[] { k, n }, Op);
            }
            else
                ArrayCalc_Normal<T>(arrayA, arrayB, new int[] { 0, n }, Op);

        }

        static public void ArrayCalc_Normal<T>(T[] arrayA, T[] arrayB, int[] range, CalculationType Op)
        {
            bool add = Op == CalculationType.Add;
            bool sub = Op == CalculationType.Subtract;
            bool mul = Op == CalculationType.Multiply;
            bool max = Op == CalculationType.Max;
            int n = arrayA.Length;

            if (typeof(T) == typeof(ushort) || typeof(T) == typeof(short))
            {
                for (int k = range[0]; k < range[1]; k++)
                {
                    if (add)
                        arrayA[k] = (T)Convert.ChangeType((Convert.ToInt32(arrayA[k]) + Convert.ToInt32(arrayB[k])), typeof(T));
                    else if (sub)
                        arrayA[k] = (T)Convert.ChangeType((Convert.ToInt32(arrayA[k]) - Convert.ToInt32(arrayB[k])), typeof(T));
                    else if (mul)
                        arrayA[k] = (T)Convert.ChangeType((Convert.ToInt32(arrayA[k]) * Convert.ToInt32(arrayB[k])), typeof(T));
                    else if (max)
                        arrayA[k] = (T)Convert.ChangeType(Math.Max(Convert.ToInt32(arrayA[k]), Convert.ToInt32(arrayB[k])), typeof(T));
                }
            }
            else if (typeof(T) == typeof(uint))
            {
                var arrayA1 = (uint[])(object)arrayA;
                var arrayB1 = (uint[])(object)arrayB;
                for (int k = range[0]; k < range[1]; k++)
                {
                    if (add)
                        arrayA1[k] = arrayA1[k] + arrayB1[k];
                    else if (sub)
                        arrayA1[k] = arrayA1[k] - arrayB1[k];
                    else if (mul)
                        arrayA1[k] = arrayA1[k] * arrayB1[k];
                    else if (max)
                        arrayA1[k] = Math.Max(arrayA1[k], arrayB1[k]);
                }
            }
            else if (typeof(T) == typeof(int))
            {
                var arrayA1 = (int[])(object)arrayA;
                var arrayB1 = (int[])(object)arrayB;
                for (int k = range[0]; k < range[1]; k++)
                {
                    if (add)
                        arrayA1[k] = arrayA1[k] + arrayB1[k];
                    else if (sub)
                        arrayA1[k] = arrayA1[k] - arrayB1[k];
                    else if (mul)
                        arrayA1[k] = arrayA1[k] * arrayB1[k];
                    else if (max)
                        arrayA1[k] = Math.Max(arrayA1[k], arrayB1[k]);
                }
            }
            else if (typeof(T) == typeof(long))
            {
                var arrayA1 = (long[])(object)arrayA;
                var arrayB1 = (long[])(object)arrayB;
                for (int k = range[0]; k < range[1]; k++)
                {
                    if (add)
                        arrayA1[k] = arrayA1[k] + arrayB1[k];
                    else if (sub)
                        arrayA1[k] = arrayA1[k] - arrayB1[k];
                    else if (mul)
                        arrayA1[k] = arrayA1[k] * arrayB1[k];
                    else if (max)
                        arrayA1[k] = Math.Max(arrayA1[k], arrayB1[k]);
                }
            }
            else if (typeof(T) == typeof(double))
            {
                var arrayA1 = (double[])(object)arrayA;
                var arrayB1 = (double[])(object)arrayB;
                for (int k = range[0]; k < range[1]; k++)
                {
                    if (add)
                        arrayA1[k] = arrayA1[k] + arrayB1[k];
                    else if (sub)
                        arrayA1[k] = arrayA1[k] - arrayB1[k];
                    else if (mul)
                        arrayA1[k] = arrayA1[k] * arrayB1[k];
                    else if (max)
                        arrayA1[k] = Math.Max(arrayA1[k], arrayB1[k]);
                }
            }
            else if (typeof(T) == typeof(float))
            {
                var arrayA1 = (float[])(object)arrayA;
                var arrayB1 = (float[])(object)arrayB;
                for (int k = range[0]; k < range[1]; k++)
                {
                    if (add)
                        arrayA1[k] = arrayA1[k] + arrayB1[k];
                    else if (sub)
                        arrayA1[k] = arrayA1[k] - arrayB1[k];
                    else if (mul)
                        arrayA1[k] = arrayA1[k] * arrayB1[k];
                    else if (max)
                        arrayA1[k] = Math.Max(arrayA1[k], arrayB1[k]);
                }
            }
            else
            {
                for (int k = range[0]; k < range[1]; k++)
                {
                    if (add)
                        arrayA[k] = (dynamic)arrayA[k] + (dynamic)arrayB[k];
                    else if (sub)
                        arrayA[k] = (dynamic)arrayA[k] - (dynamic)arrayB[k];
                    else if (mul)
                        arrayA[k] = (dynamic)arrayA[k] * (dynamic)arrayB[k];
                    else if (max)
                        arrayA[k] = Math.Max((dynamic)arrayA[k], (dynamic)arrayB[k]);
                }

            }

        }


        static public T[,,] MatrixCalc3D<T>(T[,,] matrixA, T[,,] matrixB, CalculationType Op)
        where T : struct
        {
            int rows = matrixA.GetLength(0);
            int cols = matrixA.GetLength(1);
            int third = matrixA.GetLength(2);

            T[,,] result = new T[rows, cols, third];
            var linearA = new T[matrixA.Length];
            var linearB = new T[matrixA.Length];

            int bufSize = Buffer.ByteLength(linearA);

            Buffer.BlockCopy(matrixA, 0, linearA, 0, bufSize);
            Buffer.BlockCopy(matrixB, 0, linearB, 0, bufSize);

            ArrayCalc(linearA, linearB, Op);

            Buffer.BlockCopy(linearA, 0, result, 0, bufSize);

            return result;
        }

        static public T[,] MatrixCalc2D<T>(T[,] matrixA, T[,] matrixB, CalculationType Op, bool overwrite)
            where T : struct
        {
            int rows = matrixA.GetLength(0);
            int cols = matrixA.GetLength(1);

            T[,] result;
            if (overwrite)
                result = matrixA;
            else
                result = new T[rows, cols];

            var linearA = new T[matrixA.Length];
            var linearB = new T[matrixA.Length];

            int bufSize = Buffer.ByteLength(linearA);

            Buffer.BlockCopy(matrixA, 0, linearA, 0, bufSize);
            Buffer.BlockCopy(matrixB, 0, linearB, 0, bufSize);

            ArrayCalc(linearA, linearB, Op);

            Buffer.BlockCopy(linearA, 0, result, 0, bufSize);

            return result;
        }

        static public T[][] MatrixCalc2D<T>(T[][] matrixA, T[][] matrixB, CalculationType Op, bool overwrite)
            where T : struct
        {
            int rows = matrixA.Length;
            int cols = matrixA[0].Length;

            T[][] result;
            if (overwrite)
                result = matrixA;
            else
                result = MatrixCopy2D(matrixA);

            for (int j = 0; j < rows; j++)
            {
                ArrayCalc(matrixA[j], matrixB[j], Op);
            }
            return result;
        }

        static public T[][][] MatrixCalc3D<T>(T[][][] matrixA, T[][][] matrixB, CalculationType Op, bool overwrite)
            where T : struct
        {
            if (matrixA == null)
                return null;

            int rows = matrixA.Length;
            int cols = matrixA[0].Length;
            int third = matrixA[0][0].Length;
            T[][][] result;
            if (overwrite)
                result = matrixA;
            else
                result = MatrixCopy3D<T>(matrixA);

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; ++j)
                    ArrayCalc<T>(result[i][j], matrixB[i][j], Op);
            return result;
        }

        static public float[,] InverseMatrix(float[,] matrixA)
        {
            int rows = matrixA.GetLength(0);
            int cols = matrixA.GetLength(1);
            float[,] result = new float[rows, cols];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    result[i, j] = 1.0f / result[i, j];
            return result;
        }

        static public T[,] DivideConstantFromMatrix<T>(T[,] matrixA, T C)
            where T : struct
        {
            if (matrixA == null)
                return null;

            int rows = matrixA.GetLength(0);
            int cols = matrixA.GetLength(1);
            var result = new T[rows, cols];
            var resultL = new T[rows * cols];

            var matrixA_L = LinearizeArray<T>(matrixA);

            resultL = DivideConstantFromVector(matrixA_L, C);
            int bufLen = Buffer.ByteLength(result);
            Buffer.BlockCopy(resultL, 0, result, 0, bufLen);

            return result;
        }

        /// <summary>
        /// Convert 0-1 float value to 0-255 value.
        /// </summary>
        /// <param name="vector">Value in 0.. 1</param>
        /// <returns></returns>
        static public byte[] FloatToByteVector(float[] vector)
        {
            float[] floatVec = MultiplyConstantToVector(vector, Byte.MaxValue);
            byte[] byteVec = new byte[floatVec.Length];
            for (int i = 0; i < floatVec.Length; i++)
            {
                if (vector[i] < 0)
                    byteVec[i] = 0;
                else if (vector[i] < Byte.MaxValue)
                    byteVec[i] = (byte)floatVec[i];
                else
                    byteVec[i] = Byte.MaxValue;

            }
            return byteVec;
        }

        static public byte[,] FloatToByteMatrix(float[,] matrix)
        {
            var floatVec = LinearizeArray<float>(matrix);
            var byteVec = FloatToByteVector(floatVec);
            return (byte[,])Reshape(byteVec, new int[] { matrix.GetLength(0), matrix.GetLength(1) });
        }

        static public T[] MultiplyConstantToVector<T>(T[] vector, T C)
            where T : struct
        {
            int siz = Marshal.SizeOf(typeof(T));
            var n_simd = simd_byte / siz;

            T[] result;

            if (simd_on)
            {
                int len = vector.Length;
                Array.Resize(ref vector, (int)Math.Ceiling((double)len / (double)n_simd) * n_simd);
                result = new T[vector.Length];

                int i = 0;
                var vecC = System.Numerics.Vector<T>.One * C;
                for (i = 0; i < vector.Length; i += n_simd)
                {
                    var vec = new System.Numerics.Vector<T>(vector, i);
                    System.Numerics.Vector.Multiply(vec, vecC).CopyTo(result, i);
                }

                Array.Resize(ref result, len);
            }
            else
            {
                result = new T[vector.Length];
                for (int i = 0; i < vector.Length; i++)
                    result[i] = (dynamic)vector[i] * C;
            }
            return result;
        }

        static public T[] DivideConstantFromVector<T>(T[] vector, T C)
            where T : struct
        {
            int siz = Marshal.SizeOf(typeof(T));
            var n_simd = simd_byte / siz;

            T[] result;

            if (simd_on)
            {
                int len = vector.Length;
                Array.Resize(ref vector, (int)Math.Ceiling((double)len / (double)n_simd) * n_simd);
                result = new T[vector.Length];

                int i = 0;
                var vecC = System.Numerics.Vector<T>.One * C;
                for (i = 0; i < vector.Length; i += n_simd)
                {
                    var vec = new System.Numerics.Vector<T>(vector, i);
                    System.Numerics.Vector.Divide(vec, vecC).CopyTo(result, i);
                }

                Array.Resize(ref result, len);
            }
            else
            {
                result = new T[vector.Length];
                for (int i = 0; i < vector.Length; i++)
                    result[i] = (dynamic)vector[i] / C;
            }
            return result;
        }

        static public T[] SubtractConstantFromVector<T>(T[] vector, T C)
        where T : struct
        {
            int siz = Marshal.SizeOf(typeof(T));
            var n_simd = simd_byte / siz;

            T[] result;

            if (simd_on)
            {
                int len = vector.Length;
                Array.Resize(ref vector, (int)Math.Ceiling((double)len / (double)n_simd) * n_simd);
                result = new T[vector.Length];

                int i = 0;
                var vecC = System.Numerics.Vector<T>.One * C;
                for (i = 0; i < vector.Length; i += n_simd)
                {
                    var vec = new System.Numerics.Vector<T>(vector, i);
                    System.Numerics.Vector.Subtract(vec, vecC).CopyTo(result, i);
                }

                Array.Resize(ref result, len);
            }
            else
            {
                result = new T[vector.Length];
                for (int i = 0; i < vector.Length; i++)
                    result[i] = (dynamic)vector[i] - C;
            }
            return result;
        }

        static public T[][] SubtractConstantFromMatrix<T>(T[][] matrixA, T C)
            where T : struct
        {
            if (matrixA == null)
                return null;

            int rows = matrixA.Length;
            int cols = matrixA[0].Length;
            var result = new T[rows][];

            for (int i = 0; i < rows; i++)
            {
                result[i] = SubtractConstantFromVector(matrixA[i], C);
            }
            return result;
        }

        static public T[,] SubtractConstantFromMatrix<T>(T[,] matrixA, T C)
            where T : struct
        {
            int siz = Marshal.SizeOf(typeof(T));

            if (matrixA == null)
                return null;

            T[] linearF = LinearizeArray<T>(matrixA);
            T[] result1 = SubtractConstantFromVector(linearF, C);

            var result = new T[matrixA.GetLength(0), matrixA.GetLength(1)];

            Buffer.BlockCopy(result1, 0, result, 0, result.Length * siz);
            return result;
        }

        static public float[,] ConvertToFloatMatrix<T>(T[,] matrixA)
        {
            float[,] result = new float[matrixA.GetLength(0), matrixA.GetLength(1)];
            for (int y = 0; y < matrixA.GetLength(0); y++)
                for (int x = 0; x < matrixA.GetLength(1); x++)
                    result[y, x] = Convert.ToSingle(matrixA[y, x]);

            return result;
        }

        static public double[][] MatrixProduct(double[][] A, double[][] B)
        {
            int aRows = A.Length;
            int aCols = A[0].Length;
            int bRows = B.Length;
            int bCols = B[0].Length;
            if (aCols != bRows)
            {
                Debug.WriteLine("Non-conformable matrices");
                return null;
            }
            double[][] C = MatrixCreate2D<double>(aRows, bCols);

            for (int i = 0; i < aRows; i++)
            {
                double[] iRowA = A[i];
                double[] iRowC = C[i];
                for (int k = 0; k < aCols; k++)
                {
                    double[] kRowB = B[k];
                    double ikA = iRowA[k];
                    for (int j = 0; j < bCols; j++)
                    {
                        iRowC[j] += ikA * kRowB[j];
                    }
                }
            }
            return C;
        }

        static public double[,] MatrixProduct(double[,] matrixA, double[,] matrixB)
        {
            int aRows = matrixA.GetLength(0);
            int aCols = matrixA.GetLength(1);
            int bRows = matrixB.GetLength(0);
            int bCols = matrixB.GetLength(1);
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices");
            double[,] result = new double[aRows, bCols];
            for (int i = 0; i < aRows; ++i)
                for (int k = 0; k < aCols; ++k) //Cash friendly!
                    for (int j = 0; j < bCols; ++j)
                        result[i, j] = result[i, j] + matrixA[i, k] * matrixB[k, j];
            return result;
        }

        static public double[] MatrixProduct(double[][] matrixA, double[] vectorB)
        {
            int aRows = matrixA.GetLength(0);
            int aCols = matrixA[0].GetLength(0);
            int bRows = vectorB.Length;
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices");
            double[] result = new double[aRows];

            for (int i = 0; i < aRows; i++)
                result[i] = ArrayDotProduct(matrixA[i], vectorB);

            return result;
        }

        static public double[] MatrixProduct(double[,] matrixA, double[] vectorB)
        {
            int aRows = matrixA.GetLength(0);
            int aCols = matrixA.GetLength(1);
            int bRows = vectorB.Length;
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices");
            double[] result = new double[aRows];

            for (int i = 0; i < aRows; ++i)
                for (int k = 0; k < aCols; ++k)
                    result[i] = result[i] + matrixA[i, k] * vectorB[k];

            return result;
        }

        //Pretty slow.
        static public T[][] MatrixTranspose<T>(T[][] matrixA)
        {
            //matrix size = matrixcA[n][n1]
            int n = matrixA.Length;
            int n1 = matrixA[0].Length;

            var matrixB = MatrixCreate2D<T>(n1, n); //create matrix[n1][n]

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n1; j++)
                {
                    matrixB[j][i] = matrixA[i][j];
                }

            return matrixB;
        }

        /// <summary>
        /// MatrixInverse: invert 2D matrix
        /// </summary>
        /// <param name="matrix">Input Matrix (2D) matrix [,] </param>
        /// <returns>Output Matrix (2D) matrix [,] </returns>
        static public double[,] MatrixInverse(double[,] matrix)
        {
            // assumes determinant is not 0
            int n = matrix.GetLength(0);
            double[,] result = (double[,])matrix.Clone();

            int toggle;
            toggle = MatrixDecompose(matrix, out double[,] lum, out int[] perm);

            double[] b = new double[n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                    if (i == perm[j])
                        b[j] = 1.0;
                    else
                        b[j] = 0.0;

                double[] x = Helper(lum, b); // 
                for (int j = 0; j < n; ++j)
                    result[j, i] = x[j];
            }
            return result;
        } //MatrixInverse

        /// <summary>
        /// Invert Matrix.
        /// https://msdn.microsoft.com/en-us/magazine/mt736457.aspx
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        static public double[][] MatrixInverse(double[][] matrix)
        {
            // assumes determinant is not 0
            int n = matrix.GetLength(0);
            double[][] result = MatrixCopy2D<double>(matrix);

            int toggle;
            toggle = MatrixDecompose(matrix, out double[][] lum, out int[] perm);

            double[] b = new double[n];

            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                    b[j] = (i == perm[j]) ? 1.0 : 0.0;

                double[] x = Helper(lum, b); // 
                for (int j = 0; j < n; ++j) //slow...
                    result[j][i] = x[j];
            }
            return result;
        } //MatrixInverse

        /// <summary>
        /// MatrixDecompose
        /// Source: http://quaetrix.com/Matrix/code.html
        /// Crout's LU decomposition for matrix determinant and inverse
        /// lower gets dummy 1.0s on diagonal (0.0s above)
        /// upper gets lum values on diagonal (0.0s below)
        /// </summary>
        /// <param name="m">Input Matrix </param>
        /// <param name="lum"> Output: stores combined lower & upper in lum[,] </param>
        /// <param name="perm">Outpu: stores row permuations into perm[]</param>
        /// <returns> returns +1 or -1 according to even or odd number of row permutations</returns>
        static int MatrixDecompose(double[,] m, out double[,] lum, out int[] perm)
        {
            int toggle = +1; // even (+1) or odd (-1) row permutatuions
            int n = m.GetLength(0);
            lum = (double[,])m.Clone();
            perm = Enumerable.Range(0, n).ToArray(); //0,1,2...

            for (int j = 0; j < n - 1; ++j) // process by column. note n-1 
            {
                double max = Math.Abs(lum[j, j]);
                int piv = j;

                for (int i = j + 1; i < n; ++i) // find pivot index
                {
                    double xij = Math.Abs(lum[i, j]);
                    if (xij > max)
                    {
                        max = xij;
                        piv = i;
                    }
                } // i

                if (piv != j)
                {
                    for (int i = 0; i < n; ++i)
                        SwapNum(ref lum[piv, i], ref lum[j, i]);

                    SwapNum(ref perm[piv], ref perm[j]);
                    toggle = -toggle;
                }

                double xjj = lum[j, j];
                if (xjj != 0.0)
                {
                    for (int i = j + 1; i < n; ++i)
                    {
                        double xij = lum[i, j] / xjj;
                        lum[i, j] = xij;
                        for (int k = j + 1; k < n; ++k)
                            lum[i, k] -= xij * lum[j, k];
                    }
                }

            } // j

            return toggle;
        } // MatrixDecompose

        /// <summary>
        /// MatrixDecompose
        /// Source: http://quaetrix.com/Matrix/code.html
        /// Crout's LU decomposition for matrix determinant and inverse
        /// lower gets dummy 1.0 on diagonal (0.0 above)
        /// upper gets lum values on diagonal (0.0 below)
        /// </summary>
        /// <param name="m">Input Matrix  double[][] </param>
        /// <param name="lum"> Output: stores combined lower & upper in lum[][] </param>
        /// <param name="perm">Outpu: stores row permuations into perm[]</param>
        /// <returns> returns +1 or -1 according to even or odd number of row permutations</returns>
        static int MatrixDecompose(double[][] m, out double[][] lum, out int[] perm)
        {
            int toggle = +1; // even (+1) or odd (-1) row permutatuions
            int n = m.Length;
            lum = MatrixCopy2D<double>(m);
            perm = Enumerable.Range(0, n).ToArray(); //0,1,2...

            for (int j = 0; j < n - 1; ++j) // process by column. note n-1 
            {
                double max = Math.Abs(lum[j][j]);
                int piv = j;

                for (int i = j + 1; i < n; ++i) // find pivot index
                {
                    double xij = Math.Abs(lum[i][j]);
                    if (xij > max)
                    {
                        max = xij;
                        piv = i;
                    }
                } // i

                if (piv != j)
                {
                    SwapNum(ref lum[piv], ref lum[j]); //Swap reference.
                    SwapNum(ref perm[piv], ref perm[j]); //swap int
                    toggle = -toggle;
                }

                double xjj = lum[j][j];
                if (xjj != 0.0)
                {
                    for (int i = j + 1; i < n; ++i)
                    {
                        double xij = lum[i][j] / xjj;
                        lum[i][j] = xij;

                        for (int k = j + 1; k < n; ++k)
                            lum[i][k] -= xij * lum[j][k];
                    }
                }//xjj != 0
            } // j

            return toggle;

        } // MatrixDecompose

        static void SwapNum<T>(ref T x, ref T y)
        {
            T temp = x;
            x = y;
            y = temp;
        }

        static double[] Helper(double[,] luMatrix, double[] b) // helper for [,] version.
        {
            int n = luMatrix.GetLength(0);
            double[] x = new double[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; ++i)
            {
                double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i, j] * x[j];
                x[i] = sum;
            }

            x[n - 1] /= luMatrix[n - 1, n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i, j] * x[j];
                x[i] = sum / luMatrix[i, i];
            }

            return x;
        } // Helper

        static double[] Helper(double[][] luMatrix, double[] b) // helper
        {
            int n = luMatrix.GetLength(0);
            double[] x = new double[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; ++i)
            {
                double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum;
            }

            x[n - 1] /= luMatrix[n - 1][n - 1];

            for (int i = n - 2; i >= 0; --i)
            {
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum / luMatrix[i][i];
            }

            return x;
        } // Helper



        public static double Norm(double[] x)
        {
            return Math.Sqrt(MatrixCalc.ArrayDotProduct(x, x));
        }


        public static double Erfc(double x)
        {
            return MathNet.Numerics.SpecialFunctions.Erfc(x);
        }

        public static double Erf(double x)
        {
            return MathNet.Numerics.SpecialFunctions.Erf(x);
        }

        /// <summary>
        /// My own implementation of ERF. Not used anymore.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Erf_ry(double x)
        {
            double sign = 1;
            if (x < 0)
                sign = -1;
            double y = 0;

            double t = 1 / (1 + 0.5 * Math.Abs(x));
            double[] a = new double[10];
            a[0] = -1.26551223;
            a[1] = 1.00002368;
            a[2] = 0.37409196;
            a[3] = 0.09678418;
            a[4] = -0.18628806;
            a[5] = 0.27886807;
            a[6] = -1.13520398;
            a[7] = 1.48851587;
            a[8] = -0.82215223;
            a[9] = 0.17087277;
            double t1 = a[0];
            double t_n = 1;
            for (int i = 1; i < 10; i++)
            {
                t_n = t_n * t;
                t1 = t1 + a[i] * t_n;
            }



            double tau = t * Math.Exp(-x * x + t1);
            y = sign * (1 - tau);

            return y;
        }



        /// <summary>
        /// This is a fucntion providing convolution of exponential and Guassian.
        /// </summary>
        /// <param name="beta0">parameters: peak, decay, gaussian sigma, offset</param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double ExpGauss(double[] beta0, double x)
        {
            double y;
            double pop1 = beta0[0];
            double k1 = beta0[1];
            double tauG = beta0[2];
            double t0 = beta0[3];

            double y1 = pop1 * Math.Exp(tauG * tauG * k1 * k1 / 2 - (x - t0) * k1);
            double y2 = MatrixCalc.Erfc((tauG * tauG * k1 - (x - t0)) / (Math.Sqrt(2) * tauG));
            y = y1 * y2 / 2;

            return y;
        }
    }

    public enum CalculationType
    {
        Add = 1,
        Subtract = 2,
        Multiply = 3,
        Max = 4,
    }
}
