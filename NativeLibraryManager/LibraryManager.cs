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
		private readonly LibraryItem[] _items;
		private readonly Assembly _targetAssembly;

		private bool _libLoaded = false;

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="targetAssembly">Calling assembly.</param>
		/// <param name="items">Library binaries for different platforms.</param>
		public LibraryManager(Assembly targetAssembly, params LibraryItem[] items)
		{
			_targetAssembly = targetAssembly;
			_items = items;
		}

		/// <summary>
		/// Extract and load native library based on current platform and process bitness.
		/// Throws an exception if current platform is not supported.
		/// </summary>
		/// <param name="loadLibrary">
		/// Use LoadLibrary API call on Windows to explicitly load library into the process.
		/// </param>
		public void LoadNativeLibrary(bool loadLibrary = true)
		{
			if (_libLoaded)
			{
				return;
			}

			lock (_resourceLocker)
			{
				if (_libLoaded)
				{
					return;
				}

                var item = FindItem();
                item.LoadItem(_targetAssembly, loadLibrary);

				_libLoaded = true;
            }
        }

		/// <summary>
		/// Finds a library item based on current platform and bitness.
		/// </summary>
		/// <returns>Library item based on platform and bitness.</returns>
		/// <exception cref="NoBinaryForPlatformException"></exception>
		public LibraryItem FindItem()
		{
			var platform = GetPlatform();
			var bitness = Environment.Is64BitProcess ? Bitness.x64 : Bitness.x32;

            var item = _items.FirstOrDefault(x => x.Platform == platform && x.Bitness == bitness);
            if (item == null)
            {
                throw new NoBinaryForPlatformException($"There is no supported native library for platform '{platform}' and bitness '{bitness}'");
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
	}
}