/**
 * @file PQControl.cpp
 *
 * @brief This class controls PicoQuant files.
 * Dinamically calls th260lib64.dll or mhlib64 with LoadLibrary and GetProcAddress.
 *
 * @author Ryohei Yasuda
 * @date June 20th 2019
 * Copyright Max Planck Florida Institute for Neuroscience 2019
 *
 */

#include "stdafx.h"
#include "PQControl.h"

bool PQControl::instanceLoaded = false;
bool PQControl::libraryLoaded = false;
HINSTANCE PQControl::hPQ = NULL;
PQControl* PQControl::instance = NULL;

#ifdef _WIN64
char PQControl::libName_TH[MAX_PATH] = "th260lib64"; //type 2
char PQControl::libName_MH[MAX_PATH] = "mhlib64"; //type 3.
#else 
char PQControl::libName_TH[MAX_PATH] = "th260lib"; //type 2
char PQControl::libName_MH[MAX_PATH] = "mhlib"; //type 3.
#endif

PQControl* PQControl::CreatePQControl(int acq_type, char* libName)
{
	if (!instanceLoaded || instance == NULL)
		instance = new PQControl(acq_type, libName);

	return instance;
}

PQControl::PQControl(int acq_type, char* libName)
{
	instance = this;
	instanceLoaded = true;
	acqType = acq_type;
	strcpy_s(libraryName, MAX_PATH, libName);

	int errorcode = LoadPQLibrary(libraryName);

	if (errorcode != 0)
	{
		if (acqType == 2)
		{
			strcpy_s(libraryName, MAX_PATH, libName_TH);
		}
		else if (acqType == 3)
		{
			strcpy_s(libraryName, 16, libName_MH);
		}
		errorcode = LoadPQLibrary(libraryName);
	}

	if (acqType == 2)
		TTREADMAX = TTREADMAX_TH;
	else
		TTREADMAX = TTREADMAX_MH;

}


PQControl::~PQControl()
{
	unLoadPQLibrary();
	instanceLoaded = false;
}


int PQControl::LoadPQLibrary(char* libraryName)
{
	hPQ = LoadLibrary(libraryName);
	libraryLoaded = hPQ != NULL;
	return libraryLoaded ? 0 : -1;
}

void PQControl::unLoadPQLibrary()
{
	if (hPQ != NULL && libraryLoaded)
		FreeLibrary(hPQ);
	libraryLoaded = false;
}

int PQControl::PQ_GetLibraryVersion(char* version)
{
	if (libraryLoaded)
	{
		GetLibraryVersion func = (GetLibraryVersion)GetProcAddress(hPQ, "TH260_GetLibraryVersion");
		if (acqType == 3)
			func = (GetLibraryVersion)GetProcAddress(hPQ, "MH_GetLibraryVersion");
		return func(version);
	}
	else return -102;
}

int PQControl::PQ_GetHardwareInfo(int device, char* Model, char* Partno, char* Version)
{
	if (libraryLoaded)
	{
		GetHardwareInfo func = (GetHardwareInfo)GetProcAddress(hPQ, "TH260_GetHardwareInfo");
		if (acqType == 3)
			func = (GetHardwareInfo)GetProcAddress(hPQ, "MH_GetHardwareInfo");
		return func(device, Model, Partno, Version);
	}
	else
		return -102;
}

int PQControl::PQ_GetErrorString(char* errstring, int errcode)
{
	if (libraryLoaded)
	{
		GetErrorString func = (GetErrorString)GetProcAddress(hPQ, "TH260_GetErrorString");
		if (acqType == 3)
			func = (GetErrorString)GetProcAddress(hPQ, "MH_GetErrorString");
		return func(errstring, errcode);
	}
	else
		return -102;
}

int PQControl::PQ_OpenDevice(int devidx, char* serial)
{
	if (libraryLoaded)
	{
		OpenDevice func = (OpenDevice)GetProcAddress(hPQ, "TH260_OpenDevice");
		if (acqType == 3)
			func = (OpenDevice)GetProcAddress(hPQ, "MH_OpenDevice");
		return func(devidx, serial);
	}
	else
		return -102;
}

int PQControl::PQ_Initialize(int devidx, int mode)
{
	if (libraryLoaded)
	{
		if (acqType == 2)
		{
			Initialize_TH func = (Initialize_TH)GetProcAddress(hPQ, "TH260_Initialize");
			return func(devidx, mode);
		}
		else if (acqType == 3)
		{
			Initialize_MH func = (Initialize_MH)GetProcAddress(hPQ, "MH_Initialize");
			return func(devidx, mode, 0);
		}
		else
			return -103;
	}
	else
		return -102;
}

