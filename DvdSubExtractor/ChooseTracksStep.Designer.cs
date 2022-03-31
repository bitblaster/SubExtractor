namespace DvdSubExtractor
{
    partial class ChooseTracksStep
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
            this.audioCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.titleListBox = new System.Windows.Forms.CheckedListBox();
            this.anglesComboBox = new System.Windows.Forms.ComboBox();
            this.createSampleButton = new System.Windows.Forms.Button();
            this.createAngleSampleButton = new System.Windows.Forms.Button();
            this.sampleUpdatesLabel = new System.Windows.Forms.Label();
            this.subtitleListBox = new System.Windows.Forms.ListBox();
            this.dvdLabel = new System.Windows.Forms.Label();
            this.splitChaptersButton = new System.Windows.Forms.Button();
            this.splitCellsButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // audioCheckedListBox
            // 
            this.audioCheckedListBox.FormattingEnabled = true;
            this.audioCheckedListBox.Location = new System.Drawing.Point(87, 286);
            this.audioCheckedListBox.Name = "audioCheckedListBox";
            this.audioCheckedListBox.Size = new System.Drawing.Size(330, 109);
            this.audioCheckedListBox.TabIndex = 5;
            this.toolTip1.SetToolTip(this.audioCheckedListBox, "Check audio tracks to be re-encoded");
            this.audioCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.audioCheckedListBox_ItemCheck);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(94, 269);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Audio Tracks";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(94, 401);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Subtitle Tracks";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(105, 220);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "DVD Angle";
            // 
            // titleListBox
            // 
            this.titleListBox.FormattingEnabled = true;
            this.titleListBox.Location = new System.Drawing.Point(87, 73);
            this.titleListBox.Name = "titleListBox";
            this.titleListBox.Size = new System.Drawing.Size(640, 139);
            this.titleListBox.TabIndex = 0;
            this.toolTip1.SetToolTip(this.titleListBox, "List of video tracks on the DVD");
            this.titleListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.titleListBox_ItemCheck);
            this.titleListBox.SelectedIndexChanged += new System.EventHandler(this.titleListBox_SelectedIndexChanged);
            // 
            // anglesComboBox
            // 
            this.anglesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.anglesComboBox.FormattingEnabled = true;
            this.anglesComboBox.Location = new System.Drawing.Point(87, 236);
            this.anglesComboBox.Name = "anglesComboBox";
            this.anglesComboBox.Size = new System.Drawing.Size(105, 21);
            this.anglesComboBox.TabIndex = 2;
            this.toolTip1.SetToolTip(this.anglesComboBox, "Choose the Angle (usually specifies the language of the credits) of the video");
            this.anglesComboBox.SelectedIndexChanged += new System.EventHandler(this.anglesComboBox_SelectedIndexChanged);
            // 
            // createSampleButton
            // 
            this.createSampleButton.Location = new System.Drawing.Point(435, 318);
            this.createSampleButton.Name = "createSampleButton";
            this.createSampleButton.Size = new System.Drawing.Size(183, 45);
            this.createSampleButton.TabIndex = 6;
            this.createSampleButton.Text = "Create Sample Video with first checked Audio Track";
            this.toolTip1.SetToolTip(this.createSampleButton, "Create and launch video viewer to preview 5 minutes of the selected video with th" +
        "e first checked audio track");
            this.createSampleButton.UseVisualStyleBackColor = true;
            this.createSampleButton.Click += new System.EventHandler(this.createSampleButton_Click);
            // 
            // createAngleSampleButton
            // 
            this.createAngleSampleButton.Location = new System.Drawing.Point(435, 227);
            this.createAngleSampleButton.Name = "createAngleSampleButton";
            this.createAngleSampleButton.Size = new System.Drawing.Size(183, 37);
            this.createAngleSampleButton.TabIndex = 3;
            this.createAngleSampleButton.Text = "Create Sample Video of Angle";
            this.toolTip1.SetToolTip(this.createAngleSampleButton, "Create and launch video viewer to preview 5 minutes of the selected video angle");
            this.createAngleSampleButton.UseVisualStyleBackColor = true;
            this.createAngleSampleButton.Click += new System.EventHandler(this.createAngleSampleButton_Click);
            // 
            // sampleUpdatesLabel
            // 
            this.sampleUpdatesLabel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.sampleUpdatesLabel.Location = new System.Drawing.Point(435, 272);
            this.sampleUpdatesLabel.Name = "sampleUpdatesLabel";
            this.sampleUpdatesLabel.Size = new System.Drawing.Size(183, 38);
            this.sampleUpdatesLabel.TabIndex = 9;
            this.sampleUpdatesLabel.Visible = false;
            // 
            // subtitleListBox
            // 
            this.subtitleListBox.Enabled = false;
            this.subtitleListBox.FormattingEnabled = true;
            this.subtitleListBox.Location = new System.Drawing.Point(87, 418);
            this.subtitleListBox.Name = "subtitleListBox";
            this.subtitleListBox.Size = new System.Drawing.Size(330, 108);
            this.subtitleListBox.TabIndex = 8;
            this.toolTip1.SetToolTip(this.subtitleListBox, "Subtitle tracks which are part of the selected video track");
            // 
            // dvdLabel
            // 
            this.dvdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dvdLabel.Location = new System.Drawing.Point(71, 35);
            this.dvdLabel.Name = "dvdLabel";
            this.dvdLabel.Size = new System.Drawing.Size(656, 23);
            this.dvdLabel.TabIndex = 10;
            this.dvdLabel.Text = "label2";
            this.dvdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // splitChaptersButton
            // 
            this.splitChaptersButton.Location = new System.Drawing.Point(207, 227);
            this.splitChaptersButton.Name = "splitChaptersButton";
            this.splitChaptersButton.Size = new System.Drawing.Size(102, 37);
            this.splitChaptersButton.TabIndex = 11;
            this.splitChaptersButton.Text = "Split By Chapter";
            this.toolTip1.SetToolTip(this.splitChaptersButton, "Split the video track into its chapters (cells with discontiguous timestamps). Us" +
        "eful to fix synchronization problems");
            this.splitChaptersButton.UseVisualStyleBackColor = true;
            this.splitChaptersButton.Click += new System.EventHandler(this.splitChaptersButton_Click);
            // 
            // splitCellsButton
            // 
            this.splitCellsButton.Location = new System.Drawing.Point(315, 227);
            this.splitCellsButton.Name = "splitCellsButton";
            this.splitCellsButton.Size = new System.Drawing.Size(102, 37);
            this.splitCellsButton.TabIndex = 12;
            this.splitCellsButton.Text = "Split By Cell";
            this.toolTip1.SetToolTip(this.splitCellsButton, "Split the video track into its DVD cells. Useful to fix synchronization problems");
            this.splitCellsButton.UseVisualStyleBackColor = true;
            this.splitCellsButton.Click += new System.EventHandler(this.splitCellsButton_Click);
            // 
            // ChooseTracksStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitCellsButton);
            this.Controls.Add(this.splitChaptersButton);
            this.Controls.Add(this.dvdLabel);
            this.Controls.Add(this.subtitleListBox);
            this.Controls.Add(this.sampleUpdatesLabel);
            this.Controls.Add(this.createAngleSampleButton);
            this.Controls.Add(this.createSampleButton);
            this.Controls.Add(this.anglesComboBox);
            this.Controls.Add(this.titleListBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.audioCheckedListBox);
            this.Name = "ChooseTracksStep";
            this.Size = new System.Drawing.Size(842, 662);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox audioCheckedListBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox titleListBox;
        private System.Windows.Forms.ComboBox anglesComboBox;
        private System.Windows.Forms.Button createSampleButton;
        private System.Windows.Forms.Button createAngleSampleButton;
        private System.Windows.Forms.Label sampleUpdatesLabel;
        private System.Windows.Forms.ListBox subtitleListBox;
        private System.Windows.Forms.Label dvdLabel;
        private System.Windows.Forms.Button splitChaptersButton;
        private System.Windows.Forms.Button splitCellsButton;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
