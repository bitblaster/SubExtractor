using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DvdNavigatorCrm;

namespace DvdSubExtractor
{
    public class DvdTrackItem : IComparable<DvdTrackItem>
    {
        HashSet<int> selectedAudioStreams = new HashSet<int>();
        const float MinimumTrackTimeAutoCheck = 15.0f * 50.0f;

        public DvdTrackItem(DvdTitleSet tset, int titleIndex) :
            this(tset, titleIndex, tset.Titles[titleIndex])
        {
        }

        public DvdTrackItem(DvdTitleSet tset, int titleIndex, DvdTitle title)
        {
            this.TitleSet = tset;
            this.TitleIndex = titleIndex;
            this.Title = title;
            this.PlaybackTime = this.Title.PlaybackTime;
            if(this.PlaybackTime > MinimumTrackTimeAutoCheck)
            {
                this.IsSelected = true;
            }
            foreach(int streamId in this.Title.AudioStreams)
            {
                this.selectedAudioStreams.Add(streamId);
            }
            if(this.Title.AngleCount != 0)
            {
                this.Angle = 1;
            }
            this.AspectRatio = tset.VideoAttributes.AspectRatio;

            this.ChapterCount = 1;
            this.CellCount = 1;
            foreach(TitleCell cell in title.TitleCells.Skip(1))
            {
                if(cell.Cell.IsStcDiscontinuity)
                {
                    this.ChapterCount++;
                }
                this.CellCount++;
            }
        }

        public DvdTitleSet TitleSet { get; private set; }
        public DvdTitle Title { get; private set; }
        public int TitleIndex { get; private set; }
        public float PlaybackTime { get; private set; }
        public VideoAspectRatio AspectRatio { get; private set; }
        public int ProgramNumber { get; set; }
        public bool IsSelected { get; set; }
        public ICollection<int> SelectedAudioStreams { get { return this.selectedAudioStreams; } }
        public bool AudioStreamsEdited { get; set; }
        public int Angle { get; set; }
        public bool SubtitleDataCreated { get; set; }
        public bool MpegFileCreated { get; set; }
        public bool D2vFileCreated { get; set; }
        public bool HasBeenSplit { get; set; }
        public int ChapterCount { get; private set; }
        public int CellCount { get; private set; }

        public int CompareTo(DvdTrackItem other)
        {
            return this.Title.PlaybackTime.CompareTo(other.Title.PlaybackTime);
        }

        public override string ToString()
        {
            int minutes = (int)this.Title.PlaybackTime / 60;
            int seconds = Convert.ToInt32(this.Title.PlaybackTime) - minutes * 60;
            return String.Format("{0} Program {1} Track {2} (Len {3}:{4:d2} Angles {5} Aspect {6} Chap {7} Cell {8})",
                Path.GetFileNameWithoutExtension(this.TitleSet.FileName), this.ProgramNumber,
                this.TitleIndex, minutes, seconds, this.Title.AngleCount, 
                (this.AspectRatio == VideoAspectRatio._16by9) ? "16:9" : "4:3", this.ChapterCount, this.CellCount);
        }

        public class SortByName : IComparer<DvdTrackItem>
        {
            public int Compare(DvdTrackItem x, DvdTrackItem y)
            {
                int compareResult = string.Compare(x.TitleSet.FileName, 
                    y.TitleSet.FileName, StringComparison.InvariantCultureIgnoreCase);
                if(compareResult == 0)
                {
                    compareResult = x.TitleIndex.CompareTo(y.TitleIndex);
                }
                return compareResult;
            }
        }
    }
}
