namespace DvdSubExtractor
{
    partial class CreateSubtitleFileStep
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
            this.subtitleFileLabel = new System.Windows.Forms.Label();
            this.srtSaveButton = new System.Windows.Forms.Button();
            this.dvdLabel = new System.Windows.Forms.Label();
            this.srtEditButton = new System.Windows.Forms.Button();
            this.nextTitleButton = new System.Windows.Forms.Button();
            this.sectionListLabel = new System.Windows.Forms.Label();
            this.sectionListTextBox = new System.Windows.Forms.RichTextBox();
            this.initialAdjustmentUpDown = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.wordSpacingButton = new System.Windows.Forms.Button();
            this.leftCropLabel = new System.Windows.Forms.Label();
            this.leftCropUpDown = new System.Windows.Forms.NumericUpDown();
            this.topCropLabel = new System.Windows.Forms.Label();
            this.topCropUpDown = new System.Windows.Forms.NumericUpDown();
            this._1080pCheckBox = new System.Windows.Forms.CheckBox();
            this.slow2524CheckBox = new System.Windows.Forms.CheckBox();
            this.removeSdhSubsCheck = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.saveAsButton = new System.Windows.Forms.Button();
            this.saveAsFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.srtRadioButton = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.assRadioButton = new System.Windows.Forms.RadioButton();
            this.normalLinesButton = new System.Windows.Forms.RadioButton();
            this.dvdLineBreaksButton = new System.Windows.Forms.RadioButton();
            this.lineBreaksGroupBox = new System.Windows.Forms.GroupBox();
            this.exactPositionButton = new System.Windows.Forms.RadioButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.saveProgressBar = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.initialAdjustmentUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftCropUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.topCropUpDown)).BeginInit();
            this.panel1.SuspendLayout();
            this.lineBreaksGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // subtitleFileLabel
            // 
            this.subtitleFileLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.subtitleFileLabel.Location = new System.Drawing.Point(152, 106);
            this.subtitleFileLabel.Name = "subtitleFileLabel";
            this.subtitleFileLabel.Size = new System.Drawing.Size(506, 23);
            this.subtitleFileLabel.TabIndex = 3;
            this.subtitleFileLabel.Text = "label1";
            this.subtitleFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.subtitleFileLabel, "File name to be create with Save button");
            // 
            // srtSaveButton
            // 
            this.srtSaveButton.Location = new System.Drawing.Point(197, 143);
            this.srtSaveButton.Name = "srtSaveButton";
            this.srtSaveButton.Size = new System.Drawing.Size(117, 23);
            this.srtSaveButton.TabIndex = 4;
            this.srtSaveButton.Text = "Save";
            this.toolTip1.SetToolTip(this.srtSaveButton, "Create the subtitle file");
            this.srtSaveButton.UseVisualStyleBackColor = true;
            this.srtSaveButton.Click += new System.EventHandler(this.createSubtitleFileButton_Click);
            // 
            // dvdLabel
            // 
            this.dvdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dvdLabel.Location = new System.Drawing.Point(149, 16);
            this.dvdLabel.Name = "dvdLabel";
            this.dvdLabel.Size = new System.Drawing.Size(506, 23);
            this.dvdLabel.TabIndex = 0;
            this.dvdLabel.Text = "label2";
            this.dvdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // srtEditButton
            // 
            this.srtEditButton.Location = new System.Drawing.Point(497, 143);
            this.srtEditButton.Name = "srtEditButton";
            this.srtEditButton.Size = new System.Drawing.Size(117, 23);
            this.srtEditButton.TabIndex = 6;
            this.srtEditButton.Text = "Edit";
            this.toolTip1.SetToolTip(this.srtEditButton, "Open the subtitle file with the editor specified in Options");
            this.srtEditButton.UseVisualStyleBackColor = true;
            this.srtEditButton.Click += new System.EventHandler(this.openSubtitleFileInEditorButton_Click);
            // 
            // nextTitleButton
            // 
            this.nextTitleButton.Location = new System.Drawing.Point(67, 329);
            this.nextTitleButton.Name = "nextTitleButton";
            this.nextTitleButton.Size = new System.Drawing.Size(155, 48);
            this.nextTitleButton.TabIndex = 17;
            this.nextTitleButton.Text = "OCR Next Title";
            this.toolTip1.SetToolTip(this.nextTitleButton, "Jump to the OCR step for the next track or subtitle file selected");
            this.nextTitleButton.UseVisualStyleBackColor = true;
            this.nextTitleButton.Click += new System.EventHandler(this.nextTitleButton_Click);
            // 
            // sectionListLabel
            // 
            this.sectionListLabel.AutoSize = true;
            this.sectionListLabel.Location = new System.Drawing.Point(397, 299);
            this.sectionListLabel.Name = "sectionListLabel";
            this.sectionListLabel.Size = new System.Drawing.Size(272, 13);
            this.sectionListLabel.TabIndex = 19;
            this.sectionListLabel.Text = "Section List (Comparing Mpeg and DGIndex timestamps)";
            // 
            // sectionListTextBox
            // 
            this.sectionListTextBox.Location = new System.Drawing.Point(295, 317);
            this.sectionListTextBox.Name = "sectionListTextBox";
            this.sectionListTextBox.ReadOnly = true;
            this.sectionListTextBox.Size = new System.Drawing.Size(468, 328);
            this.sectionListTextBox.TabIndex = 20;
            this.sectionListTextBox.Text = "";
            // 
            // initialAdjustmentUpDown
            // 
            this.initialAdjustmentUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.initialAdjustmentUpDown.Location = new System.Drawing.Point(185, 207);
            this.initialAdjustmentUpDown.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.initialAdjustmentUpDown.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            -2147483648});
            this.initialAdjustmentUpDown.Name = "initialAdjustmentUpDown";
            this.initialAdjustmentUpDown.Size = new System.Drawing.Size(58, 20);
            this.initialAdjustmentUpDown.TabIndex = 8;
            this.toolTip1.SetToolTip(this.initialAdjustmentUpDown, "Adjust the timing of the subtitles");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(85, 209);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Subtitle Offset (ms)";
            // 
            // wordSpacingButton
            // 
            this.wordSpacingButton.Location = new System.Drawing.Point(67, 413);
            this.wordSpacingButton.Name = "wordSpacingButton";
            this.wordSpacingButton.Size = new System.Drawing.Size(155, 48);
            this.wordSpacingButton.TabIndex = 18;
            this.wordSpacingButton.Text = "Advanced Word Spacing Adjustment";
            this.toolTip1.SetToolTip(this.wordSpacingButton, "Go to the word spacing adjustment page to correct spacing errors with certain cha" +
        "racters");
            this.wordSpacingButton.UseVisualStyleBackColor = true;
            this.wordSpacingButton.Click += new System.EventHandler(this.wordSpacingButton_Click);
            // 
            // leftCropLabel
            // 
            this.leftCropLabel.AutoSize = true;
            this.leftCropLabel.Location = new System.Drawing.Point(85, 237);
            this.leftCropLabel.Name = "leftCropLabel";
            this.leftCropLabel.Size = new System.Drawing.Size(85, 13);
            this.leftCropLabel.TabIndex = 9;
            this.leftCropLabel.Text = "Left Crop (pixels)";
            // 
            // leftCropUpDown
            // 
            this.leftCropUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.leftCropUpDown.Location = new System.Drawing.Point(197, 235);
            this.leftCropUpDown.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.leftCropUpDown.Name = "leftCropUpDown";
            this.leftCropUpDown.Size = new System.Drawing.Size(46, 20);
            this.leftCropUpDown.TabIndex = 10;
            this.toolTip1.SetToolTip(this.leftCropUpDown, "Adjust the vertical position of any subtitles with a position tag");
            // 
            // topCropLabel
            // 
            this.topCropLabel.AutoSize = true;
            this.topCropLabel.Location = new System.Drawing.Point(85, 264);
            this.topCropLabel.Name = "topCropLabel";
            this.topCropLabel.Size = new System.Drawing.Size(86, 13);
            this.topCropLabel.TabIndex = 11;
            this.topCropLabel.Text = "Top Crop (pixels)";
            // 
            // topCropUpDown
            // 
            this.topCropUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.topCropUpDown.Location = new System.Drawing.Point(197, 262);
            this.topCropUpDown.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.topCropUpDown.Minimum = new decimal(new int[] {
            500,
            0,
            0,
            -2147483648});
            this.topCropUpDown.Name = "topCropUpDown";
            this.topCropUpDown.Size = new System.Drawing.Size(46, 20);
            this.topCropUpDown.TabIndex = 12;
            this.toolTip1.SetToolTip(this.topCropUpDown, "Adjust the horizontal position of any subtitles with a position tag");
            // 
            // _1080pCheckBox
            // 
            this._1080pCheckBox.AutoSize = true;
            this._1080pCheckBox.Location = new System.Drawing.Point(301, 265);
            this._1080pCheckBox.Name = "_1080pCheckBox";
            this._1080pCheckBox.Size = new System.Drawing.Size(209, 17);
            this._1080pCheckBox.TabIndex = 15;
            this._1080pCheckBox.Text = "1080p Adjustments (2x Font && Margins)";
            this.toolTip1.SetToolTip(this._1080pCheckBox, "Double the font size and margins set in Options (done by default for SRT files) b" +
        "ecause Bluray subtitles are much larger than DVD");
            this._1080pCheckBox.UseVisualStyleBackColor = true;
            // 
            // slow2524CheckBox
            // 
            this.slow2524CheckBox.AutoSize = true;
            this.slow2524CheckBox.Location = new System.Drawing.Point(301, 238);
            this.slow2524CheckBox.Name = "slow2524CheckBox";
            this.slow2524CheckBox.Size = new System.Drawing.Size(102, 17);
            this.slow2524CheckBox.TabIndex = 14;
            this.slow2524CheckBox.Text = "Slow 25->24 fps";
            this.toolTip1.SetToolTip(this.slow2524CheckBox, "Scale the time stamps of the subtitles to match the slowing of video and audio by" +
        " 23.976/25 (PAL -> NTSC)");
            this.slow2524CheckBox.UseVisualStyleBackColor = true;
            // 
            // removeSdhSubsCheck
            // 
            this.removeSdhSubsCheck.AutoSize = true;
            this.removeSdhSubsCheck.Location = new System.Drawing.Point(301, 210);
            this.removeSdhSubsCheck.Name = "removeSdhSubsCheck";
            this.removeSdhSubsCheck.Size = new System.Drawing.Size(117, 17);
            this.removeSdhSubsCheck.TabIndex = 13;
            this.removeSdhSubsCheck.Text = "Remove SDH subs";
            this.toolTip1.SetToolTip(this.removeSdhSubsCheck, "Remove the Deaf and Hard of hearing text from the subtitles (typically in parenth" +
        "eses)");
            this.removeSdhSubsCheck.UseVisualStyleBackColor = true;
            this.removeSdhSubsCheck.CheckedChanged += new System.EventHandler(this.removeSdhSubsCheck_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(199, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Output Format:";
            // 
            // saveAsButton
            // 
            this.saveAsButton.Location = new System.Drawing.Point(347, 143);
            this.saveAsButton.Name = "saveAsButton";
            this.saveAsButton.Size = new System.Drawing.Size(117, 23);
            this.saveAsButton.TabIndex = 5;
            this.saveAsButton.Text = "Save As...";
            this.toolTip1.SetToolTip(this.saveAsButton, "Create the subtitle file after choosing the file name");
            this.saveAsButton.UseVisualStyleBackColor = true;
            this.saveAsButton.Click += new System.EventHandler(this.saveAsButton_Click);
            // 
            // saveAsFileDialog
            // 
            this.saveAsFileDialog.Filter = "Subtitle files|*.srt;*.ass|All files|*.*";
            // 
            // srtRadioButton
            // 
            this.srtRadioButton.AutoSize = true;
            this.srtRadioButton.Location = new System.Drawing.Point(16, 6);
            this.srtRadioButton.Name = "srtRadioButton";
            this.srtRadioButton.Size = new System.Drawing.Size(91, 17);
            this.srtRadioButton.TabIndex = 0;
            this.srtRadioButton.TabStop = true;
            this.srtRadioButton.Text = "SRT (SubRip)";
            this.toolTip1.SetToolTip(this.srtRadioButton, "Create a Subrip (SRT) text file with Save or Save As.");
            this.srtRadioButton.UseVisualStyleBackColor = true;
            this.srtRadioButton.CheckedChanged += new System.EventHandler(this.subtitleStyle_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.assRadioButton);
            this.panel1.Controls.Add(this.srtRadioButton);
            this.panel1.Location = new System.Drawing.Point(282, 42);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(265, 53);
            this.panel1.TabIndex = 2;
            // 
            // assRadioButton
            // 
            this.assRadioButton.AutoSize = true;
            this.assRadioButton.Location = new System.Drawing.Point(16, 31);
            this.assRadioButton.Name = "assRadioButton";
            this.assRadioButton.Size = new System.Drawing.Size(189, 17);
            this.assRadioButton.TabIndex = 1;
            this.assRadioButton.TabStop = true;
            this.assRadioButton.Text = "ASS (Advanced SubStation Alpha)";
            this.toolTip1.SetToolTip(this.assRadioButton, "Create a Substation Alpha (ASS) text file with Save or Save As.");
            this.assRadioButton.UseVisualStyleBackColor = true;
            // 
            // normalLinesButton
            // 
            this.normalLinesButton.AutoSize = true;
            this.normalLinesButton.Location = new System.Drawing.Point(11, 21);
            this.normalLinesButton.Name = "normalLinesButton";
            this.normalLinesButton.Size = new System.Drawing.Size(135, 17);
            this.normalLinesButton.TabIndex = 0;
            this.normalLinesButton.TabStop = true;
            this.normalLinesButton.Text = "SubExtractor Optimized";
            this.toolTip1.SetToolTip(this.normalLinesButton, "Text at the bottom center of the screen uses the font size set in Options and som" +
        "e line breaks are removed so that text flows up to the margins");
            this.normalLinesButton.UseVisualStyleBackColor = true;
            this.normalLinesButton.CheckedChanged += new System.EventHandler(this.normalLinesButton_CheckedChanged);
            // 
            // dvdLineBreaksButton
            // 
            this.dvdLineBreaksButton.AutoSize = true;
            this.dvdLineBreaksButton.Location = new System.Drawing.Point(11, 47);
            this.dvdLineBreaksButton.Name = "dvdLineBreaksButton";
            this.dvdLineBreaksButton.Size = new System.Drawing.Size(146, 17);
            this.dvdLineBreaksButton.TabIndex = 1;
            this.dvdLineBreaksButton.TabStop = true;
            this.dvdLineBreaksButton.Text = "Keep Source Line Breaks";
            this.toolTip1.SetToolTip(this.dvdLineBreaksButton, "Text at the bottom center of the screen keeps the line breaks of the original but" +
        " with the font size and margins set in Options");
            this.dvdLineBreaksButton.UseVisualStyleBackColor = true;
            this.dvdLineBreaksButton.CheckedChanged += new System.EventHandler(this.dvdLineBreaksButton_CheckedChanged);
            // 
            // lineBreaksGroupBox
            // 
            this.lineBreaksGroupBox.Controls.Add(this.exactPositionButton);
            this.lineBreaksGroupBox.Controls.Add(this.dvdLineBreaksButton);
            this.lineBreaksGroupBox.Controls.Add(this.normalLinesButton);
            this.lineBreaksGroupBox.Location = new System.Drawing.Point(547, 190);
            this.lineBreaksGroupBox.Name = "lineBreaksGroupBox";
            this.lineBreaksGroupBox.Size = new System.Drawing.Size(216, 100);
            this.lineBreaksGroupBox.TabIndex = 21;
            this.lineBreaksGroupBox.TabStop = false;
            this.lineBreaksGroupBox.Text = "Line Breaks and Positions";
            // 
            // exactPositionButton
            // 
            this.exactPositionButton.AutoSize = true;
            this.exactPositionButton.Location = new System.Drawing.Point(11, 73);
            this.exactPositionButton.Name = "exactPositionButton";
            this.exactPositionButton.Size = new System.Drawing.Size(181, 17);
            this.exactPositionButton.TabIndex = 2;
            this.exactPositionButton.TabStop = true;
            this.exactPositionButton.Text = "Keep Source Lines and Positions";
            this.toolTip1.SetToolTip(this.exactPositionButton, "Text at the bottom center of the screen keeps the same line breaks, positioning a" +
        "nd font size as the original subtitles");
            this.exactPositionButton.UseVisualStyleBackColor = true;
            this.exactPositionButton.CheckedChanged += new System.EventHandler(this.exactPositionButton_CheckedChanged);
            // 
            // saveProgressBar
            // 
            this.saveProgressBar.Location = new System.Drawing.Point(197, 175);
            this.saveProgressBar.Name = "saveProgressBar";
            this.saveProgressBar.Size = new System.Drawing.Size(267, 23);
            this.saveProgressBar.TabIndex = 22;
            this.saveProgressBar.Visible = false;
            // 
            // CreateSubtitleFileStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.saveProgressBar);
            this.Controls.Add(this.lineBreaksGroupBox);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.saveAsButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.removeSdhSubsCheck);
            this.Controls.Add(this.slow2524CheckBox);
            this.Controls.Add(this._1080pCheckBox);
            this.Controls.Add(this.topCropLabel);
            this.Controls.Add(this.topCropUpDown);
            this.Controls.Add(this.leftCropLabel);
            this.Controls.Add(this.leftCropUpDown);
            this.Controls.Add(this.wordSpacingButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.initialAdjustmentUpDown);
            this.Controls.Add(this.sectionListTextBox);
            this.Controls.Add(this.sectionListLabel);
            this.Controls.Add(this.nextTitleButton);
            this.Controls.Add(this.srtEditButton);
            this.Controls.Add(this.dvdLabel);
            this.Controls.Add(this.srtSaveButton);
            this.Controls.Add(this.subtitleFileLabel);
            this.Name = "CreateSubtitleFileStep";
            this.Size = new System.Drawing.Size(842, 662);
            ((System.ComponentModel.ISupportInitialize)(this.initialAdjustmentUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftCropUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.topCropUpDown)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.lineBreaksGroupBox.ResumeLayout(false);
            this.lineBreaksGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label subtitleFileLabel;
        private System.Windows.Forms.Button srtSaveButton;
        private System.Windows.Forms.Label dvdLabel;
        private System.Windows.Forms.Button srtEditButton;
        private System.Windows.Forms.Button nextTitleButton;
        private System.Windows.Forms.Label sectionListLabel;
        private System.Windows.Forms.RichTextBox sectionListTextBox;
        private System.Windows.Forms.NumericUpDown initialAdjustmentUpDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button wordSpacingButton;
        private System.Windows.Forms.Label leftCropLabel;
        private System.Windows.Forms.NumericUpDown leftCropUpDown;
        private System.Windows.Forms.Label topCropLabel;
        private System.Windows.Forms.NumericUpDown topCropUpDown;
        private System.Windows.Forms.CheckBox _1080pCheckBox;
        private System.Windows.Forms.CheckBox slow2524CheckBox;
        private System.Windows.Forms.CheckBox removeSdhSubsCheck;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button saveAsButton;
        private System.Windows.Forms.SaveFileDialog saveAsFileDialog;
        private System.Windows.Forms.RadioButton srtRadioButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton assRadioButton;
        private System.Windows.Forms.RadioButton normalLinesButton;
        private System.Windows.Forms.RadioButton dvdLineBreaksButton;
        private System.Windows.Forms.GroupBox lineBreaksGroupBox;
        private System.Windows.Forms.RadioButton exactPositionButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ProgressBar saveProgressBar;
    }
}
