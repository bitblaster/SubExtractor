using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DvdNavigatorCrm
{
    class MainProgram
    {
		[STAThread]
		public static int Main(string[] args)
        {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new IfoViewer());

			/*
			IfoViewer viewer = new IfoViewer();
            if(args.Length != 1)
            {
                throw new ArgumentException();
            }

            string path = args[0];
            using(VideoTitleSet titleSet = new VideoTitleSet(path))
            {
				if(!titleSet.IsValidTitleSet)
				{
					Console.WriteLine("Not a VTS IFO file!!!");
				}
				else
				{
					titleSet.ParsePTT();
					titleSet.ParsePGCI();
					Console.Write(titleSet.ToString());
					//Console.ReadKey(true);
				}
            }*/

            return 0;
        }
    }
}
