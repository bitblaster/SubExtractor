using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DvdSubOcr;
using DvdNavigatorCrm;

namespace DvdSubExtractor
{
    public partial class OcrReviewForm : Form
    {
        OcrMap ocrMap;
        int movieId;

        public OcrReviewForm()
        {
            InitializeComponent();
        }

        public bool ChangesMade { get; private set; }

        public void LoadMap(OcrMap ocrMap, int movieId)
        {
            this.ocrMap = ocrMap;
            this.movieId = movieId;

            this.splitListBox.Items.Clear();
            foreach(KeyValuePair<string, OcrMap.SplitMapEntry> split in ocrMap.Splits)
            {
                if(split.Value.MovieIds.Contains(movieId))
                {
                    SplitEntry entry = new SplitEntry(split.Key,
                        split.Value.Split1, split.Value.Split2);
                    this.splitListBox.Items.Add(entry);
                }
            }
            if(this.splitListBox.Items.Count != 0)
            {
                this.splitListBox.SelectedIndex = 0;
            }
            else
            {
                this.splitListBox.Enabled = false;
                this.removeSplitButton.Enabled = false;
            }

            SortedDictionary<OcrCharacter, List<OcrEntry>> matchEntries =
                new SortedDictionary<OcrCharacter, List<OcrEntry>>();

            foreach(OcrEntry entry in this.ocrMap.GetMatchesForMovie(this.movieId, true))
            {
                List<OcrEntry> entries;
                if(!matchEntries.TryGetValue(entry.OcrCharacter, out entries))
                {
                    entries = new List<OcrEntry>();
                    matchEntries[entry.OcrCharacter] = entries;
                }
                entries.Add(entry);
            }

            /*HashSet<string> highDefs = new HashSet<string>(this.ocrMap.HighDefMatches);
            foreach(var v in this.ocrMap.Matches)
            {
                if(highDefs.Contains(v.Key) && (v.Value.Count != 0))
                {
                    foreach(OcrEntry ent in v.Value)
                    {
                        List<OcrEntry> oldEntries;
                        if(!matchEntries.TryGetValue(ent.OcrCharacter, out oldEntries))
                        {
                            oldEntries = new List<OcrEntry>();
                            matchEntries[ent.OcrCharacter] = oldEntries;
                        }
                        oldEntries.Add(ent);
                    }
                }
            }*/

            foreach(KeyValuePair<OcrCharacter, List<OcrEntry>> ocr in matchEntries)
            {
                ocr.Value.Sort((ocr1, ocr2) => ocr1.CalculateBounds().Height.CompareTo(ocr2.CalculateBounds().Height));
                foreach(OcrEntry entry in ocr.Value)
                {
                    this.ocrListBox.Items.Add(new OcrMatchEntry(entry));
                }
            }
            if(this.ocrListBox.Items.Count != 0)
            {
                this.ocrListBox.SelectedIndex = 0;
            }
            else
            {
                this.ocrListBox.Enabled = false;
                this.removeOcrButton.Enabled = false;
            }
        }

        void DataDispose()
        {
            this.split1PictureBox.Image = null;
            this.split2PictureBox.Image = null;
            foreach(SplitEntry split in this.splitListBox.Items)
            {
                split.Dispose();
            }
            this.splitListBox.Items.Clear();

            foreach(OcrMatchEntry entry in this.ocrListBox.Items)
            {
                entry.Dispose();
            }
            this.ocrListBox.Items.Clear();
        }

        class OcrMatchEntry : IDisposable
        {
            static readonly StringFormat CharFormat = CreateCharFormat();
            static readonly Font FontNormal = new Font("Arial", 18.0f, FontStyle.Regular);
            static readonly Font FontItalic = new Font("Arial", 18.0f, FontStyle.Italic);

            static StringFormat CreateCharFormat()
            {
                StringFormat format = new StringFormat(StringFormatFlags.NoClip);
                format.Alignment = StringAlignment.Far;
                format.LineAlignment = StringAlignment.Center;
                return format;
            }

