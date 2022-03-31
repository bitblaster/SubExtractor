using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DvdNavigatorCrm;

namespace DvdSubOcr
{
    public class SubtitleStream
    {
        public SubtitleStream(int id, SubpictureAttributes attribs)
        {
            this.StreamId = id;
            this.Language = attribs.Language;
            this.Extension = attribs.CodeExtension;
            this.Format = attribs.SubpictureFormat;
        }

        public int StreamId { get; private set; }
        public string Language { get; private set; }
        public SubpictureCodeExtension Extension { get; private set; }
        public SubpictureFormat Format { get; private set; }

        public override string ToString()
        {
            return this.StreamId.ToString("x2");
        }
    }
}
