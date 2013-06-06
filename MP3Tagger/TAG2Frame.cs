using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Logger;

namespace MP3Tagger
{
	public enum ImageType
	{
	 	Other = 0,
	 	Icon = 1,
		OtherIcon = 2,
		CoverFront = 3,
		CoverBack = 4,
		LeafletPage = 5,
		Media = 6,
		LeadArtist = 7,
		Artist = 8,
		Conductor = 9,
		Band = 10,
		Composer = 11,
		Lyricist = 12,
		RecordingLocation = 13,
		DuringRecording = 14,
		DuringPerformance = 15,
		MovieCapture = 16,
		ABrightColouredFish = 17,
		Illustration = 18,
		BandLogotype = 19,
		Publisher = 20
	}

	public enum FrameTypeEnum
	{
		Unsupported = 0,
		CommonText = 1,
		Picture = 2,
		Lyrics = 3,
		Comment = 4,
		Genre = 5
	}

	public class TAG2Frame
	{
		public static byte HeaderByteLength = 10;

		#region private fields

		private Encoding _defaultEncoding = Encoding.GetEncoding("iso-8859-1");

		private byte[] _originalFrameHeader;			
		private byte _versionMajor;			

		private byte[] _flags  = new byte[] {0,0};					

		private long _size  = 0;
		private byte[] _originalData;
		private string _value;
		private string _name;


		#region image fields

		private string _imgMime;
		private string _imgDescription;
		private Image _img;
		private ImageType _imgType;

		#endregion

		#endregion

		// Frame ID       $xx xx xx xx (four characters)
		// Size           $xx xx xx xx
		// Flags          $xx xx

		#region public methods

		public void WriteToLog()
		{						
			Logger.Logger.WriteToLog("Frame Info:");
			Logger.Logger.WriteToLog("Name:"+Name);
			Logger.Logger.WriteToLog("Size:"+Size);
			Logger.Logger.WriteToLog("Value:"+Value);
			if (Name != "APIC")
			Logger.Logger.WriteToLog("Data:"+TAG2Frame.ByteArrayToDecimalStrings(OriginalData));
			Logger.Logger.WriteToLog("-------------");
		}

		public List<byte> ToByteList()
		{
			/*
			     Frame ID      $xx xx xx xx  (four characters)
			     Size      4 * %0xxxxxxx
			     Flags         %0abc0000 %0h00kmnp

 					   a - Tag alter preservation
					   b - File alter preservation
					   c - Read only

					   h - Grouping identity
					   
					   k - Compression
					   m - Encryption
					   n - Unsynchronisation
					   p - Data length indicator

			*/

			var res = new List<byte>();

			while (Name.Length<4) Name+=" ";
			if (Name.Length>4) Name = Name.Substring(0,4);

			// header
			var id3 = _defaultEncoding.GetBytes(Name);
			res.AddRange(id3);

			var valueData = new List<byte>();
			switch (FrameType)
			{
				case FrameTypeEnum.CommonText:
				case FrameTypeEnum.Genre: valueData = SaveCommonTextData(); break;
				case FrameTypeEnum.Comment:valueData = SaveComment(); break;
				case FrameTypeEnum.Lyrics:valueData = SaveLyrics(); break;
				case FrameTypeEnum.Picture: valueData = SaveImageData(); break;
				case FrameTypeEnum.Unsupported: valueData = new List<byte>(OriginalData); break;

				default: break;
			}

			var size = valueData.Count+ 1; // zero byte
			var sizeAsByteArray = MakeID3v2SizeAsByteArray(size,7);
			res.AddRange(sizeAsByteArray);

			// flags 
			res.Add (0);
			res.Add (0);

			// value 
			res.AddRange(valueData);

			// zero byte
			res.Add (0);  

			return res;
		}

