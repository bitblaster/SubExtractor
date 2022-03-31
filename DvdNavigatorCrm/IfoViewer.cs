using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DvdNavigatorCrm;

namespace DvdNavigatorCrm
{
	public partial class IfoViewer : Form
	{
		public IfoViewer()
		{
			InitializeComponent();
		}

		private void browseButton_Click(object sender, EventArgs e)
		{
			using(OpenFileDialog fd = new OpenFileDialog())
			{
				fd.CheckFileExists = true;
				fd.DefaultExt = "ifo";
				fd.Filter = "Ifo files (*.ifo)|*.ifo";
				if(fd.ShowDialog() == DialogResult.OK)
				{
					DvdTitleSet vts = new DvdTitleSet(fd.FileName);
					if(!vts.IsValidTitleSet)
					{
						VideoManagerTitleSet vmts = new VideoManagerTitleSet(fd.FileName);
						if(vmts.IsValidTitleSet)
						{
							vmts.Parse();
							this.ifoDumpEdit.Text = vmts.ToString();
							this.ifoDumpEdit.Select(0, 0);
							this.ifoDumpEdit.ScrollToCaret();
						}
						else
						{
							this.ifoDumpEdit.Text = "Invalid File";
						}
					}
					else
					{
						vts.Parse();
						this.ifoDumpEdit.Text = vts.ToString();
						this.ifoDumpEdit.Select(0, 0);
						this.ifoDumpEdit.ScrollToCaret();
					}
				}
			}
		}

        void ValidateIfosInDirectoryRecursively(string dirName, StringBuilder sb)
        {
            foreach (string fileName in Directory.GetFiles(dirName, "*.ifo"))
            {
                try
                {
					DvdTitleSet vts = new DvdTitleSet(fileName);
                    if (!vts.IsValidTitleSet)
                    {
                        VideoManagerTitleSet vmts = new VideoManagerTitleSet(fileName);
                        if (!vmts.IsValidTitleSet)
                        {
                            sb.AppendFormat("{0} NOT VALID VTS or VTMS\n", fileName);
                        }
                        else
                        {
                            vmts.Parse();
                            sb.AppendFormat("{0} valid vmts\n", fileName);
                        }
                    }
                    else
                    {
                        vts.Parse();
                        sb.AppendFormat("{0} valid vts\n", fileName);
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendFormat("{0} EXCEPTION\n", fileName);
                }
            }
            foreach (string childDirectory in Directory.GetDirectories(dirName))
            {
                ValidateIfosInDirectoryRecursively(childDirectory, sb);
            }
        }

        private void directoryButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fd = new FolderBrowserDialog())
            {
                fd.ShowNewFolderButton = false;
                if (fd.ShowDialog(this) == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    ValidateIfosInDirectoryRecursively(fd.SelectedPath, sb);
                    this.ifoDumpEdit.Text = sb.ToString();
                }
            }
        }
	}
}