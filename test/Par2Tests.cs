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
		[TestMethod]
		public void BadInputsReturnOne()
		{
			TestHelpers.ClearOptions();
			int result = TestHelpers.RunMain(new string[] { "badinput" });
			Assert.AreEqual(1,result);
		}

		[TestMethod]
		public void CreatePar2Files()
		{
			TestHelpers.ClearOptions();
			using (var setup = TestData.SetupTestFolder())
			{
				string folder = setup.Folder;
				int result = TestHelpers.RunMain(new string[] { folder });
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
			TestHelpers.ClearOptions();
			using (var setup = TestData.SetupTestFolder())
			{
				string folder = setup.Folder;

				//create regular par2 files
				int result = TestHelpers.RunMain(new string[] { folder });
				Assert.AreEqual(0,result);
				string par2File = TestData.FileNamePar2(folder);
				Assert.IsTrue(File.Exists(par2File));
				string par2FileVol = TestData.FileNamePar2Vol(folder);
				Assert.IsTrue(File.Exists(par2FileVol));

				//change the contents of original
				string file = Path.Combine(folder,TestData.TestFileGzName);
				TestData.ModifyFileData(file);

				//save off the par files for later
				File.Copy(par2File,par2File+".1");
				File.Copy(par2FileVol,par2FileVol+".1");

				//run again - should update par2 files since last modified is 
				// after par2 files were created
				int result2 = TestHelpers.RunMain(new string[] { folder });
				Assert.AreEqual(0,result2);

				//compare - files should be different
				Assert.IsFalse(TestData.AreFilesEqual(par2File,par2File+".1"));
				Assert.IsFalse(TestData.AreFilesEqual(par2FileVol,par2FileVol+".1"));
			}
		}


		[TestMethod]
		public void RestoreTest1()
		{
			TestHelpers.ClearOptions();
			using (var setup = TestData.SetupTestFolder())
			using (var setup2 = TestData.SetupTestFolder())
			{
				string folder = setup.Folder;
				string folder2 = setup2.Folder;

				//create regular par2 files
				int result = TestHelpers.RunMain(new string[] { folder });
				Assert.AreEqual(0,result);
				string par2File = TestData.FileNamePar2(folder);
				Assert.IsTrue(File.Exists(par2File));
				string par2FileVol = TestData.FileNamePar2Vol(folder);
				Assert.IsTrue(File.Exists(par2FileVol));

				string file = Path.Combine(folder,TestData.TestFileGzName);
				string file2 = Path.Combine(folder2,TestData.TestFileGzName);

				//change the contents or original
				TestData.ModifyFileData(file,DateTimeOffset.Now.AddHours(-1));

				//original should be different
				Assert.IsFalse(TestData.AreFilesEqual(file,file2));

				//save off the par files for later
				File.Copy(par2File,par2File+".1");
				File.Copy(par2FileVol,par2FileVol+".1");

				//run again with auto heal
				int result2 = TestHelpers.RunMain(new string[] { folder, "-a" });
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
			TestHelpers.ClearOptions();
			using (var setup = TestData.SetupTestFolder())
			{
				string folder = setup.Folder;

				//create regular par2 files
				int result = TestHelpers.RunMain(new string[] { folder });
				Assert.AreEqual(0,result);
				string par2File = TestData.FileNamePar2(folder);
				Assert.IsTrue(File.Exists(par2File));
				string par2FileVol = TestData.FileNamePar2Vol(folder);
				Assert.IsTrue(File.Exists(par2FileVol));

				//remove original
				string file = Path.Combine(folder,TestData.TestFileGzName);
				File.Delete(file);

				//run again
				int result2 = TestHelpers.RunMain(new string[] { folder });
				Assert.AreEqual(0,result2);

				//par2 files should also be gone
				Assert.IsFalse(File.Exists(par2File));
				Assert.IsFalse(File.Exists(par2FileVol));
			}
		}
	}
}