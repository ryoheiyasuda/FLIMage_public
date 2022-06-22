using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroscopeHardwareLibs.Stage_Contoller
{
    public class MotorCtrl_ZoZoLab : MotorCtrl_MP285A
    {

        public MotorCtrl_ZoZoLab(String port, Double[] resolution, int _velocity, int MotorDisplayUpdateTime)
            : base(port, resolution, _velocity, false, MotorDisplayUpdateTime, MotorCtrl.MotorTypeEnum.zozolab)
        {
            tolerance = 10;
        }

        public override void TypeSpecificCommand()
        {
            int success = 0;
            for (int i = 0; i < 20; i++)
            {
                success = ZoZo_SetXYResolution(resolutionX);
                if (success != 0)
                {
                    break;
                }
                else
                    System.Threading.Thread.Sleep(50);
            }
        }

        public int ZoZo_getPolarity(out bool[] polarityCode)
        {
            return mp285.GetPolarity(out polarityCode);
        }

        public int ZoZo_setPolarity(bool[] polarityCode)
        {
            return mp285.SetPolarity(polarityCode);
        }

        public int ZoZo_getROESpeed(out int speed)
        {
            int success = mp285.GetROESpeed(out speed);
            return success;
        }

        public int ZoZo_SetROESpeed(int speed)
        {
            int success = mp285.SetROESpeed(speed);
            return success;
        }

        public int ZoZo_SetXYResolution(double resolution_um)
        {
            int success = mp285.SetXYResolution((int)(resolution_um * 1000));
            if (success == 1)
            {
                resolutionX = resolution_um;
                resolutionY = resolution_um;

            }
            return success;
        }

        public int ZoZo_GetXYResolution()
        {
            int ret = mp285.GetXYResolution(out int resolution);
            if (resolution != 0)
            {
                resolutionX = (double)resolution/1000;
                resolutionY = (double)resolution/1000;
            }
            return ret;
        }

    } //motorCtrl

}
