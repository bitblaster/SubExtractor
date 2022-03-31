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
using System.Text;

namespace DvdNavigatorCrm
{
    public class StreamDefinition : IStreamDefinition
    {
        public StreamDefinition(int streamId)
        {
            this.StreamId = streamId;
        }

        public StreamDefinition(int streamId, int programId)
        {
            this.StreamId = streamId;
            this.ProgramId = programId;
            this.Language = "";
        }

        public int StreamId { get; private set; }
        public int ProgramId { get; private set; }
        public StreamType StreamType { get; set; }
        public Codec Codec { get; set; }
        public string Language { get; set; }
        public IStreamExtraInformation ExtraInformation { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Id={0} Prog={1} Type={2} Codec={3} Lang='{4}'",
                this.StreamId, this.ProgramId, this.StreamType, this.Codec, this.Language);
            return sb.ToString();
        }
    }
}
