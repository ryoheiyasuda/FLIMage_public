using MathLibrary;
using MicroscopeHardwareLibs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;

namespace FLIMage.HardwareControls
{
    //This class controls National Instruments cards.
    public class IOControls
    {
        const int nCyclePerClock_resonant = 8;

        public class Calibration
        {
            public ScanParameters State;
            public double[][] calibrationCurve;
            public double[][] calibrationCurveFit;
            public double[][] calibrationOutput;
            public bool[] success;
            public double[] noiseValue;
            public double contrastThreshold = 0.15;
            public double noiseThreshold = 0.20;
            public bool active = false;
            public double[][] beta;
            Calib calib;

            public Calibration(ScanParameters State_in, Shading shading_in)
            {
                State = State_in;
                success = new bool[State.Init.EOM_nChannels];
                createFalseCurves();
#if DEBUG
                calib = new Calib(State, this, shading_in);
                active = true;
#else
                try
                {
                    calib = new Calib(State, this, shading_in);
                    active = true;
                }
                catch (Exception E)
                {
                    Debug.WriteLine("Problem in calibration start: " + E.Message);
                    createFalseCurves();
                }
#endif
            }

            public void createFalseCurves()
            {
                beta = new double[State.Init.EOM_nChannels][];
                calibrationCurve = new double[State.Init.EOM_nChannels][];
                for (int i = 0; i < State.Init.EOM_nChannels; i++)
                {
                    calibrationCurve[i] = new double[101];

                    for (int j = 0; j < calibrationCurve[i].Length; j++)
                        calibrationCurve[i][j] = i / 101;

                    beta[i] = new double[4];
                    beta[i][0] = 0;
                    beta[i][1] = Math.PI / 2.0 / 1.0;
                    beta[i][2] = 0.1;
                    beta[i][3] = 0;
                }
            }

            public double[] readIntensity()
            {
                double[] result = new double[State.Init.EOM_nChannels];
                if (active)
                    result = calib.readIntensity();
                return result;
            }

            public void PutValue(double[] input)
            {
                if (active)
                    calib.EOM_AO.putValue_Single_EOM(input);
            }

            public double GetEOMVoltageByFitting(double percent, int ch)
            {
                double val = 0;
                if (beta != null && beta[ch] != null && success[ch])
                    val = MatrixCalc.InverseSinusoidal(beta[ch], percent);
                else
                    val = MatrixCalc.InverseSinusoidal(new double[] { 0, 3.14, 1, 0 }, percent);


                if (val > 2)
                    val = 2;
                if (val < -2)
                    val = -2;

                return val;
            }

            public void MakeCalibrationCurveFit()
            {
                calibrationCurveFit = new double[State.Init.EOM_nChannels][];
                for (int ch = 0; ch < calibrationCurve.Length; ch++)
                {
                    calibrationCurveFit[ch] = new double[101];
                    if (beta != null && beta[ch] != null)
                        for (int j = 0; j < 101; j++)
                            calibrationCurveFit[ch][j] = MatrixCalc.InverseSinusoidal(beta[ch], j);
                }
            }

            public bool[] calibrateEOM(bool plot)
            {
#if !DEBUG
                try
                {
#endif
                calib.noiseThreshold = noiseThreshold;
                calib.contrastThreshold = contrastThreshold;
                success = calib.calcibrateEOMs(plot);
                if (beta != null)
                {
                    beta = calib.beta;
                    calibrationCurve = calib.calibrationCurve;
                    MakeCalibrationCurveFit();
                    calibrationOutput = calib.calibrationOutput;
                    noiseValue = calib.noiseValue;
                }
                return success;
#if !DEBUG
                }
                catch (Exception E)
                {
                    Debug.WriteLine(E.Message);
                    active = false;

                    return new bool[State.Init.EOM_nChannels];
                }
#endif
            }
        }

        //Class to calibrate laser intensity and voltage applied to pockels cells.
        //Calibration curve is stored.
        public class Calib
        {
            public double[][] calibrationCurve;
            public double[][] calibrationOutput;
            public bool[] success;
            public double[] contrast;
            public double[] noiseValue;
            public double contrastThreshold = 0.15;
            public double noiseThreshold = 0.20;
            public ScanParameters State;
            public pockelAI EOM_AI;
            public AnalogOutput EOM_AO;
            public String DigitalUncagingShutterPort;
            public Shading shading;
            public double[][] beta;

            Calibration calib;

            public Calib(ScanParameters State_in, Calibration calib_in, Shading shading_in)
            {
                State = State_in;
                calib = calib_in;

                success = new bool[State.Init.EOM_nChannels];
                createFalseCurves();

                DigitalUncagingShutterPort = State.Init.MirrorAOBoard + "/port0/" + State.Init.DigitalShutterPort;
            }

            public void createFalseCurves()
            {
                calibrationCurve = new double[State.Init.EOM_nChannels][];
                for (int i = 0; i < State.Init.EOM_nChannels; i++)
                {
                    calibrationCurve[i] = new double[101];

                    for (int j = 0; j < calibrationCurve[i].Length; j++)
                        calibrationCurve[i][j] = i / 101;
                }
            }

            public double[] readIntensity()
            {
                if (EOM_AI != null)
                    return EOM_AI.getSingleValue();
                return null;
            }

            public bool[] calcibrateEOMs(bool plot)
            {
                //dioTrigger dio = new dioTrigger(State);
                EOM_AI = new pockelAI(State);
                EOM_AO = new AnalogOutput(State, shading, false, true, false, false);

                double maxV = 1.9;
                double minV = -0.1;
                int nChannels = State.Init.EOM_nChannels;
                double outputRate = 4000;
                int sampleN = 2000;
                int repeat = 4;

                if (State.Init.singleSampleCalib)
                {
                    sampleN = 500;
                }

                int addUncageChannel = 0;
                if (State.Init.AO_uncagingShutter)
                {
                    addUncageChannel = 1;
                    State.Init.uncagingShutter = new bool[State.Init.uncagingShutter.Length];
                    State.Init.uncagingShutter[State.Init.EOM_nChannels] = true;
                }

                double[][] inputValues = new double[nChannels + addUncageChannel][];
                for (int i = 0; i < nChannels; i++)
                {
                    inputValues[i] = new double[sampleN];
                    for (int j = 0; j < sampleN; j++)
                        inputValues[i][j] = (double)(j + 1) / (double)sampleN * (maxV - minV) + minV;
                }


                double[] inputValue = new double[nChannels + addUncageChannel];
                double[] outputValue = new double[nChannels];
                double[] maxValue = new double[nChannels];
                int[] maxInput = new int[nChannels];
                double[] minValue = new double[nChannels];
                int[] minInput = new int[nChannels];

                double[][] outputValues = new double[nChannels][];
                double[][] outputValuesN = new double[nChannels][];


                for (int i = 0; i < nChannels; i++)
                {
                    outputValues[i] = new double[sampleN];
                    outputValuesN[i] = new double[sampleN];
                }

                Debug.WriteLine("Calibration started...");
                /////
                ///
                for (int ch = 0; ch < nChannels; ch++)
                {
                    inputValue[ch] = inputValues[ch][0];
                }

                if (State.Init.AO_uncagingShutter)
                    inputValue[nChannels] = 5;
                if (State.Init.DO_uncagingShutter)
                    new Digital_Out(DigitalUncagingShutterPort, true);
                EOM_AO.putValue_Single_EOM(inputValue);



                if (State.Init.singleSampleCalib)
                {
                    for (int j = 0; j < sampleN; j++)
                    {
                        for (int ch = 0; ch < nChannels; ch++)
                            inputValue[ch] = inputValues[ch][j];

                        EOM_AO.putValue_Single_EOM(inputValue);
                        //System.Threading.Thread.Sleep(1);
                        double[] values = EOM_AI.getSingleValue();

                        for (int ch = 0; ch < nChannels; ch++)
                            outputValues[ch][j] = values[ch];
                    }
                }
                else
                {
                    double[,] input_values = new double[nChannels + addUncageChannel, sampleN * repeat];
                    for (int i = 0; i < nChannels + addUncageChannel; i++)
                        for (int j = 0; j < sampleN; j++)
                            for (int k = 0; k < repeat; k++)
                            {
                                if (i < nChannels)
                                    input_values[i, j * repeat + k] = inputValues[i][j];
                                else
                                    input_values[i, j * repeat + k] = 5;
                            }

                    dioTrigger dio = new dioTrigger(State);
                    bool syncClock = true;
                    int nSamples = input_values.GetLength(1);
                    var AIThread = System.Threading.Tasks.Task.Factory.StartNew((Action)delegate
                    {
                        EOM_AI.TurnExportClock(syncClock);
                        EOM_AI.SetupAI(nSamples + 1, outputRate); //It seems like there is 1 clock delay?
                        EOM_AI.Start();
                        EOM_AO.PutValue(input_values, outputRate, syncClock);
                        EOM_AO.Start();
                        if (State.Init.DO_uncagingShutter)
                            new Digital_Out(DigitalUncagingShutterPort, false);
                        dio.Evoke();
                    });

                    AIThread.Wait();

                    int timeout = (int)(nSamples / outputRate * 1000.0);
                    System.Threading.Thread.Sleep(timeout);

                    bool success1 = EOM_AI.WaitUntilDone(1000);
                    System.Threading.Thread.Sleep(5);
                    EOM_AO.Stop();

                    if (!success1)
                    {
                        EOM_AI.Dispose();
                        EOM_AO.Dispose();
                        EOM_AO.putValue_Single_EOM(inputValue);
                        return success;
                    }

                    var AIResult = EOM_AI.readSample();

                    ///
                    for (int ch = 0; ch < nChannels; ch++)
                    {
                        for (int j = 0; j < sampleN; j++)
                        {
                            for (int k = 0; k < repeat; k++)
                            {
                                if (k == 0)
                                    outputValues[ch][j] = AIResult[ch, j * repeat];
                                else
                                    outputValues[ch][j] += AIResult[ch, j * repeat + k];
                            }
                            outputValues[ch][j] /= repeat;
                        }
                    }

                    Debug.WriteLine("Calibration finished...");
                }


                for (int ch = 0; ch < nChannels; ch++)
                {
                    maxValue[ch] = outputValues[ch].Max();
                    maxInput[ch] = outputValues[ch].ToList().IndexOf(maxValue[ch]);
                    minValue[ch] = outputValues[ch].Min();
                    minInput[ch] = outputValues[ch].ToList().IndexOf(minValue[ch]);

                    State.Init.minimumPower[ch] = (int)(0.5 + 100 * minValue[ch] / maxValue[ch]);
                }

                for (int i = 0; i < sampleN; i++)
                {
                    for (int ch = 0; ch < nChannels; ch++)
                    {
                        outputValuesN[ch][i] = (outputValues[ch][i] - minValue[ch]) / (maxValue[ch] - minValue[ch]) * 100;
                    }
                }

                Array.Resize(ref outputValues, 2 * nChannels);
                Array.Resize(ref inputValues, 2 * nChannels);
                beta = new double[nChannels][];
                for (int ch = 0; ch < nChannels; ch++)
                {
                    beta[ch] = FittingVoltage(inputValues[ch], outputValues[ch]);
                    outputValues[ch + nChannels] = MatrixCalc.Sinusoidal(beta[ch], inputValues[ch]);
                    inputValues[ch + nChannels] = inputValues[ch];
                }

                // Not necessary!
                double[][] calibEOM = new double[nChannels][];
                double[][] calibEOMo = new double[nChannels][];
                for (int i = 0; i < nChannels; i++)
                {
                    calibEOM[i] = new double[101];
                    calibEOMo[i] = new double[101];
                }
                //double torr = 2;

                for (int ch = 0; ch < nChannels; ch++)
                {
                    int startP = minInput[ch];
                    int endP = maxInput[ch];
                    if (startP > endP)
                    {
                        int temp = startP;
                        startP = endP;
                        endP = temp;
                    }

                    for (int percentage = 0; percentage < calibEOM[ch].Length; percentage++)
                    {
                        Boolean existValue = false;
                        double nearValueDiff = 100;
                        int torrPos = 0;

                        for (int i = startP; i < endP; i++)
                        {
                            if ((int)(outputValuesN[ch][i] + 0.5) == percentage)
                            {
                                calibEOM[ch][percentage] = inputValues[ch][i];
                                calibEOMo[ch][percentage] = outputValues[ch][i];
                                existValue = true;
                                break;
                            }

                            if (Math.Abs(outputValuesN[ch][i] - percentage) < nearValueDiff)
                            {
                                nearValueDiff = Math.Abs(outputValuesN[ch][i] - percentage);
                                torrPos = i;
                            }
                        }
                        if (!existValue)
                        {
                            calibEOM[ch][percentage] = inputValues[ch][torrPos];
                            calibEOMo[ch][percentage] = outputValues[ch][torrPos];
                        }
                    }
                }


                calibrationCurve = calibEOM;
                calibrationOutput = calibEOMo;

                contrast = new double[nChannels];
                success = new bool[nChannels];

                for (int ch = 0; ch < nChannels; ch++)
                    contrast[ch] = Math.Abs(calibrationOutput[ch].Min() / calibrationOutput[ch].Max());

                double[][] noise = new double[nChannels][];
                noiseValue = new double[nChannels];

                int fw = 1; //look at +/- 1;
                for (int ch = 0; ch < nChannels; ch++)
                {
                    noise[ch] = new double[calibrationCurve[ch].Length - fw * 2 - 1];
                    double maxVo = calibrationCurve[ch].Max();
                    for (int i = 0; i < noise[ch].Length; i++)
                    {
                        noise[ch][i] = 0;
                        double[] cut1 = new double[fw * 2 + 1];
                        Array.Copy(calibrationCurve[ch], i, cut1, 0, cut1.Length);

                        noise[ch][i] = (cut1.Max() - cut1.Min()) / maxVo;
                    }
                    noiseValue[ch] = noise[ch].Max();
                }

                for (int ch = 0; ch < nChannels; ch++)
                {
                    success[ch] = (noiseValue[ch] < noiseThreshold && contrast[ch] < contrastThreshold);
                }

                for (int i = 0; i < nChannels; i++)
                {
                    inputValue[i] = calibEOM[i][0];
                }
                if (State.Init.AO_uncagingShutter)
                    inputValue[nChannels] = 0; //Close shutter.

                EOM_AI.Dispose();
                EOM_AO.Dispose();
                EOM_AO.putValue_Single_EOM(inputValue);
                if (State.Init.DO_uncagingShutter)
                    new Digital_Out(DigitalUncagingShutterPort, false);

                if (plot)
                {
                    String[] LegendStr = new String[nChannels * 2];
                    for (int i = 0; i < nChannels; i++)
                        LegendStr[i] = "Laser-" + (i + 1).ToString();
                    for (int i = nChannels; i < nChannels * 2; i++)
                        LegendStr[i] = "Fitting-" + (i - nChannels + 1).ToString();

                    var plot1 = new FLIMage.Plotting.Plot(inputValues, outputValues, "Applied voltage (V)", "Photodiode (V)", (double)sampleN / maxV, LegendStr);
                    plot1.Show();
                }


                return success;
            }

