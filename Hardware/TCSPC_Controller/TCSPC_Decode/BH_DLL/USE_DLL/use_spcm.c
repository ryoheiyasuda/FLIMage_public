/*--------------------------------------------------------
   USE_SPCM.C -- Simple example of using SPC DLL library spcm32.dll
                 for SPC modules
                 for DPC230 module see use_dpc.c file
                 (c) Becker &Hickl GmbH, 2000-2008
  --------------------------------------------------------*/

#include <windows.h>
#ifdef _CVI_
#include <utility.h> // only in LabWindows environment (Timer() function)
#endif
#include <time.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>

#include "spcm_def.h"

#define MAX_PART  5

int   mod_active[MAX_NO_OF_SPC], total_no_of_spc, no_of_active_spc;
SPCModInfo  mod_info[MAX_NO_OF_SPC];

short spc_error, force_use, act_mod;
SPCdata spc_dat;
SPC_EEP_Data eep;
SPCMemConfig  spc_mem_info;
short work_mode, meas_page, block_length, no_of_routing_bits, words_per_phot;
int cycles, max_page, max_curve, max_block_no, accu_cycles, curr_accu_cycle, 
    curr_cycle;
short spc_state,armed, init_status, module_type, fifo_stopt_possible, first_write;
short sync_state[MAX_NO_OF_SPC], mod_state[MAX_NO_OF_SPC];
short collection_paused[MAX_NO_OF_SPC];
float ovfl_time, new_time, old_time, disp_time, mem_bank, last_gap[MAX_NO_OF_SPC];

unsigned short offset_value, *buffer, *ptr, fpga_version;
unsigned long page_size, photons_to_read, words_to_read, fifo_size, 
              max_ph_to_read, max_words_in_buf, words_left, words_in_buf, current_cnt,
              mt_clock, photon_left, phot_in_buf, phot_cnt, strbuf_size, phot_buf_size;
unsigned int spc_header;
short fifo_type, stream_hndl, stream_type, what_to_read;
PhotStreamInfo stream_info;
PhotInfo   phot_info;
PhotInfo64 phot_info64, *phot_buffer, *phot_ptr;
char phot_fname[80] = "test_photons.spc";

rate_values rates[MAX_NO_OF_SPC];


short  test_fill_state ( void );
short  save_photons_in_file ( void );
static void init_fifo_measurement( void ); // init actions same for part 4 and 5


#pragma argsused

//int WINAPI  WinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance,
//                     LPSTR lpCmdLine,  int nShowCmd )

