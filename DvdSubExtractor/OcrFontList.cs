using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DvdSubOcr;

namespace DvdSubExtractor
{
    public class OcrFontList : IDisposable
    {
        List<OcrFont> normalFonts = new List<OcrFont>();
        List<OcrFont> italicFonts = new List<OcrFont>();
        //List<OcrFont> smallLineFonts = new List<OcrFont>();
        //List<OcrFont> smallLineItalicFonts = new List<OcrFont>();
        class LineAndFonts
        {
            public SubtitleLine Line { get; set; }
            public OcrFont NormalFont { get; set; }
            public OcrFont ItalicFont { get; set; }
        }
        Dictionary<SubtitleLine, LineAndFonts> linesWithFonts = new Dictionary<SubtitleLine, LineAndFonts>();
        OcrFont genericNormalFont;
        OcrFont genericItalicFont;
        List<Font> disposableFonts = new List<Font>();

        public OcrFontList()
        {
            this.NormalFonts = this.normalFonts.AsReadOnly();
            this.ItalicFonts = this.italicFonts.AsReadOnly();
            //this.SmallLineFonts = this.smallLineFonts.AsReadOnly();
            //this.SmallLineItalicFonts = this.smallLineItalicFonts.AsReadOnly();
        }

        public IList<OcrFont> NormalFonts { get; private set; }
        //public IList<OcrFont> SmallLineFonts { get; private set; }
        public IList<OcrFont> ItalicFonts { get; private set; }
        //public IList<OcrFont> SmallLineItalicFonts { get; private set; }

        public IEnumerable<OcrFont> AllFonts
        {
            get
            {
                foreach(OcrFont font in NormalFonts)
                {
                    yield return font;
                }
                foreach(OcrFont font in ItalicFonts)
                {
                    yield return font;
                }
                /*foreach(OcrFont font in SmallLineFonts)
                {
                    yield return font;
                }
                foreach(OcrFont font in SmallLineItalicFonts)
                {
                    yield return font;
                }*/
                if(this.genericNormalFont != null)
                {
                    yield return this.genericNormalFont;
                }
                if(this.genericItalicFont != null)
                {
                    yield return this.genericItalicFont;
                }
            }
        }

        public void AddRange(IEnumerable<SubtitleLine> lineList)
        {
            foreach(SubtitleLine line in lineList)
            {
                Add(line);
            }
        }

        public void Add(SubtitleLine line)
        {
            OcrFont fontNormal = new OcrFont(false, line);
            if(fontNormal.ComparableCharacterCount > 0)
            //if(fontNormal.ComparableCharacterCount > 1)
            {
                normalFonts.Add(fontNormal);
            }
            /*else
            {
                if(fontNormal.TotalCharacterCount > 0)
                {
                    smallLineFonts.Add(fontNormal);
                }
            }*/

            OcrFont fontItalic = new OcrFont(true, line);
            if(fontItalic.ComparableCharacterCount > 0)
            //if(fontItalic.ComparableCharacterCount > 1)
            {
                italicFonts.Add(fontItalic);
            }
            /*else
            {
                if(fontItalic.TotalCharacterCount > 0)
                {
                    smallLineItalicFonts.Add(fontItalic);
                }
            }*/
        }

        public void MergeSimilarFonts()
        {
            bool moreMerge;
            this.normalFonts.Sort();
            do
            {
                moreMerge = false;

                for(int index = 0; index < this.normalFonts.Count - 1; index++)
                {
                    OcrFont font1 = this.normalFonts[index];
                    for(int index2 = index + 1; index2 < this.normalFonts.Count; index2++)
                    {
                        OcrFont font2 = this.normalFonts[index2];
                        if(font1.IsMatchingFont(font2) == OcrFont.Matching.Yes)
                        {
                            font1.MergeFonts(font2);
                            this.normalFonts.RemoveAt(index2);
                            moreMerge = true;
                            index2--;
                        }
                    }
                    if(moreMerge)
                    {
                        break;
                    }
                }
            } while(moreMerge);

            this.italicFonts.Sort();
            do
            {
                moreMerge = false;

                for(int index = 0; index < this.italicFonts.Count - 1; index++)
                {
                    OcrFont font1 = this.italicFonts[index];
                    for(int index2 = index + 1; index2 < this.italicFonts.Count; index2++)
                    {
                        OcrFont font2 = this.italicFonts[index2];
                        if(font1.IsMatchingFont(font2) == OcrFont.Matching.Yes)
                        {
                            font1.MergeFonts(font2);
                            this.italicFonts.RemoveAt(index2);
                            moreMerge = true;
                            index2--;
                        }
                    }
                    if(moreMerge)
                    {
                        break;
                    }
                }
            } while(moreMerge);
        }

