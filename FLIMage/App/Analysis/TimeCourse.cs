using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCSPC_controls;
using MathLibrary;
using Utilities;
using static FLIMage.ScanParameters;

namespace FLIMage.Analysis
{
    public class ImageInfo
    {
        public static string[] paramNames = { "Lifetime", "Lifetime_fit", "Fraction2", "Fraction2_fit",
                            "meanIntensity", "meanIntensity_bg", "sumIntensity", "sumIntensity_bg", "nPixels"};
        public static DateTime startDate = new DateTime(2010, 1, 1);

        public double time_milliseconds; // in millisecond
        public DateTime acquiredTime;
        public String BaseName;
        public String FileName;
        public int nROIs;
        public int nTotalROIs;
        public bool ZStack;
        public int Page;
        public int Page5D;
        public int Slice;
        public int Frame;
        public int FileCounter;
        public int nChannels;
        double Res;
        public double[][] fitting_param;
        public double[] Lifetime;
        public double[] Lifetime_fit;
        public double[] Fraction2;
        public double[] Fraction2_fit;
        public double[] meanIntensity;
        public double[] sumIntensity;
        public double[] meanIntensity_bg;
        public double[] sumIntensity_bg;
        public double[] nPixels;

        public double[][] Lifetime_ROI;
        public double[][] Lifetime_fit_ROI;
        public double[][] Fraction2_ROI;
        public double[][] Fraction2_fit_ROI;
        public double[][] meanIntensity_ROI;
        public double[][] sumIntensity_ROI;
        public double[][] meanIntensity_bg_ROI;
        public double[][] sumIntensity_bg_ROI;
        public double[][] nPixels_ROI;
        public int[] roiID;

        public ImageInfo()
        {
        }

        public ImageInfo(FLIMData FLIM)
        {
            ScanParameters State = FLIM.State;
            acquiredTime = FLIM.acquiredTime;
            Res = State.Spc.spcData.resolution[0] / 1000;
            //ROI Roi = FLIM.Roi;
            ROI Roi = FLIM.Roi;
            ROI roifit = FLIM.RoiFit;
            ROI bgRoi = FLIM.bgRoi;

            List<ROI> ROI_Multi = FLIM.ROIs;
            nROIs = ROI_Multi.Count;
            nTotalROIs = 0;
            foreach (var roi in FLIM.ROIs)
            {
                nTotalROIs += 1;
                if (roi.ROI_type == ROI.ROItype.PolyLine)
                    nTotalROIs += roi.polyLineROIs.Count;
            }

            FileCounter = FLIM.fileCounter; //State.Files.fileCounter;
            bool use5DTimePages = Uses5DTimePages(FLIM) && FLIM.currentPage5D >= 0;
            Page5D = FLIM.currentPage5D;
            Page = use5DTimePages ? FLIM.currentPage5D : FLIM.currentPage;
            ZStack = State.Acq.ZStack;
            nChannels = FLIM.nChannels;

            FileName = FLIM.fileName; //State.Files.fullName();
            BaseName = FLIM.baseName; //State.Files.baseName;

            fitting_param = new double[nChannels][];
            fitting_param[0] = (double[])State.Spc.analysis.fit_param1.Clone();

            if (nChannels > 1)
                fitting_param[1] = (double[])State.Spc.analysis.fit_param2.Clone();

            roiID = null;

            if (nTotalROIs > 0)
            {
                foreach (string fieldname in paramNames)
                {
                    var arr1 = new double[nTotalROIs][];
                    GetType().GetField(fieldname + "_ROI").SetValue(this, arr1);
                }

                roiID = new int[nTotalROIs];

                int i = 0;
                for (int roiN = 0; roiN < nROIs; roiN++)
                {
                    getData(ROI_Multi[roiN], bgRoi, use5DTimePages ? Page : -1);
                    roiID[i] = ROI_Multi[roiN].ID;

                    foreach (string fieldname in paramNames)
                    {
                        var arr1 = (double[][])GetType().GetField(fieldname + "_ROI").GetValue(this);
                        var arr2 = (double[])GetType().GetField(fieldname).GetValue(this);
                        arr1[i] = (double[])arr2.Clone();
                        GetType().GetField(fieldname + "_ROI").SetValue(this, arr1);
                    }

                    i++;

                    if (ROI_Multi[roiN].ROI_type == ROI.ROItype.PolyLine)
                    {
                        foreach (var roi1 in ROI_Multi[roiN].polyLineROIs)
                        {
                            getData(roi1, bgRoi, use5DTimePages ? Page : -1);

                            roiID[i] = (ROI_Multi[roiN].ID + 1) * 1000 + roi1.ID;

                            foreach (string fieldname in paramNames)
                            {
                                var arr1 = (double[][])GetType().GetField(fieldname + "_ROI").GetValue(this);
                                var arr2 = (double[])GetType().GetField(fieldname).GetValue(this);
                                arr1[i] = (double[])arr2.Clone();
                                GetType().GetField(fieldname + "_ROI").SetValue(this, arr1);
                            }
                            i++;
                        }
                    }

                }
            }
            else
            {
                Roi.flim_parameters.tau_m = roifit.flim_parameters.tau_m;
                Roi.flim_parameters.beta = roifit.flim_parameters.beta;
                if (use5DTimePages)
                {
                    ROI_FLIM_Parameters roiPageParams = GetParametersForPage(Roi, Page);
                    ROI_FLIM_Parameters fitPageParams = GetParametersForPage(roifit, Page);
                    if (roiPageParams != null && fitPageParams != null)
                    {
                        roiPageParams.tau_m = fitPageParams.tau_m;
                        roiPageParams.beta = fitPageParams.beta;
                        roiPageParams.n_exponentials = fitPageParams.n_exponentials;
                    }
                }
            }

            getData(Roi, bgRoi, use5DTimePages ? Page : -1);

            DateTime pageTime = acquiredTime;
            bool synthesizeTiming = false;
            if (use5DTimePages)
            {
                if (FLIM.acquiredTime_Pages5D != null
                    && Page >= 0
                    && Page < FLIM.acquiredTime_Pages5D.Length
                    && FLIM.acquiredTime_Pages5D[Page] > ImageInfo.startDate)
                {
                    pageTime = FLIM.acquiredTime_Pages5D[Page];
                }
                else
                {
                    synthesizeTiming = true;
                }
            }
            else if (FLIM.acquiredTime_Pages != null && FLIM.acquiredTime_Pages.Length > 1)
            {
                DateTime firstTime = FLIM.acquiredTime_Pages[0];
                bool allSame = true;
                for (int i = 1; i < FLIM.acquiredTime_Pages.Length; i++)
                {
                    if (FLIM.acquiredTime_Pages[i] != firstTime)
                    {
                        allSame = false;
                        break;
                    }
                }

                if (allSame)
                {
                    pageTime = firstTime;
                    synthesizeTiming = true;
                }
            }

            double tempM = (pageTime - ImageInfo.startDate).TotalMilliseconds;
            if (tempM > 0 && !synthesizeTiming)
            {
                time_milliseconds = tempM;
            }
            else
            {
                DateTime dt = pageTime;
                double time_milliseconds_base = (dt - new DateTime(2000, 1, 1)).TotalMilliseconds;

                if (use5DTimePages)
                {
                    double fInterval = State.Acq.frameInterval();
                    if (State.Acq.aveFrame)
                        fInterval = fInterval * State.Acq.nAveFrame;

                    Frame = Math.Max(0, Page);
                    Slice = Math.Max(0, FLIM.currentPage);

                    double timeFromBase = 1000.0 * Frame * fInterval;

                    time_milliseconds = time_milliseconds_base + timeFromBase;
                }
                else if (FLIM.n_pages > 1)
                {
                    double fInterval = State.Acq.frameInterval();
                    if (State.Acq.aveFrame)
                        fInterval = fInterval * State.Acq.nAveFrame;

                    int numFrames = State.Acq.aveFrame
                        ? Math.Max(1, State.Acq.nFrames / State.Acq.nAveFrame)
                        : Math.Max(1, State.Acq.nFrames);

                    Frame = Page % numFrames;
                    Slice = Page / numFrames;

                    double timeFromBase = 1000.0 * (Frame * fInterval + Slice * State.Acq.sliceInterval);

                    time_milliseconds = time_milliseconds_base + timeFromBase;
                }
                else
                {
                    time_milliseconds = time_milliseconds_base;
                }

            }
        }

        void getData(ROI Roi, ROI bgRoi)
        {
            getData(Roi, bgRoi, -1);
        }

        void getData(ROI Roi, ROI bgRoi, int parameterPage)
        {
            ROI_FLIM_Parameters roiParams = GetParametersForPage(Roi, parameterPage);
            ROI_FLIM_Parameters bgParams = GetParametersForPage(bgRoi, parameterPage);

            Fraction2 = new double[nChannels];
            Fraction2_fit = new double[nChannels];

            meanIntensity = CloneParameterArray(roiParams?.meanIntensity);
            sumIntensity = CloneParameterArray(roiParams?.sumIntensity);

            meanIntensity_bg = (double[])meanIntensity.Clone();
            sumIntensity_bg = (double[])sumIntensity.Clone();

            nPixels = CloneParameterArray(roiParams?.nPixels);

            for (int ch = 0; ch < meanIntensity.Length; ch++)
            {
                if (bgParams?.meanIntensity != null && bgParams.meanIntensity.Length > ch)
                    meanIntensity_bg[ch] = meanIntensity[ch] - bgParams.meanIntensity[ch];
                sumIntensity_bg[ch] = meanIntensity_bg[ch] * nPixels[ch];
            }

            Lifetime = CloneParameterArray(roiParams?.tau_m_fromMAP);
            Lifetime_fit = CloneParameterArray(roiParams?.tau_m);

            for (int ch = 0; ch < nChannels; ch++)
            {
                if (fitting_param == null || fitting_param.Length <= ch || fitting_param[ch] == null || fitting_param[ch].Length <= 3)
                    continue;

                double tauD = fitting_param[ch][1];
                double tauAD = fitting_param[ch][3];
                double tau_m0 = Lifetime[ch];

                Fraction2[ch] = tauD * (tauD - tau_m0) / (tauD - tauAD) / (tauD + tauAD - tau_m0);
                if (roiParams?.tau_m != null && roiParams.tau_m.Length > ch)
                    Lifetime_fit[ch] = roiParams.tau_m[ch];

                if (roiParams != null && roiParams.n_exponentials == 2 &&
                    roiParams.beta != null && roiParams.beta.Length > ch &&
                    roiParams.beta[ch] != null && roiParams.beta[ch].Length > 3)
                {
                    tauD = Res / roiParams.beta[ch][1];
                    tauAD = Res / roiParams.beta[ch][3];
                    //Debug.WriteLine("TauD, TauAD, Tau_m_fit = {0}, {1}, {2}", tauD, tauAD, Lifetime_fit[ch]);
                    double frac = roiParams.beta[ch][2] / (roiParams.beta[ch][0] + roiParams.beta[ch][2]);
                    Fraction2_fit[ch] = frac;
                }
            }
        }

