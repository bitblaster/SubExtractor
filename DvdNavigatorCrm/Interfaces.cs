using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DvdNavigatorCrm
{
    public interface IStreamDefinition
    {
        int StreamId { get; }
        int ProgramId { get; }
        StreamType StreamType { get; }
        Codec Codec { get; }
        string Language { get; }
        IStreamExtraInformation ExtraInformation { get; }
    }

    public interface IStreamExtraInformation
    {
    }

    public interface IBasicSplitterCallback
    {
        void Discontinuity();
        void StreamFound(IStreamDefinition stream);
        void StreamData(IStreamDefinition stream, int packetOffset, int packetLength, double? pts, long bufferPosition);
        void AngleCellChanged(bool isAngle, CellIdVobId cellVobId, long bufferPosition);
        void StreamPackHeader(ulong scr, uint muxRate, int packetOffset, int packetLength, long bufferPosition);
        void HeaderPacket(int packetTypeCode, int packetOffset, int packetLength, long bufferPosition);
        void StreamPacket(IStreamDefinition streamDef, int packetOffset, int packetLength, long bufferPosition, int pesHeaderLength);
        void PacketTrace(string text);
    }

    public interface IDataBuffer
    {
        byte[] GetBuffer();
        int Offset { get; }
        int Length { get; }
        int Id { get; }
    }

    public interface IByteBuffer : IEnumerable<byte>
    {
        int Count { get; }
        bool IsEmpty { get; }
        byte this[int index] { get; }

        void CopyTo(byte[] array, int arrayIndex);
        void CopyTo(int sourceIndex, byte[] array, int arrayIndex, int arrayLength);
        int IndexOf(byte item, int startIndex);
        int IndexOf(byte item, int startIndex, int searchLength);
        int IndexOf(byte[] pattern);
        int IndexOf(byte[] pattern, int startIndex);
        int IndexOf(byte[] pattern, int startIndex, int searchLength);
        IEnumerable<int> IndicesOf(byte item);
        IEnumerable<int> IndicesOf(byte item, int startIndex);
        IEnumerable<int> IndicesOf(byte item, int startIndex, int searchLength);
        IEnumerable<int> IndicesOf(byte[] pattern);
        IEnumerable<int> IndicesOf(byte[] pattern, int startIndex);
        IEnumerable<int> IndicesOf(byte[] pattern, int startIndex, int searchLength);
    }

    public interface IBasicSplitter : IDisposable
    {
        int MaxBytesWithoutPacket { get; }

        DemuxResult DemuxBuffer(IByteBuffer dataBuffer, long positionAtStartOfBuffer);
        void ExternalDiscontinuity();
    }

    public interface IDvdTitleSet
    {
        IList<PartOfTitle> GetTitleParts(int title);
        ProgramGroupChain GetChain(int chain);
        AudioAttributes GetAudioAttributes(int index);
        SubpictureAttributes GetSubtitleAttributes(int index);
    }

    public interface ISubtitleStorage
    {
        void AddHeader(string dvdDirectoryPath, int ifoNumber, int trackNumber, int angle);
        void AddVideoAudioInfo(VideoAttributes video, IList<AudioStreamItem> audio);
        void AddPalette(IList<int> yuvColors);
        void AddStream(int streamId, SubpictureAttributes subAttributes);
        void AddSubtitlePacket(int streamId, byte[] buffer, int offset, int length, double pts);
        void AddCellStartOffsets(double previousPtsOffset, SaverCellType cellType, long filePosition, double firstCellPts, double firstAudioPts, double firstVideoPts);
        void Close();
    }
}
