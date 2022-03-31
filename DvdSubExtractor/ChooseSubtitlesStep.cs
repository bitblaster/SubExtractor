using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DvdSubOcr;
using DvdNavigatorCrm;

namespace DvdSubExtractor
{
    public partial class ChooseSubtitlesStep : UserControl, IWizardItem
    {
        ExtractData data;
        bool recursiveBinSelect;
        int selectedSubIndex;
        SubtitleBitmap subtitle;
        SubtitlePacklist packlist;
        bool recursiveIndexUpDown;
        Bitmap pictureBitmap;
        static string initialDirectory;

        public ChooseSubtitlesStep()
        {
            InitializeComponent();
        }

        class DataFilePath
        {
            public DataFilePath(string path)
            {
                this.FilePath = path;
            }

            public string FilePath { get; private set; }

            public override string ToString()
            {
                return Path.GetFileNameWithoutExtension(this.FilePath);
            }
        }

        public void Initialize(ExtractData data)
        {
            this.data = data;
            if(this.data.WorkingData != null)
            {
                this.data.WorkingData.Dispose();
                this.data.WorkingData = null;
            }

            if(Properties.Settings.Default.ScaleSubtitleBitmap)
            {
                this.scaleBitmapCheckBox.Checked = true;
            }

            bool selectedMatch = false;
            string matchFilePath = this.data.SelectedSubtitleBinaryFile ?? "";
            int originalStreamId = this.data.SelectedSubtitleStreamId;
            foreach(DvdTrackItem item in this.data.Programs)
            {
                string filePath = Path.Combine(Properties.Settings.Default.OutputDirectory,
                    this.data.ComputeSubtitleDataFileName(item));
                if(File.Exists(filePath))
                {
                    int index = this.binFileComboBox.Items.Add(new DataFilePath(filePath));
                    if(string.Compare(filePath, matchFilePath, true) == 0)
                    {
                        selectedMatch = true;
                        this.binFileComboBox.SelectedIndex = index;
                    }
                }
            }

            if(!selectedMatch)
            {
                if(matchFilePath.Length != 0)
                {
                    int index = this.binFileComboBox.Items.Add(new DataFilePath(matchFilePath));
                    this.binFileComboBox.SelectedIndex = index;
                    selectedMatch = true;
                }
                else
                {
                    this.data.SelectedSubtitleStreamId = -1;
                    if(this.binFileComboBox.Items.Count != 0)
                    {
                        this.binFileComboBox.SelectedIndex = 0;
                    }
                }
            }

            if(selectedMatch)
            {
                for(int index = 0; index < this.subtitleListBox.Items.Count; index++)
                {
                    SubtitleListItem subItem = this.subtitleListBox.Items[index] as SubtitleListItem;
                    if(subItem.Stream.StreamId == originalStreamId)
                    {
                        this.subtitleListBox.SelectedIndex = index;
                        break;
                    }
                }
            }

            this.data.NewStepInitialize(this.IsComplete, this.data.Programs.Count != 0, 
                this.HelpText, new Type[] { typeof(LoadFolderStep) });
        }

        public void Terminate()
        {
            if((this.packlist != null) && !string.IsNullOrEmpty(this.data.SelectedSubtitleBinaryFile) &&
                this.packlist.Streams.ContainsKey(this.data.SelectedSubtitleStreamId))
            {
                this.data.WorkingData = new OcrWorkingData(this.packlist, this.data.SelectedSubtitleStreamId, this.forcedCheckBox.Checked);
                this.data.WorkingData.VideoAttributes = this.packlist.VideoAttributes;
                OcrRectangle.AdjustForVideoSize(this.packlist.VideoAttributes.Size);
            }

            if(this.subtitle != null)
            {
                this.subtitle.Dispose();
                this.subtitle = null;
            }
        }

        bool IsComplete
        {
            get
            {
                return (this.subtitle != null) && (this.subtitlePictureBox.Image != null);
            }
        }

