using PdfPrintCore;
using System.Diagnostics;
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

        /// <summary>
        /// Test native methods <see cref="NativeMethods.PRN_ListPrinters"/>
        /// </summary>
        [TestMethod]
        public void PRN_ListPrinters_ShouldBeSuccessfully()
        {
            nint ptr = NativeMethods.PRN_ListPrinters();
            Assert.AreNotEqual(ptr, nint.Zero);
            Marshal.FreeHGlobal(ptr);
        }

        /// <summary>
        /// Test native methods <see cref="NativeMethods.PRN_FreePrinterInfo(nint)"/>
        /// </summary>
        [TestMethod]
        public void PRN_FreePrinterInfo_ShouldBeSuccessfully()
        {
            int loop = 100000;
            long memSnapshots = 0;

            var lib = NativeMethods.LoadNativeMethods();
            NativeLibrary.Free(lib);

            long releasedIncreased = 0;
            using (var process = Process.GetCurrentProcess())
            {
                process.Refresh();
                memSnapshots = process.WorkingSet64;
                for (int i = 0; i<loop; i++)
                {
                    nint ptr = NativeMethods.PRN_GetPrinterInfo(nint.Zero);
                    NativeMethods.PRN_FreePrinterInfo(ptr);
                }
                process.Refresh();
                releasedIncreased = process.WorkingSet64 - memSnapshots;
            }
        }

        /// <summary>
        /// Test native methods <see cref="NativeMethods.PRN_GetLastError"/>
        /// </summary>
        [TestMethod]
        public void PRN_GetLastError_ShouldBeSuccessfully()
        {
            int result = NativeMethods.PRN_Print(nint.Zero, 0, nint.Zero, nint.Zero, new NativeMethods.PRINT_OPTIONS());
            Assert.IsTrue(result != 0);

            nint error = NativeMethods.PRN_GetLastError();
            Assert.AreNotEqual(error, nint.Zero);

            string errorMessage = Marshal.PtrToStringUni(error);
            Assert.IsTrue(errorMessage.Length > 0);

            nint error2 = NativeMethods.PRN_GetLastError();
            Assert.AreEqual(error2, nint.Zero);

            NativeMethods.PRN_FreeLastError(error);
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
