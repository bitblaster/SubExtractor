using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using DvdSubOcr;

namespace DvdSubExtractor
{
    public class OcrFont : IComparable<OcrFont>
    {
        const float AllowableVariation = 1.125f;
        const float MinimumAgreementCharacters = .90f;

        SortedDictionary<OcrCharacter, SizeF> textSizes = new SortedDictionary<OcrCharacter, SizeF>();
        SortedDictionary<OcrCharacter, int> textWeights = new SortedDictionary<OcrCharacter, int>();
        List<SubtitleLine> lines = new List<SubtitleLine>();

        public OcrFont(bool isItalic) : this(isItalic, null) { }

        public OcrFont(bool isItalic, SubtitleLine line)
        {
            this.IsItalic = isItalic;
            this.Kerning = new FontKerning(isItalic);
            this.Lines = this.lines.AsReadOnly();

            if(line != null)
            {
                Dictionary<OcrCharacter, IList<SizeF>> characterSizes = new Dictionary<OcrCharacter, IList<SizeF>>();
                for(int index = 0; index < line.Text.Count; index++)
                {
                    OcrCharacter ocr = line.Text[index];
                    if(ocr.Italic == this.IsItalic)
                    {
                        this.TotalCharacterCount++;
                        if(!SubConstants.UselessForFontMatchingCharacters.Contains(ocr.Value))
                        {
                            IList<SizeF> sizes;
                            if(!characterSizes.TryGetValue(ocr, out sizes))
                            {
                                sizes = new List<SizeF>();
                                characterSizes[ocr] = sizes;
                            }
                            sizes.Add(line.TextBounds[index].Size);
                        }
                    }
                }

                foreach(KeyValuePair<OcrCharacter, IList<SizeF>> pair in characterSizes)
                {
                    SizeF minSize = new SizeF(pair.Value.Min(size => size.Width), pair.Value.Min(size => size.Height));
                    SizeF maxSize = new SizeF(pair.Value.Max(size => size.Width), pair.Value.Max(size => size.Height));
                    if(IsCloseEnoughMatch(minSize, maxSize))
                    {
                        this.textSizes[pair.Key] = new SizeF(
                            pair.Value.Average(size => size.Width), pair.Value.Average(size => size.Height));
                        this.textWeights[pair.Key] = pair.Value.Count;
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("Character {0} has a variable size in this line", pair.Key.Value));
                    }
                }
                this.lines.Add(line);
            }
        }

        public int TotalCharacterCount { get; private set; }
        public int ComparableCharacterCount { get { return this.textSizes.Count; } }
        public string ComparableCharacters
        {
            get
            {
                return this.textSizes.Aggregate(new StringBuilder(), (sb, pair) => sb.Append(pair.Key.Value)).ToString();
            }
        }
        public KeyValuePair<OcrCharacter, float> TallestComparableCharacter
        {
            get
            {
                float maxHeight = 0.0f;
                OcrCharacter maxChar = null;
                foreach(KeyValuePair<OcrCharacter, SizeF> entry in this.textSizes)
                {
                    if(entry.Value.Height > maxHeight)
                    {
                        maxHeight = entry.Value.Height;
                        maxChar = entry.Key;
                    }
                }
                return new KeyValuePair<OcrCharacter, float>(maxChar, maxHeight);
            }
        }
        public KeyValuePair<OcrCharacter, float> TallestCharacter
        {
            get
            {
                float maxHeight = 0.0f;
                OcrCharacter maxChar = null;
                foreach(SubtitleLine line in this.lines)
                {
                    for(int index = 0; index < line.Text.Count; index++)
                    {
                        if(line.TextBounds[index].Height > maxHeight)
                        {
                            maxHeight = line.TextBounds[index].Height;
                            maxChar = line.Text[index];
                        }
                    }
                }
                return new KeyValuePair<OcrCharacter, float>(maxChar, maxHeight);
            }
        }
        public bool IsItalic { get; private set; }
        public IList<SubtitleLine> Lines { get; private set; }
        public FontKerning Kerning { get; private set; }
        public Font MatchingRealFont { get; set; }

