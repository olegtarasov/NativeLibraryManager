using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace NativeLibraryManager
{
    internal class LibraryItemInternal : LibraryItem
    {
        private readonly ILogger<LibraryItem> _logger;

        internal LibraryItemInternal(LibraryItem item, ILogger<LibraryItem> logger = null) 
            : base(item.Platform, item.Bitness, item.Files)
        {
            _logger = logger;
        }
        
        public override void LoadItem(string targetDirectory)
        {
	        foreach (var file in Files)
	        {
		        string path = Path.Combine(targetDirectory, file.FileName);

		        _logger?.LogInformation($"Unpacking native library {file.FileName} to {path}");

		        UnpackFile(path, file.Resource);

		        // if (!loadLibrary)
		        // {
			       //  continue;
		        // }
		        //
		        // if (Platform == Platform.Windows)
		        // {
			       //  LoadWindowsLibrary(path);
		        // }
		        // else if (Platform == Platform.Linux)
		        // {
			       //  LoadLinuxLibrary(path);
		        // }
	        }
        }

        private void UnpackFile(string path, byte[] bytes)
		{
			if (File.Exists(path))
			{
				_logger?.LogInformation($"File {path} already exists, computing hashes.");
				using (var md5 = MD5.Create())
				{
					using (var stream = File.OpenRead(path))
					{
						string fileHash = BitConverter.ToString(md5.ComputeHash(stream));
						string curHash = BitConverter.ToString(md5.ComputeHash(bytes));

						if (string.Equals(fileHash, curHash))
						{
							_logger?.LogInformation($"Hashes are equal, no need to unpack.");
							return;
						}
					}
				}
			}

			File.WriteAllBytes(path, bytes);
		}

		internal void LoadLinuxLibrary(string path)
		{
			_logger?.LogInformation($"Linux dlopen of {path}");
			var result = dlopen(path, RTLD_LAZY | RTLD_GLOBAL);
			if (result.Equals(null) )
			{
				_logger?.LogInformation($"Linux dlopen failed to load {path}");
			}
		}

		internal void LoadWindowsLibrary(string path)
		{
			_logger?.LogInformation($"Directly loading {path}...");
			var result = LoadLibraryEx(path, IntPtr.Zero, LoadLibraryFlags.LOAD_LIBRARY_SEARCH_APPLICATION_DIR | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_SYSTEM32 | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_USER_DIRS);
			_logger?.LogInformation(result == IntPtr.Zero ? "FAILED!" : "Success");
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