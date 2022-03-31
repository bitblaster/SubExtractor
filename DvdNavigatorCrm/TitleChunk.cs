using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DvdNavigatorCrm
{
    public class TitleChunk
    {
        public const int MaxTitleChunkLength = 1 << 24;     // 16MB limit

        public TitleChunk(string filePath, int startOffset, int length, long position, bool isDiscontinuity,
            IEnumerable<int> subtitlePalette, int angle)
        {
            this.FilePath = filePath;
            this.StartOffset = startOffset;
            this.Length = length;
            this.Position = position;
            this.IsDiscontinuity = isDiscontinuity;
            this.SubtitlePalette = new List<int>(subtitlePalette);
            this.Angle = angle;
        }

        public string FilePath { get; private set; }
        public int StartOffset { get; private set; }
        public int Length { get; private set; }
        public long Position { get; private set; }
        public bool IsDiscontinuity { get; private set; }
        public IList<int> SubtitlePalette { get; private set; }
        public int Angle { get; private set; }

        public override string ToString()
        {
            string cellText = String.Format("{0} Start {1}K Length {2}K Position {3}",
                Path.GetFileNameWithoutExtension(this.FilePath),
                this.StartOffset >> 10, this.Length >> 10, this.Position);
            if(this.IsDiscontinuity)
            {
                cellText += " Disc";
            }
            return cellText;
        }
    }
}
