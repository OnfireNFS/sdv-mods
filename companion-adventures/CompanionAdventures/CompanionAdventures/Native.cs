using System.Runtime.InteropServices;
using CompanionAdventures.NativeUtil;

namespace CompanionAdventures;

public static class Native
{ 
    public static string Version()
    {
        return NativeString.Wrap(NativeMethods.version(), NativeMethods.free_string);
    }

    public static string SayHello(string name)
    {
        return NativeString.Wrap(NativeMethods.say_hello(name), NativeMethods.free_string);
    }
}