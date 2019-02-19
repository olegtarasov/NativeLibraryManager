namespace NativeLibraryManager
{
	/// <summary>
	/// A class to store the information about native library for concrete platform and bitness.
	/// </summary>
	public class LibraryItem
	{
		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="fileName">Filename to use when extracting the library.</param>
		/// <param name="resource">Library binary.</param>
		/// <param name="platform">Platform for which this binary is used.</param>
		/// <param name="bitness">Bitness for which this binary is used.</param>
		public LibraryItem(string fileName, byte[] resource, Platform platform, Bitness bitness)
		{
			FileName = fileName;
			Resource = resource;
			Platform = platform;
			Bitness = bitness;
		}

		/// <summary>
		/// Filename to use when extracting the library.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Library binary.
		/// </summary>
		public byte[] Resource { get; set; }

		/// <summary>
		/// Platform for which this binary is used.
		/// </summary>
		public Platform Platform { get; set; }

		/// <summary>
		/// Bitness for which this binary is used.
		/// </summary>
		public Bitness Bitness { get; set; }
	}
}