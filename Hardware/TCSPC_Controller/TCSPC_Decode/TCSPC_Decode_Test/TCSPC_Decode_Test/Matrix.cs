using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace TCSPC_controls
{
    class Matrix
    {
        static bool simd_on = Vector.IsHardwareAccelerated;
        static int simd_16 = Vector<ushort>.Count;
        static int simd_32 = Vector<int>.Count;
        static int simd_64 = Vector<double>.Count;

        public static void SubtractArrayWithConstant(ulong[] SourceArray, int[] destinationArray, ulong subtractor)
        {
            int n = SourceArray.Length;

            if (simd_on)
            {
                var vbu = Vector<ulong>.One * (ulong)subtractor;
                int i = 0;
                for (i = 0; i <= n - simd_32; i += simd_32) //Calculate every 8.
                {
                    //calculate i
                    var vau = new Vector<ulong>(SourceArray, i);
                    var vcu = Vector.Subtract(vau, vbu);

                    //calculate i + 4 (simd_64 = 4) for 256 register.
                    var vau2 = new Vector<ulong>(SourceArray, i + simd_64);
                    var vcu2 = Vector.Subtract(vau2, vbu);

                    var vc = (Vector<int>)Vector.Narrow(vcu, vcu2); //you can do this cast!!

                    vc.CopyTo(destinationArray, i);
                }
                for (; i < n; i++)
                    destinationArray[i] = (int)(SourceArray[i] - (ulong)subtractor);
            }
            else
            {
                for (int i = 0; i < n; i++)
                    destinationArray[i] = (int)(SourceArray[i] - (ulong)subtractor);
            }
        }

        public static void MultiplyArrayWithConstant(ulong[] SourceArray, ulong[] destinationArray, int multiplier, int n)
        {
            if (simd_on)
            {
                var vbf = Vector<double>.One * (double)multiplier;
                int i = 0;
                for (i = 0; i <= n - simd_64; i += simd_64)
                {
                    var va = new Vector<ulong>(SourceArray, i);
                    var vaf = Vector.ConvertToDouble(va);
                    var vcf = Vector.Multiply<double>(vaf, vbf);
                    Vector.ConvertToUInt64(vcf).CopyTo(destinationArray, i);

                }
                for (; i < n; i++)
                    destinationArray[i] = SourceArray[i] * (ulong)multiplier;
            }
            else
            {
                for (int i = 0; i < n; i++)
                    destinationArray[i] = SourceArray[i] * (ulong)multiplier;
            }
        }


        public static void DivideArrayWithConstant(ulong[] SourceArray, uint[] destinationArray, uint divider, int n)
        {
            //int n = SourceArray.Length;

            if (simd_on && n > simd_16)
            {
                var vbf = Vector<double>.One * (double)divider;
                int i = 0;
                for (i = 0; i <= n - simd_32; i += simd_32)
                {
                    var va = new Vector<ulong>(SourceArray, i);
                    var vaf = Vector.ConvertToDouble(va);
                    var vcf = Vector.Divide<double>(vaf, vbf);
                    var vc1 = Vector.ConvertToUInt64(vcf);

                    va = new Vector<ulong>(SourceArray, i + simd_64);
                    vaf = Vector.ConvertToDouble(va);
                    vcf = Vector.Divide<double>(vaf, vbf);
                    var vc2 = Vector.ConvertToUInt64(vcf);

                    var vc = Vector.Narrow(vc1, vc2);
                    vc.CopyTo(destinationArray, i);
                }
                for (; i < n; i++)
                    destinationArray[i] = (uint)(SourceArray[i] / divider);
            }
            else
            {
                for (int i = 0; i < n; i++)
                    destinationArray[i] = (uint)(SourceArray[i] / divider);
            }
        }

        public static void CopyDataLines(ushort[][][,,] source, ushort[][][,,] destination, int startLine, int endLine)
        {
            for (int ch = 0; ch < source.Length; ch++)
            {
                if (source[ch] != null)
                {
                    int y_length = source[ch][0].GetLength(0);
                    int x_length = source[ch][0].GetLength(1);
                    int t_length = source[ch][0].GetLength(2);
                    for (int z = 0; z < source[0].Length; z++)
                    {
                        for (int l = startLine; l < endLine; l++)
                        {
                            Array.Copy(source[ch][z], l * x_length * t_length, destination[ch][z], l * x_length * t_length, x_length * t_length);
                            //destination[ch][z][l] = MatrixCopy2D<ushort>(source[ch][z][l]);
                        }
                    }
                }
            }
        }
        public static void CopyDataLines(ushort[][][][][] source, ushort[][][][][] destination, int startLine, int endLine)
        {

            for (int ch = 0; ch < source.Length; ch++)
            {
                if (source[ch] != null)
                {
                    for (int z = 0; z < source[0].Length; z++)
                    {
                        for (int l = startLine; l < endLine; l++)
                        {
                            for (int x = 0; x < source[ch][z][l].Length; x++)
                                Array.Copy(source[ch][z][l], destination[ch][z][l], source[ch][z][l].Length);
                            //destination[ch][z][l] = MatrixCopy2D<ushort>(source[ch][z][l]);
                        }
                    }
                }
            }
        }

        static public void MatrixClear4D<T>(T[][][][] MatrixA)
        {
            for (int i = 0; i < MatrixA.Length; i++)
                for (int j = 0; j < MatrixA[i].Length; j++)
                    for (int k = 0; k < MatrixA[i][j].Length; k++)
                        Array.Clear(MatrixA[i][j][k], 0, MatrixA[i][j][k].Length);

        }

        static public void MatrixClear4D<T>(T[][,,] MatrixA)
        {
            for (int i = 0; i < MatrixA.Length; i++)
                Array.Clear(MatrixA[i], 0, MatrixA[i].Length);

        }

        static public T[][] MatrixCopy2D<T>(T[][] MatrixA)
        {
            int n_Ch = MatrixA.Length;
            int rows = MatrixA[0].Length;
            T[][] result = MatrixCreate2D<T>(n_Ch, rows);
            for (int ch = 0; ch < n_Ch; ch++)
                Array.Copy(MatrixA[ch], result[ch], rows);
            return result;
        }

        static public T[][][] MatrixCopy3D<T>(T[][][] MatrixA)
        {
            int NCols = MatrixA.Length;
            T[][][] result = new T[NCols][][];
            for (int i = 0; i < NCols; i++)
                result[i] = MatrixCopy2D<T>(MatrixA[i]);

            return result;
        }

        static public T[][] MatrixCreate2D<T>(int rows, int cols) //x and y
        {
            T[][] result = new T[rows][];
            for (int i = 0; i < rows; i++)
                result[i] = new T[cols];

            return result;
        }


        static public T[][][] MatrixCreate3D<T>(int rows, int cols, int third)
        {
            T[][][] result = new T[rows][][];

            for (int i = 0; i < rows; i++)
            {
                result[i] = MatrixCreate2D<T>(cols, third);
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
    }
}
