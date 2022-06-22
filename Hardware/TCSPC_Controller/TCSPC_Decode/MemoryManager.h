#include "Listener.h"

class MemoryManager: public Notifier
{
public:
	static const int maxNinstances = 3; //Up to 3 TCSPC cards handled.
	static int* instanceStatus;
	static MemoryManager** memInstances;
	static MemoryManager* CreateMemoryManager(int ID);

	int debug = 1;

	//Channel parameters.
	int instanceID;

	int nChannels;
	int nZlocs;
	int nLines;
	int nPixels;
	int* nDtime;
	unsigned short*** FLIMData_Measure;

	bool TagMode = false;


	MemoryManager(int dev);
	~MemoryManager();

	void InitializeMemory(int ch_in, int z_in, int y_in, int x_in, unsigned int* dtime_in);

	void AddToPixel(unsigned int channel, unsigned int zloc, unsigned int yloc, unsigned int xloc, unsigned int tloc);
	void AddToPixelsBlock(unsigned int* channels, unsigned int* zlocs, unsigned int* ylocs, unsigned int* xlocs, unsigned int* tlocs, int startP, int endP);
	void SwitchMemoryBank(int *eraseChannels);
	void clearMemoryBank(int* clearChannels);
	void deleteMemoryBank();

private:
	bool _memory_created;
	void createMemoryBank(int* createChannels);
	bool IfAllChannels(int* bool1);
	int* AllChannelBool(bool bool1);

};

