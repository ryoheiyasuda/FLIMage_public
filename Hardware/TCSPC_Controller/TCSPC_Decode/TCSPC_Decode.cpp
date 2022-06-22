/**
 * @file TCSPC_Decode.cpp
 *
 * @brief This class deocdes photon and marker information from TCSPC stream and asign
 * to pixels. This is a main program to control Decode Engine etc.
 *
 * @author Ryohei Yasuda
 * @date June 20th 2019
 * Copyright - Max Planck Florida Institute for Neuroscience 2019
 *
 */

#include "stdafx.h"
#include "TCSPC_Decode.h"

#ifdef _DEBUG
#pragma message("   *****   Compiling in debug mode.   *****")
#endif

TCSPC_Decode** TCSPC_Decode::instances = new TCSPC_Decode*[DecodeEngine::maxNinstances](); //Allocate the memArray. Global variable.
int* TCSPC_Decode::instanceStatus = new int[DecodeEngine::maxNinstances]();

TCSPC_Decode* TCSPC_Decode::CreateTCSPC_Decode(int ID)
{
	TCSPC_Decode* td;
	if (DecodeEngine::maxNinstances > ID)
	{
		if (instanceStatus[ID] == 1 && instances[ID] != NULL)
		{
			td = instances[ID]; //just reuse it.
		}
		else
			td = new TCSPC_Decode(ID);

		return td;
	}
	else
		return NULL;
}


TCSPC_Decode::TCSPC_Decode(int id)
{
	deviceID = id;
	instances[deviceID] = this;
	instanceStatus[deviceID] = 1;

	meas_thread_on = false;
	Running = false;
	debug = 1;
}


TCSPC_Decode::~TCSPC_Decode()
{
	if (deviceOpen)
		CloseDevice();

	_Measurement_Thread_Join();

	delete engine;
	instanceStatus[deviceID] = 0;

	int statusSum = 0;
	for (int i = 0; i < DecodeEngine::maxNinstances; i++)
		statusSum += instanceStatus[i];

	if (statusSum == 0) //if all instance is deleted, pq will be deleted.
	{
		if (use_pq)
			delete pq;
		else
			delete bh;
	}
}

int TCSPC_Decode::InitializeDecodeEngine(callback myfunc, DecodeEngine::parameters* param1, char* DLL_Path)
{
	new DLL_Listener(deviceID, myfunc);
	RegisterListener(DLL_Listener::dllA[deviceID]);

#ifdef NDEBUG
	debug = 0;
#else
	debug = param1->debug;
#endif

#ifdef _DEBUG
	DebugNotification("Debug: Start decoding engine...", 1);
	DebugNotification("Debug: DLL path = " + string(DLL_Path), 1);
#endif

	//Register callback for engine and set parameters.
	engine = DecodeEngine::CreateDecodeEngine(deviceID);
	engine->RegisterListener(DLL_Listener::dllA[deviceID]);
	engine->FLIMbuffer->RegisterListener(DLL_Listener::dllA[deviceID]);
	engine->FLIMbuffer->debug = debug;
	engine->Initialize(param1);

	use_pq = (engine->acqType == PQ_MULTIHARP || engine->acqType == PQ_TH260);

	string libraryName;
	string successStr;
	bool success;
	if (use_pq)
	{
		pq = PQControl::CreatePQControl(param1->acqType, DLL_Path);
		success = pq->libraryLoaded;
		libraryName = string(pq->libraryName);
	}
	else
	{
		bh = BHControl::CreateBHControl(DLL_Path);
		success = bh->libraryLoaded;
		libraryName = string(bh->libraryName);
	}

#ifdef _DEBUG
	successStr = success ? "Success" : "Failed";
	DebugNotification("Debug: Loading library DLL ..." + libraryName + "Loaded " + successStr, 1);
#endif

	return success ? 0 : -100;
}

void TCSPC_Decode::DebugNotification(string message, int debug_level)
{
	if (debug >= debug_level)
		Notify(message);
}

