using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DvdSubOcr
{
    public class SubtitleBitmap : SubtitleInformation, IDisposable
    {
        bool isDisposed;

        public SubtitleBitmap(int left, int top, int width, int height, double pts, double duration,
            Color[] paletteEntries, bool isForced) : base(left, top, width, height, 
            pts, duration, paletteEntries, isForced)
        {
            this.Stride = (width + 3) / 4 * 4;
            this.Data = Marshal.AllocCoTaskMem(this.Stride * height);
            this.Bitmap = new Bitmap(width, height, this.Stride, PixelFormat.Format8bppIndexed, this.Data);
            ColorPalette palette = this.Bitmap.Palette;
            for(int index = 0; index < paletteEntries.Length; index++)
            {
                if(paletteEntries[index].A > 0)
                {
                    // you can get some funny blending if you let partially transparent colors into a windows bitmap
                    palette.Entries[index] = Color.FromArgb(255, paletteEntries[index]);
                }
                else
                {
                    palette.Entries[index] = paletteEntries[index];
                }
            }
            this.Bitmap.Palette = palette;
        }

        public void UpdatePalette(Color[] paletteEntries)
        {
            base.RgbPalette = new List<Color>(paletteEntries).AsReadOnly();
            ColorPalette palette = this.Bitmap.Palette;
            for(int index = 0; index < paletteEntries.Length; index++)
            {
                palette.Entries[index] = paletteEntries[index];
            }
            this.Bitmap.Palette = palette;
        }

        public int Stride { get; private set; }
        public Bitmap Bitmap { get; private set; }
        public IntPtr Data { get; private set; }

        public void Dispose()
        {
            if(!this.isDisposed)
            {
                this.isDisposed = true;
                this.Bitmap.Dispose();
                Marshal.FreeCoTaskMem(this.Data);
            }
        }
    }
}
