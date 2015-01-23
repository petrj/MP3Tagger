using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.XPath;

namespace MP3Tagger
{
	
	public static class Configuration
	{		
		# region private fields
		
		private static List<string> _runApplications = new List<string>();
		private static string _defaultLanguage = "en";
		private static string _defaultTAG1Encoding = "iso-8859-1";
	
		#endregion
		
		# region properties

		public static string DefaultTAG1Encoding
		{
			get 
			{
				return _defaultTAG1Encoding;
			}
			set 
			{
				_defaultTAG1Encoding = value;
			}			
		}

		public static string DefaultLanguage
		{
			get 
			{
				return _defaultLanguage;
			}
			set 
			{
				_defaultLanguage = value;
			}			
		}
	
		public static List<string> RunApplications
		{
			get 
			{
				return _runApplications;
			}
			set 
			{
				_runApplications = value;
			}			
		}

		#endregion
		
		private static string GetNodeValue(XPathNavigator navigator,string path)
		{
			string val = String.Empty;
			
			XPathNodeIterator iterator = navigator.Select(path);
			while (iterator.MoveNext()) 
			{
				val = iterator.Current.Value;				
			}
			
			return val;
		}		

		public static string CorrectPath(string path)
		{
			if (path.Length == 0)
				return String.Empty;
			
			if (!path.EndsWith( System.IO.Path.DirectorySeparatorChar.ToString()))
			{
				path+= System.IO.Path.DirectorySeparatorChar;
			}
			
			return path;
		}
		
		private static List<string> LoadRunApplications(XPathNavigator navigator,string path)
		{
			List<string> apps = new List<string>();
			
			XPathNodeIterator iterator = navigator.Select(path);
			while (iterator.MoveNext()) 
			{
				apps.Add(iterator.Current.Value);
				Logger.Logger.WriteToLog("Adding supported run application:" + iterator.Current.Value);
			}			
			
			return apps;
		}

		private static bool TryParseBoolean(string value, bool defaultValue = false)
		{
			bool resultValue = defaultValue;

			bool tryBool = false;
			if (Boolean.TryParse(value,out tryBool))
			{
				resultValue = tryBool;
			} else
			if ((value == "1") || (value == "A") || (value == "ANO")  || (value == "Y")  || (value == "YES") )
			{
				resultValue = true;
			} else 
			if ((value == "0") || (value == "N") || (value == "NE")  || (value == "NO") )
			{
				resultValue = false;
			}

			return resultValue;
		}

		/// <summary>
		/// Loads the configuration.
		/// </summary>
		/// <param name='path'>
		/// Path to xml config file
		/// if path equals null, loding from execution directory from Configuration.xml
		/// </param>
		public static void LoadConfiguration(string path = null)
		{			
			Logger.Logger.WriteToLog("Loading configuration");

			if (path == null)
			{
				path = Path.Combine(global::System.AppDomain.CurrentDomain.BaseDirectory,"Configuration.xml");
			}

			if (System.IO.File.Exists(path))
			{
				try
				{
					XPathNavigator nav = new XPathDocument (path).CreateNavigator ();

					RunApplications = LoadRunApplications(nav,"//MP3Tagger/Configuration/RunApplications/App");
					
					DefaultLanguage  = GetNodeValue(nav,"//MP3Tagger/Configuration/DefaultLanguage");
					DefaultTAG1Encoding = GetNodeValue(nav,"//MP3Tagger/Configuration/DefaultTAG1Encoding");

					Logger.Logger.WriteToLog("Configuration loaded:");
				}
				catch(Exception ex)
				{
					Logger.Logger.WriteToLog(String.Format("Error while loading configuration",path),ex);
				}
			} else
			{
				Logger.Logger.WriteToLog(String.Format("Error - configuration not found ({0})",path));
			}
		}		

		
	}
}
