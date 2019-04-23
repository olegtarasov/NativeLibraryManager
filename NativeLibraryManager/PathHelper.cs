using System;
using System.IO;
using System.Reflection;

namespace NativeLibraryManager
{
    public static class PathHelper
    {
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

        public static string CombineWithCurrentDirectory(this Assembly targetAssembly, string fileName)
        {
            string curDir = GetCurrentDirectory(targetAssembly);
            return !string.IsNullOrEmpty(curDir) ? Path.Combine(curDir, fileName) : fileName;
        }
    }
}