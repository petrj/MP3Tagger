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

		public bool OpenFile(string fileName)
		{
			if (!File.Exists(fileName))
				throw new FileNotFoundException();

			FileName = fileName;				

			try
			{		
				ID3v1.ReadFromFile(fileName,true);

			} catch (Exception ex)
			{
				Logger.Logger.WriteToLog("Error while reading MP3 tag.",ex);
			}

			try
			{		
				ID3v2.ReadFromFile(fileName,true);

			} catch (Exception ex)
			{
				Logger.Logger.WriteToLog("Error while reading MP3 v2 tag.",ex);
			}


			return true;
		}
	}
}

