using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using DvdSubOcr;
using DvdNavigatorCrm;

namespace DvdSubOcr
{
    public class ContiguousEncode
    {
        IntPtr buffer;
        int width;
        int stride;
        int height;
        OcrMap ocrMap;
        List<string> splitEncodes = new List<string>();

        public ContiguousEncode(IntPtr buffer, int width, int height, int stride)
        {
            this.buffer = buffer;
            this.width = width;
            this.height = height;
            this.stride = stride;
            this.SplitEncodes = this.splitEncodes.AsReadOnly();
        }

        public ContiguousEncode(IntPtr buffer, int width, int height, int stride, OcrMap map)
            : this(buffer, width, height, stride)
        {
            this.ocrMap = map;
        }

        public IList<string> SplitEncodes { get; private set; }

        static void MergeRectangles(List<RectangleWithColors> sections, RectangleWithColors newRect)
        {
            int intersectRectIndex = sections.Count;
            sections.Add(newRect);
            while(intersectRectIndex != -1)
            {
                RectangleWithColors intersectRect = sections[intersectRectIndex];
                int newIntersectRectIndex = -1;
                for(int rectIndex = 0; rectIndex < sections.Count; rectIndex++)
                {
                    if((rectIndex != intersectRectIndex) && 
                        sections[rectIndex].Rectangle.IntersectsWith(intersectRect.Rectangle))
                    {
                        sections[rectIndex].MergeWith(intersectRect);
                        sections.RemoveAt(intersectRectIndex);
                        newIntersectRectIndex = (intersectRectIndex > rectIndex) ? rectIndex : rectIndex - 1;
                        break;
                    }
                }
                intersectRectIndex = newIntersectRectIndex;
            }
        }
            

        public IList<RectangleWithColors> FindSections(IEnumerable<int> colorIndexes, bool is1080p)
        {
            bool[] goodColors = new bool[256];
            for(int index = 0; index < 256; index++)
            {
                goodColors[index] = false;
            }
            foreach(int index in colorIndexes)
            {
                goodColors[index] = true;
            }

            int sectionMarginX = SubConstants.SectionMarginX;
            int sectionMarginY = SubConstants.SectionMarginY;
            if(is1080p)
            {
                sectionMarginX *= 2;
                sectionMarginY *= 2;
            }

            List<RectangleWithColors> sections = new List<RectangleWithColors>();
            unsafe
            {
                byte* data = (byte*)this.buffer.ToPointer();

                for(int y = 0; y < this.height; y++)
                {
                    for(int x = 0; x < this.width; x++)
                    {
                        Point p = new Point(x, y);
                        if(goodColors[*data])
                        {
                            Rectangle newRect = new Rectangle(
                                x - sectionMarginX, y - sectionMarginY,
                                2 * sectionMarginX + 1, 2 * sectionMarginY + 1);

                            MergeRectangles(sections, new RectangleWithColors(newRect, *data));
                        }
                        data++;
                    }
                    data += (this.stride - this.width);
                }
            }
            foreach(RectangleWithColors rect in sections)
            {
                rect.TrimRectangle(sectionMarginX, sectionMarginY);
            }
            return sections;
        }

