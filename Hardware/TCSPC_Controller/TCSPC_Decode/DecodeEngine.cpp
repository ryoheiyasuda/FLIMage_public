/**
 * @file DecodeEngine.cpp
 *
 * @brief This class deocdes photon and marker information from TCSPC stream and asign
 * to pixels.
 *
 * @author Ryohei Yasuda
 * @date June 20th 2019
 * Copyright - Max Planck Florida Institute for Neuroscience 2019
 *
 */

#include "stdafx.h"
#include "DecodeEngine.h"

#ifdef _DEBUG
#pragma message("   *****   Compiling Decode Engine in debug mode.   *****")
#endif

DecodeEngine** DecodeEngine::instances = new DecodeEngine * [DecodeEngine::maxNinstances](); //Allocate the memArray. Global variable.
int* DecodeEngine::instanceStatus = new int[DecodeEngine::maxNinstances]();

DecodeEngine* DecodeEngine::CreateDecodeEngine(int ID)
{
	DecodeEngine* de;
	if (maxNinstances > ID)
	{
		if (instanceStatus[ID] == 1 && instances[ID] != NULL)
		{
			de = instances[ID]; //just reuse it.
		}
		else
			de = new DecodeEngine(ID);

		return de;
	}
	else
		return NULL;
}

DecodeEngine::DecodeEngine(int id)
{
	if (id < DecodeEngine::maxNinstances)
	{
		device = id;
		instances[device] = this;
		instanceStatus[device] = 1;
		FLIMbuffer = MemoryManager::CreateMemoryManager(device); //create if it is not created.
	}
}

DecodeEngine::~DecodeEngine()
{
	FLIMbuffer->deleteMemoryBank();
	delete FLIMbuffer; //delete memory buffer.
	instanceStatus[device] = 0;
}

