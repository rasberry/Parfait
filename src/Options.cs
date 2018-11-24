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
				+"\n  -t             test mode. show info, but don't perform any actions"
				+"\n  -v             verbose mode. show more info"
				+"\n Repair Options:"
				+"\n  -Ra            auto repair any file that is identified as 'CanRepair'"
				+"\n  -RR            (!) repair files instead of re-generating the par2 info. NOTE: this will revert any changes made since par2 info was generated"
				+"\n  -Rm (file)     manually repair a file. add additional -Rm options to repair multiple files (or use -Rl)"
				+"\n  -Rl (listfile) manually repair list of files. files are specified in a file (one file per line)"
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
					if (String.IsNullOrWhiteSpace(args[a])) {
						Log.Error("invalid data folder");
						return false;
					} else {
						// Log.Debug("a = "+args[a]);
						DataFolder = Path.GetFullPath(args[a]);
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
				else if (curr == "-t") {
					DryRun = true;
					Verbose = true; //not much point in a dry run if you don't know what it's doing
				}
				else if (curr == "-v") {
					Verbose = true;
				}
				else if (curr == "-Ra") {
					AutoRepair = true;
				}
				else if (curr == "-RR") {
					RevertRepair = true;
				}
				else if (curr == "-Rm" && ++a < args.Length) {
					RepairFiles.Add(args[a]);
				}
				else if (curr == "-Rl" && ++a < args.Length) {
					var fs = File.Open(args[a],FileMode.Create,FileAccess.Write,FileShare.Read);
					RepairFiles.AddRange(File.ReadLines(args[a]));
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
		public static bool Verbose = false;
		public static bool DryRun = false;
		public static bool AutoRepair = false;
		public static bool RevertRepair = false;
		public static List<string> RepairFiles = new List<string>();
	}
}