using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DvdSubOcr
{
    public class CharacterSplit
    {
        BlockEncode blockEncode;
        IList<bool> blockDecoded;
        HashSet<Point> splitPixels = new HashSet<Point>();

        public CharacterSplit(BlockEncode block, OcrMap ocrMap, int movieId)
        {
            this.blockEncode = block;
            this.blockDecoded = block.DecodeToBoolArray();
            this.OcrMap = ocrMap;
            this.MovieId = movieId;
        }

        public OcrMap OcrMap { get; private set; }
        public int MovieId { get; private set; }

        public class SplitResult
        {
            HashSet<Point> splitPixels = new HashSet<Point>();

            public SplitResult()
            {
                this.SplitPixels = this.splitPixels;
            }

            public OcrEntry Entry1 { get; set; }
            public OcrEntry Entry2 { get; set; }
            public ICollection<Point> SplitPixels { get; private set; }

            public void Fill(OcrEntry entry1, OcrEntry entry2, ICollection<Point> pixels)
            {
                this.Entry1 = entry1;
                this.Entry2 = entry2;
                this.splitPixels.Clear();
                foreach(Point p in pixels)
                {
                    this.splitPixels.Add(p);
                }
            }

            public void FillIfBetter(OcrEntry entry, ICollection<Point> pixels)
            {
                if(this.Entry2 != null)
                {
                    return;
                }
                if(this.Entry1 != null)
                {
                    BlockEncode oldEncode = new BlockEncode(Point.Empty, this.Entry1.FullEncode, 0);
                    BlockEncode newEncode = new BlockEncode(Point.Empty, entry.FullEncode, 0);
                    if(newEncode.PixelCount <= oldEncode.PixelCount)
                    {
                        return;
                    }
                }

                this.Entry1 = entry;
                this.splitPixels.Clear();
                foreach(Point p in pixels)
                {
                    this.splitPixels.Add(p);
                }
            }
        }

        public static void AddEncodeToCollection(BlockEncode encode, Point offset, 
            ICollection<Point> collection)
        {
            Point decodePt = offset;
            int widthLeft = encode.Width;
            foreach(char c in encode.Encode)
            {
                int hex = BlockEncode.HexCharToValue(c);
                if((hex & 8) != 0)
                {
                    collection.Add(decodePt);
                }
                decodePt.X++;
                if((hex & 4) != 0)
                {
                    collection.Add(decodePt);
                }
                decodePt.X++;
                if((hex & 2) != 0)
                {
                    collection.Add(decodePt);
                }
                decodePt.X++;
                if((hex & 1) != 0)
                {
                    collection.Add(decodePt);
                }
                decodePt.X++;

                widthLeft -= 4;
                if(widthLeft == 0)
                {
                    widthLeft = encode.Width;
                    decodePt.X -= widthLeft;
                    decodePt.Y++;
                }
            }
        }

        void FindHighDefEntries(bool perfectSplit, out OcrEntry entry1, out OcrEntry entry2)
        {
            entry1 = null;
            entry2 = null;

            Rectangle blockRect = new Rectangle(0, 0, this.blockEncode.Width,
                this.blockEncode.Height);
            int stride = (blockRect.Width + 3) / 4 * 4;
            IntPtr hMem = Marshal.AllocCoTaskMem(stride * blockRect.Height);
            try
            {
                unsafe
                {
                    byte* data = (byte*)hMem.ToPointer();
                    for(int y = 0; y < blockRect.Height; y++)
                    {
                        int blockRow = y * blockRect.Width;
                        int dataRow = y * stride;
                        for(int x = 0; x < blockRect.Width; x++)
                        {
                            if(this.blockDecoded[blockRow + x])
                            {
                                if(splitPixels.Contains(new Point(x, y)))
                                {
                                    data[dataRow + x] = 2;
                                }
                                else
                                {
                                    data[dataRow + x] = 1;
                                }
                            }
                            else
                            {
                                data[dataRow + x] = 0;
                            }
                        }
                    }
                }

                ContiguousEncode contigEncode = new ContiguousEncode(hMem, blockRect.Width,
                    blockRect.Height, stride);

                IList<BlockEncode> encodes1 = contigEncode.FindEncodes(new int[] { 2 }, null);
                if(encodes1.Count == 1)
                {
                    foreach(OcrEntry entry in this.OcrMap.FindMatches(encodes1[0].FullEncode, this.MovieId, true))
                    {
                        entry1 = entry;
                        break;
                    }
                }

                if(!perfectSplit || (entry1 != null))
                {
                    IList<BlockEncode> encodes2 = contigEncode.FindEncodes(new int[] { 1 }, null);
                    if(encodes2.Count == 1)
                    {
                        foreach(OcrEntry entry in this.OcrMap.FindMatches(encodes2[0].FullEncode, this.MovieId, true))
                        {
                            entry2 = entry;
                            break;
                        }
                    }
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(hMem);
            }
        }

        bool FindHighDefSplit(SplitResult result, bool exactMovieMatch, HashSet<Point> pixelsInBlock, int xOffset, int yOffset)
        {
            this.splitPixels.Clear();

            if(yOffset == 0)
            {
                foreach(Point p in pixelsInBlock)
                {
                    if(p.X >= xOffset)
                    {
                        this.splitPixels.Add(p);
                    }
                }
            }
            else
            {
                foreach(Point p in pixelsInBlock)
                {
                    if(p.Y >= yOffset)
                    {
                        if(p.X >= xOffset)
                        {
                            this.splitPixels.Add(p);
                        }
                    }
                    else
                    {
                        if(p.X + (p.Y - yOffset) / 2 >= xOffset)
                        {
                            this.splitPixels.Add(p);
                        }
                    }
                }
            }

            OcrEntry entry1, entry2;
            FindHighDefEntries(exactMovieMatch, out entry1, out entry2);
            if((entry1 != null) && (entry2 != null))
            {
                result.Fill(entry1, entry2, this.splitPixels);
                return true;
            }
            if(!exactMovieMatch)
            {
                if(entry1 != null)
                {
                    result.FillIfBetter(entry1, this.splitPixels);
                }
                if(entry2 != null)
                {
                    result.FillIfBetter(entry2, this.splitPixels);
                }
            }
            return false;
        }

        public bool AutoTestSplits(SplitResult result, bool exactMovieMatch, bool isHighDef)
        {
            HashSet<Point> pixelsInBlock = new HashSet<Point>();
            for(int x = 0; x < this.blockEncode.Width; x++)
            {
                for(int y = 0; y < this.blockEncode.Height; y++)
                {
                    int pixelOffset = y * this.blockEncode.Width + x;
                    if(this.blockDecoded[pixelOffset])
                    {
                        pixelsInBlock.Add(new Point(x, y));
                    }
                }
            }

            if(isHighDef)
            {
                DateTime dtStart = DateTime.Now;
                for(int yOffset = 0; yOffset < this.blockEncode.Height; yOffset++)
                {
                    DateTime dtNext = DateTime.Now;
                    if((dtNext - dtStart) > new TimeSpan(0, 0, 2))
                    {
                        break;
                    }
                    if((yOffset > 0) && (yOffset < 6))
                    {
                        continue;
                    }

                    for(int xOffset = 6; xOffset < this.blockEncode.TrueWidth; xOffset++)
                    {
                        if(FindHighDefSplit(result, exactMovieMatch, pixelsInBlock, xOffset, yOffset))
                        {
                            for(int xDelta = 2; xDelta > 0; xDelta--)
                            {
                                for(int yDelta = 6; yDelta < this.blockEncode.Height; yDelta++)
                                {
                                    if((xOffset + xDelta < this.blockEncode.TrueWidth) && FindHighDefSplit(result, true, pixelsInBlock, xOffset + xDelta, yDelta))
                                    {
                                        return true;
                                    }
                                }
                            }
                            return true;
                        }
                    }
                }
                return (result.Entry1 != null);
            }
            else
            {
                bool checkAngledSplit = (this.blockEncode.TrueWidth * this.blockEncode.Height) < SubConstants.MaxSizeCheckAngledAutoSplit;

                HashSet<Point> matchPixels = new HashSet<Point>();
                foreach(OcrEntry entry1 in this.OcrMap.GetMatchesForMovie(this.MovieId, exactMovieMatch))
                {
                    BlockEncode block = new BlockEncode(entry1.FullEncode);
                    int diffX = this.blockEncode.TrueWidth - block.TrueWidth;
                    int diffY = this.blockEncode.Height - block.Height;
                    if((diffX < 0) || (diffY < 0))
                    {
                        continue;
                    }

                    matchPixels.Clear();
                    AddEncodeToCollection(block, Point.Empty, matchPixels);

                    for(int xOffset = 0; xOffset <= diffX; xOffset += Math.Max(diffX, 1))
                    {
                        for(int yOffset = 0; (yOffset == 0) || (checkAngledSplit && (yOffset <= diffY)); yOffset++)
                        {
                            this.splitPixels.Clear();
                            bool isMatch = true;
                            foreach(Point p in matchPixels)
                            {
                                Point pOffset = new Point(p.X + xOffset, p.Y + yOffset);
                                if(!pixelsInBlock.Contains(pOffset))
                                {
                                    isMatch = false;
                                    break;
                                }
                                this.splitPixels.Add(pOffset);
                            }

                            if(isMatch)
                            {
                                IList<BlockEncode> encodes = FindNonSplitEncodes();
                                if(encodes.Count == 1)
                                {
                                    foreach(OcrEntry entry2 in this.OcrMap.FindMatches(encodes[0].FullEncode, this.MovieId, isHighDef))
                                    {
                                        if((xOffset < encodes[0].Origin.X) ||
                                            ((xOffset == encodes[0].Origin.X) &&
                                            (yOffset < encodes[0].Origin.Y)))
                                        {
                                            result.Fill(entry1, entry2, this.splitPixels);
                                        }
                                        else
                                        {
                                            result.Fill(entry2, entry1, this.splitPixels);
                                        }
                                        return true;
                                    }

                                    if(!SubConstants.CheapSplitSymbols.Contains(entry1.OcrCharacter.Value))
                                    {
                                        result.FillIfBetter(entry1, this.splitPixels);
                                    }
                                }
                            }
                        }
                    }
                    //Rectangle entryRect = entry.CalculateBounds();
                }
            }

            return (result.Entry1 != null);
        }

        public void FindSplitEncodes(IEnumerable<Point> splitPoints, 
            out IList<BlockEncode> encodes1, out IList<BlockEncode> encodes2)
        {
            this.splitPixels.Clear();
            foreach(Point p in splitPoints)
            {
                this.splitPixels.Add(p);
            }
            FindSplitEncodes(out encodes1, out encodes2);
        }

        void FindSplitEncodes(out IList<BlockEncode> encodes1, out IList<BlockEncode> encodes2)
        {
            encodes1 = new List<BlockEncode>();
            encodes2 = new List<BlockEncode>();

            Rectangle blockRect = new Rectangle(0, 0, this.blockEncode.Width, 
                this.blockEncode.Height);
            int stride = (blockRect.Width + 3) / 4 * 4;
            IntPtr hMem = Marshal.AllocCoTaskMem(stride * blockRect.Height);
            try
            {
                unsafe
                {
                    byte* data = (byte*)hMem.ToPointer();
                    for(int y = 0; y < blockRect.Height; y++)
                    {
                        int blockRow = y * blockRect.Width;
                        int dataRow = y * stride;
                        for(int x = 0; x < blockRect.Width; x++)
                        {
                            if(this.blockDecoded[blockRow + x])
                            {
                                if(splitPixels.Contains(new Point(x, y)))
                                {
                                    data[dataRow + x] = 2;
                                }
                                else
                                {
                                    data[dataRow + x] = 1;
                                }
                            }
                            else
                            {
                                data[dataRow + x] = 0;
                            }
                        }
                    }
                }

                ContiguousEncode contigEncode = new ContiguousEncode(hMem, blockRect.Width,
                    blockRect.Height, stride);

                encodes1 = contigEncode.FindEncodes(new int[] { 1 }, null);
                encodes2 = contigEncode.FindEncodes(new int[] { 2 }, null);
            }
            finally
            {
                Marshal.FreeCoTaskMem(hMem);
            }
        }

        IList<BlockEncode> FindNonSplitEncodes()
        {
            Rectangle blockRect = new Rectangle(0, 0, this.blockEncode.Width,
                this.blockEncode.Height);
            int stride = (blockRect.Width + 3) / 4 * 4;
            IntPtr hMem = Marshal.AllocCoTaskMem(stride * blockRect.Height);
            try
            {
                unsafe
                {
                    byte* data = (byte*)hMem.ToPointer();
                    for(int y = 0; y < blockRect.Height; y++)
                    {
                        for(int x = 0; x < blockRect.Width; x++)
                        {
                            if(this.blockDecoded[y * blockRect.Width + x] &&
                                (!splitPixels.Contains(new Point(x, y))))
                            {
                                data[y * stride + x] = 1;
                            }
                            else
                            {
                                data[y * stride + x] = 0;
                            }
                        }
                    }
                }

                ContiguousEncode contigEncode = new ContiguousEncode(hMem, blockRect.Width,
                    blockRect.Height, stride);

                return contigEncode.FindEncodes(new int[] { 1 }, null);
            }
            finally
            {
                Marshal.FreeCoTaskMem(hMem);
            }
        }
    }
}
