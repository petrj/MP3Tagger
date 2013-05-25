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

		// creating base TAG columns

		foreach (var colName in TAGBase.AllCollumnNames)
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


	private static ResponseType QuestionDialog(string message)
	{
		MessageDialog md = new MessageDialog (null, 
                                  DialogFlags.DestroyWithParent,
                              	  MessageType.Question, 
                                  ButtonsType.OkCancel, message);

		var result = (ResponseType)md.Run ();
		md.Destroy();

		return result;
	}

	private static void InfoDialog(string message,MessageType msgType = MessageType.Info )
	{
		MessageDialog md = new MessageDialog (null, 
                                  DialogFlags.DestroyWithParent,
                              	  msgType, 
                                  ButtonsType.Close, message);
			md.Run();
			md.Destroy();
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
			var tree1Values = song.ID3v1.ValuesAsOLbjecttList(TAGBase.AllCollumnNames);
			var tree2Values = song.ID3v2.ValuesAsOLbjecttList(TAGBase.AllCollumnNames);

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
        
    }

    private void OnComboCellEdit(object o, EditedArgs args)
    {

    }

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void OnOpenActionActivated (object sender, EventArgs e)
	{
		 var fc = new Gtk.FileChooserDialog("Choose the directory to open",
                    this,
                    FileChooserAction.SelectFolder,
                    "Cancel", ResponseType.Cancel,
                    "Open", ResponseType.Accept);

        if (fc.Run() == (int)ResponseType.Accept)
        {
			this.AddFolder(fc.Filename,false);
        }
        fc.Destroy();
	}

	protected void OnEditActionActivated (object sender, EventArgs e)
	{
		EditSelectedSongs();
	}

	protected void OnCloseActionActivated (object sender, EventArgs e)	
	{
		Gtk.Application.Quit();
	}

	protected void OnTreeSelectCursorRow (object o, SelectCursorRowArgs args)
	{

	}

	protected void OnDndMultipleActionActivated (object sender, EventArgs e)
	{
		tree.Selection.Mode = SelectionMode.Multiple;
		tree2.Selection.Mode = SelectionMode.Multiple;
	}

	protected void OnDndActionActivated (object sender, EventArgs e)
	{
		tree.Selection.Mode = SelectionMode.Single;
		tree2.Selection.Mode = SelectionMode.Single;
	}

	[GLib.ConnectBefore]
	protected void OnTreeButtonPressEvent (object o, ButtonPressEventArgs args)
	{
		if (args.Event.Type == Gdk.EventType.TwoButtonPress)
		{
			EditSelectedSongs();
		}
	}

	protected void OnGoForwardActionActivated (object sender, EventArgs e)	
	{
		SelectNext();
	}

	protected void OnGoBackActionActivated (object sender, EventArgs e)
	{
		SelectPrev();
	}

	protected void OnSelectionChanged (object sender, EventArgs e)
	{

	}

	protected void OnEditingModeActionActivated (object sender, EventArgs e)
	{
		_treeView1Data.ClearTreeView();
		CreateGridColumns();
		//ActualSelectionMode = SelectionMode.Single;
		FillTree();
		if (MP3List.Count>0) SelectSong(MP3List[0]);
	}

	protected void OnTreeRowActivated (object o, RowActivatedArgs args)
	{
		EditSelectedSongs();
	}

	protected void OnTreeToggleCursorRow (object o, ToggleCursorRowArgs args)
	{

	}

	protected void OnSaveActionActivated (object sender, EventArgs e)
	{
		var changedSongs = MP3List.ChangedSongs;
		if (changedSongs.Count==0)
		{
			InfoDialog("No changes");
		} else
		{
			if (QuestionDialog(String.Format("Save all changhes ({0})?",changedSongs.Count))!= ResponseType.Ok)
			return;			

			foreach (var song in changedSongs)
			{
				song.SaveChanges(false);
			}

			FillTree();
		}
	}
    #endregion

}