int main ( int argc, char *argv[] )
{
char  ini_name[256] = "d:\\spcm\\lib\\cvi\\spc150test.ini";
char  dest_inifile[256] = "spcm_test.ini", sdt_fname[256];

int   val, status, use_part;
short i,j;
short ret;

use_part = 0; //  part 0 will be executed

// assumption is done here that we work with SPC-130 modules
module_type = M_SPC150;

phot_buffer = NULL; buffer = NULL;
first_write = 1;

for ( i = 1; i < argc; i++) {
  if ( (*++argv )[0] == '-') {
    switch (( unsigned char ) (*argv )[1] ) {
      case 's' : // force used module type
      case 'S' :
        status = sscanf ( (char *)&((*argv)[2]), "%d", &val );
        if ( status == 1 ) {
          module_type = val;
          }
        break;

      case 'f' :  // change ini file name
      case 'F' :
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
      }
    }
  }
      /* initialization must be done always at the beginning */
if(( ret = SPC_init ( ini_name )) < 0){
  if ( -ret < SPC_WRONG_ID || -ret == SPC_WRONG_LICENSE  || -ret >= SPC_NO_LICENSE)
    return ret;   // fatal error, maybe wrong ini file   or DLL not registered
  }

total_no_of_spc = no_of_active_spc= 0;
for( i = 0; i < MAX_NO_OF_SPC; i++){
  SPC_get_module_info( i,(SPCModInfo *)&mod_info[i]);
  if(mod_info[i].module_type != M_WRONG_TYPE)
    total_no_of_spc++;
  if(mod_info[i].init == INIT_SPC_OK){
    no_of_active_spc++; mod_active[i] = 1;
    act_mod = i;
    }
    else
      mod_active[i] = 0;
  }
if( !total_no_of_spc){
  // no modules found at all
  // if you want to work in simulation mode there are two ways :
  //    1 - change item simulation in .ini file to required sim. mode
  //    2 - use SPC_set_mode function

            /* SPC130 simulation mode for 4 modules */
  for( i =0 ; i < MAX_NO_OF_SPC; i++)
    mod_active[i] = 1;
  force_use = 0;
  spc_error = SPC_set_mode( SPC_SIMUL130, force_use, mod_active);
  work_mode = SPC_get_mode();
  if( work_mode != SPC_SIMUL130)
    return -1;
  //  check modules once more

  total_no_of_spc = no_of_active_spc= 0;
  for( i = 0; i < MAX_NO_OF_SPC; i++){
    SPC_get_module_info( i, (SPCModInfo *)&mod_info[i]);
    if(mod_info[i].module_type != M_WRONG_TYPE)
      total_no_of_spc++;
    if(mod_info[i].init == INIT_SPC_OK){
      no_of_active_spc++; mod_active[i] = 1;
      act_mod = i;
      }
      else
        mod_active[i] = 0;
    }
  }


if( total_no_of_spc != no_of_active_spc){
  // it is possible when some SPC modules:
  //   1 - are already used by other applications
  //        mod_info[i].init == INIT_SPC_MOD_IN_USE, mod_info[i].in_use == -1
  //   2 - didn't pass correctly through the initialisation procedure
  //        (e.g. wrong EEPROM checksum, hardware test failed )
  // it is still possible ( but not recommended) to use such module,
  //   if you call SPC_set_mode  with force_use parameter = 1
  //   e.g. SPC_set_mode(SPC_HARD, 1 , mod_active);

  }

// furthermore you can have in your system SPC modules of different types
//  (it applies only for the PCI bus modules - SPC modules on ISA bus  
//       are not affected by spcm32.dll)
//  It is not recommended (can lead to strange results) to start the same 
//    measurement on SPC modules of different types ( measurement modes can
//     have different meaning or can operate in a different way )

work_mode = SPC_get_mode(); 
for( i = 0; i < MAX_NO_OF_SPC; i++){
  if(mod_active[i]){
    if(mod_info[i].module_type != module_type){
      mod_active[i] = 0;
      no_of_active_spc--;
      }
    } 
  }

 // this will deactivate unused modules in DLL
SPC_set_mode( work_mode, 0 , mod_active);  

if( !no_of_active_spc)
  return -1;

for( i = 0; i < MAX_NO_OF_SPC; i++){
  if(mod_active[i]){
    act_mod = i;
    break;
    }
  } 

ret = SPC_get_parameters(act_mod, &spc_dat);
// directly after initialisation parameters in all modules are equal
//  but later if needed you can equalize the settings as shown below
for( i = act_mod + 1; i < MAX_NO_OF_SPC; i++){
  if(mod_active[i]){
    SPC_set_parameters( i, &spc_dat);
    }
  } 
/////////////////

init_status = SPC_get_init_status(act_mod);


/* during SPC initialization all hardware parameters are set to values
   taken from ini file ( or to default values )
   This parameter set can be overwritten by using
     SPC_set_parameters or SPC_set_parameter function.
   After setting the parameter  check return value and
   read the parameter again to get the value which was sent to the hardware */

   /* change collection time on all modules */
/*
ret = SPC_set_parameter(-1, COLLECT_TIME, 1.0); 
SPC_get_parameter( act_mod, COLLECT_TIME, &spc_dat.collect_time);

ret = SPC_set_parameter( -1, TAC_RANGE, 100.);
SPC_get_parameter( act_mod, TAC_RANGE, &spc_dat.tac_range);
*/

/* it is also possible to save current module parameters to selected ini file
          or/and restore it from ini file
  
     // save parameters from spc_dat to dest_inifile using base settings and 
     //  comments from the ini file used in SPC_init call 
     //        ( parameter source_inifile = NULL) 
ret = SPC_save_parameters_to_inifile( &spc_dat, dest_inifile, NULL, 1 );

       // read module parameters from dest_inifile file to spc_dat
      //  and then send the settings to the hardware
  
ret = SPC_read_parameters_from_inifile( &spc_dat, dest_inifile );
ret = SPC_set_parameters(act_mod, &spc_dat);
*/


     /*  before the measurement SPC memory must be configured */
no_of_routing_bits = 0;  /* simple 1 dimensional measurement */
         // configure memory on all modules
ret = SPC_configure_memory( -1, spc_dat.adc_resolution, no_of_routing_bits,
                            &spc_mem_info);
if ( ret == -SPC_BAD_FUNC)  
  // for some modes ( for example scan modes)  you can't configure but only get current configuration
  ret = SPC_configure_memory( act_mod, -1, no_of_routing_bits, &spc_mem_info);
if ( spc_mem_info.maxpage == 0)
  return -1;
  
max_block_no = spc_mem_info.max_block_no;
max_page = spc_mem_info.maxpage;
max_curve = max_block_no/max_page;
block_length = spc_mem_info.block_length;

page_size = spc_mem_info.blocks_per_frame * spc_mem_info.frames_per_page * block_length;


if ( use_part >= 0 && use_part != 0 ) 
  goto part1;        // do not execute part 0
  

////////////////////////////////////////////////////////////////

// part 0 -  example of a simple measurement  ( Single mode)  
  
////////////////////////////////////////////////////////////////


/* before the measurement  : */

SPC_enable_sequencer ( -1, 0 );    
SPC_set_parameter ( -1, MODE, NORMAL );

     /*  1 - SPC memory must be configured */
no_of_routing_bits = 0;  /* simple 1 dimensional measurement */
         // configure memory on all modules
ret = SPC_configure_memory( -1, spc_dat.adc_resolution, no_of_routing_bits,
                            &spc_mem_info);

max_block_no = spc_mem_info.max_block_no;
max_page = spc_mem_info.maxpage;
max_curve = max_block_no/max_page;
block_length = spc_mem_info.block_length;

page_size = spc_mem_info.blocks_per_frame * spc_mem_info.frames_per_page * block_length;

buffer = (unsigned short *)malloc( page_size * no_of_active_spc * sizeof(unsigned short));

     /*  2 - measured blocks in SPC memory must be filled (cleared ) */
meas_page = 0;
offset_value = 0;
//ret=SPC_fill_memory(-1, 0, meas_page, offset_value);
ret = SPC_fill_memory(-1, -1, meas_page, offset_value);
if(ret > 0){
  // fill started but not yet finished
  ret = test_fill_state();
  }
if(ret < 0)  // errors during memory fill
  return -1;

     /*  3 - measurement  page must be set on all modules*/
SPC_set_page( -1, meas_page);

     /*  4 - rates should  be cleared ,sync state can be checked */
for( i = 0; i < MAX_NO_OF_SPC; i++){
  if(mod_active[i]){
    SPC_clear_rates(i);  /* it is needed one time only */
    SPC_get_sync_state( i, &sync_state[i]);
    }
  }
  
   /*  now measurement can be started on all used modules */
new_time = old_time = 0.0;
disp_time = 1.0;
  /* remember that measurement start can really be synchronous,
           if you use external trigger */
for( i = 0; i < MAX_NO_OF_SPC; i++){
  if(!mod_active[i]) continue;
  if(( ret = SPC_start_measurement(i)) < 0){
    break;
    }
  }  
if(!ret){
  /* now test SPC state to check whether measurement is finished */
  armed = 1;
  while(armed){
    spc_state = 0;
    for( i = 0; i < MAX_NO_OF_SPC; i++){
      if(!mod_active[i]) continue;
      SPC_test_state( i, &mod_state[i]);
      spc_state |= mod_state[i];
      }
    if(spc_state & SPC_ARMED){  /* 1 - still armed */
      /* if required , time from start can be tested ,
                rates and intermediate results can be read */
      SPC_get_time_from_start( act_mod, &new_time);
      if( new_time - old_time > disp_time){
        for( i = 0; i < MAX_NO_OF_SPC; i++){
          if(!mod_active[i]) continue;
          SPC_read_rates( i, &rates[i]);
          collection_paused[i] = SPC_pause_measurement(i);
          /* collection can be paused to read intermediate results  */
          /*  or stopped using SPC_stop_measurement */
          if(collection_paused[i] > 0 ){
             /* read  intermediate results */
            SPC_read_data_block(i, 0 ,meas_page, 1, 0,(short)(block_length-1), buffer);
             /* possibly display intermediate results if required */
            }
          }
             /* now restart measurement */
        for( i = 0; i < MAX_NO_OF_SPC; i++){
          if(!mod_active[i]) continue;
          if(collection_paused[i])
            if(( ret = SPC_restart_measurement(i)) != 0)
              break;
          }
        if(ret < 0)  
          armed = 0;
        }
      }
      else{
        armed = 0;
        if(spc_state & SPC_OVERFL){   /* overflow  */
          for( i= 0; i < MAX_NO_OF_SPC; i++){
            if(!mod_active[i]) continue;
            if(mod_state[i] & SPC_OVERFL){
              SPC_get_break_time( i, &ovfl_time);
              break;
              }
            }
          }
        }
    }

    /* measurement finished - read results */
  ptr = buffer;
  j = 0;
  for ( i = 0; i < MAX_NO_OF_SPC; i++){
    if(!mod_active[i]) continue;
    //    use SPC_read_data_page - in this case 'buffer' must be allocated with
    //     spc_mem_info.blocks_per_frame * spc_mem_info.frames_per_page *
    //        * spc_mem_info.block_length  16-bit words
    SPC_read_data_page( i, meas_page, meas_page, ptr);
    if ( ++j == no_of_active_spc )
      break;
    ptr += page_size;
    
/*   to read separate blocks use  SPC_read_block or SPC_read_data_block(obsolete)
    for( j = 0; j < max_curve; j++){  // here - max_curve = 1
      SPC_read_block( i, j, 0, meas_page, 0, (short)(block_length-1), buffer);
//    SPC_read_data_block( i, (short)j, meas_page, 1, 0, (short)(block_length-1), buffer);
      }
*/  
    }          


  // you can store your parameters and measurement results to a .sdt file 
  //   using function  SPC_save_data_to_sdtfile, and then load it to SPC main software
  //   
  ret = SPC_save_data_to_sdtfile ( -1, buffer, 2 * page_size * no_of_active_spc, 
                                   "dll_results.sdt" );
  }
///////////////////////////////////////////////////
//  end of Single mode measurement example
///////////////////////////////////////////////////

  
part1:  
if ( use_part >= 0 && use_part != 1 ) 
  goto part2;        // do not execute part 1
  
  
  
////////////////////////////////////////////////////////////////

//  part 1 - example of single molecule flow measurement  ( Continuous Flow mode)
//                - not for SPC7x0/830/140
  
////////////////////////////////////////////////////////////////


if( module_type < M_SPC700 && module_type != M_SPC140){

  // to read the bank and save in .sdt file
  buffer = (unsigned short *)realloc( buffer, max_page * page_size * 
                                    no_of_active_spc * sizeof(unsigned short));
  
  SPC_set_parameter( -1, COLLECT_TIME, 0.001); /* change collection time */
  SPC_get_parameter( act_mod, COLLECT_TIME, &spc_dat.collect_time);
  cycles = curr_cycle = 3;
  mem_bank = spc_dat.mem_bank;
 
  SPC_set_parameter ( -1, MODE, NORMAL );
  //  before measurement at first enable sequencer by calling SPC_enable_sequencer
  SPC_enable_sequencer( -1, 1);
  ret = SPC_fill_memory( -1, -1, -1, offset_value); /* clear whole bank */
  if(ret > 0)
  // fill started but not yet finished
    ret= test_fill_state();
  if(ret < 0)  // errors during memory fill
    return -1;
  if ( cycles > 1 ){
    mem_bank = mem_bank?0:1; /* reverse memory bank */
    SPC_set_parameter( -1, MEM_BANK, mem_bank);
    ret = SPC_fill_memory( -1, -1, -1, offset_value); /* clear second bank */
    if(ret > 0) 
      ret = test_fill_state();
    if(ret < 0)  // errors during memory fill
      return -1;
    mem_bank = mem_bank ? 0 : 1; /* reverse memory bank again */
    SPC_set_parameter( -1, MEM_BANK, mem_bank);
    }
  SPC_set_page( -1, 0);
  
  while ( curr_cycle ){
    for( i = 0; i < MAX_NO_OF_SPC; i++){
      if(!mod_active[i]) continue;
      // SPC_enable_sequencer was called with 'enable' parameter = 1,
      //   so 1st SPC_start_measurement call arms both banks, 
      //    next calls arm current bank and switch memory bank 
      if(( ret = SPC_start_measurement(i)) < 0)
        break;
      }  
    if(!ret){
      /* now test SPC state to check whether measurement is finished */
      armed=1;
      while(armed){
        spc_state = 0;
        for( i = 0; i < MAX_NO_OF_SPC; i++){
          if(!mod_active[i]) continue;
          SPC_test_state( i,& mod_state[i]);
          spc_state |= mod_state[i];
          }
        if((spc_state & SPC_ARMED) == 0){  /*  finished */
          for( i = 0; i < MAX_NO_OF_SPC; i++){
            if(!mod_active[i]) continue;
            SPC_read_gap_time( i, &last_gap[i]);
            }
          armed = 0;
          /* now read the whole actual bank and save results */
          //  e.g. read whole bank :
          //  - in this case 'buffer' must be allocated with
          //     spc_mem_info.blocks_per_frame * spc_mem_info.frames_per_page *
          //   * spc_mem_info.maxpage  * spc_mem_info.block_length  16-bit words
          //
          // !!!!!!!!!!!
          // remember that in the same time sequencer collects data in other bank
          //  you should save results and fill memory bank before the running measurement 
          //     in other bank will finish, otherwise there will be a gap time  
          for( i = 0; i < MAX_NO_OF_SPC; i++){
            if(!mod_active[i]) continue;
            SPC_read_data_page ( i, 0, max_page - 1, buffer);
            // you can store your parameters and measurement results to a .sdt file 
            //   using function  SPC_save_data_to_sdtfile, and then load it to SPC main software
            //   
            sprintf ( sdt_fname, "dll_flow_results%d_%d", i, cycles - curr_cycle );
            ret = SPC_save_data_to_sdtfile ( i, buffer, 2 * page_size * max_page, 
                                             sdt_fname );
            }
          }
        }
      if ( curr_cycle > 1){    /* clear memory for next cycle results */
          /* warning - in flow mode memory bank  changes on DLL level
                        in SPC_start_measurement procedure */
        ret = SPC_fill_memory( -1,-1,-1, offset_value);
        if(ret > 0) 
          ret = test_fill_state();
        if(ret < 0)  // errors during memory fill
          return -1;
        }
        else{
          /* last cycle for flow mode
             don't arm actual bank , only reverse memory bank */
          /* warning - in flow mode memory bank  changes on DLL level
                        in SPC_start_measurement procedure */
          SPC_get_parameter( act_mod, MEM_BANK, &mem_bank);
          mem_bank = mem_bank ? 0 : 1; /* reverse memory bank */
          SPC_set_parameter( -1, MEM_BANK, mem_bank);
          }
      }
      else
        break;
    curr_cycle--;
    }
  }
///////////////////////////////////////////////////
//  end of Continuous Flow mode measurement example
///////////////////////////////////////////////////

  
part2:  
if ( use_part >= 0 && use_part != 2 ) 
  goto part3;        // do not execute part 1
  
  
  
  
////////////////////////////////////////////////////////////////

//  part 2 - example of single molecule flow measurement with accumulation

//  ( implementation like in the main software when Continuous Flow mode with accumulation )
//             - not for SPC7x0/830/140
//  
//   for each bank measurement is started 'accu_cycles' times without filling memory
//   then next bank is started and data are read from the current bank
//   gap time information is not available
//     
////////////////////////////////////////////////////////////////


if( module_type < M_SPC700 && module_type != M_SPC140){

  // to read the bank and save in .sdt file
  buffer = (unsigned short *)realloc( buffer, max_page * page_size * 
                                    no_of_active_spc * sizeof(unsigned short));
/*  
  SPC_set_parameter ( -1, COLLECT_TIME, 0.001); // change collection time 
  SPC_get_parameter ( act_mod, COLLECT_TIME, &spc_dat.collect_time);
*/
  cycles = curr_cycle = 3;   // no of measured banks
  accu_cycles = curr_accu_cycle = 10;  // no of accumulation cycles for each bank
  mem_bank = spc_dat.mem_bank;
 
  SPC_set_parameter ( -1, MODE, NORMAL );
  //  before measurement at first enable sequencer by calling SPC_enable_sequencer
  //   important  with enable parameter = 2  !!!!!
  SPC_enable_sequencer( -1, 2);
  ret = SPC_fill_memory( -1, -1, -1, offset_value); /* clear whole bank */
  if ( ret > 0)
  // fill started but not yet finished
    ret= test_fill_state();
  if ( ret < 0 )  // errors during memory fill
    return -1;
  
  SPC_set_page ( -1, 0);
  
  while ( curr_cycle ){
    while ( curr_accu_cycle ){
      for( i = 0; i < MAX_NO_OF_SPC; i++){
        if(!mod_active[i]) continue;
        // if SPC_enable_sequencer was called with 'enable' parameter = 2,
        //   1st SPC_start_measurement does not arm both banks, but only one
        //   also memory bank is not changed 
        if(( ret = SPC_start_measurement(i)) < 0)
          break;
        }  
      if ( !ret ){
        armed = 1;
        if ( curr_accu_cycle == 1 && curr_cycle > 1 ){ // last accumulation cycle
          // clear next bank memory and arm next bank for next cycle 
               // switch  memory bank 
          SPC_get_parameter( act_mod, MEM_BANK, &mem_bank);
          mem_bank = mem_bank ? 0 : 1; // reverse memory bank   
          SPC_set_parameter( -1, MEM_BANK, mem_bank);
          ret = SPC_fill_memory( -1,-1,-1, offset_value);
          if(ret > 0) 
            ret = test_fill_state();
          if(ret < 0)  // errors during memory fill
            return -1;
           // arm next bank on all modules
          for( i = 0; i < MAX_NO_OF_SPC; i++){
            if(!mod_active[i]) continue;
            if(( ret = SPC_start_measurement(i)) < 0)
              break;
            }  
          // restoring memory bank value is not needed because
          //        SPC_start_measurement did it already
          }
          // check state of the current accumulation cycle -
          //  - test SPC state to check whether measurement is finished   
        while ( armed ){
          spc_state = 0;
          for( i = 0; i < MAX_NO_OF_SPC; i++){
            if(!mod_active[i]) continue;
            SPC_test_state( i, &mod_state[i]);
            spc_state |= mod_state[i];
            }
          if ( (spc_state & SPC_ARMED) == 0){  //  current accu cycle finished   
            armed = 0;
            for( i = 0; i < MAX_NO_OF_SPC; i++){
              if(!mod_active[i]) continue;
              // gap time information is not available
              SPC_stop_measurement(i); // to switch off SEQ_RUN
              }
            SPC_set_page ( -1, 0);
            }
          }
        }
        else
          break; // ret != 0
      curr_accu_cycle--;
      }
    if ( ret < 0 )
      break;
                     
    // all accumulation cycles done for the current bank
    // now read the whole actual bank and save results 
    //  e.g. read whole bank :
    //  - in this case 'buffer' must be allocated with
    //     spc_mem_info.blocks_per_frame * spc_mem_info.frames_per_page *
    //   * spc_mem_info.maxpage  * spc_mem_info.block_length  16-bit words
    //
    SPC_get_parameter( act_mod, MEM_BANK, &mem_bank);
    for( i = 0; i < MAX_NO_OF_SPC; i++){
      if(!mod_active[i]) continue;
      SPC_read_data_page ( i, 0, max_page - 1, buffer);
      // you can store your parameters and measurement results to a .sdt file 
      //   using function  SPC_save_data_to_sdtfile, and then load it to SPC main software
      //   
      sprintf ( sdt_fname, "dll_flow_results%d_%d.sdt", i, cycles - curr_cycle );
      ret = SPC_save_data_to_sdtfile ( i, buffer, 2 * page_size * max_page, 
                                          sdt_fname );
      }

              // switch  bank  to another ( which was already started )
    SPC_get_parameter( act_mod, MEM_BANK, &mem_bank);
    mem_bank = mem_bank ? 0 : 1; // reverse memory bank    
    SPC_set_parameter( -1, MEM_BANK, mem_bank);

    curr_cycle--; 
    curr_accu_cycle = accu_cycles;
    }
  }
///////////////////////////////////////////////////
//  end of the example of Continuous Flow mode measurement with accumulation 
///////////////////////////////////////////////////

  



part3:  
if ( use_part >= 0 && use_part != 3 ) 
  goto part4;        // do not execute part 3
  
  
////////////////////////////////////////////////////////////////

//  part 3 -  example of Scan Sync In measurement     - not for SPC130/6x0    
  
////////////////////////////////////////////////////////////////

if( module_type >= M_SPC700 || module_type == M_SPC140 || module_type == M_SPC150 ){

  // to read the bank and save in .sdt file
  buffer = (unsigned short *)realloc( buffer, max_page * page_size * 
                                    no_of_active_spc * sizeof(unsigned short));
  
  SPC_set_parameter ( -1, STOP_ON_OVFL, 0 );
  SPC_set_parameter ( -1, STOP_ON_TIME, 1 );

  // before measurement at first enable sequencer by calling SPC_enable_sequencer
  //  but not for SPC-150 module
  if ( module_type != M_SPC150 )
    SPC_enable_sequencer ( -1, 1 );    
  SPC_set_parameter ( -1, MODE, SCAN_IN );  
  
  // SPC memory for Scan modes is configured by setting scan size parameters
  //   SPC_configure_memory is used at the end to get the current state
  SPC_set_parameter ( -1, ADC_RESOLUTION, 6 ); // 6 bits ADC 
  SPC_set_parameter ( -1, SCAN_SIZE_X, 64 );   // image size 64 * 64
  SPC_set_parameter ( -1, SCAN_SIZE_Y, 64 );  
  SPC_set_parameter ( -1, SCAN_ROUT_X, 1 );    // without routing
  SPC_set_parameter ( -1, SCAN_ROUT_Y, 1 );  
  // set also other parameters which can be important in Scan mode
  // SCAN_POLARITY, SCAN_FLYBACK, SCAN_BORDERS, PIXEL_TIME, PIXEL_CLOCK, 
  // LINE_COMPRESSION, EXT_PIXCLK_DIV
  
     // read current memory configuration to spc_mem_info
  SPC_configure_memory( -1, -1, 0, &spc_mem_info);

  max_block_no = spc_mem_info.max_block_no;
  max_page = spc_mem_info.maxpage;
  max_curve = max_block_no / max_page;
  block_length = spc_mem_info.block_length;

  page_size = spc_mem_info.blocks_per_frame * spc_mem_info.frames_per_page * block_length;

  buffer = (unsigned short *)malloc( page_size * no_of_active_spc * sizeof(unsigned short));

     //      measured page in SPC memory must be filled (cleared ) 
  meas_page = 0;
  offset_value = 0;

  SPC_set_page( -1, meas_page);   // before fill

  ret = SPC_fill_memory(-1, -1, meas_page, offset_value);
  if(ret > 0){
    // fill started but not yet finished
    ret = test_fill_state();
    }
  if(ret < 0)  // errors during memory fill
    return -1;

     //   measurement  page must be set on all modules  
  SPC_set_page( -1, meas_page);

     //  rates should  be cleared ,sync state can be checked   
  for( i = 0; i < MAX_NO_OF_SPC; i++){
    if(mod_active[i]){
      SPC_clear_rates(i);  /* it is needed one time only */
      SPC_get_sync_state( i, &sync_state[i]);
      }
    }
  
   /*  now measurement can be started on all used modules */
  new_time = old_time = 0.0;
  disp_time = 1.0;
  /* remember that measurement start can really be synchronous,
           if you use external trigger */
  SPC_set_parameter ( -1, TRIGGER, 1 );   // trigger active low
  
  for( i = 0; i < MAX_NO_OF_SPC; i++){
    if(!mod_active[i]) continue;
    if(( ret = SPC_start_measurement(i)) < 0){
      break;
      }
    }  
  if(!ret){
  /* now test SPC state to check whether measurement is finished */
    armed = 1;
    while(armed){
      spc_state = 0;
      for( i = 0; i < MAX_NO_OF_SPC; i++){
        if(!mod_active[i]) continue;
        SPC_test_state( i, &mod_state[i]);
        spc_state |= mod_state[i];
        }
      // user must provide safety way out from this loop 
      //    in case when trigger or some of scan signals are not OK,
      //          it can hang up computer
      if(spc_state & SPC_WAIT_TRG){   // wait for trigger                
        continue;
        }
      if (spc_state & SPC_ARMED){  //  system armed   
        if ( (spc_state & SPC_MEASURE) == 0){
          // system armed but collection not started because
          //   it is still waiting for Sync signals
          continue;
          }
          else{ // measurement in progress 
            // if required , rates can be read   
            SPC_get_time_from_start( act_mod, &new_time);
            if( new_time - old_time > disp_time){
              for( i = 0; i < MAX_NO_OF_SPC; i++){
                if(!mod_active[i]) continue;
                SPC_read_rates( i, &rates[i]);
                // collection cannot be paused for scan modes 
                }
              }
            //  If during the measurement SPC_stop_measurement is called :
            //      1st call to the function forces very short collection time 
            //         to finish the current frame and returns error -21. 
            //         The measurement will stop automatically after finishing current frame. 
            //      2nd call will stop the measurement without waiting for the end of frame.            
            }
        }    
        else{
          armed = 0;
          }
      }
  
    /* measurement finished - reset hardware and read results */
    
    for ( i = 0; i < MAX_NO_OF_SPC; i++){
      if ( !mod_active[i] ) continue;
      SPC_stop_measurement ( i );
      }

    ptr = buffer;
    j = 0;
    for ( i = 0; i < MAX_NO_OF_SPC; i++){
      if(!mod_active[i]) continue;
      //    use SPC_read_data_page - in this case 'buffer' must be allocated with
      //     spc_mem_info.blocks_per_frame * spc_mem_info.frames_per_page *
      //        * spc_mem_info.block_length  16-bit words
      SPC_read_data_page( i, meas_page, meas_page, ptr);
      if ( ++j == no_of_active_spc )
        break;
      ptr += page_size;
      }          


    // you can store your parameters and measurement results to a .sdt file 
    //   using function  SPC_save_data_to_sdtfile, and then load it to SPC main software
    //   
    ret = SPC_save_data_to_sdtfile ( -1, buffer, 2 * page_size * no_of_active_spc, 
                                     "dll_scan_results.sdt" );
    }
  }
///////////////////////////////////////////////////
//  end of Scan Sync In measurement example
///////////////////////////////////////////////////


part4:  
if ( use_part >= 0 && use_part != 4 ) 
  goto part5;        // do not execute part 4
  
  

////////////////////////////////////////////////////////////////

//  part 4 - example of FIFO measurement for one module    - not for SPC7x0        
  
////////////////////////////////////////////////////////////////
if( module_type == M_SPC700 || module_type == M_SPC730 )  // - not for SPC7x0  
  goto part6;

init_fifo_measurement( ); // init actions same for part 4 and 5
           // read carefully all comments in init_fifo_measurement() function 
           
buffer = (unsigned short *)realloc( buffer, max_words_in_buf * sizeof(unsigned short));

photons_to_read = 10000;
if ( fifo_type == FIFO_48 )
  words_to_read = 3 * photons_to_read;
  else
    words_to_read = 2 * photons_to_read;
    
words_left = words_to_read;  

// User may want to stop the measurement : 
//    - after reading required number of photons ( as in this example ), 
//    - by user break ( by using SPC_stop_measurement )
//    - for example when Fifo overrun occurs
//    - after certain measurement time ( when Stop on time is set )   

ret = SPC_start_measurement ( act_mod );

while ( !ret ){
  // now test SPC state and read photons
  SPC_test_state( act_mod, &spc_state);
    // user must provide safety way out from this loop 
    //    in case when trigger will not occur or required number of photons 
    //          cannot be reached
  if(spc_state & SPC_WAIT_TRG){   // wait for trigger                
    continue;
    }
  if ( words_left > max_words_in_buf - words_in_buf ) 
    // limit current_cnt to the free space in buffer
    current_cnt = max_words_in_buf - words_in_buf;
    else
      current_cnt = words_left;
  ptr = (unsigned short *)&buffer[words_in_buf];
    
  if (spc_state & SPC_ARMED){  //  system armed   
    if ( spc_state & SPC_FEMPTY ) 
      continue;  // Fifo is empty - nothing to read
    
    // before the call current_cnt contains required number of words to read from fifo
    ret = SPC_read_fifo ( act_mod, &current_cnt, ptr);
    // after the call current_cnt contains number of words read from fifo  
    
    words_left -= current_cnt;
    if ( words_left <= 0 )  
      break;   // required no of photons read already

    if ( spc_state & SPC_FOVFL ){
      // Fifo overrun occured 
      //  - macro time information after the overrun is not consistent
      //    consider to break the measurement and lower photon's rate
      break;
      }
    // during the running measurement it is possible to check how occupied is FIFO
    //  by calling SPC_get_fifo_usage function
    
    words_in_buf += current_cnt;
    if ( words_in_buf == max_words_in_buf ){
      // your buffer is full, but photons are still needed 
      // save buffer contents in the file and continue reading photons
      ret = save_photons_in_file();
      words_in_buf = 0;
      }
    }
    else{
      if ( fifo_stopt_possible && ( spc_state & SPC_TIME_OVER ) != 0 ){
        // measurement stopped after collection time
        // read rest photons from the fifo
        // before the call current_cnt contains required number of words to read from fifo
        ret = SPC_read_fifo ( act_mod, &current_cnt, ptr);
        // after the call current_cnt contains number of words read from fifo  
    
        words_left -= current_cnt;
        words_in_buf += current_cnt;
        break;
        }
      }
  }

// SPC_stop_measurement should be called even if the measurement was stopped after collection time
//           to set DLL internal variables
SPC_stop_measurement ( act_mod );  

if ( words_in_buf > 0 ) 
  save_photons_in_file();   

//  to extract photons information from .spc file use section below
//    see  spcm_def.h for PhotStreamInfo and  PhotInfo definitions
// 1. Stream of photons files must be initialized

stream_type = BH_STREAM;
what_to_read = 1;   // valid photons
if ( fifo_type == FIFO_IMG ){
  stream_type |= MARK_STREAM;
  what_to_read |= ( 0x4 | 0x8 | 0x10 );   // also pixel, line, frame markers possible
  }

// there is new alternative method:
//  call SPC_get_fifo_init_vars function to get:
//      - values needed to init photons stream  
//      -  .spc file header

ret = SPC_get_fifo_init_vars ( act_mod,  &fifo_type, &stream_type, NULL, NULL);


stream_hndl = SPC_init_phot_stream ( fifo_type, phot_fname, 1, stream_type, what_to_read);
if ( stream_hndl >= 0 ){
  // 2. in every moment of extracting current stream state can be checked
  SPC_get_phot_stream_info ( stream_hndl, &stream_info );

  ret = 0;
  while( !ret ){  // untill error ( for example end of file )
    // user must provide safety way out from this loop 
    // fill phot_info structure with subsequent photons information
    ret = SPC_get_photon( stream_hndl, &phot_info ); 
    // save it somewhere
    }
  SPC_get_phot_stream_info ( stream_hndl, &stream_info );
  // - at the end close the opened stream
  SPC_close_phot_stream ( stream_hndl );
  }
// end of extracting photons section
 

///////////////////////////////////////////////////
//  end of part 4 - FIFO measurement example
///////////////////////////////////////////////////

part5:

if ( use_part >= 0 && use_part != 5 ) 
  goto part6;        // do not execute part 5

////////////////////////////////////////////////////////////////

//  part 5 - example of FIFO measurement using buffered photons stream
//                     for one module    - not for SPC7x0        
  
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

if( module_type == M_SPC700 || module_type == M_SPC730 )  // - not for SPC7x0  
  goto part6;

init_fifo_measurement( ); // init actions same for part 4 and 5
         // read carefully all comments in init_fifo_measurement() function 

SPC_set_parameter(-1, STOP_ON_TIME, 1); 
SPC_set_parameter(-1, COLLECT_TIME, 5.0); 
               
photons_to_read = 15000000;
if ( fifo_type == FIFO_48 )
  words_per_phot = 3;
  else
    words_per_phot = 2;
    
photon_left = photons_to_read;  
phot_in_buf = 0;

// allocate buffer for photons extracted from buffered stream
phot_buffer = (PhotInfo64 *)calloc( photons_to_read,  sizeof ( PhotInfo64 ));
phot_buf_size =  photons_to_read * sizeof ( PhotInfo64 );

// User may want to stop the measurement : 
//    - after reading required number of photons ( as in this example ), 
//    - by user break ( by using SPC_stop_measurement )
//    - for example when Fifo overrun occurs
//    - after certain measurement time ( when Stop on time is set )   

//1.  before starting the measurement 'buffered stream' must be initialized

// 1.1 - get macro time clock value
switch ( fifo_type){
  case FIFO_32:
  case FIFO_48:
    mt_clock = 500;  // in 0.1 ns units
    break;
    
  default:
      // for all other modes Macro Time clock is defined by MACRO_TIME_CLK parameter
    if ( spc_dat.macro_time_clk ){
      // SYNC frequency  or part of it 
      // read rates and use sync_rate to calculate it
      SPC_read_rates ( act_mod, &rates[act_mod]);
      float fval = rates[act_mod].sync_rate;
      i = spc_dat.macro_time_clk;
      while ( i > 1){
        fval = fval / 2.; i--;
        }
      fval = 1. / fval; // macro time  clock
      mt_clock = (unsigned long)( RoundRealToNearestInteger (fval * 1e10) );  // in 0.1ns units 
      mt_clock &= 0x00ffffff;    
      }
      else{
        mt_clock = 500; // 50ns in 0.1ns units
        if ( module_type == M_SPC140 ){
          SPC_get_version ( act_mod, &fpga_version ); 
          if ( ( fpga_version & 0xff00) >= 0x200 ) // SPC-140 with FPGA v. > B0 
            mt_clock = 250; // 25ns in 0.1ns units
          }
          else
            mt_clock = 250; // 25ns in 0.1ns units
        }
    break;
  }
stream_type = BH_STREAM;
what_to_read = 1;   // valid photons
if ( fifo_type == FIFO_IMG ){
  stream_type |= MARK_STREAM;
  what_to_read |= ( 0x4 | 0x8 | 0x10 );   // also pixel, line, frame markers possible
  }

// you can decide when stream buffers will be freed
// if you add bit FREE_BUF_STREAM the buffer will be freed ,
//      when, during SPC_get_photons_from_stream call, all photons from the buffer are extracted
//      after this it will be not possible to use the buffer again, for example to get data from it
//       ( SPC_get_buffer_from_stream)
//    FREE_BUF_STREAM option is recommended for long measurements with lots of data read from FIFO
//       which could make buffers allocated space too big, but saving buffers contents 
//             after extracting photons will not be possible 

//stream_type |= FREE_BUF_STREAM;


/////////////////////////////////////////////////////////////
// there is new alternative method:
//  call SPC_get_fifo_init_vars function to get:
//      - values needed to init photons stream  
//      -  .spc file header

ret = SPC_get_fifo_init_vars ( act_mod,  &fifo_type, &stream_type, (int *)&mt_clock, &spc_header);

if ( ret == -SPC_RATES_NOT_RDY ){
  // cannot calculate mt_clock because rates values are not ready yet, 
  //        default rates collecting time is 1 sec
  Delay ( 1.1);      // once more after delay
  ret = SPC_get_fifo_init_vars ( act_mod,  &fifo_type, &stream_type, (int *)&mt_clock, &spc_header);
  }

if ( stream_type & MARK_STREAM ){
  what_to_read |= ( 0x4 | 0x8 | 0x10 | 0x20  );   // pixel, line, frame markers and M3 possible
  }

//what_to_read = 1 | 2 | 0x8 | 0x10 ; //  for test , all, w/o pixel markers
if ( ret < 0 )  
  goto part6;   // error

/////////////////////////////////////////////////////////////

stream_hndl = SPC_init_buf_stream ( fifo_type, stream_type, what_to_read, mt_clock , 0 );
if ( stream_hndl < 0 ){
  goto part6;   // error
  }
  
  // 2. in every moment current stream state can be checked
SPC_get_phot_stream_info ( stream_hndl, &stream_info );

// double start_time = 0.0, stop_time = 0.0;
// it is possible to define the moment when extracting photons from the stream will start
//    using SPC_stream_start_condition. Start time can be defined. 
//    After reaching start_time appearance of photons or markers defined in start_OR(AND)_mask is tested

// SPC_stream_start_condition ( stream_hndl, start_time, 0, 0 );
//  for example
//   SPC_stream_start_condition ( stream_hndl, 5.0, 0, 0x60000000 );
//   extracting photons will start when:
//                      - at first 5 sec expires
//                      - both Marker 1 & 2 must be found in data stream read from FIFO


// it is possible to define the moment when extracting photons from the stream will stop 
//    using SPC_stream_stop_condition. Stop time can be defined. 
//    After reaching stop_time appearance of photons or markers defined in stop_OR(AND)_mask is tested

// SPC_stream_stop_condition ( stream_hndl, 0, stop_time, 0, 0 );

     // start condition : frame & line marker should appear after 1 sec
//SPC_stream_start_condition ( stream_hndl, 1.0, 0, 0x60000000 );        

     // start condition : frame marker should appear after 2 sec
//SPC_stream_stop_condition ( stream_hndl, 2.0, 0, 0x40000000 );        

ret = SPC_start_measurement ( act_mod );

while ( ret >= 0 ){
  // now test SPC state and read photons
  SPC_test_state( act_mod, &spc_state);
    // user must provide safety way out from this loop 
    //    in case when trigger will not occur or required number of photons 
    //          cannot be reached
  if(spc_state & SPC_WAIT_TRG){   // wait for trigger                
    continue;
    }

  current_cnt = photon_left * words_per_phot; 
  phot_cnt = photon_left;
  phot_ptr = (PhotInfo64 *)&phot_buffer[phot_in_buf];  
  if (spc_state & SPC_ARMED){  //  system armed   
    if ( spc_state & SPC_FEMPTY ) 
      continue;  // Fifo is empty - nothing to read
    
    if ( spc_state & SPC_WAIT_FR ){ 
       // collection time expired in FIFO32_M mode
/// !!!!!!!!!!!!!!
//    the mode = FIFO32_M ( fifo_type = FIFO_IMG ) switches off (in hardware) Stop on time
//        in order to make possible finishing current frame
//     the measurement will not stop after expiration of collection time
//     SPC_test_state sets flag SPC_WAIT_FR in the status - software should still read photons 
//       untill next frame marker appears and then should stop the measurement
//     in this example it is not implemented ( it is done in SPCM software)
//   to avoid this behaviour - set mode to normal fifo mode = 1 ( ROUT_OUT)
/// !!!!!!!!!!!!!!
      SPC_stop_measurement ( act_mod );  
      SPC_test_state( act_mod, &spc_state);
      goto read_rest;
      }
      
    // FIFO contents is read to stream buffers ( on a DLL level) - no need to allocate any external buffer now 
    // before the call current_cnt contains required number of words to read from fifo
    ret = SPC_read_fifo_to_stream ( stream_hndl, act_mod, &current_cnt );
    if ( ret < 0 )
      break;
    // after the call current_cnt contains number of words read from fifo and putted to stream buffers  
    
    // photons can be extracted from the stream directly after reading from FIFO, or in any other moment,
    //    also by using other program's thread
    
    // if you call SPC_get_photons_from_stream in this loop, be aware of the photons rate ( rates.adc_rate)
    //    - with high photons rates it can cause fifo overrun
    //  you can delay or interrupt extracting photons by changing start or stop condition 
    //     by calling SPC_stream_start(stop)_condition
    
    // before the call phot_cnt contains required number of photons to get from buffered stream
    ret = SPC_get_photons_from_stream ( stream_hndl, phot_ptr, (int *)&phot_cnt );
    // after the call phot_cnt contains number of photons taken from buffered stream
      if ( ret == 2 || ret == -SPC_STR_NO_START || ret == -SPC_STR_NO_STOP){  
          // end of the stream or start/stop condition not found yet
          // during running measurement these errors should be ignored
      ret = 0;
      }
    photon_left -= phot_cnt;
    phot_in_buf += phot_cnt;

    if ( ret == 1 ) // stop condition reached
      break;
      
    if ( phot_in_buf >= photons_to_read )  
      break;   // required no of photons read already

    if ( spc_state & SPC_FOVFL ){
      // Fifo overrun occured 
      //  - macro time information after the overrun is not consistent
      //    consider to break the measurement and lower photon's rate
  //  break;
      }
    // during the running measurement it is possible to check how occupied is FIFO
    //  by calling SPC_get_fifo_usage function
    
    }
    else{
      if ( fifo_stopt_possible && ( spc_state & SPC_TIME_OVER ) != 0 ){
        // measurement stopped after collection time
        // read rest photons from the fifo to the stream
read_rest:
        ret = 0; current_cnt = photon_left * words_per_phot; 
        while ( !ret && current_cnt > 0 ){
          current_cnt = photon_left * words_per_phot; 
          // before the call current_cnt contains required number of words to read from fifo
          ret = SPC_read_fifo_to_stream ( stream_hndl, act_mod, &current_cnt );
          // after the call current_cnt contains number of words read from fifo  
          }
        break;
        }
      }
  }

// SPC_stop_measurement should be called even if the measurement was stopped after collection time
//           to set DLL internal variables
SPC_stop_measurement ( act_mod );  

  //  in every moment current stream state can be tested
SPC_get_phot_stream_info ( stream_hndl, &stream_info );

if ( ret < 0 ){
  goto part6;   // error
  }

ret = 0;
while ( photon_left && !ret){ 
        // get rest photons from the stream
  phot_cnt = photon_left;
  phot_ptr = (PhotInfo64 *)&phot_buffer[phot_in_buf];  
  ret = SPC_get_photons_from_stream ( stream_hndl, phot_ptr, (int *)&phot_cnt );
  photon_left -= phot_cnt; phot_in_buf += phot_cnt;
  }


// as long as stream buffers are not freed you can call SPC_stream_reset,
//  then  SPC_stream_start(stop)_condition - 
//  in this way you can extract another set of photons using new defined start/stop time and condition


// alternatively save photons to binary file  with extension .ph
// first 4 bytes should contain the same header as for .spc files ( use SPC_get_fifo_init_vars to get it)
    //           bits 31,26 - 0 reserved for future use
    //           bits 30-27 - no of routing bits       
    //           bit  25    - markers enabled 
    //           bit  24    - femto flag, 1- femto units of MT clock, 0 - 0.1ns units
    //           bits 23-0  - macro time clock in 0.1ns or 1fs( femto, 10e-15 ) units
    //
// such .ph files are accepted by 'Convert FIFO files' in SPCM software

FILE *phfile;
if ( phot_in_buf ){
  phfile = fopen ( "test_photons.ph", "wb");
  if ( phfile ){
    unsigned int lval =  spc_header & 0x7bffffff;  // clear bits 26,31
    fwrite ( (void *)&lval, sizeof ( int ), 1, phfile ); // write header
    fwrite ( (void *)phot_buffer, sizeof ( PhotInfo ), phot_in_buf, phfile ); // write all photons to file
    fclose ( phfile);
    }
  }
//////////

//  data read from FIFO to stream buffers can be saved to .spc file for future use
//         see section below

SPC_get_phot_stream_info ( stream_hndl, &stream_info );

strbuf_size = 0;
int buf_size = 0;

if ( (stream_info.stream_type & FREE_BUF_STREAM) == 0 ){
  // only when buffers were not freed after extracting photons
  for ( i = 0; i < stream_info.no_of_buf; i++ ){
    SPC_get_stream_buffer_size ( stream_hndl, i, &strbuf_size);  // get size
    if ( !strbuf_size ) continue;
    if ( strbuf_size > buf_size ){
      buffer = (unsigned short *)realloc( buffer, strbuf_size);
      buf_size = strbuf_size;
      }
    SPC_get_buffer_from_stream ( stream_hndl, i, &strbuf_size, (char *)buffer, 0);
    words_in_buf = strbuf_size / 2; // used by save_photons_in_file
    save_photons_in_file();   
    }
  }
// end of saving .spc file

SPC_get_phot_stream_info ( stream_hndl, &stream_info );
  // - at the end close the opened stream
SPC_close_phot_stream ( stream_hndl );

///////////////////////////////////////////////////
//  end of part 5 - FIFO measurement example using buffered stream
///////////////////////////////////////////////////




part6:

if ( buffer )
  free ( buffer );
if ( phot_buffer )
  free ( phot_buffer );
  
return 0 ;
}






