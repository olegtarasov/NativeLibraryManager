using System;
using System.IO;
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
            LoggerFactory = new LoggerFactory(new[] {new XUnitLoggerProvider(outputHelper, new XUnitLoggerOptions())});
        }

        private LoggerFactory LoggerFactory { get; }

        
        [Fact]
        public void CanLoadLibraryFromAssemblyDirAndCallFunction()
        {
            File.Delete("libTestLib.dylib");
            File.Delete("libTestLib.so");
            File.Delete("TestLib.dll");
            
            var accessor = new ResourceAccessor(Assembly.GetExecutingAssembly());
            var libManager = new LibraryManager(
                Assembly.GetExecutingAssembly(),
                LoggerFactory,
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
        
        [Fact]
        public void CanLoadLibraryFromTempDirAndCallFunction()
        {
            File.Delete("libTestLib.dylib");
            File.Delete("libTestLib.so");
            File.Delete("TestLib.dll");
            
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                var accessor = new ResourceAccessor(Assembly.GetExecutingAssembly());
                var libManager = new LibraryManager(
                    tempDir,
                    LoggerFactory,
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
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}