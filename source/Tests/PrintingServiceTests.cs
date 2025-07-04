using PdfPrintCore;
using PdfPrintCore.Exceptions;
using System.Runtime.InteropServices;

namespace Tests
{
    [TestClass]
    public class PrintingServiceTests
    {
        private readonly Printer _printer;
        private readonly string _testDataFilePath;

        public PrintingServiceTests()
        {
            _printer = TestSettings.GetTestPrinter();
            _testDataFilePath = Path.Combine(AppContext.BaseDirectory, "TestData");
        }

        /// <summary>
        /// Test whether the default values of <see cref="PrintingService.ConvertPrintOptions(ref nint)"/> method are correct
        /// </summary>
        [TestMethod]
        public void ConvertPrintOptions_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(_testDataFilePath, "Sample.pdf");
            using PdfDocument document = new(filename);
            PrintingService service = new(document);

            nint pCancelled = nint.Zero;
            var options = service.ConvertPrintOptions(ref pCancelled);
            Assert.AreEqual(NativeMethods.RenderFlag.NONE, options.Flags & NativeMethods.RenderFlag.RENDER_FORMFILL);
            Assert.AreEqual("document", options.JobName);
            Assert.AreEqual(NativeMethods.RenderFlag.FIT_TO_PAPER, options.Flags & NativeMethods.RenderFlag.FIT_TO_PAPER);
            Assert.IsTrue(options.Copies <2);
            Assert.AreEqual((int)ColorModel.DEFAULT, options.ColorModel);
            Assert.AreEqual(0, options.Blackthreshole);
            Assert.AreEqual(nint.Zero, options.CancelledPtr);
            Assert.IsNull(options.DrawingPageCallback);
        }

        /// <summary>
        /// Test whether the value of <see cref="PrintingService.ConvertPrintOptions(ref nint)"/>  method is correct
        /// </summary>
        [TestMethod]
        public void ConvertPrintOptions2_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(_testDataFilePath, "Sample.pdf");
            using PdfDocument document = new(filename, "password");
            PrintingService service = new(document);

            service.PrintOptions.RenderFormFill = true;
            service.PrintOptions.JobName = "Job1";
            service.PrintOptions.FitToPaper = false;
            service.PrintOptions.Copies = 2;
            service.PrintOptions.ColorModel = ColorModel.GRAY_SCALE;
            service.PrintOptions.Blackthreshole = 100;
            service.OnDrawingPage +=(args) => { };

            nint pCancelled = Marshal.AllocHGlobal(sizeof(int));
            try
            {
                Marshal.WriteInt32(pCancelled, 1);

                var options = service.ConvertPrintOptions(ref pCancelled);
                Assert.AreEqual(NativeMethods.RenderFlag.RENDER_FORMFILL, options.Flags & NativeMethods.RenderFlag.RENDER_FORMFILL);
                Assert.AreEqual(service.PrintOptions.JobName, options.JobName);
                Assert.AreEqual(NativeMethods.RenderFlag.NONE, options.Flags & NativeMethods.RenderFlag.FIT_TO_PAPER);
                Assert.AreEqual(2, options.Copies);
                Assert.AreEqual((int)ColorModel.GRAY_SCALE, options.ColorModel);
                Assert.AreEqual(100, options.Blackthreshole);
                Assert.IsNotNull(options.DrawingPageCallback);
                Assert.AreEqual(pCancelled, options.CancelledPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(pCancelled);
                pCancelled = nint.Zero;
            }
        }

        /// <summary>
        /// Test <see cref="PrintingService.Print(string?)"/> method.
        /// </summary>
        [TestMethod]
        public void Print_WhenPrinterNotExist_ShouldBeFailed()
        {
            string filename = Path.Combine(_testDataFilePath, "Sample.pdf");
            using PdfDocument document = new(filename);
            PrintingService service = new(document);
            Assert.ThrowsException<NativeMethodException>(() =>
            {
                service.Print("/printer");
            });
        }

        /// <summary>
        /// Test <see cref="PrintingService.Print(string?)"/> method.
        /// </summary>
        [TestMethod]
        public void Print_Pdf_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(_testDataFilePath, "Sample.pdf");
            if (_printer is not null)
            {
                using PdfDocument document = new(filename);
                PrintingService service = new(document);
                service.PrintOptions.PrintToFile = true;
                service.Print(_printer.Name);
            }
        }

        /// <summary>
        /// Test Encrypted PDF printing with correct password. <see cref="PrintingService.Print(string?)"/>
        /// </summary>
        [TestMethod]
        public void Print_PdfWithPassword_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(_testDataFilePath, "Sample-protected.pdf");
            if (_printer is not null)
            {
                using PdfDocument document = new(filename, "2345.~K*954");
                PrintingService service = new(document);
                service.PrintOptions.PrintToFile = true;
                service.Print(_printer.Name);
            }
        }

        /// <summary>
        /// Test PDF printing with error password. <see cref="PrintingService.Print(string?)"/>
        /// </summary>
        [TestMethod]
        public void Print_PdfWithoutPassword_ShouldBeFailed()
        {
            string filename = Path.Combine(_testDataFilePath, "Sample-protected.pdf");
            if (_printer is not null)
            {
                using PdfDocument document = new(filename);
                PrintingService service = new(document);
                Assert.ThrowsException<NativeMethodException>(() =>
                {
                    service.Print(_printer.Name);
                });
            }
        }

        /// <summary>
        /// Test <see cref="PrintingService.PrintAsync(string?, CancellationToken)"/> method.
        /// </summary>
        [TestMethod]
        public void PrintAsync_Pdf_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(_testDataFilePath, "pdf-lib_page_copying_example.pdf");
            if (_printer is not null)
            {
                int beforeJobCount = GetJobCount();

                using PdfDocument document = new(filename);
                var service = new PrintingService(document);
                service.OnDrawingPage += (args) =>
                {
                    int afterJobCount = GetJobCount();
                    Assert.IsTrue(beforeJobCount < afterJobCount);
                    Console.WriteLine($"{args.CurrentPageIndex} / {args.TotalPages}");
                };
                service.PrintOptions.ColorModel = ColorModel.GRAY_SCALE;
                service.PrintOptions.RenderFormFill = true;
                service.PrintOptions.FitToPaper = false;
                service.PrintOptions.JobName = "test";
                service.PrintOptions.Copies = 1;
                service.PrintOptions.Blackthreshole = 100;
                service.PrintOptions.PrintToFile = true;

                service.PrintAsync(_printer.Name).GetAwaiter().GetResult();
            }
        }

        private int GetJobCount()
        {
            var info = PrinterService.GetPrinter(_printer.Name);
            return info.JobCountSinceLastReset;
        }
    }
}
