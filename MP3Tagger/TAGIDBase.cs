using System;

namespace MP3Tagger
{
	public class TAGIDBase
	{
		public bool Active { get; set; }
		private string _fileName;

		public TAGIDBase ()
		{
			_fileName = null;
			Active = false;
		}

		public string FileName 
		{
			get 
			{
				return _fileName;
			}
			set {
				_fileName = value;
			}
		}

	}
}

