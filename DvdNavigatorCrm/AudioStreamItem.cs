/*
 * Copyright (C) 2007, 2008 Christopher R Meadowcroft <crmeadowcroft@gmail.com>
 *
 * This file is part of DvdSubOcr, a free DVD Subtitle OCR program.
 * See for updates.
 *
 * DvdSubOcr is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * DvdSubOcr is distributed in the hope that it will be useful,
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
using DvdNavigatorCrm;

namespace DvdNavigatorCrm
{
    public class AudioStreamItem : IComparable<AudioStreamItem>
    {
        public AudioStreamItem(int streamId, AudioAttributes audioAttributes)
        {
            this.AudioAttributes = audioAttributes;
            this.StreamId = streamId;
            this.KBitsPerSecond = 0;
            switch(audioAttributes.CodingMode)
            {
            case AudioCodingMode.AC3:
                switch(audioAttributes.Channels)
                {
                case 1:
                    this.KBitsPerSecond = 96;
                    break;
                case 2:
                    this.KBitsPerSecond = 192;
                    break;
                case 3:
                    this.KBitsPerSecond = 448;
                    break;
                case 6:
                    this.KBitsPerSecond = 448;
                    break;
                }
                break;
            case AudioCodingMode.MPEG1:
            case AudioCodingMode.MPEG2:
                switch(audioAttributes.Channels)
                {
                case 1:
                    this.KBitsPerSecond = 96;
                    break;
                case 2:
                    this.KBitsPerSecond = 192;
                    break;
                case 6:
                    this.KBitsPerSecond = 576;
                    break;
                }
                break;
            case AudioCodingMode.DTS:
                switch(audioAttributes.Channels)
                {
                case 2:
                    this.KBitsPerSecond = 512;
                    break;
                case 6:
                    this.KBitsPerSecond = 1536;
                    break;
                }
                break;
            case AudioCodingMode.LPCM:
                this.KBitsPerSecond = 48 * 8 * audioAttributes.Channels;
                break;
            }
        }

        public AudioAttributes AudioAttributes { get; private set; }
        public int StreamId { get; private set; }
        public int KBitsPerSecond { get; private set; }

        public int CompareTo(AudioStreamItem other)
        {
            return this.StreamId.CompareTo(other.StreamId);
        }

        public override string ToString()
        {
            return this.StreamId.ToString();
        }
    }
}
