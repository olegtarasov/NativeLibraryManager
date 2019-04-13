using System;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using NativeLibraryManager.Logging;

namespace NativeLibraryManager
{
	/// <summary>
	/// A class to manage, extract and load native implementations of dependent libraries.
	/// </summary>
	public class LibraryManager
	{
		private static readonly ILog _log = LogProvider.For<LibraryManager>();

		private readonly object _resourceLocker = new object();
		private readonly string _name;
		private readonly LibraryItem[] _items;

		private bool _libLoaded = false;

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="name">Library name. Used in logs.</param>
		/// <param name="items">Library binaries for different platforms.</param>
		public LibraryManager(string name, params LibraryItem[] items)
		{
			_name = name;
			_items = items;
		}

        /// <summary>
        /// Extract and load native library based on current platform and process bitness.
        /// </summary>
        public bool TryLoadNativeLibrary()
        {
            return LoadNativeLibrary(false);
        }

        /// <summary>
        /// Extract and load native library based on current platform and process bitness.
        /// Throws an exception if current platform is not supported.
        /// </summary>
        public void LoadNativeLibrary()
        {
            LoadNativeLibrary(true);
        }

        private bool LoadNativeLibrary(bool throwIfNotSupported)
		{
			if (_libLoaded)
			{
				return true;
			}

			lock (_resourceLocker)
			{
				if (_libLoaded)
				{
					return true;
				}

                var item = FindItem(throwIfNotSupported);
                if (item == null)
                {
                    return false;
                }

				string file = UnpackResources(item);

				if (item.Platform == Platform.Windows)
				{
					LoadLibrary(file);
				}

				_libLoaded = true;
                return true;
            }
        }


		private void LoadLibrary(string path)
		{
			_log.Info($"Directly loading {path}...");
			var result = LoadLibraryEx(path, IntPtr.Zero, LoadLibraryFlags.LOAD_LIBRARY_SEARCH_APPLICATION_DIR | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_SYSTEM32 | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_USER_DIRS);
			_log.Info(result == IntPtr.Zero ? "FAILED!" : "Success");
		}

		private string UnpackResources(LibraryItem item)
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

			_log.Info($"Unpacking native library {_name} to {curDir}");

			string path = !string.IsNullOrEmpty(curDir) ? Path.Combine(curDir, item.FileName) : item.FileName;

			UnpackFile(path, item.Resource);

			return path;
		}

		private static void UnpackFile(string path, byte[] bytes)
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

		private LibraryItem FindItem(bool throwIfNotSupported)
		{
			var platform = GetPlatform();
			var bitness = Environment.Is64BitProcess ? Bitness.x64 : Bitness.x32;

            var item = _items.FirstOrDefault(x => x.Platform == platform && x.Bitness == bitness);
            if (item == null && throwIfNotSupported)
            {
                throw new NoBinaryForPlatform($"There is no supported native library for platform '{platform}' and bitness '{bitness}'");
            }

            return item;
        }

		private Platform GetPlatform()
		{
			string windir = Environment.GetEnvironmentVariable("windir");
			if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir))
			{
				return Platform.Windows;
			}
			else if (File.Exists(@"/proc/sys/kernel/ostype"))
			{
				string osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
				if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase))
				{
					// Note: Android gets here too
					return Platform.Linux;
				}
				else
				{
					throw new UnsupportedPlatformException($"Unsupported OS: {osType}");
				}
			}
			else if (File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
			{
				// Note: iOS gets here too
				return Platform.MacOs;
			}
			else
			{
				throw new UnsupportedPlatformException("Unsupported OS!");
			}

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