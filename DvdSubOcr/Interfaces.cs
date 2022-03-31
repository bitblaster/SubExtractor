using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public interface ISubtitleInformation
    {
        int Left { get; }
        int Top { get; }
        Point Origin { get; }
        int Width { get; }
        int Height { get; }
        Size Size { get; }
        double Pts { get; }
        IList<Color> RgbPalette { get; }
        double Duration { get; set; }
        bool Forced { get; set; }
    }

    public interface ISubtitleData : ISubtitleInformation
    {
        int StreamId { get; }

        bool TestIfEmpty();
        SubtitleBitmap DecodeBitmap();
    }
}
