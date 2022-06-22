#include "stdafx.h"
#include "MultiClampTelegraph.h"

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}



///////////////
//Should do Init --> BroadCast --> ampliferLength --> get all gain --> shutdown (free memory)
//////////////
DllExport_int MC_init()
{
	MCT_init(); //Create global variable etc.
	return 0;
}

DllExport_int MC_broadCast()
{
	MCT_broadcast();
	return 0;
}

DllExport_int MC_amplifiersLength()
{
	return get_MCT_amplifiersLength();
}

DllExport_int MC_getGain(int id, MC700Bparam* amp_param)
{
	if (id < get_MCT_amplifiersLength())
	{
		auto MC_amp = get_MCT_State(id);
		if (MC_amp != NULL)
		{
			amp_param->ID = MC_amp->ID;
			amp_param->mode = (int)MC_amp->uOperatingMode;
			amp_param->primary_gain = MC_amp->dAlpha;
			amp_param->scaleFactor = MC_amp->dScaleFactor;
			amp_param->external_cmd_sensitivity = MC_amp->dExtCmdSens;
			amp_param->LPF_cutoff = MC_amp->dLPFCutoff;
			amp_param->second_alpha = MC_amp->dSecondaryAlpha;
			amp_param->second_LPF_cutoff = MC_amp->dSecondaryLPFCutoff;
			return 0;
		}
		else
			return -1;
	}
	else
		return -1;
}

DllExport_int MC_shutdown()
{
	printf("MCT_shutdown() - Starting MCT_shutdown...\n");
	MCT_shutdown();
	return 0;
}
