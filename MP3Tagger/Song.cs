using System;
using System.IO;
using System.Collections.Generic;
using MP3Tagger;
using Logger;

namespace MP3Tagger
{
    public class Song
    {
        public TAGID3v1 ID3v1 { get; set; }
		public TAGID3v2 ID3v2 { get; set; }
        public string FileName { get; set; }
		public int Index { get; set; }

		#region private fields

		private bool _ID3v1ActiveAfterLoad = false;
		private bool _ID3v2ActiveAfterLoad = false;
		private long _ID3v2TotalByteLengthAfterLoad = 0;

		#endregion

        public Song()
        {
            ID3v1 = new TAGID3v1();
			ID3v2 = new TAGID3v2();
			FileName = null;
			Index = 0;
        }

		public void Clear()
		{
			FileName = null;
			ID3v1.Clear();
			ID3v2.Clear();
		}

		#region properties

		public long ID3v2TotalByteLengthAfterLoad 
		{
			get { return _ID3v2TotalByteLengthAfterLoad;	}
		}

		public bool ID3v1ActiveAfterLoad 
		{
			get { return _ID3v1ActiveAfterLoad;	}
		}

		public bool ID3v2ActiveAfterLoad 
		{
			get { return _ID3v2ActiveAfterLoad;	}
		}

		public bool Changed
		{
			get
			{
				return ID3v1.Changed || ID3v2.Changed;
			}
		}

		#endregion

		#region public methods

		public bool SaveChanges(bool throwExceptions=false)
		{
			try
			{
				var tmpFileName = Path.GetTempFileName();
				if (File.Exists(tmpFileName)) File.Delete(tmpFileName);
				if (SaveAs(tmpFileName))
				{
					File.Delete(FileName);
					File.Move(tmpFileName,FileName);
				}

			} catch (Exception ex)
			{
				Logger.Logger.WriteToLog(String.Format("Error while saving {0}",FileName),ex);
				if (throwExceptions) throw;
				return false;
			}

			return true;
		}

		public string UnMask(string mask)
		{
				string fName = System.IO.Path.GetFileName(FileName);
				string name = System.IO.Path.GetFileNameWithoutExtension(FileName);
				string ext = System.IO.Path.GetExtension(FileName);
				while ((ext != null) && (ext.StartsWith("."))) { ext = ext.Substring(1); };

				/*

					?f - FileName
					?e - extension (without .)

					?t - title
					?i - interpret
					?a - album
					?t - track number
					?y - year
					?p - position number in current list

				*/

				var fNameUnMasked = mask;

				fNameUnMasked = fNameUnMasked.Replace("*f",fName);
				fNameUnMasked = fNameUnMasked.Replace("*e",ext);
				fNameUnMasked = fNameUnMasked.Replace("*n",name);
				fNameUnMasked = fNameUnMasked.Replace("*2",(Index+1).ToString().PadLeft(2,'0'));
				fNameUnMasked = fNameUnMasked.Replace("*3",(Index+1).ToString().PadLeft(3,'0'));

				TAGBase tag = null;
				if (ID3v1.Active) tag = ID3v1; 
				else if (ID3v2.Active) tag = ID3v2; 

				if (tag != null)
				{
					fNameUnMasked = fNameUnMasked.Replace("*t",tag.Title);
					fNameUnMasked = fNameUnMasked.Replace("*i",tag.Artist);
					fNameUnMasked = fNameUnMasked.Replace("*a",tag.Album);
					fNameUnMasked = fNameUnMasked.Replace("*t",tag.TrackNumber.ToString());
					fNameUnMasked = fNameUnMasked.Replace("*y",tag.Year.ToString());					
				}

			return fNameUnMasked;
		}

		public bool RenameByMask(string mask, bool throwExceptions=false)
		{
			try
			{
				if (String.IsNullOrEmpty(mask))
					return false;

				if (!File.Exists(FileName))
					throw new FileNotFoundException(FileName);

				string fName = System.IO.Path.GetFileName(FileName);

				if (fName == mask)
					return false;

				var fNameUnMasked = UnMask(mask);

				if (fNameUnMasked == fName)
					return false;

				string newName =Path.Combine(System.IO.Path.GetDirectoryName(FileName), fNameUnMasked);

				Logger.Logger.WriteToLog(String.Format("Renaming {0} to {1} by mask: {2}",FileName,newName,mask));

				File.Move(FileName,newName);

				FileName = ID3v1.FileName = ID3v2.FileName = newName;
			
			} catch (Exception ex)
			{
				Logger.Logger.WriteToLog(String.Format("Error while renaming {0}",FileName),ex);
				if (throwExceptions) throw;
				return false;
			}

			return true;
		}

		public bool SaveAs(string saveFileName, bool throwExceptions=false)
		{
			Logger.Logger.WriteToLog(String.Format("Saving {0}",saveFileName));

			try
			{
				if (File.Exists(saveFileName))
					throw new FieldAccessException(String.Format("File {0} already exists.",saveFileName));

				// reading originale file
				using (var frs = new FileStream(FileName,FileMode.Open))
				{
					var originalDataByteLength = frs.Length;
					if (ID3v1ActiveAfterLoad) originalDataByteLength = originalDataByteLength - ID3v1.HeaderByteLength;
					if (ID3v2ActiveAfterLoad) originalDataByteLength = originalDataByteLength - ID3v2TotalByteLengthAfterLoad;

					// reading original binary data

					if (ID3v2ActiveAfterLoad) frs.Seek(ID3v2TotalByteLengthAfterLoad,0);
					var data = new byte[originalDataByteLength];
					frs.Read(data,0,Convert.ToInt32(originalDataByteLength));

					// writing to stream
					using (var fs = new FileStream(saveFileName,FileMode.CreateNew))
					{
						if (ID3v2.Active) ID3v2.SaveToStream(fs,throwExceptions);
						fs.Write(data,0,Convert.ToInt32(originalDataByteLength));
						if (ID3v1.Active) ID3v1.SaveToStream(fs,throwExceptions);

						fs.Close();
					}

					frs.Close();
				}

				ID3v1.Changed = false;
				ID3v2.Changed = false;

			
			} catch (Exception ex)
			{
				Logger.Logger.WriteToLog(String.Format("Error while saving {0}",saveFileName),ex);
				if (throwExceptions) throw;
				return false;
			}

			return true;
		}

		public bool OpenFile(string fileName, bool throwExceptions=false)
		{
			Logger.Logger.WriteToLog(String.Format("Opening file {0}",fileName));

			try
			{
				if (!File.Exists(fileName))
					throw new FileNotFoundException();

				FileName = fileName;

				using (var fs = new FileStream(fileName,FileMode.Open))
				{
					ID3v1.ReadFromStream(fs,throwExceptions);
					ID3v2.ReadFromStream(fs,throwExceptions);

					_ID3v1ActiveAfterLoad = ID3v1.Active;
					_ID3v2ActiveAfterLoad = ID3v2.Active;
					_ID3v2TotalByteLengthAfterLoad = ID3v2.TotalByteLength;

					fs.Close();
				}

				ID3v1.Changed = false;
				ID3v2.Changed = false;

			
			} catch (Exception ex)
			{
				Logger.Logger.WriteToLog(String.Format("Error while reading {0}",fileName),ex);
				if (throwExceptions) throw;
				return false;
			}

			return true;
		}
	
		public bool Reload(bool throwExceptions=false)
		{
			Logger.Logger.WriteToLog(String.Format("Reloading file {0}",FileName));

			return OpenFile(FileName,throwExceptions);
		}
		
		#endregion
	}
}

 