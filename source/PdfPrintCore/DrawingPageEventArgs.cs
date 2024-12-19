using System.Runtime.InteropServices;

namespace PdfPrintCore;

/// <summary>
/// Provides data for the <see cref='PrintingService.OnDrawingPage'/> event.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct DrawingPageEventArgs
{
    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int CurrentPageIndex;

    /// <summary>
    /// Gets the total number of pages in the document.
    /// </summary>
    public int TotalPages;
}
