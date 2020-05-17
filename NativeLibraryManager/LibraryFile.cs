using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using NativeLibraryManager.Logging;

namespace NativeLibraryManager
{
	/// <summary>
	/// A class to store the information about native library file.
	/// </summary>
	public class LibraryFile
	{
		private static readonly ILog Log = LogProvider.For<LibraryFile>();

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
		/// Gets the path to which current file will be unpacked.
		/// </summary>
		/// <param name="targetAssembly">Target assembly for which to compute the path.</param>
		public string GetUnpackPath(Assembly targetAssembly)
		{
			return targetAssembly.CombineWithCurrentDirectory(FileName);
		}

		internal string UnpackResources(Assembly targetAssembly)
		{
			string path = GetUnpackPath(targetAssembly);
			
			Log.Info($"Unpacking native library {FileName} to {path}");

			UnpackFile(path, Resource);

			return path;
		}

		internal static void UnpackFile(string path, byte[] bytes)
		{
			if (File.Exists(path))
			{
				Log.Info($"File {path} already exists, computing hashes.");
				using (var md5 = MD5.Create())
				{
					using (var stream = File.OpenRead(path))
					{
						string fileHash = BitConverter.ToString(md5.ComputeHash(stream));
						string curHash = BitConverter.ToString(md5.ComputeHash(bytes));

						if (string.Equals(fileHash, curHash))
						{
							Log.Info($"Hashes are equal, no need to unpack.");
							return;
						}
					}
				}
			}

			File.WriteAllBytes(path, bytes);
		}

		internal static void LoadLinuxLibrary(string path)
		{
			Log.Info($"Linux dlopen of {path}");
			var result = dlopen(path, RTLD_LAZY | RTLD_GLOBAL);
			if (result.Equals(null) )
			{
				Log.Info($"Linux dlopen failed to load {path}");
			}
		}

		internal static void LoadWindowsLibrary(string path)
		{
			Log.Info($"Directly loading {path}...");
			var result = LoadLibraryEx(path, IntPtr.Zero, LoadLibraryFlags.LOAD_LIBRARY_SEARCH_APPLICATION_DIR | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_SYSTEM32 | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_USER_DIRS);
			Log.Info(result == IntPtr.Zero ? "FAILED!" : "Success");
		}

		#region dlopen

		private const int RTLD_LAZY = 0x00001; //Only resolve symbols as needed
		private const int RTLD_GLOBAL = 0x00100; //Make symbols available to libraries loaded later
		[DllImport("dl")]
		private static extern IntPtr dlopen (string file, int mode);

		#endregion
		
		#region LoadLibraryEx

		[System.Flags]
		private enum LoadLibraryFlags : uint
		{
			DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
			LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
			LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
			LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
			LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
			LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200,
			LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000,
			LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100,
			LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800,
			LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400,
			LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

		#endregion
	}
}