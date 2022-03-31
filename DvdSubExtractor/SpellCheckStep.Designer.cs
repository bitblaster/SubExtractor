namespace DvdSubExtractor
{
    partial class SpellCheckStep
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
            this.statusLabel = new System.Windows.Forms.Label();
            this.spellingListBox = new System.Windows.Forms.ListBox();
            this.undoButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.contextLineLabel = new System.Windows.Forms.Label();
            this.dvdLabel = new System.Windows.Forms.Label();
            this.indexProgressBar = new System.Windows.Forms.ProgressBar();
            this.indexLabel = new System.Windows.Forms.Label();
            this.messageLabel = new System.Windows.Forms.Label();
            this.skipSpellingButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.wordSpacingButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // statusLabel
            // 
            this.statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusLabel.Location = new System.Drawing.Point(24, 62);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(681, 23);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "label1";
            // 
            // spellingListBox
            // 
            this.spellingListBox.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spellingListBox.FormattingEnabled = true;
            this.spellingListBox.ItemHeight = 18;
            this.spellingListBox.Location = new System.Drawing.Point(80, 130);
            this.spellingListBox.Name = "spellingListBox";
            this.spellingListBox.Size = new System.Drawing.Size(441, 346);
            this.spellingListBox.TabIndex = 1;
            this.toolTip1.SetToolTip(this.spellingListBox, "List of possible spellings of the unknown word");
            this.spellingListBox.SelectedIndexChanged += new System.EventHandler(this.spellingListBox_SelectedIndexChanged);
            // 
            // undoButton
            // 
            this.undoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.undoButton.Enabled = false;
            this.undoButton.Location = new System.Drawing.Point(265, 496);
            this.undoButton.Name = "undoButton";
            this.undoButton.Size = new System.Drawing.Size(90, 42);
            this.undoButton.TabIndex = 19;
            this.undoButton.Text = "&Undo";
            this.toolTip1.SetToolTip(this.undoButton, "Undo the last selected spelling choice");
            this.undoButton.UseVisualStyleBackColor = true;
            this.undoButton.Click += new System.EventHandler(this.undoButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Context:";
            // 
            // contextLineLabel
            // 
            this.contextLineLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.contextLineLabel.Font = new System.Drawing.Font("Courier New", 12F);
            this.contextLineLabel.Location = new System.Drawing.Point(80, 96);
            this.contextLineLabel.Name = "contextLineLabel";
            this.contextLineLabel.Size = new System.Drawing.Size(667, 23);
            this.contextLineLabel.TabIndex = 21;
            this.contextLineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dvdLabel
            // 
            this.dvdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dvdLabel.Location = new System.Drawing.Point(80, 23);
            this.dvdLabel.Name = "dvdLabel";
            this.dvdLabel.Size = new System.Drawing.Size(690, 23);
            this.dvdLabel.TabIndex = 22;
            this.dvdLabel.Text = "label2";
            this.dvdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // indexProgressBar
            // 
            this.indexProgressBar.Location = new System.Drawing.Point(559, 130);
            this.indexProgressBar.Name = "indexProgressBar";
            this.indexProgressBar.Size = new System.Drawing.Size(188, 23);
            this.indexProgressBar.TabIndex = 23;
            // 
            // indexLabel
            // 
            this.indexLabel.AutoSize = true;
            this.indexLabel.Location = new System.Drawing.Point(568, 163);
            this.indexLabel.Name = "indexLabel";
            this.indexLabel.Size = new System.Drawing.Size(35, 13);
            this.indexLabel.TabIndex = 24;
            this.indexLabel.Text = "label2";
            // 
            // messageLabel
            // 
            this.messageLabel.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.messageLabel.ForeColor = System.Drawing.Color.Blue;
            this.messageLabel.Location = new System.Drawing.Point(559, 186);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(188, 23);
            this.messageLabel.TabIndex = 38;
            this.messageLabel.Text = "label1";
            this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // skipSpellingButton
            // 
            this.skipSpellingButton.Location = new System.Drawing.Point(584, 264);
            this.skipSpellingButton.Name = "skipSpellingButton";
            this.skipSpellingButton.Size = new System.Drawing.Size(155, 36);
            this.skipSpellingButton.TabIndex = 39;
            this.skipSpellingButton.Text = "Skip All Spellchecking";
            this.toolTip1.SetToolTip(this.skipSpellingButton, "Skip the rest of the spellchecking on this track");
            this.skipSpellingButton.UseVisualStyleBackColor = true;
            this.skipSpellingButton.Click += new System.EventHandler(this.skipSpellingButton_Click);
            // 
            // wordSpacingButton
            // 
            this.wordSpacingButton.Location = new System.Drawing.Point(584, 352);
            this.wordSpacingButton.Name = "wordSpacingButton";
            this.wordSpacingButton.Size = new System.Drawing.Size(155, 48);
            this.wordSpacingButton.TabIndex = 40;
            this.wordSpacingButton.Text = "Advanced Word Spacing Adjustment";
            this.toolTip1.SetToolTip(this.wordSpacingButton, "Go to the word spacing adjustment page to correct spacing errors with certain cha" +
        "racters");
            this.wordSpacingButton.UseVisualStyleBackColor = true;
            this.wordSpacingButton.Click += new System.EventHandler(this.wordSpacingButton_Click);
            // 
            // SpellCheckStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.wordSpacingButton);
            this.Controls.Add(this.skipSpellingButton);
            this.Controls.Add(this.messageLabel);
            this.Controls.Add(this.indexLabel);
            this.Controls.Add(this.indexProgressBar);
            this.Controls.Add(this.dvdLabel);
            this.Controls.Add(this.contextLineLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.undoButton);
            this.Controls.Add(this.spellingListBox);
            this.Controls.Add(this.statusLabel);
            this.Name = "SpellCheckStep";
            this.Size = new System.Drawing.Size(842, 662);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.ListBox spellingListBox;
        private System.Windows.Forms.Button undoButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label contextLineLabel;
        private System.Windows.Forms.Label dvdLabel;
        private System.Windows.Forms.ProgressBar indexProgressBar;
        private System.Windows.Forms.Label indexLabel;
        private System.Windows.Forms.Label messageLabel;
        private System.Windows.Forms.Button skipSpellingButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button wordSpacingButton;

    }
}
