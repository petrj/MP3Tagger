using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Logger
{	
	public static class Logger
	{		
		private static string logFileName = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"app.log");

		public static string LogFileName 
		{
			get 
			{
				return logFileName;
			}
			set 
			{
				logFileName = value;
			}
		}		
		
		public static void WriteToLog(string message)
		{
			WriteToLog(message,null);
		}		
		
		public static void WriteToLog(List<string> lines)
		{					
			foreach (string line in lines)
			{
				WriteToLog(line);
			}
		}
		
		public static void WriteToLog(string message,Exception ex)
		{					
			message = "[" + DateTime.Now.ToString("yyyy-MM-dd--HH:mm:ss") + "] "+  message;
			
			if (ex != null)
			{
				message += "---> Error: " + ex.ToString();
			}
			
			//Console.WriteLine(message);
	
			//using (StreamWriter sw = File.AppendText(logFileName))
			using(FileStream fs = new FileStream(
			                                      	logFileName,
			                                      FileMode.Append, 
			                                      FileAccess.Write,
			                                     FileShare.Read)
			      )
			{
				
				
				using(StreamWriter sw = new StreamWriter(fs,System.Text.Encoding.UTF8))
				{
					sw.WriteLine(message);
					sw.Close();
				}
				fs.Close();
			}
		}
		
		
	}
}
