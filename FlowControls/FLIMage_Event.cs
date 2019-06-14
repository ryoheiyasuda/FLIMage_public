using TCSPC_controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FLIMage.HardwareControls.StageControls;

namespace FLIMage.FlowControls
{
    public class FLIMage_Event
    {
        FLIMageMain FLIMage;
        COMserver com_server;
        MotorCtrl motorCtrl;
        ScanParameters State;
        UserFunction uf;
        TextServer text_server;

        bool ChannelSaveInSeparatedFile = false;
        int RequestedChannel = -1;

        //List<String> eventList;
        //bool[] eventNotify;

        public DataTable eventNotifyTable = new DataTable();
        public DataTable saveFileParameterTable = new DataTable();

        public FLIMage_Event(FLIMageMain fc)
        {
            FLIMage = fc;
            com_server = fc.com_server;
            motorCtrl = fc.motorCtrl;
            State = fc.State;
            text_server = fc.text_server;

            FLIMage.flimage_io.EventNotify += new FLIMage_IO.FLIMage_EventHandler(EventHandling);
            com_server.r_tick += new COMserver.ReadHandler(RemoteEventHandling);

            uf = new UserFunction(fc);

            eventNotifyTable.Columns.Add("ID", typeof(int));
            eventNotifyTable.Columns.Add("Event Name", typeof(string));
            eventNotifyTable.Columns.Add("Notify", typeof(bool));

            eventNotifyTable.Rows.Add(1, "FLIMageStarted", true);
            eventNotifyTable.Rows.Add(2, "GrabStart", false);
            eventNotifyTable.Rows.Add(3, "GrabAbort", false);
            eventNotifyTable.Rows.Add(4, "AcquisitionDone", true);
            eventNotifyTable.Rows.Add(5, "SliceAcquisitionStart", false);
            eventNotifyTable.Rows.Add(6, "SliceAcquisitionDone", false);
            eventNotifyTable.Rows.Add(7, "FrameAcquisitionDone", false);
            eventNotifyTable.Rows.Add(8, "FocusStart", false);
            eventNotifyTable.Rows.Add(9, "FocusStop", false);
            eventNotifyTable.Rows.Add(10, "UncagingDone", true);
            eventNotifyTable.Rows.Add(11, "UncagingStart", false);
            eventNotifyTable.Rows.Add(12, "StageMoveStart", false);
            eventNotifyTable.Rows.Add(13, "StageMoveDone", true);
            eventNotifyTable.Rows.Add(14, "ParametersChanged", false);
            eventNotifyTable.Rows.Add(15, "SaveImageDone", false);

            saveFileParameterTable.Columns.Add("CommandName", typeof(string));
            saveFileParameterTable.Columns.Add("NArguments", typeof(int));
            saveFileParameterTable.Columns.Add("Parameter1", typeof(String));
            saveFileParameterTable.Columns.Add("Parameter2", typeof(String));
            saveFileParameterTable.Columns.Add("Parameter3", typeof(String));

            saveFileParameterTable.Rows.Add("CurrentPosition", 3, "", "", "");
            saveFileParameterTable.Rows.Add("FOVXYum", 2, "", "", "");
            saveFileParameterTable.Rows.Add("ScanVoltageXY", 2, "", "", "");
            saveFileParameterTable.Rows.Add("ScanVoltageMultiplier", 2, "", "", "");
            saveFileParameterTable.Rows.Add("ScanVoltageRangeReference", 2, "", "", "");
            saveFileParameterTable.Rows.Add("UncagingLocation", 2, "", "", "");
            saveFileParameterTable.Rows.Add("Zoom", 1, "", "", "");
            saveFileParameterTable.Rows.Add("ZSliceNum", 1, "", "", "");
            saveFileParameterTable.Rows.Add("ResolutionXY", 2, "", "", "");
            saveFileParameterTable.Rows.Add("Rotation", 1, "", "", "");
            saveFileParameterTable.Rows.Add("ZStep", 1, "", "", "");
            saveFileParameterTable.Rows.Add("IntensitySaving", 1, "", "", "");
            saveFileParameterTable.Rows.Add("IntensityFilePath", 1, "", "", "");
            saveFileParameterTable.Rows.Add("ChannelsToBeSaved", 1, "", "", "");

            String listFName = State.Files.commandPathName + Path.DirectorySeparatorChar + State.Files.eventOutputListFileName;
            if (!File.Exists(listFName))
            {
                WriteEventNotifyList();
            }
            else
            {
                ReadEventNotifyList();
            }
        }

