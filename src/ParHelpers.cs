using System;
using System.Diagnostics;
using System.IO;

namespace Parfait
{
	public static class ParHelpers
	{
		// https://github.com/Parchive/par2cmdline/blob/master/src/par2cmdline.h#L233
		public enum ParResult {
			Success = 0,
			CanRepair = 1,
			CannotRepair = 2,
			BadArguments = 3,
			NoEnoughData = 4,
			RepairFailed = 5,
			FileMissing = 6,
			Error = 7,
			OutOfMemory = 8
		}

		const int BlockSize = 4096;

		public static ParResult CreateFolderPar(string folder, int tolerance)
		{
			string par2File = Helpers.GetPar2ArchiveName(folder);
			string par2Folder = Path.GetDirectoryName(par2File);

			if (!Directory.Exists(par2Folder)) {
				var dinfo = Directory.CreateDirectory(par2Folder);
				if (!dinfo.Exists) {
					throw new DirectoryNotFoundException("Could not create "+par2Folder);
				}
			}

			string qq = Options.Par2LogFile != null ? " -q" : " -q -q";
			string args = string.Format("c{0} -r{1} -n1 \"{2}\" \"{3}\"",qq, tolerance, par2File, folder);
			int ret = RunPar(args);
			return (ParResult)ret;
		}

		public static ParResult VerifyFolder(string folder)
		{
			string par2File = Helpers.GetPar2ArchiveName(folder);
			string par2Folder = Path.GetDirectoryName(par2File);
			
			if (!Directory.Exists(par2Folder)) {
				throw new DirectoryNotFoundException("Could not find "+par2Folder);
			}
			if (!File.Exists(par2File)) {
				throw new FileNotFoundException("Could not find "+par2File);
			}

			string qq = Options.Par2LogFile != null ? " -q" : " -q -q";
			string args = string.Format("v{0} \"{1}\" \"{2}\"",qq, par2File, folder);
			int ret = RunPar(args);
			return (ParResult)ret;
		}

		public static ParResult RepairFolder(string folder)
		{
			string par2File = Helpers.GetPar2ArchiveName(folder);
			string par2Folder = Path.GetDirectoryName(par2File);
			
			if (!Directory.Exists(par2Folder)) {
				throw new DirectoryNotFoundException("Could not find "+par2Folder);
			}
			if (!File.Exists(par2File)) {
				throw new FileNotFoundException("Could not find "+par2File);
			}

			string qq = Options.Par2LogFile != null ? " -q" : " -q -q";
			string args = string.Format("r -p{0} \"{1}\" \"{2}\"",qq, par2File, folder);
			int ret = RunPar(args);
			return (ParResult)ret;
		}

		public static ParResult CreateFilePar(string file, string par2DataFile, int tolerance)
		{
			string parDir = Path.GetDirectoryName(par2DataFile);
			Directory.CreateDirectory(parDir);
			string fileDir = Path.GetDirectoryName(file);
			string qq = Options.Par2LogFile != null ? "-q " : "-q -q ";
			
			string qs = "";
			var fi = new FileInfo(file);
			if (fi.Length > 200/tolerance*BlockSize) {
				qs = "-s"+BlockSize+" ";
			}

			//apparently -B has to go before -a or it doesn't work
			string args = "c " + qq + qs + "-r"+tolerance+" -n1 -B"
				+" \"" + fileDir + "\" -a \"" + par2DataFile + "\" \"" + file + "\"";
			int ret = RunPar(args);
			return (ParResult)ret;
		}

		public static ParResult VerifyFile(string file, string par2DataFile)
		{
			string fileDir = Path.GetDirectoryName(file);
			string qq = Options.Par2LogFile != null ? "-q " : "-q -q ";
			string args = "v "+qq+"-B \""+fileDir+"\" -a \""+par2DataFile+"\"";
			int ret = RunPar(args);
			return (ParResult)ret;
		}

		public static ParResult RepairFile(string file, string par2DataFile)
		{
			string fileDir = Path.GetDirectoryName(file);
			string qq = Options.Par2LogFile != null ? "-q " : "-q -q ";
			string args = "r "+qq+"-p -B \""+fileDir+"\" -a \""+par2DataFile+"\"";
			int ret = RunPar(args);
			return (ParResult)ret;
		}

		public static void RemoveParSet(string parFile)
		{
			string parDir = Path.GetDirectoryName(parFile);
			string root = Path.GetFileNameWithoutExtension(parFile);
			var fileList = Directory.EnumerateFiles(parDir);
			foreach(string f in fileList) {
				string name = Path.GetFileName(f);
				if (name.StartsWith(root)) {
					Helpers.DeleteFile(f);
				}
			}
		}

		static int RunPar(string args)
		{
			int exit = Exec(Options.Par2ExePath, args, out string stdout, out string stderr);

			Log.File(exit + ": " + Options.Par2ExePath + " " + args);
			if (!String.IsNullOrWhiteSpace(stdout)) {
				Log.File("SO: " + stdout);
			}
			if (!String.IsNullOrWhiteSpace(stderr)) {
				Log.File("SE: " + stderr);
			}
			return exit;
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
				proc.PriorityClass = ProcessPriorityClass.Idle;
				proc.Start();
				stdout = proc.StandardOutput.ReadToEnd(); //The output result
				stderr = proc.StandardError.ReadToEnd();
				proc.WaitForExit();
				return proc.ExitCode;
			}
		}
	}
}
