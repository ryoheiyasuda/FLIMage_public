#pragma pack(push,1)

#define MAX_NO_OF_SPC    32    /* max number of SPC modules controlled by DLL */


#ifndef SPC_DEFINITIONS
#define SPC_DEFINITIONS

#define OK                0    /* no error */

/*- Error codes  ------------------------------------------------------------*/
#define  SPCDLL_ERROR_KEYWORDS \
{ \
  keyword (SPC_NONE), \
  keyword (SPC_OPEN_FILE), \
  keyword (SPC_FILE_NVALID), \
  keyword (SPC_MEM_ALLOC), \
  keyword (SPC_READ_STR), \
  keyword (SPC_WRONG_ID), \
  keyword (SPC_EEPROM_CHKSUM), \
  keyword (SPC_EEPROM_READ), \
  keyword (SPC_EEPROM_WRITE), \
  keyword (SPC_EEP_WR_DIS), \
  keyword (SPC_BAD_PAR_ID), \
  keyword (SPC_BAD_PAR_VAL), \
  keyword (SPC_HARD_TEST), \
  keyword (SPC_BAD_PARA1), \
  keyword (SPC_BAD_PARA2), \
  keyword (SPC_BAD_PARA3), \
  keyword (SPC_BAD_PARA4), \
  keyword (SPC_BAD_PARA5), \
  keyword (SPC_BAD_PARA6), \
  keyword (SPC_BAD_PARA7), \
  keyword (SPC_CANT_ARM),\
  keyword (SPC_CANT_STOP),\
  keyword (SPC_INV_REPT),\
  keyword (SPC_NO_SEQ),\
  keyword (SPC_SEQ_RUN),\
  keyword (SPC_FILL_TOUT),\
  keyword (SPC_BAD_FUNC),\
  keyword (SPC_WINDRV_ERR),\
  keyword (SPC_NOT_INIT),\
  keyword (SPC_ERR_ID),\
  keyword (SPC_RATES_NOT_RDY),\
  keyword (SPC_NO_ACT_MOD),\
  keyword (SPC_MOD_NO), \
  keyword (SPC_NOT_ACTIVE), \
  keyword (SPC_IN_USE), \
  keyword (SPC_WINDRV_VER),\
  keyword (SPC_DMA_ERR),\
  keyword (SPC_WRONG_LICENSE),\
  keyword (SPC_WRITE_STR),\
  keyword (SPC_MAX_STREAM),\
  keyword (SPC_XILINX_ERR),\
  keyword (SPC_DET_NFOUND),\
  keyword (SPC_FIRMWARE_VER),\
  keyword (SPC_NO_LICENSE),\
  keyword (SPC_LICENSE_NOT_VALID),\
  keyword (SPC_LICENSE_DATE_EXP),\
  keyword (SPC_DEEPROM_CHKSUM), \
  keyword (SPC_DEEPROM_READ), \
  keyword (SPC_DEEPROM_WRITE), \
  keyword (SPC_RAM_BUSY), \
  keyword (SPC_STR_TYPE), \
  keyword (SPC_STR_SIZE), \
  keyword (SPC_STR_BUF_NO), \
  keyword (SPC_STR_NO_START), \
  keyword (SPC_STR_NO_STOP), \
};

/*
   use function SPC_get_error_string to get error string programmatically

   SPC_NONE            0       no error
   SPC_OPEN_FILE      -1       can't open file
   SPC_FILE_NVALID    -2       not valid configuration file
   SPC_MEM_ALLOC      -3       memory allocation error
   SPC_READ_STR       -4       file read error (string or binary)
   SPC_WRONG_ID       -5       wrong module id read from SPC
   SPC_EEP_CHKSUM     -6       wrong EEPROM checksum
   SPC_EEPROM_READ    -7       error during reading EEPROM
   SPC_EEPROM_WRITE   -8       error during writing to EEPROM
   SPC_EEP_WR_DIS     -9       write access to EEPROM denied
   SPC_BAD_PAR_ID     -10      unknown SPC parameter id
   SPC_BAD_PAR_VAL    -11      wrong SPC parameter value
   SPC_HARD_TEST      -12      SPC hardware test error
   SPC_BAD_PARA1      -13      wrong value of 1st function parameter
   SPC_BAD_PARA2      -14      wrong value of 2nd function parameter
   SPC_BAD_PARA3      -15      wrong value of 3rd function parameter
   SPC_BAD_PARA4      -16      wrong value of 4th function parameter
   SPC_BAD_PARA5      -17      wrong value of 5th function parameter
   SPC_BAD_PARA6      -18      wrong value of 6th function parameter
   SPC_BAD_PARA7      -19      wrong value of 7th function parameter
   SPC_CANT_ARM       -20      cannot arm SPC module
   SPC_CANT_STOP      -21      cannot stop SPC module
   SPC_INV_REPT       -22      invalid contents of repeat timer
   SPC_NO_SEQ         -23      sequencer does not exist on the module
   SPC_SEQ_RUN        -24      cannot execute function when sequencer is running
   SPC_FILL_TOUT      -25      timeout during filling SPC memory
   SPC_BAD_FUNC       -26      function not available for the module or current mode
   SPC_WINDRV_ERR     -27      Error in WinDriver
   SPC_NOT_INIT       -28      SPC is not yet initialized or unknown module type
   SPC_ERR_ID         -29      Error ID is out of range
   SPC_RATES_NOT_RDY  -30      Rates values not ready yet
   SPC_NO_ACT_MOD     -31      No active modules - nothing was initialized
   SPC_MOD_NO         -32      module number out of range
   SPC_NOT_ACTIVE     -33      can't execute function - module not active
   SPC_IN_USE         -34      Cannot initialize - SPC module already in use
   SPC_WINDRV_VER     -35      Incorrect WinDriver version
   SPC_DMA_ERR        -36      DMA Transfer Error
   SPC_WRONG_LICENSE  -37      Corrupted license key
   SPC_WRITE_STR      -38      can't write string to the file
   SPC_MAX_STREAM     -39      cannot open more than 8 streams of photon files
   SPC_XILINX_ERR     -40      Xilinx chip not configured - reduced SPC functionality
   SPC_DET_NFOUND     -41      Cannot find detector's Xilinx file
   SPC_FIRMWARE_VER   -42      Incorrect firmware version of DPC module
   SPC_NO_LICENSE     -43      License key not read from registry
   SPC_LICENSE_NOT_VALID -44   License is not valid for SPCM DLL
   SPC_LICENSE_DATE_EXP  -45   License date expired
   SPC_DEEP_CHKSUM     -46     wrong EEPROM checksum on DITH board
   SPC_DEEPROM_READ    -47     error during reading EEPROM on DITH board
   SPC_DEEPROM_WRITE   -48     error during writing to EEPROM on DITH board
   SPC_RAM_BUSY        -49     DPC RAM used by FW - cannot be accessed by SW
   SPC_STR_TYPE        -50     function not available for this stream type ( buffers or files)
   SPC_STR_SIZE        -51     max stream size reached ( allocated buffers) for buffered streams
   SPC_STR_BUF_NO      -52     max no of buffers reached ( 32k) for buffered streams
   SPC_STR_NO_START    -53     start condition for getting photons from the stream not reached
   SPC_STR_NO_STOP     -54     stop condition to finish taking photons from the stream not reached
*/



/*- SPC parameters IDs ------------------------------------------------------*/

