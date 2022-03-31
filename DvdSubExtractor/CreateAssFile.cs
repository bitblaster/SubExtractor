using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DvdSubOcr;
using DvdNavigatorCrm;

namespace DvdSubExtractor
{
    public class CreateAssFile
    {
        ExtractData data;
        SortedDictionary<double, double> ptsOffsets;
        string fileFullPath;
        string[] headers;
        CreateSubOptions options;

        public static bool Create(ExtractData data, SortedDictionary<double, double> ptsOffsets, CreateSubOptions options)
        {
            try
            {
                CreateAssFile ass = new CreateAssFile(data, ptsOffsets, options);
                ass.CreateSubtitleFile();
                return true;
            }
            catch(IOException ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
            return false;
        }

        private CreateAssFile(ExtractData data, SortedDictionary<double, double> ptsOffsets, CreateSubOptions options)
        {
            this.data = data;
            this.ptsOffsets = ptsOffsets;
            this.fileFullPath = Path.Combine(options.OutputDirectory, options.FileName);
            this.options = options;

            this.headers = AssHeader.Clone() as string[];
            if(this.data.WorkingData.VideoAttributes.HorizontalResolution != 0)
            {
                // if cropping to change from 16x9 to 4x3, put that in the file
                if((this.data.WorkingData.VideoAttributes.AspectRatio == VideoAspectRatio._16by9) &&
                    (this.options.Crop.X > this.data.WorkingData.VideoAttributes.HorizontalResolution / 10))
                {
                    headers[4] = string.Format(AssHeader[4],
                        this.data.WorkingData.VideoAttributes.HorizontalResolution * 3 / 4);
                }
                else
                {
                    headers[4] = string.Format(AssHeader[4],
                        this.data.WorkingData.VideoAttributes.HorizontalResolution - Math.Max(0, (2 * this.options.Crop.X)));
                }
                headers[5] = string.Format(AssHeader[5],
                    this.data.WorkingData.VideoAttributes.VerticalResolution - Math.Max(0, (2 * this.options.Crop.Y)));
            }
            else
            {
                headers[4] = string.Format(AssHeader[4], SubConstants.DefaultDvdHorizontalPixels);
                headers[5] = string.Format(AssHeader[5], SubConstants.DefaultDvdVerticalPixels);
            }
            headers[7] = string.Format(AssHeader[7], "");
            headers[8] = string.Format(AssHeader[8], "");
        }

        static readonly string[] AssHeader = new string[] 
        {
            "[Script Info]",
            "Title: Default file",
            "ScriptType: v4.00+",
            "WrapStyle: 0",
            "PlayResX: {0}",
            "PlayResY: {0}",
            "ScaledBorderAndShadow: yes",
            "Audio File: {0}",
            "Video File: {0}",
            "Video Aspect Ratio: 0",
            "Video Zoom: 6",
            "Video Position: 0",
            "",
            "[V4+ Styles]",
            "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding",
        };

        static string CreateAssStyleColor(Color color)
        {
            return @"&H" + (255 - color.A).ToString("X2") + color.B.ToString("X2") + color.G.ToString("X2") + color.R.ToString("X2");
        }

        //const string AssStyleDescription = "Style: {0},{1},{2},&H00E2DFE4,&H000000FF,&H20000000,&HC8000000,{3},0,0,0,100,100,0,0,1,{4},{5},2,40,40,15,1";

        static string CreateAssStyle(string name, string fontName, int fontSize, 
            Color textColor, Color borderColor, Color shadowColor,
            bool isBold, float borderWidth, float shadowWidth, int horizontalMargin, int verticalMargin, int scaleX)
        {
            return string.Format(CultureInfo.InvariantCulture.NumberFormat, 
                "Style: {0},{1},{2},{3},&H000000FF,{4},{5},{6},0,0,0,{11},100,0,0,1,{7},{8},2,{9},{9},{10},1",
                name, fontName, fontSize, CreateAssStyleColor(textColor), CreateAssStyleColor(borderColor),
                CreateAssStyleColor(shadowColor), isBold ? "1" : "0", borderWidth.ToString("f1", CultureInfo.InvariantCulture.NumberFormat),
                shadowWidth.ToString("f1", CultureInfo.InvariantCulture.NumberFormat), horizontalMargin, verticalMargin, scaleX);
        }

        static readonly string[] AssHeaderAfterStyle = new string[] 
        {
            "",
            "[Events]",
            "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text"
        };

        class SubtitleEntry
        {
            public string TimeText { get; set; }
            public string StyleText { get; set; }
            public string Text { get; set; }
            public string FontStyleName { get; set; }
            public bool IsDefaultLocation { get; set; }
            public bool IsDiscontinuity { get; set; }
            public bool NeedsHyphenation { get; set; }
            public Point OriginalOrigin { get; set; }
            public string OriginalColor { get; set; }
        }

        class SubtitleBlock
        {
            public double Pts { get; set; }
            public double Duration { get; set; }
            public List<SubtitleEntry> Entries { get; set; }
        }

        static Color FindBrightestColor(int colorIndex, IList<Color> rgbPalette)
        {
            int bitFlag = 1;
            Color brightestColor = Color.Black;
            for(int index = 0; index < rgbPalette.Count; index++)
            {
                if((colorIndex & bitFlag) == bitFlag)
                {
                    Color color = rgbPalette[index];
                    if(brightestColor.R + brightestColor.G + brightestColor.B < color.R + color.G + color.B)
                    {
                        brightestColor = color;
                    }
                }
                bitFlag <<= 1;
            }
            return brightestColor;
        }

        static string BuildDialogueTimeEntry(string fontStyleName, string prefix, TimeSpan timeStart, TimeSpan timeEnd)
        {
            string timeLine = string.Format(
                CultureInfo.InvariantCulture.NumberFormat, "Dialogue: {0}0,{1:d1}:{2:d2}:{3:d2}.{4:d2},{5:d1}:{6:d2}:{7:d2}.{8:d2},{9},Unknown,0000,0000,0000,,",
                prefix, timeStart.Hours, timeStart.Minutes, timeStart.Seconds, timeStart.Milliseconds / 10,
                timeEnd.Hours, timeEnd.Minutes, timeEnd.Seconds, timeEnd.Milliseconds / 10, fontStyleName);
            return timeLine;
        }

        const string DefaultStyleName = "Dialogue1";
        const string GenericStyleName = "Dialogue{0}";

        private void CreateSubtitleFile()
        {
            OcrWorkingData work = this.data.WorkingData;

            Color mostCommonColor = Properties.Settings.Default.SubtitleForeColor;
            Dictionary<Color, int> colorsUsed = new Dictionary<Color, int>();
            foreach(KeyValuePair<int, IList<SubtitleLine>> entry in work.AllLinesBySubtitle)
            {
                if(entry.Value.Count != 0)
                {
                    ISubtitleData subData = work.Subtitles[entry.Key];
                    foreach(SubtitleLine line in entry.Value)
                    {
                        Color brightest = FindBrightestColor(line.ColorIndex, subData.RgbPalette);
                        int count;
                        colorsUsed.TryGetValue(brightest, out count);
                        colorsUsed[brightest] = count + 1;
                    }
                }
            }

            KeyValuePair<Color, int> colorMax = new KeyValuePair<Color, int>(mostCommonColor, 0);
            foreach(KeyValuePair<Color, int> entry in colorsUsed)
            {
                if(entry.Value > colorMax.Value)
                {
                    colorMax = entry;
                }
            }
            mostCommonColor = colorMax.Key;

            Color defaultColor = (Properties.Settings.Default.SubtitleColorScheme == 1) ? 
                mostCommonColor : Properties.Settings.Default.SubtitleForeColor;

            Dictionary<SubtitleLine, string> lineStyleNames = new Dictionary<SubtitleLine, string>();
            Dictionary<OcrFont, string> usedFontStyleNames = new Dictionary<OcrFont, string>();
            int styleIndex = 2;
            Dictionary<OcrFont, string> fontStyleNames = new Dictionary<OcrFont, string>();
            foreach(OcrFont font in work.FontList.AllFonts)
            {
                string styleName = string.Format(GenericStyleName, styleIndex++);
                fontStyleNames[font] = styleName;
            }

            foreach(IList<SubtitleLine> lineList in work.AllLinesBySubtitle.Values)
            {
                foreach(SubtitleLine line in lineList)
                {
                    OcrFont dominantFont =
                        this.data.WorkingData.FontList.FindDominantFontForLine(line);
                    lineStyleNames[line] = fontStyleNames[dominantFont];
                    if(!usedFontStyleNames.ContainsKey(dominantFont))
                    {
                        usedFontStyleNames[dominantFont] = fontStyleNames[dominantFont];
                    }
                }
            }

            Size videoSize = this.data.WorkingData.VideoAttributes.Size;
            List<SubtitleBlock> allSubs = new List<SubtitleBlock>();
            foreach(KeyValuePair<int, IList<SubtitleLine>> entry in work.AllLinesBySubtitle)
            {
                if(entry.Value.Count != 0)
                {
                    ISubtitleData subData = work.Subtitles[entry.Key];
                    string comment = entry.Value[0].Comment;

                    double ptsAdjustment = this.options.OverallPtsAdjustment;
                    foreach(KeyValuePair<double, double> offsetEntry in this.ptsOffsets)
                    {
                        if(subData.Pts >= offsetEntry.Key)
                        {
                            ptsAdjustment += offsetEntry.Value;
                        }
                        else
                        {
                            break;
                        }
                    }

                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture.NumberFormat, "Pts {0:f0} Adjust {1:f0}", subData.Pts, ptsAdjustment));

                    double pts = subData.Pts + ptsAdjustment;
                    double duration = subData.Duration;
                    Point origin = subData.Origin;

                    bool isMaxDuration = false;
                    if(duration >= SubConstants.MaximumMillisecondsOnScreen)
                    {
                        duration = Math.Min(duration, SubConstants.MaximumMillisecondsOnScreen);
                        isMaxDuration = true;
                    }
                    if(entry.Key < work.Subtitles.Count - 1)
                    {
                        double nextPts = work.Subtitles[entry.Key + 1].Pts + ptsAdjustment;
                        if(isMaxDuration)
                        {
                            // don't allow overlapping timestamps for srt subtitles
                            duration = Math.Min(duration, nextPts - pts);
                        }
                        else if(duration > 0)
                        {
                            if(Math.Abs(pts + duration - nextPts) <= SubConstants.PtsSlushMilliseconds)
                            {
                                duration = nextPts - pts;
                            }
                        }
                    }

                    if(duration > 0)
                    {
                        if(this.options.Adjust25to24)
                        {
                            pts = pts * 25.0 / 24.0;
                            duration = duration * 25.0 / 24.0;
                        }

                        Point originOffset = new Point(-this.options.Crop.X, -this.options.Crop.Y);
                        List<SubtitleEntry> newEntryList = new List<SubtitleEntry>(
                            AddDialogueEntries("", entry.Value, lineStyleNames, pts, duration, origin, originOffset, videoSize, 
                            subData.RgbPalette, mostCommonColor, 
                            this.options.PositionAllSubs == LineBreaksAndPositions.KeepBreaksAndPositions, this.options.RemoveSDH));

                        if(this.options.PositionAllSubs == LineBreaksAndPositions.KeepBreaks)
                        {
                            newEntryList.ForEach(act => act.IsDiscontinuity = true);
                        }

                        if(!String.IsNullOrWhiteSpace(comment) && (newEntryList.Count > 0))
                        {
                            SubtitleEntry firstEntry = newEntryList[0];

                            string timeText;
                            string text;
                            timeText = firstEntry.TimeText.Replace("Dialogue:", "Comment:");
                            text = comment + "\r\n";

                            SubtitleEntry commentEntry = new SubtitleEntry()
                            {
                                FontStyleName = firstEntry.FontStyleName,
                                IsDefaultLocation = false,
                                IsDiscontinuity = true,
                                NeedsHyphenation = false,
                                OriginalColor = firstEntry.OriginalColor,
                                OriginalOrigin = firstEntry.OriginalOrigin,
                                StyleText = "",
                                Text = text,
                                TimeText = timeText,
                            };
                            newEntryList.Insert(0, commentEntry);
                        }

                        allSubs.Add(new SubtitleBlock()
                        {
                            Entries = newEntryList,
                            Pts = pts,
                            Duration = duration
                        });
                    }
                }
            }

