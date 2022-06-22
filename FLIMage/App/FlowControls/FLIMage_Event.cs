using FLIMage.HardwareControls;
using FLIMage.HardwareControls.StageControls;
using MicroscopeHardwareLibs.Stage_Contoller;
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
using System.Windows.Forms;
using TCSPC_controls;

namespace FLIMage.FlowControls
{
    public class FLIMage_Event
    {
        FLIMageMain flimage;
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
            flimage = fc;
            com_server = fc.com_server;
            motorCtrl = fc.motorCtrl;
            State = fc.State;
            text_server = fc.text_server;

            flimage.flimage_io.EventNotify += new FLIMage_IO.FLIMage_EventHandler(EventHandling);
            com_server.r_tick += new COMserver.ReadHandler(RemoteEventHandling);

            uf = new UserFunction(fc);

            eventNotifyTable.Columns.Add("ID", typeof(int));
            eventNotifyTable.Columns.Add("Event Name", typeof(string));
            eventNotifyTable.Columns.Add("Notify", typeof(bool));

            eventNotifyTable.Rows.Add(1, "flimageStarted", true);
            eventNotifyTable.Rows.Add(2, "GrabStart", false);
            eventNotifyTable.Rows.Add(3, "GrabAbort", false);
            eventNotifyTable.Rows.Add(4, "AcquisitionDone", true);
            eventNotifyTable.Rows.Add(5, "SliceAcquisitionStart", false);
            eventNotifyTable.Rows.Add(6, "SliceAcquisitionDone", false);
            eventNotifyTable.Rows.Add(7, "FrameAcquisitionDone", false);
            eventNotifyTable.Rows.Add(8, "FocusStart", false);
            eventNotifyTable.Rows.Add(9, "FocusStop", false);
            eventNotifyTable.Rows.Add(10, "UncagingDone", true);
            eventNotifyTable.Rows.Add(11, "DODone", false);
            eventNotifyTable.Rows.Add(12, "EphysDone", true);
            eventNotifyTable.Rows.Add(13, "StageMoveStart", false);
            eventNotifyTable.Rows.Add(14, "StageMoveDone", false);
            eventNotifyTable.Rows.Add(15, "ParametersChanged", false);
            eventNotifyTable.Rows.Add(16, "SaveImageDone", false);
            eventNotifyTable.Rows.Add(17, "ExtCommandExecuted", false);

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
            State = flimage.State;
            //eventNotifyTable.AcceptChanges();
            String listFName = State.Files.commandPathName + Path.DirectorySeparatorChar + State.Files.eventOutputListFileName;

            String str = "flimage notification data, text format\r\n";
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
            State = flimage.State;

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
            flimage.flimage_io.EventNotify -= EventHandling;
        }

        public void EventHandling(FLIMage_IO fc, ProcessEventArgs e)
        {
            State = flimage.State;

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

                if (flimage.script != null)
                {
                    flimage.script.EventHandling(e.EventName); //Display
                    flimage.script.displaySendText(eventStr, CommandReceivedFrom.FLIMage);
                }
            }