void TCSPC_Decode::NotifyErrorByCode(int error_code, string error_message)
{
	if (use_pq)
		pq->PQ_GetErrorString(Errstr, error_code);
	else
		bh->SPC_get_error_string(error_code, Errstr, error_str_length);

	Notify("Error: " + error_message + " dev ID = " + to_string(deviceID) + ". Error = " + Errstr);
}

int TCSPC_Decode::OpenDevice()
{
	int retcode = 0;
	//int[] dev = new int[MAXDEVNUM];

	if (use_pq)
		retcode = pq->PQ_OpenDevice(deviceID, Serial);
	else
	{
		retcode = bh->SPC_init(bh->initFilePath);

		SPCModInfo* mod_info = new SPCModInfo;
		retcode = bh->SPC_get_module_info(deviceID, mod_info);
		if (mod_info->in_use == -1)
		{
			int* in_use = new int[BH_MaxDev]();
			for (int i = 0; i < BH_MaxDev; i++)
				in_use[i] = 1;
			retcode = bh->SPC_set_mode(deviceID, 1, in_use);
			retcode = bh->SPC_close();
			delete[] in_use;

			retcode = bh->SPC_init(bh->initFilePath);
		}

		delete mod_info;
	}

	deviceOpen = true; //does not mean that it suceeded.

	if (retcode == 0) //Grab any HydraHarp we can open
	{
#ifdef _DEBUG
		DebugNotification("Debug: Device open...", 1);
#endif
	}
	else
	{
		if (use_pq && retcode == pq->PQ_ERROR_DEVICE_OPEN_FAIL)
		{
			Notify("Error: PQ Device (ID = " + to_string(deviceID) + ") does not exist");
		}
		else
		{
			NotifyErrorByCode(retcode, "OpenDevice failed");
		}
	}

	return (retcode);
}

int TCSPC_Decode::CloseDevice()
{
	deviceOpen = false;
	if (use_pq)
		return pq->PQ_CloseDevice(deviceID);
	else
	{
		// Should be called after all devices stop.
		bool allClose = true;
		for (int i = 0; i < DecodeEngine::maxNinstances; i++)
		{
			if (instanceStatus[i] != 0)
				allClose = allClose && (!instances[i]->deviceOpen);
		}

		if (allClose)
			return bh->SPC_close();
		else
			return 0;
	}
}

int TCSPC_Decode::InitializeDevice()
{
	int retcode = 0;

	if (use_pq)
	{
		retcode = pq->PQ_Initialize(deviceID, engine->acq_modePQ);  //Histo mode
		if (retcode < 0)
		{
			NotifyErrorByCode(retcode, "PQ_Initialize error. Aborted.");
			return (retcode);
		}
		else
			return 0;
	}
	else
	{
#ifdef _DEBUG
		DebugNotification("Debug: TCSPC_Decode::InitializeDevice: Use BH device.", 1);
#endif
		return 0;
	}
}

int TCSPC_Decode::StartMeasurement(DecodeEngine::parameters* param1)
{
	int retcode = 0;
	focusing = param1->focus;
	saturated = false;
	last_event = false;
	force_stop = false;

	engine->Initialize(param1);

#ifdef _DEBUG
	DebugNotification("Debug: TCSPC_Decode::StartMeas at : " + Utilities::TimeStringNow(), 1);
#endif

	int frameN = param1->nFrames;
	int acqTime = 60 * 60 * 1000 * MAXHOUR;
	int Tacq = acqTime;

	if (use_pq)
		retcode = pq->PQ_StartMeas(deviceID, Tacq); //Start!!
	else
		retcode = bh->SPC_start_measurement(deviceID);

	if (retcode < 0)
	{
		NotifyErrorByCode(retcode, "Start measurement failed");

		Running = false;
		return (retcode);
	}
	else
	{
		Running = true;
#ifdef _DEBUG
		DebugNotification("Debug: Start measurement suceeded", 1);
#endif
		return (0);
	}

}

void TCSPC_Decode::_Measurement_Thread_Join()
{
#ifdef _DEBUG
	DebugNotification("Debug: Measurement_Thread joining. Joinable = " + to_string(meas_thread.joinable()), 1);
#endif
	if (meas_thread.joinable())
	{
		meas_thread.join();
	}
	else
	{
#ifdef _DEBUG
		DebugNotification("Debug: Measurement_Thread NOT joinable", 1);
#endif
	}
	meas_thread_on = false;
}

