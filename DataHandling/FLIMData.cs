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
using FLIMage.Analysis;

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

        public UInt16[][][] FLIMRaw5D;
        public UInt16[][,,] FLIMRaw;
        public UInt16[][,,] FLIMRawZProjection;
        public UInt16[][][] Project;
        public UInt16[][][] ProjectF;

        //public int[][] Lifetime;

        public List<double[]> Lifetime_X;
        public List<double[]> Lifetime_Y;


        public float[][][] LifetimeMapBase;
        public float[][][] LifetimeMap;
        public float[][][] LifetimeMapF;

        public UInt16[][][][] FLIM_Pages5D;
        public UInt16[][][] FLIM_Pages;
        public UInt16[][][][] Project_Pages;
        public float[][][][] LifetimeMapBase_Pages;
        public bool[] ProjectCalculated;
        public bool[] FLIMMapCalculated;

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
        public DateTime acquiredTime;
        public DateTime[] acquiredTime_Pages;
        public DateTime[] acquiredTime_Pages5D;
        //
        public int currentPage = -1;
        public int currentPage5D = -1;
        //public int currentFastZ = -1;

        public double[] low_threshold;

        public double[] beta;
        public List<double[]> fittingList;
        public List<double[]> Residuals;
        public List<double[]> fitCurve;

        public double[] xi_square;
        public double[] tau_m;
        public double[] offset_fit;

        public int[] syncRate;
        public int[] countRate;

        //public Rectangle roi;
        public ROI Roi;
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
            FLIM_DataConstruct(Scan);
            if (recoverRoi)
            {
                Roi = flim1.Roi;
                ROIs = flim1.ROIs;
                currentRoi = flim1.currentRoi;
                Fit_type = flim1.Fit_type;
            }
        }

        public FLIMData(ScanParameters Scan)
        {
            FLIM_DataConstruct(Scan);
        }

        public void InitializeData(ScanParameters Scan)
        {
            clearMemory();
            FLIM_DataConstruct(Scan);
        }

        public void FLIM_DataConstruct(ScanParameters Scan)
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

            FLIMRaw5D = new UInt16[nChannels][][];

            FLIMRaw = new UInt16[nChannels][,,];
            //Project = new UInt16[nChannels][][];
            //ProjectF = new UInt16[nChannels][][];

            fit_range = new int[nChannels][];

            //LifetimeMapBase = new float[nChannels][][];
            //LifetimeMap = new float[nChannels][][];
            //LifetimeMapF = new float[nChannels][][];

            low_threshold = new double[nChannels];

            Lifetime_X = new List<double[]>();
            Lifetime_Y = new List<double[]>();
            Residuals = new List<double[]>();
            fitCurve = new List<double[]>();
            fittingList = new List<double[]>();

            tau_m = new double[N_MAXCHANNEL];
            xi_square = new double[N_MAXCHANNEL];
            offset_fit = new double[N_MAXCHANNEL];

            acquiredTime = DateTime.Now;

            FLIMRaw5D = MatrixCalc.makeNew5DSlice(nChannels, nFastZ, height, width, n_time);

            for (int i = 0; i < nChannels; i++)
            {
                FLIMRaw[i] = new ushort[height, width, n_time[i]];
                //Project[i] = MatrixCalc.MatrixCreate2D<UInt16>(height, width);
                //ProjectF[i] = MatrixCalc.MatrixCreate2D<UInt16>(height, width);
                //LifetimeMapBase[i] = MatrixCalc.MatrixCreate2D<float>(height, width);
                //LifetimeMap[i] = MatrixCalc.MatrixCreate2D<float>(height, width);
                //LifetimeMapF[i] = MatrixCalc.MatrixCreate2D<float>(height, width);

                double[] X1 = new double[n_time[i]];
                Lifetime_X.Add(X1);
                Lifetime_Y.Add(X1);
            }

            clearMemory();

            if (State.Acq.fastZScan && State.Acq.FastZ_nSlices > 1)
            {
                nFastZ = State.Acq.FastZ_nSlices;
                FLIM_Pages = new ushort[nFastZ][][];
                for (int z = 0; z < nFastZ; z++)
                    FLIM_Pages[z] = new ushort[nChannels][];
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

            ROIs = new List<ROI>();
            ResetRoi();

            loadFittingParamFromState();

            //intitializeAll();
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
            fittingList.Clear();
            Residuals.Clear();
            fitCurve.Clear();
            fit_range = new int[nChannels][];
            for (int ch = 0; ch < nChannels; ch++)
            {
                double[] intensity_range = (double[])State.Display.GetType().GetField("Intensity_Range" + (ch + 1)).GetValue(State.Display);
                low_threshold[ch] = intensity_range[0];
                int[] fit_range1 = (int[])State.Spc.analysis.GetType().GetField("fit_range" + (ch + 1)).GetValue(State.Spc.analysis);
                double[] beta1 = (double[])State.Spc.analysis.GetType().GetField("fit_param" + (ch + 1)).GetValue(State.Spc.analysis);


                fit_range[ch] = (int[])fit_range1.Clone();
                beta = (double[])beta1.Clone();

                double res = psPerUnit / 1000.0;
                beta[1] = res / beta[1];
                beta[3] = res / beta[3];
                beta[4] = beta[4] / res;
                beta[5] = beta[5] / res;

                fittingList.Add(beta);
                Residuals.Add(X1); //put some rondam values.
                fitCurve.Add(X1);
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
                    Roi = new ROI(roi1.ROI_type, roi1.X, roi1.Y, nChannels, 0, roi1.Roi3d, roi1.Z);
                    success = true;
                }
            }
            return success;
        }

        public void ResetRoi()
        {
            Rectangle rect = new Rectangle();
            rect.Location = new Point(0, 0);
            rect.Width = width;
            rect.Height = height;

            Roi = new ROI(ROI.ROItype.Rectangle, rect, nChannels, 0, false, new int[] { 0 });
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
                roi.ID = 1;
            else
                roi.ID = ROIs[ROIs.Count - 1].ID + 1;

            ROIs.Add(roi);
            //Roi = new ROI(Roi.ROI_type, Roi.X, Roi.Y, nChannels);
            currentRoi = ROIs.Count - 1;
        }

        public void addCurrentRoi()
        {
            ROI roi = new ROI(Roi); //copy Roi
            roi.Roi3d = Roi.Roi3d;
            if (ZStack || nFastZ > 1)
                roi.Z = new int[] { currentPage };

            if (ROIs.Count == 0)
                roi.ID = 1;
            else
                roi.ID = ROIs[ROIs.Count - 1].ID + 1;

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
                }
            }

            return acquiredTime;
        }


        /// <summary>
        /// This is for backward compatibility with Matlab version.
        /// </summary>
        /// <param name="Description"></param>
        public void decodeHeader(String Description)
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
            else
            {
                bool FLIM_on1 = true;
                /// This is for backward compatibility with Matlab version.
                foreach (String s in headerstr)
                {
                    //Debug.WriteLine(s);
                    String[] pS = s.Split('=');
                    if (pS.Length > 1)
                    {
                        String s1 = pS[0].Replace(" ", "");
                        String strA = pS[1].Replace(";", "").Replace("\n", "").Replace(" ", "").Replace("\r", "");
                        if (s.Contains(".n_dataPoint") || s.Contains("datainfo.adc_re"))
                        {
                            n_time = new int[] { Convert.ToInt32(strA)
        };
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
                            for (int j = 0; j < countRate.Length; j++)
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
                        else if (s.Contains("Acquired_Time"))
                        {
                            acquiredTime = DateTime.ParseExact(strA, "yyyy-MM-ddTHH:mm:ss.fff", null);
                        }
                        //else if (s.Contains("fit_range"))
                        //{
                        //    int ch = Convert.ToInt32(s1.Substring(s1.Length - 1)) - 1;
                        //    strA = strA.Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "");
                        //    String[] StrB = strA.Split(',');
                        //    fit_range[ch] = new int[2];
                        //    for (int i = 0; i < 2; i++)
                        //        fit_range[ch][i] = Convert.ToInt32(StrB[i]);
                        //}
                        //else if (s.Contains("analysis.offset"))
                        //{
                        //    strA = strA.Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", ""); ;
                        //    String[] StrB = strA.Split(',');
                        //    offset = new double[2];
                        //    offset[0] = Convert.ToDouble(StrB[0]);
                        //    offset[1] = Convert.ToDouble(StrB[1]);
                        //}
                    }
                }

                //DELETE??
                State.Acq.nChannels = nChannels;
                State.Acq.linesPerFrame = height;
                State.Acq.pixelsPerLine = width;
                State.Spc.spcData.n_dataPoint = n_time[0];

                FLIM_on = Enumerable.Repeat<bool>(FLIM_on1, nChannels).ToArray();

                if (!FLIM_on1)
                    n_time = Enumerable.Repeat<int>(State.Spc.spcData.n_dataPoint, nChannels).ToArray();
                else
                    n_time = Enumerable.Repeat<int>(1, nChannels).ToArray();
                State.Acq.nFrames = nFrames;
                State.Acq.nSlices = nSlices;
                State.Acq.nAveFrame = nAveFrame;
                State.Spc.spcData.resolution[0] = psPerUnit;

                nAveragedFrame = Enumerable.Repeat<int>(State.Acq.nAveFrame, nChannels).ToArray();


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

        public void create_SimulatedFLIM()
        {
            FLIMRaw5D = new ushort[nChannels][][];

            for (int i = 0; i < nChannels; i++)
            {
                FLIMRaw5D[i] = new ushort[nFastZ][];
                ushort[,,] tempImage;
                for (int z = 0; z < nFastZ; z++)
                {
                    tempImage = CreateFLIM_Sim(n_time[0], height, width);
                    MatrixCalc.CopyFrom3DToLinear(tempImage, out FLIMRaw5D[i][z]);
                    FLIMRaw[i] = tempImage;
                }
            }

            calculateAll();
            ZProjection = false;
            addCurrentToPage5D(true, true, 0);
        }

        //public void loadFLIM(UInt16[][][][] FLIMData)
        //{
        //    //if (nChannels != FLIMData.GetLength(0))
        //    //    return;

        //    //if (n_time != FLIMData[0].GetLength(0))
        //    //    return;

        //    //width = FLIMData[0].GetLength(1);
        //    //height = FLIMData[0].GetLength(2);

        //    for (int i = 0; i < nChannels; i++)
        //    {
        //        fittingList[i] = Enumerable.Repeat<double>(0.0, 6).ToArray(); //Reset fittingList...
        //    }

        //    FLIMRaw = FLIMData;
        //    ZProjection = false;
        //    ZProjectionCalculated = false;
        //    //State.Spc.spcData.resolution = psPerUnit;
        //    //State.Acq.nChannels = nChannels;
        //    //State.Acq.linesPerFrame = height;
        //    //State.Acq.pixelsPerLine = width;
        //}

        public void LoadFLIMdata5D_Realtime(ushort[][][,,] FLIMData5D)
        {
            nChannels = FLIMData5D.Length;
            for (int ch = 0; ch < nChannels; ch++)
                if (FLIMData5D[ch] != null)
                    nFastZ = FLIMData5D[ch].Length;

            FLIMRaw5D = CreateLinear5D(FLIMData5D);
        }

        public ushort[][][] CreateLinear5D(ushort[][][,,] FLIM5D)
        {
            int nCh = FLIM5D.Length;
            var flimData = new ushort[nCh][][];
            for (int ch = 0; ch < nCh; ch++)
            {
                if (FLIM5D[ch] != null)
                {
                    int nZ = FLIM5D[ch].Length;
                    flimData[ch] = new ushort[nZ][];
                    for (int z = 0; z < nZ; z++)
                    {
                        MatrixCalc.CopyFrom3DToLinear<ushort>(FLIM5D[ch][z], out flimData[ch][z]);
                    }
                }
            }

            return flimData;
        }

        public void Delete5DFLIM(int page_position)
        {
            if (page_position < n_pages5D && page_position >= 0)
            {
                for (int i = page_position; i < n_pages5D - 1; i++)
                {
                    FLIM_Pages5D[i + 1] = FLIM_Pages5D[i];
                    acquiredTime_Pages5D[i + 1] = acquiredTime_Pages5D[i];
                }
                Array.Resize(ref FLIM_Pages5D, n_pages5D - 1);
                Array.Resize(ref acquiredTime_Pages5D, n_pages5D - 1);
                n_pages5D -= 1;
            }
        }

        public void Delete4DFLIM(int page_position)
        {
            if (page_position < n_pages && page_position >= 0 && KeepPagesInMemory)
            {
                for (int i = page_position; i < n_pages - 1; i++)
                {
                    FLIM_Pages[i + 1] = FLIM_Pages[i];
                    acquiredTime_Pages[i + 1] = acquiredTime_Pages[i];
                    Project_Pages[i + 1] = Project_Pages[i];
                    LifetimeMapBase_Pages[i + 1] = LifetimeMapBase_Pages[i];
                    ProjectCalculated[i + 1] = ProjectCalculated[i];
                    FLIMMapCalculated[i + 1] = FLIMMapCalculated[i];
                }

                resizePage(n_pages - 1);

                //n_pages -= 1;
                //Array.Resize(ref Project_Pages, n_pages);
                //Array.Resize(ref LifetimeMapBase_Pages, n_pages);
                //Array.Resize(ref acquiredTime_Pages, n_pages);
                //Array.Resize(ref FLIM_Pages, n_pages);
                //Array.Resize(ref ProjectCalculated, n_pages);
                //Array.Resize(ref FLIMMapCalculated, n_pages);
            }
        }


        public void Add_AllFLIM_PageFormat_To_FLIM_Pages5D(ushort[][][] FLIM_i, DateTime acqTime, int page_position)
        {
            expandArray5D(page_position);
            FLIM_Pages5D[page_position] = ImageProcessing.FLIM_Pages2FLIMRaw5D(FLIM_i);

            n_pages5D = FLIM_Pages5D.Length;

            acquiredTime_Pages5D[page_position] = acqTime;
            currentPage5D = page_position;
            n_pages5D = FLIM_Pages5D.Length;
        }

        public void Add5DFLIM(ushort[][][] FLIM_i, DateTime acqTime, int page_position, bool deepCopy)
        {
            expandArray5D(page_position);
            if (deepCopy)
                FLIM_Pages5D[page_position] = MatrixCalc.MatrixCopy3D<ushort>(FLIM_i);
            else
                FLIM_Pages5D[page_position] = FLIM_i;

            n_pages5D = FLIM_Pages5D.Length;

            acquiredTime_Pages5D[page_position] = acqTime;
            currentPage5D = page_position;
            n_pages5D = FLIM_Pages5D.Length;
        }

        public void expandArray5D(int page_position)
        {
            if (FLIM_Pages5D.Length <= page_position)
                Array.Resize(ref FLIM_Pages5D, page_position + 1);

            if (acquiredTime_Pages5D.Length <= page_position)
                Array.Resize(ref acquiredTime_Pages5D, page_position + 1);
        }

        public void LoadFLIMRawFromLinearAllChannels(ushort[][] FLIM_Linear)
        {
            AssureFLIMRawSize();
            for (int ch = 0; ch < nChannels; ch++)
                MatrixCalc.CopyFromLinearTo3D(FLIM_Linear[ch], FLIMRaw[ch]);
        }


        public void addToPageAndCalculate5D(ushort[][][] FLIM_5D, DateTime acqTime, bool calcProjection, bool calcLifetime1, int page_position, bool deepCopy)
        {

            int n_c = FLIM_5D.Length;
            int n_z = State.Acq.FastZ_nSlices;

            for (int c = 0; c < n_c; c++)
            {
                if (FLIM_5D[c] != null)
                {
                    n_z = FLIM_5D[c].Length;
                    break;
                }
            }

            if (KeepPagesInMemory)
            {
                Add5DFLIM(FLIM_5D, acqTime, page_position, deepCopy);
            }

            var FLIM5D = ImageProcessing.PermuteFLIM5D(FLIM_5D); //it is copied.
            for (int z = 0; z < n_z; z++)
            {
                n_pages++;
                PutToPageAndCalculate(FLIM5D[z], acqTime, calcProjection, calcLifetime1, n_pages - 1);
            }
        }

        public void FLIM_Pages_FastZ_ToFLIMRaw5D()
        {
            if (nFastZ > 1)
                Array.Resize(ref FLIM_Pages, nFastZ);

            FLIMRaw5D = ImageProcessing.FLIM_Pages2FLIMRaw5D(FLIM_Pages);
        }

        private void Calculate4DFromFLIMRaw5D()
        {
            int n_c = FLIMRaw5D.Length;
            int n_z = State.Acq.FastZ_nSlices;

            for (int c = 0; c < n_c; c++)
            {
                if (FLIMRaw5D[c] != null)
                {
                    n_z = FLIMRaw5D[c].Length;
                    break;
                }
            }

            var FLIM5D = ImageProcessing.PermuteFLIM5D(FLIMRaw5D); //copied.

            FLIM_Pages = FLIM5D;
            acquiredTime_Pages = new DateTime[n_z];
            ProjectCalculated = new bool[n_z];
            FLIMMapCalculated = new bool[n_z];
            Project_Pages = new ushort[n_z][][][];
            LifetimeMapBase_Pages = new float[n_z][][][];

            for (int z = 0; z < n_z; z++)
            {
                acquiredTime_Pages[z] = acquiredTime;
            }

            n_pages = n_z;
        }

        public void resizePage5D(int new_pageN)
        {
            n_pages5D = new_pageN;
            Array.Resize(ref FLIM_Pages5D, new_pageN);
            Array.Resize(ref acquiredTime_Pages5D, new_pageN);
        }

        public void resizePage(int new_pageN)
        {
            n_pages = new_pageN;
            Array.Resize(ref acquiredTime_Pages, new_pageN);
            Array.Resize(ref FLIM_Pages, new_pageN);
            Array.Resize(ref Project_Pages, new_pageN);
            Array.Resize(ref LifetimeMapBase_Pages, new_pageN);
            Array.Resize(ref FLIMMapCalculated, new_pageN);
            Array.Resize(ref ProjectCalculated, new_pageN);
        }

        private void expandPage(int new_n_pages)
        {
            if (new_n_pages > Project_Pages.Length)
                Array.Resize(ref Project_Pages, new_n_pages);

            if (new_n_pages > LifetimeMapBase_Pages.Length)
                Array.Resize(ref LifetimeMapBase_Pages, new_n_pages);

            if (new_n_pages > acquiredTime_Pages.Length)
                Array.Resize(ref acquiredTime_Pages, new_n_pages);

            if (new_n_pages > FLIM_Pages.Length && (KeepPagesInMemory || nFastZ > 1))
            {
                Array.Resize(ref FLIM_Pages, new_n_pages);
                n_pages = new_n_pages;
            }

            if (new_n_pages > ProjectCalculated.Length)
                Array.Resize(ref ProjectCalculated, new_n_pages);

            if (new_n_pages > FLIMMapCalculated.Length)
                Array.Resize(ref FLIMMapCalculated, new_n_pages);

        }

        public void CopyFromFLIM_PageToFLIMRaw(int page)
        {
            for (int ch = 0; ch < nChannels; ch++)
            {
                if (FLIM_Pages != null && FLIM_Pages[page] != null && FLIM_Pages[page][ch] != null)
                {
                    if (FLIM_Pages[page][ch].Length != n_time[ch] * width * height)
                        FLIMRaw[ch] = new ushort[height, width, n_time[ch]];

                    MatrixCalc.CopyFromLinearTo3D(FLIM_Pages[page][ch], FLIMRaw[ch]);
                }
                else
                {
                    Debug.WriteLine("FLIMData CopyFromFLIM_PagesToFLIMRaw error");
                }
            }
        }

        public ushort[][] CopyFrom3DToLinearChannels(ushort[][][][] FLIM4D)
        {
            var linear = new ushort[nChannels][];
            for (int i = 0; i < nChannels; i++)
            {
                if (FLIM4D[i] != null)
                    MatrixCalc.CopyFrom3DToLinear<ushort>(FLIM4D[i], out linear[i]);
                else
                    linear[i] = null;
            }
            return linear;
        }

        public ushort[][] CopyFrom3DToLinearChannels(ushort[][,,] FLIM4D)
        {
            var linear = new ushort[nChannels][];
            for (int i = 0; i < nChannels; i++)
            {
                if (FLIM4D[i] != null)
                    MatrixCalc.CopyFrom3DToLinear<ushort>(FLIM4D[i], out linear[i]);
                else
                    linear[i] = null;
            }
            return linear;
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
                FLIMRaw = makeFLIMRawTypeArray(nChannels, height, width, n_time);
        }

        public void PutToPageOnly(ushort[][] FLIM_page, DateTime acqTime, int page)
        {
            expandPage(page + 1);
            FLIM_Pages[page] = FLIM_page;
            acquiredTime_Pages[page] = acqTime;
            ProjectCalculated[page] = false;
            FLIMMapCalculated[page] = false;
        }

        public void CalculateZProjectionImageOnly(int channel)
        {
            var ZProc1 = new ushort[Project_Pages.Length][][];
            for (int z = 0; z < Project_Pages.Length; z++)
                ZProc1[z] = Project_Pages[z][channel];

            Project[channel] = ImageProcessing.GetMaxZProjection(ZProc1);
        }

        public void CalculateAllPages_Direct(bool calculateLifetimeMap)
        {
            for (int i = 0; i < n_pages; i++)
            {
                CalculatePage_Direct(i, calculateLifetimeMap);
            }
        }

        public void fitRangeChanged()
        {
            for (int i = 0; i < n_pages; i++)
                FLIMMapCalculated[i] = false;
        }

        public void CalculatePage_Direct(int page, bool calculateLifetimeMap)
        {
            if (ProjectCalculated.Length > page && !ProjectCalculated[page] && FLIM_Pages[page] != null)
            {
                ushort[][][] prjct = new ushort[nChannels][][];
                for (int ch = 0; ch < nChannels; ch++)
                    prjct[ch] = ImageProcessing.GetProjectFromFLIM_Linear(FLIM_Pages[page][ch], dimensionYXT(ch));

                Project_Pages[page] = prjct;
                ProjectCalculated[page] = true;
            }

            if (calculateLifetimeMap && !FLIMMapCalculated[page])
            {
                var lifetime_map = new float[nChannels][][];
                for (int ch = 0; ch < nChannels; ch++)
                    lifetime_map[ch] = ImageProcessing.GetLifetimeMapFromFLIM_Linear(FLIM_Pages[page][ch], fit_range[ch], (float)psPerUnit, 0, dimensionYXT(ch));

                LifetimeMapBase_Pages[page] = lifetime_map;
                FLIMMapCalculated[page] = true;
            }

        }

        /// <summary>
        /// Put FLIM_i reference to FLIM_Pages, and then calculate project and lifetime.
        /// </summary>
        /// <param name="FLIM_i"></param>
        /// <param name="acqTime"></param>
        /// <param name="calcProjection"></param>
        /// <param name="calcLifetime1"></param>
        /// <param name="page"></param>
        public void PutToPageAndCalculate(ushort[][] FLIM_i, DateTime acqTime, bool calcProjection, bool calcLifetime1, int page)
        {
            expandPage(page + 1);

            FLIM_Pages[page] = FLIM_i;

            CalculatePage_Direct(page, true);

            acquiredTime_Pages[page] = acqTime;
            //FLIMMapCalculated[page] = FLIMMapCalculated[page] || calcLifetime1;
            //ProjectCalculated[page] = ProjectCalculated[page] || calcProjection;
            currentPage = page;
        }


        public void calculateProjectCh(int ch)
        {
            if (FLIMRaw == null || FLIMRaw[ch] == null)
            {
                Project[ch] = null;
                ProjectF[ch] = null;
                return;
            }

            Project[ch] = ImageProcessing.GetProjectFromFLIM(FLIMRaw[ch]);
            if (Project[ch] != null)
                ProjectF[ch] = Project[ch];
            else
                ProjectF[ch] = null;
        }

        public void calculateProject()
        {
            Project = new UInt16[nChannels][][];
            ProjectF = new UInt16[nChannels][][];
            for (int ch = 0; ch < nChannels; ch++)
            {
                calculateProjectCh(ch);
            }
        }

        public void fitDataROIs(int mode)
        {
            //if (!Fit_type.Equals(FitType.GlobalRois))
            //    return;

            int nROIs = ROIs.Count;

            for (int i = 0; i < nROIs; i++)
            {
                ROIs[i].beta.Clear();
                ROIs[i].xi_square = new double[nChannels];
                ROIs[i].tau_m = new double[nChannels];

                if (ROIs[i].LifetimeX.Count != nChannels)
                    return;

                for (int ch = 0; ch < nChannels; ch++)
                {
                    double[] beta1 = fittingList[ch]; //fitting from all ROIs.
                    int n = ROIs[i].LifetimeX[ch].Length;

                    int p = 6;
                    //int p = beta1.Length;

                    bool[] fix = Enumerable.Repeat<bool>(true, p).ToArray();

                    double[] x = ROIs[i].LifetimeX[ch]; //this is just 1,2,3,4...
                    double[] y = ROIs[i].LifetimeY[ch];

                    double[] beta0;

                    if (p == beta1.Length)
                        beta0 = (double[])beta1.Clone();
                    else
                        beta0 = new double[p];

                    double maxY = ROIs[i].LifetimeX[ch].Max();
                    if (mode == 1)
                    {
                        beta0[0] = maxY;
                    }
                    else
                    {
                        beta0[0] = maxY / 2;
                        beta0[2] = maxY / 2;
                    }

                    Fitting.Nlinfit fit = new Fitting.Nlinfit(beta0, x, y);
                    fit.fix = fix;

                    if (mode == 1)
                    {
                        fit.modelFunc = ((betaA, xA) => ExpGaussArray(betaA, xA));
                        fix[0] = false;
                        fix[1] = false;
                    }
                    else
                    {
                        fit.modelFunc = ((betaA, xA) => Exp2GaussArray(betaA, xA));
                        fix[0] = false;
                        fix[2] = false;
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

                    ROIs[i].tau_m[ch] = tau_mA;
                    ROIs[i].xi_square[ch] = fit.xi_square;
                    ROIs[i].beta.Add(fit.beta);
                } //Channels
            } //ROI
        }


        public void fitData(int ch, int mode, double[] beta0, bool[] fix)
        {
            if (Lifetime_X == null || ch >= Lifetime_X.Count)
                return;

            if (Lifetime_X[ch] == null)
                return;

            int n = Lifetime_X[ch].Length;
            int p = 6;// beta0.Length;

            double[] x = new double[n];
            double[] y = new double[n];

            double maxY = -1000;
            double maxX = 0;
            double sum = 0;
            double sumX = 0;

            x = Lifetime_X[ch];
            y = Lifetime_Y[ch];

            for (int i = 0; i < n; i++)
            {
                if (y[i] > maxY)
                {
                    maxY = y[i];
                    maxX = x[i];
                }
                sum = sum + y[i];
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

            Fitting.Nlinfit fit = new Fitting.Nlinfit(beta0, x, y);
            fit.fix = fix;
            if (mode == 1)
            {
                fit.modelFunc = ((betaA, xA) => ExpGaussArray(betaA, xA));
                fit.betaMax[1] = resP / minTau;
                fit.betaMin[1] = 1 / maxTau;
                fit.betaMax[2] = tauG_Max / resP; //picosecond
                fit.betaMin[2] = tauG_Min / resP;
                //fit.betaMax[3] = 10000 / resP;
            }
            else
            {
                fit.modelFunc = ((betaA, xA) => Exp2GaussArray(betaA, xA));
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
            if (mode == 1)
            {
                tau_m[ch] = 1 / fit.beta[1];
            }
            else
            {

                double[] b = fit.beta;

                //res = psPerUnit / 1000;
                Console.Write("Fitting = {0}, {1}", res / b[1], res / b[3]);

                tau_m[ch] = (b[0] / b[1] / b[1] + b[2] / b[3] / b[3]) / (b[0] / b[1] + b[2] / b[3]);

            }

            //tau_m[ch] = sumX / sum;
            offset_fit[ch] = res * (sumX / sum - tau_m[ch]);
            fitCurve[ch] = fit.fitCurve;
            fittingList[ch] = (double[])fit.beta.Clone();
            Residuals[ch] = fit.residual;
            xi_square[ch] = fit.xi_square;
            Roi.beta[ch] = (double[])fit.beta.Clone();
            Roi.xi_square[ch] = fit.xi_square;

            Roi.tau_m[ch] = tau_m[ch];
        }



        public double Exp2Gauss(double[] beta0, double x)
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

            y = ExpGauss(beta1, x) + ExpGauss(beta2, x);
            return y;
        }

        public double[] Exp2GaussArray(double[] beta0, double[] x)
        {
            double[] y = new double[x.Length];

            for (int i = 0; i < x.Length; i++)
            {
                y[i] = Exp2Gauss(beta0, x[i]);
            }
            return y;
        }

        public double ExpGauss(double[] beta0, double x)
        {
            double y;
            double res = State.Spc.spcData.resolution[0]; //picoseconds
            double pulseI = 1.0e12 / State.Spc.datainfo.syncRate[0] / res;

            double pop1 = beta0[0];
            double k1 = beta0[1];
            double tauG = beta0[2];
            double t0 = beta0[3];

            double[] beta1 = { pop1, k1, tauG, t0 };
            double[] beta2 = { pop1, k1, tauG, t0 - pulseI };

            y = MatrixCalc.ExpGauss(beta1, x) + MatrixCalc.ExpGauss(beta2, x);

            return y;
        }


        public double[] ExpGaussArray(double[] beta0, double[] x)
        {
            double[] y = new double[x.Length];

            for (int i = 0; i < x.Length; i++)
            {
                y[i] = ExpGauss(beta0, x[i]);
            }
            return y;
        }

        public void saveFittingParameters(double[] fitting_param, int channel)
        {
            int c = channel;

            double[] fit_param1 = (double[])fitting_param.Clone();
            object obj = State.Spc.analysis;

            obj.GetType().GetField("fit_param" + (c + 1)).SetValue(obj, fit_param1);


            //if (c == 0)
            //    State.Spc.analysis.fit_param1 = fit_param1;
            //else if (c == 1)
            //    State.Spc.analysis.fit_param2 = fit_param1;
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
        /// Projection type is enumerated.
        /// </summary>
        public enum projectionType
        {
            Sum = 1,
            Max = 2,
            Min = 3,
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
            FLIMRawZProjection = new ushort[nChannels][,,];
            for (int ch = 0; ch < nChannels; ch++)
            {
                if (FLIMRaw[ch] != null)
                    FLIMRawZProjection[ch] = (ushort[,,])FLIMRaw[ch].Clone();
                else
                    FLIMRawZProjection[ch] = null;
            }

            ushort[][] FLIMRawZPro = new ushort[nChannels][];
            for (int ch = 0; ch < nChannels; ch++)
            {
                if (FLIM_Pages[startPage][ch] != null)
                    FLIMRawZPro[ch] = (ushort[])FLIM_Pages[startPage][ch].Clone();
            }

            if (procType == projectionType.Sum)
            {
                for (int page = startPage; page < endPage; page++)
                {
                    for (int ch = 0; ch < nChannels; ch++)
                    {
                        if (FLIM_Pages[page][ch] != null)
                            MatrixCalc.ArrayCalc(FLIMRawZPro[ch], FLIM_Pages[page][ch], "Add");
                        else
                        {
                            FLIMRawZPro[ch] = null;
                            break;
                        }
                    }
                }
            }
            else if (procType == projectionType.Max || procType == projectionType.Min)
            {
                UInt16[][] ProjectAMax = MatrixCalc.MatrixCreate2D<UInt16>(height, width);

                CalculateAllPages_Direct(false);

                for (int ch = 0; ch < nChannels; ch++)
                {
                    for (int page = startPage; page < endPage; page++)
                    {
                        if (Project_Pages[page] == null)
                            return;

                        UInt16[][] ProjectA = Project_Pages[page][ch];

                        if (page == startPage)
                            ProjectAMax = MatrixCalc.MatrixCopy2D(ProjectA);
                        else
                        {
                            if (procType == projectionType.Max)
                            {
                                for (int y = 0; y < height; y++)
                                    for (int x = 0; x < width; x++)
                                        if (ProjectAMax[y][x] < ProjectA[y][x])
                                        {
                                            Array.Copy(FLIM_Pages[page][ch], (y * width + x) * n_time[ch], FLIMRawZPro[ch], (y * width + x) * n_time[ch], n_time[ch]);
                                            ProjectAMax[y][x] = ProjectA[y][x];
                                        }
                            }
                            else
                            {
                                for (int y = 0; y < width; y++)
                                    for (int x = 0; x < width; x++)
                                        if (ProjectAMax[y][x] > ProjectA[y][x])
                                        {
                                            Array.Copy(FLIM_Pages[page][ch], (y * width + x) * n_time[ch], FLIMRawZPro[ch], (y * width + x) * n_time[ch], n_time[ch]);
                                            ProjectAMax[y][x] = ProjectA[y][x];
                                        }
                            }
                        }
                    } //page
                } //channel
            } //max or min

            for (int ch = 0; ch < nChannels; ch++)
                MatrixCalc.CopyFromLinearTo3D(FLIMRawZPro[ch], FLIMRawZProjection[ch]);

            //currentPage = page_range[0];

            Debug.WriteLine("elapsed time (calculaton) - page" + n + " = " + sw.ElapsedMilliseconds);
            ZProjectionCalculated = true;

            FLIMRaw = FLIMRawZProjection;
            ZProjection = true;

            calculateAll();
        }

        public void filterMAP(int fw, int ch)
        {
            if (FLIM_on[ch])
            {
                if (LifetimeMap == null)
                    LifetimeMap = new float[nChannels][][];
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
                        LifetimeMapF = new float[nChannels][][];
                    LifetimeMapF[ch] = MatrixCalc.MatrixCopy2D<float>(LifetimeMap[ch]);
                }

                if (ProjectF == null)
                    ProjectF = new ushort[nChannels][][];
                ProjectF[ch] = MatrixCalc.MatrixCopy2D<UInt16>(Project[ch]);
            }

            else
            {
                State.Display.filterWindow_FLIM = fw;

                var temp = MatrixCalc.MatrixCreate2D<float>(height, width);
                var tempI = MatrixCalc.MatrixCreate2D<UInt16>(height, width); //new UInt16[height, width];

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
                                        sum = sum + LifetimeMap[ch][posY][posX];
                                    sum1 = sum1 + Project[ch][posY][posX];
                                    pix = pix + 1.0f;
                                }

                            }
                        if (pix > 0)
                        {
                            if (FLIM_on[ch])
                                temp[y][x] = sum / pix;
                            tempI[y][x] = (UInt16)(sum1 / pix);
                        }
                    }

                if (FLIM_on[ch])
                    LifetimeMapF[ch] = temp; //c];
                ProjectF[ch] = tempI;
            }

        }


        public void calculate_MeanLifetime_General(ROI roi, int ch, bool threeD)
        {
            double sum = 0;
            double pix = 0;
            double sumi = 0;
            double pixi = 0;
            double val;
            int intensity;
            int nRoi = ROIs.Count;

            int nPages = n_pages;
            if (!threeD)
            {
                nPages = 1;
                calculateLifetimeMap();
            }
            else
            {
                CalculateAllPages_Direct(true);
            }

            for (int page = 0; page < nPages; page++)
            {
                if (!threeD || roi.Z.Any(z => z == page))
                {
                    ushort[][] img;
                    float[][] lf_img;

                    if (threeD)
                    {
                        img = Project_Pages[page][ch];
                        lf_img = LifetimeMapBase_Pages[page][ch];
                    }
                    else
                    {
                        img = Project[ch];
                        if (LifetimeMapBase != null)
                            lf_img = LifetimeMapBase[ch];
                        else
                            lf_img = null;
                    }

                    //Note that if lf_img is null, it will calculate just intensity.
                    if (img == null)
                        return;

                    int height1 = img.Length;
                    int width1 = img[0].Length;

                    //Check if they have the same size. If different size, just calculate intensity.
                    if (lf_img == null || height1 != lf_img.Length || width1 != lf_img[0].Length)
                        lf_img = null;

                    for (int y = roi.Rect.Top; y < roi.Rect.Bottom; y++)
                        for (int x = roi.Rect.Left; x < roi.Rect.Right; x++)
                            if (x < width1 && y < height1 && x >= 0 && y >= 0)
                            {
                                intensity = img[y][x];

                                Point P = new Point(x, y);

                                if (lf_img != null)
                                    val = lf_img[y][x] - offset[ch];
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

            roi.intensity[ch] = sumi / pixi;
            roi.sumIntensity[ch] = sumi;
            roi.nPixels[ch] = pixi;
            if (pix > 0)
                roi.tau_m_fromMAP[ch] = sum / pix;
            else
                roi.tau_m_fromMAP[ch] = 0;
        }


        public void calculate_MeanLifetime_ch(int ch)
        {
            bool threeD = ThreeDRoi && (nFastZ > 1 || ZStack) && n_pages > 1;

            calculate_MeanLifetime_General(Roi, ch, threeD);
            for (int i = 0; i < ROIs.Count; i++)
                calculate_MeanLifetime_General(ROIs[i], ch, threeD);
        }

        public void calculate_MeanLifetime()
        {
            for (int ch = 0; ch < nChannels; ch++)
                calculate_MeanLifetime_ch(ch);
        }


        public void calculateLifetimeChRoi(int ch, ROI roi, int[] range, bool threeD)
        {
            int simd_ulong = Vector<ulong>.Count;
            int simd_ushort = Vector<ushort>.Count;
            bool simd_accel = Vector.IsHardwareAccelerated;

            int n_point = range[1] - range[0];
            int fitStart = range[0];

            roi.LifetimeY[ch] = new double[n_point];
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

            int xstart = 0;
            int xend = width;
            int ystart = 0;
            int yend = height;

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

            ushort[][] Img = Project[ch];
            ushort[,,] lifetimeImg = FLIMRaw[ch];

            if (threeD)
            {
                nPages = n_pages;
                lifetimeImg = (ushort[,,])FLIMRaw[ch].Clone(); //Need to copy, because we load the data.
            }

            for (int page = 0; page < nPages; page++)
            {
                if (!threeD || roi.Z.Any(z => z == page))
                {
                    if (threeD)
                    {
                        Img = Project_Pages[page][ch];
                        MatrixCalc.CopyFromLinearTo3D(FLIM_Pages[page][ch], lifetimeImg);
                    }

                    if (Img == null || lifetimeImg == null)
                        return;

                    ushort[] FLIMRawT = new ushort[n_time[ch]];
                    for (int y = ystart; y < yend; ++y)
                        for (int x = xstart; x < xend; ++x)
                        {
                            int val, intensity;
                            intensity = Img[y][x];
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
                                            roi.LifetimeY[ch][t] += val;
                                        }
                                    }
                                    else
                                    {
                                        for (int t = 0; t < n_point; ++t)
                                        {
                                            val = FLIMRawT[t + fitStart];
                                            roi.LifetimeY[ch][t] += val;
                                        }
                                    }
                                } //roi condition.
                            } //threshold
                        } // xy loop
                } //threeD condition.
            } //page loop

            for (int k = 0; k < iblock; k++)
                Vector.ConvertToDouble(vlist[k]).CopyTo(roi.LifetimeY[ch], k * simd_ulong);

        } //roi                    


        public void calculateLifetimeCh(int ch)
        {
            if (FLIMRaw[ch] == null)
                return;

            if (n_time[ch] <= 2 || !FLIM_on[ch])
                return;

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
                return;

            double[] lifetimeX1 = new double[n_point];
            double[] lifetimeY1 = new double[n_point];
            //double[,] sum1 = new double[nRoi,n_point];

            bool threeD = ThreeDRoi && (nFastZ > 1 || ZStack) && n_pages > 1;

            if (Fit_type.Equals(FitType.GlobalRois))
            {
                for (int i = 0; i < nRoi; i++)
                {
                    calculateLifetimeChRoi(ch, ROIs[i], fit_range[ch], threeD);
                } //Roi loop


                for (int i = 0; i < nRoi; i++)
                    for (int t = 0; t < n_point; t++)
                    {
                        ROIs[i].LifetimeX[ch][t] = (double)(fitStart + t);
                        lifetimeY1[t] += ROIs[i].LifetimeY[ch][t]; //Add all ROIs.
                    }
            }
            else if (Fit_type.Equals(FitType.WholeImage))
            {
                var wholeRoi = new ROI(ROI.ROItype.Rectangle, new Rectangle(0, 0, width, height), nChannels, -100, false, new int[] { 0, 0 });
                calculateLifetimeChRoi(ch, wholeRoi, fit_range[ch], false);
                lifetimeY1 = wholeRoi.LifetimeY[ch];
            }
            else
            {
                calculateLifetimeChRoi(ch, Roi, fit_range[ch], threeD);
                lifetimeY1 = Roi.LifetimeY[ch];

            }//else

            for (int t = 0; t < n_point; t++)
                lifetimeX1[t] = (double)(fitStart + t);

            double[] someVal = { 0.0, 0.0 };
            while (Lifetime_X.Count < nChannels)
            {
                for (int i = 0; i < nChannels - Lifetime_X.Count; i++)
                {
                    Lifetime_X.Add(someVal);
                    Lifetime_Y.Add(someVal);
                }
            }

            Lifetime_X[ch] = lifetimeX1;
            Lifetime_Y[ch] = lifetimeY1;

        } // end function


        /// <summary>
        /// Calculate Lifetime
        /// </summary>
        public void calculateLifetime()
        {
            if (!FLIM_on.Any(x => x == true))
                return;

            int nRoi = ROIs.Count;

            if (Fit_type.Equals(FitType.GlobalRois))
            {
                for (int i = 0; i < nRoi; i++)
                {
                    ROIs[i].LifetimeX.Clear(); //Initialize
                    ROIs[i].LifetimeY.Clear();
                    for (int ch = 0; ch < nChannels; ch++)
                    {
                        int n_point = fit_range[ch][1] - fit_range[ch][0];
                        ROIs[i].LifetimeX.Add(new double[n_point]); //Channel1
                        ROIs[i].LifetimeY.Add(new double[n_point]);
                    }
                }
            }

            for (int ch = 0; ch < nChannels; ch++)
            {
                calculateLifetimeCh(ch);
            } //ch
        }

        public void calculateLifetimeMapCh(int ch)
        {
            if (FLIMRaw == null || FLIMRaw[ch] == null)
                return;

            if (LifetimeMapBase == null || LifetimeMapBase.Length != nChannels)
                LifetimeMapBase = new float[nChannels][][];

            LifetimeMapBase[ch] = ImageProcessing.GetLifetimeMapFromFLIM(FLIMRaw[ch], fit_range[ch], (float)psPerUnit, 0);

            if (LifetimeMap == null || LifetimeMap.Length != nChannels)
            {
                LifetimeMap = new float[nChannels][][];
                LifetimeMapF = new float[nChannels][][];
            }

            if (LifetimeMapBase != null && LifetimeMapBase[ch] != null)
            {
                LifetimeMap[ch] = MatrixCalc.SubtractConstantFromMatrix(LifetimeMapBase[ch], (float)offset[ch]);
                LifetimeMapF[ch] = LifetimeMap[ch];
            }
            //if (currentPage < LifetimeMapBase_Pages.Count && currentPage >= 0)
            //    LifetimeMapBase_Pages[currentPage][ch] = (double[,])LifetimeMapBase[ch].Clone();
        }

        public void calculateLifetimeMap()
        {
            if (!FLIM_on.Any(x => x == true))
                return;

            LifetimeMapBase = new float[nChannels][][];
            LifetimeMap = new float[nChannels][][];
            LifetimeMapF = new float[nChannels][][];

            for (int i = 0; i < nChannels; i++)
            {
                calculateLifetimeMapCh(i);
                //LifetimeMap[i] = ImageProcessing.GetLifetimeMapFromFLIM(FLIMRaw[i], psPerUnit, offset[i]);
            }
        }


        public void calculateAll()
        {
            calculateProject();
            calculateLifetimeMap();
            calculateLifetime();
        }

        //PAGES////////////////////
        public void preAllocPages(int num_pages)
        {
            clearMemory();
            for (int i = 0; i < num_pages; i++)
            {
                PutToPageAndCalculate(MatrixCalc.MatrixCreate2D<ushort>(nChannels, height * width * n_time.Max()), DateTime.Now, false, false, i);
            }
        }

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
            Array.Copy(FLIMMapCalculated, initialPage, FLIMMapCalculated, 0, new_pageN);
            Array.Copy(ProjectCalculated, initialPage, ProjectCalculated, 0, new_pageN);
            Array.Copy(acquiredTime_Pages, initialPage, acquiredTime_Pages, 0, new_pageN);

            resizePage(new_pageN);

            //Array.Resize(ref FLIM_Pages, new_pageN);
            //Array.Resize(ref Project_Pages, new_pageN);
            //Array.Resize(ref LifetimeMapBase_Pages, new_pageN);
            //Array.Resize(ref FLIMMapCalculated, new_pageN);
            //Array.Resize(ref ProjectCalculated, new_pageN);

            //ushort[][][] temp_FLIM_page = (ushort[][][])FLIM_Pages.Clone();
            //ushort[][][][] temp_Project_Pages = (ushort[][][][])Project_Pages.Clone();
            //float[][][][] temp_LifetimeMapBase_Pages = (float[][][][])LifetimeMapBase_Pages.Clone();
            //bool[] temp_FLIMMapCalculated = (bool[])FLIMMapCalculated.Clone();
            //bool[] temp_ProjectCalculated = (bool[])ProjectCalculated.Clone();

            //FLIM_Pages = new ushort[new_pageN][][];
            //Project_Pages = new ushort[new_pageN][][][];
            //LifetimeMapBase_Pages = new float[new_pageN][][][];
            //FLIMMapCalculated = new bool[new_pageN];
            //ProjectCalculated = new bool[new_pageN];

            //for (int i = 0; i < NPages; i++)
            //{
            //    FLIM_Pages[i] = temp_FLIM_page[i + initialPage];
            //    Project_Pages[i] = temp_Project_Pages[i + initialPage];
            //    FLIMMapCalculated[i] = temp_FLIMMapCalculated[i + initialPage];
            //    ProjectCalculated[i] = temp_ProjectCalculated[i + initialPage];
            //}

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
            FLIM_Pages5D = new ushort[1][][][];
            acquiredTime_Pages5D = new DateTime[1];
            n_pages5D = 0;
        }

        public void clearPages4D()
        {
            //MatrixCalc.MatrixNull5D(FLIM_Pages);
            //MatrixCalc.MatrixNull4D(Project_Pages);
            //MatrixCalc.MatrixNull4D(LifetimeMapBase_Pages);

            FLIM_Pages = new ushort[1][][];
            //
            Project_Pages = new ushort[1][][][];
            LifetimeMapBase_Pages = new float[1][][][];
            acquiredTime_Pages = new DateTime[1];
            FLIMMapCalculated = new bool[1];
            ProjectCalculated = new bool[1];
            currentPage = -1;
            n_pages = 0;

            ZProjectionCalculated = false;
            ZProjection = false;
        }


        public void addMAPAtPage(int page)
        {
            if (page > Project_Pages.Length)
                expandPage(page + 1);

            //Debug.WriteLine("Added to Page");

            Project_Pages[page] = MatrixCalc.MatrixCopy3D<UInt16>(Project);
            ProjectCalculated[page] = true;
            LifetimeMapBase_Pages[page] = MatrixCalc.MatrixCopy3D<float>(LifetimeMapBase);
            FLIMMapCalculated[page] = true;
        }

        public void addCurrentFLIMRawToPage4D(bool addMAP, bool AddToFLIM_Pages, int page)
        {
            if (page >= Project_Pages.Length)
                expandPage(page + 1);

            if (AddToFLIM_Pages)
            {
                FLIM_Pages[page] = CopyFrom3DToLinearChannels(FLIMRaw);
                Debug.WriteLine("Added to page" + page);
            }

            ushort[][][] newImage;
            float[][][] newFLIM;

            if (addMAP)
            {
                calculateAll();
                newImage = MatrixCalc.MatrixCopy3D<UInt16>(Project);
                if (FLIM_on.Any(x => x == true))
                {
                    newFLIM = MatrixCalc.MatrixCopy3D<float>(LifetimeMapBase);
                    FLIMMapCalculated[page] = true;
                }
                else
                {
                    newFLIM = null;
                    FLIMMapCalculated[page] = false;
                }

                ProjectCalculated[page] = true;
            }
            else
            {
                newImage = null;
                newFLIM = null;
                ProjectCalculated[page] = false;
                FLIMMapCalculated[page] = false;
            }

            currentPage = page;

            Project_Pages[page] = newImage;

            if (FLIM_on.Any(x => x == true))
            {
                LifetimeMapBase_Pages[page] = newFLIM;
            }

            acquiredTime_Pages[page] = acquiredTime;

            n_pages = FLIM_Pages.Length;
        }

        public void SetupPagesFromZProject(ushort[][][] flim_pages, DateTime dt)
        {
            FLIM_Pages = flim_pages; // MatrixCalc.MatrixCopy3D(flim_pages);
            n_pages = FLIM_Pages.Length;
            Project_Pages = new ushort[n_pages][][][];
            LifetimeMapBase_Pages = new float[n_pages][][][];
            ProjectCalculated = new bool[n_pages];
            FLIMMapCalculated = new bool[n_pages];
            ZProjectionCalculated = false;
            acquiredTime = dt;
            acquiredTime_Pages = Enumerable.Repeat(dt, n_pages).ToArray();
        }

        public void addCurrentToPage5D(bool AddMAP, bool AddToFLIM_Pages, int page)
        {
            if (page + 1 > FLIM_Pages5D.Length)
                Array.Resize(ref FLIM_Pages5D, page + 1);

            if (page + 1 > acquiredTime_Pages5D.Length)
                Array.Resize(ref acquiredTime_Pages5D, page + 1);

            if (AddToFLIM_Pages)
            {
                FLIM_Pages5D[page] = MatrixCalc.MatrixCopy3D<UInt16>(FLIMRaw5D);
                acquiredTime_Pages5D[page] = acquiredTime;
            }

            acquiredTime = acquiredTime_Pages5D[page];
            Calculate4DFromFLIMRaw5D();

            //Calculate All!

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
                        Array.Resize(ref FLIM_Pages, nFastZ);

                    Calculate4DFromFLIMRaw5D();
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
                //FLIMRaw = FLIM_Pages[page];
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
                    //FLIMRaw = FLIM_Pages[page];
                    else
                        MessageBox.Show("Could not open this image");
                }

            }

            if (ProjectCalculated[page])
            {
                Project = Project_Pages[page];
            }
            else
            {
                calculateProject();
                Project_Pages[page] = MatrixCalc.MatrixCopy3D(Project);
                ProjectCalculated[page] = true;
            }

            if (FLIMMapCalculated[page])
            {
                LifetimeMapBase = LifetimeMapBase_Pages[page]; // MatrixCalc.MatrixCopy3D<double>(LifetimeMapBase_Pages[currentPage]);
            }
            else
            {
                if (FLIM_on.Any(x => x == true))
                {
                    calculateLifetimeMap();
                    LifetimeMapBase_Pages[page] = MatrixCalc.MatrixCopy3D(LifetimeMapBase);
                    FLIMMapCalculated[page] = true;
                }
            }

            if (acquiredTime_Pages != null && acquiredTime_Pages.Length > page)
                acquiredTime = acquiredTime_Pages[page];

            currentPage = page;

            ZProjection = false;
            //calculateAll();

        }

        //////////////////////////////////////////////////////////////////////////
        public UInt16[,,] CreateFLIM_Sim(int n_dtime, int height, int width)
        {
            UInt16[,,] acqFImg = new ushort[height, width, n_dtime];
            double res = State.Spc.spcData.resolution[0] / 1000.0;
            double t_decay = 2.6 / res;
            //double BigN = 100000.0;
            double offset = (2.0 / res);
            double tau_g = 0.15 / 0.25;
            double F;
            int F_int;
            double[] beta0 = { 10, 1 / t_decay, tau_g, offset };
            double[] beta2 = { 0.3, 1 / (2.6 / res), 0.6, 1 / (0.5 / res), tau_g, offset };
            Random rnd = new Random();
            //Fitting.Nlinfit fit = new Fitting.Nlinfit();

            for (int y = 0; y < height; ++y)

                for (int x = 0; x < width; ++x)
                    for (int t = 0; t < n_dtime; ++t)
                    {
                        //double r = (double)y / (double)height;
                        //double t_decay2 = t_decay * (0.5 + 0.5 * r);
                        //beta0[1] = t_decay2;
                        //F = Fitting.expGauss(beta0, t) + 0.1;

                        F = Exp2Gauss(beta2, t);
                        //F = -F * Math.Log(1.0 - (double)(rnd.Next(1, (int)BigN)) / BigN);

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
            return acqFImg;
        }

        //////////////////////////////////////////////////////////////////////////

        public enum FileType
        {
            ZStack = 1,
            TimeCourse = 2,
            TimeCourse_ZStack = 3,
        }

        public enum Z_Projection
        {
            None = 1,
            Maxium = 2,
            Sum = 3,
        }

        public enum FitType
        {
            WholeImage = 1,
            SelectedRoi = 2,
            GlobalRois = 3,
        } //Class FLIMData
    }
}
