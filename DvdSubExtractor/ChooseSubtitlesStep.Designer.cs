namespace DvdSubExtractor
{
    partial class ChooseSubtitlesStep
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
            if(disposing && (this.pictureBitmap != null))
            {
                this.pictureBitmap.Dispose();
                this.pictureBitmap = null;
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
            this.subtitleListBox = new System.Windows.Forms.ListBox();
            this.previousButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.binFileComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.browseButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.indexLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.indexUpDown = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.forcedCheckBox = new System.Windows.Forms.CheckBox();
            this.subtitlePictureBox = new System.Windows.Forms.PictureBox();
            this.scaleBitmapCheckBox = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.indexUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.subtitlePictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // subtitleListBox
            // 
            this.subtitleListBox.FormattingEnabled = true;
            this.subtitleListBox.Location = new System.Drawing.Point(53, 65);
            this.subtitleListBox.Name = "subtitleListBox";
            this.subtitleListBox.Size = new System.Drawing.Size(312, 108);
            this.subtitleListBox.TabIndex = 1;
            this.toolTip1.SetToolTip(this.subtitleListBox, "Subtitle tracks in data file");
            this.subtitleListBox.SelectedIndexChanged += new System.EventHandler(this.subtitleListBox_SelectedIndexChanged);
            // 
            // previousButton
            // 
            this.previousButton.Location = new System.Drawing.Point(407, 97);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(105, 42);
            this.previousButton.TabIndex = 2;
            this.previousButton.Text = "Previous Subtitle";
            this.toolTip1.SetToolTip(this.previousButton, "Display previous subtitle in the current data file and track");
            this.previousButton.UseVisualStyleBackColor = true;
            this.previousButton.Click += new System.EventHandler(this.previousButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(524, 97);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(105, 42);
            this.nextButton.TabIndex = 3;
            this.nextButton.Text = "Next Subtitle";
            this.toolTip1.SetToolTip(this.nextButton, "Display next subtitle in the current data file and track");
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // binFileComboBox
            // 
            this.binFileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.binFileComboBox.FormattingEnabled = true;
            this.binFileComboBox.Location = new System.Drawing.Point(53, 31);
            this.binFileComboBox.Name = "binFileComboBox";
            this.binFileComboBox.Size = new System.Drawing.Size(435, 21);
            this.binFileComboBox.TabIndex = 4;
            this.toolTip1.SetToolTip(this.binFileComboBox, "Current subtitle data file");
            this.binFileComboBox.SelectedIndexChanged += new System.EventHandler(this.binFileComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(57, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Subtitle Data File";
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(524, 31);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(105, 23);
            this.browseButton.TabIndex = 6;
            this.browseButton.Text = "Browse...";
            this.toolTip1.SetToolTip(this.browseButton, "Select new subtitle data file(s)");
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // indexLabel
            // 
            this.indexLabel.AutoSize = true;
            this.indexLabel.Location = new System.Drawing.Point(517, 151);
            this.indexLabel.Name = "indexLabel";
            this.indexLabel.Size = new System.Drawing.Size(35, 13);
            this.indexLabel.TabIndex = 7;
            this.indexLabel.Text = "label2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(404, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(162, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Select the Subtitle Track to OCR";
            // 
            // indexUpDown
            // 
            this.indexUpDown.Location = new System.Drawing.Point(450, 148);
            this.indexUpDown.Name = "indexUpDown";
            this.indexUpDown.Size = new System.Drawing.Size(60, 20);
            this.indexUpDown.TabIndex = 9;
            this.toolTip1.SetToolTip(this.indexUpDown, "Current subtitle index within the track");
            this.indexUpDown.ValueChanged += new System.EventHandler(this.indexUpDown_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(409, 151);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Index";
            // 
            // forcedCheckBox
            // 
            this.forcedCheckBox.AutoSize = true;
            this.forcedCheckBox.Enabled = false;
            this.forcedCheckBox.Location = new System.Drawing.Point(665, 98);
            this.forcedCheckBox.Name = "forcedCheckBox";
            this.forcedCheckBox.Size = new System.Drawing.Size(83, 17);
            this.forcedCheckBox.TabIndex = 11;
            this.forcedCheckBox.Text = "Forced Only";
            this.toolTip1.SetToolTip(this.forcedCheckBox, "Show only those subtitles with the Forced attribute set");
            this.forcedCheckBox.UseVisualStyleBackColor = true;
            this.forcedCheckBox.CheckedChanged += new System.EventHandler(this.forcedCheckBox_CheckedChanged);
            // 
            // subtitlePictureBox
            // 
            this.subtitlePictureBox.BackColor = System.Drawing.Color.Sienna;
            this.subtitlePictureBox.Location = new System.Drawing.Point(0, 182);
            this.subtitlePictureBox.Name = "subtitlePictureBox";
            this.subtitlePictureBox.Size = new System.Drawing.Size(839, 480);
            this.subtitlePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.subtitlePictureBox.TabIndex = 0;
            this.subtitlePictureBox.TabStop = false;
            // 
            // scaleBitmapCheckBox
            // 
            this.scaleBitmapCheckBox.AutoSize = true;
            this.scaleBitmapCheckBox.Location = new System.Drawing.Point(665, 121);
            this.scaleBitmapCheckBox.Name = "scaleBitmapCheckBox";
            this.scaleBitmapCheckBox.Size = new System.Drawing.Size(95, 17);
            this.scaleBitmapCheckBox.TabIndex = 12;
            this.scaleBitmapCheckBox.Text = "Scale to Video";
            this.toolTip1.SetToolTip(this.scaleBitmapCheckBox, "Show the subtitle as it would appear on the entire video frame instead of just th" +
        "e bounding subtitle rectangle");
            this.scaleBitmapCheckBox.UseVisualStyleBackColor = true;
            this.scaleBitmapCheckBox.CheckedChanged += new System.EventHandler(this.scaleBitmapCheckBox_CheckedChanged);
            // 
            // ChooseSubtitlesStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.scaleBitmapCheckBox);
            this.Controls.Add(this.forcedCheckBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.indexUpDown);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.indexLabel);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.binFileComboBox);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.previousButton);
            this.Controls.Add(this.subtitleListBox);
            this.Controls.Add(this.subtitlePictureBox);
            this.Name = "ChooseSubtitlesStep";
            this.Size = new System.Drawing.Size(842, 662);
            ((System.ComponentModel.ISupportInitialize)(this.indexUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.subtitlePictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox subtitlePictureBox;
        private System.Windows.Forms.ListBox subtitleListBox;
        private System.Windows.Forms.Button previousButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.ComboBox binFileComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label indexLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown indexUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox forcedCheckBox;
        private System.Windows.Forms.CheckBox scaleBitmapCheckBox;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