void TCSPC_Decode::StartBackgroundMeasurement()
{
#ifdef _DEBUG
	DebugNotification("Debug: TCSPC_Decode::StartBackgroundMeasurement starting now... thread = " + to_string(meas_thread_on)
		+ " thread joinable = " + to_string(meas_thread.joinable()), 1);
#endif

	_Measurement_Thread_Join();

	meas_thread_on = true;
	meas_thread = thread([this]
	{
		BackgroundAcq();
		meas_thread_on = false;
	});

#ifdef _DEBUG
	DebugNotification("Debug: TCSPC_Decode::StartBackgroundMeasurement suceeded", 1);
#endif
}

int TCSPC_Decode::StopMeasurement(bool force)
{
	int retcode = 0;

	if (Running)
	{
		if (force)
			engine->Running = false; //Force stopping engine.

		Running = false;

		force_stop = force;
		last_event = true;

#ifdef _DEBUG
		DebugNotification("Debug: Stopping thread...", 1);
		DebugNotification("Debug: Waiting for thread to finish.", 1);
#endif
		//Wait until thread is done. Running = false will activate the stop signal.
		int k = 0;
		while (meas_thread_on)
		{
			this_thread::sleep_for(chrono::milliseconds(waitTime)); //waitTime = 2 ms.
			Running = false;
			k++;
#ifdef _DEBUG
			if (k % 100 == 0)
				DebugNotification("Debug: Still waiting... = " + to_string(k*waitTime) + " ms", 1);
#endif
			if (k > 500)
			{
				break;
				meas_thread_on = false;
			}
		}

#ifdef _DEBUG
		DebugNotification("Debug: Time taken for thread to finish = " + to_string(k*waitTime) + " ms", 1);
#endif

		_Measurement_Thread_Join();

	}

	if (use_pq)
		retcode = pq->PQ_StopMeas(deviceID); //Now stopping the device.
	else
		retcode = bh->SPC_stop_measurement(deviceID);

	if (retcode < 0)
	{
		NotifyErrorByCode(retcode, "StopMeas Failed");
	}

	return (retcode);

}

////////////////////////////


int TCSPC_Decode::ReadBuffer(int &nRec)
{
	int retcode = 0;
	int flags = 0;
	int ctcstatus = 0;

	NRecords = 0;

	if (!Running)
		return (-1);

	if (use_pq)
	{
		retcode = pq->PQ_GetFlags(deviceID, flags);
		if (retcode < 0)
		{
			NotifyErrorByCode(retcode, "PQ_GetFlags Error.");
			return (retcode);
		}

		if ((flags & pq->FLAG_FIFOFULL) != 0) //bit and. 
		{
			saturated = true;
			Notify("Saturated");
			return (-100);
		}
	}
	if (force_stop)
		return (-100);

	if (use_pq)
	{
		retcode = pq->PQ_ReadFiFo(deviceID, buffer, pq->TTREADMAX, NRecords);
		nRec = NRecords;
	}
	else
	{
		unsigned long count16 = bh->TTREADMAX * 2;
		unsigned short* data = new unsigned short[count16];

		short retcode = bh->SPC_read_fifo(deviceID, &count16, data);

		nRec = count16 / 2;
		memcpy(buffer, data, count16 * sizeof(unsigned short));
		delete[] data;
	}


	if (retcode < 0)
	{
		NotifyErrorByCode(retcode, "Error in Reading FiFo");
	}

	if (NRecords == 0)
	{
		if (use_pq)
		{
			int rcode = pq->PQ_CTCStatus(deviceID, ctcstatus);
			if (rcode < 0)
			{
				NotifyErrorByCode(retcode, "PQ_CTCStatus Error. Aborted. Error:");
				return rcode;
			}
		}
		else
		{
#ifdef _DEBUG
			short state = 0;
			int retcode = bh->SPC_test_state(deviceID, &state);
			ostringstream os;
			os << "Debug: " << endl;
			os << "Measurement in progress in TDC 1: " << ((state & SPC_ARMED1) > 0) << endl;
			os << "Measurement in progress in TDC 2: " << ((state & SPC_ARMED2) > 0) << endl;
			os << "image measurement active, no wait for trigger or 1st frame pulse: " << ((state & SPC_MEASURE) > 0) << endl;
			os << "FIFO empty in TDC 1: " << ((state & SPC_FEMPTY1) > 0) << endl;
			os << "FIFO overflow in TDC 1: " << ((state & 0x400) > 0) << endl;
			os << "FIFO overflow in TDC 2: " << ((state & 0x800) > 0) << endl;
			os << "collection timer of TDC 1 expired: " << ((state & 0x8) > 0) << endl;
			os << "collection timer of TDC 2 expired: " << ((state & 0x20) > 0) << endl;
			os << "wait for trigger" << (state & 0x1000) << endl;
			DebugNotification(os.str(), 1);
#endif
			//SPC_ARMED1	0x80 	measurement in progress in TDC 1
			//	SPC_ARMED2	0x40 	measurement in progress in TDC 2
			//	SPC_MEASURE	0x80 	image measurement active, no wait for trigger or 1st frame pulse
			//	SPC_FEMPTY1      	0x100   	FIFO empty in TDC 1
			//	SPC_FEMPTY2      	0x200   	FIFO empty in TDC 2
			//	SPC_FOVL1      	0x400   	FIFO overflow in TDC 1, data lost
			//	SPC_FOVL2      	0x800   	FIFO overflow in TDC 2, data lost
			//	SPC_CTIM_OVER1  	0x8  	collection timer of TDC 1 expired
			//	SPC_CTIM_OVER2  	0x20 	collection timer of TDC 2 expired
			//	SPC_TIME_OVER	0x4 	measurement stopped on expiration of collection timer
			//	SPC_WAIT_FR	0x2000 	image measurement waits for frame signal to stop
			//	SPC_WAIT_TRG 	0x1000 	wait for trigger

		}
	} //NRecords = 0.
	return 0;
}

