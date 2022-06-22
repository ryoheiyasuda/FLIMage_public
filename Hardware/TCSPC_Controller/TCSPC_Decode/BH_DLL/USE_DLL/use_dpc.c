/*--------------------------------------------------------
   USE_DPC.C -- Simple example of using SPC DLL library spcm32.dll
                 for DPC-230 module
                 (c) Becker &Hickl GmbH, 2008
  --------------------------------------------------------*/

#include <windows.h>
#ifdef _CVI_
#include <utility.h> // only in LabWindows environment (Timer() function)
#endif
#include <time.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#include "spcm_def.h"

#define MAX_PART  2

int   mod_active[MAX_NO_OF_SPC], total_no_of_dpc, no_of_active_dpc;

short     dpc_error, force_use, act_mod, work_mode, dpc_state, 
          image_mode, relative_mode, tcspc_mode, sync_state, scan_state,
          stopped_by_time, init_status, module_type, tdc_active[2], armed, empty,
          fifo_type, stream_hndl, stream_type, what_to_read, stopped_by_time,
          tdc1_stream_hndl, tdc2_stream_hndl, tdc1_stream_type, tdc2_stream_type,
          buf_stream_hndl[2];
unsigned short  *buffer[2], *ptr, fpga_version, 
                arm_mask, empty_mask, ovfl_mask;
unsigned long  photons_to_read, words_to_read, fifo_size, current_cnt, buf_size[2],
               max_ph_to_read, max_words_in_buf, words_left[2], words_in_buf[2],
               phot_in_buf[2], photon_left[2], phot_cnt, strbuf_size, mt_clock;
unsigned int spc_header[2];
float  usage_degree[2], new_time, old_time, disp_time;

SPCModInfo  mod_info[MAX_NO_OF_SPC];
SPCdata   dpc_dat;
PhotStreamInfo   stream_info, buf_stream_info[2];
PhotInfo         phot_info;
PhotInfo64       phot_info64, *phot_buffer[2], *phot_ptr;
  // to be used in SPCM software 'Convert FIFO File' file names should contain 
  //   "_1-8" or "_9-16"  - this gives information from which TDC data were read
char             phot_fname[2][256] = {"tdc1_raw_data_1-8.spc", "tdc2_raw_data_9-16.spc"},
                 phot_ph_fname[2][256] = {"tdc1_ph_data_1-8.ph", "tdc2_ph_data_9-16.ph"},
                 dpc_phot_name[256] = "dpc_data.spc";

rate_values rates[MAX_NO_OF_SPC];


short  save_photons_in_file ( short tdc_no );
short  extract_photons ( short fifo_type, short stream_type, short what_to_read, 
                         char *inp_fname, char *out_fname );
short    convert_dpc_raw2spc ( char *inp1_fname, char *inp2_fname, char *out_fname, 
                               short inp1_type, short inp2_type );


#pragma argsused


