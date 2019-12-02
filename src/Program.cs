using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;

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

			//get list of all folders
			var allFolders = new HashSet<string>();
			allFolders.UnionWith(Options.RootFolders);

			if(Options.Recurse) {
				foreach(string root in Options.RootFolders) {
					var folders = Helpers.EnumerateFolders(root,true,Options.IncludeHiddenFolders);
					allFolders.UnionWith(folders);
				}
			}

			foreach(var folder in allFolders) {
				UpdateFolder(folder);
			}

			return 0;
		}

		static void UpdateFolder(string folder)
		{
			//if .par2 folder doesn't exist - create it
			//verify files
			//if repairs are needed
			//	if autorepair is on - attept repairs
			//		if reparis fail stop
			//	else stop
			//if any file is modified afer the par2 file
			//	re-create par2 file

			// skip empty folder names
			if (String.IsNullOrWhiteSpace(folder)) { return; }

			string par2File = Helpers.GetPar2ArchiveName(folder);
			string par2Folder = Path.GetDirectoryName(par2File);
			
			//skip empty folders
			if (Helpers.IsFolderEmpty(folder,Options.IncludeHiddenFiles)) { return; }

			//split planning and execution so that we can handle dry run
			bool doCreate = false;
			bool doVerify = false;
			bool doRepair = false;

			// create if data folder if something is missing
			if (!Directory.Exists(par2Folder) || !File.Exists(par2File)) {
				if (Options.AutoCreate) {
					Log.Info("Create\t"+folder);
					doCreate = true;
				}
			}
			else {
				Log.Info("Verify\t"+folder);
				doVerify = true;
				bool needsUpdate = Helpers.DoesFolderNeedUpdate(folder,Options.IncludeHiddenFiles);
				if (needsUpdate && Options.AutoCreate) {
					Log.Info("Update\t"+folder);
					doCreate = true;
				}
			}

			//if it's a dry run skip actually executing the steps
			if (Options.DryRun) {
				return;
			}

			if (doVerify) {
				var result = ParHelpers.VerifyFolder(folder);
				HandleParResult(result,folder);
				if (result == ParHelpers.ParResult.CanRepair) {
					doRepair = Options.AutoRecover;
				}
			}
			if (doRepair) {
				var result = ParHelpers.RepairFolder(folder);
				if (result == ParHelpers.ParResult.Success) {
					Log.Message("Reparied\t"+folder);
					//the par2 file gets deleted if the repair was successfull
					doCreate = true;
				}
			}
			if (doCreate) {
				var result = ParHelpers.CreateFolderPar(folder,Options.Tolerance);
				HandleParResult(result,folder);
			}
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
			if (Options.AutoCreate && !Directory.Exists(dataFolder)) {
				if (!Helpers.CreateFolder(dataFolder)) {
					return; //skip this folder since we could not create it
				}
			}
			//if we're missing the par2 file create it
			if (!File.Exists(par2DataFile)) {
				if (Options.AutoCreate) {
					Log.Info("Create\t"+file);
					doCreate = true;
				}
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
				var result = ParHelpers.CreateFilePar(file, par2DataFile,Options.Tolerance);
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
					var result2 = ParHelpers.CreateFilePar(file, par2DataFile, Options.Tolerance);
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
