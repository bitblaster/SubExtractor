using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DvdSubOcr;
using DvdNavigatorCrm;

namespace DvdSubExtractor
{
    public partial class OcrBlocksStep : UserControl, IWizardItem
    {
        class SubtitleData
        {
            public SubtitleData(ISubtitleData dvdData)
            {
                this.DvdData = dvdData;
            }

            public void AddRectangle(OcrRectangle rectangle)
            {
                if(this.Rectangles == null)
                {
                    this.Rectangles = new List<OcrRectangle>();
                }
                this.Rectangles.Add(rectangle);
            }

            public void ResetData()
            {
                this.Rectangles = null;
            }

            public ISubtitleData DvdData { get; private set; }
            public IList<OcrRectangle> Rectangles { get; private set; }
            public string Comment { get; set; }
        }

        class UndoData
        {
            public UndoData(int subtitleIndex, BlockEncode splitEncode)
            {
                this.SubtitleIndex = subtitleIndex;
                this.SplitEncode = splitEncode;
            }

            public UndoData(int subtitleIndex, OcrEntry ocrEntry)
            {
                this.SubtitleIndex = subtitleIndex;
                this.OcrEntry = ocrEntry;
            }

            public int SubtitleIndex { get; private set; }
            public BlockEncode SplitEncode { get; private set; }
            public OcrEntry OcrEntry { get; private set; }
        }

        ExtractData data;
        List<UndoData> undoList = new List<UndoData>();
        OcrMap ocrMap = new OcrMap();
        int movieId;
        string originalSplitHelp;
        List<Control> controlsToDisableDuringSplit = new List<Control>();
        int subtitleIndex;
        List<SubtitleData> allData = new List<SubtitleData>();
        SubtitleBitmap currentSubtitleBitmap;
        ISubtitleData currentDvdData;
        IList<OcrRectangle> currentRectangles;
        BlockEncode unmatchedEncode;
        List<BlockEncode> selectedEncodes = new List<BlockEncode>();
        OcrRectangle unmatchedRectangle;
        bool recursiveIgnoreCheck;
        bool isComplete;
        string helpText = "";
        ITaskbarList4 taskBarIntf;
        bool isHighDef;
        IList<KeyValuePair<int, Color>> savedPalette;
        IList<KeyValuePair<int, Color>> altPalette;
        OcrRectangle altPaletteRect;

        public OcrBlocksStep()
        {
            InitializeComponent();

#if DEBUG
            this.retryButton.Visible = true;
            this.startOverMovieButton.Visible = true;
            this.startOverSubButton.Visible = true;
#endif
        }

        const string UnknownCharacterHelp = "This page is where the main work of the Subitle OCR " +
            "program is done:  you will work with the computer to convert the " +
            "DVD Subtitle bitmaps into normal Text.\n\nMostly this involves seeing what is " +
            "highlighted in the top-left window and clicking with your mouse " +
            "on the matching character in the Character Selection Box below it.\n\n" +
            "Things to watch out for:  If a character, like i or j or ? has more than " +
            "1 piece to it, select ALL the pieces " +
            "(using the mouse or ARROW KEYS) " +
            "in the top window BEFORE you click on the matching character below.\n\n" +
            "If the text in the DVD is in an Italic (slanted) font you must press and " +
            "hold either Ctrl key down when selecting your character.  You may not care " +
            "if the resulting final product has Italics formatting codes in it, but this " +
            "program can't find the spaces between words correctly unless it knows if the " +
            "characters are in Italics or not, so please USE THE CTRL KEY FOR ITALICS.\n\n" +
            "Sometimes characters run together, so a big block of pixels " +
            "is highlighted in the top-left window (more than 1 character at once).  In this " +
            "case you need to Split the block using the \'Split in 2\' button.\n\n" +
            "If the patterns in the top-left window look funny: for example if they are just big " +
            "outlines of letters instead of being filled-in, you can hit the " +
            "\'Different Palette\' button if it's enabled.  This program tries to find " +
            "the best combination of colors to make OCR'ing easy, but it isn't perfect.";

        const string SplitIn2Help = "This feature lets you split characters in a DVD " +
            "Subtitle that are running together into separate blocks that are easier to " +
            "process.  Typically, 2 characters like an 'r' and a 'w' are touching and " +
            "you want them separated.  This program automatically tries to find any " +
            "characters that fit a known pattern on the left or right of the big block " +
            "when you first hit 'Split in 2', but that doesn't always work and you have " +
            "to paint (with the mouse) over one of the characters (r or w in our example) " +
            "then hit 'Commit Split'.\n\nRight-Click with your mouse in the block window " + 
            "to restart painting if you go too far.\n\nLeft-Click to do a paint bucket-type fill " + 
            "of the pixels underneath the mouse.\n\nIf more than 2 characters are in " + 
            "the block, just paint over 1 on the left or right and Commit - you can split " +
            "the remaining block again later.\n\nSometimes 2 parts of a character, like " +
            "the lower leg and dot over an 'i', are in the same block connected to " +
            "another character.  Don't worry about it - just paint over 1 part at a time. " +
            "You can put the character back together again later.\n\nIf the Auto-split " +
            "that runs at the start finds a character that looks reasonable, it's best " +
            "not to fiddle with painting and just hit Commit even if there's a pixel " +
            "or 2 wrong.  Don't sweat the small stuff";

        const string OffBaselineHelp = "Sometimes 2 characters look the same in many " + 
            "fonts, like (comma) and (single-quote) or (period) and the lower " +
            "dot in a (colon).  The only way an OCR program like this one can tell " +
            "them apart is to see where they fall vertically within their line of text.\n\n" +
            "This message shows up when a character is possibly recognized, but seems to be " + 
            "above or below the baseline of the other characters around it.  Hopefully " +
            "choosing the correct character (or selecting a 2nd block and choosing the " +
            "correct character in the case of a colon) will allow you to proceed.\n\n" +
            "SOMETIMES though this program gets the baseline wrong and a character " +
            "should be accepted anyway.  If you try hitting the correct character a " +
            "couple of times and this message still appears, Check the 'Ignore Baseline " +
            "Here' Checkbox and you'll be allowed to proceed.";

        const string CompletedOcrHelp = "Congratulations, it looks like every pixel on " +
            "every subtitle in your program has been converted to text.  Time to go " +
            "to the Next Step";

