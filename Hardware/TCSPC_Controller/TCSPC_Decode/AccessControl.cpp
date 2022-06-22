/**
 * @file AccessControl.cpp
 *
 * @brief This class provides a key for the entire DLL.
 * 
 * The program calculates computer ID, based on mac address, and 
 * see if computer ID + 1192 equals to provided serial ID.
 * The value is put to static value compIDOK.
 *
 * For the acquisition of mac address, the code from the following site was used.
 * https://stackoverflow.com/questions/13646621/how-to-get-mac-address-in-windows-with-c
 *
 * @author Ryohei Yasuda
 * @date March 8th 2019
 * Copyright - Max Planck Florida Institute for Neuroscience 2019
 *
 */

#include "stdafx.h"
#include "AccessControl.h"

AccessControl::AccessControl()
{
}

AccessControl::~AccessControl()
{
}

bool AccessControl::compIDOK = false;
char AccessControl::mac_addr[13] = { '\0' };

// Return computer ID. See if serial ID is good and put value into compIDOK.
int AccessControl::TestSerialID(int serialID)
{
	int comID = GetComputerID();
#ifdef _DEBUG
	//compIDOK = true;
	compIDOK = (int)pow(2, 25) % comID + 1192 == serialID;
#else
	compIDOK = (int)pow(2, 25) % comID + 1192 == serialID;
#endif
	return comID;
}


// Return computer ID.
int AccessControl::GetComputerID()
{
	PIP_ADAPTER_INFO AdapterInfo;
	DWORD dwBufLen = sizeof(IP_ADAPTER_INFO);

	AdapterInfo = (IP_ADAPTER_INFO *)malloc(sizeof(IP_ADAPTER_INFO));
	if (AdapterInfo == NULL) {
		//printf("Error allocating memory needed to call GetAdaptersinfo\n");
		return 0; // it is safe to call free(NULL)
	}

	// Make an initial call to GetAdaptersInfo to get the necessary size into the dwBufLen variable
	if (GetAdaptersInfo(AdapterInfo, &dwBufLen) == ERROR_BUFFER_OVERFLOW) {
		free(AdapterInfo);
		AdapterInfo = (IP_ADAPTER_INFO *)malloc(dwBufLen);
		if (AdapterInfo == NULL) {
			//printf("Error allocating memory needed to call GetAdaptersinfo\n");
			return 0;
		}
	}

	if (GetAdaptersInfo(AdapterInfo, &dwBufLen) == NO_ERROR) {
		// Contains pointer to current adapter info
		PIP_ADAPTER_INFO pAdapterInfo = AdapterInfo;
		do {
			// technically should look at pAdapterInfo->AddressLength
			//   and not assume it is 6.
			sprintf_s(mac_addr, sizeof(mac_addr), "%02X%02X%02X%02X%02X%02X",
				pAdapterInfo->Address[0], pAdapterInfo->Address[1],
				pAdapterInfo->Address[2], pAdapterInfo->Address[3],
				pAdapterInfo->Address[4], pAdapterInfo->Address[5]);
			//printf("Address: %s, mac: %s\n", pAdapterInfo->IpAddressList.IpAddress.String, mac_addr);
			// print them all, return the last one.
			// return mac_addr;

			//printf("\n");
			pAdapterInfo = pAdapterInfo->Next;
		} while (pAdapterInfo);
	}
	free(AdapterInfo);

	int sum = 0;
	for (int i = 0; i < 12; i++)
		sum += i * (int)mac_addr[i];

	return sum; // caller must free.
}

