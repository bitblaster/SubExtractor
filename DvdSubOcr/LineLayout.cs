using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    static public class LineLayout
    {
        static Dictionary<char, float> baseLines = new Dictionary<char, float>();
        static Dictionary<char, float> topLines = new Dictionary<char, float>();
        static Dictionary<char, int> baselineSlush = new Dictionary<char, int>();
        static HashSet<char> trustedForHeight = new HashSet<char>();
        static Dictionary<char, int> diacritics = new Dictionary<char, int>();
        static HashSet<char> untrustedForBaseline = InitializeDictionaries();

        static HashSet<char> InitializeDictionaries()
        {
            HashSet<char> untrusted = new HashSet<char>();

            foreach(char c in SubConstants.DiacriticCharacters)
            {
                if(SubConstants.DiacriticLowCharacters.Contains(c))
                {
                    diacritics.Add(c, -1);
                }
                else if(SubConstants.DiacriticHighCharacters.Contains(c))
                {
                    diacritics.Add(c, 1);
                }
                else
                {
                    diacritics.Add(c, 0);
                }
            }

            foreach(string characterString in CharacterSelector.AllCharacters)
            {
                foreach(char c in characterString)
                {
                    float calcBase, calcTop;
                    FindCharacterBounds(c, out calcBase, out calcTop);
                    //Debug.WriteLine(string.Format("{0}: Base {1:f2} Calc {2:f2} Top {3:f2} Calc {4:f2}",
                    //    c, baseLines[c], calcBase, topLines[c], calcTop));

                    baseLines[c] = calcBase;
                    topLines[c] = calcTop;
                    baselineSlush[c] = Char.IsLetterOrDigit(c) ? 6 : 3;
                }
            }

            /*
            foreach(string characterString in CharacterSelector.AllCharacters)
            {
                foreach(char c in characterString)
                {
                    baseLines[c] = 0.0f;
                    topLines[c] = (Char.IsLetter(c) && Char.IsLower(c)) ? .6f : 1.0f;
                    baselineSlush[c] = Char.IsLetterOrDigit(c) ? 6 : 3;
                }
            }
            foreach(char c in SubConstants.TallLowerCaseCharacters)
            {
                topLines[c] = 1.0f;
            }
            topLines['i'] = .8f;
            topLines['j'] = .8f;
            foreach(char c in SubConstants.DescendingCharacters)
            {
                baseLines[c] = -.3f;
            }
            foreach(char c in SubConstants.AccentedCharacters)
            {
                topLines[c] = topLines[c] + .2f;
            }

            topLines['¡'] = .8f;
            topLines['~'] = .7f;
            topLines['-'] = .7f;
            topLines['—'] = .7f;
            topLines['_'] = .3f;
            topLines['='] = .8f;
            topLines[','] = .1f;
            topLines['.'] = .1f;
            topLines[':'] = .8f;
            topLines[';'] = .8f;
            topLines['¿'] = .8f;
            topLines['œ'] = .6f;
            topLines['æ'] = .6f;
            topLines['ß'] = 1.1f;
            topLines['ø'] = .6f;
            topLines['„'] = .1f;

            baseLines['^'] = .6f;
            baseLines['°'] = .6f;
            baseLines['~'] = .4f;
            baseLines['-'] = .4f;
            baseLines['—'] = .4f;
            baseLines['='] = .4f;
            baseLines['*'] = .5f;
            baseLines[','] = -.1f;
            baseLines['.'] = .0f;
            baseLines[':'] = .2f;
            baseLines['\''] = .7f;
            baseLines['\"'] = .7f;
            baseLines['ß'] = -.1f;
            baseLines['J'] = -.1f;
            baseLines['Q'] = -.1f;
            baseLines['$'] = -.05f;
            baseLines['„'] = -.05f;
             */

            //baseLines['า'] = -.2f;
            //topLines['า'] = 1.0f;
            //baseLines['ำ'] = -.2f;
            //topLines['ำ'] = 1.0f;
            baseLines['ั'] = 1.1f;
            topLines['ั'] = 1.3f;
            baseLines['ิ'] = 1.1f;
            topLines['ิ'] = 1.3f;
            baseLines['ี'] = 1.1f;
            topLines['ี'] = 1.3f;
            baseLines['ึ'] = 1.1f;
            topLines['ึ'] = 1.3f;
            baseLines['ื'] = 1.1f;
            topLines['ื'] = 1.3f;
            baseLines['ํ'] = 1.1f;
            topLines['ํ'] = 1.3f;
            baseLines['็'] = 1.2f;
            topLines['็'] = 1.5f;
            baseLines['่'] = 1.2f;
            topLines['่'] = 1.5f;
            baseLines['้'] = 1.2f;
            topLines['้'] = 1.5f;
            baseLines['๊'] = 1.2f;
            topLines['๊'] = 1.5f;
            baseLines['๋'] = 1.2f;
            topLines['๋'] = 1.5f;
            baseLines['์'] = 1.2f;
            topLines['์'] = 1.5f;
            baseLines['๎'] = 1.2f;
            topLines['๎'] = 1.5f;

            baseLines['ุ'] = -.3f;
            topLines['ุ'] = 0.0f;
            baseLines['ู'] = -.3f;
            topLines['ู'] = 0.0f;
            baseLines['ฺ'] = -.3f;
            topLines['ฺ'] = 0.0f;

            baselineSlush['็'] = 12;
            baselineSlush['่'] = 12;
            baselineSlush['้'] = 12;
            baselineSlush['๊'] = 12;
            baselineSlush['๋'] = 12;
            baselineSlush['์'] = 12;
            baselineSlush['๎'] = 12;

            baselineSlush['Y'] = 8;
            baselineSlush['Ý'] = 8;
            baselineSlush['H'] = 8;
            baselineSlush['y'] = 8;
            baselineSlush['ý'] = 8;
            baselineSlush['\''] = 6;
            baselineSlush['\"'] = 6;
            baselineSlush[':'] = 5;
            baselineSlush[';'] = 5;
            baselineSlush['ß'] = 5;
            baselineSlush['Ω'] = 5;
            baselineSlush['Σ'] = 5;
            baselineSlush['='] = 5;
            baselineSlush['~'] = 8;
            baselineSlush['^'] = 5;
            baselineSlush['°'] = 5;
            baselineSlush['*'] = 5;
            baselineSlush['-'] = 8;
            baselineSlush['—'] = 8;
            baselineSlush['_'] = 8;
            baselineSlush['♪'] = 10;
            baselineSlush['['] = 8;
            baselineSlush[']'] = 8;
            baselineSlush['('] = 8;
            baselineSlush[')'] = 8;
            baselineSlush['{'] = 8;
            baselineSlush['}'] = 8;
            baselineSlush['\\'] = 8;
            baselineSlush['/'] = 8;
            baselineSlush['|'] = 8;
            baselineSlush['>'] = 8;
            baselineSlush['<'] = 8;
            baselineSlush['€'] = 10;
            baselineSlush['£'] = 10;
            baselineSlush['¥'] = 10;
            baselineSlush['$'] = 10;
            baselineSlush['§'] = 10;
            baselineSlush['!'] = 5;
            baselineSlush['¡'] = 6;
            baselineSlush['„'] = 6;
            baselineSlush['"'] = 8;

            foreach(char c in SubConstants.TrustedForHeightCharacters)
            {
                if(!baseLines.ContainsKey(c))
                {
                    float calcBase, calcTop;
                    FindCharacterBounds(c, out calcBase, out calcTop);

                    baseLines[c] = calcBase;
                    topLines[c] = calcTop;
                }
                if(!baselineSlush.ContainsKey(c))
                {
                    baselineSlush[c] = 6;
                }

                trustedForHeight.Add(c);
            }
            foreach(char c in SubConstants.UntrustedForBaselineCharacters)
            {
                untrusted.Add(c);
            }
            return untrusted;
        }

        public static bool IsDiacritic(Char c)
        {
            return diacritics.ContainsKey(c);
        }

        class CharacterInLine
        {
            public CharacterInLine(BlockEncode block, EncodeMatch match, Rectangle bounds)
            {
                Block = block;
                Match = match;
                Bounds = bounds;
            }

            public BlockEncode Block { get; private set; }
            public EncodeMatch Match { get; private set; }
            public Rectangle Bounds { get; private set; }
        }

        static void AddWeight(SortedList<int, int> list, int key, int value)
        {
            int count;
            if(list.TryGetValue(key, out count))
            {
                list[key] = count + value;
            }
            else
            {
                list[key] = value;
            }
        }

        static void FindCharacterBounds(Char c, out float baseLineOffset, out float topLineOffset)
        {
            const int TestWidth = 80;
            const int TestHeight = 40;
            using(Bitmap bmp = new Bitmap(TestWidth, TestHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
            {
                baseLineOffset = 0.0f;
                topLineOffset = (Char.IsLetter(c) && Char.IsLower(c)) ? .6f : 1.0f;

                using(Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Black);
                    using(Font font = new Font("Arial", 18.0f))
                    {
                        string testString = "E " + new String(c, 1);
                        StringFormat format = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Center;
                        g.DrawString(testString, font, Brushes.White, TestWidth / 2, TestHeight / 2, format);
                    }
                }
                BitmapData bdata = bmp.LockBits(new Rectangle(0, 0, TestWidth, TestHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                int stride = bdata.Stride;
                unsafe
                {
                    byte* pdata = (byte *)bdata.Scan0.ToPointer();
                    byte* middleLine = pdata + stride * TestHeight / 2;
                    byte* leftEdgeE = middleLine;
                    int leftEdge = 0;
                    while(*leftEdgeE < 128)
                    {
                        leftEdgeE += 3;
                        leftEdge++;
                    }
                    byte* topEdgeE = leftEdgeE - stride;
                    int topEdge = TestHeight / 2;
                    while(*topEdgeE >= 128)
                    {
                        topEdgeE -= stride;
                        topEdge++;
                    }
                    byte* bottomEdgeE = leftEdgeE + stride;
                    int bottomEdge = TestHeight / 2 - 1;
                    while(*bottomEdgeE >= 128)
                    {
                        bottomEdgeE += stride;
                        bottomEdge--;
                    }
                    byte* rightEdgeE = topEdgeE + 3 + stride;
                    int rightEdge = leftEdge + 1;
                    while(*rightEdgeE >= 128)
                    {
                        rightEdgeE += 3;
                        rightEdge++;
                    }

                    int originLeft = rightEdge + 2;
                    byte* remainOrigin = pdata + originLeft * 3;
                    int remainWidth = TestWidth - originLeft;
                    int remainTopEdge = TestHeight;
                    byte* remainLine = remainOrigin;
                    bool foundData = false;
                    for(int line = 0; line < TestHeight; line++)
                    {
                        byte* remainPos = remainLine;
                        for(int x = 0; x < remainWidth; x++)
                        {
                            if(*remainPos >= 128)
                            {
                                foundData = true;
                                break;
                            }
                            remainPos += 3;
                        }

                        if(foundData)
                        {
                            break;
                        }
                        remainTopEdge--;
                        remainLine += stride;
                    }

                    if(foundData)
                    {
                        int remainBottomEdge = 0;
                        remainLine = remainOrigin + (TestHeight - 1) * stride;
                        foundData = false;
                        for(int line = 0; line < TestHeight; line++)
                        {
                            byte* remainPos = remainLine;
                            for(int x = 0; x < remainWidth; x++)
                            {
                                if(*remainPos >= 128)
                                {
                                    foundData = true;
                                    break;
                                }
                                remainPos += 3;
                            }

                            if(foundData)
                            {
                                break;
                            }
                            remainBottomEdge++;
                            remainLine -= stride;
                        }

                        if(foundData)
                        {
                            float heightE = Convert.ToSingle(topEdge - bottomEdge);
                            baseLineOffset = Convert.ToSingle(remainBottomEdge - bottomEdge) / heightE;
                            topLineOffset = Convert.ToSingle(remainTopEdge - bottomEdge) / heightE;
                        }
                    }
                }

                bmp.UnlockBits(bdata);
            }
        }

        static int CalculateBaseline(Rectangle bounds, Char c, float averageHeight)
        {
            float baseLineOffset;
            float topLineOffset;
            if(!baseLines.TryGetValue(c, out baseLineOffset))
            {
                FindCharacterBounds(c, out baseLineOffset, out topLineOffset);
                baseLines[c] = baseLineOffset;
                topLines[c] = topLineOffset;
                baselineSlush[c] = 10;
            }
            topLineOffset = topLines[c];

            if(!float.IsNaN(averageHeight) && !trustedForHeight.Contains(c))
            {
                float middleLine = (baseLineOffset + topLineOffset) / 2;
                float middleChar = (bounds.Top + bounds.Bottom) / 2.0f;

                int estBaseline = Convert.ToInt32(middleChar + middleLine * averageHeight);
                return estBaseline;
            }

            float fractionOfChar = topLineOffset - baseLineOffset;
            float heightCalibration = bounds.Height / fractionOfChar;

            int baseline = bounds.Bottom +
                Convert.ToInt32(baseLineOffset * heightCalibration);
            return baseline;
        }

        static public SubtitleText ConvertToLines(IList<BlockEncode> blocks,
            IList<EncodeMatch> matches, Point subtitleOrigin, bool useNearestBaseline)
        {
            List<CharacterInLine> characterBounds = new List<CharacterInLine>();
            float heightTotal = 0.0f;
            int heightCount = 0;
            int maximumHeight = 0;
            for(int index = 0; index < blocks.Count; index++)
            {
                BlockEncode block = blocks[index];
                EncodeMatch match = matches[index];
                if((match != null) && (match.OcrEntry.OcrCharacter != OcrCharacter.Unmatched))
                {
                    Rectangle bounds = match.OcrEntry.CalculateBounds();
                    maximumHeight = Math.Max(maximumHeight, bounds.Height);
                    bounds.Offset(block.Origin + new Size(subtitleOrigin));
                    characterBounds.Add(new CharacterInLine(block, match, bounds));

                    char c = match.OcrEntry.OcrCharacter.Value;
                    if(trustedForHeight.Contains(c))
                    {
                        heightTotal += bounds.Height / (topLines[c] - baseLines[c]);
                        heightCount++;
                    }
                }
            }

            float averageHeight = float.NaN;
            if(heightCount != 0)
            {
                averageHeight = heightTotal / heightCount;
            }

            SortedList<int, int> baselineCandidates = new SortedList<int, int>();
            foreach(CharacterInLine charLine in characterBounds)
            {
                char c = charLine.Match.OcrEntry.OcrCharacter.Value;
                if(!untrustedForBaseline.Contains(c))
                {
                    int baseline = CalculateBaseline(charLine.Bounds, c, averageHeight);

                    AddWeight(baselineCandidates, baseline - 3, 1);
                    AddWeight(baselineCandidates, baseline - 2, 2);
                    AddWeight(baselineCandidates, baseline - 1, 3);
                    AddWeight(baselineCandidates, baseline, 4);
                    AddWeight(baselineCandidates, baseline + 1, 3);
                    AddWeight(baselineCandidates, baseline + 2, 2);
                    AddWeight(baselineCandidates, baseline + 3, 1);
                }
            }

            int lastIndex = -100;
            int lastMax = -1;
            List<int> peaks = new List<int>();
            foreach(KeyValuePair<int, int> pair in baselineCandidates)
            {
                if(pair.Key != lastIndex + 1)
                {
                    peaks.Add(pair.Key);
                    lastMax = pair.Value;
                }
                else if(pair.Value > lastMax)
                {
                    peaks[peaks.Count - 1] = pair.Key;
                    lastMax = pair.Value;
                }
                lastIndex = pair.Key;
                //Debug.WriteLine(string.Format("Line {0} Count {1}", pair.Key, pair.Value));
            }

            /*foreach(int yOffset in peaks)
            {
                Debug.WriteLine(string.Format("Line {0} Peak", yOffset));
            }*/

            bool hasPeaks = (peaks.Count != 0);
            if(!hasPeaks)
            {
                peaks.Add(0);
            }
            List<OcrCharacter>[] lineText = new List<OcrCharacter>[peaks.Count];
            List<Rectangle>[] lineTextBounds = new List<Rectangle>[peaks.Count];
            List<int>[] lineBlockIndexes = new List<int>[peaks.Count];
            List<EncodeMatch>[] lineEncodes = new List<EncodeMatch>[peaks.Count];
            for(int index = 0; index < peaks.Count; index++)
            {
                lineText[index] = new List<OcrCharacter>();
                lineTextBounds[index] = new List<Rectangle>();
                lineBlockIndexes[index] = new List<int>();
                lineEncodes[index] = new List<EncodeMatch>();
            }
            Rectangle[] lineBounds = new Rectangle[peaks.Count];

            List<KeyValuePair<int, EncodeMatch>> errorMatches = new List<KeyValuePair<int, EncodeMatch>>();
            for(int index = 0; index < blocks.Count; index++)
            {
                BlockEncode block = blocks[index];
                EncodeMatch match = matches[index];
                if((match != null) && (match.OcrEntry.OcrCharacter != OcrCharacter.Unmatched))
                {
                    char c = match.OcrEntry.OcrCharacter.Value;
                    Rectangle bounds = match.OcrEntry.CalculateBounds();
                    bounds.Offset(block.Origin + new Size(subtitleOrigin));
                    if(hasPeaks)
                    {
                        int baseline = CalculateBaseline(bounds, c, averageHeight);
                        int slush;
                        if(!baselineSlush.TryGetValue(c, out slush))
                        {
                            slush = 10;
                        }
                        if(maximumHeight >= SubConstants.NormalFontHeight)
                        {
                            slush = Convert.ToInt32(slush * 
                                Convert.ToSingle(maximumHeight) / SubConstants.NormalFontHeight);
                        }

                        int peakIndex = 0;
                        int foundPeakIndex = -1;
                        int nearestPeak = -1;
                        foreach(int peak in peaks)
                        {
                            int nearness = Math.Abs(peak - baseline);
                            if(nearness <= slush)
                            {
                                foundPeakIndex = peakIndex;
                                break;
                            }
                            if(useNearestBaseline && ((nearestPeak == -1) || (nearness < nearestPeak)))
                            {
                                nearestPeak = nearness;
                                foundPeakIndex = peakIndex;
                            }
                            peakIndex++;
                        }

                        if(foundPeakIndex != -1)
                        {
                            lineText[foundPeakIndex].Add(match.OcrEntry.OcrCharacter);
                            lineTextBounds[foundPeakIndex].Add(bounds);
                            lineBlockIndexes[foundPeakIndex].Add(index);
                            lineEncodes[foundPeakIndex].Add(match);
                            if(lineBounds[foundPeakIndex].IsEmpty)
                            {
                                lineBounds[foundPeakIndex] = bounds;
                            }
                            else
                            {
                                lineBounds[foundPeakIndex] = Rectangle.Union(
                                    lineBounds[foundPeakIndex], bounds);
                            }
                        }
                        else
                        {
                            errorMatches.Add(new KeyValuePair<int, EncodeMatch>(index, match));
                        }
                    }
                    else
                    {
                        lineText[0].Add(match.OcrEntry.OcrCharacter);
                        lineTextBounds[0].Add(bounds);
                        lineBlockIndexes[0].Add(index);
                        lineEncodes[0].Add(match);
                        if(lineBounds[0].IsEmpty)
                        {
                            lineBounds[0] = bounds;
                        }
                        else
                        {
                            lineBounds[0] = Rectangle.Union(lineBounds[0], bounds);
                        }
                    }
                }
            }

            if(!useNearestBaseline)
            {
                TestCharacterOrdering(lineText, lineTextBounds, lineBlockIndexes, lineEncodes, errorMatches);
            }
            ReorderLineContainingTones(lineText, lineTextBounds);

            List<SubtitleLine> lines = new List<SubtitleLine>();
            for(int index = 0; index < peaks.Count; index++)
            {
                int totalColors = 0;
                foreach(int blockIndex in lineBlockIndexes[index])
                {
                    totalColors |= blocks[blockIndex].ColorBitFlags;
                }
                SubtitleLine line = new SubtitleLine(lineText[index],
                    lineTextBounds[index], lineBlockIndexes[index], lineBounds[index], totalColors);
                line.CorrectObviousErrorsInLine();
                lines.Add(line);
            }

            return new SubtitleText(lines, errorMatches);
        }

        static bool IsThaiCharacter(char c)
        {
            return ((c >= '\x0e01') && (c < '\x0e60'));
        }

        static void TestCharacterOrdering(List<OcrCharacter>[] lineText, 
            List<Rectangle>[] lineTextBounds, List<int>[] lineBlockIndexes,
            List<EncodeMatch>[] lineEncodes, List<KeyValuePair<int, EncodeMatch>> errorMatches)
        {
            for(int lineIndex = 0; lineIndex < lineText.Length; lineIndex++)
            {
                List<OcrCharacter> line = lineText[lineIndex];
                bool wasThai = false;
                for(int charIndex = 0; charIndex < line.Count; charIndex++)
                {
                    bool isBad = false;
                    OcrCharacter ocr = line[charIndex];
                    switch(ocr.Value)
                    {
                    case '่':
                        if(((charIndex == 0) || !wasThai) &&
                           ((charIndex >= line.Count - 1) || !IsThaiCharacter(line[charIndex + 1].Value)))
                        {
                            isBad = true;
                            wasThai = false;
                        }
                        break;
                    case '\'':
                        if(((charIndex != 0) && wasThai ) ||
                           ((charIndex < line.Count - 1) && IsThaiCharacter(line[charIndex + 1].Value)))
                        {
                            isBad = true;
                            wasThai = true;
                        }
                        break;
                    }

                    if(isBad)
                    {
                        int blockIndex = lineBlockIndexes[lineIndex][charIndex];
                        EncodeMatch match = lineEncodes[lineIndex][charIndex];
                        errorMatches.Add(new KeyValuePair<int,EncodeMatch>(blockIndex, match));
                        lineText[lineIndex].RemoveAt(charIndex);
                        lineTextBounds[lineIndex].RemoveAt(charIndex);
                        lineBlockIndexes[lineIndex].RemoveAt(charIndex);
                        lineEncodes[lineIndex].RemoveAt(charIndex);
                        return;
                    }
                    else
                    {
                        wasThai = IsThaiCharacter(ocr.Value);
                    }
                }
            }
        }

        static void ReorderLineContainingTones(List<OcrCharacter>[] lineText, List<Rectangle>[] lineTextBounds)
        {
            for(int lineIndex = 0; lineIndex < lineText.Length; lineIndex++)
            {
                List<OcrCharacter> line = lineText[lineIndex];
                List<Rectangle> lineBounds = lineTextBounds[lineIndex];

                bool hasDiacrits = false;
                // first re-order accents that were placed before the characters they affect - accents come after characters
                for(int index = 0; index < line.Count - 1; index++)
                {
                    OcrCharacter ocr = line[index];
                    if(diacritics.ContainsKey(ocr.Value))
                    {
                        hasDiacrits = true;
                        OcrCharacter ocrNext = line[index + 1];
                        if(!diacritics.ContainsKey(ocrNext.Value))
                        {
                            Rectangle rect = lineBounds[index];
                            Rectangle rectNext = lineBounds[index + 1];
                            int midDiacritic = rect.Left + rect.Width / 2;
                            if(midDiacritic > rectNext.Left)
                            {
                                if(!ocrNext.Italic || SubConstants.DiacriticLowCharacters.Contains(ocr.Value))
                                {
                                    line[index] = ocrNext;
                                    line[index + 1] = ocr;
                                    lineBounds[index] = rectNext;
                                    lineBounds[index + 1] = rect;
                                }
                                else
                                {
                                    Console.WriteLine("italic high accent");
                                }
                            }
                        }
                    }
                }

                if(!hasDiacrits)
                {
                    continue;
                }

                // make sure the italic-ness of accents matches that of characters
                bool isItalic = line[0].Italic;
                for(int index = 1; index < line.Count; index++)
                {
                    OcrCharacter ocr = line[index];
                    if(diacritics.ContainsKey(ocr.Value))
                    {
                        if(ocr.Italic != isItalic)
                        {
                            line[index] = new OcrCharacter(ocr.Value, isItalic);
                        }
                    }
                    else
                    {
                        isItalic = ocr.Italic;
                    }
                }

                // order the accents
                int lastNonAccent = -1;
                for(int index = 0; index < line.Count - 1; index++)
                {
                    OcrCharacter ocr = line[index];
                    int ocrLevel;
                    if(diacritics.TryGetValue(ocr.Value, out ocrLevel))
                    {
                        OcrCharacter ocrNext = line[index + 1];
                        int ocrNextLevel;
                        if(diacritics.TryGetValue(ocrNext.Value, out ocrNextLevel))
                        {
                            if(ocrLevel > ocrNextLevel)
                            {
                                Rectangle rect = lineBounds[index];
                                Rectangle rectNext = lineBounds[index + 1];
                                line[index] = ocrNext;
                                line[index + 1] = ocr;
                                lineBounds[index] = rectNext;
                                lineBounds[index + 1] = rect;
                                index = lastNonAccent;  // essentially do a bubble sort on (up to 3) accents for a character
                            }
                        }
                    }
                    else
                    {
                        lastNonAccent = index;
                    }
                }

                for(int index = 0; index < line.Count - 1; index++)
                {
                    OcrCharacter ocr = line[index];
                    if(ocr.Value == 'ํ')
                    {
                        int diacritCount = 1;
                        while((index + diacritCount < line.Count) && diacritics.ContainsKey(line[index + diacritCount].Value))
                        {
                            diacritCount++;
                        }

                        if((index + diacritCount < line.Count) && (line[index + diacritCount].Value == 'า'))
                        {
                            OcrCharacter ocrCombo = line[index + diacritCount];
                            line[index + diacritCount] = new OcrCharacter('ำ', ocrCombo.Italic);
                            line.RemoveAt(index);
                            lineBounds.RemoveAt(index);
                        }
                    }
                    else if(ocr.Value == 'เ')
                    {
                        OcrCharacter ocrNext = line[index + 1];
                        if(ocrNext.Value == 'เ')
                        {
                            Rectangle ocrRect = lineBounds[index];
                            Rectangle ocrNextRect = lineBounds[index + 1];
                            line[index] = new OcrCharacter('แ', ocr.Italic);
                            lineBounds[index] = new Rectangle(ocrRect.X, ocrRect.Y, ocrNextRect.Right - ocrRect.Left, ocrRect.Height);
                            line.RemoveAt(index + 1);
                            lineBounds.RemoveAt(index + 1);
                        }
                    }
                }

                // reverse position of tone marks after Sara Am
                for(int index = 0; index < line.Count - 1; index++)
                {
                    OcrCharacter ocr = line[index];
                    if(ocr.Value == 'ำ')
                    {
                        OcrCharacter ocrNext = line[index + 1];
                        if(SubConstants.DiacriticHighCharacters.Contains(ocrNext.Value))
                        {
                            Rectangle ocrRect = lineBounds[index];
                            Rectangle ocrNextRect = lineBounds[index + 1];
                            line[index] = ocrNext;
                            line[index + 1] = ocr;
                            lineBounds[index] = ocrNextRect;
                            lineBounds[index + 1] = ocrRect;
                        }
                    }
                }
            }
        }
    }
}
