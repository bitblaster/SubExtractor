using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DvdNavigatorCrm;
using DvdSubOcr;

namespace DvdSubExtractor
{
    public partial class OptionsForm : Form
    {
        string fontFamilyName;
        bool fontBold;
        float fontSize;
        OcrMap ocrMap;
        List<string> allWords = new List<string>();

        public OptionsForm()
        {
            InitializeComponent();

            if(Directory.Exists(Properties.Settings.Default.OutputDirectory))
            {
                this.labelOutputDirectory.Text = Properties.Settings.Default.OutputDirectory;
            }
            else
            {
                string storageFile = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                this.labelOutputDirectory.Text = Path.Combine(storageFile, SubtitleStorage.DvdOcrSubdirectory);
            }

            this.minimumTrackLengthUpDown.Value = Convert.ToDecimal(Properties.Settings.Default.MinimumDvdTrackLength);
            this.sampleVideoLengthUpDown.Value = Convert.ToDecimal(Properties.Settings.Default.SampleVideoLength);

            if(File.Exists(Properties.Settings.Default.SampleVideoPlayerPath))
            {
                this.videoPlayerLabel.Text = Properties.Settings.Default.SampleVideoPlayerPath;
            }
            else
            {
                this.videoPlayerLabel.Text = "";
            }

            if(File.Exists(Properties.Settings.Default.SubtitleEditorPath))
            {
                this.subtitleEditorLabel.Text = Properties.Settings.Default.SubtitleEditorPath;
            }
            else
            {
                this.subtitleEditorLabel.Text = "";
            }

            if(File.Exists(Properties.Settings.Default.DgIndexPath))
            {
                this.dgIndexLabel.Text = Properties.Settings.Default.DgIndexPath;
            }
            else
            {
                this.dgIndexLabel.Text = "";
            }
            this.ocrDataFileTextBox.Text = DvdSubOcr.OcrMap.StorageFile;
            this.dataWithProgramCheckBox.Checked = Properties.Settings.Default.DataFileInExeDirectory;

            this.storeSupOutputInSourceDirCheckbox.Checked = Properties.Settings.Default.StoreSupTextInSourceDir;
            this.storeSrtAnsiCheckBox.Checked = Properties.Settings.Default.StoreSrtAsANSI;
            this.fontFamilyName = Properties.Settings.Default.SubitleFileFontName;
            this.fontBold = Properties.Settings.Default.SubtitleFontBold;
            this.fontSize = Properties.Settings.Default.SubtitleFontDefaultSize;
            UpdateFontName();
            this.fontAdjustmentUpDown.Value = Properties.Settings.Default.FontSizeAdjustmentPercent;
            this.fontOutlineUpDown.Value = Convert.ToDecimal(Properties.Settings.Default.SubtitleFontOutline);
            this.fontShadowUpDown.Value = Convert.ToDecimal(Properties.Settings.Default.SubtitleFontShadow);

            this.verticalMarginUpDown.Value = Convert.ToDecimal(Properties.Settings.Default.SubtitleVerticalMargin);
            this.horizontalMargin4x3UpDown.Value = Convert.ToDecimal(Properties.Settings.Default.SubtitleHorizontal4x3Margin);
            this.horizontalMargin16x9UpDown.Value = Convert.ToDecimal(Properties.Settings.Default.SubtitleHorizontal16x9Margin);

            switch(Properties.Settings.Default.SubtitleColorScheme)
            {
            case 1:
                this.dvdColorsButton.Checked = true;
                break;
            case 0:
            default:
                this.customColorsButton.Checked = true;
                break;
            }
            colorsButton_CheckedChanged(this, EventArgs.Empty);

            this.textColorLabel.ForeColor = Color.FromArgb(255, Properties.Settings.Default.SubtitleForeColor);
            this.textOpacityUpDown.Value = Convert.ToInt32(Properties.Settings.Default.SubtitleForeColor.A * 100.0 / 255.0);
            this.borderColorLabel.BackColor = Color.FromArgb(255, Properties.Settings.Default.SubtitleBorderColor);
            this.borderOpacityUpDown.Value = Convert.ToInt32(Properties.Settings.Default.SubtitleBorderColor.A * 100.0 / 255.0);
            this.shadingColorLabel.BackColor = Color.FromArgb(255, Properties.Settings.Default.SubtitleShadingColor);
            this.shadingOpacityUpDown.Value = Convert.ToInt32(Properties.Settings.Default.SubtitleShadingColor.A * 100.0 / 255.0);
            this.spanishSpecialCheckBox.Checked = Properties.Settings.Default.SpanishSpecialCharacters;
            this.frenchSpecialCheckBox.Checked = Properties.Settings.Default.FrenchSpecialCharacters;
        }

