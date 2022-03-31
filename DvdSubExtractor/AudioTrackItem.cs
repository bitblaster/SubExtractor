using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DvdNavigatorCrm;

namespace DvdSubExtractor
{
    public class AudioTrackItem
    {
        public AudioTrackItem(int track, AudioAttributes audio)
        {
            this.Attributes = audio;
            this.StreamId = track;
        }

        public AudioAttributes Attributes { get; private set; }
        public int StreamId { get; private set; }

        public override string ToString()
        {
            string text = string.Format("{0:x} {1} ({2} {3} channels)",
                this.StreamId, DvdLanguageCodes.GetLanguageText(this.Attributes.Language),
                this.Attributes.CodingMode, this.Attributes.Channels);
            switch(this.Attributes.CodeExtension)
            {
            case AudioCodeExtension.DirectorsComments:
            case AudioCodeExtension.AlternateDirectorsComments:
                text += " Commentary";
                break;
            }
            return text;
        }
    }
}
