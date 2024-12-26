using PdfPrintCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
    public static List<string> ListPrinters()
    {
        nint ptr = NativeMethods.PRN_ListPrinters();

        try
        {
            string result = Marshal.PtrToStringUni(ptr)!;
            if (result.StartsWith('/'))
                throw new NativeMethodException(result);

            string[] printers = result.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return [.. printers];
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    /// <summary>
    /// Gets the printer info by name.
    /// </summary>
    /// <param name="printer">Printer name</param>
    /// <returns><see cref="PrinterInfo"/></returns>
    public static PrinterInfo GetPrinterInfo(string printer)
    {
        if (printer is null)
            return new PrinterInfo() { Status = PrinterStatus.Unknow };

        nint ptrInfo = IntPtr.Zero;
        nint ptrPrinter = Marshal.StringToHGlobalUni(printer);

        try
        {
            ptrInfo = NativeMethods.PRN_GetPrinterInfo(ptrPrinter);
            var info = Marshal.PtrToStructure<NativeMethods.PRINTER_INFO>(ptrInfo);
            return new PrinterInfo()
            {
                JobCountSinceLastReset = info.JobCountSinceLastReset,
                Location = info.Location,
                Status = (PrinterStatus)info.Status,
                StateMessage = info.StateMessage
            };
        }
        finally
        {
            Marshal.DestroyStructure<NativeMethods.PRINTER_INFO>(ptrInfo);
            Marshal.FreeHGlobal(ptrInfo);
            Marshal.FreeHGlobal(ptrPrinter);
        }
    }
}