int PQControl::PQ_GetNumOfInputChannels(int devidx, int &nchannels)
{
	if (libraryLoaded)
	{
		GetNumOfInputChannels func = (GetNumOfInputChannels)GetProcAddress(hPQ, "TH260_GetNumOfInputChannels");
		if (acqType == 3)
			func = (GetNumOfInputChannels)GetProcAddress(hPQ, "MH_GetNumOfInputChannels");
		return func(devidx, nchannels);
	}
	else
		return -102;
}

int PQControl::PQ_SetSyncDiv(int devidx, int div)
{
	if (libraryLoaded)
	{
		SetSyncDiv func = (SetSyncDiv)GetProcAddress(hPQ, "TH260_SetSyncDiv");
		if (acqType == 3)
			func = (SetSyncDiv)GetProcAddress(hPQ, "MH_SetSyncDiv");
		return func(devidx, div);
	}
	else
		return -102;
}

int PQControl::PQ_SetSyncCFD(int devidx, int level, int zerox)
{
	if (libraryLoaded)
	{
		SetSyncCFD func = (SetSyncCFD)GetProcAddress(hPQ, "TH260_SetSyncCFD");
		if (acqType == 3)
			func = (SetSyncCFD)GetProcAddress(hPQ, "MH_SetSyncCFD");
		return func(devidx, level, zerox);
	}
	else
		return -102;
}


int PQControl::PQ_SetSyncEdgeTrg(int devidx, int level, int edge)
{
	if (libraryLoaded)
	{
		SetSyncEdgeTrg func = (SetSyncEdgeTrg)GetProcAddress(hPQ, "TH260_SetSyncEdgeTrg");
		if (acqType == 3)
			func = (SetSyncEdgeTrg)GetProcAddress(hPQ, "MH_SetSyncEdgeTrg");
		return func(devidx, level, edge);
	}
	else
		return -102;
}

int PQControl::PQ_SetSyncChannelOffset(int devidx, int value)
{
	if (libraryLoaded)
	{
		SetSyncChannelOffset func = (SetSyncChannelOffset)GetProcAddress(hPQ, "TH260_SetSyncChannelOffset");
		if (acqType == 3)
			func = (SetSyncChannelOffset)GetProcAddress(hPQ, "MH_SetSyncChannelOffset");
		return func(devidx, value);
	}
	else
		return -102;
}


int PQControl::PQ_SetInputCFD(int devidx, int channel, int level, int zerox)
{
	if (libraryLoaded)
	{
		SetInputCFD func = (SetInputCFD)GetProcAddress(hPQ, "TH260_SetInputCFD");
		if (acqType == 3)
			func = (SetInputCFD)GetProcAddress(hPQ, "MH_SetInputCFD");
		return func(devidx, channel, level, zerox);
	}
	else
		return -102;
}

int PQControl::PQ_SetInputEdgeTrg(int devidx, int channel, int level, int edge)
{
	if (libraryLoaded)
	{
		SetInputEdgeTrg func = (SetInputEdgeTrg)GetProcAddress(hPQ, "TH260_SetInputEdgeTrg");
		if (acqType == 3)
			func = (SetInputEdgeTrg)GetProcAddress(hPQ, "MH_SetInputEdgeTrg");
		return func(devidx, channel, level, edge);
	}
	else
		return -102;
}

int PQControl::PQ_SetInputChannelOffset(int devidx, int channel, int value)
{
	if (libraryLoaded)
	{
		SetInputChannelOffset func = (SetInputChannelOffset)GetProcAddress(hPQ, "TH260_SetInputChannelOffset");
		if (acqType == 3)
			func = (SetInputChannelOffset)GetProcAddress(hPQ, "MH_SetInputChannelOffset");
		return func(devidx, channel, value);
	}
	else
		return -102;
}

int PQControl::PQ_SetInputChannelEnable(int devidx, int channel, int enable)
{
	if (libraryLoaded)
	{
		SetInputChannelEnable func = (SetInputChannelEnable)GetProcAddress(hPQ, "TH260_SetInputChannelEnable");
		if (acqType == 3)
			func = (SetInputChannelEnable)GetProcAddress(hPQ, "MH_SetInputChannelEnable");
		return func(devidx, channel, enable);
	}
	else
		return -102;
}

