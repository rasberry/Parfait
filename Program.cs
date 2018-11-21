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

			//if we're missing the par2 file create it
			if (!File.Exists(par2DataFile)) {
				Log.Info("Create\t"+par2DataFile);
				ParHelpers.CreatePar(file, par2DataFile);
			}
			else {
				var par2Info = new FileInfo(par2DataFile);
				var fileInfo = new FileInfo(file);
				//if par2 is newer than file - verify
				if (par2Info.LastWriteTimeUtc >= fileInfo.LastWriteTime) {
					Log.Info("Verify\t"+par2DataFile);
					ParHelpers.VerifyFile(file, par2DataFile);
					//TODO do something with non success return
				}
				//assume file was modified by a human and we need to re-create the par2
				else {
					Log.Info("ReCreate\t"+par2DataFile);
					ParHelpers.RemoveParSet(par2DataFile);
					ParHelpers.CreatePar(file, par2DataFile);
				}
			}
		}
	}
}
