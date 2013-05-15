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

        public Song()
        {
            ID3v1 = new TAGID3v1();
			ID3v2 = new TAGID3v2();
			FileName = null;
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