            private double[] FittingVoltage(double[] x, double[] y)
            {
                double maxValue = y.Max();
                int maxInput = y.ToList().IndexOf(maxValue);
                double minValue = y.Min();
                int minInput = y.ToList().IndexOf(minValue);
                double intervalX = x[maxInput] - x[minInput];
                double[] beta0 = new double[4];
                beta0[0] = minValue;
                beta0[1] = Math.PI / intervalX;
                beta0[2] = (maxValue - minValue);
                beta0[3] = x[minInput];

                Fitting.Nlinfit fit = new Fitting.Nlinfit(beta0, x, y);
                fit.modelFunc = ((betaA, xA) => MatrixCalc.Sinusoidal(betaA, xA));
                fit.Perform();
                return fit.beta;
            }
        }

        public class Shading
        {
            ScanParameters State;
            public float[][,] ShadingImages;
            public int width, height;
            public bool shading_on = false;
            public bool shading_uncaging = false;
            public Calibration calibration;
            public bool calibration_exist = false;
            double[][] calib1;
            double[][] calib_fit;

            public Shading(ScanParameters State_in)
            {
                State = State_in;
                calibration = new Calibration(State, this);
                ShadingImages = new float[State.Init.EOM_nChannels][,];
                applyCalibration(State);
            }

            public void AddShadingImageFromBinary(float[,] Image, int LaserN)
            {
                ShadingImages[LaserN] = Image;
                height = ShadingImages[LaserN].GetLength(0);
                width = ShadingImages[LaserN].GetLength(1);
            }

            public void AddShadingImage(FLIMData FLIM, int LaserN, int ShadingCh)
            {
                ShadingImages[LaserN] = ImageProcessing.ImageSmooth(FLIM.Project[ShadingCh], 10);
                float maxValue = (float)MatrixCalc.calcMax(ShadingImages[LaserN]);
                ShadingImages[LaserN] = MatrixCalc.DivideConstantFromMatrix(ShadingImages[LaserN], maxValue);
                ShadingImages[LaserN] = ImageProcessing.ImageSqrt(ShadingImages[LaserN]);
                ShadingImages[LaserN] = MatrixCalc.InverseMatrix(ShadingImages[LaserN]);
                State = FLIM.State;
                height = ShadingImages[LaserN].GetLength(0);
                width = ShadingImages[LaserN].GetLength(1);
            }

            public void applyCalibration(ScanParameters State_in)
            {
                State = State_in;
                calibration_exist = makeCalib(State, calibration, out calib1, out calib_fit);
            }

            public double getEOMVoltage_NoShading(double percent, int LaserN)
            {
                if (LaserN < State.Init.EOM_nChannels)
                {

                    double returnValue;
                    if (calib_fit != null && calibration.success[LaserN])
                        returnValue = calib_fit[LaserN][(int)percent];
                    else
                        returnValue = percent / 100;

                    return returnValue;

                    //return calibration.GetEOMVoltageByFitting(percent, LaserN);

                }
                else
                    return 0;
            }

            public double getZeroEOMVoltage(int LaserN)
            {
                if (LaserN < State.Init.EOM_nChannels)
                {
                    double returnValue = 0;
                    if (calib_fit != null && calibration.success[LaserN])
                        returnValue = calib1[LaserN][0];

                    if (returnValue > 2)
                        returnValue = 2;
                    else if (returnValue < -2)
                        returnValue = -2;
                    return returnValue;
                }
                else
                    return 0;
            }

            public double getEOMVoltage(double Xvol, double Yvol, int LaserN, double power, bool uncaging)
            {
                if (LaserN >= State.Init.EOM_nChannels)
                    return power / 100.0;

                double val = 1;
                double returnValue;
                if ((shading_on && !uncaging) || (shading_uncaging && uncaging) && calibration_exist)
                {
                    //Think about it!!
                    double maxX = State.Acq.XMaxVoltage; // It should be from the entire image. So, multiplicator is not necessary.
                    double maxY = State.Acq.YMaxVoltage; // It should be from the entire image. So, multiplicator is not necessary.

                    int xpixel = (int)((Xvol / maxX + 0.5) * width);
                    int ypixel = (int)((Yvol / maxY + 0.5) * height);

                    if (ShadingImages[LaserN] != null && ypixel >= 0 && xpixel >= 0 && ypixel < height && xpixel < width)
                        val = ShadingImages[LaserN][ypixel, xpixel];
                    if (val > 3)
                        val = 3;

                    returnValue = calibration.GetEOMVoltageByFitting(val * power, LaserN);
                }
                else
                {
                    if (calib_fit != null)
                        returnValue = calib_fit[LaserN][(int)power];
                    else
                        returnValue = power / 100;
                }

                if (returnValue > 2)
                    returnValue = 2;
                else if (returnValue < -2)
                    returnValue = -2;

                return returnValue;
            }

            //public double getEOMVoltage_Old(double X, double Y, int LaserN, double power, bool uncaging)
            //{
            //    double val = 1;
            //    double returnValue;
            //    if ((shading_on && !uncaging) || (shading_uncaging && uncaging) && calibration_exist)
            //    {
            //        int xpixel = (int)((X / State.Acq.XMaxVoltage + 0.5) * width);
            //        int ypixel = (int)((Y / State.Acq.YMaxVoltage + 0.5) * height);
            //        if (ShadingImages[LaserN] != null && shading_on)
            //            val = ShadingImages[LaserN][ypixel][xpixel];
            //    }

            //    returnValue = calib1[LaserN][(int)(val * power)];
            //    return returnValue;
            //}
        }

        //This class is to see if digital channel is on or off
        public class Digital_In
        {
            public Digital_In(String port_in, out bool signal)
            {
                var DI = new NiDaq.DigitalIn_SingleValue(port_in);
                signal = DI.readDI();
            }
        }

        public class Digital_Out
        {
            public Digital_Out(String port_in, bool signal)
            {
                new NiDaq.DigitalOut_SingleValue(port_in, signal);
            }
        }


        //THis class is to control shutter through digital IO
        public class ShutterCtrl
        {
            public String port;
            public ScanParameters State;

            public ShutterCtrl(ScanParameters State_in)
            {
                State = State_in;
                port = State.Init.shutterPort;
            }

            public void open()
            {
                try
                {
                    new Digital_Out(port, true);
                }
                catch (Exception EX)
                {
                    MessageBox.Show("Error in opening shutter: " + EX.Message);
                }
            }

            public void Close()
            {
                try
                {
                    new Digital_Out(port, false);
                }
                catch (Exception EX)
                {
                    MessageBox.Show("Error in closing shutter: " + EX.Message);
                }
            }
        }

        //This class is to send a DIO trigger signal from digital output defined by State.Init.triggerPort.
        public class dioTrigger
        {
            ScanParameters State;
            String port;
            public dioTrigger(ScanParameters State_in)
            {
                State = State_in;
                port = State.Init.triggerPort;
            }

            public void Evoke()
            {
                new Digital_Out(port, false);
                new Digital_Out(port, true);
                new Digital_Out(port, false);
            }
        }

        public class AO_Write
        {
            public AO_Write(String port, double value)
            {
                double[] range = new double[] { -10, 10 };
                if (value > -2)
                    range[0] = -2;
                else if (value > 0)
                    range[0] = 0;
                else if (value > 2)
                    range[0] = 2;

                //maxValue
                if (value < -2)
                    range[1] = -2;
                else if (value < 0)
                    range[1] = 0;
                else if (value < 2)
                    range[1] = 2;

                NiDaq.AO_Write ao_write = new NiDaq.AO_Write(port, value, range);
            }


            public AO_Write(String port, double value, double[] range)
            {
                NiDaq.AO_Write ao_write = new NiDaq.AO_Write(port, value, range);
            }
        }

        public class AI_Read_SingleValue
        {
            public AI_Read_SingleValue(String port, double[] range, out double result)
            {
                new NiDaq.AI_Read_SingleValue(port, range, out result);
            }
        }


