using System.Runtime.InteropServices;

namespace CompanionAdventures.NativeUtil;

public sealed class NativeString: IDisposable
{
    private IntPtr _pointer;
    private Action<IntPtr> _freer;
    private string? _value = null;
    private bool _disposed = false;

    public NativeString(Func<IntPtr> alloc, Action<IntPtr> freer)
    {
        this._freer = freer ?? throw new ArgumentNullException(nameof(freer)); // Freer is mandatory
        this._pointer = alloc(); // Check freer is not null before calling pointer()
    }
    
    /// <summary>
    /// Returns the internal string representation. Calls Marshal.PtrToStringUTF8 internally.
    /// Note: This uses a singleton pattern so that Marshal.PtrToStringUTF8 is only called once.
    /// </summary>
    public string Value
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (_pointer == IntPtr.Zero)
                return string.Empty; // Return empty if the native pointer was null

            _value ??= Marshal.PtrToStringUTF8(_pointer);
            
            // Adjust encoding if necessary
            return _value ?? string.Empty;
        }
    }
    
    private void FreeUnmanagedResources()
    {
        if (!_disposed)
        {
            if (_pointer != IntPtr.Zero)
            {
                _freer(_pointer);
                _pointer = IntPtr.Zero;
            }
            
            _disposed = true;
        }
    }
    
    public void Dispose()
    {
        FreeUnmanagedResources();
        // Suppress finalization. If Dispose is called, the finalizer doesn't need to run.
        GC.SuppressFinalize(this);
    }
    
    // Finalizer in case Dispose is not called explicitly.
    ~NativeString()
    {
        FreeUnmanagedResources();
    }

    public static string Wrap(Func<IntPtr> alloc, Action<IntPtr> free)
    {
        using var nativeString = new NativeString(alloc, free);
        return nativeString.Value;
    }
}