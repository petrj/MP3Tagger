
// This file has been generated by the GUI designer. Do not modify.
namespace MP3Tagger
{
	public partial class ProgressBarWindow
	{
		private global::Gtk.Fixed @fixed;
		
		private global::Gtk.ProgressBar progressBar;
		
		private global::Gtk.Label label;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MP3Tagger.ProgressBarWindow
			this.Name = "MP3Tagger.ProgressBarWindow";
			this.Title = global::Mono.Unix.Catalog.GetString ("Please wait ...");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Container child MP3Tagger.ProgressBarWindow.Gtk.Container+ContainerChild
			this.@fixed = new global::Gtk.Fixed ();
			this.@fixed.HeightRequest = 50;
			this.@fixed.Name = "fixed";
			this.@fixed.HasWindow = false;
			// Container child fixed.Gtk.Fixed+FixedChild
			this.progressBar = new global::Gtk.ProgressBar ();
			this.progressBar.WidthRequest = 320;
			this.progressBar.Name = "progressBar";
			this.progressBar.Text = "";
			this.progressBar.PulseStep = 0.01D;
			this.@fixed.Add (this.progressBar);
			global::Gtk.Fixed.FixedChild w1 = ((global::Gtk.Fixed.FixedChild)(this.@fixed [this.progressBar]));
			w1.X = 16;
			w1.Y = 32;
			// Container child fixed.Gtk.Fixed+FixedChild
			this.label = new global::Gtk.Label ();
			this.label.Name = "label";
			this.label.LabelProp = global::Mono.Unix.Catalog.GetString ("Title");
			this.@fixed.Add (this.label);
			global::Gtk.Fixed.FixedChild w2 = ((global::Gtk.Fixed.FixedChild)(this.@fixed [this.label]));
			w2.X = 14;
			w2.Y = 7;
			this.Add (this.@fixed);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 366;
			this.DefaultHeight = 99;
			this.Show ();
		}
	}
}
