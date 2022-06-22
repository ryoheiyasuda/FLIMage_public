using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TCSPC_controls
{
    public class MemoryManagerNative
    {
        int nChannels;
        int nZlocs;
        int nLines;
        int nPixels;
        int[] nDtime;

        int Device = 0;

        public MemoryManagerNative(int dev, int nCh, int nZ, int nY, int nX, int[] nTime)
        {
            Device = dev;
            nChannels = nCh;
            nZlocs = nZ;
            nLines = nY;
            nPixels = nX;
            nDtime = (int[])nTime.Clone();
            MM_StartMemoryManager(Device, nChannels, nZ, nY, nX, nDtime);
        }

        public void InitializeMemory(int nCh, int nZ, int nY, int nX, int[] nTime)
        {
            nChannels = nCh;
            nZlocs = nZ;
            nLines = nY;
            nPixels = nX;
            nDtime = (int[])nTime.Clone();
            //MM_ClearObject();
            MM_StartMemoryManager(Device, nChannels, nZ, nY, nX, nDtime);
        }

        public void AddToPixelsBlock(uint[] c, uint[] z, uint[] y, uint[] x, uint[] t, int startP, int endP)
        {
            MM_AddToPixelsBlock(Device, c, z, y, x, t, startP, endP);
        }

        public void AddToPixel(uint c, uint z, uint y, uint x, uint t)
        {
            MM_AddToPixel(Device, c, z, y, x, t);
        }

        public void SwitchMemoryBank(bool[] eraseChannel)
        {
            int[] eraseChannelI = convertToIntArray(eraseChannel);
            MM_SwitchMemoryBank(Device, eraseChannelI);
        }

        public ushort GetPixel(uint c, uint z, uint y, uint x, uint t, int bank)
        {
            ushort value = 0;
            MM_GetPixel(Device, c, z, y, x, t, ref value, bank);
            return value;
        }

        public void EraseMeasurementMemory(bool[] eraseChannel)
        {
            int[] eraseChannelI = convertToIntArray(eraseChannel);
            MM_EraseMeasurementMemory(Device, eraseChannelI);
        }

        public int[] convertToIntArray(bool[] array)
        {
            var result = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
                result[i] = array[i] ? 1 : 0;

            return result;
        }

        public void CopyDataLinesFromMeasureBank(ushort[][][,,] destination, int startLine, int endLine)
        {
            int nLine = endLine - startLine;
            for (int c = 0; c < nChannels; c++)
            {
                if (nDtime[c] != 0)
                {
                    for (int z = 0; z < nZlocs; z++)
                    {
                        MM_GetDataLines(Device, destination[c][z], c, z, startLine, endLine, 0);
                    }

                }
            }
        }

        public ushort[][][,,] GetData(int bank)
        {
            var data = new ushort[nChannels][][,,];
            for (int c = 0; c < nChannels; c++)
            {
                if (nDtime[c] != 0)
                {
                    data[c] = new ushort[nZlocs][,,];
                    for (int z = 0; z < nZlocs; z++)
                    {
                        data[c][z] = new ushort[nLines, nPixels, nDtime[c]]; //Should allocate first.
                        MM_GetData(Device, data[c][z], c, z, bank); //copy data in DLL.
                    }
                }
            }
            return data;
        }

        public void Dispose()
        {
            MM_ClearObject(Device);
        }

        ////public StringBuilder str = new StringBuilder(1024);
        //public delegate void SendMessageDelegate(int id, String str);
        //public SendMessageDelegate sm = new SendMessageDelegate(CallbackReturn);

        //static public void CallbackReturn(int id, String str)
        //{
        //    Console.WriteLine(id + ": " + str);
        //}

        //public void RegisterListener(int id)
        //{
        //    MM_sendMessage(id, sm);
        //}


        //[DllImport("TCSPC_Decode.dll", EntryPoint = "MM_RegisterListener", CallingConvention = CallingConvention.Cdecl)]
        //private static extern void MM_sendMessage(int id, SendMessageDelegate message);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "StartMemoryManager", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MM_StartMemoryManager(int id, int nChannels, int nZ, int nY, int nX, int[] nTime);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "AddToPixelsBlock", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MM_AddToPixelsBlock(int id, uint[] c, uint[] z, uint[] y, uint[] x, uint[] t, int startP, int endP);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "AddToPixel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MM_AddToPixel(int id, uint c, uint z, uint y, uint x, uint t);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "SwitchMemoryBank", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MM_SwitchMemoryBank(int id, int[] eraseChannel);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "EraseMeasurementMemory", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MM_EraseMeasurementMemory(int id, int[] eraseChannel);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "GetPixelValue", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MM_GetPixel(int id, uint c, uint z, uint y, uint x, uint t, ref ushort value, int bank);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "GetDataLines", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MM_GetDataLines(int id, ushort[,,] data, int channel, int zloc, int startLine, int endLine, int bank);

        /// <summary>
        /// Copy array from Memory Manager. Bank = 0: Measure, Bank = 1: Done.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="destination"></param>
        /// <param name="channel"></param>
        /// <param name="zloc"></param>
        /// <param name="bank"></param>
        /// <returns></returns>
        [DllImport("TCSPC_Decode.dll", EntryPoint = "GetData", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MM_GetData(int id, ushort[,,] destination, int channel, int zloc, int bank);

        [DllImport("TCSPC_Decode.dll", EntryPoint = "ClearObject", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MM_ClearObject(int id);

    }
}
