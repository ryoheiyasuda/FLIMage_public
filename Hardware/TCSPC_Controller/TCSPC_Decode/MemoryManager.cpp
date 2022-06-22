/**
 * @file MemoryManager.cpp
 *
 * @brief This class manages memory for accumulating photon information from TCSPC card.
 * There used to be 3 memory banks for acquisition, display and clearing, but now only 1 memory for simplicity.
 *
 * @author Ryohei Yasuda
 * @date June 20th 2019
 * Copyright - Max Planck Florida Institute for Neuroscience 2019
 *
 */

#include "stdafx.h"
#include "MemoryManager.h"

MemoryManager** MemoryManager::memInstances = new MemoryManager*[MemoryManager::maxNinstances](); //Allocate the memArray. Global variable.
int* MemoryManager::instanceStatus = new int[MemoryManager::maxNinstances]();

MemoryManager* MemoryManager::CreateMemoryManager(int ID)
{
	MemoryManager* mm;
	if (MemoryManager::maxNinstances > ID)
	{
		if (MemoryManager::instanceStatus[ID] == 1 && MemoryManager::memInstances[ID] != NULL)
		{
			MemoryManager::memInstances[ID]->deleteMemoryBank();
			MemoryManager::memInstances[ID]->_memory_created = false;
			mm = MemoryManager::memInstances[ID];
		}
		else
			mm = new MemoryManager(ID);

		return mm;
	}
	else
		return NULL;
}

MemoryManager::MemoryManager(int ID)
{
	_memory_created = false;
	if (MemoryManager::maxNinstances > ID)
	{
		instanceID = ID;
		MemoryManager::memInstances[ID] = this;
		MemoryManager::instanceStatus[ID] = 1;
	}
	else
	{
	}
}

MemoryManager::~MemoryManager() //deconstructor
{
	try
	{
		MemoryManager::instanceStatus[instanceID] = 0;
	}
	catch (int e)
	{
		Notify("Error: MM Meomry Manager closing failed");
	}
}


void MemoryManager::InitializeMemory(int ch_in, int z_in, int y_in, int x_in, unsigned int* dtime_in)
{
	if (_memory_created)
	{
		deleteMemoryBank(); //Avod making a lot of them.
	}

	nChannels = ch_in;
	nZlocs = z_in;
	nLines = y_in;
	nPixels = x_in;
	nDtime = new int[nChannels];
	memcpy(nDtime, dtime_in, nChannels * sizeof(int));
	TagMode = (z_in > 1);

	//Create memory.
	int* eraseChannels = AllChannelBool(true);
	createMemoryBank(eraseChannels);
	_memory_created = true;

	delete[] eraseChannels;

#ifdef _DEBUG
	Notify("Debug: MM - Initializing Memory Done: " + to_string(ch_in) + "," + to_string(z_in) + "," + to_string(y_in) + "," + to_string(x_in));
#endif
}


void MemoryManager::AddToPixel(unsigned int channel, unsigned int zloc, unsigned int yloc, unsigned int xloc, unsigned int tloc)
{
	if (xloc < (unsigned int)nPixels) //&& yloc < nLines && channel < nChannels && )
	{
		if (zloc < (unsigned int)nZlocs)
		{
			FLIMData_Measure[channel][zloc][(yloc * nPixels + xloc) * nDtime[channel] + tloc] ++;
		}
	}
}

void MemoryManager::AddToPixelsBlock(unsigned int* channels, unsigned int* zlocs, unsigned int* ylocs, unsigned int* xlocs, unsigned int* tlocs, int startP, int endP)
{
	if (startP > endP)
	{
		int tmpP = startP;
		startP = endP;
		endP = tmpP;
	}

	for (int i = startP; i < endP; i++)
	{
		AddToPixel(channels[i], zlocs[i], ylocs[i], xlocs[i], tlocs[i]);
	}
}

int* MemoryManager::AllChannelBool(bool value)
{
	int* channelBool = new int[nChannels];
	for (int i = 0; i < nChannels; i++)
		channelBool[i] = value ? 1 : 0;
	return channelBool;
}

bool MemoryManager::IfAllChannels(int* values)
{
	bool result = true;
	for (int i = 0; i < nChannels; i++)
		result = result && (values[i] != 0);
	return result;
}

void MemoryManager::deleteMemoryBank()
{
#ifdef _DEBUG
	Notify("Debug: MM - Deleting memory bank");
#endif

	for (int c = 0; c < nChannels; c++)
	{
		for (int z = 0; z < nZlocs; z++)
		{
			delete[] FLIMData_Measure[c][z];
		}

		delete[] FLIMData_Measure[c];
	}

	delete[] FLIMData_Measure;

#ifdef _DEBUG
	Notify("Debug: MM - Deleting memory bank Done");
#endif
}


void MemoryManager::clearMemoryBank(int* clearChannels)
{
	for (int c = 0; c < nChannels; c++)
		if (clearChannels[c] != 0 && nDtime[c] > 0)
		{
			for (int z = 0; z < nZlocs; z++)
			{
				memset(FLIMData_Measure[c][z], 0, nPixels * nLines * nDtime[c] * sizeof(unsigned short));
			}
		}
}

void MemoryManager::createMemoryBank(int* createChannels)
{
	FLIMData_Measure = new unsigned short**[nChannels]();

	for (int c = 0; c < nChannels; c++)
	{
		if (createChannels[c] != 0)
		{
			FLIMData_Measure[c] = new unsigned short*[nZlocs]();
			for (int z = 0; z < nZlocs; z++)
			{
				if (nDtime[c] > 0)
					FLIMData_Measure[c][z] = new unsigned short[nLines * nPixels * nDtime[c]]();
				else
					FLIMData_Measure[c][z] = NULL; //To make it deletable...
			}
		}
	}
}

void MemoryManager::SwitchMemoryBank(int* eraseChannels)
{
#ifdef _DEBUG
	Notify("Debug: MM - Swiching memory...Erase = [" + to_string(eraseChannels[0]) + ", " + to_string(eraseChannels[1]) + "]");
#endif

	clearMemoryBank(eraseChannels);
}