            string colorCommonStyle = String.Format(CultureInfo.InvariantCulture.NumberFormat, @"{{\c&H{0:X2}{1:X2}{2:X2}&}}",
                defaultColor.B, defaultColor.G, defaultColor.R);

            // try to extend the duration of identical lines in consequetive blocks
            for(int index = 0; index < allSubs.Count; index++)
            {
                SubtitleBlock block1 = allSubs[index];
                foreach(SubtitleEntry entry1 in block1.Entries)
                {
                    double newDuration = block1.Duration;
                    for(int index2 = index + 1; index2 < allSubs.Count; index2++)
                    {
                        SubtitleBlock block2 = allSubs[index2];
                        bool foundExtension = false;

                        if(Math.Abs(block1.Pts + newDuration - block2.Pts) <
                            SubConstants.PtsSlushMilliseconds)
                        {
                            for(int entryIndex = 0; entryIndex < block2.Entries.Count; entryIndex++)
                            {
                                SubtitleEntry entry2 = block2.Entries[entryIndex];
                                if((entry2.Text == entry1.Text) && (entry2.OriginalColor == entry1.OriginalColor) &&
                                    ((entry2.IsDefaultLocation && entry1.IsDefaultLocation) ||
                                        (entry2.OriginalOrigin == entry1.OriginalOrigin)))
                                {
                                    newDuration = block2.Pts + block2.Duration - block1.Pts;
                                    TimeSpan timeStart = new TimeSpan(Convert.ToInt64(block1.Pts * 10000));
                                    TimeSpan timeEnd = timeStart + new TimeSpan(Convert.ToInt64(newDuration * 10000));
                                    if(entry1.IsDefaultLocation && !entry2.IsDefaultLocation)
                                    {
                                        entry1.TimeText = BuildDialogueTimeEntry(entry2.FontStyleName, "", timeStart, timeEnd);
                                        entry1.FontStyleName = entry2.FontStyleName;
                                        entry1.StyleText = entry2.StyleText;
                                        entry1.IsDefaultLocation = false;
                                    }
                                    else
                                    {
                                        entry1.TimeText = BuildDialogueTimeEntry(entry1.FontStyleName, "", timeStart, timeEnd);
                                        if(entry2.StyleText.Length > entry1.StyleText.Length)
                                        {
                                            entry1.StyleText = entry2.StyleText;
                                        }
                                    }
                                    if(entry2.IsDiscontinuity && !entry1.IsDiscontinuity)
                                    {
                                        entry1.IsDiscontinuity = true;
                                        entry1.NeedsHyphenation = entry2.NeedsHyphenation;
                                    }
                                    block2.Entries.RemoveAt(entryIndex);
                                    foundExtension = true;
                                    break;
                                }
                                else
                                {
                                    if((entry1.Text == entry2.Text) && (entry1.StyleText == entry2.StyleText))
                                    {
                                        Console.WriteLine("");
                                    }
                                }
                            }
                        }

                        if(!foundExtension)
                        {
                            break;
                        }
                    }

                    if(entry1.IsDiscontinuity && entry1.NeedsHyphenation)
                    {
                        entry1.Text = SubConstants.ChangeOfSpeakerPrefix + entry1.Text;
                    }
                }

                // combine entries that have the same time span
                for(int entryIndex1 = 0; entryIndex1 < block1.Entries.Count; entryIndex1++)
                {
                    SubtitleEntry entry1 = block1.Entries[entryIndex1];
                    if(!entry1.IsDefaultLocation)
                    {
                        continue;
                    }

                    for(int entryIndex2 = entryIndex1 + 1; entryIndex2 < block1.Entries.Count; entryIndex2++)
                    {
                        SubtitleEntry entry2 = block1.Entries[entryIndex2];
                        if(!entry2.IsDefaultLocation)
                        {
                            continue;
                        }

                        if((entry1.TimeText == entry2.TimeText) && (entry1.FontStyleName == entry2.FontStyleName))
                        {
                            if(entry1.StyleText == entry2.StyleText)
                            {
                                if(entry2.IsDiscontinuity)
                                {
                                    entry1.Text = entry1.Text.TrimEnd('\r', '\n') + "\\N" + entry2.Text;
                                }
                                else
                                {
                                    entry1.Text = entry1.Text.TrimEnd('\r', '\n') + " " + entry2.Text;
                                }
                            }
                            else
                            {
                                if(entry2.StyleText.Length == 0)
                                {
                                    entry2.StyleText = colorCommonStyle;
                                }

                                if(entry2.IsDiscontinuity)
                                {
                                    entry1.Text = entry1.Text.TrimEnd('\r', '\n') + "\\N" + entry2.StyleText + entry2.Text;
                                }
                                else
                                {
                                    entry1.Text = entry1.Text.TrimEnd('\r', '\n') + " " + entry2.StyleText + entry2.Text;
                                }
                            }
                            block1.Entries.RemoveAt(entryIndex2);
                            entryIndex2--;
                        }
                    }
                }
                block1.Entries.Reverse();
            }

