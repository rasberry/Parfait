using System;
using System.Reflection;

namespace Parfait.Test
{
	public static class TestHelpers
	{
		public static void ClearOptions()
		{
			var constructor = typeof(Options).GetConstructor(
				BindingFlags.Static | BindingFlags.NonPublic,null, new Type[0], null);
			constructor.Invoke(null, null);
		}

		public static int RunMain(string[] args)
		{
			try {
				return Program.MainMain(args);
			} finally {
				if (Options.Par2LogFile != null) {
					Options.Par2LogFile.Dispose();
				}
			}
		}
	}
}