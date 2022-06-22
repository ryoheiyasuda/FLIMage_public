#pragma once

class PQControl
{
public:
	static HINSTANCE hPQ;
	static bool instanceLoaded;
	static bool libraryLoaded;
	static PQControl* instance;
	static PQControl* CreatePQControl(int acq_type, char* libName);

	static const int PQ_ERROR_DEVICE_OPEN_FAIL = -1;
	static const int TTREADMAX_TH = 131062;
	static const int TTREADMAX_MH = 1048576;
	static const int FLAG_FIFOFULL = 0x0002;
	static const int MODE_T2 = 2;
	static const int Mode_T3 = 3;

	static char libName_TH[MAX_PATH];
	static char libName_MH[MAX_PATH];

	char libraryName[MAX_PATH];
	int TTREADMAX;
	int acqType = 2;

	char thP[16] = "TimeHarp 260 P";
	char thN[16] = "TimeHarp 260 N";

	PQControl(int acq_type, char* libName);
	~PQControl();
	int LoadPQLibrary(char* libName);
	void unLoadPQLibrary();

#pragma pack(push,1)
	struct pq_parameters
	{
		int Mode;

		int NumChannels;
		int sync_divider;
		int sync_threshold;
		int sync_zc_level;

		int ch_threshold0;
		int ch_threshold1;
		int ch_threshold2;
		int ch_threshold3;

		int ch_zc_level0;
		int ch_zc_level1;
		int ch_zc_level2;
		int ch_zc_level3;

		int sync_offset;

		int ch_offset0;
		int ch_offset1;
		int ch_offset2;
		int ch_offset3;

		double resolution;
		int binning;

		int hardware;
	};


	struct pq_rate
	{
		int sync_rate;
		int ch_rate0;
		int ch_rate1;
		int ch_rate2;
		int ch_rate3;
	};

#pragma pack( pop )

	int PQ_GetLibraryVersion(char* version);
	int PQ_GetErrorString(char* errstring, int errcode);
	int PQ_OpenDevice(int devidx, char* serial);
	int PQ_Initialize(int devidx, int mode);
	int PQ_GetHardwareInfo(int device, char* Model, char* Partno, char* Version);
	int PQ_GetNumOfInputChannels(int devidx, int &nchannels);
	int PQ_SetSyncDiv(int devidx, int div);
	int PQ_SetSyncCFD(int devidx, int level, int zerox);
	int PQ_SetSyncEdgeTrg(int devidx, int level, int edge);
	int PQ_SetSyncChannelOffset(int devidx, int value);
	int PQ_SetInputCFD(int devidx, int channel, int level, int zerox);
	int PQ_SetInputEdgeTrg(int devidx, int channel, int level, int edge);
	int PQ_SetInputChannelOffset(int devidx, int channel, int value);
	int PQ_SetInputChannelEnable(int devidx, int channel, int enable);
	int PQ_SetInputDeadTime(int devidx, int channel, int tdcode);
	int PQ_SetBinning(int devidx, int binning);
	int PQ_SetOffset(int devidx, int offset);
	int PQ_SetHistoLen(int devidx, int lencode, int &actuallen);
	int PQ_GetResolution(int devidx, double &resolution);
	int PQ_GetSyncRate(int devidx, int &syncrate);
	int PQ_GetCountRate(int devidx, int channel, int &cntrate);
	int PQ_GetWarnings(int devidx, int &warnings);
	int PQ_GetWarningsText(int devidx, char* warningstext, int warnings);
	int PQ_SetStopOverflow(int devidx, int stop_ovfl, unsigned int stopcount);
	int PQ_ClearHistMem(int devidx);
	int PQ_StartMeas(int devidx, int tacq);
	int PQ_StopMeas(int devidx);
	int PQ_CTCStatus(int devidx, int &ctcstatus);
	int PQ_GetHistogram(int devidx, unsigned int* chcount, int channel, int clear);
	int PQ_ReadFiFo(int devidx, unsigned int* buffer, int count, int &nactual);
	int PQ_GetFlags(int devidx, int &flags);
	int PQ_CloseDevice(int devidx);
	int PQ_SetMarkerEdges(int devidx, int me0, int me1, int me2, int me3);
	int PQ_SetMarkerHoldoffTime(int devidx, int holdofftime);
};

typedef int(__cdecl *GetLibraryVersion)(char* a);
typedef int(__cdecl *GetHardwareInfo)(int device, char* Model, char* Partno, char* Version);
typedef int(__cdecl *GetErrorString)(char* errstring, int errcode);
typedef int(__cdecl *OpenDevice)(int devidx, char* serial);
typedef int(__cdecl *Initialize_TH)(int devidx, int mode);
typedef int(__cdecl *Initialize_MH)(int devidx, int mode, int refsource);
typedef int(__cdecl *GetNumOfInputChannels)(int devidx, int &nchannels);
typedef int(__cdecl *SetSyncDiv)(int devidx, int div);
typedef int(__cdecl *SetSyncCFD)(int devidx, int level, int zerox);
typedef int(__cdecl *SetSyncEdgeTrg)(int devidx, int level, int edge);
typedef int(__cdecl *SetSyncChannelOffset)(int devidx, int value);
typedef int(__cdecl *SetInputCFD)(int devidx, int channel, int level, int zerox);
typedef int(__cdecl *SetInputEdgeTrg)(int devidx, int channel, int level, int edge);
typedef int(__cdecl *SetInputChannelOffset)(int devidx, int channel, int value);
typedef int(__cdecl *SetInputChannelEnable)(int devidx, int channel, int enable);
typedef int(__cdecl *SetInputDeadTime)(int devidx, int channel, int tdcode);
typedef int(__cdecl *SetBinning)(int devidx, int binning);
typedef int(__cdecl *SetOffset)(int devidx, int offset);
typedef int(__cdecl *SetHistoLen)(int devidx, int lencode, int &actuallen);
typedef int(__cdecl *GetResolution)(int devidx, double &resolution);
typedef int(__cdecl *GetSyncRate)(int devidx, int &syncrate);
typedef int(__cdecl *GetCountRate)(int devidx, int channel, int &cntrate);
typedef int(__cdecl *GetWarnings)(int devidx, int &warnings);
typedef int(__cdecl *GetWarningsText)(int devidx, char* warningstext, int warnings);
typedef int(__cdecl *SetStopOverflow)(int devidx, int stop_ovfl, unsigned int stopcount);
typedef int(__cdecl *ClearHistMem)(int devidx);
typedef int(__cdecl *StartMeas)(int devidx, int tacq);
typedef int(__cdecl *StopMeas)(int devidx);
typedef int(__cdecl *CTCStatus)(int devidx, int &ctcstatus);
typedef int(__cdecl *GetHistogram)(int devidx, unsigned int* chcount, int channel, int clear);
typedef int(__cdecl *ReadFiFo_TH)(int devidx, unsigned int* buffer, int count, int &nactual);
typedef int(__cdecl *ReadFiFo_MH)(int devidx, unsigned int* buffer, int &nactual);
typedef int(__cdecl *GetFlags)(int devidx, int &flags);
typedef int(__cdecl *CloseDevice)(int devidx);
typedef int(__cdecl *SetMarkerEdges)(int devidx, int me0, int me1, int me2, int me3);
typedef int(__cdecl *SetMarkerHoldoffTime)(int devidx, int holdofftime);