#define  SPC_PARAMETERS_KEYWORDS \
{ \
  keyword (CFD_LIMIT_LOW), \
  keyword (CFD_LIMIT_HIGH), \
  keyword (CFD_ZC_LEVEL), \
  keyword (CFD_HOLDOFF), \
  keyword (SYNC_ZC_LEVEL), \
  keyword (SYNC_FREQ_DIV), \
  keyword (SYNC_HOLDOFF), \
  keyword (SYNC_THRESHOLD), \
  keyword (TAC_RANGE), \
  keyword (TAC_GAIN), \
  keyword (TAC_OFFSET), \
  keyword (TAC_LIMIT_LOW), \
  keyword (TAC_LIMIT_HIGH), \
  keyword (ADC_RESOLUTION), \
  keyword (EXT_LATCH_DELAY), \
  keyword (COLLECT_TIME), \
  keyword (DISPLAY_TIME), \
  keyword (REPEAT_TIME), \
  keyword (STOP_ON_TIME), \
  keyword (STOP_ON_OVFL), \
  keyword (DITHER_RANGE), \
  keyword (COUNT_INCR), \
  keyword (MEM_BANK), \
  keyword (DEAD_TIME_COMP), \
  keyword (SCAN_CONTROL), \
  keyword (ROUTING_MODE), \
  keyword (TAC_ENABLE_HOLD), \
  keyword (MODE), \
  keyword (SCAN_SIZE_X), \
  keyword (SCAN_SIZE_Y), \
  keyword (SCAN_ROUT_X), \
  keyword (SCAN_ROUT_Y), \
  keyword (SCAN_POLARITY), \
  keyword (SCAN_FLYBACK), \
  keyword (SCAN_BORDERS), \
  keyword (PIXEL_TIME), \
  keyword (PIXEL_CLOCK), \
  keyword (LINE_COMPRESSION), \
  keyword (TRIGGER), \
  keyword (EXT_PIXCLK_DIV), \
  keyword (RATE_COUNT_TIME), \
  keyword (MACRO_TIME_CLK), \
  keyword (ADD_SELECT), \
  keyword (ADC_ZOOM), \
  keyword (XY_GAIN), \
  keyword (IMG_SIZE_X), \
  keyword (IMG_SIZE_Y), \
  keyword (IMG_ROUT_X), \
  keyword (IMG_ROUT_Y), \
  keyword (MASTER_CLOCK), \
  keyword (ADC_SAMPLE_DELAY), \
  keyword (DETECTOR_TYPE), \
  keyword (X_AXIS_TYPE), \
  keyword (CHAN_ENABLE), \
  keyword (CHAN_SLOPE), \
  keyword (CHAN_SPEC_NO), \
};                                   




#ifdef keyword
#undef keyword  /* prevents redefinition warning */
#endif

#define keyword(key) key

		 // new parameters defines for DPC-230 module - CFD1..4 TH or ZC
#define CFD_TH1   CFD_LIMIT_LOW    
#define CFD_TH2   CFD_LIMIT_HIGH    
#define CFD_TH3   CFD_ZC_LEVEL    
#define CFD_TH4   CFD_HOLDOFF    
#define CFD_ZC1   SYNC_ZC_LEVEL    
#define CFD_ZC2   SYNC_HOLDOFF    
#define CFD_ZC3   SYNC_THRESHOLD    
#define CFD_ZC4   TAC_LIMIT_HIGH    

		 // new parameters defines for DPC-590 module - SYNC_FREQ
#define SYNC_FREQ TAC_LIMIT_LOW


enum spcdll_error_enum     SPCDLL_ERROR_KEYWORDS
	enum spc_parameters_enum   SPC_PARAMETERS_KEYWORDS


	/* rate values structure */
	typedef struct _rate_values
{
	float sync_rate;   // for DPC-230 - total photons rate in TDC1
	float cfd_rate;    // for DPC-230 - total photons rate in TDC2
	float tac_rate;    // for DPC-230 - not used
	float adc_rate;    // for DPC-230 - not used

} rate_values;


typedef struct
{
	float tdc1_rate[8];   //  photons rates for all TDC1 channels
	float tdc2_rate[8];   //  photons rates for all TDC2 channels
	float sync_rate;
	float tdc1_total;     //  sum of rates for all TDC1 active channels
	float tdc2_total;     //  sum of rates for all TDC2 active channels

} rate_values_dpc;    // DPC330 rates

#endif     // SPC_DEFINITIONS


/*---------------------------------------------------------------------------*/

	  /* correct SPC modules id values - tested in SPC_test_id */
#define SPC400_ID               0x8

	  /* possible modes of DLL operation - returned from SPC_set(get)_mode */
#define SPC_HARD             0      /* hardware mode */
#define SPC_SIMUL600         600    /* simulation mode of SPC600 module */
#define SPC_SIMUL630         630    /* simulation mode of SPC630 module */
#define SPC_SIMUL700         700    /* simulation mode of SPC700 module */
#define SPC_SIMUL730         730    /* simulation mode of SPC730 module */
#define SPC_SIMUL130         130    /* simulation mode of SPC130 module */
#define SPC_SIMUL830         830    /* simulation mode of SPC830 module */
#define SPC_SIMUL140         140    /* simulation mode of SPC140 module */
#define SPC_SIMUL930         930    /* simulation mode of SPC930 module */
#define DPC_SIMUL230         230    /* simulation mode of DPC230 module */
#define SPC_SIMUL150         150    /* simulation mode of SPC150 module */
#define SPC_SIMUL151         151    /* simulation mode of SPC150N module */
#define SPC_SIMUL152         152    /* simulation mode of SPC150NX module */
#define SPC_SIMUL131         131    /* simulation mode of SPC130EM module */
#define SPC_SIMUL132         132    /* simulation mode of SPC130EMN module */
#define SPC_SIMUL160         160    /* simulation mode of SPC160 module */
#define SPC_SIMUL161         161    /* simulation mode of SPC160X module */
#define SPC_SIMUL162         162    /* simulation mode of SPC160PCIE module */
#define DPC_SIMUL330         330    /* simulation mode of DPC330 module */
#define DPC_SIMUL590         590    /* simulation mode of DPC590 module */

	/* supported module types  - returned value from SPC_test_id */
#define M_WRONG_TYPE  -1
#define M_SPC600    600   /* PCI version of 400 + 401 + 402 */  
#define M_SPC630    630   /* PCI version of 430 + 431 + 432 */  
#define M_SPC700    700   /* PCI version of 500 + 505 + 506 */  
#define M_SPC730    730   /* PCI version of 530 + 535 + 536 */  
#define M_SPC130    130   
#define M_SPC830    830   
#define M_SPC140    140   
#define M_SPC930    930   
#define M_DPC230    230   
#define M_SPC150    150   
#define M_SPC151    151    // SPC-150N = SPC-150 with faster discriminators and reduced timing wobble
#define M_SPC152    152    // SPC-150NX = SPC-150N with changed TAC range ( instead of 50-5000ns):
						   //       2 variants: SPC-150NX 25-2500ns and SPC-150NX-12 12.5-1250ns
#define M_SPC131    131    // SPC-130-EM  = SPC130 with extended memory ( also known as SPC131 )
#define M_SPC132    132    // SPC-130-EMN = SPC130 with extended memory, 
						   //      faster discriminators and reduced timing wobble ( also known as SPC132 )
#define M_SPC160    160    // SPC-160 - Special module for optical tomography
#define M_SPC161    161    // SPC-160X = SPC-160 with TAC range 25-2500ns ( instead of 50-5000ns)
#define M_SPC162    162    // SPC-160PCIE = SPC-160 on PCI-EX bus
#define M_DPC330    330    // DPC330 - Special version of DPC230
#define M_DPC590    590    // DPC590 - Special version of DPC230

		 /* masks for SPC module state  - function SPC_test_state */
#define SPC_OVERFL       0x1     /* stopped on overflow */
#define SPC_OVERFLOW     0x2     /* overflow occured */
#define SPC_TIME_OVER    0x4     /* stopped on expiration of collection timer */
#define SPC_COLTIM_OVER  0x8     /* collection timer expired */
#define SPC_CMD_STOP     0x10    /* stopped on user command */
#define SPC_ARMED        0x80    /* measurement in progress (current bank) */
#define SPC_REPTIM_OVER  0x20    /* repeat timer expired */
#define SPC_COLTIM_2OVER 0x100   /* second overflow of collection timer */
#define SPC_REPTIM_2OVER 0x200   /* second overflow of repeat timer */
		 /* masks valid only for modules SPC130, SPC6x0 */
#define SPC_SEQ_GAP      0x40    /* Sequencer is waiting for other bank to be armed */
	 /* masks valid only for modules SPC13x, SPC6x0, SPC830, SPC140, SPC930, SPC15x, SPC16x
				 in normal modes when sequencer is enabled */
#define SPC_FOVFL        0x400   /* Fifo overflow,data lost, fifo modes only */
#define SPC_FEMPTY       0x800   /* Fifo empty, fifo modes only */

				 /* masks valid only for SPC7x0, SPC830, SPC140, SPC930, SPC15x, SPC131(2), SPC16x modules */
