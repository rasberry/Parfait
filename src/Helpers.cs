using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Parfait
{
	public static class Helpers
	{
		public static string MapFileToPar2File(string file)
		{
			string dir = Path.GetDirectoryName(file);
			string name = Path.GetFileName(file);

			string par2File = Path.Combine(dir,Options.DataFolder,name + ".par2");
			return par2File;
		}

		public static string MarPar2ToOrigFile(string par2File)
		{
			if (!Path.IsPathRooted(par2File)) {
				throw new BadPathException("path must be rooted");
			}

			string origFile = par2File;

			if (!origFile.EndsWith(".par2")) {
				throw new BadPathException("par2 path must end in '.par2'");
			}
			origFile = origFile.Substring(0,origFile.Length - 5);

			// take care of the .vol* files - something.txt.vol00+20.par2
			string volExt = Path.GetExtension(origFile);
			if (volExt.StartsWith(".vol")) {
				origFile = origFile.Substring(0,origFile.Length - volExt.Length);
			}

			return MoveUpOneFolder(origFile);
		}

		static string MoveUpOneFolder(string pathWithFile)
		{
			string folder = Path.GetDirectoryName(pathWithFile);
			string file = Path.GetFileName(pathWithFile);
			string parentDir = Directory.GetParent(folder).FullName;
			return Path.Combine(parentDir,file);
		}

		public static IEnumerable<string> EnumerateFiles(string root, bool recurse = false, bool allowHidden = false)
		{
			if (!Directory.Exists(root)) {
				yield break;
			}

			//had to implement a manual loop because the regular .net one doesn't skip hidden files/folders
			var files = Directory.EnumerateFiles(root,"*",SearchOption.TopDirectoryOnly);
			foreach(string f in files) {
				bool show = allowHidden || (!Options.IncludeHiddenFiles && !IsHidden(f));
				if (show) {
					//Log.Debug("yield file "+f);
					yield return f;
				}
			}

			if (recurse) {
				var folders = EnumerateFolders(root, true, allowHidden);
				foreach(string d in folders) {
					var deepFiles = EnumerateFiles(d, false, allowHidden);
					foreach(string deep in deepFiles) {
						//Log.Debug("yield file "+d);
						yield return deep;
					}
				}
			}
		}

		public static IEnumerable<string> EnumerateFolders(string folder, bool recurse = false, bool allowHidden = false)
		{
			var folders = Directory.EnumerateDirectories(folder,"*",SearchOption.TopDirectoryOnly);
			foreach(string d in folders) {
				//Log.Debug("enum folder "+d);
				bool show = allowHidden || (!Options.IncludeHiddenFolders && !IsHidden(d));
				if (show) {
					//Log.Debug("yield folder "+d);
					yield return d;

					if (recurse) {
						var deepFolders = EnumerateFolders(d,true,allowHidden);
						foreach(string deep in deepFolders) {
							//Log.Debug("yield folder "+d);
							yield return deep;
						}
					}
				}
			}
		}

		static string[] _hideChecks = new string[] {
			Path.DirectorySeparatorChar+".",
			Path.AltDirectorySeparatorChar+"."
		};
		public static bool IsHidden(string item)
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

		public static void DeleteFile(string path)
		{
			Log.File("I: Delete\t"+path);
			if (!Options.DryRun) {
				File.Delete(path);
			}
		}
	}
}