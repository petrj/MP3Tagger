using System;
using System.Collections.Generic;
using Gtk;

namespace Grid
{
	public class TreeViewData
	{
		private List<Gtk.TreeViewColumn> _columns = new List<Gtk.TreeViewColumn>();
		private Dictionary<int,List<object>> _data = new Dictionary<int,List<object>>();
		private List<Gtk.TreeIter> _treeIters;

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

		public void CreateTreeViewColumns(Gtk.TreeView tree)
		{
			var columnPosition=0;
			foreach (var column in Columns)
			{
				tree.AppendColumn (column);		 

				column.AddAttribute(column.Data["cellRenderer"] as Gtk.CellRendererText, column.Data["cellType"] as string,columnPosition);
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
			TreeIters = new List<TreeIter>();

			foreach (var row in Data.Keys)
			{
				var treeIter = listStore.AppendValues (Data[row].ToArray());
				TreeIters.Add( treeIter );
			}
	 
			return listStore;				
		}

		public List<Gtk.TreeIter> TreeIters
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
	}
}

