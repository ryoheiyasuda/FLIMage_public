/**
 * @file BHControl.cpp
 *
 * @brief This class controls Becker and Hicl DLL files.
 * Dinamically calls spcm64.dll with LoadLibrary and GetProcAddress.
 *
 * @author Ryohei Yasuda
 * @date June 20th 2019
 * Copyright Max Planck Florida Institute for Neuroscience 2019
 *
 */

#include "stdafx.h"
#include "BHControl.h"

bool BHControl::instanceLoaded = false;
bool BHControl::libraryLoaded = false;
HINSTANCE BHControl::hBH = NULL;
BHControl* BHControl::instance = NULL;

BHControl* BHControl::CreateBHControl(char* dllName)
{
	if (!instanceLoaded || instance == NULL)
		instance = new BHControl(dllName);

	return instance;
}

BHControl::BHControl(char* dllName)
{
	instance = this;
	instanceLoaded = true;
	strcpy_s(libraryName, MAX_PATH, dllName);

	int errorcode = LoadBHLibrary(libraryName);

	if (errorcode != 0)
	{
		char libName[MAX_PATH];
		char* buf = nullptr;
		size_t sz = 0;
		if (_dupenv_s(&buf, &sz, "ProgramFiles(x86)") == 0 && buf != nullptr)
		{
			string DLLPath = string(buf);
#ifdef _WIN64
			DLLPath += "\\BH\\SPCM\\DLL\\spcm64.dll"; //Hardcode!!
#else
			DLLPath += "\\BH\\SPCM\\DLL\\spcm32.dll"; //Hardcode!!
#endif
			strcpy_s(libName, MAX_PATH, DLLPath.c_str());
			free(buf);

			ifstream f(DLLPath);
			if (f.good())
			{
				LoadBHLibrary(libraryName);
			}
			else
				libraryLoaded = false;
		}
		else
			libraryLoaded = false;
	}

	if (libraryLoaded)
	{
		string libstr = string(libraryName);
		size_t found = libstr.find_last_of("/\\");
		string folder = libstr.substr(0, found) + "\\spcm.ini";
		strcpy_s(initFilePath, MAX_PATH, folder.c_str());
	}
}


BHControl::~BHControl()
{
	unLoadBHLibrary();
	instanceLoaded = false;
}


int BHControl::LoadBHLibrary(char* libraryName)
{
	hBH = LoadLibrary(libraryName);
	libraryLoaded = hBH != NULL;
	return libraryLoaded ? 0 : 1;
}

void BHControl::unLoadBHLibrary()
{
	if (hBH != NULL && libraryLoaded)
		FreeLibrary(hBH);
	libraryLoaded = false;
}

