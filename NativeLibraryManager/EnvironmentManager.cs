using System;
using System.Collections.Generic;
using System.Linq;

namespace NativeLibraryManager
{
    /// <summary>
    /// Environment variables helper.
    /// </summary>
    public static class EnvironmentManager
    {
        private static readonly Dictionary<Platform, string> _varNames = new Dictionary<Platform, string>
        {
            {Platform.MacOs, "DYLD_LIBRARY_PATH"},
            {Platform.Linux, "LD_LIBRARY_PATH"},
            {Platform.Windows, "PATH"}
        };
        
        /// <summary>
        /// Adds specified directories to library search path depending on a platform.
        /// </summary>
        /// <param name="platform">Current platform.</param>
        /// <param name="dirs">Directories to add.</param>
        public static void AddDirectoriesToSearchPath(Platform platform, params string[] dirs)
        {
            if (dirs.Length == 0 || !_varNames.TryGetValue(platform, out string varName))
            {
                return;
            }

            string curVar = Environment.GetEnvironmentVariable(varName) ?? string.Empty;
            var parts = curVar.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            var filtered = dirs.Where(x => !parts.Contains(x)).ToArray();            
            
            if (filtered.Length == 0)
            {
                return;
            }
            
            Environment.SetEnvironmentVariable(varName, string.Join(";", parts.Concat(filtered)));
        }
    }
}