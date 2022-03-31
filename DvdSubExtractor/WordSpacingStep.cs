using System;
using System.Collections.Generic;
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
    public partial class WordSpacingStep : UserControl, IWizardItem
    {
        ExtractData data;
        Font listBoxNormalFont;
        Font listBoxItalicFont;
        bool updatingUpDowns;
        OcrMap ocrMap = new OcrMap();

        public WordSpacingStep()
        {
            InitializeComponent();

            this.listBoxNormalFont = new Font(this.spacingListBox.Font, FontStyle.Regular | FontStyle.Bold);
            this.listBoxItalicFont = new Font(this.spacingListBox.Font, FontStyle.Italic | FontStyle.Bold);
        }

        public void Initialize(ExtractData data)
        {
            try
            {
                this.ocrMap.Load();
            }
            catch(Exception)
            {
                MessageBox.Show("OCR Map failed to load - probably out of date");
                this.ocrMap = new OcrMap();
            }

            this.data = data;
            this.dvdLabel.Text = Path.GetFileNameWithoutExtension(this.data.SelectedSubtitleBinaryFile);

            this.data.NewStepInitialize(true, true, this.HelpText,
                new Type[] { typeof(LoadFolderStep), typeof(ChooseSubtitlesStep) });

            if((this.data != null) || (this.data.WorkingData != null) || (this.data.WorkingData.AllLines != null))
            {
                SortedSet<char> kerningChars = new SortedSet<char>();
                foreach(SubtitleLine line in this.data.WorkingData.AllLines)
                {
                    foreach(OcrCharacter ocr in line.Text)
                    {
                        if((ocr.Value != ' ') && !kerningChars.Contains(ocr.Value))
                        {
                            kerningChars.Add(ocr.Value);
                        }
                    }
                }
                this.kerningCharacterComboBox.Items.AddRange(kerningChars.Cast<object>().ToArray());
            }
            if(this.kerningCharacterComboBox.Items.Count == 0)
            {
                this.kerningCharacterComboBox.Items.AddRange(String.Concat(CharacterSelector.AllCharacters).Cast<object>().ToArray());
            }
            this.kerningCharacterComboBox.SelectedIndex = 0;
        }

        public void Terminate()
        {
        }

        string HelpText
        {
            get
            {
                return "This program attempts to find the breaks between words by looking at the separation between the letters, " + 
                    "but because of the shape of certain letters in various DVD Subtitle fonts there often needs to be extra adjustments or spaces " + 
                    "will be added or missed. This dialog allows you to temporarily (until the program is closed) change these " +
                    "adjustments so that there are fewer spelling errors in your subtitle file. For example, if you used " + 
                    "Spell Checking on your subtitle (.ASS) file and " +
                    "found that letters ending in italic k are repeatedly being combined with the next word " + 
                    "(a space wasn't inserted properly \"hackcomputer\" or \"brickbuilding\" ) " + 
                    "you would select the Character k, click the Italics radio button, then increase the Right adjustment value by 1. " + 
                    "Hopefully that will be enough to insert spaces after words ending in k correctly, but not so much that words with a k " + 
                    "in the middle are split in 2.";
            }
        }

        private void kerningChoice_Changed(object sender, EventArgs e)
        {
            Char selectedChar;
            this.updatingUpDowns = true;
            try
            {
                this.kerningLeftUpDown.Enabled = this.normalRadioButton.Checked;
                this.kerningRightUpDown.Enabled = this.normalRadioButton.Checked;
                this.kerningItalicLeftUpDown.Enabled = this.italicRadioButton.Checked;
                this.kerningItalicRightUpDown.Enabled = this.italicRadioButton.Checked;

                selectedChar = (char)this.kerningCharacterComboBox.SelectedItem;
                int value = 0;
                FontKerning.LeftKerning.TryGetValue(selectedChar, out value);
                this.kerningLeftUpDown.Value = value;
                value = 0;
                FontKerning.RightKerning.TryGetValue(selectedChar, out value);
                this.kerningRightUpDown.Value = value;
                value = 0;
                FontKerning.LeftItalicKerning.TryGetValue(selectedChar, out value);
                this.kerningItalicLeftUpDown.Value = value;
                value = 0;
                FontKerning.RightItalicKerning.TryGetValue(selectedChar, out value);
                this.kerningItalicRightUpDown.Value = value;

                value = FontKerning.GetDefaultLeftKerning(false, selectedChar);
                this.defaultLeftLabel.Text = value.ToString();
                value = FontKerning.GetDefaultLeftKerning(true, selectedChar);
                this.defaultItalicLeftLabel.Text = value.ToString();
                value = FontKerning.GetDefaultRightKerning(false, selectedChar);
                this.defaultRightLabel.Text = value.ToString();
                value = FontKerning.GetDefaultRightKerning(true, selectedChar);
                this.defaultItalicRightLabel.Text = value.ToString();
            }
            finally
            {
                this.updatingUpDowns = false;
            }

            FillListWithSpellings(selectedChar, this.italicRadioButton.Checked);
        }

        class SpacingItem
        {
            public SpacingItem(string text, int subtitleIndex)
            {
                this.Text = text;
                this.SubtitleIndex = subtitleIndex;
            }

            public string Text { get; private set; }
            public int SubtitleIndex { get; private set; }

            public override string ToString()
            {
                return this.Text;
            }
        }

        void FillListWithSpellings(char c, bool italic)
        {
            this.spacingListBox.Items.Clear();
            HashSet<string> usedStrings = new HashSet<string>();
            Font correctFont = italic ? this.listBoxItalicFont : this.listBoxNormalFont;
            if(!object.ReferenceEquals(this.spacingListBox.Font, correctFont))
            {
                this.spacingListBox.Font = correctFont;
            }

            const int SubtextWidth = 16;

            int subCount = this.data.WorkingData.AllLinesBySubtitle.Count;
            for(int subIndex = 0; subIndex < subCount; subIndex++)
            {
                foreach(SubtitleLine line in this.data.WorkingData.AllLinesBySubtitle[subIndex])
                {
                    for(int index = 0; index < line.Text.Count; index++)
                    {
                        OcrCharacter ocr = line.Text[index];
                        if((ocr.Value == c) && (ocr.Italic == italic))
                        {
                            int firstCharacter = index;
                            while((firstCharacter > 0) && (index - firstCharacter < SubtextWidth))
                            {
                                OcrCharacter prevOcr = line.Text[firstCharacter - 1];
                                if((prevOcr.Italic != italic) && Char.IsLetterOrDigit(prevOcr.Value))
                                {
                                    break;
                                }
                                firstCharacter--;
                            }
                            int lastCharacter = index;
                            while((lastCharacter < line.Text.Count - 1) && (lastCharacter - index < SubtextWidth))
                            {
                                OcrCharacter nextOcr = line.Text[lastCharacter + 1];
                                if((nextOcr.Italic != italic) && Char.IsLetterOrDigit(nextOcr.Value))
                                {
                                    break;
                                }
                                lastCharacter++;
                            }

                            if((firstCharacter < index) || (lastCharacter > index))
                            {
                                IEnumerable<OcrCharacter> subString = line.Text.Skip(firstCharacter).Take(lastCharacter - firstCharacter + 1);
                                string subText = subString.Aggregate(new StringBuilder(), (sb, next) => sb.Append(next.Value)).ToString();
                                if(!usedStrings.Contains(subText))
                                {
                                    usedStrings.Add(subText);
                                    if(index - firstCharacter < SubtextWidth)
                                    {
                                        subText = new string(' ', SubtextWidth - (index - firstCharacter)) + subText;
                                    }
                                    if(lastCharacter - index < SubtextWidth)
                                    {
                                        subText = subText + new string(' ', SubtextWidth - (lastCharacter - index));
                                    }
                                    this.spacingListBox.Items.Add(new SpacingItem(subText, subIndex));
                                    index = lastCharacter;
                                }
                            }
                        }
                    }
                }
            }
            if(this.spacingListBox.Items.Count != 0)
            {
                this.spacingListBox.SelectedIndex = 0;
            }
        }

        private void kerningItalicLeftUpDown_ValueChanged(object sender, EventArgs e)
        {
            if(!this.updatingUpDowns)
            {
                Char c = (char)this.kerningCharacterComboBox.SelectedItem;
                FontKerning.LeftItalicKerning[c] = Convert.ToInt32(this.kerningItalicLeftUpDown.Value);
                this.data.WorkingData.CompileSubtitleLines();
                this.data.WorkingData.CorrectSpellings(this.ocrMap);
                FillListWithSpellings(c, true);
            }
        }

        private void kerningItalicRightUpDown_ValueChanged(object sender, EventArgs e)
        {
            if(!this.updatingUpDowns)
            {
                Char c = (char)this.kerningCharacterComboBox.SelectedItem;
                FontKerning.RightItalicKerning[c] = Convert.ToInt32(this.kerningItalicRightUpDown.Value);
                this.data.WorkingData.CompileSubtitleLines();
                this.data.WorkingData.CorrectSpellings(this.ocrMap);
                FillListWithSpellings(c, true);
            }
        }

        private void kerningLeftUpDown_ValueChanged(object sender, EventArgs e)
        {
            if(!this.updatingUpDowns)
            {
                Char c = (char)this.kerningCharacterComboBox.SelectedItem;
                FontKerning.LeftKerning[c] = Convert.ToInt32(this.kerningLeftUpDown.Value);
                this.data.WorkingData.CompileSubtitleLines();
                this.data.WorkingData.CorrectSpellings(this.ocrMap);
                FillListWithSpellings(c, false);
            }
        }

        private void kerningRightUpDown_ValueChanged(object sender, EventArgs e)
        {
            if(!this.updatingUpDowns)
            {
                Char c = (char)this.kerningCharacterComboBox.SelectedItem;
                FontKerning.RightKerning[c] = Convert.ToInt32(this.kerningRightUpDown.Value);
                this.data.WorkingData.CompileSubtitleLines();
                this.data.WorkingData.CorrectSpellings(this.ocrMap);
                FillListWithSpellings(c, false);
            }
        }

        private void spacingListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if((this.spacingListBox.Items.Count != 0) && (this.spacingListBox.SelectedIndex >= 0))
            {
                SpacingItem item = this.spacingListBox.SelectedItem as SpacingItem;
                ISubtitleData subData = this.data.WorkingData.Subtitles[item.SubtitleIndex];
                using(SubtitleBitmap subBitmap = subData.DecodeBitmap())
                {
                    this.matchSoFarView.UpdateBackground(subBitmap.Bitmap, subBitmap.Origin, this.data.WorkingData.VideoAttributes.Size);
                }
            }
        }

        private void saveSpacingButton_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.SavedKerningValues))
            {
                if(MessageBox.Show(this, "Are you sure you want to over-write the saved Spacing values?", "Are you sure?", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }
            Properties.Settings.Default.SavedKerningValues = FontKerning.KerningDiffList;
            Properties.Settings.Default.Save();
        }

        private void restoreSavedSpacingButton_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.SavedKerningValues))
            {
                FontKerning.KerningDiffList = Properties.Settings.Default.SavedKerningValues;
                UpdateAfterChangedSpacing();
            }
            else
            {
                MessageBox.Show(this, "There are no saved Spacing values");
            }
        }

        private void restoreDefaultSpacingButton_Click(object sender, EventArgs e)
        {
            FontKerning.RestoreDefaultKernings();
            UpdateAfterChangedSpacing();
        }

        void UpdateAfterChangedSpacing()
        {
            this.data.WorkingData.CompileSubtitleLines();
            this.data.WorkingData.CorrectSpellings(this.ocrMap);

            Char selectedChar = (char)this.kerningCharacterComboBox.SelectedItem;

            this.updatingUpDowns = true;
            try
            {
                int value;
                FontKerning.LeftKerning.TryGetValue(selectedChar, out value);
                this.kerningLeftUpDown.Value = value;
                FontKerning.RightKerning.TryGetValue(selectedChar, out value);
                this.kerningRightUpDown.Value = value;
                FontKerning.LeftItalicKerning.TryGetValue(selectedChar, out value);
                this.kerningItalicLeftUpDown.Value = value;
                FontKerning.RightItalicKerning.TryGetValue(selectedChar, out value);
                this.kerningItalicRightUpDown.Value = value;
            }
            finally
            {
                this.updatingUpDowns = false;
            }

            FillListWithSpellings(selectedChar, this.italicRadioButton.Checked);
        }
    }
}