        public IEnumerable<BlocksAndPalette> FindAllEncodesAndPalettes(IList<int> colorIndexes)
        {
            int maxCombinations = Convert.ToInt32(Math.Pow(2, colorIndexes.Count));
            for(int bitIndex = 1; bitIndex < maxCombinations; bitIndex++)
            {
                List<int> subColors = new List<int>();
                int roller = 1;
                for(int colorIndex = 0; colorIndex < colorIndexes.Count; colorIndex++)
                {
                    if((bitIndex & roller) != 0)
                    {
                        subColors.Add(colorIndexes[colorIndex]);
                    }
                    roller <<= 1;
                }

                IList<BlockEncode> blocks = FindEncodes(subColors, colorIndexes);

                int averagePixelCount = 0;
                int countedBlocks = 0;
                int pixelCount = 0;
                int colorBitFlags = 0;
                foreach(BlockEncode block in blocks)
                {
                    if(block.PixelCount >= SubConstants.MinimumCountablePixelCount)
                    {
                        pixelCount += block.PixelCount;
                        countedBlocks++;
                    }
                    colorBitFlags |= block.ColorBitFlags;
                }

                int colorCount = 0;
                for(int index = 0, bitFlag = 1; index < 16; index++, bitFlag <<= 1)
                {
                    if((bitFlag & colorBitFlags) == bitFlag)
                    {
                        colorCount++;
                    }
                }
                // if the palette doesn't use all the colors skip it. A palette with fewer colors will contain
                // the same block data
                //if(colorCount == colorIndexes.Count)
                {
                    if(countedBlocks != 0)
                    {
                        averagePixelCount = Convert.ToInt32(pixelCount / countedBlocks);
                    }
                    else if(blocks.Count != 0)
                    {
                        foreach(BlockEncode block in blocks)
                        {
                            pixelCount += block.PixelCount;
                        }
                        averagePixelCount = Convert.ToInt32(pixelCount / blocks.Count);
                    }

                    yield return new BlocksAndPalette(blocks, subColors, this.splitEncodes,
                        averagePixelCount, countedBlocks);
                }
            }
        }

