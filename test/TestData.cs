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

		public static string SetupTestFolder()
		{
			string dir = Path.GetTempFileName();
			if (File.Exists(dir)) { File.Delete(dir); }
			Directory.CreateDirectory(dir);
			string file = Path.Combine(dir,TestFileGzName);
			File.WriteAllBytes(file,TestFileGzip);
			return dir;
		}

		public static void DeleteFolder(string dir)
		{
			if (!Directory.Exists(dir)) { return; }
			Directory.Delete(dir,true);
		}

		public static void ModifyFileData(string file, DateTimeOffset? lastWriteTime = null)
		{
			using (var fs = File.Open(file,FileMode.Open,FileAccess.ReadWrite,FileShare.Read)) {
				int len = (int)(fs.Length & int.MaxValue);
				for(int i=0; i<5; i++) {
					long next = Rnd.Next(len);
					fs.Seek(next,SeekOrigin.Begin);
					fs.WriteByte((byte)'A');
				}
			}
		}

		static Random Rnd = new Random();

		public const string TestFileGzName = "TestFile.txt.gz";
	}
}
