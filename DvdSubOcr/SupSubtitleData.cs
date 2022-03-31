using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public class SupSubtitleData : SubtitleInformation, ISubtitleData
    {
        bool? isEmpty;
        PcsData data;
        bool paletteUpdated = false;

        public SupSubtitleData(int streamId, PcsData data) :
            base(Decode(data, true))
        {
            this.StreamId = streamId;
            this.data = data;
            if(this.Size.IsEmpty)
            {
                this.isEmpty = true;
            }
        }

        public int StreamId { get; private set; }

        public bool TestIfEmpty()
        {
            if(!this.isEmpty.HasValue)
            {
                this.isEmpty = (this.data.PcsObjects.Count == 0);
            }
            return this.isEmpty.Value;
        }

        public SubtitleBitmap DecodeBitmap()
        {
            SubtitleBitmap bmp = Decode(this.data, false) as SubtitleBitmap;
            if(!this.paletteUpdated)
            {
                this.RgbPalette = new List<Color>(bmp.RgbPalette).AsReadOnly();
                this.paletteUpdated = true;
            }
            return bmp;
        }

        private unsafe static ISubtitleInformation Decode(PcsData pic, bool onlyDecodeHeaderInformation)
        {
            BluRaySupPalette palette = SupDecoder.DecodePalette(pic.PaletteInfos);

            List<Color> colors = new List<Color>();
            for(int colorIndex = 0; colorIndex < 256; colorIndex++)
            {
                colors.Add(Color.FromArgb(palette.GetArgb(colorIndex)));
            }

            bool isForced = false;
            Rectangle r = Rectangle.Empty;
            for(int ioIndex = 0; ioIndex < pic.PcsObjects.Count; ioIndex++)
            {
                Rectangle ioRect = new Rectangle(pic.PcsObjects[ioIndex].Origin, pic.BitmapObjects[ioIndex][0].Size);
                if(r.IsEmpty)
                {
                    r = ioRect;
                }
                else
                {
                    r = Rectangle.Union(r, ioRect);
                }
                if(pic.PcsObjects[ioIndex].IsForced)
                {
                    isForced = true;
                }
            }

            double pts = Convert.ToDouble(pic.Pts + 45) / 90.0;
            double duration = Convert.ToDouble(pic.PtsEnd + 45) / 90.0 - pts;
            if(onlyDecodeHeaderInformation)
            {
                return new SubtitleInformation(r.Left, r.Top, r.Width, r.Height, pts, duration, colors.ToArray(), isForced);
            }

            int[] paletteBucket = new int[256];
            SubtitleBitmap bmp = new SubtitleBitmap(r.Left, r.Top, r.Width, r.Height, pts, duration, colors.ToArray(), isForced);

            int bmpPixelCount = bmp.Stride * bmp.Height;
            byte *bmpFill = (byte *)bmp.Data.ToPointer();
            for(int pixIndex = 0; pixIndex < bmpPixelCount; pixIndex++)
            {
                *bmpFill++ = 0xff;
            }

            for(int ioIndex = 0; ioIndex < pic.PcsObjects.Count; ioIndex++)
            { 
                Rectangle rect = new Rectangle(pic.PcsObjects[ioIndex].Origin, pic.BitmapObjects[ioIndex][0].Size);
                Point offset = pic.PcsObjects[ioIndex].Origin - new Size(r.Location);
                IntPtr bmpStart = new IntPtr((byte *)bmp.Data.ToPointer() + offset.Y * bmp.Stride + offset.X);
                SupDecoder.DecodeImage(pic.PcsObjects[ioIndex], pic.BitmapObjects[ioIndex], paletteBucket, bmpStart, bmp.Stride);
            }

            IList<Color> uniqueColors;
            BluRaySupPalette dvdPalette = SupDecoder.ConvertToFewerColors(palette, paletteBucket, 2, out uniqueColors);
            if(uniqueColors.Count < 2)
            {
                dvdPalette = SupDecoder.ConvertToFewerColors(palette, paletteBucket, 3, out uniqueColors);
                if(uniqueColors.Count < 2)
                {
                    dvdPalette = SupDecoder.ConvertToFewerColors(palette, paletteBucket, 4, out uniqueColors);
                }
            }
            while(uniqueColors.Count < 16)
            {
                uniqueColors.Add(Color.FromArgb(0, Color.Black));
            }
            bmp.UpdatePalette(uniqueColors.ToArray());

            Dictionary<int, int> oldToNewPalette = new Dictionary<int,int>();
            for(int palIndex = 0; palIndex < 256; palIndex++)
            {
                Color dvdColor = Color.FromArgb(dvdPalette.GetArgb(palIndex));
                oldToNewPalette[palIndex] = uniqueColors.IndexOf(dvdColor);
            }

            byte *bmpReplace = (byte *)bmp.Data.ToPointer();
            for(int pixIndex = 0; pixIndex < bmpPixelCount; pixIndex++)
            {
                byte oldValue = *bmpReplace;
                *bmpReplace++ = (byte)oldToNewPalette[oldValue];
            }

            return bmp;
        }
    }
}
