using MathLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using MicroscopeHardwareLibs;

namespace FLIMage.HardwareControls
{
    //This class controls National Instruments cards.
    public class IOControls
    {

        //Static method to export base clock from master card to slave. Used when State.Init.MasterClockPort and State.Init.SlaveClockPort are defined.
        //Does not work in version 17. Sample clock will be used instead.
        public static void exportBaseClockSignal(ScanParameters State)
        {
            if (State.Init.EOM_slaveClockPort != "")
            {
                NiDaq.exportBaseClockSignal(State.Init.masterClock, State.Init.masterClockPort);
                Debug.WriteLine("***Terminal: " + State.Init.masterClock + " was connected with terminal: " + State.Init.masterClockPort + "***");
            }
        }


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
                return MatrixCalc.InverseSinusoidal(beta[ch], percent);
            }

            public void MakeCalibrationCurveFit()
            {
                calibrationCurveFit = new double[State.Init.EOM_nChannels][];
                for (int ch = 0; ch < calibrationCurve.Length; ch++)
                {
                    calibrationCurveFit[ch] = new double[101];
                    for (int j = 0; j < 101; j++)
                        calibrationCurveFit[ch][j] = MatrixCalc.InverseSinusoidal(beta[ch], j);
                }
            }

            public bool[] calibrateEOM(bool plot)
            {
                try
                {
                    calib.noiseThreshold = noiseThreshold;
                    calib.contrastThreshold = contrastThreshold;
                    success = calib.calcibrateEOMs(plot);
                    beta = calib.beta;
                    calibrationCurve = calib.calibrationCurve;
                    MakeCalibrationCurveFit();
                    calibrationOutput = calib.calibrationOutput;
                    noiseValue = calib.noiseValue;
                }
                catch (Exception E)
                {
                    Debug.WriteLine(E.Message);
                    active = false;
                }

                return success;
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

                EOM_AI = new pockelAI(State);
                EOM_AO = new AnalogOutput(State, shading, false);

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
                return EOM_AI.getSingleValue();
            }

            public bool[] calcibrateEOMs(bool plot)
            {
                //dioTrigger dio = new dioTrigger(State);
                EOM_AO = new AnalogOutput(State, shading, false);

                double maxV = 2.0;
                double minV = 0;
                int nChannels = State.Init.EOM_nChannels;
                int sampleN = 1000;
                int repeat = 4;

                int addUncageChannel = 0;
                if (State.Init.AO_uncagingShutter)
                    addUncageChannel = 1;

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
                dioTrigger dio = new dioTrigger(State);
                for (int ch = 0; ch < nChannels; ch++)
                {
                    inputValue[ch] = inputValues[ch][0];
                }

                if (State.Init.AO_uncagingShutter)
                    inputValue[nChannels] = 5;

                if (State.Init.DO_uncagingShutter)
                    new Digital_Out(DigitalUncagingShutterPort, true);

                EOM_AO.putValue_Single_EOM(inputValue);
                double[,] values = new double[nChannels + addUncageChannel, sampleN * repeat];


                for (int i = 0; i < nChannels + addUncageChannel; i++)
                    for (int j = 0; j < sampleN; j++)
                        for (int k = 0; k < repeat; k++)
                        {
                            if (i < nChannels)
                                values[i, j * repeat + k] = inputValues[i][j];
                            else
                                values[i, j * repeat + k] = 5;
                        }

                double outputRate = 5000;
                EOM_AI.setupAI(sampleN * repeat, outputRate);
                EOM_AO.putvalue(values, outputRate);

                EOM_AI.start(false);
                EOM_AO.Start(false);
                dio.Evoke();

                int timeout = 5000;
                bool success1 = EOM_AO.WaitUntilDone(timeout);
                bool success2 = EOM_AI.WaitUntilDone(timeout);

                if (!success1 || !success2)
                    return success;

                EOM_AO.Stop();
                EOM_AO.Dispose();

                var AIResult = EOM_AI.readSample();

                EOM_AI.stop();


                ///
                for (int ch = 0; ch < nChannels; ch++)
                {
                    for (int j = 0; j < sampleN; j++)
                        for (int k = 0; k < repeat; k++)
                        {
                            {
                                if (k == 0)
                                    outputValues[ch][j] = AIResult[ch, j * repeat] / repeat;
                                else
                                    outputValues[ch][j] += AIResult[ch, j * repeat + k] / repeat;
                            }
                        }
                }

                Debug.WriteLine("Calibration finished...");

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


                if (State.Init.DO_uncagingShutter)
                    new Digital_Out(DigitalUncagingShutterPort, false);

                for (int i = 0; i < nChannels; i++)
                {
                    inputValue[i] = calibEOM[i][0];
                }
                if (State.Init.AO_uncagingShutter)
                    inputValue[nChannels] = 0; //Close shutter.

                EOM_AO.putValue_Single_EOM(inputValue);

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
                float maxValue = MatrixCalc.GetMax(ShadingImages[LaserN]);
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

            public double getZeroEOMVoltage(int LaserN)
            {
                double returnValue = calib1[LaserN][0];
                if (returnValue > 2)
                    returnValue = 2;
                else if (returnValue < -2)
                    returnValue = -2;

                return returnValue;
            }

            public double getEOMVoltage(double Xvol, double Yvol, int LaserN, double power, bool uncaging)
            {
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

                    if (returnValue > 2)
                        returnValue = 2;
                    else if (returnValue < -2)
                        returnValue = -2;
                }
                else
                {
                    if (calib_fit != null)
                        returnValue = calib_fit[LaserN][(int)power];
                    else
                        returnValue = power / 100;
                }
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
                new Digital_Out(port, true);
            }

