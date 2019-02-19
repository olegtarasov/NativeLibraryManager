using System;
using System.Runtime.Serialization;

namespace NativeLibraryManager
{
	/// <summary>
	/// Thrown when there is no binary for current platform and bitness.
	/// </summary>
	public class NoBinaryForPlatform : Exception
	{
		/// <inheritdoc />
		public NoBinaryForPlatform()
		{
		}

		/// <inheritdoc />
		protected NoBinaryForPlatform(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		/// <inheritdoc />
		public NoBinaryForPlatform(string message) : base(message)
		{
		}

		/// <inheritdoc />
		public NoBinaryForPlatform(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}