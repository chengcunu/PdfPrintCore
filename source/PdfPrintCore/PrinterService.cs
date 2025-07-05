using System.Collections.Generic;
using System.Linq;
using static PdfPrintCore.NativeMethods;

namespace PdfPrintCore;

/// <summary>
/// Information about printer.
/// </summary>
public class PrinterService
{
    /// <summary>
    /// Gets the names of all printers installed on the machine.
    /// </summary>
    /// <returns></returns>
    public static List<string> ListInstalledPrinters()
    {
        var result = NativeMethods.ListInstalledPrinters();
        return [.. result];
    }

    /// <summary>
    /// Gets all printers installed on the machine.
    /// </summary>
    /// <returns></returns>
    public static List<Printer> ListPrinters()
    {
        var result = NativeMethods.ListPrinters();
        return result.Select(x => ConvertPrinter(x)).ToList();
    }

    /// <summary>
    /// Gets the printer info by name.
    /// </summary>
    /// <param name="printer">Printer name</param>
    /// <returns><see cref="Printer"/></returns>
    public static Printer GetPrinter(string printer)
    {
        if (printer is null)
            return new Printer() { Status = PrinterStatus.UNKNOW };

        var result = NativeMethods.GetPrinter(printer);
        return ConvertPrinter(result);
    }

    private static Printer ConvertPrinter(PRN_PRINTER printer)
    {
        return new Printer()
        {
            Name = printer.Name,
            IsDefault = printer.IsDefault,
            JobCountSinceLastReset = printer.JobCountSinceLastReset,
            Location = printer.Location,
            Comment = printer.Comment,
            Status = (PrinterStatus)printer.Status,
            StateMessage = printer.StateMessage,
            DriverName = printer.DriverName,
            IsShared = printer.IsShared,
        };
    }
}
