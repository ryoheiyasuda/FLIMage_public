using MathLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCSPC_controls;
using FLIMage.Analysis;
using FLIMage.HardwareControls.StageControls;
using FLIMage.Uncaging;

namespace FLIMage
{
    public class FLIMage_IO
    {
        bool DEBUGMODE = false;

        FLIMageMain FLIMage;
        public ScanParameters State;

        //Timer to acquire count rate.
        Timer RateTimer = new Timer();

        public bool use_nidaq = true; //use NIDAQ card
        public bool use_pq = true; // use PicoQuant card
        public bool use_bh = true; // use Becker and Hickl card

        public Stopwatch UIstopWatch_Loop = new Stopwatch();
        public Stopwatch UIstopWatch_Image = new Stopwatch();
        public Stopwatch UIstopWatch_Frame = new Stopwatch();
        public Stopwatch SW_PerformanceMonitor = new Stopwatch();

        //For synchronization.
        object syncFLIMacq = new object();
        object saveBufferObj = new object();
        object syncFLIMdisplay = new object();
        object syncFLIMsave = new object();
        object toolTipObj = new object();
        object waitSliceTaskobj = new object();
        object waitLoopImageTaskobj = new object();


        public bool imageSequencing = false;
        public bool focusing = false;
        public bool grabbing = false;
        public bool looping = false;
        public bool allowLoop = true;
        public bool stopGrabActivated = false;
        public bool force_stop = false;
        public bool runningImgAcq = false;

        //////Counters for frames etc.
        //  int n_average;

        public bool[] newFile = new bool[] { false, false }; //For saving.

        public int averageCounter;
        public int averageSliceCounter;
        public int internalFrameCounter;
        // int internalAveFrameCounter;
        public int internalSliceCounter;
        public int internalImageCounter;

        //// Serve as an acquisiton counter. Event activated by NI mirror card.
        public int AO_FrameCounter;
        // int internalStripeCounter;
        public int savePageCounterTotal = 0;
        public int savePageCounter = 0;
        public int displayPageCounterTotal = 0;
        public int displayPageCounter = 0;
        public int deletedPageCounter = 0;
        public double measuredSliceInterval;
        Task waitSlice, waitImage;
        Task saveTask;
        public bool save_image_busy = false;
        bool waitingImageTask = false;
        bool waitingSliceTask = false;

        ////// FLIM data stroage. All temporal. 
        List<UInt16[][][]> FLIMSaveBuffer = new List<ushort[][][]>(); //Used only when images are not saved in memory.
        List<DateTime> acquiredTimeList = new List<DateTime>(); //Store acquisition time info.
        DateTime acquiredTime;
        public FLIMData FLIM_ImgData;

        //National instrument DAQ card 
        public IOControls.lineClock lineClock;
        public IOControls.MirrorAO mirrorAO;
        public IOControls.pockelAO pockelAO;
        public IOControls.MirrorAO mirrorAO_S; //parking mirror
        public IOControls.dioTrigger dioTrigger;
        public IOControls.ShutterCtrl ShutterCtrl;
        public IOControls.DigitalIn DI;
        public IOControls.AO_Write UncagingShutterAO; //This is static.
        //public IOControls.DigitalUncagingShutterSignal UncagingShutter_DO; //for time control
        public IOControls.DigitalUncagingShutterSignal UncagingShutter_DO_S; //_S for static shutter control.
        public IOControls.DigitalLineClock dLineClock; //for time control

        ////// Uncaging prameters.
        public UncagingCalibration uc;
        Timer UncagingTimer = new Timer();
        public double[] uncaging_Calib = new double[2];
        int uncaging_SliceCounter = 0;


        ///// Shaidng
        public IOControls.Shading shading;

        ///////FLIM card
        public FiFio_multiBoards FiFo_acquire;
        public FLIM_Parameters parameters;

        //////Rate
        int badSyncRateCounter = 0;
        int badSyncRateMaxCount = 10;

        //////  Event handler for broadcasting.
        public event FLIMage_EventHandler EventNotify;
        public ProcessEventArgs envt = new ProcessEventArgs("", null);
        public delegate void FLIMage_EventHandler(FLIMage_IO flimage_io, ProcessEventArgs evnt);

        public FileIO fileIO;

        public FLIMage_IO(FLIMageMain FLIMage)
        {
            State = FLIMage.State;
            fileIO = new FileIO(State); //initialize fileIO

            use_nidaq = State.Init.NIDAQ_on;
            use_pq = State.Init.FLIM_on && (String.Equals(State.Init.FLIM_mode, "PQ") || String.Equals(State.Init.FLIM_mode, "MH"));
            use_bh = State.Init.FLIM_on && (String.Equals(State.Init.FLIM_mode, "BH"));

            if (use_nidaq)
            {
                try
                {
                    IOControls.exportBaseClockSignal(State);
                    lineClock = new IOControls.lineClock(State, false);
                    dLineClock = new IOControls.DigitalLineClock(State);

                    dioTrigger = new IOControls.dioTrigger(State);
                    ShutterCtrl = new IOControls.ShutterCtrl(State);
                    shading = new IOControls.Shading(State);
                    mirrorAO = new IOControls.MirrorAO(State, shading);
                    mirrorAO_S = new IOControls.MirrorAO(State, shading);

                    if (State.Init.EOM_nChannels > 0)
                        pockelAO = new IOControls.pockelAO(State, shading, true);

                    if (State.Init.DO_uncagingShutter)
                    {
                        //UncagingShutter_DO = new IOControls.DigitalUncagingShutterSignal(State);
                        UncagingShutter_DO_S = new IOControls.DigitalUncagingShutterSignal(State);
                    }

                    if (State.Init.AO_uncagingShutter)
                        UncagingShutterAO = new IOControls.AO_Write(State.Init.UncagingShutterAnalogPort, 0);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Problem in loading NIDAQmx: " + ex.Message);
                    use_nidaq = false;
                }
            }

            parameters = new FLIM_Parameters();

            if (use_bh || use_pq)
            {
                parameters.ComputerID = State.Init.ComputerID;
                parameters.FLIMserial = State.Init.FLIMserial;
                parameters.spcData = State.Spc.spcData; //linked together.
                parameters.BoardType = State.Init.FLIM_mode;
                FiFo_acquire = new FiFio_multiBoards(parameters);
                State.Init.ComputerID = parameters.ComputerID;

                FiFio_multiBoards.ErrorCode error = FiFo_acquire.Initialize();
                //SetupFLIMParameters(); included in FiFo_acquire

                if (error != FiFio_multiBoards.ErrorCode.NONE)
                {
                    use_bh = false;
                    use_pq = false;
                }

                SetupTimerRate();

                bool serialError = (error == FiFio_multiBoards.ErrorCode.COMPUTERID_INCORRECT);

                if (error == FiFio_multiBoards.ErrorCode.NONE)
                {
                    FiFo_acquire.FrameDone += new FiFio_multiBoards.FrameDoneHandler(BackGroundFLIMAcq);
                    FiFo_acquire.StripeDone += new FiFio_multiBoards.StripeDoneHandler(StripeDoneEvent);
                    FiFo_acquire.MeasDone += new FiFio_multiBoards.MeasDoneHandler(MeasDoneEvent);
                    //FiFo_acquire.SetupParameters(focusing, parameters);

                    FLIMage.UpdateSPC_GUI(this);
                }

                if (serialError)
                {
                    MessageBox.Show("Wrong FLIM Serial number!! FLIMage DLL was not loaded");
                    File.WriteAllText(State.Files.deviceFileName, fileIO.AllSetupValues_device());
                }
            }

            if (!use_bh && !use_pq)
            {
                State.Init.FLIM_on = false;
            }
        }

