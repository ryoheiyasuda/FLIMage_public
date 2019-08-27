using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCSPC_controls;
using MathLibrary;

namespace FLIMage.Analysis
{
    public class ImageInfo
    {
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
        public double[] tau_m;
        public double[] tau_m_fit;
        public double[] fraction2;
        public double[] fraction2_fit;
        public double[] intensity;
        public double[] sumIntensity;
        public double[] nPixels;

        public double[][] tau_m_ROI;
        public double[][] tau_m_fit_ROI;
        public double[][] fraction2_ROI;
        public double[][] fraction2_fit_ROI;
        public double[][] intensity_ROI;
        public double[][] sumIntensity_ROI;
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
            List<ROI> ROI_Multi = FLIM.ROIs;
            nROIs = ROI_Multi.Count;

            FileCounter = State.Files.fileCounter;
            Page = FLIM.currentPage;
            ZStack = State.Acq.ZStack;
            nChannels = FLIM.nChannels;

            FileName = State.Files.fullName();
            BaseName = State.Files.baseName;

            fitting_param = new double[nChannels][];
            fitting_param[0] = (double[])State.Spc.analysis.fit_param1.Clone();

            if (nChannels > 1)
                fitting_param[1] = (double[])State.Spc.analysis.fit_param2.Clone();

            roiID = null;

            if (nROIs > 0)
            {
                tau_m_ROI = new double[nROIs][];
                tau_m_fit_ROI = new double[nROIs][];
                fraction2_ROI = new double[nROIs][];
                fraction2_fit_ROI = new double[nROIs][];
                intensity_ROI = new double[nROIs][];
                sumIntensity_ROI = new double[nROIs][];
                nPixels_ROI = new double[nROIs][];
                roiID = new int[nROIs];

                for (int i = 0; i < nROIs; i++)
                {
                    getData(ROI_Multi[i]);
                    roiID[i] = ROI_Multi[i].ID - 1;
                    tau_m_ROI[i] = (double[])tau_m.Clone();
                    tau_m_fit_ROI[i] = (double[])tau_m_fit.Clone();
                    fraction2_ROI[i] = (double[])fraction2.Clone();
                    fraction2_fit_ROI[i] = (double[])fraction2_fit.Clone();
                    intensity_ROI[i] = (double[])intensity.Clone();
                    sumIntensity_ROI[i] = (double[])sumIntensity.Clone();
                    nPixels_ROI[i] = (double[])nPixels.Clone();
                }
            }

            getData(Roi);

            double tempM = (acquiredTime - new DateTime(2006, 1, 1)).TotalMilliseconds;
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

        void getData(ROI Roi)
        {
            fraction2 = new double[nChannels];
            fraction2_fit = new double[nChannels];

            intensity = (double[])Roi.intensity.Clone();
            sumIntensity = (double[])Roi.sumIntensity.Clone();
            nPixels = (double[])Roi.nPixels.Clone();
            tau_m = (double[])Roi.tau_m_fromMAP.Clone();
            tau_m_fit = (double[])Roi.tau_m.Clone();

            for (int ch = 0; ch < nChannels; ch++)
            {
                double tauD = fitting_param[ch][1];
                double tauAD = fitting_param[ch][3];
                double tau_m0 = tau_m[ch];

                fraction2[ch] = tauD * (tauD - tau_m0) / (tauD - tauAD) / (tauD + tauAD - tau_m0);
                tau_m_fit[ch] = Res * Roi.tau_m[ch];

                if (Roi.beta[ch].Length == 6)
                {
                    tauD = Res / Roi.beta[ch][1];
                    tauAD = Res / Roi.beta[ch][3];
                    //Debug.WriteLine("TauD, TauAD, Tau_m_fit = {0}, {1}, {2}", tauD, tauAD, tau_m_fit[ch]);
                    double frac = Roi.beta[ch][2] / (Roi.beta[ch][0] + Roi.beta[ch][2]);
                    fraction2_fit[ch] = frac;
                }
            }
        }