        const string WordErrorHelp = "A common error was identified during OCR: " +
            "2 single-quote characters are found side by side.  In this case " +
            "one of the single-quotes will be highlighted and you need " +
            "to either select the other one beside it then click on the double-quotes character " +
            "in the Selection Box to train the OCR Engine what a double-quote looks like, " + 
            "or hit the single-quote character to let the OCR engine know that in this case there is no error.";

        const string ThetaErrorHelp = "An error was identified during OCR: " +
            "an O-shaped character was found surrounding a dash-shaped " +
            "character. Select and match the appropriate Theta character instead.";

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

            this.isHighDef = data.IsHighDef;
            this.originalSplitHelp = this.splitHelpLabel.Text;
            this.controlsToDisableDuringSplit.AddRange(new Control[] {
                this.characterSelector, this.manualEntryTextBox, this.manualItalicCheckBox,
                this.unknownCharacterButton, this.beginSplitButton, this.commentButton,
                this.tryDifferentPaletteButton, this.undoButton,
                this.startOverMovieButton, this.startOverSubButton, this.reviewButton });
            this.data = data;
            string dataFileText = Path.GetFileNameWithoutExtension(this.data.SelectedSubtitleBinaryFile);
            this.dvdLabel.Text = dataFileText;
            this.movieId = this.ocrMap.AddMovieName(dataFileText);
            this.blockViewer.MovieId = this.movieId;
            foreach(ISubtitleData dvdData in this.data.WorkingData.Subtitles)
            {
                this.allData.Add(new SubtitleData(dvdData));
            }
            this.progressBar1.Maximum = this.allData.Count;
            if((this.data.WorkingData.Rectangles != null) && 
                (this.data.WorkingData.Rectangles.Count == this.allData.Count))
            {
                int index = 0;
                foreach(IList<OcrRectangle> rectList in this.data.WorkingData.Rectangles)
                {
                    if(rectList != null)
                    {
                        foreach(OcrRectangle rect in rectList)
                        {
                            this.allData[index].AddRectangle(rect);
                        }
                    }
                    index++;
                }
                this.subtitleIndex = this.data.WorkingData.Subtitles.Count - 1;
            }
            
            if(Properties.Settings.Default.WaitForEnterDuringOcr)
            {
                this.manualWaitEnterCheckBox.Checked = true;
            }

            this.FindForm().KeyDown += this.OcrBlocksStep_KeyDown;
            this.FindForm().KeyUp += this.OcrBlocksStep_KeyUp;

            this.data.NewStepInitialize(false, true, "", new Type[] { typeof(LoadFolderStep) });

            if(NativeMethods.SupportsTaskProgress)
            {
                CTaskbarList taskBar = new CTaskbarList();
                this.taskBarIntf = (ITaskbarList4)taskBar;
                this.taskBarIntf.HrInit();
                this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.Indeterminate);
            }

