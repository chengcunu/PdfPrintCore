using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static PdfPrintCore.NativeMethods;

namespace PdfPrintCore;

/// <summary>
/// Defines a reusable object that sends output to the printer.
/// </summary>
public class PrintingService
{
    private readonly PdfDocument _document;
    private PrintSettings? _printSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref='PrintingService'/> class.
    /// </summary>
    /// <param name="document">The pdf document of printing.</param>
    public PrintingService(PdfDocument document)
    {
        _document = document;
    }

    /// <summary>
    /// Gets or sets the printer on which the document is printed.
    /// </summary>
    public PrintSettings PrinterSettings
    {
        get => _printSettings??=new();
        set => _printSettings = value;
    }

    /// <summary>
    /// Occurs when a page is printed.
    /// </summary>
    public event DrawingPageEventHandle? OnDrawingPage;

    /// <summary>
    /// Print document
    /// </summary>
    /// <param name="printer">Printer name</param>
    public void Print(string? printer)
    {
        nint zero = nint.Zero;
        PRINT_OPTIONS options = GetPrintOptions(ref zero);
        Print(printer, _document.Data, _document.Password, options);
    }

    /// <summary>
    /// Print document
    /// </summary>
    /// <param name="printer">Printer name</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task PrintAsync(string? printer, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            nint ptr_cancelled = Marshal.AllocHGlobal(sizeof(int));

            try
            {
                PRINT_OPTIONS options = GetPrintOptions(ref ptr_cancelled);
                using var ctr = cancellationToken.Register(() =>
                {
                    Marshal.WriteInt32(ptr_cancelled, 1);
                });

                Print(printer, _document.Data, _document.Password, options);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr_cancelled);
                ptr_cancelled = nint.Zero;
            }
        }, cancellationToken);
    }

    private PRINT_OPTIONS GetPrintOptions(ref nint ptr_cancelled)
    {
        RenderFlag flags = RenderFlag.None;

        if (this.PrinterSettings.FitToPaper)
            flags |= RenderFlag.FitToPaper;

        if (this.PrinterSettings.RenderFormFill)
            flags |= RenderFlag.RenderFormFill;

        return new PRINT_OPTIONS()
        {
            JobName = !string.IsNullOrEmpty(this.PrinterSettings.JobName) ? this.PrinterSettings.JobName : "document",
            Copies = this.PrinterSettings.Copies ?? 0,
            ColorModel = (int)this.PrinterSettings.ColorModel,
            Blackthreshole = this.PrinterSettings.Blackthreshole ?? 0,
            Flags = flags,
            DrawingPageCallback = OnDrawingPage,
            CancelledPtr = ptr_cancelled,
        };
    }

    private static void Print(string? printer, byte[] buffer, string? password, PRINT_OPTIONS printOptions)
    {
        GCHandle handle = default;
        nint ptrPrinter = nint.Zero;
        nint ptrPassword = nint.Zero;

        try
        {
            handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            ptrPrinter = Marshal.StringToHGlobalUni(printer);
            ptrPassword = Marshal.StringToHGlobalUni(password);

            nint ptrRet = NativeMethods.Print(handle.AddrOfPinnedObject(), buffer.Length, ptrPrinter, ptrPassword, printOptions);
            try
            {
                if (ptrRet != nint.Zero)
                {
                    string errorMessage = Marshal.PtrToStringUni(ptrRet)!;
                    throw new Exception(errorMessage);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptrRet);
            }
        }
        finally
        {
            handle.Free();
            Marshal.FreeHGlobal(ptrPassword);
            Marshal.FreeHGlobal(ptrPrinter);
        }
    }
}