        public void WriteEventNotifyList()
        {
            State = FLIMage.State;
            //eventNotifyTable.AcceptChanges();
            String listFName = State.Files.commandPathName + Path.DirectorySeparatorChar + State.Files.eventOutputListFileName;

            String str = "FLIMage notification data, text format\r\n";
            for (int i = 0; i < eventNotifyTable.Rows.Count; i++)
            {
                String str1 = String.Format("{0}, notify = {1}", eventNotifyTable.Rows[i][1], eventNotifyTable.Rows[i][2]);
                str = str + str1 + "\r\n";
            }
            File.WriteAllText(listFName, str);

        }

        public bool IfNotify(String EventName)
        {
            bool notify = false;
            for (int i = 0; i < eventNotifyTable.Rows.Count; i++)
            {
                if (EventName == (String)eventNotifyTable.Rows[i][1])
                {
                    notify = (bool)eventNotifyTable.Rows[i][2];
                    break;
                }
            }
            return notify;
        }

        public void ReadEventNotifyList()
        {
            State = FLIMage.State;

            String allText;
            String listFName = State.Files.commandPathName + Path.DirectorySeparatorChar + State.Files.eventOutputListFileName;
            if (File.Exists(listFName))
            {
                allText = File.ReadAllText(listFName);
                String[] sP = allText.Split('\n');
                for (int i = 0; i < sP.Length; i++)
                {
                    String[] ssP = sP[i].Split(',');

                    if (ssP.Length > 1)
                    {
                        String EventName = ssP[0];
                        bool EventNotify1 = ssP[1].ToLower().Contains("true");
                        for (int j = 0; j < eventNotifyTable.Rows.Count; j++)
                        {
                            if (EventName == (String)eventNotifyTable.Rows[j][1])
                            {
                                eventNotifyTable.Rows[j][0] = j; //ID.
                                eventNotifyTable.Rows[j][2] = EventNotify1;
                                break;
                            }
                        }
                    }
                }
                eventNotifyTable.AcceptChanges();
            }

            WriteEventNotifyList();
        }

        public void UnSubscribe()
        {
            com_server.r_tick -= RemoteEventHandling;
            FLIMage.flimage_io.EventNotify -= EventHandling;
        }

        public void EventHandling(FLIMage_IO fc, ProcessEventArgs e)
        {
            State = FLIMage.State;

            String eventStr = e.EventName;

            if (IfNotify(eventStr))
            {
                if (com_server.connected || State.Files.useCommandFile)
                {
                    eventStr = Text_EventHandling(eventStr);
                }

                if (com_server.connected)
                    com_server.sendCommand(eventStr);
                else
                    UpdateComServerNotConnectedText();

                if (State.Files.useCommandFile)
                    text_server.WriteEventsInCommandFile(eventStr); //Write.

                if (FLIMage.script != null)
                {
                    FLIMage.script.EventHandling(e.EventName); //Display
                    FLIMage.script.displaySendText(eventStr, CommandReceivedFrom.FLIMage);
                }
            }

            uf.FLIM_EventHandling(fc, e);
        }


        public void UpdateComServerNotConnectedText()
        {
            if (FLIMage.script != null)
            {
                if (!com_server.connectedR)
                    FLIMage.script.displayStatusText("PIPE not communicating (FLIMage out) ...", CommandReceivedFrom.FLIMage);

                if (!com_server.connected)
                    FLIMage.script.displayStatusText("PIPE not communicating (Client out) ...", CommandReceivedFrom.Client);
            }
        }

