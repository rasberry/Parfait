using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests
{
	[TestClass]
	public class FileToParMapTests
	{
		public FileToParMapTests()
		{
			//var sw = new StreamWriter(File.OpenWrite("t.txt"));
			// Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
		}

		static bool IsWindows() {
			return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		}
		static bool IsLinux() {
			return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		}
		static bool IsMac() {
			return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		}

		[TestMethod]
		public void MapFileToPar2File_WindowsTest1()
		{
			if (!IsWindows()) { Assert.Inconclusive(); return; }

			string dataFolder = "d:\\temp\\par2";
			string file = "d:\\Projects\\test.txt";

			string par2File = MapFileToPar2File(file,dataFolder);
			Trace.WriteLine("par2File = "+par2File);
			Assert.IsTrue("d:\\temp\\par2\\d$\\Projects\\test.txt.par2" == par2File);
		}

		[TestMethod]
		public void MapFileToPar2File_WindowsTest2()
		{
			if (!IsWindows()) { Assert.Inconclusive(); return; }

			string dataFolder = "d:\\temp\\par2";
			string file = "c:\\Projects\\test.txt";

			string par2File = MapFileToPar2File(file,dataFolder);
			Trace.WriteLine("par2File = "+par2File);
			Assert.IsTrue("d:\\temp\\par2\\c$\\Projects\\test.txt.par2" == par2File);
		}

		[TestMethod]
		public void MapFileToPar2File_LinuxTest1()
		{
			if (!IsLinux()) { Assert.Inconclusive(); return; }

			string dataFolder = "/tmp/par2";
			string file = "/Projects/test.txt";

			string par2File = MapFileToPar2File(file,dataFolder);
			Trace.WriteLine("par2File = "+par2File);
			Assert.IsTrue("/tmp/par2/Projects/test.txt.par2" == par2File);
		}

		[TestMethod]
		public void MarPar2ToOrigFile_WindowsTest1()
		{
			if (!IsWindows()) { Assert.Inconclusive(); return; }

			string dataFolder = "d:\\temp\\par2";
			string par2File = "d:\\temp\\par2\\c$\\Projects\\test.txt.par2";

			string origFile = MarPar2ToOrigFile(par2File,dataFolder);
			Trace.WriteLine("origFile = "+origFile);
			Assert.IsTrue("c:\\Projects\\test.txt" == origFile);
		}

		[TestMethod]
		public void MarPar2ToOrigFile_LinuxTest1()
		{
			if (!IsLinux()) { Assert.Inconclusive(); return; }

			string dataFolder = "/tmp/par2";
			string par2File = "/tmp/par2/Projects/test.txt.par2";

			string origFile = MarPar2ToOrigFile(par2File,dataFolder);
			Trace.WriteLine("origFile = "+origFile);
			Assert.IsTrue("/Projects/test.txt" == origFile);
		}

		static string MapFileToPar2File(string file, string dataFolder)
		{
			if (!Path.IsPathRooted(file)) {
				throw new Exception("file path must be absolute");
			}
			if (!Path.IsPathRooted(dataFolder)) {
				throw new Exception("data folder path must be absolute");
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

		static string MarPar2ToOrigFile(string par2File, string dataFolder)
		{
			if (!Path.IsPathRooted(par2File)) {
				throw new Exception("par2 path must be absolute");
			}
			if (!Path.IsPathRooted(dataFolder)) {
				throw new Exception("data folder path must be absolute");
			}
			string origFile = Path.GetRelativePath(dataFolder,par2File);
			if (1 == origFile.IndexOf('$')) {
				char[] temp = origFile.ToCharArray();
				temp[1] = ':';
				origFile = new String(temp);
			}
			else if (0 != origFile.IndexOf(Path.DirectorySeparatorChar)) {
				origFile = Path.DirectorySeparatorChar + origFile;
			}
			if (!origFile.EndsWith(".par2")) {
				throw new Exception("par2 path must end in '.par2'");
			}
			return origFile.Substring(0,origFile.Length - 5);
		}
	}
}