void DecodeEngine::SetParameters(parameters* pm)
{
	resolution = pm->resolution;
	binning = pm->binning;
	acq_modePQ = pm->acq_modePQ;
	acqType = pm->acqType;
	n_average = pm->n_average;

	nLines = pm->nLines;
	nPixels = pm->nPixels;
	nZlocs = pm->nZlocs;
	nFrames = pm->nFrames;
	channelPerDevice = pm->nChannels;
	debug = pm->debug;

	if (channelPerDevice > MAXINPCHAN)
		channelPerDevice = MAXINPCHAN;

	nDtime[0] = pm->nDtime0;
	nDtime[1] = pm->nDtime1;
	nDtime[2] = pm->nDtime2;
	nDtime[3] = pm->nDtime3;

	startPoint = pm->nStartPoint;
	endPoint = pm->nEndPoint;

	acquisition[0] = pm->acquisition0;
	acquisition[1] = pm->acquisition1;
	acquisition[2] = pm->acquisition2;
	acquisition[3] = pm->acquisition3;

	acquireFLIM[0] = pm->acquireFLIM0;
	acquireFLIM[1] = pm->acquireFLIM1;
	acquireFLIM[2] = pm->acquireFLIM2;
	acquireFLIM[3] = pm->acquireFLIM3;

	aveFrame[0] = pm->aveFrame0;
	aveFrame[1] = pm->aveFrame1;
	aveFrame[2] = pm->aveFrame2;
	aveFrame[3] = pm->aveFrame3;

	tagID = pm->TagID;
	lineID = pm->LineID;
	frameID = pm->FrameID;
	skipFirstLines = pm->skipFirstLines;
	pixel_binning = pm->pixel_binning;

	BiDirectionalScanX = pm->BiDirectionalScanX;
	BiDirectionalScanY = pm->BiDirectionalScanY;

	//Tag lens related.
	focusing = pm->focus;
	TagMode = (pm->enableFastZscan != 0) && (pm->nZlocs > 1);
	StripeDuringFocus = (pm->StripeDuringFocus != 0);
	LinesPerStripe = pm->LinesPerStripe;

	fastZ_measureTagParameters = pm->fastZ_measureTagParameters != 0;
	fastZ_FrequencyKHz = pm->fastZ_FrequencyKHz; //KHerz
	fastZ_ZScanPerPixel = pm->fastZ_ZScanPerPixel;
	fastZ_ZScanPerPixel_Bidirecitonal = pm->fastZ_ZScanPerPixel_Bidirecitonal;
	fastZ_XYFillFraction = pm->fastZ_XYFillFraction;
	fastZ_VoxelTimeUs = pm->fastZ_VoxelTimeUs;
	fastZ_ZScanPerLine = pm->fastZ_ZScanPerLine;
	fastZ_nFastZSlices = pm->fastZ_nFastZSlices;
	fastZ_VoxelCount = pm->fastZ_nFastZSlices;
	fastZ_phaseRange[0] = pm->fastZ_phaseRangeStart;
	fastZ_phaseRange[1] = pm->fastZ_phaseRangeEnd;
	fastZ_phaseRangeCount[0] = (unsigned int)pm->fastZ_phaseRangeStart;
	fastZ_phaseRangeCount[1] = (unsigned int)pm->fastZ_phaseRangeEnd;
	fastZ_phase_detection_mode = pm->fastZ_phase_detection_mode != 0;

	fastZ_CountPerFastZCycle = pm->fastZ_CountPerFastZCycle; //Count
	fastZ_CountPerFastZCycleHalf = pm->fastZ_CountPerFastZCycleHalf;
	fastZ_CountPerFastZSlice = pm->fastZ_CountPerFastZSlice;
	fastZ_residual_for_PhaseDetection = pm->fastZ_residual_for_PhaseDetection;

	msPerLine = pm->msPerLine;
	time_per_unit = pm->time_per_unit;
	pixel_time = pm->pixel_time;
	line_time_correction = pm->line_time_correction;

	if (line_time_correction == 0)
		line_time_correction = 1;

	eraseMemory[0] = pm->eraseMemory0;
	eraseMemory[1] = pm->eraseMemory1;
	eraseMemory[2] = pm->eraseMemory2;
	eraseMemory[3] = pm->eraseMemory3;

	time_per_unitC = time_per_unit / line_time_correction;
	pixelCount = (unsigned int)(pixel_time / time_per_unitC);

	imageOffset = (unsigned long)(pm->AcquisitionDelay / 1000.0 / time_per_unitC);
	imageOffset_Rev = (unsigned long)(pm->BiDirectionalDelay / 1000.0 / time_per_unitC);

	// Binning adjustment for pixel time and number of lines.
	if (pixel_binning < 1)
	{
		pixel_binning = 1;
	}

	//
	if (pixel_binning > 1)
	{
		nLines = nLines / pixel_binning;
		nPixels = nPixels / pixel_binning;
		pixelCount = pixelCount * pixel_binning;
		skipFirstLines = skipFirstLines / pixel_binning;

		StripeDuringFocus = false;
	}
	//Binning adjustment done.

#ifdef _DEBUG
	ostringstream os;
	os << "Debug: Setup parameters" << endl;
	os << "Debug Level = " << debug << endl;
	os << "PARAMETERS BEGIN" << endl;

	os << "channelPerDevice = " << channelPerDevice << endl;
	for (int i = 0; i < channelPerDevice; i++)
		os << "eraseMemory[" + to_string(i) + "] = " << eraseMemory[i] << endl;

	for (int i = 0; i < channelPerDevice; i++)
		os << "acquisition[" + to_string(i) + "] = " << acquisition[i] << endl;

	for (int i = 0; i < channelPerDevice; i++)
		os << "acquireFLIM[" + to_string(i) + "] = " << acquireFLIM[i] << endl;

	for (int i = 0; i < channelPerDevice; i++)
		os << "aveFrame[" + to_string(i) + "] = " << aveFrame[i] << endl;

	os << "resolution = " << resolution << endl;
	os << "binning = " << binning << endl;
	os << "acq_modePQ = " << acq_modePQ << endl;

	os << "acqType = " << acqType;
	if (acqType == 2)
		os << " (PQ TH260)";
	else if (acqType == 1)
		os << " (BH SPC150)";
	else if (acqType == 3)
		os << " (PQ MultiHarp)";
	os << endl;

	os << "msPerLine = " << msPerLine << endl;
	os << "nLines = " << nLines << endl;
	os << "nPixels = " << nPixels << endl;
	os << "nZlocs = " << nZlocs << endl;
	os << "nFrames = " << nFrames << endl;

	for (int i = 0; i < channelPerDevice; i++)
		os << "nDtime[" + to_string(i) + "] = " << nDtime[i] << endl;

	os << "start point = " << startPoint << endl;
	os << "end point = " << endPoint << endl;

	os << "n_average = " << n_average << endl;

	os << "BiDirectionalScanX = " << BiDirectionalScanX << endl;
	os << "BiDirectionalScanY = " << BiDirectionalScanY << endl;
	os << "tagID = " << tagID << endl;
	os << "lineID = " << lineID << endl;
	os << "frameID = " << frameID << endl;
	os << "skipFirstLines = " << skipFirstLines << endl;
	os << "pixel binning = " << pixel_binning << endl;

	os << "LinesPerStripe = " << LinesPerStripe << endl;
	os << "line_time_correction = " << line_time_correction << endl;

	os << "tagMode = " << TagMode << endl;
	os << "pixel_time = " << pixel_time << endl;
	os << "pixelCount = " << pixelCount << endl;
	os << "time_per_unitC = " << time_per_unitC << endl;
	os << "imageOffset = " << imageOffset << endl;
	os << "imageOffset_Rev = " << imageOffset_Rev << endl;
	os << "focusing = " << focusing << endl;

	os << "fastZ_FrequencyKHz = " << fastZ_FrequencyKHz << endl;
	os << "fastZ_ZScanPerPixel = " << fastZ_ZScanPerPixel << endl;
	os << "fastZ_ZScanPerPixel_Bidirecitonal = " << fastZ_ZScanPerPixel_Bidirecitonal << endl;
	os << "fastZ_XYFillFraction = " << fastZ_XYFillFraction << endl;
	os << "fastZ_VoxelTimeUs = " << fastZ_VoxelTimeUs << endl;
	os << "fastZ_ZScanPerLine = " << fastZ_ZScanPerLine << endl;
	os << "fastZ_nFastZSlices = " << fastZ_nFastZSlices << endl;
	os << "fastZ_VoxelCount = " << fastZ_VoxelCount << endl;

	os << "fastZ_CountPerFastZCycle = " << fastZ_CountPerFastZCycle << endl;
	os << "fastZ_CountPerFastZCycleHalf = " << fastZ_CountPerFastZCycleHalf << endl;
	os << "fastZ_CountPerFastZSlice = " << fastZ_CountPerFastZSlice << endl;
	os << "fastZ_residual_for_PhaseDetection = " << fastZ_residual_for_PhaseDetection << endl;

	os << "fastZ_phase_detection_mode = " << fastZ_phase_detection_mode << endl;
	os << "fastZ_phaseRangeCount[0] = " << fastZ_phaseRangeCount[0] << endl;
	os << "fastZ_phaseRangeCount[1] = " << fastZ_phaseRangeCount[1] << endl;
	os << "fastZ_phaseRange[0] = " << fastZ_phaseRange[0] << endl;
	os << "fastZ_phaseRange[1] = " << fastZ_phaseRange[1] << endl;

	os << "PARAMETERS END" << endl << endl;
	Notify(os.str());
#endif
}

