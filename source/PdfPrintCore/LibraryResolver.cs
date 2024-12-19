using System.Reflection;
using System.Runtime.InteropServices;

namespace PdfPrintCore;

/// <summary>
/// The native library resolver
/// </summary>
internal static class LibraryResolver
{
    static LibraryResolver()
    {
        NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, DllImportResolver);
    }

    private static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == NativeMethods.LibraryName)
            return NativeMethods.LoadNativeMethods();

        return nint.Zero;
    }

    internal static void EnsureRegistered()
    {
    }
}
