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
using System.Linq;
using System.Text;

namespace DvdNavigatorCrm
{
    public class DataBuffer : IDataBuffer
    {
        static volatile int nextId;

        byte[] buffer;

        public DataBuffer(byte[] buffer) : this(buffer, 0, buffer.Length)
        {
        }

        public DataBuffer(byte[] buffer, int offset, int length) : this()
        {
            this.buffer = buffer;
            if((offset < 0) || (offset >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            this.Offset = offset;
            if((length <= 0) || (offset + length > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("length");
            }
            this.Length = length;
        }

        protected DataBuffer()
        {
            this.Id = ++nextId;
        }

        protected void Initialize(byte[] buffer, int offset, int length)
        {
            this.buffer = buffer;
            if((offset < 0) || (offset >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            this.Offset = offset;
            if((length <= 0) || (offset + length > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("length");
            }
            this.Length = length;
        }

        public byte[] GetBuffer() { return this.buffer; }
        public int Offset { get; private set; }
        public int Length { get; private set; }
        public int Id { get; private set; }

        public override string ToString()
        {
            return string.Format("DataBuffer [{0} offset {1} len {2}]", this.Id, this.Offset, this.Length);
        }
    }
}
