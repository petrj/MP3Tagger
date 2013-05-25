using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using MP3Tagger;

namespace MP3Tagger
{
	public class TAGID3v2 : MP3Tagger.TAGBase
	{
		// http://id3.org/id3v2.4.0-structure
		// http://id3.org/id3v2.3.0

		#region private fields

		private List<TAG2Frame> _frames = new List<TAG2Frame>();
		private Dictionary<string,TAG2Frame> _frameByName = new Dictionary<string,TAG2Frame>();

		private byte _versionMajor  = 0;							// 1
		private byte _versionRevision  = 0;							// 1
		private long _framesSize = 0;

		#endregion

		#region Basic Frames names constants

		public static string frameNameTitle = "TIT2";
		public static string frameNameArtist = "TPE1";
		public static string frameNameAlbum = "TALB";

		public static string frameNameYear = "TYER";
		public static string frameNameComment = "COMM";
		public static string frameNameGenre = "TCON";
		public static string frameNameTrack = "TRCK";

		#endregion

		public TAGID3v2()
		{
			HeaderByteLength = 10;
			Active = false;
		}

		#region properties

		public int TotalByteLength
		{
			get
			{
				return HeaderByteLength + Convert.ToInt32(FramesSize);
			}
		}

		public byte VersionMajor 
		{
			get { return _versionMajor;	}
			set {_versionMajor = value;	}
		}

		public byte VersionRevision 
		{
			get {return _versionRevision;}
			set {_versionRevision = value;}
		}
		/*
		public override string GenreText
		{
			get
			{
				if (FrameByName.ContainsKey(frameNameGenre))
				{
					var val = FrameByName[frameNameGenre].Value;
					if (val.StartsWith("(") && val.Contains(")"))
					{
						var x = val.Split(')',2);
						if (x.Length == 2)
						{
							return x[1];
						}					
					}
				}					

				return null;
			}
		}
*/

		public Dictionary<string, TAG2Frame> FrameByName 
		{
			get { return _frameByName;	}
			set {_frameByName = value; }
		}

		public List<TAG2Frame> Frames 
		{
			get 
			{
				return _frames;
			}
			set 
			{
				_frames = value;
			}
		}

		public long FramesSize
		{
			get
			{
				return _framesSize;
			}
			set
			{
				_framesSize = value;
			}
		}

		public bool IsValid
		{
			get
			{
				//
				// ID3v2/file identifier   "ID3"
				// ID3v2 version           $03 00
				// ID3v2 flags             %abc00000
				// ID3v2 size              4 * %0xxxxxxx
				//
				// An ID3v2 tag can be detected with the following pattern:
	     		// $49 44 33 yy yy xx zz zz zz zz
	     		// Where yy is less than $FF, xx is the 'flags' byte and zz is less than $80. 

			  if (
					(OriginalHeader[0]==73) &&    	// I
              		(OriginalHeader[1]==68) &&		// D
              		(OriginalHeader[2]==51) &&		// 3
					(VersionMajor<255) &&
					(VersionRevision<255) &&
					((OriginalHeader[5] & 31) == 0) &&
					(OriginalHeader[6] < 128) &&
					(OriginalHeader[7] < 128) &&
					(OriginalHeader[8] < 128) &&
					(OriginalHeader[9] < 128)
					)
				{
					return true;
				}

				return false;
			}
		}

		#endregion

		#region Flags

		public bool FlagUnsynchronisation
		{
			get { return (OriginalHeader[5] & 128) == 128; }
		}

		public bool FlagExtendedHeader
		{
			get { return (OriginalHeader[5] & 64) == 64; }
		}

		public bool FlagExperimental
		{
			get { return (OriginalHeader[5] & 32) == 32; }
		}

		public bool FlagFooter
		{
			get { return (OriginalHeader[5] & 16) == 16; }
		}

		#endregion

		#region private methods

		private void SetBaseValues()
		{
			if (FrameByName.ContainsKey(frameNameAlbum)) Album = FrameByName[frameNameAlbum].Value;
			if (FrameByName.ContainsKey(frameNameTitle)) Title = FrameByName[frameNameTitle].Value;
			if (FrameByName.ContainsKey(frameNameArtist)) Artist = FrameByName[frameNameArtist].Value;
			if (FrameByName.ContainsKey(frameNameComment)) Comment = FrameByName[frameNameComment].Value;

			// parsing track number
			if (FrameByName.ContainsKey(frameNameTrack))			
			{
				byte track;
				if (byte.TryParse(FrameByName[frameNameTrack].Value, out track))
				{
					TrackNumber = track;
				}
			}

			// parsing year
			if (FrameByName.ContainsKey(frameNameYear))
			{
				var yearAsString = FrameByName[frameNameYear].Value;
					int year;
					if (int.TryParse(yearAsString,out year))
					{
						Year = year;
					
					}
			}

			// parsing genre
			if (FrameByName.ContainsKey(frameNameGenre))
				{
					var val = FrameByName[frameNameGenre].Value.Trim();
					if (val.StartsWith("(") && val.Contains(")"))
					{
						var pos1 = val.IndexOf("(");
						var pos2 = val.IndexOf(")");
						if (pos1 >-1 && pos2>pos1)
						{
							var genreAsString = val.Substring(pos1+1,pos2-pos1-1);
							byte g;
							if (byte.TryParse(genreAsString,out g))
						    {
								if (g>=0 && g<ID3Genre.Length)
								Genre = g;
							}
						}
						
					}
				}					

		}

