using System;
using System.Collections.Generic;

namespace MP3Tagger
{
	public partial class SongDetail : Gtk.Window
	{
		List<Song> _songs = new List<Song>();

		public SongDetail () : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build ();

			this.Shown+= OnShown;
		}

		public List<Song> Songs 
		{
			get { return _songs; }
			set { _songs = value; }
		}

		protected void OnCloseActionActivated (object sender, EventArgs e)
		{
			this.Hide();
		}

		protected void OnShown(object sende, EventArgs e)
		{
		
		}		

		protected void OnCancelActionActivated (object sender, EventArgs e)
		{
			this.Hide();
		}

		protected void OnApplyActionActivated (object sender, EventArgs e)
		{
			this.Hide();
		}

	}
}