//////////
void TCSPC_Decode::BackgroundAcq()
{
	decoding = false;
	saturated = false;
	NRecords = 0;

	if (use_pq)
		buffer = new unsigned int[pq->TTREADMAX]();
	else
		buffer = new unsigned int[bh->TTREADMAX]();

	unsigned int* bufCp;
	int nRec = 0;
	bufCp = NULL;

#ifdef _DEBUG
	DebugNotification("Debug: TCSPC_Decode::BackgroundAcq(): Prepare for FLIM background...", 1);
#endif

	while (Running)
	{

		if (decode_thread.joinable())
		{
			decode_thread.join();
		}

		if (bufCp != NULL)
		{
			delete[] bufCp;
			bufCp = NULL;
		}

		if (force_stop && !Running)
		{
#ifdef _DEBUG
			DebugNotification("Debug: Force stop", 1);
#endif
			Running = false;
			engine->Running = false;
			break;
		}


		int retcode = ReadBuffer(nRec); //new value in nRec.

#ifdef _DEBUG
		DebugNotification("Read Buffer: Length = " + to_string(nRec), 2);
#endif

		if (engine->last_event) //Finished the last event.
		{
			Running = false;
			break;
		}

		if (retcode == 0 && nRec > 0 && !saturated)
		{
			bufCp = new unsigned int[nRec];
			memcpy(bufCp, buffer, nRec * sizeof(unsigned int)); //copy data buffer -> bufCp
			decode_thread = thread([this, bufCp, nRec]
			{
				engine->DecodeBuffer(bufCp, nRec);
			});
		}

		if (force_stop || saturated || !Running)
		{
#ifdef _DEBUG
			DebugNotification("Debug: Force stop", 1);
#endif
			Running = false;
			engine->Running = false;
			break;
		}

		if (retcode < 0)
		{
			if (retcode == -10)
			{
#ifdef _DEBUG
				DebugNotification("Debug: Done?? Error - 10. ", 1);
#endif
				Running = false;
				break;
			}
			else if (retcode == -11)
			{
				Notify("Error: Device locked --- will wait for some time");
				this_thread::sleep_for(chrono::milliseconds(waitTime)); //give some time for unlocking.
			}
			else
			{
				NotifyErrorByCode(retcode, "Error in ReadBuffer:");
				Running = false;
				break;
			}
		}//retcode < 0


		this_thread::sleep_for(chrono::milliseconds(waitTime));

	} //Loop

#ifdef _DEBUG
	DebugNotification("Debug: Measurement thread loop break", 1);
#endif

	if (decoding)
	{
		if (decode_thread.joinable())
			decode_thread.join();
		delete[] bufCp;
		decoding = false;
	}

	decoding = false;

	if (!force_stop)
	{
		int retcode = ReadBuffer(nRec); // clear Buffer? if any.
		if (retcode == 0 && nRec > 0 && !saturated)
			engine->DecodeBuffer(buffer, nRec);
	}

	delete[] buffer;

	Notify("MeasurementDone");

#ifdef _DEBUG
	DebugNotification("Debug: Measurement thread done", 1);
#endif

	return;
}

