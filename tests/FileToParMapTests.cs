using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parfait;

namespace Parfait.Tests
{
	[TestClass]
	public class FileToParMapTests
	{
		[TestMethod]
		public void MapFileToPar2File_WindowsTest1()
		{
			if (!TestHelpers.IsWindows()) { Assert.Inconclusive(); return; }

			string dataFolder = "d:\\temp\\par2";
			string file = "d:\\Projects\\test.txt";

			string par2File = Helpers.MapFileToPar2File(file,dataFolder);
			Trace.WriteLine("par2File = "+par2File);
			Assert.IsTrue("d:\\temp\\par2\\d$\\Projects\\test.txt.par2" == par2File);
		}

		[TestMethod]
		public void MapFileToPar2File_WindowsTest2()
		{
			if (!TestHelpers.IsWindows()) { Assert.Inconclusive(); return; }

			string dataFolder = "d:\\temp\\par2";
			string file = "c:\\Projects\\test.txt";

			string par2File = Helpers.MapFileToPar2File(file,dataFolder);
			Trace.WriteLine("par2File = "+par2File);
			Assert.IsTrue("d:\\temp\\par2\\c$\\Projects\\test.txt.par2" == par2File);
		}

		[TestMethod]
		public void MapFileToPar2File_LinuxTest1()
		{
			if (!TestHelpers.IsLinux()) { Assert.Inconclusive(); return; }

			string dataFolder = "/tmp/par2";
			string file = "/Projects/test.txt";

			string par2File = Helpers.MapFileToPar2File(file,dataFolder);
			Trace.WriteLine("par2File = "+par2File);
			Assert.IsTrue("/tmp/par2/Projects/test.txt.par2" == par2File);
		}

		[TestMethod]
		public void MarPar2ToOrigFile_WindowsTest1()
		{
			if (!TestHelpers.IsWindows()) { Assert.Inconclusive(); return; }

			string dataFolder = "d:\\temp\\par2";
			string par2File = "d:\\temp\\par2\\c$\\Projects\\test.txt.par2";

			string origFile = Helpers.MapPar2ToOrigFile(par2File,dataFolder);
			Trace.WriteLine("origFile = "+origFile);
			Assert.IsTrue("c:\\Projects\\test.txt" == origFile);
		}

		[TestMethod]
		public void MarPar2ToOrigFile_LinuxTest1()
		{
			if (!TestHelpers.IsLinux()) { Assert.Inconclusive(); return; }

			string dataFolder = "/tmp/par2";
			string par2File = "/tmp/par2/Projects/test.txt.par2";

			string origFile = Helpers.MapPar2ToOrigFile(par2File,dataFolder);
			Trace.WriteLine("origFile = "+origFile);
			Assert.IsTrue("/Projects/test.txt" == origFile);
		}

		[TestMethod]
		[ExpectedException(typeof(BadPathException))]
		public void MarPar2ToOrigFile_WindowsTest3()
		{
			if (!TestHelpers.IsWindows()) { Assert.Inconclusive(); return; }

			string dataFolder = "temp\\par2";
			string par2File = "temp\\par2\\Projects\\test.txt.par2";

			string origFile = Helpers.MapPar2ToOrigFile(par2File,dataFolder);
			Trace.WriteLine("origFile = "+origFile);
			Assert.IsTrue("Projects\\test.txt" == origFile);
		}

		[TestMethod]
		[ExpectedException(typeof(BadPathException))]
		public void MarPar2ToOrigFile_LinuxTest3()
		{
			if (!TestHelpers.IsLinux()) { Assert.Inconclusive(); return; }

			string dataFolder = "tmp/par2";
			string par2File = "tmp/par2/Projects/test.txt.par2";

			string origFile = Helpers.MapPar2ToOrigFile(par2File,dataFolder);
			Trace.WriteLine("origFile = "+origFile);
			Assert.IsTrue("Projects/test.txt" == origFile);
		}

		[TestMethod]
		public void MarPar2ToOrigFile_WindowsTest4()
		{
			if (!TestHelpers.IsWindows()) { Assert.Inconclusive(); return; }

			string dataFolder = "d:\\temp\\par2";
			string par2File = "d:\\temp\\par2\\c$\\Projects\\test.txt.vol00+11.par2";

			string origFile = Helpers.MapPar2ToOrigFile(par2File,dataFolder);
			Trace.WriteLine("origFile = "+origFile);
			Assert.IsTrue("c:\\Projects\\test.txt" == origFile);
		}

		[TestMethod]
		public void MarPar2ToOrigFile_LinuxTest4()
		{
			if (!TestHelpers.IsLinux()) { Assert.Inconclusive(); return; }

			string dataFolder = "/tmp/par2";
			string par2File = "/tmp/par2/Projects/test.txt.vol00+11.par2";

			string origFile = Helpers.MapPar2ToOrigFile(par2File,dataFolder);
			Trace.WriteLine("origFile = "+origFile);
			Assert.IsTrue("/Projects/test.txt" == origFile);
		}
	}
}
