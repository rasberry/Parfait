using System;
using System.IO;
using System.Text;
using Parfait;

namespace Parfait.Test
{
	public static class TestData
	{
		public static byte[] TestFileData = null;
		public static byte[] TestFileBytes(int size = 1024) {
			if (TestFileData == null) {
				var raw = new byte[size];
				Rnd.NextBytes(raw);
				TestFileData = raw;
			}
			return TestFileData;
		}

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
			File.WriteAllBytes(file,TestFileBytes());
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
				int len = (int)fs.Length;
				for(int i=0; i<howManyChanges; i++) {
					long next = Rnd.Next(len);
					fs.Seek(next,SeekOrigin.Begin);
					int b = fs.ReadByte();
					fs.Seek(next,SeekOrigin.Begin); //have to seek again since read advances the cursor
					byte n = (byte)(b == 0x41 ? 0x42 : 0x41); //'B' or 'A'
					fs.WriteByte(n);
				}
			}

			if (lastWriteTime != null) {
				File.SetLastWriteTimeUtc(file,lastWriteTime.Value.UtcDateTime);
			}
		}

		public static string MakeTestFileAs(string folder, string name)
		{
			string file = Path.Combine(folder,name);
			File.WriteAllBytes(file,TestFileBytes());
			return file;
		}

		public static string FileNamePar2(string folder)
		{
			string par2File = Path.Combine(folder,".par2","TestFile.txt.gz.par2");
			return par2File;
		}
		public static string FileNamePar2Vol(string folder)
		{
			string par2FileVol = Path.Combine(folder,".par2","TestFile.txt.gz.vol00+13.par2");
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

		//const long TicksPerDay = 24*60*60*10000000L;
		//static Random InitRnd() {
		//	int seed = (int)(DateTime.UtcNow.Ticks / TicksPerDay);
		//	return new Random(seed);
		//}
		static Random Rnd = new Random();

		public const string TestFileGzName = "TestFile.txt.gz";
	}
}
