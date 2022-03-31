using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public class SubtitleText
    {
        List<SubtitleLine> lines;
        List<KeyValuePair<int, EncodeMatch>> errors;

        public SubtitleText(IEnumerable<SubtitleLine> lines,
            IEnumerable<KeyValuePair<int, EncodeMatch>> errors)
        {
            this.lines = new List<SubtitleLine>(lines);
            this.errors = new List<KeyValuePair<int, EncodeMatch>>(errors);
        }

        public IList<SubtitleLine> Lines { get { return this.lines.AsReadOnly(); } }

        public IList<KeyValuePair<int, EncodeMatch>> Errors 
        { 
            get 
            { 
                return this.errors.AsReadOnly(); 
            } 
        }
    }
}
