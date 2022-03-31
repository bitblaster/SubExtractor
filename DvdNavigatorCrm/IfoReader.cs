/*
 * Copyright (C) 2007, 2008 Chris Meadowcroft <crmeadowcroft@gmail.com>
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
using System.IO;
using System.Text;

namespace DvdNavigatorCrm
{
	class IfoReader
	{
		string fileName;
		byte[] data;
		int dataOffset;

		public IfoReader(string fileName)
		{
			this.fileName = fileName;
			this.data = File.ReadAllBytes(fileName);
		}

		public IfoReader(string fileName, byte[] data)
		{
			this.fileName = fileName;
			this.data = data;
		}

		public string FileName { get { return this.fileName; } }

		public long Length { get { return this.data.Length; } }

		public int Read(byte[] buffer, int length)
		{
			length = (int)Math.Min(length, this.data.Length - dataOffset);
			Buffer.BlockCopy(this.data, this.dataOffset, buffer, 0, length);
			dataOffset += length;
			return length;
		}

		public int ReadByte()
		{
			byte next = this.data[this.dataOffset];
			this.dataOffset++;
			return next;
		}

		public int ReadByte(int offsetFromStart)
		{
			SeekFromStart(offsetFromStart);
			return ReadByte();
		}

		public long SeekFromStart(int offset)
		{
			this.dataOffset = Math.Max(0, Math.Min(offset, this.data.Length));
			return this.dataOffset;
		}

		public long SeekFromCurrent(int offset)
		{
			this.dataOffset = Math.Max(0, Math.Min(this.dataOffset + offset, this.data.Length));
			return this.dataOffset;
		}

		public UInt32 ReadUInt32()
		{
			UInt32 nextU32 = ((UInt32)this.data[this.dataOffset] << 24) + ((UInt32)this.data[this.dataOffset + 1] << 16) +
				((UInt32)this.data[this.dataOffset + 2] << 8) + (UInt32)this.data[this.dataOffset + 3];
			this.dataOffset += 4;
			return nextU32;
		}

		public UInt32 ReadUInt32(int offsetFromStart)
		{
			SeekFromStart(offsetFromStart);
			return ReadUInt32();
		}

		public UInt16 ReadUInt16()
		{
			UInt16 nextU16 = Convert.ToUInt16((this.data[this.dataOffset] << 8) + this.data[this.dataOffset + 1]);
			this.dataOffset += 2;
			return nextU16;
		}

		public UInt16 ReadUInt16(int offsetFromStart)
		{
			SeekFromStart(offsetFromStart);
			return ReadUInt16();
		}

		public void ReadTimingInfo(out int hours, out int minutes, out int seconds, out int frames, out float fps)
		{
			int bcdHours = ReadByte();
			hours = ((bcdHours & 0xf0) >> 4) * 10 + (bcdHours & 0x0f);
			int bcdMinutes = ReadByte();
			minutes = ((bcdMinutes & 0xf0) >> 4) * 10 + (bcdMinutes & 0x0f);
			int bcdSeconds = ReadByte();
			seconds = ((bcdSeconds & 0xf0) >> 4) * 10 + (bcdSeconds & 0x0f);
			int fraction = ReadByte();
			fps = ((fraction & 0xc0) == 0x40) ? 25.0f : 29.97f;
			frames = ((fraction & 0x30) >> 4) * 10 + (fraction & 0x0f);
		}
	}
}