#define SPC_FBRDY        0x800   /* Flow back of scan finished, scan modes only */
#define SPC_SCRDY        0x400   /* Scan ready (data can be read ), scan modes only */
#define SPC_MEASURE      0x40    /* Measurement active =
									  no margin, no wait for trigger, armed */


#define SPC_WAIT_TRG     0x1000   /* wait for trigger */
#define SPC_HFILL_NRDY   0x8000   /* hardware fill not finished */

									  /* masks valid only for SPC140, SPC930, SPC15x, SPC131(2), SPC16x modules */
#define SPC_SEQ_STOP     0x4000   /* disarmed (measurement stopped) by sequencer */
#define SPC_SEQ_GAP150   0x2000  /* SPC15x, SPC16x, SPC131(2) - Sequencer is waiting for other bank to be armed
										normal and Scan In modes when sequencer is enabled */
										// mask for SPC140, SPC830, SPC15x, SPC16x, DPC230 modules ( in FIFO IMAGE mode )
#define SPC_WAIT_FR      0x2000  /* FIFO IMAGE measurement waits for the frame signal to stop */

		 /* masks valid only for DPC230 modules */
#define SPC_FOVFL2       0x800   /* TDC 2 Fifo overflow,data lost */
#define SPC_FOVFL1       0x400   /* TDC 1 Fifo overflow,data lost */
#define SPC_FEMPTY2      0x200   /* TDC 2 Fifo empty */
#define SPC_FEMPTY1      0x100   /* TDC 1 Fifo empty */
#define SPC_ARMED1       0x80    // TDC 1 armed   
#define SPC_ARMED2       0x4000  // TDC 2 armed   
#define SPC_CTIM_OVER2   0x20    // collection timer of TDC 2 expired   
#define SPC_CTIM_OVER1   0x8     // collection timer of TDC 1 expired   
		 /*
		 other flags valid for DPC230 and defined above :
			SPC_MEASURE, SPC_TIME_OVER, SPC_WAIT_TRG , SPC_WAIT_FR,
		 */

		 /* sequencer state bits - returned from function SPC_get_sequencer_state  */
#define SPC_SEQ_ENABLE   0x1  /* sequencer is enabled */
#define SPC_SEQ_RUNNING  0x2  /* sequencer is running */
#define SPC_SEQ_GAP_BANK 0x4  /* sequencer is waiting for other bank to be armed */


	/*  initialisation error codes -
		   - possible return values of function SPC_get_init_status */
#define INIT_SPC_OK                    0
#define INIT_SPC_NOT_DONE             -1 /* init not done */
#define INIT_SPC_WRONG_EEP_CHKSUM     -2 /* wrong EEPROM checksum */
#define INIT_SPC_WRONG_MOD_ID         -3 /* wrong module identification code */
#define INIT_SPC_HARD_TEST_ERR        -4 /* hardware test failed */
#define INIT_SPC_CANT_OPEN_PCI_CARD   -5 /* cannot open PCI card */
#define INIT_SPC_MOD_IN_USE           -6 /* module in use - cannot initialise */
#define INIT_SPC_WINDRVR_VER          -7 /* incorrect WinDriver version */
#define INIT_SPC_WRONG_LICENSE        -8 /* corrupted license key */
#define INIT_SPC_FIRMWARE_VER         -9 /* incorrect firmware version of DPC/SPC module */
#define INIT_SPC_NO_LICENSE          -10 /* license key not read from registry */
#define INIT_SPC_LICENSE_NOT_VALID   -11 /* license is not valid for SPCM DLL */
#define INIT_SPC_LICENSE_DATE_EXP    -12 /* license date expired */

#define INIT_SPC_XILINX_ERR           -100  // only for SPC-930 module
		   // Xilinx chip configuration error -1xx - where xx = Xilinx error code   
		   //   SPC has reduced functionality, when Xilinx chip is not configured





			  /* mode values for SPC7xx, 830, 930 and 140, 15x, 16x, 130-EM(N) ( 131(2)) */

#define ROUT_IN   0           
#define ROUT_OUT  1  // for 830, 930, 140, 131(2) , 15x & 16x- FIFO mode         
#define SCAN_IN   2           
#define SCAN_OUT  3           

#define FIFO_32M  5    // 32 bits fifo type with markers - only SPC140, 15x, 16x & 830
					   //  for Fifo Imaging - Markers are used as scan clocks

		 /* mode values for SPC6xx */

#define NORMAL    0           
#define WIDE      1    // not implemented
#define FIFO_48   2    // 48 bits fifo type of SPC6x0       
#define FIFO_32   3    // 32 bits fifo type of SPC6x0        

		 /* mode values for SPC130 */

#define NORMAL    0           
#define WIDE      1    // not implemented
#define FIFO130   2           

		 /* additional mode values for SPC930 */

#define CAMERA_MODE   4   // camera mode

		 /* mode values for DPC230 */
#define FIFO_TCSPC       6   // TCSPC FIFO mode of DPC230
#define FIFO_TCSPC_IMG   7   // TCSPC FIFO Imaging mode of DPC230
#define FIFO_ABS_TIME    8   // Absolute Time FIFO mode of DPC230
#define FIFO_ABS_IMG     9   // Absolute Time FIFO Imaging mode of DPC230
#define HW_TCSPC        10   // Hardware histogram TCSPC mode of DPC330
#define HW_MULTISCALER  12   // Hardware histogram multiscaler mode of DPC330


						//  fifo types definitions
						// in the moment there are no differences in data format
						//    between following 4 fifo types
#define FIFO_130    4   // fifo type of SPC130
#define FIFO_830    5   // fifo type of SPC830 , 930
#define FIFO_140    6   // fifo type of SPC140
#define FIFO_150    7   // fifo type of SPC15x, 131(2), 16x

#define FIFO_D230   8   // fifo type of DPC230
#define FIFO_IMG    9   // fifo image type of SPC140,15x,16x, 830 - like FIFO_140, 
						//      but with marker photons
#define FIFO_D590   10  // fifo type of DPC590

/*---------------------------------------------------------------------------*/

			  /* structure for memory configuration parameters */
typedef struct _SPCMemConfig {
	long   max_block_no;    /* total number of blocks per memory bank */
	long   blocks_per_frame; /* no of blocks per frame */
	long   frames_per_page;  /* no of frames per page */
	long   maxpage;         /* max number of pages to use in a measurement */
	long   block_length;    /* no of 16-bits(32-bits for DPC modules) words per one block */
}SPCMemConfig;

typedef struct _SPCMemConfig *SPCMemConfigType;

