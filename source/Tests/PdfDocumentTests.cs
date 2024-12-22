using PdfPrintCore;

namespace Tests
{
    [TestClass]
    public class PdfDocumentTests
    {
        private readonly string _filename;
        private readonly int _fileLength;

        public PdfDocumentTests()
        {
            _filename = Path.Combine(AppContext.BaseDirectory, "Assets", "Sample.pdf");
            using FileStream fs = new(_filename, FileMode.Open, FileAccess.Read);
            _fileLength = (int)fs.Length;
        }

        /// <summary>
        /// Test the constructor of <see cref="PdfDocument(string, string?)"/> class.
        /// </summary>
        [TestMethod]
        public void PdfDocument_Filenameconstructor_ShouldBeSuccessfully()
        {
            using PdfDocument document = new(_filename, "passoword");
            Assert.AreEqual("passoword", document.Password);
            Assert.AreEqual(_fileLength, document.Data.Length);
        }

        /// <summary>
        /// Test the constructor of <see cref="PdfDocument(Stream, string?)"/> class.
        /// </summary>
        [TestMethod]
        public void PdfDocument_Streamconstructor_ShouldBeSuccessfully()
        {
            using FileStream fs = new(_filename, FileMode.Open, FileAccess.Read);
            using PdfDocument document = new(fs, "passoword1");
            Assert.AreEqual("passoword1", document.Password);
            Assert.AreEqual(_fileLength, document.Data.Length);
        }

        /// <summary>
        /// Test the constructor of <see cref="PdfDocument(byte[], string?)"/> class.
        /// </summary>
        [TestMethod]
        public void PdfDocument_Bufferconstructor_ShouldBeSuccessfully()
        {
            byte[] fileData;
            using (FileStream fs = new(_filename, FileMode.Open, FileAccess.Read))
            {
                fileData = new byte[fs.Length];
                fs.ReadExactly(fileData);
            }

            using PdfDocument document = new(fileData, "passoword2");
            Assert.AreEqual("passoword2", document.Password);
            Assert.AreEqual(_fileLength, document.Data.Length);
        }
    }
}