void DecodeEngine::DebugNotification(string message, int debug_level)
{
	if (debug >= debug_level)
		Notify(message);
}

void DecodeEngine::Initialize(parameters* pm)
{
	SetParameters(pm);

	//Initialize.
	oflcorrection = 0;
	lineCorrection = 0;
	lastSyncEvent = 0;

	frameCounter = 0;
	totalFrameCounter = 0;
	totalFrameCounterOut = 0;
	aveFrameCounter = 0;
	line_correction_average = 0;
	lineCounter = 0;
	d_lineCounter = 0;
	tagCounter = largeValue;
	imageWidthCount = (unsigned long)pixelCount * (unsigned long)(nPixels - 1);

	//Stripe
	line_StripeCounter = 0;
	StripeCounter = 0;

	//Tag
	lastTagTime = 0;
	if (!TagMode)
	{
		nZlocs = 1;
		fastZ_CountPerFastZSlice = pixelCount;
	}
	//else
	//{
	//	if (fastZ_phase_detection_mode)
	//	{
	//		fastZ_CountPerFastZSlice = fastZ_CountPerFastZCycle / nZlocs;
	//		fastZ_residual_for_PhaseDetection = fastZ_CountPerFastZCycle % nZlocs;
	//	}
	//	else
	//	{
	//		fastZ_CountPerFastZSlice = (fastZ_phaseRangeCount[1] - fastZ_phaseRangeCount[0]) / nZlocs;
	//		fastZ_residual_for_PhaseDetection = 0;
	//	}
	//}

	saturated = false;
	first_event = true; //first line event
	frame_event_triggered = false;
	last_event = false;
	waitFirstTagEvent = true;

	if (focusing)
		nFrames = 32768;

	nTotalLines = nFrames * nLines;

#ifdef _DEBUG
	DebugNotification("Debug: DE - Initializing Memory...", 1);
#endif

	if (AllOne(eraseMemory, channelPerDevice))
	{
		FLIMbuffer->InitializeMemory(channelPerDevice, nZlocs, nLines, nPixels, nDtime); //Delete and remake.
	}
	else
	{
		FLIMbuffer->clearMemoryBank(eraseMemory);
	}
	Running = true;
}

