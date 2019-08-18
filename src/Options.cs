using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Parfait
{
	public static class Options
	{
		static void Usage()
		{
			Log.Message(""
				+WL(02,  "Usage: "+nameof(Parfait)+" [options] (folder) [...]")
				+WL(04,"\n Options:")
				+WL(04,"\n  -t             Test mode. show info, but don't perform any actions")
				+WL(04,"\n  -r             Recurse into sub-folders")
				+WL(04,"\n  -hf            Include hidden files")
				+WL(04,"\n  -hd            Include hidden folders")
				+WL(04,"\n  -c (number)    Amount of corruption to tolerate (0-100 % of file size) (defaults to 5)")
				+WL(04,"\n  -a             Enable automatic recovery")
				+WL(04,"\n  -n             Don't automatically create recovery files or .par2 folders")
				+WL(04,"\n  -v             Verbose mode. show more info")
				+WL(04,"\n  -l (log file)  Log output to this file (includes par2 commands)")
				+WL(04,"\n  -x (file)      Path to par2 executable")
				+WL(04,"\n  -h / --help    Show this help")
				+WL(04,"\n")
				+WL(03,"\n By default these steps are performed on each folder supplied:")
				+WL(03,"\n 1 - if no .par2 folder exists, create folder and generate recovery info for"
					+" all files. add -r option to create .par2 folders and files recursively. use -n to skip this step")
				+WL(03,"\n 2 - if .par2 folder is found, create recovery info for any files that are missing that data")
				+WL(03,"\n 3 - check recovery info against existing files and output warnings for any corruption")
				+WL(03,"\n 4 - if -a option is supplied, attempt to recover any corrupt files")
				+WL(03,"\n 5 - remove recovery info for any files that no longer exists")
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
				else if (curr == "-n") {
					AutoCreate = false;
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

		static string WL(int indent, string s) {
			int w = Console.WindowWidth;
			int l = 0, c = 0;
			var sb = new  StringBuilder();
			while(l < s.Length) {
				if (c < w) {
					sb.Append(s[l]);
					l++; c++;
				}
				else {
					sb.AppendLine();
					sb.Append(new string(' ',indent));
					c = indent;
				}
			}
			return sb.ToString();
		}

		public static string Par2ExePath = "par2";
		public static StreamWriter Par2LogFile = null;
		public static List<string> RootFolders = new List<string>();
		public static bool Verbose = false;
		public static bool DryRun = false;
		public static bool AutoRecover = false;
		public static bool AutoCreate = true;
		public static bool Recurse = false;
		public static bool IncludeHiddenFiles = false;
		public static bool IncludeHiddenFolders = false;
		public static string DataFolder = ".par2";
		public static int Tolerance = 5;
	}
}