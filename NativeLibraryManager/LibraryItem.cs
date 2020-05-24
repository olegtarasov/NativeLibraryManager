using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace NativeLibraryManager
{
    /// <summary>
    /// Library binaries for specified platform and bitness.
    /// </summary>
    public class LibraryItem
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="platform">Binary platform.</param>
        /// <param name="bitness">Binary bitness.</param>
        /// <param name="files">A collection of files for this bitness and platform.</param>
        public LibraryItem(Platform platform, Bitness bitness, params LibraryFile[] files)
        {
            Platform = platform;
            Bitness = bitness;
            Files = files;
        }

        /// <summary>
        /// Library files.
        /// </summary>
        public LibraryFile[] Files { get; set; }

        /// <summary>
        /// Platform for which this binary is used.
        /// </summary>
        public Platform Platform { get; set; }

        /// <summary>
        /// Bitness for which this binary is used.
        /// </summary>
        public Bitness Bitness { get; set; }
        
        [Obsolete("targetAssembly is no longer required. Use the other overload.")]
        public void LoadItem(Assembly targetAssembly, bool loadLibrary = true)
        {
        }

        /// <summary>
        /// Unpacks the library and directly loads it if on Windows.
        /// </summary>
        /// <param name="targetDirectory">Target directory to which library is extracted.</param>
        /// <param name="loadLibrary">Load library explicitly.</param>
        public virtual void LoadItem(string targetDirectory, bool loadLibrary)
        {
            throw new InvalidOperationException("This item was never added to the LibraryManager. Create a LibraryManager, add this item and then call LibraryManager.LoadNativeLibrary().");
        }
    }
}