int TCSPC_Decode::BH_allParameters(SPCdata* spc_data)
{
	if (use_pq)
		return -103;

	int retcode = bh->SPC_set_parameters((short)deviceID, spc_data);
	if (retcode < 0)
	{
		NotifyErrorByCode(retcode, "SPC_Set_Parameters error.");
		return retcode;
	}

#ifdef _DEBUG
	DebugNotification("Debug: BH_allParameters: set parameter suceeded", 1);
	ostringstream os;
	os << "Debug: " << endl;
	os << "adc_resolution = " << spc_data->adc_resolution << endl;
	os << "adc_zoom = " << spc_data->adc_zoom << endl;
	os << "cfd_holdoff = " << spc_data->cfd_holdoff << endl;
	os << "cfd_limit_high = " << spc_data->cfd_limit_high << endl;
	os << "cfd_limit_low = " << spc_data->cfd_limit_low << endl;
	os << "cfd_zc_level = " << spc_data->cfd_zc_level << endl;
	os << "dead_time_comp = " << spc_data->dead_time_comp << endl;
	os << "macro_time_clk = " << spc_data->macro_time_clk << endl;
	os << "rate_count_time = " << spc_data->rate_count_time << endl;
	os << "trigger = " << spc_data->trigger << endl;
	os << "mode = " << spc_data->mode << endl;
	os << "tac_gain = " << spc_data->tac_gain << endl;
	os << "tac_range = " << spc_data->tac_range << endl;
	os << "sync_freq_div = " << spc_data->sync_freq_div << endl;
	os << "sync_threshold = " << spc_data->sync_threshold << endl;
	os << "sync_holdoff = " << spc_data->sync_holdoff << endl;
	os << "sync_zc_level = " << spc_data->sync_zc_level << endl;
	DebugNotification(os.str(), 1);
#endif

	return retcode;
}

