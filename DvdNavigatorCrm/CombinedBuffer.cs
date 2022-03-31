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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DvdNavigatorCrm
{
    public class CombinedBuffer : IByteBuffer
    {
        static readonly IEnumerable<IDataBuffer> emptyList = new List<IDataBuffer>().AsReadOnly();
        List<IDataBuffer> buffers = new List<IDataBuffer>();
        int totalLength;
        int extraOffset;

        class BufferOffset
        {
            public byte[] Buffer;
            public int StartIndex;
            public int StartOffset;
            public int EndIndex;

            public int BufferToGlobal(int index)
            {
                return index - this.StartOffset;
            }
            public int GlobalToBuffer(int index)
            {
                return index + this.StartOffset;
            }
        }
        BufferOffset[] offsets = new BufferOffset[] { };

        public CombinedBuffer()
        {
        }

        public CombinedBuffer(IDataBuffer buffer)
        {
            AddBuffer(buffer, 0);
        }

        public CombinedBuffer(byte[] data)
        {
            AddBuffer(new DataBuffer(data), 0);
        }

        void BuildOffsetArray()
        {
            this.offsets = new BufferOffset[this.buffers.Count];
            int startIndex = 0;
            int nextOffset = this.extraOffset;
            for(int index = 0; index < this.offsets.Length; index++)
            {
                IDataBuffer dataBuffer = this.buffers[index];
                BufferOffset bufferOffset = new BufferOffset();
                bufferOffset.Buffer = this.buffers[index].GetBuffer();
                bufferOffset.StartIndex = startIndex;
                bufferOffset.StartOffset = dataBuffer.Offset + nextOffset - startIndex;
                int length = dataBuffer.Length - nextOffset;
                bufferOffset.EndIndex = startIndex + length;

                this.offsets[index] = bufferOffset;
                nextOffset = 0;
                startIndex += length;
            }
        }

        public long Position { get; private set; }

        public void AddBuffer(IDataBuffer buffer, long position)
        {
            if(this.buffers.Count == 0)
            {
                this.Position = position;
            }
            this.buffers.Add(buffer);
            this.totalLength += buffer.Length;
            BuildOffsetArray();
        }

        public IEnumerable<IDataBuffer> MovePositionForwardAndReturnUnusedBuffers(int offset)
        {
            if((offset < 0) || (offset > this.totalLength))
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            this.extraOffset += offset;
            this.totalLength -= offset;
            this.Position += offset;

            List<IDataBuffer> finishedBuffers = null;
            foreach(IDataBuffer buffer in this.buffers)
            {
                if(this.extraOffset >= buffer.Length)
                {
                    this.extraOffset -= buffer.Length;
                    if(finishedBuffers == null)
                    {
                        finishedBuffers = new List<IDataBuffer>();
                    }
                    finishedBuffers.Add(buffer);
                }
                else
                {
                    break;
                }
            }

            if(finishedBuffers != null)
            {
                this.buffers.RemoveRange(0, finishedBuffers.Count);
                BuildOffsetArray();
                return finishedBuffers;
            }
            else
            {
                BuildOffsetArray();
                return emptyList;
            }
        }

        public IEnumerable<IDataBuffer> RemoveAllBuffers()
        {
            this.totalLength = 0;
            this.extraOffset = 0;
            this.Position = 0L;
            this.offsets = new BufferOffset[] { };
            IEnumerable<IDataBuffer> result = this.buffers;
            this.buffers = new List<IDataBuffer>();
            return result;
        }

        public byte this[int index]
        {
            get
            {
                if((index < 0) || (index >= this.totalLength))
                {
                    throw new IndexOutOfRangeException();
                }
                for(int bufferIndex = 0; bufferIndex < this.offsets.Length; bufferIndex++)
                {
                    BufferOffset offset = this.offsets[bufferIndex];
                    if(index < offset.EndIndex)
                    {
                        return offset.Buffer[offset.GlobalToBuffer(index)];
                    }
                }
                throw new InvalidOperationException();
            }
        }

        IEnumerable<int> IndicesOf(byte item, BufferOffset offset, int startIndex, int searchLength)
        {
            int searchStartIndex = offset.GlobalToBuffer(Math.Max(offset.StartIndex, startIndex));
            int searchEndIndex = offset.GlobalToBuffer(Math.Min(offset.EndIndex, startIndex + searchLength));
            while(searchStartIndex < searchEndIndex)
            {
                int foundIndex = Array.IndexOf(offset.Buffer, item, searchStartIndex,
                    searchEndIndex - searchStartIndex);
                if(foundIndex == -1)
                {
                    yield break;
                }
                yield return offset.BufferToGlobal(foundIndex);
                searchStartIndex = foundIndex + 1;
            }
        }

        public int IndexOf(byte item)
        {
            for(int bufferIndex = 0; bufferIndex < this.offsets.Length; bufferIndex++)
            {
                foreach(int foundIndex in IndicesOf(item, this.offsets[bufferIndex], 0, this.totalLength))
                {
                    return foundIndex;
                }
            }
            return -1;
        }

        public IEnumerable<int> IndicesOf(byte item)
        {
            for(int bufferIndex = 0; bufferIndex < this.offsets.Length; bufferIndex++)
            {
                foreach(int foundIndex in IndicesOf(item, this.offsets[bufferIndex], 0, this.totalLength))
                {
                    yield return foundIndex;
                }
            }
        }

        public int IndexOf(byte item, int startIndex)
        {
            return IndexOf(item, startIndex, this.totalLength - startIndex);
        }

        public IEnumerable<int> IndicesOf(byte item, int startIndex)
        {
            return IndicesOf(item, startIndex, this.totalLength - startIndex);
        }

        public int IndexOf(byte item, int startIndex, int searchLength)
        {
            if((startIndex < 0) || (startIndex >= this.totalLength))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if((searchLength <= 0) || (startIndex + searchLength > this.totalLength))
            {
                throw new ArgumentOutOfRangeException("searchLength");
            }

            for(int bufferIndex = 0; bufferIndex < this.offsets.Length; bufferIndex++)
            {
                BufferOffset offset = this.offsets[bufferIndex];
                if(startIndex < offset.EndIndex)
                {
                    foreach(int foundIndex in IndicesOf(item, offset, startIndex, searchLength))
                    {
                        return foundIndex;
                    }
                }
            }
            return -1;
        }

        public IEnumerable<int> IndicesOf(byte item, int startIndex, int searchLength)
        {
            if((startIndex < 0) || (startIndex >= this.totalLength))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if((searchLength <= 0) || (startIndex + searchLength > this.totalLength))
            { 
                throw new ArgumentOutOfRangeException("searchLength");
            }

            for(int bufferIndex = 0; bufferIndex < this.offsets.Length; bufferIndex++)
            {
                BufferOffset offset = this.offsets[bufferIndex];
                if(startIndex < offset.EndIndex)
                {
                    foreach(int foundIndex in IndicesOf(item, offset, startIndex, searchLength))
                    {
                        yield return foundIndex;
                    }
                }
            }
        }

        bool CheckPartialMatch(int bufferIndex, int startIndex, byte[] pattern, int patternIndex)
        {
            if(bufferIndex >= this.offsets.Length)
            {
                return false;
            }

            BufferOffset offset = this.offsets[bufferIndex];
            int bufferRemainingLength = offset.EndIndex - startIndex;
            int patternRemainingLength = pattern.Length - patternIndex;
            int matchLength = Math.Min(bufferRemainingLength, patternRemainingLength);
            int matchStartIndex = offset.GlobalToBuffer(startIndex);
            for(int index = 0; index < matchLength; index++)
            {
                if(offset.Buffer[matchStartIndex + index] != pattern[patternIndex + index])
                {
                    return false;
                }
            }
            if(matchLength < patternRemainingLength)
            {
                return CheckPartialMatch(bufferIndex + 1, startIndex + matchLength,
                    pattern, patternIndex + matchLength);
            }
            return true;
        }

        IEnumerable<int> IndicesOf(byte[] pattern, int offsetIndex, int startIndex, int searchLength)
        {
            BufferOffset offset = this.offsets[offsetIndex];
            int searchStartIndex = offset.GlobalToBuffer(Math.Max(offset.StartIndex, startIndex));
            int searchEndIndex = offset.GlobalToBuffer(Math.Min(offset.EndIndex, 
                startIndex + searchLength - pattern.Length + 1));
            int lastFoundIndexThatFits = searchEndIndex - pattern.Length;
            while(searchStartIndex < searchEndIndex)
            {
                int foundIndex = Array.IndexOf(offset.Buffer, pattern[0], searchStartIndex,
                    searchEndIndex - searchStartIndex);
                if(foundIndex == -1)
                {
                    yield break;
                }

                bool match = true;
                if(foundIndex <= lastFoundIndexThatFits)
                {
                    for(int index = 1; index < pattern.Length; index++)
                    {
                        if(pattern[index] != offset.Buffer[foundIndex + index])
                        {
                            match = false;
                            break;
                        }
                    }
                }
                else
                {
                    match = CheckPartialMatch(offsetIndex, offset.BufferToGlobal(foundIndex + 1), pattern, 1);
                }

                if(match)
                {
                    yield return offset.BufferToGlobal(foundIndex);
                }
                searchStartIndex = foundIndex + 1;
            }
        }

        public int IndexOf(byte[] pattern)
        {
            if((pattern == null) || (pattern.Length == 0))
            {
                throw new ArgumentException("pattern cannot be null or empty");
            }

            for(int bufferIndex = 0; bufferIndex < this.offsets.Length; bufferIndex++)
            {
                foreach(int foundIndex in IndicesOf(pattern, bufferIndex, 0, this.totalLength))
                {
                    return foundIndex;
                }
            }
            return -1;
        }

        public int IndexOf(byte[] pattern, int startIndex)
        {
            return IndexOf(pattern, startIndex, this.totalLength - startIndex);
        }

        public int IndexOf(byte[] pattern, int startIndex, int searchLength)
        {
            if((pattern == null) || (pattern.Length == 0))
            {
                throw new ArgumentException("pattern cannot be null or empty");
            }
            if((startIndex < 0) || (startIndex >= this.totalLength))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if((searchLength <= 0) || (startIndex + searchLength > this.totalLength))
            {
                throw new ArgumentOutOfRangeException("searchLength");
            }

            for(int bufferIndex = 0; bufferIndex < this.offsets.Length; bufferIndex++)
            {
                foreach(int foundIndex in IndicesOf(pattern, bufferIndex, startIndex, searchLength))
                {
                    return foundIndex;
                }
            }
            return -1;
        }

        public IEnumerable<int> IndicesOf(byte[] pattern)
        {
            if((pattern == null) || (pattern.Length == 0))
            {
                throw new ArgumentException("pattern");
            }

            for(int bufferIndex = 0; bufferIndex < this.offsets.Length; bufferIndex++)
            {
                foreach(int foundIndex in IndicesOf(pattern, bufferIndex, 0, this.totalLength))
                {
                    yield return foundIndex;
                }
            }
        }

        public IEnumerable<int> IndicesOf(byte[] pattern, int startIndex)
        {
            return IndicesOf(pattern, startIndex, this.totalLength - startIndex);
        }

        public IEnumerable<int> IndicesOf(byte[] pattern, int startIndex, int searchLength)
        {
            if((pattern == null) || (pattern.Length == 0))
            {
                throw new ArgumentException("pattern cannot be null or empty");
            }
            if((startIndex < 0) || (startIndex >= this.totalLength))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if((searchLength <= 0) || (startIndex + searchLength > this.totalLength))
            {
                throw new ArgumentOutOfRangeException("searchLength");
            }

            for(int bufferIndex = 0; bufferIndex < this.offsets.Length; bufferIndex++)
            {
                if(startIndex < this.offsets[bufferIndex].EndIndex)
                {
                    foreach(int foundIndex in IndicesOf(pattern, bufferIndex, startIndex, searchLength))
                    {
                        yield return foundIndex;
                    }
                }
            }
        }

        public bool Contains(byte item)
        {
            return (IndexOf(item) != -1);
        }

        public void CopyTo(byte[] array, int arrayIndex)
        {
            for(int bufferIndex = 0; bufferIndex < this.offsets.Length; bufferIndex++)
            {
                BufferOffset offset = this.offsets[bufferIndex];
                int length = offset.EndIndex - offset.StartIndex;
                Buffer.BlockCopy(offset.Buffer, offset.StartOffset + offset.StartIndex,
                    array, arrayIndex, length);
                arrayIndex += length;
            }
        }

        public void CopyTo(int sourceIndex, byte[] array, int arrayIndex, int arrayLength)
        {
            if((sourceIndex < 0) || (sourceIndex >= this.totalLength))
            {
                throw new ArgumentOutOfRangeException("sourceIndex");
            }

            for(int bufferIndex = 0; bufferIndex < this.offsets.Length; bufferIndex++)
            {
                BufferOffset offset = this.offsets[bufferIndex];
                if(sourceIndex < offset.EndIndex)
                {
                    int length;
                    if(sourceIndex <= offset.StartIndex)
                    {
                        length = Math.Min(offset.EndIndex - offset.StartIndex, arrayLength);
                        Buffer.BlockCopy(offset.Buffer, offset.StartOffset + offset.StartIndex, 
                            array, arrayIndex, length);
                    }
                    else
                    {
                        length = Math.Min(offset.EndIndex - sourceIndex, arrayLength);
                        Buffer.BlockCopy(offset.Buffer, offset.StartOffset + sourceIndex,
                            array, arrayIndex, length);
                    }
                    arrayIndex += length;
                    arrayLength -= length;
                    if(arrayLength == 0)
                    {
                        break;
                    }
                }
            }
        }

        public void CopyTo(int sourceIndex, Stream output, int outputLength)
        {
            if((sourceIndex < 0) || (sourceIndex >= this.totalLength))
            {
                throw new ArgumentOutOfRangeException("sourceIndex");
            }

            for(int bufferIndex = 0; bufferIndex < this.offsets.Length; bufferIndex++)
            {
                BufferOffset offset = this.offsets[bufferIndex];
                if(sourceIndex < offset.EndIndex)
                {
                    int length;
                    if(sourceIndex <= offset.StartIndex)
                    {
                        length = Math.Min(offset.EndIndex - offset.StartIndex, outputLength);
                        output.Write(offset.Buffer, offset.StartOffset + offset.StartIndex, length);
                    }
                    else
                    {
                        length = Math.Min(offset.EndIndex - sourceIndex, outputLength);
                        output.Write(offset.Buffer, offset.StartOffset + sourceIndex, length);
                    }
                    outputLength -= length;
                    if(outputLength == 0)
                    {
                        break;
                    }
                }
            }
        }

        public int Count
        {
            get { return this.totalLength; }
        }

        public bool IsEmpty
        {
            get { return (this.totalLength == 0); }
        }

        public IEnumerator<byte> GetEnumerator()
        {
            for(int bufferIndex = 0; bufferIndex < this.offsets.Length; bufferIndex++)
            {
                BufferOffset offset = this.offsets[bufferIndex];
                int start = offset.GlobalToBuffer(offset.StartIndex);
                int end = offset.GlobalToBuffer(offset.EndIndex);
                for(int index = start; index < end; index++)
                {
                    yield return offset.Buffer[index];
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