            public OcrMatchEntry(OcrEntry entry)
            {
                this.Entry = entry;
                Color backColor = Color.Transparent;
                this.Image = entry.CreateBlockBitmap(Color.Black, backColor, 100, 20);
                using(Graphics g = Graphics.FromImage(this.Image))
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.DrawString(new string(entry.OcrCharacter.Value, 1),
                        entry.OcrCharacter.Italic ? FontItalic : FontNormal, Brushes.Blue,
                        new RectangleF(0, 0, this.Image.Width, this.Image.Height),
                        CharFormat);
                }
            }

            public OcrEntry Entry { get; private set; }
            public Bitmap Image { get; private set; }

            public void Dispose()
            {
                if(this.Image != null)
                {
                    this.Image.Dispose();
                    this.Image = null;
                }
            }
        }

        class SplitEntry : IDisposable
        {
            public SplitEntry(string fullEncode, OcrSplit split1, OcrSplit split2)
            {
                this.FullEncode = fullEncode;
                BlockEncode block = new BlockEncode(fullEncode);
                this.OriginalImage = block.CreateBlockBitmap(Color.Black,
                    Color.Transparent, 20, 20);
                BlockEncode block1 = new BlockEncode(split1.FullEncode);
                this.Split1Image = block1.CreateBlockBitmap(Color.Blue,
                    Color.White, 20, 20);
                BlockEncode block2 = new BlockEncode(split2.FullEncode);
                this.Split2Image = block2.CreateBlockBitmap(Color.Green,
                    Color.White, 20, 20);
            }

            public string FullEncode { get; private set; }
            public Bitmap OriginalImage { get; private set; }
            public Bitmap Split1Image { get; private set; }
            public Bitmap Split2Image { get; private set; }

            public void Dispose()
            {
                if(this.OriginalImage != null)
                {
                    this.OriginalImage.Dispose();
                    this.OriginalImage = null;
                }
                if(this.Split1Image != null)
                {
                    this.Split1Image.Dispose();
                    this.Split1Image = null;
                }
                if(this.Split2Image != null)
                {
                    this.Split2Image.Dispose();
                    this.Split2Image = null;
                }
            }
        }