            int scaleX = SubConstants.FontScaleNormal;
            if(this.data.WorkingData.VideoAttributes.HorizontalResolution == SubConstants.DefaultDvdHorizontalPixels)
            {
                switch(this.data.WorkingData.VideoAttributes.AspectRatio)
                {
                case VideoAspectRatio._16by9:
                    scaleX = SubConstants.FontScaleX16x9Dvd;
                    break;
                case VideoAspectRatio._4by3:
                    scaleX = SubConstants.FontScaleX4x3Dvd;
                    break;
                }
            }

            using(StreamWriter fstream = new StreamWriter(this.fileFullPath, false, Encoding.UTF8))
            {
                if(headers != null)
                {
                    foreach(string line in headers)
                    {
                        fstream.WriteLine(line);
                    }
                }

                int horizMargin = (this.data.WorkingData.VideoAttributes.AspectRatio == VideoAspectRatio._16by9) ?
                    Properties.Settings.Default.SubtitleHorizontal16x9Margin :
                    Properties.Settings.Default.SubtitleHorizontal4x3Margin;
                if((this.data.WorkingData.VideoAttributes.HorizontalResolution != 0) &&
                    (this.data.WorkingData.VideoAttributes.AspectRatio == VideoAspectRatio._16by9) &&
                    (this.options.Crop.X > this.data.WorkingData.VideoAttributes.HorizontalResolution / 10))
                {
                    horizMargin = Properties.Settings.Default.SubtitleHorizontal4x3Margin;
                }
                int vertMargin = Properties.Settings.Default.SubtitleVerticalMargin;
                float defaultFontSize = Properties.Settings.Default.SubtitleFontDefaultSize;
                float fontOutline = Properties.Settings.Default.SubtitleFontOutline;
                float fontShadow = Properties.Settings.Default.SubtitleFontShadow;

                if(this.options.Is1080p)
                {
                    defaultFontSize *= 2.0f;
                    horizMargin *= 2;
                    vertMargin *= 2;
                    fontOutline *= 2.0f;
                    fontShadow *= 2.0f;
                }

                string defaultStyleLine = CreateAssStyle(DefaultStyleName,
                    Properties.Settings.Default.SubitleFileFontName,
                    Convert.ToInt32(defaultFontSize),
                    defaultColor,
                    Properties.Settings.Default.SubtitleBorderColor,
                    Properties.Settings.Default.SubtitleShadingColor,
                    Properties.Settings.Default.SubtitleFontBold,
                    fontOutline, fontShadow,
                    horizMargin, vertMargin, scaleX);
                fstream.WriteLine(defaultStyleLine);

                double fontHeightConversion = 1.0 +
                    Convert.ToDouble(Properties.Settings.Default.FontSizeAdjustmentPercent) / 100.0;

                HashSet<string> stylesWithEntries = new HashSet<string>();
                foreach(SubtitleBlock subBlock in allSubs)
                {
                    foreach(SubtitleEntry subEntry in subBlock.Entries)
                    {
                        if(!stylesWithEntries.Contains(subEntry.FontStyleName))
                        {
                            stylesWithEntries.Add(subEntry.FontStyleName);
                        }
                    }
                }

                foreach(KeyValuePair<OcrFont, string> entry in usedFontStyleNames)
                {
                    if(stylesWithEntries.Contains(entry.Value))
                    {
                        int fontHeight = entry.Key.MatchingRealFont.Height;
                        if(fontHeightConversion != 1.0)
                        {
                            fontHeight = Convert.ToInt32(Convert.ToDouble(fontHeight) * fontHeightConversion);
                        }

                        string styleLine = CreateAssStyle(entry.Value,
                            Properties.Settings.Default.SubitleFileFontName,
                            fontHeight,
                            defaultColor,
                            Properties.Settings.Default.SubtitleBorderColor,
                            Properties.Settings.Default.SubtitleShadingColor,
                            Properties.Settings.Default.SubtitleFontBold,
                            fontOutline, fontShadow,
                            horizMargin, vertMargin, scaleX);
                        fstream.WriteLine(styleLine);
                    }
                }

                foreach(string line in AssHeaderAfterStyle)
                {
                    fstream.WriteLine(line);
                }

                foreach(SubtitleBlock subBlock in allSubs)
                {
                    foreach(SubtitleEntry subEntry in subBlock.Entries)
                    {
                        fstream.Write(subEntry.TimeText);
                        fstream.Write(subEntry.StyleText);
                        fstream.Write(subEntry.Text);
                    }
                }
            }
        }

