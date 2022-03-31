using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DvdNavigatorCrm;

namespace DvdSubExtractor
{
    public partial class ChooseTracksStep : UserControl, IWizardItem
    {
        ExtractData data;
        bool recursiveAudioCheck;
        bool recursiveAngleSelect;
        const string SampleAngleFileName = "SampleAngle.mpg";
        const string SampleAudioFileName = "SampleAudio.mpg";

        public ChooseTracksStep()
        {
            InitializeComponent();
        }

        public void Initialize(ExtractData data)
        {
            this.data = data;
            this.dvdLabel.Text = this.data.DvdName;
            LoadProgramChains();
            this.data.NewStepInitialize(this.IsComplete, true, this.HelpText, new Type[] { typeof(LoadFolderStep) });
        }

        public void Terminate()
        {
            foreach(DvdTrackItem item in this.data.Programs)
            {
                if(!item.AudioStreamsEdited)
                {
                    TryFindMatchingTrackAudio(item);
                }
            }

            string fileName = Path.Combine(Properties.Settings.Default.OutputDirectory,
                SampleAudioFileName);
            try
            {
                if(File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch(Exception)
            {
            }

            string fileName2 = Path.Combine(Properties.Settings.Default.OutputDirectory,
                SampleAngleFileName);
            try
            {
                if(File.Exists(fileName2))
                {
                    File.Delete(fileName2);
                }
            }
            catch(Exception)
            {
            }
        }

        bool IsComplete
        {
            get
            {
                return (this.titleListBox.Items.Count != 0) &&
                    (this.titleListBox.CheckedItems.Count != 0);
            }
        }

        string HelpText
        {
            get
            {
                string text = "This is where you will choose the Programs and Audio Tracks " +
                    "that are going to be made into stand-alone files for OCR'ing and playing " +
                    "outside the DVD folder structure.\n\nCheck the Programs you want to work " +
                    "with in the top list (they are sorted by length), the DVD Angle of the Program " +
                    "(if there is more than 1), and the Audio Tracks you want to keep for each " + 
                    "Program before hitting Next.\n\nMultiple DVD Angles in a program usually exist " +
                    "to choose between English or foreign language Opening and Closing Credits or signs. " + 
                    "To determine which Angle you want to keep you need to create and view a sample of each.\n\n" +
                    "You can see and hear samples of the Programs by hitting one of the 2 Create " +
                    "Sample buttons.  There's no problem is you want to keep more than 1 audio " +
                    "track in the output file, but there's no way to specify which audio track is " +
                    "to be played by default in mpeg video files so further processing " +
                    "will be needed with video editors or muxers like MkvToolnix if you want to " + 
                    "specify a default audio language in your final video.";
                return text;
            }
        }

        void LoadProgramChains()
        {
            this.titleListBox.Items.Clear();

            IList<DvdTrackItem> tracks = this.data.Programs;
            if(tracks.Count != 0)
            {
                foreach(DvdTrackItem item in tracks)
                {
                    int index = this.titleListBox.Items.Add(item);
                    if(item.IsSelected)
                    {
                        this.titleListBox.SetItemChecked(index, true);
                    }
                }
                this.titleListBox.SelectedIndex = 0;
                LoadAudioTracks();
                LoadAngles();
            }
            this.data.IsCurrentStepComplete = this.IsComplete;
        }

        void LoadAngles()
        {
            this.anglesComboBox.Items.Clear();
            this.anglesComboBox.Enabled = false;
            this.createAngleSampleButton.Enabled = false;
            if((this.titleListBox.Items.Count != 0) && (this.titleListBox.SelectedIndex != -1))
            {
                DvdTrackItem item = this.titleListBox.SelectedItem as DvdTrackItem;
                DvdTitle title = item.Title;
                if(title.AngleCount != 0)
                {
                    this.recursiveAngleSelect = true;
                    try
                    {
                        this.anglesComboBox.Enabled = true;
                        this.createAngleSampleButton.Enabled = true;
                        for(int index = 0; index < item.Title.AngleCount; index++)
                        {
                            this.anglesComboBox.Items.Add(index + 1);
                        }
                        this.anglesComboBox.SelectedIndex = item.Angle - 1;
                    }
                    finally
                    {
                        this.recursiveAngleSelect = false;
                    }
                }
            }
        }

        public static bool IsAudioEqual(AudioAttributes x, AudioAttributes y)
        {
            return (x.Channels == y.Channels) && (x.CodeExtension == y.CodeExtension) &&
                (x.CodingMode == y.CodingMode) && (x.Language == y.Language);
        }

        bool TryFindMatchingTrackAudio(DvdTrackItem item)
        {
            int audioCount = item.Title.AudioStreams.Count;
            if(audioCount == 0)
            {
                return false;
            }

            for(int index = 0; index < this.data.Programs.Count; index++)
            {
                DvdTrackItem otherItem = this.data.Programs[index];
                if(object.ReferenceEquals(otherItem, item) || 
                    !otherItem.AudioStreamsEdited || 
                    (audioCount != otherItem.Title.AudioStreams.Count))
                {
                    continue;
                }

                bool foundMatch = true;
                for(int audioIndex = 0; audioIndex < audioCount; audioIndex++)
                {
                    int streamId = item.Title.AudioStreams[audioIndex];
                    int otherStreamId = otherItem.Title.AudioStreams[audioIndex];
                    if(streamId != otherStreamId)
                    {
                        foundMatch = false;
                        break;
                    }
                    if(!IsAudioEqual(item.Title.GetAudioStream(streamId),
                        otherItem.Title.GetAudioStream(streamId)))
                    {
                        foundMatch = false;
                        break;
                    }
                }
                if(foundMatch)
                {
                    item.SelectedAudioStreams.Clear();
                    foreach(int streamId in otherItem.SelectedAudioStreams)
                    {
                        item.SelectedAudioStreams.Add(streamId);
                    }
                    return true;
                }
            }
            return false;
        }

        void LoadAudioTracks()
        {
            this.recursiveAudioCheck = true;
            try
            {
                this.audioCheckedListBox.Items.Clear();
                int selectedIndex = this.titleListBox.SelectedIndex;
                if((this.titleListBox.Items.Count != 0) && (selectedIndex != -1))
                {
                    DvdTrackItem item = this.titleListBox.SelectedItem as DvdTrackItem;
                    DvdTitle title = item.Title;
                    if(!item.AudioStreamsEdited)
                    {
                        TryFindMatchingTrackAudio(item);
                    }
                    item.AudioStreamsEdited = true;
                    foreach(int streamId in title.AudioStreams)
                    {
                        AudioTrackItem audioItem = new AudioTrackItem(streamId, title.GetAudioStream(streamId));
                        int index = this.audioCheckedListBox.Items.Add(audioItem);
                        if(item.SelectedAudioStreams.Contains(streamId))
                        {
                            this.audioCheckedListBox.SetItemChecked(index, true);
                        }
                    }
                    if(this.audioCheckedListBox.Items.Count != 0)
                    {
                        this.audioCheckedListBox.SelectedIndex = 0;
                    }
                }
            }
            finally
            {
                this.recursiveAudioCheck = false;
            }
        }

        void LoadSubtitleTracks()
        {
            this.subtitleListBox.Items.Clear();
            if((this.titleListBox.Items.Count != 0) && (this.titleListBox.SelectedIndex != -1))
            {
                DvdTrackItem item = this.titleListBox.SelectedItem as DvdTrackItem;
                DvdTitle title = item.Title;
                foreach(int trackId in title.SubtitleTracks)
                {
                    SubtitleTrackItem subItem = new SubtitleTrackItem(trackId, title.GetSubtitleTrack(trackId));
                    this.subtitleListBox.Items.Add(subItem);
                }
            }
        }

        private void titleListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadAudioTracks();
            LoadAngles();
            LoadSubtitleTracks();

            bool hasChapters = false;
            bool hasCells = false;
            if((this.titleListBox.Items.Count != 0) && (this.titleListBox.SelectedIndex != -1))
            {
                DvdTrackItem item = this.titleListBox.SelectedItem as DvdTrackItem;
                if(!item.HasBeenSplit)
                {
                    DvdTitle title = item.Title;
                    foreach(TitleCell cell in title.TitleCells.Skip(1))
                    {
                        if(cell.Cell.IsStcDiscontinuity)
                        {
                            hasChapters = true;
                            break;
                        }
                    }
                    hasCells = (title.TitleCells.Count > 1);
                }
            }
            this.splitChaptersButton.Enabled = hasChapters;
            this.splitCellsButton.Enabled = hasCells;
        }

        private void splitChaptersButton_Click(object sender, EventArgs e)
        {
            if((this.titleListBox.Items.Count != 0) && (this.titleListBox.SelectedIndex != -1))
            {
                DvdTrackItem item = this.titleListBox.SelectedItem as DvdTrackItem;
                DvdTitle title = item.Title;

                int startCellIndex = 0;
                List<DvdTrackItem> newItems = new List<DvdTrackItem>();
                for(int index = 0; index < title.TitleCells.Count; index++)
                {
                    if((index == title.TitleCells.Count - 1) || title.TitleCells[index + 1].Cell.IsStcDiscontinuity)
                    {
                        DvdTitle splitTitle = new DvdTitle(item.TitleSet, item.TitleIndex + 1, true);
                        splitTitle.TrimCells(startCellIndex, index - startCellIndex + 1);
                        DvdTrackItem splitItem = new DvdTrackItem(item.TitleSet, item.TitleIndex, splitTitle);
                        splitItem.Angle = item.Angle;
                        newItems.Add(splitItem);
                        startCellIndex = index + 1;
                    }
                }

                item.HasBeenSplit = true;
                this.splitChaptersButton.Enabled = false;
                this.splitCellsButton.Enabled = false;
                this.titleListBox.SetItemChecked(this.titleListBox.SelectedIndex, false);

                int nextIndex = this.titleListBox.SelectedIndex + 1;
                foreach(DvdTrackItem newItem in newItems)
                {
                    this.data.Programs.Insert(nextIndex, newItem);
                    this.titleListBox.Items.Insert(nextIndex, newItem);
                    this.titleListBox.SetItemChecked(nextIndex, true);
                    nextIndex++;
                }
                for(int index = 0; index < this.data.Programs.Count; index++)
                {
                    this.data.Programs[index].ProgramNumber = index + 1;
                }
            }
        }

        private void splitCellsButton_Click(object sender, EventArgs e)
        {
            if((this.titleListBox.Items.Count != 0) && (this.titleListBox.SelectedIndex != -1))
            {
                DvdTrackItem item = this.titleListBox.SelectedItem as DvdTrackItem;
                DvdTitle title = item.Title;

                List<DvdTrackItem> newItems = new List<DvdTrackItem>();
                for(int index = 0; index < title.TitleCells.Count; index++)
                {
                    DvdTitle splitTitle = new DvdTitle(item.TitleSet, item.TitleIndex + 1, true);
                    splitTitle.TrimCells(index, 1);
                    DvdTrackItem splitItem = new DvdTrackItem(item.TitleSet, item.TitleIndex, splitTitle);
                    splitItem.Angle = item.Angle;
                    newItems.Add(splitItem);
                }

                item.HasBeenSplit = true;
                this.splitChaptersButton.Enabled = false;
                this.splitCellsButton.Enabled = false;
                this.titleListBox.SetItemChecked(this.titleListBox.SelectedIndex, false);

                int nextIndex = this.titleListBox.SelectedIndex + 1;
                foreach(DvdTrackItem newItem in newItems)
                {
                    this.data.Programs.Insert(nextIndex, newItem);
                    this.titleListBox.Items.Insert(nextIndex, newItem);
                    this.titleListBox.SetItemChecked(nextIndex, true);
                    nextIndex++;
                }
                for(int index = 0; index < this.data.Programs.Count; index++)
                {
                    this.data.Programs[index].ProgramNumber = index + 1;
                }
            }
        }

        private void createAngleSampleButton_Click(object sender, EventArgs e)
        {
            if(!OptionsForm.DoesOutputPathExist || !OptionsForm.DoesVideoPlayerExist)
            {
                return;
            }

            try
            {
                if((this.titleListBox.Items.Count != 0) && (this.titleListBox.SelectedIndex != -1))
                {
                    DvdTrackItem item = this.titleListBox.SelectedItem as DvdTrackItem;
                    int angle = (int)this.anglesComboBox.SelectedItem;
                    string fileName = Path.Combine(Properties.Settings.Default.OutputDirectory,
                        SampleAngleFileName);
                    double sampleLengthMs = Properties.Settings.Default.SampleVideoLength * 1000.0;
                    TitleSaver saver = new TitleSaver(item.TitleSet, item.TitleIndex, item.Title,
                        new int[] { angle }, null, fileName, null, 0.0, sampleLengthMs);
                    this.sampleUpdatesLabel.Visible = true;
                    saver.Run(new Action<string>(this.UpdateSaveSampleStatus));
                    this.sampleUpdatesLabel.Visible = false;
                    Process.Start(Properties.Settings.Default.SampleVideoPlayerPath,
                        "\"" + fileName + "\"");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Exception occurred while creating sample: " + ex.Message);
            }
        }

        private void createSampleButton_Click(object sender, EventArgs e)
        {
            if(!OptionsForm.DoesOutputPathExist || !OptionsForm.DoesVideoPlayerExist)
            {
                return;
            }

            try
            {
                if((this.titleListBox.Items.Count != 0) && (this.titleListBox.SelectedIndex != -1))
                {
                    DvdTrackItem item = this.titleListBox.SelectedItem as DvdTrackItem;
                    List<int> angles = new List<int>();
                    angles.Add(0);
                    if(this.anglesComboBox.Enabled)
                    {
                        angles.Add((int)this.anglesComboBox.SelectedItem);
                    }
                    string fileName = Path.Combine(Properties.Settings.Default.OutputDirectory,
                        SampleAudioFileName);
                    double sampleLengthMs = Properties.Settings.Default.SampleVideoLength * 1000.0;
                    List<int> audioStreamIds = new List<int>();
                    if(this.audioCheckedListBox.Items.Count != 0)
                    {
                        AudioTrackItem audioItem;
                        if(this.audioCheckedListBox.CheckedIndices.Count != 0)
                        {
                            audioItem = this.audioCheckedListBox.Items[
                                this.audioCheckedListBox.CheckedIndices[0]] as AudioTrackItem;
                        }
                        else
                        {
                            audioItem = this.audioCheckedListBox.Items[0] as AudioTrackItem;
                        }
                        audioStreamIds.Add(audioItem.StreamId);
                    }
                    TitleSaver saver = new TitleSaver(item.TitleSet, item.TitleIndex, item.Title,
                        angles, audioStreamIds, null, fileName, null, 0.0, sampleLengthMs);
                    this.sampleUpdatesLabel.Visible = true;
                    saver.Run(new Action<string>(this.UpdateSaveSampleStatus));
                    this.sampleUpdatesLabel.Visible = false;
                    Process.Start(Properties.Settings.Default.SampleVideoPlayerPath,
                        "\"" + fileName + "\"");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Exception occurred while creating sample: " + ex.Message);
            }
        }

        void UpdateSaveSampleStatus(string status)
        {
            if(!this.Disposing && !this.IsDisposed && this.IsHandleCreated)
            {
                if(InvokeRequired)
                {
                    BeginInvoke(new Action<string>(UpdateSaveSampleStatus), status);
                }
                else
                {
                    this.sampleUpdatesLabel.Text = status;
                    Application.DoEvents();
                }
            }
        }

        private void audioCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if(this.recursiveAudioCheck)
            {
                return;
            }

            int selectedIndex = this.titleListBox.SelectedIndex;
            if((this.titleListBox.Items.Count != 0) && (selectedIndex != -1))
            {
                DvdTrackItem item = this.titleListBox.SelectedItem as DvdTrackItem;
                DvdTitle title = item.Title;
                item.SelectedAudioStreams.Clear();
                foreach(AudioTrackItem audio in this.audioCheckedListBox.CheckedItems)
                {
                    item.SelectedAudioStreams.Add(audio.StreamId);
                }
                AudioTrackItem changedItem = this.audioCheckedListBox.Items[e.Index] as AudioTrackItem;
                if(e.NewValue == CheckState.Checked)
                {
                    item.SelectedAudioStreams.Add(changedItem.StreamId);
                }
                else
                {
                    item.SelectedAudioStreams.Remove(changedItem.StreamId);
                }
            }
        }

        private void titleListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if(!this.Disposing && !this.IsDisposed && this.IsHandleCreated)
            { 
                this.data.Programs[e.Index].IsSelected = (e.NewValue == CheckState.Checked);
                BeginInvoke(new Action(OnCompleteStatusUpdated));
            }
        }

        void OnCompleteStatusUpdated()
        {
            this.data.IsCurrentStepComplete = this.IsComplete;
        }

        private void anglesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(this.recursiveAngleSelect)
            {
                return;
            }

            int selectedIndex = this.anglesComboBox.SelectedIndex;
            if((this.anglesComboBox.Items.Count != 0) && (selectedIndex != -1) &&
                (this.titleListBox.Items.Count != 0) && (this.titleListBox.SelectedIndex != -1))
            {
                DvdTrackItem item = this.titleListBox.SelectedItem as DvdTrackItem;
                DvdTitle title = item.Title;
                if(title.AngleCount != 0)
                {
                    item.Angle = this.anglesComboBox.SelectedIndex + 1;
                    item.MpegFileCreated = false;
                    item.SubtitleDataCreated = false;
                    item.D2vFileCreated = false;
                }
            }
        }
    }
}
