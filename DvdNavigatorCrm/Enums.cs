using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DvdNavigatorCrm
{
    public enum StreamType
    {
        Unknown = 0,
        Video = 1,
        Audio = 2,
        Subtitle = 3,
        Navigation = 4,
        Index = 5,
    }

    public enum Codec
    {
        Unknown = 0,

        // video
        H264 = 1,
        MP4V = 2,
        MPGV = 3,

        // audio
        AC3 = 100,
        DTS = 101,
        LPCM = 102,
        MP4A = 103,
        MPGA = 104,

        // subtitle
        SPU = 200,
        OGT = 201,
        CVD = 202,
        SSA = 203,
    }

    public enum DecoderStatus
    {
        None = 0,
        NeedData = 1,
        OutputFull = 2,
        End = 3,
    }

    public enum DemuxStatus
    {
        Success = 0,
        InvalidBytes = 1,
        PartialPacket = 2,
        End = 3,
    }

    public enum ProcessPacketStatus
    {
        StreamEnd = 0,
        PacketDone = 1,
        InvalidPacketBytes = 2,
    }

    public enum PartialSaveStatus
    {
        InRange = 0,
        BeforeStart = 1,
        AfterEnd = 2,
    }

    [Flags]
    public enum SaverCellType
    {
        None = 0,
        First = 1,
        Last = 2,
    }
}