short test_fill_state ( void )
{
short i,ret,state;
//double stime;
time_t starttime, endtime;

// Timer() function available in LabWindows environment
// with other compiler use similar function which delivers time value

for( i = 0; i < MAX_NO_OF_SPC; i++){
  if ( mod_active[i] ){
    time( &starttime );
//  stime = Timer();
    while(1){
      ret = SPC_test_state ( i, &state);
      if ( ret < 0) return ret;
      if( (state & SPC_HFILL_NRDY) == 0) 
        break;  // fill finished
      time ( &endtime );
      if ( difftime ( endtime, starttime) > 2.5){
//    if(Timer() - stime > 2.5 ){   // 0.5 for SPC600 ,2.5 for SPC700
        ret = -SPC_FILL_TOUT;  
        return ret;
        }
      }
    }
  }
return 0;  
}




short  save_photons_in_file( void )
{
long ret;
int i;
unsigned short first_frame[3], no_of_fifo_routing_bits;
unsigned long lval;
float fval;
FILE *stream;

if ( first_write ){
    // in order to be compatible with BH .spc files 1st photon entry in the file
    //   should contain macro time clock & current no of rout bits information 

    // for all 32bit fifo types
    //           bit  31  - invalid flag
    //           bits 30-27 = no of rout bits,
    //           bits 26-24 flags 26 - raw data file, 25 - markers enabled
    //           bits 23-0 macro time clock in 0.1ns units
    // for SPC6x0 in 48bit mode
    //    1st 16-bit word  bit  12  - invalid flag
    //                     bits 11-8 = no of rout bits,
    //    2nd 16-bit word  bits 15-0 macro time clock in 0.1ns units

  no_of_fifo_routing_bits = 3; // it means 8 routing channels - default value
                               //  set to 0 if router is not used
  switch ( fifo_type ){
    case FIFO_48:
      first_frame[0] = 0x1000 | (no_of_fifo_routing_bits << 8 );
      first_frame[1] = 500;  // MT clock always 50ns , in 0.1ns units   
      first_frame[2] = 0;
      break;
    case FIFO_32:
      lval = 0x80000000 | (no_of_fifo_routing_bits << 27) | 500; // MT clock always 50ns 
      first_frame[0] = (unsigned short)lval;
      first_frame[1] = (unsigned short)( lval >> 16 );
      break;
    default:
      // for all other modes Macro Time clock is defined by MACRO_TIME_CLK parameter
      if ( spc_dat.macro_time_clk ){
        // SYNC frequency  or part of it 
        // read rates and use sync_rate to calculate it
        SPC_read_rates ( act_mod, &rates[act_mod]);
        fval = rates[act_mod].sync_rate;
        i = spc_dat.macro_time_clk;
        while ( i > 1){
          fval = fval / 2.; i--;
          }
        fval = 1. / fval; // macro time  clock
        lval = (unsigned long)( RoundRealToNearestInteger (fval * 1e10) );  // in 0.1ns units 
        lval &= 0x00ffffff;    
        }
        else{
          lval = 500; // 50ns in 0.1ns units
          if ( module_type == M_SPC140 ){
            SPC_get_version ( act_mod, &fpga_version ); 
            if ( ( fpga_version & 0xff00) >= 0x200 ) // SPC-140 with FPGA v. > B0 
              lval = 250; // 25ns in 0.1ns units
            }
          if ( module_type == M_SPC150 )
            lval = 250; // 25ns in 0.1ns units
          }
      lval = 0x80000000 | (no_of_fifo_routing_bits << 27) | lval; 
      if ( fifo_type == FIFO_IMG )   // SPC-140 with FPGAv. > B0 , SPC-150
        lval |= 0x02000000;   // bit 25
      first_frame[0] = (unsigned short)lval;
      first_frame[1] = (unsigned short)( lval >> 16 );
      break;
    }

/////////////////////////////////////////////////////////////
// there is new alternative method:
//  call SPC_get_fifo_init_vars function to get:
//      - values needed to init photons stream  
//      -  .spc file header

  first_frame[2] = 0;
 
  ret = SPC_get_fifo_init_vars ( act_mod,  NULL, NULL, NULL, &spc_header);
  if ( !ret ){
    first_frame[0] = (unsigned short)spc_header;
    first_frame[1] = (unsigned short)( spc_header >> 16 );
    }
    else
     return -1;
/////////////////////////////////////////////////////////////

  first_write = 0;
      // write 1st frame to the file
  stream = fopen ( phot_fname, "wb");
  if ( !stream )
    return -1;
  
  if ( fifo_type == FIFO_48 )
    fwrite ( (void *)&first_frame[0], 2, 3, stream ); // write 3 words ( 48 bits )
    else
      fwrite ( (void *)&first_frame[0], 2, 2, stream ); // write 2 words ( 32 bits )
  }
  else{
    stream = fopen (phot_fname, "ab");
    if ( !stream )
      return -1;
    fseek ( stream, 0, SEEK_END);     // set file pointer to the end
    }

ret = fwrite ( (void *)buffer, 1, 2 * words_in_buf, stream ); // write photons buffer
fclose ( stream);
if ( ret != 2 * words_in_buf )
  return -1;     // error type in errno

return 0;
}



