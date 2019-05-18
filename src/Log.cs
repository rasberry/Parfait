using System;

namespace Parfait
{
	public static class Log
	{
		public static void Message(string m)
		{
			InternalWrite(m);
		}

		public static void Error(string m)
		{
			InternalWrite("E: "+m,true);
		}

		public static void Warning(string m)
		{
			InternalWrite("W: "+m);
		}

		public static void Info(string m)
		{
			if (Options.Verbose) {
				InternalWrite("I: "+m);
			}
		}

		public static void Debug(string m)
		{
			#if DEBUG
			InternalWrite("D: "+m);
			#endif
		}

		public static void File(string m)
		{
			if (Options.Par2LogFile != null) {
				Options.Par2LogFile.WriteLine(m);
			}
		}

		static void InternalWrite(string m, bool isError = false)
		{
			if (isError) {
				Console.Error.WriteLine(m);
			} else {
				Console.WriteLine(m);
			}
			File(m);
		}
	}
}