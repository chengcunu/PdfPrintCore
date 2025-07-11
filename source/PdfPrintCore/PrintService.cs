using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static PdfPrintCore.NativeMethods;

namespace PdfPrintCore;

/// <summary>
/// Defines a reusable object that sends output to the printer.
/// </summary>
public class PrintService
{
    private readonly PdfDocument _document;
    private PrintOptions? _printOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref='PrintService'/> class.
    /// </summary>
    /// <param name="document">The pdf document of printing.</param>
    public PrintService(PdfDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        _document = document;
    }

    /// <summary>
    /// Gets or sets the printer on which the document is printed.
    /// </summary>
    public PrintOptions PrintOptions
    {
        get => _printOptions??=new();
        set => _printOptions = value;
    }

    /// <summary>
    /// Occurs when a page is printed.
    /// </summary>
    public event DrawingPageEventHandler? OnDrawingPage;

    /// <summary>
    /// Print document
    /// </summary>
    /// <param name="printer">Printer name</param>
    public void Print(string? printer)
    {
        nint pCancelled = nint.Zero;
        PRINT_OPTIONS options = GetPrintOptions(ref pCancelled);
        NativeMethods.Print(printer, _document.Data, _document.Password, options);
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
            nint pCancelled = Marshal.AllocHGlobal(sizeof(int));
            try
            {
                PRINT_OPTIONS options = GetPrintOptions(ref pCancelled);
                using var ctr = cancellationToken.Register(() =>
                {
                    Marshal.WriteInt32(pCancelled, 1);
                });

                NativeMethods.Print(printer, _document.Data, _document.Password, options);
            }
            finally
            {
                Marshal.FreeHGlobal(pCancelled);
                pCancelled = nint.Zero;
            }
        }, cancellationToken);
    }

    internal PRINT_OPTIONS GetPrintOptions(ref nint pCancelled)
    {
        RenderFlag flags = RenderFlag.NONE;
        if (PrintOptions.FitToPaper) flags |= RenderFlag.FIT_TO_PAPER;
        if (PrintOptions.RenderFormFill) flags |= RenderFlag.RENDER_FORMFILL;
        if (PrintOptions.PrintToFile) flags |= RenderFlag.PRINT_TO_FILE;

        return new PRINT_OPTIONS()
        {
            JobName = !string.IsNullOrEmpty(PrintOptions.JobName) ? PrintOptions.JobName : "document",
            Copies = PrintOptions.Copies ?? 0,
            ColorModel = (int)PrintOptions.ColorModel,
            Blackthreshole = PrintOptions.Blackthreshole ?? 0,
            Flags = flags,
            DrawingPageCallback = OnDrawingPage,
            CancelledPtr = pCancelled,
        };
    }
}