        string HelpText
        {
            get 
            { 
                return "Once you have the Subtitle data extracted from a DVD Program into " +
                    "a \'bin\' file, it's time to start the OCR (Optical Character Recognition) " +
                    "and make a new text subtitle track for your program.\n\n" +
                    "First, choose from the Subtitle data files you've just created in the " +
                    "drop-down list, or " +
                    "Browse and find one you created earlier on your hard drive.\n\nThen, choose " +
                    "one of the Subtitle tracks that were on the DVD and examine the subtitles " +
                    "within it.  Some tracks will have closed caption text (usually in brackets), " +
                    "some will only display translated signs but not speech.\n\nSelect the " +
                    "track in the list that you want to OCR and see on screen with your video " +
                    "program before hitting Next"; 
            }
        }

        class SubtitleListItem
        {
            public SubtitleListItem(SubtitleStream stream, SubtitlePacklist pack)
            {
                this.Stream = stream;

                foreach(ISubtitleData data in pack.Subtitles)
                {
                    if(data.StreamId == stream.StreamId)
                    {
                        this.Count++;
                    }
                }
                if((stream.Extension & SubpictureCodeExtension.Forced) == SubpictureCodeExtension.Forced)
                {
                    this.ForcedCount = this.Count;
                }
                else
                {
                    this.ForcedCount = pack.ForcedCount[stream.StreamId];
                }
                this.Description = string.Format("{0} {1}", DvdLanguageCodes.GetLanguageText(this.Stream.Language),
                    this.Stream.Format);
                this.LanguageCode = string.IsNullOrEmpty(this.Stream.Language) ? "" : this.Stream.Language;
            }

            public SubtitleStream Stream { get; private set; }
            public int Count { get; private set; }
            public int ForcedCount { get; private set; }
            public string Description { get; private set; }
            public string LanguageCode { get; private set; }

            public override string ToString()
            {
                return String.Format("Id {0:x2} {1} {2} ({3} Subtitles {4} Forced)",
                    this.Stream.StreamId, DvdLanguageCodes.GetLanguageText(this.Stream.Language),
                    this.Stream.Format, this.Count, this.ForcedCount);
            }
        }

