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
using DvdSubOcr;

namespace DvdSubExtractor
{
    public partial class SpellCheckStep : UserControl, IWizardItem
    {
        ExtractData data;
        List<string> wordsToCheck = new List<string>();
        HashSet<string> ignoredWords = new HashSet<string>();
        OcrMap ocrMap = new OcrMap();
        SpellingNeeded currentSpelling;
        IEnumerator<SpellingNeeded> spellings;
        List<string> undoList = new List<string>();
        Font contextNormalFont;
        Font contextItalicFont;
        ITaskbarList4 taskBarIntf;

        public SpellCheckStep()
        {
            InitializeComponent();

            this.contextNormalFont = this.contextLineLabel.Font;
            this.contextItalicFont = new Font(this.contextNormalFont, FontStyle.Italic);
            this.indexLabel.Text = "";
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
            this.messageLabel.Text = "";

            this.statusLabel.Text = "Matching Font Sizes.";
            this.data.WorkingData.CompileSubtitleLines();

            if(this.IsDisposed || this.Disposing)
            {
                return;
            }

            this.data.OptionsUpdated += this.data_OptionsUpdated;

            this.data.NewStepInitialize(false, true, this.HelpText, 
                new Type[] { typeof(LoadFolderStep), typeof(ChooseSubtitlesStep) });

            if(NativeMethods.SupportsTaskProgress)
            {
                CTaskbarList taskBar = new CTaskbarList();
                this.taskBarIntf = (ITaskbarList4)taskBar;
                this.taskBarIntf.HrInit();
            }

            BeginAgain();
        }

        public OcrMap OcrMap { get { return this.ocrMap; } }

        void data_OptionsUpdated(object sender, EventArgs e)
        {
            if(this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(this.BeginAgain));
            }
        }

