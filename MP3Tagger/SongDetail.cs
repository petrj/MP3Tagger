using System;
using System.Collections.Generic;
using Grid;
using Gtk;

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

			_framesTreeViewData.AppendStringColumn("Name", null, false);
            _framesTreeViewData.AppendStringColumn("Value", null, false);
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

		// Eric Butler eric@extremeboredom.net
		// Thu May 12 04:34:38 EDT 2005
		// http://lists.ximian.com/pipermail/gtk-sharp-list/2005-May/005880.html)
		private Gdk.Pixbuf ImageToPixbuf (System.Drawing.Image image)
		{
			using (System.IO.MemoryStream stream = new System.IO.MemoryStream ()) {
				image.Save (stream, System.Drawing.Imaging.ImageFormat.Bmp);
				stream.Position = 0;
				return new Gdk.Pixbuf (stream);
			}
		}


		private void FillImage(Gtk.Image image, ImageType imgType, bool throwExceptions = false)
		{
			try
			{
				var imgFrame = CurrentSong.ID3v2.GetFrameByImageType(imgType);
					if (imgFrame != null)
					{				
						image.Pixbuf = ImageToPixbuf(imgFrame.ImageData);
					}
			}
			catch (Exception ex)
			{
				Logger.Logger.WriteToLog(String.Format("Error while reading image (type {0})",imgType),ex);
				if (throwExceptions) throw;
			}
		}


		private void FillAll()
		{
			if (CurrentSong != null)
			{
				if (CurrentSong.FileName != null && System.IO.File.Exists(CurrentSong.FileName))			   
				{
					Title = System.IO.Path.GetFileName(CurrentSong.FileName);
				}

				checkButtonID31Active.Active = CurrentSong.ID3v1.Active;
				checkButtonID32Active.Active = CurrentSong.ID3v2.Active;

				tagWidget1.Tag = CurrentSong.ID3v1 as TAGBase;
				tagWidget2.Tag = CurrentSong.ID3v2 as TAGBase;

				FillFrames(CurrentSong.ID3v2);

				FillImage(imageCoverFront,ImageType.CoverFront);

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
			/*
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
			*/
		}

		protected void OnCancelActionActivated (object sender, EventArgs e)
		{
			MainWin.FillTree();
			MainWin.SelectSong(CurrentSong);
		}

		protected void OnApplyActionActivated (object sender, EventArgs e)
		{
			if (CurrentSong != null)
			{
			    tagWidget1.ApplyChanges();
                tagWidget2.ApplyChanges();
			}

			MainWin.ApplySongEdit();
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

		protected void OnCloseAction1Activated (object sender, EventArgs e)		
		{
			this.Hide();
		}

		protected void OnButtonSetFrontCoverImageClicked (object sender, EventArgs e)
		{
			var fileName = Dialogs.OpenFileDialog("Choose image");
			if (fileName != null)
			{
				CurrentSong.ID3v2.LoadImageFromFile(fileName,ImageType.CoverFront,false);
				FillImage(imageCoverFront,ImageType.CoverFront);
			}
		}		

		protected void OnSaveAction1Activated (object sender, EventArgs e)
		{
			MainWin.SaveChanges();
		}


		protected void OnCheckButtonID32ActiveClicked (object sender, EventArgs e)
		{
			if (CurrentSong != null)
			{			 
				CurrentSong.ID3v2.Active = checkButtonID32Active.Active;

			}
		}
		#endregion
	}
}