        void LoadSubtitleListFromPack()
        {
            this.subtitleListBox.Items.Clear();
            foreach(SubtitleStream stream in this.packlist.Streams.Values)
            {
                if(this.packlist.Count[stream.StreamId] > 0)
                {
                    this.subtitleListBox.Items.Add(new SubtitleListItem(stream, this.packlist));
                }
            }

            if(this.subtitleListBox.Items.Count != 0)
            {
                this.subtitleListBox.SelectedIndex = 0;
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Subtitle Files (*.bin;*.idx;*.sup)|*.bin;*.idx;*.sup";
            //openFileDialog1.Filter = "Subtitle Files (*.bin)|*.bin|VobSub Files (*.idx)|*.idx|" +
            //    "Blu-Ray Subtitle Files (*.sup)|*.sup|All files (*.*)|*.*";
            if(string.IsNullOrWhiteSpace(initialDirectory))
            {
                string savedInitialDir = Properties.Settings.Default.InitialBrowseDirectory;
                if(!string.IsNullOrEmpty(savedInitialDir) && Directory.Exists(savedInitialDir))
                {
                    initialDirectory = savedInitialDir;
                }
                else
                {
                    initialDirectory = Properties.Settings.Default.OutputDirectory;
                }
            }
            openFileDialog1.InitialDirectory = initialDirectory;
            openFileDialog1.Multiselect = true;
            if(openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    this.recursiveBinSelect = true;
                    string filePath;
                    if(openFileDialog1.FileNames.Length > 1)
                    {
                        List<string> filePaths = new List<string>(openFileDialog1.FileNames.Select(name => Path.GetFullPath(name)));
                        string badFileExtension = filePaths.FirstOrDefault(name => Path.GetExtension(name).ToLowerInvariant() == ".bin");
                        if(badFileExtension != null)
                        {
                            MessageBox.Show(this, "Multi-selection of *.bin files not supported");
                            return;
                        }
                        // selection dialog usually puts the last file first unfortunately
                        if(string.Compare(filePaths[0], filePaths[filePaths.Count - 1], true) > 0)
                        {
                            filePaths.Add(filePaths[0]);
                            filePaths.RemoveAt(0);
                        }
                        filePath = filePaths[0];
                        this.data.SelectedSubtitleFiles = filePaths;
                        foreach(string path in filePaths.Skip(1).Reverse())
                        {
                            this.binFileComboBox.Items.Insert(0, new DataFilePath(path));
                        }
                    }
                    else
                    {
                        filePath = Path.GetFullPath(openFileDialog1.FileName);
                    }

                    initialDirectory = Path.GetDirectoryName(filePath);
                    if(Properties.Settings.Default.InitialBrowseDirectory != initialDirectory)
                    {
                        Properties.Settings.Default.InitialBrowseDirectory = initialDirectory;
                        Properties.Settings.Default.Save();
                    }
                    if(!Properties.Settings.Default.InitialStepIsBrowse)
                    {
                        Properties.Settings.Default.InitialStepIsBrowse = true;
                        Properties.Settings.Default.Save();
                    }

                    this.openFileDialog1.InitialDirectory = initialDirectory;
                    SubtitlePacklist newPack = new SubtitlePacklist(filePath);
                    if(newPack.Subtitles.Count != 0)
                    {
                        this.packlist = newPack;
                        this.binFileComboBox.Items.Insert(0, new DataFilePath(filePath));
                        this.binFileComboBox.SelectedIndex = 0;
                        this.data.SelectedSubtitleBinaryFile = filePath;
                        LoadSubtitleListFromPack();
                    }
                    else
                    {
                        MessageBox.Show("No Subtitles found");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Subtitle file out of date or corrupted: Exception " +
                        ex.Message);
                }
                finally
                {
                    this.recursiveBinSelect = false;
                }
                this.data.IsCurrentStepComplete = this.IsComplete;
            }
        }

        private void binFileComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(this.recursiveBinSelect)
            {
                return;
            }

            if((this.binFileComboBox.Items.Count != 0) && (this.binFileComboBox.SelectedIndex != -1))
            {
                DataFilePath dataFile = this.binFileComboBox.Items[this.binFileComboBox.SelectedIndex] as DataFilePath;
                this.packlist = new SubtitlePacklist(dataFile.FilePath);
                this.data.SelectedSubtitleBinaryFile = dataFile.FilePath;
                LoadSubtitleListFromPack();
            }
            this.data.IsCurrentStepComplete = this.IsComplete;
        }

        private void subtitleListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.indexLabel.Text = "";
            this.recursiveIndexUpDown = true;
            try
            {
                this.indexUpDown.Minimum = 0;
                this.indexUpDown.Value = 0;
                this.indexUpDown.Maximum = 0;
                this.indexUpDown.Enabled = false;
            }
            finally
            {
                this.recursiveIndexUpDown = false;
            }