        private static ROI_FLIM_Parameters GetParametersForPage(ROI roi, int parameterPage)
        {
            if (roi == null)
                return null;

            if (parameterPage >= 0 &&
                roi.flim_parameters_Pages != null &&
                parameterPage < roi.flim_parameters_Pages.Length &&
                roi.flim_parameters_Pages[parameterPage] != null)
            {
                return roi.flim_parameters_Pages[parameterPage];
            }

            return roi.flim_parameters;
        }

        private double[] CloneParameterArray(double[] values)
        {
            double[] result = new double[nChannels];
            if (values != null)
                Array.Copy(values, result, Math.Min(values.Length, result.Length));
            return result;
        }

        private static bool Uses5DTimePages(FLIMData FLIM)
        {
            if (FLIM == null || FLIM.n_pages5D <= 0)
                return false;

            if (FLIM.nFastZ > 1)
                return true;

            if (FLIM.ZStack)
                return FLIM.n_pages5D > 1 || HasMultiSlice5DPage(FLIM);

            return FLIM.n_pages5D > 1 && FLIM.FLIM_Pages5D != null;
        }

        private static bool HasMultiSlice5DPage(FLIMData FLIM)
        {
            if (FLIM?.FLIM_Pages5D == null)
                return false;

            int pageCount = Math.Min(FLIM.n_pages5D, FLIM.FLIM_Pages5D.Length);
            for (int page = 0; page < pageCount; page++)
            {
                ushort[][][,,] page5D = FLIM.FLIM_Pages5D[page];
                if (page5D == null)
                    continue;

                for (int ch = 0; ch < page5D.Length; ch++)
                {
                    if (page5D[ch] != null && page5D[ch].Length > 1)
                        return true;
                }
            }

            return false;
        }

        public ImageInfo DeepCopy()
        {
            ImageInfo iminfo1 = new ImageInfo();
            Copier.DeepCopyClass(this, iminfo1);
            return iminfo1;
        }

    }

    /// <summary>
    /// Calculate time coruse from Multiple files?
    /// </summary>
    public class TimeCourse
    {
        public List<ImageInfo> ImInfos = new List<ImageInfo>();
        public String FileName;
        public String BaseName;
        public int FileNumber;
        public HashSet<int> UniqueIDs = new HashSet<int>();
        public double[] time_milliseconds;
        public double[] time_seconds;

        public double[][] Lifetime;
        public double[][] Fraction2;
        public double[][] Lifetime_fit;
        public double[][] Fraction2_fit;

        public double[][] meanIntensity;
        public double[][] sumIntensity;
        public double[][] meanIntensity_bg;
        public double[][] sumIntensity_bg;

        public double[][] nPixels;

        public double[,][] Lifetime_ROI;
        public double[,][] Fraction2_ROI;
        public double[,][] Lifetime_fit_ROI;
        public double[,][] Fraction2_fit_ROI;

        public double[,][] meanIntensity_ROI;
        public double[,][] sumIntensity_ROI;
        public double[,][] meanIntensity_bg_ROI;
        public double[,][] sumIntensity_bg_ROI;

        public double[,][] nPixels_ROI;


        public string[] paramNames = (string[])ImageInfo.paramNames.Clone();

        public int nROI = 0;
        public int nTotalROIs = 0;
        public int nData = 0;
        public int nChannels = 1;
        public double[][][] fitting_param;

        private List<ROI> lineScanRois = null;
        private List<int> lineScanRoiIdList = null;
        private ROI lineScanSelectedRoi = null;
        private ROI lineScanBgRoi = null;
        private double[][] lineScanFitParams = null;
        private double[][] lineScanFixedBeta = null;
        private double[][] lineScanGlobalDecay = null;
        private double[][] lineScanX = null;
        private int[] lineScanTStart = null;
        private int[] lineScanTEnd = null;
        private int lineScanFitMode = 1;
        private int lineScanGlobalPixelCount = 0;
        private bool lineScanFixTau1 = false;
        private bool lineScanFixTau2 = false;
        private bool lineScanFixTauG = false;
        private bool lineScanFixT0 = false;
        private bool lineScanFitWithBaseline = true;

        private struct RoiBounds
        {
            public int XStart;
            public int XEnd;
            public int YStart;
            public int YEnd;
            public bool Valid;
            public int Width => XEnd - XStart;
            public bool ContainsY(int y) => y >= YStart && y < YEnd;
        }

        public TimeCourse()
        {
            ImInfos = new List<ImageInfo>();
            UniqueIDs = new HashSet<int>();
        }

        private static bool Uses5DTimePages(FLIMData flim)
        {
            if (flim == null || flim.n_pages5D <= 0)
                return false;

            if (flim.nFastZ > 1)
                return true;

            if (flim.ZStack)
                return flim.n_pages5D > 1 || HasMultiSlice5DPage(flim);

            return flim.n_pages5D > 1 && flim.FLIM_Pages5D != null;
        }

        private static bool HasMultiSlice5DPage(FLIMData flim)
        {
            if (flim?.FLIM_Pages5D == null)
                return false;

            int pageCount = Math.Min(flim.n_pages5D, flim.FLIM_Pages5D.Length);
            for (int page = 0; page < pageCount; page++)
            {
                ushort[][][,,] page5D = flim.FLIM_Pages5D[page];
                if (page5D == null)
                    continue;

                for (int ch = 0; ch < page5D.Length; ch++)
                {
                    if (page5D[ch] != null && page5D[ch].Length > 1)
                        return true;
                }
            }

            return false;
        }

        private static int GetTimeCoursePageCount(FLIMData flim, bool use5DTimePages)
        {
            if (flim == null)
                return 0;

            if (use5DTimePages)
                return Math.Max(1, flim.n_pages5D);

            if (!flim.ZStack)
                return Math.Max(1, flim.n_pages);

            return 1;
        }

        private static double[] GetTimeMilliseconds(FLIMData flim, int nData, bool use5DTimePages)
        {
            double[] times = new double[Math.Max(0, nData)];
            if (flim == null || times.Length == 0)
                return times;

            DateTime[] acquiredTimes = use5DTimePages ? flim.acquiredTime_Pages5D : flim.acquiredTime_Pages;
            double baseMs = (flim.acquiredTime - ImageInfo.startDate).TotalMilliseconds;
            if (baseMs <= 0)
                baseMs = 0;

            double intervalMs = 0;
            try
            {
                intervalMs = flim.State.Acq.frameInterval() * 1000.0;
                if (flim.State.Acq.aveFrame)
                    intervalMs = intervalMs * flim.State.Acq.nAveFrame;
            }
            catch { }

            for (int i = 0; i < times.Length; i++)
            {
                double timeMs = 0;
                if (acquiredTimes != null && acquiredTimes.Length > i)
                    timeMs = (acquiredTimes[i] - ImageInfo.startDate).TotalMilliseconds;

                if (timeMs <= 0)
                    timeMs = baseMs + i * intervalMs;

                times[i] = timeMs;
            }

            return times;
        }

        public void InitializeLineScan(FLIMData flim, int totalTimePoints)
        {
            ImInfos = new List<ImageInfo>();
            UniqueIDs = new HashSet<int>();

            FileName = flim.fileName;
            BaseName = flim.baseName;
            FileNumber = flim.fileCounter;
            nChannels = flim.nChannels;

            lineScanSelectedRoi = flim.Roi;
            lineScanBgRoi = flim.bgRoi;
            lineScanRois = new List<ROI>();

            foreach (var roi in flim.ROIs)
            {
                if (roi == null || roi.IsLineScanTrace)
                    continue;
                if (roi.ROI_type == ROI.ROItype.PolyLine)
                    continue;
                lineScanRois.Add(roi);
                UniqueIDs.Add(roi.ID);
            }

            lineScanRoiIdList = UniqueIDs.ToList();
            nROI = UniqueIDs.Count;
            nTotalROIs = nROI;

            nData = Math.Max(0, totalTimePoints);
            time_milliseconds = new double[nData];
            time_seconds = new double[nData];

            foreach (var param in paramNames)
                SetupArrayEach(param, nData);

            lineScanFitParams = GetFitParams(flim.State, nChannels);
            lineScanFitMode = flim?.RoiFit?.flim_parameters?.n_exponentials > 0
                ? flim.RoiFit.flim_parameters.n_exponentials
                : 1;
            lineScanFixedBeta = null;
            lineScanGlobalDecay = null;
            lineScanX = null;
            lineScanTStart = null;
            lineScanTEnd = null;
            lineScanGlobalPixelCount = 0;
            fitting_param = new double[nChannels][][];
            for (int ch = 0; ch < nChannels; ch++)
                fitting_param[ch] = new double[nData][];
        }