typedef struct _SPCdata {    /* structure for library data  */
	unsigned short base_adr;  /* base I/O address on PCI bus */
	short          init;      /* set to initialisation result code */
	float cfd_limit_low;   /* for SPCx3x(140,15x,131(2),16x) -500 .. 0mV ,for SPCx0x 5 .. 80mV
							  for DPC230 = CFD_TH1 threshold of CFD1 -510 ..0 mV */
	float cfd_limit_high;  /* 5 ..80 mV, default 80 mV , not for SPC130,140,15x,131(2),16x,930
							  for DPC230 = CFD_TH2 threshold of CFD2 -510 ..0 mV */
	float cfd_zc_level;    /* SPCx3x(140,15x,131(2),16x) -96 .. 96mV, SPCx0x -10 .. 10mV
							  for DPC230 = CFD_TH3 threshold of CFD3 -510 ..0 mV */
	float cfd_holdoff;     /* SPCx0x: 5 .. 20 ns, other modules: no influence
							  for DPC230 = CFD_TH4 threshold of CFD4 -510 ..0 mV */
	float sync_zc_level;   /* SPCx3x(140,15x,131(2),16x): -96 .. 96mV, SPCx0x: -10..10mV
							  for DPC230 = CFD_ZC1 Zero Cross level of CFD1 -96 ..96 mV */
	float sync_holdoff;    /* 4 .. 16 ns ( SPC130,140,15x,131(2),16x,930: no influence)
							  for DPC230 = CFD_ZC2 Zero Cross level of CFD2 -96 ..96 mV */
	float sync_threshold;  /* SPCx3x(140,15x,131(2),16x): -500 .. -20mV, SPCx0x: no influence
							  for DPC230 = CFD_ZC3 Zero Cross level of CFD3 -96 ..96 mV */
	float tac_range;       /* 50 .. 5000 ns, for SPC161 25 .. 2500,
								for SPC150NX 25 .. 2500, for SPC150NX-12 12.5 ..  1250,
								for DPC230 = DPC range in TCSPC and Multiscaler mode
									  0.16461 .. 1e7 ns */
	short sync_freq_div;   /* 1,2,4,8,16 ( SPC130,140,15x,131(2),16x,930, DPC230 : 1,2,4) */
	short tac_gain;        /* 1 .. 15    not for DPC230 */
	float tac_offset;      /* 0 .. 100%, for SPC16x,150N(151),132  0 .. 50%
					   for DPC230 = TDC offset in TCSPC and Multiscaler mode -100 .. 100% */
	float tac_limit_low;   /* 0 .. 100%  not for DPC230 */
						   // for DPC590 = SYNC_FREQ  1 .. 100 MHz 
	float tac_limit_high;  /* 0 .. 100%
							  for DPC230 = CFD_ZC4 Zero Cross level of CFD4 -96 ..96 mV */
	short adc_resolution;  /* 6,8,10,12 bits, default 10 ,
							  (additionally 0,2,4 bits for SPC830,140,15x,131(2),16x,930 )
					   for DPC230 = no of points of decay curve in TCSPC and Multiscaler mode
											0,2,4,6,8,10,12,14,16  bits */
	short ext_latch_delay; /* 0 ..255 ns, (SPC130, DPC230 : no influence) */
						   /* SPC140,15x,131(2),16x,930: only values 0,10,20,30,40,50 ns are possible */
	float collect_time;    /* 1e-7 .. 100000s , default 0.01s */
	float display_time;    /* 0.1 .. 100000s , default 1.0s, obsolete, not used in DLL */
	float repeat_time;     /* 1e-7 .. 100000s , default 10.0s, not for DPC230 */
	short stop_on_time;    /* 1 (stop) or 0 (no stop) */
	short stop_on_ovfl;    /* 1 (stop) or 0 (no stop), not for DPC230  */
	short dither_range;    /* possible values - 0, 32,   64,   128,  256
								 have meaning:  0, 1/64, 1/32, 1/16, 1/8
								 value 256 (1/8) only for SPC6x0,7x0, 830
								 not for DPC230 */
	short count_incr;      /* 1 .. 255, not for DPC230  */
	short mem_bank;        /* for SPC130,600,630, 15x,131(2),16x :  0 , 1 , default 0
							  other SPC modules: always 0
							  DPC230 : bit 1 - DPC 1 active, bit 2 - DPC 2 active
							*/
	short dead_time_comp;  /* 0 (off) or 1 (on), default 1, not for DPC230   */
	unsigned short scan_control; /* SPC505(535,506,536) scanning(routing) control word,
									other SPC modules always 0 */
	unsigned short routing_mode;     /* DPC230  bits 0-7 - control bits
							   SPC15x,830,140,131(2),16x)
								  - bits 6 - in FIFO_32M mode,
											 = 0 (default) Marker 3 not used,
											 = 1 waiting for Marker 3 to start collection timer,
												  ( used in accumulated Mosaic measurements)
								  - bits 7 - in FIFO_32M mode,
											 = 0 (default) Frame pulses on Marker 2,
											 = 1 Frame pulses on Marker 3,
								  - bits 8 - 11 - enable(1)/disable(0), default 0
												of recording Markers 0-3 entries in FIFO mode
								  - bits 12 - 15 - active edge 0(falling), 1(rising), default 0
												 of Markers 0-3 in FIFO mode
							   other SPC modules - not used  */
	float tac_enable_hold;  /* SPC230 10.0 .. 265.0 ns - duration of TAC enable pulse ,
							   DPC230 - macro time clock in ps, default 82.305 ps,
							   other SPC modules always 0 */
	short pci_card_no;      /* module no on PCI bus (0-7)  */
	unsigned short mode;    /* for SPC7x0      , default 0
								  0 - normal operation (routing in),
								  1 - block address out, 2 -  Scan In, 3 - Scan Out
							   for SPC6x0      , default 0
								  0 - normal operation (routing in)
								  2 - FIFO mode 48 bits, 3 - FIFO mode 32 bits
							   for SPC130      , default 0
								  0 - normal operation (routing in)
								  2 - FIFO mode 32 bits
							   for SPC140 , default 0
								  0 - normal operation (routing in)
								  1 - FIFO mode 32 bits, 2 -  Scan In, 3 - Scan Out
								  5 - FIFO_mode 32 bits with markers ( FIFO_32M ), with FPGA v. > B0
							   for SPC15x,16x, default 0
								  0 - normal operation (routing in)
								  1 - FIFO mode 32 bits, 2 -  Scan In, 3 - Scan Out
								  5 - FIFO_mode 32 bits with markers ( FIFO_32M )
							   for SPC830,930 , default 0
								  0 - normal operation (routing in)
								  1 - FIFO mode 32 bits, 2 -  Scan In, 3 - Scan Out
								  4 - Camera mode ( only SPC930 )
								  5 - FIFO_mode 32 bits with markers ( FIFO_32M ),
												  SPC830 with FPGA v. > C0
							   for DPC230 , default 8
								  6 - TCSPC FIFO
								  7 - TCSPC FIFO Image mode
								  8 - Absolute Time FIFO mode
								  9 - Absolute Time FIFO Image mode
							   for SPC131(2) , default 0
								  0 - normal operation (routing in)
								  1 - FIFO mode 32 bits
								*/
	unsigned long scan_size_x;  /* for SPC7x0,830,140,15x,16x,930 modules in scanning modes 1 .. 65536,
										   default 1, not for DPC230  */
	unsigned long scan_size_y;  /* for SPC7x0,830,140,15x,16x,930 modules in scanning modes 1 .. 65536,
										   default 1, not for DPC230  */
	unsigned long scan_rout_x;  /* number of X routing channels in Scan In & Scan Out modes, not for DPC230
									for SPC7x0,830,140,15x,16x,930 modules
								 1 .. 128, ( SPC7x0,830 ), 1 .. 16 (SPC140,15x,16x,930), default 1 */
	unsigned long scan_rout_y;  /* number of Y routing channels in Scan In & Scan Out modes, not for DPC230
									for SPC7x0,830,140,15x,16x, 930 modules
								 1 .. 128, ( SPC7x0,830 ), 1 .. 16 (SPC140,15x,16x,930), default 1 */
								 /* INT(log2(scan_size_x)) + INT(log2(scan_size_y)) +
									INT(log2(scan_rout_x)) + INT(log2(scan_rout_y)) <= max number of scanning bits
													max number of scanning bits depends on current adc_resolution:
															12 (10 for SPC7x0,140,15x,16x)   -              12
															14 (12 for SPC7x0,140,15x,16x)   -              10
															16 (14 for SPC7x0,140,15x,16x)   -               8
															18 (16 for SPC7x0,140,15x,16x)   -               6
															20 (18 for SPC140,15x,16x)       -               4
															22 (20 for SPC140,15x,16x)       -               2
															24 (22 for SPC140,15x,16x)       -               0
															*/
	unsigned long  scan_flyback;   /* for SPC7x0,830,140,15x,16x,930 modules in Scan Out or Rout Out mode,
										   default & minimum = 1, not for DPC230  */
										   /* bits 15-0  Flyback X in number of pixels
												bits 31-16 Flyback Y in number of lines */
	unsigned long  scan_borders;   /* for SPC7x0,830,140,15x,16x,930 modules in Scan In mode,
										   default 0, not for DPC230  */
										   /* bits 15-0  Upper boarder, bits 31-16 Left boarder */
	unsigned short scan_polarity;    /* for SPC7x0,830,140,15x,16x,930 modules in scanning modes,
										   default 0, not for DPC230  */
										   /* bit 0 - polarity of HSYNC (Line), bit 1 - polarity of VSYNC (Frame),
											  bit 2 - pixel clock polarity
											  bit = 0 - falling edge(active low)
											  bit = 1 - rising  edge(active high)
											for SPC140,15x,16x,830 in FIFO_32M mode
											  bit = 8 - HSYNC (Line) marker disabled (1) or enabled (0, default )
														  when disabled, line marker will not appear in FIFO photons stream */
	unsigned short pixel_clock;   /* for SPC7x0,830,140,15x,16x,930 modules in Scan In mode, or DPC230 in Image modes
							   pixel clock source, 0 - internal,1 - external, default 0
				   for SPC140,15x,16x,830 in FIFO_32M mode it disables/enables pixel markers
												   in photons stream */
	unsigned short line_compression;   /* line compression factor for SPC7x0,830,140,15x,16x,930 modules
									 in Scan In mode,   1,2,4,8,16,32,64,128, default 1*/
	unsigned short trigger;    /* external trigger condition -
			 bits 1 & 0 mean :   00 - ( value 0 ) none(default),
								 01 - ( value 1 ) active low,
								 10 - ( value 2 ) active high
		  when sequencer is enabled on SPC130,6x0,15x,16x,131(2) modules additionally
			bits 9 & 8 of the value mean:
			 00 - trigger only at the start of the sequence,
			 01 ( 100 hex, 256 decimal ) - trigger on each bank
			 11 ( 300 hex, 768 decimal ) - trigger on each curve in the bank
		  for SPC15x,16x, 131(2), 140 and SPC130 (FPGA v. > C0) multi-module configuration
				 bits 13 & 12 of the value mean:
			 x0 - module does not use trigger bus ( trigger defined via bits 0-1),
			 01 ( 1000 hex, 4096 decimal ) - module uses trigger bus as slave
											  ( waits for the trigger on master),
			 11 ( 3000 hex, 12288 decimal ) - module uses trigger bus as master
									( trigger defined via bits 0-1),
									( only one module can be the master )
			*/
	float pixel_time;    /* pixel time in sec for SPC7x0,830,140,15x,16x,930 modules in Scan In mode,
								50e-9 .. 1.0 , default 200e-9 */
	unsigned long ext_pixclk_div;  /* divider of external pixel clock for SPC7x0,830,140,15x,16x modules
								  in Scan In mode, 1 .. 0x3fe, default 1*/
	float rate_count_time;    /* rate counting time in sec  default 1.0 sec
								for SPC130,830,930,15x,16x,131(2) can be : 1.0, 250ms, 100ms, 50ms
								for SPC140 fixed to 50ms
								for DPC230 - 1.0sec,
											 0.0 - don't count rate outside the measurement, */
	short macro_time_clk;     /*  macro time clock definition for SPC130,140,15x,16x,131(2),830,930 in FIFO mode
								for SPC130, SPC140,15x,16x,131(2):
									0 - 50ns (default), 25ns for SPC15x,16x,131(2) & 140 with FPGA v. > B0 ,
									1 - SYNC freq., 2 - 1/2 SYNC freq.,
									3 - 1/4 SYNC freq., 4 - 1/8 SYNC freq.
								for SPC830:
									0 - 50ns (default), 1 - SYNC freq.,
								for SPC930:
									0 - 50ns (default), 1 - SYNC freq., 2 - 1/2 SYNC freq.*/
	short add_select;     /* selects ADD signal source for all modules except SPC930 & DPC230 :
							  0 - internal (ADD only), 1 - external */
	short test_eep;        /* test EEPROM checksum or not  */
	short adc_zoom;     /* selects ADC zoom level for module SPC830,140,15x,16x,131(2),930 default 0
							 bit 4 = 0(1) - zoom off(on ),
							 bits 0 - 3 zoom level =
								 0 - zoom of the 1st 1/16th of ADC range,
								15 - zoom of the 16th 1/16th of ADC range */
	unsigned long img_size_x;  /* image X size ( SPC140,15x,16x,830 in FIFO_32M, SPC930 in Camera mode ),
										1 .. 1024, default 1 */
	unsigned long img_size_y;  /* image Y size ( SPC140,15x,16x,830 in FIFO_32M, SPC930 in Camera mode ),
								  actually equal to img_size_x ( quadratic image ) */
	unsigned long img_rout_x;  /* no of X routing channels ( SPC140,15x,16x,830 in FIFO_32M, SPC930 in Camera mode ),
										1 .. 16, default 1 */
	unsigned long img_rout_y;  /* no of Y routing channels ( SPC140,15x,16x,830 in FIFO_32M, SPC930 in Camera mode ),
										1 .. 16, default 1 */
	short xy_gain;      /* selects gain for XY ADCs for module SPC930, 1,2,4, default 1 */
	short master_clock;  /*  use Master Clock( 1 ) or not ( 0 ), default 0,
								 only for SPC140,15x,16x,131(2) multi-module configuration
						  - value 2 (when read) means Master Clock state was set by other application
									and cannot be changed */
	short adc_sample_delay; /* ADC's sample delay, only for module SPC930
							   0,10,20,30,40,50 ns (default 0 ) */
	short detector_type;    /*  for module SPC930 :
							  detector type used in Camera mode, 1 .. 9899, default 1,
						normally recognised automatically from the corresponding .bit file
								 1 - Hamamatsu Resistive Anode 4 channels detector
								 2 - Wedge & Strip 3 channels detector
						 for module DPC230 :
							type of active inputs : bit 1 - TDC1, bit 2 - TDC2,
							   bit value 0 , CFD inputs active,
							   bit value 1 , TTL inputs active */

	unsigned long  chan_enable;   /* for module DPC230/330 - enable(1)/disable(0) input channels
								  bits 0-7   - en/disable TTL channel 0-7 in TDC1
								  bits 8-9   - en/disable CFD channel 0-1 in TDC1
								  bits 12-19 - en/disable TTL channel 0-7 in TDC2
								  bits 20-21 - en/disable CFD channel 0-1 in TDC2
								  */
	unsigned long  chan_slope;   /* for module DPC230 - active slope of input channels
									 1 - rising, 0 - falling edge active
								  bits 0-7   - slope of TTL channel 0-7 in TDC1
								  bits 8-9   - slope of CFD channel 0-1 in TDC1
								  bits 12-19 - slope of TTL channel 0-7 in TDC2
								  bits 20-21 - slope of CFD channel 0-1 in TDC2
								  */
	unsigned long  chan_spec_no;     /* for module DPC230/330 - channel numbers of special inputs
													 default 0x8813 in imaging modes
				bits 0-4 - reference chan. no ( TCSPC and Multiscaler modes)
						  default = 19, value:
						 0-1 CFD chan. 0-1 of TDC1,   2-9 TTL chan. 0-7 of TDC1
					   10-11 CFD chan. 0-1 of TDC2, 12-19 TTL chan. 0-7 of TDC2
				bits  8-10 - frame clock TTL chan. no ( imaging modes ) 0-7, default 0
				bits 11-13 - line  clock TTL chan. no ( imaging modes ) 0-7, default 1
				bits 14-16 - pixel clock TTL chan. no ( imaging modes ) 0-7, default 2
				bit  17    - TDC no for pixel, line, frame clocks ( imaging modes )
								0 = TDC1, 1 = TDC2, default 0
				bits 18-19 - not used
				bits 20-23 - active channels of TDC1 for DPC-330 Hardware Histogram modes
				bits 24-27 - active channels of TDC2 for DPC-330 Hardware Histogram modes
				bits 28-31 - not used
				*/
	short x_axis_type;      /* X axis representation, only for module SPC930
								 0 - time (default ), 1 - ADC1 Voltage,
								 2 - ADC2 Voltage, 3 - ADC3 Voltage, 4 - ADC4 Voltage
							 */
}SPCdata;

