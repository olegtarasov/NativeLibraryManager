using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CLAP;
using Microsoft.Extensions.Logging;
using NativeLibraryManager;
using Serilog;
using Serilog.Extensions.Logging;

namespace TestProcess
{
    internal class Program
    {
        internal static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            var factory = new LoggerFactory(new[] {new SerilogLoggerProvider() });

            try
            {
                return Parser.Run(args, new UnitTests(factory));
            }
            catch (Exception e)
            {
                Log.ForContext<Program>().Error($"Test failed with exception: {e.GetType().Name}, {e.Message}");
                return 1;
            }
        }
    }

    internal class UnitTests
    {
        [DllImport("TestLib")]
        private static extern int hello();

        private readonly ILoggerFactory _factory;
        private readonly ILogger<UnitTests> _logger;

        public UnitTests(ILoggerFactory factory)
        {
            _factory = factory;
            _logger = factory.CreateLogger<UnitTests>();
            try
            {
                File.Delete("libTestLib.dylib");
                File.Delete("libTestLib.so");
                File.Delete("TestLib.dll");
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Failed to cleanup libraries before running a test: {e.Message}");
            }
        }
        (Action<string> LogInformation, Action<string> LogWarning)  LogCreate(Type  type)
        {
            var logger = _factory.CreateLogger(type);
            return (info => logger.LogInformation(info), war => logger.LogWarning(war));
        }
       [Verb]
        public int CanLoadLibraryFromCurrentDirAndCallFunction()
        {
            var accessor = new ResourceAccessor(Assembly.GetExecutingAssembly());

            var libManager = new LibraryManager(LogCreate,
                new LibraryItem(Platform.MacOs, Bitness.x64,
                    new LibraryFile("libTestLib.dylib", accessor.Binary("libTestLib.dylib"))),
                new LibraryItem(Platform.Windows, Bitness.x64, 
                    new LibraryFile("TestLib.dll", accessor.Binary("TestLib.dll"))),
                new LibraryItem(Platform.Linux, Bitness.x64,
                    new LibraryFile("libTestLib.so", accessor.Binary("libTestLib.so"))));
        
            libManager.LoadNativeLibrary();
        
            int result = hello();
            
            _logger.LogInformation($"Function result is {result}");

            return result == 42 ? 0 : 1;
        }

        [Verb]
        public int CanLoadLibraryFromTempDirAndCallFunction()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            
            var accessor = new ResourceAccessor(Assembly.GetExecutingAssembly());
            var libManager = new LibraryManager(
                tempDir,
                LogCreate,
                new LibraryItem(Platform.MacOs, Bitness.x64,
                    new LibraryFile("libTestLib.dylib", accessor.Binary("libTestLib.dylib"))),
                new LibraryItem(Platform.Windows, Bitness.x64,
                    new LibraryFile("TestLib.dll", accessor.Binary("TestLib.dll"))),
                new LibraryItem(Platform.Linux, Bitness.x64,
                    new LibraryFile("libTestLib.so", accessor.Binary("libTestLib.so"))))
            {
                LoadLibraryExplicit = true
            };

            var item = libManager.FindItem();
            libManager.LoadNativeLibrary();

            int result;
            try
            {
                result = hello();
            }
            catch (DllNotFoundException)
            {
                if (item.Platform == Platform.MacOs)
                {
                    _logger.LogWarning("Hit an expected exception on MacOs. Skipping test.");
                    return 0;
                }

                throw;
            }

            _logger.LogInformation($"Function result is {result}");

            return result == 42 ? 0 : 1;
        }
    }
}