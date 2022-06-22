using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class FileHandling
    {
        static public List<string> GetFiles(string directoryName, string[] searchPatterns)
        {
            var list1 = new List<string>();
            if (Directory.Exists(directoryName))
            {
                foreach (var ext in searchPatterns)
                {
                    var files = Directory.GetFiles(directoryName, ext);
                    list1.AddRange(files);
                }
            }
            return list1;
        }
    }

    public class StringHandling
    {
        public static string ConvertIntArrayToMatLabStyleText(int[] values)
        {
            StringBuilder sb = new StringBuilder();
            int[] valCopy = (int[])values.Clone();
            Array.Sort(valCopy);
            int[] valDiff = new int[valCopy.Length - 1];
            for (int i = 0; i < valCopy.Length - 1; i++)
                valDiff[i] = valCopy[i + 1] - valCopy[i];

            bool continu = false;
            int contigCount = 0;
            int interval = 0;
            sb.Append(valCopy[0]);

            //for example, 0,1,2,4,5,6,7,9 --> 0:2,4:7,9
            //0,2,4,6,7,9 --> 0:2:6,7,9
            for (int i = 0; i < valDiff.Length - 1; i++)
            {
                if (valDiff[i] == valDiff[i + 1]) //i=0->0-1 == 1-2. i=3->3-4==4-5. i=4
                {
                    contigCount++;
                    interval = valDiff[i]; //interval = 1
                    continu = true;
                }
                else //i=1 -> 1-2 != 2-3, i=2 -> 2-3 != 3-4, i=5
                {
                    if (continu) //i=1.
                    {
                        if (interval > 1)
                        {
                            sb.Append(":");
                            sb.Append(interval);
                        }

                        sb.Append(":");
                        sb.Append(valCopy[i + 1]); //i=1, ":2", i=5, ":7"
                    }
                    else //i=2, i=6
                        sb.Append("," + valCopy[i + 1]); //i=2, ",4", i=6, "9".

                    continu = false;
                    contigCount = 0;
                }
            }

            //Last value.
            if (valCopy.Length > 1)
            {
                if (continu)
                {
                    if (interval > 1)
                    {
                        sb.Append(":");
                        sb.Append(interval);
                    }

                    sb.Append(":");
                    sb.Append(valCopy[valCopy.Length - 1]);
                }
                else
                {
                    sb.Append(",");
                    sb.Append(valCopy[valCopy.Length - 1]);
                }
            }

            return sb.ToString();
        }

        public static int ConvertMatlabStyleTextToIntArray(string text, out int[] values)
        {
            string[] sP = text.Split(',', ' ');
            List<int> valList = new List<int>();

            if (sP.Length >= 1)
            {
                for (int i = 0; i < sP.Length; i++)
                {
                    var sPP = sP[i].Split(':');
                    if (sPP.Length == 1)
                    {
                        if (Int32.TryParse(sPP[0], out int val))
                            valList.Add(val);
                        else
                        {
                            values = null;
                            return -1;
                        }
                    }
                    else if (sPP.Length == 2)
                    {
                        if (Int32.TryParse(sPP[0], out int initialV) &&
                        Int32.TryParse(sPP[1], out int endV))
                        {
                            if (initialV < endV)
                            {
                                for (int j = initialV; j <= endV; j++)
                                    valList.Add(j);
                            }
                            else if (initialV == endV)
                            {
                                valList.Add(initialV);
                            }
                            else
                            {
                                for (int j = endV; j <= initialV; j++)
                                    valList.Add(j);
                            }
                        }
                        else
                        {
                            values = null;
                            return -1;
                        }
                    }
                    else if (sPP.Length == 3)
                    {
                        if (Int32.TryParse(sPP[0], out int initialV) && Int32.TryParse(sPP[2], out int endV)
                            && Int32.TryParse(sPP[1], out int interval))
                        {
                            if (initialV < endV && interval > 0)
                            {
                                for (int j = initialV; j <= endV; j += Math.Abs(interval))
                                    valList.Add(j);
                            }
                            else if (initialV == endV)
                            {
                                valList.Add(initialV);
                            }
                            else
                            {
                                for (int j = endV; j <= initialV; j += Math.Abs(interval))
                                    valList.Add(j);
                            }
                        }
                        else
                        {
                            values = null;
                            return -1;
                        }

                    }
                }
            }
            values = valList.ToArray();
            return 0;
        }
    }
}
