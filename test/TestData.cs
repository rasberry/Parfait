using System;
using System.IO;
using System.Text;
using Parfait;

namespace Parfait.Test
{
	public static class TestData
	{
		static string TestFileB64 = ""
			+"H4sICMnhxFwCA1Rlc3RGaWxlLnR4dACtVk1v3DYQvftXTC89bRYJbMcJcjDsOrETuIXjdbtnShpJ"
			+"zFLkhh/e6ua/ESD9c/4lfUOuguamBXraJYecefNm5lHPT99v3SPT6JKn95YHzWFBrfMUex4psjFi"
			+"K/YPKpkYls9P/xy9oDuHQ/e67pVvnp++Bbowg7Kq3izo1dnp66Oj56fvNww3KlKrjAmkLRmJtdOx"
			+"p14PgU2LBSL0CtvWkdePyswKcPw2B3jo2eMmP7KnnQqkqHOuwV9PuKyoUg1tWdVcfN5yjDgZHa20"
			+"p08u8LanS2U3yHnrOeiGbSTXSvJ070ZlaOVqzXGUzVtnG2cX9CmZERDeHC8BKTiqdeRGslNkfgT4"
			+"nLStxwWt/HJBFwN7XStL+EU+NiKcwQ+OKguEzuioa63gfMXbyEMFS47wE40GxaHG7Wyh8Mp1wB16"
			+"kAvmAlPaFkNrWM1j8Tj7vyyog9GTA+eihLTIxHaJZ/k6yb5undtQxeggXkgN0Dg4CoStRqbSRrns"
			+"FfdYz/J7mv1eOQuToPJuRwG4wAU4yY1pWXd95Tzo0G3ZKiyhYDscQ490RoV5nPzcujuXTAPm0aCo"
			+"cO4l+lUCg2Jk+HsKEZ0bKcC0ISkFWmdjJSyCfElNx9NuYD4EwVpmDw2HwAF8lcaSmuzEEJSeR99Z"
			+"dnaBDun6SDesPLD/XTM3QYAdMGzrvvAtiQzS4xUIqJg8w884x9HJyR5M9Inpg9eMnkB2Mm4Vg8k7"
			+"FwKHoJ09wN0fjjqlbci961KkraxmOZh6K0hjIbOsT0a3fE6QFgv6S3G/Jswp+H/QA7/by6OK2Z9g"
			+"X8XUQh9wT7IZVMPQi1kAXu+HBgDEt1wvata6BG6UJDbL0dm+RABdRk56XrQQdXLAKLPxw5C5hmEa"
			+"x1kR3uQId7A5q22XE79UzUIwa/sl+WnzGmEP8HgD1c0s57l9UAYaDLD5/RGm/4Sy7UTmhdclrSfi"
			+"Fa2SfXGlIdKYTAkMKWz4lznP08sc+Vo0YUG/QYrUIivxPW9TVFGLzItqYMg1xL72uAgvTTm1f29k"
			+"DAd0MM/Kdh8zD9EAdSQ8dUlVhstQXzvTnBOyGfDKhCXaD5vT8pz+0j7O0+HTV4VVFaJQtoFMruX/"
			+"rLvlPVhBJOp+3zbTxwAezMcMItdmpCzjxTbCUs/UtlLzj/G/ouYoAifA2pE+5k7iMD0/1In0QgUP"
			+"eCv+V+Vc69AXEXA0JNACTPk5MHgaCd8c01qcz6TgX+QS7rF2CQAA"
		;

		public static byte[] TestFileGzip { get {
			var raw = Convert.FromBase64String(TestFileB64);
			return raw;
		}}

		public interface IFolderSetup : IDisposable
		{
			string Folder { get; }
		}
		
		public static IFolderSetup SetupTestFolder(bool keep = false)
		{
			return new TestFolderSetup(keep);
		}

		class TestFolderSetup : IFolderSetup
		{
			public TestFolderSetup(bool keep)
			{
				Folder = CreateTestFolder();
				Keep = keep;
			}
			public string Folder { get; private set; }
			public bool Keep;
			public void Dispose()
			{
				if (!Keep && Folder != null) {
					DeleteFolder(Folder);
				}
			}
		}

		static string CreateTestFolder()
		{
			string dir = Path.GetTempFileName();
			if (File.Exists(dir)) { File.Delete(dir); }
			Directory.CreateDirectory(dir);
			string file = Path.Combine(dir,TestFileGzName);
			File.WriteAllBytes(file,TestFileGzip);
			return dir;
		}

		static void DeleteFolder(string dir)
		{
			if (!Directory.Exists(dir)) { return; }
			Directory.Delete(dir,true);
		}

		public static void ModifyFileData(string file, DateTimeOffset? lastWriteTime = null, int howManyChanges = 1)
		{
			using (var fs = File.Open(file,FileMode.Open,FileAccess.ReadWrite,FileShare.Read)) {
				//int len = (int)(fs.Length & int.MaxValue);
				int len = (int)fs.Length;
				for(int i=0; i<howManyChanges; i++) {
					long next = Rnd.Next(len);
					fs.Seek(next,SeekOrigin.Begin);
					fs.WriteByte(0x041); //'A'
				}
			}

			if (lastWriteTime != null) {
				File.SetLastWriteTimeUtc(file,lastWriteTime.Value.UtcDateTime);
			}
		}

		public static string MakeTestFileAs(string folder, string name)
		{
			string file = Path.Combine(folder,name);
			File.WriteAllBytes(file,TestFileGzip);
			return file;
		}

		public static string FileNamePar2(string folder)
		{
			string par2File = Path.Combine(folder,".par2","TestFile.txt.gz.par2");
			return par2File;
		}
		public static string FileNamePar2Vol(string folder)
		{
			string par2FileVol = Path.Combine(folder,".par2","TestFile.txt.gz.vol00+12.par2");
			return par2FileVol;
		}

		public static bool AreFilesEqual(string one,string two)
		{
			if (!File.Exists(one)) { return false; }
			if (!File.Exists(two)) { return false; }
			using(var fs1 = File.Open(one,FileMode.Open,FileAccess.Read,FileShare.Read))
			using(var fs2 = File.Open(two,FileMode.Open,FileAccess.Read,FileShare.Read))
			{
				if (fs1.Length != fs2.Length) { return false; }
				int b1,b2;
				do {
					b1 = fs1.ReadByte();
					b2 = fs2.ReadByte();
					if (b1 != b2) { return false; }
				} while (b1 != -1 && b2 != -1);
			}
			return true;
		}

		static Random Rnd = new Random();

		public const string TestFileGzName = "TestFile.txt.gz";
	}
}
