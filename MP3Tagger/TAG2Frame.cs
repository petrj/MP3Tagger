using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Logger;

namespace MP3Tagger
{

	public class TAG2Frame
	{
		public static byte HeaderByteLength = 10;

		#region private fields

		private Encoding _defaultEncoding = Encoding.GetEncoding("iso-8859-1");
		private static List<string> SupportedFrames = new List<string> () {"TIT2"};

		private bool _frameSupported = false;

		private byte[] _originalFrameHeader;			
		private byte _versionMajor;			

		private byte[] _flags  = new byte[] {0,0};					

		private long _size  = 0;
		private byte[] _originalData;
		private string _value;
		private string _name;

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

					//for (var i=0;i<4;i++) FrameID[i] = System.Text.Encoding.ASCII.GetString( new byte[] {frameHeader[i]}).ToCharArray()[0];
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

		private void ParseDataToValue()
		{

			Value = String.Empty;
			_frameSupported = false;

			if (Size<1)
			{
				return;
			}

			if (Name=="APIC")
			{
				return; // APIC not supported yet
			}

			bool firstByteEncodingFlag = false;;
			if (
					(Name.StartsWith("T")) || 	//  User defined text information frame
					(Name.StartsWith("W")) ||	//	User defined URL link frame
					(Name == "USLT") ||			//  Unsynchronised lyrics/text transcription
					(Name == "SYLT") ||			//  Synchronised lyrics/text
					(Name == "COMM") ||			//  Comment
					(Name == "USER") ||			//  Terms of use frame
					(Name == "OWNE") ||			//  
					(Name == "COMR") ||			//  
					(Name == "APIC")  			//  
				)
			{
				firstByteEncodingFlag = true;
			}

			var currentEncoding = DefaultEncoding;

			List <byte> dataBytes = new List <byte>();

				for (var i=0;i<Size;i++)
					{
						if ( (i==0) && (firstByteEncodingFlag))
						{					

						/*
						$00 – ISO-8859-1 (LATIN-1, Identical to ASCII for values smaller than 0x80).
						$01 – UCS-2 (UTF-16 encoded Unicode with BOM), in ID3v2.2 and ID3v2.3.
						$02 – UTF-16BE encoded Unicode without BOM, in ID3v2.4.
						$03 – UTF-8 encoded Unicode, in ID3v2.4.
						*/

							var encodingByte = OriginalData[i];
							switch (encodingByte)
							{
								case 0:currentEncoding = DefaultEncoding; break;
								case 1:currentEncoding = System.Text.Encoding.Unicode; break;
								case 2:currentEncoding = System.Text.Encoding.Unicode; break;
								case 3:currentEncoding = System.Text.Encoding.UTF8; break;
								default:throw new Exception(String.Format("Unknown frame encoding:{0}",encodingByte));	
							}							
							
							continue;
						}

						if (i>0)
						{
							if (currentEncoding == System.Text.Encoding.Unicode &&
					    		i >=2 &&
						    		(
										(
								    		OriginalData[i] == 0 &&
								    		OriginalData[i-1] == 0
										) 
									)
					    		)
								{
									dataBytes.RemoveAt( dataBytes.Count-1 );
									break;
								}

							if (currentEncoding == DefaultEncoding &&
					    		OriginalData[i] == 0)
								{
									break;
								}
						}

						dataBytes.Add(OriginalData[i]);
					}

			if (dataBytes.Count > 0)
			{
				if (currentEncoding == Encoding.Unicode &&
				    dataBytes.Count>=2 &&
				    (
					    (
					    	dataBytes[dataBytes.Count-1] == 255 &&
					    	dataBytes[dataBytes.Count-2] == 254
						) ||
					    (
					    	dataBytes[dataBytes.Count-1] == 254 &&
					    	dataBytes[dataBytes.Count-2] == 255
						)				    				    
				    )
				   )
				{
					// removing unicode BOM 
					dataBytes.RemoveRange(0,2);
				}

				Value = currentEncoding.GetString( dataBytes.ToArray() );

				if (Name.StartsWith("T")) _frameSupported = true;
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

		public static long MakeID3v2Size(byte[] byteArray, int bitLeftCount=7)
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

		public bool FrameSupported 
		{
			get 
			{
				return _frameSupported;
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

				if (valid != 6)
				{
					var  t= 0;
				}

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

