using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCSPC_controls
{
    public class FLIM_Parameters
    {
        public int nLines = 128;
        public int nPixels = 128;
        public int nDtime = 64;
        public int nFrames = 1;
        public int nChannels = 2;
        public int nStripes = 32;
        public bool StripeDuringFocus = false;

        public double msPerLine = 2;
        public int n_average = 1;

        public bool enableFastZscan = false;
        public int nFastZSlices = 1;

        public double pixel_time = 10e-6;
        public double fillFraction = 0.7;

        public double AcquisitionDelay = 0.1; //in milliseconds.

        public bool BiDirectionalScan = false;
        public bool SineWaveScan = false;
        public double BiDirectionalDelay = 0; //in milliseconds.

        public int line_StripeCounter = 0;
        public int StripeCounter = 0;
        public int LinesPerStripe = 32;
        public bool[] averageFrame = { false, false };
        public int device = 0;
        public bool eraseMemory_afterAcqisition = false;

        public bool[] acquireFLIM = new bool[] { true, true };
        public bool[] acquisition = new bool[] { true, true };

        public String BoardType = "PQ";

        public int ComputerID = 0;
        public int FLIMserial = 0;


        public SPCData spcData = new SPCData();
        public RateInfo rateInfo = new RateInfo();
        public FastZScan fastZScan = new FastZScan();


        public class RateInfo
        {
            public int[] syncRate = { 0, 0 };
            public int[] countRate = { 0, 0 };
        }

        public class FastZScan
        {
            public double FrequencyKHz = 188; //KHerz
            public float ZScanPerPixel = 2.5f;
            public uint ZScanPerPixel_Bidirecitonal = 1;
            public double XYFillFraction = 0.7;
            public uint TimePerZScan = 425; //Count
            public double VoxelTimeUs = 0; //Microsecond;
            public int ZScanPerLine = 128;
            public int nFastZSlices = 10;
            public int VoxelCount = 3;
            public double[] phaseRange = new double[] { 45.0, 135.0 };
            public uint[] phaseRangeCount = new uint[] { 0, 425 };
            public bool phase_detection_mode = true;
        }

        public class SPCData
        {
            public int n_dataPoint = 50; // will be method.

            public int binning = 2;  //PQ only.

            //specific to PQ
            public String HW_Model = "THarp 260 N";

            //specific to BH
            public String BH_DLLDir = "C:\\Program Files (x86)\\BH\\SPCM\\DLL";
            public String BH_init_file = "C:\\Program Files (x86)\\BH\\SPCM\\DLL\\spcm.ini";

            public int device = 0;
            public double time_per_unit = 1.24677e-08;

            public double[] resolution = { 250, 250 }; //ns
            public int[] sync_divider = { 4, 4 };
            public double[] sync_threshold = { -50, -50 };
            public double[] sync_zc_level = { 0, 0 };
            public int sync_offset = 7000;
            public double[] ch_threshold = { -5, -5 };
            public double[] ch_zc_level = { 0, 0 };
            public int[] ch_offset = { 0, 0 };

            public double line_time_correction = 1.0;
            public double measured_line_time_correction = 1.0;

            //Specific to PQ
            public int n_devicesPQ = 1;
            public int channelPerDevicePQ = 2;
            public int TagID = 2;
            public int acq_modePQ = 3;
            public int lineID_PQ = 1; //M1, M2, M3, M4

            //Specific to BH
            public int n_devicesBH = 2;
            public int channelPerDeviceBH = 1;
            public int acq_modeBH = 5;
            public int lineID_BH = 1;

            // parameters specific to Spcm
            public short adc_res = 6;
            public double[] tac_range = { 50F, 50F };
            public int[] tac_gain = { 4, 4 };
            public double[] tac_offset = { 5, 5 }; //%
            public double[] tac_limit_low = { 5, 5 }; //%
            public double[] tac_limit_high = { 95, 95 }; //%
            public short module_type = 150;
        }

    }

}
