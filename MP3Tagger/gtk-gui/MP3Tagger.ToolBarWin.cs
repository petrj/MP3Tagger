
// This file has been generated by the GUI designer. Do not modify.
namespace MP3Tagger
{
	public partial class ToolBarWin
	{
		private global::Gtk.UIManager UIManager;
		private global::Gtk.Action actionClose;
		private global::Gtk.Action actionAddSingleFile;
		private global::Gtk.Action actionAddFolder;
		private global::Gtk.Action actionRemoveSelected;
		private global::Gtk.Action actionRemoveAll;
		private global::Gtk.Action actionSelectAll;
		private global::Gtk.Action actionUnselectAll;
		private global::Gtk.Action actionAddFolderRecursive;
		private global::Gtk.Fixed @fixed;
		private global::Gtk.Toolbar toolbar;
		
		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MP3Tagger.ToolBarWin
			this.UIManager = new global::Gtk.UIManager ();
			global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
			this.actionClose = new global::Gtk.Action ("actionClose", global::Mono.Unix.Catalog.GetString ("Close"), null, "gtk-close");
			this.actionClose.ShortLabel = global::Mono.Unix.Catalog.GetString ("Close");
			w1.Add (this.actionClose, null);
			this.actionAddSingleFile = new global::Gtk.Action ("actionAddSingleFile", global::Mono.Unix.Catalog.GetString ("Add File"), null, "gtk-new");
			this.actionAddSingleFile.ShortLabel = global::Mono.Unix.Catalog.GetString ("Add File");
			w1.Add (this.actionAddSingleFile, null);
			this.actionAddFolder = new global::Gtk.Action ("actionAddFolder", global::Mono.Unix.Catalog.GetString ("Add folder"), null, "gtk-open");
			this.actionAddFolder.ShortLabel = global::Mono.Unix.Catalog.GetString ("Add folder");
			w1.Add (this.actionAddFolder, null);
			this.actionRemoveSelected = new global::Gtk.Action ("actionRemoveSelected", global::Mono.Unix.Catalog.GetString ("Selected"), null, "gtk-dnd");
			this.actionRemoveSelected.ShortLabel = global::Mono.Unix.Catalog.GetString ("Selected");
			w1.Add (this.actionRemoveSelected, null);
			this.actionRemoveAll = new global::Gtk.Action ("actionRemoveAll", global::Mono.Unix.Catalog.GetString ("All"), null, "gtk-dnd-multiple");
			this.actionRemoveAll.ShortLabel = global::Mono.Unix.Catalog.GetString ("All");
			w1.Add (this.actionRemoveAll, null);
			this.actionSelectAll = new global::Gtk.Action ("actionSelectAll", global::Mono.Unix.Catalog.GetString ("Select all"), null, "gtk-select-all");
			this.actionSelectAll.ShortLabel = global::Mono.Unix.Catalog.GetString ("Select all");
			w1.Add (this.actionSelectAll, null);
			this.actionUnselectAll = new global::Gtk.Action ("actionUnselectAll", global::Mono.Unix.Catalog.GetString ("Unselect all"), null, "gtk-remove");
			this.actionUnselectAll.ShortLabel = global::Mono.Unix.Catalog.GetString ("Unselect all");
			w1.Add (this.actionUnselectAll, null);
			this.actionAddFolderRecursive = new global::Gtk.Action ("actionAddFolderRecursive", global::Mono.Unix.Catalog.GetString ("Add folder rec."), null, "gtk-directory");
			this.actionAddFolderRecursive.ShortLabel = global::Mono.Unix.Catalog.GetString ("Add folder rec.");
			w1.Add (this.actionAddFolderRecursive, null);
			this.UIManager.InsertActionGroup (w1, 0);
			this.AddAccelGroup (this.UIManager.AccelGroup);
			this.Name = "MP3Tagger.ToolBarWin";
			this.Title = global::Mono.Unix.Catalog.GetString ("Add");
			this.Icon = global::Stetic.IconLoader.LoadIcon (this, "gtk-add", global::Gtk.IconSize.Menu);
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			this.Modal = true;
			this.Resizable = false;
			this.AllowGrow = false;
			// Container child MP3Tagger.ToolBarWin.Gtk.Container+ContainerChild
			this.@fixed = new global::Gtk.Fixed ();
			this.@fixed.Name = "fixed";
			this.@fixed.HasWindow = false;
			// Container child fixed.Gtk.Fixed+FixedChild
			this.UIManager.AddUiFromString ("<ui><toolbar name='toolbar'><toolitem name='actionAddSingleFile' action='actionAddSingleFile'/><toolitem name='actionAddFolder' action='actionAddFolder'/><toolitem name='actionAddFolderRecursive' action='actionAddFolderRecursive'/><toolitem name='actionRemoveSelected' action='actionRemoveSelected'/><toolitem name='actionRemoveAll' action='actionRemoveAll'/><toolitem name='actionSelectAll' action='actionSelectAll'/><toolitem name='actionUnselectAll' action='actionUnselectAll'/><separator/><toolitem name='actionClose' action='actionClose'/></toolbar></ui>");
			this.toolbar = ((global::Gtk.Toolbar)(this.UIManager.GetWidget ("/toolbar")));
			this.toolbar.Name = "toolbar";
			this.toolbar.ShowArrow = false;
			this.@fixed.Add (this.toolbar);
			global::Gtk.Fixed.FixedChild w2 = ((global::Gtk.Fixed.FixedChild)(this.@fixed [this.toolbar]));
			w2.X = 2;
			w2.Y = 11;
			this.Add (this.@fixed);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 804;
			this.DefaultHeight = 94;
			this.Show ();
			this.Shown += new global::System.EventHandler (this.OnShown);
			this.actionClose.Activated += new global::System.EventHandler (this.OnCloseActionActivated);
			this.actionAddSingleFile.Activated += new global::System.EventHandler (this.OnActionAddFileActivated);
			this.actionAddFolder.Activated += new global::System.EventHandler (this.OnActionAddFolderActivated);
			this.actionRemoveSelected.Activated += new global::System.EventHandler (this.OnActionRemoveSelectedActivated);
			this.actionRemoveAll.Activated += new global::System.EventHandler (this.OnActionRemoveAllActivated);
			this.actionSelectAll.Activated += new global::System.EventHandler (this.OnActionSelectAllActivated);
			this.actionUnselectAll.Activated += new global::System.EventHandler (this.OnActionUnselectAllActivated);
			this.actionAddFolderRecursive.Activated += new global::System.EventHandler (this.OnActionAddFolderREcursiveActivated);
		}
	}
}
