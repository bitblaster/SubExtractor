using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DvdNavigatorCrm;

namespace DvdSubOcr
{
    public class CellStartInfo
    {
        public int SubtitleIndex { get; set; }
        public double PtsOffset { get; set; }
        public SaverCellType CellType { get; set; }
        public long FilePosition { get; set; }
        public double FirstCellPts { get; set; }
        public double FirstAudioPts { get; set; }
        public double FirstVideoPts { get; set; }

        public double MinimumAudioVideo
        {
            get
            {
                return Math.Min(this.FirstVideoPts, this.FirstAudioPts);
            }
        }
    }

    public class SubtitlePacklist : ISubtitleStorage
    {
        IList<int> currentPalette;
        List<ISubtitleData> allSubtitles = new List<ISubtitleData>();
        Dictionary<int, SubtitleStream> streams = new Dictionary<int, SubtitleStream>();
        Dictionary<int, double> streamPts = new Dictionary<int, double>();
        Dictionary<int, int> streamCount = new Dictionary<int, int>();
        Dictionary<int, int> streamForcedCount = new Dictionary<int, int>();
        List<AudioStreamItem> audioStreams = new List<AudioStreamItem>();
        List<CellStartInfo> cellStarts = new List<CellStartInfo>();

        public SubtitlePacklist(string filePath)
        {
            switch(Path.GetExtension(filePath).ToLowerInvariant())
            {
            case ".idx":
                VobSubReader.ReadSubs(filePath, this);
                break;
            case ".bin":
                SubtitleStorage.ReadStorage(filePath, this);
                break;
            case ".sup":
                ReadBluRaySupFile(filePath);
                break;
            }
        }

