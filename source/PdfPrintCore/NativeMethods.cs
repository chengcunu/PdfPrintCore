using PdfPrintCore.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        nint pResult = Imports.PRN_ListInstalledPrinters();

        try
        {
            string result = Marshal.PtrToStringUni(pResult)!;
            if (result.StartsWith('/'))
                throw new NativeMethodException(result);

            string[] printerNames = result.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return [.. printerNames];
        }
        finally
        {
            Imports.PRN_FreeHGlobal(pResult);
        }
    }

    internal static List<PRN_PRINTER> ListPrinters()
    {
        nint pResult = Imports.PRN_ListPrinters();
        if (pResult == nint.Zero)
        {
            ThrowLastError();
        }

        try
        {
            string resultJson = Marshal.PtrToStringUni(pResult)!;
            var result = JsonSerializer.Deserialize(resultJson, DefaultJsonSerializerContext.Default.ListPRN_PRINTER);
            return result!;
        }
        finally
        {
            Imports.PRN_FreeHGlobal(pResult);
        }
    }

    internal static PRN_PRINTER GetPrinter(string printer)
    {
        nint pPrinter = Marshal.StringToHGlobalUni(printer);
        nint pResult = IntPtr.Zero;

        try
        {
            pResult = Imports.PRN_GetPrinter(pPrinter);
            if (pResult == nint.Zero)
            {
                ThrowLastError();
            }

            string resultJson = Marshal.PtrToStringUni(pResult)!;
            return JsonSerializer.Deserialize(resultJson, DefaultJsonSerializerContext.Default.PRN_PRINTER)!;
        }
        finally
        {
            Marshal.FreeHGlobal(pPrinter);
            Imports.PRN_FreeHGlobal(pResult);
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
        nint pError = Imports.PRN_GetLastError();
        try
        {
            if (pError != nint.Zero)
            {
                string errorMessage = Marshal.PtrToStringUni(pError)!;
                throw new NativeMethodException(errorMessage);
            }
            else
            {
                throw new NativeMethodException("Internal server error.");
            }
        }
        finally
        {
            Imports.PRN_FreeHGlobal(pError);
        }
    }

    private static partial class Imports
    {
        [LibraryImport(LIBRARY_NAME), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial nint PRN_ListInstalledPrinters();

        [LibraryImport(LIBRARY_NAME), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial nint PRN_ListPrinters();

        [LibraryImport(LIBRARY_NAME), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial nint PRN_GetPrinter(nint printer);

        [DllImport(LIBRARY_NAME), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static extern bool PRN_Print(nint buffer, int size, nint printer, nint password, PRINT_OPTIONS options);

        [LibraryImport(LIBRARY_NAME), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial nint PRN_GetLastError();

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
        public int JobCountSinceLastReset { get; set; }
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
    internal enum RenderFlag : int
    {
        NONE = 0,
        FIT_TO_PAPER = 0x1,
        RENDER_FORMFILL = 0x2,
        PRINT_TO_FILE = 0x4,
    }
}

[JsonSerializable(typeof(NativeMethods.PRN_PRINTER))]
[JsonSerializable(typeof(List<NativeMethods.PRN_PRINTER>))]
internal partial class DefaultJsonSerializerContext : JsonSerializerContext { }