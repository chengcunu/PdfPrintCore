using System.Buffers;
using System.IO.Pipelines;
using System.Security.Cryptography;

namespace PdfPrintCore.API.Demo
{
    public static class Base64
    {
        /// <summary>
        /// Convert base64 string to bytes array from stream
        /// </summary>
        /// <param name="bodyReader"></param>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static async Task<byte[]> ConvertFromBase64StreamAsync(PipeReader bodyReader, int blockSize = 4096)
        {
            byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(blockSize);
            try
            {
                using MemoryStream outputStream = new();
                using FromBase64Transform transform = new(FromBase64TransformMode.IgnoreWhiteSpaces);

                byte[] transformBuffer = new byte[blockSize];
                while (true)
                {
                    ReadResult readResult = await bodyReader.ReadAsync();
                    var buffer = readResult.Buffer;

                    while (buffer.Length > blockSize)
                    {
                        buffer.Slice(0, blockSize).CopyTo(sharedBuffer.AsSpan());
                        int bytesWritten = transform.TransformBlock(sharedBuffer, 0, blockSize, transformBuffer, 0);
                        outputStream.Write(transformBuffer, 0, bytesWritten);

                        buffer = buffer.Slice(blockSize);
                    }

                    if (readResult.IsCompleted && buffer.Length > 0)
                    {
                        var remainBuffer = transform.TransformFinalBlock(buffer.ToArray(), 0, (int)buffer.Length);
                        outputStream.Write(remainBuffer, 0, remainBuffer.Length);
                    }

                    bodyReader.AdvanceTo(buffer.Start, buffer.End);
                    if (readResult.IsCompleted)
                    {
                        break;
                    }
                }

                transform.Clear();

                return outputStream.ToArray();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sharedBuffer);
            }
        }
    }
}
