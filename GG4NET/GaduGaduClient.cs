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
        private System.Timers.Timer _pingTimer;
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
        public event EventHandler ConnectFailed = null;
        public event EventHandler LoginFailed = null;
        public event EventHandler ServerObtainingFailed = null;
        public event EventHandler Connected = null;
        public event EventHandler Disconnected = null;
        public event EventHandler Logged = null;
        public event EventHandler ServerObtained = null;
        public event EventHandler<StatusEventArgs> StatusChanged = null;
        public event EventHandler<MessageEventArgs> MessageReceived = null;
        public event EventHandler NoMailNotify = null;
        public event EventHandler<MultiloginEventArgs> MultiloginNotify = null;
        public event EventHandler<TypingNotifyEventArgs> TypingNotifyReceived = null;
        public event EventHandler<PublicDirectoryReplyEventArgs> PublicDirectoryReplyReceived = null;
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
            Connect(GGPort.P8074);
        }
        public void Connect(GGPort port)
        {
            if (_socket != null) if (_socket.Connected) throw new Exception("You are already connected! Call Disconnect method");

            if (!ThreadPool.QueueUserWorkItem(ObtainServer, port)) ObtainServer(port);         
        }
        public virtual void Connect(EndPoint serverEp)
        {
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
            SendMessage(recipient, message, string.Format("<span style=\"color:#000000; font-family:'MS Shell Dlg 2'; font-size:9pt; \">{0}</span>\0", message), null);
        }
        public void SendMessage(uint recipient, string message, byte[] attributes)
        {
            SendMessage(recipient, message, string.Format("<span style=\"color:#000000; font-family:'MS Shell Dlg 2'; font-size:9pt; \">{0}</span>\0", message), attributes);
        }
        public void SendMessage(uint recipient, string message, string htmlMessage)
        {
            SendMessage(recipient, message, htmlMessage, null);
        }
        public void SendMessage(uint recipient, string message, string htmlMessage, byte[] attributes)
        {
            if (_isLogged)
            {
                Send(Packets.WriteSendMessage(recipient, message, htmlMessage, attributes));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
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

        public void SendTypingNotify(uint uin, ushort length)
        {
            if (_isLogged)
            {
                SendTypingNotify(uin, TypingNotifyType.None, length);
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }
        public void SendTypingNotify(uint uin, TypingNotifyType type, ushort length)
        {
            if (_isLogged)
            {
                Send(Packets.WriteTypingNotify(uin, type, length));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }

        public void SendPublicDirectoryRequest(uint uin, string firstName, string lastName, string nickname, int startBirthYear, int stopBirthYear, string city, Gender gender, bool activeOnly, string familyName, string familyCity, uint uinStart)
        {
            if (_isLogged)
            {
                Send(Packets.WritePublicDirectoryRequest(Container.GG_PUBDIR50_SEARCH, uin, firstName, lastName, nickname, startBirthYear, stopBirthYear, city, gender, activeOnly, familyName, familyCity, uinStart));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }
        public void SendMyDataToPublicDirectory(string firstName, string lastName, string nickname, int birthYear, string city, Gender gender, string familyName, string familyCity)
        {
            if (_isLogged)
            {
                Send(Packets.WritePublicDirectoryRequest(Container.GG_PUBDIR50_WRITE, 0, firstName, lastName, nickname, birthYear, 0, city, gender, false, familyName, familyCity, 0));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }
        public void GetMyDataFromPublicDirectory()
        {
            if (_isLogged)
            {
                Send(Packets.WritePublicDirectoryRequest(Container.GG_PUBDIR50_SEARCH, _uin, string.Empty, string.Empty, string.Empty, 0, 0, string.Empty, Gender.None, false, string.Empty, string.Empty, 0));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
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
            GGPort port = (GGPort)data;

            IPAddress ip = HTTPServices.ObtainServer(_uin);
            if (ip == IPAddress.None)
            {
                OnServerObtainingFailed();
                return;
            }
            OnServerObtained();
            Connect(new IPEndPoint(ip, (int)port));
        }
        private void _receiver_PacketArrived(object sender, PacketReceiverMessage e)
        {
            switch (e.PacketType)
            {
                case Container.GG_WELCOME: ProcessWelcome(e.Data); break;

                case Container.GG_LOGIN80_OK: ProcessLoginOk(e.Data); break;

                case Container.GG_LOGIN80_FAILED: ProcessLoginFailed(e.Data); break;

                case Container.GG_RECV_OWN_MSG:
                case Container.GG_RECV_MSG80: ProcessReceiveMessage(e.Data); break;

                case Container.GG_NOTIFY_REPLY80:
                case Container.GG_STATUS80: ProcessNotifyReply(e.Data); break;

                case Container.GG_NEED_EMAIL: ProcessNeedEmail(e.Data); break;

                case Container.GG_MULTILOGON_INFO: ProcessMultilogonInfo(e.Data); break;

                case Container.GG_TYPING_NOTIFY: ProcessTypingNotify(e.Data); break;

                case Container.GG_PUBDIR50_REPLY: ProcessPublicDirectoryReply(e.Data); break;
            }
        }
        private void CloseSocket()
        {
            try
            {
                _isLogged = false;
                _socket.Close();
            }
            catch { }
            _pingTimer.Stop();
        }
        #endregion

        #region PacketProcessors
        protected virtual void ProcessWelcome(byte[] data)
        {
            uint seed;
            Packets.ReadWelcome(data, out seed);
            Send(Packets.WriteLogin(_uin, _pass, seed, _status, _description));
        }
        protected virtual void ProcessLoginOk(byte[] data)
        {
            Send(Packets.WriteEmptyContactList());

            //sending contact list
            //400 per packet
            //using GG_NOTIFY_ADD because GG_NOTIFY_FIRST/LAST not working for me
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
        }
        protected virtual void ProcessLoginFailed(byte[] data)
        {
            _isLogged = false;
            OnLoginFailed();
        }
        protected virtual void ProcessReceiveMessage(byte[] data)
        {
            uint uin;
            uint seq;
            DateTime time;
            string plain;
            string html;
            byte[] attrib;
            Packets.ReadReceiveMessage(data, out uin, out seq, out time, out plain, out html, out attrib);

            Send(Packets.WriteReceiveAck(seq));
            OnMessageReceived(new MessageEventArgs(uin, time, plain, html, attrib)); 
        }
        protected virtual void ProcessNotifyReply(byte[] data)
        {
            List<ContactInfo> conList;
            Packets.ReadNotifyReply(data, out conList);
            foreach (ContactInfo item in conList)
            {
                if (item.Uin == _uin) //multilogin status adaptation
                {
                    _status = item.Status;
                    _description = item.Description;
                    OnStatusChanged(new StatusEventArgs(item));
                }
                else
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
            }
        }
        protected virtual void ProcessNeedEmail(byte[] data)
        {
            OnNoMailNotify();
        }
        protected virtual void ProcessMultilogonInfo(byte[] data)
        {
            MultiloginInfo[] infos;
            Packets.ReadMultiloginInfo(data, out infos);
            foreach (MultiloginInfo info in infos) OnMultiloginNotify(new MultiloginEventArgs(info));
        }
        protected virtual void ProcessTypingNotify(byte[] data)
        {
            uint uin;
            TypingNotifyType type;
            ushort length;

            Packets.ReadTypingNotify(data, out uin, out type, out length);

            OnTypingNotifyReceived(new TypingNotifyEventArgs(uin, type, length));
        }
        protected virtual void ProcessPublicDirectoryReply(byte[] data)
        {
            PublicDirectoryReply[] reply;
            uint ns;
            Packets.ReadPublicDirectoryReply(data, out reply, out ns);
            OnPublicDirectoryReplyReceived(new PublicDirectoryReplyEventArgs(reply, ns));
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
                _pingTimer = new System.Timers.Timer(1000 * 60 * 4); //4 minutes
                _pingTimer.Elapsed += (s, e) => { Send(Packets.WritePing()); };
                _pingTimer.Start();
            }
            catch { OnConnectFailed(); CloseSocket(); }
        }       
        protected void OnDisconnectCallback(IAsyncResult result)
        {
            try
            {
                _socket.EndDisconnect(result);
            }
            catch { }
            CloseSocket();
            OnDisconnected();
        }
        protected void OnReceiveCallback(IAsyncResult result)
        {
            try
            {
                int bytesReaded = _socket.EndReceive(result);
                if (bytesReaded > 0)
                {
                    _receiver.DataReceived(_buffer, 0, bytesReaded);
                    _socket.BeginReceive(_buffer, 0, 8192, 0, new AsyncCallback(OnReceiveCallback), _socket);
                }
                else throw new Exception();
            }
            catch { CloseSocket(); OnDisconnected(); }
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
        protected virtual void OnConnectFailed()
        {
            if (ConnectFailed != null) ConnectFailed(this, EventArgs.Empty);
        }
        protected virtual void OnLoginFailed()
        {
            if (LoginFailed != null) LoginFailed(this, EventArgs.Empty);
        }
        protected virtual void OnServerObtainingFailed()
        {
            if (ServerObtainingFailed != null) ServerObtainingFailed(this, EventArgs.Empty);
        }
        protected virtual void OnConnected()
        {
            if (Connected != null) Connected(this, EventArgs.Empty);
        }
        protected virtual void OnDisconnected()
        {
            if (Disconnected != null) Disconnected(this, EventArgs.Empty);
        }
        protected virtual void OnLogged()
        {
            if (Logged != null) Logged(this, EventArgs.Empty);
        }
        protected virtual void OnServerObtained()
        {
            if (ServerObtained != null) ServerObtained(this, EventArgs.Empty);
        }
        protected virtual void OnStatusChanged(StatusEventArgs e)
        {
            if (StatusChanged != null) StatusChanged(this, e);
        }
        protected virtual void OnMessageReceived(MessageEventArgs e)
        {
            if (MessageReceived != null) MessageReceived(this, e);
        }
        protected virtual void OnNoMailNotify()
        {
            if (NoMailNotify != null) NoMailNotify(this, EventArgs.Empty);
        }
        protected virtual void OnMultiloginNotify(MultiloginEventArgs e)
        {
            if (MultiloginNotify != null) MultiloginNotify(this, e);
        }
        protected virtual void OnTypingNotifyReceived(TypingNotifyEventArgs e)
        {
            if (TypingNotifyReceived != null) TypingNotifyReceived(this, e);
        }
        protected virtual void OnPublicDirectoryReplyReceived(PublicDirectoryReplyEventArgs e)
        {
            if (PublicDirectoryReplyReceived != null) PublicDirectoryReplyReceived(this, e);
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
