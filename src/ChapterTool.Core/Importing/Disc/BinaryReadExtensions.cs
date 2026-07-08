using System.Text;

namespace ChapterTool.Core.Importing.Disc;

internal static class BinaryReadExtensions
{
    extension(Stream stream)
    {
        /// <summary>
        /// Executes the ReadExactBytes operation.
        /// </summary>
        /// <param name="length">The span length.</param>
        /// <returns>The operation result.</returns>
        public byte[] ReadExactBytes(int length)
        {
            var bytes = new byte[length];
            var read = stream.Read(bytes, 0, length);
            if (read != length)
            {
                throw new EndOfStreamException();
            }

            return bytes;
        }

        /// <summary>
        /// Executes the ReadAscii operation.
        /// </summary>
        /// <param name="length">The span length.</param>
        /// <returns>The operation result.</returns>
        public string ReadAscii(int length) =>
            Encoding.ASCII.GetString(stream.ReadExactBytes(length));

        /// <summary>
        /// Executes the SkipBytes operation.
        /// </summary>
        /// <param name="length">The span length.</param>
        /// <returns>The operation result.</returns>
        public void SkipBytes(long length)
        {
            stream.Seek(length, SeekOrigin.Current);
            if (stream.Position > stream.Length)
            {
                throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// Executes the ReadUInt32BigEndian operation.
        /// </summary>
        /// <returns>The operation result.</returns>
        public uint ReadUInt32BigEndian()
        {
            var b = stream.ReadExactBytes(4);
            return b[3] + ((uint)b[2] << 8) + ((uint)b[1] << 16) + ((uint)b[0] << 24);
        }

        /// <summary>
        /// Executes the ReadUInt16BigEndian operation.
        /// </summary>
        /// <returns>The operation result.</returns>
        public ushort ReadUInt16BigEndian()
        {
            var b = stream.ReadExactBytes(2);
            return (ushort)(b[1] + (b[0] << 8));
        }
    }
}