        static string CreateColorTag(int colorIndex, IList<Color> rgbPalette, Color mostCommonColor)
        {
            Color brightestColor = FindBrightestColor(colorIndex, rgbPalette);
            string colorOverride = "";
            if(brightestColor != mostCommonColor)
            {
                colorOverride = String.Format(CultureInfo.InvariantCulture.NumberFormat, @"{{\c&H{0:X2}{1:X2}{2:X2}&}}", brightestColor.B, brightestColor.G, brightestColor.R);
            }
            //if(brightestColor.B + brightestColor.G + brightestColor.R < 200)
            int bitCount = 0;
            for(int bitFlag = 0x8000; bitFlag > 0; bitFlag >>= 1)
            {
                if((colorIndex & bitFlag) != 0)
                {
                    bitCount++;
                }
            }
            if(bitCount == 1)
            {
                // if there is no shading or border around the original DVD subtitle, turn off our own border
                colorOverride += @"{\bord0}";
            }
            return colorOverride;
        }

        static void AppendSubtitleLineToString(StringBuilder sb, IList<OcrCharacter> lineText)
        {
            bool isItalic = false;
            foreach(OcrCharacter ocr in lineText)
            {
                if(ocr.Italic != isItalic)
                {
                    if(isItalic)
                    {
                        sb.Append(@"{\i0}");
                    }
                    else
                    {
                        sb.Append(@"{\i1}");
                    }
                    isItalic = ocr.Italic;
                }
                sb.Append(ocr.Value);
            }
            if(isItalic)
            {
                sb.Append(@"{\i0}");
            }
        }

