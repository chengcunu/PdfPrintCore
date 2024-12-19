namespace PdfPrintCore;

/// <summary>
/// Information about how a document should be printed.
/// </summary>
public class PrintSettings
{
    /// <summary>
    /// Sets the name to display to the user while printing the document; for example, in a print status dialog or a printer queue.
    /// </summary>
    public string? JobName { get; set; }

    /// <summary>
    /// Sets the number of copies to print.
    /// </summary>
    public short? Copies { get; set; }

    /// <summary>
    /// Sets the threshole of black color model [-100, 100]
    /// </summary>
    public short? Blackthreshole { get; set; }

    /// <summary>
    /// Sets whether allowed to shrink to fit the printing area and auto rotate, default is true. 
    /// </summary>
    public bool FitToPaper { get; set; } = true;

    /// <summary>
    /// Sets whether to render the PDF form, default is false.
    /// </summary>
    public bool RenderFormFill { get; set; }

    /// <summary>
    ///  Sets whether the output is in color
    /// </summary>
    public ColorModel ColorModel { get; set; } = ColorModel.Default;
}
