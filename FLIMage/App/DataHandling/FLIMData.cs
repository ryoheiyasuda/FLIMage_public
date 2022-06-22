using FLIMage.Analysis;
using MathLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace FLIMage
{
    public class FLIMData
    {
        const int N_MAXCHANNEL = 4;

        public int width; //X
        public int height; //Y
        public int[] n_time; //Time
        public bool ZStack;
        public int nFastZ = 1; //Z
        public int n_pages = 1;
        public int n_pages5D = 1;

        //These data are read-only.
        public UInt16[][][,,] FLIMRaw5D { get; private set; } //[channel][z][y,x,t]
        public UInt16[][,,] FLIMRaw { get; private set; } //[channel][y,x,t]
        public UInt16[][,,] FLIMRawZProjection { get; private set; } //[channel][y,x,t]
        public UInt16[][,] Project { get; private set; } //[ch][y,x]
        public UInt16[][,] ProjectF { get; private set; } //[ch][y,x]
        public float[][,] LifetimeMapBase { get; private set; }  //[channel][y][x]
        public float[][,] LifetimeMap { get; private set; }  //[channel][y][x]
        public float[][,] LifetimeMapF { get; private set; }  //[channel][y][x]
        public bool[] ProjectCalculated { get; private set; }
        public bool[] FLIMMapCalculated { get; private set; }
        public bool LifetimeCalculated { get; private set; }
        public DateTime acquiredTime { get; private set; }

        public UInt16[][][][,,] FLIM_Pages5D { get; private set; }  //[page][c][z][data]
        public UInt16[][][,,] FLIM_Pages { get; private set; }  //[page][c][data]
        public UInt16[][][,] Project_Pages { get; private set; } //[page][c][y][x]
        public float[][][,] LifetimeMapBase_Pages { get; private set; }
        public bool[] ProjectCalculated_Pages { get; private set; }
        public bool[] FLIMMapCalculated_Pages { get; private set; }
        public bool[] LifetimeCalculated_Pages { get; private set; }
        public DateTime[] acquiredTime_Pages { get; private set; }
        public DateTime[] acquiredTime_Pages5D { get; private set; }

        public bool page_timecourse;

        public double[] offset;
        public int[][] fit_range;

        public double psPerUnit;
        public double msPerLine;
        public int nChannels;
        public int nSlices;
        public int nFrames;
        public int nAveFrame;
        public int[] nAveragedFrame = new int[] { 1, 1 };
        public bool[] aveFrames = new bool[2];

        public bool[] saveChannels = new bool[] { true, true };

        public bool FLIM_on_eachChannel = false;
        //
        public bool[] FLIM_on = new bool[] { true, true };
        public projectionType z_projection_type = projectionType.Max;
        //public bool saveFLIMPageInMemory = false;
        public bool ZProjection = false;
        public int[] ZProjection_Range = new int[] { 0, 0 };
        public bool ZProjectionCalculated = false;
        //
        //
        public int currentPage = -1;
        public int currentPage5D = -1;
        //public int currentFastZ = -1;

        public double[] low_threshold;

        //public double[] beta;
        //public List<double[]> fittingList { get; private set; }
        //public List<double[]> Residuals { get; private set; }
        //public List<double[]> fitCurve { get; private set; }
        //public double[] xi_square;
        //public double[] tau_m;
        //public double[] offset_fit;

        public int[] syncRate;
        public int[] countRate;

        //public Rectangle roi;
        public ROI Roi;
        public ROI bgRoi;
        public ROI RoiFit;
        public List<ROI> ROIs;
        public int currentRoi;
        public FitType Fit_type = FitType.WholeImage;
        public bool ThreeDRoi = false;
        //

        public bool KeepPagesInMemory;
        //
        public string baseName = "Test";
        public int fileCounter = 0;
        public string fileName = "Test001";
        public string fullFileName = "Test001";
        public string pathName = "C:\\";
        public string fileExtension = ".flim";
        public bool numberedFile = true;

        public string image_description = "";

        object FLIM_Pages_lockfor5D = new object();

        public FileIO.FileFormat format = FileIO.FileFormat.None;
        public int imagesPerFile = 1;
        public ScanParameters State;
        //public FileIO fileIO;

        public FLIMData(ScanParameters Scan, FLIMData flim1, bool recoverRoi)
        {
            FLIM_DataConstruct(Scan, !recoverRoi);
            if (recoverRoi)
            {
                Roi = new ROI(flim1.Roi);
                RoiFit = new ROI(flim1.RoiFit);
                ROIs.Clear();
                foreach (var roi in flim1.ROIs)
                {
                    ROIs.Add(new ROI(roi));
                }
                currentRoi = flim1.currentRoi;
                Fit_type = flim1.Fit_type;

                LifetimeCalculated = flim1.LifetimeCalculated;
                LifetimeCalculated_Pages = (bool[])flim1.LifetimeCalculated_Pages.Clone();
                RoiFit.flim_parameters = flim1.RoiFit.flim_parameters.Copy();
                RoiFit.flim_parameters_Pages = new ROI_FLIM_Parameters[flim1.RoiFit.flim_parameters_Pages.Length];
                for (int i = 0; i < RoiFit.flim_parameters_Pages.Length; i++)
                    RoiFit.flim_parameters_Pages[i] = flim1.RoiFit.flim_parameters_Pages[i].Copy();
            }
        }

        public FLIMData(ScanParameters Scan)
        {
            FLIM_DataConstruct(Scan, true);
        }

        public void InitializeData(ScanParameters Scan, bool recoverRoi)
        {
            RoiFit = new ROI();
            RoiFit.flim_parameters.initializeParams(nChannels, n_time);
            RoiFit.initialize_flimParameter_Pages(nChannels, n_time, n_pages);

            clearMemory();
            var currentRoi1 = currentRoi;
            var fittype = Fit_type;
            FLIM_DataConstruct(Scan, !recoverRoi);
            if (recoverRoi)
            {
                currentRoi = currentRoi1;
                Fit_type = fittype;
            }
        }

        public void FLIM_DataConstruct(ScanParameters Scan, bool updateROI)
        {
            FileIO fileIO = new FileIO(Scan);
            State = fileIO.CopyState(fileIO.headerList_nonDevice);
            width = State.Acq.pixelsPerLine;
            height = State.Acq.linesPerFrame;

            z_projection_type = projectionType.Max;

            if (State.Acq.ZStack && State.Acq.nSlices > 1 && State.Acq.sliceStep > 0.0)
                ZStack = true;

            if (State.Acq.fastZScan)
            {
                nFastZ = State.Acq.FastZ_nSlices;
            }
            else
                nFastZ = 1;

            FLIM_on = State.Acq.acqFLIMA;
            psPerUnit = State.Spc.spcData.resolution[0];
            offset = State.Spc.analysis.offset;

            LifetimeCalculated = false;

            syncRate = new int[nChannels]; // State.Spc.datainfo.syncRate;
            countRate = new int[nChannels]; // State.Spc.datainfo.countRate;
            for (int j = 0; j < nChannels; j++)
            {
                syncRate[j] = State.Spc.datainfo.syncRate[j];
                countRate[j] = State.Spc.datainfo.countRate[j];
            }
            nAveFrame = State.Acq.nAveFrame;

            nChannels = State.Acq.nChannels;

            nAveragedFrame = Enumerable.Repeat<int>(State.Acq.nAveFrame, nChannels).ToArray();

            for (int c = 0; c < nChannels; c++)
            {
                if (!State.Acq.aveFrameA[c])
                    nAveragedFrame[c] = 1;
            }

            n_time = Enumerable.Repeat<int>(State.Spc.spcData.n_dataPoint, nChannels).ToArray();

            for (int i = 0; i < n_time.Length; i++)
            {
                if (!State.Acq.acqFLIMA[i])
                    n_time[i] = 1;
                if (!State.Acq.acquisition[i])
                    n_time[i] = 0;
            }

            FLIMMapCalculated = new bool[nChannels];
            ProjectCalculated = new bool[nChannels];
            LifetimeCalculated = false;

            FLIMRaw5D = new UInt16[nChannels][][,,];

            FLIMRaw = new UInt16[nChannels][,,];

            fit_range = new int[nChannels][];

            low_threshold = new double[nChannels];

            acquiredTime = DateTime.Now;

            FLIMRaw5D = MatrixCalc.makeNew5DSlice(nChannels, nFastZ, height, width, n_time);

            for (int i = 0; i < nChannels; i++)
            {
                FLIMRaw[i] = new ushort[height, width, n_time[i]];
            }

            clearMemory();

            if (State.Acq.fastZScan && State.Acq.FastZ_nSlices > 1)
            {
                nFastZ = State.Acq.FastZ_nSlices;
                FLIM_Pages = new ushort[nFastZ][][,,];
                for (int z = 0; z < nFastZ; z++)
                    FLIM_Pages[z] = new ushort[nChannels][,,];
                if (currentPage >= 0 && currentPage < nFastZ)
                    ;
                else
                    currentPage = -1;
            }
            else
            {
                n_pages = 0;
                nFastZ = 1;
                currentPage = -1;
            }

            n_pages5D = 0;
            currentPage5D = -1;

            if (updateROI)
            {
                ROIs = new List<ROI>();
                ResetRoi(true);
            }

            loadFittingParamFromState();

            //intitializeAll();
        }

        public void PutROIs(List<ROI> rois)
        {
            ROIs.Clear();
            foreach (ROI roi1 in rois)
            {
                var roi_copy = new ROI(roi1);
                ROIs.Add(roi_copy);
            }

        }


        public List<ROI> CopyROIs()
        {
            List<ROI> rois = new List<ROI>();
            foreach (ROI roi1 in ROIs)
            {
                var roi_copy = new ROI(roi1);
                rois.Add(roi_copy);
            }

            return rois;
        }

        public void loadFittingParamFromState()
        {
            double[] X1 = new double[n_time[0]];
            fit_range = new int[nChannels][];

            RoiFit = new ROI(ROI.ROItype.Rectangle, new Rectangle(0, 0, width, height), nChannels, -100, false, new int[] { 0, 0 });
            RoiFit.flim_parameters.initializeParams(nChannels, n_time);

            for (int ch = 0; ch < nChannels; ch++)
            {
                double[] intensity_range = (double[])State.Display.GetType().GetField("Intensity_Range" + (ch + 1)).GetValue(State.Display);
                low_threshold[ch] = intensity_range[0];
                int[] fit_range1 = (int[])State.Spc.analysis.GetType().GetField("fit_range" + (ch + 1)).GetValue(State.Spc.analysis);
                double[] beta1 = (double[])State.Spc.analysis.GetType().GetField("fit_param" + (ch + 1)).GetValue(State.Spc.analysis);


                fit_range[ch] = (int[])fit_range1.Clone();
                double[] beta = (double[])beta1.Clone();

                double res = psPerUnit / 1000.0;
                beta[1] = res / beta[1];
                beta[3] = res / beta[3];
                beta[4] = beta[4] / res;
                beta[5] = beta[5] / res;

                RoiFit.flim_parameters.beta[ch] = beta;
                RoiFit.flim_parameters.beta0[ch] = (double[])beta.Clone();
                RoiFit.flim_parameters.residual[ch] = X1;
                RoiFit.flim_parameters.fitCurve[ch] = (double[])X1.Clone();
            }
        }

        public void copyState(ScanParameters Scan)
        {
            FileIO fileIO = new FileIO(Scan);
            State = fileIO.CopyState(); //It has its own State!
        }

        public bool SetRoi(ROI roi1)
        {
            bool success = false;
            if (roi1 != null)
            {
                if (roi1.Rect.X >= 0 && roi1.Rect.Y >= 0 && roi1.Rect.Width > 0 && roi1.Rect.Height > 0
                    && roi1.Rect.Right <= width && roi1.Rect.Bottom <= height)
                {
                    Roi = new ROI(roi1.ROI_type, roi1.X, roi1.Y, nChannels, roi1.polyLineROI_Radius, 0, roi1.Roi3d, roi1.Z);
                    success = true;
                }
            }
            return success;
        }

        public void ResetRoi(bool resetAll)
        {
            Rectangle rect = new Rectangle();
            rect.Location = new Point(0, 0);
            rect.Width = width;
            rect.Height = height;

            Roi = new ROI(ROI.ROItype.Rectangle, rect, nChannels, 0, false, new int[] { 0 });

            if (resetAll)
            {
                ROIs.Clear();
                rect = new Rectangle();
                rect.Location = new Point(0, 0);
                rect.Width = 0;
                rect.Height = 0;
                bgRoi = new ROI(ROI.ROItype.Rectangle, rect, nChannels, 0, false, new int[] { 0 });
            }
        }


        public void addToMultiRoi(ROI Roi1, int id)
        {
            ROI roi = new ROI(Roi1);
            roi.ID = id;
            ROIs.Add(roi);
            currentRoi = ROIs.Count - 1;
        }

        public void addToMultiRoi(ROI Roi1)
        {
            ROI roi = new ROI(Roi1);
            if (ROIs.Count == 0)
                roi.ID = 0;
            else
                roi.ID = ROIs.Select(x => x.ID).ToArray().Max() + 1;

            ROIs.Add(roi);
            //Roi = new ROI(Roi.ROI_type, Roi.X, Roi.Y, nChannels);
            currentRoi = ROIs.Count - 1;
        }

        public void addCurrentRoi(float polyLineRadius)
        {
            ROI roi = new ROI(Roi); //copy Roi
            roi.GetEqualDistanceCenters(polyLineRadius);

            roi.Roi3d = Roi.Roi3d;
            if (ZStack || nFastZ > 1)
                roi.Z = new int[] { currentPage };

            if (ROIs.Count == 0)
                roi.ID = 0;
            else
                roi.ID = ROIs.Select(x => x.ID).ToArray().Max() + 1;

            ROIs.Add(roi);
            //Roi = new ROI(Roi.ROI_type, Roi.X, Roi.Y, nChannels);
            currentRoi = ROIs.Count - 1;
        }

        public void removeCurrentRoi()
        {
            ROIs.RemoveAt(currentRoi);
        }


        public void intitializeAll()
        {
            FLIMRaw5D = MatrixCalc.makeNew5DSlice(nChannels, nFastZ, height, width, n_time);
            FLIMRaw = makeFLIMRawTypeArray(nChannels, height, width, n_time);
            FLIMMapCalculated = new bool[nChannels];
            ProjectCalculated = new bool[nChannels];
            LifetimeCalculated = false;
        }

        private ushort[][,,] makeFLIMRawTypeArray(int nCh, int y, int x, int[] t)
        {
            var result = new ushort[nCh][,,];
            for (int ch = 0; ch < nCh; ch++)
            {
                if (t[ch] != 0)
                    result[ch] = new ushort[y, x, t[ch]];
            }
            return result;
        }

        public DateTime decodeAcquiredTimeOnly(String Description)
        {
            DateTime acquiredTime = new DateTime();
            String[] headerstr = Description.Split('\r');
            FileIO fileIO = new FileIO(State);
            foreach (String s in headerstr)
            {
                fileIO.ExecuteLine(s);
                String[] pS = s.Split('=');

                if (pS.Length > 1)
                {
                    String strA = pS[1].Replace(";", "").Replace("\n", "").Replace(" ", "").Replace("\r", "");

                    if (s.Contains("Acquired_Time"))
                    {
                        acquiredTime = DateTime.ParseExact(strA, "yyyy-MM-ddTHH:mm:ss.fff", null);

                    }
                    else if (s.Contains("SaveChannels"))
                    {
                        String[] sP2 = strA.Replace("]", "").Replace("[", "").Split(',');
                        saveChannels = new bool[sP2.Length];
                        for (int i = 0; i < saveChannels.Length; i++)
                            saveChannels[i] = Convert.ToBoolean(sP2[i]);
                    }
                }
            }

            return acquiredTime;
        }


        /// <summary>
        /// This is for backward compatibility with Matlab version.
        /// </summary>
        /// <param name="Description"></param>
        public void decodeHeader(String Description, String fullFilename)
        {
            FileIO fileIO = new FileIO(State);
            ScanParameters allState = fileIO.CopyState();

            String[] headerstr = Description.Split('\r');

            image_description = Description;

            State.Acq.FastZ_nSlices = 1; //for compatibiltiy
            State.Acq.fastZScan = false; //for compatibility.//
            FLIM_on_eachChannel = false; //for compatibility//
            State.Files.channelsInSeparatedFile = false; // For compatibility//

            bool aveFrame_eachChannel = false;
            bool saveChannelbool = false;

            format = FileIO.FileFormat.None;

            if (headerstr[0].Contains("FLIMimage"))
            {
                foreach (String s in headerstr)
                {
                    try
                    {
                        fileIO.ExecuteLine(s, true);
                    }
                    catch (Exception EX)
                    {
                        Debug.WriteLine(s);
                        Debug.WriteLine("Problem in excecuting line: 443" + EX.ToString());
                    }
                    //if (s.Contains("n_dataPoint"))
                    //{
                    //    Debug.WriteLine(s);
                    //}

                    String[] pS = s.Split('=');
                    if (pS.Length > 1)
                    {
                        String strA = pS[1].Replace(";", "").Replace("\n", "").Replace(" ", "").Replace("\r", "");
                        String name = pS[0];

                        if (name.Contains("Acquired_Time"))
                        {
                            acquiredTime = DateTime.ParseExact(strA, "yyyy-MM-ddTHH:mm:ss.fff", null);
                        }

                        else if (name.Contains("SaveChannels"))
                        {

                            String[] sP2 = strA.Replace("]", "").Replace("[", "").Split(',');
                            saveChannels = new bool[sP2.Length];
                            for (int i = 0; i < saveChannels.Length; i++)
                                saveChannels[i] = Convert.ToBoolean(sP2[i]);

                            saveChannelbool = true;

                        }

                        else if (name.Contains("acqFLIMA"))
                            FLIM_on_eachChannel = true;

                        else if (name.Contains("aveFrameA"))
                            aveFrame_eachChannel = true;

                        else if (name.Contains("Format"))
                        {
                            String[] formatNames = Enum.GetNames(typeof(FileIO.FileFormat));
                            foreach (var formatname in formatNames)
                            {
                                if (strA == formatname)
                                    Enum.TryParse(formatname, out format);
                            }
                        }
                    }
                }


                width = State.Acq.pixelsPerLine;
                height = State.Acq.linesPerFrame;

                psPerUnit = State.Spc.spcData.resolution[0];
                //offset = State.Spc.analysis.offset;

                nSlices = State.Acq.nSlices;
                nFrames = State.Acq.nFrames;
                nChannels = State.Acq.nChannels;

                ZStack = (State.Acq.ZStack && State.Acq.nSlices > 1 && State.Acq.sliceStep > 0.0);
                nFastZ = State.Acq.fastZScan ? State.Acq.FastZ_nSlices : 1;

                if (!saveChannelbool)
                {
                    saveChannels = Enumerable.Repeat(true, nChannels).ToArray();
                }

                if (!aveFrame_eachChannel) //Existence of State.Acq.acqFLIMA
                {
                    State.Acq.aveFrameA = Enumerable.Repeat(State.Acq.aveFrame, nChannels).ToArray();
                    nAveragedFrame = Enumerable.Repeat(State.Acq.nAveFrame, nChannels).ToArray();
                }

                nAveFrame = State.Acq.nAveFrame;

                if (nAveragedFrame == null || nAveragedFrame.Length != nChannels)
                    nAveragedFrame = new int[nChannels];

                for (int c = 0; c < nChannels; c++)
                {
                    nAveragedFrame[c] = State.Acq.nAveFrame;
                    if (State.Acq.aveFrameA[c])
                    {
                        if (State.Acq.aveSlice)
                            nAveragedFrame[c] = nAveragedFrame[c] * State.Acq.nAveSlice;
                    }
                    else
                        nAveragedFrame[c] = 1;
                }


                n_time = Enumerable.Repeat<int>(State.Spc.spcData.n_dataPoint, nChannels).ToArray();

                if (!FLIM_on_eachChannel)
                    State.Acq.acqFLIMA = Enumerable.Repeat<bool>(State.Acq.acqFLIM, nChannels).ToArray();

                for (int i = 0; i < n_time.Length; i++)
                {
                    if (!State.Acq.acqFLIMA[i])
                        n_time[i] = 1;

                    if (!saveChannels[i])
                        n_time[i] = 0;
                }

                syncRate = new int[nChannels];
                countRate = new int[nChannels];

                for (int j = 0; j < nChannels; j++)
                {
                    if (j < State.Spc.datainfo.syncRate.Length)
                        syncRate[j] = State.Spc.datainfo.syncRate[j];
                    else
                        syncRate[j] = State.Spc.datainfo.syncRate[State.Spc.datainfo.syncRate.Length - 1];
                    countRate[j] = State.Spc.datainfo.countRate[j];
                }

                loadFittingParamFromState();

            }
            else //not FLIMage
            {
                bool FLIM_on1 = true;
                // This is for backward compatibility with Matlab version.

                double[] multiPage = null;
                int pageIndex = 0;
                DateTime acquiredTime_d = DateTime.Now;

                foreach (String s in headerstr)
                {
                    //Debug.WriteLine(s);
                    String[] pS = s.Split('=');
                    if (pS.Length > 1)
                    {
                        String s1 = pS[0].Replace(" ", "");
                        //String strA = pS[1].Replace(";", "").Replace("\n", "").Replace(" ", "").Replace("\r", "");
                        String strA = pS[1].Replace(";", "").Replace("'", "").Replace("\n", "").Replace("\r", "");
                        strA = strA.Trim();

                        if (s.Contains(".n_dataPoint") || s.Contains("datainfo.adc_re"))
                        {
                            n_time = new int[] { Convert.ToInt32(strA) };
                        }
                        else if (s.Contains("spcData.resolution") || s.Contains("psPerUnit"))
                        {
                            psPerUnit = Convert.ToDouble(strA);
                        }
                        else if (s.Contains(".linesPerFrame")) //Same
                        {
                            height = Convert.ToInt32(strA);
                        }
                        else if (s.Contains(".pixelsPerLine")) //Same
                        {
                            width = Convert.ToInt32(strA);
                        }
                        else if (s.Contains(".msPerLine")) //Same
                        {
                            msPerLine = Convert.ToDouble(strA);
                        }
                        else if (s.Contains(".syncRate") || s.Contains(".pulseRate"))
                        {
                            //syncRate[0] = Convert.ToInt32(strA);

                            strA = strA.Replace("[", "").Replace("]", "");
                            String[] StrB = strA.Split(',');
                            syncRate = new int[StrB.Length];
                            for (int j = 0; j < syncRate.Length; j++)
                                syncRate[j] = Convert.ToInt32(StrB[j]);
                            //countRate[1] = Convert.ToInt32(StrB[1]);

                        }
                        else if (s.Contains(".countRate"))
                        {
                            strA = strA.Replace("[", "").Replace("]", "");
                            String[] StrB = strA.Split(',');
                            countRate = new int[StrB.Length];
                            for (int j = 0; j < StrB.Length; j++)
                                countRate[j] = Convert.ToInt32(StrB[j]);
                        }
                        else if (s.Contains(".nAveFrame") || s.Contains(".numAvgFramesSaveGUI"))
                        {
                            nAveFrame = Convert.ToInt32(strA);
                        }
                        else if (s.Contains(".nFrames") || s.Contains(".numberOfFrames"))
                        {
                            nFrames = Convert.ToInt32(strA);
                        }
                        else if (s.Contains(".nSlices") || s.Contains(".numberOfZSlices"))
                        {
                            nSlices = Convert.ToInt32(strA);
                        }
                        else if (s.Contains(".nChannels") || s.Contains(".scan_rout_x"))
                        {
                            nChannels = Convert.ToInt32(strA);
                        }
                        else if (s.Contains(".acqFLIM"))
                        {
                            FLIM_on1 = Convert.ToBoolean(strA);
                        }
                        else if (s.Contains("triggerTimeString"))
                        {
                            acquiredTime = DateTime.ParseExact(strA, "M/d/yyyy H:mm:ss.fff", new System.Globalization.CultureInfo("en-US"));
                        }
                        else if (s.Contains("datainfo.triggerTime"))
                        {
                            var strB = strA.Split('.');
                            if (strB.Length == 1)
                                acquiredTime_d = DateTime.ParseExact(strA, "yyyy/MM/dd HH:mm:ss", new System.Globalization.CultureInfo("en-US"));
                            else if (strB[1].Length == 3)
                                acquiredTime_d = DateTime.ParseExact(strA, "yyyy/MM/dd HH:mm:ss.fff", new System.Globalization.CultureInfo("en-US"));
                            else if (strB[1].Length == 2)
                                acquiredTime_d = DateTime.ParseExact(strA, "yyyy/MM/dd HH:mm:ss.ff", new System.Globalization.CultureInfo("en-US"));
                            else if (strB[1].Length == 1)
                                acquiredTime_d = DateTime.ParseExact(strA, "yyyy/MM/dd HH:mm:ss.f", new System.Globalization.CultureInfo("en-US"));
                        }
                        else if (s.Contains("multiPages.page"))
                        {
                            pageIndex = Convert.ToInt32(strA);
                        }
                        else if (s.Contains("multiPages.timing"))
                        {
                            strA = strA.Replace("[", "").Replace("]", "");
                            String[] StrB = strA.Split(' ');
                            multiPage = new Double[StrB.Length];
                            for (int j = 0; j < StrB.Length; j++)
                                multiPage[j] = Convert.ToDouble(StrB[j]);
                        }
                    }

                }

                if (multiPage != null && pageIndex < multiPage.Length)
                {
                    acquiredTime = acquiredTime_d;
                }

                //DELETE??
                State.Acq.nChannels = nChannels;
                State.Acq.linesPerFrame = height;
                State.Acq.pixelsPerLine = width;
                State.Spc.spcData.n_dataPoint = n_time[0];

                FLIM_on = Enumerable.Repeat<bool>(FLIM_on1, nChannels).ToArray();

                if (FLIM_on1)
                    n_time = Enumerable.Repeat<int>(State.Spc.spcData.n_dataPoint, nChannels).ToArray();
                else
                    n_time = Enumerable.Repeat<int>(1, nChannels).ToArray();

                State.Acq.nFrames = nFrames;
                State.Acq.nSlices = nSlices;
                State.Acq.nAveFrame = nAveFrame;
                State.Spc.spcData.resolution[0] = psPerUnit;

                if (nSlices > 1)
                {
                    State.Acq.ZStack = true;
                    ZStack = true;
                    nFastZ = 0;
                }
                nAveragedFrame = Enumerable.Repeat<int>(State.Acq.nAveFrame, nChannels).ToArray();

                State.Files.fullNameTofileNames(fullFilename);

            } //Matlab compatible

            State.Display = allState.Display;
            State.Files.initFileName = allState.Files.initFileName;
            State.Files.initFolderPath = allState.Files.initFolderPath;

            intitializeAll();
        }


        public void fileUpdateRealtime(ScanParameters Scan, bool beforeFileCounterUpdate)
        {
            if (beforeFileCounterUpdate)
                fileCounter = Scan.Files.fileCounter;
            else
                fileCounter = Scan.Files.fileCounter - 1;

            State.Files.channelsInSeparatedFile = !State.Acq.aveFrameA.All(x => x == State.Acq.aveFrameA[0]) && State.Acq.nAveFrame > 1;

            baseName = Scan.Files.baseName;
            pathName = Scan.Files.pathName;
            fullFileName = Scan.Files.fullName();
            fileName = Scan.Files.fileName;

            State.Files.fileCounter = fileCounter;
            State.Files.baseName = baseName;
            State.Files.pathName = pathName;
            State.Files.fullName();
        }


        public String fullName(int channel, bool channelsInSeparatedFile)
        {
            if (numberedFile)
            {
                if (!channelsInSeparatedFile)
                    fileName = String.Format("{0}{1:000}", baseName, fileCounter);
                else
                    fileName = String.Format("{0}_Ch{1}_{2:000}", baseName, channel + 1, fileCounter);
                return String.Format("{0}{1}{2}{3}", pathName, Path.DirectorySeparatorChar, fileName, fileExtension);
            }
            else
            {
                if (!channelsInSeparatedFile)
                    fileName = String.Format("{0}", baseName);
                else
                    fileName = String.Format("{0}_Ch{1}", baseName, channel + 1);

                return (String.Format("{0}{2}fileName{1}", fileName, fileExtension, Path.DirectorySeparatorChar));
            }
        }

        public void create_SimulatedFLIM(double[] beta2)
        {
            FLIMRaw5D = new ushort[nChannels][][,,];
            double res0 = State.Spc.spcData.resolution[0]; //picoseconds
            double pulseI = 1.0e12 / State.Spc.datainfo.syncRate[0] / res0;

            for (int i = 0; i < nChannels; i++)
            {
                FLIMRaw5D[i] = new ushort[nFastZ][,,];
                ushort[,,] tempImage;
                for (int z = 0; z < nFastZ; z++)
                {
                    //var res = State.Spc.spcData.resolution[0] / 1000.0;
                    //double[] beta2 = { 4, 1 / (2.6 / res), 6, 1 / (0.5 / res), tau_g / res, offset / res };
                    tempImage = MathLibrary.ImageProcessing.CreateFLIM_Sim(n_time[0], height, width, beta2, pulseI, 2);
                    //MatrixCalc.CopyFrom3DToLinear(tempImage, out FLIMRaw5D[i][z]);
                    FLIMRaw5D[i][z] = (ushort[,,])tempImage.Clone();
                    FLIMRaw[i] = tempImage;
                }
            }

            //calculateAll();
            ZProjection = false;
            addCurrentToPage5D(0, false);
        }

        public void LoadFLIMData4D_Page_fromFLIMData5D(ushort[][][,,] FLIMData5D, int page, DateTime acquired_time, bool deepCopy)
        {
            var FLIM4D_Pages = ImageProcessing.PermuteFLIM5D(FLIMData5D, deepCopy);
            acquiredTime = acquired_time;
            if (page <= FLIM_Pages.Length)
                expandPage(page + 1);
            FLIM_Pages[page] = FLIM4D_Pages[0];
            acquiredTime_Pages[page] = acquiredTime;
        }

        public void LoadFLIMdata5D_Realtime(ushort[][][,,] FLIMData5D, DateTime acquired_time, bool deepCopy)
        {
            if (deepCopy)
                FLIMRaw5D = (ushort[][][,,])Copier.DeepCopyArray(FLIMData5D);
            else //Shallow copy
                FLIMRaw5D = ShallowCopyFLIM5D(FLIMData5D);

            acquiredTime = acquired_time;
        }

        private T[] ArrayResizePrivate<T>(T[] arr, int new_size)
        {
            Array.Resize(ref arr, new_size);
            return arr;
        }

        public void Delete5DFLIM(int page_position)
        {
            if (page_position < n_pages5D && page_position >= 0)
            {
                for (int i = page_position; i < n_pages5D - 1; i++)
                {
                    FLIM_Pages5D[i] = FLIM_Pages5D[i + 1];
                    acquiredTime_Pages5D[i] = acquiredTime_Pages5D[i + 1];
                }

                FLIM_Pages5D = ArrayResizePrivate(FLIM_Pages5D, n_pages5D - 1);
                acquiredTime_Pages5D = ArrayResizePrivate(acquiredTime_Pages5D, n_pages5D - 1);
                n_pages5D = FLIM_Pages5D.Length;

                if (page_position >= n_pages5D)
                    page_position = n_pages5D - 1;

                gotoPage5D(page_position);
            }
        }

        public void Delete4DFLIM(int page_position)
        {
            if (page_position < n_pages && page_position >= 0 && KeepPagesInMemory)
            {
                for (int i = page_position; i < n_pages - 1; i++)
                {
                    FLIM_Pages[i] = FLIM_Pages[i + 1];
                    acquiredTime_Pages[i + 1] = acquiredTime_Pages[i + 1];
                    Project_Pages[i] = Project_Pages[i + 1];
                    LifetimeMapBase_Pages[i] = LifetimeMapBase_Pages[i + 1];
                    ProjectCalculated_Pages[i] = ProjectCalculated_Pages[i + 1];
                    FLIMMapCalculated_Pages[i] = FLIMMapCalculated_Pages[i + 1];
                    LifetimeCalculated_Pages[i] = LifetimeCalculated_Pages[i + 1];
                }

                resizePage(n_pages - 1);
                if (page_position >= n_pages)
                    page_position = n_pages - 1;

                gotoPage(page_position);
            }
        }

        public void Add_AllFLIM_PageFormat_To_FLIM_Pages5D(ushort[][][] FLIM_i, DateTime acqTime, int page_position)
        {
            expandArray5D(page_position);
            FLIM_Pages5D[page_position] = ImageProcessing.FLIM_Pages2FLIMRaw5D(FLIM_i, new int[] { height, width }, n_time);

            n_pages5D = FLIM_Pages5D.Length;

            acquiredTime_Pages5D[page_position] = acqTime;
            currentPage5D = page_position;
            n_pages5D = FLIM_Pages5D.Length;
        }

        public void Add_AllFLIM_PageFormat_To_FLIM_Pages5D(ushort[][][,,] FLIM_i, DateTime acqTime, int page_position)
        {
            expandArray5D(page_position);
            FLIM_Pages5D[page_position] = ImageProcessing.FLIM_Pages2FLIMRaw5D(FLIM_i);

            n_pages5D = FLIM_Pages5D.Length;

            acquiredTime_Pages5D[page_position] = acqTime;
            currentPage5D = page_position;
            n_pages5D = FLIM_Pages5D.Length;
        }

        public static ushort[][][,,] ShallowCopyFLIM5D(ushort[][][,,] FLIM_i)
        {
            //return (ushort[][][,,])FLIM_i.Clone(); Very shallow.
            if (FLIM_i == null)
                return null;

            var FLIM_shallowCopy = new ushort[FLIM_i.Length][][,,];
            for (int i = 0; i < FLIM_i.Length; i++)
            {
                if (FLIM_i[i] != null)
                {
                    FLIM_shallowCopy[i] = (ushort[][,,])FLIM_i[i].Clone();
                }
                else
                    FLIM_shallowCopy[i] = null;
            }
            return FLIM_shallowCopy;
        }

        public static ushort[][,,] ShallowCopyFLIM4D(ushort[][,,] FLIM_i)
        {
            ushort[][,,] FLIM_shallowCopy = null;
            if (FLIM_i != null)
                FLIM_shallowCopy = (ushort[][,,])FLIM_i.Clone();
            return FLIM_shallowCopy;
        }

        public static T[][,] ShallowCopy3D<T>(T[][,] imageData)
        {
            T[][,] result = (T[][,])imageData.Clone();
            return result;
        }

        public void Add5DFLIM(ushort[][][,,] FLIM_i, DateTime acqTime, int page_position, bool deepCopy)
        {
            expandArray5D(page_position);
            if (deepCopy)
                FLIM_Pages5D[page_position] = (ushort[][][,,])Copier.DeepCopyArray(FLIM_i);
            else
                FLIM_Pages5D[page_position] = ShallowCopyFLIM5D(FLIM_i);

            n_pages5D = FLIM_Pages5D.Length;

            acquiredTime_Pages5D[page_position] = acqTime;
            currentPage5D = page_position;
            n_pages5D = FLIM_Pages5D.Length;
        }

        public void expandArray5D(int page_position)
        {
            if (FLIM_Pages5D.Length <= page_position)
                FLIM_Pages5D = ArrayResizePrivate(FLIM_Pages5D, page_position + 1);

            if (acquiredTime_Pages5D.Length <= page_position)
                acquiredTime_Pages5D = ArrayResizePrivate(acquiredTime_Pages5D, page_position + 1);
        }

        public void LoadFLIMRawFromData4D(ushort[][,,] FLIM_data, DateTime acquired_time, bool deepCopy)
        {
            if (deepCopy)
            {
                AssureFLIMRawSize();
                FLIMRaw = (ushort[][,,])Copier.DeepCopyArray(FLIM_data);
            }
            else
                FLIMRaw = ShallowCopyFLIM4D(FLIM_data);

            acquiredTime = acquired_time;
            FLIMMapCalculated = new bool[nChannels];
            ProjectCalculated = new bool[nChannels];
            LifetimeCalculated = false;
        }

        public void addToPageAndCalculate5D(ushort[][][,,] FLIM_5D, DateTime acqTime, bool calcProjection, bool calcLifetime1, int page_position, bool deepCopy)
        {
            if (KeepPagesInMemory)
            {
                Add5DFLIM(FLIM_5D, acqTime, page_position, deepCopy);
            }

            var FLIM5D = ImageProcessing.PermuteFLIM5D(FLIM_5D, true);
            var previous_n_pages = n_pages;

            expandPage(previous_n_pages + FLIM5D.Length);

            for (int z = 0; z < FLIM5D.Length; z++)
            {
                PutToPageAndCalculate(FLIM5D[z], acqTime, calcProjection, calcLifetime1, previous_n_pages + z);
            }
        }

        public void FLIM_Pages_FastZ_ToFLIMRaw5D()
        {
            if (nFastZ > 1)
                FLIM_Pages = ArrayResizePrivate(FLIM_Pages, nFastZ);
            //Array.Resize(ref FLIM_Pages, nFastZ);

            FLIMRaw5D = ImageProcessing.FLIM_Pages2FLIMRaw5D(FLIM_Pages);
        }


        public void resizePage5D(int new_pageN)
        {
            n_pages5D = new_pageN;
            FLIM_Pages5D = ArrayResizePrivate(FLIM_Pages5D, new_pageN);
            acquiredTime_Pages5D = ArrayResizePrivate(acquiredTime_Pages5D, new_pageN);
        }

        public void resizePage(int new_pageN)
        {
            n_pages = new_pageN;
            acquiredTime_Pages = ArrayResizePrivate(acquiredTime_Pages, new_pageN);
            FLIM_Pages = ArrayResizePrivate(FLIM_Pages, new_pageN);
            Project_Pages = ArrayResizePrivate(Project_Pages, new_pageN);
            LifetimeMapBase_Pages = ArrayResizePrivate(LifetimeMapBase_Pages, new_pageN);
            FLIMMapCalculated_Pages = ArrayResizePrivate(FLIMMapCalculated_Pages, new_pageN);
            ProjectCalculated_Pages = ArrayResizePrivate(ProjectCalculated_Pages, new_pageN);
            LifetimeCalculated_Pages = ArrayResizePrivate(LifetimeCalculated_Pages, new_pageN);

            InitializeAllROI_FlimParameters_Pages();
        }

        public void expandPage(int new_n_pages)
        {
            if (new_n_pages > Project_Pages.Length)
                Project_Pages = ArrayResizePrivate(Project_Pages, new_n_pages);

            if (new_n_pages > LifetimeMapBase_Pages.Length)
                LifetimeMapBase_Pages = ArrayResizePrivate(LifetimeMapBase_Pages, new_n_pages);

            if (new_n_pages > acquiredTime_Pages.Length)
                acquiredTime_Pages = ArrayResizePrivate(acquiredTime_Pages, new_n_pages);

            if (new_n_pages > FLIM_Pages.Length && (KeepPagesInMemory || nFastZ > 1 || ZStack))
            {
                FLIM_Pages = ArrayResizePrivate(FLIM_Pages, new_n_pages);
                n_pages = new_n_pages;
            }

            if (new_n_pages > ProjectCalculated_Pages.Length)
                ProjectCalculated_Pages = ArrayResizePrivate(ProjectCalculated_Pages, new_n_pages);

            if (new_n_pages > FLIMMapCalculated_Pages.Length)
                FLIMMapCalculated_Pages = ArrayResizePrivate(FLIMMapCalculated_Pages, new_n_pages);

            if (new_n_pages > LifetimeCalculated_Pages.Length)
                LifetimeCalculated_Pages = ArrayResizePrivate(LifetimeCalculated_Pages, new_n_pages);

        }

        public void CopyFromFLIM_PageToFLIMRaw(int page)
        {
            //FLIMRaw = (ushort[][,,]) Copier.DeepCopyArray(FLIM_Pages[page]);
            FLIMRaw = ShallowCopyFLIM4D(FLIM_Pages[page]);
            ProjectCalculated = new bool[nChannels];
            FLIMMapCalculated = new bool[nChannels];
            LifetimeCalculated = false;
        }


        public int[] dimensionYXT(int channel)
        {
            return new int[] { height, width, n_time[channel] };
        }

        public void AssureFLIMRawSize()
        {
            bool correct = false;
            if (FLIMRaw.Length == nChannels)
            {
                for (int c = 0; c < nChannels; c++)
                {
                    if (FLIMRaw[c] != null)
                    {
                        correct = FLIMRaw[c].GetLength(0) == height && FLIMRaw[c].GetLength(1) == width
                            && FLIMRaw[c].GetLength(2) == n_time[c];
                    }
                }
            }

            if (!correct)
            {
                FLIMRaw = makeFLIMRawTypeArray(nChannels, height, width, n_time);
                FLIMMapCalculated = new bool[nChannels];
                ProjectCalculated = new bool[nChannels];
                LifetimeCalculated = false;
            }
        }

        public void PutToPage_Linear(ushort[][] flim_page, DateTime acqTime, int page)
        {
            var flim4d = new ushort[flim_page.Length][,,];
            for (int ch = 0; ch < flim_page.Length; ch++)
            {
                if (flim_page[ch] == null)
                {
                    if (page < FLIM_Pages.Length && FLIM_Pages[page] != null)
                        flim4d[ch] = FLIM_Pages[page][ch];
                }
                else
                {
                    flim4d[ch] = (ushort[,,])MatrixCalc.Reshape(flim_page[ch], new int[] { height, width, n_time[ch] });
                }
            }
            PutToPage(flim4d, acqTime, page);
        }


        public void PutToPage(ushort[][,,] flim_page, DateTime acqTime, int page)
        {
            expandPage(page + 1);
            FLIM_Pages[page] = flim_page;
            acquiredTime_Pages[page] = acqTime;
            ProjectCalculated_Pages[page] = false;
            FLIMMapCalculated_Pages[page] = false;
            LifetimeCalculated_Pages[page] = false;
        }

        //public void CalculateZProjectionImageOnly(int channel)
        //{
        //    var ZProc1 = new ushort[Project_Pages.Length][,];
        //    for (int z = 0; z < Project_Pages.Length; z++)
        //        ZProc1[z] = Project_Pages[z][channel];

        //    Project[channel] = ImageProcessing.GetMaxZProjection(ZProc1);
        //}

        public void CalculateAllPages_Direct(bool calculateLifetimeMap)
        {
            for (int i = 0; i < n_pages; i++)
            {
                CalculatePage_Direct(i, calculateLifetimeMap);
            }
        }

        public void fitRangeChanged()
        {
            //All recalculated. 
            ProjectCalculated = new bool[nChannels];
            ProjectCalculated_Pages = new bool[n_pages];
            FLIMMapCalculated = new bool[nChannels];
            FLIMMapCalculated_Pages = new bool[n_pages];
            ResetLifetimeCalculation(true);
        }

        public void CalculatePage_Direct(int page, bool calculateLifetimeMap)
        {
            if (ProjectCalculated_Pages.Length > page && !ProjectCalculated_Pages[page] && FLIM_Pages[page] != null)
            {
                if (!ProjectCalculated_Pages[page])
                {
                    ushort[][,] prjct = new ushort[nChannels][,];
                    for (int ch = 0; ch < nChannels; ch++)
                        prjct[ch] = ImageProcessing.GetProjectFromFLIM(FLIM_Pages[page][ch], fit_range[ch]);

                    Project_Pages[page] = prjct;
                    ProjectCalculated_Pages[page] = true;
                }
            }

            if (LifetimeMapBase_Pages.Length > page && !FLIMMapCalculated_Pages[page] && FLIM_Pages[page] != null)
            {
                if (calculateLifetimeMap)
                {
                    var lifetime_map = new float[nChannels][,];
                    for (int ch = 0; ch < nChannels; ch++)
                        lifetime_map[ch] = ImageProcessing.GetLifetimeMapFromFLIM(FLIM_Pages[page][ch], fit_range[ch], (float)psPerUnit, 0);

                    LifetimeMapBase_Pages[page] = lifetime_map;
                    FLIMMapCalculated_Pages[page] = true;
                }
            }
        }

        public int[] ApplyFitRange(int[] fit_range1, int channel)
        {
            int[] save_fit = (int[])fit_range[channel].Clone();

            int[] fit_range2;
            if (fit_range1 != null)
                fit_range2 = (int[])fit_range1.Clone();
            else
            {
                if (fit_range[0] != null)
                    fit_range2 = (int[])fit_range[0].Clone();
                else
                    fit_range2 = new int[] { 0, n_time[channel] - 1 };
            }

            fit_range[channel] = new int[] { fit_range2.Min(), fit_range2.Max() };

            if (!fit_range[channel].SequenceEqual(save_fit))
            {
                FLIMMapCalculated_Pages = new bool[FLIMMapCalculated_Pages.Length];
                FLIMMapCalculated[channel] = false;
            }

            return (int[])fit_range[channel].Clone();
        }

        /// <summary>
        /// Put FLIM_i reference to FLIM_Pages, and then calculate project and lifetime.
        /// </summary>
        /// <param name="FLIM_i"></param>
        /// <param name="acqTime"></param>
        /// <param name="calcProjection"></param>
        /// <param name="calcLifetime1"></param>
        /// <param name="page"></param>
        public void PutToPageAndCalculate(ushort[][,,] FLIM_i, DateTime acqTime, bool calcProjection, bool calcLifetime1, int page)
        {
            expandPage(page + 1);

            FLIM_Pages[page] = FLIM_i;

            CalculatePage_Direct(page, true);

            acquiredTime_Pages[page] = acqTime;
            currentPage = page;
        }


        public void calculateProjectCh(int ch)
        {
            if (ch >= nChannels)
                return;

            if (FLIMRaw == null)
            {
                Project = new ushort[nChannels][,];
                ProjectF = new ushort[nChannels][,];
                ProjectCalculated = new bool[nChannels];
                LifetimeCalculated = false;
                return;
            }

            if (FLIMRaw[ch] == null)
            {
                Project[ch] = null;
                ProjectF[ch] = null;
                ProjectCalculated[ch] = false;
                return;
            }

            if (Project[ch] == null)
                ProjectCalculated[ch] = false;

            if (!ProjectCalculated[ch])
            {
                Project[ch] = ImageProcessing.GetProjectFromFLIM(FLIMRaw[ch], fit_range[ch]);
                ProjectCalculated[ch] = Project[ch] != null;
            }

            if (ProjectF == null)
                ProjectF = new ushort[nChannels][,];

            if (Project[ch] != null)
                ProjectF[ch] = Project[ch];
            else
                ProjectF[ch] = null;
        }

        public void calculateProject()
        {
            if (Project == null || Project.Length != nChannels)
            {
                Project = new UInt16[nChannels][,];
                ProjectF = new UInt16[nChannels][,];
                ProjectCalculated = new bool[nChannels];
                LifetimeCalculated = false;
            }

            for (int ch = 0; ch < nChannels; ch++)
            {
                calculateProjectCh(ch);
            }
        }

        public void fitData_ROI_Ch(ROI roi1, int ch, int mode, double[] beta1, int page)
        {
            //double[] beta1 = fittingList[ch]; //fitting from all ROIs.
            //int n = roi1.flim_parameters.LifetimeX[ch].Length;

            int p = 6;
            if (mode == 1)
                p = 4;
            //int p = beta1.Length;

            bool page_direct = PageDirect(page);


            bool[] fix = Enumerable.Repeat<bool>(true, p).ToArray();

            double[] x = roi1.flim_parameters.LifetimeX[ch]; //this is just 1,2,3,4...
            double[] y = roi1.flim_parameters.LifetimeY[ch];

            if (page_direct)
            {
                x = roi1.flim_parameters_Pages[page].LifetimeX[ch];
                y = roi1.flim_parameters_Pages[page].LifetimeY[ch];
            }

            if (y == null)
            {
                return;
            }

            int n = y.Length;

            double[] beta0;

            if (p == beta1.Length)
                beta0 = (double[])beta1.Clone();
            else
                beta0 = new double[p];

            double maxY = y.Max();

            if (mode == 1)
            {
                beta0[0] = maxY;
                fix[0] = false;
                fix[1] = false;
            }
            else
            {
                double ratio = maxY / (beta0[0] + beta0[2]);
                beta0[0] = beta0[0] * ratio;
                beta0[2] = beta0[2] * ratio;
                fix[0] = false;
                fix[2] = false;
            }

            Fitting.Nlinfit fit = new Fitting.Nlinfit(beta0, x, y);
            fit.fix = fix;

            double res0 = State.Spc.spcData.resolution[0]; //picoseconds
            double pulseI = 1.0e12 / State.Spc.datainfo.syncRate[0] / res0;


            if (mode == 1)
            {
                fit.modelFunc = ((betaA, xA) => ImageProcessing.ExpGaussArray(betaA, xA, pulseI));
            }
            else
            {
                fit.modelFunc = ((betaA, xA) => ImageProcessing.Exp2GaussArray(betaA, xA, pulseI));
            }

            fit.PoisonWeights(); //Apply poison waits.
            fit.Perform();

            double tau_mA;

            if (mode == 1)
            {
                tau_mA = 1 / fit.beta[1];
            }
            else
            {
                double[] b = fit.beta;
                tau_mA = (b[0] / b[1] / b[1] + b[2] / b[3] / b[3]) / (b[0] / b[1] + b[2] / b[3]);
            }

            double res = psPerUnit / 1000;

            if (page_direct)
            {
                roi1.flim_parameters_Pages[page].tau_m[ch] = tau_mA * res;
                roi1.flim_parameters_Pages[page].xi_square[ch] = fit.xi_square;
                roi1.flim_parameters_Pages[page].beta[ch] = (double[])fit.beta.Clone();
                roi1.flim_parameters_Pages[page].fitCurve[ch] = fit.fitCurve;
            }
            else
            {
                roi1.flim_parameters.tau_m[ch] = tau_mA * res;
                roi1.flim_parameters.xi_square[ch] = fit.xi_square;
                roi1.flim_parameters.beta[ch] = (double[])fit.beta.Clone();
                roi1.flim_parameters.fitCurve[ch] = fit.fitCurve;
            }
        }

        public void fitDataAllROIs(int mode, int page1, double[][] betaCh)
        {
            int nROIs = ROIs.Count;

            bool pageDirect = PageDirect(page1);

            if (pageDirect && ROIs.Any(x => x.flim_parameters_Pages[page1].LifetimeY == null))
            {
                ResetLifetimeCalculation(true);
                return;
            }
            else if (!pageDirect && ROIs.Any(x => x.flim_parameters.LifetimeY == null))
            {
                ResetLifetimeCalculation(false);
                return;
            }


            for (int i = 0; i < nROIs; i++)
            {
                if (pageDirect)
                {
                    ROIs[i].flim_parameters_Pages[page1].AssureSizeAllParameters();
                    if (ROIs[i].flim_parameters_Pages[page1].LifetimeX.Length != nChannels)
                        return;
                }
                else
                {
                    ROIs[i].flim_parameters.AssureSizeAllParameters();

                    if (ROIs[i].flim_parameters.LifetimeX.Length != nChannels)
                        return;
                }

                for (int ch = 0; ch < nChannels; ch++)
                {
                    fitData_ROI_Ch(ROIs[i], ch, mode, betaCh[ch], page1);

                    if (ROIs[i].ROI_type == ROI.ROItype.PolyLine)
                    {
                        for (int j = 0; j < ROIs[i].polyLineROIs.Count; j++)
                        {
                            if (ROIs[i].flim_parameters.LifetimeX[ch] == null)
                                return;

                            var microRoi = ROIs[i].polyLineROIs[j];
                            if (microRoi.flim_parameters.LifetimeX[ch] == null)
                                microRoi.flim_parameters.LifetimeX[ch] = (double[])ROIs[i].flim_parameters.LifetimeX[ch].Clone();

                            if (pageDirect && microRoi.flim_parameters_Pages[page1].LifetimeX[ch] == null)
                                microRoi.flim_parameters_Pages[page1].LifetimeX[ch] = (double[])ROIs[i].flim_parameters_Pages[page1].LifetimeX[ch].Clone();

                            fitData_ROI_Ch(microRoi, ch, mode, betaCh[ch], page1);
                        }
                    }
                } //Channels
            } //ROI
        }

        public bool PageDirect(int page)
        {
            bool threeD = ThreeDRoi && (nFastZ > 1 || ZStack) && n_pages > 1;
            bool page_direct = page >= 0 && page < n_pages && !ZStack && nFastZ < 2
                && RoiFit.flim_parameters_Pages != null && !threeD;

            //InitializeAllROI_FlimParameters_Pages();

            return page_direct;
        }

        public double[] fitData(int ch, int mode, double[] beta0, bool[] fix, int page)
        {
            if (RoiFit.flim_parameters.LifetimeX == null || ch >= RoiFit.flim_parameters.LifetimeX.Length)
                return null;

            if (RoiFit.flim_parameters.LifetimeX[ch] == null)
                return null;

            double[] beta_i = (double[])beta0.Clone();


            int p = 6;// beta0.Length;

            double[] x = RoiFit.flim_parameters.LifetimeX[ch];
            double[] y = RoiFit.flim_parameters.LifetimeY[ch];

            bool page_direct = PageDirect(page);
            if (page_direct)
            {
                x = RoiFit.flim_parameters_Pages[page].LifetimeX[ch];
                y = RoiFit.flim_parameters_Pages[page].LifetimeY[ch];
            }

            int n = x.Length;
            double maxY = y.Max();
            int indx = y.ToList().IndexOf(maxY);
            double maxX = x[indx];
            double sum = y.Sum();
            double sumX = 0;

            for (int i = 0; i < n; i++)
            {
                sumX = sumX + y[i] * x[i];
            }

            double Tau1 = sum / maxY;
            double resP = (double)psPerUnit;
            double res = psPerUnit / 1000;
            double TauG = 100 / resP;

            double[] beta1 = new double[p];
            if (mode == 1)
            {
                beta1[0] = maxY * (1 + TauG / Tau1);
                beta1[1] = 1 / Tau1;
                beta1[2] = TauG;
                beta1[3] = maxX - 2 * TauG; //peak position
            }
            else
            {
                beta1[0] = maxY / 2;
                beta1[1] = 1 / Tau1 / 2;
                beta1[2] = maxY / 2;
                beta1[3] = 1 / Tau1 / 0.5;
                beta1[4] = TauG;
                beta1[5] = maxX - 1 * TauG; //peak position
            }

            for (int i = 0; i < p; i++)
            {
                if (beta0.Length > i && beta1.Length > i && !fix[i])
                    beta0[i] = beta1[i];
            }

            double tauG_Max = 500; //ps
            double tauG_Min = 60; //ps
            double maxTau = 10000; //ps
            double minTau = 100; //picoseconds

            double pulseI = 1.0e12 / State.Spc.datainfo.syncRate[0] / resP;

            Fitting.Nlinfit fit = new Fitting.Nlinfit(beta0, x, y);
            fit.fix = fix;
            if (mode == 1)
            {
                fit.modelFunc = ((betaA, xA) => ImageProcessing.ExpGaussArray(betaA, xA, pulseI));
                fit.betaMax[1] = resP / minTau;
                fit.betaMin[1] = 1 / maxTau;
                fit.betaMax[2] = tauG_Max / resP; //picosecond
                fit.betaMin[2] = tauG_Min / resP;
                //fit.betaMax[3] = 10000 / resP;
            }
            else
            {
                fit.modelFunc = ((betaA, xA) => ImageProcessing.Exp2GaussArray(betaA, xA, pulseI));
                //fit.betaMin[0] = 0;
                fit.betaMax[1] = resP / minTau;
                fit.betaMin[1] = 1 / maxTau;
                //fit.betaMin[2] = 0;
                fit.betaMax[3] = resP / minTau;
                fit.betaMin[3] = 1 / maxTau;
                fit.betaMax[4] = tauG_Max / resP;
                fit.betaMin[4] = tauG_Min / resP;
                fit.betaMax[5] = 10000 / resP;
            }

            fit.PoisonWeights();
            try
            {
                fit.Perform();
            }
            catch
            {
                Debug.WriteLine("Fitting did not work!");
            }

            double tau_m0;
            if (mode == 1)
            {
                tau_m0 = 1 / fit.beta[1];
            }
            else
            {

                double[] b = fit.beta;

                tau_m0 = (b[0] / b[1] / b[1] + b[2] / b[3] / b[3]) / (b[0] / b[1] + b[2] / b[3]);
            }


            if (fit.fitCurve != null)
            {
                if (page_direct)
                {
                    RoiFit.flim_parameters_Pages[page].fitCurve[ch] = (double[])fit.fitCurve.Clone();
                    RoiFit.flim_parameters_Pages[page].residual[ch] = (double[])fit.residual.Clone();
                    RoiFit.flim_parameters_Pages[page].offset_fit[ch] = res * (sumX / sum - tau_m0);

                    RoiFit.flim_parameters_Pages[page].beta[ch] = (double[])fit.beta.Clone();
                    RoiFit.flim_parameters_Pages[page].xi_square[ch] = fit.xi_square;
                    RoiFit.flim_parameters_Pages[page].tau_m[ch] = tau_m0 * res;
                }
                else
                {
                    RoiFit.flim_parameters.fitCurve[ch] = (double[])fit.fitCurve.Clone();
                    RoiFit.flim_parameters.residual[ch] = (double[])fit.residual.Clone();
                    RoiFit.flim_parameters.offset_fit[ch] = res * (sumX / sum - tau_m0);

                    RoiFit.flim_parameters.beta[ch] = (double[])fit.beta.Clone();
                    RoiFit.flim_parameters.xi_square[ch] = fit.xi_square;
                    RoiFit.flim_parameters.tau_m[ch] = tau_m0 * res;
                }

                return (double[])fit.beta.Clone();
            }
            return null;
        }



        public void saveFittingParameters(double[][] fitting_param)
        {
            for (int ch = 0; ch < nChannels; ch++)
            {
                double[] fit_param1 = (double[])fitting_param[ch].Clone();
                object obj = State.Spc.analysis;

                obj.GetType().GetField("fit_param" + (ch + 1)).SetValue(obj, fit_param1);

                double res = psPerUnit / 1000;
                double[] beta = (double[])fit_param1.Clone();
                beta[1] = res / fit_param1[1];
                beta[3] = res / fit_param1[3];
                beta[4] = fit_param1[4] / res;
                beta[5] = fit_param1[5] / res;

                RoiFit.flim_parameters.beta0[ch] = beta;
            }
        }

        public bool IsRoiInFocus(ROI roi)
        {
            if (!ZStack && nFastZ <= 1)
                return true;
            else
            {
                if (roi.Roi3d && roi.Z.Any(x => x == currentPage))
                    return true;
                else
                    return false;
            }
        }

        private bool IfInsideAllROIs(Point P)
        {
            bool b = false;
            int n = ROIs.Count;
            for (int i = 0; i < n; i++)
            {
                if (ROIs[i].IsInsideRoi(P))
                {
                    b = true;
                    break;
                }
            }

            return b;
        }



        /// <summary>
        /// Calculate Z projection.
        /// </summary>
        /// <param name="procType"></param>
        /// <param name="page_range"></param>
        public void calcZProject(projectionType procType, int[] page_range)
        {
            ZProjection_Range = (int[])page_range.Clone();

            z_projection_type = procType;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            int n = n_pages; // FLIM_Pages.Count;
            if (n == 0)
                return;

            int startPage = page_range[0];
            int endPage = page_range[1];
            if (startPage < 0 || startPage >= n)
            {
                startPage = 0;
            }
            if (endPage <= startPage || endPage < 1 || endPage > n)
            {
                endPage = n;
            }

            gotoPage(startPage);

            FLIMRawZProjection = (ushort[][,,])Copier.DeepCopyArray(FLIM_Pages[startPage]);

            if (procType == projectionType.Sum)
            {
                for (int page = startPage; page < endPage; page++)
                {
                    for (int ch = 0; ch < nChannels; ch++)
                    {
                        if (FLIM_Pages[page][ch] != null)
                        {
                            if (page != startPage)
                                MatrixCalc.ArrayCalc(FLIMRawZProjection[ch], FLIM_Pages[page][ch], CalculationType.Add);
                        }
                        else
                        {
                            FLIMRawZProjection[ch] = null;
                            break;
                        }
                    }
                }
            }
            else if (procType == projectionType.Max || procType == projectionType.Min)
            {
                UInt16[,] ProjectAMax = new ushort[height, width];

                CalculateAllPages_Direct(false);

                if (Project_Pages == null)
                    return;

                for (int ch = 0; ch < nChannels; ch++)
                {
                    for (int page = startPage; page < endPage; page++)
                    {
                        if (Project_Pages[page] != null && Project_Pages[page][ch] != null)
                        {
                            UInt16[,] ProjectA = Project_Pages[page][ch];
                            if (page == startPage)
                            {
                                ProjectAMax = (ushort[,])Utilities.Copier.DeepCopyArray(ProjectA);
                            }
                            else
                            {
                                if (procType == projectionType.Max)
                                {
                                    for (int y = 0; y < height; y++)
                                        for (int x = 0; x < width; x++)
                                            if (ProjectAMax[y, x] < ProjectA[y, x])
                                            {
                                                Array.Copy(FLIM_Pages[page][ch], (y * width + x) * n_time[ch], FLIMRawZProjection[ch], (y * width + x) * n_time[ch], n_time[ch]);
                                                ProjectAMax[y, x] = ProjectA[y, x];
                                            }
                                }
                                else //Minimum.
                                {
                                    for (int y = 0; y < width; y++)
                                        for (int x = 0; x < width; x++)
                                            if (ProjectAMax[y, x] > ProjectA[y, x])
                                            {
                                                Array.Copy(FLIM_Pages[page][ch], (y * width + x) * n_time[ch], FLIMRawZProjection[ch], (y * width + x) * n_time[ch], n_time[ch]);
                                                ProjectAMax[y, x] = ProjectA[y, x];
                                            }
                                }
                            }
                        }
                    } //page
                } //channel
            } //max or min

            //currentPage = page_range[0];

            Debug.WriteLine("elapsed time (calculaton) - page" + n + " = " + sw.ElapsedMilliseconds);
            ZProjectionCalculated = true;

            //FLIMRaw = (ushort[][,,])Copier.DeepCopyArray(FLIMRawZProjection);
            FLIMRaw = ShallowCopyFLIM4D(FLIMRawZProjection);
            ProjectCalculated = new bool[nChannels];
            FLIMMapCalculated = new bool[nChannels];
            LifetimeCalculated = false;
            ZProjection = true;

            calculateAll();
        }

        public void filterMAP(int fw, int ch)
        {
            if (FLIM_on[ch])
            {
                if (LifetimeMap == null)
                    LifetimeMap = new float[nChannels][,];
                LifetimeMap[ch] = MatrixCalc.SubtractConstantFromMatrix(LifetimeMapBase[ch], (float)offset[ch]);
                if (LifetimeMap[ch] == null)
                    return;
            }

            if (Project[ch] == null)
            {
                ProjectF[ch] = null;
                return;
            }

            if (fw <= 1)
            {
                if (FLIM_on[ch])
                {
                    if (LifetimeMapF == null)
                        LifetimeMapF = new float[nChannels][,];
                    LifetimeMapF[ch] = (float[,])Utilities.Copier.DeepCopyArray(LifetimeMap[ch]);
                }

                if (ProjectF == null)
                    ProjectF = new ushort[nChannels][,];
                ProjectF[ch] = (ushort[,])Utilities.Copier.DeepCopyArray(Project[ch]);
            }

            else
            {
                State.Display.filterWindow_FLIM = fw;

                var temp = new float[height, width];
                var tempI = new ushort[height, width]; //new UInt16[height, width];

                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        float sum = 0;
                        float sum1 = 0;
                        float pix = 0.0f;
                        for (int h = 0; h < fw; h++)
                            for (int w = 0; w < fw; w++)
                            {
                                int posX = (int)(x + w - fw / 2);
                                int posY = (int)(y + h - fw / 2);
                                if (posX >= 0 && posX < width && posY >= 0 && posY < height)
                                {
                                    if (FLIM_on[ch])
                                        sum = sum + LifetimeMap[ch][posY, posX];
                                    sum1 = sum1 + Project[ch][posY, posX];
                                    pix = pix + 1.0f;
                                }

                            }
                        if (pix > 0)
                        {
                            if (FLIM_on[ch])
                                temp[y, x] = sum / pix;
                            tempI[y, x] = (UInt16)(sum1 / pix);
                        }
                    }

                if (FLIM_on[ch])
                    LifetimeMapF[ch] = temp; //c];
                ProjectF[ch] = tempI;
            }

        }



        /// <summary>
        /// Calculate meanLifetime
        /// </summary>
        /// <param name="roi"></param>
        /// <param name="ch"></param>
        /// <param name="threeD"></param>
        /// <param name="page1">-1 for current page</param>
        /// <param name="recalc_all"></param>
        public void calculate_MeanLifetime_General(ROI roi, int ch, bool threeD, int page1, bool recalc_all, double[] offsetC)
        {
            int start_page = 0;
            double sum = 0;
            double pix = 0;
            double sumi = 0;
            double pixi = 0;
            double val;
            int intensity;
            int nRoi = ROIs.Count;

            int nPages = n_pages;
            bool page_direct = PageDirect(page1);

            if (page_direct && (roi.flim_parameters_Pages == null || roi.flim_parameters_Pages.Length != n_pages))
                InitializeROI_FlimParameters_Pages(roi);

            if (!threeD || page_direct)
            {
                nPages = 1;
                if (page_direct)
                {
                    start_page = page1;
                    page_direct = true;
                }
                threeD = false;
            }

            for (int page = start_page; page < start_page + nPages; page++)
            {
                if (!threeD || roi.Z.Any(z => z == page))
                {
                    ushort[,] img;
                    float[,] lf_img;


                    if (threeD || page_direct)
                    {
                        if (recalc_all || Project_Pages == null
                            || Project_Pages[page] == null
                            || Project_Pages[page][ch] == null)
                            CalculatePage_Direct(page, true);

                        img = Project_Pages[page][ch];
                        lf_img = LifetimeMapBase_Pages[page][ch];
                    }
                    else
                    {
                        if (recalc_all)
                            calculateLifetimeMap(new double[2]); //Slightly faster.

                        img = Project[ch];
                        if (LifetimeMapBase != null)
                            lf_img = LifetimeMapBase[ch];
                        else
                            lf_img = null;
                    }

                    //Note that if lf_img is null, it will calculate just intensity.
                    if (img == null)
                        return;

                    int height1 = img.GetLength(0);
                    int width1 = img.GetLength(1);

                    //Check if they have the same size. If different size, just calculate intensity.
                    if (lf_img == null || height1 != lf_img.GetLength(0) || width1 != lf_img.GetLength(1))
                        lf_img = null;

                    for (int y = (int)roi.Rect.Top; y < roi.Rect.Bottom; y++)
                        for (int x = (int)roi.Rect.Left; x < roi.Rect.Right; x++)
                            if (x < width1 && y < height1 && x >= 0 && y >= 0)
                            {
                                intensity = img[y, x];

                                Point P = new Point(x, y);

                                if (lf_img != null)
                                    val = lf_img[y, x];
                                else
                                    val = 0;

                                if (roi.IsInsideRoi(P))
                                {
                                    sumi = sumi + intensity;
                                    pixi = pixi + 1;

                                    if (intensity > low_threshold[ch])
                                    {
                                        sum = sum + val;
                                        pix = pix + 1;
                                    }
                                }
                            }
                } //ThreeDCond
            } //pges

            if (page_direct && roi.flim_parameters_Pages != null && roi.flim_parameters_Pages.Length > page1)
            {
                roi.flim_parameters_Pages[page1].meanIntensity[ch] = sumi / pixi;
                roi.flim_parameters_Pages[page1].sumIntensity[ch] = sumi;
                roi.flim_parameters_Pages[page1].nPixels[ch] = pixi;
                if (pix > 0)
                    roi.flim_parameters_Pages[page1].tau_m_fromMAP[ch] = sum / pix - offsetC[ch];
                else
                    roi.flim_parameters_Pages[page1].tau_m_fromMAP[ch] = 0;
            }
            else
            {
                roi.flim_parameters.meanIntensity[ch] = sumi / pixi;
                roi.flim_parameters.sumIntensity[ch] = sumi;
                roi.flim_parameters.nPixels[ch] = pixi;
                if (pix > 0)
                    roi.flim_parameters.tau_m_fromMAP[ch] = sum / pix - offsetC[ch];
                else
                    roi.flim_parameters.tau_m_fromMAP[ch] = 0;
            }
        }


        public void InitializeAllROI_FlimParameters_Pages()
        {
            InitializeROI_FlimParameters_Pages(Roi);
            InitializeROI_FlimParameters_Pages(RoiFit);
            for (int i = 0; i < ROIs.Count; i++)
                InitializeROI_FlimParameters_Pages(ROIs[i]);
        }

        public void InitializeROI_FlimParameters_Pages(ROI roi)
        {
            int nPage = n_pages;
            if (nFastZ > 1)
                nPage = n_pages5D;
            else if (ZStack)
                nPage = 1;

            if (roi.flim_parameters_Pages == null)
                roi.flim_parameters_Pages = new ROI_FLIM_Parameters[n_pages];
            else if (roi.flim_parameters_Pages.Length != n_pages)
                Array.Resize(ref roi.flim_parameters_Pages, n_pages);

            for (int i = 0; i < n_pages; i++)
            {
                if (roi.flim_parameters_Pages[i] == null)
                {
                    roi.flim_parameters_Pages[i] = new ROI_FLIM_Parameters();
                    roi.flim_parameters_Pages[i].initializeParams(nChannels, n_time);
                }
                else if (roi.flim_parameters_Pages[i].nChannels != nChannels)
                {
                    roi.flim_parameters_Pages[i].ChangeChannelNumber(nChannels);
                }
            }
        }

        /// <summary>
        /// Calculate mean lifetime
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="page">-1 for current page</param>
        public void calculate_MeanLifetime_ch(int ch, int page1, double[] offsetC)
        {
            bool page_direct = PageDirect(page1);
            bool threeD = ThreeDRoi && (nFastZ > 1 || ZStack) && n_pages > 1 && !page_direct;

            calculate_MeanLifetime_General(Roi, ch, threeD, page1, true, offsetC);

            if (Roi.ROI_type == ROI.ROItype.PolyLine)
            {
                foreach (var roi1 in Roi.polyLineROIs)
                    calculate_MeanLifetime_General(roi1, ch, threeD, page1, false, offsetC);
            }

            for (int i = 0; i < ROIs.Count; i++)
            {
                calculate_MeanLifetime_General(ROIs[i], ch, threeD, page1, false, offsetC);
                if (ROIs[i].ROI_type == ROI.ROItype.PolyLine)
                {
                    foreach (var roi1 in ROIs[i].polyLineROIs)
                        calculate_MeanLifetime_General(roi1, ch, threeD, page1, false, offsetC);
                }
            }

            if (bgRoi.Rect.Width > 0 && bgRoi.Rect.Height > 0)
                calculate_MeanLifetime_General(bgRoi, ch, threeD, page1, false, offsetC);
            else
            {
                if (bgRoi.flim_parameters.meanIntensity.Length != nChannels)
                {
                    bgRoi.flim_parameters.meanIntensity = new double[nChannels];
                    bgRoi.flim_parameters.sumIntensity = new double[nChannels];
                    bgRoi.flim_parameters.nPixels = new double[nChannels];
                }

                bgRoi.flim_parameters.meanIntensity[ch] = 0;
                bgRoi.flim_parameters.sumIntensity[ch] = 0;
                bgRoi.flim_parameters.nPixels[ch] = 1;

            }
        }

        public void calculate_MeanLifetime(int page, double[] offsetC)
        {
            for (int ch = 0; ch < nChannels; ch++)
                calculate_MeanLifetime_ch(ch, page, offsetC);
        }


        public void calculateLifetimeChRoi(int ch, ROI roi, int[] range, bool threeD, int page1)
        {
            int simd_ulong = Vector<ulong>.Count;
            int simd_ushort = Vector<ushort>.Count;
            bool simd_accel = Vector.IsHardwareAccelerated;

            int n_point = range[1] - range[0];
            int fitStart = range[0];

            bool page_direct = PageDirect(page1);
            if (page_direct && (roi.flim_parameters_Pages == null || roi.flim_parameters_Pages.Length != n_pages))
                InitializeROI_FlimParameters_Pages(roi);

            var lifetimeY = new double[n_point];
            List<Vector<ulong>> vlist = new List<Vector<ulong>>();
            int t1;
            for (t1 = 0; t1 <= n_point - simd_ushort; t1 += simd_ushort)
            {
                vlist.Add(Vector<ulong>.Zero);
                vlist.Add(Vector<ulong>.Zero);
                vlist.Add(Vector<ulong>.Zero);
                vlist.Add(Vector<ulong>.Zero);
            }

            int iblock = vlist.Count;
            int rest = t1;

            float xstart = 0;
            float xend = width;
            float ystart = 0;
            float yend = height;

            if (roi != null)
            {
                xstart = Math.Max(0, roi.Rect.Left);
                xend = Math.Min(roi.Rect.Right, width);
                ystart = Math.Max(0, roi.Rect.Top);
                yend = Math.Min(roi.Rect.Bottom, height);

            }
            else
                return;

            int nPages = 1;
            int savePage = currentPage;

            int start_page = 0;
            if (page_direct)
            {
                start_page = page1;
                threeD = false;
            }

            ushort[,] Img = Project[ch];
            ushort[,,] lifetimeImg = FLIMRaw[ch];

            if (threeD)
            {
                nPages = n_pages;
                lifetimeImg = (ushort[,,])FLIMRaw[ch].Clone();
            }

            double[] lifeimte = new double[n_time[ch]];

            for (int page = start_page; page < start_page + nPages; page++)
            {
                if (!threeD || roi.Z.Any(z => z == page))
                {
                    if (threeD || page_direct)
                    {
                        CalculatePage_Direct(page, true);
                        Img = Project_Pages[page][ch];
                        lifetimeImg = FLIM_Pages[page][ch];
                    }

                    if (Img == null || lifetimeImg == null)
                        return;

                    ushort[] FLIMRawT = new ushort[n_time[ch]];
                    for (int y = (int)ystart; y < yend; ++y)
                        for (int x = (int)xstart; x < xend; ++x)
                        {
                            int val, intensity;
                            intensity = Img[y, x];
                            MatrixCalc.extract3rdAxis(lifetimeImg, ref FLIMRawT, y, x);
                            Point P = new Point(x, y);

                            if (intensity >= low_threshold[ch])
                            {
                                if (roi.ROI_type == ROI.ROItype.Rectangle || roi.IsInsideRoi(P)) //if rectangle, it is always in.
                                {
                                    if (simd_accel)
                                    {
                                        for (int k = 0; k < iblock; k += 4)
                                        {
                                            var vF = new Vector<ushort>(FLIMRawT, k * simd_ulong + fitStart);
                                            Vector.Widen(vF, out Vector<uint> vF1, out Vector<uint> vF2);
                                            Vector.Widen(vF1, out Vector<ulong> vF11, out Vector<ulong> vF12);
                                            Vector.Widen(vF2, out Vector<ulong> vF21, out Vector<ulong> vF22);

                                            vlist[k] += vF11;
                                            vlist[k + 1] += vF12;
                                            vlist[k + 2] += vF21;
                                            vlist[k + 3] += vF22;
                                        }

                                        for (int t = rest; t < n_point; t++) //Rest
                                        {
                                            val = FLIMRawT[t + fitStart];
                                            lifetimeY[t] += val;
                                        }
                                    }
                                    else
                                    {
                                        for (int t = 0; t < n_point; ++t)
                                        {
                                            val = FLIMRawT[t + fitStart];
                                            lifetimeY[t] += val;
                                        }
                                    }
                                } //roi condition.
                            } //threshold
                        } // xy loop
                } //threeD condition.
            } //page loop

            if (simd_accel)
                for (int k = 0; k < iblock; k++)
                    Vector.ConvertToDouble(vlist[k]).CopyTo(lifetimeY, k * simd_ulong);


            if (page_direct)
                roi.flim_parameters_Pages[page1].LifetimeY[ch] = (double[])lifetimeY.Clone();
            else
            {
                if (roi.flim_parameters.LifetimeY.Length < ch + 1)
                    Array.Resize(ref roi.flim_parameters.LifetimeY, ch + 1);
                roi.flim_parameters.LifetimeY[ch] = (double[])lifetimeY.Clone();
            }
        } //roi                    

        /// <summary>
        /// Return success.
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="page1"></param>
        /// <param name="page_direct"></param>
        /// <returns></returns>
        public bool calculateLifetimeCh(int ch, int page1, out bool page_direct)
        {
            page_direct = PageDirect(page1);
            if (FLIMRaw == null)
                return false;

            if (FLIMRaw[ch] == null)
                return false;

            if (n_time[ch] <= 2 || !FLIM_on[ch])
                return false;


            int nRoi = ROIs.Count;

            int n_point = fit_range[ch][1] - fit_range[ch][0];
            int fitStart = fit_range[ch][0];

            if (n_point < 1)
                n_point = 1;

            if (fitStart < 0)
                fitStart = 0;

            if (n_point > FLIMRaw[ch].GetLength(2) - fitStart)
                n_point = FLIMRaw[ch].GetLength(2) - fitStart;

            if (n_point > 1)
            {
                fit_range[ch][0] = fitStart;
                fit_range[ch][1] = fitStart + n_point;
            }
            else
                return false;

            bool threeD = ThreeDRoi && (nFastZ > 1 || ZStack) && n_pages > 1;

            double[] lifetimeX1 = new double[n_point];
            double[] lifetimeY1 = new double[n_point];
            //double[,] sum1 = new double[nRoi,n_point];



            bool calc_something = ((page_direct && !LifetimeCalculated_Pages[page1])
                        || (!page_direct && !LifetimeCalculated));


            for (int t = 0; t < n_point; t++)
                lifetimeX1[t] = (double)(fitStart + t);

            if (page_direct && RoiFit.flim_parameters_Pages == null)
            {
                InitializeROI_FlimParameters_Pages(RoiFit);
                ResetLifetimeCalculation(true);
                calc_something = true;
            }

            if (Fit_type.Equals(FitType.GlobalRois))
            {

                if (calc_something)
                {
                    for (int i = 0; i < nRoi; i++)
                    {
                        calculateLifetimeChRoi(ch, ROIs[i], fit_range[ch], threeD, page1);

                        if (page_direct)
                        {
                            ROIs[i].flim_parameters_Pages[page1].LifetimeX[ch] = (double[])lifetimeX1.Clone();
                        }
                        else
                        {
                            ROIs[i].flim_parameters.LifetimeX[ch] = (double[])lifetimeX1.Clone();
                        }

                        if (calc_something)
                            if (ROIs[i].ROI_type == ROI.ROItype.PolyLine)
                            {
                                for (int j = 0; j < ROIs[i].polyLineROIs.Count; j++)
                                {
                                    calculateLifetimeChRoi(ch, ROIs[i].polyLineROIs[j], fit_range[ch], threeD, page1);
                                }
                            }
                    } //Roi loop

                    for (int i = 0; i < nRoi; i++)
                    {
                        if (page_direct && !LifetimeCalculated_Pages[page1])
                            MatrixCalc.ArrayCalc(lifetimeY1, ROIs[i].flim_parameters_Pages[page1].LifetimeY[ch], CalculationType.Add);
                        else
                            MatrixCalc.ArrayCalc(lifetimeY1, ROIs[i].flim_parameters.LifetimeY[ch], CalculationType.Add);
                    }

                    if (page_direct)
                    {
                        RoiFit.flim_parameters_Pages[page1].LifetimeX[ch] = (double[])lifetimeX1.Clone();
                        RoiFit.flim_parameters_Pages[page1].LifetimeY[ch] = (double[])lifetimeY1.Clone();
                        //LifetimeCalculated_Pages[page1] = true; Channel is not calculated y et.
                    }
                    else
                    {
                        RoiFit.flim_parameters.LifetimeX[ch] = (double[])lifetimeX1.Clone();
                        RoiFit.flim_parameters.LifetimeY[ch] = (double[])lifetimeY1.Clone();
                        //LifetimeCalculated = true;
                    }
                }
            }
            else if (Fit_type.Equals(FitType.WholeImage))
            {
                if (RoiFit.Rect.Width != width || RoiFit.Rect.Height != height)
                {
                    RoiFit = new ROI(ROI.ROItype.Rectangle, new Rectangle(0, 0, width, height), nChannels, -100, false, new int[] { 0, 0 });
                    ResetLifetimeCalculation(true);
                    calc_something = true;
                }

                if (calc_something)
                {
                    if (page_direct)
                    {
                        calculateLifetimeChRoi(ch, RoiFit, fit_range[ch], false, page1);
                        RoiFit.flim_parameters_Pages[page1].LifetimeX[ch] = (double[])lifetimeX1.Clone();
                        lifetimeY1 = RoiFit.flim_parameters_Pages[page1].LifetimeY[ch];
                    }
                    else
                    {
                        calculateLifetimeChRoi(ch, RoiFit, fit_range[ch], false, page1);
                        RoiFit.flim_parameters.LifetimeX[ch] = (double[])lifetimeX1.Clone();
                        lifetimeY1 = RoiFit.flim_parameters.LifetimeY[ch];
                    }
                }
            }
            else //Selected.
            {
                if (Roi.Rect.Width == 0 || Roi.Rect.Height == 0)
                {
                    Roi = new ROI(ROI.ROItype.Rectangle, new Rectangle(0, 0, width, height), nChannels, -1, false, new int[] { 0, 0 });
                    ResetLifetimeCalculation(true);
                    calc_something = true;
                }

                if (calc_something)
                {
                    calculateLifetimeChRoi(ch, Roi, fit_range[ch], threeD, page1);
                    if (page_direct)
                    {
                        if (Roi.flim_parameters_Pages[page1].LifetimeX.Length < ch + 1)
                            Array.Resize(ref Roi.flim_parameters_Pages[page1].LifetimeX, ch + 1);
                        Roi.flim_parameters_Pages[page1].LifetimeX[ch] = (double[])lifetimeX1.Clone();

                        if (Roi.flim_parameters_Pages[page1].LifetimeY.Length < ch + 1)
                            Array.Resize(ref Roi.flim_parameters_Pages[page1].LifetimeY, ch + 1);
                        lifetimeY1 = Roi.flim_parameters_Pages[page1].LifetimeY[ch];
                    }
                    else
                    {
                        if (Roi.flim_parameters.LifetimeX.Length < ch + 1)
                            Array.Resize(ref Roi.flim_parameters.LifetimeX, ch + 1);
                        Roi.flim_parameters.LifetimeX[ch] = (double[])lifetimeX1.Clone();
                        if (Roi.flim_parameters.LifetimeY.Length < ch + 1)
                            Array.Resize(ref Roi.flim_parameters.LifetimeY, ch + 1);
                        lifetimeY1 = Roi.flim_parameters.LifetimeY[ch];
                    }

                    if (Roi.ROI_type == ROI.ROItype.PolyLine)
                    {
                        for (int j = 0; j < Roi.polyLineROIs.Count; j++)
                        {
                            if (page_direct)
                                Roi.polyLineROIs[j].flim_parameters_Pages[page1].LifetimeX[ch] = (double[])lifetimeX1.Clone();
                            else
                                Roi.polyLineROIs[j].flim_parameters.LifetimeX[ch] = (double[])lifetimeX1.Clone();

                            calculateLifetimeChRoi(ch, Roi.polyLineROIs[j], fit_range[ch], threeD, page1);
                        }
                    }

                    if (page_direct)
                    {
                        if (RoiFit.flim_parameters_Pages == null)
                        {
                            RoiFit.initialize_flimParameter_Pages(nChannels, n_time, n_pages);
                        }
                        RoiFit.flim_parameters_Pages[page1].LifetimeX[ch] = (double[])lifetimeX1.Clone();
                        RoiFit.flim_parameters_Pages[page1].LifetimeY[ch] = (double[])lifetimeY1.Clone();
                    }
                    else
                    {
                        RoiFit.flim_parameters.LifetimeX[ch] = (double[])lifetimeX1.Clone();
                        RoiFit.flim_parameters.LifetimeY[ch] = (double[])lifetimeY1.Clone();
                    }
                } //caclulte something.
            }//Type.

            return true;

        } // end function

        public void ResetLifetimeCalculation(bool all_pages)
        {
            if (all_pages || LifetimeCalculated_Pages == null)
                LifetimeCalculated_Pages = new bool[n_pages];
            else
            {
                if (LifetimeCalculated_Pages.Length != n_pages)
                {
                    var arr = LifetimeCalculated_Pages;
                    Array.Resize(ref arr, n_pages);
                    LifetimeCalculated_Pages = arr;
                }
            }

            if (currentPage >= 0 && currentPage < LifetimeCalculated_Pages.Length)
                LifetimeCalculated_Pages[currentPage] = false;
            LifetimeCalculated = false;

            ResetFitting(all_pages);
        }


        /// <summary>
        /// Calculate Lifetime
        /// </summary>
        public void calculateLifetime(int page1)
        {
            if (!FLIM_on.Any(x => x == true))
                return;

            int nRoi = ROIs.Count;
            bool page_direct = PageDirect(page1);


            if (Fit_type.Equals(FitType.GlobalRois))
            {
                for (int i = 0; i < nRoi; i++)
                {
                    var n_point = new int[nChannels];
                    for (int ch = 0; ch < nChannels; ch++)
                        n_point[ch] = fit_range[ch][1] - fit_range[ch][0];
                    ROIs[i].flim_parameters.initializeParams(nChannels, n_point); //Initialize
                    if (ROIs[i].ROI_type == ROI.ROItype.PolyLine)
                    {
                        for (int j = 0; j < ROIs[i].polyLineROIs.Count; j++)
                            ROIs[i].polyLineROIs[j].flim_parameters.initializeParams(nChannels, n_point);
                    }
                }

                if (page_direct)
                {

                    foreach (ROI roi in ROIs)
                    {
                        if (roi.flim_parameters_Pages == null ||
                            roi.flim_parameters_Pages[page1].LifetimeY == null ||
                                roi.flim_parameters_Pages[page1].LifetimeY.Any(x => x == null))
                        {
                            ResetLifetimeCalculation(true);
                            break;
                        }
                    }
                }
                else if (!page_direct)
                {
                    foreach (ROI roi in ROIs)
                        if (roi.flim_parameters == null ||
                            roi.flim_parameters.LifetimeY == null ||
                            roi.flim_parameters.LifetimeY.Any(x => x == null))
                        {
                            ResetLifetimeCalculation(false);
                            break;
                        }
                }
            }

            bool[] success = new bool[nChannels];
            for (int ch = 0; ch < nChannels; ch++)
            {
                success[ch] = calculateLifetimeCh(ch, page1, out page_direct);
            } //ch

            if (success.All(x => x == true))
                if (page_direct)
                {
                    LifetimeCalculated_Pages[page1] = true;
                }
                else
                    LifetimeCalculated = true;


        }

        public void ResetFitting(bool resetAllPages)
        {
            Roi.flim_parameters.AssureSizeAllParameters();
            Roi.initialize_flimParameter_Pages(nChannels, n_time, n_pages);
            RoiFit.flim_parameters.AssureSizeAllParameters();
            RoiFit.initialize_flimParameter_Pages(nChannels, n_time, n_pages);

            //ResetLifetimeCalculation(resetAllPages);
        }

        public void calculateLifetimeMapCh(int ch, double offset1)
        {
            if (ch >= nChannels)
                return;

            if (FLIMRaw == null)
            {
                LifetimeMapBase = new float[nChannels][,];
                LifetimeMap = new float[nChannels][,];
                LifetimeMapF = new float[nChannels][,];

                FLIMMapCalculated = new bool[nChannels];
                return;
            }

            if (FLIMRaw[ch] == null)
            {
                FLIMMapCalculated[ch] = false;
                LifetimeMapBase[ch] = null;
                LifetimeMap[ch] = null;
                LifetimeMapF = null;
            }

            if (LifetimeMapBase == null || LifetimeMapBase.Length != nChannels)
            {
                LifetimeMapBase = new float[nChannels][,];
                FLIMMapCalculated = new bool[nChannels];
            }

            if (!FLIMMapCalculated[ch] || LifetimeMapBase[ch] == null)
            {
                LifetimeMapBase[ch] = ImageProcessing.GetLifetimeMapFromFLIM(FLIMRaw[ch], fit_range[ch], (float)psPerUnit, 0);
                FLIMMapCalculated[ch] = (LifetimeMapBase[ch] != null);
            }

            calculateLifetimeMapCh_Fast(ch, offset1);
        }

        public void calculateLifetimeMapCh_Fast(int ch, double offset1)
        {
            if (LifetimeMap == null || LifetimeMap.Length != nChannels)
            {
                LifetimeMap = new float[nChannels][,];
                LifetimeMapF = new float[nChannels][,];
            }

            if (LifetimeMapBase != null && LifetimeMapBase[ch] != null)
            {
                if (offset1 == 0)
                    LifetimeMap[ch] = (float[,])LifetimeMapBase[ch].Clone();
                else
                    LifetimeMap[ch] = MatrixCalc.SubtractConstantFromMatrix(LifetimeMapBase[ch], (float)offset1);

                if (LifetimeMapF == null)
                    LifetimeMapF = new float[nChannels][,];

                LifetimeMapF[ch] = LifetimeMap[ch];
            }
        }

        public void calculateLifetimeMap(double[] offsetCh)
        {
            if (!FLIM_on.Any(x => x == true))
                return;

            if (LifetimeMap == null || LifetimeMap.Length != nChannels)
            {
                LifetimeMap = new float[nChannels][,];
                LifetimeMapF = new float[nChannels][,];
                FLIMMapCalculated = new bool[nChannels];
            }

            for (int i = 0; i < nChannels; i++)
            {
                calculateLifetimeMapCh(i, offsetCh[i]);
            }
        }


        public void calculateAll()
        {
            calculateProject();
            calculateLifetimeMap(offset);
            calculateLifetime(-1);
        }

        //PAGES////////////////////
        public void RemovePageRange5D(int initialPage, int NPages)
        {
            if (initialPage < 0)
                initialPage = 0;

            if (initialPage > FLIM_Pages5D.Length - 1)
                initialPage = FLIM_Pages5D.Length - 1;

            int End_Page = initialPage + NPages;

            if (End_Page > FLIM_Pages5D.Length)
                End_Page = FLIM_Pages5D.Length;

            int new_pageN = FLIM_Pages5D.Length - NPages;

            Array.Copy(FLIM_Pages5D, initialPage, FLIM_Pages5D, 0, new_pageN);
            Array.Copy(acquiredTime_Pages5D, initialPage, acquiredTime_Pages5D, 0, new_pageN);

            resizePage5D(new_pageN);

            //Array.Resize(ref FLIM_Pages5D, new_pageN);
            //Array.Resize(ref acquiredTime_Pages5D, new_pageN);

            //ushort[][][][] temp_pages = (ushort[][][][])FLIM_Pages5D.Clone();
            //DateTime[] temp_acquiredTime_Pages = (DateTime[])acquiredTime_Pages.Clone();

            //FLIM_Pages5D = new ushort[new_pageN][][][];
            //acquiredTime_Pages = new DateTime[new_pageN];

            //for (int i = 0; i < NPages; i++)
            //{
            //    FLIM_Pages5D[i] = temp_pages[i + initialPage];
            //    acquiredTime_Pages[i] = temp_acquiredTime_Pages[i + initialPage];
            //}

            RemovePageRange(initialPage * nFastZ, NPages * nFastZ);
        }

        public void RemovePageRange(int initialPage, int NPages)
        {
            if (initialPage < 0)
                initialPage = 0;

            if (initialPage > FLIM_Pages.Length - 1)
                initialPage = FLIM_Pages.Length - 1;

            int End_Page = initialPage + NPages;

            if (End_Page > FLIM_Pages.Length)
                End_Page = FLIM_Pages.Length;

            NPages = End_Page - initialPage;
            if (NPages > Project_Pages.Length - initialPage)
                NPages = Project_Pages.Length - initialPage;

            int new_pageN = FLIM_Pages5D.Length - NPages;

            Array.Copy(FLIM_Pages, initialPage, FLIM_Pages, 0, new_pageN);
            Array.Copy(Project_Pages, initialPage, Project_Pages, 0, new_pageN);
            Array.Copy(LifetimeMapBase_Pages, initialPage, LifetimeMapBase_Pages, 0, new_pageN);
            Array.Copy(FLIMMapCalculated_Pages, initialPage, FLIMMapCalculated_Pages, 0, new_pageN);
            Array.Copy(ProjectCalculated_Pages, initialPage, ProjectCalculated_Pages, 0, new_pageN);
            Array.Copy(LifetimeCalculated_Pages, initialPage, LifetimeCalculated_Pages, 0, new_pageN);
            Array.Copy(acquiredTime_Pages, initialPage, acquiredTime_Pages, 0, new_pageN);

            resizePage(new_pageN);

            n_pages = FLIM_Pages.Length;
            currentPage = -1;
        }


        public void clearMemory()
        {
            clearPages5D();
            clearPages4D();
        }

        public void clearPages5D()
        {
            //MatrixCalc.MatrixNull6D(FLIM_Pages5D);
            FLIM_Pages5D = new ushort[1][][][,,];
            acquiredTime_Pages5D = new DateTime[1];
            n_pages5D = 0;
        }

        public void clearPages4D()
        {
            //MatrixCalc.MatrixNull5D(FLIM_Pages);
            //MatrixCalc.MatrixNull4D(Project_Pages);
            //MatrixCalc.MatrixNull4D(LifetimeMapBase_Pages);

            FLIM_Pages = new ushort[0][][,,];
            //
            Project_Pages = new ushort[0][][,];
            LifetimeMapBase_Pages = new float[0][][,];
            acquiredTime_Pages = new DateTime[0];
            FLIMMapCalculated_Pages = new bool[0];
            LifetimeCalculated_Pages = new bool[0];

            ProjectCalculated_Pages = new bool[0];
            ProjectCalculated = new bool[nChannels];
            FLIMMapCalculated = new bool[nChannels];

            LifetimeCalculated = false;
            currentPage = -1;
            n_pages = 0;

            ZProjectionCalculated = false;
            ZProjection = false;
        }

        public void addCurrentFLIMRawToPage4D(bool addMAP, int page, bool deepCopy)
        {
            if (page >= Project_Pages.Length)
                expandPage(page + 1);

            if (deepCopy)
                FLIM_Pages[page] = (ushort[][,,])Copier.DeepCopyArray(FLIMRaw);
            else
                FLIM_Pages[page] = ShallowCopyFLIM4D(FLIMRaw);

            ushort[][,] newImage;
            float[][,] newFLIM;

            if (addMAP)
            {
                calculateAll();
                newImage = (ushort[][,])Utilities.Copier.DeepCopyArray(Project);
                ProjectCalculated_Pages[page] = true;

                if (FLIM_on.Any(x => x == true))
                {
                    newFLIM = (float[][,])Utilities.Copier.DeepCopyArray(LifetimeMapBase);
                    FLIMMapCalculated_Pages[page] = true;
                }
                else
                {
                    newFLIM = null;
                    FLIMMapCalculated_Pages[page] = false;
                }

            }
            else
            {
                newImage = null;
                newFLIM = null;
                ProjectCalculated_Pages[page] = false;
                FLIMMapCalculated_Pages[page] = false;
            }

            LifetimeCalculated_Pages[page] = false;

            currentPage = page;

            Project_Pages[page] = newImage;

            if (FLIM_on.Any(x => x == true))
            {
                LifetimeMapBase_Pages[page] = newFLIM;
            }

            acquiredTime_Pages[page] = acquiredTime;

            n_pages = FLIM_Pages.Length;
        }

        public void MakeFLIM_Pages4DFromFLIMRaw5D(bool deepCopy)
        {
            FLIM_Pages = ImageProcessing.PermuteFLIM5D(FLIMRaw5D, deepCopy);
            n_pages = FLIM_Pages.Length;
            Project_Pages = new ushort[n_pages][][,];
            LifetimeMapBase_Pages = new float[n_pages][][,];
            ProjectCalculated_Pages = new bool[n_pages];
            FLIMMapCalculated_Pages = new bool[n_pages];
            LifetimeCalculated_Pages = new bool[n_pages];
            LifetimeCalculated = false;
            ZProjectionCalculated = false;
            acquiredTime_Pages = Enumerable.Repeat(acquiredTime, n_pages).ToArray();
        }

        /// <summary>
        /// FLIMRaw5D --> FLIM_Page5D[page]
        /// </summary>
        /// <param name="AddMAP"></param>
        /// <param name="AddToFLIM_Pages5D"></param>
        /// <param name="page"></param>
        /// <param name="deepCopy"></param>
        public void addCurrentToPage5D(int page, bool deepCopy)
        {
            if (page + 1 > FLIM_Pages5D.Length)
                FLIM_Pages5D = ArrayResizePrivate(FLIM_Pages5D, page + 1);

            if (page + 1 > acquiredTime_Pages5D.Length)
                acquiredTime_Pages5D = ArrayResizePrivate(acquiredTime_Pages5D, page + 1);

            if (deepCopy)
                FLIM_Pages5D[page] = (ushort[][][,,])Utilities.Copier.DeepCopyArray(FLIMRaw5D);
            else
                FLIM_Pages5D[page] = ShallowCopyFLIM5D(FLIM_Pages5D[page]);

            acquiredTime_Pages5D[page] = acquiredTime;

            MakeFLIM_Pages4DFromFLIMRaw5D(true);

            n_pages5D = FLIM_Pages5D.Length;
        }

        public void gotoPage5D(int page)
        {
            if (FLIM_Pages5D.Length == 0 && KeepPagesInMemory)
                return;

            if (n_pages5D <= page)
                page = n_pages5D - 1;
            if (page < 0)
                page = 0;

            FileIO.FileError error = 0;

            int saveCurrentPage = currentPage;

            if (KeepPagesInMemory)
            {
                if (FLIM_Pages5D != null)
                {
                    FLIMRaw5D = FLIM_Pages5D[page];


                    acquiredTime = acquiredTime_Pages5D[page];
                    if (FLIM_Pages.Length != nFastZ)
                        FLIM_Pages = ArrayResizePrivate(FLIM_Pages, nFastZ);

                    MakeFLIM_Pages4DFromFLIMRaw5D(true);
                }
            }
            else
            {
                int pageStart = page * nFastZ;
                for (int i = 0; i < nFastZ; i++)
                {
                    error = FileIO.OpenFLIMTiffFilePage(fullFileName, (short)(pageStart + i), i, this, i == 0, KeepPagesInMemory);
                }
            }

            if (saveCurrentPage < n_pages)
                gotoPage(saveCurrentPage);
            else
                gotoPage(0);

            currentPage5D = page;
        }


        public void gotoPage(int page)
        {
            if (n_pages <= page)
                page = n_pages - 1;
            if (page < 0)
                page = 0;

            FileIO.FileError error = 0;

            if (KeepPagesInMemory || nFastZ > 1 || State.Acq.ZStack)
            {
                CopyFromFLIM_PageToFLIMRaw(page);
            }
            else
            {
                if (page == currentPage && !ZProjection)
                    return;


                error = FileIO.OpenFLIMTiffFilePage(fullFileName, (short)page, page, this, false, KeepPagesInMemory);

                if (error != FileIO.FileError.Success) //No file??
                {
                    if (FLIM_Pages.Length > page) //Attempt to open if it exist in the page.
                        CopyFromFLIM_PageToFLIMRaw(page);
                    else
                        MessageBox.Show("Could not open this image");
                }

            }

            ProjectCalculated = new bool[nChannels];
            FLIMMapCalculated = new bool[nChannels];
            LifetimeCalculated = false;

            if (ProjectCalculated_Pages[page] && Project_Pages[page] != null)
            {
                //Project = (ushort[][,])Copier.DeepCopyArray(Project_Pages[page]);
                Project = ShallowCopy3D(Project_Pages[page]);
                ProjectCalculated = Enumerable.Repeat(true, nChannels).ToArray();
            }
            else
            {
                calculateProject();
                Project_Pages[page] = (ushort[][,])Utilities.Copier.DeepCopyArray(Project);
                ProjectCalculated_Pages[page] = true;
            }

            if (FLIMMapCalculated_Pages[page] && LifetimeMapBase_Pages[page] != null)
            {
                LifetimeMapBase = ShallowCopy3D(LifetimeMapBase_Pages[page]);
                //LifetimeMapBase = (float[][,])Copier.DeepCopyArray(LifetimeMapBase_Pages[page]);
                FLIMMapCalculated = Enumerable.Repeat(true, nChannels).ToArray();
            }
            else
            {
                if (FLIM_on.Any(x => x == true))
                {
                    calculateLifetimeMap(offset);
                    LifetimeMapBase_Pages[page] = (float[][,])Utilities.Copier.DeepCopyArray(LifetimeMapBase);
                    FLIMMapCalculated_Pages[page] = true;
                }
            }

            CopyLifetimeDecayFromPageToCurrent(page);

            if (acquiredTime_Pages != null && acquiredTime_Pages.Length > page)
                acquiredTime = acquiredTime_Pages[page];

            currentPage = page;
            ZProjection = false;
        }

        public void CopyLifetimeDecayFromPageToCurrent(int page)
        {
            if (LifetimeCalculated_Pages != null
                && PageDirect(page)
                && LifetimeCalculated_Pages[page]
                && RoiFit.flim_parameters_Pages[page] != null)
            {
                RoiFit.flim_parameters = RoiFit.flim_parameters_Pages[page].Copy();
                if (RoiFit.flim_parameters.LifetimeY != null && RoiFit.flim_parameters.LifetimeY[0] != null
                    && RoiFit.flim_parameters.LifetimeY[0].Length == RoiFit.flim_parameters.LifetimeX[0].Length)
                    LifetimeCalculated = true;
            }
        }


        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Projection type is enumerated.
        /// </summary>
        public enum projectionType
        {
            Sum = 1,
            Max = 2,
            Min = 3,
        }

        public enum FileType
        {
            ZStack = 1,
            TimeCourse = 2,
            TimeCourse_ZStack = 3,
        }

        public enum FitType
        {
            WholeImage = 1,
            SelectedRoi = 2,
            GlobalRois = 3,
        } //Class FLIMData
    }
}
