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
			if (!Options.ParseArgs(args)) { return; }
			try {
				MainMain(args);
			} catch(Exception e) {
				#if DEBUG
				Log.Error(e.ToString());
				#else
				Log.Error(e.Message);
				#endif
			} finally {
				if (Options.Par2LogFile != null) {
					Options.Par2LogFile.Dispose();
				}
			}
		}

		static void MainMain(string[] args)
		{
			UpdateArchive();
			// PruneArchive();
		}

		//static DateTimeOffset GetLastRunDate()
		//{
		//	string lr = Path.Combine(DataFolder,"last-run");
		//	if (!File.Exists(lr)) {
		//		Log.Warning("Cannot find last-run. All files will be processed");
		//		return DateTimeOffset.MinValue;
		//	}
		//	string lrText = (File.ReadAllText(lr) ?? "").Trim();
		//	if (!long.TryParse(lrText, out long unixTime)) {
		//		Log.Warning("Could not parse last-run. All files will be processed");
		//		return DateTimeOffset.MinValue;
		//	}
		//	return DateTimeOffset.FromUnixTimeSeconds(unixTime);
		//}

		static void UpdateArchive()
		{
			// make sure data folder exists
			if (!Directory.Exists(Options.DataFolder)) {
				Directory.CreateDirectory(Options.DataFolder);
			}

			foreach(string root in Options.RootFolders) {
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
			string dataFileRoot = Path.GetFullPath(
				Path.Combine(Options.DataFolder,noRoot)
			);
			string par2DataFile = dataFileRoot + ".par2";
			string infoDataFile = dataFileRoot + ".json";



			//TODO check to see if src file has changed
			if (!File.Exists(par2DataFile)) {
				CreatePar(file, par2DataFile);
			}
			else {
				VerifyFile(file, par2DataFile);
			}
		}

		static void CreatePar(string file, string par2DataFile)
		{
			//0 = success
			//1 = ?

			string parDir = Path.GetDirectoryName(par2DataFile);
			Directory.CreateDirectory(parDir);
			string fileDir = Path.GetDirectoryName(file);
			string qq = Options.Par2LogFile != null ? "-q " : "-q -q ";

			//apparently -B has to go before -a or it doesn't work
			string args = "c "+qq+"-r1 -n1 -B \"" + fileDir + "\" -a \"" + par2DataFile + "\" \"" + file + "\"";
			RunPar(args);
		}

		static void VerifyFile(string file, string par2DataFile)
		{
			//0 = success
			//1 = damaged but can repair
			//2 = damaged and cannot repair

			string fileDir = Path.GetDirectoryName(file);
			string qq = Options.Par2LogFile != null ? "-q " : "-q -q ";
			string args = "v "+qq+"-B \""+fileDir+"\" -a \""+par2DataFile+"\"";
			RunPar(args);
		}

		static void RunPar(string args)
		{
			int exit = Exec(Options.Par2Path, args, out string stdout, out string stderr);

			if (Options.Par2LogFile != null)
			{
				Options.Par2LogFile.WriteLine(exit + ": " + Options.Par2Path + " " + args);
				if (!String.IsNullOrWhiteSpace(stdout)) {
					Options.Par2LogFile.WriteLine("SO: " + stdout);
				}
				if (!String.IsNullOrWhiteSpace(stderr)) {
					Options.Par2LogFile.WriteLine("SE: " + stderr);
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
