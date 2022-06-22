/**
 * @file dllmain.cpp
 *
 * @brief DLL output/input control for TCSPC_Decode.dll
 * The dll controls TCSPC board and produce images.
 *
 * @author Ryohei Yasuda
 * @date June 20th 2019
 * Copyright Max Planck Florida Institute for Neuroscience 2019
 *
 */

#include "stdafx.h"
#include "TCSPC_Decode.h"
#include "AccessControl.h"

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}


//////////////
DllExport_int Get_ComputerID()
{
	return AccessControl::TestSerialID(0);
}

DllExport_int Check_SerialKey(int serialID)
{
	AccessControl::TestSerialID(serialID);
	return (int)AccessControl::compIDOK;
}

DllExport_int Start_TCSPC_Decode(int id, callback myFunc, DecodeEngine::parameters* pm,
	AccessControl::accessID* access, char* dllPath)
{
	bool serialOK = false;
	int compID = AccessControl::TestSerialID(access->FLIMSerial);
	access->compID = compID;

	serialOK = AccessControl::compIDOK;
	string str1 = "";
	char* cstr;

#ifdef _DEBUG
	str1 = "Debug: CompID = " + to_string(access->compID) +
		", Serial ID = " + to_string(access->FLIMSerial) + "\nMacAddr = ";
	for (int i = 0; i < 12; i++)
		str1 = str1 + to_string((int)AccessControl::mac_addr[i]) + ",";
	cstr = const_cast<char*>(str1.c_str());
	myFunc(id, cstr);
#endif

	if (!serialOK)
	{
		str1 = "Error: Wrong serial ID";
		cstr = const_cast<char*>(str1.c_str());
		myFunc(id, cstr);
		return -101;
	}

	TCSPC_Decode::CreateTCSPC_Decode(id);

	int retcode = TCSPC_Decode::instances[id]->InitializeDecodeEngine(myFunc, pm, dllPath);
	if (retcode < 0)
	{
		str1 = "Error: DLLmain: Start_TCSPC_Decode DLL was not loaded";
		cstr = const_cast<char*>(str1.c_str());
		myFunc(id, cstr);
		return retcode; //If DLL is not there, it does not matter if serial is good or not.
	}
	else
	{
#ifdef _DEBUG
		str1 = "Debug: DLLmain: Start_TCSPC_Decode DLL loading suceeded";
		cstr = const_cast<char*>(str1.c_str());
		myFunc(id, cstr);
#endif
	}

	retcode = TCSPC_Decode::instances[id]->OpenDevice();
	if (retcode < 0)
		return retcode;
	else
	{
		retcode = TCSPC_Decode::instances[id]->InitializeDevice();
		return retcode;
	}
}

//Set and put parameters for PicoQuant card.
DllExport_int PQ_AllParameters(int id, PQControl::pq_parameters *pqr)
{
	if (TCSPC_Decode::instanceStatus[id] == 1)
		return TCSPC_Decode::instances[id]->PQ_allParameters(pqr);
	else
		return -101;
}

//Set and put parameters for Becker and Hicl card.
DllExport_int BH_AllParameters(int id, SPCdata *spc_data)
{
	if (TCSPC_Decode::instanceStatus[id] == 1)
		return TCSPC_Decode::instances[id]->BH_allParameters(spc_data);
	else
		return -101;
}

DllExport_int Start_Measurement(int id, DecodeEngine::parameters* param1)
{
	if (TCSPC_Decode::instanceStatus[id] == 1)
	{

#ifdef _DEBUG
		TCSPC_Decode::instances[id]->DebugNotification("Debug: DLLmain: Start_Measurement Begin", 1);
#endif

		int retcode = TCSPC_Decode::instances[id]->StartMeasurement(param1);
		TCSPC_Decode::instances[id]->StartBackgroundMeasurement();

#ifdef _DEBUG
		TCSPC_Decode::instances[id]->DebugNotification("Debug: DLLmain: Start_Measurement Done", 1);
#endif

		return retcode;
	}
	else
		return -101;
}

DllExport_int Stop_Measurement(int id, int force)
{
	if (TCSPC_Decode::instanceStatus[id] == 1)
	{
#ifdef _DEBUG
		TCSPC_Decode::instances[id]->DebugNotification("Debug: DLLmain: Stop_Measurement Begin", 1);
#endif

		bool _force = force != 0;
		int retcode = TCSPC_Decode::instances[id]->StopMeasurement(_force);

#ifdef _DEBUG
		TCSPC_Decode::instances[id]->DebugNotification("Debug: DLLmain: Stop measurement Done. retcode = " + to_string(retcode), 1);
#endif	
		return 0;
	}
	else
		return -101;
}

//Get photon counting rate and laser pulse rate.
DllExport_int GetRate(int id, PQControl::pq_rate* pqr)
{
	if (TCSPC_Decode::instanceStatus[id] == 1)
	{
		return TCSPC_Decode::instances[id]->GetRate(pqr);
	}
	else
		return -101;
}

