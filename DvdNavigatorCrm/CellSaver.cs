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
    class CellSaver
    {
        Dictionary<int, SubtitlePacker> packers = new Dictionary<int, SubtitlePacker>();
        double ptsOffset;
        int writeCountdown = 1000;
        double? startPts;
        double? endPts;
        int sectorOffset;
        const int SectorSize = 2048;
        byte[] paddingPacket = new byte[SectorSize];
        const int PacketHeaderSize = 6;
        bool headerWrittenLast;

        public CellSaver(double ptsOffset, PartialSaveStatus lastCellStatus, double? startPts, double? endPts)
        {
            this.ptsOffset = ptsOffset;
            if(lastCellStatus == PartialSaveStatus.AfterEnd)
            {
                throw new ArgumentException("lastCellStatus cannot be AfterEnd at start");
            }
            this.PartialSaveStatus = lastCellStatus;
            this.startPts = startPts;
            this.endPts = endPts;
            if(this.startPts.HasValue != this.endPts.HasValue)
            {
                throw new ArgumentException("startPts and endPts must either both have values or neither");
            }
            paddingPacket[0] = 0;
            paddingPacket[1] = 0;
            paddingPacket[2] = 1;
            paddingPacket[3] = MpegPSSplitter.PaddingStreamCode;
            for(int index = 6; index < this.paddingPacket.Length; index++)
            {
                this.paddingPacket[index] = 0xff;
            }
        }

        public double? FirstPackHeaderPts { get; private set; }

        public event EventHandler SavedPacket;

        public PartialSaveStatus PartialSaveStatus { get; private set; }

        void OnSavedPacket()
        {
            EventHandler tempHandler = SavedPacket;
            if(tempHandler != null)
            {
                tempHandler(this, EventArgs.Empty);
            }
        }

        void WritePaddingPacket(FileStream mpegOut, int length)
        {
            int paddingLength = length - 6;
            paddingPacket[4] = (byte)(paddingLength >> 8);
            paddingPacket[5] = (byte)(paddingLength & 0xff);
            mpegOut.Write(paddingPacket, 0, length);
        }

        void WriteMpegPacket(FileStream mpegOut, byte[] data)
        {
            if((this.sectorOffset + data.Length != SectorSize) &&
                (this.sectorOffset + data.Length + PacketHeaderSize > SectorSize))
            {
                WritePaddingPacket(mpegOut, SectorSize - this.sectorOffset);
                this.sectorOffset = 0;
            }
            mpegOut.Write(data, 0, data.Length);
            this.sectorOffset += data.Length;
        }

        public void Run(IList<PacketBuffer> packets, FileStream mpegOut,
            ISubtitleStorage storage, Func<bool> stopFunc)
        {
            try
            {
                int packetIndex = 0;
                foreach(PacketBuffer buffer in packets)
                {
                    if(stopFunc())
                    {
                        return;
                    }

                    if(buffer.DataHolder != null)
                    {
                        buffer.DataHolder.LoadInMemory();
                    }
                    try
                    {
                        OnSavedPacket();
                        packetIndex++;
                        if(packetIndex > packets.Count - 1000)
                        {
                            this.writeCountdown = 1000;
                        }

                        StreamPackHeaderBuffer packHeader = buffer as StreamPackHeaderBuffer;
                        if(packHeader != null)
                        {
                            double dScr = OffsetPackHeader(packHeader);
                            if(!this.FirstPackHeaderPts.HasValue)
                            {
                                this.FirstPackHeaderPts = dScr;
                            }
                            switch(this.PartialSaveStatus)
                            {
                            case PartialSaveStatus.BeforeStart:
                                if(dScr >= this.startPts.Value)
                                {
                                    if(dScr <= this.endPts.Value)
                                    {
                                        this.PartialSaveStatus = PartialSaveStatus.InRange;
                                    }
                                    else
                                    {
                                        this.PartialSaveStatus = PartialSaveStatus.AfterEnd;
                                    }
                                }
                                break;
                            case PartialSaveStatus.InRange:
                                if(this.endPts.HasValue && (dScr > this.endPts.Value))
                                {
                                    this.PartialSaveStatus = PartialSaveStatus.AfterEnd;
                                    return;
                                }
                                break;
                            }

                            if((this.PartialSaveStatus == PartialSaveStatus.InRange) &&
                                (mpegOut != null) && !this.headerWrittenLast)
                            {
                                if(this.sectorOffset != 0)
                                {
                                    WritePaddingPacket(mpegOut, SectorSize - this.sectorOffset);
                                    this.sectorOffset = 0;
                                }
                                WriteMpegPacket(mpegOut, packHeader.DataHolder.Data);
                                this.headerWrittenLast = true;
                            }
                        }

                        if(this.PartialSaveStatus != PartialSaveStatus.InRange)
                        {
                            continue;
                        }

                        PalettePacketBuffer palette = buffer as PalettePacketBuffer;
                        if((palette != null) && (storage != null))
                        {
                            storage.AddPalette(palette.Palette);
                        }
                        HeaderPacketBuffer header = buffer as HeaderPacketBuffer;
                        if((header != null) && (mpegOut != null))
                        {
                            WriteMpegPacket(mpegOut, header.DataHolder.Data);
                            this.headerWrittenLast = false;
                        }
                        StreamPacketBuffer stream = buffer as StreamPacketBuffer;
                        if(stream != null)
                        {
                            IStreamDefinition defn = stream.StreamDefinition;
                            double? pts = OffsetBuffer(stream);
                            switch(defn.StreamType)
                            {
                            case StreamType.Subtitle:
                                if(storage != null)
                                {
                                    SubtitlePacker packer;
                                    if(!this.packers.TryGetValue(defn.StreamId, out packer))
                                    {
                                        packer = new SubtitlePacker(defn.StreamId, storage);
                                        this.packers[defn.StreamId] = packer;
                                    }
                                    CombinedBuffer combo = new CombinedBuffer(stream.DataHolder.Data);
                                    packer.HandleBytes(combo, stream.PesHeaderLength,
                                        stream.DataHolder.Length - stream.PesHeaderLength, pts, 0);
                                }
                                break;
                            case StreamType.Audio:
                            case StreamType.Video:
                                if(mpegOut != null)
                                {
                                    WriteMpegPacket(mpegOut, stream.DataHolder.Data);
                                    this.headerWrittenLast = false;
                                }
                                break;
                            }
                        }
                    }
                    finally
                    {
                        if(buffer.DataHolder != null)
                        {
                            buffer.DataHolder.ReleaseFromMemory();
                        }
                    }
                }
            }
            finally
            {
                if((mpegOut != null) && (this.sectorOffset != 0))
                {
                    WritePaddingPacket(mpegOut, SectorSize - this.sectorOffset);
                    this.sectorOffset = 0;
                }
            }
        }

        double OffsetPackHeader(StreamPackHeaderBuffer packHeader)
        {
            double dScr = 0;
            byte[] data = packHeader.DataHolder.Data;
            if((data[4] >> 6) == 0x01)
            {
                ulong scr = (
                    (((ulong)data[4] & 0x38) << 27) |
                    (((ulong)data[4] & 0x03) << 28) |
                    (((ulong)data[5]) << 20) |
                    (((ulong)data[6] & 0xf8) << 12) |
                    (((ulong)data[6] & 0x03) << 13) |
                    (((ulong)data[7]) << 5) |
                    (((ulong)data[8]) >> 3)
                ) * 300L + (
                    (((ulong)data[8] & 0x03) << 7) |
                    (((ulong)data[9] & 0xfe) >> 1));
                if(this.writeCountdown > 0)
                {
                    this.writeCountdown--;
                }
                dScr = Math.Max(0, Convert.ToDouble(scr) / (300.0 * 90.0) + this.ptsOffset);
                ulong newScr = Convert.ToUInt64(dScr * 90.0 * 300.0);
                ulong extension = newScr % 300;
                newScr = (newScr - extension) / 300;
                data[4] = (byte)((byte)((newScr >> 27) & 0x38) | (data[4] & 0xc4) |
                    ((byte)((newScr >> 28) & 0x03)));
                data[5] = (byte)((newScr >> 20) & 0xff);
                data[6] = (byte)((byte)((newScr >> 12) & 0xf8) | (data[6] & 0x04) |
                    ((byte)((newScr >> 13) & 0x03)));
                data[7] = (byte)((newScr >> 5) & 0xff);
                data[8] = (byte)((byte)((newScr << 3) & 0xf8) | (data[8] & 0x04) |
                    ((byte)((extension >> 7) & 0x03)));
                data[9] = (byte)((byte)((extension << 1) & 0xfe) | (data[9] & 0x01));

                /*uint muxRate = ((uint)data[10] << 14) |
                    ((uint)data[11] << 6) |
                    ((uint)data[12] >> 2);
                muxRate = Convert.ToUInt32(muxRate * 1.1);
                data[10] = (byte)((muxRate >> 14) & 0xff);
                data[11] = (byte)((muxRate >> 6) & 0xff);
                data[12] = (byte)((byte)((muxRate << 2) & 0xfc) | (data[12] & 0x03));*/
            }
            else
            {
                ulong scr = (
                    (((ulong)data[4] & 0x0e) << 29) |
                    (((ulong)data[5]) << 22) |
                    (((ulong)data[6] & 0xfe) << 14) |
                    (((ulong)data[7]) << 7) |
                    (((ulong)data[8]) >> 1)
                ) * 300L;
                dScr = Math.Max(0, Convert.ToDouble(scr) / (300.0 * 90.0) + this.ptsOffset);
                ulong newScr = Convert.ToUInt64(dScr * 90.0 * 300.0);
                ulong extension = newScr % 300;
                newScr = (newScr - extension) / 300;
                data[4] = (byte)((byte)((newScr >> 29) & 0x0e) | (data[4] & 0xf1));
                data[5] = (byte)((newScr >> 22) & 0xff);
                data[6] = (byte)((byte)((newScr >> 14) & 0xfe) | (data[6] & 0x01));
                data[7] = (byte)((newScr >> 7) & 0xff);
                data[8] = (byte)((byte)((newScr << 1) & 0xfe) | (data[8] & 0x01));
            }
            return dScr;
        }

        double? OffsetBuffer(StreamPacketBuffer streamBuffer)
        {
            double? dPts = null;
            byte[] data = streamBuffer.DataHolder.Data;
            int ptsDtsFlag = data[7] & 0xc0;
            if((ptsDtsFlag & 0x80) != 0)
            {
                ulong pts = (((ulong)data[9] & 0x0e) << 29) |
                    ((ulong)data[10] << 22) |
                    (((ulong)data[11] & 0xfe) << 14) |
                    ((ulong)data[12] << 7) |
                    ((ulong)data[13] >> 1);

                if(this.writeCountdown > 0)
                {
                    this.writeCountdown--;
                }

                double offset = this.ptsOffset;
                dPts = Math.Max(0, Convert.ToDouble(pts) / 90.0 + offset);
                ulong newPts = Convert.ToUInt64(dPts * 90.0);
                data[9] = (byte)((byte)((newPts >> 29) & 0x0e) | (data[9] & 0xf1));
                data[10] = (byte)((newPts >> 22) & 0xff);
                data[11] = (byte)((byte)((newPts >> 14) & 0xfe) | (data[11] & 0x01));
                data[12] = (byte)((newPts >> 7) & 0xff);
                data[13] = (byte)((byte)((newPts << 1) & 0xff) | (data[13] & 0x01));

                if((ptsDtsFlag & 0x40) != 0)
                {
                    ulong dts = (((ulong)data[14] & 0x0e) << 29) |
                        ((ulong)data[15] << 22) |
                        (((ulong)data[16] & 0xfe) << 14) |
                        ((ulong)data[17] << 7) |
                        ((ulong)data[18] >> 1);
                    double dDts = Math.Max(0, Convert.ToDouble(dts) / 90.0 + offset);
                    ulong newDts = Convert.ToUInt64(dDts * 90.0);
                    data[14] = (byte)((byte)((newDts >> 29) & 0x0e) | (data[14] & 0xf1));
                    data[15] = (byte)((newDts >> 22) & 0xff);
                    data[16] = (byte)((byte)((newDts >> 14) & 0xfe) | (data[16] & 0x01));
                    data[17] = (byte)((newDts >> 7) & 0xff);
                    data[18] = (byte)((byte)((newDts << 1) & 0xff) | (data[18] & 0x01));
                }
            }
            return dPts;
        }
    }
}
