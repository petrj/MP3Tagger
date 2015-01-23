using System;
using System.IO;
using System.Collections.Generic;
using Gtk;
using Logger;

namespace MP3Tagger
{
	public class SongList : List<Song> 
	{
		public enum SortColumnEnum
		{
			Unsorted = 0,
			FileName = 1,

			Title = 2,
			Artist = 3,
			Album = 4,
			Year = 5,
			Genre = 6,
			Comment = 7,
			Track = 8,

			Changed = 9
		}

		private SortColumnEnum _sortColumn = SortColumnEnum.Unsorted;
		public bool _asc = true;

		#region properties

		public  List<Song> ChangedSongs
		{
			get
			{
				var c=new List<Song>();
				foreach (var song in this) 
				{
					if (song.Changed) c.Add(song);
				}

				return c;
			}
		}	

		public bool Asc 
		{
			get { return _asc; }
			set {_asc = value; }
		}

		public SortColumnEnum SortColumn 
		{
			get 
			{
				return _sortColumn;
			}
			set
			{
				_sortColumn = value;
			}
		}

		#endregion

		#region methods

		public void SortBy(SortColumnEnum col,bool id3v1)
		{
			if (col == SortColumn)
			{
				Asc = !Asc;
			} else
			{
				SortColumn = col;
				Asc = true;
			}

			Song tmp;
			bool swap;
			TAGBase tagA;
			TAGBase tagB;
			object a,b;
			Type t;

			for (var i = 0; i<Count-1;i++)
			{
				for (var j = i; j<Count;j++)
				{
					swap = false;

					tagA = id3v1 ? this[i].ID3v1 : this[i].ID3v2 as TAGBase;
					tagB = id3v1 ? this[j].ID3v1 : this[j].ID3v2 as TAGBase;

					switch (col)
					{
						case SortColumnEnum.FileName:
							a = Path.GetFileName(this[i].FileName);
							b = Path.GetFileName(this[j].FileName);
							t = typeof(string);
							break;

						case SortColumnEnum.Title: 
							a = tagA.Title;
							b = tagB.Title;
							t = typeof(string);
							break;
						case SortColumnEnum.Artist: 
							a = tagA.Artist;
							b = tagB.Artist;
							t = typeof(string);
							break;
						case SortColumnEnum.Album: 
							a = tagA.Album;
							b = tagB.Album;
							t = typeof(string);
							break;

						case SortColumnEnum.Year: 
							a = tagA.Year;
							b = tagB.Year;
							t = typeof(int);
							break;
						case SortColumnEnum.Genre: 
							a = tagA.Genre;
							b = tagB.Genre;
							t = typeof(int);
							break;
						case SortColumnEnum.Comment: 
							a = tagA.Comment;
							b = tagB.Comment;
							t = typeof(string);
							break;

						case SortColumnEnum.Track: 
							a = tagA.TrackNumber;
							b = tagB.TrackNumber;
							t = typeof(int);
							break;

						case SortColumnEnum.Changed:
							a = tagA.Changed;
							b = tagB.Changed;
							t = typeof(int);
							break;

						default:
							a = b = null; 
							t = null;
							break;
					}


					if (t == typeof(string))
					{
						if (Convert.ToString(a).CompareTo(Convert.ToString(b))>0 && Asc) swap = true;
						if (Convert.ToString(a).CompareTo(Convert.ToString(b))<0 && !Asc) swap = true;
					}

					if (t == typeof(int))
					{
						int ia,ib;
						if (  (int.TryParse( Convert.ToString(a) , out ia)) && (int.TryParse( Convert.ToString(b) , out ib)))
						{
							if (ia>ib && Asc) swap = true;
							if (ia<ib && !Asc) swap = true;
						}
					}

					if (swap)
					{
						tmp = this[j];
						this[j] = this[i];
						this[i] = tmp;

					}
				}
			}

			for (var i = 0; i<=Count-1;i++) this[i].Index = i;

		}

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
					//Logger.Logger.WriteToLog(String.Format("Adding file {0}",f));
					var mp3 = new Song();
					mp3.Index = Count;
					mp3.OpenFile(f);
					this.Add(mp3);

					mp3.ID3v1.WriteToLog();
					mp3.ID3v2.WriteToLog();


					// detailed frame logging
					//foreach (TAG2Frame frame in mp3.ID3v2.Frames) { frame.WriteToLog(); }
				}

				i++;
			}

			SortColumn = SortColumnEnum.Unsorted;

			progress(this, new MP3Tagger.ProgressEventArgs(100));
		}

		public Song AddFile(string fName)
		{
			if ( (File.Exists(fName)) && (Path.GetExtension(fName) == ".mp3"))
			{
				Logger.Logger.WriteToLog(String.Format("Adding file {0}",fName));

					var mp3 = new Song();
					mp3.Index = Count;
					mp3.OpenFile(fName);
					this.Add(mp3);

					mp3.ID3v1.WriteToLog();
					mp3.ID3v2.WriteToLog();

				SortColumn = SortColumnEnum.Unsorted;
				return mp3;
			}

			return null;
		}
	
	
		#endregion
	}
}

