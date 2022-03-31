using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace DvdSubExtractor
{
    public partial class HandbrakeForm : Form
    {
        EnumItem<AudioEncoder>[] allEncoders = EnumItem<AudioEncoder>.Convert(
            AudioEncoder.AAC, AudioEncoder.MP3, AudioEncoder.Passthru).ToArray();
        EnumItem<AudioMixing>[] aacMixing = EnumItem<AudioMixing>.Convert(
            AudioMixing.Stereo, AudioMixing.Dolby_ProLogic_II, AudioMixing.Dolby_Surround,
            AudioMixing._6_Channel_Discreet).ToArray();
        EnumItem<AudioMixing>[] mp3Mixing = EnumItem<AudioMixing>.Convert(
            AudioMixing.Stereo, AudioMixing.Dolby_ProLogic_II, 
            AudioMixing.Dolby_Surround).ToArray();
        EnumItem<AudioMixing>[] stereoMixing = EnumItem<AudioMixing>.Convert(AudioMixing.Stereo).ToArray();
        EnumItem<AudioBitrate>[] allBitrates = EnumItem<AudioBitrate>.Convert(
            AudioBitrate._160, AudioBitrate._192, AudioBitrate._224, 
            AudioBitrate._256, AudioBitrate._320).ToArray();

        HandbrakeOptions options;

        public HandbrakeForm()
        {
            InitializeComponent();

            this.ac3Channels6DestComboBox.Items.AddRange(allEncoders);
            this.ac3Channels2DestComboBox.Items.AddRange(allEncoders);
            this.dtsChannels6DestComboBox.Items.AddRange(allEncoders);
            this.lpcmChannels2DestComboBox.Items.AddRange(allEncoders);
            this.mpeg2Channels2DestComboBox.Items.AddRange(allEncoders);

            this.ac3Channels6MixingComboBox.Items.AddRange(aacMixing);
            this.dtsChannels6MixingComboBox.Items.AddRange(mp3Mixing);
            this.ac3Channels2MixingComboBox.Items.AddRange(stereoMixing);
            this.mpeg2Channels2MixingComboBox.Items.AddRange(stereoMixing);
            this.lpcmChannels2MixingComboBox.Items.AddRange(stereoMixing);

            this.detelecineComboBox.Items.AddRange(
                EnumItem<VideoDetelecine>.Convert(
                Enum.GetValues(typeof(VideoDetelecine))).ToArray());
            this.deinterlaceComboBox.Items.AddRange(
                EnumItem<VideoDeinterlace>.Convert(
                Enum.GetValues(typeof(VideoDeinterlace))).ToArray());
            this.denoiseComboBox.Items.AddRange(
                EnumItem<VideoDenoise>.Convert(
                Enum.GetValues(typeof(VideoDenoise))).ToArray());
            this.deblockComboBox.Items.AddRange(
                EnumItem<VideoDeblock>.Convert(
                Enum.GetValues(typeof(VideoDeblock))).ToArray());

            this.qualityUpDown.Minimum = (decimal)VideoProfile.ConstantQualityMin;
            this.qualityUpDown.Maximum = (decimal)VideoProfile.ConstantQualityMax;
            this.qualityUpDown.Increment = (decimal)VideoProfile.ConstantQualityStep;
            this.bitrateUpDown.Minimum = VideoProfile.AverageBitrateMin;
            this.bitrateUpDown.Maximum = VideoProfile.AverageBitrateMax;
            this.bitrateUpDown.Increment = VideoProfile.AverageBitrateStep;

            this.options = HandbrakeXml.Load(Properties.Settings.Default.HandbrakeXml);
            if(this.options == null)
            {
                this.options = new HandbrakeOptions();
                LoadDefaultAudioChoices();
                SaveToOptions();
            }
            else
            {
                LoadFromOptions();
            }
        }

        class EnumItem<T> where T : struct
        {
            public EnumItem(T value)
            {
                this.Value = value;
                this.Display = value.ToString().Replace('_', ' ').Trim();
            }

            public T Value { get; private set; }
            public string Display { get; private set; }

            public static IEnumerable<EnumItem<T>> Convert(params T[] original)
            {
                return original.Select(e => new EnumItem<T>(e));
            }

            public static IEnumerable<EnumItem<T>> Convert(Array original)
            {
                return original.OfType<T>().Select(e => new EnumItem<T>(e));
            }

            public override string ToString()
            {
                return this.Display;
            }

            public override bool Equals(object obj)
            {
                EnumItem<T> other = obj as EnumItem<T>;
                if(other != null)
                {
                    return this.Value.Equals(other.Value);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return this.Value.GetHashCode();
            }
        }

        void LoadDefaultAudioChoices()
        {
            this.ac3Channels6DestComboBox.SelectedItem = new EnumItem<AudioEncoder>(AudioEncoder.Passthru);
            this.dtsChannels6DestComboBox.SelectedItem = new EnumItem<AudioEncoder>(AudioEncoder.Passthru);
            this.ac3Channels2DestComboBox.SelectedItem = new EnumItem<AudioEncoder>(AudioEncoder.AAC);
            this.lpcmChannels2DestComboBox.SelectedItem = new EnumItem<AudioEncoder>(AudioEncoder.AAC);
            this.mpeg2Channels2DestComboBox.SelectedItem = new EnumItem<AudioEncoder>(AudioEncoder.AAC);

            this.ac3Channels2MixingComboBox.SelectedItem = new EnumItem<AudioMixing>(AudioMixing.Stereo);
            this.lpcmChannels2MixingComboBox.SelectedItem = new EnumItem<AudioMixing>(AudioMixing.Stereo);
            this.mpeg2Channels2MixingComboBox.SelectedItem = new EnumItem<AudioMixing>(AudioMixing.Stereo);

            this.ac3Channels2BitrateComboBox.SelectedItem = new EnumItem<AudioBitrate>(AudioBitrate._256);
            this.lpcmChannels2BitrateComboBox.SelectedItem = new EnumItem<AudioBitrate>(AudioBitrate._256);
            this.mpeg2Channels2BitrateComboBox.SelectedItem = new EnumItem<AudioBitrate>(AudioBitrate._256);
        }

        static void LoadAudioEncoder(EncoderOptions encOptions, 
            ComboBox destCombo, ComboBox mixCombo, ComboBox bitrateCombo)
        {
            destCombo.SelectedItem = new EnumItem<AudioEncoder>(encOptions.Encoder);
            mixCombo.SelectedItem = new EnumItem<AudioMixing>(encOptions.Mixing);
            bitrateCombo.SelectedItem = new EnumItem<AudioBitrate>(encOptions.Bitrate);
        }

        void LoadFromOptions()
        {
            LoadAudioEncoder(this.options.Ac3Channels6, this.ac3Channels6DestComboBox,
                this.ac3Channels6MixingComboBox, this.ac3Channels6BitrateComboBox);
            LoadAudioEncoder(this.options.Ac3Channels2, this.ac3Channels2DestComboBox,
                this.ac3Channels2MixingComboBox, this.ac3Channels2BitrateComboBox);
            LoadAudioEncoder(this.options.DtsChannels6, this.dtsChannels6DestComboBox,
                this.dtsChannels6MixingComboBox, this.dtsChannels6BitrateComboBox);
            LoadAudioEncoder(this.options.Mpeg2Channels2, this.mpeg2Channels2DestComboBox,
                this.mpeg2Channels2MixingComboBox, this.mpeg2Channels2BitrateComboBox);
            LoadAudioEncoder(this.options.LpcmChannels2, this.lpcmChannels2DestComboBox,
                this.lpcmChannels2MixingComboBox, this.lpcmChannels2BitrateComboBox);

            this.profileListBox.Items.AddRange(this.options.Profiles.ToArray());
            if(this.options.Profiles.Count != 0)
            {
                this.profileListBox.SelectedIndex = 0;
            }
            else
            {
                profileListBox_SelectedIndexChanged(this, EventArgs.Empty);
                //this.profileListBox.SelectedIndex = -1;
            }
        }

        static bool TryGetComboValue<T>(ComboBox combo, ref T value) where T : struct
        {
            if(combo.SelectedIndex != -1)
            {
                EnumItem<T> item = combo.SelectedItem as EnumItem<T>;
                if(item != null)
                {
                    value = item.Value;
                    return true;
                }
            }
            return false;
        }

        static EncoderOptions SaveAudioEncoder(
            ComboBox destCombo, ComboBox mixCombo, ComboBox bitCombo)
        {
            AudioEncoder enc = AudioEncoder.Passthru;
            AudioMixing mix = AudioMixing.Stereo;
            AudioBitrate bitrate = AudioBitrate._256;
            if(TryGetComboValue(destCombo, ref enc))
            {
                TryGetComboValue(mixCombo, ref mix);
                TryGetComboValue(bitCombo, ref bitrate);
            }
            return new EncoderOptions()
            {
                Encoder = enc,
                Mixing = mix,
                Bitrate = bitrate
            };
        }

        void SaveToOptions()
        {
            this.options.Ac3Channels6 = SaveAudioEncoder(this.ac3Channels6DestComboBox,
                this.ac3Channels6MixingComboBox, this.ac3Channels6BitrateComboBox);
            this.options.Ac3Channels2 = SaveAudioEncoder(this.ac3Channels2DestComboBox,
                this.ac3Channels2MixingComboBox, this.ac3Channels2BitrateComboBox);
            this.options.DtsChannels6 = SaveAudioEncoder(this.dtsChannels6DestComboBox,
                this.dtsChannels6MixingComboBox, this.dtsChannels6BitrateComboBox);
            this.options.Mpeg2Channels2 = SaveAudioEncoder(this.mpeg2Channels2DestComboBox,
                this.mpeg2Channels2MixingComboBox, this.mpeg2Channels2BitrateComboBox);
            this.options.LpcmChannels2 = SaveAudioEncoder(this.lpcmChannels2DestComboBox,
                this.lpcmChannels2MixingComboBox, this.lpcmChannels2BitrateComboBox);
        }

        private void ac3Channels6DestComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ac3Channels6MixingComboBox.Items.Clear();
            this.ac3Channels6BitrateComboBox.Items.Clear();
            if(this.ac3Channels6DestComboBox.SelectedIndex != -1)
            {
                EnumItem<AudioEncoder> item = this.ac3Channels6DestComboBox.SelectedItem as EnumItem<AudioEncoder>;
                switch(item.Value)
                {
                case AudioEncoder.AAC:
                    this.ac3Channels6MixingComboBox.Items.AddRange(aacMixing);
                    this.ac3Channels6MixingComboBox.SelectedItem = 
                        new EnumItem<AudioMixing>(AudioMixing.Dolby_ProLogic_II);
                    this.ac3Channels6BitrateComboBox.Items.AddRange(allBitrates);
                    this.ac3Channels6BitrateComboBox.SelectedItem =
                        new EnumItem<AudioBitrate>(AudioBitrate._256);
                    break;
                case AudioEncoder.MP3:
                    this.ac3Channels6MixingComboBox.Items.AddRange(mp3Mixing);
                    this.ac3Channels6MixingComboBox.SelectedItem =
                        new EnumItem<AudioMixing>(AudioMixing.Dolby_ProLogic_II);
                    this.ac3Channels6BitrateComboBox.Items.AddRange(allBitrates);
                    this.ac3Channels6BitrateComboBox.SelectedItem =
                        new EnumItem<AudioBitrate>(AudioBitrate._256);
                    break;
                case AudioEncoder.Passthru:
                    break;
                }
            }
            this.ac3Channels6MixingComboBox.Enabled = this.ac3Channels6MixingComboBox.Items.Count > 1;
            this.ac3Channels6BitrateComboBox.Enabled = this.ac3Channels6BitrateComboBox.Items.Count > 1;
        }

        private void ac3Channels2DestComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ac3Channels2MixingComboBox.Items.Clear();
            this.ac3Channels2BitrateComboBox.Items.Clear();
            if(this.ac3Channels2DestComboBox.SelectedIndex != -1)
            {
                EnumItem<AudioEncoder> item = this.ac3Channels2DestComboBox.SelectedItem as EnumItem<AudioEncoder>;
                switch(item.Value)
                {
                case AudioEncoder.AAC:
                    this.ac3Channels2MixingComboBox.Items.AddRange(stereoMixing);
                    this.ac3Channels2MixingComboBox.SelectedItem =
                        new EnumItem<AudioMixing>(AudioMixing.Stereo);
                    this.ac3Channels2BitrateComboBox.Items.AddRange(allBitrates);
                    this.ac3Channels2BitrateComboBox.SelectedItem =
                        new EnumItem<AudioBitrate>(AudioBitrate._256);
                    break;
                case AudioEncoder.MP3:
                    this.ac3Channels2MixingComboBox.Items.AddRange(stereoMixing);
                    this.ac3Channels2MixingComboBox.SelectedItem =
                        new EnumItem<AudioMixing>(AudioMixing.Stereo);
                    this.ac3Channels2BitrateComboBox.Items.AddRange(allBitrates);
                    this.ac3Channels2BitrateComboBox.SelectedItem =
                        new EnumItem<AudioBitrate>(AudioBitrate._256);
                    break;
                case AudioEncoder.Passthru:
                    break;
                }
            }
            this.ac3Channels2MixingComboBox.Enabled = this.ac3Channels2MixingComboBox.Items.Count > 1;
            this.ac3Channels2BitrateComboBox.Enabled = this.ac3Channels2BitrateComboBox.Items.Count > 1;
        }

        private void dtsChannels6DestComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.dtsChannels6MixingComboBox.Items.Clear();
            this.dtsChannels6BitrateComboBox.Items.Clear();
            if(this.dtsChannels6DestComboBox.SelectedIndex != -1)
            {
                EnumItem<AudioEncoder> item = this.dtsChannels6DestComboBox.SelectedItem as EnumItem<AudioEncoder>;
                switch(item.Value)
                {
                case AudioEncoder.AAC:
                    this.dtsChannels6MixingComboBox.Items.AddRange(aacMixing);
                    this.dtsChannels6MixingComboBox.SelectedItem =
                        new EnumItem<AudioMixing>(AudioMixing.Dolby_ProLogic_II);
                    this.dtsChannels6BitrateComboBox.Items.AddRange(allBitrates);
                    this.dtsChannels6BitrateComboBox.SelectedItem =
                        new EnumItem<AudioBitrate>(AudioBitrate._256);
                    break;
                case AudioEncoder.MP3:
                    this.dtsChannels6MixingComboBox.Items.AddRange(mp3Mixing);
                    this.dtsChannels6MixingComboBox.SelectedItem =
                        new EnumItem<AudioMixing>(AudioMixing.Dolby_ProLogic_II);
                    this.dtsChannels6BitrateComboBox.Items.AddRange(allBitrates);
                    this.dtsChannels6BitrateComboBox.SelectedItem =
                        new EnumItem<AudioBitrate>(AudioBitrate._256);
                    break;
                case AudioEncoder.Passthru:
                    break;
                }
            }
            this.dtsChannels6MixingComboBox.Enabled = this.dtsChannels6MixingComboBox.Items.Count > 1;
            this.dtsChannels6BitrateComboBox.Enabled = this.dtsChannels6BitrateComboBox.Items.Count > 1;
        }

        private void mpeg2Channels2DestComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.mpeg2Channels2MixingComboBox.Items.Clear();
            this.mpeg2Channels2BitrateComboBox.Items.Clear();
            if(this.mpeg2Channels2DestComboBox.SelectedIndex != -1)
            {
                EnumItem<AudioEncoder> item = this.mpeg2Channels2DestComboBox.SelectedItem as EnumItem<AudioEncoder>;
                switch(item.Value)
                {
                case AudioEncoder.AAC:
                    this.mpeg2Channels2MixingComboBox.Items.AddRange(stereoMixing);
                    this.mpeg2Channels2MixingComboBox.SelectedItem =
                        new EnumItem<AudioMixing>(AudioMixing.Stereo);
                    this.mpeg2Channels2BitrateComboBox.Items.AddRange(allBitrates);
                    this.mpeg2Channels2BitrateComboBox.SelectedItem =
                        new EnumItem<AudioBitrate>(AudioBitrate._256);
                    break;
                case AudioEncoder.MP3:
                    this.mpeg2Channels2MixingComboBox.Items.AddRange(stereoMixing);
                    this.mpeg2Channels2MixingComboBox.SelectedItem =
                        new EnumItem<AudioMixing>(AudioMixing.Stereo);
                    this.mpeg2Channels2BitrateComboBox.Items.AddRange(allBitrates);
                    this.mpeg2Channels2BitrateComboBox.SelectedItem =
                        new EnumItem<AudioBitrate>(AudioBitrate._256);
                    break;
                case AudioEncoder.Passthru:
                    break;
                }
            }
            this.mpeg2Channels2MixingComboBox.Enabled = this.mpeg2Channels2MixingComboBox.Items.Count > 1;
            this.mpeg2Channels2BitrateComboBox.Enabled = this.mpeg2Channels2BitrateComboBox.Items.Count > 1;
        }

        private void lpcmChannels2DestComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.lpcmChannels2MixingComboBox.Items.Clear();
            this.lpcmChannels2BitrateComboBox.Items.Clear();
            if(this.lpcmChannels2DestComboBox.SelectedIndex != -1)
            {
                EnumItem<AudioEncoder> item = this.lpcmChannels2DestComboBox.SelectedItem as EnumItem<AudioEncoder>;
                switch(item.Value)
                {
                case AudioEncoder.AAC:
                    this.lpcmChannels2MixingComboBox.Items.AddRange(stereoMixing);
                    this.lpcmChannels2MixingComboBox.SelectedItem =
                        new EnumItem<AudioMixing>(AudioMixing.Stereo);
                    this.lpcmChannels2BitrateComboBox.Items.AddRange(allBitrates);
                    this.lpcmChannels2BitrateComboBox.SelectedItem =
                        new EnumItem<AudioBitrate>(AudioBitrate._256);
                    break;
                case AudioEncoder.MP3:
                    this.lpcmChannels2MixingComboBox.Items.AddRange(stereoMixing);
                    this.lpcmChannels2MixingComboBox.SelectedItem =
                        new EnumItem<AudioMixing>(AudioMixing.Stereo);
                    this.lpcmChannels2BitrateComboBox.Items.AddRange(allBitrates);
                    this.lpcmChannels2BitrateComboBox.SelectedItem =
                        new EnumItem<AudioBitrate>(AudioBitrate._256);
                    break;
                case AudioEncoder.Passthru:
                    break;
                }
            }
            this.lpcmChannels2MixingComboBox.Enabled = this.lpcmChannels2MixingComboBox.Items.Count > 1;
            this.lpcmChannels2BitrateComboBox.Enabled = this.lpcmChannels2BitrateComboBox.Items.Count > 1;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            SaveToOptions();
            Properties.Settings.Default.HandbrakeXml = HandbrakeXml.Save(this.options);
            Properties.Settings.Default.Save();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {

        }

        private void profileListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            IEnumerable<Control> videoControls = new Control[] {
                this.deblockComboBox, this.detelecineComboBox,
                this.deinterlaceComboBox, this.denoiseComboBox,
                this.qualityPanel, this.profileX264OptionsTextBox,
                this.extraOptionsTextBox };
            if(this.profileListBox.SelectedIndex == -1)
            {
                foreach(Control control in videoControls)
                {
                    control.Enabled = false;
                }
                this.qualityRadioButton.Checked = false;
                this.bitrateRadioButton.Checked = false;
                this.profileX264OptionsTextBox.Text = "";
                this.extraOptionsTextBox.Text = "";
            }
            else
            {
                foreach(Control control in videoControls)
                {
                    control.Enabled = true;
                }

                VideoProfile profile = this.profileListBox.SelectedItem as VideoProfile;
                this.deblockComboBox.SelectedItem = new EnumItem<VideoDeblock>(profile.Deblock);
                this.detelecineComboBox.SelectedItem = new EnumItem<VideoDetelecine>(profile.Detelecine);
                this.deinterlaceComboBox.SelectedItem = new EnumItem<VideoDeinterlace>(profile.Deinterlace);
                this.denoiseComboBox.SelectedItem = new EnumItem<VideoDenoise>(profile.Denoise);

                this.qualityUpDown.Value = (decimal)profile.Quality;
                this.bitrateUpDown.Value = (decimal)profile.Bitrate;
                if(profile.QualityType == VideoQualityType.ConstantQuality)
                {
                    this.qualityRadioButton.Checked = true;
                    this.bitrateUpDown.Enabled = false;
                }
                else
                {
                    this.bitrateRadioButton.Checked = true;
                    this.qualityUpDown.Enabled = false;
                }
                this.profileX264OptionsTextBox.Text = profile.x264Options;
                this.extraOptionsTextBox.Text = profile.ExtraOptions;
            }
        }

        private void addProfileButton_Click(object sender, EventArgs e)
        {

        }

        private void editProfileButton_Click(object sender, EventArgs e)
        {

        }

        private void removeProfileButton_Click(object sender, EventArgs e)
        {

        }
    }
}
