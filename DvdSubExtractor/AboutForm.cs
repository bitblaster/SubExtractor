using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DvdSubExtractor
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();

            this.aboutTextBox.Text = "DVD Subtitle Extractor " + Application.ProductVersion + "\n" +
                "Copyright © 2009-2012 Christopher R Meadowcroft";
        }
    }
}