            if((this.subtitleListBox.Items.Count != 0) && (this.subtitleListBox.SelectedIndex != -1))
            {
                SubtitleListItem subItem = this.subtitleListBox.SelectedItem as SubtitleListItem;
                if(subItem.ForcedCount == 0)
                {
                    this.forcedCheckBox.Checked = false;
                    this.forcedCheckBox.Enabled = false;
                }
                else
                {
                    this.forcedCheckBox.Enabled = true;
                }
                int newIndex = this.packlist.FindFirst(subItem.Stream.StreamId, this.forcedCheckBox.Checked);
                if(newIndex >= 0)
                {
                    this.selectedSubIndex = newIndex;
                    this.data.SelectedSubtitleStreamId = subItem.Stream.StreamId;
                    this.data.SelectedSubtitleDescription = subItem.LanguageCode ?? "";
                    LoadCurrentSubtitleBitmap();
                    return;
                }
            }
            else
            {
                this.forcedCheckBox.Checked = false;
                this.forcedCheckBox.Enabled = false;
            }
            this.data.SelectedSubtitleStreamId = -1;
        }

        private void previousButton_Click(object sender, EventArgs e)
        {
            if((this.subtitleListBox.Items.Count != 0) && (this.subtitleListBox.SelectedIndex != -1))
            {
                SubtitleListItem subItem = this.subtitleListBox.SelectedItem as SubtitleListItem;
                int newIndex = this.packlist.FindPrevious(this.selectedSubIndex, subItem.Stream.StreamId,
                    this.forcedCheckBox.Checked);
                if(newIndex >= 0)
                {
                    this.selectedSubIndex = newIndex;
                    LoadCurrentSubtitleBitmap();
                    return;
                }
            }
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if((this.subtitleListBox.Items.Count != 0) && (this.subtitleListBox.SelectedIndex != -1))
            {
                SubtitleListItem subItem = this.subtitleListBox.SelectedItem as SubtitleListItem;
                int newIndex = this.packlist.FindNext(this.selectedSubIndex, subItem.Stream.StreamId,
                    this.forcedCheckBox.Checked);
                if(newIndex >= 0)
                {
                    this.selectedSubIndex = newIndex;
                    LoadCurrentSubtitleBitmap();
                }
            }
        }

        private void indexUpDown_ValueChanged(object sender, EventArgs e)
        {
            if(!recursiveIndexUpDown)
            {
                if((this.subtitleListBox.Items.Count != 0) && (this.subtitleListBox.SelectedIndex != -1))
                {
                    SubtitleListItem subItem = this.subtitleListBox.SelectedItem as SubtitleListItem;
                    int newIndex = this.packlist.FindIndexFromStream(
                        Convert.ToInt32(this.indexUpDown.Value) - 1, subItem.Stream.StreamId,
                        this.forcedCheckBox.Checked);
                    if(newIndex >= 0)
                    {
                        this.selectedSubIndex = newIndex;
                        LoadCurrentSubtitleBitmap();
                    }
                }
            }
        }

        void LoadCurrentSubtitleBitmap()
        {
            ISubtitleData subtitleData = this.packlist.Subtitles[this.selectedSubIndex];
            SubtitleBitmap oldSubtitle = this.subtitle;
            this.subtitle = subtitleData.DecodeBitmap();
            if(this.subtitle != null)
            {
                if(!this.scaleBitmapCheckBox.Checked || (this.packlist.VideoAttributes.HorizontalResolution == 0) ||
                    ((this.subtitle.Width == this.subtitlePictureBox.Width) && (this.subtitle.Height == this.subtitlePictureBox.Height)))
                {
                    this.subtitlePictureBox.Image = this.subtitle.Bitmap;
                }
                else
                {
                    if(this.pictureBitmap == null)
                    {
                        this.pictureBitmap = new Bitmap(this.subtitlePictureBox.Width, this.subtitlePictureBox.Height);
                    }
                    else
                    {
                        if((this.pictureBitmap.Width != this.subtitlePictureBox.Width) || (this.pictureBitmap.Height != this.subtitlePictureBox.Height))
                        {
                            this.subtitlePictureBox.Image = null;
                            this.pictureBitmap.Dispose();
                            this.pictureBitmap = new Bitmap(this.subtitlePictureBox.Width, this.subtitlePictureBox.Height);
                        }
                    }
                    float scaleX = Convert.ToSingle(this.subtitlePictureBox.Width) / this.packlist.VideoAttributes.HorizontalResolution;
                    float scaleY = Convert.ToSingle(this.subtitlePictureBox.Height) / this.packlist.VideoAttributes.VerticalResolution;
                    using(Graphics g = Graphics.FromImage(this.pictureBitmap))
                    {
                        g.Clear(this.subtitlePictureBox.BackColor);
                        RectangleF r = new RectangleF(subtitleData.Origin.X * scaleX, subtitleData.Origin.Y * scaleY, 
                            subtitleData.Width * scaleX, subtitleData.Height * scaleY);
                        g.DrawImage(this.subtitle.Bitmap, r);
                    }
                    this.subtitlePictureBox.Image = this.pictureBitmap;
                }
                TimeSpan span = TimeSpan.FromMilliseconds(subtitleData.Pts);
                double duration = subtitleData.Duration;
                bool isMaxDuration = false;
                if(duration >= SubConstants.MaximumMillisecondsOnScreen)
                {
                    duration = Math.Min(duration, SubConstants.MaximumMillisecondsOnScreen);
                    isMaxDuration = true;
                }
                if(this.selectedSubIndex < this.packlist.Subtitles.Count - 1)
                {
                    double nextPts = this.packlist.Subtitles[this.selectedSubIndex + 1].Pts;
                    if(isMaxDuration)
                    {
                        duration = Math.Min(duration, nextPts - subtitleData.Pts);
                    }
                    else if(duration > 0)
                    {
                        if(Math.Abs(subtitleData.Pts + duration - nextPts) <= SubConstants.PtsSlushMilliseconds)
                        {
                            duration = nextPts - subtitleData.Pts;
                        }
                    }
                }

                TimeSpan spanDuration = TimeSpan.FromMilliseconds(duration);
                int index = this.packlist.IndexInStream(this.selectedSubIndex, this.forcedCheckBox.Checked) + 1;
                this.indexLabel.Text = string.Format(
                    "Time {0}:{1:d2}:{2:d2}.{3:d2} Len {4}.{5:d2}",
                    span.Hours, span.Minutes, span.Seconds, span.Milliseconds / 10,
                    spanDuration.Seconds, spanDuration.Milliseconds / 10);
                this.recursiveIndexUpDown = true;
                try
                {
                    int maxIndex = this.packlist.Count[subtitleData.StreamId];
                    if(this.indexUpDown.Maximum != maxIndex)
                    {
                        this.indexUpDown.Maximum = maxIndex;
                    }
                    this.indexUpDown.Value = index;
                    if(this.indexUpDown.Minimum != 1)
                    {
                        this.indexUpDown.Minimum = 1;
                    }
                    this.indexUpDown.Enabled = true;
                }
                finally
                {
                    this.recursiveIndexUpDown = false;
                }

                /*List<int> alphaColors = new List<int>();
                for(int index = 0; index < 4; index++)
                {
                    if(this.subtitle.Bitmap.Palette.Entries[index].A != 0)
                    {
                        alphaColors.Add(index);
                    }
                }

                ContiguousEncode encode = new ContiguousEncode(this.subtitle.Data,
                    this.subtitle.Bitmap.Width, this.subtitle.Bitmap.Height, 
                    this.subtitle.Stride, null);

                this.paletteRichTextBox.Clear();
                foreach(ContiguousEncode.RectangleWithColors rect in encode.FindSections(alphaColors))
                {
                    this.paletteRichTextBox.AppendText(rect.ToString());
                    this.paletteRichTextBox.AppendText("\n");
                }*/
            }
            else
            {
                this.subtitlePictureBox.Image = null;
                this.indexLabel.Text = "";
            }
            if(oldSubtitle != null)
            {
                oldSubtitle.Dispose();
            }
            this.data.IsCurrentStepComplete = this.IsComplete;
        }

        private void forcedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            subtitleListBox_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void scaleBitmapCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ScaleSubtitleBitmap = this.scaleBitmapCheckBox.Checked;
            Properties.Settings.Default.Save();

            if(this.packlist != null)
            {
                LoadCurrentSubtitleBitmap();
            }
        }
    }
}
