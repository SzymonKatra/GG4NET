using System;

namespace GG4NET
{
    internal class PacketReceiverMessage : EventArgs
    {
        private uint _packetType;
        private uint _packetLength;
        private byte[] _data;

        public uint PacketType { get { return _packetType; } }
        public uint PacketLength { get { return _packetLength; } }
        public byte[] Data { get { return _data; } }

        public PacketReceiverMessage(uint type, uint length, byte[] data)
        {
            _packetType = type;
            _packetLength = length;
            _data = data;
        }
    }

    internal class PacketReceiver
    {
        private byte[] _packetTypeBuffer = new byte[4];
        private byte[] _packetLengthBuffer = new byte[4];
        private byte[] _dataBuffer = null;
        private bool _packetTypeReceived = false;
        private int _bytesReceived = 0;

        public event EventHandler<PacketReceiverMessage> PacketArrived = null;

        public void DataReceived(byte[] buffer, int offset, int size)
        {
            int pos = offset;
            int offSiz = Math.Min(buffer.Length, offset + size);
            while (pos < offSiz)
            {
                int available = offSiz - pos;   
                if (_dataBuffer != null) //receiving packet data
                {
                    int toRead = Math.Min(_dataBuffer.Length - _bytesReceived, available);
                    Array.Copy(buffer, pos, _dataBuffer, _bytesReceived, toRead);
                    pos += toRead;
                    ReadFinished(toRead);
                }          
                else if (!_packetTypeReceived) //receiving packet type
                {
                    int toRead = Math.Min(_packetTypeBuffer.Length - _bytesReceived, available);
                    Array.Copy(buffer, pos, _packetTypeBuffer, _bytesReceived, toRead);
                    pos += toRead;
                    ReadFinished(toRead);
                }
                else //receiving packet structHeader
                {
                    int toRead = Math.Min(_packetLengthBuffer.Length - _bytesReceived, available);
                    Array.Copy(buffer, pos, _packetLengthBuffer, _bytesReceived, toRead);
                    pos += toRead;
                    ReadFinished(toRead);
                }
            }
        }
        protected void ReadFinished(int bytesTransferred)
        {
            _bytesReceived += bytesTransferred;
            if (_dataBuffer != null)
            {
                if (_bytesReceived >= _dataBuffer.Length)
                {
                    if (PacketArrived != null) PacketArrived(this, new PacketReceiverMessage(BitConverter.ToUInt32(_packetTypeBuffer, 0), BitConverter.ToUInt32(_packetLengthBuffer, 0), _dataBuffer));
                    _packetTypeReceived = false;
                    _dataBuffer = null;
                    _bytesReceived = 0;
                }
            }
            else if (!_packetTypeReceived)
            {
                if (_bytesReceived >= 4)
                {
                    _packetTypeReceived = true;
                    _bytesReceived = 0;
                }
            }
            else
            {
                if (_bytesReceived >= 4)
                {
                    _dataBuffer = new byte[BitConverter.ToUInt32(_packetLengthBuffer, 0)];
                    _bytesReceived = 0;
                }
            }
        }
    }
}
