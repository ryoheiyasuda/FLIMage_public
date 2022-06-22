using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace MathLibrary
{
    public class Fitting
    {
        /// <summary>
        /// Non linear fitting.
        /// </summary>
        public class Nlinfit
        {
            public double[] beta0; //initial parameters
            public double[] beta; // final parameters.
            public double[] betaMax; //Max bound
            public double[] betaMin; //Min bound
            public bool[] fix; //fix the beta?
            public double[] x;
            public double[] y;
            public double[] weights;
            public double[][] Jt; //Jacobian, transversed (faster); 
            public double[] residual;
            public double[] fitCurve;
            public double xi_square;

            public double x_resolution;
            public double pulseInterval = 12; //nanoseconds

            public int maxiter = 100;
            public double betatol = 1e-6;
            public double rtol = 1e-8;

            int dimmension = 1;

            //For 2-dimmensional fitting.
            public double[,] x2;

            public Func<double[], double[], double[]> modelFunc; //
            public Func<double[], double[,], double[]> modelFunc2; //For multidimensional

            /// <summary>
            /// This is the most regular Nlinfit.
            /// </summary>
            /// <param name="beta1"></param>
            /// <param name="x1"></param>
            /// <param name="y1"></param>
            public Nlinfit(double[] beta1, double[] x1, double[] y1)
            {
                beta0 = beta1;
                x = x1;
                y = y1;
                fix = new bool[beta1.Length]; //all false.

                int n1 = x1.Length;

                dimmension = 1;
                weights = Enumerable.Repeat<double>(1.0, n1).ToArray();
                betaMax = Enumerable.Repeat(double.PositiveInfinity, beta1.Length).ToArray();
                betaMin = Enumerable.Repeat(double.NegativeInfinity, beta1.Length).ToArray();
            }


            /// <summary>
            /// three dimmensional mode of Nlinfit.
            /// </summary>
            /// <param name="beta1"></param>
            /// <param name="xy">[x, y]</param>
            /// <param name="z"></param>
            public Nlinfit(double[] beta1, double[,] xy, double[] z)
            {
                beta0 = beta1;
                x2 = xy;
                y = z;

                fix = new bool[beta1.Length]; //all false.

                int n0 = xy.GetLength(0);
                int n1 = xy.GetLength(1);
                int n = z.Length;

                dimmension = 2;
                weights = Enumerable.Repeat<double>(1.0, n).ToArray();
                betaMax = Enumerable.Repeat(double.PositiveInfinity, beta1.Length).ToArray();
                betaMin = Enumerable.Repeat(double.NegativeInfinity, beta1.Length).ToArray();
            }


            /// <summary>
            /// Apply weights = 1/sqrt(photons)
            /// </summary>
            public void PoisonWeights()
            {
                if (y == null)
                    return;

                int n = y.Length;
                weights = new double[n];

                for (int i = 0; i < n; i++)
                {
                    if (y[i] != 0)
                        weights[i] = 1.0 / Math.Sqrt(y[i]);
                    else
                        weights[i] = 1.0;
                }
            }

            /// <summary>
            /// Calculate Jacobian --- it calculate transposed form (Jt)
            /// </summary>
            /// <param name="beta1"></param>
            /// <param name="p_fit"></param>
            private void CalcJacobian(double[] beta1, int p_fit)
            {
                int p = beta.GetLength(0);
                int n = y.Length;

                double dStep = betatol * 0.01; //Perhaps make sense to have 100x resolution from betatol?
                double[] betaNew = new double[p];
                double delta = 0;

                Jt = MatrixCalc.MatrixCreate2D<double>(p_fit, n);
                double[] yfit;
                double[] yplus;

                int k = 0;
                for (int i = 0; i < p; i++)
                {
                    if (!fix[i])
                    {
                        betaNew = (double[])beta1.Clone();

                        delta = dStep * beta1[i];

                        if (delta == 0)
                            delta = dStep * MatrixCalc.Norm(beta1);

                        betaNew[i] = beta1[i] + delta;

                        if (dimmension == 1)
                        {
                            yfit = modelFunc(beta1, x);
                            yplus = modelFunc(betaNew, x);
                        }
                        else //2d.
                        {
                            yfit = modelFunc2(beta1, x2);
                            yplus = modelFunc2(betaNew, x2);
                        }

                        for (int j = 0; j < n; j++)
                            Jt[k][j] = (yplus[j] - yfit[j]) / delta * weights[j]; //deltaY/deltaB * weight

                        k++; //Count only (!fix[i]).
                    }
                }
            }

            /// <summary>
            /// Helper of GetStep. This adjusts for boundaries.
            /// </summary>
            /// <param name="lambda"></param>
            /// <param name="r">residence</param>
            /// <param name="beta">fitting parameters</param>
            /// <param name="step1">calculated steps</param>
            /// <returns></returns>
            private double[] CalcStep(double lambda, double[] r, double[] beta, out double[] step1)
            {
                double[] step = GetStep(r, lambda);
                double[] beta1 = new double[beta.Length];

                step1 = new double[beta.Length]; //Actual step. Start with all 0.

                int p = beta.Length;
                //Debug.WriteLine("Step:" + step[0] + ", " + step[1] + ", " + step[2] + ", " + step[3]);
                int k = 0;
                //Note that step is only for !fix[i].
                for (int i = 0; i < p; i++)
                {
                    if (!fix[i])
                    {
                        if (beta[i] + step[k] > betaMax[i] || beta[i] + step[k] < betaMin[i]) //bounce back if it exceeds the boundary.
                            step1[i] = -step[k];
                        else
                            step1[i] = step[k];
                        k++;
                    }

                    beta1[i] = beta[i] + step1[i];
                }

                return beta1;
            }


            /// <summary>
            /// Levenberg-Marquardt algorithm for nonlinear regression
            /// return error: success = 0;
            /// </summary>
            /// <returns></returns>
            public int Perform()
            {
                //We start with success. Later, if there is a problem, we change ret value.
                int ret = 0;

                int p = beta0.Length;
                int n = y.Length;

                if (p < 1 || n < 1)
                {
                    Debug.WriteLine("Wrong value sets");
                    return -1;
                }

                int p_fit = fix.Select(x => (!x) ? 1 : 0).ToArray().Sum();

                int iter = 0;

                beta = (double[])beta0.Clone();

                double lambda = 0.01;
                double eps = float.Epsilon; //Very small step. minimum step this program.
                double sqrteps = Math.Sqrt(eps);

                double[] r = new double[1];
                double sse = GetSSE(beta, out r);
                double sseold = sse;

                double[] beta1 = (double[])beta.Clone();
                double[] betaold = (double[])beta1.Clone();
                double[] rold = (double[])r.Clone();

                while (iter < maxiter)
                {
                    iter = iter + 1;
                    betaold = (double[])beta1.Clone();
                    rold = (double[])r.Clone();
                    sseold = sse;

                    CalcJacobian(beta1, p_fit);
                    beta1 = CalcStep(lambda, r, betaold, out double[] step);
                    sse = GetSSE(beta1, out r); //r (residual) is also returned. 

                    if (sse < sseold) //Lambda is good!
                    {
                        if (0.1 * lambda > eps) //make it smaller and smaller.
                            lambda = 0.1 * lambda;
                        else
                            lambda = eps;
                    }
                    else
                    {
                        //Diverging? try adjusting lambda.
                        while (sse > sseold || Double.IsNaN(sse))
                        {
                            lambda = 10 * lambda;

                            if (lambda > 1e16)
                            {
                                Debug.WriteLine("Break out");
                                ret = -3;
                                break;
                            }

                            beta1 = CalcStep(lambda, r, betaold, out step);
                            sse = GetSSE(beta1, out r);
                        }
                    }

                    //Debug.WriteLine("Beta:" + beta[0] + ", " + beta[1] + ", " + beta[2] + ", " + beta[3]);

                    if (Double.IsNaN(sse))
                    {
                        ret = -4;
                        Debug.WriteLine("Problem with sse = NaN. Iter:" + iter);
                        break;
                    }

                    if (ret != 0)
                    {
                        Debug.WriteLine("Problem with Breakout. Iter:" + iter);
                        break;
                    }

                    if (MatrixCalc.Norm(step) < betatol * (sqrteps + MatrixCalc.Norm(beta1)))
                    {
                        Debug.WriteLine("Finished by Betatol. Norm(step) = {0}, Iter: {1}", iter, MatrixCalc.Norm(step));
                        ret = 0;
                        break;
                    }

                    if (Math.Abs(sseold - sse) <= rtol * sse)
                    {
                        Debug.WriteLine("Finished by rtol. SSE difference = {0}, iter: {1}", Math.Abs(sseold - sse), iter);
                        ret = 0;
                        break;
                    }

                } //while

                if (ret != 0)
                {
                    beta = betaold;
                    r = rold;
                    sse = sseold;
                }
                else
                {
                    beta = beta1;
                }

                //Should not be necessary. Junst in case...
                for (int i = 0; i < beta.Length; i++)
                {
                    if (fix[i])
                        beta[i] = beta0[i];
                }


                if (dimmension == 1)
                    fitCurve = modelFunc(beta, x);
                else
                    fitCurve = modelFunc2(beta, x2);

                residual = r;
                xi_square = sse / (n - p);

                //Debug.WriteLine("StopWatch = " + sw.ElapsedMilliseconds + " ms");
                return ret;
            }

            public double[][] DirectJtJ(double[][] Jt)
            {
                var n = Jt[0].Length;
                var p = Jt.Length;
                var JMat = MatrixCalc.MatrixCreate2D<double>(p, p);
                for (int i = 0; i < p; i++)
                    for (int j = i; j < p; j++)
                    {
                        JMat[i][j] = MatrixCalc.ArrayDotProduct(Jt[i], Jt[j]);
                        if (i != j)
                            JMat[j][i] = JMat[i][j];
                    }
                return JMat;
            }


            /// <summary>
            /// Calculating Levenberg–Marquardt step-size.
            /// </summary>
            /// <param name="r">residual</param>
            /// <param name="lambda">slope parameter</param>
            /// <returns></returns>
            public double[] GetStep(double[] r, double lambda)
            {
                int p = Jt.Length; //beta length

                //var J = MatrixCalc.MatrixTranspose(Jt);
                //var JMatrix = MatrixCalc.MatrixProduct(Jt, J); 
                //Dot product. Final product = p x p.

                var JMatrix = DirectJtJ(Jt); //Same as above, but much faster.

                for (int i = 0; i < p; i++)
                    JMatrix[i][i] *= (1 + lambda);  //J'J + lambda*diag(J'J)

                
                var Jtr = MatrixCalc.MatrixProduct(Jt, r);

                //Solve delta for JMatrix*step = J'r
                var step = MatrixCalc.MatrixProduct(MatrixCalc.MatrixInverse(JMatrix), Jtr);

                //Solution with MathNet.Save as above equation.Same results.
                //This gives 1e-8 level similarity with the above, but still i like my solution.
                //It is fast and simple.
                //var JtrV = Vector<double>.Build.Dense(Jtr);
                //var JmatrixV = Matrix<double>.Build.DenseOfRowArrays(JMatrix);
                //var step = JmatrixV.Solve(JtrV).AsArray();

                return step;
            }


            /// <summary>
            /// Calculate squared sum of errors (SSE) and residuals (r).
            /// </summary>
            /// <param name="yfit"></param>
            /// <param name="ydata"></param>
            /// <param name="weight"></param>
            /// <param name="r"></param>
            /// <returns></returns>
            public double GetSSE(double[] beta, out double[] r)
            {
                int n = y.GetLength(0);

                double[] yfit;

                if (dimmension == 1)
                    yfit = modelFunc(beta, x);
                else
                    yfit = modelFunc2(beta, x2);

                r = new double[n];

                double sse = 0;
                for (int j = 0; j < n; j++)
                {
                    r[j] = (y[j] - yfit[j]) * weights[j]; //Residuals.
                    sse = sse + r[j] * r[j]; //SSE. Calculate at the same time.
                }

                return sse;
            }


        }// class nolin fit

    }


}
