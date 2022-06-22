using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCSPC_controls
{
    public class MemoryManager
    {
        const int nBank = 3;

        int nChannels;
        int nZlocs;
        int nLines;
        int nPixels;
        int[] nDtime;

        int measureBank = 0;
        int doneBank = 1;
        int clearBank = 2;

        bool TagMode = false;

        public ushort[][][,,] FLIMData_Measure;
        public ushort[][][,,] FLIMData_Done;
        public ushort[][][,,] FLIMData_Clear;
        public ushort[][][][,,] memBuffer = new ushort[nBank][][][,,];

        public object[] memObj = new object[nBank];
        bool task_started = false;
        Task memoryMakeTask;

        public MemoryManager(int ch_in, int z_in, int y_in, int x_in, int[] dtime_in)
        {
            for (int i = 0; i < memObj.Length; i++)
                memObj[i] = new object();

            InitializeMemory(ch_in, z_in, y_in, x_in, dtime_in);
        }

        public void InitializeMemory(int ch_in, int z_in, int y_in, int x_in, int[] dtime_in)
        {
            nChannels = ch_in;
            nZlocs = z_in;
            nLines = y_in;
            nPixels = x_in;
            nDtime = (int[])dtime_in.Clone();

            TagMode = (z_in > 1);

            measureBank = 0;
            task_started = false;

            bool[] eraseMemory = Enumerable.Repeat<bool>(true, nChannels).ToArray();

            for (int i = 0; i < nBank; i++)
                clearMemoryBank(i, allChannelBool(true));

            SetMemoryBankNamesByMeasurement(0, allChannelBool(true));
        }

        public void EraseMeasurementMemory(bool[] erase)
        {
            clearMemoryBank(measureBank, erase);
            clearMemoryBank(clearBank, erase);
        }

        public bool[] allChannelBool(bool bool1)
        {
            return Enumerable.Repeat<bool>(bool1, nChannels).ToArray();
        }

        public void SetMemoryBankNamesByMeasurement(int bank, bool[] erase)
        {
            measureBank = bank % nBank;
            clearBank = (bank + 1) % nBank;
            doneBank = (bank + 2) % nBank;


            lock (memObj[0])
                lock (memObj[1])
                    lock (memObj[2])
                    {
                        if (FLIMData_Measure == null || FLIMData_Measure.Length != nChannels)
                        {
                            FLIMData_Measure = new ushort[nChannels][][,,];
                            FLIMData_Done = new ushort[nChannels][][,,];
                            FLIMData_Clear = new ushort[nChannels][][,,];
                        }

                        for (int ch = 0; ch < nChannels; ch++)
                        {
                            if (erase[ch])
                            {
                                FLIMData_Measure[ch] = memBuffer[measureBank][ch];
                                FLIMData_Done[ch] = memBuffer[doneBank][ch];
                                FLIMData_Clear[ch] = memBuffer[clearBank][ch];
                            }
                        }
                    }

        }

        public void AddToPixelsBlock(uint[] channels, uint[] zlocs, uint[] ylocs, uint[] xlocs, uint[] tlocs, int startP, int endP)
        {
            if (startP > endP)
            {
                int tmpP = startP;
                startP = endP;
                endP = tmpP;
            }

            lock (FLIMData_Measure)
            {
                for (int i = startP; i < endP; i++)
                {
                    AddToPixel(channels[i], zlocs[i], ylocs[i], xlocs[i], tlocs[i]);
                }
            }
        }

        public void AddToPixel(uint channel, uint zloc, uint yloc, uint xloc, uint tloc)
        {
            if (xloc < nPixels) //&& yloc < nLines && channel < nChannels && )
            {
                if (zloc < nZlocs)
                {
                    FLIMData_Measure[channel][zloc][yloc, xloc, tloc]++;
                }
            }
        }

        public void CopyDataLinesFromMeasureBank(ushort[][][,,] destination, int startLine, int endLine)
        {
            int nLine = endLine - startLine;
            for (int ch = 0; ch < nChannels; ch++)
            {
                if (nDtime[ch] != 0)
                {
                    for (int z = 0; z < nZlocs; z++)
                    {
                        Array.Copy(FLIMData_Measure[ch][z], startLine * nPixels * nDtime[ch], destination[ch][z], startLine * nPixels * nDtime[ch], nPixels * nDtime[ch] * nLine);
                    }
                }
            }
        }

        public ushort[][][,,] GetData(int bank)
        {
            if (bank == 0)
                return FLIMData_Measure;
            else if (bank == 2)
                return FLIMData_Clear;
            else
                return FLIMData_Done;
        }

        public void makeMatrix5D(ref ushort[][][,,] data5D, bool[] clearChannel)
        {
            if (data5D == null || data5D.Length != nChannels)
                data5D = new ushort[nChannels][][,,];

            for (int ch = 0; ch < nChannels; ch++)
            {
                if (clearChannel[ch])
                {
                    if (nDtime[ch] > 0)
                    {
                        if (data5D[ch] != null
                            && data5D[ch].Length == nZlocs
                            && data5D[ch][0].GetLength(0) == nLines
                            && data5D[ch][0].GetLength(1) == nPixels
                            && data5D[ch][0].GetLength(2) == nDtime[ch])
                        {
                            Matrix.MatrixClear4D(data5D[ch]); //avoiding making a lot of array.
                        }
                        else
                        {
                            data5D[ch] = new ushort[nZlocs][,,];
                            for (int z = 0; z < nZlocs; z++)
                            {
                                data5D[ch][z] = new ushort[nLines, nPixels, nDtime[ch]];
                            }
                            //data5D[ch] = Matrix.MatrixCreate4D<ushort>(nZlocs, nLines, nPixels, nDtime[ch]);
                            //Debug.WriteLine("Memory created ");
                        }
                    }
                    else //nDtime[ch] == 0
                    {
                        data5D[ch] = null; //if Dtime[ch] == 0, this should be null.
                        nDtime[ch] = 0; // Just in case Dtime[ch] < 0.....
                        //if (data5D[ch] == null)
                        //    data5D[ch] = Matrix.MatrixCreate4D<ushort>(nZlocs, nLines, nPixels, nDtime.Max());
                    }
                }
            } //clearChannel[ch] 
        }

        private void clearMemoryBank(int bank, bool[] erase)
        {
            lock (memObj[bank])
            {
                makeMatrix5D(ref memBuffer[bank], erase);
            }
        }

        //public void CopyMemoryBank(int fromBank, int toBank, int channel)
        //{
        //    for (int z = 0; z < nZlocs; z++)
        //        Array.Copy(memBuffer[fromBank][channel][z], memBuffer[toBank][channel][z], memBuffer[fromBank][channel][z].Length);
        //}

        public void CopyChannelsFromMeasureToDone(bool[] erase)
        {
            lock (memObj[doneBank])
                lock (memObj[measureBank])
                {
                    for (int ch = 0; ch < nChannels; ch++)
                    {
                        if (!erase[ch] && nDtime[ch] > 0) //!erase[ch] means averaging.
                        {
                            for (int z = 0; z < nZlocs; z++)
                                Array.Copy(FLIMData_Measure[ch][z], FLIMData_Done[ch][z], FLIMData_Measure[ch][z].Length);
                        }
                    }
                }
        }

        public void SwitchMemoryBank(bool[] erase)
        {

            if (task_started && memoryMakeTask != null && !memoryMakeTask.IsCompleted)
            {
                //In case this is not done yet.
                //Debug.WriteLine("Memory clear waiting");
                memoryMakeTask.Wait();
            }

            //Switch the memory bank.
            //only channels that will be cleared
            SetMemoryBankNamesByMeasurement(measureBank + 1, erase); //Measure --> Done. Done --> Clear. Clear --> Measure

            //Memory clear will be done in different thread.
            memoryMakeTask = Task.Factory.StartNew((object obj) =>
            {
                var data = (dynamic)obj;
                clearMemoryBank(data.clearBank1, data.erase); //Note that all channels need to be cleaned.
            }, new { clearBank1 = clearBank, erase = (bool[])erase.Clone() }); //just in case other thread changes clearBank etc.

            task_started = true;

            //need to copy data from done to measure for averaging. 
            CopyChannelsFromMeasureToDone(erase); //For display, t is necessary to copy.

        } //Memory manager.
    }
}
