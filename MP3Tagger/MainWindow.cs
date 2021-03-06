using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Grid;
using Gtk;
using MP3Tagger;
using Logger;

public partial class MainWindow: Gtk.Window
{
	#region private fields

    private SongList _songList = new SongList();
	private MP3Tagger.ProgressBarWindow progressWin;
	private SongDetail editWindow;
	private ToolBarWin toolBarWin;
	private InfoWin infoWin;
	private Song _multiSelectSong;

	private TreeViewData _treeView1Data;
	private TreeViewData _treeView2Data;

	public Language Lng { get; set ; }

	#endregion

	#region public fields

	public string[] _args;

	#endregion
   
	#region init

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Logger.Logger.WriteToLog("Starting new application instance");

		Build ();

		Lng = new Language();

		_treeView1Data = new TreeViewData(tree); 
		_treeView2Data = new TreeViewData(tree2); 

		progressWin = new MP3Tagger.ProgressBarWindow(this);
		progressWin.Hide();

		EditingModeActive = false;
		ActualSelectionMode = SelectionMode.Multiple;

		CreateGridColumns();

		_multiSelectSong = new Song();

		editWindow = new SongDetail(this);
		editWindow.Hide();

		toolBarWin = new ToolBarWin(this);
		toolBarWin.Hide();

		infoWin = new InfoWin();
		infoWin.Hide();


		tree.Selection.Changed+=new EventHandler(OnSelectionChanged);
		tree2.Selection.Changed+=new EventHandler(OnSelectionChanged);

        tree.ButtonPressEvent += tree_ButtonPressEvent;
		tree2.ButtonPressEvent += tree_ButtonPressEvent;
			