/* structure for module info */
typedef struct _SPCModInfo {
	short    module_type;      /* module type */
	short    bus_number;       /* PCI bus number */
	short    slot_number;      /* slot number on PCI bus */
	short    in_use;           /* -1 used and locked by other application, 0 - not used
								   1 - in use  */
	short    init;             /* set to initialisation result code */
	unsigned short base_adr;   /* base I/O address - not valid for DPC modules
														and 64-bit operating systems*/
}SPCModInfo;


/* structure containing SPC adjust parameters stored in EEPROM  - not for DPC230*/
typedef struct _SPC_Adjust_Para {
	short vrt1;     //   for SPC15x,16x,131(2) RESET Duration
	short vrt2;     //   for SPC15x,16x,131(2) SADC  Duration
	short vrt3;     //   for SPC15x,16x,131(2) SADC  Delay   
	short dith_g;
	float gain_1;
	float gain_2;
	float gain_4;
	float gain_8;
	float tac_r0;
	float tac_r1;
	float tac_r2;
	float tac_r4;
	float tac_r8;
	short sync_div;
}SPC_Adjust_Para;



/* EEPROM data structure */

typedef struct _SPC_EEP_Data {  /* structure for SPC module EEPROM data  */
	char module_type[16];        /* SPC module type */
	char serial_no[16];          /* SPC module serial number */
	char date[16];               /* SPC module production date */
	SPC_Adjust_Para adj_para;       /* structure with adjust parameters  - for DPC230 not filled*/
}SPC_EEP_Data;