        public void AddLineScanFrame(FLIMData flim, ushort[][,,] frameData, int startLineIndex, int maxLines, double baseTimeMs, double lineTimeMs, int startLineInFrame = 0)
        {
            if (frameData == null || frameData.Length == 0)
                return;
            if (startLineIndex < 0 || startLineIndex >= nData)
                return;

            ushort[,,] firstArr = null;
            for (int ch = 0; ch < frameData.Length; ch++)
            {
                if (frameData[ch] != null)
                {
                    firstArr = frameData[ch];
                    break;
                }
            }
            if (firstArr == null)
                return;

            int nLinesFrame = firstArr.GetLength(0);
            int startLine = Math.Max(0, startLineInFrame);
            if (startLine >= nLinesFrame)
                return;
            int width = firstArr.GetLength(1);
            int linesToUse = Math.Min(Math.Min(nLinesFrame - startLine, maxLines), nData - startLineIndex);

            double baseMs = baseTimeMs;
            if (baseMs <= 0 && lineTimeMs > 0)
                baseMs = startLineIndex * lineTimeMs;

            var selectedBounds = GetRoiBounds(lineScanSelectedRoi, width, nLinesFrame);
            var bgBounds = GetRoiBounds(lineScanBgRoi, width, nLinesFrame);
            RoiBounds[] roiBounds = null;
            if (nROI > 0 && lineScanRois != null && lineScanRois.Count > 0)
                roiBounds = lineScanRois.Select(r => GetRoiBounds(r, width, nLinesFrame)).ToArray();

            int[] tStart = new int[nChannels];
            int[] tEnd = new int[nChannels];
            double[] res_ps = new double[nChannels];
            double[] offset = new double[nChannels];
            double[] syncRate = new double[nChannels];

            for (int ch = 0; ch < nChannels; ch++)
            {
                var arr = frameData[ch];
                if (arr == null)
                    continue;

                int nT = arr.GetLength(2);
                int t0 = 0;
                int t1 = nT;
                if (lineScanTStart != null && ch < lineScanTStart.Length && lineScanTEnd != null && ch < lineScanTEnd.Length)
                {
                    t0 = lineScanTStart[ch];
                    t1 = lineScanTEnd[ch];
                }
                else
                {
                    int[] range = flim.fit_range != null && ch < flim.fit_range.Length ? flim.fit_range[ch] : null;
                    if (range != null && range.Length >= 2)
                    {
                        t0 = Math.Max(0, Math.Min(nT, range[0]));
                        t1 = Math.Max(t0, Math.Min(nT, range[1]));
                    }
                }

                tStart[ch] = t0;
                tEnd[ch] = t1;
                double res = 250.0;
                try
                {
                    if (flim.State?.Spc?.spcData?.resolution != null && flim.State.Spc.spcData.resolution.Length > ch)
                        res = flim.State.Spc.spcData.resolution[ch];
                }
                catch { }
                res_ps[ch] = res;
                offset[ch] = (flim.offset != null && flim.offset.Length > ch) ? flim.offset[ch] : 0.0;

                double sr = 80.0e6;
                try
                {
                    if (flim.State?.Spc?.datainfo?.syncRate != null && flim.State.Spc.datainfo.syncRate.Length > ch)
                        sr = flim.State.Spc.datainfo.syncRate[ch];
                    else if (flim.State?.Spc?.datainfo?.syncRate != null && flim.State.Spc.datainfo.syncRate.Length > 0)
                        sr = flim.State.Spc.datainfo.syncRate[0];
                }
                catch { }
                syncRate[ch] = sr;
            }

            for (int y = 0; y < linesToUse; y++)
            {
                int idx = startLineIndex + y;
                int lineInFrame = startLine + y;
                time_milliseconds[idx] = baseMs + y * lineTimeMs;
                if (time_seconds != null && time_seconds.Length > idx && time_milliseconds.Length > 0)
                    time_seconds[idx] = time_milliseconds[idx] / 1000.0 - time_milliseconds[0] / 1000.0;

                for (int ch = 0; ch < nChannels; ch++)
                {
                    var arr = frameData[ch];
                    if (arr == null)
                        continue;

                    if (lineScanFitParams != null && lineScanFitParams.Length > ch && lineScanFitParams[ch] != null)
                        fitting_param[ch][idx] = (double[])lineScanFitParams[ch].Clone();

                    double bgMean = 0.0;
                    if (bgBounds.Valid && bgBounds.ContainsY(lineInFrame) && bgBounds.Width > 0)
                    {
                        ComputeLineMetrics(arr, lineInFrame, bgBounds, tStart[ch], tEnd[ch], res_ps[ch], out double bgSum, out _);
                        bgMean = bgSum / bgBounds.Width;
                    }

                    if (selectedBounds.Valid && selectedBounds.ContainsY(lineInFrame) && selectedBounds.Width > 0)
                    {
                        ComputeLineMetrics(arr, lineInFrame, selectedBounds, tStart[ch], tEnd[ch], res_ps[ch], out double sum, out double wsum_ps);
                        double mean = sum / selectedBounds.Width;
                        double tau = (sum > 0.0) ? (wsum_ps / sum / 1000.0 - offset[ch]) : 0.0;
                        double fitTau = tau;
                        double fitFrac = CalcFraction2(ch, tau);

                        if (HasLineScanFixedFit())
                        {
                            double[] x = lineScanX != null && ch < lineScanX.Length ? lineScanX[ch] : null;
                            if (x == null || x.Length != (tEnd[ch] - tStart[ch]))
                            {
                                int nPoints = Math.Max(0, tEnd[ch] - tStart[ch]);
                                x = new double[nPoints];
                                for (int i = 0; i < nPoints; i++)
                                    x[i] = tStart[ch] + i;
                            }

                            double[] decay = BuildLineDecay(arr, lineInFrame, selectedBounds, tStart[ch], tEnd[ch]);
                            if (decay != null && TryFitLineDecayFixed(x, decay, ch, res_ps[ch], syncRate[ch], out double tauFit, out double fracFit))
                            {
                                fitTau = tauFit;
                                fitFrac = fracFit;
                            }
                        }

                        meanIntensity[ch][idx] = mean;
                        sumIntensity[ch][idx] = sum;
                        meanIntensity_bg[ch][idx] = mean - bgMean;
                        sumIntensity_bg[ch][idx] = meanIntensity_bg[ch][idx] * selectedBounds.Width;
                        nPixels[ch][idx] = selectedBounds.Width;
                        Lifetime[ch][idx] = tau;
                        Lifetime_fit[ch][idx] = fitTau;
                        Fraction2[ch][idx] = CalcFraction2(ch, tau);
                        Fraction2_fit[ch][idx] = fitFrac;
                    }
                    else
                    {
                        meanIntensity[ch][idx] = 0.0;
                        sumIntensity[ch][idx] = 0.0;
                        meanIntensity_bg[ch][idx] = 0.0;
                        sumIntensity_bg[ch][idx] = 0.0;
                        nPixels[ch][idx] = 0.0;
                        Lifetime[ch][idx] = 0.0;
                        Lifetime_fit[ch][idx] = 0.0;
                        Fraction2[ch][idx] = 0.0;
                        Fraction2_fit[ch][idx] = 0.0;
                    }

                    if (nROI > 0 && roiBounds != null && lineScanRoiIdList != null)
                    {
                        for (int r = 0; r < roiBounds.Length; r++)
                        {
                            int roiID = lineScanRois[r].ID;
                            int roiIndex = lineScanRoiIdList.IndexOf(roiID);
                            if (roiIndex < 0)
                                continue;

                            var bounds = roiBounds[r];
                            if (bounds.Valid && bounds.ContainsY(lineInFrame) && bounds.Width > 0)
                            {
                                ComputeLineMetrics(arr, lineInFrame, bounds, tStart[ch], tEnd[ch], res_ps[ch], out double sum, out double wsum_ps);
                                double mean = sum / bounds.Width;
                                double tau = (sum > 0.0) ? (wsum_ps / sum / 1000.0 - offset[ch]) : 0.0;
                                double fitTau = tau;
                                double fitFrac = CalcFraction2(ch, tau);

                                if (HasLineScanFixedFit())
                                {
                                    double[] x = lineScanX != null && ch < lineScanX.Length ? lineScanX[ch] : null;
                                    if (x == null || x.Length != (tEnd[ch] - tStart[ch]))
                                    {
                                        int nPoints = Math.Max(0, tEnd[ch] - tStart[ch]);
                                        x = new double[nPoints];
                                        for (int i = 0; i < nPoints; i++)
                                            x[i] = tStart[ch] + i;
                                    }

                                    double[] decay = BuildLineDecay(arr, lineInFrame, bounds, tStart[ch], tEnd[ch]);
                                    if (decay != null && TryFitLineDecayFixed(x, decay, ch, res_ps[ch], syncRate[ch], out double tauFit, out double fracFit))
                                    {
                                        fitTau = tauFit;
                                        fitFrac = fracFit;
                                    }
                                }

                                meanIntensity_ROI[ch, roiIndex][idx] = mean;
                                sumIntensity_ROI[ch, roiIndex][idx] = sum;
                                meanIntensity_bg_ROI[ch, roiIndex][idx] = mean - bgMean;
                                sumIntensity_bg_ROI[ch, roiIndex][idx] = meanIntensity_bg_ROI[ch, roiIndex][idx] * bounds.Width;
                                nPixels_ROI[ch, roiIndex][idx] = bounds.Width;
                                Lifetime_ROI[ch, roiIndex][idx] = tau;
                                Lifetime_fit_ROI[ch, roiIndex][idx] = fitTau;
                                Fraction2_ROI[ch, roiIndex][idx] = CalcFraction2(ch, tau);
                                Fraction2_fit_ROI[ch, roiIndex][idx] = fitFrac;
                            }
                            else
                            {
                                meanIntensity_ROI[ch, roiIndex][idx] = 0.0;
                                sumIntensity_ROI[ch, roiIndex][idx] = 0.0;
                                meanIntensity_bg_ROI[ch, roiIndex][idx] = 0.0;
                                sumIntensity_bg_ROI[ch, roiIndex][idx] = 0.0;
                                nPixels_ROI[ch, roiIndex][idx] = 0.0;
                                Lifetime_ROI[ch, roiIndex][idx] = 0.0;
                                Lifetime_fit_ROI[ch, roiIndex][idx] = 0.0;
                                Fraction2_ROI[ch, roiIndex][idx] = 0.0;
                                Fraction2_fit_ROI[ch, roiIndex][idx] = 0.0;
                            }
                        }
                    }
                }
            }
        }

        public void FinalizeLineScan()
        {
            if (time_milliseconds == null || time_milliseconds.Length == 0)
                return;

            for (int i = 0; i < time_milliseconds.Length; i++)
                time_seconds[i] = time_milliseconds[i] / 1000.0 - time_milliseconds[0] / 1000.0;
        }

        public void TrimLineScan(int usedPoints)
        {
            if (usedPoints < 0 || usedPoints == nData)
                return;
            if (usedPoints > nData)
                usedPoints = nData;

            nData = usedPoints;
            Array.Resize(ref time_milliseconds, nData);
            Array.Resize(ref time_seconds, nData);

            foreach (var param in paramNames)
            {
                var field = GetType().GetField(param);
                if (field != null)
                {
                    var arr = (double[][])field.GetValue(this);
                    if (arr != null)
                    {
                        for (int ch = 0; ch < arr.Length; ch++)
                            Array.Resize(ref arr[ch], nData);
                        field.SetValue(this, arr);
                    }
                }

                var fieldRoi = GetType().GetField(param + "_ROI");
                if (fieldRoi != null)
                {
                    var arr = (double[,][])fieldRoi.GetValue(this);
                    if (arr != null)
                    {
                        for (int ch = 0; ch < arr.GetLength(0); ch++)
                            for (int roi = 0; roi < arr.GetLength(1); roi++)
                                if (arr[ch, roi] != null)
                                    Array.Resize(ref arr[ch, roi], nData);
                        fieldRoi.SetValue(this, arr);
                    }
                }
            }

            if (fitting_param != null)
            {
                for (int ch = 0; ch < fitting_param.Length; ch++)
                    Array.Resize(ref fitting_param[ch], nData);
            }
        }

