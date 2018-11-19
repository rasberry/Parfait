using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using System.Linq;

namespace Parfait
{
	class Program
	{
		static void Main(string[] args)
		{
			if (!ParseArgs(args)) { return; }
			try {
				MainMain(args);
			} catch(Exception e) {
				#if DEBUG
				Log.Error(e.ToString());
				#else
				Log.Error(e.Message);
				#endif
			} finally {
				if (Par2LogFile != null) {
					Par2LogFile.Dispose();
				}
			}
		}

		static void MainMain(string[] args)
		{
			LastRunDate = GetLastRunDate();
			UpdateArchive();
		}

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

		static bool ParseArgs(string[] args)
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

		static string DataFolder = null;
		static string Par2Path = "par2";
		static StreamWriter Par2LogFile = null;
		static List<string> RootFolders = new List<string>();
		static DateTimeOffset LastRunDate = DateTimeOffset.MinValue;

		static DateTimeOffset GetLastRunDate()
		{
			string lr = Path.Combine(DataFolder,"last-run");
			if (!File.Exists(lr)) {
				Log.Warning("Cannot find last-run. All files will be processed");
				return DateTimeOffset.MinValue;
			}
			string lrText = (File.ReadAllText(lr) ?? "").Trim();
			if (!long.TryParse(lrText, out long unixTime)) {
				Log.Warning("Could not parse last-run. All files will be processed");
				return DateTimeOffset.MinValue;
			}
			return DateTimeOffset.FromUnixTimeSeconds(unixTime);
		}

		static void UpdateArchive()
		{
			// make sure data folder exists
			if (!Directory.Exists(DataFolder)) {
				Directory.CreateDirectory(DataFolder);
			}

			foreach(string root in RootFolders) {
				Log.Debug(root);
				var allFiles = EnumerateFiles(root);

				foreach(string file in allFiles) {
					UpdateFile(file);
				}
			}
		}

		static IEnumerable<string> EnumerateFiles(string root)
		{
			var files = Directory.EnumerateFiles(root,"*",SearchOption.TopDirectoryOnly);
			foreach(string f in files) {
				if (!IsHidden(f)) { yield return f; }
			}
			var folders = Directory.EnumerateDirectories(root,"*",SearchOption.TopDirectoryOnly);
			foreach(string d in folders) {
				if (!IsHidden(d)) { 
					var deepFiles = EnumerateFiles(d);
					foreach(string df in deepFiles) { yield return df; }
				}
			}
		}

		//static Stack<string> _folderStack = new Stack<string>();
		//static IEnumerable<string> EnumFiles(string root)
		//{
		//	_folderStack.Push(root);
		//	while(_folderStack.Count > 0)
		//	{
		//		string current = _folderStack.Pop();
		//		var files = Directory.EnumerateFiles(current,"*",SearchOption.TopDirectoryOnly);
		//		foreach(string f in files) {
		//			if (!IsHidden(f)) { yield return f; }
		//		}
		//		var folders = Directory.EnumerateDirectories(current,"*",SearchOption.TopDirectoryOnly);
		//		foreach(string f in folders) {
		//			if (!IsHidden(f)) { 
		//				_folderStack.Push(f);
		//			}
		//		}
		//	}
		//}

		static string[] _hideChecks = new string[] {
			Path.DirectorySeparatorChar+".",
			Path.AltDirectorySeparatorChar+"."
		};
		static bool IsHidden(string item)
		{
			var att = File.GetAttributes(item);
			if (att.HasFlag(FileAttributes.Hidden)) {
				return true;
			}

			foreach(string check in _hideChecks) {
				int ix = item.IndexOf(check);
				if (ix != -1) { return true; }
			}
			return false;
		}

		static void UpdateFile(string file)
		{
			// skip empty file names
			if (String.IsNullOrWhiteSpace(file)) { return; }
			file = Path.GetFullPath(file);

			// skip 0 byte files
			var info = new FileInfo(file);
			if (info.Length < 1) { return; }
			
			string root = Path.GetPathRoot(file);
			string noRoot = String.IsNullOrWhiteSpace(root)
				? file
				: Path.GetRelativePath(root,file)
			;
			string par2DataFile = Path.GetFullPath(
				Path.Combine(DataFolder,noRoot)
			);

			//TODO check to see if src file has changed
			if (!File.Exists(par2DataFile))
			{
				string parDir = Path.GetDirectoryName(par2DataFile);
				Directory.CreateDirectory(parDir);
				string fileDir = Path.GetDirectoryName(file);

				string args = "c -q -q -r1 -n1 -B \""+fileDir+"\" -a \""+par2DataFile+"\" \""+file+"\"";
				int exit = Exec(Par2Path,args,out string stdout, out string stderr);

				if (Par2LogFile != null) {
					Par2LogFile.WriteLine(exit+": "+Par2Path+" "+args);
					if (!String.IsNullOrWhiteSpace(stdout)) {
						Par2LogFile.WriteLine("SO: "+stdout);
					}
					if (!String.IsNullOrWhiteSpace(stderr)) {
						Par2LogFile.WriteLine("SE: "+stderr);
					}
				}
			}
		}

		static int Exec(string program, string args, out string stdout,out string stderr)
		{
			stdout = stderr = null;
			using (Process proc = new Process())
			{
				var si = new ProcessStartInfo();
				si.FileName = program;
				si.Arguments = args;
				si.UseShellExecute = false;
				si.RedirectStandardOutput = true;
				si.RedirectStandardError = true;
				si.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				si.CreateNoWindow = true; //don't diplay a window

				proc.StartInfo = si;
				proc.Start();
				proc.PriorityClass = ProcessPriorityClass.Idle;
				stdout = proc.StandardOutput.ReadToEnd(); //The output result
				stderr = proc.StandardError.ReadToEnd();
				proc.WaitForExit();
				return proc.ExitCode;
			}
		}
	}
}
