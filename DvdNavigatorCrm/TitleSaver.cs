using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading;
using DvdNavigatorCrm;

namespace DvdNavigatorCrm
{
    public class TitleSaver
    {
        const int MaxPsmLength = 1024;
        const int MinimumMillisecondsChapterLength = 10000;

        DvdTitleSet titleSet;
        DvdTitle title;
        ICollection<int> angles;
        string ifoFileBase;
        List<long> vobFileSizes = new List<long>();
        List<AudioStreamItem> audioStreams = new List<AudioStreamItem>();
        List<int> audioStreamsInOrderFound = new List<int>();
        ISubtitleStorage storage;
        FileStream fsMpeg;
        StreamWriter chaptersWriter;
        byte[] programStreamMap;
        //bool psmAdded;
        Action<string> updateMethod;
        int totalPacketsToSave;
        int packetsSaved;
        bool stopRun;
        double? startPts;
        double? endPts;
        double previousCellPtsEnd;
        double? chainPtsOffset;
        long? maximumLength;
        const long MaxBytesPerMilliSecond = 2000;
        PartialSaveStatus partialSaveStatus;
        bool tooManyBytesRead;
        List<double> chapterOffsets = new List<double>();
        double chapterFunnyBusiness = 0;
        SortedDictionary<int, long> audioLengths = new SortedDictionary<int, long>();
        class TitleChainData
        {
            public TitleChainData(SaverCellType cellType)
            {
                this.Chunks = new List<TitleChunk>();
                this.AngleCellIds = new List<CellIdVobId>();
                this.VobCellIds = new List<CellIdVobId>();
                this.CellType = cellType;
            }

            public IList<TitleChunk> Chunks { get; private set; }
            public IList<CellIdVobId> AngleCellIds { get; private set; }
            public IList<CellIdVobId> VobCellIds { get; private set; }
            public SaverCellType CellType { get; set; }
            public float PlaybackTime { get; set; }
        }

        public TitleSaver(DvdTitleSet titleSet, int titleIndex, DvdTitle title, IEnumerable<int> angles, 
            string storageFileName, string mpegFileName, 
            string chapterFileName, double? startPts, double? endPts) :
            this(titleSet, titleIndex, title, angles, titleSet.Titles[titleIndex].AudioStreams,
            storageFileName, mpegFileName, chapterFileName, startPts, endPts)
        {
        }

        public TitleSaver(DvdTitleSet titleSet, int titleIndex, DvdTitle title, IEnumerable<int> angles, 
            IEnumerable<int> audioStreamIds, string storageFileName, string mpegFileName, 
            string chapterFileName, double? startPts, double? endPts)
        {
            this.titleSet = titleSet;
            this.title = title;
            this.angles = new HashSet<int>(angles);
            string dvdPath = Path.GetDirectoryName(this.titleSet.FileName);
            this.ifoFileBase = Path.Combine(dvdPath, Path.GetFileNameWithoutExtension(this.titleSet.FileName).Remove(7));
            int ifoNumber = Int32.Parse(Path.GetFileNameWithoutExtension(this.titleSet.FileName).Substring(4, 2));
            this.startPts = startPts;
            this.endPts = endPts;
            if(this.startPts.HasValue != this.endPts.HasValue)
            {
                throw new ArgumentException("startPts and endPts must either both have values or neither");
            }
            if(this.startPts.HasValue)
            {
                this.maximumLength = Convert.ToInt64((this.endPts.Value - this.startPts.Value) * MaxBytesPerMilliSecond);
            }

            if(!String.IsNullOrEmpty(storageFileName))
            {
                this.storage = SubtitleStorage.CreateWriter(storageFileName);
                int angle = 0;
                foreach(int nextAngle in this.angles)
                {
                    if(nextAngle != 0)
                    {
                        angle = nextAngle;
                        break;
                    }
                }
                this.storage.AddHeader(dvdPath, ifoNumber, titleIndex, angle);
                foreach(int streamId in this.title.SubtitleStreams)
                {
                    SubpictureAttributes subAttributes = this.title.GetSubtitleStream(streamId);
                    this.storage.AddStream(streamId, subAttributes);
                }
            }

            if(!String.IsNullOrEmpty(mpegFileName))
            {
                // truncate the file
                this.fsMpeg = File.Create(mpegFileName);
            }

            if(!String.IsNullOrEmpty(chapterFileName))
            {
                // truncate the file
                this.chaptersWriter = File.CreateText(chapterFileName);
            }

            for(int vobIndex = 1; vobIndex <= 9; vobIndex++)
            {
                string vobName = string.Format("{0}{1}.VOB", ifoFileBase, vobIndex);
                if(File.Exists(vobName))
                {
                    this.vobFileSizes.Add(new FileInfo(vobName).Length);
                }
                else
                {
                    break;
                }
            }

            foreach(int streamId in audioStreamIds)
            {
                AudioAttributes audioAttributes = this.title.GetAudioStream(streamId);
                this.audioStreams.Add(new AudioStreamItem(streamId, audioAttributes));
            }

            if(this.startPts.HasValue && (this.startPts.Value > 0))
            {
                this.partialSaveStatus = PartialSaveStatus.BeforeStart;
            }
            else
            {
                this.partialSaveStatus = PartialSaveStatus.InRange;
            }

            this.programStreamMap = BuildProgramStreamMap();
        }

