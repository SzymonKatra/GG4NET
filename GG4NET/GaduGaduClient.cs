using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GG4NET
{
    public class GaduGaduClient : IDisposable
    {
        #region Properties
        private uint _uin = 0;
        private string _pass = string.Empty;
        private Status _status = Status.Available;
        private string _description = string.Empty;
        private List<ContactInfo> _contactList = new List<ContactInfo>();
        private EndPoint _serverEp = new IPEndPoint(IPAddress.None, 0);
        private Socket _socket = null;
        private PacketReceiver _receiver = null;
        private bool _isLogged = false;
        private byte[] _buffer = null;
        private bool _disposed = false;

        public uint Uin { get { return _uin; } }
        public string Password { get { return _pass; } }
        public Status Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                if (_isLogged) Send(Packets.WriteStatus(_status, _description));
            }
        }
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                if (_isLogged) Send(Packets.WriteStatus(_status, _description));
            }
        }
        public bool IsLogged { get { return _isLogged; } }
        #endregion

        #region Events
        private EventHandler _connectFailed = null;
        private EventHandler _loginFailed = null;
        private EventHandler _serverObtainingFailed = null;

        public event EventHandler Connected = null;
        public event EventHandler Disconnected = null;
        public event EventHandler Logged = null;
        public event EventHandler ServerObtained = null;
        public event EventHandler<StatusEventArgs> StatusChanged = null;
        public event EventHandler<MessageEventArgs> MessageReceived = null;
        public event EventHandler NoMailNotify = null;
        #endregion

        #region Constructors
        public GaduGaduClient(uint uin, string password)
        {
            _uin = uin;
            _pass = password;
        }
        #endregion

        #region Methods
        #region Common
        public void Connect()
        {
            Connect(null, null, null, GGPort.P8074);
        }
        public void Connect(EventHandler connectFailed, EventHandler loginFailed, EventHandler serverObtainingFailed)
        {
            Connect(connectFailed, loginFailed, serverObtainingFailed, GGPort.P8074);
        }
        public void Connect(EventHandler connectFailed, EventHandler loginFailed, EventHandler serverObtainingFailed, GGPort port)
        {
            if (_socket != null) if (_socket.Connected) throw new Exception("You are already connected! Call Disconnect method");

            object asyncData = new object[] { connectFailed, loginFailed, port };
            if (!ThreadPool.QueueUserWorkItem(ObtainServer, asyncData)) ObtainServer(asyncData);

            _serverObtainingFailed = serverObtainingFailed;           
        }
        public void Connect(EventHandler connectFailed, EventHandler loginFailed, EndPoint serverEp)
        {
            _connectFailed = connectFailed;
            _loginFailed = loginFailed;
            if (_socket != null) if (_socket.Connected) throw new Exception("You are already connected! Call Disconnect method");

            _serverEp = serverEp;
            _socket = new Socket(_serverEp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.BeginConnect(_serverEp, new AsyncCallback(OnConnectCallback), _socket);
        }
        public void Disconnect()
        {
            if (_socket == null) throw new Exception("You are not connected!");
            _socket.BeginDisconnect(false, new AsyncCallback(OnDisconnectCallback), _socket);
        }
        public void SendMessage(uint recipient, string message)
        {
            Send(Packets.WriteSendMessage(recipient, message));
        }

        public void AddNotify(uint uin)
        {
            AddNotify(uin, ContactType.Normal);
        }
        public void AddNotify(uint uin, ContactType type)
        {
            _contactList.Add(new ContactInfo() { Uin = uin, Type = type });
            if (_isLogged) Send(Packets.WriteAddNotify(uin, type));
        }
        public void RemoveNotify(uint uin)
        {
            ContactInfo ci = _contactList.Find(x => x.Uin == uin);
            _contactList.Remove(ci);
            if (_isLogged) Send(Packets.WriteRemoveNotify(uin, ci.Type));
        }
        public void RemoveNotify(uint uin, ContactType type)
        {
            ContactInfo ci = _contactList.Find(x => x.Uin == uin);
            _contactList.Remove(ci);
            if (_isLogged) Send(Packets.WriteRemoveNotify(uin, type));
        }
        public ContactInfo GetNotifyInfo(uint uin)
        {
            return _contactList.Find(x => x.Uin == uin);
        }
        #endregion

        #region Other
        protected void Send(byte[] data)
        {
            if (_socket == null || !_socket.Connected) throw new Exception("You are not connected!");
            _socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(OnSendCallback), _socket);
        }
        private void ObtainServer(object data)
        {
            object[] asyncData = (object[])data;
            EventHandler connectFailed = (EventHandler)asyncData[0];
            EventHandler loginFailed = (EventHandler)asyncData[1];
            GGPort port = (GGPort)asyncData[2];

            IPAddress ip = HTTPServices.ObtainServer(_uin);
            if (ip == IPAddress.None)
            {
                if (_serverObtainingFailed != null) _serverObtainingFailed(this, EventArgs.Empty);
                return;
            }
            OnServerObtained();
            Connect(connectFailed, loginFailed, new IPEndPoint(ip, (int)port));
        }
        private void _receiver_PacketArrived(object sender, PacketReceiverMessage e)
        {
            switch (e.PacketType)
            {
                #region GG_WELCOME
                case Container.GG_WELCOME:
                    uint seed;
                    Packets.ReadWelcome(e.Data, out seed);
                    Send(Packets.WriteLogin(_uin, _pass, seed, _status, _description));
                    break;
                #endregion
                #region GG_LOGIN80_OK
                case Container.GG_LOGIN80_OK:
                    //if (_contactList.Count <= 0)
                    //{
                    //    Send(Packets.WriteEmptyContactList());
                    //}
                    //else
                    //{
                    //    int remOffset = 0;
                    //    while (remOffset < _contactList.Count) Send(Packets.WriteContactList(_contactList, ref remOffset));
                    //}
                    Send(Packets.WriteEmptyContactList());
                    //foreach (ContactInfo item in _contactList) Send(Packets.WriteAddNotify(item));
                    for (int i = 0; i < _contactList.Count; i++)
                    {
                        int toSend = Math.Min(400, _contactList.Count - i);
                        using (PacketWriter writer = new PacketWriter())
                        {
                            for (int j = i; j < toSend + i; j++) writer.Write(Packets.WriteAddNotify(_contactList[j].Uin, _contactList[j].Type));
                            Send(writer.Data);
                        }
                        i += toSend;
                    }
                    _isLogged = true;
                    OnLogged();
                    break;
                #endregion
                #region GG_LOGIN80_FAILED
                case Container.GG_LOGIN80_FAILED:
                    if (_loginFailed != null) _loginFailed(this, EventArgs.Empty);
                    _isLogged = false;
                    break;
                #endregion
                #region GG_RECV_MSG80
                case Container.GG_RECV_MSG80:
                    uint sen;
                    uint seq;
                    DateTime time;
                    string plain;
                    string html;
                    byte[] attrib;
                    Packets.ReadReceiveMessage(e.Data, out sen, out seq, out time, out plain, out html, out attrib);

                    Send(Packets.WriteReceiveAck(seq));
                    OnMessageReceived(new MessageEventArgs(sen, time, plain, html, attrib));                
                    break;
                #endregion
                #region GG_NOTIFY_REPLY80 GG_STATUS80
                case Container.GG_NOTIFY_REPLY80:
                case Container.GG_STATUS80:
                    List<ContactInfo> conList;
                    Packets.ReadNotifyReply(e.Data, out conList);
                    foreach (ContactInfo item in conList)
                    {
                        for (int i = 0; i < _contactList.Count; i++)
                        {
                            if (_contactList[i].Uin == item.Uin)
                            {
                                _contactList[i] = item;
                                OnStatusChanged(new StatusEventArgs(item));
                                break;
                            }
                        }
                    }
                    break;
                #endregion
                #region GG_NEED_EMAIL
                case Container.GG_NEED_EMAIL:
                    OnNoMailNotify();
                    break;
                #endregion
            }
        }
        #endregion

        #region Callback
        protected void OnConnectCallback(IAsyncResult result)
        {
            try
            {
                _socket.EndConnect(result);
                OnConnected();
                _buffer = new byte[8192];
                _receiver = new PacketReceiver();
                _receiver.PacketArrived += _receiver_PacketArrived;
                _socket.BeginReceive(_buffer, 0, 8192, 0, new AsyncCallback(OnReceiveCallback), _socket);
            }
            catch { if (_connectFailed != null)_connectFailed(this, EventArgs.Empty); }
        }       
        protected void OnDisconnectCallback(IAsyncResult result)
        {
            try
            {
                _isLogged = false;
                _socket.EndDisconnect(result);
                _socket.Close();
            }
            catch { }
            OnDisconnected();
        }
        protected void OnReceiveCallback(IAsyncResult result)
        {
            try
            {
                int bytesReaded = _socket.EndReceive(result);
                if (bytesReaded > 0)
                {
                    //byte[] bf = new byte[bytesReaded];
                    //Buffer.BlockCopy(_buffer, 0, bf, 0, bytesReaded);
                    //_receiver.DataReceived(bf);
                    _receiver.DataReceived(_buffer, 0, bytesReaded);
                    _socket.BeginReceive(_buffer, 0, 8192, 0, new AsyncCallback(OnReceiveCallback), _socket);
                }
                else throw new Exception();
            }
            catch { _isLogged = false; OnDisconnected(); }
        }
        protected void OnSendCallback(IAsyncResult result)
        {
            try
            {
                _socket.EndSend(result);
            }
            catch { }
        }
        #endregion

        #region EventPerformers
        protected void OnConnected()
        {
            if (Connected != null) Connected(this, EventArgs.Empty);
        }
        protected void OnDisconnected()
        {
            if (Disconnected != null) Disconnected(this, EventArgs.Empty);
        }
        protected void OnLogged()
        {
            if (Logged != null) Logged(this, EventArgs.Empty);
        }
        protected void OnServerObtained()
        {
            if (ServerObtained != null) ServerObtained(this, EventArgs.Empty);
        }
        protected void OnStatusChanged(StatusEventArgs e)
        {
            if (StatusChanged != null) StatusChanged(this, e);
        }
        protected void OnMessageReceived(MessageEventArgs e)
        {
            if (MessageReceived != null) MessageReceived(this, e);
        }
        protected void OnNoMailNotify()
        {
            if (NoMailNotify != null) NoMailNotify(this, EventArgs.Empty);
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //managed
                    _socket.Dispose();
                    _buffer = null;
                }

                //unmanaged

                _disposed = true;
            }
        }
        ~GaduGaduClient()
        {
            Dispose(false);
        }
        #endregion
        #endregion
    }
}
