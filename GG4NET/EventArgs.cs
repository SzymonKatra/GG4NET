using System;

namespace GG4NET
{
    /// <summary>
    /// Argumenty dla wiadomości.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        #region Properties
        private uint _uin;
        private DateTime _time;
        private string _message;
        private string _htmlMessage;
        private byte[] _attributes;
        private uint[] _conferenceParticipants;

        /// <summary>
        /// Nadawca wiadomości.
        /// </summary>
        public uint Uin
        {
            get
            {
                return _uin;
            }
            set
            {
                _uin = value;
            }
        }
        /// <summary>
        /// Czas nadania wiadomości.
        /// </summary>
        public DateTime Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
            }
        }
        /// <summary>
        /// Wiadomość zapisana czystym tekstem w ASCII.
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }
        /// <summary>
        /// Wiadomość zapisana w formacie HTML w UTF8.
        /// </summary>
        public string HtmlMessage
        {
            get
            {
                return _htmlMessage;
            }
            set
            {
                _htmlMessage = value;
            }
        }
        /// <summary>
        /// Atrybuty wiadomości zapisanej czystym tekstem.
        /// Informacje na temat atrybutów można uzyskać tutaj: http://toxygen.net/libgadu/protocol/#ch1.6 .
        /// </summary>
        public byte[] Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }
        /// <summary>
        /// Członkowie konferencji.
        /// Jeżeli ta wiadomość nie jest wiadomością konferencyjną, zawiera tylko numer nadawcy.
        /// </summary>
        public uint[] ConferenceParticipants
        {
            get { return _conferenceParticipants; }
            set { _conferenceParticipants = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Stwórz argumenty dla wiadomości.
        /// </summary>
        public MessageEventArgs()
            : this(0, DateTime.MinValue, string.Empty, string.Empty, null)
        {
        }
        /// <summary>
        /// Stwórz argumenty dla wiadomości.
        /// </summary>
        /// <param name="uin">Nadawca</param>
        /// <param name="time">Czas nadania</param>
        /// <param name="message">Wiadomość</param>
        public MessageEventArgs(uint uin, DateTime time, string message)
            : this(uin, time, message, message, null, null)
        {
        }
        /// <summary>
        /// Stwórz argumenty dla wiadomości.
        /// </summary>
        /// <param name="uin">Nadawca</param>
        /// <param name="time">Czas nadania</param>
        /// <param name="message">Wiadomość</param>
        /// <param name="htmlMessage">Wiadomość HTML</param>
        public MessageEventArgs(uint uin, DateTime time, string message, string htmlMessage)
            : this(uin, time, message, htmlMessage, null, null)
        {
        }
        /// <summary>
        /// Stwórz argumenty dla wiadomości.
        /// </summary>
        /// <param name="uin">Nadawca</param>
        /// <param name="time">Czas nadania</param>
        /// <param name="message">Wiadomość</param>
        /// <param name="htmlMessage">Wiadomość HTML</param>
        /// <param name="attributes">Atrybuty wiadomości</param>
        public MessageEventArgs(uint uin, DateTime time, string message, string htmlMessage, byte[] attributes)
            : this(uin, time, message, htmlMessage, attributes, null)
        {
        }
        /// <summary>
        /// Stwórz argumenty dla wiadomości.
        /// </summary>
        /// <param name="uin">Nadawca</param>
        /// <param name="time">Czas nadania</param>
        /// <param name="message">Wiadomość</param>
        /// <param name="htmlMessage">Wiadomość HTML</param>
        /// <param name="attributes">Atrybuty wiadomości</param>
        /// <param name="conferenceParticipants">Członkowie konferencji</param>
        public MessageEventArgs(uint uin, DateTime time, string message, string htmlMessage, byte[] attributes, uint[] conferenceParticipants)
        {
            _uin = uin;
            _time = time;
            _message = message;
            _htmlMessage = htmlMessage;
            _attributes = attributes;
            _conferenceParticipants = (conferenceParticipants == null ? new uint[] { uin } : conferenceParticipants);
        }
        #endregion
    }

    /// <summary>
    /// Argumenty dla multilogowania.
    /// </summary>
    public class MultiloginEventArgs : EventArgs
    {
        #region Properties
        private MultiloginInfo _info;

        /// <summary>
        /// Informacje o multilogowaniu.
        /// </summary>
        public MultiloginInfo Info
        {
            get { return _info; }
            set { _info = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Stwórz argumenty dla multilogowania.
        /// </summary>
        public MultiloginEventArgs()
        {
        }
        /// <summary>
        /// Stwórz argumenty dla multilogowania.
        /// </summary>
        /// <param name="info">Informacje o multilogowaniu</param>
        public MultiloginEventArgs(MultiloginInfo info)
        {
            _info = info;
        }
        #endregion
    }

    /// <summary>
    /// Argumenty dla odpowiedzi z publicznego katalogu GG.
    /// </summary>
    public class PublicDirectoryReplyEventArgs : EventArgs
    {
        #region Properties
        private PublicDirectoryReply[] _reply;
        private uint _nextStart = 0;

        /// <summary>
        /// Odpowiedź serwera GG.
        /// </summary>
        public PublicDirectoryReply[] Reply
        {
            get { return _reply; }
            set { _reply = value; }
        }
        /// <summary>
        /// Indeks od którego należy zacząć kolejne szukanie.
        /// </summary>
        public uint NextStart
        {
            get { return _nextStart; }
            set { _nextStart = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Stwórz argumenty dla odpowiedzi z publicznego katalogu GG.
        /// </summary>
        public PublicDirectoryReplyEventArgs()
        {
        }
        /// <summary>
        /// Stwórz argumenty dla odpowiedzi z publicznego katalogu GG.
        /// </summary>
        /// <param name="reply">Odpowiedź serwera GG</param>
        /// <param name="nextStart">Indeks od którego należy zacząć kolejne szukanie</param>
        public PublicDirectoryReplyEventArgs(PublicDirectoryReply[] reply, uint nextStart = 0)
        {
            _reply = reply;
            _nextStart = nextStart;
        }
        #endregion
    }

    /// <summary>
    /// Argumenty dla statusu.
    /// </summary>
    public class StatusEventArgs : EventArgs
    {
        #region Properties
        private UserInfo _contact;

        /// <summary>
        /// Informacje o kontakcie który zmienił status.
        /// </summary>
        public UserInfo Contact
        {
            get { return _contact; }
            set { _contact = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Stwórz argumenty dla statusu.
        /// </summary>
        public StatusEventArgs()
            : this(new UserInfo())
        {
        }
        /// <summary>
        /// Stwórz argumenty dla statusu.
        /// </summary>
        /// <param name="contact">Informacje o kontacie który zmienił status</param>
        public StatusEventArgs(UserInfo contact)
        {
            _contact = contact;
        }
        #endregion
    }

    /// <summary>
    /// Argumenty dla powiadomienia o pisaniu.
    /// </summary>
    public class TypingNotifyEventArgs : EventArgs
    {
        #region Properties
        private uint _uin = 0;
        private TypingNotifyType _type = TypingNotifyType.None;
        private ushort _length = 0;

        /// <summary>
        /// Numer GG.
        /// </summary>
        public uint Uin
        {
            get { return _uin; }
            set { _uin = value; }
        }
        /// <summary>
        /// Rodzaj powiadomienia.
        /// </summary>
        public TypingNotifyType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        /// <summary>
        /// Długość wpisanego tekstu.
        /// </summary>
        public ushort Length
        {
            get { return _length; }
            set { _length = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Stwórz argumenty dla powiadomienia o pisaniu.
        /// </summary>
        public TypingNotifyEventArgs()
            : this(0, TypingNotifyType.None, 0)
        {
        }
        /// <summary>
        /// Stwórz argumenty dla powiadomienia o pisaniu.
        /// </summary>
        /// <param name="uin">Numer GG</param>
        public TypingNotifyEventArgs(uint uin)
            : this(uin, TypingNotifyType.None, 0)
        {
        }
        /// <summary>
        /// Stwórz argumenty dla powiadomienia o pisaniu.
        /// </summary>
        /// <param name="uin">Numer GG</param>
        /// <param name="length">Długość wpisanego tekstu</param>
        public TypingNotifyEventArgs(uint uin, ushort length)
            : this(uin, (length >= 1 ? TypingNotifyType.Start : TypingNotifyType.Stop), length)
        {
        }
        /// <summary>
        /// Stwórz argumenty dla powiadomienia o pisaniu.
        /// </summary>
        /// <param name="uin">Numer GG</param>
        /// <param name="type">Typ powiadomienia</param>
        public TypingNotifyEventArgs(uint uin, TypingNotifyType type)
            : this(uin, type, 0)
        {
        }
        /// <summary>
        /// Stwórz argumenty dla powiadomienia o pisaniu.
        /// </summary>
        /// <param name="uin">Numer GG</param>
        /// <param name="type">Typ powiadomienia</param>
        /// <param name="length">Długość wpisanego tekstu</param>
        public TypingNotifyEventArgs(uint uin, TypingNotifyType type, ushort length)
        {
            _uin = uin;
            _type = type;
            _length = length;
        }
        #endregion
    }

    /// <summary>
    /// Argumenty dla wiadomości XML.
    /// </summary>
    public class XmlMessageEventArgs : EventArgs
    {
        private string _message = string.Empty;

        /// <summary>
        /// Wiadomość XML.
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        /// <summary>
        /// Stwórz argumenty dla wiadomości XML.
        /// </summary>
        public XmlMessageEventArgs()
        {
        }
        /// <summary>
        /// Stwórz argumenty dla wiadomości XML.
        /// </summary>
        /// <param name="message">Wiadomość XML</param>
        public XmlMessageEventArgs(string message)
        {
            _message = message;
        }
    }

    /// <summary>
    /// Argumenty dla listy kontaktów.
    /// </summary>
    public class ContactListEventArgs : EventArgs
    {
        private ContactList _contactList;
        private uint _version;

        /// <summary>
        /// Lista kontaktów.
        /// </summary>
        public ContactList ContactList
        {
            get { return _contactList; }
            set { _contactList = value; }
        }
        /// <summary>
        /// Wersja listy kontaktów
        /// </summary>
        public uint Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// Stwórz argumenty dla listy kontaktów.
        /// </summary>
        public ContactListEventArgs()
        {
        }
        /// <summary>
        /// Stwórz argumenty dla listy kontaktów.
        /// </summary>
        /// <param name="contactList">Lista kontaktów</param>
        /// <param name="version">Wersja listy kontaktów</param>
        public ContactListEventArgs(ContactList contactList, uint version = 0)
        {
            _contactList = contactList;
            _version = version;
        }
    }
}
