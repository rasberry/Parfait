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

		public static ParResult CreatePar(string file, string par2DataFile, int tolerance)
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
			Log.Debug(exit + ": " + Options.Par2ExePath + " " + args);
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
				// proc.PriorityClass = ProcessPriorityClass.Idle;
				proc.Start();
				stdout = proc.StandardOutput.ReadToEnd(); //The output result
				stderr = proc.StandardError.ReadToEnd();
				proc.WaitForExit();
				return proc.ExitCode;
			}
		}
	}
}