DllExport_int Close_Device(int id)
{
	if (TCSPC_Decode::instanceStatus[id] == 1)
		return TCSPC_Decode::instances[id]->CloseDevice();
	else
		return -101;
}

///////////////////////////////////////////////////////////////
//For Testing decode engine. Not used for data acquisition
/////////////////////////////////////////////////////////////
DllExport_int StartDecodeEngine(int id, callback myFunc, AccessControl::accessID* access)
{
	int compID = AccessControl::TestSerialID(access->FLIMSerial);
	access->compID = compID;

	string s = "Debug: Starting Decode Engine";
	char* cstr = const_cast<char*>(s.c_str());
	myFunc(id, cstr);

	if (!AccessControl::compIDOK)
	{
		s = "Error: Computer ID and FLIM serial does not much: Computer ID = " + to_string(access->compID) + ", FLIM serial = " + to_string(access->FLIMSerial);
		cstr = const_cast<char*>(s.c_str());
		myFunc(id, cstr);
		return -101;
	}

	auto engine = DecodeEngine::CreateDecodeEngine(id);
	new DLL_Listener(id, myFunc);
	engine->RegisterListener(DLL_Listener::dllA[id]);
	engine->FLIMbuffer->RegisterListener(DLL_Listener::dllA[id]);

	s = "Debug: Start decoding engine done";
	cstr = const_cast<char*>(s.c_str());
	myFunc(id, cstr);

	return 0;
}

DllExport_int InitializeDecodeEngine(int id, DecodeEngine::parameters* pm)
{
	DecodeEngine::instances[id]->debug = pm->debug;
	DecodeEngine::instances[id]->FLIMbuffer->debug = pm->debug;
	DecodeEngine::instances[id]->DebugNotification("Debug: Initialize decode engine", 1);
	DecodeEngine::instances[id]->Initialize(pm);
	//DecodeEngine::instances[id]->DebugNotification("Debug: Finished decode engine initializing", 1);

	return 0;
}

DllExport_int DecodeBuffer(int id, unsigned int* buffer, int nRecords)
{
#ifdef _DEBUG
	if (DecodeEngine::instances[id]->debug >= 1)
	{
		ostringstream os;
		os << "Debug: DecodeBuffer buffer[0] = " << buffer[0] << ", " << " nRecords = " << nRecords;
		DecodeEngine::instances[id]->DebugNotification(os.str(), 1);
	}
#endif

	int returnVal = 0;
	if (DecodeEngine::instanceStatus[id] == 1 && MemoryManager::instanceStatus[id] == 1)
	{

		DecodeEngine::instances[id]->DecodeBuffer(buffer, nRecords);
		returnVal = 0;
	}
	else
		returnVal = -1;

	//if (DecodeEngine::instances[id]->debug >= 1)
	//{
	//	DecodeEngine::instances[id]->DebugNotification("Debug: DecodeBuffer done. Return value = " + to_string(returnVal), 1);
	//}

	return returnVal;
}

DllExport_int DE_GetLineCount(int id)
{
	return DecodeEngine::instances[id]->lineCounter;
}

DllExport_int DE_GetFrameCount(int id)
{
	return DecodeEngine::instances[id]->totalFrameCounter;
}

DllExport_int DE_GetPhotonCount(int id)
{
	return DecodeEngine::instances[id]->PhotonCounter;
}

DllExport_int DE_GetData(int id, unsigned short* Dest, int channel, int zloc)
{
	if (DecodeEngine::instances[id]->debug >= 1)
	{
		ostringstream os;
		os << "Debug: DE_GetData: id = " << id << ", channel = " << channel << ", zloc = " << zloc;
		DecodeEngine::instances[id]->DebugNotification(os.str(), 1);
	}

	int returnVal = 0;
	if (DecodeEngine::instanceStatus[id] == 1 && MemoryManager::instanceStatus[id] == 1)
	{
		returnVal = DecodeEngine::instances[id]->GetData(Dest, channel, zloc);
	}
	else
	{
		returnVal = -1;
	}

	if (DecodeEngine::instances[id]->debug >= 1)
	{
		//		DecodeEngine::instances[id]->DebugNotification("Debug: DE_GetData done. Return value = " + to_string(returnVal), 1);
	}

	return returnVal;
}

DllExport_int DE_GetDataLine(int id, unsigned short* Dest, int channel, int zloc, int startLine, int endLine)
{
	if (DecodeEngine::instances[id]->debug >= 1)
	{
		ostringstream os;
		os << "Debug: DE_GetDataLine: id = " << id << ", channel = " << channel << ", zloc = " << zloc;
		os << "line = " << startLine << ": " << endLine;
		DecodeEngine::instances[id]->DebugNotification(os.str(), 1);
	}

	int returnVal = 0;
	if (DecodeEngine::instanceStatus[id] == 1 && MemoryManager::instanceStatus[id] == 1)
	{
		returnVal = DecodeEngine::instances[id]->GetDataLine(Dest, channel, zloc, startLine, endLine);
	}
	else
	{
		returnVal = -1;
	}

	if (DecodeEngine::instances[id]->debug >= 1)
	{
		//DecodeEngine::instances[id]->DebugNotification("Debug: DE_GetDataLine done. Return value = " + to_string(returnVal), 1);
	}
	return returnVal;
}


