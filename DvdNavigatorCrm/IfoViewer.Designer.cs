namespace DvdNavigatorCrm
{
	partial class IfoViewer
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.ifoDumpEdit = new System.Windows.Forms.RichTextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.directoryButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ifoDumpEdit
            // 
            this.ifoDumpEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ifoDumpEdit.Location = new System.Drawing.Point(4, 6);
            this.ifoDumpEdit.Name = "ifoDumpEdit";
            this.ifoDumpEdit.ReadOnly = true;
            this.ifoDumpEdit.Size = new System.Drawing.Size(491, 406);
            this.ifoDumpEdit.TabIndex = 0;
            this.ifoDumpEdit.Text = "";
            // 
            // browseButton
            // 
            this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.browseButton.Location = new System.Drawing.Point(13, 420);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(95, 23);
            this.browseButton.TabIndex = 1;
            this.browseButton.Text = "Browse...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // directoryButton
            // 
            this.directoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.directoryButton.Location = new System.Drawing.Point(185, 420);
            this.directoryButton.Name = "directoryButton";
            this.directoryButton.Size = new System.Drawing.Size(95, 23);
            this.directoryButton.TabIndex = 2;
            this.directoryButton.Text = "Root Dir...";
            this.directoryButton.UseVisualStyleBackColor = true;
            this.directoryButton.Click += new System.EventHandler(this.directoryButton_Click);
            // 
            // IfoViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 452);
            this.Controls.Add(this.directoryButton);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.ifoDumpEdit);
            this.Name = "IfoViewer";
            this.Text = "IfoViewer";
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox ifoDumpEdit;
		private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button directoryButton;
	}
}