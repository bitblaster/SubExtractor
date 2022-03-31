using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DvdSubOcr
{
    public partial class CharacterSelector : UserControl
    {
        const int LineLength = 27;

        SolidBrush backgroundBrush = new SolidBrush(Color.WhiteSmoke);
        SolidBrush textBrush = new SolidBrush(Color.Black);
        SolidBrush selectedCellBrush = new SolidBrush(Color.Yellow);
        SolidBrush hoveredCellBrush = new SolidBrush(Color.LightGreen);
        Font font = new Font("Tahoma", 14.0f, FontStyle.Regular);
        Font fontItalics = new Font("Tahoma", 14.0f, FontStyle.Italic);
        Font fontSpecial = new Font("Arial", 14.0f, FontStyle.Regular);
        Font fontSpecialItalics = new Font("Arial", 14.0f, FontStyle.Italic);
        bool isItalics;
        StringFormat format = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
        OcrCharacter selectedCharacter;
        OcrCharacter hoveredCharacter;
        const string SpecialCharacters = "♪♥";

        public static string[] AllCharacters = new string[] { 
            "ABCDEFGHIJKLMNOPQRSTUVWXYZØ°",
            "abcdefghijklmnopqrstuvwxyzøþ",
            "0123456789!¡?¿,.;:\'\"„*~-_=+—",
            "ßŒœÆæ♪♥€£¥¢@#$%^&/\\[]{}()<>|",
            "ÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝαΩ",
            "àáâãäåçèéêëìíîïñòóôõöùúûüýªº"
        };

        public CharacterSelector()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.StandardClick, true);
            SetStyle(ControlStyles.StandardDoubleClick, true);
            SetStyle(ControlStyles.UserPaint, true);
            UpdateStyles();

            this.format.Alignment = StringAlignment.Center;
            this.format.LineAlignment = StringAlignment.Center;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OcrCharacter SelectedCharacter
        {
            get { return this.selectedCharacter; }
            set
            {
                if(value != this.selectedCharacter)
                {
                    this.selectedCharacter = value;
                    Invalidate();
                }
            }
        }

        public void Clear()
        {
            if(this.selectedCharacter != null)
            {
                this.selectedCharacter = null;
                Invalidate();
            }
        }

        public bool Italics
        {
            get { return this.isItalics; }
            set
            {
                if(value != this.isItalics)
                {
                    this.isItalics = value;
                    if(this.hoveredCharacter != null)
                    {
                        this.hoveredCharacter = new OcrCharacter(
                            this.hoveredCharacter.Value, value);
                    }
                    Invalidate();
                }
            }
        }

        public event EventHandler<SelectedCharacterArgs> SelectedCharacterChanged;

        protected virtual void OnSelectedCharacterChanged(OcrCharacter newSelection)
        {
            if(newSelection != this.selectedCharacter)
            {
                OcrCharacter old = this.selectedCharacter;
                this.selectedCharacter = newSelection;

                EventHandler<SelectedCharacterArgs> temp = SelectedCharacterChanged;
                if(temp != null)
                {
                    temp(this, new SelectedCharacterArgs(old, newSelection));
                }

                Refresh();
                System.Threading.Thread.Sleep(250);
                //Invalidate();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            //base.OnClick(e);
            Point mouse = PointToClient(Control.MousePosition);
            Point p = new Point(mouse.X / CellWidth, mouse.Y / CellHeight);
            if((p.Y >= 0) && (p.Y < AllCharacters.Length))
            {
                if((p.X >= 0) && (p.X < AllCharacters[p.Y].Length))
                {
                    OcrCharacter c = new OcrCharacter(AllCharacters[p.Y][p.X], this.isItalics);
                    if(c.Equals(this.selectedCharacter))
                    {
                        OnSelectedCharacterChanged(null);
                    }
                    else
                    {
                        OnSelectedCharacterChanged(c);
                    }
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.hoveredCharacter = null;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Point p = new Point(e.X / CellWidth, e.Y / CellHeight);
            if((p.Y >= 0) && (p.Y < AllCharacters.Length))
            {
                if((p.X >= 0) && (p.X < AllCharacters[p.Y].Length))
                {
                    this.hoveredCharacter = new OcrCharacter(AllCharacters[p.Y][p.X], this.isItalics);
                    Invalidate();
                }
            }
        }

        private int CellWidth { get { return this.ClientRectangle.Width / LineLength; } }
        private int CellHeight { get { return this.ClientRectangle.Height / AllCharacters.Length; } }

        private void DrawRow(Graphics g, string characters, int yOffset)
        {
            int cellWidth = this.CellWidth;
            int x = cellWidth / 2;
            foreach(char c in characters)
            {
                Font fontUsed = this.isItalics ? this.fontItalics : this.font;
                if((this.selectedCharacter != null) && (c == this.selectedCharacter.Value))
                {
                    fontUsed = this.selectedCharacter.Italic ? this.fontItalics : this.font;
                    Rectangle rect = new Rectangle(
                        x - cellWidth / 2, yOffset - CellHeight / 2, cellWidth, CellHeight);
                    g.FillRectangle(this.selectedCellBrush, rect);
                }
                if((this.hoveredCharacter != null) && (c == this.hoveredCharacter.Value))
                {
                    fontUsed = this.hoveredCharacter.Italic ? this.fontItalics : this.font;
                    Rectangle rect = new Rectangle(
                        x - cellWidth / 2, yOffset - CellHeight / 2, cellWidth, CellHeight);
                    g.FillRectangle(this.hoveredCellBrush, rect);
                }
                int realX = x;
                switch(c)
                {
                case 'V':
                    realX--;
                    break;
                case 'X':
                    realX++;
                    break;
                }
                if(SpecialCharacters.Contains(c))
                {
                    if(fontUsed == this.fontItalics)
                    {
                        fontUsed = this.fontSpecialItalics;
                    }
                    else
                    {
                        fontUsed = this.fontSpecial;
                    }
                }
                g.DrawString(new string(c, 1), fontUsed, this.textBrush, realX, yOffset, this.format);
                x += cellWidth;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(backgroundBrush, e.ClipRectangle);

            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            e.Graphics.InterpolationMode = InterpolationMode.High;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            int cellHeight = this.CellHeight;
            int y = cellHeight / 2;
            foreach(string characterString in AllCharacters)
            {
                DrawRow(e.Graphics, characterString, y);
                y += cellHeight;
            }
        }
    }
}