        public String FLIMageQuery(String s, ref CommandMode cm)
        {
            String replyMessage = "";
            string[] sP = s.Split('.');
            String tempStr;

            String fieldName = sP[1];
            FieldInfo member = FLIMage.GetType().GetField(sP[1]);

            if (member != null)
            {
                tempStr = member.GetValue(FLIMage).ToString();
                cm = CommandMode.Get_Parameter;
                replyMessage = "FLIMage." + fieldName + " = " + tempStr;
            }
            else
            {
                replyMessage = "Invalid";
                cm = CommandMode.None;
            }

            return replyMessage;
        }

        public String MotorQuery(String s, ref CommandMode cm)
        {
            String replyMessage = "";
            string[] sP = s.Split('.');
            String tempStr;

            String fieldName = sP[1];
            FieldInfo member = motorCtrl.GetType().GetField(sP[1]);

            if (member != null)
            {
                tempStr = member.GetValue(motorCtrl).ToString();
                cm = CommandMode.Get_Parameter;
                replyMessage = "Motor." + fieldName + " = " + tempStr;
            }
            else
            {
                replyMessage = "Invalid";
                cm = CommandMode.None;
            }

            return replyMessage;
        }


        public String ExecuteReceivedCommand(String receivedMessage, bool issueUpdateFile, out CommandMode cm)
        {
            State = FLIMage.State;

            String replyMessage = receivedMessage;
            String s = Regex.Replace(receivedMessage, @"\s+", ""); //Remove space etc.
            s = s.Replace(";", "");

            cm = CommandMode.None;

            replyMessage = String_Execute_and_Response(s, issueUpdateFile, ref cm);

            if (cm != CommandMode.None)
                return replyMessage;

            FileIO fio = new FileIO(State);

            String tempStr = "";

            bool query = false;

            if (s.Contains("?"))
            {
                query = true;
                s = s.Replace("?", "");
            }

            if (s.Contains("Motor.") && !s.Contains("State."))
            {
                string[] sP = s.Split('.');
                if (sP.Length > 1)
                {
                    if (query)
                    {
                        replyMessage = MotorQuery(s, ref cm);
                    }
                    else
                    {
                        replyMessage = MotorQuery(s, ref cm);

                        if (cm == CommandMode.Execution)
                            replyMessage = s + ": Done";
                    }
                }
            }
            //else if (s.Contains("Data.Intensity"))
            //{
            //    replyMessage = s;
            //}
            else if (s.Contains("FLIMage."))
            {
                string[] sP = s.Split('.');
                if (query)
                {
                    replyMessage = FLIMageQuery(s, ref cm);
                }
                else
                {
                    if (sP.Length > 1)
                    {
                        String argument = "";
                        String command = sP[1];
                        if (sP[1].Contains("("))
                        {
                            String[] ssP = sP[1].Split(new char[] { '(', ')' });
                            command = ssP[0];
                            argument = ssP[1];
                        }

                        if (FLIMage.ExternalCommand(command))
                        {
                            replyMessage = "Done";
                            cm = CommandMode.Execution;
                        }
                        else
                        {
                            replyMessage = FLIMageQuery(s, ref cm);
                        }
                    }
                    else
                        replyMessage = "Invalid";
                }
            }
            else if (s.Contains("State."))
            {
                if (query)
                {
                    tempStr = fio.ExecuteLine(s, false);
                    if (tempStr != "")
                    {
                        replyMessage = s + " = " + tempStr.ToString();
                        cm = CommandMode.Get_Parameter;
                    }
                    else
                    {
                        replyMessage = "Invalid";
                        cm = CommandMode.None;
                    }
                }
                else
                {
                    if (s.Contains("="))
                    {
                        fio.ExecuteLine(s);
                        //FLIMage.ReSetupValues();
                        cm = CommandMode.Set_Parameter;
                    }
                    else
                    {
                        cm = CommandMode.Get_Parameter;
                    }

                    tempStr = fio.ExecuteLine(s, false);
                    if (tempStr != "")
                    {
                        replyMessage = s + " = " + tempStr.ToString();
                    }
                    else
                    {
                        replyMessage = "Invalid";
                        cm = CommandMode.None;
                    }
                }
            }

            if (cm == CommandMode.Set_Parameter && issueUpdateFile)
                FLIMage.ReSetupValues(false);

            return replyMessage;
        }


