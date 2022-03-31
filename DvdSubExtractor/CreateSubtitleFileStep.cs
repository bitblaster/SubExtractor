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
    public partial class CreateSubtitleFileStep : UserControl, IWizardItem
    {
        ExtractData data;
        string nextSubtitleBinaryFile;
        SortedDictionary<double, double> ptsOffsets = new SortedDictionary<double, double>();
        static string initialDirectory;
        string lastSavedFile;
        List<Control> disabledSrtOptions = new List<Control>();

        public CreateSubtitleFileStep()
        {
            InitializeComponent();

            this.disabledSrtOptions.AddRange(new Control[] { this.leftCropLabel, this.leftCropUpDown, this.topCropLabel,
                this.topCropUpDown, this._1080pCheckBox, this.lineBreaksGroupBox });
        }

        public void Initialize(ExtractData data)
        {
            this.data = data;
            this.dvdLabel.Text = CalculateSubFilename();

            switch(Properties.Settings.Default.SubtitleStyle)
            {
            case 0:
                this.srtRadioButton.Checked = true;
                break;
            case 1:
            default:
                this.assRadioButton.Checked = true;
                break;
            }
            this.subtitleStyle_SelectedIndexChanged(this, EventArgs.Empty);

            switch(Properties.Settings.Default.PositionSubs)
            {
            case 0:
            default:
                this.normalLinesButton.Checked = true;
                break;
            case 1:
                this.dvdLineBreaksButton.Checked = true;
                break;
            case 2:
                this.exactPositionButton.Checked = true;
                break;
            }

            this.removeSdhSubsCheck.Checked = Properties.Settings.Default.RemoveSDHSet;

            bool selectedMatch = false;
            string matchFilePath = this.data.SelectedSubtitleBinaryFile ?? "";
            this.nextTitleButton.Enabled = false;
            foreach(DvdTrackItem item in this.data.Programs)
            {
                string filePath = Path.Combine(CalculateOutputDirectory(), this.data.ComputeSubtitleDataFileName(item));
                if(File.Exists(filePath))
                {
                    if(selectedMatch)
                    {
                        this.nextSubtitleBinaryFile = filePath;
                        this.nextTitleButton.Enabled = true;
                        break;
                    }
                    if(string.Compare(filePath, matchFilePath, true) == 0)
                    {
                        selectedMatch = true;
                    }
                }
            }
            if(!selectedMatch && (this.data.SelectedSubtitleFiles != null))
            {
                foreach(string subtitlePath in this.data.SelectedSubtitleFiles)
                {
                    if(File.Exists(subtitlePath))
                    {
                        if(selectedMatch)
                        {
                            this.nextSubtitleBinaryFile = subtitlePath;
                            this.nextTitleButton.Enabled = true;
                            break;
                        }
                        if(string.Compare(subtitlePath, matchFilePath, true) == 0)
                        {
                            selectedMatch = true;
                        }
                    }
                }
            }

            this.data.NewStepInitialize(false, true, this.HelpText,
                new Type[] { typeof(LoadFolderStep), typeof(ChooseSubtitlesStep) });

            SortedDictionary<long, double> posPtsMap = ParseD2v();

            if(this.data.WorkingData.CellStarts.Count != 0)
            {
                int subDataIndex = 0;
                double? previousBinPts = null;
                double? previousD2vPts = null;
                foreach(CellStartInfo startInfo in this.data.WorkingData.CellStarts)
                {
                    bool isFirstCell = ((startInfo.CellType & SaverCellType.First) == SaverCellType.First);
                    double audio = startInfo.FirstAudioPts - startInfo.FirstCellPts;
                    double video = startInfo.FirstVideoPts - startInfo.FirstCellPts;

                    double d2vVsBinPts = 0.0;
                    double binPts = startInfo.PtsOffset + startInfo.FirstVideoPts;
                    if(posPtsMap.Count != 0)
                    {
                        double? foundD2vPts = null;
                        foreach(KeyValuePair<long, double> ptsEntry in posPtsMap)
                        {
                            if(ptsEntry.Key >= startInfo.FilePosition)
                            {
                                foundD2vPts = ptsEntry.Value;
                                break;
                            }
                        }

                        if(foundD2vPts.HasValue)
                        {
                            if(previousD2vPts.HasValue)
                            {
                                d2vVsBinPts = (foundD2vPts.Value - previousD2vPts.Value) - (binPts - previousBinPts.Value);
                            }
                            previousD2vPts = foundD2vPts;
                        }
                        else
                        {
                            previousD2vPts = null;
                        }

                        if(previousBinPts.HasValue)
                        {
                            this.ptsOffsets[startInfo.PtsOffset + startInfo.FirstCellPts] = d2vVsBinPts;
                        }
                        else
                        {
                            d2vVsBinPts = -startInfo.PtsOffset - startInfo.FirstVideoPts;
                            this.ptsOffsets[startInfo.PtsOffset + startInfo.FirstCellPts] = d2vVsBinPts;
                        }
                    }
                    else
                    {
                        if(isFirstCell)
                        {
                            if(previousBinPts.HasValue)
                            {
                                this.ptsOffsets[startInfo.PtsOffset + startInfo.FirstCellPts] = Math.Min(42.0 - startInfo.MinimumAudioVideo, 0.0);
                            }
                            else
                            {
                                this.ptsOffsets[startInfo.PtsOffset + startInfo.FirstCellPts] = -startInfo.PtsOffset - startInfo.FirstVideoPts;
                            }
                        }
                    }
                    previousBinPts = binPts;

                    if(isFirstCell)
                    {
                        TimeSpan offsetTime = new TimeSpan(Convert.ToInt64(startInfo.PtsOffset + startInfo.FirstCellPts) * 10000);
                        this.sectionListTextBox.AppendText(
                            string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0}: Cell {1} Pos {2} Audio {3:f0} Video {4:f0} Offset {5}:{6:d2}:{7:d2} D2V {8:f0}\r\n",
                            ++subDataIndex, startInfo.CellType, startInfo.FilePosition,
                            audio, video, offsetTime.Hours, offsetTime.Minutes, offsetTime.Seconds, d2vVsBinPts));
                    }
                    else
                    {
                        this.sectionListTextBox.AppendText(
                            string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0}: Cell {1} Pos {2} Audio {3:f0} Video {4:f0} D2V {5:f0}\r\n",
                            ++subDataIndex, startInfo.CellType, startInfo.FilePosition, audio, video, d2vVsBinPts));
                    }
                }
            }
            else
            {
                this.sectionListLabel.Visible = false;
                this.sectionListTextBox.Visible = false;
            }

            if(this.data.WorkingData.VideoAttributes.VerticalResolution >= 1000)
            {
                this._1080pCheckBox.Visible = true;
                this._1080pCheckBox.Checked = true;
            }
            else
            {
                this._1080pCheckBox.Visible = false;
            }
        }

        public void Terminate()
        {
        }

        string HelpText
        {
            get
            {
                return "Create a text subtitle file in any of 3 formats in this step. SRT files " +
                    "are simple subtitles without any style other than italics - the text " +
                    "goes at the bottom of the screen in the font you've configured your " +
                    "player with.  Ssa and Ass (Sub Station Alpha and Advanced Sub Station Alpha) " +
                    "files have font and positioning features.  This program creates a " +
                    "style common to many fansubbing groups but the normal editor for " +
                    "such files, AegiSub, can be used to further edit them after they are " +
                    "created here." + 
                    "\n\nIf you are going to crop the video, say 8 pixels off the top and 20 off the left, " +
                    "you should set the Left offset to -8 and Top offset to -20 so the subtitles are still aligned." +
                    "\n\nIf the style of these subtitles is \"Thought Bubbles\" where the text moves with the speaker " + 
                    "as is common in closed captions, be sure to check Exactly Position Every Line." +
                    "\n\nFeature \"Remove SDH Subs\" attempts to removed text that indicates sounds or speakers, typically added " +
                    "for the hearing impaired. SDH text is typically surrounded by parentheses: () or []";
            }
        }

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

        SortedDictionary<long, double> ParseD2v()
        {
            SortedDictionary<long, double> positionPts = new SortedDictionary<long, double>();

            string fileName = Path.Combine(Path.GetDirectoryName(this.data.SelectedSubtitleBinaryFile), 
                Path.GetFileNameWithoutExtension(this.data.SelectedSubtitleBinaryFile) + ".d2v");
            if(File.Exists(fileName))
            {
                bool beforeHeader = true;
                bool afterHeader = false;
                string[] d2vLines = File.ReadAllLines(fileName);
                double pts = 0.0;
                double framePts = 1000.0 / 29.97;
                double repeatFieldFramePts = framePts * 1.5;
                foreach(string line in d2vLines)
                {
                    string trimmed = line.Trim();
                    if(trimmed.Length == 0)
                    {
                        continue;
                    }

                    if(beforeHeader)
                    {
                        if(trimmed.IndexOf('=') != -1)
                        {
                            beforeHeader = false;
                        }
                        continue;
                    }
                    if(!afterHeader)
                    {
                        if(trimmed.IndexOf('=') != -1)
                        {
                            if(trimmed.Substring(0, trimmed.IndexOf('=')).Trim() == "Frame_Rate")
                            {
                                string rate = trimmed.Substring(trimmed.IndexOf('=') + 1);
                                if(rate.IndexOf('(') != -1)
                                {
                                    rate = rate.Substring(0, rate.IndexOf('(')).Trim();
                                }
                                double fps = Convert.ToDouble(Int32.Parse(rate)) / 1000.0;
                                if(fps == 25.0)
                                {
                                    framePts = 1000.0 / fps;
                                    repeatFieldFramePts = framePts * 1.5;
                                }
                            }
                            continue;
                        }
                        afterHeader = true;
                    }

                    try
                    {
                        string[] words = trimmed.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        UInt16 info = UInt16.Parse(words[0], NumberStyles.HexNumber);
                        long position = long.Parse(words[3]);
                        positionPts[position] = pts;
                        bool endOfStreamFound = false;
                        for(int flagIndex = 7; flagIndex < words.Length; flagIndex++)
                        {
                            UInt16 flags = UInt16.Parse(words[flagIndex], NumberStyles.HexNumber);
                            if(flags == 0xff)
                            {
                                endOfStreamFound = true;
                                break;
                            }
                            if((flags & 1) != 0)
                            {
                                pts += repeatFieldFramePts;
                            }
                            else
                            {
                                pts += framePts;
                            }
                        }
                        if(endOfStreamFound)
                        {
                            break;
                        }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("D2V parse exception " + ex.Message);
                        positionPts.Clear();
                        break;
                    }
                }
            }

            return positionPts;
        }

        string CalculateOutputDirectory()
        {
            if(Properties.Settings.Default.StoreSupTextInSourceDir)
            {
                string binaryFileExt = Path.GetExtension(this.data.SelectedSubtitleBinaryFile).ToLowerInvariant();
                if((binaryFileExt == ".sup") || (binaryFileExt == ".idx"))
                {
                    return Path.GetDirectoryName(this.data.SelectedSubtitleBinaryFile);
                }
            }
            return Properties.Settings.Default.OutputDirectory;
        }

        string CalculateSubFilename()
        {
            if(Path.GetExtension(this.data.SelectedSubtitleBinaryFile).ToLowerInvariant() == ".sup")
            {
                return Path.GetFileNameWithoutExtension(this.data.SelectedSubtitleBinaryFile);
            }
            else
            {
                return string.Format("{0} T{1:x} {2}",
                    Path.GetFileNameWithoutExtension(this.data.SelectedSubtitleBinaryFile),
                    this.data.SelectedSubtitleStreamId, this.data.SelectedSubtitleDescription);
            }
        }

        private void CreateSubtitle(string fileName, string outputDirectory)
        {
            CreateSubOptions options = new CreateSubOptions()
            {
                Adjust25to24 = this.slow2524CheckBox.Visible && this.slow2524CheckBox.Checked,
                Crop = new Point(Convert.ToInt32(this.leftCropUpDown.Value), Convert.ToInt32(this.topCropUpDown.Value)),
                FileName = fileName,
                Is1080p = this._1080pCheckBox.Visible && this._1080pCheckBox.Checked,
                OutputDirectory = outputDirectory,
                OverallPtsAdjustment = Convert.ToInt32(this.initialAdjustmentUpDown.Value),
                PositionAllSubs = this.exactPositionButton.Checked ? LineBreaksAndPositions.KeepBreaksAndPositions :
                    (this.dvdLineBreaksButton.Checked ? LineBreaksAndPositions.KeepBreaks : LineBreaksAndPositions.Normal),
                RemoveSDH = (this.removeSdhSubsCheck.Visible && this.removeSdhSubsCheck.Checked) ? RemoveSDH.Normal : RemoveSDH.None,
            };

            if(this.srtRadioButton.Checked)
            {
                this.saveProgressBar.Value = 0;
                this.saveProgressBar.Maximum = 100;
                this.saveProgressBar.Show();
                Application.DoEvents();
                if(CreateSrtFile.Create(this.data, this.ptsOffsets, options))
                {
                    this.saveProgressBar.Value = 100;
                    //MessageBox.Show(this, "SubRip SRT File Created");
                }
            }
            else if(this.assRadioButton.Checked)
            {
                this.saveProgressBar.Value = 0;
                this.saveProgressBar.Maximum = 100;
                this.saveProgressBar.Show();
                Application.DoEvents();
                if(CreateAssFile.Create(this.data, this.ptsOffsets, options))
                {
                    this.saveProgressBar.Value = 100;
                    //MessageBox.Show(this, "Advanced Substation Alpha Subtitle File Created");
                }
            }
        }

        private void saveAsButton_Click(object sender, EventArgs e)
        {
            string fileName = this.SubtitleFileName;
            this.saveAsFileDialog.DefaultExt = Path.GetExtension(fileName).Substring(1);
            this.saveAsFileDialog.Filter = string.Format("Subtitle files|*.{0}|All files|*.*", this.saveAsFileDialog.DefaultExt);
            this.saveAsFileDialog.FileName = fileName;
            if(String.IsNullOrWhiteSpace(initialDirectory))
            {
                initialDirectory = CalculateOutputDirectory();
            }
            this.saveAsFileDialog.InitialDirectory = initialDirectory;
            if(this.saveAsFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                this.lastSavedFile = null;
                string fileFullPath = this.saveAsFileDialog.FileName;
                CreateSubtitle(Path.GetFileName(fileFullPath), Path.GetDirectoryName(fileFullPath));
                initialDirectory = Path.GetDirectoryName(fileFullPath);
                if(File.Exists(fileFullPath))
                {
                    this.lastSavedFile = fileFullPath;
                    if(File.Exists(Properties.Settings.Default.SubtitleEditorPath))
                    {
                        this.srtEditButton.Enabled = true;
                    }
                    else
                    {
                        this.srtEditButton.Enabled = false;
                    }
                }
            }
        }

        private void createSubtitleFileButton_Click(object sender, EventArgs e)
        {
            this.lastSavedFile = null;
            string fileName = this.SubtitleFileName;
            string outputDirectory = CalculateOutputDirectory();
            CreateSubtitle(fileName, outputDirectory);

            string fullPath = Path.Combine(outputDirectory, fileName);
            if(File.Exists(fullPath))
            {
                this.subtitleFileLabel.Text = "(Overwrite) " + fileName;
                if(File.Exists(Properties.Settings.Default.SubtitleEditorPath))
                {
                    this.srtEditButton.Enabled = true;
                }
                else
                {
                    this.srtEditButton.Enabled = false;
                }
            }
        }

        string SubtitleFileName
        {
            get
            {
                string suffix;
                if(this.srtRadioButton.Checked)
                {
                    suffix = ".srt";
                }
                else
                {
                    suffix = ".ass";
                }
                return CalculateSubFilename() + suffix;
            }
        }

        private void subtitleStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.lastSavedFile = null;

            this.saveProgressBar.Hide();
            this.saveProgressBar.Value = 0;
            string fileName = this.SubtitleFileName;
            string fullPath = Path.Combine(CalculateOutputDirectory(), fileName);
            if(File.Exists(fullPath))
            {
                this.subtitleFileLabel.Text = "(Overwrite) " + fileName;
            }
            else
            {
                this.subtitleFileLabel.Text = "(Create) " + fileName;
            }

            Properties.Settings.Default.SubtitleStyle = this.srtRadioButton.Checked ? 0 : 1;
            Properties.Settings.Default.Save();

            if(this.srtRadioButton.Checked)
            {
                this.disabledSrtOptions.ForEach(c => c.Enabled = false);
            }
            else
            {
                this.disabledSrtOptions.ForEach(c => c.Enabled = true);
            }

            if(File.Exists(Properties.Settings.Default.SubtitleEditorPath) && File.Exists(fullPath))
            {
                this.srtEditButton.Enabled = true;
            }
            else
            {
                this.srtEditButton.Enabled = false;
            }
        }

        private void openSubtitleFileInEditorButton_Click(object sender, EventArgs e)
        {
            string subEditorFile = Properties.Settings.Default.SubtitleEditorPath;
            if(!string.IsNullOrEmpty(subEditorFile) && File.Exists(subEditorFile))
            {
                string fullPath;
                if(String.IsNullOrEmpty(this.lastSavedFile))
                {
                    fullPath = Path.Combine(CalculateOutputDirectory(), this.SubtitleFileName);
                }
                else
                {
                    fullPath = this.lastSavedFile;
                }
                Process.Start(subEditorFile, "\"" + fullPath + "\"");
            }
        }

        private void nextTitleButton_Click(object sender, EventArgs e)
        {
            this.data.SelectedSubtitleBinaryFile = this.nextSubtitleBinaryFile;
            this.data.SelectedSubtitleStreamId = -1;
            this.data.OnJumpTo(this, typeof(ChooseSubtitlesStep));
        }

        private void wordSpacingButton_Click(object sender, EventArgs e)
        {
            this.data.OnJumpTo(this, typeof(WordSpacingStep));
        }

        private void removeSdhSubsCheck_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.RemoveSDHSet = this.removeSdhSubsCheck.Checked;
            Properties.Settings.Default.Save();
        }

        private void normalLinesButton_CheckedChanged(object sender, EventArgs e)
        {
            if(this.normalLinesButton.Checked)
            {
                Properties.Settings.Default.PositionSubs = 0;
                Properties.Settings.Default.Save();
            }
        }

        private void dvdLineBreaksButton_CheckedChanged(object sender, EventArgs e)
        {
            if(this.dvdLineBreaksButton.Checked)
            {
                Properties.Settings.Default.PositionSubs = 1;
                Properties.Settings.Default.Save();
            }
        }

        private void exactPositionButton_CheckedChanged(object sender, EventArgs e)
        {
            if(this.exactPositionButton.Checked)
            {
                Properties.Settings.Default.PositionSubs = 2;
                Properties.Settings.Default.Save();
            }
        }
    }
}
