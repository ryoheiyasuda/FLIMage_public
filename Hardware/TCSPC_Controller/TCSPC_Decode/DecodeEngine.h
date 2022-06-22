#pragma once
#include "MemoryManager.h"
#include "Listener.h"

class DecodeEngine : public Notifier
{
public:
	static const unsigned long T3WRAPAROUND = 1024;
	static const unsigned long T2WRAPAROUND = 33554432;
	static const unsigned long WRAPAROUND_BH = 4096;
	static const int MAXDEVNUM = 4;
	static const int MODE_T2 = 2;
	static const int MODE_T3 = 3;
	static const int MAXLENCODE = 5;
	static const int MAXINPCHAN = 4;
	static const int Version_PQ = 3;
	static const long largeValue = 2147483648; //2^31

	static const int maxNinstances = 3;
	static int* instanceStatus;
	static DecodeEngine** instances;
	static DecodeEngine* CreateDecodeEngine(int ID);

	bool Running = false;
	MemoryManager* FLIMbuffer;

	int lineID = 3;
	int frameID = 2;
	int tagID = 1;

	int skipFirstLines = 0;
	int pixel_binning = 0;

	int frameCounter;
	int totalFrameCounter;
	int totalFrameCounterOut;
	double d_lineCounter;
	unsigned int lineCounter;
	unsigned int aveFrameCounter;
	int PhotonCounter;

	//Tags
	unsigned long tagCounter;
	unsigned long tagTimeStart;
	bool waitFirstTagEvent;

	unsigned long timeFromTag;
	unsigned long lastTagTime;
	unsigned long tagTime;

	int PQ_Mode = MODE_T3;

	unsigned short*** StripeBuffer;
	int line_StripeCounter = 0;
	int StripeCounter = 0;
	int LinesPerStripe = 0;
	int nStripes;
	bool StripeDuringFocus;

	//Remember values for next round of FiFo acquisition
	unsigned long truensync;
	unsigned long oflcorrection; //Need to put on top? need to make it global;
	unsigned long lineCorrection;
	unsigned long lastSyncEvent;

	unsigned long imageWidthCount;

	int NRecords;

	int nFrames = 1000;

	unsigned int nDtime[MAXINPCHAN];
	unsigned int startPoint; //For FLIM.
	unsigned int endPoint; //for FLIM.

	int nLines;
	int nPixels;
	int nZlocs;
	double msPerLine;
	double measured_line_time_correction;
	double line_time_correction = 1;
	double pixel_time;

	int n_average = 1;
	int nTotalLines = 0;

	unsigned int syncRate;
	unsigned long imageOffset;
	unsigned long imageOffset_Rev;
	unsigned int pixelCount;
	double time_per_unit = 1.25e-08; //50 ns.Revision B ?
	double time_per_unitC = 1.25e-08; //Corrected for actual time.
	double line_correction_average = 0;

	int aveFrame[MAXINPCHAN];
	int acquireFLIM[MAXINPCHAN]; // if average.
	int acquisition[MAXINPCHAN];
	int eraseMemory[MAXINPCHAN];

	int eraseMemory_acq[MAXINPCHAN];

	int acqType = 2; //2 PQ, 3 MH, 1, BH
	int acq_modePQ = MODE_T3;
	double resolution = 200.0; //ps
	int binning = 2;

	unsigned int channelPerDevice;
	int device = 0;

	bool first_event = true;
	bool frame_event_triggered = false;
	bool last_event = true;
	bool acq_done = false;
	bool focusing = false;
	bool saturated = false;

	bool syncEvent;
	bool line;
	bool frame = false;
	bool tagMark;
	//bool overflow;

	unsigned long nsync;
	unsigned int channel;
	unsigned int dtime;
	unsigned int xLoc;
	unsigned long pixelLoc;
	unsigned int zLoc;
	unsigned int yLoc;

	int BiDirectionalScanX = 0;
	int BiDirectionalScanY = 0;

	//Taglens related.
	bool TagMode = false;

