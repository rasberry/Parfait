using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

//let the test project access internal entities
[assembly:InternalsVisibleTo("Parfait.Test")]

namespace Parfait
{
	internal class Program
	{
		static int Main(string[] args)
		{
			try {
				return MainMain(args);
			} catch(Exception e) {
				#if DEBUG
				Log.Error("Fatal; "+e.ToString());
				#else
				Log.Error("Fatal; "+e.Message);
				#endif
				return 2; //an exception was thrown
			} finally {
				if (Options.Par2LogFile != null) {
					Options.Par2LogFile.Dispose();
				}
			}
		}

		internal static int MainMain(string[] args)
		{
			if (!Options.ParseArgs(args)) {
				return 1; //arguments were incorrect
			}

			foreach(string root in Options.RootFolders) {
				//main processing
				var allFiles = Helpers.EnumerateFiles(root, Options.Recurse);
				foreach(string file in allFiles) {
					UpdateDataFile(file);
				}

				//prune par2 files if original is missing
				PruneArchive(root);
				if (Options.Recurse) {
					var allFolders = Helpers.EnumerateFolders(root, true);
					foreach(string folder in allFolders) {
						PruneArchive(folder);
					}
				}
			}
			return 0; //success
		}

		static void UpdateDataFile(string file)
		{
			// skip empty file names
			if (String.IsNullOrWhiteSpace(file)) { return; }

			// skip 0 byte files
			var info = new FileInfo(file);
			if (info.Length < 1) { return; }

			string par2DataFile = Helpers.MapFileToPar2File(file);

			//split planning and execution so that we can handle dry run
			bool doCreate = false;
			bool doVerify = false;
			bool doRemove = false;
			bool doRepair = false;

			// make sure data folder exists
			string dataFolder = Path.GetDirectoryName(par2DataFile);
			if (!Directory.Exists(dataFolder)) {
				if (!Helpers.CreateFolder(dataFolder)) {
					return; //skip this folder since we could not create it
				}
			}
			//if we're missing the par2 file create it
			if (!File.Exists(par2DataFile)) {
				Log.Info("Create\t"+file);
				doCreate = true;
			}
			else {
				var par2Info = new FileInfo(par2DataFile);
				var fileInfo = new FileInfo(file);
				//if par2 is newer than file - verify
				if (par2Info.LastWriteTimeUtc >= fileInfo.LastWriteTimeUtc) {
					Log.Info("Verify\t"+file);
					doVerify = true;
				}
				//assume file was modified by a human and we need to re-create the par2
				else {
					Log.Info("ReCreate\t"+file);
					doRemove = true;
					doCreate = true;
				}
			}

			//if it's a dry run skip actually executing the steps
			if (Options.DryRun) {
				return;
			}

			//perform the actions
			if (doRemove) {
				ParHelpers.RemoveParSet(par2DataFile);
			}
			if (doCreate) {
				var result = ParHelpers.CreatePar(file, par2DataFile,Options.Tolerance);
				HandleParResult(result,file);
			}
			if (!doCreate && doVerify) {
				var result = ParHelpers.VerifyFile(file, par2DataFile);
				HandleParResult(result,file);

				if (result == ParHelpers.ParResult.CanRepair) {
					doRepair = Options.AutoRecover;
				}
			}
			if (doRepair) {
				var result = ParHelpers.RepairFile(file, par2DataFile);
				HandleParResult(result,file);
				if (result == ParHelpers.ParResult.Success) {
					Log.Message("Reparied\t"+file);
					//the par2 file gets deleted if the repair was successfull
					var result2 = ParHelpers.CreatePar(file, par2DataFile, Options.Tolerance);
					HandleParResult(result2,file);
				}
			}
		}

		static void HandleParResult(ParHelpers.ParResult result, string file)
		{
			switch(result)
			{
			case ParHelpers.ParResult.Success: return;
			case ParHelpers.ParResult.CanRepair:
				Log.Message("File is damaged and can be repaired\t"+file); return;
			case ParHelpers.ParResult.CannotRepair:
				Log.Warning("File is damaged and CANNOT be repaired\t"+file); return;
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

		static void PruneArchive(string root)
		{
			// Log.Debug("PruneArchive '"+root+"'");
			string par2Folder = Path.Combine(root,Options.DataFolder);
			var parFiles = Helpers.EnumerateFiles(par2Folder, allowHidden:true);
			var noDups = new HashSet<string>();
			foreach(string p in parFiles) {
				if (!p.EndsWith(".par2")) { continue; }
				string orig = Helpers.MapPar2ToOrigFile(p);
				if (!File.Exists(orig)) {
					if (!noDups.Contains(orig)) {
						Log.Info("Pruned\t"+orig);
						noDups.Add(orig);
					}
					Helpers.DeleteFile(p);
				}
			}
		}
	}
}