int BHControl::SPC_init(char* ini_file)
{
	if (libraryLoaded)
	{
		spc_init func = (spc_init)GetProcAddress(hBH, "SPC_init");
		return (int)func(ini_file);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_get_init_status(short mod_no)
{
	if (libraryLoaded)
	{
		spc_get_init_status func = (spc_get_init_status)GetProcAddress(hBH, "SPC_get_init_status");
		return (int)func(mod_no);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_get_parameters(short mod_no, SPCdata * data)
{
	if (libraryLoaded)
	{
		spc_get_parameters func = (spc_get_parameters)GetProcAddress(hBH, "SPC_get_parameters");
		return (int)func(mod_no, data);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_set_parameters(short mod_no, SPCdata *data)
{
	if (libraryLoaded)
	{
		spc_set_parameters func = (spc_set_parameters)GetProcAddress(hBH, "SPC_set_parameters");
		return (int)func(mod_no, data);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_get_parameter(short mod_no, short par_id, float * value)
{
	if (libraryLoaded)
	{
		spc_get_parameter func = (spc_get_parameter)GetProcAddress(hBH, "SPC_get_parameter");
		return (int)func(mod_no, par_id, value);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_set_parameter(short mod_no, short par_id, float value)
{
	if (libraryLoaded)
	{
		spc_set_parameter func = (spc_set_parameter)GetProcAddress(hBH, "SPC_set_parameter");
		return (int)func(mod_no, par_id, value);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_configure_memory(short mod_no, short adc_resolution, short no_of_routing_bits, SPCMemConfig  * mem_info)
{
	if (libraryLoaded)
	{
		spc_configure_memory func = (spc_configure_memory)GetProcAddress(hBH, "SPC_configure_memory");
		return (int)func(mod_no, adc_resolution, no_of_routing_bits, mem_info);
	}
	else return BHControlErrorCode;
}


int BHControl::SPC_fill_memory(short mod_no, long block, long page, unsigned short fill_value)
{
	if (libraryLoaded)
	{
		spc_fill_memory func = (spc_fill_memory)GetProcAddress(hBH, "SPC_fill_memory");
		return (int)func(mod_no, block, page, fill_value);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_clear_mom_memory(short mod_no)
{
	if (libraryLoaded)
	{
		spc_clear_mom_memory func = (spc_clear_mom_memory)GetProcAddress(hBH, "SPC_clear_mom_memory");
		return (int)func(mod_no);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_get_sync_state(short mod_no, short *sync_state)
{
	if (libraryLoaded)
	{
		spc_get_sync_state func = (spc_get_sync_state)GetProcAddress(hBH, "SPC_get_sync_state");
		return (int)func(mod_no, sync_state);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_get_fifo_usage(short mod_no, float *usage_degree)
{
	if (libraryLoaded)
	{
		spc_get_fifo_usage func = (spc_get_fifo_usage)GetProcAddress(hBH, "SPC_get_fifo_usage");
		return (int)func(mod_no, usage_degree);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_get_scan_clk_state(short mod_no, short *scan_state)
{
	if (libraryLoaded)
	{
		spc_get_scan_clk_state func = (spc_get_scan_clk_state)GetProcAddress(hBH, "SPC_get_scan_clk_state");
		return (int)func(mod_no, scan_state);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_clear_rates(short mod_no)
{
	if (libraryLoaded)
	{
		spc_clear_rates func = (spc_clear_rates)GetProcAddress(hBH, "SPC_clear_rates");
		return (int)func(mod_no);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_read_rates(short mod_no, rate_values *rates)
{
	if (libraryLoaded)
	{
		spc_read_rates func = (spc_read_rates)GetProcAddress(hBH, "SPC_read_rates");
		return (int)func(mod_no, rates);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_get_time_from_start(short mod_no, float *time)
{
	if (libraryLoaded)
	{
		auto func = (spc_get_time_from_start)GetProcAddress(hBH, "SPC_get_time_from_start");
		return (int)func(mod_no, time);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_test_state(short mod_no, short *state)
{
	if (libraryLoaded)
	{
		auto func = (spc_test_state)GetProcAddress(hBH, "SPC_test_state");
		return (int)func(mod_no, state);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_start_measurement(short mod_no)
{
	if (libraryLoaded)
	{
		auto func = (spc_start_measurement)GetProcAddress(hBH, "SPC_start_measurement");
		return (int)func(mod_no);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_pause_measurement(short mod_no)
{
	if (libraryLoaded)
	{
		auto func = (spc_pause_measurement)GetProcAddress(hBH, "SPC_pause_measurement");
		return (int)func(mod_no);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_restart_measurement(short mod_no)
{
	if (libraryLoaded)
	{
		auto func = (spc_restart_measurement)GetProcAddress(hBH, "SPC_restart_measurement");
		return (int)func(mod_no);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_stop_measurement(short mod_no)
{
	if (libraryLoaded)
	{
		auto func = (spc_stop_measurement)GetProcAddress(hBH, "SPC_stop_measurement");
		return (int)func(mod_no);
	}
	else return BHControlErrorCode;
}


int BHControl::SPC_enable_sequencer(short mod_no, short enable)
{
	if (libraryLoaded)
	{
		auto func = (spc_enable_sequencer)GetProcAddress(hBH, "SPC_enable_sequencer");
		return (int)func(mod_no, enable);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_get_sequencer_state(short mod_no, short *state)
{
	if (libraryLoaded)
	{
		auto func = (spc_get_sequencer_state)GetProcAddress(hBH, "SPC_get_sequencer_state");
		return (int)func(mod_no, state);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_read_fifo(short mod_no, unsigned long * count, unsigned short *data)
{
	if (libraryLoaded)
	{
		auto func = (spc_read_fifo)GetProcAddress(hBH, "SPC_read_fifo");
		return (int)func(mod_no, count, data);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_test_id(short mod_no)
{
	if (libraryLoaded)
	{
		auto func = (spc_test_id)GetProcAddress(hBH, "SPC_test_id");
		return (int)func(mod_no);
	}
	else return BHControlErrorCode;
}


int BHControl::SPC_get_module_info(short mod_no, SPCModInfo* mod_info)
{
	if (libraryLoaded)
	{
		auto func = (spc_get_module_info)GetProcAddress(hBH, "SPC_get_module_info");
		return (int)func(mod_no, mod_info);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_close(void)
{
	if (libraryLoaded)
	{
		auto func = (spc_close)GetProcAddress(hBH, "SPC_close");
		return (int)func();
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_set_mode(short mode, short force_use, int* in_use)
{
	if (libraryLoaded)
	{
		auto func = (spc_set_mode)GetProcAddress(hBH, "SPC_set_mode");
		return (int)func(mode, force_use, in_use);
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_get_mode(void)
{
	if (libraryLoaded)
	{
		auto func = (spc_get_mode)GetProcAddress(hBH, "SPC_get_mode");
		return (int)func();
	}
	else return BHControlErrorCode;
}

int BHControl::SPC_get_error_string(short error_id, char * dest_string, short max_length)
{
	if (libraryLoaded)
	{
		auto func = (spc_get_error_string)GetProcAddress(hBH, "SPC_get_error_string");
		return (int)func(error_id, dest_string, max_length);
	}
	else return BHControlErrorCode;
}
