using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;
using FLIMage.Analysis;

namespace FLIMage.Dialogs
{
    public partial class ExportForm : Form
    {
        const int MaxChannel = 4;
        Image_Display image_display;
        FLIMData FLIM_ImgData;
        ScanParameters State;
        bool[] SaveChannels;
        int[] ZStackFormat = new int[4];
        CheckBox[] SaveChannelCheckbox = new CheckBox[MaxChannel];

        SettingManager settingManager;
        String settingName = "ExportForm";

        public ExportForm(Image_Display ImageDisp)
        {
            image_display = ImageDisp;
            State = image_display.FLIM_ImgData.State;
            FLIM_ImgData = image_display.FLIM_ImgData;
            InitializeComponent();
        }

        private void ExportForm_Load(object sender, EventArgs e)
        {
            if (FLIM_ImgData.n_pages <= 1)
                ProjectionBox.Enabled = false;
            else
                ProjectionBox.Enabled = true;


            for (int i = 0; i < MaxChannel; i++)
            {
                Control[] found = this.Controls.Find("SaveChannel" + (i + 1), true);
                SaveChannelCheckbox[i] = (CheckBox)found[0];
                if (State.Acq.nChannels < i + 1)
                    found[0].Visible = false;
            }

            SaveChannels = new bool[State.Acq.nChannels];
            for (int i = 0; i < State.Acq.nChannels; i++)
            {
                SaveChannelCheckbox[i].Checked = true;
            }

            FastZGroup.Enabled = image_display.FastZStack;
            if (image_display.FastZStack)
            {
                DefaultZStackSetting();
            }

            InitializeSetting();
        }

        void InitializeSetting()
        {
            settingManager = new SettingManager(settingName, State.Files.initFolderPath);
            settingManager.AddToDict(AllFiles);
            settingManager.AddToDict(ZProjectionCheckBox);
            settingManager.AddToDict(NoProjection);
            settingManager.LoadToObject();
        }

        public void SaveSetting()
        {
            if (settingManager != null)
            {
                settingManager.SaveFromObject();
            }
        }

        private void DefaultZStackSetting()
        {
            int col = (int)Math.Ceiling(Math.Sqrt(FLIM_ImgData.n_pages));
            int ro = (int)Math.Ceiling((double)FLIM_ImgData.n_pages / (double)col);
            Column.Text = col.ToString();
            Row.Text = ro.ToString();
            StartSlice.Text = "1";
            LastSlice.Text = FLIM_ImgData.n_pages.ToString();
            ZStackFormat[0] = col;
            ZStackFormat[1] = ro;
            ZStackFormat[2] = 0;
            ZStackFormat[3] = FLIM_ImgData.n_pages;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            this.Hide();

            for (int i = 0; i < State.Acq.nChannels; i++)
            {
                SaveChannels[i] = SaveChannelCheckbox[i].Checked;
            }

            bool[] saveFormat = new bool[] { NoProjection.Checked, ZProjectionCheckBox.Checked };
            
            Int32.TryParse(Column.Text, out ZStackFormat[0]);
            Int32.TryParse(Row.Text, out ZStackFormat[1]);
            Int32.TryParse(StartSlice.Text, out ZStackFormat[2]);
            Int32.TryParse(LastSlice.Text, out ZStackFormat[3]);

            ZStackFormat[2] -= 1;
            if (ZStackFormat.Any(x => x < 0) || ZStackFormat[3] < ZStackFormat[2])
            {
                DefaultZStackSetting();
            }

            if (!AllFiles.Checked)
                image_display.SaveCurrentIntensityImage(saveFormat, SaveChannels, ZStackFormat, true);
            else
                image_display.BatchExporting(saveFormat, SaveChannels, ZStackFormat);

            this.Close();
        }

        private void ExportForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            settingManager.SaveFromObject();
        }
    }
}
