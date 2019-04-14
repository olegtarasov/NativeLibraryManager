using System;
using System.Runtime.Serialization;

namespace NativeLibraryManager
{
	/// <summary>
	/// Thrown when there is no binary for current platform and bitness.
	/// </summary>
	public class NoBinaryForPlatformException : Exception
	{
		/// <inheritdoc />
		public NoBinaryForPlatformException()
		{
		}

		/// <inheritdoc />
		protected NoBinaryForPlatformException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		/// <inheritdoc />
		public NoBinaryForPlatformException(string message) : base(message)
		{
		}

		/// <inheritdoc />
		public NoBinaryForPlatformException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}