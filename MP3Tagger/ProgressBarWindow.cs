using System;

namespace MP3Tagger
{
	public partial class ProgressBarWindow : Gtk.Window
	{
		private double _percents = 0;
		private Gtk.Window _mainWin = null;

		public ProgressBarWindow (Gtk.Window parent) : 
				base(Gtk.WindowType.Toplevel)
		{
			_mainWin = parent;
			this.Build ();
			this.Shown += delegate { Dialogs.CenterChildToParent(parent,this); };
		}

		public string Description
		{
			set { this.label.LabelProp = value; }
			get { return this.label.LabelProp; }
		}		

		public double Percents 
		{
			get { return _percents;	}
			set 
			{
				_percents = value; 
				this.progressBar.Fraction = _percents / 100;
				this.progressBar.Text = Math.Round(_percents,0).ToString()+" %";
			}
		}


	}
}


