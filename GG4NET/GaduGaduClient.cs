using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GG4NET
{
    public class GaduGaduClient : IDisposable
    {
        private uint _uin = 0;
        private string _pass = string.Empty;
        private Status _status = Status.Available;
        private string _description = string.Empty;
        private bool _disposed = false;

        private EndPoint _serverEp = new IPEndPoint(IPAddress.None, 0);
        private Socket _socket = null;
        private PacketReceiver _receiver = null;
        private bool _isLogged = false;
        private byte[] _buffer = null;

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

        private EventHandler _connectFailed = null;
        private EventHandler _loginFailed = null;

        public event EventHandler Connected = null;
        public event EventHandler Disconnected = null;
        public event EventHandler Logged = null;
        public event EventHandler ServerObtained = null;
        public event EventHandler<MessageEventArgs> MessageReceived = null;

        public GaduGaduClient(uint uin, string password)
        {
            _uin = uin;
            _pass = password;
        }

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
            IPAddress ip = HTTPServices.ObtainServer(_uin);
            if (ip == IPAddress.None)
            {
                if (serverObtainingFailed != null) serverObtainingFailed(this, EventArgs.Empty);
                return;
            }
            if (ServerObtained != null) ServerObtained(this, EventArgs.Empty);
            Connect(connectFailed, loginFailed, new IPEndPoint(ip, (int)port));
            
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

        protected void Send(byte[] data)
        {
            if (_socket == null || !_socket.Connected) throw new Exception("You are not connected!");
            _socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(OnSendCallback), _socket);
        }
        private void _receiver_PacketArrived(object sender, PacketReceiverMessage e)
        {
            switch (e.PacketType)
            {
                case Container.GG_WELCOME:
                    uint seed;
                    Packets.ReadWelcome(e.Data, out seed);
                    Send(Packets.WriteLogin(_uin, _pass, seed, _status, _description));
                    break;

                case Container.GG_LOGIN80_OK:
                    if (Logged != null) Logged(this, EventArgs.Empty);
                    Send(Packets.WriteEmptyList());
                    _isLogged = true;
                    break;

                case Container.GG_LOGIN80_FAILED:
                    if (_loginFailed != null) _loginFailed(this, EventArgs.Empty);
                    _isLogged = false;
                    break;

                case Container.GG_RECV_MSG80:
                    uint sen;
                    uint seq;
                    DateTime time;
                    string plain;
                    string html;
                    Packets.ReadReceiveMessage(e.Data, out sen, out seq, out time, out plain, out html);

                    if (MessageReceived != null) MessageReceived(this, new MessageEventArgs(sen, time, plain, html));

                    Send(Packets.WriteReceiveAck(seq));
                    break;
            }
        }

        protected void OnConnectCallback(IAsyncResult result)
        {
            try
            {
                _socket.EndConnect(result);
                if (Connected != null) Connected(this, EventArgs.Empty);
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
            if (Disconnected != null) Disconnected(this, EventArgs.Empty);
        }
        protected void OnReceiveCallback(IAsyncResult result)
        {
            try
            {
                int bytesReaded = _socket.EndReceive(result);
                if (bytesReaded > 0)
                {
                    byte[] bf = new byte[bytesReaded];
                    Buffer.BlockCopy(_buffer, 0, bf, 0, bytesReaded);
                    _receiver.DataReceived(bf);
                    _socket.BeginReceive(_buffer, 0, 8192, 0, new AsyncCallback(OnReceiveCallback), _socket);
                }
                else throw new Exception();
            }
            catch { _isLogged = false; if (Disconnected != null) Disconnected(this, EventArgs.Empty); }
        }
        protected void OnSendCallback(IAsyncResult result)
        {
            try
            {
                _socket.EndSend(result);
            }
            catch { }
        }

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
    }
}
