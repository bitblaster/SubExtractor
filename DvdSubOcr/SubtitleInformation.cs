using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public class SubtitleInformation : ISubtitleInformation
    {
        public SubtitleInformation(int left, int top, int width, int height, double pts, double duration,
            IEnumerable<Color> paletteEntries, bool isForced)
        {
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
            this.Pts = pts;
            this.Duration = duration;
            this.RgbPalette = new List<Color>(paletteEntries).AsReadOnly();
            this.Forced = isForced;
        }

        public SubtitleInformation(ISubtitleInformation other)
        {
            if(other != null)
            {
                this.Left = other.Left;
                this.Top = other.Top;
                this.Width = other.Width;
                this.Height = other.Height;
                this.Pts = other.Pts;
                this.Duration = other.Duration;
                this.RgbPalette = new List<Color>(other.RgbPalette).AsReadOnly();
                this.Forced = other.Forced;
            }
        }

        public int Left { get; private set; }
        public int Top { get; private set; }
        public Point Origin { get { return new Point(Left, Top); } }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Size Size { get { return new Size(Width, Height); } }
        public double Pts { get; private set; }
        public IList<Color> RgbPalette { get; protected set; }
        public double Duration { get; set; }
        public bool Forced { get; set; }
    }
}
