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

		[TestMethod]
		public void BadInputsReturnOne()
		{
			ClearOptions();
			int result = Program.MainMain(new string[] { "badinput" });
			Assert.AreEqual(1,result);
		}

		[TestMethod]
		public void CreatePar2Files()
		{
			ClearOptions();
			string folder = TestData.SetupTestFolder();
			
			int result = Program.MainMain(new string[] { folder });
			Assert.AreEqual(0,result);
			string par2File = Path.Combine(folder,".par2","TestFile.txt.gz.par2");
			Assert.IsTrue(File.Exists(par2File));
			string par2FileVol = Path.Combine(folder,".par2","TestFile.txt.gz.vol0+2.par2");
			Assert.IsTrue(File.Exists(par2FileVol));
			
			TestData.DeleteFolder(folder);
		}

		[TestMethod]
		public void RecreateTest1()
		{
			ClearOptions();
			string folder = TestData.SetupTestFolder();

			int result = Program.MainMain(new string[] { folder });
			Assert.AreEqual(0,result);
			string file = Path.Combine(folder,TestData.TestFileGzName);
			TestData.ModifyFileData(file);
			//TODO
			//rename resulting par2 files
			//run mainain again
			//compare - files should be different
		}

		//TODO test that changing original file causes recreate
		//TODO test that changing original before causes restore
		//TODO test that removing original also removes par2 files
	}
}