        public static bool IfSameBoard_With_Mirror(String BoardName, ScanParameters State)
        {
            String[] sP;
            sP = State.Init.mirrorAOPortX.Split('/');
            String BoardMirror = sP[0];

            return (String.Compare(BoardMirror, BoardName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        static public double[] ConcatChannels(double[] DataA, double[] DataB)
        {
            if (DataA == null && DataB == null)
                return null;
            if (DataA == null)
                return DataB;
            if (DataB == null)
                return DataA;

            double[] result = new double[DataA.Length + DataB.Length];
            Array.Copy(DataA, result, DataA.Length);
            Array.Copy(DataB, 0, result, DataA.Length, DataB.Length);
            return result;
        }

        static public double[,] ConcatChannels(double[,] DataA, double[,] DataB)
        {
            if (DataA == null && DataB == null)
                return null;
            if (DataA == null)
                return DataB;
            if (DataB == null)
                return DataA;

            int nChB = DataB.GetLength(0);
            int nChA = DataA.GetLength(0);
            int nCh = nChA + nChB;
            int nSamplesA = DataA.GetLength(1);
            int nSamplesB = DataB.GetLength(1);
            int nSamples = nSamplesA;
            if (nSamples < nSamplesB)
                nSamples = nSamplesB; //whichever the bigger one.

            double[,] DataAll = new double[nCh, nSamples];

            if (nSamplesA == nSamplesB)
            {
                Buffer.BlockCopy(DataA, 0, DataAll, 0, Buffer.ByteLength(DataA));
                Buffer.BlockCopy(DataB, 0, DataAll, Buffer.ByteLength(DataA), Buffer.ByteLength(DataB));
            }
            else
            {
#if DEBUG
                DataAll = null; //Will be easier to catch a bug this way.
#else
                int byteLenthRowA = nSamplesA * sizeof(double);
                int byteLenthRowB = nSamplesB * sizeof(double);
                int byteLenthRow = nSamples * sizeof(double);
                for (int i = 0; i < nChA; i++)
                {
                    Buffer.BlockCopy(DataA, i * byteLenthRowA, DataAll, i * byteLenthRow, Buffer.ByteLength(DataA));
                }
                for (int i = 0; i < nChB; i++)
                {
                    Buffer.BlockCopy(DataB, i * byteLenthRowB, DataAll, (i + nChA) * byteLenthRow, Buffer.ByteLength(DataB));
                }
#endif
            }
            return DataAll;
        }


        static public bool[][] makeDigitalOutputAllChannels(ScanParameters State, bool includeClock, bool includeUncage, bool includeDigital, bool image_grabbing,
            bool image_focusing, out double outputRate, out int nSamples, out bool continous)
        {
            if (image_focusing)
            {
                includeUncage = false;
                includeDigital = false;
            }

            int AddClockC = includeClock ? 1 : 0;
            int AddUncage = includeUncage ? 1 : 0;
            int AddDigital = includeDigital ? State.DO.NChannels : 0;

            //Default output parameters.
            outputRate = 10000; //default rate. 0.1 ms.
            nSamples = 0;
            continous = false;

            if (AddDigital + AddUncage + AddClockC == 0)
                return null;

            if (includeDigital)
                outputRate = State.DO.outputRate;

            if (includeUncage)
                outputRate = State.Uncaging.outputRate;

            if (includeUncage && includeDigital)
                Math.Max(State.Uncaging.outputRate, State.DO.outputRate);

            if (includeClock && (image_grabbing || image_focusing))
                outputRate = State.Acq.outputRate;

            if (outputRate < 1000)
                outputRate = 10000;

            bool[][] waveform_output = new bool[AddDigital + AddUncage + AddClockC][];
            double totalLength_ms = State.DO.sampleLength;

            if (includeUncage)
                totalLength_ms = State.Uncaging.sampleLength;

            if (includeUncage && includeDigital)
                Math.Max(State.Uncaging.sampleLength, State.DO.sampleLength); //Need adjustment.

            nSamples = (int)(outputRate / 1000.0 * totalLength_ms);

            if (nSamples % 2 == 1) //NI-DAQ cannot wirte add number samples.
                nSamples += 1;

            double msPerLine = State.Acq.msPerLine;
            if (State.Acq.fastZScan)
                msPerLine = State.Acq.FastZ_msPerLine;

            if (image_focusing && includeClock) //This is only condition that continuous is true.
            {
                nSamples = (int)(outputRate / 1000.0 * msPerLine * State.Acq.linesPerFrame);
                continous = true;
            }

            if (image_grabbing && (includeDigital || includeUncage))
                nSamples = (int)(outputRate / 1000.0 * msPerLine * State.Acq.linesPerFrame * State.Acq.nFrames);

            if (includeClock) //nSamples will be changed.
            {
                int linePerCycle = State.Acq.linesPerFrame * State.Acq.nFrames;

                bool[] waveform = new bool[nSamples];

                if (!State.Init.lineClockAcitveHigh)
                    for (int i = 0; i < nSamples; i++)
                        waveform[i] = true;

                for (int i = 0; i < linePerCycle; i++)
                {
                    int loc = (int)(i * outputRate * msPerLine / 1000.0);
                    waveform[loc] = State.Init.lineClockAcitveHigh;
                }

                waveform_output[0] = waveform;
            }

            if (includeUncage)
            {
                if (State.Init.DO_uncagingShutter)
                    waveform_output[AddClockC] = MakeWaveFormForDigitalUncaging(State, outputRate, image_grabbing, nSamples);
                else //Never been reached.
                    waveform_output[AddClockC] = new bool[nSamples];
            }

            if (includeDigital)
            {
                for (int i = 0; i < State.DO.NChannels; i++)
                    waveform_output[i + AddUncage + AddClockC] = MakeWaveFormForDigitalOutputChannel(State, outputRate, image_grabbing, i, nSamples);
            }

            return waveform_output;
        }


        static public double[][] GetDigitalOutputInDouble(ScanParameters State, bool uncaging_on, bool digital_on, bool for_frame, out double outputRate)
        {
            bool clock_on = false;
            bool[][] digitalform = makeDigitalOutputAllChannels(State, clock_on, uncaging_on, digital_on, for_frame, false, out outputRate, out int nSamples, out bool cont);
            double[][] result = new double[digitalform.Length][];
            for (int i = 0; i < digitalform.Length; i++)
            {
                result[i] = new double[nSamples];
                for (int j = 0; j < digitalform[i].Length; j++)
                    result[i][j] = digitalform[i][j] ? 1.0 : 0.0;
            }
            return result;
        }

        static public bool[] MakeWaveFormForDigitalOutputChannel(ScanParameters State, double outputRate, bool forFrame, int channel, int nSamples)
        {
            int baseLine_train = (int)(State.DO.baselineBeforeTrain_forFrame * outputRate / 1000.0);
            int trainInterval = (int)(State.DO.pulseSetInterval_forFrame * outputRate / 1000.0);

            int nRepeat = State.DO.trainRepeat;
            if (!forFrame)
            {
                baseLine_train = 0;
                trainInterval = 0;
                nRepeat = 1;
            }

            int initialDelay_pulse = (int)(State.DO.pulseDelay[channel] * outputRate / 1000.0);
            int pulseWidth = (int)(State.DO.pulseWidth[channel] * outputRate / 1000.0);
            int pulseInterval = (int)(State.DO.pulseISI[channel] * outputRate / 1000.0);

            bool end_of_pulse = false;
            bool[] waveform = new bool[nSamples];
            if (!State.DO.active_high[channel])
                for (int i = 0; i < nSamples; i++)
                    waveform[i] = true;

            for (int train = 0; train < nRepeat; train++)
            {
                int train_Start = baseLine_train + train * trainInterval;
                int initialDelay = train_Start + initialDelay_pulse;

                for (int j = 0; j < State.DO.nPulses[channel]; j++)
                {
                    int pulseStart = initialDelay + j * pulseInterval;
                    int pulseEnd = initialDelay + j * pulseInterval + pulseWidth;

                    if (pulseStart < 0)
                        pulseStart = 0;

                    if (pulseEnd < 0)
                        pulseEnd = 0;

                    if (pulseStart >= nSamples || pulseEnd >= nSamples)
                    {
                        end_of_pulse = true;
                        break;
                    }

                    for (int i = pulseStart; i < pulseEnd; i++)
                        waveform[i] = State.DO.active_high[channel];
                }

                if (end_of_pulse)
                    break;
            }

            return waveform;
        }

        static public bool[] MakeWaveFormForDigitalUncaging(ScanParameters State, double outputRate, bool forFrame, int nSamples)
        {
            bool activeHigh = State.Uncaging.shutter_activeHigh;
            int baseLine_train = (int)(State.Uncaging.baselineBeforeTrain_forFrame * outputRate / 1000.0);
            int trainInterval = (int)(State.Uncaging.pulseSetInterval_forFrame * outputRate / 1000.0);

            int nRepeat = State.Uncaging.trainRepeat;
            if (!forFrame)
            {
                baseLine_train = 0;
                trainInterval = 0;
                nRepeat = 1;
            }

            int initialDelay_pulse = (int)((State.Uncaging.pulseDelay - State.Uncaging.DigitalShutter_delay) * outputRate / 1000.0);
            int pulseWidth = (int)((State.Uncaging.pulseWidth + State.Uncaging.DigitalShutter_delay) * outputRate / 1000.0);
            int pulseInterval = (int)(State.Uncaging.pulseISI * outputRate / 1000.0);

            bool end_of_pulse = false;
            bool[] waveform = new bool[nSamples];
            if (!activeHigh)
                for (int i = 0; i < nSamples; i++)
                    waveform[i] = true;


            for (int train = 0; train < nRepeat; train++)
            {
                int train_Start = baseLine_train + train * trainInterval;
                int initialDelay = train_Start + initialDelay_pulse;

                for (int j = 0; j < State.Uncaging.nPulses; j++)
                {
                    int pulseStart = initialDelay + j * pulseInterval;
                    int pulseEnd = initialDelay + j * pulseInterval + pulseWidth;

                    if (pulseStart < 0)
                        pulseStart = 0;

                    if (pulseEnd < 0)
                        pulseEnd = 0;

                    if (pulseStart >= nSamples || pulseEnd >= nSamples)
                    {
                        end_of_pulse = true;
                        break;
                    }

                    for (int i = pulseStart; i < pulseEnd; i++)
                        waveform[i] = activeHigh;
                }

                if (end_of_pulse)
                    break;
            }

            return waveform;
        }

        public class DigitalOutputControl
        {
            private String digitalLinePort;
            private String DO_ShutterPort = "";
            private String[] DO_Port;

            NiDaq.DigitalOutputSignal hDO;
            private String sampleClockPort = "";
            private String triggerPort = "";
            private ScanParameters State;
            private String Board;

            public DigitalOutputControl(ScanParameters State_in)
            {
                State = State_in;
                Board = State.Init.triggerPort.Split('/')[0];
                digitalLinePort = Board + "/port0/" + State.Init.DigitalLinePort;
                DO_ShutterPort = Board + "/port0/" + State.Init.DigitalShutterPort;
                DO_Port = new string[3];
                DO_Port[0] = Board + "/port0/" + State.Init.DigitalOutput1;
                DO_Port[1] = Board + "/port0/" + State.Init.DigitalOutput2;
                DO_Port[2] = Board + "/port0/" + State.Init.DigitalOutput3;
            }

            public void RestateScanParameters(ScanParameters State_in)
            {
                State = State_in;
            }

            public void PutValue(bool clock_on, bool uncaging_on, bool digital_on, bool grabbing, bool focusing)
            {
                bool include_uncaging = uncaging_on && State.Init.DO_uncagingShutter; //This is because clock...
                bool include_digital = digital_on;
                bool include_clock = clock_on;

                if (!include_uncaging && !include_digital && !include_clock)
                {
                    return;
                }

                bool[][] data1 = makeDigitalOutputAllChannels(State, include_clock, include_uncaging, include_digital, grabbing, focusing, out double outputRate, out int nSamples, out bool continuous);

                List<string> ports = new List<string>();
                if (include_clock)
                    ports.Add(digitalLinePort);
                if (include_uncaging)
                    ports.Add(DO_ShutterPort);
                if (include_digital)
                {
                    for (int j = 0; j < DO_Port.Length; j++)
                    {
                        if (State.DO.NChannels > j)
                            ports.Add(DO_Port[j]);
                    }
                }

                Stop();
                Dispose();
                hDO = new NiDaq.DigitalOutputSignal(ports.ToArray());
                triggerPort = State.Init.TriggerInput;
                if (State.Acq.externalTrigger)
                    triggerPort = State.Init.ExternalTriggerInputPort;
                sampleClockPort = "";

                hDO.PutValue(data1, outputRate, sampleClockPort, triggerPort, continuous);
            }

            public void Start()
            {
                if (hDO != null)
                {
                    hDO.Start();
                }
            }

            public void PutSingleValue(bool ON)
            {
                Dispose();

                List<string> ports = new List<string>();
                ports.Add(digitalLinePort);
                ports.Add(DO_ShutterPort);
                for (int j = 0; j < DO_Port.Length; j++)
                {
                    if (State.DO.NChannels > j)
                        ports.Add(DO_Port[j]);
                }

                hDO = new NiDaq.DigitalOutputSignal(ports.ToArray());

                bool[] data = new bool[2 + DO_Port.Length];
                data[0] = !State.Init.lineClockAcitveHigh; //uncage
                data[1] = !State.Uncaging.shutter_activeHigh; //uncage

                for (int j = 0; j < DO_Port.Length; j++)
                    data[2 + j] = !State.DO.active_high[j];

                if (ON)
                {
                    for (int i = 0; i < data.GetLength(0); i++)
                        data[i] = !data[i];
                }

                hDO.PutSingleValue(data); //Inclde dispose.

            }

            public void WaitUntilDone(int timeout)
            {
                if (hDO != null)
                {
                    hDO.WaitUntilDone(timeout);
                }
            }

            public void Stop()
            {
                if (hDO != null)
                {
                    hDO.Stop();
                }
            }

            public void Dispose()
            {
                if (hDO != null)
                    hDO.Dispose();
            }

        }

        /// <summary>
        /// It creates sample clock sync with Resonant clock.
        /// This produces re-triggerable sample clock.
        /// </summary>
        public class TriggeredLineClock
        {
            String outputPort;
            String clock_outputPort;
            String clock_export_destination;
            double inputRate;
            String LineClock_In;
            String board;
            ScanParameters State;
            NiDaq.DigitalOutputSignal digital_out;

            public TriggeredLineClock(ScanParameters State_in)
            {
                State = State_in;
                board = State.Init.ResonantAOBoard;
                LineClock_In = State.Init.ResonantClockInput_fromScanner;
                outputPort = State.Init.ResonantClockOutput_regulated;
                clock_outputPort = State.Init.ResonantClockOutput_divClock;
                clock_export_destination = State.Init.ResonantClockInput_from_divClock;
            }

            public void Start()
            {
                int nSamples = 8;
                inputRate = State.Init.resonantFreq_Hz;
                digital_out = new NiDaq.DigitalOutputSignal(new string[] { board + "/" + outputPort, board + "/" + clock_outputPort });
                digital_out.MakeLineTriggeredClock(nSamples, inputRate, nCyclePerClock_resonant, LineClock_In);
                //NiDaq.ConnectTerminals(clock_outputPort, clock_export_destination);
                digital_out.Start();
            }

            public void Stop()
            {
                if (digital_out != null)
                {
                    digital_out.Stop();
                    digital_out.Dispose();
                }
            }
        }

        public class AnalogOutput
        {
            public double outputRate;
            public String portX;
            public String portY;

            public String resonant_Zoom;

            public ScanParameters State;
            public Shading shading;

            public String[] AO_Ports;
            public NiDaq.AnalogOutput analog_output;

            public double[,] DataXY;
            public double[,] DataEOM;
            public double[,] DataAll;
            public double[,] DataZ;

            public event FrameDoneHandler FrameDone;
            public EventArgs e = null;
            public delegate void FrameDoneHandler(AnalogOutput mirrorAO, EventArgs e);

            double maxV_Mirror = 10;
            double minV_Mirror = -10;
            double maxV_EOM = 2;
            double minV_EOM = -2;
            double maxV_shutter = 5;
            double minV_shutter = 0;

            double zoom_max = 5;
            double zoom_min = 0;

            double[] maxVots;
            double[] minVolts;

            bool includeMirror = true;
            bool includeEOM = true;
            bool resonantScanning = false;
            bool includeZ = false;
            int addUncagingAO = 0;

            public AnalogOutput(ScanParameters State_in, Shading shading_in, bool includeMirror_in, bool EOM_in, bool for_resonant, bool include_z_in)
            {
                State = State_in;
                shading = shading_in;
                zoom_max = State.Acq.XMaxVoltage_Resonant;

                includeMirror = includeMirror_in;

                resonantScanning = for_resonant;

                includeZ = State.Init.usePiezo && include_z_in;

                if (resonantScanning)
                {
                    portX = State.Init.ResonantMirrorX;
                    portY = State.Init.ResonantMirrorY;
                    resonant_Zoom = State.Init.ResonantZoom;
                }
                else
                {
                    portX = State.Init.mirrorAOPortX;
                    portY = State.Init.mirrorAOPortY;
                }

                var portList = new List<string>();
                var maxValues = new List<double>();
                var minValues = new List<double>();

                if (includeMirror)
                {
                    portList.Add(portX);
                    portList.Add(portY);
                    maxValues.Add(maxV_Mirror);
                    maxValues.Add(maxV_Mirror);
                    minValues.Add(minV_Mirror);
                    minValues.Add(minV_Mirror);
                }

                if (State.Init.EOM_nChannels > 0 && EOM_in)
                {
                    for (int i = 0; i < State.Init.EOM_nChannels; i++)
                    {
                        String port = (String)State.Init.GetType().GetField("EOM_Port" + i).GetValue(State.Init);
                        portList.Add(port);
                        maxValues.Add(maxV_EOM);
                        minValues.Add(minV_EOM);
                    }
                }

                if (State.Init.AO_uncagingShutter && EOM_in)
                {
                    addUncagingAO = 1;
                    portList.Add(State.Init.UncagingShutterAnalogPort);
                    maxValues.Add(maxV_shutter);
                    minValues.Add(minV_shutter);
                }

                includeEOM = (State.Init.EOM_nChannels + addUncagingAO) > 0 && EOM_in;

                if (includeZ)
                {
                    portList.Add(State.Init.Piezo_Z_Signal);
                    maxValues.Add(State.Init.piezo_voltage_range.Max());
                    minValues.Add(State.Init.piezo_voltage_range.Min());
                }

                if (portList.Count > 0)
                {
                    AO_Ports = portList.ToArray();
                    maxVots = maxValues.ToArray();
                    minVolts = minValues.ToArray();
                    analog_output = new NiDaq.AnalogOutput(AO_Ports, maxVots, minVolts);
                }
                else
                {
                    AO_Ports = null;
                    analog_output = null;
                }
            }


            public void RestateScanParameters(ScanParameters State_in)
            {
                State = State_in;
                zoom_max = State.Acq.XMaxVoltage_Resonant;
            }

            public Double[,] PutValueResonantEOM_LineClockTriggered(Shading shading, double delay_microseconds)
            {
                double resonantOutputRate = State.Acq.resonantOutputRate; //Default 512 KHz. 512000
                int nSamples = (int)(resonantOutputRate / State.Init.resonantFreq_Hz * nCyclePerClock_resonant); //256K / 8K = 32
                double tickPeriod = 1e6 / resonantOutputRate; //microsecond.

                int EOM_nChannels = State.Init.EOM_nChannels;

                double startX1 = (1 - State.Acq.fillFraction) / 2;
                double endX1 = startX1 + State.Acq.fillFraction;

                var phases = new double[] { startX1 / 2, endX1 / 2, startX1 / 2 + 0.5, endX1 / 2 + 0.5 };

                int delay_tick = (int)Math.Floor(delay_microseconds / tickPeriod);
                double extra_delay = delay_microseconds - delay_tick * tickPeriod; //Should be always positive.

                if (delay_tick < -nSamples * phases[0])
                {
                    delay_tick = -(int)(nSamples * phases[0]);
                    extra_delay = 0;
                }

                if (delay_tick > nSamples * phases[0])
                {
                    delay_tick = (int)(nSamples * phases[0]);
                    extra_delay = 0;
                }

                int nSamplesXStart = (int)(nSamples * phases[0]) + delay_tick;
                int nSamplesXEnd = (int)(nSamples * phases[1] + 1) + delay_tick;
                int nSamplesXStart_Rev = (int)(nSamples * phases[2]) + delay_tick;
                int nSamplesXEnd_Rev = (int)(nSamples * phases[3] + 1) + delay_tick;

                nSamplesXEnd_Rev = Math.Min(nSamples, nSamplesXEnd_Rev); //Just in case...
                nSamples = Math.Min(nSamples, nSamplesXEnd_Rev + 1);
                var EOMOuput = new double[EOM_nChannels, nSamples];

                for (int ch = 0; ch < EOM_nChannels; ch++)
                {
                    double power = shading.getEOMVoltage_NoShading(State.Acq.power[ch], ch);
                    double power0 = shading.getZeroEOMVoltage(ch);

                    for (int i = 0; i < nSamplesXStart; i++)
                        EOMOuput[ch, i] = power0;

                    for (int i = nSamplesXStart; i < nSamplesXEnd; i++)
                        EOMOuput[ch, i] = power;

                    for (int i = nSamplesXEnd; i < nSamplesXStart_Rev; i++)
                        EOMOuput[ch, i] = power0;

                    for (int i = nSamplesXStart_Rev; i < nSamplesXEnd_Rev; i++)
                        EOMOuput[ch, i] = power;

                    for (int i = nSamplesXEnd_Rev; i < nSamples; i++)
                        EOMOuput[ch, i] = power0;

                    EOMOuput[ch, nSamples - 1] = power0;
                }

                analog_output.PutValue_RetriggerByLineClock(EOMOuput, resonantOutputRate, extra_delay, "", State.Init.ResonantClockInput_from_divClock, true, false);

                return EOMOuput;
            }

            public void setResonantZoom(double zoom)
            {
                double zoom1 = zoom_max / zoom;
                if (zoom1 > zoom_max)
                    zoom1 = zoom_max;
                if (zoom1 < zoom_min)
                    zoom1 = zoom_min;
                var ao_zoom = new NiDaq.AO_Write(resonant_Zoom, zoom1, new double[] { zoom_min, zoom_max });
            }

            public double[,] putValueResonantScan() //Only mirror.
            {
                if (analog_output == null || !resonantScanning)
                    return null;

                outputRate = State.Init.resonantFreq_Hz * nCyclePerClock_resonant;
                int nSamples = State.Acq.linesPerFrame / 2 * nCyclePerClock_resonant; //Bidirectional scan.
                int fly_back_last_line = nSamples - nCyclePerClock_resonant / 2;
                int line_length = nCyclePerClock_resonant / 2;
                if (includeMirror)
                {
                    int nSplit = State.Acq.nSplitScanning;

                    DataXY = new double[2, nSamples * 2];
                    double voltX = State.Acq.XOffset;
                    double voltY = State.Acq.YMaxVoltage / State.Acq.zoom * State.Acq.scanVoltageMultiplier[1];

                    if (nSplit <= 1)
                    {
                        for (int y = 0; y < nSamples; y++)
                            DataXY[0, y] = voltX;

                        for (int y = 0; y < nSamples; y++)
                            DataXY[1, y] = ((double)y / nSamples - 0.5) * voltY + State.Acq.YOffset;

                        //for (int y = nSamples; y < nSamples * 2; y++)
                        //    DataXY[1, y] = ((double)(nSamples * 2 - y) / nSamples - 0.5) * voltY + State.Acq.YOffset;
                    }
                    else
                    {
                        for (int p = 0; p < nSplit; p++)
                        {
                            var xpos = State.Acq.XOffset_Split[p];
                            var ypos = State.Acq.YOffset_Split[p];
                            var startY = (State.Acq.linesPerFrame / nSplit) * p;
                            var endY = State.Acq.linesPerFrame / nSplit * (p + 1);

                            for (int y = startY; y < endY; y++)
                            {
                                DataXY[0, y] = xpos;
                                DataXY[1, y] = ((double)(y - startY) / (endY - startY) - 0.5) * voltY + ypos;
                            }
                        }
                    }

                    for (int y = nSamples; y < nSamples * 2; y++)
                    {
                        DataXY[0, y] = DataXY[0, y - nSamples];
                        DataXY[1, y] = DataXY[1, y - nSamples];
                    }

                }
                else
                    DataXY = null;

                DataEOM = null;
                DataAll = ConcatChannels(DataXY, DataEOM);


                if (includeZ)
                {
                    if (State.Acq.scanZWithPiezo && State.Acq.nSlices > 1)
                    {
                        DataAll = ReplicateByFrameNumber(DataAll, State.Acq.nSlices / 2);
                        var nSamples_all = DataAll.GetLength(1);
                        DataZ = new double[1, nSamples_all];
                        var centerV = (State.Init.piezo_voltage_range.Max() + State.Init.piezo_voltage_range.Min()) / 2;
                        var startV = State.Acq.scanZWithPiezoRange_um[0] / State.Init.Piezo_um_per_V - centerV;
                        var endV = State.Acq.scanZWithPiezoRange_um[1] / State.Init.Piezo_um_per_V - centerV;
                        var stepV = (endV - startV) / State.Acq.nSlices;
                        for (int i = 0; i < State.Acq.nSlices; i++)
                        {
                            for (int j = 0; j < nSamples; j++)
                                DataZ[0, i * nSamples + j] = startV + stepV * i;
                        }
                        DataAll = ConcatChannels(DataAll, DataZ);
                    }
                    else
                    {
                        DataZ = new double[1, nSamples];
                        for (int j = 0; j < nSamples; j++)
                            DataZ[0, j] = State.Acq.currentPositionPiezo_V;
                    }
                }


                String triggerP = ""; // State.Init.TriggerInput;
                //if (State.Acq.externalTrigger)
                //    triggerP = State.Init.ExternalTriggerInputPort;

                String sampleClockP = State.Init.ResonantClockInput_from_divClock;

                var error = analog_output.PutValue(DataAll, outputRate, sampleClockP, triggerP, true, true);

                //var error = analog_output.PutValue(DataAll, outputRate, "", triggerP, true, false);

                if (error < 0)
                {
                    MessageBox.Show("Error in Put Value: error code: " + error);
                    return null;
                }

                if (includeMirror)
                {
                    analog_output.SetReturnFunction(DataXY.GetLength(1));
                    analog_output.EveryNSamplesEvent += EveryNSampleEvent;
                }
                return DataXY;
            }

            //Put value for simple scanning.
            public double[,] putValueScan(bool focus, bool shutter_open)
            {
                if (analog_output == null)
                {
                    MessageBox.Show("AO device is not setup correctly. Check your device file.");
                    return null;
                }

                outputRate = State.Acq.outputRate;
                if (includeMirror)
                {
                    try
                    {
                        DataXY = MakeMirrorOutputXY(State);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Problem in creating XY data. " + ex.Message);
                        DataXY = null;
                    }
                }
                else
                    DataXY = null;

                if (includeEOM)
                {
                    try
                    {
                        DataEOM = MakeEOMOutput(State, shading, focus, shutter_open); //focus will not change nSamples.
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Problem in creating EOM data. " + ex.Message);
                        DataEOM = null;
                    }
                }
                else
                    DataEOM = null;

                DataAll = ConcatChannels(DataXY, DataEOM);

                if (includeZ)
                {
                    var nSamples = DataAll.GetLength(1);
                    DataAll = ReplicateByFrameNumber(DataAll, State.Acq.nSlices * 2);
                    DataZ = calculateZMotion(State, nSamples, outputRate);
                    DataAll = ConcatChannels(DataAll, DataZ);
                }

                String triggerP = State.Init.TriggerInput;
                if (State.Acq.externalTrigger)
                    triggerP = State.Init.ExternalTriggerInputPort;

                String sampleClockP = State.Init.SampleClockPort;

                if (!State.Acq.flyBackBlancking)
                    sampleClockP = "";

                var error = analog_output.PutValue(DataAll, outputRate, sampleClockP, triggerP, true, false);
                if (error < 0)
                {
                    MessageBox.Show("Error in Put Value: error code: " + error);
                    return null;
                }

                if (includeMirror)
                {
                    analog_output.SetReturnFunction(DataXY.GetLength(1));
                    analog_output.EveryNSamplesEvent += EveryNSampleEvent;
                }
                return DataXY;
            }

            public void PutValue(double[,] values, double outputRate1, bool slaveMode)
            {
                String triggerP = State.Init.TriggerInput;
                if (State.Acq.externalTrigger)
                    triggerP = State.Init.ExternalTriggerInputPort;

                outputRate = outputRate1;
                int samplesPerChannel = (int)(values.GetLength(1));

                var sampleClockP = ""; //State.Init.SampleClockPort;

                var error = analog_output.PutValue(values, outputRate, sampleClockP, triggerP, false, slaveMode);

                if (error < 0)
                {
                    MessageBox.Show("Error in Put Value: error code: " + error);
                }
            }

            void EveryNSampleEvent(object sender, EventArgs e)
            {
                FrameDone?.Invoke(this, null);
            }

            /// <summary>
            /// Return true for success.
            /// </summary>
            /// <returns></returns>
            public bool putValueScanAndUncaging()
            {
                if (analog_output == null)
                {
                    MessageBox.Show("No AO output was created. Check the device setting.");
                    return false;
                }

                outputRate = State.Acq.outputRate;
                //if (includeZ)
                //{
                //    var nFrame2 = (int)Math.Ceiling((double)State.Acq.nFrames / State.Acq.nSlices / 2);
                //    State.Acq.nFrames = nFrame2 * State.Acq.nSlices * 2;
                //}

                if (includeMirror)
                {
                    try
                    {
                        DataXY = makeMirrorOutput_Imaging_Uncaging(State);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Problem in creating XY data! " + ex.Message);

                        DataXY = null;
                    }
                }
                else
                    DataXY = null;

                if (includeEOM)
                {
                    try
                    {
                        DataEOM = makeEOMOutput_Imaging_Uncaging(State, shading); //(State, calib, focus, shutter_open);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Problem in creating EOM data! " + ex.Message);
                        DataEOM = null;
                    }
                }
                else
                    DataEOM = null;

                if (DataEOM == null && DataXY == null)
                {
                    return false;
                }

                DataAll = ConcatChannels(DataXY, DataEOM);

                if (DataAll == null)
                    return false;

                if (includeZ)
                {
                    var nSamples_all = DataAll.GetLength(1);

                    DataZ = new double[1, nSamples_all];
                    DataAll = ConcatChannels(DataAll, DataZ);
                }

                String triggerP = State.Init.TriggerInput;
                if (State.Acq.externalTrigger)
                    triggerP = State.Init.ExternalTriggerInputPort;

                string sampleClockP = State.Init.SampleClockPort;

                if (!State.Acq.flyBackBlancking)
                    sampleClockP = "";

                var error = analog_output.PutValue(DataAll, outputRate, sampleClockP, triggerP, false, false);

                if (error < 0)
                {
                    MessageBox.Show("Error in Put Value: error code: " + error);
                    return false;
                }

                if (includeMirror)
                {
                    analog_output.SetReturnFunction(GetNSamplesScan(State));
                    analog_output.EveryNSamplesEvent += EveryNSampleEvent;
                }

                return true;
            }

            public double[,] putvalueUncageOnce() //lower sampling rate!!
            {
                if (analog_output == null)
                    return null;

                outputRate = State.Uncaging.outputRate;

                if (includeMirror)
                    DataXY = MakeUncagePulses_MirrorAO(State, outputRate);
                else
                    DataXY = null;

                if (includeEOM)
                    DataEOM = MakePockelsPulses_PockelsAO(State, outputRate, State.Init.AO_uncagingShutter, false, shading);
                else
                    DataEOM = null;

                DataAll = ConcatChannels(DataXY, DataEOM);

                String triggerP = State.Init.TriggerInput;
                if (State.Acq.externalTrigger)
                    triggerP = State.Init.ExternalTriggerInputPort;

                String sampleClockP = ""; //State.Init.SampleClockPort;

                var error = analog_output.PutValue(DataAll, outputRate, sampleClockP, triggerP, false, false);

                if (error < 0)
                {
                    MessageBox.Show("Error in Put Value: error code: " + error);
                    return null;
                }

                return DataAll;
            }

            public double[] mirrorStartPos()
            {
                double[,] values = new double[2, 1];
                double maxX = State.Acq.XMaxVoltage * State.Acq.scanVoltageMultiplier[0] / State.Acq.zoom / State.Acq.fillFraction;
                double maxY = State.Acq.YMaxVoltage * State.Acq.scanVoltageMultiplier[1] / State.Acq.zoom;
                if (State.Init.enableResonantScanner && State.Acq.resonantScanning)
                {
                    maxX = 0; //maxX * State.Acq.scanVoltageMultiplier[0] / State.Acq.zoom; To the center.
                    maxY = 0;
                }

                values[0, 0] = -0.5 * maxX; //Default position at left top corner for galvo.
                values[1, 0] = -0.5 * maxY;
                RotateAndOffset(values, State, 0);
                return new double[] { values[0, 0], values[1, 0] };
            }

            public void putValue_S_ToStartPos(bool zero_EOM, bool ao_shutter)
            {
                if (analog_output == null)
                    return;

                double[] xyValue = null;
                if (includeMirror)
                    xyValue = mirrorStartPos();

                double[] powerArray = null;
                if (includeEOM)
                    powerArray = EOM_Value(zero_EOM, ao_shutter);

                var dataAll = ConcatChannels(xyValue, powerArray);
                putValue_Single(dataAll);
            }

            public double[] EOM_Value(bool zero_EOM, bool ao_shutter)
            {
                double[] powerArray = new double[State.Init.EOM_nChannels + addUncagingAO];

                if (shading != null)
                    for (int i = 0; i < State.Init.EOM_nChannels; i++)
                    {
                        if (zero_EOM)
                            powerArray[i] = shading.calibration.GetEOMVoltageByFitting(0, i);   //calibration.calibrationCurve[i][State.Acq.power[0]];
                        else
                            powerArray[i] = shading.calibration.GetEOMVoltageByFitting(State.Acq.power[i], i);//calibrationCurve[i][State.Acq.power[i]];                                                                                                              //}
                    }
                if (addUncagingAO == 1)
                    powerArray[powerArray.Length - 1] = ao_shutter ? 5.0 : 0.0;

                return powerArray;
            }

            public void putValue_Single(double[] XY, bool zero_EOM, bool ao_shutter)
            {
                if (analog_output == null)
                    return;

                double[] xyValue = null;
                if (includeMirror)
                    xyValue = XY;

                double[] powerArray = null;
                if (includeEOM)
                    powerArray = EOM_Value(zero_EOM, ao_shutter);

                var dataAll = ConcatChannels(xyValue, powerArray);
                putValue_Single(dataAll);
            }

            private void putValue_Single(double[] values)
            {
                if (analog_output == null)
                    return;

                analog_output.PutValue_SingleValue(values);
            }

            public void putValue_Single_EOM(double[] EOMvalues)
            {
                if (analog_output == null)
                    return;

                double[] xyValue = null;
                if (includeMirror)
                    xyValue = mirrorStartPos();

                double[] powerArray = null;
                if (includeEOM)
                    powerArray = EOMvalues;

                var dataAll = ConcatChannels(xyValue, powerArray);
                putValue_Single(dataAll);
            }


            public void Start()
            {
                analog_output.Start();
            }

            public bool WaitUntilDone(int timeout)
            {
                return analog_output.WaitUntilDone(timeout);
            }

            public void Stop()
            {
                if (analog_output != null)
                    analog_output.Stop();
            }

            public void Dispose()
            {
                if (analog_output != null)
                    analog_output.Dispose();
            }
        } //MirrorAO

        public enum CurrentPosition
        {
            Start = 1,
            End = 2,
            Center = 3,
            InStack = 4,
        }

        public static double[,] calculateZMotion(ScanParameters State, int nSamples, double outputRate)
        {
            var nSamples_all = nSamples * State.Acq.nSlices * 2;
            var DataZ1 = new double[1, nSamples_all];
            var centerV = (State.Init.piezo_voltage_range.Max() + State.Init.piezo_voltage_range.Min()) / 2;
            var startV = State.Acq.scanZWithPiezoRange_um[0] / State.Init.Piezo_um_per_V - centerV;
            var endV = State.Acq.scanZWithPiezoRange_um[1] / State.Init.Piezo_um_per_V - centerV;
            if (State.Acq.nSlices < 1)
            {
                for (int j = 0; j < nSamples_all; j++)
                    DataZ1[0, j] = State.Acq.currentPositionPiezo_V;
                return DataZ1;
            }

            var stepV = (endV - startV) / (State.Acq.nSlices - 1);
            var movementTime = (int)(State.Init.PiezoSettlingTime_ms / 1000.0 * outputRate / 2); //10K / 1000 * 10 ~ 100 samples.
            for (int i = 0; i < State.Acq.nSlices * 2; i++)
            {
                var mov_before = startV;
                var move_during = startV;
                var move_after = startV + stepV / 2;
                if (i > 0 && i < State.Acq.nSlices)
                {
                    move_during = startV + stepV * i;
                    mov_before = move_during - stepV / 2;
                    if (i < State.Acq.nSlices - 1)
                        move_after = move_during + stepV / 2;
                    else //Final
                        move_after = endV;
                }
                else if (i >= State.Acq.nSlices)
                {
                    mov_before = endV;
                    move_during = endV;
                    move_after = endV - stepV / 2;

                    if (i > State.Acq.nSlices)
                    {
                        move_during = endV - stepV * (i - State.Acq.nSlices);
                        mov_before = move_during + stepV / 2;
                        if (i < State.Acq.nSlices * 2 - 1)
                            move_after = move_during - stepV / 2;
                        else
                            move_after = startV;
                    }
                }


                for (int j = 0; j < nSamples; j++)
                    DataZ1[0, i * nSamples + j] = move_during;

                for (int j = 0; j < movementTime; j++)
                {
                    DataZ1[0, i * nSamples + j]
                        = (move_during - mov_before) * j / movementTime + mov_before;
                }

                for (int j = 0; j < movementTime; j++)
                {
                    DataZ1[0, (i + 1) * nSamples - movementTime + j]
                        = (move_after - move_during) * j / movementTime + move_during;
                }

            }

            return DataZ1;
        }


        public class PiezoControl
        {
            ScanParameters State;
            String Port_AI;
            String Port_AO;
            NiDaq.AnalogOutput PiezoAO;
            double maxV = 10;
            double minV = 0;
            double centerV = 0;
            double current_voltage = 0;
            double current_voltage_feedback_signal = 0;
            public double time_for_movement_ms = 50;
            public double stack_start_um = 0;
            public double stack_end_um = 100;
            public CurrentPosition current_position = CurrentPosition.Center;
            object get_position_loc = new object();
            object set_position_loc = new object();

            public PiezoControl(ScanParameters State_in)
            {
                State = State_in;
                Port_AI = State.Init.Piezo_Z_Monitor;
                Port_AO = State.Init.Piezo_Z_Signal;
                maxV = State.Init.piezo_voltage_range.Max();
                minV = State.Init.piezo_voltage_range.Min();
                centerV = (State.Init.piezo_voltage_range[1] + State.Init.piezo_voltage_range[0]) / 2;
                current_voltage = 0;
                move_piezo_V_step(0);
                //System.Threading.Thread.Sleep(100);
                move_to_center();
            }

            public void CorrectOutOfRange()
            {
                double min_um = V_to_um(minV);
                double max_um = V_to_um(maxV);
                bool start_in_range = stack_start_um >= min_um && stack_start_um <= max_um;
                bool end_in_range = stack_end_um >= min_um && stack_end_um <= max_um;

                if (!start_in_range)
                {
                    if (stack_start_um > stack_end_um)
                        stack_start_um = max_um;
                    else
                        stack_start_um = min_um;
                }

                if (!end_in_range)
                {
                    if (stack_start_um > stack_end_um)
                        stack_end_um = min_um;
                    else
                        stack_end_um = max_um;
                }
            }

            public void move_to_center()
            {
                move_piezo_V(centerV);
                current_voltage_feedback_signal = getPosition_V();
            }


            public void move_piezo_V_step(double destination_V)
            {
                PiezoAO = new NiDaq.AnalogOutput(new string[] { Port_AO }, new double[] { maxV }, new double[] { minV });
                PiezoAO.PutValue_SingleValue(new double[] { destination_V });
                current_voltage = destination_V;
                PiezoAO.Dispose();
            }

            /// <summary>
            /// Return how much it moved in voltage.
            /// </summary>
            /// <param name="destination_V"></param>
            /// <returns></returns>
            public void move_piezo_V(double destination_V)
            {
                lock (set_position_loc)
                {
                    double outputRate = 10000;
                    var currentPos_V = current_voltage; // getPosition_V();
                    var movement_V = destination_V - currentPos_V;

                    int nSamples = (int)(time_for_movement_ms * outputRate / 1000.0); //100 samples

                    if (destination_V > maxV)
                        destination_V = maxV;
                    else if (destination_V < minV)
                        destination_V = minV;

                    PiezoAO = new NiDaq.AnalogOutput(new string[] { Port_AO }, new double[] { maxV }, new double[] { minV });

                    double[,] DataAll = new double[1, nSamples];
                    for (int i = 0; i < nSamples; i++)
                    {
                        double val1 = currentPos_V + (i + 1) * movement_V / nSamples; //(i+1) ensures it goes to the current_v.
                        if (val1 > maxV)
                            val1 = maxV;
                        else if (val1 < minV)
                            val1 = minV;

                        DataAll[0, i] = val1;
                    }

                    String triggerP = State.Init.TriggerInput;
                    if (State.Acq.externalTrigger)
                        triggerP = State.Init.ExternalTriggerInputPort;
                    String sampleClockP = ""; // State.Init.SampleClockPort;

                    var error = PiezoAO.PutValue(DataAll, outputRate, sampleClockP, "", false, false); //immediately move. Trigger is not necessary since it is only 1 channel.
                    PiezoAO.Start();
                    if (error < 0)
                    {
                        MessageBox.Show("Error in Put Value: error code: " + error);
                    }

                    PiezoAO.WaitUntilDone((int)time_for_movement_ms * 2);
                    //System.Threading.Thread.Sleep((int)time_for_movement_ms);


                    PiezoAO.Dispose();

                    current_voltage = destination_V;
                }
            }

            public double um_to_V(double um)
            {
                double voltage = um / State.Init.Piezo_um_per_V + centerV;
                if (voltage < State.Init.piezo_voltage_range.Min())
                    voltage = State.Init.piezo_voltage_range.Min();
                else if (voltage > State.Init.piezo_voltage_range.Max())
                    voltage = State.Init.piezo_voltage_range.Max();
                return voltage;
            }

            public double V_to_um(double voltage)
            {
                return (voltage - centerV) * State.Init.Piezo_um_per_V;
            }

            /// <summary>
            /// It moves piezo to destination (um). Return destination in um.
            /// </summary>
            /// <param name="destination_um"></param>
            /// <returns></returns>
            public double move_Piezo_um(double destination_um)
            {
                move_piezo_V(um_to_V(destination_um));
                return getPosition_um();
            }

            /// <summary>
            /// Move 1 step for the distance_um.
            /// return destination in um.
            /// </summary>
            /// <param name="distance_um"></param>
            /// <returns></returns>
            public double move_Piezo_1step_um(double distance_um)
            {
                var destination_um = distance_um + V_to_um(current_voltage);//getPosition_um();
                return move_Piezo_um(destination_um);
            }

            /// <summary>
            /// Get current piezo position in um.
            /// </summary>
            /// <returns></returns>
            public double getPosition_um()
            {
                current_voltage_feedback_signal = getPosition_V();
                return V_to_um(current_voltage_feedback_signal);
            }

            /// <summary>
            /// move piezo to zero and calculate how much it moved in um.
            /// </summary>
            /// <returns></returns>
            public double goto_center_um()
            {
                double org = getPosition_um();
                move_piezo_V(centerV);
                double dest = getPosition_um();
                return dest - org;
            }

            public double getPosition_V()
            {
                lock (get_position_loc)
                {
                    var ai = new NiDaq.AI_Read_SingleValue(Port_AI, new double[] { minV, maxV }, out double result);
                    current_voltage_feedback_signal = result;

                    return result;
                }
            }


        }

        static public double[,] DefinePulsePosition(ScanParameters State)
        {
            double[,] posXY = new double[2, State.Uncaging.nPulses];
            int pos = State.Uncaging.currentPosition; //pos = 0 is reserved for curent.

            for (int pulse = 0; pulse < State.Uncaging.nPulses; pulse++)
            {
                if (State.Uncaging.rotatePosition && State.Uncaging.multiUncagingPosition)
                {
                    int k = pulse % State.Uncaging.UncagingPositionsVX.Length;
                    posXY[0, pulse] = State.Uncaging.UncagingPositionsVX[k];
                    posXY[1, pulse] = State.Uncaging.UncagingPositionsVY[k];
                }
                else if (pos > 1 && State.Uncaging.multiUncagingPosition && State.Uncaging.UncagingPositionsVX.Length > pos - 1)
                {
                    posXY[0, pulse] = State.Uncaging.UncagingPositionsVX[pos - 1];
                    posXY[1, pulse] = State.Uncaging.UncagingPositionsVY[pos - 1];
                }
                else
                {
                    posXY[0, pulse] = State.Uncaging.PositionV[0];
                    posXY[1, pulse] = State.Uncaging.PositionV[1];
                }
            }
            return posXY;
        }


        static public double[,] MakeUncagePulses_MirrorAO(ScanParameters State, double outputRate)
        {
            double sDelay = State.Uncaging.AnalogShutter_delay;
            double pulseDelay = State.Uncaging.pulseDelay;
            double pulseISI = State.Uncaging.pulseISI;
            double pulseWidth = State.Uncaging.pulseWidth;
            int nPulses = State.Uncaging.nPulses;

            int samplesPerChannel = (int)(outputRate * State.Uncaging.sampleLength / 1000.0);

            if (samplesPerChannel % 2 == 1)
                samplesPerChannel += 1; //NI-DAQ cannot write odd number of samples.


            double[,] DataXY = new double[2, samplesPerChannel];

            double[,] posXY = DefinePulsePosition(State);

            for (int ch = 0; ch < 2; ch++)
            {
                int delayT = (int)(outputRate * (pulseDelay + pulseWidth) / 1000.0);
                for (int i = 0; i < delayT; i++)
                {
                    DataXY[ch, i] = posXY[ch, 0]; //Fills all with this first.
                                                  //Beginning. set to position 0.
                }

                int startT = delayT;
                for (int pulse = 1; pulse < nPulses; pulse++)
                {
                    int endT = delayT + (int)(outputRate * (pulseISI * pulse) / 1000.0);
                    if (endT > samplesPerChannel)
                        endT = samplesPerChannel;

                    for (int i = startT; i < endT; i++)
                    {
                        DataXY[ch, i] = posXY[ch, pulse]; //Right after finishing the previous pulse, moves to the next location.
                    }

                    if (endT == samplesPerChannel)
                        break;
                    startT = endT;
                }

                if (nPulses > 0)
                    for (int i = startT; i < samplesPerChannel; i++)
                    {
                        DataXY[ch, i] = posXY[ch, nPulses - 1]; //Keep the last one?
                    }
            }

            return DataXY;
        }

        static public bool makeCalib(ScanParameters State, Calibration calib, out double[][] calib1, out double[][] calib_fit)
        {
            //double[][] calib1;
            bool exist = (calib != null);
            if (exist)
            {
                calib1 = calib.calibrationCurve;
                calib_fit = calib.calibrationCurveFit;
            }
            else
            {
                calib1 = new double[State.Init.EOM_nChannels][];
                calib_fit = new double[State.Init.EOM_nChannels][];
                for (int i = 0; i < State.Init.EOM_nChannels; i++)
                {
                    calib1[i] = new double[101];
                    calib_fit[i] = new double[101];
                    for (int j = 0; j < calib1[i].Length; j++)
                    {
                        calib1[i][j] = (double)j / 101.0;
                        calib_fit[i][j] = (double)j / 101.0;
                    }
                }
            }

            return exist;
        }

        static public void MakeUncagingShutterPulse(ScanParameters State, double[,] DataEOM, int channel, double outputRate)
        {
            double sDelay = State.Uncaging.AnalogShutter_delay;
            double pulseDelay = State.Uncaging.pulseDelay;
            ////
            double pulseISI = State.Uncaging.pulseISI;
            double pulseWidth = State.Uncaging.pulseWidth;
            int nPulses = State.Uncaging.nPulses;

            int samplesPerChannel = DataEOM.GetLength(1); ///// will be more flexible this way.
            int totalNChannels = DataEOM.GetLength(0);
            int sampleBeforeUncage = (int)(outputRate * pulseDelay / 1000.0);

            int ch = channel;

            if (ch >= totalNChannels)
                return;

            for (int i = 0; i < samplesPerChannel; i++)
                DataEOM[ch, i] = 0.0;

            for (int pulse = 0; pulse < nPulses; pulse++)
            {
                // only during the pulse, pockels cells will open.
                int pulseStart = sampleBeforeUncage + (int)(outputRate * (pulse * pulseISI - sDelay) / 1000.0);
                bool done = false;
                if (pulseStart > samplesPerChannel)
                {
                    break;
                }

                int pulseEnd = pulseStart + (int)(outputRate * (pulseWidth + sDelay) / 1000.0);

                if (pulseEnd > samplesPerChannel)
                {
                    pulseEnd = samplesPerChannel;
                    done = true;
                }

                for (int i = pulseStart; i < pulseEnd; i++)
                    DataEOM[ch, i] = 5.0;

                if (done)
                    break;

                if (pulseISI - pulseWidth < sDelay * 2 && pulse + 1 < nPulses) //Fill the gap if it is too short.
                {
                    int NextPulseStart = sampleBeforeUncage + (int)(outputRate * ((pulse + 1) * pulseISI - sDelay) / 1000.0);
                    if (NextPulseStart < pulseEnd)
                    {
                        NextPulseStart = pulseEnd;
                        if (done)
                            break;
                    }
                    for (int i = pulseEnd; i < NextPulseStart; i++)
                    {
                        DataEOM[ch, i] = 5.0;
                    }
                }

                if (done)
                    break;
            } //Uncaging channel.

            DataEOM[ch, samplesPerChannel - 1] = 0.0; //Last sample is zero. Shutter closed!
        }

        static public double[,] MakePockelsPulses_PockelsAO(ScanParameters State, double outputRate, bool Shutter, bool for_frame, Shading shading)
        {
            double sDelay = State.Uncaging.AnalogShutter_delay;
            double pulseDelay = State.Uncaging.pulseDelay;
            double pulseISI = State.Uncaging.pulseISI;
            double pulseWidth = State.Uncaging.pulseWidth;
            int nPulses = State.Uncaging.nPulses;

            int addUncaging = 0;
            if (State.Init.AO_uncagingShutter && Shutter)
            {
                addUncaging = 1;
                State.Init.uncagingShutter = new bool[State.Init.uncagingShutter.Length];
                State.Init.uncagingShutter[State.Init.EOM_nChannels] = true;
            }

            int nChannels = State.Init.EOM_nChannels;

            int samplesShutterDelay = (int)(sDelay * outputRate / 1000.0);

            int nRepeat = 1;
            int repeatInterval = 0;
            if (for_frame)
            {
                nRepeat = State.Uncaging.trainRepeat;
                repeatInterval = (int)(State.Uncaging.pulseSetInterval_forFrame * outputRate / 1000.0);
            }

            double[,] DataEOM;

            int samplesPerChannel = (int)(outputRate * (State.Uncaging.sampleLength) / 1000.0);

            double msPerLine = State.Acq.msPerLine;
            if (State.Acq.fastZScan)
                msPerLine = State.Acq.FastZ_msPerLine;
            double frameInterval = ((double)State.Acq.linesPerFrame * msPerLine / 1000.0); //in seconds

            if (for_frame)
                samplesPerChannel = (int)(outputRate * frameInterval * State.Acq.nFrames);


            if (samplesPerChannel % 2 == 1) //NI-DAQ cannot wirte add number samples.
                samplesPerChannel += 1;

            int sampleBeforeUncage = (int)(outputRate * pulseDelay / 1000.0);

            int repeatTotalSamplesPerChannel = samplesPerChannel;
            int samplesPerRepeat = samplesPerChannel;
            int samplesBeforeRepeat = 0;
            if (for_frame)
            {
                samplesPerRepeat = (int)(outputRate * State.Uncaging.pulseSetInterval_forFrame / 1000.0);
                repeatTotalSamplesPerChannel = (int)(outputRate * (State.Uncaging.pulseSetInterval_forFrame * nRepeat + State.Uncaging.baselineBeforeTrain_forFrame) / 1000.0);
                if (repeatTotalSamplesPerChannel < samplesPerRepeat)
                {
                    repeatTotalSamplesPerChannel = samplesPerRepeat;
                }
                samplesBeforeRepeat = (int)(outputRate * State.Uncaging.baselineBeforeTrain_forFrame / 1000.0);
            }

            DataEOM = new double[nChannels + addUncaging, samplesPerChannel];

            double[,] posXY = DefinePulsePosition(State);
            //double[][] calib1 = makeCalib(State, calib);

            for (int ch = 0; ch < nChannels + addUncaging; ch++)
            {
                if (!State.Init.uncagingShutter[ch] && shading != null)
                {
                    for (int i = 0; i < samplesPerChannel; i++)
                        DataEOM[ch, i] = shading.getZeroEOMVoltage(ch); // calib1[ch][0]; //First put minimum to all of them.
                }

                for (int rep = 0; rep < nRepeat; rep++)
                {
                    if (State.Init.uncagingLasers[ch] || State.Init.uncagingShutter[ch])
                    {
                        for (int pulse = 0; pulse < nPulses; pulse++)
                        {
                            // only during the pulse, pockels cells will open.
                            bool done = false;
                            int pulseStart = sampleBeforeUncage + (int)(outputRate * pulse * pulseISI / 1000.0);
                            int pulseShutterStart = pulseStart - samplesShutterDelay;
                            if (pulseShutterStart > samplesPerChannel)
                            {
                                break;
                            }

                            int pulseEnd = pulseStart + (int)(outputRate * pulseWidth / 1000.0);
                            if (pulseEnd > samplesPerChannel)
                            {
                                pulseEnd = samplesPerChannel;
                                done = true;
                            }

                            pulseStart = pulseStart + samplesBeforeRepeat + rep * samplesPerRepeat;
                            pulseShutterStart = pulseShutterStart + samplesBeforeRepeat + rep * samplesPerRepeat;
                            pulseEnd = pulseEnd + samplesBeforeRepeat + rep * samplesPerRepeat;

                            if (State.Init.uncagingShutter[ch])
                            {
                                for (int i = pulseShutterStart; i < pulseEnd; i++)
                                {
                                    DataEOM[ch, i] = 5;
                                }
                            }

                            if (State.Init.uncagingLasers[ch] && shading != null)
                            {
                                for (int i = pulseStart; i < pulseEnd; i++)
                                {
                                    DataEOM[ch, i] = shading.getEOMVoltage(posXY[0, pulse], posXY[1, pulse], ch, State.Uncaging.Power, true);
                                }
                            }

                            if (done)
                                break;

                        }
                    }

                    else if (State.Init.imagingLasers[ch] && !State.Init.uncagingLasers[ch] && shading != null)
                    {
                        for (int i = 0; i < samplesPerChannel; i++)
                        {
                            if (State.Uncaging.TurnOffImagingDuringUncaging)
                                DataEOM[ch, i] = shading.getZeroEOMVoltage(ch); //all zero.
                        }
                    }


                    if (State.Init.uncagingShutter[ch])
                        DataEOM[ch, samplesPerChannel - 1] = 0;
                }  //repeat
            } //ch
            return DataEOM;
        }

        public class pockelAI
        {
            public String[] AI_Ports;
            public double[] maxVots;
            public double[] minVolts;
            public NiDaq.AnalogInput analog_input;
            public int nChannels;

            public int samplesPerTrigger;
            public ScanParameters State;
            public double maxV_EOM = 2;
            public double minV_EOM = -2;

            public pockelAI(ScanParameters State_in)
            {
                State = State_in;
                //triggerPort = State.Init.EOM_AI_Trigger;
                var portList = new List<string>();
                var maxValues = new List<double>();
                var minValues = new List<double>();

                if (State.Init.EOM_nChannels > 0)
                {
                    for (int i = 0; i < State.Init.EOM_nChannels; i++)
                    {
                        String port = (String)State.Init.GetType().GetField("EOM_AI_Port" + i).GetValue(State.Init);
                        portList.Add(port);
                        maxValues.Add(maxV_EOM);
                        minValues.Add(minV_EOM);
                    }
                }

                nChannels = State.Init.EOM_nChannels;

                if (portList.Count > 0)
                {
                    AI_Ports = portList.ToArray();
                    maxVots = maxValues.ToArray();
                    minVolts = minValues.ToArray();
                    analog_input = new NiDaq.AnalogInput(AI_Ports, maxVots, minVolts);
                }
                else
                {
                    AI_Ports = null;
                    analog_input = null;
                }
            }

            public bool WaitUntilDone(int timeout)
            {
                return analog_input.WaitUntilDone(timeout);
            }

            public void TurnExportClock(bool ON)
            {
                analog_input.ExportClock = ON;
            }

            public void SetupAI(int samplesPerChannel, double inputRate)
            {
                samplesPerTrigger = samplesPerChannel;
                var sampleClockP = ""; //State.Init.SampleClockPort
                analog_input.SetupAI(samplesPerTrigger, inputRate, sampleClockP, State.Init.TriggerInput, false);
            }

            public void Start()
            {
                bool externalTrigger = State.Acq.externalTrigger;
                string trig_port = State.Init.TriggerInput;
                if (externalTrigger)
                    trig_port = State.Init.ExternalTriggerInputPort;

                analog_input.Start(trig_port);
            }

            public double[] getSingleValue()
            {
                return analog_input.GetSingleValue();
            }

            public void Stop()
            {
                analog_input.Stop();
            }
            public void Dispose()
            {
                analog_input.Dispose();
            }

            public double[,] readSample()
            {
                var result = analog_input.ReadSample();
                return result;
            }
        }

        public class MiniScopeClock
        {
            public NiDaq.CounterOutput FClk;
            public NiDaq.CounterOutput SClk;
            public ScanParameters State;
            public bool running = false;
            public MiniScopeClock()
            {

            }
            public void Setup(ScanParameters State_in)
            {
                State = State_in;
                String FClk_Port = State.Init.FClkPort;
                String SClk_Port = State.Init.SClkPort;
                double FClkFreq = State.Acq.FClkFreq;
                double SClkFreq = State.Acq.SClkFreq;
                double dutyCycle = 0.5;

                double delay = State.Acq.LineClockDelay_us / 1.0e6;

                String TriggerPort = State.Init.TriggerInput;
                if (State.Acq.externalTrigger)
                    TriggerPort = State.Init.ExternalTriggerInputPort;

                Stop();
                Dispose();
                FClk = new NiDaq.CounterOutput(FClk_Port, TriggerPort, 0, FClkFreq, dutyCycle);
                SClk = new NiDaq.CounterOutput(SClk_Port, TriggerPort, 0, SClkFreq, dutyCycle);
            }

            public void Start()
            {
                FClk.Start();
                SClk.Start();
                running = true;
            }

            public void Stop()
            {

#if !DEBUG
                try
                {
#endif

                if (running)
                {
                    if (SClk != null)
                        SClk.Stop();
                    if (FClk != null)
                        FClk.Stop();
                }
                Dispose();
#if !DEBUG
                }
                catch (Exception Ex)
                {
                    Debug.WriteLine("Stop FClk or SClk failed: " + Ex.Message);
                }
#endif
            }

            public void Dispose()
            {
                if (FClk != null)
                    FClk.Dispose();
                if (SClk != null)
                    SClk.Dispose();

                running = false;
            }
        }



        public class LineClockByCounter
        {
            public NiDaq.CounterOutput lineClockCounter;
            public String lineClockPort;
            public String frameClockPort;
            public String TriggerPort;
            public Double msPerLine;
            public Double frequency;
            public Double freqFrame;
            public Double dutyCycle;
            public Double dutyFrame;
            public double delay;
            public int nLines;

            public ScanParameters State;
            public String Board;
            public String sampleClockPort;
            public bool running = false;

            public LineClockByCounter() //Constructor
            {
            }

            public void Setup(ScanParameters State_in)
            {
                State = State_in;
                lineClockPort = State.Init.lineClockPort;

                msPerLine = State.Acq.msPerLine;
                if (State.Acq.fastZScan)
                    msPerLine = State.Acq.FastZ_msPerLine;

                nLines = State.Acq.linesPerFrame * State.Acq.nFrames;

                frequency = 1000.0 / msPerLine;
                dutyCycle = 0.0001; //0.2 us

                freqFrame = 1000.0 / msPerLine / State.Acq.linesPerFrame;
                dutyFrame = dutyCycle / State.Acq.linesPerFrame;

                delay = State.Acq.LineClockDelay_us;

                TriggerPort = State.Init.TriggerInput;
                if (State.Acq.externalTrigger)
                    TriggerPort = State.Init.ExternalTriggerInputPort;

                Stop();
                Dispose();

                lineClockCounter = new NiDaq.CounterOutput(lineClockPort, TriggerPort, delay, frequency, dutyCycle);
            }

            public void Start()
            {
                lineClockCounter.Start();
                running = true;
            }

            public void Stop()
            {
#if !DEBUG
                try
                {
#endif
                if (lineClockCounter != null)
                {
                    if (running)
                        lineClockCounter.Stop();
                    running = false;
                    Dispose();
                }
#if !DEBUG
                }
                catch (Exception Ex)
                {
                    Debug.WriteLine("Stop LineClock failed: " + Ex.Message);
                }
#endif
            }

            public void Dispose()
            {
                if (lineClockCounter != null)
                    lineClockCounter.Dispose();
                running = false;
            }
        }

        public static double GetAcquisitionDelay_ms(ScanParameters State)
        {
            double msPerLine = State.Acq.msPerLine;
            if (State.Acq.resonantScanning)
                msPerLine = 500.0 / State.Init.resonantFreq_Hz;
            else if (State.Acq.fastZScan)
                msPerLine = State.Acq.FastZ_msPerLine;

            double ScanFraction = State.Acq.BiDirectionalScan ? 1 : State.Acq.scanFraction;
            double ScanDelay = 0;

            if (State.Acq.resonantScanning)
            {
                ScanDelay = State.Acq.resonantScanDelay_us / 1000.0; //convert to milliseconds.
                ScanFraction = 1;
            }
            else
            {
                ScanDelay = State.Acq.ScanDelay;
            }

            return ScanDelay + msPerLine * (1 - State.Acq.fillFraction / ScanFraction) / 2;
        }

        public static double GetBidirectionalDelay_ms(ScanParameters State)
        {
            if (State.Acq.resonantScanning)
                return GetAcquisitionDelay_ms(State) + 500.0 / State.Init.resonantFreq_Hz;
            else
                return GetAcquisitionDelay_ms(State); // GetAcquisitionDelay_ms(State) + State.Acq.ScanDelay2 / State.Acq.zoom;
        }

        public static double[,] MakeEOMOutput(ScanParameters State, Shading shading, bool Focus, bool shutter_open)
        {
            double NLines = (double)State.Acq.linesPerFrame;
            int NFrames = State.Acq.nFrames;
            double OutputRate = State.Acq.outputRate;
            double msPerLine = State.Acq.msPerLine;
            if (State.Acq.fastZScan)
                msPerLine = State.Acq.FastZ_msPerLine;
            double fillFraction = State.Acq.fillFraction; // + 0.02 * State.Acq.msPerLine;
            double ScanDelay = GetAcquisitionDelay_ms(State) + State.Acq.EOMDelay + State.Acq.LineClockDelay_us / 1000.0; // - 0.01 * State.Acq.msPerLine;
            int addUncaging = 0;

            if (State.Init.AO_uncagingShutter)
            {
                addUncaging = 1;
                State.Init.uncagingShutter = new bool[State.Init.uncagingShutter.Length];
                State.Init.uncagingShutter[State.Init.EOM_nChannels] = true;
            }

            bool[] imageLaser = State.Init.imagingLasers;
            bool[] uncageLaser = State.Init.uncagingLasers;
            int[] power = State.Acq.power;
            int EOM_nChannels = State.Init.EOM_nChannels;

            int nSamplesY = (int)(msPerLine * OutputRate * NLines / 1000.0);
            int nSamplesX = (int)(msPerLine * OutputRate / 1000.0);
            int nSamplesXStart = (int)(ScanDelay * OutputRate / 1000.0);
            int nSamplesXFill = (int)(nSamplesX * fillFraction);
            int nSamplesXEnd = nSamplesXFill + nSamplesXStart;
            int RoleOver = 0;
            if (nSamplesXEnd > nSamplesX)
            {
                nSamplesXEnd = nSamplesX;
                RoleOver = nSamplesXFill + nSamplesXStart - nSamplesXEnd;
            }

            double MaxVX = State.Acq.XMaxVoltage * State.Acq.scanVoltageMultiplier[0] / State.Acq.zoom; // * (LineTime - ScanDelay)/LineTime;
            double MaxVY = State.Acq.YMaxVoltage * State.Acq.scanVoltageMultiplier[1] / State.Acq.zoom;

            int nSamplesX_Rev = nSamplesX;
            double BiDirectionalDelay = GetBidirectionalDelay_ms(State);
            int nSamplesXStart_Rev = (int)(BiDirectionalDelay * OutputRate / 1000); //Should have + adjust.
            int nSamplesXFill_Rev = nSamplesXFill; //Correct.
            int nSamplesXEnd_Rev = nSamplesXFill_Rev + nSamplesXStart_Rev;
            int RoleOver_Rev = 0;
            //if (nSamplesXEnd_Rev > nSamplesX_Rev)
            //{
            //    nSamplesXEnd_Rev = nSamplesX_Rev;
            //    RoleOver_Rev = nSamplesXFill_Rev + nSamplesXStart_Rev - nSamplesXEnd_Rev;
            //}
            double value1 = 0;


            if (shutter_open)
                value1 = 5.0;

            double[,] EOMOuput = new double[EOM_nChannels + addUncaging, nSamplesY];


            for (int ch = 0; ch < EOM_nChannels + addUncaging; ch++)
                if (imageLaser[ch] || (uncageLaser[ch] && Focus))
                {
                    bool uncaging_cond = uncageLaser[ch] && Focus; //When focusing, one may want to see uncaging images....

                    //if we don't correct for shading, the value is always the same.
                    double vol = (double)power[ch] / 100.0;

                    if (shading != null)
                        vol = shading.getEOMVoltage(State.Acq.XOffset, State.Acq.YOffset, ch, power[ch], uncaging_cond);
                    double zero = 0;
                    if (shading != null)
                        zero = shading.getZeroEOMVoltage(ch);

                    int nSplitLine = State.Acq.nSplitScanning;

                    if (State.Acq.BiDirectionalScan)
                    {
                        for (int j = 0; j < NLines - 1; j += 2) //Even number.
                        {
                            double Y = (j / NLines - 0.5) * MaxVY;

                            for (int i = RoleOver_Rev; i < nSamplesXStart; i++)
                            {
                                if (State.Acq.flyBackBlancking)
                                    EOMOuput[ch, i + j * nSamplesX] = zero;
                                else
                                    EOMOuput[ch, i + j * nSamplesX] = vol;
                            }

                            if (shading != null && shading.shading_on)
                            {
                                int count = nSamplesXEnd - nSamplesXStart;
                                double[] Xa = Enumerable.Range(nSamplesXStart, count).
                                    Select(i => ((double)(i - nSamplesXStart) / (double)nSamplesXFill - 0.5) * MaxVX).ToArray();
                                double[] Ya = Enumerable.Repeat(Y, count).ToArray();
                                double[,] XYpos = new double[2, count];

                                Buffer.BlockCopy(Xa, 0, XYpos, 0, count * sizeof(double));
                                Buffer.BlockCopy(Ya, 0, XYpos, count * sizeof(double), count * sizeof(double));

                                RotateAndOffset(XYpos, State, j * nSplitLine / (int)NLines); //j: line. 

                                for (int i = nSamplesXStart; i < nSamplesXEnd; i++)
                                    EOMOuput[ch, i + j * nSamplesX] = shading.getEOMVoltage(XYpos[0, i], XYpos[1, i], ch, power[ch], uncaging_cond);
                            }
                            else
                            {
                                for (int i = nSamplesXStart; i < nSamplesXEnd; i++)
                                    EOMOuput[ch, i + j * nSamplesX] = vol;
                            }

                            for (int i = nSamplesXEnd; i < nSamplesX; i++)
                            {
                                if (State.Acq.flyBackBlancking)
                                    EOMOuput[ch, i + j * nSamplesX] = zero;
                                else
                                    EOMOuput[ch, i + j * nSamplesX] = vol;
                            }
                        }
                        for (int j = 1; j < NLines; j += 2) //Odd number.
                        {
                            double Y = (j / NLines - 0.5) * MaxVY;

                            for (int i = RoleOver; i < nSamplesXStart_Rev; i++)
                            {
                                if (State.Acq.flyBackBlancking)
                                    EOMOuput[ch, i + j * nSamplesX] = zero;
                                else
                                    EOMOuput[ch, i + j * nSamplesX] = vol;

                            }

                            if (shading != null && shading.shading_on)
                            {
                                int count = nSamplesXEnd_Rev - nSamplesXStart_Rev;
                                double[] Xa = Enumerable.Range(nSamplesXStart_Rev, count).
                                    Select(i => ((double)(nSamplesXFill - i + nSamplesXStart_Rev - 1) / (double)nSamplesXFill - 0.5) * MaxVX).ToArray();

                                double[] Ya = Enumerable.Repeat(Y, count).ToArray();
                                double[,] XYpos = new double[2, count];
                                Buffer.BlockCopy(Xa, 0, XYpos, 0, count * sizeof(double));
                                Buffer.BlockCopy(Ya, 0, XYpos, count * sizeof(double), count * sizeof(double));

                                RotateAndOffset(XYpos, State, j * nSplitLine / (int)NLines);

                                for (int i = nSamplesXStart; i < nSamplesXEnd; i++)
                                    EOMOuput[ch, i + j * nSamplesX] = shading.getEOMVoltage(XYpos[0, i], XYpos[1, i], ch, power[ch], uncaging_cond);
                            }
                            else
                            {
                                for (int i = nSamplesXStart_Rev; i < nSamplesXEnd_Rev; i++)
                                {
                                    EOMOuput[ch, i + j * nSamplesX] = vol;
                                }
                            }

                            for (int i = nSamplesXEnd_Rev; i < nSamplesX; i++)
                            {
                                if (State.Acq.flyBackBlancking)
                                    EOMOuput[ch, i + j * nSamplesX] = zero;
                                else
                                    EOMOuput[ch, i + j * nSamplesX] = vol;
                            }
                        }

                    }
                    else
                    {
                        for (int j = 0; j < NLines; j++)
                        {
                            double Y = (j / NLines - 0.5) * MaxVY;

                            for (int i = RoleOver; i < nSamplesXStart; i++)
                            {
                                if (State.Acq.flyBackBlancking)
                                    EOMOuput[ch, i + j * nSamplesX] = zero;
                                else
                                    EOMOuput[ch, i + j * nSamplesX] = vol;
                            }

                            if (shading != null && shading.shading_on)
                            {
                                int count = nSamplesXEnd - nSamplesXStart;
                                double[] Xa = Enumerable.Range(nSamplesXStart, count).
                                    Select(i => ((double)(i - nSamplesXStart) / (double)nSamplesXFill - 0.5) * MaxVX).ToArray();
                                double[] Ya = Enumerable.Repeat(Y, count).ToArray();
                                double[,] XYpos = new double[2, count];

                                Buffer.BlockCopy(Xa, 0, XYpos, 0, count * sizeof(double));
                                Buffer.BlockCopy(Ya, 0, XYpos, count * sizeof(double), count * sizeof(double));

                                RotateAndOffset(XYpos, State, j * nSplitLine / (int)NLines);

                                for (int i = nSamplesXStart; i < nSamplesXEnd; i++)
                                    EOMOuput[ch, i + j * nSamplesX] = shading.getEOMVoltage(XYpos[0, i], XYpos[1, i], ch, power[ch], uncaging_cond);

                            }
                            else
                            {
                                for (int i = nSamplesXStart; i < nSamplesXEnd; i++)
                                    EOMOuput[ch, i + j * nSamplesX] = vol;
                            }

                            for (int i = nSamplesXEnd; i < nSamplesX; i++)
                            {
                                if (State.Acq.flyBackBlancking)
                                    EOMOuput[ch, i + j * nSamplesX] = zero;
                                else
                                    EOMOuput[ch, i + j * nSamplesX] = vol;
                            }
                        }
                    }
                } //ImageLaser.               
                else if (uncageLaser[ch] && !Focus)
                {
                    double vol = 0.0;
                    if (shading != null)
                        vol = shading.getZeroEOMVoltage(ch);

                    for (int j = 0; j < NLines; j++)
                        for (int i = 0; i < nSamplesX; i++)
                        {
                            EOMOuput[ch, i + j * nSamplesX] = vol;
                        }
                }
                else if (State.Init.uncagingShutter[ch])
                {
                    for (int j = 0; j < NLines; j++)
                        for (int i = 0; i < nSamplesX; i++)
                            EOMOuput[ch, i + j * nSamplesX] = value1;
                }

            return EOMOuput;
        }

        public static double[,] makeEOMOutput_Imaging_Uncaging(ScanParameters State, Shading shading)
        {
            int nFrames = State.Acq.nFrames;
            double OutputRate = State.Acq.outputRate;

            double[,] OutputFrame = ReplicateByFrameNumber(MakeEOMOutput(State, shading, false, false), nFrames);
            int nChannels = OutputFrame.GetLength(0);
            int nSamples = OutputFrame.GetLength(1);

            double msPerLine = State.Acq.msPerLine;
            if (State.Acq.fastZScan)
                msPerLine = State.Acq.FastZ_msPerLine;

            double frameInterval = ((double)State.Acq.linesPerFrame * msPerLine / 1000.0); //in seconds

            bool anyUncaging = State.Init.uncagingLasers.Any(item => item == true);
            double sDelay = State.Uncaging.AnalogShutter_delay;
            if (!anyUncaging)
                sDelay = 0;

            int nUncaging = State.Uncaging.nPulses;
            int baseLineCount = (int)(State.Uncaging.pulseDelay * OutputRate / 1000.0);
            int intervalCount = (int)(State.Uncaging.pulseISI * OutputRate / 1000.0);

            int shutterDelayCount = (int)(State.Uncaging.AnalogShutter_delay * OutputRate / 1000.0);

            int shutterWidthCount = (int)((State.Uncaging.pulseWidth + State.Uncaging.AnalogShutter_delay) * OutputRate / 1000.0);
            int pockelsCloseCount = (int)(sDelay * OutputRate / 1000.0);

            int beforeTrainCount = (int)(State.Uncaging.baselineBeforeTrain_forFrame * OutputRate / 1000.0);
            int repeatPeriodCount = (int)((State.Uncaging.pulseSetInterval_forFrame) * OutputRate / 1000.0);

            double[,] uncagingPos = DefinePulsePosition(State);

            for (int ch = 0; ch < nChannels; ch++) //pulse On.
            {
                for (int repeat = 0; repeat < State.Uncaging.trainRepeat; repeat++)
                {
                    for (int pulse = 0; pulse < nUncaging; pulse++)
                        for (int j = 0; j < shutterWidthCount; j++)
                        {
                            int loc = beforeTrainCount + baseLineCount - shutterDelayCount + pulse * intervalCount + j + repeat * repeatPeriodCount;
                            if (loc < nSamples && loc >= 0)
                                if (State.Init.uncagingLasers[ch] && j >= pockelsCloseCount)
                                {
                                    if (shading != null)
                                        OutputFrame[ch, loc] = shading.getEOMVoltage(uncagingPos[0, pulse], uncagingPos[1, pulse], ch, State.Uncaging.Power, true); //calib1[ch][(int)State.Uncaging.Power];
                                    else
                                        OutputFrame[ch, loc] = State.Uncaging.Power / 100;
                                }
                                else if (State.Init.uncagingLasers[ch] && j < pockelsCloseCount)
                                {
                                    if (shading != null)
                                        OutputFrame[ch, loc] = shading.getZeroEOMVoltage(ch);
                                    else
                                        OutputFrame[ch, loc] = 0;
                                }
                                else if (State.Init.imagingLasers[ch] && anyUncaging && !State.Init.uncagingLasers[ch] && State.Uncaging.TurnOffImagingDuringUncaging)
                                {
                                    if (shading != null)
                                        OutputFrame[ch, loc] = shading.getZeroEOMVoltage(ch);
                                    else
                                        OutputFrame[ch, loc] = 0;
                                    //Imaging laser = 0, while uncaging. But only if uncaging laser is on.
                                    //In addition, if it is uncaging laser AND imaging laser, uncaging is priotized. (It can be used for FRAP etc).
                                }
                                else if (State.Init.uncagingShutter[ch])
                                    OutputFrame[ch, loc] = 5;

                        }
                }
            }
            return OutputFrame;
        }

        public static double[,] makeMirrorOutput_Imaging_Uncaging(ScanParameters State)
        {
            int nFrames = State.Acq.nFrames;
            double OutputRate = State.Acq.outputRate;

            double[,] OutputFrame = ReplicateByFrameNumber(MakeMirrorOutputXY(State), nFrames);
            int nChannels = OutputFrame.GetLength(0);
            int nSamples = OutputFrame.GetLength(1);

            double msPerLine = State.Acq.msPerLine;
            if (State.Acq.fastZScan)
                msPerLine = State.Acq.FastZ_msPerLine;

            double frameInterval = ((double)State.Acq.linesPerFrame * msPerLine / 1000.0); //in seconds

            int nTrain = State.Uncaging.trainRepeat;
            int nUncaging = State.Uncaging.nPulses;
            int uncaging_shutterDelayCount = (int)(State.Uncaging.Mirror_delay * OutputRate / 1000.0);
            int uncaging_delay = (int)(State.Uncaging.pulseDelay * OutputRate / 1000.0);

            int baseLineCount = (int)(State.Uncaging.FramesBeforeUncage * frameInterval * OutputRate) + uncaging_delay - uncaging_shutterDelayCount;
            int pulse_intervalCount = (int)(State.Uncaging.pulseISI * OutputRate / 1000.0);
            int train_intervalCount = (int)(State.Uncaging.Uncage_FrameInterval * frameInterval * OutputRate);

            int uncagingMoveCount = (int)(State.Uncaging.Mirror_delay / 2.0 * OutputRate / 1000.0); //Take some time for moving?
            int uncagingWidthCount = (int)((State.Uncaging.pulseWidth + State.Uncaging.Mirror_delay) * OutputRate / 1000.0);
            int PostUncagingCount = (int)(State.Acq.msPerLine / 2.0 * OutputRate / 1000.0); //Half of msPerLine.

            bool anyUncaging = State.Init.uncagingLasers.Any(item => item == true);

            double[,] uncagingPos = DefinePulsePosition(State);

            if (anyUncaging && State.Uncaging.MoveMirrorsToUncagingPosition)
            {
                for (int ch = 0; ch < nChannels; ch++) //pulse On.
                    for (int train = 0; train < nTrain; train++)
                        for (int pulse = 0; pulse < nUncaging; pulse++)
                        {
                            int baseCount = baseLineCount + train * train_intervalCount + pulse * pulse_intervalCount;

                            if (baseCount < nSamples)
                            {
                                double uncagePos1 = uncagingPos[ch, pulse];
                                double initialPos = OutputFrame[ch, baseCount];
                                int procEnd = baseCount + uncagingWidthCount + PostUncagingCount;

                                if (procEnd > nSamples)
                                    procEnd = nSamples;

                                PostUncagingCount = procEnd - uncagingWidthCount - baseCount;

                                double endPos = OutputFrame[ch, procEnd - 1];

                                double movIncrement = (uncagePos1 - initialPos) / uncagingMoveCount;
                                double postIncrement = (endPos - uncagePos1) / PostUncagingCount;


                                for (int j = 0; j < uncagingMoveCount; j++) //use 0.5 * mirror_delay to move.
                                {
                                    int loc = baseCount + j;
                                    if (loc >= 0 && loc < nSamples)
                                        OutputFrame[ch, loc] = initialPos + j * movIncrement;
                                }
                                for (int j = uncagingMoveCount; j < uncagingWidthCount; j++)
                                {
                                    int loc = baseCount + j;
                                    if (loc >= 0 && loc < nSamples)
                                        OutputFrame[ch, loc] = uncagePos1;
                                    //                                OutputFrame[ch, loc] = State.Uncaging.PositionV[ch];
                                }

                                if (PostUncagingCount > 0)
                                {
                                    for (int j = 0; j < PostUncagingCount; j++)
                                    {
                                        int loc = baseCount + uncagingWidthCount + j;
                                        if (loc < nSamples) //will be always >0
                                            OutputFrame[ch, loc] = uncagePos1 + j * postIncrement;
                                    }
                                }
                            }
                        }
            }

            return OutputFrame;
        }

        public static double[,] ReplicateByFrameNumber(double[,] OutputBase, int nFrames)
        {
            int nCh = OutputBase.GetLength(0);
            int nSample = OutputBase.GetLength(1);
            double[,] finalOutput = new double[OutputBase.GetLength(0), nSample * nFrames];

            for (int i = 0; i < nFrames; i++)
            {
                for (int ch = 0; ch < nCh; ch++)
                    for (int x = 0; x < nSample; x++)
                        finalOutput[ch, x + i * nSample] = OutputBase[ch, x];
            }

            return finalOutput;
        }

        public static int GetNSamplesScan(ScanParameters State)
        {
            double NLines = (double)State.Acq.linesPerFrame;
            double OutputRate = State.Acq.outputRate;
            double msPerLine = State.Acq.msPerLine;
            if (State.Acq.fastZScan)
                msPerLine = State.Acq.FastZ_msPerLine;
            int nSamples_All = (int)(msPerLine * OutputRate * NLines / 1000.0);
            return nSamples_All;
        }

        public static double[,] MakeMirrorOutputXY(ScanParameters State)
        {
            double NLines = (double)State.Acq.linesPerFrame;
            double OutputRate = State.Acq.outputRate;
            double msPerLine = State.Acq.msPerLine;
            if (State.Acq.fastZScan)
                msPerLine = State.Acq.FastZ_msPerLine;

            double ScanFraction = State.Acq.BiDirectionalScan ? 1 : State.Acq.scanFraction;
            double fillFraction = State.Acq.fillFraction;
            double ScanDelay = State.Acq.ScanDelay;
            double MaxVX = State.Acq.XMaxVoltage * State.Acq.scanVoltageMultiplier[0] / State.Acq.zoom / fillFraction; // * (LineTime - ScanDelay)/LineTime;
            double MaxVY = State.Acq.YMaxVoltage * State.Acq.scanVoltageMultiplier[1] / State.Acq.zoom;
            double xOffset = State.Acq.XOffset;
            double yOffset = State.Acq.YOffset;

            if (State.Init.UseExternalMirrorOffset)
            {
                xOffset = 0;
                yOffset = 0;
            }

            int nSamples_All = (int)(msPerLine * OutputRate * NLines / 1000.0);

            int nSamplesX = (int)(msPerLine * OutputRate / 1000.0);
            int nSamplesXScan = (int)(nSamplesX * ScanFraction);
            int nSamplesXFB = nSamplesX - nSamplesXScan;

            int nSamplesYFB = nSamplesXFB; // or nSamplesX if whole last line.


            int nSamplesYScan = nSamples_All - nSamplesYFB; // nSamplesX; // nSamplesXFB; Last Line?

            double[,] mirrorOutput = new double[2, nSamples_All];

            if (State.Acq.SineWaveScan)
            {
                for (int j = 0; j < NLines - 1; j += 2)
                {
                    for (int i = 0; i < nSamplesX; i++)
                        mirrorOutput[0, i + j * nSamplesX] = -MaxVX * 0.5 * Math.Cos((double)i * Math.PI / (double)nSamplesX);

                    for (int i = 0; i < nSamplesX; i++)
                        mirrorOutput[0, i + (j + 1) * nSamplesX] = MaxVX * 0.5 * Math.Cos((double)i * Math.PI / (double)nSamplesX);
                }

            }
            else if (State.Acq.BiDirectionalScan)
            {
                nSamplesXScan = (int)(nSamplesX * State.Acq.scanFraction);

                for (int j = 0; j < NLines - 1; j += 2) //Even
                {
                    for (int i = 0; i < nSamplesX; i++)
                        mirrorOutput[0, i + j * nSamplesX] = ((double)i / (double)nSamplesX - 0.5) * MaxVX;
                }

                for (int j = 1; j < NLines; j += 2) //Odd
                {
                    for (int i = 0; i < nSamplesX; i++)
                        mirrorOutput[0, i + j * nSamplesX] = ((double)(nSamplesX - i) / (double)nSamplesX - 0.5) * MaxVX;
                }

            }
            else //Normal scanning
            {
                nSamplesXScan = (int)(nSamplesX * State.Acq.scanFraction);

                double[] singleLineX = new double[nSamplesX];
                for (int i = 0; i < nSamplesXScan; i++)
                {
                    singleLineX[i] = ((double)i / (double)nSamplesXScan - 0.5) * MaxVX;
                }
                for (int i = nSamplesXScan; i < nSamplesX; i++)
                {
                    //Fly back.
                    singleLineX[i] = ((double)(nSamplesX - i) / (double)nSamplesXFB - 0.5) * MaxVX; //Usual.

                    //singleLineX[i] = MaxVX * 0.5 * Math.Cos((double)(i - nSamplesXScan) * Math.PI / (double)nSamplesXFB); //Sinusoidal wave.

                    //singleLineX[i] = ((double)(i - nSamplesX) / (double)nSamplesXScan - 0.5) * MaxVX; Very fast.
                }


                for (int j = 0; j < NLines; j++)
                    Buffer.BlockCopy(singleLineX, 0, mirrorOutput, j * nSamplesX * sizeof(double), nSamplesX * sizeof(double));

            }//Bidirectional

            for (int i = 0; i < nSamplesYScan; i++)
            {
                mirrorOutput[1, i] = ((double)i / (double)nSamplesYScan - 0.5) * MaxVY;
            }

            for (int i = nSamplesYScan; i < nSamples_All; i++)
            {
                mirrorOutput[1, i] = ((double)(nSamples_All - i) / (double)nSamplesYFB - 0.5) * MaxVY;
            }

            RotateAndOffset(mirrorOutput, State, -1);
            return mirrorOutput;
        }

        /// <summary>
        /// This can be used to convert pixel on image to mirror voltage for uncaging (uncaging only!!).
        /// </summary>
        /// <param name="xy_pixel"></param>
        /// <param name="State"></param>
        /// <returns></returns>
        public static double[] pixelOnImageToVoltage(double[] xy_pixel, ScanParameters State)
        {
            double[] frac = PixelsToFracOnScreen(xy_pixel, State);
            double[] voltage = PositionFracToVoltage(frac, State);
            return voltage;
        }


        //Process single voltage.
        public static double[] VoltageRotateOffset(double[] voltageXY, ScanParameters State, int split_pos)
        {
            double[,] voltagePosition = new double[2, 1];
            double[] voltagePosition_final = new double[2];
            voltagePosition[0, 0] = voltageXY[0];
            voltagePosition[1, 0] = voltageXY[1];

            RotateAndOffset(voltagePosition, State, split_pos);

            for (int i = 0; i < 2; i++)
                voltagePosition_final[i] = voltagePosition[i, 0];

            return voltagePosition_final;
        }

        public static double[] PixelsToFracOnScreen(double[] pixelLocationOnImage, ScanParameters State)
        {
            double[] Frac = new double[2];
            int nPixels = Math.Max(State.Acq.pixelsPerLine, State.Acq.linesPerFrame);
            double startX = (nPixels - State.Acq.pixelsPerLine) / 2.0;
            double startY = (nPixels - State.Acq.linesPerFrame) / 2.0;
            Frac[0] = (pixelLocationOnImage[0] + startX) / (double)nPixels;
            Frac[1] = (pixelLocationOnImage[1] + startY) / (double)nPixels;

            return Frac;
        }


        public static double[] PositionFracToVoltage(double[] FracPosition_On_Display, ScanParameters State)
        {
            double[,] voltagePosition = new double[2, 1];
            double[] voltagePosition_final = new double[2];

            if (!State.Acq.SineWaveScan)
            {
                double maxX = State.Acq.XMaxVoltage / State.Acq.zoom;
                double maxY = State.Acq.YMaxVoltage / State.Acq.zoom;

                voltagePosition[0, 0] = (FracPosition_On_Display[0] - 0.5) * maxX; //for disaply, multiplicator is not necessary.
                voltagePosition[1, 0] = (FracPosition_On_Display[1] - 0.5) * maxY;
            }
            else
            {
                double valX = Math.Sin(Math.PI / 4 * (State.Acq.fillFraction * (FracPosition_On_Display[0] - 0.5))) / State.Acq.fillFraction;
                double valY = (FracPosition_On_Display[1] - 0.5);
                //double valY = Math.Sin(FracPosition_On_Display[1] - 0.5);
                voltagePosition[0, 0] = valX * State.Acq.XMaxVoltage / State.Acq.zoom;
                voltagePosition[1, 0] = valY * State.Acq.YMaxVoltage / State.Acq.zoom;

            }

            int NSplit = State.Acq.nSplitScanning;
            double splitHeight = 1 / (double)NSplit;
            int split_pos = (int)Math.Floor(FracPosition_On_Display[1] * State.Acq.scanVoltageMultiplier[1] / splitHeight);

            RotateAndOffset(voltagePosition, State, split_pos);

            for (int i = 0; i < 2; i++)
                voltagePosition_final[i] = voltagePosition[i, 0] + State.Uncaging.CalibV[i];

            return voltagePosition_final;
        }

        public static double GetZeroOffset(ScanParameters State, int splitMirrorPosition)
        {
            int nSplit = State.Acq.nSplitScanning;
            var offset_correctionY_step = State.Acq.YMaxVoltage * State.Acq.scanVoltageMultiplier[1] / State.Acq.zoom / nSplit;

            double offset_correctionY = State.Acq.YMaxVoltage * State.Acq.scanVoltageMultiplier[1] / State.Acq.zoom / 2
                        - offset_correctionY_step / 2 - splitMirrorPosition * offset_correctionY_step; //1/2 - 1/8

            if (State.Acq.flipXYScan[1])
                offset_correctionY = -offset_correctionY;

            return -offset_correctionY;
        }


        /// <summary>
        /// If splitMirrorPosition is -1, it assume that mirrorOutput is for an entire scan.
        /// </summary>
        /// <param name="mirrorOutput"></param>
        /// <param name="State"></param>
        /// <param name="splitMirrorPosition"></param>
        public static void RotateAndOffset(double[,] mirrorOutput, ScanParameters State, int splitMirrorPosition)
        {
            double[] limit = new double[] { State.Init.AbsoluteMaxVoltageScan, State.Init.AbsoluteMaxVoltageScan };

            int nCh = mirrorOutput.GetLength(0);
            int len = mirrorOutput.GetLength(1);

            if (nCh != 2)
                return;

            if (State.Acq.nSplitScanning > 1) //This means split scanning.
            {
                int nSplit = State.Acq.nSplitScanning;

                if (splitMirrorPosition >= 0 && splitMirrorPosition < State.Acq.YOffset_Split.Length) //if specified.
                {
                    double offset_correctionY = -GetZeroOffset(State, splitMirrorPosition);
                    var offset = new double[] { State.Acq.XOffset_Split[splitMirrorPosition], State.Acq.YOffset_Split[splitMirrorPosition] };
                    for (int k = 0; k < len; k++)
                        mirrorOutput[1, k] += offset_correctionY;
                    MatrixCalc.RotateOffsetTimeSeries(mirrorOutput, State.Acq.Rotation_Split[splitMirrorPosition], offset, limit,
                        State.Acq.flipXYScan, State.Acq.switchXYScan);
                }
                else //Entire scanning.
                {
                    int nSamplesX = (int)(State.Acq.msPerLine * State.Acq.outputRate / 1000.0);
                    int nSamplesXScan = (int)(nSamplesX * State.Acq.scanFraction);
                    int nSamplesXFB = nSamplesX - nSamplesXScan;

                    var siz = sizeof(double);
                    var len_each = len / nSplit;

                    for (int j = 0; j < nSplit; j++)
                    {
                        double offset_correctionY = -GetZeroOffset(State, j);
                        var mirrorOutputSplit = new double[nCh, len / nSplit];
                        var buf_offset0 = len_each * j;

                        Buffer.BlockCopy(mirrorOutput, buf_offset0 * siz, mirrorOutputSplit, 0, len_each * siz);
                        Buffer.BlockCopy(mirrorOutput, (buf_offset0 + len) * siz, mirrorOutputSplit, len_each * siz, len_each * siz);

                        for (int k = 0; k < mirrorOutputSplit.GetLength(1); k++)
                            mirrorOutputSplit[1, k] += offset_correctionY;

                        var offset = new double[] { State.Acq.XOffset_Split[j], State.Acq.YOffset_Split[j] };
                        MatrixCalc.RotateOffsetTimeSeries(mirrorOutputSplit, State.Acq.Rotation_Split[j], offset, limit,
                            State.Acq.flipXYScan, State.Acq.switchXYScan);

                        Buffer.BlockCopy(mirrorOutputSplit, 0, mirrorOutput, buf_offset0 * siz, len_each * siz);
                        Buffer.BlockCopy(mirrorOutputSplit, len_each * siz, mirrorOutput, (buf_offset0 + len) * siz, len_each * siz);

                        if (j > 0)
                        {
                            int pos_bef1 = j * len_each - nSamplesXFB;
                            int pos_after1 = j * len_each;

                            int pos_after2 = pos_after1;
                            if (j == nSplit - 1)
                                pos_after2 = 0;

                            for (int x = 0; x < 2; x++)
                            {
                                double mirror_before = mirrorOutput[x, pos_bef1];
                                double mirror_after = mirrorOutput[x, pos_after2];
                                for (int k = pos_bef1; k < pos_after1; k++)
                                {
                                    //Just for easier debug.
                                    double val = mirror_before + (k - pos_bef1) * (mirror_after - mirror_before) / nSamplesXFB;
                                    mirrorOutput[x, k] = val;
                                }
                            }
                        }
                    } //nSplit

                    //Last
                    int pos_bef = nSplit * len_each - nSamplesXFB;
                    int pos_after = nSplit * len_each;

                    for (int x = 0; x < 2; x++)
                    {
                        double mirror_before = mirrorOutput[x, pos_bef];
                        double mirror_after = mirrorOutput[x, 0];
                        for (int k = pos_bef; k < pos_after; k++)
                        {
                            //Just for easier debug.
                            double val = mirror_before + (k - pos_bef) * (mirror_after - mirror_before) / nSamplesXFB;
                            mirrorOutput[x, k] = val;
                        }
                    }


                }
            }
            else //Not a split scanning.
            {
                double[] offset = new double[] { State.Acq.XOffset, State.Acq.YOffset };
                MatrixCalc.RotateOffsetTimeSeries(mirrorOutput, State.Acq.Rotation, offset, limit,
                    State.Acq.flipXYScan, State.Acq.switchXYScan);
            }
        }

    } //Class

} //Name space