            public void close()
            {
                new Digital_Out(port, false);
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

        static double[] ConcatChannels(double[] DataA, double[] DataB)
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

        static double[,] ConcatChannels(double[,] DataA, double[,] DataB)
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

            Buffer.BlockCopy(DataA, 0, DataAll, 0, DataA.Length * sizeof(double));
            Buffer.BlockCopy(DataB, DataA.Length * sizeof(double), DataAll, 0, DataB.Length * sizeof(double));

            return DataAll;
        }


        static public bool[][] makeDigitalOutputAllChannels(ScanParameters State, bool includeClock, bool includeUncage, bool includeDigital, bool image_grabbing, out double outputRate, out int nSamples)
        {
            int AddClockC = includeClock ? 1 : 0;
            int AddUncage = includeUncage ? 1 : 0;
            int AddDigital = includeDigital ? State.DO.NChannels : 0;

            outputRate = 10000;
            nSamples = 0;
            if (AddDigital + AddUncage + AddClockC == 0)
                return null;

            if (includeDigital)
                outputRate = State.DO.outputRate;

            if (includeUncage)
                outputRate = State.Uncaging.outputRate;

            if (includeUncage && includeDigital)
                Math.Max(State.Uncaging.outputRate, State.DO.outputRate);

            if (includeClock)
                outputRate = State.Acq.outputRate / 10;

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

            if (image_grabbing)
            {
                nSamples = (int)(outputRate / 1000.0 * msPerLine * State.Acq.linesPerFrame * State.Acq.nFrames);
            }

            if (includeClock) //nSamples will be changed.
            {
                int linePerCycle = State.Acq.linesPerFrame * State.Acq.nFrames;

                outputRate = State.Acq.outputRate;
                nSamples = (int)(outputRate / 1000.0 * msPerLine * State.Acq.linesPerFrame * State.Acq.nFrames);

                if (!image_grabbing) //focusing.
                {
                    linePerCycle = State.Acq.linesPerFrame;
                    nSamples = (int)(outputRate / 1000.0 * msPerLine * State.Acq.linesPerFrame);
                }

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
            bool[][] digitalform = makeDigitalOutputAllChannels(State, clock_on, uncaging_on, digital_on, for_frame, out outputRate, out int nSamples);
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
            private ScanParameters State;
            private String Board;

            public DigitalOutputControl(ScanParameters State_in)
            {
                State = State_in;

                digitalLinePort = Board + "/port0/" + State.Init.DigitalLinePort;
                DO_ShutterPort = Board + "/port0/" + State.Init.DigitalShutterPort;
                DO_Port = new string[3];
                DO_Port[0] = Board + "/port0/" + State.Init.DigitalOutput1;
                DO_Port[1] = Board + "/port0/" + State.Init.DigitalOutput2;
                DO_Port[2] = Board + "/port0/" + State.Init.DigitalOutput3;
            }

            public void PutValue_and_Start(bool ext_trigger, bool clock_on, bool uncaging_on, bool digital_on, bool grabbing)
            {
                bool include_uncaging = uncaging_on && State.Init.DO_uncagingShutter; //This is because clock...
                bool include_digital = digital_on;
                bool include_clock = clock_on;

                if (!include_uncaging && !include_digital && !include_clock)
                {
                    return;
                }

                bool[][] data1 = makeDigitalOutputAllChannels(State, include_clock, include_uncaging, include_digital, grabbing, out double outputRate, out int nSamples);

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

                hDO = new NiDaq.DigitalOutputSignal(ports.ToArray());
                String trig_port = State.Init.TriggerInput;
                if (ext_trigger)
                    trig_port = State.Init.ExternalTriggerInputPort;

                hDO.PutValue_and_Start(data1, outputRate, sampleClockPort, trig_port, !grabbing);
            }

            public void PutSingleValue(bool ON)
            {
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

                hDO.PutSingleValue(data);
            }

            public void Stop()
            {
                if (hDO != null)
                    hDO.Stop();
            }

            public void Dispose()
            {
                if (hDO != null)
                    hDO.Dispose();
            }

        }


        public class AnalogOutput
        {
            public double outputRate;
            public String portX;
            public String portY;

            public ScanParameters State;
            public Shading shading;

            public String[] AO_Ports;
            public NiDaq.AnalogOutput analog_output;

            public double[,] DataXY;
            public double[,] DataEOM;
            public double[,] DataAll;

            public event FrameDoneHandler FrameDone;
            public EventArgs e = null;
            public delegate void FrameDoneHandler(AnalogOutput mirrorAO, EventArgs e);

            double maxV_Mirror = 10;
            double minV_Mirror = -10;
            double maxV_EOM = 2;
            double minV_EOM = -2;

            double[] maxVots;
            double[] minVolts;

            bool includeMirror = true;
            bool includeEOM = true;
            int addUncagingAO = 0;

            public AnalogOutput(ScanParameters State_in, Shading shading_in, bool includeMirror_in)
            {
                State = State_in;
                shading = shading_in;

                includeMirror = includeMirror_in;

                portX = State.Init.mirrorAOPortX;
                portY = State.Init.mirrorAOPortY;

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

                if (State.Init.EOM_nChannels > 0)
                {
                    for (int i = 0; i < State.Init.EOM_nChannels; i++)
                    {
                        String port = (String)State.Init.GetType().GetField("EOM_Port" + i).GetValue(State.Init);
                        portList.Add(port);
                        maxValues.Add(maxV_EOM);
                        minValues.Add(minV_EOM);
                    }
                }

                if (State.Init.AO_uncagingShutter)
                {
                    addUncagingAO = 1;
                    portList.Add(State.Init.UncagingShutterAnalogPort);
                    maxValues.Add(10);
                    minValues.Add(-10);
                }

                includeEOM = (State.Init.EOM_nChannels + addUncagingAO) > 0;

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

            //Put value for simple scanning.
            public double[,] putValueScan(bool focus, bool shutter_open)
            {
                if (analog_output == null)
                    return null;

                outputRate = State.Acq.outputRate;
                if (includeMirror)
                    DataXY = MakeMirrorOutputXY(State);
                else
                    DataXY = null;

                if (includeEOM)
                    DataEOM = MakeEOMOutput(State, shading, focus, shutter_open);
                else
                    DataEOM = null;

                DataAll = ConcatChannels(DataXY, DataEOM);

                analog_output.Putvalue(DataAll, outputRate, State.Init.SampleClockPort, focus);
                return DataXY;
            }

            public void putvalue(double[,] values, double outputRate1)
            {
                outputRate = outputRate1;
                int samplesPerChannel = (int)(values.GetLength(1));
                analog_output.Putvalue(values, outputRate, State.Init.SampleClockPort, false);
            }

            void EveryNSampleEvent(object sender, EventArgs e)
            {
                FrameDone?.Invoke(this, null);
            }

            public void putValueScanAndUncaging()
            {
                if (analog_output == null)
                    return;

                outputRate = State.Acq.outputRate;
                double[,] DataXY_Org = MakeMirrorOutputXY(State);

                if (includeMirror)
                    DataXY = makeMirrorOutput_Imaging_Uncaging(State);
                else
                    DataXY = null;

                if (includeEOM)
                    DataEOM = makeEOMOutput_Imaging_Uncaging(State, shading); //(State, calib, focus, shutter_open);
                else
                    DataEOM = null;

                DataAll = ConcatChannels(DataXY, DataEOM);

                analog_output.Putvalue(DataAll, outputRate, State.Init.SampleClockPort, false);
                analog_output.SetReturnFunction(DataXY_Org.GetLength(1));
                analog_output.EveryNSamplesEvent += EveryNSampleEvent;
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
                analog_output.Putvalue(DataAll, outputRate, State.Init.SampleClockPort, false);
                return DataXY;
            }

            public double[] mirrorStartPos()
            {
                double[,] values = new double[2, 1];
                double maxX = State.Acq.XMaxVoltage * State.Acq.scanVoltageMultiplier[0] / State.Acq.zoom / State.Acq.fillFraction;
                double maxY = State.Acq.YMaxVoltage * State.Acq.scanVoltageMultiplier[1] / State.Acq.zoom;
                values[0, 0] = -0.5 * maxX; //Default position.
                values[1, 0] = -0.5 * maxY;
                RotateAndOffset(values, State);
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


            public void Start(bool externalTrigger)
            {
                if (externalTrigger)
                    analog_output.Start(State.Init.ExternalTriggerInputPort);
                else
                    analog_output.Start(State.Init.TriggerInput);
            }

            public bool WaitUntilDone(int timeout)
            {
                return analog_output.WaitUntilDone(timeout);
            }

            public void Stop()
            {
                analog_output.Stop();
            }

            public void Dispose()
            {
                if (analog_output != null)
                    analog_output.Dispose();
            }
        } //MirrorAO

        public class PiezoControl
        {
            ScanParameters State;
            String Port_AI;
            String Port_AO;
            NiDaq.AnalogOutput PiezoAO;
            NiDaq.AnalogOutput PiezoAI;
            bool sameBoard_AI = false;
            bool sameBoard_AO = false;
            double maxV = 10;
            double minV = 0;
            double centerV = 5;
            public double time_for_movement_ms = 50;

            public PiezoControl(ScanParameters State_in)
            {
                State = State_in;
                Port_AI = State.Init.Piezo_Z_Monitor;
                Port_AO = State.Init.Piezo_Z_Signal;
            }


            /// <summary>
            /// Return how much it moved in voltage.
            /// </summary>
            /// <param name="destination_V"></param>
            /// <returns></returns>
            public void move_piezo_V(double destination_V)
            {
                double outputRate = 10000;
                var currentPos_V = getPosition_V();
                var movement_V = destination_V - currentPos_V;

                int nSamples = (int)(time_for_movement_ms * outputRate / 1000.0); //100 samples

                PiezoAO = new NiDaq.AnalogOutput(new string[] { Port_AO }, new double[] { maxV }, new double[] { minV });

                double[,] DataAll = new double[1, nSamples];
                for (int i = 0; i < nSamples; i++)
                    DataAll[0, i] = currentPos_V + i * movement_V / nSamples;

                PiezoAO.Putvalue(DataAll, outputRate, State.Init.SampleClockPort, false); //immediately move. Trigger is not necessary since it is only 1 channel.

                PiezoAO.Dispose();
            }

            public double um_to_V(double um)
            {
                return um / State.Init.Piezo_um_per_V + centerV;
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
                var destination_um = distance_um + getPosition_um();
                return move_Piezo_um(destination_um);
            }

            /// <summary>
            /// Get current piezo position in um.
            /// </summary>
            /// <returns></returns>
            public double getPosition_um()
            {
                double v = getPosition_V();
                return V_to_um(v);
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
                var ai = new NiDaq.AI_Read_SingleValue(Port_AI, new double[] { minV, maxV }, out double result);
                return result;
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

        static public double[,] MakePockelsPulses_PockelsAO(ScanParameters State, double outputRate, bool Shutter, bool repeat, Shading shading)
        {
            double sDelay = State.Uncaging.AnalogShutter_delay;
            double pulseDelay = State.Uncaging.pulseDelay;
            double pulseISI = State.Uncaging.pulseISI;
            double pulseWidth = State.Uncaging.pulseWidth;
            int nPulses = State.Uncaging.nPulses;

            int addUncaging = 0;
            if (State.Init.AO_uncagingShutter && Shutter)
                addUncaging = 1;

            int nChannels = State.Init.EOM_nChannels;

            int samplesShutterDelay = (int)(sDelay * outputRate / 1000.0);

            int nRepeat = 1;
            int repeatInterval = 0;
            if (repeat)
            {
                nRepeat = State.Uncaging.trainRepeat;
                repeatInterval = (int)(State.Uncaging.pulseSetInterval_forFrame * outputRate / 1000.0);
            }

            double[,] DataEOM;

            int samplesPerChannel = (int)(outputRate * (State.Uncaging.sampleLength) / 1000.0);

            if (samplesPerChannel % 2 == 1) //NI-DAQ cannot wirte add number samples.
                samplesPerChannel += 1;

            int sampleBeforeUncage = (int)(outputRate * pulseDelay / 1000.0);

            int repeatTotalSamplesPerChannel = samplesPerChannel;
            int samplesPerRepeat = samplesPerChannel;
            int samplesBeforeRepeat = 0;
            if (repeat)
            {
                samplesPerRepeat = (int)(outputRate * State.Uncaging.pulseSetInterval_forFrame / 1000.0);
                repeatTotalSamplesPerChannel = (int)(outputRate * (State.Uncaging.pulseSetInterval_forFrame * nRepeat + State.Uncaging.baselineBeforeTrain_forFrame) / 1000.0);
                if (repeatTotalSamplesPerChannel < samplesPerRepeat)
                {
                    repeatTotalSamplesPerChannel = samplesPerRepeat;
                }
                samplesBeforeRepeat = (int)(outputRate * State.Uncaging.baselineBeforeTrain_forFrame / 1000.0);
            }

            DataEOM = new double[nChannels + addUncaging, repeatTotalSamplesPerChannel];

            double[,] posXY = DefinePulsePosition(State);
            //double[][] calib1 = makeCalib(State, calib);

            for (int ch = 0; ch < nChannels + addUncaging; ch++)
            {
                if (!State.Init.uncagingShutter[ch] && shading != null)
                {
                    for (int i = 0; i < repeatTotalSamplesPerChannel; i++)
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

                            if (repeatTotalSamplesPerChannel < pulseEnd && repeat)
                            {
                                break;
                            }

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
                        for (int i = 0; i < repeatTotalSamplesPerChannel; i++)
                        {
                            if (State.Uncaging.TurnOffImagingDuringUncaging)
                                DataEOM[ch, i] = shading.getZeroEOMVoltage(ch); //all zero.
                        }
                    }


                    if (State.Init.uncagingShutter[ch])
                        DataEOM[ch, repeatTotalSamplesPerChannel - 1] = 0;
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

            public void setupAI(int samplesPerChannel, double inputRate)
            {
                samplesPerTrigger = samplesPerChannel;
                analog_input.SetupAI(samplesPerTrigger, inputRate, State.Init.SampleClockPort, false);
            }

            public void start(bool externalTrigger)
            {
                string trig_port = State.Init.TriggerInput;
                if (externalTrigger)
                    trig_port = State.Init.ExternalTriggerInputPort;

                analog_input.Start(trig_port);
            }

            public double[] getSingleValue()
            {
                return analog_input.GetSingleValue();
            }

            public void stop()
            {
                analog_input.Stop();
            }
            public void dispose()
            {
                analog_input.Dispose();
            }

            public double[,] readSample()
            {
                var result = analog_input.ReadSample();
                return result;
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
            public int nLines;

            public ScanParameters State;
            public String Board;
            public String ExternalTriggerInputPort;
            public String sampleClockPort;

            public LineClockByCounter(ScanParameters State_in, bool focus) //Constructor
            {
                State = State_in;

                lineClockPort = State.Init.lineClockPort;
                frameClockPort = State.Init.frameClockPort;

                msPerLine = State.Acq.msPerLine;
                if (State.Acq.fastZScan)
                    msPerLine = State.Acq.FastZ_msPerLine;

                nLines = State.Acq.linesPerFrame * State.Acq.nFrames;

                frequency = 1000.0 / msPerLine;
                dutyCycle = 0.0001; //0.2 us

                freqFrame = 1000.0 / msPerLine / State.Acq.linesPerFrame;
                dutyFrame = dutyCycle / State.Acq.linesPerFrame;

                double delay = State.Acq.LineClockDelay / 1000.0;

                lineClockCounter = new NiDaq.CounterOutput(lineClockPort, State.Init.TriggerInput, delay, frequency, dutyCycle);
            }

            public void start(bool externalTrigger)
            {
                if (externalTrigger)
                    lineClockCounter.Start(State.Init.ExternalTriggerInputPort);
                else
                    lineClockCounter.Start(State.Init.TriggerInput);
            }

            public void stop()
            {
                try
                {
                    lineClockCounter.Stop();
                }
                catch (Exception Ex)
                {
                    Debug.WriteLine("Stop LineClock failed: " + Ex.Message);
                }
            }

            public void dispose()
            {
                if (lineClockCounter != null)
                    lineClockCounter.Dispose();
            }
        }

        public static double GetAcquisitionDelay_ms(ScanParameters State)
        {
            double msPerLine = State.Acq.msPerLine;
            if (State.Acq.fastZScan)
                msPerLine = State.Acq.FastZ_msPerLine;

            double ScanFraction = State.Acq.BiDirectionalScan ? 1 : State.Acq.scanFraction;
            double ScanSize = 1 / State.Acq.zoom / State.Acq.fillFraction / msPerLine;
            return State.Acq.ScanDelay + State.Acq.ScanDelay2 * ScanSize + State.Acq.ScanDelay4 * ScanSize * ScanSize + msPerLine * (1 - State.Acq.fillFraction / ScanFraction) / 2;
        }

        public static double GetBidirectionalDelay_ms(ScanParameters State)
        {
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
            double ScanDelay = GetAcquisitionDelay_ms(State); // - 0.01 * State.Acq.msPerLine;
            int addUncaging = 0;
            if (State.Init.AO_uncagingShutter)
                addUncaging = 1;

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
                if (imageLaser[ch])
                {
                    if (State.Acq.BiDirectionalScan)
                    {
                        for (int j = 0; j < NLines - 1; j += 2) //Even number.
                        {
                            double Y = (j / NLines - 0.5) * MaxVY;

                            //for (int i = 0; i < RoleOver_Rev; i++) //Role over from odd number?
                            //{
                            //    EOMOuput[ch, i + j * nSamplesX] = shading.getEOMVoltage(XY[0, i], XY[1, i], ch, power[ch]); // calib1[ch][power[ch]];
                            //}

                            for (int i = RoleOver_Rev; i < nSamplesXStart; i++)
                                EOMOuput[ch, i + j * nSamplesX] = shading.getZeroEOMVoltage(ch);

                            for (int i = nSamplesXStart; i < nSamplesXEnd; i++)
                            {
                                double X = ((double)(i - nSamplesXStart) / (double)nSamplesXFill - 0.5) * MaxVX;
                                var XYpos = VoltageRotateOffset(new double[] { X, Y }, State);
                                //RotateAndOffset([])
                                EOMOuput[ch, i + j * nSamplesX] = shading.getEOMVoltage(XYpos[0], XYpos[1], ch, power[ch], false);
                            }

                            for (int i = nSamplesXEnd; i < nSamplesX; i++)
                                EOMOuput[ch, i + j * nSamplesX] = shading.getZeroEOMVoltage(ch);
                        }
                        for (int j = 1; j < NLines; j += 2) //Odd number.
                        {
                            double Y = (j / NLines - 0.5) * MaxVY;

                            //for (int i = 0; i < RoleOver; i++)
                            //    EOMOuput[ch, i + j * nSamplesX] = shading.getEOMVoltage(XY[0, i], XY[1, i], ch, power[ch]);

                            for (int i = RoleOver; i < nSamplesXStart_Rev; i++)
                                EOMOuput[ch, i + j * nSamplesX] = shading.getZeroEOMVoltage(ch);

                            for (int i = nSamplesXStart_Rev; i < nSamplesXEnd_Rev; i++)
                            {
                                double X = ((double)(nSamplesXFill - i + nSamplesXStart_Rev - 1) / (double)nSamplesXFill - 0.5) * MaxVX;

                                EOMOuput[ch, i + j * nSamplesX] = shading.getEOMVoltage(X, Y, ch, power[ch], false);
                            }

                            for (int i = nSamplesXEnd_Rev; i < nSamplesX; i++)
                                EOMOuput[ch, i + j * nSamplesX] = shading.getZeroEOMVoltage(ch);
                        }

                    }
                    else
                    {
                        for (int j = 0; j < NLines; j++)
                        {
                            double Y = (j / NLines - 0.5) * MaxVY;
                            //for (int i = 0; i < RoleOver; i++)
                            //    EOMOuput[ch, i + j * nSamplesX] = shading.getEOMVoltage(XY[0, i], XY[1, i], ch, power[ch]);

                            for (int i = RoleOver; i < nSamplesXStart; i++)
                                EOMOuput[ch, i + j * nSamplesX] = shading.getZeroEOMVoltage(ch);

                            for (int i = nSamplesXStart; i < nSamplesXEnd; i++)
                            {
                                double X = ((double)(i - nSamplesXStart) / (double)nSamplesXFill - 0.5) * MaxVX;
                                var XYpos = VoltageRotateOffset(new double[] { X, Y }, State);
                                EOMOuput[ch, i + j * nSamplesX] = shading.getEOMVoltage(XYpos[0], XYpos[1], ch, power[ch], false);
                            }

                            for (int i = nSamplesXEnd; i < nSamplesX; i++)
                                EOMOuput[ch, i + j * nSamplesX] = shading.getZeroEOMVoltage(ch);
                        }
                    }
                } //ImageLaser.
                else if (uncageLaser[ch] && Focus)
                {
                    for (int j = 0; j < NLines; j++)
                    {
                        for (int i = 0; i < nSamplesX; i++)
                        {
                            EOMOuput[ch, i + j * nSamplesX] = shading.getEOMVoltage(State.Acq.XOffset, State.Acq.YOffset, ch, power[ch], true); //Put zero?
                        }
                    }

                }
                else if (uncageLaser[ch] && !Focus)
                {
                    for (int j = 0; j < NLines; j++)
                        for (int i = 0; i < nSamplesX; i++)
                        {
                            EOMOuput[ch, i + j * nSamplesX] = shading.getZeroEOMVoltage(ch);
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

            double[,] OutputFrame = ReplcateByFrameNumber(MakeEOMOutput(State, shading, false, false), nFrames);
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
                                    OutputFrame[ch, loc] = shading.getEOMVoltage(uncagingPos[0, pulse], uncagingPos[1, pulse], ch, State.Uncaging.Power, true); //calib1[ch][(int)State.Uncaging.Power];
                                else if (State.Init.uncagingLasers[ch] && j < pockelsCloseCount)
                                    OutputFrame[ch, loc] = shading.getZeroEOMVoltage(ch);
                                else if (State.Init.imagingLasers[ch] && anyUncaging && !State.Init.uncagingLasers[ch] && State.Uncaging.TurnOffImagingDuringUncaging)
                                {
                                    OutputFrame[ch, loc] = shading.getZeroEOMVoltage(ch);
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

            double[,] OutputFrame = ReplcateByFrameNumber(MakeMirrorOutputXY(State), nFrames);
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

        public static double[,] ReplcateByFrameNumber(double[,] OutputBase, int nFrames)
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

            if (State.Init.MicroscopeSystem.Contains("Thor")) //Safety feature for thorlabs.
            {
                double minFlyBackTimeY = 0.4; // ms.
                if (msPerLine * (1 - ScanFraction) < minFlyBackTimeY)
                    nSamplesYFB = (int)(minFlyBackTimeY * OutputRate / 1000.0);
            }

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
                    //singleLineX[i] = ((double)(nSamplesX - i) / (double)nSamplesXFB - 0.5) * MaxVX; Usual.

                    singleLineX[i] = MaxVX * 0.5 * Math.Cos((double)(i - nSamplesXScan) * Math.PI / (double)nSamplesXFB); //Sinusoidal wave.

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

            RotateAndOffset(mirrorOutput, State);
            return mirrorOutput;
        }

        public static double[] pixelsOnImageToVoltage(double[] xy_pixel, ScanParameters State)
        {
            double[] frac = new double[2];
            frac[0] = xy_pixel[0] / (double)State.Acq.pixelsPerLine;
            frac[1] = xy_pixel[1] / (double)State.Acq.linesPerFrame;

            double[,] voltagePosition = new double[2, 1];
            double[] voltagePosition_final = new double[2];

            voltagePosition[0, 0] = (frac[0] - 0.5) * State.Acq.XMaxVoltage * State.Acq.scanVoltageMultiplier[0] / State.Acq.zoom;
            voltagePosition[1, 0] = (frac[1] - 0.5) * State.Acq.YMaxVoltage * State.Acq.scanVoltageMultiplier[1] / State.Acq.zoom;

            RotateAndOffset(voltagePosition, State);

            for (int i = 0; i < 2; i++)
                voltagePosition_final[i] = voltagePosition[i, 0] + State.Uncaging.CalibV[i];

            return voltagePosition_final;

        }

        public static double[] VoltageRotateOffset(double[] voltageXY, ScanParameters State)
        {
            double[,] voltagePosition = new double[2, 1];
            double[] voltagePosition_final = new double[2];
            voltagePosition[0, 0] = voltageXY[0];
            voltagePosition[1, 0] = voltageXY[1];

            RotateAndOffset(voltagePosition, State);

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


        public static double[] PositionFracToVoltage(double[] FracPosition, ScanParameters State)
        {
            double[,] voltagePosition = new double[2, 1];
            double[] voltagePosition_final = new double[2];

            double maxX = State.Acq.XMaxVoltage / State.Acq.zoom; // * State.Acq.scanVoltageMultiplier[0] It is already included.... pixels/wdith etc.
            double maxY = State.Acq.YMaxVoltage / State.Acq.zoom; // * State.Acq.scanVoltageMultiplier[1] 

            voltagePosition[0, 0] = (FracPosition[0] - 0.5) * maxX; //for disaply, multiplicator is not necessary.
            voltagePosition[1, 0] = (FracPosition[1] - 0.5) * maxY;

            RotateAndOffset(voltagePosition, State);

            for (int i = 0; i < 2; i++)
                voltagePosition_final[i] = voltagePosition[i, 0] + State.Uncaging.CalibV[i];

            return voltagePosition_final;
        }


        public static void RotateAndOffset(double[,] mirrorOutput, ScanParameters State)
        {
            double[] offset = new double[] { State.Acq.XOffset, State.Acq.YOffset };
            double[] limit = new double[] { State.Init.AbsoluteMaxVoltageScan, State.Init.AbsoluteMaxVoltageScan };
            MatrixCalc.RotateOffsetTimeSeries(mirrorOutput, State.Acq.Rotation, offset, limit, State.Acq.flipXYScan, State.Acq.switchXYScan);
        }

    } //Class

} //Name space
