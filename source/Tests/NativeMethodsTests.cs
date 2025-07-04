using PdfPrintCore;
using System.Runtime.InteropServices;

namespace Tests
{
    [TestClass]
    public class NativeMethodsTests
    {
        /// <summary>
        /// Test native library resolver <see cref="NativeMethods.LoadNativeMethods"/>
        /// </summary>
        [TestMethod]
        public void LoadNativeMethods_ShouldBeSuccessfully()
        {
            var lib = NativeMethods.LoadNativeMethods();
            Assert.AreNotEqual(nint.Zero, lib);
            NativeLibrary.Free(lib);
        }
    }
}
