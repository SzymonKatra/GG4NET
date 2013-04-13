using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GG4NET
{
    /// <summary>
    /// Class to read data packets
    /// </summary>
    internal class PacketReader : IDisposable
    {
        private bool _disposed = false;

        /// <summary>
        /// Content
        /// </summary>
        public byte[] Data { get; private set; }
        /// <summary>
        /// Base Stream
        /// </summary>
        public Stream BaseStream { get; private set; }
        /// <summary>
        /// Base BinaryReader
        /// </summary>
        public BinaryReader BaseReader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Data to read</param>
        public PacketReader(byte[] data)
            : this(data, Encoding.UTF8)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Data to read</param>
        /// <param name="encoding">Used encoding</param>
        public PacketReader(byte[] data, Encoding encoding)
        {
            Data = data;
            BaseStream = new MemoryStream(Data);
            BaseReader = new BinaryReader(BaseStream, encoding);
        }

        #region ReadMethods
        /// <summary>
        /// Reads the next char
        /// </summary>
        /// <returns>Returns the next avaiable character and does not advance the byte or character position</returns>
        public int PeekChar()
        {
            return BaseReader.PeekChar();
        }
        /// <summary>
        /// Read data
        /// </summary>
        /// <returns></returns>
        public int Read()
        {
            return BaseReader.Read();
        }
        /// <summary>
        /// Read the specified number of bytes
        /// </summary>
        /// <param name="buffer">Buffer to store readed data</param>
        /// <param name="index">Start read position</param>
        /// <param name="count">Count of bytes from start to read</param>
        /// <returns></returns>
        public int Read(byte[] buffer, int index, int count)
        {
            return BaseReader.Read(buffer, index, count);
        }
        /// <summary>
        /// Read the specified number of chars
        /// </summary>
        /// <param name="buffer">Buffer to store readed data</param>
        /// <param name="index">Start read position</param>
        /// <param name="count">Count of bytes from start to read</param>
        /// <returns></returns>
        public int Read(char[] buffer, int index, int count)
        {
            return BaseReader.Read(buffer, index, count);
        }
        /// <summary>
        /// Reads boolean
        /// </summary>
        /// <returns></returns>
        public bool ReadBoolean()
        {
            return BaseReader.ReadBoolean();
        }
        /// <summary>
        /// Reads single byte
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            return BaseReader.ReadByte();
        }
        /// <summary>
        /// Reads the specified number of bytes
        /// </summary>
        /// <param name="count">Number of bytes to read</param>
        /// <returns></returns>
        public byte[] ReadBytes(int count)
        {
            return BaseReader.ReadBytes(count);
        }
        /// <summary>
        /// Reads single character
        /// </summary>
        /// <returns></returns>
        public char ReadChar()
        {
            return BaseReader.ReadChar();
        }
        /// <summary>
        /// Reads the specified number of characters
        /// </summary>
        /// <param name="count">Number of characters to read</param>
        /// <returns></returns>
        public char[] ReadChars(int count)
        {
            return BaseReader.ReadChars(count);
        }
        /// <summary>
        /// Reads single decimal
        /// </summary>
        /// <returns></returns>
        public decimal ReadDecimal()
        {
            return BaseReader.ReadDecimal();
        }
        /// <summary>
        /// Reads single double
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            return BaseReader.ReadDouble();
        }
        /// <summary>
        /// Reads single short
        /// </summary>
        /// <returns></returns>
        public short ReadInt16()
        {
            return BaseReader.ReadInt16();
        }
        /// <summary>
        /// Reads single int
        /// </summary>
        /// <returns></returns>
        public int ReadInt32()
        {
            return BaseReader.ReadInt32();
        }
        /// <summary>
        /// Reads single long
        /// </summary>
        /// <returns></returns>
        public long ReadInt64()
        {
            return BaseReader.ReadInt64();
        }
        /// <summary>
        /// Reads single sbyte
        /// </summary>
        /// <returns></returns>
        public sbyte ReadSByte()
        {
            return BaseReader.ReadSByte();
        }
        /// <summary>
        /// Reads single float
        /// </summary>
        /// <returns></returns>
        public float ReadSingle()
        {
            return BaseReader.ReadSingle();
        }
        /// <summary>
        /// Reads string
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            return BaseReader.ReadString();
        }
        /// <summary>
        /// Reads unsigned short
        /// </summary>
        /// <returns></returns>
        public ushort ReadUInt16()
        {
            return BaseReader.ReadUInt16();
        }
        /// <summary>
        /// Reads unsigned int
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt32()
        {
            return BaseReader.ReadUInt32();
        }
        /// <summary>
        /// Reads unsigned long
        /// </summary>
        /// <returns></returns>
        public ulong ReadUInt64()
        {
            return BaseReader.ReadUInt64();
        }
        #endregion

        /// <summary>
        /// Closes the reader
        /// </summary>
        public void Close()
        {
            BaseReader.Close();
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
                    BaseReader.Dispose();
                    BaseReader = null;
                    Data = null;
                }

                //unmanaged

                _disposed = true;
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PacketReader()
        {
            Dispose(false);
        }
    }
}
