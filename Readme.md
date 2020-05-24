![GitHub Workflow Status](https://img.shields.io/github/workflow/status/olegtarasov/NativeLibraryManager/Build%20and%20publish%20Nuget?style=flat-square)
![Nuget](https://img.shields.io/nuget/v/NativeLibraryManager?style=flat-square)
![Donwloads](https://img.shields.io/nuget/dt/NativeLibraryManager?label=Nuget&style=flat-square)

# Native dependency manager for .NET Standard libraries

This library helps you manage dependencies that you want to bundle with your .NET standard assembly. Originally it was developed
to be used with native shared libraries, but you can bundle any file you want.

The main feature of this library is cross-platform support. You tell it which dependencies are for which platform, and `LibraryManager`
will extract and load relevant files under each patform.

# How to use the library

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

We should note that resource name you pass to ResourceAccessor is just a path to original file relative to project root with slashes 
`\\` replaced with dots `.` So, for example, if we place some file in `Foo\Bar\lib.dll` project folder, we would adderss it as:

```csharp
accessor.Binary("Foo.Bar.lib.dll")
```

## Target dependency directory

When you pass an assembly to `LibraryManager` constructor, it determines that assembly's location and extracts dependencies to that
directory. If the specified assembly doesn't have a location, current directory is used.

Alternatively, you can use another constructor overload, where you specify the target directory itself. This may be useful when you
are not sure whether your app directory is writable.

Anyway, you can change `LibraryManager.TargetDirectory` property to change target directory any time before calling 
`LibraryManager.LoadNativeLibrary()`.

## Library search path modification

By default `LibraryManager` adds `LibraryManager.TargetDirectory` to current process library search path environment variable.
It's `LD_LIBRARY_PATH` for Linux; `DYLD_LIBRARY_PATH` for MacOs; and plain ol' `PATH` for Windows.

You may not want to modify these variables in some circumstances, so you can disable this behavior by setting
`LibraryManager.ModifyLibrarySearchPath` to `false` before calling `LibraryManager.LoadNativeLibrary()`.

## Explicit library loading

**Warning! I strongly suggest that you use library search path modification (on by default) instead of loading libraries explicitly.
Tests show that explicit library loading doesn't work in many cases and is generally useless, at least on MacOs.**

In previous versions of `NativeLibraryManager` the default behavior was to explicitly load every file using `LoadLibraryEx` on Windows
and `dlopen` on Linux (explit loading wasn't implemented for MacOs). This approach was quite rigid and caused at least two problems:

1. There might have been some supporting files which didn't require explicit loading. You couldn't load some files and not load the others.
2. You should have observed a specific order in which you defined `LibraryFile`s if some of them were dependent on others.

Starting from v. `1.0.21` explicit loading is disabled by default in favor of `LibraryManager.ModifyLibrarySearchPath`. 
I recommend this approach as more flexible and error-proof.

Nevertheless, sometimes you might want to load libraries explicitly. To do so, set `LibraryManager.LoadLibraryExplicit` to `True` before
calling `LibraryManager.LoadNativeLibrary()`.

You can also set `LibraryFile.CanLoadExplicitly` to `False` for supporting files, which you want to exclude from explicit loading.

When `LibraryManager.LoadLibraryExplicit` is `True`, `LoadLibraryEx` will be called to explicitly load libraries on Windows, and
`dlopen` will be called on Linux **and MacOs**.

### Dependency load order with explicit loading

As mentioned earlier, there is a restriction when explicitly loading dependencies. If your native library depends on other native
libraries, which you would also like to bundle with you assembly, you should observe a special order in which you specify `LibraryFile` items.
You should put libraries with no dependencies ("leaves") first, and dependent libraries last. Use `ldd` on Linux or `Dependency Walker` on 
Windows to discover the dependecies in your libraries.

## Logging with `Microsoft.Extensions.Logging`

`LibraryManager` writes a certain amount of logs in case you would like to debug something. This library uses .NET Core 
`Microsoft.Extensions.Logging` abstraction, so in order to enable logging, just obtain an instance of 
`Microsoft.Extensions.Logging.ILoggerFactory` and pass it as a parameter to `LibraryManager` constructor.
