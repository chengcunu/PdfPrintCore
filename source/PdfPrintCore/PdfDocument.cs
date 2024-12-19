using System;
using System.IO;

namespace PdfPrintCore;

public class PdfDocument : IDisposable
{
    private readonly byte[] _data;
    private readonly string? _password;

    private bool disposedValue;

    /// <summary>
    ///Initializes a new instance of the <see cref='PdfDocument'/> class.
    /// </summary>
    /// <param name="buffer">PDF file bytes</param>
    /// <param name="password">PDF document password</param>
    public PdfDocument(byte[] buffer, string? password = null)
    {
        _data = buffer;
        _password = password;
    }

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    /// <param name="stream">PDF file stream</param>
    /// <param name="password">PDF document password</param>
    public PdfDocument(Stream stream, string? password = null)
    {
        _data = new byte[stream.Length];
        _password = password;

        stream.Seek(0, SeekOrigin.Begin);
        stream.ReadExactly(_data);
    }

    /// <summary>
    ///  Initializes a new instance
    /// </summary>
    /// <param name="filename">PDF file fullname</param>
    /// <param name="password">PDF document password</param>
    public PdfDocument(string filename, string? password = null)
    {
        _password = password;

        using FileStream fs = new(filename, FileMode.Open, FileAccess.Read);
        _data = new byte[fs.Length];
        fs.ReadExactly(_data);
    }

    public byte[] Data => _data;
    public string? Password => _password;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing) { }
            disposedValue =true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
