namespace DvdSubExtractor
{
    partial class RunExtractorStep
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
            this.titleListBox = new System.Windows.Forms.ListBox();
            this.subtitleDataFileLabel = new System.Windows.Forms.Label();
            this.createSubtitleDataCheckBox = new System.Windows.Forms.CheckBox();
            this.createMpegFileCheckBox = new System.Windows.Forms.CheckBox();
            this.mpegFileLabel = new System.Windows.Forms.Label();
            this.dvdLabel = new System.Windows.Forms.Label();
            this.encodeAllButton = new System.Windows.Forms.Button();
            this.saveUpdatesLabel = new System.Windows.Forms.Label();
            this.cancelSaveButton = new System.Windows.Forms.Button();
            this.encodeSelectedButton = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.createD2vCheckBox = new System.Windows.Forms.CheckBox();
            this.d2vFileLabel = new System.Windows.Forms.Label();
            this.messageLabel = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // titleListBox
            // 
            this.titleListBox.FormattingEnabled = true;
            this.titleListBox.Location = new System.Drawing.Point(44, 70);
            this.titleListBox.Name = "titleListBox";
            this.titleListBox.Size = new System.Drawing.Size(727, 134);
            this.titleListBox.TabIndex = 1;
            this.toolTip1.SetToolTip(this.titleListBox, "List of selected programs from the source DVD or subtitle file");
            this.titleListBox.SelectedIndexChanged += new System.EventHandler(this.titleListBox_SelectedIndexChanged);
            // 
            // subtitleDataFileLabel
            // 
            this.subtitleDataFileLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.subtitleDataFileLabel.Location = new System.Drawing.Point(113, 299);
            this.subtitleDataFileLabel.Name = "subtitleDataFileLabel";
            this.subtitleDataFileLabel.Size = new System.Drawing.Size(553, 23);
            this.subtitleDataFileLabel.TabIndex = 5;
            this.subtitleDataFileLabel.Text = "label1";
            this.subtitleDataFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // createSubtitleDataCheckBox
            // 
            this.createSubtitleDataCheckBox.AutoSize = true;
            this.createSubtitleDataCheckBox.Location = new System.Drawing.Point(132, 274);
            this.createSubtitleDataCheckBox.Name = "createSubtitleDataCheckBox";
            this.createSubtitleDataCheckBox.Size = new System.Drawing.Size(151, 17);
            this.createSubtitleDataCheckBox.TabIndex = 4;
            this.createSubtitleDataCheckBox.Text = "Create Subtitle Data File(s)";
            this.toolTip1.SetToolTip(this.createSubtitleDataCheckBox, "If checked, creates a BIN (SubExtractor data) file for use in OCRing the subtitle" +
        "s in the DVD tracks");
            this.createSubtitleDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // createMpegFileCheckBox
            // 
            this.createMpegFileCheckBox.AutoSize = true;
            this.createMpegFileCheckBox.Location = new System.Drawing.Point(132, 213);
            this.createMpegFileCheckBox.Name = "createMpegFileCheckBox";
            this.createMpegFileCheckBox.Size = new System.Drawing.Size(119, 17);
            this.createMpegFileCheckBox.TabIndex = 2;
            this.createMpegFileCheckBox.Text = "Create Movie File(s)";
            this.toolTip1.SetToolTip(this.createMpegFileCheckBox, "If checked, creates a MPG (Mpeg2 Program Stream) file for use in re-encoding or p" +
        "laying the track from the selected DVD tracks");
            this.createMpegFileCheckBox.UseVisualStyleBackColor = true;
            // 
            // mpegFileLabel
            // 
            this.mpegFileLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mpegFileLabel.Location = new System.Drawing.Point(113, 238);
            this.mpegFileLabel.Name = "mpegFileLabel";
            this.mpegFileLabel.Size = new System.Drawing.Size(553, 23);
            this.mpegFileLabel.TabIndex = 3;
            this.mpegFileLabel.Text = "label1";
            this.mpegFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dvdLabel
            // 
            this.dvdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dvdLabel.Location = new System.Drawing.Point(113, 33);
            this.dvdLabel.Name = "dvdLabel";
            this.dvdLabel.Size = new System.Drawing.Size(553, 23);
            this.dvdLabel.TabIndex = 0;
            this.dvdLabel.Text = "label2";
            this.dvdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // encodeAllButton
            // 
            this.encodeAllButton.Location = new System.Drawing.Point(168, 436);
            this.encodeAllButton.Name = "encodeAllButton";
            this.encodeAllButton.Size = new System.Drawing.Size(178, 23);
            this.encodeAllButton.TabIndex = 6;
            this.encodeAllButton.Text = "Encode All Programs";
            this.toolTip1.SetToolTip(this.encodeAllButton, "Run the selected operations on all selected DVD tracks ");
            this.encodeAllButton.UseVisualStyleBackColor = true;
            this.encodeAllButton.Click += new System.EventHandler(this.encodeAllButton_Click);
            // 
            // saveUpdatesLabel
            // 
            this.saveUpdatesLabel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.saveUpdatesLabel.Location = new System.Drawing.Point(420, 429);
            this.saveUpdatesLabel.Name = "saveUpdatesLabel";
            this.saveUpdatesLabel.Size = new System.Drawing.Size(183, 38);
            this.saveUpdatesLabel.TabIndex = 8;
            // 
            // cancelSaveButton
            // 
            this.cancelSaveButton.Enabled = false;
            this.cancelSaveButton.Location = new System.Drawing.Point(168, 475);
            this.cancelSaveButton.Name = "cancelSaveButton";
            this.cancelSaveButton.Size = new System.Drawing.Size(178, 23);
            this.cancelSaveButton.TabIndex = 7;
            this.cancelSaveButton.Text = "Cancel";
            this.toolTip1.SetToolTip(this.cancelSaveButton, "Cancel the in-progress operation");
            this.cancelSaveButton.UseVisualStyleBackColor = true;
            this.cancelSaveButton.Click += new System.EventHandler(this.cancelSaveButton_Click);
            // 
            // encodeSelectedButton
            // 
            this.encodeSelectedButton.Location = new System.Drawing.Point(168, 397);
            this.encodeSelectedButton.Name = "encodeSelectedButton";
            this.encodeSelectedButton.Size = new System.Drawing.Size(178, 23);
            this.encodeSelectedButton.TabIndex = 9;
            this.encodeSelectedButton.Text = "Encode Selected Program";
            this.toolTip1.SetToolTip(this.encodeSelectedButton, "Run the selected operations on a single selected track from the DVD");
            this.encodeSelectedButton.UseVisualStyleBackColor = true;
            this.encodeSelectedButton.Click += new System.EventHandler(this.encodeSelectedButton_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(414, 397);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(195, 23);
            this.progressBar1.TabIndex = 34;
            // 
            // createD2vCheckBox
            // 
            this.createD2vCheckBox.AutoSize = true;
            this.createD2vCheckBox.Location = new System.Drawing.Point(132, 337);
            this.createD2vCheckBox.Name = "createD2vCheckBox";
            this.createD2vCheckBox.Size = new System.Drawing.Size(130, 17);
            this.createD2vCheckBox.TabIndex = 35;
            this.createD2vCheckBox.Text = "Create DgIndex File(s)";
            this.toolTip1.SetToolTip(this.createD2vCheckBox, "If checked, creates D2V (Mpeg2 index) and splits the audio tracks into separate f" +
        "iles for use in re-encoding the DVD tracks");
            this.createD2vCheckBox.UseVisualStyleBackColor = true;
            // 
            // d2vFileLabel
            // 
            this.d2vFileLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.d2vFileLabel.Location = new System.Drawing.Point(113, 362);
            this.d2vFileLabel.Name = "d2vFileLabel";
            this.d2vFileLabel.Size = new System.Drawing.Size(553, 23);
            this.d2vFileLabel.TabIndex = 36;
            this.d2vFileLabel.Text = "label1";
            this.d2vFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // messageLabel
            // 
            this.messageLabel.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.messageLabel.ForeColor = System.Drawing.Color.Blue;
            this.messageLabel.Location = new System.Drawing.Point(377, 475);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(282, 23);
            this.messageLabel.TabIndex = 37;
            this.messageLabel.Text = "label1";
            this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RunExtractorStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.messageLabel);
            this.Controls.Add(this.createD2vCheckBox);
            this.Controls.Add(this.d2vFileLabel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.encodeSelectedButton);
            this.Controls.Add(this.cancelSaveButton);
            this.Controls.Add(this.saveUpdatesLabel);
            this.Controls.Add(this.encodeAllButton);
            this.Controls.Add(this.dvdLabel);
            this.Controls.Add(this.createMpegFileCheckBox);
            this.Controls.Add(this.mpegFileLabel);
            this.Controls.Add(this.createSubtitleDataCheckBox);
            this.Controls.Add(this.subtitleDataFileLabel);
            this.Controls.Add(this.titleListBox);
            this.Name = "RunExtractorStep";
            this.Size = new System.Drawing.Size(842, 662);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox titleListBox;
        private System.Windows.Forms.Label subtitleDataFileLabel;
        private System.Windows.Forms.CheckBox createSubtitleDataCheckBox;
        private System.Windows.Forms.CheckBox createMpegFileCheckBox;
        private System.Windows.Forms.Label mpegFileLabel;
        private System.Windows.Forms.Label dvdLabel;
        private System.Windows.Forms.Button encodeAllButton;
        private System.Windows.Forms.Label saveUpdatesLabel;
        private System.Windows.Forms.Button cancelSaveButton;
        private System.Windows.Forms.Button encodeSelectedButton;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.CheckBox createD2vCheckBox;
        private System.Windows.Forms.Label d2vFileLabel;
        private System.Windows.Forms.Label messageLabel;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
