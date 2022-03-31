using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public class OcrSplit : IEquatable<OcrSplit>
    {
        public OcrSplit(Point offset, string fullEncode)
        {
            this.Offset = offset;
            this.FullEncode = fullEncode;
        }

        public Point Offset { get; private set; }
        public string FullEncode { get; private set; }

        public bool Equals(OcrSplit other)
        {
            return (this.Offset == other.Offset) && (this.FullEncode == other.FullEncode);
        }
    }
}
