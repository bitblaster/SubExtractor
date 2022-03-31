using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public class FontKerning
    {
        static Dictionary<char, int> leftKerning;  // pixels letter bleeds to the left
        static Dictionary<char, int> rightKerning; // pixels letter bleeds to the right
        static Dictionary<char, int> leftItalicKerning;  // pixels letter bleeds to the left
        static Dictionary<char, int> rightItalicKerning; // pixels letter bleeds to the right
        static Dictionary<char, int> defaultLeftKerning;  
        static Dictionary<char, int> defaultRightKerning; 
        static Dictionary<char, int> defaultLeftItalicKerning;  
        static Dictionary<char, int> defaultRightItalicKerning; 
        static object initializer = BuildKerningMaps();

        SortedList<int, int> separations = new SortedList<int, int>();
        List<int> peaks = new List<int>();
        IList<int> peaksReadOnly;
        bool peaksUpToDate = true;

        public static IDictionary<char, int> LeftKerning { get { return leftKerning; } }
        public static IDictionary<char, int> RightKerning { get { return rightKerning; } }
        public static IDictionary<char, int> LeftItalicKerning { get { return leftItalicKerning; } }
        public static IDictionary<char, int> RightItalicKerning { get { return rightItalicKerning; } }

        public static int GetDefaultLeftKerning(bool italic, char c)
        {
            int value;
            if(italic)
            {
                defaultLeftItalicKerning.TryGetValue(c, out value);
            }
            else
            {
                defaultLeftKerning.TryGetValue(c, out value);
            }
            return value;
        }

        public static int GetDefaultRightKerning(bool italic, char c)
        {
            int value;
            if(italic)
            {
                defaultRightItalicKerning.TryGetValue(c, out value);
            }
            else
            {
                defaultRightKerning.TryGetValue(c, out value);
            }
            return value;
        }

        public static void RestoreDefaultKernings()
        {
            leftKerning = new Dictionary<char, int>(defaultLeftKerning);
            rightKerning = new Dictionary<char, int>(defaultRightKerning);
            leftItalicKerning = new Dictionary<char, int>(defaultLeftItalicKerning);
            rightItalicKerning = new Dictionary<char, int>(defaultRightItalicKerning);
        }

        static void BuildDiffs(StringBuilder sb, IDictionary<char, int> defaults, IDictionary<char, int> current)
        {
            sb.Append('{');
            foreach(var item in current)
            {
                int defaultValue;
                if(!defaults.TryGetValue(item.Key, out defaultValue) || (item.Value != defaultValue))
                {
                    sb.AppendFormat("({0},{1})", item.Key, item.Value);
                }
            }
            sb.Append('}');
        }

        static int ReadDiffs(String s, int index, IDictionary<char, int> current)
        {
            if(s[index++] != '{')
            {
                throw new ArgumentException();
            }
            while(s[index] != '}')
            {
                if(s[index++] != '(')
                {
                    throw new ArgumentException();
                }
                char c = s[index++];
                if(s[index++] != ',')
                {
                    throw new ArgumentException();
                }
                int closeIndex = s.IndexOf(')', index);
                int countLength = closeIndex - index;
                int count = Int32.Parse(s.Substring(index, countLength));
                index += countLength + 1;
                current[c] = count;
            }
            return index + 1;
        }

        public static string KerningDiffList
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                BuildDiffs(sb, defaultLeftKerning, leftKerning);
                BuildDiffs(sb, defaultRightKerning, rightKerning);
                BuildDiffs(sb, defaultLeftItalicKerning, leftItalicKerning);
                BuildDiffs(sb, defaultRightItalicKerning, rightItalicKerning);
                if(sb.Length <= 8)
                {
                    return "";
                }
                return sb.ToString();
            }
            set
            {
                RestoreDefaultKernings();
                if(!string.IsNullOrWhiteSpace(value))
                {
                    int index = ReadDiffs(value, 0, leftKerning);
                    index = ReadDiffs(value, index, rightKerning);
                    index = ReadDiffs(value, index, leftItalicKerning);
                    index = ReadDiffs(value, index, rightItalicKerning);
                }
            }
        }

        public FontKerning(bool isItalic)
        {
            this.IsItalic = isItalic;
            this.Separations = this.separations;
            this.peaksReadOnly = this.peaks.AsReadOnly();
        }

        public void ClearWeights()
        {
            this.separations.Clear();
        }

        public IDictionary<int, int> Separations { get; private set; }
        public bool IsItalic { get; private set; }
        
        public IList<int> Peaks
        {
            get
            {
                UpdatePeaks();
                return this.peaksReadOnly;
            }
        }

        public void AddTextWeights(IList<OcrCharacter> text, IList<Rectangle> textBounds)
        {
            if(text.Count == 0)
            {
                return;
            }

            this.peaksUpToDate = false;

            Rectangle leftRect = Rectangle.Empty;
            OcrCharacter leftText = null;
            for(int index = 0; index < text.Count; index++)
            {
                OcrCharacter rightText = text[index];
                if(LineLayout.IsDiacritic(rightText.Value) || (textBounds[index].Width == 0))
                {
                    continue;
                }

                if(leftText == null)
                {
                    leftText = text[index];
                    leftRect = textBounds[index];
                    continue;
                }

                Rectangle rightRect = textBounds[index];

                if((this.IsItalic == leftText.Italic) && (leftText.Italic == rightText.Italic) && (rightText.Value != 'ำ'))
                {
                    int diff = rightRect.Left - leftRect.Right;

                    if((diff < -20) || (diff > 20))
                    {
                        //Debug.WriteLine("Weird diff");
                    }

                    diff += FindKerning(leftText, rightText);

                    //AddWeight(dict, diff - 2, 1);
                    AddWeight(this.separations, diff - 1, 1);
                    AddWeight(this.separations, diff, 3);
                    AddWeight(this.separations, diff + 1, 1);
                    //AddWeight(dict, diff + 2, 1);
                }

                leftText = rightText;
                leftRect = rightRect;
            }
        }

        void UpdatePeaks()
        {
            if(this.peaksUpToDate || (this.separations.Count == 0))
            {
                return;
            }

            this.peaksUpToDate = true;
            this.peaks.Clear();
            int peakIndex = -1000;
            int peakValue = 0;
            foreach(KeyValuePair<int, int> pair in this.separations)
            {
                if(pair.Value >= peakValue)
                {
                    peakIndex = pair.Key;
                    peakValue = pair.Value;
                }
            }
            this.peaks.Add(peakIndex);

            int originalPeakIndex = peakIndex;
            int valleyIndex;
            int valleyValue;
            int lastIndex;
            int lastValue;
            bool foundValley;
            while(true)
            {
                valleyIndex = peakIndex + 1;
                valleyValue = peakValue;
                lastIndex = peakIndex;
                lastValue = -1;
                foundValley = false;
                foreach(KeyValuePair<int, int> pair in this.separations)
                {
                    if(pair.Key > peakIndex)
                    {
                        if(pair.Key != lastIndex + 1)
                        {
                            valleyIndex = pair.Key - 1;
                            valleyValue = 0;
                            foundValley = true;
                            break;
                        }
                        if((pair.Value >= lastValue) && (lastValue * 2 < peakValue))
                        {
                            valleyIndex = lastIndex;
                            valleyValue = lastValue;
                            foundValley = true;
                            break;
                        }
                    }
                    lastIndex = pair.Key;
                    lastValue = pair.Value;
                }

                if(!foundValley)
                {
                    break;
                }

                int peak2Index = valleyIndex + 1;
                int peak2Value = 0;
                foreach(KeyValuePair<int, int> pair in this.separations)
                {
                    if((pair.Key > valleyIndex) && (pair.Value >= peak2Value))
                    {
                        peak2Index = pair.Key;
                        peak2Value = pair.Value;
                    }
                }

                if((peak2Value >= valleyValue * 5 / 4) && (peak2Value >= valleyValue + 3))
                {
                    this.peaks.Add(peak2Index);
                    return;
                }
                else
                {
                    peakIndex = peak2Index;
                    Console.WriteLine("2nd Peak in word spacing wasn't big enough to count");
                }
            }

            // try in the other direction
            peakIndex = originalPeakIndex;
            valleyIndex = peakIndex - 1;
            valleyValue = peakValue;
            lastIndex = peakIndex;
            lastValue = -1;
            foreach(KeyValuePair<int, int> pair in this.separations.Reverse())
            {
                if(pair.Key < peakIndex)
                {
                    if(pair.Key != lastIndex - 1)
                    {
                        valleyIndex = pair.Key + 1;
                        valleyValue = 0;
                        foundValley = true;
                        break;
                    }
                    if((pair.Value >= lastValue) && (lastValue * 2 < peakValue))
                    {
                        valleyIndex = lastIndex;
                        valleyValue = lastValue;
                        foundValley = true;
                        break;
                    }
                }
                lastIndex = pair.Key;
                lastValue = pair.Value;
            }

            if(foundValley)
            {
                int peakMinus2Index = valleyIndex - 1;
                int peakMinus2Value = 0;
                foreach(KeyValuePair<int, int> pair in this.separations.Reverse())
                {
                    if((pair.Key < valleyIndex) && (pair.Value >= peakMinus2Value))
                    {
                        peakMinus2Index = pair.Key;
                        peakMinus2Value = pair.Value;
                    }
                }
                if((peakMinus2Value >= valleyValue * 5 / 4) && (peakMinus2Value >= valleyValue + 3))
                {
                    if((peaks.Count > 0) && (Math.Abs(peaks[0] - peakMinus2Index) > 1))
                    {
                        this.peaks.Insert(0, peakMinus2Index);
                    }
                    else
                    {
                        Console.WriteLine("2nd Negative Peak in word spacing wasn't big enough to count");
                    }
                }
                else
                {
                    Console.WriteLine("2nd Negative Peak in word spacing wasn't big enough to count");
                }
            }
            else
            {
                Console.WriteLine("No Valley Found");
            }
        }

        private static object BuildKerningMaps()
        {
            /*
             * Unadjusted spelling errors
             * THA T (in italics) (Black Heaven 1)
            */
            if(leftKerning == null)
            {
                leftKerning = new Dictionary<char, int>();
                rightKerning = new Dictionary<char, int>();
                leftItalicKerning = new Dictionary<char, int>();
                rightItalicKerning = new Dictionary<char, int>();

                leftItalicKerning['A'] = 2;
                leftItalicKerning['À'] = 2;
                leftItalicKerning['Á'] = 2;
                leftItalicKerning['Â'] = 2;
                leftItalicKerning['Ã'] = 2;
                leftItalicKerning['Ä'] = 2;
                leftItalicKerning['Å'] = 2;
                leftItalicKerning['Ą'] = 2;
                leftItalicKerning['B'] = 1;
                leftItalicKerning['b'] = 1;
                leftItalicKerning['d'] = 1;
                leftItalicKerning['f'] = 1;
                leftItalicKerning['h'] = 1;
                leftItalicKerning['j'] = 4;
                leftItalicKerning['J'] = 3;
                leftItalicKerning['k'] = 1;
                leftItalicKerning['l'] = -1;
                leftItalicKerning['o'] = -1;
                leftItalicKerning['ò'] = -1;
                leftItalicKerning['ó'] = -1;
                leftItalicKerning['ô'] = -1;
                leftItalicKerning['õ'] = -1;
                leftItalicKerning['ö'] = -1;
                leftItalicKerning['ó'] = -1;
                leftItalicKerning['p'] = 2;
                leftItalicKerning['s'] = 1;
                leftItalicKerning['ś'] = 1;
                leftItalicKerning['y'] = 2;     // reduced from 3 for Soul Eater
                leftItalicKerning['ý'] = 2;
                leftItalicKerning['\''] = -2;
                leftItalicKerning[']'] = -3;
                leftItalicKerning[')'] = -3;
                leftItalicKerning['}'] = -3;
                leftItalicKerning['0'] = -1;
                leftItalicKerning['-'] = -2;
                leftItalicKerning['—'] = -2;
                leftItalicKerning['_'] = -2;
                leftItalicKerning['\"'] = -2;
                leftItalicKerning['„'] = -2;
                defaultLeftItalicKerning = new Dictionary<char, int>(leftItalicKerning);

                rightItalicKerning['d'] = 2;
                rightItalicKerning['f'] = 4;
                rightItalicKerning['k'] = 1;
                rightItalicKerning['l'] = 2;
                rightItalicKerning['I'] = 2;
                rightItalicKerning['Ì'] = 2;
                rightItalicKerning['Í'] = 2;
                rightItalicKerning['Î'] = 2;
                rightItalicKerning['Ï'] = 2;
                rightItalicKerning['L'] = -2;
                rightItalicKerning['O'] = -1;
                rightItalicKerning['Ò'] = -1;
                rightItalicKerning['Ó'] = -1;
                rightItalicKerning['Ô'] = -1;
                rightItalicKerning['Õ'] = -1;
                rightItalicKerning['Ö'] = -1;
                rightItalicKerning['Ó'] = -1;
                rightItalicKerning['r'] = 2;
                rightItalicKerning['v'] = 1;
                rightItalicKerning['w'] = 1;
                rightItalicKerning['y'] = 1;
                rightItalicKerning['ý'] = 1;
                rightItalicKerning['['] = -3;
                rightItalicKerning['('] = -3;
                rightItalicKerning['{'] = -3;
                rightItalicKerning['0'] = -1;
                rightItalicKerning['\"'] = -1;
                rightItalicKerning['„'] = -1;
                rightItalicKerning['-'] = -2;
                rightItalicKerning['—'] = -2;
                rightItalicKerning['_'] = -2;
                defaultRightItalicKerning = new Dictionary<char, int>(rightItalicKerning);

                leftKerning['A'] = 1;
                leftKerning['À'] = 1;
                leftKerning['Á'] = 1;
                leftKerning['Â'] = 1;
                leftKerning['Ã'] = 1;
                leftKerning['Ä'] = 1;
                leftKerning['Å'] = 1;
                leftKerning['Ą'] = 1;
                leftKerning['c'] = 1;
                leftKerning['ç'] = 1;
                leftKerning['ć'] = 1;
                leftKerning['f'] = 1;   // added for Haibane Renmai
                leftKerning['l'] = -1;
                leftKerning['I'] = -1;
                leftKerning['Ì'] = -1;
                leftKerning['Í'] = -1;
                leftKerning['Î'] = -1;
                leftKerning['Ï'] = -1;
                leftKerning['j'] = 2;
                leftKerning['J'] = 1;
                leftKerning['l'] = -1;
                leftKerning['o'] = 1;
                leftKerning['ò'] = 1;
                leftKerning['ó'] = 1;
                leftKerning['ô'] = 1;
                leftKerning['õ'] = 1;
                leftKerning['ö'] = 1;
                leftKerning['O'] = 1;
                leftKerning['Ò'] = 1;
                leftKerning['Ó'] = 1;
                leftKerning['Ô'] = 1;
                leftKerning['Õ'] = 1;
                leftKerning['Ö'] = 1;
                leftKerning['w'] = 1;
                leftKerning['t'] = 1;   // added for One Piece
                leftKerning['T'] = 1;
                leftKerning['V'] = 1;   // added for One Piece
                leftKerning['W'] = 1;
                leftKerning['y'] = 2;
                leftKerning['ý'] = 2;
                leftKerning['Y'] = 1;
                leftKerning['Ý'] = 1;
                leftKerning['1'] = -1;
                leftKerning[']'] = -3;
                leftKerning[')'] = -3;
                leftKerning['}'] = -3;
                leftKerning['-'] = -2;
                leftKerning['—'] = -2;
                leftKerning['_'] = -2;
                leftKerning['\"'] = -1;
                leftKerning['„'] = -1;
                defaultLeftKerning = new Dictionary<char, int>(leftKerning);

                rightKerning['A'] = 1;
                rightKerning['À'] = 1;
                rightKerning['Á'] = 1;
                rightKerning['Â'] = 1;
                rightKerning['Ã'] = 1;
                rightKerning['Ä'] = 1;
                rightKerning['Å'] = 1;
                rightKerning['Ą'] = 2;
                rightKerning['ą'] = 1;
                rightKerning['f'] = 2;
                rightKerning['G'] = -1;
                rightKerning['l'] = -1;
                rightKerning['I'] = -1;
                rightKerning['J'] = -1;
                rightKerning['k'] = 1;  // added for One Piece
                rightKerning['o'] = 1;
                rightKerning['ò'] = 1;
                rightKerning['ó'] = 1;
                rightKerning['ô'] = 1;
                rightKerning['õ'] = 1;
                rightKerning['ö'] = 1;
                rightKerning['ó'] = 1;
                rightKerning['O'] = 1;
                rightKerning['Ò'] = 1;
                rightKerning['Ó'] = 1;
                rightKerning['Ô'] = 1;
                rightKerning['Õ'] = 1;
                rightKerning['Ö'] = 1;
                rightKerning['Ó'] = 1;
                rightKerning['r'] = 1;
                rightKerning['t'] = 1;  // added for Haibane Renmai
                rightKerning['w'] = 1;
                rightKerning['W'] = 1;
                rightKerning['Y'] = 1;
                rightKerning['Ý'] = 1;
                rightKerning['\"'] = -1;
                rightKerning['„'] = -1;
                rightKerning['1'] = -3;
                rightKerning['['] = -3;
                rightKerning['('] = -3;
                rightKerning['{'] = -3;
                rightKerning['-'] = -2;
                rightKerning['—'] = -2;
                rightKerning['_'] = -2;
                defaultRightKerning = new Dictionary<char, int>(rightKerning);
            }
            return typeof(FontKerning);
        }

        public static void AddWeight(SortedList<int, int> list, int key, int value)
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

        public static int FindKerning(OcrCharacter leftText, OcrCharacter rightText)
        {
            int diff = 0;
            int kern;
            if(leftText.Italic)
            {
                if(rightItalicKerning.TryGetValue(leftText.Value, out kern))
                {
                    diff += kern;
                }
            }
            else
            {
                if(rightKerning.TryGetValue(leftText.Value, out kern))
                {
                    diff += kern;
                }
            }
            if(rightText.Italic)
            {
                if(leftItalicKerning.TryGetValue(rightText.Value, out kern))
                {
                    diff += kern;
                }
            }
            else
            {
                if(leftKerning.TryGetValue(rightText.Value, out kern))
                {
                    diff += kern;
                }
            }
            return diff;
        }
    }
}
