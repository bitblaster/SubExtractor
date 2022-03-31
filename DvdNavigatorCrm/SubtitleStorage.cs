using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DvdNavigatorCrm
{
    public class SubtitleStorage : ISubtitleStorage
    {
        public const string DvdOcrSubdirectory = "DvdSubExtractor";

        public static ISubtitleStorage CreateWriter(string filePath)
        {
            return new SubtitleStorage(filePath);
        }

        public static void ReadStorage(string filePath, ISubtitleStorage client)
        {
            SubtitleStorage reader = new SubtitleStorage(filePath, client);
            try
            {
                reader.Read();
            }
            finally
            {
                reader.Close();
            }
        }

        FileStream fileStream;
        BinaryWriter writer;
        BinaryReader reader;
        ISubtitleStorage client;

        SubtitleStorage(string filePath)
        {
            if(!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            this.fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            try
            {
                this.writer = new BinaryWriter(this.fileStream, Encoding.Unicode);
            }
            catch(Exception)
            {
                Close();
                throw;
            }
        }

        SubtitleStorage(string filePath, ISubtitleStorage client)
        {
            this.fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                this.reader = new BinaryReader(this.fileStream, Encoding.Unicode);
            }
            catch(Exception)
            {
                Close();
                throw;
            }
            this.client = client;
        }

        const uint MagicPacket = 0xcfbead98;
        const int IdHeader = 0x1f01;
        const int IdPalette = 0x1f02;
        const int IdStream = 0x1f03;
        const int IdSubtitlePacket = 0x1f04;
        const int IdVideoAudio = 0x1f05;
        const int IdCellStart = 0x1f06;

        void Read()
        {
            try
            {
                while(true)
                {
                    uint magic = this.reader.ReadUInt32();
                    if(magic != MagicPacket)
                    {
                        throw new InvalidDataException(string.Format("Missing Magic Packet at {0}", this.fileStream.Position));
                    }

                    int id = this.reader.ReadInt32();
                    switch(id)
                    {
                    case IdHeader:
                        this.client.AddHeader(this.reader.ReadString(), this.reader.ReadInt32(),
                            this.reader.ReadInt32(), this.reader.ReadInt32());
                        break;
                    case IdVideoAudio:
                        {
                            VideoAttributes video = new VideoAttributes()
                            {
                                CodingMode = (VideoCodingMode)reader.ReadInt32(),
                                Standard = (VideoStandard)reader.ReadInt32(),
                                AspectRatio = (VideoAspectRatio)reader.ReadInt32(),
                                ClosedCaptionLine21Field1 = reader.ReadBoolean(),
                                ClosedCaptionLine21Field2 = reader.ReadBoolean(),
                                VerticalResolution = reader.ReadInt32(),
                                HorizontalResolution = reader.ReadInt32(),
                                IsLetterBoxed = reader.ReadBoolean(),
                                PalStandard = (PalStandard)reader.ReadInt32(),
                            };
                            int streamCount = this.reader.ReadInt32();
                            List<AudioStreamItem> items = new List<AudioStreamItem>();
                            for(int index = 0; index < streamCount; index++)
                            {
                                int audioStreamId = this.reader.ReadInt32();
                                AudioAttributes attribs = new AudioAttributes()
                                {
                                    TrackId = reader.ReadInt32(),
                                    CodingMode = (AudioCodingMode)reader.ReadInt32(),
                                    Language = reader.ReadString(),
                                    Channels = reader.ReadInt32(),
                                    CodeExtension = (AudioCodeExtension)reader.ReadInt32()
                                };
                                items.Add(new AudioStreamItem(audioStreamId, attribs));

                            }
                            this.client.AddVideoAudioInfo(video, items);
                        }
                        break;
                    case IdCellStart:
                        {
                            double ptsOffset = this.reader.ReadDouble();
                            SaverCellType cellType = (SaverCellType)this.reader.ReadInt32();
                            long filePosition = this.reader.ReadInt64();
                            double firstCellPts = this.reader.ReadDouble();
                            double firstAudioPts = this.reader.ReadDouble();
                            double firstVideoPts = this.reader.ReadDouble();
                            this.client.AddCellStartOffsets(ptsOffset, cellType, filePosition, firstCellPts, firstAudioPts, firstVideoPts);
                        }
                        break;
                    case IdStream:
                        {
                            int streamId = this.reader.ReadInt32();
                            SubpictureAttributes attribs = new SubpictureAttributes()
                            {
                                Language = this.reader.ReadString(),
                                CodeExtension = (SubpictureCodeExtension)this.reader.ReadInt32(),
                                SubpictureFormat = (SubpictureFormat)this.reader.ReadInt32()
                            };
                            this.client.AddStream(streamId, attribs);
                        }
                        break;
                    case IdPalette:
                        {
                            int paletteCount = this.reader.ReadInt32();
                            List<int> palette = new List<int>();
                            for(int index = 0; index < paletteCount; index++)
                            {
                                palette.Add(this.reader.ReadInt32());
                            }
                            this.client.AddPalette(palette);
                        }
                        break;
                    case IdSubtitlePacket:
                        {
                            int streamId = this.reader.ReadInt32();
                            int byteCount = this.reader.ReadInt32();
                            this.client.AddSubtitlePacket(streamId,
                                this.reader.ReadBytes(byteCount),
                                0, byteCount, this.reader.ReadDouble());
                        }
                        break;
                    default:
                        throw new InvalidDataException(string.Format("Unknown Id {0}", id));
                    }
                }
            }
            catch(EndOfStreamException)
            {
            }
        }

        public void AddHeader(string dvdDirectoryPath, int ifoNumber, int trackNumber, int angle)
        {
            this.writer.Write(MagicPacket);
            this.writer.Write(IdHeader);
            this.writer.Write(dvdDirectoryPath);
            this.writer.Write(ifoNumber);
            this.writer.Write(trackNumber);
            this.writer.Write(angle);
        }

        public void AddCellStartOffsets(double previousPtsOffset, SaverCellType cellType, long filePosition, double firstCellPts, double firstAudioPts, double firstVideoPts)
        {
            this.writer.Write(MagicPacket);
            this.writer.Write(IdCellStart);
            this.writer.Write(previousPtsOffset);
            this.writer.Write((int)cellType);
            this.writer.Write(filePosition);
            this.writer.Write(firstCellPts);
            this.writer.Write(firstAudioPts);
            this.writer.Write(firstVideoPts);
        }

        public void AddVideoAudioInfo(VideoAttributes video, IList<AudioStreamItem> audio)
        {
            this.writer.Write(MagicPacket);
            this.writer.Write(IdVideoAudio);
            this.writer.Write((int)video.CodingMode);
            this.writer.Write((int)video.Standard);
            this.writer.Write((int)video.AspectRatio);
            this.writer.Write(video.ClosedCaptionLine21Field1);
            this.writer.Write(video.ClosedCaptionLine21Field2);
            this.writer.Write(video.VerticalResolution);
            this.writer.Write(video.HorizontalResolution);
            this.writer.Write(video.IsLetterBoxed);
            this.writer.Write((int)video.PalStandard);

            this.writer.Write(audio.Count);
            foreach(AudioStreamItem item in audio)
            {
                this.writer.Write(item.StreamId);
                this.writer.Write(item.AudioAttributes.TrackId);
                this.writer.Write((int)item.AudioAttributes.CodingMode);
                this.writer.Write(item.AudioAttributes.Language);
                this.writer.Write(item.AudioAttributes.Channels);
                this.writer.Write((int)item.AudioAttributes.CodeExtension);
            }
        }

        public void AddStream(int streamId, SubpictureAttributes attribs)
        {
            this.writer.Write(MagicPacket);
            this.writer.Write(IdStream);
            this.writer.Write(streamId);
            this.writer.Write(attribs.Language);
            this.writer.Write((int)attribs.CodeExtension);
            this.writer.Write((int)attribs.SubpictureFormat);
        }

        public void AddPalette(IList<int> yuvColors)
        {
            this.writer.Write(MagicPacket);
            this.writer.Write(IdPalette);
            this.writer.Write(yuvColors.Count);
            foreach(int color in yuvColors)
            {
                this.writer.Write(color);
            }
        }

        public void AddSubtitlePacket(int streamId, byte[] buffer, int offset, int length, double pts)
        {
            this.writer.Write(MagicPacket);
            this.writer.Write(IdSubtitlePacket);
            this.writer.Write(streamId);
            this.writer.Write(length);
            this.writer.Write(buffer, offset, length);
            this.writer.Write(pts);
        }

        public void Close()
        {
            if(this.writer != null)
            {
                this.writer.Close();
                this.writer = null;
            }
            if(this.reader != null)
            {
                this.reader.Close();
                this.reader = null;
            }
            if(this.fileStream != null)
            {
                this.fileStream.Dispose();
                this.fileStream = null;
            }
        }
    }
}
