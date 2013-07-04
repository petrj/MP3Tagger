using System;

namespace MP3Tagger
{
	public partial class ProgressBarWindow : Gtk.Window
	{
		private double _percents = 0;
		private Gtk.Window _mainWin = null;

		public void CenterToParent()
		{
			if (_mainWin==null)
				return;

			int parentX = 0;
			int parentY = 0;
			int parentW = 0;
			int parentH = 0;

			_mainWin.GetPosition(out parentX,out parentY);
			_mainWin.GetSize(out parentW,out parentH);

			int w = 0;
			int h = 0;
			GetSize(out w,out h);

			var x = parentX + Convert.ToInt32( (parentW-w) / 2);
			var y = parentY + Convert.ToInt32( (parentH-h) / 2);

			if (x<=0) x =0;
			if (y<=0) y =0;

			Move(x,y);
			this.KeepAbove = true;
		}

		public ProgressBarWindow (Gtk.Window parent) : 
				base(Gtk.WindowType.Toplevel)
		{
			_mainWin = parent;
			this.Build ();
			this.Shown += delegate { CenterToParent(); };
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


