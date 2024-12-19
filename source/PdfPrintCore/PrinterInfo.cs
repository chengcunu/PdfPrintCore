namespace PdfPrintCore;

public class PrinterInfo
{
    public PrinterStatus Status { get; set; }
    public string? StateMessage { get; set; }
    public string? Location { get; set; }
    public int JobCountSinceLastReset { get; set; }
}

public enum PrinterStatus
{
    Ready = 0,
    Printing = 1,
    Stopped = 2,
    Unknow = 3
}