        public static void FixSubtitleLineHyphens(IList<OcrCharacter> lineText)
        {
            if(lineText.Count >= 2)
            {
                switch(lineText[0].Value)
                {
                case '-':
                    switch(lineText[1].Value)
                    {
                    case ' ':
                        if((lineText.Count >= 3) && (lineText[0].Italic != lineText[2].Italic))
                        {
                            lineText[0] = new OcrCharacter(lineText[0].Value, lineText[2].Italic);
                        }
                        break;
                    case '-':
                    case '—':
                        if((lineText.Count >= 3) && (lineText[2].Value == ' '))
                        {
                            lineText.RemoveAt(1);
                            if(lineText.Count >= 3)
                            {
                                if(lineText[0].Italic != lineText[2].Italic)
                                {
                                    lineText[0] = new OcrCharacter(lineText[0].Value, lineText[2].Italic);
                                }
                                if(lineText[1].Italic != lineText[2].Italic)
                                {
                                    lineText[1] = new OcrCharacter(lineText[1].Value, lineText[2].Italic);
                                }
                            }
                        }
                        break;
                    default:
                        lineText.Insert(1, new OcrCharacter(' ', lineText[1].Italic));
                        if(lineText[0].Italic != lineText[2].Italic)
                        {
                            lineText[0] = new OcrCharacter(lineText[0].Value, lineText[2].Italic);
                        }
                        break;
                    }
                    break;
                case '—':
                    if(lineText[1].Value == ' ')
                    {
                        lineText.RemoveAt(1);
                        if((lineText.Count >= 2) && (lineText[0].Italic != lineText[1].Italic))
                        {
                            lineText[0] = new OcrCharacter(lineText[0].Value, lineText[1].Italic);
                        }
                    }
                    break;
                default:
                    break;
                }
            }
        }

