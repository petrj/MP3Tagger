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
		private TreeViewData _framesTreeViewData;

		private int lastX = -1;
		private int lastY = -1;

		#endregion

		#region public fields

		public MainWindow MainWin { get; set; } 

		public SongDetail (MainWindow parent) : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build ();

			this.Shown+= OnShown;
			this.WidgetEvent+=OnWidgetEvent;

			this.
			MainWin = parent;

			_framesTreeViewData = new TreeViewData(treeViewFrames);

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

		public void ApplyLanguage()
		{		
			var lng = MainWin.Lng;

			// toollbar
			applyAction.ShortLabel = lng.Translate("OK");
			undoAction.ShortLabel = lng.Translate("Undo");
			goBackAction.ShortLabel = lng.Translate("Previous");
			goForwardAction.ShortLabel = lng.Translate("Next");
			closeAction1.ShortLabel = lng.Translate("Close");

			checkButtonID31Active.Label = lng.Translate("Tag1");
			checkButtonID32Active.Label = lng.Translate("Tag2");

			labelFName.LabelProp = lng.Translate("File");

			labelTAG1.LabelProp = lng.Translate("Tag1");
			labelTAG2.LabelProp = lng.Translate("Tag2");

			labelTAG2Image.LabelProp = lng.Translate("Tag2Images");
			labelTAG2Frames.LabelProp = lng.Translate("Tag2Frames");

			buttonSetFrontCoverImage.Label = lng.Translate("ChooseImage");

			tagWidget1.ApplyLanguage(lng);
			tagWidget2.ApplyLanguage(lng);
		}

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
						image.Pixbuf = ImageToPixbuf(imgFrame.FrameImage.ImageData);
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
				//comboboxentryFileName.Active = 0;

				var fileNameListStore = new Gtk.ListStore (typeof(string));            	
				fileNameListStore.AppendValues("");
				fileNameListStore.AppendValues("*i - *t.mp3");
				fileNameListStore.AppendValues("*y-*a-*t.mp3");

				int activeIndex;
				if (CurrentSong.FileName != null && System.IO.File.Exists(CurrentSong.FileName))			   
				{
					Title = System.IO.Path.GetFileName(CurrentSong.FileName);
					fileNameListStore.AppendValues(Title);
					activeIndex = 3;
				} else
				{
					Title = String.Format(MainWin.Lng.Translate("Edit"));
					activeIndex = 0;
				}

				comboboxentryFileName.Model = fileNameListStore;							 
				comboboxentryFileName.Active = activeIndex;

				checkButtonID31Active.Active = CurrentSong.ID3v1.Active;
				checkButtonID32Active.Active = CurrentSong.ID3v2.Active;

				tagWidget1.Tag = CurrentSong.ID3v1 as TAGBase;
				tagWidget2.Tag = CurrentSong.ID3v2 as TAGBase;

				FillFrames(CurrentSong.ID3v2);

				FillImage(imageCoverFront,ImageType.CoverFront);

				fixedTAG1.Sensitive = labelTAG1.Sensitive = (CurrentSong.ID3v1 as TAGBase).Active;

				fixedTAG2.Sensitive =  fixedTAG2Frames.Sensitive =  fixedImages.Sensitive =
				labelTAG2.Sensitive = labelTAG2Frames.Sensitive = labelTAG2Image.Sensitive = 
					(CurrentSong.ID3v2 as TAGBase).Active;

				Show();
			}
		}

		private void FillFrames(TAGID3v2 TAG2)
		{
			if (_framesTreeViewData.Columns.Count == 0)
			{
				_framesTreeViewData.AppendStringColumn(MainWin.Lng.Translate("Name"), null, false);
	            _framesTreeViewData.AppendStringColumn(MainWin.Lng.Translate("Value"), null, false);
				_framesTreeViewData.CreateTreeViewColumns();
			}

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
			if (lastX == -1 || lastY == -1)
			{
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

				lastX = xMainWin+(wMainWin-w)/2;
				lastY = yMainWin+(hMainWin-h)/2;			
			}

			Move(lastX,lastY);
		}

		protected void OnWidgetEvent(object sender, EventArgs e)
		{
			if (lastX == -1 || lastY == -1 || !Visible )
				return;

			if (e is WidgetEventArgs)
			{
				if ((e as WidgetEventArgs).Args.Length>0)
				{
					if ((e as WidgetEventArgs).Args[0] is Gdk.EventConfigure)
					{
						GetPosition(out lastX,out lastY);
					}
				}
			}
		}

		protected void OnCheckButtonID32ActiveClicked (object sender, EventArgs e)
		{
			if (CurrentSong != null)
			{			 
				CurrentSong.ID3v2.Active = checkButtonID32Active.Active;
				FillAll();
			}
		}

		#region toolbar action events

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

			MainWin.ApplySongEdit(comboboxentryFileName.ActiveText);
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

		#endregion

		protected void OnButtonSetFrontCoverImageClicked (object sender, EventArgs e)
		{
			var fileName = Dialogs.OpenFileDialog(MainWin.Lng.Translate("ChooseImage"));
			if (fileName != null)
			{
				CurrentSong.ID3v2.LoadImageFrameFromFile(fileName,ImageType.CoverFront,false);
				FillImage(imageCoverFront,ImageType.CoverFront);
			}
		}


		protected void OnButtonMaskHelpActivated (object sender, EventArgs e)
		{

		}

		protected void OnButtonMaskHelpClicked (object sender, EventArgs e)
		{
			var firstSelectedSong = MainWin.FirstSelectedSong;
			if (firstSelectedSong == null)
				return;

			var help = MainWin.Lng.Translate("FileNameByMask");
			help += firstSelectedSong.UnMask(comboboxentryFileName.ActiveText);

			help += System.Environment.NewLine;
			help += System.Environment.NewLine;
			help += MainWin.Lng.Translate("MaskHelp");

			Dialogs.InfoDialog(help);
		}
		#endregion
	}
}