        public void StopRun()
        {
            this.stopRun = true;
        }

        public void Run(Action<string> updater)
        {
            this.updateMethod = updater;

            try
            {
                //this.psmAdded = false;
                List<TitleChainData> chains = new List<TitleChainData>();
                TitleChainData chainData = new TitleChainData(SaverCellType.First);
                int oldProgramChainIndex = -1;
                this.TotalLength = 0L;
                foreach(TitleCell cell in this.title.TitleCells)
                {
                    if(this.angles.Contains(cell.CellAngle))
                    {
                        bool isDiscontinuity = false;
                        if(this.TotalLength != 0L)
                        {
                            if(cell.Cell.IsStcDiscontinuity || (cell.ProgramChainIndex != oldProgramChainIndex))
                            {
                                isDiscontinuity = true;
                            }
                        }
                        oldProgramChainIndex = cell.ProgramChainIndex;

                        if(chainData.Chunks.Count != 0)
                        {
                            if(isDiscontinuity)
                            {
                                chainData.CellType |= SaverCellType.Last;
                                chains.Add(chainData);
                                chainData = new TitleChainData(SaverCellType.First);
                            }
                            else
                            {
                                chains.Add(chainData);
                                chainData = new TitleChainData(SaverCellType.None);
                            }
                            chainData.PlaybackTime = 0.0f;
                        }

                        chainData.VobCellIds.Add(new CellIdVobId() { CellId = cell.Cell.CellId, VobId = cell.Cell.VobId });
                        if(cell.CellAngle != 0)
                        {
                            chainData.AngleCellIds.Add(new CellIdVobId() { CellId = cell.Cell.CellId, VobId = cell.Cell.VobId });
                        }

                        long cellStart = (long)cell.Cell.FirstVobuStartSector * 0x800L;
                        long cellEnd = (long)(cell.Cell.LastVobuEndSector + 1) * 0x800L;
                        AddChunksFromCell(chainData.Chunks, cellStart, cellEnd, vobFileSizes, ifoFileBase, isDiscontinuity, cell, this.TotalLength);
                        this.TotalLength += (cellEnd - cellStart);
                        chainData.PlaybackTime = cell.Cell.PlaybackTime;
                    }
                    else
                    {
                        if(cell.CellAngle == 0)
                        {
                            oldProgramChainIndex = -1;
                        }
                    }
                }
                if(chainData.Chunks.Count != 0)
                {
                    chainData.CellType |= SaverCellType.Last;
                    chains.Add(chainData);
                }

                this.stopRun = false;
                this.tooManyBytesRead = false;
                this.chainPtsOffset = null;
                this.TotalRead = 0L;
                foreach(TitleChainData chain in chains)
                {
                    LoadAndSaveChain(chain.CellType, chain.Chunks, chain.VobCellIds, chain.AngleCellIds, chain.PlaybackTime);
                    if(IsLoadStopped() || (this.partialSaveStatus == PartialSaveStatus.AfterEnd))
                    {
                        break;
                    }
                }

                if(this.chaptersWriter != null)
                {
                    int chapterNumber = 1;
                    double lastChapter = -MinimumMillisecondsChapterLength - 1;
                    foreach(double chapterOffset in this.chapterOffsets)
                    {
                        if(chapterOffset >= lastChapter + MinimumMillisecondsChapterLength)
                        {
                            TimeSpan timeStart = new TimeSpan(Convert.ToInt64(chapterOffset) * 10000L);
                            string timeLine = string.Format(
                                "CHAPTER{0:d2}={1:d2}:{2:d2}:{3:d2}.{4:d3}",
                                chapterNumber, timeStart.Hours, timeStart.Minutes,
                                timeStart.Seconds, timeStart.Milliseconds);
                            this.chaptersWriter.WriteLine(timeLine);
                            string nameLine = string.Format("CHAPTER{0:d2}NAME=Chapter {0}",
                                chapterNumber);
                            this.chaptersWriter.WriteLine(nameLine);
                            chapterNumber++;
                            lastChapter = chapterOffset;
                        }
                    }
                }

                if(this.storage != null)
                {
                    List<AudioStreamItem> audioInOrder = new List<AudioStreamItem>();
                    foreach(int streamId in this.audioStreamsInOrderFound)
                    {
                        audioInOrder.AddRange(this.audioStreams.Where(
                            item => item.StreamId == streamId));
                    }
                    this.storage.AddVideoAudioInfo(this.titleSet.VideoAttributes, audioInOrder);
                }
            }
            finally
            {
                if(this.storage != null)
                {
                    this.storage.Close();
                }
                if(this.fsMpeg != null)
                {
                    this.fsMpeg.Close();
                }
                if(this.chaptersWriter != null)
                {
                    this.chaptersWriter.Close();
                }
            }

            foreach(KeyValuePair<int, long> entry in this.audioLengths)
            {
                Debug.WriteLine(string.Format("Stream {0:x} had {1} bytes", entry.Key, entry.Value));
            }
        }

