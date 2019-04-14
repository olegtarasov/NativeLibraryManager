namespace NativeLibraryManager
{
    public abstract class LibraryItemBase
    {
        protected LibraryItemBase(Platform platform, Bitness bitness)
        {
            Platform = platform;
            Bitness = bitness;
        }

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