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
				+"\n  -c (number)    Amount of corruption to tolerate (0-100 % of file size)"
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
				else if (curr == "-c" && ++a < args.Length) {
					if (!int.TryParse(args[a],out int Tolerance)
						|| Tolerance < 1 || Tolerance > 100)
					{
						Log.Error("Invalid tolerance amount "+args[a]);
						return false;
					}
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
				int len = RootFolders.Count;
				for(int f=0; f<len; f++)
				{
					string folder = RootFolders[f];
					string rooted = Path.GetFullPath(folder);
					if (!Directory.Exists(rooted)) {
						Log.Error("cannot find folder \""+rooted+"\"");
						return false;
					}
					if (0 != String.Compare(folder,rooted)) {
						RootFolders[f] = rooted;
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
		public static int Tolerance = 5;
	}
}