#pragma once
#include "SPCM_Mod.h"
#include <fstream>

class BHControl
{
public:
	static HINSTANCE hBH;
	static bool instanceLoaded;
	static bool libraryLoaded;
	static BHControl* instance;
	static BHControl* CreateBHControl(char* library);
	static const int TTREADMAX = 2096992;

	static const int BHControlErrorCode = -102;

	char libraryName[MAX_PATH]; //should be full path.
	char DLL_Folder_Path[MAX_PATH];
	char initFilePath[MAX_PATH];

	BHControl(char* library);
	~BHControl();

	int LoadBHLibrary(char* library);
	void unLoadBHLibrary();

	int SPC_init(char *ini_file);
	int SPC_get_init_status(short mod_no);
	int SPC_get_parameters(short mod_no, SPCdata * data);
	int SPC_set_parameters(short mod_no, SPCdata *data);
	int SPC_get_parameter(short mod_no, short par_id, float * value);
	int SPC_set_parameter(short mod_no, short par_id, float value);
	int SPC_configure_memory(short mod_no, short adc_resolution, short no_of_routing_bits, SPCMemConfig  * mem_info);
	int SPC_fill_memory(short mod_no, long block, long page, unsigned short fill_value);
	int SPC_clear_mom_memory(short mod_no);
	int SPC_get_sync_state(short mod_no, short *sync_state);
	int SPC_get_fifo_usage(short mod_no, float *usage_degree);
	int SPC_get_scan_clk_state(short mod_no, short *scan_state);
	int SPC_clear_rates(short mod_no);
	int SPC_read_rates(short mod_no, rate_values *rates);
	int SPC_get_time_from_start(short mod_no, float *time);
	int SPC_test_state(short mod_no, short *state);
	int SPC_start_measurement(short mod_no);
	int SPC_pause_measurement(short mod_no);
	int SPC_restart_measurement(short mod_no);
	int SPC_stop_measurement(short mod_no);
	int SPC_enable_sequencer(short mod_no, short enable);
	int SPC_get_sequencer_state(short mod_no, short *state);
	int SPC_read_fifo(short mod_no, unsigned long * count, unsigned short *data);
	/*- Low leve functions ------------------------------------------------------*/
	int SPC_test_id(short mod_no);
	/*- Miscellaneous -----------------------------------------------------------*/
	int SPC_get_module_info(short mod_no, SPCModInfo * mod_info);
	int SPC_close();
	int SPC_set_mode(short mode, short force_use, int *in_use);
	int SPC_get_mode();
	int SPC_get_error_string(short error_id, char * dest_string, short max_length);


	///////////////Not implemented./////////
	//int SPC_prepare_time_gates(short mod_no, short gates_no, short rchan_no, int *gates, short equal_rchan);
	//int SPC_read_block(short mod_no, long block, long frame, long page, short from, short to, unsigned short *data);
	//int SPC_read_data_block(short mod_no, long block, long page, short reduction_factor, short from, short to, unsigned short *data);
	//int SPC_write_data_block(short mod_no, long block, long page, short from, short to, unsigned short *data);
	//int SPC_read_data_frame(short mod_no, long frame, long page, unsigned short *data);
	//int SPC_read_data_page(short mod_no, long first_page, long last_page, unsigned short *data);
	//int SPC_read_mom_data(short mod_no, short with_counters, short first_chan, short last_chan, unsigned long *data);
	//int SPC_set_page(short mod_no, long page);
	//int SPC_get_break_time(short mod_no, float *time);
	//int SPC_get_actual_coltime(short mod_no, float *time);
	//int SPC_read_gap_time(short mod_no, float *time);

	//int SPC_get_eeprom_data(short mod_no, SPC_EEP_Data *eep_data);
	//int SPC_write_eeprom_data(short mod_no, unsigned short write_enable, SPC_EEP_Data *eep_data);
	//int SPC_get_adjust_parameters(short mod_no, SPC_Adjust_Para * adjpara);
	//int SPC_set_adjust_parameters(short mod_no, SPC_Adjust_Para * adjpara);

	//// for SPC130, SPC6x0, 830, 140, 15x,16x, 131(2), DPC230 modules 

	///*- DPC modules specific functions ------------------------------------------*/
	//int SPC_get_start_offset(short mod_no, short bank, unsigned long * ticks);
	//short DPC_fill_memory(short mod_no, short page, short bank, unsigned long fill_value);
	//short DPC_read_rates(short mod_no, rate_values_dpc *rates);

	///*- Low level functions -----------------------------------------------------*/
	//int SPC_get_version(short mod_no, unsigned short *version); //not in manual
	//int SPC_clear_status_flags(short mod_no, unsigned short flags); //not in manual

	///*- Miscellaneous -----------------------------------------------------------*/
	//int SPC_read_parameters_from_inifile(SPCdata *data, char *inifile);
	//int SPC_save_parameters_to_inifile(SPCdata *data, char * dest_inifile, char *source_inifile, int with_comments);
	//int SPC_save_data_to_sdtfile(short mod_no, unsigned short *data_buf, unsigned long bytes_no, char *sdt_file);
	//int SPC_init_phot_stream(short fifo_type, char *spc_file, short files_to_use, short stream_type, short what_to_read);
	//int SPC_close_phot_stream(short stream_hndl);
	//int SPC_get_phot_stream_info(short stream_hndl, PhotStreamInfo *stream_info);
	//int SPC_get_photon(short stream_hndl, PhotInfo *phot_info);
	//int SPC_get_detector_info(short previous_type, short * det_type, char * fname);
	//int SPC_convert_dpc_raw_data(short tdc1_stream_hndl, short tdc2_stream_hndl, short init, char *spc_file, int max_per_call);

	//// functions for buffered streams of photons
	//int SPC_get_fifo_init_vars(short mod_no, short *fifo_type, short *stream_type, int *mt_clock, unsigned int *spc_header);
	//int SPC_init_buf_stream(short fifo_type, short stream_type, short what_to_read, int mt_clock, unsigned int   start01_offs);
	//int SPC_add_data_to_stream(short stream_hndl, void *buffer, unsigned int   bytes_no);
	//int SPC_read_fifo_to_stream(short stream_hndl, short mod_no, unsigned long * count);
	//int SPC_get_photons_from_stream(short stream_hndl, PhotInfo64 *phot_info, int *phot_no);
	//int SPC_stream_start_condition(short stream_hndl, double start_time, unsigned int   start_OR_mask, unsigned int start_AND_mask);
	//int SPC_stream_stop_condition(short stream_hndl, double stop_time, unsigned int   stop_OR_mask, unsigned int stop_AND_mask);
	//int SPC_stream_reset(short stream_hndl);
	//int SPC_get_stream_buffer_size(short stream_hndl, unsigned short buf_no, unsigned int *buf_size);
	//int SPC_get_buffer_from_stream(short stream_hndl, unsigned short buf_no, unsigned int *buf_size, char *data_buf, short free_buf);

};
