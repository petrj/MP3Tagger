using System;
using System.IO;
using System.Collections.Generic;
using Gtk;
using Logger;

namespace MP3Tagger
{
	public class SongList : List<Song> 
	{
		public void AddFilesFromFolder(string dir, bool recursive, MP3Tagger.ProgressEventHandler progress)
		{
			Logger.Logger.WriteToLog(String.Format("Adding folder {0}",dir));

			var files = Directory.GetFiles(dir,"*.*",recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			var i=0;
			foreach (var f in files)
			{
				var percents = Convert.ToDecimal(i)/Convert.ToDecimal(files.Length/100.00);
				progress(this, new MP3Tagger.ProgressEventArgs(Convert.ToDouble(percents)));

				if (Path.GetExtension(f).ToLower() == ".mp3")
				{
					Logger.Logger.WriteToLog(String.Format("Adding file {0}",f));
					var mp3 = new Song();
					mp3.OpenFile(f);
					this.Add(mp3);

					mp3.ID3v1.WriteToLog();
					mp3.ID3v2.WriteToLog();


					// detailed frame logging
					//foreach (TAG2Frame frame in mp3.ID3v2.Frames) { frame.WriteToLog(); }
				}

				i++;
			}

			progress(this, new MP3Tagger.ProgressEventArgs(100));
		}
	}
}

