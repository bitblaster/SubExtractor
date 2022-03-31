using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DvdNavigatorCrm;
using DvdSubOcr;

namespace DvdSubExtractor
{
    public class ExtractData
    {
        const float PlaybackTimeDifferential = 0.8f;

        List<DvdTrackItem> programs = new List<DvdTrackItem>();
        bool isCurrentStepComplete;
        bool isPreviousStepComplete;
        string helpText = "";
        IEnumerable<Type> jumpToSteps = new List<Type>();

        public ExtractData()
        {
        }

        public IList<DvdTrackItem> Programs { get { return this.programs; } }
        public string DvdFolder { get; private set; }
        public string DvdName { get; private set; }
        public IList<string> SelectedSubtitleFiles { get; set; }

        public string SelectedSubtitleBinaryFile { get; set; }
        public int SelectedSubtitleStreamId { get; set; }
        public string SelectedSubtitleDescription { get; set; }
        public OcrWorkingData WorkingData { get; set; }

        public bool IsHighDef
        {
            get
            {
                if(!String.IsNullOrEmpty(this.SelectedSubtitleBinaryFile))
                {
                    string binaryFileExt = Path.GetExtension(this.SelectedSubtitleBinaryFile).ToLowerInvariant();
                    if(binaryFileExt == ".sup")
                    {
                        return true;
                    }
                }
                if((this.WorkingData != null) && (this.WorkingData.VideoAttributes.HorizontalResolution > 1400))
                {
                    return true;
                }
                return false;
            }
        }

        public event EventHandler OptionsUpdated;

        public void OnOptionsUpdated(object sender)
        {
            EventHandler tempHandler = OptionsUpdated;
            if(tempHandler != null)
            {
                tempHandler(sender, EventArgs.Empty);
            }
        }

        public void NewStepInitialize(bool currentComplete, bool previousComplete, string stepHelpText, IEnumerable<Type> jumpableSteps)
        {
            this.IsCurrentStepComplete = currentComplete;
            this.IsPreviousStepComplete = previousComplete;
            this.HelpText = stepHelpText;
            this.JumpToSteps = jumpableSteps;
        }

        public event EventHandler IsCurrentStepCompleteUpdated;

        public bool IsCurrentStepComplete 
        {
            get { return this.isCurrentStepComplete; }
            set
            {
                this.isCurrentStepComplete = value;
                EventHandler tempHandler = IsCurrentStepCompleteUpdated;
                if(tempHandler != null)
                {
                    tempHandler(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler IsPreviousStepCompleteUpdated;

        public bool IsPreviousStepComplete
        {
            get { return this.isPreviousStepComplete; }
            set
            {
                this.isPreviousStepComplete = value;
                EventHandler tempHandler = IsPreviousStepCompleteUpdated;
                if(tempHandler != null)
                {
                    tempHandler(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler HelpTextUpdated;

        public string HelpText 
        {
            get { return this.helpText; }
            set
            {
                this.helpText = value;
                EventHandler tempHandler = HelpTextUpdated;
                if(tempHandler != null)
                {
                    tempHandler(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler JumpToStepsUpdated;

        public IEnumerable<Type> JumpToSteps
        {
            get { return this.jumpToSteps; }
            set
            {
                this.jumpToSteps = value;
                EventHandler tempHandler = JumpToStepsUpdated;
                if(tempHandler != null)
                {
                    tempHandler(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler<TypeEventArgs> JumpTo;

        public void OnJumpTo(object sender, Type typeOfWizard)
        {
            EventHandler<TypeEventArgs> tempHandler = this.JumpTo;
            if(tempHandler != null)
            {
                tempHandler(sender, new TypeEventArgs(typeOfWizard));
            }
        }

        public void LoadDvdPrograms(string dvdFolder)
        {
            this.DvdFolder = Path.GetFullPath(dvdFolder);
            if(dvdFolder.Length <= 3)
            {
                DriveInfo drive = new DriveInfo(dvdFolder.Substring(0, 1));
                this.DvdName = drive.VolumeLabel;
            }
            else
            {
                this.DvdName = Path.GetFileName(this.DvdFolder);
            }
            this.programs.Clear();

            string dvdPath = this.DvdFolder;
            if(!Directory.Exists(dvdPath))
            {
                return;
            }

            try
            {
                string[] trackIfos = Directory.GetFiles(dvdPath, "*_0.ifo");
                if((trackIfos.Length == 0) && Directory.Exists(Path.Combine(dvdPath, "VIDEO_TS")))
                {
                    dvdPath = Path.Combine(dvdPath, "VIDEO_TS");
                    trackIfos = Directory.GetFiles(dvdPath, "*_0.ifo");
                }

                foreach(string ifoPath in trackIfos)
                {
                    DvdTitleSet titleSet = new DvdTitleSet(ifoPath);
                    if(titleSet.IsValidTitleSet)
                    {
                        titleSet.Parse();

                        for(int titleIndex = 0; titleIndex < titleSet.Titles.Count; titleIndex++)
                        {
                            DvdTitle title = titleSet.Titles[titleIndex];
                            if(title.PlaybackTime >= Properties.Settings.Default.MinimumDvdTrackLength)
                            {
                                this.programs.Add(new DvdTrackItem(titleSet, titleIndex));
                            }
                        }
                    }
                }
            }
            catch(IOException ex)
            {
                MessageBox.Show("IOException: " + ex.Message);
            }
            finally
            {
                if(this.programs.Count != 0)
                {
                    this.programs.Sort();
                    this.programs.Reverse();
                    // sort groups of tracks that are within 20% of each other's playback time
                    // by track order instead of time
                    for(int index = 0; index < programs.Count - 1; index++)
                    {
                        int endNameSort = index;
                        while((endNameSort + 1 < programs.Count) &&
                            (this.programs[index].PlaybackTime * PlaybackTimeDifferential <
                                this.programs[endNameSort + 1].PlaybackTime))
                        {
                            endNameSort++;
                        }
                        if(endNameSort != index)
                        {
                            this.programs.Sort(index, endNameSort - index + 1, new DvdTrackItem.SortByName());
                            index = endNameSort;
                        }
                    }
                    for(int index = 0; index < programs.Count; index++)
                    {
                        this.programs[index].ProgramNumber = index + 1;
                    }
                }
            }
        }

        public string ComputeMpegFileName(DvdTrackItem item)
        {
            if(item.Angle == 0)
            {
                return string.Format("{0} Track {1}.mpg",
                    this.DvdName, item.ProgramNumber);
            }
            else
            {
                return string.Format("{0} Track {1} Angle {2}.mpg",
                    this.DvdName, item.ProgramNumber, item.Angle);
            }
        }

        public string ComputeSubtitleDataFileName(DvdTrackItem item)
        {
            if(item.Angle == 0)
            {
                return string.Format("{0} Track {1}.bin",
                    this.DvdName, item.ProgramNumber);
            }
            else
            {
                return string.Format("{0} Track {1} Angle {2}.bin",
                    this.DvdName, item.ProgramNumber, item.Angle);
            }
        }

        public string ComputeD2vFileName(DvdTrackItem item)
        {
            if(item.Angle == 0)
            {
                return string.Format("{0} Track {1}.d2v",
                    this.DvdName, item.ProgramNumber);
            }
            else
            {
                return string.Format("{0} Track {1} Angle {2}.d2v",
                    this.DvdName, item.ProgramNumber, item.Angle);
            }
        }
    }
}
