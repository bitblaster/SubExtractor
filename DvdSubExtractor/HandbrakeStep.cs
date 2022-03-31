using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DvdSubExtractor
{
    public partial class HandbrakeStep : UserControl, IWizardItem
    {
        ExtractData data;

        public HandbrakeStep()
        {
            InitializeComponent();
        }

        public void Initialize(ExtractData data)
        {
            this.data = data;
        }

        public void Terminate()
        {
        }

        public void OptionsUpdated()
        {
        }

        public bool IsComplete { get; private set; }

        public string HelpText
        {
            get
            {
                return "";
            }
        }

        public event EventHandler StatusUpdated;
        public event EventHandler<TypeEventArgs> JumpTo;

        private void OnStatusUpdated()
        {
            EventHandler tempHandler = this.StatusUpdated;
            if(tempHandler != null)
            {
                tempHandler(this, EventArgs.Empty);
            }
        }

        private void OnJumpTo(Type type)
        {
            EventHandler<TypeEventArgs> tempHandler = this.JumpTo;
            if(tempHandler != null)
            {
                tempHandler(this, new TypeEventArgs(type));
            }
        }

        public IEnumerable<Type> JumpToStepsAllowed
        {
            get
            {
                yield return typeof(LoadFolderStep);
                yield return typeof(ChooseSubtitlesStep);
            }
        }

        private void optionsButton_Click(object sender, EventArgs e)
        {
            using(HandbrakeForm options = new HandbrakeForm())
            {
                options.ShowDialog(this);
            }
        }
    }
}
