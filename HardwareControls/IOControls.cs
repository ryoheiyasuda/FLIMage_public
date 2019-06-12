using MathLibrary;
using NationalInstruments;
using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;

namespace FLIMage
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
                DaqSystem.Local.ConnectTerminals(State.Init.masterClock, State.Init.masterClockPort);
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
                    calib.EOM_AO_S.putValue_S(input);
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
            public pockelAI EOM_AI_S;
            public pockelAO EOM_AO;
            public pockelAO EOM_AO_S;
            public DigitalUncagingShutterSignal EOM_DO;
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
                EOM_AO_S = new pockelAO(State, shading, false);
                EOM_AI_S = new pockelAI(State);

                if (State.Init.DO_uncagingShutter)
                    EOM_DO = new DigitalUncagingShutterSignal(State);

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
                return EOM_AI_S.getSingleValue();
            }

            public bool[] calcibrateEOMs(bool plot)
            {
                //dioTrigger dio = new dioTrigger(State);
                EOM_AO = new pockelAO(State, shading, true);

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
                    EOM_DO.TurnOnOff(true);

                EOM_AO.putValue_S(inputValue);
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
                EOM_AO.start(false);
                dio.Evoke();

                int timeout = 5000;
                try
                {
                    EOM_AO.hEOM.WaitUntilDone(timeout);
                }
                catch (DaqException ex)
                {
                    EOM_AO.stop();
                    EOM_AO.dispose();
                    Debug.WriteLine("Calibration error in EOM_AO:" + ex.Message);
                }

                try
                {
                    EOM_AI.hEOM_AI.WaitUntilDone(timeout);
                }
                catch (DaqException ex)
                {
                    EOM_AI.stop();
                    EOM_AI.dispose();
                    Debug.WriteLine("Calibration error in EOM_AI:" + ex.Message);
                    return success;
                }


                EOM_AO.stop();
                EOM_AO.dispose();


                EOM_AI.readSample();

                EOM_AI.stop();


                ///
                for (int ch = 0; ch < nChannels; ch++)
                {
                    for (int j = 0; j < sampleN; j++)
                        for (int k = 0; k < repeat; k++)
                        {
                            {
                                if (k == 0)
                                    outputValues[ch][j] = EOM_AI.result[ch, j * repeat] / repeat;
                                else
                                    outputValues[ch][j] += EOM_AI.result[ch, j * repeat + k] / repeat;
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
                    EOM_DO.TurnOnOff(false);

                for (int i = 0; i < nChannels; i++)
                {
                    inputValue[i] = calibEOM[i][0];
                }
                if (State.Init.AO_uncagingShutter)
                    inputValue[nChannels] = 0; //Close shutter.

                EOM_AO_S.putValue_S(inputValue);

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
            public float[][][] ShadingImages;
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
                ShadingImages = new float[State.Init.EOM_nChannels][][];
                applyCalibration(State);
            }

            public void AddShadingImageFromBinary(float[][] Image, int LaserN)
            {
                ShadingImages[LaserN] = Image;
                width = ShadingImages[LaserN].Length;
                height = ShadingImages[LaserN][0].Length;
            }

            public void AddShadingImage(FLIMData FLIM, int LaserN, int ShadingCh)
            {
                ShadingImages[LaserN] = ImageProcessing.ImageSmooth(FLIM.Project[ShadingCh], 10);
                float maxValue = MatrixCalc.GetMax(ShadingImages[LaserN]);
                ShadingImages[LaserN] = MatrixCalc.DivideConstantFromMatrix(ShadingImages[LaserN], maxValue);
                ShadingImages[LaserN] = ImageProcessing.ImageSqrt(ShadingImages[LaserN]);
                ShadingImages[LaserN] = MatrixCalc.InverseMatrix(ShadingImages[LaserN]);
                State = FLIM.State;
                width = ShadingImages[LaserN].Length;
                height = ShadingImages[LaserN][0].Length;
            }

            public void applyCalibration(ScanParameters State_in)
            {
                State = State_in;
                calibration_exist = makeCalib(State, calibration, out calib1, out calib_fit);
            }

            public double getZeroEOMVoltage(int LaserN)
            {
                return calib1[LaserN][0];
            }

            public double getEOMVoltage(double Xvol, double Yvol, int LaserN, double power, bool uncaging)
            {
                double val = 1;
                double returnValue;
                if ((shading_on && !uncaging) || (shading_uncaging && uncaging) && calibration_exist)
                {
                    int xpixel = (int)((Xvol / State.Acq.XMaxVoltage + 0.5) * width);
                    int ypixel = (int)((Yvol / State.Acq.YMaxVoltage + 0.5) * height);
                    if (ShadingImages[LaserN] != null && ypixel >= 0 && xpixel >= 0 && ypixel < height && xpixel < width)
                        val = ShadingImages[LaserN][ypixel][xpixel];
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
                return returnValue;
            }

            public double getEOMVoltage_Old(double X, double Y, int LaserN, double power, bool uncaging)
            {
                double val = 1;
                double returnValue;
                if ((shading_on && !uncaging) || (shading_uncaging && uncaging) && calibration_exist)
                {
                    int xpixel = (int)((X / State.Acq.XMaxVoltage + 0.5) * width);
                    int ypixel = (int)((Y / State.Acq.XMaxVoltage + 0.5) * height);
                    if (ShadingImages[LaserN] != null && shading_on)
                        val = ShadingImages[LaserN][ypixel][xpixel];
                }

                returnValue = calib1[LaserN][(int)(val * power)];
                return returnValue;
            }
        }

        //This class is to see if digital channel is on or off
        public class DigitalIn
        {
            public Task DI;
            public DigitalSingleChannelReader reader;
            public String port;
            public bool value;
            public DigitalIn(String channel)
            {
                port = channel;
                DI = new Task();
                DI.DIChannels.CreateChannel(port, "DI", ChannelLineGrouping.OneChannelForEachLine);
                reader = new DigitalSingleChannelReader(DI.Stream);
                value = reader.ReadSingleSampleSingleLine();

            }
            public bool readDI()
            {
                value = reader.ReadSingleSampleSingleLine();
                return value;
            }
            public void dispose()
            {
                if (DI != null)
                    DI.Dispose();
            }
        }

        public class DigitalOut
        {
            Task DO;
            public DigitalSingleChannelWriter writer;
            String port;
            public DigitalOut(String port_in)
            {
                port = port_in;
                DO = new Task();
                //shutter.DOChannels.CreateChannel(port, "DIO", ChannelLineGrouping.OneChannelForAllLines);
                DO.DOChannels.CreateChannel(port, "DIO", ChannelLineGrouping.OneChannelForEachLine);
                writer = new DigitalSingleChannelWriter(DO.Stream);
                writer.WriteSingleSampleSingleLine(true, false);
            }

            public void PutValue(bool signal)
            {
                writer.WriteSingleSampleSingleLine(true, signal);
            }

            public void Dispose()
            {
                DO.Dispose();
            }
        }


        //THis class is to control shutter through digital IO
        public class ShutterCtrl
        {
            public Task shutter;
            public Task shutterEx;
            public DigitalSingleChannelWriter writer;
            public DigitalSingleChannelWriter writerEx;
            public String port;
            public ScanParameters State;

            public ShutterCtrl(ScanParameters State_in)
            {
                State = State_in;
                shutter = new Task();
                port = State.Init.shutterPort;
                //shutter.DOChannels.CreateChannel(port, "DIO", ChannelLineGrouping.OneChannelForAllLines);
                shutter.DOChannels.CreateChannel(port, "DIO", ChannelLineGrouping.OneChannelForEachLine);
                writer = new DigitalSingleChannelWriter(shutter.Stream);
                writer.WriteSingleSampleSingleLine(true, false);
            }

            public void open()
            {
                writer.WriteSingleSampleSingleLine(true, true);
                shutter.WaitUntilDone();
            }

            public void close()
            {
                writer.WriteSingleSampleSingleLine(true, false);
                shutter.WaitUntilDone();
            }

            public void dispose()
            {
                if (shutter != null)
                    shutter.Dispose();
            }

        }

        //This class is to send a DIO trigger signal from digital output defined by State.Init.triggerPort.
        public class dioTrigger
        {
            //public Task dioTask;
            public Task dioTaskS;
            public String port;
            private DigitalSingleChannelWriter writer;
            private DigitalSingleChannelWriter writerS;
            public ScanParameters State;
            private DigitalWaveform waveform;
            int nSamples = 2;
            double outputRate = 1000;

            public dioTrigger(ScanParameters State_in)
            {
                State = State_in;
                port = State.Init.triggerPort;

                dioTaskS = new Task();
                dioTaskS.DOChannels.CreateChannel(port, "DIO", ChannelLineGrouping.OneChannelForEachLine);
                writerS = new DigitalSingleChannelWriter(dioTaskS.Stream);

                writerS.WriteSingleSampleSingleLine(true, false);
            }


            public void Evoke()
            {
                //if (dioTask != null)
                //    dioTask.Dispose();

                writerS.WriteSingleSampleSingleLine(true, false);
                writerS.WriteSingleSampleSingleLine(true, true);
                writerS.WriteSingleSampleSingleLine(true, false);
            }

            public void dispose()
            {
                dioTaskS.Dispose();
            }
        }


        public class AO_Write
        {
            public Task AO;
            public AnalogSingleChannelWriter writer;
            public double maxV = 10;
            public double minV = -10;

            public AO_Write(String port, double value)
            {
                AO = new Task();
                AO.AOChannels.CreateVoltageChannel(port, "Port1", minV, maxV, AOVoltageUnits.Volts);
                AO.Control(TaskAction.Verify);
                writer = new AnalogSingleChannelWriter(AO.Stream);
                writer.WriteSingleSample(true, value);

            }

            public void AO_putValue(double value)
            {
                try
                {
                    writer.WriteSingleSample(true, value);
                }
                catch (Exception EX)
                {
                    Debug.WriteLine("Problem --- need to fix. AO_putValue: " + EX.Message);
                }
            }

            public void dispose()
            {
                if (AO != null)
                {
                    AO.Dispose();
                }
            }
        }

        public static bool IfSameBoard_With_Mirror(String BoardName, ScanParameters State)
        {
            String[] sP;
            sP = State.Init.mirrorAOPortX.Split('/');
            String BoardMirror = sP[0];

            return (String.Compare(BoardMirror, BoardName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        static double[,] ConcatChannels(double[,] DataA, double[,] DataB)
        {
            int nChB = DataB.GetLength(0);
            int nChA = DataA.GetLength(0);
            int nCh = nChA + nChB;
            int nSamplesA = DataA.GetLength(1);
            int nSamplesB = DataB.GetLength(1);
            int nSamples = nSamplesA;
            if (nSamples < nSamplesB)
                nSamples = nSamplesB; //whichever the bigger one.

            double[,] DataAll = new double[nCh, nSamples];

            for (int ch = 0; ch < nCh; ch++)
                for (int x = 0; x < nSamples; x++)
                {
                    if (ch < nChA && x < nSamplesA)
                    {
                        DataAll[ch, x] = DataA[ch, x];
                    }
                    else if (ch >= nChA && x < nSamplesB)
                        DataAll[ch, x] = DataB[ch - nChA, x];
                }

            return DataAll;
        }

        public class DigitalUncagingShutterSignal
        {
            private String digitalOutputPort;
            private String triggerPort = "";
            private String ExternalTriggerPort = "";
            private String sampleClockPort = "";
            private String Board = "";
            private ScanParameters State;

            private int nSamples;
            private Task myTask;
            private Task onoffTask;
            private DigitalWaveform waveform;
            private DigitalSingleChannelWriter writer, writerOnOff;
            private bool activeHigh = true;

            private bool portActive = false;

            private bool sameBoardWithMirrors = false;

            public DigitalUncagingShutterSignal(ScanParameters State_in)
            {
                State = State_in;
                GetTriggerPortName(State.Init.mirrorAOPortX, State, ref Board, ref triggerPort, ref ExternalTriggerPort, ref sampleClockPort);
                digitalOutputPort = Board + "/" + "port0" + "/" + State.Init.DigitalShutterPort;

                sameBoardWithMirrors = IfSameBoard_With_Mirror(Board, State);

                if (sameBoardWithMirrors)
                    sampleClockPort = "";

                activeHigh = !State.Init.DO_uncagingShutter_useForPMTsignal;

                onoffTask = new Task();
                onoffTask.DOChannels.CreateChannel(digitalOutputPort, "", ChannelLineGrouping.OneChannelForEachLine);

                writerOnOff = new DigitalSingleChannelWriter(onoffTask.Stream);
            }

            public void PutValue_and_Start(bool ext_trigger)
            {
                double outputRate = State.Uncaging.outputRate;

                if (sameBoardWithMirrors)
                {
                    if (ext_trigger)
                        myTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, DigitalEdgeStartTriggerEdge.Rising);
                    else
                        myTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(ExternalTriggerPort, DigitalEdgeStartTriggerEdge.Rising);
                }
                myTask.Control(TaskAction.Verify);

                double totalLength_ms = State.Uncaging.sampleLength;

                int nSamples = (int)(outputRate / 1000.0 * State.Uncaging.sampleLength);

                myTask = new Task();
                myTask.DOChannels.CreateChannel(digitalOutputPort, "", ChannelLineGrouping.OneChannelForEachLine);
                myTask.Timing.ConfigureSampleClock(sampleClockPort, outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, nSamples);

                //waveform = new DigitalWaveform(nSamples, 1);

                waveform = MakeWaveFormForDigitalUncaging(State, outputRate, false, 0, nSamples);

                writer = new DigitalSingleChannelWriter(myTask.Stream);
                writer.WriteWaveform(false, waveform);
                myTask.Start();
            }

            public void TurnOnOff(bool ON)
            {
                if (!activeHigh)
                    ON = !ON;

                try
                {
                    writerOnOff.WriteSingleSampleSingleLine(true, ON);
                }
                catch (Exception EX)
                {
                    Debug.WriteLine(EX.ToString());
                }
            }

            public void Stop()
            {
                if (myTask != null && portActive)
                {
                    myTask.Stop();
                    myTask.WaitUntilDone();
                    //myTask.Dispose();
                    portActive = false;
                }
            }

            public void dispose()
            {
                if (myTask != null)
                    myTask.Dispose();
            }
        }

        static public DigitalWaveform MakeWaveFormForDigitalUncaging(ScanParameters State, double outputRate, bool forFrame, int channel, int nSamples)
        {
            bool activeHigh = !State.Init.DO_uncagingShutter_useForPMTsignal;
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
            DigitalState state = DigitalState.ForceDown;
            if (activeHigh)
                state = DigitalState.ForceUp;

            DigitalWaveform waveform = new DigitalWaveform(nSamples, 1, state);


            for (int train = 0; train < nRepeat; train++)
            {
                int train_Start = baseLine_train + train * trainInterval;
                int initialDelay = train_Start + initialDelay_pulse;

                for (int j = 0; j < State.Uncaging.pulse_number; j++)
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
                    {
                        if (activeHigh)
                            waveform.Signals[channel].States[i] = DigitalState.ForceUp;
                        else
                            waveform.Signals[channel].States[i] = DigitalState.ForceDown;
                    }
                }

                if (end_of_pulse)
                    break;
            }

            return waveform;
        }

        //
        //Clock is coming out of the second board. For now.
        //
        public class DigitalLineClock
        {
            private String digitalLinePort;
            private String digitalShutterPort = "";
            private String triggerPort;
            private String ExternalTriggerPort;
            private String sampleClockPort;
            private bool sameBoardWithMirrors;
            private ScanParameters State;
            private String Board;
            private int nSamples;
            private Task myTask;
            private DigitalWaveform[] waveform;
            private DigitalMultiChannelWriter writer;
            private bool activeHigh = true;

            private bool portActive = false;

            public DigitalLineClock(ScanParameters State_in)
            {
                State = State_in;
                GetTriggerPortName(State.Init.mirrorAOPortX, State, ref Board, ref triggerPort, ref ExternalTriggerPort, ref sampleClockPort);

                digitalLinePort = Board + "/port0/" + State.Init.DigitalLinePort;
                digitalShutterPort = Board + "/port0/" + State.Init.DigitalShutterPort;

                sameBoardWithMirrors = IfSameBoard_With_Mirror(Board, State);

                if (sameBoardWithMirrors)
                {
                    sampleClockPort = "";
                }

            }

            public void PutValue_and_Start(bool ext_trigger)
            {
                portActive = true;
                double outputRate = State.Acq.outputRate / 10;

                double msPerLine = State.Acq.msPerLine;
                if (State.Acq.fastZScan)
                    msPerLine = State.Acq.FastZ_msPerLine;

                nSamples = (int)(outputRate / 1000.0 * msPerLine * State.Acq.linesPerFrame * State.Acq.nFrames);

                myTask = new Task();
                myTask.DOChannels.CreateChannel(digitalLinePort, "", ChannelLineGrouping.OneChannelForEachLine);
                myTask.DOChannels.CreateChannel(digitalShutterPort, "", ChannelLineGrouping.OneChannelForEachLine);

                myTask.Timing.ConfigureSampleClock(sampleClockPort, outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, nSamples);

                if (sameBoardWithMirrors)
                {
                    if (ext_trigger)
                        myTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, DigitalEdgeStartTriggerEdge.Rising);
                    else
                        myTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(ExternalTriggerPort, DigitalEdgeStartTriggerEdge.Rising);
                }

                myTask.Control(TaskAction.Verify);

                DigitalState digital_state = DigitalState.ForceDown;
                if (!activeHigh)
                    digital_state = DigitalState.ForceUp;

                waveform = new DigitalWaveform[2];
                waveform[0] = new DigitalWaveform(nSamples, 1, digital_state);

                for (int i = 0; i < State.Acq.linesPerFrame * State.Acq.nFrames; i++)
                {
                    int loc = i * (int)(outputRate / (1000.0 * msPerLine));
                    if (activeHigh)
                        waveform[0].Signals[0].States[loc] = DigitalState.ForceUp;
                    else
                        waveform[0].Signals[0].States[loc] = DigitalState.ForceDown;
                }

                if (State.Init.DO_uncagingShutter)
                    waveform[1] = MakeWaveFormForDigitalUncaging(State, outputRate, true, 0, nSamples);
                else
                    waveform[1] = new DigitalWaveform(nSamples, 1, DigitalState.ForceDown);

                writer = new DigitalMultiChannelWriter(myTask.Stream);
                writer.WriteWaveform(false, waveform);
                myTask.Start();
            }

            public void Stop()
            {
                if (myTask != null && portActive)
                {
                    myTask.Stop();
                    myTask.WaitUntilDone();
                    //myTask.Dispose();
                    portActive = false;
                }
            }

            public void dispose()
            {
                if (myTask != null)
                    myTask.Dispose();
            }

        }


        //Analog output class to control galvanoic mirrors. X and Y outputs are defined by State.Init.mirrorAOPortX and State.Init.mirrorAOPortY
        public class MirrorAO
        {
            public Task hAOXY;
            public Task hAOXY_S;
            public AnalogMultiChannelWriter writerXY;
            public AnalogMultiChannelWriter writerXY_S;

            public double outputRate;
            public double msPerLine;
            public int nFrames;
            public int nLines;
            public double ScanFraction;
            public String portX;
            public String portY;
            public String triggerPort = "";
            public String sampleClockPort = "";
            public String Board = "";
            public String ExternalTriggerInputPort = "";

            public ScanParameters State;
            public Shading shading;

            public event FrameDoneHandler FrameDone;
            public EventArgs e = null;
            public delegate void FrameDoneHandler(MirrorAO mirrorAO, EventArgs e);
            public bool SameBoard = false;

            public MirrorAO(ScanParameters State_in, Shading shading_in)
            {
                State = State_in;
                shading = shading_in;

                hAOXY = new Task();
                hAOXY_S = new Task();

                double maxV = 10;
                double minV = -10;

                portX = State.Init.mirrorAOPortX;
                portY = State.Init.mirrorAOPortY;

                hAOXY.AOChannels.CreateVoltageChannel(portX, "aoChannelX", minV, maxV, AOVoltageUnits.Volts);
                hAOXY.AOChannels.CreateVoltageChannel(portY, "aoChannelY", minV, maxV, AOVoltageUnits.Volts);

                hAOXY_S.AOChannels.CreateVoltageChannel(portX, "aoChannelXS", minV, maxV, AOVoltageUnits.Volts);
                hAOXY_S.AOChannels.CreateVoltageChannel(portY, "aoChannelYS", minV, maxV, AOVoltageUnits.Volts);

                GetTriggerPortName(portX, State, ref Board, ref triggerPort, ref ExternalTriggerInputPort, ref sampleClockPort);
                State.Init.MirrorAOBoard = Board;


                if (State.Init.EOM_nChannels > 0)
                {
                    string tmpBoard = "";
                    string tmpTrigger = "";
                    string tmpExt = "";
                    string tmpSample = "";
                    GetTriggerPortName(State.Init.EOM_Port0, State, ref tmpBoard, ref tmpTrigger, ref tmpExt, ref tmpSample);
                    SameBoard = IfSameBoard_With_Mirror(tmpBoard, State);
                    minV = -2;
                    maxV = 2;

                    if (SameBoard)
                    {
                        hAOXY.AOChannels.CreateVoltageChannel(State.Init.EOM_Port0, "M_EOM0", minV, maxV, AOVoltageUnits.Volts);
                        if (State.Init.EOM_nChannels > 1)
                        {
                            hAOXY.AOChannels.CreateVoltageChannel(State.Init.EOM_Port1, "M_EOM1S", minV, maxV, AOVoltageUnits.Volts);
                        }
                        if (State.Init.EOM_nChannels > 2)
                        {
                            hAOXY.AOChannels.CreateVoltageChannel(State.Init.EOM_Port2, "M_EOM2", minV, maxV, AOVoltageUnits.Volts);
                        }
                        if (State.Init.EOM_nChannels > 3)
                        {
                            hAOXY.AOChannels.CreateVoltageChannel(State.Init.EOM_Port3, "M_EOM3", minV, maxV, AOVoltageUnits.Volts);
                        }

                        if (State.Init.AO_uncagingShutter)
                        {
                            hAOXY.AOChannels.CreateVoltageChannel(State.Init.UncagingShutterAnalogPort, "M_UncagingShutter", 0, 5, AOVoltageUnits.Volts);
                        }
                    }
                }

                hAOXY.Control(TaskAction.Verify);
                hAOXY_S.Control(TaskAction.Verify);


                //
                try
                {
                    if (sampleClockPort != "")
                        hAOXY.ExportSignals.ExportHardwareSignal(ExportSignal.SampleClock, sampleClockPort);
                }
                catch (DaqException ex)
                {
                    Debug.WriteLine("Error in exporting clock:" + sampleClockPort + ":" + ex.Message);
                }

            }

            //Put value for simple scanning.
            public double[,] putValueScan(bool focus, bool shutter_open, bool makeNew)
            {
                outputRate = State.Acq.outputRate;
                msPerLine = State.Acq.msPerLine;
                if (State.Acq.fastZScan)
                    msPerLine = State.Acq.FastZ_msPerLine;

                nFrames = State.Acq.nFrames;
                nLines = State.Acq.linesPerFrame;


                int samplesPerChannel = (int)(outputRate * msPerLine * nLines * nFrames / 1000.0);

                if (makeNew)
                {
                    if (focus)
                        hAOXY.Timing.ConfigureSampleClock("", outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples);
                    else
                        hAOXY.Timing.ConfigureSampleClock("", outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, samplesPerChannel);

                    hAOXY.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, DigitalEdgeStartTriggerEdge.Rising);

                    hAOXY.EveryNSamplesWrittenEventInterval = (int)(outputRate * msPerLine * nLines / 1000.0);
                    hAOXY.EveryNSamplesWritten += new EveryNSamplesWrittenEventHandler(EveryNSampleEvent);

                    writerXY = new AnalogMultiChannelWriter(hAOXY.Stream);
                }

                double[,] DataXY = MakeMirrorOutputXY(State);
                double[,] DataAll;

                if (SameBoard)
                {
                    double[,] DataEOM = MakeEOMOutput(State, shading, focus, shutter_open);
                    DataAll = ConcatChannels(DataXY, DataEOM);
                }
                else
                {
                    DataAll = DataXY;
                }

                writerXY.WriteMultiSample(false, DataAll);
                return DataXY;
            }

            void EveryNSampleEvent(object sender, EveryNSamplesWrittenEventArgs e)
            {
                FrameDone(this, null);
            }

            public void putValueScanAndUncaging()
            {
                outputRate = State.Acq.outputRate;
                msPerLine = State.Acq.msPerLine;
                if (State.Acq.fastZScan)
                    msPerLine = State.Acq.FastZ_msPerLine;

                nFrames = State.Acq.nFrames;
                nLines = State.Acq.linesPerFrame;

                int samplesPerChannel = (int)(outputRate * msPerLine * nLines * nFrames / 1000);

                hAOXY.Timing.ConfigureSampleClock("", outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, samplesPerChannel);
                hAOXY.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, DigitalEdgeStartTriggerEdge.Rising);

                double[,] DataXY = makeMirrorOutput_Imaging_Uncaging(State);

                double[,] DataAll;

                if (SameBoard)
                {
                    double[,] DataEOM = makeEOMOutput_Imaging_Uncaging(State, shading); //(State, calib, focus, shutter_open);
                    DataAll = ConcatChannels(DataXY, DataEOM);
                }
                else
                {
                    DataAll = DataXY;
                }

                hAOXY.EveryNSamplesWrittenEventInterval = (int)(outputRate * msPerLine * nLines / 1000);
                hAOXY.EveryNSamplesWritten += new EveryNSamplesWrittenEventHandler(EveryNSampleEvent);

                writerXY = new AnalogMultiChannelWriter(hAOXY.Stream);
                writerXY.WriteMultiSample(false, DataAll);
            }

            public double[,] putvalueUncageOnce() //lower sampling rate!!
            {
                outputRate = State.Uncaging.outputRate;

                int samplesPerChannel = (int)(outputRate * State.Uncaging.sampleLength / 1000.0);
                //double[,] DataXY = new double[2, samplesPerChannel];

                double[,] DataXY = MakeUncagePulses_MirrorAO(State, outputRate);

                double[,] DataAll;

                if (SameBoard)
                {
                    double[,] DataEOM = MakePockelsPulses_PockelsAO(State, outputRate, State.Init.AO_uncagingShutter, false, shading);
                    DataAll = ConcatChannels(DataXY, DataEOM);
                }
                else
                {
                    DataAll = DataXY;
                }

                hAOXY.Timing.ConfigureSampleClock("", outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, samplesPerChannel);
                hAOXY.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, DigitalEdgeStartTriggerEdge.Rising);

                writerXY = new AnalogMultiChannelWriter(hAOXY.Stream);
                writerXY.WriteMultiSample(false, DataAll);

                return DataXY;
            }

            /// <summary>
            /// Put Single Values to Mirror channels. 
            /// double[] values: {X, Y};
            /// </summary>
            /// <param name="values"></param>
            public void putValue_S(double[] values)
            {
                try
                {
                    writerXY_S = new AnalogMultiChannelWriter(hAOXY_S.Stream);
                    writerXY_S.WriteSingleSample(true, values);
                    hAOXY_S.WaitUntilDone();
                }
                catch (Exception EX)
                {
                    Debug.WriteLine(EX.ToString());
                }
            }

            public bool start(bool externalTrigger)
            {
                if (externalTrigger)
                    hAOXY.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(ExternalTriggerInputPort, DigitalEdgeStartTriggerEdge.Rising);
                //else
                //    hAOXY.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, DigitalEdgeStartTriggerEdge.Rising);
                try
                {
                    hAOXY.Start();
                    return true;
                }
                catch (DaqException ex)
                {
                    hAOXY.Start();
                    return false;
                }
            }

            public void WaitUntilDone(int timeout)
            {
                try
                {
                    hAOXY.WaitUntilDone(timeout);
                }
                catch (DaqException ex)
                {
                    Debug.WriteLine("TIMEOUT: hAOXY mirror output" + ex.Message);
                    stop();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TIMEOUT: hAOXY mirror data" + ex.Message);
                }
            }

            public void stop()
            {
                try
                {
                    hAOXY.Stop();
                    hAOXY.WaitUntilDone();
                }
                catch { }
                finally { }
            }

            public void dispose()
            {
                //stop();

                if (hAOXY != null)
                    hAOXY.Dispose();

                if (hAOXY_S != null)
                    hAOXY_S.Dispose();
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
                if (!State.Init.uncagingShutter[ch])
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

                            if (State.Init.uncagingLasers[ch])
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

                    else if (State.Init.imagingLasers[ch] && !State.Init.uncagingLasers[ch])
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

        static public void GetTriggerPortName(String DeviceName, ScanParameters State, ref String BoardName, ref string triggerPort, ref String ExternalTriggerInputPort, ref String SampleClockPort)
        {
            String[] sP = DeviceName.Split('/');
            BoardName = sP[0];
            for (int i = 0; i < sP.Length; i++)
                if (sP[i].StartsWith("Dev"))
                {
                    BoardName = sP[i];
                }

            ExternalTriggerInputPort = "/" + BoardName + "/" + State.Init.ExternalTriggerInputPort;
            triggerPort = "/" + BoardName + "/" + State.Init.TriggerInput;
            SampleClockPort = "/" + BoardName + "/" + State.Init.SampleClockPort;
        }

        public class pockelAI
        {
            public Task hEOM_AI, hEOM_AI_S;
            public AnalogMultiChannelReader readerEOM_AI; // readerEOM_AI_S;
            public Task runningTask;
            public AsyncCallback analogCallback;

            public String port0;
            public String port1;
            public String port2;
            public String port3;

            public int nChannels;

            public int samplesPerTrigger;
            public String triggerPort;
            public String SampleClockPort;
            public DigitalEdgeStartTriggerEdge triggerEdge;

            public bool measurement_done = false;

            //public double[,] data;
            public double[,] result;

            public ScanParameters State;

            public String Board;
            public String ExternalTriggerInputPort;

            public pockelAI(ScanParameters State_in)
            {
                State = State_in;
                //triggerPort = State.Init.EOM_AI_Trigger;
                port0 = State.Init.EOM_AI_Port0;
                port1 = State.Init.EOM_AI_Port1;
                port2 = State.Init.EOM_AI_Port2;
                port3 = State.Init.EOM_AI_Port3;
                nChannels = State.Init.EOM_nChannels;

                GetTriggerPortName(port0, State, ref Board, ref triggerPort, ref ExternalTriggerInputPort, ref SampleClockPort);

                double maxV = 1;
                double minV = -1;

                if (nChannels > 4)
                    nChannels = 4;

                triggerEdge = DigitalEdgeStartTriggerEdge.Rising;

                hEOM_AI = new Task();
                hEOM_AI_S = new Task();

                hEOM_AI.AIChannels.CreateVoltageChannel(port0, "EOM_AI_Ch0", (AITerminalConfiguration)(-1), minV, maxV, AIVoltageUnits.Volts);
                hEOM_AI_S.AIChannels.CreateVoltageChannel(port0, "EOM_AI_Ch0S", (AITerminalConfiguration)(-1), minV, maxV, AIVoltageUnits.Volts);

                if (nChannels > 1)
                {
                    hEOM_AI.AIChannels.CreateVoltageChannel(port1, "EOM_AI_Ch1", (AITerminalConfiguration)(-1), minV, maxV, AIVoltageUnits.Volts);
                    hEOM_AI_S.AIChannels.CreateVoltageChannel(port1, "EOM_AI_Ch1S", (AITerminalConfiguration)(-1), minV, maxV, AIVoltageUnits.Volts);
                }

                if (nChannels > 2)
                {
                    hEOM_AI.AIChannels.CreateVoltageChannel(port2, "EOM_AI_Ch2", (AITerminalConfiguration)(-1), minV, maxV, AIVoltageUnits.Volts);
                    hEOM_AI_S.AIChannels.CreateVoltageChannel(port2, "EOM_AI_Ch2S", (AITerminalConfiguration)(-1), minV, maxV, AIVoltageUnits.Volts);
                }

                if (nChannels > 3)
                {
                    hEOM_AI.AIChannels.CreateVoltageChannel(port3, "EOM_AI_Ch3", (AITerminalConfiguration)(-1), minV, maxV, AIVoltageUnits.Volts);
                    hEOM_AI_S.AIChannels.CreateVoltageChannel(port3, "EOM_AI_Ch3S", (AITerminalConfiguration)(-1), minV, maxV, AIVoltageUnits.Volts);
                }

                hEOM_AI.Control(TaskAction.Verify);
                hEOM_AI_S.Control(TaskAction.Verify);

                //hEOM_AI.EveryNSamplesReadEventInterval = 1;
                //hEOM_AI.EveryNSamplesRead += new EveryNSamplesReadEventHandler(SampleCompleted);
            }

            public void SampleCompleted(object obj, EveryNSamplesReadEventArgs e)
            {
                //Debug.WriteLine("Sample Completed!!"); //just put function.
            }

            //public void setEveryNSampleNumber(int sampleNumber)
            //{
            //    hEOM_AI.EveryNSamplesReadEventInterval = sampleNumber;
            //}

            public void setupAI(int samplesPerChannel, double inputRate)
            {
                //inputRate = State.Acq.inputRate;
                samplesPerTrigger = samplesPerChannel;

                //hEOM_AI.Timing.ConfigureSampleClock("", inputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, samplesPerTrigger);
                hEOM_AI.Timing.ConfigureSampleClock("", inputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, samplesPerTrigger);
                hEOM_AI.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, triggerEdge);
                hEOM_AI.Control(TaskAction.Verify);

                hEOM_AI.EveryNSamplesReadEventInterval = samplesPerChannel;
                //result = new double[,]; //[State.Init.EOM_nChannels, samplesPerTrigger];
            }

            public void startWithAsyncCallback() //does not work.
            {
                readerEOM_AI = new AnalogMultiChannelReader(hEOM_AI.Stream);
                analogCallback = new AsyncCallback(AnalogInCallback);
                readerEOM_AI.SynchronizeCallbacks = true;
                readerEOM_AI.BeginReadMultiSample((int)samplesPerTrigger, analogCallback, hEOM_AI);
                measurement_done = false;
                hEOM_AI.Start();
            }

            public void start(bool externalTrigger)
            {
                if (externalTrigger)
                    hEOM_AI.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(ExternalTriggerInputPort, triggerEdge);
                //else
                //    hEOM_AI.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, triggerEdge);

                hEOM_AI.Control(TaskAction.Verify);

                readerEOM_AI = new AnalogMultiChannelReader(hEOM_AI.Stream);
                measurement_done = false;
                hEOM_AI.Start();
                //
            }


            public double[] getSingleValue()
            {
                AnalogMultiChannelReader reader = new AnalogMultiChannelReader(hEOM_AI_S.Stream);
                double[] data1;

                try
                {
                    data1 = reader.ReadSingleSample();
                    measurement_done = true;
                }
                catch (DaqException ex)
                {
                    data1 = new double[nChannels];
                    Debug.WriteLine("Failed:" + ex.Message);
                }
                return data1;
            }

            public void stop()
            {
                //runningTask = null;
                hEOM_AI.Stop();
            }
            public void dispose()
            {
                hEOM_AI.Dispose();
                hEOM_AI_S.Dispose();
            }

            public void readSample()
            {
                if (hEOM_AI.Stream.AvailableSamplesPerChannel == samplesPerTrigger)
                {
                    result = readerEOM_AI.ReadMultiSample((int)samplesPerTrigger);
                }
                else
                {
                    result = readerEOM_AI.ReadMultiSample((int)hEOM_AI.Stream.AvailableSamplesPerChannel);
                }
                measurement_done = true;
            }

            public void AnalogInCallback(IAsyncResult ar)
            {
                try
                {
                    result = readerEOM_AI.EndReadMultiSample(ar);
                }
                catch (DaqException ex)
                {
                    //MessageBox.Show(ex.Message);
                    Debug.WriteLine("Problem in AnalogInCallback by pockelAI: " + ex.Message);
                    runningTask = null;
                }
                finally
                {
                    measurement_done = true;
                }
            }

        }


        public class pockelAO
        {
            public Task hEOM, hEOM_S;
            public AnalogMultiChannelWriter writerEOM;
            public AnalogMultiChannelWriter writerEOM_S;

            public double outputRate;
            public double msPerLine;
            public int nFrames;
            public int nLines;
            public int addUncaging = 0;
            public double ScanFraction;
            public String port0;
            public String port1;
            public String port2;
            public String port3;
            public String portU;
            public String sampleClockPort;
            public String slaveClockPort;
            public int nChannels;

            public String triggerPort;
            public ScanParameters State;
            public Shading shading;

            public String Board;
            public String ExternalTriggerInputPort;

            public pockelAO(ScanParameters State_in, Shading shading_in, bool sync)
            {
                State = State_in;
                shading = shading_in;

                outputRate = State.Acq.outputRate;
                port0 = State.Init.EOM_Port0;
                port1 = State.Init.EOM_Port1;
                port2 = State.Init.EOM_Port2;
                port3 = State.Init.EOM_Port3;
                portU = State.Init.UncagingShutterAnalogPort;
                nChannels = State.Init.EOM_nChannels;
                slaveClockPort = State.Init.EOM_slaveClockPort;

                GetTriggerPortName(port0, State, ref Board, ref triggerPort, ref ExternalTriggerInputPort, ref sampleClockPort);
                State.Init.EOMBoard = Board;

                if (sampleClockPort.Length <= 4) //Like NA;
                    sampleClockPort = "";

                if (State.Init.AO_uncagingShutter)
                    addUncaging = 1;

                if (nChannels > 4)
                    nChannels = 4;

                double maxV = 2;
                double minV = -2;

                hEOM = new Task();
                hEOM_S = new Task();

                if (nChannels > 0)
                {
                    hEOM.AOChannels.CreateVoltageChannel(port0, "EOM_Ch0", minV, maxV, AOVoltageUnits.Volts);
                    hEOM_S.AOChannels.CreateVoltageChannel(port0, "EOM_Ch0S", minV, maxV, AOVoltageUnits.Volts);
                }


                if (nChannels > 1)
                {
                    hEOM.AOChannels.CreateVoltageChannel(port1, "EOM_Ch1", minV, maxV, AOVoltageUnits.Volts);
                    hEOM_S.AOChannels.CreateVoltageChannel(port1, "EOM_Ch1S", minV, maxV, AOVoltageUnits.Volts);
                }

                if (nChannels > 2)
                {
                    hEOM.AOChannels.CreateVoltageChannel(port2, "EOM_Ch2", minV, maxV, AOVoltageUnits.Volts);
                    hEOM_S.AOChannels.CreateVoltageChannel(port2, "EOM_Ch2S", minV, maxV, AOVoltageUnits.Volts);
                }

                if (nChannels > 3)
                {
                    hEOM.AOChannels.CreateVoltageChannel(port3, "EOM_Ch3", minV, maxV, AOVoltageUnits.Volts);
                    hEOM_S.AOChannels.CreateVoltageChannel(port3, "EOM_Ch3S", minV, maxV, AOVoltageUnits.Volts);
                }

                if (State.Init.AO_uncagingShutter)
                {
                    hEOM.AOChannels.CreateVoltageChannel(portU, "UncagingShutter", 0, 5, AOVoltageUnits.Volts);
                    hEOM_S.AOChannels.CreateVoltageChannel(portU, "UncagingShutterS", 0, 5, AOVoltageUnits.Volts);

                    State.Init.uncagingShutter = new bool[State.Init.uncagingLasers.Length];
                    State.Init.uncagingShutter[nChannels] = true;
                }


                hEOM.Control(TaskAction.Verify);
                hEOM_S.Control(TaskAction.Verify);


                try
                {
                    if (slaveClockPort != "" && sync)
                    {
                        hEOM.Timing.MasterTimebaseSource = slaveClockPort;
                        Debug.WriteLine("*** Base clock of EOMs: " + hEOM.Timing.MasterTimebaseSource.ToString() + ", Rate = " + hEOM.Timing.MasterTimebaseRate.ToString() + "***");
                    }
                }
                catch (DaqException ex)
                {
                    Debug.WriteLine("Error in synchronizing signals" + ex.ToString());
                }

            }


            public void putvalueUncageOnce()
            {
                outputRate = State.Uncaging.outputRate;
                bool anyUncaging = State.Init.uncagingLasers.Any(item => item == true);
                double sDelay = State.Uncaging.AnalogShutter_delay;
                if (!anyUncaging)
                    sDelay = 0;
                int samplesPerChannel = (int)(outputRate * State.Uncaging.sampleLength / 1000.0);

                shading.applyCalibration(State);
                double[,] DataEOM = MakePockelsPulses_PockelsAO(State, outputRate, State.Init.AO_uncagingShutter, false, shading);
                hEOM.Timing.ConfigureSampleClock("", outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, samplesPerChannel);
                hEOM.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, DigitalEdgeStartTriggerEdge.Rising);
                hEOM.EveryNSamplesWrittenEventInterval = DataEOM.GetLength(1);
                Debug.WriteLine("Data Length = " + DataEOM.GetLength(1));
                hEOM.EveryNSamplesWritten += new EveryNSamplesWrittenEventHandler(EveryNSampleEvent);

                writerEOM = new AnalogMultiChannelWriter(hEOM.Stream);
                writerEOM.WriteMultiSample(false, DataEOM);
            }

            void EveryNSampleEvent(object sender, EveryNSamplesWrittenEventArgs e)
            {
                Debug.WriteLine("finished!!");
            }

            public void putValueScan(bool focus, bool shutter_open, bool makeNew)
            {
                outputRate = State.Acq.outputRate;
                msPerLine = State.Acq.msPerLine;
                if (State.Acq.fastZScan)
                    msPerLine = State.Acq.FastZ_msPerLine;

                nFrames = State.Acq.nFrames;
                nLines = State.Acq.linesPerFrame;

                int samplesPerChannel = (int)(outputRate * msPerLine * nLines * nFrames / 1000);

                if (makeNew)
                {
                    if (focus)
                        hEOM.Timing.ConfigureSampleClock(sampleClockPort, outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples);
                    else
                        hEOM.Timing.ConfigureSampleClock(sampleClockPort, outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, samplesPerChannel);


                    hEOM.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, DigitalEdgeStartTriggerEdge.Rising);

                    writerEOM = new AnalogMultiChannelWriter(hEOM.Stream);
                }

                double[,] DataEOM = MakeEOMOutput(State, shading, focus, shutter_open);

                writerEOM.WriteMultiSample(false, DataEOM);
            }


            public void putValueScan(bool focus, bool shutter_open, double[,] DataXY)
            {
                putValueScan(focus, shutter_open, true);
            }

            public void putValueScanAndUncaging()
            {
                outputRate = State.Acq.outputRate;
                msPerLine = State.Acq.msPerLine;
                if (State.Acq.fastZScan)
                    msPerLine = State.Acq.FastZ_msPerLine;

                nFrames = State.Acq.nFrames;
                nLines = State.Acq.linesPerFrame;

                int samplesPerChannel = (int)(outputRate * msPerLine * nLines * nFrames / 1000);

                hEOM.Timing.ConfigureSampleClock(sampleClockPort, outputRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, samplesPerChannel);
                hEOM.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, DigitalEdgeStartTriggerEdge.Rising);

                double[,] DataEOM = makeEOMOutput_Imaging_Uncaging(State, shading);
                writerEOM = new AnalogMultiChannelWriter(hEOM.Stream);
                writerEOM.WriteMultiSample(false, DataEOM);
            }

            public void putvalue(double[,] values, double outputRate1)
            {
                int samplesPerChannel = (int)(values.GetLength(1));

                hEOM.Timing.ConfigureSampleClock("", outputRate1, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, samplesPerChannel);
                //hEOM.Triggers.StartTrigger.ConfigureNone();
                hEOM.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, DigitalEdgeStartTriggerEdge.Rising);
                writerEOM = new AnalogMultiChannelWriter(hEOM.Stream);
                writerEOM.WriteMultiSample(false, values);
            }

            public int putValue_S(double[] values)
            {
                int error = 0;
                writerEOM_S = new AnalogMultiChannelWriter(hEOM_S.Stream);
                try
                {
                    writerEOM_S.WriteSingleSample(true, values);
                    hEOM_S.WaitUntilDone();
                }
                catch (DaqException ex)
                {
                    Debug.WriteLine("Problem in putValue_S" + ex.Message);
                    error = -1;
                }
                return error;
            }

            public void WaitUntioDone(int timeout)
            {
                try
                {
                    hEOM.WaitUntilDone(timeout);
                }
                catch (DaqException ex)
                {
                    Debug.WriteLine("Problem in EOM timeout" + ex.Message);
                    stop();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Problem in EOM waitUntilDone: " + ex.Message);
                }
            }


            public void startWithoutTrigger()
            {
                hEOM.Triggers.StartTrigger.ConfigureNone();
                hEOM.Start();
            }

            public void start(bool externalTrigger)
            {
                if (externalTrigger)
                    hEOM.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(ExternalTriggerInputPort, DigitalEdgeStartTriggerEdge.Rising);
                //else
                //    hEOM.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(triggerPort, DigitalEdgeStartTriggerEdge.Rising);

                hEOM.Start();
            }

            public void stop()
            {
                try
                {
                    hEOM.Stop();
                    hEOM.WaitUntilDone();
                }
                catch
                {

                }
                finally { }
            }

            public void dispose()
            {
                if (hEOM != null)
                {
                    hEOM.Dispose();
                }

                if (hEOM_S != null)
                    hEOM_S.Dispose();
            }

        }


        public class lineClock
        {
            public Task lineClockTask;
            //public Task frameClockTask;
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

            public lineClock(ScanParameters State_in, bool focus) //Constructor
            {
                State = State_in;

                lineClockPort = State.Init.lineClockPort;
                frameClockPort = State.Init.frameClockPort;

                GetTriggerPortName(lineClockPort, State, ref Board, ref TriggerPort, ref ExternalTriggerInputPort, ref sampleClockPort);

                msPerLine = State.Acq.msPerLine;
                if (State.Acq.fastZScan)
                    msPerLine = State.Acq.FastZ_msPerLine;

                nLines = State.Acq.linesPerFrame * State.Acq.nFrames;

                lineClockTask = new Task();
                frequency = 1000.0 / msPerLine;
                dutyCycle = 0.0001; //0.2 us

                freqFrame = 1000.0 / msPerLine / State.Acq.linesPerFrame;
                dutyFrame = dutyCycle / State.Acq.linesPerFrame;

                double delay = State.Acq.LineClockDelay / 1000.0;

                lineClockTask.COChannels.CreatePulseChannelFrequency(lineClockPort, "PulseTrain", COPulseFrequencyUnits.Hertz, COPulseIdleState.Low, delay, frequency, dutyCycle);

                lineClockTask.Triggers.StartTrigger.Type = StartTriggerType.DigitalEdge;
                lineClockTask.Triggers.StartTrigger.DigitalEdge.Edge = DigitalEdgeStartTriggerEdge.Rising;
                lineClockTask.Triggers.StartTrigger.DigitalEdge.Source = TriggerPort;

                int nRepeat = nLines + 2; //signal the last line. +1 necessary?
                if (focus)
                {
                    //nRepeat = 32767 * State.Acq.linesPerFrame;
                    lineClockTask.Timing.ConfigureImplicit(SampleQuantityMode.ContinuousSamples);
                }
                else
                {
                    lineClockTask.Timing.ConfigureImplicit(SampleQuantityMode.ContinuousSamples);
                    //lineClockTask.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, nRepeat);
                }
            }

            public void start(bool externalTrigger)
            {
                if (externalTrigger)
                    lineClockTask.Triggers.StartTrigger.DigitalEdge.Source = ExternalTriggerInputPort;
                //else
                //    lineClockTask.Triggers.StartTrigger.DigitalEdge.Source = TriggerPort;

                lineClockTask.Start();
            }

            public void stop()
            {
                try
                {
                    lineClockTask.Stop();
                    lineClockTask.WaitUntilDone();
                }
                catch (Exception Ex)
                {
                    Debug.WriteLine("Stop LineClock failed: " + Ex.Message);
                }
            }

            public void dispose()
            {
                if (lineClockTask != null)
                    lineClockTask.Dispose();
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

            int uncagingWidthCount = (int)((State.Uncaging.pulseWidth + State.Uncaging.Mirror_delay) * OutputRate / 1000.0);


            bool anyUncaging = State.Init.uncagingLasers.Any(item => item == true);

            double[,] uncagingPos = DefinePulsePosition(State);

            if (anyUncaging && State.Uncaging.MoveMirrorsToUncagingPosition)
            {
                for (int ch = 0; ch < nChannels; ch++) //pulse On.
                    for (int train = 0; train < nTrain; train++)
                        for (int pulse = 0; pulse < nUncaging; pulse++)
                            for (int j = 0; j < uncagingWidthCount; j++)
                            {
                                int loc = baseLineCount + train * train_intervalCount + pulse * pulse_intervalCount + j;
                                if (loc >= 0 && loc < nSamples)
                                    OutputFrame[ch, loc] = uncagingPos[ch, pulse];
                                //                                OutputFrame[ch, loc] = State.Uncaging.PositionV[ch];
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

            int nSamplesYScan = nSamples_All - nSamplesXFB;
            int nSamplesYFB = nSamples_All - nSamplesYScan;

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

                for (int j = 0; j < NLines; j++)
                {
                    for (int i = 0; i < nSamplesXScan; i++)
                    {
                        mirrorOutput[0, i + j * nSamplesX] = ((double)i / (double)nSamplesXScan - 0.5) * MaxVX;
                    }
                    for (int i = nSamplesXScan; i < nSamplesX; i++)
                    {
                        mirrorOutput[0, i + j * nSamplesX] = ((double)(nSamplesX - i) / (double)nSamplesXFB - 0.5) * MaxVX;


                        //mirrorOutput[0, i + j * nSamplesX] = ((double)(i - nSamplesX) / (double)nSamplesXScan - 0.5) * MaxVX;
                        //This protocol jumps to -0.5, and then move forward just like scanning. Maybe not good.....
                    }
                }
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

            voltagePosition[0, 0] = (FracPosition[0] - 0.5) * State.Acq.XMaxVoltage / State.Acq.zoom; //for disaply, multiplicator is not necessary.
            voltagePosition[1, 0] = (FracPosition[1] - 0.5) * State.Acq.YMaxVoltage / State.Acq.zoom;

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
