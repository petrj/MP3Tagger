using System;
using System.Collections.Generic;
using Gtk;

namespace Grid
{
	public class TreeViewData
	{
		public Gtk.TreeView Tree { get; set; }

		#region private fields

		private List<Gtk.TreeViewColumn> _columns = new List<Gtk.TreeViewColumn>();

		// key .. int: 0 .. (count-1)
		// value .. List<> of any object
		private Dictionary<int,List<object>> _data = new Dictionary<int,List<object>>();

		// key .. index of Data value
		// value .. TreeIter object
		private Dictionary<int,Gtk.TreeIter> _treeIters;

		#endregion

		public TreeViewData(Gtk.TreeView treeView)
		{
			Tree = treeView;
		}

		#region public methods

		/// <summary>
		/// Experimental function 
		/// </summary>
		/// <returns>
		/// The combo column.
		/// </returns>
		/// <param name='name'>
		/// Name.
		/// </param>
		/// <param name='editable'>
		/// Editable.
		/// </param>
		public Gtk.TreeViewColumn AppendComboColumn(string name,bool editable = false)
		{
			var newColumn = new Gtk.TreeViewColumn ();
            newColumn.Title = name;
				 
			var cellRenderer = new Gtk.CellRendererCombo();
			cellRenderer.Editable = editable;


			var listStore = new Gtk.ListStore (typeof(string));
			var treeIter = listStore.AppendValues( new string[] {"Value 1","Value 2","Value 3"} );

			cellRenderer.Model = listStore; 
	 
			newColumn.PackStart (cellRenderer, true);

			Columns.Add(newColumn);

			newColumn.Data["cellRenderer"] = cellRenderer;
			newColumn.Data["cellType"] = "text";
			newColumn.Data["cellTypeOf"] = typeof(string);

			return newColumn;
		}

		public Gtk.TreeViewColumn AppendCheckBoxColumn(string name,bool editable = false)
		{
			var newColumn = new Gtk.TreeViewColumn ();
            newColumn.Title = name;
				 
			var cellRenderer = new Gtk.CellRendererToggle();
			cellRenderer.Activatable = editable;
	 
			newColumn.PackStart (cellRenderer, true);	 

			Columns.Add(newColumn);

			newColumn.Data["cellRenderer"] = cellRenderer;
			newColumn.Data["cellType"] = "active";
			newColumn.Data["cellTypeOf"] = typeof(bool);

			return newColumn;
		}


		public Gtk.TreeViewColumn AppendStringColumn(string name,bool editable = false)
		{
			var newColumn = new Gtk.TreeViewColumn ();
            newColumn.Title = name;
				 
			var cellRenderer = new Gtk.CellRendererText ();
			cellRenderer.Editable = editable;
	 
			newColumn.PackStart (cellRenderer, true);	 

			Columns.Add(newColumn);

			newColumn.Data["cellRenderer"] = cellRenderer;
			newColumn.Data["cellType"] = "text";
			newColumn.Data["cellTypeOf"] = typeof(string);

			return newColumn;
		}

		public void CreateTreeViewColumns()
		{
			var columnPosition=0;
			foreach (var column in Columns)
			{
				Tree.AppendColumn (column);		 

				column.AddAttribute(column.Data["cellRenderer"] as Gtk.CellRenderer, column.Data["cellType"] as string,columnPosition);
				columnPosition++;
			}
		}

		public int AppendData(List<object> values)
		{
			var rowIndex = Data.Keys.Count;
			Data.Add(rowIndex, values);
			return rowIndex;
		}

		public Gtk.ListStore  CreateTreeViewListStore()
		{		 
			var tps = new Type[Columns.Count];
			for (var i=0;i<Columns.Count;i++)
			{
				tps[i] = (Type)Columns[i].Data["cellTypeOf"];
			}

			var listStore = new Gtk.ListStore (tps);
			TreeIters = new Dictionary<int,TreeIter>();

			foreach (var row in Data.Keys)
			{
				var treeIter = listStore.AppendValues (Data[row].ToArray());
				TreeIters.Add(row, treeIter );
			}
	 
			return listStore;				
		}

		#endregion

		#region properties

		public Dictionary<int,Gtk.TreeIter> TreeIters
		{
			get { return _treeIters;}
			set { _treeIters = value; }
		}

		public List<Gtk.TreeViewColumn> Columns 
		{
			get 
			{
				return _columns;
			}
			set 
			{
				_columns = value;
			}
		}

		public Dictionary<int, List<object>> Data 
		{
			get 
			{
				return _data;
			}
		}

		#endregion
	}
}

