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

		public void ApplyChanges()
		{
				_tag.Title = entryTitle.Text;
				_tag.Artist = entryArtist.Text;
				_tag.Album = entryAlbum.Text;
				if (comboBoxGenre.Active>=0 && comboBoxGenre.Active<TAGBase.ID3Genre.Length && TAGBase.ID3Genre.Length<=255)
				{
					_tag.Genre = (byte)comboBoxGenre.Active;
				}
				else 	_tag.Genre = 0;

				int y;
				if (int.TryParse(entryYear.Text,out y))
				{
					_tag.Year = y;
				}
				_tag.Year = 0;

				_tag.Comment = textViewComment.Buffer.Text;
	
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

