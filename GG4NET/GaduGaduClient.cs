using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

namespace GG4NET
{
    /// <summary>
    /// Klasa używana do obsługi Gadu-Gadu
    /// </summary>
    public class GaduGaduClient : IDisposable
    {
        #region Properties
        private uint _uin = 0;
        private string _pass = string.Empty;
        private Status _status = Status.Available;
        private string _description = string.Empty;
        private List<UserInfo> _contactList = new List<UserInfo>();
        private EndPoint _serverEp = new IPEndPoint(IPAddress.None, 0);
        private Socket _socket = null;
        private PacketReceiver _receiver = null;
        private bool _isLogged = false;
        private byte[] _buffer = null;
        private System.Timers.Timer _pingTimer;
        private uint _lastContactListVersion = 0;
        private SynchronizationContext _syncContext = null;
        private List<ImageData> _receivingImages;
        private bool _disposed = false;

        /// <summary>
        /// Numer GG
        /// </summary>
        public uint Uin { get { return _uin; } }
        /// <summary>
        /// Hasło do GG
        /// </summary>
        public string Password { get { return _pass; } }
        /// <summary>
        /// Status
        /// </summary>
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
        /// <summary>
        /// Opis
        /// </summary>
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
        /// <summary>
        /// Czy zalogowany?
        /// </summary>
        public bool IsLogged { get { return _isLogged; } }
        /// <summary>
        /// Kontekst z którym synchronizować zdarzenia.
        /// </summary>
        public SynchronizationContext SyncContext
        {
            get { return _syncContext; }
            set { _syncContext = value; }
        }
        #endregion

        #region Events
        /// <summary>Wywołany gdy łączenie z serwerem GG nie powiedzie się.</summary>
        public event EventHandler ConnectFailed = null;
        /// <summary>Wywołany gdy logowanie do serwera GG nie powiedzie się.</summary>
        public event EventHandler LoginFailed = null;
        /// <summary>Wywołany gdy szukanie serwera GG nie powiedzie się.</summary>
        public event EventHandler ServerObtainingFailed = null;
        /// <summary>Wywołany gdy łączenie z serwerem GG powiedzie się.</summary>
        public event EventHandler Connected = null;
        /// <summary>Wywołany gdy utracimy połączenie z serwerem GG.</summary>
        public event EventHandler Disconnected = null;
        /// <summary>Wywołany gdy logowanie do serwera GG powiedzie się.</summary>
        public event EventHandler Logged = null;
        /// <summary>Wywołany gdy szukanie serwera GG powiedzie się.</summary>
        public event EventHandler ServerObtained = null;
        /// <summary>Wywołany status użytkownika z naszej listy kontaktów został zmieniony.</summary>
        public event EventHandler<StatusEventArgs> StatusChanged = null;
        /// <summary>Wywołany gdy otrzymamy od kogoś wiadomość.</summary>
        public event EventHandler<MessageEventArgs> MessageReceived = null;
        /// <summary>Wywołany gdy zostanie wysłana wiadomość z innego klienta zalogowanego na nasz numer.</summary>
        public event EventHandler<MessageEventArgs> OwnMessageReceived = null;
        /// <summary>Wywołany gdy serwer GG prosi nas o uzupełnienie adresu e-mail w profilu.</summary>
        public event EventHandler NoMailNotifyReceived = null;
        /// <summary>Wywołany gdy wykryto innego użytkownika zalogowanego na ten numerem.</summary>
        public event EventHandler<MultiloginEventArgs> MultiloginNotifyReceived = null;
        /// <summary>Wywołany gdy otrzymamy powiadomienie o pisaniu.</summary>
        public event EventHandler<TypingNotifyEventArgs> TypingNotifyReceived = null;
        /// <summary>Wywołany gdy otrzymamy odpowiedź publicznego katalogu.</summary>
        public event EventHandler<PublicDirectoryReplyEventArgs> PublicDirectoryReplyReceived = null;
        /// <summary>Wywołany gdy otrzymamy od serwera wiadomość XML GG Live.</summary>
        public event EventHandler<XmlMessageEventArgs> XmlGGLiveMessageReceived = null;
        /// <summary>Wywołany gdy otrzymamy od serwera wiadomość systemowa XML.</summary>
        public event EventHandler<XmlMessageEventArgs> XmlSystemMessageReceived = null;
        /// <summary>Wywołany gdy otrzymamy od serwera nową listę kontaktów.</summary>
        public event EventHandler<ContactListEventArgs> ContactListReceived = null;
        /// <summary>Wywołany gdy otrzymamy obrazek.</summary>
        public event EventHandler<ImageEventArgs> ImageReceived = null;
        /// <summary>Wywołany gdy otrzymamy zapytanie o obrazek.</summary>
        public event EventHandler<ImageEventArgs> ImageRequest = null;
        #endregion

        #region Constructors
        /// <summary>
        /// Stwórz klienta Gadu-Gadu
        /// </summary>
        /// <param name="uin">Numer GG</param>
        /// <param name="password">Hasło do danego numerka</param>
        public GaduGaduClient(uint uin, string password)
        {
            _uin = uin;
            _pass = password;
        }
        #endregion