        public ImageInfo DeepCopy()
        {
            ImageInfo iminfo1 = new ImageInfo();
            var members = this.GetType().GetFields();
            object ValB;

            foreach (var member in members)
            {
                var value = member.GetValue(this);
                var valType = member.FieldType;
                var memberName = member.Name;

                if (value != null)
                {
                    if (valType == typeof(double[][]))
                    {
                        var orgVal = (double[][])value;
                        var valA = new double[orgVal.Length][];
                        for (int ch = 0; ch < orgVal.Length; ch++)
                        {
                            valA[ch] = (double[])orgVal[ch].Clone();
                        }
                        ValB = (object)valA;
                    }
                    else if (valType == typeof(double[]))
                    {
                        var orgVal = (double[])value;
                        ValB = orgVal.Clone();
                    }
                    else
                        ValB = (object)value;

                    iminfo1.GetType().GetField(memberName).SetValue(iminfo1, ValB);
                }
            }

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
        public double[] time_milliseconds;
        public double[] time_seconds;

        public double[][] Lifetime;
        public double[][] Fraction2;
        public double[][] Lifetime_fit;
        public double[][] Fraction2_fit;
        public double[][] Intensity;
        public double[][] sumIntensity;
        public double[][] nPixels;

        public double[,][] lifetime_ROI;
        public double[,][] fraction2_ROI;
        public double[,][] lifetime_fit_ROI;
        public double[,][] fraction2_fit_ROI;
        public double[,][] intensity_ROI;
        public double[,][] sumIntensity_ROI;
        public double[,][] nPixels_ROI;
        public int nROI = 0;
        public int nData = 0;
        public int nChannels = 1;
        double[][][] fitting_param;

        public TimeCourse()
        {
            ImInfos.Clear();
        }


        /// <summary>
        /// Check if the file is new and then either add or integrate.
        /// </summary>
        /// <param name="iminfo"></param>
        /// <param name="num"></param>
        public bool AddFile(ImageInfo iminfo, int num)
        {
            bool success = true;
            if (iminfo.FileName != FileName && nData > 0)
            {
                return false;
                //ImInfos.Clear();
                //ImInfos.Add(iminfo);
                //time_milliseconds = new double[] { iminfo.time_milliseconds };
            }
            else
            {
                if (ImInfos.Count > num) //Replacing existing.
                {
                    ImInfos[num] = iminfo;
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
                        ImInfos.Add(iminfo);
                        time_milliseconds[ImInfos.Count - 1] = iminfo.time_milliseconds; //putting place holder.
                    }
                }
            }

            nData = ImInfos.Count;
            nROI = Math.Max(nROI, iminfo.nROIs);

            FileName = iminfo.FileName;
            BaseName = iminfo.BaseName;
            FileNumber = iminfo.FileCounter;

            simpleAdd(iminfo, num);
            return success;
        }

        public void SortByTime()
        {
            int[] count = new int[ImInfos.Count];

            for (int i = 0; i < ImInfos.Count; i++)
                count[i] = i;

            Array.Sort(time_milliseconds, count);
            List<ImageInfo> ImInfosNew = new List<ImageInfo>();

            for (int i = 0; i < ImInfos.Count; i++)
            {
                ImInfosNew.Add(ImInfos[count[i]]);
            }

            ImInfos = ImInfosNew;
            calculate();
        }

