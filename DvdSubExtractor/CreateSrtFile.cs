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
    public class CreateSrtFile
    {
        ExtractData data;
        SortedDictionary<double, double> ptsOffsets;
        string fileFullPath;
        CreateSubOptions options;

        public static bool Create(ExtractData data, SortedDictionary<double, double> ptsOffsets, CreateSubOptions options)
        {
            try
            {
                CreateSrtFile srt = new CreateSrtFile(data, ptsOffsets, options);
                srt.CreateSubtitleFile();
                return true;
            }
            catch(IOException ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
            return false;
        }

        private CreateSrtFile(ExtractData data, SortedDictionary<double, double> ptsOffsets, CreateSubOptions options)
        {
            this.data = data;
            this.ptsOffsets = ptsOffsets;
            this.fileFullPath = Path.Combine(options.OutputDirectory, options.FileName);
            this.options = options;
        }

        class SubtitleEntry
        {
            public string TimeText { get; set; }
            public string Text { get; set; }
        }

        class SubtitleBlock
        {
            public double Pts { get; set; }
            public double Duration { get; set; }
            public SubtitleEntry Entry { get; set; }
        }

        private void CreateSubtitleFile()
        {
            OcrWorkingData work = this.data.WorkingData;

            Dictionary<SubtitleLine, string> lineStyleNames = new Dictionary<SubtitleLine, string>();
            Dictionary<OcrFont, string> usedFontStyleNames = new Dictionary<OcrFont, string>();

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

                    if(duration >= SubConstants.MaximumMillisecondsOnScreen)
                    {
                        duration = Math.Min(duration, SubConstants.MaximumMillisecondsOnScreen);
                    }
                    if(entry.Key < work.Subtitles.Count - 1)
                    {
                        double nextPts = work.Subtitles[entry.Key + 1].Pts + ptsAdjustment;
                        // don't allow overlapping timestamps for srt subtitles
                        duration = Math.Min(duration, nextPts - pts);
                    }

                    if(duration > 0)
                    {
                        if(this.options.Adjust25to24)
                        {
                            pts = pts * 25.0 / 24.0;
                            duration = duration * 25.0 / 24.0;
                        }

                        SubtitleEntry newEntry = AddSrtEntries(entry.Value,
                            lineStyleNames, pts, duration, origin, videoSize, options.RemoveSDH);

                        if((newEntry != null) || !String.IsNullOrWhiteSpace(comment))
                        {
                            if(!String.IsNullOrWhiteSpace(comment))
                            {
                                string commentText = "COMMENT: " + comment + "\r\n";
                                if(newEntry != null)
                                {
                                    newEntry.Text += commentText;
                                }
                                else
                                {
                                    newEntry = new SubtitleEntry()
                                    {
                                        TimeText = CreateSrtTime(pts, duration),
                                        Text = commentText,
                                    };
                                }
                            }

                            if(allSubs.Count > 0)
                            {
                                SubtitleBlock prevSub = allSubs[allSubs.Count - 1];
                                if((Math.Abs(prevSub.Pts + prevSub.Duration - pts) < SubConstants.PtsSlushMilliseconds) && 
                                    (prevSub.Entry.Text == newEntry.Text))
                                {
                                    allSubs.RemoveAt(allSubs.Count - 1);
                                    pts = prevSub.Pts;
                                    duration += prevSub.Duration;
                                    newEntry.TimeText = CreateSrtTime(pts, duration);
                                }
                            }

                            allSubs.Add(new SubtitleBlock()
                            {
                                Entry = newEntry,
                                Pts = pts,
                                Duration = duration
                            });
                        }
                    }
                }
            }

            /*for(int blockIndex = 0; blockIndex < allSubs.Count - 1; blockIndex++)
            {
                SubtitleBlock block1 = allSubs[blockIndex];
                SubtitleBlock block2 = allSubs[blockIndex + 1];

                if((block1.Entries.Count == 1) && (block2.Entries.Count == 1) && 
                    (Math.Abs(block1.Pts + block1.Duration - block2.Pts) < SubConstants.PtsSlushMilliseconds) &&
                    (block1.Entries[0].Text == block2.Entries[0].Text))
                {
                    TimeSpan timeStart = new TimeSpan(Convert.ToInt64(block1.Pts * 10000));
                    TimeSpan timeEnd = timeStart + new TimeSpan(Convert.ToInt64(block1.Duration * 10000));
                    block1.Entries[0].TimeText = BuildDialogueTimeEntry(entry2.FontStyleName, "", timeStart, timeEnd);

                }
            }*/

            Encoding enc = Properties.Settings.Default.StoreSrtAsANSI ? Encoding.Default : Encoding.UTF8;
            using(StreamWriter fstream = new StreamWriter(this.fileFullPath, false, enc))
            {
                int index = 1;
                foreach(SubtitleBlock subBlock in allSubs)
                {
                    fstream.WriteLine(index++.ToString());
                    fstream.Write(subBlock.Entry.TimeText);
                    fstream.Write(subBlock.Entry.Text);
                    fstream.WriteLine();
                }
            }
        }

        static string CreateSrtTime(double pts, double duration)
        {
            TimeSpan timeStart = new TimeSpan(Convert.ToInt64(pts * 10000 + 5000));
            TimeSpan timeEnd = new TimeSpan(Convert.ToInt64((pts + duration) * 10000 + 5000));
            // 00:02:15,734 --> 00:02:19,724
            return String.Format(
                "{0:d2}:{1:d2}:{2:d2},{3:d3} --> {4:d2}:{5:d2}:{6:d2},{7:d3}\r\n",
                timeStart.Hours, timeStart.Minutes, timeStart.Seconds, timeStart.Milliseconds,
                timeEnd.Hours, timeEnd.Minutes, timeEnd.Seconds, timeEnd.Milliseconds);
        }

        static SubtitleEntry AddSrtEntries(IEnumerable<SubtitleLine> lines,
            Dictionary<SubtitleLine, string> fontStyleNames, double pts, double duration,
            Point origin, Size videoSize, RemoveSDH removeSDH)
        {
            List<SubtitleLine> allLines = new List<SubtitleLine>(lines.Where(line => line.Text.Count > 0));
            allLines.Sort((sub1, sub2) => ((sub1.Bounds.Top + sub1.Bounds.Height / 2).CompareTo(
                sub2.Bounds.Top + sub2.Bounds.Height / 2))); // go from the top to the bottom

            List<SdhSubLine> subLines = new List<SdhSubLine>(
                allLines.ConvertAll(line => new SdhSubLine { Text = line.Text, RectangleIndex = line.RectangleIndex, OriginalLine = line }));
            if(removeSDH != RemoveSDH.None)
            {
                SdhSubLine.RemoveSDH(subLines);

                HashSet<int> rectMap = new HashSet<int>();
                foreach(SdhSubLine sdh in subLines)
                {
                    if(sdh.Text.Count != 0)
                    {
                        rectMap.Add(sdh.RectangleIndex);
                    }
                }
                if(rectMap.Count > 1)
                {
                    rectMap.Clear();
                    foreach(SdhSubLine sdh in subLines)
                    {
                        if((sdh.Text.Count != 0) && !rectMap.Contains(sdh.RectangleIndex))
                        {
                            rectMap.Add(sdh.RectangleIndex);
                            sdh.LineStartRemoved = true;
                        }
                    }
                }

                bool carry = false;
                int lineCount = 0;
                foreach(SdhSubLine sdh in subLines)
                {
                    if(sdh.Text.Count != 0)
                    {
                        bool addCount = false;
                        if(sdh.LineStartRemoved || SubConstants.CharactersThatSignalLineBreak.Contains(sdh.Text[0].Value))
                        {
                            addCount = true;
                        }
                        else if(carry)
                        {
                            addCount = true;
                            sdh.LineStartRemoved = true;
                        }

                        if(addCount)
                        {
                            lineCount++;
                            carry = false;
                        }
                    }
                    else
                    {
                        if(sdh.LineStartRemoved)
                        {
                            carry = true;
                        }
                    }
                }

                foreach(SdhSubLine sdh in subLines)
                {
                    if(sdh.LineStartRemoved && (sdh.Text.Count != 0) && (lineCount > 1) &&
                        !SubConstants.CharactersThatSignalLineBreak.Contains(sdh.Text[0].Value))
                    {
                        List<OcrCharacter> newText = new List<OcrCharacter>(sdh.Text);
                        newText.Insert(0, new OcrCharacter(SubConstants.ChangeOfSpeakerPrefix, sdh.Text[0].Italic));
                        sdh.Text = newText;
                    }
                }

                subLines.RemoveAll(srt => srt.Text.Count == 0);
            }
            else
            {
                HashSet<int> rectMap = new HashSet<int>();
                foreach(SdhSubLine sdh in subLines)
                {
                    if(sdh.Text.Count != 0)
                    {
                        rectMap.Add(sdh.RectangleIndex);
                    }
                }
                if(rectMap.Count > 1)
                {
                    rectMap.Clear();
                    foreach(SdhSubLine sdh in subLines)
                    {
                        if((sdh.Text.Count != 0) && !rectMap.Contains(sdh.RectangleIndex))
                        {
                            rectMap.Add(sdh.RectangleIndex);
                            if(!SubConstants.CharactersThatSignalLineBreak.Contains(sdh.Text[0].Value))
                            {
                                List<OcrCharacter> newText = new List<OcrCharacter>(sdh.Text);
                                newText.Insert(0, new OcrCharacter(SubConstants.ChangeOfSpeakerPrefix, sdh.Text[0].Italic));
                                sdh.Text = newText;
                            }
                        }
                    }
                }
            }

            SubtitleEntry entry = new SubtitleEntry();
            StringBuilder sbText = new StringBuilder();
            foreach(SdhSubLine line in subLines)
            {
                if(entry.TimeText == null)
                {
                    entry.TimeText = CreateSrtTime(pts, duration);
                }

                bool isItalic = false;
                CreateAssFile.FixSubtitleLineHyphens(line.Text);
                foreach(OcrCharacter ocr in line.Text)
                {
                    if(ocr.Italic && !isItalic)
                    {
                        sbText.Append(@"<i>");
                        isItalic = true;
                    }
                    else if(!ocr.Italic && isItalic)
                    {
                        sbText.Append(@"</i>");
                        isItalic = false;
                    }
                    sbText.Append(ocr.Value);
                }
                if(isItalic)
                {
                    sbText.Append(@"</i>");
                }
                sbText.AppendLine();
            }

            if(entry.TimeText != null)
            {
                string newText = sbText.ToString();
                newText = newText.Replace(CloseAndOpenItalics, JustNewLine);
                entry.Text = newText;
                return entry;
            }
            else
            {
                return null;
            }
        }

        static readonly string CloseAndOpenItalics = "</i>" + Environment.NewLine + "<i>";
        static readonly string JustNewLine = Environment.NewLine;
    }
}
