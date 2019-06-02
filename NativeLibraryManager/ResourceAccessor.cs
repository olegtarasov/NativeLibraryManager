using System;
using System.IO;
using System.Reflection;

namespace NativeLibraryManager
{
    /// <summary>
    /// A helper class to load resources from an assembly.
    /// </summary>
    public class ResourceAccessor
    {
        private readonly Assembly _assembly;
        private readonly string _assemblyName;

        /// <summary>
        /// Creates a resource accessor for the specified assembly.
        /// </summary>
        public ResourceAccessor(Assembly assembly)
        {
            _assembly = assembly;
            _assemblyName = _assembly.GetName().Name;
        }

        /// <summary>
        /// Gets a resource with specified name as an array of bytes.
        /// </summary>
        /// <param name="name">Resource name with folders separated by dots.</param>
        /// <exception cref="InvalidOperationException">
        /// When resource is not found.
        /// </exception>
        public byte[] Binary(string name)
        {
            using (var stream = new MemoryStream())
            {
                var resource = _assembly.GetManifestResourceStream(GetName(name));
                if (resource == null)
                {
                    throw new InvalidOperationException("Resource not available.");
                }
                
                resource.CopyTo(stream);

                return stream.ToArray();
            }
        }

        private string GetName(string name) =>
            name.StartsWith(_assemblyName) ? name : $"{_assemblyName}.{name}";
    }
}