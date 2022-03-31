namespace DvdSubExtractor
{
    partial class LoadFolderStep
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
            this.programChainListbox = new System.Windows.Forms.ListBox();
            this.programChainLabel = new System.Windows.Forms.Label();
            this.dvdFolderPath = new System.Windows.Forms.Label();
            this.dvdFolderBrowseButton = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.reloadButton = new System.Windows.Forms.Button();
            this.dvdPathTextBox = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // programChainListbox
            // 
            this.programChainListbox.Location = new System.Drawing.Point(101, 137);
            this.programChainListbox.Name = "programChainListbox";
            this.programChainListbox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.programChainListbox.Size = new System.Drawing.Size(634, 342);
            this.programChainListbox.TabIndex = 21;
            this.programChainListbox.TabStop = false;
            this.toolTip1.SetToolTip(this.programChainListbox, "List of program chains available for Re-encoding and OCR in the selected DVD fold" +
        "er");
            // 
            // programChainLabel
            // 
            this.programChainLabel.AutoSize = true;
            this.programChainLabel.Location = new System.Drawing.Point(108, 119);
            this.programChainLabel.Name = "programChainLabel";
            this.programChainLabel.Size = new System.Drawing.Size(77, 13);
            this.programChainLabel.TabIndex = 20;
            this.programChainLabel.Text = "DVD Programs";
            // 
            // dvdFolderPath
            // 
            this.dvdFolderPath.AutoEllipsis = true;
            this.dvdFolderPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dvdFolderPath.Location = new System.Drawing.Point(101, 71);
            this.dvdFolderPath.Name = "dvdFolderPath";
            this.dvdFolderPath.Size = new System.Drawing.Size(634, 23);
            this.dvdFolderPath.TabIndex = 19;
            this.dvdFolderPath.Text = "DVD Folder";
            this.dvdFolderPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dvdFolderBrowseButton
            // 
            this.dvdFolderBrowseButton.Location = new System.Drawing.Point(101, 36);
            this.dvdFolderBrowseButton.Name = "dvdFolderBrowseButton";
            this.dvdFolderBrowseButton.Size = new System.Drawing.Size(129, 23);
            this.dvdFolderBrowseButton.TabIndex = 18;
            this.dvdFolderBrowseButton.Text = "Browse DVD...";
            this.toolTip1.SetToolTip(this.dvdFolderBrowseButton, "Choose the DVD folder to analyze for tracks and subtitles");
            this.dvdFolderBrowseButton.UseVisualStyleBackColor = true;
            this.dvdFolderBrowseButton.Click += new System.EventHandler(this.dvdFolderBrowseButton_Click);
            // 
            // reloadButton
            // 
            this.reloadButton.Location = new System.Drawing.Point(606, 36);
            this.reloadButton.Name = "reloadButton";
            this.reloadButton.Size = new System.Drawing.Size(129, 23);
            this.reloadButton.TabIndex = 22;
            this.reloadButton.Text = "(Re)Load DVD";
            this.toolTip1.SetToolTip(this.reloadButton, "Restart the selections of tracks, chapters, etc. for a DVD folder");
            this.reloadButton.UseVisualStyleBackColor = true;
            this.reloadButton.Click += new System.EventHandler(this.reloadButton_Click);
            // 
            // dvdPathTextBox
            // 
            this.dvdPathTextBox.Location = new System.Drawing.Point(236, 37);
            this.dvdPathTextBox.Name = "dvdPathTextBox";
            this.dvdPathTextBox.Size = new System.Drawing.Size(364, 20);
            this.dvdPathTextBox.TabIndex = 23;
            this.toolTip1.SetToolTip(this.dvdPathTextBox, "Selected DVD path");
            this.dvdPathTextBox.TextChanged += new System.EventHandler(this.dvdPathTextBox_TextChanged);
            // 
            // LoadFolderStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dvdPathTextBox);
            this.Controls.Add(this.reloadButton);
            this.Controls.Add(this.programChainListbox);
            this.Controls.Add(this.programChainLabel);
            this.Controls.Add(this.dvdFolderPath);
            this.Controls.Add(this.dvdFolderBrowseButton);
            this.Name = "LoadFolderStep";
            this.Size = new System.Drawing.Size(842, 662);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox programChainListbox;
        private System.Windows.Forms.Label programChainLabel;
        private System.Windows.Forms.Label dvdFolderPath;
        private System.Windows.Forms.Button dvdFolderBrowseButton;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Button reloadButton;
        private System.Windows.Forms.TextBox dvdPathTextBox;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