int PQControl::PQ_SetInputDeadTime(int devidx, int channel, int tdcode)
{
	if (libraryLoaded)
	{
		SetInputDeadTime func = (SetInputDeadTime)GetProcAddress(hPQ, "TH260_SetInputDeadTime");
		if (acqType == 3)
			func = (SetInputDeadTime)GetProcAddress(hPQ, "MH_SetInputDeadTime");
		return func(devidx, channel, tdcode);
	}
	else
		return -102;
}

int PQControl::PQ_SetBinning(int devidx, int binning)
{
	if (libraryLoaded)
	{
		SetBinning func = (SetBinning)GetProcAddress(hPQ, "TH260_SetBinning");
		if (acqType == 3)
			func = (SetBinning)GetProcAddress(hPQ, "MH_SetBinning");
		return func(devidx, binning);
	}
	else
		return -102;
}

int PQControl::PQ_SetOffset(int devidx, int offset)
{
	if (libraryLoaded)
	{
		SetOffset func = (SetOffset)GetProcAddress(hPQ, "TH260_SetOffset");
		if (acqType == 3)
			func = (SetOffset)GetProcAddress(hPQ, "MH_SetOffset");
		return func(devidx, offset);
	}
	else
		return -102;
}

int PQControl::PQ_SetHistoLen(int devidx, int lencode, int &actuallen)
{
	if (libraryLoaded)
	{
		SetHistoLen func = (SetHistoLen)GetProcAddress(hPQ, "TH260_SetHistoLen");
		if (acqType == 3)
			func = (SetHistoLen)GetProcAddress(hPQ, "MH_SetHistoLen");
		return func(devidx, lencode, actuallen);
	}
	else
		return -102;
}

int PQControl::PQ_GetResolution(int devidx, double &resolution)
{
	if (libraryLoaded)
	{
		GetResolution func = (GetResolution)GetProcAddress(hPQ, "TH260_GetResolution");
		if (acqType == 3)
			func = (GetResolution)GetProcAddress(hPQ, "MH_GetResolution");
		return func(devidx, resolution);
	}
	else
		return -102;
}

int PQControl::PQ_GetSyncRate(int devidx, int &syncrate)
{
	if (libraryLoaded)
	{
		GetSyncRate func = (GetSyncRate)GetProcAddress(hPQ, "TH260_GetSyncRate");
		if (acqType == 3)
			func = (GetSyncRate)GetProcAddress(hPQ, "MH_GetSyncRate");
		return func(devidx, syncrate);
	}
	else
		return -102;
}

int PQControl::PQ_GetCountRate(int devidx, int channel, int &cntrate)
{
	if (libraryLoaded)
	{
		GetCountRate func = (GetCountRate)GetProcAddress(hPQ, "TH260_GetCountRate");
		if (acqType == 3)
			func = (GetCountRate)GetProcAddress(hPQ, "MH_GetCountRate");
		return func(devidx, channel, cntrate);
	}
	else
		return -102;
}

int PQControl::PQ_GetWarnings(int devidx, int &warnings)
{
	if (libraryLoaded)
	{
		GetWarnings func = (GetWarnings)GetProcAddress(hPQ, "TH260_GetWarnings");
		if (acqType == 3)
			func = (GetWarnings)GetProcAddress(hPQ, "MH_GetWarnings");
		return func(devidx, warnings);
	}
	else
		return -102;
}

int PQControl::PQ_GetWarningsText(int devidx, char* warningstext, int warnings)
{
	if (libraryLoaded)
	{
		GetWarningsText func = (GetWarningsText)GetProcAddress(hPQ, "TH260_GetWarningsText");
		if (acqType == 3)
			func = (GetWarningsText)GetProcAddress(hPQ, "MH_GetWarningsText");
		return func(devidx, warningstext, warnings);
	}
	else
		return -102;
}

int PQControl::PQ_SetStopOverflow(int devidx, int stop_ovfl, unsigned int stopcount)
{
	if (libraryLoaded)
	{
		SetStopOverflow func = (SetStopOverflow)GetProcAddress(hPQ, "TH260_SetStopOverflow");
		if (acqType == 3)
			func = (SetStopOverflow)GetProcAddress(hPQ, "MH_SetStopOverflow");
		return func(devidx, stop_ovfl, stopcount);
	}
	else
		return -102;
}

