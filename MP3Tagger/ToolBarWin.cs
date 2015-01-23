using System;
using System.Collections.Generic;

namespace MP3Tagger
{
	public partial class ToolBarWin : Gtk.Window
	{
		public MainWindow MainWin { get; set; }
		private List<Gtk.ToolButton> LngButtons { get; set; }

		public enum KindEnum
		{
			Add = 0,
			Remove = 1,
			Languages = 2,
			Selection = 3
		}

		public void Show(KindEnum kind)
		{
			// hidding all action buttons (made from GUI) except Close
			foreach (var item in toolbar.AllChildren)
			{
				if (item is Gtk.ToolButton)
				{
					if ((item as Gtk.ToolButton).Name != "actionClose")
					{
						if ((item as Gtk.ToolButton).Action != null)
						{
							(item as Gtk.ToolButton).Action.Visible = false;
						} 
					}
				}
			}
			// hidding language buttons
			foreach (var btn in LngButtons) btn.Visible = false;

			switch (kind)
			{
				case KindEnum.Add:

						actionAddSingleFile.Visible = true;
						actionAddFolder.Visible = true;
						actionAddFolderRecursive.Visible = true;
						//actionRemoveAll.Visible = false;
						//actionRemoveSelected.Visible = false;
					break;
				case KindEnum.Remove:
						//actionAddFile.Visible = false;
						//actionAddFolder.Visible = false;
						actionRemoveAll.Visible = true;
						actionRemoveSelected.Visible = true;
					break;
				case KindEnum.Languages:
						foreach (var btn in LngButtons) btn.Visible = true;
					break;
				case KindEnum.Selection:
						actionSelectAll.Visible = true;
						actionUnselectAll.Visible = true;
					break;
			}


			
			Show();
		}

		private void CreateLanguageButtons()
		{
			var availableLanguages = Language.AvailableLanguages;
			foreach (var lng in availableLanguages)
			{
				Gtk.ToolButton button = new Gtk.ToolButton (Gtk.Stock.SelectFont);
				button.Visible = true;
				button.Label = lng.Description;
				button.Clicked += OnChangeLanguage;
				button.Data["Flag"] = lng.Flag;
				toolbar.Insert (button, 0); 
				LngButtons.Add(button);
			}
		}

	  	public ToolBarWin (MainWindow mainWin) : base(Gtk.WindowType.Toplevel)
        {
            this.Build ();

			LngButtons = new List<Gtk.ToolButton>();
			CreateLanguageButtons();

			Show();

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

		protected void OnChangeLanguage(object sender, EventArgs e)
		{
			if ( (sender is Gtk.ToolButton) && ((sender as Gtk.ToolButton).Data.ContainsKey("Flag")))
			{
				Hide ();
				var flag  = Convert.ToString((sender as Gtk.ToolButton).Data["Flag"]);
				MainWin.LoadLanguage(flag);
			}
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
			actionSelectAll.ShortLabel = lng.Translate("SelectAll");
			actionUnselectAll.ShortLabel = lng.Translate("UnSelectAll");
			actionAddFolderRecursive.ShortLabel = lng.Translate("FolderRec");
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

		protected void OnActionSelectAllActivated (object sender, EventArgs e)
		{
			Hide ();
			MainWin.SelectAll();
		}

		protected void OnActionUnselectAllActivated (object sender, EventArgs e)
		{
			Hide ();
			MainWin.UnSelectAll();

		}

		protected void OnActionAddFolderREcursiveActivated (object sender, EventArgs e)
		{
			var dir = Dialogs.OpenDirectoryDialog(MainWin.Lng.Translate("ChooseDir"));
			if (dir != null)
			{
				Hide ();
				MainWin.AddFolder(dir,true);
			}
		}


	}
}

