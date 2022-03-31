using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public class EncodeMatch
    {
        public EncodeMatch(OcrEntry entry, IList<int> extraBlocks)
        {
            this.OcrEntry = entry;
            this.ExtraBlocks = new List<int>(extraBlocks);
        }

        public OcrEntry OcrEntry { get; private set; }
        public IList<int> ExtraBlocks { get; private set; }
    }
}
