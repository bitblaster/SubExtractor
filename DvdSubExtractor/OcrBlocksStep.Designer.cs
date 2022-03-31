namespace DvdSubExtractor
{
    partial class OcrBlocksStep
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.unknownCharacterButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.doneSplitButton = new System.Windows.Forms.Button();
            this.cancelSplitButton = new System.Windows.Forms.Button();
            this.beginSplitButton = new System.Windows.Forms.Button();
            this.splitHelpLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.undoButton = new System.Windows.Forms.Button();
            this.tryDifferentPaletteButton = new System.Windows.Forms.Button();
            this.subtitleIndexLabel = new System.Windows.Forms.Label();
            this.startOverSubButton = new System.Windows.Forms.Button();
            this.startOverMovieButton = new System.Windows.Forms.Button();
            this.reviewButton = new System.Windows.Forms.Button();
            this.ignoreBaselineCheckBox = new System.Windows.Forms.CheckBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.retryButton = new System.Windows.Forms.Button();
            this.paletteIndexLabel = new System.Windows.Forms.Label();
            this.commentButton = new System.Windows.Forms.Button();
            this.dvdLabel = new System.Windows.Forms.Label();
            this.manualEntryTextBox = new System.Windows.Forms.TextBox();
            this.manualCharLabel = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.manualItalicCheckBox = new System.Windows.Forms.CheckBox();
            this.manualWaitEnterCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.matchSoFarView = new DvdSubOcr.MatchSoFarView();
            this.blockViewer = new DvdSubOcr.BlockViewer();
            this.characterSelector = new DvdSubOcr.CharacterSelector();
            this.triangleFill4 = new DvdSubExtractor.TriangleFill();
            this.triangleFill3 = new DvdSubExtractor.TriangleFill();
            this.triangleFill2 = new DvdSubExtractor.TriangleFill();
            this.triangleFill1 = new DvdSubExtractor.TriangleFill();
            this.SuspendLayout();
            // 
            // unknownCharacterButton
            // 
            this.unknownCharacterButton.Location = new System.Drawing.Point(149, 629);
            this.unknownCharacterButton.Name = "unknownCharacterButton";
            this.unknownCharacterButton.Size = new System.Drawing.Size(112, 23);
            this.unknownCharacterButton.TabIndex = 14;
            this.unknownCharacterButton.Text = "Ignore";
            this.toolTip1.SetToolTip(this.unknownCharacterButton, "The selected pattern is not part of any character");
            this.unknownCharacterButton.Click += new System.EventHandler(this.unknownCharacterButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(34, 432);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(201, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Press Ctrl key for Italic Characters";
            // 
            // doneSplitButton
            // 
            this.doneSplitButton.Enabled = false;
            this.doneSplitButton.Location = new System.Drawing.Point(674, 422);
            this.doneSplitButton.Name = "doneSplitButton";
            this.doneSplitButton.Size = new System.Drawing.Size(107, 23);
            this.doneSplitButton.TabIndex = 20;
            this.doneSplitButton.Text = "Save Split";
            this.toolTip1.SetToolTip(this.doneSplitButton, "When one part of the block to split is correctly selected (green), save it");
            this.doneSplitButton.UseVisualStyleBackColor = true;
            this.doneSplitButton.Click += new System.EventHandler(this.doneSplitButton_Click);
            // 
            // cancelSplitButton
            // 
            this.cancelSplitButton.Enabled = false;
            this.cancelSplitButton.Location = new System.Drawing.Point(674, 451);
            this.cancelSplitButton.Name = "cancelSplitButton";
            this.cancelSplitButton.Size = new System.Drawing.Size(107, 23);
            this.cancelSplitButton.TabIndex = 21;
            this.cancelSplitButton.Text = "Cancel Split";
            this.toolTip1.SetToolTip(this.cancelSplitButton, "Cancel the split operation");
            this.cancelSplitButton.UseVisualStyleBackColor = true;
            this.cancelSplitButton.Click += new System.EventHandler(this.cancelSplitButton_Click);
            // 
            // beginSplitButton
            // 
            this.beginSplitButton.Enabled = false;
            this.beginSplitButton.Location = new System.Drawing.Point(549, 436);
            this.beginSplitButton.Name = "beginSplitButton";
            this.beginSplitButton.Size = new System.Drawing.Size(107, 23);
            this.beginSplitButton.TabIndex = 19;
            this.beginSplitButton.Text = "Split in 2";
            this.toolTip1.SetToolTip(this.beginSplitButton, "If the highlighted block contains all or part of 2 different characters, begin th" +
        "e split operation");
            this.beginSplitButton.UseVisualStyleBackColor = true;
            this.beginSplitButton.Click += new System.EventHandler(this.beginSplitButton_Click);
            // 
            // splitHelpLabel
            // 
            this.splitHelpLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.splitHelpLabel.Location = new System.Drawing.Point(546, 388);
            this.splitHelpLabel.Name = "splitHelpLabel";
            this.splitHelpLabel.Size = new System.Drawing.Size(235, 31);
            this.splitHelpLabel.TabIndex = 18;
            this.splitHelpLabel.Text = "If the hilited block(s) contains parts of 2 different characters, Split it up:";
            this.splitHelpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(22, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(532, 14);
            this.label4.TabIndex = 2;
            this.label4.Text = "If a character is in 2 or more pieces, click or use Arrow Keys to hilite them all" +
    " (up to 10) before selecting";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(538, 486);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(264, 42);
            this.label2.TabIndex = 22;
            this.label2.Text = "If the pieces look like just the outlines of characters, or if each character is " +
    "split in many parts, try a different Palette of colors:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(12, 34);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(708, 15);
            this.label5.TabIndex = 1;
            this.label5.Text = "Type or Click on the Character below which matches the darkened pattern, or use t" +
    "he Split or Palette Features";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(612, 57);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(96, 14);
            this.label6.TabIndex = 4;
            this.label6.Text = "Text OCR\'d So Far";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // undoButton
            // 
            this.undoButton.Enabled = false;
            this.undoButton.Location = new System.Drawing.Point(278, 629);
            this.undoButton.Name = "undoButton";
            this.undoButton.Size = new System.Drawing.Size(112, 23);
            this.undoButton.TabIndex = 15;
            this.undoButton.Text = "&Undo";
            this.toolTip1.SetToolTip(this.undoButton, "Undo the last OCR match or split");
            this.undoButton.UseVisualStyleBackColor = true;
            this.undoButton.Click += new System.EventHandler(this.undoButton_Click);
            // 
            // tryDifferentPaletteButton
            // 
            this.tryDifferentPaletteButton.Location = new System.Drawing.Point(612, 540);
            this.tryDifferentPaletteButton.Name = "tryDifferentPaletteButton";
            this.tryDifferentPaletteButton.Size = new System.Drawing.Size(107, 24);
            this.tryDifferentPaletteButton.TabIndex = 23;
            this.tryDifferentPaletteButton.Text = "Different Palette";
            this.toolTip1.SetToolTip(this.tryDifferentPaletteButton, "Try a different set of colors from the subtitle which show the characters best");
            this.tryDifferentPaletteButton.UseVisualStyleBackColor = true;
            this.tryDifferentPaletteButton.Click += new System.EventHandler(this.tryDifferentPaletteButton_Click);
            // 
            // subtitleIndexLabel
            // 
            this.subtitleIndexLabel.Location = new System.Drawing.Point(499, 576);
            this.subtitleIndexLabel.Name = "subtitleIndexLabel";
            this.subtitleIndexLabel.Size = new System.Drawing.Size(122, 23);
            this.subtitleIndexLabel.TabIndex = 27;
            this.subtitleIndexLabel.Text = "label7";
            this.subtitleIndexLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // startOverSubButton
            // 
            this.startOverSubButton.Location = new System.Drawing.Point(484, 622);
            this.startOverSubButton.Name = "startOverSubButton";
            this.startOverSubButton.Size = new System.Drawing.Size(111, 37);
            this.startOverSubButton.TabIndex = 16;
            this.startOverSubButton.Text = "Start Over on this Subtitle";
            this.startOverSubButton.UseVisualStyleBackColor = true;
            this.startOverSubButton.Visible = false;
            this.startOverSubButton.Click += new System.EventHandler(this.startOverSubButton_Click);
            // 
            // startOverMovieButton
            // 
            this.startOverMovieButton.Location = new System.Drawing.Point(602, 622);
            this.startOverMovieButton.Name = "startOverMovieButton";
            this.startOverMovieButton.Size = new System.Drawing.Size(111, 37);
            this.startOverMovieButton.TabIndex = 17;
            this.startOverMovieButton.Text = "Start Over for the whole Movie";
            this.startOverMovieButton.UseVisualStyleBackColor = true;
            this.startOverMovieButton.Visible = false;
            this.startOverMovieButton.Click += new System.EventHandler(this.startOverMovieButton_Click);
            // 
            // reviewButton
            // 
            this.reviewButton.Location = new System.Drawing.Point(720, 611);
            this.reviewButton.Name = "reviewButton";
            this.reviewButton.Size = new System.Drawing.Size(111, 42);
            this.reviewButton.TabIndex = 28;
            this.reviewButton.Text = "Review and Correct OCR Matches";
            this.toolTip1.SetToolTip(this.reviewButton, "View and fix any mistaken OCR matches or Splits made for this movie");
            this.reviewButton.UseVisualStyleBackColor = true;
            this.reviewButton.Click += new System.EventHandler(this.reviewButton_Click);
            // 
            // ignoreBaselineCheckBox
            // 
            this.ignoreBaselineCheckBox.AutoSize = true;
            this.ignoreBaselineCheckBox.Location = new System.Drawing.Point(274, 431);
            this.ignoreBaselineCheckBox.Name = "ignoreBaselineCheckBox";
            this.ignoreBaselineCheckBox.Size = new System.Drawing.Size(125, 17);
            this.ignoreBaselineCheckBox.TabIndex = 7;
            this.ignoreBaselineCheckBox.Text = "Ignore Baseline Here";
            this.toolTip1.SetToolTip(this.ignoreBaselineCheckBox, "Retry the OCR without checking that the highlighted character lines up vertically" +
        " with its neighbors to left and right");
            this.ignoreBaselineCheckBox.UseVisualStyleBackColor = true;
            this.ignoreBaselineCheckBox.Visible = false;
            this.ignoreBaselineCheckBox.CheckedChanged += new System.EventHandler(this.ignoreBaselineCheckBox_CheckedChanged);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(627, 576);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(195, 23);
            this.progressBar1.TabIndex = 26;
            // 
            // retryButton
            // 
            this.retryButton.Location = new System.Drawing.Point(488, 366);
            this.retryButton.Name = "retryButton";
            this.retryButton.Size = new System.Drawing.Size(55, 55);
            this.retryButton.TabIndex = 25;
            this.retryButton.Text = "Retry (Debug)";
            this.retryButton.UseVisualStyleBackColor = true;
            this.retryButton.Visible = false;
            this.retryButton.Click += new System.EventHandler(this.retryButton_Click);
            // 
            // paletteIndexLabel
            // 
            this.paletteIndexLabel.AutoSize = true;
            this.paletteIndexLabel.Location = new System.Drawing.Point(725, 546);
            this.paletteIndexLabel.Name = "paletteIndexLabel";
            this.paletteIndexLabel.Size = new System.Drawing.Size(35, 13);
            this.paletteIndexLabel.TabIndex = 24;
            this.paletteIndexLabel.Text = "label1";
            // 
            // commentButton
            // 
            this.commentButton.Location = new System.Drawing.Point(20, 630);
            this.commentButton.Name = "commentButton";
            this.commentButton.Size = new System.Drawing.Size(112, 23);
            this.commentButton.TabIndex = 13;
            this.commentButton.Text = "Comment";
            this.toolTip1.SetToolTip(this.commentButton, "Add a comment at this point in the final subtitle file output");
            this.commentButton.UseVisualStyleBackColor = true;
            this.commentButton.Click += new System.EventHandler(this.commentButton_Click);
            // 
            // dvdLabel
            // 
            this.dvdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dvdLabel.Location = new System.Drawing.Point(65, 6);
            this.dvdLabel.Name = "dvdLabel";
            this.dvdLabel.Size = new System.Drawing.Size(531, 23);
            this.dvdLabel.TabIndex = 0;
            this.dvdLabel.Text = "label2";
            this.dvdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // manualEntryTextBox
            // 
            this.manualEntryTextBox.Location = new System.Drawing.Point(151, 603);
            this.manualEntryTextBox.MaxLength = 1;
            this.manualEntryTextBox.Name = "manualEntryTextBox";
            this.manualEntryTextBox.Size = new System.Drawing.Size(34, 20);
            this.manualEntryTextBox.TabIndex = 10;
            this.manualEntryTextBox.TextChanged += new System.EventHandler(this.manualEntryTextBox_TextChanged);
            this.manualEntryTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.manualEntryTextBox_KeyDown);
            // 
            // manualCharLabel
            // 
            this.manualCharLabel.AutoSize = true;
            this.manualCharLabel.Location = new System.Drawing.Point(19, 606);
            this.manualCharLabel.Name = "manualCharLabel";
            this.manualCharLabel.Size = new System.Drawing.Size(129, 13);
            this.manualCharLabel.TabIndex = 9;
            this.manualCharLabel.Text = "Manually Enter Character:";
            // 
            // manualItalicCheckBox
            // 
            this.manualItalicCheckBox.AutoSize = true;
            this.manualItalicCheckBox.Location = new System.Drawing.Point(191, 605);
            this.manualItalicCheckBox.Name = "manualItalicCheckBox";
            this.manualItalicCheckBox.Size = new System.Drawing.Size(48, 17);
            this.manualItalicCheckBox.TabIndex = 30;
            this.manualItalicCheckBox.Text = "Italic";
            this.toolTip1.SetToolTip(this.manualItalicCheckBox, "Determines whether a manually entered character is Italic or not");
            this.manualItalicCheckBox.UseVisualStyleBackColor = true;
            this.manualItalicCheckBox.CheckedChanged += new System.EventHandler(this.manualItalicCheckBox_CheckedChanged);
            // 
            // manualWaitEnterCheckBox
            // 
            this.manualWaitEnterCheckBox.AutoSize = true;
            this.manualWaitEnterCheckBox.Location = new System.Drawing.Point(386, 605);
            this.manualWaitEnterCheckBox.Name = "manualWaitEnterCheckBox";
            this.manualWaitEnterCheckBox.Size = new System.Drawing.Size(112, 17);
            this.manualWaitEnterCheckBox.TabIndex = 37;
            this.manualWaitEnterCheckBox.Text = "Wait for Enter Key";
            this.toolTip1.SetToolTip(this.manualWaitEnterCheckBox, "Determines whether a manually entered character is Italic or not");
            this.manualWaitEnterCheckBox.UseVisualStyleBackColor = true;
            this.manualWaitEnterCheckBox.CheckedChanged += new System.EventHandler(this.manualWaitEnterCheckBox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(235, 606);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(145, 13);
            this.label1.TabIndex = 29;
            this.label1.Text = "(Toggle with Space key)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(391, 634);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(78, 13);
            this.label7.TabIndex = 33;
            this.label7.Text = "(Backspace)";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(498, 606);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(99, 13);
            this.label8.TabIndex = 36;
            this.label8.Text = "(Toggle with F2)";
            // 
            // matchSoFarView
            // 
            this.matchSoFarView.BackColor = System.Drawing.Color.Sienna;
            this.matchSoFarView.ForeColor = System.Drawing.Color.Aquamarine;
            this.matchSoFarView.Location = new System.Drawing.Point(490, 76);
            this.matchSoFarView.Name = "matchSoFarView";
            this.matchSoFarView.Size = new System.Drawing.Size(342, 284);
            this.matchSoFarView.TabIndex = 5;
            this.matchSoFarView.TabStop = false;
            // 
            // blockViewer
            // 
            this.blockViewer.Location = new System.Drawing.Point(9, 76);
            this.blockViewer.Message = null;
            this.blockViewer.MovieId = 0;
            this.blockViewer.Name = "blockViewer";
            this.blockViewer.Size = new System.Drawing.Size(473, 346);
            this.blockViewer.TabIndex = 3;
            this.blockViewer.EncodeClicked += new System.EventHandler<DvdSubOcr.BlockViewer.BlockEncodeArgs>(this.blockViewer_EncodeClicked);
            // 
            // characterSelector
            // 
            this.characterSelector.Italics = false;
            this.characterSelector.Location = new System.Drawing.Point(9, 453);
            this.characterSelector.Name = "characterSelector";
            this.characterSelector.Size = new System.Drawing.Size(473, 144);
            this.characterSelector.TabIndex = 8;
            this.characterSelector.SelectedCharacterChanged += new System.EventHandler<DvdSubOcr.SelectedCharacterArgs>(this.characterSelector_SelectedCharacterChanged);
            // 
            // triangleFill4
            // 
            this.triangleFill4.FillColor = System.Drawing.Color.WhiteSmoke;
            this.triangleFill4.ForeColor = System.Drawing.Color.SteelBlue;
            this.triangleFill4.Location = new System.Drawing.Point(9, 69);
            this.triangleFill4.Name = "triangleFill4";
            this.triangleFill4.Origin = DvdSubExtractor.Corner.BottomLeft;
            this.triangleFill4.Size = new System.Drawing.Size(473, 7);
            this.triangleFill4.TabIndex = 35;
            this.toolTip1.SetToolTip(this.triangleFill4, "Indicates manual entry is Italic");
            this.triangleFill4.Visible = false;
            // 
            // triangleFill3
            // 
            this.triangleFill3.FillColor = System.Drawing.Color.WhiteSmoke;
            this.triangleFill3.ForeColor = System.Drawing.Color.SteelBlue;
            this.triangleFill3.Location = new System.Drawing.Point(9, 422);
            this.triangleFill3.Name = "triangleFill3";
            this.triangleFill3.Origin = DvdSubExtractor.Corner.TopRight;
            this.triangleFill3.Size = new System.Drawing.Size(473, 7);
            this.triangleFill3.TabIndex = 34;
            this.toolTip1.SetToolTip(this.triangleFill3, "Indicates manual entry is Italic");
            this.triangleFill3.Visible = false;
            // 
            // triangleFill2
            // 
            this.triangleFill2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.triangleFill2.FillColor = System.Drawing.Color.WhiteSmoke;
            this.triangleFill2.ForeColor = System.Drawing.Color.SteelBlue;
            this.triangleFill2.Location = new System.Drawing.Point(2, 76);
            this.triangleFill2.Name = "triangleFill2";
            this.triangleFill2.Origin = DvdSubExtractor.Corner.BottomRight;
            this.triangleFill2.Size = new System.Drawing.Size(7, 346);
            this.triangleFill2.TabIndex = 32;
            this.toolTip1.SetToolTip(this.triangleFill2, "Indicates manual entry is Italic");
            this.triangleFill2.Visible = false;
            // 
            // triangleFill1
            // 
            this.triangleFill1.FillColor = System.Drawing.Color.WhiteSmoke;
            this.triangleFill1.ForeColor = System.Drawing.Color.SteelBlue;
            this.triangleFill1.Location = new System.Drawing.Point(482, 76);
            this.triangleFill1.Name = "triangleFill1";
            this.triangleFill1.Size = new System.Drawing.Size(7, 346);
            this.triangleFill1.TabIndex = 31;
            this.toolTip1.SetToolTip(this.triangleFill1, "Indicates manual entry is Italic");
            this.triangleFill1.Visible = false;
            // 
            // OcrBlocksStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.manualWaitEnterCheckBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.triangleFill4);
            this.Controls.Add(this.triangleFill3);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.triangleFill2);
            this.Controls.Add(this.triangleFill1);
            this.Controls.Add(this.manualItalicCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.manualCharLabel);
            this.Controls.Add(this.manualEntryTextBox);
            this.Controls.Add(this.dvdLabel);
            this.Controls.Add(this.commentButton);
            this.Controls.Add(this.paletteIndexLabel);
            this.Controls.Add(this.retryButton);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.ignoreBaselineCheckBox);
            this.Controls.Add(this.reviewButton);
            this.Controls.Add(this.splitHelpLabel);
            this.Controls.Add(this.tryDifferentPaletteButton);
            this.Controls.Add(this.startOverMovieButton);
            this.Controls.Add(this.beginSplitButton);
            this.Controls.Add(this.startOverSubButton);
            this.Controls.Add(this.cancelSplitButton);
            this.Controls.Add(this.matchSoFarView);
            this.Controls.Add(this.doneSplitButton);
            this.Controls.Add(this.subtitleIndexLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.undoButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.unknownCharacterButton);
            this.Controls.Add(this.blockViewer);
            this.Controls.Add(this.characterSelector);
            this.Name = "OcrBlocksStep";
            this.Size = new System.Drawing.Size(842, 662);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DvdSubOcr.CharacterSelector characterSelector;
        private DvdSubOcr.BlockViewer blockViewer;
        private System.Windows.Forms.Button unknownCharacterButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button doneSplitButton;
        private System.Windows.Forms.Button cancelSplitButton;
        private System.Windows.Forms.Button beginSplitButton;
        private System.Windows.Forms.Label splitHelpLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button undoButton;
        private System.Windows.Forms.Button tryDifferentPaletteButton;
        private System.Windows.Forms.Label subtitleIndexLabel;
        private DvdSubOcr.MatchSoFarView matchSoFarView;
        private System.Windows.Forms.Button startOverSubButton;
        private System.Windows.Forms.Button startOverMovieButton;
        private System.Windows.Forms.Button reviewButton;
        private System.Windows.Forms.CheckBox ignoreBaselineCheckBox;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button retryButton;
        private System.Windows.Forms.Label paletteIndexLabel;
        private System.Windows.Forms.Button commentButton;
        private System.Windows.Forms.Label dvdLabel;
        private System.Windows.Forms.TextBox manualEntryTextBox;
        private System.Windows.Forms.Label manualCharLabel;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox manualItalicCheckBox;
        private TriangleFill triangleFill1;
        private TriangleFill triangleFill2;
        private System.Windows.Forms.Label label7;
        private TriangleFill triangleFill3;
        private TriangleFill triangleFill4;
        private System.Windows.Forms.CheckBox manualWaitEnterCheckBox;
        private System.Windows.Forms.Label label8;
    }
}