bool DecodeEngine::AllOne(int* Arr1, int len)
{
	bool result = true;
	for (int i = 0; i < len; i++)
	{
		result = result && (Arr1[i] != 0);
	}
	return result;
}

void DecodeEngine::DecodeBuffer(unsigned int* Buffer, int nRecords)
{
#ifdef _DEBUG
	DebugNotification("Debug: DE - Decode buffer acqType = " + to_string(acqType) + " acq_modePQ=" + to_string(acq_modePQ), 2);
#endif
	if ((acqType == 2 || acqType == 3) && acq_modePQ == MODE_T3)
	{
#ifdef _DEBUG
		DebugNotification("Debug: DE - T3 Decode buffer started Nrecord = " + to_string(nRecords) + " Buffer[0]=" + to_string(Buffer[0]), 2);
#endif

		for (int i = 0; i < nRecords; i++)
		{
			DecodeT3(Buffer[i]);
			if (!Running)
				return;
		}
	}
	else if ((acqType == 2 || acqType == 3) && acq_modePQ == MODE_T2)
	{
#ifdef _DEBUG
		DebugNotification("Debug: DE - T2 Decode buffer started Nrecord = " + to_string(nRecords) + " Buffer[0]=" + to_string(Buffer[0]), 2);
#endif

		for (int i = 0; i < nRecords; i++)
		{
			DecodeT2(Buffer[i]);
			if (!Running)
				return;
		}
	}
	else if (acqType == 1)
	{
#ifdef _DEBUG
		DebugNotification("Debug: DE - BH Decode buffer started Nrecord = " + to_string(nRecords) + " Buffer[0]=" + to_string(Buffer[0]), 2);
#endif

		for (int i = 0; i < nRecords; i++)
		{
			DecodeBH(Buffer[i]);
		}
	}
	return;
}

void DecodeEngine::DecodeBH(unsigned int buf)
{
	bool mark = ((buf >> 28) & 1) == 1; //28 bit 1;
	bool gap = ((buf >> 29) & 1) == 1; //29 bit 1
	bool ofl = ((buf >> 30) & 1) == 1; //30 bit 1
	bool invalid = ((buf >> 31) & 1) == 1; //31 bit 1
	bool photon = !mark && !invalid;

	unsigned int nOverflow;

	if (!photon && !mark && ofl)
	{
		nOverflow = buf & 268435455; //0-27
	}
	else
	{
		nOverflow = 1;
		nsync = buf & 4095; //0-11 bit 12
		channel = (buf >> 12) & 15; //12-15 bit 4
	}

	if (ofl) //
	{
		if (mark || photon) //photon or mark.
			oflcorrection = oflcorrection + WRAPAROUND_BH;
		else
			oflcorrection = oflcorrection + WRAPAROUND_BH * nOverflow;
	}

	line = mark && (((channel >> lineID) & 1) == 1);

	if (frameID >= 0)
		frame = mark && (((channel >> frameID) & 1) == 1);

	if (TagMode || fastZ_measureTagParameters && !saturated)
	{
		tagMark = (((channel >> tagID) & 1) == 1);
		if (tagMark)
			ProcessTag();
	}

	if (frame)
	{
		frame_event_triggered = true; //triggered
	}

	if (line && !fastZ_measureTagParameters && !saturated)
	{
		ProcessLine();
	}

	if (photon && !first_event && !saturated)
	{
		dtime = (buf >> 16) & 4095; //16-27 bit 12    
		dtime = (unsigned int)((4095 - dtime) * nDtime[channel] / 4096);
		ProcessPhoton();
	}

	if (gap)
	{
		saturated = true;
		Notify("Saturated");
	}

#ifdef _DEBUG
	if ((debug == 3 && photon) || (debug == 4) || (debug == 5 && line))
	{
		ostringstream os;
		os << "Debug: DE - nsync=" << nsync << ", ch=" << channel << ", dtime="
			<< dtime << ", mark=" << mark << ", line = " << lineCounter << " (line ID = " << lineID << ")";
		os << " ofl=" << oflcorrection << " lineCorr=" << lineCorrection;
		if (TagMode)
			os << "Tag = " << tagCounter << endl;
		if (ofl)
			os << " (Ofl)";
		else if (line)
			os << " (line) ";
		if (photon)
			os << " (photon) ";
		Notify(os.str());
	}
#endif
}