            uf.FLIM_EventHandling(fc, e);
        }


        public void UpdateComServerNotConnectedText()
        {
            if (flimage.script != null)
            {
                if (!com_server.connectedR)
                {
                    flimage.script.displayStatusText("PIPE not communicating (flimage to Client) ...", CommandReceivedFrom.FLIMage);
                    com_server.Close();
                }

                if (!com_server.connected)
                {
                    flimage.script.displayStatusText("PIPE not communicating (Client to flimage) ...", CommandReceivedFrom.Client);
                    com_server.Close();
                }
            }
        }

        public String flimageQuery(String s, ref CommandMode cm)
        {
            String replyMessage = "";
            string[] sP = s.Split('.');
            String tempStr;

            String fieldName = sP[1];
            FieldInfo member = flimage.GetType().GetField(sP[1]);

            if (member != null)
            {
                tempStr = member.GetValue(flimage).ToString();
                cm = CommandMode.Get_Parameter;
                replyMessage = "flimage." + fieldName + " = " + tempStr;
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
            State = flimage.State;

            String replyMessage = receivedMessage;
            //String s = Regex.Replace(receivedMessage, @"\s+", ""); //Remove space etc.
            //s = s.Replace(";", "");
            String s = receivedMessage.Replace(";", "");

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
            else if (s.Contains("flimage."))
            {
                string[] sP = s.Split('.');
                if (query)
                {
                    replyMessage = flimageQuery(s, ref cm);
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

                        if (flimage.ExternalCommand(command))
                        {
                            replyMessage = "Done";
                            cm = CommandMode.Execution;
                        }
                        else
                        {
                            replyMessage = flimageQuery(s, ref cm);
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
                        //flimage.ReSetupValues();
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
            } //.State

            if (cm == CommandMode.Set_Parameter && issueUpdateFile)
                flimage.ReSetupValues(false);

            return replyMessage;
        }


        public String Text_EventHandling(String EventStr)
        {
            State = flimage.State;

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
                        writeString = String.Format("{0}, {1}, {2}", EventStr, flimage.flimage_io.internalSliceCounter, State.Acq.nSlices);
                        break;
                    }
                case "FrameAcquisitionDone":
                    {
                        writeString = String.Format("{0}, {1}, {2}", EventStr, flimage.flimage_io.internalFrameCounter, State.Acq.nSlices);
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
            State = flimage.State;

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
                            saveFileParameterTable.Rows[i][2] = flimage.saveIntensityImage ? "1" : "0";
                            break;
                        }
                    case "IntensityFilePath":
                        {
                            int FileCounter = 0;
                            if (flimage.flimage_io.grabbing)
                                FileCounter = State.Files.fileCounter;
                            else
                                FileCounter = State.Files.fileCounter - 1;
                            saveFileParameterTable.Rows[i][2] = flimage.fileIO.FLIM_FilePath(RequestedChannel - 1, ChannelSaveInSeparatedFile, FileCounter, FileIO.ImageType.Intensity, "", State.Files.pathName);
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
            State = flimage.State;

            // int MaxNumArg = 10;
            String[] sP = CommandString.Split(',');
            String CommandInput = sP[0];
            CommandInput = Regex.Replace(CommandInput, @"[\s+]", "");
            //double[] valueStack = new double[MaxNumArg];
            String[] valueStack = new String[sP.Length - 1];

            if (sP.Length > 1)
            {
                for (int i = 1; i < sP.Length; i++)
                {
                    valueStack[i - 1] = sP[i]; // Convert.ToDouble(sP[i]);
                }
            }

            String writeString = CommandInput + " processed.";

            int wait_interval = 100;

            switch (CommandInput)
            {
                case "SetMotorPosition":
                    {
                        double[] values = new double[valueStack.Length];
                        for (int i = 0; i < valueStack.Length; i++)
                            values[i] = Convert.ToDouble(valueStack[i]);
                        //double[] XYZ = motorCtrl.convertToUncalibratedPosition(values, true);
                        double[] motorPos = motorCtrl.getCalibratedAbsolutePosition(); //current position in um
                        double[] tol = new double[] { 0.2, 0.2, 0.05 };

                        Debug.WriteLine("Set motor position to {0}, {1}, {2}", values[0], values[1], values[2]);
                        motorCtrl.SetNewPosition_um(values);
                        flimage.ExternalCommand("SetMotorPosition");
                        motorPos = motorCtrl.getCalibratedAbsolutePosition();

                        //for (int i = 0; i < 5; i++)
                        //{
                        //    motorCtrl.SetNewPosition_um(values);
                        //    flimage.ExternalCommand("SetMotorPosition");
                        //    motorCtrl.GetPosition();
                        //    motorPos = motorCtrl.getCalibratedAbsolutePosition();
                        //    if (Math.Abs(motorPos[0] - values[0]) < tol[0] && 
                        //        Math.Abs(motorPos[1] - values[1]) < tol[1] && 
                        //        Math.Abs(motorPos[2] - values[2]) < tol[2])
                        //        break;
                        //}

                        writeString = String.Format("SetMotorPositionDone, {0}, {1}, {2}", motorPos[0], motorPos[1], motorPos[2]);
                        cm = CommandMode.Execution; //It is execution and set parameter.
                        break;
                    }
                case "StartLoop":
                    {
                        flimage.ExternalCommand("StartLoop");
                        cm = CommandMode.Execution;
                        break;
                    }
                case "StopLoop":
                    {
                        flimage.ExternalCommand("StopLoop");
                        cm = CommandMode.Execution;
                        break;
                    }
                case "StartGrab":
                    {
                        flimage.ExternalCommand("StartGrab");
                        cm = CommandMode.Execution;
                        break;
                    }
                case "IsDORunning":
                    {
                        int value = 0;
                        if (flimage.digital_panel != null)
                            value = flimage.digital_panel.digital_running ? 1 : 0;
                        writeString = String.Format("IsDORunning, {0}", value);
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "IsUncaging":
                    {
                        int value = 0;
                        if (flimage.uncaging_panel != null)
                            value = flimage.uncaging_panel.uncaging_running ? 1 : 0;
                        writeString = String.Format("IsUncaging, {0}", value);
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "IsGrabbing":
                    {
                        int value = (flimage.flimage_io.grabbing || flimage.flimage_io.post_grabbing_process == false || flimage.flimage_io.focusing) ? 1 : 0;
                        writeString = String.Format("IsGrabbing, {0}", value);
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "StopGrab":
                    {
                        flimage.ExternalCommand("AbortGrab");
                        cm = CommandMode.Execution;
                        break;
                    }
                case "SetUncagingLocation":
                    {
                        double[] values = new double[valueStack.Length];
                        for (int i = 0; i < valueStack.Length; i++)
                            values[i] = Convert.ToDouble(valueStack[i]);

                        double x = values[0];
                        double y = values[1];

                        flimage.image_display.uncagingLocFrac = HardwareControls.IOControls.PixelsToFracOnScreen(new double[] { x, y }, State);

                        flimage.image_display.uncaging_on = true;
                        flimage.image_display.ActivateUncaging(true);
                        flimage.UpdateUncagingFromDisplay();
                        cm = CommandMode.Set_Parameter;
                        writeString = String.Format("UncagingLocation, {0}, {1}", x, y);
                        break;
                    }
                case "StartUncaging":
                    {
                        double[] values = new double[valueStack.Length];
                        for (int i = 0; i < valueStack.Length; i++)
                            values[i] = Convert.ToDouble(valueStack[i]);

                        double x = values[0];
                        double y = values[1];

                        flimage.image_display.uncagingLocFrac = HardwareControls.IOControls.PixelsToFracOnScreen(new double[] { x, y }, State);

                        flimage.image_display.uncaging_on = true;
                        flimage.image_display.ActivateUncaging(true);
                        flimage.UpdateUncagingFromDisplay();
                        flimage.ExternalCommand("StartUncaging");
                        cm = CommandMode.Execution;
                        break;
                    }
                case "StopUncaging":
                    {
                        flimage.ExternalCommand("StopUncaging");
                        cm = CommandMode.Execution;
                        break;
                    }
                case "SetChannelsToBeSaved":
                    {

                        int channel = Convert.ToInt32(valueStack[0]);
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
                        int onoff = Convert.ToInt32(valueStack[0]);
                        flimage.saveIntensityImage = onoff != 0;
                        cm = CommandMode.Set_Parameter;
                        writeString = String.Format("IntensitySaving, {0}", onoff);
                        break;
                    }
                case "SetZoom":
                    {
                        State.Acq.zoom = Convert.ToDouble(valueStack[0]);
                        writeString = String.Format("Zoom, {0}", State.Acq.zoom);
                        cm = CommandMode.Set_Parameter;
                        break;
                    }
                case "LoadSetting":
                    {
                        flimage.Invoke((Action)delegate
                        {
                            if (int.TryParse(valueStack[0], out int settingN))
                                flimage.ExternalCommand("LoadSettingWithNumber", settingN.ToString());
                            else
                                flimage.ExternalCommand("LoadSettingFile", valueStack[0]);
                        });
                        writeString = String.Format("Setting, {0}", valueStack[0]);
                        break;

                    }
                case "Uncaging":
                    {
                        int value = Convert.ToInt32(valueStack[0]);
                        flimage.State.Uncaging.uncage_whileImage = !(value == 0);
                        cm = CommandMode.Set_Parameter;
                        break;
                    }
                case "GetIntensityFilePath":
                    {
                        writeString = String.Format("IntensityFilePath, {0}", flimage.fileIO.FLIM_FilePath(RequestedChannel - 1, ChannelSaveInSeparatedFile, State.Files.fileCounter - 1, FileIO.ImageType.Intensity, "", State.Files.pathName));
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "GetCurrentPosition":
                case "GetMotorPosition":
                    {
                        motorCtrl.GetPosition();
                        double[] motorPos = motorCtrl.getCalibratedAbsolutePosition();
                        writeString = String.Format("CurrentPosition, {0}, {1}, {2}", motorPos[0], motorPos[1], motorPos[2]);
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "GetCurrentPosition_um":
                    {
                        motorCtrl.GetPosition();
                        double[] motorPos = motorCtrl.getCalibratedAbsolutePosition();
                        double[] offset = ImageParameterCalculation.MirrorOffsetToMicrometers(State);
                        writeString = String.Format("CurrentPosition_um, {0}, {1}, {2}", motorPos[0] + offset[0], 
                            motorPos[1] + offset[1], motorPos[2]);
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "GetFOVXY":
                    {
                        writeString = String.Format("FovXYum, {0}, {1}", State.Acq.field_of_view[0], State.Acq.field_of_view[1]);
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "PixelToVoltage":
                    {
                        double[] pixelOnImage = new double[2];
                        for (int i = 0; i < 2; i++)
                            pixelOnImage[i] = Convert.ToDouble(valueStack[i]);

                        double[] v = HardwareControls.IOControls.pixelOnImageToVoltage(pixelOnImage, State);
                        writeString = String.Format("PixelToVoltage, {0}, {1}", v[0], v[1]);
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "SetFOVXY":
                    {
                        if (valueStack.Length != 2)
                        {
                            writeString = "Error: requires 2 input values";
                            break;
                        }
                        double[] values = new double[valueStack.Length];
                        for (int i = 0; i < valueStack.Length; i++)
                            values[i] = Convert.ToDouble(valueStack[i]);

                        State.Acq.field_of_view[0] = values[0];
                        State.Acq.field_of_view[1] = values[1];
                        writeString = String.Format("FovXYum, {0}, {1}", State.Acq.field_of_view[0], State.Acq.field_of_view[1]);
                        cm = CommandMode.Set_Parameter;
                        break;
                    }
                case "SetScanVoltageXY":
                    {
                        if (valueStack.Length != 2)
                        {
                            writeString = "Error: requires 2 input values";
                            break;
                        }
                        double[] values = new double[2];
                        for (int i = 0; i < valueStack.Length; i++)
                            values[i] = Convert.ToDouble(valueStack[i]);

                        State.Acq.XOffset = values[0];
                        State.Acq.YOffset = values[1];
                        writeString = String.Format("ScanVoltageXY, {0}, {1}", State.Acq.XOffset, State.Acq.YOffset);
                        cm = CommandMode.Set_Parameter;
                        break;
                    }
                case "SetScanMirrorXY_um":
                    {
                        if (valueStack.Length != 2)
                        {
                            writeString = "Error: requires 2 input values";
                            break;
                        }
                        double[] values = new double[2];
                        for (int i = 0; i < valueStack.Length; i++)
                            values[i] = Convert.ToDouble(valueStack[i]);
                        ImageParameterCalculation.MirrorOffsetVoltageFromMicrometers(State, values);
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
                case "GetScanMirrorXY_um":
                    {
                        double[] offset = ImageParameterCalculation.MirrorOffsetToMicrometers(State);
                        writeString = String.Format("ScanMirrorXY_um, {0}, {1}", offset[0], offset[1]);
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
                        State.Acq.nSlices = Convert.ToInt32(valueStack[0]);
                        State.Acq.ZStack = true;
                        writeString = String.Format("ZSliceNum, {0}", State.Acq.nSlices);
                        cm = CommandMode.Set_Parameter;
                        break;
                    }
                case "SetResolutionXY":
                    {
                        int[] values = new int[valueStack.Length];
                        for (int i = 0; i < valueStack.Length; i++)
                            values[i] = Convert.ToInt32(valueStack[i]);

                        State.Acq.pixelsPerLine = (int)values[0];
                        State.Acq.linesPerFrame = (int)values[1];
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
                case "Disconnect":
                    {
                        flimage.script.TurnOnServer(false);
                        cm = CommandMode.None;
                        break;
                    }
                case "GetNPages":
                    {
                        writeString = String.Format("NPages, {0}", flimage.image_display.FLIM_ImgData.n_pages);
                        cm = CommandMode.None;
                        break;
                    }
                case "OpenFile":
                case "ReadImageJROI":
                case "BinFrames":
                case "CalcTimeCourse":
                case "SetFLIMIntensityOffset":
                case "FixTau":
                case "FixTauAll":
                case "SetChannel":
                case "SetFitRange":
                case "AlignFrames":
                case "ApplyFitOffset":
                case "FitEachFrame":
                case "FitData":
                    {
                        String arg;
                        for (int i = 0; i < valueStack.Length; i++)
                            if (valueStack[i] == null)
                            {
                                Array.Resize(ref valueStack, i);
                                break;
                            }
                        arg = String.Join(",", valueStack);
                        flimage.ExternalCommand(CommandInput, arg);
                        cm = CommandMode.Execution;
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
                flimage.ReSetupValues(issueUpdateFile);
            }

            return writeString;

        }


        public void RemoteEventHandling(COMserver cm, EventArgs e)
        {
            if (com_server.connectedR)
            {
                flimage.script.messageReceived(com_server, e);

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