        class LineState
        {
            public int LeftEdge { get; set; }
            public int RightEdge { get; set; }
            public int MidX { get; set; }
            public int MidY { get; set; }
            public int MidYAbove { get; set; }
            public bool Centered { get; set; }
            public bool LineBreak { get; set; }
            public bool IsDefaultWithoutY { get; set; }
            public bool IsDefault { get; set; }
        }

        static IEnumerable<SubtitleEntry> AddDialogueEntries(string prefix, IEnumerable<SubtitleLine> lines,
            Dictionary<SubtitleLine, string> fontStyleNames, double pts, double duration, Point origin, Point originOffset,
            Size videoSize, IList<Color> rgbPalette, Color mostCommonColor, bool positionAllLines, RemoveSDH removeSDH)
        {
            TimeSpan timeStart = new TimeSpan(Convert.ToInt64(pts * 10000 + 5000));
            TimeSpan timeEnd = new TimeSpan(Convert.ToInt64((pts + duration) * 10000 + 5000));

            int leftAllowed = Convert.ToInt32(videoSize.Width * SubConstants.LeftCenterMinimum);
            int rightAllowed = Convert.ToInt32(videoSize.Width * SubConstants.RightCenterMaximum);
            int edgeSlush = SubConstants.DvdLineEdgeSlush;
            if(videoSize.Width > SubConstants.DefaultDvdHorizontalPixels * 3 / 2)
            {
                edgeSlush *= 2;
            }

            List<SubtitleLine> allLines = new List<SubtitleLine>(lines.Where(line => line.Text.Count > 0));
            allLines.Sort((sub1, sub2) => -((sub1.Bounds.Top + sub1.Bounds.Height / 2).CompareTo(
                sub2.Bounds.Top + sub2.Bounds.Height / 2))); // sort from the bottom to the top

            List<SdhSubLine> subLines = new List<SdhSubLine>(
                allLines.ConvertAll(line => new SdhSubLine { Text = line.Text, RectangleIndex = line.RectangleIndex, OriginalLine = line }));
            if(removeSDH != RemoveSDH.None)
            {
                subLines.Reverse();
                SdhSubLine.RemoveSDH(subLines);

                HashSet<int> carryRect = new HashSet<int>();
                Dictionary<int, int> linesPerRect = new Dictionary<int, int>();
                foreach(SdhSubLine sdh in subLines)
                {
                    if(!linesPerRect.ContainsKey(sdh.RectangleIndex))
                    {
                        linesPerRect[sdh.RectangleIndex] = 0;
                    }

                    if(sdh.Text.Count != 0)
                    {
                        bool addCount = false;
                        if(sdh.LineStartRemoved || SubConstants.CharactersThatSignalLineBreak.Contains(sdh.Text[0].Value))
                        {
                            addCount = true;
                        }
                        else if(carryRect.Contains(sdh.RectangleIndex))
                        {
                            addCount = true;
                            sdh.LineStartRemoved = true;
                        }

                        if(addCount)
                        {
                            linesPerRect[sdh.RectangleIndex] = linesPerRect[sdh.RectangleIndex] + 1;
                            carryRect.Remove(sdh.RectangleIndex);
                        }
                    }
                    else
                    {
                        if(sdh.LineStartRemoved)
                        {
                            carryRect.Add(sdh.RectangleIndex);
                        }
                    }
                }

                foreach(SdhSubLine sdh in subLines)
                {
                    if(sdh.LineStartRemoved && (sdh.Text.Count != 0) && (linesPerRect[sdh.RectangleIndex] > 1) &&
                        !SubConstants.CharactersThatSignalLineBreak.Contains(sdh.Text[0].Value))
                    {
                        List<OcrCharacter> newText = new List<OcrCharacter>(sdh.Text);
                        newText.Insert(0, new OcrCharacter(SubConstants.ChangeOfSpeakerPrefix, sdh.Text[0].Italic));
                        sdh.Text = newText;
                    }
                }

                subLines.RemoveAll(srt => srt.Text.Count == 0);
                subLines.Reverse();
            }

            List<LineState> states = new List<LineState>();
            foreach(SdhSubLine subLine in subLines)
            {
                SubtitleLine line = subLine.OriginalLine;
                LineState state = new LineState();
                state.MidY = origin.Y + line.Bounds.Top + line.Bounds.Height / 2;
                state.LeftEdge = origin.X + line.Bounds.Left;
                state.RightEdge = origin.X + line.Bounds.Right;
                state.MidX = state.LeftEdge + line.Bounds.Width / 2;
                state.Centered = (state.MidX > leftAllowed) && (state.MidX < rightAllowed);
                int lineHeight = Math.Max(line.Bounds.Height, videoSize.Height / 20) * 3 / 2;
                state.MidYAbove = origin.Y + line.Bounds.Top - lineHeight;
                state.LineBreak = SubConstants.CharactersThatSignalLineBreak.Contains(subLine.Text[0].Value);
                state.IsDefaultWithoutY = state.Centered;
                states.Add(state);
            }

            if(!positionAllLines)
            {
                // extend default (non-positioned, placed bottom center of screen) state of lines up and down if there are 
                // other lines just above or below which have the same left edge or if both lines have a line breaking character at the start like '-'
                for(int index1 = 0; index1 < subLines.Count - 1; index1++)
                {
                    LineState state1 = states[index1];
                    if(state1.IsDefaultWithoutY)
                    {
                        for(int index2 = index1 + 1; index2 < subLines.Count; index2++)
                        {
                            LineState state2 = states[index2];
                            if(state2.MidY < state1.MidYAbove)
                            {
                                break;
                            }
                            if(!state2.IsDefaultWithoutY)
                            {
                                if((Math.Abs(state1.LeftEdge - state2.LeftEdge) <= edgeSlush) || (Math.Abs(state1.RightEdge - state2.RightEdge) <= edgeSlush))
                                {
                                    state2.IsDefaultWithoutY = true;
                                }
                            }
                        }
                    }
                }
                for(int index1 = subLines.Count - 1; index1 > 0; index1--)
                {
                    LineState state1 = states[index1];
                    if(state1.IsDefaultWithoutY)
                    {
                        for(int index2 = index1 - 1; index2 >= 0; index2--)
                        {
                            LineState state2 = states[index2];
                            if(state1.MidY < state2.MidYAbove)
                            {
                                break;
                            }
                            if(!state2.IsDefaultWithoutY)
                            {
                                if((Math.Abs(state1.LeftEdge - state2.LeftEdge) <= edgeSlush) || (Math.Abs(state1.RightEdge - state2.RightEdge) <= edgeSlush))
                                {
                                    state2.IsDefaultWithoutY = true;
                                }
                            }
                        }
                    }
                }
                int midYAllowed = Convert.ToInt32(videoSize.Height * SubConstants.UnpositionedSubtitleMaxVertical);
                for(int index = 0; index < subLines.Count; index++)
                {
                    SubtitleLine line = subLines[index].OriginalLine;
                    LineState state = states[index];
                    if(state.IsDefaultWithoutY && (state.MidY >= midYAllowed))
                    {
                        state.IsDefault = true;
                        midYAllowed = state.MidYAbove;
                    }
                    else
                    {
                        // if a line is found in a block that isn't in the default area, stop looking for more since if we find a line 
                        // above this one that is in the default area it will be displayed out of vertical order
                        break;
                    }
                }
            }

            int midYAllowedForCentering = Convert.ToInt32(videoSize.Height * SubConstants.CenteredSubtitleMinVertical);
            List<SdhSubLine> defaultPositionLines = new List<SdhSubLine>();
            for(int index = 0; index < subLines.Count; index++)
            {
                SubtitleLine line = subLines[index].OriginalLine;
                LineState state = states[index];

                if(state.IsDefault)
                {
                    defaultPositionLines.Add(subLines[index]);
                }
                else
                {
                    string fontStyleName = fontStyleNames[line];
                    string timeLine = BuildDialogueTimeEntry(fontStyleName, prefix, timeStart, timeEnd);
                    const string PositionLeftAlignTag = @"{{\an4\pos({0},{1})}}";
                    const string PositionCenterAlignTag = @"{{\an5\pos({0},{1})}}";
                    string positionTag;
                    if(state.Centered && (state.MidY < midYAllowedForCentering))
                    {
                        positionTag = string.Format(CultureInfo.InvariantCulture.NumberFormat, PositionCenterAlignTag,
                            state.MidX + originOffset.X, state.MidY + originOffset.Y);
                    }
                    else
                    {
                        positionTag = string.Format(CultureInfo.InvariantCulture.NumberFormat, PositionLeftAlignTag,
                            state.LeftEdge + originOffset.X, state.MidY + originOffset.Y);
                    }
                    string colorTag = CreateColorTag(line.ColorIndex, rgbPalette, mostCommonColor);
                    positionTag += colorTag;
                    StringBuilder sb = new StringBuilder();
                    FixSubtitleLineHyphens(subLines[index].Text);
                    AppendSubtitleLineToString(sb, subLines[index].Text);
                    sb.AppendLine();
                    yield return new SubtitleEntry()
                    {
                        FontStyleName = fontStyleName,
                        TimeText = timeLine,
                        StyleText = positionTag,
                        Text = sb.ToString(),
                        OriginalOrigin = origin + new Size(line.Bounds.Location),
                        OriginalColor = colorTag,
                    };
                }
            }

            defaultPositionLines.Reverse();
            int oldColorIndex = -2;
            foreach(SdhSubLine line in defaultPositionLines)
            {
                if(oldColorIndex == -2)
                {
                    oldColorIndex = line.OriginalLine.ColorIndex;
                }
                else
                {
                    if(line.OriginalLine.ColorIndex != oldColorIndex)
                    {
                        oldColorIndex = -1;
                        break;
                    }
                }
            }

            foreach(SdhSubLine subLine in defaultPositionLines)
            {
                StringBuilder sb = new StringBuilder();
                FixSubtitleLineHyphens(subLine.Text);
                AppendSubtitleLineToString(sb, subLine.Text);
                sb.AppendLine();
                char firstCharacter = subLine.Text[0].Value;
                string colorTag = CreateColorTag(subLine.OriginalLine.ColorIndex, rgbPalette, mostCommonColor);
                bool isLineBreak = SubConstants.CharactersThatSignalLineBreak.Contains(firstCharacter);
                yield return new SubtitleEntry()
                {
                    FontStyleName = DefaultStyleName,
                    TimeText = BuildDialogueTimeEntry(DefaultStyleName, prefix, timeStart, timeEnd),
                    IsDiscontinuity = isLineBreak || Char.IsUpper(firstCharacter) || (oldColorIndex == -1) || (subLine.OriginalLine.ColorIndex != oldColorIndex),
                    NeedsHyphenation = false,
                    IsDefaultLocation = true,
                    StyleText = colorTag,
                    Text = sb.ToString(),
                    OriginalOrigin = origin + new Size(subLine.OriginalLine.Bounds.Location),
                    OriginalColor = colorTag,
                };
                oldColorIndex = subLine.OriginalLine.ColorIndex;
            }
        }
    }
}