void DecodeEngine::DecodeT2(unsigned int buf)
{
	nsync = buf & 33554431; //0 - 24 bit. timetag.
	channel = (buf >> 25) & 63;  // 25 - 30 bit.
	bool special = (buf >> 31) == 1; //31 bit.

	if (special)
	{
		if (channel == 0x3F) //an overflow record
		{
			//number of overflows is stored in timetag
			if (nsync == 0) //if it is zero it is an old style single overflow
			{
				oflcorrection += T2WRAPAROUND;  //should never happen with new Firmware!
			}
			else
			{
				oflcorrection += T2WRAPAROUND * nsync;
			}
		}

		if ((channel >= 1) && (channel <= 15)) //markers
		{
			truensync = oflcorrection + nsync;
			channel = channel;

			line = ((channel >> lineID) & 1) == 1;

			if (frameID >= 0)
				frame = ((channel >> frameID) & 1) == 1;

			if (TagMode || fastZ_measureTagParameters)
			{
				tagMark = (((channel >> tagID) & 1) == 1);
				if (tagMark)
					ProcessTag();
			}

			if (frame)
			{
				frame_event_triggered = true;
			}

			if (line && !fastZ_measureTagParameters)
			{
				ProcessLine();
			}
		}

		if (channel == 0) //sync event.
		{
			//truensync = oflcorrection + T2Rec.bits.timetag;
			if (!first_event)
				lastSyncEvent = oflcorrection + nsync - lineCorrection;
		}
	}
	else //regular input channel
	{
		truensync = oflcorrection + nsync;
		channel = channel;

		if (!first_event)
			ProcessPhoton();
	}
}

void DecodeEngine::DecodeT3(unsigned int buf)
{
	nsync = buf & 1023; //0-9 bit
	dtime = (buf >> 10) & 32767; //10-24 bit. 
	channel = (buf >> 25) & 63; //25 - 30 bit.
	bool special = (buf >> 31) == 1; //31 bit
	bool ofl = special && channel == 0x3F;
	bool mark = special && (channel >= 1) && (channel <= 15);
	bool photon = !special;

	if (ofl) //overflow
	{
		//number of overflows is stored in nsync
		if (nsync == 0 || Version_PQ == 1)
			oflcorrection += T3WRAPAROUND;
		else
			oflcorrection += T3WRAPAROUND * nsync;
	}
	else if (mark) //markers
	{
		truensync = oflcorrection + nsync;
		line = (((channel >> lineID) & 1) == 1);

		if (frameID >= 0)
			frame = ((channel >> frameID) & 1) == 1;

		if (TagMode || fastZ_measureTagParameters)
		{
			tagMark = (((channel >> tagID) & 1) == 1);
			if (tagMark)
				ProcessTag();
		}

		if (frame)
		{
			frame_event_triggered = true; //triggered
		}

		if (line && !fastZ_measureTagParameters)
		{
			ProcessLine();
		}
	}
	else if (photon) {
		truensync = oflcorrection + nsync;
		if (!first_event)
			ProcessPhoton();
	}

#ifdef _DEBUG
	if ((debug == 3 && photon) || (debug == 4) || (debug == 5 && line))
	{
		ostringstream os;
		os << "Debug: DE - nsync=" << nsync << ", ch=" << channel << ", dtime=" << dtime
			<< ", sp=" << special << ", line = " << lineCounter << " (line ID = " << lineID << ")";
		os << " ofl=" << oflcorrection << " lineCorr=" << lineCorrection;
		if (TagMode)
			os << "Tag = " << tagCounter << endl;
		if (ofl)
			os << " (Ofl)";
		else if (line && special)
			os << " (line) ";
		if (!special)
			os << " (photon) ";
		Notify(os.str());
	}
#endif
}

