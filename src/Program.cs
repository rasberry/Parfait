using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Parfait
{
	class Program
	{
		static int Main(string[] args)
		{
			if (!Options.ParseArgs(args)) {
				return 1; //arguments were incorrect
			}
			try {
				MainMain(args);
			} catch(Exception e) {
				#if DEBUG
				Log.Error(e.ToString());
				#else
				Log.Error(e.Message);
				#endif
				return 2; //an exception was thrown
			} finally {
				if (Options.Par2LogFile != null) {
					Options.Par2LogFile.Dispose();
				}
			}
			return 0; //success
		}

		static void MainMain(string[] args)
		{
			if (Options.RepairFiles.Count > 0) {
				// manual repair mode
				DoRepairs();
			}
			else {
				//regular create / update mode
				UpdateArchive();
				PruneArchive();
			}
		}

		static void UpdateArchive()
		{
			// make sure data folder exists
			if (!Directory.Exists(Options.DataFolder)) {
				Directory.CreateDirectory(Options.DataFolder);
			}

			foreach(string root in Options.RootFolders) {
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

			string par2DataFile = Helpers.MapFileToPar2File(file,Options.DataFolder);

			bool doCreate = false;
			bool doVerify = false;
			bool doRemove = false;
			//if we're missing the par2 file create it
			if (!File.Exists(par2DataFile)) {
				Log.Info("Create\t"+par2DataFile);
				doCreate = true;
			}
			else {
				var par2Info = new FileInfo(par2DataFile);
				var fileInfo = new FileInfo(file);
				//if par2 is newer than file - verify
				if (par2Info.LastWriteTimeUtc >= fileInfo.LastWriteTimeUtc) {
					Log.Info("Verify\t"+par2DataFile);
					doVerify = true;
				}
				//repair instead of re-create (!)
				else if (Options.RevertRepair) {
					ParHelpers.RepairFile(file,Options.DataFolder);
				}
				//assume file was modified by a human and we need to re-create the par2
				else {
					Log.Info("ReCreate\t"+par2DataFile);
					doRemove = true;
					doCreate = true;
				}
			}

			if (Options.DryRun) { return; }
			if (doRemove) {
				ParHelpers.RemoveParSet(par2DataFile);
			}
			if (doCreate) {
				var result = ParHelpers.CreatePar(file, par2DataFile);
				HandleParResult(result,file);
			}
			else if (doVerify) {
				var result = ParHelpers.VerifyFile(file, par2DataFile);
				HandleParResult(result,file);
			}
		}

		static void HandleParResult(ParHelpers.ParResult result, string file)
		{
			switch(result)
			{
			case ParHelpers.ParResult.Success: return;
			case ParHelpers.ParResult.CanRepair:
				Log.Message("File can be repaired\t"+file); return;
			case ParHelpers.ParResult.CannotRepair:
				Log.Warning("File CANNOT be repaired\t"+file); return;
			case ParHelpers.ParResult.BadArguments:
				Log.Error("Bad arguments passed to Par2"); return;
			case ParHelpers.ParResult.NoEnoughData:
				Log.Warning("Not enough data to verify\t"+file); return;
			case ParHelpers.ParResult.RepairFailed:
				Log.Warning("Repair FAILED\t"+file); return;
			case ParHelpers.ParResult.FileMissing:
				Log.Error("Cannot find file\t"+file); return;
			case ParHelpers.ParResult.Error:
				Log.Warning("Par2 reported a generic error"); return;
			case ParHelpers.ParResult.OutOfMemory:
				Log.Warning("Par2 reported out of memory error"); return;
			}
		}

		static void PruneArchive()
		{
			var parFiles = EnumerateFiles(Options.DataFolder);
			foreach(string f in parFiles) {
				string orig = Helpers.MapPar2ToOrigFile(f,Options.DataFolder);
				if (!File.Exists(orig)) {
					Log.Info("Remove\t"+f);
					if (!Options.DryRun) {
						File.Delete(f);
					}
				}
			}
		}

		static void DoRepairs()
		{
			foreach(string f in Options.RepairFiles)
			{
				string parFile = Helpers.MapFileToPar2File(f,Options.DataFolder);
				Log.Info("Repair\t"+f);
				if (!Options.DryRun) {
					var result = ParHelpers.RepairFile(f,parFile);
					HandleParResult(result,f);
				}
			}
		}
	}
}
