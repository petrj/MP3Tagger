using System;
using System.IO;
using Gtk;

namespace MP3Tagger
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			if (args.Length>0)
			{
				foreach (var arg in args)
				{
					if (Directory.Exists(arg))
					{
						win.AddFolder(args[0],false);
					}
				}
			}
			win.Show ();
			Application.Run ();
		}
	}
}