		#endregion

		#region public override methods

		public override void Clear()
		{
			Frames.Clear();
			FrameByName.Clear();

			_versionMajor  = 0;						
			_versionRevision  = 0;
			_framesSize = 0;

			base.Clear(); 
		}

		public override void WriteToLog()
		{
			Logger.Logger.WriteToLog(String.Format("TAG v2 {0}:",FileName));
			base.WriteToLog();
		}

		public override bool ReadFromStream (FileStream fStream, bool throwExceptions)
		{
				/*

			 +-----------------------------+
		     |      Header (10 bytes)      |
		     +-----------------------------+
		     |       Extended Header       |
		     | (variable length, OPTIONAL) |
		     +-----------------------------+
		     |   Frames (variable length)  |
		     +-----------------------------+
		     |           Padding           |
		     | (variable length, OPTIONAL) |
		     +-----------------------------+
		     | Footer (10 bytes, OPTIONAL) |
		     +-----------------------------+

				ID3v2/file identifier      "ID3"
				     ID3v2 version              $04 00
				     ID3v2 flags                %abcd0000
				     ID3v2 size             4 * %0xxxxxxx

		     */

			try
			{
				Loaded = false;
				Active = false;

				Logger.Logger.WriteToLog(String.Format("Reading TAG v2 ..."));

				if (fStream.Length<HeaderByteLength)
				{
					throw new Exception(String.Format("File length < {0}",HeaderByteLength));
				}

				OriginalHeader = new byte[HeaderByteLength];

				fStream.Seek(0,0);
				fStream.Read(OriginalHeader,0,HeaderByteLength);

				byte[] id3tag2Indication = new byte[] {OriginalHeader[0],OriginalHeader[1],OriginalHeader[2]};

				VersionMajor = OriginalHeader[3];
				VersionRevision = OriginalHeader[4];

				var size = new byte[] {0,0,0,0};
				for (var i=0;i<4;i++) size[i] = OriginalHeader[6+i];
				FramesSize = TAG2Frame.MakeID3v2Size(size,7);

				if (!IsValid)
				{
					Logger.Logger.WriteToLog("TAG v2 not found (or invalid header)");
					return false;
				}

				if (FlagUnsynchronisation)
				{
					throw new Exception("TAG v2 Unsynchronisation not supported yet");
				}

				if (FlagExtendedHeader)
				{
					throw new Exception("Extended header not supported yet");
				}

				if (FlagFooter)
				{
					throw new Exception("Usage of footer - not supported yet");
				}

					var framesTotalSize = FramesSize;

					long actSize = 0;
					while(actSize<framesTotalSize)
					{
						var frame = new TAG2Frame();
						var ok = frame.ReadFromOpenedStream(fStream,VersionMajor,false);													
						if (!ok)
						{
							//if (throwExceptions) throw new Exception("Error reading frame");
							if (Frames.Count == 0) Logger.Logger.WriteToLog("First Invalid frame found");
							break;
						}

						actSize+=TAG2Frame.HeaderByteLength+frame.Size;
						
						/*
						Logger.Logger.WriteToLog(String.Format("Adding frame {0}",frame.Name));
						Logger.Logger.WriteToLog("Name:"+frame.Name);
						Logger.Logger.WriteToLog("Size:"+frame.Size);
						Logger.Logger.WriteToLog("Value:"+frame.Value);
						if (frame.Name != "APIC")
						Logger.Logger.WriteToLog("Data:"+TAG2Frame.ByteArrayToDecimalStrings(frame.OriginalData));
						Logger.Logger.WriteToLog("-------------");
						*/

						Frames.Add(frame);

						if (!FrameByName.ContainsKey(frame.Name)) FrameByName.Add(frame.Name,frame);
					}

				SetBaseValues();

				Logger.Logger.WriteToLog(String.Format("TAG found: (Title:{0}, Artist:{1}, ...)",Title,Artist));

				Active = true;
				Loaded = true;
				Changed = false;

				return true;

			} catch (Exception ex)
			{
				Logger.Logger.WriteToLog(String.Format("Error while reading TAG v2"),ex);
				if (throwExceptions) throw;
				return false;
			}
		}	

		#endregion
	}
}

