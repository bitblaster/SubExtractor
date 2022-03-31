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
    public partial class RunExtractorStep : UserControl, IWizardItem
    {
        ExtractData data;
        TitleSaver titleSaver;
        bool runCanceled;
        ITaskbarList4 taskBarIntf;

        public RunExtractorStep()
        {
            InitializeComponent();
        }

        public void Initialize(ExtractData data)
        {
            this.data = data;
            this.dvdLabel.Text = Path.GetFileName(this.data.DvdFolder);
            this.createSubtitleDataCheckBox.Checked = true;
            this.createMpegFileCheckBox.Checked = true;
            this.createD2vCheckBox.Checked = true;
            this.messageLabel.Text = "";

            this.OptionsUpdated(this, EventArgs.Empty);
            this.data.OptionsUpdated += this.OptionsUpdated;

            if(NativeMethods.SupportsTaskProgress)
            {
                CTaskbarList taskBar = new CTaskbarList();
                this.taskBarIntf = (ITaskbarList4)taskBar;
                this.taskBarIntf.HrInit();
                this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.NoProgress);
            }

            this.data.NewStepInitialize(false, true, this.HelpText, new Type[] { typeof(LoadFolderStep) });
            LoadProgramChains();

            bool hasDgIndex = OptionsForm.DoesDGIndexExist;
        }

        public void Terminate()
        {
            data.OptionsUpdated -= this.OptionsUpdated;
            if(this.taskBarIntf != null)
            {
                this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.NoProgress);
            }
            cancelSaveButton_Click(this, EventArgs.Empty);
        }

        void OptionsUpdated(object sender, EventArgs e)
        {
            if(File.Exists(Properties.Settings.Default.DgIndexPath))
            {
                if(!this.createD2vCheckBox.Enabled)
                {
                    this.createD2vCheckBox.Enabled = true;
                    this.createD2vCheckBox.Checked = true;
                    titleListBox_SelectedIndexChanged(this, EventArgs.Empty);
                }
            }
            else
            {
                this.createD2vCheckBox.Checked = false;
                this.createD2vCheckBox.Enabled = false;
                this.d2vFileLabel.Text = "DGIndex.exe not configured in Options";
            }
        }

        string HelpText
        {
            get
            {
                return "In this step, we will create 4 new files for each Program you have chosen " +
                    "from the DVD:  an mpg movie file with associated chapter txt file, " +
                    "which contains the video and audio tracks selected " +
                    "and is playable in various media players or can be used as input to re-encoding " + 
                    "with another program, the subtitle data file which is needed in the next step " + 
                    "of the OCR process, and a d2v DGIndex file which will allow proper aligning of the " +
                    "subtitle timestamps.\n\nPlease go through the Programs either all at once or one " +
                    "at a time and Encode them to create these 4 files for each in your Output " +
                    "directory (set in Options Dialog).";
            }
        }

        class LoadTrackItem
        {
            ExtractData data;

            public LoadTrackItem(ExtractData data, DvdTrackItem item)
            {
                this.data = data;
                this.Item = item;
            }

            public DvdTrackItem Item { get; private set; }

            public bool IsPartiallyComplete
            {
                get
                {
                    if(this.Item.MpegFileCreated || this.Item.D2vFileCreated || this.Item.SubtitleDataCreated)
                    {
                        return true;
                    }

                    string storageFileName = Path.Combine(
                        Properties.Settings.Default.OutputDirectory,
                        this.data.ComputeSubtitleDataFileName(this.Item));
                    if(File.Exists(storageFileName))
                    {
                        return true;
                    }
                    string mpegFileName = Path.Combine(
                        Properties.Settings.Default.OutputDirectory,
                        this.data.ComputeMpegFileName(this.Item));
                    if(File.Exists(mpegFileName))
                    {
                        return true;
                    }
                    string d2vFileName = Path.Combine(
                        Properties.Settings.Default.OutputDirectory,
                        this.data.ComputeD2vFileName(this.Item));
                    if(File.Exists(d2vFileName))
                    {
                        return true;
                    }

                    return false;
                }
            }

            public override string ToString()
            {
                int minutes = (int)this.Item.Title.PlaybackTime / 60;
                int seconds = Convert.ToInt32(this.Item.Title.PlaybackTime) - minutes * 60;
                StringBuilder text = new StringBuilder();
                text.AppendFormat("Program {0} Duration {1}:{2:d2}",
                    this.Item.ProgramNumber, minutes, seconds);
                if(this.Item.Title.AngleCount != 0)
                {
                    text.AppendFormat(" Angle {0}", this.Item.Angle);
                }
                if(this.Item.SelectedAudioStreams.Count != 0)
                {
                    text.Append(" Audio (");
                    StringBuilder audioText = new StringBuilder();
                    foreach(int streamId in this.Item.SelectedAudioStreams)
                    {
                        if(audioText.Length != 0)
                        {
                            audioText.Append(", ");
                        }
                        audioText.AppendFormat("{0:x} {1}", streamId,
                            this.Item.Title.GetAudioStream(streamId).Language);
                    }
                    text.Append(audioText);
                    text.Append(")");
                }

                if(this.Item.SubtitleDataCreated)
                {
                    text.Append(" (Subtitle File Created)");
                }
                else
                {
                    string storageFileName = Path.Combine(
                         Properties.Settings.Default.OutputDirectory,
                         this.data.ComputeSubtitleDataFileName(this.Item));
                    if(File.Exists(storageFileName))
                    {
                        text.Append(" (Old Subtitle Data File)");
                    }
                    else
                    {
                        text.Append(" (No Subtitle Data File)");
                    }
                }
                if(this.Item.MpegFileCreated)
                {
                    text.Append(" (Mpeg File Created)");
                }
                else
                {
                    string mpegFileName = Path.Combine(
                        Properties.Settings.Default.OutputDirectory,
                        this.data.ComputeMpegFileName(this.Item));
                    if(File.Exists(mpegFileName))
                    {
                        text.Append(" (Old Mpeg File)");
                    }
                    else
                    {
                        text.Append(" (No Mpeg File)");
                    }
                }
                if(this.Item.D2vFileCreated)
                {
                    text.Append(" (D2v File Created)");
                }
                else
                {
                    string d2vFileName = Path.Combine(
                        Properties.Settings.Default.OutputDirectory,
                        this.data.ComputeD2vFileName(this.Item));
                    if(File.Exists(d2vFileName))
                    {
                        text.Append(" (Old D2v File)");
                    }
                    else
                    {
                        text.Append(" (No D2v File)");
                    }
                }
                return text.ToString();
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
                    if(item.IsSelected)
                    {
                        LoadTrackItem trackItem = new LoadTrackItem(this.data, item);
                        if(!this.data.IsCurrentStepComplete && trackItem.IsPartiallyComplete)
                        {
                            this.data.IsCurrentStepComplete = true;
                        }
                        this.titleListBox.Items.Add(trackItem);
                    }
                }
                this.titleListBox.SelectedIndex = 0;
            }
        }

        private void titleListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if((this.titleListBox.Items.Count != 0) && (this.titleListBox.SelectedIndex != -1))
            {
                LoadTrackItem item = this.titleListBox.SelectedItem as LoadTrackItem;
                this.subtitleDataFileLabel.Text = this.data.ComputeSubtitleDataFileName(item.Item);
                this.mpegFileLabel.Text = this.data.ComputeMpegFileName(item.Item);
                if(this.createD2vCheckBox.Enabled)
                {
                    this.d2vFileLabel.Text = this.data.ComputeD2vFileName(item.Item);
                }
                this.encodeSelectedButton.Enabled = true;
            }
            else
            {
                this.encodeSelectedButton.Enabled = false;
            }
        }

        private void encodeSelectedButton_Click(object sender, EventArgs e)
        {
            if((this.titleListBox.Items.Count != 0) && (this.titleListBox.SelectedIndex != -1))
            {
                int selected = this.titleListBox.SelectedIndex;
                EncodePrograms(selected, 1);
                this.titleListBox.SelectedIndex = selected;
            }
        }

        private void encodeAllButton_Click(object sender, EventArgs e)
        {
            if(this.titleListBox.Items.Count != 0)
            {
                EncodePrograms(0, this.titleListBox.Items.Count);
                this.titleListBox.SelectedIndex = 0;
            }
        }

        void EncodePrograms(int startIndex, int count)
        {
            if(!OptionsForm.DoesOutputPathExist)
            {
                return;
            }

            if(this.taskBarIntf != null)
            {
                this.taskBarIntf.SetProgressValue(this.TopLevelControl.Handle, 0, 100);
            }

            this.messageLabel.Text = "";
            bool firstEncode = true;

            try
            {
                int programsEncoded = 0;
                List<LoadTrackItem> items = new List<LoadTrackItem>(
                    this.titleListBox.Items.Cast<LoadTrackItem>().Skip(startIndex).Take(count));
                int index = startIndex - 1;
                foreach(LoadTrackItem item in items)
                {
                    index++;

                    if((item.Item.MpegFileCreated || !this.createMpegFileCheckBox.Checked) &&
                        (item.Item.SubtitleDataCreated || !this.createSubtitleDataCheckBox.Checked))
                    {
                        continue;
                    }

                    this.titleListBox.SelectedIndex = index;
                    string mpegFileName = null;
                    string chapterFileName = null;
                    if(this.createMpegFileCheckBox.Checked)
                    {
                        mpegFileName = Path.Combine(
                            Properties.Settings.Default.OutputDirectory,
                            this.data.ComputeMpegFileName(item.Item));
                        chapterFileName = Path.Combine(
                            Properties.Settings.Default.OutputDirectory,
                            Path.GetFileNameWithoutExtension(mpegFileName) + ".txt");
                    }
                    string storageFileName = null;
                    if(this.createSubtitleDataCheckBox.Checked)
                    {
                        storageFileName = Path.Combine(
                             Properties.Settings.Default.OutputDirectory,
                             this.data.ComputeSubtitleDataFileName(item.Item));
                    }

                    int[] angles;
                    if(item.Item.Angle != 0)
                    {
                        angles = new int[] { 0, item.Item.Angle };
                    }
                    else
                    {
                        angles = new int[] { 0 };
                    }

                    List<int> audioStreamIds = new List<int>(item.Item.SelectedAudioStreams);

                    this.titleSaver = new TitleSaver(item.Item.TitleSet, item.Item.TitleIndex,
                        item.Item.Title, angles, audioStreamIds, storageFileName, mpegFileName,
                        chapterFileName, null, null);
                    Thread t = new Thread(this.RunSaver);
                    RunBegin();
                    t.Start();
                    while(!t.Join(100))
                    {
                        Application.DoEvents();
                        if(this.Disposing || this.IsDisposed || !this.IsHandleCreated)
                        {
                            return;
                        }
                    }

                    if(this.Disposing || this.IsDisposed || !this.IsHandleCreated ||
                        this.runCanceled)
                    {
                        return;
                    }

                    programsEncoded++;
                    if((mpegFileName != null) && File.Exists(mpegFileName))
                    {
                        item.Item.MpegFileCreated = true;
                    }
                    if((storageFileName != null) && File.Exists(storageFileName))
                    {
                        item.Item.SubtitleDataCreated = true;
                        if(firstEncode)
                        {
                            this.data.SelectedSubtitleBinaryFile = storageFileName;
                            this.data.SelectedSubtitleStreamId = -1;
                            firstEncode = false;
                        }
                    }

                    this.titleListBox.Items.RemoveAt(index);
                    this.titleListBox.Items.Insert(index, item);

                    if(!this.data.IsCurrentStepComplete &&
                        (item.Item.MpegFileCreated || item.Item.D2vFileCreated || item.Item.SubtitleDataCreated))
                    {
                        this.data.IsCurrentStepComplete = true;
                    }
                }

                int programsIndexed = 0;
                index = startIndex - 1;
                foreach(LoadTrackItem item in items)
                {
                    index++;

                    if(item.Item.D2vFileCreated || !this.createD2vCheckBox.Checked)
                    {
                        continue;
                    }
                    string mpegFileName = Path.Combine(
                        Properties.Settings.Default.OutputDirectory,
                        this.data.ComputeMpegFileName(item.Item));
                    if(!File.Exists(mpegFileName))
                    {
                        continue;
                    }

                    this.titleListBox.SelectedIndex = index;

                    string d2vFileName = Path.Combine(Properties.Settings.Default.OutputDirectory,
                        this.data.ComputeD2vFileName(item.Item));
                    string d2vFilePath = Path.Combine(Properties.Settings.Default.OutputDirectory,
                        Path.GetFileNameWithoutExtension(d2vFileName));
                    List<int> audioStreamIds = new List<int>(item.Item.SelectedAudioStreams);
                    string audioTracks = string.Join(",", audioStreamIds.ConvertAll(a => a.ToString("X")).ToArray());
                    string args = string.Format("-SD=< -AIF=<{0}< -OF=<{1}< -FO=0 -exit -hide -OM=1 -TN={2}",
                        mpegFileName, d2vFilePath, audioTracks);

                    RunBegin();
                    using(Process process = new Process())
                    {
                        process.StartInfo.FileName = Properties.Settings.Default.DgIndexPath;

                        process.StartInfo.Arguments = args;

                        this.progressBar1.Value = 0;
                        this.progressBar1.Maximum = 100;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.OutputDataReceived += this.process_OutputDataReceived;
                        process.ErrorDataReceived += this.process_ErrorDataReceived;
                        process.Start();

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        while(!process.WaitForExit(100))
                        {
                            Application.DoEvents();
                            if(this.Disposing || this.IsDisposed || !this.IsHandleCreated)
                            {
                                return;
                            }
                        }

                        if(this.Disposing || this.IsDisposed || !this.IsHandleCreated ||
                            this.runCanceled)
                        {
                            return;
                        }

                        this.progressBar1.Value = 0;
                    }

                    programsIndexed++;
                    if((d2vFileName != null) && File.Exists(d2vFileName))
                    {
                        item.Item.D2vFileCreated = true;
                    }

                    this.titleListBox.Items.RemoveAt(index);
                    this.titleListBox.Items.Insert(index, item);
                }

                if((programsEncoded == 0) && (programsIndexed == 0))
                {
                    MessageBox.Show("Nothing to do - program(s) were already encoded and indexed");
                }
                else
                {
                    this.messageLabel.Text = "Encoding complete!";
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Exception occurred while creating output: " + ex.Message);
            }
            finally
            {
                if(this.runCanceled)
                {
                    this.messageLabel.Text = "Encoding canceled";
                }
                RunComplete();
            }
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if(!this.Disposing && !this.IsDisposed && this.IsHandleCreated)
            {
                if(InvokeRequired)
                {
                    BeginInvoke(new Action<object, DataReceivedEventArgs>(process_OutputDataReceived), null, e);
                }
                else
                {
                    if(!String.IsNullOrEmpty(e.Data))
                    {
                        this.saveUpdatesLabel.Text = "DGIndex " + e.Data;
                        int value;
                        if(Int32.TryParse(e.Data, out value))
                        {
                            this.progressBar1.Value = value;
                            this.saveUpdatesLabel.Text += "%";
                            if(this.taskBarIntf != null)
                            {
                                this.taskBarIntf.SetProgressValue(this.TopLevelControl.Handle, (uint)value, 100);
                            }
                        }
                    }
                }
            }
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if(!this.Disposing && !this.IsDisposed && this.IsHandleCreated)
            {
                if(InvokeRequired)
                {
                    BeginInvoke(new Action<object, DataReceivedEventArgs>(process_ErrorDataReceived), null, e);
                }
                else
                {
                    if(!String.IsNullOrEmpty(e.Data))
                    {
                        if(this.taskBarIntf != null)
                        {
                            this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.Error);
                        }
                        this.saveUpdatesLabel.Text = "DGIndex Error: " + e.Data;
                    }
                }
            }
        }

        void RunSaver()
        {
            try
            {
                this.runCanceled = false;
                this.titleSaver.Run(this.UpdateSaveStatus);
            }
            catch(IOException ex)
            {
                MessageBox.Show("IOException: " + ex.Message);
            }
            finally
            {
                this.titleSaver = null;
            }
        }


        void UpdateSaveStatus(string status)
        {
            if(!this.Disposing && !this.IsDisposed && this.IsHandleCreated)
            {
                if(InvokeRequired)
                {
                    BeginInvoke(new Action<string>(UpdateSaveStatus), status);
                }
                else
                {
                    this.saveUpdatesLabel.Text = status;
                    if(this.titleSaver != null)
                    {
                        int newMax = Convert.ToInt32(this.titleSaver.TotalLength / (1 << 20));
                        if(newMax != this.progressBar1.Maximum)
                        {
                            this.progressBar1.Maximum = newMax;
                        }
                        int totalRead = Convert.ToInt32(this.titleSaver.TotalRead / (1 << 20));
                        this.progressBar1.Value = totalRead;
                        if(this.taskBarIntf != null)
                        {
                            this.taskBarIntf.SetProgressValue(this.TopLevelControl.Handle, (ulong)totalRead, (ulong)newMax);
                        }
                    }
                }
            }
        }

        void RunBegin()
        {
            this.createSubtitleDataCheckBox.Enabled = false;
            this.createMpegFileCheckBox.Enabled = false;
            this.createD2vCheckBox.Enabled = false;
            this.titleListBox.Enabled = false;
            this.encodeAllButton.Enabled = false;
            this.encodeSelectedButton.Enabled = false;
            this.cancelSaveButton.Enabled = true;
            this.data.IsCurrentStepComplete = false;
        }

        void RunComplete()
        {
            if(!this.Disposing && !this.IsDisposed && this.IsHandleCreated)
            {
                this.createSubtitleDataCheckBox.Enabled = true;
                this.createMpegFileCheckBox.Enabled = true;
                this.createD2vCheckBox.Enabled = true;
                this.titleListBox.Enabled = true;
                this.encodeAllButton.Enabled = true;
                this.encodeSelectedButton.Enabled = true;
                this.cancelSaveButton.Enabled = false;
                this.saveUpdatesLabel.Text = "";
                this.progressBar1.Value = 0;
                if(this.taskBarIntf != null)
                {
                    this.taskBarIntf.SetProgressState(this.TopLevelControl.Handle, TaskbarProgressBarStatus.Indeterminate);
                }
                this.data.IsCurrentStepComplete = true;
            }
        }

        private void cancelSaveButton_Click(object sender, EventArgs e)
        {
            TitleSaver saver = this.titleSaver;
            if(saver != null)
            {
                saver.StopRun();
                this.runCanceled = true;
            }
        }
    }
}
