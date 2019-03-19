
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Stage_Control;

namespace FLIMimage
{
    public class TextServer
    {
        FLIMageMain FLIMage;
        ScanParameters State;
        MotorCtrl motorCtrl;

        FileSystemWatcher watcher;

        object fileOpenLock = new object();

        Timer timer;
        int interval = 1000;

        int LineNumber = 0;
        long currentBit = 0;
        long previousBit = -1;
        int Executed = -1;
        List<String> commandQ = new List<string>();

        //public event FileChanged file_changed;
        //public EventArgs e = EventArgs.Empty;
        //public delegate void FileChanged(TextServer ts, EventArgs e);

        public TextServer(FLIMageMain f)
        {
            FLIMage = f;
            State = f.State;
            motorCtrl = f.motorCtrl;

            timer = new Timer(interval);
            timer.Elapsed += new ElapsedEventHandler(HandleTimer);
            timer.Interval = interval;
            timer.Enabled = true;

            Directory.CreateDirectory(State.Files.commandPathName);

            for (int i = 0; i < 100; i++)
            {
                try
                {
                    File.WriteAllText(Path.Combine(State.Files.commandPathName, State.Files.FLIMageOutputFileName), "");
                    File.WriteAllText(Path.Combine(State.Files.commandPathName, State.Files.ClientOutputFileName), "");
                    break;
                }
                catch
                {
                    System.Threading.Thread.Sleep(20);
                }
            }

            StartNewFileWatcher();
        }


        public void HandleTimer(object source, ElapsedEventArgs e)
        {
            if (State.Files.useCommandFile)
            {
                OpenClientOutputFile();
            }
        }

        public void OpenClientOutputFile()
        {
            try
            {
                lock (fileOpenLock)
                {
                    String fileName = Path.Combine(State.Files.commandPathName, State.Files.ClientOutputFileName);
                    if (File.Exists(fileName))
                    {
                        using (Stream stream = File.OpenRead(fileName))
                        {
                            if (stream.Length > 0)
                            {
                                currentBit = stream.Length;

                                if (currentBit > previousBit)
                                {
                                    //Debug.WriteLine("Previous = {0}, Current = {1}", previousBit, currentBit);

                                    stream.Seek(previousBit + 1, SeekOrigin.Begin);
                                    previousBit = currentBit;

                                    using (StreamReader sr = new StreamReader(stream))
                                    {
                                        string s = "";

                                        while ((s = sr.ReadLine()) != null)
                                        {
                                            commandQ.Add(s);
                                            LineNumber++;
                                        }

                                        sr.Close();
                                    }

                                    if (commandQ.Count > Executed)
                                    {
                                        ExecuteAllLines();
                                    }
                                }
                            }
                            else
                            {
                                previousBit = -1;
                                currentBit = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in Opening command file" + ex.Message);
            }
        }


        public void ExecuteAllLines()
        {
            String allStrClient = "";
            String allStrFLIMage = "";
            int startLine = Executed + 1;

            bool needUpdateFile = false;

            for (int i = startLine; i < commandQ.Count; i++)
            {
                if (i > Executed)
                {
                    String s = commandQ[i];
                    allStrClient = s + "\r\n";

                    String writeString = FLIMage.flim_event.ExecuteReceivedCommand(s, false, out FLIMage_Event.CommandMode cm);

                    if (cm == FLIMage_Event.CommandMode.Get_Parameter || cm == FLIMage_Event.CommandMode.Set_Parameter)
                    {
                        needUpdateFile = true;
                        allStrFLIMage = writeString + "\r\n";
                        WriteEventsInCommandFile(writeString);
                    }

                    Executed = i;                    
                }
            }

            if (needUpdateFile)
                FLIMage.ReSetupValues(true);

            FLIMage.script.displaySendText(allStrFLIMage, FLIMage_Event.CommandReceivedFrom.FLIMage);
            FLIMage.script.displaySendText(allStrClient, FLIMage_Event.CommandReceivedFrom.Client);
        }

        public void StartNewFileWatcher()
        {
            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = State.Files.commandPathName;
            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter = State.Files.ClientOutputFileName; // "*.txt";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);


            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (State.Files.useCommandFile)
            {
                lock (fileOpenLock)
                {
                    if (e.FullPath == Path.Combine(State.Files.commandPathName, State.Files.ClientOutputFileName))
                    {

                        for (int i = 0; i < 5; i++) //Try just 5 times.
                        {
                            try
                            {
                                OpenClientOutputFile();

                                break; //if suceed, it will break;
                            }
                            catch
                            {
                                System.Threading.Thread.Sleep(50); //Retry after 50 ms.
                                Debug.WriteLine("Failed accessing to the file: " + i);
                            }
                        }


                    }
                    else
                    {
                        // Specify what is done when a file is changed, created, or deleted.
                        //Debug.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
                    }
                }
            }
        }

        public void WriteEventsInCommandFile(String writeString)
        {
            if (State.Files.useCommandFile)
            {
                int trialN = 0;
                for (int i = 0; i < 100; i++)
                {
                    try
                    {
                        File.AppendAllText(Path.Combine(State.Files.commandPathName, State.Files.FLIMageOutputFileName), writeString + "\r\n");
                        break;
                    }
                    catch
                    {
                        trialN++;
                        System.Threading.Thread.Sleep(20);
                        Debug.WriteLine("Failed coonection. Trying again... (" + trialN + ")");
                    }
                }
                //FLIMage.script.displaySendText(writeString, FLIMage_Event.CommandReceivedFrom.FLIMage);
            }
        }
    }
}
