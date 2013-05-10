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
	    progressWin.Hide();

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
        progressWin.Description = e.Text;

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

		if (_treeView1Data.TreeIters.Count>0)
		{
			tree.Selection.UnselectAll();
			tree.Selection.SelectIter( _treeView1Data.TreeIters[0]);
		}

		//tree.Selection.SelectPath( tree.selectio

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
		var selectedSongs = GetSelectedSongs();
		if (selectedSongs.Count>0)
		{
			editWindow.Songs = selectedSongs;
			editWindow.Show();
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
    #endregion

}
