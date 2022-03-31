/*
 * Copyright (C) 2007, 2008, 2009 Chris Meadowcroft <crmeadowcroft@gmail.com>
 *
 * This file is part of CMPlayer, a free video player.
 * See http://sourceforge.net/projects/crmplayer for updates.
 *
 * CMPlayer is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * CMPlayer is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace DvdNavigatorCrm
{
    public class MpegPSSplitter : IBasicSplitter
    {
        //static TraceCrm tracer = TraceCrm.CreateTracer(typeof(MpegPSSplitter));

        public const int NormalMaxBytesWithoutPacket = 500000;

        public const byte EndCode = 0xb9;
        public const byte PackHeaderCode = 0xba;
        public const byte SystemHeaderCode = 0xbb;
        public const byte ProgramStreamMapCode = 0xbc;
        public const byte PrivateStream1Code = 0xbd;
        public const byte PaddingStreamCode = 0xbe;
        public const byte PrivateStream2Code = 0xbf;
        public const byte EcmStreamCode = 0xf0;
        public const byte EmmStreamCode = 0xf1;
        public const byte ProgramStreamDirectoryCode = 0xff;
        public const byte DvdSectorFillCode = 0x01;
        public const byte DsiSubstreamId = 1;

        public const int PacketHeaderLength = 4;

        protected enum KnownStreamIds
        {
            AllAudioStreamId = 0xb8,
            AllVideoStreamId = 0xb9,
            EndCode = 0xb9,
            PackHeaderCode = 0xba,
            SystemHeaderCode = 0xbb,
            ProgramStreamMapCode = 0xbc,
            PrivateStream1Code = 0xbd,
            PaddingStreamCode = 0xbe,
            PrivateStream2Code = 0xbf,
            EcmStreamCode = 0xf0,
            EmmStreamCode = 0xf1,
            ProgramStreamDirectoryCode = 0xff,
            DvdSectorFillCode = 0x01,
        }

        IByteBuffer buffer;
        int bytesLeft;
        int bufferOffset;
        long positionAtStartOfBuffer;
        Dictionary<int, ProgramStreamMapEntry> programStreamMap = new Dictionary<int, ProgramStreamMapEntry>();
        Dictionary<int, int> bufferSizeBounds = new Dictionary<int, int>();
        Dictionary<int, MpegStreamDefinition> streams = new Dictionary<int, MpegStreamDefinition>();
        Dictionary<int, MpegStreamDefinition> privateStreams = new Dictionary<int, MpegStreamDefinition>();
        int programStreamMapVersion = -1;
        bool audioLockFlag;
        bool videoLockFlag;
        int maxBytesWithoutPacket;
        IBasicSplitterCallback callback;

        public MpegPSSplitter(int maxBytesWithoutPacket, IBasicSplitterCallback callback)
        {
            //tracer.WriteLine(TraceSeverity.Important, "Created {0} maxBytes", maxBytesWithoutPacket);
            this.maxBytesWithoutPacket = maxBytesWithoutPacket;
            this.callback = callback;
        }

        public int MaxBytesWithoutPacket { get { return this.maxBytesWithoutPacket; } }

        protected virtual void OnDiscontinuity()
        {
            if(this.callback != null)
            {
                this.callback.Discontinuity();
            }
        }

        protected virtual void OnStreamFound(IStreamDefinition stream)
        {
            if(this.callback != null)
            {
                this.callback.StreamFound(stream);
            }
        }

        protected virtual void OnAngleCellChanged(bool isAngle, CellIdVobId cellVobId)
        {
            if(this.callback != null)
            {
                this.callback.AngleCellChanged(isAngle, cellVobId, this.BufferPosition);
            }
        }

        protected virtual void OnStreamData(IStreamDefinition stream, int packetOffset, 
            int packetLength, double? pts)
        {
            if(this.callback != null)
            {
                this.callback.StreamData(stream, packetOffset, packetLength, pts,
                    this.BufferPosition);
            }
        }

        protected virtual void OnStreamPackHeader(ulong scr, uint muxRate, int packetLength)
        {
            if(this.callback != null)
            {
                this.callback.StreamPackHeader(scr, muxRate, this.bufferOffset, packetLength, this.BufferPosition);
            }
        }

        protected virtual void OnHeaderPacket(int packetTypeCode, int packetLength)
        {
            if(this.callback != null)
            {
                this.callback.HeaderPacket(packetTypeCode, this.bufferOffset, packetLength, this.BufferPosition);
            }
        }

        protected virtual void OnStreamPacket(IStreamDefinition streamDef, int packetLength, int pesHeaderLength)
        {
            if(this.callback != null)
            {
                this.callback.StreamPacket(streamDef, this.bufferOffset, packetLength, this.BufferPosition, pesHeaderLength);
            }
        }

        protected virtual void OnPacketTrace(string text, params object[] traceValues)
        {
            if(this.callback != null)
            {
                if(traceValues.Length != 0)
                {
                    text = string.Format(text, traceValues);
                }
                this.callback.PacketTrace(text);
            }
        }

        public void ExternalDiscontinuity()
        {
        }

        public DemuxResult DemuxBuffer(IByteBuffer dataBuffer, long positionAtStartOfBuffer)
        {
            this.buffer = dataBuffer;
            this.bufferOffset = 0;
            this.bytesLeft = dataBuffer.Count;
            this.positionAtStartOfBuffer = positionAtStartOfBuffer;

            int bytesWithoutPacket = 0;
            DemuxStatus status = DeMultiplexCurrentBuffer();
            if(status == DemuxStatus.InvalidBytes)
            {
                OnDiscontinuity();
                OnPacketTrace("Discontinuity at {0}", BufferPosition);
                while(status == DemuxStatus.InvalidBytes)
                {
                    //tracer.WriteLine(TraceSeverity.Important, "InvalidBytes position {0}", this.BufferPosition);
                    int nextStartCode;
                    if(this.bytesLeft > this.maxBytesWithoutPacket)
                    {
                        nextStartCode = FindPartialStartCode(this.buffer, this.bufferOffset + 1, this.maxBytesWithoutPacket);
                        if(nextStartCode == -1)
                        {
                            return new DemuxResult() { Status = DemuxStatus.InvalidBytes, BytesUsed = 0 };
                        }
                    }
                    else
                    {
                        nextStartCode = FindPartialStartCode(this.buffer, this.bufferOffset + 1, this.bytesLeft - 1);
                        if(nextStartCode == -1)
                        {
                            bytesWithoutPacket += (this.bytesLeft - PacketHeaderLength);
                            if(bytesWithoutPacket >= this.maxBytesWithoutPacket)
                            {
                                return new DemuxResult() { Status = DemuxStatus.InvalidBytes, BytesUsed = 0 };
                            }
                            return new DemuxResult() { Status = DemuxStatus.PartialPacket, BytesUsed = this.bufferOffset };
                        }
                    }
                    bytesWithoutPacket += (nextStartCode - this.bufferOffset);
                    this.bytesLeft -= (nextStartCode - this.bufferOffset);
                    this.bufferOffset = nextStartCode;
                    //tracer.WriteLine(TraceSeverity.Important, "FindStartCode at {0}", BufferPosition);
                    status = DeMultiplexCurrentBuffer();
                }
            }
            return new DemuxResult() { Status = status, BytesUsed = this.bufferOffset };
        }

        protected ulong SystemClockReference { get; private set; }
        protected uint ProgramMuxRate { get; private set; }
        protected long BufferPosition { get { return this.positionAtStartOfBuffer + this.bufferOffset; } }

        static int FindPartialStartCode(IByteBuffer buffer, int nextByteToCheck, int countDown)
        {
            while(countDown >= 3)
            {
                if((buffer[nextByteToCheck + 2] == 1) &&
                    (buffer[nextByteToCheck + 1] == 0) &&
                    (buffer[nextByteToCheck] == 0))
                {
                    return nextByteToCheck;
                }
                countDown--;
                nextByteToCheck++;
            }
            if(buffer[nextByteToCheck + 1] == 0)
            {
                if(buffer[nextByteToCheck] == 0)
                {
                    return nextByteToCheck;
                }
                return nextByteToCheck + 1;
            }
            return -1;
        }

        ProgramStreamMapEntry FindMapEntry(int streamId)
        {
            ProgramStreamMapEntry entry;
            if(!this.programStreamMap.TryGetValue(streamId, out entry))
            {
                entry = new ProgramStreamMapEntry();
                this.programStreamMap[streamId] = entry;
            }
            return entry;
        }

        MpegStreamDefinition FindStream(int id)
        {
            MpegStreamDefinition streamDef;
            if(!this.streams.TryGetValue(id, out streamDef))
            {
                streamDef = new MpegStreamDefinition(id);
                this.streams[id] = streamDef;
            }
            return streamDef;
        }

        MpegStreamDefinition FindPrivateStream(int id)
        {
            MpegStreamDefinition streamDef;
            if(!this.privateStreams.TryGetValue(id, out streamDef))
            {
                streamDef = new MpegStreamDefinition(id);
                this.privateStreams[id] = streamDef;
            }
            return streamDef;
        }

        DemuxStatus DeMultiplexCurrentBuffer()
        {
            if(this.bytesLeft < 0)
            {
                throw new InvalidOperationException("bytesLeft is negative, what the...");
            }

            if(this.bytesLeft < PacketHeaderLength)
            {
                return DemuxStatus.PartialPacket;
            }

            if((this.buffer[this.bufferOffset] != 0) || (this.buffer[this.bufferOffset + 1] != 0) ||
                (this.buffer[this.bufferOffset + 2] != 1))
            {
                //tracer.WriteLine(TraceSeverity.Important, "Invalid Header at {0}", BufferPosition);
                return DemuxStatus.InvalidBytes;
            }

            int packetLength = CalculatedPacketLength();
            if((packetLength == -1) || (packetLength > this.bytesLeft))
            {
                return DemuxStatus.PartialPacket;
            }

            // we want enough buffer to check if a correct packet header follows this packet
            if(this.buffer[this.bufferOffset + 3] != EndCode)
            {
                if(packetLength > this.bytesLeft)
                {
                    return DemuxStatus.PartialPacket;
                }
                if((packetLength + PacketHeaderLength <= this.bytesLeft) &&
                    ((this.buffer[this.bufferOffset + packetLength] != 0) ||
                    (this.buffer[this.bufferOffset + packetLength + 1] != 0) ||
                    (this.buffer[this.bufferOffset + packetLength + 2] != 1)))
                {
                    // something is rotten, the next packet isn't starting correctly
                    //tracer.WriteLine(TraceSeverity.Important, "Next Packet Invalid Header at {0}",
                    //    BufferPosition + packetLength);
                    return DemuxStatus.InvalidBytes;
                }
            }

            switch(ProcessPacket(packetLength))
            {
            case ProcessPacketStatus.StreamEnd:
                return DemuxStatus.End;
            case ProcessPacketStatus.PacketDone:
            case ProcessPacketStatus.InvalidPacketBytes:
                break;
            }
            return DemuxStatus.Success;
        }

        int CalculatedPacketLength()
        {
            switch(this.buffer[this.bufferOffset + 3])
            {
            case EndCode:	// End code
                return 4;
            case DvdSectorFillCode:	// Hacked in empty sector from a DVD source
                return 0x0800;
            case PackHeaderCode:	// Pack Header code
                if((this.bytesLeft >= 14) && ((this.buffer[this.bufferOffset + 4] >> 6) == 0x01))
                {
                    return 14 + (this.buffer[this.bufferOffset + 13] & 0x07);
                }
                if((this.bytesLeft >= 12) && ((this.buffer[this.bufferOffset + 4] >> 4) == 0x02))
                {
                    return 12;
                }
                break;
            default:
                if(this.bytesLeft >= 6)
                {
                    return 6 + (((int)this.buffer[this.bufferOffset + 4] << 8) | (int)this.buffer[this.bufferOffset + 5]);
                }
                break;
            }
            return -1;
        }

        protected virtual ProcessPacketStatus ProcessPacket(int packetLength)
        {
            byte packetTypeCode = this.buffer[this.bufferOffset + 3];

            //tracer.WriteLine(TraceSeverity.Information,
            //    "Packet {0:x} length {1} offset {2} position {3}",
            //    packetTypeCode, packetLength, this.bufferOffset, BufferPosition);

            ProcessPacketStatus packetStatus;
            if((packetTypeCode == PrivateStream1Code) ||
                MpegStreamDefinition.IsAudioStream(packetTypeCode) ||
                MpegStreamDefinition.IsVideoStream(packetTypeCode))
            {
                packetStatus = ParseStreamPacket(packetLength);
            }
            else
            {
                switch(packetTypeCode)
                {
                case EndCode:
                    OnPacketTrace("EndCode at {0} length {1}", BufferPosition, packetLength);
                    OnHeaderPacket(packetTypeCode, packetLength);
                    packetStatus = ProcessPacketStatus.StreamEnd;
                    break;
                case PackHeaderCode:
                    packetStatus = ParsePackHeader(packetLength);
                    break;
                case SystemHeaderCode:
                    OnPacketTrace("SystemHeaderCode at {0} length {1}", BufferPosition, packetLength);
                    OnHeaderPacket(packetTypeCode, packetLength);
                    packetStatus = ParseSystemHeader(packetLength);
                    break;
                case ProgramStreamMapCode:
                    OnPacketTrace("ProgramStreamMapCode at {0} length {1}", BufferPosition, packetLength);
                    OnHeaderPacket(packetTypeCode, packetLength);
                    packetStatus = ParsePsmHeader(packetLength);
                    break;
                case PrivateStream2Code:
                    OnPacketTrace("PrivateStream2Code at {0} length {1}", BufferPosition, packetLength);
                    packetStatus = ParsePrivateStream2Packet(packetLength);
                    break;
                default:
                    //tracer.WriteLine(TraceSeverity.Information, "Unknown Packet Type {0}", packetTypeCode);
                    OnPacketTrace("UnknownPacket at {0} length {1}", BufferPosition, packetLength);
                    packetStatus = ParseUnknownPacket(packetLength);
                    break;
                }
            }

            this.bufferOffset += packetLength;
            this.bytesLeft -= packetLength;
            return packetStatus;
        }

        protected virtual ProcessPacketStatus ParseUnknownPacket(int packetLength)
        {
            return ProcessPacketStatus.PacketDone;
        }

        protected virtual ProcessPacketStatus ParsePackHeader(int packetLength)
        {
            uint muxRate = 0;
            if((packetLength >= 14) && ((this.buffer[this.bufferOffset + 4] >> 6) == 0x01))
            {
                this.SystemClockReference = (
                    (((ulong)this.buffer[this.bufferOffset + 4] & 0x38) << 27) |
                    (((ulong)this.buffer[this.bufferOffset + 4] & 0x03) << 28) |
                    (((ulong)this.buffer[this.bufferOffset + 5]) << 20) |
                    (((ulong)this.buffer[this.bufferOffset + 6] & 0xf8) << 12) |
                    (((ulong)this.buffer[this.bufferOffset + 6] & 0x03) << 13) |
                    (((ulong)this.buffer[this.bufferOffset + 7]) << 5) |
                    (((ulong)this.buffer[this.bufferOffset + 8]) >> 3)
                ) * 300L + (
                    (((ulong)this.buffer[this.bufferOffset + 8] & 0x03) << 7) |
                    (((ulong)this.buffer[this.bufferOffset + 9] & 0xfe) >> 1));
                muxRate = ((uint)this.buffer[this.bufferOffset + 10] << 14) |
                    ((uint)this.buffer[this.bufferOffset + 11] << 6) |
                    ((uint)this.buffer[this.bufferOffset + 12] >> 2);
            }
            else if((packetLength >= 12) && ((this.buffer[this.bufferOffset + 4] >> 4) == 0x02))
            {
                this.SystemClockReference = (
                    (((ulong)this.buffer[this.bufferOffset + 4] & 0x0e) << 29) |
                    (((ulong)this.buffer[this.bufferOffset + 5]) << 22) |
                    (((ulong)this.buffer[this.bufferOffset + 6] & 0xfe) << 14) |
                    (((ulong)this.buffer[this.bufferOffset + 7]) << 7) |
                    (((ulong)this.buffer[this.bufferOffset + 8]) >> 1)
                ) * 300L;
                muxRate = ((uint)this.buffer[this.bufferOffset + 9] << 15) |
                    ((uint)this.buffer[this.bufferOffset + 10] << 7) |
                    ((uint)this.buffer[this.bufferOffset + 11] >> 1);
            }

            OnPacketTrace("PackHeader at {0} length {1} scr {2:f2}",
                BufferPosition, packetLength, this.SystemClockReference / (300.0 * 90.0));

            // some bad streams mess up the mux rate
            if(muxRate > 0)
            {
                this.ProgramMuxRate = muxRate;
            }
            OnStreamPackHeader(this.SystemClockReference, this.ProgramMuxRate, packetLength);

            //clocker.WriteLine(TraceSeverity.Information, "SCR: {0} MUX: {1}", this.SystemClockReference,
            //	this.ProgramMuxRate);
            return ProcessPacketStatus.PacketDone;
        }

        protected virtual ProcessPacketStatus ParseSystemHeader(int packetLength)
        {
            //int rateBound = (((int)this.buffer[this.bufferOffset + 6] & 0x7f) << 15) |
            //		((int)this.buffer[this.bufferOffset + 7] << 7) |
            //		(((int)this.buffer[this.bufferOffset + 8] & 0xfe) >> 1);
            //int audioBound = ((int)this.buffer[this.bufferOffset + 9] & 0xfc) >> 2;
            //bool fixedFlag = ((int)this.buffer[this.bufferOffset + 9] & 0x02) != 0;
            //bool CSPSFlag = ((int)this.buffer[this.bufferOffset + 9] & 0x01) != 0;
            this.audioLockFlag = ((int)this.buffer[this.bufferOffset + 10] & 0x80) != 0;
            this.videoLockFlag = ((int)this.buffer[this.bufferOffset + 10] & 0x40) != 0;
            //tracer.WriteLine(TraceSeverity.Information, "audioLockFlag {0} videoLockFlag {1}", audioLockFlag, videoLockFlag);
            //int videoBound = ((int)this.buffer[this.bufferOffset + 10] & 0x3f);
            //bool packRateRestrictionFlag = ((int)this.buffer[this.bufferOffset + 11] & 0x80) != 0;

            int packetEnd = this.bufferOffset + packetLength;
            for(int pstdOffset = this.bufferOffset + 12;
                (pstdOffset + 2 < packetEnd) && ((this.buffer[pstdOffset] & 0x80) != 0);
                pstdOffset += 3)
            {
                int streamId = this.buffer[pstdOffset];
                bool pstdBigBoundScale = (this.buffer[pstdOffset + 1] & 0x20) != 0;
                int bufferSizeBound = (((int)this.buffer[pstdOffset + 1] & 0x1f) << 8) |
                        ((int)this.buffer[pstdOffset + 2]);
                if(pstdBigBoundScale)
                {
                    bufferSizeBound <<= 10;
                }
                else
                {
                    bufferSizeBound <<= 7;
                }

                int oldBounds;
                if(!this.bufferSizeBounds.TryGetValue(streamId, out oldBounds) ||
                    (this.bufferSizeBounds[streamId] != bufferSizeBound))
                {
                    this.bufferSizeBounds[streamId] = bufferSizeBound;
                    string streamName;
                    try
                    {
                        streamName = Enum.GetName(typeof(KnownStreamIds), streamId);
                    }
                    catch(ArgumentException)
                    {
                        streamName = streamId.ToString("x");
                    }
                    //tracer.WriteLine(TraceSeverity.Important, "StreamId {0} SizeBound {1}k",
                    //    streamName, (bufferSizeBound >> 10));
                }
            }
            return ProcessPacketStatus.PacketDone;
        }

        protected virtual ProcessPacketStatus ParsePsmHeader(int packetLength)
        {
            int version = this.buffer[this.bufferOffset + 6] & 0x1f;
            if(version == this.programStreamMapVersion)
            {
                //tracer.WriteLine(TraceSeverity.Error, "PSM with version {0} repeated at {1}",
                //    version, BufferPosition);
                return ProcessPacketStatus.InvalidPacketBytes;
            }
            this.programStreamMapVersion = version;

            int infoLength = ((int)this.buffer[this.bufferOffset + 8] << 8) +
                (int)this.buffer[this.bufferOffset + 9];
            if(infoLength + 12 > packetLength)
            {
                //tracer.WriteLine(TraceSeverity.Error, "PSM with too long info length at {0}", BufferPosition);
                return ProcessPacketStatus.InvalidPacketBytes;
            }

            int mapOffset = this.bufferOffset + 10 + infoLength;
            int mapLength = ((int)this.buffer[mapOffset] << 8) +
                (int)this.buffer[mapOffset + 1];
            if(infoLength + 10 + mapLength + 6 > packetLength)
            {
                //tracer.WriteLine(TraceSeverity.Error, "PSM with too long map length at {0}", BufferPosition);
                return ProcessPacketStatus.InvalidPacketBytes;
            }

            int crcStart = this.bufferOffset + packetLength - 4;
            mapOffset += 2;
            while(mapOffset < crcStart)
            {
                byte typeCode = this.buffer[mapOffset];
                int streamId = this.buffer[mapOffset + 1];

                ProgramStreamMapEntry entry = FindMapEntry(streamId);
                entry.TypeCode = typeCode;

                int streamInfoLength = ((int)this.buffer[mapOffset + 2] << 8) +
                    (int)this.buffer[mapOffset + 3];

                mapOffset += 4;
                if(streamInfoLength >= 2)
                {
                    if(mapOffset + streamInfoLength > crcStart)
                    {
                        //tracer.WriteLine(TraceSeverity.Error, "PSM with too long stream info length at {0}",
                        //    BufferPosition);
                        return ProcessPacketStatus.InvalidPacketBytes;
                    }

                    int descriptorOffset = mapOffset;
                    byte[] descriptor = new byte[streamInfoLength];
                    this.buffer.CopyTo(descriptorOffset, descriptor, 0, streamInfoLength);
                    entry.SetDescriptor(descriptor);

                    while(descriptorOffset + 2 <= crcStart)
                    {
                        byte descriptorType = this.buffer[descriptorOffset];
                        int descriptorLength = this.buffer[descriptorOffset + 1];
                        if((descriptorType == 0x0a) && (descriptorLength >= 3))
                        {
                            byte[] langBytes = new byte[3];
                            this.buffer.CopyTo(descriptorOffset + 2, langBytes, 0, 3);
                            entry.Lang = Encoding.ASCII.GetString(langBytes);
                        }
                        descriptorOffset += descriptorLength + 2;
                    }
                }
                mapOffset += streamInfoLength;
            }

#if CHECKCRC
			uint crc = ((uint)this.buffer[crcStart] << 24) |
				((uint)this.buffer[crcStart + 1] << 16) |
				((uint)this.buffer[crcStart + 2] << 8) |
				(uint)this.buffer[crcStart + 3];
#endif
            return ProcessPacketStatus.PacketDone;
        }

        static UInt32 GetUInt32(IByteBuffer buffer, int offset)
        {
            return (Convert.ToUInt32(buffer[offset]) << 24) +
                (Convert.ToUInt32(buffer[offset + 1]) << 16) +
                (Convert.ToUInt32(buffer[offset + 2]) << 8) +
                Convert.ToUInt32(buffer[offset + 3]);
        }

        static UInt16 GetUInt16(IByteBuffer buffer, int offset)
        {
            return Convert.ToUInt16((buffer[offset] << 8) + buffer[offset + 1]);
        }

        protected virtual ProcessPacketStatus ParsePrivateStream2Packet(int packetLength)
        {
            byte substreamId = this.buffer[this.bufferOffset + 6];
            if(substreamId == DsiSubstreamId)
            {
                int infoOffset = this.bufferOffset + 11;
                UInt32 logicalBlockNumber = GetUInt32(this.buffer, infoOffset);
                UInt32 vobuEnd = GetUInt32(this.buffer, infoOffset + 4);
                UInt16 currentVobNumber = GetUInt16(this.buffer, infoOffset + 20);
                byte cellNumber = this.buffer[infoOffset + 23];
                UInt16 interleavedFlags = GetUInt16(this.buffer, infoOffset + 28);
                bool PREUFlag = (interleavedFlags & 0x8000) != 0;
                bool ILVUFlag = (interleavedFlags & 0x4000) != 0;
                bool UnitStartFlag = (interleavedFlags & 0x2000) != 0;
                bool UnitEndFlag = (interleavedFlags & 0x1000) != 0;
                /*Debug.WriteLine(string.Format(
                    "PREU {0} ILVU {1} UStart {2} UEnd {3}",
                    PREUFlag, ILVUFlag, UnitStartFlag, UnitEndFlag));*/

                if(ILVUFlag)
                {
                    CellIdVobId cellVobId = new CellIdVobId() 
                        { CellId = cellNumber, VobId = currentVobNumber };
                    OnAngleCellChanged(true, cellVobId);
                }
                else
                {
                    OnAngleCellChanged(false, null);
                }
            }
            return ProcessPacketStatus.PacketDone;
        }

        protected virtual ProcessPacketStatus ParseStreamPacket(int packetLength)
        {
            if((this.buffer[this.bufferOffset + 6] & 0xc0) != 0x80)
            {
                //tracer.WriteLine(TraceSeverity.Error, "Malformed Stream Packet at {0}",
                //    BufferPosition);
                return ProcessPacketStatus.InvalidPacketBytes;
            }

            byte packetTypeCode = this.buffer[this.bufferOffset + 3];
            int ptsDtsFlag = this.buffer[this.bufferOffset + 7] & 0xc0;
            int escrFlag = this.buffer[this.bufferOffset + 7] & 0x20;
            if(escrFlag != 0)
            {
                Debug.WriteLine("escrFlag");
            }
            int pesHeaderLength = 9 + this.buffer[this.bufferOffset + 8];

            MpegStreamDefinition streamDef;
            if(packetTypeCode == PrivateStream1Code)
            {
                //tracer.WriteLine(TraceSeverity.Information, "Private1 Packet");
                int streamId = this.buffer[this.bufferOffset + pesHeaderLength];
                streamDef = FindPrivateStream(streamId);
                if(streamDef.Codec == Codec.Unknown)
                {
                    streamDef.FindPSPrivateStreamCodec(this.programStreamMap);
                    if(streamDef.Codec != Codec.Unknown)
                    {
                        OnStreamFound(streamDef);
                    }
                }
                pesHeaderLength += 1 + streamDef.SubHeaderSize;
            }
            else
            {
                streamDef = FindStream(packetTypeCode);
                if(streamDef.Codec == Codec.Unknown)
                {
                    streamDef.FindPSStreamCodec(this.programStreamMap);
                    if(streamDef.Codec != Codec.Unknown)
                    {
                        OnStreamFound(streamDef);
                    }
                }
            }

            double? newPts = null;
            double? newDts = null;
            if((ptsDtsFlag & 0x80) != 0)
            {
                long pts = (((long)this.buffer[this.bufferOffset + 9] & 0x0e) << 29) |
                    ((long)this.buffer[this.bufferOffset + 10] << 22) |
                    (((long)this.buffer[this.bufferOffset + 11] & 0xfe) << 14) |
                    ((long)this.buffer[this.bufferOffset + 12] << 7) |
                    ((long)this.buffer[this.bufferOffset + 13] >> 1);
                newPts = Convert.ToDouble(pts) / 90.0f;

                if((ptsDtsFlag & 0x40) != 0)
                {
                    long dts = (((long)this.buffer[this.bufferOffset + 14] & 0x0e) << 29) |
                        ((long)this.buffer[this.bufferOffset + 15] << 22) |
                        (((long)this.buffer[this.bufferOffset + 16] & 0xfe) << 14) |
                        ((long)this.buffer[this.bufferOffset + 17] << 7) |
                        ((long)this.buffer[this.bufferOffset + 18] >> 1);
                    newDts = Convert.ToDouble(dts) / 90.0f;
                }
            }

            //tracer.WriteLine(TraceSeverity.Information, "Packet of {0} bytes on stream {1}",
            //    packetLength - pesHeaderLength, streamDef.StreamId);

            OnPacketTrace("{0} StreamId {1:x} at {2} length {3} pts {4} dts {5}",
                streamDef.StreamType, streamDef.StreamId, BufferPosition, packetLength, newPts, newDts);

            switch(streamDef.StreamType)
            {
            case StreamType.Video:
                streamDef.RateLocked = this.videoLockFlag;
                break;
            case StreamType.Audio:
                streamDef.RateLocked = this.audioLockFlag;
                break;
            }

            OnStreamData(streamDef, this.bufferOffset + pesHeaderLength,
                packetLength - pesHeaderLength, newPts);
            OnStreamPacket(streamDef, packetLength, pesHeaderLength);
            return ProcessPacketStatus.PacketDone;
        }

        protected virtual void Dispose(bool managedDispose)
        {
            if(managedDispose)
            {
                //tracer.WriteLine(TraceSeverity.Important, "Dispose");
                this.buffer = null;
                this.callback = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MpegPSSplitter()
        {
            Dispose(false);
        }
    }
}
