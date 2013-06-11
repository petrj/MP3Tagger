using System;
using System.IO;
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
	private Song _multiSelectSong;

	private TreeViewData _treeView1Data;
	private TreeViewData _treeView2Data;

	#endregion

	#region public fields

	public string[] _args;

	#endregion
   
	#region constructor

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Logger.Logger.WriteToLog("Starting new application instance");

		Build ();

		_treeView1Data = new TreeViewData(tree); 
		_treeView2Data = new TreeViewData(tree2); 

		progressWin = new MP3Tagger.ProgressBarWindow();
	    progressWin.Hide();

		EditingModeActive = false;
		ActualSelectionMode = SelectionMode.Multiple;

		CreateGridColumns();

		_multiSelectSong = new Song();

		editWindow = new SongDetail(this);
		editWindow.Hide();

		tree.Selection.Changed+=new EventHandler(OnSelectionChanged);
		tree2.Selection.Changed+=new EventHandler(OnSelectionChanged);

        tree.ButtonPressEvent += tree_ButtonPressEvent;
			
		this.Show();
	}
    

	#endregion

	#region methods

	private void CreateGridColumns()
	{
		_treeView1Data.Data.Clear();
		_treeView2Data.Data.Clear();

		_treeView1Data.Columns.Clear();
		_treeView2Data.Columns.Clear();

		// filename
		_treeView1Data.AppendStringColumn("FileName", null, false);
        _treeView2Data.AppendStringColumn("FileName", null, false);

		// creating base TAG columns

		foreach (var colName in TAGBase.BaseCollumnNames)
		{
            if (colName == "Genre")
                continue;

            _treeView1Data.AppendStringColumn(colName, OnStringCellEdit,EditingModeActive);
            _treeView2Data.AppendStringColumn(colName, OnStringCellEdit, EditingModeActive);
		}

        var genreCol1 = _treeView1Data.AppendComboColumn("Genre", OnComboCellEdit, EditingModeActive, TAGBase.ID3Genre);
	    genreCol1.MinWidth = 150;

        var genreCol2 = _treeView2Data.AppendComboColumn("Genre", OnComboCellEdit, EditingModeActive, TAGBase.ID3Genre);
        genreCol2.MinWidth = 150;

        _treeView1Data.AppendCheckBoxColumn("Changed", null, false);
        _treeView2Data.AppendCheckBoxColumn("Changed", null, false);

		// creating TreeView columns

		_treeView1Data.CreateTreeViewColumns();
		_treeView2Data.CreateTreeViewColumns();
	}

	public bool SaveChanges()
	{
		var changedSongs = MP3List.ChangedSongs;
		if (changedSongs.Count==0)
		{
			Dialogs.InfoDialog("No changes");
		} else
		{
			if (Dialogs.QuestionDialog(String.Format("Save all changes ({0})?",changedSongs.Count))!= ResponseType.Ok)
			return false;			

			foreach (var song in changedSongs)
			{
				song.SaveChanges(false);
			}

			FillTree();
		}

		return true;
	}

	public void AddFolder(string dir,bool recursive)
	{
		progressWin.Title = "Čekejte, prosím";
		progressWin.Description = "Probíhá načítání mp3 tagů ...";
		progressWin.Percents = 0;
		progressWin.Show();

        MP3List.AddFilesFromFolder(dir,recursive,Progress);
        FillTree();
		if (MP3List.Count>0) SelectSong(MP3List[0]);
		progressWin.Destroy();
	}

    public void Progress(object sender, MP3Tagger.ProgressEventArgs e) 
    {
		progressWin.Percents = e.Percent;
		while (GLib.MainContext.Iteration());
    }

    public void FillTree()
    {
		Logger.Logger.WriteToLog("Filling TreeView");

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

        Show();
	}

	#endregion

    #region properties

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

	public void ApplySongEdit()
	{
		var selectedSongs = GetSelectedSongs();
		if (selectedSongs.Count > 1)
		{
			// multi selection
			if (MultiSelectSong.ID3v1.Active)
			{
				foreach (var song in selectedSongs)
				{
					MultiSelectSong.ID3v1.CopyNonEmptyValuesTo(song.ID3v1);
					MultiSelectSong.ID3v2.CopyNonEmptyValuesTo(song.ID3v2);
				}
			}
		}	

		FillTree();
		SelectSongs(selectedSongs);
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


	private void EditSelectedSongs()
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
            editWindow.Show();
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

	#region events

    private void OnStringCellEdit(object o, EditedArgs args)
    {
		try
		{

			var selectedSongs = GetSelectedSongs();		

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
				SelectSongs(selectedSongs);
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

    [GLib.ConnectBefore]
    private void tree_ButtonPressEvent(object o, ButtonPressEventArgs args)
    {
        if (args.Event.Type == Gdk.EventType.TwoButtonPress)
        {
            EditSelectedSongs();
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

	protected void OnOpenActionActivated (object sender, EventArgs e)
	{
		var dir = Dialogs.OpenDirectoryDialog("Choose the directory to open");
		if (dir != null)
			AddFolder(dir,false);
	}

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

		if (MP3List.Count>0) SelectSong(MP3List[0]);
	}

	protected void OnSaveActionActivated (object sender, EventArgs e)
	{
		SaveChanges();
	}

	#endregion

    #endregion

}