        public IList<BlockEncode> FindEncodes(IEnumerable<int> colorIndexes, IEnumerable<int> encodeColors)
        {
            this.splitEncodes.Clear();

            bool[] colorValidity = new bool[256];
            foreach(int index in colorIndexes)
            {
                colorValidity[index] = true;
            }

            bool[] encodeValidity;
            if(encodeColors != null)
            {
                encodeValidity = new bool[256];
                foreach(int index in encodeColors)
                {
                    encodeValidity[index] = true;
                }
            }
            else
            {
                encodeValidity = colorValidity;
            }

            int[] encodeGroupMap = new int[(this.width + 4) * this.height];
            List<Rectangle> groups = new List<Rectangle>();
            List<BlockEncode> encodes = new List<BlockEncode>();
            unsafe
            {
                byte* data = (byte*)this.buffer.ToPointer();
                int encodeGroupOffset = 0;
                bool emptyPreviousRow = true;
                for (int y = 0; y < height; y++)
                {
                    int groupId = 0;
                    bool emptyRow = true;
                    for (int x = 0; x < width; x++)
                    {
                        if (colorValidity[*data])
                        {
                            emptyRow = false;
                            Rectangle r = new Rectangle(x, y, 1, 1);
                            if (!emptyPreviousRow)
                            {
                                int lastRowOffset = encodeGroupOffset - this.width - 4;
                                if ((groupId == 0) && (x != 0))
                                {
                                    groupId = encodeGroupMap[lastRowOffset - 1];
                                }
                                if (groupId == 0)
                                {
                                    groupId = encodeGroupMap[lastRowOffset];
                                }
                                if (x < this.width - 1)
                                {
                                    if (groupId == 0)
                                    {
                                        groupId = encodeGroupMap[lastRowOffset + 1];
                                    }
                                    else
                                    {
                                        int otherGroupId = encodeGroupMap[lastRowOffset + 1];
                                        if ((otherGroupId != 0) && (otherGroupId != groupId))
                                        {
                                            Rectangle otherRect = groups[otherGroupId - 1];
                                            groups[groupId - 1] = Rectangle.Union(groups[groupId - 1], otherRect);
                                            for (int y2 = otherRect.Top; y2 < otherRect.Bottom; y2++)
                                            {
                                                int ptOffset = y2 * (this.width + 4) + otherRect.Left;
                                                for (int x2 = 0; x2 < otherRect.Width; x2++)
                                                {
                                                    if (encodeGroupMap[ptOffset] == otherGroupId)
                                                    {
                                                        encodeGroupMap[ptOffset] = groupId;
                                                    }
                                                    ptOffset++;
                                                }
                                            }
                                            groups[otherGroupId - 1] = Rectangle.Empty;
                                        }
                                    }
                                }
                            }

                            if (groupId == 0)
                            {
                                groups.Add(r);
                                groupId = groups.Count;
                            }
                            else
                            {
                                groups[groupId - 1] = Rectangle.Union(groups[groupId - 1], r);
                            }
                            encodeGroupMap[encodeGroupOffset] = groupId;
                        }
                        else
                        {
                            groupId = 0;
                        }
                        data++;
                        encodeGroupOffset++;
                    }
                    encodeGroupOffset += 4;
                    data += (stride - width);
                    emptyPreviousRow = emptyRow;
                }

                int encodeGroupId = 0;
                foreach (Rectangle rect in groups)
                {
                    encodeGroupId++;
                    if (rect.IsEmpty)
                    {
                        continue;
                    }

                    int encodeWidth = (rect.Width + 3) / 4 * 4;
                    StringBuilder sb = new StringBuilder(encodeWidth.ToString("d3"));
                    for (int y = rect.Top; y < rect.Bottom; y++)
                    {
                        int ptOffset = y * (this.width + 4) + rect.Left;
                        int ptOffsetEnd = ptOffset + rect.Width;
                        while(ptOffset < ptOffsetEnd)
                        {
                            sb.Append(ValueToHexChar(
                                encodeGroupMap[ptOffset] == encodeGroupId,
                                encodeGroupMap[ptOffset + 1] == encodeGroupId,
                                encodeGroupMap[ptOffset + 2] == encodeGroupId,
                                encodeGroupMap[ptOffset + 3] == encodeGroupId));
                            ptOffset += 4;
                        }
                    }

                    Rectangle colorRect = Rectangle.Intersect(Rectangle.Inflate(rect, 1, 1), 
                        new Rectangle(0, 0, this.width, this.height));
                    byte* encodeRow = (byte*)this.buffer.ToPointer() + colorRect.Top * stride + colorRect.Left;
                    int encodeColorBits = 0;
                    for(int y = colorRect.Top; y < colorRect.Bottom; y++)
                    {
                        byte* encodeData = encodeRow;
                        byte* encodeDataEnd = encodeData + colorRect.Width;
                        while(encodeData < encodeDataEnd)
                        {
                            byte color = *encodeData;
                            if(encodeValidity[color])
                            {
                                encodeColorBits |= (1 << color);
                            }
                            encodeData++;
                        }
                        encodeRow += stride;
                    }

                    encodes.Add(new BlockEncode(rect.Location, sb.ToString(), encodeColorBits));
                }
            }

            if(this.ocrMap != null)
            {
                List<BlockEncode> splitEncodes = new List<BlockEncode>();
                foreach(BlockEncode encode in encodes)
                {
                    RecursiveSplitEncode(encode, splitEncodes);
                }
                encodes = splitEncodes;
            }

            encodes.Sort();
            return encodes;
        }

        private void RecursiveSplitEncode(BlockEncode encode, List<BlockEncode> encodeList)
        {
            OcrMap.SplitMapEntry split = this.ocrMap.FindSplit(encode.FullEncode);
            if(split != null)
            {
                this.splitEncodes.Add(encode.FullEncode);

                BlockEncode splitEncode1 = new BlockEncode(
                    encode.Origin + new Size(split.Split1.Offset),
                    split.Split1.FullEncode, encode.ColorBitFlags);
                RecursiveSplitEncode(splitEncode1, encodeList);

                BlockEncode splitEncode2 = new BlockEncode(
                    encode.Origin + new Size(split.Split2.Offset),
                    split.Split2.FullEncode, encode.ColorBitFlags);
                RecursiveSplitEncode(splitEncode2, encodeList);
            }
            else
            {
                encodeList.Add(encode);
            }
        }

        private static char ValueToHexChar(bool b1, bool b2, bool b3, bool b4)
        {
            int value = (b1 ? 8 : 0) + (b2 ? 4 : 0) + (b3 ? 2 : 0) + (b4 ? 1 : 0);
            if(value < 10)
            {
                return (char)('0' + value);
            }
            return (char)('a' + value - 10);
        }
    }
}
