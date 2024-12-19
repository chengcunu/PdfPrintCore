using System;
using System.Runtime.InteropServices;

namespace PdfPrintCore;

public static partial class PlatformConfiguration
{
    private static string? linuxFlavor;

    private static readonly Lazy<bool> isGlibcLazy = new(IsGlibcImplementation);

    public static bool IsUnix => IsOsx || IsLinux;

    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsOsx => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static bool IsArm
    {
        get
        {
            Architecture processArchitecture = RuntimeInformation.ProcessArchitecture;
            return processArchitecture == Architecture.Arm || processArchitecture == Architecture.Arm64;
        }
    }

    public static bool Is64Bit => IntPtr.Size == 8;

    public static string? LinuxFlavor
    {
        get
        {
            if (!IsLinux)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(linuxFlavor))
            {
                return linuxFlavor;
            }

            if (!IsGlibc)
            {
                return "musl";
            }

            return null;
        }
        set
        {
            linuxFlavor = value;
        }
    }

    public static bool IsGlibc => IsLinux &&isGlibcLazy.Value;

    private static bool IsGlibcImplementation()
    {
        try
        {
            gnu_get_libc_version();
            return true;
        }
        catch (TypeLoadException)
        {
            return false;
        }
    }

    [LibraryImport("libc")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial nint gnu_get_libc_version();
}
