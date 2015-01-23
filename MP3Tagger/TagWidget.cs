using System;
using MP3Tagger;
using Gtk;

namespace MP3Tagger
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TagWidget : Gtk.Bin
	{
		private TAGBase _tag;
		
		private string _infoMaskWinTitle;
		private string _infoMaskWinText;

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

				entryTrackNumber.Text = value.TrackNumber == 0 ? String.Empty : value.TrackNumber.ToString();

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
				_tag.Title =  entryTitle.Text;
				_tag.Artist = entryArtist.Text;
				_tag.Album = entryAlbum.Text;
				if (comboBoxGenre.Active>=0 && comboBoxGenre.Active<TAGBase.ID3Genre.Length)
				{
						_tag.Genre = (byte)comboBoxGenre.Active;
				}
				else 	_tag.Genre = 255;

				int y;
				if (int.TryParse(entryYear.Text,out y))
				{
						_tag.Year = y;
				} else
						_tag.Year = 0;

				byte tn;
				if (byte.TryParse(entryTrackNumber.Text,out tn))
				{
					_tag.TrackNumber = tn;
				} else _tag.TrackNumber = 0;

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

		public void ApplyLanguage(Language lng)
		{	
			labelTitle.LabelProp = lng.Translate("Title");
			labelArtist.LabelProp = lng.Translate("Artist");
			labelComment.LabelProp = lng.Translate("Comment");
			labelAlbum.LabelProp = lng.Translate("Album");
			labelYear.LabelProp = lng.Translate("Year");
			labelGenre.LabelProp = lng.Translate("Genre");
			labelTrackNumber.LabelProp = lng.Translate("TrackNumber");
			
			_infoMaskWinTitle = lng.Translate("MaskHelpTitle");
			_infoMaskWinText = lng.Translate("MaskHelp");
		}

		protected void OnButtonMaskHelpClicked (object sender, EventArgs e)
		{
			var help = _infoMaskWinTitle;
			help += System.Environment.NewLine;
			help += System.Environment.NewLine;
			help += _infoMaskWinText;

			Dialogs.InfoDialog(help);
		}
	}
}