/* structure for the stream of photon files (.spc ) */
typedef struct {
	short fifo_type;         // Fifo type: 2 (FIFO_48),  3 (FIFO_32), 
							 //            4 (FIFO_130), 5 (FIFO_830), 6 (FIFO_140),
							 //            7 (FIFO_150), 8 (FIFO_D230), 9 (FIFO_IMG)
	short stream_type;       // bit 0 = 1 - stream of BH .spc files ( 1st entry in each file 
							 //                 contains MT clock, flags & rout.chan info)
							 // bit 0 = 0 - no special meaning of the first entry in the file
							 // bit 1 = 1 - stream contains data of DPC1 from DPC-230 module
							 //                                        (only when raw data)
							 // bit 2 = 1 - stream contains data of DPC2 from DPC-230 module 
							 //                                        (only when raw data)
							 // bit 3 = 1/0 ( TTL / CFD ) inputs active for data from DPC-230 module 
							 //                                        (only when raw data)
							 // bit 7 = 1 - stream contains reference channel entries 
							 //              ( Multiscaler or TCSPC mode of DPC module )
							 // bit 8 = 1 - stream contains data from DPC-230 module 
							 //             mt_clock unit is [fs] ( 1e-15 ) instead of ( 1e-10)
							 // bit 9 = 1 - stream contains marker entries ( FIFO_IMG mode )
							 // bit 10 = 1 - stream contains raw data ( diagnostics only )
							 // bit 12 = 1 - stream of photons taken from buffers (not from files)
							 // bit 13 = 1 - only for 'buffered' streams ( bit 12 = 1)
							 //              stream's buffer is freed after extracting all photons from it
	int   mt_clock;          // macro time clock ( from the 1st photon entry ) - 
							 //      unit depends on the stream_type bit 8
							 //            bit 8 = 0   unit is [0.1 ns] ( 1e-10 )
							 //            bit 8 = 1   unit is   [1 fs] ( 1e-15 ) - DPC-230 data
	short rout_chan;         // number of used routing channels ( from the 1st photon entry )
	short what_to_read;      // bitwise definition: 
							 //     bit 0 = 1 read valid photons
							 //     bits 1-5 for SPC modules :
							 //         bit 1 = 1 read invalid photons 
							 //         bit 2 = 1 read markers 0 ( pixel )
							 //         bit 3 = 1 read markers 1 ( line )
							 //         bit 4 = 1 read markers 2 ( frame )
							 //         bit 5 = 1 read markers 3
							 //     bits 1-5 for DPC-230 module read events from special channels
							 //                       ( parameter CHAN_SPEC_NO )
							 //         bit 1     not used yet 
							 //         bit 2 = 1 events in pixel chan. no
							 //         bit 3 = 1 events in line  chan. no
							 //         bit 4 = 1 events in frame chan. no
							 //         bit 5 = 1 events in Reference chan. no
	short no_of_files;       // number of files in stream
	short no_of_ready_files; // number of finished files ( all photons taken from already)
	char  base_name[264];    // base stream name 
	char  cur_name[264];     // current file used to get photons
							 //   made from base_name and cur_no
	short first_no;          // first file/buffer number
	short cur_no;            // current file/buffer number
	int   fifo_overruns;   // current number of Fifo overruns found in the stream
	unsigned __int64  stream_size;  // total stream size in bytes ( all files/buffers)
								//   for buffered stream - this is the size of all currently allocated buffers
								//                         ( without buffers which were already freed ) 
								//   for stream of files - this is the sum of sizes of all stream files
	unsigned __int64  cur_stream_offs; // current offset in stream  in bytes ( all files/buffers)
								//   for buffered stream - includes also buffers which were used and are already freed  
	unsigned __int64  cur_file_offs;   // offset in the current file in bytes
	unsigned __int64   invalid_phot;    // current number of invalid photons found in the stream
	unsigned __int64   read_photons;    // current number of read photons from the stream
	unsigned __int64   read_0_mark;    // current number of read markers 0 ( pixel )
								   //     ( pixel chan. events for DPC-230)   
	unsigned __int64   read_1_mark;    // current number of read markers 1 ( line )
								   //     ( line chan. events for DPC-230)   
	unsigned __int64   read_2_mark;    // current number of read markers 2 ( frame )
								   //     ( frame chan. events for DPC-230)   
	unsigned __int64   read_3_mark;    // current number of read markers 3
								   //     ( reference chan. events for DPC-230) 
	unsigned int   start01_offs;   // Start Offset of the 1st photon ( for DPC230 data )
	short             no_of_buf;   // current number of buffers added to the stream
	short       no_of_ready_buf;   // number of finished buffers ( all photons taken from already)
	unsigned int   cur_buf_offs;   // offset in the current buffer in bytes
	unsigned int   start_OR_mask;     // events on selected channels/markers ( in addition to start_time)
								   //   must happen to start extracting photons, default 0
								   // bits 31-28 - markers bits, 27-0 rout channel appearance bits
	unsigned int   start_AND_mask;     // events on selected channels/markers ( in addition to start_time)
	unsigned int   stop_OR_mask;      // events on selected channels/markers ( in addition to stop_time)
								   //   must happen to stop extracting photons, default 0
								   // bits 31-28 - markers bits, 27-0 rout channel appearance bits
	unsigned int   stop_AND_mask;  // events on selected channels/markers ( in addition to stop_time)
	short          start_found;    // start condition found
	short          stop_reached;   // stop condition reached
	double         start_time;     // min time of the first photon to be extracted, 0(default) - not defined 
	double         stop_time;      // time of the last photon to be extracted, 0(default) - not defined
	double         curr_time;      // current position in the stream (absolute macro time) 
	unsigned int   start_found_chan;  // shows bitwise which channels were found 
								   //   while looking for start of extracting photons
								   // bits 31-28 - markers bits, 27-0 rout channel appearance bits
	unsigned int   stop_found_chan;   // shows bitwise which channels were found 
								   //   while looking for the end of extracting photons
								   // bits 31-28 - markers bits, 27-0 rout channel appearance bits
}PhotStreamInfo;



/* structure for the photon data
	  read from the photons stream using SPC_get_photon function */
typedef struct {
	unsigned long  mtime_lo;   /* macro time clocks low  32 bits */
	unsigned long  mtime_hi;   /* macro time clocks high 32 bits */
	unsigned short micro_time; /* micro time = 4095(255) - ADC value,  0-255 or 0-4095,
									0 for markers entries, not valid for DPC-230 */
	unsigned short rout_chan;  /* routing channel, 0-15,
									(0 - 19 for DPC-230, 0-255 for SPC-6x0 48-bit mode ),
								 0 for markers entries */
	unsigned short flags;      /* photon flags, see defines below  */
}PhotInfo;

typedef struct {
	unsigned __int64  mtime;   /* macro time clocks 64 bits */
	unsigned short micro_time; /* micro time = 4095(255) - ADC value,  0-255 or 0-4095,
									0 for markers entries, not valid for DPC-230 */
	unsigned short rout_chan;  /* routing channel, 0-15,
									(0 - 19 for DPC-230, 0-255 for SPC-6x0 48-bit mode ),
								 0 for markers entries */
	unsigned short flags;      /* photon flags, see defines below  */
}PhotInfo64;


