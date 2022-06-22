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

namespace MathLibrary
{
    public partial class Image_Showing_Window : Form
    {
        PlotOnPictureBox pp;

        public Image_Showing_Window(ushort[,] image_ushort)
        {
            InitializeComponent();
            Bitmap bitmapImage = ImageProcessing.FormatImage(image_ushort);
            showImage(bitmapImage);
        }


        public Image_Showing_Window(Bitmap bitmapImage)
        {
            InitializeComponent();
            showImage(bitmapImage);
        }

        public void showImage(Bitmap bitmapImage)
        {
            var bitmap = ImageProcessing.ResizeBitmap(bitmapImage, pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bitmap;
        }

        public Image_Showing_Window(double[] data_x, double[] data_y)
        {
            InitializeComponent();
            pp = new PlotOnPictureBox(pictureBox1);
            pp.ClearData();
            pp.AddData(data_x, data_y, "-o", 1);
        }

        public void AddPlot(double[] data_x, double[] data_y)
        {
            if (pp == null)
                pp = new PlotOnPictureBox(pictureBox1);
            pp.AddData(data_x, data_y, "-o", 1);
            pictureBox1.Invalidate();
        }


        private void Image_Showing_Window_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (pp != null)
                pp.DataPlot_Paint(sender, e);
        }
    }
}
