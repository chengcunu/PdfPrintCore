using PdfPrintCore;
using System.Runtime.InteropServices;

namespace Tests
{
    [TestClass]
    public class PrintingServiceTests
    {
        private readonly Printer _testPrinter;

        public PrintingServiceTests()
        {
            _testPrinter = TestSettings.GetTestPrinter();
        }

        [TestMethod]
        public void GetPrintOptions_DefaultValue_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(AppContext.BaseDirectory, "Assets", "Sample.pdf");
            using PdfDocument document = new(filename);
            PrintingService service = new(document);

            nint cancellPtr = nint.Zero;
            var options = service.GetPrintOptions(ref cancellPtr);
            Assert.AreEqual(NativeMethods.RenderFlag.None, options.Flags & NativeMethods.RenderFlag.RenderFormFill);
            Assert.AreEqual("document", options.JobName);
            Assert.AreEqual(NativeMethods.RenderFlag.FitToPaper, options.Flags & NativeMethods.RenderFlag.FitToPaper);
            Assert.IsTrue(options.Copies <2);
            Assert.AreEqual((int)ColorModel.Default, options.ColorModel);
            Assert.AreEqual(0, options.Blackthreshole);
            Assert.AreEqual(nint.Zero, options.CancelledPtr);
            Assert.IsNull(options.DrawingPageCallback);
        }

        [TestMethod]
        public void GetPrintOptions_SetValue_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(AppContext.BaseDirectory, "Assets", "Sample.pdf");
            using PdfDocument document = new(filename, "password");
            PrintingService service = new(document);

            service.PrinterSettings.RenderFormFill = true;
            service.PrinterSettings.JobName = "Job1";
            service.PrinterSettings.FitToPaper = false;
            service.PrinterSettings.Copies = 2;
            service.PrinterSettings.ColorModel = ColorModel.GrayScale;
            service.PrinterSettings.Blackthreshole = 100;
            service.OnDrawingPage +=(args) => { };

            nint cancellPtr = Marshal.AllocHGlobal(sizeof(int));
            try
            {
                Marshal.WriteInt32(cancellPtr, 1);

                var options = service.GetPrintOptions(ref cancellPtr);
                Assert.AreEqual(NativeMethods.RenderFlag.RenderFormFill, options.Flags & NativeMethods.RenderFlag.RenderFormFill);
                Assert.AreEqual(service.PrinterSettings.JobName, options.JobName);
                Assert.AreEqual(NativeMethods.RenderFlag.None, options.Flags & NativeMethods.RenderFlag.FitToPaper);
                Assert.AreEqual(2, options.Copies);
                Assert.AreEqual((int)ColorModel.GrayScale, options.ColorModel);
                Assert.AreEqual(100, options.Blackthreshole);
                Assert.IsNotNull(options.DrawingPageCallback);
                Assert.AreEqual(cancellPtr, options.CancelledPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(cancellPtr);
                cancellPtr = nint.Zero;
            }
        }

        [TestMethod]
        public void Print_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(AppContext.BaseDirectory, "Assets", "Sample.pdf");
            if (_testPrinter is not null)
            {
                int jobCount = GetJobCount();

                using PdfDocument document = new(filename);
                PrintingService service = new(document);
                service.Print(_testPrinter.Name);

                int afterJobCount = GetJobCount();
                if (_testPrinter.IsInvalid)
                    Assert.AreEqual(jobCount + 1, afterJobCount);
                else
                    Assert.AreEqual(jobCount, afterJobCount);
            }
            else
            {
                Assert.Inconclusive("No specified test printer.");
            }
        }

        [TestMethod]
        public void PrintAsync_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(AppContext.BaseDirectory, "Assets", "pdf-lib_form_creation_example.pdf");
            if (_testPrinter is not null)
            {
                int jobCount = GetJobCount();

                using PdfDocument document = new(filename);
                var service = new PrintingService(document);
                service.OnDrawingPage +=static (args) =>
                {
                    Console.WriteLine($"{args.CurrentPageIndex} / {args.TotalPages}");
                };
                service.PrinterSettings.ColorModel = ColorModel.GrayScale;
                service.PrinterSettings.RenderFormFill = true;
                service.PrinterSettings.FitToPaper = false;
                service.PrinterSettings.JobName = "test";
                service.PrinterSettings.Copies = 1;
                service.PrinterSettings.Blackthreshole = 100;

                service.PrintAsync(_testPrinter.Name).GetAwaiter().GetResult();

                int afterJobCount = GetJobCount();
                if (_testPrinter.IsInvalid)
                    Assert.AreEqual(jobCount + 1, afterJobCount);
                else
                    Assert.AreEqual(jobCount, afterJobCount);
            }
        }

        private int GetJobCount()
        {
            if (_testPrinter is not null)
            {
                var info = PrinterService.GetPrinterInfo(_testPrinter.Name);
                return info.JobCountSinceLastReset;
            }
            return 0;
        }
    }
}
