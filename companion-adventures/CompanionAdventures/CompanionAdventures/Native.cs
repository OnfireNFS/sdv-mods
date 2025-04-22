using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CompanionAdventures;

public static class Native
{
    internal const String LibraryName = "libcompanionadventures";

    static Native()
    {
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), RuntimeResolver);
    }
    
    private static IntPtr RuntimeResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        IntPtr handle = IntPtr.Zero;
        if (libraryName == LibraryName)
        {
            // Determine OS and Architecture
            string rid = "";
            string fileExtension = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileExtension = ".dll";
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                {
                    rid = "windows-x64";
                }
                // Add other Windows architectures if needed (e.g., Arm64)
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                fileExtension = ".so";
                 if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                {
                    rid = "linux-x64";
                }
                else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                {
                    rid = "linux-arm64";
                }
                // Add other Linux architectures if needed
            }
            // Add OSPlatform.OSX if needed

            if (!string.IsNullOrEmpty(rid))
            {
                // Construct the path relative to the application's base directory
                string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
                string nativeLibPath = Path.Combine(assemblyLocation, "runtimes", rid, "native", LibraryName + fileExtension);

                Console.WriteLine($"Attempting to load native library from: {nativeLibPath}");

                // Try to load the library from the calculated path
                if (File.Exists(nativeLibPath))
                {
                   NativeLibrary.TryLoad(nativeLibPath, out handle);
                }
                else
                {
                   Console.WriteLine($"Native library not found at expected path: {nativeLibPath}");
                }
            }
        }

        // Return the handle (IntPtr.Zero if loading failed or name didn't match)
        // Returning Zero tells the runtime to fallback to default loading mechanisms (if any)
        return handle;
    }

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 add(Int32 a, Int32 b);
        
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 subtract(Int32 a, Int32 b);
}