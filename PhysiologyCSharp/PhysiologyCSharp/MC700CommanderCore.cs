using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhysiologyCSharp
{
    public class MC700CommanderCore
    {
        public MC700B_Parameters[] MC700_Params;

        public MC700CommanderCore()
        {
            try
            {
                MC_init();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error in loading DLL (MC700CommanderDLL.dll)." + ex.Message);   
            }
        }

        public void GetMC700BGain()
        {
            for (int i = 0; i < 3; i++)
            {
                MC_broadCast();
                System.Threading.Thread.Sleep(10);
            }

            int n = MC_amplifiersLength();
            MC700_Params = new MC700B_Parameters[n];
            int actualN = n;
            for (int i = 0; i < n; i++)
            {
                MC700_Params[i] = new MC700B_Parameters();

                MC700B_Param MC_param = new MC700B_Param();
                MC_getGain(i, ref MC_param);

                if (MC_param.ID == 0)
                {
                    actualN = i;
                    break;
                }

                var fields_Struct = MC_param.GetType().GetFields();
                
                for (int j = 0; j < fields_Struct.Length; j++)
                {
                    var value = fields_Struct[j].GetValue(MC_param);
                    var name = fields_Struct[j].Name;
                    MC700_Params[i].GetType().GetField(name).SetValue(MC700_Params[i], value);
                }
            }

            Array.Resize(ref MC700_Params, actualN);
            //
        }

        public void Close()
        {
            MC_shutdown();
        }


        public class MC700B_Parameters
        {
            public int ID;
            public int mode;
            public double primary_gain;
            public double scaleFactor;
            public double LPF_cutoff;
            public double external_cmd_sensitivity;
            public double second_alpha;
            public double second_LPF_cutoff;
        }

        [DllImport("MC700BCommanderDLL.dll", EntryPoint = "MC_init", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MC_init();

        [DllImport("MC700BCommanderDLL.dll", EntryPoint = "MC_broadCast", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MC_broadCast();

        [DllImport("MC700BCommanderDLL.dll", EntryPoint = "MC_amplifiersLength", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MC_amplifiersLength();

        [DllImport("MC700BCommanderDLL.dll", EntryPoint = "MC_shutdown", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MC_shutdown();

        [DllImport("MC700BCommanderDLL.dll", EntryPoint = "MC_getGain", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MC_getGain(int id, ref MC700B_Param amp_param);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct MC700B_Param{
            public int ID;
            public int mode;
            public double primary_gain;
            public double scaleFactor;
            public double LPF_cutoff;
            public double external_cmd_sensitivity;
            public double second_alpha;
            public double second_LPF_cutoff;
        }
    }
}