		this.Show();
	}

	public void LoadLanguage(string flag)
	{
		Lng.LoadByFlag(flag);
		ApplyLanguage();
		editWindow.ApplyLanguage();
		toolBarWin.ApplyLanguage();

		CreateGridColumns();
		FillTree();
	}

	public void ApplyConfiguration()
	{
		// default language
		if (String.IsNullOrEmpty(Configuration.DefaultLanguage))
		{
			LoadLanguage("en");
		} else
		{
			LoadLanguage(Configuration.DefaultLanguage);
		}

		MultiSelectSong.ID3v1.DefaultEncoding = Encoding.GetEncoding(Configuration.DefaultTAG1Encoding);
	}

	private void ApplyLanguage()
	{
		// toollbar

		addAction.ShortLabel = addAction.Tooltip = Lng.Translate("Add");
		removeAction.ShortLabel = removeAction.Tooltip = Lng.Translate("Remove");
		selectSingleAction.ShortLabel = selectSingleAction.Tooltip = Lng.Translate("Single");
		selectMultipleAction.ShortLabel = selectMultipleAction.Tooltip = Lng.Translate("Multi");
		editAction.ShortLabel = editAction.Tooltip = Lng.Translate("Edit");
		editingModeAction.ShortLabel = editingModeAction.Tooltip= Lng.Translate("Write");
		saveAction.ShortLabel = saveAction.Tooltip = Lng.Translate("Save");
		selectAction.ShortLabel = selectAction.Tooltip = Lng.Translate("Select");
		closeAction.ShortLabel = closeAction.Tooltip = Lng.Translate("Close");
		goForwardAction.ShortLabel = goForwardAction.Tooltip = Lng.Translate("Next");
		goBackAction.ShortLabel = goBackAction.Tooltip = Lng.Translate("Previous");
		changeLanguageAction.ShortLabel = changeLanguageAction.Tooltip = Lng.Translate("Language");
		dialogInfoAction.ShortLabel = dialogInfoAction.Tooltip = Lng.Translate("Info");

		labelID3v1Tree.LabelProp = Lng.Translate("Tag1");
		labelID3v2Tree.LabelProp = Lng.Translate("Tag2");

		infoWin.Title = Lng.Translate("Info");
	}
    
	#endregion

	#region methods


	private void ExecuteProcess(string cmd,string prms = null)	
	{
		var proc = new Process 
		{
		    StartInfo = new ProcessStartInfo 
			{
			        FileName = cmd,
			        Arguments = prms,
			        UseShellExecute = false,
			        RedirectStandardOutput = false,
			        CreateNoWindow = false
			}
	
		};
				
		proc.Start();						
	}

	private void CreateGridColumns()
	{
		_treeView1Data.Data.Clear();
		_treeView2Data.Data.Clear();

		_treeView1Data.Columns.Clear();
		_treeView2Data.Columns.Clear();

		// filename

		var fileNameColName = Lng.Translate("FileName");

		_treeView1Data.AppendStringColumn(fileNameColName, null, false);
        _treeView2Data.AppendStringColumn(fileNameColName, null, false);

		// creating base TAG columns

		foreach (var colName in TAGBase.BaseCollumnNames)
		{
            if (colName == "Genre")
                continue;

			var treeColName = Lng.Translate(colName);

			switch (colName)
			{
				case "Title": if (MP3List.SortColumn == SongList.SortColumnEnum.Title) treeColName = treeColName +"* " ; break;
				case "Artist": if (MP3List.SortColumn == SongList.SortColumnEnum.Artist) treeColName = treeColName + "* " ; break;
				case "Album": if (MP3List.SortColumn == SongList.SortColumnEnum.Album) treeColName = treeColName + "* " ; break;
				case "Year": if (MP3List.SortColumn == SongList.SortColumnEnum.Year) treeColName = treeColName + "* " ; break;
				case "Comment": if (MP3List.SortColumn == SongList.SortColumnEnum.Comment) treeColName = treeColName + "* " ; break;
				case "Track": if (MP3List.SortColumn == SongList.SortColumnEnum.Track) treeColName = treeColName + "* " ; break;
			}

            _treeView1Data.AppendStringColumn(treeColName, OnStringCellEdit,EditingModeActive);
            _treeView2Data.AppendStringColumn(treeColName, OnStringCellEdit, EditingModeActive);
		}

		var genreColName = Lng.Translate("Genre");

		if (MP3List.SortColumn == SongList.SortColumnEnum.Genre) genreColName = genreColName + "* " ; 

        var genreCol1 = _treeView1Data.AppendComboColumn(genreColName, OnComboCellEdit, EditingModeActive, TAGBase.ID3Genre);
	    genreCol1.MinWidth = 150;

        var genreCol2 = _treeView2Data.AppendComboColumn(genreColName, OnComboCellEdit, EditingModeActive, TAGBase.ID3Genre);
        genreCol2.MinWidth = 150;

		var changedColName = Lng.Translate("Changed");

		if (MP3List.SortColumn == SongList.SortColumnEnum.Changed) changedColName = changedColName + "* " ; 

        _treeView1Data.AppendCheckBoxColumn(changedColName,null, false);
        _treeView2Data.AppendCheckBoxColumn(changedColName, null, false);

		// creating TreeView columns

		_treeView1Data.CreateTreeViewColumns();
		_treeView2Data.CreateTreeViewColumns();
	}

	public void CopyTagOfSelectedSongs(bool fromTAG1)
	{
		var selSongs = GetSelectedSongs();
		foreach(var song in MP3List)
		{
			if (fromTAG1 && song.ID3v1.Active)
			{
				song.ID3v1.CopyTo(song.ID3v2);
			}
			if (!fromTAG1 && song.ID3v2.Active)
			{
				song.ID3v2.CopyTo(song.ID3v1);
			}
		}

		FillTree();
	}

	public void RunSelectedSong(string player)
	{
		var selSongs = GetSelectedSongs();
		if (selSongs.Count == 0)
		{
			Dialogs.InfoDialog(Lng.Translate("NoSelection"));
			return;
		}
		if (selSongs.Count > 1)
		{
			Dialogs.InfoDialog(Lng.Translate("SingleSelectionExpected"));
			return;
		}

		//ExecuteProcess(selSongs[0].FileName);
		ExecuteProcess(player,"\"" + selSongs[0].FileName+ "\"");
	}

	public bool SaveChanges()
	{
		var changedSongs = MP3List.ChangedSongs;
		if (changedSongs.Count==0)
		{
			Dialogs.InfoDialog(Lng.Translate("NoChanges"));
		} else
		{
			if (Dialogs.QuestionDialog(String.Format(Lng.Translate("SaveAllConfirm"),changedSongs.Count))!= ResponseType.Ok)
			return false;			

			foreach (var song in changedSongs)
			{
				song.SaveChanges(false);
			}

			FillTree();
		}

		return true;
	}

	public void SelectAll()
	{
		_treeView1Data.Tree.Selection.SelectAll();
		_treeView2Data.Tree.Selection.SelectAll();
	}

	public void UnSelectAll()
	{
		_treeView1Data.Tree.Selection.UnselectAll();
		_treeView2Data.Tree.Selection.UnselectAll();
	}

	public int Remove(bool onlySelected)
	{
		var res = 0;

		if (onlySelected)
		{
			var  selectedsongs = GetSelectedSongs();
			var changedCount = 0;
			foreach (var song in selectedsongs)	
			if (song.Changed) changedCount++;

			if (selectedsongs.Count == 0)
			{
				Dialogs.InfoDialog(Lng.Translate("NoItemSelected"));
				return res;
			} else
			{
				var question2 = selectedsongs.Count.ToString();;
				if (changedCount > 0)
				{
					question2 += ", "+Lng.Translate("UnSaved")+" " + changedCount.ToString();
				} 

				if (Dialogs.QuestionDialog(String.Format(Lng.Translate("ConfirmRemoveSelected"),question2)) == Gtk.ResponseType.Ok)
				{
					res = selectedsongs.Count;
					foreach (var song in selectedsongs)					
						MP3List.Remove(song);
				}
			}
		} else
		{
			if (MP3List.Count == 0)
			{
				return res;
			}

			var changedCount = MP3List.ChangedSongs.Count;
			var question2 = MP3List.Count.ToString();

			if (changedCount > 0)
			{
				question2 += ", "+Lng.Translate("UnSaved") + " "+changedCount.ToString();
			} 

			if (Dialogs.QuestionDialog(String.Format(Lng.Translate("ConfirmRemoveAll"),question2)) == Gtk.ResponseType.Ok)
				{
					res = MP3List.Count;
					MP3List.Clear();
				}
		}

		if (res>0)		
		{
			UnSelectAll();
			FillTree();
		}

		return res;
	}

	public void AddFile(string fname)
	{
		var addedMp3 = MP3List.AddFile(fname);
		if (addedMp3 != null)
		{
        	FillTree();
		}
	}

	public void AddFolder(string dir,bool recursive)
	{
		progressWin.Title = Lng.Translate("WaitPlease");
		progressWin.Description = Lng.Translate("LoadingTags");
		progressWin.Percents = 0;
		progressWin.Show();

		while (GLib.MainContext.Iteration());

        MP3List.AddFilesFromFolder(dir,recursive,Progress);
        FillTree();

		progressWin.Hide();
	}

    public void Progress(object sender, MP3Tagger.ProgressEventArgs e) 
    {
		progressWin.Percents = e.Percent;
		while (GLib.MainContext.Iteration());
    }

    public void FillTree()
    {
		// todo - after grid refresh scroll bar starts on beginning

		Logger.Logger.WriteToLog("Filling TreeView");

		var selectedSongs = GetSelectedSongs();

		_treeView1Data.Data.Clear();
		_treeView2Data.Data.Clear();

        foreach (var song in MP3List)
        {
			var tree1Values = song.ID3v1.ValuesAsOLbjecttList(TAGBase.BaseCollumnNames);
			var tree2Values = song.ID3v2.ValuesAsOLbjecttList(TAGBase.BaseCollumnNames);

			tree1Values.Insert(0,System.IO.Path.GetFileName(song.FileName));
			tree2Values.Insert(0,System.IO.Path.GetFileName(song.FileName));

			// checkbox
			tree1Values.Add(song.ID3v1.Changed);
            tree2Values.Add(song.ID3v2.Changed);

			// add data values to tree
            _treeView1Data.AppendData(tree1Values);
			_treeView2Data.AppendData(tree2Values);
        }

        tree.Model = _treeView1Data.CreateTreeViewListStore();
		tree2.Model = _treeView2Data.CreateTreeViewListStore();

		SelectSongs(selectedSongs);	

		//ensure to select first row
		if (GetSelectedSongs().Count == 0)
		{
			if (MP3List.Count>0) SelectSong(MP3List[0]);
		}

		/*
		// scroll to first selected ?
		var selSongs = GetSelectedSongs();
		if (selSongs.Count>0)
		{
			var ind = selSongs[0].Index;
			var iter = _treeView1Data.TreeIters[ind];
			tree.ScrollToCell((Gtk.TreePath)iter,tree.Columns[0],false,(float)0,(float)0);
		}
		*/

        //Show();
	}

	#endregion

    #region properties

	public Song FirstSelectedSong
	{
		get
		{

			var selected = GetSelectedSongs();
			if (selected != null && selected.Count>0)
			{
				return selected[0];
			}

			return null;
		}

	}

	public Song MultiSelectSong
	{
		get { return _multiSelectSong; }
		set { _multiSelectSong = value; }
	}

	public SelectionMode ActualSelectionMode
	{
		get
		{
			return tree.Selection.Mode;
		}
		set
		{
			tree.Selection.Mode = value;
			tree2.Selection.Mode = value;

			if (value == SelectionMode.Multiple)
			{
				selectSingleAction.Active = false;
				selectMultipleAction.Active = true;
			} else
			{
				selectSingleAction.Active = true;
				selectMultipleAction.Active = false;
			}
		}
	}


	public bool EditingModeActive
	{
		get
		{
			return editingModeAction.Active;
		}
		set
		{
			editingModeAction.Active = value;
		}
	}

	public string[] Args 
	{
		get { return _args; }
		set {_args = value;	}
	}

    public SongList MP3List
    {
        get { return _songList; }
        set { _songList = value; }
    }

    #endregion

	#region selection methods

	public void ReloadSelectedSongs()
	{
		foreach (var song in GetSelectedSongs())
		{
			song.Reload();
		}
	}

	public void ApplySongEdit(string fileRenameMask = "")
	{
		var selectedSongs = GetSelectedSongs();

		// apply multi selection changes ?
		if (selectedSongs.Count > 1)
		{
			foreach (var song in selectedSongs)
			{
				song.ID3v1.Active = MultiSelectSong.ID3v1.Active;
				song.ID3v2.Active = MultiSelectSong.ID3v2.Active;

				if (MultiSelectSong.ID3v1.Active) MultiSelectSong.ID3v1.CopyNonEmptyValuesTo(song.ID3v1);
				if (MultiSelectSong.ID3v2.Active) 
				{
					MultiSelectSong.ID3v2.CopyNonEmptyValuesTo(song.ID3v2);

					// image ?
					var multiSelectImgFrame = MultiSelectSong.ID3v2.GetFrameByImageType(MP3Tagger.ImageType.CoverFront);
					if (multiSelectImgFrame != null)
					{
						var imgFrame = song.ID3v2.GetOrCreateImageFrame(MP3Tagger.ImageType.CoverFront);
						multiSelectImgFrame.FrameImage.CopyTo(imgFrame.FrameImage);
					}
				}
			}

		}

		if (selectedSongs.Count > 0)
		{
			// renaming by mask?
			if (!String.IsNullOrEmpty(fileRenameMask) &&
			    fileRenameMask.Trim() != "*f" &&
			    fileRenameMask.Trim() != System.IO.Path.GetFileName(selectedSongs[0].FileName))
			{
				if (Dialogs.ConfirmDialog(String.Format(Lng.Translate("ConfirmRenameByMask"),selectedSongs.Count,fileRenameMask)))
				{	
					foreach (var song in selectedSongs)
					{
						song.RenameByMask(fileRenameMask);
					}
				}
			}			

		}

		if (selectedSongs.Count > 0)
		{
			foreach (var song in selectedSongs)
			{
				if (!song.ID3v1.Active) song.ID3v1.Clear(); else song.ID3v1.UnMask();
				if (!song.ID3v2.Active) song.ID3v2.Clear(); else song.ID3v2.UnMask();
			}
		}

		FillTree();
	}

	public int ActualSelectedSongIndex(TreeViewData data)
	{
		Gtk.TreeIter iter;
		var ok = data.Tree.Selection.GetSelected(out iter);

		if (!ok)
		{
			return -1;
		}

		foreach (KeyValuePair<int,Gtk.TreeIter> kvp in data.TreeIters)
		{
			if (kvp.Value.UserData == iter.UserData)
			{
				var selectedDataIndex = kvp.Key;

				if (MP3List.Count>selectedDataIndex)
				{
					return selectedDataIndex;
				}
			}
		}

		return -1;
	}

	public Song ActualSelectedSong(TreeViewData data)
	{
		var index = ActualSelectedSongIndex(data);
		if (index == -1)
			return null;

		return MP3List[index];
	}

	public List<Song> GetSelectedSongs()
	{
		// detecting actual Tree 

		if (notebook.CurrentPage  == 0)
		{
			return GetSelectedSongs(_treeView1Data);
		}
		if (notebook.CurrentPage  == 1)
		{
			return GetSelectedSongs(_treeView2Data);
		}

		return null;
	}

	private List<Song> GetSelectedSongs(TreeViewData data)
	{
		var selectedSongs = new List<Song>();

		foreach(Gtk.TreePath selectedItem in data.Tree.Selection.GetSelectedRows())
		{
			var indicies = selectedItem.Indices;
			if (indicies.Length>0)
			{
				var selectedIndex = indicies[0];
				if (MP3List.Count>selectedIndex)
				{
					selectedSongs.Add(MP3List[selectedIndex]);
				}
			}
		}

		return selectedSongs;
	}

	public void EditSelectedSongs()
	{
		try
		{

			Song actualSelectedSong = null;
			var selectedSongs = GetSelectedSongs();

			if (selectedSongs.Count == 1)		
			{
				// single edit
				actualSelectedSong = selectedSongs[0];

			} else
			if (selectedSongs.Count > 1)		
			{
				//	multiple edit

				MultiSelectSong.Clear();
				actualSelectedSong = MultiSelectSong;

				// detect id3 v1 and v2
				var v1Count = 0;
				var v2Count = 0;
				foreach (var s in selectedSongs)
				{
					if (s.ID3v1.Active) v1Count++;
					if (s.ID3v2.Active) v2Count++;
				}

				if ( (double) v1Count >= (double)selectedSongs.Count/(double)2)
				{
					MultiSelectSong.ID3v1.Active = true;
				}
				if ( (double) v2Count >= (double)selectedSongs.Count/(double)2)
				{
					MultiSelectSong.ID3v2.Active = true;
				}

			}

			if (actualSelectedSong != null)
	        {
	            editWindow.CurrentSong = actualSelectedSong;
	            
	            // workaround for bug when controls are not shown 
	            editWindow.Visible = true;	            
	            editWindow.ShowNow();
	            //editWindow.Show();
	            	            
	        }
		}
		catch (Exception ex)
		{
			Logger.Logger.WriteToLog("Error while editing",ex);
			Dialogs.InfoDialog(Lng.Translate("Error"));
		}
	}

	public void SelectSong(Song song)
	{
		SelectRows(new List<int>() { song.Index });
	}

	public void SelectSongs(List<Song> songs)
	{
			var rows = new List<int>();
			foreach (var s in songs)
			{
				rows.Add(s.Index);
			}
			SelectRows(rows);		
	}

	public void SelectRow(int rowIndex)
	{
		SelectRows(new List<int>() { rowIndex });
	}

	public void SelectRows(List<int> rows)
	{
		if (rows == null)
			return;

		tree.Selection.UnselectAll();
		tree2.Selection.UnselectAll();

		foreach (var row in rows)
		{
			if (row>=0 && row<=MP3List.Count-1)		
			{
				tree.Selection.SelectIter( _treeView1Data.TreeIters[row]);
				tree2.Selection.SelectIter( _treeView2Data.TreeIters[row]);
			}
		}

        if (editWindow.IsActive && rows.Count == 1)
        {
            editWindow.CurrentSong = MP3List[rows[0]];        
		}

		ScrollToFirstSelected();
	}

	public void ScrollToFirstSelected()
	{
		// scroll to first selected ?
		TreePath[] treePaths;
		if (notebook.CurrentPage == 0)		
			treePaths = tree.Selection.GetSelectedRows();
		else
			treePaths = tree2.Selection.GetSelectedRows();

		if (treePaths != null &&
		    treePaths.Length > 0)
		{
			tree.ScrollToCell(treePaths[0],tree.Columns[0],false,(float)0,(float)0);
		}
	}


	public void SelectNext()
	{
		if (MP3List.Count == 0)
			return;

		var selectedSongs = GetSelectedSongs();

		if (selectedSongs.Count == 1)
		{
			// single edit

			int actualSelectedSongIndex;

			actualSelectedSongIndex = selectedSongs[0].Index;
			actualSelectedSongIndex++;

			if (actualSelectedSongIndex>=MP3List.Count)
			{
				actualSelectedSongIndex = 0;
			}			

			SelectRow(actualSelectedSongIndex);
		}
	}

	public void SelectPrev()
	{
		if (MP3List.Count == 0)
			return;

		var selectedSongs = GetSelectedSongs();

		if (selectedSongs.Count == 1)
		{
			// single edit

			int actualSelectedSongIndex;

			actualSelectedSongIndex = selectedSongs[0].Index;
			actualSelectedSongIndex--;

			if (actualSelectedSongIndex<0)
			{
				actualSelectedSongIndex = MP3List.Count-1;
			}			

			SelectRow(actualSelectedSongIndex);
		}
	}

	#endregion

	#region Context Menu

	private Gtk.MenuItem AddContextMenuButton(Gtk.Menu menu, string actionKey, string title)
	{
		Gtk.MenuItem item = new MenuItem(title);
		item.ButtonPressEvent +=new ButtonPressEventHandler(contextMenu_ButtonPressEvent);
		item.Data["key"] = actionKey;
		menu.Add(item);

		return item;
	}

	private void AddSortContextMenuButton(Gtk.Menu menu, SongList.SortColumnEnum sortColumn, string title)
	{
		Gtk.MenuItem item = new MenuItem(title);
		item.ButtonPressEvent +=new ButtonPressEventHandler(contextMenu_ButtonPressEvent);
		item.Data["key"] = "sort";
		item.Data["sort"] = sortColumn;
		menu.Add(item);
	}


	private void AddSortContextMenu(Gtk.Menu menu)
	{
		Gtk.MenuItem sortItem = new MenuItem(Lng.Translate("Sort"));

		menu.Add(sortItem);

		// sort menu

		Gtk.Menu sortMenu = new Menu();

		AddSortContextMenuButton(sortMenu,SongList.SortColumnEnum.FileName,Lng.Translate("FileName"));

		AddSortContextMenuButton(sortMenu,SongList.SortColumnEnum.Title,Lng.Translate("Title"));
		AddSortContextMenuButton(sortMenu,SongList.SortColumnEnum.Artist,Lng.Translate("Artist"));
		AddSortContextMenuButton(sortMenu,SongList.SortColumnEnum.Album,Lng.Translate("Album"));

		AddSortContextMenuButton(sortMenu,SongList.SortColumnEnum.Year,Lng.Translate("Year"));
		AddSortContextMenuButton(sortMenu,SongList.SortColumnEnum.Genre,Lng.Translate("Genre"));
		AddSortContextMenuButton(sortMenu,SongList.SortColumnEnum.Album,Lng.Translate("Comment"));
		AddSortContextMenuButton(sortMenu,SongList.SortColumnEnum.Track,Lng.Translate("TrackNumber"));
		AddSortContextMenuButton(sortMenu,SongList.SortColumnEnum.Changed,Lng.Translate("Changed"));

		sortItem.Submenu = sortMenu;
	}

	private void AddRunContextMenu(Gtk.Menu menu)
	{
		Gtk.MenuItem runItem = new MenuItem(Lng.Translate("Run"));

		menu.Add(runItem);

		// submenu

		Gtk.Menu runMenu = new Menu();

		foreach (var runApp in Configuration.RunApplications)
		{
			var item = AddContextMenuButton(runMenu,"run",runApp);
			item.Data["runApp"] = runApp;
		}

		runItem.Submenu = runMenu;
	}
		 
	private void ShowContextMenu()
	{
		Gtk.Menu contextMenu = new Menu();

		//AddContextMenuButton(contextMenu,"run",Lng.Translate("Run"));

		AddRunContextMenu(contextMenu);
		AddSortContextMenu(contextMenu);
			
		contextMenu.ShowAll();
		contextMenu.Popup();
		contextMenu.Dispose();
	}

	#endregion

	#region events

    private void OnStringCellEdit(object o, EditedArgs args)
    {
		try
		{
				if (
						(o != null) &&
						(o is Gtk.Object) &&
						((o as Gtk.Object).Data != null) &&
						((o as Gtk.Object).Data.ContainsKey("colName")) &&
						((o as Gtk.Object).Data["colName"] != null) &&
						((o as Gtk.Object).Data.ContainsKey("colPosition")) &&
						((o as Gtk.Object).Data["colPosition"] != null)
					)
				{
					var selectedCol = Convert.ToInt32((o as Gtk.Object).Data["colPosition"]);
					var selectedRow = Convert.ToInt32(args.Path);

					var editingSong = MP3List[selectedRow];
					

					TAGBase baseTag = null;
					if (notebook.CurrentPage  == 0)
					{
							baseTag = editingSong.ID3v1 as TAGBase;
					} else
					//if (notebook.CurrentPage  == 1)
					{
						baseTag = editingSong.ID3v2 as TAGBase;
					}


					switch (selectedCol)
					{
						case 1: baseTag.Title = args.NewText; break;
						case 2: baseTag.Artist = args.NewText; break;
						case 3: baseTag.Album = args.NewText; break;
						//case 4: baseTag.Year = args.NewText; break;
						case 5: baseTag.Comment = args.NewText; break;
						//case 6: baseTag.Track = args.NewText; break;
						//case 7: baseTag.Genre = args.NewText; break;
						
					}

						if (selectedCol == 4)
						{
							// editing year

							int year;
							if (int.TryParse(args.NewText,out year))
							{
								baseTag.Year = year;
							}
						}

						if (selectedCol == 6)
						{
							// editing track number

							byte trck;
							if (byte.TryParse(args.NewText,out trck))
							{
								baseTag.TrackNumber = trck;
							}
						}

				FillTree();
			}

		} catch (Exception ex)
		{
				Logger.Logger.WriteToLog("Error editing cell (path:{0})",ex);								
		}

    }

    private void OnComboCellEdit(object o, EditedArgs args)
    {
		try
		{

		if (
				(o != null) &&
				(o is Gtk.Object) &&
				((o as Gtk.Object).Data != null) &&
				((o as Gtk.Object).Data.ContainsKey("colPosition")) &&
				((o as Gtk.Object).Data["colPosition"] != null)
			)
		{
			var selectedCol = Convert.ToInt32((o as Gtk.Object).Data["colPosition"]);
			var selectedRow = Convert.ToInt32(args.Path);

			var editingSong = MP3List[selectedRow];			

			TAGBase baseTag = null;
			if (notebook.CurrentPage  == 0)
			{
					baseTag = editingSong.ID3v1 as TAGBase;
			} else
			//if (notebook.CurrentPage  == 1)
			{
				baseTag = editingSong.ID3v2 as TAGBase;
			}

			if (selectedCol == 7)
			{
				// editing genre
				for (var i=0;i<TAGBase.ID3Genre.Length;i++)
				{
					if (args.NewText == TAGBase.ID3Genre[i])
					{
							baseTag.Genre = Convert.ToByte(i);
							break;
					}
				}
			}

			FillTree();
		}

		} catch (Exception ex)
		{
				Logger.Logger.WriteToLog("Error editing cell (path:{0})",ex);								
		}
    }

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void OnTreeSelectCursorRow (object o, SelectCursorRowArgs args)
	{

	}

    //[GLib.ConnectBefore]
    private void contextMenu_ButtonPressEvent(object o, ButtonPressEventArgs args)
    {
		if (o == null ||
		    !(o is Gtk.MenuItem))		
			return;

		var menuItem = (o as Gtk.MenuItem);

		if ( menuItem.Data == null ||
		    !(menuItem.Data.ContainsKey("key")) )
			return;

		var key = menuItem.Data["key"].ToString();

		if (key == "run")		
		{
			if (menuItem.Data.ContainsKey("runApp"))
			{
				var player = Convert.ToString(menuItem.Data["runApp"]);
				RunSelectedSong(player);
			}
		}

		if (key == "sort" && menuItem.Data.ContainsKey("sort"))
		{
			var sortCol = (SongList.SortColumnEnum) menuItem.Data["sort"];

			MP3List.SortBy(sortCol,notebook.CurrentPage == 0);

			CreateGridColumns();
			FillTree();
		}
    }

    [GLib.ConnectBefore]
    private void tree_ButtonPressEvent(object o, ButtonPressEventArgs args)
    {
        if (args.Event.Type == Gdk.EventType.TwoButtonPress)
        {
			// double click
            EditSelectedSongs();
        }
		if(args.Event.Button == 3) 
		{
			// right button click
			ShowContextMenu();
		}

    }

	protected void OnSelectionChanged (object sender, EventArgs e)
	{
	}

	protected void OnTreeRowActivated (object o, RowActivatedArgs args)
	{
		EditSelectedSongs();
	}

	protected void OnTreeToggleCursorRow (object o, ToggleCursorRowArgs args)
	{

	}

	#region toolbar action events

	protected void OnEditActionActivated (object sender, EventArgs e)
	{
		EditSelectedSongs();
	}

	protected void OnCloseActionActivated (object sender, EventArgs e)	
	{
		Gtk.Application.Quit();
	}


	protected void OnDndMultipleActionActivated (object sender, EventArgs e)
	{
		tree.Selection.Mode = SelectionMode.Multiple;
		tree2.Selection.Mode = SelectionMode.Multiple;
	}

	protected void OnSelectSingleActionActivated (object sender, EventArgs e)
	{
		tree.Selection.Mode = SelectionMode.Single;
		tree2.Selection.Mode = SelectionMode.Single;
	}

	protected void OnGoForwardActionActivated (object sender, EventArgs e)	
	{
		SelectNext();
	}

	protected void OnGoBackActionActivated (object sender, EventArgs e)
	{
		SelectPrev();
	}

	protected void OnEditingModeActionActivated (object sender, EventArgs e)
	{
		_treeView1Data.ClearTreeView();
		_treeView2Data.ClearTreeView();

		CreateGridColumns();

		FillTree();
	}

	protected void OnSaveActionActivated (object sender, EventArgs e)
	{
		SaveChanges();
	}

	protected void OnAddActionActivated (object sender, EventArgs e)
	{
		toolBarWin.Show(ToolBarWin.KindEnum.Add);
	}


	protected void OnRemoveActionActivated (object sender, EventArgs e)
	{
		toolBarWin.Show(ToolBarWin.KindEnum.Remove);
	}


	protected void OnChangeLanguageActionActivated (object sender, EventArgs e)
	{
		toolBarWin.Show(ToolBarWin.KindEnum.Languages);
	}

	protected void OnSelectActionActivated (object sender, EventArgs e)
	{
		toolBarWin.Show(ToolBarWin.KindEnum.Selection);
	}

	protected void OnSizeAllocated(object o, SizeAllocatedArgs args)
	{
		// resizing notebook size

		int margin = 10;

		int w = 0;
		int h = 0;
		GetSize(out w,out h);


		int wNotebook = 0;
		int hNotebook = 0;

		notebook.GetSizeRequest(out wNotebook,out hNotebook);

		notebook.SetSizeRequest(w-margin,h-50-margin);
	}

	protected void OnNotebookSizeAllocated (object o, SizeAllocatedArgs args)
	{

	}

	protected void OnDialogInfoActionActivated (object sender, EventArgs e)	
	{
		Dialogs.CenterChildToParent(this,infoWin);
		infoWin.Show();
	}

	#endregion

    #endregion

}
