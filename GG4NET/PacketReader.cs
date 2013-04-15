using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GG4NET
{
    /// <summary>
    /// Utility class for reading complex packets
    /// </summary>
    internal class PacketReader : BinaryReader
    {
        #region Constructors
        /// <summary>
        /// Construct packet reader with specified buffer to read, UTF8 encoding and start offset 0.
        /// </summary>
        /// <param name="data">Data to read</param>
        public PacketReader(byte[] data)
            : this(data, Encoding.UTF8, 0)
        {
        }
        /// <summary>
        /// Construct packet reader with specified buffer to read, encoding and start offset 0
        /// </summary>
        /// <param name="data">Data to read</param>
        /// <param name="encoding">Encoding used to read</param>
        public PacketReader(byte[] data, Encoding encoding)
            : this(data, encoding, 0)
        {
        }
        /// <summary>
        /// Construct packet reader with specified buffer to read, encoding and start offset
        /// </summary>
        /// <param name="buffer">Buffer which contains data</param>
        /// <param name="encoding">Encoding used to read</param>
        /// <param name="startOffset">Start buffer offset</param>
        public PacketReader(byte[] buffer, Encoding encoding, long startOffset)
            : base(new MemoryStream(buffer), encoding)
        {
            if (startOffset > 0) this.BaseStream.Position = startOffset;
        }
        #endregion
    }
}