		public bool ReadFromOpenedStream(FileStream fStream, byte versionMajor, bool throwExceptions=false)
		{
			try
			{
				VersionMajor = versionMajor;

				if (fStream.Position+10>fStream.Length)
				{
					if (throwExceptions) throw new Exception(String.Format("Stream end, cannot read frame header"));;;
					return false;
				}

				OriginalFrameHeader = new byte[HeaderByteLength];

				fStream.Read(OriginalFrameHeader,0,HeaderByteLength);

					var size = new byte[] {0,0,0,0};
					for (var i=0;i<4;i++) size[i] = OriginalFrameHeader[4+i];
					Size = MakeID3v2Size(size,8);

					Flags[0] = OriginalFrameHeader[8]; // %abc00000   %0abc0000 
					Flags[1] = OriginalFrameHeader[9]; // %ijk00000   0h00kmnp

					if (!IsValid)
					{
						if (throwExceptions) throw new Exception(String.Format("Invalid frame header: {0}",ByteArrayToDecimalStrings(OriginalFrameHeader)));
						return false;
					}					

					if (FlagCompressed)
					{
						if (throwExceptions) throw new Exception("Compression not supported yet");
						return false;
					}

					if (FlagEncrypted)
					{
						if (throwExceptions) throw new Exception("Encryption not supported yet");
						return false;
					}			


					if (FlagGrouping)
					{
						if (throwExceptions) throw new Exception("Grouping not supported yet");
						return false;
					}

					if (FlagUnsynchronisation)
					{
						if (throwExceptions) throw new Exception("Unsynchronisation not supported yet");
						return false;
					}

					if (FlagDataLengthIndicatorPresent)
					{
						if (throwExceptions) throw new Exception("Usage of Data Length Indicator - not supported yet");
						return false;
					}

					Name =  String.Empty;
					for (var i=0;i<4;i++) Name +=  System.Text.Encoding.ASCII.GetString( new byte[] {OriginalFrameHeader[i]});					

					if (fStream.Position+Size>fStream.Length)
					{
						if (throwExceptions) throw new Exception(String.Format("Stream end, cannot read frame data"));
						return false;
					}

					_originalData = new byte[Size];
					fStream.Read(OriginalData,0,(int)Size);

					//Value =  DefaultEncoding.GetString(_originalData);

					ParseDataToValue();			

				return true;

			} catch (Exception ex)
			{
				if (throwExceptions) throw;
				return false;
			}		
		
		}			

		#endregion

		#region private methods

		private List<byte> SaveImageData()
		{
			var res = new List<byte>();

			res.Add(0); // default encoding

			// mime
			res.AddRange(DefaultEncoding.GetBytes(ImgMime));
			res.Add(0); // zero byte;

			// image type
			res.Add (Convert.ToByte(ImgType));

			// description
			res.AddRange(DefaultEncoding.GetBytes(ImgDescription));
			res.Add(0);  // zero byte;

			// binary data
			using (var  ms = new MemoryStream())
			{
 				ImageData.Save(ms,ImageData.RawFormat);
				res.AddRange(ms.ToArray());
			} 

			return res;
		}

		private List<byte> SaveComment()
		{
			var res = new List<byte>();

			res.Add(0); // default encoding

			res.Add(101); // e
			res.Add(110); // n
			res.Add(103); // g

			res.Add(0); // content description

			var bytes = DefaultEncoding.GetBytes(Value);
			res.AddRange(bytes);

			res.Add(0); // zero byte;

			return res;
		}

		private List<byte> SaveLyrics()
		{
			var res = new List<byte>();

			res.Add(0); // default encoding

			res.Add(101); // e
			res.Add(110); // n
			res.Add(103); // g

			res.Add(0); // content description

			var bytes = DefaultEncoding.GetBytes(Value);
			res.AddRange(bytes);

			res.Add(0); // zero byte;

			return res;
		}

		private List<byte> SaveCommonTextData()
		{
			var res = new List<byte>();

			res.Add(0); // default encoding

			var bytes = DefaultEncoding.GetBytes(Value);
			res.AddRange(bytes);

			res.Add(0); // zero byte;

			return res;
		}

		private Encoding GetFrameEncoding(byte encodingByte)
		{
			/*
			$00 – ISO-8859-1 (LATIN-1, Identical to ASCII for values smaller than 0x80).
			$01 – UCS-2 (UTF-16 encoded Unicode with BOM), in ID3v2.2 and ID3v2.3.
			$02 – UTF-16BE encoded Unicode without BOM, in ID3v2.4.
			$03 – UTF-8 encoded Unicode, in ID3v2.4.
			*/

			switch (encodingByte)
			{
				case 0: return DefaultEncoding;
				case 1: return System.Text.Encoding.Unicode;
				case 2: return System.Text.Encoding.Unicode; 
				case 3: return System.Text.Encoding.UTF8;
				default:throw new Exception(String.Format("Unknown frame encoding:{0}",encodingByte));	
			}			
		}

