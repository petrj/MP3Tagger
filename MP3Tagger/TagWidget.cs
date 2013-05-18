using System;
using MP3Tagger;
using Gtk;

namespace MP3Tagger
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TagWidget : Gtk.Bin
	{
		private TAGBase _tag;

		public TAGBase Tag 
		{
			get 
			{
				return _tag;
			}
			set 
			{
				_tag = value;

				entryTitle.Text = value.Title == null ? String.Empty : value.Title;
				entryArtist.Text = value.Artist == null ? String.Empty : value.Artist;
				entryAlbum.Text = value.Album == null ? String.Empty : value.Album;
				entryYear.Text = value.Year == 0 ? String.Empty : value.Year.ToString();
				textViewComment.Buffer.Text = value.Comment == null ? String.Empty : value.Comment;

				if (TAGBase.ID3Genre.Length>value.Genre)
				{
					comboBoxGenre.Active = value.Genre;
				} else
				{
					comboBoxGenre.Active = -1;
				}
			}
		}

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