void DecodeEngine::ProcessTag()
{
	unsigned long syncFromLine = oflcorrection + nsync - lineCorrection; //Time from line.

#ifdef _DEBUG
	DebugNotification("Debug: ProcessTag, syncFromLine = " + to_string(syncFromLine), 3);
#endif

	if (waitFirstTagEvent)
	{
		if (fastZ_measureTagParameters)
		{
			tagCounter = 0;
			waitFirstTagEvent = false;
			tagTimeStart = syncFromLine;
		}
		else if (BiDirectionalScanX == 1 && !first_event)
		{
			unsigned int lineCounter1 = lineCounter;

			if (pixel_binning > 1)
				lineCounter1 = (unsigned int)(d_lineCounter * (double)pixel_binning);

			bool even_condition = (lineCounter1 & 1) == 0 && syncFromLine > imageOffset;
			bool odd_condition = (lineCounter1 & 1) == 1 && syncFromLine > imageOffset_Rev;
			if (even_condition || odd_condition)
			{
				tagCounter = 0;
				waitFirstTagEvent = false;
				tagTimeStart = syncFromLine;
			}
		}
		else if (!first_event)
		{
			//Start after condition meet.
			if (syncFromLine > imageOffset) //First Z-cycle after image offset. 
			{
				tagCounter = 0;
				waitFirstTagEvent = false;
				tagTimeStart = syncFromLine;
			}
		}
	}
	else
	{
		tagCounter += 2; //Measure time from tagMark. BiDirectional. 
	}

	if (TagMode)
	{
		lastTagTime = syncFromLine;
	}

	if (fastZ_measureTagParameters)
	{
		tagTime = syncFromLine;
	}

}

double DecodeEngine::calcKHz() //should be called after running software for a while.
{
	return (double)tagCounter / 2.0 / (tagTime - tagTimeStart) / time_per_unitC / 1000.0;
}

