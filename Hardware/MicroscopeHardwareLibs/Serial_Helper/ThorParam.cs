using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroscopeHardwareLibs
{
    public class ThorParam
    {
        public static int PARAM_DEVICE_TYPE = 0;
        public static int PARAM_X_ACCEL = 205;
        public static int PARAM_X_DECEL = 206;
        public static int PARAM_X_HOME = 201;
        public static int PARAM_X_JOYSTICK_VELOCITY = 208;
        public static int PARAM_X_POS = 200;
        public static int PARAM_X_POS_CURRENT = 207;
        public static int PARAM_X_STEPS_PER_MM = 204;
        public static int PARAM_X_STOP = 209;
        public static int PARAM_X_VELOCITY = 203;
        public static int PARAM_X_VELOCITY_CURRENT = 210;
        public static int PARAM_X_ZERO = 202;
        public static int PARAM_Y_ACCEL = 305;
        public static int PARAM_Y_DECEL = 306;
        public static int PARAM_Y_HOME = 301;
        public static int PARAM_Y_JOYSTICK_VELOCITY = 308;
        public static int PARAM_Y_POS = 300;
        public static int PARAM_Y_POS_CURRENT = 307;
        public static int PARAM_Y_STEPS_PER_MM = 304;
        public static int PARAM_Y_STOP = 309;
        public static int PARAM_Y_VELOCITY = 303;
        public static int PARAM_Y_VELOCITY_CURRENT = 310;
        public static int PARAM_Y_ZERO = 302;
        public static int PARAM_Z_ACCEL = 405;
        public static int PARAM_Z_DECEL = 406;
        public static int PARAM_Z_HOME = 401;
        public static int PARAM_Z_JOYSTICK_VELOCITY = 408;
        public static int PARAM_Z_POS = 400;
        public static int PARAM_Z_POS_CURRENT = 407;
        public static int PARAM_Z_STEPS_PER_MM = 404;
        public static int PARAM_Z_STOP = 409;
        public static int PARAM_Z_VELOCITY = 403;
        public static int PARAM_Z_VELOCITY_CURRENT = 410;
        public static int PARAM_Z_ZERO = 402;
        public static int STAGE_X = 0x00000004;
        public static int STAGE_Y = 0x00000008;
        public static int STAGE_Z = 0x00000010;

        public static int PARAM_PMT1_GAIN_POS = 700;
        public static int PARAM_PMT1_ENABLE = 701;
        public static int PARAM_PMT2_GAIN_POS = 702;
        public static int PARAM_PMT2_ENABLE = 703;
        public static int PARAM_PMT3_GAIN_POS = 704;
        public static int PARAM_PMT3_ENABLE = 705;
        public static int PARAM_PMT4_GAIN_POS = 706;
        public static int PARAM_PMT4_ENABLE = 707;
        public static int PARAM_SCANNER_ENABLE = 708;
        public static int PARAM_POWER_ENABLE = 709;

        public static int PARAM_POWER_POS = 710;
        public static int PARAM_POWER_HOME = 711;
        public static int PARAM_POWER_VELOCITY = 712;
        public static int PARAM_PMT1_SAFETY = 713;
        public static int PARAM_PMT2_SAFETY = 714;
        public static int PARAM_PMT3_SAFETY = 715;
        public static int PARAM_PMT4_SAFETY = 716;

        //public static int PARAM_DEVICE_TYPE = 0;    /// used to set a new position
		//public static int PARAM_POWER_POS = 710;    /// reads the current position
		public static int PARAM_POWER_POS_CURRENT = 727;    /// used to calibrate a new zero
		public static int PARAM_POWER_ZERO = 732;   /// reads the original zero position
		public static int PARAM_POWER_ZERO_POS = 735;   /// reads the device's serial number
		public static int PARAM_POWER_SERIALNUMBER = 738;

        public static int PARAM_R_POS = 1200;///<Set Location for R
		public static int PARAM_R_HOME = 1201;///<Home R
		public static int PARAM_R_ZERO = 1202;///<Set the current location to zero for R. The device will read zero for postion after this parameter is set
		public static int PARAM_R_POS_CURRENT = 1207;///<Get Current Location for R
		public static int PARAM_R_STOP = 1209; ///<Get Current Velocity for R
		public static int PARAM_R_VELOCITY_CURRENT = 1210; //<Get Current Velocity for R

        public static int PARAM_LIGHTPATH_GG = 1600;///<Switch the mirror on/off of Galvo/Galvo light path
		public static int PARAM_LIGHTPATH_GR = 1601; ///<Switch the mirror on/off of Galvo/Resonance light path
		public static int PARAM_LIGHTPATH_CAMERA = 1602;///<Switch the mirror on/off of Camera light path		

    }
}
