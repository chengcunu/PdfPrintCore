namespace PdfPrintCore;

public class Printer
{
    public string? Name { get; set; }
    public bool IsDefault { get; set; }
    public PrinterStatus Status { get; set; }
    public string? StateMessage { get; set; }
    public string? Location { get; set; }
    public int JobCountSinceLastReset { get; set; }
}

public enum PrinterStatus
{
    READY = 0,
    PRINTING = 1,
    STOPED = 2,
    UNKNOW = 3,
    OFFLINE = 4,
}