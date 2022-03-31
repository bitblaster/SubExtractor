using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DvdSubOcr;

namespace DvdSubOcr
{
    class BlockReducer
    {
        BlockEncode encode;
        Dictionary<string, int> reducedBlocks;

        const int Div2Size1 = 10;
        //const int Div2SizeWidth = 12;
        const int Div2Size2 = 20;
        //const int Div2SizeHeight = 15;
        //const int Div2Height = 8;

        public BlockReducer(BlockEncode encode)
        {
            this.encode = encode;
            this.reducedBlocks = new Dictionary<string, int>();
            this.ReducedEncodes = this.reducedBlocks;
            this.FullEncode = this.encode.FullEncode;
            
            if(((encode.TrueWidth <= Div2Size1) && (encode.Height <= Div2Size2)) || 
                ((encode.Height <= Div2Size1) && (encode.TrueWidth <= Div2Size2)))
            {
                ReduceBy2();
            }
            else
            {
                ReduceBy3();
            }
        }

        public BlockReducer(string fullEncode, IEnumerable<KeyValuePair<string, int>> reducedEncodes)
        {
            this.FullEncode = fullEncode;
            this.ReducedEncodes = new List<KeyValuePair<string, int>>(reducedEncodes).AsReadOnly();
        }

        public string FullEncode { get; private set; }
        public IEnumerable<KeyValuePair<string, int>> ReducedEncodes { get; private set; }

        const int MinCountFor2Reduc = 3;

        void ReduceBy2()
        {
            const int border = 2 - (MinCountFor2Reduc + 1) / 2;
            int width = this.encode.Width + 2 * border;
            int height = this.encode.Height + 2 * border;
            IList<bool> orig = this.encode.DecodeToBoolArray(border);

            AddReduc2(orig, width, height, 0, 0);
            AddReduc2(orig, width, height, 1, 0);
            AddReduc2(orig, width, height, 0, 1);
            AddReduc2(orig, width, height, 1, 1);
        }

        void AddReduc2(IList<bool> orig, int width, int height, int widthOffset, int heightOffset)
        {
            int reducWidth = (width - widthOffset) / 2 + 3;
            int reducHeight = (height - heightOffset) / 2;
            bool[] reduc = new bool[reducWidth * reducHeight];
            int reducOffset = 0;
            int yMin = reducHeight;
            int yMax = -1;
            int xMin = reducWidth;
            int xMax = -1;
            for(int y = heightOffset; y < height - 1; y += 2)
            {
                int offset = y * width;
                for(int x = widthOffset; x < width - 1; x += 2)
                {
                    if((orig[offset + x] ? 1 : 0) + (orig[offset + x + 1] ? 1 : 0)
                        + (orig[offset + width + x] ? 1 : 0) + (orig[offset + width + x + 1] ? 1 : 0)
                        >= MinCountFor2Reduc)
                    {
                        reduc[reducOffset++] = true;
                        yMin = Math.Min(yMin, y / 2);
                        yMax = Math.Max(yMax, y / 2);
                        xMin = Math.Min(xMin, x / 2);
                        xMax = Math.Max(xMax, x / 2);
                    }
                    else
                    {
                        reduc[reducOffset++] = false;
                    }
                }
                reducOffset += 3;
            }
            if(xMax >= xMin)
            {
                string encode = CreateEncode(reduc, reducWidth, yMin, yMax, xMin, xMax);
                int count;
                this.reducedBlocks.TryGetValue(encode, out count);
                this.reducedBlocks[encode] = count + 1;
            }
        }

        const int MinCountFor3Reduc = 6;

        void ReduceBy3()
        {
            const int border = 3 - (MinCountFor3Reduc + 2) / 3;
            int width = this.encode.Width + 2 * border;
            int height = this.encode.Height + 2 * border;
            IList<bool> orig = this.encode.DecodeToBoolArray(border);

            AddReduc3(orig, width, height, 0, 0);
            AddReduc3(orig, width, height, 1, 0);
            AddReduc3(orig, width, height, 2, 0);
            AddReduc3(orig, width, height, 0, 1);
            AddReduc3(orig, width, height, 1, 1);
            AddReduc3(orig, width, height, 2, 1);
            AddReduc3(orig, width, height, 0, 2);
            AddReduc3(orig, width, height, 1, 2);
            AddReduc3(orig, width, height, 2, 2);
        }

        void AddReduc3(IList<bool> orig, int width, int height, int widthOffset, int heightOffset)
        {
            int reducWidth = (width - widthOffset) / 3 + 3;
            int reducHeight = (height - heightOffset) / 3;
            bool[] reduc = new bool[reducWidth * reducHeight];
            int reducOffset = 0;
            int yMin = reducHeight;
            int yMax = -1;
            int xMin = reducWidth;
            int xMax = -1;
            for(int y = heightOffset; y < height - 2; y += 3)
            {
                int offset = y * width;
                int offset2 = offset + width;
                int offset3 = offset2 + width;
                for(int x = widthOffset; x < width - 2; x += 3)
                {
                    if((orig[offset + x] ? 1 : 0) + (orig[offset + x + 1] ? 1 : 0) + (orig[offset + x + 2] ? 1 : 0)
                        + (orig[offset2 + x] ? 1 : 0) + (orig[offset2 + x + 1] ? 1 : 0) + (orig[offset2 + x + 2] ? 1 : 0)
                        + (orig[offset3 + x] ? 1 : 0) + (orig[offset3 + x + 1] ? 1 : 0) + (orig[offset3 + x + 2] ? 1 : 0)
                        >= MinCountFor3Reduc)
                    {
                        reduc[reducOffset++] = true;
                        yMin = Math.Min(yMin, y / 3);
                        yMax = Math.Max(yMax, y / 3);
                        xMin = Math.Min(xMin, x / 3);
                        xMax = Math.Max(xMax, x / 3);
                    }
                    else
                    {
                        reduc[reducOffset++] = false;
                    }
                }
                reducOffset += 3;
            }
            if(xMax >= xMin)
            {
                string encode = CreateEncode(reduc, reducWidth, yMin, yMax, xMin, xMax);
                int count;
                this.reducedBlocks.TryGetValue(encode, out count);
                this.reducedBlocks[encode] = count + 1;
            }
        }

        string CreateEncode(bool[] reduc, int reducWidth, int yMin, int yMax, int xMin, int xMax)
        {
            int encodeWidth = (xMax - xMin) / 4 * 4 + 4;
            StringBuilder sb = new StringBuilder(encodeWidth.ToString("d3"));
            for(int y = yMin; y <= yMax; y++)
            {
                int ptOffset = y * reducWidth + xMin;
                int ptOffsetEnd = ptOffset + xMax - xMin + 1;
                while(ptOffset < ptOffsetEnd)
                {
                    sb.Append(ValueToHexChar(reduc[ptOffset], reduc[ptOffset + 1], reduc[ptOffset + 2], reduc[ptOffset + 3]));
                    ptOffset += 4;
                }
            }
            return sb.ToString();
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
