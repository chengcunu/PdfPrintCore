using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PdfPrintCore;

public class License
{
    /// <summary>
    /// Load license file
    /// </summary>
    /// <param name="filename">License file fullname</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static bool Load(string filename)
    {
        if (!File.Exists(filename))
            throw new FileNotFoundException(filename);

        nint ptr = Marshal.StringToHGlobalUni(filename);
        try
        {
            int verified = NativeMethods.LoadLicense(ptr);
            return verified == 0;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    /// <summary>
    /// Load license from stream
    /// </summary>
    /// <param name="stream">License file stream</param>
    /// <returns></returns>
    public static bool Load(Stream stream)
    {
        if (!stream.CanRead)
            throw new ArgumentException("Can not read stream.");

        byte[] buffer = new byte[stream.Length];
        stream.Seek(0, SeekOrigin.Begin);
        stream.ReadExactly(buffer);

        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            int verified = NativeMethods.LoadLicenseBuffer(handle.AddrOfPinnedObject(), buffer.Length);
            return verified == 0;
        }
        finally
        {
            handle.Free();
        }
    }

    /// <summary>
    /// Load license xml
    /// </summary>
    /// <param name="xml">License xml string</param>
    /// <returns></returns>
    public static bool LoadXml(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        nint ptr = Marshal.StringToHGlobalUni(xml);
        try
        {
            int verified = NativeMethods.LoadLicenseXml(ptr);
            return verified == 0;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