        public int CompareTo(OcrFont other)
        {
            return -this.ComparableCharacterCount.CompareTo(other.ComparableCharacterCount);
        }

        public void UpdateFontKerning()
        {
            this.Kerning.ClearWeights();
            foreach(SubtitleLine line in this.Lines)
            {
                this.Kerning.AddTextWeights(line.Text, line.TextBounds);
            }
        }

        public enum Matching
        {
            Undetermined = 0,
            Yes = 1,
            No = 2,
        }

        public Matching IsMatchingFont(OcrFont other)
        {
            if(this.IsItalic != other.IsItalic)
            {
                throw new InvalidOperationException("Cannot compare Italic and non-Italic fonts");
            }

            int matches = 0;
            int failures = 0;
            foreach(KeyValuePair<OcrCharacter, SizeF> pair in this.textSizes)
            {
                SizeF otherSize;
                if(other.textSizes.TryGetValue(pair.Key, out otherSize))
                {
                    if(IsCloseEnoughMatch(pair.Value, otherSize))
                    {
                        matches++;
                    }
                    else
                    {
                        failures++;
                    }
                }
            }

            if(matches != 0)
            {
                if(((float)matches / ((float)(matches + failures)) >= MinimumAgreementCharacters))
                {
                    return Matching.Yes;
                }
                return Matching.No;
            }
            return Matching.Undetermined;
        }

        public void MergeFonts(OcrFont other)
        {
            if(this.IsItalic != other.IsItalic)
            {
                throw new InvalidOperationException("Cannot merge Italic and non-Italic fonts");
            }

            SortedDictionary<OcrCharacter, SizeF> newTextSizes = new SortedDictionary<OcrCharacter, SizeF>();
            SortedDictionary<OcrCharacter, int> newTextWeights = new SortedDictionary<OcrCharacter, int>();

            foreach(KeyValuePair<OcrCharacter, SizeF> pair in this.textSizes)
            {
                SizeF otherSize;
                if(other.textSizes.TryGetValue(pair.Key, out otherSize))
                {
                    int thisWeight = this.textWeights[pair.Key];
                    int otherWeight = other.textWeights[pair.Key];

                    float avgWidth = (pair.Value.Width * thisWeight + otherSize.Width * otherWeight) /
                        (thisWeight + otherWeight);
                    float avgHeight = (pair.Value.Height * thisWeight + otherSize.Height * otherWeight) /
                        (thisWeight + otherWeight);

                    newTextSizes.Add(pair.Key, new SizeF(avgWidth, avgHeight));
                    newTextWeights.Add(pair.Key, thisWeight + otherWeight);
                }
                else
                {
                    newTextSizes.Add(pair.Key, pair.Value);
                    newTextWeights.Add(pair.Key, this.textWeights[pair.Key]);
                }
            }
            
            foreach(KeyValuePair<OcrCharacter, SizeF> pair in other.textSizes)
            {
                if(!this.textSizes.ContainsKey(pair.Key))
                {
                    newTextSizes.Add(pair.Key, pair.Value);
                    newTextWeights.Add(pair.Key, other.textWeights[pair.Key]);
                }
            }

            this.textSizes = newTextSizes;
            this.textWeights = newTextWeights;
            this.lines.AddRange(other.lines);
            this.TotalCharacterCount += other.TotalCharacterCount;
        }

        static bool IsCloseEnoughMatch(SizeF original, SizeF test)
        {
            SizeF sizeMax = new SizeF(Math.Max(original.Width, test.Width),
                Math.Max(original.Height, test.Height));
            SizeF sizeMin = new SizeF(Math.Min(original.Width, test.Width),
                Math.Min(original.Height, test.Height));

            if(!sizeMin.IsEmpty)
            {
                return (sizeMax.Width / sizeMin.Width < AllowableVariation) &&
                    (sizeMax.Height / sizeMin.Height < AllowableVariation);
            }
            return false;
        }
    }
}
