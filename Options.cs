using System;
using System.Collections.Generic;
using System.IO;

namespace Parfait
{
	public static class Options
	{
		static void Usage()
		{
			Log.Message(""
				+  "Usage: "+nameof(Parfait)+" [options] (root folder) [...]"
				+"\n Options:"
				+"\n  -d (folder)    location of par2 data folder"
				+"\n  -x (file)      path to par2 executable"
				+"\n  -l (log file)  log par2 commands and output to this file"
			);
		}

		public static bool ParseArgs(string[] args)
		{
			if (args.Length < 1) {
				Usage();
				return false;
			}

			for(int a=0; a<args.Length; a++)
			{
				string curr = args[a];
				if (curr == "-h" || curr == "--help") {
					Usage();
					return false;
				}
				else if (curr == "-d" && ++a < args.Length) {
					DataFolder = args[a];
					if (String.IsNullOrWhiteSpace(DataFolder)) {
						Log.Error("invalid data folder");
						return false;
					}
				}
				else if (curr == "-x" && ++a < args.Length) {
					Par2Path = args[a];
					if (!File.Exists(Par2Path ?? "")) {
						Log.Error("par2 executable not found");
						return false;
					}
				}
				else if (curr == "-l" && ++a < args.Length) {
					var fs = File.Open(args[a],FileMode.Create,FileAccess.Write,FileShare.Read);
					Par2LogFile = new StreamWriter(fs);
				}
				else if (!String.IsNullOrWhiteSpace(curr)) {
					RootFolders.Add(curr);
				}
			}

			if (RootFolders.Count < 1) {
				Log.Error("you must specify at least one root folder");
				return false;
			}
			else {
				foreach(string folder in RootFolders) {
					if (!Directory.Exists(folder)) {
						Log.Error("cannot find folder \""+folder+"\"");
						return false;
					}
				}
			}

			return true;
		}

		public static string DataFolder = null;
		public static string Par2Path = "par2";
		public static StreamWriter Par2LogFile = null;
		public static List<string> RootFolders = new List<string>();
	}
}