int main ( int argc, char *argv[] )
{
char  ini_name[256] = "d:\\spcm\\lib\\dpc230.ini";
char  dest_inifile[256] = "dpc_test.ini", *fname1, *fname2;

int   val, status, use_part, extract_phot, cmd_inp_type, use_ini_file;
short i, j, ret, sval;
float fval;

use_part = 0; //  part 0 will be executed
use_ini_file = 0;    // default ( above) .ini file used,  1- ini file from command line
extract_phot = 0;  // do not extract photons from created .spc files
cmd_inp_type = -1;

// assumption is done here that we work with DPC-230 modules
module_type = M_DPC230;

for ( i = 1; i < argc; i++) {
  if ( (*++argv )[0] == '-') {
    switch (( unsigned char ) (*argv )[1] ) {
      case 'f' :  // change ini file name
      case 'F' :
        use_ini_file = 1;
        strcpy ( ini_name, ( char *) &((*argv )[2] ));
        break;

      case 'p' :  // choose part to execute ( -1 means all )
      case 'P' :
        status = sscanf ( (char *)&((*argv)[2]), "%d", &val );
        if ( status == 1 ) {
          if ( val > MAX_PART )
            val = -1;  // all
          use_part = val;  
          }
        break;

      case 'e' :  // do extract photons from .spc files ( default = 0 )
      case 'E' :
        extract_phot = 1;
        break;
      
      case 'i' :  // force input types for convert_dpc_raw2spc
      case 'I' :    // bit 1 = 1 - TDC1 TTL, bit 2 = 1 - TDC2 TTL
        status = sscanf ( (char *)&((*argv)[2]), "%d", &val );
        if ( status == 1 ) {
          cmd_inp_type = val & 6;
          }
        break;
      }
    }
  }
      /* initialization must be done always at the beginning */
if(( ret = SPC_init ( ini_name )) < 0){
  if ( -ret < SPC_WRONG_ID || -ret == SPC_WRONG_LICENSE  || -ret >= SPC_NO_LICENSE)
    return ret;   // fatal error, maybe wrong ini file   or DLL not registered
  }

total_no_of_dpc = no_of_active_dpc= 0;
for( i = 0; i < MAX_NO_OF_SPC; i++){
  SPC_get_module_info ( i, (SPCModInfo *)&mod_info[i]);
  if ( mod_info[i].module_type != M_WRONG_TYPE)
    total_no_of_dpc++;
  if ( mod_info[i].init == INIT_SPC_OK){
    no_of_active_dpc++; mod_active[i] = 1;
    act_mod = i;
    }
    else
      mod_active[i] = 0;
  }

work_mode = SPC_get_mode();

if( !total_no_of_dpc || work_mode == DPC_SIMUL230){
  // no modules found at all or simulation
  // if you want to work in simulation mode there are two ways :
  //    1 - change item simulation in .ini file to required sim. mode
  //    2 - use SPC_set_mode function

            /* DPC230 simulation mode for 1 module */
  mod_active[0] = 1;
  for( i = 1 ; i < MAX_NO_OF_SPC; i++)
    mod_active[i] = 0;
  force_use = 0;
  dpc_error = SPC_set_mode( DPC_SIMUL230, force_use, mod_active);
  work_mode = SPC_get_mode();
  if( work_mode != DPC_SIMUL230)
    return -1;
  //  check modules once more

  total_no_of_dpc = no_of_active_dpc= 0;
  for( i = 0; i < MAX_NO_OF_SPC; i++){
    SPC_get_module_info ( i, (SPCModInfo *)&mod_info[i]);
    if ( mod_info[i].module_type != M_WRONG_TYPE)
      total_no_of_dpc++;
    if ( mod_info[i].init == INIT_SPC_OK){
      no_of_active_dpc++; mod_active[i] = 1;
      act_mod = i;
      }
      else
        mod_active[i] = 0;
    }
  }


if( total_no_of_dpc != no_of_active_dpc){
  // it is possible when some DPC modules:
  //   1 - are already used by other applications
  //        mod_info[i].init == INIT_SPC_MOD_IN_USE, mod_info[i].in_use == -1
  //   2 - didn't pass correctly through the initialisation procedure
  //        (e.g. wrong EEPROM checksum, hardware test failed )
  // it is still possible ( but not recommended) to use such module,
  //   if you call SPC_set_mode  with force_use parameter = 1
  //   e.g. SPC_set_mode(SPC_HARD, 1 , mod_active);

  }

// furthermore you can have in your system SPC modules of different types
//  (it applies only for the PCI bus modules )
//  It is not recommended (can lead to strange results) to start the same 
//    measurement on SPC modules of different types ( measurement modes can
//     have different meaning or can operate in a different way )

work_mode = SPC_get_mode(); 
for ( i = 0; i < MAX_NO_OF_SPC; i++){
  if ( mod_active[i] ){
    if ( mod_info[i].module_type != module_type){
      mod_active[i] = 0;
      no_of_active_dpc--;
      }
    } 
  }

 // this will deactivate unused modules in DLL
SPC_set_mode ( work_mode, 0 , mod_active);  
              
if( !no_of_active_dpc)
  return -1;

for( i = 0; i < MAX_NO_OF_SPC; i++){
  if ( mod_active[i]){
    act_mod = i;
    break;
    }
  } 

init_status = SPC_get_init_status ( act_mod );

fifo_size = 4194304; //  4M of 32-bit words for one TDC
fifo_type = FIFO_D230;


if ( use_ini_file)  // do not change parameters from .ini file
  goto  part0;

ret = SPC_get_parameters ( act_mod, &dpc_dat);
// directly after initialisation parameters in all modules are equal
//  but later if needed you can equalize the settings as shown below
for( i = act_mod + 1; i < MAX_NO_OF_SPC; i++){
  if ( mod_active[i] ){
    SPC_set_parameters ( i, &dpc_dat);
    }
  } 
/////////////////


/* during SPC initialization all hardware parameters are set to values
   taken from ini file ( or to default values )
   This parameter set can be overwritten by using
     SPC_set_parameters or SPC_set_parameter function.
   After setting the parameter  check return value and
   read the parameter again to get the value which was sent to the hardware */

   /* change collection time on all modules */
/*
ret = SPC_set_parameter(-1, COLLECT_TIME, 1.0); 
SPC_get_parameter( act_mod, COLLECT_TIME, &dpc_dat.collect_time);

*/

/* it is also possible to save current module parameters to selected ini file
          or/and restore it from ini file
  
     // save parameters from dpc_dat to dest_inifile using base settings and 
     //  comments from the ini file used in SPC_init call 
     //        ( parameter source_inifile = NULL) 
ret = SPC_save_parameters_to_inifile( &dpc_dat, dest_inifile, NULL, 1 );

       // read module parameters from dest_inifile file to dpc_dat
      //  and then send the settings to the hardware
  
ret = SPC_read_parameters_from_inifile ( &dpc_dat, dest_inifile );
ret = SPC_set_parameters ( act_mod, &dpc_dat);
*/


ret = SPC_save_parameters_to_inifile( &dpc_dat, dest_inifile, NULL, 1 );

     /*  before the measurement */


//  set the parameters


// configure inputs

// you can activate one or two TDCs - use parameter MEM_BANK 
// 

SPC_set_parameter ( act_mod, MEM_BANK, 6 ); // both TDCs active, TDC1 - 2, TDC2 - 4

// choose which inputs will be active CFD or TTL - parameter DETECTOR_TYPE

SPC_set_parameter ( act_mod, DETECTOR_TYPE, 0 ); // both TDCs active inputs CFD
//SPC_set_parameter ( act_mod, DETECTOR_TYPE, 6 ); // both TDCs active inputs TTL

// enable required inputs - parameter CHAN_ENABLE
//  use channels names and numbers as in SPCM application   
    // enable CFD1 & CFD4 ( Channel 1 & 12 )
SPC_set_parameter ( act_mod, CHAN_ENABLE, 0x200100 ); 
  
// set active slopes for enabled inputs - parameter CHAN_SLOPE
    // set slopes to active falling edge for all channels
SPC_set_parameter ( act_mod, CHAN_SLOPE, 0x000000 ); 
    // set slopes to active rising edge for all channels
//SPC_set_parameter ( act_mod, CHAN_SLOPE, 0x3ff3ff ); 

// Warning : active edge of CFD channels is currently fixed to falling edge ( hardware demand )


//  measurement mode - depends on user's experiment - see DPC-230 User Handbook
//  some examples below

//  1. absolute timing  
image_mode = 0; relative_mode = 0; tcspc_mode = 0;
SPC_set_parameter ( act_mod, MODE, FIFO_ABS_TIME );  

//  2. relative timing  multichannel-scaler mode
image_mode = 0; relative_mode = 1; tcspc_mode = 0;
SPC_set_parameter ( act_mod, MODE, FIFO_ABS_TIME );  

//  3. relative timing  TCSPC mode
image_mode = 0; relative_mode = 1; tcspc_mode = 1;
SPC_set_parameter ( act_mod, MODE, FIFO_TCSPC );  

//  4. relative timing  TCSPC imaging mode
image_mode = 1; relative_mode = 1; tcspc_mode = 1;
SPC_set_parameter ( act_mod, MODE, FIFO_TCSPC_IMG );

//  5. relative timing  multichannel-scaler mode
image_mode = 1; relative_mode = 1; tcspc_mode = 0;
SPC_set_parameter ( act_mod, MODE, FIFO_ABS_IMG );  

/* measurements mode remarks  //////////////////////

for relative modes  reference channel must be defined  
     using bits 0-4 of CHAN_SPEC_NO parameter
    reference channel in TCSPC modes has fixed values : 
       it is always in TDC2 and has value ( counting  from 0 )
          11 - when CFD inputs are active
          19 - when TTL inputs are active
    appearance of the reference signal can be detected by using SPC_get_sync_state
    
for image modes  scan clocks channels must be defined  
     using bits 8-17 of CHAN_SPEC_NO parameter
    all scan clocks must be on the same TDC
    appearance of the scan clocks signals can be detected by using SPC_get_scan_clk_state

    the measurement will start with the 1st frame pulse after SPC_start_measurement and 
      after detecting trigger pulse ( if trigger active ).
    after request to stop ( SPC_stop_measurement call or collection time expired )
         the measurement will continue up to the next frame pulse. 
in each measurement mode:
   - you collect photons into all active channels on one or two TDCs,
   - read them out using SPC_read_fifo ( raw format ) and save in a file(s)
   - convert raw data files to DPC .spc format using SPC_convert_dpc_raw_data function
   - finally extract photons information from .spc files

further interpretation of the collected photon stream depends on user's needs
  as in SPCM software,
  you can build different histograms  : intensity (MCS), FIDA, FCS ( Auto or Cross)
  for relative modes you can build decay curves
  for image modes you can build the image

///////////////////////////////////////////////////*/

//  continue with absolute timing mode 
image_mode = 0; relative_mode = 0; tcspc_mode = 0;
SPC_set_parameter ( act_mod, MODE, FIFO_ABS_TIME );  

if ( relative_mode ){
  // set reference channel number
  sval = dpc_dat.chan_spec_no & (~0x1f);  // clear bits 0-4
  if ( tcspc_mode ){  // fixed values on TDC2 for reference channel no
    SPC_get_parameter ( act_mod, DETECTOR_TYPE, &fval );
    if ( (short)fval & 4  )
      sval |= 19;  // TDC2 active inputs - TTL   - Channel 20
      else
        sval |= 11;  // TDC2 active inputs - CFD - Channel 12
    }
    else{
      // for multichannel-scaler reference channel can be on TDC1 or TDC2
      // for example
      sval |= 2;   // 1st TTL channel on TDC1 - Channel 3
      }
  SPC_set_parameter ( act_mod, CHAN_SPEC_NO, (float)sval );
  // test whether reference signal is present
  SPC_get_sync_state ( act_mod, &sync_state );
  }

if ( image_mode ){
  sval = dpc_dat.chan_spec_no & (~0x3ff00);  // clear bits 8-17
  // set scan clocks channels no
  // for example : scan clocks on TDC1, frame - Channel 4, line - Channel 5, pixel - Channel 6
  // frame clock -  2nd TTL channel - bits 8-10 = 1
  sval |= ( 1 << 8 );
  // line clock  -  3rd TTL channel - bits 11-13 = 2
  sval |= ( 2 << 11 );
  // pixel clock -  4th TTL channel - bits 14-16 = 3
  sval |= ( 3 << 14 );
  // finally add bit 17 which tells on which TDC are scan clocks
  // in our case = 0 - TDC1
  sval |= ( 0 << 17 );
  
  SPC_set_parameter ( act_mod, CHAN_SPEC_NO, (float)sval );
  // test whether scan clocks signals are present
  SPC_get_scan_clk_state ( act_mod, &scan_state );
  }

// measurement can be stopped after specified time
SPC_set_parameter ( act_mod, STOP_ON_TIME, 1 ); 
SPC_set_parameter ( act_mod, COLLECT_TIME, 10.0 ); // stop after 10 sec


// to switch on/off reading rates outside the measurement parameter RATE_COUNT_TIME is used
//       ( during the measurement rates are always collected )
SPC_set_parameter ( act_mod, RATE_COUNT_TIME, 0 ); // switch off
SPC_set_parameter ( act_mod, RATE_COUNT_TIME, 1.0); // switch on reading rates

SPC_clear_rates ( act_mod ); // this is required to activate/ deactivate reading rates
                             //   after changing RATE_COUNT_TIME
// for DPC module there are separate rate values for each DPC 
//     which means total photon rate in all active input channels of the TDC                            
ret = SPC_read_rates( act_mod, &rates[act_mod] );



part0:

ret = SPC_get_parameters ( act_mod, &dpc_dat);  // update dpc_dat with current parameters values

tdc_active[0] = ( dpc_dat.mem_bank & 2 ) != 0; 
tdc_active[1] = ( dpc_dat.mem_bank & 4 ) != 0;

switch ( dpc_dat.mode ){
  case FIFO_ABS_TIME:      //   absolute timing  
    image_mode = 0; relative_mode = 0; tcspc_mode = 0;
    break;

  case FIFO_TCSPC:  //   relative timing  TCSPC mode
    image_mode = 0; relative_mode = 1; tcspc_mode = 1;
    break;

  case FIFO_TCSPC_IMG:   //   relative timing  TCSPC imaging mode
    image_mode = 1; relative_mode = 1; tcspc_mode = 1;
    break;
  case FIFO_ABS_IMG:     //   relative timing  multichannel-scaler mode
    image_mode = 1; relative_mode = 1; tcspc_mode = 0;
    break;
  }
  
if ( use_part >= 0 && use_part != 0 ) 
  goto part1;        // do not execute part 0
  
////////////////////////////////////////////////////////////////

// part 0 -  example of a absolute timing  measurement 
//  In the ‘Absolute Time’ mode every photon is characterised by 
//   its time from the start of the measurement and its input channel number. 
//  The interpretation of the data is merely a matter of the user software.
  
////////////////////////////////////////////////////////////////

  // there is no need ( and also no way ) to clear FIFO memory before measurement

  // DPC memory in FIFO mode is a big buffer which is filled with photons.
  // From the user point of view it works like a fifo - 
  //     you can read photon frames untill the fifo is empty 
  //     ( or you reach required number of photons ). 
  //  If your photon's rate is too high or you don't read photons fast enough, 
  //    FIFO overrun can happen, 
  //  it means that photons which were not read before are overwritten with the new ones.
  //  - macro time information after the overrun is not consistent
  // The photon's rate border at which overruns can appear depends on:
  //   - your experiment ( photon's rate ), 
  //   - your computer's speed, hard disk, disk cache, operating memory size
  //   - number of tasks running in the same time
  //  You can experiment using our measurement software to decide 
  //       how big memory buffer to use to read photons and when to write to hard disk
  //  To increase the border :
  //     - close all unnecessary applications
  //     - do not write to the hard disk very big amount of data in one piece - 
  //           it can slow down your measurement loop
 
  // buffer must be allocated for predefined max number of photons to read in one call
  // max number of photons to read in one call - here max_ph_to_read variable
  // max_ph_to_read should also be defined carefully depending on the same aspects as 
  //     overrun considerations above
  // if it is too big - you can block (slow down) your system during reading fifo
  //    ( for high photons rates)
  // if it is too small - you can decrease your photon' rate at which overrun occurs
  //    ( by big overhead for calling DLL function)
  // user can experiment with max_ph_to_read value to get the best performance of your
  //    system
  
  max_ph_to_read = 2000000; // big fifo, fast DMA readout
  max_words_in_buf = 2 * max_ph_to_read;

  // allocate buffers for both TDCs
  if ( tdc_active[0] ){
    buffer[0] = (unsigned short *)realloc( buffer[0], max_words_in_buf * sizeof(unsigned short));
    words_left[0] = words_to_read;
    }
    else{
      buffer[0] = NULL;
      words_left[0] = 0;
      }
  if ( tdc_active[1] ){
    buffer[1] = (unsigned short *)realloc( buffer[1], max_words_in_buf * sizeof(unsigned short));
    words_left[1] = words_to_read;
    }
    else{
      buffer[1] = NULL;
      words_left[1] = 0;
      }
      
  photons_to_read = 10000;
  words_to_read = 2 * photons_to_read;
  new_time = old_time = 0.0;
  disp_time = 1.0;
      
  words_in_buf[0] = words_in_buf[1] = 0;  

  // User may want to stop the measurement : 
  //    - after reading required number of photons ( as in this example ), 
  //    - by user break ( by using SPC_stop_measurement )
  //    - for example when Fifo overrun occurs
  //    - after certain measurement time ( when Stop on time is set )   
  
  ret = SPC_start_measurement ( act_mod );  // only avtive TDCs are armed

  while ( !ret ){
    // now test SPC state and read photons
    SPC_test_state ( act_mod, &dpc_state);
      // user must provide safety way out from this loop 
      //    in case when trigger will not occur or required number of photons 
      //          cannot be reached
    if ( dpc_state & SPC_WAIT_TRG){   // wait for trigger                
      continue;
      }
    // state of active TDC must be tested 
    armed = 0; empty = 1;
    for ( i = 0; i < 2; i++ ){
      if ( !tdc_active[i] ) continue;
      if ( words_left[i] <= 0 ) continue;  
      // test only active TDCs
      arm_mask = i ? SPC_ARMED2 : SPC_ARMED1;
      empty_mask = i ? SPC_FEMPTY2 : SPC_FEMPTY1;
      ovfl_mask = i ? SPC_FOVFL2 : SPC_FOVFL1;
      
      if ( dpc_state & arm_mask){   //  TDC armed   
        armed = 1;
        if ( image_mode && ( dpc_state & SPC_MEASURE ) == 0 )   
          // collecting photons not yet started - waiting for the 1st frame pulse
          //    in the channel defined by parameter CHAN_SPEC_NO
          continue;
        }
      if ( dpc_state & empty_mask ) 
        continue;  // TDC Fifo is empty - nothing to read
      empty = 0;
      if ( words_left[i] > max_words_in_buf - words_in_buf[i] ) 
        // limit current_cnt to the free space in buffer
        current_cnt = max_words_in_buf - words_in_buf[i];
        else
          current_cnt = words_left[i];
      ptr = (unsigned short *)&buffer[i][words_in_buf[i]];
      
      // before the call current_cnt contains required number of words to read from fifo
      ret = SPC_read_fifo ( act_mod + i * MAX_NO_OF_SPC, &current_cnt, ptr);
      // after the call current_cnt contains number of words read from fifo  
      
      words_left[i] -= current_cnt;
      if ( words_left[i] <= 0 )  
        continue;   // required no of photons read already
     
      if ( dpc_state & ovfl_mask ){
        // Fifo overrun occured 
        //  - macro time information after the overrun is not consistent
        //    consider to break the measurement and lower photon's rate
        ret = 1;
        break;
        }
      // during the running measurement it is possible to check how occupied is FIFO
      //  by calling SPC_get_fifo_usage function
      //    !!!  separate values for each TDC
      SPC_get_fifo_usage ( act_mod + i * MAX_NO_OF_SPC, &usage_degree[i] ); 
      
      // if required, time from start and current rates can be tested
      SPC_get_time_from_start ( act_mod, &new_time);
      if( new_time - old_time > disp_time){
        SPC_read_rates( act_mod, &rates[act_mod] );
        }
      words_in_buf[i] += current_cnt;
      if ( words_in_buf[i] == max_words_in_buf ){
        // your buffer is full, but photons are still needed 
        // save buffer contents in the file and continue reading photons
        //  use separate files for both TDC
        ret = save_photons_in_file ( i );
        words_in_buf[i] = 0;
        }
      }
    //  test end condition
    if ( words_left[0] <= 0 && words_left[1] <= 0 )
      ret = 1;
      else{
        if ( !armed ){
          if ( empty ) 
            ret = 1;
          stopped_by_time = (dpc_state & SPC_TIME_OVER) != 0;
          }
        }
    }

  // SPC_stop_measurement should be called even if the measurement was stopped after collection time
  //           to set DLL internal variables
  SPC_stop_measurement ( act_mod );
  
  // save rest of the photons 
  if ( words_in_buf[0] > 0 ) 
    save_photons_in_file ( 0 );   
  if ( words_in_buf[1] > 0 ) 
    save_photons_in_file ( 1 );   


//  files created above in the measurement mode contain raw data read from active TDCs 
//  to extract photons information from these files use section below
//    see  spcm_def.h for PhotStreamInfo and  PhotInfo definitions

  if ( extract_phot ){
    what_to_read = 1;   // valid photons
    for ( i = 0; i < 2; i++ ){
      if ( !tdc_active[i] ) continue;

      stream_type = BH_STREAM | DPC_STREAM | RAW_STREAM;
    
      // adding DPC1(2)_DATA, DPC_TTL flags to stream type gives as a result correct 
      //       extracted photon's channels number as in SPCM software - in the range 1..20
    
      stream_type |= (i ? DPC2_DATA : DPC1_DATA);
      sval = (dpc_dat.detector_type >> (i + 1)) & 1; // type of active inputs 
      if ( sval )
        stream_type |= DPC_TTL;    // TTL inputs active
    
/////////////////////////////////////////////////////////////
// there is new alternative method:
//  call SPC_get_fifo_init_vars function to get:
//      - values needed to init photons stream  
//      -  .spc file header
/////////////////////////////////////////////////////////////

      ret = SPC_get_fifo_init_vars ( act_mod + i * MAX_NO_OF_SPC,  
                                     &fifo_type, &stream_type, NULL, NULL);
      
      extract_photons ( fifo_type, stream_type, what_to_read, phot_fname[i], NULL );
      }
    }
  // photons from both TDCs are extracted in series - at first TDC1 then TDC2
  //   it means you must at the end sort photons from different TDCs 
  //      in order to have correct time scale 
   
  // end of extracting photons from raw data files


//  the section above is not a recommended way to extract photons especially 
//    if you use both TDCs in the measurement
//  this was the only way before implementing SPC_convert_dpc_raw_data function

// end of part 0

part1:

if ( use_part >= 0 && use_part != 1 ) 
  goto part2;        // do not execute part 1

////////////////////////////////////////////////////////////////

// part 1 -  example of extracting photons from raw data files.
  
////////////////////////////////////////////////////////////////

//  The recommended way to extract photons
//
//  1, Convert raw data files created during the measurement into one .spc file
//      Such a file contains sorted ( in time scale) photons from both TDCs in a format 
//        used by SPCM software for DPC .spc files.
//      The format is described in the file SPC_data_file_structure.h
//      The format is easy to understand and does not contain any special entries 
//           like in raw data file.
//    Such .spc file can be used also in SPCM software ( Convert .spc files panel )  
//       to calculate different histograms.
//
//  2. Photons can be extracted from converted .spc file using  SPC_get_photon function  


  fname1 =  tdc_active[0] ? (char *)phot_fname[0] : NULL;
  fname2 =  tdc_active[1] ? (char *)phot_fname[1] : NULL;
  
  // inp_type can be forced by command line parameter -i
  if ( cmd_inp_type >= 0 )
    sval = cmd_inp_type;
    else
      sval = dpc_dat.detector_type;
      
  ret =  convert_dpc_raw2spc ( fname1, fname2, dpc_phot_name, sval & 2, sval  & 4 );
  // succesfully finished conversion when ret = 0
      
      

  // finally you can extract photons and they are now sorted in time for both TDCs

  if ( !ret && extract_phot ){
    what_to_read = 1;   // valid photons
    stream_type = BH_STREAM | DPC_STREAM;     // without raw flag

    extract_photons ( fifo_type, stream_type, what_to_read, dpc_phot_name, NULL );
    }
  
 // end of part 1
 
part2:

if ( use_part >= 0 && use_part != 2 ) 
  goto part3;        // do not execute part 2

////////////////////////////////////////////////////////////////

//  part 2 - example of FIFO measurement using buffered photons stream
//                                                               
  
////////////////////////////////////////////////////////////////

//  new set of functions to control FIFO measurements is available starting 
//      from SPCM DLL v.4.0
// The functions use stream of photons inserted to the buffers ( called as 'buffered stream')
//       instead of stream of .spc files
// This simplifies extracting photons information from the FIFO data even during
//      running measurements, without necessity to create .spc files
// The functions dedicated to use with buffered streams are :
//    SPC_init_buf_stream, SPC_add_data_to_stream, SPC_read_fifo_to_stream, SPC_stream_reset
//    SPC_stream_start(stop)_condition, SPC_get_photons_from_stream, SPC_get_stream_buffer_size,
//    SPC_get_buffer_from_stream

// init actions same as for part 0
// read carefully all comments from part 0

photons_to_read = 15000000;

fval = dpc_dat.tac_enable_hold;     // macro time clock in ps
mt_clock = (unsigned long) (fval * 1e3); // femto
mt_clock &= 0x00ffffff;    // in 1fs units

SPC_set_parameter ( act_mod, COLLECT_TIME, 10.0 ); // stop after 10 sec

// User may want to stop the measurement : 
//    - after reading required number of photons ( as in this example ), 
//    - by user break ( by using SPC_stop_measurement )
//    - for example when Fifo overrun occurs
//    - after certain measurement time ( when Stop on time is set )   

//1.  before starting the measurement 'buffered stream' must be initialized

what_to_read = 1;   // valid photons
for ( i = 0; i < 2; i++ ){
  phot_in_buf[i] = 0;
  
  if ( !tdc_active[i] ){
    phot_buffer[i] = NULL;
    photon_left[i] = 0;
    buf_stream_hndl[i] = -1;
    continue;
    }
  
  // allocate buffers for photons extracted from buffered stream for both TDCs

  phot_buffer[i] = (PhotInfo64 *)calloc( photons_to_read,  sizeof ( PhotInfo64 ));
  photon_left[i] = photons_to_read;  

  stream_type = BH_STREAM | DPC_STREAM | RAW_STREAM;

  // adding DPC1(2)_DATA, DPC_TTL flags to stream type gives as a result correct 
  //       extracted photon's channels number as in SPCM software - in the range 1..20

  stream_type |= (i ? DPC2_DATA : DPC1_DATA);
  sval = (dpc_dat.detector_type >> (i + 1)) & 1; // type of active inputs 
  if ( sval )
    stream_type |= DPC_TTL;    // TTL inputs active

/////////////////////////////////////////////////////////////
// there is new alternative method:
//  call SPC_get_fifo_init_vars function to get:
//      - values needed to init photons stream  
//      -  .spc file header

  ret = SPC_get_fifo_init_vars ( act_mod + i * MAX_NO_OF_SPC,  
                            &fifo_type, &stream_type, (int *)&mt_clock, &spc_header[i]);
/////////////////////////////////////////////////////////////

  
  
  // you can decide when stream buffers will be freed
  // if you add bit FREE_BUF_STREAM the buffer will be freed ,
  //      when, during SPC_get_photons_from_stream call, all photons from the buffer are extracted
  //      after this it will be not possible to use the buffer again, for example to get data from it
  //       ( SPC_get_buffer_from_stream)
  //    FREE_BUF_STREAM option is recommended for long measurements with lots of data read from FIFO
  //       which could make buffers allocated space too big, but saving buffers contents 
  //             after extracting photons will not be possible 

  //stream_type |= FREE_BUF_STREAM;
  
  //   we initialize separate streams for each active TDC
  buf_stream_hndl[i] = SPC_init_buf_stream ( fifo_type, stream_type, what_to_read, mt_clock , 0 );
  if ( buf_stream_hndl[i] < 0 ){
    goto part3;   // error
    }

  
// it is possible to define the moment when extracting photons from the stream will start
//    using SPC_stream_start_condition. Start time can be defined. 
//    After reaching start_time appearance of photons or markers defined in start_OR(AND)_mask is tested

//  for example
//   SPC_stream_start_condition ( buf_stream_hndl[i], 2.0, 0, 0x00004000 );
//   extracting photons will start when:
//                      - at first 2 sec expires
//                      - Channel 14 photons must be found in data stream read from FIFO

//   SPC_stream_start_condition ( buf_stream_hndl[i], 2.0, 0, 0x0000000c );
//               2sec &  both Channel 2 & 3 photons must be found in data stream read from FIFO

//  SPC_stream_start_condition ( buf_stream_hndl[i], 2.0, 0, 0 );

// it is possible to define the moment when extracting photons from the stream will stop 
//    using SPC_stream_stop_condition. Stop time can be defined. 
//    After reaching stop_time appearance of photons or markers defined in stop_OR(AND)_mask is tested

//  SPC_stream_stop_condition ( buf_stream_hndl[i], 8.0, 0, 0 );
  }
 

  // 2. in every moment current stream state can be checked
if ( buf_stream_hndl[0] >= 0)
  SPC_get_phot_stream_info ( buf_stream_hndl[0], &buf_stream_info[0] );
if ( buf_stream_hndl[1] >= 0)
  SPC_get_phot_stream_info ( buf_stream_hndl[1], &buf_stream_info[1] );



  // 3. start the measurement
  
  ret = SPC_start_measurement ( act_mod );  // only active TDCs are armed

  while ( !ret ){
    // now test SPC state and read photons
    SPC_test_state ( act_mod, &dpc_state);
      // user must provide safety way out from this loop 
      //    in case when trigger will not occur or required number of photons 
      //          cannot be reached
    if ( dpc_state & SPC_WAIT_TRG){   // wait for trigger                
      continue;
      }
    // state of active TDC must be tested 
    armed = 0; empty = 1; ret = -1;
    for ( i = 0; i < 2; i++ ){
      if ( !tdc_active[i] ) continue;
      // test only active TDCs
      if ( photon_left[i] <= 0 ) continue; 
      ret = 0;
      arm_mask = i ? SPC_ARMED2 : SPC_ARMED1;
      empty_mask = i ? SPC_FEMPTY2 : SPC_FEMPTY1;
      ovfl_mask = i ? SPC_FOVFL2 : SPC_FOVFL1;
      
      if ( dpc_state & arm_mask){   //  TDC armed   
        armed = 1;
        if ( image_mode && ( dpc_state & SPC_MEASURE ) == 0 )   
          // collecting photons not yet started - waiting for the 1st frame pulse
          //    in the channel defined by parameter CHAN_SPEC_NO
          continue;
        }
      if ( dpc_state & empty_mask ) 
        continue;  // TDC Fifo is empty - nothing to read
      
      if ( armed ){
        empty = 0;
        current_cnt = photon_left[i] * 2;  // 2 = words_per_phot
        phot_cnt = photon_left[i];
        phot_ptr = (PhotInfo64 *)&phot_buffer[i][phot_in_buf[i]];  
        // FIFO contents is read to stream buffers ( on a DLL level) - no need to allocate any external buffer now 
        // before the call current_cnt contains required number of words to read from fifo
        ret = SPC_read_fifo_to_stream ( buf_stream_hndl[i], act_mod + i * MAX_NO_OF_SPC, &current_cnt );
        if ( ret < 0 )
          break;
        // after the call current_cnt contains number of words read from fifo and putted to stream buffers  
        
        // photons can be extracted from the stream directly after reading from FIFO, or in any other moment,
        //    also by using other program's thread
        // if you call SPC_get_photons_from_stream in this loop, be aware of the photons rate
        //    - with high photons rates it can cause fifo overrun
        //  you can delay or interrupt extracting photons by changing start or stop condition 
        //     by calling SPC_stream_start(stop)_condition
      
        // before the call phot_cnt contains required number of photons to get from buffered stream
        ret = SPC_get_photons_from_stream ( buf_stream_hndl[i], phot_ptr, (int *)&phot_cnt );
        // after the call phot_cnt contains number of photons taken from buffered stream
      
        if ( ret == 2 || ret == -SPC_STR_NO_START || ret == -SPC_STR_NO_STOP){  
          // end of the stream or start/stop condition not found yet
          // during running measurement these errors should be ignored
          ret = 0;
          }
      
        //  in every moment current stream state can be tested
        SPC_get_phot_stream_info ( buf_stream_hndl[i], &buf_stream_info[i] );
        
        photon_left[i] -= phot_cnt;
        phot_in_buf[i] += phot_cnt;
      
        if ( ret == 1 ){ // stop condition reached
          photon_left[i] = 0;
          ret = 0;
          }
        
        if ( phot_in_buf[i] >= photons_to_read )  
          photon_left[i] = 0;   // required no of photons read already
      
        if ( photon_left[i] <= 0 )  
          continue;   // required no of photons read already
       
        if ( dpc_state & ovfl_mask ){
          // Fifo overrun occured 
          //  - macro time information after the overrun is not consistent
          //    consider to break the measurement and lower photon's rate
//        ret = 1;
//        break;
          }
        // during the running measurement it is possible to check how occupied is FIFO
        //  by calling SPC_get_fifo_usage function
        //    !!!  separate values for each TDC
        SPC_get_fifo_usage ( act_mod + i * MAX_NO_OF_SPC, &usage_degree[i] ); 
        
        // if required, time from start and current rates can be tested
        SPC_get_time_from_start ( act_mod, &new_time);
        if( new_time - old_time > disp_time){
          SPC_read_rates( act_mod, &rates[act_mod] );
          }
        }  
        else{   // not armed
          if ( ( dpc_state & SPC_TIME_OVER ) != 0 ){
            // measurement stopped after collection time
            // read rest photons from the fifo to the stream
            ret = 0; current_cnt = photon_left[i] * 2; 
            while ( !ret && current_cnt > 0 ){
              current_cnt = photon_left[i] * 2;
              // before the call current_cnt contains required number of words to read from fifo
              ret = SPC_read_fifo_to_stream ( buf_stream_hndl[i], act_mod + i * MAX_NO_OF_SPC, &current_cnt );
              // after the call current_cnt contains number of words read from fifo  
              }
            continue;
            }
          }
    }
    //  test end condition
  if ( photon_left[0] <= 0 && photon_left[1] <= 0 )
    ret = 1;
  if ( !armed ){
    if ( empty ) 
      ret = 1;
    stopped_by_time = (dpc_state & SPC_TIME_OVER) != 0;
    }
  }
  
  // SPC_stop_measurement should be called even if the measurement was stopped after collection time
  //           to set DLL internal variables
SPC_stop_measurement ( act_mod );

// as long as stream buffers are not freed you can call SPC_stream_reset,
//  then  SPC_stream_start(stop)_condition - 
//  in this way you can extract another set of photons using new defined start/stop time and condition

strbuf_size = 0;
buf_size[0] = buf_size[1] = 0;

     // get rest photons from the streams , aditionally save fifo data to .spc files ( optional )
for ( i = 0; i < 2; i++ ){
  // test only active TDCs
  if ( !tdc_active[i] ) continue;
  ret = 0;
  while ( photon_left[i] && !ret){ 
    phot_cnt = photon_left[i];
    phot_ptr = (PhotInfo64 *)&phot_buffer[i][phot_in_buf[i]];  
    // before the call phot_cnt contains required number of photons to get from buffered stream
    ret = SPC_get_photons_from_stream ( buf_stream_hndl[i], phot_ptr, (int *)&phot_cnt );
    // after the call phot_cnt contains number of photons taken from buffered stream
    photon_left[i] -= phot_cnt;
    phot_in_buf[i] += phot_cnt;
    }
  SPC_get_phot_stream_info ( buf_stream_hndl[i], &buf_stream_info[i] );
  
  // alternatively save photons to binary file e.g. with extension .ph
  // such .ph files are accepted by 'Convert FIFO files' in SPCM software
 // first 4 bytes should contain the same header as for .spc files ( use SPC_get_fifo_init_vars to get it)
    //           bits 31,26 - 0 reserved for future use
    //           bits 30-27 - no of routing bits       
    //           bit  25    - markers enabled 
    //           bit  24    - femto flag, 1- femto units of MT clock, 0 - 0.1ns units
    //           bits 23-0  - macro time clock in 0.1ns or 1fs( femto, 10e-15 ) units
    //
  
  FILE *phfile;
  if ( phot_in_buf[i] ){
    phfile = fopen ( phot_ph_fname[i], "wb");
    if ( phfile ){
      // macro time  clock of extracted photons  can be corrected in DLL 
      //    by SPC_init_buf_stream function ( removing ACAM nonlinearity )
      //   use mt_clock from stream info
      unsigned int lval =  spc_header[i] & 0xff000000;  // clear bits 0- 23
      lval &= 0x03ffffff;  // clear bits 26-31
      lval |= (buf_stream_info[i].mt_clock & 0xffffff ); // add macro time clock
      fwrite ( (void *)&lval, sizeof ( int ), 1, phfile ); // write header
      fwrite ( (void *)phot_buffer[i], sizeof ( PhotInfo ), phot_in_buf[i], phfile ); // write all photons to file
      fclose ( phfile);
      }
    }  
  /////////// 
  
  //  data read from FIFO to stream buffers can also be saved to .spc file for future use
  if ( (buf_stream_info[i].stream_type & FREE_BUF_STREAM) == 0 ){
    // only when buffers were not freed after extracting photons
    for ( j = 0; j < buf_stream_info[i].no_of_buf; j++ ){
      SPC_get_stream_buffer_size ( buf_stream_hndl[i], j, &strbuf_size);  // get size
      if ( !strbuf_size ) continue;
      if ( strbuf_size > buf_size[i] ){
        buffer[i] = (unsigned short *)realloc( buffer[i], strbuf_size);
        buf_size[i] = strbuf_size;
        }
      SPC_get_buffer_from_stream ( buf_stream_hndl[i], j, &strbuf_size, (char *)buffer[i], 0);
      words_in_buf[i] = strbuf_size / 2; // used by save_photons_in_file
      save_photons_in_file ( i );   
      }
    }  
  SPC_get_phot_stream_info ( buf_stream_hndl[i], &buf_stream_info[i] );
  // - at the end close the opened stream
  SPC_close_phot_stream ( buf_stream_hndl[i] );
  }
  
  
part3:

if ( buffer[0] )
  free ( buffer[0] );
if ( buffer[1] )
  free ( buffer[1] );
if ( phot_buffer[0] )
  free ( phot_buffer[0] );
if ( phot_buffer[1] )
  free ( phot_buffer[1] );
  
return 0 ;
}







