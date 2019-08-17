using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parfait;

namespace Parfait.Test
{
	[TestClass]
	public class Par2CorruptTests
	{
		[TestMethod]
		public void CorruptPar2Data1()
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
				
				//double check they are actually the same
				Assert.IsTrue(TestData.AreFilesEqual(file,file2));

				//change the contents or original
				TestData.ModifyFileData(file,DateTimeOffset.Now.AddHours(-1),2);

				//original should be different
				Assert.IsFalse(TestData.AreFilesEqual(file,file2));

				//save off the par files for later
				File.Copy(par2File,par2File+".1");
				File.Copy(par2FileVol,par2FileVol+".1");

				//modify par2 data file
				TestData.ModifyFileData(par2File,null,1); //this files doesn't seem to matter
				TestData.ModifyFileData(par2FileVol,null,3);

				// should be different now
				Assert.IsFalse(TestData.AreFilesEqual(par2File,par2File+".1"));
				Assert.IsFalse(TestData.AreFilesEqual(par2FileVol,par2FileVol+".1"));

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
	}
}

// what happens if the par2 data gets corrupted ?