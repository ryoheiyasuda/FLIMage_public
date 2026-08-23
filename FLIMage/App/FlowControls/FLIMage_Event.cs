using FLIMage.HardwareControls;
using FLIMage.HardwareControls.StageControls;
using MicroscopeHardwareLibs.Stage_Contoller;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
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

        // ── Command serialization (stage 1) ──────────────────────────────────
        // Every remote command (from the pipe receive thread or from the
        // RemoteControl "Client output" window) is funneled through a single
        // background worker so that ExecuteReceivedCommand never runs on two
        // threads at once. This fixes the pre-existing UI-thread vs receive-thread
        // race and is the foundation for multi-client support (stage 2): the
        // worker stays identical, only the reply routing becomes per-session.
        //
        // Reply routing is owned by the caller: whoever enqueues awaits the
        // returned Task and writes the reply to its own sink (the pipe stream
        // for the receive thread, the UI for the window). The worker only
        // computes the reply string.
        //
        // No command gating is applied here: as today, callers are expected to
        // poll GetAnalysisStatus and avoid sending conflicting commands during a
        // background analysis. Serialization only guarantees one command runs at
        // a time; it does not police what commands are sent.
        // by Kengo(Claude) 06-10-2026
        public struct CommandResult
        {
            public string Reply;
            public CommandMode Mode;
        }

        class CmdJob
        {
            public string Message;
            public TaskCompletionSource<CommandResult> Reply =
                new TaskCompletionSource<CommandResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        readonly BlockingCollection<CmdJob> _cmdQueue = new BlockingCollection<CmdJob>();
        Thread _cmdWorker;

        public FLIMage_Event(FLIMageMain fc)
        {
            flimage = fc;
            com_server = fc.com_server;
            motorCtrl = fc.motorCtrl;
            State = fc.State;
            text_server = fc.text_server;

            flimage.flimage_io.EventNotify += new FLIMage_IO.FLIMage_EventHandler(EventHandling);
            com_server.CommandHandler = RemoteCommandHandling;

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
            eventNotifyTable.Rows.Add(18, "AnalysisDone", true);

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

            StartCommandWorker();
        }

        // ── Command worker (stage 1) ─────────────────────────────────────────
        void StartCommandWorker()
        {
            _cmdWorker = new Thread(CommandWorkerLoop)
            {
                IsBackground = true,
                Name = "PipeCmdWorker"
            };
            _cmdWorker.Start();
        }

        void CommandWorkerLoop()
        {
            foreach (var job in _cmdQueue.GetConsumingEnumerable())
            {
                CommandResult result = new CommandResult { Reply = "", Mode = CommandMode.None };
                try
                {
                    result.Reply = ExecuteReceivedCommand(job.Message, true, out CommandMode cm);
                    result.Mode = cm;
                }
                catch (Exception ex)
                {
                    result.Reply = "Error: " + ex.Message;
                    result.Mode = CommandMode.None;
                }
                job.Reply.TrySetResult(result);
            }
        }

        // Enqueue a command for serialized execution and return a Task that
        // completes with the reply. Every command goes through the single worker;
        // there is no fast path, so ordering and one-at-a-time execution hold for
        // all commands.
        public Task<CommandResult> EnqueueCommand(string message)
        {
            var job = new CmdJob { Message = message };
            try
            {
                _cmdQueue.Add(job);
            }
            catch (InvalidOperationException)
            {
                // Queue already completed (shutting down). Fail gracefully.
                job.Reply.TrySetResult(new CommandResult { Reply = "", Mode = CommandMode.None });
            }
            return job.Reply.Task;
        }

        // Stop the worker and drain the queue. Called during shutdown.
        public void StopCommandWorker()
        {
            try { _cmdQueue.CompleteAdding(); }
            catch (Exception ex) { Debug.WriteLine("StopCommandWorker: " + ex.Message); }
            _cmdWorker?.Join(1000);
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
            com_server.CommandHandler = null;
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
                    com_server.Broadcast(eventStr); //All connected clients receive the event.
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


        // With the multi-client server (stage 2), "no client connected" is a
        // normal state while the accept loops keep listening, so this only
        // updates the status display. The old single-client version also
        // called com_server.Close() here to release the half-open pipe; that
        // would now kill the accept loops and disconnect other clients.
        // by Kengo(Claude) 06-11-2026
        public void UpdateComServerNotConnectedText()
        {
            if (flimage.script != null && !com_server.connected)
            {
                flimage.script.displayStatusText("PIPE not communicating (flimage to Client) ...", CommandReceivedFrom.FLIMage);
                flimage.script.displayStatusText("PIPE not communicating (Client to flimage) ...", CommandReceivedFrom.Client);
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
                if (query)
                {
                    replyMessage = flimageQuery(s, ref cm);
                }
                else
                {
                    int commandStart = s.IndexOf("flimage.", StringComparison.Ordinal) + "flimage.".Length;
                    if (commandStart >= "flimage.".Length && commandStart < s.Length)
                    {
                        String argument = "";
                        String command = s.Substring(commandStart);
                        if (command.Contains("("))
                        {
                            String[] ssP = command.Split(new char[] { '(', ')' });
                            command = ssP[0];
                            if (ssP.Length > 1)
                                argument = ssP[1];
                        }

                        if (flimage.ExternalCommand(command, argument))
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
                        string[] sP = s.Split('=');
                        replyMessage = sP[0] + " = " + tempStr.ToString();
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
                case "ReadFileDone":
                    {
                        writeString = EventStr;
                        String fileName = SaveParameterFile();
                        writeString = String.Format("ReadFileDone");
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
                            saveFileParameterTable.Rows[i][2] = flimage.fileIO.FLIM_FilePath(RequestedChannel - 1, ChannelSaveInSeparatedFile, FileCounter, FileIO.ImageType.Intensity, "", State.Files.pathName, State.Files.baseName, ".tif");
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
                case "MovePiezoStep":
                    {
                        if (flimage.flimage_io.piezo != null)
                        {
                            flimage.ExternalCommand("MovePiezoStep", valueStack[0]);
                            cm = CommandMode.Execution; //It is execution and set parameter.
                            writeString = String.Format("MovePiezoStep, {0}", flimage.flimage_io.piezo.getPosition_um());
                        }
                        else
                        {
                            writeString = "Piezo is not setup";
                        }
                        break;
                    }
                case "SetMotorPosition":
                    {
                        double[] values = new double[valueStack.Length];
                        for (int i = 0; i < valueStack.Length; i++)
                            values[i] = Convert.ToDouble(valueStack[i]);
                        //KENGO BEGIN 8-11-2025
                        //Error handling while motorCtrl is null
                        if (motorCtrl != null)
                        {
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

                            cm = CommandMode.Execution; //It is execution and set parameter.
                            writeString = String.Format("SetMotorPositionDone, {0}, {1}, {2}", motorPos[0], motorPos[1], motorPos[2]);
                        }
                        else
                        {
                            writeString = "Error: Motor is not setup";
                        }
                        //KENGO END
                        break;
                    }
                // Remote command to read calibrated relative XYZ (um) without moving stage or touching other state. Tetsuya 20260503
                case "GetRelativeXYZ":
                    {
                        if (motorCtrl != null && motorCtrl.connected)
                        {
                            double[] r = motorCtrl.getCalibratedRelativePosition();
                            writeString = String.Format("RelativeXYZ, {0}, {1}, {2}", r[0], r[1], r[2]);
                            cm = CommandMode.Get_Parameter;
                        }
                        else
                        {
                            writeString = "Error: Motor is not setup";
                            cm = CommandMode.None;
                        }
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
                case "SetPower":
                    {
                        // SetPower, ch, value  (ch: 1-indexed, value: 0-100)
                        int ch = Convert.ToInt32(valueStack[0]) - 1;
                        int val = Convert.ToInt32(valueStack[1]);
                        if (ch >= 0 && ch < State.Acq.power.Length && val >= 0 && val <= 100)
                        {
                            State.Acq.power[ch] = val;
                            // ReSetupValues calls updateState which zeros EOM via putValue_S_ToStartPos(true).
                            // ParkMirrors must be called AFTER ReSetupValues to override the zeroing.
                            flimage.ReSetupValues(issueUpdateFile);
                            flimage.flimage_io.ResetFocus();
                            if (!flimage.flimage_io.grabbing && !flimage.flimage_io.focusing && !flimage.flimage_io.refocusing)
                                flimage.flimage_io.ParkMirrors(false);
                            writeString = String.Format("Power, {0}, {1}", ch + 1, val);
                            cm = CommandMode.Execution;  // prevent second ReSetupValues at end of switch
                        }
                        else
                            writeString = "Error: invalid channel or value";
                        break;
                    }
                case "GetPower":
                    {
                        // GetPower, ch  (ch: 1-indexed)
                        int ch = Convert.ToInt32(valueStack[0]) - 1;
                        if (ch >= 0 && ch < State.Acq.power.Length)
                        {
                            writeString = String.Format("Power, {0}, {1}", ch + 1, State.Acq.power[ch]);
                            cm = CommandMode.Get_Parameter;
                        }
                        else
                            writeString = "Error: invalid channel";
                        break;
                    }
                case "StartGrab":
                    {
                        flimage.ExternalCommand("StartGrab");
                        cm = CommandMode.Execution;
                        break;
                    }
                case "SetOverwriteWarningOff":
                    {
                        flimage.ExternalCommand("SetOverwriteWarningOff");
                        cm = CommandMode.Execution;
                        break;
                    }
                case "SetOverwriteWarningOn":
                    {
                        flimage.ExternalCommand("SetOverwriteWarningOn");
                        cm = CommandMode.Execution;
                        break;
                    }

                case "StartDO":
                    {
                        flimage.ExternalCommand("StartDO");
                        cm = CommandMode.Execution;
                        break;
                    }

                case "StopDO":
                    {
                        flimage.ExternalCommand("StopDO");
                        cm = CommandMode.Execution;
                        break;
                    }

                case "SetCenter":
                    {
                        flimage.SetCenter();
                        break;
                    }

                case "CreateUncagingLocation":
                    {
                        //Kengo BIGEN 12-1-2023
                        //Extend to take multiple locations
                        //double[] values = new double[valueStack.Length];
                        //for (int i = 0; i < valueStack.Length; i++)
                        //    values[i] = Convert.ToDouble(valueStack[i]);
                        //double x = values[0];
                        //double y = values[1];
                        //double[] Frac = HardwareControls.IOControls.PixelsToFracOnScreen(new double[] { x, y }, State);

                        //flimage.image_display.uncagingLocs.Add(new double[] {Frac[0], Frac[1]});
                        //flimage.UpdateUncagingFromDisplay();
                        //flimage.image_display.DrawImages_public();
                        //writeString = String.Format("CreateUncagingLoc, {0}, {1}", Frac[0], Frac[1]);
                        if (valueStack.Length % 2 != 0 || valueStack.Length == 0)
                            break;
                        String s = "";
                        for (int i = 0; i < valueStack.Length; i += 2)
                        {
                            double x = Convert.ToDouble(valueStack[i]);
                            double y = Convert.ToDouble(valueStack[i + 1]);
                            double[] Frac = HardwareControls.IOControls.PixelsToFracOnScreen(new double[] { x, y }, State);
                            flimage.image_display.uncagingLocs.Add(new double[] { Frac[0], Frac[1] });
                            s += String.Format(", {0}, {1}", Frac[0], Frac[1]);
                        }
                        flimage.UpdateUncagingFromDisplay();
                        flimage.image_display.DrawImages_public();
                        writeString = "CreateUncagingLoc" + s;
                        //Kengo END
                        break;
                    }

                case "ClearUncagingLocation":
                    {
                        flimage.image_display.uncagingLocs.Clear();
                        flimage.UpdateUncagingFromDisplay();
                        flimage.image_display.DrawImages_public();
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
                        int value = (flimage.flimage_io.grabbing || flimage.flimage_io.focusing) ? 1 : 0;
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
                        writeString = String.Format("IntensityFilePath, {0}", flimage.fileIO.FLIM_FilePath(RequestedChannel - 1, ChannelSaveInSeparatedFile, State.Files.fileCounter - 1, FileIO.ImageType.Intensity, "", State.Files.pathName, State.Files.baseName, ".tif"));
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                case "GetCurrentPosition":
                case "GetMotorPosition":
                    {
                        //KENGO BEGIN 8-11-2025
                        //Error handling while motorCtrl is null
                        if (motorCtrl != null)
                        {
                            motorCtrl.GetPosition();
                            double[] motorPos = motorCtrl.getCalibratedAbsolutePosition();
                            writeString = String.Format("CurrentPosition, {0}, {1}, {2}", motorPos[0], motorPos[1], motorPos[2]);
                            cm = CommandMode.Get_Parameter;
                        }
                        else
                        {
                            writeString = "Error: Motor is not setup";
                        }
                        //KENGO END
                        break;
                    }
                case "GetCurrentPosition_um":
                    {
                        //KENGO BEGIN 8-11-2025
                        //Error handling while motorCtrl is null
                        if (motorCtrl != null)
                        {
                            motorCtrl.GetPosition();
                            double[] motorPos = motorCtrl.getCalibratedAbsolutePosition();
                            double[] offset = ImageParameterCalculation.MirrorOffsetToMicrometers(State);
                            writeString = String.Format("CurrentPosition_um, {0}, {1}, {2}", motorPos[0] + offset[0], 
                                motorPos[1] + offset[1], motorPos[2]);
                            cm = CommandMode.Get_Parameter;
                        }
                        else
                        {
                            writeString = "Error: Motor is not setup";
                        }
                        //KENGO END
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
                // Kengo BEGIN 12-30-2025
                case "GetCurrentPage":
                    {
                        if (flimage.image_display.displayZProjection)
                            writeString = String.Format("CurrentPage, {0} - {1}", flimage.image_display.FLIM_ImgData.ZProjection_Range[0] + 1, flimage.image_display.FLIM_ImgData.ZProjection_Range[1]);
                        else
                            writeString = String.Format("CurrentPage, {0}", flimage.image_display.FLIM_ImgData.currentPage + 1);
                        cm = CommandMode.None;
                        break;
                    }
                // END
                case "GetFullFileName":
                    {
                        writeString = String.Format("FullFileName, {0}", flimage.image_display.FLIM_ImgData.fullFileName);
                        cm = CommandMode.None;
                        break;
                    }
                case "GetFileCounter":
                    {
                        writeString = String.Format("FileCounter, {0}", flimage.image_display.FLIM_ImgData.fileCounter);
                        cm = CommandMode.None;
                        break;
                    }
                case "GetMemoryInfo":
                    {
                        // Managed heap, private bytes, and time-course retention counters for leak diagnosis.
                        long managedBytes = GC.GetTotalMemory(false);
                        long privateBytes = Process.GetCurrentProcess().PrivateMemorySize64;
                        int tcfFileCount = 0;
                        int tcImInfoCount = 0;
                        try
                        {
                            var tcf = flimage.image_display.TCF;
                            if (tcf != null && tcf.TCF != null)
                            {
                                tcfFileCount = tcf.TCF.Count;
                                tcImInfoCount = tcf.TCF.Sum(t => t.ImInfos == null ? 0 : t.ImInfos.Count);
                            }
                        }
                        catch { }
                        writeString = String.Format("MemoryInfo, {0}, {1}, {2}, {3}, {4}",
                            managedBytes, privateBytes, tcfFileCount, tcImInfoCount,
                            flimage.analyzeAfterEachAcquisition ? 1 : 0);
                        cm = CommandMode.None;
                        break;
                    }
                case "ForceGC":
                    {
                        long before = GC.GetTotalMemory(false);
                        System.Runtime.GCSettings.LargeObjectHeapCompactionMode =
                            System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        long after = GC.GetTotalMemory(true);
                        writeString = String.Format("ForceGC, {0}, {1}", before, after);
                        cm = CommandMode.None;
                        break;
                    }
                case "GetAnalyzeAfterAcq":
                    {
                        writeString = String.Format("AnalyzeAfterAcq, {0}", flimage.analyzeAfterEachAcquisition ? 1 : 0);
                        cm = CommandMode.None;
                        break;
                    }
                case "GetCurrentChannel":
                    {
                        writeString = String.Format("CurrentChannel, {0}", flimage.image_display.currentChannel + 1);
                        cm = CommandMode.None;
                        break;
                    }
                case "GetVersion":
                    {
                        writeString = String.Format("Version, {0}", flimage.versionText);
                        cm = CommandMode.None;
                        break;
                    }
                case "GetAnalysisStatus":
                    {
                        writeString = String.Format("GetAnalysisStatus, {0}", flimage.image_display.AnalysisStatus); // Kengo 05-23-2025 change the reply format
                        cm = CommandMode.None;
                        break;
                    }
                //KENGO BEGIN 05-21-2025
                //To get last value of realtime plot 
                case "GetRealtimeValue":
                    {
                        double value = 0;
                        if (flimage.image_display.realtimeData.Count > 0)
                            value = flimage.image_display.realtimeData[flimage.image_display.realtimeData.Count - 1];
                        writeString = String.Format("GetRealtimeValue, {0}", value);
                        cm = CommandMode.Get_Parameter;
                        break;
                    }
                //Kengo END
                //Tetsuya 6-9-2024
                case "MotorDisconnect":
                    {
                        motorCtrl.disconnect();
                        writeString = String.Format("Motor control disconnected");
                        break;
                    }
                case "MotorReopen":
                    {
                        motorCtrl.reopen();
                        writeString = String.Format("Motor control reopened");
                        break;
                    }
                // END Tetsuya 6-9-2024
                case "OpenFile":
                case "ReadImageJROI":
                //Kengo BEGIN 05-19-2025 Add remote commands
                case "TranslateFrames":
                case "SaveImageJROI":
                case "SaveROIs":
                case "RecoverROIs":
                case "RemoveAllROIs":
                case "ShiftAllROIs":
                case "BatchProcessing":
                //Kengo END
                case "BinFrames":
                case "CalcTimeCourse":
                case "SetFLIMIntensityOffset":
                case "SetMinFLIMIntensity":
                case "SetMaxFLIMIntensity":
                case "FixTau":
                case "FixTauAll":
                case "SetChannel":
                case "SetFitRange":
                case "AlignFrames":
                case "ApplyFitOffset":
                case "FitEachFrame":
                case "FitData":
                case "SetFitParams":
                case "HoldThisImage":
                case "ConcatenateImages":
                case "SaveCurrentImage":
                case "ExportCurrentIntensityImageInTIFF":
                //Kengo BEGIN 05-22-2025 Add
                case "Focus": 
                case "StopMotor":
                case "SetZeroAll":
                case "SetAnalyzeAfterAcq":
                case "SaveSetting":
                case "DeleteCurrentPage":
                case "BlankCurrentPage":
                case "ExtractPages":
                //END
                //KENGO BEGIN 12-30-2025 add
                case "SetPages":
                case "SetFileCounter":
                case "ResetTimeCourse": // KENGO 1-4-2026
                case "CalcCurrentPage":
                //END
                case "SetDIOPanel":
                    {
                        String arg;
                        Console.WriteLine("set dio executed");
                        for (int i = 0; i < valueStack.Length; i++)
                            if (valueStack[i] == null)
                            {
                                Array.Resize(ref valueStack, i);
                                break;
                            }
                        arg = String.Join(",", valueStack);
                        if (!flimage.ExternalCommand(CommandInput, arg))
                            writeString = CommandString + ": Invalid";
                        cm = CommandMode.Execution;
                        break;
                    }
                ///tetsuya 12/20/2024
                case "ReadRois":
                    {
                        flimage.image_display.ReadRois(true);
                        flimage.image_display.DrawImages_public();
                        break;
                    }
                /// end tetsuya
                default:
                    {
                        writeString = CommandString + ": Invalid Command"; //Kengo ADD 05-19-2025
                        cm = CommandMode.None;
                        break;
                    }
            }

            Debug.WriteLine(writeString);

            if (cm == CommandMode.Set_Parameter) // || cm == CommandMode.Get_Parameter)
            {
                flimage.ReSetupValues(issueUpdateFile);
            }

            return writeString;

        }


        // Called by COMserver from each client session's receive thread
        // (possibly several threads concurrently). Routes execution through
        // the serializing worker; blocking here is fine (one thread per
        // client) and keeps the request/reply contract: COMserver writes the
        // returned reply back on the pipe of the client that sent the command.
        // by Kengo(Claude) 06-11-2026
        public String RemoteCommandHandling(String receivedMessage)
        {
            flimage.script?.messageReceived(receivedMessage);
            return EnqueueCommand(receivedMessage).GetAwaiter().GetResult().Reply;
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
