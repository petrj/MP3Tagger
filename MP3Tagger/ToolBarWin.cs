using System;

namespace MP3Tagger
{
	public partial class ToolBarWin : Gtk.Window
	{
		public MainWindow MainWin { get; set; }

		public enum KindEnum
		{
			Add = 0,
			Remove = 1
		}

		public void Show(KindEnum kind)
		{
			// hidding all except Close
			foreach (var item in toolbar.AllChildren)
			{
				if (item is Gtk.ToolButton)
				{
					if ((item as Gtk.ToolButton).Name != "actionClose")
					{
						(item as Gtk.ToolButton).Action.Visible = false;
					}
				}
			}

			switch (kind)
			{
				case KindEnum.Add:

						actionAddSingleFile.Visible = true;
						actionAddFolder.Visible = true;
						//actionRemoveAll.Visible = false;
						//actionRemoveSelected.Visible = false;
					break;
				case KindEnum.Remove:
						//actionAddFile.Visible = false;
						//actionAddFolder.Visible = false;
						actionRemoveAll.Visible = true;
						actionRemoveSelected.Visible = true;
					break;
			}


			
			Show();
		}

	  	public ToolBarWin (MainWindow mainWin) : base(Gtk.WindowType.Toplevel)
        {
            this.Build ();
			MainWin = mainWin;
        }

		protected void OnCloseActionActivated (object sender, EventArgs e)
		{
			Hide ();
		}

		protected void OnShown(object sender, EventArgs e)
		{
			// center
				int xMainWin;
				int yMainWin;
				MainWin.GetPosition(out xMainWin,out yMainWin);
				this.Move( xMainWin+5,yMainWin+80);
		}

		protected void OnActionAddFileActivated (object sender, EventArgs e)
		{
			var songFileName = Dialogs.OpenFileDialog("ChooseMp3");
			if (songFileName != null)
			{
				Hide ();
				MainWin.AddFile(songFileName);
			}		
		}	
		
		protected void OnActionAddFolderActivated (object sender, EventArgs e)
		{
			var dir = Dialogs.OpenDirectoryDialog(MainWin.Lng.Translate("ChooseDir"));
			if (dir != null)
			{
				Hide ();
				MainWin.AddFolder(dir,false);
			}
		}


		public void ApplyLanguage()
		{		
			var lng = MainWin.Lng;

			Title = lng.Translate("Add");
		
			actionAddSingleFile.ShortLabel = lng.Translate("File");
			actionAddFolder.ShortLabel = lng.Translate("Folder");
			actionClose.ShortLabel = lng.Translate("Close");
			actionRemoveSelected.ShortLabel = lng.Translate("Selected");
			actionRemoveAll.ShortLabel = lng.Translate("All");
		}

		protected void OnActionRemoveSelectedActivated (object sender, EventArgs e)
		{
			Hide ();
			MainWin.Remove(true);
		}

		protected void OnActionRemoveAllActivated (object sender, EventArgs e)
		{
			Hide ();
			MainWin.Remove(false);
		}

	}
}

