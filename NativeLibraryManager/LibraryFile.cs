using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace NativeLibraryManager
{
	/// <summary>
	/// A class to store the information about native library file.
	/// </summary>
	public class LibraryFile
	{
		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="fileName">Filename to use when extracting the library.</param>
		/// <param name="resource">Library binary.</param>
		public LibraryFile(string fileName, byte[] resource)
		{
			FileName = fileName;
			Resource = resource;
		}

		/// <summary>
		/// Filename to use when extracting the library.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Library binary.
		/// </summary>
		public byte[] Resource { get; set; }

		/// <summary>
		/// Specifies whether this file is a shared library, which can be loaded explicitly with
		/// <code>LoadLibraryEx</code> on Windows and <code>dlopen</code> on Linux and MacOs.
		///
		/// Default is <code>True</code>, but explicit loading is disabled by default with
		/// <see cref="LibraryManager.LoadLibraryExplicit"/>.
		///
		/// Set this to <code>False</code> if this file is not a library, but a supporting file which
		/// shouldn't be loaded explicitly when <see cref="LibraryManager.LoadLibraryExplicit"/> is <code>True</code>. 
		/// </summary>
		public bool CanLoadExplicitly { get; set; } = true;
		
		/// <summary>
		/// Gets the path to which current file will be unpacked.
		/// </summary>
		/// <param name="targetAssembly">Target assembly for which to compute the path.</param>
		[Obsolete("This method is no longer used to determine unpack path. It's determined at LibraryManager, once for all files.", true)]
		public string GetUnpackPath(Assembly targetAssembly)
		{
			return null;
		}
	}
}