using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DvdSubExtractor
{
    enum AudioSource
    {
        AC3_2Channel = 0,
        AC3_6Channel = 1,
        DTS_6Channel = 2,
        MPEG2_2Channel = 3,
        LPCM_2Channel = 4,
    }

    enum AudioEncoder
    {
        Passthru = 0,
        AAC = 1,
        MP3 = 2,
    }

    enum AudioMixing
    {
        Passthru = 0,
        Stereo = 2,
        Dolby_Surround = 3,
        Dolby_ProLogic_II = 4,
        _6_Channel_Discreet = 6,
    }

    enum AudioBitrate
    {
        Passthru = 0,
        _160 = 160,
        _192 = 192,
        _224 = 224,
        _256 = 256,
        _320 = 320,
    }

    enum VideoDetelecine
    {
        Off = 0,
        On = 1,
    }

    enum VideoDeinterlace
    {
        Off = 0,
        Decomb = 1,
        Fast = 2,
        Slow = 3,
        Slower = 4,
    }

    enum VideoDenoise
    {
        Off = 0,
        Weak = 1,
        Medium = 2,
        Strong = 3,
    }

    enum VideoDeblock
    {
        Off = 0,
        _5 = 5,
        _6 = 6,
        _7 = 7,
        _8 = 8,
        _9 = 9,
        _10 = 10,
        _11 = 11,
        _12 = 12,
        _13 = 13,
        _14 = 14,
        _15 = 15,
    }

    enum VideoQualityType
    {
        ConstantQuality = 0,
        AverageBitrate = 1,
    }

    public enum SubtitleColorScheme
    {
        Custom = 0,
        OriginalDvd = 1,
        BrightenedDvd = 2,
    }

    public enum RemoveSDH
    {
        None = 0,
        Normal = 1,
        EvenNonCapitalizedTextBeforeColons = 2,
    }

    public enum LineBreaksAndPositions
    {
        Normal = 0,
        KeepBreaks = 1,
        KeepBreaksAndPositions = 2,
    }
}
