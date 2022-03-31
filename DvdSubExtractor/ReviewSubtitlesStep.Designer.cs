namespace DvdSubExtractor
{
    partial class ReviewSubtitlesStep
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
            this.label3 = new System.Windows.Forms.Label();
            this.indexLabel = new System.Windows.Forms.Label();
            this.nextButton = new System.Windows.Forms.Button();
            this.previousButton = new System.Windows.Forms.Button();
            this.subtitlePictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.subtitlePictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(40, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(337, 15);
            this.label3.TabIndex = 17;
            this.label3.Text = "Press Ctrl key down to see the original DVD Subtitle";
            // 
            // indexLabel
            // 
            this.indexLabel.AutoSize = true;
            this.indexLabel.Location = new System.Drawing.Point(516, 62);
            this.indexLabel.Name = "indexLabel";
            this.indexLabel.Size = new System.Drawing.Size(35, 13);
            this.indexLabel.TabIndex = 16;
            this.indexLabel.Text = "label2";
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(578, 12);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(105, 42);
            this.nextButton.TabIndex = 15;
            this.nextButton.Text = "Next Subtitle in Track";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // previousButton
            // 
            this.previousButton.Location = new System.Drawing.Point(461, 12);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(105, 42);
            this.previousButton.TabIndex = 14;
            this.previousButton.Text = "Previous Subtitle in Track";
            this.previousButton.UseVisualStyleBackColor = true;
            this.previousButton.Click += new System.EventHandler(this.previousButton_Click);
            // 
            // subtitlePictureBox
            // 
            this.subtitlePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.subtitlePictureBox.BackColor = System.Drawing.Color.BurlyWood;
            this.subtitlePictureBox.Location = new System.Drawing.Point(0, 82);
            this.subtitlePictureBox.Name = "subtitlePictureBox";
            this.subtitlePictureBox.Size = new System.Drawing.Size(720, 480);
            this.subtitlePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.subtitlePictureBox.TabIndex = 13;
            this.subtitlePictureBox.TabStop = false;
            // 
            // ReviewSubtitlesStep
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.indexLabel);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.previousButton);
            this.Controls.Add(this.subtitlePictureBox);
            this.Name = "ReviewSubtitlesStep";
            this.Size = new System.Drawing.Size(720, 562);
            ((System.ComponentModel.ISupportInitialize)(this.subtitlePictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label indexLabel;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button previousButton;
        private System.Windows.Forms.PictureBox subtitlePictureBox;
    }
}