		/// <summary>
		/// Zeros the bytes at position.
		/// </summary>
		/// <returns>
		/// number of zero bytes found
		/// </returns>
		/// <param name='position'>
		/// Position.
		/// </param>
		/// <param name='currentEncoding'>
		/// Current encoding.
		/// </param>
		private int ZeroBytesAtPosition(int position,System.Text.Encoding currentEncoding)
		{
			if (OriginalData[position] == 0)
							{
								// terminating sequence?
								if (currentEncoding == DefaultEncoding)
								{									
									return 1; // single byte encoding
								}
								if (currentEncoding == System.Text.Encoding.Unicode &&
						    		position>1 &&	 // min 2 bytes in OriginalData
						    		OriginalData[position-1] == 0)
								{
									// double byte encoding
									return 2;
								}					    	
							}
			return 0;
		}

		private void ParseTextFrameData()
		{
			var frameEncoding = GetFrameHeaderEncoding();
			var currentEncoding = frameEncoding == null ? DefaultEncoding : frameEncoding;

			if (frameEncoding == null)
			{
				// no encoding byte presents
				int position = 0;
				Value = ParseFrameValueFromPosition(ref position,DefaultEncoding);
			} else
			{
				int position = 1;
				Value = ParseFrameValueFromPosition(ref position,frameEncoding);
			}
		}

		private System.Text.Encoding GetFrameHeaderEncoding()
		{
			if (Size==0)
				return null;

			if (_originalData[0]>3)
			{
				return null;
			}

			if (
					(Name.StartsWith("T")) || 	//  User defined text information frame
					(Name.StartsWith("W")) ||	//	User defined URL link frame
					(Name == "USLT") ||			//  Unsynchronised lyrics/text transcription
					(Name == "SYLT") ||			//  Synchronised lyrics/text
					(Name == "USER") ||			//  Terms of use frame
					(Name == "OWNE") ||			//  
					(Name == "COMM") ||			//  
					(Name == "APIC") ||			//  
					(Name == "COMR") 			//  
				)
			{
				var encodingByte = OriginalData[0];
				return GetFrameEncoding(encodingByte);
			}

			return null;								
		}

		private void RemoveBOM(List <byte> byteArrayList,int pos)
		{
			if (pos>=0 && byteArrayList.Count>=2 && pos+1<=byteArrayList.Count-1)
			{
				if (
					    (
					    	byteArrayList[pos] == 255 &&
					    	byteArrayList[pos+1] == 254
						) ||
					    (
					    	byteArrayList[pos] == 254 &&
					    	byteArrayList[pos+1] == 255
						)				    				    
				    )
				{
					// removing unicode BOM 
					byteArrayList.RemoveRange(pos,2);
				}
			}
		}

		private void ParseUnsynchronisedLyricsText()
		{
			/*
		     <Header for 'Unsynchronised lyrics/text transcription', ID: "USLT">

			 Text encoding        $xx
		     Language             $xx xx xx
		     Content descriptor   <text string according to encoding> $00 (00)
		     Lyrics/text          <full text string according to encoding>
		     
			 */

			if (OriginalData.Length<5)  // encoding (1) + language (3) + content descrip (min 1)
			return;

			var frameEncoding = GetFrameHeaderEncoding();
			var currentEncoding = frameEncoding == null ? DefaultEncoding : frameEncoding;

			// reading 3 bytes Language
			//OriginalData

			var languageAsByteList = new List<byte>();
			languageAsByteList.Add(OriginalData[1]);
			languageAsByteList.Add(OriginalData[2]);
			languageAsByteList.Add(OriginalData[3]);
			var language = DefaultEncoding.GetString( languageAsByteList.ToArray() );

			var position = 4;
			var contentDesc = ParseFrameValueFromPosition(ref position,currentEncoding);		

			Value = ParseFrameValueFromPosition(ref position,currentEncoding);		
		}

