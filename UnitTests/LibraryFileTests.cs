using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using NativeLibraryManager;
using Shouldly;
using Xunit;

namespace UnitTests
{
    public class LibraryFileTests
    {
        [Fact]
        public void CanUnpackFile()
        {
            var file = new LibraryFile("TestFile.txt", SampleData.SampleFile);

            string unpackedFile = file.UnpackResources(Assembly.GetExecutingAssembly());
            
            unpackedFile.ShouldNotBeNullOrEmpty();
            File.Exists(unpackedFile).ShouldBeTrue();
            File.ReadAllText(unpackedFile).ShouldBe(Encoding.UTF8.GetString(SampleData.SampleFile));
            File.Delete(unpackedFile);
        }
    }
}