using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DvdNavigatorCrm;
using DvdSubOcr;

namespace DvdSubExtractor
{
    public partial class SubWizard : Form
    {
        ExtractData data = new ExtractData();
        static Type[] stepTypes = new Type[] { typeof(LoadFolderStep),
            typeof(ChooseTracksStep), typeof(RunExtractorStep), typeof(ChooseSubtitlesStep),
            typeof(OcrBlocksStep), typeof(SpellCheckStep), typeof(CreateSubtitleFileStep), typeof(WordSpacingStep) };
        static Type[] subtitleOptionTypes = new Type[] { typeof(CreateSubtitleFileStep) };
        static Type[] subdialogTypes = new Type[] { typeof(WordSpacingStep) };

        const int ChooseSubtitlesStepIndex = 3;

        IWizardItem currentItem;
        int stepIndex;
        int oldIndex = -1;
        string originalTitle;

        public SubWizard()
        {
            InitializeComponent();
            Localize();

            if(Properties.Settings.Default.LargeMode)
            {
                this.Font = SubConstants.LargeFormFont;
                this.helpTextBox.Font = SubConstants.LargeHelpFont;
                this.mainToolStrip.Font = SubConstants.LargeMainToolStripFont;
            }

            object[] fileAttribs = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);
            string fileVersion = "";
            foreach(AssemblyFileVersionAttribute attrib in fileAttribs)
            {
                fileVersion = attrib.Version;
                break;
            }
            this.Text = this.Text + " " + fileVersion;
            this.originalTitle = this.Text;
            this.data.HelpTextUpdated += this.data_HelpTextUpdated;
            this.data.IsCurrentStepCompleteUpdated += this.data_IsCurrentStepCompleteUpdated;
            this.data.IsPreviousStepCompleteUpdated += this.data_IsPreviousStepCompleteUpdated;
            this.data.JumpToStepsUpdated += this.data_JumpToStepsUpdated;
            this.data.JumpTo += this.data_JumpTo;

            if(Properties.Settings.Default.InitialStepIsBrowse)
            {
                this.stepIndex = ChooseSubtitlesStepIndex;
            }
        }

        public SubWizard(string subFile) : this()
        {
            this.stepIndex = ChooseSubtitlesStepIndex;
            this.data.SelectedSubtitleBinaryFile = subFile;
        }

        public SubWizard(string subFile, int streamId) : this(subFile)
        {
            this.data.SelectedSubtitleStreamId = streamId;
        }

        void data_HelpTextUpdated(object sender, EventArgs e)
        {
            if(!this.IsDisposed && !this.Disposing && this.IsHandleCreated)
            {
                this.helpTextBox.Text = this.data.HelpText;
            }
        }

        void data_IsCurrentStepCompleteUpdated(object sender, EventArgs e)
        {
            if(!this.IsDisposed && !this.Disposing && this.IsHandleCreated)
            {
                this.nextButton.Enabled = this.data.IsCurrentStepComplete &&
                    (this.stepIndex < stepTypes.Length - 1);
            }
        }

        void data_IsPreviousStepCompleteUpdated(object sender, EventArgs e)
        {
            if(!this.IsDisposed && !this.Disposing && this.IsHandleCreated)
            {
                this.previousStepButton.Enabled = this.data.IsPreviousStepComplete &&
                    (this.stepIndex > 0);
            }
        }

        void data_JumpToStepsUpdated(object sender, EventArgs e)
        {
            if(!this.IsDisposed && !this.Disposing && this.IsHandleCreated)
            {
                this.openDvdToolStripMenuItem.Enabled = this.data.JumpToSteps.Contains(typeof(LoadFolderStep));
                this.openFileToolStripMenuItem.Enabled = this.data.JumpToSteps.Contains(typeof(ChooseSubtitlesStep));
            }
        }

