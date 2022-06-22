using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TagLensController
{
    public class TagCommand
    {
        // TAG LENS DRIVING LIMITS
        const bool debugMessage = false;
        const double MIN_FREQ = 30000;  // Hz
        const double MAX_FREQ = 525000;  // Hz
        public const double doubleAULT_FREQ = 75000;  // Hz

        const double MIN_AMP = 0.0;  // min amplitude in mV
        double MAX_AMP = 57017.0 / 2.0;  // amplitude max in mV (now is read from controller)
        const double MIN_AMP_100 = 0;  // minimum amplitude in %
        const double MAX_AMP_100 = 100;  // maximum amplitude in %
        public const double doubleAULT_AMP = 25;  // public doubleault amplitude 0 to 100

        // limits for the phase of VP1,VP2,VP//
        double MIN_VPX_PHASE = 0;
        double MAX_VPX_PHASE = 3600;
        double MIN_VPX_WIDTH = 0;
        double MAX_VPX_WIDTH = 2000;

        double MAX_FRAME_RATE = 500000.0;
        double MIN_FRAME_RATE = 0.1;


        double MIN_GATE_CYCLES = 0;
        double MAX_GATE_CYCLES = 10;

        public double double_CONN_TIMEOUT = 1.0;

        public int[] RGBVersion = new int[] { 2, 5, 0 };
        //int[] calibration;

        TagLControl tagc;
        public Calibration calib;

        public TagCommand()
        {
            tagc = new TagLControl();
        }

        public void CloseAll()
        {
            if (IsOpen())
                SetPiezoOff();
            Close();
        }

        public void Close()
        {
            tagc.Disconnect();
        }

        public bool Connect(String aPort)
        {
            try
            {
                tagc.Connect(aPort);
                bool success = GetVersion();

                if (success)
                {
                    calib = GetCalibration();
                }
                else
                {
                    Close();
                }
                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Opening port failed: " + ex.Message);
                return false;
            }
        }

        public int[] GetFabInfo()
        {
            GetCalibration();
            int[] info = new int[3];

            if (calib.calibration != null)
            {
                Array.Copy(calib.calibration, 10, info, 0, 3);
                return info;
            }
            else
                return null;
        }

        public bool GetVersion()
        {
            bool success = false;
            tagc.SendCommand("INF_VER_8B", 0, out String str, out int[] values);
            if (str != null && str.Length >= 8)
            {
                RGBVersion[0] = Convert.ToInt32(str.Substring(0, 2));
                RGBVersion[1] = Convert.ToInt32(str.Substring(3, 2));
                RGBVersion[2] = Convert.ToInt32(str.Substring(6, 2));
                success = true;
            }

            return success;
        }

        public bool IsOpen()
        {
            return tagc.IsConnected();
        }

        public bool SetPiezoOn()
        {
            tagc.SendCommand("CMD_PIEZO_SET_B", 1, out string str, out int[] values);
            if (values != null && values[0] == 1)
                return true;
            else
                return false;
        }

        public bool SetPiezoOff()
        {
            tagc.SendCommand("CMD_PIEZO_SET_B", 0, out string str, out int[] values);
            if (values != null && values[0] == 1)
                return true;
            else
                return false;
        }


        public Calibration GetCalibration()
        {
            calib = new Calibration();
            tagc.SendCommand("INF_CAL_ALL", 0, out string str, out int[] calibration);
            if (RGBVersion[0] >= 2)
                MAX_AMP = 30000;
            else
                MAX_AMP = calibration[8];

            calib.Values2Dict(calibration);
            return calib;
        }

        public Calibration SetCalibration(Calibration calc)
        {
            int LenCalConst = (int)(tagc.commands["CMD_CAL_ALL"][2]) / 4;
            if (calc.calConstants.Count == LenCalConst) //88 byte?
            {
                tagc.SendCommand("CMD_CAL_ALL", calib.calibration, out String str, out int[] calibration);
                calib.Values2Dict(calibration);
                if (RGBVersion[0] >= 2)
                    MAX_AMP = 30000;
                else
                    MAX_AMP = calibration[8];
            }
            else
                GetCalibration();
            return calib;
        }

        public Calibration WriteCalibration(Calibration calc)
        {
            int LenCalConst = (int)(tagc.commands["CMD_CAL_ALL"][2]) / 4;
            if (calc.calConstants.Count == LenCalConst) //88 byte?
            {
                tagc.SendCommand("CMD_CAL_ALL", calib.calibration, out String str, out int[] calibration);
                calib.Values2Dict(calibration);
                tagc.SendCommand("CMD_CAL_WRT", calib.calibration, out str, out calibration);
                calib.Values2Dict(calibration);
                if (RGBVersion[0] >= 2)
                    MAX_AMP = 30000;
                else
                    MAX_AMP = calibration[8];
            }
            else
                GetCalibration();
            return calib;
        }

        public double[] GetLensAFIVPPQ()
        {
            //Returns all lens state values at once
            //A = Amplitude 0 - 100(%)
            //F = Frequency(Hz)
            //I = RMS Current(mA)
            //V = RMS Voltage(V)
            //P = phase(deg)
            //P = Real Power(mW)
            //Q = Imag Power(mVA)
            //or None if there was a failure in reading the lens

            tagc.SendCommand("INF_LNS_AFIVPPQ", 0, out String str, out int[] AFIVPPQ);
            if (AFIVPPQ == null || AFIVPPQ.Length != 7)
            {
                return null;
            }
            else
            {
                double[] values = new double[AFIVPPQ.Length];
                values[0] = AFIVPPQ[0] * (100.0 / MAX_AMP);
                values[1] = AFIVPPQ[1] / 100.0;
                values[2] = AFIVPPQ[2] / 1000.0;
                values[3] = AFIVPPQ[3] / 1000.0;
                values[4] = AFIVPPQ[4] / 100.0;
                values[5] = AFIVPPQ[5] / 1000.0;
                values[6] = AFIVPPQ[6] / 1000.0;

                return values;
            }

        }

        public double GetRMSVoltage()
        {
            tagc.SendCommand("INF_SIN_VRM_mV", 0, out String str, out int[] ret);
            if (ret == null)
            {
                if (debugMessage)
                    Debug.WriteLine("Failed to get rms Voltage");
                return 0.0;
            }
            return (double)ret[0] / 1000.0; // return value in V
        }

        public double GetRMSCurrent_uA()
        {
            tagc.SendCommand("INF_SIN_IRM_uA", 0, out String str, out int[] ret);
            if (ret == null)
            {
                if (debugMessage)
                    Debug.WriteLine("Failed to get rms Current");
                return 0.0;
            }
            return (double)ret[0] / 1000.0; // return value in mA
        }

        public double GetCurrentPhase_deg()
        {
            tagc.SendCommand("INF_SIN_PHA_36000", 0, out String str, out int[] ret);
            if (ret == null)
            {
                if (debugMessage)
                    Debug.WriteLine("Failed to get Phase");
                return 0.0;
            }
            return (double)ret[0] / 100.0; // return value in deg
        }

        public double[] GetPQmWmVA()
        {
            tagc.SendCommand("INF_SIN_PQ_uWuVA", 0, out String str, out int[] ret);
            if (ret == null)
            {
                if (debugMessage)
                    Debug.WriteLine("Failed to get P and Q");
                return new double[] { 0.0, 0.0 };
            }
            return new double[] { (double)(ret[0] / 1000.0), (double)(ret[1] / 1000.0) };
        }


        public double GetFrequency()
        {
            tagc.SendCommand("INF_SIN_FRQ_cHz", 0, out String str, out int[] ret);
            if (ret == null)
            {
                if (debugMessage)
                    Debug.WriteLine("Failed to get frequency");
                return 0.0;
            }
            return (double)ret[0] / 1000.0; // return value in Hz
        }

        public double SetFrequency(double frequency_Hz)
        {
            if (MIN_FREQ <= frequency_Hz && frequency_Hz < MAX_FREQ)
            {
                tagc.SendCommand("CMD_SIN_FRQ_cHz", (int)frequency_Hz * 100, out String str, out int[] ret);
                return (double)ret[0] / 100.0;
            }
            else
            {
                if (debugMessage)
                    Debug.WriteLine("Frequency must be between " + MIN_FREQ + " and " + MAX_FREQ + " Hz");
                return GetFrequency();
            }
        }

        public void CheckFrequency(double frequency_Hz)
        {
            // sets the frequency and checks the set value is correct //
            double freq = SetFrequency(frequency_Hz);
            if (debugMessage)
                Debug.WriteLine("Set Frequency: {0:0.00} Hz", freq);
            Debug.Assert(Math.Abs(freq - frequency_Hz) <= 0.1);
            // test get_frequency
            freq = GetFrequency();
            if (debugMessage)
                Debug.WriteLine("Get Frequency: {0:0.00} Hz", freq);
            Debug.Assert(Math.Abs(freq - frequency_Hz) <= 0.1);
        }

        public double GetAmplitude_0to100()
        {
            // returns the amplitude as a number 0 to 100 //
            tagc.SendCommand("INF_SIN_AMP_mV", 0, out String str, out int[] ret);
            if (ret == null)
            {
                if (debugMessage)
                    Debug.WriteLine("Failed to get amplitude");
                return 0.0;
            }
            return (double)ret[0] * (100.0 / MAX_AMP);
        }

        public double SetAmplitude_0to100(double amplitude)
        {
            // validate amplitude
            if (amplitude >= MIN_AMP_100 && amplitude <= MAX_AMP_100)
            {
                amplitude = amplitude / 100.0 * MAX_AMP;
                tagc.SendCommand("CMD_SIN_AMP_mV", (int)amplitude, out String str, out int[] ret);
                return (double)ret[0] * (100.0 / MAX_AMP);
            }
            else
            {
                if (debugMessage)
                    Debug.WriteLine("Amplitude must be between 0 && 100");
                return -1; // GetAmplitude_0to100();
            }
        }

        public void CheckAmplitude_0to100(double setAmplitude)
        {
            // sets the amplitude && checks the set value is correct //
            double amplitude = SetAmplitude_0to100(setAmplitude);
            if (debugMessage)
                Debug.WriteLine("Set Amplitude: {0:0.00} %%", amplitude);
            Debug.Assert(Math.Abs(amplitude - setAmplitude) <= 0.02);
            // tet get_amplitude
            amplitude = GetAmplitude_0to100();
            Debug.Assert(Math.Abs(amplitude - setAmplitude) <= 0.02);
            if (debugMessage)
                Debug.WriteLine("Get Amplitude: {0:0.00} %%", amplitude);
        }



        // LOCK FUNCTIONS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        

        public int EnableLockLens(double amp_0_100, double minAmp_0_100 = 0, double maxAmp_0_100 = 100, double ampStep_0_100 = 1, double minFreq_Hz = MIN_FREQ, double maxFreq_Hz = MAX_FREQ, double freqStep_Hz = 10.0)
        {
            int[] lockState;
            // starts the frequency locking if values are within the allowed ranges//
            if (amp_0_100 >= MIN_AMP_100 && amp_0_100 <= MAX_AMP_100 &&
            minAmp_0_100 >= MIN_AMP_100 &&
            maxAmp_0_100 >= minAmp_0_100 &&
            maxAmp_0_100 <= MAX_AMP_100 &&
            ampStep_0_100 >= 0.1 &&
            ampStep_0_100 <= 5.0 &&
            minFreq_Hz >= MIN_FREQ &&
            maxFreq_Hz <= MAX_FREQ &&
            freqStep_Hz >= 0.1 &&
            freqStep_Hz <= 100.0)
            {
                int amplitude_mV = (int)(amp_0_100 / 100.0 * MAX_AMP);
                int minAmplitude_mV = (int)(minAmp_0_100 / 100.0 * MAX_AMP);
                int maxAmplitude_mV = (int)(maxAmp_0_100 / 100.0 * MAX_AMP);
                int ampStep_mV = (int)(ampStep_0_100 / 100.0 * MAX_AMP);

                int minFreqHz = (int)(minFreq_Hz * 100.0);
                int maxFreqHz = (int)(maxFreq_Hz * 100.0);
                int freqStepHz = (int)(freqStep_Hz * 100.0);
                tagc.SendCommand("CMD_LOCK_LENS", new int[] { amplitude_mV, minAmplitude_mV, maxAmplitude_mV, ampStep_mV, minFreqHz, maxFreqHz, freqStepHz }, out String str, out lockState);
            }
            else
            {
                if (debugMessage)
                    Debug.WriteLine("Lock Lens Argument Invalid");
                tagc.SendCommand("INF_LOCK_STATE", 0, out String str, out lockState);
            }

            return lockState[0];
        }


        public int DisableLock()
        {
            // disables the lock && return the state of the lock //
            int[] lockState;
            tagc.SendCommand("CMD_LOCK_LENS", new int[] { -1, -1, -1, -1, -1, -1, -1 }, out String str, out lockState);
            if (lockState == null)
            {
                if (debugMessage)
                    Debug.WriteLine("Lock Lens Argument Invalid");
                return -1;
            }
            else
                return lockState[0];
        }

        public int GetLockPhase()
        {
            // state is 0 if unlocked, 1 if scanning, 2, 3 or 4 if locked//
            int[] lockState;
            tagc.SendCommand("INF_LOCK_STATE", 0, out String str, out lockState);
            return lockState[0];
        }

        public double[] GetLockInfo()
        {
            // returns information about the lock:
            //0) minF,maxF,freqStepScan,FreqStepLock,minAmp,maxAmp,ampStep
            //7) powerDev,currentDev
            //9) lockInitialFreq, lockInitialFreqStep, lockInitialAmp, lockInitialRMSVoltage,
            //   lockInitialRMSCurrent, lockInitialPhase, lockInitialPower
            //16) lockLastPeakCurrent,lockLastPeakPower
            //18) freqDer,pwrDer,freqStep 
            //if it fails to get the info returns None
            //
            tagc.SendCommand("INF_DEBUG1_GET", 0, out String str, out int[] ret);
            if (ret == null)
                // public doubleault reading in case of error
                return null;

            double[] convFact = new double[] {100.0, 100.0, 100.0, 100.0, MAX_AMP, MAX_AMP, MAX_AMP,
                    1000.0, 1000.0,
                    100.0, 100.0, 1000.0, 1000.0, 1000.0, 1000.0, 1000.0,
                    1000.0, 1000.0,
                    100.0, 1000.0, 100.0 };

            double[] values = new double[ret.Length];

            for (int indx = 0; indx < ret.Length; indx++)
                values[indx] = (double)ret[indx] / convFact[indx];

            return values;
        }

        // PULSING FUNCTIONS //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public int GetPulseMode()
        {
            // returns the pulse mode currently selected //
            int[] ret;
            tagc.SendCommand("INF_PULSE_STATE", 0, out String str, out ret);
            if (ret == null)
            {
                if (debugMessage)
                    Debug.WriteLine("Failed to get Pulse Mode");
                return 0;  // pretend the pulse mode is off
            }
            else
                return ret[0];
        }

        public bool PulseOff()
        {
            // disables the pulse outputs (RGB,Multiplane,F3) //
            int[] ret;
            tagc.SendCommand("CMD_PULSE_OFF", 0, out String str, out ret);
            if (ret == null)
            {
                if (debugMessage)
                    Debug.WriteLine("Failed to set Pulse Mode Off");
                return false;  // pretend the pulse mode is off
            }
            else
                return true;
        }

        public bool SetRGBPhaseDuration(double[] phase, double[] duration, out double[] phase_out, out double[] duration_out)
        {
            // sets the phase && duration of the three RGB pulses VP1 VP2 VP3 (0.0-360.0,nS)
            //&& returns the set phases && durations //
            // need to add range checking && warning
            List<int> phaDur = new List<int>();
            for (int indx = 0; indx < 3; indx++)
            {
                phaDur.Add((int)(phase[indx] * 100.0));
                phaDur.Add((int)(duration[indx]));
            }

            int[] ret;
            tagc.SendCommand("CMD_RGB_PHA_36000_DUR_nS", phaDur.ToArray(), out String str, out ret);

            if (ret == null)
            {
                if (debugMessage)
                    Debug.WriteLine("Failed to set RGB Phase Duration");
                phase_out = phase;  // pretend you had set the right one
                duration_out = duration;
                return false;
            }
            else
            {
                phase_out = new double[] { 0, 0, 0 };
                duration_out = new double[] { 0, 0, 0 };
                for (int indx = 0; indx < 3; indx++)
                {
                    phase_out[indx] = (double)ret[2 * indx] / 100.0;
                    duration_out[indx] = (double)ret[2 * indx + 1];
                }
                return true;
            }
        }

        public void GetRGBPhaseDuration(out double[] phase_out, out double[] duration_out)
        {
            // reads the phase && duration of VP1 VP2 VP3 (0.0-360.0,nS) for the RGB mode//
            int[] ret;
            tagc.SendCommand("INF_RGB_PHA_36000_DUR_nS", 0, out String str, out ret);
            phase_out = new double[] { 0, 0, 0 };
            duration_out = new double[] { 0, 0, 0 };
            if (ret != null)
            {
                for (int indx = 0; indx < 3; indx++)
                {
                    phase_out[indx] = (double)ret[2 * indx] / 100.0;
                    duration_out[indx] = (double)ret[2 * indx + 1];
                }
            }
            else
            {
                if (debugMessage)
                    Debug.WriteLine("Failed to get RGB Phase Duration");
            }
        }

        // sets pha (0.0-360.0) && dur (nS) for three pulses (Multipulse Mode)//
        // need to add range checking && warning
        public void SetMultipulsePhaDur(double[] phase, double[] duration, out double[] phase_out, out double[] duration_out)
        {
            List<int> phaDur = new List<int>();
            for (int indx = 0; indx < 3; indx++)
            {
                phaDur.Add((int)(phase[indx] * 10.0));
                phaDur.Add((int)(duration[indx]));
            }

            int[] ret;
            tagc.SendCommand("CMD_VP1M_PH_DUR", phaDur.ToArray(), out string str, out ret);
            if (ret != null)
            {
                phase_out = new double[] { 0, 0, 0 };
                duration_out = new double[] { 0, 0, 0 };
                for (int indx = 0; indx < 3; indx++)
                {
                    phase_out[indx] = (double)ret[2 * indx] / 10.0;
                    duration_out[indx] = (double)ret[2 * indx + 1];
                }
            }
            else
            {
                // pretend we set the values
                if (debugMessage)
                    Debug.WriteLine("Failed to set Multiphase Phase Duration");
                phase_out = phase;
                duration_out = duration;
            }
        }


        public void GetMultipulsePhaDur(out double[] phase_out, out double[] duration_out)
        {
            // reads pha (0.0-360.0) && dur (nS) for three pulses (Multipulse Mode)//
            int[] ret;
            tagc.SendCommand("INF_VP1M_PH_DUR", 0, out String str, out ret);
            if (ret != null)
            {
                phase_out = new double[] { 0, 0, 0 };
                duration_out = new double[] { 0, 0, 0 };
                for (int indx = 0; indx < 3; indx++)
                {
                    phase_out[indx] = (double)ret[2 * indx] / 10.0;
                    duration_out[indx] = (double)ret[2 * indx + 1];
                }
            }
            else
            {
                // public doubleault return value in case of failure
                if (debugMessage)
                    Debug.WriteLine("Failed to set Multiphase Phase Duration");
                phase_out = new double[] { 0, 0, 0 };
                duration_out = new double[] { 100, 100, 100 };
            }
        }

        // Sets numPhases, frameRate &&  pha (0.0-360.0) && dur (nS) for six pulses (F3 Mode) 
        //irrespective of the number of phases selected 
        //Values will be updated//
        public void SetF3NphFrPhaDur(ref int numPhases, ref double frameRate, double[] phase, double[] duration)
        {
            if (numPhases > 6)
            {
                if (debugMessage)
                    Debug.WriteLine("Num Phases greater than 6");
                numPhases = 6;
            }
            int[] ret;
            tagc.SendCommand("INF_SIN_FRQ_cHz", 0, out String str, out ret);
            int freq_cHz = ret[0];

            if (frameRate > MAX_FRAME_RATE)
            {
                if (debugMessage)
                    Debug.WriteLine("Frame rate too high, max {0} Hz", MAX_FRAME_RATE);
                frameRate = MAX_FRAME_RATE;
            }

            if (frameRate < MIN_FRAME_RATE)
            {
                if (debugMessage)
                    Debug.WriteLine("Frame rate too low, min {0} Hz", MIN_FRAME_RATE);
                frameRate = MIN_FRAME_RATE;
            }

            int numCycles = (int)(freq_cHz / (frameRate * 100.0));
            List<int> cyclPhaDur = new List<int>();
            cyclPhaDur.Add(numPhases);
            cyclPhaDur.Add(numCycles);
            for (int indx = 0; indx < 6; indx++)
            {
                cyclPhaDur.Add((int)(phase[indx] * 10.0));
                cyclPhaDur.Add((int)duration[indx]);
            }

            tagc.SendCommand("CMD_F3_NP_CYC_PHA_DUR", cyclPhaDur.ToArray(), out str, out ret);
            if (ret != null)
            {
                numPhases = ret[0];
                frameRate = freq_cHz / ret[1] / 100.0;
                for (int indx = 0; indx < 6; indx++)
                {
                    phase[indx] = (int)(ret[2 * indx + 2] / 10.0);
                    duration[indx] = ret[2 * indx + 3];
                }
            }
        }

        public void GetF3MphFrPhaDur(ref int numPhases, ref double frameRate, double[] phase, double[] duration)
        {
            // reads active num phases, cycles &&  pha (0.0-360.0) && dur (nS)
            //for six pulses(F3 Mode)//
            int[] ret;

            tagc.SendCommand("INF_SIN_FRQ_cHz", 0, out string str, out ret);
            double freq = ret[0] / 100.0;

            tagc.SendCommand("INF_F3_NP_CYC_PHA_DUR", 0, out str, out ret);
            // if type(ret) is not bool :
            numPhases = ret[0];
            frameRate = freq / ret[1];

            phase = new double[6];
            duration = new double[6];
            for (int indx = 0; indx < 6; indx++)
            {
                phase[indx] = (int)(ret[2 * indx + 2] / 10.0);
                duration[indx] = ret[2 * indx + 3];
            }
            // else:
            //    Debug.WriteLine "Error:get_F3_nph_fr_pha_dur"
            //    return 0,0,[0,0,0,0,0,0],[0,0,0,0,0,0]
        }

        public bool GetGating(int numCycles)
        {
            // returns the current gating settings (gating on or off, number of cycles)
            //
            int[] gatingStatus;
            tagc.SendCommand("INF_GAT_NPULSES", 0, out string str, out gatingStatus);
            if (gatingStatus[0] >= 0)
            {
                numCycles = gatingStatus[0];
                return true; // gated,gatingStatus[0] cycles per gate pulse
            }
            else
            {
                numCycles = 0;
                return false;
            }
        }

        public bool SetGating(bool enabled, int numCycles, out int numCycle_out)
        {
            // sets the gating mode && returns the current gating settings (gating on or off, number of cycles)
            //
            int[] gatingStatus;
            if (enabled)
                tagc.SendCommand("CMD_GAT_NPULSES", numCycles, out string str, out gatingStatus);
            else
                tagc.SendCommand("CMD_GAT_NPULSES", -1, out string str, out gatingStatus);


            if (gatingStatus[0] >= 0)
            {
                numCycle_out = gatingStatus[0];
                return true; // gated,gatingStatus[0] cycles per gate pulse
            }
            else
            {
                numCycle_out = 0;
                return false; // not gated,0 cycle
            }
        }

        public void SetDefault(out double frequency, out double amplitude)
        {
            //Return the device to the public doubleault settings.//
            frequency = SetFrequency(75000);
            amplitude = SetAmplitude_0to100(25);
        }



        public class Calibration
        {
            public Dictionary<String, int[]> calConstants;
            public int[] calibration;

            public List<Dictionary<String, double>> defResAll;

            public Calibration()
            {
                calConstants = new Dictionary<string, int[]>();
                calConstants.Add("Vin_slope", new int[] { 25575, 0, 0 });
                calConstants.Add("Vpiezo_slope", new int[] { 1, 1, 0 });
                calConstants.Add("Ipiezo_slope", new int[] { 1, 2, 0 });
                calConstants.Add("Vrms_offset", new int[] { 170, 3, 0 });
                calConstants.Add("Vrms_slope", new int[] { 53 * 1024, 4, 0 });
                calConstants.Add("Irms_offset", new int[] { 0, 5, 0 });
                calConstants.Add("Irms_slope", new int[] { 350, 6, 0 });
                calConstants.Add("Phase_offset", new int[] { 36000 + 21800, 7, 0 });
                calConstants.Add("Vsin_offset", new int[] { (int)(57677.0 / 2.0), 8, 0 });
                calConstants.Add("Vsin_slope", new int[] { (int)(59.933 / 2.0), 9, 0 });
                calConstants.Add("SerialNumber", new int[] { 0, 10, 0 });
                calConstants.Add("FabricationDate", new int[] { 20150130, 11, 0 });
                calConstants.Add("phaFreqCal_0", new int[] { 1, 0, 1 });
                calConstants.Add("phaFreqCal_1", new int[] { 0, 1, 1 });
                calConstants.Add("phaFreqCal_2", new int[] { 0, 2, 1 });
                calConstants.Add("phaFreqCal_3", new int[] { 0, 3, 1 });
                calConstants.Add("phaFreqCal_4", new int[] { 0, 4, 1 });
                calConstants.Add("IrmsFreqCal_0", new int[] { 1, 5, 1 });
                calConstants.Add("IrmsFreqCal_1", new int[] { 0, 6, 1 });
                calConstants.Add("IrmsFreqCal_2", new int[] { 0, 7, 1 });
                calConstants.Add("IrmsFreqCal_3", new int[] { 0, 8, 1 });
                calConstants.Add("IrmsFreqCal_4", new int[] { 0, 9, 1 });
                Dict2Values(calConstants);

                defResAll = new List<Dictionary<string, double>>();

                Dictionary<string, double> defRes = new Dictionary<string, double>();
                defRes.Add("Res.Freq(Hz)", 69000.00);
                defRes.Add("Min.Freq(Hz)", 67000.00);
                defRes.Add("Max.Freq(Hz)", 72000.00);
                defRes.Add("Stp.Freq(Hz)", 5.00);
                defRes.Add("Res.Amp(%)", 45.0);
                defRes.Add("Min.Amp(%)", 10.0);
                defRes.Add("Max.Amp(%)", 90.0);
                defRes.Add("Stp.Amp(%)", 1.00);
                defRes.Add("Opt.Slope(D/mW)", 1.0);
                defRes.Add("Opt.Offs(D)", 0.0);
                defRes.Add("Z Ph0 (um)", 0.0);
                defRes.Add("Z Ph180 (um)", 100.0);

                defResAll.Add(defRes);

                defRes = new Dictionary<string, double>();
                defRes.Add("Res.Freq(Hz)", 188000.0);
                defRes.Add("Min.Freq(Hz)", 186000.0);
                defRes.Add("Max.Freq(Hz)", 192000.0);
                defRes.Add("Stp.Freq(Hz)", 5.00);
                defRes.Add("Res.Amp(%)", 25.0);
                defRes.Add("Min.Amp(%)", 5.0);
                defRes.Add("Max.Amp(%)", 90.0);
                defRes.Add("Stp.Amp(%)", 1.00);
                defRes.Add("Opt.Slope(D/mW)", 1.0);
                defRes.Add("Opt.Offs(D)", 0.0);
                defRes.Add("Z Ph0 (um)", 0.0);
                defRes.Add("Z Ph180 (um)", 100.0);

                defResAll.Add(defRes);

                defRes = new Dictionary<string, double>();
                defRes.Add("Res.Freq(Hz)", 310000.0);
                defRes.Add("Min.Freq(Hz)", 309000.0);
                defRes.Add("Max.Freq(Hz)", 313000.0);
                defRes.Add("Stp.Freq(Hz)", 5.00);
                defRes.Add("Res.Amp(%)", 10.0);
                defRes.Add("Min.Amp(%)", 2.0);
                defRes.Add("Max.Amp(%)", 25.0);
                defRes.Add("Stp.Amp(%)", 1.00);
                defRes.Add("Opt.Slope(D/mW)", 1.0);
                defRes.Add("Opt.Offs(D)", 0.0);
                defRes.Add("Z Ph0 (um)", 0.0);
                defRes.Add("Z Ph180 (um)", 100.0);

                defResAll.Add(defRes);
            }

            public Dictionary<String, int[]> Values2Dict(int[] calibration_in)
            {
                calibration = calibration_in;
                for (int i = 0; i < calibration.Length; i++)
                    calConstants[calConstants.Keys.ElementAt(i)][0] = calibration[i];
                return calConstants;
            }

            public int[] Dict2Values(Dictionary<string, int[]> dict_in)
            {
                calConstants = dict_in;
                calibration = new int[calConstants.Count];
                for (int i = 0; i < calibration.Length; i++)
                    calibration[i] = calConstants[calConstants.Keys.ElementAt(i)][0];
                return calibration;
            }

            public void LoadLensFile(String filePath)
            {
                if (File.Exists(filePath))
                {
                    String mode = "";
                    int resonanceID = -1;
                    using (StreamReader sr = File.OpenText(filePath))
                    {
                        string s = "";
                        while ((s = sr.ReadLine()) != null)
                        {
                            try
                            {
                                if (s.Contains("[calibration]"))
                                    mode = "calibration";
                                else if (s.Contains("[drivingkit]"))
                                    mode = "drivingkit";
                                else if (s.Contains("[[resonances]]"))
                                    mode = "resonances";

                                if (mode == "calibration")
                                {
                                    String[] sP = s.Split('=');
                                    String key1 = sP[0];
                                    String valueStr = sP[1];
                                    Regex.Replace(key1, @"\s+", "");
                                    Regex.Replace(valueStr, @"\s+", "");
                                    calConstants[key1][0] = Convert.ToInt32(valueStr);
                                }
                                else if (mode == "resonances")
                                {
                                    if (s.Contains("[[["))
                                    {
                                        resonanceID++;
                                        String s1 = s.Replace("[", "").Replace("]", "");
                                        Regex.Replace(s1, @"\s+", "");
                                        defResAll[resonanceID]["Res.Freq(Hz)"] = Convert.ToDouble(s1);
                                    }
                                    else
                                    {
                                        String[] sP = s.Split('=');
                                        String key1 = sP[0];
                                        String valueStr = sP[1];
                                        Regex.Replace(key1, @"\s+", "");
                                        Regex.Replace(valueStr, @"\s+", "");
                                        defResAll[resonanceID][key1] = Convert.ToDouble(valueStr);
                                    }
                                }
                            }
                            catch
                            {
                                if (debugMessage)
                                    Debug.WriteLine("Problem in " + s);
                            }
                        }
                        sr.Close();
                    }
                }
            }
        }

    }
}
