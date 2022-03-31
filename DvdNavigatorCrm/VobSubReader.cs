using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DvdNavigatorCrm;

namespace DvdNavigatorCrm
{
    public class VobSubReader : IDisposable
    {
        public static void ReadSubs(string indexFilePath, ISubtitleStorage storage)
        {
            using(VobSubReader reader = new VobSubReader(indexFilePath, storage))
            {
                reader.Read();
            }
        }

        ISubtitleStorage storage;
        FileStream subReader;
        string[] indexLines;

        public VobSubReader(string indexFilePath, ISubtitleStorage storage)
        {
            this.storage = storage;
            this.storage.AddHeader(Path.GetFileNameWithoutExtension(indexFilePath), 1, 1, 0);

            if(!File.Exists(indexFilePath))
            {
                return;
            }
            this.indexLines = File.ReadAllLines(indexFilePath, Encoding.ASCII);
            string subFilePath = Path.Combine(Path.GetDirectoryName(indexFilePath),
                Path.GetFileNameWithoutExtension(indexFilePath) + ".sub");
            if(!File.Exists(subFilePath))
            {
                return;
            }
            this.subReader = new FileStream(subFilePath, FileMode.Open, FileAccess.Read);
        }

        bool videoAdded;
        string language = "en";
        int streamId = 0;
        bool streamAdded;
        int horizontalRez = 720;
        int verticalRez = 480;
        int timeOffset;

        void AddVideo()
        {
            this.storage.AddVideoAudioInfo(
                new VideoAttributes
                {
                    AspectRatio = VideoAspectRatio._16by9,
                    CodingMode = VideoCodingMode.Mpeg_2,
                    HorizontalResolution = this.horizontalRez,
                    VerticalResolution = this.verticalRez,
                    Standard = (verticalRez != 480) ? VideoStandard.PAL : VideoStandard.NTSC,
                }, new List<AudioStreamItem>());
            this.videoAdded = true;
        }

        void AddStream()
        {
            this.streamId++;
            this.storage.AddStream(this.streamId,
                new SubpictureAttributes
                {
                    TrackId = 1,
                    SubpictureFormat = SubpictureFormat.Wide,
                    CodeExtension = SubpictureCodeExtension.Normal,
                    Language = language
                });
            this.streamAdded = true;
        }

        void Read()
        {
            if((this.indexLines == null) || (this.subReader == null))
            {
                return;
            }

            foreach(string line in this.indexLines)
            {
                string trimmed = line.Trim();
                if((trimmed.Length == 0) || (trimmed[0] == '#'))
                {
                    continue;
                }
                int firstColon = trimmed.IndexOf(':');
                if(firstColon <= 0)
                {
                    Debug.WriteLine("Bad line in index file " + line);
                    continue;
                }

                string command = trimmed.Substring(0, firstColon).ToLowerInvariant();
                string value = trimmed.Substring(firstColon + 1);
                switch(command)
                {
                case "size":
                    {
                        int xPosition = value.IndexOf("x", StringComparison.InvariantCultureIgnoreCase);
                        if(xPosition != -1)
                        {
                            string xRez = value.Substring(0, xPosition);
                            string yRez = value.Substring(xPosition + 1);
                            int horizRez, vertRez;
                            if(Int32.TryParse(xRez, out horizRez) && Int32.TryParse(yRez, out vertRez))
                            {
                                this.horizontalRez = horizRez;
                                this.verticalRez = vertRez;
                            }
                        }
                    }
                    break;
                case "org":
                    break;
                case "scale":
                    break;
                case "alpha":
                    break;
                case "smooth":
                    break;
                case @"fadein/out":
                    break;
                case "align":
                    break;
                case "time offset":
                case "delay":
                    {
                        int offset;
                        if(Int32.TryParse(value, out offset))
                        {
                            this.timeOffset = offset;
                        }
                    }
                    break;
                case "forced subs":
                    break;
                case "palette":
                    {
                        string[] values = value.Split(',');
                        int[] newPalette = new int[16];
                        for(int index = 0; index < 16; index++)
                        {
                            if(values.Length > index)
                            {
                                int rgbValue;
                                if(Int32.TryParse(values[index].Trim(), NumberStyles.AllowHexSpecifier,
                                    NumberFormatInfo.InvariantInfo, out rgbValue))
                                {
                                    newPalette[index] = VsRipYUVFromRGB(rgbValue);
                                }
                            }
                        }
                        this.storage.AddPalette(newPalette);
                    }
                    break;
                case "custom colors":
                    break;
                case "langidx":
                    break;
                case "id":
                    if(!this.videoAdded)
                    {
                        AddVideo();
                    }
                    this.streamAdded = false;
                    {
                        int firstComma = value.IndexOf(',');
                        if(firstComma != -1)
                        {
                            string lang = value.Substring(0, firstComma).Trim();
                            if(lang.Length != 0)
                            {
                                this.language = lang;
                            }
                        }
                    }
                    break;
                case "timestamp":
                    if(!this.streamAdded)
                    {
                        AddStream();
                    }
                    {
                        int firstComma = value.IndexOf(',');
                        if(firstComma != -1)
                        {
                            string[] timeParts = value.Substring(0, firstComma).Trim().Split(':');
                            int hours, minutes, seconds, milliseconds;
                            if((timeParts.Length == 4) && Int32.TryParse(timeParts[0], out hours) &&
                                Int32.TryParse(timeParts[1], out minutes) &&
                                Int32.TryParse(timeParts[2], out seconds) &&
                                Int32.TryParse(timeParts[3], out milliseconds))
                            {
                                string nextPart = value.Substring(firstComma + 1);
                                int secondColon = nextPart.IndexOf(':');
                                if((secondColon != -1) &&
                                    (nextPart.Substring(0, secondColon).Trim().ToLowerInvariant() == "filepos"))
                                {
                                    string posString = nextPart.Substring(secondColon + 1).Trim();
                                    int nextComma = posString.IndexOf(',', 1);
                                    if(nextComma != -1)
                                    {
                                        posString = posString.Substring(0, nextComma).Trim();
                                    }
                                    UInt32 filePos;
                                    if(UInt32.TryParse(posString, NumberStyles.AllowHexSpecifier,
                                        NumberFormatInfo.InvariantInfo, out filePos))
                                    {
                                        double pts = (double)milliseconds + seconds * 1000.0 +
                                            minutes * 1000.0 * 60.0 + hours * 1000.0 * 60.0 * 60.0;
                                        NewSubtitle(pts, filePos);
                                    }
                                }
                            }
                        }

                    }
                    break;
                default:
                    break;
                }
            }
        }