        private void splitListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if((this.splitListBox.Items.Count != 0) && (this.splitListBox.SelectedIndex >= 0))
            {
                SplitEntry entry = this.splitListBox.Items[this.splitListBox.SelectedIndex] as SplitEntry;
                this.split1PictureBox.Image = entry.Split1Image;
                this.split2PictureBox.Image = entry.Split2Image;
            }
            else
            {
                this.split1PictureBox.Image = null;
                this.split2PictureBox.Image = null;
            }
        }

        private void splitListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if((this.splitListBox.Items.Count != 0) && (e.Index >= 0))
            {
                SplitEntry entry = this.splitListBox.Items[e.Index] as SplitEntry;
                e.ItemWidth = entry.OriginalImage.Width;
                e.ItemHeight = entry.OriginalImage.Height;
            }
        }

        private void splitListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if((this.splitListBox.Items.Count != 0) && (e.Index >= 0))
            {
                if((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);
                }
                else
                {
                    e.Graphics.FillRectangle(Brushes.White, e.Bounds);
                }
                SplitEntry entry = this.splitListBox.Items[e.Index] as SplitEntry;
                e.Graphics.DrawImage(entry.OriginalImage, e.Bounds.Location);
            }
        }

        private void removeSplitBbutton_Click(object sender, EventArgs e)
        {
            if((this.splitListBox.Items.Count != 0) && (this.splitListBox.SelectedIndex >= 0))
            {
                SplitEntry entry = this.splitListBox.Items[this.splitListBox.SelectedIndex] as SplitEntry;
                this.ocrMap.RemoveSplit(entry.FullEncode);
                this.splitListBox.Items.Remove(entry);
                entry.Dispose();
                this.ChangesMade = true;
                if(this.splitListBox.Items.Count != 0)
                {
                    //this.splitListBox.SelectedIndex = 0;
                }
                else
                {
                    this.split1PictureBox.Image = null;
                    this.split2PictureBox.Image = null;
                    this.removeSplitButton.Enabled = false;
                }
            }
        }

        private void doneButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void removeOcrButton_Click(object sender, EventArgs e)
        {
            if((this.ocrListBox.Items.Count != 0) && (this.ocrListBox.SelectedIndex >= 0))
            {
                OcrMatchEntry entry = this.ocrListBox.SelectedItem as OcrMatchEntry;
                this.ocrMap.RemoveMatch(entry.Entry);
                this.ocrListBox.Items.Remove(entry);
                entry.Dispose();
                this.ChangesMade = true;
                if(this.ocrListBox.Items.Count != 0)
                {
                    //this.ocrListBox.SelectedIndex = 0;
                }
                else
                {
                    this.removeOcrButton.Enabled = false;
                }
            }
        }

        private void removeCharacterButton_Click(object sender, EventArgs e)
        {
            if((this.ocrListBox.Items.Count != 0) && (this.ocrListBox.SelectedIndex >= 0))
            {
                OcrMatchEntry selectedEntry = this.ocrListBox.SelectedItem as OcrMatchEntry;
                List<OcrMatchEntry> charEntries = new List<OcrMatchEntry>();
                foreach(OcrMatchEntry charEntry in this.ocrListBox.Items)
                {
                    if(charEntry.Entry.OcrCharacter == selectedEntry.Entry.OcrCharacter)
                    {
                        charEntries.Add(charEntry);
                    }
                }

                foreach(OcrMatchEntry charEntry in charEntries)
                {
                    this.ocrMap.RemoveMatch(charEntry.Entry);
                    this.ocrListBox.Items.Remove(charEntry);
                    charEntry.Dispose();
                }

                this.ChangesMade = true;
                if(this.ocrListBox.Items.Count != 0)
                {
                    //this.ocrListBox.SelectedIndex = 0;
                }
                else
                {
                    this.removeOcrButton.Enabled = false;
                }
            }
        }

        private void ocrListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if((this.ocrListBox.Items.Count != 0) && (e.Index >= 0))
            {
                OcrMatchEntry entry = this.ocrListBox.Items[e.Index] as OcrMatchEntry;
                e.ItemWidth = entry.Image.Width;
                e.ItemHeight = entry.Image.Height;
            }
        }

        private void ocrListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if((this.ocrListBox.Items.Count != 0) && (e.Index >= 0))
            {
                if((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);
                }
                else if((e.State & DrawItemState.Focus) == DrawItemState.Focus)
                {
                    e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);
                    Rectangle rect = e.Bounds;
                    rect.Width--;
                    rect.Height--;
                    e.Graphics.DrawRectangle(Pens.DarkGray, rect);
                }
                else
                {
                    e.Graphics.FillRectangle(Brushes.White, e.Bounds);
                }
                OcrMatchEntry entry = this.ocrListBox.Items[e.Index] as OcrMatchEntry;
                e.Graphics.DrawImage(entry.Image, e.Bounds.Location);
            }
        }

        private void removeAllTrainingsButton_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Do you really want to remove ALL trainings used in OCRing this movie?", "Really?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                List<OcrEntry> entriesMovie = new List<OcrEntry>(
                    this.ocrMap.GetMatchesForMovie(this.movieId, true));
                foreach(OcrEntry oldEntry in entriesMovie)
                {
                    this.ocrMap.RemoveMatch(oldEntry, this.movieId);
                }
                this.ocrListBox.Items.Clear();
                this.removeOcrButton.Enabled = false;
                this.removeCharacterButton.Enabled = false;
                this.removeAllTrainingsButton.Enabled = false;
                this.ChangesMade = true;
            }
        }

        private void removeAllSplitsButton_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Do you really want to remove ALL splits used in OCRing this movie?", "Really?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                List<string> splitsMovie = new List<string>();
                foreach(KeyValuePair<string, OcrMap.SplitMapEntry> split in this.ocrMap.Splits)
                {
                    if(split.Value.MovieIds.Contains(this.movieId))
                    {
                        splitsMovie.Add(split.Key);
                    }
                }
                foreach(string fullEncode in splitsMovie)
                {
                    this.ocrMap.RemoveSplit(fullEncode, this.movieId);
                }

                this.splitListBox.Items.Clear();
                this.removeSplitButton.Enabled = false;
                this.removeAllSplitsButton.Enabled = false;
                this.ChangesMade = true;
            }
        }
    }
}
