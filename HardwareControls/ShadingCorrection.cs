using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace FLIMimage
{
    public partial class ShadingCorrection : Form
    {
        FLIMageMain FLIMage;
        ScanParameters State;
        Image_Display image_display;
        IOControls.Shading shading;
        FileIO fileIO;

        String[] shading_filePath;

        WindowLocManager winManager;
        String WindowName = "ShadingCorrection.loc";

        public ShadingCorrection(FLIMageMain FLIMage_in)
        {
            InitializeComponent();
            FLIMage = FLIMage_in;
            State = FLIMage.State;
            image_display = FLIMage.image_display;
            shading = FLIMage.flimage_io.shading;
            fileIO = FLIMage.fileIO;
            ShadingFileSetting();
        }

        void ShadingFileSetting()
        {
            shading.shading_on = ShadingOnCB.Checked;
            shading.shading_uncaging = Shading_On_Uncaging.Checked;

            String DirPath = Path.Combine(State.Files.initFolderPath, "Shading");
            Directory.CreateDirectory(DirPath);
            shading_filePath = new string[State.Init.EOM_nChannels];
            for (int i = 0; i < State.Init.EOM_nChannels; i++)
            {
                String filePath = Path.Combine(DirPath, "Shaidng-" + (i + 1).ToString() + ".img");
                shading_filePath[i] = filePath;
                if (File.Exists(filePath))
                {
                    float[][] image = null;
                    fileIO.LoadFloatArrayFromTiff(filePath, 0, out image);
                    //fileIO.SaveFloatImageInTiff(image, filePath + ".tif");

                    if (image != null)
                        shading.AddShadingImageFromBinary(image, i);

                    if (i == 0)
                        ShadingFilePath1.Text = shading_filePath[i];
                    else if (i == 1)
                        ShadingFilePath2.Text = shading_filePath[i];
                    else if (i == 2)
                        ShadingFilePath3.Text = shading_filePath[i];
                }
            }
        }

        private void ShadingImage1_Click(object sender, EventArgs e)
        {
            Button buttonBoj = (Button)sender;
            int LaserN = 0;
            for (int i = 0; i < 4; i++)
            {
                if (buttonBoj.Name.EndsWith((i + 1).ToString()))
                    LaserN = i;
            }

            int ShadingCh = image_display.currentChannel;

            shading.AddShadingImage(FLIMage.flimage_io.FLIM_ImgData, LaserN, ShadingCh);
            fileIO.SaveFloatImageInTiff(shading.ShadingImages[LaserN], shading_filePath[LaserN]);
            //fileIO.SaveArray(shading_filePath[LaserN], shading.ShadingImages[LaserN]);

            if (LaserN == 0)
                ShadingFilePath1.Text = shading_filePath[LaserN];
            else if (LaserN == 1)
                ShadingFilePath2.Text = shading_filePath[LaserN];
            else if (LaserN == 2)
                ShadingFilePath3.Text = shading_filePath[LaserN];
        }

        private void ShadingOnCB_Click(object sender, EventArgs e)
        {
            shading.shading_on = ShadingOnCB.Checked;
            shading.shading_uncaging = Shading_On_Uncaging.Checked;
        }

        private void ShadingCorrection_FormClosing(object sender, FormClosingEventArgs e)
        {
            shading.shading_on = false;
            shading.shading_uncaging = false;

            SaveWindowLocation();
            Hide();
            FLIMage.ToolWindowClosed();
        }

        public void SaveWindowLocation()
        {
            winManager.SaveWindowLocation();
        }

        private void ShadingCorrection_Load(object sender, EventArgs e)
        {
            winManager = new WindowLocManager(this, WindowName, State.Files.windowsInfoPath);
            winManager.LoadWindowLocation(false);
        }
    }
}