        private static RoiBounds GetRoiBounds(ROI roi, int width, int height)
        {
            var bounds = new RoiBounds { Valid = false };
            if (roi == null)
                return bounds;

            int xStart = Math.Max(0, (int)Math.Floor(roi.Rect.Left));
            int xEnd = Math.Min(width, (int)Math.Ceiling(roi.Rect.Right));
            int yStart = Math.Max(0, (int)Math.Floor(roi.Rect.Top));
            int yEnd = Math.Min(height, (int)Math.Ceiling(roi.Rect.Bottom));

            if (xEnd <= xStart || yEnd <= yStart)
                return bounds;

            bounds.XStart = xStart;
            bounds.XEnd = xEnd;
            bounds.YStart = yStart;
            bounds.YEnd = yEnd;
            bounds.Valid = true;
            return bounds;
        }

        private static void ComputeLineMetrics(ushort[,,] arr, int y, RoiBounds bounds, int tStart, int tEnd, double res_ps, out double sum, out double wsum_ps)
        {
            sum = 0.0;
            wsum_ps = 0.0;

            for (int x = bounds.XStart; x < bounds.XEnd; x++)
            {
                for (int t = tStart; t < tEnd; t++)
                {
                    ushort v = arr[y, x, t];
                    sum += v;
                    wsum_ps += v * (t * res_ps);
                }
            }
        }

        private static double[] BuildLineDecay(ushort[,,] arr, int y, RoiBounds bounds, int tStart, int tEnd)
        {
            int nPoints = tEnd - tStart;
            if (nPoints <= 0)
                return null;

            double[] decay = new double[nPoints];
            for (int x = bounds.XStart; x < bounds.XEnd; x++)
            {
                for (int t = tStart; t < tEnd; t++)
                    decay[t - tStart] += arr[y, x, t];
            }
            return decay;
        }

        public void PrepareLineScanFit(FLIMData flim, bool fixTau1, bool fixTau2, bool fixTauG, bool fixT0, bool fitWithBaseline)
        {
            if (flim == null)
                return;

            lineScanFixTau1 = fixTau1;
            lineScanFixTau2 = fixTau2;
            lineScanFixTauG = fixTauG;
            lineScanFixT0 = fixT0;
            lineScanFitWithBaseline = fitWithBaseline;

            lineScanFitMode = flim?.RoiFit?.flim_parameters?.n_exponentials > 0
                ? flim.RoiFit.flim_parameters.n_exponentials
                : 1;

            lineScanTStart = new int[nChannels];
            lineScanTEnd = new int[nChannels];
            lineScanX = new double[nChannels][];
            lineScanGlobalDecay = new double[nChannels][];
            lineScanFixedBeta = new double[nChannels][];
            lineScanGlobalPixelCount = 0;

            for (int ch = 0; ch < nChannels; ch++)
            {
                var arr = flim.FLIMRaw != null && ch < flim.FLIMRaw.Length ? flim.FLIMRaw[ch] : null;
                int nT = arr != null ? arr.GetLength(2) : 0;
                int t0 = 0;
                int t1 = nT;
                int[] range = flim.fit_range != null && ch < flim.fit_range.Length ? flim.fit_range[ch] : null;
                if (range != null && range.Length >= 2)
                {
                    t0 = Math.Max(0, Math.Min(nT, range[0]));
                    t1 = Math.Max(t0, Math.Min(nT, range[1]));
                }

                lineScanTStart[ch] = t0;
                lineScanTEnd[ch] = t1;
                int nPoints = Math.Max(0, t1 - t0);
                lineScanX[ch] = new double[nPoints];
                for (int i = 0; i < nPoints; i++)
                    lineScanX[ch][i] = t0 + i;
                lineScanGlobalDecay[ch] = new double[nPoints];
            }
        }

        public void AccumulateLineScanGlobalDecay(ushort[][,,] frameData, int maxLines)
        {
            if (frameData == null || frameData.Length == 0 || lineScanGlobalDecay == null)
                return;

            ushort[,,] firstArr = null;
            for (int ch = 0; ch < frameData.Length; ch++)
            {
                if (frameData[ch] != null)
                {
                    firstArr = frameData[ch];
                    break;
                }
            }
            if (firstArr == null)
                return;

            int nLinesFrame = firstArr.GetLength(0);
            int width = firstArr.GetLength(1);
            int linesToUse = Math.Min(nLinesFrame, maxLines);

            var selectedBounds = GetRoiBounds(lineScanSelectedRoi, width, nLinesFrame);
            if (!selectedBounds.Valid || selectedBounds.Width <= 0)
                return;

            for (int y = 0; y < linesToUse; y++)
            {
                if (!selectedBounds.ContainsY(y))
                    continue;

                lineScanGlobalPixelCount += selectedBounds.Width;

                for (int ch = 0; ch < nChannels; ch++)
                {
                    var arr = frameData[ch];
                    if (arr == null)
                        continue;

                    int tStart = lineScanTStart != null && ch < lineScanTStart.Length ? lineScanTStart[ch] : 0;
                    int tEnd = lineScanTEnd != null && ch < lineScanTEnd.Length ? lineScanTEnd[ch] : arr.GetLength(2);
                    if (tEnd <= tStart || lineScanGlobalDecay[ch] == null)
                        continue;

                    for (int x = selectedBounds.XStart; x < selectedBounds.XEnd; x++)
                    {
                        for (int t = tStart; t < tEnd; t++)
                            lineScanGlobalDecay[ch][t - tStart] += arr[y, x, t];
                    }
                }
            }
        }

        public void FinalizeLineScanFixedFit(FLIMData flim)
        {
            if (lineScanGlobalDecay == null || lineScanX == null || flim == null)
                return;

            double[] resPs = new double[nChannels];
            for (int ch = 0; ch < nChannels; ch++)
            {
                double res = 250.0;
                try
                {
                    if (flim.State?.Spc?.spcData?.resolution != null && flim.State.Spc.spcData.resolution.Length > ch)
                        res = flim.State.Spc.spcData.resolution[ch];
                }
                catch { }
                resPs[ch] = res;
            }

            for (int ch = 0; ch < nChannels; ch++)
            {
                double[] y = lineScanGlobalDecay[ch];
                double[] x = lineScanX[ch];
                if (y == null || x == null || y.Length < 2 || y.Sum() <= 0.0)
                    continue;

                int p = lineScanFitMode == 1 ? 5 : 7;
                double[] beta0 = BuildInitialBetaFromDecay(x, y, lineScanFitMode, resPs[ch]);
                if (beta0 == null || beta0.Length != p)
                    continue;

                var fit = new Fitting.Nlinfit(beta0, x, y);
                fit.fix = new bool[p];
                ApplyLineScanFixedParams(ch, resPs[ch], beta0, fit.fix);

                double res0 = resPs[ch];
                double syncRateHz = 80.0e6;
                try
                {
                    if (flim.State?.Spc?.datainfo?.syncRate != null && flim.State.Spc.datainfo.syncRate.Length > ch)
                        syncRateHz = flim.State.Spc.datainfo.syncRate[ch];
                    else if (flim.State?.Spc?.datainfo?.syncRate != null && flim.State.Spc.datainfo.syncRate.Length > 0)
                        syncRateHz = flim.State.Spc.datainfo.syncRate[0];
                }
                catch { }
                double pulseI = 1.0e12 / syncRateHz / res0;

                double tauG_Max = 1000;
                double tauG_Min = 10;
                double maxTau = 100000;
                double minTau = 10;

                double rateMin = res0 / maxTau;
                double rateMax = res0 / minTau;
                double tauGMinBin = tauG_Min / res0;
                double tauGMaxBin = tauG_Max / res0;

                if (lineScanFitMode == 1)
                {
                    fit.modelFunc = ((betaA, xA) => ImageProcessing.ExpGaussArray(betaA, xA, pulseI));
                    fit.modelFuncInPlace = ((betaA, xA, yA) => ImageProcessing.ExpGaussArrayInPlace(betaA, xA, pulseI, yA));
                    fit.jacobianFuncInPlace = ((betaA, xA, jtA) => ImageProcessing.ExpGaussJacobianInPlace(betaA, xA, pulseI, jtA));
                    fit.betaMin[0] = 0;
                    fit.betaMin[1] = rateMin;
                    fit.betaMax[1] = rateMax;
                    fit.betaMin[2] = tauGMinBin;
                    fit.betaMax[2] = tauGMaxBin;
                    fit.betaMin[4] = 0;
                }
                else
                {
                    fit.modelFunc = ((betaA, xA) => ImageProcessing.Exp2GaussArray(betaA, xA, pulseI));
                    fit.modelFuncInPlace = ((betaA, xA, yA) => ImageProcessing.Exp2GaussArrayInPlace(betaA, xA, pulseI, yA));
                    fit.jacobianFuncInPlace = ((betaA, xA, jtA) => ImageProcessing.Exp2GaussJacobianInPlace(betaA, xA, pulseI, jtA));
                    fit.betaMin[0] = 0;
                    fit.betaMin[1] = rateMin;
                    fit.betaMax[1] = rateMax;
                    fit.betaMin[2] = 0;
                    fit.betaMin[3] = rateMin;
                    fit.betaMax[3] = rateMax;
                    fit.betaMin[4] = tauGMinBin;
                    fit.betaMax[4] = tauGMaxBin;
                    fit.betaMin[6] = 0;
                }

                for (int bi = 0; bi < beta0.Length; bi++)
                {
                    double mn = fit.betaMin[bi];
                    double mx = fit.betaMax[bi];
                    if (!Double.IsInfinity(mn) && beta0[bi] < mn)
                        beta0[bi] = mn;
                    if (!Double.IsInfinity(mx) && beta0[bi] > mx)
                        beta0[bi] = mx;
                }

                fit.PoissonMaximumLikelihood();
                try
                {
                    fit.Perform();
                    lineScanFixedBeta[ch] = (double[])fit.beta.Clone();
                }
                catch
                {
                    lineScanFixedBeta[ch] = null;
                }
            }
        }

        public bool HasLineScanFixedFit()
        {
            if (lineScanFixedBeta == null)
                return false;
            for (int ch = 0; ch < lineScanFixedBeta.Length; ch++)
            {
                if (lineScanFixedBeta[ch] != null && lineScanFixedBeta[ch].Length >= 5)
                    return true;
            }
            return false;
        }

        public void ClearLineScanFixedFit()
        {
            lineScanFixedBeta = null;
            lineScanGlobalDecay = null;
            lineScanX = null;
            lineScanTStart = null;
            lineScanTEnd = null;
            lineScanGlobalPixelCount = 0;
            lineScanFixTau1 = false;
            lineScanFixTau2 = false;
            lineScanFixTauG = false;
            lineScanFixT0 = false;
            lineScanFitWithBaseline = true;
        }

