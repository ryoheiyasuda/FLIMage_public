using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCSPC_controls;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
using MathLibrary;
using System.IO;
using FLIMage.HardwareControls;
using FLIMage.HardwareControls.StageControls;
using MicroscopeHardwareLibs.Stage_Contoller;

namespace FLIMage.FlowControls
{
    public class UserFunction
    {
        FLIMageMain flimage;
        MotorCtrl motorCtrl;
        COMserver com_server;
        ScanParameters State;

        List<int> Markers = new List<int>();

        //Drift parameters
        private UInt16[][] TemplateProject;
        private UInt16[][][] Template3DProject;
        //private int driftCorrectionChannel = 1; // 0 or 1.


        public UserFunction(FLIMageMain fc)
        {
            flimage = fc;
            com_server = fc.com_server;
            motorCtrl = fc.motorCtrl;
            State = fc.State;
        }


        public void MarkerScript(String eventStr)
        {
            try
            {
                if (State.Init.MarkerInput != "")
                {
                    switch (eventStr)
                    {
                        case "GrabStart":
                        case "FocusStart":
                            {
                                Markers.Clear();
                                break;
                            }
                        case "FrameDone":
                            {
                                bool value = readDigitalValue(State.Init.MarkerInput);
                                Debug.WriteLine("Digital input (Frame " + flimage.flimage_io.AO_FrameCounter + ") = " + value);
                                Markers.Add((Convert.ToInt32(value)));
                                break;
                            }
                        case "SaveFileDone":
                            {
                                String dataCSV = List2CSV(Markers);

                                Directory.CreateDirectory(State.Files.pathName + "\\Analysis");
                                String fileName = State.Files.pathName + "\\Analysis\\" + State.Files.baseName + "_Markers.csv";
                                TrySave(fileName, dataCSV);
                                break;
                            }
                    }
                }
            }
            catch (Exception E)
            {
                Debug.WriteLine(E.Message);
            }
        }

        public String List2CSV<T>(List<T> ListA)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < ListA.Count; i++)
            {
                sb.Append(ListA[i]);
                if (i < ListA.Count - 1)
                    sb.Append(",");
            }

            return sb.ToString();
        }


        private void TrySave(string fileName, string allStr)
        {
            try
            {
                File.WriteAllText(fileName, allStr);
            }
            catch (Exception E)
            {
                Debug.WriteLine("Error in saving ..." + E.Message);
                //MessageBox.Show("The excel file: " + fileName + " is locked. Close the file and press OK");
                for (int i = 1; i < 3; i++)
                {
                    try
                    {
                        String fileName1 = Path.GetFileNameWithoutExtension(fileName);
                        fileName = fileName + "(" + i + ").csv";
                        File.WriteAllText(fileName, allStr);
                        break;
                    }
                    catch (Exception E2)
                    {
                        Debug.WriteLine("Error in saving ..." + E2.Message);
                        //MessageBox.Show("The excel file: " + fileName + " is locked. Close the file and press OK");
                    }
                }
            }
        }



        /// <summary>
        /// This event is evoked when FLIMage broadcast the following events in "case".
        /// </summary>
        /// <param name="fc"></param>
        /// FLIMcontrols is the main window of FLIMage.
        /// <param name="e"></param>
        /// This contains information about the event. e.EventName is the name of the event.
        /// There might be data associated.

        public void FLIM_EventHandling(FLIMage_IO fc, ProcessEventArgs e)
        {
            ///////// HERE YOU CAN WRITE WHATEVER.
            String eventStr = e.EventName;

            MarkerScript(eventStr);

            try
            {
                switch (eventStr)
                {
                    case "GrabStart":
                        {
                            break;
                        }
                    case "AcquisitionDone":
                        {
                            break;
                        }
                    case "FrameDone":
                        {
                            break;
                        }
                    case "SaveFrameDone":
                        {
                            break;
                        }
                    case "SaveSliceDone":
                        {
                            break;
                        }
                    case "SaveFileDone":
                        {
                            break;
                        }
                    case "SliceAcquisitionStart":
                        {
                            break;
                        }
                    case "SliceAcquisitionDone":
                        {
                            break;
                        }
                    case "FocusStart":
                        {
                            break;
                        }
                    case "FocusStop":
                        {
                            break;
                        }
                    case "GrabAbort":
                        {
                            break;
                        }
                    case "FLIMageStarted":
                        {
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            catch (Exception E)
            {
                Debug.WriteLine("Problem in user function (" + E.Source + "): " + E.Message);
            }

        }


        public bool readDigitalValue(String port)
        {
            bool value = false;
            if (flimage.flimage_io.use_nidaq)
            {
                IOControls.Digital_In DI = new IOControls.Digital_In(port, out value);
            }
            return value;
        }


    } //class userfuncion
}
