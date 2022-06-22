using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FLIMage.Analysis;
using MathLibrary;

namespace FLIMage.Dialogs
{
    public partial class ScanAreaWindow : Form
    {
        FLIMageMain flimage;
        ROI Roi;
        ROI[] ROIs;
        PointF originalPoint;
        bool drag_mouse = false;

        public ScanAreaWindow(FLIMageMain flimage_main)
        {
            InitializeComponent();
            flimage = flimage_main;
        }

        public void UpdateScanPositionWindow()
        {
            ScanPosition.Invalidate();
        }

        private void ScanPosition_MouseDown(object sender, MouseEventArgs e)
        {
            drag_mouse = false;
            if (ROIs != null)
            {
                for (int loc = 0; loc < ROIs.Length; loc++)
                {
                    PointF pnt = new PointF(e.X, e.Y);
                    if (ROIs[loc].IsInsideRoi(pnt))
                    {
                        flimage.current_splitScanLocation = loc;
                        originalPoint = pnt;
                        drag_mouse = true;
                        return;
                    }
                }
            }

            if (Roi != null)
            {
                PointF pnt = new PointF(e.X, e.Y);
                if (Roi.IsInsideRoi(pnt))
                {
                    originalPoint = pnt;
                    drag_mouse = true;
                }
            }
        }

        private void calcVoltageMove(PointF pnt)
        {
            var State = flimage.State;

            if (pnt.X < 0 || pnt.X >= ScanPosition.Width)
                return;
            if (pnt.Y < 0 || pnt.Y >= ScanPosition.Height)
                return;

            PointF movement = new PointF(pnt.X - originalPoint.X, pnt.Y - originalPoint.Y);
            double xm = State.Acq.XMaxVoltage / ScanPosition.Width * movement.X;
            double ym = State.Acq.YMaxVoltage / ScanPosition.Height * movement.Y;
            originalPoint = pnt;

            if (ROIs != null)
            {
                State.Acq.XOffset_Split[flimage.current_splitScanLocation] += xm;
                State.Acq.YOffset_Split[flimage.current_splitScanLocation] += ym;
            }

            if (Roi != null)
            {
                State.Acq.XOffset += xm;
                State.Acq.YOffset += ym;
            }
        }

        private void ScanPosition_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag_mouse)
            {
                PointF pnt = new PointF(e.X, e.Y);
                calcVoltageMove(pnt);
                UpdateScanPositionWindow();
            }
        }

        private void ScanPosition_MouseUp(object sender, MouseEventArgs e)
        {
            if (drag_mouse)
            {
                PointF pnt = new PointF(e.X, e.Y);
                calcVoltageMove(pnt);

                //UpdateScanPositionWindow();

                flimage.Fill_OffsetGUI();
                flimage.flimage_io.ResetFocus();
            }

            drag_mouse = false;
        }

       
        private void ScanPosition_Paint(object sender, PaintEventArgs e)
        {
            flimage.ScanPosition_PaintProcol(flimage.State, e, ScanPosition, ref ROIs, ref Roi);
        }

        private void ScanAreaWindow_Resize(object sender, EventArgs e)
        {
            ScanPosition.Width = Width - 16;
            ScanPosition.Height = Height - 40;
            UpdateScanPositionWindow();
        }
    }
}
