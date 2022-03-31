using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DvdSubExtractor
{
    public class CreateSubOptions
    {
        public string FileName { get; set; }
        public string OutputDirectory { get; set; } 
        public Point Crop { get; set; } 
        public double OverallPtsAdjustment { get; set; }
        public bool Adjust25to24 { get; set; }
        public bool Is1080p { get; set; }
        public LineBreaksAndPositions PositionAllSubs { get; set; }
        public RemoveSDH RemoveSDH { get; set; }
    }
}
