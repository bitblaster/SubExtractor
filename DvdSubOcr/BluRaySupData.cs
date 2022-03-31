using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    /** PGS composition state */
    public enum CompositionState
    {
        /** normal: doesn't have to be complete */
        Normal,
        /** acquisition point */
        AcquPoint,
        /** epoch start - clears the screen */
        EpochStart,
        /** epoch continue */
        EpochContinue,
        /** unknown value */
        Invalid
    }

    public class PaletteInfo
    {
        /// <summary>
        /// number of palette entries
        /// </summary>
        public int PaletteSize { get; set; }

        /// <summary>
        /// raw palette data
        /// </summary>
        public byte[] PaletteBuffer { get; set; }

        public PaletteInfo()
        {
        }

        public PaletteInfo(PaletteInfo paletteInfo)
        {
            PaletteSize = paletteInfo.PaletteSize;
            PaletteBuffer = new byte[paletteInfo.PaletteBuffer.Length];
            Buffer.BlockCopy(paletteInfo.PaletteBuffer, 0, PaletteBuffer, 0, PaletteBuffer.Length);
        }
    }

    public class ImageObjectFragment
    {
        /// <summary>
        /// size of this part of the RLE buffer
        /// </summary>
        public int ImagePacketSize { get; set; }

        /// <summary>
        /// Buffer for raw image data fragment
        /// </summary>
        public byte[] ImageBuffer { get; set; }
    }

    public class PcsObject
    {
        public int ObjectId;
        public int WindowId;
        public bool IsForced;
        public Point Origin;
    }

    public class PcsData
    {
        public int CompNum;
        public CompositionState CompositionState;
        public bool PaletteUpdate;
        public long Pts;
        public long PtsEnd;
        public Size Size;
        public int FramesPerSecondType;
        public int PaletteId;
        public IList<PcsObject> PcsObjects;
        public string Message;
        public IList<IList<OdsData>> BitmapObjects;
        public IList<PaletteInfo> PaletteInfos;
    }

    public class PdsData
    {
        public string Message;
        public int PaletteId;
        public int PaletteVersion;
        public PaletteInfo PaletteInfo;
    }

    public class OdsData
    {
        public int ObjectId;
        public int ObjectVersion;
        public string Message;
        public bool IsFirst;
        public Size Size;
        public ImageObjectFragment Fragment;
    }

    public static class SupDecoder
    {
        private const int AlphaCrop = 14;

        public static BluRaySupPalette DecodePalette(IList<PaletteInfo> paletteInfos)
        {
            BluRaySupPalette palette = new BluRaySupPalette(256);
            // by definition, index 0xff is always completely transparent
            // also all entries must be fully transparent after initialization

            bool fadeOut = false;
            for(int j = 0; j < paletteInfos.Count; j++)
            {
                PaletteInfo p = paletteInfos[j];
                int index = 0;

                for(int i = 0; i < p.PaletteSize; i++)
                {
                    // each palette entry consists of 5 bytes
                    int palIndex = p.PaletteBuffer[index];
                    int y = p.PaletteBuffer[++index];
                    int cr = p.PaletteBuffer[++index];
                    int cb = p.PaletteBuffer[++index];
                    int alpha = p.PaletteBuffer[++index];

                    int alphaOld = palette.GetAlpha(palIndex);
                    // avoid fading out
                    if(alpha >= alphaOld)
                    {
                        if(alpha < AlphaCrop)
                        {// to not mess with scaling algorithms, make transparent color black
                            y = 16;
                            cr = 128;
                            cb = 128;
                        }
                        palette.SetAlpha(palIndex, alpha);
                    }
                    else
                    {
                        fadeOut = true;
                    }

                    palette.SetYCbCr(palIndex, y, cb, cr);
                    index++;
                }
            }
            if(fadeOut)
            {
                System.Diagnostics.Debug.Print("fade out detected -> patched palette\n");
            }
            return palette;
        }

        unsafe static void FillData(byte *pixData, int index, int width, int stride, int value, int count)
        {
            byte b = (byte)value;
            int y = index / width;
            int x = index % width;
            byte* pixLineStart = pixData + (y * stride);
            byte* pixStart = pixLineStart + x;
            int writeLen = Math.Min(count, width - x);
            count -= writeLen;
            while(writeLen > 0)
            {
                *pixStart++ = b;
                writeLen--;
            }
            while(count > width)
            {
                pixLineStart += stride;
                pixStart = pixLineStart;
                writeLen = width;
                count -= writeLen;
                while(writeLen > 0)
                {
                    *pixStart++ = b;
                    writeLen--;
                }
            }
            if(count > 0)
            {
                pixStart = pixLineStart + stride;
                writeLen = count;
                while(count > 0)
                {
                    *pixStart++ = b;
                    count--;
                }
            }
        }

        public static void DecodeImage(PcsObject pcs, IList<OdsData> data, int[] paletteBucket, IntPtr bitmapPtr, int stride)
        {
            if(data.Count != 1)
            {
                throw new ArgumentOutOfRangeException("data.Count");
            }

            int w = data[0].Size.Width;
            int h = data[0].Size.Height;

            unsafe
            {
                byte* pixData = (byte*)bitmapPtr.ToPointer();
                byte[] buf = data[0].Fragment.ImageBuffer;

                int index = 0;
                int ofs = 0;
                int xpos = 0;
                do
                {
                    int b = buf[index++] & 0xff;
                    if(b == 0)
                    {
                        b = buf[index++] & 0xff;
                        if(b == 0)
                        {
                            // next line
                            ofs = (ofs / w) * w;
                            if(xpos < w)
                            {
                                ofs += w;
                            }
                            xpos = 0;
                        }
                        else
                        {
                            int size;
                            if((b & 0xC0) == 0x40)
                            {
                                // 00 4x xx -> xxx zeroes
                                size = ((b - 0x40) << 8) + (buf[index++] & 0xff);
                                FillData(pixData, ofs, w, stride, 0, size);
                                ofs += size;
                                xpos += size;
                                paletteBucket[0] += size;
                            }
                            else if((b & 0xC0) == 0x80)
                            {
                                // 00 8x yy -> x times value y
                                size = (b - 0x80);
                                b = buf[index++] & 0xff;
                                FillData(pixData, ofs, w, stride, b, size);
                                ofs += size;
                                xpos += size;
                                paletteBucket[b] += size;
                            }
                            else if((b & 0xC0) != 0)
                            {
                                // 00 cx yy zz -> xyy times value z
                                size = ((b - 0xC0) << 8) + (buf[index++] & 0xff);
                                b = buf[index++] & 0xff;
                                FillData(pixData, ofs, w, stride, b, size);
                                ofs += size;
                                xpos += size;
                                paletteBucket[b] += size;
                            }
                            else
                            {
                                // 00 xx -> xx times 0
                                FillData(pixData, ofs, w, stride, 0, b);
                                ofs += b;
                                xpos += b;
                                paletteBucket[0] += b;
                            }
                        }
                    }
                    else
                    {
                        FillData(pixData, ofs, w, stride, b, 1);
                        ofs++;
                        xpos++;
                        paletteBucket[b]++;
                    }
                } while(index < buf.Length);
            }
        }

        public static Bitmap DecodeImage(PcsObject pcs, IList<OdsData> data, BluRaySupPalette palette, int[] paletteBucket)
        {
            int w = data[0].Size.Width;
            int h = data[0].Size.Height;

            //Bitmap bm = new Bitmap(w, h);
            FastBitmap bm = new FastBitmap(new Bitmap(w, h));
            bm.LockImage();

            int index = 0;
            int ofs = 0;
            int xpos = 0;

            // just for multi-packet support, copy all of the image data in one common buffer
            byte[] buf;
            int bufSize;
            if(data.Count > 1)
            {
                bufSize = 0;
                foreach(OdsData ods in data)
                {
                    bufSize += ods.Fragment.ImagePacketSize;
                }
                buf = new byte[bufSize];
                int offset = 0;
                foreach(OdsData ods in data)
                {
                    Buffer.BlockCopy(ods.Fragment.ImageBuffer, 0, buf, offset, ods.Fragment.ImagePacketSize);
                    offset += ods.Fragment.ImagePacketSize;
                }
            }
            else
            {
                buf = data[0].Fragment.ImageBuffer;
                bufSize = data[0].Fragment.ImagePacketSize;
            }

            index = 0;
            do
            {
                int b = buf[index++] & 0xff;
                if(b == 0)
                {
                    b = buf[index++] & 0xff;
                    if(b == 0)
                    {
                        // next line
                        ofs = (ofs / w) * w;
                        if(xpos < w)
                        {
                            ofs += w;
                        }
                        xpos = 0;
                    }
                    else
                    {
                        int size;
                        if((b & 0xC0) == 0x40)
                        {
                            // 00 4x xx -> xxx zeroes
                            size = ((b - 0x40) << 8) + (buf[index++] & 0xff);
                            for(int i = 0; i < size; i++)
                            {
                                PutPixel(bm, ofs++, 0, palette);
                            }
                            paletteBucket[0] += size;
                            xpos += size;
                        }
                        else if((b & 0xC0) == 0x80)
                        {
                            // 00 8x yy -> x times value y
                            size = (b - 0x80);
                            b = buf[index++] & 0xff;
                            for(int i = 0; i < size; i++)
                            {
                                PutPixel(bm, ofs++, b, palette);
                            }
                            paletteBucket[b] += size;
                            xpos += size;
                        }
                        else if((b & 0xC0) != 0)
                        {
                            // 00 cx yy zz -> xyy times value z
                            size = ((b - 0xC0) << 8) + (buf[index++] & 0xff);
                            b = buf[index++] & 0xff;
                            for(int i = 0; i < size; i++)
                            {
                                PutPixel(bm, ofs++, b, palette);
                            }
                            paletteBucket[b] += size;
                            xpos += size;
                        }
                        else
                        {
                            // 00 xx -> xx times 0
                            for(int i = 0; i < b; i++)
                            {
                                PutPixel(bm, ofs++, 0, palette);
                            }
                            paletteBucket[0] += b;
                            xpos += b;
                        }
                    }
                }
                else
                {
                    PutPixel(bm, ofs++, b, palette);
                    paletteBucket[b]++;
                    xpos++;
                }
            } while(index < buf.Length);

            bm.UnlockImage();
            return bm.GetBitmap();
        }

        private static void PutPixel(FastBitmap bmp, int index, int color, BluRaySupPalette palette)
        {
            int x = index % bmp.Width;
            int y = index / bmp.Width;
            if(color > 0 && x < bmp.Width && y < bmp.Height)
                bmp.SetPixel(x, y, Color.FromArgb(palette.GetArgb(color)));
        }

        private static int TestColorDiff(Color c1, Color c2, int maxDiff)
        {
            int rDiff = Math.Abs(c1.R - c2.R);
            if(rDiff > maxDiff)
            {
                return -1;
            }
            int gDiff = Math.Abs(c1.G - c2.G);
            if(gDiff > maxDiff)
            {
                return -1;
            }
            int bDiff = Math.Abs(c1.B - c2.B);
            if(bDiff > maxDiff)
            {
                return -1;
            }
            return Math.Max(rDiff, Math.Max(gDiff, bDiff));
        }

        class ColorData
        {
            public ColorData(Color c) 
            {
                this.Color = c;
                this.OriginalIndexes = new List<int>();
            }
            
            public ColorData(Color c, IEnumerable<int> indexes)
            {
                this.Color = c;
                this.OriginalIndexes = new List<int>(indexes);
            }

            public Color Color;
            public IList<int> OriginalIndexes;
            public int BucketCount;
        }

        const int MinimumConvertableAlpha = AlphaCrop;

        public static BluRaySupPalette ConvertToFewerColors(BluRaySupPalette palette, int[] paletteBucket, int maxColors, out IList<Color> uniqueColors)
        {
            List<ColorData> colorDataList = new List<ColorData>();
            Dictionary<Color, ColorData> colorDataDict = new Dictionary<Color, ColorData>();
            for(int index = 0; index < 256; index++)
            {
                Color c = Color.FromArgb(palette.GetArgb(index));
                if((c.A >= MinimumConvertableAlpha) && (paletteBucket[index] > 0))
                {
                    c = Color.FromArgb(255, c);
                    ColorData data;
                    if(!colorDataDict.TryGetValue(c, out data))
                    {
                        data = new ColorData(c);
                        colorDataDict[c] = data;
                        colorDataList.Add(data);
                    }
                    data.BucketCount += paletteBucket[index];
                    data.OriginalIndexes.Add(index);
                }
            }

            colorDataList.Sort(
                delegate(ColorData data1, ColorData data2)
                {
                    return -data1.BucketCount.CompareTo(data2.BucketCount);
                });

            if(colorDataList.Count >= maxColors)
            {
                List<ColorData> newColorDataList = new List<ColorData>();
                int maxDiff = 8;
                do
                {
                    newColorDataList.Clear();
                    newColorDataList.Add(new ColorData(colorDataList[0].Color, colorDataList[0].OriginalIndexes));

                    foreach(ColorData data in colorDataList.Skip(1))
                    {
                        int bestDiff = -1;
                        ColorData bestData = null;
                        foreach(ColorData newData in newColorDataList)
                        {
                            int colorDiff = TestColorDiff(data.Color, newData.Color, maxDiff);
                            if((colorDiff >= 0) && ((bestDiff == -1) || (colorDiff < bestDiff)))
                            {
                                bestDiff = colorDiff;
                                bestData = newData;
                            }
                        }
                        if(bestData != null)
                        {
                            foreach(int oldIndex in data.OriginalIndexes)
                            {
                                bestData.OriginalIndexes.Add(oldIndex);
                            }
                        }
                        else
                        {
                            newColorDataList.Add(new ColorData(data.Color, data.OriginalIndexes));
                            if(newColorDataList.Count > maxColors)
                            {
                                break;
                            }
                        }
                    }
                    maxDiff += 8;
                } while(newColorDataList.Count > maxColors);

                colorDataList = newColorDataList;
            }

            BluRaySupPalette smallPalette = new BluRaySupPalette(256);
            uniqueColors = new List<Color>();
            foreach(ColorData data in colorDataList)
            {
                uniqueColors.Add(data.Color);
                foreach(int index in data.OriginalIndexes)
                {
                    smallPalette.SetAlpha(index, 255);
                    smallPalette.SetRgb(index, data.Color.R, data.Color.G, data.Color.B);
                }
            }
            return smallPalette;
        }
    }
}
