using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PdfPrintCore;

internal static partial class NativeMethods
{
    internal const string LibraryName = "printcore";

    static NativeMethods()
    {
        LibraryResolver.EnsureRegistered();
    }

    internal static nint LoadNativeMethods()
    {
        string architecuter = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException("Only supported x64, arm64")
        };

        string libName;
        string runtimeIdentifier;

        if (PlatformConfiguration.IsWindows)
        {
            runtimeIdentifier = $"win-{architecuter}";
            libName = "printcore.dll";
        }
        else if (PlatformConfiguration.IsLinux)
        {
            runtimeIdentifier = PlatformConfiguration.IsGlibc ? $"linux-{architecuter}" : $"linux-{PlatformConfiguration.LinuxFlavor}-{architecuter}";
            libName = "libprintcore.so";
        }
        else if (PlatformConfiguration.IsOsx)
        {
            runtimeIdentifier = $"osx-{architecuter}";
            libName = "libprintcore.dylib";
        }
        else
        {
            throw new NotSupportedException("Only support win, linux, osx");
        }

        string libPath = Path.Combine(AppContext.BaseDirectory, "runtimes", runtimeIdentifier, "native", libName);
        if (!NativeLibrary.TryLoad(libPath, out var _lib))
        {
            if (!NativeLibrary.TryLoad(libName, out _lib))
            {
                libPath = Path.Combine(AppContext.BaseDirectory, libName);
                NativeLibrary.TryLoad(libPath, out _lib);
            }
        }

        return _lib;
    }

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint ListPrinters();

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint GetPrinterInfo(nint printer);

    [DllImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static extern nint Print(nint buffer, int size, nint printer, nint password, PRINT_OPTIONS options);

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int LoadLicense(nint filename);

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int LoadLicenseXml(nint license);

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int LoadLicenseBuffer(nint buffer, int size);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PRINTER_INFO
    {
        public int Status;
        public string? StateMessage;
        public string? Location;
        public int JobCountSinceLastReset;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal class PRINT_OPTIONS
    {
        public string? JobName;
        public int Copies;
        public int ColorModel;
        public int Blackthreshole;
        public RenderFlag Flags;
        public nint CancelledPtr;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public DrawingPageEventHandle? DrawingPageCallback;
    }

    [Flags]
    public enum RenderFlag : int
    {
        None = 0,
        FitToPaper = 0x1,
        RenderFormFill = 0x2,
    }
}