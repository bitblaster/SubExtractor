using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DvdSubExtractor
{
    public partial class SubWizard
    {
        static string[] stepNames = new string[] { "Choose DVD Folder",
            "Choose Tracks", "Re-Encode Tracks", "Load Subtitles File", 
            "OCR Subtitles", "Spelling and Spacing", "Create Subtitle File", "Word Spacing Adjustment" };

        void Localize()
        {
            this.previousStepButton.Text = "Previous Step";
            this.nextButton.Text = "Next Step";
            this.openFileToolStripMenuItem.Text = "Open Subtitle File";
            this.openDvdToolStripMenuItem.Text = "Open DVD Folder";
            this.optionsButton.Text = "Options...";
            this.aboutButton.Text = "About...";
            this.Text = "Subtitle Extractor";
        }
    }
}
