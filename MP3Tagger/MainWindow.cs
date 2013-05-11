using System;
using System.Collections.Generic;
using Grid;
using Gtk;
using MP3Tagger;
using Logger;

public partial class MainWindow: Gtk.Window
{
    SongList _songList = new SongList();
	MP3Tagger.ProgressBarWindow progressWin;
	SongDetail editWindow;

	private TreeViewData _treeView1Data = new TreeViewData(); 
	private TreeViewData _treeView2Data = new TreeViewData(); 

	public string[] _args;
   
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Logger.Logger.WriteToLog("Starting new application instance");

		Build ();

		progressWin = new MP3Tagger.ProgressBarWindow();

		foreach (var colName in TAGBase.AllCollumnNames)
		{
			_treeView1Data.AppendStringColumn(colName);
			_treeView2Data.AppendStringColumn(colName);
		}

		//tree.Selection.Mode = SelectionMode.Multiple;
		//tree.Selection.Mode = SelectionMode.Extended;
		tree.Selection.Mode = SelectionMode.Browse;
		//tree.Selection.Mode = SelectionMode.Browse;
		//tree.Selection.Mode = SelectionMode.Single;

		editWindow = new SongDetail();
		editWindow.MainWin = this;
		editWindow.Hide();
			
		this.Show();
	}

	private static void InfoDialog(string message,MessageType msgType)
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
		progressWin.Destroy();
	}

    public void Progress(object sender, MP3Tagger.ProgressEventArgs e) 
    {
		progressWin.Percents = e.Percent;
		while (GLib.MainContext.Iteration());
    }

    private void FillTree()
    {
		Logger.Logger.WriteToLog("Filling TreeView");

        
        _treeView1Data.CreateTreeViewColumns(tree);
		_treeView2Data.CreateTreeViewColumns(tree2);

        foreach (var song in MP3List)
        {
            //ViewData.AppendData(new List<object>() { "Show must go on", "Queen", "Best of" });

            _treeView1Data.AppendData(song.ID3v1.ValuesAsOLbjecttList(TAGBase.AllCollumnNames));
			_treeView2Data.AppendData(song.ID3v2.ValuesAsOLbjecttList(TAGBase.AllCollumnNames));
        }

        tree.Model = _treeView1Data.CreateTreeViewListStore();
		tree2.Model = _treeView2Data.CreateTreeViewListStore();

		SelectRow(0);

        Show();
    }

    #region properties

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

    #region events

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


	public int ActualSelectedSongIndex()
	{
		Gtk.TreeIter iter;
		var ok = tree.Selection.GetSelected(out iter);

		if (!ok)
		{
			return -1;
		}

		foreach (KeyValuePair<int,Gtk.TreeIter> kvp in _treeView1Data.TreeIters)
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

	public Song ActualSelectedSong()
	{
		var index = ActualSelectedSongIndex();
		if (index == -1)
			return null;

		return MP3List[index];
	}


	private List<Song> GetSelectedSongs()
	{
		var selectedSongs = new List<Song>();

		foreach(Gtk.TreePath selectedItem in tree.Selection.GetSelectedRows())
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
		var actualSelectedSong = ActualSelectedSong();

		if (actualSelectedSong == null)
		{
			InfoDialog("No row selected",MessageType.Warning);
			return;
		}

		var selectedSongs = GetSelectedSongs();
		if (selectedSongs.Count>0)
		{
			editWindow.CurrentSong = actualSelectedSong;
			editWindow.Songs = selectedSongs;
			editWindow.Show();
		}
	}

	public void SelectRow(int index)
	{
		if (index>=0 && index<=MP3List.Count-1)		
		{
			tree.Selection.UnselectAll();
			tree.Selection.SelectIter( _treeView1Data.TreeIters[index]);

			if (editWindow.Visible)
			{
				editWindow.CurrentSong = MP3List[index];
			}
		}
	}

	public void SelectNext()
	{
		if (MP3List.Count == 0)
			return;

		var actualSelectedSongIndex = ActualSelectedSongIndex();

		if (actualSelectedSongIndex == -1 || MP3List.Count == 1)
		{
			SelectRow(0);
			return;
		}

		var selectionMode = tree.Selection.Mode;
		if ( (selectionMode == SelectionMode.Single) || (selectionMode == SelectionMode.Browse))
		{
			var nextSelectedSongIndex = actualSelectedSongIndex+1;
			if (nextSelectedSongIndex>MP3List.Count-1) nextSelectedSongIndex = 0;
			SelectRow(nextSelectedSongIndex);
		}
	}

	public void SelectPrev()
	{
		if (MP3List.Count == 0)
			return;

		var actualSelectedSongIndex = ActualSelectedSongIndex();

		if (actualSelectedSongIndex == -1 || MP3List.Count == 1)
		{
			SelectRow(0);
			return;
		}

		var selectionMode = tree.Selection.Mode;
		if ( (selectionMode == SelectionMode.Single) || (selectionMode == SelectionMode.Browse))
		{
			var nextSelectedSongIndex = actualSelectedSongIndex-1;
			if (nextSelectedSongIndex<0) nextSelectedSongIndex = MP3List.Count-1;
			SelectRow(nextSelectedSongIndex);
		}
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
		var x = 0;
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
    #endregion

}