int TCSPC_Decode::PQ_allParameters(PQControl::pq_parameters* pqp)
{
	if (!use_pq)
		return -103;

	int retcode = 0;
	int NumChannels = 0;
	int SyncTrigEdge = 1;
	int InputTrigEdge = 1; // 1 is negative signal.

	retcode = pq->PQ_GetHardwareInfo(deviceID, Model, Partno, Version); //this is only for information
	if (retcode < 0)
	{
		NotifyErrorByCode(retcode, "PQ_GetHardwareInfo error");
		return retcode;
	}

#ifdef _DEBUG
	DebugNotification("Debug: Model: " + string(Model) + ", Part No: " + string(Partno) + ", Version: " + string(Version), 1);
#endif

	retcode = pq->PQ_GetNumOfInputChannels(deviceID, NumChannels);
	if (retcode < 0)
	{
		NotifyErrorByCode(retcode, "PQ_GetNumOfInputChannels.");
		return retcode;
	}
	else
	{
		if (pqp->NumChannels > NumChannels)
		{
			pqp->NumChannels = NumChannels;
			nChannelsPerDevice = NumChannels;
		}
		else
			nChannelsPerDevice=pqp->NumChannels;
	}

#ifdef _DEBUG
	DebugNotification("Debug: Number of input channels = " + to_string(pqp->NumChannels), 1);
#endif

	retcode = pq->PQ_SetSyncDiv(deviceID, pqp->sync_divider);
	if (retcode < 0)
	{
		NotifyErrorByCode(retcode, "PQ_SetSyncDiv.");
		return retcode;
	}

#ifdef _DEBUG
	DebugNotification("Debug: Frequency devicer " + to_string(pqp->sync_divider), 1);
#endif

	if (strcmp(Model, pq->thP) == 0)
	{
		pqp->hardware = 1;
		retcode = pq->PQ_SetSyncCFD(deviceID, pqp->sync_threshold, pqp->sync_zc_level);
		if (retcode < 0)
		{
			NotifyErrorByCode(retcode, "PQ_SetSyncCFD.");
			return retcode;
		}

#ifdef _DEBUG
		DebugNotification("Debug: sync threshold (CFD) is : " + to_string(pqp->sync_threshold), 1);
		DebugNotification("Debug: sync ZC level (CFD) is : " + to_string(pqp->sync_zc_level), 1);
#endif

		for (int i = 0; i < NumChannels; i++)
		{
			int ch_threshold = 0;
			int zc_level = 0;
			if (i == 0)
			{
				ch_threshold = pqp->ch_threshold0;
				zc_level = pqp->ch_zc_level0;
			}
			else if (i == 1)
			{
				ch_threshold = pqp->ch_threshold1;
				zc_level = pqp->ch_zc_level1;
			}
			else if (i == 2)
			{
				ch_threshold = pqp->ch_threshold2;
				zc_level = pqp->ch_zc_level2;
			}
			else if (i == 3)
			{
				ch_threshold = pqp->ch_threshold3;
				zc_level = pqp->ch_zc_level3;
			}

			retcode = pq->PQ_SetInputCFD(deviceID, i, ch_threshold, zc_level);

			if (retcode < 0)
			{
				NotifyErrorByCode(retcode, "PQ_SetInputCFD. Channel = " + to_string(i));
				return retcode;
			}

#ifdef _DEBUG
			DebugNotification("Debug: Ch" + to_string(i) + " threshold (CFD) is : " + to_string(ch_threshold), 1);
			DebugNotification("Debug: Ch" + to_string(i) + " ZC level (CFD) is : " + to_string(ch_threshold), 1);
#endif
		}
	}
	else if (strcmp(Model, pq->thN) == 0 || engine->acqType == 3) //acqType ==3, harp mode.
	{
		pqp->hardware = 2;
		retcode = pq->PQ_SetSyncEdgeTrg(deviceID, pqp->sync_threshold, SyncTrigEdge);
		if (retcode < 0)
		{
			NotifyErrorByCode(retcode, "PQ_SetSyncEdgeTrg.");
			return retcode;
		}

#ifdef _DEBUG
		DebugNotification("Debug: sync threshold is : " + to_string(pqp->sync_threshold), 1);
		DebugNotification("Debug: Trigger edge is : " + to_string(InputTrigEdge), 1);
#endif

		for (int i = 0; i < NumChannels; i++)
		{
			int ch_threshold = 0;
			if (i == 0)
				ch_threshold = pqp->ch_threshold0;
			else if (i == 1)
				ch_threshold = pqp->ch_threshold1;
			else if (i == 2)
				ch_threshold = pqp->ch_threshold2;
			else if (i == 3)
				ch_threshold = pqp->ch_threshold3;

			retcode = pq->PQ_SetInputEdgeTrg(deviceID, i, ch_threshold, InputTrigEdge);
			if (retcode < 0)
			{
				NotifyErrorByCode(retcode, "PQ_SetInputEdgeTrg.");
				return retcode;
			}

#ifdef _DEBUG
			DebugNotification("Debug: Ch" + to_string(i) + " threshold is : " + to_string(ch_threshold), 1);
#endif
		}
	}

	retcode = pq->PQ_SetSyncChannelOffset(deviceID, pqp->sync_offset);
	if (retcode < 0)
	{
		NotifyErrorByCode(retcode, "PQ_SetSyncChannelOffset.");
		return retcode;
	}

#ifdef _DEBUG
	DebugNotification("Debug: Sync offset is: " + to_string(pqp->sync_offset), 1);
#endif

	for (int i = 0; i < NumChannels; i++)
	{
		int ch_offset;

		if (i == 0)
			ch_offset = pqp->ch_offset0;
		else if (i == 1)
			ch_offset = pqp->ch_offset1;
		else if (i == 2)
			ch_offset = pqp->ch_offset2;
		else if (i == 3)
			ch_offset = pqp->ch_offset3;

		retcode = pq->PQ_SetInputChannelOffset(deviceID, i, ch_offset);
		if (retcode < 0)
		{
			NotifyErrorByCode(retcode, "PQ_SetInputChannelOffset. Ch" + to_string(i));
			return retcode;
		}
#ifdef _DEBUG
		DebugNotification("Debug: Input Channel offset" + to_string(1) + " is: " + to_string(ch_offset), 1);
#endif
	}

	for (int i = 0; i < NumChannels; i++)
	{
		retcode = pq->PQ_SetInputChannelEnable(deviceID, i, 1); //Both enabled.
		if (retcode < 0)
		{
			NotifyErrorByCode(retcode, "PQ_SetInputChannelEnable.");
			return retcode;
		}
	}

	if (pqp->Mode != pq->MODE_T2)
	{
		retcode = pq->PQ_SetBinning(deviceID, pqp->binning);
		if (retcode < 0)
		{
			NotifyErrorByCode(retcode, "PQ_SetBinning.");
		}

#ifdef _DEBUG
		DebugNotification("Debug: Binning : " + to_string(pqp->binning), 1);
#endif
		retcode = pq->PQ_SetOffset(deviceID, 0);
		if (retcode < 0)
		{
			NotifyErrorByCode(retcode, "PQ_SetOffset.");
			return retcode;
		}
	}

	double Resolution = 200;
	retcode = pq->PQ_GetResolution(deviceID, Resolution);
	if (retcode < 0)
	{
		NotifyErrorByCode(retcode, "PQ_GetResolution.");
		return retcode;
	}
	else
	{
		pqp->resolution = Resolution;
	}

#ifdef _DEBUG
	DebugNotification("Debug: Resolution is: " + to_string(Resolution), 1);
#endif

	return (retcode);
}