static short first_write[2] = { 1, 1};

// tdc_no = 0..1

short  save_photons_in_file( short tdc_no )
{
long ret;
int i;
unsigned short first_frame[2][2], second_frame[2][2], no_of_fifo_routing_bits;
unsigned long lval;
float fval;
FILE *stream;

if ( first_write[tdc_no] ){
    // in order to be compatible with BH .spc files 1st photon entry in the file
    //   should contain macro time clock & flags 

    //           bit  31  - invalid flag
    //           bits 26-24 flags 26 - raw data file, 
    //                            25 - measured in imaging mode
    //                            24 - femto flag, femto units of MT clock
    //           bits 23-0 macro time clock in 1fs( femto, 10e-15 ) units

  fval = dpc_dat.tac_enable_hold;     // macro time clock in ps
  lval = (unsigned long) (fval * 1e3); // femto
  lval &= 0x00ffffff;    // in 1fs units
  lval = lval | 0xc0000000;
  lval |= 0x01000000;    // femto flag, bit 24
  lval |= 0x04000000;    // raw data flag, bit 26
  if ( image_mode ){
    lval |= 0x02000000;   // measured in imaging mode, bit 25
    }
  first_frame[tdc_no][0] = (unsigned short)lval;
  first_frame[tdc_no][1] = (unsigned short)( lval >> 16);
  
/////////////////////////////////////////////////////////////
// there is new alternative method:
//  call SPC_get_fifo_init_vars function to get:
//      - values needed to init photons stream  
//      -  .spc file header

  ret = SPC_get_fifo_init_vars ( act_mod + tdc_no * MAX_NO_OF_SPC,  NULL, NULL, NULL, &spc_header[tdc_no]);
  first_frame[tdc_no][0] = (unsigned short)spc_header[tdc_no];
  first_frame[tdc_no][1] = (unsigned short)( spc_header[tdc_no] >> 16 );
/////////////////////////////////////////////////////////////

  // second frame of temporary DPC raw photons file contains DPC start01 offset
  //    start01 offset is TDC offset used to calculate real time scale
  second_frame[tdc_no][0] = second_frame[tdc_no][0] = 0;
  if ( !SPC_get_start_offset ( act_mod, tdc_no, &lval ) ){
    second_frame[tdc_no][0] = (unsigned short)lval; 
    second_frame[tdc_no][1] = (unsigned short)(lval >> 16);
    }
  first_write[tdc_no] = 0;
      // write 1st frame to the file
  stream = fopen ( phot_fname[tdc_no], "wb");
  if ( !stream )
    return -1;
  
  fwrite ( (void *)&first_frame[tdc_no][0], 2, 2, stream ); // write 2 words ( 32 bits )
  fwrite ( (void *)&second_frame[tdc_no][0], 2, 2, stream ); // write 2 words ( 32 bits )
  }
  else{
    stream = fopen (phot_fname[tdc_no], "ab");
    if ( !stream )
      return -1;
    fseek ( stream, 0, SEEK_END);     // set file pointer to the end
    }

ret = fwrite ( (void *)buffer[tdc_no], 1, 2 * words_in_buf[tdc_no], stream ); // write photons buffer
fclose ( stream);
if ( ret != 2 * words_in_buf[tdc_no] )
  return -1;

return 0;
}



