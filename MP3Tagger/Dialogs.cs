using System;
using Gtk;

namespace MP3Tagger
{
	public static class Dialogs
	{

		public static string OpenDirectoryDialog(string message)
		{
			string dir = null;

			 var fc = new Gtk.FileChooserDialog(message,
                null,
                FileChooserAction.SelectFolder,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept);

	        if (fc.Run() == (int)ResponseType.Accept)
	        {
				dir = fc.Filename;
	        }
        	fc.Destroy();

			return dir;
		}

		public static string OpenFileDialog(string message)
		{
			string fileName = null;

			 var fc = new Gtk.FileChooserDialog(message,
                null,
                FileChooserAction.Open,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept);

	        if (fc.Run() == (int)ResponseType.Accept)
	        {
				fileName = fc.Filename;
	        }
        	fc.Destroy();

			return fileName;
		}

		public static ResponseType QuestionDialog(string message)
		{
			MessageDialog md = new MessageDialog (null, 
	                                  DialogFlags.DestroyWithParent,
	                              	  MessageType.Question, 
	                                  ButtonsType.OkCancel, message);

			var result = (ResponseType)md.Run ();
			md.Destroy();

			return result;
		}

		public static bool ConfirmDialog(string message)
		{
			MessageDialog md = new MessageDialog (null, 
	                                  DialogFlags.DestroyWithParent,
	                              	  MessageType.Question, 
	                                  ButtonsType.YesNo, message);

			var result = (ResponseType)md.Run ();
			md.Destroy();

			return result == ResponseType.Yes;
		}

		public static void InfoDialog(string message,MessageType msgType = MessageType.Info )
		{
			MessageDialog md = new MessageDialog (null, 
	                                  DialogFlags.DestroyWithParent,
	                              	  msgType, 
	                                  ButtonsType.Close, message);
				md.Run();
				md.Destroy();
		}
	}
}

