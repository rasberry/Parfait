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
				+  "Usage: "+nameof(Parfait)+" [options] (folder) [...]"
				+"\n Options:"
				+"\n  -t             Test mode. show info, but don't perform any actions"
				+"\n  -r             Recurse into sub-folders"
				+"\n  -hf            Include hidden files"
				+"\n  -hd            Include hidden folders"
				+"\n  -a             Enable automatic recovery"
				+"\n  -v             Verbose mode. show more info"
				+"\n  -l (log file)  Log output to this file"
				+"\n  -x (file)      Path to par2 executable"
				+"\n  -h / --help    Show this help"
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
				else if (curr == "-x" && ++a < args.Length) {
					Par2ExePath = args[a];
					if (!File.Exists(Par2ExePath ?? "")) {
						Log.Error("par2 executable not found");
						return false;
					}
				}
				else if (curr == "-l" && ++a < args.Length) {
					var fs = File.Open(args[a],FileMode.Create,FileAccess.Write,FileShare.Read);
					Par2LogFile = new StreamWriter(fs);
				}
				else if (curr == "-t") {
					DryRun = true;
					Verbose = true; //not much point in a dry run if you don't know what it's doing
				}
				else if (curr == "-v") {
					Verbose = true;
				}
				else if (curr == "-a") {
					AutoRecover = true;
				}
				else if (curr == "-r") {
					Recurse = true;
				}
				else if (curr == "-hf") {
					IncludeHiddenFiles = true;
				}
				else if (curr == "-hd") {
					IncludeHiddenFolders = true;
				}
				else if (!String.IsNullOrWhiteSpace(curr)) {
					RootFolders.Add(curr);
				}
			}

			if (RootFolders.Count < 1) {
				Log.Error("you must specify at least one folder");
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

		public static string Par2ExePath = "par2";
		public static StreamWriter Par2LogFile = null;
		public static List<string> RootFolders = new List<string>();
		public static bool Verbose = false;
		public static bool DryRun = false;
		public static bool AutoRecover = false;
		public static bool Recurse = false;
		public static bool IncludeHiddenFiles = false;
		public static bool IncludeHiddenFolders = false;
		public static string DataFolder = ".par2";
	}
}