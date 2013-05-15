using System;
using System.IO;
using System.Text;

namespace MP3Tagger
{
	public class TAGID3v1 : MP3Tagger.TAGBase
	{
		/*
			header 	3 	"TAG"
			title 	30 	30 characters of the title
			artist 	30 	30 characters of the artist name
			album 	30 	30 characters of the album name
			year 	4 	A four-digit year
			comment 	28[3] or 30 	The comment.
			zero-byte[3] 	1 	If a track number is stored, this byte contains a binary 0.
			track[3] 	1 	The number of the track on the album, or 0. Invalid, if previous byte is not a binary 0.
			genre 	1 	Index in a list of genres, or 255
		 */

		#region Exended header

		// http://en.wikipedia.org/wiki/ID3

		private bool _extendedHeaderPresent = false;
		public byte[] OriginalExtendedHeaderData { get; set; }

		public string ExtendedTitle { get; set; }
		public string ExtendedArtist { get; set; }
		public string ExtendedAlbum { get; set; }
		public int ExtendedSpeed { get; set; }
		public string ExtendedGenre { get; set; }

		public string ExtendedStartTime { get; set; }
		public string ExtendedEndTime { get; set; }

		public static byte ExtendedByteLength { get; set; }

		#endregion 

		public TAGID3v1 ()
		{
			Genre = 255;
			Active = false;
			HeaderByteLength = 128;   // whole tag byte length
			ExtendedByteLength = 227;
		}	

		#region properties

		public bool ExtendedHeaderPresent 
		{
			get { return _extendedHeaderPresent; }
		}

		#endregion

		public void ConsoleInfo()
		{
			Console.WriteLine("Title:"+Title);
			Console.WriteLine("Artist:"+Artist);
			Console.WriteLine("Album:"+Album);
			Console.WriteLine("Year:"+Year.ToString());
			Console.WriteLine("Comment:"+Comment);
			Console.WriteLine("Genre:"+GenreText);
		}

		#region private methods

		private bool ReadExtendedHeader(FileStream fs, bool throwExceptions=false)
		{
			if (HeaderByteLength-ExtendedByteLength < fs.Length)
			{
				fs.Seek(fs.Length-HeaderByteLength-ExtendedByteLength,0);

				OriginalExtendedHeaderData = new byte[TAGID3v1.ExtendedByteLength];
				fs.Read(OriginalExtendedHeaderData,0,ExtendedByteLength);

				var flag = System.Text.Encoding.ASCII.GetString(OriginalExtendedHeaderData,0,4);

				if (flag != "TAG+")
				{
					if (throwExceptions) throw new Exception("TAG+ not found");
					return false;
				}

				ExtendedTitle = DefaultEncoding.GetString(OriginalExtendedHeaderData,4,60).Trim();
				ExtendedArtist = DefaultEncoding.GetString(OriginalExtendedHeaderData,64,60).Trim();
				ExtendedAlbum = DefaultEncoding.GetString(OriginalExtendedHeaderData,124,60).Trim();
				ExtendedSpeed = OriginalExtendedHeaderData[184];
				ExtendedGenre = DefaultEncoding.GetString(OriginalExtendedHeaderData,185,30).Trim();
				ExtendedStartTime = DefaultEncoding.GetString(OriginalExtendedHeaderData,215,6).Trim();
				ExtendedStartTime = DefaultEncoding.GetString(OriginalExtendedHeaderData,221,6).Trim();

				_extendedHeaderPresent = true;

				return true;
			}

			return false;
		}

		private string GetNotNullSubString(int start,int count)
		{
			var finish = start+count-1;
			while ( (finish>=start) && OriginalHeader[finish] == 0 )
			{
				finish--;
			}

			return DefaultEncoding.GetString(OriginalHeader,start,finish-start+1).Trim();
		}

		private void ParseHeader()
		{
				Title = GetNotNullSubString(3,30);
				Artist = GetNotNullSubString(33,30);
				Album = GetNotNullSubString(63,30);
				var y = GetNotNullSubString(93,4);
				Comment = GetNotNullSubString(97,30);

				int year;
				if (int.TryParse(y,out year))
				{
					Year = year;
				}

				Genre = OriginalHeader[HeaderByteLength-1];

				Active = true;
		}

		#endregion

		#region public override methods

		public override void WriteToLog()
		{
			Logger.Logger.WriteToLog(String.Format("TAG v1 {0}:",FileName));
			base.WriteToLog();
		}

		public override bool ReadFromStream(FileStream fStream, bool throwExceptions=false)
		{
			try
			{
				Loaded = false;
				Active = false;

				Logger.Logger.WriteToLog(String.Format("Reading TAG v1 ..."));
		
				if (fStream.Length<HeaderByteLength)
				{
					throw new Exception(String.Format("File length < {0}",HeaderByteLength));
				}

				OriginalHeader = new byte[HeaderByteLength];

				fStream.Seek(fStream.Length-HeaderByteLength,0);
				fStream.Read(OriginalHeader,0,HeaderByteLength);

				ReadExtendedHeader(fStream);

				var flag = System.Text.Encoding.ASCII.GetString(OriginalHeader,0,3);

				if (flag != "TAG")
				{
					Logger.Logger.WriteToLog("TAG not found");
					return false; 
				}

				ParseHeader();

				Logger.Logger.WriteToLog(String.Format("TAG found: (Title:{0}, Artist:{1}, ...)",Title,Artist));

				Loaded = true;
				Changed = false;

				return true;

			} catch (Exception ex)
			{
				Logger.Logger.WriteToLog(String.Format("Error while reading TAG v1"),ex);
				if (throwExceptions) throw;
				return false;
			}

		}

		#endregion
	}
}