		private string ParseFrameValueFromPosition(ref int position, System.Text.Encoding currentEncoding)
		{
			var dataBytes = new List <byte>();
			int i;
			for (i=position;i<Size;i++)
			{
				position++;
				int zeroBytes = ZeroBytesAtPosition(i,currentEncoding);
				if (zeroBytes == 0)
				{
					dataBytes.Add(OriginalData[i]);
				} else
				if (zeroBytes == 1)
				{
					break;
				}
				else
				if (zeroBytes == 2)
				{
					dataBytes.RemoveAt( dataBytes.Count-1 );
					break;
				}
			}

			if (dataBytes.Count > 0)
			{
				if (currentEncoding == Encoding.Unicode)
				{
					RemoveBOM(dataBytes,0);
					RemoveBOM(dataBytes,dataBytes.Count-2);
				}			

				return currentEncoding.GetString( dataBytes.ToArray() );
			}

			return null;
		}

		private void ParseCommentData()
		{
			/*
			  <Header for 'Comment', ID: "COMM">
			  
		     Text encoding          $xx
		     Language               $xx xx xx
		     Short content descrip. <text string according to encoding> $00 (00)
		     The actual text        <full text string according to encoding>
		     
			 */

			if (OriginalData.Length<5)  // encoding (1) + language (3) + Short content descrip (min 1)
			return;

			var frameEncoding = GetFrameHeaderEncoding();
			var currentEncoding = frameEncoding == null ? DefaultEncoding : frameEncoding;

			// reading 3 bytes Language
			//OriginalData

			var languageAsByteList = new List<byte>();
			languageAsByteList.Add(OriginalData[1]);
			languageAsByteList.Add(OriginalData[2]);
			languageAsByteList.Add(OriginalData[3]);
			var language = DefaultEncoding.GetString( languageAsByteList.ToArray() );

			var position = 4;
			var shortDesc = ParseFrameValueFromPosition(ref position,currentEncoding);		

			Value = ParseFrameValueFromPosition(ref position,currentEncoding);		
		}

		private void ParseImageData()
		{
			/* 
				viz http://id3.org/id3v2.3.0

				<Header for 'Attached picture', ID: "APIC">
				Text encoding   $xx
				MIME type       <text string> $00
				Picture type    $xx
				Description     <text string according to encoding> $00 (00)
				Picture data    <binary data>
			*/

			if (OriginalData.Length<4)
				return;

			var frameEncoding = GetFrameHeaderEncoding();
			var currentEncoding = frameEncoding == null ? DefaultEncoding : frameEncoding;

			var position = 1;
			ImgMime = ParseFrameValueFromPosition(ref position,DefaultEncoding);		

			var pictureType = OriginalData[position];
			ImgType = (ImageType)pictureType;

			position++;

			ImgDescription =  ParseFrameValueFromPosition(ref position,frameEncoding);	

			// reading image bytes

			var imgBytes = new List<byte>();
			while (position<OriginalData.Length)
			{
				imgBytes.Add(OriginalData[position]);
				position++;
			}

			var ms = new MemoryStream(imgBytes.ToArray());
        	ImageData = System.Drawing.Image.FromStream(ms);
		}		

		private bool ParseDataToValue(bool throwExceptions = false)
		{
			try
			{
				Value = String.Empty;

				if (Size<1)
				{
					return false;
				}

				switch (FrameType)
				{
					case FrameTypeEnum.Picture: ParseImageData(); return true; 
					case FrameTypeEnum.Comment: ParseCommentData(); return true; 
					case FrameTypeEnum.Lyrics: ParseUnsynchronisedLyricsText(); return true; 
					default: ParseTextFrameData(); return true;
				}							

			} catch (Exception ex)
			{
				Logger.Logger.WriteToLog("Error parsing frame",ex);
				if (throwExceptions) throw new Exception("Error parsing frame",ex);
				return false;
			}
		}

		#endregion

		#region static - common methods

		public static string ByteArrayToDecimalStrings(byte[] ar)
		{
				var arrayAsByteString = String.Empty;
				for (var j=0;j<ar.Length;j++) arrayAsByteString+= ar[j].ToString()+" ";					

				return arrayAsByteString;
		}

		public static byte[] MakeID3v2SizeAsByteArray(long num, byte bitRightCount=7)
		{
			var size = new byte[4];

			for (var i=3;i>=0;i--)
			{
				size[i] = Convert.ToByte(num & 127);
				num = num >> bitRightCount;
			}

			return size;
		}

		public static long MakeID3v2Size(byte[] byteArray, byte bitLeftCount=7)
		{
			if (byteArray.Length!=4)
				return -1;

			  // The ID3v2 tag size is encoded with four bytes where the most significant bit (bit 7) is set to zero in every byte, 
			  // making a total of 28 bits. The zeroed bits are ignored.
			long res = 0;

				res = byteArray[0];
				res = res << bitLeftCount;

				res+=byteArray[1];
				res = res << bitLeftCount;

				res+=byteArray[2];
				res = res << bitLeftCount;

				res+=byteArray[3];

				return res;
		}

