using System;
using System.Collections.Generic;

namespace MP3Tagger
{
	public partial class SongDetail : Gtk.Window
	{
		private List<Song> _songs = new List<Song>();
		private Song _currentSong;
		private bool _shownFirst = true;

		public MainWindow MainWin { get; set; } 

		public SongDetail () : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build ();

			this.Shown+= OnShown;
		}

		public Song CurrentSong 
		{
			get { return _currentSong; }
			set 
			{ 
				_currentSong = value;
				FillAll();
			}
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

		private void FillAll()
		{
			if (CurrentSong != null)
			{
				tagwidget1.Tag = CurrentSong.ID3v1 as TAGBase;
			}
		}

		protected void OnShown(object sende, EventArgs e)
		{
			//if (_shownFirst)
			//{
				// center
				int xMainWin;
				int yMainWin;
				int wMainWin;
				int hMainWin;
				MainWin.GetPosition(out xMainWin,out yMainWin);
				MainWin.GetSize(out wMainWin,out hMainWin);

				int w;
				int h;
				GetSize(out w,out h);

				this.Move( xMainWin+(wMainWin-w)/2,yMainWin+(hMainWin-h)/2);
			//}

			_shownFirst = false;

		}

		protected void OnCancelActionActivated (object sender, EventArgs e)
		{
			this.Hide();
		}

		protected void OnApplyActionActivated (object sender, EventArgs e)
		{
			this.Hide();
		}	

		protected void OnGoBackActionActivated (object sender, EventArgs e)
		{
			MainWin.SelectPrev();
		}

		protected void OnGoForwardActionActivated (object sender, EventArgs e)
		{
			MainWin.SelectNext();
		}
		


	}
}