        void NewSubtitle(double pts, UInt32 filePosition)
        {
            try
            {
                this.subReader.Position = filePosition;

                List<byte[]> allBuffers = new List<byte[]>();
                int subStreamId = 0;
                byte[] firstBuffer = ReadPacket(ref subStreamId);
                if(firstBuffer == null)
                {
                    return;
                }
                int packetLength = (firstBuffer[0] << 8) + firstBuffer[1];
                allBuffers.Add(firstBuffer);
                int currentLength = firstBuffer.Length;
                while(currentLength < packetLength)
                {
                    int nextSubStreamId = 0;
                    byte[] nextBuffer = ReadPacket(ref nextSubStreamId);
                    if(nextBuffer == null)
                    {
                        return;
                    }
                    if(nextSubStreamId == subStreamId)
                    {
                        allBuffers.Add(nextBuffer);
                        currentLength += nextBuffer.Length;
                    }
                }

                byte[] buffer = new byte[packetLength];
                int currentPosition = 0;
                foreach(byte[] nextBuffer in allBuffers)
                {
                    Buffer.BlockCopy(nextBuffer, 0, buffer, currentPosition, nextBuffer.Length);
                    currentPosition += nextBuffer.Length;
                }

                this.storage.AddSubtitlePacket(this.streamId, buffer, 0, buffer.Length, pts);
            }
            catch(IOException)
            {
            }
        }

        byte[] ReadPacket(ref int subStreamId)
        {
            byte[] startPack = new byte[4];
            this.subReader.Read(startPack, 0, 4);

            if((startPack[0] != 0) || (startPack[1] != 0) || (startPack[2] != 1))
            {
                return null;
            }

            if(startPack[3] == MpegPSSplitter.PaddingStreamCode)
            {
                byte[] paddingHeader = new byte[2];
                this.subReader.Read(paddingHeader, 0, 2);
                int paddingLength = (int)(paddingHeader[0] << 8) + paddingHeader[1];
                this.subReader.Position += paddingLength;
                subStreamId = -1;
                return new byte[0];
            }

            if(startPack[3] != MpegPSSplitter.PackHeaderCode)
            {
                return null;
            }

            int headerType = this.subReader.ReadByte();
            if((headerType >> 6) == 0x01)
            {
                byte[] tempBuffer = new byte[9];
                this.subReader.Read(tempBuffer, 0, 9);
                this.subReader.Seek(tempBuffer[8] & 0x07, SeekOrigin.Current);
            }
            else if((headerType >> 4) == 0x02)
            {
                this.subReader.Seek(7, SeekOrigin.Current);
            }
            else
            {
                return null;
            }

            this.subReader.Read(startPack, 0, 4);
            if((startPack[0] != 0) || (startPack[1] != 0) || (startPack[2] != 1) ||
                (startPack[3] != MpegPSSplitter.PrivateStream1Code))
            {
                return null;
            }

            byte[] pesHeader = new byte[5];
            this.subReader.Read(pesHeader, 0, 5);
            if((pesHeader[2] & 0xc0) != 0x80)
            {
                // not a good PES packet
                return null;
            }
            int ptsDtsFlag = pesHeader[3] & 0xc0;
            int pesHeaderLength = pesHeader[4];
            this.subReader.Seek(pesHeaderLength, SeekOrigin.Current);
            subStreamId = this.subReader.ReadByte();

            int packetLength = (int)(pesHeader[0] << 8) + pesHeader[1] - 3 - pesHeaderLength - 1;

            byte[] buffer = new byte[packetLength];
            this.subReader.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public void Dispose()
        {
            if(this.subReader != null)
            {
                this.subReader.Close();
                this.subReader = null;
            }
        }

        /* bad calc that VsRip uses for yuv -> rgb that we reverse below
         * 
            y = (y-16)*255/219;
            pgc.pal[j].rgbRed = (BYTE)min(max(1.0*y + 1.4022*(u-128), 0), 255);
            pgc.pal[j].rgbGreen = (BYTE)min(max(1.0*y - 0.3456*(u-128) - 0.7145*(v-128), 0), 255);
            pgc.pal[j].rgbBlue = (BYTE)min(max(1.0*y + 1.7710*(v-128), 0) , 255);
         * */

        public static int VsRipYUVFromRGB(int rgb)
        {
            int r = (rgb >> 16) & 0xff;
            int g = (rgb >> 8) & 0xff;
            int b = rgb & 0xff;

            int y = Math.Min(Math.Max(16, ((66 * r + 129 * g + 25 * b + 128) >> 8) + 16), 255);
            int v = Math.Min(Math.Max(0, ((-38 * r - 74 * g + 112 * b + 128) >> 8) + 128), 255);
            int u = Math.Min(Math.Max(0, ((112 * r - 94 * g - 18 * b + 128) >> 8) + 128), 255);

            return (y << 16) + (u << 8) + v;
        }
    }
}
