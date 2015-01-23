using System;

namespace MP3Tagger
{
	public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);

	public class ProgressEventArgs : EventArgs
	{
	    public double Percent { get; private set; }

	    private ProgressEventArgs() {}

	    public ProgressEventArgs(double percent)
	    {
	        Percent = percent;
	    }
	}
}

