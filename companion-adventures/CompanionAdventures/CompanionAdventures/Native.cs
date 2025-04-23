using CompanionAdventures.NativeUtil;

namespace CompanionAdventures;

public static class Native
{ 
    public static string Version()
    {
        return NativeString.Wrap(NativeMethods.version, NativeMethods.free_string);
    }
}