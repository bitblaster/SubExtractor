using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DvdNavigatorCrm;

namespace DvdSubExtractor
{
    public partial class LoadFolderStep : UserControl, IWizardItem
    {
        ExtractData data;

        public LoadFolderStep()
        {
            InitializeComponent();
        }

        public void Initialize(ExtractData data)
        {
            this.data = data;
            if(!string.IsNullOrEmpty(this.data.DvdFolder))
            {
                this.dvdFolderPath.Text = this.data.DvdName;
                this.dvdPathTextBox.Text = this.data.DvdFolder;
                this.reloadButton.Enabled = true;
            }
            else
            {
                this.reloadButton.Enabled = false;
            }
            if(this.data.Programs.Count != 0)
            {
                LoadProgramChains();
            }

            this.data.NewStepInitialize(this.IsComplete, false, this.HelpText, new Type[] { typeof(ChooseSubtitlesStep) });
        }

        public void Terminate()
        {
        }

        bool IsComplete
        {
            get 
            {
                return this.programChainListbox.Items.Count != 0;
            }
        }

        string HelpText
        {
            get 
            {
                return "This is where you choose the DVD you want to work with. " +
                    "This should be a folder on a local or network hard drive which contains " +
                    "a sub-folder named VIDEO_TS from a DVD, or just has the IFO and VOB files " +
                    "found in a VIDEO_TS folder directly within it.\n\nActual optical DVDs almost certainly " + 
                    "won't work, since they are encrypted and unreadable by programs like this.  " + 
                    "You need to find and run a program (such as DVDFab HD Decrypter) that will " + 
                    "backup your DVDs to your hard drive first.\n\n" +
                    "When you've chosen the DVD folder and some programs have appeared in ths " +
                    "listbox, hit Next";
            }
        }

        private void dvdFolderBrowseButton_Click(object sender, EventArgs e)
        {
            this.folderBrowserDialog.SelectedPath = Properties.Settings.Default.DvdFolderPath;
            if(this.folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                if(Directory.Exists(this.folderBrowserDialog.SelectedPath))
                {
                    string selectedPath = this.folderBrowserDialog.SelectedPath;
                    if(!CheckFolderPath(selectedPath))
                    {
                        return;
                    }
                    this.data.LoadDvdPrograms(selectedPath);
                    if(this.data.Programs.Count == 0)
                    {
                        return;
                    }
                    this.dvdFolderPath.Text = this.data.DvdName;
                    this.dvdPathTextBox.Text = selectedPath;
                    this.reloadButton.Enabled = true;
                    Properties.Settings.Default.DvdFolderPath = selectedPath;
                    Properties.Settings.Default.Save();
                    LoadProgramChains();
                    if(Properties.Settings.Default.InitialStepIsBrowse)
                    {
                        Properties.Settings.Default.InitialStepIsBrowse = false;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }

        bool CheckFolderPath(string selectedPath)
        {
            if(!Directory.Exists(selectedPath))
            {
                MessageBox.Show(this, "Path does not exist", "Folder error", MessageBoxButtons.OK);
                return false;
            }

            string root = Path.GetPathRoot(selectedPath);
            if((root == selectedPath) || (Path.Combine(root, "VIDEO_TS") == selectedPath))
            {
                DriveInfo drive = new DriveInfo(root.Substring(0, 1));
                if(drive.DriveType == DriveType.CDRom)
                {
                    if(MessageBox.Show(this,
                        "This DVD probably won't work because it's encrypted, " +
                        "you need to backup your DVD to a hard drive first.  Continue?",
                        "Possible Error", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        void LoadProgramChains()
        {
            this.programChainListbox.Items.Clear();

            IList<DvdTrackItem> tracks = this.data.Programs;
            if(tracks.Count != 0)
            {
                this.programChainListbox.Items.AddRange(tracks.ToArray());
            }
            else
            {
                MessageBox.Show("No DVD Tracks found in directory");
            }
            this.data.IsCurrentStepComplete = this.IsComplete;
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            string selectedPath = Path.GetFullPath(this.dvdPathTextBox.Text);
            if(!CheckFolderPath(selectedPath))
            {
                return;
            }
            this.data.LoadDvdPrograms(selectedPath);
            if(this.data.Programs.Count == 0)
            {
                return;
            }
            this.dvdFolderPath.Text = this.data.DvdName;
            this.reloadButton.Enabled = true;
            Properties.Settings.Default.DvdFolderPath = selectedPath;
            Properties.Settings.Default.Save();
            LoadProgramChains();
        }

        private void dvdPathTextBox_TextChanged(object sender, EventArgs e)
        {
            this.reloadButton.Enabled =  (this.dvdPathTextBox.Text.Length != 0);
        }
    }
}
