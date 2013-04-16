using System.Text;
using System.IO;

namespace GG4NET
{
    /// <summary>
    /// Utility class for writing complex packets
    /// </summary>
    internal class PacketWriter : BinaryWriter
    {
        #region Properties
        private MemoryStream _memoryStream;

        /// <summary>
        /// Written data
        /// </summary>
        public byte[] Data
        {
            get { return _memoryStream.ToArray(); }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Construct packet writer using UTF8 encoding for writing
        /// </summary>
        public PacketWriter()
            : this(Encoding.UTF8)
        {
        }
        /// <summary>
        /// Construct packet writer using specified encoding for writing
        /// </summary>
        /// <param name="encoding">Encoding used to write</param>
        public PacketWriter(Encoding encoding)
            : this(encoding, new MemoryStream())
        {
        }
        private PacketWriter(Encoding encoding, MemoryStream memoryStream)
            : base(memoryStream, encoding)
        {
            _memoryStream = memoryStream;
        }
        #endregion
    }
}
