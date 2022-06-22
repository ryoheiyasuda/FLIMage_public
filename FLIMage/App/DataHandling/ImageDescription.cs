using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FLIMage
{
    public partial class ImageDescription : Form
    {
        public DataTable imageDescription = new DataTable();
        public FLIMData FLIM_ImgData;

        public ImageDescription(FLIMData FLIM_in)
        {
            InitializeComponent();

            FLIM_ImgData = FLIM_in;
        }

        private void ImageDescription_Load(object sender, EventArgs e)
        {
            if ((Control.ModifierKeys) != 0)
            {
                DescriptionText.Text = FLIM_ImgData.image_description;
                DescriptionText.Select(0, 0);
                dataGridView1.Visible = false;
            }
            else
                CreateAndShowTable();
        }

        private void CreateAndShowTable()
        {
            CreateImageDescription();
            dataGridView1.DataSource = imageDescription;
            //Formatting
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;

            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                int widthCol = dataGridView1.Columns[i].Width;
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns[i].Width = widthCol;
                dataGridView1.Columns[i].ReadOnly = (i != 2);
            }

            var totalHeight = dataGridView1.Rows.GetRowsHeight(DataGridViewElementStates.None);
            var totalWidth = dataGridView1.Columns.GetColumnsWidth(DataGridViewElementStates.None);

            //totalWidth += dataGridView1.RowHeadersWidth;
            totalHeight += dataGridView1.ColumnHeadersHeight;

            this.ClientSize = new Size(totalWidth + 4, totalHeight + 4);

            dataGridView1.Refresh();
        }

        public void CreateImageDescription()
        {
            imageDescription.Clear();
            //Column settings
            imageDescription.Columns.Add("Property", typeof(string));
            imageDescription.Columns.Add("Value");
            //
            imageDescription.Rows.Add("Acquisition date", FLIM_ImgData.acquiredTime);
            //
            imageDescription.Rows.Add("Width (Pixel)", FLIM_ImgData.width);
            imageDescription.Rows.Add("Height (Pixel)", FLIM_ImgData.height);

            for (int i = 0; i < FLIM_ImgData.nChannels; i++)
                imageDescription.Rows.Add("Length of Lifetime - " + (i + 1).ToString(), FLIM_ImgData.n_time[i]);

            imageDescription.Rows.Add("Temporal resolution (ns)", FLIM_ImgData.psPerUnit / 1000.0);
            imageDescription.Rows.Add("Z stack", FLIM_ImgData.ZStack ? FLIM_ImgData.nSlices : 1);
            imageDescription.Rows.Add("Z step size", FLIM_ImgData.State.Acq.sliceStep);
            imageDescription.Rows.Add("Fast Z stack", FLIM_ImgData.nFastZ);
            imageDescription.Rows.Add("Zoom", FLIM_ImgData.State.Acq.zoom);

            double x = FLIM_ImgData.State.Acq.field_of_view[0] * FLIM_ImgData.State.Acq.scanVoltageMultiplier[0] / FLIM_ImgData.State.Acq.zoom;
            double y = FLIM_ImgData.State.Acq.field_of_view[1] * FLIM_ImgData.State.Acq.scanVoltageMultiplier[1] / FLIM_ImgData.State.Acq.zoom;
            imageDescription.Rows.Add("Image size - X (μm)", string.Format("{0:0.00}", x));
            imageDescription.Rows.Add("Image size - Y (μm)", string.Format("{0:0.00}", y));

            imageDescription.Rows.Add("Motor position - X (μm)", FLIM_ImgData.State.Motor.motorPosition[0]);
            imageDescription.Rows.Add("Motor position - Y (μm)", FLIM_ImgData.State.Motor.motorPosition[1]);
            imageDescription.Rows.Add("Motor position - Z (μm)", FLIM_ImgData.State.Motor.motorPosition[2]);
        }
    }
}