        public String Text_EventHandling(String EventStr)
        {
            State = FLIMage.State;

            bool ifParameterSave = IfNotify("ParametersChanged");
            String writeString = EventStr;
            switch (EventStr)
            {
                case "AcquisitionDone":
                    {
                        writeString = EventStr;
                        String fileName = SaveParameterFile();
                        writeString = String.Format("AcquisitionDone");
                        if (ifParameterSave)
                            writeString = writeString + String.Format("{0}ParameterFileSaved, {1}", "\r\n", fileName);
                        break;
                    }
                case "StageMoveDone": //NEED TO IMPLEMENT!!
                    {
                        if (motorCtrl != null)
                        {
                            double[] motorPos = motorCtrl.getCalibratedAbsolutePosition();
                            String fileName = SaveParameterFile();
                            writeString = String.Format("StageMoveDone, {0}, {1}, {2}", motorPos[0], motorPos[1], motorPos[2]);
                            if (ifParameterSave)
                                writeString = writeString + String.Format("{0}ParameterFileSaved, {1}", "\r\n", fileName);
                        }
                        break;
                    }
                case "SliceAcquisitionDone":
                    {
                        writeString = String.Format("{0}, {1}, {2}", EventStr, FLIMage.flimage_io.internalSliceCounter, State.Acq.nSlices);
                        break;
                    }
                case "FrameAcquisitionDone":
                    {
                        writeString = String.Format("{0}, {1}, {2}", EventStr, FLIMage.flimage_io.internalFrameCounter, State.Acq.nSlices);
                        break;
                    }
                case "ParametersChanged":
                    {
                        String fileName = SaveParameterFile();
                        writeString = String.Format("ParameterFileSaved, {0}", fileName);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return (writeString);
            //WriteEventsInCommandFile(writeString);
        }

        public void UpdateAllParameters()
        {
            State = FLIMage.State;

            for (int i = 0; i < saveFileParameterTable.Rows.Count; i++)
            {
                switch ((String)saveFileParameterTable.Rows[i][0])
                {
                    case "CurrentPosition":
                        {
                            if (motorCtrl != null)
                            {
                                double[] motorPos = motorCtrl.getCalibratedAbsolutePosition();
                                for (int j = 0; j < 3; j++)
                                    saveFileParameterTable.Rows[i][j + 2] = motorPos[j].ToString();
                            }
                            break;
                        }
                    case "FOVXYum":
                        {
                            for (int j = 0; j < 2; j++)
                                saveFileParameterTable.Rows[i][j + 2] = State.Acq.field_of_view[j].ToString();
                            break;
                        }
                    case "ScanVoltageXY":
                        {
                            saveFileParameterTable.Rows[i][2] = State.Acq.XOffset.ToString();
                            saveFileParameterTable.Rows[i][3] = State.Acq.YOffset.ToString();
                            break;
                        }
                    case "ScanVoltageMultiplier":
                        {
                            for (int j = 0; j < 2; j++)
                                saveFileParameterTable.Rows[i][j + 2] = State.Acq.scanVoltageMultiplier[j].ToString();
                            break;
                        }
                    case "ScanVoltageRangeReference":
                        {
                            saveFileParameterTable.Rows[i][2] = State.Acq.XMaxVoltage.ToString();
                            saveFileParameterTable.Rows[i][3] = State.Acq.YMaxVoltage.ToString();
                            break;
                        }
                    case "UncagingLocation":
                        {
                            int nPixels = Math.Max(State.Acq.pixelsPerLine, State.Acq.linesPerFrame);
                            double startX = (nPixels - State.Acq.pixelsPerLine) / 2.0;
                            double startY = (nPixels - State.Acq.linesPerFrame) / 2.0;
                            int uncagingLocX = (int)(State.Uncaging.Position[0] * nPixels - startX);
                            int UncagingLocY = (int)(State.Uncaging.Position[1] * nPixels - startY);
                            saveFileParameterTable.Rows[i][2] = uncagingLocX.ToString();
                            saveFileParameterTable.Rows[i][3] = UncagingLocY.ToString();
                            break;
                        }
                    case "Zoom":
                        {
                            saveFileParameterTable.Rows[i][2] = State.Acq.zoom.ToString();
                            break;
                        }
                    case "Rotation":
                        {
                            saveFileParameterTable.Rows[i][2] = State.Acq.Rotation.ToString();
                            break;
                        }
                    case "ZStep":
                        {
                            saveFileParameterTable.Rows[i][2] = State.Acq.sliceStep.ToString();
                            break;
                        }
                    case "ZSliceNum":
                        {
                            saveFileParameterTable.Rows[i][2] = State.Acq.nSlices.ToString();
                            break;
                        }
                    case "ResolutionXY":
                        {
                            saveFileParameterTable.Rows[i][2] = State.Acq.pixelsPerLine.ToString();
                            saveFileParameterTable.Rows[i][3] = State.Acq.linesPerFrame.ToString();
                            break;
                        }
                    case "IntensitySaving":
                        {
                            saveFileParameterTable.Rows[i][2] = FLIMage.saveIntensityImage ? "1" : "0";
                            break;
                        }
                    case "IntensityFilePath":
                        {
                            int FileCounter = 0;
                            if (FLIMage.flimage_io.grabbing)
                                FileCounter = State.Files.fileCounter;
                            else
                                FileCounter = State.Files.fileCounter - 1;
                            saveFileParameterTable.Rows[i][2] = FLIMage.fileIO.FLIM_FilePath(RequestedChannel - 1, ChannelSaveInSeparatedFile, FileCounter, FileIO.ImageType.Intensity, "", State.Files.pathName);
                            Debug.WriteLine("FileName: " + saveFileParameterTable.Rows[i][2]);
                            break;
                        }
                    case "ChannelsToBeSaved":
                        {
                            saveFileParameterTable.Rows[i][2] = RequestedChannel.ToString();
                            break;
                        }
                }
            }
        }

        public String SaveParameterFile()
        {
            UpdateAllParameters();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < saveFileParameterTable.Rows.Count; i++)
            {
                switch ((int)saveFileParameterTable.Rows[i][1])
                {
                    case 1:
                        {
                            sb.AppendFormat("{0}, {1}", saveFileParameterTable.Rows[i][0], saveFileParameterTable.Rows[i][2]);
                            break;
                        }
                    case 2:
                        {
                            sb.AppendFormat("{0}, {1}, {2}", saveFileParameterTable.Rows[i][0], saveFileParameterTable.Rows[i][2], saveFileParameterTable.Rows[i][3]);
                            break;
                        }
                    case 3:
                        {
                            sb.AppendFormat("{0}, {1}, {2}, {3}", saveFileParameterTable.Rows[i][0], saveFileParameterTable.Rows[i][2], saveFileParameterTable.Rows[i][3], saveFileParameterTable.Rows[i][4]);
                            break;
                        }
                    default:
                        {
                            sb.AppendFormat(saveFileParameterTable.Rows[i][0] + ": Error");
                            break;
                        }
                }
                sb.AppendLine();
            }

            String filePath = Path.Combine(State.Files.commandPathName, State.Files.parameterFile);
            File.WriteAllText(filePath, sb.ToString());
            return filePath;
        }

        public String String_Execute_and_Response(String CommandString, bool issueUpdateFile, ref CommandMode cm)
        {
            State = FLIMage.State;

            int MaxNumArg = 3;
            String[] sP = CommandString.Split(',');
            String CommandInput = sP[0];
            double[] valueStack = new double[MaxNumArg];

            if (sP.Length > 1)
            {
                for (int i = 1; i < sP.Length; i++)
                {
                    valueStack[i - 1] = Convert.ToDouble(sP[i]);
                }
            }


            String writeString = "";
            switch (CommandInput)
            {
                case "SetMotorPosition":
                    {
                        double[] XYZ = motorCtrl.convertToUncalibratedPosition(valueStack, true);
                        motorCtrl.SetNewPosition(XYZ);
                        FLIMage.ExternalCommand("SetMotorPosition");
                        cm = CommandMode.Execution; //It is execution and set parameter.
                        break;
                    }
                case "StartLoop":
                    {
                        FLIMage.ExternalCommand("StartLoop");
                        cm = CommandMode.Execution;
                        break;
                    }
                case "StopLoop":
                    {
                        FLIMage.ExternalCommand("StopLoop");
                        cm = CommandMode.Execution;
                        break;
                    }
                case "StartGrab":
                    {
                        FLIMage.ExternalCommand("StartGrab");
                        cm = CommandMode.Execution;
                        break;
                    }
                case "SetUncagingLocation":
                    {
                        double x = valueStack[0];
                        double y = valueStack[1];

                        FLIMage.image_display.uncagingLocFrac = IOControls.PixelsToFracOnScreen(new double[] { x, y }, State);

                        FLIMage.image_display.uncaging_on = true;
                        FLIMage.image_display.ActivateUncaging();
                        FLIMage.UpdateUncagingFromDisplay();
                        cm = CommandMode.Set_Parameter;
                        writeString = String.Format("UncagingLocation, {0}, {1}", x, y);
                        break;
                    }
                case "StartUncaging":
                    {
                        double x = valueStack[0];
                        double y = valueStack[1];

                        FLIMage.image_display.uncagingLocFrac = IOControls.PixelsToFracOnScreen(new double[] { x, y }, State);

                        FLIMage.image_display.uncaging_on = true;
                        FLIMage.image_display.ActivateUncaging();
                        FLIMage.UpdateUncagingFromDisplay();
                        FLIMage.ExternalCommand("StartUncaging");
                        cm = CommandMode.Execution;
                        break;
                    }
                case "SetChannelsToBeSaved":
                    {
                        int channel = (int)valueStack[0];
                        if (channel > 0 && State.Acq.nChannels >= channel)
                        {
                            RequestedChannel = channel;
                            ChannelSaveInSeparatedFile = true;
                        }
                        else
                        {
                            ChannelSaveInSeparatedFile = false;
                            channel = -1;
                        }
                        cm = CommandMode.Set_Parameter;
                        writeString = String.Format("ChannelsToBeSaved, {0}", channel);
                        break;
                    }
                case "SetIntensitySaving":
                    {
                        int onoff = (int)valueStack[0];
                        FLIMage.saveIntensityImage = onoff != 0;
                        cm = CommandMode.Set_Parameter;
                        writeString = String.Format("IntensitySaving, {0}", onoff);
                        break;
                    }
                case "SetZoom":
                    {
                        State.Acq.zoom = valueStack[0];
                        writeString = String.Format("Zoom, {0}", State.Acq.zoom);
                        cm = CommandMode.Set_Parameter;
                        break;
                    }
                case "LoadSetting":
                    {
                        FLIMage.ExternalCommand("LoadSettingWithNumber", Math.Round(valueStack[0]).ToString());
                        writeString = String.Format("Setting, {0}", valueStack[0]);
                        break;
                    }
                case "GetIntensityFilePath":
                    {
                        writeString = String.Format("IntensityFilePath, {0}", FLIMage.fileIO.FLIM_FilePath(RequestedChannel - 1, ChannelSaveInSeparatedFile, State.Files.fileCounter - 1, FileIO.ImageType.Intensity, "", State.Files.pathName));
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "GetCurrentPosition":
                    {
                        motorCtrl.GetPosition();
                        double[] motorPos = motorCtrl.getCalibratedAbsolutePosition();
                        writeString = String.Format("CurrentPosition, {0}, {1}, {2}", motorPos[0], motorPos[1], motorPos[2]);
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "GetFOVXY":
                    {
                        writeString = String.Format("FovXYum, {0}, {1}", State.Acq.field_of_view[0], State.Acq.field_of_view[1]);
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "SetFOVXY":
                    {
                        State.Acq.field_of_view[0] = valueStack[0];
                        State.Acq.field_of_view[1] = valueStack[1];
                        writeString = String.Format("FovXYum, {0}, {1}", State.Acq.field_of_view[0], State.Acq.field_of_view[1]);
                        cm = CommandMode.Set_Parameter;
                        break;
                    }
                case "SetScanVoltageXY":
                    {
                        State.Acq.XOffset = valueStack[0];
                        State.Acq.YOffset = valueStack[1];
                        writeString = String.Format("ScanVoltageXY, {0}, {1}", State.Acq.XOffset, State.Acq.YOffset);
                        cm = CommandMode.Set_Parameter;
                        break;
                    }
                case "GetScanVoltageXY":
                    {
                        writeString = String.Format("ScanVoltageXY, {0}, {1}", State.Acq.XOffset, State.Acq.YOffset);
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "GetScanVoltageMultiplier":
                    {
                        writeString = String.Format("ScanVoltageMultiplier, {0}, {1}", State.Acq.scanVoltageMultiplier[0], State.Acq.scanVoltageMultiplier[1]);
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "GetScanVoltageRangeReference":
                    {
                        writeString = String.Format("ScanVoltageRangeReference, {0}, {1}", State.Acq.XMaxVoltage, State.Acq.YMaxVoltage); //slow, fast?
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "SetZSliceNum":
                    {
                        State.Acq.nSlices = (int)valueStack[0];
                        State.Acq.ZStack = true;
                        writeString = String.Format("ZSliceNum, {0}", State.Acq.nSlices);
                        cm = CommandMode.Set_Parameter;
                        break;
                    }
                case "SetResolutionXY":
                    {
                        State.Acq.pixelsPerLine = (int)valueStack[0];
                        State.Acq.linesPerFrame = (int)valueStack[1];
                        writeString = String.Format("ResolutionXY, {0}, {1}", State.Acq.pixelsPerLine, State.Acq.linesPerFrame);
                        cm = CommandMode.Set_Parameter;
                        break;
                    }
                case "GetResolutionXY":
                    {
                        writeString = String.Format("ResolutionXY, {0}, {1}", State.Acq.pixelsPerLine, State.Acq.linesPerFrame);
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                default:
                    {
                        cm = CommandMode.None;
                        break;
                    }
            }

            Debug.WriteLine(writeString);

            if (cm == CommandMode.Set_Parameter || cm == CommandMode.Get_Parameter)
            {
                FLIMage.ReSetupValues(issueUpdateFile);
            }

            return writeString;

        }


        public void RemoteEventHandling(COMserver cm, EventArgs e)
        {
            if (com_server.connectedR)
            {
                FLIMage.script.messageReceived(com_server, e);

                String receivedMessage = com_server.ReceivedR;

                CommandMode command_mode;
                String replyMessage = ExecuteReceivedCommand(receivedMessage, true, out command_mode);

                if (com_server.ssR.WriteString(replyMessage) == 0) //Sending message to COM server.
                {
                    com_server.connectedR = false;
                }
            }

            UpdateComServerNotConnectedText();
        }

        public enum CommandReceivedFrom
        {
            FLIMage = 1,
            Client = 2,
        }

        public enum CommandMode
        {
            Execution = 1,
            Get_Parameter = 2,
            Set_Parameter = 3,
            None = 4,
        }
    }
}