// extract photons from one file

short    extract_photons ( short fifo_type, short stream_type, short what_to_read, 
                           char *inp_fname, char *out_fname )
{
short ret, ret1;
FILE *out_file;

if ( out_fname ){
  out_file = fopen ( out_fname, "wb");
  if ( !out_file )
    return -1;
  }
  else
    out_file = NULL;
  
  // 1. Stream of photons files must be initialized
stream_hndl = SPC_init_phot_stream ( fifo_type, inp_fname, 1, stream_type, what_to_read);

if ( stream_hndl >= 0 ){
  // 2. in every moment of extracting current stream state can be checked
  SPC_get_phot_stream_info ( stream_hndl, &stream_info );
  
  ret = 0;
  while( !ret ){  // untill error or end of the file 
        // user must provide safety way out from this loop 
        // fill phot_info structure with subsequent photons information
    ret = SPC_get_photon( stream_hndl, &phot_info ); 
        
        // save somewhere subsequent phot_info structures for further usage
    if ( out_file && !ret ){   
      ret1 = fwrite ( (void *)&phot_info, sizeof ( PhotInfo ) , 1, out_file ); // write photons to out file
      if ( ret1 != sizeof ( PhotInfo ) )
        return -1;
      }
    }
  SPC_get_phot_stream_info ( stream_hndl, &stream_info );
      // - at the end close the opened stream
  SPC_close_phot_stream ( stream_hndl );
  }

return 0;           
}





