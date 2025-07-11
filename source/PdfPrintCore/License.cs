using System;
using System.IO;
using System.Text;

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
        ArgumentNullException.ThrowIfNull(filename);
        if (!File.Exists(filename))
            throw new FileNotFoundException(filename);

        using FileStream fs = new(filename, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(fs, Encoding.UTF8);
        string xml = reader.ReadToEnd();
        return Load(xml);
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

        using var reader = new StreamReader(stream, Encoding.UTF8);
        string xml = reader.ReadToEnd();
        return Load(xml);
    }

    /// <summary>
    /// Load license xml
    /// </summary>
    /// <param name="xml">License xml string</param>
    /// <returns></returns>
    public static bool LoadXml(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);
        return NativeMethods.LoadLicense(xml);
    }
}
