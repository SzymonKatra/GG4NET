using System;
using System.Collections.Generic;
using System.Net;

namespace GG4NET
{
    public class MultiloginInfo
    {
        private IPAddress _ip;
        private DateTime _logonTime;
        private uint _flags = 0;
        private uint _features = 0;
        private ulong _connectionId;
        private string _clientName;

        public IPAddress IP
        {
            get { return _ip; }
            set { _ip = value; }
        }
        public DateTime LogonTime
        {
            get { return _logonTime; }
            set { _logonTime = value; }
        }
        internal uint Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }
        internal uint Features
        {
            get { return _features; }
            set { _features = value; }
        }
        public ulong ConnectionId
        {
            get { return _connectionId; }
            set { _connectionId = value; }
        }
        public string ClientName
        {
            get { return _clientName; }
            set { _clientName = value; }
        }

        public MultiloginInfo()
            : this(IPAddress.None, DateTime.MinValue, 0, string.Empty)
        {
        }
        public MultiloginInfo(IPAddress ip, DateTime logonTime, ulong connectionId, string clientName)
        {
            _ip = ip;
            _logonTime = logonTime;
            _connectionId = connectionId;
            _clientName = clientName;
        }
    }
}