        static void AddChunksFromCell(IList<TitleChunk> chunks, long cellStart, long cellEnd, IList<long> vobFileSizes,
            string ifoFileBase, bool isDiscontinuity, TitleCell cell, long totalLength)
        {
            long cellLength = cellEnd - cellStart;
            VobNumber vob = VobNumber.Calculate(vobFileSizes, cellStart);
            string ifoFilePath = String.Format("{0}{1}.VOB", ifoFileBase, vob.IfoFileNumber);

            while(cellLength > vob.IfoRemainder)
            {
                chunks.Add(new TitleChunk(ifoFilePath, vob.IfoOffset, vob.IfoRemainder,
                    totalLength, isDiscontinuity, cell.ProgramChain.Palette,
                    cell.CellAngle));
                totalLength += vob.IfoRemainder;
                isDiscontinuity = false;
                cellLength -= vob.IfoRemainder;
                cellStart += vob.IfoRemainder;
                vob = VobNumber.Calculate(vobFileSizes, cellStart);
                ifoFilePath = String.Format("{0}{1}.VOB", ifoFileBase, vob.IfoFileNumber);
            }

            chunks.Add(new TitleChunk(ifoFilePath, vob.IfoOffset, (int)cellLength,
                totalLength, isDiscontinuity, cell.ProgramChain.Palette,
                cell.CellAngle));
        }

        void loader_BytesRead(object sender, LoadedBytesEventArgs e)
        {
            this.TotalRead += e.ByteCount;
            this.updateMethod(string.Format("{0}M of {1}M read", this.TotalRead >> 20, this.TotalLength >> 20));
            if(this.maximumLength.HasValue && (this.TotalRead > this.maximumLength.Value))
            {
                this.tooManyBytesRead = true;
            }
        }

        void saver_SavedPacket(object sender, EventArgs e)
        {
            this.packetsSaved++;
            if((this.packetsSaved % 1024) == 0)
            {
                this.updateMethod(string.Format("{0}M of {1}M read, {2}k of {3}k packets written",
                    this.TotalRead >> 20, this.TotalLength >> 20, 
                    this.packetsSaved >> 10, this.totalPacketsToSave >> 10));
            }
        }

        public long TotalRead { get; private set; }
        public long TotalLength { get; private set; }

        bool IsLoadStopped()
        {
            return this.stopRun || this.tooManyBytesRead;
        }

        bool IsRunStopped()
        {
            return this.stopRun;
        }

