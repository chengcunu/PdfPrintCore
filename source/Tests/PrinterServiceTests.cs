﻿using PdfPrintCore;

namespace Tests
{
    [TestClass]
    public sealed class PrinterServiceTests
    {
        private readonly Printer _testPrinter;

        public PrinterServiceTests()
        {
            _testPrinter = TestSettings.GetTestPrinter();
        }

        [TestMethod]
        public void ListPrinters_ShouldBeSuccessfully()
        {
            var printers = PrinterService.ListPrinters();
            if (_testPrinter is not null)
            {
                var p = printers.FirstOrDefault(x => x == _testPrinter.Name);
                Assert.IsNotNull(p);
            }
            else
            {
                Assert.Inconclusive("No specified test printer.");
            }
        }

        [TestMethod]
        public void GetPrinterInfo_ShouldBeSuccessfully()
        {
            if (_testPrinter is not null)
            {
                var info = PrinterService.GetPrinterInfo(_testPrinter.Name);
                Assert.IsNotNull(info);
                Assert.AreNotEqual(PrinterStatus.Unknow, info.Status);
            }
            else
            {
                Assert.Inconclusive("No specified test printer.");
            }
        }
    }
}