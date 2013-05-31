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

				ID3v1.Changed = false;
				ID3v2.Changed = false;

			} catch (Exception ex)
			{
				Logger.Logger.WriteToLog(String.Format("Error while saving {0}",FileName),ex);
				if (throwExceptions) throw;
				return false;
			}

			return true;
		}

		public bool SaveAs(string saveFileName, bool throwExceptions=false)
		{
			try
			{
				if (File.Exists(saveFileName))
					throw new FieldAccessException(String.Format("File {0} already exists.",saveFileName));

				// reading originale file
				using (var frs = new FileStream(FileName,FileMode.Open))
				{
					var dataByteLength = Convert.ToInt32(frs.Length);
					if (ID3v1.Active) dataByteLength = dataByteLength - ID3v1.HeaderByteLength;
					if (ID3v2.Active) dataByteLength = dataByteLength - ID3v2.TotalByteLength;

					// reading original pbinary data

					if (ID3v2.Active) frs.Seek(ID3v2.TotalByteLength,0);
					var data = new byte[dataByteLength];
					frs.Read(data,0,dataByteLength);

					using (var fs = new FileStream(saveFileName,FileMode.CreateNew))
					{
						if (ID3v2.Active) ID3v2.SaveToStream(fs,throwExceptions);
						fs.Write(data,0,dataByteLength);
						if (ID3v1.Active) ID3v1.SaveToStream(fs,throwExceptions);

						fs.Close();
					}

					frs.Close();
				}
			
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
			try
			{
				if (!File.Exists(fileName))
					throw new FileNotFoundException();

				FileName = fileName;

				using (var fs = new FileStream(fileName,FileMode.Open))
				{
					ID3v1.ReadFromStream(fs,throwExceptions);
					ID3v2.ReadFromStream(fs,throwExceptions);

					fs.Close();
				}
			
			} catch (Exception ex)
			{
				Logger.Logger.WriteToLog(String.Format("Error while reading {0}",fileName),ex);
				if (throwExceptions) throw;
				return false;
			}

			return true;
		}
	}
}

 