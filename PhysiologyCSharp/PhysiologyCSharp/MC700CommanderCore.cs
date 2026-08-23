using System;
using System.Windows.Forms;

namespace PhysiologyCSharp
{
    public class MC700CommanderCore
    {
        private readonly MultiClampTelegraphClient telegraphClient;
        public MC700B_Parameters[] MC700_Params;

        public MC700CommanderCore()
        {
            MC700_Params = Array.Empty<MC700B_Parameters>();

            try
            {
                telegraphClient = new MultiClampTelegraphClient();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in starting managed MC700 telegraph client. " + ex.Message);
            }
        }

        public void GetMC700BGain()
        {
            if (telegraphClient == null)
            {
                MC700_Params = Array.Empty<MC700B_Parameters>();
                return;
            }

            var states = telegraphClient.GetAmplifierStates();
            MC700_Params = new MC700B_Parameters[states.Length];

            for (int i = 0; i < states.Length; i++)
            {
                var state = states[i];
                MC700_Params[i] = new MC700B_Parameters
                {
                    ID = state.ID,
                    mode = state.mode,
                    primary_gain = state.primary_gain,
                    scaleFactor = state.scaleFactor,
                    LPF_cutoff = state.LPF_cutoff,
                    external_cmd_sensitivity = state.external_cmd_sensitivity,
                    second_alpha = state.second_alpha,
                    second_LPF_cutoff = state.second_LPF_cutoff
                };
            }
        }

        public void Close()
        {
            if (telegraphClient != null)
            {
                telegraphClient.Dispose();
            }
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
    }
}
