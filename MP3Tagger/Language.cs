using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;

namespace MP3Tagger
{
	public class Language : Dictionary<string,string>
	{
		public static string DefaultFlag = "en";

		public string Flag { get; set; }
		public string Description { get; set; }

		public static string LanguagePath
		{
			get
			{
				return Path.Combine(global::System.AppDomain.CurrentDomain.BaseDirectory,"lng/");
			}
		}

		public static List<Language> AvailableLanguages
		{
			get
			{
				var res  = new List<Language> ();
				var xmls = Directory.GetFiles(LanguagePath,"*.xml");
				foreach (var xml in xmls)
				{
					var lng = new Language();
					lng.LoadFromFile(xml);
					res.Add(lng);
				}

				return res;
			}
		}

		public void LoadByFlag(string flag)
		{
			LoadFromFile(Path.Combine(LanguagePath,flag+".xml"));
		}

		public void LoadFromFile(string fileName)
		{
			Clear();

			var doc = new XmlDocument();
			
	        doc.Load(fileName);

			var mainNode = doc.SelectSingleNode("dict");
			for (var k=0;k<mainNode.Attributes.Count;k++)
			{
				var attr = mainNode.Attributes[k];
				if (attr.Name == "description") 
				{
					Description = attr.Value;
				}
				if (attr.Name == "language") 
				{
					Flag = attr.Value;
				}
			}



        	var xpath = "dict/w";
			var nodes = doc.SelectNodes(xpath);				 

			for (var i=0; i<nodes.Count;i++)
			{
				var node = nodes[i];
				var k ="";
				var v ="";

				for (var j=0;j<node.Attributes.Count;j++)
				{
					var attr = node.Attributes[j];
					if (attr.Name == "key") 
					{
						k = attr.Value;
						if (v == "") v = k; // key to value if value is empty
					}
					if (attr.Name == "value" && !String.IsNullOrEmpty(attr.Value)) v = attr.Value;
				}

				if (k != "" && !this.ContainsKey(k))
				{
					Add(k,/*"*" + */v);
				}
			}
		}

		public Language ()
		{

		}

		public Language (string flag)
		{
			Flag = flag;

			var fName = Path.Combine(LanguagePath,flag+".xml");

			if (File.Exists(fName))
			{
				LoadFromFile(fName);
			}
		}

		public string Translate(string key, string defaultValue)
		{
			if (this.ContainsKey(key))
			{
				return "" + this[key];
			} else
				return "" + defaultValue;
		}

		public string Translate(string key)
		{
			return Translate(key,key);
		}
	}
}

