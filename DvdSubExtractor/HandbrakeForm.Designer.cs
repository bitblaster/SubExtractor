namespace DvdSubExtractor
{
    partial class HandbrakeForm
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ac3Channels6DestComboBox = new System.Windows.Forms.ComboBox();
            this.ac3Channels6MixingComboBox = new System.Windows.Forms.ComboBox();
            this.ac3Channels2MixingComboBox = new System.Windows.Forms.ComboBox();
            this.ac3Channels2DestComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dtsChannels6MixingComboBox = new System.Windows.Forms.ComboBox();
            this.dtsChannels6DestComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.mpeg2Channels2MixingComboBox = new System.Windows.Forms.ComboBox();
            this.mpeg2Channels2DestComboBox = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.lpcmChannels2MixingComboBox = new System.Windows.Forms.ComboBox();
            this.lpcmChannels2DestComboBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.ac3Channels6BitrateComboBox = new System.Windows.Forms.ComboBox();
            this.ac3Channels2BitrateComboBox = new System.Windows.Forms.ComboBox();
            this.dtsChannels6BitrateComboBox = new System.Windows.Forms.ComboBox();
            this.mpeg2Channels2BitrateComboBox = new System.Windows.Forms.ComboBox();
            this.lpcmChannels2BitrateComboBox = new System.Windows.Forms.ComboBox();
            this.profileX264OptionsTextBox = new System.Windows.Forms.TextBox();
            this.profileListBox = new System.Windows.Forms.ListBox();
            this.addProfileButton = new System.Windows.Forms.Button();
            this.removeProfileButton = new System.Windows.Forms.Button();
            this.detelecineComboBox = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.deinterlaceComboBox = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.denoiseComboBox = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.deblockComboBox = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.qualityPanel = new System.Windows.Forms.Panel();
            this.bitrateUpDown = new System.Windows.Forms.NumericUpDown();
            this.bitrateRadioButton = new System.Windows.Forms.RadioButton();
            this.qualityUpDown = new System.Windows.Forms.NumericUpDown();
            this.qualityRadioButton = new System.Windows.Forms.RadioButton();
            this.label14 = new System.Windows.Forms.Label();
            this.editProfileButton = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.extraOptionsTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.qualityPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bitrateUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityUpDown)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(150, 503);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(95, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(27, 503);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(95, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(26, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Source Audio Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "AC3 6 Channel";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(164, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(139, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Destination Audio Type";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(454, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Mixing";
            // 
            // ac3Channels6DestComboBox
            // 
            this.ac3Channels6DestComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ac3Channels6DestComboBox.FormattingEnabled = true;
            this.ac3Channels6DestComboBox.Location = new System.Drawing.Point(178, 41);
            this.ac3Channels6DestComboBox.Name = "ac3Channels6DestComboBox";
            this.ac3Channels6DestComboBox.Size = new System.Drawing.Size(117, 21);
            this.ac3Channels6DestComboBox.TabIndex = 10;
            this.ac3Channels6DestComboBox.SelectedIndexChanged += new System.EventHandler(this.ac3Channels6DestComboBox_SelectedIndexChanged);
            // 
            // ac3Channels6MixingComboBox
            // 
            this.ac3Channels6MixingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ac3Channels6MixingComboBox.FormattingEnabled = true;
            this.ac3Channels6MixingComboBox.Location = new System.Drawing.Point(420, 41);
            this.ac3Channels6MixingComboBox.Name = "ac3Channels6MixingComboBox";
            this.ac3Channels6MixingComboBox.Size = new System.Drawing.Size(117, 21);
            this.ac3Channels6MixingComboBox.TabIndex = 11;
            // 
            // ac3Channels2MixingComboBox
            // 
            this.ac3Channels2MixingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ac3Channels2MixingComboBox.FormattingEnabled = true;
            this.ac3Channels2MixingComboBox.Location = new System.Drawing.Point(420, 69);
            this.ac3Channels2MixingComboBox.Name = "ac3Channels2MixingComboBox";
            this.ac3Channels2MixingComboBox.Size = new System.Drawing.Size(117, 21);
            this.ac3Channels2MixingComboBox.TabIndex = 14;
            // 
            // ac3Channels2DestComboBox
            // 
            this.ac3Channels2DestComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ac3Channels2DestComboBox.FormattingEnabled = true;
            this.ac3Channels2DestComboBox.Location = new System.Drawing.Point(178, 69);
            this.ac3Channels2DestComboBox.Name = "ac3Channels2DestComboBox";
            this.ac3Channels2DestComboBox.Size = new System.Drawing.Size(117, 21);
            this.ac3Channels2DestComboBox.TabIndex = 13;
            this.ac3Channels2DestComboBox.SelectedIndexChanged += new System.EventHandler(this.ac3Channels2DestComboBox_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(43, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "AC3 2 Channel";
            // 
            // dtsChannels6MixingComboBox
            // 
            this.dtsChannels6MixingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dtsChannels6MixingComboBox.FormattingEnabled = true;
            this.dtsChannels6MixingComboBox.Location = new System.Drawing.Point(420, 97);
            this.dtsChannels6MixingComboBox.Name = "dtsChannels6MixingComboBox";
            this.dtsChannels6MixingComboBox.Size = new System.Drawing.Size(117, 21);
            this.dtsChannels6MixingComboBox.TabIndex = 17;
            // 
            // dtsChannels6DestComboBox
            // 
            this.dtsChannels6DestComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dtsChannels6DestComboBox.FormattingEnabled = true;
            this.dtsChannels6DestComboBox.Location = new System.Drawing.Point(178, 97);
            this.dtsChannels6DestComboBox.Name = "dtsChannels6DestComboBox";
            this.dtsChannels6DestComboBox.Size = new System.Drawing.Size(117, 21);
            this.dtsChannels6DestComboBox.TabIndex = 16;
            this.dtsChannels6DestComboBox.SelectedIndexChanged += new System.EventHandler(this.dtsChannels6DestComboBox_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(43, 100);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "DTS 6 Channel";
            // 
            // mpeg2Channels2MixingComboBox
            // 
            this.mpeg2Channels2MixingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mpeg2Channels2MixingComboBox.FormattingEnabled = true;
            this.mpeg2Channels2MixingComboBox.Location = new System.Drawing.Point(420, 125);
            this.mpeg2Channels2MixingComboBox.Name = "mpeg2Channels2MixingComboBox";
            this.mpeg2Channels2MixingComboBox.Size = new System.Drawing.Size(117, 21);
            this.mpeg2Channels2MixingComboBox.TabIndex = 20;
            // 
            // mpeg2Channels2DestComboBox
            // 
            this.mpeg2Channels2DestComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mpeg2Channels2DestComboBox.FormattingEnabled = true;
            this.mpeg2Channels2DestComboBox.Location = new System.Drawing.Point(178, 125);
            this.mpeg2Channels2DestComboBox.Name = "mpeg2Channels2DestComboBox";
            this.mpeg2Channels2DestComboBox.Size = new System.Drawing.Size(117, 21);
            this.mpeg2Channels2DestComboBox.TabIndex = 19;
            this.mpeg2Channels2DestComboBox.SelectedIndexChanged += new System.EventHandler(this.mpeg2Channels2DestComboBox_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(43, 128);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "MPEG 2 Channel";
            // 
            // lpcmChannels2MixingComboBox
            // 
            this.lpcmChannels2MixingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lpcmChannels2MixingComboBox.FormattingEnabled = true;
            this.lpcmChannels2MixingComboBox.Location = new System.Drawing.Point(420, 153);
            this.lpcmChannels2MixingComboBox.Name = "lpcmChannels2MixingComboBox";
            this.lpcmChannels2MixingComboBox.Size = new System.Drawing.Size(117, 21);
            this.lpcmChannels2MixingComboBox.TabIndex = 23;
            // 
            // lpcmChannels2DestComboBox
            // 
            this.lpcmChannels2DestComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lpcmChannels2DestComboBox.FormattingEnabled = true;
            this.lpcmChannels2DestComboBox.Location = new System.Drawing.Point(178, 153);
            this.lpcmChannels2DestComboBox.Name = "lpcmChannels2DestComboBox";
            this.lpcmChannels2DestComboBox.Size = new System.Drawing.Size(117, 21);
            this.lpcmChannels2DestComboBox.TabIndex = 22;
            this.lpcmChannels2DestComboBox.SelectedIndexChanged += new System.EventHandler(this.lpcmChannels2DestComboBox_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(43, 156);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(87, 13);
            this.label8.TabIndex = 21;
            this.label8.Text = "LPCM 2 Channel";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(337, 21);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(44, 13);
            this.label9.TabIndex = 24;
            this.label9.Text = "Bitrate";
            // 
            // ac3Channels6BitrateComboBox
            // 
            this.ac3Channels6BitrateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ac3Channels6BitrateComboBox.FormattingEnabled = true;
            this.ac3Channels6BitrateComboBox.Location = new System.Drawing.Point(317, 41);
            this.ac3Channels6BitrateComboBox.Name = "ac3Channels6BitrateComboBox";
            this.ac3Channels6BitrateComboBox.Size = new System.Drawing.Size(83, 21);
            this.ac3Channels6BitrateComboBox.TabIndex = 25;
            // 
            // ac3Channels2BitrateComboBox
            // 
            this.ac3Channels2BitrateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ac3Channels2BitrateComboBox.FormattingEnabled = true;
            this.ac3Channels2BitrateComboBox.Location = new System.Drawing.Point(317, 69);
            this.ac3Channels2BitrateComboBox.Name = "ac3Channels2BitrateComboBox";
            this.ac3Channels2BitrateComboBox.Size = new System.Drawing.Size(83, 21);
            this.ac3Channels2BitrateComboBox.TabIndex = 26;
            // 
            // dtsChannels6BitrateComboBox
            // 
            this.dtsChannels6BitrateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dtsChannels6BitrateComboBox.FormattingEnabled = true;
            this.dtsChannels6BitrateComboBox.Location = new System.Drawing.Point(317, 97);
            this.dtsChannels6BitrateComboBox.Name = "dtsChannels6BitrateComboBox";
            this.dtsChannels6BitrateComboBox.Size = new System.Drawing.Size(83, 21);
            this.dtsChannels6BitrateComboBox.TabIndex = 27;
            // 
            // mpeg2Channels2BitrateComboBox
            // 
            this.mpeg2Channels2BitrateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mpeg2Channels2BitrateComboBox.FormattingEnabled = true;
            this.mpeg2Channels2BitrateComboBox.Location = new System.Drawing.Point(317, 125);
            this.mpeg2Channels2BitrateComboBox.Name = "mpeg2Channels2BitrateComboBox";
            this.mpeg2Channels2BitrateComboBox.Size = new System.Drawing.Size(83, 21);
            this.mpeg2Channels2BitrateComboBox.TabIndex = 28;
            // 
            // lpcmChannels2BitrateComboBox
            // 
            this.lpcmChannels2BitrateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lpcmChannels2BitrateComboBox.FormattingEnabled = true;
            this.lpcmChannels2BitrateComboBox.Location = new System.Drawing.Point(317, 153);
            this.lpcmChannels2BitrateComboBox.Name = "lpcmChannels2BitrateComboBox";
            this.lpcmChannels2BitrateComboBox.Size = new System.Drawing.Size(83, 21);
            this.lpcmChannels2BitrateComboBox.TabIndex = 29;
            // 
            // profileX264OptionsTextBox
            // 
            this.profileX264OptionsTextBox.Location = new System.Drawing.Point(159, 150);
            this.profileX264OptionsTextBox.Multiline = true;
            this.profileX264OptionsTextBox.Name = "profileX264OptionsTextBox";
            this.profileX264OptionsTextBox.Size = new System.Drawing.Size(395, 44);
            this.profileX264OptionsTextBox.TabIndex = 30;
            // 
            // profileListBox
            // 
            this.profileListBox.FormattingEnabled = true;
            this.profileListBox.Location = new System.Drawing.Point(6, 16);
            this.profileListBox.Name = "profileListBox";
            this.profileListBox.Size = new System.Drawing.Size(132, 173);
            this.profileListBox.TabIndex = 31;
            this.profileListBox.SelectedIndexChanged += new System.EventHandler(this.profileListBox_SelectedIndexChanged);
            // 
            // addProfileButton
            // 
            this.addProfileButton.Location = new System.Drawing.Point(18, 199);
            this.addProfileButton.Name = "addProfileButton";
            this.addProfileButton.Size = new System.Drawing.Size(110, 23);
            this.addProfileButton.TabIndex = 32;
            this.addProfileButton.Text = "Add Profile";
            this.addProfileButton.UseVisualStyleBackColor = true;
            this.addProfileButton.Click += new System.EventHandler(this.addProfileButton_Click);
            // 
            // removeProfileButton
            // 
            this.removeProfileButton.Location = new System.Drawing.Point(18, 257);
            this.removeProfileButton.Name = "removeProfileButton";
            this.removeProfileButton.Size = new System.Drawing.Size(110, 23);
            this.removeProfileButton.TabIndex = 33;
            this.removeProfileButton.Text = "Remove Profile";
            this.removeProfileButton.UseVisualStyleBackColor = true;
            this.removeProfileButton.Click += new System.EventHandler(this.removeProfileButton_Click);
            // 
            // detelecineComboBox
            // 
            this.detelecineComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.detelecineComboBox.FormattingEnabled = true;
            this.detelecineComboBox.Location = new System.Drawing.Point(155, 30);
            this.detelecineComboBox.Name = "detelecineComboBox";
            this.detelecineComboBox.Size = new System.Drawing.Size(117, 21);
            this.detelecineComboBox.TabIndex = 35;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(159, 12);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(58, 13);
            this.label10.TabIndex = 34;
            this.label10.Text = "Detelecine";
            // 
            // deinterlaceComboBox
            // 
            this.deinterlaceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deinterlaceComboBox.FormattingEnabled = true;
            this.deinterlaceComboBox.Location = new System.Drawing.Point(155, 77);
            this.deinterlaceComboBox.Name = "deinterlaceComboBox";
            this.deinterlaceComboBox.Size = new System.Drawing.Size(117, 21);
            this.deinterlaceComboBox.TabIndex = 37;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(159, 61);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(106, 13);
            this.label11.TabIndex = 36;
            this.label11.Text = "Decomb/Deinterlace";
            // 
            // denoiseComboBox
            // 
            this.denoiseComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.denoiseComboBox.FormattingEnabled = true;
            this.denoiseComboBox.Location = new System.Drawing.Point(294, 30);
            this.denoiseComboBox.Name = "denoiseComboBox";
            this.denoiseComboBox.Size = new System.Drawing.Size(117, 21);
            this.denoiseComboBox.TabIndex = 39;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(299, 12);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(46, 13);
            this.label12.TabIndex = 38;
            this.label12.Text = "Denoise";
            // 
            // deblockComboBox
            // 
            this.deblockComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deblockComboBox.FormattingEnabled = true;
            this.deblockComboBox.Location = new System.Drawing.Point(294, 77);
            this.deblockComboBox.Name = "deblockComboBox";
            this.deblockComboBox.Size = new System.Drawing.Size(117, 21);
            this.deblockComboBox.TabIndex = 41;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(298, 61);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(47, 13);
            this.label13.TabIndex = 40;
            this.label13.Text = "Deblock";
            // 
            // qualityPanel
            // 
            this.qualityPanel.Controls.Add(this.bitrateUpDown);
            this.qualityPanel.Controls.Add(this.bitrateRadioButton);
            this.qualityPanel.Controls.Add(this.qualityUpDown);
            this.qualityPanel.Controls.Add(this.qualityRadioButton);
            this.qualityPanel.Location = new System.Drawing.Point(417, 12);
            this.qualityPanel.Name = "qualityPanel";
            this.qualityPanel.Size = new System.Drawing.Size(141, 108);
            this.qualityPanel.TabIndex = 42;
            // 
            // bitrateUpDown
            // 
            this.bitrateUpDown.Increment = new decimal(new int[] {
            250,
            0,
            0,
            0});
            this.bitrateUpDown.Location = new System.Drawing.Point(44, 79);
            this.bitrateUpDown.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.bitrateUpDown.Minimum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.bitrateUpDown.Name = "bitrateUpDown";
            this.bitrateUpDown.Size = new System.Drawing.Size(82, 20);
            this.bitrateUpDown.TabIndex = 3;
            this.bitrateUpDown.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // bitrateRadioButton
            // 
            this.bitrateRadioButton.AutoSize = true;
            this.bitrateRadioButton.Location = new System.Drawing.Point(6, 56);
            this.bitrateRadioButton.Name = "bitrateRadioButton";
            this.bitrateRadioButton.Size = new System.Drawing.Size(77, 17);
            this.bitrateRadioButton.TabIndex = 2;
            this.bitrateRadioButton.TabStop = true;
            this.bitrateRadioButton.Text = "Avg Bitrate";
            this.bitrateRadioButton.UseVisualStyleBackColor = true;
            // 
            // qualityUpDown
            // 
            this.qualityUpDown.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.qualityUpDown.Location = new System.Drawing.Point(44, 30);
            this.qualityUpDown.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.qualityUpDown.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.qualityUpDown.Name = "qualityUpDown";
            this.qualityUpDown.Size = new System.Drawing.Size(82, 20);
            this.qualityUpDown.TabIndex = 1;
            this.qualityUpDown.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // qualityRadioButton
            // 
            this.qualityRadioButton.AutoSize = true;
            this.qualityRadioButton.Location = new System.Drawing.Point(6, 7);
            this.qualityRadioButton.Name = "qualityRadioButton";
            this.qualityRadioButton.Size = new System.Drawing.Size(102, 17);
            this.qualityRadioButton.TabIndex = 0;
            this.qualityRadioButton.TabStop = true;
            this.qualityRadioButton.Text = "Constant Quality";
            this.qualityRadioButton.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(163, 134);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(69, 13);
            this.label14.TabIndex = 43;
            this.label14.Text = "x264 Options";
            // 
            // editProfileButton
            // 
            this.editProfileButton.Location = new System.Drawing.Point(18, 228);
            this.editProfileButton.Name = "editProfileButton";
            this.editProfileButton.Size = new System.Drawing.Size(110, 23);
            this.editProfileButton.TabIndex = 44;
            this.editProfileButton.Text = "Save Changes";
            this.editProfileButton.UseVisualStyleBackColor = true;
            this.editProfileButton.Click += new System.EventHandler(this.editProfileButton_Click);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(165, 211);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(148, 13);
            this.label15.TabIndex = 46;
            this.label15.Text = "Additional Handbrake Options";
            // 
            // extraOptionsTextBox
            // 
            this.extraOptionsTextBox.Location = new System.Drawing.Point(159, 227);
            this.extraOptionsTextBox.Multiline = true;
            this.extraOptionsTextBox.Name = "extraOptionsTextBox";
            this.extraOptionsTextBox.Size = new System.Drawing.Size(395, 44);
            this.extraOptionsTextBox.TabIndex = 45;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.profileListBox);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.detelecineComboBox);
            this.groupBox1.Controls.Add(this.extraOptionsTextBox);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.editProfileButton);
            this.groupBox1.Controls.Add(this.deinterlaceComboBox);
            this.groupBox1.Controls.Add(this.removeProfileButton);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.addProfileButton);
            this.groupBox1.Controls.Add(this.denoiseComboBox);
            this.groupBox1.Controls.Add(this.profileX264OptionsTextBox);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.qualityPanel);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.deblockComboBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 203);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(575, 292);
            this.groupBox1.TabIndex = 47;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Video Options";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.lpcmChannels2BitrateComboBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.mpeg2Channels2BitrateComboBox);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.dtsChannels6BitrateComboBox);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.ac3Channels2BitrateComboBox);
            this.groupBox2.Controls.Add(this.ac3Channels6DestComboBox);
            this.groupBox2.Controls.Add(this.ac3Channels6BitrateComboBox);
            this.groupBox2.Controls.Add(this.ac3Channels6MixingComboBox);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.lpcmChannels2MixingComboBox);
            this.groupBox2.Controls.Add(this.ac3Channels2DestComboBox);
            this.groupBox2.Controls.Add(this.lpcmChannels2DestComboBox);
            this.groupBox2.Controls.Add(this.ac3Channels2MixingComboBox);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.mpeg2Channels2MixingComboBox);
            this.groupBox2.Controls.Add(this.dtsChannels6DestComboBox);
            this.groupBox2.Controls.Add(this.mpeg2Channels2DestComboBox);
            this.groupBox2.Controls.Add(this.dtsChannels6MixingComboBox);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Location = new System.Drawing.Point(12, 10);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(575, 187);
            this.groupBox2.TabIndex = 48;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Audio Options";
            // 
            // HandbrakeForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(603, 536);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "HandbrakeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Handbrake Options";
            this.qualityPanel.ResumeLayout(false);
            this.qualityPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bitrateUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityUpDown)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox ac3Channels6DestComboBox;
        private System.Windows.Forms.ComboBox ac3Channels6MixingComboBox;
        private System.Windows.Forms.ComboBox ac3Channels2MixingComboBox;
        private System.Windows.Forms.ComboBox ac3Channels2DestComboBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox dtsChannels6MixingComboBox;
        private System.Windows.Forms.ComboBox dtsChannels6DestComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox mpeg2Channels2MixingComboBox;
        private System.Windows.Forms.ComboBox mpeg2Channels2DestComboBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox lpcmChannels2MixingComboBox;
        private System.Windows.Forms.ComboBox lpcmChannels2DestComboBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox ac3Channels6BitrateComboBox;
        private System.Windows.Forms.ComboBox ac3Channels2BitrateComboBox;
        private System.Windows.Forms.ComboBox dtsChannels6BitrateComboBox;
        private System.Windows.Forms.ComboBox mpeg2Channels2BitrateComboBox;
        private System.Windows.Forms.ComboBox lpcmChannels2BitrateComboBox;
        private System.Windows.Forms.TextBox profileX264OptionsTextBox;
        private System.Windows.Forms.ListBox profileListBox;
        private System.Windows.Forms.Button addProfileButton;
        private System.Windows.Forms.Button removeProfileButton;
        private System.Windows.Forms.ComboBox detelecineComboBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox deinterlaceComboBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox denoiseComboBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox deblockComboBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Panel qualityPanel;
        private System.Windows.Forms.NumericUpDown qualityUpDown;
        private System.Windows.Forms.RadioButton qualityRadioButton;
        private System.Windows.Forms.NumericUpDown bitrateUpDown;
        private System.Windows.Forms.RadioButton bitrateRadioButton;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button editProfileButton;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox extraOptionsTextBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}