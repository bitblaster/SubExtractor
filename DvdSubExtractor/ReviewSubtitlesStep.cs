using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DvdSubOcr;

namespace DvdSubExtractor
{
    public partial class ReviewSubtitlesStep : UserControl, IWizardItem
    {
        ExtractData data;
        int subIndex;
        Bitmap subBitmap;
        bool showOriginalSubs;

        public ReviewSubtitlesStep()
        {
            InitializeComponent();
            this.subBitmap = new Bitmap(this.subtitlePictureBox.Width,
                    this.subtitlePictureBox.Height);
        }

        public void Initialize(ExtractData data)
        {
            this.data = data;
            DrawCurrentSubtitle();

            this.FindForm().KeyDown += this.ReviewSubtitlesStep_KeyDown;
            this.FindForm().KeyUp += this.ReviewSubtitlesStep_KeyUp;
        }

        public void Terminate()
        {
            this.FindForm().KeyDown -= this.ReviewSubtitlesStep_KeyDown;
            this.FindForm().KeyUp -= this.ReviewSubtitlesStep_KeyUp;
        }

        public void OptionsUpdated()
        {
        }

        public bool IsComplete
        {
            get { return true; }
        }

        public string HelpText
        {
            get { return "Review the results of your DVD Subtitle OCR.  Hit 'Next Step' whenever you're ready."; }
        }

        public IEnumerable<Type> JumpToStepsAllowed
        {
            get
            {
                yield return typeof(LoadFolderStep);
                yield return typeof(ChooseSubtitlesStep);
                yield break;
            }
        }

        public event EventHandler StatusUpdated;
        public event EventHandler<TypeEventArgs> JumpTo;

        private void OnStatusUpdated()
        {
            EventHandler tempHandler = this.StatusUpdated;
            if(tempHandler != null)
            {
                tempHandler(this, EventArgs.Empty);
            }
        }

        private void OnJumpTo(Type type)
        {
            EventHandler<TypeEventArgs> tempHandler = this.JumpTo;
            if(tempHandler != null)
            {
                tempHandler(this, new TypeEventArgs(type));
            }
        }

        void ReviewSubtitlesStep_KeyDown(object sender, KeyEventArgs e)
        {
            this.showOriginalSubs = e.Control;
            DrawCurrentSubtitle();
        }

        void ReviewSubtitlesStep_KeyUp(object sender, KeyEventArgs e)
        {
            this.showOriginalSubs = e.Control;
            DrawCurrentSubtitle();
        }

        void DrawCurrentSubtitle()
        {
            if(this.subIndex >= this.data.WorkingData.AllLinesBySubtitle.Count)
            {
                return;
            }

            using(Graphics g = Graphics.FromImage(this.subBitmap))
            {
                g.Clear(Color.Transparent);

                PointF subOffset = this.data.WorkingData.Subtitles[this.subIndex].Origin;
                if(this.showOriginalSubs)
                {
                    DvdSubtitleData subtitleData = this.data.WorkingData.Subtitles[this.subIndex];
                    DvdSubtitleBitmap subtitle = DvdSubtitleDecoder.DecodeBitmap(subtitleData.Data,
                        subtitleData.Pts, subtitleData.YuvPalette);
                    if(subtitle != null)
                    {
                        g.DrawImage(subtitle.Bitmap, subOffset);
                        subtitle.Dispose();
                    }
                }
                else
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    StringFormat format = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Center;
                    foreach(SubtitleLine line in this.data.WorkingData.AllLinesBySubtitle[this.subIndex])
                    {
                        KeyValuePair<OcrFont, OcrFont> fonts = this.data.WorkingData.FontList.FindFontsForLine(line);
                        Font fontNormal = fonts.Key.MatchingRealFont;
                        Font fontItalic = fonts.Value.MatchingRealFont;

                        Brush textBrushNormal = Brushes.Black, textBrushItalic = Brushes.Black;
                        PointF center = subOffset + new SizeF(line.Bounds.Left + line.Bounds.Width / 2,
                            line.Bounds.Top + line.Bounds.Height / 2);

                        float totalWidth = 0.0f;
                        foreach(KeyValuePair<bool, string> textPart in line.SplitByItalics())
                        {
                            SizeF size = g.MeasureString(textPart.Value, textPart.Key ? fontItalic : fontNormal);
                            totalWidth += size.Width;
                        }
                        center.X -= Convert.ToInt32(totalWidth / 2);
                        foreach(KeyValuePair<bool, string> textPart in line.SplitByItalics())
                        {
                            Font font = textPart.Key ? fontItalic : fontNormal;
                            Brush textBrush = textPart.Key ? textBrushItalic : textBrushNormal;
                            g.DrawString(textPart.Value, font, textBrush, center, format);
                            center.X += Convert.ToInt32(g.MeasureString(textPart.Value, font).Width);
                        }
                    }
                }
            }

            this.subtitlePictureBox.Image = null;
            this.subtitlePictureBox.Image = this.subBitmap;
            this.indexLabel.Text = String.Format("Subtitle {0} of {1}",
                this.subIndex + 1, this.data.WorkingData.AllLinesBySubtitle.Count);
        }

        private void previousButton_Click(object sender, EventArgs e)
        {
            if(this.subIndex > 0)
            {
                this.subIndex--;
                DrawCurrentSubtitle();
            }
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if(this.subIndex < this.data.WorkingData.AllLinesBySubtitle.Count - 1)
            {
                this.subIndex++;
                DrawCurrentSubtitle();
            }
        }
    }
}
