using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Logger;


namespace MP3Tagger
{
	public class TAGBase
	{
		#region public fields

		public bool Active { get; set; }
		public bool Loaded { get; set; }
		public bool Changed { get; set; }

		public Encoding DefaultEncoding = Encoding.GetEncoding("iso-8859-1");
		private string _fileName = null;

		public static List<string> AllCollumnNames = new List<string>() {"Title", "Artist", "Album", "Year","Comment", "Genre"};

		// basic values
		private string _title;
		private string _artist;
		private string _album;
		private int _year;
		private string _comment;
		private byte _genre;
		//------------------------------------------//

		public byte[] OriginalHeader  { get; set; }
		public byte HeaderByteLength  { get; set; }

		#endregion

		public TAGBase()
		{
			Loaded = false;
			Active = false;
			Changed = false;
		}

		#region vasic values properties

		public string Comment 
		{
			get { return _comment; }
			set 
			{
				if (value != _comment) Changed = true;
				_comment = value;

			}
		}

		public byte Genre 
		{
			get { return _genre; }
			set 
			{
				if (value != _genre) Changed = true;
				_genre = value;
			}
		}

		public int Year 
		{
			get { return _year;	}
			set 
			{
				if (value != _year) Changed = true;
				_year = value;
			}
		}

		public string Album 
		{
			get { return _album; }
			set 
			{
				if (value != _album) Changed = true;
				_album = value;
			}
		}

		public string Artist 
		{
			get { return _artist;}
			set 
			{
				if (value != _artist) Changed = true;
				_artist = value;
			}
		}

		public string Title 
		{
			get { return _title; }
			set 
			{
				if (value != _title) Changed = true;
				_title = value;
			}
		}

		#endregion

		#region properties

		public string FileName 
		{
			get { return _fileName;	}
		}

		public virtual string GenreText
		{
			get
			{
				if (Genre>=0 && Genre<ID3Genre.Length)
				{
					return ID3Genre[Genre];
				}

				return String.Empty;
			}
		}

		#endregion

		#region public methods

		public virtual void WriteToLog()
		{
			Logger.Logger.WriteToLog(String.Format("  Title  :{0}",Title));
			Logger.Logger.WriteToLog(String.Format("  Artist :{0}",Artist));
			Logger.Logger.WriteToLog(String.Format("  Album  :{0}",Album));
			Logger.Logger.WriteToLog(String.Format("  Year   :{0}",Year));
			Logger.Logger.WriteToLog(String.Format("  Comment:{0}",Comment));
			Logger.Logger.WriteToLog(String.Format("  Genre  :{0}",GenreText));
		}

        public List<object> ValuesAsOLbjecttList(List<string> columnNames)
        {
            var result = new List<object>();

            foreach (var col in columnNames)
            {
                switch (col)
                {
                    case "Artist": result.Add(Artist);break;
                    case "Album": result.Add(Album); break;
                    case "Year": result.Add(Year == 0 ? String.Empty : Year.ToString()); break;
                    case "Comment": result.Add(Comment); break;
                    case "Title": result.Add(Title); break;
                    case "Genre": result.Add(GenreText); break;

				default: result.Add("Unknown column "+col); break;
                }
            }

            return result;
        }

	    public virtual bool ReadFromFile(string fileName, bool throwExceptions=false)
		{
			try
			{
				_fileName = fileName;

				if (!File.Exists(fileName))
				{
					throw new Exception(String.Format("File {0} does not exist",FileName));
				}

				var fInfo = new FileInfo(fileName);
				if (fInfo.Length<HeaderByteLength)
				{
					throw new Exception(String.Format("File length < {0}",HeaderByteLength));
				}

				var result = false;

				using (var fStream = new FileStream(FileName,FileMode.Open))
				{
					result = ReadFromStream(fStream,throwExceptions);
					fStream.Close();
				}

				return result;

			} catch (Exception ex)
			{
				Logger.Logger.WriteToLog(String.Format("Error while reading TAG v1"),ex);
				if (throwExceptions) throw;
				return false;
			}
		}

		public virtual bool ReadFromStream(FileStream fStream, bool throwExceptions=false)
		{
			throw new Exception("Calling base ReadFromStream");
		}

		#endregion

		#region Genre Constant

	 	public static string[] ID3Genre = 
		{
		"Blues",
		"Classic Rock",
		"Country",
		"Dance",
		"Disco",
		"Funk",
		"Grunge",
		"Hip-Hop",
		"Jazz",
		"Metal",
		"New Age",
		"Oldies",
		"Other",
		"Pop",
		"R&B",
		"Rap",
		"Reggae",
		"Rock",
		"Techno",
		"Industrial",
		"Alternative",
		"Ska",
		"Death Metal",
		"Pranks",
		"Soundtrack",
		"Euro-Techno",
		"Ambient",
		"Trip-Hop",
		"Vocal",
		"Jazz+Funk",
		"Fusion",
		"Trance",
		"Classical",
		"Instrumental",
		"Acid",
		"House",
		"Game",
		"Sound Clip",
		"Gospel",
		"Noise",
		"Alternative Rock",
		"Bass",
		"Soul",
		"Punk",
		"Space",
		"Meditative",
		"Instrumental Pop",
		"Instrumental Rock",
		"Ethnic",
		"Gothic",
		"Darkwave",
		"Techno-Industrial",
		"Electronic",
		"Pop-Folk",
		"Eurodance",
		"Dream",
		"Southern Rock",
		"Comedy",
		"Cult",
		"Gangsta",
		"Top 40",
		"Christian Rap",
		"Pop/Funk",
		"Jungle",
		"Native US",
		"Cabaret",
		"New Wave",
		"Psychadelic",
		"Rave",
		"Showtunes",
		"Trailer",
		"Lo-Fi",
		"Tribal",
		"Acid Punk",
		"Acid Jazz",
		"Polka",
		"Retro",
		"Musical",
		"Rock & Roll",
		"Hard Rock",
		"Folk",
		"Folk-Rock",
		"National Folk",
		"Swing",
		"Fast Fusion",
		"Bebob",
		"Latin",
		"Revival",
		"Celtic",
		"Bluegrass",
		"Avantgarde",
		"Gothic Rock",
		"Progressive Rock",
		"Psychedelic Rock",
		"Symphonic Rock",
		"Slow Rock",
		"Big Band",
		"Chorus",
		"Easy Listening",
		"Acoustic",
		"Humour",
		"Speech",
		"Chanson",
		"Opera",
		"Chamber Music",
		"Sonata",
		"Symphony",
		"Booty Bass",
		"Primus",
		"Porn Groove",
		"Satire",
		"Slow Jam",
		"Club",
		"Tango",
		"Samba",
		"Folklore",
		"Ballad",
		"Power Ballad",
		"Rhytmic Soul",
		"Freestyle",
		"Duet",
		"Punk Rock",
		"Drum Solo",
		"Acapella",
		"Euro-House",
		"Dance Hall",
		"Goa",
		"Drum & Bass",
		"Club-House",
		"Hardcore",
		"Terror",
		"Indie",
		"BritPop",
		"Negerpunk",
		"Polsk Punk",
		"Beat",
		"Christian Gangsta Rap",
		"Heavy Metal",
		"Black Metal",
		"Crossover",
		"Contemporary Christian",
		"Christian Rock",
		"Merengue",
		"Salsa",
		"Trash Metal",
		"Anime",
		"Jpop",
		"Synthpop"
		};

		#endregion
	
	}
}