        public void ShowSubtitleTab()
        {
            this.tabControl1.SelectedIndex = 1;
        }

        public void ShowLandIWordTab(OcrMap ocrMap)
        {
            this.ocrMap = ocrMap;
            this.tabControl1.SelectedIndex = 2;
            InitializeIandLWords();
        }

        void UpdateFontName()
        {
            if(this.fontBold)
            {
                this.fontFamilyLabel.Text = string.Format("{0} {1:f0} (Bold)", this.fontFamilyName, this.fontSize);
            }
            else
            {
                this.fontFamilyLabel.Text = string.Format("{0} {1:f0}", this.fontFamilyName, this.fontSize);
            }
        }

        private void outputDirectoryButton_Click(object sender, EventArgs e)
        {
            this.folderBrowserDialog.SelectedPath = this.labelOutputDirectory.Text;
            if(this.folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                if(Directory.Exists(this.folderBrowserDialog.SelectedPath))
                {
                    this.labelOutputDirectory.Text = this.folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.OutputDirectory = this.labelOutputDirectory.Text;
            Properties.Settings.Default.StoreSupTextInSourceDir = this.storeSupOutputInSourceDirCheckbox.Checked;
            Properties.Settings.Default.MinimumDvdTrackLength = Convert.ToSingle(this.minimumTrackLengthUpDown.Value);
            Properties.Settings.Default.SampleVideoLength = Convert.ToInt32(this.sampleVideoLengthUpDown.Value);
            Properties.Settings.Default.SampleVideoPlayerPath = this.videoPlayerLabel.Text;
            Properties.Settings.Default.SubtitleEditorPath = this.subtitleEditorLabel.Text;
            Properties.Settings.Default.DgIndexPath = this.dgIndexLabel.Text;
            Properties.Settings.Default.StoreSrtAsANSI = this.storeSrtAnsiCheckBox.Checked;
            Properties.Settings.Default.SubitleFileFontName = this.fontFamilyName;
            Properties.Settings.Default.SubtitleFontBold = this.fontBold;
            Properties.Settings.Default.SubtitleFontDefaultSize = this.fontSize;
            Properties.Settings.Default.FontSizeAdjustmentPercent = Convert.ToInt32(this.fontAdjustmentUpDown.Value);
            Properties.Settings.Default.SubtitleFontOutline = Convert.ToSingle(this.fontOutlineUpDown.Value);
            Properties.Settings.Default.SubtitleFontShadow = Convert.ToSingle(this.fontShadowUpDown.Value);
            Properties.Settings.Default.SubtitleVerticalMargin = Convert.ToInt32(this.verticalMarginUpDown.Value);
            Properties.Settings.Default.SubtitleHorizontal4x3Margin = Convert.ToInt32(this.horizontalMargin4x3UpDown.Value);
            Properties.Settings.Default.SubtitleHorizontal16x9Margin = Convert.ToInt32(this.horizontalMargin16x9UpDown.Value);
            Properties.Settings.Default.SubtitleForeColor = Color.FromArgb(Convert.ToInt32((double)this.textOpacityUpDown.Value * 255.0 / 100.0), this.textColorLabel.ForeColor);
            Properties.Settings.Default.SubtitleBorderColor = Color.FromArgb(Convert.ToInt32((double)this.borderOpacityUpDown.Value * 255.0 / 100.0), this.borderColorLabel.BackColor);
            Properties.Settings.Default.SubtitleShadingColor = Color.FromArgb(Convert.ToInt32((double)this.shadingOpacityUpDown.Value * 255.0 / 100.0), this.shadingColorLabel.BackColor);
            if(this.dvdColorsButton.Checked)
            {
                Properties.Settings.Default.SubtitleColorScheme = 1;
            }
            else
            {
                Properties.Settings.Default.SubtitleColorScheme = 0;
            }

            if(this.dataWithProgramCheckBox.Checked != Properties.Settings.Default.DataFileInExeDirectory)
            {
                string oldLocation = OcrMap.StorageFilePath(Properties.Settings.Default.DataFileInExeDirectory);
                string newLocation = OcrMap.StorageFilePath(this.dataWithProgramCheckBox.Checked);

                bool canUseNewLocation = false;
                bool newFileExists = File.Exists(newLocation);
                bool oldFileExists = File.Exists(oldLocation);
                if(oldFileExists && (!newFileExists || (File.GetLastWriteTime(oldLocation) > File.GetLastWriteTime(newLocation))))
                {
                    try
                    {
                        if(newFileExists)
                        {
                            File.Delete(newLocation);
                        }
                        File.Copy(oldLocation, newLocation);
                        canUseNewLocation = true;
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Unable to copy OcrMap.bin file to new location: " + ex.Message);
                    }
                }
                else
                {
                    if(!newFileExists)
                    {
                        try
                        {
                            using(FileStream fs = File.Create(newLocation))
                            {
                            }
                            File.Delete(newLocation);
                            canUseNewLocation = true;
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show("Unable to access OcrMap.bin file in new location: " + ex.Message);
                        }
                    }
                    else
                    {
                        canUseNewLocation = true;
                    }
                }
                if(canUseNewLocation)
                {
                    if(oldFileExists)
                    {
                        try
                        {
                            File.Delete(oldLocation);
                        }
                        catch(Exception)
                        {
                        }
                    }
                    Properties.Settings.Default.DataFileInExeDirectory = this.dataWithProgramCheckBox.Checked;
                    OcrMap.UseProgramExeForStorage = this.dataWithProgramCheckBox.Checked;
                }
            }

            Properties.Settings.Default.SpanishSpecialCharacters = this.spanishSpecialCheckBox.Checked;
            OcrMap.UseSpanishSpecialChars = this.spanishSpecialCheckBox.Checked;
            Properties.Settings.Default.FrenchSpecialCharacters = this.frenchSpecialCheckBox.Checked;
            SubtitleLine.CharactersNeverAfterASpace = Properties.Settings.Default.FrenchSpecialCharacters ?
                SubConstants.CharactersNeverAfterASpaceFrench : SubConstants.CharactersNeverAfterASpace;

            Properties.Settings.Default.Save();

            if(this.ocrMap != null)
            {
                this.ocrMap.Save();
            }
        }

        private void videoPlayerButton_Click(object sender, EventArgs e)
        {
            if(File.Exists(this.videoPlayerLabel.Text))
            {
                this.openFileDialog.InitialDirectory = Path.GetDirectoryName(this.videoPlayerLabel.Text);
            }
            else
            {
                this.openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }
            this.openFileDialog.Filter = "Programs (*.exe)|*.exe|All files (*.*)|*.*";

            if(this.openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                if(File.Exists(this.openFileDialog.FileName))
                {
                    this.videoPlayerLabel.Text = this.openFileDialog.FileName;
                }
            }
        }

        const int TraceMpegLength = 1000000;

        static StreamWriter traceFileWriter;

        public static void TraceMpegFile(IWin32Window window)
        {
            using(OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Mpeg Files (*.mpg)|*.mpg|All files (*.*)|*.*";
                if(openFileDialog.ShowDialog(window) == DialogResult.OK)
                {
                    //int fileLength = TraceMpegLength;
                    int fileLength = Convert.ToInt32(Math.Min((long)Int32.MaxValue,
                        new FileInfo(openFileDialog.FileName).Length));
                    TitleChunk chunk = new TitleChunk(openFileDialog.FileName, 0,
                        fileLength, 0, false, new int[16], 0);
                    CellLoader loader = new CellLoader(null, null, null);
                    string traceFilePath = Path.Combine(Properties.Settings.Default.OutputDirectory,
                       Path.GetFileNameWithoutExtension(openFileDialog.FileName) + ".Trace.txt");
                    traceFileWriter = File.CreateText(traceFilePath);
                    loader.SplitterTrace += loader_SplitterTrace;
                    loader.Run(new TitleChunk[] { chunk }, delegate() { return false; });
                    loader.SplitterTrace -= loader_SplitterTrace;
                    traceFileWriter.Close();
                    traceFileWriter = null;
                    loader.ClearPackets();
                }
            }
        }

        static void loader_SplitterTrace(object sender, TraceEventArgs e)
        {
            if(traceFileWriter != null)
            {
                traceFileWriter.WriteLine(e.Text);
            }
        }

        public static bool DoesDGIndexExist
        {
            get
            {
                if(!File.Exists(Properties.Settings.Default.DgIndexPath))
                {
                    MessageBox.Show("DgIndex is not configured.  Please open the Options dialog and set it.");
                    return false;
                }
                return true;
            }
        }

        public static bool DoesVideoPlayerExist
        {
            get
            {
                if(!File.Exists(Properties.Settings.Default.SampleVideoPlayerPath))
                {
                    MessageBox.Show("Video Player is not configured.  Please open the Options dialog and set it.");
                    return false;
                }
                return true;
            }
        }

        public static bool DoesOutputPathExist
        {
            get
            {
                if(!Directory.Exists(Properties.Settings.Default.OutputDirectory))
                {
                    MessageBox.Show("Output Path does not exist.  Please open the Options dialog and set it.");
                    return false;
                }
                return true;
            }
        }

        private void subtitleEditorButton_Click(object sender, EventArgs e)
        {
            if(File.Exists(this.subtitleEditorLabel.Text))
            {
                this.openFileDialog.InitialDirectory = Path.GetDirectoryName(this.subtitleEditorLabel.Text);
            }
            else
            {
                this.openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }
            this.openFileDialog.Filter = "Programs (*.exe)|*.exe|All files (*.*)|*.*";

            if(this.openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                if(File.Exists(this.openFileDialog.FileName))
                {
                    this.subtitleEditorLabel.Text = this.openFileDialog.FileName;
                }
            }
        }

        private void dgIndexButton_Click(object sender, EventArgs e)
        {
            if(File.Exists(this.dgIndexLabel.Text))
            {
                this.openFileDialog.InitialDirectory = Path.GetDirectoryName(this.dgIndexLabel.Text);
            }
            else
            {
                this.openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }
            this.openFileDialog.Filter = "Programs (*.exe)|*.exe|All files (*.*)|*.*";

            if(this.openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                if(File.Exists(this.openFileDialog.FileName))
                {
                    this.dgIndexLabel.Text = this.openFileDialog.FileName;
                }
            }
        }

        bool IsLoadStopped()
        {
            return false;
        }

        private void fontFamilyButton_Click(object sender, EventArgs e)
        {
            this.fontDialog1.Font = new Font(this.fontFamilyName, this.fontSize, this.fontBold ? FontStyle.Bold : FontStyle.Regular);
            if(this.fontDialog1.ShowDialog(this) == DialogResult.OK)
            {
                this.fontFamilyName = this.fontDialog1.Font.Name;
                this.fontBold = this.fontDialog1.Font.Bold;
                this.fontSize = this.fontDialog1.Font.Size;
                UpdateFontName();
            }
        }

        private void defaultsButton_Click(object sender, EventArgs e)
        {
            this.fontFamilyName = "Tahoma";
            this.fontBold = false;
            this.fontSize = 32.0f;
            UpdateFontName();
            this.fontAdjustmentUpDown.Value = 0;
            this.fontOutlineUpDown.Value = Convert.ToDecimal(1.4);
            this.fontShadowUpDown.Value = Convert.ToDecimal(1.7);
            this.dvdColorsButton.Checked = true;
            this.verticalMarginUpDown.Value = 15;
            this.horizontalMargin4x3UpDown.Value = 20;
            this.horizontalMargin16x9UpDown.Value = 80;
            this.textColorLabel.ForeColor = Color.FromArgb(255, 228, 223, 226);
            this.textOpacityUpDown.Value = 100;
            this.borderColorLabel.BackColor = Color.Black;
            this.borderOpacityUpDown.Value = 88;
            this.shadingColorLabel.BackColor = Color.Black;
            this.shadingOpacityUpDown.Value = 22;
        }

        private void textColorButton_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = this.textColorLabel.ForeColor;
            if(colorDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                this.textColorLabel.ForeColor = colorDialog1.Color;
            }
        }

        private void borderColorButton_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = this.borderColorLabel.BackColor;
            if(colorDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                this.borderColorLabel.BackColor = colorDialog1.Color;
            }
        }