        void ReadBluRaySupFile(string filePath)
        {
            const int StreamId = 1;

            AddHeader(Path.GetDirectoryName(filePath), 1, 1, 0);
            AddStream(StreamId,
                new SubpictureAttributes
                {
                    TrackId = 1,
                    SubpictureFormat = SubpictureFormat.Wide,
                    CodeExtension = SubpictureCodeExtension.Normal,
                    Language = "en"
                });

            StringBuilder log = new StringBuilder();
            IList<PcsData> pcsDatas = BluRaySupParser.ParseBluRaySup(filePath, log);
            if(pcsDatas.Count > 0)
            {
                AddVideoAudioInfo(
                    new VideoAttributes
                    {
                        AspectRatio = VideoAspectRatio._16by9,
                        CodingMode = VideoCodingMode.Mpeg_2,
                        HorizontalResolution = pcsDatas[0].Size.Width,
                        VerticalResolution = pcsDatas[0].Size.Height,
                        Standard = VideoStandard.NTSC,
                    }, new List<AudioStreamItem>());

                foreach(PcsData pcs in pcsDatas)
                {
                    SupSubtitleData subData = new SupSubtitleData(1, pcs);
                    if(subData.Pts >= this.streamPts[StreamId])
                    {
                        if(!subData.TestIfEmpty())
                        {
                            this.streamPts[StreamId] = subData.Pts;
                            this.allSubtitles.Add(subData);
                            this.streamCount[StreamId] = this.streamCount[StreamId] + 1;
                            if(subData.Forced)
                            {
                                this.streamForcedCount[StreamId] =
                                    this.streamForcedCount[StreamId] + 1;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Empty BluRay Subtitle");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Out of Order Subtitle Packet");
                    }
                }
            }
        }

        public string DvdDirectoryPath { get; private set; }
        public int IfoNumber { get; private set; }
        public int TrackNumber { get; private set; }
        public int Angle { get; private set; }
        public IDictionary<int, SubtitleStream> Streams { get { return this.streams; } }
        public IList<ISubtitleData> Subtitles { get { return allSubtitles.AsReadOnly(); } }
        public IList<AudioStreamItem> AudioStreams { get { return this.audioStreams.AsReadOnly(); } }
        public VideoAttributes VideoAttributes { get; private set; }
        public IDictionary<int, int> Count { get { return this.streamCount; } }
        public IDictionary<int, int> ForcedCount { get { return this.streamForcedCount; } }
        public IList<CellStartInfo> CellStarts { get { return cellStarts; } }

        public int IndexInStream(int currentIndex, bool forcedOnly)
        {
            int streamId = this.allSubtitles[currentIndex].StreamId;
            int count = 0;
            for(int subIndex = currentIndex - 1; subIndex >= 0; subIndex--)
            {
                if((this.allSubtitles[subIndex].StreamId == streamId) && (!forcedOnly || this.allSubtitles[subIndex].Forced))
                {
                    count++;
                }
            }
            return count;
        }

        public int FindIndexFromStream(int streamIndex, int streamId, bool forcedOnly)
        {
            int count = 0;
            for(int subIndex = 0; subIndex < this.allSubtitles.Count; subIndex++)
            {
                if((this.allSubtitles[subIndex].StreamId == streamId) && (!forcedOnly || this.allSubtitles[subIndex].Forced))
                {
                    if(count == streamIndex)
                    {
                        return subIndex;
                    }
                    count++;
                }
            }
            return -1;
        }

        public int FindFirst(int? streamId, bool forcedOnly)
        {
            if(streamId.HasValue)
            {
                for(int currentIndex = 0; currentIndex < this.allSubtitles.Count; currentIndex++)
                {
                    if((this.allSubtitles[currentIndex].StreamId == streamId.Value) &&
                        (!forcedOnly || this.allSubtitles[currentIndex].Forced))
                    {
                        return currentIndex;
                    }
                }
            }
            else
            {
                return 0;
            }
            return -1;
        }

        public int FindNext(int currentIndex, int? streamId, bool forcedOnly)
        {
            if(streamId.HasValue)
            {
                currentIndex++;
                while(currentIndex < this.allSubtitles.Count)
                {
                    if((this.allSubtitles[currentIndex].StreamId == streamId.Value) &&
                        (!forcedOnly || this.allSubtitles[currentIndex].Forced))
                    {
                        return currentIndex;
                    }
                    currentIndex++;
                }
            }
            else
            {
                if(currentIndex < this.allSubtitles.Count - 1)
                {
                    return currentIndex + 1;
                }
            }
            return -1;
        }

        public int FindPrevious(int currentIndex, int? streamId, bool forcedOnly)
        {
            if(streamId.HasValue)
            {
                currentIndex--;
                while(currentIndex >= 0)
                {
                    if((this.allSubtitles[currentIndex].StreamId == streamId.Value) &&
                        (!forcedOnly || this.allSubtitles[currentIndex].Forced))
                    {
                        return currentIndex;
                    }
                    currentIndex--;
                }
            }
            else
            {
                if(currentIndex > 0)
                {
                    return currentIndex - 1;
                }
            }
            return -1;
        }

        public void AddHeader(string dvdDirectoryPath, int ifoNumber, int trackNumber, int angle)
        {
            if(!string.IsNullOrEmpty(this.DvdDirectoryPath))
            {
                throw new InvalidOperationException("Second AddHeader found");
            }
            this.DvdDirectoryPath = dvdDirectoryPath;
            this.IfoNumber = ifoNumber;
            this.TrackNumber = trackNumber;
            this.Angle = angle;
        }

        HashSet<int> cellStartedStreams = new HashSet<int>();

        public void AddCellStartOffsets(double previousPtsOffset, SaverCellType cellType, long filePosition, 
            double firstCellPts, double firstAudioPts, double firstVideoPts)
        {
            this.cellStartedStreams.Clear();
            this.cellStarts.Add(new CellStartInfo
            {
                SubtitleIndex = this.allSubtitles.Count,
                PtsOffset = previousPtsOffset,
                CellType = cellType,
                FilePosition = filePosition,
                FirstCellPts = firstCellPts,
                FirstAudioPts = firstAudioPts,
                FirstVideoPts = firstVideoPts
            });
        }

        public void AddVideoAudioInfo(VideoAttributes video, IList<AudioStreamItem> audio)
        {
            this.VideoAttributes = video;
            this.audioStreams.AddRange(audio);
        }

        public void AddPalette(IList<int> yuvColors)
        {
            this.currentPalette = new List<int>(yuvColors);
        }

        public void AddStream(int streamId, SubpictureAttributes attribs)
        {
            this.streams[streamId] = new SubtitleStream(streamId, attribs);
            this.streamPts[streamId] = -1.0;
            this.streamCount[streamId] = 0;
            this.streamForcedCount[streamId] = 0;
        }

        public void AddSubtitlePacket(int streamId, byte[] buffer, int offset, int length, double pts)
        {
            double packetPts = pts;

            if(!this.streamPts.ContainsKey(streamId))
            {
                SubpictureAttributes tempAttribs = new SubpictureAttributes()
                {
                    Language = "",
                    CodeExtension = SubpictureCodeExtension.UnSpecified,
                    SubpictureFormat = DvdNavigatorCrm.SubpictureFormat.Wide,
                };
                this.streams[streamId] = new SubtitleStream(streamId, tempAttribs);
                this.streamPts[streamId] = -1.0;
                this.streamCount[streamId] = 0;
                this.streamForcedCount[streamId] = 0;
            }

            if(packetPts >= this.streamPts[streamId])
            {
                byte[] data = new byte[length];
                Buffer.BlockCopy(buffer, offset, data, 0, length);
                DvdSubtitleData subData = new DvdSubtitleData(streamId, this.currentPalette, data, packetPts);
                if(!subData.TestIfEmpty())
                {
                    this.streamPts[streamId] = packetPts;
                    this.allSubtitles.Add(subData);
                    this.streamCount[streamId] = this.streamCount[streamId] + 1;
                    if(subData.Forced)
                    {
                        this.streamForcedCount[streamId] = 
                            this.streamForcedCount[streamId] + 1;
                    }
                }
                else
                {
                    Debug.WriteLine("Empty Subtitle of length " + length.ToString());
                }
            }
            else
            {
                Debug.WriteLine("Out of Order Subtitle Packet");
            }
        }

        public void Close()
        {
        }
    }
}
