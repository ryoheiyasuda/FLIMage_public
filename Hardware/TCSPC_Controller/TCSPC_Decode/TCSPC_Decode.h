#pragma once
#include "PQControl.h"
#include "BHControl.h"
#include "DecodeEngine.h"
#include <condition_variable>
#include "Utilities.h"

//#include "stdafx.h"


class TCSPC_Decode : public Notifier
{
public:
	PQControl* pq;
	BHControl* bh;

	DecodeEngine* engine;
	//DecodeEngine::parameters* pm;

	static int* instanceStatus;
	static TCSPC_Decode** instances;
	static TCSPC_Decode* CreateTCSPC_Decode(int ID);
	static const int error_str_length = 100;

	static const int MAXHOUR = 2;
	static const int BH_SPC150 = 1;
	static const int BH_MaxDev = 8;
	static const int PQ_TH260 = 2;
	static const int PQ_MULTIHARP = 3;

	int nChannelsPerDevice;
	int deviceID;
	bool deviceOpen = false;

	int debug;
	bool focusing;
	bool saturated = false;
	bool last_event = false;
	bool force_stop = false;
	bool decoding = false;
	bool Running = false;
	bool use_pq = false;

	char Serial[8];
	char Errstr[error_str_length]; //can be 40 for MH, but it is OK.
	char Version[16];
	char Model[16];
	char Partno[8];

	unsigned int* buffer;
	int NRecords = 0;
	int waitTime = 2;

	mutex mut;
	condition_variable cond_var;

	thread decode_thread;
	thread meas_thread;
	bool meas_thread_on;

	TCSPC_Decode(int ID);
	~TCSPC_Decode();

	int OpenDevice();
	int CloseDevice();
	int StartMeasurement(DecodeEngine::parameters* param1);
	int StopMeasurement(bool force);

	int InitializeDevice();
	int ReadBuffer(int& NRec);
	void BackgroundAcq();

	int GetRate(PQControl::pq_rate* pqr);
	int BH_allParameters(SPCdata* spc_data);
	int PQ_allParameters(PQControl::pq_parameters* pqp);
	void StartBackgroundMeasurement();

	int InitializeDecodeEngine(callback myfunc, DecodeEngine::parameters* param1, char* DLL_Path);
	void DebugNotification(string message, int debug_level);
	void NotifyErrorByCode(int error_code, string error_message);

private:
	void _Measurement_Thread_Join();
};

