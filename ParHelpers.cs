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

		public static ParResult CreatePar(string file, string par2DataFile)
		{
			string parDir = Path.GetDirectoryName(par2DataFile);
			Directory.CreateDirectory(parDir);
			string fileDir = Path.GetDirectoryName(file);
			string qq = Options.Par2LogFile != null ? "-q " : "-q -q ";

			//apparently -B has to go before -a or it doesn't work
			string args = "c "+qq+"-r1 -n1 -B \"" + fileDir + "\" -a \"" + par2DataFile + "\" \"" + file + "\"";
			int ret = RunPar(args);
			//TODO do something with non success returns
			return (ParResult)ret;
		}

		public static ParResult VerifyFile(string file, string par2DataFile)
		{
			string fileDir = Path.GetDirectoryName(file);
			string qq = Options.Par2LogFile != null ? "-q " : "-q -q ";
			string args = "v "+qq+"-B \""+fileDir+"\" -a \""+par2DataFile+"\"";
			int ret = RunPar(args);
			//TODO do something with non success returns
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
					File.Delete(f);
				}
			}
		}

		static int RunPar(string args)
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