        public void PostFLIMageShowInitialization(FLIMageMain flim_in)
        {
            FLIMage = flim_in;

            if (use_nidaq)
            {
                CalibEOM(false); //should be in flim_io.
            }

            if (use_nidaq)
            {
                if (State.Init.UseExternalMirrorOffset)
                {
                    IOControls.AO_Write aoX = new IOControls.AO_Write(State.Init.mirrorOffsetX, State.Acq.XOffset);
                    IOControls.AO_Write aoY = new IOControls.AO_Write(State.Init.mirrorOffsetY, State.Acq.YOffset);
                }
            }

            InitializeCounter(); //Counter reset.
        }

        public void InitializeCounter()
        {
            averageCounter = 0;
            averageSliceCounter = 0;

            internalSliceCounter = 0;
            internalFrameCounter = 0;

            //internalStripeCounter = 0;
            //internalAveFrameCounter = 0;

            if (FLIMage.uncaging_panel != null)
                FLIMage.uncaging_panel.uncaging_count = 0;

            uncaging_SliceCounter = 0;
            savePageCounterTotal = 0;
            savePageCounter = 0;
            displayPageCounter = 0;
            displayPageCounterTotal = 0;

            newFile = boolAllChannels(true);

            if (FLIMage != null)
            {
                if (!FLIMage.InvokeRequired)
                    FLIMage.InitializeCounter_GUI_Update();
                else
                {
                    FLIMage.Invoke((Action)delegate
                    {
                        FLIMage.InitializeCounter_GUI_Update();
                    });
                }
            }

        }

        /// <summary>
        /// Making the event handler for RateTimer. 
        /// </summary>
        void SetupTimerRate()
        {
            RateTimer.Tick += new EventHandler(TimerEventProcessorRate); //Attach function
            RateTimer.Interval = 1000;
            RateTimer.Start();
        }

        public void updateState(ScanParameters State_in)
        {
            State = State_in;
            EventNotify?.Invoke(this, new ProcessEventArgs("ParametersChanged", null));
        }

        public void Notify(ProcessEventArgs e)
        {
            EventNotify?.Invoke(this, e);
        }

        /// <summary>
        /// Called by NIDAQ mirror AO using the FrameDone event listener, when a frame is done. 
        /// See IOcontrolls.cs for the listener setup.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void mirrorAOFrameDoneEvent(object o, EventArgs e)
        {
            AO_FrameCounter++;
            //EventNotify?.Invoke(this, new ProcessEventArgs("FrameScanDone", (object)AO_FrameCounter));

            if (AO_FrameCounter <= State.Acq.nFrames)
            {
                FLIMage.BeginInvoke((Action)delegate
                {
                    FLIMage.AO_FrameUpdate(AO_FrameCounter);
                });
            }
        }

        /// <summary>
        /// Start new measurment --- designed for TagLens parameter measurement. 
        /// </summary>
        /// <param name="eraseAllMemory"></param>
        /// <param name="focusing"></param>
        /// <param name="measure_tagParameters"></param>
        public void FiFo_StartNew(bool eraseAllMemory, bool focusing, bool measure_tagParameters)
        {
            bool[] erase = boolAllChannels(eraseAllMemory);
            FiFo_acquire.StartMeas(erase, focusing, measure_tagParameters);
        }

        public void ResetFocus()
        {
            if (focusing)
            {
                StopFocus();
                System.Threading.Thread.Sleep(100);
                StartGrab(true);
            }
        }

        /// <summary>
        /// Time Function to update Sync Rate and photon counting rate.
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="myEventArgs"></param>
        public void TimerEventProcessorRate(Object myObject, EventArgs myEventArgs)
        {
            bool badRate = (badSyncRateCounter > badSyncRateMaxCount);

            if (use_bh || use_pq) //!runningImgAcq)
            {
                int ret = -1;

                double syncRate1 = 0;
                double syncRate2 = 0;

                ret = FiFo_acquire.GetRate();

                if (ret == 0)
                {
                    //State.Spc.datainfo = PQ_acquire.State.Spc.datainfo;
                    State.Spc.datainfo.syncRate = (int[])parameters.rateInfo.syncRate.Clone();
                    State.Spc.datainfo.countRate = (int[])parameters.rateInfo.countRate.Clone();

                    syncRate1 = State.Spc.datainfo.syncRate[0]/1e6;
                    syncRate2 = State.Spc.datainfo.syncRate[1]/1e6;

                    if (syncRate1 > State.Acq.ExpectedLaserPulseRate_MHz * 0.9
                        && syncRate1 < State.Acq.ExpectedLaserPulseRate_MHz * 1.1
                        && syncRate2 > State.Acq.ExpectedLaserPulseRate_MHz * 0.9
                        && syncRate2 < State.Acq.ExpectedLaserPulseRate_MHz * 1.1 || focusing)
                    {
                        badSyncRateCounter = 0;
                    }
                    else
                    {
                        badSyncRateCounter++; // laserWarningButton.Visible = true;
                    }
                }
            }

            if (FLIMage != null)
                FLIMage.RateTimerEvent_GUI_Update(badRate);
        }



        public void CalibEOM(bool plot)
        {
            if (use_nidaq)
            {
                if (State.Init.openShutterDuringCalibration)
                    ShutterCtrl.open();

                bool[] success = shading.calibration.calibrateEOM(plot);
                shading.applyCalibration(State);

                if (FLIMage.fastZcontrol != null && FLIMage.fastZcontrol.Visible)
                {
                    /////
                }
                else
                    ShutterCtrl.close();

                FLIMage.CalibEOM_GUI_Update(success);

            } //nidaq
        }

        public void StopRateTimer()
        {
            if (use_pq || use_bh)
            {
                RateTimer.Stop();
                RateTimer.Dispose();
                System.Threading.Thread.Sleep(50);
            }
        }

        public void TCSPC_Close()
        {
            StopRateTimer();
            FiFo_acquire.closeDevice();
        }

        void waitForAcquisitionTaskCompleted()
        {
            //There is no "wait" function in FiFO_acquire.
            if (FiFo_acquire != null)
            {
                System.Threading.Thread.Sleep(5);
                while (!FiFo_acquire.IsCompleted())
                    System.Threading.Thread.Sleep(5);
            }
        }

