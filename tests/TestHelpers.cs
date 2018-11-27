using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Parfait.Tests
{
	public static class TestHelpers
	{
		public static bool IsWindows() {
			return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		}
		public static bool IsLinux() {
			return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		}
		public static bool IsMac() {
			return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		}

		public static string TestDataFolder {
			get {
				return Path.Combine(Path.GetTempPath(),"par2_test_" + SessionName);
			}
		}

		static string _session_temp = null;
		static string SessionName {
			get {
				if (_session_temp == null) {
					_session_temp = DateTime.UtcNow.ToString("yyyyMMdd-hhmmss");
				}
				return _session_temp;
			}
		}

	}
}