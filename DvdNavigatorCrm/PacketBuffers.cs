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
    public class PacketBuffer
    {
        public DataHolder DataHolder;
    }

    class HeaderPacketBuffer : PacketBuffer
    {
        public int PacketTypeCode;
    }

    class StreamPackHeaderBuffer : PacketBuffer
    {
        public ulong SCR;
        public uint MuxRate;
    }

    class StreamPacketBuffer : PacketBuffer
    {
        public IStreamDefinition StreamDefinition;
        public int PesHeaderLength;
    }

    class PalettePacketBuffer : PacketBuffer
    {
        public IList<int> Palette;
    }
}
