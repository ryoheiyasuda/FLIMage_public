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

            // Minimize the Poisson deviance (Cash statistic) instead of an
            // observed-count weighted least-squares approximation.
            public bool poissonMaximumLikelihood;

            public double x_resolution;

            public int maxiter = 100;
            public double rtol = 1e-8;
            double eps = float.Epsilon; //Can be double.Epsilon --- but this seems to be better.
            double  epss = Math.Sqrt(float.Epsilon);

            int dim1 = 1;

            //For 2-dim1al fitting.
            public double[,] x2;

            public Func<double[], double[], double[]> modelFunc; //
            public Action<double[], double[], double[]> modelFuncInPlace;
            public Action<double[], double[], double[][]> jacobianFuncInPlace;
            public Func<double[], double[,], double[]> modelFunc2; //For multidimnsional
            public Action<double[], double[,], double[]> modelFunc2InPlace;

            private double[] yfitWork;
            private double[] yplusWork;
            private double[] sseYfitWork;
            private double[] stepYfitWork;
            private double[] betaJacobianWork;
            private double[][] fullJacobianWork;

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

                dim1 = 1;
                weights = Enumerable.Repeat<double>(1.0, n1).ToArray();
                betaMax = Enumerable.Repeat(double.PositiveInfinity, beta1.Length).ToArray();
                betaMin = Enumerable.Repeat(double.NegativeInfinity, beta1.Length).ToArray();
            }


            /// <summary>
            /// three dim1al mode of Nlinfit.
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

                dim1 = 2;
                weights = Enumerable.Repeat<double>(1.0, n).ToArray();
                betaMax = Enumerable.Repeat(double.PositiveInfinity, beta1.Length).ToArray();
                betaMin = Enumerable.Repeat(double.NegativeInfinity, beta1.Length).ToArray();
            }


            /// <summary>
            /// Apply weights = 1/sqrt(photons)
            /// </summary>
            public void PoissonWeights()
            {
                if (y == null)
                    return;

                int n = y.Length;
                weights = new double[n];

                for (int i = 0; i < n; i++)
                {
                    // Be defensive: some pipelines can produce <=0 counts (e.g. background subtraction).
                    // sqrt(negative) => NaN, which makes SSE NaN and can massively slow the app due to
                    // repeated fitting + Output window spam during "calculate upon open".
                    if (y[i] > 0)
                        weights[i] = 1.0 / Math.Sqrt(y[i]);
                    else
                        weights[i] = 1.0;
                }
            }

            /// <summary>
            /// Fit photon counts by maximizing their Poisson likelihood.
            /// This avoids the low-count bias introduced by weights calculated
            /// from the noisy observed histogram.
            /// </summary>
            public void PoissonMaximumLikelihood()
            {
                poissonMaximumLikelihood = true;
                for (int i = 0; i < weights.Length; i++)
                    weights[i] = 1.0;
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

                Jt = GetJacobianBuffer(Jt, p_fit, n);

                if (dim1 == 1 && jacobianFuncInPlace != null)
                {
                    double[][] fullJt = GetJacobianBuffer(fullJacobianWork, p, n);
                    fullJacobianWork = fullJt;
                    jacobianFuncInPlace(beta1, x, fullJt);
                    double[] analyticYfit = poissonMaximumLikelihood
                        ? EvaluateModel(beta1, GetWorkBuffer(ref yfitWork, n))
                        : null;

                    int kFull = 0;
                    for (int i = 0; i < p; i++)
                    {
                        if (!fix[i])
                        {
                            double[] src = fullJt[i];
                            double[] dst = Jt[kFull];
                            for (int j = 0; j < n; j++)
                            {
                                double jacobianWeight = poissonMaximumLikelihood
                                    ? 1.0 / Math.Sqrt(Math.Max(analyticYfit[j], 1e-12))
                                    : weights[j];
                                double value = src[j] * jacobianWeight;
                                dst[j] = (Double.IsNaN(value) || Double.IsInfinity(value)) ? 0.0 : value;
                            }
                            kFull++;
                        }
                    }
                    return;
                }

                //double dStep = betatol * 0.01; //Perhaps make sense to have 100x resolution from betatol?
                double[] betaNew = GetWorkBuffer(ref betaJacobianWork, p);
                double delta = 0;
                double relStep = 1e-8;

                double[] yfit = EvaluateModel(beta1, GetWorkBuffer(ref yfitWork, n));
                double[] yplus;

                // If the model produces invalid values, avoid propagating NaNs through the Jacobian.
                if (yfit == null || yfit.Length < n)
                    return;
                for (int j = 0; j < n; j++)
                {
                    if (Double.IsNaN(yfit[j]) || Double.IsInfinity(yfit[j]) ||
                        Double.IsNaN(weights[j]) || Double.IsInfinity(weights[j]))
                        return;
                }

                int k = 0;
                for (int i = 0; i < p; i++)
                {
                    if (!fix[i])
                    {
                        Array.Copy(beta1, betaNew, p);

                        delta = relStep * (Math.Abs(beta1[i]) + 1.0); //dStep * beta1[i];
                        delta = Math.Max(delta, epss);

                        betaNew[i] = beta1[i] + delta;

                        yplus = EvaluateModel(betaNew, GetWorkBuffer(ref yplusWork, n));
                        if (yplus == null || yplus.Length < n)
                            return;

                        for (int j = 0; j < n; j++)
                        {
                            double yp = yplus[j];
                            if (Double.IsNaN(yp) || Double.IsInfinity(yp))
                                Jt[k][j] = 0;
                            else
                                Jt[k][j] = (yp - yfit[j]) / delta *
                                    (poissonMaximumLikelihood ? 1.0 / Math.Sqrt(Math.Max(yfit[j], 1e-12)) : weights[j]);
                        }

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
                        // Project the trial point onto the bounds. Discarding an
                        // entire component when it crossed a bound frequently
                        // left all parameters unchanged in Poisson fits.
                        double candidate = beta[i] + step[k];
                        if (candidate > betaMax[i])
                            candidate = betaMax[i];
                        else if (candidate < betaMin[i])
                            candidate = betaMin[i];
                        step1[i] = candidate - beta[i];
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

                int p_fit = CountFreeParameters();

                int iter = 0;

                beta = (double[])beta0.Clone();

                double lambda = 0.01;

                double[] r = new double[n];
                double sse = GetSSE(beta, r);
                double sseold = sse;

                double[] beta1 = (double[])beta.Clone();
                double[] betaold = new double[p];
                double[] rold = new double[n];
                double[] stepResidual = poissonMaximumLikelihood ? new double[n] : null;

                while (iter < maxiter)
                {
                    iter++;
                    Array.Copy(beta1, betaold, p);
                    Array.Copy(r, rold, n);   // residual at betaold
                    sseold = sse;

                    // Jacobian at betaold
                    CalcJacobian(betaold, p_fit);

                    // Fisher scoring for Poisson ML uses the Pearson working
                    // residual with the Fisher-weighted Jacobian. Deviance
                    // residuals remain appropriate for objective comparison and
                    // reporting, but using them as the step vector can stall LM.
                    double[] residualForStep = rold;
                    if (poissonMaximumLikelihood)
                    {
                        FillPoissonStepResidual(betaold, stepResidual);
                        residualForStep = stepResidual;
                    }

                    // First trial step from betaold
                    beta1 = CalcStep(lambda, residualForStep, betaold, out double[] step);
                    sse = GetSSE(beta1, r);    // r now at beta1

                    if (sse < sseold)
                    {
                        // good step: shrink lambda
                        lambda = Math.Max(0.1 * lambda, eps);
                    }
                    else
                    {
                        // bad step: increase lambda, but ALWAYS use rold
                        while (sse > sseold || Double.IsNaN(sse))
                        {
                            lambda *= 10.0;
                            if (lambda > 1e16)
                            {
                                // At extreme damping the LM step is below useful
                                // floating-point resolution. Different CPUs can
                                // round the trial SSE to either side of sseold, so
                                // accept the last finite state as convergence.
                                if (IsFinite(sseold) &&
                                    AllFinite(betaold) &&
                                    AllFinite(rold))
                                {
                                    Array.Copy(betaold, beta1, p);
                                    Array.Copy(rold, r, n);
                                    sse = sseold;
                                }
                                else
                                {
                                    ret = -3;
                                }
                                break;
                            }

                            // IMPORTANT: still step from betaold using *rold*
                            beta1 = CalcStep(lambda, residualForStep, betaold, out step);
                            sse = GetSSE(beta1, r);
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

                    if (Math.Abs(sseold - sse) <= rtol * Math.Max(sse, 1.0))
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


                fitCurve = new double[n];
                double[] finalCurve = EvaluateModel(beta, fitCurve);
                if (!Object.ReferenceEquals(finalCurve, fitCurve))
                    fitCurve = finalCurve;

                residual = r;

                int p2t = CountFreeParameters();
                xi_square = sse / (n - p2t);

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
                var Jtr = MatrixCalc.MatrixProduct(Jt, r);

                // Normalize the normal equations by the norm of each Jacobian
                // column. FLIM fits mix amplitudes of order 1e6 with rates of
                // order 1e-2; solving the raw equations can therefore lose the
                // amplitude step and report convergence far from a stationary
                // point. This transformation is algebraically equivalent to
                // diagonal Marquardt scaling, but is much better conditioned:
                //
                //   Hs = D^-1 (J'J) D^-1, gs = D^-1 J'r,
                //   Hs z = gs, step = D^-1 z.
                //
                // For an identifiable parameter Hs has a unit diagonal.
                var parameterScale = new double[p];
                for (int i = 0; i < p; i++)
                {
                    double diagonal = Math.Abs(JMatrix[i][i]);
                    double scale = Math.Sqrt(diagonal);
                    parameterScale[i] = IsFinite(scale) && scale > 1e-12
                        ? scale
                        : 1.0;
                }

                for (int i = 0; i < p; i++)
                {
                    Jtr[i] /= parameterScale[i];
                    for (int j = 0; j < p; j++)
                        JMatrix[i][j] /= parameterScale[i] * parameterScale[j];
                }

                const double normalizedDiagonalFloor = 1e-12;
                for (int i = 0; i < p; i++)
                    JMatrix[i][i] += lambda *
                        Math.Max(Math.Abs(JMatrix[i][i]), normalizedDiagonalFloor) +
                        normalizedDiagonalFloor;

                // Solve the normalized system without forming an explicit inverse,
                // then convert the step back to the original parameter units.
                var normalizedStep = MatrixCalc.MatrixSolve(JMatrix, Jtr);
                var step = new double[p];
                for (int i = 0; i < p; i++)
                    step[i] = normalizedStep[i] / parameterScale[i];

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
                r = new double[n];
                return GetSSE(beta, r);
            }

            private double GetSSE(double[] beta, double[] r)
            {
                int n = y.GetLength(0);
                const double InvalidSse = 1e300;

                double[] yfit = EvaluateModel(beta, GetWorkBuffer(ref sseYfitWork, n));

                if (yfit == null || yfit.Length < n)
                    return InvalidSse;

                double sse = 0;
                for (int j = 0; j < n; j++)
                {
                    double w = weights[j];
                    double yf = yfit[j];
                    if (Double.IsNaN(w) || Double.IsInfinity(w) ||
                        Double.IsNaN(yf) || Double.IsInfinity(yf))
                        return InvalidSse;

                    if (poissonMaximumLikelihood)
                    {
                        // The squared signed deviance residuals sum to twice
                        // the Poisson log-likelihood ratio (the Cash deviance).
                        yf = Math.Max(yf, 1e-12);
                        double deviance = y[j] > 0
                            ? 2.0 * (yf - y[j] + y[j] * Math.Log(y[j] / yf))
                            : 2.0 * yf;
                        r[j] = Math.Sign(y[j] - yf) * Math.Sqrt(Math.Max(0.0, deviance));
                    }
                    else
                        r[j] = (y[j] - yf) * w; //Residuals.
                    if (Double.IsNaN(r[j]) || Double.IsInfinity(r[j]))
                        return InvalidSse;

                    sse = sse + r[j] * r[j]; //SSE. Calculate at the same time.
                }

                return sse;
            }

            private int CountFreeParameters()
            {
                int count = 0;
                for (int i = 0; i < fix.Length; i++)
                    if (!fix[i])
                        count++;

                return count;
            }

            private static bool AllFinite(double[] values)
            {
                for (int i = 0; i < values.Length; i++)
                    if (!IsFinite(values[i]))
                        return false;

                return true;
            }

            private static bool IsFinite(double value)
            {
                return !Double.IsNaN(value) && !Double.IsInfinity(value);
            }

            private void FillPoissonStepResidual(double[] beta, double[] destination)
            {
                int n = y.Length;
                double[] yfit = EvaluateModel(beta, GetWorkBuffer(ref stepYfitWork, n));
                for (int i = 0; i < n; i++)
                {
                    double expected = Math.Max(yfit[i], 1e-12);
                    destination[i] = (y[i] - expected) / Math.Sqrt(expected);
                }
            }

            private double[] GetWorkBuffer(ref double[] buffer, int length)
            {
                if (buffer == null || buffer.Length != length)
                    buffer = new double[length];

                return buffer;
            }

            private static double[][] GetJacobianBuffer(double[][] buffer, int rows, int columns)
            {
                if (buffer == null || buffer.Length != rows ||
                    (rows > 0 && (buffer[0] == null || buffer[0].Length != columns)))
                    return MatrixCalc.MatrixCreate2D<double>(rows, columns);

                return buffer;
            }

            private double[] EvaluateModel(double[] beta, double[] destination)
            {
                if (dim1 == 1)
                {
                    if (modelFuncInPlace != null)
                    {
                        modelFuncInPlace(beta, x, destination);
                        return destination;
                    }

                    return modelFunc(beta, x);
                }

                if (modelFunc2InPlace != null)
                {
                    modelFunc2InPlace(beta, x2, destination);
                    return destination;
                }

                return modelFunc2(beta, x2);
            }


        }// class nolin fit

    }


}
