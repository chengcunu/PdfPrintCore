[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace Tests
{
    using PdfPrintCore;

    public class TestSettings
    {
        /// <summary>
        /// Get the printer for testing purposes
        /// </summary>
        /// <returns></returns>
        public static Printer GetTestPrinter()
        {
            if (PlatformConfiguration.IsWindows)
                return new Printer("Microsoft Print to PDF");
            else if (PlatformConfiguration.IsLinux)
                return new Printer("PDF");
            else if (PlatformConfiguration.IsOsx)
                return new Printer("InvalidPrinter") { IsInvalid = true };
            else
                return null;
        }
    }

    public class Printer(string name)
    {
        public string Name { get; set; } = name;
        public bool IsInvalid { get; set; }
    }
}