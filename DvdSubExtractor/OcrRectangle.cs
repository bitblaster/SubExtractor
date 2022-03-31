using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using DvdSubOcr;
using DvdNavigatorCrm;

namespace DvdSubExtractor
{
    public class OcrRectangle
    {
        Point subtitleOrigin;
        Rectangle sectionRectangle;
        List<BlocksAndPalette> palettes;
        int paletteIndex = 0;
        IList<int> allColorIndexes;
        IList<Color> allColors;
        HashSet<int> blocksUsedAsExtraPieces = new HashSet<int>();
        List<EncodeMatch> matches = new List<EncodeMatch>();
        List<BlockEncode> reorderedBlocks = new List<BlockEncode>();
        int movieId;
        OcrMap ocrMap;
        SubtitleText subText;
        bool isHighDef;

        public OcrRectangle(SubtitleBitmap subBitmap, Point subtitleOrigin, 
            OcrMap ocrMap, int movieId, Rectangle rect, IEnumerable<int> colorIndexes, bool isHighDef)
        {
            this.isHighDef = isHighDef;
            this.subtitleOrigin = subtitleOrigin;
            this.sectionRectangle = rect;
            this.allColorIndexes = new List<int>(colorIndexes);
            this.allColors = new List<Color>(subBitmap.RgbPalette);
            this.movieId = movieId;
            this.ocrMap = ocrMap;
            this.Matches = this.matches.AsReadOnly();
            FindPalettes(subBitmap);
        }

        public int PaletteCount { get { return this.palettes.Count; } }
        public IList<BlockEncode> Blocks 
        {
            get { return this.reorderedBlocks; }
            private set
            {
                this.reorderedBlocks.Clear();
                this.reorderedBlocks.AddRange(value);
                //this.reorderedBlocks.Sort((a, b) => a.HeightOrder.CompareTo(b.HeightOrder));
            }
        }
        public IList<string> SplitEncodes { get; private set; }
        public IList<EncodeMatch> Matches { get; private set; }
        public SubtitleText SubtitleText { get { return subText; } }
        public bool? IgnoreDoubleQuotes { get; set; }
        public string Comment { get; set; }

        public void NextPalette()
        {
            if((this.palettes != null) && (this.palettes.Count != 0))
            {
                this.paletteIndex = (this.paletteIndex + 1) % this.palettes.Count;
                this.Blocks = this.palettes[this.paletteIndex].Blocks;
                this.SplitEncodes = this.palettes[this.paletteIndex].SplitEncodes;
            }
        }

        public IList<KeyValuePair<int, Color>> GetChangedPalette()
        {
            if((this.palettes != null) && (this.palettes.Count != 0) && this.paletteIndex != 0)
            {
                return this.palettes[this.paletteIndex].ColorIndexes.Select(i => new KeyValuePair<int, Color>(i, this.allColors[i])).ToList();
            }
            return null;
        }

