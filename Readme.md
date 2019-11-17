# Native dependency manager for .NET Standard libraries

This library helps you manage dependencies that you want to bundle with your .NET standard assembly. Originally it was developed
to be used with native shared libraries, but you can bundle any file you want.

The main feature of this library is cross-platform support. You tell it which dependencies are for which platform, and `LibraryManager`
will extract and load relevent files under each patform.

# How to use the library

A larger version of this tutorial is available as a blog post: https://www.olegtarasov.me/call-native-lib-from-net-core/

## Pack your dependencies as embedded resources

Put your dependencies somewhere relative to your project. Let's assume you have one library compiled for each platform: `TestLib.dll`
for Windows, `libTestLib.so` for Linux and `libTestLib.dylib` for macOs. Add these files as embedded resources to your `.csproj` as follows:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <!-- Some other stuff here -->

    <ItemGroup>
      <EmbeddedResource Include="libTestLib.dylib" />
      <EmbeddedResource Include="libTestLib.so" />
      <EmbeddedResource Include="TestLib.dll" />
    </ItemGroup>
</Project>
```

Now your dependencies will be compiled into your assembly as resources.

## Use `LibraryManager` to specify and extract dependencies

```csharp
private static void Main(string[] args)
{
    var accessor = new ResourceAccessor(Assembly.GetExecutingAssembly());
    var libManager = new LibraryManager(
        Assembly.GetExecutingAssembly(),
        new LibraryItem(Platform.MacOs, Bitness.x64,
            new LibraryFile("libTestLib.dylib", accessor.Binary("libTestLib.dylib"))),
        new LibraryItem(Platform.Windows, Bitness.x64, 
            new LibraryFile("TestLib.dll", accessor.Binary("TestLib.dll"))),
        new LibraryItem(Platform.Linux, Bitness.x64,
            new LibraryFile("libTestLib.so", accessor.Binary("libTestLib.so"))));
    
    libManager.LoadNativeLibrary();

    // Library is loaded, other code here
}
```

Each LibraryItem specifies a bundle of files that should be extracted for a specific platform. It this case we create 3 instances to 
support Windows, Linux and MacOs — all 64-bit. LibraryItem takes any number of LibraryFile objects. With these objects you specify 
the extracted file name and an actual binary file in the form of byte array. This is where `ResourceAccessor` comes in handy.

We should note that resource name you pass to ResourceAccessor is just a path to original file relative to project root with slashes `\` replaced with dots `..` So, for example, if we place some file in `Foo\Bar\lib.dll` project folder, we would adderss it as:

```csharp
accessor.Binary("Foo.Bar.lib.dll")
```

## Explicitly loading libraries under Windows and Linux

When called under Windows and Linux, `LoadNativeLibrary()` explicitly loads all dependencies into process memory. It uses `LoadLibraryEx` under
Windows and `dlopen` under Linux. If you don't want to load libraries explicitly, pass `False` to the `LoadNativeLibrary()` call:

```csharp
libManager.LoadNativeLibrary(False);
```

### Dependency load order under Linux

There is one more restriction when explicitly loading dependencies under Linux. If your native library depends on other native
libraries, which you would also like to bundle with you assembly, you should observe a special order in which you specify `LibraryFile` items.
You should put libraries with no dependencies ("leaves") first, and dependent libraries last. Use `ldd` to ensure you load dependent
libraries only when all dependencies are already loaded.