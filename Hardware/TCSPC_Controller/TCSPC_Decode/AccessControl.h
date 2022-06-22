#pragma once
#include <stdio.h>
#include <Windows.h>
#include <Iphlpapi.h>
#include <Assert.h>
#pragma comment(lib, "iphlpapi.lib")

class AccessControl
{
public:
	static bool compIDOK;
	static char mac_addr[13];

	AccessControl();
	~AccessControl();

#pragma pack(push,1)
	struct accessID {
		int compID;
		int FLIMSerial;
	};
#pragma pack(pop)

	static int GetComputerID();
	static int TestSerialID(int serialID);
};

