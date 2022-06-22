using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCSPC_controls;
using MathLibrary;
using Utilities;

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
        public bool ZStack;
        public int Page;
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
            ROI Roi = FLIM.Roi;
            ROI bgRoi = FLIM.bgRoi;

            List<ROI> ROI_Multi = FLIM.ROIs;
            nROIs = ROI_Multi.Count;
            int nTotalROIs = 0;
            foreach (var roi in FLIM.ROIs)
            {
                nTotalROIs += 1;
                if (roi.ROI_type == ROI.ROItype.PolyLine)
                    nTotalROIs += roi.polyLineROIs.Count;
            }

            FileCounter = FLIM.fileCounter; //State.Files.fileCounter;
            Page = FLIM.currentPage;
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
                    getData(ROI_Multi[roiN], bgRoi);
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
                            getData(roi1, bgRoi);

                            roiID[i] = (ROI_Multi[roiN].ID) * 1000 + roi1.ID;

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

            getData(Roi, bgRoi);

            double tempM = (acquiredTime - ImageInfo.startDate).TotalMilliseconds;
            if (tempM > 0) //perhaps this.
            {
                time_milliseconds = tempM;
            }
            else
            {
                DateTime dt = acquiredTime;
                double time_milliseconds_base = (dt - new DateTime(2000, 1, 1)).TotalMilliseconds;

                if (FLIM.n_pages > 1)
                {
                    double fInterval = State.Acq.frameInterval() * State.Acq.nAveFrame;

                    int numFrames = 1;
                    if (State.Acq.aveFrame)
                        numFrames = State.Acq.nFrames / State.Acq.nAveFrame;

                    Frame = Page % numFrames;
                    Slice = Page / numFrames;

                    double timeFromBase = 1000.0 * (Frame * fInterval + Slice * State.Acq.sliceInterval);

                    time_milliseconds = time_milliseconds_base + timeFromBase;
                }
                else
                    time_milliseconds = time_milliseconds_base;

            }
        }

        void getData(ROI Roi, ROI bgRoi)
        {
            Fraction2 = new double[nChannels];
            Fraction2_fit = new double[nChannels];

            meanIntensity = (double[])Roi.flim_parameters.meanIntensity.Clone();
            sumIntensity = (double[])Roi.flim_parameters.sumIntensity.Clone();

            meanIntensity_bg = (double[])meanIntensity.Clone();
            sumIntensity_bg = (double[])sumIntensity.Clone();

            nPixels = (double[])Roi.flim_parameters.nPixels.Clone();

            for (int ch = 0; ch < meanIntensity.Length; ch++)
            {
                if (bgRoi.flim_parameters.meanIntensity.Length > ch)
                    meanIntensity_bg[ch] = meanIntensity[ch] - bgRoi.flim_parameters.meanIntensity[ch];
                sumIntensity_bg[ch] = meanIntensity_bg[ch] * nPixels[ch];
            }

            Lifetime = (double[])Roi.flim_parameters.tau_m_fromMAP.Clone();
            Lifetime_fit = (double[])Roi.flim_parameters.tau_m.Clone();

            for (int ch = 0; ch < nChannels; ch++)
            {
                double tauD = fitting_param[ch][1];
                double tauAD = fitting_param[ch][3];
                double tau_m0 = Lifetime[ch];

                Fraction2[ch] = tauD * (tauD - tau_m0) / (tauD - tauAD) / (tauD + tauAD - tau_m0);
                Lifetime_fit[ch] = Roi.flim_parameters.tau_m[ch];

                if (Roi.flim_parameters.beta[ch].Length == 6)
                {
                    tauD = Res / Roi.flim_parameters.beta[ch][1];
                    tauAD = Res / Roi.flim_parameters.beta[ch][3];
                    //Debug.WriteLine("TauD, TauAD, Tau_m_fit = {0}, {1}, {2}", tauD, tauAD, Lifetime_fit[ch]);
                    double frac = Roi.flim_parameters.beta[ch][2] / (Roi.flim_parameters.beta[ch][0] + Roi.flim_parameters.beta[ch][2]);
                    Fraction2_fit[ch] = frac;
                }
            }
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
        public int nData = 0;
        public int nChannels = 1;
        public double[][][] fitting_param;

        public TimeCourse()
        {
            ImInfos = new List<ImageInfo>();
            UniqueIDs = new HashSet<int>();
        }

        public void AddFLIM(FLIMData flim, int num)
        {
            if (flim.Roi.flim_parameters_Pages == null)
            {
                ImageInfo im = new ImageInfo(flim);
                AddFile(im, num);
                return;
            }

            time_milliseconds = flim.acquiredTime_Pages.Select(x => (x - ImageInfo.startDate).TotalMilliseconds).ToArray();
            time_seconds = new double[time_milliseconds.Length];
            time_seconds = time_milliseconds.Select(x => (x - time_milliseconds[0]) / 1000.0).ToArray();
            //for (int i = 0; i < time_milliseconds.Length; i++)
            //{
            //    time_milliseconds[i] = time_milliseconds[i] >= time_milliseconds[0] ? time_milliseconds[i] : time_milliseconds[i - 1];
            //    time_seconds[i] = (time_milliseconds[i] - time_milliseconds[0]) / 1000.0;
            //}

            UniqueIDs = new HashSet<int>(); //If you Clear(), it will affect previous TC.

            if (flim.ROIs.Count > 0)
                foreach (var roi in flim.ROIs)
                    UniqueIDs.Add(roi.ID);

            nROI = flim.ROIs.Count;
            FileName = flim.fileName;
            BaseName = flim.baseName;
            FileNumber = flim.fileCounter;
            nChannels = flim.nChannels;

            nData = 1;
            if (!flim.ZStack)
            {
                if (flim.nFastZ > 1)
                    nData = flim.n_pages5D;
                else
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
                    nPixels[ch][i] = param_roi[i].nPixels[ch];
                    meanIntensity[ch][i] = param_roi[i].meanIntensity[ch];
                    sumIntensity[ch][i] = param_roi[i].sumIntensity[ch];
                    double bg = 0;
                    if (param_bg != null)
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

            for (int roi = 0; roi < nROI; roi++)
            {
                param_roi = flim.ROIs[roi].flim_parameters_Pages;
                for (int ch = 0; ch < nChannels; ch++)
                {
                    for (int i = 0; i < nData; i++)
                    {
                        nPixels_ROI[ch, roi][i] = param_roi[i].nPixels[ch];
                        meanIntensity_ROI[ch, roi][i] = param_roi[i].meanIntensity[ch];
                        sumIntensity_ROI[ch, roi][i] = param_roi[i].sumIntensity[ch];
                        double bg = 0;
                        if (param_bg != null)
                            bg = param_bg[i].meanIntensity[ch];
                        meanIntensity_bg_ROI[ch, roi][i] = meanIntensity_ROI[ch, roi][i] - bg;
                        sumIntensity_bg_ROI[ch, roi][i] = meanIntensity_bg_ROI[ch, roi][i] * nPixels_ROI[ch, roi][i];
                        Lifetime_ROI[ch, roi][i] = param_roi[i].tau_m_fromMAP[ch];
                        Lifetime_fit_ROI[ch, roi][i] = param_roi[i].tau_m[ch];

                        double tauD = res / param_roi[i].beta[ch][1];
                        double tauAD = res / param_roi[i].beta[ch][3];
                        double tau_m0 = Lifetime_ROI[ch, roi][i];

                        Fraction2_ROI[ch, roi][i] = tauD * (tauD - tau_m0) / (tauD - tauAD) / (tauD + tauAD - tau_m0);
                        double frac = param_roi[i].beta[ch][2] / (param_roi[i].beta[ch][0] + param_roi[i].beta[ch][2]);
                        Fraction2_fit_ROI[ch, roi][i] = frac;
                    }
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

            if (ImInfos.Count > num) //Replacing existing.
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
                    arr = new double[nChannels, nROI][];
                }
                for (int roi = 0; roi < nROI; roi++)
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
                return;
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

            if (nChannels != nCh || nROI != nRois)
            {
                nChannels = nCh;
                nROI = nRois;

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

                        if (TCFi_arr != null)
                            for (int ch = 0; ch < nChannels; ch++)
                            {
                                if (TCFi_arr.Length > ch && TCFi_arr[ch] != null)
                                    Array.Copy(TCFi_arr[ch], 0, arr1[ch], offset, TCF[i].nData);
                            }

                        if (TCFi_arr_roi != null && TCF[i].UniqueIDs.Count > 0)
                            for (int ch = 0; ch < TCFi_arr_roi.GetLength(0); ch++)
                                for (int roi = 0; roi < TCFi_arr_roi.GetLength(1); roi++)
                                {
                                    int TCFi_roiID = TCF[i].UniqueIDs.ToList()[roi];
                                    int this_roiIndex = UniqueIDs.ToList().IndexOf(TCFi_roiID);

                                    if (this_roiIndex < arr_roi.GetLength(1) && TCFi_arr_roi[ch, roi] != null)
                                        Array.Copy(TCFi_arr_roi[ch, roi], 0, arr_roi[ch, this_roiIndex], offset, TCF[i].nData);
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
            bool allzero = true;
            for (int roi = 0; roi < nROI; roi++)
            {
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
                    newID.Add(roiIDs[roi]);

                UniqueIDs = newID;
                nROI = UniqueIDs.ToList().Count;
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