// photon flags bitwise defines
#define INV_PH   1         // Invalid Photon ( not for DPC-230 )    
#define FI_OVR   2         // Fifo Overrun - data lost due to Fifo full
#define MARK_0   0x1000    // not a photon but Marker0
#define MARK_1   0x2000    // not a photon but Marker1
#define MARK_2   0x4000    // not a photon but Marker2
#define MARK_3   0x8000    // not a photon but Marker3
#define P_MARK   MARK_0    // Pixel Event 
#define L_MARK   MARK_1    // Line  Event 
#define F_MARK   MARK_2    // Frame Event 
#define R_MARK   MARK_3    // Reference channel Event ( DPC-230 only )
#define ANY_MARK       (MARK_0 | MARK_1 | MARK_2 | MARK_3)    // any marker
#define NOT_PHOTON     (INV_PH | ANY_MARK)


		 // stream types bitwise
#define BH_STREAM   0x1       // BH stream ( 1st entry in each file 
							  //     contains MT clock, flags & rout.chan info)
#define DPC1_DATA   0x2       // stream with DPC230 module data from DPC1 - only when raw data
#define DPC2_DATA   0x4       // stream with DPC230 module data from DPC2 - only when raw data
#define DPC_TTL     0x8       // stream with DPC230 module data - active inputs TTL - only when raw data
							  //             if no DPC_TTL flag, active inputs were CFD
#define REF_STREAM  0x80      // stream with reference channel entries
#define DPC_STREAM  0x100     // stream with DPC230 module data 
#define MARK_STREAM 0x200     // stream with markers entries ( FIFO_IMG mode )
#define RAW_STREAM  0x400     // stream with raw data ( only BH diagnostic mode )

#define BUF_STREAM  0x1000    // stream input from buffers ( not from files )

#define FREE_BUF_STREAM        0x2000     // stream buffer will be freed automatically 
										  //     after extracting photons from it

#define STREAM_MIN_BUF_SIZE    0x800004    // 8MB default and minimum size of a stream buffer
#define STREAM_MAX_BUF_SIZE    0x4000002   // 64MB max size of a data buffer added to the stream
				 // 256 MB max size of all currently allocated stream buffers for 32bit DLL
#define STREAM_MAX_SIZE32      0x10000002   
				 // 1024 MB max size of all currently allocated stream buffers for 64bit DLL
#define STREAM_MAX_SIZE64      0x40000002   


/*   SPC_LV_.. versions of the functions are prepared especially for usage
		  in LabView environment in 'Call Library' function node to avoid
		  problems with different strings representation in C and LabView
		  when using strings within clusters */

#ifndef LVSTR_DEFINED

#define LVSTR_DEFINED
typedef struct {     /* string definition in LabView */
	int   cnt;              /* number of bytes that follow */
	unsigned char str[1];   /* cnt bytes */
} LVStr, *LVStrPtr, **LVStrHandle;
#endif

/* LabView version of the structure for the stream of photons files (.spc ) */

typedef struct {
	short fifo_type;         // Fifo type: 2 (FIFO_48),  3 (FIFO_32), 
							 //            4 (FIFO_130), 5 (FIFO_830),  6 (FIFO_140),
							 //            7 (FIFO_150), 8 (FIFO_D230), 9 (FIFO_IMG)
	short stream_type;       // bit 0 = 1 - stream of BH .spc files ( 1st entry in each file 
							 //                 contains MT clock, flags & rout.chan info)
							 // bit 0 = 0 - no special meaning of the first entry in the file
							 // bit 1 = 1 - stream contains data of DPC1 from DPC-230 module
							 //                                        (only when raw data)
							 // bit 2 = 1 - stream contains data of DPC2 from DPC-230 module 
							 //                                        (only when raw data)
							 // bit 3 = 1/0 ( TTL / CFD ) inputs active for data from DPC-230 module 
							 //                                        (only when raw data)
							 // bit 8 = 1 - stream contains data from DPC-230 module 
							 //             mt_clock unit is [fs] ( 1e-15 ) instead of ( 1e-10)
							 // bit 9 = 1 - stream contains marker entries ( FIFO_IMG mode )
							 // bit 10 = 1 - stream contains raw data ( diagnostics only )
							 // bit 12 = 1 - stream photons taken from buffers (not from files)
	int   mt_clock;          // macro time clock ( from the 1st photon entry for stream of files ) - 
							 //      unit depends on the stream_type bit 8
							 //            bit 8 = 0   unit is [0.1 ns] ( 1e-10 )
							 //            bit 8 = 1   unit is   [1 fs] ( 1e-15 ) - DPC-230 data
	short rout_chan;         // number of used routing channels ( from the 1st photon entry )
	short what_to_read;      // bitwise definition: 
							 //     bit 0 = 1 read valid photons
							 //     bits 1-5 for SPC modules :
							 //         bit 1 = 1 read invalid photons 
							 //         bit 2 = 1 read markers 0 ( pixel )
							 //         bit 3 = 1 read markers 1 ( line )
							 //         bit 4 = 1 read markers 2 ( frame )
							 //         bit 5 = 1 read markers 3
							 //     bits 1-5 for DPC-230 module read events from special channels
							 //                       ( parameter CHAN_SPEC_NO )
							 //         bit 1     not used yet 
							 //         bit 2 = 1 events in pixel chan. no
							 //         bit 3 = 1 events in line  chan. no
							 //         bit 4 = 1 events in frame chan. no
							 //         bit 5 = 1 events in Reference chan. no
	short no_of_files;       // number of files in stream
	short no_of_ready_files; // number of finished files ( all photons taken from already)
	LVStrHandle  base_name;    // handle to base stream name LabView string
	LVStrHandle  cur_name;     // handle to current file used to get photons LabView string
							 //   made from base_name and cur_no
	short first_no;          // first file number
	short cur_no;            // current file number
	int   fifo_overruns;        // current number of Fifo overruns found in the stream
	unsigned __int64  stream_size;  // total stream size in bytes ( all files/buffers)
								//   for buffered stream - this is the size of all currently allocated buffers
								//                         ( without buffers which were already freed ) 
								//   for stream of files - this is the sum of sizes of all stream files
	unsigned __int64  cur_stream_offs; // current offset in stream  in bytes ( all files/buffers)
								//   for buffered stream - includes also buffers which were used and are already freed  
	unsigned __int64  cur_file_offs;   // offset in the current file in bytes
	unsigned __int64   invalid_phot;    // current number of invalid photons found in the stream
	unsigned __int64   read_photons;    // current number of read photons from the stream
	unsigned __int64   read_0_mark;    // current number of read markers 0 ( pixel )
								   //     ( pixel chan. events for DPC-230)   
	unsigned __int64   read_1_mark;    // current number of read markers 1 ( line )
								   //     ( line chan. events for DPC-230)   
	unsigned __int64   read_2_mark;    // current number of read markers 2 ( frame )
								   //     ( frame chan. events for DPC-230)   
	unsigned __int64   read_3_mark;    // current number of read markers 3
								   //     ( reference chan. events for DPC-230) 
	unsigned int   start01_offs;   // Start Offset of the 1st photon ( for DPC230 data )
	short             no_of_buf;   // current number of buffers added to the stream
	short       no_of_ready_buf;   // number of finished buffers ( all photons taken from already)
	unsigned int   cur_buf_offs;   // offset in the current buffer in bytes
	unsigned int   start_OR_mask;     // events on selected channels/markers ( in addition to start_time)
								   //   must happen to start extracting photons, default 0
								   // bits 31-28 - markers bits, 27-0 rout channel appearance bits
	unsigned int   start_AND_mask;     // events on selected channels/markers ( in addition to start_time)
	unsigned int   stop_OR_mask;      // events on selected channels/markers ( in addition to stop_time)
								   //   must happen to stop extracting photons, default 0
								   // bits 31-28 - markers bits, 27-0 rout channel appearance bits
	unsigned int   stop_AND_mask;  // events on selected channels/markers ( in addition to stop_time)
	short          start_found;    // start condition found
	short          stop_reached;   // stop condition reached
	double         start_time;     // min time of the first photon to be extracted, 0(default) - not defined 
	double         stop_time;      // time of the last photon to be extracted, 0(default) - not defined
	double         curr_time;      // current position in the stream (absolute macro time) 
	unsigned int   start_found_chan;  // shows bitwise which channels were found 
								   //   while looking for start of extracting photons
								   // bits 31-28 - markers bits, 27-0 rout channel appearance bits
	unsigned int   stop_found_chan;   // shows bitwise which channels were found 
								   //   while looking for the end of extracting photons
								   // bits 31-28 - markers bits, 27-0 rout channel appearance bits
}PhotStreamInfo_LV;