        private static double[] BuildInitialBetaFromDecay(double[] x, double[] y, int mode, double resPs)
        {
            if (x == null || y == null || x.Length == 0 || y.Length == 0)
                return null;

            double maxY = y.Max();
            if (maxY <= 0)
                return null;

            int indx = Array.IndexOf(y, maxY);
            double maxX = indx >= 0 && indx < x.Length ? x[indx] : x[0];
            double sum = y.Sum();
            if (sum <= 0)
                return null;

            double sumX = 0;
            for (int i = 0; i < y.Length; i++)
                sumX += y[i] * x[i];

            double tau1_bin = sum / maxY;
            double tauG = 100.0 / resPs;

            if (mode == 1)
            {
                var beta1 = new double[5];
                beta1[0] = maxY * (1 + tauG / tau1_bin);
                beta1[1] = 1 / tau1_bin;
                beta1[2] = tauG;
                beta1[3] = maxX - 2 * tauG;
                beta1[4] = 0;
                return beta1;
            }

            var beta2 = new double[7];
            beta2[0] = maxY / 2;
            beta2[1] = 1 / tau1_bin * 0.25;
            beta2[2] = maxY / 2;
            beta2[3] = 1 / tau1_bin * 2;
            beta2[4] = tauG;
            beta2[5] = maxX - 1 * tauG;
            beta2[6] = 0;
            return beta2;
        }

        private bool TryFitLineDecayFixed(double[] x, double[] y, int ch, double resPs, double syncRateHz, out double lifetimeFit, out double fraction2Fit)
        {
            lifetimeFit = 0.0;
            fraction2Fit = 0.0;

            if (x == null || y == null || y.Length < 2 || lineScanFixedBeta == null || ch >= lineScanFixedBeta.Length)
                return false;
            if (lineScanFixedBeta[ch] == null || lineScanFixedBeta[ch].Length < 5)
                return false;
            if (y.Sum() <= 0.0)
                return false;

            int p = lineScanFitMode == 1 ? 5 : 7;
            if (lineScanFixedBeta[ch].Length < p)
                return false;

            double[] beta0 = (double[])lineScanFixedBeta[ch].Clone();
            bool[] fix = Enumerable.Repeat(true, p).ToArray();

            double maxY = y.Max();
            if (lineScanFitMode == 1)
            {
                beta0[0] = Math.Max(maxY, 1.0);
                fix[0] = false;
                fix[4] = !lineScanFitWithBaseline;
            }
            else
            {
                double denom = beta0[0] + beta0[2];
                double ratio = denom > 0.0 ? maxY / denom : 0.0;
                if (ratio > 0.0)
                {
                    beta0[0] = beta0[0] * ratio;
                    beta0[2] = beta0[2] * ratio;
                }
                else
                {
                    beta0[0] = maxY / 2.0;
                    beta0[2] = maxY / 2.0;
                }
                fix[0] = false;
                fix[2] = false;
                fix[6] = !lineScanFitWithBaseline;
            }

            var fit = new Fitting.Nlinfit(beta0, x, y);
            fit.fix = fix;

            if (syncRateHz <= 0.0)
                syncRateHz = 80.0e6;
            double pulseI = 1.0e12 / syncRateHz / resPs;
            try
            {
                if (lineScanFitMode == 1)
                {
                    fit.modelFunc = ((betaA, xA) => ImageProcessing.ExpGaussArray(betaA, xA, pulseI));
                    fit.modelFuncInPlace = ((betaA, xA, yA) => ImageProcessing.ExpGaussArrayInPlace(betaA, xA, pulseI, yA));
                    fit.jacobianFuncInPlace = ((betaA, xA, jtA) => ImageProcessing.ExpGaussJacobianInPlace(betaA, xA, pulseI, jtA));
                }
                else
                {
                    fit.modelFunc = ((betaA, xA) => ImageProcessing.Exp2GaussArray(betaA, xA, pulseI));
                    fit.modelFuncInPlace = ((betaA, xA, yA) => ImageProcessing.Exp2GaussArrayInPlace(betaA, xA, pulseI, yA));
                    fit.jacobianFuncInPlace = ((betaA, xA, jtA) => ImageProcessing.Exp2GaussJacobianInPlace(betaA, xA, pulseI, jtA));
                }
            }
            catch
            {
                return false;
            }

            double tauG_Max = 1000;
            double tauG_Min = 10;
            double maxTau = 100000;
            double minTau = 10;

            double rateMin = resPs / maxTau;
            double rateMax = resPs / minTau;
            double tauGMinBin = tauG_Min / resPs;
            double tauGMaxBin = tauG_Max / resPs;

            if (lineScanFitMode == 1)
            {
                fit.betaMin[0] = 0;
                fit.betaMin[1] = rateMin;
                fit.betaMax[1] = rateMax;
                fit.betaMin[2] = tauGMinBin;
                fit.betaMax[2] = tauGMaxBin;
                fit.betaMin[4] = 0;
            }
            else
            {
                fit.betaMin[0] = 0;
                fit.betaMin[1] = rateMin;
                fit.betaMax[1] = rateMax;
                fit.betaMin[2] = 0;
                fit.betaMin[3] = rateMin;
                fit.betaMax[3] = rateMax;
                fit.betaMin[4] = tauGMinBin;
                fit.betaMax[4] = tauGMaxBin;
                fit.betaMin[6] = 0;
            }

            for (int bi = 0; bi < beta0.Length; bi++)
            {
                double mn = fit.betaMin[bi];
                double mx = fit.betaMax[bi];
                if (!Double.IsInfinity(mn) && beta0[bi] < mn)
                    beta0[bi] = mn;
                if (!Double.IsInfinity(mx) && beta0[bi] > mx)
                    beta0[bi] = mx;
            }

            fit.PoissonMaximumLikelihood();
            try
            {
                fit.Perform();
            }
            catch
            {
                return false;
            }

            double[] b = fit.beta;
            double res = resPs / 1000.0;
            if (lineScanFitMode == 1)
            {
                lifetimeFit = 1.0 / b[1] * res;
                fraction2Fit = 0.0;
            }
            else
            {
                lifetimeFit = (b[0] / b[1] / b[1] + b[2] / b[3] / b[3]) / (b[0] / b[1] + b[2] / b[3]);
                lifetimeFit *= res;
                double denom = b[0] + b[2];
                fraction2Fit = denom > 0.0 ? b[2] / denom : 0.0;
            }

            return true;
        }

        private double CalcFraction2(int ch, double tau_m0)
        {
            if (lineScanFitParams == null || lineScanFitParams.Length <= ch || lineScanFitParams[ch] == null)
                return 0.0;

            double tauD = lineScanFitParams[ch].Length > 1 ? lineScanFitParams[ch][1] : 0.0;
            double tauAD = lineScanFitParams[ch].Length > 3 ? lineScanFitParams[ch][3] : 0.0;
            double denom = (tauD - tauAD) * (tauD + tauAD - tau_m0);
            if (denom == 0.0)
                return 0.0;
            return tauD * (tauD - tau_m0) / denom;
        }

        private static double[][] GetFitParams(ScanParameters state, int nChannels)
        {
            var result = new double[nChannels][];
            for (int ch = 0; ch < nChannels; ch++)
            {
                var field = state?.Spc?.analysis?.GetType().GetField("fit_param" + (ch + 1));
                if (field != null)
                {
                    var val = field.GetValue(state.Spc.analysis) as double[];
                    result[ch] = val != null ? (double[])val.Clone() : new double[0];
                }
                else
                {
                    result[ch] = new double[0];
                }
            }
            return result;
        }

        private void ApplyLineScanFixedParams(int ch, double resPs, double[] beta0, bool[] fix)
        {
            if (beta0 == null || fix == null)
                return;

            if (lineScanFitMode == 1)
            {
                if (fix.Length >= 5)
                {
                    fix[1] = lineScanFixTau1;
                    fix[2] = lineScanFixTauG;
                    fix[3] = lineScanFixT0;
                    fix[4] = !lineScanFitWithBaseline;
                }
            }
            else
            {
                if (fix.Length >= 7)
                {
                    fix[1] = lineScanFixTau1;
                    fix[3] = lineScanFixTau2;
                    fix[4] = lineScanFixTauG;
                    fix[5] = lineScanFixT0;
                    fix[6] = !lineScanFitWithBaseline;
                }
            }

            if (lineScanFitParams == null || lineScanFitParams.Length <= ch || lineScanFitParams[ch] == null)
                return;

            var param = lineScanFitParams[ch];
            if (param.Length < 6)
                return;

            double resNs = resPs / 1000.0;
            if (resNs <= 0)
                return;

            if (lineScanFitMode == 1)
            {
                if (lineScanFixTau1 && param[1] > 0)
                    beta0[1] = resNs / param[1];
                if (lineScanFixTauG)
                    beta0[2] = param[4] / resNs;
                if (lineScanFixT0)
                    beta0[3] = param[5] / resNs;
            }
            else
            {
                if (lineScanFixTau1 && param[1] > 0)
                    beta0[1] = resNs / param[1];
                if (lineScanFixTau2 && param[3] > 0)
                    beta0[3] = resNs / param[3];
                if (lineScanFixTauG)
                    beta0[4] = param[4] / resNs;
                if (lineScanFixT0)
                    beta0[5] = param[5] / resNs;
            }
        }