        void BeginAgain()
        {
            this.undoList.Clear();
            this.statusLabel.Text = "Please choose the correctly spelled word or 'No Good Spelling Listed' from the list below.";
            this.spellings = FindAdjustableWords().GetEnumerator();

            if(NativeMethods.SupportsTaskProgress)
            {
                this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.Indeterminate);
            }
            NextSpellingWord();
        }

        public void Terminate()
        {
            this.data.OptionsUpdated -= this.data_OptionsUpdated;

            if(this.taskBarIntf != null)
            {
                this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.NoProgress);
            }

            try
            {
                this.ocrMap.RemoveLandIWord("Al");
                this.ocrMap.RemoveLandIWord("AI");
                this.ocrMap.Save();
            }
            catch(Exception ex)
            {
                MessageBox.Show("OCR Map failed to save with exception message: " + ex.Message);
            }
        }

        string HelpText
        {
            get { return "Some words are common mistakes for OCR programs such as this.  " +
                "In particular, discriminating between capital I and lower-case l is impossible " +
                "with many of the fonts used by DVD authors.\n\nIn this step you will be asked to choose " +
                "the correct spelling of various words to fix up your subtitles and build a dictionary " +
                "of words that will make this step less painful for each new DVD program you run through it."; }
        }

        class SpellingNeeded
        {
            public SubtitleLine Line { get; set; }
            public string OriginalWord { get; set; }
            public int CharacterIndex { get; set; }
            public IEnumerable<string> Choices { get; set; }
        }

        void NextSpellingWord()
        {
            if((this.spellings != null) && (this.spellings.MoveNext()))
            {
                if(this.taskBarIntf != null)
                {
                    this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.Paused);
                }
                this.currentSpelling = this.spellings.Current;
                this.spellingListBox.Items.Clear();
                this.spellingListBox.Items.AddRange(this.currentSpelling.Choices.ToArray());
                this.contextLineLabel.Font = 
                    this.currentSpelling.Line.Text[this.currentSpelling.CharacterIndex].Italic ?
                    this.contextItalicFont : this.contextNormalFont;
                this.contextLineLabel.Text = this.currentSpelling.Line.ToString();
            }
            else
            {
                if(this.taskBarIntf != null)
                {
                    this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.Indeterminate);
                }
                this.spellingListBox.Items.Clear();
                this.contextLineLabel.Text = "";
                this.messageLabel.Text = "Complete!";
                if(this.spellings != null)
                {
                    this.spellings.Dispose();
                    this.spellings = null;
                }
                this.data.IsCurrentStepComplete = true;
                this.statusLabel.Text = "";
            }
        }

        IEnumerable<SpellingNeeded> FindAdjustableWords()
        {
            int subCount = this.data.WorkingData.AllLinesBySubtitle.Count;
            this.indexProgressBar.Maximum = subCount - 1;
            for(int subIndex = 0; subIndex < subCount; subIndex++)
            {
                if(this.taskBarIntf != null)
                {
                    this.taskBarIntf.SetProgressValue(this.TopLevelControl.Handle, (ulong)subIndex, (ulong)subCount);
                }

                this.indexLabel.Text = string.Format("{0} of {1}", subIndex + 1, subCount);
                this.indexProgressBar.Value = subIndex;
                foreach(SubtitleLine line in this.data.WorkingData.AllLinesBySubtitle[subIndex])
                {
                    foreach(SpellingCorrectionResult result in line.CorrectSpelling(this.ocrMap, this.ignoredWords))
                    {
                        yield return new SpellingNeeded
                        {
                            Line = line, 
                            CharacterIndex = result.CharacterIndex, 
                            Choices = new string[] { "No Good Spelling Listed" }.Concat(result.Choices),
                            OriginalWord = result.OriginalWord,
                        };
                    }
                }
            }
        }

        private void spellingListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(this.currentSpelling != null)
            {
                if((this.spellingListBox.Items.Count != 0) && (this.spellingListBox.SelectedIndex != -1))
                {
                    string word = this.currentSpelling.OriginalWord;
                    if(this.spellingListBox.SelectedIndex == 0)
                    {
                        this.ignoredWords.Add(word);
                        this.undoList.Add(word);
                        this.undoButton.Enabled = true;
                    }
                    else
                    {
                        int charIndex = this.currentSpelling.CharacterIndex;
                        SubtitleLine line = this.currentSpelling.Line;
                        string result = this.spellingListBox.SelectedItem as string;
                        this.ocrMap.AddLandIWord(result);
                        this.undoList.Add(result);
                        this.undoButton.Enabled = true;
                        if(result != word)
                        {
                            for(int index = 0; index < word.Length; index++)
                            {
                                line.Text[index + charIndex] = new OcrCharacter(result[index],
                                        line.Text[index + charIndex].Italic);
                            }
                        }
                    }
                    NextSpellingWord();
                }
            }
        }

        private void undoButton_Click(object sender, EventArgs e)
        {
            if((this.undoList.Count != 0) && (this.spellings != null))
            {
                string word = this.undoList[this.undoList.Count - 1];
                if(this.ignoredWords.Contains(word))
                {
                    this.ignoredWords.Remove(word);
                }
                else
                {
                    this.ocrMap.RemoveLandIWord(word);
                }
                this.undoList.RemoveAt(this.undoList.Count - 1);
                if(this.undoList.Count == 0)
                {
                    this.undoButton.Enabled = false;
                }

                this.spellings.Dispose();
                this.spellings = FindAdjustableWords().GetEnumerator();
                NextSpellingWord();
            }
        }

        private void skipSpellingButton_Click(object sender, EventArgs e)
        {
            if(this.spellings != null)
            {
                this.spellings.Dispose();
                this.spellings = null;
            }
            this.data.OnJumpTo(this, typeof(CreateSubtitleFileStep));
            //NextSpellingWord();
        }

        private void wordSpacingButton_Click(object sender, EventArgs e)
        {
            this.data.OnJumpTo(this, typeof(WordSpacingStep));
        }
    }
}
