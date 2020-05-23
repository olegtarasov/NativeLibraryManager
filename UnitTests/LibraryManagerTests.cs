using System.Reflection;
using System.Runtime.InteropServices;
using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.Logging;
using NativeLibraryManager;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests
{
    public class LibraryManagerTests
    {
        [DllImport("TestLib")]
        private static extern int hello();
        
        public LibraryManagerTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        private ITestOutputHelper OutputHelper { get; }

        
        [Fact]
        public void CanLoadLibraryAndCallFunction()
        {
            var factory = new LoggerFactory(new[] {new XUnitLoggerProvider(OutputHelper, new XUnitLoggerOptions())});
            var accessor = new ResourceAccessor(Assembly.GetExecutingAssembly());
            var libManager = new LibraryManager(
                Assembly.GetExecutingAssembly(),
                factory,
                new LibraryItem(Platform.MacOs, Bitness.x64,
                    new LibraryFile("libTestLib.dylib", accessor.Binary("libTestLib.dylib"))),
                new LibraryItem(Platform.Windows, Bitness.x64, 
                    new LibraryFile("TestLib.dll", accessor.Binary("TestLib.dll"))),
                new LibraryItem(Platform.Linux, Bitness.x64,
                    new LibraryFile("libTestLib.so", accessor.Binary("libTestLib.so"))));
    
            libManager.LoadNativeLibrary();

            int result = hello();
            result.ShouldBe(42);
        }
    }
}