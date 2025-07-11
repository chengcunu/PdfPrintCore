using Microsoft.Win32.SafeHandles;
using PdfPrintCore.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PdfPrintCore;

internal static partial class NativeMethods
{
    internal const string LIBRARY_NAME = "printcore";

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

    internal static string[] ListInstalledPrinters()
    {
        using var resultHandle = Imports.PRN_ListInstalledPrinters();
        string result = resultHandle.GetResult()!;
        if (result.StartsWith('/'))
            throw new NativeMethodException(result);

        string[] printerNames = result.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return [.. printerNames];
    }

    internal static List<PRN_PRINTER> ListPrinters()
    {
        using var resultHandle = Imports.PRN_ListPrinters();
        if (resultHandle.IsInvalid)
        {
            ThrowLastError();
        }

        string resultJson = resultHandle.GetResult()!;
        var result = JsonSerializer.Deserialize(resultJson, DefaultJsonSerializerContext.Default.ListPRN_PRINTER);
        return result!;
    }

    internal static PRN_PRINTER GetPrinter(string printer)
    {
        nint pPrinter = Marshal.StringToHGlobalUni(printer);

        try
        {
            using var resultHandle = Imports.PRN_GetPrinter(pPrinter);
            if (resultHandle.IsInvalid)
            {
                ThrowLastError();
            }

            string resultJson = resultHandle.GetResult()!;
            return JsonSerializer.Deserialize(resultJson, DefaultJsonSerializerContext.Default.PRN_PRINTER)!;
        }
        finally
        {
            Marshal.FreeHGlobal(pPrinter);
        }
    }

    internal static void Print(string? printer, byte[] buffer, string? password, PRINT_OPTIONS printOptions)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentNullException.ThrowIfNull(printOptions);

        GCHandle handle = default;
        nint pPrinter = nint.Zero;
        nint pPassword = nint.Zero;

        try
        {
            handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            pPrinter = Marshal.StringToHGlobalUni(printer);
            pPassword = Marshal.StringToHGlobalUni(password);

            bool successed = Imports.PRN_Print(handle.AddrOfPinnedObject(), buffer.Length, pPrinter, pPassword, printOptions);
            if (!successed)
            {
                ThrowLastError();
            }
        }
        finally
        {
            handle.Free();
            Marshal.FreeHGlobal(pPassword);
            Marshal.FreeHGlobal(pPrinter);
        }
    }

    internal static bool LoadLicense(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        nint pXml = Marshal.StringToHGlobalUni(xml);
        try
        {
            return Imports.PRN_LoadLicense(pXml);
        }
        finally
        {
            Marshal.FreeHGlobal(pXml);
        }
    }

    private static void ThrowLastError()
    {
        using var errorHandle = Imports.PRN_GetLastError();
        string errorMessage = errorHandle.IsInvalid
            ? "Unknow native method error."
            : errorHandle.GetResult()!;
        throw new NativeMethodException(errorMessage);
    }

    private static partial class Imports
    {
        [LibraryImport(LIBRARY_NAME), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial StringResultHandle PRN_ListInstalledPrinters();

        [LibraryImport(LIBRARY_NAME), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial StringResultHandle PRN_ListPrinters();

        [LibraryImport(LIBRARY_NAME), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial StringResultHandle PRN_GetPrinter(nint printer);

        [DllImport(LIBRARY_NAME), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static extern bool PRN_Print(nint buffer, int size, nint printer, nint password, PRINT_OPTIONS options);

        [LibraryImport(LIBRARY_NAME), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial StringResultHandle PRN_GetLastError();

        [LibraryImport(LIBRARY_NAME), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial void PRN_FreeHGlobal(nint handle);

        [LibraryImport(LIBRARY_NAME), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool PRN_LoadLicense(nint xml);
    }

    internal class PRN_PRINTER
    {
        public string? Name { get; set; }
        public bool IsDefault { get; set; }
        public int Status { get; set; }
        public string? StateMessage { get; set; }
        public string? Location { get; set; }
        public string? Comment { get; set; }
        public int JobCountSinceLastReset { get; set; }
        public string? DriverName { get; set; }
        public bool IsShared { get; set; }
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
        public DrawingPageEventHandler? DrawingPageCallback;
    }

    [Flags]
    internal enum RenderFlag : int
    {
        NONE = 0,
        FIT_TO_PAPER = 0x1,
        RENDER_FORMFILL = 0x2,
        PRINT_TO_FILE = 0x4,
    }

    [SecurityCritical]
    internal sealed class StringResultHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public StringResultHandle() : base(true)
        {
        }

        [SecurityCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
            {
                Imports.PRN_FreeHGlobal(handle);
            }

            return true;
        }

        public string? GetResult() => Marshal.PtrToStringUni(handle);
    }
}

[JsonSerializable(typeof(NativeMethods.PRN_PRINTER))]
[JsonSerializable(typeof(List<NativeMethods.PRN_PRINTER>))]
internal partial class DefaultJsonSerializerContext : JsonSerializerContext { }