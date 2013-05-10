using System;

namespace MP3Tagger
{
	public partial class ProgressBarWindow : Gtk.Window
	{
		private double _percents = 0;

		public ProgressBarWindow () : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
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


