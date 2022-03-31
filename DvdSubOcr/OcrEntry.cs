using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public class OcrEntry
    {
        HashSet<int> movieNameIds = new HashSet<int>();
        List<KeyValuePair<Point, string>> extraPieces = new List<KeyValuePair<Point, string>>();
        IList<KeyValuePair<Point, string>> extraPiecesReadOnly;
        List<BlockEncode> blocks;
        Rectangle rectBounds = Rectangle.Empty;

        public OcrEntry(string fullEncode, OcrCharacter ocr)
        {
            this.extraPiecesReadOnly = this.extraPieces.AsReadOnly();
            this.FullEncode = fullEncode;
            this.OcrCharacter = ocr;
        }

        public OcrEntry(string encode, OcrCharacter ocr, 
            IEnumerable<KeyValuePair<Point, string>> extraEntries)
            : this(encode, ocr)
        {
            this.extraPieces.AddRange(extraEntries);
        }

        public string FullEncode { get; private set; }
        public OcrCharacter OcrCharacter { get; private set; }
        public int ExtraPieceCount { get { return this.extraPieces.Count; } }
        public IList<KeyValuePair<Point, string>> ExtraPieces { get { return this.extraPiecesReadOnly; } }
        public ICollection<int> MovieIds { get { return this.movieNameIds; } }

        public bool IsBitPatternEqual(OcrEntry otherEntry)
        {
            if((this.FullEncode != otherEntry.FullEncode) ||
                (this.ExtraPieceCount != otherEntry.ExtraPieceCount))
            {
                return false;
            }
            for(int index = 0; index < this.extraPieces.Count; index++)
            {
                if((this.extraPieces[index].Key != otherEntry.extraPieces[index].Key) ||
                    (this.extraPieces[index].Value != otherEntry.extraPieces[index].Value))
                {
                    return false;
                }
            }
            return true;
        }

        private void BuildBlocks()
        {
            if(this.blocks == null)
            {
                blocks = new List<BlockEncode>();
                blocks.Add(new BlockEncode(Point.Empty, this.FullEncode, 0));
                foreach(KeyValuePair<Point, string> extraBlock in extraPieces)
                {
                    blocks.Add(new BlockEncode(extraBlock.Key, extraBlock.Value, 0));
                }
            }
        }

        public Rectangle CalculateBounds()
        {
            if(this.rectBounds.IsEmpty)
            {
                BuildBlocks();
                foreach(BlockEncode block in this.blocks)
                {
                    this.rectBounds = Rectangle.Union(this.rectBounds, 
                        new Rectangle(block.Origin, block.Size));
                }
            }
            return this.rectBounds;
        }

        public Bitmap CreateBlockBitmap(Color foreColor, Color backColor,
            int minimumWidth, int minimumHeight)
        {
            BuildBlocks();
            CalculateBounds();

            Bitmap bmp = new Bitmap(
                Math.Max(this.rectBounds.Width + 4, minimumWidth),
                Math.Max(this.rectBounds.Height + 4, minimumHeight));

            using(Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(backColor);
            }

            foreach(BlockEncode block in this.blocks)
            {
                IList<bool> decoded = block.DecodeToBoolArray();
                for(int y = 0; y < block.Height; y++)
                {
                    for(int x = 0; x < block.TrueWidth; x++)
                    {
                        if(decoded[y * block.Width + x])
                        {
                            bmp.SetPixel(
                                x - this.rectBounds.Left + block.Origin.X + 2,
                                y - this.rectBounds.Top + block.Origin.Y + 2, 
                                foreColor);
                        }
                    }
                }
            }
            return bmp;
        }
    }
}
