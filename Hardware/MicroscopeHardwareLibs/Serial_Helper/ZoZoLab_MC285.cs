using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroscopeHardwareLibs
{

    public class ZoZoLab_MC285 : SutterMP285
    {
        public ZoZoLab_MC285(String portname, int boudRate) : base(portname, boudRate, "zozolab")
        {
        }        
    }
}