void DecodeEngine::ProcessPhoton()
{
	PhotonCounter++;

	unsigned long syncFromLine = oflcorrection + nsync - lineCorrection;
	if (syncFromLine >= imageOffset && channel < channelPerDevice && dtime >= startPoint && dtime < endPoint) //Assuming imageOffset < imageOffsetRev. It is true in general.
	{
		dtime = dtime - startPoint;

		if (acquireFLIM[channel] == 0)
			dtime = 0;

		if (acquisition[channel] != 0 && dtime < nDtime[channel])
		{
			//This information is required only for photons.
			yLoc = lineCounter;
			//
			//Transfer data from i --> PhotonCounter. (i > photoncounter). Need to do this carefully.
			if (acq_modePQ == MODE_T2)
				dtime = (syncFromLine - lastSyncEvent) / (unsigned long)binning;

			//                        
			if (!TagMode)
			{
				if (BiDirectionalScanX == 1)
				{
					unsigned int lineCounter1 = lineCounter;

					if (pixel_binning > 1)
						lineCounter1 = (unsigned int)(d_lineCounter * (double)pixel_binning);

					if ((lineCounter1 & 1) == 0) //even-
						pixelLoc = syncFromLine - imageOffset; //imageOffset = 0 for now.
					else
						pixelLoc = imageWidthCount - (syncFromLine - imageOffset_Rev);
					//(parameters.nPixels - 1) * pixelCount - (syncFromLine[i] - imageOffset_Rev);
				}
				else if (BiDirectionalScanX == 2)
				{
					if (syncFromLine < imageOffset_Rev)
						pixelLoc = syncFromLine - imageOffset;
					else
					{
						pixelLoc = imageWidthCount - (syncFromLine - imageOffset_Rev);
						yLoc = yLoc + 1;
					}
				}
				else
					pixelLoc = syncFromLine - imageOffset;

				if (pixelCount != 0)
					xLoc = pixelLoc / pixelCount;
				else
					Notify("Error: DE -pixelLoc = " + to_string(pixelLoc) + "pixelCount = " + to_string(pixelCount));

				zLoc = 0;

				yLoc -= skipFirstLines; //New function for version 2.0.21
				if (yLoc < 0)
					yLoc = 0;

#ifdef _DEBUG
				if (debug >= 3)
				{
					ostringstream os;
					os << "Debug: DE - AddToPixel: ch=" << channel << " Z=" << zLoc << " Y=" << yLoc << " X=" << xLoc << " t=" << dtime;
					os << " pixelLoc=" << pixelLoc << "/" << pixelCount;
					Notify(os.str());
				}
#endif
				auto yLoc1 = yLoc;
				if (BiDirectionalScanY == 1 && totalFrameCounter % 2 == 1)
					yLoc1 = nLines - yLoc - 1;
				FLIMbuffer->AddToPixel(channel, zLoc, yLoc1, xLoc, dtime);
			}
			else if (!waitFirstTagEvent)
			{
				unsigned long timeCount = syncFromLine - lastTagTime;

				unsigned int addBDTagCount = 0; //For bidrectional count.

				bool validPhotonEvent = false;

				if (fastZ_phase_detection_mode)
				{
					if (timeCount >= fastZ_CountPerFastZCycleHalf)
						timeCount = timeCount + fastZ_residual_for_PhaseDetection; //Adjust residual.

					if (timeCount < fastZ_CountPerFastZCycle)
						validPhotonEvent = true;
				}
				else
				{
					if (timeCount >= fastZ_CountPerFastZCycleHalf)
					{
						timeCount = fastZ_CountPerFastZCycle - timeCount;
						addBDTagCount = 1;
					}

					if (timeCount >= fastZ_phaseRangeCount[0] && timeCount < fastZ_phaseRangeCount[1])
					{
						timeCount -= fastZ_phaseRangeCount[0];
						validPhotonEvent = true;
					}
				}

				if (validPhotonEvent) //only if it is valid photon event.
				{
					zLoc = timeCount / fastZ_CountPerFastZSlice;
					xLoc = (tagCounter + addBDTagCount) / fastZ_ZScanPerPixel_Bidirecitonal;

#ifdef _DEBUG
					if (debug >= 3)
					{
						ostringstream os;
						os << "Debug: DE - AddToPixel: ch=" << channel << " Z=" << zLoc << " Y=" << yLoc << " X=" << xLoc << " t=" << dtime;
						Notify(os.str());
					}
#endif

					FLIMbuffer->AddToPixel(channel, zLoc, yLoc, xLoc, dtime);
				}
#ifdef _DEBUG
				else
				{
					if (debug >= 3)
					{
						ostringstream os;
						os << "Debug: DE - ProcessPhoton invalid photon: ch=" << channel << " timeCount=" << timeCount <<
							" fastZ_phaseRangeCount[0]=" << fastZ_phaseRangeCount[0] << " fastZ_phaseRangeCount[1]=" << fastZ_phaseRangeCount[1] <<
							" fastZ_CountPerFastZSlice=" << fastZ_CountPerFastZSlice;
						Notify(os.str());
					}
				}
#endif
			}
		}
	}
}

void DecodeEngine::ProcessLine()
{
	unsigned long lineTime = oflcorrection;
	unsigned long oldlineCorrection = lineCorrection;

	lineCorrection = nsync;
	oflcorrection = 0;

	tagCounter = largeValue;
	waitFirstTagEvent = true;

	if (first_event)
	{
		lineCounter = 0;
		d_lineCounter = 0;
		if (frameID >= 0)
		{
			if (frame_event_triggered)
			{
				first_event = false;
				frame_event_triggered = false;
			}
		}
		else
			first_event = false; //Triggered!!

		lastTagTime = 0;
		line_correction_average = 0;
		PhotonCounter = 0;

#ifdef _DEBUG
		DebugNotification("Debug: DE- Start event detected", 1);
#endif
	}
	else
	{
		line_correction_average += (double)(lineTime + lineCorrection) - (double)oldlineCorrection;


		//Count twice for bidirectional scanning with line trigger.
		unsigned int mult_factor = 1;

		if (BiDirectionalScanX == 2)
			mult_factor = 2;

		if (pixel_binning > 1)
		{
			//When pixel binning is used, double counter is required.
			d_lineCounter += 1.0 * (double)mult_factor / (double)pixel_binning;
			lineCounter = (unsigned int)d_lineCounter;

			if (d_lineCounter == (double)lineCounter)
				line_StripeCounter += mult_factor;
		}
		else
		{
			lineCounter += mult_factor;
			line_StripeCounter += mult_factor;
			d_lineCounter = (double)lineCounter;
		}


		if (frameID >= 0)
		{
			if (frame_event_triggered && lineCounter < nLines && lineCounter > nLines / 4) //Premature frame event.
			{
				lineCounter = nLines;
				d_lineCounter = (double)lineCounter;
			}

			if (!frame_event_triggered && lineCounter >= nLines)
			{
				lineCounter = nLines - 1;
				d_lineCounter = (double)lineCounter;
			}

			frame_event_triggered = false;
		}

		if (line_StripeCounter == LinesPerStripe || lineCounter == nLines || last_event)
		{
			line_StripeCounter = 0;
			StripeCounter++;

			//Adding strpie events.
			////////////////////////
			if (focusing && StripeDuringFocus)
				DoStripeEvent(false, false);
			/////////////////////

			if (lineCounter == nLines) //Frame is done!
			{
				double aveLineTime = line_correction_average * time_per_unit * 1000.0 / (d_lineCounter * (double)pixel_binning);
				measured_line_time_correction = aveLineTime / msPerLine;

				line_correction_average = 0;
				StripeCounter = 0;
				d_lineCounter = 0;
				lineCounter = 0;

				totalFrameCounter++;
				aveFrameCounter++;

				//if (!(focusing && StripeDuringFocus))
				//	DoStripeEvent(false, false);

				if (frameCounter <= nFrames)
				{
					if (aveFrameCounter == n_average)
					{
						aveFrameCounter = 0;
						frameCounter++;

						if (totalFrameCounter != nFrames) //For Slice event.
							DoStripeEvent(true, true);
						else
							DoStripeEvent(true, false);
					}
					else
						DoStripeEvent(true, false);
				}
			}
		}

		if (lineCounter >= nTotalLines) //Seems to be not necessary?
		{
			last_event = true;
			return;
		}
	}
}

