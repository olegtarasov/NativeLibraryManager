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
		private static readonly ILog _log = LogProvider.For<LibraryFile>();

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

		internal string UnpackResources()
		{
			string curDir;
			var ass = Assembly.GetExecutingAssembly().Location;
			if (string.IsNullOrEmpty(ass))
			{
				curDir = Environment.CurrentDirectory;
			}
			else
			{
				curDir = Path.GetDirectoryName(ass);
			}

			_log.Info($"Unpacking native library {FileName} to {curDir}");

			string path = !string.IsNullOrEmpty(curDir) ? Path.Combine(curDir, FileName) : FileName;

			UnpackFile(path, Resource);

			return path;
		}

		internal static void UnpackFile(string path, byte[] bytes)
		{
			if (File.Exists(path))
			{
				_log.Info($"File {path} already exists, computing hashes.");
				using (var md5 = MD5.Create())
				{
					using (var stream = File.OpenRead(path))
					{
						string fileHash = BitConverter.ToString(md5.ComputeHash(stream));
						string curHash = BitConverter.ToString(md5.ComputeHash(bytes));

						if (string.Equals(fileHash, curHash))
						{
							_log.Info($"Hashes are equal, no need to unpack.");
							return;
						}
					}
				}
			}

			File.WriteAllBytes(path, bytes);
		}

		internal void LoadWindowsLibrary(string path)
		{
			_log.Info($"Directly loading {path}...");
			var result = LoadLibraryEx(path, IntPtr.Zero, LoadLibraryFlags.LOAD_LIBRARY_SEARCH_APPLICATION_DIR | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_SYSTEM32 | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_USER_DIRS);
			_log.Info(result == IntPtr.Zero ? "FAILED!" : "Success");
		}
		
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