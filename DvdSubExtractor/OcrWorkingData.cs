using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using DvdNavigatorCrm;
using DvdSubOcr;

namespace DvdSubExtractor
{
    public class OcrWorkingData : IDisposable
    {
        static readonly int[] FontSizes = new int[] { 
            14, 15, 16, 17, 18, 19, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 44, 48, 52, 56, 60, 66, 72, 80, 88, 96 };

        List<ISubtitleData> streamSubtitles = new List<ISubtitleData>();
        List<IList<OcrRectangle>> rectangles = new List<IList<OcrRectangle>>();
        SortedDictionary<int, IList<SubtitleLine>> allLinesBySubtitle = new SortedDictionary<int, IList<SubtitleLine>>();
        List<AudioStreamItem> audioStreams = new List<AudioStreamItem>();
        List<CellStartInfo> cellStarts = new List<CellStartInfo>();
        HashSet<string> allowedBaselineErrors = new HashSet<string>();

        public OcrWorkingData(SubtitlePacklist packList, int streamId, bool forcedOnly)
        {
            this.streamSubtitles.AddRange(packList.Subtitles.Where(
                sub => (sub.StreamId == streamId) && (!forcedOnly || sub.Forced)));
            this.audioStreams.AddRange(packList.AudioStreams);
            this.cellStarts.AddRange(packList.CellStarts);
        }

        public IList<ISubtitleData> Subtitles { get { return streamSubtitles.AsReadOnly(); } }
        public IList<AudioStreamItem> AudioStreams { get { return this.audioStreams.AsReadOnly(); } }
        public VideoAttributes VideoAttributes { get; set; }
        public IList<CellStartInfo> CellStarts { get { return cellStarts; } }
        public ISet<string> AllowedBaselineErrors { get { return this.allowedBaselineErrors; } }

        public void ClearRectangles()
        {
            this.Rectangles.Clear();
        }

        public IList<IList<OcrRectangle>> Rectangles { get { return rectangles; } }

        public void ClearCompiledLines()
        {
            if(this.FontList != null)
            {
                this.FontList.Dispose();
                this.FontList = null;
            }
            this.AllLinesBySubtitle.Clear();
        }

        public void CompileSubtitleLines()
        {
            this.ClearCompiledLines();
            this.FontList = new OcrFontList();
            for(int index = 0; index < this.Rectangles.Count; index++)
            {
                IList<OcrRectangle> rectList = this.Rectangles[index];
                IList<SubtitleLine> lines = new List<SubtitleLine>();
                if(rectList != null)
                {
                    int rectIndex = 0;
                    foreach(OcrRectangle rect in rectList)
                    {
                        bool commentAdded = false;
                        foreach(SubtitleLine line in rect.SubtitleText.Lines)
                        {
                            SubtitleLine newLine = new SubtitleLine(
                                line.Text, line.TextBounds, line.BlockIndexes, line.Bounds, line.ColorIndex);
                            line.RectangleIndex = rectIndex;
                            if(!commentAdded)
                            {
                                newLine.Comment = rect.Comment;
                                commentAdded = true;
                            }
                            lines.Add(newLine);
                        }
                        rectIndex++;
                    }
                    this.FontList.AddRange(lines);
                }
                this.AllLinesBySubtitle.Add(index, lines);
            }

            this.FontList.MergeSimilarFonts();
            this.FontList.BuildFontLineMap();
            this.FontList.AddSpacesToAllLines();

            List<Font> allNormalFonts = new List<Font>();
            List<Font> allItalicFonts = new List<Font>();
            foreach(int fontSize in FontSizes)
            {
                allNormalFonts.Add(new Font(Properties.Settings.Default.SubitleFileFontName, fontSize, FontStyle.Regular));
                allItalicFonts.Add(new Font(Properties.Settings.Default.SubitleFileFontName, fontSize, FontStyle.Italic));
            }

            float windowsToDvdFontHeightConversion;
            switch(this.VideoAttributes.AspectRatio)
            {
            case VideoAspectRatio._4by3:
                windowsToDvdFontHeightConversion = SubConstants.WindowsToDvd4x3FontHeightConversion;
                break;
            default:
                windowsToDvdFontHeightConversion = SubConstants.WindowsToDvdFontHeightConversion;
                break;
            }
            this.FontList.MatchToWindowsFonts(allNormalFonts, allItalicFonts,
                allNormalFonts[5].Clone() as Font, allItalicFonts[5].Clone() as Font,
                windowsToDvdFontHeightConversion);
        }

        public void CorrectSpellings(OcrMap ocrMap)
        {
            int subCount = this.AllLinesBySubtitle.Count;
            for(int subIndex = 0; subIndex < subCount; subIndex++)
            {
                foreach(SubtitleLine line in this.AllLinesBySubtitle[subIndex])
                {
                    line.CorrectSpelling(ocrMap);
                }
            }
        }

        public OcrFontList FontList { get; set; }
        public SortedDictionary<int, IList<SubtitleLine>> AllLinesBySubtitle { get { return allLinesBySubtitle; } }

        public IEnumerable<SubtitleLine> AllLines
        {
            get
            {
                foreach(IList<SubtitleLine> lineList in this.allLinesBySubtitle.Values)
                {
                    foreach(SubtitleLine line in lineList)
                    {
                        yield return line;
                    }
                }
            }
        }

        void Dispose(bool isManaged)
        {
            if(this.FontList != null)
            {
                this.FontList.Dispose();
                this.FontList = null;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        ~OcrWorkingData()
        {
            Dispose(false);
        }
    }
}
