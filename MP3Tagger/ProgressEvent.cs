using System;

namespace MP3Tagger
{
	public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);

	public class ProgressEventArgs : EventArgs
	{
	    public double Percent { get; private set; }
        public string Text { get; private set; }

	    public ProgressEventArgs(double percent, string text)
	    {
	        Percent = percent;
	        Text = text;
	    }
	}
}

