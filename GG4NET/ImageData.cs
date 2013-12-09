using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GG4NET
{
    /// <summary>
    /// Dane obraza.
    /// </summary>
    public struct ImageData
    {
        /// <summary>
        /// Suma kontrolna CRC32.
        /// </summary>
        public uint Crc32;
        /// <summary>
        /// Wielkość obrazka w bajtach.
        /// </summary>
        public uint Length;
        /// <summary>
        /// Nazwa pliku obrazka.
        /// </summary>
        public string FileName;
        /// <summary>
        /// Dane obrazka.
        /// </summary>
        public byte[] Data;
    }
}
