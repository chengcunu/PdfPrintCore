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

            bool loaded = TryLoadLib(out nint _lib);
            NativeLibrary.Free(_lib);

            Assert.IsTrue(loaded);
            Assert.AreEqual(lib, _lib);
        }

        private static bool TryLoadLib(out nint lib)
        {
            string architecuter = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.Arm64 => "arm64",
                _ => throw new PlatformNotSupportedException()
            };

            string libPath = Path.Combine(AppContext.BaseDirectory, "runtimes");
            if (PlatformConfiguration.IsWindows)
                libPath = Path.Combine(libPath, $"win-{architecuter}", "native", "printcore.dll");
            else if (PlatformConfiguration.IsLinux)
                if (PlatformConfiguration.IsGlibc)
                    libPath = Path.Combine(libPath, $"linux-{architecuter}", "native", "libprintcore.so");
                else
                    libPath = Path.Combine(libPath, $"linux-{PlatformConfiguration.LinuxFlavor}-{architecuter}", "native", "libprintcore.so");
            else if (PlatformConfiguration.IsOsx)
                libPath = Path.Combine(libPath, $"osx-{architecuter}", "native", "libprintcore.dylib");
            else
                throw new NotSupportedException();

            return NativeLibrary.TryLoad(libPath, out lib);
        }
    }
}
