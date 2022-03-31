using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DvdSubExtractor
{
    public enum Corner
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3,
    }

    public partial class TriangleFill : UserControl
    {
        Corner origin = Corner.TopLeft;
        Color fillColor = Color.White;

        public TriangleFill()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserPaint, true);
            UpdateStyles();
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Sets the origin of the filled triangle")]
        [DefaultValue(Corner.TopLeft)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Corner Origin
        {
            get { return this.origin; }
            set
            {
                if(value != this.origin)
                {
                    this.origin = value;
                    Invalidate();
                }
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Color of the triangle")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color FillColor
        {
            get { return this.fillColor; }
            set
            {
                if(value != this.fillColor)
                {
                    this.fillColor = value;
                    Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            Rectangle rect = this.ClientRectangle;
            e.Graphics.Clear(this.BackColor);

            Point[] points;
            switch(this.origin)
            {
            case Corner.TopLeft:
                points = new Point[] { rect.Location, new Point(rect.Right, rect.Top), new Point(rect.Left, rect.Bottom) };
                break;
            case Corner.TopRight:
                points = new Point[] { new Point(rect.Right, rect.Top), new Point(rect.Right, rect.Bottom), rect.Location };
                break;
            case Corner.BottomLeft:
                points = new Point[] { new Point(rect.Left, rect.Bottom), rect.Location, new Point(rect.Right, rect.Bottom) };
                break;
            case Corner.BottomRight:
            default:
                points = new Point[] { new Point(rect.Right - 1, rect.Top), new Point(rect.Right - 1, rect.Bottom), new Point(rect.Left - 1, rect.Bottom) };
                break;
            }

            using(SolidBrush fillBrush = new SolidBrush(this.ForeColor))
            {
                e.Graphics.FillPolygon(fillBrush, points);
            }

            /*using(Pen linePen = new Pen(this.ForeColor, 2.5f))
            {
                e.Graphics.DrawLine(linePen, points[1], points[2]);
            }*/

            switch(this.origin)
            {
            case Corner.TopLeft:
            case Corner.TopRight:
                e.Graphics.DrawLine(Pens.Black, rect.Location, new Point(rect.Right, rect.Top));
                break;
            case Corner.BottomLeft:
            case Corner.BottomRight:
            default:
                e.Graphics.DrawLine(Pens.Black, new Point(rect.Left, rect.Bottom - 1), new Point(rect.Right, rect.Bottom - 1));
                break;
            }
        }
    }
}