        public void BuildFontLineMap()
        {
            this.linesWithFonts.Clear();

            Dictionary<SubtitleLine, OcrFont> normalFontMap = new Dictionary<SubtitleLine, OcrFont>();
            foreach(OcrFont font in NormalFonts)
            {
                font.UpdateFontKerning();
                foreach(SubtitleLine line in font.Lines)
                {
                    normalFontMap[line] = font;
                }
            }
            /*foreach(OcrFont font in SmallLineFonts)
            {
                font.UpdateFontKerning();
                foreach(SubtitleLine line in font.Lines)
                {
                    normalFontMap[line] = font;
                }
            }*/

            foreach(OcrFont font in ItalicFonts)
            {
                font.UpdateFontKerning();
                foreach(SubtitleLine line in font.Lines)
                {
                    OcrFont normal;
                    if(normalFontMap.TryGetValue(line, out normal))
                    {
                        this.linesWithFonts.Add(line, new LineAndFonts()
                        {
                            Line = line,
                            NormalFont = normal,
                            ItalicFont = font
                        });
                        normalFontMap.Remove(line);
                    }
                    else
                    {
                        this.linesWithFonts.Add(line, new LineAndFonts()
                        {
                            Line = line,
                            NormalFont = null,
                            ItalicFont = font
                        });
                    }
                }
            }
            /*foreach(OcrFont font in SmallLineItalicFonts)
            {
                font.UpdateFontKerning();
                foreach(SubtitleLine line in font.Lines)
                {
                    OcrFont normal;
                    if(normalFontMap.TryGetValue(line, out normal))
                    {
                        this.linesWithFonts.Add(line, new LineAndFonts()
                        {
                            Line = line,
                            NormalFont = normal,
                            ItalicFont = font
                        });
                        normalFontMap.Remove(line);
                    }
                    else
                    {
                        this.linesWithFonts.Add(line, new LineAndFonts()
                        {
                            Line = line,
                            NormalFont = null,
                            ItalicFont = font
                        });
                    }
                }
            }*/

            foreach(KeyValuePair<SubtitleLine, OcrFont> pair in normalFontMap)
            {
                this.linesWithFonts.Add(pair.Key, new LineAndFonts()
                {
                    Line = pair.Key,
                    NormalFont = pair.Value,
                    ItalicFont = null
                });
            }
        }

        public void AddSpacesToAllLines()
        {
            FontKerning emptyNormalKerning = new FontKerning(false);
            FontKerning emptyItalicKerning = new FontKerning(true);
            foreach(LineAndFonts lineAndFonts in this.linesWithFonts.Values)
            {
                FontKerning normal = (lineAndFonts.NormalFont != null) ? 
                    lineAndFonts.NormalFont.Kerning : emptyNormalKerning;
                FontKerning italic = (lineAndFonts.ItalicFont != null) ? 
                    lineAndFonts.ItalicFont.Kerning : emptyItalicKerning;
                lineAndFonts.Line.InsertSpaces(normal, italic);
            }
        }

        void MatchFont(OcrFont ocrFont, IList<Font> fonts, float windowsToDvdFontHeightConversion)
        {
            string text;
            float tallest;
            KeyValuePair<OcrCharacter, float> tallestPair = ocrFont.TallestComparableCharacter;
            if(tallestPair.Key == null)
            {
                tallestPair = ocrFont.TallestCharacter;
            }
            text = new string(tallestPair.Key.Value, 1);
            tallest = tallestPair.Value;

            float minimumHeightDifference = float.MaxValue;
            Font closestFont = null;
            foreach(Font font in fonts)
            {
                SizeF sizeText = TextRenderer.MeasureText(text, font);
                float adjustedHeight = sizeText.Height * windowsToDvdFontHeightConversion;
                float heightDifference = Math.Abs(adjustedHeight - tallest);
                if(heightDifference < minimumHeightDifference)
                {
                    minimumHeightDifference = heightDifference;
                    closestFont = font;
                }
            }
            if(closestFont != null)
            {
                ocrFont.MatchingRealFont = closestFont;
            }
        }

        public void MatchToWindowsFonts(IList<Font> normalFonts, IList<Font> italicFonts,
            Font genericNormalFont, Font genericItalicFont, float windowsToDvdFontHeightConversion)
        {
            this.disposableFonts.ForEach(f => f.Dispose());
            this.disposableFonts.Clear();

            this.disposableFonts.AddRange(normalFonts);
            this.disposableFonts.AddRange(italicFonts);
            this.disposableFonts.Add(genericNormalFont);
            this.disposableFonts.Add(genericItalicFont);

            this.genericNormalFont = new OcrFont(false);
            this.genericNormalFont.MatchingRealFont = genericNormalFont;
            this.genericItalicFont = new OcrFont(true);
            this.genericItalicFont.MatchingRealFont = genericItalicFont;

            foreach(OcrFont font in this.NormalFonts)
            {
                MatchFont(font, normalFonts, windowsToDvdFontHeightConversion);
            }
            /*foreach(OcrFont font in this.SmallLineFonts)
            {
                MatchFont(font, g, normalFonts);
            }*/
            foreach(OcrFont font in this.ItalicFonts)
            {
                MatchFont(font, italicFonts, windowsToDvdFontHeightConversion);
            }
            /*foreach(OcrFont font in this.SmallLineItalicFonts)
            {
                MatchFont(font, g, italicFonts);
            }*/
        }

        public OcrFont FindDominantFontForLine(SubtitleLine line)
        {
            LineAndFonts landf;
            if(this.linesWithFonts.TryGetValue(line, out landf))
            {
                if(line.DominantFont == SubtitleLine.DominantFontStyle.Normal)
                {
                    return landf.NormalFont ?? this.genericNormalFont;
                }
                else
                {
                    return landf.ItalicFont ?? this.genericItalicFont;
                }
            }
            return this.genericNormalFont;
        }

        public KeyValuePair<OcrFont, OcrFont> FindFontsForLine(SubtitleLine line)
        {
            LineAndFonts landf;
            if(this.linesWithFonts.TryGetValue(line, out landf))
            {
                return new KeyValuePair<OcrFont, OcrFont>(
                    landf.NormalFont ?? this.genericNormalFont,
                    landf.ItalicFont ?? this.genericItalicFont);
            }
            return new KeyValuePair<OcrFont, OcrFont>(null, null);
        }

        void Dispose(bool isManagedDispose)
        {
            this.disposableFonts.ForEach(f => f.Dispose());
            this.disposableFonts.Clear();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        ~OcrFontList()
        {
            Dispose(false);
        }
    }
}