        public void AddFLIM(FLIMData flim, int num)
        {
            bool use5DTimePages = Uses5DTimePages(flim);

            if (flim.Roi.flim_parameters_Pages == null)
            {
                ImageInfo im = new ImageInfo(flim);
                AddFile(im, num);
                return;
            }

            UniqueIDs = new HashSet<int>(); //If you Clear(), it will affect previous TC.

            if (flim.ROIs.Count > 0)
                foreach (var roi in flim.ROIs)
                    UniqueIDs.Add(roi.ID);

            nROI = flim.ROIs.Count;

            nTotalROIs = 0;
            for (var i = 0; i < nROI; i++)
            {
                var roi = flim.ROIs[i];
                nTotalROIs += 1;
                if (roi.ROI_type == ROI.ROItype.PolyLine)
                {
                    for (var j = 0; j < roi.polyLineROIs.Count; j++)
                    {
                        nTotalROIs += 1;
                        UniqueIDs.Add((roi.ID + 1) * 1000 + j);
                    }
                }
            }

            FileName = flim.fileName;
            BaseName = flim.baseName;
            FileNumber = flim.fileCounter;
            nChannels = flim.nChannels;

            nData = GetTimeCoursePageCount(flim, use5DTimePages);

            time_milliseconds = GetTimeMilliseconds(flim, nData, use5DTimePages);
            time_seconds = new double[time_milliseconds.Length];
            time_seconds = time_milliseconds.Select(x => (x - time_milliseconds[0]) / 1000.0).ToArray();
            //for (int i = 0; i < time_milliseconds.Length; i++)
            //{
            //    time_milliseconds[i] = time_milliseconds[i] >= time_milliseconds[0] ? time_milliseconds[i] : time_milliseconds[i - 1];
            //    time_seconds[i] = (time_milliseconds[i] - time_milliseconds[0]) / 1000.0;
            //}

            if (!use5DTimePages && !flim.ZStack)
            {
                nData = flim.n_pages;
            }

            foreach (var param in paramNames)
                SetupArrayEach(param, nData);

            double res = flim.State.Spc.spcData.resolution[0] / 1000;

            var param_roi = flim.Roi.flim_parameters_Pages;
            var param_bg = flim.bgRoi.flim_parameters_Pages;

            for (int ch = 0; ch < nChannels; ch++)
            {
                for (int i = 0; i < nData; i++)
                {
                    if (param_roi == null || param_roi.Length <= i || param_roi[i] == null)
                        continue;

                    nPixels[ch][i] = param_roi[i].nPixels[ch];
                    meanIntensity[ch][i] = param_roi[i].meanIntensity[ch];
                    sumIntensity[ch][i] = param_roi[i].sumIntensity[ch];
                    double bg = 0;
                    if (param_bg != null && param_bg.Length > i && param_bg[i] != null)
                        bg = param_bg[i].meanIntensity[ch];
                    meanIntensity_bg[ch][i] = meanIntensity[ch][i] - bg;
                    sumIntensity_bg[ch][i] = meanIntensity_bg[ch][i] * nPixels[ch][i];
                    Lifetime[ch][i] = param_roi[i].tau_m_fromMAP[ch];

                    ///Fit result.
                    Lifetime_fit[ch][i] = param_roi[i].tau_m[ch];

                    double tauD = res / param_roi[i].beta[ch][1];
                    double tauAD = res / param_roi[i].beta[ch][3];
                    double tau_m0 = Lifetime[ch][i];

                    Fraction2[ch][i] = tauD * (tauD - tau_m0) / (tauD - tauAD) / (tauD + tauAD - tau_m0);
                    double frac = param_roi[i].beta[ch][2] / (param_roi[i].beta[ch][0] + param_roi[i].beta[ch][2]);
                    Fraction2_fit[ch][i] = frac;
                }
            }

            int roi_indx = 0;
            for (int roi = 0; roi < nROI; roi++)
            {
                param_roi = flim.ROIs[roi].flim_parameters_Pages;
                for (int ch = 0; ch < nChannels; ch++)
                {
                    for (int i = 0; i < nData; i++)
                    {
                        if (param_roi == null || param_roi.Length <= i || param_roi[i] == null)
                            continue;

                        nPixels_ROI[ch, roi_indx][i] = param_roi[i].nPixels[ch];
                        meanIntensity_ROI[ch, roi_indx][i] = param_roi[i].meanIntensity[ch];
                        sumIntensity_ROI[ch, roi_indx][i] = param_roi[i].sumIntensity[ch];
                        double bg = 0;
                        if (param_bg != null && param_bg.Length > i && param_bg[i] != null)
                            bg = param_bg[i].meanIntensity[ch];
                        meanIntensity_bg_ROI[ch, roi_indx][i] = meanIntensity_ROI[ch, roi_indx][i] - bg;
                        sumIntensity_bg_ROI[ch, roi_indx][i] = meanIntensity_bg_ROI[ch, roi_indx][i] * nPixels_ROI[ch, roi_indx][i];
                        Lifetime_ROI[ch, roi_indx][i] = param_roi[i].tau_m_fromMAP[ch];
                        Lifetime_fit_ROI[ch, roi_indx][i] = param_roi[i].tau_m[ch];

                        double tauD = res / param_roi[i].beta[ch][1];
                        double tauAD = res / param_roi[i].beta[ch][3];
                        double tau_m0 = Lifetime_ROI[ch, roi_indx][i];

                        Fraction2_ROI[ch, roi_indx][i] = tauD * (tauD - tau_m0) / (tauD - tauAD) / (tauD + tauAD - tau_m0);
                        double frac = param_roi[i].beta[ch][2] / (param_roi[i].beta[ch][0] + param_roi[i].beta[ch][2]);
                        Fraction2_fit_ROI[ch, roi_indx][i] = frac;
                    }
                }
                roi_indx++;

                if (flim.ROIs[roi].ROI_type == ROI.ROItype.PolyLine)
                {
                    foreach (var poly_roi in flim.ROIs[roi].polyLineROIs)
                    {
                        param_roi = poly_roi.flim_parameters_Pages;
                        for (int ch = 0; ch < nChannels; ch++)
                        {
                            for (int i = 0; i < nData; i++)
                            {
                                if (param_roi == null || param_roi.Length <= i || param_roi[i] == null)
                                    continue;

                                nPixels_ROI[ch, roi_indx][i] = param_roi[i].nPixels[ch];
                                meanIntensity_ROI[ch, roi_indx][i] = param_roi[i].meanIntensity[ch];
                                sumIntensity_ROI[ch, roi_indx][i] = param_roi[i].sumIntensity[ch];
                                double bg = 0;
                                if (param_bg != null && param_bg.Length > i && param_bg[i] != null)
                                    bg = param_bg[i].meanIntensity[ch];
                                meanIntensity_bg_ROI[ch, roi_indx][i] = meanIntensity_ROI[ch, roi_indx][i] - bg;
                                sumIntensity_bg_ROI[ch, roi_indx][i] = meanIntensity_bg_ROI[ch, roi_indx][i] * nPixels_ROI[ch, roi_indx][i];
                                Lifetime_ROI[ch, roi_indx][i] = param_roi[i].tau_m_fromMAP[ch];
                                Lifetime_fit_ROI[ch, roi_indx][i] = param_roi[i].tau_m[ch];

                                double tauD = res / param_roi[i].beta[ch][1];
                                double tauAD = res / param_roi[i].beta[ch][3];
                                double tau_m0 = Lifetime_ROI[ch, roi_indx][i];

                                Fraction2_ROI[ch, roi_indx][i] = tauD * (tauD - tau_m0) / (tauD - tauAD) / (tauD + tauAD - tau_m0);
                                double frac = param_roi[i].beta[ch][2] / (param_roi[i].beta[ch][0] + param_roi[i].beta[ch][2]);
                                Fraction2_fit_ROI[ch, roi_indx][i] = frac;
                            }
                        }
                        roi_indx++;
                    }
                    //foreach (var roi1 in flim.ROIs[roi].polyLineROIs)
                    //{
                    //    getData(roi1, bgRoi);

                    //    roiID[i] = (ROI_Multi[roiN].ID) * 1000 + roi1.ID;

                    //    foreach (string fieldname in paramNames)
                    //    {
                    //        var arr1 = (double[][])GetType().GetField(fieldname + "_ROI").GetValue(this);
                    //        var arr2 = (double[])GetType().GetField(fieldname).GetValue(this);
                    //        arr1[roi_indx] = (double[])arr2.Clone();
                    //        GetType().GetField(fieldname + "_ROI").SetValue(this, arr1);
                    //    }                       
                    //}
                }
            }
        }


        /// <summary>
        /// Check if the file is new and then either add or integrate.
        /// </summary>
        /// <param name="iminfo"></param>
        /// <param name="num"></param>
        public bool AddFile(ImageInfo iminfo, int num)
        {
            bool success = true;
            if (iminfo.FileName != FileName)
            {
                //return false;
                ImInfos = new List<ImageInfo>();
                UniqueIDs = new HashSet<int>(); //If you Clear(), it will affect previous TC.
                //time_milliseconds = new double[] { iminfo.time_milliseconds };
            }

            if (ImInfos.Count > num && ImInfos.Count > 0) //Replacing existing.
            {
                ImInfos[num] = iminfo.DeepCopy();
                time_milliseconds[num] = iminfo.time_milliseconds;
            }
            else
            {
                if (time_milliseconds == null || time_milliseconds.Length == 0)
                    time_milliseconds = new double[num + 1];
                else
                    Array.Resize(ref time_milliseconds, num + 1);

                while (ImInfos.Count <= num)
                {
                    ImInfos.Add(iminfo.DeepCopy());
                    time_milliseconds[ImInfos.Count - 1] = iminfo.time_milliseconds; //putting place holder.
                }
            }

            if (iminfo.roiID != null)
                foreach (var id in iminfo.roiID)
                    UniqueIDs.Add(id);

            nROI = UniqueIDs.Count;

            nData = ImInfos.Count;

            FileName = iminfo.FileName;
            BaseName = iminfo.BaseName;
            FileNumber = iminfo.FileCounter;

            AddToTimeCourse(iminfo, num);
            return success;
        }

        public void SortByTime()
        {
            ImInfos = ImInfos.OrderBy(x => x.time_milliseconds).ToList();
            time_milliseconds = ImInfos.Select(x => x.time_milliseconds).ToArray();
            calculate();
        }

        private void SetupArrayEach(string fieldName, int n_data)
        {
            double[] values = null;

            var field = this.GetType().GetField(fieldName);
            if (field != null)
            {
                var arr = (double[][])field.GetValue(this);
                if (arr == null || arr.Length != nChannels)
                {
                    arr = new double[nChannels][];
                }
                for (int ch = 0; ch < nChannels; ch++)
                {
                    if (arr[ch] == null)
                        arr[ch] = new double[n_data];
                    else
                        Array.Resize(ref arr[ch], n_data);
                }
                field.SetValue(this, arr);
            }

            field = this.GetType().GetField(fieldName + "_ROI");
            if (field != null)
            {
                var arr = (double[,][])field.GetValue(this);
                if (arr == null || arr.GetLength(0) != nChannels || arr.GetLength(1) != nROI)
                {
                    arr = new double[nChannels, nTotalROIs][];
                }
                for (int roi = 0; roi < nTotalROIs; roi++)
                    for (int ch = 0; ch < nChannels; ch++)
                    {
                        if (arr[ch, roi] == null)
                            arr[ch, roi] = new double[n_data];
                        else
                            Array.Resize(ref arr[ch, roi], n_data);
                    }
                field.SetValue(this, arr);
            }

        }


