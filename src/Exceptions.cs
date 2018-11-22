using System;

namespace Parfait
{
	public class BadPathException : Exception
	{
		public BadPathException() : base() {}
		public BadPathException(string message) : base(message) {}
		public BadPathException(string message, Exception innerException) : base(message,innerException) {}
	}
}
