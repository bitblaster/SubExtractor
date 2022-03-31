namespace DvdSubExtractor
{
    partial class SubWizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SubWizard));
            this.helpTextBox = new System.Windows.Forms.RichTextBox();
            this.panelItem = new System.Windows.Forms.Panel();
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.openDvdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previousStepButton = new System.Windows.Forms.ToolStripButton();
            this.nextButton = new System.Windows.Forms.ToolStripButton();
            this.optionsButton = new System.Windows.Forms.ToolStripButton();
            this.aboutButton = new System.Windows.Forms.ToolStripButton();
            this.mainToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // helpTextBox
            // 
            this.helpTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.helpTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.helpTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.helpTextBox.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpTextBox.Location = new System.Drawing.Point(845, 27);
            this.helpTextBox.Name = "helpTextBox";
            this.helpTextBox.ReadOnly = true;
            this.helpTextBox.Size = new System.Drawing.Size(242, 664);
            this.helpTextBox.TabIndex = 2;
            this.helpTextBox.TabStop = false;
            this.helpTextBox.Text = "";
            // 
            // panelItem
            // 
            this.panelItem.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelItem.Location = new System.Drawing.Point(1, 29);
            this.panelItem.Name = "panelItem";
            this.panelItem.Size = new System.Drawing.Size(842, 662);
            this.panelItem.TabIndex = 1;
            this.panelItem.Visible = false;
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.AllowMerge = false;
            this.mainToolStrip.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.previousStepButton,
            this.nextButton,
            this.optionsButton,
            this.aboutButton});
            this.mainToolStrip.Location = new System.Drawing.Point(0, 0);
            this.mainToolStrip.Name = "mainToolStrip";
            this.mainToolStrip.Size = new System.Drawing.Size(1098, 27);
            this.mainToolStrip.TabIndex = 0;
            this.mainToolStrip.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openDvdToolStripMenuItem,
            this.openFileToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Margin = new System.Windows.Forms.Padding(20, 2, 0, 0);
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(59, 25);
            this.toolStripDropDownButton1.Text = "File";
            // 
            // openDvdToolStripMenuItem
            // 
            this.openDvdToolStripMenuItem.Name = "openDvdToolStripMenuItem";
            this.openDvdToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.openDvdToolStripMenuItem.Text = "DVD Folder";
            this.openDvdToolStripMenuItem.Click += new System.EventHandler(this.openDvdToolStripMenuItem_Click);
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.openFileToolStripMenuItem.Text = "Subtitle File";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // previousStepButton
            // 
            this.previousStepButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.previousStepButton.Image = global::DvdSubExtractor.Properties.Resources.LeftGreenArrow;
            this.previousStepButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.previousStepButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.previousStepButton.Margin = new System.Windows.Forms.Padding(40, 2, 0, 0);
            this.previousStepButton.Name = "previousStepButton";
            this.previousStepButton.Size = new System.Drawing.Size(120, 25);
            this.previousStepButton.Text = "Previous Step";
            this.previousStepButton.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
            this.previousStepButton.ToolTipText = "Previous Step (progress will be saved)";
            this.previousStepButton.Click += new System.EventHandler(this.previousButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.nextButton.Image = global::DvdSubExtractor.Properties.Resources.RightGreenArrow;
            this.nextButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.nextButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.nextButton.Margin = new System.Windows.Forms.Padding(40, 2, 0, 0);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(120, 25);
            this.nextButton.Text = "Next Step";
            this.nextButton.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // optionsButton
            // 
            this.optionsButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.optionsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.optionsButton.Image = global::DvdSubExtractor.Properties.Resources.RectanglePurple;
            this.optionsButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.optionsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.optionsButton.Margin = new System.Windows.Forms.Padding(40, 2, 0, 0);
            this.optionsButton.Name = "optionsButton";
            this.optionsButton.Size = new System.Drawing.Size(71, 25);
            this.optionsButton.Text = "Options...";
            this.optionsButton.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
            this.optionsButton.Click += new System.EventHandler(this.optionsButton_Click);
            // 
            // aboutButton
            // 
            this.aboutButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.aboutButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.aboutButton.Image = global::DvdSubExtractor.Properties.Resources.RectanglePurple;
            this.aboutButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.aboutButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.aboutButton.Margin = new System.Windows.Forms.Padding(40, 2, 0, 0);
            this.aboutButton.Name = "aboutButton";
            this.aboutButton.Size = new System.Drawing.Size(62, 25);
            this.aboutButton.Text = "About...";
            this.aboutButton.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
            this.aboutButton.Click += new System.EventHandler(this.aboutButton_Click);
            // 
            // SubWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1098, 704);
            this.Controls.Add(this.mainToolStrip);
            this.Controls.Add(this.panelItem);
            this.Controls.Add(this.helpTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1106, 731);
            this.Name = "SubWizard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Subtitle Extractor";
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox helpTextBox;
        private System.Windows.Forms.Panel panelItem;
        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripButton previousStepButton;
        private System.Windows.Forms.ToolStripButton optionsButton;
        private System.Windows.Forms.ToolStripButton aboutButton;
        private System.Windows.Forms.ToolStripButton nextButton;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem openDvdToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
    }
}