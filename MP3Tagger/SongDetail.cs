using System;
using System.Collections.Generic;
using Grid;

namespace MP3Tagger
{
	public partial class SongDetail : Gtk.Window
	{
		#region private fields

		private List<Song> _songs = new List<Song>();
		private Song _currentSong;
		private bool _shownFirst = true;
		private TreeViewData _framesTreeViewData;

		#endregion

		#region public fields

		public MainWindow MainWin { get; set; } 

		public SongDetail (MainWindow parent) : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build ();

			this.Shown+= OnShown;
			MainWin = parent;

			_framesTreeViewData = new TreeViewData(treeViewFrames);

			_framesTreeViewData.AppendStringColumn("Name");
			_framesTreeViewData.AppendStringColumn("Value");
			_framesTreeViewData.CreateTreeViewColumns();

		}

		#endregion

		#region properties

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

		#endregion

		#region methods

		private void FillAll()
		{
			if (CurrentSong != null)
			{
				checkButtonID31Active.Active = CurrentSong.ID3v1.Active;
				checkButtonID32Active.Active = CurrentSong.ID3v2.Active;

				tagWidget1.Tag = CurrentSong.ID3v1 as TAGBase;
				tagWidget2.Tag = CurrentSong.ID3v2 as TAGBase;

				FillFrames(CurrentSong.ID3v2);

				Show();
			}
		}

		private void FillFrames(TAGID3v2 TAG2)
		{
			_framesTreeViewData.Data.Clear();
			foreach (var frame in TAG2.Frames)
			{
				_framesTreeViewData.AppendData( new List<object> {frame.Name,frame.Value} );
			}

			treeViewFrames.Model = _framesTreeViewData.CreateTreeViewListStore();
		}

		#endregion

		#region events

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

		protected void OnCloseActionActivated (object sender, EventArgs e)
		{
			this.Hide();
		}
		
		#endregion

	}
}
