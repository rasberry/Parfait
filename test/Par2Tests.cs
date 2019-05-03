using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parfait;

namespace Parfait.Test
{
	[TestClass]
	public class Par2Tests
	{
		static void ClearOptions()
		{
			var constructor = typeof(Options).GetConstructor(
				BindingFlags.Static | BindingFlags.NonPublic,null, new Type[0], null);
			constructor.Invoke(null, null);
		}

		static int RunMain(string[] args)
		{
			try {
				return Program.MainMain(args);
			} finally {
				if (Options.Par2LogFile != null) {
					Options.Par2LogFile.Dispose();
				}
			}
		}

		[TestMethod]
		public void BadInputsReturnOne()
		{
			ClearOptions();
			int result = RunMain(new string[] { "badinput" });
			Assert.AreEqual(1,result);
		}

		[TestMethod]
		public void CreatePar2Files()
		{
			ClearOptions();
			using (var setup = TestData.SetupTestFolder())
			{
				string folder = setup.Folder;
				int result = RunMain(new string[] { folder });
				Assert.AreEqual(0,result);
				string par2File = TestData.FileNamePar2(folder);
				Assert.IsTrue(File.Exists(par2File));
				string par2FileVol = TestData.FileNamePar2Vol(folder);
				Assert.IsTrue(File.Exists(par2FileVol));
			}
		}

		[TestMethod]
		public void RecreateTest1()
		{
			ClearOptions();
			using (var setup = TestData.SetupTestFolder())
			{
				string folder = setup.Folder;

				//create regular par2 files
				int result = RunMain(new string[] { folder });
				Assert.AreEqual(0,result);
				string par2File = TestData.FileNamePar2(folder);
				Assert.IsTrue(File.Exists(par2File));
				string par2FileVol = TestData.FileNamePar2Vol(folder);
				Assert.IsTrue(File.Exists(par2FileVol));

				//change the contents or original
				string file = Path.Combine(folder,TestData.TestFileGzName);
				TestData.ModifyFileData(file);

				//save off the par files for later
				File.Copy(par2File,par2File+".1");
				File.Copy(par2FileVol,par2FileVol+".1");

				//run again
				int result2 = RunMain(new string[] { folder });
				Assert.AreEqual(0,result2);

				//compare - files should be different
				Assert.IsFalse(TestData.AreFilesEqual(par2File,par2File+".1"));
				Assert.IsFalse(TestData.AreFilesEqual(par2FileVol,par2FileVol+".1"));
			}
		}


		[TestMethod]
		public void RestoreTest1()
		{
			ClearOptions();
			using (var setup = TestData.SetupTestFolder())
			using (var setup2 = TestData.SetupTestFolder())
			{
				string folder = setup.Folder;
				string folder2 = setup2.Folder;

				//create regular par2 files
				int result = RunMain(new string[] { folder });
				Assert.AreEqual(0,result);
				string par2File = TestData.FileNamePar2(folder);
				Assert.IsTrue(File.Exists(par2File));
				string par2FileVol = TestData.FileNamePar2Vol(folder);
				Assert.IsTrue(File.Exists(par2FileVol));

				//change the contents or original
				string file = Path.Combine(folder,TestData.TestFileGzName);
				TestData.ModifyFileData(file,DateTimeOffset.Now.AddHours(-1));

				//original should be different
				string file2 = TestData.MakeTestFileAs(folder2,TestData.TestFileGzName+".1");
				Assert.IsTrue(TestData.AreFilesEqual(file,file2));

				//save off the par files for later
				File.Copy(par2File,par2File+".1");
				File.Copy(par2FileVol,par2FileVol+".1");

				//run again with auto heal
				int result2 = RunMain(new string[] { folder, "-a" });
				Assert.AreEqual(0,result2);

				//compare - files should be same
				Assert.IsTrue(TestData.AreFilesEqual(par2File,par2File+".1"));
				Assert.IsTrue(TestData.AreFilesEqual(par2FileVol,par2FileVol+".1"));

				//original should be he same
				Assert.IsTrue(TestData.AreFilesEqual(file2,file));
			}
		}

		[TestMethod]
		public void RemoveTest1()
		{
			ClearOptions();
			using (var setup = TestData.SetupTestFolder())
			{
				string folder = setup.Folder;

				//create regular par2 files
				int result = RunMain(new string[] { folder });
				Assert.AreEqual(0,result);
				string par2File = TestData.FileNamePar2(folder);
				Assert.IsTrue(File.Exists(par2File));
				string par2FileVol = TestData.FileNamePar2Vol(folder);
				Assert.IsTrue(File.Exists(par2FileVol));

				//remove original
				string file = Path.Combine(folder,TestData.TestFileGzName);
				File.Delete(file);

				//run again
				int result2 = RunMain(new string[] { folder });
				Assert.AreEqual(0,result2);

				//par2 files should also be gone
				Assert.IsFalse(File.Exists(par2File));
				Assert.IsFalse(File.Exists(par2FileVol));
			}
		}
	}
}