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
    public class CellLoader : IBasicSplitterCallback
    {
        const int MaximumReadSize = 1 << 16;
        List<PacketBuffer> packets = new List<PacketBuffer>();
        List<PacketBuffer> savedPackets = new List<PacketBuffer>();
        List<CellIdVobId> vobCellIds = new List<CellIdVobId>();
        List<CellIdVobId> angleCellIds = new List<CellIdVobId>();
        class AudioStreamData
        {
            public AudioStreamItem Item { get; set; }
            public double? Pts { get; set; }
            public int BytesSincePts { get; set; }
            public double LastPts { get; set; }
        }
        Dictionary<int, AudioStreamData> audioData = new Dictionary<int, AudioStreamData>();
        List<int> audioStreamsFound = new List<int>();
        CombinedBuffer buffer = new CombinedBuffer();
        bool discardDataDueToAngle;
        bool? wasAngle;
        ulong previousScr;
        bool acceptAnyAudio;
        MpegPSSplitter splitter;
        int memoryDataUsed;
        const int MaxMemoryUsed = 1 << 29;
        FileStream dataFile;

        class ChunkData
        {
            public DataBuffer DataBuffer { get; set; }
            public IList<int> Palette { get; set; }
        }

        public CellLoader(IList<CellIdVobId> vobCells, IList<CellIdVobId> angleCells, IEnumerable<AudioStreamItem> audioStreams)
        {
            this.splitter = new MpegPSSplitter(TitleChunk.MaxTitleChunkLength, this);
            this.AudioStreamsFound = this.audioStreamsFound.AsReadOnly();

            if(vobCells != null)
            {
                this.vobCellIds.AddRange(vobCells);
            }
            if(angleCells != null)
            {
                this.angleCellIds.AddRange(angleCells);
            }

            if(audioStreams == null)
            {
                this.acceptAnyAudio = true;
            }
            else
            {
                foreach(AudioStreamItem item in audioStreams)
                {
                    if(item.KBitsPerSecond != 0)
                    {
                        this.audioData[item.StreamId] = new AudioStreamData() { Item = item };
                    }
                }
            }
        }

        public double? FirstPts { get; private set; }
        public double? FirstVideoPts { get; private set; }
        public double? FirstAudioPts { get; private set; }
        public double? LastPts { get; private set; }
        public IList<PacketBuffer> Packets { get { return packets; } }
        public IList<int> AudioStreamsFound { get; private set; }
        public SortedDictionary<int, long> AudioLengths = new SortedDictionary<int, long>();

        public void Stop()
        {
            this.Stopped = true;
        }

        public bool Stopped { get; private set; }

        public event EventHandler<LoadedBytesEventArgs> BytesRead;

        void OnBytesRead(int byteCount)
        {
            EventHandler<LoadedBytesEventArgs> tempHandler = BytesRead;
            if(tempHandler != null)
            {
                tempHandler(this, new LoadedBytesEventArgs(byteCount));
            }
        }

        public void Run(IList<TitleChunk> chunks, Func<bool> stopFunc)
        {
            long position = 0L;
            IList<int> palette = null;
            foreach(ChunkData data in ReadData(chunks, MaximumReadSize, delegate() { return this.Stopped; }))
            {
                if(stopFunc())
                {
                    return;
                }
                OnBytesRead(data.DataBuffer.Length);
                buffer.AddBuffer(data.DataBuffer, position);

                bool newPalette = true;
                if((palette != null) && (palette.Count == data.Palette.Count))
                {
                    newPalette = false;
                    for(int index = 0; index < palette.Count; index++)
                    {
                        if(palette[index] != data.Palette[index])
                        {
                            newPalette = true;
                            break;
                        }
                    }
                }
                if(newPalette)
                {
                    palette = data.Palette;
                    this.packets.Add(new PalettePacketBuffer() { Palette = palette });
                }

                DemuxResult result = new DemuxResult() { Status = DemuxStatus.Success, BytesUsed = 0 };
                while(result.Status == DemuxStatus.Success)
                {
                    if(this.Stopped)
                    {
                        return;
                    }

                    result = this.splitter.DemuxBuffer(buffer, buffer.Position);
                    IEnumerable<IDataBuffer> releasedBuffers =
                        buffer.MovePositionForwardAndReturnUnusedBuffers(result.BytesUsed);
                    if(releasedBuffers.Count() != 0)
                    {
                        releasedBuffers = null;
                        GC.Collect(0);
                    }
                }

                if(result.Status == DemuxStatus.InvalidBytes)
                {
                    throw new InvalidDataException("Not valid Mpeg2 PS stream data");
                }
                if(result.Status == DemuxStatus.End)
                {
                    break;
                }
            }
        }

        public void ClearPackets()
        {
            this.packets.Clear();
            this.savedPackets.Clear();
            if(this.dataFile != null)
            {
                this.dataFile.Close();
                string fileName = this.dataFile.Name;
                this.dataFile.Dispose();
                File.Delete(fileName);
                this.dataFile = null;
            }
        }

        static byte[] AllocateBuffer(int length)
        {
            byte[] data;
            try
            {
                data = new byte[length];
            }
            catch(OutOfMemoryException)
            {
                GC.Collect();
                data = new byte[length];
            }
            return data;
        }

        static IEnumerable<ChunkData> ReadData(IList<TitleChunk> chunks, int maximumReadSize, Func<bool> stopCheck)
        {
            FileStream fileStream = null;
            try
            {
                int chunkIndex = 0;
                int chunkRead = 0;
                ChunkData chunkData = null;
                while(chunkIndex < chunks.Count)
                {
                    TitleChunk chunk = chunks[chunkIndex];
                    if((fileStream == null) || (fileStream.Name != chunk.FilePath))
                    {
                        if(fileStream != null)
                        {
                            fileStream.Close();
                        }
                        fileStream = new FileStream(chunk.FilePath, FileMode.Open, FileAccess.Read);
                    }
                    if(fileStream.Position != chunk.StartOffset + chunkRead)
                    {
                        fileStream.Seek(chunk.StartOffset + chunkRead, SeekOrigin.Begin);
                    }

                    int readSize = Math.Min(maximumReadSize, chunk.Length - chunkRead);
                    byte[] data = AllocateBuffer(readSize);
                    //long filePosition = fileStream.Position;
                    //Debug.WriteLine(string.Format("BeginRead {0} from {1} to {2} len {3}", Path.GetFileNameWithoutExtension(fileStream.Name), 
                    //    filePosition, filePosition + readSize, readSize));
                    IAsyncResult result = fileStream.BeginRead(data, 0, readSize, null, null);
                    if(stopCheck())
                    {
                        yield break;
                    }
                    if(chunkData != null)
                    {
                        yield return chunkData;
                        chunkData = null;
                        if(stopCheck())
                        {
                            yield break;
                        }
                    }

                    int bytesRead = fileStream.EndRead(result);
                    if(stopCheck())
                    {
                        yield break;
                    }

                    if(bytesRead != 0)
                    {
                        //Debug.WriteLine(string.Format("EndRead {0} from {1} to {2} len {3}", Path.GetFileNameWithoutExtension(fileStream.Name), filePosition,
                        //    filePosition + bytesRead, readSize));

                        chunkData = new ChunkData()
                        {
                            DataBuffer = new DataBuffer(data, 0, bytesRead),
                            Palette = chunk.SubtitlePalette,
                        };
                    }

                    chunkRead += bytesRead;
                    if(chunkRead == chunk.Length)
                    {
                        chunkRead = 0;
                        chunkIndex++;
                    }
                }

                if(chunkData != null)
                {
                    yield return chunkData;
                    chunkData = null;
                }
            }
            finally
            {
                if(fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }

        void IBasicSplitterCallback.Discontinuity()
        {
        }

        void IBasicSplitterCallback.StreamFound(IStreamDefinition stream)
        {
            if(stream.StreamType == StreamType.Audio)
            {
                if(!this.audioStreamsFound.Contains(stream.StreamId))
                {
                    this.audioStreamsFound.Add(stream.StreamId);
                }
            }
        }

        DataHolder HoldData(CombinedBuffer buffer, int index, int length)
        {
            DataHolder holder;
            if(this.memoryDataUsed + length > MaxMemoryUsed)
            {
                if(this.dataFile == null)
                {
                    this.dataFile = new FileStream(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite);
                }
                buffer.CopyTo(index, this.dataFile, length);
                holder = new DataHolder(this.dataFile, length);
            }
            else
            {
                byte[] data = AllocateBuffer(length);
                buffer.CopyTo(index, data, 0, length);
                holder = new DataHolder(data);
                this.memoryDataUsed += length;
            }
            return holder;
        }

        void IBasicSplitterCallback.StreamPackHeader(ulong scr, uint muxRate, int packetOffset, int packetLength, long bufferPosition)
        {
            //Debug.WriteLine(string.Format("StreamPackHeader {0} Scr {1}",
            //    this.discardDataDueToAngle ? "DISCARD" : "", scr / (300 * 90)));

            DataHolder holder = HoldData(this.buffer, packetOffset, packetLength);
            StreamPackHeaderBuffer buffer = new StreamPackHeaderBuffer() { DataHolder = holder, SCR = scr, MuxRate = muxRate };
            if(!this.discardDataDueToAngle)
            {
                UpdateFirstPts(Convert.ToDouble(scr) / (300.0 * 90.0));
                if((this.previousScr != 0) && (scr > this.previousScr))
                {
                    UpdateLastPts(Convert.ToDouble(scr + this.previousScr) / (2.0 * 300.0 * 90.0));
                }
                this.previousScr = scr;
                this.packets.Add(buffer);
            }
            else
            {
                this.savedPackets.Add(buffer);
            }
        }

        void IBasicSplitterCallback.HeaderPacket(int packetTypeCode, int packetOffset, int packetLength, long bufferPosition)
        {
            DataHolder holder = HoldData(this.buffer, packetOffset, packetLength);
            HeaderPacketBuffer buffer = new HeaderPacketBuffer() { DataHolder = holder, PacketTypeCode = packetTypeCode };
            if(!this.discardDataDueToAngle)
            {
                this.packets.Add(buffer);
            }
            else
            {
                this.savedPackets.Add(buffer);
            }
        }

        void DealWithAudio(IStreamDefinition streamDef, int packetOffset, int packetLength, long bufferPosition, int pesHeaderLength)
        {
            long lastLength;
            AudioLengths.TryGetValue(streamDef.StreamId, out lastLength);
            AudioLengths[streamDef.StreamId] = lastLength + packetLength - pesHeaderLength;
        }

        void IBasicSplitterCallback.StreamPacket(IStreamDefinition streamDef, int packetOffset, int packetLength, long bufferPosition, int pesHeaderLength)
        {
            if(streamDef.StreamType == StreamType.Audio)
            {
                DealWithAudio(streamDef, packetOffset, packetLength, bufferPosition, pesHeaderLength);
            }
            if(!this.discardDataDueToAngle)
            {
                if(streamDef.StreamType == StreamType.Audio)
                {
                    if(!this.acceptAnyAudio && !this.audioData.ContainsKey(streamDef.StreamId))
                    {
                        return;
                    }
                }
                DataHolder holder = HoldData(this.buffer, packetOffset, packetLength);
                this.packets.Add(new StreamPacketBuffer() { DataHolder = holder, StreamDefinition = streamDef, PesHeaderLength = pesHeaderLength });
            }
            else
            {
                this.savedPackets.Clear();
            }
        }

        void UpdateFirstPts(double? pts)
        {
            if(pts.HasValue)
            {
                if(this.FirstPts.HasValue)
                {
                    this.FirstPts = Math.Min(this.FirstPts.Value, pts.Value);
                }
                else
                {
                    this.FirstPts = pts;
                }
            }
        }

        void UpdateLastPts(double? pts)
        {
            if(pts.HasValue)
            {
                if(this.LastPts.HasValue)
                {
                    this.LastPts = Math.Max(this.LastPts.Value, pts.Value);
                }
                else
                {
                    this.LastPts = pts;
                }
            }
        }

        void IBasicSplitterCallback.AngleCellChanged(bool isAngle, CellIdVobId cellVobId, long bufferPosition)
        {
            if(isAngle)
            {
                if(this.angleCellIds.Contains(cellVobId) || ((this.angleCellIds.Count == 0) && this.vobCellIds.Contains(cellVobId)))
                {
                    if(this.savedPackets.Count != 0)
                    {
                        if(this.discardDataDueToAngle)
                        {
                            this.packets.AddRange(this.savedPackets);
                        }
                        this.savedPackets.Clear();
                    }
                    if(!this.wasAngle.HasValue || this.wasAngle.Value)
                    {
                        Debug.WriteLine(string.Format("Angle Good {0}", cellVobId.ToString()));
                        this.wasAngle = false;
                    }
                    this.discardDataDueToAngle = false;
                }
                else
                {
                    if(!this.discardDataDueToAngle)
                    {
                        while(this.packets.Count > 0)
                        {
                            StreamPackHeaderBuffer oldStreamHeader = this.packets[this.packets.Count - 1] as StreamPackHeaderBuffer;
                            HeaderPacketBuffer oldHeader = this.packets[this.packets.Count - 1] as HeaderPacketBuffer;
                            if((oldStreamHeader != null) || (oldHeader != null))
                            {
                                this.packets.RemoveAt(this.packets.Count - 1);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if(!this.wasAngle.HasValue || !this.wasAngle.Value)
                    {
                        Debug.WriteLine(string.Format("Angle Discard {0}", cellVobId.ToString()));
                        this.wasAngle = true;
                    }
                    this.discardDataDueToAngle = true;
                }
            }
            else
            {
                if(this.savedPackets.Count != 0)
                {
                    if(this.discardDataDueToAngle)
                    {
                        this.packets.AddRange(this.savedPackets);
                    }
                    this.savedPackets.Clear();
                }
                if(this.wasAngle.HasValue)
                {
                    Debug.WriteLine("No Angle");
                    this.wasAngle = null;
                }
                this.discardDataDueToAngle = false;
            }
        }

        void IBasicSplitterCallback.StreamData(IStreamDefinition stream, int packetOffset, int packetLength,
            double? pts, long bufferPosition)
        {
            if(this.discardDataDueToAngle)
            {
                return;
            }

            switch(stream.StreamType)
            {
            case StreamType.Video:
                UpdateFirstPts(pts);
                if(pts.HasValue)
                {
                    if(!this.FirstVideoPts.HasValue)
                    {
                        this.FirstVideoPts = pts.Value;
                    }
                    else
                    {
                        this.FirstVideoPts = Math.Min(pts.Value, this.FirstVideoPts.Value);
                    }
                }
                else
                {
                    if(!this.FirstVideoPts.HasValue)
                    {
                        Debug.WriteLine("Video before first pts");
                    }
                }
                break;
            case StreamType.Subtitle:
                UpdateFirstPts(pts);
                break;
            case StreamType.Audio:
                UpdateFirstPts(pts);
                if(pts.HasValue)
                {
                    if(!this.FirstAudioPts.HasValue)
                    {
                        this.FirstAudioPts = pts.Value;
                    }
                    else
                    {
                        this.FirstAudioPts = Math.Min(pts.Value, this.FirstAudioPts.Value);
                    }
                }
                else
                {
                    if(!this.FirstAudioPts.HasValue)
                    {
                        Debug.WriteLine("Audio before first pts");
                    }
                }
                if(this.acceptAnyAudio && !this.audioData.ContainsKey(stream.StreamId))
                {
                    AudioAttributes attribs = new AudioAttributes()
                    { Channels = 2, CodeExtension = AudioCodeExtension.Normal, 
                        CodingMode = AudioCodingMode.AC3, Language = "en", 
                        TrackId = this.audioData.Count + 1 };
                    this.audioData[stream.StreamId] = new AudioStreamData() 
                    { Item = new AudioStreamItem(stream.StreamId, attribs) };
                }

                AudioStreamData data;
                if(this.audioData.TryGetValue(stream.StreamId, out data))
                {
                    if(pts.HasValue)
                    {
                        if(!data.Pts.HasValue && (data.BytesSincePts != 0))
                        {
                            double audioPts = pts.Value -
                                (double)data.BytesSincePts /
                                    ((double)data.Item.KBitsPerSecond / 8.0);
                            UpdateFirstPts(audioPts);
                        }
                        data.Pts = pts.Value;
                        data.BytesSincePts = 0;
                    }

                    data.BytesSincePts += packetLength;
                    if(data.Pts.HasValue)
                    {
                        double audioPts = data.Pts.Value
                            + (double)data.BytesSincePts / (2 * ((double)data.Item.KBitsPerSecond / 8.0));
                        //UpdateLastPts(audioPts);
                        data.LastPts = audioPts;
                    }
                }
                break;
            }
        }

        public event EventHandler<TraceEventArgs> SplitterTrace;

        void OnSplitterTrace(string text)
        {
            EventHandler<TraceEventArgs> tempHandler = this.SplitterTrace;
            if(tempHandler != null)
            {
                tempHandler(this, new TraceEventArgs(text));
            }
        }

        void IBasicSplitterCallback.PacketTrace(string text)
        {
            OnSplitterTrace(text);
        }
    }
}
