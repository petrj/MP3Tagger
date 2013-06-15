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

		public Language ():this(DefaultFlag)
		{

		}

		public void LoadFromFile(string fileName)
		{
			Clear();

			var doc = new XmlDocument();
			
	        doc.Load(fileName);

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

		public Language (string flag)
		{
			Flag = flag;

			string lngPath = Path.Combine(global::System.AppDomain.CurrentDomain.BaseDirectory,"lng/");
			var fName = Path.Combine(lngPath,flag+".xml");

			if (File.Exists(fName))
			{
				LoadFromFile(fName);
			}
		}

		public string Translate(string key, string defaultValue)
		{
			if (this.ContainsKey(key))
			{
				return "*" + this[key];
			} else
				return "*" + defaultValue;
		}

		public string Translate(string key)
		{
			return Translate(key,key);
		}
	}
}

