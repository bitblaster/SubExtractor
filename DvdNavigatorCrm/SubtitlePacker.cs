using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DvdNavigatorCrm;

namespace DvdNavigatorCrm
{
    public class SubtitlePacker
    {
        int streamId;
        ISubtitleStorage storage;
        double? currentPts;
        int dataLength;
        int controlSequenceOffset;
        byte[] byteBuffer;
        int bytesReceived;

        public SubtitlePacker(int streamId, ISubtitleStorage storage)
        {
            this.streamId = streamId;
            this.storage = storage;
        }

        public DecoderStatus HandleBytes(IByteBuffer buffer, int offset, int length,
            double? pts, long bufferPosition)
        {
            if((this.dataLength == 0) || (pts.HasValue && this.currentPts.HasValue && (this.currentPts.Value != pts.Value)))
            {
                this.dataLength = (buffer[offset] << 8) + buffer[offset + 1];
                this.controlSequenceOffset = (buffer[offset + 2] << 8) + buffer[offset + 3];

                // sanity check, in case we missed the start of the subtitle and got a funny length
                if((this.controlSequenceOffset >= this.dataLength) || (length > this.dataLength))
                {
                    Reset();
                    return DecoderStatus.None;
                }

                if((this.byteBuffer == null) || (this.byteBuffer.Length < this.dataLength))
                {
                    this.byteBuffer = new byte[this.dataLength];
                }
                this.currentPts = pts;
                length = Math.Min(this.dataLength, length);
                buffer.CopyTo(offset, this.byteBuffer, 0, length);
                this.bytesReceived = length;
            }
            else
            {
                // sanity check, in case we missed the start of the subtitle and got a funny length
                if(length > this.dataLength - this.bytesReceived)
                {
                    Reset();
                    return DecoderStatus.None;
                }

                if(!this.currentPts.HasValue)
                {
                    this.currentPts = pts;
                }
                buffer.CopyTo(offset, this.byteBuffer, this.bytesReceived, length);
                this.bytesReceived += length;
            }

            if(this.bytesReceived != this.dataLength)
            {
                //Debug.WriteLine(string.Format("HandleBytes this.bytesReceived {0} != this.dataLength {1}",
                //    this.bytesReceived, this.dataLength));
                return DecoderStatus.NeedData;
            }

            if(this.currentPts.HasValue)
            {
                //Debug.WriteLine(
                //    string.Format("AddSubtitlePacket id {0:x2}, len {1}, pts {2:f2}, pos {3}",
                //    this.streamId, this.dataLength, this.currentPts.Value, bufferPosition));
                this.storage.AddSubtitlePacket(streamId, this.byteBuffer, 0, this.dataLength, this.currentPts.Value);
            }
            else
            {
                Debug.WriteLine(
                    string.Format("ERROR AddSubtitlePacket NO PTS id {0:x2}, len {1} pos {2}",
                    this.streamId, this.dataLength, bufferPosition));
            }

            Reset();
            return DecoderStatus.None;
        }

        void Reset()
        {
            this.currentPts = null;
            this.dataLength = 0;
            this.bytesReceived = 0;
        }
    }
}