	int fastZ_measureTagParameters;
	double fastZ_FrequencyKHz; //KHerz
	float fastZ_ZScanPerPixel;
	unsigned int fastZ_ZScanPerPixel_Bidirecitonal;
	double fastZ_XYFillFraction;
	double fastZ_VoxelTimeUs; //Microsecond;
	int fastZ_ZScanPerLine;
	int fastZ_nFastZSlices;
	int fastZ_VoxelCount;
	double fastZ_phaseRange[2];
	unsigned int fastZ_phaseRangeCount[2];
	bool fastZ_phase_detection_mode = false;

	unsigned int fastZ_CountPerFastZCycle = 425; //Count
	unsigned int fastZ_CountPerFastZCycleHalf = 212;
	unsigned int fastZ_CountPerFastZSlice = 15;
	unsigned int fastZ_residual_for_PhaseDetection = 0;

	int debug;

	thread stripeTask, frameTask;

#pragma pack(push,1)
	struct parameters {
		int n_average;
		double resolution;
		int binning;
		int enableFastZscan; //0 or 1
		int nZlocs; //ZSCANNNING
		int nLines;
		int	nPixels;
		int	nFrames;
		int nChannels; //not used though.

		int BiDirectionalScanX; //1 = 1 pulse per line. 2 = 1 pulse per 2 line (round trip).
		int BiDirectionalScanY;
		int TagID;
		int LineID;
		int FrameID;
		int skipFirstLines = 0;
		int pixel_binning = 0;

		//TCSPC parameters
		int acqType; //PQ = 2, 3
		int acq_modePQ;
		double time_per_unit;
		double pixel_time;
		double line_time_correction;
		double msPerLine;

		int nDtime0;
		int nDtime1;
		int nDtime2;
		int nDtime3;

		int nStartPoint;
		int nEndPoint;

		int acquisition0;
		int acquisition1;
		int acquisition2;
		int acquisition3;

		int acquireFLIM0;
		int acquireFLIM1;
		int acquireFLIM2;
		int acquireFLIM3;

		int aveFrame0;
		int aveFrame1;
		int aveFrame2;
		int aveFrame3;

		int focus;
		int StripeDuringFocus;
		int LinesPerStripe;

		double AcquisitionDelay;
		double BiDirectionalDelay;

		int eraseMemory0;
		int eraseMemory1;
		int eraseMemory2;
		int eraseMemory3;

		int fastZ_measureTagParameters;
		double fastZ_FrequencyKHz; //KHerz
		float fastZ_ZScanPerPixel;
		unsigned int fastZ_ZScanPerPixel_Bidirecitonal;
		double fastZ_XYFillFraction;
		double fastZ_VoxelTimeUs; //Microsecond;
		int fastZ_ZScanPerLine;
		int fastZ_nFastZSlices;
		int fastZ_VoxelCount;
		double fastZ_phaseRangeStart;
		double fastZ_phaseRangeEnd;
		unsigned int fastZ_phaseRangeCountStart;
		unsigned int fastZ_phaseRangeCountEnd;
		int fastZ_phase_detection_mode;

		unsigned int fastZ_CountPerFastZCycle; //Count
		unsigned int fastZ_CountPerFastZCycleHalf;
		unsigned int fastZ_CountPerFastZSlice;
		unsigned int fastZ_residual_for_PhaseDetection;

		int debug;
	};
#pragma pack( pop )

	DecodeEngine(int device);
	~DecodeEngine();
	void DecodeBuffer(unsigned int* Buffer, int nRecords);
	void DecodeT3(unsigned int buf);
	void DecodeT2(unsigned int buf);
	void DecodeBH(unsigned int buf);
	void ProcessLine();
	void ProcessTag();
	void ProcessPhoton();
	void DoStripeEvent(bool frameEvent, bool eraseMemory);
	void Initialize(parameters *pm);
	void SetParameters(parameters *pm);
	void DebugNotification(string message, int debug_level);
	double calcKHz();
	bool AllOne(int* Arr1, int len);
	int GetData(unsigned short* Data, int channel, int z_loc);
	int GetDataLine(unsigned short* Data, int channel, int z_loc, int startLine, int endLine);

private:

};

