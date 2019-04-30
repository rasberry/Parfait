using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parfait;

namespace Parfait.Test
{
	[TestClass]
	public class FileToParMapTests
	{
		public FileToParMapTests()
		{
			//var sw = new StreamWriter(File.OpenWrite("t.txt"));
			// Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
		}

		static bool IsWindows { get {
			return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		}}
		static bool IsLinux { get {
			return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		}}
		static bool IsMac { get {
			return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		}}

		[TestMethod]
		public void MapFileToPar2File_AbsolutePath1()
		{
			string file = IsWindows
				? "d:\\Projects\\test.txt"
				: "/Projects/test.txt"
			;

			string par2File = Helpers.MapFileToPar2File(file);
			Trace.WriteLine("par2File = "+par2File);
			string test = IsWindows
				? "d:\\Projects\\.par2\\test.txt.par2"
				: "/Projects/.par2/test.txt.par2"
			;
			Assert.IsTrue(test == par2File);
		}

		[TestMethod]
		public void MapFileToPar2File_RelativePath1()
		{
			string file = "test.txt";

			string par2File = Helpers.MapFileToPar2File(file);
			Trace.WriteLine("par2File = "+par2File);
			string test = IsWindows
				? ".par2\\test.txt.par2"
				: ".par2/test.txt.par2"
			;
			Assert.IsTrue(test == par2File);
		}

		[TestMethod]
		public void MapFileToPar2File_RelativePath2()
		{
			string file = IsWindows
				? "where\\test.txt"
				: "where/test.txt"
			;

			string par2File = Helpers.MapFileToPar2File(file);
			Trace.WriteLine("par2File = "+par2File);
			string test = IsWindows
				? "where\\.par2\\test.txt.par2"
				: "where/.par2/test.txt.par2"
			;
			Assert.IsTrue(test == par2File);
		}

		[TestMethod]
		public void MarPar2ToOrigFile_Absolute1()
		{
			string par2File = IsWindows
				? "d:\\temp\\.par2\\test.txt.par2"
				: "/temp/.par2/test.txt.par2"
			;

			string origFile = Helpers.MarPar2ToOrigFile(par2File);
			Trace.WriteLine("origFile = "+origFile);
			string test = IsWindows
				? "d:\\temp\\test.txt"
				: "/temp/test.txt"
			;
			Assert.IsTrue(test == origFile);
		}

		[TestMethod]
		[ExpectedException(typeof(BadPathException))]
		public void MarPar2ToOrigFile_Relative1()
		{
			string par2File = IsWindows
				? "temp\\.par2\\test.txt.par2"
				: "temp/.par2/test.txt.par2"
			;

			string origFile = Helpers.MarPar2ToOrigFile(par2File);
			Trace.WriteLine("origFile = "+origFile);
			string test = IsWindows
				? "temp\\test.txt"
				: "temp/test.txt"
			;
			Assert.IsTrue(test == origFile);
		}

		[TestMethod]
		public void MarPar2ToOrigFile_Absolute2()
		{
			string par2File = IsWindows
				? "d:\\temp\\.par2\\test.txt.vol00+11.par2"
				: "/temp/.par2/test.txt.vol00+11.par2"
			;

			string origFile = Helpers.MarPar2ToOrigFile(par2File);
			Trace.WriteLine("origFile = "+origFile);
			string test = IsWindows
				? "d:\\temp\\test.txt"
				: "/temp/test.txt"
			;
			Assert.IsTrue(test == origFile);
		}
	}
}
