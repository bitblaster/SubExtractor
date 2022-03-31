using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DvdSubOcr
{
    public partial class FlatTitleBar : UserControl
    {
        static readonly StringFormat titleFormat = CreateTitleFormat();

        Point startingMove;
        bool moving;
        bool hasMinimizeBox = true;
        bool hasCloseBox = true;
        bool formHooked;

        static StringFormat CreateTitleFormat()
        {
            StringFormat format = new StringFormat(StringFormatFlags.NoWrap);
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            return format;
        }

        public FlatTitleBar()
        {
            InitializeComponent();
            this.BackColor = Color.Transparent;

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.UserMouse, true);
            SetStyle(ControlStyles.Selectable, false);
            UpdateStyles();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            //base.OnMouseDown(e);
            if(!this.moving && (e.Button == MouseButtons.Left) && 
                (this.FindForm().WindowState == FormWindowState.Normal))
            {
                this.moving = true;
                this.startingMove = Control.MousePosition;
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            this.moving = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if(this.moving)
            {
                if((Control.MouseButtons != MouseButtons.Left) ||
                     (this.FindForm().WindowState != FormWindowState.Normal))
                {
                    this.moving = false;
                }
                else
                {
                    Point nextPosition = Control.MousePosition;
                    if(nextPosition != this.startingMove)
                    {
                        Point newLocation = this.Parent.Location + new Size(nextPosition) - new Size(this.startingMove);
                        this.Parent.Location = newLocation;
                        this.startingMove = nextPosition;
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if(!this.formHooked)
            {
                this.formHooked = true;
                FindForm().Activated += this.FlatTitleBar_Invalidate;
                FindForm().Deactivate += this.FlatTitleBar_Invalidate;
            }

            if(FindForm() == Form.ActiveForm)
            {
                //using(Brush backBrush = new SolidBrush(SystemColors.ActiveCaption))
                using(Brush backBrush = new LinearGradientBrush(
                    this.ClientRectangle,
                    SystemColors.ActiveCaption, SystemColors.ActiveBorder, 
                    LinearGradientMode.ForwardDiagonal))
                {
                    e.Graphics.FillRectangle(backBrush, this.ClientRectangle);
                }
            }

            //base.OnPaint(e);
            if(this.Text.Length != 0)
            {
                using(Brush foreBrush = new SolidBrush(this.ForeColor))
                {
                    Rectangle rect = this.ClientRectangle;
                    rect.Height += 4;
                    e.Graphics.DrawString(this.Text, this.Font, foreBrush, rect, titleFormat);
                }
            }
        }

        void FlatTitleBar_Invalidate(object sender, EventArgs e)
        {
            Invalidate();
        }

        public static void DrawCoolBorder(Control control, Graphics g)
        {
            Rectangle rect = new Rectangle(Point.Empty, control.Size);
            rect.Width--;
            rect.Height--;
            g.DrawRectangle(Pens.DarkBlue, rect);
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(true)]
        public bool MinimizeBox
        {
            get { return this.hasMinimizeBox; }
            set
            {
                if(value != this.hasMinimizeBox)
                {
                    this.hasMinimizeBox = value;
                    this.minimizeLabel.Visible = value;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(true)]
        public bool CloseBox
        {
            get { return this.hasCloseBox; }
            set
            {
                if(value != this.hasCloseBox)
                {
                    this.hasCloseBox = value;
                    if(value)
                    {
                        this.minimizeLabel.Location = new Point(
                            this.Width - this.minimizeLabel.Width - this.closeLabel.Width,
                            this.minimizeLabel.Top);
                    }
                    else
                    {
                        this.minimizeLabel.Location = new Point(
                            this.Width - this.minimizeLabel.Width,
                            this.minimizeLabel.Top);
                    }
                    this.closeLabel.Visible = value;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if(value != base.Text)
                {
                    base.Text = value;
                    Invalidate();
                }
            }
        }

        private void minimizeLabel_Click(object sender, EventArgs e)
        {
            this.FindForm().WindowState = FormWindowState.Minimized;
        }

        private void closeLabel_Click(object sender, EventArgs e)
        {
            this.FindForm().Close();
        }

        [DefaultValue(typeof(Color), "Transparent")]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = Color.Transparent;
            }
        }
    }
}
