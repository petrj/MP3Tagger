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

	public class TAG2Image
	{
		private string _imgMime;
		private string _imgDescription;
		private Image _img;
		private ImageType _imgType;

		public void CopyTo(TAG2Image img)
		{
			img.ImageData = ImageData;
			img.ImgDescription = ImgDescription;
			img.ImgMime = ImgMime;
			img.ImgType = ImgType;
		}

		// http://programcsharp.com/blog/archive/2008/01/17/Get-the-MIME-type-of-a-System.Drawing-Image.aspx

	    public static string GetMimeType(Image i)
	    {
	        foreach (System.Drawing.Imaging.ImageCodecInfo codec in System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders())
	        {
	            if (codec.FormatID == i.RawFormat.Guid)
	                return codec.MimeType;
	        }

	        return "image/unknown";
	    }

		public void LoadFromFile(string fileName)
		{
				ImageData = Image.FromFile(fileName);
				ImgMime = GetMimeType(ImageData);
		}

		#region properties

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
	}
}

