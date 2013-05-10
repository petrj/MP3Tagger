using System;
using MP3Tagger;
using Gtk;

namespace MP3Tagger
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TagWidget : Gtk.Bin
	{
		public TagWidget ()
		{
			this.Build ();

			foreach (var genreText in TAGBase.ID3Genre)
			{
				comboBoxGenre.AppendText(genreText);
			}
		}
	}
}

