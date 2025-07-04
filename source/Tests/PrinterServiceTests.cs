using PdfPrintCore;

namespace Tests
{
    [TestClass]
    public sealed class PrinterServiceTests
    {
        private readonly Printer _printer;

        public PrinterServiceTests()
        {
            _printer = TestSettings.GetTestPrinter();
        }

        /// <summary>
        /// Test <see cref="PrinterService.ListInstalledPrinters"/>
        /// </summary>
        [TestMethod]
        public void ListInstalledPrinters_ShouldBeSuccessfully()
        {
            var printerNames = PrinterService.ListInstalledPrinters();
            if (_printer is not null)
            {
                var name = printerNames.FirstOrDefault(x => x == _printer.Name);
                Assert.IsNotNull(name);
            }
        }

        /// <summary>
        /// Test <see cref="PrinterService.ListPrinters"/>
        /// </summary>
        [TestMethod]
        public void ListPrinters_ShouldBeSuccessfully()
        {
            var printers = PrinterService.ListPrinters();
            if (_printer is not null)
            {
                var name = printers.FirstOrDefault(x => x.Name == _printer.Name);
                Assert.IsNotNull(name);
            }
        }

        /// <summary>
        /// Test <see cref="PrinterService.GetPrinter(string)"/>
        /// </summary>
        [TestMethod]
        public void GetPrinter_ShouldBeSuccessfully()
        {
            if (_printer is not null)
            {
                var prn = PrinterService.GetPrinter(_printer.Name);
                Assert.IsNotNull(prn);
                Assert.AreNotEqual(PrinterStatus.UNKNOW, prn.Status);
                Assert.AreEqual(_printer.Name, prn.Name);
            }
        }

        [TestMethod]
        public void GetPrinter_WhenPrinterNotExist_ShouldBeUnknowStatus()
        {
            var prn = PrinterService.GetPrinter("/printer");
            Assert.IsNotNull(prn);
            Assert.AreEqual(PrinterStatus.UNKNOW, prn.Status);
        }
    }
}