void DecodeEngine::DoStripeEvent(bool frameEvent, bool erase)
{
	if (StripeDuringFocus && focusing)
	{
		int StartLine = (line_StripeCounter - 1) * LinesPerStripe; ///NEEED REVISION!!!!
		int EndLine = line_StripeCounter * LinesPerStripe;
		if (StartLine < 0)
			StartLine = 0;
		if (EndLine > nLines)
			EndLine = nLines;

		Notify("StripeDone, " + to_string(StartLine) + ", " + to_string(EndLine));
	}

	if (frameEvent)
	{
		totalFrameCounterOut++;
		for (int i = 0; i < MAXINPCHAN; i++)
		{
			if (aveFrame[i] == 1 && !erase)
				eraseMemory_acq[i] = 0;
			else
				eraseMemory_acq[i] = 1;
		}

#ifdef _DEBUG
		DebugNotification("Debug: DoStripeEvent. aveFrame = " + to_string(aveFrame[0]) + ", " + to_string(aveFrame[1]), 1);
		DebugNotification("Debug: Erase = " + to_string(erase), 1);
		DebugNotification("Debug: EraseMemory_acq = " + to_string(eraseMemory_acq[0]) + ", " + to_string(eraseMemory_acq[1]), 1);
		DebugNotification("Debug: Total Frame Counter Out = " + to_string(totalFrameCounterOut), 1);
		DebugNotification("Debug: Total Frame Counter = " + to_string(totalFrameCounter), 1);
#endif
		if (totalFrameCounterOut <= nFrames)
		{
			Notify("FrameDone"); //This evokes an event.
			FLIMbuffer->SwitchMemoryBank(eraseMemory_acq);
		}
		if (last_event)
			Running = false;
	} //FrameEvent.
}

int DecodeEngine::GetData(unsigned short* Dest, int channel, int z_loc)
{
	MemoryManager* mm = FLIMbuffer;
	if (channel < mm->nChannels && z_loc < mm->nZlocs)
	{
		memcpy(Dest, mm->FLIMData_Measure[channel][z_loc], mm->nPixels * mm->nLines * mm->nDtime[channel] * sizeof(unsigned short));
		return 0;
	}
	else
		return -10;
}

int DecodeEngine::GetDataLine(unsigned short* Dest, int channel, int z_loc, int startLine, int endLine)
{
	MemoryManager* mm = FLIMbuffer;
	if (channel < mm->nChannels && z_loc < mm->nZlocs)
	{
		int loc = startLine * (mm->nPixels * mm->nDtime[channel]);
		int length = (endLine - startLine) * (mm->nPixels * mm->nDtime[channel]) * sizeof(unsigned short);

		memcpy(Dest + loc, mm->FLIMData_Measure[channel][z_loc] + loc, length);
		return 0;
	}
	else
		return -10;

}

