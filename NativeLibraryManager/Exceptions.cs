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