        #region Methods
        #region Common
        #region Connection
        /// <summary>
        /// Znajdź serwer GG i połącz się z nim pod domyślnym portem.
        /// </summary>
        public void Connect()
        {
            Connect(GGPort.P8074);
        }
        /// <summary>
        /// Znajdź serwer GG i połącz się z nim pod podanym portem.
        /// </summary>
        /// <param name="port">Port użyty do połączenia</param>
        public void Connect(GGPort port)
        {
            if (_socket != null) if (_socket.Connected) throw new Exception("You are already connected! Call Disconnect method");

            if (!ThreadPool.QueueUserWorkItem(ObtainServer, port)) ObtainServer(port);         
        }
        /// <summary>
        /// Połącz się z serwerem GG o podanym punkcie końcowym.
        /// </summary>
        /// <param name="serverEp">Punkt końcowy serwera GG</param>
        public virtual void Connect(EndPoint serverEp)
        {
            if (_socket != null) if (_socket.Connected) throw new Exception("You are already connected! Call Disconnect method");

            _serverEp = serverEp;
            _socket = new Socket(_serverEp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.BeginConnect(_serverEp, new AsyncCallback(OnConnectCallback), _socket);
        }
        /// <summary>
        /// Rozłącz się z serwerem GG.
        /// </summary>
        public void Disconnect()
        {
            if (_socket == null || !_socket.Connected) throw new Exception("You are not connected!");
            _socket.BeginDisconnect(false, new AsyncCallback(OnDisconnectCallback), _socket);
        }
        #endregion

        #region Messages
        /// <summary>
        /// Wyślij wiadomość na dany numerek GG.
        /// </summary>
        /// <param name="recipient">Odbiorca</param>
        /// <param name="message">Wiadomość</param>
        public void SendMessage(uint recipient, string message)
        {
            MessageBuilder builder = new MessageBuilder();
            builder.AppendText(message);
            SendMessage(recipient, builder);
        }
        ///// <summary>
        ///// Wyślij wiadomość na dany numerek GG.
        ///// </summary>
        ///// <param name="recipient">Odbiorca</param>
        ///// <param name="message">Wiadomość</param>
        ///// <param name="attributes">Atrybuty wiadomości</param>
        //public void SendMessage(uint recipient, string message, byte[] attributes)
        //{
        //    SendMessage(recipient, message, string.Format("<span style=\"color:#000000; font-family:'MS Shell Dlg 2'; font-size:9pt; \">{0}</span>\0", message), attributes);
        //}
        ///// <summary>
        ///// Wyślij wiadomość na dany numerek GG.
        ///// </summary>
        ///// <param name="recipient">Odbiorca</param>
        ///// <param name="message">Wiadomość</param>
        ///// <param name="htmlMessage">Wiadomość w formacie HTML</param>
        //public void SendMessage(uint recipient, string message, string htmlMessage)
        //{
        //    SendMessage(recipient, message, htmlMessage, null);
        //}
        ///// <summary>
        ///// Wyślij wiadomość na dany numerek GG.
        ///// </summary>
        ///// <param name="recipient">Odbiorca</param>
        ///// <param name="message">Wiadomość</param>
        ///// <param name="htmlMessage">Wiadomość w formacie HTML</param>
        ///// <param name="attributes">Atrybuty wiadomości</param>
        //public void SendMessage(uint recipient, string message, string htmlMessage, byte[] attributes)
        //{
        //    if (_isLogged)
        //    {
        //        Send(Packets.WriteSendMessage(recipient, message, htmlMessage, attributes));
        //    }
        //    else throw new NotLoggedException("You are not logged to GG network!");
        //}
        /// <summary>
        /// Wyślij wiadomość na dany numerek GG.
        /// </summary>
        /// <param name="recipient">Odbiorca</param>
        /// <param name="message">Wiadomość</param>
        public void SendMessage(uint recipient, MessageBuilder message)
        {
            if (_isLogged)
            {
                message.CloseMessage();
                Send(Packets.WriteSendMessage(recipient, message.PlainMessage, message.HtmlMessage, message.Attributes));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }

        /// <summary>
        /// Wyślij wiadomość konferencyjną.
        /// </summary>
        /// <param name="recipients">Odbiorcy</param>
        /// <param name="message">Wiadomość</param>
        public void SendMessage(uint[] recipients, string message)
        {
            MessageBuilder builder = new MessageBuilder();
            builder.AppendText(message);
            builder.CloseMessage();
            SendMessage(recipients, builder);
        }
        /// <summary>
        /// Wyślij wiadomość konferencyjną.
        /// </summary>
        /// <param name="recipients">Odbiorcy</param>
        /// <param name="message">Wiadomość</param>
        public void SendMessage(uint[] recipients, MessageBuilder message)
        {
            if (_isLogged)
            {
                message.CloseMessage();

                byte[] conferenceHeader = new byte[5];
                conferenceHeader[0] = Container.GG_CONFERENCE_FLAG;
                byte[] lenBytes = BitConverter.GetBytes((uint)recipients.Length - 1);
                Buffer.BlockCopy(lenBytes, 0, conferenceHeader, 1, 4); //write number of recipients

                foreach (uint uin in recipients)
                {
                    using (PacketWriter writer = new PacketWriter())
                    {
                        writer.Write(conferenceHeader);

                        foreach (uint recNumber in recipients)
                        {
                            if (recNumber == uin) continue;
                            writer.Write(recNumber);
                        }

                        //byte[] conferenceBody = writer.Data;
                        //byte[] allAttributes = new byte[message.Attributes.Length + conferenceBody.Length];
                        
                        //Buffer.BlockCopy(conferenceBody, 0, allAttributes, 0, conferenceBody.Length);
                        //Buffer.BlockCopy(message.Attributes, 0, allAttributes, conferenceBody.Length, message.Attributes.Length);                

                        Send(Packets.WriteSendMessage(uin, message.PlainMessage, message.HtmlMessage, writer.Data/*allAttributes*/));
                        //TODO: fix conference with plain formatted message

                        writer.Close();
                    }
                }
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }
        #endregion

        #region Multilogin
        /// <summary>
        /// Rozłącza podaną sesję multilogowania.
        /// </summary>
        /// <param name="connectionId">Indetyfikator połączenia.</param>
        public void DisconnectMultiloginSession(ulong connectionId)
        {
            if (_isLogged)
            {
                Send(Packets.WriteDisconnectMultiloginSession(connectionId));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }
        #endregion

        #region Notify
        /// <summary>
        /// Dodaj numerek GG do listy kontaktów aby otrzymywać od niego powiadomienia o statusie.
        /// </summary>
        /// <param name="uin">Numerek GG</param>
        public void AddNotify(uint uin)
        {
            AddNotify(uin, ContactType.Normal);
        }
        /// <summary>
        /// Dodaj numerek GG do listy kontaktów aby otrzymywać od niego powiadomienia o statusie.
        /// </summary>
        /// <param name="uin">Numerek GG</param>
        /// <param name="type">Typ kontaktu</param>
        public void AddNotify(uint uin, ContactType type)
        {
            _contactList.Add(new UserInfo() { Uin = uin, Type = type });
            if (_isLogged) Send(Packets.WriteAddNotify(uin, type));
        }
        /// <summary>
        /// Usuń numerek GG z listy kontaktów aby nie otrzymywać od niego powiadomień o statusie.
        /// </summary>
        /// <param name="uin">Numerek GG</param>
        public void RemoveNotify(uint uin)
        {
            UserInfo ci = _contactList.Find(x => x.Uin == uin);
            _contactList.Remove(ci);
            if (_isLogged) Send(Packets.WriteRemoveNotify(uin, ci.Type));
        }
        /// <summary>
        /// Usuń numerek GG z listy kontaktów aby nie otrzymywać od niego powiadomień o statusie.
        /// </summary>
        /// <param name="uin">Numerek GG</param>
        /// <param name="type">Typ kontaktu</param>
        public void RemoveNotify(uint uin, ContactType type)
        {
            UserInfo ci = _contactList.Find(x => x.Uin == uin);
            _contactList.Remove(ci);
            if (_isLogged) Send(Packets.WriteRemoveNotify(uin, type));
        }
        /// <summary>
        /// Zdobądź informację o podanym numerku GG dodanym do listy kontaktów.
        /// </summary>
        /// <param name="uin">Numerek GG</param>
        public UserInfo GetNotifyInfo(uint uin)
        {
            return _contactList.Find(x => x.Uin == uin);
        }
        #endregion

        #region TypingNotify
        /// <summary>
        /// Wyślij powiadomienie o pisaniu na dany numer GG.
        /// </summary>
        /// <param name="uin">Numer GG</param>
        /// <param name="length">Długość wiadomości</param>
        public void SendTypingNotify(uint uin, ushort length)
        {
            if (_isLogged)
            {
                SendTypingNotify(uin, TypingNotifyType.None, length);
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }
        /// <summary>
        /// Wyślij powiadomienie o pisaniu na dany numer GG.
        /// </summary>
        /// <param name="uin">Numer GG</param>
        /// <param name="type">Typ powiadomienia</param>
        /// <param name="length">Długość wiadomości</param>
        public void SendTypingNotify(uint uin, TypingNotifyType type, ushort length)
        {
            if (_isLogged)
            {
                Send(Packets.WriteTypingNotify(uin, type, length));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }
        #endregion

        #region PublicDirectory
        /// <summary>
        /// Wyślij zapytanie do publicznego katalogu GG.
        /// </summary>
        /// <param name="uin">Numer GG</param>
        /// <param name="firstName">Imię</param>
        /// <param name="lastName">Nazwisko</param>
        /// <param name="nickname">Przezwisko</param>
        /// <param name="startBirthYear">Początkowy rok urodzenia</param>
        /// <param name="stopBirthYear">Końcowy rok urodzenia (0 jeśli chcesz znaleźć osoby urodzone w roku podanym we wcześniejszym parametrze)</param>
        /// <param name="city">Miejsce zamieszkania</param>
        /// <param name="gender">Płeć</param>
        /// <param name="activeOnly">Wyszukuj tylko dostępnych?</param>
        /// <param name="familyName">Nazwisko panieńskie</param>
        /// <param name="familyCity">Miejsce zamieszkania w czasie dzieciństwa</param>
        /// <param name="startSearch">Zacznić wyszukiwać od indeksu</param>
        public void SendPublicDirectoryRequest(uint uin, string firstName, string lastName, string nickname, int startBirthYear, int stopBirthYear, string city, Gender gender, bool activeOnly, string familyName, string familyCity, uint startSearch)
        {
            if (_isLogged)
            {
                Send(Packets.WritePublicDirectoryRequest(Container.GG_PUBDIR50_SEARCH, uin, firstName, lastName, nickname, startBirthYear, stopBirthYear, city, gender, activeOnly, familyName, familyCity, startSearch));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }
        /// <summary>
        /// Zapisz swoje dane do publicznego katalogu GG.
        /// </summary>
        /// <param name="firstName">Imię</param>
        /// <param name="lastName">Nazwisko</param>
        /// <param name="nickname">Przezwisko</param>
        /// <param name="birthYear">Rok urodzenia</param>
        /// <param name="city">Miejsce zamieszkania</param>
        /// <param name="gender">Płeć</param>
        /// <param name="familyName">Nazwisko panieńskie</param>
        /// <param name="familyCity">Miejsce zamieszkania w czasie dzieciństwa</param>
        public void SendMyDataToPublicDirectory(string firstName, string lastName, string nickname, int birthYear, string city, Gender gender, string familyName, string familyCity)
        {
            if (_isLogged)
            {
                Send(Packets.WritePublicDirectoryRequest(Container.GG_PUBDIR50_WRITE, 0, firstName, lastName, nickname, birthYear, 0, city, gender, false, familyName, familyCity, 0));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }
        /// <summary>
        /// Zdobądź moje dane z publicznego katalogu GG. Zostaną przysłane w zdarzeniu PublicDirectoryReplyReceived.
        /// </summary>
        public void GetMyDataFromPublicDirectory()
        {
            if (_isLogged)
            {
                Send(Packets.WritePublicDirectoryRequest(Container.GG_PUBDIR50_SEARCH, _uin, string.Empty, string.Empty, string.Empty, 0, 0, string.Empty, Gender.None, false, string.Empty, string.Empty, 0));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }
        #endregion

        #region ContactList
        /// <summary>
        /// Importuje listę kontaktów z serwera. Zostanie przysłana w zdarzeniu ContactListReceived.
        /// </summary>
        public void ImportContactList()
        {
            if (_isLogged)
            {
                Send(Packets.WriteUserListRequest(Container.GG_USERLIST100_GET, 0, ContactListType.XML, null));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }
        /// <summary>
        /// Eksportuje podaną listę kontaktów na serwer.
        /// </summary>
        /// <param name="contactList">Lista kontaktów do wyeksportowania.</param>
        public void ExportContactList(ContactList contactList)
        {
            if (_isLogged)
            {
                Send(Packets.WriteUserListRequest(Container.GG_USERLIST100_PUT, _lastContactListVersion, ContactListType.XML, contactList.ExportToString(ContactListType.XML)));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }
        /// <summary>
        /// Usuwa listę kontaktów z serwera.
        /// </summary>
        public void DeleteContactList()
        {
            if (_isLogged)
            {
                Send(Packets.WriteUserListRequest(Container.GG_USERLIST100_PUT, _lastContactListVersion, ContactListType.XML, " "));
            }
            else throw new NotLoggedException("You are not logged to GG network!");
        }
        #endregion

        #region Images
        /// <summary>
        /// Wyślij zapytanie o obrazek na podany numer.
        /// </summary>
        /// <param name="uin">Numer GG.</param>
        /// <param name="hash">Hash obrazka.</param>
        public void SendImageRequest(uint uin, string hash)
        {
            uint crc32;
            uint len;
            Utils.ParseImageHash(hash, out crc32, out len);
            SendImageRequest(uin, crc32, len);
        }
        /// <summary>
        /// Wyślij zapytanie o obrazek na podany numer.
        /// </summary>
        /// <param name="uin">Numer GG.</param>
        /// <param name="crc32">Suma kontrolna CRC32 obrazka.</param>
        /// <param name="length">Wielkość obrazka w bajtach.</param>
        public void SendImageRequest(uint uin, uint crc32, uint length)
        {
            using (PacketWriter writer = new PacketWriter())
            {
                writer.Write((byte)Container.GG_IMAGE_REQUEST_FLAG);
                writer.Write(length);
                writer.Write(crc32);

                Send(Packets.WriteSendMessage(uin, string.Empty, string.Empty, writer.Data, Container.GG_CLASS_CHAT));

                writer.Close();
            }
        }

        /// <summary>
        /// Wyślij odpowiedź na zapytanie o obrazek.
        /// </summary>
        /// <param name="uin">Numer GG.</param>
        /// <param name="crc32">Suma kontrolna CRC32 obrazka.</param>
        /// <param name="length">Wielkość obrazka w bajtach.</param>
        /// <param name="filePath">Ścieżka do pliku z obrazkiem</param>
        public void SendImageResponse(uint uin, uint crc32, uint length, string filePath)
        {
            SendImageResponse(uin, crc32, length, new FileStream(filePath, FileMode.Open), Path.GetFileName(filePath));
        }
        /// <summary>
        /// Wyślij odpowiedź na zapytanie o obrazek.
        /// </summary>
        /// <param name="uin">Numer GG.</param>
        /// <param name="crc32">Suma kontrolna CRC32 obrazka.</param>
        /// <param name="length">Wielkość obrazka w bajtach.</param>
        /// <param name="image">Dane obrazka.</param>
        /// <param name="fileName">Nazwa pliku</param>
        public void SendImageResponse(uint uin, uint crc32, uint length, byte[] image, string fileName)
        {
            SendImageResponse(uin, crc32,length,new MemoryStream(image),fileName);
        }
        /// <summary>
        /// Wyślij odpowiedź na zapytanie o obrazek.
        /// </summary>
        /// <param name="uin">Numer GG.</param>
        /// <param name="crc32">Suma kontrolna CRC32 obrazka.</param>
        /// <param name="length">Wielkość obrazka w bajtach.</param>
        /// <param name="imageStream">Strumień obrazka.</param>
        /// <param name="fileName">Nazwa pliku</param>
        public void SendImageResponse(uint uin, uint crc32, uint length, Stream imageStream, string fileName)
        {
            bool headerSent = false;
            byte[] fName = null;
            if (!string.IsNullOrEmpty(fileName)) fName = Encoding.UTF8.GetBytes(fileName);
            int shouldRead = Container.GG_IMAGE_REPLY_MESSAGE_MAX_SIZE - (1 + 4 + 4 + fName.Length); //flag + size + crc32 + file name length
            byte[] buffer = new byte[shouldRead];
            int len;

            while ((len = imageStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                using (PacketWriter writer = new PacketWriter())
                {
                    if (!headerSent)
                    {
                        writer.Write(Container.GG_IMAGE_RESPONSE_FLAG);
                    }
                    else writer.Write(Container.GG_IMAGE_RESPONSE_FLAG_MORE_DATA);

                    writer.Write(length);
                    writer.Write(crc32);
                    if (!headerSent && fName != null) writer.Write(fName);
                    writer.Write(buffer, 0, len);

                    Send(Packets.WriteSendMessage(uin, string.Empty, string.Empty, writer.Data));

                    headerSent = true;
                    writer.Close();
                }

                if (headerSent) shouldRead = Container.GG_IMAGE_REPLY_MESSAGE_MAX_SIZE - (1 + 4 + 4); //flag + size + crc32
                if (shouldRead != buffer.Length) Array.Resize(ref buffer, shouldRead);
            }

            imageStream.Close();
        }
        #endregion
        #endregion

        #region Other
        /// <summary>
        /// Wyślij dane.
        /// </summary>
        /// <param name="data">Dane</param>
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
            try
            {
                switch (e.PacketType)
                {
                    case Container.GG_WELCOME: ProcessWelcome(e.Data); break;

                    case Container.GG_LOGIN80_OK: ProcessLoginOk(e.Data); break;

                    case Container.GG_LOGIN80_FAILED: ProcessLoginFailed(e.Data); break;

                    case Container.GG_RECV_MSG80: ProcessReceiveMessage(e.Data); break;

                    case Container.GG_RECV_OWN_MSG: ProcessReceiveOwnMessage(e.Data); break;

                    case Container.GG_NOTIFY_REPLY80:
                    case Container.GG_STATUS80: ProcessNotifyReply(e.Data); break;

                    case Container.GG_NEED_EMAIL: ProcessNeedEmail(e.Data); break;

                    case Container.GG_MULTILOGON_INFO: ProcessMultilogonInfo(e.Data); break;

                    case Container.GG_TYPING_NOTIFY: ProcessTypingNotify(e.Data); break;

                    case Container.GG_PUBDIR50_REPLY: ProcessPublicDirectoryReply(e.Data); break;

                    case Container.GG_XML_ACTION: ProcessXmlGGLiveMessage(e.Data); break;

                    case Container.GG_XML_EVENT: ProcessXmlSystemMessage(e.Data); break;

                    case Container.GG_USERLIST100_REPLY: ProcessUserListReply(e.Data); break;

                    case Container.GG_USERLIST100_VERSION: ProcessUserListVersion(e.Data); break;
                }
            }
            catch { }
        }
        private void CloseSocket()
        {
            if (_pingTimer != null) _pingTimer.Stop();
            try
            {
                _isLogged = false;
                _socket.Close();
                _socket = null;
            }
            catch { }         
        }
        #endregion

        #region PacketProcessors
        /// <summary>
        /// Przetwórz pakiet powitalny.
        /// </summary>
        /// <param name="data">Dane</param>
        protected virtual void ProcessWelcome(byte[] data)
        {
            uint seed;
            Packets.ReadWelcome(data, out seed);
            Send(Packets.WriteLogin(_uin, _pass, seed, _status, _description));
        }
        /// <summary>
        /// Przetwórz pakiet o pomyślnym logowaniu.
        /// </summary>
        /// <param name="data">Dane</param>
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

            _pingTimer = new System.Timers.Timer(50000); //50 seconds
            _pingTimer.Elapsed += (s, e) => { Send(Packets.WritePing()); };
            _pingTimer.Start();

            _isLogged = true;

            _receivingImages = new List<ImageData>();

            OnLogged();
        }
        /// <summary>
        /// Przetwórz pakiet niepomyślnym logowaniu.
        /// </summary>
        /// <param name="data">Dane</param>
        protected virtual void ProcessLoginFailed(byte[] data)
        {
            _isLogged = false;
            OnLoginFailed();
        }
        /// <summary>
        /// Przetwórz pakiet o przysłanej wiadomości.
        /// </summary>
        /// <param name="data">Dane</param>
        protected virtual void ProcessReceiveMessage(byte[] data)
        {
            uint uin;
            uint seq;
            DateTime time;
            string plain;
            string html;
            byte[] attrib;
            Packets.ReadReceiveMessage(data, out uin, out seq, out time, out plain, out html, out attrib);

            uint[] confMembers;
            bool isImage;
            ParseAttributes(uin, attrib, out isImage, out confMembers);

            Send(Packets.WriteReceiveAck(seq));
            if (!isImage) OnMessageReceived(new MessageEventArgs(uin, time, plain, html, attrib, confMembers)); ;
        }
        /// <summary>
        /// Przetwórz pakiet o wysłanej wiadomości z innego klienta zalogowanego na nasz numer.
        /// </summary>
        /// <param name="data">Dane</param>
        protected virtual void ProcessReceiveOwnMessage(byte[] data)
        {
            uint uin;
            uint seq;
            DateTime time;
            string plain;
            string html;
            byte[] attrib;
            Packets.ReadReceiveMessage(data, out uin, out seq, out time, out plain, out html, out attrib);

            uint[] confMembers;
            bool isImage;
            ParseAttributes(uin, attrib, out isImage, out confMembers);

            Send(Packets.WriteReceiveAck(seq));
            if (!isImage) OnOwnMessageReceived(new MessageEventArgs(uin, time, plain, html, attrib, confMembers));
        }
        /// <summary>
        /// Przetwórz pakiet o zmianie statusu osoby z listy kontaktów.
        /// </summary>
        /// <param name="data">Dane</param>
        protected virtual void ProcessNotifyReply(byte[] data)
        {
            List<UserInfo> conList;
            Packets.ReadNotifyReply(data, out conList);
            foreach (UserInfo item in conList)
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
        /// <summary>
        /// Przetwórz pakiet o informacji od serwera o braku adresu e-mail w profilu.
        /// </summary>
        /// <param name="data">Dane</param>
        protected virtual void ProcessNeedEmail(byte[] data)
        {
            OnNoMailNotifyReceived();
        }
        /// <summary>
        /// Przetwórz pakiet na temat multilogowania.
        /// </summary>
        /// <param name="data">Dane</param>
        protected virtual void ProcessMultilogonInfo(byte[] data)
        {
            MultiloginInfo[] infos;
            Packets.ReadMultiloginInfo(data, out infos);
            foreach (MultiloginInfo info in infos) OnMultiloginNotifyReceived(new MultiloginEventArgs(info));
        }
        /// <summary>
        /// Przetwórz pakiet w którym są zawarte informacje na temat aktualnie wpisywanej wiadomości od kogoś innego.
        /// </summary>
        /// <param name="data">Dane</param>
        protected virtual void ProcessTypingNotify(byte[] data)
        {
            uint uin;
            TypingNotifyType type;
            ushort length;

            Packets.ReadTypingNotify(data, out uin, out type, out length);

            OnTypingNotifyReceived(new TypingNotifyEventArgs(uin, type, length));
        }
        /// <summary>
        /// Przetwórz pakiet w którym zawarta jest odpowiedź na zapytanie do publicznego katalogu GG.
        /// </summary>
        /// <param name="data">Dane</param>
        protected virtual void ProcessPublicDirectoryReply(byte[] data)
        {
            PublicDirectoryReply[] reply;
            uint ns;
            Packets.ReadPublicDirectoryReply(data, out reply, out ns);
            OnPublicDirectoryReplyReceived(new PublicDirectoryReplyEventArgs(reply, ns));
        }
        /// <summary>
        /// Przetwórz pakiet w którym została przysłana wiadomość XML GGLive.
        /// </summary>
        /// <param name="data">Dane</param>
        protected virtual void ProcessXmlGGLiveMessage(byte[] data)
        {
            string msg;
            Packets.ReadXmlMessage(data, out msg);

            OnXmlGGLiveMessageReceived(new XmlMessageEventArgs(msg));
        }
        /// <summary>
        /// Przetwórz pakiet w którym została przysłana wiadomość XML system.
        /// </summary>
        /// <param name="data">Dane</param>
        protected virtual void ProcessXmlSystemMessage(byte[] data)
        {
            string msg;
            Packets.ReadXmlMessage(data, out msg);

            OnXmlSystemMessageReceived(new XmlMessageEventArgs(msg));
        }
        /// <summary>
        /// Przetwórz pakiet w którym została przysłana lista kontaktów.
        /// </summary>
        /// <param name="data">Dane</param>
        protected virtual void ProcessUserListReply(byte[] data)
        {
            byte replyType;
            uint lastVer;
            ContactListType formatType;
            string reply;
            Packets.ReadUserListReply(data, out replyType, out lastVer, out formatType, out reply);
            if (replyType == Container.GG_USERLIST100_REPLY_LIST)
            {
                _lastContactListVersion = lastVer;
                OnContactListReceived(new ContactListEventArgs(ContactList.ImportFromString(reply, formatType), lastVer));
            }
            else if (replyType == 0x01)
            {
                _lastContactListVersion = lastVer;
            }
        }
        /// <summary>
        /// Przetwórz pakiet w którym została przysłana wersja listy kontaktów.
        /// </summary>
        /// <param name="data">Dane.</param>
        protected virtual void ProcessUserListVersion(byte[] data)
        {
            Packets.ReadUserListVersion(data, out _lastContactListVersion);
            OnContactListReceived(new ContactListEventArgs(null, _lastContactListVersion));
        }

        private void ParseAttributes(uint uin, byte[] attributes, out bool isImage, out uint[] conferenceMembers)
        {
            conferenceMembers = null;
            isImage = false;

            if (attributes != null || attributes.Length > 0)
            {
                int currentOffset = 0;
                if (currentOffset < attributes.Length)
                {
                    if (attributes[currentOffset] == Container.GG_IMAGE_RESPONSE_FLAG || attributes[currentOffset] == Container.GG_IMAGE_RESPONSE_FLAG_MORE_DATA)
                    {
                        byte currentFlag = attributes[currentOffset];
                        try
                        {
                            ImageData receivedImage = new ImageData();
                            //ReadImage(attrib, currentOffset, out currentOffset, out crc32, out len, out fileName, out imgData);
                            ReadImage(attributes, currentOffset, out currentOffset, out receivedImage.Crc32, out receivedImage.Length, out receivedImage.FileName, out receivedImage.Data);

                            if (currentFlag == Container.GG_IMAGE_RESPONSE_FLAG_MORE_DATA)
                            {
                                int index = _receivingImages.FindIndex(x => x.Crc32 == receivedImage.Crc32 && x.Length == receivedImage.Length);
                                ImageData imgStruct = _receivingImages[index];
                                Array.Resize(ref imgStruct.Data, imgStruct.Data.Length + receivedImage.Data.Length);
                                Buffer.BlockCopy(receivedImage.Data, 0, imgStruct.Data, imgStruct.Data.Length - receivedImage.Data.Length, receivedImage.Data.Length);
                                if (attributes.Length >= Container.GG_IMAGE_REPLY_MESSAGE_MAX_SIZE)
                                {
                                    _receivingImages[index] = imgStruct;
                                }
                                else
                                {
                                    _receivingImages.RemoveAt(index);
                                    OnImageReceived(new ImageEventArgs(imgStruct, uin));
                                }
                            }
                            else if (currentFlag == Container.GG_IMAGE_RESPONSE_FLAG)
                            {
                                if (attributes.Length >= Container.GG_IMAGE_REPLY_MESSAGE_MAX_SIZE)
                                {
                                    _receivingImages.Add(receivedImage);
                                }
                                else
                                {
                                    OnImageReceived(new ImageEventArgs(receivedImage, uin));
                                }
                            }
                        }
                        catch { }

                        isImage = true;
                    }
                    else if (attributes[currentOffset] == Container.GG_IMAGE_REQUEST_FLAG)
                    {
                        ImageData imgReq = new ImageData();
                        ReadImageRequest(attributes, currentOffset, out currentOffset, out imgReq.Crc32, out imgReq.Length);
                        OnImageRequest(new ImageEventArgs(imgReq, uin));

                        isImage = true;
                    }
                    else  if (attributes[currentOffset] == Container.GG_CONFERENCE_FLAG)
                    {
                        conferenceMembers = ReadConference(attributes, currentOffset, out currentOffset);
                    }
                    else if (attributes[currentOffset] == Container.GG_ATTRIBUTES_FLAG)
                    {
                        //TODO: text attributes parse
                    }
                }
            }

            if (conferenceMembers == null) conferenceMembers = new uint[0];
        }
        /// <summary>
        /// Odczytuje informacje o konferencji z atrybutów.
        /// </summary>
        /// <param name="attributes">Atrybuty</param>
        /// <param name="offset">Offset od którego rozpocząć czytanie</param>
        /// <param name="newOffset">Nowy offset</param>
        /// <returns>Uczestnicy konferencji</returns>
        protected uint[] ReadConference(byte[] attributes, int offset, out int newOffset)
        {
            uint[] conferenceMembers = null;
            uint count = 0;
            newOffset = offset;
            try
            {
                //for (int i = 0; i < attributes.Length; i++)
                //{
                    //if (attributes[i] == Container.GG_CONFERENCE_FLAG)
                    //{
                        using (PacketReader reader = new PacketReader(attributes))
                        {
                            reader.BaseStream.Position = offset + 1; // +1 because we bypass flag // i + 1;

                            count = reader.ReadUInt32(); //count of members
                            conferenceMembers = new uint[count];
                            for (uint j = 0; j < count; j++) conferenceMembers[j] = reader.ReadUInt32(); //read uin's
                            newOffset = (int)reader.BaseStream.Position;
                            reader.Close();
                        }
                    //    i += (int)(1 + count * 4);
                    //}
                //}
            }
            catch { }
            //if (conferenceMembers == null) conferenceMembers = new uint[0];
            return conferenceMembers;
        }
        /// <summary>
        /// Odczytuje informacje o obrazku z atrybutów.
        /// </summary>
        /// <param name="attributes">Atrybuty</param>
        /// <param name="offset">Offset</param>
        /// <param name="newOffset">Nowy offset</param>
        /// <param name="crc32">Suma kontrolna CRC32</param>
        /// <param name="length">Wielkość obrazka w bajtach</param>
        /// <param name="fileName">Nazwa pliku obrazka</param>
        /// <param name="imageData">Dane obrazka</param>
        protected void ReadImage(byte[] attributes, int offset, out int newOffset, out uint crc32, out uint length, out string fileName, out byte[] imageData)
        {
            newOffset = offset;
            crc32 = 0;
            length = 0;
            fileName = string.Empty;
            imageData = null;
            using (PacketReader reader = new PacketReader(attributes))
            {
                reader.BaseStream.Position = offset;
                byte flag = reader.ReadByte();
                length = reader.ReadUInt32();
                crc32 = reader.ReadUInt32();
                if (flag != Container.GG_IMAGE_RESPONSE_FLAG_MORE_DATA)
                {
                    int endOfFileName = -1;
                    for (int i = offset; i < attributes.Length; ++i)
                    {
                        if (attributes[i] == 0)
                        {
                            endOfFileName = i;
                            break;
                        }
                    }
                    byte[] fName = reader.ReadBytes(endOfFileName - offset);
                    fileName = Encoding.UTF8.GetString(fName);
                }
                imageData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
                newOffset = (int)reader.BaseStream.Position;
                reader.Close();
            }
        }
        /// <summary>
        /// Odczytaj informacje o zapytaniu o obrazek.
        /// </summary>
        /// <param name="attributes">Atrybuty</param>
        /// <param name="offset">Offset</param>
        /// <param name="newOffset">Nowy offset</param>
        /// <param name="crc32">Suma kontrolna CRC32</param>
        /// <param name="length">Wielkość obrazka w bajtach</param>
        protected void ReadImageRequest(byte[] attributes, int offset, out int newOffset, out uint crc32, out uint length)
        {
            newOffset = offset;
            crc32 = 0;
            length = 0;
            try
            {
                using (PacketReader reader = new PacketReader(attributes))
                {
                    reader.BaseStream.Position = offset + 1;
                    length = reader.ReadUInt32();
                    crc32 = reader.ReadUInt32();
                    newOffset = (int)reader.BaseStream.Position;
                    reader.Close();
                }
            }
            catch { }
        }
        #endregion

        #region Callback
        /// <summary>
        /// Metoda zwrotna łączenia.
        /// </summary>
        /// <param name="result">Rezultat</param>
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
            catch { OnConnectFailed(); CloseSocket(); }
        }
        /// <summary>
        /// Metoda zwrotna rozłączenia.
        /// </summary>
        /// <param name="result">Rezultat</param>
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
        /// <summary>
        /// Metoda zwrotna przysłanych danych.
        /// </summary>
        /// <param name="result">Rezultat</param>
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
                else throw new SocketException();
            }
            catch (SocketException) { CloseSocket(); OnDisconnected(); }
        }
        /// <summary>
        /// Metoda zwrotna wysyłania danych.
        /// </summary>
        /// <param name="result">Rezultat</param>
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
        /// <summary>Wywołuje zdarzenie ConnectFailed.</summary>
        protected virtual void OnConnectFailed()
        {
            RaiseEvent(ConnectFailed);
        }
        /// <summary>Wywołuje zdarzenie LoginFailed.</summary>
        protected virtual void OnLoginFailed()
        {
            RaiseEvent(LoginFailed);
        }
        /// <summary>Wywołuje zdarzenie ServerObtainingFailed.</summary>
        protected virtual void OnServerObtainingFailed()
        {
            RaiseEvent(ServerObtainingFailed);
        }
        /// <summary>Wywołuje zdarzenie Connected.</summary>
        protected virtual void OnConnected()
        {
            RaiseEvent(Connected);
        }
        /// <summary>Wywołuje zdarzenie Disconnected.</summary>
        protected virtual void OnDisconnected()
        {
            RaiseEvent(Disconnected);
        }
        /// <summary>Wywołuje zdarzenie Logged.</summary>
        protected virtual void OnLogged()
        {
            RaiseEvent(Logged);
        }
        /// <summary>Wywołuje zdarzenie ServerObtained.</summary>
        protected virtual void OnServerObtained()
        {
            RaiseEvent(ServerObtained);
        }
        /// <summary>Wywołuje zdarzenie StatusChanged.</summary>
        /// <param name="e">Parametry</param>
        protected virtual void OnStatusChanged(StatusEventArgs e)
        {
            RaiseEvent(StatusChanged, e);
        }
        /// <summary>Wywołuje zdarzenie MessageReceived.</summary>
        /// <param name="e">Parametry</param>
        protected virtual void OnMessageReceived(MessageEventArgs e)
        {
            RaiseEvent(MessageReceived, e);
        }
        /// <summary>Wywołuje zdarzenie OwnMessageReceived.</summary>
        /// <param name="e">Parametry</param>
        protected virtual void OnOwnMessageReceived(MessageEventArgs e)
        {
            RaiseEvent(OwnMessageReceived, e);
        }
        /// <summary>Wywołuje zdarzenie NoMailNotifyReceive.</summary>
        protected virtual void OnNoMailNotifyReceived()
        {
            RaiseEvent(NoMailNotifyReceived);
        }
        /// <summary>Wywołuje zdarzenie MultilogonNotifyReceived.</summary>
        /// <param name="e">Parametry</param>
        protected virtual void OnMultiloginNotifyReceived(MultiloginEventArgs e)
        {
            RaiseEvent(MultiloginNotifyReceived, e);
        }
        /// <summary>Wywołuje zdarzenie TypingNotifyReceived.</summary>
        /// <param name="e">Parametry</param>
        protected virtual void OnTypingNotifyReceived(TypingNotifyEventArgs e)
        {
            RaiseEvent(TypingNotifyReceived, e);
        }
        /// <summary>Wywołuje zdarzenie PublicDirectoryReplyReceived.</summary>
        /// <param name="e">Parametry</param>
        protected virtual void OnPublicDirectoryReplyReceived(PublicDirectoryReplyEventArgs e)
        {
            RaiseEvent(PublicDirectoryReplyReceived, e);
        }
        /// <summary>Wywołuje zdarzenie XmlGGLiveMessageReceived.</summary>
        /// <param name="e">Parametry</param>
        protected virtual void OnXmlGGLiveMessageReceived(XmlMessageEventArgs e)
        {
            RaiseEvent(XmlGGLiveMessageReceived, e);
        }
        /// <summary>Wywołuje zdarzenie XmlSystemMessageReceived.</summary>
        /// <param name="e">Parametry</param>
        protected virtual void OnXmlSystemMessageReceived(XmlMessageEventArgs e)
        {
            RaiseEvent(XmlSystemMessageReceived, e);
        }
        /// <summary>Wywołuje zdarzenie ContactListReceived.</summary>
        /// <param name="e">Parametry</param>
        protected virtual void OnContactListReceived(ContactListEventArgs e)
        {
            RaiseEvent(ContactListReceived, e);
        }
        /// <summary>Wywołuje zdarzenie ImageReceived.</summary>
        /// <param name="e">Parametry</param>
        protected virtual void OnImageReceived(ImageEventArgs e)
        {
            RaiseEvent(ImageReceived, e);
        }
        /// <summary>Wywołuje zdarzenie ImageRequest.</summary>
        /// <param name="e">Parametry</param>
        protected virtual void OnImageRequest(ImageEventArgs e)
        {
            RaiseEvent(ImageRequest, e);
        }

        /// <summary>
        /// Synchronizuje zdarzenia wywoływane na różnych wątkach.
        /// </summary>
        /// <param name="handler">Zdarzenie</param>
        protected void RaiseEvent(EventHandler handler)
        {
            RaiseEvent(handler, EventArgs.Empty);
        }
        /// <summary>
        /// Synchronizuje zdarzenia wywoływane na różnych wątkach.
        /// </summary>
        /// <param name="handler">Zdarzenie</param>
        /// <param name="e">Argumenty</param>
        protected void RaiseEvent(EventHandler handler, EventArgs e)
        {
            if (handler == null) return;
            if (_syncContext != null)
                _syncContext.Post(new SendOrPostCallback((state) => { handler(this, e); }), null);
            else handler(this, e);
        }
        /// <summary>
        /// Synchronizuje zdarzenia wywoływane na różnych wątkach.
        /// </summary>
        /// <typeparam name="T">Typ argumentów</typeparam>
        /// <param name="handler">Zdarzenie</param>
        /// <param name="e">Argumenty</param>
        protected void RaiseEvent<T>(EventHandler<T> handler, T e) where T : EventArgs
        {
            if (handler == null) return;
            if (_syncContext != null)
                _syncContext.Post(new SendOrPostCallback((state) => { handler(this, e); }), null);
            else handler(this, e);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Znisz obiekt klienta GG.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Znisz obiekt klienta GG.
        /// </summary>
        /// <param name="disposing">Niszczyć zasoby niezarządzane?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //managed
                    _pingTimer.Dispose();
                    _pingTimer = null;
                    CloseSocket();
                    _buffer = null;
                    _uin = 0;
                    _pass = null;
                    _status = GG4NET.Status.None;
                    _description = null;
                    _contactList = null;
                    _serverEp = null;
                    _socket = null;
                    _receiver = null;
                    _receivingImages = null;
                    _isLogged = false;  
                }

                //unmanaged

                _disposed = true;
            }
        }
        /// <summary>
        /// Destruktor.
        /// </summary>
        ~GaduGaduClient()
        {
            Dispose(false);
        }
        #endregion
        #endregion
    }
}