        private void shadingColorButton_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = this.shadingColorLabel.BackColor;
            if(colorDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                this.shadingColorLabel.BackColor = colorDialog1.Color;
            }
        }

        void colorsButton_CheckedChanged(object sender, EventArgs e)
        {
            if(this.dvdColorsButton.Checked)
            {
                this.textColorLabel.Enabled = false;
                this.textColorButton.Enabled = false;
                this.textColorOpacityLabel.Enabled = false;
                this.textOpacityUpDown.Enabled = false;
            }
            else
            {
                this.textColorLabel.Enabled = true;
                this.textColorButton.Enabled = true;
                this.textColorOpacityLabel.Enabled = true;
                this.textOpacityUpDown.Enabled = true;
            }
        }

        private void filterLandITextBox_TextChanged(object sender, EventArgs e)
        {
            LoadLandIList();
        }

        void LoadLandIList()
        {
            this.lAndIListBox.Items.Clear();
            string filter = this.filterLandITextBox.Text.Trim();
            if(String.IsNullOrWhiteSpace(filter))
            {
                foreach(string s in this.allWords)
                {
                    this.lAndIListBox.Items.Add(s);
                }
            }
            else
            {
                foreach(string s in this.allWords.Where(s => s.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase)))
                {
                    this.lAndIListBox.Items.Add(s);
                }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tabControl1.SelectedIndex == 2)
            {
                InitializeIandLWords();
            }
        }

        void InitializeIandLWords()
        {
            if(this.ocrMap == null)
            {
                this.ocrMap = new OcrMap();
                try
                {
                    this.ocrMap.Load();
                }
                catch(Exception)
                {
                    MessageBox.Show("OCR Map failed to load - probably out of date");
                }
            }

            if(this.allWords.Count == 0)
            {
                this.allWords.AddRange(this.ocrMap.SpellingWords);
                this.allWords.Sort(StringComparer.CurrentCultureIgnoreCase);
            }

            if(this.lAndIListBox.Items.Count == 0)
            {
                LoadLandIList();
            }
        }

        private void removeFromIandListButton_Click(object sender, EventArgs e)
        {
            if((this.ocrMap != null) && (this.lAndIListBox.Items.Count > 0) && (this.lAndIListBox.SelectedIndex >= 0))
            {
                string removedWord = this.lAndIListBox.SelectedItem as string;
                this.ocrMap.RemoveLandIWord(removedWord);
                this.lAndIListBox.Items.Remove(removedWord);
            }
        }

        private void dataWithProgramCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.ocrDataFileTextBox.Text = OcrMap.StorageFilePath(this.dataWithProgramCheckBox.Checked);
        }

        private void clearlAndIListButton_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Are you sure you want to remove all the l and I spelling words in the database?", "Clear List", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                this.ocrMap.ClearLandIWords();
                this.lAndIListBox.Items.Clear();
            }
        }
    }
}