        //
        public void StartDAQ(bool[] eraseSPCmemory, bool putValue, bool recordTriggerTime)
        {
            runningImgAcq = true;
            waitForAcquisitionTaskCompleted();

            if (FiFo_acquire == null)
                return;

            if (use_nidaq)
            {
                if (putValue)
                {
                    //Is this better?
                    lineClock.dispose();
                    lineClock = new IOControls.lineClock(State, focusing);

                    dLineClock.dispose();
                    dLineClock = new IOControls.DigitalLineClock(State);
                    dLineClock.PutValue_and_Start(State.Acq.externalTrigger);

                    mirrorAO.dispose();
                    mirrorAO = new IOControls.MirrorAO(State, shading);
                    mirrorAO.FrameDone += new IOControls.MirrorAO.FrameDoneHandler(mirrorAOFrameDoneEvent);


                    if (State.Init.EOM_nChannels > 0 && !mirrorAO.SameBoard)
                    {
                        pockelAO.dispose();
                        pockelAO = new IOControls.pockelAO(State, shading, true);
                    }

                    if (!focusing && State.Uncaging.sync_withFrame && State.Uncaging.uncage_whileImage)
                    {
                        mirrorAO.putValueScanAndUncaging();
                        if (State.Init.EOM_nChannels > 0 && !mirrorAO.SameBoard)
                            pockelAO.putValueScanAndUncaging();

                    }
                    else
                    {
                        bool uncaging_shutter = false;
                        if (FLIMage.uncaging_panel != null)
                        {
                            uncaging_shutter = FLIMage.uncaging_panel.UncagingShutter;
                        }

                        try
                        {
                            mirrorAO.putValueScan(focusing, uncaging_shutter, true);
                        }
                        catch (Exception EX)
                        {
                            Debug.WriteLine("mirrorAO error !" + EX.Message);
                        }

                        if (State.Init.EOM_nChannels > 0 && !mirrorAO.SameBoard)
                            pockelAO.putValueScan(focusing, uncaging_shutter, true);

                        if (State.Init.DO_uncagingShutter)
                        {
                            uncagingShutterCtrl(uncaging_shutter, false, true);
                        }

                    }

                    FLIM_ImgData.copyState(State);

                    if (State.Init.EOM_nChannels > 0 && !mirrorAO.SameBoard)
                        pockelAO.start(State.Acq.externalTrigger);

                    bool success = mirrorAO.start(State.Acq.externalTrigger);
                    lineClock.start(State.Acq.externalTrigger);
                }
            }

            FiFo_acquire.StartMeas(eraseSPCmemory, focusing, false);

            if (use_nidaq)
            {
                if (putValue)
                {
                    ShutterCtrl.open();
                    System.Threading.Thread.Sleep(2); //shutter open.

                    if (!State.Acq.externalTrigger)
                        dioTrigger.Evoke();

                    DateTime at = new DateTime();
                    at = DateTime.Now;

                    if (eraseSPCmemory.Any(x => x == true))
                    {
                        acquiredTime = at;
                        Debug.WriteLine("Triggered..." + acquiredTime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                    }

                    //if (eraseSPCmemory || internalSliceCounter == 0)
                    //{
                    //    acquiredTime = at;
                    //    Debug.WriteLine("Triggered..." + acquiredTime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                    //}
                    if (recordTriggerTime)
                    {
                        State.Acq.triggerTime = acquiredTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");
                        FLIM_ImgData.State.Acq.triggerTime = State.Acq.triggerTime;
                    }

                } //put value
            } //nidaq
        }

        /// <summary>
        /// Actual program for start grab.
        /// </summary>
        /// <param name="focus"></param>
        public void StartGrab(bool focus)
        {
            FLIMage.ImageDisplayOpen();
            FLIMage.GetParametersFromGUI(this); //Setup all parameters.

            if (State.Uncaging.uncage_whileImage && FLIMage.uncaging_panel != null)
                FLIMage.uncaging_panel.SetupUncage(this);

            if (State.Acq.XOffset > State.Acq.XMaxVoltage || State.Acq.YOffset > State.Acq.YMaxVoltage)
            {
                MessageBox.Show("Offset exceeds maximum voltage!!");
                return;
            }

            FLIMage.SetupFLIMParameters(); //this will setup parameters for FLIM card..

            //Initialize FLIM_ImgData and fileIO, that reflects State.
            if (FLIMage.fastZcontrol != null)
                FLIMage.fastZcontrol.ControlsDuringScanning(true);

            List<ROI> ROIs = FLIM_ImgData.ROIs;
            FLIM_ImgData.InitializeData(State);
            FLIM_ImgData.ROIs = ROIs;
            fileIO = new FileIO(State);


            FLIMage.image_display.SetupRealtimeImaging(State, FLIM_ImgData);
            FLIMage.image_display.SetFastZModeDisplay(State.Acq.fastZScan);

            Power_putEOMValues(false);
            runningImgAcq = true;

            savePageCounter = 0;
            displayPageCounter = 0;
            deletedPageCounter = 0;
            AO_FrameCounter = 0;

            if (FLIMage.image_display.plot_realtime.Visible)
                FLIMage.image_display.plot_realtime.WarningTextDisplay("");



            if (focus)
            {
                focusing = true;
                EventNotify?.Invoke(this, new ProcessEventArgs("FocusStart", null));
            }
            else
            {

                grabbing = true;
                focusing = false;
                //RateTimer.Stop();
                EventNotify?.Invoke(this, new ProcessEventArgs("GrabStart", null));
                //EventNotify?.Invoke(this, new ProcessEventArgs("SliceAcquisitionStart", null));

            }

            FLIMage.StarGrab_GUI_Update(focus);

            force_stop = false;
            FLIMSaveBuffer.Clear();
            acquiredTimeList.Clear();

            FLIMage.image_display.InitializeStripeBuffer(State.Acq.nChannels, State.Acq.linesPerFrame, State.Acq.pixelsPerLine);


            //FLIM_ImgData.InitializeData(State);
            //System.GC.SuppressFinalize(FLIM_ImgData);
            //FLIM_ImgData = new FLIMData(State);

            if (!focusing) //Setup FLIM_ImgData mode. ZStack? FastZ?
            {
                FLIM_ImgData.clearMemory();
                FLIM_ImgData.ZStack = (State.Acq.ZStack && State.Acq.nSlices > 1 && State.Acq.sliceStep > 0.0);
                FLIM_ImgData.nFastZ = State.Acq.fastZScan ? State.Acq.FastZ_nSlices : 1;
            }

            InitializeCounter(); //Counters Reset.

            UIstopWatch_Frame.Start();

            System.Threading.Thread.Sleep(10);

            bool[] eraseMemoryA = boolAllChannels(true);

            if (!focus)
            {
                //UIstopWatch.Reset();
                if (internalImageCounter == 0)
                    UIstopWatch_Loop.Reset();

                UIstopWatch_Image.Reset();

                if (FLIMage.physiology != null && FLIMage.physiology.image_trigger_waiting)
                    FLIMage.physiology.StartAcq();

                StartDAQ(eraseMemoryA, true, true); //Including trigger.

                if (internalImageCounter == 0)
                {
                    UIstopWatch_Loop.Start();
                }
                UIstopWatch_Image.Start();

            }
            else
            {
                StartDAQ(eraseMemoryA, true, false); //Including trigger.
            }

            SW_PerformanceMonitor.Start();
        } //Start grab core.



        public void StopNIDAQIOControls()
        {
            if (use_pq || use_bh && runningImgAcq)
            {
                runningImgAcq = false;
                if (grabbing || focusing)
                    StopGrab(true, true);
                System.Threading.Thread.Sleep(50);
            }

            if (dLineClock != null)
                dLineClock.dispose();

            if (lineClock != null)
                lineClock.dispose();

            if (mirrorAO_S != null)
                mirrorAO_S.dispose(); //parking mirror

            if (dioTrigger != null)
                dioTrigger.dispose();

            if (ShutterCtrl != null)
                ShutterCtrl.dispose();

            if (DI != null)
                DI.dispose();

            if (UncagingShutterAO != null)
                UncagingShutterAO.dispose();

            if (UncagingShutter_DO_S != null)
                UncagingShutter_DO_S.dispose();

        }

        /// <summary>
        /// Stop NIDAQ boards used for grabbing.
        /// </summary>
        public void StopDAQ()
        {
            if (use_nidaq)
            {
                if (FLIMage.fastZcontrol != null && FLIMage.fastZcontrol.Visible)
                {
                    //does not do any thing
                }
                else
                    ShutterCtrl.close();

                lineClock.stop();
                dLineClock.Stop();
                mirrorAO.stop();

                if (State.Init.EOM_nChannels > 0)
                    pockelAO.stop();

            }
            runningImgAcq = false;
        }

        public void StopFocus()
        {
            StopGrab(true, true);
        }

        public void StopGrab(bool force)
        {
            if (grabbing)
            {
                EventNotify?.Invoke(this, new ProcessEventArgs("GrabAbort", null));
            }

            StopGrab(force, false);
        }

        public void StopGrab(bool force, bool focusStop)
        {
            force_stop = force;

            if (focusStop)
                stopGrabActivated = false;

            //KillThread(); //Kill threadtask.

            if (FiFo_acquire != null)
                FiFo_acquire.StopMeas(force);

            StopDAQ();
            DisposeDAQ();
            ParkMirrors();
            runningImgAcq = false;

            if (!looping || stopGrabActivated)
            {
                UIstopWatch_Loop.Stop();
                UIstopWatch_Image.Stop();
            }

            SW_PerformanceMonitor.Stop();

            if (focusStop)
            {
                if (FLIMage.InvokeRequired)
                {
                    FLIMage.BeginInvoke((Action)delegate
                    {
                        FLIMage.StopFocus_GUI_Update();
                    });
                }
                else
                    FLIMage.StopFocus_GUI_Update();

                focusing = false;
            }
            else
            {
                if (FLIMage.InvokeRequired)
                {
                    FLIMage.BeginInvoke((Action)delegate
                    {
                        FLIMage.StopGrab_GUI_Update();
                    });
                }
                else
                    FLIMage.StopGrab_GUI_Update();

                grabbing = false;
                looping = false;
            }
        }

        /// <summary>
        /// When stopping, mirrors need to be parked.
        /// </summary>
        void ParkMirrors()
        {
            double[] values = State.Init.mirrorParkPosition;
            mirrorAO_S.putValue_S(values);
            Power_putEOMValues(true);
        }


        public void Power_putEOMValues(bool zero)
        {
            int addUncaging = 0;
            if (State.Init.AO_uncagingShutter)
                addUncaging = 1;
            double[] powerArray = new double[State.Init.EOM_nChannels + addUncaging];
            for (int i = 0; i < State.Init.EOM_nChannels; i++)
            {
                //if (calib.success[i])
                //{
                if (zero)
                    powerArray[i] = shading.calibration.GetEOMVoltageByFitting(0, i);   //calibration.calibrationCurve[i][State.Acq.power[0]];
                else
                    powerArray[i] = shading.calibration.GetEOMVoltageByFitting(State.Acq.power[i], i);//calibrationCurve[i][State.Acq.power[i]];
                //}
            }

            if (FLIMage.uncaging_panel != null && FLIMage.uncaging_panel.UncagingShutter && addUncaging == 1)
                powerArray[powerArray.Length - 1] = 5.0;
            else if (addUncaging == 1)
                powerArray[powerArray.Length - 1] = 0.0;


            if (FLIMage.uncaging_panel != null && State.Init.DO_uncagingShutter)
                uncagingShutterCtrl(FLIMage.uncaging_panel.UncagingShutter, true, true);

            shading.calibration.PutValue(powerArray);
        }


        /// <summary>
        /// NI-DAQ boards for grabbing are disposed. When stopping grab/focus.
        /// </summary>
        public void DisposeDAQ()
        {
            if (use_nidaq)
            {
                lineClock.dispose();
                dLineClock.dispose();
                mirrorAO.dispose();

                if (State.Init.EOM_nChannels > 0)
                    pockelAO.dispose();
            }
        }

        public void uncagingShutterCtrl(bool ON, bool controlAO, bool controlDO)
        {
            if (State.Init.DO_uncagingShutter && controlDO)
            {
                UncagingShutter_DO_S.TurnOnOff(ON);
            }

            if (State.Init.AO_uncagingShutter && controlAO)
            {
                if (ON)
                    UncagingShutterAO.AO_putValue(5);
                else
                    UncagingShutterAO.AO_putValue(0);
            }
        }

        /// <summary>
        /// Wait for next slice and then execute next slice. This occurs in different thread, so that this window is released.
        /// </summary>
        void WaitForNextSlice()
        {
            double eTime = UIstopWatch_Image.ElapsedMilliseconds; //Start measuring the time
            waitingSliceTask = true; //you can turn off this to stop this task. Not called by any function for now.

            if (DEBUGMODE)
                Debug.WriteLine("Now WaitForNextSlice task started");

            DisposeDAQ(); //Just to make sure in this thread...
            waitForAcquisitionTaskCompleted();
            uncaging_SliceCounter++;

            int standard_waitTime = 40;  //With this cycle, we can see if stopGrab is activated. 

            if (State.Uncaging.sync_withSlice && State.Uncaging.uncage_whileImage) //Uncaging protocol!!
            {
                while (waitingSliceTask && !State.Acq.ZStack) //Cycle roughly every standard_waitTime.
                {
                    double eTime2 = UIstopWatch_Image.ElapsedMilliseconds;
                    double waitTime = State.Acq.sliceInterval * internalSliceCounter * 1000.0 - State.Uncaging.uncagingTime() - eTime2;

                    if (waitTime > standard_waitTime)
                    {
                        System.Threading.Thread.Sleep(standard_waitTime);
                        if (stopGrabActivated)
                            break;
                    }
                    else
                    {
                        if (waitTime > 0 && !stopGrabActivated)
                            System.Threading.Thread.Sleep((int)waitTime);
                        break;
                    }
                } //While

                if (stopGrabActivated || !waitingSliceTask) //stoped in the middle.
                {
                    FLIMage.MoveBackToHome();
                    return;
                }

                if (uncaging_SliceCounter >= State.Uncaging.SlicesBeforeUncage)
                {
                    bool fire = (uncaging_SliceCounter - State.Uncaging.SlicesBeforeUncage) % State.Uncaging.Uncage_SliceInterval == 0;
                    fire = fire && FLIMage.uncaging_panel.uncaging_count < State.Uncaging.trainRepeat;
                    //Stopwatch sw1 = new Stopwatch();
                    //sw1.Start();
                    if (fire)
                    {
                        if (FLIMage.uncaging_panel != null)
                        {
                            Power_putEOMValues(true); //Put zero value first before opening the shutter.
                            ShutterCtrl.open();
                            FLIMage.uncaging_panel.UncageOnce(false, this); //Takes 40 + uncaging ms.
                                                                            //Debug.WriteLine("Uncaging time: {0}", sw1.ElapsedMilliseconds);
                        }
                    }
                    else
                        System.Threading.Thread.Sleep((int)State.Uncaging.uncagingTime());
                }
            }
            else
            {
                while (waitingSliceTask && !State.Acq.ZStack)
                {
                    double eTime2 = UIstopWatch_Image.ElapsedMilliseconds;
                    double waitTime = State.Acq.sliceInterval * internalSliceCounter * 1000.0 - eTime2;

                    if (FLIM_ImgData.ZStack) //for ZStack, we willl 
                        waitTime = 0;

                    if (waitTime > standard_waitTime)
                    {
                        System.Threading.Thread.Sleep(standard_waitTime);
                        if (stopGrabActivated)
                            break;
                    }
                    else
                    {
                        if (waitTime > 0)
                            System.Threading.Thread.Sleep((int)waitTime);
                        break;
                    }
                }
                //FocusButton.Enabled = false;
            } //While


            if (stopGrabActivated || !waitingSliceTask) //When stopped, the above loop breaks;
            {
                FLIMage.MoveBackToHome();
                return;
            }

            eTime = UIstopWatch_Image.ElapsedMilliseconds;
            measuredSliceInterval = eTime / 1000.0 / internalSliceCounter;

            bool[] eraseMemoryA = boolAllChannels(!State.Acq.aveSlice || averageSliceCounter == 0);

            AO_FrameCounter = 0;
            waitingSliceTask = false; //Now it finished its work.

            if (DEBUGMODE)
                Debug.WriteLine("Before Start DAQ in wait slice task"); //Too let us know where we are.

            if (!stopGrabActivated)
            {
                Power_putEOMValues(false);
                StartDAQ(eraseMemoryA, true, false);
                EventNotify?.Invoke(this, new ProcessEventArgs("SliceAcquisitionStart", null));
            }

            if (DEBUGMODE)
                Debug.WriteLine("Finished wait slice task");

        }

        bool[] boolAllChannels(bool bool1)
        {
            return Enumerable.Repeat<bool>(bool1, State.Acq.nChannels).ToArray();
        }

        void LoopingImageAcq()
        {

            waitForAcquisitionTaskCompleted(); //// You have to do it from different thread!!;

            waitingImageTask = true;

            FLIMage.BeginInvoke((Action)delegate ()
            {
                FLIMage.FocusButtonEnable(true);
            });

            //looping = true;

            while (waitingImageTask)
            {
                double eTime = UIstopWatch_Image.ElapsedMilliseconds;
                int waitTime = (int)(State.Acq.imageInterval * 1000 - eTime);
                if (waitTime > 10)
                {
                    System.Threading.Thread.Sleep(10);
                }
                else if (waitTime <= 0)
                {
                    System.Threading.Thread.Sleep(1);
                    break;
                }
                else
                {
                    if (waitTime > 0)
                        System.Threading.Thread.Sleep((int)(State.Acq.imageInterval * 1000 - eTime));
                    break;
                }

                if (stopGrabActivated)
                {
                    return;
                }
            }

            //FocusButton.Enabled = false;
            if (focusing)
                StopFocus();

            if (!waitingImageTask || stopGrabActivated)
                return;

            waitingImageTask = false;

            if (!stopGrabActivated)
                StartGrab(false);
            else
                StopGrab(true);
        }

        ///////////////////////////////////Measurement Done Handle//////////////////////////////////////
        public void MeasDoneEvent(FiFio_multiBoards fifo, EventArgs e)
        {
            //Debug.WriteLine("Measurement Done!");
            if (fifo.saturated)
            {
                if (focusing)
                    StopFocus();
                if (grabbing)
                    StopGrab(true);

                MessageBox.Show("FiFo saturated!!");
            }

            try
            {
                if (FLIMage.fastZcontrol != null)
                {
                    FLIMage.fastZcontrol.BeginInvoke((Action)delegate
                    {
                        FLIMage.fastZcontrol.CalculateFastZParameters();
                    });
                }
                else
                    State.Acq.fastZScan = false;
            }
            catch (Exception EX)
            {
                Debug.WriteLine("***Main window is closed!!*****" + EX.ToString());
            }
        }


        ///////////////////////////////////STRIPE EVENT HANDLING////////////////////////////////////////

        ///////////////////////////////////FRAME EVENT HANDLING////////////////////////////////////////

        public void BackGroundFLIMAcq(FiFio_multiBoards fifo, EventArgs e) //called when a Frame is done.
        {
            lock (syncFLIMacq) //acquisition lock.
            {
                BackGroundFLIMAll(fifo);
            }
        }

        public int[] GetAverageFrame(int nAverage)
        {
            int[] aveN = new int[State.Acq.nChannels];
            for (int c = 0; c < State.Acq.nChannels; c++)
                if (State.Acq.aveFrameA[c])
                    aveN[c] = nAverage;
                else
                    aveN[c] = 1;

            return aveN;
        }

        public int[] Get_n_time()
        {
            int[] n_time = Enumerable.Repeat<int>(State.Spc.spcData.n_dataPoint, State.Acq.nChannels).ToArray();
            for (int i = 0; i < State.Acq.nChannels; i++)
            {
                if (!State.Acq.acqFLIMA[i])
                    n_time[i] = 1;
            }

            return n_time;
        }

        /// <summary>
        /// This handles acquired images. Called by FiFo when a frame is acquired.
        /// </summary>
        /// <param name="fifo"></param>
        public void BackGroundFLIMAll(FiFio_multiBoards fifo)
        {
            if (DEBUGMODE)
                Debug.WriteLine("Frame done event received. Time = " + DateTime.Now.ToString("HH:mm:ss.fff"));

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            if (FLIMage.snapShot & (use_pq || use_bh))
            {
                UInt16[][][,,] FLIMTemp = (UInt16[][][,,])fifo.FLIM_data;

                FLIM_ImgData.LoadFLIMdata5D_Realtime(FLIMTemp); //Save in FLIM_ImgData class. Linearize first.
                var flimZdata = ImageProcessing.PermuteFLIM5D(FLIM_ImgData.FLIMRaw5D); //Copy to FLIMPage.

                if (!State.Acq.fastZScan)
                    FLIM_ImgData.LoadFLIMRawFromLinearAllChannels(flimZdata[0]);
                else
                {
                    FLIM_ImgData.SetupPagesFromZProject(flimZdata, acquiredTime);
                    FLIMage.image_display.calcZProjection();
                }


                FLIM_ImgData.nAveragedFrame = Enumerable.Repeat<int>(1, State.Acq.nChannels).ToArray();
                FLIM_ImgData.calculateProject();


                StopFocus();

                FLIMage.DisplaySnapShot();

                FLIMage.BeginInvoke((Action)delegate
                {
                    FLIMage.SetParametersFromState(true);
                });

                return;
            } //Snapshot.

            if (use_pq || use_bh)
            {
                Boolean running = false;

                if (runningImgAcq)
                {
                    running = fifo.Running;
                }

                if (running)
                {
                    UInt16[][][,,] FLIMTemp = fifo.FLIM_data;

                    FLIM_ImgData.LoadFLIMdata5D_Realtime(FLIMTemp); //Save in FLIM_ImgData class. This linearize the vector.

                    var flimZdata = ImageProcessing.PermuteFLIM5D(FLIM_ImgData.FLIMRaw5D);

                    if (!State.Acq.fastZScan)
                        FLIM_ImgData.LoadFLIMRawFromLinearAllChannels(flimZdata[0]);
                    else
                    {
                        FLIM_ImgData.SetupPagesFromZProject(flimZdata, acquiredTime);
                        //image_display.calcZProjection();
                    }

                    bool[] savebool = boolAllChannels(false);
                    bool protecting_save_task = false;

                    if (focusing)
                    {
                        FLIM_ImgData.nAveragedFrame = Enumerable.Repeat(1, State.Acq.nChannels).ToArray();
                    }
                    else
                    {
                        for (int i = 0; i < State.Acq.nChannels; i++)
                            savebool[i] = (!(State.Acq.aveFrameA[i] && State.Acq.nAveFrame > 1)) && (State.Acq.acquisition[i]);

                        if (State.Acq.aveFrameA.Any(x => x == true) && State.Acq.nAveFrame > 1)
                        {
                            averageCounter++;

                            if (!State.Acq.aveSlice)
                            {
                                FLIM_ImgData.nAveragedFrame = GetAverageFrame(averageCounter);
                            }
                            else
                                FLIM_ImgData.nAveragedFrame = Enumerable.Repeat(averageCounter + State.Acq.nAveFrame * averageSliceCounter, State.Acq.nChannels).ToArray();

                            if (State.Acq.nAveFrame == averageCounter) //Finished averaging 1 slice
                            {
                                averageCounter = 0;
                                averageSliceCounter++;

                                protecting_save_task = true;
                                //When you finish 1 slice, you should wait for saving task.

                                //Debug.WriteLine("AverageSliceCounter :" + averageSliceCounter + "   n_Slice: " + State.Acq.nSlices);

                                if (!State.Acq.aveSlice)
                                {
                                    for (int i = 0; i < State.Acq.nChannels; i++)
                                    {
                                        if (State.Acq.aveFrameA[i] && State.Acq.acquisition[i])
                                            savebool[i] = true;
                                    }

                                }
                                else // aveSlice
                                {
                                    if (averageSliceCounter == State.Acq.nAveSlice)
                                    {
                                        averageSliceCounter = 0;
                                        for (int i = 0; i < State.Acq.nChannels; i++)
                                        {
                                            if (State.Acq.aveFrameA[i] && State.Acq.acquisition[i])
                                                savebool[i] = true;
                                        }
                                    }
                                } // aveSlice
                            }
                        }
                    } //if focus.

                    FLIM_ImgData.saveChannels = (bool[])savebool.Clone();

                    SaveUpdateAcquiredImage(savebool, protecting_save_task);

                    if (runningImgAcq)
                        FLIMProgressChanged();


                } //Running
            } //if (use_bh || use_pq)
            //Debug.WriteLine("Time to acquire file: " + sw.ElapsedMilliseconds + " ms");
        } //BackgroundFLIMAll.

        /// <summary>
        /// SaveUpdateAcquiredImage: Called by BackgroundFLIMAll. Control saving and displaying acquired data.
        /// </summary>
        /// <param name="saveFileBools"></param>
        /// <param name="protecting_save_task"></param>
        public void SaveUpdateAcquiredImage(bool[] saveFileBools, bool protecting_save_task)
        {
            DateTime acTime;
            double msPerLine = State.Acq.fastZScan ? State.Acq.FastZ_msPerLine : State.Acq.msPerLine;

            //Deepcopy of FLIMRaw5D.
            if (saveFileBools.Any(x => x == true)) //If any savefile exists.
            {
                acTime = acquiredTime.AddMilliseconds(internalFrameCounter * msPerLine * State.Acq.linesPerFrame);

                var FLIMForSave = MatrixCalc.MatrixCopy3D(FLIM_ImgData.FLIMRaw5D);

                FLIM_ImgData.saveChannels = saveFileBools;

                for (int i = 0; i < saveFileBools.Length; i++)
                {
                    if (!saveFileBools[i])
                    {
                        FLIMForSave[i] = null;
                    }
                }


                lock (saveBufferObj)
                {
                    if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
                    {
                        //Adding FLIMforSave data to FLIM_Pages5D. We don't need deep copy because it is already copied from FLIMRaw5D. (the last argument.
                        FLIM_ImgData.Add5DFLIM(FLIMForSave, acTime, savePageCounter, false);

                        //Debug.WriteLine("Loaded to page: " + displayPageCounter + "/" + FLIM_ImgData.FLIM_Pages5D.Count);
                    }
                    else
                    {
                        FLIMSaveBuffer.Add(FLIMForSave); //This is not a copy.
                        acquiredTimeList.Add(acTime);
                    }
                }
            }


            UpdateImages();


            if (saveFileBools.Any(x => x == true))
            {
                SaveFile(protecting_save_task);
            }
        }

        /// <summary>
        /// If not keeping the data in memory, the stack is removed after saving. For ZStack, everything is saved in memory.
        /// </summary>
        public void CheckDeletePageBuffer()
        {
            lock (saveBufferObj)
            {
                if (!(FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack))
                {
                    if (displayPageCounter > deletedPageCounter && savePageCounter > deletedPageCounter)
                    {
                        if (FLIMSaveBuffer != null && FLIMSaveBuffer.Count > 0)
                        {
                            RemoveFrameAt(0);
                            if (acquiredTimeList != null && acquiredTimeList.Count > 0)
                                acquiredTimeList.RemoveAt(0);
                            deletedPageCounter++;
                        }
                    }
                }
            }
        }

        public void RemoveFrameAt(int frameToWork) //Clear memory very well.
        {
            if (FLIMSaveBuffer != null && frameToWork < FLIMSaveBuffer.Count)
                FLIMSaveBuffer.RemoveAt(frameToWork);
        }

        public int totalPagesSaved()
        {
            int aveNFrame = 1;
            if (State.Acq.aveFrameA.Any(x => x == true))
                aveNFrame = State.Acq.nAveFrame;

            int TotalNPages = State.Acq.nFrames / aveNFrame;
            int aveNslices = 1;
            if (State.Acq.aveSlice)
                aveNslices = State.Acq.nAveSlice;

            TotalNPages = TotalNPages * State.Acq.nSlices / aveNslices;

            //Debug.WriteLine("Total pages saved = " + TotalNPages);

            return TotalNPages;
        }

        public void SaveFile(bool protecting_save_task)
        {
            if (DEBUGMODE)
                Debug.WriteLine("Start Save File");

            bool updated = false;
            int savePage = savePageCounter - deletedPageCounter;
            //int safeMergin = 0;

            if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
            {
                //Save task occurs only when savePage Counter is less than 5D page.
                updated = (FLIM_ImgData.FLIM_Pages5D.Length > savePageCounter);
            }
            else
            {
                updated = (FLIMSaveBuffer.Count > savePage && savePage >= 0);
            }

            //If previous saveTask is still running, it will wait until it is done.
            if (protecting_save_task && saveTask != null && !saveTask.IsCompleted)
                saveTask.Wait();

            if (updated && (saveTask == null || saveTask.IsCompleted))
            {
                int savePage1 = savePage; //Protect the count.

                if (protecting_save_task)
                    SaveTask(savePage1);
                else
                    saveTask = Task.Factory.StartNew(() => { SaveTask(savePage1); });

            }
            else
            {
                Debug.WriteLine("Saving BUSY***************************Could not save:" + (savePageCounterTotal + 1) + " (" + (savePageCounter + 1) + "/" + FLIM_ImgData.FLIM_Pages5D.Count() + ")");
            }

            if (FLIMage.InvokeRequired)
            {
                FLIMage.Invoke((Action)delegate
                {
                    FLIMage.UpdateSavedNumberOfFile();
                });
            }
            else
            {
                FLIMage.UpdateSavedNumberOfFile();
            }
            //busySaving = false;
        }

        /// <summary>
        /// Save acquired image. Called in different thread (Task.Factory.StartNew). 
        /// </summary>
        /// <param name="savePage"></param>
        public void SaveTask(int savePage)
        {
            if (DEBUGMODE)
                Debug.WriteLine("Start Save Task");

            lock (syncFLIMsave)
            {
                if (DEBUGMODE)
                    Debug.WriteLine("Start FLIMsave sync");

                UInt16[][][] FLIMImage; //Before permutation.
                DateTime acqTimeTemp;

                ushort[][][] FLIM_5D; //After permutation.

                ////Overwrite and savefile setting
                bool[] overwrite = (bool[])newFile.Clone();

                int error = 0;
                String fullFileName = State.Files.fullName();

                bool[] saveCh = boolAllChannels(true);

                //Everything about FLIM_Page5D or FLIMSaveBufer. 
                lock (saveBufferObj)
                {
                    if (DEBUGMODE)
                        Debug.WriteLine("Start FLIMmovie sync");

                    if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
                    {
                        FLIMImage = FLIM_ImgData.FLIM_Pages5D[savePageCounter];
                        acqTimeTemp = FLIM_ImgData.acquiredTime_Pages5D[savePageCounter];
                    }
                    else
                    {
                        FLIMImage = FLIMSaveBuffer[savePage];
                        acqTimeTemp = acquiredTimeList[savePage];
                    }

                    FLIM_5D = ImageProcessing.PermuteFLIM5D(FLIMImage); //Finished copy.

                    for (int ch = 0; ch < State.Acq.nChannels; ch++)
                    {
                        if (FLIMImage[ch] == null)
                            saveCh[ch] = false;
                    }
                }

                if (DEBUGMODE)
                    Debug.WriteLine("Start Saving File.");

                if (!State.Files.channelsInSeparatedFile)
                {
                    bool overwrite1 = overwrite[0];
                    newFile = boolAllChannels(false);

                    if (FLIM_5D.Length == 1)
                        error = fileIO.SaveFLIMInTiff(fullFileName, FLIM_5D[0], acqTimeTemp, overwrite1, saveCh);
                    else
                        error = fileIO.SaveFLIMInTiffZStack(fullFileName, FLIM_5D, acqTimeTemp, overwrite1, saveCh);
                }
                else
                    for (int ch = 0; ch < State.Acq.nChannels; ch++)
                    {
                        if (FLIMImage[ch] != null)
                        {
                            saveCh = new bool[State.Acq.nChannels];
                            saveCh[ch] = true;
                            newFile[ch] = false;

                            bool overwrite1 = overwrite[ch];
                            String fileName = State.Files.fullName(ch);
                            if (FLIM_5D.Length == 1)
                            {
                                ushort[][] FLIM_separated = new ushort[State.Acq.nChannels][];
                                FLIM_separated[ch] = FLIM_5D[0][ch];
                                error = fileIO.SaveFLIMInTiff(fileName, FLIM_separated, acqTimeTemp, overwrite1, saveCh);

                                if (DEBUGMODE)
                                    Debug.WriteLine("Saving file.. {0}, channel = {1}, overwrite = {2}", fileName, ch, overwrite1);
                            }
                            else
                            {
                                ushort[][][] FLIM_separated = new ushort[FLIM_5D.Length][][];
                                for (int z = 0; z < FLIM_5D.Length; z++)
                                {
                                    FLIM_separated[z][ch] = FLIM_5D[z][ch];
                                }
                                error = fileIO.SaveFLIMInTiffZStack(fileName, FLIM_separated, acqTimeTemp, overwrite1, saveCh);

                                if (DEBUGMODE)
                                    Debug.WriteLine("Saving file.. {0}, channel = {1}, overwrite = {2}, n_zslice = ", fileName, ch, overwrite1, FLIM_5D.Length);

                            }
                        }
                    }


                if (FLIMage.saveIntensityImage)
                {
                    //bool ChannelsInSeparatedFiles = false; // image_display.ExportSeparatedChannelFiles;
                    for (int c = 0; c < State.Acq.nChannels; c++)
                    {
                        if (FLIMImage[c] != null)
                        {
                            if (State.Files.channelsInSeparatedFile)
                            {
                                saveCh = new bool[State.Acq.nChannels];
                                saveCh[c] = true;
                            }

                            for (int z = 0; z < FLIM_5D.Length; z++)
                            {
                                String fileName = fileIO.FLIM_FilePath(c, State.Files.channelsInSeparatedFile, State.Files.fileCounter, FileIO.ImageType.Intensity, "", State.Files.pathName);
                                bool overwrite1 = (overwrite[c] && (z == 0)) && (State.Files.channelsInSeparatedFile || c == 0);
                                int[] dimYXT = FLIM_ImgData.dimensionYXT(c);
                                fileIO.Save2DImageInTiff(fileName, ImageProcessing.GetProjectFromFLIM_Linear(FLIM_5D[z][c], dimYXT), acqTimeTemp, overwrite1, saveCh);
                            }
                        }
                    }
                }

                if (overwrite.Any(x => x == true))
                    FLIM_ImgData.image_description = fileIO.image_description;


                savePageCounter++;
                savePageCounterTotal++;

                int TotalNPages = totalPagesSaved();

                if (TotalNPages == savePageCounter)
                {
                    EventNotify?.Invoke(this, new ProcessEventArgs("SaveImageDone", FLIMImage));
                }

                //if (parameters.enableFastZscan)  //always in different files. to avoid confusion.
                //{
                //    newFile = true;
                //    State.Files.fileCounter++;
                //    UpdateFileName();
                //}

                if (savePageCounter == State.Acq.maxNFramePerFile && savePageCounterTotal < TotalNPages)
                {
                    //Finished saving. Reaching maximum page number. 

                    newFile = boolAllChannels(true);
                    State.Files.fileCounter++;
                    FLIMage.UpdateFileName();

                    if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
                    {
                        lock (saveBufferObj)
                        {
                            FLIM_ImgData.RemovePageRange5D(0, State.Acq.maxNFramePerFile);
                            displayPageCounter = displayPageCounter - State.Acq.maxNFramePerFile;

                            savePageCounter = 0;

                            if (displayPageCounter < 0)
                                displayPageCounter = 0;
                        }

                    }
                    else
                    {
                        savePageCounter = 0;
                        deletedPageCounter = deletedPageCounter - State.Acq.maxNFramePerFile;
                        displayPageCounter = displayPageCounter - State.Acq.maxNFramePerFile;

                        if (displayPageCounter < 0)
                            displayPageCounter = 0;

                        if (deletedPageCounter < -1)
                        {
                            // Should be -1. It will be added at CheckDeletePageBuffer();
                            Debug.WriteLine("Deleted Counter < 0 problem!!" + deletedPageCounter + ", Disp = " + displayPageCounter);
                        }

                    }
                }
                // }


                //Debug.WriteLine("PageCount = " + FLIM_ImgData.FLIM_Pages5D.Count + ", save Count" + savePageCounter);

                CheckDeletePageBuffer();

                //EventNotify?.Invoke(this, new ProcessEventArgs("FrameAcquisitionDone", null));

            } //Sync

            if (DEBUGMODE)
                Debug.WriteLine("Ended Save Task");
        }

        /// <summary>
        /// CheckSavingParameters: creating directory for saving etc before starting grab.
        /// </summary>
        /// <returns></returns>
        public int CheckSavingParameters()
        {
            bool[] saveChannels = Enumerable.Repeat<bool>(true, FLIM_ImgData.nChannels).ToArray();
            fileIO.CreateHeader(saveChannels);
            System.IO.Directory.CreateDirectory(State.Files.pathName);
            System.IO.Directory.CreateDirectory(State.Files.pathNameIntensity);
            System.IO.Directory.CreateDirectory(State.Files.pathNameFLIM);
            if (System.IO.File.Exists(State.Files.fullName()))
            {
                DialogResult dr = MessageBox.Show("File already exist! Do you want to overwrite?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                    return (0);
                else
                    return (-1);

            }
            else
                return (0);
        }


        ///////////////////////////////////FRAME EVENT HANDLING////////////////////////////////////////

        ///////////////////////////////////STRIPE EVENT HANDLING////////////////////////////////////////
        public void StripeDoneEventAll(UInt16[][][,,] StripeImage, StripeEventArgs e)
        {
            if (DEBUGMODE)
                Debug.WriteLine("Stripe event called: device = " + e.device + "/ channel" + e.channelList[0] + " / " + e.channelList.Count + " FirstLine = " + e.StartLine + "EndLine = " + e.EndLine);
            try
            {
                for (int ch = 0; ch < e.channelList.Count; ch++)
                {
                    FLIMage.image_display.UpdateStripe(StripeImage, e.channelList[ch], e.StartLine, e.EndLine);
                }
            }
            catch (Exception E1)
            {
                Debug.WriteLine("Error during Stripe: " + E1.Message);
            }
        }

        public void StripeDoneEvent(FiFio_multiBoards fifo, StripeEventArgs e)
        {
            if (focusing)
            {
                StripeDoneEventAll(fifo.FLIM_Stripe, e);
            }
        }






        /// <summary>
        /// Called when Background acquisition is done. However, it should be noted that save task and display Task are in spearated threads.
        /// </summary>
        public void FLIMProgressChanged()
        {
            int[] nAve = GetAverageFrame(State.Acq.nAveFrame);
            for (int ch = 0; ch < State.Acq.nChannels; ch++)
            {
                if (State.Acq.aveSlice)
                    nAve[ch] = nAve[ch] * State.Acq.nAveSlice;
            }

            internalFrameCounter++;

            //Update GUI. We are in different thread from main window. So, we will evoke it.
            FLIMage.BeginInvoke((Action)delegate
             {
                 FLIMage.UpdateCounters();
             });
            //Debug.WriteLine("Acquired frame {0} / {1}, Slice {2} / {3}. Time = {4} ms", internalFrameCounter, Acq_nFrames, internalSliceCounter, State.Acq.nSlices, UIstopWatch_Frame.ElapsedMilliseconds);

            UIstopWatch_Frame.Restart();

            if (internalFrameCounter == State.Acq.nFrames && !focusing)
            {
                //Debug.WriteLine("Slice done!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                internalSliceCounter++;
                internalFrameCounter = 0;


                StopDAQ(); //close shutter included.
                DisposeDAQ();
                FiFo_acquire.StopMeas(true);

                if (saveTask != null && !saveTask.IsCompleted)
                    saveTask.Wait();

                int NPages = 0;

                if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
                {
                    if (FLIM_ImgData.FLIM_Pages5D[0] != null)
                        NPages = FLIM_ImgData.FLIM_Pages5D.Length;
                    deletedPageCounter = 0;
                }
                else
                {
                    NPages = FLIMSaveBuffer.Count;
                }

                while (NPages > savePageCounter - deletedPageCounter)
                {
                    if (FLIM_ImgData.KeepPagesInMemory || FLIM_ImgData.ZStack)
                    {
                        NPages = FLIM_ImgData.FLIM_Pages5D.Length;
                        deletedPageCounter = 0;
                    }
                    else
                        NPages = FLIMSaveBuffer.Count;

                    SaveFile(true);
                    Debug.WriteLine("Saving extra file.... " + (savePageCounterTotal), "/" + (savePageCounter));
                }

                FLIMSaveBuffer.Clear();

                FLIMage.BeginInvoke((Action)delegate
                {
                    FLIMage.UpdateMeasuredSliceInterval();
                });

                if (internalSliceCounter == State.Acq.nSlices) //All slices done.
                {
                    FLIMage.MoveBackToHome();

                    State.Files.fileCounter++; //This needs to be after MoveMotorBack, since movemotorbacktohome sends notification with the current fileCounter.
                    internalImageCounter++;

                    if (internalImageCounter >= State.Acq.nImages || !allowLoop)
                    {
                        Task.Factory.StartNew(() => { StopGrab(true); });
                    }

                    if (State.Acq.fastZScan && !State.Acq.ZStack)
                    {
                        //Setup display.
                        var FLIM5D = (ushort[][][])FLIM_ImgData.FLIMRaw5D;
                        FLIM_ImgData.clearPages4D(); //Clean the the 4D stack. it is separated from acquisition.
                        FLIM_ImgData.addToPageAndCalculate5D(FLIM5D, acquiredTime, true, true, 0, true);
                    }

                    try
                    {
                        var image_display = FLIMage.image_display;
                        FLIMage.UpdateFileName();
                        image_display.ZStack = FLIM_ImgData.ZStack || State.Acq.fastZScan;
                        image_display.displayZProjection = FLIM_ImgData.ZStack || State.Acq.fastZScan;

                        //FLIM_ImgData.n_pages = savePageCounter;
                        image_display.UpdateImages(true, false, false, true);

                        //if (!State.Acq.acqFLIMA)
                        //    image_display.calculateTimecourse();

                        FLIM_ImgData.fileUpdateRealtime(State, false); //FileName update.
                        image_display.UpdateFileName();

                        if (FLIMage.analyzeAfterEachAcquisiiton)
                        {
                            image_display.BeginInvoke((Action)delegate
                            {
                                image_display.plot_regular.Show();
                                image_display.plot_regular.Activate();
                                image_display.OpenFLIM(FLIM_ImgData.State.Files.fullName(), true);
                            });
                        }
                    }
                    catch (Exception E)
                    {
                        Debug.WriteLine("Problem in displaying and /or analyzing saved image: " + E.Message);
                        //WriteStatusText("Problem in displaying and /or analyzing saved image");
                    }

                    EventNotify?.Invoke(this, new ProcessEventArgs("AcquisitionDone", null));

                    grabbing = false; //grab is finished.

                    //////////////
                    if (internalImageCounter < State.Acq.nImages && allowLoop)
                    {
                        waitImage = Task.Factory.StartNew(() =>
                        {
                            lock (waitLoopImageTaskobj) //This task will never overlap. Don't use in any other lock.
                                LoopingImageAcq();
                        });
                    }
                    else
                    {
                        FLIMage.StopLoop();
                    }

                }
                else if (internalSliceCounter < State.Acq.nSlices) // Slices still not done.
                {
                    if (State.Acq.ZStack)
                    {
                        FLIMage.MoveMotorStep(true);
                        if (internalSliceCounter == State.Acq.nSlices - 1)
                            FLIMage.motorCtrl.stack_Position = MotorCtrl.StackPosition.End;
                    }

                    EventNotify?.Invoke(this, new ProcessEventArgs("SliceAcquisitionDone", null));

                    waitSlice = Task.Factory.StartNew(() =>
                    {
                        lock (waitSliceTaskobj) //This task will never overlap. Don't use in any other lock.
                            WaitForNextSlice();
                    });
                }
                else
                {
                    Debug.WriteLine("Warning.... Slice counter: {0} > n Slices: {1}", internalSliceCounter, State.Acq.nSlices);
                }

            }//internalFrame

            FLIMage.Invoke((Action)delegate
            {
                FLIMage.UpdateAverageFrameCounter();
            });
        }

        public void UpdateImages() //Can be slow. //Done with timer. //Thread safe.
        {
            bool updated = false;

            FLIM_ImgData.State.Uncaging.Position = (double[])State.Uncaging.Position.Clone();
            displayPageCounter++;
            updated = !FLIMage.image_display.update_image_busy;
            if (updated)
            {
                if (State.Acq.fastZScan)
                {
                    if (displayPageCounter == 1) //First page = 1. After ++.
                        FLIMage.image_display.SetFastZModeDisplay(State.Acq.fastZScan); //To set the page at the center.
                    FLIMage.image_display.setupImageUpdateForZStack();
                }

                FLIMage.image_display.UpdateImages(true, true, focusing, true);
            }
            else
            {
                //if (!focusing)
                Debug.WriteLine("**** Busy ************" + displayPageCounter);

                displayPageCounter++;
                displayPageCounterTotal++;
            }

            if (displayPageCounter == State.Acq.maxNFramePerFile && displayPageCounterTotal < totalPagesSaved())
            {
                Debug.WriteLine("Total page saved:" + totalPagesSaved() + ", displayCounter = " + displayPageCounterTotal);
                FLIMage.image_display.realtimeData.Clear();
            }


            //Refresh();

            //Debug.WriteLine("Time from previous frame: " + SW_PerformanceMonitor.ElapsedMilliseconds);
            //if (SW_PerformanceMonitor.ElapsedMilliseconds > 1.3 * State.Acq.frameInterval() * 1000.0)
            //    Debug.WriteLine("Performance is not great!!" + SW_PerformanceMonitor.ElapsedMilliseconds + " ms per frame");


            SW_PerformanceMonitor.Restart();


        }




    } //Class

    public class ProcessEventArgs : EventArgs
    {
        public string EventName { get; internal set; }
        public object EventData { get; internal set; }
        public ProcessEventArgs(string name, object data)
        {
            this.EventName = name;
            this.EventData = data;
        }
    }

}

