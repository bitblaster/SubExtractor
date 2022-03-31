namespace DvdSubExtractor
{
    partial class OcrReviewForm
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
            if(disposing)
            {
                DataDispose();
                if(components != null)
                {
                    components.Dispose();
                }
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
            this.components = new System.ComponentModel.Container();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.splitListBox = new System.Windows.Forms.ListBox();
            this.split1PictureBox = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.split2PictureBox = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.removeSplitButton = new System.Windows.Forms.Button();
            this.doneButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ocrListBox = new System.Windows.Forms.ListBox();
            this.removeCharacterButton = new System.Windows.Forms.Button();
            this.removeOcrButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.removeAllTrainingsButton = new System.Windows.Forms.Button();
            this.removeAllSplitsButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split1PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.split2PictureBox)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox2.Controls.Add(this.splitListBox);
            this.groupBox2.Controls.Add(this.split1PictureBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.split2PictureBox);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.removeSplitButton);
            this.groupBox2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox2.Location = new System.Drawing.Point(339, 7);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(467, 371);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Splits";
            // 
            // splitListBox
            // 
            this.splitListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.splitListBox.FormattingEnabled = true;
            this.splitListBox.Location = new System.Drawing.Point(8, 29);
            this.splitListBox.Name = "splitListBox";
            this.splitListBox.Size = new System.Drawing.Size(131, 327);
            this.splitListBox.TabIndex = 4;
            this.splitListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.splitListBox_DrawItem);
            this.splitListBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.splitListBox_MeasureItem);
            this.splitListBox.SelectedIndexChanged += new System.EventHandler(this.splitListBox_SelectedIndexChanged);
            // 
            // split1PictureBox
            // 
            this.split1PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.split1PictureBox.Location = new System.Drawing.Point(239, 29);
            this.split1PictureBox.Name = "split1PictureBox";
            this.split1PictureBox.Size = new System.Drawing.Size(100, 56);
            this.split1PictureBox.TabIndex = 4;
            this.split1PictureBox.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(236, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Split 1";
            // 
            // split2PictureBox
            // 
            this.split2PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.split2PictureBox.Location = new System.Drawing.Point(345, 29);
            this.split2PictureBox.Name = "split2PictureBox";
            this.split2PictureBox.Size = new System.Drawing.Size(100, 56);
            this.split2PictureBox.TabIndex = 6;
            this.split2PictureBox.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(342, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Split 2";
            // 
            // removeSplitButton
            // 
            this.removeSplitButton.Location = new System.Drawing.Point(145, 29);
            this.removeSplitButton.Name = "removeSplitButton";
            this.removeSplitButton.Size = new System.Drawing.Size(75, 56);
            this.removeSplitButton.TabIndex = 1;
            this.removeSplitButton.Text = "Remove a Split";
            this.toolTip1.SetToolTip(this.removeSplitButton, "Remove the selected saved split operation for all movies");
            this.removeSplitButton.UseVisualStyleBackColor = false;
            this.removeSplitButton.Click += new System.EventHandler(this.removeSplitBbutton_Click);
            // 
            // doneButton
            // 
            this.doneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.doneButton.BackColor = System.Drawing.SystemColors.Control;
            this.doneButton.Location = new System.Drawing.Point(286, 386);
            this.doneButton.Name = "doneButton";
            this.doneButton.Size = new System.Drawing.Size(99, 31);
            this.doneButton.TabIndex = 2;
            this.doneButton.Text = "Done";
            this.toolTip1.SetToolTip(this.doneButton, "Return to OCR step");
            this.doneButton.UseVisualStyleBackColor = false;
            this.doneButton.Click += new System.EventHandler(this.doneButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(this.ocrListBox);
            this.groupBox1.Controls.Add(this.removeCharacterButton);
            this.groupBox1.Controls.Add(this.removeOcrButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(321, 371);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "OCR Training";
            // 
            // ocrListBox
            // 
            this.ocrListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ocrListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.ocrListBox.FormattingEnabled = true;
            this.ocrListBox.Location = new System.Drawing.Point(8, 27);
            this.ocrListBox.Name = "ocrListBox";
            this.ocrListBox.Size = new System.Drawing.Size(142, 327);
            this.ocrListBox.TabIndex = 3;
            this.ocrListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ocrListBox_DrawItem);
            this.ocrListBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.ocrListBox_MeasureItem);
            // 
            // removeCharacterButton
            // 
            this.removeCharacterButton.Location = new System.Drawing.Point(230, 27);
            this.removeCharacterButton.Name = "removeCharacterButton";
            this.removeCharacterButton.Size = new System.Drawing.Size(83, 56);
            this.removeCharacterButton.TabIndex = 2;
            this.removeCharacterButton.Text = "Remove all Trainings for this Character";
            this.toolTip1.SetToolTip(this.removeCharacterButton, "Remove all OCR matches for this highlighted pattern for all movies");
            this.removeCharacterButton.UseVisualStyleBackColor = false;
            this.removeCharacterButton.Click += new System.EventHandler(this.removeCharacterButton_Click);
            // 
            // removeOcrButton
            // 
            this.removeOcrButton.Location = new System.Drawing.Point(156, 27);
            this.removeOcrButton.Name = "removeOcrButton";
            this.removeOcrButton.Size = new System.Drawing.Size(68, 56);
            this.removeOcrButton.TabIndex = 1;
            this.removeOcrButton.Text = "Remove a Training";
            this.toolTip1.SetToolTip(this.removeOcrButton, "Remove the selected OCR match for this and all other movies that use it");
            this.removeOcrButton.UseVisualStyleBackColor = false;
            this.removeOcrButton.Click += new System.EventHandler(this.removeOcrButton_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(819, 181);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 95);
            this.label1.TabIndex = 3;
            this.label1.Text = "These are the rules specific to this DVD or Subtitle File that are used to perfor" +
    "m the OCR. Remove any Errors then hit Done.";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // removeAllTrainingsButton
            // 
            this.removeAllTrainingsButton.BackColor = System.Drawing.SystemColors.Control;
            this.removeAllTrainingsButton.Location = new System.Drawing.Point(854, 34);
            this.removeAllTrainingsButton.Name = "removeAllTrainingsButton";
            this.removeAllTrainingsButton.Size = new System.Drawing.Size(75, 56);
            this.removeAllTrainingsButton.TabIndex = 2;
            this.removeAllTrainingsButton.Text = "Remove All Trainings for Movie";
            this.toolTip1.SetToolTip(this.removeAllTrainingsButton, "Remove all OCR matches for this movie (and remove them for any other movies that " +
        "use the same matches)");
            this.removeAllTrainingsButton.UseVisualStyleBackColor = false;
            this.removeAllTrainingsButton.Click += new System.EventHandler(this.removeAllTrainingsButton_Click);
            // 
            // removeAllSplitsButton
            // 
            this.removeAllSplitsButton.BackColor = System.Drawing.SystemColors.Control;
            this.removeAllSplitsButton.Location = new System.Drawing.Point(854, 111);
            this.removeAllSplitsButton.Name = "removeAllSplitsButton";
            this.removeAllSplitsButton.Size = new System.Drawing.Size(75, 56);
            this.removeAllSplitsButton.TabIndex = 1;
            this.removeAllSplitsButton.Text = "Remove all Splits for Movie";
            this.toolTip1.SetToolTip(this.removeAllSplitsButton, "Remove all Split operations for this movie (and remove them for any other movies " +
        "that use the same splits)");
            this.removeAllSplitsButton.UseVisualStyleBackColor = false;
            this.removeAllSplitsButton.Click += new System.EventHandler(this.removeAllSplitsButton_Click);
            // 
            // OcrReviewForm
            // 
            this.AcceptButton = this.doneButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(971, 424);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.removeAllSplitsButton);
            this.Controls.Add(this.removeAllTrainingsButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.doneButton);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "OcrReviewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Program Ocr Review";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split1PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.split2PictureBox)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.PictureBox split1PictureBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox split2PictureBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button removeSplitButton;
        private System.Windows.Forms.Button doneButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button removeOcrButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button removeCharacterButton;
        private System.Windows.Forms.Button removeAllTrainingsButton;
        private System.Windows.Forms.Button removeAllSplitsButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ListBox ocrListBox;
        private System.Windows.Forms.ListBox splitListBox;
    }
}