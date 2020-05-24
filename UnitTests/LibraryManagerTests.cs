using System;
using System.Diagnostics;
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
    /// <summary>
    /// We need to use a separate process per test so that we get a clear picture (libraries must be unloaded before the next test).
    /// </summary>
    public class LibraryManagerTests
    {
        [DllImport("TestLib")]
        private static extern int hello();

        private readonly ITestOutputHelper _outputHelper;

        public LibraryManagerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }
        
        [Fact]
        public void CanLoadLibraryFromCurrentDirAndCallFunction()
        {
            RunTest(nameof(CanLoadLibraryFromCurrentDirAndCallFunction)).ShouldBe(0);
        }
        
        [Fact]
        public void CanLoadLibraryFromTempDirAndCallFunction()
        {
            RunTest(nameof(CanLoadLibraryFromTempDirAndCallFunction)).ShouldBe(0);
        }

        private int RunTest(string name)
        {
            var process = Process.Start(new ProcessStartInfo("dotnet", $"TestProcess.dll {name}")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });

            process.OutputDataReceived += ProcessOnOutputDataReceived;
            process.ErrorDataReceived += ProcessOnOutputDataReceived;
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            process.CancelOutputRead();
            process.CancelErrorRead();

            return process.ExitCode;
        }

        private void ProcessOnOutputDataReceived (object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e?.Data))
            {
                _outputHelper.WriteLine(e.Data);
            }
        }
    }
}