		#endregion

		#region flags

		public bool FlagDiscardAfterTagChange
		{
			get
			{
				if (VersionMajor<=3) return (Flags[0] & 128) == 128;
				else return (Flags[0] & 64) == 64;
			}
		}

		public bool FlagDiscardAfterFileChange
		{
			get
			{
				if (VersionMajor<=3) return (Flags[0] & 64) == 64;
				else return (Flags[0] & 32) == 32;
			}
		}

		public bool FlagReadOnly
		{
			get
			{
				if (VersionMajor<=3) return (Flags[0] & 32) == 32;
				else return (Flags[0] & 16) == 16;
			}
		}

		public bool FlagCompressed
		{
			get
			{
				// Frame is compressed using [#ZLIB zlib] with 4 bytes for 'decompressed size' appended to the frame header
				if (VersionMajor<=3) return (Flags[1] & 128) == 128;
				else return (Flags[1] & 8) == 8;
			}
		}

		public bool FlagEncrypted
		{
			get
			{
				return (Flags[1] & 64) == 164;

				if (VersionMajor<=3) return (Flags[64] & 64) == 64;
				else return (Flags[4] & 4) == 4;
			}
		}

		public bool FlagGrouping
		{
			get
			{
				if (VersionMajor<=3) return (Flags[1] & 32) == 32;
				else return (Flags[1] & 64) == 64;
			}
		}

		public bool FlagUnsynchronisation
		{
			get { return (VersionMajor>3) && ((Flags[1] & 2) == 2); }
		}

		public bool FlagDataLengthIndicatorPresent
		{
			get { return (VersionMajor>3) && ((Flags[1] & 1) == 1); }
		}

		#endregion

		#region properties

		#region image

		public Image ImageData 
		{
			get { return _img; }
			set { _img = value; }
		}

		public string ImgDescription 
		{
			get { return _imgDescription;	}
			set { _imgDescription = value; }
		}

		public string ImgMime 
		{
			get { return _imgMime; }
			set { _imgMime = value; }
		}

		public ImageType ImgType 
		{
			get { return _imgType; }
			set { _imgType = value;}
		}
		#endregion

		public FrameTypeEnum FrameType
		{
			get 
			{
				if (Name=="APIC")
				{
					return FrameTypeEnum.Picture;
				}

				if (Name=="COMM")
				{
					return FrameTypeEnum.Comment;
				}

				if (Name == "USLT")
				{
					return FrameTypeEnum.Lyrics;
				}

				if (TAGBase.FrameNamesDictionary.ContainsValue(Name))
				{
					return FrameTypeEnum.CommonText;
				}

				return FrameTypeEnum.Unsupported;
			}
		}

		public byte VersionMajor 
		{
			get { return _versionMajor;	}
			set {_versionMajor = value;	}
		}

		public Encoding DefaultEncoding 
		{
			get { return _defaultEncoding; }
			set {_defaultEncoding = value; }
		}	

		public byte[] OriginalFrameHeader 
		{
			get 
			{
				return _originalFrameHeader;
			}
			set 
			{
				_originalFrameHeader = value;
			}
		}

		public byte[] OriginalData 
		{
			get { return _originalData;}
			set { _originalData = value;}
		}

		public bool IsValid
		{
			get
			{
				var valid = 0;

				for (var i=0;i<=3;i++)
				{
					// validating name
					var asciiCode = OriginalFrameHeader[i];
					if (
							(asciiCode>=65 && asciiCode<=90) ||  // A..Z
							(asciiCode>=48 && asciiCode<=57)	 // 0..9
						)
					{
						valid++;
					}
				}

				if ( (Flags[0] & 31) == 0) valid++;
				if ( (Flags[1] & 31) == 0) valid++;

				return valid == 6;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
			   
		}

		public string Value 
		{
			get 
			{
				return _value;
			}
			set 
			{
				_value = value;
			}
		}

		public byte[] Flags 
		{
			get {return _flags;}
			set {_flags = value;}
		}

		public long Size 
		{
			get {return _size;}
			set {_size = value;}
		}

		#endregion
	}
}