#pragma pack( pop )

/*
 CVIDECL means C calling convention - default for C and C++ programs
 DLLSTDCALL means _stdcall calling convention which is required by Visual Basic

 The only difference between SPCstd_.. and SPC_.. functions is calling convention

*/


/*- General functions -------------------------------------------------------*/

typedef short(__cdecl *spc_init)(char* ini_file);
typedef short(__cdecl *spc_get_init_status)(short mod_no);
typedef short(__cdecl *spc_get_parameters)(short mod_no, SPCdata* data);
typedef short(__cdecl *spc_set_parameters)(short mod_no, SPCdata* data);
typedef short(__cdecl *spc_get_parameter)(short mod_no, short par_id, float* value);
typedef short(__cdecl *spc_set_parameter)(short mod_no, short par_id, float value);
typedef short(__cdecl *spc_configure_memory)(short mod_no, short adc_resolution, short no_of_routing_bits, SPCMemConfig* mem_info);
typedef short(__cdecl *spc_fill_memory)(short mod_no, long block, long page, unsigned short fill_value);
typedef short(__cdecl *spc_clear_mom_memory)(short mod_no);
typedef short(__cdecl *spc_prepare_time_gates)(short mod_no, short gates_no, short rchan_no, int *gates, short equal_rchan);
typedef short(__cdecl *spc_read_block)(short mod_no, long block, long frame, long page, short from, short to, unsigned short *data);
typedef short(__cdecl *spc_read_data_block)(short mod_no, long block, long page, short reduction_factor, short from, short to, unsigned short *data);
typedef short(__cdecl *spc_write_data_block)(short mod_no, long block, long page, short from, short to, unsigned short *data);
typedef short(__cdecl *spc_read_data_frame)(short mod_no, long frame, long page, unsigned short *data);
typedef short(__cdecl *spc_read_data_page)(short mod_no, long first_page, long last_page, unsigned short *data);
typedef short(__cdecl *spc_read_mom_data)(short mod_no, short with_counters, short first_chan, short last_chan, unsigned long *data);
typedef short(__cdecl *spc_set_page)(short mod_no, long page);
typedef short(__cdecl *spc_get_sync_state)(short mod_no, short *sync_state);
typedef short(__cdecl *spc_get_fifo_usage)(short mod_no, float *usage_degree);
typedef short(__cdecl *spc_get_scan_clk_state)(short mod_no, short *scan_state);
typedef short(__cdecl *spc_clear_rates)(short mod_no);
typedef short(__cdecl *spc_read_rates)(short mod_no, rate_values *rates);
typedef short(__cdecl *spc_get_time_from_start)(short mod_no, float *time);
typedef short(__cdecl *spc_get_break_time)(short mod_no, float *time);
typedef short(__cdecl *spc_get_actual_coltime)(short mod_no, float *time);
typedef short(__cdecl *spc_test_state)(short mod_no, short *state);
typedef short(__cdecl *spc_start_measurement)(short mod_no);
typedef short(__cdecl *spc_pause_measurement)(short mod_no);
typedef short(__cdecl *spc_restart_measurement)(short mod_no);
typedef short(__cdecl *spc_stop_measurement)(short mod_no);
typedef short(__cdecl *spc_enable_sequencer)(short mod_no, short enable);
typedef short(__cdecl *spc_get_sequencer_state)(short mod_no, short *state);
typedef short(__cdecl *spc_read_gap_time)(short mod_no, float *time);
typedef short(__cdecl *spc_get_eeprom_data)(short mod_no, SPC_EEP_Data *eep_data);
typedef short(__cdecl *spc_write_eeprom_data)(short mod_no, unsigned short write_enable, SPC_EEP_Data *eep_data);
typedef short(__cdecl *spc_get_adjust_parameters)(short mod_no, SPC_Adjust_Para * adjpara);
typedef short(__cdecl *spc_set_adjust_parameters)(short mod_no, SPC_Adjust_Para * adjpara);
typedef short(__cdecl *spc_read_fifo)(short mod_no, unsigned long * count, unsigned short *data);

/*- DPC modules specific functions ------------------------------------------*/
					 // for DPC230, get additional start offs of the 1st photon
typedef short(__cdecl *spc_get_start_offset)(short mod_no, short bank, unsigned long * ticks);
typedef short(__cdecl *dpc_fill_memory)(short mod_no, short page, short bank, unsigned long fill_value);
typedef short(__cdecl *dpc_read_rates)(short mod_no, rate_values_dpc *rates);

/*- Low level functions -----------------------------------------------------*/
typedef short(__cdecl *spc_test_id)(short mod_no);
typedef short(__cdecl *spc_get_version)(short mod_no, unsigned short *version);
typedef short(__cdecl *spc_clear_status_flags)(short mod_no, unsigned short flags);

/*- Miscellaneous -----------------------------------------------------------*/
typedef short(__cdecl *spc_get_module_info)(short mod_no, SPCModInfo * mod_info);
typedef short(__cdecl *spc_close)();
typedef short(__cdecl *spc_set_mode)(short mode, short force_use, int *in_use);
typedef short(__cdecl *spc_get_mode)(void);
typedef short(__cdecl *spc_get_error_string)(short error_id, char * dest_string, short max_length);
typedef short(__cdecl *spc_read_parameters_from_inifile)(SPCdata *data, char *inifile);
typedef short(__cdecl *spc_save_parameters_to_inifile)(SPCdata *data, char * dest_inifile, char *source_inifile, int with_comments);
typedef short(__cdecl *spc_save_data_to_sdtfile)(short mod_no, unsigned short *data_buf, unsigned long bytes_no, char *sdt_file);
typedef short(__cdecl *spc_init_phot_stream)(short fifo_type, char *spc_file, short files_to_use, short stream_type, short what_to_read);
typedef short(__cdecl *spc_close_phot_stream)(short stream_hndl);
typedef short(__cdecl *spc_get_phot_stream_info)(short stream_hndl, PhotStreamInfo *stream_info);
typedef short(__cdecl *spc_get_photon)(short stream_hndl, PhotInfo *phot_info);
typedef short(__cdecl *spc_get_detector_info)(short previous_type, short * det_type, char * fname);
typedef short(__cdecl *spc_convert_dpc_raw_data)(short tdc1_stream_hndl, short tdc2_stream_hndl, short init, char *spc_file, int max_per_call);

// functions for buffered streams of photons
typedef short(__cdecl *spc_get_fifo_init_vars)(short mod_no, short *fifo_type, short *stream_type, int *mt_clock, unsigned int *spc_header);
typedef short(__cdecl *spc_init_buf_stream)(short fifo_type, short stream_type, short what_to_read, int mt_clock, unsigned int   start01_offs);
typedef short(__cdecl *spc_add_data_to_stream)(short stream_hndl, void *buffer, unsigned int   bytes_no);
typedef short(__cdecl *spc_read_fifo_to_stream)(short stream_hndl, short mod_no, unsigned long * count);
typedef short(__cdecl *spc_get_photons_from_stream)(short stream_hndl, PhotInfo64 *phot_info, int *phot_no);
typedef short(__cdecl *spc_stream_start_condition)(short stream_hndl, double start_time, unsigned int   start_OR_mask, unsigned int   start_AND_mask);
typedef short(__cdecl *spc_stream_stop_condition)(short stream_hndl, double stop_time, unsigned int   stop_OR_mask, unsigned int   stop_AND_mask);
typedef short(__cdecl *spc_stream_reset)(short stream_hndl);
typedef short(__cdecl *spc_get_stream_buffer_size)(short stream_hndl, unsigned short buf_no, unsigned int *buf_size);
typedef short(__cdecl *spc_get_buffer_from_stream)(short stream_hndl, unsigned short buf_no, unsigned int *buf_size, char *data_buf, short free_buf);
