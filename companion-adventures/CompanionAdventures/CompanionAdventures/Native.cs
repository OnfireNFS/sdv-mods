using System.Reflection;
using System.Runtime.InteropServices;
using StardewModdingAPI;

namespace CompanionAdventures;

public static class Native
{
    private const String LIBRARY = "libcompanionadventures";

    private static string DetermineRid(string platform)
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return $"{platform.ToLower()}-x64";
        }
        else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            return $"{platform.ToLower()}-arm64";
        }
        else
        {
            throw new PlatformNotSupportedException(
                "\n\n Companion Adventures does not support this platform:" +
                $"\n  Platform: {platform}" +
                $"\n  Architecture: {RuntimeInformation.ProcessArchitecture}" +
                $"\n  rid: {platform.ToLower()}-{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}" +
                "\n\n If you believe this is a mistake please contact the mod developer\n"
            );
        }
    }
    
    private static IntPtr RuntimeResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        var handle = IntPtr.Zero;
        // Early Exit: If the supplied library name doesn't match the library we are looking for return
        if (libraryName != LIBRARY)
        {
            return handle;
        }
        
        // TODO: Is there a way to access the smapi monitor outside of a mod instance here?
        
        // Determine OS and Architecture
        string rid = "";
        string fileExtension = "";

        var platform = Constants.TargetPlatform;
        
        if (platform == GamePlatform.Windows)
        {
            fileExtension = ".dll";
            rid = DetermineRid("Windows");
        }
        else if (platform == GamePlatform.Mac)
        {
            fileExtension = ".dylib";
            rid = DetermineRid("OSX");
        }
        else if (platform == GamePlatform.Linux)
        {
            fileExtension = ".so";
            rid = DetermineRid("Linux");
        }
        else if (platform == GamePlatform.Android)
        {
            Console.WriteLine(
                "\n\n Companion Adventures may not work correctly on this platform:" +
                "\n  Platform: Android" +
                $"\n  Architecture: {RuntimeInformation.ProcessArchitecture}" +
                $"\n  rid: linux-{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}\n"
            );
            
            fileExtension = ".so";
            rid = DetermineRid("Linux");
        }
        else
        {
            throw new PlatformNotSupportedException(
                "\n\n Companion Adventures does not support this platform:" +
                $"\n  Platform: {RuntimeInformation.OSDescription}" +
                $"\n  Architecture: {RuntimeInformation.ProcessArchitecture}" +
                "\n\n If you believe this is a mistake please contact the mod developer\n"
            );
        }

        // Early Exit: If rid is somehow empty at this point
        // Unreachable!: This shouldn't be possible because if the platform is unknown an exception is thrown
        // If the platform is known rid shouldn't be null
        if (string.IsNullOrEmpty(rid))
        {
            return handle;
        }
        
        // Construct the path relative to the application's base directory
        string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
        string nativeLibPath = Path.Combine(assemblyLocation, "runtimes", rid, "native", LIBRARY + fileExtension);

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
        
        return handle;
    }
    
    static Native()
    {
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), RuntimeResolver);
    }

    [DllImport(LIBRARY)]
    public static extern Boolean loaded();
    
    [DllImport(LIBRARY)]
    public static extern Int32 add(Int32 a, Int32 b);
        
    [DllImport(LIBRARY)]
    public static extern Int32 subtract(Int32 a, Int32 b);
}