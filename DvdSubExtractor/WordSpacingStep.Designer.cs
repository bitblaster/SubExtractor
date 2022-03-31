namespace DvdSubExtractor
{
    partial class WordSpacingStep
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
            this.dvdLabel = new System.Windows.Forms.Label();
            this.italicRadioButton = new System.Windows.Forms.RadioButton();
            this.normalRadioButton = new System.Windows.Forms.RadioButton();
            this.restoreDefaultSpacingButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.defaultItalicRightLabel = new System.Windows.Forms.Label();
            this.defaultItalicLeftLabel = new System.Windows.Forms.Label();
            this.defaultRightLabel = new System.Windows.Forms.Label();
            this.defaultLeftLabel = new System.Windows.Forms.Label();
            this.kerningItalicRightUpDown = new System.Windows.Forms.NumericUpDown();
            this.kerningItalicLeftUpDown = new System.Windows.Forms.NumericUpDown();
            this.kerningRightUpDown = new System.Windows.Forms.NumericUpDown();
            this.kerningCharacterComboBox = new System.Windows.Forms.ComboBox();
            this.kerningLeftUpDown = new System.Windows.Forms.NumericUpDown();
            this.spacingListBox = new System.Windows.Forms.ListBox();
            this.matchSoFarView = new DvdSubOcr.MatchSoFarView();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.saveSpacingButton = new System.Windows.Forms.Button();
            this.restoreSavedSpacingButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.kerningItalicRightUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kerningItalicLeftUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kerningRightUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kerningLeftUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // dvdLabel
            // 
            this.dvdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dvdLabel.Location = new System.Drawing.Point(91, 13);
            this.dvdLabel.Name = "dvdLabel";
            this.dvdLabel.Size = new System.Drawing.Size(646, 23);
            this.dvdLabel.TabIndex = 0;
            this.dvdLabel.Text = "label2";
            this.dvdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // italicRadioButton
            // 
            this.italicRadioButton.AutoSize = true;
            this.italicRadioButton.Location = new System.Drawing.Point(577, 111);
            this.italicRadioButton.Name = "italicRadioButton";
            this.italicRadioButton.Size = new System.Drawing.Size(47, 17);
            this.italicRadioButton.TabIndex = 11;
            this.italicRadioButton.TabStop = true;
            this.italicRadioButton.Text = "Italic";
            this.toolTip1.SetToolTip(this.italicRadioButton, "Select if the italic version of the character need spacing correction");
            this.italicRadioButton.UseVisualStyleBackColor = true;
            this.italicRadioButton.CheckedChanged += new System.EventHandler(this.kerningChoice_Changed);
            // 
            // normalRadioButton
            // 
            this.normalRadioButton.AutoSize = true;
            this.normalRadioButton.Checked = true;
            this.normalRadioButton.Location = new System.Drawing.Point(577, 62);
            this.normalRadioButton.Name = "normalRadioButton";
            this.normalRadioButton.Size = new System.Drawing.Size(82, 17);
            this.normalRadioButton.TabIndex = 5;
            this.normalRadioButton.TabStop = true;
            this.normalRadioButton.Text = "Normal  Left";
            this.toolTip1.SetToolTip(this.normalRadioButton, "Select if the normal (non-italic) version of the character need spacing correctio" +
        "n");
            this.normalRadioButton.UseVisualStyleBackColor = true;
            this.normalRadioButton.CheckedChanged += new System.EventHandler(this.kerningChoice_Changed);
            // 
            // restoreDefaultSpacingButton
            // 
            this.restoreDefaultSpacingButton.Location = new System.Drawing.Point(624, 588);
            this.restoreDefaultSpacingButton.Name = "restoreDefaultSpacingButton";
            this.restoreDefaultSpacingButton.Size = new System.Drawing.Size(174, 23);
            this.restoreDefaultSpacingButton.TabIndex = 4;
            this.restoreDefaultSpacingButton.Text = "Restore Default Spacing";
            this.toolTip1.SetToolTip(this.restoreDefaultSpacingButton, "Reset all character spacing to the program defaults");
            this.restoreDefaultSpacingButton.UseVisualStyleBackColor = true;
            this.restoreDefaultSpacingButton.Click += new System.EventHandler(this.restoreDefaultSpacingButton_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(493, 89);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 3;
            this.label7.Text = "Character";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Blue;
            this.label6.Location = new System.Drawing.Point(655, 111);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(94, 15);
            this.label6.TabIndex = 14;
            this.label6.Text = "(Default Values)";
            // 
            // defaultItalicRightLabel
            // 
            this.defaultItalicRightLabel.AutoSize = true;
            this.defaultItalicRightLabel.ForeColor = System.Drawing.Color.Blue;
            this.defaultItalicRightLabel.Location = new System.Drawing.Point(751, 135);
            this.defaultItalicRightLabel.Name = "defaultItalicRightLabel";
            this.defaultItalicRightLabel.Size = new System.Drawing.Size(13, 13);
            this.defaultItalicRightLabel.TabIndex = 16;
            this.defaultItalicRightLabel.Text = "0";
            // 
            // defaultItalicLeftLabel
            // 
            this.defaultItalicLeftLabel.AutoSize = true;
            this.defaultItalicLeftLabel.ForeColor = System.Drawing.Color.Blue;
            this.defaultItalicLeftLabel.Location = new System.Drawing.Point(654, 135);
            this.defaultItalicLeftLabel.Name = "defaultItalicLeftLabel";
            this.defaultItalicLeftLabel.Size = new System.Drawing.Size(13, 13);
            this.defaultItalicLeftLabel.TabIndex = 13;
            this.defaultItalicLeftLabel.Text = "0";
            // 
            // defaultRightLabel
            // 
            this.defaultRightLabel.AutoSize = true;
            this.defaultRightLabel.ForeColor = System.Drawing.Color.Blue;
            this.defaultRightLabel.Location = new System.Drawing.Point(751, 87);
            this.defaultRightLabel.Name = "defaultRightLabel";
            this.defaultRightLabel.Size = new System.Drawing.Size(13, 13);
            this.defaultRightLabel.TabIndex = 10;
            this.defaultRightLabel.Text = "0";
            // 
            // defaultLeftLabel
            // 
            this.defaultLeftLabel.AutoSize = true;
            this.defaultLeftLabel.ForeColor = System.Drawing.Color.Blue;
            this.defaultLeftLabel.Location = new System.Drawing.Point(654, 87);
            this.defaultLeftLabel.Name = "defaultLeftLabel";
            this.defaultLeftLabel.Size = new System.Drawing.Size(13, 13);
            this.defaultLeftLabel.TabIndex = 7;
            this.defaultLeftLabel.Text = "0";
            // 
            // kerningItalicRightUpDown
            // 
            this.kerningItalicRightUpDown.Location = new System.Drawing.Point(697, 133);
            this.kerningItalicRightUpDown.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.kerningItalicRightUpDown.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            -2147483648});
            this.kerningItalicRightUpDown.Name = "kerningItalicRightUpDown";
            this.kerningItalicRightUpDown.Size = new System.Drawing.Size(52, 20);
            this.kerningItalicRightUpDown.TabIndex = 15;
            this.toolTip1.SetToolTip(this.kerningItalicRightUpDown, "Current spacing adjustment to the right of the character");
            this.kerningItalicRightUpDown.ValueChanged += new System.EventHandler(this.kerningItalicRightUpDown_ValueChanged);
            // 
            // kerningItalicLeftUpDown
            // 
            this.kerningItalicLeftUpDown.Location = new System.Drawing.Point(600, 133);
            this.kerningItalicLeftUpDown.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.kerningItalicLeftUpDown.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            -2147483648});
            this.kerningItalicLeftUpDown.Name = "kerningItalicLeftUpDown";
            this.kerningItalicLeftUpDown.Size = new System.Drawing.Size(52, 20);
            this.kerningItalicLeftUpDown.TabIndex = 12;
            this.toolTip1.SetToolTip(this.kerningItalicLeftUpDown, "Current spacing adjustment to the left of the character");
            this.kerningItalicLeftUpDown.ValueChanged += new System.EventHandler(this.kerningItalicLeftUpDown_ValueChanged);
            // 
            // kerningRightUpDown
            // 
            this.kerningRightUpDown.Location = new System.Drawing.Point(697, 85);
            this.kerningRightUpDown.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.kerningRightUpDown.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            -2147483648});
            this.kerningRightUpDown.Name = "kerningRightUpDown";
            this.kerningRightUpDown.Size = new System.Drawing.Size(52, 20);
            this.kerningRightUpDown.TabIndex = 9;
            this.toolTip1.SetToolTip(this.kerningRightUpDown, "Current spacing adjustment to the right of the character");
            this.kerningRightUpDown.ValueChanged += new System.EventHandler(this.kerningRightUpDown_ValueChanged);
            // 
            // kerningCharacterComboBox
            // 
            this.kerningCharacterComboBox.DropDownHeight = 300;
            this.kerningCharacterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.kerningCharacterComboBox.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.kerningCharacterComboBox.FormattingEnabled = true;
            this.kerningCharacterComboBox.IntegralHeight = false;
            this.kerningCharacterComboBox.Location = new System.Drawing.Point(417, 83);
            this.kerningCharacterComboBox.Name = "kerningCharacterComboBox";
            this.kerningCharacterComboBox.Size = new System.Drawing.Size(70, 27);
            this.kerningCharacterComboBox.TabIndex = 2;
            this.toolTip1.SetToolTip(this.kerningCharacterComboBox, "Choose the character with incorrect left or right spacing around it");
            this.kerningCharacterComboBox.SelectedIndexChanged += new System.EventHandler(this.kerningChoice_Changed);
            // 
            // kerningLeftUpDown
            // 
            this.kerningLeftUpDown.Location = new System.Drawing.Point(600, 85);
            this.kerningLeftUpDown.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.kerningLeftUpDown.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            -2147483648});
            this.kerningLeftUpDown.Name = "kerningLeftUpDown";
            this.kerningLeftUpDown.Size = new System.Drawing.Size(52, 20);
            this.kerningLeftUpDown.TabIndex = 6;
            this.toolTip1.SetToolTip(this.kerningLeftUpDown, "Current spacing adjustment to the left of the character");
            this.kerningLeftUpDown.ValueChanged += new System.EventHandler(this.kerningLeftUpDown_ValueChanged);
            // 
            // spacingListBox
            // 
            this.spacingListBox.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spacingListBox.FormattingEnabled = true;
            this.spacingListBox.ItemHeight = 17;
            this.spacingListBox.Location = new System.Drawing.Point(17, 66);
            this.spacingListBox.Name = "spacingListBox";
            this.spacingListBox.Size = new System.Drawing.Size(326, 582);
            this.spacingListBox.TabIndex = 1;
            this.spacingListBox.SelectedIndexChanged += new System.EventHandler(this.spacingListBox_SelectedIndexChanged);
            // 
            // matchSoFarView
            // 
            this.matchSoFarView.BackColor = System.Drawing.Color.Sienna;
            this.matchSoFarView.ForeColor = System.Drawing.Color.Aquamarine;
            this.matchSoFarView.Location = new System.Drawing.Point(351, 167);
            this.matchSoFarView.Name = "matchSoFarView";
            this.matchSoFarView.Size = new System.Drawing.Size(480, 360);
            this.matchSoFarView.TabIndex = 17;
            this.matchSoFarView.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(705, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Right";
            // 
            // saveSpacingButton
            // 
            this.saveSpacingButton.Location = new System.Drawing.Point(372, 546);
            this.saveSpacingButton.Name = "saveSpacingButton";
            this.saveSpacingButton.Size = new System.Drawing.Size(174, 23);
            this.saveSpacingButton.TabIndex = 18;
            this.saveSpacingButton.Text = "Save Spacing";
            this.toolTip1.SetToolTip(this.saveSpacingButton, "Save current values for when the program restarts");
            this.saveSpacingButton.UseVisualStyleBackColor = true;
            this.saveSpacingButton.Click += new System.EventHandler(this.saveSpacingButton_Click);
            // 
            // restoreSavedSpacingButton
            // 
            this.restoreSavedSpacingButton.Location = new System.Drawing.Point(372, 588);
            this.restoreSavedSpacingButton.Name = "restoreSavedSpacingButton";
            this.restoreSavedSpacingButton.Size = new System.Drawing.Size(174, 23);
            this.restoreSavedSpacingButton.TabIndex = 19;
            this.restoreSavedSpacingButton.Text = "Restore Saved Spacing";
            this.toolTip1.SetToolTip(this.restoreSavedSpacingButton, "Restore previously Saved Spacing values");
            this.restoreSavedSpacingButton.UseVisualStyleBackColor = true;
            this.restoreSavedSpacingButton.Click += new System.EventHandler(this.restoreSavedSpacingButton_Click);
            // 
            // WordSpacingStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.restoreSavedSpacingButton);
            this.Controls.Add(this.saveSpacingButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.matchSoFarView);
            this.Controls.Add(this.spacingListBox);
            this.Controls.Add(this.italicRadioButton);
            this.Controls.Add(this.normalRadioButton);
            this.Controls.Add(this.restoreDefaultSpacingButton);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.defaultItalicRightLabel);
            this.Controls.Add(this.defaultItalicLeftLabel);
            this.Controls.Add(this.defaultRightLabel);
            this.Controls.Add(this.defaultLeftLabel);
            this.Controls.Add(this.kerningItalicRightUpDown);
            this.Controls.Add(this.kerningItalicLeftUpDown);
            this.Controls.Add(this.kerningRightUpDown);
            this.Controls.Add(this.kerningCharacterComboBox);
            this.Controls.Add(this.kerningLeftUpDown);
            this.Controls.Add(this.dvdLabel);
            this.Name = "WordSpacingStep";
            this.Size = new System.Drawing.Size(842, 662);
            ((System.ComponentModel.ISupportInitialize)(this.kerningItalicRightUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.kerningItalicLeftUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.kerningRightUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.kerningLeftUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label dvdLabel;
        private System.Windows.Forms.RadioButton italicRadioButton;
        private System.Windows.Forms.RadioButton normalRadioButton;
        private System.Windows.Forms.Button restoreDefaultSpacingButton;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label defaultItalicRightLabel;
        private System.Windows.Forms.Label defaultItalicLeftLabel;
        private System.Windows.Forms.Label defaultRightLabel;
        private System.Windows.Forms.Label defaultLeftLabel;
        private System.Windows.Forms.NumericUpDown kerningItalicRightUpDown;
        private System.Windows.Forms.NumericUpDown kerningItalicLeftUpDown;
        private System.Windows.Forms.NumericUpDown kerningRightUpDown;
        private System.Windows.Forms.ComboBox kerningCharacterComboBox;
        private System.Windows.Forms.NumericUpDown kerningLeftUpDown;
        private System.Windows.Forms.ListBox spacingListBox;
        private DvdSubOcr.MatchSoFarView matchSoFarView;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button saveSpacingButton;
        private System.Windows.Forms.Button restoreSavedSpacingButton;

    }
}