        void LoadAndSaveChain(SaverCellType cellType, IList<TitleChunk> chunks, IList<CellIdVobId> vobCellIds, IList<CellIdVobId> angleCellIds, float playbackTime)
        {
            long previousTotalRead = this.TotalRead;
            CellLoader loader = new CellLoader(vobCellIds, angleCellIds, this.audioStreams);
            loader.BytesRead += this.loader_BytesRead;
            loader.Run(chunks, new Func<bool>(this.IsLoadStopped));
            loader.BytesRead -= this.loader_BytesRead;
            GC.Collect();

            // skip cells without both audio and video. They're just trouble - messing up synchronization no end
            if(this.stopRun || !loader.FirstAudioPts.HasValue || !loader.FirstVideoPts.HasValue)
            {
                this.TotalRead = previousTotalRead;
                return;
            }

            foreach(int streamId in loader.AudioStreamsFound)
            {
                if(!this.audioStreamsInOrderFound.Contains(streamId))
                {
                    this.audioStreamsInOrderFound.Add(streamId);
                }
            }

            foreach(KeyValuePair<int, long> entry in loader.AudioLengths)
            {
                long lastLength;
                this.audioLengths.TryGetValue(entry.Key, out lastLength);
                this.audioLengths[entry.Key] = lastLength + entry.Value;
            }

            /*if(!this.psmAdded)
            {
                for(int index = 0; index < this.loader.Packets.Count; index++)
                {
                    StreamPackHeaderBuffer packet = this.loader.Packets[index] as StreamPackHeaderBuffer;
                    if(packet != null)
                    {
                        if(index < this.loader.Packets.Count - 1)
                        {
                            HeaderPacketBuffer systemHeader = this.loader.Packets[index + 1] as HeaderPacketBuffer;
                            if(systemHeader != null)
                            {
                                index++;
                            }
                        }
                        this.loader.Packets.Insert(index + 1,
                            new HeaderPacketBuffer() { Data = this.programStreamMap, PacketTypeCode = MpegPSSplitter.ProgramStreamMapCode });
                        this.psmAdded = true;
                        //index++;
                        break;
                    }
                }
            }*/

            if(!this.chainPtsOffset.HasValue)
            {
                // start our new mpeg file at 0 timestamp
                this.chainPtsOffset = -loader.FirstPts.Value;
            }
            else
            {
                if((cellType & SaverCellType.First) == SaverCellType.First)
                {
                    this.chainPtsOffset = this.previousCellPtsEnd - loader.FirstPts.Value;
                }
                else
                {
                    if(Math.Abs(this.previousCellPtsEnd - loader.FirstPts.Value) >= 500.0)
                    {
                        // cells in a chain shouldn't be discontinuous, but DVD authors are evil,
                        // so if the jump is 1 second or more make an adjustment
                        this.chainPtsOffset += (this.previousCellPtsEnd - loader.FirstPts.Value);
                        this.chapterFunnyBusiness += (this.previousCellPtsEnd - loader.FirstPts.Value);
                    }
                }
            }

            long filePosition = 0;
            if(this.storage != null)
            {
                if(this.fsMpeg != null)
                {
                    this.fsMpeg.Flush();
                    filePosition = this.fsMpeg.Position;
                }
                this.storage.AddCellStartOffsets(this.chainPtsOffset.Value, cellType, filePosition, loader.FirstPts.Value, 
                    loader.FirstAudioPts.Value, loader.FirstVideoPts.Value);
            }

            Debug.WriteLine(string.Format("Cell {6} Offset {0:f2} First {1:f2} Video {2:f2} Audio {3:f2} Last {4:f2} FilePos {5}",
                this.chainPtsOffset, loader.FirstPts.Value, loader.FirstVideoPts.Value, loader.FirstAudioPts.Value,
                loader.LastPts.Value, filePosition, cellType));

            CellSaver saver = new CellSaver(this.chainPtsOffset.Value, this.partialSaveStatus, this.startPts, this.endPts);
            this.totalPacketsToSave = loader.Packets.Count;
            this.packetsSaved = 0;
            saver.SavedPacket += this.saver_SavedPacket;
            saver.Run(loader.Packets, this.fsMpeg, this.storage, new Func<bool>(this.IsRunStopped));
            saver.SavedPacket -= this.saver_SavedPacket;
            loader.ClearPackets();
            GC.Collect();
            if(saver.FirstPackHeaderPts.HasValue)
            {
                this.chapterOffsets.Add(saver.FirstPackHeaderPts.Value - this.chapterFunnyBusiness);
            }
            this.partialSaveStatus = saver.PartialSaveStatus;

            if((cellType & SaverCellType.Last) == SaverCellType.Last)
            {
                // if we're ending a chain, store the adjusted pts since the next chain will be 
                // discontinuous anyway
                this.previousCellPtsEnd = loader.LastPts.Value + this.chainPtsOffset.Value;
            }
            else
            {
                this.previousCellPtsEnd = loader.LastPts.Value;
            }
        }

        static uint[] crc32_table = new uint[256];