DllExport_int calcKHz(int id, double &value)
{
	if (DecodeEngine::instances[id]->debug >= 1)
		DecodeEngine::instances[id]->DebugNotification("Debug: calcKHz start", 1);

	value = DecodeEngine::instances[id]->calcKHz();
	if (DecodeEngine::instances[id]->debug >= 1)
	{
		//		DecodeEngine::instances[id]->DebugNotification("Debug: calcKHz done. Return value = " + to_string(returnVal), 1);
	}
	return 0;
}

////////////////For Memory Manager testing///////////////////////
////////////////////////////////////////////////////////////////
//DllExport MM_RegisterListener(int id, callback myFunc)
//{
//	new DLL_Listener(id, myFunc);
//	MemoryManager::memInstances[id]->RegisterListener(DLL_Listener::dllA[id]);
//}


DllExport_int StartMemoryManager(int id, int n_channels, int n_z, int n_y, int n_x, unsigned int* n_time)
{
	MemoryManager::CreateMemoryManager(id);
	MemoryManager::memInstances[id]->InitializeMemory(n_channels, n_z, n_y, n_x, n_time);
	return 0;
}

DllExport_int AddToPixel(int id, unsigned int c, unsigned int z, unsigned int y, unsigned int x, unsigned int t)
{
	if (MemoryManager::instanceStatus[id] == 1)
	{
		MemoryManager::memInstances[id]->AddToPixel(c, z, y, x, t);
		return 0;
	}
	else
		return -1;
}


DllExport_int GetPixelValue(int id, unsigned int c, unsigned int z, unsigned int y, unsigned int x, unsigned int t, unsigned short &value)
{
	if (MemoryManager::instanceStatus[id] == 1)
	{
		MemoryManager* mm = MemoryManager::memInstances[id];
		value = mm->FLIMData_Measure[c][z][(y * mm->nPixels + x) * mm->nDtime[c] + t];
		return 0;
	}
	else
	{
		return -1;
	}
}

DllExport_int AddToPixelsBlock(int id, unsigned int* c, unsigned int* z, unsigned int* y, unsigned int* x, unsigned int* t, int startP, int endP)
{
	if (MemoryManager::instanceStatus[id] == 1)
	{
		MemoryManager::memInstances[id]->AddToPixelsBlock(c, z, y, x, t, startP, endP);
		return 0;
	}
	else
		return -1;
}

DllExport_int SwitchMemoryBank(int id, int* eraseChannels)
{
	if (MemoryManager::instanceStatus[id] == 1)
	{
		MemoryManager::memInstances[id]->SwitchMemoryBank(eraseChannels);
		return 0;
	}
	else
		return -1;
}

DllExport_int EraseMeasurementMemory(int id, int* eraseChannels)
{
	if (MemoryManager::instanceStatus[id] == 1)
	{
		MemoryManager::memInstances[id]->clearMemoryBank(eraseChannels);
		return 0;
	}
	else
		return -1;
}

DllExport_int GetData(int id, unsigned short* Data, int channel, int z_loc)
{
	if (MemoryManager::instanceStatus[id] == 1)
	{
		MemoryManager* mm = MemoryManager::memInstances[id];
		if (channel < mm->nChannels && z_loc < mm->nZlocs)
		{
			memcpy(Data, mm->FLIMData_Measure[channel][z_loc], mm->nPixels * mm->nLines * mm->nDtime[channel] * sizeof(unsigned short));
			return 0;
		}
		else
			return -10;
	}
	else
	{
		return -1;
	}
}


DllExport_int GetDataLines(int id, unsigned short* Data, int channel, int z_loc, int startLine, int endLine)
{
	if (MemoryManager::instanceStatus[id] == 1)
	{
		MemoryManager* mm = MemoryManager::memInstances[id];
		if (channel < mm->nChannels && z_loc < mm->nZlocs)
		{
			int loc = startLine * (mm->nPixels * mm->nDtime[channel]);
			int length = (endLine - startLine) * (mm->nPixels * mm->nDtime[channel]) * sizeof(unsigned short);

			memcpy(Data + loc, mm->FLIMData_Measure[channel][z_loc] + loc, length);
			return 0;
		}
		else
			return -10;
	}
	else
	{
		return -1;
	}
}

DllExport_int ClearObject(int id)
{
	if (MemoryManager::instanceStatus[id] == 1)
	{
		MemoryManager::memInstances[id]->deleteMemoryBank();
		return 0;
	}
	else
		return -1;
}


