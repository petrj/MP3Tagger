using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
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

		private int _totalByteLength = 0;

		#endregion

		public TAGID3v2()
		{
			HeaderByteLength = 10;
			TotalByteLength = HeaderByteLength;
			Active = false;
			Changed = false;

			VersionMajor = 3;
			VersionRevision = 0;
		}

		#region properties

		public int TotalByteLength
		{
			get
			{
				return _totalByteLength;
			}
			set
			{
				_totalByteLength = value;
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

		#region public methods	

		public TAG2Frame GetOrCreateImageFrame(ImageType imgType)
		{
			var imageFrame = GetFrameByImageType(imgType);
				if (imageFrame== null)
				{
					// no frame found, creating new

					imageFrame = new TAG2Frame();
					imageFrame.Name = "APIC";
					imageFrame.FrameImage.ImgType = imgType;
					imageFrame.FrameImage.ImgDescription = "";
					imageFrame.FrameImage.ImgMime = "";

					AddFrame(imageFrame);
				}

			return imageFrame;
		}

		public bool LoadImageFrameFromFile(string fileName,ImageType imgType, bool throwExceptions)
		{
			try
			{
				var imageFrame = GetOrCreateImageFrame(imgType);
				imageFrame.FrameImage.LoadFromFile(fileName);
				
				Changed = true;
			}
			catch (Exception ex)
			{
				Logger.Logger.WriteToLog(String.Format("Error while loading image (type {0})",imgType),ex);
				if (throwExceptions) throw;
				return false;
			}

			return true;
		}

		public TAG2Frame GetFrameByImageType(ImageType imgType)
		{
			foreach (var frame in Frames)
			{
				if (frame.FrameType == FrameTypeEnum.Picture && frame.FrameImage.ImgType == imgType)
				{
					return frame;
				}
			}			

			return null;
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

		private void ParseBaseValues()
		{
			foreach (var name in BaseCollumnNames)
			{
				var frameName = FrameNamesDictionary[name];
				if (FrameByName.ContainsKey(frameName))
                {
					switch (name)
					{
						case "Title": Title = FrameByName[frameName].Value; break;
						case "Album": Album = FrameByName[frameName].Value; break;
						case "Artist": Artist = FrameByName[frameName].Value; break;
						case "Comment": Comment = FrameByName[frameName].Value; break;
					}
                }
			}

			// parsing track number
			if (FrameByName.ContainsKey(FrameNamesDictionary["Track"]))			
			{
				byte track;
				if (byte.TryParse(FrameByName[FrameNamesDictionary["Track"]].Value, out track))
				{
					TrackNumber = track;
				}
			}

			// parsing year
			if (FrameByName.ContainsKey(FrameNamesDictionary["Year"]))
			{
				var yearAsString = FrameByName[FrameNamesDictionary["Year"]].Value;
					int year;
					if (int.TryParse(yearAsString,out year))
					{
						Year = year;
					
					}
			}

			// parsing genre
			if (FrameByName.ContainsKey(FrameNamesDictionary["Genre"]))
				{
					var val = FrameByName[FrameNamesDictionary["Genre"]].Value.Trim();
                    
                    if (val != String.Empty)
                    {
                        if (val.StartsWith("(") && val.Contains(")")) // like "Metal (9)" or  "(9) Metal" 
                        {
                            var pos1 = val.IndexOf("(");
                            var pos2 = val.IndexOf(")");
                            if (pos1 > -1 && pos2 > pos1)
                            {
                                var genreAsString = val.Substring(pos1 + 1, pos2 - pos1 - 1);
                                byte g;
                                if (byte.TryParse(genreAsString, out g))
                                {
                                    if (g >= 0 && g < ID3Genre.Length)
                                        Genre = g;
                                }
                            }
                        }
                        else
                        {
                            // looking for genre only by its name ...
                            for (byte i=0; i<TAGBase.ID3Genre.Length;i++)
                            {
                                var genreString = TAGBase.ID3Genre[i].ToLower();
                                if (genreString== val.ToLower())
                                {
                                    Genre = i;
                                    break;
                                }
                            }
                        }
                    }
				}					

		}

		public List<byte> ToByteList()
		{
			var data = new List<byte>();

			foreach (var frame in Frames)
			{
				var byteList = frame.ToByteList();
				data.AddRange(byteList);
			}

			return data;
		}

		public void AddFrame(TAG2Frame frame)
		{
			Frames.Add(frame);
			if (!FrameByName.ContainsKey(frame.Name)) FrameByName.Add(frame.Name,frame);
		}

		public void AddFrame(string name,string value)
		{
			var newFrame = new TAG2Frame();
			newFrame.Name = name;		
			newFrame.Value = value;

			AddFrame(newFrame);
		}

		public void TransferBaseValuesToFrames()
		{
			foreach (var name in BaseCollumnNames)
				{
					var frameName = FrameNamesDictionary[name];
					if (FrameByName.ContainsKey(frameName))
	                {
						switch (name)
						{
							case "Title": FrameByName[frameName].Value = Title; break;
							case "Album": FrameByName[frameName].Value = Album; break;
							case "Artist": FrameByName[frameName].Value = Artist; break;
							case "Comment": FrameByName[frameName].Value = Comment; break;
						}
	                } else
					{
						// frame does not exist
						var newFrame = new TAG2Frame();
						newFrame.Name = frameName;						
						switch (name)
						{
							case "Title": newFrame.Value = Title; break;
							case "Album": newFrame.Value =  Album; break;
							case "Artist": newFrame.Value = Artist; break;
							case "Comment": newFrame.Value = Comment; break;
						}

						AddFrame(newFrame);
					}
				}

			//  track number
			var trackFrameName = FrameNamesDictionary["Track"];
			if (!FrameByName.ContainsKey(trackFrameName))			
			{
					AddFrame(trackFrameName,"");
			}
			FrameByName[trackFrameName].Value = TrackNumber.ToString();

			// year
			var yearFrameName = FrameNamesDictionary["Year"];
			if (!FrameByName.ContainsKey(yearFrameName))
			{
				AddFrame(yearFrameName,"");
			}
			FrameByName[yearFrameName].Value = Year.ToString();


			// genre
			var genreFrameName = FrameNamesDictionary["Genre"];
			if (!FrameByName.ContainsKey(genreFrameName))
			{
				AddFrame(genreFrameName,"");
			}
			FrameByName[genreFrameName].Value = GenreText + "("+Genre+")";
		}

		#endregion

		#region public override methods

		public override bool SaveToStream (FileStream fStream, bool throwExceptions)
		{
			/*

				http://id3.org/id3v2.4.0-structure

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

					a - Unsynchronisation
				   	b - Extended header
				   	c - Experimental indicator
				   	d - Footer present		
			*/

			try
			{
				Logger.Logger.WriteToLog(String.Format("Saving TAG v2 ..."));
		
				var tag = new List<byte>();

				TransferBaseValuesToFrames();

				// generating frames data

				var framesData = ToByteList();
				TotalByteLength = framesData.Count+HeaderByteLength;

				// header v 4.0

				tag.Add(73);  // I
				tag.Add(68);  // D
				tag.Add(51);  // 3

				tag.Add(4);  // VersionMajor
				tag.Add(0);  // VersionRevision

				tag.Add(0);  // no flags

				tag.AddRange(TAG2Frame.MakeID3v2SizeAsByteArray(TotalByteLength)); // size

				// writing frames

				tag.AddRange(framesData); 

				fStream.Write(tag.ToArray(),0,tag.Count);

				Logger.Logger.WriteToLog(String.Format("TAG saved: (Title:{0}, Artist:{1}, ...)",Title,Artist));

				Changed = false;	
				return true;

			} catch (Exception ex)
			{
				Logger.Logger.WriteToLog(String.Format("Error while saving TAG v2"),ex);
				if (throwExceptions) throw;
				return false;
			}

			return false;
		}

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
				Changed = false;

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

				var totalTagSize = new byte[] {0,0,0,0};
				for (var i=0;i<4;i++) totalTagSize[i] = OriginalHeader[6+i];

				if (!IsValid)
				{
					Logger.Logger.WriteToLog("TAG v2 not found (or invalid header)");
					return false;
				}

				TotalByteLength =  Convert.ToInt32(TAG2Frame.MakeID3v2Size(totalTagSize,7));

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

					long actSize = 0;
					while(actSize<TotalByteLength-HeaderByteLength)
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

						AddFrame(frame);						
					}

				ParseBaseValues();

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

