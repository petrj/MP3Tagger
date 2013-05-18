using System;
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

		foreach (var colName in TAGBase.AllCollumnNames)
		{
			_treeView1Data.AppendStringColumn(colName,true);
			_treeView2Data.AppendStringColumn(colName);
		}

		_treeView1Data.AppendComboColumn("Genre*",true);
		_treeView2Data.AppendComboColumn("Genre*",true);

		_treeView1Data.AppendCheckBoxColumn("Ch",true);
		_treeView2Data.AppendCheckBoxColumn("Ch",false);

		//tree.Selection.Mode = SelectionMode.Multiple;
		//tree.Selection.Mode = SelectionMode.Extended;
		tree.Selection.Mode = SelectionMode.Browse;
		//tree.Selection.Mode = SelectionMode.Browse;
		//tree.Selection.Mode = SelectionMode.Single;

		//tree.Selection.Mode = SelectionMode.

		editWindow = new SongDetail(this);
		editWindow.Hide();
			
		this.Show();
	
	}

	#endregion

	#region methods

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

        
        _treeView1Data.CreateTreeViewColumns();
		_treeView2Data.CreateTreeViewColumns();

        foreach (var song in MP3List)
        {
            //ViewData.AppendData(new List<object>() { "Show must go on", "Queen", "Best of" });

			var tree1Values = song.ID3v1.ValuesAsOLbjecttList(TAGBase.AllCollumnNames);
			var tree2Values = song.ID3v2.ValuesAsOLbjecttList(TAGBase.AllCollumnNames);

			// combo
			tree1Values.Add("Value 1");
			tree2Values.Add("");

			// checkbox
			tree1Values.Add(song.ID3v1.Changed);
			tree2Values.Add(true);

			// add data values to tree
            _treeView1Data.AppendData(tree1Values);
			_treeView2Data.AppendData(tree2Values);
        }

        tree.Model = _treeView1Data.CreateTreeViewListStore();
		tree2.Model = _treeView2Data.CreateTreeViewListStore();

		SelectRow(0);

        Show();
	}

	#endregion

    #region properties

	public TreeViewData SelectedTreeViewData
	{
		get
		{
			if (tree.IsFocus)
			{
				return _treeView1Data;
			} else
			{
				return _treeView2Data;
			}
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

	private void EditSelectedSongs(TreeViewData data)
	{
		var actualSelectedSong = ActualSelectedSong(data);

		if (actualSelectedSong == null)
		{
			InfoDialog("No row selected",MessageType.Warning);
			return;
		}

		editWindow.CurrentSong = actualSelectedSong;
		editWindow.Show();
	}

	public void SelectRow(int index)
	{
		if (index>=0 && index<=MP3List.Count-1)		
		{
			tree.Selection.UnselectAll();
			tree.Selection.SelectIter( _treeView1Data.TreeIters[index]);

			tree2.Selection.UnselectAll();
			tree2.Selection.SelectIter( _treeView2Data.TreeIters[index]);

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

		int actualSelectedSongIndex;
		var actualTreeViewData = SelectedTreeViewData;

		actualTreeViewData = SelectedTreeViewData;
		actualSelectedSongIndex = ActualSelectedSongIndex(actualTreeViewData);			

		if (actualSelectedSongIndex == -1 || MP3List.Count == 1)
		{
			SelectRow(0);
			return;
		}

		var selectionMode = actualTreeViewData.Tree.Selection.Mode;
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

		int actualSelectedSongIndex;
		var actualTreeViewData = SelectedTreeViewData;

		actualTreeViewData = SelectedTreeViewData;
		actualSelectedSongIndex = ActualSelectedSongIndex(actualTreeViewData);			

		if (actualSelectedSongIndex == -1 || MP3List.Count == 1)
		{
			SelectRow(0);
			return;
		}

		var selectionMode = actualTreeViewData.Tree.Selection.Mode;
		if ( (selectionMode == SelectionMode.Single) || (selectionMode == SelectionMode.Browse))
		{
			var nextSelectedSongIndex = actualSelectedSongIndex-1;
			if (nextSelectedSongIndex<0) nextSelectedSongIndex = MP3List.Count-1;
			SelectRow(nextSelectedSongIndex);
		}
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

	protected void OnEditActionActivated (object sender, EventArgs e)
	{
		EditSelectedSongs(_treeView1Data);
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
			EditSelectedSongs(SelectedTreeViewData);
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