int PQControl::PQ_ClearHistMem(int devidx)
{
	if (libraryLoaded)
	{
		ClearHistMem func = (ClearHistMem)GetProcAddress(hPQ, "TH260_ClearHistMem");
		if (acqType == 3)
			func = (ClearHistMem)GetProcAddress(hPQ, "MH_ClearHistMem");
		return func(devidx);
	}
	else
		return -102;
}

int PQControl::PQ_StartMeas(int devidx, int tacq)
{
	if (libraryLoaded)
	{
		StartMeas func = (StartMeas)GetProcAddress(hPQ, "TH260_StartMeas");
		if (acqType == 3)
			func = (StartMeas)GetProcAddress(hPQ, "MH_StartMeas");
		return func(devidx, tacq);
	}
	else
		return -102;
}

int PQControl::PQ_StopMeas(int devidx)
{
	if (libraryLoaded)
	{
		StopMeas func = (StopMeas)GetProcAddress(hPQ, "TH260_StopMeas");
		if (acqType == 3)
			func = (StopMeas)GetProcAddress(hPQ, "MH_StopMeas");
		return func(devidx);
	}
	else
		return -102;
}

int PQControl::PQ_CTCStatus(int devidx, int &ctcstatus)
{
	if (libraryLoaded)
	{
		CTCStatus func = (CTCStatus)GetProcAddress(hPQ, "TH260_CTCStatus");
		if (acqType == 3)
			func = (CTCStatus)GetProcAddress(hPQ, "MH_CTCStatus");
		return func(devidx, ctcstatus);
	}
	else
		return -102;

}

int PQControl::PQ_GetHistogram(int devidx, unsigned int* chcount, int channel, int clear)
{
	if (libraryLoaded)
	{
		GetHistogram func = (GetHistogram)GetProcAddress(hPQ, "TH260_GetHistogram");
		if (acqType == 3)
			func = (GetHistogram)GetProcAddress(hPQ, "MH_GetHistogram");
		return func(devidx, chcount, channel, clear);
	}
	else
		return -102;
}

int PQControl::PQ_ReadFiFo(int devidx, unsigned int* buffer, int count, int &nactual)
{
	if (libraryLoaded)
	{
		if (acqType == 2)
		{
			ReadFiFo_TH func = (ReadFiFo_TH)GetProcAddress(hPQ, "TH260_ReadFiFo");
			return func(devidx, buffer, count, nactual);
		}
		else if (acqType == 3)
		{
			ReadFiFo_MH func = (ReadFiFo_MH)GetProcAddress(hPQ, "MH_ReadFiFo");
			return func(devidx, buffer, nactual);
		}
		else
			return -102;
	}
	else
		return -102;
}

int PQControl::PQ_GetFlags(int devidx, int &flags)
{
	if (libraryLoaded)
	{
		GetFlags func = (GetFlags)GetProcAddress(hPQ, "TH260_GetFlags");
		if (acqType == 3)
			func = (GetFlags)GetProcAddress(hPQ, "MH_GetFlags");
		return func(devidx, flags);
	}
	else
		return -102;
}

int PQControl::PQ_CloseDevice(int devidx)
{
	if (libraryLoaded)
	{
		CloseDevice func = (CloseDevice)GetProcAddress(hPQ, "TH260_CloseDevice");
		if (acqType == 3)
			func = (CloseDevice)GetProcAddress(hPQ, "MH_CloseDevice");
		return func(devidx);
	}
	else
		return -102;
}

int PQControl::PQ_SetMarkerEdges(int devidx, int me0, int me1, int me2, int me3)
{
	if (libraryLoaded)
	{
		SetMarkerEdges func = (SetMarkerEdges)GetProcAddress(hPQ, "TH260_SetMarkerEdges");
		if (acqType == 3)
			func = (SetMarkerEdges)GetProcAddress(hPQ, "MH_SetMarkerEdges");
		return func(devidx, me0, me1, me2, me3);
	}
	else
		return -102;
}

int PQControl::PQ_SetMarkerHoldoffTime(int devidx, int holdofftime)
{
	if (libraryLoaded)
	{
		SetMarkerHoldoffTime func = (SetMarkerHoldoffTime)GetProcAddress(hPQ, "TH260_SetMarkerHoldoffTime");
		if (acqType == 3)
			func = (SetMarkerHoldoffTime)GetProcAddress(hPQ, "MH_SetMarkerHoldoffTime");
		return func(devidx, holdofftime);
	}
	else
		return -102;
}