            LoadCurrentSubtitleData();
            FindNextOcr();
        }

        public void Terminate()
        {
            if(this.taskBarIntf != null)
            {
                this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.NoProgress);
            }

            this.FindForm().KeyDown -= this.OcrBlocksStep_KeyDown;
            this.FindForm().KeyUp -= this.OcrBlocksStep_KeyUp;
            try
            {
                this.ocrMap.Save();
            }
            catch(Exception ex)
            {
                MessageBox.Show("OCR Map failed to save with exception message: " + ex.Message);
            }

            this.data.WorkingData.ClearCompiledLines();
            if(this.IsComplete)
            {
                this.data.WorkingData.ClearRectangles();
                foreach(SubtitleData subData in this.allData)
                {
                    if(!String.IsNullOrWhiteSpace(subData.Comment) && (subData.Rectangles.Count != 0))
                    {
                        subData.Rectangles[0].Comment = subData.Comment;
                    }
                    this.data.WorkingData.Rectangles.Add(subData.Rectangles);
                }
            }
        }

        bool IsComplete 
        {
            get { return this.isComplete; }
            set
            {
                this.isComplete = value;
                this.data.IsCurrentStepComplete = value;
            }
        }

        string HelpText
        {
            get { return this.helpText; }
            set
            {
                this.helpText = value;
                this.data.HelpText = this.HelpText;
            }
        }

        void LoadCurrentSubtitleData()
        {
            if(this.currentSubtitleBitmap != null)
            {
                this.currentSubtitleBitmap.Dispose();
                this.currentSubtitleBitmap = null;
            }
            this.currentDvdData = null;
            this.currentRectangles = null;
            this.unmatchedEncode = null;
            this.selectedEncodes.Clear();
            this.unmatchedRectangle = null;
            this.characterSelector.Clear();
            this.manualEntryTextBox.Clear();
            this.blockViewer.UpdateBlockPicture(null, null, null);
            this.beginSplitButton.Enabled = false;
            this.unknownCharacterButton.Enabled = false;
            this.commentButton.Enabled = false;
            this.IsComplete = false;

            if((this.subtitleIndex < 0) || (this.subtitleIndex >= this.allData.Count))
            {
                this.subtitleIndexLabel.Text = "";
                this.progressBar1.Value = 0;
                if(this.taskBarIntf != null)
                {
                    this.taskBarIntf.SetProgressValue(this.TopLevelControl.Handle, 100, 100);
                }
                return;
            }

            SubtitleData data = this.allData[this.subtitleIndex];
            this.subtitleIndexLabel.Text = String.Format("Subtitle {0} of {1}", this.subtitleIndex + 1,
                this.allData.Count);
            this.progressBar1.Value = this.subtitleIndex;
            if(this.taskBarIntf != null)
            {
                this.taskBarIntf.SetProgressValue(this.TopLevelControl.Handle, (ulong)this.subtitleIndex, (ulong)this.allData.Count);
                //uint result2 = this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TBPFLAG.TBPF_NORMAL);
            }

            this.currentDvdData = data.DvdData;
            this.currentSubtitleBitmap = this.currentDvdData.DecodeBitmap();
            this.matchSoFarView.UpdateBackground(this.currentSubtitleBitmap.Bitmap, 
                this.currentSubtitleBitmap.Origin, this.data.WorkingData.VideoAttributes.Size);

            if(data.Rectangles == null)
            {
                this.altPalette = null;
                this.altPaletteRect = null;
                List<int> alphaColors = new List<int>();
                for(int index = 0; index < 4; index++)
                {
                    if(this.currentSubtitleBitmap.Bitmap.Palette.Entries[index].A != 0)
                    {
                        alphaColors.Add(index);
                    }
                }

                ContiguousEncode encode = new ContiguousEncode(
                    this.currentSubtitleBitmap.Data,
                    this.currentSubtitleBitmap.Bitmap.Width, 
                    this.currentSubtitleBitmap.Bitmap.Height,
                    this.currentSubtitleBitmap.Stride);

                foreach(RectangleWithColors rect in encode.FindSections(alphaColors, this.data.WorkingData.VideoAttributes.VerticalResolution > 1000))
                {
                    OcrRectangle newRect = new OcrRectangle(this.currentSubtitleBitmap,
                        rect.Rectangle.Location, this.ocrMap, this.movieId, 
                        rect.Rectangle, rect.ColorIndexes, this.isHighDef);
                    if(this.savedPalette != null)
                    {
                        newRect.SelectTopPalette(this.savedPalette);
                    }
                    data.AddRectangle(newRect);
                    foreach(string fullEncode in newRect.SplitEncodes)
                    {
                        this.ocrMap.AddSplit(fullEncode, this.movieId);
                    }
                }
            }
            this.currentRectangles = data.Rectangles;
        }

        void FindNextOcr()
        {
            this.unmatchedEncode = null;
            this.selectedEncodes.Clear();
            this.unmatchedRectangle = null;
            this.characterSelector.Clear();
            this.manualEntryTextBox.Clear();
            this.blockViewer.UpdateBlockPicture(null, null, null);
            this.beginSplitButton.Enabled = false;
            this.unknownCharacterButton.Enabled = false;
            this.commentButton.Enabled = false;
            this.manualEntryTextBox.Enabled = false;
            this.manualItalicCheckBox.Enabled = false;
            this.blockViewer.Message = "";
            this.ignoreBaselineCheckBox.Visible = false;
            this.IsComplete = false;
            if(this.taskBarIntf != null)
            {
                this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.Normal);
            }

            this.Enabled = false;
            try
            {
                while(true)
                {
                    Application.DoEvents();
                    if(this.IsDisposed || this.Disposing)
                    {
                        return;
                    }
                    if(this.currentRectangles != null)
                    {
                        for(int index = 0; index < this.currentRectangles.Count; index++)
                        {
                            OcrRectangle rect = this.currentRectangles[index];
                            OcrRectangle.BestMatchResult matchResult = rect.FindBestMatches(this.data.WorkingData.AllowedBaselineErrors);
                            if(matchResult != OcrRectangle.BestMatchResult.AllMatch)
                            {
                                int unmatched = rect.FindNextUnmatchedBlock(0);
                                if(unmatched != -1)
                                {
                                    if(matchResult == OcrRectangle.BestMatchResult.BaselineError)
                                    {
                                        while(unmatched != -1)
                                        {
                                            if(this.ocrMap.FindMatches(rect.Blocks[unmatched].FullEncode, this.movieId, this.isHighDef).Count() != 0)
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                unmatched = rect.FindNextUnmatchedBlock(unmatched + 1);
                                            }
                                        }
                                        if(unmatched == -1)
                                        {
                                            unmatched = rect.FindNextUnmatchedBlock(0);
                                        }
                                    }

                                    this.matchSoFarView.Update(rect.Blocks, rect.Matches);
                                    this.unmatchedEncode = rect.Blocks[unmatched];
                                    this.selectedEncodes.Add(this.unmatchedEncode);
                                    this.unmatchedRectangle = rect;
                                    this.blockViewer.UpdateBlockPicture(this.unmatchedEncode, this.selectedEncodes,
                                        rect.Blocks);
                                    this.beginSplitButton.Enabled = true;
                                    this.unknownCharacterButton.Enabled = true;
                                    this.commentButton.Enabled = true;
                                    this.manualEntryTextBox.Enabled = true;
                                    this.manualItalicCheckBox.Enabled = true;
                                    this.tryDifferentPaletteButton.Enabled =
                                        (this.unmatchedRectangle.PaletteCount > 1);

                                    if(matchResult == OcrRectangle.BestMatchResult.BaselineError)
                                    {
                                        foreach(OcrEntry entry in
                                            this.ocrMap.FindMatches(this.unmatchedEncode.FullEncode, this.movieId, this.isHighDef))
                                        {
                                            this.ShowIgnoreBaselineCheck();
                                            this.blockViewer.Message = "Character Matched but Off-Baseline";
                                            this.HelpText = OffBaselineHelp;
                                            if(this.taskBarIntf != null)
                                            {
                                                this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.Paused);
                                            }
                                            if(this.unmatchedRectangle != null)
                                            {
                                                this.paletteIndexLabel.Text = this.unmatchedRectangle.PaletteIndexListAsString;
                                            }
                                            return;
                                        }
                                    }

                                    // Unknown Character
                                    CharacterSplit splitter = new CharacterSplit(this.unmatchedEncode,
                                        this.ocrMap, this.movieId);
                                    CharacterSplit.SplitResult result = new CharacterSplit.SplitResult();
                                    if(splitter.AutoTestSplits(result, true, this.isHighDef) && (result.Entry2 != null))
                                    {
                                        char char1 = result.Entry1.OcrCharacter.Value;
                                        char char2 = result.Entry2.OcrCharacter.Value;
                                        if((Char.IsLetterOrDigit(char1) || SubConstants.GoodSplitSymbols.Contains(char1)) &&
                                            (Char.IsLetterOrDigit(char2) || SubConstants.GoodSplitSymbols.Contains(char2)) &&
                                            !SubConstants.VeryCheapSplitCharacters.Contains(char1) &&
                                            !SubConstants.VeryCheapSplitCharacters.Contains(char2))
                                        {
                                            if(!((char1 == 'r') && ((char2 == 'n') || (char2 == 'ñ') || (char2 == 'ń'))) &&
                                                !((char2 == 'r') && ((char1 == 'n') || (char1 == 'ñ') || (char1 == 'ń'))))
                                            {
                                                IList<BlockEncode> encodes1, encodes2;
                                                splitter.FindSplitEncodes(result.SplitPixels, out encodes1, out encodes2);

                                                if((encodes1.Count == 1) && (encodes2.Count == 1))
                                                {
                                                    this.ocrMap.AddSplit(this.unmatchedEncode.FullEncode,
                                                        new OcrSplit(encodes1[0].Origin, encodes1[0].FullEncode),
                                                        new OcrSplit(encodes2[0].Origin, encodes2[0].FullEncode),
                                                        this.movieId);
                                                    Debug.WriteLine(string.Format("Found Perfect Split {0} & {1}",
                                                        result.Entry1.OcrCharacter.Value, result.Entry2.OcrCharacter.Value));
                                                    this.allData[this.subtitleIndex].ResetData();
                                                    this.subtitleIndex--;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    // normal character not matched case
                                    HideIgnoreBaselineCheck();
                                    this.HelpText = UnknownCharacterHelp;
                                    if(this.taskBarIntf != null)
                                    {
                                        this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.Paused);
                                    }
                                    if(this.unmatchedRectangle != null)
                                    {
                                        this.paletteIndexLabel.Text = this.unmatchedRectangle.PaletteIndexListAsString;
                                    }
                                    return;
                                }
                            }

                            int wordError = FindWordError(rect);
                            if(wordError != -1)
                            {
                                this.matchSoFarView.Update(rect.Blocks, rect.Matches);
                                this.unmatchedEncode = rect.Blocks[wordError];
                                this.selectedEncodes.Add(this.unmatchedEncode);
                                this.unmatchedRectangle = this.currentRectangles[index];
                                this.blockViewer.UpdateBlockPicture(this.unmatchedEncode, 
                                    this.selectedEncodes, rect.Blocks);
                                this.beginSplitButton.Enabled = true;
                                this.unknownCharacterButton.Enabled = true;
                                this.commentButton.Enabled = true;
                                this.manualEntryTextBox.Enabled = true;
                                this.manualItalicCheckBox.Enabled = true;
                                this.blockViewer.Message = "Double Quotes Detected";
                                rect.IgnoreDoubleQuotes = false;
                                HideIgnoreBaselineCheck();
                                this.HelpText = WordErrorHelp;
                                if(this.taskBarIntf != null)
                                {
                                    this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.Paused);
                                }
                                if(this.unmatchedRectangle != null)
                                {
                                    this.paletteIndexLabel.Text = this.unmatchedRectangle.PaletteIndexListAsString;
                                }
                                return;
                            }

                            Tuple<int, int> thetaError = FindThetaError(rect);
                            if(thetaError != null)
                            {
                                this.matchSoFarView.Update(rect.Blocks, rect.Matches);
                                this.unmatchedEncode = rect.Blocks[thetaError.Item1];
                                this.selectedEncodes.Add(this.unmatchedEncode);
                                this.selectedEncodes.Add(rect.Blocks[thetaError.Item2]);
                                this.unmatchedRectangle = this.currentRectangles[index];
                                this.blockViewer.UpdateBlockPicture(this.unmatchedEncode,
                                    this.selectedEncodes, rect.Blocks);
                                this.beginSplitButton.Enabled = true;
                                this.unknownCharacterButton.Enabled = true;
                                this.commentButton.Enabled = true;
                                this.manualEntryTextBox.Enabled = true;
                                this.manualItalicCheckBox.Enabled = true;
                                this.blockViewer.Message = "Theta character detected";
                                HideIgnoreBaselineCheck();
                                this.HelpText = ThetaErrorHelp;
                                if(this.taskBarIntf != null)
                                {
                                    this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.Paused);
                                }
                                if(this.unmatchedRectangle != null)
                                {
                                    this.paletteIndexLabel.Text = this.unmatchedRectangle.PaletteIndexListAsString;
                                }
                                return;
                            }

                            Application.DoEvents();
                            if(this.IsDisposed || this.Disposing)
                            {
                                return;
                            }
                        }
                    }

                    if(this.altPalette != null)
                    {
                        if(this.altPaletteRect != null)
                        {
                            this.paletteIndexLabel.Text = this.altPaletteRect.PaletteIndexListAsString;
                            if(this.altPaletteRect.Blocks.Count != 0)
                            {
                                this.matchSoFarView.Update(this.altPaletteRect.Blocks, this.altPaletteRect.Matches);
                                this.blockViewer.UpdateBlockPicture(this.altPaletteRect.Blocks[this.altPaletteRect.Blocks.Count - 1],
                                    this.altPaletteRect.Blocks, this.altPaletteRect.Blocks);
                            }
                        }
                        if(MessageBox.Show(
                            this,
                            "Use the current Palette for the rest of the movie?", "Use New Palette",
                            MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            this.savedPalette = this.altPalette;
                        }
                        this.altPalette = null;
                        this.altPaletteRect = null;
                    }

                    if(this.subtitleIndex >= this.data.WorkingData.Subtitles.Count - 1)
                    {
                        this.blockViewer.Message = "OCR Complete!";
                        this.blockViewer.UpdateBlockPicture(null, null, null);
                        this.matchSoFarView.Update(new BlockEncode[] { }, new EncodeMatch[] { });
                        this.unmatchedEncode = null;
                        this.selectedEncodes.Clear();
                        this.unmatchedRectangle = null;
                        this.characterSelector.Clear();
                        this.manualEntryTextBox.Clear();
                        this.tryDifferentPaletteButton.Enabled = false;
                        this.IsComplete = true;
                        this.HelpText = CompletedOcrHelp;
                        if(this.taskBarIntf != null)
                        {
                            this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.Indeterminate);
                        }
                        if(this.unmatchedRectangle != null)
                        {
                            this.paletteIndexLabel.Text = this.unmatchedRectangle.PaletteIndexListAsString;
                        }
                        return;
                    }

                    this.subtitleIndex++;
                    this.data.WorkingData.AllowedBaselineErrors.Clear();
                    LoadCurrentSubtitleData();
                }
            }
            finally
            {
                this.Enabled = true;

                if(this.manualEntryTextBox.Enabled)
                {
                    //this.FindForm().Activate();
                    this.manualEntryTextBox.Focus();
                }
            }
        }

        static Tuple<int, int> FindThetaError(OcrRectangle rect)
        {
            if(rect.SubtitleText != null)
            {
                foreach(SubtitleLine line in rect.SubtitleText.Lines)
                {
                    bool wasOh = false;
                    Rectangle ohRect = new Rectangle();
                    int index = 0;
                    foreach(OcrCharacter ocr in line.Text)
                    {
                        if(SubConstants.DashCharacters.Contains(ocr.Value))
                        {
                            if(wasOh)
                            {
                                BlockEncode dashEnc = rect.Blocks[line.BlockIndexes[index]];
                                Rectangle dashRect = new Rectangle(dashEnc.Origin, new Size(dashEnc.TrueWidth, dashEnc.Height));
                                if(ohRect.Contains(dashRect))
                                {
                                    return new Tuple<int,int>(line.BlockIndexes[index - 1], line.BlockIndexes[index]);
                                }
                            }
                        }
                        wasOh = SubConstants.OhCharacters.Contains(ocr.Value);
                        if(wasOh)
                        {
                            BlockEncode ohEnc = rect.Blocks[line.BlockIndexes[index]];
                            ohRect = new Rectangle(ohEnc.Origin, new Size(ohEnc.TrueWidth, ohEnc.Height));
                        }
                        index++;
                    }
                }
            }
            return null;
        }

        static int FindWordError(OcrRectangle rect)
        {
            if(rect.IgnoreDoubleQuotes.HasValue && rect.IgnoreDoubleQuotes.Value)
            {
                return -1;
            }
            if(rect.SubtitleText != null)
            {
                foreach(SubtitleLine line in rect.SubtitleText.Lines)
                {
                    bool wasQuote = false;
                    int index = 0;
                    foreach(OcrCharacter ocr in line.Text)
                    {
                        if((ocr.Value == '\'') || (ocr.Value == '่'))
                        {
                            if(wasQuote)
                            {
                                //this.wordErrorLabel.Text = "Double Quotes";
                                return line.BlockIndexes[index];
                            }
                            wasQuote = true;
                        }
                        else
                        {
                            wasQuote = false;
                        }
                        index++;
                    }
                }
            }
            return -1;
        }

        private void blockViewer_EncodeClicked(object sender, BlockViewer.BlockEncodeArgs e)
        {
            bool wasSelected = false;
            foreach(BlockEncode sel in this.selectedEncodes)
            {
                if(object.ReferenceEquals(sel, e.Encode))
                {
                    this.selectedEncodes.Remove(sel);
                    wasSelected = true;
                    break;
                }
            }
            if(!wasSelected && (this.selectedEncodes.Count < SubConstants.MaximumOcrPieces))
            {
                this.selectedEncodes.Add(e.Encode);
            }
            this.blockViewer.UpdateBlockPicture(this.unmatchedEncode, this.selectedEncodes,
                this.unmatchedRectangle.Blocks);
        }

        private void tryDifferentPaletteButton_Click(object sender, EventArgs e)
        {
            if(this.unmatchedRectangle != null)
            {
                this.unmatchedRectangle.NextPalette();
                this.altPalette = this.unmatchedRectangle.GetChangedPalette();
                this.altPaletteRect = this.unmatchedRectangle;
                foreach(string fullEncode in this.unmatchedRectangle.SplitEncodes)
                {
                    this.ocrMap.AddSplit(fullEncode, this.movieId);
                }
                FindNextOcr();
            }
        }

        private void beginSplitButton_Click(object sender, EventArgs e)
        {
            this.cancelSplitButton.Enabled = true;
            this.doneSplitButton.Enabled = true;
            foreach(Control control in this.controlsToDisableDuringSplit)
            {
                control.Enabled = false;
            }
            this.splitHelpLabel.Text = "If Auto-Split doesn't work - Paint the block " +
                "with your mouse so that only one character is black";
            this.blockViewer.Message = "Paint 1 Character Green With Mouse, Left-click to Fill In, Right-Click to Restart";

            BlockEncode biggestEncode;
            List<KeyValuePair<Point, string>> otherEncodes;
            BuildSelectedList(out biggestEncode, out otherEncodes);
            this.blockViewer.UpdateBlockPicture(biggestEncode, null, null);

            this.blockViewer.BeginSplit(this.ocrMap, this.isHighDef);
            this.HelpText = SplitIn2Help;
            //Cursor.Position = this.blockViewer.PointToScreen(new Point(this.blockViewer.Size));
        }

        private void doneSplitButton_Click(object sender, EventArgs e)
        {
            if(this.blockViewer.CommitSplit())
            {
                this.cancelSplitButton.Enabled = false;
                this.doneSplitButton.Enabled = false;
                foreach(Control control in this.controlsToDisableDuringSplit)
                {
                    control.Enabled = true;
                }
                this.splitHelpLabel.Text = this.originalSplitHelp;
                this.undoList.Add(new UndoData(this.subtitleIndex, this.unmatchedEncode));
                SetUndoState();
                this.allData[this.subtitleIndex].ResetData();

                var oldAltPalette = this.altPalette;
                var oldAltPaletteRect = this.altPaletteRect;
                var oldSavedPalette = this.savedPalette;
                if(this.altPalette != null)
                {
                    this.savedPalette = this.altPalette;
                }
                LoadCurrentSubtitleData();
                this.altPalette = oldAltPalette;
                this.altPaletteRect = null;
                if((this.altPalette != null) && (this.currentRectangles != null) && (this.currentRectangles.Count != 0))
                {
                    this.altPaletteRect = this.currentRectangles[0];
                }
                this.savedPalette = oldSavedPalette;

                this.blockViewer.CancelSplit();
                FindNextOcr();
            }
        }

        private void cancelSplitButton_Click(object sender, EventArgs e)
        {
            this.cancelSplitButton.Enabled = false;
            this.doneSplitButton.Enabled = false;
            foreach(Control control in this.controlsToDisableDuringSplit)
            {
                control.Enabled = true;
            }
            this.splitHelpLabel.Text = this.originalSplitHelp;
            this.blockViewer.CancelSplit();
            FindNextOcr();
        }

        private void BuildSelectedList(out BlockEncode biggestEncode,
            out List<KeyValuePair<Point, string>> otherEncodes)
        {
            int maxPixels = -1;
            int maxIndex = -1;
            for(int index = 0; index < this.selectedEncodes.Count; index++)
            {
                if(this.selectedEncodes[index].PixelCount > maxPixels)
                {
                    maxIndex = index;
                    maxPixels = this.selectedEncodes[index].PixelCount;
                }
            }
            biggestEncode = this.selectedEncodes[maxIndex];
            otherEncodes = new List<KeyValuePair<Point, string>>();
            for(int index = 0; index < this.selectedEncodes.Count; index++)
            {
                if(index != maxIndex)
                {
                    BlockEncode other = this.selectedEncodes[index];
                    otherEncodes.Add(new KeyValuePair<Point, string>(
                        other.Origin - new Size(biggestEncode.Origin), other.FullEncode));
                }
            }
        }

        private void characterSelector_SelectedCharacterChanged(object sender, SelectedCharacterArgs e)
        {
            if(this.selectedEncodes.Count == 0)
            {
                return;
            }

            BlockEncode biggestEncode;
            List<KeyValuePair<Point, string>> otherEncodes;
            BuildSelectedList(out biggestEncode, out otherEncodes);

            OcrEntry entryOld = null, entryNew = null;
            if(e.OldSelection != null)
            {
                entryOld = new OcrEntry(biggestEncode.FullEncode, e.OldSelection, otherEncodes);
            }
            if(e.Selection != null)
            {
                entryNew = new OcrEntry(biggestEncode.FullEncode, e.Selection, otherEncodes);
            }

            if(entryOld != null)
            {
                this.ocrMap.RemoveMatch(entryOld, this.movieId);
            }
            if(entryNew != null)
            {
                bool cancelNew = false;
                if(SubConstants.UsuallyInTwoPiecesCharacters.Contains(entryNew.OcrCharacter.Value) &&
                    (otherEncodes.Count == 0))
                {
                    if(MessageBox.Show(this, "This Character normally has at least 2 pieces. " +
                        "Are you sure you've selected every piece?",
                        "Warning - Entire Character may not be Selected",
                        MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        cancelNew = true;
                    }
                }
                if(!cancelNew)
                {
                    if((this.currentRectangles != null) && ((e.Selection.Value == '\'') || (e.Selection.Value == '่')))
                    {
                        foreach(OcrRectangle rect in this.currentRectangles)
                        {
                            if(rect.IgnoreDoubleQuotes.HasValue && !rect.IgnoreDoubleQuotes.Value)
                            {
                                rect.IgnoreDoubleQuotes = true;
                                break;
                            }
                        }
                    }

                    this.ocrMap.AddMatch(entryNew, this.movieId, this.isHighDef);
                    this.undoList.Add(new UndoData(this.subtitleIndex, entryNew));
                    SetUndoState();
                    this.undoButton.Enabled = true;
                    this.blockViewer.ShowFeedback(entryNew.OcrCharacter);
                }
            }

            FindNextOcr();
        }

        void OcrBlocksStep_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.F2)
            {
                this.manualWaitEnterCheckBox.Checked = !this.manualWaitEnterCheckBox.Checked;
            }
            bool isItalicTemp = e.Control && !e.Alt;    // do not trigger for AltGr
            this.characterSelector.Italics = isItalicTemp ^ this.manualItalicCheckBox.Checked;
        }

        void OcrBlocksStep_KeyUp(object sender, KeyEventArgs e)
        {
            bool isItalicTemp = e.Control && !e.Alt;    // do not trigger for AltGr
            this.characterSelector.Italics = isItalicTemp ^ this.manualItalicCheckBox.Checked;
        }

        private void unknownCharacterButton_Click(object sender, EventArgs e)
        {
            if(this.selectedEncodes.Count == 0)
            {
                return;
            }

            BlockEncode biggestEncode;
            List<KeyValuePair<Point, string>> otherEncodes;
            BuildSelectedList(out biggestEncode, out otherEncodes);

            OcrEntry entryNew = new OcrEntry(biggestEncode.FullEncode, 
                OcrCharacter.Unmatched, otherEncodes);
            this.ocrMap.AddMatch(entryNew, this.movieId, false);
            this.undoList.Add(new UndoData(this.subtitleIndex, entryNew));
            SetUndoState();
            FindNextOcr();
        }

        void SetUndoState()
        {
            if(this.undoList.Count != 0)
            {
                this.undoButton.Enabled = true;
            }
            else
            {
                this.undoButton.Enabled = false;
            }
        }

        private void undoButton_Click(object sender, EventArgs e)
        {
            UndoData lastChange = this.undoList[this.undoList.Count - 1];
            for(int index = this.subtitleIndex; index >= lastChange.SubtitleIndex; index--)
            {
                this.allData[index].ResetData();
            }
            if(lastChange.OcrEntry != null)
            {
                this.ocrMap.RemoveMatch(lastChange.OcrEntry, this.movieId);
            }
            else
            {
                this.ocrMap.RemoveSplit(lastChange.SplitEncode.FullEncode, this.movieId);
            }
            this.undoList.RemoveAt(this.undoList.Count - 1);

            SetUndoState();
            this.subtitleIndex = lastChange.SubtitleIndex;
            LoadCurrentSubtitleData();
            FindNextOcr();
        }

        private void startOverSubButton_Click(object sender, EventArgs e)
        {
            for(int undoIndex = this.undoList.Count - 1; undoIndex >= 0; undoIndex--)
            {
                UndoData lastChange = this.undoList[undoIndex];
                if(lastChange.SubtitleIndex != this.subtitleIndex)
                {
                    break;
                }
                if(lastChange.OcrEntry != null)
                {
                    this.ocrMap.RemoveMatch(lastChange.OcrEntry, this.movieId);
                }
                else
                {
                    this.ocrMap.RemoveSplit(lastChange.SplitEncode.FullEncode, this.movieId);
                }
                this.undoList.RemoveAt(undoIndex);
            }

            SetUndoState();

            foreach(OcrRectangle rect in this.currentRectangles)
            {
                foreach(string fullEncode in rect.SplitEncodes)
                {
                    this.ocrMap.RemoveSplit(fullEncode, this.movieId);
                }
                foreach(EncodeMatch match in rect.Matches)
                {
                    if(match != null)
                    {
                        this.ocrMap.RemoveMatch(match.OcrEntry, this.movieId);
                    }
                }
            }

            this.allData[this.subtitleIndex].ResetData();
            LoadCurrentSubtitleData();
            FindNextOcr();
        }

        private void startOverMovieButton_Click(object sender, EventArgs e)
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

            List<OcrEntry> entriesMovie = new List<OcrEntry>(
                this.ocrMap.GetMatchesForMovie(this.movieId, true));
            foreach(OcrEntry oldEntry in entriesMovie)
            {
                this.ocrMap.RemoveMatch(oldEntry, this.movieId);
            }

            this.undoList.Clear();
            SetUndoState();
            foreach(SubtitleData data in this.allData)
            {
                data.ResetData();
            }
            this.subtitleIndex = 0;
            LoadCurrentSubtitleData();
            FindNextOcr();
        }

        private void reviewButton_Click(object sender, EventArgs e)
        {
            using(OcrReviewForm review = new OcrReviewForm())
            {
                review.LoadMap(this.ocrMap, this.movieId);
                review.ShowDialog(this);
                if(review.ChangesMade)
                {
                    this.undoList.Clear();
                    SetUndoState();
                    foreach(SubtitleData data in this.allData)
                    {
                        data.ResetData();
                    }
                    this.subtitleIndex = 0;
                    LoadCurrentSubtitleData();
                    FindNextOcr();
                }
                else
                {
                    this.manualEntryTextBox.Focus();
                }
            }
        }

        private void ShowIgnoreBaselineCheck()
        {
            this.ignoreBaselineCheckBox.Visible = true;
            this.recursiveIgnoreCheck = true;
            this.ignoreBaselineCheckBox.Checked = false;
            this.recursiveIgnoreCheck = false;
        }

        private void HideIgnoreBaselineCheck()
        {
            this.ignoreBaselineCheckBox.Visible = false;
            this.recursiveIgnoreCheck = true;
            this.ignoreBaselineCheckBox.Checked = false;
            this.recursiveIgnoreCheck = false;
        }

        private void ignoreBaselineCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if((this.unmatchedEncode != null) && !this.recursiveIgnoreCheck)
            {
                this.data.WorkingData.AllowedBaselineErrors.Add(this.unmatchedEncode.FullEncode);
                FindNextOcr();
            }
        }

        private void retryButton_Click(object sender, EventArgs e)
        {
            SubtitleData data = this.allData[this.subtitleIndex];
            data.ResetData();
            LoadCurrentSubtitleData();
            FindNextOcr();
        }

        private void commentButton_Click(object sender, EventArgs e)
        {
            SubtitleData data = this.allData[this.subtitleIndex];
            string comment = data.Comment ?? "";
            using(CommentBox cbox = new CommentBox())
            {
                cbox.Comment = comment;
                if(cbox.ShowDialog(this) == DialogResult.OK)
                {
                    data.Comment = cbox.Comment;
                }
            }
        }

        void ManualMatchEntered(char c, bool isItalic)
        {
            if(this.selectedEncodes.Count == 0)
            {
                return;
            }

            BlockEncode biggestEncode;
            List<KeyValuePair<Point, string>> otherEncodes;
            BuildSelectedList(out biggestEncode, out otherEncodes);

            OcrCharacter ocrChar = new OcrCharacter(c, isItalic);
            OcrEntry entryNew = new OcrEntry(biggestEncode.FullEncode, ocrChar, otherEncodes);
            bool cancelNew = false;
            if(SubConstants.UsuallyInTwoPiecesCharacters.Contains(entryNew.OcrCharacter.Value) &&
                (otherEncodes.Count == 0))
            {
                if(MessageBox.Show(this, "This Character normally has at least 2 pieces. " +
                    "Are you sure you've selected every piece?",
                    "Warning - Entire Character may not be Selected",
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    cancelNew = true;
                }
            }
            if(!cancelNew)
            {
                if((this.currentRectangles != null) && ((entryNew.OcrCharacter.Value == '\'') || (entryNew.OcrCharacter.Value == '่')))
                {
                    foreach(OcrRectangle rect in this.currentRectangles)
                    {
                        if(rect.IgnoreDoubleQuotes.HasValue && !rect.IgnoreDoubleQuotes.Value)
                        {
                            rect.IgnoreDoubleQuotes = true;
                            break;
                        }
                    }
                }

                this.ocrMap.AddMatch(entryNew, this.movieId, this.isHighDef);
                this.undoList.Add(new UndoData(this.subtitleIndex, entryNew));
                this.blockViewer.ShowFeedback(ocrChar);
                SetUndoState();
            }

            FindNextOcr();
        }

        private void manualEntryTextBox_TextChanged(object sender, EventArgs e)
        {
            if(!this.manualWaitEnterCheckBox.Checked)
            {
                if(this.manualEntryTextBox.Text.Length == 1)
                {
                    if(this.manualEntryTextBox.Text == " ")
                    {
                        this.manualItalicCheckBox.Checked = !this.manualItalicCheckBox.Checked;
                        this.manualEntryTextBox.Clear();
                    }
                    else
                    {
                        ManualMatchEntered(this.manualEntryTextBox.Text[0], this.manualItalicCheckBox.Checked);
                    }
                }
            }
            else
            {
                string text = this.manualEntryTextBox.Text;
                this.manualEntryTextBox.Text = text.Replace(" ", "");
            }
        }

        private void manualItalicCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if(this.manualItalicCheckBox.Checked)
            {
                this.triangleFill1.Visible = true;
                this.triangleFill2.Visible = true;
                this.triangleFill3.Visible = true;
                this.triangleFill4.Visible = true;
            }
            else
            {
                this.triangleFill1.Visible = false;
                this.triangleFill2.Visible = false;
                this.triangleFill3.Visible = false;
                this.triangleFill4.Visible = false;
            }
            this.characterSelector.Italics = ((Control.ModifierKeys & Keys.Control) == Keys.Control) ^ this.manualItalicCheckBox.Checked;
            this.manualEntryTextBox.Focus();
        }

        private void manualEntryTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Func<BlockEncode, BlockEncode, bool> blockValid = null;
            Func<BlockEncode, BlockEncode, double> blockDistance = null;
            switch(e.KeyCode)
            {
            case Keys.Back:
                if(this.selectedEncodes.Count > 1)
                {
                    this.selectedEncodes.RemoveAt(this.selectedEncodes.Count - 1);
                    this.blockViewer.UpdateBlockPicture(this.unmatchedEncode, this.selectedEncodes,
                        this.unmatchedRectangle.Blocks);
                }
                else if(this.undoButton.Enabled)
                {
                    undoButton_Click(this, EventArgs.Empty);
                }
                break;
            case Keys.Up:
                blockValid = (block, unmatched) => 
                    {
                        int blockCenter = block.Origin.Y + block.Height / 2;
                        int unmatCenter = unmatched.Origin.Y + unmatched.Height / 2;
                        return (unmatCenter > block.Origin.Y + block.Height) || (unmatched.Origin.Y > blockCenter);
                    };
                blockDistance = (block, unmatched) =>
                    {
                        Point blockCenter = new Point(block.Origin.X + block.TrueWidth / 2, block.Origin.Y + block.Height / 2);
                        Point unmatCenter = new Point(this.unmatchedEncode.Origin.X + this.unmatchedEncode.TrueWidth / 2, this.unmatchedEncode.Origin.Y + this.unmatchedEncode.Height / 2);

                        int distX = Math.Abs(blockCenter.X - unmatCenter.X);
                        int distY = Math.Max(0, Math.Min(unmatCenter.Y - (block.Origin.Y + block.Height), unmatched.Origin.Y - blockCenter.Y));
                        return 2 * distX + distY;
                    };
                break;
            case Keys.Down:
                blockValid = (block, unmatched) => 
                    {
                        int blockCenter = block.Origin.Y + block.Height / 2;
                        int unmatCenter = unmatched.Origin.Y + unmatched.Height / 2;
                        return (unmatCenter < block.Origin.Y) || (unmatched.Origin.Y + unmatched.Height < blockCenter);
                    };
                blockDistance = (block, unmatched) =>
                    {
                        Point blockCenter = new Point(block.Origin.X + block.TrueWidth / 2, block.Origin.Y + block.Height / 2);
                        Point unmatCenter = new Point(this.unmatchedEncode.Origin.X + this.unmatchedEncode.TrueWidth / 2, this.unmatchedEncode.Origin.Y + this.unmatchedEncode.Height / 2);

                        int distX = Math.Abs(blockCenter.X - unmatCenter.X);
                        int distY = Math.Max(0, Math.Min(block.Origin.Y - unmatCenter.Y, blockCenter.Y - (unmatched.Origin.Y + unmatched.Height)));
                        return 2 * distX + distY;
                    };
                break;
            case Keys.Left:
                blockValid = (block, unmatched) => 
                    {
                        int blockCenter = block.Origin.X + block.TrueWidth / 2;
                        int unmatCenter = unmatched.Origin.X + unmatched.TrueWidth / 2;
                        return (unmatCenter > block.Origin.X + block.TrueWidth) || (unmatched.Origin.X > blockCenter);
                    };
                blockDistance = (block, unmatched) =>
                    {
                        Point blockCenter = new Point(block.Origin.X + block.TrueWidth / 2, block.Origin.Y + block.Height / 2);
                        Point unmatCenter = new Point(this.unmatchedEncode.Origin.X + this.unmatchedEncode.TrueWidth / 2, this.unmatchedEncode.Origin.Y + this.unmatchedEncode.Height / 2);

                        int distX = Math.Max(0, Math.Min(unmatCenter.X - (block.Origin.X + block.TrueWidth), unmatched.Origin.X - blockCenter.X));
                        int distY = Math.Abs(blockCenter.Y - unmatCenter.Y);
                        return distX + 2 * distY;
                    };
                break;
            case Keys.Right:
                blockValid = (block, unmatched) =>
                    {
                        int blockCenter = block.Origin.X + block.TrueWidth / 2;
                        int unmatCenter = unmatched.Origin.X + unmatched.TrueWidth / 2;
                        return (unmatCenter < block.Origin.X) || (unmatched.Origin.X + unmatched.TrueWidth < blockCenter);
                    };
                blockDistance = (block, unmatched) =>
                    {
                        Point blockCenter = new Point(block.Origin.X + block.TrueWidth / 2, block.Origin.Y + block.Height / 2);
                        Point unmatCenter = new Point(this.unmatchedEncode.Origin.X + this.unmatchedEncode.TrueWidth / 2, this.unmatchedEncode.Origin.Y + this.unmatchedEncode.Height / 2);

                        int distX = Math.Max(0, Math.Min(block.Origin.X - unmatCenter.X, blockCenter.X - (unmatched.Origin.X + unmatched.TrueWidth)));
                        int distY = Math.Abs(blockCenter.Y - unmatCenter.Y);
                        return distX + 2 * distY;
                    };
                break;
            case Keys.Space:
                if(this.manualWaitEnterCheckBox.Checked)
                {
                    this.manualItalicCheckBox.Checked = !this.manualItalicCheckBox.Checked;
                    e.Handled = true;
                }
                break;
            case Keys.Enter:
                if(this.manualWaitEnterCheckBox.Checked && (this.manualEntryTextBox.Text.Length >= 1))
                {
                    string text = this.manualEntryTextBox.Text;
                    ManualMatchEntered(text[0], this.manualItalicCheckBox.Checked);
                    this.manualEntryTextBox.Clear();
                }
                break;
            }

            if(blockValid != null)
            {
                BlockEncode match = null;
                double nearestDown = Double.MaxValue;
                Size blockViewSize = new Size(this.blockViewer.Width / 3, this.blockViewer.Height / 3);
                foreach(BlockEncode block in this.unmatchedRectangle.Blocks)
                {
                    Point blockCenter = new Point(block.Origin.X + block.TrueWidth / 2, block.Origin.Y + block.Height / 2);
                    Point unmatCenter = new Point(this.unmatchedEncode.Origin.X + this.unmatchedEncode.TrueWidth / 2, this.unmatchedEncode.Origin.Y + this.unmatchedEncode.Height / 2);
                    if((blockCenter.X > unmatCenter.X + blockViewSize.Width / 2 - 2) ||
                        (blockCenter.X < unmatCenter.X - blockViewSize.Width / 2 + 2) ||
                        (blockCenter.Y > unmatCenter.Y + blockViewSize.Height / 2 - 2) ||
                        (blockCenter.Y < unmatCenter.Y - blockViewSize.Height / 2 + 2))
                    {
                        continue;
                    }

                    if(blockValid(block, this.unmatchedEncode) && !this.selectedEncodes.Contains(block))
                    {
                        double dist = blockDistance(block, this.unmatchedEncode);
                        if(dist < nearestDown)
                        {
                            nearestDown = dist;
                            match = block;
                        }
                    }
                }
                if(match != null)
                {
                    this.selectedEncodes.Add(match);
                    this.blockViewer.UpdateBlockPicture(this.unmatchedEncode, this.selectedEncodes,
                        this.unmatchedRectangle.Blocks);
                }
            }
        }

        private void manualWaitEnterCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if(this.manualWaitEnterCheckBox.Checked)
            {
                this.manualEntryTextBox.MaxLength = 10;
                this.manualEntryTextBox.AcceptsReturn = true;
                Properties.Settings.Default.WaitForEnterDuringOcr = true;
            }
            else
            {
                this.manualEntryTextBox.Clear();
                this.manualEntryTextBox.MaxLength = 1;
                this.manualEntryTextBox.AcceptsReturn = false;
                Properties.Settings.Default.WaitForEnterDuringOcr = false;
            }
            Properties.Settings.Default.Save();
            this.manualEntryTextBox.Focus();
        }
    }
}
