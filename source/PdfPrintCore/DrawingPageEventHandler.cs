namespace PdfPrintCore;

/// <summary>
/// Represents the method that will handle the <see cref='PrintService.OnDrawingPage'/> event of a <see cref='PrintService'/>.
/// </summary>
/// <param name="data"></param>
public delegate void DrawingPageEventHandler(DrawingPageEventArgs data);