short    convert_dpc_raw2spc ( char *inp1_fname, char *inp2_fname, char *out_fname, 
                               short inp1_type, short inp2_type )
{
short ret, sval;
int   max_per_call;

//  To convert raw data files created during the measurement into one .spc file
//         use SPC_convert_dpc_raw_data function
//   To avoid blocking the computer SPC_convert_dpc_raw_data function takes in one call
//     specified max number of photon frames from the input streams.
//   It must be called in the loop untill it returns 0 = successfully finished, or < 0 = error 


  //  Input streams of photons must be initialized
stream_type = BH_STREAM | DPC_STREAM | RAW_STREAM;

  // adding DPC1(2)_DATA, DPC_TTL flags to stream type gives as a result correct 
  //       extracted photon's channels number as in SPCM software - in the range 1..20
if ( inp1_fname ){
  tdc1_stream_type = stream_type | DPC1_DATA;
  if ( inp1_type )      // type of active inputs for DPC1  0 - CFD,  != 0 - TTL
    tdc1_stream_type |= DPC_TTL;    // DPC1 TTL inputs active
  
  tdc1_stream_hndl = SPC_init_phot_stream ( fifo_type, inp1_fname, 1, 
                                          tdc1_stream_type, what_to_read);
  }
  else
    tdc1_stream_hndl = -1;  // not used
   
if ( inp2_fname ){
  tdc2_stream_type = stream_type | DPC2_DATA;
  if ( inp2_type )      // type of active inputs for DPC2  0 - CFD,  != 0 - TTL
    tdc2_stream_type |= DPC_TTL;    // DPC2 TTL inputs active

  tdc2_stream_hndl = SPC_init_phot_stream ( fifo_type, inp2_fname, 1, 
                                          tdc2_stream_type, what_to_read);
  }
  else
    tdc2_stream_hndl = -1;  // not used

      // max_per_call = maximum number of frames read from each input file in one call 
max_per_call = 200000;
      //            it should not be to big to avoid blocking the PC
      // 1st call with init = 1
ret = SPC_convert_dpc_raw_data ( tdc1_stream_hndl, tdc2_stream_hndl, 1, out_fname, max_per_call );
if ( ret <= 0 )
  goto fexit;
  
while ( ret > 0 ){
      // subsuquent calls with init = 0
  ret = SPC_convert_dpc_raw_data ( tdc1_stream_hndl, tdc2_stream_hndl, 0, out_fname, max_per_call );
      // user must provide safety way out from this loop to avoid blocking the PC
  // ret > 0 = conversion progress in range 1000 .. 1 =  rest to do
  printf ( "\n%.1f %% done", 100. - (float)ret / 10. );
  }
  


fexit:

    // - at the end close the opened streams
if ( tdc1_stream_hndl >= 0 )
  SPC_close_phot_stream ( tdc1_stream_hndl );
if ( tdc2_stream_hndl >= 0 )
  SPC_close_phot_stream ( tdc2_stream_hndl );

return ret;
}
