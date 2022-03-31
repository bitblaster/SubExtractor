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
    public partial class CommentBox : Form
    {
        public CommentBox()
        {
            InitializeComponent();
        }

        public String Comment
        {
            get
            {
                return this.commentTextBox.Text;
            }
            set
            {
                this.commentTextBox.Text = value;
            }
        }
    }
}
