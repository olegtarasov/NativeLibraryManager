namespace NativeLibraryManager
{
    /// <summary>
    /// Library binaries for specified platform and bitness.
    /// </summary>
    public class LibraryItem : LibraryItemBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="platform">Binary platform.</param>
        /// <param name="bitness">Binary bitness.</param>
        /// <param name="files">A collection of files for this bitness and platform.</param>
        public LibraryItem(Platform platform, Bitness bitness, params LibraryFile[] files) : base(platform, bitness)
        {
            Files = files;
        }

        /// <summary>
        /// Library files.
        /// </summary>
        public LibraryFile[] Files { get; set; }

        /// <summary>
        /// Unpacks the library and directly loads it if on Windows.
        /// </summary>
        /// <param name="loadLibrary">
        /// Use LoadLibrary API call on Windows to explicitly load library into the process.
        /// </param>
        public virtual void LoadItem(bool loadLibrary = true)
        {
            for (int i = 0; i < Files.Length; i++)
            {
                string file = Files[i].UnpackResources();
                if (Platform == Platform.Windows && loadLibrary)
                {
                    Files[i].LoadWindowsLibrary(file);
                }  
            }
        }
    }
}