using System;

namespace PdfPrintCore.Exceptions
{
    public class NativeMethodException : Exception
    {
        public NativeMethodException()
        {
        }

        public NativeMethodException(string message)
            : base(message)
        {
        }

        public NativeMethodException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