        void InitializeCrc()
        {
            uint i, j, k;
            for(i = 0; i < 256; i++)
            {
                k = 0;
                for(j = (i << 24) | 0x800000; j != 0x80000000; j <<= 1)
                {
                    k = (k << 1) ^ ((((k ^ j) & 0x80000000) != 0) ? (uint)0x04c11db7 : 0);
                }
                crc32_table[i] = k;
            }
        }

        byte[] BuildProgramStreamMap()
        {
            InitializeCrc();

            int psmLength = 10;
            byte[] psm = new byte[MaxPsmLength];
            psm[0] = 0;
            psm[1] = 0;
            psm[2] = 1;
            psm[3] = MpegPSSplitter.ProgramStreamMapCode;
            psm[6] = 0xe2;
            psm[7] = 0xff;

            int infoLength = 0;
            psm[8] = (byte)(infoLength >> 8);
            psm[9] = (byte)(infoLength & 0xff);
            psmLength += infoLength;

            int esMapLength = 0;
            int esMapOffset = 12 + infoLength;

            // fill in the video stream
            psm[esMapOffset] = 0x02;
            psm[esMapOffset + 1] = 0xe0;
            psm[esMapOffset + 2] = 0;
            psm[esMapOffset + 3] = 0;
            esMapLength += 4;

            foreach(int streamId in this.title.AudioStreams)
            {
                esMapOffset = 12 + infoLength + esMapLength;
                AudioAttributes audioAttributes = this.title.GetAudioStream(streamId);

                switch(audioAttributes.CodingMode)
                {
                case AudioCodingMode.MPEG1:
                case AudioCodingMode.MPEG2:
                    psm[esMapOffset] = 0x04;
                    psm[esMapOffset + 1] = (byte)(streamId & 0xff);
                    break;
                case AudioCodingMode.AC3:
                case AudioCodingMode.DTS:
                case AudioCodingMode.LPCM:
                default:
                    psm[esMapOffset] = 0x81;
                    //psm[esMapOffset + 1] = (byte)(streamId & 0xff);
                    psm[esMapOffset + 1] = 0x81;
                    break;
                }
                if((audioAttributes.Language == null) || (audioAttributes.Language.Length < 2))
                {
                    psm[esMapOffset + 2] = 0;
                    psm[esMapOffset + 3] = 0;
                    esMapLength += 4;
                }
                else
                {
                    psm[esMapOffset + 2] = 0;
                    psm[esMapOffset + 3] = 6;
                    psm[esMapOffset + 4] = 0x0a;
                    psm[esMapOffset + 5] = 4;

                    byte[] lang;
                    if(audioAttributes.Language.Length > 2)
                    {
                        lang = Encoding.ASCII.GetBytes(audioAttributes.Language);
                    }
                    else
                    {
                        string languageCode = DvdLanguageCodes.GetLanguage639Code(
                            audioAttributes.Language);
                        lang = Encoding.ASCII.GetBytes(languageCode);
                    }

                    psm[esMapOffset + 6] = lang[0];
                    psm[esMapOffset + 7] = lang[1];
                    psm[esMapOffset + 8] = lang[2];

                    psm[esMapOffset + 9] = 0;
                    esMapLength += 10;
                }
            }
            
            psm[10 + infoLength] = (byte)(esMapLength >> 8);
            psm[11 + infoLength] = (byte)(esMapLength & 0xff);
            psmLength += esMapLength;

            psm[4] = (byte)(psmLength >> 8);
            psm[5] = (byte)(psmLength & 0xff);

            byte[] realPsm = new byte[psmLength + 6];
            Buffer.BlockCopy(psm, 0, realPsm, 0, realPsm.Length);

            realPsm[realPsm.Length - 4] = 0;
            realPsm[realPsm.Length - 3] = 0;
            realPsm[realPsm.Length - 2] = 0;
            realPsm[realPsm.Length - 1] = 0;

            uint crc = 0xffffffff;
            for(int i = 0; i < realPsm.Length; i++)
            {
                crc = (crc << 8) ^ crc32_table[((crc >> 24) ^ realPsm[i]) & 0xff];
            }

            realPsm[realPsm.Length - 4] = (byte)(crc >> 24);
            realPsm[realPsm.Length - 3] = (byte)((crc >> 16) & 0xff);
            realPsm[realPsm.Length - 2] = (byte)((crc >> 8) & 0xff);
            realPsm[realPsm.Length - 1] = (byte)(crc & 0xff);
            return realPsm;
        }
    }
}