        public void simpleAdd(ImageInfo imimfo, int num)
        {
            nChannels = ImInfos[0].nChannels;
            int oldnRoi = nROI;

            if (ImInfos[num] != null && ImInfos[num].roiID != null)
                nROI = Math.Max(ImInfos[num].roiID.Max(), nROI);

            if (time_seconds == null || (nROI != oldnRoi) || (lifetime_ROI == null && nROI > 0))
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

            for (int ch = 0; ch < nChannels; ch++)
            {
                Array.Resize(ref fitting_param[ch], nData);
                Array.Resize(ref Lifetime[ch], nData);
                Array.Resize(ref Lifetime_fit[ch], nData);
                Array.Resize(ref Fraction2[ch], nData);
                Array.Resize(ref Fraction2_fit[ch], nData);
                Array.Resize(ref Intensity[ch], nData);
                Array.Resize(ref sumIntensity[ch], nData);
                Array.Resize(ref nPixels[ch], nData);

                if (nROI > 0 && lifetime_ROI.GetLength(1) < nROI)
                {
                    MatrixCalc.ResizeArray2D(ref lifetime_ROI, nChannels, nROI);
                    MatrixCalc.ResizeArray2D(ref lifetime_fit_ROI, nChannels, nROI);
                    MatrixCalc.ResizeArray2D(ref fraction2_ROI, nChannels, nROI);
                    MatrixCalc.ResizeArray2D(ref fraction2_fit_ROI, nChannels, nROI);
                    MatrixCalc.ResizeArray2D(ref intensity_ROI, nChannels, nROI);
                    MatrixCalc.ResizeArray2D(ref sumIntensity_ROI, nChannels, nROI);
                    MatrixCalc.ResizeArray2D(ref nPixels_ROI, nChannels, nROI);
                }

                for (int i = 0; i < nROI; i++)
                {
                    Array.Resize(ref lifetime_ROI[ch, i], nData);
                    Array.Resize(ref lifetime_fit_ROI[ch, i], nData);
                    Array.Resize(ref fraction2_ROI[ch, i], nData);
                    Array.Resize(ref fraction2_fit_ROI[ch, i], nData);
                    Array.Resize(ref intensity_ROI[ch, i], nData);
                    Array.Resize(ref sumIntensity_ROI[ch, i], nData);
                    Array.Resize(ref nPixels_ROI[ch, i], nData);
                }

                fitting_param[ch][num] = (double[])ImInfos[num].fitting_param[ch].Clone();

                Lifetime[ch][num] = ImInfos[num].tau_m[ch];
                Fraction2[ch][num] = ImInfos[num].fraction2[ch];
                Lifetime_fit[ch][num] = ImInfos[num].tau_m_fit[ch];
                Fraction2_fit[ch][num] = ImInfos[num].fraction2_fit[ch];
                Intensity[ch][num] = ImInfos[num].intensity[ch];
                sumIntensity[ch][num] = ImInfos[num].sumIntensity[ch];
                nPixels[ch][num] = ImInfos[num].nPixels[ch];

                for (int i = 0; i < ImInfos[num].nROIs; i++)
                {
                    int roiID = ImInfos[num].roiID[i]; //getroiID for each ImInfo.
                    if (ImInfos[num].tau_m_ROI != null && ImInfos[num].tau_m_ROI.Length > i)
                    {
                        lifetime_ROI[ch, roiID][num] = ImInfos[num].tau_m_ROI[i][ch];
                        fraction2_ROI[ch, roiID][num] = ImInfos[num].fraction2_ROI[i][ch];
                        intensity_ROI[ch, roiID][num] = ImInfos[num].intensity_ROI[i][ch];
                        sumIntensity_ROI[ch, roiID][num] = ImInfos[num].sumIntensity_ROI[i][ch];
                        nPixels_ROI[ch, roiID][num] = ImInfos[num].nPixels_ROI[i][ch];
                    }
                    if (ImInfos[num].tau_m_fit_ROI != null && ImInfos[num].tau_m_fit_ROI.Length > i)
                    {
                        try
                        {
                            lifetime_fit_ROI[ch, roiID][num] = ImInfos[num].tau_m_fit_ROI[i][ch];
                            fraction2_fit_ROI[ch, roiID][num] = ImInfos[num].fraction2_fit_ROI[i][ch];
                        }
                        catch
                        {

                        }
                    }
                }
            }

        }//Simple Add()

        public void initializeArray(int nChannel, int nRois)
        {
            if (nChannels <= 0)
                return;

            nChannels = nChannel;

            Lifetime = new double[nChannels][];
            Fraction2 = new double[nChannels][];
            Lifetime_fit = new double[nChannels][];
            Fraction2_fit = new double[nChannels][];
            Intensity = new double[nChannels][];
            sumIntensity = new double[nChannels][];
            nPixels = new double[nChannels][];
            fitting_param = new double[nChannels][][];

            if (nRois > 0)
            {
                nROI = nRois;

                lifetime_ROI = new double[nChannels, nRois][];
                fraction2_ROI = new double[nChannels, nRois][];
                lifetime_fit_ROI = new double[nChannels, nRois][];
                fraction2_fit_ROI = new double[nChannels, nRois][];
                intensity_ROI = new double[nChannels, nRois][];
                sumIntensity_ROI = new double[nChannels, nRois][];
                nPixels_ROI = new double[nChannels, nRois][];
            }
        }

