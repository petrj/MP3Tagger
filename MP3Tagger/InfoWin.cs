using System;

namespace MP3Tagger
{
	public partial class InfoWin : Gtk.Window
	{
		public InfoWin () : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build ();

			// todo auto-assembly versioning 
		}

		protected void OnButtonOKClicked (object sender, EventArgs e)
		{
			Hide ();
		}

	}
}