///////////
int TCSPC_Decode::GetRate(PQControl::pq_rate* pqr)
{
	int retcode = 0;
	int Syncrate = 0;
	int Countrate = 0;

	if (!use_pq)
	{
		rate_values* rates = new rate_values();
		//
		retcode = bh->SPC_read_rates(deviceID, rates);
		pqr->ch_rate0 = (int)rates->cfd_rate;
		pqr->ch_rate1 = (int)rates->cfd_rate;
		pqr->ch_rate2 = (int)rates->cfd_rate;
		pqr->ch_rate3 = (int)rates->cfd_rate;
		pqr->sync_rate = (int)rates->sync_rate;

		return 0;
	}
	else
	{		
		retcode = pq->PQ_GetSyncRate(deviceID, Syncrate);
		if (retcode == -11)
		{
			Notify("Error: Device locked (Rate)");
			return (retcode);
		}
		else if (retcode < 0)
		{
			NotifyErrorByCode(retcode, "PQ_GetSyncRate Error: ");
			return (retcode);
		}
		else
		{
			pqr->sync_rate = Syncrate;
		}

		//DebugNotification("Get Rate ** " + to_string(nChannelsPerDevice), 1);
		

		for (int i = 0; i < nChannelsPerDevice; i++)
		{
			retcode = pq->PQ_GetCountRate(deviceID, i, Countrate);
			if (retcode < 0)
			{
				NotifyErrorByCode(retcode, "PQ_GetCountRate Error, Channel = " + to_string(i) + ": ");
				return (retcode);
			}
			else
			{
#ifdef _DEBUG
				DebugNotification("Debug: PMT count ch = : " + to_string(i) + ":" + to_string(Countrate), 10);
#endif
				if (i == 0)
					pqr->ch_rate0 = Countrate;
				else if (i == 1)
					pqr->ch_rate1 = Countrate;
				else if (i == 2)
					pqr->ch_rate2 = Countrate;
				else if (i == 3)
					pqr->ch_rate3 = Countrate;
			}
		}

		return (0);
	}
}

