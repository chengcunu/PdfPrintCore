using PdfPrintCore;
using PdfPrintCore.Exceptions;
using System.Runtime.InteropServices;

namespace Tests
{
    [TestClass]
    public class PrintingServiceTests
    {
        private readonly Printer _testPrinter;
        private readonly string _filePath;

        public PrintingServiceTests()
        {
            _testPrinter = TestSettings.GetTestPrinter();
            _filePath = Path.Combine(AppContext.BaseDirectory, "TestData");
        }

        /// <summary>
        /// Test whether the default values of <see cref="PrintingService.GetPrintOptions(ref nint)"/> method are correct
        /// </summary>
        [TestMethod]
        public void GetPrintOptions_DefaultValue_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(_filePath, "Sample.pdf");
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

        /// <summary>
        /// Test whether the value of <see cref="PrintingService.GetPrintOptions(ref nint)"/>  method is correct
        /// </summary>
        [TestMethod]
        public void GetPrintOptions_SetValue_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(_filePath, "Sample.pdf");
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

        /// <summary>
        /// Test <see cref="PrintingService.Print(string?)"/> method.
        /// </summary>
        [TestMethod]
        public void Print_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(_filePath, "Sample.pdf");
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

        /// <summary>
        /// Test PDF printing with correct password. <see cref="PrintingService.Print(string?)"/>
        /// </summary>
        [TestMethod]
        public void Print_CorrectPassword_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(_filePath, "Sample-protected.pdf");
            if (_testPrinter is not null)
            {
                int jobCount = GetJobCount();

                using PdfDocument document = new(filename, "2345.~K*954");
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

        /// <summary>
        /// Test PDF printing with error password. <see cref="PrintingService.Print(string?)"/>
        /// </summary>
        [TestMethod]
        public void Print_ErrorPassword_ShouldBeFailed()
        {
            string filename = Path.Combine(_filePath, "Sample-protected.pdf");
            if (_testPrinter is not null)
            {
                using PdfDocument document = new(filename);
                PrintingService service = new(document);
                Assert.ThrowsException<NativeMethodException>(() =>
                {
                    service.Print(_testPrinter.Name);
                });
            }
            else
            {
                Assert.Inconclusive("No specified test printer.");
            }
        }

        /// <summary>
        /// Test <see cref="PrintingService.PrintAsync(string?, CancellationToken)"/> method.
        /// </summary>
        [TestMethod]
        public void PrintAsync_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(_filePath, "pdf-lib_form_creation_example.pdf");
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
