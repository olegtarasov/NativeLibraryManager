using System;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace NativeLibraryManager
{
	/// <summary>
	/// A class to manage, extract and load native implementations of dependent libraries.
	/// </summary>
	public class LibraryManager
	{
		private readonly object _resourceLocker = new object();
		private readonly LibraryItemInternal[] _items;
		
		private bool _libLoaded = false;

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="targetAssembly">Calling assembly.</param>
		/// <param name="items">Library binaries for different platforms.</param>
		public LibraryManager(Assembly targetAssembly, params LibraryItem[] items) : this(targetAssembly, null, items)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="targetAssembly">
		/// Calling assembly. Native libraries will be extracted to the same directory <see cref="targetAssembly"/>
		/// resides at, or to current directory if you assembly is weird and doesn't have a location.
		/// </param>
		/// <param name="loggerFactory">Logger factory.</param>
		/// <param name="items">Library binaries for different platforms.</param>
		public LibraryManager(Assembly targetAssembly, ILoggerFactory loggerFactory, params LibraryItem[] items)
			: this(targetAssembly.GetCurrentDirectory(), loggerFactory, items)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="targetDirectory">Target directory to extract the libraries.</param>
		/// <param name="loggerFactory">Logger factory.</param>
		/// <param name="items">Library binaries for different platforms.</param>
		public LibraryManager(string targetDirectory, ILoggerFactory loggerFactory, params LibraryItem[] items)
		{
			TargetDirectory = targetDirectory;
			var itemLogger = loggerFactory?.CreateLogger<LibraryItem>();

			_items = items.Select(x => new LibraryItemInternal(x, itemLogger)).ToArray();
		}

		/// <summary>
		/// Target directory to which native libraries will be extracted. Defaults to directory
		/// in which targetAssembly, passed to <see cref="LibraryManager"/> constructor, resides.
		/// </summary>
		public string TargetDirectory { get; set; }

		/// <summary>
		/// Defines whether <see cref="LibraryManager"/> should add <see cref="TargetDirectory"/>
		/// to current process' library search path.
		///
		/// Default is <code>True</code>.
		/// </summary>
		public bool ModifyLibrarySearchPath { get; set; } = true;

		/// <summary>
		/// Defines whether shared libraries will be loaded explicitly. <code>LoadLibraryEx</code> is
		/// used on Windows and <code>dlopen</code> is used on Linux and MacOs to load libraries
		/// explicitly.
		///
		/// Default is <code>False</code>, since <see cref="ModifyLibrarySearchPath"/> is preferable.
		/// </summary>
		public bool LoadLibraryExplicit { get; set; } = false;

		/// <summary>
		/// Extract and load native library based on current platform and process bitness.
		/// Throws an exception if current platform is not supported.
		/// </summary>
		/// <param name="loadLibrary">
		/// Use LoadLibrary API call on Windows to explicitly load library into the process.
		/// </param>
		[Obsolete("This method is obsolete. Direct library loading is deprecated in favor of adding target path to environment library search path.")]
		public void LoadNativeLibrary(bool loadLibrary)
		{
			LoadNativeLibrary();
        }
		
		/// <summary>
		/// Extract and load native library based on current platform and process bitness.
		/// Throws an exception if current platform is not supported.
		/// </summary>
		public void LoadNativeLibrary()
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

				if (ModifyLibrarySearchPath)
				{
					EnvironmentManager.AddDirectoriesToSearchPath(item.Platform, TargetDirectory);
				}
				
				item.LoadItem(TargetDirectory, LoadLibraryExplicit);

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

		/// <summary>
		/// Gets the platform type.
		/// </summary>
		/// <exception cref="UnsupportedPlatformException">Thrown when platform is not supported.</exception>
		public static Platform GetPlatform()
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