        void data_JumpTo(object sender, TypeEventArgs e)
        {
            if(!this.IsDisposed && !this.Disposing && this.IsHandleCreated)
            {
                int newIndex = 0;
                foreach(Type type in stepTypes)
                {
                    if(type == e.Type)
                    {
                        this.oldIndex = this.stepIndex;
                        this.stepIndex = newIndex;
                        LoadCurrentStep();
                        return;
                    }
                    newIndex++;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadCurrentStep();
        }

        protected override void OnClosed(EventArgs e)
        {
            ClearCurrentStep();
            base.OnClosed(e);
        }

        void ClearCurrentStep()
        {
            if(this.currentItem != null)
            {
                this.currentItem.Terminate();

                IDisposable disposable = this.currentItem as IDisposable;
                this.currentItem = null;
                if(disposable != null)
                {
                    disposable.Dispose();
                }
            }
            this.helpTextBox.Text = "";
            this.nextButton.Enabled = false;
            this.previousStepButton.Enabled = false;
        }

        void LoadCurrentStep()
        {
            if(this.IsDisposed || this.Disposing)
            {
                return;
            }

            ClearCurrentStep();

            this.currentItem =
                stepTypes[this.stepIndex].GetConstructor(Type.EmptyTypes).Invoke(null) as IWizardItem;
            Control control = this.currentItem as Control;

            if(this.IsDisposed || this.Disposing || (this.currentItem == null) || (control == null))
            {
                return;
            }

            Size sizeDiff = (this.panelItem.Size - control.Size);
            sizeDiff.Width /= 2;
            sizeDiff.Height = 0;
            //sizeDiff.Height /= 2;
            control.Location = this.panelItem.Location; // +sizeDiff;
            control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            control.Size = this.panelItem.Size;
            this.Controls.Add(control);
            this.currentItem.Initialize(this.data);

            if(this.IsDisposed || this.Disposing || (this.currentItem == null))
            {
                return;
            }

            control.Show();

            this.helpTextBox.Select(0, 0);
            this.helpTextBox.ScrollToCaret();
            this.Text = this.originalTitle + " - " + stepNames[this.stepIndex];

            SelectNextControl(this.mainToolStrip, true, true, true, true);
        }

        private void optionsButton_Click(object sender, EventArgs e)
        {
            using(OptionsForm options = new OptionsForm())
            {
                if((this.currentItem != null) && subtitleOptionTypes.Contains(this.currentItem.GetType()))
                {
                    options.ShowSubtitleTab();
                }
                if(this.currentItem is SpellCheckStep)
                {
                    options.ShowLandIWordTab((this.currentItem as SpellCheckStep).OcrMap);
                }
                if((options.ShowDialog(this) == DialogResult.OK) && (this.currentItem != null))
                {
                    this.data.OnOptionsUpdated(this);
                }
            }
        }

        private void previousButton_Click(object sender, EventArgs e)
        {
            if(subdialogTypes.Contains(this.currentItem.GetType()))
            {
                this.stepIndex = this.oldIndex;
                this.oldIndex = -1;
            }
            else
            {
                this.oldIndex = this.stepIndex;
                this.stepIndex--;
            }
            LoadCurrentStep();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            this.oldIndex = this.stepIndex;
            this.stepIndex++;
            LoadCurrentStep();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                // remote desktop error workaround
                e.Graphics.Clear(SystemColors.AppWorkspace);
            }
            catch(Exception)
            {
            }
            //base.OnPaint(e);
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            using(AboutForm about = new AboutForm())
            {
                about.ShowDialog(this);
            }
        }

        private void openDvdFolderButton_Click(object sender, EventArgs e)
        {
        }

        private void openSubFileButton_Click(object sender, EventArgs e)
        {
        }

        private void openDvdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(!this.IsDisposed && !this.Disposing && this.IsHandleCreated)
            {
                this.oldIndex = -1;
                this.stepIndex = stepTypes.ToList().IndexOf(typeof(LoadFolderStep));
                LoadCurrentStep();
            }
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(!this.IsDisposed && !this.Disposing && this.IsHandleCreated)
            {
                this.oldIndex = -1;
                this.stepIndex = stepTypes.ToList().IndexOf(typeof(ChooseSubtitlesStep));
                LoadCurrentStep();
            }
        }
    }
}
