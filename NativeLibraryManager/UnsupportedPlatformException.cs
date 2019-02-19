using System;
using System.Runtime.Serialization;

namespace NativeLibraryManager
{
	/// <summary>
	/// Thrown when platform is not supported.
	/// </summary>
	public class UnsupportedPlatformException : Exception
	{
		/// <inheritdoc />
		public UnsupportedPlatformException()
		{
		}

		/// <inheritdoc />
		protected UnsupportedPlatformException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		/// <inheritdoc />
		public UnsupportedPlatformException(string message) : base(message)
		{
		}

		/// <inheritdoc />
		public UnsupportedPlatformException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}