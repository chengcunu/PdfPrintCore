namespace PdfPrintCore;

/// <summary>
/// Represents the method that will handle the <see cref='PrintingService.OnDrawingPage'/> event of a <see cref='PrintingService'/>.
/// </summary>
/// <param name="data"></param>
public delegate void DrawingPageEventHandle(DrawingPageEventArgs data);