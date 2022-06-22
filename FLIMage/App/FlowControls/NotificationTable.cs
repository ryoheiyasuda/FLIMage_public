using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FLIMage.FlowControls
{
    public partial class NotificationTable : Form
    {
        public FLIMage_Event flimage_event;

        public NotificationTable(FLIMage_Event f_e)
        {
            InitializeComponent();
            flimage_event = f_e;
        }

        public void NotificationTable_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = flimage_event.eventNotifyTable;

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

        public void NotificationTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            flimage_event.WriteEventNotifyList();
        }


        public void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            flimage_event.WriteEventNotifyList();
        }
    }
}
