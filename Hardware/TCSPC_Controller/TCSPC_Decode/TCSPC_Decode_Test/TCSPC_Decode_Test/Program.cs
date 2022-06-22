using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCSPC_controls;

namespace TCSPC_Decode_Test
{
    class Program
    {

        static void Main(string[] args)
        {
            int device = 0;
            int testDE = 1;

            if (testDE == 1)
            {
                FLIM_Parameters parameters = new FLIM_Parameters();
                DecodeEngineNative de = new DecodeEngineNative(device);
                de.Initialize(new bool[] { true, true }, false, parameters);

                int lineID = de.pm.LineID;
                int tagID = de.pm.TagID;

                Random rnd = new Random();
                var buffer = new uint[10240];
                double speed = 5000; // how fast each epoch proceeds.

                int lineCount = 0;
                uint totalSyncTime = 0;
                uint syncTime = 0;
                uint Warp = 1024;
                uint lineTime = (uint)(parameters.msPerLine / 1000.0 / parameters.spcData.time_per_unit);

                for (int j = 0; j < 1000; j++)
                {
                    int i = 0;
                    do
                    {
                        bool ofl_event = false;
                        bool line_event = false;
                        uint ofl_n_event = 0;
                        uint timePass = (uint)(rnd.NextDouble() * speed); //1023; if 1 MHz, this happens order of 1000.
                        totalSyncTime += timePass;
                        syncTime += timePass;
                        if (syncTime > Warp)
                        {
                            ofl_event = true;
                            ofl_n_event = syncTime / Warp;
                            syncTime = syncTime - (ofl_n_event) * Warp;
                        }

                        //line event should ahppens at msPerLine.
                        if (totalSyncTime >= lineTime)
                        {
                            line_event = true;
                            totalSyncTime -= lineTime;
                        }

                        uint dtime = (uint)(rnd.NextDouble() * 64); //10-24 bit. must be < 32767;
                        uint channel = 0; // < 63. 63 == overflow.
                        uint special = 0; //0 or 1

                        uint sync;

                        if (ofl_event) //overflow.
                        {
                            sync = ofl_n_event;
                            channel = 63;
                            special = 1;
                        }
                        else if (line_event) //Markers
                        {
                            sync = syncTime;
                            channel = (uint)1 << (lineID); //Line
                            special = 1;
                        }
                        else //Photon
                        {
                            sync = syncTime;
                            channel = 0;
                            special = 0;
                        }

                        uint buf = sync + (dtime << 10) + (channel << 25) + (special << 31);

                        sync = buf & 1023;
                        dtime = (buf >> 10) & 32767;
                        channel = (buf >> 25) & 63;
                        special = (buf >> 31) & 1;

                        bool overflow = special == 1 && channel == 0x3F;
                        bool line = special == 1 && !overflow && (((channel >> lineID) & 1) == 1);

                        if (line)
                        {
                            lineCount++;
                            //Console.WriteLine("{0}, ch={1}, {2}, {3}, of = {4}, line = {5}, {6}", sync, channel, dtime, special, overflow, line, lineCount);
                        }
                        buffer[i] = buf;
                        i++;

                        if (ofl_event && line_event && i < buffer.Length) //both happens
                        {
                            channel = (uint)1 << (lineID); //Line
                            special = 1;
                            buf = syncTime + (dtime << 10) + (channel << 25) + (special << 31);
                            buffer[i] = buf;
                            i++;
                        }
                    } while (i < buffer.Length);

                    de.Decode(buffer, buffer.Length);
                }
                Console.WriteLine("Rate = {0} KHz", de.GetKHz());
            }
            else
            {
                MemoryManagerNative mm = new MemoryManagerNative(device, 2, 16, 128, 256, new int[] { 64, 64 }); //1.39 s.
                                                                                                                 //MemoryManager mm = new MemoryManager(2, 16, 128, 256, new int[] { 64, 64 }); //1.7s s.
                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (int rep = 0; rep < 10; rep++)
                {
                    for (int i = 0; i < 10 + rep; i++)
                    {
                        mm.AddToPixel(0, 10, 15, 28, 32);
                    }

                    mm.SwitchMemoryBank(new bool[] { true, true });
                    foreach (int i in new int[] { 1 })
                    {
                        var A = mm.GetData(i); //1 is the done bank.
                        Console.Write("Value = " + A[0][10][15, 28, 32] + ", ");

                    }
                    Console.WriteLine();
                    //if (rep == 2)
                    //    mm.RegisterListener(device); //after rep == 2, it should start.

                    Console.WriteLine("Elapsed time = " + sw.ElapsedMilliseconds / 1000.0);
                }
            }

            Console.ReadLine();
        }
    }
}