        public void ChangeROIarray(int nCh, int nRois)
        {
            if (nCh <= 0 && nRois <= 0)
                return;

            if (intensity_ROI == null)
                initializeArray(nCh, nRois);

            if (nChannels != nCh || nROI != nRois)
            {
                nChannels = nCh;
                nROI = nRois;

                MatrixCalc.ResizeArray2D(ref lifetime_ROI, nChannels, nRois);
                MatrixCalc.ResizeArray2D(ref fraction2_ROI, nChannels, nRois);
                MatrixCalc.ResizeArray2D(ref lifetime_fit_ROI, nChannels, nRois);
                MatrixCalc.ResizeArray2D(ref fraction2_fit_ROI, nChannels, nRois);
                MatrixCalc.ResizeArray2D(ref intensity_ROI, nChannels, nRois);
                MatrixCalc.ResizeArray2D(ref sumIntensity_ROI, nChannels, nRois);
                MatrixCalc.ResizeArray2D(ref nPixels_ROI, nChannels, nRois);
            }
        }


        public void calculate()
        {
            //time_milliseconds = new double[nData];
            time_seconds = new double[nData];
            nChannels = ImInfos[0].nChannels;

            Lifetime = new double[nChannels][];
            Fraction2 = new double[nChannels][];
            Lifetime_fit = new double[nChannels][];
            Fraction2_fit = new double[nChannels][];
            Intensity = new double[nChannels][];
            sumIntensity = new double[nChannels][];
            nPixels = new double[nChannels][];
            fitting_param = new double[nChannels][][];

            int[] maxRoiIDEach = new int[nData];
            for (int i = 0; i < nData; i++)
            {
                time_milliseconds[i] = ImInfos[i].time_milliseconds;
                time_seconds[i] = time_milliseconds[i] / 1000.0 - ImInfos[0].time_milliseconds / 1000.0;

                maxRoiIDEach[i] = ImInfos[i].roiID == null ? -1 : ImInfos[i].roiID.Max();
            }

            nROI = maxRoiIDEach.Max() + 1;

            if (nROI > 0)
            {
                lifetime_ROI = new double[nChannels, nROI][];
                fraction2_ROI = new double[nChannels, nROI][];
                lifetime_fit_ROI = new double[nChannels, nROI][];
                fraction2_fit_ROI = new double[nChannels, nROI][];
                intensity_ROI = new double[nChannels, nROI][];
                sumIntensity_ROI = new double[nChannels, nROI][];
                nPixels_ROI = new double[nChannels, nROI][];
            }


            for (int ch = 0; ch < nChannels; ch++)
            {
                fitting_param[ch] = new double[nData][];
                Lifetime[ch] = new double[nData];
                Lifetime_fit[ch] = new double[nData];
                Fraction2[ch] = new double[nData];
                Fraction2_fit[ch] = new double[nData];
                Intensity[ch] = new double[nData];
                sumIntensity[ch] = new double[nData];
                nPixels[ch] = new double[nData];

                for (int i = 0; i < nROI; i++)
                {
                    lifetime_ROI[ch, i] = new double[nData];
                    lifetime_fit_ROI[ch, i] = new double[nData];
                    fraction2_ROI[ch, i] = new double[nData];
                    fraction2_fit_ROI[ch, i] = new double[nData];
                    intensity_ROI[ch, i] = new double[nData];
                    sumIntensity_ROI[ch, i] = new double[nData];
                    nPixels_ROI[ch, i] = new double[nData];
                }

                for (int j = 0; j < nData; j++)
                {
                    if (ImInfos.Count > 0)
                    {
                        fitting_param[ch][j] = (double[])ImInfos[j].fitting_param[ch].Clone();

                        Lifetime[ch][j] = ImInfos[j].tau_m[ch];
                        Fraction2[ch][j] = ImInfos[j].fraction2[ch];
                        Lifetime_fit[ch][j] = ImInfos[j].tau_m_fit[ch];
                        Fraction2_fit[ch][j] = ImInfos[j].fraction2_fit[ch];
                        Intensity[ch][j] = ImInfos[j].intensity[ch];
                        sumIntensity[ch][j] = ImInfos[j].sumIntensity[ch];
                        nPixels[ch][j] = ImInfos[j].nPixels[ch];

                        for (int i = 0; i < ImInfos[j].nROIs; i++)
                        {
                            int roiID = ImInfos[j].roiID[i]; //getroiID for each ImInfo.
                            if (ImInfos[j].tau_m_ROI != null && ImInfos[j].tau_m_ROI.Length > i)
                            {
                                lifetime_ROI[ch, roiID][j] = ImInfos[j].tau_m_ROI[i][ch];
                                fraction2_ROI[ch, roiID][j] = ImInfos[j].fraction2_ROI[i][ch];
                                intensity_ROI[ch, roiID][j] = ImInfos[j].intensity_ROI[i][ch];
                                sumIntensity_ROI[ch, roiID][j] = ImInfos[j].sumIntensity_ROI[i][ch];
                                nPixels_ROI[ch, roiID][j] = ImInfos[j].nPixels_ROI[i][ch];
                            }
                            if (ImInfos[j].tau_m_fit_ROI != null && ImInfos[j].tau_m_fit_ROI.Length > i)
                            {
                                try
                                {
                                    lifetime_fit_ROI[ch, roiID][j] = ImInfos[j].tau_m_fit_ROI[i][ch];
                                    fraction2_fit_ROI[ch, roiID][j] = ImInfos[j].fraction2_fit_ROI[i][ch];
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
            }
        }//calculate()

        public TimeCourse DeepCopy()
        {
            TimeCourse TC1 = new TimeCourse();
            var members = this.GetType().GetFields();
            object ValB;

            foreach (var member in members)
            {
                var memberName = member.Name;
                var newField = TC1.GetType().GetField(memberName);

                var value = member.GetValue(this);
                var valType = member.FieldType;

                if (value != null)
                {
                    if (valType == typeof(double[][]))
                    {
                        var orgVal = (double[][])value;
                        var valA = new double[orgVal.Length][];
                        for (int ch = 0; ch < orgVal.Length; ch++)
                        {
                            if (orgVal[ch] != null)
                                valA[ch] = (double[])orgVal[ch].Clone();
                            else
                                valA[ch] = null;
                        }

                        ValB = (object)valA;
                    }
                    else if (valType == typeof(double[,][]))
                    {
                        var orgVal = (double[,][])value;
                        var valA = new double[orgVal.GetLength(0), orgVal.GetLength(1)][];
                        for (int ro = 0; ro < orgVal.GetLength(0); ro++)
                            for (int ch = 0; ch < orgVal.GetLength(1); ch++)
                            {
                                if (orgVal[ro, ch] != null)
                                    valA[ro, ch] = (double[])orgVal[ro, ch].Clone();
                            }

                        ValB = (object)valA;
                    }
                    else if (valType == typeof(double[][][]))
                    {
                        var orgVal = (double[][][])value;
                        var valA = new double[orgVal.Length][][];
                        for (int i = 0; i < orgVal.Length; i++)
                        {
                            valA[i] = new double[orgVal[i].Length][];
                            for (int j = 0; j < orgVal[i].Length; j++)
                            {
                                valA[i][j] = (double[])orgVal[i][j].Clone();
                            }
                        }
                        ValB = (object)valA;
                    }
                    else if (valType == typeof(List<ImageInfo>))
                    {
                        var orgVal = (List<ImageInfo>)value;
                        var valA = new List<ImageInfo>();

                        foreach (var val in orgVal)
                        {
                            valA.Add(val.DeepCopy());
                        }
                        ValB = (object)valA;
                    }
                    else if (valType == typeof(double[]))
                    {
                        var orgVal = (double[])value;
                        ValB = orgVal.Clone();
                    }
                    else
                        ValB = (object)value;

                    TC1.GetType().GetField(memberName).SetValue(TC1, ValB);
                }
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
        public String FileName;
        public int currentFileNumber;
        public String BaseName = "";

        public double[] time_milliseconds;
        public double[] time_seconds;
        public double[] fileNumber;

        public double[][] Lifetime;
        public double[][] Fraction2;
        public double[][] Lifetime_fit;
        public double[][] Fraction2_fit;
        public double[][] Intensity;
        public double[][] sumIntensity;
        public double[][] nPixels;

        public double[,][] lifetime_ROI;
        public double[,][] fraction2_ROI;
        public double[,][] lifetime_fit_ROI;
        public double[,][] fraction2_fit_ROI;
        public double[,][] intensity_ROI;
        public double[,][] sumIntensity_ROI;
        public double[,][] nPixels_ROI;
        public int nROI = 0;
        public int nData = 0;
        public int nChannels = 1;
        double[][][] fitting_param;

        public TimeCourse_Files()
        {
            currentFileNumber = 0;
        }

        public void DeleteCurrent()
        {
            if (currentFileNumber < TCF.Count && currentFileNumber >= 0)
                TCF.RemoveAt(currentFileNumber);

            if (TCF.Count > 0)
                calculate();
        }

        public void DeleteAll()
        {
            TCF.Clear();
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

            nROI = Math.Max(nROI, TC1.nROI);
            BaseName = TC1.BaseName;
            FileName = TC1.FileName;
            SortByTime();
            //            calculate();
        }

        public void ChangeROIarray(int nCh, int nRois)
        {
            for (int i = 0; i < TCF.Count; i++)
                TCF[i].ChangeROIarray(nCh, nRois);
        }

        private void getNData()
        {
            nData = 0;
            for (int i = 0; i < TCF.Count; i++)
                nData += TCF[i].nData;
        }

        public void SortByTime()
        {
            if (TCF.Count > 1)
            {
                int[] count = new int[TCF.Count];
                double[] time1 = new double[TCF.Count];

                for (int i = 0; i < TCF.Count; i++)
                {
                    count[i] = i;
                    time1[i] = TCF[i].time_milliseconds[0];
                }

                Array.Sort(time1, count);
                List<TimeCourse> TCFNew = new List<TimeCourse>();

                for (int i = 0; i < TCF.Count; i++)
                {
                    TCFNew.Add(TCF[count[i]]);
                }

                if (currentFileNumber < count.Count())
                    currentFileNumber = count[currentFileNumber];

                TCF = TCFNew;
                //calculate();
            }
        }

        public void calculate()
        {
            getNData();


            time_milliseconds = new double[nData];
            time_seconds = new double[nData];
            nChannels = TCF[0].nChannels;

            Lifetime = new double[nChannels][];
            Fraction2 = new double[nChannels][];
            Lifetime_fit = new double[nChannels][];
            Fraction2_fit = new double[nChannels][];
            Intensity = new double[nChannels][];
            sumIntensity = new double[nChannels][];
            nPixels = new double[nChannels][];

            fileNumber = new double[nData];
            int k = 0;
            for (int i = 0; i < TCF.Count; i++)
            {
                for (int j = 0; j < TCF[i].nData; j++)
                {
                    time_seconds[k] = TCF[i].time_milliseconds[j] / 1000.0 - TCF[0].time_milliseconds[0] / 1000.0;
                    time_milliseconds[k] = TCF[i].time_milliseconds[j];
                    fileNumber[k] = (double)TCF[i].FileNumber;
                    k++;
                }
            }

            lifetime_ROI = new double[nChannels, nROI][];
            fraction2_ROI = new double[nChannels, nROI][];
            lifetime_fit_ROI = new double[nChannels, nROI][];
            fraction2_fit_ROI = new double[nChannels, nROI][];
            intensity_ROI = new double[nChannels, nROI][];
            sumIntensity_ROI = new double[nChannels, nROI][];
            nPixels_ROI = new double[nChannels, nROI][];
            fitting_param = new double[nChannels][][];


            for (int ch = 0; ch < nChannels; ch++)
            {
                fitting_param[ch] = new double[nData][];
                Lifetime[ch] = new double[nData];
                Lifetime_fit[ch] = new double[nData];
                Fraction2[ch] = new double[nData];
                Fraction2_fit[ch] = new double[nData];
                Intensity[ch] = new double[nData];
                sumIntensity[ch] = new double[nData];
                nPixels[ch] = new double[nData];

                for (int i = 0; i < nROI; i++)
                {
                    lifetime_ROI[ch, i] = new double[nData];
                    lifetime_fit_ROI[ch, i] = new double[nData];
                    fraction2_ROI[ch, i] = new double[nData];
                    fraction2_fit_ROI[ch, i] = new double[nData];
                    intensity_ROI[ch, i] = new double[nData];
                    sumIntensity_ROI[ch, i] = new double[nData];
                    nPixels_ROI[ch, i] = new double[nData];
                }

                int offset = 0;
                for (int i = 0; i < TCF.Count; i++)
                {
                    if (TCF[i].nData > 0)
                    {
                        if (TCF[i].Intensity != null && TCF[i].Intensity[ch] != null)
                        {
                            Array.Copy(TCF[i].Lifetime[ch], 0, Lifetime[ch], offset, TCF[i].nData);
                            Array.Copy(TCF[i].Intensity[ch], 0, Intensity[ch], offset, TCF[i].nData);
                            Array.Copy(TCF[i].sumIntensity[ch], 0, sumIntensity[ch], offset, TCF[i].nData);
                            Array.Copy(TCF[i].nPixels[ch], 0, nPixels[ch], offset, TCF[i].nData);
                            Array.Copy(TCF[i].Fraction2[ch], 0, Fraction2[ch], offset, TCF[i].nData);
                            Array.Copy(TCF[i].Lifetime_fit[ch], 0, Lifetime_fit[ch], offset, TCF[i].nData);
                            Array.Copy(TCF[i].Fraction2_fit[ch], 0, Fraction2_fit[ch], offset, TCF[i].nData);
                        }

                        for (int roi = 0; roi < nROI; roi++)
                        {
                            if (TCF[i].intensity_ROI == null)
                            {
                                TCF[i].lifetime_ROI = new double[nChannels, nROI][];
                                TCF[i].fraction2_ROI = new double[nChannels, nROI][];
                                TCF[i].intensity_ROI = new double[nChannels, nROI][];
                                TCF[i].sumIntensity_ROI = new double[nChannels, nROI][];
                                TCF[i].nPixels_ROI = new double[nChannels, nROI][];
                                TCF[i].lifetime_fit_ROI = new double[nChannels, nROI][];
                                TCF[i].fraction2_fit_ROI = new double[nChannels, nROI][];

                            }

                            if (TCF[i].lifetime_ROI.GetLength(1) < nROI)
                            {
                                MatrixCalc.ResizeArray2D(ref TCF[i].lifetime_ROI, nChannels, nROI);
                                MatrixCalc.ResizeArray2D(ref TCF[i].fraction2_ROI, nChannels, nROI);
                                MatrixCalc.ResizeArray2D(ref TCF[i].intensity_ROI, nChannels, nROI);
                                MatrixCalc.ResizeArray2D(ref TCF[i].sumIntensity_ROI, nChannels, nROI);
                                MatrixCalc.ResizeArray2D(ref TCF[i].nPixels_ROI, nChannels, nROI);
                                MatrixCalc.ResizeArray2D(ref TCF[i].lifetime_fit_ROI, nChannels, nROI);
                                MatrixCalc.ResizeArray2D(ref TCF[i].fraction2_fit_ROI, nChannels, nROI);
                            }

                            MatrixCalc.ChangeNDataArray(ref TCF[i].lifetime_ROI[ch, roi], nData);
                            MatrixCalc.ChangeNDataArray(ref TCF[i].fraction2_ROI[ch, roi], nData);
                            MatrixCalc.ChangeNDataArray(ref TCF[i].intensity_ROI[ch, roi], nData);
                            MatrixCalc.ChangeNDataArray(ref TCF[i].sumIntensity_ROI[ch, roi], nData);
                            MatrixCalc.ChangeNDataArray(ref TCF[i].nPixels_ROI[ch, roi], nData);
                            MatrixCalc.ChangeNDataArray(ref TCF[i].lifetime_fit_ROI[ch, roi], nData);
                            MatrixCalc.ChangeNDataArray(ref TCF[i].fraction2_fit_ROI[ch, roi], nData);

                            //if (TCF[i].lifetime_ROI[ch, roi].Length < nData)
                            //{
                            //    Array.Resize(ref TCF[i].lifetime_ROI[ch, roi], nData);
                            //    Array.Resize(ref TCF[i].fraction2_ROI[ch, roi], nData);
                            //    Array.Resize(ref TCF[i].intensity_ROI[ch, roi], nData);
                            //    Array.Resize(ref TCF[i].sumIntensity_ROI[ch, roi], nData);
                            //    Array.Resize(ref TCF[i].nPixels_ROI[ch, roi], nData);
                            //    Array.Resize(ref TCF[i].lifetime_fit_ROI[ch, roi], nData);
                            //    Array.Resize(ref TCF[i].fraction2_fit_ROI[ch, roi], nData);
                            //}

                            if (TCF[i].lifetime_ROI.GetLength(1) > roi)
                            {
                                Array.Copy(TCF[i].lifetime_ROI[ch, roi], 0, lifetime_ROI[ch, roi], offset, TCF[i].nData);
                                Array.Copy(TCF[i].fraction2_ROI[ch, roi], 0, fraction2_ROI[ch, roi], offset, TCF[i].nData);
                                Array.Copy(TCF[i].intensity_ROI[ch, roi], 0, intensity_ROI[ch, roi], offset, TCF[i].nData);
                                Array.Copy(TCF[i].sumIntensity_ROI[ch, roi], 0, sumIntensity_ROI[ch, roi], offset, TCF[i].nData);
                                Array.Copy(TCF[i].nPixels_ROI[ch, roi], 0, nPixels_ROI[ch, roi], offset, TCF[i].nData);
                                Array.Copy(TCF[i].lifetime_fit_ROI[ch, roi], 0, lifetime_fit_ROI[ch, roi], offset, TCF[i].nData);
                                Array.Copy(TCF[i].fraction2_fit_ROI[ch, roi], 0, fraction2_fit_ROI[ch, roi], offset, TCF[i].nData);
                            }
                        }
                    }

                    offset += TCF[i].nData;

                }
            }
        }//calculate()


        public TimeCourse_Files DeepCopy()
        {
            TimeCourse_Files TCF1 = new TimeCourse_Files();
            var members = this.GetType().GetFields();
            object ValB;

            foreach (var member in members)
            {
                var value = member.GetValue(this);
                var valType = member.FieldType;
                var memberName = member.Name;

                if (value != null)
                {
                    if (valType == typeof(double[][]))
                    {
                        var orgVal = (double[][])value;
                        var valA = new double[orgVal.Length][];
                        for (int ch = 0; ch < orgVal.Length; ch++)
                        {
                            valA[ch] = (double[])orgVal[ch].Clone();
                        }

                        ValB = (object)valA;
                    }
                    else if (valType == typeof(double[,][]))
                    {
                        var orgVal = (double[,][])value;
                        var valA = new double[orgVal.GetLength(0), orgVal.GetLength(1)][];
                        for (int ro = 0; ro < orgVal.GetLength(0); ro++)
                            for (int ch = 0; ch < orgVal.GetLength(1); ch++)
                            {
                                valA[ro, ch] = (double[])orgVal[ro, ch].Clone();
                            }

                        ValB = (object)valA;
                    }
                    else if (valType == typeof(double[][][]))
                    {
                        var orgVal = (double[][][])value;
                        var valA = new double[orgVal.Length][][];
                        for (int i = 0; i < orgVal.Length; i++)
                        {
                            valA[i] = new double[orgVal[i].Length][];
                            for (int j = 0; j < orgVal[i].Length; j++)
                            {
                                valA[i][j] = (double[])orgVal[i][j].Clone();
                            }
                        }
                        ValB = (object)valA;
                    }
                    else if (valType == typeof(List<TimeCourse>))
                    {
                        var orgVal = (List<TimeCourse>)value;
                        var valA = new List<TimeCourse>();

                        foreach (var val in orgVal)
                        {
                            valA.Add(val.DeepCopy());
                        }
                        ValB = (object)valA;
                    }
                    else if (valType == typeof(double[]))
                    {
                        var orgVal = (double[])value;
                        ValB = orgVal.Clone(); 
                    }
                    else
                        ValB = (object)value;

                    TCF1.GetType().GetField(memberName).SetValue(TCF1, ValB);
                }
            }

            return TCF1;
        }
    }
}
