/**
 * @file Utilities.cpp
 *
 * @brief Collection of static functions that would be useful in general.
 *
 * @author Ryohei Yasuda
 * @date June 20th 2019
 * Copyright - Max Planck Florida Institute for Neuroscience 2019
 *
 */

#include "stdafx.h"
#include "Utilities.h"


string Utilities::TimeStringNow()
{
	//SYSTEMTIME time;
	//GetSystemTime(&time);
	//WORD millis = (time.wSecond * 1000) + time.wMilliseconds;


	auto timeNow = chrono::system_clock::now();
	auto millis = chrono::duration_cast<chrono::milliseconds>(timeNow.time_since_epoch());
	auto start_time = std::chrono::system_clock::to_time_t(timeNow);
	tm buf;
	localtime_s(&buf, &start_time);
	char str1[40];
	strftime(str1, sizeof(str1), "%Y-%m-%dT%H:%M:%S", &buf);

	ostringstream os;
	os << str1 << "." << millis.count() % 1000;

	return os.str();
}