        private void AddToTimeCourse_ByParams(int num)
        {
            if (fitting_param == null)
                fitting_param = new double[nChannels][][];

            for (int ch = 0; ch < nChannels; ch++)
            {
                Array.Resize(ref fitting_param[ch], nData);
                fitting_param[ch][num] = (double[])ImInfos[num].fitting_param[ch].Clone();
            }


            foreach (string fieldname in paramNames)
            {
                var arr1 = (double[][])GetType().GetField(fieldname).GetValue(this);
                var imImfoArr = (double[])ImInfos[num].GetType().GetField(fieldname).GetValue(ImInfos[num]);
                var roi_fieldname = fieldname + "_ROI";
                var arr_roi = (double[,][])GetType().GetField(roi_fieldname).GetValue(this);
                var imImfoArr_roi = (double[][])ImInfos[num].GetType().GetField(roi_fieldname).GetValue(ImInfos[num]);

                if (arr1 == null)
                    arr1 = new double[nChannels][];

                for (int ch = 0; ch < nChannels; ch++)
                {
                    if (arr1[ch] == null)
                        arr1[ch] = new double[nData];
                    else
                        Array.Resize(ref arr1[ch], nData);
                    arr1[ch][num] = imImfoArr[ch];
                }

                GetType().GetField(fieldname).SetValue(this, arr1);

                if (arr_roi == null)
                    arr_roi = new double[nChannels, nROI][];
                else if ((nROI > 0 && arr_roi.GetLength(1) != nROI) ||
                    (nChannels > 0 && arr_roi.GetLength(0) != nChannels))
                    MatrixCalc.ResizeArray2D(ref arr_roi, nChannels, nROI);

                for (int ch = 0; ch < nChannels; ch++)
                {
                    for (int roi = 0; roi < nROI; roi++)
                    {
                        if (arr_roi[ch, roi] == null)
                            arr_roi[ch, roi] = new double[nData];
                        else if (arr_roi[ch, roi].Length != nData)
                            Array.Resize(ref arr_roi[ch, roi], nData);
                    }

                    if (ImInfos[num].roiID != null)
                        for (int roi = 0; roi < ImInfos[num].roiID.Length; roi++)
                        {
                            int roiID0 = ImInfos[num].roiID[roi]; //getroiID for each ImInfo.
                            int roiID = UniqueIDs.ToList().IndexOf(roiID0);

                            if (imImfoArr_roi != null && imImfoArr_roi.Length > roi)
                                arr_roi[ch, roiID][num] = imImfoArr_roi[roi][ch];
                            else
                                arr_roi[ch, roiID][num] = 0;
                        }
                }

                GetType().GetField(roi_fieldname).SetValue(this, arr_roi);
            }
        }

        public double[] GetArrayByName(string parameterName, int Channel, int ROI)
        {
            double[] values = null;
            if (parameterName.Contains("_ROI") && ROI >= 0)
            {
                var field = this.GetType().GetField(parameterName);
                if (field != null)
                {
                    var arr_roi = (double[,][])field.GetValue(this);
                    if (arr_roi != null && Channel < arr_roi.GetLength(0) && ROI < arr_roi.GetLength(1))
                        values = arr_roi[Channel, ROI];
                }
            }
            else
            {
                var field = this.GetType().GetField(parameterName);
                if (field != null)
                {
                    var arr1 = (double[][])field.GetValue(this);
                    if (arr1 != null && Channel < arr1.Length)
                        values = arr1[Channel];
                }
            }
            return values;
        }


        public void SetArrayByName(string parameterName, int Channel, int roi, double[] values)
        {
            //var roi = UniqueIDs.ToList().IndexOf(roiID);

            if (parameterName.Contains("_ROI") && roi >= 0)
            {
                var field = this.GetType().GetField(parameterName);
                if (field != null)
                {
                    var arr_roi = (double[,][])field.GetValue(this);
                    if (arr_roi != null && Channel < arr_roi.GetLength(0) && roi < arr_roi.GetLength(1))
                    {
                        arr_roi[Channel, roi] = values;
                        field.SetValue(this, arr_roi);
                    }
                }
            }
            else
            {
                var field = this.GetType().GetField(parameterName);
                if (field != null)
                {
                    var arr1 = (double[][])field.GetValue(this);
                    if (arr1 != null && Channel < arr1.Length)
                    {
                        arr1[Channel] = values;
                        field.SetValue(this, arr1);
                    }
                }
            }
        }

        //Add to time course.
        private void AddToTimeCourse(ImageInfo imimfo, int num)
        {
            nChannels = ImInfos[0].nChannels;
            nROI = UniqueIDs.ToList().Count;

            if (time_seconds == null || (Lifetime_ROI == null && nROI > 0))
            {
                calculate();
            }
            else
            {
                Array.Resize(ref time_seconds, nData);
                time_milliseconds[num] = ImInfos[num].time_milliseconds;
                time_seconds[num] = time_milliseconds[num] / 1000.0 - ImInfos[0].time_milliseconds / 1000.0;
            }

            AddToTimeCourse_ByParams(num);
        }//Simple Add()

        public void initializeArray(int nCh, int nRois)
        {
            if (nChannels <= 0)
                return;

            fitting_param = new double[nChannels][][];

            nChannels = nCh;
            foreach (string fieldname in paramNames)
            {
                var arr1 = new double[nChannels][];
                GetType().GetField(fieldname).SetValue(this, arr1);

                double[,][] arr_roi;
                if (nRois > 0)
                    arr_roi = new double[nChannels, nRois][];
                else
                    arr_roi = null;

                GetType().GetField(fieldname + "_ROI").SetValue(this, arr_roi);
            }

        }

        public void ChangeROIarray(int nCh, int nRois)
        {
            if (nCh <= 0 && nRois <= 0)
                return;

            if (meanIntensity_ROI == null)
                initializeArray(nCh, nRois);

            if (nChannels != nCh || nTotalROIs != nRois)
            {
                nChannels = nCh;
                nTotalROIs = nRois;

                foreach (string fieldname in paramNames)
                {
                    var arr_roi = (double[,][])GetType().GetField(fieldname + "_ROI").GetValue(this);

                    MatrixCalc.ResizeArray2D(ref arr_roi, nChannels, nRois);
                    GetType().GetField(fieldname + "_ROI").SetValue(this, arr_roi);
                }
            }
        }


        public void calculate()
        {
            if (ImInfos.Count == 0)
                return;

            //time_milliseconds = new double[nData];
            time_seconds = new double[nData];
            nChannels = ImInfos[0].nChannels;

            initializeArray(nChannels, nROI);

            fitting_param = new double[nChannels][][];

            for (int i = 0; i < nData; i++)
            {
                time_milliseconds[i] = ImInfos[i].time_milliseconds;
                time_seconds[i] = time_milliseconds[i] / 1000.0 - time_milliseconds[0] / 1000.0;
            }

            for (int ch = 0; ch < nChannels; ch++)
            {
                fitting_param[ch] = new double[nData][];
                for (int j = 0; j < nData; j++)
                {
                    if (ImInfos.Count > 0 && ImInfos.Count > j)
                    {
                        if (ImInfos[j].fitting_param[ch] != null)
                            fitting_param[ch][j] = (double[])ImInfos[j].fitting_param[ch].Clone();
                    }
                }
            }

            foreach (string fieldname in paramNames)
            {
                var arr1 = (double[][])GetType().GetField(fieldname).GetValue(this);
                var arr_roi = (double[,][])GetType().GetField(fieldname + "_ROI").GetValue(this);

                for (int ch = 0; ch < nChannels; ch++)
                    arr1[ch] = new double[nData];

                if (nROI > 0)
                {
                    for (int ch = 0; ch < nChannels; ch++)
                    {
                        for (int roi = 0; roi < nROI; roi++)
                            arr_roi[ch, roi] = new double[nData];

                        for (int j = 0; j < nData; j++)
                        {
                            if (ImInfos.Count > 0 && ImInfos.Count > j)
                            {
                                var imImfoArr = (double[])ImInfos[j].GetType().GetField(fieldname).GetValue(ImInfos[j]);
                                arr1[ch][j] = imImfoArr[ch];

                                if (ImInfos[j].roiID != null)
                                {
                                    for (int roi = 0; roi < ImInfos[j].roiID.Length; roi++)
                                    {
                                        var imImfoArr_roi = (double[][])ImInfos[j].GetType().GetField(fieldname + "_ROI").GetValue(ImInfos[j]);

                                        int roiID0 = ImInfos[j].roiID[roi]; //getroiID for each ImInfo.
                                        int roi_num = UniqueIDs.ToList().IndexOf(roiID0); //roi number (0,1,2...) of roiID0 (could be any, like 5, 8, 7)

                                        if (imImfoArr_roi != null && imImfoArr_roi.Length > roi)
                                            arr_roi[ch, roi_num][j] = imImfoArr_roi[roi][ch];
                                    }
                                }
                            }
                        }

                    }
                }

                GetType().GetField(fieldname).SetValue(this, arr1);
                GetType().GetField(fieldname + "_ROI").SetValue(this, arr_roi);
            }

        }//calculate()

        public TimeCourse DeepCopy()
        {
            TimeCourse TC1 = new TimeCourse();
            Copier.DeepCopyClass(this, TC1);

            TC1.ImInfos = new List<ImageInfo>();
            foreach (var iminfo in ImInfos)
            {
                TC1.ImInfos.Add(iminfo.DeepCopy());
            }
            return TC1;
        }
    } //Time course


    /// <summary>
    /// Time course of files.
    /// </summary>
    public class TimeCourse_Files
    {
        public List<TimeCourse> TCF = new List<TimeCourse>();
        public HashSet<int> UniqueIDs = new HashSet<int>();
        public String FileName;
        public int currentFileNumber;
        public String BaseName = "";

        public int[] plotRange;
        public double[] time_milliseconds;
        public double[] time_seconds;
        public double[] fileNumber;
        public int[] roiID;

        public double[][] Lifetime;
        public double[][] Fraction2;
        public double[][] Lifetime_fit;
        public double[][] Fraction2_fit;

        public double[][] meanIntensity;
        public double[][] sumIntensity;
        public double[][] meanIntensity_bg;
        public double[][] sumIntensity_bg;

        public double[][] nPixels;

        public double[,][] Lifetime_ROI;
        public double[,][] Fraction2_ROI;
        public double[,][] Lifetime_fit_ROI;
        public double[,][] Fraction2_fit_ROI;

        public double[,][] meanIntensity_ROI;
        public double[,][] sumIntensity_ROI;
        public double[,][] meanIntensity_bg_ROI;
        public double[,][] sumIntensity_bg_ROI;

        public double[,][] nPixels_ROI;
        public int nROI = 0;
        public int nData = 0;
        public int nChannels = 1;
        double[][][] fitting_param;

        public string[] paramNames = (string[])ImageInfo.paramNames.Clone();

        public TimeCourse_Files()
        {
            currentFileNumber = 0;
        }

        public void RemoveAt(int fileNumber)
        {
            if (fileNumber < TCF.Count && fileNumber >= 0)
                TCF.RemoveAt(fileNumber);

            if (TCF.Count > 0)
                calculate();
        }

        public void DeleteAll()
        {
            TCF = new List<TimeCourse>();
            nROI = 0;
            currentFileNumber = 0;
        }

