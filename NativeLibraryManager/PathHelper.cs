using System;
using System.IO;
using System.Reflection;

namespace NativeLibraryManager
{
    /// <summary>
    /// Contains useful functions to get paths relative to target assembly.
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Gets the directory specified assembly is located in.
        /// If the assembly was loaded from memory, returns environment
        /// working directory.
        /// </summary>
        /// <param name="targetAssembly">Assembly to get the directory from.</param>
        public static string GetCurrentDirectory(this Assembly targetAssembly)
        {
            string curDir;
            var ass = targetAssembly.Location;
            if (string.IsNullOrEmpty(ass))
            {
                curDir = Environment.CurrentDirectory;
            }
            else
            {
                curDir = Path.GetDirectoryName(ass);
            }

            return curDir;
        }

        /// <summary>
        /// Combines part of the path with assembly's directory.
        /// </summary>
        /// <param name="targetAssembly">Assembly to get directory from.</param>
        /// <param name="fileName">Right-hand part of the path.</param>
        public static string CombineWithCurrentDirectory(this Assembly targetAssembly, string fileName)
        {
            string curDir = GetCurrentDirectory(targetAssembly);
            return !string.IsNullOrEmpty(curDir) ? Path.Combine(curDir, fileName) : fileName;
        }
    }
}