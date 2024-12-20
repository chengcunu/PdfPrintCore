using PdfPrintCore;

namespace Tests
{
    [TestClass]
    public class PdfDocumentTests
    {
        [TestMethod]
        public void PdfDocument_Structure_ShouldBeSuccessfully()
        {
            string filename = Path.Combine(AppContext.BaseDirectory, "Assets", "Sample.pdf");
            byte[] fileData;
            using (FileStream fs = new(filename, FileMode.Open, FileAccess.Read))
            {
                fileData = new byte[fs.Length];
                fs.ReadExactly(fileData);
            }
            int dataLength = fileData.Length;

            using (PdfDocument document = new(filename))
            {
                Assert.IsNull(document.Password);
                Assert.AreEqual(dataLength, document.Data.Length);
            }

            using (PdfDocument document = new(filename, "passoword"))
            {
                Assert.AreEqual("passoword", document.Password);
                Assert.AreEqual(dataLength, document.Data.Length);
            }

            using(FileStream fs = new(filename, FileMode.Open, FileAccess.Read))
            {
                using PdfDocument document = new(fs, "passoword1");
                Assert.AreEqual("passoword1", document.Password);
                Assert.AreEqual(dataLength, document.Data.Length);
            }

            using (PdfDocument document = new(fileData, "passoword2"))
            {
                Assert.AreEqual("passoword2", document.Password);
                Assert.AreEqual(dataLength, document.Data.Length);
            }
        }
    }
}