        public void AddFile(TimeCourse TC1)
        {
            if (String.Compare(BaseName, TC1.BaseName) != 0)
            {
                TCF = new List<TimeCourse>();
                currentFileNumber = 0;
                UniqueIDs = new HashSet<int>();
                roiID = null;
                nROI = 0;
            }

            foreach (var uid in TC1.UniqueIDs)
                UniqueIDs.Add(uid);

            roiID = UniqueIDs.ToArray();

            bool found = false;

            for (int i = 0; i < TCF.Count; i++)
            {
                if (TCF[i].FileNumber == TC1.FileNumber) //Found a file!!
                {
                    TCF[i] = TC1.DeepCopy(); //Copy the data.
                    found = true;
                    break;
                }
            } //found

            if (!found)
            {
                TCF.Add(TC1.DeepCopy());
                currentFileNumber = TCF.Count - 1;
            }

            getNData();

            nROI = UniqueIDs.Count();
            BaseName = TC1.BaseName;
            FileName = TC1.FileName;
            SortByTime();
        }

        public void AddFileReference(TimeCourse TC1)
        {
            if (String.Compare(BaseName, TC1.BaseName) != 0)
            {
                TCF = new List<TimeCourse>();
                currentFileNumber = 0;
                UniqueIDs = new HashSet<int>();
                roiID = null;
                nROI = 0;
            }

            foreach (var uid in TC1.UniqueIDs)
                UniqueIDs.Add(uid);

            roiID = UniqueIDs.ToArray();

            bool found = false;

            for (int i = 0; i < TCF.Count; i++)
            {
                if (TCF[i].FileNumber == TC1.FileNumber)
                {
                    TCF[i] = TC1;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                TCF.Add(TC1);
                currentFileNumber = TCF.Count - 1;
            }

            getNData();

            nROI = UniqueIDs.Count();
            BaseName = TC1.BaseName;
            FileName = TC1.FileName;
            SortByTime();
        }

        private void getNData()
        {
            nData = TCF.Select(x => x.nData).ToArray().Sum();
        }

        public void SortByTime()
        {
            TCF = TCF.OrderBy(x => x.time_milliseconds[0]).ToList();
        }

        public void calculate()
        {
            getNData();

            time_milliseconds = new double[nData];
            time_seconds = new double[nData];
            nChannels = TCF[0].nChannels;


            fileNumber = new double[nData];
            fitting_param = new double[nChannels][][];

            for (int ch = 0; ch < nChannels; ch++)
                fitting_param[ch] = new double[nData][];

            int k = 0;
            for (int i = 0; i < TCF.Count; i++)
            {
                for (int j = 0; j < TCF[i].nData; j++)
                {
                    if (TCF[i].time_milliseconds[j] > 0)
                        time_seconds[k] = TCF[i].time_milliseconds[j] / 1000.0 - TCF[0].time_milliseconds[0] / 1000.0;
                    else if (k > 0)
                        time_seconds[k] = time_seconds[k - 1];
                    else
                        time_seconds[k] = 0;

                    if (time_seconds[k] < 0)
                        time_seconds[k] = time_seconds[k - 1];
                    time_milliseconds[k] = TCF[i].time_milliseconds[j];
                    fileNumber[k] = (double)TCF[i].FileNumber;
                    k++;
                }
            }

            int offset = 0;
            for (int i = 0; i < TCF.Count; i++)
            {
                if (TCF[i].nData > 0)
                {
                    for (int j = 0; j < TCF[i].nData; j++)
                    {
                        fileNumber[offset + j] = TCF[i].FileNumber;
                        for (int ch = 0; ch < nChannels; ch++)
                        {
                            try
                            {
                                if (TCF[i].fitting_param != null && TCF[i].fitting_param.Length > ch &&
                                    TCF[i].fitting_param[ch] != null && TCF[i].fitting_param[ch].Length > j &&
                                    TCF[i].fitting_param[ch][j] != null)
                                    fitting_param[ch][offset + j] = (double[])TCF[i].fitting_param[ch][j].Clone();
                                else
                                    fitting_param[ch][offset + j] = null;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }

                offset += TCF[i].nData;
            }


            var uniqueIdList = UniqueIDs.ToList();

            foreach (string fieldname in paramNames)
            {
                var arr1 = new double[nChannels][];
                for (int ch = 0; ch < nChannels; ch++)
                    arr1[ch] = new double[nData];

                var arr_roi = new double[nChannels, nROI][];
                for (int ch = 0; ch < nChannels; ch++)
                    for (int roi = 0; roi < nROI; roi++)
                        arr_roi[ch, roi] = new double[nData];

                offset = 0;
                for (int i = 0; i < TCF.Count; i++)
                {
                    if (TCF[i].nData > 0)
                    {
                        var TCFi_arr = (double[][])TCF[i].GetType().GetField(fieldname).GetValue(TCF[i]);
                        var TCFi_arr_roi = (double[,][])TCF[i].GetType().GetField(fieldname + "_ROI").GetValue(TCF[i]);
                        var tcfiUniqueIds = TCF[i].UniqueIDs.ToList();

                        if (TCFi_arr != null)
                            for (int ch = 0; ch < nChannels; ch++)
                            {
                                if (TCFi_arr.Length > ch && TCFi_arr[ch] != null)
                                    Array.Copy(TCFi_arr[ch], 0, arr1[ch], offset, TCF[i].nData);
                            }

                        if (TCFi_arr_roi != null && tcfiUniqueIds.Count > 0)
                        {
                            int maxCh = Math.Min(TCFi_arr_roi.GetLength(0), arr_roi.GetLength(0));
                            for (int ch = 0; ch < maxCh; ch++)
                                for (int roi = 0; roi < TCFi_arr_roi.GetLength(1); roi++)
                                {
                                    if (roi >= tcfiUniqueIds.Count)
                                        continue;

                                    int TCFi_roiID = tcfiUniqueIds[roi];
                                    int this_roiIndex = uniqueIdList.IndexOf(TCFi_roiID);

                                    if (this_roiIndex >= 0 && this_roiIndex < arr_roi.GetLength(1) &&
                                        TCFi_arr_roi[ch, roi] != null)
                                        Array.Copy(TCFi_arr_roi[ch, roi], 0, arr_roi[ch, this_roiIndex], offset, TCF[i].nData);
                                }
                        }
                    }

                    offset += TCF[i].nData;
                }

                GetType().GetField(fieldname).SetValue(this, arr1);
                GetType().GetField(fieldname + "_ROI").SetValue(this, arr_roi);
            }

            if (nData > 0)
                RemoveNonValueROIs();
        }//calculate()


        public void RemoveNonValueROIs()
        {
            List<int> withValueROI = new List<int>();
            for (int roi = 0; roi < nROI; roi++)
            {
                bool allzero = true;
                foreach (string fieldname in paramNames)
                {
                    var field = this.GetType().GetField(fieldname + "_ROI");
                    double[,][] arr_roi;
                    if (field != null)
                    {
                        arr_roi = (double[,][])field.GetValue(this);
                        for (int ch = 0; ch < nChannels; ch++)
                            allzero = arr_roi[ch, roi].All(x => x == 0) && allzero;
                    }
                    else
                    {
                        arr_roi = null; //allzero is still true.
                    }
                }
                if (!allzero)
                    withValueROI.Add(roi);
            }

            if (withValueROI.Count != nROI)
            {
                foreach (string fieldname in paramNames)
                {
                    var field = this.GetType().GetField(fieldname + "_ROI");
                    double[,][] arr_roi = null;
                    if (field != null)
                    {
                        arr_roi = (double[,][])field.GetValue(this);
                        var new_arr = new double[nChannels, withValueROI.Count][];
                        for (int roi = 0; roi < withValueROI.Count; roi++)
                        {
                            for (int ch = 0; ch < nChannels; ch++)
                                new_arr[ch, roi] = arr_roi[ch, withValueROI[roi]];
                        }

                        GetType().GetField(fieldname + "_ROI").SetValue(this, new_arr);
                    }
                }

                var newID = new HashSet<int>();
                var roiIDs = UniqueIDs.ToList();
                for (int roi = 0; roi < withValueROI.Count; roi++)
                    newID.Add(roiIDs[withValueROI[roi]]);

                UniqueIDs = newID;
                nROI = UniqueIDs.ToList().Count;
                roiID = UniqueIDs.ToArray();
            }
        }

        /// <summary>
        /// Get Array by the parameter name (like "Lifetime" etc). Don't include "_ROI".
        /// </summary>
        /// <param name="parameterName">Lifetime, Lifetime_fit, Fraction2, Fraction2_fit, etc...</param>
        /// <param name="Channel">Channel counted from 0</param>
        /// <param name="ROI">Not ROI-ID, but roi number counted from 0</param>
        /// <returns></returns>
        public double[] GetArrayByName(string parameterName, int Channel, int ROI)
        {
            double[] values = null;
            if (parameterName.Contains("_ROI") && ROI >= 0)
            {
                var field = this.GetType().GetField(parameterName);
                if (field != null)
                {
                    var arr_roi = (double[,][])field.GetValue(this);
                    if (arr_roi != null && Channel < arr_roi.GetLength(0) && ROI < arr_roi.GetLength(1))
                        values = arr_roi[Channel, ROI];
                }
            }
            else
            {
                var field = this.GetType().GetField(parameterName);
                if (field != null)
                {
                    var arr1 = (double[][])field.GetValue(this);
                    if (arr1 != null && Channel < arr1.Length)
                        values = arr1[Channel];
                }
            }
            return values;
        }


        public void SetArrayByName(string parameterName, int Channel, int ROI, double[] values)
        {
            if (parameterName.Contains("_ROI") && ROI >= 0)
            {
                var field = this.GetType().GetField(parameterName);
                if (field != null)
                {
                    var arr_roi = (double[,][])field.GetValue(this);
                    if (arr_roi != null && Channel < arr_roi.GetLength(0) && ROI < arr_roi.GetLength(1))
                    {
                        arr_roi[Channel, ROI] = values;
                        field.SetValue(this, arr_roi);
                    }
                }
            }
            else
            {
                var field = this.GetType().GetField(parameterName);
                if (field != null)
                {
                    var arr1 = (double[][])field.GetValue(this);
                    if (arr1 != null && Channel < arr1.Length)
                    {
                        arr1[Channel] = values;
                        field.SetValue(this, arr1);
                    }
                }
            }
        }

        public TimeCourse_Files DeepCopy()
        {
            TimeCourse_Files TCF1 = new TimeCourse_Files();
            Copier.DeepCopyClass(this, TCF1);

            TCF1.TCF = new List<TimeCourse>();
            foreach (var tcf in TCF)
            {
                TCF1.TCF.Add(tcf.DeepCopy());
            }

            return TCF1;
        }
    }
}