static void init_fifo_measurement( void ) // init actions same for part 4 and 5
{    
float curr_mode;

    // in most of the modules types with FIFO mode it is possible to stop the fifo measurement 
    //   after specified Collection time
fifo_stopt_possible = 1;
first_write = 1;

SPC_get_version ( act_mod, &fpga_version ); 

  // SPC-150 and the newest SPC-140 & SPC-830 can record in FIFO modes 
  //                   up to 4 external markers events
  //  predefined mode FIFO32_M is used for Fifo Imaging - it uses markers 0-2 
  //     as  Pixel, Line & Frame clocks
  // in normal Fifo mode ( ROUT_OUT ) recording markers 0-3 can be enabled by 
  //    setting a parameter ROUTING_MODE ( bits 8-11 )


// before the measurement sequencer must be disabled
SPC_enable_sequencer ( act_mod, 0 );    
// set correct measurement mode

SPC_get_parameter ( act_mod, MODE, &curr_mode );  

switch (module_type ){
  case M_SPC130:
    SPC_set_parameter ( act_mod, MODE, FIFO130 );  
    fifo_type = FIFO_130;
    fifo_size = 262144;  // 256K 16-bit words
    if ( fpga_version < 0x306)  // < v.C6
      fifo_stopt_possible = 0;
    break;

  case M_SPC600:
  case M_SPC630:
    SPC_set_parameter ( act_mod, MODE, FIFO_32 );  // or FIFO_48
    fifo_type = FIFO_32;  // or FIFO_48
    fifo_size = 2 * 262144;   // 512K 16-bit words
    if ( fpga_version <=  0x207)  // <= v.B7
      fifo_stopt_possible = 0;
    break;

  case M_SPC830:
         // ROUT_OUT for 830 == fifo
         // with FPGA v. > CO  also FIFO32_M possible 
    SPC_set_parameter ( act_mod, MODE, ROUT_OUT );   // ROUT_OUT in 830 == fifo
                                                     // or FIFO_32M
    fifo_type = FIFO_830;    // or FIFO_IMG
    fifo_size = 64 * 262144; // 16777216 ( 16M )16-bit words
    break;

  case M_SPC140:
         // ROUT_OUT for 140 == fifo
         // with FPGA v. > BO  also FIFO32_M possible 
    SPC_set_parameter ( act_mod, MODE, ROUT_OUT );   // or FIFO_32M
    fifo_type = FIFO_140;  // or FIFO_IMG
    fifo_size = 16 * 262144;  // 4194304 ( 4M ) 16-bit words
    break;

  case M_SPC150:
         // ROUT_OUT in 150 == fifo
    if ( curr_mode != ROUT_OUT &&  curr_mode != FIFO_32M ){
      SPC_set_parameter ( act_mod, MODE, ROUT_OUT );   
      curr_mode = ROUT_OUT;
      }
    fifo_size = 16 * 262144;  // 4194304 ( 4M ) 16-bit words
    if ( curr_mode == ROUT_OUT) 
      fifo_type = FIFO_150; 
      else  // FIFO_IMG ,  marker 3 can be enabled via ROUTING_MODE
        fifo_type = FIFO_IMG;  
    break;
    
  }
unsigned short rout_mode, scan_polarity;
float fval;

 // ROUTING_MODE sets active markers and their polarity in Fifo mode ( not for FIFO32_M)
      // bits 8-11 - enable Markers0-3,  bits 12-15 - active edge of Markers0-3

 // SCAN_POLARITY sets markers polarity in FIFO32_M mode
SPC_get_parameter ( act_mod, SCAN_POLARITY, &fval );  
scan_polarity = fval;
SPC_get_parameter ( act_mod, ROUTING_MODE, &fval );  
rout_mode = fval;

// use the same polarity of markers in Fifo_Img and Fifo mode
rout_mode &= 0xfff8;
rout_mode |= scan_polarity & 0x7;

SPC_get_parameter ( act_mod, MODE, &curr_mode );  
if ( curr_mode == ROUT_OUT ){
  rout_mode |= 0xf00;     // markers 0-3 enabled
  SPC_set_parameter ( act_mod, ROUTING_MODE, rout_mode  ); 
  }
if ( curr_mode == FIFO_32M ){        
  rout_mode |= 0x800;     // additionally enable marker 3
  SPC_set_parameter ( act_mod, ROUTING_MODE, rout_mode ); 
  SPC_set_parameter ( act_mod, SCAN_POLARITY, scan_polarity ); 
  }

         // switch off stop_on_overfl
SPC_set_parameter ( act_mod, STOP_ON_OVFL, 0 ); 
SPC_set_parameter ( act_mod, STOP_ON_TIME, 0 ); 
if ( fifo_stopt_possible ){
    // if Stop on time is possible, the measurement can be stopped automatically 
    //     after Collection time
/// !!!!!!!!!!!!!!
//    the mode = FIFO32_M ( fifo_type = FIFO_IMG ) switches off (in hardware) Stop on time
//        in order to make possible finishing current frame
//     the measurement will not stop after expiration of collection time
//     SPC_test_state sets flag SPC_WAIT_FR in the status - software should still read photons 
//       untill next frame marker appears and then should stop the measurement
//
//   to avoid this behaviour - set mode to normal fifo mode = 1 ( ROUT_OUT)
/// !!!!!!!!!!!!!!
  SPC_set_parameter ( act_mod, STOP_ON_TIME, 1 ); 
  SPC_set_parameter ( act_mod, COLLECT_TIME, 10.0 ); // default  - stop after 10 sec
  }


// there is no need ( and also no way ) to clear FIFO memory before the measurement

// In FIFO mode the whole SPC memory is a big buffer which is filled with photons.
// From the user point of view it works like a fifo - 
//     you can read photon frames untill the fifo is empty 
//     ( or you reach required number of photons ). 
//  If your photon's rate is too high or you don't read photons fast enough, 
//    FIFO overrun can happen, 
//  it means that photons which were not read before are overwritten with the new ones.
//  - macro time information after the overrun is not consistent
// The photon's rate border at which overruns can appear depends on:
//   - module type ( fifo size ),
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
// max number of photons to read in one call - hier max_ph_to_read variable
// max_ph_to_read should also be defined carefully depending on the same aspects as 
//     overrun considerations above
// if it is too big - you can block (slow down) your system during reading fifo
//    ( for high photons rates)
// if it is too small - you can decrease your photon' rate at which overrun occurs
//    ( by big overhead for calling DLL function)
// user can experiment with max_ph_to_read value to get the best performance of your
//    system

if ( module_type == M_SPC830 )
  max_ph_to_read = 2000000; // big fifo, fast DMA readout
  else
    max_ph_to_read = 200000;
if ( fifo_type == FIFO_48 )
  max_words_in_buf = 3 * max_ph_to_read;
  else
    max_words_in_buf = 2 * max_ph_to_read;
                 
}
