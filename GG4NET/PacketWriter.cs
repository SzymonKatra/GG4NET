using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GG4NET
{
    /// <summary>
    /// Class to write data packets
    /// </summary>
    internal class PacketWriter : IDisposable
    {
        private bool _disposed = false;

        /// <summary>
        /// Buffer to write data
        /// </summary>
        public byte[] Data
        {
            get
            {
                if (BaseStream == null) return new byte[0]; else return BaseStream.ToArray();
            }
        }
        /// <summary>
        /// Base Stream
        /// </summary>
        public MemoryStream BaseStream { get; private set; }
        /// <summary>
        /// Base BinaryWriter
        /// </summary>
        public BinaryWriter BaseWriter { get; private set; }

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public PacketWriter()
            : this(Encoding.UTF8)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public PacketWriter(Encoding encoding)
        {
            BaseStream = new MemoryStream();
            BaseWriter = new BinaryWriter(BaseStream, encoding);
        }
        #endregion

        #region WriteMethods
        /// <summary>
        /// Writes boolean
        /// </summary>
        /// <param name="value">Boolean to write</param>
        public void Write(bool value)
        {
            BaseWriter.Write(value);
        }
        /// <summary>
        /// Writes single byte
        /// </summary>
        /// <param name="value">Byte to write</param>
        public void Write(byte value)
        {
            BaseWriter.Write(value);
        }
        /// <summary>
        /// Writes bytes
        /// </summary>
        /// <param name="buffer">Bytes to write</param>
        public void Write(byte[] buffer)
        {
            BaseWriter.Write(buffer);
        }
        /// <summary>
        /// Writes single character
        /// </summary>
        /// <param name="ch">Character to write</param>
        public void Write(char ch)
        {
            BaseWriter.Write(ch);
        }
        /// <summary>
        /// Writes characters
        /// </summary>
        /// <param name="chars">Characters to write</param>
        public void Write(char[] chars)
        {
            BaseWriter.Write(chars);
        }
        /// <summary>
        /// Writes decimal
        /// </summary>
        /// <param name="value">Decimal to write</param>
        public void Write(decimal value)
        {
            BaseWriter.Write(value);
        }
        /// <summary>
        /// Writes double
        /// </summary>
        /// <param name="value">Double to write</param>
        public void Write(double value)
        {
            BaseWriter.Write(value);
        }
        /// <summary>
        /// Writes float
        /// </summary>
        /// <param name="value">Float to write</param>
        public void Write(float value)
        {
            BaseWriter.Write(value);
        }
        /// <summary>
        /// Writes int
        /// </summary>
        /// <param name="value">Int to write</param>
        public void Write(int value)
        {
            BaseWriter.Write(value);
        }
        /// <summary>
        /// Writes long
        /// </summary>
        /// <param name="value">Long to write</param>
        public void Write(long value)
        {
            BaseWriter.Write(value);
        }
        /// <summary>
        /// Writes sbyte
        /// </summary>
        /// <param name="value">SByte to write</param>
        public void Write(sbyte value)
        {
            BaseWriter.Write(value);
        }
        /// <summary>
        /// Writes short
        /// </summary>
        /// <param name="value">Short to write</param>
        public void Write(short value)
        {
            BaseWriter.Write(value);
        }
        /// <summary>
        /// Writes string
        /// </summary>
        /// <param name="value">String to write</param>
        public void Write(string value)
        {
            BaseWriter.Write(value);
        }
        /// <summary>
        /// Writes unsigned int
        /// </summary>
        /// <param name="value">Unsigned int to write</param>
        public void Write(uint value)
        {
            BaseWriter.Write(value);
        }
        /// <summary>
        /// Writes unsigned long
        /// </summary>
        /// <param name="value">Unsigned long to write</param>
        public void Write(ulong value)
        {
            BaseWriter.Write(value);
        }
        /// <summary>
        /// Writes unsigned short
        /// </summary>
        /// <param name="value">Unsigned short to write</param>
        public void Write(ushort value)
        {
            BaseWriter.Write(value);
        }
        /// <summary>
        /// Writes the specified bytes from specified buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="index">Start position</param>
        /// <param name="count">Bytes count</param>
        public void Write(byte[] buffer, int index, int count)
        {
            BaseWriter.Write(buffer, index, count);
        }
        /// <summary>
        /// Writes the specified characters from specified buffer
        /// </summary>
        /// <param name="chars">Buffer</param>
        /// <param name="index">Start position</param>
        /// <param name="count">Characters count</param>
        public void Write(char[] chars, int index, int count)
        {
            BaseWriter.Write(chars, index, count);
        }
        #endregion

        /// <summary>
        /// Closes the writer
        /// </summary>
        public void Close()
        {
            BaseWriter.Close();
        }

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Disposes this object
        /// </summary>
        /// <param name="disposing">Dispose managed resurces?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //managed
                    BaseStream.Dispose();
                    BaseStream = null;
                    BaseWriter.Dispose();
                    BaseWriter = null;
                }

                //unmanaged

                _disposed = true;
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PacketWriter()
        {
            Dispose(false);
        }
    }
}