        public bool SelectTopPalette(IList<KeyValuePair<int, Color>> colors)
        {
            if((this.palettes != null) && (this.palettes.Count != 0))
            {
                for(int index = 0; index < this.palettes.Count; index++)
                {
                    BlocksAndPalette testPalette = this.palettes[index];
                    if(testPalette.ColorIndexes.Count == colors.Count)
                    {
                        bool isMatch = true;
                        for(int colorIndex = 0; colorIndex < colors.Count; colorIndex++)
                        {
                            int testIndex = testPalette.ColorIndexes[colorIndex];
                            if((testIndex != colors[colorIndex].Key) ||
                                (this.allColors[testIndex] != colors[colorIndex].Value))
                            {
                                isMatch = false;
                                break;
                            }
                        }
                        if(isMatch)
                        {
                            this.paletteIndex = index;
                            this.Blocks = this.palettes[this.paletteIndex].Blocks;
                            this.SplitEncodes = this.palettes[this.paletteIndex].SplitEncodes;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public string PaletteIndexListAsString
        {
            get
            {
                if((this.palettes != null) && (this.paletteIndex < this.palettes.Count))
                {
                    BlocksAndPalette pal = this.palettes[this.paletteIndex];
                    StringBuilder sb = new StringBuilder();
                    foreach(int index in pal.ColorIndexes)
                    {
                        if(sb.Length == 0)
                        {
                            sb.Append(index.ToString());
                        }
                        else
                        {
                            sb.AppendFormat(",{0}", index);
                        }
                    }
                    return sb.ToString();
                }
                return "";
            }
        }

        public int FindNextUnmatchedBlock(int currentBlockIndex)
        {
            for(int index = currentBlockIndex; index < this.Blocks.Count; index++)
            {
                if((this.matches[index] == null) &&
                    !this.blocksUsedAsExtraPieces.Contains(index))
                {
                    return index;
                }
            }
            return -1;
        }

        public bool IsBlockUsedAsExtraPiece(int index)
        {
            return this.blocksUsedAsExtraPieces.Contains(index);
        }

        public enum BestMatchResult
        {
            AllMatch = 0,
            BaselineError = 1,
            CharacterOrderError = 2,
            UnmatchedCharacter = 3,
        }

        static readonly char[] FirstPassSkippedChars = new char[] { ':', ';' };

        public BestMatchResult FindBestMatches(ISet<string> allowedBaselineErrors)
        {
            Dictionary<int, List<OcrEntry>> badMatches = new Dictionary<int, List<OcrEntry>>();
            List<KeyValuePair<int, EncodeMatch>> baselineErrors = new List<KeyValuePair<int, EncodeMatch>>();
            bool[] matchFound = new bool[this.Blocks.Count];
            do
            {
                this.matches.Clear();
                foreach(BlockEncode block in this.Blocks)
                {
                    this.matches.Add(null);
                }
                this.blocksUsedAsExtraPieces.Clear();

                bool firstTwoPartPass = true;
                for(int extraPieces = SubConstants.MaximumOcrPieces; extraPieces >= 0; extraPieces--)
                {
                    for(int blockIndex = 0; blockIndex < this.Blocks.Count; blockIndex++)
                    {
                        BlockEncode block = this.Blocks[blockIndex];
                        if((this.matches[blockIndex] == null) && !this.blocksUsedAsExtraPieces.Contains(blockIndex))
                        {
                            List<OcrEntry> badMatchList = null;
                            badMatches.TryGetValue(blockIndex, out badMatchList);
                            EncodeMatch match;
                            if((extraPieces == 1) && firstTwoPartPass)
                            {
                                match = FindBestMatch(block, extraPieces, this.blocksUsedAsExtraPieces, badMatchList, FirstPassSkippedChars);
                            }
                            else
                            {
                                match = FindBestMatch(block, extraPieces, this.blocksUsedAsExtraPieces, badMatchList, null);
                            }
                            if(match != null)
                            {
                                this.matches[blockIndex] = match;
                                foreach(int index in match.ExtraBlocks)
                                {
                                    this.blocksUsedAsExtraPieces.Add(index);
                                }
                            }
                        }
                    }

                    if((extraPieces == 1) && firstTwoPartPass)
                    {
                        firstTwoPartPass = false;
                        extraPieces++;
                    }
                }

                bool allHadMatches = true;
                for(int index = 0; index < this.matches.Count; index++)
                {
                    if(!matchFound[index])
                    {
                        if((this.matches[index] != null) || this.blocksUsedAsExtraPieces.Contains(index))
                        {
                            matchFound[index] = true;
                        }
                        else
                        {
                            allHadMatches = false;
                        }
                    }
                }

                this.subText = LineLayout.ConvertToLines(this.Blocks, this.matches, this.subtitleOrigin, !allHadMatches);

                baselineErrors.Clear();
                if(subText.Errors.Count != 0)
                {
                    foreach(KeyValuePair<int, EncodeMatch> badMatch in this.subText.Errors)
                    {
                        if((allowedBaselineErrors == null) || !allowedBaselineErrors.Contains(badMatch.Value.OcrEntry.FullEncode))
                        {
                            baselineErrors.Add(badMatch);

                            int blockIndex = badMatch.Key;
                            List<OcrEntry> badMatchList = null;
                            if(!badMatches.TryGetValue(blockIndex, out badMatchList))
                            {
                                badMatchList = new List<OcrEntry>();
                                badMatches[blockIndex] = badMatchList;
                            }
                            badMatchList.Add(badMatch.Value.OcrEntry);
                        }
                    }
                    if(baselineErrors.Count == 0)
                    {
                        // if we've decided to ignore all baseline errors, make sure the error characters are put 
                        // into the nearest line before returning
                        this.subText = LineLayout.ConvertToLines(this.Blocks, this.matches, this.subtitleOrigin, true);
                    }
                }
            }
            while(baselineErrors.Count != 0);

            bool allMatch = true;
            bool baselineIssue = false;
            for(int index = 0; index < this.matches.Count; index++)
            {
                EncodeMatch match = this.matches[index];
                if(match != null)
                {
                    this.ocrMap.AddMatch(match.OcrEntry, this.movieId, this.isHighDef);
                }
                else
                {
                    if(allMatch && !this.blocksUsedAsExtraPieces.Contains(index))
                    {
                        allMatch = false;
                        if(matchFound[index])
                        {
                            baselineIssue = true;
                        }
                    }
                }
            }

            if(allMatch)
            {
                return BestMatchResult.AllMatch;
            }
            else if(baselineIssue)
            {
                return BestMatchResult.BaselineError;
            }
            else
            {
                return BestMatchResult.UnmatchedCharacter;
            }
        }

        EncodeMatch FindBestMatch(BlockEncode block, int extraPieces,
            HashSet<int> unusableBlocks, IList<OcrEntry> badMatches, IList<char> skippedCharacters)
        {
            EncodeMatch bestMatch = null;
            List<int> otherBlockIndexes = new List<int>();
            foreach(OcrEntry entry in this.ocrMap.FindMatches(block.FullEncode, this.movieId, this.isHighDef, extraPieces))
            {
                if((badMatches != null) && (badMatches.Contains(entry)))
                {
                    continue;
                }

                if((skippedCharacters != null) && skippedCharacters.Contains(entry.OcrCharacter.Value))
                {
                    continue;
                }

                if(block.IsMatch(entry, this.Blocks, unusableBlocks, otherBlockIndexes, this.ocrMap, this.isHighDef))
                {
                    if(this.isHighDef)
                    {
                        bestMatch = new EncodeMatch(entry, otherBlockIndexes);
                        break;
                    }
                    else
                    {
                        if((bestMatch == null) || entry.MovieIds.Contains(this.movieId))
                        {
                            bestMatch = new EncodeMatch(entry, otherBlockIndexes);
                        }
                        else
                        {
                            char bestChar = bestMatch.OcrEntry.OcrCharacter.Value;
                            bool isBestLowercase = char.IsLower(bestChar) || (bestChar == '¡');
                            char entryChar = entry.OcrCharacter.Value;
                            bool isEntryLowercase = char.IsLower(entryChar) || (entryChar == '¡');
                            if((isEntryLowercase && !isBestLowercase) ||
                                ((isEntryLowercase == isBestLowercase) && (entry.MovieIds.Count > bestMatch.OcrEntry.MovieIds.Count)))
                            {
                                bestMatch = new EncodeMatch(entry, otherBlockIndexes);
                            }
                        }
                    }
                }
            }
            return bestMatch;
        }

        void FindPalettes(SubtitleBitmap subBitmap)
        {
            IntPtr rectangleData;
            unsafe
            {
                byte* data = (byte*)subBitmap.Data.ToPointer() +
                    this.sectionRectangle.X + (subBitmap.Stride * this.sectionRectangle.Y);
                rectangleData = new IntPtr(data);
            }

            ContiguousEncode encode = new ContiguousEncode(rectangleData,
                this.sectionRectangle.Width, this.sectionRectangle.Height,
                subBitmap.Stride, ocrMap);

            this.palettes = new List<BlocksAndPalette>(
                encode.FindAllEncodesAndPalettes(this.allColorIndexes));
            if(this.palettes.Count != 0)
            {
                foreach(BlocksAndPalette pal in this.palettes)
                {
                    HashSet<OcrCharacter> matches = new HashSet<OcrCharacter>();
                    foreach(BlockEncode block in pal.Blocks)
                    {
                        foreach(OcrEntry entry in ocrMap.FindMatches(block.FullEncode, movieId, this.isHighDef, 0))
                        {
                            char val = entry.OcrCharacter.Value;
                            if(!SubConstants.CheapMatchesDuringOcrCharacters.Contains(val) && 
                                (Char.IsLetterOrDigit(val) || SubConstants.LikelyValidForOcrSymbols.Contains(val)))
                            {
                                if(!matches.Contains(entry.OcrCharacter))
                                {
                                    pal.InterestingMatches++;
                                    pal.InterestingMatchBlocks.Add(block);
                                    matches.Add(entry.OcrCharacter);
                                }
                                break;
                            }
                        }
                    }
                }

                // check if one of the palettes contains a big contiguous blob that contains all the parts of another palette
                // if it does, it contains an outlining color that we want to mark as lower in the sort of likely palettes using WrapAround
                foreach(BlocksAndPalette pal in this.palettes)
                {
                    // only looking at single colors in this block
                    if(pal.ColorIndexes.Count != 1)
                    {
                        continue;
                    }

                    int maxSize = 0;
                    Rectangle rectBiggest = Rectangle.Empty;
                    foreach(BlockEncode blockOuter in pal.Blocks)
                    {
                        int blockSize = blockOuter.TrueWidth * blockOuter.Height;
                        if(blockSize > maxSize)
                        {
                            maxSize = blockSize;
                            rectBiggest = new Rectangle(blockOuter.Origin, blockOuter.Size);
                        }
                    }
                    rectBiggest.Inflate(-1, -1);

                    foreach(BlocksAndPalette palInner in this.palettes)
                    {
                        if(object.ReferenceEquals(pal, palInner) || (pal.CountedBlocks >= palInner.CountedBlocks) ||
                            pal.WrapsAround.Contains(palInner) || palInner.WrapsAround.Contains(pal))
                        {
                            continue;
                        }

                        // check if each block of a palette wraps around 1 or more blocks of another palette, 
                        // without them sharing a color. If so this is an outline color palette and should be marked so
                        bool allWrapped = true;
                        foreach(BlockEncode blockInner in palInner.Blocks)
                        {
                            Rectangle rectInner = new Rectangle(blockInner.Origin, blockInner.Size);
                            if(!rectBiggest.Contains(rectInner))
                            {
                                bool foundWrapper = false;
                                foreach(BlockEncode blockOuter in pal.Blocks)
                                {
                                    Rectangle rectOuter = new Rectangle(blockOuter.Origin, blockOuter.Size);
                                    Rectangle rectWrap = Rectangle.Inflate(rectOuter, 2, 0);
                                    if(rectWrap.Contains(rectInner) && !Rectangle.Equals(rectOuter, rectInner))
                                    {
                                        foundWrapper = true;
                                        break;
                                    }
                                }
                                if(!foundWrapper)
                                {
                                    allWrapped = false;
                                    break;
                                }
                            }
                        }
                        if(allWrapped)
                        {
                            pal.WrapsAround.Add(palInner);
                        }
                    }
                }

                foreach(BlocksAndPalette pal in this.palettes)
                {
                    // only looking at single colors in this block
                    if(pal.ColorIndexes.Count == 1)
                    {
                        continue;
                    }

                    foreach(BlocksAndPalette palInner in this.palettes)
                    {
                        if(object.ReferenceEquals(pal, palInner) || (palInner.ColorIndexes.Count != 1) || 
                            (palInner.WrapsAround.Count == 0) || palInner.WrapsAround.Contains(pal))
                        {
                            continue;
                        }

                        if(pal.ColorIndexes.Contains(palInner.ColorIndexes[0]))
                        {
                            foreach(BlocksAndPalette palWrap in palInner.WrapsAround)
                            {
                                pal.WrapsAround.Add(palWrap);
                            }
                        }
                    }
                }

                foreach(BlocksAndPalette pal in this.palettes)
                {
                    // only looking at single colors in this block
                    int maxSize = 0;
                    Rectangle rectBiggest = Rectangle.Empty;
                    foreach(BlockEncode blockOuter in pal.Blocks)
                    {
                        int blockSize = blockOuter.TrueWidth * blockOuter.Height;
                        if(blockSize > maxSize)
                        {
                            maxSize = blockSize;
                            rectBiggest = new Rectangle(blockOuter.Origin, blockOuter.Size);
                        }
                    }
                    rectBiggest.Inflate(-1, -1);

                    foreach(BlocksAndPalette palInner in this.palettes)
                    {
                        if(object.ReferenceEquals(pal, palInner) ||
                            pal.WrapsAround.Contains(palInner) || palInner.WrapsAround.Contains(pal) ||
                            pal.WrapsAroundGently.Contains(palInner) || palInner.WrapsAroundGently.Contains(pal))
                        {
                            continue;
                        }

                        // check if each block of a palette wraps around 1 or more blocks of another palette, 
                        // without them sharing a color. If so this is an outline color palette and should be marked so
                        bool allWrapped = true;
                        foreach(BlockEncode blockInner in palInner.Blocks)
                        {
                            Rectangle rectInner = new Rectangle(blockInner.Origin, blockInner.Size);
                            if(!rectBiggest.Contains(rectInner))
                            {
                                bool foundWrapper = false;
                                foreach(BlockEncode blockOuter in pal.Blocks)
                                {
                                    Rectangle rectOuter = new Rectangle(blockOuter.Origin, blockOuter.Size);
                                    //rectOuter.Inflate(-1, -1);
                                    if(rectOuter.Contains(rectInner) && !Rectangle.Equals(rectOuter, rectInner))
                                    {
                                        foundWrapper = true;
                                        break;
                                    }
                                }
                                if(!foundWrapper)
                                {
                                    allWrapped = false;
                                    break;
                                }
                            }
                        }
                        if(allWrapped)
                        {
                            pal.WrapsAroundGently.Add(palInner);

                            bool sharesColors = true;
                            foreach(int colIndex in palInner.ColorIndexes)
                            {
                                if(!pal.ColorIndexes.Contains(colIndex))
                                {
                                    sharesColors = false;
                                    break;
                                }
                            }
                            if(sharesColors)
                            {
                                bool sharesBlocks = false;
                                foreach(BlockEncode blockInner in palInner.InterestingMatchBlocks)
                                {
                                    foreach(BlockEncode blockOuter in pal.InterestingMatchBlocks)
                                    {
                                        if((blockInner.Origin == blockOuter.Origin) && (blockInner.FullEncode == blockOuter.FullEncode))
                                        {
                                            sharesBlocks = true;
                                            break;
                                        }
                                    }
                                    if(sharesBlocks)
                                    {
                                        break;
                                    }
                                }
                                if(!sharesBlocks)
                                {
                                    pal.WrapsAndShares.Add(palInner);
                                }
                            }
                        }
                    }
                }

                this.palettes.Sort(ComparePalettes);
                this.paletteIndex = 0;
                this.Blocks = this.palettes[0].Blocks;
                this.SplitEncodes = this.palettes[0].SplitEncodes;
            }
        }

        static int NormalPixelCount = SubConstants.NormalPixelCount;
        static int MinimumNormalPixelCount = SubConstants.MinimumNormalPixelCount;
        static int MaximumNormalPixelCount = SubConstants.MaximumNormalPixelCount;
        static int MinimumLowPixelCount = SubConstants.MinimumLowPixelCount;

        public static void AdjustForVideoSize(Size videoSize)
        {
            if(videoSize.Height > 1000)
            {
                NormalPixelCount = SubConstants.NormalPixelCount * 2;
                MinimumNormalPixelCount = SubConstants.MinimumNormalPixelCount * 2;
                MaximumNormalPixelCount = SubConstants.MaximumNormalPixelCount * 2;
                MinimumLowPixelCount = SubConstants.MinimumLowPixelCount * 2;
            }
            else
            {
                NormalPixelCount = SubConstants.NormalPixelCount;
                MinimumNormalPixelCount = SubConstants.MinimumNormalPixelCount;
                MaximumNormalPixelCount = SubConstants.MaximumNormalPixelCount;
                MinimumLowPixelCount = SubConstants.MinimumLowPixelCount;
            }
        }

        static int ComparePalettes(BlocksAndPalette p1, BlocksAndPalette p2)
        {
            if(p1.InterestingMatches != p2.InterestingMatches)
            {
                if(p2.InterestingMatches == 0)
                {
                    return -1;
                }
                if(p1.InterestingMatches == 0)
                {
                    return 1;
                }
            }

            if(p1.WrapsAround.Contains(p2))
            {
                return 1;
            }
            if(p2.WrapsAround.Contains(p1))
            {
                return -1;
            }

            if((p1.InterestingMatches > 0) && p1.WrapsAndShares.Contains(p2) && (p1.InterestingMatches >= p2.InterestingMatches))
            {
                return -1;
            }
            if((p2.InterestingMatches > 0) && p2.WrapsAndShares.Contains(p1) && (p2.InterestingMatches >= p1.InterestingMatches))
            {
                return 1;
            }

            if(p1.WrapsAroundGently.Contains(p2) && (p2.AveragePixelCount >= MinimumLowPixelCount) && (p1.InterestingMatches <= p2.InterestingMatches))
            {
                return 1;
            }
            if(p2.WrapsAroundGently.Contains(p1) && (p1.AveragePixelCount >= MinimumLowPixelCount) && (p2.InterestingMatches <= p1.InterestingMatches))
            {
                return -1;
            }

            if(p1.InterestingMatches > p2.InterestingMatches * 2)
            {
                return -1;
            }
            if(p2.InterestingMatches > p1.InterestingMatches * 2)
            {
                return 1;
            }

            if(p1.CountedBlocks == p2.CountedBlocks)
            {
                if(p1.AveragePixelCount == p2.AveragePixelCount)
                {
                    return 0;
                }

                if(Math.Abs(p1.AveragePixelCount - NormalPixelCount) <
                    Math.Abs(p2.AveragePixelCount - NormalPixelCount))
                {
                    return -1;
                }
                return 1;
            }

            if(p1.CountedBlocks < p2.CountedBlocks)
            {
                return -ComparePalettes(p2, p1);
            }

            if(p1.AveragePixelCount >= MinimumNormalPixelCount)
            {
                return -1;
            }

            if((p2.AveragePixelCount >= MinimumNormalPixelCount) &&
                (p2.AveragePixelCount <= MaximumNormalPixelCount))
            {
                return 1;
            }

            if(p1.AveragePixelCount >= MinimumLowPixelCount)
            {
                return -1;
            }

            if(p2.AveragePixelCount >= MinimumLowPixelCount)
            {
                return 1;
            }

            if(p1.AveragePixelCount >= p2.AveragePixelCount)
            {
                return -1;
            }
            return 1;
        }
    }
}
