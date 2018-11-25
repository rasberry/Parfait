using System;
using System.Diagnostics;
using System.IO;

namespace Parfait
{
	public static class Helpers
	{
		public static string MapFileToPar2File(string file, string dataFolder)
		{
			if (!Path.IsPathRooted(file)) {
				throw new BadPathException("file path must be absolute");
			}
			if (!Path.IsPathRooted(dataFolder)) {
				throw new BadPathException("data folder path must be absolute");
			}
			//for windows change the drive ':' to '$'
			if (1 == file.IndexOf(':')) {
				char[] temp = file.ToCharArray();
				temp[1] = '$';
				file = new String(temp);
			}
			//for linux remove the starting slash
			if (0 == file.IndexOf(Path.DirectorySeparatorChar)) {
				file = file.Substring(1);
			}
			//file must be relative for Path.Combine to work
			string comb = Path.Combine(dataFolder,file) + ".par2";
			return Path.GetFullPath(comb);
		}

		public static string MapPar2ToOrigFile(string par2File, string dataFolder)
		{
			if (!Path.IsPathRooted(par2File)) {
				throw new BadPathException("par2 path must be absolute");
			}
			if (!Path.IsPathRooted(dataFolder)) {
				throw new BadPathException("data folder path must be absolute");
			}
			string origFile = Path.GetRelativePath(dataFolder,par2File);
			//for windows put back the drive ':'
			if (1 == origFile.IndexOf('$')) {
				char[] temp = origFile.ToCharArray();
				temp[1] = ':';
				origFile = new String(temp);
			}
			//for linux make sure the path starts with a slash
			else if (0 != origFile.IndexOf(Path.DirectorySeparatorChar)) {
				origFile = Path.DirectorySeparatorChar + origFile;
			}
			if (!origFile.EndsWith(".par2")) {
				throw new BadPathException("par2 path must end in '.par2'");
			}
			origFile = origFile.Substring(0,origFile.Length - 5);
			// take care of the .vol* files - something.txt.vol00+20.par2
			string volExt = Path.GetExtension(origFile);
			if (volExt.StartsWith(".vol")) {
				origFile = origFile.Substring(0,origFile.Length - volExt.Length);
			}
			return origFile;

		}
	}
}